using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Models;

namespace DistributedLoggingSystem.Interface
{
    public interface ILogStorageBackend
    {
        Task StoreLogAsync(Log log);
        Task<List<Log>> RetrieveLogsAsync(LogQueryParameters queryParameters);

    }

}
