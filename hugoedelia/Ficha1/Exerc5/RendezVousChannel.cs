using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SyncUtils;

namespace Exerc5
{
    class RendezVousChannel<S, R>
    {
        private readonly LinkedList<Token> _toDoList = new LinkedList<Token>();
        private readonly LinkedList<object> _servers = new LinkedList<object>();

        internal class Token
        {
            public S _request;
            public R _response;
            public bool done;

            public Token(S request)
            {
                _request = request;
                done = false;
            }
        }

        public object AcceptService(int timeout, out S request)
        {
            lock (this)
            {
                int lastime = (timeout != Timeout.Infinite) ? Environment.TickCount : 0;

                Token toDo = null;

                Object serverThread = new object();
                //add to servers list
                _servers.AddLast(serverThread);

                do
                {
                    try
                    {
                        if (_toDoList.Count == 0)
                        {
                            SyncUtils.SyncUtils.Wait(this, serverThread, timeout);
                        }
                        else
                        {
                            toDo = _toDoList.First();

                            //remove from service list
                            _toDoList.RemoveFirst();

                            request = toDo._request;
                            //remove from server list
                            _servers.Remove(serverThread);

                            return toDo;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        //remove from servers list
                        _servers.Remove(serverThread);

                        if (toDo == null)
                        {
                            //server job was not completed
                            throw;
                        }

                        //server job was completed
                        request = toDo._request;

                        _toDoList.Remove(toDo);

                        Thread.CurrentThread.Interrupt();

                        return toDo;
                    }

                    if (SyncUtils.SyncUtils.AdjustTimeout(ref lastime, ref timeout) == 0)
                    {
                        request = default(S);
                        //remove for servers list
                        _servers.Remove(serverThread);
                        return null;
                    }
                } while (true);
            }
        }

        public void CompleteService(object token, R response)
        {
            lock (this)
            {
                var toDo = token as Token;
                //set the response
                toDo._response = response;
                //set flag
                toDo.done = true;
                //notify the client
                SyncUtils.SyncUtils.Notify(this, toDo);
            }

        }

        public bool RequestService(S request, int timeout, out R response)
        {

            lock (this)
            {
                int lastime = (timeout != Timeout.Infinite) ? Environment.TickCount : 0;
                Token myToken = new Token(request);
                _toDoList.AddLast(myToken);
                //notify only one server thread
                if (_servers.Count > 0)
                {
                    SyncUtils.SyncUtils.Notify(this, _servers.First);
                }

                do
                {
                    try
                    {
                        if (!myToken.done)
                        {
                            //wait for service done or timeout
                            SyncUtils.SyncUtils.Wait(this, myToken, timeout);
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        _toDoList.Remove(myToken);
                        if (myToken.done)
                        {
                            //Service is complete
                            Console.WriteLine("Interrupted completed");
                            response = myToken._response;
                            Thread.CurrentThread.Interrupt();
                            return true;
                        }
                        //service is not complete, it's a give up
                        Console.WriteLine("Interrupted Uncompleted");

                        throw;
                    }

                    //if service is done
                    if (myToken.done)
                    {
                        _toDoList.Remove(myToken);
                        response = myToken._response;
                        return true;
                    }

                    if (SyncUtils.SyncUtils.AdjustTimeout(ref lastime, ref timeout) == 0)
                    {
                        _toDoList.Remove(myToken);
                        response = default(R);
                        return false;
                    }
                } while (true);

            }
        }
    }
}
