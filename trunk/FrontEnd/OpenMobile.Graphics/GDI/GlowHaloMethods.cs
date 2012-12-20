/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
// This class contains code written by Thorsten Gudera, Aragorn257@gmx.de (no specific license)

namespace OpenMobile.Graphics.GDI
{

    public struct PixelData
    {
        public byte blue;
        public byte green;
        public byte red;
        public byte alpha;
    }

    public struct HSLData
    {
        public float Hue;
        public float Saturation;
        public float Luminance;
    }

    public class FRange
    {
        public float Min;
        public float Max;

        public FRange() { }
    }

    public class HSLRange
    {
        public FRange Zrange1;
        public FRange Zrange2;

        public HSLRange()
        {
            Zrange1 = new FRange();
            Zrange2 = new FRange();
        }

        public FRange Range1
        {
            get { return Zrange1; }
            set { Zrange1 = value; }
        }

        public FRange Range2
        {
            get { return Zrange2; }
            set { Zrange2 = value; }
        }

        public bool Contains(float Value)
        {
            if (Value >= Range1.Min && Value <= Range1.Max)
                return true;
            if (Value >= Range2.Min && Value <= Range2.Max)
                return true;
            return false;
        }
    }

    // Rahmenwerk von Microsoft Beispiel
    // Unsafe Image Processing
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp11152001.asp
    // evtl jetzt: http://blogs.msdn.com/ericgu/archive/2007/06/20/lost-column-2-unsafe-image-processing.aspx
    public unsafe class BitmapFunctions
    {
        System.Drawing.Bitmap bitmap;

        int width;
        BitmapData bitmapData = null;
        Byte* pBase = null;

        public BitmapFunctions(System.Drawing.Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public BitmapFunctions()
        {

        }

        public void Dispose()
        {
            if (bitmapData != null)
                UnlockBitmap();
        }

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                return (bitmap);
            }
            set
            {
                this.bitmap = value;
            }
        }

        public Point PixelSize
        {
            get
            {
                GraphicsUnit unit = GraphicsUnit.Pixel;
                RectangleF bounds = bitmap.GetBounds(ref unit);

                return new Point((int)bounds.Width, (int)bounds.Height);
            }
        }

        public void LockBitmap()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = bitmap.GetBounds(ref unit);
            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle((int)boundsF.X,
                (int)boundsF.Y,
                (int)boundsF.Width,
                (int)boundsF.Height);

            // Figure out the number of bytes in a row
            // This is rounded up to be a multiple of 4
            // bytes, since a scan line in an image must always be a multiple of 4 bytes
            // in length. 
            width = (int)boundsF.Width * sizeof(PixelData);
            if (width % 4 != 0)
            {
                width = 4 * (width / 4 + 1);
            }

