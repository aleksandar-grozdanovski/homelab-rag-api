using HomelabRAG.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomelabRAG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly OllamaService _ollamaService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(
        DocumentService documentService,
        OllamaService ollamaService,
        ILogger<QueryController> logger)
    {
        _documentService = documentService;
        _ollamaService = ollamaService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        try
        {
            _logger.LogInformation("Received query: {Question}", request.Question);

            // Find similar chunks
            var similarChunks = await _documentService.FindSimilarChunksAsync(
                request.Question,
                request.TopK ?? 5
            );

            if (similarChunks.Count == 0)
            {
                return Ok(new
                {
                    answer = "I don't have any relevant information in the documentation to answer this question.",
                    sources = new List<object>()
                });
            }

            // Get context from chunks
            var context = similarChunks.Select(c => c.Content).ToList();

            // Generate response using Ollama
            var answer = await _ollamaService.GenerateResponseAsync(request.Question, context);

            var sources = similarChunks.Select(c => new
            {
                fileName = c.Document?.FileName,
                chunkIndex = c.ChunkIndex,
                preview = c.Content.Length > 200 
                    ? c.Content.Substring(0, 200) + "..." 
                    : c.Content
            }).ToList();

            return Ok(new
            {
                question = request.Question,
                answer,
                sources,
                chunksUsed = similarChunks.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Question}", request.Question);
            return StatusCode(500, new { error = "Failed to process query", details = ex.Message });
        }
    }
}

public record QueryRequest(string Question, int? TopK);
