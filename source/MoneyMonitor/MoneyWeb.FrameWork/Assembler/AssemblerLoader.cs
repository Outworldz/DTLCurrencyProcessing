using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.IO;
using MoneyWeb.Data.Interface;
using System.Windows.Forms;

namespace MoneyWeb.FrameWork.Assembler
{
    public static class AssemblerLoader
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// establish the assembly list
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<IXmlAssembler> ResponseAssemblerList(string filename)
        {
            List<IXmlAssembler> plugins = new List<IXmlAssembler>();
            try
            {
                Assembly asm = Assembly.LoadFrom(filename);
                System.Type[] types = asm.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.GetInterface("IXmlAssembler") != null)
                    {
                        IXmlAssembler obj = (IXmlAssembler)Activator.CreateInstance(type, null, null);
                        plugins.Add(obj);
                    }
                }

            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("Failed to loading the ITextureExaminer plugins {0}", ex.Message.ToString());
            }

            return plugins;
        }
        /// <summary>
        /// list the assembler filepaths list
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> ListAssembler(string path)
        {
            List<string> plugins = new List<string>();

            try
            {
                string[] filepaths = Directory.GetFiles(path, "~/MoneyWeb.XmlRpcHandler.dll");

                foreach (string fp in filepaths)
                {
                    //MessageBox.Show(fp);
                    Assembly asm = Assembly.LoadFrom(path); 
                    System.Type[] types = asm.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (type.GetInterface("IXmlAssembler")!=null)
                            plugins.Add(fp);
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("Failed to load the plugins form path {0} : {1}", path, ex.Message.ToString());
            }

            return plugins;

        }
    }
}
