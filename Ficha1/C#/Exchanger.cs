using System.Collections.Generic;
using System.Threading;

namespace CSharp
{
    public class Exchanger< T >
    {
        private readonly object _monitor = new object();
        private Request _request;

        public bool Exchange( T mine, int timeout, out T yours )
        {
            lock( _monitor )
            {
                if( _request != default( Request ) )
                {
                    yours        = _request.mine;
                    _request.his = mine;
                    _request     = default( Request );
                    Monitor.PulseAll( _monitor );
                }

                Request request = new Request();
                request.mine = mine;
                _request = request;
                
                while( true )
                {
                    if ( !Monitor.Wait( _monitor, timeout ) )
                    {
                        if( request.his != null )
                        {
                            yours = request.his;
                            return true;
                        }
                    }
                    _request.mine = mine;
                }
            }
        }

        private class Request
        {
            public T mine;
            public T his;
        }
    }
}