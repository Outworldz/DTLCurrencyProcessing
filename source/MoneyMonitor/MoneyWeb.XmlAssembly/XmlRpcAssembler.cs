using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepThink.OpenSim.Region.WebTerminal.XmlAPI.Base;
using MoneyWeb.Data.Interface;
using System.Xml;
using System.Web;
using System.Collections;
using System.Windows.Forms;

namespace MoneyWeb.XmlRpcHanlder
{
    public class XmlRpcAssembler : XmlModuleBase , IXmlAssembler
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
           // throw new NotImplementedException();
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

        public string GenerateXml(HttpContext context, Object obj)
        {
            HttpRequest request = context.Request;
            Hashtable RespData = obj as Hashtable;
            //MessageBox.Show("xml-assembly");
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

            //MessageBox.Show(RespData["success"].ToString());

            //foreach (DictionaryEntry de in RespData)
            //{
            //    MessageBox.Show(de.Key.ToString() + " xml-assembly   " + de.Value.ToString());
            //}
            plusParentNode("entry");
 
            
                plusChildNode("/selectChoice/entry", RespData);
            

            //switch (RespData["success"].ToString())
            //{
            //    case "True":
            //        {
            //            plusParentNode("entry");
            //            target = new Hashtable();
            //            target["uuid"] = RespData["uuid"].ToString();
            //            //target["avatar_name"] = request.Form.Get("avatar_name").ToString();
            //            target["seruri"] = request.Form.Get("seruri").ToString();
            //           // target["password"] = request.Form.Get("password").ToString();
            //            plusChildNode("/selectChoice/entry", target);

            //        }
            //        break;
            //    case "False":
            //        {
            //            plusParentNode("entry");
            //            //target = new Hashtable();
            //            //target["message"] = RespData["message"].ToString();
            //            // target["seruri"] = RespData["seruri"].ToString();
            //            plusChildNode("/selectChoice/entry", RespData);
            //        }
            //        break;
            //}
     

           // target = new Hashtable();

           // plusChildNode("/selectChoice/entry", RespData);
           // plusParentNode("entry");
            //foreach (DictionaryEntry de in RespData)
            //{
            //    MessageBox.Show(de.Key.ToString() + " xml-assembly   " + de.Value.ToString());
            //}
            //target = new Hashtable();
            
          //  plusChildNode("/selectChoice/entry", RespData);

            //IDictionaryEnumerator enumerator = RespData.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    plusParentNode("entry");
            //    target = new Hashtable();
            //    target.Add("optionValue", enumerator.Value.ToString());
            //    target.Add("optionKey", enumerator.Key.ToString());
            //    plusChildNode("/selectChoice/entry", target);
            //}
           // MessageBox.Show(InnerXML);
            return InnerXML;
        }

        public string AssemblerType
        {
            get { return "XML-RPC"; }
        }

        #endregion
    }
}
