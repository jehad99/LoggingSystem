using Amazon.S3;
using Amazon.S3.Model;
using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DistributedLoggingSystem.Services
{
    public class BatchLogService
    {
        private readonly ILogStorageBackend _logStorageBackend;

        public BatchLogService(LogStorageBackendFactory factory)
        {
            _logStorageBackend = factory.CreateBackend();
        }

        public async Task AddLog(Log log)
        {
            await _logStorageBackend.StoreLogAsync(log);
        }

        public async Task<List<Log>> GetLogs(LogQueryParameters queryParameters)
        {
            return await _logStorageBackend.RetrieveLogsAsync(queryParameters);
        }

    }

}
