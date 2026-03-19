using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace RansomGuard.API.Tests.Integration
{
    public class FileUploadIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public FileUploadIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
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

        private static byte[] CreateMinimalPEFile()
        {
            var bytes = new byte[0x200]; // 512 bytes

            // DOS Header
            bytes[0x00] = 0x4D; // 'M'
            bytes[0x01] = 0x5A; // 'Z'
            bytes[0x3C] = 0x40; // e_lfanew: PE header at offset 0x40

            // PE Signature at 0x40
            bytes[0x40] = 0x50; // 'P'
            bytes[0x41] = 0x45; // 'E'
            bytes[0x42] = 0x00;
            bytes[0x43] = 0x00;

            // COFF File Header at 0x44
            bytes[0x44] = 0x4C; // Machine: IMAGE_FILE_MACHINE_I386 (0x014C)
            bytes[0x45] = 0x01;
            bytes[0x46] = 0x00; // NumberOfSections: 0
            bytes[0x47] = 0x00;
            bytes[0x54] = 0xE0; // SizeOfOptionalHeader: 224 (PE32)
            bytes[0x55] = 0x00;
            bytes[0x56] = 0x02; // Characteristics: IMAGE_FILE_EXECUTABLE_IMAGE
            bytes[0x57] = 0x00;

            // Optional Header at 0x58
            bytes[0x58] = 0x0B; // Magic: 0x010B (PE32)
            bytes[0x59] = 0x01;

            return bytes;
        }
    }
}