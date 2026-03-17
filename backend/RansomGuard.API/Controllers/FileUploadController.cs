using Microsoft.AspNetCore.Mvc;
using RansomGuard.API.Models;
using RansomGuard.API.Services;
using RansomGuard.API.Validators;

namespace RansomGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadHelper _fileHelper;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            IFileUploadHelper fileHelper,
            ILogger<FileUploadController> logger)
        {
            _fileHelper = fileHelper;
            _logger = logger;
        }

        /// <summary>
        /// Upload a PE file (.exe or .dll) for ransomware analysis
        /// </summary>
        /// <param name="file">The PE file to analyze</param>
        /// <returns>Upload result with analysis ID</returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            _logger.LogInformation("Upload attempt: {Filename}, Size: {Size}", file?.FileName, file?.Length);

            // Validation: File present
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = "FILE_REQUIRED",
                    Message = "No file uploaded"
                });
            }

            // Validation: File size - Call static method
            if (!FileValidator.IsValidSize(file.Length))
            {
                _logger.LogWarning("File too large: {Size} bytes", file.Length);
                return BadRequest(new ErrorResponse
                {
                    Code = "FILE_TOO_LARGE",
                    Message = "File exceeds 10MB limit"
                });
            }

            // Validation: Extension - Call static method
            if (!FileValidator.IsValidExtension(file.FileName))
            {
                _logger.LogWarning("Invalid extension: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_FILE_TYPE",
                    Message = "Only .exe and .dll files are allowed"
                });
            }

            // Validation: Path traversal - Call static method
            if (FileValidator.ContainsPathTraversal(file.FileName))
            {
                _logger.LogWarning("Path traversal attempt: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_FILENAME",
                    Message = "Filename contains invalid characters"
                });
            }

            // Validation: PE header (magic bytes) - Call static method
            using var stream = file.OpenReadStream();
            if (!await FileValidator.IsValidPEHeaderAsync(stream))
            {
                _logger.LogWarning("Invalid PE header: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_PE_HEADER",
                    Message = "File is not a valid PE executable"
                });
            }

            // Save file with GUID filename
            var (filePath, hash) = await _fileHelper.SaveUploadedFileAsync(stream, file.FileName);


#pragma warning disable S1135 // Track uses of "TODO" tags
            // TODO: Call PE Analysis Service (Milestone 5)
            // TODO: Save to database (Milestone 5)
            // For now, return placeholder response

#pragma warning restore S1135 // Track uses of "TODO" tags

            var uploadId = Guid.NewGuid();
            _logger.LogInformation("Upload successful: {UploadId}, Hash: {Hash}", uploadId, hash);

            return Ok(new UploadResponse
            {
                UploadId = uploadId,
                Message = "File uploaded successfully. Analysis pending.",
                RiskScore = 0,
                Verdict = Verdict.Safe
            });
        }
    }
}