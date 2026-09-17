using HomelabRAG.API.Data;
using HomelabRAG.API.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI.Extensions;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ApiSecurity:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 32)
    throw new InvalidOperationException("ApiSecurity:ApiKey must be set to a random value of at least 32 characters.");

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

// Configure LLM services - register both for runtime selection
var groqApiKey = builder.Configuration["GroqSettings:ApiKey"] ?? "";
var groqApiUrl = builder.Configuration["GroqSettings:ApiUrl"];
var groqBaseDomain = string.IsNullOrWhiteSpace(groqApiUrl)
    ? "https://api.groq.com/openai/v1"
    : groqApiUrl;

Console.WriteLine($"Configuring LLM services:");

// Always register Ollama (required for embeddings)
Console.WriteLine("  ✓ Ollama service registered");
builder.Services.AddScoped<OllamaService>();
// Register as ILLMService for DocumentService (embeddings)
builder.Services.AddScoped<ILLMService>(sp => sp.GetRequiredService<OllamaService>());

// Register Groq if API key is available
if (!string.IsNullOrWhiteSpace(groqApiKey) && groqApiKey != "gsk-dummy-key")
{
    Console.WriteLine($"  ✓ Groq service registered");
    Console.WriteLine($"    API URL: {groqBaseDomain}");
    
    builder.Services.AddOpenAIService(settings =>
    {
        settings.ApiKey = groqApiKey;
        settings.BaseDomain = groqBaseDomain;
        settings.Organization = string.Empty;
    });
    builder.Services.AddScoped<GroqLLMService>();
}
else
{
    Console.WriteLine("  ⚠ Groq API key not configured - Groq provider will not be available");
}

// Set default provider based on configuration
var defaultProvider = builder.Configuration["LLMProvider"]?.ToLower() ?? "groq";
Console.WriteLine($"  Default provider: {defaultProvider}");

builder.Services.AddScoped<DocumentService>();

// Only explicitly trusted browser origins may call the API.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustedOrigins", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RAGDbContext>();
    try
    {
        Console.WriteLine("Ensuring database is created...");
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating database: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("TrustedOrigins");
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var suppliedKey = context.Request.Headers["X-API-Key"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(apiKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        if (expectedBytes.Length != suppliedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }
    }
    await next();
});
app.MapControllers();

// Health check endpoint
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
