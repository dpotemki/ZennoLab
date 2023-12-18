namespace ZennoLab.ResourceAllocator.Models
{
    public class Project
    {
        public int MemoryCount { get; set; }
        public int AppTimeout { get; set; }
        public int TryCount { get; set; }
        public int MaxThreads { get; set; }
        public bool CalculateCanCompleteWithinInterval(int seconds)
        {
            var blockTime = AppTimeout;
            var blocks = (double)TryCount / MaxThreads;
            var totalTime = blockTime * blocks;

            return totalTime <= seconds;
        }
    }
}
