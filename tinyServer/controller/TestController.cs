using System;
using System.Collections.Generic;
using System.Text;
using tinyServer.request;
using tinyServer.response;

namespace tinyServer.controller
{
    class TestController
    {

        [ControllerAttribute(Url = "/test")]
        public void Test(Request req, Response resp)
        {
            req.QueryParamas.TryGetValue("name", out string name);
            resp.WriteJSON(new Dictionary<string, object> {
                { "response", $"hello, {name}" }
            });
        }

    }
}
