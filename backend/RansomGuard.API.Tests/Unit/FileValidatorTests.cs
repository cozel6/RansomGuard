using FluentAssertions;
using RansomGuard.API.Validators;
using Xunit;

namespace RansomGuard.API.Tests.Unit;

public class FileValidatorTests
{

    public FileValidatorTests()
    {

    }

    #region Extension Validation Tests

    [Theory]
    [InlineData("malware.exe", true)]
    [InlineData("library.dll", true)]
    [InlineData("UPPERCASE.EXE", true)]
    [InlineData("MixedCase.DLL", true)]
    public void IsValidExtension_ValidExtensions_ReturnsTrue(string filename, bool expected)
    {
        // Act
        var result = FileValidator.IsValidExtension(filename);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.bat")]
    [InlineData("archive.zip")]
    [InlineData("noextension")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValidExtension_InvalidExtensions_ReturnsFalse(string filename)
    {
        // Act
        var result = FileValidator.IsValidExtension(filename);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Size Validation Tests

    [Theory]
    [InlineData(1024, true)]                    // 1KB - valid
    [InlineData(5242880, true)]                 // 5MB - valid
    [InlineData(10485760, true)]                // 10MB exact - valid
    public void IsValidSize_ValidSizes_ReturnsTrue(long fileSize, bool expected)
    {
        // Act
        var result = FileValidator.IsValidSize(fileSize);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]                             // 0 bytes
    [InlineData(-1)]                            // negative
    [InlineData(10485761)]                      // 10MB + 1 byte
    [InlineData(52428800)]                      // 50MB
    public void IsValidSize_InvalidSizes_ReturnsFalse(long fileSize)
    {
        // Act
        var result = FileValidator.IsValidSize(fileSize);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Path Traversal Tests

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\..\\windows\\system32\\")]
    [InlineData("../config/secrets.json")]
    [InlineData("normal/path/file.exe")]
    [InlineData("path\\with\\backslash.dll")]
    public void ContainsPathTraversal_MaliciousFilenames_ReturnsTrue(string filename)
    {
        // Act
        var result = FileValidator.ContainsPathTraversal(filename);

        // Assert
        result.Should().BeTrue("because path traversal should be detected");
    }

    [Theory]
    [InlineData("safe-filename.exe")]
    [InlineData("library_v2.dll")]
    [InlineData("MyProgram.EXE")]
    public void ContainsPathTraversal_SafeFilenames_ReturnsFalse(string filename)
    {
        // Act
        var result = FileValidator.ContainsPathTraversal(filename);

        // Assert
        result.Should().BeFalse("because filename is safe");
    }

    #endregion
    #region PE Header Validation Tests

    [Fact]
    public async Task IsValidPEHeader_ValidMZSignature_ReturnsTrue()
    {
        // Arrange
        var stream = new MemoryStream([0x4D, 0x5A, 0x00, 0x00]); // MZ header

        // Act
        var result = await FileValidator.IsValidPEHeaderAsync(stream);

        // Assert
        result.Should().BeTrue();
        stream.Position.Should().Be(0); // Stream position restored
    }

    [Fact]
    public async Task IsValidPEHeader_InvalidSignature_ReturnsFalse()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // ZIP header (PK)

        // Act
        var result = await FileValidator.IsValidPEHeaderAsync(stream);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidPEHeader_EmptyStream_ReturnsFalse()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var result = await FileValidator.IsValidPEHeaderAsync(stream);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidPEHeader_NullStream_ReturnsFalse()
    {
        // Act
        var result = await FileValidator.IsValidPEHeaderAsync(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}