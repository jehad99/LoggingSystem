namespace DistributedLoggingSystem.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Service { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string BackendType { get; set; }
    }
}
