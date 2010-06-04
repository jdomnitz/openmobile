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
using System.Drawing;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A textbox control
    /// </summary>
    public class OMTextBox : OMLabel, IClickable, IHighlightable
    {
        protected textboxFlags flags;
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
            try
            {
                OnClick(this, screen);
            }
            catch (Exception) { };//If no one has hooked the click event
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
                if ((flags&textboxFlags.NumericOnly)==textboxFlags.NumericOnly)
                {
                    double tmp;
                    if (double.TryParse(value,out tmp)==false)
                        return;
                }
                if ((flags&textboxFlags.AlphabeticalOnly)==textboxFlags.AlphabeticalOnly)
                {
                    if (IsAlphabetic(value) == false)
                        return;
                }
                text = value;
                this.refreshMe(this.toRegion());
                try
                {
                    if (OnTextChange!=null)
                        OnTextChange(this, this.containingScreen());
                }
                catch (Exception) { };
            }
        }

        private bool IsAlphabetic(string strToCheck)
        {
            foreach (int chr in strToCheck)
                if ((chr < 0x41 || chr > 0x5A && chr < 0x61 || chr > 0x7A)&&(chr!=30))
                    return false;
            return true;
        }

        /// <summary>
        /// Text formatting arguments
        /// </summary>
        [Browsable(false)]
        public override eTextFormat Format
        {
            get { return eTextFormat.Normal; }
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
            this.TextAlignment = Alignment.CenterCenter;
            this.Format = eTextFormat.Normal;
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
            this.TextAlignment = Alignment.CenterCenter;
            this.Format = eTextFormat.Normal;
            this.Color = Color.Black;
            this.OutlineColor = Color.Blue;
        }
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics g, renderingParams e)
        {
            float tmp = 1;
            if (this.Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            Rectangle r = new Rectangle(this.Left, this.Top, this.Width, this.Height);
            
            Renderer.FillRoundRectangle(g,new SolidBrush(Color.FromArgb((int)(tmp * 255), background)), r,10F);
            Renderer.DrawRoundRectangle(g,new Pen(Color.FromArgb((int)(tmp * 255), this.Color), 2F), r,10F);
            
            if (this.Mode == eModeType.Highlighted)
            {
                Rectangle r2 = new Rectangle(r.Left + 1, r.Top + 1, r.Width - 2, r.Height - 2);
                Renderer.DrawRoundRectangle(g,new Pen(Color.FromArgb((int)(40 * tmp), this.OutlineColor), 4F), r2,10F);
                Renderer.DrawRoundRectangle(g, new Pen(Color.FromArgb((int)(75 * tmp), this.OutlineColor), 3F), r2, 10F);
                Renderer.DrawRoundRectangle(g, new Pen(Color.FromArgb((int)(120 * tmp), this.OutlineColor), 2F), r2, 10F);
                Renderer.DrawRoundRectangle(g, new Pen(Color.FromArgb((int)(200 * tmp), this.OutlineColor), 1F), r2, 10F);
            }
            using (StringFormat f = new StringFormat(StringFormatFlags.NoWrap))
            {
                f.Alignment=StringAlignment.Center;
                f.LineAlignment=StringAlignment.Center;
                if ((flags & textboxFlags.EllipsisCenter) == textboxFlags.EllipsisCenter)
                    f.Trimming = StringTrimming.EllipsisPath;
                else if ((flags & textboxFlags.EllipsisEnd) == textboxFlags.EllipsisEnd)
                    f.Trimming = StringTrimming.EllipsisCharacter;
                else if ((flags & textboxFlags.TrimNearestWord) == textboxFlags.TrimNearestWord)
                    f.Trimming = StringTrimming.Word;
                if (text!=null)
                    g.DrawString((this.flags&textboxFlags.Password)==textboxFlags.Password ? new String('*',text.Length): text, this.Font, new SolidBrush(Color.FromArgb((int)(tmp * Color.A), this.Color)), new RectangleF(this.Left, this.Top, this.Width + 5, this.Height),f);
            }
        }
    }
}