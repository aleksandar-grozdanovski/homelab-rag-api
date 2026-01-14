namespace HomelabRAG.API.Services;

public interface ILLMService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<string> GenerateResponseAsync(string prompt, List<string> context);
}
