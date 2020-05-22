using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using tinyServer.request;
using tinyServer.response;

namespace tinyServer.controller
{

    class MethodContextHolder
    {
        public MethodInfo MethodInfo;
        public object obj;
    }

    class ControllerRegistry
    {

        private Dictionary<string, MethodContextHolder> MethodMap = new Dictionary<string, MethodContextHolder>();


        public ControllerRegistry()
        {
            RegisterController(new TestController());
        }

        private void RegisterController(object controller) 
        {
            Type myType = controller.GetType();
            // Get the public methods.
            MethodInfo[] methodInfos = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methodInfos)
            {
                object[] attributes = method.GetCustomAttributes(typeof(ControllerAttribute), true);
                if (attributes.Length == 0) continue;
                ControllerAttribute attr = (ControllerAttribute)attributes[0];
                MethodContextHolder methodContextHolder = new MethodContextHolder
                {
                    MethodInfo = method,
                    obj = controller
                };
                MethodMap.Add(attr.Url, methodContextHolder);
            }
            
        }

        public bool IsUrlRegistered(string url)
        {
            return MethodMap.ContainsKey(url);
        }

        public Response CallMethodByUrl(Request request) 
        {
            MethodMap.TryGetValue(request.Url, out MethodContextHolder m);
            Response response = new Response();
            m.MethodInfo.Invoke(m.obj, new object[] { request, response });
            return response;
        }



    }
}
