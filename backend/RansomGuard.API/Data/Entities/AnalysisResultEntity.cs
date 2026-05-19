namespace RansomGuard.API.Data.Entities;

public class AnalysisResultEntity
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime Timestamp { get; set; }
    public int RiskScore { get; set; }
    public double Entropy { get; set; }
    public string SuspiciousAPIs { get; set; } = string.Empty;
    public string Verdict { get; set; } = string.Empty;
    public int SectionCount { get; set; }
    public int ImportCount { get; set; }
    public int ExportCount { get; set; }
    public double? MlConfidence { get; set; }
    public string? MlModelVersion { get; set; }
}
