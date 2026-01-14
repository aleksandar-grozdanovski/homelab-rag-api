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
    private readonly OllamaService _ollamaService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(RAGDbContext context, OllamaService ollamaService, ILogger<DocumentService> logger)
    {
        _context = context;
        _ollamaService = ollamaService;
        _logger = logger;
    }

    public async Task<Document> IngestDocumentAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

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
            
            var embedding = await _ollamaService.GenerateEmbeddingAsync(chunkText);
            
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

    public async Task<List<DocumentChunk>> FindSimilarChunksAsync(string query, int topK = 5)
    {
        var queryEmbedding = await _ollamaService.GenerateEmbeddingAsync(query);
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
