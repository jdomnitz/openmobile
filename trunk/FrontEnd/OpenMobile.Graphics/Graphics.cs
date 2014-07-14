using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using OpenMobile.Math;

namespace OpenMobile.Graphics
{
    /// <summary>
    /// Data for reflections
    /// </summary>
    public class ReflectionsData
    {
        /// <summary>
        /// Creates new reflections data
        /// </summary>
        public ReflectionsData()
        {
        }

        /// <summary>
        /// Creates new reflections data
        /// <para>NB! Default reflection length is set to 0.2 (20%) of height. So height has to be 20% more than width to make room for reflection</para>
        /// </summary>
        /// <param name="reflectionColorEnd"></param>
        public ReflectionsData(Color reflectionColorEnd)
        {
            this._ReflectionColorEnd = reflectionColorEnd;
        }
        /// <summary>
        /// Creates new reflections data
        /// <para>NB! Default reflection length is set to 0.2 (20%) of height. So height has to be 20% more than width to make room for reflection</para>
        /// </summary>
        /// <param name="reflectionColorStart"></param>
        /// <param name="reflectionColorEnd"></param>
        public ReflectionsData(Color reflectionColorStart, Color reflectionColorEnd)
        {
            this._ReflectionColorStart = reflectionColorStart;
            this._ReflectionColorEnd = reflectionColorEnd;
        }
        /// <summary>
        /// Creates new reflections data
        /// <para>NB! Default reflection length is set to 0.2 (20%) of height. So height has to be 20% more than width to make room for reflection</para>
        /// </summary>
        /// <param name="reflectionColorStart"></param>
        /// <param name="reflectionColorEnd"></param>
        /// <param name="reflectionLength"></param>
        public ReflectionsData(Color reflectionColorStart, Color reflectionColorEnd, float reflectionLength)
        {
            this._ReflectionColorStart = reflectionColorStart;
            this._ReflectionColorEnd = reflectionColorEnd;
            this._ReflectionLength = reflectionLength;
        }
        /// <summary>
        /// Creates new reflections data
        /// <para>NB! Default reflection length is set to 0.2 (20%) of height. So height has to be 20% more than width to make room for reflection</para>
        /// </summary>
        /// <param name="reflectionColorStart"></param>
        /// <param name="reflectionColorEnd"></param>
        /// <param name="reflectionLength"></param>
        /// <param name="reflectionOffset"></param>
        public ReflectionsData(Color reflectionColorStart, Color reflectionColorEnd, float reflectionLength, int reflectionOffset)
        {
            this._ReflectionColorStart = reflectionColorStart;
            this._ReflectionColorEnd = reflectionColorEnd;
            this._ReflectionLength = reflectionLength;
            this._ReflectionOffset = reflectionOffset;
        }

        /// <summary>
        /// The color to start with when fading out the reflection
        /// </summary>
        public Color ReflectionColorStart
        {
            get
            {
                return this._ReflectionColorStart;
            }
            set
            {
                if (this._ReflectionColorStart != value)
                {
                    this._ReflectionColorStart = value;
                }
            }
        }
        private Color _ReflectionColorStart = Color.White;

        /// <summary>
        /// The color to end with when fading out the reflection
        /// </summary>
        public Color ReflectionColorEnd
        {
            get
            {
                return this._ReflectionColorEnd;
            }
            set
            {
                if (this._ReflectionColorEnd != value)
                {
                    this._ReflectionColorEnd = value;
                }
            }
        }
        private Color _ReflectionColorEnd = Color.Transparent;

        /// <summary>
        /// Length of reflection in percentage of total dimension (usually height)
        /// </summary>
        public float ReflectionLength
        {
            get
            {
                return this._ReflectionLength;
            }
            set
            {
                if (this._ReflectionLength != value)
                {
                    if (value >= 0 && value <= 1)
                        this._ReflectionLength = value;
                }
            }
        }
        private float _ReflectionLength = 0.2f;

        /// <summary>
        /// The offset for the reflection
        /// </summary>
        public int ReflectionOffset
        {
            get
            {
                return this._ReflectionOffset;
            }
            set
            {
                if (this._ReflectionOffset != value)
                {
                    this._ReflectionOffset = value;
                }
            }
        }
        private int _ReflectionOffset;        
    }

    public class MouseData
    {
        /// <summary>
        /// The mouse position
        /// </summary>
        public Point CursorPosition = new Point();
    }

    public sealed class Graphics
    {
        static bool v2;
        static Bitmap virtualG = new Bitmap(1000, 600);
        static System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);

        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        IGraphics implementation;
        static string version;
        static string renderer;

        /// <summary>
        /// OpenGL Coordinate axes
        /// </summary>
        public enum Axis { X, Y, Z }

        /// <summary>
        /// OpenGL Rendering init
        /// </summary>
        public void Begin()
        {
            implementation.Begin();
        }

