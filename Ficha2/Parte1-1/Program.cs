using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parte1_1
{
    class Program
    {
        static void Main(string[] args)
        {

        }

        public static ParallelLoopResult For(int start, int end, ParallelOptions options ,Action<int> body)
        {
            int threadCount = Environment.ProcessorCount;
            int blockSize = (end - start + 1) / threadCount;
            Task[] tasks = new Task[threadCount];
            for(int count = 0; count < threadCount; ++count)
            {
                tasks[count] = new Task();
            }
            return new ParallelLoopResult();
        }
    }
}
