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

        public static ManualResetEvent tcpClientConnec = new ManualResetEvent(false);


        /// <param name="log"> The Logger instance to be used.</param>
        /// 
        private volatile bool shutdown = false;
        private Logger log;
        private TcpListener srv;
        public void Run( Logger log )
        {
            srv = null;
            this.log = log;
            try
            {
                srv = new TcpListener(IPAddress.Loopback, portNumber);
                srv.Start();
                while (!shutdown)
                {
                    tcpClientConnec.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    srv.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), srv);
                    tcpClientConnec.WaitOne();
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
        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            Console.WriteLine("DoAcceptTcpClientCallback");
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient socket = listener.EndAcceptTcpClient(ar);
            socket.LingerState = new LingerOption(true, 10);
            log.LogMessage(String.Format("Listener - Connection established with {0}.",
                socket.Client.RemoteEndPoint));
            // Instantiating protocol handler and associate it to the current TCP connection
            Handler protocolHandler = new Handler(socket.GetStream(), log);
            tcpClientConnec.Set();
            // Synchronously process requests made through de current TCP connection
            Task.Factory.StartNew((handler) => ((Handler)handler).Run(), protocolHandler);
            //protocolHandler.Run();
            Program.ShowInfo(Store.Instance);
        }

        public void Stop()
        {
            shutdown = true;
        }

    }
}