        /// <summary>
        /// OpenGL Rendering End
        /// </summary>
        public void End()
        {
            implementation.End();
        }

        /// <summary>
        /// OpenGL Clear graphics
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
        {
            implementation.Clear(color);
        }

        /// <summary>
        /// OpenGL Version string
        /// </summary>
        public static string Version
        {
            get
            {
                return version;
            }
        }

        /// <summary>
        /// OpenGL Maximum texture size
        /// </summary>
        public int MaxTextureSize
        {
            get
            {
                return implementation.MaxTextureSize;
            }
        }

        /// <summary>
        /// OpenMobile graphics engine version
        /// </summary>
        public static string GraphicsEngine
        {
            get
            {
                if (v2)
                {
                    return "GreenNitrous v2";
                }
                else
                    return "GreenNitrous v1";
            }
        }

        /// <summary>
        /// OpenGL Renderer string
        /// </summary>
        public static string Renderer
        {
            get
            {
                return renderer;
            }
        }

        /// <summary>
        /// OpenGL Type
        /// </summary>
        public static string GLType
        {
            get
            {
                return "OpenGL";
            }
        }

        /// <summary>
        /// Unregister a OpenGL texture
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="texture"></param>
        internal static void DeleteTexture(int screen, uint texture)
        {
            if (v2)
                V2Graphics.DeleteTexture(screen, texture);
            else
                V1Graphics.DeleteTexture(screen, texture);
        }

        /// <summary>
        /// Gets or sets the current clipping rectangle
        /// </summary>
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

