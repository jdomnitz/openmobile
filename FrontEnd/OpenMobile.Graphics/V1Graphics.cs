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
using OpenMobile.Math;

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
        /// Outlined (normal 2 pixel)
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
        /// Outlined fat (5 pixel)
        /// </summary>
        OutlineFat = 13,
        /// <summary>
        /// Outlined without any fill
        /// </summary>
        OutlineNoFill,
        /// <summary>
        /// Outlined without any fill narrow (1 pixel)
        /// </summary>
        OutlineNoFillNarrow,
        /// <summary>
        /// Outlined without any fill fat (5 pixel)
        /// </summary>
        OutlineNoFillFat,
        /// <summary>
        /// Outlined italic
        /// </summary>
        OutlineItalic,
        /// <summary>
        /// Outlined italic narrow (1 pixel)
        /// </summary>
        OutlineItalicNarrow,
        /// <summary>
        /// Outlined italic fat (5 pixel)
        /// </summary>
        OutlineItalicFat,
        /// <summary>
        /// Outlined italic without any fill
        /// </summary>
        OutlineItalicNoFill,
        /// <summary>
        /// Outlined italic without any fill narrow (1 pixel)
        /// </summary>
        OutlineItalicNoFillNarrow,
        /// <summary>
        /// Outlined italic without any fill fat (5 pixel)
        /// </summary>
        OutlineItalicNoFillFat,
        /// <summary>
        /// Glowing Italic Text
        /// </summary>
        GlowItalic,
        /// <summary>
        /// Bold Glowing Italic Text
        /// </summary>
        BoldGlowItalic,
        /// <summary>
        /// Glowing Italic Text (big glow)
        /// </summary>
        GlowItalicBig,
        /// <summary>
        /// Glowing bold Text (big glow)
        /// </summary>
        GlowBoldBig,

    };
    /// <summary>
    /// Alignment arguments
    /// </summary>
    [Flags]
    public enum Alignment
    {
        /// <summary>
        /// Top Left
        /// </summary>
        TopLeft = 1,
        /// <summary>
        /// Top Center
        /// </summary>
        TopCenter = 2,
        /// <summary>
        /// Top Right
        /// </summary>
        TopRight = 4,
        /// <summary>
        /// Center Left
        /// </summary>
        CenterLeft = 8,
        /// <summary>
        /// Center Center
        /// </summary>
        CenterCenter = 16,
        /// <summary>
        /// Center Right
        /// </summary>
        CenterRight = 32,
        /// <summary>
        /// Buttom Left
        /// </summary>
        BottomLeft = 64,
        /// <summary>
        /// Bottom Center
        /// </summary>
        BottomCenter = 128,
        /// <summary>
        /// Bottom Right
        /// </summary>
        BottomRight = 256,
        /// <summary>
        /// Vertical
        /// </summary>
        VerticalCentered = 512,
        /// <summary>
        /// Center Left (Ellipsis)
        /// </summary>
        CenterLeftEllipsis = 1024,
        /// <summary>
        /// Word Wrap
        /// </summary>
        WordWrap = 2048
    };

    /// <summary>
    /// Fit modes
    /// </summary>
    public enum FitModes
    {
        /// <summary>
        /// No fit is active
        /// </summary>
        None,
        /// <summary>
        /// Ensure large objects fit inside control
        /// </summary>
        Fit,
        /// <summary>
        /// Scale small object to fill the control
        /// </summary>
        Fill,
        /// <summary>
        /// Scale large objects down to fit and scale small object up to fill
        /// </summary>
        FitFill,
        /// <summary>
        /// Scale complete string on one single line down to match control
        /// </summary>
        FitSingleLine,
        /// <summary>
        /// Scale complete string on one single line up to match control
        /// </summary>
        FillSingleLine,
        /// <summary>
        /// Scale complete string on one single line up or down to match control
        /// </summary>
        FitFillSingleLine
    }

    /// <summary>
    /// Control size modes
    /// </summary>
    public enum ControlSizeMode
    {
        /// <summary>
        /// No automatically sizing of control
        /// </summary>
        None,
        /// <summary>
        /// Control can grow vertically to fit content
        /// </summary>
        GrowVertically,
        /// <summary>
        /// Control can grow horizontally to fit content
        /// </summary>
        GrowHorizontally,
        /// <summary>
        /// Control can grow in both direction to fit content
        /// </summary>
        GrowBoth
    }

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
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1000, 600, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            string version=GL.GetString(StringName.Version);
            string[] extensions = GL.GetString(StringName.Extensions).Split(new char[]{' '});
            if (Array.Exists(extensions,t=>t=="GL_ARB_texture_non_power_of_two"))
                npot = true;
            AA = ((version[0] != '1') || (int.Parse(version[2].ToString()) >= 3));
            #if DEBUG
            try
            {
            #endif
            if (AA)
                GL.Disable(EnableCap.Multisample);
            #if DEBUG
            }catch(Exception){}
            #endif
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
        }
        public void Begin()
        {
        }
        public void End()
        {
            if (textures[screen].Count > 0)
            {
                GL.DeleteTextures(textures[screen].Count, textures[screen].ToArray());
                textures[screen].Clear();
            }
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
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(color);
        }
        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Multisample);
            GL.Begin(BeginMode.LineStrip);
            float yrad = height / 2F;
            float xrad = width / 2F;
            for (double t = startAngle; t <= (startAngle+sweepAngle); t=t+0.5)
            {
                double rad = t * DEG2RAD;
                GL.Vertex2(x + xrad + (xrad * System.Math.Cos(rad)), y + yrad + (yrad * System.Math.Sin(rad)));
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.Multisample);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        const float DEG2RAD = (float)(System.Math.PI / 180);
        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(BeginMode.LineLoop);
            {
                float yrad = height / 2F;
                float xrad = width / 2F;
                for (double t = 0; t < 360; t++)
                {
                    double rad = t * DEG2RAD;
                    GL.Vertex2(x + xrad + (xrad * System.Math.Cos(rad)), y + yrad + (yrad * System.Math.Sin(rad)));
                }
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public void DrawPoint(Color color, int x, int y, int width, int height)
        {
            //// Save matrix
            //GL.PushMatrix();

            //// Move base point to 0,0,0
            //_3D_Translate(0, 0, 0);

            //GL.Color4(pen.Color);
            //GL.PointSize(System.Math.Min(width, height));

            //GL.Begin(BeginMode.Points);
            //GL.Vertex3(x, y, 0);
            //GL.End();

            //// Restore matrix
            //GL.PopMatrix();
        }

        public void DrawImage(OImage image, Point[] destPoints)
        {
            if (destPoints.Length != 4)
                return;
            if (image == null)
                return;
            GL.Enable(EnableCap.Texture2D);
            if (image.GetTexture(screen) == 0)
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(TextureTarget.Texture2D, image.GetTexture(screen));
            GL.Color4(Color.White);
            GL.Begin(BeginMode.Quads);
            {
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(destPoints[0].X, destPoints[0].Y);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(destPoints[1].X, destPoints[1].Y);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(destPoints[2].X, destPoints[2].Y);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(destPoints[3].X, destPoints[3].Y);
            }
            GL.End();
            GL.Disable(EnableCap.Texture2D);
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
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle, Math.Vector3 rotation)
        {
        }
        public void DrawImage(OImage image, int X, int Y, int Z, int Width, int Height, float transparency, eAngle angle, Math.Vector3 rotation, ReflectionsData reflectionData)
        {
        }
        public void DrawImage(OImage image,int X, int Y,int Width, int Height,float transparency,eAngle angle)
        {
            if (image == null)
                return;
            GL.Enable(EnableCap.Texture2D);
            if (image.TextureGenerationRequired(screen))
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(TextureTarget.Texture2D, image.GetTexture(screen));
            GL.Color4(1F,1F,1F,transparency);
            GL.Begin(BeginMode.Quads);
            {
                switch(angle)
                {
                    case eAngle.Normal:
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X, Height+Y);
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width+X, Height+Y);
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width+X, Y);
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X, Y);
                        break;
                    case eAngle.FlipHorizontal:
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(X, Height + Y); //BL
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(Width + X, Height + Y); //BR
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(Width + X, Y); //TR
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(X, Y); //TL
                        break;
                    case eAngle.FlipVertical:
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X, Height + Y);
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width + X, Height + Y);
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width + X, Y);
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate90:
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(X, Height + Y);
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width + X, Height + Y);
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(Width + X, Y);
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate180:
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(X, Height + Y);
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(Width + X, Height + Y);
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(Width + X, Y);
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(X, Y);
                        break;
                    case eAngle.Rotate270:
                        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X, Height + Y);
                        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(Width + X, Height + Y);
                        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width + X, Y);
                        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(X, Y);
                        break;
                }
            }
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public void DrawCube(OImage[] image, int x, int y, int z, double width, double height, int depth, Vector3 rotation)
        {
        }

        public bool LoadTexture(ref OImage image)
        {
            if (image == null || image.Size == Size.Empty)
                return false;
            
            // Load texture
            uint texture = image.GetTexture(screen);

            // Delete old texture before generating a new one
            if (texture != 0)
                GL.DeleteTexture(texture);

            // Create new texture name
            GL.GenTextures(1, out texture);

            // Bind texture to opengl
            GL.BindTexture(TextureTarget.Texture2D, texture);

            Bitmap img = image.image;
            bool kill = false;
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
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
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
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
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
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                        OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        tmp.UnlockBits(data);
                        tmp.Dispose();
                        break;
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(screen, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            //Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
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

            GL.Enable(EnableCap.Texture2D);
            if (image.GetTexture(screen) == 0)
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(TextureTarget.Texture2D, image.GetTexture(screen));
            GL.Color4(Color.White);
            GL.Begin(BeginMode.Quads);
            {
                GL.TexCoord2(srcX, srcHeight); GL.Vertex2(rect.X, rect.Height+rect.Y);
                GL.TexCoord2(srcWidth, srcHeight); GL.Vertex2(rect.Width+rect.X, rect.Height+rect.Y);
                GL.TexCoord2(srcWidth, srcY); GL.Vertex2(rect.Width+rect.X, rect.Y);
                GL.TexCoord2(srcX, srcY); GL.Vertex2(rect.X, rect.Y);
            }
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
        public void DrawLine(Pen pen, Point[] points)
        {
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Begin(BeginMode.LineStrip);
            foreach (Point p in points)
                GL.Vertex2(p.X, p.Y);
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
        }
        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            if (pen.DashStyle != DashStyle.Solid)
                GL.Enable(EnableCap.LineStipple);
            if (pen.DashStyle == DashStyle.Dot)
                GL.LineStipple(2, 0xAAAA);
            else if (pen.DashStyle != DashStyle.Solid)
                GL.LineStipple(2, 0xFF);
            GL.Begin(BeginMode.Lines);
            {
                GL.Vertex2(x1, y1);
                GL.Vertex2(x2, y2);
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.LineStipple);
        }
        public void DrawPolygon(Pen pen, Point[] points)
        {
            if (points.Length < 3)
                return;
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.LineSmooth);
            GL.Begin(BeginMode.Lines);
            {
                for (int i = 0; i < points.Length-1; i++)
                {
                    GL.Vertex2(points[i].X, points[i].Y);
                    GL.Vertex2(points[i+1].X, points[i+1].Y);
                }
                GL.Vertex2(points[points.Length - 1].X, points[points.Length - 1].Y);
                GL.Vertex2(points[0].X, points[0].Y);
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
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
                    GL.Disable(EnableCap.ScissorTest);
                else
                {
                    if (_clip.Height<0)
                        _clip.Height=0;
                    if (_clip.Width < 0)
                        _clip.Width=0;
                    GL.Enable(EnableCap.ScissorTest);
                    try
                    {
                        GL.Scissor((int)(_clip.X * wscale), (int)((600 - _clip.Y - _clip.Height) * hscale), (int)(_clip.Width * wscale), (int)(_clip.Height * hscale));
                    }
                    catch(Exception)
                    {
                        _clip = NoClip;
                        GL.Disable(EnableCap.ScissorTest);
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
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            double ang = 0;
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(BeginMode.Lines);
            {
                GL.Vertex2(x, y + radius);
                GL.Vertex2(x, y + height - radius);//Left Line 

                GL.Vertex2(x + radius, y);
                GL.Vertex2(x + width - radius, y);//Top Line 

                GL.Vertex2(x + width, y + radius);
                GL.Vertex2(x + width, y + height - radius);//Right Line 

                GL.Vertex2(x + radius, y + height);
                GL.Vertex2(x + width - radius, y + height);//Bottom Line 
            }
            GL.End();

            float cX = x + radius, cY = y + radius;
            GL.Begin(BeginMode.LineStrip);
            {
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius;
            }
            GL.End();
            GL.Begin(BeginMode.LineStrip);
            {
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            GL.End();
            GL.Begin(BeginMode.LineStrip);
            {
                cY = y + height - radius;
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            GL.End();
            GL.Begin(BeginMode.LineStrip);
            {
                cX = x + radius;
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
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
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(BeginMode.Polygon);
            {
                GL.Color4(gr.SecondColor);
                GL.Vertex2(x, y + height - radius); //(0,1)
                GL.Vertex2(x + radius, y + height); //(1,0)
                GL.Vertex2(x + width - radius, y + height); //(2,0)
                GL.Vertex2(x + width, y + height - radius); //(3,1)
            }
            GL.End();
            GL.Begin(BeginMode.Quads);
            {
                GL.Vertex2(x, y + height - radius); //(0,1)
                GL.Vertex2(x + width, y + height - radius); //(3,1)
                GL.Color4(gr.Color);
                GL.Vertex2(x + width, y + radius); //(3,2)
                GL.Vertex2(x, y + radius); //(0,2)
            }
            GL.End();
            GL.Begin(BeginMode.Polygon);
            {
                GL.Vertex2(x + width, y + radius); //(3,2)
                GL.Vertex2(x + width - radius, y); //(2,3)
                GL.Vertex2(x + radius, y); //(1,3)  
                GL.Vertex2(x, y + radius); //(0,2)
            }
            GL.End();

            float cX = x + radius, cY = y + radius; //Bottom left
            GL.Begin(BeginMode.TriangleFan);
            {
                GL.Vertex2(cX, cY);
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius; //bottom right
            }
            GL.End();
            GL.Begin(BeginMode.TriangleFan);
            {
                GL.Vertex2(cX, cY);
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            GL.End();
            GL.Color4(gr.SecondColor);
            GL.Begin(BeginMode.TriangleFan);
            {
                cY = y + height - radius; //top right
                GL.Vertex2(cX, cY);
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            GL.End();
            GL.Begin(BeginMode.TriangleFan);
            {
                cX = x + radius; //top left
                GL.Vertex2(cX, cY);
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }
        private void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            GL.Color4(color);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(BeginMode.Polygon);
            {
                GL.Vertex2(x + width - radius, y);
                GL.Vertex2(x + radius, y);

                GL.Vertex2(x, y + radius);
                GL.Vertex2(x, y + height - radius);

                GL.Vertex2(x + radius, y + height);
                GL.Vertex2(x + width - radius, y + height);

                GL.Vertex2(x + width, y + height - radius);
                GL.Vertex2(x + width, y + radius);
            }
            GL.End();

            float cX = x + radius, cY = y + radius;
            GL.Begin(BeginMode.TriangleFan);
            {
                GL.Vertex2(cX, cY);
                for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Left 
                }
                cX = x + width - radius;
            }
            GL.End();
            GL.Begin(BeginMode.TriangleFan);
            {
                GL.Vertex2(cX, cY);
                for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Top Right 
                }
            }
            GL.End();
            GL.Begin(BeginMode.TriangleFan);
            {
                cY = y + height - radius;
                GL.Vertex2(cX, cY);
                for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY); //Bottom Right 
                }
            }
            GL.End();
            GL.Begin(BeginMode.TriangleFan);
            {
                cX = x + radius;
                GL.Vertex2(cX, cY);
                for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
                {
                    GL.Vertex2(radius * System.Math.Cos(ang) + cX, radius * System.Math.Sin(ang) + cY);//Bottom Left 
                }
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            GL.Color4(pen.Color);
            GL.LineWidth(pen.Width);
            GL.Begin(BeginMode.Lines);
            {
                GL.Vertex2(width+x, height+y);
                GL.Vertex2(width+x, y);

                GL.Vertex2(x, height+y);
                GL.Vertex2(width+x, height+y);

                GL.Vertex2(width+x, y);
                GL.Vertex2(x, y);

                GL.Vertex2(x, y);
                GL.Vertex2(x, height+y);
            }
            GL.End();
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
            GL.Color4(brush.Color);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            if (AA)
                GL.Enable(EnableCap.Multisample);
                GL.Begin(BeginMode.TriangleFan);
                {
                    GL.Vertex2(x + (width / 2), y + (height / 2));
                    for (int angle = 0; angle < 360; angle++)
                        GL.Vertex2(x + (width / 2) + System.Math.Sin(angle) * (width / 2), y + (height / 2) + System.Math.Cos(angle) * (height / 2));
                }
                GL.End();
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
            if (AA)
                GL.Disable(EnableCap.Multisample);
        }
        public void FillPolygon(Brush brush, Point[] points)
        {
            //TODO - Support LinearGradiantBrush
            GL.Color4(brush.Color);
            GL.Enable(EnableCap.LineSmooth);
            GL.Begin(BeginMode.Polygon);
            {
                foreach (Point p in points)
                    GL.Vertex2(p.X, p.Y);
            }
            GL.End();
            GL.Disable(EnableCap.LineSmooth);
        }
        public void FillRectangle(GradientData gradient, Rectangle rect, float opacity)
        {
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
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
            GL.Begin(BeginMode.Quads);
            {
                GL.Color4(brush.SecondColor);
                GL.Vertex2(x+width, y+height); //(1,1)
                GL.Vertex2(x+width, y); //(1,0)
                
                GL.Color4(brush.Color);
                GL.Vertex2(x, y);
                GL.Vertex2(x, y+height);
            }
            GL.End();
        }

        private void fillHorizontalRectangle(Brush brush, float x, float y, float width, float height)
        {
            GL.Begin(BeginMode.Quads);
            {
                GL.Color4(brush.SecondColor);
                GL.Vertex2(x+width, y+height);
                GL.Vertex2(x, y+height);

                GL.Color4(brush.Color);
                GL.Vertex2(x, y);
                GL.Vertex2(x+width, y);
            }
            GL.End();
        }
        private void fillRectangleSolid(Brush brush, float x, float y, float width, float height)
        {
            GL.Color4(brush.Color);
            GL.Begin(BeginMode.Quads);
            {
                GL.Vertex2(x+width, y+height);
                GL.Vertex2(x+width, y);
                GL.Vertex2(x, y);
                GL.Vertex2(x, y+height);
            }
            GL.End();
        }

        public void Resize(int Width, int Height)
        {
            width = Width;
            height = Height;
            wscale = (width / 1000F);
            hscale = (height / 600F);
            GL.Viewport(0, 0, width, height);
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
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor((int)(x * wscale), (int)((600 - y - height) * hscale), (int)(width * wscale), (int)(height * hscale));
        }
        public void ResetTransform()
        {
            GL.LoadIdentity();
        }
        public void Translate(double dx, double dy)
        {
            GL.Translate(dx, dy, 0);
        }
        public void Translate(double dx, double dy, double dz)
        {
            GL.Translate(dx, dy, dz);
        }
        public void Rotate(double angle, Graphics.Axis axis)
        {
            GL.Rotate(angle, (axis == Graphics.Axis.X ? 1 : 0), (axis == Graphics.Axis.Y ? 1 : 0), (axis == Graphics.Axis.Z ? 1 : 0));
        }

        public void Rotate(double x, double y, double z)
        {
            GL.Rotate(x, 1, 0, 0);
            GL.Rotate(y, 0, 1, 0);
            GL.Rotate(z, 0, 0, 1);
        }

        public void Scale(double sx, double sy, double sz)
        {
            GL.Scale(sx, sy, sz);
        }
        public void Transform(Matrix4d m)
        {
            //Raw.MultMatrix(ref m);
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
            GL.Enable(EnableCap.Texture2D);
            if (image.GetTexture(screen) == 0)
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(TextureTarget.Texture2D, image.GetTexture(screen));
            GL.Color4(1F, 1F, 1F, 0.0);
            GL.Begin(BeginMode.Quads);
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X, (int)(Height * percent) + Y);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width + X, (int)(Height * percent) + Y);
                GL.Color4(1F, 1F, 1F, 0.2);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width + X, Y);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X, Y);
            }
            GL.End();
            GL.Color4(1F, 1F, 1F, 0.0);
            GL.Begin(BeginMode.Quads);
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X, (int)(Height * percent) + Y + 2);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width + X, (int)(Height*percent) + Y+2);
                GL.Color4(1F, 1F, 1F, 0.2);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width + X, Y+2);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X, Y+2);
            }
            GL.End();
            GL.Color4(1F, 1F, 1F, 0.0);
            GL.Begin(BeginMode.Quads);
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(X+2, (int)(Height*percent) + Y);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(Width + 2 + X, (int)(Height * percent) + Y);
                GL.Color4(1F, 1F, 1F, 0.2);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(Width+2 + X, Y);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(X+2, Y);
            }
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public void _3D_ModelView_Set(Vector3d cameraLocation, Vector3d cameraRotation, Vector3d cameraOffset, double zoom, bool activateMatrix)
        {
            // Not implemented
        }

        public void _3D_ModelView_Push()
        {
            GL.PushMatrix();
        }

        public void _3D_ModelView_Pop()
        {
            GL.PopMatrix();
        }

        /// <summary>
        /// Sets or gets the modelview matrix
        /// </summary>
        public Matrix4d _3D_ModelViewMatrix
        {
            get
            {
                return this.modelview;
            }
            set
            {
                this.modelview = value;
            }
        }

        /// <summary>
        /// Activates the model view
        /// </summary>
        public void _3D_ModelView_ResetAndPlaceCamera(Vector3d cameraLocation)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref modelview);
        }


        /// <summary>
        /// The matrix for the global modelview
        /// </summary>
        private Matrix4d modelview;

    }
}
