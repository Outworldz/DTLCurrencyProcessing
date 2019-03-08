using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.Data.Interface;

namespace MoneyWeb.Data
{
    public static class MoneyAssembler
    {
        public static IXmlAssembler Plugin;
    }

    public static class AssemblerDictionary
    {
        public static Dictionary<string, IXmlAssembler> AssemblerPlugins; 
    }
}
