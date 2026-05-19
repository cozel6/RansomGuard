using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RansomGuard.API.Services;

public class MlServiceClient : IMlServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MlServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public MlServiceClient(HttpClient httpClient, ILogger<MlServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MlPrediction?> PredictAsync(string filePath, string originalFilename)
    {
        try
        {
            _logger.LogInformation("Sending file to ML service: {Filename}", originalFilename);

            using var fileStream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", originalFilename);

            var response = await _httpClient.PostAsync("/predict", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ML Service returned {StatusCode}: {Body}", response.StatusCode, errorBody);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MlPredictionDto>(json, JsonOptions);

            if (result == null)
            {
                _logger.LogWarning("ML Service returned null response");
                return null;
            }

            _logger.LogInformation(
                "ML prediction: {Prediction} (confidence: {Confidence:F3})",
                result.Prediction, result.Confidence);

            return new MlPrediction(result.Prediction, result.Confidence, result.ModelVersion, result.RawScore);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("ML service unavailable: {Message}", ex.Message);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning("ML service request timed out: {Message}", ex.Message);
            return null;
        }
    }

    private sealed class MlPredictionDto
    {
        [JsonPropertyName("prediction")]
        public string Prediction { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = string.Empty;

        [JsonPropertyName("raw_score")]
        public double RawScore { get; set; }
    }
}