            bitmapData =
                bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            pBase = (Byte*)bitmapData.Scan0.ToPointer();
        }

        //[CLSCompliant(false)]
        public PixelData* PixelAt(int x, int y)
        {
            return (PixelData*)(pBase + y * width + x * sizeof(PixelData));
        }

        public void UnlockBitmap()
        {
            bitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }

        public static Point ScrollToPic(System.Drawing.Bitmap bmp)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int scanline = bmData.Stride;

            IntPtr Scan0 = bmData.Scan0;

            Point lefttop = new Point();
            bool complete = false;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[3] != 0)
                        {
                            lefttop = new Point(x, y);
                            complete = true;
                            break;
                        }

                        p += 4;
                    }
                    if (complete)
                        break;
                }
            }

            bmp.UnlockBits(bmData);

            return lefttop;
        }

        public void drawOneColor(int nAlpha, int nRed, int nGreen, int nBlue)
        {
            Point size = PixelSize;

            LockBitmap();

            for (int y = 0; y < size.Y; y++)
            {
                PixelData* pPixel = PixelAt(0, y);
                for (int x = 0; x < size.X; x++)
                {
                    int value = nAlpha;
                    if (value < 0) value = 0;
                    if (value > 255) value = 255;
                    pPixel->alpha = (byte)System.Math.Max(System.Math.Min((double)value * ((double)pPixel->alpha / 255.0), 255), 0);

                    int value2 = nRed;
                    if (value2 < 0) value2 = 0;
                    if (value2 > 255) value2 = 255;
                    pPixel->red = (byte)value2;

                    int value3 = nGreen;
                    if (value3 < 0) value3 = 0;
                    if (value3 > 255) value3 = 255;
                    pPixel->green = (byte)value3;

                    int value4 = nBlue;
                    if (value4 < 0) value4 = 0;
                    if (value4 > 255) value4 = 255;
                    pPixel->blue = (byte)value4;
                    pPixel++;
                }
            }
            UnlockBitmap();
        }

        public void Bereich(float HueMin, float HueMax, float Hue, float Saturation, float Luminance, bool AddSaturation, bool AddLuminance,
            float SaturationMin, float SaturationMax, float LuminanceMin, float LuminanceMax)
        {
            Point size = PixelSize;

            LockBitmap();

            for (int y = 0; y < size.Y; y++)
            {
                PixelData* pPixel = PixelAt(0, y);
                for (int x = 0; x < size.X; x++)
                {
                    byte red = pPixel->red;

                    byte green = pPixel->green;

                    byte blue = pPixel->blue;

                    HSLData p = RGBtoHSL(red, green, blue);

                    HSLRange fSat = new HSLRange();
                    if (SaturationMin < SaturationMax)
                    {
                        fSat.Range1.Min = SaturationMin;
                        fSat.Range1.Max = SaturationMax;
                        fSat.Range2.Min = 0;
                        fSat.Range2.Max = 0;
                    }
                    else
                    {
                        fSat.Range1.Min = SaturationMax;
                        fSat.Range1.Max = 1.0F;
                        fSat.Range2.Min = 0;
                        fSat.Range2.Max = SaturationMin;
                    }

                    HSLRange fLum = new HSLRange();
                    if (LuminanceMin < LuminanceMax)
                    {
                        fLum.Range1.Min = LuminanceMin;
                        fLum.Range1.Max = LuminanceMax;
                        fLum.Range2.Min = 0;
                        fLum.Range2.Max = 0;
                    }
                    else
                    {
                        fLum.Range1.Min = LuminanceMax;
                        fLum.Range1.Max = 1.0F;
                        fLum.Range2.Min = 0;
                        fLum.Range2.Max = LuminanceMin;
                    }

                    if (HueMin < HueMax)
                    {
                        if ((p.Hue >= HueMin) && (p.Hue <= HueMax))
                        {
                            if (fSat.Contains(p.Saturation) && fLum.Contains(p.Luminance))
                            {
                                float val_hue = p.Hue + Hue;

                                if (val_hue > 360)
                                {
                                    val_hue -= 360;
                                }

                                p.Hue = val_hue;

                                if (AddSaturation)
                                    p.Saturation += Saturation;
                                else
                                    p.Saturation = Saturation;

                                if (p.Saturation > 1.0F)
                                    p.Saturation = 1.0F;

                                if (p.Saturation < 0.0F)
                                    p.Saturation = 0.0F;

                                if (AddLuminance)
                                    p.Luminance += Luminance;
                                else
                                    p.Luminance = Luminance;

                                if (p.Luminance > 1.0F)
                                    p.Luminance = 1.0F;

                                if (p.Luminance < 0.0F)
                                    p.Luminance = 0.0F;

                                PixelData nPixel = HSLtoRGB(p.Hue, p.Saturation, p.Luminance);

                                pPixel->red = nPixel.red;
                                pPixel->green = nPixel.green;
                                pPixel->blue = nPixel.blue;
                            }
                        }
                    }
                    else
                    {
                        if ((p.Hue >= HueMin) || (p.Hue <= HueMax))
                        {
                            if (fSat.Contains(p.Saturation) && fLum.Contains(p.Luminance))
                            {
                                float val_hue = p.Hue + Hue;

                                if (val_hue > 360)
                                {
                                    val_hue -= 360;
                                }

                                p.Hue = val_hue;

                                if (AddSaturation)
                                    p.Saturation += Saturation;
                                else
                                    p.Saturation = Saturation;

                                if (p.Saturation > 1.0F)
                                    p.Saturation = 1.0F;

                                if (p.Saturation < 0.0F)
                                    p.Saturation = 0.0F;

                                if (AddLuminance)
                                    p.Luminance += Luminance;
                                else
                                    p.Luminance = Luminance;

                                if (p.Luminance > 1.0F)
                                    p.Luminance = 1.0F;

                                if (p.Luminance < 0.0F)
                                    p.Luminance = 0.0F;

                                PixelData nPixel = HSLtoRGB(p.Hue, p.Saturation, p.Luminance);

                                pPixel->red = nPixel.red;
                                pPixel->green = nPixel.green;
                                pPixel->blue = nPixel.blue;
                            }
                        }
                    }

                    pPixel++;
                }
            }
            UnlockBitmap();
        }

        public static HSLData RGBtoHSL(int Red, int Green, int Blue)
        {
            HSLData hsl = new HSLData();

            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, Red, Green, Blue);
            hsl.Hue = c.GetHue();
            hsl.Saturation = c.GetSaturation();
            hsl.Luminance = c.GetBrightness();

            return hsl;
        }

        //see http://www.mpa-garching.mpg.de/MPA-GRAPHICS/hsl-rgb.html
        public static PixelData HSLtoRGB(double H, double S, double L)
        {
            double Temp1 = 0.0, Temp2 = 0.0;
            double r = 0.0, g = 0.0, b = 0.0;

            if (S == 0)
            {
                r = L;
                g = L;
                b = L;
            }
            else
            {
                if (L < 0.5)
                    Temp2 = L * (1.0 + S);
                else
                    Temp2 = (L + S) - (S * L);

                Temp1 = 2.0 * L - Temp2;

                //bischen Spaghetti hier, evtl. in eigene Funktion auslagern

                double hTmp = H / 360.0;
                double rTmp, gTmp, bTmp;

                rTmp = hTmp + (1.0 / 3.0);
                gTmp = hTmp;
                bTmp = hTmp - (1.0 / 3.0);

                if (rTmp < 0.0)
                    rTmp += 1.0;
                if (gTmp < 0.0)
                    gTmp += 1.0;
                if (bTmp < 0.0)
                    bTmp += 1.0;

                if (rTmp > 1.0)
                    rTmp -= 1.0;
                if (gTmp > 1.0)
                    gTmp -= 1.0;
                if (bTmp > 1.0)
                    bTmp -= 1.0;

                if (6.0 * rTmp < 1.0)
                    r = Temp1 + (Temp2 - Temp1) * 6.0 * rTmp;
                else if (2.0 * rTmp < 1.0)
                    r = Temp2;
                else if (3.0 * rTmp < 2.0)
                    r = Temp1 + (Temp2 - Temp1) * ((2.0 / 3.0) - rTmp) * 6.0;
                else
                    r = Temp1;

                if (6.0 * gTmp < 1.0)
                    g = Temp1 + (Temp2 - Temp1) * 6.0 * gTmp;
                else if (2.0 * gTmp < 1.0)
                    g = Temp2;
                else if (3.0 * gTmp < 2.0)
                    g = Temp1 + (Temp2 - Temp1) * ((2.0 / 3.0) - gTmp) * 6.0;
                else
                    g = Temp1;

                if (6.0 * bTmp < 1.0)
                    b = Temp1 + (Temp2 - Temp1) * 6.0 * bTmp;
                else if (2.0 * bTmp < 1.0)
                    b = Temp2;
                else if (3.0 * bTmp < 2.0)
                    b = Temp1 + (Temp2 - Temp1) * ((2.0 / 3.0) - bTmp) * 6.0;
                else
                    b = Temp1;
            }

            PixelData RGB = new PixelData();

            r *= 255.0;
            g *= 255.0;
            b *= 255.0;

            RGB.red = (byte)((int)r);
            RGB.green = (byte)((int)g);
            RGB.blue = (byte)((int)b);

            return RGB;
        }
    }

    public class fipbmp
    {
        public static bool Gaussian_Blur_NxN_Sigma_As_Tolerance(System.Drawing.Bitmap b, int Length, double Weight, int Sigma, bool doTransparency, bool SrcOnSigma, Convolution conv)
        {
            if ((Length & 0x01) != 1)
                return false;

            double[,] Kernel = new double[Length, Length];

            int Radius = Length / 2;

            double a = -2.0 * Radius * Radius / System.Math.Log(Weight);
            double Sum = 0.0;

            for (int y = 0; y < Kernel.GetLength(1); y++)
            {
                for (int x = 0; x < Kernel.GetLength(0); x++)
                {
                    double dist = System.Math.Sqrt((x - Radius) * (x - Radius) + (y - Radius) * (y - Radius));
                    Kernel[x, y] = System.Math.Exp(-dist * dist / a);
                    Sum += Kernel[x, y];
                }
            }

            for (int y = 0; y < Kernel.GetLength(1); y++)
            {
                for (int x = 0; x < Kernel.GetLength(0); x++)
                {
                    Kernel[x, y] /= Sum;
                }
            }

            double[,] AddVals = conv.CalculateStandardAddVals(Kernel);

            return conv.Convolve_Sigma_As_Tolerance(b, Kernel, AddVals, 0, Sigma, doTransparency, SrcOnSigma);
        }

        public static bool FastZGaussian_Blur_NxN(System.Drawing.Bitmap b, int Length, double Weight, int Sigma, bool doTransparency, bool SrcOnSigma, Convolution conv)
        {
            if ((Length & 0x01) != 1)
                return false;

            double[] KernelVector = new double[Length];

            int Radius = Length / 2;

            double a = -2.0 * Radius * Radius / System.Math.Log(Weight);
            double Sum = 0.0;

            for (int x = 0; x < KernelVector.Length; x++)
            {
                double dist = System.Math.Abs(x - Radius);
                KernelVector[x] = System.Math.Exp(-dist * dist / a);
                Sum += KernelVector[x];
            }

            for (int x = 0; x < KernelVector.Length; x++)
            {
                KernelVector[x] /= Sum;
            }

            double[] AddValVector = conv.CalculateStandardAddVals(KernelVector, System.Math.Min(255, b.Width - 1));

            ProgressEventArgs pe = new ProgressEventArgs(b.Height + b.Width, 0, 1);

            //Change to ConvolveH on errors
            conv.ConvolveH_Par(b, KernelVector, AddValVector, 0, Sigma, doTransparency, System.Math.Min(255, b.Width - 1), SrcOnSigma, pe);

            b.RotateFlip(RotateFlipType.Rotate270FlipNone);

            //Change to ConvolveH on errors
            conv.ConvolveH_Par(b, KernelVector, AddValVector, 0, Sigma, doTransparency, System.Math.Min(255, b.Width - 1), SrcOnSigma, pe);

            b.RotateFlip(RotateFlipType.Rotate90FlipNone);

            pe = null;

            return true;
        }

        public static bool Gaussian_Blur_NxN(System.Drawing.Bitmap b, int Length, double Weight, int Sigma, bool doTransparency, bool SrcOnSigma, Convolution conv)
        {
            if ((Length & 0x01) != 1)
                return false;

            double[,] Kernel = new double[Length, Length];

            int Radius = Length / 2;

            double a = -2.0 * Radius * Radius / System.Math.Log(Weight);
            double Sum = 0.0;

            for (int y = 0; y < Kernel.GetLength(1); y++)
            {
                for (int x = 0; x < Kernel.GetLength(0); x++)
                {
                    double dist = System.Math.Sqrt((x - Radius) * (x - Radius) + (y - Radius) * (y - Radius));
                    Kernel[x, y] = System.Math.Exp(-dist * dist / a);
                    Sum += Kernel[x, y];
                }
            }

            for (int y = 0; y < Kernel.GetLength(1); y++)
            {
                for (int x = 0; x < Kernel.GetLength(0); x++)
                {
                    Kernel[x, y] /= Sum;
                }
            }

            double[,] AddVals = conv.CalculateStandardAddVals(Kernel);

            return conv.Convolve(b, Kernel, AddVals, 0, Sigma, doTransparency, SrcOnSigma);
        }

        public static System.Drawing.Bitmap ScanForPic(System.Drawing.Bitmap bmp)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int scanline = bmData.Stride;

            System.IntPtr Scan0 = bmData.Scan0;

            Point top = new Point(), left = new Point(), right = new Point(), bottom = new Point();
            bool complete = false;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[3] != 0)
                        {
                            top = new Point(x, y);
                            complete = true;
                            break;
                        }

                        p += 4;
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int y = bmp.Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            bottom = new Point(x + 1, y + 1);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            left = new Point(x, y);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int x = bmp.Width - 1; x >= 0; x--)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            right = new Point(x + 1, y + 1);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }
            }

            bmp.UnlockBits(bmData);

            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(left.X, top.Y, right.X - left.X, bottom.Y - top.Y);

            System.Drawing.Bitmap b = null;
            System.Drawing.Graphics g = null;

            try
            {
                b = new System.Drawing.Bitmap(rectangle.Width, rectangle.Height);
                g = System.Drawing.Graphics.FromImage(b);
                g.DrawImage(bmp, 0, 0, rectangle, GraphicsUnit.Pixel);
                g.Dispose();

                return b;
            }
            catch
            {
                if (b != null)
                    b.Dispose();
                if (g != null)
                    g.Dispose();

                return null;
            }
        }

        public static Rectangle ScanForPicRcOnly(System.Drawing.Bitmap bmp)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int scanline = bmData.Stride;

            System.IntPtr Scan0 = bmData.Scan0;

            Point top = new Point(), left = new Point(), right = new Point(), bottom = new Point();
            bool complete = false;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[3] != 0)
                        {
                            top = new Point(x, y);
                            complete = true;
                            break;
                        }

                        p += 4;
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int y = bmp.Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            bottom = new Point(x + 1, y + 1);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            left = new Point(x, y);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }

                p = (byte*)(void*)Scan0;
                complete = false;

                for (int x = bmp.Width - 1; x >= 0; x--)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (p[x * 4 + y * scanline + 3] != 0)
                        {
                            right = new Point(x + 1, y + 1);
                            complete = true;
                            break;
                        }
                    }
                    if (complete)
                        break;
                }
            }

            bmp.UnlockBits(bmData);

            Rectangle rectangle = new Rectangle(left.X, top.Y, right.X - left.X, bottom.Y - top.Y);

            return rectangle;
        }

        public static unsafe void IncreaseAlpha(System.Drawing.Bitmap bmp, double amount)
        {
            BitmapData bmData = null;

            try
            {
                bmData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                int w = bmp.Width;
                int h = bmp.Height;

                byte[] alphaGamma = new byte[256];

                for (int i = 0; i < 256; i++)
                    alphaGamma[i] = (byte)System.Math.Min(255, (int)((255.0 * System.Math.Pow(i / 255.0, 1.0 / amount)) + 0.5));

                for (int y = 0; y < bmData.Height; y++)
                //Parallel.For(0, h, y =>
                {
                    byte* p = (byte*)bmData.Scan0.ToPointer();
                    p += (y * bmData.Stride);

                    for (int x = 0; x < w; x++)
                    {
                        if (p[3] > 0)
                        {
                            int value = alphaGamma[p[3]];
                            if (value < 0) value = 0;
                            if (value > 255) value = 255;
                            p[3] = (byte)value;
                        }

                        p += 4;
                    }
                }//);

                bmp.UnlockBits(bmData);
            }
            catch
            {
                try
                {
                    bmp.UnlockBits(bmData);
                }
                catch
                {

                }
            }
        }
    }

    //Kleine Convolution Klasse für eindimensionale und quadratische Kernel. Geschrieben von Thorsten Gudera, Aragorn257@gmx.de
    //Credits: Christian Graus: Image Processing for Dummies with C# and GDI+ Part 2 - Convolution Filters
    // http://www.codeproject.com/cs/media/csharpfilters.asp?target=image%7Cprocessing%7Cfor%7Cdummies
    //Volkmar Miszalok: http://www.miszalok.de/Lectures/L07_ImageProcessing/IP_Filters/IP_Filters_deutsch.htm
    public class Convolution
    {
        public delegate void ProgressPlusEventHandler(ProgressEventArgs e);
        public event ProgressPlusEventHandler ProgressPlus;

        public object lockObject = new object();

        public double[,] CalculateStandardAddVals(double[,] Kernel)
        {
            if (Kernel.GetLength(0) != Kernel.GetLength(1))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if ((Kernel.GetLength(0) & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }

            double[,] AddVals = new double[Kernel.GetLength(0), Kernel.GetLength(1)];

            int h = Kernel.GetLength(0) / 2;

            double KSum = 0.0;

            for (int i = 0; i < Kernel.GetLength(1); i++)
            {
                for (int j = 0; j < Kernel.GetLength(0); j++)
                {
                    KSum += Kernel[j, i];
                }
            }

            for (int l = 0; l < h; l++)
            {
                double Sum = 0.0;

                for (int r = h - l; r < Kernel.GetLength(1); r++)
                {
                    for (int c = h; c < Kernel.GetLength(0); c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[0, l] = KSum - Sum;

                for (int x = 1; x < Kernel.GetLength(0) - 1; x++)
                {
                    Sum = 0.0;

                    for (int r = h - l; r < Kernel.GetLength(1); r++)
                    {
                        for (int c = System.Math.Max(h - x, 0); c < Kernel.GetLength(0) + System.Math.Min(h - x, 0); c++)
                        {
                            Sum += Kernel[c, r];
                        }
                    }

                    AddVals[x, l] = KSum - Sum;
                }

                Sum = 0.0;

                for (int r = h - l; r < Kernel.GetLength(1); r++)
                {
                    for (int c = 0; c < Kernel.GetLength(0) - h; c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[Kernel.GetLength(0) - 1, l] = KSum - Sum;

                //############
                Sum = 0.0;

                for (int r = 0; r < Kernel.GetLength(1); r++)
                {
                    for (int c = h - l; c < Kernel.GetLength(0); c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[l, h] = KSum - Sum;

                Sum = 0.0;

                for (int r = 0; r < Kernel.GetLength(1); r++)
                {
                    for (int c = 0; c < Kernel.GetLength(0) - l - 1; c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[h + 1 + l, h] = KSum - Sum;
                //############

                Sum = 0.0;

                for (int r = 0; r < Kernel.GetLength(1) - (l + 1); r++)
                {
                    for (int c = h; c < Kernel.GetLength(0); c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[0, h + 1 + l] = KSum - Sum;

                for (int x = 1; x < Kernel.GetLength(0) - 1; x++)
                {
                    Sum = 0.0;

                    for (int r = 0; r < Kernel.GetLength(1) - (l + 1); r++)
                    {
                        for (int c = System.Math.Max(h - x, 0); c < Kernel.GetLength(0) + System.Math.Min(h - x, 0); c++)
                        {
                            Sum += Kernel[c, r];
                        }
                    }

                    AddVals[x, h + 1 + l] = KSum - Sum;
                }

                Sum = 0.0;

                for (int r = 0; r < Kernel.GetLength(1) - (l + 1); r++)
                {
                    for (int c = 0; c < Kernel.GetLength(0) - h; c++)
                    {
                        Sum += Kernel[c, r];
                    }
                }
                AddVals[Kernel.GetLength(0) - 1, h + 1 + l] = KSum - Sum;
            }

            return AddVals;
        }

        public double[] CalculateStandardAddVals(double[] Kernel, int MaxVal)
        {
            if ((Kernel.Length & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }
            if (Kernel.Length < 3)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }
            if (Kernel.Length > MaxVal)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }

            double[] AddVals = new double[Kernel.Length];

            int h = Kernel.GetLength(0) / 2;

            double KSum = 0.0;

            for (int i = 0; i < Kernel.Length; i++)
            {
                KSum += Kernel[i];
            }

            double Sum = 0.0;

            for (int c = h; c < Kernel.Length; c++)
            {
                Sum += Kernel[c];
            }

            AddVals[0] = KSum - Sum;

            for (int x = 1; x < Kernel.Length - 1; x++)
            {
                Sum = 0.0;

                for (int c = System.Math.Max(h - x, 0); c < Kernel.Length + System.Math.Min(h - x, 0); c++)
                {
                    Sum += Kernel[c];
                }

                AddVals[x] = KSum - Sum;
            }

            Sum = 0.0;

            for (int c = 0; c < Kernel.Length - h; c++)
            {
                Sum += Kernel[c];
            }

            AddVals[Kernel.Length - 1] = KSum - Sum;

            return AddVals;
        }

        //Der Sigmafilter funktioniert normalerweise nur mit ungewichteten Kernels. Bei gewichteten kommt es zwangsläufig zu Fehlern, die sich
        //in Farbfehlern, oder haloartigen Strukturen zeigen. Man kann aber ganz ordentliche Ergebnisse erzielen, durch das setzen der Variable
        //SrcOnSigma (auf true), dann wird bei entsprechenden Abständen schlichtweg der (Farb-)Wert aus dem Originalbild genommen.
        public bool Convolve_Sigma_As_Tolerance(System.Drawing.Bitmap b, double[,] Kernel, double[,] AddVals, int Bias, int Sigma, bool DoTrans, bool SrcOnSigma)
        {
            if (Kernel.GetLength(0) != Kernel.GetLength(1))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (AddVals.GetLength(0) != AddVals.GetLength(1))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (AddVals.GetLength(0) != Kernel.GetLength(0))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if ((Kernel.GetLength(0) & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }
            if (Kernel.GetLength(0) < 3)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to" + (System.Math.Min(b.Width - 1, b.Height - 1)).ToString() + ".");
            }
            if (Kernel.GetLength(0) > System.Math.Min(b.Width - 1, b.Height - 1))
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to" + (System.Math.Min(b.Width - 1, b.Height - 1)).ToString() + ".");
            }

            int h = Kernel.GetLength(0) / 2;

            System.Drawing.Bitmap bSrc = null;
            BitmapData bmData = null;
            BitmapData bmSrc = null;

            try
            {
                bSrc = (System.Drawing.Bitmap)b.Clone();
                bmData = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height),
                                           ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bmSrc = bSrc.LockBits(new System.Drawing.Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmData.Stride;

                System.IntPtr Scan0 = bmData.Scan0;
                System.IntPtr SrcScan0 = bmSrc.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    byte* pSrc = (byte*)(void*)SrcScan0;

                    int nWidth = b.Width;
                    int nHeight = b.Height;

                    double Sum = 0.0, Sum2 = 0.0, Sum3 = 0.0, Sum4 = 0.0, KSum = 0.0;
                    double count = 0.0, count2 = 0.0, count3 = 0.0, count4 = 0.0;
                    double z = 1.0 / (Kernel.GetLength(0) * Kernel.GetLength(1));

                    # region First Part
                    for (int l = 0; l < h; l++)
                    {
                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;

                        p = (byte*)(void*)Scan0;
                        pSrc = (byte*)(void*)SrcScan0;

                        bool ignore = false;

                        for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                        {
                            for (int c = h, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (h - l)) * (Kernel.GetLength(0) - h));

                                if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[1 + (l * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[2 + (l * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[3 + (l * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;

                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;

                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;

                                    if (DoTrans)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                }
                                else
                                    ignore = true;

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[0, l];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (l * stride);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                            {
                                p[0] = pSrc[0 + (l * stride) + (l * 4)];
                                p[1] = pSrc[1 + (l * stride) + (l * 4)];
                                p[2] = pSrc[2 + (l * stride) + (l * 4)];
                                p[3] = pSrc[3 + (l * stride) + (l * 4)];
                            }
                        }

                        for (int x = 1; x < nWidth - 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;

                            p = (byte*)(void*)Scan0;
                            pSrc = (byte*)(void*)SrcScan0;

                            if (x > Kernel.GetLength(0) - (h + 1))
                            {
                                p += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                                pSrc += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                            }

                            ignore = false;

                            for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = System.Math.Max(h - x, 0), cc = 0; (x - h + Kernel.GetLength(0) <= b.Width) ? c < Kernel.GetLength(0) : c < (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width)); c++, cc++)
                                {
                                    double zz = (x - h + Kernel.GetLength(0) <= b.Width) ? Kernel.GetLength(0) : (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width));
                                    z = 1.0 / ((zz - System.Math.Max(h - x, 0)) * (Kernel.GetLength(1) - (h - l)));

                                    if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                    {
                                        Sum += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;

                                        Sum2 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;

                                        Sum3 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;

                                        if (DoTrans)
                                        {
                                            Sum4 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                    }
                                    else
                                        ignore = true;

                                    if (r == h && c == h)
                                    {
                                        if (x - h + Kernel.GetLength(0) <= b.Width)
                                            KSum += Kernel[c, r] + AddVals[System.Math.Min(x, h), l];
                                        else
                                            KSum += Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l];
                                    }
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p = (byte*)(void*)Scan0;
                            p += (l * stride) + (x * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                {
                                    p[0] = pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[1] = pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[2] = pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[3] = pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                }
                            }
                        }

                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;

                        p = (byte*)(void*)Scan0;
                        p += (nWidth - h - 1) * 4;

                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nWidth - h - 1) * 4;

                        ignore = false;

                        for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                        {
                            for (int c = 0, cc = 0; c < Kernel.GetLength(0) - h; c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (h - l)) * (Kernel.GetLength(0) - h));

                                if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[1 + (l * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[2 + (l * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[3 + (l * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;

                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;

                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;

                                    if (DoTrans)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                }
                                else
                                    ignore = true;

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (l * stride) + (h * 4);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                            {
                                p[0] = pSrc[0 + (l * stride) + (h * 4)];
                                p[1] = pSrc[1 + (l * stride) + (h * 4)];
                                p[2] = pSrc[2 + (l * stride) + (h * 4)];
                                p[3] = pSrc[3 + (l * stride) + (h * 4)];
                            }
                        }
                    }
                    # endregion

                    # region Main Body

                    for (int y = 0; y < nHeight - Kernel.GetLength(1) + 1; y++)
                    {
                        # region First Pixels
                        for (int l = 0; l < h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;

                            p = (byte*)(void*)Scan0;
                            p += y * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride;

                            bool ignore = false;

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = h - l, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                                {
                                    z = 1.0 / (Kernel.GetLength(1) * (Kernel.GetLength(0) - (h - l)));

                                    if ((System.Math.Abs((int)pSrc[0 + (h * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[1 + (h * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[2 + (h * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[3 + (h * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                    {
                                        Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;

                                        Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;

                                        Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;

                                        if (DoTrans)
                                        {
                                            Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                    }
                                    else
                                        ignore = true;

                                    if (r == h && c == h)
                                        KSum += Kernel[c, r] + AddVals[l, r];
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (l * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                {
                                    p[0] = pSrc[0 + (h * stride) + (l * 4)];
                                    p[1] = pSrc[1 + (h * stride) + (l * 4)];
                                    p[2] = pSrc[2 + (h * stride) + (l * 4)];
                                    p[3] = pSrc[3 + (h * stride) + (l * 4)];
                                }
                            }
                        }
                        # endregion

                        # region Standard
                        z = 1.0 / (Kernel.GetLength(0) * Kernel.GetLength(1));

                        for (int x = 0; x < nWidth - Kernel.GetLength(0) + 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;

                            p = (byte*)(void*)Scan0;
                            p += y * stride + x * 4;

                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride + x * 4;

                            bool ignore = false;

                            for (int r = 0; r < Kernel.GetLength(1); r++)
                            {
                                for (int c = 0; c < Kernel.GetLength(0); c++)
                                {
                                    if ((System.Math.Abs((int)pSrc[0 + (h * stride) + (h * 4)] - (int)pSrc[0 + (r * stride) + (c * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[1 + (h * stride) + (h * 4)] - (int)pSrc[1 + (r * stride) + (c * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[2 + (h * stride) + (h * 4)] - (int)pSrc[2 + (r * stride) + (c * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[3 + (h * stride) + (h * 4)] - (int)pSrc[3 + (r * stride) + (c * 4)]) <= Sigma))
                                    {
                                        Sum += ((double)pSrc[0 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count += z;

                                        Sum2 += ((double)pSrc[1 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count2 += z;

                                        Sum3 += ((double)pSrc[2 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count3 += z;

                                        if (DoTrans)
                                        {
                                            Sum4 += ((double)pSrc[3 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                    }
                                    else
                                        ignore = true;

                                    KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                {
                                    p[0] = pSrc[0 + (h * stride) + (h * 4)];
                                    p[1] = pSrc[1 + (h * stride) + (h * 4)];
                                    p[2] = pSrc[2 + (h * stride) + (h * 4)];
                                    p[3] = pSrc[3 + (h * stride) + (h * 4)];
                                }
                            }
                        }
                        # endregion

                        # region Last Pixels
                        for (int l = nWidth - Kernel.GetLength(0) + 1; l < nWidth - h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;

                            p = (byte*)(void*)Scan0;
                            p += (y * stride) + (l * 4);
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (y * stride) + (l * 4);

                            bool ignore = false;

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = 0, cc = 0; c < nWidth - l; c++, cc++)
                                {
                                    z = 1.0 / (Kernel.GetLength(1) * (nWidth - l));

                                    if ((System.Math.Abs((int)pSrc[0 + (h * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[1 + (h * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[2 + (h * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[3 + (h * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                    {
                                        Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;

                                        Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;

                                        Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;

                                        if (DoTrans)
                                        {
                                            Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                    }
                                    else
                                        ignore = true;

                                    if (r == h && c == h)
                                        KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r];
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                {
                                    p[0] = pSrc[0 + (h * stride) + (h * 4)];
                                    p[1] = pSrc[1 + (h * stride) + (h * 4)];
                                    p[2] = pSrc[2 + (h * stride) + (h * 4)];
                                    p[3] = pSrc[3 + (h * stride) + (h * 4)];
                                }
                            }
                        }
                        # endregion
                    }
                    # endregion

                    # region Last Part
                    for (int l = 0; l < h; l++)
                    {
                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;

                        p = (byte*)(void*)Scan0;
                        p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;

                        bool ignore = false;

                        for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                        {
                            for (int c = h, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (l + 1)) * (Kernel.GetLength(0) - h));

                                if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[1 + (l * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[2 + (l * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[3 + (l * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;

                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;

                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;

                                    if (DoTrans)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                }
                                else
                                    ignore = true;

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[0, h + l + 1];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (h * stride);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                            {
                                p[0] = pSrc[0 + (l * stride) + (l * 4)];
                                p[1] = pSrc[1 + (l * stride) + (l * 4)];
                                p[2] = pSrc[2 + (l * stride) + (l * 4)];
                                p[3] = pSrc[3 + (l * stride) + (l * 4)];
                            }
                        }

                        for (int x = 1; x < nWidth - 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;

                            p = (byte*)(void*)Scan0;
                            p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;

                            if (x > Kernel.GetLength(0) - (h + 1))
                            {
                                p += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                                pSrc += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                            }

                            ignore = false;

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                            {
                                for (int c = System.Math.Max(h - x, 0), cc = 0; (x - h + Kernel.GetLength(0) <= b.Width) ? c < Kernel.GetLength(0) : c < (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width)); c++, cc++)
                                {
                                    double zz = (x - h + Kernel.GetLength(0) <= b.Width) ? Kernel.GetLength(0) : (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width));
                                    z = 1.0 / ((zz - System.Math.Max(h - x, 0)) * (Kernel.GetLength(1) - (l + 1)));

                                    if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                        (System.Math.Abs((int)pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                    {
                                        Sum += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;

                                        Sum2 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;

                                        Sum3 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;

                                        if (DoTrans)
                                        {
                                            Sum4 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                    }
                                    else
                                        ignore = true;

                                    if (r == h && c == h)
                                    {
                                        if (x - h + Kernel.GetLength(0) <= b.Width)
                                            KSum += Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1];
                                        else
                                            KSum += Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1];
                                    }
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p = (byte*)(void*)Scan0;
                            p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                            p += (h * stride) + (x * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                {
                                    p[0] = pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[1] = pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[2] = pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                    p[3] = pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                }
                            }
                        }

                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;

                        p = (byte*)(void*)Scan0;
                        p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        p += (nWidth - h - 1) * 4;

                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        pSrc += (nWidth - h - 1) * 4;

                        ignore = false;

                        for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                        {
                            for (int c = 0, cc = 0; c < Kernel.GetLength(0) - h; c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (l + 1)) * (Kernel.GetLength(0) - h));

                                if ((System.Math.Abs((int)pSrc[0 + (l * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[1 + (l * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[2 + (l * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma) &&
                                    (System.Math.Abs((int)pSrc[3 + (l * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma))
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;

                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;

                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;

                                    if (DoTrans)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                }
                                else
                                    ignore = true;

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (h * stride) + (h * 4);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                            {
                                p[0] = pSrc[0 + (l * stride) + (h * 4)];
                                p[1] = pSrc[1 + (l * stride) + (h * 4)];
                                p[2] = pSrc[2 + (l * stride) + (h * 4)];
                                p[3] = pSrc[3 + (l * stride) + (h * 4)];
                            }
                        }
                    }
                    # endregion
                }

                b.UnlockBits(bmData);
                bSrc.UnlockBits(bmSrc);
                bSrc.Dispose();

                return true;
            }
            catch
            {
                try
                {
                    b.UnlockBits(bmData);
                }
                catch
                {

                }

                try
                {
                    bSrc.UnlockBits(bmSrc);
                }
                catch
                {

                }

                if (bSrc != null)
                {
                    bSrc.Dispose();
                    bSrc = null;
                }
            }
            return false;
        }

        //Der Sigmafilter funktioniert normalerweise nur mit ungewichteten Kernels. Bei gewichteten kommt es zwangsläufig zu Fehlern, die sich
        //in Farbfehlern, oder haloartigen Strukturen zeigen. Man kann aber ganz ordentliche Ergebnisse erzielen, durch das setzen der Variable
        //SrcOnSigma (auf true), dann wird bei entsprechenden Abständen schlichtweg der (Farb-)Wert aus dem Originalbild genommen.
        public bool Convolve(System.Drawing.Bitmap b, double[,] Kernel, double[,] AddVals, int Bias, int Sigma, bool DoTrans, bool SrcOnSigma)
        {
            if (Kernel.GetLength(0) != Kernel.GetLength(1))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (AddVals.GetLength(0) != AddVals.GetLength(1))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (AddVals.GetLength(0) != Kernel.GetLength(0))
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if ((Kernel.GetLength(0) & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }
            if (Kernel.GetLength(0) < 3)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to" + (System.Math.Min(b.Width - 1, b.Height - 1)).ToString() + ".");
            }
            if (Kernel.GetLength(0) > System.Math.Min(b.Width - 1, b.Height - 1))
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to" + (System.Math.Min(b.Width - 1, b.Height - 1)).ToString() + ".");
            }

            int h = Kernel.GetLength(0) / 2;

            System.Drawing.Bitmap bSrc = null;
            BitmapData bmData = null;
            BitmapData bmSrc = null;

            try
            {
                bSrc = (System.Drawing.Bitmap)b.Clone();
                bmData = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height),
                                           ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bmSrc = bSrc.LockBits(new System.Drawing.Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmData.Stride;

                System.IntPtr Scan0 = bmData.Scan0;
                System.IntPtr SrcScan0 = bmSrc.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    byte* pSrc = (byte*)(void*)SrcScan0;

                    int nWidth = b.Width;
                    int nHeight = b.Height;

                    double Sum = 0.0, Sum2 = 0.0, Sum3 = 0.0, Sum4 = 0.0, KSum = 0.0;
                    double count = 0.0, count2 = 0.0, count3 = 0.0, count4 = 0.0;
                    double z = 1.0 / (Kernel.GetLength(0) * Kernel.GetLength(1));

                    # region First Part
                    for (int l = 0; l < h; l++)
                    {
                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;
                        bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                        p = (byte*)(void*)Scan0;
                        pSrc = (byte*)(void*)SrcScan0;

                        for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                        {
                            for (int c = h, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (h - l)) * (Kernel.GetLength(0) - h));

                                if (System.Math.Abs((int)pSrc[0 + (l * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[0, l];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (l * stride);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                                p[0] = pSrc[0 + (l * stride) + (l * 4)];
                            if (ignore2)
                                p[1] = pSrc[1 + (l * stride) + (l * 4)];
                            if (ignore3)
                                p[2] = pSrc[2 + (l * stride) + (l * 4)];
                            if (ignore4)
                                p[3] = pSrc[3 + (l * stride) + (l * 4)];
                        }

                        for (int x = 1; x < nWidth - 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            ignore = ignore2 = ignore3 = ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            pSrc = (byte*)(void*)SrcScan0;

                            if (x > Kernel.GetLength(0) - (h + 1))
                            {
                                p += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                                pSrc += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                            }

                            for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = System.Math.Max(h - x, 0), cc = 0; (x - h + Kernel.GetLength(0) <= b.Width) ? c < Kernel.GetLength(0) : c < (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width)); c++, cc++)
                                {
                                    double zz = (x - h + Kernel.GetLength(0) <= b.Width) ? Kernel.GetLength(0) : (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width));
                                    z = 1.0 / ((zz - System.Math.Max(h - x, 0)) * (Kernel.GetLength(1) - (h - l)));

                                    if (System.Math.Abs((int)pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;
                                    }
                                    else
                                        ignore = true;

                                    if (System.Math.Abs((int)pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum2 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;
                                    }
                                    else
                                        ignore2 = true;

                                    if (System.Math.Abs((int)pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum3 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;
                                    }
                                    else
                                        ignore3 = true;

                                    if (DoTrans)
                                    {
                                        if (System.Math.Abs((int)pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                        {
                                            Sum4 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l]))) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                        else
                                            ignore4 = true;
                                    }

                                    if (r == h && c == h)
                                    {
                                        if (x - h + Kernel.GetLength(0) <= b.Width)
                                            KSum += Kernel[c, r] + AddVals[System.Math.Min(x, h), l];
                                        else
                                            KSum += Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), l];
                                    }
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p = (byte*)(void*)Scan0;
                            p += (l * stride) + (x * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)];
                            }
                        }

                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;
                        ignore = ignore2 = ignore3 = ignore4 = false;

                        p = (byte*)(void*)Scan0;
                        p += (nWidth - h - 1) * 4;

                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nWidth - h - 1) * 4;

                        for (int r = h - l, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                        {
                            for (int c = 0, cc = 0; c < Kernel.GetLength(0) - h; c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (h - l)) * (Kernel.GetLength(0) - h));

                                if (System.Math.Abs((int)pSrc[0 + (l * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, l];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (l * stride) + (h * 4);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                                p[0] = pSrc[0 + (l * stride) + (h * 4)];
                            if (ignore2)
                                p[1] = pSrc[1 + (l * stride) + (h * 4)];
                            if (ignore3)
                                p[2] = pSrc[2 + (l * stride) + (h * 4)];
                            if (ignore4)
                                p[3] = pSrc[3 + (l * stride) + (h * 4)];
                        }
                    }
                    # endregion

                    # region Main Body

                    for (int y = 0; y < nHeight - Kernel.GetLength(1) + 1; y++)
                    {
                        # region First Pixels
                        for (int l = 0; l < h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride;

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = h - l, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                                {
                                    z = 1.0 / (Kernel.GetLength(1) * (Kernel.GetLength(0) - (h - l)));

                                    if (System.Math.Abs((int)pSrc[0 + (h * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;
                                    }
                                    else
                                        ignore = true;

                                    if (System.Math.Abs((int)pSrc[1 + (h * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;
                                    }
                                    else
                                        ignore2 = true;

                                    if (System.Math.Abs((int)pSrc[2 + (h * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;
                                    }
                                    else
                                        ignore3 = true;

                                    if (DoTrans)
                                    {
                                        if (System.Math.Abs((int)pSrc[3 + (h * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                        {
                                            Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[l, r])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                        else
                                            ignore4 = true;
                                    }

                                    if (r == h && c == h)
                                        KSum += Kernel[c, r] + AddVals[l, r];
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (l * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * stride) + (l * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * stride) + (l * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * stride) + (l * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * stride) + (l * 4)];
                            }
                        }
                        # endregion

                        # region Standard
                        z = 1.0 / (Kernel.GetLength(0) * Kernel.GetLength(1));

                        for (int x = 0; x < nWidth - Kernel.GetLength(0) + 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride + x * 4;

                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride + x * 4;

                            for (int r = 0; r < Kernel.GetLength(1); r++)
                            {
                                for (int c = 0; c < Kernel.GetLength(0); c++)
                                {
                                    if (System.Math.Abs((int)pSrc[0 + (h * stride) + (h * 4)] - (int)pSrc[0 + (r * stride) + (c * 4)]) <= Sigma)
                                    {
                                        Sum += ((double)pSrc[0 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count += z;
                                    }
                                    else
                                        ignore = true;

                                    if (System.Math.Abs((int)pSrc[1 + (h * stride) + (h * 4)] - (int)pSrc[1 + (r * stride) + (c * 4)]) <= Sigma)
                                    {
                                        Sum2 += ((double)pSrc[1 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count2 += z;
                                    }
                                    else
                                        ignore2 = true;

                                    if (System.Math.Abs((int)pSrc[2 + (h * stride) + (h * 4)] - (int)pSrc[2 + (r * stride) + (c * 4)]) <= Sigma)
                                    {
                                        Sum3 += ((double)pSrc[2 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                        count3 += z;
                                    }
                                    else
                                        ignore3 = true;

                                    if (DoTrans)
                                    {
                                        if (System.Math.Abs((int)pSrc[3 + (h * stride) + (h * 4)] - (int)pSrc[3 + (r * stride) + (c * 4)]) <= Sigma)
                                        {
                                            Sum4 += ((double)pSrc[3 + (r * stride) + (c * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                        else
                                            ignore4 = true;
                                    }

                                    KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * stride) + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * stride) + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * stride) + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * stride) + (h * 4)];
                            }
                        }
                        # endregion

                        # region Last Pixels
                        for (int l = nWidth - Kernel.GetLength(0) + 1; l < nWidth - h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += (y * stride) + (l * 4);
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (y * stride) + (l * 4);

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1); r++, rc++)
                            {
                                for (int c = 0, cc = 0; c < nWidth - l; c++, cc++)
                                {
                                    z = 1.0 / (Kernel.GetLength(1) * (nWidth - l));

                                    if (System.Math.Abs((int)pSrc[0 + (h * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;
                                    }
                                    else
                                        ignore = true;

                                    if (System.Math.Abs((int)pSrc[1 + (h * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;
                                    }
                                    else
                                        ignore2 = true;

                                    if (System.Math.Abs((int)pSrc[2 + (h * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;
                                    }
                                    else
                                        ignore3 = true;

                                    if (DoTrans)
                                    {
                                        if (System.Math.Abs((int)pSrc[3 + (h * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                        {
                                            Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                        else
                                            ignore4 = true;
                                    }

                                    if (r == h && c == h)
                                        KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - (nWidth - l) + h, r];
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * stride) + (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * stride) + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * stride) + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * stride) + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * stride) + (h * 4)];
                            }
                        }
                        # endregion
                    }
                    # endregion

                    # region Last Part
                    for (int l = 0; l < h; l++)
                    {
                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;
                        bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                        p = (byte*)(void*)Scan0;
                        p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;

                        for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                        {
                            for (int c = h, cc = 0; c < Kernel.GetLength(0); c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (l + 1)) * (Kernel.GetLength(0) - h));

                                if (System.Math.Abs((int)pSrc[0 + (l * stride) + (l * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * stride) + (l * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * stride) + (l * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * stride) + (l * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[0, h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[0, h + l + 1];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (h * stride);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                                p[0] = pSrc[0 + (l * stride) + (l * 4)];
                            if (ignore2)
                                p[1] = pSrc[1 + (l * stride) + (l * 4)];
                            if (ignore3)
                                p[2] = pSrc[2 + (l * stride) + (l * 4)];
                            if (ignore4)
                                p[3] = pSrc[3 + (l * stride) + (l * 4)];
                        }

                        for (int x = 1; x < nWidth - 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            ignore = ignore2 = ignore3 = ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;

                            if (x > Kernel.GetLength(0) - (h + 1))
                            {
                                p += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                                pSrc += (x - Kernel.GetLength(0) + (h + 1)) * 4;
                            }

                            for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                            {
                                for (int c = System.Math.Max(h - x, 0), cc = 0; (x - h + Kernel.GetLength(0) <= b.Width) ? c < Kernel.GetLength(0) : c < (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width)); c++, cc++)
                                {
                                    double zz = (x - h + Kernel.GetLength(0) <= b.Width) ? Kernel.GetLength(0) : (Kernel.GetLength(0) - (x - h + Kernel.GetLength(0) - b.Width));
                                    z = 1.0 / ((zz - System.Math.Max(h - x, 0)) * (Kernel.GetLength(1) - (l + 1)));

                                    if (System.Math.Abs((int)pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count += z;
                                    }
                                    else
                                        ignore = true;

                                    if (System.Math.Abs((int)pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum2 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count2 += z;
                                    }
                                    else
                                        ignore2 = true;

                                    if (System.Math.Abs((int)pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum3 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count3 += z;
                                    }
                                    else
                                        ignore3 = true;

                                    if (DoTrans)
                                    {
                                        if (System.Math.Abs((int)pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                        {
                                            Sum4 += (r == h && c == h) ? ((x - h + Kernel.GetLength(0) <= b.Width) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1]))) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                            count4 += z;
                                        }
                                        else
                                            ignore4 = true;
                                    }

                                    if (r == h && c == h)
                                    {
                                        if (x - h + Kernel.GetLength(0) <= b.Width)
                                            KSum += Kernel[c, r] + AddVals[System.Math.Min(x, h), h + l + 1];
                                        else
                                            KSum += Kernel[c, r] + AddVals[(x + Kernel.GetLength(0) - b.Width), h + l + 1];
                                    }
                                    else
                                        KSum += Kernel[c, r];
                                }
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p = (byte*)(void*)Scan0;
                            p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                            p += (h * stride) + (x * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (l * stride) + (System.Math.Min(x, h) * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (l * stride) + (System.Math.Min(x, h) * 4)];
                            }
                        }

                        Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                        count = count2 = count3 = count4 = 0.0;
                        ignore = ignore2 = ignore3 = ignore4 = false;

                        p = (byte*)(void*)Scan0;
                        p += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        p += (nWidth - h - 1) * 4;

                        pSrc = (byte*)(void*)SrcScan0;
                        pSrc += (nHeight - Kernel.GetLength(1) + l + 1) * stride;
                        pSrc += (nWidth - h - 1) * 4;

                        for (int r = 0, rc = 0; r < Kernel.GetLength(1) - (l + 1); r++, rc++)
                        {
                            for (int c = 0, cc = 0; c < Kernel.GetLength(0) - h; c++, cc++)
                            {
                                z = 1.0 / ((Kernel.GetLength(1) - (l + 1)) * (Kernel.GetLength(0) - h));

                                if (System.Math.Abs((int)pSrc[0 + (l * stride) + (h * 4)] - (int)pSrc[0 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (r == h && c == h) ? ((double)pSrc[0 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[0 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * stride) + (h * 4)] - (int)pSrc[1 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (r == h && c == h) ? ((double)pSrc[1 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[1 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * stride) + (h * 4)] - (int)pSrc[2 + (rc * stride) + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (r == h && c == h) ? ((double)pSrc[2 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[2 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * stride) + (h * 4)] - (int)pSrc[3 + (rc * stride) + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (r == h && c == h) ? ((double)pSrc[3 + (rc * stride) + (cc * 4)] * (Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1])) : ((double)pSrc[3 + (rc * stride) + (cc * 4)] * Kernel[c, r]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (r == h && c == h)
                                    KSum += Kernel[c, r] + AddVals[Kernel.GetLength(0) - 1, h + l + 1];
                                else
                                    KSum += Kernel[c, r];
                            }
                        }

                        if (KSum == 0.0)
                            KSum = 1.0;

                        if (count == 0.0) count = 1.0;
                        if (count2 == 0.0) count2 = 1.0;
                        if (count3 == 0.0) count3 = 1.0;
                        if (count4 == 0.0) count4 = 1.0;

                        p += (h * stride) + (h * 4);
                        p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                        p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                        p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                        if (DoTrans)
                            p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                        if (SrcOnSigma)
                        {
                            if (ignore)
                                p[0] = pSrc[0 + (l * stride) + (h * 4)];
                            if (ignore2)
                                p[1] = pSrc[1 + (l * stride) + (h * 4)];
                            if (ignore3)
                                p[2] = pSrc[2 + (l * stride) + (h * 4)];
                            if (ignore4)
                                p[3] = pSrc[3 + (l * stride) + (h * 4)];
                        }
                    }
                    # endregion
                }

                b.UnlockBits(bmData);
                bSrc.UnlockBits(bmSrc);
                bSrc.Dispose();

                return true;
            }
            catch
            {
                try
                {
                    b.UnlockBits(bmData);
                }
                catch
                {

                }

                try
                {
                    bSrc.UnlockBits(bmSrc);
                }
                catch
                {

                }

                if (bSrc != null)
                {
                    bSrc.Dispose();
                    bSrc = null;
                }
            }
            return false;
        }

        //Der Sigmafilter funktioniert normalerweise nur mit ungewichteten Kernels. Bei gewichteten kommt es zwangsläufig zu Fehlern, die sich
        //in Farbfehlern, oder haloartigen Strukturen zeigen. Man kann aber ganz ordentliche Ergebnisse erzielen, durch das setzen der Variable
        //SrcOnSigma (auf true), dann wird bei entsprechenden Abständen schlichtweg der (Farb-)Wert aus dem Originalbild genommen.
        public bool ConvolveH(System.Drawing.Bitmap b, double[] Kernel, double[] AddVals, int Bias, int Sigma, bool DoTrans, int MaxVal, bool SrcOnSigma, ProgressEventArgs pe)
        {
            if ((Kernel.Length & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }
            if (AddVals.Length != Kernel.Length)
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (Kernel.Length < 3)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }
            if (Kernel.Length > MaxVal)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }

            int h = Kernel.Length / 2;

            System.Drawing.Bitmap bSrc = null;
            BitmapData bmData = null;
            BitmapData bmSrc = null;

            try
            {
                bSrc = (System.Drawing.Bitmap)b.Clone();
                bmData = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height),
                                           ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bmSrc = bSrc.LockBits(new System.Drawing.Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmData.Stride;

                System.IntPtr Scan0 = bmData.Scan0;
                System.IntPtr SrcScan0 = bmSrc.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    byte* pSrc = (byte*)(void*)SrcScan0;

                    int nWidth = b.Width;
                    int nHeight = b.Height;

                    double Sum = 0.0, Sum2 = 0.0, Sum3 = 0.0, Sum4 = 0.0, KSum = 0.0;
                    double count = 0.0, count2 = 0.0, count3 = 0.0, count4 = 0.0;
                    double z = 1.0 / Kernel.Length;

                    # region Main Body

                    for (int y = 0; y < nHeight; y++)
                    {
                        # region First Pixels
                        for (int l = 0; l < h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride;

                            for (int c = h - l, cc = 0; c < Kernel.Length; c++, cc++)
                            {
                                z = 1.0 / (Kernel.Length - (h - l));

                                if (System.Math.Abs((int)pSrc[0 + (l * 4)] - (int)pSrc[0 + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (c == h) ? ((double)pSrc[0 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[0 + (cc * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * 4)] - (int)pSrc[1 + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (c == h) ? ((double)pSrc[1 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[1 + (cc * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * 4)] - (int)pSrc[2 + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (c == h) ? ((double)pSrc[2 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[2 + (cc * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * 4)] - (int)pSrc[3 + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (c == h) ? ((double)pSrc[3 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[3 + (cc * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (c == h)
                                    KSum += Kernel[c] + AddVals[l];
                                else
                                    KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (l * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (l * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (l * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (l * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (l * 4)];
                            }
                        }
                        # endregion

                        # region Standard
                        z = 1.0 / Kernel.Length;

                        for (int x = 0; x < nWidth - Kernel.Length + 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride + x * 4;

                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride + x * 4;

                            for (int c = 0; c < Kernel.Length; c++)
                            {
                                if (System.Math.Abs((int)pSrc[0 + (h * 4)] - (int)pSrc[0 + (c * 4)]) <= Sigma)
                                {
                                    Sum += ((double)pSrc[0 + (c * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (h * 4)] - (int)pSrc[1 + (c * 4)]) <= Sigma)
                                {
                                    Sum2 += ((double)pSrc[1 + (c * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (h * 4)] - (int)pSrc[2 + (c * 4)]) <= Sigma)
                                {
                                    Sum3 += ((double)pSrc[2 + (c * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (h * 4)] - (int)pSrc[3 + (c * 4)]) <= Sigma)
                                    {
                                        Sum4 += ((double)pSrc[3 + (c * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * 4)];
                            }
                        }
                        # endregion

                        # region Last Pixels
                        for (int l = nWidth - Kernel.GetLength(0) + 1; l < nWidth - h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += (y * stride) + (l * 4);
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (y * stride) + (l * 4);

                            for (int c = 0, cc = 0; c < nWidth - l; c++, cc++)
                            {
                                z = 1.0 / (nWidth - l);

                                if (System.Math.Abs((int)pSrc[0 + (h * 4)] - (int)pSrc[0 + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (c == h) ? ((double)pSrc[0 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[0 + (cc * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (h * 4)] - (int)pSrc[1 + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (c == h) ? ((double)pSrc[1 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[1 + (cc * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (h * 4)] - (int)pSrc[2 + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (c == h) ? ((double)pSrc[2 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[2 + (cc * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (h * 4)] - (int)pSrc[3 + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (c == h) ? ((double)pSrc[3 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[3 + (cc * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (c == h)
                                    KSum += Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h];
                                else
                                    KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * 4)];
                            }
                        }
                        # endregion

                        if (ProgressPlus != null)
                        {
                            if (pe.ImgWidthHeight < Int32.MaxValue)
                                pe.CurrentProgress++;
                            try
                            {
                                if ((int)pe.CurrentProgress % pe.PrgInterval == 0)
                                    ProgressPlus(pe);
                            }
                            catch
                            {

                            }
                        }
                    }
                    # endregion
                }

                b.UnlockBits(bmData);
                bSrc.UnlockBits(bmSrc);
                bSrc.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    b.UnlockBits(bmData);
                }
                catch
                {

                }

                try
                {
                    bSrc.UnlockBits(bmSrc);
                }
                catch
                {

                }

                if (bSrc != null)
                {
                    bSrc.Dispose();
                    bSrc = null;
                }
            }
            return false;
        }

        //Test-Method for Parallelizing. I you get errors, choose the method above.
        //faster then version with local intvars for reducing arithmetic operations
        public bool ConvolveH_Par(System.Drawing.Bitmap b, double[] Kernel, double[] AddVals, int Bias, int Sigma, bool DoTrans, int MaxVal, bool SrcOnSigma, ProgressEventArgs pe)
        {
            if ((Kernel.Length & 0x1) != 1)
            {
                throw new Exception("Kernelrows Length must be Odd.");
            }
            if (AddVals.Length != Kernel.Length)
            {
                throw new Exception("Kernel must be quadratic.");
            }
            if (Kernel.Length < 3)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }
            if (Kernel.Length > MaxVal)
            {
                throw new Exception("Kernelrows Length must be in the range from 3 to " + MaxVal.ToString() + ".");
            }

            int h = Kernel.Length / 2;

            System.Drawing.Bitmap bSrc = null;
            BitmapData bmData = null;
            BitmapData bmSrc = null;

            try
            {
                bSrc = (System.Drawing.Bitmap)b.Clone();
                bmData = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height),
                                           ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bmSrc = bSrc.LockBits(new System.Drawing.Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int stride = bmData.Stride;

                System.IntPtr Scan0 = bmData.Scan0;
                System.IntPtr SrcScan0 = bmSrc.Scan0;

                unsafe
                {
                    # region Main Body

                    int nWidth = b.Width;
                    int nHeight = b.Height;

                    for (int y = 0; y < nHeight; y++)
                    //Parallel.For(0, nHeight, y =>
                    {
                        byte* p = (byte*)(void*)Scan0;
                        byte* pSrc = (byte*)(void*)SrcScan0;

                        double Sum = 0.0, Sum2 = 0.0, Sum3 = 0.0, Sum4 = 0.0, KSum = 0.0;
                        double count = 0.0, count2 = 0.0, count3 = 0.0, count4 = 0.0;
                        double z = 1.0 / Kernel.Length;

                        # region First Pixels
                        for (int l = 0; l < h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride;
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride;

                            for (int c = h - l, cc = 0; c < Kernel.Length; c++, cc++)
                            {
                                z = 1.0 / (Kernel.Length - (h - l));

                                if (System.Math.Abs((int)pSrc[0 + (l * 4)] - (int)pSrc[0 + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (c == h) ? ((double)pSrc[0 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[0 + (cc * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (l * 4)] - (int)pSrc[1 + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (c == h) ? ((double)pSrc[1 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[1 + (cc * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (l * 4)] - (int)pSrc[2 + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (c == h) ? ((double)pSrc[2 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[2 + (cc * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (l * 4)] - (int)pSrc[3 + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (c == h) ? ((double)pSrc[3 + (cc * 4)] * (Kernel[c] + AddVals[l])) : ((double)pSrc[3 + (cc * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (c == h)
                                    KSum += Kernel[c] + AddVals[l];
                                else
                                    KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (l * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (l * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (l * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (l * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (l * 4)];
                            }
                        }
                        # endregion

                        # region Standard
                        z = 1.0 / Kernel.Length;

                        for (int x = 0; x < nWidth - Kernel.Length + 1; x++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += y * stride + x * 4;

                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += y * stride + x * 4;

                            for (int c = 0; c < Kernel.Length; c++)
                            {
                                if (System.Math.Abs((int)pSrc[0 + (h * 4)] - (int)pSrc[0 + (c * 4)]) <= Sigma)
                                {
                                    Sum += ((double)pSrc[0 + (c * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (h * 4)] - (int)pSrc[1 + (c * 4)]) <= Sigma)
                                {
                                    Sum2 += ((double)pSrc[1 + (c * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (h * 4)] - (int)pSrc[2 + (c * 4)]) <= Sigma)
                                {
                                    Sum3 += ((double)pSrc[2 + (c * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (h * 4)] - (int)pSrc[3 + (c * 4)]) <= Sigma)
                                    {
                                        Sum4 += ((double)pSrc[3 + (c * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * 4)];
                            }
                        }
                        # endregion

                        # region Last Pixels
                        for (int l = nWidth - Kernel.GetLength(0) + 1; l < nWidth - h; l++)
                        {
                            Sum = Sum2 = Sum3 = Sum4 = KSum = 0.0;
                            count = count2 = count3 = count4 = 0.0;
                            bool ignore = false, ignore2 = false, ignore3 = false, ignore4 = false;

                            p = (byte*)(void*)Scan0;
                            p += (y * stride) + (l * 4);
                            pSrc = (byte*)(void*)SrcScan0;
                            pSrc += (y * stride) + (l * 4);

                            for (int c = 0, cc = 0; c < nWidth - l; c++, cc++)
                            {
                                z = 1.0 / (nWidth - l);

                                if (System.Math.Abs((int)pSrc[0 + (h * 4)] - (int)pSrc[0 + (cc * 4)]) <= Sigma)
                                {
                                    Sum += (c == h) ? ((double)pSrc[0 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[0 + (cc * 4)] * Kernel[c]);
                                    count += z;
                                }
                                else
                                    ignore = true;

                                if (System.Math.Abs((int)pSrc[1 + (h * 4)] - (int)pSrc[1 + (cc * 4)]) <= Sigma)
                                {
                                    Sum2 += (c == h) ? ((double)pSrc[1 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[1 + (cc * 4)] * Kernel[c]);
                                    count2 += z;
                                }
                                else
                                    ignore2 = true;

                                if (System.Math.Abs((int)pSrc[2 + (h * 4)] - (int)pSrc[2 + (cc * 4)]) <= Sigma)
                                {
                                    Sum3 += (c == h) ? ((double)pSrc[2 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[2 + (cc * 4)] * Kernel[c]);
                                    count3 += z;
                                }
                                else
                                    ignore3 = true;

                                if (DoTrans)
                                {
                                    if (System.Math.Abs((int)pSrc[3 + (h * 4)] - (int)pSrc[3 + (cc * 4)]) <= Sigma)
                                    {
                                        Sum4 += (c == h) ? ((double)pSrc[3 + (cc * 4)] * (Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h])) : ((double)pSrc[3 + (cc * 4)] * Kernel[c]);
                                        count4 += z;
                                    }
                                    else
                                        ignore4 = true;
                                }

                                if (c == h)
                                    KSum += Kernel[c] + AddVals[Kernel.Length - (nWidth - l) + h];
                                else
                                    KSum += Kernel[c];
                            }

                            if (KSum == 0.0)
                                KSum = 1.0;

                            if (count == 0.0) count = 1.0;
                            if (count2 == 0.0) count2 = 1.0;
                            if (count3 == 0.0) count3 = 1.0;
                            if (count4 == 0.0) count4 = 1.0;

                            p += (h * 4);
                            p[0] = (Byte)System.Math.Max(System.Math.Min((int)((Sum / KSum) / (double)count) + Bias, 255), 0);
                            p[1] = (Byte)System.Math.Max(System.Math.Min((int)((Sum2 / KSum) / (double)count2) + Bias, 255), 0);
                            p[2] = (Byte)System.Math.Max(System.Math.Min((int)((Sum3 / KSum) / (double)count3) + Bias, 255), 0);
                            if (DoTrans)
                                p[3] = (Byte)System.Math.Max(System.Math.Min((int)((Sum4 / KSum) / (double)count4) + Bias, 255), 0);

                            if (SrcOnSigma)
                            {
                                if (ignore)
                                    p[0] = pSrc[0 + (h * 4)];
                                if (ignore2)
                                    p[1] = pSrc[1 + (h * 4)];
                                if (ignore3)
                                    p[2] = pSrc[2 + (h * 4)];
                                if (ignore4)
                                    p[3] = pSrc[3 + (h * 4)];
                            }
                        }
                        # endregion

                        //geht hier auf diese Weise
                        if (ProgressPlus != null)
                        {
                            lock (this.lockObject)
                            {
                                if (pe.ImgWidthHeight < Int32.MaxValue)
                                    pe.CurrentProgress++;
                                try
                                {
                                    if ((int)pe.CurrentProgress % pe.PrgInterval == 0)
                                        ProgressPlus(pe);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }//);
                    # endregion
                }

                b.UnlockBits(bmData);
                bSrc.UnlockBits(bmSrc);
                bSrc.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    b.UnlockBits(bmData);
                }
                catch
                {

                }

                try
                {
                    bSrc.UnlockBits(bmSrc);
                }
                catch
                {

                }

                if (bSrc != null)
                {
                    bSrc.Dispose();
                    bSrc = null;
                }
            }
            return false;
        }

    }

    public class ProgressEventArgs
    {
        private int _prgInterval = 1;
        public int ImgWidthHeight { get; set; }
        public double CurrentProgress { get; set; }
        public int PrgInterval { get { return _prgInterval; } set { _prgInterval = System.Math.Max(value, 1); } }

        public ProgressEventArgs(int ImageWidthHeight, double StartValue, int Interval)
        {
            ImgWidthHeight = ImageWidthHeight;
            CurrentProgress = StartValue;
            PrgInterval = Interval;
        }
    }
}