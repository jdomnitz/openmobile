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
            }
        }

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
            }
        }

        /// <summary>
        /// Creates a button with the default size and position
        /// </summary>
        public OMButton()
        {
            this.Top = 20;
            this.Left = 20;
            this.Width = 300;
            this.Height = 120;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
        }
        /// <summary>
        /// Creates a new OMButton
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        public OMButton(int x, int y)
        {
            this.Top = y;
            this.Left = x;
            this.Width = 300;
            this.Height = 120;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
        }
        /// <summary>
        /// Initializes a button with a starting location and size
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMButton(int x, int y, int w, int h)
        {
            this.Top = y;
            this.Left = x;
            this.Width = w;
            this.Height = h;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
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
            float alpha = 1;
            if (this.Mode == eModeType.transitioningIn)
                alpha = e.globalTransitionIn;
            else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                alpha = e.globalTransitionOut;
            alpha *= ((float)transparency / 100);
            if ((this.Mode == eModeType.Highlighted) && (this.FocusImage.image != null))
            {
                g.DrawImage(focusImage.image, this.Left, this.Top, this.Width, this.Height, alpha, orientation);
                if (textTexture == null)
                    textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), alpha);
                return;
            }
            else if ((this.Mode == eModeType.Clicked) || (this.Mode == eModeType.ClickedAndTransitioningOut))
            {
                alpha *= e.transparency;
                if (downImage.image != null)
                {
                    g.DrawImage(downImage.image, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), alpha, orientation);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                    g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), alpha);
                    return;
                }
                else if (focusImage.image != null)
                {
                    g.DrawImage(focusImage.image, this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5), alpha, orientation);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                    g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), alpha);
                    return;
                }
                else if (image.image == null)
                {
                    if (image == imageItem.MISSING)
                        g.DrawRoundRectangle(new Pen(Color.White, 4F), left + 2, top + 2, width - 4, height - 4, 8);
                }
                else
                {
                    g.DrawImage(image.image, this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5), alpha, orientation);
                }
            }
            if (image.image == null)
            {
                if (image == imageItem.MISSING)
                    g.DrawRoundRectangle(new Pen(Color.White, 4F), left + 2, top + 2, width - 4, height - 4, 8);
            }
            else
            {
                g.DrawImage(image.image, left, top, width, height, alpha, orientation);
            }

            if (textTexture == null)
                textTexture = g.GenerateTextTexture(left, top, width, height, text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
            g.DrawImage(textTexture, left, top, width, height, alpha);

            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Yellow);
        }
    }
}
