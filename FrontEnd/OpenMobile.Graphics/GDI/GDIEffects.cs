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

namespace OpenMobile.Graphics.GDI
{
    public class AlphaBlending
    {
        public static void AlphaScale(Bitmap bmp, float alphaDelta)
        {
            System.Drawing.Rectangle rc = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpDat = bmp.LockBits(rc, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            unsafe
            {
                int x, y; byte a; float fa = 255.0f;
                byte* ptr = (byte*)bmpDat.Scan0;
                for (y = 0; y < rc.Height; y++)
                {
                    a = (byte)fa;
                    for (x = 0; x < bmpDat.Width; x++)
                        ptr[(x << 2) + 3] = a;
                    ptr += bmpDat.Stride;
                    fa -= alphaDelta;
                    if (fa < 0.000001)
                        fa = 0.001f;
                }
            }
            bmp.UnlockBits(bmpDat);
        }
    }

    public class Reflection
    {
        /// <summary>
        /// Creates a reflection of an image (bitmap) 
        /// </summary>
        /// <param name="source">Source bitmap data</param>
        /// <param name="AlphaDropOff">Float value indicating how fast the image should fade into the background</param>
        /// <param name="UseGaussianBlur">True = use GaussianBlur effect on the reflection</param>
        /// <returns></returns>
        public static Bitmap GetReflection(Bitmap source, float AlphaDropOff, bool UseGaussianBlur)
        {
            if (source == null)
                return null;

            Bitmap bmp = new Bitmap(source);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (UseGaussianBlur)
                OpenMobile.Graphics.GDI.BitmapFilter.GaussianBlur(bmp, 1);
            if (AlphaDropOff > 0)
                OpenMobile.Graphics.GDI.AlphaBlending.AlphaScale(bmp, AlphaDropOff);
            return bmp;
        }

        /// <summary>
        /// Creates a reflection of an OImage
        /// </summary>
        /// <param name="source"></param>
        /// <param name="AlphaDropOff"></param>
        /// <param name="UseGaussianBlur"></param>
        /// <returns></returns>
        public static OImage GetReflection(OImage source, float AlphaDropOff, bool UseGaussianBlur)
        {
            return new OImage(GetReflection(source.image, AlphaDropOff, UseGaussianBlur));
        }

        /// <summary>
        /// Creates a reflection of an imageItem
        /// </summary>
        /// <param name="source"></param>
        /// <param name="VisibleArea">Visible area of reflection in percent (0.0 -> 1.0)</param>
        /// <param name="UseGaussianBlur"></param>
        /// <returns></returns>
        public static imageItem GetReflection(imageItem source, float VisibleArea, bool UseGaussianBlur)
        {
            if (source == null || source.image == null)
                return new imageItem();

            // Calculate alpha drop off value
            float AlphaDropOff = 0;
            if (VisibleArea > 0.0f && VisibleArea < 1.0f)
                AlphaDropOff = 255.0f / (source.image.Height * VisibleArea);
            else if (VisibleArea <= 0)
                AlphaDropOff = 0;
            else
                AlphaDropOff = 255.0f;

            return new imageItem(GetReflection(source.image, AlphaDropOff, UseGaussianBlur));
        }

        /// <summary>
        /// Creates a reflection of an imageItem (visible area of reflection is 50% of source image)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="AlphaDropOff"></param>
        /// <param name="UseGaussianBlur"></param>
        /// <returns></returns>
        public static imageItem GetReflection(imageItem source, bool UseGaussianBlur)
        {
            if (source == null || source.image == null)
                return new imageItem();

            // Calculate alpha drop off value
            float AlphaDropOff = 255.0f / (source.image.Height * 0.5f);            

            return new imageItem(GetReflection(source.image, AlphaDropOff, UseGaussianBlur));
        }

    }
}
