﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

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
                try
                {
                    if (ImageAnimator.CanAnimate(value.image) == true)
                        ImageAnimator.Animate(value.image, new EventHandler(update));
                }
                catch (InvalidOperationException) { }
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
                if (orientation != value)
                {
                    if (image.image == null)
                        return;
                    if ((orientation == eAngle.FlipHorizontal) || (value == eAngle.FlipHorizontal))
                    {
                        image.image = (Image)image.image.Clone();
                        image.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        if (focusImage.image != null)
                        {
                            focusImage.image = (Image)focusImage.image.Clone();
                            focusImage.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        if (downImage.image != null)
                        {
                            downImage.image = (Image)downImage.image.Clone();
                            downImage.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                    }
                    else if ((orientation == eAngle.FlipVertical) || (value == eAngle.FlipVertical))
                    {
                        image.image = (Image)image.image.Clone();
                        image.image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        if (focusImage.image != null)
                        {
                            focusImage.image = (Image)focusImage.image.Clone();
                            focusImage.image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        }
                        if (downImage.image != null)
                        {
                            downImage.image = (Image)downImage.image.Clone();
                            downImage.image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        }
                    }
                }
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
            this.TextAlignment = Alignment.CenterCenter;
            this.Format = eTextFormat.Normal;
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
            this.TextAlignment = Alignment.CenterCenter;
            this.Format = eTextFormat.Normal;
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
            this.TextAlignment = Alignment.CenterCenter;
            this.Format = eTextFormat.Normal;
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics g, renderingParams e)
        {
            if (Width == 0)
                return;
            using (ImageAttributes ia = new ImageAttributes())
            {
                ColorMatrix cm = new ColorMatrix();
                float tmp = 1;
                cm.Matrix00 = cm.Matrix33 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                if (this.Mode == eModeType.transitioningIn)
                    tmp = e.globalTransitionIn;
                if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                    tmp = e.globalTransitionOut;
                cm.Matrix33 = tmp;
                if ((this.Mode == eModeType.Highlighted) && (this.FocusImage.image != null))
                {
                    cm.Matrix33 *= ((float)transparency / 100);
                    ia.SetColorMatrix(cm);
                    g.DrawImage(focusImage.image, new Rectangle(this.Left, this.Top, this.Width, this.Height), 0, 0, focusImage.image.Width, focusImage.image.Height, GraphicsUnit.Pixel, ia);
                    if (this.Text != null)
                        Renderer.renderText(g, this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, tmp, 1, this.Color, this.OutlineColor);
                    cm = null;
                    return;
                }
                else if ((this.Mode == eModeType.Clicked) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                {
                    if (focusImage.image != null)
                    {
                        cm.Matrix33 *= e.transparency * ((float)transparency / 100);
                        ia.SetColorMatrix(cm);
                        g.DrawImage(focusImage.image, new Rectangle(this.Left - e.transitionTop, this.Top - e.transitionTop, this.Width + (int)(e.transitionTop * 2.5), this.Height + (int)(e.transitionTop * 2.5)), 0, 0, focusImage.image.Width, focusImage.image.Height, GraphicsUnit.Pixel, ia);
                        Renderer.renderText(g, this.Left - e.transitionTop, this.Top - e.transitionTop, this.Width + (int)(e.transitionTop * 2.5), this.Height + (int)(e.transitionTop * 2.5), this.Text, this.Font, this.Format, this.TextAlignment, e.transparency, this.Color, this.OutlineColor);
                        cm = null;
                        return;
                    }
                    else if (downImage.image != null)
                    {
                        cm.Matrix33 *= e.transparency * ((float)transparency / 100);
                        ia.SetColorMatrix(cm);
                        g.DrawImage(downImage.image, new Rectangle(this.Left - e.transitionTop, this.Top - e.transitionTop, this.Width + (int)(e.transitionTop * 2.5), this.Height + (int)(e.transitionTop * 2.5)), 0, 0, downImage.image.Width, downImage.image.Height, GraphicsUnit.Pixel, ia);
                        Renderer.renderText(g, this.Left - e.transitionTop, this.Top - e.transitionTop, this.Width + (int)(e.transitionTop * 2.5), this.Height + (int)(e.transitionTop * 2.5), this.Text, this.Font, this.Format, this.TextAlignment, e.transparency, this.Color, this.OutlineColor);
                        cm = null;
                        return;
                    }
                }
                cm.Matrix33 *= ((float)transparency / 100);
                ia.SetColorMatrix(cm);
                if (image.image == null)
                {
                    if (image == imageItem.MISSING)
                    {
                        using (Pen p = new Pen(Color.White, 4F))
                            Renderer.DrawRoundRectangle(g, p, new RectangleF(left + 2, top + 2, width - 4, height - 4), 8F);
                    }
                }
                else
                {
                    lock (image.image)
                    {
                        g.DrawImage(image.image, new Rectangle(this.Left, this.Top, this.Width, this.Height), 0, 0, image.image.Width, image.image.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                
                // Debug function added by Borte
                if (ShowArea)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(75, Color.Yellow)), this.toRegion());

                Renderer.renderText(g, this.Left, this.Top, this.Width, this.Height, this.Text, this.Font, this.Format, this.TextAlignment, tmp, 1, this.Color, this.OutlineColor);
                cm = null;
            }
        }
    }
}