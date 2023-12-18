namespace ZennoLab.Miner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!Mining.ValidateArguments(args, out var megabyteSize, out var appTimeout))
            {
                return;
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Timer timer = new Timer(o => cancellationTokenSource.Cancel(), null, TimeSpan.FromSeconds(appTimeout), Timeout.InfiniteTimeSpan);

            var ramAllocationResult = Mining.AllocateAndFillMemoryArrays(megabyteSize);

            try
            {
                //simple cpu loading
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Mining.AllocateCPU();
                }
            }
            finally
            {
                timer.Dispose();
            }

            Console.WriteLine($"MemoryArrays count{ramAllocationResult.Count()}");

            Console.WriteLine($"Allocated memory size: {ramAllocationResult} MB");
        }
    }
}
