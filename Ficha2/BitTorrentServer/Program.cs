/*
 * INSTITUTO SUPERIOR DE ENGENHARIA DE LISBOA
 * Licenciatura em Engenharia Informática e de Computadores
 *
 * Programação Concorrente - Inverno 2011-2012
 *
 * Código base para a 2ª Série de Exercícios.
 */

using System;
using System.Net;

namespace BitTorrentServer
{
    class Program
    {
        public static void ShowInfo( Store store )
        {
            foreach ( string fileName in store.GetTrackedFiles() )
            {
                Console.WriteLine( fileName );
                foreach ( IPEndPoint endPoint in store.GetFileLocations( fileName ) )
                {
                    Console.Write( endPoint + " ; " );
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /*
                static void TestStore()
                {
                    Store store = Store.Instance;

                    store.Register("xpto", new IPEndPoint(IPAddress.Parse("193.1.2.3"), 1111));
                    store.Register("xpto", new IPEndPoint(IPAddress.Parse("194.1.2.3"), 1111));
                    store.Register("xpto", new IPEndPoint(IPAddress.Parse("195.1.2.3"), 1111));
                    ShowInfo(store);
                    Console.ReadLine();
                    store.Register("ypto", new IPEndPoint(IPAddress.Parse("193.1.2.3"), 1111));
                    store.Register("ypto", new IPEndPoint(IPAddress.Parse("194.1.2.3"), 1111));
                    ShowInfo(store);
                    Console.ReadLine();
                    store.Unregister("xpto", new IPEndPoint(IPAddress.Parse("195.1.2.3"), 1111));
                    ShowInfo(store);
                    Console.ReadLine();

                    store.Unregister("xpto", new IPEndPoint(IPAddress.Parse("193.1.2.3"), 1111));
                    store.Unregister("xpto", new IPEndPoint(IPAddress.Parse("194.1.2.3"), 1111));
                    ShowInfo(store);
                    Console.ReadLine();
                }
        */


        /// <summary>
        ///	Application's starting point. Starts a tracking server that listens at the TCP port 
        ///	specified as a command line argument.
        /// </summary>
        public static void Main( string[] args )
        {
            // Checking command line arguments
            if ( args.Length != 1 )
            {
                Console.WriteLine( "Utilização: {0} <numeroPortoTCP>", AppDomain.CurrentDomain.FriendlyName );
                Environment.Exit( 1 );
            }

            ushort port;
            if ( !ushort.TryParse( args[0], out port ) )
            {
                Console.WriteLine( "Usage: {0} <TCPPortNumber>", AppDomain.CurrentDomain.FriendlyName );
                return;
            }

            // Start servicing
            Logger log = new Logger();
            log.Start();
            try
            {
                new Listener( port ).Run( log );
                Console.ReadLine();
            }
            finally
            {
                log.Stop();
            }
        }
    }
}
