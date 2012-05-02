using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentServer
{
    public class HandlerDispatcher
    {
        public Dictionary<string, IMessageHandler> Handlers { get; private set; }
        public HandlerDispatcher()
        {
            Handlers = new Dictionary<string, IMessageHandler>();
        }

        public void RegisterMessageHandler(string name, IMessageHandler handler)
        {
            Handlers.Add(name, handler);
        }

        public void ProcessConnection(Stream stream, Logger log)
        {
            AsyncStreamReader reader = new AsyncStreamReader(stream);
            log.LogMessage("Processing Connection");
            reader.BeginReadLine( log, ( result ) => {
                                                         string cmd = reader.EndReadLine(result);
                                                         if ( string.IsNullOrEmpty(cmd))
                                                         {
                                                             return;
                                                         }
                                                         log.LogMessage(String.Format("Line Read => {0}", cmd));
                                                         Dispatch(cmd, reader, log);
            }, reader );
        }

        private void HandleMessage( IMessageHandler cmd, AsyncStreamReader reader, Logger log )
        {
            reader.BeginReadLine(log, (result) =>
                                          {
                                              var ctx = reader.EndReadLine(result);
                                              if (ctx == null)
                                              {
                                                  Console.WriteLine("Null String");
                                                  reader.Close();
                                                  Program.ShowInfo(Store.Instance);
                                                  return;
                                              }
                                              if (ctx == string.Empty)
                                              {
                                                  Console.WriteLine("Empty String");
                                                  reader.Close();
                                                  Task.Factory.StartNew(() => ProcessConnection(reader.BaseStream, log));
                                                  return;
                                              }
                                              Task.Factory.StartNew(() => HandlePayload(cmd, reader, log, null));
                                          }, reader);
        }

        private void HandlePayload( IMessageHandler cmd, AsyncStreamReader reader, Logger log, StringBuilder buffer )
        {
            if ( buffer == null )
            {
                buffer = new StringBuilder();
            }
            reader.BeginReadLine(log, (result) =>
                                          {
                                              var payload = reader.EndReadLine(result);
                                              if( string.IsNullOrEmpty(payload))
                                              {
                                                  if ( buffer.Length != 0 )
                                                  {
                                                      // TODO : Commands that dont require lines read, for example LIST_FILES and LIST_LOCATION, it wont run
                                                      Task.Factory.StartNew(() =>
                                                                                {
                                                                                    StreamWriter writer = new StreamWriter(reader.BaseStream);
                                                                                    foreach (string response in buffer.ToString().Split('\n').Select(line => cmd.ProcessCommand(line, log)).Where(response => !string.IsNullOrEmpty(response)))
                                                                                    {
                                                                                        writer.WriteLine(response);
                                                                                    }
                                                                                    writer.Close();
                                                                                });
                                                  }
                                                  reader.Close();
                                                  Task.Factory.StartNew( () => ProcessConnection( reader.BaseStream, log ) );
                                                  return;
                                              }
                                              buffer.Append(buffer);
                                              Task.Factory.StartNew(() => HandlePayload(cmd, reader, log, buffer));
                                          }, reader);
        }

        private void Dispatch(string command, AsyncStreamReader reader, Logger log)
        {
            Console.WriteLine("Dispatching Command: {0}", command);
            if( !Handlers.ContainsKey(command) )
            {
                StreamWriter writer = new StreamWriter(reader.BaseStream);
                writer.WriteLine( "Invalid Operation" );
                writer.Flush();
                writer.Close();
                Console.WriteLine( "Invalid Operation" );
                // TODO: Initialize MessageHandler. Reply in Stream. Wait for more Commands.
                throw new InvalidOperationException();
            }
            Console.WriteLine("Dispatching Command: {0}", command);
            IMessageHandler handler = Handlers[command];
            Task.Factory.StartNew(() => HandleMessage(handler, reader, log));
        }
    }
}