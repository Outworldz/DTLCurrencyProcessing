using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using MoneyWeb.FrameWork.Events;

namespace MoneyWeb.HttpHandler.Base
{
    public class HttpRequestBase
    {
        /// <summary>
        /// const string for the embedded resource
        /// </summary>
        private const string _ASSEMBLY_RESOURCE = "MoneyWeb.HttpHandler.Assembly.xml";

        protected AssemblyConfigEvent Assemblyconfig;

        protected string httpAppPath; 

        #region constructor Members

        public HttpRequestBase()
        {
            string xmlstring =  getResourceString(_ASSEMBLY_RESOURCE);
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(xmlstring);
            XmlReader reader = XmlReader.Create(new StringReader(xmlstring));
            Assemblyconfig = new AssemblyConfigEvent(reader);
            //AssemblySetting = Assemblyconfig.
            //MessageBox.Show(xmlstring);
        }
        #endregion

        #region private Members

        /// <summary>
        /// Extract a named string resource from the embedded resources
        /// </summary>
        /// <param name="name">name of embedded resource</param>
        /// <returns>string contained within the embedded resource</returns>
        private  string getResourceString(string name)
        {
            Assembly assem = GetType().Assembly;
            string[] names = assem.GetManifestResourceNames();
            string resourceString = null;
            //XmlTextReader xmlReader = null;
           

            foreach (string s in names)
            {
                if (s.EndsWith(name))
                {
                    using (Stream resource = assem.GetManifestResourceStream(s))
                    {
                        // xmlReader = new XmlTextReader(resource);

                         using (StreamReader resourceReader = new StreamReader(resource))
                         {
                             resourceString = resourceReader.ReadToEnd();

                         }
                    }
                }
            }
            return resourceString;
        }
        #endregion
    }
}
