using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.ResourceAllocator.Models;

namespace ZennoLab.Tests
{
    public class ResourceAllocatorBenchmark
    {


        [Benchmark]
        public async Task ExecuteProjectTest()
        {
            var projectsWrapper = JsonConvert.DeserializeObject<JsonWrapper>(StaticData.ProjectsList);

            ThreadPool.GetMaxThreads(out var workingThreads, out var completionPortThreads);
            var pathToCompiledZennoLab_Miner = "D:\\temp\\ZennoLab\\ZennoLab.Miner\\bin\\Release\\net8.0\\publish\\ZennoLab.Miner.exe";
            var manager = new ZennoLab.ResourceManager(projectsWrapper.Projects, workingThreads, new ZennoLab.WindowsSystemResourceMonitor(), pathToCompiledZennoLab_Miner);
            await manager.ExecuteAllProjectsAsync(120);
        }

    }
}
