<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="MoneyMonitor.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<HTML xmlns="http://www.w3.org/1999/xhtml">
<HEAD>
<TITLE>Deep Think Money Panel</TITLE>
    <meta http-equiv="Content-type" content="text/html; charset=utf-8" />
	<meta name="Keywords" content="" />
	<meta name="Description" content="" />
	<meta name="Copyright" content="" />
  	<meta name = "get_user_by_name" content = "seruri|useruri|avatar_name|password|verifynumber" />
  	<meta name ="WebGetTransaction" content = "startTime|endTime" />
  	<meta name="WebGetTransactionNum" content = "startTime|endTime"/>
    <link rel="stylesheet" href="css/reset.css" type="text/css" media="all" />
    <link rel="stylesheet" href="css/master.css" type="text/css" media="all" />
    <link rel="stylesheet" href="css/style.css" type="text/css" media="all" />
    <link rel="stylesheet" href="css/module.css" type="text/css" media="all" />    
    <script type='text/javascript' src = 'js/util.js' > </script>
    <script type='text/javascript' src = 'js/mods.js' > </script>
    <script type='text/javascript' src = 'js/layout.js'> </script>
	<script type='text/javascript' src = 'js/xmlrpc.js'> </script>
	<script type='text/javascript' src ='js/My97DatePicker/WdatePicker.js'></script>
	
</HEAD>
<BODY>
	 <FORM id="form1" name="form1">
		<div id="header">
			<div id="headerLeft">
				Money Server Panel 			
				<span id="HeaderLeftLicense">Licensed to DeepThink Pty Ltd</span>	
			</div>
			<div id="headerRight">
			</div>
		</div>
	    <div id="content">
	    		<div id="contentLeft">
	    		
	<div id="PARENT">
<ul id="nav">
<li><a href="#Menu=ChildMenu1"  onclick="DoMenu('ChildMenu1')">My Money Server</a>
	<ul id="ChildMenu1" class="collapsed">
	<li><a href="javascript:LoginLayout('216.75.21.228','216.75.21.228')">Log in</a></li>
	<li><a href="javascript:Logoff()" >Log off</a></li>

	</ul>
</li>
<li><a href="#Menu=ChildMenu2" onclick="DoMenu('ChildMenu2')">My Account</a>
	<ul id="ChildMenu2" class="collapsed">
	<!--a href="http://www.netany.net" target="_blank">Pay</a></li-->
	<li><a href="javascript:GetBalance()">User Balance</a></li>
	<li><a href="javascript:QueryTransaction()">Transaction Query</a></li>
	</ul>
</li>
<li><a href="#Menu=ChildMenu3" onclick="DoMenu('ChildMenu3')">Balances Manage</a>
	<ul id="ChildMenu3" class="collapsed">

	</ul>
</li>
<li><a href="#Menu=ChildMenu4" onclick="DoMenu('ChildMenu4')">Transactions Manage</a>
	<ul id="ChildMenu4" class="collapsed">

	</ul>
</li>
</ul>
</div>


		</div>
			<div id="contentRight"  style="border: thin ridge #C0C0C0">

	    </div>
	    </div>


	</FORM>
</BODY>
</HTML>
