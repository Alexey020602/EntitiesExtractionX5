using Microsoft.ML;
namespace Api;

public class OnnxModelConfigurator
{
    private readonly MLContext mlContext;
    private readonly ITransformer mlModel;

    public OnnxModelConfigurator(IOnnxModel onnxModel)
    {
        mlContext = new MLContext();
        // Model creation and pipeline definition for images needs to run just once,
        // so calling it from the constructor:
        mlModel = SetupMlNetModel(onnxModel);
    }

    private ITransformer SetupMlNetModel(IOnnxModel onnxModel)
    {
        var dataView = mlContext.Data.LoadFromEnumerable(new List<ModelInput>());

        var pipeline = mlContext.Transforms.ApplyOnnxModel(
            modelFile: onnxModel.ModelPath, 
            outputColumnName: onnxModel.ModelOutput, 
            inputColumnName: onnxModel.ModelInput);

        var mlNetModel = pipeline.Fit(dataView);

        return mlNetModel;
    }

    public PredictionEngine<ModelInput, T> GetMlNetPredictionEngine<T>()
        where T : class, IOnnxObjectPrediction, new()
    {
        return mlContext.Model.CreatePredictionEngine<ModelInput, T>(mlModel);
    }

    public void SaveMLNetModel(string mlnetModelFilePath)
    {
        // Save/persist the model to a .ZIP file to be loaded by the PredictionEnginePool
        mlContext.Model.Save(mlModel, null, mlnetModelFilePath);
    }
}

public interface IOnnxModel
{
    string ModelPath { get; }

    // To check Model input and output parameter names, you can
    // use tools like Netron: https://github.com/lutzroeder/netron
    string ModelInput { get; }
    string ModelOutput { get; }
}

public interface IOnnxObjectPrediction
{
    float[] PredictedLabels { get; set; }
}