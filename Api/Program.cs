using System.Text.Json;
using CSnakes.Runtime;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddLogging();
// builder.Logging.AddFilter("CSnakes", LogLevel.Debug);
builder.Services.AddOpenApi();

var home = Path.Join(builder.Environment.ContentRootPath, "python");
var venv = Path.Join(home, "venv");
var modelPath = Path.Join(builder.Environment.WebRootPath, "custom_ru_core_news_lg_with_9_labels_50_epochs");
builder.Services.WithPython()
    .WithHome(home)
    .WithVirtualEnvironment(venv)
    .WithPipInstaller()
    .FromRedistributable("3.12")
    // .FromFolder("/home/app/.config/CSnakes", "3.12")
    ;

builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().Model());

builder.AddServiceDefaults();

builder.Services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions()
{
    DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
});


builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Python home path {Home}", home);
    logger.LogInformation("Current Directory: {CurrentDirectory}", Environment.CurrentDirectory);
    logger.LogInformation("wwwroot path {WwwRoot}", app.Environment.WebRootPath);
    logger.LogInformation("current root path {CurrentRoot}", app.Environment.ContentRootPath);
    logger.LogInformation("venv path {Venv}", venv);
    logger.LogInformation("model path {ModelPath}", modelPath);
    scope.ServiceProvider.GetRequiredService<IModel>().Initialize(modelPath);
} 

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapPost("/api/predict", 
    (PredictionRequest request, IModel module) =>
    {
        var result = module.Predict(request.Input);
        return result.Select(tuple => new PredictionItem(tuple.Item1, tuple.Item2, tuple.Item3));
    });

await app.RunAsync();
return;