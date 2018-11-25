using Newtonsoft.Json;
using System;

namespace SharpServer.Messages
{
    public class MBasicResponse : MBasicMessage
    {
        public bool err { get; set; } = false;
        public string serr { get; set; } = "";

        public new string ToString()
        {
            var serializer = new JsonSerializerSettings();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None, serializer);
        }
    }
}
