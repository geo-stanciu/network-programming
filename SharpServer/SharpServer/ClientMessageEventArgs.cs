using System;
namespace SharpServer
{
    public class ClientMessageEventArgs : EventArgs
    {
        public ConnectedEndPoint Client { get; set; } = null;
        public string Message { get; }

        public ClientMessageEventArgs(ConnectedEndPoint client, string message)
        {
            Client = client;
            Message = message;
        }
    }
}
