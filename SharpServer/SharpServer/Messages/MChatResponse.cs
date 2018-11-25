using System;

namespace SharpServer.Messages
{
    public class MChatResponse : MBasicResponse
    {
        public MChatPayload payload { get; set; }

        public MChatResponse()
            : base()
        {
            pid = MessageId.Text;
        }
    }
}
