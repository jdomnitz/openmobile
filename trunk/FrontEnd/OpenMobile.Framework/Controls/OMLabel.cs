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

namespace OpenMobile.Controls
{
    /// <summary>
    /// A label for displaying text
    /// </summary>
    public class OMLabel : OMControl, ISensorDisplay
    {
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
        protected Color color = BuiltInComponents.SystemSettings.SkinTextColor;
        /// <summary>
        /// Sets the font of the text
        /// </summary>
        protected Font font = new Font(Font.GenericSansSerif, 18F);
        /// <summary>
        /// Outline color of the text
        /// </summary>
        protected Color outlineColor = Color.Black;
        /// <summary>
        /// Sensor name to subscribe to
        /// </summary>
        protected string displaySensorName = "";
        
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
                raiseUpdate(false);
            }
        }
        /// <summary>
        /// Create a new OMLabel
        /// </summary>
        [Obsolete("Use OMLabel(string name, int x, int y, int w, int h) instead")]
        public OMLabel()
        {
            Init("", 0, 0, 100, 130);
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
        {
            Init("", x, y, w, h);
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
        {
            Init(name, x, y, w, h);
        }
        private void Init(string Name, int x, int y, int w, int h)
        {
            this.Name = Name;
            this.Top = y;
            this.Left = x;
            this.Width = w;
            this.Height = h;
            this.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter;
            this.Format = OpenMobile.Graphics.eTextFormat.Normal;
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
                raiseUpdate(false);
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
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// Sets the Font size of the text
        /// </summary>
        public float FontSize
        {
            get
            {
                return font.Size;
            }
            set
            {
                if (font == null)
                    return;
                font.Size = value;
                textTexture = null;
                raiseUpdate(false);
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
                raiseUpdate(false);

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
                raiseUpdate(false);
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
                raiseUpdate(false);
            }
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
            if (g.TextureGenerationRequired(textTexture))
                textTexture = g.GenerateTextTexture(textTexture, left, top, width + 5, height, text, font, textFormat, textAlignment, color, outlineColor);
            g.DrawImage(textTexture, left, top, width+5, height, tmp);

            // Skin debug function 
            if (_SkinDebug)
                base.DrawSkinDebugInfo(g, Color.Green);
        }

        /// <summary>
        /// sensor to be watched
        /// </summary>
        protected Plugin.Sensor sensor;
        /// <summary>
        /// Sets the sensor to subscribe to
        /// </summary>
        public string sensorName
        {
            get
            {
                return sensor.Name;
            }
            set
            {
                Plugin.Sensor sensor = helperFunctions.Sensors.getPluginByName(value);
                if (sensor != null)
                {
                    this.sensor = sensor;
                    //sensor.newSensorDataReceived += new Plugin.SensorDataReceived(delegate(OpenMobile.Plugin.Sensor sender)
                    //{
                    //    this.Text = sender.FormatedValue();
                    //    raiseUpdate(false);
                    //});
                    this.Text = sensor.FormatedValue();
                    raiseUpdate(false);
                }
            }
        }

    }
}
