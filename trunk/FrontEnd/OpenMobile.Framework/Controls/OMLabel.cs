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
    /// A label for displaying text
    /// </summary>
    public class OMLabel:OMControl
    {
        // Comment by Borte: access modefier changed to protected to allow more access when this control is inherited
        /// <summary>
        /// Label Text
        /// </summary>
        protected string text = String.Empty;
        /// <summary>
        /// Format for the labels text
        /// </summary>
        protected OpenMobile.Graphics.eTextFormat textFormat = OpenMobile.Graphics.eTextFormat.Normal;
        /// <summary>
        /// Text alignment
        /// </summary>
        protected OpenMobile.Graphics.Alignment textAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
        /// <summary>
        /// Sets the color of the text
        /// </summary>
        protected Color color = Color.White;
        /// <summary>
        /// Sets the font of the text
        /// </summary>
        protected Font font = new Font(Font.GenericSansSerif, 18F);
        /// <summary>
        /// Outline color of the text
        /// </summary>
        protected Color outlineColor = Color.Black;

        /// <summary>
        /// Sets the color of the text
        /// </summary>
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                textTexture = null;
            }
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        public OMLabel()
        {
            height=100;
            width = 130;
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMLabel(int x, int y, int w, int h)
        {
            left = x;
            top = y;
            width = w;
            height = h;
        }
        /// <summary>
        /// Creates a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return base.Clone();
        }
        /// <summary>
        /// Sets the Glow or Outline color of the text
        /// </summary>
        public virtual Color OutlineColor
        {
            get
            {
                return outlineColor;
            }
            set
            {
                if (outlineColor == value)
                    return;
                outlineColor = value;
                textTexture = null;
            }
        }

        /// <summary>
        /// Sets the Font of the text (Note: Size and Font Name only)
        /// </summary>
        public Font Font
        {
            get
            {
                return font;
            }
            set
            {
                if (font == value)
                    return;
                font = value;
                textTexture = null;
            }
        }

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Label";
            }
        }
        /// <summary>
        /// Texture for text
        /// </summary>
        protected OImage textTexture;
        /// <summary>
        /// The text displayed in the label
        /// </summary>
        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text == value)
                    return;
                text = value;
                if (textTexture != null)
                {
                    textTexture = null;
                }
                
            }
        }
        /// <summary>
        /// Sets the format of the displayed text
        /// </summary>
        public virtual OpenMobile.Graphics.eTextFormat Format
        {
            get
            {
                return textFormat;
            }
            set
            {
                if (textFormat == value)
                    return;
                textFormat = value;
                textTexture = null;
            }
        }
        /// <summary>
        /// Sets the alignment of the displayed text
        /// </summary>
        public virtual OpenMobile.Graphics.Alignment TextAlignment
        {
            get
            {
                return textAlignment;
            }
            set
            {
                if (textAlignment == value)
                    return;
                textAlignment = value;
                textTexture = null;
            }
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
            if (textTexture==null)
                textTexture=g.GenerateTextTexture(left, top, width, height, text, font, textFormat, textAlignment, color, outlineColor);
            g.DrawImage(textTexture, left, top, width, height,tmp);
        }
    }
}
