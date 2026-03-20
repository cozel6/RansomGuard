using Microsoft.AspNetCore.Mvc;
using RansomGuard.API.Models;
using RansomGuard.API.Services;
using System.Text.Json;

namespace RansomGuard.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisRepository _repository;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(IAnalysisRepository repository, ILogger<AnalysisController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve analysis result by ID
        /// </summary>
        /// <param name="id">Analysis ID (GUID)</param>
        /// <returns>Analysis result</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAnalysis(Guid id)
        {
            var entity = await _repository.GetAnalysisByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Analysis not found: {Id}", id);
                return NotFound(new ErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "Analysis result not found"
                });
            }
            var result = new AnalysisResult
            {
                UploadId = entity.Id,
                Filename = entity.Filename,
                Timestamp = entity.Timestamp,
                RiskScore = entity.RiskScore,
                Entropy = entity.Entropy,
                SuspiciousAPIs = JsonSerializer.Deserialize<List<string>>(entity.SuspiciousAPIs) ?? new(),
                Verdict = Enum.Parse<Verdict>(entity.Verdict),
                FileHash = entity.FileHash,
                SectionCount = entity.SectionCount,
                ImportCount = entity.ImportCount,
                ExportCount = entity.ExportCount
            };
            return Ok(result);
        }
        /// <summary>
        /// Get recent analysis history
        /// </summary>
        /// <param name="count">Number of results (default 10, max 100)</param>
        /// <returns>List of recent analyses</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<AnalysisResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory([FromQuery] int count = 10)
        {
            // Cap at 100 to prevent abuse
            count = Math.Min(count, 100);

            var entities = await _repository.GetRecentAnalysesAsync(count);

            var results = entities.Select(e => new AnalysisResult
            {
                UploadId = e.Id,
                Filename = e.Filename,
                Timestamp = e.Timestamp,
                RiskScore = e.RiskScore,
                Entropy = e.Entropy,
                SuspiciousAPIs = JsonSerializer.Deserialize<List<string>>(e.SuspiciousAPIs) ?? new(),
                Verdict = Enum.Parse<Verdict>(e.Verdict),
                FileHash = e.FileHash,
                SectionCount = e.SectionCount,
                ImportCount = e.ImportCount,
                ExportCount = e.ExportCount
            }).ToList();

            return Ok(results);
        }
    }
}
