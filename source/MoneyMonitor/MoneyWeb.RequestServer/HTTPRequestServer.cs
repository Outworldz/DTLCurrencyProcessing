using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.RequestServer.Base;
using MoneyWeb.RequestServer.Interface;
using System.Web;
using MoneyWeb.HttpHandler;
using System.Windows.Forms;

namespace MoneyWeb.RequestServer
{
    public class HTTPRequestServer : HTTPRequestBase, IHttpRequestHandle
    {
       
        private static HttpRequestFactory ReqFactory = new HttpRequestFactory();

        public override void ContextError(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
        }

        public override void ContextEndRequest(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
        }

        public override void ContextBeginRequest(object sender, EventArgs e)
        {
            HttpApplication HttpApp = sender as HttpApplication;

            HandleHttpRequest(HttpApp);
        }

        #region IHttpRequestHandle Members

        public void HandleHttpRequest(System.Web.HttpApplication HttpApp)
        {
            switch (HttpApp.Request.ContentType)
            {
                case "application/x-www-form-urlencoded":
                case "application/x-www-form-urlencoded; charset=UTF-8":
                    ReqFactory.HttpRequestHandler(HttpApp);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
