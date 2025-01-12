using DistributedLoggingSystem.Interface;
using DistributedLoggingSystem.Services.BackEndStorageTypes;

public class LogStorageBackendFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _backendType;

    public LogStorageBackendFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _backendType = configuration["LogStorage:Backend"] ?? "Database";
    }

    public ILogStorageBackend CreateBackend()
    {
        return _backendType switch
        {
            "S3" => _serviceProvider.GetRequiredService<S3LogStorageBackend>(),
            "Database" => _serviceProvider.GetRequiredService<DatabaseLogStorageBackend>(),
            "FileSystem" => _serviceProvider.GetRequiredService<FileSystemLogStorageBackend>(),
            _ => throw new Exception($"Unsupported backend type: {_backendType}")
        };
    }
}
