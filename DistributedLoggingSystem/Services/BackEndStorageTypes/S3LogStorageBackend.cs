using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;
using System.Text;

namespace DistributedLoggingSystem.Services.BackEndStorageTypes
{
    public class S3LogStorageBackend : ILogStorageBackend
    {
        private readonly S3HttpClientService _s3Service;
        private readonly string _bucketName;

        public S3LogStorageBackend(S3HttpClientService s3Service, IConfiguration configuration)
        {
            _s3Service = s3Service;
            _bucketName = configuration["AWS:BucketName"] ?? "logs";
        }

        public async Task StoreLogAsync(Log log)
        {
            var fileName = $"logs/{log.Service}/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}.log";
            var logContent = $"{log.Timestamp:O} [{log.Level}] {log.Message}";
            await _s3Service.UploadFileAsync(_bucketName, fileName, Encoding.UTF8.GetBytes(logContent));
        }

        public async Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters)
        {
            // Implement logic to list and retrieve logs from S3, decompress, and parse them
            throw new NotImplementedException("RetrieveLogsAsync for S3 is not implemented yet.");
        }
    }

}
