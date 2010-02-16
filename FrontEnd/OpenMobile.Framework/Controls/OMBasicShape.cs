using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Controls;
using System.Drawing;
using System.ComponentModel;

namespace OpenMobile.Controls
{
    public enum shapes
    {
        Rectangle=0
    }
    public class OMBasicShape:OMControl
    {
        private int left, top, width, height;
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

        public override void Render(System.Drawing.Graphics g, renderingParams e)
        {
            if (shape == shapes.Rectangle)
            {
                g.FillRectangle(new SolidBrush(fillColor), toRegion());
            }
        }
        private shapes shape;
        private Color fillColor;
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
        public override Rectangle toRegion()
        {
            return new Rectangle(Left,Top,Width,Height);
        }
    }
}
