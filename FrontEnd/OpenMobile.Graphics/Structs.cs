#pragma warning disable 0659,0661
using System;
using System.Reflection;
namespace OpenMobile.Graphics
{
    public enum Gradient:byte
    {
        None=0,
        Vertical=1,
        Horizontal=2
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
    
    public struct Brush:IDisposable
    {
        Color color;
        Color secondColor;
        Gradient gradient;
        
        public Brush(Color color)
        {
            this.color = color;
            this.secondColor = Color.Empty;
            gradient = Gradient.None;
        }
        public Brush(Color first, Color second,Gradient gradient)
        {
            this.color = first;
            this.secondColor = second;
            this.gradient = gradient;
        }
        public Color Color
        {
            get
            {
                return color;
            }
        }
        public Color SecondColor
        {
            get
            {
                return secondColor;
            }
        }
        public Gradient Gradient
        {
            get
            {
                return gradient;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Legacy
        }

        #endregion
    }

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
        public static bool operator ==(Color left, Color right)
        {
            return ((left.A == right.A) && (left.b == right.B) && (left.G == right.G) && (left.R == right.R));
        }
        public override bool Equals(object obj)
        {
            Color right = (Color)obj;
            return ((this.A == right.A) && (this.b == right.B) && (this.G == right.G) && (this.R == right.R));
        }
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }
        internal Color(int a, int r, int g, int b)
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
                return (name!="");
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
        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            return new Color(alpha, red, green, blue);
        }

        public static Color FromArgb(int alpha, Color baseColor)
        {
            return new Color(alpha, baseColor.R, baseColor.G, baseColor.B,baseColor.Name);
        }

        public static Color FromArgb(int red, int green, int blue)
        {
            return new Color(0xff, red, green, blue);
        }
        public override string ToString()
        {
            return name+"("+a.ToString()+','+r.ToString()+','+g.ToString()+','+b.ToString()+')';
        }
        static Color()
        {
            Empty = new Color();
            black = new Color(0xFF, 0, 0, 0, "Black");
            white = new Color(0xFF, 255, 255, 255, "White");
            transparent = new Color(0, 0xFF, 0xFF, 0xFF, "Transparent");
        }
    }
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
        static PointF()
        {
            Empty=new PointF();
        }
    }
#pragma warning disable 0660
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
    }
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

        static Point()
        {
            Empty = new Point();
        }
    }
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

    public struct Rectangle
    {
        public static readonly Rectangle Empty;
        public int X;
        public int Y;
        public int Width;
        public int Height;

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
        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            this.X = (int)Math.Round(x);
            this.Y = (int)Math.Round(y);
            this.Width = (int)Math.Round(width);
            this.Height = (int)Math.Round(height);
        }

        public Rectangle(Point location, Size size)
        {
            this.X = location.X;
            this.Y = location.Y;
            this.Width = size.Width;
            this.Height = size.Height;
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

        public static Rectangle Round(System.Drawing.RectangleF value)
        {
            return new Rectangle((int)Math.Round((double)value.X), (int)Math.Round((double)value.Y), (int)Math.Round((double)value.Width), (int)Math.Round((double)value.Height));
        }

        public override string ToString()
        {
            return ("{X=" + this.X.ToString() + ",Y=" + this.Y.ToString() + ",Width=" + this.Width.ToString() + ",Height=" + this.Height.ToString() + "}");
        }

        static Rectangle()
        {
            Empty = new Rectangle();
        }
    }
}