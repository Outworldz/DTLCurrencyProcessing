using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MoneyWeb.VerifyCode.Interface;
using System.Web;

namespace MoneyWeb.VerifyCode.Base
{
    public abstract class VerifyBase : IVerifyImage
    {
        #region constructor Members
        public VerifyBase()
        {
            Length = this.ilength;
            FontSize = this.fontSize;
            Chaos = this.chaos;
            BackgroundColor = this.backgroundColor;
            ChaosColor = this.chaosColor;
            Colors = this.colors;
            Fonts = this.fonts;
            Padding = this.padding;
        }
        #endregion

        private int ilength = 6;

        public int Length
        {
            get { return ilength; }
            set { ilength = value; }
        }

        private int fontSize = 40;

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        private int padding = 2;

        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        private bool chaos = true;

        public bool Chaos
        {
            get { return chaos; }
            set { chaos = value; }
        }

        private Color chaosColor = Color.LightGray;

        public Color ChaosColor
        {
            get { return chaosColor; }
            set { chaosColor = value; }
        }
        private Color backgroundColor = Color.White;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private Color[] colors = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };

        public Color[] Colors
        {
            get { return colors; }
            set { colors = value; }
        }

        private string[] fonts = { "Arial", "Georgia" };

        public string[] Fonts
        {
            get { return fonts; }
            set { fonts = value; }
        }

        private string codeSerial = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";

        public string CodeSerial
        {
            get { return codeSerial; }
            set { codeSerial = value; }
        }

        #region IVerifyImage Members

        public string CreateVerifyCode(int codeLen)
        {
            if (codeLen == 0)
                codeLen = Length;
            string[] arr = CodeSerial.Split(',');
            string code = "";
            int randValue = -1;
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < codeLen; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                code += arr[randValue];
            }
            return code;
        }

        public string CreateVerifyCode()
        {
            return CreateVerifyCode(0);
        }

        #endregion

        public abstract Bitmap TwistImage(Bitmap srcBmp, bool bXdir, double dMultValue, double dPhase);

        public abstract Bitmap CreateImageCode(string code);

        public abstract void CreateImageOnPage(string code, HttpContext context);
    }
}
