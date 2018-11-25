using System;

namespace SharpClient.Messages
{
    public class MChatMessage : MBasicMessage
    {
        public MChatPayload payload { get; set; }

        public MChatMessage()
            : base()
        {
            pid = MessageId.Text;
        }
    }
}
