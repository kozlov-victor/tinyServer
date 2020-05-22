using System;
using System.Collections.Generic;
using System.Text;

namespace tinyServer.controller
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    class ControllerAttribute:Attribute
    {
        public string Url;
    }
}
