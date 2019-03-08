using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.XmlRpcHandler.Interface;
using System.Collections;
using Nwc.XmlRpc;
using System.Windows.Forms;
using MoneyWeb.Data;
using System.Web;
using MoneyWeb.FrameWork.Assembler;
using MoneyWeb.Data.Interface;
using MoneyWeb.FrameWork.Framework;

namespace MoneyWeb.XmlRpcHandler
{
    public class XmlRpcFactory : IRpcHandler
    {
        private const int _REQUEST_TIMEOUT = 30000;

        private const int MAX_TRANSACTION_NUM = 10;

        #region Constructor Memebers

        public XmlRpcFactory(string serpath) 
        {
            if (AssemblerDictionary.AssemblerPlugins.Count == 0)
            {
                InitAssemblerDictionary(serpath);
            }
        }

        #endregion

        #region private Members
        private void InitAssemblerDictionary(string serpath)
        {
            List<string> assembers = AssemblerLoader.ListAssembler(serpath);
            AssemblerDictionary.AssemblerPlugins = new Dictionary<string, IXmlAssembler>();
            foreach (string file in assembers)
            {
                //MessageBox.Show(file);
                foreach (IXmlAssembler obj in AssemblerLoader.ResponseAssemblerList(file))
                {
                    AssemblerDictionary.AssemblerPlugins.Add(obj.AssemblerType, obj);
                }
                break;
            }
        }

        private string getDataTime(int time)
        {
            long ticksToUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            DateTime dateTransfer = new DateTime(ticksToUnixEpoch + 10000000L * (long)time);
            string m_dateTime = dateTransfer.ToString();
            return m_dateTime;
        }
        private string getStatus(int type)
        {
            string strtype = string.Empty;
            switch (type)
            {
                case 0:
                    strtype =  "Success";
                    break;
                case 1:
                    strtype = "Pending";
                    break;
                default:
                    strtype = "Failed";
                    break;
            }
            return strtype;
        }

        private string getNavigationhtml(int currentPage, int number)
        {
            string html = string.Empty;
            html += "<div class=breadcrumbs><ul>";

            if (currentPage == 1)
            {
                html += "<li>First Page";
            }
            else
            {
                html += "<li><a href='javascript:LoadcurrentPage(1)'>First Page</a>";
                html += "<li><a href=\"javascript:LoadcurrentPage(" + (currentPage - 1).ToString() + ")\">" + "Previous Page" + "</a>";
            }
            for (int ix = 1; ix <= (number-1) / 10 + 1; ix++)
            {
                if (ix == currentPage)
                {
                    html += "<li>" + ix.ToString();
                }
                else
                {
                    html += "<li><a href=\"javascript:LoadcurrentPage(" + ix.ToString() + ")\">" + ix.ToString() + "</a>";
                     
                }
            }
            if (currentPage != (number-1) / 10 + 1)
            {
                html += "<li><a href=\"javascript:LoadcurrentPage(" + (currentPage + 1).ToString() + ")\">" + "Next Page" + "</a>";
                html += "<li><a href=\"javascript:LoadcurrentPage(" + (number / 10 + 1).ToString() + ")\">" + "Last Page" + "</a></li></ul></div>";

            }
            else
            {
                html += "<li>Last Page</a></li></ul></div>";
            }
           

            return html;
            
        }

        #endregion

        #region IRpcHandler Members

