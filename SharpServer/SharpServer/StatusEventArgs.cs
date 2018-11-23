using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
