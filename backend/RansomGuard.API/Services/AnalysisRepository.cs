using Microsoft.EntityFrameworkCore;
using RansomGuard.API.Data;
using RansomGuard.API.Data.Entities;

namespace RansomGuard.API.Services
{
    public interface IAnalysisRepository
    {
        Task<Guid> SaveAnalysisAsync(AnalysisResultEntity entity);
        Task<AnalysisResultEntity?> GetAnalysisByIdAsync(Guid id);
        Task<List<AnalysisResultEntity>> GetRecentAnalysesAsync(int count);
    }

    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly RansomGuardDbContext _context;
        private readonly ILogger<AnalysisRepository> _logger;
        public AnalysisRepository(RansomGuardDbContext context, ILogger<AnalysisRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AnalysisResultEntity?> GetAnalysisByIdAsync(Guid id)
        {
            return await _context.AnalysisResults
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<AnalysisResultEntity>> GetRecentAnalysesAsync(int count)
        {
            return await _context.AnalysisResults
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Guid> SaveAnalysisAsync(AnalysisResultEntity entity)
        {
            _context.AnalysisResults.Add(entity);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Analysis saved: {Id}, Verdict: {Verdict}, Risk: {Risk}",
                entity.Id, entity.Verdict, entity.RiskScore);

            return entity.Id;
        }
    }
}
