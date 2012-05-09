using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exerc5
{
    class Program
    {
        static RendezVousChannel<int,int> rvp= new RendezVousChannel<int,int>();
        private static volatile bool shutdown;

        /*
         * 
         * Server Thread
         * 
         */

        private static void ServerThread(object otid)
        {
            int tid = (int) otid;

            Random r = new Random(tid);
            int count = 0;
            int timeouts = 0;

            Console.WriteLine("server {0} started...",tid);

            do
            {
                object token;
                int request;

                try
                {
                    while((token=rvp.AcceptService(r.Next(0,30),out request))==null)
                    {
                        timeouts++;
                        if(shutdown)
                        {
                            goto Exit;
                        }
                    }

                    //service time

                    Thread.Sleep(r.Next(0,15));

                    //complete request with response

                    rvp.CompleteService(token, request);

                    if((++count % 100) == 0 )
                    {
                        Console.Write("-s{0}",tid);
                    }

                }catch(ThreadInterruptedException)
                {
                    break;
                }
            } while (!shutdown);

            Exit:
            do
            {
                try
                {
                    Console.WriteLine("+++ server[{0}] exiting, {1} requests executed, {2} accepts timed out",
                                tid, count, timeouts);
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("EXCEPTION");
                }
            } while (true);

        }

        /*
        * 
        * Client Thread
        * 
        */
        private static void ClientThread(object otid)
        {
            int tid = (int) otid; 

            Random r = new Random(tid);

            int count = 0;
            int timeouts = 0;

            Console.WriteLine("\nClient {0} started",tid);

            do
            {
                int response;
                try
                {
                    while(!rvp.RequestService(tid,1000/*r.Next(0,300)*/,out response))
                    {
                        timeouts++;
                        if(shutdown)
                        {
                            goto Exit;
                        }
                    }
                    if(response !=tid)
                    {
                        Console.WriteLine("*** Failure: expected %d, received %d", tid, response);
                        goto Exit;
                    }
                    if((++count % 100)== 0)
                    {
                        Console.Write("-c{0}",tid);
                    }
                }catch(ThreadInterruptedException)
                {
                    break;
                }

            } while (!shutdown);

            Exit:
            do
            {
                try
                {
                    Console.WriteLine("+++ client[{0}] exiting, {1} requests accepted, {2} timed out",
                                tid, count, timeouts);
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("EXCEPTION");
                }
            } while (true);
        }

        const int SERVER_THREADS = 10;
        const int CLIENT_THREADS = 20;

        public static void Main()
        {
            Thread[] threads = new Thread[SERVER_THREADS + CLIENT_THREADS];

		    for (int i = 0; i < SERVER_THREADS; i++) {
			    threads[i] = new Thread(ServerThread);
			    threads[i].Start(i + 1);
		    }

		    for (int i = 0; i < CLIENT_THREADS; i++) {
			    threads[SERVER_THREADS + i] = new Thread(ClientThread);
			    threads[SERVER_THREADS + i].Start(i + 1);
		    }

		    Console.WriteLine("+++hit <enter> to terminate the test...");
		    Console.ReadLine();
		    shutdown = true;
		    for (int i = 0; i < SERVER_THREADS + CLIENT_THREADS; i++) {
			    //threads[i].Interrupt();
                if(i%3==0)
                threads[i].Interrupt();
			    threads[i].Join();
		    }

            Console.ReadLine();

	    }
            
        

    }
    }
