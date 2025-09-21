using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;

namespace Api;

public interface IPredictor
{
    Task<PredictionResponse> Predict(string input, CancellationToken cancellationToken);
}

internal sealed class DefaultPredictor(
    ITypoCorrectionService typoCorrectionService,
    INerService nerService,
    HybridCache cache) : IPredictor
{
    public async Task<PredictionResponse> Predict(string input, CancellationToken cancellationToken)
    {
        var jsonResponse = await cache.GetOrCreateAsync(
            input,
            input,
            ProcessInput, 
            cancellationToken: cancellationToken);

        return DeserializeResponse(jsonResponse);
    }

    private static PredictionResponse DeserializeResponse(string jsonResponse)
    {
        return JsonSerializer.Deserialize<PredictionResponse>(jsonResponse) ?? throw new InvalidOperationException("Predictor returned null");
    }

    private async ValueTask<string> ProcessInput(string input, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            typoCorrectionService.Correct(input),
            typoCorrectionService.Correct(input),
            ProcessCorrectedInput, 
            cancellationToken: cancellationToken);
    }

    private ValueTask<string> ProcessCorrectedInput(string correctedInput, CancellationToken cancellationToken)
    {
        var response = nerService.Process(correctedInput);
        return ValueTask.FromResult(JsonSerializer.Serialize(response));
    }
}