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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenMobile.Graphics.OpenGL;

namespace OpenMobile.Graphics
{
    #region enums
    /// <summary>
    /// Text format arguments
    /// </summary>
    public enum eTextFormat
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Drop Shadow
        /// </summary>
        DropShadow = 1,
        /// <summary>
        /// Bold
        /// </summary>
        Bold = 2,
        /// <summary>
        /// Italic
        /// </summary>
        Italic = 4,
        /// <summary>
        /// Underlined
        /// </summary>
        Underline = 6,
        /// <summary>
        /// Bold and Drop Shadow
        /// </summary>
        BoldShadow = 3,
        /// <summary>
        /// Italic and Drop Shadow
        /// </summary>
        ItalicShadow = 5,
        /// <summary>
        /// Underlined and Drop Shadow
        /// </summary>
        UnderlineShadow = 7,
        /// <summary>
        /// Outlined (normal 3 pixel)
        /// </summary>
        Outline = 8,
        /// <summary>
        /// Glowing Text
        /// </summary>
        Glow = 9,
        /// <summary>
        /// Bold Glowing Text
        /// </summary>
        BoldGlow=10,
        /// <summary>
        /// Glowing Text (big glow)
        /// </summary>
        GlowBig = 11,
        /// <summary>
        /// Outlined narrow (1 pixel)
        /// </summary>
        OutlineNarrow = 12,
        /// <summary>
        /// Outlined fat (6 pixel)
        /// </summary>
        OutlineFat = 13,
    };
    /// <summary>
    /// Alignment arguments
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Top Left
        /// </summary>
        TopLeft = 0,
        /// <summary>
        /// Top Center
        /// </summary>
        TopCenter = 1,
        /// <summary>
        /// Top Right
        /// </summary>
        TopRight = 2,
        /// <summary>
        /// Center Left
        /// </summary>
        CenterLeft = 10,
        /// <summary>
        /// Center Center
        /// </summary>
        CenterCenter = 11,
        /// <summary>
        /// Center Right
        /// </summary>
        CenterRight = 12,
        /// <summary>
        /// Buttom Left
        /// </summary>
        BottomLeft = 20,
        /// <summary>
        /// Bottom Center
        /// </summary>
        BottomCenter = 21,
        /// <summary>
        /// Bottom Right
        /// </summary>
        BottomRight = 22,
        /// <summary>
        /// Vertical
        /// </summary>
        VerticalCentered = 111,
        /// <summary>
        /// Center Left (Ellipsis)
        /// </summary>
        CenterLeftEllipsis = 1010,
        /// <summary>
        /// Centered with Word Wrap
        /// </summary>
        WordWrap = 10011,
        /// <summary>
        /// Top Centered Word Wrap
        /// </summary>
        WordWrapTC = 10001,
        /// <summary>
        /// Top Left Word Wrap
        /// </summary>
        WordWrapTL = 10000
    };
    /// <summary>
    /// The angle to rotate the control
    /// </summary>
    public enum eAngle
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal,
        /// <summary>
        /// Flipped across the Y-axis
        /// </summary>
        FlipHorizontal,
        /// <summary>
        /// Flipped across the X-axis
        /// </summary>
        FlipVertical,
        /// <summary>
        /// Rotate clockwise 90degress
        /// </summary>
        Rotate90,
        /// <summary>
        /// Rotate clockwise 180degress
        /// </summary>
        Rotate180,
        /// <summary>
        /// Rotate clockwise 270degress
        /// </summary>
        Rotate270
    };
    #endregion
    public sealed class V1Graphics : IGraphics
    {
        #region private vars
            private static int[] POTS = { 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
            static List<List<uint>> textures = new List<List<uint>>();
            private static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
            private Rectangle _clip = NoClip;
            private int height;
            private int width;
            private int maxTextureSize;
            private bool npot;
            private bool AA;
            float wscale;
            float hscale;
        #endregion
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
            string version=Raw.GetString(StringName.Version);
            string[] extensions = Raw.GetString(StringName.Extensions).Split(new char[]{' '});
            if (Array.Exists(extensions,t=>t=="GL_ARB_texture_non_power_of_two"))
                npot = true;
            AA = ((version[0] != '1') || (int.Parse(version[2].ToString()) >= 3));
            #if DEBUG
            try
            {
            #endif
            if (AA)
                Raw.Disable(EnableCap.Multisample);
            #if DEBUG
            }catch(Exception){}
            #endif
            Raw.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
        }
        public int MaxTextureSize
        {
            get
            {
                return maxTextureSize;
            }
        }
        public void Clear(Color color)
        {
            Raw.Clear(ClearBufferMask.ColorBufferBit);
            Raw.ClearColor(color);
        }
        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.Multisample);
            Raw.Begin(BeginMode.LineStrip);
            float yrad = height / 2F;
            float xrad = width / 2F;
            for (double t = startAngle; t <= (startAngle+sweepAngle); t=t+0.5)
            {
                double rad = t * DEG2RAD;
                Raw.Vertex2(x + xrad + (xrad * System.Math.Cos(rad)), y + yrad + (yrad * System.Math.Sin(rad)));
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.Multisample);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        const float DEG2RAD = (float)(System.Math.PI / 180);
        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Begin(BeginMode.LineLoop);
            {
                float yrad = height / 2F;
                float xrad = width / 2F;
                for (double t = 0; t < 360; t++)
                {
                    double rad = t * DEG2RAD;
                    Raw.Vertex2(x + xrad + (xrad * System.Math.Cos(rad)), y + yrad + (yrad * System.Math.Sin(rad)));
                }
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }
        public void DrawImage(OImage image, Point[] destPoints)
        {
            if (destPoints.Length != 4)
                return;
            if (image == null)
                return;
            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.Color4(Color.White);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(destPoints[0].X, destPoints[0].Y);
                Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(destPoints[1].X, destPoints[1].Y);
                Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(destPoints[2].X, destPoints[2].Y);
                Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(destPoints[3].X, destPoints[3].Y);
            }
            Raw.End();
            Raw.Disable(EnableCap.Texture2D);
        }

        public void DrawImage(OImage image, Rectangle rect)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void DrawImage(OImage image, int X, int Y, int Width, int Height)
        {
            DrawImage(image, X, Y, Width, Height, 1F,eAngle.Normal);
        }
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            DrawImage(image, X, Y, Width, Height, transparency,eAngle.Normal);
        }
        public void DrawImage(OImage image,int X, int Y,int Width, int Height,float transparency,eAngle angle)
        {
            if (image == null)
                return;
            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.Color4(1F,1F,1F,transparency);
            Raw.Begin(BeginMode.Quads);
            {
                switch(angle)
                {
                    case eAngle.Normal:
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X, Height+Y);
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width+X, Height+Y);
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width+X, Y);
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X, Y);
                        break;
                    case eAngle.FlipHorizontal:
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(X, Height + Y); //BL
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(Width + X, Height + Y); //BR
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(Width + X, Y); //TR
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(X, Y); //TL
                        break;
                    case eAngle.FlipVertical:
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X, Height + Y);
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width + X, Height + Y);
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width + X, Y);
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate90:
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(X, Height + Y);
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width + X, Height + Y);
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(Width + X, Y);
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate180:
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(X, Height + Y);
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(Width + X, Height + Y);
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(Width + X, Y);
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate270:
                        Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X, Height + Y);
                        Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(Width + X, Height + Y);
                        Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width + X, Y);
                        Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(X, Y);
                        break;
                }
            }
            Raw.End();
            Raw.Disable(EnableCap.Texture2D);
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

            // Set correct image to use if image data is not shared among all screens
            if (!image.Shared)
                image.SetScreen(screen);

            Bitmap img = image.image;
            bool kill=false;
            lock (image.image)
            {
                if (!checkImage(image.image))
                {
                    kill = true;
                    fixImage(ref img);
                }
                BitmapData data;
                switch (img.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        try
                        {
                            data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                                        OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        try
                        {
                            data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                        OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                        Bitmap tmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        try
                        {
                            System.Drawing.Graphics.FromImage(tmp).DrawImage(img, 0, 0);
                            data = tmp.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                        OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        tmp.UnlockBits(data);
                        tmp.Dispose();
                        break;
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(screen,texture);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            return true;
        }

        private void fixImage(ref Bitmap image)
        {
            int width = getPOTS(image.Width);
            int height = getPOTS(image.Height);
            Bitmap TextureBitmap = new Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(TextureBitmap))
                g.DrawImage(image, 0, 0, width, height);
            image = TextureBitmap;
        }
        private int getPOTS(int current)
        {
            if (current >= maxTextureSize)
                return maxTextureSize;
            foreach (int i in POTS)
                if (i >= current)
                    return i;
            return maxTextureSize;
        }
        private bool checkImage(Image image)
        {
            if ((image.Width > maxTextureSize) || (image.Height > maxTextureSize))
                return false;
            if (npot)
                return true;
            bool wok=false, hok=false;
            foreach (int i in POTS)
                if (image.Height == i)
                    hok = true;
            foreach (int i in POTS)
                if (image.Width == i)
                    wok = true;
            return (wok && hok);
        }
        public void DrawImage(OImage image, Rectangle rect, Rectangle srcRect)
        {
            if (image == null)
                return;
            double srcX = srcRect.X / 1000.0;
            double srcY = srcRect.Y / 600.0;
            double srcWidth = srcRect.Width / 1000.0;
            double srcHeight = srcRect.Height / 600.0;

            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.Color4(Color.White);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.TexCoord2(srcX, srcHeight); Raw.Vertex2(rect.X, rect.Height+rect.Y);
                Raw.TexCoord2(srcWidth, srcHeight); Raw.Vertex2(rect.Width+rect.X, rect.Height+rect.Y);
                Raw.TexCoord2(srcWidth, srcY); Raw.Vertex2(rect.Width+rect.X, rect.Y);
                Raw.TexCoord2(srcX, srcY); Raw.Vertex2(rect.X, rect.Y);
            }
            Raw.End();
            Raw.Disable(EnableCap.Texture2D);
        }
        public void DrawLine(Pen pen, Point[] points)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Begin(BeginMode.LineStrip);
            foreach (Point p in points)
                Raw.Vertex2(p.X, p.Y);
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
        }
        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            if (pen.DashStyle != DashStyle.Solid)
                Raw.Enable(EnableCap.LineStipple);
            if (pen.DashStyle == DashStyle.Dot)
                Raw.LineStipple(2, 0xAAAA);
            else if (pen.DashStyle != DashStyle.Solid)
                Raw.LineStipple(2, 0xFF);
            Raw.Begin(BeginMode.Lines);
            {
                Raw.Vertex2(x1, y1);
                Raw.Vertex2(x2, y2);
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.LineStipple);
        }
        public void DrawPolygon(Pen pen, Point[] points)
        {
            if (points.Length < 3)
                return;
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Begin(BeginMode.Lines);
            {
                for (int i = 0; i < points.Length-1; i++)
                {
                    Raw.Vertex2(points[i].X, points[i].Y);
                    Raw.Vertex2(points[i+1].X, points[i+1].Y);
                }
                Raw.Vertex2(points[points.Length - 1].X, points[points.Length - 1].Y);
                Raw.Vertex2(points[0].X, points[0].Y);
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }
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
                    if (_clip.Height<0)
                        _clip.Height=0;
                    if (_clip.Width < 0)
                        _clip.Width=0;
                    Raw.Enable(EnableCap.ScissorTest);
                    try
                    {
                        Raw.Scissor((int)(_clip.X * wscale), (int)((600 - _clip.Y - _clip.Height) * hscale), (int)(_clip.Width * wscale), (int)(_clip.Height * hscale));
                    }
                    catch(Exception)
                    {
                        _clip = NoClip;
                        Raw.Disable(EnableCap.ScissorTest);
                    }
                }
            }
        }
        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void DrawRoundRectangle(Pen pen, int x, int y, int width, int height,int radius)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            double ang = 0;
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Begin(BeginMode.Lines);
            {
                Raw.Vertex2(x, y + radius);
                Raw.Vertex2(x, y + height - radius);//Left Line 

                Raw.Vertex2(x + radius, y);
                Raw.Vertex2(x + width - radius, y);//Top Line 

                Raw.Vertex2(x + width, y + radius);
                Raw.Vertex2(x + width, y + height - radius);//Right Line 

                Raw.Vertex2(x + radius, y + height);
                Raw.Vertex2(x + width - radius, y + height);//Bottom Line 
            }
            Raw.End();

            float cX = x + radius, cY = y + radius;
            Raw.Begin(BeginMode.LineStrip);
            {
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius;
            }
            Raw.End();
            Raw.Begin(BeginMode.LineStrip);
            {
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            Raw.End();
            Raw.Begin(BeginMode.LineStrip);
            {
                cY = y + height - radius;
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            Raw.End();
            Raw.Begin(BeginMode.LineStrip);
            {
                cX = x + radius;
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }
        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            FillRoundRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }
        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            DrawRoundRectangle(p, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }
        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            if (brush.Gradient==Gradient.None)
                FillSolidRoundRectangle(brush.Color,x,y,width,height,radius);
            else if (brush.Gradient==Gradient.Horizontal)
                FillHorizRoundRectangle(brush,x,y,width,height,radius);
            else
                FillVertRoundRectangle(brush, x, y, width, height, radius);
        }

        private void FillHorizRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            //TODO - Actually implement
            fillRectangleSolid(brush, x, y, width, height);
        }

        private void FillVertRoundRectangle(Brush gr, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Begin(BeginMode.Polygon);
            {
                Raw.Color4(gr.SecondColor);
                Raw.Vertex2(x, y + height - radius); //(0,1)
                Raw.Vertex2(x + radius, y + height); //(1,0)
                Raw.Vertex2(x + width - radius, y + height); //(2,0)
                Raw.Vertex2(x + width, y + height - radius); //(3,1)
            }
            Raw.End();
            Raw.Begin(BeginMode.Quads);
            {
                Raw.Vertex2(x, y + height - radius); //(0,1)
                Raw.Vertex2(x + width, y + height - radius); //(3,1)
                Raw.Color4(gr.Color);
                Raw.Vertex2(x + width, y + radius); //(3,2)
                Raw.Vertex2(x, y + radius); //(0,2)
            }
            Raw.End();
            Raw.Begin(BeginMode.Polygon);
            {
                Raw.Vertex2(x + width, y + radius); //(3,2)
                Raw.Vertex2(x + width - radius, y); //(2,3)
                Raw.Vertex2(x + radius, y); //(1,3)  
                Raw.Vertex2(x, y + radius); //(0,2)
            }
            Raw.End();

            float cX = x + radius, cY = y + radius; //Bottom left
            Raw.Begin(BeginMode.TriangleFan);
            {
                Raw.Vertex2(cX, cY);
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius; //bottom right
            }
            Raw.End();
            Raw.Begin(BeginMode.TriangleFan);
            {
                Raw.Vertex2(cX, cY);
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            Raw.End();
            Raw.Color4(gr.SecondColor);
            Raw.Begin(BeginMode.TriangleFan);
            {
                cY = y + height - radius; //top right
                Raw.Vertex2(cX, cY);
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            Raw.End();
            Raw.Begin(BeginMode.TriangleFan);
            {
                cX = x + radius; //top left
                Raw.Vertex2(cX, cY);
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }
        private void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            Raw.Color4(color);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Begin(BeginMode.Polygon);
            {
                Raw.Vertex2(x + width - radius, y);
                Raw.Vertex2(x + radius, y);

                Raw.Vertex2(x, y + radius);
                Raw.Vertex2(x, y + height - radius);

                Raw.Vertex2(x + radius, y + height);
                Raw.Vertex2(x + width - radius, y + height);

                Raw.Vertex2(x + width, y + height - radius);
                Raw.Vertex2(x + width, y + radius);
            }
            Raw.End();

            float cX = x + radius, cY = y + radius;
            Raw.Begin(BeginMode.TriangleFan);
            {
                Raw.Vertex2(cX, cY);
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius;
            }
            Raw.End();
            Raw.Begin(BeginMode.TriangleFan);
            {
                Raw.Vertex2(cX, cY);
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            Raw.End();
            Raw.Begin(BeginMode.TriangleFan);
            {
                cY = y + height - radius;
                Raw.Vertex2(cX, cY);
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            Raw.End();
            Raw.Begin(BeginMode.TriangleFan);
            {
                cX = x + radius;
                Raw.Vertex2(cX, cY);
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    Raw.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Begin(BeginMode.Lines);
            {
                Raw.Vertex2(width+x, height+y);
                Raw.Vertex2(width+x, y);

                Raw.Vertex2(x, height+y);
                Raw.Vertex2(width+x, height+y);

                Raw.Vertex2(width+x, y);
                Raw.Vertex2(x, y);

                Raw.Vertex2(x, y);
                Raw.Vertex2(x, height+y);
            }
            Raw.End();
        }
        int screen;
        public V1Graphics(int screen)
        {
            this.screen = screen;
            lock (textures)
            {
                if (textures.Count == 0)
                    for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                        textures.Add(new List<uint>());
            }
        }
        public OImage GenerateStringTexture(string s, Font font, Color color, int Left,int Top,int Width,int Height, StringFormat format)
        {
            throw new NotImplementedException();
        }
        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            throw new NotImplementedException();
        }
        public void FillEllipse(Brush brush, Rectangle rect)
        {
            FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            Raw.Color4(brush.Color);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            if (AA)
                Raw.Enable(EnableCap.Multisample);
                Raw.Begin(BeginMode.TriangleFan);
                {
                    Raw.Vertex2(x + (width / 2), y + (height / 2));
                    for (int angle = 0; angle < 360; angle++)
                        Raw.Vertex2(x + (width / 2) + System.Math.Sin(angle) * (width / 2), y + (height / 2) + System.Math.Cos(angle) * (height / 2));
                }
                Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
            if (AA)
                Raw.Disable(EnableCap.Multisample);
        }
        public void FillPolygon(Brush brush, Point[] points)
        {
            //TODO - Support LinearGradiantBrush
            Raw.Color4(brush.Color);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Begin(BeginMode.Polygon);
            {
                foreach (Point p in points)
                    Raw.Vertex2(p.X, p.Y);
            }
            Raw.End();
            Raw.Disable(EnableCap.LineSmooth);
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            if (brush.Gradient==Gradient.None)
                fillRectangleSolid(brush, x, y, width, height);
            else if (brush.Gradient==Gradient.Horizontal)
                fillHorizontalRectangle(brush, x, y, width, height);
            else
                fillVerticalRectangle(brush, x, y, width, height);
        }

        private void fillVerticalRectangle(Brush brush, float x, float y, float width, float height)
        {
            Raw.Begin(BeginMode.Quads);
            {
                Raw.Color4(brush.SecondColor);
                Raw.Vertex2(x+width, y+height); //(1,1)
                Raw.Vertex2(x+width, y); //(1,0)
                
                Raw.Color4(brush.Color);
                Raw.Vertex2(x, y);
                Raw.Vertex2(x, y+height);
            }
            Raw.End();
        }

        private void fillHorizontalRectangle(Brush brush, float x, float y, float width, float height)
        {
            Raw.Begin(BeginMode.Quads);
            {
                Raw.Color4(brush.SecondColor);
                Raw.Vertex2(x+width, y+height);
                Raw.Vertex2(x, y+height);

                Raw.Color4(brush.Color);
                Raw.Vertex2(x, y);
                Raw.Vertex2(x+width, y);
            }
            Raw.End();
        }
        private void fillRectangleSolid(Brush brush, float x, float y, float width, float height)
        {
            Raw.Color4(brush.Color);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.Vertex2(x+width, y+height);
                Raw.Vertex2(x+width, y);
                Raw.Vertex2(x, y);
                Raw.Vertex2(x, y+height);
            }
            Raw.End();
        }
        public void Finish()
        {
            if (textures[screen].Count > 0)
            {
                Raw.DeleteTextures(textures[screen].Count, textures[screen].ToArray());
                textures[screen].Clear();
            }
        }

        public void Resize(int Width, int Height)
        {
            width = Width;
            height = Height;
            wscale = (width / 1000F);
            hscale = (height / 600F);
            Raw.Viewport(0, 0, width, height);
        }

        internal static void DeleteTexture(int screen,uint texture)
        {
            if (screen<textures.Count)
                //textures[screen].Add(texture);
                textures[screen].Remove(texture);
        }
        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency)
        {
            if ((Width == 0) || (Height == 0))
                return;
            Rectangle clp = Clip;
            SetClipFast(rect.X, rect.Y, rect.Width, rect.Height);
            float xs = ((float)rect.Width / Width), ys = ((float)rect.Height / Height);
            DrawImage(image, rect.X - (int)(xs * x), rect.Y - (int)(y * ys), (int)(xs * (image.Width - x)), (int)(ys * (image.Height - y)), transparency, eAngle.Normal);
            Clip = clp;
        }
        public void DrawImage(OImage image, Rectangle rect, float transparency)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency,eAngle.Normal);
        }
        public void DrawImage(OImage image, Rectangle rect, float transparency,eAngle angle)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency,angle);
        }
        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height)
        {
            DrawImage(img, rect, x, y, width, height, 1F);
        }
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            DrawLine(pen,(int)x1,(int)y1,(int)x2,(int)y2);
        }
        [Obsolete("Set Clip Property Directly")]
        public void SetClip(Rectangle Rect)
        {
            Clip=Rect;
        }
        public void SetClipFast(int x, int y, int width, int height)
        {
            if (height<0)
                height=0;
            if (width < 0)
                width=0;
            Raw.Enable(EnableCap.ScissorTest);
            Raw.Scissor((int)(x * wscale), (int)((600 - y - height) * hscale), (int)(width * wscale), (int)(height * hscale));
        }
        public void ResetTransform()
        {
            Raw.LoadIdentity();
        }
        public void TranslateTransform(float dx, float dy)
        {
            Raw.Translate(dx, dy, 0);
        }
        public void TranslateTransform(float dx, float dy,float dz)
        {
            Raw.Translate(dx, dy, dz);
        }
        #region System.Drawing
        [Obsolete]
        public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void ResetClip()
        {
            Clip = NoClip;
        }

        public void DrawReflection(int X,int Y, int Width, int Height,OImage image, float percent, float angle)
        {
            if (image == null)
                return;
            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.Color4(1F, 1F, 1F, 0.0);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X, (int)(Height * percent) + Y);
                Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width + X, (int)(Height * percent) + Y);
                Raw.Color4(1F, 1F, 1F, 0.2);
                Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width + X, Y);
                Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X, Y);
            }
            Raw.End();
            Raw.Color4(1F, 1F, 1F, 0.0);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X, (int)(Height * percent) + Y + 2);
                Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width + X, (int)(Height*percent) + Y+2);
                Raw.Color4(1F, 1F, 1F, 0.2);
                Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width + X, Y+2);
                Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X, Y+2);
            }
            Raw.End();
            Raw.Color4(1F, 1F, 1F, 0.0);
            Raw.Begin(BeginMode.Quads);
            {
                Raw.TexCoord2(0.0f, 0.0f); Raw.Vertex2(X+2, (int)(Height*percent) + Y);
                Raw.TexCoord2(1.0f, 0.0f); Raw.Vertex2(Width + 2 + X, (int)(Height * percent) + Y);
                Raw.Color4(1F, 1F, 1F, 0.2);
                Raw.TexCoord2(1.0f, 1.0f); Raw.Vertex2(Width+2 + X, Y);
                Raw.TexCoord2(0.0f, 1.0f); Raw.Vertex2(X+2, Y);
            }
            Raw.End();
            Raw.Disable(EnableCap.Texture2D);
        }
    }
}
