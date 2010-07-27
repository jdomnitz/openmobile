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
using System.Drawing.Drawing2D;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A progress bar control
    /// </summary>
    public class OMProgress:OMControl
    {
        protected int height;
        protected int width;
        protected int top;
        protected int left;
        protected int value=0;
        protected int minimum = 0;
        protected int maximum = 100;
        protected bool vertical;
        protected Color firstColor=Color.DarkRed;
        protected Color secondColor=Color.Red;
        protected Color backColor = Color.FromArgb(180, Color.White);

        /// <summary>
        /// The background color of the progress bar
        /// </summary>
        [Category("Progress Bar"), Description("The background color of the progress bar")]
        public Color BackgroundColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor == value)
                    return;
                backColor = value;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// Display the progress bar vertically instead
        /// </summary>
        public bool Vertical
        {
            get
            {
                return vertical;
            }
            set
            {
                vertical = value;
            }
        }

        /// <summary>
        /// Create the control with the default size and location
        /// </summary>
        public OMProgress()
        {
            this.Top = 20;
            this.Left = 20;
            this.Width = 100;
            this.Height = 25;
        }
        /// <summary>
        /// Create a new progress bar
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMProgress(int x,int y,int w,int h)
        {
            top = y;
            left = x;
            width = w;
            height = h;
        }
        /// <summary>
        /// A number between the minimum and the maximum value
        /// </summary>
        [Category("Progress Bar"),Description("An integer between the minimum and maximum value")]
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if ((value >= this.minimum) && (value <= this.maximum))
                    if (this.value!=value)
                    {
                        this.value = value;
                        refreshMe(this.toRegion());
                    }
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 100%
        /// </summary>
        [Category("Progress Bar"),Description("The value at which the progress bar displays 100%")]
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (maximum == value)
                    return;
                if (Value > value)
                    Value = value;
                maximum = value;
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 0%
        /// </summary>
        [Category("Progress Bar"),Description("The value at which the progress bar displays 0%")]
        public int Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
            }
        }
        /// <summary>
        /// Represents the first of two colors for the progress bars gradient
        /// </summary>
        [Category("Progress Bar"),Description("Represents the first of two colors for the progress bar's gradient or the color of a solid background.")]
        public Color FirstColor
        {
            get
            {
                return firstColor;
            }
            set
            {
                if (firstColor == value)
                    return;
                firstColor = value;
                refreshMe(this.toRegion());
            }
        }

        /// <summary>
        /// Represents the second of two colors for the progress bars gradient
        /// </summary>
        [Category("Progress Bar"), Description("Represents the second of two colors for the progress bar's gradient.")]
        public Color SecondColor
        {
            get
            {
                return secondColor;
            }
            set
            {
                if (secondColor == value)
                    return;
                secondColor = value;
                refreshMe(this.toRegion());
            }
        }

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Progress Bar";
            }
        }

        /// <summary>
        /// The height of the control in pixels
        /// </summary>
        [Category("General"),Description("The height of the control in pixels")]
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
        /// The width of the control in pixels
        /// </summary>
        [Category("General"),Description("The width of the control in pixels")]
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
        [Category("General"),Description("The distance between the top edge of the control and the top edge of the user interface")]
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
                refreshMe(new Rectangle(left, top > oldtop ? oldtop : top, width + 2, height + Math.Abs(oldtop - top) + 2));
            }
        }

        /// <summary>
        /// The distance between the left edge of the control and the left edge of the user interface
        /// </summary>
        [Category("General"),Description("The distance between the left edge of the control and the left edge of the user interface")]
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
                refreshMe(new Rectangle(left > oldleft ? oldleft : left, top, width + Math.Abs(oldleft - left), height));
            }
        }
        /// <summary>
        /// Returns the region occupied by the control
        /// </summary>
        /// <returns></returns>
        public override Rectangle toRegion()
        {
            return new Rectangle(Left, Top, Width, Height);
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            float tmp = 1;
            if (Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            Rectangle rec = this.toRegion();
            g.FillRectangle(new Brush(Color.FromArgb((int)(backColor.A * tmp), backColor)), rec);
            if (vertical == false)
            {
                rec.Width = (int)(width * ((float)value / maximum));
                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * firstColor.A), firstColor), Color.FromArgb((int)(tmp * secondColor.A), secondColor),Gradient.Horizontal), rec);
                rec.Width = width;
            }
            else
            {
                rec.Y = rec.Y+rec.Height - (int)(height * ((float)value / maximum));
                rec.Height= (int)(height * ((float)value / maximum));
                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * secondColor.A), secondColor),Color.FromArgb((int)(tmp * firstColor.A), firstColor),Gradient.Vertical), rec);
                rec.Y = top;
                rec.Height = height;
            }
            g.DrawRectangle(new Pen(Color.FromArgb((int)(tmp * 255), Color.Black), 1.5F), rec);
        }
    }
}
