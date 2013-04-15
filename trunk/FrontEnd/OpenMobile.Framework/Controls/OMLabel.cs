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
    [System.Serializable]
    public class OMLabel : OMControlGraphicsBase
    {
        /// <summary>
        /// Label Text
        /// </summary>
        protected string _text = String.Empty;
        /// <summary>
        /// Format for the labels text
        /// </summary>
        protected OpenMobile.Graphics.eTextFormat _textFormat = OpenMobile.Graphics.eTextFormat.Normal;
        /// <summary>
        /// Text alignment
        /// </summary>
        protected OpenMobile.Graphics.Alignment _textAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
        /// <summary>
        /// Sets the color of the text
        /// </summary>
        protected Color _color = BuiltInComponents.SystemSettings.SkinTextColor;
        /// <summary>
        /// The background color of the textbox
        /// </summary>
        protected Color background = Color.Transparent;
        /// <summary>
        /// Sets the font of the text
        /// </summary>
        protected Font _font = new Font(Font.GenericSansSerif, 18F);
        /// <summary>
        /// Outline color of the text
        /// </summary>
        protected Color _outlineColor = BuiltInComponents.SystemSettings.SkinFocusColor;//Color.Black;
        /// <summary>
        /// Sensor name to subscribe to
        /// </summary>
        protected string _displaySensorName = "";

        /// <summary>
        /// Get's the size of the text contained in this control
        /// </summary>
        public Size GetSizeOfContainedText()
        {
            SizeF s = Graphics.Graphics.MeasureString(_text, _font, _textFormat, _textAlignment, _Region);
            return new Size(s.Width, s.Height);
        }
        
        /// <summary>
        /// Sets the color of the text
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                _RefreshGraphic = true;
                raiseUpdate(false);
            }
        }

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
        /// Enabled automatically fitting of text to the label size
        /// </summary>
        public FitModes AutoFitTextMode
        {
            get
            {
                return this._AutoFitTextMode;
            }
            set
            {
                if (this._AutoFitTextMode != value)
                {
                    this._AutoFitTextMode = value;
                    RefreshGraphic();
                }
            }
        }
        protected FitModes _AutoFitTextMode;        

        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        [Obsolete("Use OMLabel(string name, int x, int y, int w, int h) instead")]
        public OMLabel()
            : base("", 0, 0, 100, 130)
        {
            Init();
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        [Obsolete("Use OMLabel(string name, int x, int y, int w, int h) instead")]
        public OMLabel(int x, int y, int w, int h)
            : base("", x, y, w, h)
        {
            Init();
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMLabel(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            Init();
        }
        /// <summary>
        /// Create a new OMLabel with the specified text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMLabel(string name, int x, int y, int w, int h, string text)
            : base(name, x, y, w, h)
        {
            Init();
            this.Text = text;
        }
        private void Init()
        {
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
        }

        /// <summary>
        /// Creates a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public override object Clone(OMPanel parent)
        {
            return base.Clone(parent);
        }
        /// <summary>
        /// Sets the Glow or Outline color of the text
        /// </summary>
        public virtual Color OutlineColor
        {
            get
            {
                return _outlineColor;
            }
            set
            {
                if (_outlineColor == value)
                    return;
                _outlineColor = value;
                _RefreshGraphic = true;
                Refresh();
            }
        }

        /// <summary>
        /// Sets the Font of the text (Note: Size and Font Name only)
        /// </summary>
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                if (_font == value)
                    return;
                _font = value;
                _RefreshGraphic = true;
                Refresh();
            }
        }

        /// <summary>
        /// Sets the Font size of the text
        /// </summary>
        public float FontSize
        {
            get
            {
                return _font.Size;
            }
            set
            {
                if (_font == null)
                    return;
                _font.Size = value;
                _RefreshGraphic = true;
                Refresh();
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
                return _text;
            }
            set
            {
                //if (value == null)
                //    value = String.Empty;
                //if (_text == value)
                //    return;
                _text = value;
                
                // Check for datasource present
                base.DataSource_InLine(ref _text);

                _RefreshGraphic = true;
                Refresh();
            }
        }
        /// <summary>
        /// Sets the format of the displayed text
        /// </summary>
        public virtual OpenMobile.Graphics.eTextFormat Format
        {
            get
            {
                return _textFormat;
            }
            set
            {
                if (_textFormat == value)
                    return;
                _textFormat = value;
                _RefreshGraphic = true;
                Refresh();
            }
        }
        /// <summary>
        /// Sets the alignment of the displayed text
        /// </summary>
        public virtual OpenMobile.Graphics.Alignment TextAlignment
        {
            get
            {
                return _textAlignment;
            }
            set
            {
                if (_textAlignment == value)
                    return;
                _textAlignment = value;
                _RefreshGraphic = true;
                Refresh();
            }
        }

        /// <summary>
        /// Render the controls background
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void Render_Background(Graphics.Graphics g, renderingParams e)
        {
            // Render background (if any)
            if (background != Color.Transparent)
            {
                using (Brush Fill = new Brush(Color.FromArgb((int)(this.GetAlphaValue255(background.A)), background)))
                {
                    g.FillRectangle(Fill, left, top, width, height);
                }
            }

            // Render shape (if any)
            base.DrawShape(g, e);
        }

        /// <summary>
        /// Render the controls foreground
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void Render_Foreground(Graphics.Graphics g, renderingParams e, string text)
        {
            // No use in rendering if text is empty
            if (!String.IsNullOrEmpty(text))
            {
                if (_RefreshGraphic)
                    textTexture = g.GenerateTextTexture(textTexture, left, top, width, height, text, _font, _textFormat, _textAlignment, _color, _outlineColor, _AutoFitTextMode);

                g.DrawImage(textTexture, left, top, width, height, _RenderingValue_Alpha);
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

            Render_Background(g, e);
            Render_Foreground(g, e, _text);

            base.RenderFinish(g, e);
        }

        #region DataSource

        internal override void DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            _text = dataSource.FormatedValue;
            _RefreshGraphic = true;
            Refresh();
        }

        internal override void DataSource_Missing()
        {
            _text = "ERR";
            _RefreshGraphic = true;
            Refresh();
        }

        internal override void DataSource_InLine_Changed(string s)
        {
            _text = s;
            _RefreshGraphic = true;
            Refresh();
        }

        #endregion
    }
}
