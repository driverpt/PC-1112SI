using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parte1_1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestParallelLoop();
            Console.ReadKey();
        }

        public static void TestParallelLoop()
        {
            var options = new ParallelOptions
            {
                CancellationToken = new CancellationToken(),
                MaxDegreeOfParallelism = 4,
                TaskScheduler = TaskScheduler.Default
            };
            ParallelLoopResult result = Program.For(0, 100, options, (arr) => Console.Write(arr + "-"));
            Console.WriteLine(result.IsCompleted);

        }

        public static ParallelLoopResult For( int start, int end, ParallelOptions options, Action<int> body )
        {
            // Following the HAPPY PATH :)
            // Problem : If the division result is a decimal number
            var segment = ( ( end - start ) / options.MaxDegreeOfParallelism );
            var totalSplits = ( end - start ) / segment;
            int[] nums = Enumerable.Range(0, totalSplits).ToArray();
            
            return Parallel.ForEach(nums,options, i2 => Task.Factory.StartNew(() =>
                                                                                {
                                                                                    int i1 = i2;
                                                                                    Console.WriteLine("Start -> {0}",start +(i1*segment));
                                                                                    Console.WriteLine("End   -> {0}",start + segment +(i1*segment));
                                                                                    for (int j = start + (i1*segment);j < start + segment + (i1*segment);++j)
                                                                                    {
                                                                                        body(j);
                                                                                    }
                                                                                },
                                                                            TaskCreationOptions.AttachedToParent));
        }
      }
    }