        /// <summary>
        /// Draws an arc
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            implementation.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }
        /// <summary>
        /// Draws an arc
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="rect"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            implementation.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        /// <summary>
        /// Draws an Ellipse
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            implementation.DrawEllipse(pen, x, y, width, height);
        }
        /// <summary>
        /// Draws an Ellipse
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="rect"></param>
        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            implementation.DrawEllipse(pen, rect);
        }

        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        /// <param name="srcRect"></param>
        public void DrawImage(OImage image, Rectangle rect, Rectangle srcRect)
        {
            implementation.DrawImage(image, rect, srcRect);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height)
        {
            implementation.DrawImage(img, rect, x, y, width, height);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        /// <param name="transparency"></param>
        public void DrawImage(OImage image, Rectangle rect, float transparency)
        {
            implementation.DrawImage(image, rect, transparency);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        /// <param name="transparency"></param>
        /// <param name="angle"></param>
        public void DrawImage(OImage image, Rectangle rect, float transparency, eAngle angle)
        {
            implementation.DrawImage(image, rect, transparency, angle);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="transparency"></param>
        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency)
        {
            implementation.DrawImage(image, rect, x, y, Width, Height, transparency);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        public void DrawImage(OImage image, Rectangle rect)
        {
            implementation.DrawImage(image, rect);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public void DrawImage(OImage image, int X, int Y, int Width, int Height)
        {
            implementation.DrawImage(image, X,Y, Width, Height);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="transparency"></param>
        /// <param name="angle"></param>
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            implementation.DrawImage(image, X, Y, Width, Height, transparency, angle);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="transparency"></param>
        /// <param name="angle"></param>
        /// <param name="rotation"></param>
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle, Vector3 rotation)
        {
            implementation.DrawImage(image, X, Y, Width, Height, transparency, angle, rotation.ToOpenTKVector3());
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="transparency"></param>
        /// <param name="angle"></param>
        /// <param name="rotation"></param>
        /// <param name="reflectionData"></param>
        public void DrawImage(OImage image, int X, int Y, int Z, int Width, int Height, float transparency, eAngle angle, Vector3 rotation, ReflectionsData reflectionData)
        {
            implementation.DrawImage(image, X, Y, Z, Width, Height, transparency, angle, rotation.ToOpenTKVector3(), reflectionData);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="destPoints"></param>
        public void DrawImage(OImage image, Point[] destPoints)
        {
            implementation.DrawImage(image, destPoints);
        }
        /// <summary>
        /// Draws an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="transparency"></param>
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            implementation.DrawImage(image, X, Y, Width, Height, transparency);
        }

        /// <summary>
        /// Draws a cube
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="rotation"></param>
        public void DrawCube(OImage[] image, int x, int y, int z, double width, double height, int depth, Vector3 rotation)
        {
            implementation.DrawCube(image, x, y, z, width, height, depth, rotation.ToOpenTKVector3());
        }

        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            implementation.DrawLine(pen, x1, y1, x2, y2);
        }
        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            implementation.DrawLine(pen, pt1, pt2);
        }
        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            implementation.DrawLine(pen, x1, y1, x2, y2);
        }
        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        public void DrawLine(Pen pen, Point[] points)
        {
            implementation.DrawLine(pen, points);
        }

        /// <summary>
        /// Draws a plygon line
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        public void DrawPolygon(Pen pen, Point[] points)
        {
            implementation.DrawPolygon(pen, points);
        }

        /// <summary>
        /// Pastes pixels 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void PastePixels(IntPtr pixels, int x, int y, int width, int height)
        {
            GL.DrawPixels(width, height, PixelFormat.Bgra, PixelType.UnsignedInt8888, pixels);
        }
 
        /// <summary>
        /// Copies pixels
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public IntPtr CopyPixels(int x, int y, int width, int height)
        {
            IntPtr ret=IntPtr.Zero;
            GL.ReadPixels(x, y, width, height, PixelFormat.Bgra, PixelType.UnsignedInt8888, ret);
            return ret;
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            implementation.DrawRectangle(pen, x, y, width, height);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="rect"></param>
        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            implementation.DrawRectangle(pen, rect);
        }

        /// <summary>
        /// Draws a reflection of an image
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="image"></param>
        /// <param name="percent"></param>
        /// <param name="angle"></param>
        public void DrawReflection(int X, int Y, int Width, int Height, OImage image, float percent, float angle)
        {
            implementation.DrawReflection(X, Y, Width, Height, image, percent, angle);
        }

        /// <summary>
        /// Draws a rounded rectangle
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        public void DrawRoundRectangle(Pen pen, int x, int y, int width, int height, int radius)
        {
            implementation.DrawRoundRectangle(pen, x, y, width, height, radius);
        }
        /// <summary>
        /// Draws a rounded rectangle
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            implementation.DrawRoundRectangle(p,rect,radius);
        }

        public void DrawPoint(Pen p, int x, int y, int width, int height)
        {
            implementation.DrawPoint(p.Color, x, y, width, height);
        }
        public void DrawPoint(Brush brush, int x, int y, int width, int height)
        {
            implementation.DrawPoint(brush.Color, x, y, width, height);
        }

        /// <summary>
        /// Draws a filled ellipse
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            implementation.FillEllipse(brush, x, y, width, height);
        }
        /// <summary>
        /// Draws a filled ellipse
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        public void FillEllipse(Brush brush, Rectangle rect)
        {
            implementation.FillEllipse(brush, rect);
        }

        /// <summary>
        /// Draws a filled polygon
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="points"></param>
        public void FillPolygon(Brush brush, Point[] points)
        {
            implementation.FillPolygon(brush, points);
        }
 
        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            implementation.FillRectangle(brush, x, y, width, height);
        }
        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            implementation.FillRectangle(brush, rect);
        }
        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="gradient"></param>
        /// <param name="rect"></param>
        public void FillRectangle(GradientData gradient, Rectangle rect, float opacity)
        {
            implementation.FillRectangle(gradient, rect, opacity);
        }

        /// <summary>
        /// Draws a filled rounded rectangle
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            implementation.FillRoundRectangle(brush, x, y, width, height, radius);
        }
        /// <summary>
        /// Draws a filled rounded rectangle
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            implementation.FillRoundRectangle(brush, rect, radius);
        }

        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="s"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="Left"></param>
        /// <param name="Top"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public OImage GenerateStringTexture(string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format)
        {
            return GenerateStringTexture(null, font, color, Left, Top, Width, Height, format);
        }
        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="s"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="Left"></param>
        /// <param name="Top"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="format"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return GenerateTextTexture(null, x, y, w, h, text, font, format, alignment, color, secondColor);
        }
        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        public OImage GenerateTextTexture(OImage image, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return GenerateTextTexture(image, x, y, w, h, text, font, format, alignment, color, secondColor, FitModes.None);
        }
        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <param name="fitMode"></param>
        /// <returns></returns>
        public OImage GenerateTextTexture(OImage image, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor, FitModes fitMode)
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
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor, fitMode);
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
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor, fitMode);
                }

                image.image = bmp;
                return image;
            }
        }

        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="screen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <returns></returns>
        static public OImage GenerateTextTexture(OImage image, int screen, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            return GenerateTextTexture(image, screen, x, y, w, h, text, font, format, alignment, color, secondColor, FitModes.None);
        }
        /// <summary>
        /// Renders a string to a image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="screen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <param name="fitMode"></param>
        /// <returns></returns>
        static public OImage GenerateTextTexture(OImage image, int screen, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor, FitModes fitMode)
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
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor, fitMode);
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
                    renderText(g, 0, 0, w, h, text, font, format, alignment, color, secondColor, fitMode);
                }

                image.image = bmp;
                return image;
            }
        }


        /// <summary>
        /// Screen number this graphics object belongs to
        /// </summary>
        public int screen
        {
            get { return _screen; }
        }
        private int _screen;

        /// <summary>
        /// Initializes the graphics object
        /// </summary>
        /// <param name="screen"></param>
        public Graphics(int screen)
        {
            // Initialize scale factors
            _ScaleFactors = new PointF[OpenTK.DisplayDevice.AvailableDisplays.Count];
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

        /// <summary>
        /// Initializes the graphics engine (Selects the proper rendering methods to use)
        /// </summary>
        public void Initialize(OpenTK.GameWindow targetWindow, MouseData mouseData)
        {
            #if LINUX
            if (OpenTK.Configuration.RunningOnAndroid)
            {
                version = OpenTK.Graphics.ES11.GL.GetString(OpenTK.Graphics.ES11.StringName.Version);
                v2 = true;
                implementation = new ESGraphics(screen);
                renderer = OpenTK.Graphics.ES11.GL.GetString(OpenTK.Graphics.ES11.StringName.Renderer);
            }
            else
            #endif
            {
                version = GL.GetString(StringName.Version);
                if (version.Length < 3)
                    v2 = false;
                else if (version[0] >= '2') //2.0 or higher
                    v2 = true;
                else if (version.Substring(0, 3) == "1.5") //1.5 with npots support
                {
                    string[] extensions = GL.GetString(StringName.Extensions).Split(new char[] { ' ' });
                    v2 = (Array.Exists(extensions, t => t == "GL_ARB_texture_non_power_of_two"));
                }
                if (v2)
                    implementation = new V2Graphics(screen, targetWindow, mouseData);
                else
                    implementation = new V1Graphics(screen);
                renderer = GL.GetString(StringName.Renderer);
            }
            if (OpenTK.Configuration.RunningOnWindows)
            {
                // DPI readout is not working due to new security model in .Net4
                //IntPtr dc = GetDC(IntPtr.Zero);
                //dpi = GetDeviceCaps(dc, 88) / 96F;
                dpi = 1;
            }
            implementation.Initialize(screen);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        /// <summary>
        /// Calculates the space a string occupies
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ft"></param>
        /// <returns></returns>
        public static SizeF MeasureString(String str, Font ft)
        {
            lock (virtualG)
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(virtualG);
                System.Drawing.SizeF ret=gr.MeasureString(str, new System.Drawing.Font(ft.Name, ft.Size, (System.Drawing.FontStyle)ft.Style));
                return new SizeF(ret.Width,ret.Height);
            }
        }

        /// <summary>
        /// Calculates the space a string occupies
        /// </summary>
        /// <param name="text"></param>
        /// <param name="f"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the space a string occupies
        /// </summary>
        /// <param name="text"></param>
        /// <param name="f"></param>
        /// <param name="format"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the space a string occupies
        /// </summary>
        /// <param name="text"></param>
        /// <param name="f"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="width"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the space a string occupies
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="rect"></param>
        /// <param name="format"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a OpenMobile font to System.Drawing.Font
        /// </summary>
        /// <param name="font"></param>
        /// <param name="format"></param>
        /// <returns></returns>
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
        static public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC, FitModes fitMode)
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

            // Autofit text
            if (fitMode == FitModes.Fit || fitMode == FitModes.Fill || fitMode == FitModes.FitFill)
            {
                RectangleF layout_rect_text = rectTxt;
                layout_rect_text.Height = 0;    // Hack to make addstring dynamic in the height direction

                // Ensure string wrapping is active by removing the nowrap flag
                sFormat.FormatFlags &= ~StringFormatFlags.NoWrap;

                // Add basic string first
                gpTxt.AddString(text, currentFont.FontFamily, (int)f, currentFont.Size, layout_rect_text, sFormat);

                // Get bounding area
                RectangleF bound = gpTxt.GetBounds();

                if (fitMode == FitModes.Fit || fitMode == FitModes.FitFill)
                {
                    AutoFitText_Fit_Internal(gpTxt, rectTxt, text, currentFont, currentFont.Size, f, sFormat);
                }
                else if (fitMode == FitModes.Fill || fitMode == FitModes.FitFill)
                {
                    // Check if string is too small
                    if (bound.Width <= rectTxt.Width && bound.Height <= rectTxt.Height)
                    {   // To small, fill
                        float fontStartSize = (currentFont.Size);
                        for (float fittedFontSize = fontStartSize; fittedFontSize < 100; fittedFontSize++)
                        {
                            gpTxt.Reset();
                            gpTxt.AddString(text, currentFont.FontFamily, (int)f, fittedFontSize, layout_rect_text, sFormat);
                            bound = gpTxt.GetBounds();

                            // Fitting completed?
                            if ((bound.Width >= (rectTxt.Width * 0.8f)) || (bound.Height >= (rectTxt.Height * 0.8f)))
                            {
                                // Text is now slighty too big, scale back down to get a proper result
                                AutoFitText_Fit_Internal(gpTxt, rectTxt, text, currentFont, fittedFontSize, f, sFormat);
                                break;
                            }
                        }
                    }
                }

                // Translate to correct placement
                Matrix m = new Matrix();
                
                // Default move text to top so we have a fixed starting point
                m.Translate(0 + x, -bound.Top + y);

                if (((alignment & Alignment.CenterLeft) == Alignment.CenterLeft) ||
                    ((alignment & Alignment.CenterCenter) == Alignment.CenterCenter) ||
                    ((alignment & Alignment.CenterRight) == Alignment.CenterRight))
                {
                    m.Translate(0, (rectTxt.Height / 2) - (bound.Height / 2));
                }

                else if (((alignment & Alignment.BottomLeft) == Alignment.BottomLeft) ||
                    ((alignment & Alignment.BottomCenter) == Alignment.BottomCenter) ||
                    ((alignment & Alignment.BottomRight) == Alignment.BottomRight))
                {
                    m.Translate(0, (rectTxt.Height) - (bound.Height));
                }
                gpTxt.Transform(m);
                m.Reset();
            }
            else if ((fitMode == FitModes.FitSingleLine) ||
                (fitMode == FitModes.FillSingleLine) ||
                (fitMode == FitModes.FitFillSingleLine))
            {
                #region SingleLine

                // Ensure string wrapping is not active
                sFormat.FormatFlags |= StringFormatFlags.NoWrap;

                // Add basic string first
                gpTxt.AddString(text, currentFont.FontFamily, (int)f, currentFont.Size, new System.Drawing.Point(rectTxt.Left, rectTxt.Top), sFormat);

                // calculate best ratio for stretching
                RectangleF bound = gpTxt.GetBounds();
                float ratio = System.Math.Min((rectTxt.Width / bound.Width) * 0.95f,
                    (rectTxt.Height / bound.Height) * 0.9f);

                Matrix m = new Matrix();
                if ((fitMode == FitModes.FitSingleLine && ratio < 1) ||
                    (fitMode == FitModes.FillSingleLine && ratio > 1) ||
                    (fitMode == FitModes.FitFillSingleLine))
                {
                    // Scale path to correct size
                    m.Scale(ratio, ratio);
                }

                // Default move text to upper left so we have a fixed starting point
                m.Translate(-bound.Left, -bound.Top);

                // Apply transformation
                gpTxt.Transform(m);
                m.Reset();

                #region Alignment

                // Get new bounds after transformation
                bound = gpTxt.GetBounds();

                // Vertical alignment
                if (((alignment & Alignment.CenterLeft) == Alignment.CenterLeft) ||
                    ((alignment & Alignment.CenterCenter) == Alignment.CenterCenter) ||
                    ((alignment & Alignment.CenterRight) == Alignment.CenterRight))
                {
                    m.Translate(0, (rectTxt.Height / 2) - (bound.Height / 2));
                }
                else if (((alignment & Alignment.BottomLeft) == Alignment.BottomLeft) ||
                    ((alignment & Alignment.BottomCenter) == Alignment.BottomCenter) ||
                    ((alignment & Alignment.BottomRight) == Alignment.BottomRight))
                {
                    m.Translate(0, (rectTxt.Height) - (bound.Height));
                }

                // Horizontal alignment
                if (((alignment & Alignment.TopCenter) == Alignment.TopCenter) ||
                    ((alignment & Alignment.CenterCenter) == Alignment.CenterCenter) ||
                    ((alignment & Alignment.BottomCenter) == Alignment.BottomCenter))
                {
                    m.Translate((rectTxt.Width - bound.Width) / 2F, 0);
                }
                else if (((alignment & Alignment.TopRight) == Alignment.TopRight) ||
                    ((alignment & Alignment.CenterRight) == Alignment.CenterRight) ||
                    ((alignment & Alignment.BottomRight) == Alignment.BottomRight))
                {
                    m.Translate(rectTxt.Width - bound.Width, 0);
                }

                gpTxt.Transform(m);
                m.Reset();

                #endregion

                #endregion
            }
            else
            {
                // Add string
                gpTxt.AddString(text, currentFont.FontFamily, (int)f, currentFont.Size, rectTxt, sFormat);
            }
                                   
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

        /// <summary>
        /// INTERNAL: Autofit's the text output
        /// </summary>
        /// <param name="gpTxt"></param>
        /// <param name="rectTxt"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="initialFontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="sFormat"></param>
        private static void AutoFitText_Fit_Internal(GraphicsPath gpTxt, System.Drawing.Rectangle rectTxt, string text, System.Drawing.Font font, float initialFontSize, System.Drawing.FontStyle fontStyle, StringFormat sFormat)
        {
            RectangleF layout_rect_text = rectTxt;
            layout_rect_text.Height = 0;    // Hack to make addstring dynamic in the height direction

            // Get bounding area
            RectangleF bound = gpTxt.GetBounds();

            // Check if string is too big
            if (bound.Width >= rectTxt.Width || bound.Height >= rectTxt.Height)
            {   // To big, fit                
                // Precalculate font size to save calculation loops
                float fontScale = 0.5f + (rectTxt.Height / bound.Height);
                if (fontScale > 1f)
                    fontScale = 1f;
                float fontStartSize = (initialFontSize * fontScale);
                for (float fittedFontSize = fontStartSize; fittedFontSize > 5; fittedFontSize--)
                {
                    gpTxt.Reset();
                    gpTxt.AddString(text, font.FontFamily, (int)fontStyle, fittedFontSize, layout_rect_text, sFormat);
                    bound = gpTxt.GetBounds();

                    // Fitting completed?
                    if (bound.Width <= rectTxt.Width && bound.Height <= rectTxt.Height)
                        break;
                }
            }

        }

        /// <summary>
        /// Resets the clip region
        /// </summary>
        public void ResetClip()
        {
            implementation.ResetClip();
        }

        /// <summary>
        /// Resets the transformation matrix
        /// </summary>
        public void ResetTransform()
        {
            implementation.ResetTransform();
        }

        /// <summary>
        /// Resizes the screen
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public void Resize(int Width, int Height)
        {
            implementation.Resize(Width, Height);
        }

        /// <summary>
        /// Sets the clip region
        /// </summary>
        /// <param name="Rect"></param>
        public void SetClip(Rectangle Rect)
        {
            implementation.SetClip(Rect);
        }

        /// <summary>
        /// Sets the clip region
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetClipFast(int x, int y, int width, int height)
        {
            implementation.SetClipFast(x, y, width, height);
        }

        /// <summary>
        /// Translates the graphics
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Translate(double dx, double dy)
        {
            implementation.Translate(dx, dy);
        }

        /// <summary>
        /// Translates the graphics
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        public void Translate(double dx, double dy, double dz)
        {
            implementation.Translate(dx, dy, dz);
        }

        /// <summary>
        /// Loads a texture into OpenGL 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public uint LoadTexture(ref OImage image)
        {
            if (image.TextureGenerationRequired(this.screen))
            {
                lock (image)
                {
                    implementation.LoadTexture(ref image);
                }
            }
            return image.GetTexture(this.screen);
        }

        /// <summary>
        /// Converts a screen releative point to a absolute point
        /// </summary>
        /// <param name="screenRelativePoint"></param>
        /// <returns></returns>
        public Point GetScreenAbsPoint(Point screenRelativePoint)
        {
            screenRelativePoint.Translate(-500, -300);
            return screenRelativePoint;
        }

        /// <summary>
        /// Rotates the graphics
        /// </summary>
        /// <param name="rotation"></param>
        public void Rotate(Vector3 rotation)
        {
            Rotate(rotation.X, rotation.Y, rotation.Z);
        }

        /// <summary>
        /// Rotates the graphics
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="az"></param>
        public void Rotate(double ax, double ay, double az)
        {
            // Rotate in the order of X, Y and then Z
            if (ax != 0)
                implementation.Rotate(ax, Axis.X);
            if (ay != 0)
                implementation.Rotate(ay, Axis.Y);
            if (az != 0)
                implementation.Rotate(az, Axis.Z);
        }

        /// <summary>
        /// Scales the graphics
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(Vector3 scale)
        {
            Scale(scale.X, scale.Y, scale.Z);
        }

        /// <summary>
        /// Scales the graphics
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="sz"></param>
        public void Scale(double sx, double sy, double sz)
        {
            implementation.Scale(sx, sy, sz);
        }

        /// <summary>
        /// Sets the model matrix
        /// </summary>
        /// <param name="cameraLocation">Moves the camera location while looking at 0,0,0</param>
        /// <param name="cameraRotation">Rotates the camera while looking at 0,0,0</param>
        /// <param name="cameraOffset">Offsets the camera</param>
        /// <param name="zoom">Zoom level</param>
        /// <param name="activateMatrix">Activates the matrix immediately. NB! This can only be done from the rendering thread</param>
        public void _3D_ModelView_Set(Vector3 cameraLocation, Vector3 cameraRotation, Vector3 cameraOffset, double zoom, bool activateMatrix)
        {
            implementation._3D_ModelView_Set(cameraLocation.ToOpenTKVector3d(), cameraRotation.ToOpenTKVector3d(), cameraOffset.ToOpenTKVector3d(), zoom, activateMatrix);
        }

        /// <summary>
        /// Pushes the current modelview matrix onto the stack
        /// </summary>
        public void _3D_ModelView_Push()
        {
            implementation._3D_ModelView_Push();
        }

        /// <summary>
        /// Pops the current modelview matrix back from the stack
        /// </summary>
        public void _3D_ModelView_Pop()
        {
            implementation._3D_ModelView_Pop();
        }

        /// <summary>
        /// Sets the model matrix according to the camera data
        /// </summary>
        /// <param name="cameraData"></param>
        public void _3D_ModelView_Set(_3D_Control cameraData)
        {
            // Get current matrix
            OpenTK.Matrix4d modelView = implementation._3D_ModelViewMatrix;

            bool changed = false;

            // Rotate camera
            if (!cameraData.CameraRotation.Equals(Vector3.Zero))
            {
                modelView *= OpenTK.Matrix4d.CreateRotationX(OpenMobile.Math.MathHelper.DegreesToRadians(cameraData.CameraRotation.X));
                modelView *= OpenTK.Matrix4d.CreateRotationY(OpenMobile.Math.MathHelper.DegreesToRadians(cameraData.CameraRotation.Y));
                modelView *= OpenTK.Matrix4d.CreateRotationZ(OpenMobile.Math.MathHelper.DegreesToRadians(cameraData.CameraRotation.Z));
                changed = true;
            }

            // Offset camera
            if (!cameraData.CameraOffset.Equals(Vector3.Zero))
            {
                modelView *= OpenTK.Matrix4d.CreateTranslation(cameraData.CameraOffset.ToOpenTKVector3d());
                changed = true;
            }

            // Zoom 
            if (cameraData.CameraZoom != 0)
            {
                modelView *= OpenTK.Matrix4d.Scale(cameraData.CameraZoom);
                changed = true;
            }

            // Activate changes
            if (changed)
                implementation._3D_ModelViewMatrix = modelView;
        }

        /// <summary>
        /// Sets or gets the modelview matrix
        /// </summary>
        public OpenTK.Matrix4d _3D_ModelViewMatrix
        {
            get
            {
                return implementation._3D_ModelViewMatrix;
            }
            set
            {
                implementation._3D_ModelViewMatrix = value;
            }
        }

        /// <summary>
        /// Resets and places the camera at the given location
        /// </summary>
        /// <param name="cameraLocation"></param>
        public void _3D_ModelView_ResetAndPlaceCamera(Vector3 cameraLocation)
        {
            implementation._3D_ModelView_ResetAndPlaceCamera(cameraLocation.ToOpenTKVector3d());
        }

    }

    /// <summary>
    /// Class that can automatically wrap a given string.
    /// </summary>
    public class StringWrap
    {
        /// <summary>
        /// List of characters to break a string around.  A <c>char</c> key points to an
        /// <c>int</c> weight.  Higher weights indicate a higher preference to break at
        /// that character.
        /// </summary>
        public readonly System.Collections.Hashtable Breakable = new System.Collections.Hashtable();

        /// <summary>
        /// Class that can automatically wrap a given string.
        /// </summary>
        public StringWrap()
        {
            Breakable[' '] = 10;
            Breakable[','] = 20;
            Breakable[';'] = 30;
            Breakable['.'] = 40;
        }

        /// <summary>
        /// Cleanse the given text from duplicate (two or more in a row) characters, as
        /// specified by <c>Breakable</c>.  Will also remove all existing newlines.
        /// </summary>
        /// <param name="text">Text to be cleansed.</param>
        /// <returns>Cleansed text.</returns>
        protected string Cleanse(string text)
        {
            text = text.Replace('\n', ' ');
            string find, replace;
            foreach (char c in Breakable.Keys)
            {
                find = new string(c, 2);
                replace = new string(c, 1);
                while (text.IndexOf(find) != -1)
                    text = text.Replace(find, replace);
            }
            return text;
        }

        /// <summary>
        /// Perform an automatic wrap given text, a target ratio, and a font ratio.
        /// </summary>
        /// <param name="text">Text to automatically wrap.</param>
        /// <param name="targetRatio">Ratio (Height / Width) of the target area
        /// for this text.</param>
        /// <param name="fontRatio">Average ratio (Height / Width) of a single
        /// character.</param>
        /// <returns>Automatically wrapped text.</returns>
        public string PerformWrap(string text, float targetRatio, float fontRatio)
        {
            string wrap = "";
            text = Cleanse(text);

            int rows = (int)System.Math.Sqrt(targetRatio * text.Length / fontRatio),
                cols = text.Length / rows,
                start = cols, index = 0, last;

            for (int i = 0; i < rows - 1; i++)
            {
                last = index;
                index = BestBreak(text, start, cols * 2);
                wrap += text.Substring(last, index - last).Trim() + "\n";
                start = index + cols;
            }

            wrap += text.Substring(index);
            return wrap;
        }

        /// <summary>
        /// Find the best place to break text given a starting index and a search radius.
        /// </summary>
        /// <param name="text">Full text to search through.</param>
        /// <param name="start">Index in string to start searching at.  Will be used
        /// as the center of the radius.</param>
        /// <param name="radius">Radius (in characters) to search around the given
        /// starting index.</param>
        /// <returns>Optimal index to break the text at.</returns>
        protected int BestBreak(string text, int start, int radius)
        {
            int bestIndex = start;
            float bestWeight = 0, examWeight;

            radius = System.Math.Min(System.Math.Min(start + radius, text.Length - 1) - start,
                start - System.Math.Max(start - radius, 0));
            for (int i = start - radius; i <= start + radius; i++)
            {
                object o = Breakable[text[i]];
                if (o == null) continue;

                examWeight = (int)o / (float)System.Math.Abs(start - i);
                if (examWeight > bestWeight)
                {
                    bestIndex = i;
                    bestWeight = examWeight;
                }
            }

            return System.Math.Min(bestIndex + 1, text.Length - 1);
        }

    }

    /// <summary>
    /// 3D: Control data
    /// </summary>
    public struct _3D_Control
    {
        /// <summary>
        /// Control rotation 
        /// </summary>
        public Vector3 ControlRotation
        {
            get
            {
                return this._ControlRotation;
            }
            set
            {
                this._ControlRotation = value;
            }
        }
        private Vector3 _ControlRotation;

        /// <summary>
        /// Control rotation center
        /// </summary>
        public Vector3 ControlRotationPoint
        {
            get
            {
                return this._ControlRotationPoint;
            }
            set
            {
                this._ControlRotationPoint = value;
            }
        }
        private Vector3 _ControlRotationPoint;

        /// <summary>
        /// Camera rotation 
        /// </summary>
        public Vector3 CameraRotation
        {
            get
            {
                return this._CameraRotation;
            }
            set
            {
                this._CameraRotation = value;
            }
        }
        private Vector3 _CameraRotation;

        /// <summary>
        /// Camera offset
        /// </summary>
        public Vector3 CameraOffset
        {
            get
            {
                return this._CameraOffset;
            }
            set
            {
                this._CameraOffset = value;
            }
        }
        private Vector3 _CameraOffset;

        /// <summary>
        /// Camera Zoom level
        /// </summary>
        public double CameraZoom
        {
            get
            {
                return this._CameraZoom;
            }
            set
            {
                if (this._CameraZoom != value)
                {
                    this._CameraZoom = value;
                }
            }
        }
        private double _CameraZoom;

        /// <summary>
        /// Initializes a new set of cameracontrol data
        /// </summary>
        /// <param name="controlRotation"></param>
        public _3D_Control(Vector3 controlRotation)
        {
            _ControlRotation = controlRotation;
            _ControlRotationPoint = Vector3.Zero;
            _CameraRotation = Vector3.Zero;
            _CameraOffset = Vector3.Zero;
            _CameraZoom = 0;
        }
        /// <summary>
        /// Initializes a new set of cameracontrol data
        /// </summary>
        /// <param name="controlRotation"></param>
        /// <param name="controlRotationPoint"></param>
        public _3D_Control(Vector3 controlRotation, Vector3 controlRotationPoint)
        {
            _ControlRotation = controlRotation;
            _ControlRotationPoint = controlRotationPoint;
            _CameraRotation = Vector3.Zero;
            _CameraOffset = Vector3.Zero;
            _CameraZoom = 0;
        }
        /// <summary>
        /// Initializes a new set of cameracontrol data
        /// </summary>
        /// <param name="controlRotation"></param>
        /// <param name="controlRotationPoint"></param>
        /// <param name="cameraRotation"></param>
        public _3D_Control(Vector3 controlRotation, Vector3 controlRotationPoint, Vector3 cameraRotation)
        {
            _ControlRotation = controlRotation;
            _ControlRotationPoint = controlRotationPoint;
            _CameraRotation = cameraRotation;
            _CameraOffset = Vector3.Zero;
            _CameraZoom = 0;
        }
        /// <summary>
        /// Initializes a new set of cameracontrol data
        /// </summary>
        /// <param name="controlRotation"></param>
        /// <param name="controlRotationPoint"></param>
        /// <param name="cameraLocation"></param>
        /// <param name="cameraOffset"></param>
        public _3D_Control(Vector3 controlRotation, Vector3 controlRotationPoint, Vector3 cameraRotation, Vector3 cameraOffset)
        {
            _ControlRotation = controlRotation;
            _ControlRotationPoint = controlRotationPoint;
            _CameraRotation = cameraRotation;
            _CameraOffset = cameraOffset;
            _CameraZoom = 0;
        }
        /// <summary>
        /// Initializes a new set of cameracontrol data
        /// </summary>
        /// <param name="controlRotation"></param>
        /// <param name="controlRotationPoint"></param>
        /// <param name="cameraLocation"></param>
        /// <param name="cameraOffset"></param>
        /// <param name="cameraZoom"></param>
        public _3D_Control(Vector3 controlRotation, Vector3 controlRotationPoint, Vector3 cameraRotation, Vector3 cameraOffset, int cameraZoom)
        {
            _ControlRotation = controlRotation;
            _ControlRotationPoint = controlRotationPoint;
            _CameraRotation = cameraRotation;
            _CameraOffset = cameraOffset;
            _CameraZoom = cameraZoom;
        }

    }
}
