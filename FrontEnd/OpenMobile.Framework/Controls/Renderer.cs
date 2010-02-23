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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OpenMobile
{
    /// <summary>
    /// Renders the default controls
    /// </summary>
    public static class Renderer
    {   
        /// <summary>
        /// Draws an image with transparency
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="transparency"></param>
        public static void drawTransparentImage(Graphics g, Image i, int left, int top, int width, int height, float transparency)
        {
            ColorMatrix cm = new ColorMatrix();
            {
                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                cm.Matrix33 = transparency;
                using (ImageAttributes ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(cm);
                    g.DrawImage(i, new Rectangle(left, top, width, height), 0, 0, i.Width, i.Height, GraphicsUnit.Pixel, ia);
                }
            }
        }
        /// <summary>
        /// Draws a rectangle with rounded corners
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        public static void FillRoundRectangle(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using(GraphicsPath gP = new GraphicsPath())
            {
                gP.AddLine(rect.Left + radius, rect.Top, rect.Left + rect.Width - (radius * 2), rect.Top);
                gP.AddArc(rect.Left + rect.Width - (radius * 2), rect.Top, radius * 2, radius * 2, 270, 90);
                gP.AddLine(rect.Left + rect.Width, rect.Top + radius, rect.Left + rect.Width, rect.Top + rect.Height - (radius * 2));
                gP.AddArc(rect.Left + rect.Width - (radius * 2), rect.Top + rect.Height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
                gP.AddLine(rect.Left + rect.Width - (radius * 2), rect.Top + rect.Height, rect.Left + radius, rect.Top + rect.Height);
                gP.AddArc(rect.Left, rect.Top + rect.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
                gP.AddLine(rect.Left, rect.Top + rect.Height - (radius * 2), rect.Left, rect.Top + radius);
                gP.AddArc(rect.Left, rect.Top, radius * 2, radius * 2, 180, 90);
                gP.CloseFigure();
                g.FillPath(brush, gP);
            }
        }
        /// <summary>
        /// Draw a rounded rectangle using the given pen and edge radius
        /// </summary>
        /// <param name="g"></param>
        /// <param name="p"></param>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        public static void DrawRoundRectangle(Graphics g, Pen p, RectangleF rect, float radius)
        {
            using (GraphicsPath gP = new GraphicsPath())
            {
                gP.AddLine(rect.Left + radius, rect.Top, rect.Left + rect.Width - (radius * 2), rect.Top);
                gP.AddArc(rect.Left + rect.Width - (radius * 2), rect.Top, radius * 2, radius * 2, 270, 90);
                gP.AddLine(rect.Left + rect.Width, rect.Top + radius, rect.Left + rect.Width, rect.Top + rect.Height - (radius * 2));
                gP.AddArc(rect.Left + rect.Width - (radius * 2), rect.Top + rect.Height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
                gP.AddLine(rect.Left + rect.Width - (radius * 2), rect.Top + rect.Height, rect.Left + radius, rect.Top + rect.Height);
                gP.AddArc(rect.Left, rect.Top + rect.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
                gP.AddLine(rect.Left, rect.Top + rect.Height - (radius * 2), rect.Left, rect.Top + radius);
                gP.AddArc(rect.Left, rect.Top, radius * 2, radius * 2, 180, 90);
                gP.CloseFigure();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(p,gP);
                g.SmoothingMode = SmoothingMode.Default;
            }
        }
        /// <summary>
        /// Render the given text
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        /// <param name="font"></param>
        /// <param name="alignment"></param>
        /// <param name="transparency"></param>
        public static void renderText(Graphics g, int x, int y, int w, int h, string text, Font font, textFormat format, Alignment alignment,float transparency,Color color,Color secondColor)
        {
            int modifyFont;
            if (transparency <= 0.1)
            {
                modifyFont = 100;
            }
            else
            {
                modifyFont = (int)((1 / transparency) / transparency)/2;
            }
            renderText(g, x, y, w, h, text, font, format, alignment, transparency, modifyFont,color,secondColor);
        }
        /// <summary>
        /// Render the given text
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <param name="alignment"></param>
        /// <param name="transparency"></param>
        /// <param name="modifyFont"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="secondColor"></param>
        public static void renderText(Graphics g, int x, int y, int w, int h, string text, Font font, textFormat format, Alignment alignment, float transparency,int modifyFont,Color color,Color secondColor)
        {
            if ((text==null)||(text == ""))
                return;
            FontStyle f = FontStyle.Regular;
            if ((format == textFormat.Bold) || (format == textFormat.BoldShadow))
            {
                f = FontStyle.Bold;
            }
            if ((format == textFormat.Italic) || (format == textFormat.ItalicShadow))
            {
                f = FontStyle.Italic;
            }
            if ((format == textFormat.Underline) || (format == textFormat.UnderlineShadow))
            {
                f = FontStyle.Underline;
            }
            if (format == textFormat.Outline)
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    GraphicsPath path = new GraphicsPath(FillMode.Winding);
                    path.AddString(text, font.FontFamily, (int)f, font.Size+6, new RectangleF(x, y, w, h), sFormat);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawPath(new Pen(new SolidBrush(Color.FromArgb((int)(transparency * secondColor.A), secondColor)), 3), path);
                    g.FillPath(new SolidBrush(Color.FromArgb((int)(transparency * color.A), color)), path);
                }
            }
            else if (format == textFormat.Glow)
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100) / 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    using (Font currentFont = new Font(font.FontFamily, font.Size + modifyFont, f))
                    {
                        for (int i = -3; i < 3; i++)
                            for (int j = -3; j < 3; j++)
                                g.DrawString(text, currentFont, new SolidBrush(Color.FromArgb(secondColor.A / (3 * (Math.Abs(i) + Math.Abs(j) + 1)), secondColor)), new RectangleF(x + i, y + j, w, h),sFormat);
                        g.DrawString(text, currentFont, new SolidBrush(Color.FromArgb((int)(transparency * color.A), color)), new RectangleF(x, y, w, h),sFormat);
                    }
                }
            }
            else
            {
                using (StringFormat sFormat = new StringFormat())
                {
                    sFormat.Alignment = (StringAlignment)((float)alignment % 10);
                    sFormat.LineAlignment = (StringAlignment)(int)(((float)alignment % 100)/ 10);
                    if (((int)alignment & 100) == 100)
                        sFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                    Font currentFont;
                    if (modifyFont > 0)
                        currentFont = new Font(font.FontFamily, font.Size + modifyFont, f);
                    else
                        currentFont = new Font(font, f);
                    using (SolidBrush defaultBrush = new SolidBrush(Color.FromArgb((int)(color.A* transparency), color)))
                    {
                        if (((int)format % 2) == 1)
                            g.DrawString(text, currentFont, new SolidBrush(Color.FromArgb((int)(transparency * 255), secondColor)), new RectangleF(x + 1, y + 2, w, h), sFormat);
                        g.DrawString(text, currentFont, defaultBrush, new RectangleF(x, y, w, h), sFormat);
                    }
                    currentFont.Dispose();
                }
            }
        }
    }
}
