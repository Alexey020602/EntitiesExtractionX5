namespace Api;

public interface INerService
{
    Task<PredictionResponse> Process(string query);
}