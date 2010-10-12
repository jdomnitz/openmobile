using System;
using System.Collections.Generic;
using System.Drawing;
using OpenMobile.Graphics.OpenGL;
using System.Drawing.Imaging;

namespace OpenMobile.Graphics
{
    public sealed class V2Graphics:IGraphics
    {
        #region private vars
        int screen;
        static System.Drawing.Bitmap virtualG;
        static List<List<uint>> textures = new List<List<uint>>();
        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        float wscale;
        float hscale;
        int width;
        int height;
        #endregion

        internal static void DeleteTexture(int screen, uint texture)
        {
            if (screen < textures.Count)
                textures[screen].Add(texture);
        }

        public V2Graphics(int screen)
        {
            this.screen = screen;
            if (textures.Count == 0)
                for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                    textures.Add(new List<uint>());
            virtualG = new Bitmap(1000, 600);
        }


        private bool loadTexture(ref OImage image)
        {
            if (image == null)
                return false;
            if (image.Size == Size.Empty)
                return false;

            uint texture;
            Raw.GenTextures(1, out texture);
            Raw.BindTexture(TextureTarget.Texture2D, texture);
            Bitmap img = image.image;
            bool kill = false;
            lock (image.image)
            {
                if ((image.Width > maxTextureSize) || (image.Height > maxTextureSize))
                {
                    kill = true;
                    fixImage(ref img);
                }
                BitmapData data;
                if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    try
                    {
                        data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    catch (InvalidOperationException) { return false; }
                    Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                                    OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    img.UnlockBits(data);
                }
                else if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    try
                    {
                        data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    catch (InvalidOperationException) { return false; }
                    Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                    OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    img.UnlockBits(data);
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(screen, texture);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            return true;
        }

        private void fixImage(ref Bitmap image)
        {
            int width=image.Width;
            int height=image.Height;
            if (image.Width > maxTextureSize)
                width = maxTextureSize;
            if (image.Height > maxTextureSize)
                height = maxTextureSize;
            Bitmap TextureBitmap = new Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(TextureBitmap))
                g.DrawImage(image, 0, 0, width, height);
            image = TextureBitmap;
        }
        public void Clear(Color color)
        {
            Raw.Clear(ClearBufferMask.ColorBufferBit);
            Raw.ClearColor(color);
        }
        private Rectangle _clip;
        public Rectangle Clip
        {
            get
            {
                return _clip;
            }
            set
            {
                _clip = value;
                if (_clip == NoClip)
                    Raw.Disable(EnableCap.ScissorTest);
                else
                {
                    if (_clip.Height < 0)
                        _clip.Height = 0;
                    if (_clip.Width < 0)
                        _clip.Width = 0;
                    Raw.Enable(EnableCap.ScissorTest);
                    try
                    {
                        Raw.Scissor((int)(_clip.X * wscale), (int)((600 - _clip.Y - _clip.Height) * hscale), (int)(_clip.Width * wscale), (int)(_clip.Height * hscale));
                    }
                    catch (Exception)
                    {
                        _clip = NoClip;
                        Raw.Disable(EnableCap.ScissorTest);
                    }
                }
            }
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            //throw new NotImplementedException();
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            //throw new NotImplementedException();
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage image, Rectangle rect, Rectangle srcRect)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency, eAngle.Normal);
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency, eAngle angle)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency, angle);
        }

        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency)
        {
            DrawImage(image, rect, transparency);
        }

        public void DrawImage(OImage image, Rectangle rect)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height)
        {
            DrawImage(image, X, Y, Width, Height, 1F, eAngle.Normal);
        }
        int[] normalTex;
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.EnableClientState(ArrayCap.ColorArray);
            Raw.ColorPointer(4, ColorPointerType.Float, 0, new float[] { 1F, 1F, 1F, transparency,1F, 1F, 1F, transparency,1F, 1F, 1F, transparency,1F, 1F, 1F, transparency});
            int[] tex = new int[] { X, Height + Y, Width + X, Height + Y, X, Y, Width + X, Y };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.EnableClientState(ArrayCap.TextureCoordArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, tex);
            Raw.TexCoordPointer(2, TexCoordPointerType.Int, 0, normalTex);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            Raw.DisableClientState(ArrayCap.TextureCoordArray);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.Texture2D);
        }

        public void DrawImage(OImage image, Point[] destPoints)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            DrawImage(image, X, Y, Width, Height, transparency, eAngle.Normal);
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            //throw new NotImplementedException();
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            //throw new NotImplementedException();
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            //throw new NotImplementedException();
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            //throw new NotImplementedException();
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            Color c = pen.Color;
            float[] ar = new float[] { x, y, x + width, y, x + width, y + height, x, y + height };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.EnableClientState(ArrayCap.ColorArray);
            Raw.ColorPointer(4, ColorPointerType.UnsignedByte, 0, new byte[] { c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A });
            Raw.VertexPointer(2, VertexPointerType.Float, 0, ar);
            Raw.DrawArrays(BeginMode.LineStrip, 0, 4);
            Raw.DisableClientState(ArrayCap.ColorArray);
            Raw.DisableClientState(ArrayCap.VertexArray);
        }

        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            //throw new NotImplementedException();
        }

        public void DrawReflection(int X, int Y, int Width, int Height, OImage image, float percent, float angle)
        {
            //throw new NotImplementedException();
        }

        public void DrawRoundRectangle(Pen pen, int x, int y, int width, int height, int radius)
        {
            //throw new NotImplementedException();
        }

        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            //throw new NotImplementedException();
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void FillEllipse(Brush brush, Rectangle rect)
        {
            //throw new NotImplementedException();
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            //throw new NotImplementedException();
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            FillRectangleSolid(brush.Color, x, y, width, height);
        }

        private void FillRectangleSolid(Color c, float x, float y, float width, float height)
        {
            float[] ar = new float[] { x, y, x + width, y, x, y + height, x + width, y + height };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.EnableClientState(ArrayCap.ColorArray);
            Raw.ColorPointer(4, ColorPointerType.UnsignedByte, 0, new byte[] { c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A });
            Raw.VertexPointer(2, VertexPointerType.Float, 0, ar);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            Raw.DisableClientState(ArrayCap.ColorArray);
            Raw.DisableClientState(ArrayCap.VertexArray);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            //throw new NotImplementedException();
        }

        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            //throw new NotImplementedException();
        }

        public void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            //throw new NotImplementedException();
        }

        public void Finish()
        {
            //throw new NotImplementedException();
        }

        public OImage GenerateStringTexture(string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format)
        {
            throw new NotImplementedException();
        }

        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            throw new NotImplementedException();
        }
        int maxTextureSize;
        public void Initialize(int screen)
        {
            Raw.Disable(EnableCap.DepthTest);
            Raw.Enable(EnableCap.Blend);
            Raw.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            Raw.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Raw.MatrixMode(MatrixMode.Projection);
            Raw.LoadIdentity();
            Raw.Ortho(0, 1000, 600, 0, 0, 1);
            Raw.MatrixMode(MatrixMode.Modelview);
            Raw.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
            //Init arrays
            normalTex = new int[] { 0, 1, 1, 1, 0, 0, 1, 0 };
        }

        public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC)
        {
            throw new NotImplementedException();
        }

        public void ResetClip()
        {
            Clip = NoClip;
        }

        public void ResetTransform()
        {
            //throw new NotImplementedException();
        }

        public void Resize(int Width, int Height)
        {
            width = Width;
            height = Height;
            wscale = (width / 1000F);
            hscale = (height / 600F);
            Raw.Viewport(0, 0, width, height);
        }

        public void SetClip(Rectangle Rect)
        {
            Clip = Rect;
        }

        public void SetClipFast(int x, int y, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void TranslateTransform(float dx, float dy)
        {
            Raw.Translate(dx, dy, 0);
        }

        public void TranslateTransform(float dx, float dy, float dz)
        {
            Raw.Translate(dx, dy, dz);
        }
    }
}
