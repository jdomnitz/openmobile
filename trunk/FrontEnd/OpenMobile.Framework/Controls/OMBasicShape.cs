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
using OpenMobile.Graphics;

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
        // Start of code added by Borte
        protected int cornerRadius = 10;
        /// <summary>
        /// Sets the corner radius of a rounded rectangle (Added by Borte)
        /// </summary>
        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; }
        }
        // End of code added by Borte
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
        /// Draws the basic shape
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            // Start of code added by Borte
            // Basic shape didn't respect the transition values while rendering
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
            {
                tmp = e.globalTransitionIn;
            }
            if (this.Mode == eModeType.transitioningOut)
            {
                tmp = e.globalTransitionOut;
            }

            Brush Fill = new Brush(Color.FromArgb((int)(tmp * fillColor.A), fillColor));
            Pen BorderPen = new Pen(Color.FromArgb((int)(tmp * borderColor.A), borderColor), borderSize);
            // End of code added by Borte

            switch (shape)
            {
                case shapes.Rectangle:
                    g.FillRectangle(Fill, toRegion());
                    if (borderSize > 0)
                        g.DrawRectangle(BorderPen, toRegion());
                    break;
                case shapes.Triangle:
                    g.FillPolygon(Fill, new Point[] { new Point(left, top + height), new Point(left + width, top + height), new Point(left + (width / 2), top) });
                    if (borderSize > 0)
                        g.DrawPolygon(BorderPen, new Point[] { new Point(left, top + height), new Point(left + width, top + height), new Point(left + (width / 2), top) });
                    break;
                case shapes.Oval:
                    g.FillEllipse(Fill, toRegion());
                    if (borderSize > 0)
                        g.DrawEllipse(BorderPen, toRegion());
                    break;
                case shapes.RoundedRectangle:
                    g.FillRoundRectangle(Fill, new Rectangle(this.left, this.top, this.width, this.height), cornerRadius);
                    if (borderSize > 0)
                        g.DrawRoundRectangle(BorderPen, new Rectangle(this.left, this.top, this.width, this.height), cornerRadius);
                    break;
            }
        }
        protected shapes shape;
        protected Color fillColor;
        protected float borderSize = 0;
        protected Color borderColor;
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
                borderColor = value;
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
                borderSize = value;
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
