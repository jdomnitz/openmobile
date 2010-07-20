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
using System.Drawing.Drawing2D;
using OpenMobile.Graphics;
using OpenMobile;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Provides a basic message box control
    /// </summary>
    public class OMMessageBox:OMLabel,IClickable
    {
        /// <summary>
        /// Button Clicked
        /// </summary>
        public event userInteraction OnClick;
        protected Color backColor1=Color.Blue;
        protected Color backColor2=Color.DarkBlue;
        protected Color borderColor=Color.Black;
        protected float borderWidth=3F;
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
        private string title;
        public string Title
        {
            set
            {
                title = value;
            }
            get 
            {
                return title; 
            }
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
                return "Message Box";
            }
        }

        public OMMessageBox()
        {
            textAlignment = Alignment.WordWrap;
        }

        public OMMessageBox(int left,int top,int width,int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            textAlignment = Alignment.WordWrap;
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
            if (this.Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            height= (int)g.MeasureString(this.Text, Font, this.Width-1).Height+1;
            float letterHeight = g.MeasureString("A", Font).Height+1;
            height += (int)letterHeight;
            Rectangle r = new Rectangle(this.Left, top, this.Width, height);
            g.FillRoundRectangle(new LinearGradientBrush(r, Color.FromArgb((int)(tmp * 250), backColor1), Color.FromArgb((int)(tmp * 250), backColor2), LinearGradientMode.Vertical), r,20);
            g.DrawRoundRectangle(new Pen(borderColor, borderWidth), r, 20);
            Renderer.renderText(g, this.Left, top, this.Width, (int)letterHeight, title,this.Font, this.Format, this.TextAlignment, tmp,1,this.Color,this.OutlineColor);
            top += (int)letterHeight;
            height -= (int)letterHeight;
            base.Render(g, e);
            //Renderer.renderLabel(g,this);  //ToDo-Fix this
            top -= (int)letterHeight;
        }
    }
}
