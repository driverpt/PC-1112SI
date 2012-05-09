using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exerc4
{
    class Program
    {
        static Exchanger<int> xchg = new Exchanger<int>();
        static volatile bool shutdown;


        // exchanger thread.

        private static void ExchangerThread(object otid)
        {
            int tid = (int)otid;	// Thread.CurrentThread.ManagedThreadId;

            Random r = new Random(tid);
            int count = 0;
            int timeouts = 0;

            Console.WriteLine("+++ exchanger[{0}] started...", tid);

            do
            {
                int yourTid;
                try
                {
                    while (!xchg.Exchange(tid, /*1000*/r.Next(0, 19), out yourTid))
                    {
                        timeouts++;
                        if (shutdown)
                        {
                            goto Exit;
                        }
                    }
                    if (tid == yourTid)
                    {
                        Console.WriteLine("*** Exchange Failed!");
                        goto Exit;
                    }
                    if ((++count % 100) == 0)
                    {
                        Console.Write("-{0}", tid);
                    }
                    Thread.Sleep(r.Next(0, 10));
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            } while (!shutdown);
        Exit:
            do
            {
                try
                {
                    Console.WriteLine("+++ exchanger[{0}] exiting, {1} exchanges done, {2} timed out",
                                tid, count, timeouts);
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("EXCEPTION");
                }
            } while (true);
        }

        const int THREADS = 10;

        public static void Run()
        {
            Thread[] threads = new Thread[THREADS];
            for (int i = 0; i < THREADS; i++)
            {
                threads[i] = new Thread(ExchangerThread);
                threads[i].Start(i + 1);
            }
            Console.WriteLine("+++hit <enter> to terminate the test...");
            Console.ReadLine();
            shutdown = true;
            for (int i = 0; i < THREADS; i++)
            {
                threads[i].Interrupt();
                //threads[i].Join();
            }
        }
        static void Main(string[] args)
        {
            Run();

            Console.ReadLine();
        }
    }
}
