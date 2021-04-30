using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Links
{
    public class ImagDo
    {

        [DllImport("aocr.dll", EntryPoint = "OCR", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCR(string file, int type);

        [DllImport("aocr.dll", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);

        [DllImport("aocr.dll", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRBarCodes(string file, int type);

        [DllImport("aocr.dll", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);

        public static void imgdo(Bitmap img)
        {
            //去色
            Bitmap btp = img;
            Color c = new Color();
            int rr, gg, bb;
            for (int i = 0; i < btp.Width; i++)
            {
                for (int j = 0; j < btp.Height; j++)
                {
                    //取图片当前的像素点
                    c = btp.GetPixel(i, j);
                    rr = c.R; gg = c.G; bb = c.B;
                    //改变颜色
                    if (rr == 102 && gg == 0 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 153 && gg == 0 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    } if (rr == 153 && gg == 0 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    } if (rr == 153 && gg == 43 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 255 && gg == 255 && bb == 0)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    if (rr == 255 && gg == 255 && bb == 51)
                    {
                        //重新设置当前的像素点
                        btp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            btp.Save("d:\\去除相关颜色.png");
            //灰度
            Bitmap bmphd = btp;
            for (int i = 0; i < bmphd.Width; i++)
            {
                for (int j = 0; j < bmphd.Height; j++)
                {
                    //取图片当前的像素点
                    var color = bmphd.GetPixel(i, j);

                    var gray = (int)(color.R * 0.001 + color.G * 0.700 + color.B * 0.250);

                    //重新设置当前的像素点
                    bmphd.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            bmphd.Save("d:\\灰度.png");
            //二值化
            Bitmap erzhi = bmphd;
            Bitmap orcbmp;
            int nn = 3;
            int w = erzhi.Width;
            int h = erzhi.Height;
            BitmapData data = erzhi.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                byte[,] vSource = new byte[w, h];
                int offset = data.Stride - w * nn;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        vSource[x, y] = (byte)(((int)p[0] + (int)p[1] + (int)p[2]) / 3);
                        p += nn;
                    }
                    p += offset;
                }
                erzhi.UnlockBits(data);

                Bitmap bmpDest = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                BitmapData dataDest = bmpDest.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                p = (byte*)dataDest.Scan0;
                offset = dataDest.Stride - w * nn;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        p[0] = p[1] = p[2] = (int)vSource[x, y] > 161 ? (byte)255 : (byte)0;
                        //p[0] = p[1] = p[2] = (int)GetAverageColor(vSource, x, y, w, h) > 50 ? (byte)255 : (byte)0;
                        p += nn;

                    }
                    p += offset;
                }
                bmpDest.UnlockBits(dataDest);
                orcbmp = bmpDest;
                orcbmp.Save("d:\\二值化.png");

                try
                {
                    int startX = 0, startY = 0;
                    string imgPath = "d:\\二值化.png";
                    Image image = Image.FromFile(imgPath);
                    string a = Marshal.PtrToStringAnsi(OCRpart(imgPath, -1, startX, startY, image.Width, image.Height));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("识别图像错误:" + ex.Message);
                }

            }

        }
    }
}
