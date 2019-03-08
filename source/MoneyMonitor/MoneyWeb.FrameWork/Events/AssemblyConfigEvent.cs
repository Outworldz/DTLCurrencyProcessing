using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nini.Config;
using System.Xml;
using System.Collections;
using System.Web;
using MoneyWeb.FrameWork.Assembler;
using MoneyWeb.Data;
using MoneyWeb.Data.Interface;
using System.Windows.Forms;
using MoneyWeb.XmlRpcHanlder;
using OpenMetaverse;

namespace MoneyWeb.FrameWork.Events
{
    public class AssemblyParamsEvent : EventArgs
    {
        private Hashtable _paramtable;

        private long TicksToEpoch = new DateTime(1970, 1, 1).Ticks;

        public AssemblyParamsEvent(Hashtable paramtable)
        {
            _paramtable = paramtable;
            //InitAssemblerDictionary();
        }

        public string AssemblyPath
        {
            get { lock (_paramtable) { return _paramtable["AssemblyPath"].ToString(); } }
        }

        public string AssemblyType
        {
            get { lock (_paramtable) { return _paramtable["AssemblyType"].ToString(); } }
        }

        public string RequestMethod
        {
            get { lock (_paramtable) { return _paramtable["RequestMethod"].ToString(); } }
        }
        public int AssemblyPort
        {
            get { lock (_paramtable) { return Convert.ToInt32(_paramtable["AssemblyPort"].ToString()); } }
        }
        public void ParamsCheck(HttpRequest request)
        {
            string[] requestparam = _paramtable["RequestParams"].ToString().Split(';');

            for (int ix = 0; ix < requestparam.Length; ix++)
            {
               
                //string query = request.Form.Get(requestparam[ix].ToString()).ToString();
                //if (String.IsNullOrEmpty(query))
                //     throw "Param " + requestparam[ix].ToString() + " is null or empty"; 
                
            }
        }
        public Object[] ParamsAssembly(HttpContext context)
        {
            string[] requestparam = _paramtable["RequestParams"].ToString().Split(';');
            int paramnum = requestparam.Length;
          //  MessageBox.Show(paramnum.ToString());
            Object[] args = new Object[paramnum];
            for (int ix = 0; ix < paramnum; ix++)
            {
                args[ix] = ParamParser(requestparam[ix].ToString(), context);
              //  MessageBox.Show(args[ix].ToString());
            }
            return args;
        }


        private void InitAssembler()
        {
            List<string> assembers = AssemblerLoader.ListAssembler(".");
            foreach (string a in assembers)
            {
               // MoneyAssembler.Plugin = AssemblerLoader.ResponseAssemblerList(a);
                break;
            }
        }
 
