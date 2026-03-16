using System.Formats.Asn1;

namespace RansomGuard.API.Validators
{
    public static class FileValidator
    {
        private static readonly string[] AllowedExtensions = [".exe", ".dll"];
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public static bool IsValidExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;

            }

            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public static bool IsValidSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSize;
        }

        public static bool ContainsPathTraversal(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            // Check for path traversal patterns
            return filename.Contains("..") ||
                   filename.Contains('/') ||
                   filename.Contains('\\');
        }
        public static async Task<bool> IsValidPEHeaderAsync(Stream fileStream)
        {
            if (fileStream == null || fileStream.Length < 2)
            {
                return false;
            }
            // Save position to restore later
            var originalPosition = fileStream.Position;
            fileStream.Position = 0;
            try
            {
                // Read first 2 bytes (MZ signature)
                var buffer = new byte[2];
                var bytesRead = await fileStream.ReadAsync(buffer);
                if (bytesRead != 2)
                {
                    return false;
                }
                // Check for MZ magic bytes (0x4D5A)
                return buffer[0] == 0x4D && buffer[1] == 0x5A;
            }
            finally
            {
                // Restore steam position
                fileStream.Position = originalPosition;
            }
        }
    }
}