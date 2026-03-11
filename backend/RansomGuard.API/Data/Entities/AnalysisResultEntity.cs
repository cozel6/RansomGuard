using System.Globalization;

namespace RansomGuard.API.Data.Entities
{
    public class AnalysisResultEntity
    {
        // Prmary key
        public Guid Id { get; set; }

        // File metadada
        public string Filename { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty; // SHA256
        public long FileSize { get; set; }

        // Analysis result
        public DateTime Timestamp { get; set; }
        public int RiskScore { get; set; }
        public double Entropy { get; set; }
        public string SuspiciousAPIs { get; set; } = string.Empty; // json array
        public string Verdict { get; set; } = string.Empty; // Safe, Suspicious, Ransomware

        // PE files details
        public int SectionCount { get; set; }
        public int ImportCount { get; set; }
        public int ExportCount { get; set; }
    }
}