using PeNet;
using RansomGuard.API.Models;
using System.Security.Cryptography;

namespace RansomGuard.API.Services
{
    public interface IPEAnalysisService
    {
        Task<AnalysisResult> AnalyzeFileAsync(string filePath, string originalFilename, string fileHash);
    };
    public class PEAnalysisService : IPEAnalysisService
    {
        private readonly ILogger<PEAnalysisService> _logger;

        // Suspicious Windows APIs commonly used by ransomware
        private static readonly string[] SuspiciousAPIs =
        [
            "CryptEncrypt", "CryptDecrypt", "CryptAcquireContext",
            "BCryptEncrypt", "BCryptDecrypt", "BCryptGenRandom",
            "DeleteFile", "DeleteFileW", "DeleteFileA",
            "CreateProcess", "CreateProcessW", "CreateProcessA",
            "RegSetValue", "RegSetValueEx", "RegSetValueExW",
            "WriteFile", "WriteFileEx",
            "VirtualAlloc", "VirtualAllocEx",
            "CreateRemoteThread", "OpenProcess"
        ];

        public PEAnalysisService(ILogger<PEAnalysisService> logger)
        {
            _logger = logger;
        }

        public async Task<AnalysisResult> AnalyzeFileAsync(string filePath, string originalFilename, string fileHash)
        {
            _logger.LogInformation("Starting analysis: {Filename}", originalFilename);

            var peFile = new PeFile(filePath);

            // Extract features
            var entropy = CalculateEntropy(filePath);
            var sectionCount = peFile.ImageSectionHeaders?.Length ?? 0;
            var exportCount = peFile.ExportedFunctions?.Length ?? 0;
            var importCount = peFile.ImportedFunctions?.Length ?? 0;

            // Detect suspicious APIs
            var detectedAPIs = DetectSuspiciousAPIs(peFile);

            // Calculate risk score
            var riskScore = CalculateRiskScore(entropy, detectedAPIs.Count, sectionCount, exportCount);

            // Determine verdict
            var verdict = DetermineVerdict(riskScore);

            _logger.LogInformation(
                "Analysis complete: {Filename}, Risk: {Risk}, Verdict: {Verdict}, Entropy: {Entropy:F2}",
                originalFilename, riskScore, verdict, entropy);

            return new AnalysisResult
            {
                UploadId = Guid.NewGuid(),
                Filename = originalFilename,
                Timestamp = DateTime.UtcNow,
                RiskScore = riskScore,
                Entropy = entropy,
                SuspiciousAPIs = detectedAPIs,
                Verdict = verdict,
                FileHash = fileHash,
                SectionCount = sectionCount,
                ImportCount = importCount,
                ExportCount = exportCount,

            };
        }

        private static double CalculateEntropy(string filePath)
        {
            var fileBytes = File.ReadAllBytes(filePath);
            var frequencies = new int[256];

            // Count byte frequencies
            foreach (var b in fileBytes)
            {
                frequencies[b]++;
            }

            // Calculate Shannon entropy
            var fileLength = fileBytes.Length;
            return frequencies
                .Where(freq => freq > 0)
                .Select(freq => (double)freq / fileLength)
                .Sum(probability => -probability * Math.Log2(probability));
        }

        private static List<string> DetectSuspiciousAPIs(PeFile peFile)
        {
            if (peFile.ImportedFunctions == null)
            {
                return [];
            }

            return peFile.ImportedFunctions
                .Select(import => import.Name)
                .Where(functionName => !string.IsNullOrEmpty(functionName) &&
                    SuspiciousAPIs.Any(api => functionName.Contains(api, StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList()!; // Non-null assertion - filtered by Where clause
        }

        private static int CalculateRiskScore(double entropy, int suspiciousAPICount, int sectionCount, int exportCount)
        {
            int score = 0;

            // High entropy = encrypted/packez 
            if (entropy > 7.0)
            {
                score += 30;
            }
            else if (entropy > 6.5)
            {
                score += 15;
            }

            // Many suspicious APIs 
            score += suspiciousAPICount switch
            {
                > 5 => 40,
                > 2 => 20,
                > 0 => 10,
                _ => 0
            };

            // Unusual section count
            if (sectionCount is > 8 or < 2)
            {
                score += 20;
            }

            // Low export count = likely executable, not library
            if (exportCount < 5)
            {
                score += 10;
            }

            return Math.Min(score, 100); // Cap at 100
        }

        private static Verdict DetermineVerdict(int riskScore) => riskScore switch
        {
            >= 70 => Verdict.Ransomware,
            >= 35 => Verdict.Suspicious,
            _ => Verdict.Safe
        };
    }
}