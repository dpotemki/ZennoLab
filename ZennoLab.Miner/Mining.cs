using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennoLab.Miner
{
    public static class Mining
    { 
           public static bool ValidateArguments(string[] args, out int megabyteSize, out int appTimeout)
            {
                megabyteSize = 0;
                appTimeout = 0;

                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: ConsoleApp [MemoryCount in MB] [AppTimeout in seconds]");
                    return false;
                }

                megabyteSize = int.TryParse(args[0], out int mem) ? mem : 0;
                appTimeout = int.TryParse(args[1], out int timeout) ? timeout : 0;

                if (megabyteSize <= 0 || appTimeout <= 0)
                {
                    Console.WriteLine("Both MemoryCount and AppTimeout must be integer numbers greater than zero.");
                    return false;
                }
                return true;
            }
        public static double AllocateCPU()
            {
                Random random = new Random();

                double result = Math.Sqrt(random.NextDouble());
                return result;
            }

        public static byte[][] AllocateAndFillMemoryArrays(int megabyteSize)
            {
                long sizeInMegabyte = (long)megabyteSize * 1024 * 1024;

                // Определим максимальный размер одного массива (например, 1 ГБ)
                long maxArraySize = 1024 * 1024 * 1024; // 1 ГБ в байтах
                int arraysNeeded = (int)Math.Ceiling((double)sizeInMegabyte / maxArraySize);

                // Создаем массив массивов
                byte[][] memoryArrays = new byte[arraysNeeded][];
                Random random = new Random();

                for (int i = 0; i < arraysNeeded; i++)
                {
                    long currentArraySize = sizeInMegabyte - (i * maxArraySize);
                    if (currentArraySize > maxArraySize)
                    {
                        currentArraySize = maxArraySize;
                    }

                    memoryArrays[i] = new byte[currentArraySize];
                    random.NextBytes(memoryArrays[i]);
                }

                return memoryArrays;
            }
    }
}
