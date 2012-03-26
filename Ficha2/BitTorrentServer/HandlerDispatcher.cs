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

            reader.BeginReadLine( log, ( result ) => { 
                                                         string cmd = reader.EndReadLine(result); 
                                                         Dispatch(cmd, reader, log);
            }, reader );
        }

        private void HandleMessage( IMessageHandler cmd, AsyncStreamReader reader, Logger log )
        {
            reader.BeginReadLine(log, (result) =>
                                          {
                                              string ctx = reader.EndReadLine(result);
                                              if (string.IsNullOrEmpty(ctx))
                                              {
                                                  Task.Factory.StartNew(() => ProcessConnection(reader.BaseStream, log));
                                              }
                                              cmd.ProcessCommand(ctx, log);
                                              Task.Factory.StartNew(() => HandleMessage(cmd, reader, log));
                                          }, reader);
        }

        private void Dispatch(string command, AsyncStreamReader reader, Logger log)
        {
            if( !Handlers.ContainsKey(command) )
            {
                // TODO: Initialize MessageHandler. Reply in Stream. Wait for more Commands.
                throw new InvalidOperationException();
            }
            IMessageHandler handler = Handlers[command];
            Task task = new Task(() => HandleMessage(handler, reader, log));
        }
    }
}