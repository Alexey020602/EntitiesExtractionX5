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

app.MapPost("/api/predict",
    async (PredictionRequest request, HybridCache cache, CancellationToken cancellationToken) =>
    {
        var value = await cache.GetOrCreateAsync(request.Input, GetResponse, cancellationToken: cancellationToken);
        return PostProcessResponse(value);
    });

await app.RunAsync();
return;

PredictionResponse PostProcessResponse(string s) => s == "Response"
    ?
    [
        new(0, 8, "B-TYPE"),
        new(9, 15, "I-TYPE")
    ]
    : [];

static async ValueTask<string> GetResponse(CancellationToken cancellationToken)
{
    await Task.Delay(500, cancellationToken);
    return "Response";
}

sealed record PredictionRequest(string Input);

sealed record PredictionItem(int StartIndex, int EndIndex, string Entity);