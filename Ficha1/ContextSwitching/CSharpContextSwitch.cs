using System;
using System.Threading;

namespace CSharp
{
    public class CSharpContextSwitch
    {
        public static volatile bool Flag;
        public static volatile bool JittingTuning;
        public static long SwitchCount;


        public static void yieldingThread()
        {
            while( Flag )
            {
                if( !JittingTuning )
                {
                    ++SwitchCount;
                }
                Thread.Yield();
            }
        }

        public static void Main( string[] args )
        {
            Flag = true;
            JittingTuning = true;
            Thread thread1 = new Thread( yieldingThread );
            Thread thread2 = new Thread( yieldingThread );
            thread1.Start();
            thread2.Start();

            Console.WriteLine( "Press Enter to Start" );
            Console.ReadLine();
            int startCount = Environment.TickCount;
            JittingTuning = false;
            
            Console.WriteLine( "Press Enter to Stop" );
            Console.ReadLine();

            int endcount = Environment.TickCount;

            Console.WriteLine( "Average Context Switching Time : {0}", ( endcount - startCount )/ SwitchCount );

            Console.WriteLine( "Press Enter to Exit" );
            Console.ReadLine();
        }
    }
}