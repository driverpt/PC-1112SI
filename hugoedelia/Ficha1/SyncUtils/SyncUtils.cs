using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SyncUtils
{
    public class SyncUtils
    {
        private static void EnterUninterruptable(object mLock, out bool interrupted)
        {
            interrupted = false;
            do
            {
                try
                {
                    Monitor.Enter(mLock);
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    interrupted = true;
                }
            } while (true);
        }

        public static void Wait(object mLock, object condition, int timeout)
        {
            if (mLock == condition)
            {
                Monitor.Wait(mLock, timeout);
                return;
            }

            Monitor.Enter(condition);

            Monitor.Exit(mLock);

            //wait on condition monitor

            try
            {
                Monitor.Wait(condition, timeout);

            }
            finally
            {
                Monitor.Exit(condition);

                bool interrupted;

                EnterUninterruptable(mLock, out interrupted);

                if (interrupted)
                {
                    throw new ThreadInterruptedException();
                }
            }
        }

        public static void Notify(object mLock, object condition)
        {
            if (mLock == condition)
            {
                Monitor.Pulse(mLock);
                return;
            }

            bool interrupted;

            EnterUninterruptable(condition, out interrupted);

            Monitor.Pulse(condition);

            Monitor.Exit(condition);

            if (interrupted)
            {
                Thread.CurrentThread.Interrupt();
            }
        }

        public static void BroadCast(object mLock, object condition)
        {
            if (mLock == condition)
            {
                Monitor.PulseAll(mLock);
                return;
            }

            bool interrupted;

            EnterUninterruptable(condition, out interrupted);

            Monitor.PulseAll(condition);

            Monitor.Exit(condition);

            if (interrupted)
            {
                Thread.CurrentThread.Interrupt();
            }

        }

        public static int AdjustTimeout(ref int lastime, ref int timeout)
        {
            if (timeout != Timeout.Infinite)
            {
                int now = Environment.TickCount;
                int elapsed = (now == lastime) ? 1 : now - lastime;
                if (elapsed >= timeout)
                {
                    timeout = 0;
                }
                else
                {
                    timeout -= elapsed;
                    lastime = now;
                }
            }
            return timeout;
        }
    }
}
