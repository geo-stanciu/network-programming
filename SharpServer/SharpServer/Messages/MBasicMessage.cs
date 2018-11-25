using Newtonsoft.Json;
using System;

namespace SharpServer.Messages
{
    public class MBasicMessage
    {
        public MessageId pid { get; set; }
        public string user { get; set; }
        public string sid { get; set; }

        public new string ToString()
        {
            var serializer = new JsonSerializerSettings();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None, serializer);
        }
    }
}
