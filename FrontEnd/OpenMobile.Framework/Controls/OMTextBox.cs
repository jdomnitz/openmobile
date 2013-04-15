﻿/*********************************************************************************
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
using OpenMobile;
using OpenMobile.helperFunctions;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A textbox control
    /// </summary>
    [System.Serializable]
    public class OMTextBox : OMLabel, IClickable, IHighlightable
    {
        private float orgFontSize = 0;
        private float fontScaleFactor = 0;

        /// <summary>
        /// Sets various textbox options
        /// </summary>
        protected textboxFlags flags;
        /// <summary>
        /// The background color of the textbox
        /// </summary>
        protected Color background = Color.White;
        /// <summary>
        /// The disabled background color of the textbox
        /// </summary>
        protected Color disabledBackgroundColor = Color.Gray;
        /// <summary>
        /// Sets textbox option.  Note: multiple flags are allowed
        /// </summary>
        public textboxFlags Flags
        {
            get
            {
                return flags;
            }
            set
            {
                flags = value;
                _RefreshGraphic = true;
            }
        }

        #region OSK Properties

        private OSKInputTypes _OSKType = OSKInputTypes.None;
        /// <summary>
        /// Sets the osk input type used for this controls, 
        /// <para>if set to anything other than None it will show a osk when the control is cliked</para>
        /// </summary>
        public OSKInputTypes OSKType
        {
            get
            {
                return _OSKType;
            }
            set
            {
                _OSKType = value;
            }
        }

        private OSKShowTriggers _OSKShowTrigger = OSKShowTriggers.BeforeOnClickEvent;
        public OSKShowTriggers OSKShowTrigger
        {
            get
            {
                return _OSKShowTrigger;
            }
            set
            {
                _OSKShowTrigger = value;
            }
        }

        /// <summary>
        /// The help text that will be displayed in a empty text field (only shown if textbox value is "")
        /// </summary>
        public string OSKHelpText { get; set; }

        /// <summary>
        /// The description that will be shown in the OSK
        /// </summary>
        public string OSKDescription { get; set; }



        #endregion

        //private bool _AutoFitText = true;
        ///// <summary>
        ///// Enabled or disabled automatic adjustment of font size to fit in control
        ///// </summary>
        //public bool AutoFitText 
        //{
        //    get
        //    {
        //        return _AutoFitText;
        //    }
        //    set
        //    {
        //        _AutoFitText = value;
        //        _RefreshGraphic = true;
        //        raiseUpdate(false);
        //    }
        //}

        /// <summary>
        /// Raised when the text value is changed
        /// </summary>
        public event userInteraction OnTextChange;
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
        /// OSK Shown
        /// </summary>
        public event userInteraction OnOSKShow;

        //Same properties as a label just rendered differently

        /// <summary>
        /// Controls the background color of the textbox
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return background;
            }
            set
            {
                background = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Controls the background color of the textbox when the control is not enabled
        /// </summary>
        public Color DisabledBackgroundColor
        {
            get
            {
                return disabledBackgroundColor;
            }
            set
            {
                disabledBackgroundColor = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }


        /// <summary>
        /// Fires the buttons OnClick event
        /// </summary>
        public void clickMe(int screen)
        {
            // Don't fire events if this control is disabled
            if (this.disabled)
                return;

            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.BeforeOnClickEvent)
                ShowOSK(screen);

            if (OnClick != null)
                OnClick(this, screen);

            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.AfterOnClickEvent)
                ShowOSK(screen);

            raiseUpdate(false);
        }
        /// <summary>
        /// Hold the checkbox 
        /// </summary>
        public void holdClickMe(int screen)
        {
            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.BeforeOnHoldClickEvent)
                ShowOSK(screen);

            if (OnHoldClick != null)
                OnHoldClick(this, screen);

            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.AfterOnHoldClickEvent)
                ShowOSK(screen);

            raiseUpdate(false);
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen)
        {
            // Don't fire events if this control is disabled
            if (this.disabled)
                return;

            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.BeforeOnLongClickEvent)
                ShowOSK(screen);

            if (OnLongClick != null)
                OnLongClick(this, screen);

            // Show OSK
            if (_OSKShowTrigger == OSKShowTriggers.AfterOnLongClickEvent)
                ShowOSK(screen);

            raiseUpdate(false);
        }

        private void ShowOSK(int screen)
        {
            if (_OSKType != OSKInputTypes.None)
            {
                // Mask input?
                bool MaskInput = (flags & textboxFlags.Password) == textboxFlags.Password;

                // Show OSK
                this.Text = OSK.ShowDefaultOSK(screen, this.Text, OSKHelpText, OSKDescription, _OSKType, MaskInput);

                // Trigg OSK shown event
                if (OnOSKShow != null)
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate() { OnOSKShow(this, screen); });                
            }
        }

        /// <summary>
        /// The text contained by the textbox
        /// </summary>
        public override string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;
                //Pre-Screen
                if ((flags & textboxFlags.NumericOnly) == textboxFlags.NumericOnly)
                {
                    double tmp;
                    if ((value != "") && (double.TryParse(value, out tmp) == false))
                        return;
                }
                if ((flags & textboxFlags.AlphabeticalOnly) == textboxFlags.AlphabeticalOnly)
                {
                    if (IsAlphabetic(value) == false)
                        return;
                }
                _RefreshGraphic = true; 
                _text = value;

                //#region AutoFit

                //// Autofit size of string
                //if (fontScaleFactor != 0)
                //{
                //    this._font.Size = orgFontSize;
                //    fontScaleFactor = 0;
                //}
                //if (AutoFitText)
                //{
                //    SizeF TextSize = Graphics.Graphics.MeasureString(_text, this.Font, this.Format);

                //    if (TextSize.Width > this.width)
                //    {   // Reduce text size to fit
                //        orgFontSize = this._font.Size;

                //        fontScaleFactor = TextSize.Width / this.width;
                //        this._font.Size = this._font.Size / fontScaleFactor;

                //        // Reset text texture to regenerate text
                //        _RefreshGraphic = true;
                //    }
                //}

                //#endregion

                raiseUpdate(false);
                try
                {
                    if (OnTextChange != null)
                        OnTextChange(this, this.containingScreen());
                }
                catch (Exception) { };
            }
        }
        private static bool IsAlphabetic(string strToCheck)
        {
            foreach (int chr in strToCheck)
                if ((chr < 0x41 || chr > 0x5A && chr < 0x61 || chr > 0x7A) && (chr != 30))
                    return false;
            return true;
        }

        /// <summary>
        /// Text formatting arguments
        /// </summary>
        [Browsable(false)]
        public override OpenMobile.Graphics.eTextFormat Format
        {
            get { return OpenMobile.Graphics.eTextFormat.Normal; }
        }

        /// <summary>
        /// Create the control with the default size and location
        /// </summary>
        [Obsolete("Use OMTextBox(string name, int x, int y, int w, int h) instead")]
        public OMTextBox()
            : base("", 0, 0, 200, 200)
        {
            Init(name, 32, 100, 25, 25);
        }
        /// <summary>
        /// Create a new textbox with the given dimensions
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        [Obsolete("Use OMTextBox(string name, int x, int y, int w, int h) instead")]
        public OMTextBox(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
            Init("", x, y, w, h);
        }
        /// <summary>
        /// Create a new textbox
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMTextBox(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            Init(name, x, y, w, h);
        }
        private void Init(string Name, int x, int y, int w, int h)
        {
            this.Name = Name;
            this.Height = h;
            this.Width = w;
            this.Left = x;
            this.Top = y;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
            this.Color = Color.Black;
            this.OutlineColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            this.AutoFitTextMode = FitModes.Fit;
        }

        /// <summary>
        /// Render the controls background
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void Render_Background(Graphics.Graphics g, renderingParams e)
        {
            g.FillRoundRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * 255), (this.disabled ? disabledBackgroundColor : background))), left, top, width, height, 10);
            g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(_RenderingValue_Alpha * 255), this.Color), 1F), left, top, width, height, 10);

            if (this.Mode == eModeType.Highlighted)
            {
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(40 * _RenderingValue_Alpha), this.OutlineColor), 5F), left, top, width, height, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(75 * _RenderingValue_Alpha), this.OutlineColor), 4F), left, top, width, height, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(120 * _RenderingValue_Alpha), this.OutlineColor), 3F), left, top, width, height, 10);
            }
        }

        /// <summary>
        /// Render the controls foreground
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void Render_Foreground(Graphics.Graphics g, renderingParams e)
        {
            if (!String.IsNullOrEmpty(_text))
            {
                // Generate password masking characters if needed
                string tempStr = _text;
                if (((this.flags & textboxFlags.Password) == textboxFlags.Password) && (_text.Length > 0))
                {
                    tempStr = "";
                    tempStr = tempStr.PadRight(_text.Length);
                }

                // Render labels foreground
                base.Render_Foreground(g, e, tempStr);
            }
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            // Render labels background
            base.Render_Background(g, e);

            // Render this controls background
            Render_Background(g, e);

            // Render this controls foreground
            Render_Foreground(g, e);

            base.RenderFinish(g, e);
        }

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <param name="ClearEvents"></param>
        /// <returns></returns>
        public object Clone(bool ClearEvents)
        {
            OMTextBox TextBox = (OMTextBox)base.Clone();
            TextBox.OnClick = null;
            TextBox.OnOSKShow = null;
            TextBox.OnTextChange = null;
            return TextBox;
        }

    }
}