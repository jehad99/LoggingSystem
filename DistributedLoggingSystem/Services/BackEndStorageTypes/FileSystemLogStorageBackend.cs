using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

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

        public async Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters)
        {
            var logs = new List<Log>();
            var baseDirectory = "/var/logs/distributed_system"; // This should come from configuration

            var directory = Path.Combine(baseDirectory, queryParameters.Service ?? string.Empty);

            if (!Directory.Exists(directory))
            {
                return logs; 
            }

            var files = Directory.GetFiles(directory, "*.log", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileContent = await File.ReadAllTextAsync(file);

                var fileLogs = ParseLogs(fileContent);

                fileLogs = fileLogs.Where(log =>
                    (!queryParameters.Level.IsNullOrEmpty() || log.Level == queryParameters.Level) &&
                    (!queryParameters.StartTime.HasValue || log.Timestamp >= queryParameters.StartTime) &&
                    (!queryParameters.EndTime.HasValue || log.Timestamp <= queryParameters.EndTime)
                ).ToList();

                logs.AddRange(fileLogs);
            }

            return logs;
        }

        private List<Log> ParseLogs(string fileContent)
        {
            var logs = new List<Log>();

            var lines = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^(?<timestamp>.+?) \[(?<level>.+?)\] (?<message>.+)$");

                if (match.Success)
                {
                    logs.Add(new Log
                    {
                        Timestamp = DateTime.Parse(match.Groups["timestamp"].Value),
                        Level = match.Groups["level"].Value,
                        Message = match.Groups["message"].Value
                    });
                }
            }

            return logs;
        }

    }

}
