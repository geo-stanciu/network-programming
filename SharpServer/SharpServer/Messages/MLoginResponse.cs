using System;

namespace SharpServer.Messages
{
    class MLoginResponse : MBasicResponse
    {
        public MLoginResponse()
            : base()
        {
            pid = MessageId.Login;
        }
    }
}
