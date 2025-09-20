using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMemoryCache();
// builder.AddRedisClient("cache");
builder.AddRedisDistributedCache("cache");
builder.Services.AddHybridCache();

builder.Services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions()
{
    DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/predict", PredictionResponse (PredictionRequest request, HybridCache cache, CancellationToken cancellationToken) =>
{
    cache.GetOrCreateAsync("", (_) => ValueTask.FromResult(""));
    return cache.GetOrCreateAsync(
        key: request.Input,
        factory:(ctx) => new ValueTask( GetResponse(request.Input, cancellationToken),
            options: null,
            ta
    );
});

PredictionResponse PostProcessResponse(string s)
{
    
}
static async Task<PredictionResponse> GetResponse(string input, CancellationToken cancellationToken)
{
    await Task.Delay(500);
    return
    [
        new(0, 8, "B-TYPE"),
        new(9, 15, "I-TYPE")
    ];
}

await app.RunAsync();
sealed record PredictionRequest(string Input);
sealed record PredictionItem(int StartIndex, int EndIndex, string Entity);