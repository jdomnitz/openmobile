#pragma warning disable 0659,0661
using System;
using System.Reflection;
using System.Collections.Generic;
using OpenMobile.Math;
using System.ComponentModel;

namespace OpenMobile.Graphics
{
    using Math = System.Math;
    public enum Gradient:byte
    {
        None = 0,
        Vertical = 1,
        Horizontal = 2,
        Individual = 3
    }
    [Flags]
    public enum FontStyle:byte
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8,
    }
    [Serializable]
    public struct Pen : IDisposable
    {
        Color color;
        float width;
        System.Drawing.Drawing2D.DashStyle style;
        public Color Color
        {
            get
            {
                return color;
            }
        }
        public float Width
        {
            get
            {
                return width;
            }
        }
        public Pen(Color color)
        {
            this.color = color;
            this.width = 1F;
            this.style = System.Drawing.Drawing2D.DashStyle.Solid;
        }
        public Pen(Color color, float width)
        {
            this.color = color;
            this.width = width;
            this.style = System.Drawing.Drawing2D.DashStyle.Solid;
        }
        public Brush Brush
        {
            get
            {
                return new Brush(color);
            }
        }
        public Pen(Brush brush, float width)
        {
            this.color = brush.Color;
            this.width = width;
            this.style = System.Drawing.Drawing2D.DashStyle.Solid;
        }
        public System.Drawing.Drawing2D.DashStyle DashStyle
        {
            get { return style; }
            set { style = value; }
        }
        #region IDisposable Members
        [Obsolete]
        public void Dispose()
        {
            //Legacy
        }

        #endregion
    }

    /// <summary>
    /// Gradient data
    /// </summary>
    public class GradientData : List<GradientData.ColorQuad>
    {
        /// <summary>
        /// Creates a gradient type that gives a border around a specified center color. 
        /// The color of each corner can be freely set, use Color.Empty to specifiy a transparent area
        /// </summary>
        /// <param name="borderSizeX"></param>
        /// <param name="borderSizeY"></param>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <param name="c4"></param>
        /// <param name="c5"></param>
        /// <returns></returns>
        static public GradientData CreateColorBorder(float borderSizeX, float borderSizeY, Color c1, Color c2, Color c3, Color c4, Color c5)
        {
            PointF[] p = new PointF[8];
            p[0] = new PointF(-1, -1);
            p[1] = new PointF(1, -1);
            p[2] = new PointF(1, 1);
            p[3] = new PointF(-1, 1);
            p[4] = new PointF(p[0].X - -borderSizeX, p[0].Y - -borderSizeY);
            p[5] = new PointF(p[1].X - borderSizeX, p[1].Y - -borderSizeY);
            p[6] = new PointF(p[2].X - borderSizeX, p[2].Y - borderSizeY);
            p[7] = new PointF(p[3].X - -borderSizeX, p[3].Y - borderSizeY);
            
            GradientData gradient = new GradientData();

            // Corner
            gradient.Add(new ColorQuad(
                new ColorPoint(p[0].X, p[4].Y, c1),
                new ColorPoint(p[0].X, p[0].Y, c1),
                new ColorPoint(p[4].X, p[0].Y, c1),
                new ColorPoint(p[4].X, p[4].Y, c5)));

            gradient.Add(new ColorQuad(
                new ColorPoint(p[4].X, p[0].Y, c1),
                new ColorPoint(p[5].X, p[1].Y, c2),
                new ColorPoint(p[5].X, p[5].Y, c5),
                new ColorPoint(p[4].X, p[4].Y, c5)));

            // Corner
            gradient.Add(new ColorQuad(
                new ColorPoint(p[5].X, p[1].Y, c2),
                new ColorPoint(p[1].X, p[1].Y, c2),
                new ColorPoint(p[1].X, p[5].Y, c2),
                new ColorPoint(p[5].X, p[5].Y, c5)));

            gradient.Add(new ColorQuad(
                new ColorPoint(p[5].X, p[5].Y, c5),
                new ColorPoint(p[1].X, p[5].Y, c2),
                new ColorPoint(p[1].X, p[6].Y, c3),
                new ColorPoint(p[6].X, p[6].Y, c5)));

             // Corner
            gradient.Add(new ColorQuad(
                new ColorPoint(p[2].X, p[6].Y, c3),
                new ColorPoint(p[2].X, p[2].Y, c3),
                new ColorPoint(p[6].X, p[2].Y, c3),
                new ColorPoint(p[6].X, p[6].Y, c5)));

            gradient.Add(new ColorQuad(
                new ColorPoint(p[7].X, p[3].Y, c4),
                new ColorPoint(p[7].X, p[7].Y, c5),
                new ColorPoint(p[6].X, p[6].Y, c5),
                new ColorPoint(p[6].X, p[2].Y, c3)));

            // Corner
            gradient.Add(new ColorQuad(
                new ColorPoint(p[7].X, p[3].Y, c4),
                new ColorPoint(p[3].X, p[3].Y, c4),
                new ColorPoint(p[3].X, p[7].Y, c4),
                new ColorPoint(p[7].X, p[7].Y, c5)));

            gradient.Add(new ColorQuad(
                new ColorPoint(p[3].X, p[7].Y, c4),
                new ColorPoint(p[0].X, p[4].Y, c1),
                new ColorPoint(p[4].X, p[4].Y, c5),
                new ColorPoint(p[7].X, p[7].Y, c5)));

            gradient.Add(new ColorQuad(
                new ColorPoint(p[7].X, p[7].Y, c5),
                new ColorPoint(p[4].X, p[4].Y, c5),
                new ColorPoint(p[5].X, p[5].Y, c5),
                new ColorPoint(p[6].X, p[6].Y, c5)));

            return gradient;
        }

        /// <summary>
        /// Creates a horizontal gradient, each color is equally spaced in the horizontal direction
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        static public GradientData CreateHorizontalGradient(params Color[] colors)
        {
            GradientData gradient = new GradientData();
            double quadsize = 2.0;
            if (colors.Length > 2)
                quadsize /= (colors.Length-1);
            double lastpos = -1;
            if (colors.Length > 1)
            {
                for (int i = 0; i < colors.Length - 1; i++)
                {
                    // Create the quads
                    gradient.Add(new ColorQuad(
                        new ColorPoint(lastpos, -1, colors[i]),
                        new ColorPoint(lastpos + quadsize, -1, colors[i + 1]),
                        new ColorPoint(lastpos + quadsize, 1, colors[i + 1]),
                        new ColorPoint(lastpos, 1, colors[i])));
                    lastpos += quadsize;
                }
            }
            else if (colors.Length == 1)
            {   // Add just one big color
                gradient.Add(new ColorQuad(
                    new ColorPoint(-1, -1, colors[0]),
                    new ColorPoint(1, -1, colors[0]),
                    new ColorPoint(1, 1, colors[0]),
                    new ColorPoint(-1, 1, colors[0])));
            }
            return gradient;
        }

        /// <summary>
        /// Creates a horizontal gradient, the different positions for the colors can be freely set in the range -1 to 1 
        /// where -1 is all the way to the left and 1 is all the way to the right
        /// <para>NB! Colorpoints positions are relative to each other</para>
        /// </summary>
        /// <param name="colorPoints"></param>
        /// <returns></returns>
        static public GradientData CreateHorizontalGradient(params ColorPoint[] colorPoints)
        {
            GradientData gradient = new GradientData();
            double lastpos = -1;
            for (int i = 0; i < colorPoints.Length - 1; i++)
            {
                // Create the quads
                double newX = lastpos + colorPoints[i+1].Point.X;
                // Limit endpos
                if (newX > 1) newX = 1;

                gradient.Add(new ColorQuad(
                    new ColorPoint(lastpos, -1, colorPoints[i].Color),
                    new ColorPoint(newX, -1, colorPoints[i + 1].Color),
                    new ColorPoint(newX, 1, colorPoints[i + 1].Color),
                    new ColorPoint(lastpos, 1, colorPoints[i].Color)));
                lastpos += colorPoints[i+1].Point.X;

                // Stop if we go outside the region
                if (lastpos >= 1 || newX >= 1) 
                    break;
            }

            // Fill last part of gradient with last color
            if (lastpos < 1)
            {
                gradient.Add(new ColorQuad(
                    new ColorPoint(lastpos, -1, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(1, -1, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(1, 1, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(lastpos, 1, colorPoints[colorPoints.Length - 1].Color)));
            }
            
            return gradient;
        }

        /// <summary>
        /// Creates a vertical gradient, each color is equally spaced in the horizontal direction
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        static public GradientData CreateVerticalGradient(params Color[] colors)
        {
            GradientData gradient = new GradientData();
            double quadsize = 2.0;
            if (colors.Length > 2)
                quadsize /= (colors.Length - 1);
            double lastpos = -1;
            if (colors.Length > 1)
            {
                for (int i = 0; i < colors.Length - 1; i++)
                {
                    // Create the quads
                    gradient.Add(new ColorQuad(
                        new ColorPoint(-1, lastpos, colors[i]),
                        new ColorPoint(1, lastpos, colors[i]),
                        new ColorPoint(1, lastpos + quadsize, colors[i + 1]),
                        new ColorPoint(-1, lastpos + quadsize, colors[i + 1])));
                    lastpos += quadsize;
                }
            }
            else if (colors.Length == 1)
            {   // Add just one big color
                gradient.Add(new ColorQuad(
                    new ColorPoint(-1, -1, colors[0]),
                    new ColorPoint(1, -1, colors[0]),
                    new ColorPoint(1, 1, colors[0]),
                    new ColorPoint(-1, 1, colors[0])));
            }

            return gradient;
        }

        /// <summary>
        /// Creates a vertical gradient, the different positions for the colors can be freely set in the range -1 to 1 
        /// where -1 is all the way to the top and 1 is all the way to the bottom
        /// <para>NB! Colorpoints positions are relative to each other</para>
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        static public GradientData CreateVerticalGradient(params ColorPoint[] colorPoints)
        {
            GradientData gradient = new GradientData();
            double lastpos = -1;
            for (int i = 0; i < colorPoints.Length - 1; i++)
            {
                // Create the quads
                double newY = lastpos + colorPoints[i + 1].Point.Y;
                // Limit endpos
                if (newY > 1) newY = 1;

                gradient.Add(new ColorQuad(
                    new ColorPoint(-1, lastpos, colorPoints[i].Color),
                    new ColorPoint(1, lastpos, colorPoints[i].Color),
                    new ColorPoint(1, newY, colorPoints[i + 1].Color),
                    new ColorPoint(-1, newY, colorPoints[i + 1].Color)));
                lastpos += colorPoints[i + 1].Point.Y;

                // Stop if we go outside the region
                if (lastpos >= 1 || newY >= 1)
                    break;
            }

            // Fill last part of gradient with last color
            if (lastpos < 1)
            {
                gradient.Add(new ColorQuad(
                    new ColorPoint(-1, lastpos, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(1, lastpos, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(1, 1, colorPoints[colorPoints.Length - 1].Color),
                    new ColorPoint(-1, 1, colorPoints[colorPoints.Length - 1].Color)));
            }

            return gradient;
        }

        /// <summary>
        /// A colorpoint data
        /// </summary>
        public struct ColorPoint
        {
            public Color Color;
            public Vector3d Point;

            public ColorPoint(Vector3d point, Color color)
            {
                //if (point.X > 1 || point.X < -1)
                //    throw new Exception("Vectordata for point X is outside valid range (-1 to 1)");
                //if (point.Y > 1 || point.Y < -1)
                //    throw new Exception("Vectordata for point Y is outside valid range (-1 to 1)");
                //if (point.Z > 1 || point.Z < -1)
                //    throw new Exception("Vectordata for point Z is outside valid range (-1 to 1)");

                Point = point;
                Color = color;
            }
            public ColorPoint(double x, double y, Color color)
            {
                Point = new Vector3d(x, y, 0);

                //if (Point.X > 1 || Point.X < -1)
                //    throw new Exception("Vectordata for point X is outside valid range (-1 to 1)");
                //if (Point.Y > 1 || Point.Y < -1)
                //    throw new Exception("Vectordata for point Y is outside valid range (-1 to 1)");
                //if (Point.Z > 1 || Point.Z < -1)
                //    throw new Exception("Vectordata for point Z is outside valid range (-1 to 1)");

                Color = color;
            }

            public override string ToString()
            {
                return String.Format("[{0}, {1}]", Point, Color);
            }
        }

        /// <summary>
        /// A collection of four color points that's used for rendering the gradients
        /// </summary>
        public struct ColorQuad
        {
            public ColorPoint ColorPointV1;
            public ColorPoint ColorPointV2;
            public ColorPoint ColorPointV3;
            public ColorPoint ColorPointV4;

            public ColorQuad(ColorPoint v1, ColorPoint v2, ColorPoint v3, ColorPoint v4)
            {
                this.ColorPointV1 = v1;
                this.ColorPointV2 = v2;
                this.ColorPointV3 = v3;
                this.ColorPointV4 = v4;
            }

            public override string ToString()
            {
                return String.Format("[{0}, {1}, {2}, {3}]", ColorPointV1, ColorPointV2, ColorPointV3, ColorPointV4);
            }
        }
    }

    /// <summary>
    /// A brush to use when rendering
    /// </summary>
    [Serializable]
    public struct Brush : IDisposable
    {
        Gradient _Gradient;
        Color _Color1;
        Color _Color2;
        Color _Color3;
        Color _Color4;
        Color _Color5;

        /// <summary>
        /// Creates a single color solid brush
        /// </summary>
        /// <param name="color"></param>
        public Brush(Color color)
        {
            _Color1 = color;
            _Color2 = Color.Empty;
            _Color3 = Color.Empty;
            _Color4 = Color.Empty;
            _Color5 = Color.Empty;
            _Gradient = Gradient.None;
        }

        /// <summary>
        /// Creates a two color gradient brush
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="gradient">Direction/type of gradient</param>
        public Brush(Color first, Color second,Gradient gradient)
        {
            _Color1 = first;
            _Color2 = second;
            _Color3 = Color.Empty;
            _Color4 = Color.Empty;
            _Color5 = Color.Empty;
            _Gradient = gradient;
        }

        /// <summary>
        /// Creates a four color brush where each color represents a corner of a rectangle
        /// </summary>
        /// <param name="first">Upper left corner</param>
        /// <param name="second">Upper right corner</param>
        /// <param name="third">Lower right corner</param>
        /// <param name="forth">Lower left corner</param>
        public Brush(Color first, Color second, Color third, Color forth)
        {
            _Color1 = first;
            _Color2 = second;
            _Color3 = third;
            _Color4 = forth;
            _Color5 = Color.Empty;
            _Gradient = OpenMobile.Graphics.Gradient.Individual;
        }

        /// <summary>
        /// Creates a five color brush where color 1 to 4 represents a corner of a rectangle and the fifth is used as center color
        /// </summary>
        /// <param name="first">Upper left corner</param>
        /// <param name="second">Upper right corner</param>
        /// <param name="third">Lower right corner</param>
        /// <param name="forth">Lower left corner</param>
        /// <param name="fifth">fill color</param>
        public Brush(Color first, Color second, Color third, Color forth, Color fifth)
        {
            _Color1 = first;
            _Color2 = second;
            _Color3 = third;
            _Color4 = forth;
            _Color5 = fifth;
            _Gradient = OpenMobile.Graphics.Gradient.Individual;
        }

        /// <summary>
        /// The main color of this brush
        /// </summary>
        public Color Color
        {
            get
            {
                return _Color1;
            }
        }

        /// <summary>
        /// The gradient color of this brush
        /// </summary>
        public Color SecondColor
        {
            get
            {
                return _Color2;
            }
        }

        /// <summary>
        /// Color data for the upper left corner
        /// </summary>
        public Color ColorData_C1
        {
            get
            {
                switch (_Gradient)
                {
                    case Gradient.None:
                        return _Color1;
                    case Gradient.Vertical:
                        return _Color1;
                    case Gradient.Horizontal:
                        return _Color1;
                    case Gradient.Individual:
                        return _Color1;
                }
                return Color.Empty;
            }
        }
        /// <summary>
        /// Color data for the upper right corner
        /// </summary>
        public Color ColorData_C2
        {
            get
            {
                switch (_Gradient)
                {
                    case Gradient.None:
                        return _Color1;
                    case Gradient.Vertical:
                        return _Color1;
                    case Gradient.Horizontal:
                        return _Color2;
                    case Gradient.Individual:
                        return _Color2;
                }
                return Color.Empty;
            }
        }
        /// <summary>
        /// Color data for the lower right corner
        /// </summary>
        public Color ColorData_C3
        {
            get
            {
                switch (_Gradient)
                {
                    case Gradient.None:
                        return _Color1;
                    case Gradient.Vertical:
                        return _Color2;
                    case Gradient.Horizontal:
                        return _Color2;
                    case Gradient.Individual:
                        return _Color3;
                }
                return Color.Empty;
            }
        }
        /// <summary>
        /// Color data for the lower left corner
        /// </summary>
        public Color ColorData_C4
        {
            get
            {
                switch (_Gradient)
                {
                    case Gradient.None:
                        return _Color1;
                    case Gradient.Vertical:
                        return _Color2;
                    case Gradient.Horizontal:
                        return _Color1;
                    case Gradient.Individual:
                        return _Color4;
                }
                return Color.Empty;
            }
        }

        /// <summary>
        /// Fifth color data 
        /// </summary>
        public Color ColorData_C5
        {
            get
            {
                switch (_Gradient)
                {
                    case Gradient.Individual:
                        return _Color5;
                }
                return Color.Empty;
            }
        }

        /// <summary>
        /// The direction/type of gradient
        /// </summary>
        public Gradient Gradient
        {
            get
            {
                return _Gradient;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Legacy
        }

        #endregion
    }

  

    [Serializable]
    public struct Color
    {
        public static readonly Color Empty;
        private byte r;
        private byte g;
        private byte b;
        private byte a;
        private string name;

        public static Color FromName(string name)
        {
            foreach (PropertyInfo p in typeof(Color).GetProperties())
            {
                if (p.PropertyType!=typeof(Color))
                    continue;
                Color c = (Color)p.GetValue(null, null);
                if (c.Name == name)
                    return c;
            }
            return Empty;
        }
        public System.Drawing.Color ToSystemColor()
        {
            return System.Drawing.Color.FromArgb(this.A, this.R, this.G, this.B);
        }
        public void FromNativeColor(System.Drawing.Color c)
        {
            this = Color.FromArgb(c.A, c.R, c.G, c.B);
        }
        public static System.Drawing.Color ToNativeColor(Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
        public static Color FromSystemColor(System.Drawing.Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// Updates the Alpha value of the color
        /// </summary>
        /// <param name="a"></param>
        public Color SetAlpha(int a)
        {
            this.a = (byte)a;
            return this;
        }
        public static bool operator ==(Color left, Color right)
        {
            return ((left.A == right.A) && (left.b == right.B) && (left.G == right.G) && (left.R == right.R));
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Color))
                return false;
            Color right = (Color)obj;
            return ((this.A == right.A) && (this.b == right.B) && (this.G == right.G) && (this.R == right.R));
        }
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }
        public Color(int a, int r, int g, int b)
        {
            this.a = (byte)a;
            this.r = (byte)r;
            this.g = (byte)g;
            this.b = (byte)b;
            this.name = "";
        }
        internal Color(int a, int r, int g, int b,string name)
        {
            this.a = (byte)a;
            this.r = (byte)r;
            this.g = (byte)g;
            this.b = (byte)b;
            this.name = name;
        }
        public byte R
        {
            get
            {
                return r;
            }
        }
        public byte G
        {
            get
            {
                return g;
            }
        }
        public byte B
        {
            get
            {
                return b;
            }
        }
        public byte A
        {
            get
            {
                return a;
            }
        }
        public bool IsKnownColor
        {
            get
            {
                return !string.IsNullOrEmpty(name);
            }
        }
        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets the R part as a value in the range 0 - 1
        /// </summary>
        public double Rd
        {
            get
            {
                return r / 255.0;
            }
        }
        /// <summary>
        /// Gets the G part as a value in the range 0 - 1
        /// </summary>
        public double Gd
        {
            get
            {
                return g / 255.0;
            }
        }
        /// <summary>
        /// Gets the B part as a value in the range 0 - 1
        /// </summary>
        public double Bd
        {
            get
            {
                return b / 255.0;
            }
        }
        /// <summary>
        /// Gets the A part as a value in the range 0 - 1
        /// </summary>
        public double Ad
        {
            get
            {
                return a / 255.0;
            }
        }

        #region Predefined colors

        public static Color AliceBlue
        {
            get
            {
                return new Color(0xFF, 240, 248, 255, "AliceBlue");
            }
        }
        public static Color AntiqueWhite
        {
            get
            {
                return new Color(0xFF, 250, 235, 215, "AntiqueWhite");
            }
        }
        public static Color Aquamarine
        {
            get
            {
                return new Color(0xFF, 127, 255, 212, "Aquamarine");
            }
        }
        public static Color Aqua
        {
            get
            {
                return new Color(0xFF, 0, 255, 255, "Aqua");
            }
        }
        public static Color Azure
        {
            get
            {
                return new Color(0xFF, 240, 255, 255, "Azure");
            }
        }
        public static Color Beige
        {
            get
            {
                return new Color(0xFF, 245, 245, 220, "Beige");
            }
        }
        public static Color Bisque
        {
            get
            {
                return new Color(0xFF, 255,228,196, "Bisque");
            }
        }
        private static Color black;
        public static Color Black
        {
            get
            {
                return black;
            }
        }
        public static Color Blue
        {
            get
            {
                return new Color(0xFF, 0, 0, 0xFF, "Blue");
            }
        }
        public static Color BlanchedAlmond
        {
            get
            {
                return new Color(0xFF, 255, 255, 205, "BlanchedAlmond");
            }
        }
        public static Color BlueViolet
        {
            get
            {
                return new Color(0xFF, 138, 43, 226, "BlueViolet");
            }
        }
        public static Color Brown
        {
            get
            {
                return new Color(0xFF, 165, 42, 42, "Brown");
            }
        }
        public static Color BurlyWood
        {
            get
            {
                return new Color(0xFF, 222, 184, 135, "BurlyWood");
            }
        }
        public static Color CadetBlue
        {
            get
            {
                return new Color(0xFF, 95, 158, 160, "CadetBlue");
            }
        }
        public static Color Chartreuse
        {
            get
            {
                return new Color(0xFF, 127, 255, 0, "Chartreuse");
            }
        }
        public static Color Chocolate
        {
            get
            {
                return new Color(0xFF, 0xFF, 0, 0, "Chocolate");
            }
        }
        public static Color Coral
        {
            get
            {
                return new Color(0xFF, 255, 127, 80, "Coral");
            }
        }
        public static Color CornflowerBlue
        {
            get
            {
                return new Color(0xFF, 255, 248, 220, "CornflowerBlue");
            }
        }
        public static Color Cornsilk
        {
            get
            {
                return new Color(0xFF, 255, 248, 220, "Cornsilk");
            }
        }
        public static Color Crimson
        {
            get
            {
                return new Color(0xFF, 220, 20, 60, "Crimson");
            }
        }
        public static Color Cyan
        {
            get
            {
                return new Color(0xff, 0, 255, 255, "Cyan");
            }
        }
        public static Color DarkBlue
        {
            get
            {
                return new Color(0xFF, 0, 0, 139, "DarkBlue");
            }
        }
        public static Color DarkCyan
        {
            get
            {
                return new Color(0xFF, 0, 139, 139, "DarkCyan");
            }
        }
        public static Color DarkGray
        {
            get
            {
                return new Color(0xFF, 169, 169, 169, "DarkGray");
            }
        }
        public static Color DarkGreen
        {
            get
            {
                return new Color(0xFF, 0, 100, 0, "DarkGreen");
            }
        }
        public static Color DarkKhaki
        {
            get
            {
                return new Color(0xFF, 189, 183, 107, "DarkKhaki");
            }
        }
        public static Color DarkMagenta
        {
            get
            {
                return new Color(0xFF,139,0,139,"DarkMagenta");
            }
        }
        public static Color DarkOliveGreen
        {
            get
            {
                return new Color(0xFF, 85, 107, 47, "DarkOliveGreen");
            }
        }
        public static Color DarkOrange
        {
            get
            {
                return new Color(0xFF, 255, 140, 0, "DarkOrange");
            }
        }
        public static Color DarkOrchid
        {
            get
            {
                return new Color(0xFF, 153, 50, 204, "DarkOrchid");
            }
        }
        public static Color DarkRed
        {
            get
            {
                return new Color(0xFF, 139, 0, 0, "DarkRed");
            }
        }
        public static Color DarkSalmon
        {
            get
            {
                return new Color(0xFF, 233, 150, 122, "DarkSalmon");
            }
        }
        public static Color DarkSeaGreen
        {
            get
            {
                return new Color(0xFF, 143, 188, 143, "DarkSeaGreen");
            }
        }
        public static Color DarkSlateBlue
        {
            get
            {
                return new Color(0xFF, 72, 61, 139, "DarkSlateBlue");
            }
        }
        public static Color DarkGoldenrod
        {
            get
            {
                return new Color(0xFF, 184, 134, 11, "DarkGoldenrod");
            }
        }
        public static Color DarkSlateGray
        {
            get
            {
                return new Color(0xFF, 40, 79, 79, "DarkSlateGray");
            }
        }
        public static Color DarkTurquoise
        {
            get
            {
                return new Color(0xFF, 0, 206, 209, "DarkTurquoise");
            }
        }
        public static Color DarkViolet
        {
            get
            {
                return new Color(0xFF, 148, 0, 211, "DarkViolet");
            }
        }
        public static Color DeepPink
        {
            get
            {
                return new Color(0xFF, 255, 20, 147, "DeepPink");
            }
        }
        public static Color DeepSkyBlue
        {
            get
            {
                return new Color(0xFF, 0, 191, 255, "DeepSkyBlue");
            }
        }
        public static Color DimGray
        {
            get
            {
                return new Color(0xFF, 105, 105, 105, "DimGray");
            }
        }
        public static Color DodgerBlue
        {
            get
            {
                return new Color(0xFF, 30, 144, 255, "DodgerBlue");
            }
        }
        public static Color Firebrick
        {
            get
            {
                return new Color(0xFF, 178, 34, 34, "Firebrick");
            }
        }
        public static Color FloralWhite
        {
            get
            {
                return new Color(0xFF, 255, 250, 240, "FloralWhite");
            }
        }
        public static Color ForestGreen
        {
            get
            {
                return new Color(0xFF, 34, 139, 34, "ForestGreen");
            }
        }
        public static Color Fuschia
        {
            get
            {
                return new Color(0xFF, 255, 0, 255, "Fuschia");
            }
        }
        public static Color Gainsboro
        {
            get
            {
                return new Color(0xFF, 220, 220, 220, "Gainsboro");
            }
        }
        public static Color GhostWhite
        {
            get
            {
                return new Color(0xFF, 248, 248, 255, "GhostWhite");
            }
        }
        public static Color Gold
        {
            get
            {
                return new Color(0xFF, 255, 215, 0, "Gold");
            }
        }
        public static Color Goldenrod
        {
            get
            {
                return new Color(0xFF, 218, 165, 32, "Goldenrod");
            }
        }
        public static Color Gray
        {
            get
            {
                return new Color(0xFF, 128, 128, 128, "Gray");
            }
        }
        public static Color Green
        {
            get
            {
                return new Color(0xFF, 0, 0xFF, 0, "Green");
            }
        }
        public static Color OpenMobileGreen
        {
            get
            {
                return new Color(0xFF, 58, 176, 58, "OpenMobileGreen");
            }
        }
        public static Color GreenYellow
        {
            get
            {
                return new Color(0xFF, 173, 255, 47, "GreenYellow");
            }
        }
        public static Color Honeydew
        {
            get
            {
                return new Color(0xFF, 240, 255, 240, "Honeydew");
            }
        }
        public static Color HotPink
        {
            get
            {
                return new Color(0xFF, 255, 105, 180, "HotPink");
            }
        }
        public static Color IndianRed
        {
            get
            {
                return new Color(0xFF, 205, 92, 92, "IndianRed");
            }
        }
        public static Color Indigo
        {
            get
            {
                return new Color(0xFF, 75, 0, 130, "Indigo");
            }
        }
        public static Color Ivory
        {
            get
            {
                return new Color(0xFF, 255, 240, 240, "Ivory");
            }
        }
        public static Color Khaki
        {
            get
            {
                return new Color(0xFF, 240, 230, 140, "Khaki");
            }
        }
        public static Color Lavender
        {
            get
            {
                return new Color(0xFF, 230, 230, 250, "Lavender");
            }
        }
        public static Color LavenderBlush
        {
            get
            {
                return new Color(0xFF, 255, 240, 245, "LavenderBlush");
            }
        }
        public static Color LawnGreen
        {
            get
            {
                return new Color(0xFF, 124, 252, 0, "LawnGreen");
            }
        }
        public static Color LemonChiffon
        {
            get
            {
                return new Color(0xFF, 255, 250, 205, "LemonChiffon");
            }
        }
        public static Color LightBlue
        {
            get
            {
                return new Color(0xFF, 173, 216, 230, "LightBlue");
            }
        }
        public static Color LightCoral
        {
            get
            {
                return new Color(0xFF, 240, 128, 128, "LightCoral");
            }
        }
        public static Color LightCyan
        {
            get
            {
                return new Color(0xFF, 224, 255, 255, "LightCyan");
            }
        }
        public static Color LightGray
        {
            get
            {
                return new Color(0xFF, 211, 211, 211, "LightGray");
            }
        }
        public static Color LightGreen
        {
            get
            {
                return new Color(0xFF, 144, 238, 144, "LightGreen");
            }
        }
        public static Color LightGoldenrodYellow
        {
            get
            {
                return new Color(0xFF, 250, 250, 210, "LightGoldenrodYellow");
            }
        }
        public static Color LightPink
        {
            get
            {
                return new Color(0xFF, 255, 182, 193, "LightPink");
            }
        }
        public static Color LightSalmon
        {
            get
            {
                return new Color(0xFF, 255, 160, 122, "LightSalmon");
            }
        }
        public static Color LightSeaGreen
        {
            get
            {
                return new Color(0xFF, 32, 178, 170, "LightSeaGreen");
            }
        }
        public static Color LightSkyBlue
        {
            get
            {
                return new Color(0xFF, 135, 206, 250, "LightSkyBlue");
            }
        }
        public static Color LightSlateGray
        {
            get
            {
                return new Color(0xFF, 119, 136, 153, "LightSlateGray");
            }
        }
        public static Color LightSteelBlue
        {
            get
            {
                return new Color(0xFF, 176, 196, 222, "LightSteelBlue");
            }
        }
        public static Color LightYellow
        {
            get
            {
                return new Color(0xFF, 255, 255, 224, "LightYellow");
            }
        }
        public static Color Lime
        {
            get
            {
                return new Color(0xFF, 0, 255, 0, "Lime");
            }
        }
        public static Color LimeGreen
        {
            get
            {
                return new Color(0xFF, 50, 205, 50, "LimeGreen");
            }
        }
        public static Color Linen
        {
            get
            {
                return new Color(0xFF, 250, 240, 230, "Linen");
            }
        }
        public static Color Magenta
        {
            get
            {
                return new Color(0xFF, 255, 0, 255, "Magenta");
            }
        }
        public static Color Maroon
        {
            get
            {
                return new Color(0xFF, 128, 0, 0, "Maroon");
            }
        }
        public static Color MediumAquamarine
        {
            get
            {
                return new Color(0xFF, 102, 205, 170, "MediumAquamarine");
            }
        }
        public static Color MediumBlue
        {
            get
            {
                return new Color(0xFF, 0, 0, 205, "MediumBlue");
            }
        }
        public static Color MediumOrchid
        {
            get
            {
                return new Color(0xFF, 186, 85, 211, "MediumOrchid");
            }
        }
        public static Color MediumPurple
        {
            get
            {
                return new Color(0xFF, 147, 112, 219, "MediumPurple");
            }
        }
        public static Color MediumSeaGreen
        {
            get
            {
                return new Color(0xFF, 60, 179, 113, "MediumSeaGreen");
            }
        }
        public static Color MediumSlateBlue
        {
            get
            {
                return new Color(0xFF, 123, 104, 238, "MediumSlateBlue");
            }
        }
        public static Color MediumSpringGreen
        {
            get
            {
                return new Color(0xFF, 0, 250, 154, "MediumSpringGreen");
            }
        }
        public static Color MediumTurquoise
        {
            get
            {
                return new Color(0xFF, 72, 209, 204, "MediumTurquoise");
            }
        }
        public static Color MediumVioletRed
        {
            get
            {
                return new Color(0xFF, 199, 21, 112, "MediumVioletRed");
            }
        }
        public static Color MidnightBlue
        {
            get
            {
                return new Color(0xFF, 25, 25, 112, "MidnightBlue");
            }
        }
        public static Color MintCream
        {
            get
            {
                return new Color(0xFF, 245, 255, 250, "MintCream");
            }
        }
        public static Color MistyRose
        {
            get
            {
                return new Color(0xFF, 255, 228, 225, "MistyRose");
            }
        }
        public static Color Moccassin
        {
            get
            {
                return new Color(0xFF, 255, 228, 181, "Moccasin");
            }
        }
        public static Color NavajoRed
        {
            get
            {
                return new Color(0xFF, 255, 222, 173, "NavajoRed");
            }
        }
        public static Color Navy
        {
            get
            {
                return new Color(0xFF, 0, 0, 128, "Navy");
            }
        }
        public static Color OldLace
        {
            get
            {
                return new Color(0xFF, 253, 245, 230, "OldLace");
            }
        }
        public static Color Olive
        {
            get
            {
                return new Color(0xFF, 128, 128, 0, "Olive");
            }
        }

        public static Color OliveDrab
        {
            get
            {
                return new Color(0xFF, 107, 142, 45, "OliveDrab");
            }
        }
        public static Color Orange
        {
            get
            {
                return new Color(0xFF, 255, 165, 0, "Orange");
            }
        }
        public static Color OrangeRed
        {
            get
            {
                return new Color(0xFF, 255, 69, 0, "OrangeRed");
            }
        }
        public static Color Orchid
        {
            get
            {
                return new Color(0xFF, 218, 112, 214, "Orchid");
            }
        }
        public static Color PaleGoldenrod
        {
            get
            {
                return new Color(0xFF, 238, 232, 170, "PaleGoldenrod");
            }
        }
        public static Color PaleGreen
        {
            get
            {
                return new Color(0xFF, 152,251,152, "PaleGreen");
            }
        }
        public static Color PaleTurquoise
        {
            get
            {
                return new Color(0xFF, 175, 238, 238, "PaleTurquoise");
            }
        }
        public static Color PaleVioletRed
        {
            get
            {
                return new Color(0xFF, 219, 112, 147, "PaleVioletRed");
            }
        }
        public static Color PapayaWhip
        {
            get
            {
                return new Color(0xFF, 255, 239, 213, "PapayaWhip");
            }
        }
        public static Color PeachPuff
        {
            get
            {
                return new Color(0xFF, 218, 112, 214, "PeachPuff");
            }
        }
        public static Color Peru
        {
            get
            {
                return new Color(0xFF, 205, 133, 63, "Peru");
            }
        }
        public static Color Pink
        {
            get
            {
                return new Color(0xFF, 255, 192, 203, "Pink");
            }
        }
        public static Color Plum
        {
            get
            {
                return new Color(0xFF, 221, 160, 221, "Plum");
            }
        }
        public static Color PowderBlue
        {
            get
            {
                return new Color(0xFF, 176, 224, 230, "PowderBlue");
            }
        }
        public static Color Purple
        {
            get
            {
                return new Color(0xFF, 128, 0, 128, "Purple");
            }
        }
        public static Color Red
        {
            get
            {
                return new Color(0xFF, 0xFF, 0, 0, "Red");
            }
        }

        public static Color RosyBrown
        {
            get
            {
                return new Color(0xFF, 188, 143, 143, "RosyBrown");
            }
        }
        public static Color RoyalBlue
        {
            get
            {
                return new Color(0xFF, 65, 105, 225, "RoyalBlue");
            }
        }
        public static Color SaddleBrown
        {
            get
            {
                return new Color(0xFF, 139, 69, 19, "SaddleBrown");
            }
        }
        public static Color Salmon
        {
            get
            {
                return new Color(0xFF, 250, 128, 114, "Salmon");
            }
        }
        public static Color SandyBrown
        {
            get
            {
                return new Color(0xFF, 244, 164, 96, "SandyBrown");
            }
        }
        public static Color SeaGreen
        {
            get
            {
                return new Color(0xFF, 46, 139, 87, "SeaGreen");
            }
        }
        public static Color Seashell
        {
            get
            {
                return new Color(0xFF, 255, 245, 238, "Seashell");
            }
        }
        public static Color Sienna
        {
            get
            {
                return new Color(0xFF, 160, 82, 45, "Sienna");
            }
        }
        public static Color Silver
        {
            get
            {
                return new Color(0xFF, 192, 192, 192, "Silver");
            }
        }
        public static Color SkyBlue
        {
            get
            {
                return new Color(0xFF, 135, 206, 235, "SkyBlue");
            }
        }
        public static Color SlateBlue
        {
            get
            {
                return new Color(0xFF, 106, 90, 205, "SlateBlue");
            }
        }
        public static Color SlateGray
        {
            get
            {
                return new Color(0xFF, 112, 128, 144, "SlateGray");
            }
        }
        public static Color Snow
        {
            get
            {
                return new Color(0xFF, 255, 250, 250, "Snow");
            }
        }

        public static Color SpringGreen
        {
            get
            {
                return new Color(0xFF, 0, 255, 127, "SpringGreen");
            }
        }
        public static Color SteelBlue
        {
            get
            {
                return new Color(0xFF, 70, 130, 180, "SteelBlue");
            }
        }
        public static Color Tan
        {
            get
            {
                return new Color(0xFF, 210, 180, 140, "Tan");
            }
        }
        public static Color Teal
        {
            get
            {
                return new Color(0xFF, 0, 128, 128, "Teal");
            }
        }
        public static Color Thistle
        {
            get
            {
                return new Color(0xFF, 216, 191, 216, "Thistle");
            }
        }
        public static Color Tomato
        {
            get
            {
                return new Color(0xFF, 253, 99, 71, "Tomato");
            }
        }
        private static Color transparent;
        public static Color Transparent
        {
            get
            {
                return transparent;
            }
        }
        public static Color Turquoise
        {
            get
            {
                return new Color(0xFF, 64, 224, 208, "Turquoise");
            }
        }
        public static Color Violet
        {
            get
            {
                return new Color(0xFF, 238, 130, 238, "Violet");
            }
        }
        public static Color Wheat
        {
            get
            {
                return new Color(0xFF, 245, 222, 179, "Wheat");
            }
        }
        private static Color white;
        public static Color White
        {
            get
            {
                return white;
            }
        }
        public static Color WhiteSmoke
        {
            get
            {
                return new Color(0xFF, 245, 245, 245, "WhiteSmoke");
            }
        }
        public static Color Yellow
        {
            get
            {
                return new Color(0xFF, 0xFF, 0xFF, 0, "Yellow");
            }
        }
        public static Color YellowGreen
        {
            get
            {
                return new Color(0xFF, 154, 205, 50, "YellowGreen");
            }
        }

        #endregion

        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            return new Color(alpha, red, green, blue, "Custom");
        }

        public static Color FromArgb(int alpha, Color baseColor)
        {
            return new Color(alpha, baseColor.R, baseColor.G, baseColor.B,baseColor.Name);
        }

        public static Color FromArgb(int red, int green, int blue)
        {
            return new Color(0xff, red, green, blue, "Custom");
        }
        public override string ToString()
        {
            return name+"("+a.ToString()+","+r.ToString()+","+g.ToString()+","+b.ToString()+")";
        }
        static Color()
        {
            Empty = new Color();
            black = new Color(0xFF, 0, 0, 0, "Black");
            white = new Color(0xFF, 255, 255, 255, "White");
            transparent = new Color(0, 0xFF, 0xFF, 0xFF, "Transparent");
        }
    }
    [Serializable]
    public struct PointF
    {
        private float x;
        private float y;
#pragma warning disable 0414
        static readonly PointF Empty;
#pragma warning restore 0414
        public static bool operator ==(PointF left, PointF right)
        {
            return ((left.X == right.X) && (left.Y == right.Y));
        }

        public static bool operator !=(PointF left, PointF right)
        {
            return !(left == right);
        }

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public PointF(int x, int y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }
        public bool IsEmpty
        {
            get
            {
                return ((this.x == 0) && (this.y == 0));
            }
        }
        public float X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }
        public float Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
                return false;
            PointF point = (PointF)obj;
            return ((point.x == this.x) && (point.y == this.y));
        }

        public override string ToString()
        {
            return ("{X=" + this.x.ToString() + ", Y=" + this.y.ToString() + "}");
        }
        public Point ToPoint()
        {
            return new Point(this.x, this.y);
        }
        static PointF()
        {
            Empty=new PointF();
        }
    }
#pragma warning disable 0660
    [Serializable]
    public struct Font
    {
        string name;
        float size;
        FontStyle style;
        static Font arial;
        static Font times;
        static Font()
        {
            System.Drawing.FontFamily[] supported = System.Drawing.FontFamily.Families;
            if (Configuration.RunningOnLinux)
            {
                if (Array.Exists<System.Drawing.FontFamily>(supported, p => p.Name == "Arial"))
                    arial = new Font("Arial");
                else if (Array.Exists<System.Drawing.FontFamily>(supported, p => p.Name == "Arial"))
                    arial = new Font("Liberation Sans");
                else
                    arial = new Font("FreeSans");
            }
            else
                arial = new Font("Arial");
            times=new Font("Times New Roman");
            if (Configuration.RunningOnLinux)
                if (!Array.Exists<System.Drawing.FontFamily>(supported, p => p.Name == "Times New Roman"))
                    times = new Font("FreeSerif");

        }

        public static bool operator ==(Font left, Font right)
        {
            if (left.name != right.name)
                return false;
            if (left.size != right.size)
                return false;
            if (left.Style != right.Style)
                return false;
            return true;
        }
        public static bool operator !=(Font left, Font right)
        {
            return !(left == right);
        }

        public static Font Arial 
        {
            get
            {
                return arial;
            }
        }
        public static Font ArialBlack 
        {
            get
            {
                return new Font("Arial Black");
            }
        }
        public static Font Verdana
        {
            get
            {
                return new Font("Verdana");
            }
        }
        public static Font TrebuchetMS
        {
            get
            {
                #if OSX
                if (Configuration.RunningOnMacOS)
                    return new Font("Helvetica");
                else
                #endif
                    return new Font("Trebuchet MS");
            }
        }
        public static Font GenericSansSerif
        {
            get
            {
                return Arial;
            }
        }
        public static Font GenericSerif
        {
            get
            {
                return TimesNewRoman;
            }
        }
        public static Font GenericMonospace
        {
            get
            {
                return CourierNew;
            }
        }
        public static Font GenericCursive
        {
            get
            {
                return ComicSansMS;
            }
        }
        public static Font Tahoma
        {
            get
            {
                if (Configuration.RunningOnLinux)
                    return new Font("Kalimati");
                else if (Configuration.RunningOnMacOS)
                    return new Font("Geneva");
                else
                    return new Font("Tahoma");
            }
        }
        public static Font TimesNewRoman
        {
            get
            {
                return times;
            }
        }

        public static Font LED
        {
            get
            {
                return new Font("DigiFaceWide");
            }
        }        

        public static Font Webdings
        {
            get
            {
                return new Font("Webdings");
            }
        }
        public static Font Wingdings
        {
            get
            {
                if (Configuration.RunningOnMacOS)
                    return new Font("Zapf Dingbats");
                else if (Configuration.RunningOnLinux)
                    return new Font("Dingbats");
                return new Font("Wingdings");
            }
        }
        public static Font Symbol
        {
            get
            {
                return new Font("Symbol");
            }
        }
        public static Font Impact
        {
            get
            {
                return new Font("Impact");
            }
        }
        public static Font LucidaSans
        {
            get
            {
                if (Configuration.RunningOnWindows)
                    return new Font("Lucida Sans Unicode");
                else if (Configuration.RunningOnLinux)
                    return new Font("Garuda");
                else
                    return new Font("Lucida Grande");
            }
        }
        public static Font Palatino
        {
            get
            {
                if (Configuration.RunningOnWindows)
                    return new Font("Palatino Linotype");
                else if (Configuration.RunningOnLinux)
                    return new Font("FreeSerif");
                else
                    return new Font("Palatino");
            }
        }
        public static Font Georgia 
        {
            get
            {
                return new Font("Georgia");
            }
        }
        public static Font ComicSansMS
        {
            get
            {
                return new Font("Comic Sans MS");
            }
        }
        public static Font CourierNew
        {
            get
            {
                return new Font("Courier New");
            }
        }
        public FontStyle Style
        {
            get
            {
                return style;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public float Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public Font(Font fontName)
        {
            this.name = fontName.name;
            this.size = 12;
            this.style = FontStyle.Regular;
        }
        public Font(Font fontName, float size, FontStyle style)
        {
            this.name = fontName.name;
            this.size = size;
            this.style = style;
        }
        public Font(Font fontName, float size)
        {
            this.name = fontName.name;
            this.size = size;
            this.style = FontStyle.Regular;
        }
        public Font(string Font)
        {
            this.name = Font;
            this.size = 12;
            this.style = FontStyle.Regular;
        }
        [Obsolete("Use Enum")]
        public Font(string Font, float size)
        {
            this.name = Font;
            this.size = size;
            this.style = FontStyle.Regular;
        }
        public Font(string Font, float size,FontStyle style)
        {
            this.name = Font;
            this.size = size;
            this.style = style;
        }
        public static System.Drawing.FontStyle FormatToStyle(eTextFormat format)
        {
            System.Drawing.FontStyle f = System.Drawing.FontStyle.Regular;
            if ((format == eTextFormat.Bold) || (format == eTextFormat.BoldShadow) || (format == eTextFormat.BoldGlow))
            {
                f = System.Drawing.FontStyle.Bold;
            }
            else if (format == eTextFormat.Italic
                || format == eTextFormat.ItalicShadow
                || format == eTextFormat.OutlineItalic
                || format == eTextFormat.OutlineItalicFat
                || format == eTextFormat.OutlineItalicNarrow
                || format == eTextFormat.OutlineItalicNoFill
                || format == eTextFormat.OutlineItalicNoFillFat
                || format == eTextFormat.OutlineItalicNoFillNarrow)
            {
                f = System.Drawing.FontStyle.Italic;
            }
            else if ((format == eTextFormat.Underline) || (format == eTextFormat.UnderlineShadow))
            {
                f = System.Drawing.FontStyle.Underline;
            }
            return f;
        }
        public static eTextFormat StyleToFormat(System.Drawing.FontStyle format)
        {
            eTextFormat f = eTextFormat.Normal;
            if (format == System.Drawing.FontStyle.Bold)
            {
                f = eTextFormat.Bold;
            }
            else if (format == System.Drawing.FontStyle.Italic)
            {
                f = eTextFormat.Italic;
            }
            else if (format == System.Drawing.FontStyle.Underline)
            {
                f = eTextFormat.Underline;
            }
            return f;
        }
        public System.Drawing.Font ToSystemFont()
        {
            return new System.Drawing.Font(this.Name, 1F);
        }
        public static System.Drawing.StringFormat AlignmentToStringFormat(Alignment alignment)
        {
            System.Drawing.StringFormat sFormat = new System.Drawing.StringFormat();

            // Convert alignment
            if (((alignment & Alignment.CenterLeft) == Alignment.CenterLeft) ||
                ((alignment & Alignment.BottomLeft) == Alignment.BottomLeft) ||
                ((alignment & Alignment.TopLeft) == Alignment.TopLeft))
            {
                sFormat.Alignment = System.Drawing.StringAlignment.Near;
            }
            else if (((alignment & Alignment.CenterCenter) == Alignment.CenterCenter) ||
                ((alignment & Alignment.BottomCenter) == Alignment.BottomCenter) ||
                ((alignment & Alignment.TopCenter) == Alignment.TopCenter))
            {
                sFormat.Alignment = System.Drawing.StringAlignment.Center;
            }
            else if (((alignment & Alignment.CenterRight) == Alignment.CenterRight) ||
                ((alignment & Alignment.BottomRight) == Alignment.BottomRight) ||
                ((alignment & Alignment.TopRight) == Alignment.TopRight))
            {
                sFormat.Alignment = System.Drawing.StringAlignment.Far;
            }

            // Convert line alignment
            if (((alignment & Alignment.TopCenter) == Alignment.TopCenter) ||
                ((alignment & Alignment.TopRight) == Alignment.TopRight) ||
                ((alignment & Alignment.TopLeft) == Alignment.TopLeft))
            {
                sFormat.LineAlignment = System.Drawing.StringAlignment.Near;
            }
            else if (((alignment & Alignment.CenterCenter) == Alignment.CenterCenter) ||
                ((alignment & Alignment.CenterRight) == Alignment.CenterRight) ||
                ((alignment & Alignment.CenterLeft) == Alignment.CenterLeft))
            {
                sFormat.LineAlignment = System.Drawing.StringAlignment.Center;
            }
            else if (((alignment & Alignment.BottomCenter) == Alignment.BottomCenter) ||
                ((alignment & Alignment.BottomRight) == Alignment.BottomRight) ||
                ((alignment & Alignment.BottomLeft) == Alignment.BottomLeft))
            {
                sFormat.LineAlignment = System.Drawing.StringAlignment.Far;
            }

            if ((alignment & Alignment.VerticalCentered) == Alignment.VerticalCentered)
                sFormat.FormatFlags = System.Drawing.StringFormatFlags.DirectionVertical;

            if ((alignment & Alignment.CenterLeftEllipsis) == Alignment.CenterLeftEllipsis)
                sFormat.Trimming = System.Drawing.StringTrimming.EllipsisWord;

            if (!((alignment & Alignment.WordWrap) == Alignment.WordWrap))
                sFormat.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;

            return sFormat;
        }
    }
    [Serializable]
    public struct Point
    {
        public static readonly Point Empty;
        private int x;
        private int y;

        public static bool operator ==(Point left, Point right)
        {
            return ((left.X == right.X) && (left.Y == right.Y));
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public static bool operator >(Point left, Point right)
        {
            return left.x > right.x && left.y > right.y;
        }
        public static bool operator <(Point left, Point right)
        {
            return left.x < right.x && left.y < right.y;
        }

        public static Point operator -(Point left, Point right)
        {
            return new Point(left.x - right.x, left.y - right.y);
        }

        public static Point operator +(Point left, Point right)
        {
            return new Point(left.x + right.x, left.y + right.y);
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(float x, float y)
        {
            this.x = (int)Math.Round(x);
            this.y = (int)Math.Round(y);
        }
        public bool IsEmpty
        {
            get
            {
                return ((this.x == 0) && (this.y == 0));
            }
        }
        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }
        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;
            Point point = (Point)obj;
            return ((point.x == this.x) && (point.y == this.y));
        }

        public override string ToString()
        {
            return ("{X=" + this.x.ToString() + ", Y=" + this.y.ToString() + "}");
        }

        public System.Drawing.Point ToSystemPoint()
        {
            return new System.Drawing.Point(x, y);
        }

        public OpenMobile.Math.Vector2 ToVector2()
        {
            return new OpenMobile.Math.Vector2(x, y);
        }
        static Point()
        {
            Empty = new Point();
        }

        public void Scale(float XScale, float YScale)
        {
            this.x = (int)(x / XScale);
            this.Y = (int)(Y / YScale);
        }

        public void Translate(int x, int y)
        {
            this.x += x;
            this.y += y;
        }
        public void Translate(Point p)
        {
            this.x += p.X;
            this.y += p.Y;
        }
    }

    [Serializable]
    public struct Size
    {
        public static readonly Size Empty;
        private int width;
        private int height;
        public Size(Point pt)
        {
            this.width = pt.X;
            this.height = pt.Y;
        }

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public Size(float width, float height)
        {
            this.width = (int)Math.Round(width);
            this.height = (int)Math.Round(height);
        }
        public static bool operator ==(Size s1, Size s2)
        {
            return ((s1.Width == s2.Width) && (s1.Height == s2.Height));
        }

        public static bool operator !=(Size s1, Size s2)
        {
            return !(s1 == s2);
        }

        public bool IsEmpty
        {
            get
            {
                return ((this.width == 0) && (this.height == 0));
            }
        }
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Size))
                return false;
            Size size = (Size)obj;
            return ((size.width == this.width) && (size.height == this.height));
        }

        public override string ToString()
        {
            return ("{Width=" + this.width.ToString() + ", Height=" + this.height.ToString() + "}");
        }

        static Size()
        {
            Empty = new Size();
        }
    }

    [Serializable]
    public struct SizeF
    {
        public static readonly SizeF Empty;
        private float width;
        private float height;
        public SizeF(Point pt)
        {
            this.width = pt.X;
            this.height = pt.Y;
        }

        public SizeF(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
        public static bool operator ==(SizeF s1, SizeF s2)
        {
            return ((s1.Width == s2.Width) && (s1.Height == s2.Height));
        }

        public static bool operator !=(SizeF s1, SizeF s2)
        {
            return !(s1 == s2);
        }

        public bool IsEmpty
        {
            get
            {
                return ((this.width == 0) && (this.height == 0));
            }
        }
        public float Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public float Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is SizeF))
                return false;
            SizeF size = (SizeF)obj;
            return ((size.width == this.width) && (size.height == this.height));
        }

        public override string ToString()
        {
            return ("{Width=" + this.width.ToString() + ", Height=" + this.height.ToString() + "}");
        }

        static SizeF()
        {
            Empty = new SizeF();
        }
    }

    [Serializable]
    public struct Rectangle //: System.ComponentModel.INotifyPropertyChanged
    {
        public static readonly Rectangle Empty;

        /// <summary>
        /// X dimension
        /// </summary>
        public int X
        {
            get
            {
                return _X;
            }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    //OnPropertyChanged("X");
                }
            }
        }
        private int _X;

        /// <summary>
        /// Y dimension
        /// </summary>
        public int Y
        {
            get
            {
                return _Y;
            }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    //OnPropertyChanged("Y");
                }
            }
        }
        private int _Y;

        /// <summary>
        /// Width
        /// </summary>
        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    //OnPropertyChanged("Width");
                }
            }
        }
        private int _Width;

        /// <summary>
        /// Height
        /// </summary>
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    //OnPropertyChanged("Height");
                }
            }
        }
        private int _Height;
     
        public bool Contains(Point p)
        {
            if ((p.X < X) || p.Y < Y)
                return false;
            if ((p.X > X + Width) || (p.Y > Y + Height))
                return false;
            return true;
        }
        public bool Contains(Rectangle r)
        {
            return ((r.X < (X + Width)) &&
              (X < (r.X + r.Width)) &&
              (r.Y < (Y + Height)) &&
              (Y < r.Y + r.Height));
        }
        public int Left
        {
            get { return X; }
            set { X = value; }
        }
        public int Top
        {
            get { return Y; }
            set { Y = value; }
        }
        public int Right
        {
            get { return X + Width; }
            set { Width = value - X; }
        }
        public int Bottom
        {
            get { return Height + Y; }
            set { Height = value - Y; }
        }
        public Rectangle(int width, int height)
        {
            this._X = 0;
            this._Y = 0;
            this._Width = width;
            this._Height = height;
            //this.PropertyChanged = null;
        }
        public Rectangle(int x, int y, int width, int height)
        {
            this._X = x;
            this._Y = y;
            this._Width = width;
            this._Height = height;
            //this.PropertyChanged = null;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            this._X = (int)Math.Round(x);
            this._Y = (int)Math.Round(y);
            this._Width = (int)Math.Round(width);
            this._Height = (int)Math.Round(height);
            //this.PropertyChanged = null;
        }

        public Rectangle(Point location, Size size)
        {
            this._X = location.X;
            this._Y = location.Y;
            this._Width = size.Width;
            this._Height = size.Height;
            //this.PropertyChanged = null;
        }

        public Rectangle(System.Drawing.Rectangle systemRectangle)
        {
            this._X = systemRectangle.X;
            this._Y = systemRectangle.Y;
            this._Width = systemRectangle.Width;
            this._Height = systemRectangle.Height;
            //this.PropertyChanged = null;
        }

        public Point Location
        {
            get
            {
                return new Point(this.X, this.Y);
            }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }

        public Point Center
        {
            get
            {
                return new Point(X + (Width / 2), Y + (Height / 2));
            }
        }

        public Size Size
        {
            get
            {
                return new Size(this.Width, this.Height);
            }
            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return ((((this.Height == 0) && (this.Width == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
            {
                return false;
            }
            Rectangle rectangle = (Rectangle)obj;
            return ((((rectangle.X == this.X) && (rectangle.Y == this.Y)) && (rectangle.Width == this.Width)) && (rectangle.Height == this.Height));
        }

        public static bool operator ==(Rectangle first, Rectangle second)
        {
            return ((((first.X == second.X) && (first.Y == second.Y)) && (first.Width == second.Width)) && (first.Height == second.Height));
        }

        public static bool operator !=(Rectangle first, Rectangle second)
        {
            return !(first == second);
        }

        public static Rectangle operator +(Rectangle first, Rectangle second)
        {
            return new Rectangle(first.X + second.X, first.Y + second.Y, first.Width + second.Width, first.Height + second.Height);
        }

        public static Rectangle operator -(Rectangle first, Rectangle second)
        {
            return new Rectangle(first.X - second.X, first.Y - second.Y, first.Width - second.Width, first.Height - second.Height);
        }

        public static Rectangle operator *(Rectangle first, Rectangle second)
        {
            return new Rectangle(first.X * second.X, first.Y * second.Y, first.Width * second.Width, first.Height * second.Height);
        }

        public static Rectangle operator /(Rectangle first, Rectangle second)
        {
            return new Rectangle(first.X / second.X, first.Y / second.Y, first.Width / second.Width, first.Height / second.Height);
        }

        public static Rectangle Round(System.Drawing.RectangleF value)
        {
            return new Rectangle((int)Math.Round((double)value.X), (int)Math.Round((double)value.Y), (int)Math.Round((double)value.Width), (int)Math.Round((double)value.Height));
        }

        public override string ToString()
        {
            return ("{X=" + this.X.ToString() + ",Y=" + this.Y.ToString() + ",Width=" + this.Width.ToString() + ",Height=" + this.Height.ToString() + "}");
        }

        public System.Drawing.Rectangle ToSystemRectangle()
        {
            return new System.Drawing.Rectangle(X, Y, Width, Height);
        }

        static Rectangle()
        {
            Empty = new Rectangle();
        }

        /// <summary>
        /// Translates the rectangle with the value passed in p
        /// </summary>
        /// <param name="p"></param>
        public Rectangle Translate(Point p)
        {
            this.X += p.X;
            this.Y += p.Y;
            return this;
        }
        /// <summary>
        /// Translates the rectangle with the value passed in X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public Rectangle Translate(int x, int y)
        {
            this.X += x;
            this.Y += y;
            return this;
        }
        /// <summary>
        /// Translates the rectangle with the X and Y values from the passed rectangle
        /// </summary>
        /// <param name="r"></param>
        public Rectangle Translate(Rectangle r)
        {
            this.X += r.X;
            this.Y += r.Y;
            return this;
        }
        /// <summary>
        /// Translates the rectangle with the X and Y values from the passed rectangle
        /// </summary>
        /// <param name="r"></param>
        public Rectangle TranslateToNew(Rectangle r)
        {
            Rectangle newR = this;
            newR.X += r.X;
            newR.Y += r.Y;
            return newR;
        }
        /// <summary>
        /// Translates the rectangle with the X and Y values from the passed rectangle
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public Rectangle TranslateToNew(int x, int y)
        {
            Rectangle newR = this;
            newR.X += x;
            newR.Y += y;
            return newR;
        }
        /// <summary>
        /// Scales the rectangle with the value passed in p
        /// </summary>
        /// <param name="p"></param>
        public Rectangle Scale(Point p)
        {
            this.Width *= p.X;
            this.Height *= p.Y;
            return this;
        }
        /// <summary>
        /// Scales the rectangle with the value passed in x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Rectangle Scale(float x, float y)
        {
            this.Width = (int)(((float)this.Width) * x);
            this.Height = (int)(((float)this.Height) * y);
            return this;
        }
        /// <summary>
        /// Scales the rectangle with the X and Y values from the passed rectangle
        /// </summary>
        /// <param name="r"></param>
        public Rectangle Scale(Rectangle r)
        {
            this.Width *= r.X;
            this.Height *= r.Y;
            return this;
        }

        public Rectangle Union(Rectangle r)
        {
            if (r.Left < this.Left)
            {
                int right = this.Right;
                this.Left = r.Left;
                this.Right = right;
            }
            if (r.Top < this.Top)
            {
                int bottom = this.Bottom;
                this.Top = r.Top;
                this.Bottom = bottom;
            }
            if (r.Right > this.Right)
            {
                this.Right = r.Right;
            }
            if (r.Bottom > this.Bottom)
            {
                this.Bottom = r.Bottom;
            }
            return this;
        }

        /// <summary>
        /// Inflates this rectangle
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public Rectangle Inflate(Size size)
        {
            return Inflate(size.Width, size.Height);
        }
        /// <summary>
        /// Inflates this rectangle
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Rectangle Inflate(int width, int height)
        {
            return Inflate(width, height, false);
        }

        /// <summary>
        /// Scales the rectangle around the center
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fixedCenter"></param>
        /// <returns></returns>
        public Rectangle Inflate(int width, int height, bool fixedCenter)
        {
            if (fixedCenter)
            {
                Point center = this.Center;
                this.Width += width;
                this.Height += height;
                Point Offset = center - this.Center;
                Translate(Offset);
                return this;
            }
            else
            {
                this.Width += width;
                this.Height += height;
                return this;
            }

        }


        //#region INotifyPropertyChanged Members

        ///// <summary>
        ///// Propertychanged event
        ///// </summary>
        //public event PropertyChangedEventHandler PropertyChanged;

        ///// <summary>
        ///// Propertychanged event
        ///// </summary>
        ///// <param name="e"></param>
        //private void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //        handler(this, e);
        //}

        ///// <summary>
        ///// Propertychanged event
        ///// </summary>
        ///// <param name="propertyName"></param>
        //private void OnPropertyChanged(string propertyName)
        //{
        //    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        //}

        //#endregion

    }
}