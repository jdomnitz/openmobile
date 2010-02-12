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

namespace OpenMobile.Controls
{
    /// <summary>
    /// Allows selecting a custom color from a swatch
    /// </summary>
    public partial class CustomColor : UserControl
    {
        Point origin;
        /// <summary>
        /// Occurs when a new color is selected
        /// </summary>
        /// <param name="c"></param>
        public delegate void ColorChanged(Color c);
        /// <summary>
        /// Occurs when a new color is selected
        /// </summary>
        public event ColorChanged SelectedColorChanged;
        /// <summary>
        /// Creates a new custom color control
        /// </summary>
        public CustomColor()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        /// <summary>
        /// Paints the array of color choices
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            origin = new Point((this.Width-4) / 2, (this.Height-4) / 2);
            if (this.Width < this.Height)
                radius = (this.Width-5) / 2;
            else
                radius = (this.Height-5) / 2;
            Graphics g = e.Graphics;
            int R=255;
            int G=0;
            int B=0;
            int i=0;
            for(int x=0;x<255;x++)
            {
                G++;
                i++;
                drawPoint(g, R, G, B, i);
            }
            for (int x = 0; x < 255; x++)
            {
                R--;
                i++;
                drawPoint(g, R, G, B, i);
            }
            for (int x = 0; x < 255; x++)
            {
                B++;
                i++;
                drawPoint(g, R, G, B, i);
            }
            for (int x = 0; x < 255; x++)
            {
                G--;
                i++;
                drawPoint(g, R, G, B, i);
            }
            for (int x = 0; x < 255; x++)
            {
                R++;
                i++;
                drawPoint(g, R, G, B, i);
            }
            for (int x = 0; x < 255; x++)
            {
                B--;
                i++;
                drawPoint(g, R, G, B, i);
            }
        }
        private void drawPoint(Graphics g, int R, int G, int B,int i)
        {
                g.DrawLine(new Pen(Color.FromArgb(R, G, B),2F), origin, CalcPoint2(i));
        }
        private int radius;
        private Point CalcPoint2(int i)
        {
            double angle = ((i * Math.PI * 2)/ 1530.0)  ;
            int x = (int)Math.Round((Math.Cos(angle) * radius));
            int y = (int)Math.Round((Math.Sin(angle) * radius));
            return new Point(origin.X+ x, origin.Y+ y);
        }
        /// <summary>
        /// Paints the background color
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.Window, e.ClipRectangle);
        }

        private void CustomColor_MouseDown(object sender, MouseEventArgs e)
        {
            Bitmap b=new Bitmap(this.Width,this.Height);
            this.OnPaint(new PaintEventArgs(Graphics.FromImage(b),this.Bounds));
            Color c=b.GetPixel(e.X,e.Y);
            if (SelectedColorChanged!=null)
                SelectedColorChanged(c);
        }
    }
}

