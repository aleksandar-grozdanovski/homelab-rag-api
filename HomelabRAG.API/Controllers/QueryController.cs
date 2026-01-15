using HomelabRAG.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomelabRAG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly OllamaService? _ollamaService;
    private readonly GroqLLMService? _groqService;
    private readonly ILogger<QueryController> _logger;
    private readonly IConfiguration _configuration;

    public QueryController(
        DocumentService documentService,
        ILogger<QueryController> logger,
        IConfiguration configuration,
        OllamaService? ollamaService = null,
        GroqLLMService? groqService = null)
    {
        _documentService = documentService;
        _ollamaService = ollamaService;
        _groqService = groqService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        try
        {
            // Select LLM service based on request or default configuration
            var requestedProvider = request.Provider?.ToLower() ?? _configuration["LLMProvider"]?.ToLower() ?? "groq";
            
            ILLMService? llmService = requestedProvider switch
            {
                "groq" => _groqService,
                "ollama" => _ollamaService,
                _ => _groqService ?? _ollamaService
            };

            if (llmService == null)
            {
                return StatusCode(500, new { error = $"LLM provider '{requestedProvider}' is not available. Please configure the service." });
            }

            _logger.LogInformation("Processing query with {Provider} provider: {Question}", requestedProvider, request.Question);

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
                    sources = new List<object>(),
                    provider = requestedProvider
                });
            }

            // Get context from chunks
            var context = similarChunks.Select(c => c.Content).ToList();

            // Generate response using LLM
            var answer = await llmService.GenerateResponseAsync(request.Question, context);

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
                chunksUsed = similarChunks.Count,
                provider = requestedProvider
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Question}", request.Question);
            return StatusCode(500, new { error = "Failed to process query", details = ex.Message });
        }
    }
}

public record QueryRequest(string Question, int? TopK, string? Provider);
