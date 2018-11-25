using System;

namespace SharpClient.Messages
{
    public class MLoginCommand : MBasicMessage
    {
        public MLoginPayload payload { get; set; }

        public MLoginCommand()
            : base()
        {
            pid = MessageId.Login;
        }
    }
}
