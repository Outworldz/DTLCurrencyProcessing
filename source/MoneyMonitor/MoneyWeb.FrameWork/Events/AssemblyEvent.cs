using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Web;
using System.Collections;
using System.Windows.Forms;
using MoneyWeb.Data;
using MoneyWeb.FrameWork.Framework;

namespace MoneyWeb.FrameWork.Events
{
    public delegate Object OnAssemblyEvent(string method, AssemblyParamsEvent assemblyevent,Object[] args);

    public class RequestAssemblyEvent : EventArgs
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private OnAssemblyEvent _handler;

        private AssemblyParamsEvent _paramsevent;

        private string _method;

        private Object[] _args;

        private HttpRequest _request;

        private HttpContext _context;

        private string _xmlResp;

        public string XmlResp
        {
            get { return _xmlResp; }
            set { _xmlResp = value; }
        }

        public RequestAssemblyEvent(HttpContext httpcontext, OnAssemblyEvent requestassembly, AssemblyParamsEvent assemblyparams)
        {
            _context = httpcontext;
            _request = httpcontext.Request;
            _method = _request.Form.Get("method").ToString();
    
            _handler = requestassembly;
            _paramsevent = assemblyparams;
            
        }

        public void InvokeRequestAssembly()
        {
            Object _retobj = null;
            try
            {
         
              //_paramsevent.ParamsCheck(_request);
                switch (_method)
                {

                    default:
                        {
                            _args = _paramsevent.ParamsAssembly(_context);

                            if (_handler != null)
                            {
                                _retobj = _handler(_method, _paramsevent, _args);
                            }
                            _xmlResp = _retobj as string;
                        }
                        break;
                }

   
            }
            catch (Exception ex)
            {
                Hashtable RespData = new Hashtable();
                RespData["success"] = false;
                RespData["method"] = _method;
                RespData["error_type"] = "exception_error";
                RespData["message"] = "Failed To Invoke Method - "+ _method +" \n "+ex.Message.ToString();
               // _retobj = (Object)RespData;
                _xmlResp = AssemblerDictionary.AssemblerPlugins[_paramsevent.ResponseType].GenerateXml(_context, (Object)RespData);
            }
            

 

            //_xmlResp = _paramsevent.ResponseAssembly(_retobj, _request);
       

        }
    }
}
