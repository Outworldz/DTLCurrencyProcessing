using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.HttpHandler.Interface;
using System.Web;
using System.Windows.Forms;
using System.Collections;
using OpenMetaverse;
using System.Data;
using System.Reflection;
using MoneyWeb.HttpHandler.Base;
using MoneyWeb.FrameWork.Events;
using MoneyWeb.XmlRpcHanlder;
using MoneyWeb.FrameWork.Assembler;
using MoneyWeb.Data.Interface;

namespace MoneyWeb.HttpHandler
{
    public class HttpRequestFactory :HttpRequestBase,IHttpFactory
    {
        public HttpRequestFactory()
            : base()
        {
        }

        #region IHttpEvent Members

        public void HttpRequestHandler(object request)
        {
            HttpApplication httpApp = request as HttpApplication;
            HttpContext httpcontext = httpApp.Context;
            HttpRequest httpReq = httpApp.Request;
            HttpResponse httpResp = httpApp.Response;
            httpAppPath = httpApp.Server.MapPath("~/bin/");
   

            string repQuery = httpReq.Form.Get("method").ToString();

            if (String.IsNullOrEmpty(repQuery))
                return;
             
            AssemblyParamsEvent assemblyhanlder = Assemblyconfig.GetAssemblyParamsByName(repQuery);


            RequestAssemblyEvent requestevent = new RequestAssemblyEvent(httpcontext, this.HttpRequestAssembly, assemblyhanlder);

            requestevent.InvokeRequestAssembly();

            httpResp.Clear();
            httpResp.ContentType = "text/xml";
           
            httpResp.Charset = "utf-8";
            httpResp.Write(requestevent.XmlResp);
            httpResp.End();
            
            
        }

        public object HttpRequestAssembly(string method, AssemblyParamsEvent assemblyevent,object[] args)
        {
            Object ret = new Object();

            Assembly asm = Assembly.LoadFile(httpAppPath + assemblyevent.AssemblyPath);

            Type type = asm.GetType(assemblyevent.AssemblyType, true);

            Object obj = Activator.CreateInstance(type, new object[] { httpAppPath }, null);

            MethodInfo md = type.GetMethod(assemblyevent.RequestMethod);

            ret = md.Invoke(obj, args);

            return ret;
        }

        #endregion
    }
}
