using HomelabRAG.API.Data;
using HomelabRAG.API.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;

namespace HomelabRAG.API.Services;

public class DocumentService
{
    private readonly RAGDbContext _context;
    private readonly ILLMService _llmService;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _allowedBaseDirectory;

    public DocumentService(RAGDbContext context, ILLMService llmService, ILogger<DocumentService> logger, IConfiguration configuration)
    {
        _context = context;
        _llmService = llmService;
        _logger = logger;
        var configuredBase = configuration["DocumentIngestion:AllowedBaseDirectory"];
        if (string.IsNullOrWhiteSpace(configuredBase))
            throw new InvalidOperationException("DocumentIngestion:AllowedBaseDirectory must be configured.");
        _allowedBaseDirectory = ResolveExistingPath(configuredBase);
    }

    public async Task<Document> IngestDocumentAsync(string filePath)
    {
        filePath = GetAllowedFilePath(filePath);

        var content = await File.ReadAllTextAsync(filePath);
        var fileName = Path.GetFileName(filePath);

        // Check if document already exists
        var existing = await _context.Documents
            .FirstOrDefaultAsync(d => d.FileName == fileName);

        if (existing != null)
        {
            _logger.LogInformation("Document {FileName} already exists, skipping", fileName);
            return existing;
        }

        var document = new Document
        {
            FileName = fileName,
            FilePath = filePath,
            Content = content
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Create chunks
        var chunks = ChunkText(content);
        _logger.LogInformation("Created {ChunkCount} chunks for {FileName}", chunks.Count, fileName);

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunkText = chunks[i];
            _logger.LogInformation("Generating embedding for chunk {Index}/{Total}", i + 1, chunks.Count);
            
            var embedding = await _llmService.GenerateEmbeddingAsync(chunkText);
            
            var chunk = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunkText,
                ChunkIndex = i,
                Embedding = new Vector(embedding)
            };

            _context.DocumentChunks.Add(chunk);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully ingested {FileName} with {ChunkCount} chunks", fileName, chunks.Count);

        return document;
    }

    public string GetAllowedDirectoryPath(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException("Directory not found.");
        var resolved = ResolveExistingPath(directoryPath);
        EnsureContained(resolved);
        return resolved;
    }

    public IReadOnlyList<string> GetAllowedMarkdownFiles(string directoryPath)
    {
        var root = GetAllowedDirectoryPath(directoryPath);
        var results = new List<string>();
        var pending = new Stack<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        pending.Push(root);

        while (pending.TryPop(out var directory))
        {
            var resolvedDirectory = ResolveExistingPath(directory);
            EnsureContained(resolvedDirectory);
            if (!visited.Add(resolvedDirectory))
                continue;

            foreach (var file in Directory.EnumerateFiles(resolvedDirectory, "*.md"))
                results.Add(GetAllowedFilePath(file));
            foreach (var child in Directory.EnumerateDirectories(resolvedDirectory))
                pending.Push(child);
        }

        return results;
    }

    private string GetAllowedFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.");
        var resolved = ResolveExistingPath(filePath);
        EnsureContained(resolved);
        return resolved;
    }

    private void EnsureContained(string path)
    {
        var relative = Path.GetRelativePath(_allowedBaseDirectory, path);
        if (relative == ".." || relative.StartsWith($"..{Path.DirectorySeparatorChar}") || Path.IsPathRooted(relative))
            throw new UnauthorizedAccessException("The requested path is outside the allowed document directory.");
    }

    private static string ResolveExistingPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath)!;
        var current = root;
        foreach (var component in fullPath[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
            current = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? info.FullName;
        }
        return Path.GetFullPath(current);
    }

    public async Task<List<DocumentChunk>> FindSimilarChunksAsync(string query, int topK = 5)
    {
        var queryEmbedding = await _llmService.GenerateEmbeddingAsync(query);
        var queryVector = new Vector(queryEmbedding);

        var chunks = await _context.DocumentChunks
            .Include(c => c.Document)
            .Where(c => c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(topK)
            .ToListAsync();

        return chunks;
    }

    private List<string> ChunkText(string text, int maxChunkSize = 1000)
    {
        var chunks = new List<string>();
        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (currentChunk.Length + paragraph.Length > maxChunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            currentChunk.AppendLine(paragraph);
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        return await _context.Documents
            .Include(d => d.Chunks)
            .ToListAsync();
    }
}
