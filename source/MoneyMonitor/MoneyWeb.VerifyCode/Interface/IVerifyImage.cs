using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoneyWeb.VerifyCode.Interface
{
    public interface IVerifyImage
    {
        string CreateVerifyCode(int codeLen);

        string CreateVerifyCode();
    }
}
