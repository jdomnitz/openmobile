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

namespace OpenMobile.Graphics
{
    public class OImage : IDisposable, ICloneable
    {
        Bitmap img;
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
                img.Dispose();
                img = null;
            }
        }
        public OImage(System.Drawing.Bitmap i)
        {
            // Error check
            if (i == null)
                return;

            texture = new uint[DisplayDevice.AvailableDisplays.Count];
            img = i;
            lock (img)
            {
                height = img.Height;
                width = img.Width;
            }
        }

        public Bitmap image
        {
            get { return img; }
        }
        public Size Size
        {
            get
            {
                if (img == null)
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
                    return (img.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) > 1);
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
                Bitmap bmpPicture = new Bitmap(img.Width, img.Height);
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
                System.Drawing.Rectangle rctPicture = new System.Drawing.Rectangle(0, 0, img.Width, img.Height);
                // Draw the new image
                gfxPicture.DrawImage(img, rctPicture, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, iaPicture);

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
                gfxPicture.DrawImage(bmpPicture, rctPicture, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, iaPicture);

                // Set the PictureBox to the new inverted colors bitmap
                img = bmpPicture;
            }
        }

        public static OImage FromFile(string filename)
        {
            return new OImage(new Bitmap(filename));
        }
        public static OImage FromStream(System.IO.Stream stream)
        {
            return new OImage(new Bitmap(stream));
        }
        public static OImage FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement)
        {
            return new OImage(new Bitmap(stream, useEmbeddedColorManagement));
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
            OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
            return gr.GenerateTextTexture(0, 0, w, h, s, font, format, alignment, color, secondColor);
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
            if (img != null)
                img.Dispose();
            img = null;
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
