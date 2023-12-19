namespace ZennoLab
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.IO;
    using System.Diagnostics;
    using ZennoLab.ResourceAllocator.Models;
    using ZennoLab.ResourceAllocator.Interfaces;

    public class ResourceManager
    {
        private readonly IList<Project> _projects;
        private readonly int _maxGlobalThreads;
        private readonly ISystemResourceMonitor _systemResourceMonitor;
        private readonly string _pathToExecuredProdject;

        public ResourceManager(IList<Project> projects, int maxGlobalThreads, ISystemResourceMonitor systemResourceMonitor,string pathToExecuredProdject)
        {

            _projects = projects;
            _maxGlobalThreads = maxGlobalThreads;
            _systemResourceMonitor = systemResourceMonitor;
            _pathToExecuredProdject = pathToExecuredProdject;
        }

        public async Task ExecuteAllProjectsAsync(int maxExecuteTimeInSeconds, string pathForExecutedProject )
        {
            using var globalSemaphore = new SemaphoreSlim(_maxGlobalThreads);

            var tasks = new List<Task>();
            foreach (var project in _projects.Where(x=> x.CalculateCanCompleteWithinInterval(maxExecuteTimeInSeconds)))
            {
                tasks.Add(ExecuteProjectAsync(project, globalSemaphore));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ExecuteProjectAsync(Project project, SemaphoreSlim globalSemaphore)
        {
            using var projectSemaphore = new SemaphoreSlim(project.MaxThreads);

            var tasks = new List<Task>();
            for (int i = 0; i < project.TryCount; i++)
            {
                await globalSemaphore.WaitAsync();
                try
                {
                    while (_systemResourceMonitor.GetFreeMemoryInPercentage() < 5)
                    {
                        Thread.Sleep(100); // Delay to wait for resources to free up
                    }

                    await projectSemaphore.WaitAsync();
                    try
                    {
                        var task = StartConsoleAppAsync(project.MemoryCount, project.AppTimeout);
                        tasks.Add(task.ContinueWith(t => projectSemaphore.Release()));
                    }
                    finally
                    {
                        globalSemaphore.Release();
                    }
                }
                catch
                {
                    // Handle possible exception
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task StartConsoleAppAsync(int memoryCount, int appTimeout)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pathToExecuredProdject,
                    Arguments = $"{memoryCount} {appTimeout}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
