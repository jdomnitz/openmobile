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
using OpenMobile;
using OpenMobile.helperFunctions;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A textbox control
    /// </summary>
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
                textTexture = null;
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

        private bool _OSKShowOnLongClick = false;
        /// <summary>
        /// Specifies when to show the OSK (false = on click event, true = on long click event)
        /// </summary>
        public bool OSKShowOnLongClick
        {
            get
            {
                return _OSKShowOnLongClick;
            }
            set
            {
                _OSKShowOnLongClick = value;
            }
        }

        private bool _OSKShowAfterClickEvent = false;
        /// <summary>
        /// Specifies when to show the OSK (false = before click event, true = after click event)
        /// </summary>
        public bool OSKShowAfterClickEvent
        {
            get
            {
                return _OSKShowAfterClickEvent;
            }
            set
            {
                _OSKShowAfterClickEvent = value;
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

        private bool _AutoFitText = true;
        /// <summary>
        /// Enabled or disabled automatic adjustment of font size to fit in control
        /// </summary>
        public bool AutoFitText 
        {
            get
            {
                return _AutoFitText;
            }
            set
            {
                _AutoFitText = value;
            }
        }

        /// <summary>
        /// Raised when the text value is changed
        /// </summary>
        public event userInteraction OnTextChange;
        /// <summary>
        /// Control Clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Control clicked and held
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
            if (!_OSKShowOnLongClick && !_OSKShowAfterClickEvent)
                ShowOSK(screen);

            if (OnClick != null)
                OnClick(this, screen);

            // Show OSK
            if (!_OSKShowOnLongClick && _OSKShowAfterClickEvent)
                ShowOSK(screen);
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
            if (_OSKShowOnLongClick && !_OSKShowAfterClickEvent)
                ShowOSK(screen);

            if (OnLongClick != null)
                OnLongClick(this, screen);

            // Show OSK
            if (_OSKShowOnLongClick && _OSKShowAfterClickEvent)
                ShowOSK(screen);
        }

        private void ShowOSK(int screen)
        {
            if (_OSKType != OSKInputTypes.None)
            {
                // Mask input?
                bool MaskInput = (flags & textboxFlags.Password) == textboxFlags.Password;

                // Show osk
                OSK osk = new OSK(this.Text, OSKHelpText, OSKDescription, _OSKType, MaskInput);

                // Trigg OSK shown event
                if (OnOSKShow != null)
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate() { OnOSKShow(this, screen); });                

                // Show OSK
                this.Text = osk.ShowOSK(screen);
            }
        }

        private int count;
        /// <summary>
        /// The text contained by the textbox
        /// </summary>
        public override string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text == value)
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
                if (((flags & textboxFlags.Password) == textboxFlags.Password) && (value != null) && ((text != "") || ((value.Length == 1))) && ((text != null) && (value.Contains(text))))
                    count = 6;
                textTexture = null;
                text = value;

                #region AutoFit

                // Autofit size of string
                if (fontScaleFactor != 0)
                {
                    this.font.Size = orgFontSize;
                    fontScaleFactor = 0;
                }
                if (AutoFitText)
                {
                    SizeF TextSize = Graphics.Graphics.MeasureString(text, this.Font, this.Format);

                    if (TextSize.Width > this.width)
                    {   // Reduce text size to fit
                        orgFontSize = this.font.Size;

                        fontScaleFactor = TextSize.Width / this.width;
                        this.font.Size = this.font.Size / fontScaleFactor;

                        // Reset text texture to regenerate text
                        textTexture = null;
                    }
                }

                #endregion

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
            this.OutlineColor = StoredData.SystemSettings.SkinFocusColor;
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            float tmp = OpacityFloat;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            else if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;

            g.FillRoundRectangle(new Brush(Color.FromArgb((int)(tmp * 255), (this.disabled ? disabledBackgroundColor : background))), left, top, width, height, 10);
            g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(tmp * 255), this.Color), 1F), left, top, width, height, 10);

            if (this.Mode == eModeType.Highlighted)
            {
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(40 * tmp), this.OutlineColor), 4F), left + 1, top + 1, width - 2, height - 2, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(75 * tmp), this.OutlineColor), 3F), left + 1, top + 1, width - 2, height - 2, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(120 * tmp), this.OutlineColor), 2F), left + 1, top + 1, width - 2, height - 2, 10);
            }
            if (text != null)
            {
                // Generate password masking characters if needed
                //TODO: Remove this masking 
                string tempStr = text;
                if (((this.flags & textboxFlags.Password) == textboxFlags.Password) && (text.Length > 0))
                {
                    tempStr = new String('*', text.Length - 1);
                    if (count > 1)
                        tempStr += text[text.Length - 1];
                    else
                        tempStr += "*";
                    count--;
                }

                if ((g.TextureGenerationRequired(textTexture)) || (count > 0))
                    textTexture = g.GenerateTextTexture(textTexture, left+5, top, width, height, tempStr, font, textFormat, textAlignment, color, outlineColor);
                g.DrawImage(textTexture, this.Left+5, this.Top, this.Width, this.Height, tmp);

                /*
                using (System.Drawing.StringFormat f = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoWrap))
                {
                    f.Alignment = System.Drawing.StringAlignment.Center;
                    f.LineAlignment = System.Drawing.StringAlignment.Center;
                    if ((flags & textboxFlags.EllipsisCenter) == textboxFlags.EllipsisCenter)
                        f.Trimming = System.Drawing.StringTrimming.EllipsisPath;
                    else if ((flags & textboxFlags.EllipsisEnd) == textboxFlags.EllipsisEnd)
                        f.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                    else if ((flags & textboxFlags.TrimNearestWord) == textboxFlags.TrimNearestWord)
                        f.Trimming = System.Drawing.StringTrimming.Word;
                 * 
                    // Generate password masking characters if needed
                    string tempStr = text;
                    if (((this.flags & textboxFlags.Password) == textboxFlags.Password) && (text.Length > 0))
                    {
                        tempStr = new String('*', text.Length - 1);
                        if (count > 1)
                            tempStr += text[text.Length - 1];
                        else
                            tempStr += "*";
                        count--;
                    }

                    if ((g.TextureGenerationRequired(textTexture)) || (count > 0))
                    {
                        textTexture = g.GenerateStringTexture(textTexture, tempStr, this.Font, Color.FromArgb((int)(tmp * Color.A), this.Color), this.Left, this.Top, this.Width + 5, this.Height, f);
                    }
                    g.DrawImage(textTexture, this.Left, this.Top, this.Width + 5, this.Height, tmp);
                }
                */
            }
            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Purple);

        }

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