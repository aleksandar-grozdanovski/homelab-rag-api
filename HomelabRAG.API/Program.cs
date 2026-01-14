using HomelabRAG.API.Data;
using HomelabRAG.API.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add database context
builder.Services.AddDbContext<RAGDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()
    )
);

// Configure LLM service based on provider setting
var llmProvider = builder.Configuration["LLMProvider"]?.ToLower() ?? "ollama";
var groqEnabled = builder.Configuration.GetValue<bool>("GroqSettings:Enabled");

if (groqEnabled || llmProvider == "groq")
{
    var apiKey = builder.Configuration["GroqSettings:ApiKey"] ?? "gsk-dummy-key";
    var apiUrl = builder.Configuration["GroqSettings:ApiUrl"];
    
    Console.WriteLine($"Configuring Groq LLM service");
    Console.WriteLine($"  API URL: {apiUrl}");
    Console.WriteLine($"  API Key: {(string.IsNullOrEmpty(apiKey) ? "NOT SET" : apiKey.Substring(0, Math.Min(10, apiKey.Length)) + "...")}");
    
    // Add Groq (OpenAI-compatible) service
    builder.Services.AddOpenAIService(settings =>
    {
        settings.ApiKey = apiKey;
        settings.BaseDomain = apiUrl;
        settings.Organization = string.Empty; // Not needed for Groq
    });
    builder.Services.AddScoped<ILLMService, GroqLLMService>();
}
else
{
    Console.WriteLine("Configuring Ollama LLM service");
    // Add Ollama service (default)
    builder.Services.AddScoped<ILLMService, OllamaService>();
}

builder.Services.AddScoped<DocumentService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.MapControllers();

// Health check endpoint
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
