using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace HomelabRAG.API.Services;

public class GroqLLMService : ILLMService
{
    private readonly IOpenAIService _openAIService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqLLMService> _logger;

    public GroqLLMService(IOpenAIService openAIService, IConfiguration configuration, ILogger<GroqLLMService> logger)
    {
        _openAIService = openAIService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // Groq doesn't have embeddings yet, so we'll use a simple hash-based approach
        // or fallback to Ollama for embeddings
        // For now, throw to indicate this limitation
        var ollamaUrl = _configuration["Ollama:BaseUrl"] ?? "http://192.168.50.10:11434";
        
        _logger.LogInformation($"Using Ollama for embeddings (Groq doesn't support embeddings yet): {ollamaUrl}");
        
        using var httpClient = new HttpClient();
        var requestBody = new
        {
            model = "llama3.2",
            input = text
        };

        var response = await httpClient.PostAsJsonAsync($"{ollamaUrl}/api/embed", requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        
        if (result?.Embeddings == null || result.Embeddings.Length == 0)
        {
            throw new Exception("No embedding returned from Ollama");
        }

        return result.Embeddings[0];
    }

    public async Task<string> GenerateResponseAsync(string prompt, List<string> context)
    {
        var model = _configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";
        var contextText = string.Join("\n\n", context);
        
        _logger.LogInformation($"Generating response with Groq model: {model}");

        var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(@"You are a helpful assistant that answers questions based on the provided context from homelab documentation. 
If the context doesn't contain relevant information, say so politely. Always cite which parts of the context you used."),
                ChatMessage.FromUser($@"Context from documentation:
{contextText}

Question: {prompt}

Please provide a helpful answer based on the context above.")
            },
            Model = model,
            MaxTokens = 1000,
            Temperature = 0.7f
        });

        if (completionResult.Successful)
        {
            return completionResult.Choices.First().Message.Content ?? "No response generated.";
        }
        else
        {
            throw new Exception($"Groq API error: {completionResult.Error?.Message}");
        }
    }

    private class OllamaEmbeddingResponse
    {
        public float[][] Embeddings { get; set; } = Array.Empty<float[]>();
    }
}
