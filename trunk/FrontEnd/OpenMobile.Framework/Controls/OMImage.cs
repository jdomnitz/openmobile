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
using System.Drawing.Imaging;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// An image control
    /// </summary>
    [DefaultPropertyAttribute("Name")]
    public class OMImage : OMControl
    {
        private int height;
        private int width;
        private int top;
        private int left;
        private imageItem image;
        private int transparency = 100;

        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>

        public int Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                if (transparency == value)
                    return;
                transparency = value;
                refreshMe(this.toRegion());
            }
        }

        /// <summary>
        /// Creates the control with the default size and location
        /// </summary>
        public OMImage()
        {
            this.top = 20;
            this.left = 20;
            this.width = 100;
            this.height = 100;
        }
        /// <summary>
        /// Creates a new OMImage
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMImage(int left, int top, int width, int height)
        {
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
        }
        /// <summary>
        /// The image to be rendered
        /// </summary>
        [Category("Image"), Description("The image to be rendered")]
        public imageItem Image
        {
            get
            {
                return image;
            }
            set
            {
                if (image.image != value.image)
                {
                    image = value;
                    try
                    {
                        if (ImageAnimator.CanAnimate(value.image.image) == true)
                            ImageAnimator.Animate(value.image.image, new EventHandler(update));
                    }
                    catch (InvalidOperationException) { }
                    refreshMe(this.toRegion());
                }
            }
        }

        private void update(object sender, EventArgs e)
        {
            this.refreshMe(this.toRegion());
        }

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Image";
            }
        }
        /// <summary>
        /// The controls height in pixels
        /// </summary>
        [Category("General"), Description("The height of the control in pixels")]
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
        /// The controls height in pixels
        /// </summary>
        [Category("General"), Description("The controls height in pixels")]
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
        /// The distance from the top of the control to the top of the UI
        /// </summary>
        [Category("General"), Description("The distance from the top of the control to the top of the UI")]
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
        /// The distance from the left of the control to the UI
        /// </summary>
        [Category("General"), Description("The distance from the left of the control to the UI")]
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
            return new Rectangle(Left - 1, Top - 1, Width + 2, Height + 2);
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            if (image.image == null)
            {
                if (image==imageItem.MISSING)
                    g.FillRectangle(Brushes.Black, left, top, width, height);
            }
            else
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
                float alpha = tmp * (transparency / 100F);
                lock (image.image)
                {
                    // Start of code added by Borte
                    switch (drawmode)
                    {
                        case DrawModes.Crop:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), 0, 0, width, height, GraphicsUnit.Pixel, alpha);
                            break;
                        case DrawModes.CropLeft:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), image.image.Width - width, image.image.Height - height, width, height, GraphicsUnit.Pixel, alpha);
                            break;
                        case DrawModes.Scale:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), 0, 0, image.image.Width, image.image.Height, GraphicsUnit.Pixel, alpha);
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Draw modes for an image
        /// </summary>
        public enum DrawModes
        {
            /// <summary>
            /// Scale image to match control size (Default)
            /// </summary>
            Scale,
            /// <summary>
            /// Cut image to match control size (right and bottom side of image will be cut)
            /// </summary>
            Crop,
            /// <summary>
            /// Cut image to match control size (Left and bottom side of image will be cut)
            /// </summary>
            CropLeft
        }
        private DrawModes drawmode = DrawModes.Scale;
        /// <summary>
        /// The image drawing mode
        /// </summary>
        public DrawModes DrawMode
        {
            get
            {
                return drawmode;
            }
            set
            {
                if (drawmode == value)
                    return;
                drawmode = value;
                refreshMe(this.toRegion());
            }
        }
        // End of code added by Borte
    }
}