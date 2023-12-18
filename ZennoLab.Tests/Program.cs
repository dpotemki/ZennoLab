using BenchmarkDotNet.Running;

namespace ZennoLab.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ResourceAllocatorBenchmark>();

            Console.WriteLine("Hello, World!");
        }
    }
}
