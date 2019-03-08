using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;

namespace MoneyWeb.Data.Interface
{
    public interface IXmlAssembler
    {
        string GenerateXml(HttpContext context,Object obj);
        string AssemblerType { get; }
    }
}
