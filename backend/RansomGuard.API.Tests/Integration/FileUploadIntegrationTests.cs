using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;

namespace RansomGuard.API.Tests.Integration;

public class FileUploadIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FileUploadIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadFile_ValidPEFile_ReturnsSuccess()
    {
        var peBytes = CreateMinimalPEFile();
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(peBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        content.Add(fileContent, "file", "test.exe");

        var response = await _client.PostAsync("/api/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadFile_InvalidExtension_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent([0x4D, 0x5A]);
        content.Add(fileContent, "file", "test.txt");

        var response = await _client.PostAsync("/api/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Note: null byte injection in HTTP filename headers is blocked by the .NET HTTP client itself
    // (throws FormatException). The behavior is covered by FileValidatorTests unit tests.

[Fact]
    public async Task UploadFile_PathTraversal_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(CreateMinimalPEFile());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        content.Add(fileContent, "file", "../../etc/passwd.exe");

        var response = await _client.PostAsync("/api/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadFile_NoFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/api/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static byte[] CreateMinimalPEFile()
    {
        var bytes = new byte[0x200];
        bytes[0x00] = 0x4D; bytes[0x01] = 0x5A;
        bytes[0x3C] = 0x40;
        bytes[0x40] = 0x50; bytes[0x41] = 0x45;
        bytes[0x44] = 0x4C; bytes[0x45] = 0x01;
        bytes[0x54] = 0xE0; bytes[0x55] = 0x00;
        bytes[0x56] = 0x02; bytes[0x57] = 0x00;
        bytes[0x58] = 0x0B; bytes[0x59] = 0x01;
        return bytes;
    }
}
