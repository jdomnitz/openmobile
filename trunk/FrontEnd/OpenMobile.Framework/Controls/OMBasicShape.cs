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
using System.ComponentModel;
using System.Drawing;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Available Shapes
    /// </summary>
    public enum shapes
    {
        /// <summary>
        /// Draws a Rectangle (or Square)
        /// </summary>
        Rectangle=0,
        /// <summary>
        /// Draws a Triangle
        /// </summary>
        Triangle=1,
        /// <summary>
        /// Draws an Oval/Ellipse/Circle
        /// </summary>
        Oval=2,
        /// <summary>
        /// Draws a Rounded Rectangle
        /// </summary>
        RoundedRectangle=3
    }
    /// <summary>
    /// Allows drawing of basic shapes
    /// </summary>
    public class OMBasicShape:OMControl
    {
        private int left, top, width, height;
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        public OMBasicShape() { }
        /// <summary>
        /// Creates a new Basic Shape
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMBasicShape(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            height = h;
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
                    Rectangle r = this.toRegion();
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
                int oldtop = top;
                top = value;
                refreshMe(new Rectangle(left, top > oldtop ? oldtop : top, width + 2, height + System.Math.Abs(oldtop - top) + 2));
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
                refreshMe(new Rectangle(left > oldleft ? oldleft : left, top, width + System.Math.Abs(oldleft - left), height));
            }
        }
        /// <summary>
        /// Draws the basic shape
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(System.Drawing.Graphics g, renderingParams e)
        {
            switch (shape)
            {
                case shapes.Rectangle:
                    g.FillRectangle(new SolidBrush(fillColor), toRegion());
                    if (borderSize > 0)
                        g.DrawRectangle(new Pen(borderColor, borderSize), toRegion());
                    break;
                case shapes.Triangle:
                    g.FillPolygon(new SolidBrush(fillColor),new Point[]{new Point(left,top+height),new Point(left+width,top+height),new Point(left+(width/2),top)});
                    if (borderSize > 0)
                        g.DrawPolygon(new Pen(borderColor, borderSize), new Point[] { new Point(left, top + height), new Point(left + width, top + height), new Point(left + (width / 2), top) });
                    break;
                case shapes.Oval:
                    g.FillEllipse(new SolidBrush(fillColor), toRegion());
                    if (borderSize > 0)
                        g.DrawEllipse(new Pen(borderColor, borderSize), toRegion());
                    break;
                case shapes.RoundedRectangle:
                    Renderer.FillRoundRectangle(g, new SolidBrush(fillColor), new RectangleF(this.left, this.top, this.width, this.height), 10F);
                    if (borderSize > 0)
                        Renderer.DrawRoundRectangle(g, new Pen(borderColor, borderSize), new RectangleF(this.left, this.top, this.width, this.height), 10F);
                    break;
            }
        }
        private shapes shape;
        private Color fillColor;
        private float borderSize=0;
        private Color borderColor;
        /// <summary>
        /// The shape to draw
        /// </summary>
        public shapes Shape
        {
            set
            {
                shape = value;
            }
            get
            {
                return shape;
            }
        }
        /// <summary>
        /// The fill color of the Shape
        /// </summary>
        public Color FillColor
        {
            get
            {
                return fillColor;
            }
            set
            {
                fillColor = value;
            }
        }
        /// <summary>
        /// The color of the border
        /// </summary>
        public Color BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                if (borderColor == value)
                    return;
                borderColor = value;
                this.refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// The width in pixels of a border (0 for no border)
        /// </summary>
        public float BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                if (borderSize == value)
                    return;
                borderSize = value;
                this.refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Returns a rectangle representing the area covered
        /// </summary>
        /// <returns></returns>
        public override Rectangle toRegion()
        {
            return new Rectangle(Left,Top,Width,Height);
        }
        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Basic Shape";
            }
        }
    }
}
