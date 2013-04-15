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
    public class OMTestControl : OMControlGraphicsBase
    {
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            //g.FillRectangle(new Brush(Color.Red), Region.Left, Region.Top, Region.Width, 1);
            //g.FillRectangle(new Brush(Color.Red, Color.Blue, Color.Green, Color.Yellow), Region);
            //g.FillRectangle(new Brush(Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Green), Region);

            //GradientData gradient = GradientData.CreateColorBorder(0.05f, 0.2f, Color.Red, Color.Red, Color.Red, Color.Red, Color.Green);
            //GradientData gradient = GradientData.CreateHorizontalGradient(Color.Red, Color.Green, Color.Blue);
            //GradientData gradient = GradientData.CreateHorizontalGradient(
            //    new GradientData.ColorPoint(0, 0.0, Color.Red),
            //    new GradientData.ColorPoint(0, 0.2, Color.Green),
            //    new GradientData.ColorPoint(0, 0.2, Color.Blue),
            //    new GradientData.ColorPoint(0, 1, Color.Blue),
            //    new GradientData.ColorPoint(0, 0.2, Color.Red));
            //GradientData gradient = GradientData.CreateVerticalGradient(
            //    new GradientData.ColorPoint(0, 0.0, Color.Red),
            //    new GradientData.ColorPoint(0, 0.5, Color.Green));
            //GradientData gradient = GradientData.CreateVerticalGradient(Color.FromArgb(255, 51, 58, 77), Color.FromArgb(255, 5, 5, 5));
            //GradientData gradient = GradientData.CreateVerticalGradient(
            //    new GradientData.ColorPoint(0, 0.0, Color.FromArgb(255, 70, 70, 70)),
            //    new GradientData.ColorPoint(0, 0.1, Color.FromArgb(255, 51, 58, 77)),
            //    new GradientData.ColorPoint(0, 1.0, Color.FromArgb(255, 5, 5, 5)),
            //    new GradientData.ColorPoint(0, 0.8, Color.FromArgb(255, 5, 5, 5)),
            //    new GradientData.ColorPoint(0, 0.1, Color.FromArgb(255, 70, 70, 70))
            //    );
            //GradientData gradient = GradientData.CreateHorizontalGradient(
            //    new GradientData.ColorPoint(0.0, 0, Color.Black),
            //    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(0, 0, 200)),
            //    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(0, 0, 200)),
            //    new GradientData.ColorPoint(0.5, 0, Color.Black));
            //g.FillRectangle(gradient, Region);

            //g.DrawArc(new Pen(Color.Red, 5), Region, 0, 360);
            //g.DrawEllipse(new Pen(Color.Red, 5), Region);
            //g.DrawLine(new Pen(Color.Red, 5), Region.Left, Region.Top, Region.Right, Region.Bottom);
            //g.DrawLine(new Pen(Color.Red, 5), new Point[] {new Point(Region.Left, Region.Top), new Point(Region.Right, Region.Bottom)});
            //g.DrawPolygon(new Pen(Color.Red, 5), new Point[] { new Point(Region.Left, Region.Top), new Point(Region.Right, Region.Bottom) });
            //g.DrawRectangle(new Pen(Color.Red, 5), Region);
            //g.DrawRoundRectangle(new Pen(Color.Red, 5), Region, 20);
            //g.FillEllipse(new Brush(Color.Red), Region);
            //g.FillPolygon(new Brush(Color.Red), new Point[] { new Point(Region.Left, Region.Top), new Point(Region.Right, Region.Bottom), new Point(Region.Left, Region.Bottom) });
            //g.FillRoundRectangle(new Brush(Color.Red, Color.Green, Gradient.Vertical), Region, 20);

            //g._3D_ModelView_Push();

            //g._3D_SetModelView(Math.Vector3d.Zero, new Math.Vector3d(0, 30, 0), Math.Vector3d.Zero, 0, true);

            //Rectangle rect = new Rectangle(100, 100, 200, 200);
            //g.FillRoundRectangle(new Brush(Color.FromArgb(100, Color.Blue)), rect, 30);
            //g.DrawRoundRectangle(new Pen(Color.FromArgb(100, Color.Red), 20), rect, 30);

            //rect = new Rectangle(400, 100, 200, 200);
            //g.FillRoundRectangle(new Brush(Color.FromArgb(100, Color.Green)), rect, 30);
            //g.DrawRoundRectangle(new Pen(Color.FromArgb(100, Color.Red), 20), rect, 30);

            //g._3D_ModelView_Pop();

            base.RenderFinish(g, e);
        }
    }
}
