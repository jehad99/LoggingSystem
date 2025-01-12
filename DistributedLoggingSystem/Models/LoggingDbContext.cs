using Microsoft.EntityFrameworkCore;

namespace DistributedLoggingSystem.Models
{
    public class LoggingDbContext :DbContext
    {
        public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
        {
        }
        public DbSet<BatchMetadata> BatchMetadata { get; set; }
    }
}
