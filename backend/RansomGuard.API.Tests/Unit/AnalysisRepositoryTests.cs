using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using RansomGuard.API.Data;
using RansomGuard.API.Data.Entities;
using RansomGuard.API.Services;

namespace RansomGuard.API.Tests.Unit;

public class AnalysisRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly RansomGuardDbContext _context;
    private readonly AnalysisRepository _repository;

    public AnalysisRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RansomGuardDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new RansomGuardDbContext(options);
        _context.Database.EnsureCreated();

        var logger = new Mock<ILogger<AnalysisRepository>>().Object;
        _repository = new AnalysisRepository(_context, logger);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static AnalysisResultEntity CreateEntity(string verdict = "Safe", DateTime? timestamp = null) => new()
    {
        Id = Guid.NewGuid(),
        Filename = "test.exe",
        FileHash = "abc123",
        Timestamp = timestamp ?? DateTime.UtcNow,
        RiskScore = 10,
        Entropy = 5.0,
        SuspiciousAPIs = "[]",
        Verdict = verdict,
        SectionCount = 3,
        ImportCount = 50,
        ExportCount = 0
    };

    [Fact]
    public async Task SaveAnalysisAsync_ValidEntity_ReturnsSameId()
    {
        var entity = CreateEntity();

        var savedId = await _repository.SaveAnalysisAsync(entity);

        savedId.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetAnalysisByIdAsync_ExistingId_ReturnsEntity()
    {
        var entity = CreateEntity();
        await _repository.SaveAnalysisAsync(entity);

        var result = await _repository.GetAnalysisByIdAsync(entity.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Filename.Should().Be(entity.Filename);
        result.Verdict.Should().Be(entity.Verdict);
    }

    [Fact]
    public async Task GetAnalysisByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetAnalysisByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRecentAnalysesAsync_ReturnsOrderedByTimestampDescending()
    {
        var older = CreateEntity(timestamp: DateTime.UtcNow.AddHours(-1));
        var newer = CreateEntity(timestamp: DateTime.UtcNow);
        await _repository.SaveAnalysisAsync(older);
        await _repository.SaveAnalysisAsync(newer);

        var results = await _repository.GetRecentAnalysesAsync(10);

        results.First().Id.Should().Be(newer.Id);
        results.Last().Id.Should().Be(older.Id);
    }

    [Fact]
    public async Task GetRecentAnalysesAsync_RespectsCountLimit()
    {
        for (int i = 0; i < 5; i++)
            await _repository.SaveAnalysisAsync(CreateEntity());

        var results = await _repository.GetRecentAnalysesAsync(3);

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetRecentAnalysesAsync_WithVerdictFilter_ReturnsOnlyMatching()
    {
        await _repository.SaveAnalysisAsync(CreateEntity("Safe"));
        await _repository.SaveAnalysisAsync(CreateEntity("Ransomware"));
        await _repository.SaveAnalysisAsync(CreateEntity("Ransomware"));

        var results = await _repository.GetRecentAnalysesAsync(10, "Ransomware");

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Verdict.Should().Be("Ransomware"));
    }

    [Fact]
    public async Task GetRecentAnalysesAsync_WithNullFilter_ReturnsAll()
    {
        await _repository.SaveAnalysisAsync(CreateEntity("Safe"));
        await _repository.SaveAnalysisAsync(CreateEntity("Ransomware"));

        var results = await _repository.GetRecentAnalysesAsync(10, null);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentAnalysesAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var results = await _repository.GetRecentAnalysesAsync(10);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAnalysisAsync_StoresMlFields()
    {
        var entity = CreateEntity();
        entity.MlConfidence = 0.95;
        entity.MlModelVersion = "1.0.0";

        await _repository.SaveAnalysisAsync(entity);
        var result = await _repository.GetAnalysisByIdAsync(entity.Id);

        result!.MlConfidence.Should().Be(0.95);
        result.MlModelVersion.Should().Be("1.0.0");
    }

    [Fact]
    public async Task SaveAnalysisAsync_NullMlFields_StoredAsNull()
    {
        var entity = CreateEntity();

        await _repository.SaveAnalysisAsync(entity);
        var result = await _repository.GetAnalysisByIdAsync(entity.Id);

        result!.MlConfidence.Should().BeNull();
        result.MlModelVersion.Should().BeNull();
    }
}
