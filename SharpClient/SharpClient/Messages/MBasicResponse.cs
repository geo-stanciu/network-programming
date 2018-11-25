using System;

namespace SharpClient.Messages
{
    public class MBasicResponse : MBasicMessage
    {
        public bool err { get; set; } = false;
        public string serr { get; set; } = "";
    }
}
