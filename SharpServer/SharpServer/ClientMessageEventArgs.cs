using SharpServer.Messages;
using System;
namespace SharpServer
{
    public class ClientMessageEventArgs : EventArgs
    {
        public ConnectedEndPoint Client { get; }
        public MIncommingMessage Message { get; }

        public ClientMessageEventArgs(ConnectedEndPoint client, MIncommingMessage message)
        {
            Client = client;
            Message = message;
        }
    }
}
