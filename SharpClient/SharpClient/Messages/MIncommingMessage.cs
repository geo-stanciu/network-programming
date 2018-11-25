using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SharpClient.Messages
{
    class MIncommingMessage : MBasicResponse
    {
        public JObject payload { get; set; }
        public object content { get; set; } = null;

        public static MIncommingMessage Parse(string message)
        {
            var result = JsonConvert.DeserializeObject<MIncommingMessage>(message);

            if (result.payload == null)
                return result;

            switch (result.pid)
            {
                case MessageId.Login:
                    result.content = result.payload.ToObject<MLoginPayload>();
                    break;

                case MessageId.Text:
                    result.content = result.payload.ToObject<MChatPayload>();
                    break;

                default:
                    throw new Exception($"Unknown message ID {result.pid}.");
            }

            return result;
        }
    }
}
