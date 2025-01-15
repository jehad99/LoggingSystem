using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DistributedLoggingSystem.Services.BackEndStorageTypes
{
    public class S3LogStorageBackend : ILogStorageBackend
    {
        private readonly S3HttpClientService _s3Service;
        private readonly string _bucketName;
        private readonly string _secretAccessKey;
        private readonly string _accessKeyId;

        public S3LogStorageBackend(S3HttpClientService s3Service, IConfiguration configuration)
        {
            _s3Service = s3Service;
            _bucketName = configuration["AWS:BucketName"] ?? "logs";
            _secretAccessKey = configuration["AWS:SecretAccessKey"];
            _accessKeyId = configuration["AWS:AccessKeyId"];
        }

        public async Task StoreLogAsync(Log log)
        {
            // Ensure login is successful
            if (await _s3Service.LoginAsync(_accessKeyId, _secretAccessKey))
            {
                const string bucketName = "logs-bucket";

                // Create the bucket if it doesn't exist
                await _s3Service.CreateBucketIfNotExistsAsync(bucketName);

                // Define the prefix and file name for organizing the log
                var prefix = $"{log.Level}/{log.Service}";
                var fileName = $"{log.Timestamp:yyyy-MM-ddTHH-mm-ssZ}-{Guid.NewGuid()}.json";

                // Serialize the log content to JSON
                var logContent = JsonSerializer.Serialize(new
                {
                    service = log.Service,
                    level = log.Level,
                    message = log.Message,
                    timestamp = log.Timestamp.ToString("o")
                });

                // Upload the log
                await _s3Service.UploadLogAsync(
                    bucketName: bucketName,
                    prefix: prefix,
                    fileName: fileName,
                    logContent: logContent
                );

                // Optionally, get logs filtered by level or service
                var logs = await _s3Service.GetLogsAsync(bucketName, log.Level);
                Console.WriteLine("Filtered Logs:");
                foreach (var storedLog in logs)
                {
                    Console.WriteLine(storedLog);
                }
            }
            else
            {
                Console.WriteLine("Failed to authenticate with the S3 service.");
            }
        }

        public async Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters)
        {
            //    await _s3Service.LoginAsync(_accessKeyId, _secretAccessKey);

            //    // Build prefix based on query parameters
            //    var prefix = $"logs/{queryParameters.Service}/";
            //    if (queryParameters.StartTime.HasValue)
            //    {
            //        prefix += $"{queryParameters.StartTime.Value:yyyy-MM-dd}/";
            //    }

            //    // Retrieve the list of objects with the given prefix
            //    var objectKeys = await _s3Service.ListObjectsAsync(_bucketName, prefix);

            //    var logs = new List<Log>();
            //    foreach (var key in objectKeys)
            //    {
            //        var fileContent = await _s3Service.DownloadObjectAsync(_bucketName, key);
            //        var logEntries = ParseLogContent(Encoding.UTF8.GetString(fileContent), queryParameters);
            //        logs.AddRange(logEntries);
            //    }

            //    return logs;
            return null;
        }

        private IEnumerable<Log> ParseLogContent(string logContent, LogQueryParameters queryParameters)
        {
            var logs = new List<Log>();
            var lines = logContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var log = ParseLogLine(line);

                if (log == null) continue;

                // Apply filters based on query parameters
                if (!string.IsNullOrEmpty(queryParameters.Level) && !log.Level.Equals(queryParameters.Level, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (queryParameters.StartTime.HasValue && log.Timestamp < queryParameters.StartTime.Value)
                    continue;

                if (queryParameters.EndTime.HasValue && log.Timestamp > queryParameters.EndTime.Value)
                    continue;

                logs.Add(log);
            }

            return logs;
        }

        private Log ParseLogLine(string line)
        {
            try
            {
                var parts = line.Split(' ', 3);
                var timestamp = DateTime.Parse(parts[0]);
                var level = parts[1].Trim('[', ']');
                var message = parts[2];

                return new Log
                {
                    Timestamp = timestamp,
                    Level = level,
                    Message = message
                };
            }
            catch
            {
                // Ignore invalid log lines
                return null;
            }
        }
        

    }
}
