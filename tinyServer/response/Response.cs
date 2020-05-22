using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace tinyServer.response
{
    class Response
    {

        public Stream Stream { private set; get; } = new MemoryStream(Encoding.UTF8.GetBytes(""));
        public string ContentType { private set; get; } = "text/plain";

        public void WriteJSON(Dictionary<string,object> dict)
        {
            ContentType = "application/json";
            string json = JsonSerializer.Serialize(dict);
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        }

        public void WriteText(string text)
        {
            ContentType = "text/plain";
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

    }
}
