using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BitTorrentServer
{
    public interface IMessageHandler
    {
        void ProcessCommand(StreamReader input, StreamWriter output, Logger log);
    }

    public abstract class MessageHandler : IMessageHandler
    {

        public string MessageHandlerName { get; private set; }

        protected MessageHandler(string name)
        {
            MessageHandlerName = name;
        }

        public static void RegisterMessageHandler( IMessageHandler handler )
        {
            
        }

        public static void GetMessageHandler( string name )
        {
            
        }

        public void ProcessCommand(StreamReader input, StreamWriter output, Logger log)
        {
        }

        protected abstract void ProcessMessage(string message, Logger log);
    }

    public class RegisterMessageHandler : MessageHandler
    {
        public RegisterMessageHandler(string name)
            : base(name)
        {
        }

        public void PreProcessCommand(StreamReader input, StreamWriter output, Logger log)
        {

        }

        protected override void ProcessMessage(string message, Logger log)
        {
            // Read message payload, terminated by an empty line. 
            // Each payload line has the following format
            // <filename>:<ipAddress>:<portNumber>
            string[] triple = message.Split(':');
            if (triple.Length != 3)
            {
                log.LogMessage("Handler - Invalid REGISTER message.");
                return;
            }
            IPAddress ipAddress = IPAddress.Parse(triple[1]);
            ushort port;
            if (!ushort.TryParse(triple[2], out port))
            {
                log.LogMessage("Handler - Invalid REGISTER message.");
                return;
            }
            Store.Instance.Register(triple[0], new IPEndPoint(ipAddress, port));
        }

        // This request message does not have a corresponding response message, hence, 
        // nothing is sent to the client.
    }

    /// <summary>
    /// Handles client requests.
    /// </summary>
    public sealed class Handler
    {
        #region Message handlers

        /// <summary>
        /// Data structure that supports message processing dispatch.
        /// </summary>
        private static readonly Dictionary<string, Action<StreamReader, StreamWriter, Logger>> MESSAGE_HANDLERS;

        static Handler()
        {
            MESSAGE_HANDLERS = new Dictionary<string, Action<StreamReader, StreamWriter, Logger>>();
            MESSAGE_HANDLERS["REGISTER"] = ProcessRegisterMessage;
            MESSAGE_HANDLERS["UNREGISTER"] = ProcessUnregisterMessage;
            MESSAGE_HANDLERS["LIST_FILES"] = ProcessListFilesMessage;
            MESSAGE_HANDLERS["LIST_LOCATIONS"] = ProcessListLocationsMessage;
        }

        /// <summary>
        /// Handles REGISTER messages.
        /// </summary>
        private static void ProcessRegisterMessage(StreamReader input, StreamWriter output, Logger log)
        {
            // Read message payload, terminated by an empty line. 
            // Each payload line has the following format
            // <filename>:<ipAddress>:<portNumber>
            string line;

            while ((line = input.ReadLine()) != null && line != string.Empty)
            {
                string[] triple = line.Split(':');
                if (triple.Length != 3)
                {
                    log.LogMessage("Handler - Invalid REGISTER message.");
                    return;
                }
                IPAddress ipAddress = IPAddress.Parse(triple[1]);
                ushort port;
                if (!ushort.TryParse(triple[2], out port))
                {
                    log.LogMessage("Handler - Invalid REGISTER message.");
                    return;
                }
                Store.Instance.Register(triple[0], new IPEndPoint(ipAddress, port));
            }

            // This request message does not have a corresponding response message, hence, 
            // nothing is sent to the client.
        }

        /// <summary>
        /// Handles UNREGISTER messages.
        /// </summary>
        private static void ProcessUnregisterMessage(StreamReader input, StreamWriter output, Logger log)
        {
            // Read message payload, terminated by an empty line. 
            // Each payload line has the following format
            // <filename>:<ipAddress>:<portNumber>
            string line;
            while ((line = input.ReadLine()) != null && line != string.Empty)
            {
                string[] triple = line.Split(':');
                if (triple.Length != 3)
                {
                    log.LogMessage("Handler - Invalid UNREGISTER message.");
                    return;
                }
                IPAddress ipAddress = IPAddress.Parse(triple[1]);
                ushort port;
                if (!ushort.TryParse(triple[2], out port))
                {
                    log.LogMessage("Handler - Invalid UNREGISTER message.");
                    return;
                }
                Store.Instance.Unregister(triple[0], new IPEndPoint(ipAddress, port));
            }

            // This request message does not have a corresponding response message, hence, 
            // nothing is sent to the client.
        }

        /// <summary>
        /// Handles LIST_FILES messages.
        /// </summary>
        private static void ProcessListFilesMessage(StreamReader input, StreamWriter output, Logger log)
        {
            // Request message does not have a payload.
            // Read end message mark (empty line)
            input.ReadLine();

            string[] trackedFiles = Store.Instance.GetTrackedFiles();

            // Send response message. 
            // The message is composed of multiple lines and is terminated by an empty one.
            // Each line contains a name of a tracked file.
            foreach (string file in trackedFiles)
                output.WriteLine(file);

            // End response and flush it.
            output.WriteLine();
            output.Flush();
        }

        /// <summary>
        /// Handles LIST_LOCATIONS messages.
        /// </summary>
        private static void ProcessListLocationsMessage(StreamReader input, StreamWriter output, Logger log)
        {
            // Request message payload is composed of a single line containing the file name.
            // The end of the message's payload is marked with an empty line
            string line = input.ReadLine();
            input.ReadLine();

            IPEndPoint[] fileLocations = Store.Instance.GetFileLocations(line);

            // Send response message. 
            // The message is composed of multiple lines and is terminated by an empty one.
            // Each line has the following format
            // <ipAddress>:<portNumber>
            foreach (IPEndPoint endpoint in fileLocations)
                output.WriteLine(string.Format("{0}:{1}", endpoint.Address, endpoint.Port));

            // End response and flush it.
            output.WriteLine();
            output.Flush();
        }

        #endregion

        /// <summary>
        /// The handler's input (from the TCP connection)
        /// </summary>
        private readonly StreamReader input;

        /// <summary>
        /// The handler's output (to the TCP connection)
        /// </summary>
        private readonly StreamWriter output;

        /// <summary>
        /// The Logger instance to be used.
        /// </summary>
        private readonly Logger log;

        /// <summary>
        ///	Initiates an instance with the given parameters.
        /// </summary>
        /// <param name="connection">The TCP connection to be used.</param>
        /// <param name="log">the Logger instance to be used.</param>
        public Handler(Stream connection, Logger log)
        {
            this.log = log;
            output = new StreamWriter(connection);
            input = new StreamReader(connection);
        }


        /// <summary>
        /// Performs request servicing.
        /// </summary>
        public void Run()
        {
            //try
            //{
            string requestType;
            var reader = new AsyncStreamReader(input);
            // Read request type (the request's first line)
            var result = reader.BeginReadLine(log, null, null);
            var token = new CancellationTokenSource();
            //while ( ( requestType = input.ReadLine() ) != null && requestType != string.Empty )
            //{

            Task.Factory.StartNew(() =>
                                      {
                                          var line = reader.EndReadLine(result);
                                          if (string.IsNullOrEmpty(line))
                                          {
                                              token.Cancel();
                                          }
                                          return line;
                                      }, token.Token).ContinueWith((prev) =>
                                                                       {
                                                                           if (!string.IsNullOrEmpty(prev.Result))
                                                                           {
                                                                               return
                                                                                   MESSAGE_HANDLERS[
                                                                                       prev.Result.ToUpper()];
                                                                           }
                                                                           return null;
                                                                       }, token.Token).ContinueWith((prev) =>
                                                                                                    prev
                                                                                                        .Result(input,
                                                                                                                output,
                                                                                                                log)
                )
                .ContinueWith((prev) => Run(), token.Token);
            //reader.BeginReadLine(log, (result) =>
            //                              {

            //                              }, null);
            //requestType = requestType.ToUpper();
            //if ( !MESSAGE_HANDLERS.ContainsKey( requestType ) )
            //{
            //    log.LogMessage( "Handler - Unknown message type. Servicing ending." );
            //    return;
            //}
            //// Dispatch request processing
            //MESSAGE_HANDLERS[requestType]( input, output, log );
            //    }
            //}
            //catch ( IOException ioe )
            //{
            //    // Connection closed by the client. Log it!
            //    log.LogMessage( String.Format( "Handler - Connection closed by client {0}", ioe ) );
            //}
            //finally
            //{
            //    input.Close();
            //    output.Close();
            //}
        }

        public void ReadCommand(IAsyncResult result)
        {
            var state = result.AsyncState as AsyncStreamReader;
            if (state == null)
            {
                throw new InvalidOperationException();
            }
            var requestType = state.EndReadLine(result);

        }

        public void ReadNextLine()
        {

        }
    }

    internal class AsyncStreamReaderResult : IAsyncResult
    {
        private string result;
        private Exception exception;
        private readonly object monitor = new object();
        private readonly ManualResetEvent handle;

        public AsyncStreamReaderResult(AsyncCallback cb, object asyncstate)
        {
            handle = new ManualResetEvent(false);
            Callback = cb;
            AsyncState = asyncstate;
        }

        public bool IsCompleted { get; private set; }
        public AsyncCallback Callback { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { return handle; }
        }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public string GetResult(int timeout)
        {
            if (!handle.WaitOne(timeout))
            {
                throw new ThreadInterruptedException();
            }
            return result;
        }

        public void SetResult(string t)
        {
            lock (monitor)
            {
                if (IsCompleted)
                {
                    throw new InvalidOperationException();
                }
                result = t;
                IsCompleted = true;
                handle.Set();
            }
        }

        public void SetException(Exception t)
        {
            lock (monitor)
            {
                if (IsCompleted)
                {
                    throw new InvalidOperationException();
                }
                exception = t;
                IsCompleted = true;
                handle.Set();
            }
        }
    }

    public class AsyncStreamReader : TextReader
    {
        private const int BUFFER_SIZE = 4096;
        private readonly StreamReader _reader;

        public AsyncStreamReader(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        public AsyncStreamReader(StreamReader reader)
        {
            _reader = reader;
        }

        public IAsyncResult BeginReadLine(Logger log, AsyncCallback cb, object state)
        {
            var result = new AsyncStreamReaderResult(cb, state);
            log.LogMessage("Beggining Executing ReadLine in Alt Thread");
            ThreadPool.QueueUserWorkItem(o =>
                                             {
                                                 try
                                                 {
                                                     result.SetResult(ReadLine());
                                                     log.LogMessage("Line Read");
                                                 }
                                                 catch (Exception exception)
                                                 {
                                                     log.LogMessage("Exception occured");
                                                     result.SetException(exception);
                                                 }
                                                 finally
                                                 {
                                                     cb(result);
                                                 }
                                             });
            return result;
        }

        public new string ReadLine()
        {
            return _reader.ReadLine();
        }

        public string EndReadLine(IAsyncResult result)
        {
            return EndReadLine(result, Timeout.Infinite);
        }

        public string EndReadLine(IAsyncResult result, int timeout)
        {
            var res = result as AsyncStreamReaderResult;
            if (res == null)
            {
                throw new InvalidOperationException("Invalid Result Object");
            }
            return res.GetResult(timeout);
        }

        public void Shutdown()
        {
            _reader.Close();
        }
    }
}