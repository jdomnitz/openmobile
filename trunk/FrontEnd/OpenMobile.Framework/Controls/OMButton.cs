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
    public class OMButton : OMLabel, IClickable, IHighlightable
    {
        /// <summary>
        /// Mode Changed (Highlighted, Clicked, etc)
        /// </summary>
        public event ModeChange OnModeChange;
        /// <summary>
        /// Button Clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Button clicked and held
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
                    if ((screen >= 0) && (Visible))
                        OnModeChange(this, screen, mode);
                }
            }
        }

        /// <summary>
        /// Fires the buttons OnClick event
        /// </summary>
        public void clickMe(int screen)
        {
            if (OnClick != null)
                OnClick(this, screen);
        }

        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
            if (OnLongClick != null)
                OnLongClick(this, screen);
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
                raiseUpdate(false);
            }
        }

        #endregion

        /// <summary>
        /// An integer from 0-100 (100% being opaque)
        /// </summary>
        public byte Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                transparency = value;
                raiseUpdate(false);
            }
        }

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
                raiseUpdate(false);
            }
        }

        #region Shape properties

        /// <summary>
        /// The shape to draw
        /// </summary>
        protected shapes shape;
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
                genTriangle();
            }
        }
        Point[] triPoint = new Point[0];
        private void genTriangle()
        {
            if (shape == shapes.Triangle)
            {
                triPoint = new Point[] { new Point(left, top + height), new Point(left + width, top + height), new Point(left + (width / 2), top) };
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
        /// Sets the corner radius of a rounded rectangle
        /// </summary>
        protected int cornerRadius = 10;
        /// <summary>
        /// Sets the corner radius of a rounded rectangle
        /// </summary>
        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; }
        }

        #endregion


        /// <summary>
        /// Creates a button with the default size and position
        /// <para>[Obsolete] Use OMButton(string name, int x, int y, int w, int h) instead</para>
        /// </summary>
        [Obsolete("Use OMButton(string name, int x, int y, int w, int h) instead")]
        public OMButton()
        {
            Init("", 20, 20, 300, 120);
        }
        /// <summary>
        /// Creates a new OMButton
        /// <para>[Obsolete] Use OMButton(string name, int x, int y, int w, int h) instead</para>
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        [Obsolete("Use OMButton(string name, int x, int y, int w, int h) instead")]
        public OMButton(int x, int y)
        {
            Init("", x, y, 300, 120);
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
        {
            Init("", x, y, w, h);
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
        {
            Init(name, x, y, w, h);
        }

        private void Init(string Name, int x, int y, int w, int h)
        {
            this.Name = Name;
            this.Top = y;
            this.Left = x;
            this.Width = w;
            this.Height = h;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
        }

        /// <summary>
        /// Controls where the text is drawn on the control relative to the control itself
        /// </summary>
        public Point TextLocation { get; set; }

        Pen BorderPen;
        private void DrawShape(Graphics.Graphics g, renderingParams e, float Alpha)
        {
            Brush Fill = new Brush(Color.FromArgb((int)(Alpha * fillColor.A), fillColor));
            if (borderSize > 0)
                BorderPen = new Pen(Color.FromArgb((int)(Alpha * borderColor.A), borderColor), borderSize);

            switch (shape)
            {
                case shapes.Rectangle:
                    g.FillRectangle(Fill, left, top, width, height);
                    if (borderSize > 0)
                        g.DrawRectangle(BorderPen, left, top, width, height);
                    break;
                case shapes.Triangle:
                    g.FillPolygon(Fill, triPoint);
                    if (borderSize > 0)
                        g.DrawPolygon(BorderPen, triPoint);
                    break;
                case shapes.Oval:
                    g.FillEllipse(Fill, left, top, width, height);
                    if (borderSize > 0)
                        g.DrawEllipse(BorderPen, left, top, width, height);
                    break;
                case shapes.RoundedRectangle:
                    g.FillRoundRectangle(Fill, left, top, width, height, cornerRadius);
                    if (borderSize > 0)
                        g.DrawRoundRectangle(BorderPen, left, top, width, height, cornerRadius);
                    break;
            }
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            int transitionTop = e.transitionTop;
            if (Width == 0)
                return;
            float alpha = OpacityFloat;
            if (this.Mode == eModeType.transitioningIn)
                alpha = e.globalTransitionIn;
            else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                alpha = e.globalTransitionOut;
            alpha *= ((float)transparency / 100);

            // Draw button state
            switch (this.Mode)
            {
                case eModeType.Highlighted:
                    {
                        if (focusImage.image != null)
                        {   // Draw focused image
                            g.DrawImage(focusImage.image, this.Left, this.Top, this.Width, this.Height, alpha, orientation);
                        }
                        else if (image.image != null)
                        {   // Draw regular image
                            g.DrawImage(image.image, this.Left, this.Top, this.Width, this.Height, alpha, orientation);
                        }
                        else
                        {   // Default fallback if image is missing
                            //if (image == imageItem.MISSING)
                                //g.DrawRoundRectangle(new Pen(Color.White, 4F), left + 2, top + 2, width - 4, height - 4, 8);
                            DrawShape(g, e, alpha);
                        }
                    }
                    break;
                case eModeType.Clicked:
                case eModeType.ClickedAndTransitioningOut:
                    {
                        alpha *= e.transparency;
                        if (downImage.image != null)
                        {   // Draw down state (clicked state)
                            g.DrawImage(downImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), alpha, orientation);
                        }
                        else if (focusImage.image != null)
                        {   // Draw focus image
                            g.DrawImage(focusImage.image, this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5), alpha, orientation);
                        }
                        else if (image.image != null)
                        {   // Draw regular image
                            g.DrawImage(image.image, this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5), alpha, orientation);
                        }
                        else
                        {   // Default fallback if image is missing
                            //if (image == imageItem.MISSING)
                            //    g.DrawRoundRectangle(new Pen(Color.White, 4F), left + 2, top + 2, width - 4, height - 4, 8);
                            DrawShape(g, e, alpha);
                        }
                    }
                    break;
                default:
                    {
                        if (image.image != null)
                        {   // Draw regular image
                            g.DrawImage(image.image, left, top, width, height, alpha, orientation);
                        }
                        else
                        {   // Default fallback if image is missing
                            //if (image == imageItem.MISSING)
                            //    g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(255 * alpha), Color.White), 4F), left + 2, top + 2, width - 4, height - 4, 8);
                            DrawShape(g, e, alpha);
                        }
                    }
                    break;
            }

            // Draw overlay image
            if (_OverlayImage.image != null)
                g.DrawImage(_OverlayImage.image, left, top, width, height);

            // Draw text (if any)
            if (text != "")
            {
                if (g.TextureGenerationRequired(textTexture))
                    textTexture = g.GenerateTextTexture(textTexture, this.Left, this.Top, this.width, this.height, text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                g.DrawImage(textTexture, (this.Left + TextLocation.X), (this.Top + TextLocation.Y), width, height, alpha);
            }

            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Yellow);
        }

        public object Clone(bool ClearEvents)
        {
            OMButton btn = (OMButton)base.Clone();
            btn.OnClick = null;
            btn.OnLongClick = null;
            btn.OnModeChange = null;
            return btn;
        }
    }
}
