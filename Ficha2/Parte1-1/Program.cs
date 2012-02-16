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

    }
}
