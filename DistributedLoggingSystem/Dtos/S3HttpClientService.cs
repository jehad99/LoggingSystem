using System.Text.Json;
using System.Text;
using DistributedLoggingSystem.Dtos;

public class S3HttpClientService
{
    private readonly HttpClient _httpClient;
    private string _token;

    public S3HttpClientService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(configuration["AWS:ServiceURL"]) };
    }

    public async Task<bool> LoginAsync(string accessKey, string secretKey)
    {
        var requestBody = new
        {
            accessKey = accessKey,
            secretKey = secretKey
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/v1/login", content);

        if (response.IsSuccessStatusCode)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
            {
                var setCookieHeader = cookieValues.FirstOrDefault();
                if (setCookieHeader != null)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(setCookieHeader, @"token=([^;]+)");
                    if (match.Success)
                    {
                        _token = match.Groups[1].Value;
                        Console.WriteLine($"Extracted Token: {_token}");
                    }
                }
            }

            return true;
        }

        return false;
    }

    private void EnsureTokenHeader()
    {
        if (string.IsNullOrEmpty(_token))
        {
            throw new InvalidOperationException("Authentication token is missing. Please login first.");
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("Cookie"))
        {
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"token={_token}");
        }
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName)
    {
        EnsureTokenHeader();

        var buckets = await GetBucketsAsync();
        if (buckets.Contains(bucketName))
        {
            Console.WriteLine("Bucket already exists.");
            return false;
        }

        return await CreateBucketAsync(bucketName);
    }

    public async Task<List<string>> GetBucketsAsync()
    {
        EnsureTokenHeader();

        var response = await _httpClient.GetAsync("/api/v1/buckets");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var bucketsElement = document.RootElement.GetProperty("buckets");
            var bucketDtos = JsonSerializer.Deserialize<List<BucketDto>>(bucketsElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return bucketDtos.Select(bucket => bucket.Name).ToList() ?? new List<string>();
        }

        Console.WriteLine($"Failed to fetch buckets. Status Code: {response.StatusCode}");
        return new List<string>();
    }

    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        EnsureTokenHeader();

        var payload = new
        {
            name = bucketName,
            versioning = new
            {
                enabled = false,
                excludePrefixes = new string[] { },
                excludeFolders = false
            },
            locking = false
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/buckets", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Bucket created successfully.");
            return true;
        }

        Console.WriteLine($"Failed to create bucket. Status Code: {response.StatusCode}");
        return false;
    }

    public async Task UploadLogAsync(string bucketName, string prefix, string fileName, string logContent)
    {
        EnsureTokenHeader();

        var requestUrl = $"/api/v1/buckets/{bucketName}/objects/upload?prefix={Uri.EscapeDataString(prefix)}";

        using var contentFormData = new MultipartFormDataContent();
        var fileContent = new StringContent(logContent, Encoding.UTF8, "text/plain");
        fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
        {
            Name = "6", // Matches the "name" field in your form data
            FileName = fileName
        };
        contentFormData.Add(fileContent);

        var response = await _httpClient.PostAsync(requestUrl, contentFormData);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload log: {response.StatusCode} {response.ReasonPhrase}");
        }

        Console.WriteLine("Log uploaded successfully.");
    }

    public async Task<List<string>> GetLogsAsync(string bucketName, string filter = null)
    {
        EnsureTokenHeader();

        var response = await _httpClient.GetAsync($"/api/v1/buckets/{bucketName}/objects");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var logsElement = document.RootElement.GetProperty("objects");

            var logs = JsonSerializer.Deserialize<List<string>>(logsElement.GetRawText());

            if (!string.IsNullOrEmpty(filter))
            {
                logs = logs?.FindAll(log => log.Contains(filter));
            }

            return logs ?? new List<string>();
        }

        Console.WriteLine($"Failed to fetch logs. Status Code: {response.StatusCode}");
        return new List<string>();
    }
}
