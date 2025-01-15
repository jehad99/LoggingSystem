namespace DistributedLoggingSystem.Dtos
{
    public class BucketDto
    {
        public string Name { get; set; }
        public string CreationDate { get; set; }
        public int Objects { get; set; }
        public long Size { get; set; }
    }

}
