using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exerc4
{
    internal class Program
    {
        private static readonly Exchanger<String> exch = new Exchanger<string>();
        private static string[] arr2 = new string[4];
        private static string[] arr = {"1", "2", "3", "4"};
        private static int j = 0;

        private static void Main()
        {
            Thread[] thread = new Thread[4];
            ThreadStart job = new ThreadStart(ThreadJob);
            int s = 2;
            for (int i = 0; i < 4; i++)
            {
                thread[i] = new Thread(job);
            }

            for (int i = 0; i < 4; i++)
            {
                thread[i].Start();
                j++;
            }

            Console.WriteLine("Enter to terminate");
            Console.ReadLine();
        }

        public static void ThreadJob()
        {
            exch.Exchange(arr[j], Timeout.Infinite, out arr2[j]);
        }
    }
}
