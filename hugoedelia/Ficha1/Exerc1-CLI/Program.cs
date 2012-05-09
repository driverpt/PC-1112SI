using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exerc1_CLI
{
    class Program
    {
        volatile static bool Running = true;
        volatile static bool Started = false;
        static int TcCount = 0;

        static void Main()
        {
            Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)1;
            ThreadStart job = new ThreadStart(ThreadJob);
            Thread[] thread = new Thread[2];


            Console.WriteLine("Enter to start");

            for (int i = 0; i < 2; i++)
            {
                thread[i] = new Thread(job);
                thread[i].Start();
            }

            Console.ReadLine();
            int start = Environment.TickCount;
            Started = true;


            Console.WriteLine("Enter to terminate");

            Console.ReadLine();
            int stop = Environment.TickCount;
            Running = false;


            double d = (stop - start) / (double)TcCount;
            Console.WriteLine("Tempo de comutacao {0} Tcs", d);

            Console.ReadLine();




        }

        static void ThreadJob()
        {
            while (Running)
            {
                if (Started)
                    TcCount++;

                Thread.Yield();

            }
        }
    }
}
