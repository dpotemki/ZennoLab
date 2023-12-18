namespace ZennoLab.ResourceAllocator.Interfaces
{
    public interface ISystemResourceMonitor
    {
        double GetTotalMemoryInMb();
        double GetUsedMemoryInMb();
        double GetFreeMemoryInPercentage();

        double GetTotalCpuUsage();
    }
}
