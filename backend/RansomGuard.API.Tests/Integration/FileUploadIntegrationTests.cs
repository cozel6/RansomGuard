using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace RansomGuard.API.Tests.Integration
{
    public class FileUploadIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public FileUploadIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UploadFile_ValidPEFile_ReturnsSuccess()
        {
            // Arrange - Create minimal PE file (MZ header)
            var peBytes = CreateMinimalPEFile();
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(peBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(fileContent, "file", "test.exe");

            // Act
            var response = await _client.PostAsync("/api/fileupload/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UploadFile_InvalidExtension_ReturnsBadRequest()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x4D, 0x5A });
            content.Add(fileContent, "file", "test.txt");

            // Act
            var response = await _client.PostAsync("/api/fileupload/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private byte[] CreateMinimalPEFile()
        {
            // Minimal PE file with MZ header
            var bytes = new byte[512];
            bytes[0] = 0x4D; // M
            bytes[1] = 0x5A; // Z
            return bytes;
        }
    }
}