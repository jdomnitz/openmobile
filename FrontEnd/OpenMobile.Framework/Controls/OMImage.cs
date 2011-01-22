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
        private imageItem image;
        private byte transparency = 100;
        /// <summary>
        /// Opacity (0-100%)
        /// </summary>
        public byte Transparency
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
                raiseUpdate(false);
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
        public OMImage(short left, short top, short width, short height)
        {
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
        }
        /// <summary>
        /// The image to be rendered
        /// </summary>
        public imageItem Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
            }
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
                if (image == imageItem.MISSING)
                    g.FillRectangle(new Brush(Color.Black), left, top, width, height);
            }
            else
            {
                float alpha = 1;
                if (this.Mode == eModeType.transitioningIn)
                    alpha = e.globalTransitionIn;
                else if (this.Mode == eModeType.transitioningOut)
                    alpha = e.globalTransitionOut;
                alpha = alpha * (transparency / 100F);
                lock (image.image)
                {
                    // Start of code added by Borte
                    switch (drawmode)
                    {
                        case DrawModes.Crop:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), 0, 0, width, height, alpha);
                            break;
                        case DrawModes.CropLeft:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), image.image.Width - width, image.image.Height - height, width, height, alpha);
                            break;
                        case DrawModes.Scale:
                            g.DrawImage(image.image, left, top, width, height, alpha);
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
            /// Cut image to match control size (Left and top side of image will be cut)
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
                drawmode = value;
            }
        }
        // End of code added by Borte
    }
}