﻿using OpenMobile.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace OpenMobile.Graphics
{
    public sealed class Graphics
    {
        static bool v2;
        static Bitmap virtualG;
        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        IGraphics implementation;
        static string version;
        static string renderer;

        public void Clear(Color color)
        {
            implementation.Clear(color);
        }

        public static string Version
        {
            get
            {
                return version;
            }
        }
        public int MaxTextureSize
        {
            get
            {
                return implementation.MaxTextureSize;
            }
        }
        public static string GraphicsEngine
        {
            get
            {
                if (v2)
                {
                    if (Platform.Factory.IsEmbedded)
                        return "GreenNitrous v2 ES";
                    else
                        return "GreenNitrous v2";
                }
                else
                    return "GreenNitrous v1";
            }
        }
        public static string Renderer
        {
            get
            {
                return renderer;
            }
        }
        public static string GLType
        {
            get
            {
                if (Platform.Factory.IsEmbedded)
                    return "OpenGL ES";
                else
                    return "OpenGL";
            }
        }
        internal static void DeleteTexture(int screen, uint texture)
        {
            if (v2)
                V2Graphics.DeleteTexture(screen, texture);
            else
                V1Graphics.DeleteTexture(screen, texture);
        }
        public Rectangle Clip
        {
            get
            {
                return implementation.Clip;
            }
            set
            {
                implementation.Clip = value;
            }
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            implementation.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            implementation.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            implementation.DrawEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            implementation.DrawEllipse(pen, rect);
        }

        public void DrawImage(OImage image, Rectangle rect, Rectangle srcRect)
        {
            implementation.DrawImage(image, rect, srcRect);
        }

        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height)
        {
            implementation.DrawImage(img, rect, x, y, width, height);
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency)
        {
            implementation.DrawImage(image, rect, transparency);
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency, eAngle angle)
        {
            implementation.DrawImage(image, rect, transparency, angle);
        }

        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency)
        {
            implementation.DrawImage(image, rect, x, y, Width, Height, transparency);
        }

        public void DrawImage(OImage image, Rectangle rect)
        {
            implementation.DrawImage(image, rect);
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height)
        {
            implementation.DrawImage(image, X,Y, Width, Height);
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            implementation.DrawImage(image, X, Y, Width, Height, transparency, angle);
        }

        public void DrawImage(OImage image, Point[] destPoints)
        {
            implementation.DrawImage(image, destPoints);
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            implementation.DrawImage(image, X, Y, Width, Height, transparency);
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            implementation.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            implementation.DrawLine(pen, pt1, pt2);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            implementation.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLine(Pen pen, Point[] points)
        {
            implementation.DrawLine(pen, points);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            implementation.DrawPolygon(pen, points);
        }
        public void PastePixels(IntPtr pixels, int x, int y, int width, int height)
        {
            Raw.DrawPixels(width, height, PixelFormat.Bgra, PixelType.UnsignedInt8888, pixels);
        }
        public IntPtr CopyPixels(int x, int y, int width, int height)
        {
            IntPtr ret=IntPtr.Zero;
            Raw.ReadPixels(x, y, width, height, PixelFormat.Bgra, PixelType.UnsignedInt8888, ret);
            return ret;
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            implementation.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            implementation.DrawRectangle(pen, rect);
        }

        public void DrawReflection(int X, int Y, int Width, int Height, OImage image, float percent, float angle)
        {
            implementation.DrawReflection(X, Y, Width, Height, image, percent, angle);
        }

        public void DrawRoundRectangle(Pen pen, int x, int y, int width, int height, int radius)
        {
            implementation.DrawRoundRectangle(pen, x, y, width, height, radius);
        }

        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            implementation.DrawRoundRectangle(p,rect,radius);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            implementation.FillEllipse(brush, x, y, width, height);
        }

        public void FillEllipse(Brush brush, Rectangle rect)
        {
            implementation.FillEllipse(brush, rect);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            implementation.FillPolygon(brush, points);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            implementation.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            implementation.FillRectangle(brush, rect);
        }

        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            implementation.FillRoundRectangle(brush, x, y, width, height, radius);
        }

        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            implementation.FillRoundRectangle(brush, rect, radius);
        }

        public void Finish()
        {
            implementation.Finish();
        }

        public OImage GenerateStringTexture(string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format)
        {
            return GenerateStringTexture(null, font, color, Left, Top, Width, Height, format);
        }
        public OImage GenerateStringTexture(OImage image, string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)(Width * scaleWidth[screen]), (int)(Height * scaleHeight[screen]));
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                g.DrawString(s, new System.Drawing.Font(font.Name, font.Size/dpi, (System.Drawing.FontStyle)font.Style), new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), new System.Drawing.RectangleF(0, 0, Width, Height), format);
            }
            image = new OImage(bmp);
            return image;
        }

        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return GenerateTextTexture(null, x, y, w, h, text, font, format, alignment, color, secondColor);
        }
        public OImage GenerateTextTexture_old(OImage image, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            if ((w <= 0) || (h <= 0))
                return null;

            // Get scaled sizes
            int bmpW = (int)System.Math.Ceiling(w * (scaleWidth[screen] == 0F ? 1F : scaleWidth[screen]));
            int bmpH = (int)System.Math.Ceiling(h * (scaleHeight[screen] == 0F ? 1F : scaleHeight[screen]));

            // Let's verify that sizes isn't zero, if so as a last resort set it to 1
            if (bmpW <= 0)
                bmpW = w;
            if (bmpW <= 0)
                bmpW = 1;
            if (bmpH <= 0)
                bmpH = h;
            if (bmpH <= 0)
                bmpH = 1;

            System.Drawing.Bitmap bmp = new Bitmap(bmpW, bmpH);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                if ((scaleWidth[screen] > 0F) & (scaleHeight[screen] > 0F))
                    g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor);
            } 
            image = new OImage(bmp);
            return image;
        }
        public OImage GenerateTextTexture(OImage image, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            //if (image == null && image.image == null)
            //    return;
            
            if ((w <= 0) || (h <= 0))
                return image;

            // Get scaled sizes
            int bmpW = (int)System.Math.Ceiling(w * (scaleWidth[screen] == 0F ? 1F : scaleWidth[screen]));
            int bmpH = (int)System.Math.Ceiling(h * (scaleHeight[screen] == 0F ? 1F : scaleHeight[screen]));

            // Let's verify that sizes isn't zero, if so as a last resort set it to 1
            if (bmpW <= 0)
                bmpW = w;
            if (bmpW <= 0)
                bmpW = 1;
            if (bmpH <= 0)
                bmpH = h;
            if (bmpH <= 0)
                bmpH = 1;

            if (image == null || image.image == null)
            {
                System.Drawing.Bitmap bmp = new Bitmap(bmpW, bmpH);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    if ((scaleWidth[screen] > 0F) & (scaleHeight[screen] > 0F))
                        g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor);
                }
                if (image != null)
                    image.Dispose();
                image = new OImage(bmp);
                return image;
            }
            else
            {   // Reuse already assigned image object
                System.Drawing.Bitmap bmp = image.image;
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    if ((scaleWidth[screen] > 0F) & (scaleHeight[screen] > 0F))
                        g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor);
                }

                image.image = bmp;
                return image;
            }
        }

        static public OImage GenerateTextTexture(OImage image, int screen, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            if ((w <= 0) || (h <= 0))
                return null;

            // Get scaled sizes
            int bmpW = (int)System.Math.Ceiling(w * (scaleWidth[screen] == 0F ? 1F : scaleWidth[screen]));
            int bmpH = (int)System.Math.Ceiling(h * (scaleHeight[screen] == 0F ? 1F : scaleHeight[screen]));

            // Let's verify that sizes isn't zero, if so as a last resort set it to 1
            if (bmpW <= 0)
                bmpW = w;
            if (bmpW <= 0)
                bmpW = 1;
            if (bmpH <= 0)
                bmpH = h;
            if (bmpH <= 0)
                bmpH = 1;

            if (image == null || image.image == null)
            {
                System.Drawing.Bitmap bmp = new Bitmap(bmpW, bmpH);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    if ((scaleWidth[screen] > 0F) & (scaleHeight[screen] > 0F))
                        g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor);
                }
                if (image != null)
                    image.Dispose();
                image = new OImage(bmp);
                return image;
            }
            else
            {   // Reuse already assigned image object
                System.Drawing.Bitmap bmp = image.image;
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    if ((scaleWidth[screen] > 0F) & (scaleHeight[screen] > 0F))
                        g.ScaleTransform(scaleWidth[screen], scaleHeight[screen]);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor);
                }

                image.image = bmp;
                return image;
            }
        }

        private int _screen;
        /// <summary>
        /// Screen number this graphics object belongs to
        /// </summary>
        public int screen
        {
            get { return _screen; }
        }

        public Graphics(int screen)
        {
            // Initialize scale factors
            scaleHeight = new float[DisplayDevice.AvailableDisplays.Count];
            scaleWidth = new float[DisplayDevice.AvailableDisplays.Count];

            _screen = screen;
            virtualG = new Bitmap(1000, 600);
        }
        static private float[] scaleHeight;
        static private float[] scaleWidth;
        static private float dpi=1F;
        public void Initialize(int screen)
        {
            #if LINUX
            if (Platform.Factory.IsEmbedded)
            {
                version = ES11.Raw.GetString(OpenMobile.Graphics.ES11.StringName.Version);
                v2 = true;
                implementation = new ESGraphics(screen);
                renderer = ES11.Raw.GetString(OpenMobile.Graphics.ES11.StringName.Renderer);
            }
            else
            #endif
            {
                version = Raw.GetString(StringName.Version);
                if (version.Length < 3)
                    v2 = false;
                else if (version[0] >= '2') //2.0 or higher
                    v2 = true;
                else if (version.Substring(0, 3) == "1.5") //1.5 with npots support
                {
                    string[] extensions = Raw.GetString(StringName.Extensions).Split(new char[] { ' ' });
                    v2 = (Array.Exists(extensions, t => t == "GL_ARB_texture_non_power_of_two"));
                }
                if (v2)
                    implementation = new V2Graphics(screen);
                else
                    implementation = new V1Graphics(screen);
                renderer = Raw.GetString(StringName.Renderer);
            }
            scaleHeight[screen] = (DisplayDevice.AvailableDisplays[screen].Height / 600F);
            scaleWidth[screen] = (DisplayDevice.AvailableDisplays[screen].Width / 1000F);
            if (Configuration.RunningOnWindows)
            {
                IntPtr dc = GetDC(IntPtr.Zero);
                dpi = GetDeviceCaps(dc, 88) / 96F;
            }
            implementation.Initialize(screen);
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public static SizeF MeasureString(String str, Font ft)
        {
            lock (virtualG)
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);
                System.Drawing.SizeF ret=gr.MeasureString(str, new System.Drawing.Font(ft.Name, ft.Size, (System.Drawing.FontStyle)ft.Style));
                return new SizeF(ret.Width,ret.Height);
            }
        }

        public static SizeF MeasureString(String str, Font ft, eTextFormat format)
        {
            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            switch (format)
            {
                case eTextFormat.Bold:
                case eTextFormat.BoldGlow:
                case eTextFormat.BoldShadow:
                    style = System.Drawing.FontStyle.Bold;
                    break;
                case eTextFormat.Italic:
                case eTextFormat.ItalicShadow:
                    style = System.Drawing.FontStyle.Italic;
                    break;
            }
            lock (virtualG)
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);
                System.Drawing.SizeF ret = gr.MeasureString(str, new System.Drawing.Font(ft.Name, ft.Size, style));
                return new SizeF(ret.Width, ret.Height);
            }
        }
        public static SizeF MeasureString(String str, Font ft, int width)
        {
            lock (virtualG)
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);
                System.Drawing.SizeF ret = gr.MeasureString(str, new System.Drawing.Font(ft.Name, ft.Size, (System.Drawing.FontStyle)ft.Style), width);
                return new SizeF(ret.Width, ret.Height);
            }
        }
        public static Rectangle MeasureCharacterRanges(string text, Font font, Rectangle rect, StringFormat format)
        {
            System.Drawing.RectangleF input = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
            lock (virtualG)
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);
                System.Drawing.RectangleF ret = gr.MeasureCharacterRanges(text, new System.Drawing.Font(font.Name, font.Size, (System.Drawing.FontStyle)font.Style), input, format)[0].GetBounds(gr);
                return new Rectangle(ret.X, ret.Y, ret.Width, ret.Height);
            }
        }
        public System.Drawing.Font GetSystemFont(Font font, eTextFormat format)
        {
            return new System.Drawing.Font(font.Name, font.Size / dpi, (System.Drawing.FontStyle)Font.FormatToStyle(format));
        }
        static public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
            System.Drawing.Color secondColor = System.Drawing.Color.FromArgb(sC.A, sC.R, sC.G, sC.B);
            if (string.IsNullOrEmpty(text))
                return;
            FontStyle f = FontStyle.Regular;
            if ((format == eTextFormat.Bold) || (format == eTextFormat.BoldShadow) || (format == eTextFormat.BoldGlow))
            {
                f = FontStyle.Bold;
            }
            else if ((format == eTextFormat.Italic) || (format == eTextFormat.ItalicShadow))
            {
                f = FontStyle.Italic;
            }
            else if ((format == eTextFormat.Underline) || (format == eTextFormat.UnderlineShadow))
            {
                f = FontStyle.Underline;
            }
            if (format == eTextFormat.Outline | format == eTextFormat.OutlineNarrow | format == eTextFormat.OutlineFat)
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    if (alignment == Alignment.CenterLeftEllipsis)
                        sFormat.Trimming = StringTrimming.EllipsisWord;
                    GraphicsPath path = new GraphicsPath(FillMode.Winding);

                    path.AddString(text, new System.Drawing.Font(font.Name, 1F).FontFamily, (int)f, (font.Size + 6)/dpi, new RectangleF(x, y, w, h), sFormat);
                    g.FillPath(new SolidBrush(color), path);
                    if (format == eTextFormat.Outline)
                        g.DrawPath(new System.Drawing.Pen(secondColor, 3), path);
                    else if (format == eTextFormat.OutlineFat)
                        g.DrawPath(new System.Drawing.Pen(secondColor, 6), path);
                    else
                        g.DrawPath(new System.Drawing.Pen(secondColor, 1), path);
                }
            }
            else if ((format == eTextFormat.Glow) || (format == eTextFormat.BoldGlow))
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    if (alignment == Alignment.CenterLeftEllipsis)
                        sFormat.Trimming = StringTrimming.EllipsisWord;
                    using (System.Drawing.Font currentFont = new System.Drawing.Font(font.Name, font.Size/dpi, (System.Drawing.FontStyle)f))
                    {
                        for (int i = -3; i < 3; i++)
                            for (int j = -3; j < 3; j++)
                                g.DrawString(text, currentFont, new SolidBrush(System.Drawing.Color.FromArgb(secondColor.A / (3 * (System.Math.Abs(i) + System.Math.Abs(j) + 1)), secondColor)), new RectangleF(x + i, y + j, w, h), sFormat);
                        g.DrawString(text, currentFont, new SolidBrush(color), new RectangleF(x, y, w, h), sFormat);
                    }
                }
            }
            else if ((format == eTextFormat.GlowBig))
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    if (alignment == Alignment.CenterLeftEllipsis)
                        sFormat.Trimming = StringTrimming.EllipsisWord;
                    using (System.Drawing.Font currentFont = new System.Drawing.Font(font.Name, font.Size / dpi, (System.Drawing.FontStyle)f))
                    {
                        GraphicsPath gpTxt = new GraphicsPath();
                        gpTxt.AddString(text, currentFont.FontFamily, (int)System.Drawing.FontStyle.Regular, currentFont.Size, new System.Drawing.Rectangle(x, y, w, h), sFormat);
                        for (int i = 1; i < 15; ++i)
                        {
                            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((secondColor.A>32 ? 32 : secondColor.A) - i, secondColor), i);
                            pen.LineJoin = LineJoin.Round;
                            g.DrawPath(pen, gpTxt);
                            pen.Dispose();
                        }
                        g.FillPath(new System.Drawing.SolidBrush(color), gpTxt);
                        g.DrawPath(new System.Drawing.Pen(secondColor, 0.5F), gpTxt);
                    }
                }
            }
            else
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    if (alignment == Alignment.CenterLeftEllipsis)
                        sFormat.Trimming = StringTrimming.EllipsisWord;

                    System.Drawing.Font currentFont = new System.Drawing.Font(font.Name, font.Size/dpi, (System.Drawing.FontStyle)f);

                    // Draw text
                    using (SolidBrush defaultBrush = new SolidBrush(color))
                    {
                        if (((int)alignment & 10000) != 10000)
                            sFormat.FormatFlags = StringFormatFlags.NoWrap; 
                        if (((int)format % 2) == 1)
                            g.DrawString(text, currentFont, new SolidBrush(secondColor), new RectangleF(x + 1, y + 2, w, h), sFormat);
                        g.DrawString(text, currentFont, defaultBrush, new RectangleF(x, y, w, h), sFormat);
                    }

                    currentFont.Dispose();
                }
            }
        }

        public void ResetClip()
        {
            implementation.ResetClip();
        }

        public void ResetTransform()
        {
            implementation.ResetTransform();
        }

        public void Resize(int Width, int Height)
        {
            implementation.Resize(Width, Height);
        }

        public void SetClip(Rectangle Rect)
        {
            implementation.SetClip(Rect);
        }

        public void SetClipFast(int x, int y, int width, int height)
        {
            implementation.SetClipFast(x, y, width, height);
        }

        public void TranslateTransform(float dx, float dy)
        {
            implementation.TranslateTransform(dx, dy);
        }

        public void TranslateTransform(float dx, float dy, float dz)
        {
            implementation.TranslateTransform(dx, dy, dz);
        }
    }
}
