using Amazon.S3;
using Amazon.S3.Model;
using DistributedLoggingSystem.Models;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DistributedLoggingSystem.Services
{
    public class BatchLogService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly List<Log> _logBuffer = new();
        private readonly int _batchSize;
        private readonly LoggingDbContext _context;
        private readonly object _bufferLock = new();

        public BatchLogService(IAmazonS3 s3Client, IConfiguration configuration, LoggingDbContext context)
        {
            _s3Client = s3Client;
            _bucketName = configuration["S3BucketName"] ?? "log";
            _batchSize = int.Parse(configuration["BatchSize"] ?? "10");
            _context = context;
        }

        public void AddLog(Log log)
        {
            lock (_bufferLock)
            {
                _logBuffer.Add(log);

                if (_logBuffer.Count >= _batchSize)
                {
                    Task.Run(async () => await FlushBufferAsync());
                }
            }
        }

        public async Task FlushBufferAsync()
        {
            List<Log> logsToFlush;

            lock (_bufferLock)
            {
                if (_logBuffer.Count == 0) return;

                logsToFlush = new List<Log>(_logBuffer);
                _logBuffer.Clear();
            }

            try
            {
                var startTimestamp = logsToFlush.Min(log => log.Timestamp);
                var endTimestamp = logsToFlush.Max(log => log.Timestamp);
                var levels = string.Join(", ", logsToFlush.Select(log => log.Level).Distinct());
                var logCount = logsToFlush.Count;

                var batchContent = string.Join(Environment.NewLine, logsToFlush.Select(log => $"{log.Timestamp:O} [{log.Level}] {log.Message}"));
                var compressedContent = CompressLogs(batchContent);

                var fileName = $"logs/{DateTime.UtcNow:yyyy-MM-dd}/batch-{DateTime.UtcNow:HH-mm-ss}.gz";
                await UploadToS3Async(fileName, compressedContent);

                var batchMetadata = new BatchMetadata
                {
                    BatchFile = fileName,
                    StartTimestamp = startTimestamp,
                    EndTimestamp = endTimestamp,
                    LogCount = logCount,
                    Levels = levels
                };

                await _context.BatchMetadata.AddAsync(batchMetadata);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during log flush: {ex.Message}");
                // Optionally rethrow or log to a monitoring system
            }
        }

        private byte[] CompressLogs(string content)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                using var writer = new StreamWriter(gzipStream, Encoding.UTF8);
                writer.Write(content);
                writer.Flush();
            }
            return memoryStream.ToArray();
        }

        private async Task UploadToS3Async(string key, byte[] content)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = new MemoryStream(content),
                    ContentType = "application/gzip"
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload log batch to S3: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedResponse<Log>> GetLogsByRange(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
        {
            try
            {
                var metadata = _context.BatchMetadata
                    .Where(b => b.StartTimestamp <= endDate && b.EndTimestamp >= startDate);

                var results = metadata
                    .OrderByDescending(b => b.StartTimestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var logs = new List<Log>();

                foreach (var batch in results)
                {
                    var content = await DownloadAndDecompressAsync(batch.BatchFile);
                    var batchLogs = ParseLogs(content);
                    logs.AddRange(batchLogs);
                }

                return new PaginatedResponse<Log>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = metadata.Count(),
                    Data = logs
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching logs: {ex.Message}");
                throw;
            }
        }

        private async Task<string> DownloadAndDecompressAsync(string key)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var responseStream = response.ResponseStream;
                using var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream, Encoding.UTF8);

                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download and decompress log batch: {ex.Message}");
                throw;
            }
        }

        private List<Log> ParseLogs(string content)
        {
            var logs = new List<Log>();

            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
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
        public async Task EnsureBucketExistsAsync()
        {
            try
            {
                var buckets = await _s3Client.ListBucketsAsync();

                // Check if the bucket already exists
                if (!buckets.Buckets.Any(b => b.BucketName == _bucketName))
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest
                    {
                        BucketName = _bucketName
                    });

                    Console.WriteLine($"Bucket '{_bucketName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Bucket '{_bucketName}' already exists.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bucket: {ex.Message}");
                throw;
            }
        }

    }

    public class PaginatedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; }
    }
}
