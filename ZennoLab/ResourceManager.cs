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

        public ResourceManager(IList<Project> projects, int maxGlobalThreads, ISystemResourceMonitor systemResourceMonitor)
        {

            _projects = projects;
            _maxGlobalThreads = maxGlobalThreads;
            _systemResourceMonitor = systemResourceMonitor;
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
                    while (/*_systemResourceMonitor.GetTotalCpuUsage() > 95 || */_systemResourceMonitor.GetFreeMemoryInPercentage() < 5)
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
        
        //private static void LaunchProject(Project project)
        //{
        //    // Получаем общую потребляемую память всеми запущенными процессами.
        //    double totalMemory = 0;
        //    foreach (Process process in Process.GetProcesses())
        //    {
        //        totalMemory += process.PrivateMemorySize64;
        //    }

        //    // Если потребляемая память превышает доступное количество памяти,
        //    // останавливаем проект.
        //    if (totalMemory + project.MemoryCount > AvailableMemory)
        //    {
        //        return;
        //    }

        //    // Запускаем консольное приложение.
        //    Process.Start("myapp.exe", project.Args);
        //}

        public async Task StartConsoleAppAsync(int memoryCount, int appTimeout)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "D:\\temp\\ZennoLab\\ZennoLab.Miner\\bin\\Release\\net8.0\\publish\\ZennoLab.Miner.exe",
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
