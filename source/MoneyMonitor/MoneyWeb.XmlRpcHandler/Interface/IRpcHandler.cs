using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;

namespace MoneyWeb.XmlRpcHandler.Interface
{
    public interface IRpcHandler
    {
        string XmlRpcResponse(HttpContext context,string seruri, string resptype, Hashtable reqParams);

        string XmlRpcTransaction(HttpContext context, string seruri, string resptype, int number,int currentPage, Hashtable reqParms);

        string OnLoadVerify(HttpContext context, string resptype);

        string LoadcurrentPage(HttpContext context, string seruri, string resptype, int currentPage, string sessionID);

        string LogoffPage(System.Web.HttpContext context, string resptype);


    }
}
