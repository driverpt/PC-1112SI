using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exerc4
{
    internal class Exchanger<T>
        //throws TimeoutException,ThreadInterruptedException
    {
        //Explicit Lock Object
        private Object _lock;
        //First blocking thread change object
        private ExchangeObject exch;

        class ExchangeObject
        {
            public T _messageOffered;
        }

        public Exchanger()
        {
            _lock = new Object();
            exch = new ExchangeObject();
        }


        public bool Exchange(T mine, int timeout, out T yours)
        {
            //if timeout is less or equals to zero returns false
            if (timeout <= 0)
            {
                yours = mine;
                return false;
            }
            lock (_lock)
            {
                ExchangeObject myexch = exch;
                if (exch._messageOffered != null)
                {
                    //DO Exchange and return
                    Console.WriteLine("ThreadID#{0} - Exchange", Thread.CurrentThread.ManagedThreadId);
                    yours = exch._messageOffered;
                    exch._messageOffered = mine;
                    Monitor.Pulse(_lock);
                    exch = new ExchangeObject();
                    return true;
                }
                else if (exch._messageOffered == null)
                {
                    {
                        //WAIT until other thread arrive
                        try
                        {
                            Console.WriteLine("ThreadID#{0} - waiting to exchange", Thread.CurrentThread.ManagedThreadId);
                            double timestamp = Environment.TickCount;
                            myexch._messageOffered = mine;
                            while (true)
                            {
                                Monitor.Wait(_lock, timeout);
                                //Monitor.Wait(_lock);
                                double elapsedTime = Environment.TickCount - timestamp;
                                if (elapsedTime >= timeout)
                                {
                                    throw new TimeoutException();
                                }
                                if (myexch._messageOffered != null && myexch._messageOffered.GetHashCode()!=mine.GetHashCode())
                                {
                                    //Take the messageOffered by second thread to block to this Exchanger
                                    yours = myexch._messageOffered;
                                    //cleans all flags
                                    Monitor.Pulse(_lock);
                                    if(myexch==exch)
                                    exch = new ExchangeObject();
                                    return true;
                                }
                            }
                        }
                        catch (TimeoutException e)
                        {
                            Console.WriteLine("ThreadID#{0} Inner Timeout, bye bye",
                                              Thread.CurrentThread.ManagedThreadId);
                            if (myexch._messageOffered != null && myexch._messageOffered.GetHashCode() != mine.GetHashCode())
                            {
                                yours = exch._messageOffered;
                                Monitor.Pulse(_lock);
                            }
                            //Altought was timed-out, if occurred exchange, it still consumes it and throws exception
                            if (myexch == exch)
                            exch = new ExchangeObject();
                            throw e;
                            //Treat Exception
                        }
                        catch (ThreadInterruptedException e)
                        {
                            Console.WriteLine("!! ThreadID#{0} Inner interrupted, bye bye",
                                              Thread.CurrentThread.ManagedThreadId);
                            if (myexch._messageOffered != null && !myexch._messageOffered.Equals(mine))
                            {
                                yours = myexch._messageOffered;
                                Monitor.Pulse(_lock);
                            }
                            // Thread.CurrentThread.Interrupt();
                            if (myexch == exch)
                            exch = new ExchangeObject();
                            throw e;
                        }

                    }

                }
                Console.WriteLine("!! ThreadID#{0} - Quiting, because exchanging is occuring",
                Thread.CurrentThread.ManagedThreadId);
                yours = mine;
                exch = new ExchangeObject();
                return false;

            }
        }
    }
}
