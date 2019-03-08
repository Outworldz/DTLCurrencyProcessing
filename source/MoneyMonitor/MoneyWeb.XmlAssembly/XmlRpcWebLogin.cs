using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.Data.Interface;
using DeepThink.OpenSim.Region.WebTerminal.XmlAPI.Base;
using System.Web;
using System.Collections;
using System.Xml;

namespace MoneyWeb.XmlRpcHanlder
{
    public class XmlRpcWebLogin : XmlModuleBase, IXmlAssembler
    {
        public override void NodeInsertEvent(object src, System.Xml.XmlNodeChangedEventArgs args)
        {
          //  throw new NotImplementedException();
        }

        public override void NodeRemoveEvent(object src, System.Xml.XmlNodeChangedEventArgs args)
        {
           // throw new NotImplementedException();
        }

        public override void NodeUpdateEvent(object src, System.Xml.XmlNodeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public override System.Xml.XmlDocument getXmlDoc(string xmlPathName)
        {
            _xmlPath = xmlPathName;

            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(xmlPathName);
            _xmlDoc.NodeChanged += new XmlNodeChangedEventHandler(NodeUpdateEvent);
            _xmlDoc.NodeInserted += new XmlNodeChangedEventHandler(NodeInsertEvent);
            _xmlDoc.NodeRemoved += new XmlNodeChangedEventHandler(NodeRemoveEvent);

            return _xmlDoc;
        }

        #region IXmlAssembler Members

        public string AssemblerType
        {
            get { return "XML-LOGIN"; }
        }

        public string GenerateXml(System.Web.HttpContext context, object obj)
        {
            HttpRequest request = context.Request;
            Hashtable RespData = obj as Hashtable;

            plusRootNode("selectChoice");
            plusParentNode("selectElement");
            Hashtable target = new Hashtable();
            target["formName"] = request.Form.Get("form").ToString();
            target["formElem"] = request.Form.Get("target").ToString();
            plusChildNode("/selectChoice/selectElement", target);
            plusParentNode("selectElement");
            target = new Hashtable();

            target["method"] = RespData["method"].ToString();
            target["success"] = RespData["success"].ToString();

            plusChildNode("/selectChoice/selectElement", target);
            switch (RespData["success"].ToString())
            {
                case "True":
                    {
                        plusParentNode("entry");
                        target = new Hashtable();
                        target["userID"] = request.Form.Get("userID").ToString();
              
                        target["seruri"] = request.Form.Get("seruri").ToString();

                       
               
                        plusChildNode("/selectChoice/entry", target);

                    }
                    break;
                case "False":
                    {
                        plusParentNode("entry");
     
                        plusChildNode("/selectChoice/entry", RespData);
                    }
                    break;
            }
            return InnerXML;


        }

        #endregion
    }
}
