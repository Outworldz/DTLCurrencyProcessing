using System;
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
using MoneyWeb.VerifyCode;


public partial class VerifyCode : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        VerifyImage v = new VerifyImage();
        string code = v.CreateVerifyCode();
        v.CreateImageOnPage(code, this.Context);
        Response.Cookies.Add(new HttpCookie("CheckCode", code.ToUpper()));
        
    }
}
