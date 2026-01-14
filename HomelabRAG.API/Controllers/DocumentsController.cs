using HomelabRAG.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomelabRAG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(DocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> IngestDocument([FromBody] IngestRequest request)
    {
        try
        {
            var document = await _documentService.IngestDocumentAsync(request.FilePath);
            return Ok(new
            {
                message = "Document ingested successfully",
                documentId = document.Id,
                fileName = document.FileName,
                chunkCount = document.Chunks.Count
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting document: {FilePath}", request.FilePath);
            return StatusCode(500, new { error = "Failed to ingest document", details = ex.Message });
        }
    }

    [HttpPost("ingest-directory")]
    public async Task<IActionResult> IngestDirectory([FromBody] IngestDirectoryRequest request)
    {
        if (!Directory.Exists(request.DirectoryPath))
        {
            return NotFound(new { error = $"Directory not found: {request.DirectoryPath}" });
        }

        var markdownFiles = Directory.GetFiles(request.DirectoryPath, "*.md", SearchOption.AllDirectories);
        var results = new List<object>();

        foreach (var file in markdownFiles)
        {
            try
            {
                var document = await _documentService.IngestDocumentAsync(file);
                results.Add(new
                {
                    fileName = document.FileName,
                    status = "success",
                    chunkCount = document.Chunks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting {File}", file);
                results.Add(new
                {
                    fileName = Path.GetFileName(file),
                    status = "failed",
                    error = ex.Message
                });
            }
        }

        return Ok(new
        {
            message = $"Processed {markdownFiles.Length} files",
            results
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDocuments()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        return Ok(documents.Select(d => new
        {
            d.Id,
            d.FileName,
            d.FilePath,
            d.IngestedAt,
            chunkCount = d.Chunks.Count
        }));
    }
}

public record IngestRequest(string FilePath);
public record IngestDirectoryRequest(string DirectoryPath);
