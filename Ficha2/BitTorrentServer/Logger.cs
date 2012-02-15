/*
 * INSTITUTO SUPERIOR DE ENGENHARIA DE LISBOA
 * Licenciatura em Engenharia Informática e de Computadores
 *
 * Programação Concorrente - Inverno 2011-2012
 *
 * Código base para a 2ª Série de Exercícios.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BitTorrentServer
{
    // Logger single-threaded.
    public class Logger
    {
        private readonly TextWriter _writer;
        private DateTime _startTime;
        private volatile int _numRequests;
        private readonly BlockingCollection<string> _messagesToLog = new BlockingCollection<string>();
        private Thread _workerThread;

        private readonly object _monitor = new object();

        public Logger() : this( Console.Out ) { }
        public Logger( string logfile ) : this( new StreamWriter( new FileStream( logfile, FileMode.Append, FileAccess.Write ) ) ) { }
        public Logger( TextWriter awriter )
        {
            _numRequests = 0;
            _writer = awriter;
        }

        public void Start()
        {
            lock( _monitor )
            {
                if( _startTime != default(DateTime) )
                {
                    throw new InvalidOperationException("Logger alread started");
                }
                _startTime = DateTime.Now;
                LogMessage(String.Format("::- LOG STARTED @ {0} -::\n\n", DateTime.Now));
                _workerThread = new Thread(ProcessRequests) { Priority = ThreadPriority.Lowest };
            }
        }

        private void ProcessRequests()
        {
            foreach( var message in _messagesToLog )
            {
                _writer.WriteLine(message);
                IncrementRequests();
            }
            _writer.Close();
        }

        protected void LogMessageNewLine()
        {
            LogMessage("\n");
        }

        public void LogMessage( string msg )
        {
            lock( _messagesToLog )
            {
                _messagesToLog.Add(String.Format("{0}: {1}", DateTime.Now, msg));    
            }
        }

        public void IncrementRequests()
        {
            ++_numRequests;
        }

        public void Stop()
        {
            lock( _monitor )
            {
                long elapsed = DateTime.Now.Ticks - _startTime.Ticks;
                _writer.WriteLine();
                LogMessageNewLine();
                LogMessage(String.Format("Running for {0} second(s)", elapsed / 10000000L));
                LogMessage(String.Format("Number of request(s): {0}", _numRequests));
                LogMessageNewLine();
                LogMessage(String.Format("::- LOG STOPPED @ {0} -::", DateTime.Now));
                _messagesToLog.CompleteAdding();
//                _writer.Close();                
            }
        }

        private class MessageLog
        {
            public string txt { get; private set; }
            public MessageLog( string msg )
            {
                txt = msg;
            }
        }

    }
}
