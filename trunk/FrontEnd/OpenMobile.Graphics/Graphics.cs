﻿using OpenMobile.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using OpenMobile.Math;

namespace OpenMobile.Graphics
{
    public sealed class Graphics
    {
        static bool v2;
        static Bitmap virtualG = new Bitmap(1000, 600);
        static System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);

        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        IGraphics implementation;
        static string version;
        static string renderer;

        public enum Axis { X, Y, Z }

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
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)(Width * _ScaleFactors[screen].X), (int)(Height * _ScaleFactors[screen].Y));
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.ScaleTransform(_ScaleFactors[screen].X, _ScaleFactors[screen].Y);
                g.DrawString(s, new System.Drawing.Font(font.Name, font.Size/dpi, (System.Drawing.FontStyle)font.Style), new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), new System.Drawing.RectangleF(0, 0, Width, Height), format);
            }
            image = new OImage(bmp);
            return image;
        }

        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return GenerateTextTexture(null, x, y, w, h, text, font, format, alignment, color, secondColor);
        }
        public OImage GenerateTextTexture(OImage image, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            if ((w <= 0) || (h <= 0))
                return image;

            // Get scaled sizes
            int bmpW = (int)System.Math.Ceiling(w * _ScaleFactors[screen].X);
            int bmpH = (int)System.Math.Ceiling(h * _ScaleFactors[screen].Y);

            // Let's verify that sizes isn't zero, if so as a last resort set it to 1
            if (bmpW <= 0) bmpW = w;
            if (bmpW <= 0) bmpW = 1;
            if (bmpH <= 0) bmpH = h;
            if (bmpH <= 0) bmpH = 1;

            // Let's make sure the graphic surface we're creating is'nt too small to draw on
            if (((bmpW * _ScaleFactors[screen].X) < 1.0f) || ((bmpH * _ScaleFactors[screen].Y) < 1.0f))
                return null;

            if (image == null || image.image == null)
            {
                System.Drawing.Bitmap bmp = new Bitmap(bmpW, bmpH);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.ScaleTransform(_ScaleFactors[screen].X, _ScaleFactors[screen].Y);
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

                // Regenerate a new bitmap if sizes are different
                try
                {
                    if (bmpH != bmp.Height || bmpW != bmp.Width)
                    {
                        bmp.Dispose();
                        bmp = new Bitmap(bmpW, bmpH);
                    }
                }
                catch
                {
                    bmp.Dispose();
                    bmp = new Bitmap(bmpW, bmpH);
                }

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.ScaleTransform(_ScaleFactors[screen].X, _ScaleFactors[screen].Y);
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
            int bmpW = (int)System.Math.Ceiling(w * _ScaleFactors[screen].X);
            int bmpH = (int)System.Math.Ceiling(h * _ScaleFactors[screen].Y);

            // Let's verify that sizes isn't zero, if so as a last resort set it to 1
            if (bmpW <= 0) bmpW = w;
            if (bmpW <= 0) bmpW = 1;
            if (bmpH <= 0) bmpH = h;
            if (bmpH <= 0) bmpH = 1;

            // Let's make sure the graphic surface we're creating is'nt too small to draw on
            if (((bmpW * _ScaleFactors[screen].X) < 1.0f) || ((bmpH * _ScaleFactors[screen].Y) < 1.0f))
                return null;

            if (image == null || image.image == null)
            {
                System.Drawing.Bitmap bmp = new Bitmap(bmpW, bmpH);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.ScaleTransform(_ScaleFactors[screen].X, _ScaleFactors[screen].Y);
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

                // Regenerate a new bitmap if sizes are different
                try
                {
                    if (bmpH != bmp.Height || bmpW != bmp.Width)
                    {
                        bmp.Dispose();
                        bmp = new Bitmap(bmpW, bmpH);
                    }
                }
                catch
                {
                    bmp.Dispose();
                    bmp = new Bitmap(bmpW, bmpH);
                }

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.ScaleTransform(_ScaleFactors[screen].X, _ScaleFactors[screen].Y);
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
            //// Initialize scale factors
            _ScaleFactors = new PointF[DisplayDevice.AvailableDisplays.Count];
            for (int i = 0; i < _ScaleFactors.Length; i++)
                _ScaleFactors[i] = new PointF(1, 1);

            _screen = screen;
        }
        static private float dpi = 1F;
        static private PointF[] _ScaleFactors;

        /// <summary>
        /// Updates the scalefactors used internally in the graphics class
        /// </summary>
        /// <param name="Scale"></param>
        public void SetScaleFactors(PointF Scale)
        {
            _ScaleFactors[screen] = Scale;
        }

        public void Initialize()
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

        public static SizeF MeasureString(String text, Font f, eTextFormat format, Alignment alignment, Rectangle maxSize)
        {
            // Convert OpenMobile alignment to system StringFormat
            StringFormat sFormat = OpenMobile.Graphics.Font.AlignmentToStringFormat(alignment);

            // Convert OpenMobile fontstyle to system fontstyle
            System.Drawing.FontStyle fs = OpenMobile.Graphics.Font.FormatToStyle(format);            

            lock (virtualG)
            {
                using (System.Drawing.Font sysFont = new System.Drawing.Font(f.Name, f.Size, fs))
                {
                    System.Drawing.SizeF ret = gr.MeasureString(text, sysFont, new System.Drawing.SizeF(maxSize.Width, maxSize.Height), sFormat);
                    return new SizeF(ret.Width, ret.Height);
                }
            }
        }
        public static SizeF MeasureString(String text, Font f, eTextFormat format)
        {
            // Convert OpenMobile fontstyle to system fontstyle
            System.Drawing.FontStyle fs = OpenMobile.Graphics.Font.FormatToStyle(format);

            lock (virtualG)
            {
                using (System.Drawing.Font sysFont = new System.Drawing.Font(f.Name, f.Size, fs))
                {
                    System.Drawing.SizeF ret = gr.MeasureString(text, sysFont);
                    return new SizeF(ret.Width, ret.Height);
                }
            }
        }
        public static SizeF MeasureString(String text, Font f, eTextFormat format, Alignment alignment, int width)
        {
            // Convert OpenMobile alignment to system StringFormat
            StringFormat sFormat = OpenMobile.Graphics.Font.AlignmentToStringFormat(alignment);

            // Convert OpenMobile fontstyle to system fontstyle
            System.Drawing.FontStyle fs = OpenMobile.Graphics.Font.FormatToStyle(format);

            lock (virtualG)
            {
                using (System.Drawing.Font sysFont = new System.Drawing.Font(f.Name, f.Size, fs))
                {
                    System.Drawing.SizeF ret = gr.MeasureString(text, sysFont, width, sFormat);
                    return new SizeF(ret.Width, ret.Height);
                }
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
        static public System.Drawing.Font GetSystemFont(Font font, eTextFormat format)
        {
            return new System.Drawing.Font(font.Name, font.Size / dpi, (System.Drawing.FontStyle)Font.FormatToStyle(format));
        }

        /// <summary>
        /// Renders text to a System.Drawing.Graphics device using GDI+
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="c"></param>
        /// <param name="sC"></param>
        static public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC)
        {
            // Startup conditions
            if (string.IsNullOrEmpty(text))
                return;
            if (w == 1 | h == 1)
                return;

            // Convert OpenMobile colors to system colors
            System.Drawing.Color color = c.ToSystemColor();
            System.Drawing.Color secondColor = sC.ToSystemColor();

            // Convert OpenMobile fontstyle to system fontstyle
            System.Drawing.FontStyle f = OpenMobile.Graphics.Font.FormatToStyle(format);

            // Convert OpenMobile alignment to system StringFormat
            StringFormat sFormat = OpenMobile.Graphics.Font.AlignmentToStringFormat(alignment);

            // Set rendering parameters
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Create font object (Scaled to match the DrawString method)
            System.Drawing.Font currentFont = new System.Drawing.Font(font.Name, (font.Size / dpi) * 1.33f, (System.Drawing.FontStyle)f);

            // Set rendering rectangle 
            System.Drawing.Rectangle rectTxt =  new System.Drawing.Rectangle(x, y, w, h);

            // Create graphics path object
            GraphicsPath gpTxt = new GraphicsPath(FillMode.Winding);
            gpTxt.AddString(text, currentFont.FontFamily, (int)f, currentFont.Size, rectTxt, sFormat);
            
            // Render text with specifed effect
            switch (format)
            {
                case eTextFormat.DropShadow:
                case eTextFormat.BoldShadow:
                case eTextFormat.ItalicShadow:
                case eTextFormat.UnderlineShadow:
                    {
                        #region DropShadow

                        // Render drop shadow
                        using (GraphicsPath gpShadow = new GraphicsPath(FillMode.Winding))
                        {
                            // Add path for shadow
                            gpShadow.AddString(text, currentFont.FontFamily, (int)f, currentFont.Size, new System.Drawing.Rectangle(x + 1, y + 2, w, h), sFormat);

                            // Render shadow string
                            using (System.Drawing.SolidBrush b = new System.Drawing.SolidBrush(secondColor))
                                g.FillPath(b, gpShadow);
                        }

                        // Render normal text string
                        using (System.Drawing.SolidBrush b = new System.Drawing.SolidBrush(color))
                            g.FillPath(b, gpTxt);

                        #endregion
                    }
                    break;

                case eTextFormat.OutlineNarrow:
                case eTextFormat.OutlineFat:
                case eTextFormat.Outline:
                case eTextFormat.OutlineNoFill:
                case eTextFormat.OutlineNoFillNarrow:
                case eTextFormat.OutlineNoFillFat:
                case eTextFormat.OutlineItalicNarrow:
                case eTextFormat.OutlineItalicFat:
                case eTextFormat.OutlineItalic:
                case eTextFormat.OutlineItalicNoFill:
                case eTextFormat.OutlineItalicNoFillNarrow:
                case eTextFormat.OutlineItalicNoFillFat:
                    {
                        #region Outline

                        // Configure pen size
                        int PenSize = 1;
                        if (format == eTextFormat.Outline)
                            PenSize = 2;
                        else if (format == eTextFormat.OutlineFat)
                            PenSize = 5;

                        // Only render fill its required
                        if (format != eTextFormat.OutlineNoFill 
                            && format != eTextFormat.OutlineNoFillNarrow
                            && format != eTextFormat.OutlineNoFillFat
                            && format != eTextFormat.OutlineItalicNoFill
                            && format != eTextFormat.OutlineItalicNoFillNarrow
                            && format != eTextFormat.OutlineItalicNoFillFat)
                        {
                            // Render normal text string
                            using (System.Drawing.SolidBrush b = new System.Drawing.SolidBrush(color))
                                g.FillPath(b, gpTxt);
                        }

                        // Render path
                        using (System.Drawing.Pen p = new System.Drawing.Pen(secondColor, PenSize))
                            g.DrawPath(p, gpTxt);

                        #endregion
                    }
                    break;

                case eTextFormat.Glow:
                case eTextFormat.BoldGlow:
                case eTextFormat.GlowBig:
                case eTextFormat.GlowItalic:
                case eTextFormat.BoldGlowItalic:
                case eTextFormat.GlowItalicBig:
                case eTextFormat.GlowBoldBig:
                    {
                        #region Glow

                        // Configure glow size
                        int GlowSize = 8;
                        if (format == eTextFormat.GlowBig)
                            GlowSize = 15;

                        // Render glow effect
                        for (int i = 1; i < GlowSize; ++i)
                        {
                            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((secondColor.A > 32 ? 32 : secondColor.A) - i, secondColor), i))
                            {
                                pen.LineJoin = LineJoin.Round;
                                g.DrawPath(pen, gpTxt);
                            }
                        }

                        // Render text string 
                        using (System.Drawing.SolidBrush b = new System.Drawing.SolidBrush(color))
                            g.FillPath(b, gpTxt);
                        //using (System.Drawing.Pen p = new System.Drawing.Pen(secondColor, 0.5F))
                        //    g.DrawPath(p, gpTxt);

                        #endregion
                    }
                    break;

                // All other text effects
                default:
                    {
                        #region Normal 

                        using (System.Drawing.SolidBrush b = new System.Drawing.SolidBrush(color))
                            g.FillPath(b, gpTxt);

                        #endregion
                    }
                    break;
            }

            // Clean up
            currentFont.Dispose();
            gpTxt.Dispose();
            sFormat.Dispose();
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

        public void Rotate(Vector3 rotation)
        {
            Rotate(rotation.X, rotation.Y, rotation.Z);
        }
        public void Rotate(float ax, float ay, float az)
        {
            // Rotate in the order of X, Y and then Z
            if (ax != 0)
                implementation.Rotate(ax, Axis.X);
            if (ay != 0)
                implementation.Rotate(ay, Axis.Y);
            if (az != 0)
                implementation.Rotate(az, Axis.Z);
        }

        public void Scale(Vector3 scale)
        {
            Scale(scale.X, scale.Y, scale.Z);
        }
        public void Scale(float sx, float sy, float sz)
        {
            implementation.Scale(sx, sy, sz);
        }

    }
}
