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

namespace OpenMobile.Controls
{
    /// <summary>
    /// A textbox control
    /// </summary>
    public class OMTextBox : OMLabel, IClickable, IHighlightable
    {
        /// <summary>
        /// Sets various textbox options
        /// </summary>
        protected textboxFlags flags;
        /// <summary>
        /// The background color of the textbox
        /// </summary>
        protected Color background = Color.White;
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

        /// <summary>
        /// Raised when the text value is changed
        /// </summary>
        public event userInteraction OnTextChange;
        /// <summary>
        /// Button Clicked
        /// </summary>
        public event userInteraction OnClick;

        //Same properties as a label just rendered differently

        /// <summary>
        /// Controls the background color of the textbox
        /// </summary>
        [CategoryAttribute("General"), DescriptionAttribute("Sets the Background Color")]
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
        public void doubleClickMe(int screen) { }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen) { }
        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static new string TypeName
        {
            get
            {
                return "Textbox";
            }
        }
        private int count;
        /// <summary>
        /// The text contained by the textbox
        /// </summary>
        [CategoryAttribute("Text"), DescriptionAttribute("Sets the text to display")]
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
                if (((flags & textboxFlags.Password) == textboxFlags.Password)&&(value!=null)&&((text!="")||((value!=null)&&(value.Length==1)))&&((text!=null)&&(value.Contains(text))))
                    count = 6;
                textTexture = null;
                text = value;
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
        public OMTextBox()
        {
            this.Height = 32;
            this.Width = 100;
            this.Left = 25;
            this.Top = 25;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
            this.Color = Color.Black;
            this.OutlineColor = Color.Blue;
        }
        /// <summary>
        /// Create a new textbox with the given dimensions
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMTextBox(int x, int y, int w, int h)
        {
            this.Height = h;
            this.Width = w;
            this.Left = x;
            this.Top = y;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
            this.Color = Color.Black;
            this.OutlineColor = Color.Blue;
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            else if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;

            g.FillRoundRectangle(new Brush(Color.FromArgb((int)(tmp * 255), background)), left, top, width, height, 10);
            g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(tmp * 255), this.Color), 2F), left, top, width, height, 10);

            if (this.Mode == eModeType.Highlighted)
            {
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(40 * tmp), this.OutlineColor), 4F), left + 1, top + 1, width - 2, height - 2, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(75 * tmp), this.OutlineColor), 3F), left + 1, top + 1, width - 2, height - 2, 10);
                g.DrawRoundRectangle(new Pen(Color.FromArgb((int)(120 * tmp), this.OutlineColor), 2F), left + 1, top + 1, width - 2, height - 2, 10);
            }
            if (text != null)
            {
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
                    if ((textTexture == null)||(count>0))
                    {
                        string tempStr = text;
                        if (((this.flags & textboxFlags.Password) == textboxFlags.Password)&&(text.Length>0))
                        {
                            tempStr = new String('*', text.Length - 1);
                            if (count > 1)
                                tempStr += text[text.Length - 1];
                            else
                                tempStr += '*';
                            count--;
                        }
                        textTexture = g.GenerateStringTexture(tempStr, this.Font, Color.FromArgb((int)(tmp * Color.A), this.Color), this.Left, this.Top, this.Width + 5, this.Height, f);
                    }
                    g.DrawImage(textTexture, this.Left, this.Top, this.Width + 5, this.Height, tmp);
                }
            }
        }
    }
}