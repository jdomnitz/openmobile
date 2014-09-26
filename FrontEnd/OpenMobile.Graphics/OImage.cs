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
using OpenMobile.Graphics.GDI;

namespace OpenMobile.Graphics
{
    [Serializable]
    public class OImage : IDisposable, ICloneable
    {
        Bitmap img = null;
        public bool persist;
        int height, width;
        private bool[] _GenerateTexture = new bool[10];
        private uint[] _Texture = new uint[10];

        /// <summary>
        /// The shader effect to use when rendering this image
        /// </summary>
        public OMShaders ShaderEffect
        {
            get
            {
                return this._ShaderEffect;
            }
            set
            {
                if (this._ShaderEffect != value)
                {
                    this._ShaderEffect = value;
                }
            }
        }
        private OMShaders _ShaderEffect = OMShaders.None;        

        public uint GetTexture(int Screen)
        {
            //return _Texture;
            if (Screen < 0)
                return 0;

            //ResizeArraysToScreen(Screen);
            return _Texture[Screen];
        }

        public void SetTexture(int Screen, uint Texture)
        {
            //_Texture = Texture;
            if (Screen < 0)
                return;

            //ResizeArraysToScreen(Screen);
            _Texture[Screen] = Texture;
            _GenerateTexture[Screen] = false;
        }

        /// <summary>
        /// True = Texture generation required for this image 
        /// </summary>
        public bool TextureGenerationRequired(int Screen)
        {
            //if (_Texture == 0 || _GenerateTexture)
            //    return true;
            //return false;

            if (Screen < 0)
                return false;

            //ResizeArraysToScreen(Screen);
            if (_Texture[Screen] == 0 || _GenerateTexture[Screen])
                return true;
            return false;
        }

