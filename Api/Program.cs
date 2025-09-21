using System.Numerics;
using System.Text;
using System.Text.Json;
using Api;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var modelPath = Path.Combine(builder.Environment.WebRootPath, "seq2seq_model.onnx");
var mlModelPath = Path.Combine(builder.Environment.WebRootPath, "smyShitModel.zip");
var vocabPath = Path.Combine(builder.Environment.WebRootPath, "vocab.json");
var onnxModelConfigurator = new OnnxModelConfigurator(new MyShitModel()
{
    ModelPath = modelPath,
    ModelInput = "input",
    ModelOutput = "output",
});

onnxModelConfigurator.SaveMLNetModel(mlModelPath);

builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
    .FromFile(mlModelPath);
    

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

app.MapGet("/api/test", (string query, PredictionEnginePool<ModelInput, ModelOutput> pool) =>
{
    var result = pool.Predict(new ModelInput() { Input = query.Select(c => (long)c).ToArray() });
    return result;
});

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

public class ModelInput
{
    [ColumnName("input")]
    public long[] Input { get; set; } = [];
}

public class ModelOutput
{
    [ColumnName("output")] 
    public VBuffer<float> Output { get; set; } = default;
}