using System;
using System.Threading;

namespace CSharp
{
    public static class SyncUtils
    {
        public static int AjustTimeOut( ref int lastTime, ref int timeout )
        {
            int currentTime = Environment.TickCount;
            int elapsedTime = currentTime - lastTime;

            if ( elapsedTime < timeout )
            {
                return timeout - elapsedTime;
            }
            return 0;
        }

        private static void EnterUninterruptibility( object mlock, out bool interrupted )
        {
            interrupted = false;
            do
            {
                try
                {
                    Monitor.Enter(mlock);
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    interrupted = true;
                }
            } while (true);
        }

        public static void BroadCast( object mlock, object condition )
        {
            if (mlock == condition)
            {
                Monitor.PulseAll(mlock);
                return;
            }
            bool interrupted;
            EnterUninterruptibility( condition, out interrupted );
            Monitor.PulseAll( condition );
            Monitor.Exit( condition );
            if ( interrupted )
            {
                Thread.CurrentThread.Interrupt();
            }
        }

        public static void Wait( object mlock, object condition, int timeout )
        {
            if( mlock == condition )
            {
                Monitor.Wait(mlock, timeout);
                return;
            }
            Monitor.Enter( condition );
            Monitor.Exit( mlock );
            try
            {
                Monitor.Wait(condition, timeout);
            }
            finally 
            {
                Monitor.Exit( condition );
                bool interrupted;
                EnterUninterruptibility( mlock, out interrupted );
                if( interrupted )
                {
                    throw new ThreadInterruptedException();
                }
            }
        }

        public static void Wait( object mlock, object condition )
        {
            Wait( mlock, condition, Timeout.Infinite );
        }

        public static void Notify( object mlock, object condition )
        {
            if ( mlock == condition )
            {
                Monitor.Pulse( mlock );
                return;
            }
            bool interrupted;
            EnterUninterruptibility( condition, out interrupted );
            Monitor.Pulse(condition);
            Monitor.Exit(condition);
            if( interrupted )
            {
                Thread.CurrentThread.Interrupt();
            }
        }

    }
}