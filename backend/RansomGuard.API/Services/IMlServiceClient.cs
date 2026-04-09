namespace RansomGuard.API.Services
{
    //Response from the Python ML service POST /predict
    public record MlPrediction(

        string Prediction,
        double Confidence,
        string ModelVersion,
        double RawScore
    );

    public interface IMlServiceClient
    {
        Task<MlPrediction?> PredictAsync(string filePath, string originalFilename);
    }
}