        public OImage() 
        {
        }
        public OImage(System.Drawing.Bitmap bmp)
        {
            SetBmp(bmp);
        }
        /// <summary>
        /// Creates a image based on a color
        /// </summary>
        /// <param name="color"></param>
        public OImage(Color color, int width, int height)
        {
            // Create a bitmap with a solid color
            System.Drawing.Bitmap bmpColor = new Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpColor))
            {
                using (System.Drawing.Brush b = new SolidBrush(color.ToSystemColor()))
                {
                    g.FillRectangle(b, 0, 0, width, height);
                    SetBmp(bmpColor);
                }
            }
        }

        public void SetBmp(System.Drawing.Bitmap bmp, bool disposeOldImg = false)
        {
            // Error check
            if (bmp == null)
                return;
            if (disposeOldImg)
            {
                if (img != null)
                    img.Dispose();
            }
            img = bmp;
            lock (img)
            {
                height = img.Height;
                width = img.Width;
            }

            for (int i = 0; i < _GenerateTexture.Length; i++)
                _GenerateTexture[i] = true;
            //_GenerateTexture = true;
        }

        public void Refresh()
        {
            for (int i = 0; i < _GenerateTexture.Length; i++)
                _GenerateTexture[i] = true;
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

                for (int i = 0; i < _GenerateTexture.Length; i++)
                    _GenerateTexture[i] = true;
                //_GenerateTexture = true;
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
        /// The region this image covers
        /// </summary>
        public Rectangle Region
        {
            get
            {
                return new Rectangle(0, 0, width, height);
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

        public OImage Resize(int height = 0, int width = 0)
        {
            if (img == null)
                return this;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                if (height == 0 && width == 0)
                    return this;

                float ratio = img.Width / img.Height;
                if (height > 0 && width == 0)
                {
                    width = (int)(height * ratio);
                }
                else if (width > 0 && height == 0)
                {
                    height = (int)(width * ratio);
                }

                Bitmap bmp = new Bitmap(width, height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.DrawImage(img, new System.Drawing.RectangleF(0, 0, bmp.Width, bmp.Height), new System.Drawing.RectangleF(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                }

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;

                // Regenerate textures
                for (int i = 0; i < _GenerateTexture.Length; i++)
                    _GenerateTexture[i] = true;

                return this;
            }
        }

        public enum OverLayDirections { All, LeftToRight, RightToLeft, TopToBottom, BottomToTop }

        /// <summary>
        /// Add an overlay color onto this image
        /// </summary>
        /// <param name="c"></param>
        public OImage Overlay(Color c)
        {
            Overlay(c, 100, OverLayDirections.All);
            return this;
        }
        /// <summary>
        /// Add an overlay color onto this image
        /// </summary>
        /// <param name="c"></param>
        public OImage Overlay(Color c, int coverPercentage, OverLayDirections direction)
        {
            if (img == null)
                return this;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                // Calculate color values
                float R = 0;
                if (c.R > 0)
                    R = c.R / 255f;
                float G = 0;
                if (c.G > 0)
                    G = c.G / 255f;
                float B = 0;
                if (c.B > 0)
                    B = c.B / 255f;

                System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
                        // Matrix config
                        //           R  G  B  A  w
                        new float[] {R, 0, 0, 0, 0}, // R
                        new float[] {0, G, 0, 0, 0}, // G
                        new float[] {0, 0, B, 0, 0}, // B
                        new float[] {0, 0, 0, 1, 0}, // A
                        new float[] {0, 0, 0, 0, 1}, // w
                    });

                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);

                // Draw image using color matrix
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    switch (direction)
                    {
                        case OverLayDirections.All:
                            {
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                            }
                            break;
                        case OverLayDirections.LeftToRight:
                            {
                                // Draw original image without overlay
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                                // Draw image with overlay 
                                if (coverPercentage > 0)
                                {
                                    int left = (int)(width * (coverPercentage / 100.0f));
                                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, left, img.Height), 0, 0, left, img.Height, GraphicsUnit.Pixel, ia);
                                }
                            }
                            break;
                        case OverLayDirections.RightToLeft:
                            {
                                // Draw original image without overlay
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                                // Draw image with overlay 
                                if (coverPercentage > 0)
                                {
                                    int left = width - (int)(width * (coverPercentage / 100.0f));
                                    g.DrawImage(img, new System.Drawing.Rectangle(left, 0, img.Width, img.Height), left, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                                }
                            }
                            break;
                        case OverLayDirections.TopToBottom:
                            {
                                // Draw original image without overlay
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                                // Draw image with overlay 
                                if (coverPercentage > 0)
                                {
                                    int top = (int)(height * (coverPercentage / 100.0f));
                                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, top), 0, 0, img.Width, top, GraphicsUnit.Pixel, ia);
                                }
                            }
                            break;
                        case OverLayDirections.BottomToTop:
                            {
                                // Draw original image without overlay
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                                // Draw image with overlay 
                                if (coverPercentage > 0)
                                {
                                    int top = height - (int)(height * (coverPercentage / 100.0f));
                                    g.DrawImage(img, new System.Drawing.Rectangle(0, top, img.Width, img.Height), 0, top, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                ia.Dispose();

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;

                // Regenerate textures
                for (int i = 0; i < _GenerateTexture.Length; i++)
                    _GenerateTexture[i] = true;
            }
            return this;
        }

        public OImage Invert()
        {
            if (img == null)
                return this;

            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(
            new float[][]
            {
                new float[] {-1, 0, 0, 0, 0},
                new float[] {0, -1, 0, 0, 0},
                new float[] {0, 0, -1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
            });

            UpdateAndSetImageWithMatrix(cm);

            return this;
        }

        private unsafe void Invert(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int* bytes = (int*)data.Scan0;
            for (int i = w * h - 1; i >= 0; i--)
                bytes[i] = ~bytes[i];
            bmp.UnlockBits(data);
        }

        public OImage MakeTransparentFromPixel(int pixelX, int pixelY)
        {
            return MakeTransparent(img.GetPixel(pixelX, pixelY));
        }

        public OImage MakeTransparent(Color colorToMakeTransparent)
        {
                // Set color mapping table
                ColorMap[] colorMaps = new ColorMap[1];
                ColorMap remap = new ColorMap();
                remap.OldColor = colorToMakeTransparent.ToSystemColor();
                remap.NewColor = System.Drawing.Color.Transparent;
                colorMaps[0] = remap;
                return RemapColors(colorMaps);
        }

        public OImage MakeTransparent(System.Drawing.Color colorToMakeTransparent)
        {
            // Set color mapping table
            ColorMap[] colorMaps = new ColorMap[1];
            ColorMap remap = new ColorMap();
            remap.OldColor = colorToMakeTransparent;
            remap.NewColor = System.Drawing.Color.Transparent;
            colorMaps[0] = remap;
            return RemapColors(colorMaps);
        }

        public OImage RemapColors(ColorMap[] colorMaps)
        {
            if (img == null || colorMaps == null)
                return this;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                // Apply map
                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetRemapTable(colorMaps, ColorAdjustType.Bitmap);

                // Draw image using color matrix
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                ia.Dispose();

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;

                // Regenerate textures
                for (int i = 0; i < _GenerateTexture.Length; i++)
                    _GenerateTexture[i] = true;
            }
            return this;
        }

        public enum CropBase { Left, Top, Bottom, Right }

        /// <summary>
        /// Crops the image
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="cropBase"></param>
        public OImage Crop(int percentage, CropBase cropBase)
        {
            if (img == null)
                return this;

            float percentageF = percentage / 100f;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    switch (cropBase)
                    {
                        case CropBase.Left:
                            {
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, (int)(img.Width * percentageF), img.Height), 0, 0, img.Width * percentageF, img.Height, GraphicsUnit.Pixel);
                            }
                            break;
                        case CropBase.Top:
                            {
                                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, (int)(img.Height * percentageF)), 0, 0, img.Width, img.Height * percentageF, GraphicsUnit.Pixel);
                            }
                            break;
                        case CropBase.Bottom:
                            {
                                int height = (int)(img.Height * percentageF);
                                int top = img.Height - height;
                                g.DrawImage(img, new System.Drawing.Rectangle(0, top, img.Width, height), 0, top, img.Width, height, GraphicsUnit.Pixel);
                            }
                            break;
                        case CropBase.Right:
                            {
                                int width = (int)(img.Width * percentageF);
                                int left = img.Width - width;
                                g.DrawImage(img, new System.Drawing.Rectangle(left, 0, width, img.Height), left, 0, width, img.Height, GraphicsUnit.Pixel);
                            }
                            break;
                        default:
                            break;
                    }
                }

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;
            }
            return this;
        }

        /// <summary>
        /// Adds a border around the image without changing the size of the image (orginal image is reduced)
        /// </summary>
        /// <param name="percentage">The percentage to reduce the image with</param>
        /// <param name="backColor"></param>
        public OImage AddBorder(int percentage, Color backColor)
        {
            if (img == null)
                return this;

            float percentageF = 1f - (percentage / 100f);

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    int height = (int)(img.Height * percentageF);
                    int top = (img.Height - height) / 2;
                    int width = (int)(img.Width * percentageF);
                    int left = (img.Width - width) / 2;

                    float penWidth = System.Math.Min(img.Height - height, img.Width - width);
                    using (System.Drawing.Pen p = new System.Drawing.Pen(backColor.ToSystemColor(), penWidth))
                    {
                        g.DrawRectangle(p, new System.Drawing.Rectangle(0, 0, img.Width, img.Height));
                    }                    
                    //using (SolidBrush b = new SolidBrush(backColor.ToSystemColor()))
                    //{
                    //    g.FillRectangle(b, new System.Drawing.Rectangle(0, 0, img.Width, img.Height));
                    //}
                    g.DrawImage(img, new System.Drawing.Rectangle(left, top, width, height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                }

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;
            }
            return this;
        }

        public OImage SetBackground(Color color)
        {
            if (img == null)
                return this;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    using (SolidBrush b = new SolidBrush(color.ToSystemColor()))
                        g.FillRectangle(b, new System.Drawing.Rectangle(0, 0, img.Width, img.Height));
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                }

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;
            }
            return this;
        }

        /// <summary>
        /// Adds a glow effect around the image
        /// </summary>
        /// <param name="c"></param>
        /// <param name="glowSize">Size of glow (NB! Must be an odd number)</param>
        public OImage Glow(Color c, int glowSize = 31)
        {
            if (img == null)
                return this;

            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                // Create workspace image
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                // Draw glow color
                BitmapFunctions bf = new BitmapFunctions(bmp);
                bf.drawOneColor(c.A, c.R, c.G, c.B);
                bf.Dispose();

                // Blur (NB! Length must be an odd number)
                Convolution conv = new Convolution();
                fipbmp.FastZGaussian_Blur_NxN(bmp, glowSize, 0.01f, 255, true, false, conv);

                // Increase alpha (a darker base color like blue needs a higher alpha value to ensure visibility)
                float AlphaIncrease = c.B / 255;
                fipbmp.IncreaseAlpha(bmp, 2.0f + AlphaIncrease);

                // Draw orginal image on top
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                // Dispose original image
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }

                // Assign new image
                img = bmp;

                // Regenerate textures
                for (int i = 0; i < _GenerateTexture.Length; i++)
                    _GenerateTexture[i] = true;
            }
            return this;
        }

        /// <summary>
        /// Sets the alpha channel of the image
        /// </summary>
        /// <param name="alpha">Alpha in the range 1 to 100</param>
        public OImage SetAlpha(int alpha)
        {
            SetAlpha((float)(alpha / 100f));
            return this;
        }

        /// <summary>
        /// Sets the alpha channel of the image
        /// </summary>
        /// <param name="alpha">Alpha in the range 0.0 to 1.0</param>
        public OImage SetAlpha(float alpha)
        {
            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
                        // Matrix config
                        //           R  G  B  A  w
                        new float[] {1, 0, 0, 0, 0}, // R
                        new float[] {0, 1, 0, 0, 0}, // G
                        new float[] {0, 0, 1, 0, 0}, // B
                        new float[] {0, 0, 0, alpha, 0}, // A
                        new float[] {0, 0, 0, 0, 1}, // w
                    });

            UpdateAndSetImageWithMatrix(cm);
            return this;
        }

        private OImage UpdateAndSetImageWithMatrix(ColorMatrix cm)
        {
            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(cm);

            // Draw image using color matrix
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                ia.Dispose();
            }

            // Dispose original image
            if (img != null)
            {
                img.Dispose();
                img = null;
            }

            // Assign new image
            img = bmp;

            // Regenerate textures
            for (int i = 0; i < _GenerateTexture.Length; i++)
                _GenerateTexture[i] = true;
            return this;
        }

        public OImage RenderText(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC, FitModes fitMode)
        {
             // Create new Bitmap object with the size of the picture
            lock (img)
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img))
                {
                    Graphics.renderText(g, x, y, w, h, text, font, format, alignment, c, sC, fitMode);
                }
            }

            // Regenerate textures
            for (int i = 0; i < _GenerateTexture.Length; i++)
                _GenerateTexture[i] = true;
            return this;
        }

        public OImage AddImageOverlay(int x, int y, OImage image)
        {
            AddImageOverlay(x, y, image.image.Width, image.image.Height, image);
            return this;
        }

        public OImage AddImageOverlay(int x, int y, int w, int h, OImage image)
        {
            // Create new Bitmap object with the size of the picture
            lock (img)
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img))
                {
                    g.DrawImage(image.image, x, y, w, h);
                }
            }

            // Regenerate textures
            for (int i = 0; i < _GenerateTexture.Length; i++)
                _GenerateTexture[i] = true;
            return this;
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
            for (int i = 0; i < _Texture.Length; i++)
                _Texture[i] = 0;
            //_GenerateTexture = true;
            if (img != null)
                img.Dispose();
            img = null;
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            lock (img)
            {
                OImage newImg = new OImage((Bitmap)img.Clone());
                newImg.ShaderEffect = this.ShaderEffect;
                return newImg;
            }
        }

        /// <summary>
        /// Copies this image into a new image object
        /// </summary>
        /// <returns></returns>
        public OImage Copy()
        {
            return this.Clone() as OImage;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})",base.ToString(), this.GetHashCode());
        }

        /// <summary>
        /// Converts this image to a imageItem object
        /// </summary>
        /// <returns></returns>
        public imageItem ToImageItem()
        {
            return new imageItem(this);
        }

        public override bool Equals(object obj)
        {
            OImage obj2 = obj as OImage;
            if (obj2 == null)
                return false;
            return this.GetHashCode() == obj2.GetHashCode();
            //return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
