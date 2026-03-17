using System.Security.Cryptography;

namespace RansomGuard.API.Services
{
    public interface IFileUploadHelper
    {
        Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename);

        void DeleteFile(string filePath);
        string GetTempDirectory();
    }

    public class FileUploadHelper : IFileUploadHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileUploadHelper> _logger;
        private readonly string _tempDirectory;

        public FileUploadHelper(IConfiguration configuration, ILogger<FileUploadHelper> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _tempDirectory = _configuration["RansomGuard:TempDirectory"] ?? "./uploads/temp";

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
                _logger.LogInformation("Created temp directory: {Path}", _tempDirectory);
            }
        }
        public string GetTempDirectory() => _tempDirectory;


        public async Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename)
        {
            // Generate GUID file name (NEVER trust user input)
            var extension = Path.GetExtension(originalFilename);
            var guid = Guid.NewGuid();
            var safeFilename = $"{guid}{extension}";
            var filePath = Path.Combine(_tempDirectory, safeFilename);

            // Save file to disk
            using var fileStreamDisk = File.Create(filePath);
            fileStream.Position = 0; // Reset stream
            await fileStream.CopyToAsync(fileStreamDisk);

            // Calulate SHA256 hash
            var hash = await CalculateSHA256Async(filePath);
            _logger.LogInformation("File saved: {Filename} -> {SafeFilename}, Hash:{Hash}", originalFilename, safeFilename, hash);
            return (filePath, hash);

        }

        public void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Delete temp file: {Path}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File to delete file: {Path}", filePath);
            }
        }
        private async Task<string> CalculateSHA256Async(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexStringLower(hashBytes);
        }
    }
}

