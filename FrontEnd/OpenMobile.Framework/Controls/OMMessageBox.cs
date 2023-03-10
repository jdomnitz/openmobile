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
using System.Drawing.Drawing2D;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Provides a basic message box control
    /// </summary>
    [System.Serializable]
    public class OMMessageBox : OMLabel, IClickable
    {
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
        /// Back color 1
        /// </summary>
        protected Color backColor1 = Color.Blue;
        /// <summary>
        /// Back color 2 (gradient)
        /// </summary>
        protected Color backColor2 = Color.DarkBlue;
        /// <summary>
        /// Border color
        /// </summary>
        protected Color borderColor = Color.Black;
        /// <summary>
        /// Border width
        /// </summary>
        protected float borderWidth = 3F;
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
        /// Hold the checkbox 
        /// </summary>
        public void holdClickMe(int screen)
        {
            if (OnHoldClick != null)
                OnHoldClick(this, screen);
            raiseUpdate(false);
        }
        private string title;
        /// <summary>
        /// The message box title
        /// </summary>
        public string Title
        {
            set
            {
                if (title == value)
                    return;
                title = value;
                _RefreshGraphic = true;
            }
            get
            {
                return title;
            }
        }
        float letterHeight;
        /// <summary>
        /// The message text to display
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (_text == value)
                    return;
                base.Text = value;
                height = (int)Graphics.Graphics.MeasureString(_text, _font, _textFormat, _textAlignment, this.width - 1).Height + 1;
                
                letterHeight = Graphics.Graphics.MeasureString("A", Font).Height + 1;
            }
        }
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        public void longClickMe(int screen) { }

        /// <summary>
        /// Creates a new OMMessage box
        /// </summary>
        public OMMessageBox()
            : base("", 0, 0, 200, 200)
        {
            _textAlignment = OpenMobile.Graphics.Alignment.WordWrap;
        }
        /// <summary>
        /// Creates a new OMMessageBox
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMMessageBox(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
            _textAlignment = OpenMobile.Graphics.Alignment.WordWrap;
        }
        /// <summary>
        /// Creates a new OMMessageBox
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMMessageBox(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            _textAlignment = OpenMobile.Graphics.Alignment.WordWrap;
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            //float tmp = OpacityFloat;
            //if (this.Mode == eModeType.transitioningIn)
            //    tmp = e.globalTransitionIn;
            //if (this.Mode == eModeType.transitioningOut)
            //    tmp = e.globalTransitionOut;
            height += (int)letterHeight;
            Rectangle r = new Rectangle(this.Left, top, this.Width, height);
            g.FillRoundRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * 250), backColor1), Color.FromArgb((int)(_RenderingValue_Alpha * 250), backColor2), Gradient.Vertical), r, 20);
            g.DrawRoundRectangle(new Pen(borderColor, borderWidth), r, 20);
            if (_RefreshGraphic)
                textTexture = g.GenerateTextTexture(textTexture, this.Left, top, this.Width, (int)letterHeight, title, this.Font, this.Format, this.TextAlignment, this.Color, this.OutlineColor);
            g.DrawImage(textTexture, left, top, width, height, _RenderingValue_Alpha);
            top += (int)letterHeight;
            height -= (int)letterHeight;
            base.Render(g, e);
            //Renderer.renderLabel(g,this);  //ToDo-Fix this (pre-hardware acceleration merge)
            top -= (int)letterHeight;

            base.RenderFinish(g, e);
        }
    }
}
