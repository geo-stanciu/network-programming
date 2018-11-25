using System;

namespace SharpServer.Messages
{
    public class MBasicMessage
    {
        public int pid { get; set; } = -1;
        public string user { get; set; } = "";
        public string sid { get; set; } = "";
        public string payload { get; set; } = "";
    }
}
