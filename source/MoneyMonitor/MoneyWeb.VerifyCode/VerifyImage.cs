using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoneyWeb.VerifyCode.Base;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace MoneyWeb.VerifyCode
{
    public class VerifyImage : VerifyBase
    {
        private const double PI = 3.1415926535897932384626433832795;
        private const double PI2 = 6.283185307179586476925286766559;

        public VerifyImage() : base() { }

        public override System.Drawing.Bitmap CreateImageCode(string code)
        {
            int fSize    = FontSize;
            int fWidth  = fSize + Padding;

            int imageWidth        = (int)(code.Length * fWidth) + 4 + Padding * 2;
            int imageHeight        = fSize *2 + Padding;

            System.Drawing.Bitmap image = new System.Drawing.Bitmap(imageWidth, imageHeight);

            Graphics g = Graphics.FromImage(image);

            g.Clear(BackgroundColor);

            Random rand = new Random();


            if(this.Chaos)
            {

                Pen pen = new Pen(ChaosColor, 0);
                int c = Length * 10;

                for(int i=0;i<c;i++)
                {
                    int x = rand.Next(image.Width);
                    int y = rand.Next(image.Height);

                    g.DrawRectangle(pen, x, y, 1, 1);
                }
            }

            int left    = 0 , top = 0, top1 = 1, top2 = 1;

            int n1        = (imageHeight - FontSize - Padding * 2);
            int n2        = n1/4;
            top1        = n2;
            top2        = n2 *2;
            
            Font f;
            Brush b;

            int cindex, findex;


            for(int i=0; i<code.Length; i++)
           {
                cindex = rand.Next(Colors.Length - 1);
                findex = rand.Next(Fonts.Length - 1);
               
                f = new System.Drawing.Font(Fonts[findex], fSize, System.Drawing.FontStyle.Bold);
                b = new System.Drawing.SolidBrush(Colors[cindex]);

                if( i%2 == 1 )
                {
                    top = top2;
                }
                else
                {
                    top = top1;
                }

                left = i * fWidth;

                g.DrawString(code.Substring(i,1), f, b, left, top);
            }


            g.DrawRectangle(new Pen(Color.Gainsboro, 0), 0, 0, image.Width - 1, image.Height - 1);
            g.Dispose();


            image=TwistImage(image, true, 8, 4);

            return image;

        }

        public override void CreateImageOnPage(string code, System.Web.HttpContext context)
        {
            MemoryStream ms = new MemoryStream();
            Bitmap image = this.CreateImageCode(code);
            image.Save(ms, ImageFormat.Jpeg);
            context.Response.ClearContent();
            context.Response.ContentType = "image/Jpeg";
            context.Response.BinaryWrite(ms.GetBuffer());
            ms.Close();
            ms = null;
            image.Dispose();
            image = null;
        }

        public override System.Drawing.Bitmap TwistImage(System.Drawing.Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            Graphics graph = System.Drawing.Graphics.FromImage(destBmp);

            graph.FillRectangle(new SolidBrush(System.Drawing.Color.White), 0, 0, destBmp.Width, destBmp.Height);

            graph.Dispose();
            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;
            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bXDir ? (PI2 * (double)j) / dBaseAxisLen : (PI2 * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);


                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                   System.Drawing.Color color = srcBmp.GetPixel(i, j);
                   if (nOldX >= 0 && nOldX < destBmp.Width&& nOldY >= 0 && nOldY < destBmp.Height)
                   {
                       destBmp.SetPixel(nOldX, nOldY, color);
                   }
                }
           }

           return destBmp;


        }
    }
}
