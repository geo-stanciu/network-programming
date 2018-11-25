using System;

namespace SharpServer.Messages
{
    class MLoginResponse : MBasicResponse
    {
        public string sid { get; set; }

        public MLoginResponse()
            : base()
        {
            pid = MessageId.Login;
        }
    }
}
