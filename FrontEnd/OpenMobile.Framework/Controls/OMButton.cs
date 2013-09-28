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
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Button Mode Changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screen"></param>
    /// <param name="mode"></param>
    public delegate void ModeChange(OMButton sender, int screen, eModeType mode);
    /// <summary>
    /// A clickable button control
    /// </summary>
    [System.Serializable]
    public class OMButton : OMLabel, IClickable, IHighlightable
    {
        #region Preconfigured controls

        public static OMButton PreConfigLayout_BasicStyle_NoBackground(string name, int left, int top, int width, int height, GraphicCorners? corners, Color? borderColor)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, null, null, null, Color.Transparent, Color.Transparent, borderColor);
        }

        public static OMButton PreConfigLayout_BasicStyle_NoBackground(string name, int left, int top, int width, int height, GraphicCorners? corners, Color? borderColor, int fontSize)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, null, null, null, Color.Transparent, Color.Transparent, borderColor, null, fontSize);
        }

        public static OMButton PreConfigLayout_BasicStyle_NoBackground(string name, int left, int top, int width, int height, GraphicCorners? corners, Color? borderColor, Color? textColor, string icon, string text, int fontSize)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, icon, null, text, Color.Transparent, Color.Transparent, borderColor, null, fontSize, textColor);
        }


        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="corners"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, GraphicCorners? corners)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, null, null, null, null, null, null);
        }

        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="corners"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, GraphicCorners? corners, string icon, string text)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, icon, null, text, null, null, null);
        }

        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="opacity"></param>
        /// <param name="corners"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, int opacity, GraphicCorners? corners, string icon, string text)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, opacity, 15, corners, icon, null, text, null, null, null);
        }

        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="opacity"></param>
        /// <param name="corners"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, GraphicCorners? corners, string icon, string text, int fontSize)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, 255, 15, corners, icon, null, text, null, null, null, null, fontSize);
        }

        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="opacity"></param>
        /// <param name="cornerRadius"></param>
        /// <param name="corners"></param>
        /// <param name="icon"></param>
        /// <param name="iconImg"></param>
        /// <param name="text"></param>
        /// <param name="backColor1"></param>
        /// <param name="backColor2"></param>
        /// <param name="borderColor"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, int? opacity, int? cornerRadius, GraphicCorners? corners, string icon, OImage iconImg, string text, Color? backColor1, Color? backColor2, Color? borderColor)
        {
            return PreConfigLayout_BasicStyle(name, left, top, width, height, opacity, cornerRadius, corners, icon, iconImg, text, backColor1, backColor2, borderColor, null, null);
        }

        /// <summary>
        /// Creates a button with a default layout
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="opacity"></param>
        /// <param name="cornerRadius"></param>
        /// <param name="corners"></param>
        /// <param name="icon"></param>
        /// <param name="iconImg"></param>
        /// <param name="text"></param>
        /// <param name="backColor1"></param>
        /// <param name="backColor2"></param>
        /// <param name="borderColor"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static OMButton PreConfigLayout_BasicStyle(string name, int left, int top, int width, int height, int? opacity, int? cornerRadius, GraphicCorners? corners, string icon, OImage iconImg, string text, Color? backColor1, Color? backColor2, Color? borderColor, string fontName, int? fontSize, Color? textColor = null)
        {
            OMButton btn = new OMButton(name, left, top, width, height);

            // Create button graphics
            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            if (backColor1 != null)
                gd.BackgroundColor1 = (Color)backColor1;
            else
                gd.BackgroundColor1 = Color.Black; //Color.FromArgb(178,Color.Black);
            if (backColor2 != null)
                gd.BackgroundColor2 = (Color)backColor2;
            if (borderColor != null)
                gd.BorderColor = (Color)borderColor;
            else
                gd.BorderColor = Color.FromArgb(255, 87, 87, 87);//Color.FromArgb(128, Color.White);
            gd.Width = btn.Width;
            gd.Height = btn.Height;
            if (cornerRadius != null)
                gd.CornerRadius = cornerRadius;
            if (corners != null)
                gd.CornersRadiusAppliesTo = (GraphicCorners)corners;
            if (icon != null)
                gd.Icon = icon;
            
            // Default font data            
            Font f = OpenMobile.Graphics.Font.Arial;
            f.Size = 36;
            if (!String.IsNullOrEmpty(fontName))
                f = new Font(fontName, 36);
            if (fontSize.HasValue)
                f.Size = fontSize.Value;
            gd.TextFont = f;

            if (textColor.HasValue)
                gd.TextColor = textColor.Value;

            if (text != null)
                gd.Text = text;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            btn.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            btn.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            //gd.Opacity = 150;
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonForegroundFocused;
            //btn.checkedImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            if (opacity != null)
                gd.Opacity = opacity;
            else
                gd.Opacity = null;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackground;
            btn.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            // Set overlay image
            if (!String.IsNullOrEmpty(gd.Icon) || !String.IsNullOrEmpty(gd.Text))
            {
                gd.Opacity = 255;
                gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonForeground;
                btn.OverlayImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            }
            else if (iconImg != null)
            {
                btn.OverlayImage = new imageItem(iconImg);
            }

            return btn;
        }

        #endregion

        /// <summary>
        /// Mode Changed (Highlighted, Clicked, etc)
        /// </summary>
        public event ModeChange OnModeChange;
        /// <summary>
        /// Occurs when the control is clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Occurs when the control is held
        /// </summary>
        public event userInteraction OnHoldClick;
        /// <summary>
        /// Occurs when the control is long clicked
        /// </summary>
        public event userInteraction OnLongClick;
        /// <summary>
        /// Highlighted Image
        /// </summary>
        protected imageItem focusImage;
        /// <summary>
        /// Button Image
        /// </summary>
        protected imageItem image;
        /// <summary>
        /// Clicked Image
        /// </summary>
        protected imageItem downImage;
        /// <summary>
        /// Checked Image
        /// </summary>
        protected imageItem checkedImage;
        /// <summary>
        /// Opacity
        /// </summary>
        protected byte transparency = 100;
        /// <summary>
        /// Button Orientation
        /// </summary>
        protected eAngle orientation;
        /// <summary>
        /// Button Clicked Transition
        /// </summary>
        protected eButtonTransition transition = eButtonTransition.BoxOut;

        /// <summary>
        /// The checked state of the button
        /// </summary>
        public bool Checked
        {
            get
            {
                return this._Checked;
            }
            set
            {
                if (this._Checked != value)
                {
                    this._Checked = value;
                }
            }
        }
        private bool _Checked;

        /// <summary>
        /// The command to execute when clicked
        /// <para>NB! Only commands without parameters is supported</para>
        /// </summary>
        public string Command_Click
        {
            get
            {
                return this._Command_Click;
            }
            set
            {
                if (this._Command_Click != value)
                {
                    this._Command_Click = value;
                }
            }
        }
        private string _Command_Click;

        /// <summary>
        /// The command to execute when button is held (Command is not repeated, only executed once)
        /// <para>NB! Only commands without parameters is supported</para>
        /// </summary>
        public string Command_HoldClick
        {
            get
            {
                return this._Command_Holdclick;
            }
            set
            {
                if (this._Command_Holdclick != value)
                {
                    this._Command_Holdclick = value;
                }
            }
        }
        private string _Command_Holdclick;

        /// <summary>
        /// The command to execute when button is long clicked
        /// <para>NB! Only commands without parameters is supported</para>
        /// </summary>
        public string Command_LongClick
        {
            get
            {
                return this._Command_LongClick;
            }
            set
            {
                if (this._Command_LongClick != value)
                {
                    this._Command_LongClick = value;
                }
            }
        }
        private string _Command_LongClick;

        /// <summary>
        /// Draw modes for the button graphics
        /// </summary>
        public enum DrawModes
        {
            /// <summary>
            /// Scale graphics to match control size (Default)
            /// </summary>
            Scale,
            /// <summary>
            /// Uses a fixed size to draw the graphics centered (size is specified via GraphicSize property)
            /// </summary>
            FixedSizeCentered, 
            /// <summary>
            /// Uses a fixed size to draw the graphics aligned on the right side of the button, extending to the left (size is specified via GraphicSize property)
            /// </summary>
            FixedSizeRight,
            /// <summary>
            /// Uses a fixed size to draw the graphics aligned on the left side of the button, extending to the right (size is specified via GraphicSize property)
            /// </summary>
            FixedSizeLeft
        }
        /// <summary>
        /// The draw mode to use for the button graphics (sets the property for all images)
        /// </summary>
        public DrawModes GraphicDrawMode
        {
            get
            {
                return this._ImageDrawMode;
            }
            set
            {
                this._ImageDrawMode = value;
                this._FocusImageDrawMode = value;
                this._DownImageDrawMode = value;
                this._OverlayImageDrawMode = value;
            }
        }

        /// <summary>
        /// The draw mode to use for the button image
        /// </summary>
        public DrawModes ImageDrawMode
        {
            get
            {
                return this._ImageDrawMode;
            }
            set
            {
                if (this._ImageDrawMode != value)
                {
                    this._ImageDrawMode = value;
                }
            }
        }
        private DrawModes _ImageDrawMode;

        /// <summary>
        /// The draw mode to use for the focused state image
        /// </summary>
        public DrawModes FocusImageDrawMode
        {
            get
            {
                return this._FocusImageDrawMode;
            }
            set
            {
                if (this._FocusImageDrawMode != value)
                {
                    this._FocusImageDrawMode = value;
                }
            }
        }
        private DrawModes _FocusImageDrawMode;

        /// <summary>
        /// The draw mode to use for the down state image
        /// </summary>
        public DrawModes DownImageDrawMode
        {
            get
            {
                return this._DownImageDrawMode;
            }
            set
            {
                if (this._DownImageDrawMode != value)
                {
                    this._DownImageDrawMode = value;
                }
            }
        }
        private DrawModes _DownImageDrawMode;

        /// <summary>
        /// The draw mode to use for the overlay image
        /// </summary>
        public DrawModes OverlayImageDrawMode
        {
            get
            {
                return this._OverlayImageDrawMode;
            }
            set
            {
                if (this._OverlayImageDrawMode != value)
                {
                    this._OverlayImageDrawMode = value;
                }
            }
        }
        private DrawModes _OverlayImageDrawMode;

        /// <summary>
        /// The draw mode to use for the checked image
        /// </summary>
        public DrawModes CheckedImageDrawMode
        {
            get
            {
                return this._CheckedImageDrawMode;
            }
            set
            {
                if (this._CheckedImageDrawMode != value)
                {
                    this._CheckedImageDrawMode = value;
                }
            }
        }
        private DrawModes _CheckedImageDrawMode;

        /// <summary>
        /// The size of the graphics for the button
        /// </summary>
        public Size? GraphicSize
        {
            get
            {
                return this._GraphicSize;
            }
            set
            {
                if (this._GraphicSize != value)
                {
                    this._GraphicSize = value;
                }
            }
        }
        private Size? _GraphicSize;        

        /// <summary>
        /// The rendering mode of the control
        /// </summary>
        [Browsable(false)]
        public override eModeType Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                if (OnModeChange != null)
                {
                    int screen = this.containingScreen();
                    if ((screen >= 0) && (IsControlRenderable()))
                        OnModeChange(this, screen, mode);
                }
            }
        }

        public void RemoveAllEventsSubscriptions()
        {
            OnClick = null;
            OnHoldClick = null;
            OnLongClick = null;
        }

        /// <summary>
        /// Fires the buttons OnClick event
        /// </summary>
        public void clickMe(int screen)
        {
            // Execute attached command
            if (!String.IsNullOrEmpty(_Command_Click))
                base.Command_Execute(_Command_Click);

            if (OnClick != null)
                OnClick(this, screen);
            raiseUpdate(false);
        }

        /// <summary>
        /// Hold the control 
        /// </summary>
        public void holdClickMe(int screen)
        {
            // Execute attached command
            if (!String.IsNullOrEmpty(_Command_Holdclick))
            {
                // Check for loop command parameter present {:L:}
                if (_Command_Holdclick.Contains(OpenMobile.Data.DataSource.DataTag_Loop))
                {   // Present, replace with screen reference
                    string cmd = _Command_Holdclick.Replace(OpenMobile.Data.DataSource.DataTag_Loop, String.Empty);
                    while (this.Mode == eModeType.Clicked)
                    {
                        base.Command_Execute(cmd);
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                {
                    base.Command_Execute(_Command_Holdclick);
                }
            }

            if (OnHoldClick != null)
                OnHoldClick(this, screen);
            raiseUpdate(false);
        }

        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
            // Execute attached command
            if (!String.IsNullOrEmpty(_Command_LongClick))
                base.Command_Execute(_Command_LongClick);

            if (OnLongClick != null)
                OnLongClick(this, screen);
            raiseUpdate(false);
        }

        /// <summary>
        /// Sets the button image
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
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Sets the effect when the button is clicked
        /// </summary>
        public eButtonTransition Transition
        {
            get
            {
                return transition;
            }
            set
            {
                transition = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Sets the image displayed when the button is pressed
        /// </summary>
        public imageItem DownImage
        {
            get
            {
                return downImage;
            }
            set
            {
                downImage = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Sets the image displayed when the button is checked
        /// </summary>
        public imageItem CheckedImage
        {
            get
            {
                return checkedImage;
            }
            set
            {
                checkedImage = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }


        #region OverlayImage

        private imageItem _OverlayImage;
        /// <summary>
        /// Sets the image that's rendered on top of all other images (can be used instead of the text property)
        /// </summary>
        public imageItem OverlayImage
        {
            get
            {
                return _OverlayImage;
            }
            set
            {
                _OverlayImage = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        #endregion

        ///// <summary>
        ///// An integer from 0-100 (100% being opaque)
        ///// </summary>
        //public byte Transparency
        //{
        //    get
        //    {
        //        return transparency;
        //    }
        //    set
        //    {
        //        transparency = value;
        //        _RefreshGraphic = true;
        //        raiseUpdate(false);
        //    }
        //}

        /// <summary>
        /// Sets the image to display when the button has focus
        /// </summary>
        public imageItem FocusImage
        {
            get
            {
                return focusImage;
            }
            set
            {
                focusImage = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Sets the rotation of the control
        /// </summary>
        public eAngle Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        #region Shape properties

        /// <summary>
        /// The shape to draw
        /// </summary>
        protected shapes shape = shapes.None;
        /// <summary>
        /// The fill color of the Shape
        /// </summary>
        protected Color fillColor;
        /// <summary>
        /// The width in pixels of a border (0 for no border)
        /// </summary>
        protected float borderSize = 0;
        /// <summary>
        /// The color of the border
        /// </summary>
        protected Color borderColor;
        /// <summary>
        /// The shape to draw
        /// </summary>
        public shapes Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;
                _RefreshGraphic = true;
                //genTriangle();
            }
        }
        //Point[] triPoint = new Point[0];
        //private void genTriangle()
        //{
        //    if (shape == shapes.Triangle)
        //    {
        //        triPoint = new Point[] { new Point(left, top + height), new Point(left + width, top + height), new Point(left + (width / 2), top) };
        //    }
        //}
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
                _RefreshGraphic = true;
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
                _RefreshGraphic = true;
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
                _RefreshGraphic = true;
            }
        }

        /// <summary>
        /// Sets the corner radius of a rounded rectangle
        /// </summary>
        protected int cornerRadius = 10;
        /// <summary>
        /// Sets the corner radius of a rounded rectangle
        /// </summary>
        public int CornerRadius
        {
            get { return cornerRadius; }
            set 
            { 
                cornerRadius = value;
                _RefreshGraphic = true;
            }
        }

        #endregion


        /// <summary>
        /// Creates a button with the default size and position
        /// <para>[Obsolete] Use OMButton(string name, int x, int y, int w, int h) instead</para>
        /// </summary>
        [Obsolete("Use OMButton(string name, int x, int y, int w, int h) instead")]
        public OMButton()
            : base("", 20, 20, 300, 120)
        {
            Init();
        }
        /// <summary>
        /// Creates a new OMButton
        /// <para>[Obsolete] Use OMButton(string name, int x, int y, int w, int h) instead</para>
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        [Obsolete("Use OMButton(string name, int x, int y, int w, int h) instead")]
        public OMButton(int x, int y)
            : base("", x, y, 300, 120)
        {
            Init();
        }
        /// <summary>
        /// Initializes a button with a starting location and size
        /// <para>[Obsolete] Use OMButton(string name, int x, int y, int w, int h) instead</para>
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        [Obsolete("Use OMButton(string name, int x, int y, int w, int h) instead")]
        public OMButton(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
            Init();
        }
        /// <summary>
        /// Initializes a button with a starting location and size
        /// </summary>
        /// <param name="name">Name of control</param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMButton(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            Init();
        }

        private void Init()
        {
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
        }

        /// <summary>
        /// Controls where the text is drawn on the control relative to the control itself
        /// </summary>
        public Point TextOffset { get; set; }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            int transitionTop = 0; // e.transitionTop;
            if (Width == 0)
                return;

            // Render background (if any)
            if (background != Color.Transparent)
            {
                using (Brush Fill = new Brush(Color.FromArgb((int)(this.GetAlphaValue255(background.A)), background)))
                {
                    g.FillRectangle(Fill, left, top, width, height);
                }
            }

            // Draw button state
            switch (this.Mode)
            {
                case eModeType.Highlighted:
                    {
                        if (focusImage.image != null)
                        {   // Draw focused image
                            DrawImage(_FocusImageDrawMode, g, focusImage.image, this.Left, this.Top, this.Width, this.Height, _RenderingValue_Alpha, orientation);
                        }
                        else if (image.image != null)
                        {   // Draw regular image
                            DrawImage(_ImageDrawMode, g, image.image, this.Left, this.Top, this.Width, this.Height, _RenderingValue_Alpha, orientation);
                        }
                        else 
                        {   // Default fallback if image is missing
                            //DrawShape(g, e, _RenderingValue_Alpha);
                            base.DrawShape(g, e);
                        }
                    }
                    break;
                case eModeType.Clicked:
                case eModeType.ClickedAndTransitioningOut:
                    {
                        //alpha *= e.transparency;
                        if (downImage.image != null)
                        {   // Draw down state (clicked state)
                            DrawImage(_DownImageDrawMode, g, downImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), _RenderingValue_Alpha, orientation);
                        }
                        else if (focusImage.image != null)
                        {   // Draw focus image
                            DrawImage(_FocusImageDrawMode, g, focusImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), _RenderingValue_Alpha, orientation);
                        }
                        else if (image.image != null)
                        {   // Draw regular image
                            DrawImage(_ImageDrawMode, g, image.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), _RenderingValue_Alpha, orientation);
                        }
                        else
                        {   // Default fallback if image is missing
                            //DrawShape(g, e, _RenderingValue_Alpha);
                            base.DrawShape(g, e);
                        }
                    }
                    break;
                default:
                    {
                        if (image.image != null)
                        {   // Draw regular image
                            DrawImage(_ImageDrawMode, g, image.image, left, top, width, height, _RenderingValue_Alpha, orientation);
                        }
                        else
                        {   // Default fallback if image is missing
                            //DrawShape(g, e, _RenderingValue_Alpha);
                            base.DrawShape(g, e);
                        }

                        // Render checked state
                        if (_Checked && checkedImage.image != null)
                        {   // Draw down state (clicked state) if button is set to checked
                            DrawImage(_CheckedImageDrawMode, g, checkedImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), _RenderingValue_Alpha, orientation);
                        }
                        else if (_Checked && downImage.image != null)
                        {   // Draw down state (clicked state) if button is set to checked
                            DrawImage(_DownImageDrawMode, g, downImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), _RenderingValue_Alpha, orientation);
                        }
                    }
                    break;
            }

            // Draw overlay image
            if (_OverlayImage.image != null)
            {
                DrawImage(_OverlayImageDrawMode, g, _OverlayImage.image, left, top, width, height, _RenderingValue_Alpha, orientation);
            }

            // Draw text (if any)
            if (_text != "")
            {
                if (_RefreshGraphic)
                    textTexture = g.GenerateTextTexture(textTexture, this.Left, this.Top, this.width, this.height, _text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                g.DrawImage(textTexture, (this.Left + TextOffset.X), (this.Top + TextOffset.Y), width, height, _RenderingValue_Alpha, orientation);
            }

            base.RenderFinish(g, e);
        }

        private void DrawImage(DrawModes drawMode, Graphics.Graphics g, OImage image, int x, int y, int w, int h, float transparency, eAngle orientation)
        {
            switch (drawMode)
            {
                case DrawModes.FixedSizeCentered:
                    {
                        #region Centered

                        Point Offset = new Point((this.Region.Width / 2) - (image.Width / 2), (this.Region.Height / 2) - (image.Height / 2));
                        Rectangle imgRect = new Rectangle(x, y, w, h);
                        if (_GraphicSize == null)
                        {
                            imgRect.Width = image.Width;
                            imgRect.Height = image.Height;
                        }
                        else
                        {
                            Size s = (Size)_GraphicSize;
                            imgRect.Width = s.Width;
                            imgRect.Height = s.Height;
                        }
                        imgRect.Translate(Offset);
                        g.DrawImage(image, imgRect.Left, imgRect.Top, imgRect.Width, imgRect.Height, transparency, orientation);

                        #endregion
                    }
                    break;
                case DrawModes.FixedSizeRight:
                    {
                        #region Right

                        Rectangle imgRect = new Rectangle(x, y, w, h);
                        if (_GraphicSize == null)
                        {
                            imgRect.Width = image.Width;
                            imgRect.Height = image.Height;
                        }
                        else
                        {
                            Size s = (Size)_GraphicSize;
                            imgRect.Width = s.Width;
                            imgRect.Height = s.Height;
                        }
                        Point Offset = new Point(this.Region.Right - imgRect.Right, (this.Region.Height / 2) - (image.Height / 2));
                        imgRect.Translate(Offset);
                        g.DrawImage(image, imgRect.Left, imgRect.Top, imgRect.Width, imgRect.Height, transparency, orientation);

                        #endregion
                    }
                    break;
                case DrawModes.FixedSizeLeft:
                    {
                        #region Left

                        Rectangle imgRect = new Rectangle(x, y, w, h);
                        if (_GraphicSize == null)
                        {
                            imgRect.Width = image.Width;
                            imgRect.Height = image.Height;
                        }
                        else
                        {
                            Size s = (Size)_GraphicSize;
                            imgRect.Width = s.Width;
                            imgRect.Height = s.Height;
                        }
                        Point Offset = new Point(imgRect.Left - this.Region.Left, (this.Region.Height / 2) - (image.Height / 2));
                        imgRect.Translate(Offset);
                        g.DrawImage(image, imgRect.Left, imgRect.Top, imgRect.Width, imgRect.Height, transparency, orientation);

                        #endregion
                    }
                    break;
                default:
                    {
                        g.DrawImage(image, x, y, w, h, transparency, orientation);
                    }
                    break;
            }
        }

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <param name="ClearEvents"></param>
        /// <returns></returns>
        public object Clone(bool ClearEvents)
        {
            OMButton btn = (OMButton)base.Clone();
            btn.OnClick = null;
            btn.OnLongClick = null;
            btn.OnModeChange = null;
            return btn;
        }

        /// <summary>
        /// DataSource for the Image property
        /// </summary>
        public string DataSource_Image
        {
            get
            {
                return this._DataSource_Image;
            }
            set
            {
                this._DataSource_Image = value;
                DataSource_RegisterProperty("Image", this._DataSource_Image);
            }
        }
        private string _DataSource_Image;

        /// <summary>
        /// DataSource for the DownImage property
        /// </summary>
        public string DataSource_DownImage
        {
            get
            {
                return this._DataSource_DownImage;
            }
            set
            {
                this._DataSource_DownImage = value;
                DataSource_RegisterProperty("DownImage", this._DataSource_DownImage);
            }
        }
        private string _DataSource_DownImage;

        /// <summary>
        /// DataSource for the FocusImage property
        /// </summary>
        public string DataSource_FocusImage
        {
            get
            {
                return this._DataSource_FocusImage;
            }
            set
            {
                this._DataSource_FocusImage = value;
                DataSource_RegisterProperty("FocusImage", this._DataSource_FocusImage);
            }
        }
        private string _DataSource_FocusImage;

        /// <summary>
        /// DataSource for the OverlayImage property
        /// </summary>
        public string DataSource_OverlayImage
        {
            get
            {
                return this._DataSource_OverlayImage;
            }
            set
            {
                this._DataSource_OverlayImage = value;
                DataSource_RegisterProperty("OverlayImage", this._DataSource_OverlayImage);
            }
        }
        private string _DataSource_OverlayImage;

        /// <summary>
        /// DataSource for the CheckedImage property
        /// </summary>
        public string DataSource_CheckedImage
        {
            get
            {
                return this._DataSource_CheckedImage;
            }
            set
            {
                this._DataSource_CheckedImage = value;
                DataSource_RegisterProperty("CheckedImage", this._DataSource_CheckedImage);
            }
        }
        private string _DataSource_CheckedImage;

        internal override void DataSource_MultiProperty_OnChanged(string propertyName, Data.DataSource dataSource)
        {
            base.DataSource_MultiProperty_OnChanged(propertyName, dataSource);

            switch (propertyName)
            {
                case "Image":
                    this.Image = dataSource.GetValueAsImageItem();
                    break;
                case "DownImage":
                    this.DownImage = dataSource.GetValueAsImageItem();
                    break;
                case "FocusImage":
                    this.FocusImage = dataSource.GetValueAsImageItem();
                    break;
                case "OverlayImage":
                    this.OverlayImage = dataSource.GetValueAsImageItem();
                    break;
                case "CheckedImage":
                    this.CheckedImage = dataSource.GetValueAsImageItem();
                    break;
                default:
                    break;
            }
        }
        
    }
}
