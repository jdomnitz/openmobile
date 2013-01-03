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
using System.Reflection;
using OpenMobile.Graphics;
using OpenMobile;
using System.ComponentModel;
using System.Collections.Generic;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Available Shapes
    /// </summary>
    public enum shapes : byte
    {
        /// <summary>
        /// No shape
        /// </summary>
        None,
        /// <summary>
        /// Draws a Rectangle (or Square)
        /// </summary>
        Rectangle,
        /// <summary>
        /// Draws a polygon
        /// </summary>
        Polygon,
        /// <summary>
        /// Draws an Oval/Ellipse/Circle
        /// </summary>
        Oval,
        /// <summary>
        /// Draws a Rounded Rectangle
        /// </summary>
        RoundedRectangle,
    }

    public struct ShapeData
    {
        /// <summary>
        /// The shape to draw
        /// </summary>
        public shapes Shape
        {
            get
            {
                return _Shape;
            }
            set
            {
                _Shape = value;
            }
        }
        private shapes _Shape;

        /// <summary>
        /// The fill color of the Shape
        /// </summary>
        public Color FillColor
        {
            get
            {
                return _FillColor;
            }
            set
            {
                _FillColor = value;
            }
        }
        private Color _FillColor;

        /// <summary>
        /// The color of the border
        /// </summary>
        public Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
            }
        }
        private Color _BorderColor;

        /// <summary>
        /// The width in pixels of a border (0 for no border)
        /// </summary>
        public int BorderSize
        {
            get
            {
                return _BorderSize;
            }
            set
            {
                _BorderSize = value;
            }
        }
        private int _BorderSize;

        /// <summary>
        /// Sets the corner radius of a rounded rectangle
        /// </summary>
        public int CornerRadius
        {
            get { return _CornerRadius; }
            set
            {
                _CornerRadius = value;
            }
        }
        private int _CornerRadius;

        /// <summary>
        /// The graphic points for this shape
        /// </summary>
        public Point[] GraphicPoints
        {
            get
            {
                return _GraphicPoints;
            }
            set
            {
                _GraphicPoints = value;
            }
        }
        private Point[] _GraphicPoints;

        public ShapeData(shapes shape)
        {
            _Shape = shape;
            _FillColor = new Color();
            _BorderColor = new Color();
            _BorderSize = 0;
            _CornerRadius = 0;
            _GraphicPoints = null;
        }
        public ShapeData(shapes shape, Color fillColor)
        {
            _Shape = shape;
            _FillColor = fillColor;
            _BorderColor = Color.Empty;
            _BorderSize = 0;
            _CornerRadius = 10;
            _GraphicPoints = null;
        }
        public ShapeData(shapes shape, Color fillColor, Color borderColor, int borderSize)
        {
            _Shape = shape;
            _FillColor = fillColor;
            _BorderColor = borderColor;
            _BorderSize = borderSize;
            _CornerRadius = 10;
            _GraphicPoints = null;
        }
        public ShapeData(shapes shape, Color fillColor, Color borderColor, int borderSize, int cornerRadius)
        {
            _Shape = shape;
            _FillColor = fillColor;
            _BorderColor = borderColor;
            _BorderSize = borderSize;
            _CornerRadius = cornerRadius;
            _GraphicPoints = null;
        }
        public ShapeData(shapes shape, Color fillColor, Color borderColor, int borderSize, Point[] graphicPoints)
        {
            _Shape = shape;
            _FillColor = fillColor;
            _BorderColor = borderColor;
            _BorderSize = borderSize;
            _CornerRadius = 0;
            _GraphicPoints = graphicPoints;
        }
        public ShapeData(shapes shape, Color fillColor, Color borderColor, int borderSize, int cornerRadius, Point[] graphicPoints)
        {
            _Shape = shape;
            _FillColor = fillColor;
            _BorderColor = borderColor;
            _BorderSize = borderSize;
            _CornerRadius = cornerRadius;
            _GraphicPoints = graphicPoints;
        }
    }

    /// <summary>
    /// A base class for control graphics
    /// </summary>
    public abstract class OMControlGraphicsBase : OMControl
    {
        /// <summary>
        /// Properties for the shape object
        /// </summary>
        public ShapeData ShapeData
        {
            get
            {
                return this._ShapeData;
            }
            set
            {
                this._ShapeData = value;
            }
        }
        protected ShapeData _ShapeData = new ShapeData();

        private Pen _BorderPen;
        private Brush _FillBrush;

        /// <summary>
        /// Render the requested shape
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        protected void DrawShape(Graphics.Graphics g, renderingParams e)
        {
            // No use in rendering anything if there's nothing to render
            if (_ShapeData.Shape == shapes.None)
                return;

            // Regenerate fill brush?
            int AlphaFill = (int)this.GetAlphaValue255(_ShapeData.FillColor.A);
            if (_FillBrush.Color.R != _ShapeData.FillColor.R || _FillBrush.Color.G != _ShapeData.FillColor.G || _FillBrush.Color.B != _ShapeData.FillColor.B || _FillBrush.Color.A != AlphaFill)
                // Yes, create a new fill brush
                _FillBrush = new Brush(Color.FromArgb(AlphaFill, _ShapeData.FillColor));

            int BorderFill = (int)this.GetAlphaValue255(_ShapeData.BorderColor.A);
            if (_BorderPen.Width != _ShapeData.BorderSize || _BorderPen.Color.R != _ShapeData.BorderColor.R || _BorderPen.Color.G != _ShapeData.BorderColor.G || _BorderPen.Color.B != _ShapeData.BorderColor.B || _BorderPen.Color.A != BorderFill)
                _BorderPen = new Pen(Color.FromArgb(BorderFill, _ShapeData.BorderColor), _ShapeData.BorderSize);

            switch (_ShapeData.Shape)
            {
                case shapes.Rectangle:
                    g.FillRectangle(_FillBrush, left, top, width, height);
                    if (_ShapeData.BorderSize > 0)
                        g.DrawRectangle(_BorderPen, left, top, width, height);
                    break;
                case shapes.Polygon:
                    {
                        // Convert points from relative to absolute coordinates
                        Point[] points = (Point[])_ShapeData.GraphicPoints.Clone();
                        for (int i = 0; i < points.Length; i++)
                            points[i].Translate(this.left, this.top);
                        // BUG: Fill does not work
                        g.FillPolygon(_FillBrush, points);
                        if (_ShapeData.BorderSize > 0)
                            g.DrawPolygon(_BorderPen, points);

                        // Render points
                        if (this._SkinDebug)
                        {
                            using (Pen p = new Pen(Color.Yellow, _ShapeData.BorderSize))
                                for (int i = 0; i < points.Length; i++)
                                {
                                    g.DrawLine(p, points[i].X - 1, points[i].Y - 1, points[i].X + 1, points[i].Y + 1);
                                    g.DrawLine(p, points[i].X + 1, points[i].Y - 1, points[i].X - 1, points[i].Y + 1);
                                }
                        }
                    }
                    break;
                case shapes.Oval:
                    g.FillEllipse(_FillBrush, left, top, width, height);
                    if (_ShapeData.BorderSize > 0)
                        g.DrawEllipse(_BorderPen, left, top, width, height);
                    break;
                case shapes.RoundedRectangle:
                    g.FillRoundRectangle(_FillBrush, left, top, width, height, _ShapeData.CornerRadius);
                    if (_ShapeData.BorderSize > 0)
                        g.DrawRoundRectangle(_BorderPen, left, top, width, height, _ShapeData.CornerRadius);
                    break;
            }
        }

        /// <summary>
        /// Render the requested shape
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="shape"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="borderSize"></param>
        /// <param name="cornerRadius"></param>
        /// <param name="graphicPoints"></param>
        /// <param name="ShowPoints"></param>
        /// <param name="pointsColor"></param>
        protected void DrawShape(Graphics.Graphics g, Rectangle rect, shapes shape, Color fillColor, Color borderColor, int borderSize, int cornerRadius, Point[] graphicPoints, bool ShowPoints, Color pointsColor)
        {
            // No use in rendering anything if there's nothing to render
            if (shape == shapes.None)
                return;

            using (Brush brush = new Brush(Color.FromArgb((int)this.GetAlphaValue255(fillColor.A), fillColor)))
            {
                Pen pen = new Pen();
                if (borderSize > 0)
                    pen = new Pen(Color.FromArgb((int)this.GetAlphaValue255(borderColor.A), borderColor), borderSize);

                switch (shape)
                {
                    case shapes.Rectangle:
                        g.FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
                        if (borderSize > 0)
                            g.DrawRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height);
                        break;
                    case shapes.Polygon:
                        {
                            // Convert points from relative to absolute coordinates
                            if (graphicPoints == null || graphicPoints.Length == 0)
                                break;

                            Point[] points = (Point[])graphicPoints.Clone();
                            for (int i = 0; i < points.Length; i++)
                                points[i].Translate(this.left, this.top);
                            // BUG: Fill does not work
                            g.FillPolygon(brush, points);
                            if (borderSize > 0)
                                g.DrawPolygon(pen, points);

                            // Render points
                            if (ShowPoints)
                            {
                                using (Pen p = new Pen(pointsColor, borderSize))
                                    for (int i = 0; i < points.Length; i++)
                                    {
                                        g.DrawLine(p, points[i].X - 1, points[i].Y - 1, points[i].X + 1, points[i].Y + 1);
                                        g.DrawLine(p, points[i].X + 1, points[i].Y - 1, points[i].X - 1, points[i].Y + 1);
                                    }
                            }

                        }
                        break;
                    case shapes.Oval:
                        g.FillEllipse(brush, rect.Left, rect.Top, rect.Width, rect.Height);
                        if (borderSize > 0)
                            g.DrawEllipse(pen, rect.Left, rect.Top, rect.Width, rect.Height);
                        break;
                    case shapes.RoundedRectangle:
                        g.FillRoundRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height, cornerRadius);
                        if (borderSize > 0)
                            g.DrawRoundRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height, cornerRadius);
                        break;
                }
            }
        }

        /// <summary>
        /// Render the shape to a bitmap
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected System.Drawing.Bitmap DrawShapeToBMP(ShapeData properties, Rectangle rect)
        {
            // No use in rendering anything if there's nothing to render
            if (properties.Shape == shapes.None)
                return null;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width+1, height+1);
            using (System.Drawing.Graphics gBMP = System.Drawing.Graphics.FromImage(bmp))
            {
                using (System.Drawing.Brush brush = new System.Drawing.SolidBrush(Color.FromArgb(properties.FillColor.A, properties.FillColor).ToSystemColor()))
                {
                    System.Drawing.Pen pen = new System.Drawing.Pen(Color.FromArgb(properties.BorderColor.A, properties.BorderColor).ToSystemColor());
                    if (properties.BorderSize > 0)
                    {
                        pen.Dispose();
                        pen = new System.Drawing.Pen(Color.FromArgb(properties.BorderColor.A, properties.BorderColor).ToSystemColor(), properties.BorderSize);
                    }

                    switch (properties.Shape)
                    {
                        case shapes.Rectangle:
                            gBMP.FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
                            if (properties.BorderSize > 0)
                                gBMP.DrawRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height);
                            break;
                        case shapes.Polygon:
                            {
                                // Convert points from relative to absolute coordinates
                                if (properties.GraphicPoints == null || properties.GraphicPoints.Length == 0)
                                    break;

                                // Convert from OpenMobile.Points points to System.Points
                                System.Drawing.Point[] points = new System.Drawing.Point[properties.GraphicPoints.Length];
                                for (int i = 0; i < points.Length; i++)
                                    points[i] = new System.Drawing.Point(properties.GraphicPoints[i].X, properties.GraphicPoints[i].Y);

                                gBMP.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                gBMP.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                gBMP.FillPolygon(brush, points);
                                if (properties.BorderSize > 0)
                                    gBMP.DrawPolygon(pen, points);

                                //// Render points
                                //if (ShowPoints)
                                //{
                                //    using (Pen p = new Pen(pointsColor, borderSize))
                                //        for (int i = 0; i < points.Length; i++)
                                //        {
                                //            gBMP.DrawLine(p, points[i].X - 1, points[i].Y - 1, points[i].X + 1, points[i].Y + 1);
                                //            gBMP.DrawLine(p, points[i].X + 1, points[i].Y - 1, points[i].X - 1, points[i].Y + 1);
                                //        }
                                //}

                            }
                            break;
                        case shapes.Oval:
                            gBMP.FillEllipse(brush, rect.Left, rect.Top, rect.Width, rect.Height);
                            if (properties.BorderSize > 0)
                                gBMP.DrawEllipse(pen, rect.Left, rect.Top, rect.Width, rect.Height);
                            break;
                        //case shapes.RoundedRectangle:
                        //    gBMP.FillRoundRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height, cornerRadius);
                        //    if (borderSize > 0)
                        //        gBMP.DrawRoundRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height, cornerRadius);
                        //    break;
                    }
                    pen.Dispose();
                }
            }

            return bmp;

        }

        #region Constructors

        private void Init()
        {
        }

        /// <summary>
        /// Create a new control
        /// </summary>
        [Obsolete("Always provide a control name. Method will be removed in next release")]
        public OMControlGraphicsBase()
            : base()
        {
            Init();
        }
        /// <summary>
        /// Create a new control
        /// </summary>
        public OMControlGraphicsBase(string Name)
            : base(Name)
        {
            Init();
        }
        /// <summary>
        /// Create a new control
        /// </summary>
        public OMControlGraphicsBase(string Name, int Left, int Top, int Width, int Height)
            : base(Name, Left, Top, Width, Height)
        {
            Init();
        }

        #endregion
    }
}
