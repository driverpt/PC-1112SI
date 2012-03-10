using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        /// <summary> Initiates a tracking server instance.</summary>
        /// <param name="_portNumber"> The TCP port number to be used.</param>
        public Listener( int _portNumber ) { portNumber = _portNumber; }

        public static ManualResetEvent _tcpClientConnec = new ManualResetEvent(false);

        private volatile bool _shutdown;
        private Logger _log;
        private TcpListener _srv;

        /// <param name="log"> The Logger instance to be used.</param>
        public void Run( Logger log )
        {
            _srv = null;
            _log = log;
            try
            {
                _srv = new TcpListener(IPAddress.Loopback, portNumber);
                _srv.Start();
                while (!_shutdown)
                {
                    _tcpClientConnec.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    _srv.BeginAcceptTcpClient((DoAcceptTcpClientCallback), _srv);
                    _tcpClientConnec.WaitOne();
                }
            }
            finally
            {
                _log.LogMessage( "Listener - Ending." );
                if ( _srv != null )
                {
                    _srv.Stop();    
                }
            }
        }
        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            Console.WriteLine("DoAcceptTcpClientCallback");
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient socket = listener.EndAcceptTcpClient(ar);
            socket.LingerState = new LingerOption(true, 10);
            _log.LogMessage(String.Format("Listener - Connection established with {0}.",
                socket.Client.RemoteEndPoint));
            // Instantiating protocol handler and associate it to the current TCP connection
            Handler protocolHandler = new Handler(socket.GetStream(), _log);
            _tcpClientConnec.Set();
            // Synchronously process requests made through de current TCP connection
            Task.Factory.StartNew(handler => ((Handler)handler).Run(), protocolHandler);
            //protocolHandler.Run();
            Program.ShowInfo(Store.Instance);
        }

        public void Stop()
        {
            _shutdown = true;
        }

    }
}