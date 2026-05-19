using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RansomGuard.API.Models;
using RansomGuard.API.Services;

namespace RansomGuard.API.Tests.Unit;

public class PEAnalysisServiceTests : IDisposable
{
    private readonly PEAnalysisService _service;
    private readonly string _tempDir;

    public PEAnalysisServiceTests()
    {
        _service = new PEAnalysisService(NullLogger<PEAnalysisService>.Instance);
        _tempDir = Path.Combine(Path.GetTempPath(), $"ransomguard_tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string CreatePEFile(byte[] content, string name = "test.dll")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    private static byte[] CreateMinimalPEFile()
    {
        var bytes = new byte[0x200];
        bytes[0x00] = 0x4D; bytes[0x01] = 0x5A; // MZ
        bytes[0x3C] = 0x40; // e_lfanew
        bytes[0x40] = 0x50; bytes[0x41] = 0x45; // PE
        bytes[0x44] = 0x4C; bytes[0x45] = 0x01; // Machine: I386
        bytes[0x54] = 0xE0; bytes[0x55] = 0x00; // SizeOfOptionalHeader
        bytes[0x56] = 0x02; bytes[0x57] = 0x00; // Characteristics
        bytes[0x58] = 0x0B; bytes[0x59] = 0x01; // Magic: PE32
        return bytes;
    }

    [Fact]
    public async Task AnalyzeFileAsync_MinimalPE_ReturnsValidResult()
    {
        var path = CreatePEFile(CreateMinimalPEFile());

        var result = await _service.AnalyzeFileAsync(path, "test.dll", "testhash");

        result.Should().NotBeNull();
        result.Filename.Should().Be("test.dll");
        result.FileHash.Should().Be("testhash");
        result.UploadId.Should().NotBe(Guid.Empty);
        result.SuspiciousAPIs.Should().NotBeNull();
        result.Entropy.Should().BeGreaterThanOrEqualTo(0);
        result.RiskScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task AnalyzeFileAsync_VerdictConsistentWithRiskScore()
    {
        var path = CreatePEFile(CreateMinimalPEFile());

        var result = await _service.AnalyzeFileAsync(path, "test.dll", "hash");

        var expectedVerdict = result.RiskScore switch
        {
            >= 70 => Verdict.Ransomware,
            >= 35 => Verdict.Suspicious,
            _ => Verdict.Safe
        };
        result.Verdict.Should().Be(expectedVerdict);
    }

    [Fact]
    public async Task AnalyzeFileAsync_TimestampIsRecent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var path = CreatePEFile(CreateMinimalPEFile());

        var result = await _service.AnalyzeFileAsync(path, "test.dll", "hash");

        result.Timestamp.Should().BeAfter(before).And.BeBefore(DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task AnalyzeFileAsync_HighEntropyData_HasHigherEntropy()
    {
        var peBytes = CreateMinimalPEFile();
        var random = new Random(42);
        var randomData = new byte[4096];
        random.NextBytes(randomData);
        var highEntropyPE = peBytes.Concat(randomData).ToArray();

        var highEntropyPath = CreatePEFile(highEntropyPE, "high_entropy.dll");
        var normalPath = CreatePEFile(CreateMinimalPEFile(), "normal.dll");

        var highEntropyResult = await _service.AnalyzeFileAsync(highEntropyPath, "high_entropy.dll", "hash1");
        var normalResult = await _service.AnalyzeFileAsync(normalPath, "normal.dll", "hash2");

        highEntropyResult.Entropy.Should().BeGreaterThan(normalResult.Entropy);
    }

    [Fact]
    public async Task AnalyzeFileAsync_SuspiciousAPIs_IsNonNullList()
    {
        var path = CreatePEFile(CreateMinimalPEFile());

        var result = await _service.AnalyzeFileAsync(path, "test.dll", "hash");

        // Minimal PE has no imports so the list is empty — that is valid
        result.SuspiciousAPIs.Should().NotBeNull();
    }

    [Fact]
    public async Task AnalyzeFileAsync_NonExistentFile_ThrowsException()
    {
        var act = async () => await _service.AnalyzeFileAsync("/nonexistent/path/file.dll", "file.dll", "hash");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AnalyzeFileAsync_NewUploadId_GeneratedEachTime()
    {
        var path = CreatePEFile(CreateMinimalPEFile());

        var result1 = await _service.AnalyzeFileAsync(path, "test.dll", "hash");
        var result2 = await _service.AnalyzeFileAsync(path, "test.dll", "hash");

        result1.UploadId.Should().NotBe(result2.UploadId);
    }
}
