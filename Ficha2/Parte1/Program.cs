using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parte1
{
    class Program
    {
        public static ParallelLoopResult For( int start, int end, ParallelOptions options, Action<int> body )
        {
            // Following the HAPPY PATH :)
            // Problem : If the division result is a decimal number
            var segment = ( ( end - start ) / options.MaxDegreeOfParallelism );
            var totalSplits = ( end - start ) / segment;

            Task task = new Task( () =>
            {
                for ( int i = 0 ; i < totalSplits ; ++i )
                {
                    int i2 = i;
                    Task.Factory.StartNew( () =>
                                               {
                                                   int i1 = i2;
                                                   Console.WriteLine( "Start -> {0}", start + ( i1 * segment ) );
                                                   Console.WriteLine( "End   -> {0}", start + segment + ( i1 * segment ) );
                                                   for ( int j = start + ( i1 * segment ) ; j < start + segment + ( i1 * segment ) ; ++j )
                                                   {
                                                       body( j );
                                                   }
                                               },
                                           TaskCreationOptions.AttachedToParent );
                }
            }
            );
            task.Start( options.TaskScheduler );
            task.Wait( options.CancellationToken );

            return Parallel.For(0, 0, (x) => Console.WriteLine( "Completed" ) );
        }

        public static int NumberOfTasks { get; set; }

        public static void Alinea2()
        {
            Task t1 = Task.Factory.StartNew( () =>
            {
                Task.Factory.StartNew( () =>
                {
                    throw new Exception( "Child exception!" );
                }, TaskCreationOptions.AttachedToParent );
                throw new Exception( "Parent exception!" );
            } );
            ( ( IAsyncResult ) t1 ).AsyncWaitHandle.WaitOne();
            t1 = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.ReadLine();            
        }

        public static void Test()
        {
            // sequência de inteiros de 1 a 10
            NumberOfTasks = 0;
            var source = 1.To( 10 );
            var p1 = new Pipeline<int, int>( i => i * 3 )
                .Next( i => i / 2 )
                .Next( i => ( i % 2 == 0 ) ? "Par" : "Impar" );
            foreach ( string i in p1.Run( source ) )
            {
                Console.Write( i );
                Console.Write( " " );
            }
            Console.WriteLine();
            var source2 = 20.To( 30 );
            foreach ( string i in p1.Run( source2 ) )
            {
                Console.Write( i );
                Console.Write( " " );
            }
            Console.WriteLine();
            Console.WriteLine("{0} Tasks Ran", NumberOfTasks );
        }

        public static void TestParallelLoop()
        {
            var options = new ParallelOptions
                              {
                                  CancellationToken = new CancellationToken(),
                                  MaxDegreeOfParallelism = 4,
                                  TaskScheduler = TaskScheduler.Default
                              };
            ParallelLoopResult result = Program.For(0, 100, options, (arr) => Console.Write(arr + "-") );
            Console.WriteLine( result.IsCompleted );
            
        }

        public static void RandomInt(int number)
        {
            number = 5;
        }

        public static void Main( string[] args )
        {
            //Test();
            TestParallelLoop();
            Console.ReadKey();
        }
    }
}
