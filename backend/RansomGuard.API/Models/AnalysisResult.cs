namespace RansomGuard.API.Models
{
    public class AnalysisResult
    {
        public Guid UploadId { get; set; }
        public string Filename { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
        public int RiskScore { get; set; }
        public double Entropy { get; set; }

        public int SectionCount { get; set; }
        public int ImportCount { get; set; }
        public int ExportCount { get; set; }

        public List<string> SuspiciousAPIs { get; set; } = [];
        public Verdict Verdict { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }
}