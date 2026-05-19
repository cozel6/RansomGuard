using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RansomGuard.API.Data.Entities;
using RansomGuard.API.Services;
using System.Net;
using System.Text.Json;

namespace RansomGuard.API.Tests.Integration;

public class AnalysisControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AnalysisControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> SeedAnalysisEntity(string verdict = "Safe")
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
        var entity = new AnalysisResultEntity
        {
            Id = Guid.NewGuid(),
            Filename = "seeded.dll",
            FileHash = "hash123",
            Timestamp = DateTime.UtcNow,
            RiskScore = 10,
            Entropy = 5.0,
            SuspiciousAPIs = "[]",
            Verdict = verdict,
            SectionCount = 3,
            ImportCount = 10,
            ExportCount = 0
        };
        await repo.SaveAnalysisAsync(entity);
        return entity.Id;
    }

    [Fact]
    public async Task GetAnalysis_ExistingId_ReturnsOk()
    {
        var id = await SeedAnalysisEntity();

        var response = await _client.GetAsync($"/api/analysis/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task GetAnalysis_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/analysis/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAnalysis_InvalidGuid_ReturnsNotFound()
    {
        // Route constraint {id:guid} causes 404 when guid format is invalid
        var response = await _client.GetAsync("/api/analysis/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAnalysis_ResponseContainsVerdict()
    {
        var id = await SeedAnalysisEntity("Ransomware");

        var response = await _client.GetAsync($"/api/analysis/{id}");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("Ransomware");
    }

    [Fact]
    public async Task GetHistory_ReturnsOk()
    {
        await SeedAnalysisEntity("Safe");

        var response = await _client.GetAsync("/api/analysis/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<JsonElement[]>(body);
        results.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHistory_WithVerdictFilter_ReturnsFiltered()
    {
        await SeedAnalysisEntity("Safe");
        await SeedAnalysisEntity("Ransomware");

        var response = await _client.GetAsync("/api/analysis/history?verdictFilter=Ransomware");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Ransomware");
    }

    [Fact]
    public async Task GetHistory_WithCountParam_RespectsLimit()
    {
        for (int i = 0; i < 5; i++)
            await SeedAnalysisEntity();

        var response = await _client.GetAsync("/api/analysis/history?count=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<JsonElement[]>(body);
        results.Should().HaveCountLessOrEqualTo(2);
    }

    [Fact]
    public async Task GetAnalysis_HasCacheControlHeader()
    {
        var id = await SeedAnalysisEntity();

        var response = await _client.GetAsync($"/api/analysis/{id}");

        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl!.MaxAge.Should().Be(TimeSpan.FromSeconds(300));
    }

    [Fact]
    public async Task GetHistory_HasCacheControlHeader()
    {
        var response = await _client.GetAsync("/api/analysis/history");

        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl!.MaxAge.Should().Be(TimeSpan.FromSeconds(30));
    }
}
