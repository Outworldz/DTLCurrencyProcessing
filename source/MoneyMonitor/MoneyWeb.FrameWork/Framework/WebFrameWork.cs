using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Collections;
using OpenMetaverse;

namespace MoneyWeb.FrameWork.Framework
{
    public class WebFrameWork
    {
        public static void RegisterSession(string key, string value)
        {
            HttpContext.Current.Session.Add(key, value);
        }
        public static string GetSessionBykey(string key)
        {
            string value = string.Empty;

            if (System.Web.HttpContext.Current.Session[key] != null)
            {
              value =  HttpContext.Current.Session[key].ToString();
            }
            return value;
        }
        public static void ReleaseSession()
        {
            HttpContext.Current.Session.Abandon();
        }
        public static void RegisterCookieUserInfo(HttpRequest request,string uuid)
        {
            HttpCookie Cookie = new HttpCookie("AdminInfo");
            Cookie.Values["username"] = request.Form.Get("userName").ToString();
            Cookie.Values["userID"] = request.Form.Get("userID").ToString();
            Cookie.Values["sessionID"] = uuid;
            Cookie.Values["moneyserver"] = request.Form.Get("seruris").ToString();
            Cookie.Expires = DateTime.Now.AddDays(1);
            HttpContext.Current.Response.AppendCookie(Cookie);
        }
        public static void RegisterCookieUserInfo(string username, string userid, string sessionid,string moneyser)
        {
            HttpCookie Cookie = new HttpCookie("AdminInfo");
            Cookie.Values["username"] = username;
            Cookie.Values["userID"] = userid;
            Cookie.Values["sessionID"] = sessionid;
            Cookie.Values["moneyserver"] = moneyser;
            Cookie.Expires = DateTime.Now.AddDays(1);
            HttpContext.Current.Response.AppendCookie(Cookie);
        }
        public static void RegisterCookieServerInfo(string userserver, string moneyserver)
        {
            HttpCookie Cookie = new HttpCookie("ServerInfo");
            if (!String.IsNullOrEmpty(userserver))
            {
                Cookie.Values["userserver"] = userserver;
            }
            if (!String.IsNullOrEmpty(moneyserver))
            {
                Cookie.Values["moneyserver"] = moneyserver;
            }
            Cookie.Expires = DateTime.Now.AddYears(1);
            HttpContext.Current.Response.AppendCookie(Cookie);
        }
        public static string GetAdminCookieByKey(string key)
        {
            string cook = string.Empty;
            if (System.Web.HttpContext.Current.Request.Cookies["AdminInfo"] != null)
            {
                cook = HttpContext.Current.Request.Cookies["AdminInfo"][key].ToString();
            }
            return cook;
        }
        public static string GetServerCookieValueByNode(string key)
        {
            string cook = string.Empty;
            if (System.Web.HttpContext.Current.Request.Cookies["ServerInfo"] != null)
            {
                cook = HttpContext.Current.Request.Cookies["ServerInfo"][key].ToString();
            }
            else
            {
                RegisterCookieServerInfo("216.75.21.228", "216.75.21.228");
                cook = "216.75.21.228";
            }
            return cook;
        }
        public static void ReleaseCookieUserInfo()
        {
            HttpCookie Cookies = new HttpCookie("AdminInfo");
            Cookies.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.AppendCookie(Cookies);

        }
        public static void ReleaseVerifyInfo()
        {
            HttpCookie Cookies = new HttpCookie("CheckCode");
            Cookies.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.AppendCookie(Cookies);
        }
        public static void RegisterCache(string SessionID,Object Cache)
        {
            HttpContext.Current.Cache.Add(SessionID, Cache, null, DateTime.Now.AddDays(1), TimeSpan.Zero, CacheItemPriority.High, null);

        }
        public static string TransactionCache(int pageNum,string SessionID)
        {
            Hashtable temptable = (Hashtable)HttpContext.Current.Cache[SessionID];
            return (string)temptable[pageNum];
        }
        public static Hashtable GetCurrentCache(string sessionID)
        {
            Hashtable cachetable = new Hashtable();
            if (HttpContext.Current.Cache.Count > 0)
            {
                cachetable = (Hashtable)HttpContext.Current.Cache[sessionID];
            }
            return cachetable;
        }
        public static Hashtable TransactionHash(int pageNum, string SessionID, string method, string seruri)
        {
            Hashtable xmltable = new Hashtable();
            try
            {
                if (HttpContext.Current.Cache.Count > 0)
                {
                    Hashtable temptable = (Hashtable)HttpContext.Current.Cache[SessionID];
                    xmltable["success"] = true;
                    xmltable["method"] = method;
                    xmltable["seruri"] = seruri;
                    xmltable["startTime"] = Convert.ToInt32(temptable["startTime"]);
                    xmltable["endTime"] = Convert.ToInt32(temptable["endTime"]);
                    xmltable["number"] = Convert.ToInt32(temptable["number"]);
                    xmltable["lastIndex"] = Convert.ToInt32(temptable["lastIndex"]);
                    if (temptable.ContainsKey(pageNum))
                    {
                        xmltable["html"] = temptable[pageNum] as string;
                    }
                    else
                    {
                        xmltable["html"] = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                xmltable["success"] = false;
                xmltable["method"] = method;
                xmltable["seruri"] = seruri;
            }
            return xmltable;
        }

        public static void ReleaseCache(string SessionID)
        {
            if (HttpContext.Current.Cache.Count > 0)
            {
                HttpContext.Current.Cache.Remove(SessionID);
            }
        }
    }
}
