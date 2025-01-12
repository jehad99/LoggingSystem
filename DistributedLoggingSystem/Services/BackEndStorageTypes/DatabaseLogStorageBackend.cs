using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedLoggingSystem.Services.BackEndStorageTypes
{
    public class DatabaseLogStorageBackend : ILogStorageBackend
    {
        private readonly LoggingDbContext _context;

        public DatabaseLogStorageBackend(LoggingDbContext context)
        {
            _context = context;
        }

        public async Task StoreLogAsync(Log log)
        {
            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters)
        {
            var query = _context.Logs.AsQueryable();

            if (!string.IsNullOrEmpty(queryParameters.Service))
                query = query.Where(log => log.Service == queryParameters.Service);

            if (!string.IsNullOrEmpty(queryParameters.Level))
                query = query.Where(log => log.Level == queryParameters.Level);

            if (queryParameters.StartTime.HasValue)
                query = query.Where(log => log.Timestamp >= queryParameters.StartTime);

            if (queryParameters.EndTime.HasValue)
                query = query.Where(log => log.Timestamp <= queryParameters.EndTime);

            return await query.ToListAsync();
        }
    }

}
