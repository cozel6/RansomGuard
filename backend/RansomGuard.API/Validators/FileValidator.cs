namespace RansomGuard.API.Validators
{
    public class FileValidator
    {
        private static readonly string[] AllowedExtensions = { ".exe", ".dll" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public bool IsValidExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;

            }

            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public bool IsValidSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSize;
        }

        public bool ContainsPathTraversal(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // Check for path traversal patterns
            return filename.Contains("..") ||
                   filename.Contains("/") ||
                   filename.Contains("\\");
        }
    }
}