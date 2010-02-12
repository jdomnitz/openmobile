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
using System.Drawing;
using System.Windows.Forms;

namespace OpenMobile.Framework.Controls
{
    /// <summary>
    /// Provides a slider to select the luminosity of a color
    /// </summary>
    public partial class LuminositySlider : UserControl
    {
        private int sliderValue=50;
        /// <summary>
        /// A new color has been selected
        /// </summary>
        /// <param name="c"></param>
        public delegate void ColorChanged(Color c);
        /// <summary>
        /// Occurs when a new color is selected
        /// </summary>
        public event ColorChanged SelectedColorChanged;
        /// <summary>
        /// The value of the slider (0-100)
        /// </summary>
        public int Value
        {
            get
            {
                return sliderValue;
            }
            set
            {
                sliderValue = value;
            }
        }
        /// <summary>
        /// Provides a slider to set the luminosity of a control
        /// </summary>
        public LuminositySlider()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }
        /// <summary>
        /// Draws the luminosity sample swatch
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(Pens.Black, new Rectangle(this.Location, new Size(this.Width - 10, this.Height)));
            for (int i = 0; i <= this.Height; i++)
            {
                HSLColor col=new HSLColor(this.ForeColor);
                col.Luminosity = (double)(this.Height-i) / this.Height;
                g.DrawLine(new Pen((Color)col), 0, i, this.Width - 10, i);
            }
            int y=(int)((sliderValue/100.0)*this.Height);
            g.FillPolygon(Brushes.Black, new Point[] {new Point(this.Width-9,y),new Point(this.Width,y-5),new Point(this.Width,y+5)});
        }

        private void LuminositySlider_MouseDown(object sender, MouseEventArgs e)
        {
            sliderValue = (int)((e.Y / (float)this.Height) * 100);
            this.Invalidate();
            colorChanged();
        }
        private void colorChanged()
        {
            HSLColor color=new HSLColor(this.ForeColor);
            color.Luminosity = (double)((100-sliderValue)/100.0);
            if (SelectedColorChanged != null)
                SelectedColorChanged(Color.FromArgb(this.ForeColor.A,color));
        }

        private void LuminositySlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                sliderValue = (int)((e.Y / (float)this.Height) * 100);
                this.Invalidate();
                colorChanged();
            }
        }

        private void LuminositySlider_ForeColorChanged(object sender, EventArgs e)
        {
            HSLColor color=new HSLColor(this.ForeColor);
            sliderValue = 100-(int)(color.Luminosity * 100);
            //colorChanged();
        }
    }
    /// <summary>
    /// Converts RGB to HSL
    /// </summary>
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;

        private const double scale = 1.0;

        /// <summary>
        /// Gets/Sets the luminosity
        /// </summary>
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        #region Casts to/from System.Drawing.Color
        /// <summary>
        /// Converts an HSL color to a system.drawing.color
        /// </summary>
        /// <param name="hslColor"></param>
        /// <returns></returns>
        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.luminosity != 0)
            {
                if (hslColor.saturation == 0)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor.luminosity < 0.5)  //<=??
                temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
            else
                temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
            return temp2;
        }
        /// <summary>
        /// Converts a color to an HSL color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.luminosity = color.GetBrightness();
            hslColor.saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion
        /// <summary>
        /// Converts RGB to HSL
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void SetRGB(int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            this.hue = hslColor.hue;
            this.saturation = hslColor.saturation;
            this.luminosity = hslColor.luminosity;
        }
        /// <summary>
        /// Creates a new HSL object
        /// </summary>
        public HSLColor() { }
        /// <summary>
        /// Converts a system.drawing.color to an hsl color
        /// </summary>
        /// <param name="color"></param>
        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }
    }
}
