namespace RansomGuard.API.Services;

public record MlPrediction(
    string Prediction,
    double Confidence,
    string ModelVersion,
    double RawScore);

public interface IMlServiceClient
{
    Task<MlPrediction?> PredictAsync(string filePath, string originalFilename);
}
