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
using System.ComponentModel;
using System.Drawing;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A label for displaying text
    /// </summary>
    [DefaultPropertyAttribute("Name")]
    public class OMLabel:OMControl
    {
        // Comment by Borte: access modefier changed to protected to allow more access when this control is inherited
        protected int height=100;
        protected int width = 130;
        protected int top;
        protected int left;
        protected string text="";
        protected OpenMobile.Graphics.eTextFormat textFormat = OpenMobile.Graphics.eTextFormat.Normal;
        protected OpenMobile.Graphics.Alignment textAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
        protected Color color = Color.White;
        protected Font font = new Font(FontFamily.GenericSansSerif, 18F);
        protected Color outlineColor = Color.Black;

        /// <summary>
        /// Sets the color of the text
        /// </summary>
        [Editor(typeof(OpenMobile.transparentColor),typeof(System.Drawing.Design.UITypeEditor)),TypeConverter(typeof(OpenMobile.ColorConvertor))]
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the color of the text")]
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                if (color == value)
                    return;
                color = value;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        public OMLabel()
        {
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMLabel(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            height = h;
        }
        /// <summary>
        /// Creates a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return base.Clone();
        }
        /// <summary>
        /// Sets the Glow or Outline color of the text
        /// </summary>
        [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the Glow or Outline color of the text")]
        public virtual Color OutlineColor
        {
            get
            {
                return outlineColor;
            }
            set
            {
                outlineColor = value;
            }
        }

        /// <summary>
        /// Sets the Font of the text (Note: Size and Font Name only)
        /// </summary>
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the font of the text")]
        public Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Label";
            }
        }

        /// <summary>
        /// The controls height in pixels
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the height of the control")]
        public override int Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value)
                    return;
                if (value >= height)
                {
                    height = value;
                    refreshMe(this.toRegion());
                }
                else
                {
                    Rectangle r=this.toRegion();
                    height = value;
                    refreshMe(r);
                }
            }
        }
        /// <summary>
        /// The controls width in pixels
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the width of the control")]
        public override int Width
        {
            get
            {
                return width;
            }
            set
            {
                if (value == width)
                    return;
                if (value >= width)
                {
                    width = value;
                    refreshMe(this.toRegion());
                }
                else
                {
                    Rectangle r = this.toRegion();
                    width = value;
                    refreshMe(r);
                }
            }
        }
        /// <summary>
        /// The distance between the top edge of the control and the top edge of the user interface
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the top position of the control")]
        public override int Top
        {
            get
            {
                return top;
            }
            set
            {
                if (top == value)
                    return;
                int oldtop=top;
                top = value;
                refreshMe(new Rectangle(left,top>oldtop?oldtop:top,width+2,height+Math.Abs(oldtop-top)+2));
            }
        }
        /// <summary>
        /// The distance between the left edge of the control and the left edge of the user interface
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the left position of the control")]
        public override int Left
        {
            get
            {
                return left;
            }
            set
            {
                if (left == value)
                    return;
                int oldleft = left;
                left = value;
                refreshMe(new Rectangle(left>oldleft?oldleft:left, top, width+Math.Abs(oldleft-left), height));
            }
        }
        protected OImage textTexture;
        /// <summary>
        /// The text displayed in the label
        /// </summary>
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the text to display")]
        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text == value)
                    return;
                text = value;
                textTexture = null;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Sets the format of the displayed text
        /// </summary>
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the format of the displayed text")]
        public virtual OpenMobile.Graphics.eTextFormat Format
        {
            get
            {
                return textFormat;
            }
            set
            {
                if (textFormat == value)
                    return;
                textFormat = value;
                textTexture = null;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Sets the alignment of the displayed text
        /// </summary>
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the alignment of the displayed text")]
        public virtual OpenMobile.Graphics.Alignment TextAlignment
        {
            get
            {
                return textAlignment;
            }
            set
            {
                textAlignment = value;
                textTexture = null;
            }
        }
        /// <summary>
        /// Returns the region occupied by the control
        /// </summary>
        /// <returns></returns>
        public override Rectangle toRegion()
        {
            return new Rectangle(Left, Top, Width+2, Height+2);
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
            {
                tmp = e.globalTransitionIn;
            }
            if (this.Mode == eModeType.transitioningOut)
            {
                tmp = e.globalTransitionOut;
            }
            if (textTexture==null)
                textTexture=g.GenerateTextTexture(left, top, width, height, text, font, textFormat, textAlignment, color, outlineColor);
            g.DrawImage(textTexture, left, top, width, height);
        }
    }
}
