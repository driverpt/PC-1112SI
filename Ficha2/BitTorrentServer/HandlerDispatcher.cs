using System;
using System.Collections.Generic;
using System.IO;
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
                                                  reader.Close();
                                                  Program.ShowInfo(Store.Instance);
                                                  return;
                                              }
                                              if (ctx == string.Empty)
                                              {
                                                  Task.Factory.StartNew(() => ProcessConnection(reader.BaseStream, log));
                                                  return;
                                              }
                                              cmd.ProcessCommand(ctx, log);
                                              Task.Factory.StartNew(() => HandleMessage(cmd, reader, log));
                                          }, reader);
        }

        private void Dispatch(string command, AsyncStreamReader reader, Logger log)
        {
            if( !Handlers.ContainsKey(command) )
            {
                Console.WriteLine("Invalid Operation");
                // TODO: Initialize MessageHandler. Reply in Stream. Wait for more Commands.
                throw new InvalidOperationException();
            }
            Console.WriteLine("Dispatching Command: {0}", command);
            IMessageHandler handler = Handlers[command];
            Task.Factory.StartNew(() => HandleMessage(handler, reader, log));
        }
    }
}