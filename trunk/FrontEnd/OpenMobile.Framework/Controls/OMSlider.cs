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
using OpenMobile.Threading;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A slider bar control
    /// </summary>
    public class OMSlider:OMControl,IThrow
    {
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
                sliderWidth = value;
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
                height = value;
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
                return (int)((((sliderPosition - (sliderWidth / 2))/ (float)(width - sliderWidth)) * (maximum)) + minimum);
            }
            set
            {
                if ((value >= minimum) && (value <= maximum))
                {
                    sliderPosition = (int)(((value - minimum) / (float)maximum) * (width - sliderWidth)) + (sliderWidth / 2);
                }
            }
        }
        /// <summary>
        /// The image of the slider track
        /// </summary>
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
        imageItem sliderTrackFull;
        /// <summary>
        /// The background image for the slider
        /// </summary>
        public imageItem SliderTrackFull
        {
            get
            {
                return sliderTrackFull;
            }
            set
            {
                sliderTrackFull = value;
            }
        }

        /// <summary>
        /// The image of the slider
        /// </summary>
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
        /// Draws the slider to the User Interface
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            g.DrawImage(sliderBar.image, left, top, width, height);
            if (sliderTrackFull.image != null)
            {
                Rectangle clip = g.Clip;
                g.SetClipFast(left,top, sliderPosition, height);
                g.DrawImage(sliderTrackFull.image, left-2, top, width+4, height);
                g.Clip = clip;
            }
            g.DrawImage(slider.image, left + sliderPosition - (sliderHeight / 2), (top + (height / 2)) - (sliderHeight / 2), sliderWidth, sliderHeight);
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
            if ((sliderPosition - (sliderWidth / 2)) < 0)
                sliderPosition = (sliderWidth / 2);
            if ((sliderPosition + (sliderWidth / 2)) > Width)
                sliderPosition = Width - (SliderWidth / 2);
            SafeThread.Asynchronous(delegate() { if (OnSliderMoved != null) OnSliderMoved(this, screen); }, null);
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
