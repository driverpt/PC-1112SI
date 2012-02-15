using System;

namespace Parte1_4
{
    class Program
    {
        public static int NumberOfTasks { get; set; }

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
            //var source2 = 20.To( 30 );
            //foreach ( string i in p1.Run( source2 ) )
            //{
            //    Console.Write( i );
            //    Console.Write( " " );
            //}
            //Console.WriteLine();
            Console.WriteLine("{0} Tasks Ran", NumberOfTasks );
        }

        public static void Main( string[] args )
        {
            Test();
            Console.ReadKey();
        }
    }
}
