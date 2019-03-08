using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MoneyWeb.RequestServer.Interface
{
    public interface IHttpRequestHandle
    {
        void HandleHttpRequest(HttpApplication HttpApp);
    }
}
