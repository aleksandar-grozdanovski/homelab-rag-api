using System.Text;
using System.Text.Json;
using Pgvector;

namespace HomelabRAG.API.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(IConfiguration configuration, ILogger<OllamaService> logger)
    {
        _httpClient = new HttpClient();
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://192.168.50.10:11434";
        _model = configuration["Ollama:Model"] ?? "llama3.2";
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            model = _model,
            input = text
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/embed", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            
            var embeddingsArray = jsonDoc.RootElement.GetProperty("embeddings")[0];
            var embedding = new List<float>();
            
            foreach (var element in embeddingsArray.EnumerateArray())
            {
                embedding.Add((float)element.GetDouble());
            }

            return embedding.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text.Substring(0, Math.Min(50, text.Length)));
            throw;
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, List<string> context)
    {
        var contextText = string.Join("\n\n", context);
        var fullPrompt = $@"Context from documentation:
{contextText}

Question: {prompt}

Answer based on the context above. If the answer is not in the context, say so.";

        var request = new
        {
            model = _model,
            prompt = fullPrompt,
            stream = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            
            return jsonDoc.RootElement.GetProperty("response").GetString() ?? "No response generated";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response for prompt: {Prompt}", prompt);
            throw;
        }
    }
}