        public string XmlRpcTransaction(HttpContext context, string seruri, string resptype, int number, int currentPage, Hashtable reqParms)
        {
            ArrayList arrayParm = new ArrayList();
            HttpRequest request = context.Request;
           // arrayParm.Add(reqParms);
            XmlRpcResponse serResp = null;
            string respxml = string.Empty;
            Hashtable RespData = new Hashtable();
            Hashtable XmlData = new Hashtable();
            Hashtable Cache = new Hashtable();
            string sessionID = reqParms["sessionID"] as string;
            string method = string.Empty;
            int startindex = 1;
            if (currentPage == 1)
            {
                method = request.Form.Get("method").ToString();
                startindex = 1;
            }
            else
            {
                method = "WebGetTransaction";
                Cache = WebFrameWork.GetCurrentCache(sessionID);
                startindex = (currentPage / 10) * 10 + 1;
            }
            string html = string.Empty;
           
 
            WebFrameWork.ReleaseCache(sessionID);

            int maxnumber = number - (currentPage - 1) * 10;
            int maxpage = maxnumber >= 100 ? 10 : (number-1) / 10 + 1;

          //  MessageBox.Show(startindex.ToString() + "    /" + maxpage.ToString());

            for (int i = startindex; i <= maxpage; i++)
            {
                html = string.Empty;
                html += "<br/><br/><br/><h1>User Transaction Records (" + number.ToString() + ")</h1>";
                html += "<table id ='dtable' align='left' style='border: 1px solid #dfdfdf; left : 0px; top:0px; width: 100% ;height:100%' >";
                html += "<thead><tr style='vertical-align:middle;'><td class='top'>Index</td><td class='top' >transaction UUID</td><td class='top'>sender Name</td><td class='top'>receiver Name</td><td class='top'>amount</td><td class='top'>type</td><td class='top'>time</td><td class='top'>status</td><td class='top'>description</td></tr></thead>";
                html += "<tbody>";
                for (int ix = 0; ix < MAX_TRANSACTION_NUM; ix++)
                {
                    try
                    {
                
                        arrayParm = new ArrayList();
                        //MessageBox.Show("mehtod:   " + method);
                        //MessageBox.Show("https   " + seruri);
                        if(ix==0 && (int)reqParms["lastIndex"] == -1)
                            reqParms["lastIndex"] = Convert.ToInt32(reqParms["lastIndex"]) + 0;
                        else
                            reqParms["lastIndex"] = Convert.ToInt32(reqParms["lastIndex"]) + 1;
                        //foreach (DictionaryEntry de in reqParms)
                        //{
                        //    MessageBox.Show(de.Key + "  " + de.Value);
                        //}
                        arrayParm.Add(reqParms);
                        XmlRpcRequest serReq = new XmlRpcRequest(method, arrayParm);
                        serResp = serReq.Send(seruri, _REQUEST_TIMEOUT);
           

                    }
                    catch (Exception ex)
                    {
                        XmlData["success"] = false;
                        XmlData["method"] = method;
                        XmlData["message"] = ex.Message.ToString();
                        XmlData["seruri"] = seruri;
                       
                        break;
                    }
                    if (serResp.IsFault)
                    {
                        XmlData["success"] = false;
                        XmlData["method"] = method;
                        XmlData["message"] = "Failed to Connet server: " + seruri.ToString();
                        XmlData["seruri"] = seruri;
                        break;
                    }
                    else
                    {
              
                        RespData = (Hashtable)serResp.Value;
        
                        html += "<tr>";
                        html += "<td class='rowstate'>" + RespData["transactionIndex"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + RespData["transactionUUID"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + RespData["senderName"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + RespData["receiverName"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + RespData["amount"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + RespData["type"].ToString() + "</td>";
                        html += "<td class='rowstate'>" + getDataTime((int)RespData["time"]) + "</td>";
                        html += "<td class='rowstate'>" + getStatus((int)RespData["status"]) + "</td>";
                        html += "<td class='rowstate'>" + RespData["description"].ToString() + "</td>";
                        html += "</tr>";
                        
                        if ((bool)RespData["isEnd"] || ix==MAX_TRANSACTION_NUM-1 ||Convert.ToInt32(reqParms["lastIndex"])==(number-2) )
                        {
                            //XmlData["method"] = method;
                            XmlData["success"] = true;
                            //MessageBox.Show(reqParms["lastIndex"].ToString() + "/" + number.ToString()+"-"+i.ToString());
                            //if (i > 10)
                            //    MessageBox.Show(((i - 1) * 10 + ix).ToString()+"/"+(number-1).ToString());
                            //XmlData["seruri"] = seruri;
                            break;
                        }

                    }

                }
                html += "</tbody></table>";
            

                html += getNavigationhtml(i, number);

                      
                if ((bool)XmlData["success"])
                {
                    Cache[i] = html;
              
                }
            }
            Cache["startTime"] = Convert.ToInt32(reqParms["startTime"]);
            Cache["endTime"] = Convert.ToInt32(reqParms["endTime"]);
            Cache["number"] = number;
            Cache["lastIndex"] = Convert.ToInt32(reqParms["lastIndex"]);
        
            WebFrameWork.RegisterCache(sessionID, (object)Cache);
            XmlData.Clear();
            XmlData = WebFrameWork.TransactionHash(currentPage, sessionID, method, seruri);
            //XmlData[
            respxml = AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(context, XmlData);
          //  MessageBox.Show(respxml);

            return respxml;

        }

        public string LoadcurrentPage(HttpContext context, string seruri, string resptype, int currentPage, string sessionID)
        {
            HttpRequest request = context.Request;
            // arrayParm.Add(reqParms);
           // MessageBox.Show("hi");

            string method = request.Form.Get("method").ToString();
            Hashtable xmlData = new Hashtable();
            string respxml = string.Empty;
            xmlData = WebFrameWork.TransactionHash(currentPage, sessionID, method, seruri);
            if (String.IsNullOrEmpty(xmlData["html"].ToString()))
            {
                int number = Convert.ToInt32(xmlData["number"]);
              //  MessageBox.Show(currentPage.ToString(0 + "/" + number.ToString()));
    
                Hashtable reqParms = new Hashtable();
                reqParms["userID"] = context.Request.Cookies["AdminInfo"]["userID"].ToString();
                reqParms["sessionID"] = sessionID;
                reqParms["startTime"] = Convert.ToInt32(xmlData["startTime"]);
                reqParms["endTime"] = Convert.ToInt32(xmlData["endTime"]);
                reqParms["lastIndex"] = Convert.ToInt32(xmlData["lastIndex"]);
                //foreach (DictionaryEntry de in reqParms)
                //{
                //    MessageBox.Show(de.Key + "  " + de.Value);
                //}

                respxml = XmlRpcTransaction(context, seruri, resptype, number, currentPage, reqParms);
   
            }
            else
            {
                respxml = AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(context, xmlData);
            }
            
            return respxml;
 
        }
        public string XmlRpcResponse(HttpContext context, string seruri, string resptype, System.Collections.Hashtable reqParams)
        {
            ArrayList arrayParm = new ArrayList();
            HttpRequest request = context.Request;
            arrayParm.Add(reqParams);
            XmlRpcResponse serResp = null;
            Hashtable RespData = new Hashtable();
            string method = request.Form.Get("method").ToString();
  
            string respxml = string.Empty;
            //MessageBox.Show(seruri);
            //MessageBox.Show(method);
            //foreach (DictionaryEntry de in reqParams)
            //{
            //    MessageBox.Show(de.Value + "  " + de.Key);
            //}
           
            try
            {
                XmlRpcRequest serReq = new XmlRpcRequest(method, arrayParm);
                serResp = serReq.Send(seruri, _REQUEST_TIMEOUT);

            }
            catch (Exception ex)
            {
                RespData["success"] = false;
                RespData["method"] = method;
                RespData["message"] = ex.Message.ToString();
                RespData["seruri"] = seruri;
            }
            if (serResp.IsFault)
            {
                RespData["success"] = false;
                RespData["method"] = method;
                RespData["message"] = "Failed to Connet server: " + seruri.ToString();
                RespData["seruri"] = seruri;
            }
            else if (method.Equals("get_user_by_name"))
            {
                //MessageBox.Show(request.Form.Get("verifynumber").ToUpper() + "           " + request.Cookies["CheckCode"].Value.ToString());
                if (String.IsNullOrEmpty(request.Form.Get("verifynumber").ToString()))
                {
                    RespData["success"] = false;
                    RespData["method"] = method;
                    RespData["message"] = "Verify number is empty!";
                    RespData["seruri"] = seruri;
                    RespData["error_type"] = "verify_error";
                }
                else
                {
                    if (request.Cookies["CheckCode"] == null)
                    {
                        RespData["success"] = false;
                        RespData["method"] = method;
                        RespData["message"] = "Your webbrowser must forbid the cookies!";
                        RespData["seruri"] = seruri;
                        RespData["error_type"] = "verify_error";
                    }
                    else if (request.Form.Get("verifynumber").ToUpper() != request.Cookies["CheckCode"].Value.ToString())
                    {
                        RespData["success"] = false;
                        RespData["method"] = method;
                        RespData["message"] = "Verify number is error!";
                        RespData["seruri"] = seruri;
                        RespData["error_type"] = "verify_error";
                    }
                    else
                    {
                        RespData = (Hashtable)serResp.Value;
                        if (RespData.ContainsKey("error_type"))
                        {
                            RespData["success"] = false;
                            RespData["message"] =  RespData["error_desc"];
                        }
                        else
                        {
                            RespData["success"] = true;
                            RespData["username"] = request.Form.Get("avatar_name").ToString();
                            RespData["userID"] = (string)RespData["uuid"] + "@" + request.Form.Get("seruri").ToString();//"23cc97ee-6fa1-46cc-83bd-80fdafc1255a" + "@" + "127.0.0.1";//
                            RespData["moneyserver"] = request.Form.Get("seruris").ToString();
                           

                            //WebFrameWork.ReleaseSession();
                           // WebFrameWork.ReleaseCookieUserInfo();
        
                           // WebFrameWork.RegisterCookieUserInfo(request, (string)RespData["uuid"]);
                            
                        }
                        RespData["method"] = method;
                        RespData["seruri"] = seruri;
                    }
                }
            }
            else
            {
                RespData = (Hashtable)serResp.Value;
                if (method.Equals("WebLogin"))
                {
                    WebFrameWork.ReleaseCookieUserInfo();
                    WebFrameWork.RegisterCookieUserInfo(request, reqParams["sessionID"].ToString());
                    RespData["username"] = request.Form.Get("userName").ToString();
                    RespData["userserver"] = request.Form.Get("userID").ToString().Split('@')[1].ToString();
                    RespData["moneyserver"] = request.Form.Get("seruris").ToString();
                    WebFrameWork.RegisterCookieServerInfo(request.Form.Get("userID").ToString().Split('@')[1].ToString(), request.Form.Get("seruris").ToString());
                }
  
                RespData["success"] = true;
                RespData["method"] = method;
                RespData["seruri"] = seruri;
            }
           // MessageBox.Show(AssemblerDictionary.AssemblerPlugins[resptype].AssemblerType);
            respxml = AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(context, RespData);
       
            return respxml;
           // return RespData;
           // AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(
        }
       //
        public string LogoffPage(System.Web.HttpContext context, string resptype)
        {
            //MessageBox.Show("logoff");
            string respxml = string.Empty;
            HttpRequest request = context.Request;
            Hashtable RespData = new Hashtable();
            RespData["success"] = true;
            RespData["method"] = request.Form.Get("method").ToString();
            if (HttpContext.Current.Request.Cookies["AdminInfo"] != null)
            {
                string sessionID = context.Request.Cookies["AdminInfo"]["sessionID"].ToString();
                string username = context.Request.Cookies["AdminInfo"]["userName"].ToString();
                RespData["username"] = username;
                RespData["userserver"] = WebFrameWork.GetServerCookieValueByNode("userserver");
                RespData["moneyserver"] = WebFrameWork.GetServerCookieValueByNode("moneyserver");
                WebFrameWork.ReleaseCache(sessionID);
            }
            else
            {
                RespData["success"] = false;
            }

            WebFrameWork.ReleaseVerifyInfo();               
            WebFrameWork.ReleaseCookieUserInfo();
           // context.Response.Expires = -1;

         
            respxml = AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(context, (Object)RespData);
            return respxml;
             
        }
        public string OnLoadVerify(System.Web.HttpContext context, string resptype)
        {

            //MessageBox.Show(context.Request.Cookies["userinfo"].Value.ToString());
            string respxml = string.Empty;
            HttpRequest request = context.Request;
            Hashtable RespData = new Hashtable();
           
            RespData["method"] = request.Form.Get("method").ToString();
            if (HttpContext.Current.Request.Cookies["AdminInfo"] != null)
            {
                RespData["success"] = true;
                string username = context.Request.Cookies["AdminInfo"]["userName"].ToString();
                string userserver = context.Request.Cookies["Admininfo"]["userID"].ToString().Split('@')[1].ToString();
                RespData["username"] = username;
                RespData["userserver"] = userserver;
 
            }
            else
            {
                RespData["success"] = false;
                //MessageBox.Show("hi");
                RespData["userserver"] = WebFrameWork.GetServerCookieValueByNode("userserver");
                RespData["moneyserver"] = WebFrameWork.GetServerCookieValueByNode("moneyserver");
                
            }

            respxml = AssemblerDictionary.AssemblerPlugins[resptype].GenerateXml(context, (Object)RespData);
            return respxml;
        }

    

        #endregion
    }
}
