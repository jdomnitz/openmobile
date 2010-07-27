using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using OpenMobile.Graphics;
using OpenMobile;

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
        /// Button double clicked
        /// </summary>
        public event userInteraction OnDoubleClick;

        private imageItem focusImage;
        private imageItem image;
        private imageItem downImage;
        private int transparency = 100;
        private eAngle orientation;
        private eButtonTransition transition = eButtonTransition.BoxOut;

        /// <summary>
        /// Show the buttons area as an yellow overlay(debug usage only) - (Added by Borte)
        /// </summary>
        public bool ShowArea = false;

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
        /// Returns the type of control
        /// </summary>
        public static new string TypeName
        {
            get
            {
                return "Button";
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
        /// Fires the OnDoubleClick Event
        /// </summary>
        public void doubleClickMe(int screen)
        {
            try
            {
                if (OnDoubleClick != null)
                    OnDoubleClick(this, screen);
            }
            catch (Exception) { };//If no one has hooked the double click event
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
            try
            {
                if (OnLongClick != null)
                    OnLongClick(this, screen);
            }
            catch (Exception) { };
        }

        /// <summary>
        /// Sets the button image
        /// </summary>
        [Category("Graphical"), Description("Sets the button image")]
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
                refreshMe(this.toRegion());
            }
        }

        private void update(object sender, EventArgs e)
        {
            refreshMe(this.toRegion());
        }

        /// <summary>
        /// Sets the effect when the button is clicked
        /// </summary>
        [CategoryAttribute("Graphical"), DescriptionAttribute("Sets the effect when the button is clicked")]
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
        [Category("Graphical"), Description("Sets the image displayed when the button is pressed")]
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
        [Category("Graphical"), Description("A value from 0-100 (100% being opaque)")]
        public int Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                if (value != transparency)
                    refreshMe(this.toRegion());
                transparency = value;
            }
        }

        /// <summary>
        /// Sets the image to display when the button has focus
        /// </summary>
        [Category("Graphical"), Description("Sets the image to display when the button has focus")]
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
        [CategoryAttribute("Graphical"), DescriptionAttribute("Sets the rotation of the control")]
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
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                tmp = e.globalTransitionOut;
            alpha = tmp;
            alpha *= ((float)transparency / 100);
            if ((this.Mode == eModeType.Highlighted) && (this.FocusImage.image != null))
            {
                g.DrawImage(focusImage.image, new Rectangle(this.Left, this.Top, this.Width, this.Height), alpha,orientation);
                if (textTexture == null)
                    textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), tmp);
                return;
            }
            else if ((this.Mode == eModeType.Clicked) || (this.Mode == eModeType.ClickedAndTransitioningOut))
            {
                if (focusImage.image != null)
                {
                    g.DrawImage(focusImage.image, new Rectangle(this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5)), alpha,orientation);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                    g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), tmp);
                    return;
                }
                else if (downImage.image != null)
                {
                    g.DrawImage(downImage.image, new Rectangle(this.Left - transitionTop, this.Top - transitionTop, this.Width + (int)(transitionTop * 2.5), this.Height + (int)(transitionTop * 2.5)), alpha,orientation);
                    if (textTexture == null)
                        textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
                    g.DrawImage(textTexture, left - transitionTop, top - transitionTop, width + (int)(transitionTop * 2.5), height + (int)(transitionTop * 2.5), tmp);
                    return;
                }
            }
            if (image.image == null)
            {
                if (image == imageItem.MISSING)
                {
                    using (Pen p = new Pen(Color.White, 4F))
                        g.DrawRoundRectangle(p, new Rectangle(left + 2, top + 2, width - 4, height - 4), 8);
                }
            }
            else
            {
                g.DrawImage(image.image, new Rectangle(this.Left, this.Top, this.Width, this.Height), alpha,orientation);
            }

            // Debug function added by Borte
            if (ShowArea)
                g.FillRectangle(new Brush(Color.FromArgb(75, Color.Yellow)), this.toRegion());
            if (textTexture == null)
                textTexture = g.GenerateTextTexture(this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
            g.DrawImage(textTexture, left, top, width, height, tmp);
        }
    }
}
