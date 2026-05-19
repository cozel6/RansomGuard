using System.Security.Cryptography;

namespace RansomGuard.API.Services;

public interface IFileUploadHelper
{
    Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename);
    void DeleteFile(string filePath);
    string GetTempDirectory();
}

public class FileUploadHelper : IFileUploadHelper
{
    private readonly ILogger<FileUploadHelper> _logger;
    private readonly string _tempDirectory;

    public FileUploadHelper(IConfiguration configuration, ILogger<FileUploadHelper> logger)
    {
        _logger = logger;
        _tempDirectory = configuration["RansomGuard:TempDirectory"] ?? "./uploads/temp";

        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
            _logger.LogInformation("Created temp directory: {Path}", _tempDirectory);
        }
    }

    public string GetTempDirectory() => _tempDirectory;

    public async Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename)
    {
        var extension = Path.GetExtension(originalFilename);
        var guid = Guid.NewGuid();
        var safeFilename = $"{guid}{extension}";
        var filePath = Path.Combine(_tempDirectory, safeFilename);

        using (var fileStreamDisk = File.Create(filePath))
        {
            fileStream.Position = 0;
            await fileStream.CopyToAsync(fileStreamDisk);
        }

        var hash = await CalculateSHA256Async(filePath);
        _logger.LogInformation("File saved: {Filename} -> {SafeFilename}, Hash: {Hash}", originalFilename, safeFilename, hash);
        return (filePath, hash);
    }

    public void DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted temp file: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", filePath);
        }
    }

    private static async Task<string> CalculateSHA256Async(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexStringLower(hashBytes);
    }
}
