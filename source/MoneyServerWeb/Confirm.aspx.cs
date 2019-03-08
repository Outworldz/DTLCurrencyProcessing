using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Nwc.XmlRpc;
using log4net;
using System.Reflection;

public partial class Confirm : System.Web.UI.Page
{
    private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    string m_moneyURI = ConfigurationManager.AppSettings["MoneyServerURI"];
    const int CONFIRM_REQUEST_TIMEOUT = 30*1000;
    string m_sender = string.Empty;
    string m_destination = string.Empty;
    int m_amount = 0;
    string m_dateTime = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        string transID = Request.QueryString["transactionID"];
        if (!string.IsNullOrEmpty(transID))
        {
            Hashtable paramTable = new Hashtable();
            paramTable["transactionID"] = transID;
            Hashtable responseData = genericXMLRpcRequest("GetTransaction",paramTable);
            if (responseData != null && responseData.ContainsKey("success"))
            {
                if ((bool)responseData["success"])
                {
                    m_log.InfoFormat("Got transaction:{0} successfully ", transID);
                    m_sender = (string)responseData["sender"];
                    m_destination = (string)responseData["receiver"];
                    m_amount = (int)responseData["amount"];
                    int time = (int)responseData["time"];
                    if (time > 0)
                    {
                        long ticksToUnixEpoch = new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc).Ticks;
                        DateTime dateTransfer = new DateTime(ticksToUnixEpoch + 10000000L * (long)time);
                        m_dateTime = dateTransfer.ToString();
                    }
                    ltSender.Text = m_sender;
                    ltDestination.Text = m_destination;
                    ltAmount.Text = m_amount.ToString();
                    return;
                }
            }
        }
        ltSender.Text = "Unknown";
        ltDestination.Text = "Unknown";
        ltAmount.Text = "0";
    }

    #region Cancel button click handler
    /// <summary>
    /// Cancel the transaction
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Button2_Click(object sender, EventArgs e)
    {
        string transID = Request.QueryString["transactionID"];
        string secureCode = Request.QueryString["secureCode"];
        string strDeny = string.Empty;
        string strError = string.Empty;

        if (string.IsNullOrEmpty(transID) || string.IsNullOrEmpty(secureCode))
        {
            strError = generateErrorPage(m_sender, m_destination, m_amount);
            Response.ContentType = "text/html";
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(strError);
            Response.Flush();
            Response.Close();
            return;
        }
        Hashtable paramTable = new Hashtable();
        paramTable["transactionID"] = transID;
        paramTable["secureCode"] = secureCode;
        m_log.InfoFormat("User:{0} has canceled the transaction:{1} ", sender, transID);

        Hashtable responseData = genericXMLRpcRequest("CancelTransfer", paramTable);

        if (responseData != null && responseData.ContainsKey("success"))
        {
            if ((bool)responseData["success"])
            {
                m_log.InfoFormat("Transaction:{0} canceled successfully ", transID);
                strDeny = generateDeniedPage(m_sender, m_destination, m_amount);
                Response.ContentType = "text/html";
                Response.ContentEncoding = Encoding.UTF8;
                Response.Write(strDeny);
                Response.Flush();
                Response.Close();
                return;
            }
        }
        strError = generateErrorPage(m_sender, m_destination, m_amount);
        Response.ContentType = "text/html";
        Response.ContentEncoding = Encoding.UTF8;
        Response.Write(strError);
        Response.Flush();
        Response.Close();
    }
    #endregion 


    #region Approve button click handler
    /// <summary>
    /// Approve the transaction
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string transID = Request.QueryString["transactionID"];
        string secureCode = Request.QueryString["secureCode"];
        string strError = string.Empty;
        string strApprove = string.Empty;

        if (string.IsNullOrEmpty(transID) || string.IsNullOrEmpty(secureCode))
        {
            strError = generateErrorPage(m_sender,m_destination,m_amount);
            Response.ContentType = "text/html";
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(strError);
            Response.Flush();
            Response.Close();
            return;
        }
        Hashtable paramTable = new Hashtable();
        paramTable["transactionID"] = transID;
        paramTable["secureCode"] = secureCode;
        m_log.InfoFormat("User: {0}has accepted the transaction:{1} ", m_sender, transID);
        Hashtable responseData = genericXMLRpcRequest("ConfirmTransfer", paramTable);
        if (responseData != null && responseData.ContainsKey("success"))
        {
            if ((bool)responseData["success"])
            {
                m_log.InfoFormat("Transaction:{0} finished successfully ", transID);
                strApprove = generateApprovedPage(m_sender,m_destination,m_amount,m_dateTime);
                Response.ContentType = "text/html";
                Response.ContentEncoding = Encoding.UTF8;
                Response.Write(strApprove);
                Response.Flush();
                Response.Close();
                return;
            }
        }
        strError = generateErrorPage(m_sender, m_destination, m_amount);
        Response.ContentType = "text/html";
        Response.ContentEncoding = Encoding.UTF8;
        Response.Write(strError);
        Response.Flush();
        Response.Close();
    }
    #endregion 


    #region generate transaction error page
    private string generateErrorPage(string sender,string destination,int amount)
    {
        StringBuilder sb = new StringBuilder(5000);
        sb.Append("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN'");
        sb.Append("'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
        sb.Append("<html xmlns='http://www.w3.org/1999/xhtml'>");
        sb.Append("<head>");
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
        sb.Append("<title>Transaction Error</title>");
        sb.Append("<link href='style.css' rel='stylesheet' type='text/css' />");
        sb.Append("<style type='text/css'>");
        sb.Append("<!--");
        sb.Append(".style3 {");
        sb.Append("font-size: 24px;");
        sb.Append("font-weight: bold;");
        sb.Append("font-family: Arial, Helvetica, sans-serif;}");
        sb.Append(".style4 {font-family: Arial, Helvetica, sans-serif}");
        sb.Append(".style5 {");
        sb.Append("font-family: Arial, Helvetica, sans-serif;");
        sb.Append("font-size: 14px;}");
        sb.Append(".style6 {font-family: Arial, Helvetica, sans-serif; font-size: 14px; font-weight: bold; }");
        sb.Append("-->");
        sb.Append("</style>");
        sb.Append("</head>");
        sb.Append("<body>");
        sb.Append("<table width='450' border='0' align='center' cellpadding='5' cellspacing='0' class='border1px'>");
        sb.Append("<tr><td width='32' bgcolor='#FFFFCC'><img src='error.png' alt='icon' width='32' height='32' /></td>");
        sb.Append("<td bgcolor='#FFFFCC'><span class='style3'>Transaction Error</span></td></tr>");
        sb.Append("<tr><td colspan='2'><p class='style5'>");
        sb.Append("An error occured while making a transaction from your account. Please wait 15 minutes then try again.</p>");
        sb.Append("<table width='80%' border='0' align='center' cellpadding='3' cellspacing='0'>");
        sb.Append("<tr><td class='style5 onepixlower'>Your account</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(sender);
        sb.Append("</td></tr>");
        sb.Append("<tr><td class='style5 onepixlower'>Destination</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(destination);
        sb.Append("</td></tr>");
        sb.Append("<tr>");
        sb.Append("<td class='style5 onepixlower'>Amount to Transfer</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(amount.ToString());
        sb.Append("</td></tr>");
        sb.Append("</table>");
        sb.Append("<p class='style5'>No money has been transferred from your account.</p> </td></tr>");
        sb.Append("</table>");
        sb.Append("</body>");
        sb.Append("</html>");
        return sb.ToString();
    }
#endregion 

    #region generate transaction approved web page
    private string generateApprovedPage(string sender,string destination,int amount,string time)
    {
        StringBuilder sb = new StringBuilder(5000);
        sb.Append("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN'");
        sb.Append("'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
        sb.Append("<html xmlns='http://www.w3.org/1999/xhtml'>");
        sb.Append("<head>");
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
        sb.Append("<title>Transaction approved</title>");
        sb.Append("<link href='style.css' rel='stylesheet' type='text/css' />");
        sb.Append("<style type='text/css'>");
        sb.Append("<!--");
        sb.Append(".style3 {");
        sb.Append("font-size: 24px;");
        sb.Append("font-weight: bold;");
        sb.Append("font-family: Arial, Helvetica, sans-serif;}");
        sb.Append(".style4 {font-family: Arial, Helvetica, sans-serif}");
        sb.Append(".style5 {");
        sb.Append("font-family: Arial, Helvetica, sans-serif;");
        sb.Append("font-size: 14px;}");
        sb.Append(".style6 {font-family: Arial, Helvetica, sans-serif; font-size: 14px; font-weight: bold; }");
        sb.Append("-->");
        sb.Append("</style>");
        sb.Append("</head>");
        sb.Append("<body>");
        sb.Append("<table width='450' border='0' align='center' cellpadding='5' cellspacing='0' class='border1px'>");
        sb.Append("<tr><td width='32' bgcolor='#FFFFCC'><img src='info.png' alt='icon' width='32' height='32' /></td>");
        sb.Append("<td bgcolor='#FFFFCC'><span class='style3'>Transaction Approved</span></td></tr>");
        sb.Append("<tr><td colspan='2'><p class='style5'>");
        sb.Append("The following transaction was approved</p>");
        sb.Append("<table width='80%' border='0' align='center' cellpadding='3' cellspacing='0'>");
        sb.Append("<tr><td class='style5 onepixlower'>Your account</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(sender);
        sb.Append("</td></tr>");
        sb.Append("<tr><td class='style5 onepixlower'>Destination</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(destination);
        sb.Append("</td></tr>");
        sb.Append("<tr>");
        sb.Append("<td class='style5 onepixlower'>Amount to Transfer</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(amount.ToString());
        sb.Append("</td></tr>");
        sb.Append("<tr>");
        sb.Append("<td class='style5 onepixlower'>When</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(time);
        sb.Append("</td></tr>");
        sb.Append("</table>");
        sb.Append("<p class='style5'>You may save a copy of this page for your records, ");
        sb.Append("or you may close this window at any point in time.</p></td></tr>");
        sb.Append("</table>");
        sb.Append("</body>");
        sb.Append("</html>");
        return sb.ToString();
    }
    # endregion 

    #region generate transaction denied web page
    private string generateDeniedPage(string sender,string destination,int amount)
    {
        StringBuilder sb = new StringBuilder(5000);
        sb.Append("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN'");
        sb.Append("'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
        sb.Append("<html xmlns='http://www.w3.org/1999/xhtml'>");
        sb.Append("<head>");
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
        sb.Append("<title>Transaction denied</title>");
        sb.Append("<link href='style.css' rel='stylesheet' type='text/css' />");
        sb.Append("<style type='text/css'>");
        sb.Append("<!--");
        sb.Append(".style3 {");
        sb.Append("font-size: 24px;");
        sb.Append("font-weight: bold;");
        sb.Append("font-family: Arial, Helvetica, sans-serif;}");
        sb.Append(".style4 {font-family: Arial, Helvetica, sans-serif}");
        sb.Append(".style5 {");
        sb.Append("font-family: Arial, Helvetica, sans-serif;");
        sb.Append("font-size: 14px;}");
        sb.Append(".style6 {font-family: Arial, Helvetica, sans-serif; font-size: 14px; font-weight: bold; }");
        sb.Append("-->");
        sb.Append("</style>");
        sb.Append("</head>");
        sb.Append("<body>");
        sb.Append("<table width='450' border='0' align='center' cellpadding='5' cellspacing='0' class='border1px'>");
        sb.Append("<tr><td width='32' bgcolor='#FFFFCC'><img src='error.png' alt='icon' width='32' height='32' /></td>");
        sb.Append("<td bgcolor='#FFFFCC'><span class='style3'>Transaction Denied</span></td></tr>");
        sb.Append("<tr><td colspan='2'><p class='style5'>");
        sb.Append("The following transaction was denied</p>");
        sb.Append("<table width='80%' border='0' align='center' cellpadding='3' cellspacing='0'>");
        sb.Append("<tr><td class='style5 onepixlower'>Your account</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(sender);
        sb.Append("</td></tr>");
        sb.Append("<tr><td class='style5 onepixlower'>Destination</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(destination);
        sb.Append("</td></tr>");
        sb.Append("<tr>");
        sb.Append("<td class='style5 onepixlower'>Amount to Transfer</td>");
        sb.Append("<td class='style5 onepixlower'>");
        sb.Append(amount.ToString());
        sb.Append("</td></tr>");
        sb.Append("</table>");
        sb.Append("<p class='style5'>No money has been transferred from your account.</p></td></tr> ");
        sb.Append("</table>");
        sb.Append("</body>");
        sb.Append("</html>");
        return sb.ToString();
    }
    #endregion


    #region generic XmlRpc abstraction
    /// <summary>
    /// generic XmlRpc abstraction
    /// </summary>
    /// <param name="method">Method to invoke</param>
    /// <param name="paramTable">Hashtable containing parameters to the method</param>
    /// <returns>Hashtable with success=>bool and other values</returns>
    private Hashtable genericXMLRpcRequest(string method, Hashtable paramTable)
    {
        // Handle the error in parameter list.   
        if (paramTable.Count <= 0 ||
            string.IsNullOrEmpty(method) ||
            string.IsNullOrEmpty(m_moneyURI))
        {
            return null;
        }
        ArrayList arrayParams = new ArrayList();
        arrayParams.Add(paramTable);
        XmlRpcResponse response = null;
        try
        {
            XmlRpcRequest request = new XmlRpcRequest(method, arrayParams);
            response = request.Send(m_moneyURI, CONFIRM_REQUEST_TIMEOUT);
        }
        catch (Exception ex)
        {
            m_log.ErrorFormat(
                "[MONEY]: Unable to connect to money server {0}.  Exception {1}",
                    m_moneyURI, ex.ToString());

            Hashtable ErrorHash = new Hashtable();
            ErrorHash["success"] = false;
            ErrorHash["errorMessage"] = "Unable to connect to money server";

            return ErrorHash;
        }
        if (response.IsFault)
        {
            Hashtable ErrorHash = new Hashtable();
            ErrorHash["success"] = false;
            ErrorHash["errorMessage"] = "Money server returns error";
            return ErrorHash;
        }
        Hashtable responseData = (Hashtable)response.Value;
        return responseData;
    }
    #endregion 

}
