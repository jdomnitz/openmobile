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
    [System.Serializable]
    public class OMImage : OMControl
    {
        /// <summary>
        /// Occurs when the image is changed
        /// </summary>
        public event userInteraction OnImageChange;

        private imageItem image;

        private Color _BackgroundColor = Color.Transparent;

        public Color BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                if (value == _BackgroundColor)
                    return;

                _BackgroundColor = value;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Rotation in 3D space
        /// </summary>
        public OpenMobile.Math.Vector3 Rotation
        {
            get
            {
                return this._Rotation;
            }
            set
            {
                if (this._Rotation != value)
                {
                    this._Rotation = value;
                    Refresh();
                }
            }
        }
        private OpenMobile.Math.Vector3 _Rotation = new OpenMobile.Math.Vector3();
        

        /// <summary>
        /// Creates the control with the default size and location
        /// <para>[Obsolete] Use OMImage(string name, int left, int top, int width, int height) instead</para>
        /// </summary>
        [Obsolete("Use OMImage(string name, int left, int top, int width, int height) instead")]
        public OMImage()
            : base("", 20, 20, 100, 100)
        {
        }
        /// <summary>
        /// Creates a new OMImage
        /// <para>[Obsolete] Use OMImage(string name, int left, int top, int width, int height) instead</para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [Obsolete("Use OMImage(string name, int left, int top, int width, int height) instead")]
        public OMImage(int left, int top, int width, int height)
            : base("", left, top, width, height)
        {
        }
        /// <summary>
        /// Creates a new OMImage
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMImage(string name, int left, int top, int width, int height)
            : base(name, left, top, width, height)
        {
        }
        /// <summary>
        /// Creates a new OMImage
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMImage(string name, int left, int top, int width, int height, imageItem image)
            : base(name, left, top, width, height)
        {
            this.Image = image;
        }
        /// <summary>
        /// Creates a new OMImage with the given image and autofits the control to the image size
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="image"></param>
        public OMImage(string name, int left, int top, imageItem image)
            : base(name, left, top, 0, 0)
        {
            this.FitControlToImage = true;
            this.Image = image;
        }
        /// <summary>
        /// Creates a new OMImage with the given image and autofits the control to the image size
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="image"></param>
        public OMImage(string name, int left, int top)
            : base(name, left, top, 0, 0)
        {
            this.FitControlToImage = true;
        }

        private bool _FitControlToImage = false;
        /// <summary>
        /// Sets the control to autosize itself to the size of the assigned image
        /// </summary>
        public bool FitControlToImage
        {
            get
            {
                return _FitControlToImage;
            }
            set
            {
                _FitControlToImage = value;
                if (value)
                    FitControl(image);
            }
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
                if (image == value)
                    return;

                image = value;
                // Activate animation if not already disabled
                if (Animate)
                    _ShowAnimation = this.CanAnimate;
                else
                    _ShowAnimation = false;

                // Fit control to image?
                if (_FitControlToImage)
                    FitControl(image);

                raiseUpdate(false);

                // Raise image change event
                if (OnImageChange != null)
                    OnImageChange(this, this.parent.ActiveScreen);
            }
        }

        private void FitControl(imageItem Image)
        {
            if ((image == null) || (Image.image == null))
                return;
            this.Width = Image.image.Width;
            this.Height = Image.image.Height;
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            // Initialize rendering
            base.RenderBegin(g, e);

            // Render background (if any)
            if (_BackgroundColor != Color.Transparent)
            {
                using (Brush Fill = new Brush(Color.FromArgb((int)(this.GetAlphaValue255(_BackgroundColor.A)), _BackgroundColor)))
                {
                    g.FillRectangle(Fill, left, top, width, height);
                }
            }

            //if (image.image == null)
            //{
            //    if (image == imageItem.MISSING)
            //        g.FillRectangle(new Brush(Color.FromArgb((int)(255 * OpacityFloat), Color.Black)), left, top, width, height);
            //}
            //else
            if (image.image != null)
            {
                // Enable animation?
                if (_ShowAnimation)
                {
                    #region Animation

                    if (aimg == null)
                    {   // Create animation object
                        aimg = new OAnimatedImage(image.image.image);
                        if (DrawAnimationDelegate==null)
                            DrawAnimationDelegate = new OAnimatedImage.redraw(aimg_OnRedraw);
                        aimg.OnRedraw += DrawAnimationDelegate;
                    }
                    else
                    {   // Get current frame from animation
                        image.image = aimg.getFrame();
                    }

                    #endregion
                }

                //float alpha = OpacityFloat;
                //if (this.Mode == eModeType.transitioningIn)
                //    alpha = e.globalTransitionIn;
                //else if (this.Mode == eModeType.transitioningOut)
                //    alpha = e.globalTransitionOut;

                // Draw image background (if any)
                if (_BackgroundColor != Color.Transparent && _RenderingValue_Alpha == 1)
                    g.FillRectangle(new Brush(Color.FromArgb((int)(_BackgroundColor.A * OpacityFloat), _BackgroundColor)), new Rectangle(left + 1, top + 1, width - 2, height - 2));

                lock (image.image)
                {
                    switch (drawmode)
                    {
                        case DrawModes.Crop:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), 0, 0, width, height, _RenderingValue_Alpha);
                            break;
                        case DrawModes.CropLeft:
                            g.DrawImage(image.image, new Rectangle(left, top, width, height), image.image.Width - width, image.image.Height - height, width, height, _RenderingValue_Alpha);
                            break;
                        case DrawModes.Scale:
                            g.DrawImage(image.image, left, top, 0, width, height, _RenderingValue_Alpha, eAngle.Normal, Rotation, new ReflectionsData(Color.FromArgb(255,130,130,130),Color.Black));
                            //g.DrawImage(image.image, left, top, width, height, _RenderingValue_Alpha, eAngle.Normal);
                            break;
                    }
                }
            }

            // End rendering
            base.RenderFinish(g, e);
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

        #region Animiation

        private OAnimatedImage aimg = null;
        private OAnimatedImage.redraw DrawAnimationDelegate = null;

        /// <summary>
        /// Can the image in this control animate?
        /// </summary>
        public bool CanAnimate
        {
            get
            {
                if ((image == null) || (image.image == null))
                    return false;
                return image.image.CanAnimate;
            }
        }

        private bool _ShowAnimation = false;
        private bool _Animate = true;
        /// <summary>
        /// Enable image animation
        /// </summary>
        public bool Animate
        {
            get
            {
                return _Animate;
            }
            set
            {
                if (!CanAnimate)
                {
                    _Animate = false;
                    return;
                }
                _Animate = value;
                _ShowAnimation = value;
            }
        }

        private void aimg_OnRedraw(OAnimatedImage image)
        {   // Trigg a refresh of this object when animation timer fires
            raiseUpdate(false);
        }

        #endregion

        ~OMImage()
        {
            // Stop any animation threads
            if (aimg != null)
                aimg.OnRedraw -= DrawAnimationDelegate; 
        }

        #region DataSource

        internal override void DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            // Is this a binary data source, if so use the true/false state to show/hide image
            if (dataSource.DataType == OpenMobile.Data.DataSource.DataTypes.binary)
            {
                try
                {
                    this.Visible = (bool)dataSource.Value;
                }
                catch
                {
                    this.Visible = false;
                }
            }
            else if (dataSource.Value is OImage)
            {   // Actual image data present
                this.Image = new imageItem(dataSource.Value as OImage);
            }
            else if (dataSource.Value is imageItem)
            {   // Actual image data present
                this.Image = (imageItem)dataSource.Value;
            }
            else
            {   // This is not a binary datasource, use null to detect state
                this.Visible = dataSource.Value != null;
            }
            _RefreshGraphic = true;
            Refresh();
        }

        #endregion


    }
}