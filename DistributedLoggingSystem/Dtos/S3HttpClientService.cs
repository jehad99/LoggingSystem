using System.Net.Http.Headers;
using System.Text;

public class S3HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _serviceUrl;

    public S3HttpClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _accessKey = configuration["AWS:AccessKeyId"];
        _secretKey = configuration["AWS:SecretAccessKey"];
        _serviceUrl = configuration["AWS:ServiceURL"];
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, byte[] content = null)
    {
        var url = $"{_serviceUrl}/{path}";
        var request = new HttpRequestMessage(method, url);

        // Example: Add necessary headers (adjust as per the S3 protocol)
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_accessKey}:{_secretKey}")));

        if (content != null)
        {
            request.Content = new ByteArrayContent(content);
        }

        return request;
    }

    public async Task CreateBucketAsync(string bucketName)
    {
        var request = CreateRequest(HttpMethod.Put, bucketName);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Conflict)
        {
            throw new Exception($"Failed to create bucket: {response.ReasonPhrase}");
        }
    }

    public async Task UploadFileAsync(string bucketName, string filePath, byte[] fileContent)
    {
        var request = CreateRequest(HttpMethod.Put, $"{bucketName}/{filePath}", fileContent);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload file: {response.ReasonPhrase}");
        }
    }

    public async Task<byte[]> DownloadFileAsync(string bucketName, string filePath)
    {
        var request = CreateRequest(HttpMethod.Get, $"{bucketName}/{filePath}");
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to download file: {response.ReasonPhrase}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
