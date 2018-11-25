using System;

namespace SharpServer
{
    public class StatusEventArgs : EventArgs
    {
        public string StatusText { get; }

        public StatusEventArgs(string statusText)
        {
            StatusText = statusText;
        }
    }
}
