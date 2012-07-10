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
        Bitmap img = null;
        public bool persist;
        int height, width;
        private bool _GenerateTexture = true;
        private uint _Texture = 0;

        /// <summary>
        /// The OpenGL texture reference for this image (0 = not texture generated)
        /// </summary>
        public uint Texture 
        {
            get
            {
                return _Texture;
            }
            set
            {
                _Texture = value;
                _GenerateTexture = false;
            }
        }
        //public uint Texture = 0;

        /// <summary>
        /// True = Texture generation required for this image 
        /// </summary>
        public bool TextureGenerationRequired
        {
            get
            {
                if (_Texture == 0 || _GenerateTexture)
                    return true;
                return false;
            }
        }

        public OImage() 
        {
        }
        public OImage(System.Drawing.Bitmap i)
        {
            // Error check
            if (i == null)
                return;
            img = i;
            lock (img)
            {
                height = img.Height;
                width = img.Width;
            }
            _GenerateTexture = true;
        }

        /// <summary>
        /// Loads an image
        /// </summary>
        public Bitmap image
        {
            get 
            {
                return img;
            }
            set
            {
                if (img != null & img != value)
                {
                    img.Dispose();
                    img = null;
                }
                img = value;
                _GenerateTexture = true;
            }
        }

        /// <summary>
        /// Size of image
        /// </summary>
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

        /// <summary>
        /// Height of image
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Width of image
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// Can this image animate?
        /// </summary>
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
        
        /// <summary>
        /// Add an overlay color onto this image
        /// </summary>
        /// <param name="c"></param>
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

                // Create matrix
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

        #region Static methods

        /// <summary>
        /// Creates an image object from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates an image object from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates an image object from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="useEmbeddedColorManagement"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates an image object using the WebDings font
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static OImage FromWebdingsFont(int w, int h, string s, Color color)
        {
            return FromWebdingsFont(w, h, s, eTextFormat.Normal, Alignment.CenterCenter, color, color);
        }

        /// <summary>
        /// Generates an image object using the WebDings font
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        public static OImage FromWebdingsFont(int w, int h, string s, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return FromFont(w, h, s, new Font(OpenMobile.Graphics.Font.Webdings, h*0.65F), format, alignment, color, secondColor);
        }

        /// <summary>
        /// Generates an image object using the WebDings font
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="size"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        public static OImage FromWebdingsFont(int w, int h, string s, float size, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return FromFont(w, h, s, new Font(OpenMobile.Graphics.Font.Webdings, size), format, alignment, color, secondColor);
        }

        /// <summary>
        /// Generates an image object using the specified font
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        public static OImage FromFont(int w, int h, string s, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
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

        /// <summary>
        /// Properly disposes of this object
        /// </summary>
        /// <param name="img"></param>
        public static void SafeDispose(OImage img)
        {
            img.Dispose();
            img = null;
        }

        #endregion

        ~OImage()
        {
            Dispose();
        }
        public void Dispose()
        {
            // TODO: Add delete texture call
            _Texture = 0;
            if (img != null)
                img.Dispose();
            img = null;
            GC.SuppressFinalize(this);
        }
        
        public object Clone()
        {
            lock (img)
            {
                return new OImage((Bitmap)img.Clone()); ;
            }
        }
    }
}

/* ORG CODE
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
        public void ClearTexture(int screen)
        {
            // Clear texture
            if (screen < this.texture.Length)
            {
                if (this.texture[screen] > 0)
                    Graphics.DeleteTexture(screen, this.texture[screen]);

                this.texture[screen] = 0;

                img[screen].Dispose();
                img[screen] = null;
            }
        }
        internal void SetTexture(int screen, uint texture)
        {
            // Set texture
            if (screen < this.texture.Length)
            {
                if (this.texture[screen] > 0)
                {
                    Graphics.DeleteTexture(screen, this.texture[screen]);
                }

                this.texture[screen] = texture;
            }

            for (int i = 0; i < this.texture.Length; i++)
                if (this.texture[i] == 0)
                    return;

            // Cleanup
            if (!persist)
            {
                for (int i = 0; i < img.Length; i++)
                {
                    if (img[i] != null)
                    {
                        img[i].Dispose();
                        img[i] = null;
                    }
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
            int ScreenCount = System.Math.Max(screen, DisplayDevice.AvailableDisplays.Count);
            if (img.Length < ScreenCount)
                Array.Resize<Bitmap>(ref img, ScreenCount);
            if (screen < DisplayDevice.AvailableDisplays.Count)
            {
                // Properly dispose image before setting the new one
                if (img[screen] != null)
                {
                    img[screen].Dispose();
                    img[screen] = null;
                }
                img[screen] = i;
            }
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
                //// Change the elements
                //cmPicture.Matrix00 = -1;
                //cmPicture.Matrix11 = -1;
                //cmPicture.Matrix22 = -1;
                //// Set the new color matrix
                //iaPicture.SetColorMatrix(cmPicture);
                // Set the Graphics object from the bitmap
                System.Drawing.Graphics gfxPicture = System.Drawing.Graphics.FromImage(bmpPicture);
                gfxPicture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                // New rectangle for the picture, same size as the original picture
                System.Drawing.Rectangle rctPicture = new System.Drawing.Rectangle(0, 0, img[Screen].Width, img[Screen].Height);
                // Draw the new image
                gfxPicture.DrawImage(img[Screen], rctPicture, 0, 0, img[Screen].Width, img[Screen].Height, GraphicsUnit.Pixel, iaPicture);

                // Create matrix
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
            return FromFont(w, h, s, new Font(OpenMobile.Graphics.Font.Webdings, h * 0.65F), format, alignment, color, secondColor);
        }
        public static OImage FromWebdingsFont(int w, int h, string s, float size, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return FromFont(w, h, s, new Font(OpenMobile.Graphics.Font.Webdings, size), format, alignment, color, secondColor);
        }
        public static OImage FromFont(int w, int h, string s, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
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
*/