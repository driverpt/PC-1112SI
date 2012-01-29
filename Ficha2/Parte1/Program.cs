using System;
using System.Threading.Tasks;

namespace Parte1
{
    class Program
    {
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
            Console.WriteLine("{0} Tasks Ran", NumberOfTasks );
        }

        public static void Main( string[] args )
        {
            Test();
            Console.ReadKey();
        }
    }
}
