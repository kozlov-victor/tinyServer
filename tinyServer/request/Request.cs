using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using TinyServer;

namespace tinyServer.request
{
    class Request
    {

        public string Url;
        public Dictionary<string, string> QueryParamas;

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (query == "") return result;
            string[] pairs = query.Split("&");
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split("=");
                string key = keyValue[0];
                string value = keyValue.Length == 2 ? keyValue[1] : null;
                result.Add(key,value);
            }
            return result;
        }

        public static Request FromRaw(string requestString)
        {
            string[] requestUriParts = requestString.Split("?");
            string requestUri = requestUriParts[0];
            string requestParamsStr = requestUriParts.Length == 2 ? requestUriParts[1] : "";

            // Приводим ее к изначальному виду, преобразуя экранированные символы
            // Например, "%20" -> " "
            string uri = Uri.UnescapeDataString(requestUri);
            return new Request {
                Url = uri,
                QueryParamas = ParseQueryString(requestParamsStr)
            };
        }

    }
}
