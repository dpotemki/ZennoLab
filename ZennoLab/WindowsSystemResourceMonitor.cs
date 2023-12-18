namespace ZennoLab
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using ZennoLab.ResourceAllocator.Interfaces;

    public class WindowsSystemResourceMonitor : ISystemResourceMonitor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        public double GetTotalMemoryInMb()
        {
            GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);
            return totalMemoryInKilobytes / 1024.0 / 1024.0;
        }

        public double GetUsedMemoryInMb()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / 1024.0 / 1024.0;
            }
            return 0;
        }

        public double GetFreeMemoryInPercentage()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return (double)memStatus.ullAvailPhys / memStatus.ullTotalPhys * 100.0;
            }
            return 0;
        }

        public double GetTotalCpuUsage()
        {
            using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(1000); // Дать время для инициализации счетчика
                return cpuCounter.NextValue();
            }
        }
    }
}
