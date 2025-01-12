namespace DistributedLoggingSystem.Models
{
    public class BatchMetadata
    {
        public int Id { get; set; }
        public string BatchFile { get; set; } 
        public DateTime StartTimestamp { get; set; } 
        public DateTime EndTimestamp { get; set; } 
        public int LogCount { get; set; } 
        public string Levels { get; set; } 

    }
}
