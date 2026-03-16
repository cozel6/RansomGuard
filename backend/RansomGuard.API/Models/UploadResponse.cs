namespace RansomGuard.API.Models
{
    public class UploadResponse
    {
        public Guid UploadId { get; set; }

        public string Message { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public Verdict Verdict { get; set; }
    }
}