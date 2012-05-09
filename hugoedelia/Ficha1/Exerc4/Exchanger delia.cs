using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SyncUtils;


namespace Exerc4
{
    class Exchanger<T> : IExchanger<T>
    {
        class ExchangeObject
        {
            public T firstObject;
            public T secondObject;
        }

        private ExchangeObject _toExchange = null;

        public bool Exchange(T mine, int timeout, out T your)
        {
            lock (this)
            {

                int lastime = (timeout != Timeout.Infinite) ? Environment.TickCount : 0;

                if (_toExchange == null)
                {
                    ExchangeObject myExchanger = _toExchange = new ExchangeObject();
                    _toExchange.firstObject = mine;
                    do
                    {
                        try
                        {
                            Monitor.Wait(this, timeout);
                        }
                        catch (ThreadInterruptedException)
                        {
                            if (myExchanger.secondObject.Equals(default(T)))
                            {
                                throw;
                            }
                            your = myExchanger.secondObject;
                            Thread.CurrentThread.Interrupt();
                            return true;
                        }

                        if (SyncUtils.SyncUtils.AdjustTimeout(ref lastime, ref timeout) == 0)
                        {
                            Console.WriteLine("TimeOUT!! on thread -> {0}", Thread.CurrentThread.ManagedThreadId);

                            your = default(T);
                            return false;
                        }

                        if (myExchanger.secondObject != null)
                        {
                            your = myExchanger.secondObject;
                            return true;
                        }
                    } while (true);
                }
                else
                {
                    _toExchange.secondObject = mine;
                    your = _toExchange.firstObject;
                    _toExchange = null;
                    Monitor.Pulse(this);
                    return true;
                }
            }
        }
    }
}
