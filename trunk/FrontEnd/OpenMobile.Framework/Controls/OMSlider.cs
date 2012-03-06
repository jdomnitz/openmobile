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
    public class OMSlider : OMControl, IThrow, IMouse
    {
        /// <summary>
        /// height of the slider bar
        /// </summary>
        protected int sliderHeight = 25;
        /// <summary>
        /// width of the slider bar
        /// </summary>
        protected int sliderWidth;
        /// <summary>
        /// Slider position (OM units)
        /// </summary>
        protected int sliderPosition = 0;
        /// <summary>
        /// slider bar image
        /// </summary>
        protected imageItem sliderBar;
        /// <summary>
        /// slider track image
        /// </summary>
        protected imageItem slider;
        /// <summary>
        /// minimum value
        /// </summary>
        protected int minimum = 0;
        /// <summary>
        /// maximum value
        /// </summary>
        protected int maximum = 100;
        /// <summary>
        /// Create a new OMSlider
        /// </summary>
        [System.Obsolete("Use OMSlider(string name, int x, int y, int w, int h) instead")]
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
        [System.Obsolete("Use OMSlider(string name, int x, int y, int w, int h, int sHeight, int sWidth) instead")]
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
        [System.Obsolete("Use OMSlider(string name, int x, int y, int w, int h, int sHeight, int sWidth) instead")]
        public OMSlider(int x, int y, int w, int h, int sHeight, int sWidth)
        {
            left = x;
            top = y;
            width = w;
            sliderHeight = h;
            height = sHeight;
            sliderWidth = sWidth;
        }
        /// <summary>
        /// Create a new OMSlider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="sHeight"></param>
        /// <param name="sWidth"></param>
        public OMSlider(string name, int x, int y, int w, int h, int sHeight, int sWidth)
        {
            this.Name = name;
            left = x;
            top = y;
            width = w;
            sliderHeight = h;
            height = sHeight;
            sliderWidth = sWidth;
        }
        /// <summary>
        /// Occurs when the slider value changes
        /// </summary>
        public delegate void slidermoved(OMSlider sender, int screen);
        /// <summary>
        /// Occurs when the slider value changes
        /// </summary>
        public event slidermoved OnSliderMoved;

        /// <summary>
        /// The width of the image on top of the slider track
        /// </summary>
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
        public int Value
        {
            get
            {
                return (int)((((sliderPosition - (sliderWidth / 2)) / (float)(width - sliderWidth)) * (maximum - minimum)) + minimum);
            }
            set
            {
                if ((value >= minimum) && (value <= maximum))
                {
                    sliderPosition = (int)(((value - minimum) / (float)(maximum - minimum)) * (width - sliderWidth)) + (sliderWidth / 2);
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
                g.SetClipFast(left, top, sliderPosition, height);
                g.DrawImage(sliderTrackFull.image, left - 2, top, width + 4, height);
                g.Clip = clip;
            }
            g.DrawImage(slider.image, left + sliderPosition - (sliderHeight / 2), (top + (height / 2)) - (sliderHeight / 2), sliderWidth, sliderHeight);

            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Yellow);
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
        public void MouseThrowStart(int screen, Point StartLocation, PointF sf, ref bool Cancel)
        {
            dragged = true;
        }
        /// <summary>
        /// The throw has ended
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="EndLocation"></param>
        public void MouseThrowEnd(int screen, Point EndLocation)
        {
            dragged = false;
        }

        #endregion

        /// <summary>
        /// Is the slider being dragged
        /// </summary>
        public bool Dragging
        {
            get
            {
                return dragged;
            }
        }

        #region IMouse Members
        /// <summary>
        /// The slider is being dragged
        /// </summary>
        protected bool dragged;
        /// <summary>
        /// Not Used
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, float WidthScale, float HeightScale)
        {
            //
        }
        /// <summary>
        /// Not Used
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            //
        }
        /// <summary>
        /// Mouse Up
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            if (!dragged)
            {
                sliderPosition = (int)(e.X / WidthScale) - left;
                if ((sliderPosition - (sliderWidth / 2)) < 0)
                    sliderPosition = (sliderWidth / 2);
                if ((sliderPosition + (sliderWidth / 2)) > Width)
                    sliderPosition = Width - (SliderWidth / 2);
                SafeThread.Asynchronous(delegate() { if (OnSliderMoved != null) OnSliderMoved(this, screen); }, null);
            }
        }

        #endregion
    }
}
