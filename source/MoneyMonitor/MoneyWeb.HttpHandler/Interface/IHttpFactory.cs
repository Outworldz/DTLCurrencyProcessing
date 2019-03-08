using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.FrameWork.Events;

namespace MoneyWeb.HttpHandler.Interface
{
    public interface IHttpFactory
    {
        void HttpRequestHandler(Object request);

        Object HttpRequestAssembly(string method,AssemblyParamsEvent assemblyevent, object[] args);
    }
}
