﻿/*********************************************************************************
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

namespace OpenMobile.Graphics
{
    public class OImage : IDisposable, ICloneable
    {
        Bitmap[] img = new Bitmap[1];
        public bool persist;
        private uint[] texture;
        int height, width;

        public uint Texture(int screen)
        {
            if (screen < texture.Length)
                return texture[screen];
            return 0;
        }
        public void rotateTexture()
        {
            for (int i = 0; i < texture.Length; i++)
                if (texture[i] > 0)
                {
                    Graphics.DeleteTexture(i, texture[i]);
                    texture[i] = 0;
                }
        }
        internal void SetTexture(int screen, uint texture)
        {
            if (screen < this.texture.Length)
                this.texture[screen] = texture;
            for (int i = 0; i < this.texture.Length; i++)
                if (this.texture[i] == 0)
                    return;
            if (!persist)
            {
                for (int i = 0; i < img.Length; i++)
                {
                    img[i].Dispose();
                    img[i] = null;
                }
            }
        }

        public bool Shared
        {
            get
            {
                return (img.Length == 1);
            }
        }

        public OImage() 
        {
            Screen = 0;
            texture = new uint[DisplayDevice.AvailableDisplays.Count];
        }
        public OImage(System.Drawing.Bitmap i)
        {
            // Error check
            if (i == null)
                return;
            Screen = 0;
            texture = new uint[DisplayDevice.AvailableDisplays.Count];
            img[Screen] = i;
            lock (img)
            {
                height = img[Screen].Height;
                width = img[Screen].Width;
            }
        }

        public int Screen = 0;
        public void SetScreen(int Screen)
        {
            this.Screen = Screen;
        }

        public Bitmap image
        {
            get 
            {
                return img[Screen];
            }
        }
        public Bitmap GetImage(int screen)
        {
            return img[screen];
        }
        public void SetImage(int screen, Bitmap i)
        {
            if (img.Length < DisplayDevice.AvailableDisplays.Count)
                Array.Resize<Bitmap>(ref img, DisplayDevice.AvailableDisplays.Count);
            img[screen] = i;
        }
        public Size Size
        {
            get
            {
                if (img[Screen] == null)
                    return new Size();
                lock (img)
                    return new Size(width, height);
            }
        }
        public int Height
        {
            get
            {
                return height;
            }
        }
        public int Width
        {
            get
            {
                return width;
            }
        }
        public bool CanAnimate
        {
            get
            {
                if (img == null)
                    return false;
                try
                {
                    return (img[Screen].GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) > 1);
                }
                catch { }
                return false;
            }
        }        
        
        public void Overlay(Color c)
        {
            if (img == null)
                return;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                Bitmap bmpPicture = new Bitmap(img[Screen].Width, img[Screen].Height);
                // Image attributes for setting the attributes of the picture
                System.Drawing.Imaging.ImageAttributes iaPicture = new System.Drawing.Imaging.ImageAttributes();

                System.Drawing.Imaging.ColorMatrix cmPicture = new System.Drawing.Imaging.ColorMatrix();
                /*
                // Change the elements
                cmPicture.Matrix00 = -1;
                cmPicture.Matrix11 = -1;
                cmPicture.Matrix22 = -1;
                // Set the new color matrix
                iaPicture.SetColorMatrix(cmPicture);
                */
                // Set the Graphics object from the bitmap
                System.Drawing.Graphics gfxPicture = System.Drawing.Graphics.FromImage(bmpPicture);
                gfxPicture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                // New rectangle for the picture, same size as the original picture
                System.Drawing.Rectangle rctPicture = new System.Drawing.Rectangle(0, 0, img[Screen].Width, img[Screen].Height);
                // Draw the new image
                gfxPicture.DrawImage(img[Screen], rctPicture, 0, 0, img[Screen].Width, img[Screen].Height, GraphicsUnit.Pixel, iaPicture);

                cmPicture = new System.Drawing.Imaging.ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                //new float[] {.0f, .50f, .0f, .0f, 1}
                new float[] {((float)c.R/255f), ((float)c.G/255f), ((float)c.B/255f), .0f, 1}
            });
                // Set the new color matrix
                iaPicture.SetColorMatrix(cmPicture);
                gfxPicture.DrawImage(bmpPicture, rctPicture, 0, 0, img[Screen].Width, img[Screen].Height, GraphicsUnit.Pixel, iaPicture);

                // Set the PictureBox to the new inverted colors bitmap
                img[Screen] = bmpPicture;
            }
        }

        public static OImage FromFile(string filename)
        {
            try
            {
                return new OImage(new Bitmap(filename));
            }
            catch
            {
                return null;
            }
        }
        public static OImage FromStream(System.IO.Stream stream)
        {
            try
            {
                return new OImage(new Bitmap(stream));
            }
            catch
            {
                return null;
            }
        }
        public static OImage FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement)
        {
            try
            {
                return new OImage(new Bitmap(stream, useEmbeddedColorManagement));
            }
            catch
            {
                return null;
            }
        }
        public static OImage FromWebdingsFont(int w, int h, string s, Color color)
        {
            return FromWebdingsFont(w, h, s, eTextFormat.Normal, Alignment.CenterCenter, color, color);
        }
        public static OImage FromWebdingsFont(int w, int h, string s, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return FromFont(w, h, s, new Font(OpenMobile.Graphics.Font.Webdings, h*0.65F), format, alignment, color, secondColor);
        }
        public static OImage FromFont(int w, int h, string s, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            // TODO: Make this dynamic instead of locked to screen 0
            try
            {
                OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
                return gr.GenerateTextTexture(0, 0, w, h, s, font, format, alignment, color, secondColor);
            }
            catch
            {
                return null;
            }
        }

        ~OImage()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (texture == null)
                return;
            for (int i = 0; i < texture.Length; i++)
                if (texture[i] > 0)
                    Graphics.DeleteTexture(i, texture[i]);
            texture = null;
            for (int i = 0; i < img.Length; i++)
            {
                if (img[i] != null)
                    img[i].Dispose();
                img[i] = null;
            }
            GC.SuppressFinalize(this);
        }

        public object Clone()
        {
            lock (img)
            {
                OImage ret = new OImage((Bitmap)img.Clone());
                ret.texture = (uint[])this.texture.Clone();
                return ret;
            }
        }
    }
}