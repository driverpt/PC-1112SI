using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BitTorrentServer
{
    /// <summary>
    /// This class instances are file tracking servers. They are responsible for accepting 
    /// and managing established TCP connections.
    /// </summary>
    public sealed class Listener
    {
        /// <summary>
        /// TCP port number in use.
        /// </summary>
        private readonly int portNumber;

        private readonly HandlerDispatcher _dispatcher;

        /// <summary> Initiates a tracking server instance.</summary>
        /// <param name="_portNumber"> The TCP port number to be used.</param>
        public Listener( int _portNumber, HandlerDispatcher dispatcher )
        {
            portNumber = _portNumber;
            _dispatcher = dispatcher;
        }

        /// <summary>
        ///	Server's main loop implementation.
        /// </summary>
        /// <param name="log"> The Logger instance to be used.</param>
        public void Run( Logger log )
        {
            TcpListener srv = null;
            try
            {
                srv = new TcpListener( IPAddress.Loopback, portNumber );
                srv.Start();
                while ( true )
                {
                    log.LogMessage( "Listener - Waiting for connection requests." );
                    using ( TcpClient socket = srv.AcceptTcpClient() )
                    {
                        socket.LingerState = new LingerOption( true, 10 );
                        log.LogMessage( String.Format( "Listener - Connection established with {0}.",
                                                       socket.Client.RemoteEndPoint ) );
                        // Instantiating protocol handler and associate it to the current TCP connection
                        _dispatcher.ProcessConnection(socket.GetStream(), log);
                        //Handler protocolHandler = new Handler( socket.GetStream(), log );
                        // Synchronously process requests made through de current TCP connection
                        //Task.Factory.StartNew((handler) => ((Handler) handler).Run(), protocolHandler);
                        //protocolHandler.Run();
                    }

                    Program.ShowInfo( Store.Instance );
                }
            }
            finally
            {
                log.LogMessage( "Listener - Ending." );
                if ( srv != null )
                {
                    srv.Stop();    
                }
            }
        }
    }
}