        private Object ParamParser(string paramarray,HttpContext context)
        {
            string[] param = paramarray.Split('/');
            HttpRequest request = context.Request;
            Object obj = null;
           // MessageBox.Show(param[0].ToString() + "   " + param[1].ToString());
            switch (param[1].ToString())
            {
                case "string":
                case "String":
                    {
                        if (param[0].ToString().Equals("seruri"))
                        {
                            obj = (Object)("http://" + request.Form.Get(param[0].ToString()).ToString() + ":" + AssemblyPort.ToString() + "/");
                        }
                        else if (param[0].ToString().Equals("resptype"))
                            obj = (Object)ResponseType;
                        else if (param[0].ToString().Equals("seruris"))
                        {
                            string moneyserver = request.Form.Get(param[0].ToString()).ToString();
                            if (String.IsNullOrEmpty(moneyserver))
                            {
                                moneyserver = context.Request.Cookies["AdminInfo"]["moneyserver"].ToString();
                            }

                            obj = (Object)("https://" + moneyserver + ":" + AssemblyPort.ToString());
                        }
                        else if (param[0].ToString().Equals("sessionID"))
                            obj = (Object)context.Request.Cookies["AdminInfo"]["sessionID"].ToString();
                        else
                            obj = (Object)request.Form.Get(param[0].ToString()).ToString();
                    }
                    break;
                case "int":
                    {
                       // MessageBox.Show(request.Form.Get(param[0].ToString()));
                        obj = (Object)Convert.ToInt32(request.Form.Get(param[0].ToString()));
                    }
                    break;
                case "Hashtable":
                    {
                        string[] hashparams = param[0].ToString().Split(',');
                        Hashtable hashobj = new Hashtable();
                       // MessageBox.Show(hashparams.Length.ToString());
                        for (int i = 0; i < hashparams.Length-1; i++)
                        {
                           //MessageBox.Show(hashparams[i].ToString());
                            if (hashparams[i].ToString().Equals("startTime") || hashparams[i].ToString().Equals("endTime"))
                            {
                                string[] times = request.Form.Get(hashparams[i].ToString()).Split('-');
                                long timeTicks = new DateTime(Convert.ToInt32(times[0]), Convert.ToInt32(times[1]), Convert.ToInt32(times[2]),Convert.ToInt32(times[3]), Convert.ToInt32(times[4]), Convert.ToInt32(times[5]), DateTimeKind.Utc).Ticks;
                                int time = (int)((timeTicks - TicksToEpoch) / 10000000);
                                hashobj[hashparams[i].ToString()] = time;
                            }
                            else if (hashparams[i].ToString().Equals("sessionID"))
                            {
                                if (HttpContext.Current.Request.Cookies["AdminInfo"] == null)
                                {
                                    hashobj[hashparams[i].ToString()] = UUID.Random().ToString();
                                }
                                else
                                {
                                    hashobj[hashparams[i].ToString()] = context.Request.Cookies["AdminInfo"]["sessionID"].ToString();
                                }
                               
                            }
                            else if(hashparams[i].ToString().Equals("userID"))
                            {
                                
                                string usrID = request.Form.Get("userID").ToString();
                                if (String.IsNullOrEmpty(usrID))
                                {
                                    usrID = context.Request.Cookies["AdminInfo"]["userID"].ToString();
                                }
                                hashobj[hashparams[i].ToString()] = usrID;
                            }
                            else if (hashparams[i].ToString().Equals("lastIndex"))
                            {
                                hashobj[hashparams[i].ToString()] = Convert.ToInt32(request.Form.Get(hashparams[i]));
                            }
                            else
                            {
                                hashobj[hashparams[i].ToString()] = request.Form.Get(hashparams[i].ToString()).ToString();
                            }
                         //  MessageBox.Show(hashparams[i].ToString()+ "->" +hashobj[hashparams[i].ToString()].ToString());
                        }
                        obj = (Object)hashobj;
                    }
                    break;
      
                case "HttpRequest":
                    {
                        obj = (Object)request;
                    }
                    break;
                case "HttpContext":
                    {
                        obj = (Object)context;
                    }
                    break;
                default:
                    break;
            }
            return obj;
        }
        public string[] RequestParams
        {
            get 
            {
                lock (_paramtable) 
                {
                    string[] requestparam = _paramtable["RequestParams"].ToString().Split(';');
                    return requestparam;
                } 
            }
        }
        public ArrayList RequestList
        {
            get 
            { 
                lock (_paramtable) 
                {
                    ArrayList requestlist = new ArrayList();
                    string[] requestparam = _paramtable["RequestParams"].ToString().Split(';');
                    for (int ix = 0; ix < requestparam.Length; ix++)
                    {
                        requestlist.Add(requestparam[ix].ToString());
                    }
                    return requestlist;
                } 
            }
        }
        public string ResponseAssembly(Object  obj, HttpContext context)
        {
            string responsetype = _paramtable["ResponseType"].ToString();
            //MessageBox.Show(responsetype);
            string xmlresponse = string.Empty;
            try
            {
                xmlresponse = AssemblerDictionary.AssemblerPlugins[responsetype].GenerateXml(context, obj);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
            }
            return xmlresponse;
        }
        public string ResponseType
        {
            get { lock (_paramtable) { return _paramtable["ResponseType"].ToString(); } }
        }
    }

    public class AssemblyConfigEvent : EventArgs
    {
        private Dictionary<string, Hashtable> AssemblyDictionary = new Dictionary<string, Hashtable>();

        public AssemblyConfigEvent(XmlReader xmlreader)
        {
            IConfigSource xmlconfig = new XmlConfigSource(xmlreader);
            IEnumerator emumerator = xmlconfig.Configs.GetEnumerator();
            while (emumerator.MoveNext())
            {
                IConfig config = emumerator.Current as IConfig;
                Hashtable param = new Hashtable();
                foreach (string key in config.GetKeys())
                {
                    param.Add(key, config.GetString(key, string.Empty));
                }
                AssemblyDictionary.Add(config.Name, param);
            }
           // InitAssemblerDictionary();
            InitAssembler();
        }

        public AssemblyParamsEvent GetAssemblyParamsByName(string method)
        {
            AssemblyParamsEvent param = new AssemblyParamsEvent(AssemblyDictionary[method]);
            return param;
        }

        private void InitAssembler()
        {
            AssemblerDictionary.AssemblerPlugins = new Dictionary<string,IXmlAssembler>();
            IXmlAssembler asm = new XmlRpcAssembler();
            AssemblerDictionary.AssemblerPlugins[asm.AssemblerType] = asm;
        }

        private void InitAssemblerDictionary()
        {
            List<string> assembers = AssemblerLoader.ListAssembler(".");
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
    }
}
