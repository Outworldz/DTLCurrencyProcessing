<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Confirm.aspx.cs" Inherits="Confirm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Approve or Deny</title>
    <link href="style.css" rel="stylesheet" type="text/css" />
<style type="text/css">
<!--
.style3 {
	font-size: 24px;
	font-weight: bold;
	font-family: Arial, Helvetica, sans-serif;
}
.style4 {font-family: Arial, Helvetica, sans-serif}
.style5 {
	font-family: Arial, Helvetica, sans-serif;
	font-size: 14px;
}
.style6 {font-family: Arial, Helvetica, sans-serif; font-size: 14px; font-weight: bold; }
-->
</style>
</head>
<body>
    <form id="form1" runat="server">
    
    <table width="450" border="0" align="center" cellpadding="5" cellspacing="0" class="border1px">
  <tr>
    <td width="32" bgcolor="#FFFFCC"><img src="warn.png" alt="icon" width="32" height="32" /></td>
    <td bgcolor="#FFFFCC"><span class="style3">Transaction Notice</span></td>
  </tr>
  <tr>
    <td colspan="2"><p class="style5">Your currency server operator has requested that you please confirm the details of the following currency operation.</p>
      <table width="80%" border="0" align="center" cellpadding="3" cellspacing="0">
        <tr>
          <td class="style5 onepixlower">Your account</td>
          <td class="style5 onepixlower"><asp:Literal ID = "ltSender" runat ="server"></asp:Literal></td>
        </tr>
        <tr>
          <td class="style5 onepixlower">Destination</td>
          <td class="style5 onepixlower"><asp:Literal ID = "ltDestination" runat = "server"></asp:Literal></td>
        </tr>
        <tr>
          <td class="style5 onepixlower">Amount to Transfer</td>
          <td class="style5 onepixlower"><asp:Literal ID = "ltAmount" runat = "server"></asp:Literal></td>
        </tr>
        <tr>
          <td class="style5 onepixlower">When</td>
          <td class="style5 onepixlower">Immediately</td>
        </tr>
      </table>      
      <p class="style5">If you did not request this transaction, please hit the 'Cancel' button listed below, otherwise if you recognise this account and the amount to be debited, please hit Approve.</p>
      <table width="80%" border="0" align="center" cellpadding="0" cellspacing="0">
        <tr>
          <td><label>
            <div align="center">
              <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Approve" />
              </div>
          </label></td>
          <td><div align="center">
            <asp:Button ID="Button2" runat="server" onclick="Button2_Click" 
            Text="Cancel" />
          </div></td>
        </tr>
      </table>      </td>
  </tr>
</table>
    </form>
</body>
</html>