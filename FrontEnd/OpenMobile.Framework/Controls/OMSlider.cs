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
using System.Threading;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A slider bar control
    /// </summary>
    public class OMSlider:OMControl,IThrow
    {
        protected int width;
        protected int height;
        protected int top;
        protected int left;
        protected int sliderHeight=25;
        protected int sliderWidth;
        protected int sliderPosition = 0;
        protected imageItem sliderBar;
        protected imageItem slider;
        protected int minimum = 0;
        protected int maximum = 100;
        /// <summary>
        /// Create a new OMSlider
        /// </summary>
        public OMSlider()
        {
        }
        /// <summary>
        /// Create a new OMSlider
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMSlider(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            sliderHeight = h;
        }
        /// <summary>
        /// Create a new OMSlider
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="sHeight"></param>
        /// <param name="sWidth"></param>
        public OMSlider(int x, int y, int w, int h,int sHeight,int sWidth)
        {
            left = x;
            top = y;
            width = w;
            sliderHeight = h;
            height=sHeight;
            sliderWidth = sWidth;
        }
        /// <summary>
        /// Occurs when the slider value changes
        /// </summary>
        public delegate void slidermoved(OMSlider sender,int screen);
        /// <summary>
        /// Occurs when the slider value changes
        /// </summary>
        public event slidermoved OnSliderMoved;

        /// <summary>
        /// Raises the OnSliderMoved event
        /// </summary>
        public void sliderMoved(int screen)
        {
            if (OnSliderMoved!=null)
                OnSliderMoved(this,screen);
        }

        /// <summary>
        /// The height of the slider
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the height of the control")]
        public override int Height
        {
            get
            {
                return sliderHeight;
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
        /// The width of the slider track
        /// </summary>
        [CategoryAttribute("General"), Description("The width of the slider track")]
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
        /// The top of this control
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
                refreshMe(new Rectangle(left, top > oldtop ? oldtop : top, width, height + Math.Abs(oldtop - top)));
            }
        }
        /// <summary>
        /// The width of the image on top of the slider track
        /// </summary>
        [Description("The width of the image on top of the slider track")]
        public int SliderWidth
        {
            get
            {
                return sliderWidth;
            }
            set
            {
                if (sliderWidth == value)
                    return;
                sliderWidth = value;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// The height of the slider bar image
        /// </summary>
        [Description("The height of the slider bar image")]
        public int SliderBarHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value)
                    return;
                height = value;
                refreshMe(this.toRegion());
            }
        }
        /// <summary>
        /// The leftmost point of the slider bar
        /// </summary>
        [CategoryAttribute("General"), Description("The leftmost point of the slider bar")]
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
        /// The minimum slider value
        /// </summary>
        [Category("Slider"),Description("The minimum slider value")]
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
        /// The maximum slider value
        /// </summary>
        [Category("Slider"),Description("The maximum slider value")]
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
            }
        }
        /// <summary>
        /// The value of the slider
        /// </summary>
        [Category("Slider"),Description("The value of the slider")]
        public int Value
        {
            get
            {
                return (int)((sliderPosition/(float)width)*(maximum))+minimum;
            }
            set
            {
                if ((value >= minimum) && (value <= maximum))
                {
                    sliderPosition = (int)(((value-minimum) / (float)maximum) * width);
                    refreshMe(this.toRegion());
                }
            }
        }
        /// <summary>
        /// The image of the slider track
        /// </summary>
        [Category("Graphics"),Description("The image of the slider track")]
        public imageItem SliderBar
        {
            get
            {
                return sliderBar;
            }
            set 
            {
                sliderBar = value;
            }
        }
        /// <summary>
        /// The image of the slider
        /// </summary>
        [Category("Graphics"),Description("The image of the slider")]
        public imageItem Slider
        {
            get
            {
                return slider;
            }
            set 
            {
                slider = value;
            }
        }
        /// <summary>
        /// The type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Slider";
            }
        }
        /// <summary>
        /// Returns the region occupied by the control
        /// </summary>
        /// <returns></returns>
        public override Rectangle toRegion()
        {
            if (sliderHeight>height)
                return new Rectangle(Left - (sliderWidth / 2), Top - (sliderHeight / 2)+(height/2)-1, Width + sliderWidth, sliderHeight+2);
            else
                return new Rectangle(Left, Top, Width, Height);
        }

        /// <summary>
        /// Draws the slider to the User Interface
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(System.Drawing.Graphics g,renderingParams e)
        {
            try
            {
                g.DrawImage(sliderBar.image, new Rectangle(left, top, width, height));
                g.DrawImage(slider.image, new Rectangle(left + sliderPosition-(sliderHeight/2), (top + (height / 2)) - (sliderHeight / 2), sliderWidth, sliderHeight));
            }
            catch (Exception) { }
        }

        #region IThrow Members
        /// <summary>
        /// The slider has been thrown
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="TotalDistance"></param>
        /// <param name="RelativeDistance"></param>
        public void MouseThrow(int screen, Point TotalDistance, Point RelativeDistance)
        {
            sliderPosition += RelativeDistance.X;
            if (sliderPosition < 0)
                sliderPosition = 0;
            if ((sliderPosition + (sliderWidth / 2)) < 0)
                sliderPosition = -(sliderWidth / 2);
            if ((sliderPosition + (sliderWidth / 2)) > Width)
                sliderPosition = Width - (SliderWidth / 2);
            new Thread(delegate() { sliderMoved(screen); }).Start();
            this.refreshMe(this.toRegion());
        }
        /// <summary>
        /// The throw has started
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="StartLocation"></param>
        /// <param name="Cancel"></param>
        /// <param name="sf"></param>
        public void MouseThrowStart(int screen, Point StartLocation,PointF sf, ref bool Cancel)
        {
            
        }
        /// <summary>
        /// The throw has ended
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="EndLocation"></param>
        public void MouseThrowEnd(int screen, Point EndLocation)
        {
            
        }

        #endregion
    }
}
