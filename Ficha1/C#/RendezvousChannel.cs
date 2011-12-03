using System;
using System.Collections.Generic;
using System.Threading;

namespace CSharp
{
    public class RendezvousChannel<S, R>
    {
        private readonly LinkedList<RendezVousToken<S, R>> _queue = new LinkedList<RendezVousToken<S, R>>();
        private readonly object _monitor = new object();
        private int _serverThreads = 0;

        public bool Request(S service, int timeout, out R response)
        {
            lock ( _monitor )
            {
                RendezVousToken<S,R> token = new RendezVousToken<S, R>();
                token.service = service;
                LinkedListNode<RendezVousToken<S, R>> node = _queue.AddLast(token);
                try
                {
                    if( _serverThreads == 0 )
                    {
                        int currentTime = Environment.TickCount;
                        do
                        {
                            SyncUtils.Wait(_monitor, token, timeout);
                            if( token.response != null )
                            {
                                response = token.response;
                                return true;
                            }
                            if( SyncUtils.AjustTimeOut(ref currentTime, ref timeout) == 0 )
                            {
                                _queue.Remove(node);
                                response = default(R);
                                return false;
                            }
                        } while (true);
                    }

                }
                catch( ThreadInterruptedException )
                {
                    _queue.Remove(node);
                    response = default(R);
                    return false;
                }
            }


            response = default(R);
            return false;
        }

        public object Accept( int timeout, out S service )
        {
            lock( _monitor )
            {
                ++_serverThreads;
                if ( _queue.Count == 0 )
                {
                    int currentTime = Environment.TickCount;
                    do
                    {
                        SyncUtils.Wait(_monitor, _monitor, timeout);
                        if( _queue.Count != 0 )
                        {
                            break;
                        }
                        if( SyncUtils.AjustTimeOut(ref currentTime, ref timeout) == 0 )
                        {
                            service = default(S);
                            return null;
                        }
                    } while ( true );
                }
                LinkedListNode<RendezVousToken<S, R>> node;
                node = _queue.First;
                _queue.RemoveFirst();
                service = node.Value.service;
                --_serverThreads;
                return node.Value;
            }
        }

        private void Reply( object rendezVousToken, R response )
        {
            lock( _monitor )
            {
                RendezVousToken<S, R> token = rendezVousToken as RendezVousToken<S, R>;
                if ( token == null )
                {
                    response = default(R);
                    return;
                }
                token.response = response;
                SyncUtils.BroadCast( _monitor, token );
            }
        }

        private class RendezVousToken<S,R>
        {
            public S service;
            public R response;
        }

    }
}