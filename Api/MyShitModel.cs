using Api;

public sealed record MyShitModel : IOnnxModel
{
    public required string ModelPath { get; init; }
    public required string ModelInput { get; init; }
    public required string ModelOutput { get; init; }
}