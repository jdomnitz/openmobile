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
    /// A progress bar control
    /// </summary>
    public class OMProgress : OMControl, ISensorDisplay 
    {
        /// <summary>
        /// progress value
        /// </summary>
        protected int value = 0;
        /// <summary>
        /// minimum value
        /// </summary>
        protected int minimum = 0;
        /// <summary>
        /// maximum value
        /// </summary>
        protected int maximum = 100;
        /// <summary>
        /// vertical progress bar
        /// </summary>
        protected bool vertical;
        /// <summary>
        /// Represents the first of two colors for the progress bars gradient
        /// </summary>
        protected Color firstColor = Color.DarkRed;
        /// <summary>
        /// Represents the second of two colors for the progress bars gradient
        /// </summary>
        protected Color secondColor = Color.Red;
        /// <summary>
        /// Background color
        /// </summary>
        protected Color backColor = Color.FromArgb(180, Color.White);

        /// <summary>
        /// The background color of the progress bar
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return backColor;
            }
            set
            {
                backColor = value;
            }
        }
        /// <summary>
        /// Display the progress bar vertically instead
        /// </summary>
        public bool Vertical
        {
            get
            {
                return vertical;
            }
            set
            {
                vertical = value;
            }
        }

        /// <summary>
        /// Create the control with the default size and location
        /// </summary>
        public OMProgress()
        {
            this.Top = 20;
            this.Left = 20;
            this.Width = 100;
            this.Height = 25;
        }
        /// <summary>
        /// Create a new progress bar
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMProgress(int x, int y, int w, int h)
        {
            top = y;
            left = x;
            width = w;
            height = h;
        }
        /// <summary>
        /// A number between the minimum and the maximum value
        /// </summary>
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if ((value >= this.minimum) && (value <= this.maximum))
                {
                    this.value = value;
                    raiseUpdate(false);
                }
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 100%
        /// </summary>
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (maximum == value)
                    return;
                if (Value > value)
                    Value = value;
                maximum = value;
            }
        }
        /// <summary>
        /// The value at which the progress bar displays 0%
        /// </summary>
        public int Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
            }
        }
        /// <summary>
        /// Represents the first of two colors for the progress bars gradient
        /// </summary>
        public Color FirstColor
        {
            get
            {
                return firstColor;
            }
            set
            {
                firstColor = value;
            }
        }

        /// <summary>
        /// Represents the second of two colors for the progress bars gradient
        /// </summary>
        public Color SecondColor
        {
            get
            {
                return secondColor;
            }
            set
            {
                secondColor = value;
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
            if (Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            else if (Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            g.FillRectangle(new Brush(Color.FromArgb((int)(backColor.A * tmp), backColor)), left, top, width, height);
            if (vertical == false)
                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * firstColor.A), firstColor), Color.FromArgb((int)(tmp * secondColor.A), secondColor), Gradient.Horizontal), left, top, (int)(width * ((float)value / maximum)), height);
            else
                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * secondColor.A), secondColor), Color.FromArgb((int)(tmp * firstColor.A), firstColor), Gradient.Vertical), left, top + height - (int)(height * ((float)value / maximum)), width, (int)(height * ((float)value / maximum)));
        }

        /// <summary>
        /// sensor name to be watched
        /// </summary>
        protected string sensor;
        /// <summary>
        /// Sets the sensor to subscribe to
        /// </summary>
        public string sensorName
        {
            get
            {
                return sensor;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                this.sensor = value;

                object o;
                BuiltInComponents.Host.getData(eGetData.GetAvailableSensors, "", out o);
                if (o == null)
                    return;
                System.Collections.Generic.List<OpenMobile.Plugin.Sensor> sensors = (System.Collections.Generic.List<OpenMobile.Plugin.Sensor>)o;

                OpenMobile.Plugin.Sensor sensor = sensors.Find(s => s.Name == this.sensor);
                if (sensor != null)
                    sensor.newSensorDataReceived += new OpenMobile.Plugin.SensorDataReceived(sensor_newSensorDataReceived);
                this.Value = (int)sensor.Value;
            }
        }

        void sensor_newSensorDataReceived(OpenMobile.Plugin.Sensor sender)
        {
            this.Value = (int)sender.Value;
        }

    }
}