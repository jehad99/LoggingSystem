using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;

namespace DistributedLoggingSystem.Services.BackEndStorageTypes
{
    public class FileSystemLogStorageBackend : ILogStorageBackend
    {
        private readonly string _baseDirectory;

        public FileSystemLogStorageBackend(IConfiguration configuration)
        {
            _baseDirectory = configuration["LogStorage:FilePath"] ?? "/var/logs/distributed_system";
        }

        public async Task StoreLogAsync(Log log)
        {
            var directoryPath = Path.Combine(_baseDirectory, log.Service, DateTime.UtcNow.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, $"{Guid.NewGuid()}.log");
            var logContent = $"{log.Timestamp:O} [{log.Level}] {log.Message}";
            await File.WriteAllTextAsync(filePath, logContent);
        }

        public Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters)
        {
            // Implement logic to read logs from files and apply filters
            throw new NotImplementedException("RetrieveLogsAsync for File System is not implemented yet.");
        }
    }

}
