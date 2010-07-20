using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging ;

namespace OpenMobile.Graphics
{
    public class Graphics
    {
        protected System.Drawing.Graphics g;

        public void Initialize()
        {

        }

        public void Dispose()
        {
        }
       
        public static Graphics FromHwnd(IntPtr Handle)
        {
            Graphics graph = new Graphics();
            graph.g = System.Drawing.Graphics.FromHwnd(Handle);
            return graph;
        }

        public static Graphics FromGraphics(System.Drawing.Graphics sourceGraph)
        {
            Graphics graph = new Graphics();
            graph.g = sourceGraph;
            return graph;
        }
        
        public System.Drawing.Graphics Raw()
        {
            return g;
        }

        public void Clear(System.Drawing.Color color)
        {
            g.Clear(color);
        }


        public void DrawArc(Pen pen, System.Drawing.RectangleF rect, float startAngle, float sweepAngle)
        {
            g.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            g.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            g.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, System.Drawing.Rectangle rect, float startAngle, float sweepAngle)
        {
            g.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        public void DrawEllipse(Pen pen, float x, float y, float width, float height)
        {
            g.DrawEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            g.DrawEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse(Pen pen, System.Drawing.RectangleF rect)
        {
            g.DrawEllipse(pen, rect);
        }

        public void DrawEllipse(Pen pen, System.Drawing.Rectangle rect)
        {
            g.DrawEllipse(pen, rect);
        }

        public void DrawImage(OImage image, System.Drawing.RectangleF rect)
        {
            g.DrawImage(image.image , rect);
        }

        public void DrawImage(OImage image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit)
        {
            g.DrawImage(image.image , destRect, srcRect, srcUnit);
        }

        public void DrawImage(OImage image, System.Drawing.Rectangle rect)
        {
            g.DrawImage(image.image , rect);
        }

        public void DrawImage(OImage image, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit)
        {
            g.DrawImage(image.image , destRect, srcRect, srcUnit);
        }

        public void DrawImage(OImage image, System.Drawing.Point[] destPoints)
        {
            g.DrawImage(image.image , destPoints);
        }

        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, GraphicsUnit unit, ImageAttributes ia)
        {
            g.DrawImage(image.image, rect, x, y, Width, Height, unit, ia);
        }

        public void DrawImage(OImage img, int Left, int Top)
        {
            g.DrawImage(img.image, Left, Top);
        }

        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height, GraphicsUnit unit)
        {
            g.DrawImage(img.image, rect, x, y, width, height, unit);
        }

        public void DrawImageUnscaled(OImage image, int x, int y, int width, int height)
        {
            g.DrawImageUnscaled(image.image , x, y, width, height);
        }

        public void DrawImageUnscaled(OImage image, System.Drawing.Rectangle rect)
        {
            g.DrawImageUnscaled(image.image, rect);
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLine(Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            g.DrawLine(pen, pt1, pt2);
        }

        public void DrawPolygon(Pen pen, System.Drawing.Point[] points)
        {
            g.DrawPolygon(pen, points);
        }

        public void DrawRectangle(Pen pen, System.Drawing.Rectangle rect)
        {
            g.DrawRectangle(pen, rect);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            g.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawString(string s, System.Drawing.Font font, Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat format)
        {
            g.DrawString(s, font, brush, layoutRectangle, format);
        }

        public void DrawString(string s, System.Drawing.Font font, Brush brush, System.Drawing.RectangleF layoutRectangle)
        {
            g.DrawString(s, font, brush, layoutRectangle);
        }

        public void DrawString(string str, Font f, Brush brush, PointF pt, StringFormat strFormat)
        {
            g.DrawString(str, f, brush, pt, strFormat );
        }

        public void DrawString(String str, Font ft, SolidBrush brush, Point pt)
        {
            g.DrawString(str, ft, brush, pt);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            g.FillEllipse(brush, x, y, width, height);
        }

        public void FillEllipse(Brush brush, System.Drawing.Rectangle rect)
        {
            g.FillEllipse(brush, rect);
        }
       
        public void FillEllipse(Brush brush, System.Drawing.RectangleF rect)
        {
            g.FillEllipse(brush, rect);
        }

        public void FillPolygon(Brush brush, System.Drawing.Point[] points)
        {
            g.FillPolygon(brush, points);
        }

        public void FillPolygon(Brush brush, System.Drawing.PointF[] points)
        {
            g.FillPolygon(brush, points);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            g.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle(SolidBrush brush, Rectangle rect)
        {
            g.FillRectangle(brush, rect);
        }

        public void FillRectangle(LinearGradientBrush brush, RectangleF rect)
        {
            g.FillRectangle(brush, rect);
        }

        public void Flush(FlushIntention Type)
        {
            g.Flush(Type);
        }

        public void FillRectangle(SolidBrush brush, RectangleF rect)
        {
            g.FillRectangle(brush, rect);
        }

        public void FillPath(Brush brush, GraphicsPath gp)
        {
            g.FillPath(brush, gp);
        }

        public void DrawPath(Pen p, GraphicsPath gP)
        {
            g.DrawPath(p, gP);
        }

        public void FillPath(SolidBrush brush, GraphicsPath path)
        {
            g.FillPath(brush, path);
        }

        //Fill all these in later........................

        public Region Clip
        {
            get { return g.Clip; }
            set { g.Clip = value; }
        }

        //public void SetClip(Region Reg)
        //{
        //    g.SetClip(;
        //}

        public void SetClip(Rectangle  Rect)
        {
            g.SetClip(Rect);
        }

        public void SetClip(RectangleF Rect)
        {
            g.SetClip(Rect);
        }

        public RectangleF ClipBounds
        {
            get { return g.ClipBounds; }
            set  {}
        }

        public SizeF MeasureString(String str, Font ft)
        {
            return g.MeasureString(str, ft);
        }

        public SizeF MeasureString(String str, Font ft, int width)
        {
            return g.MeasureString(str, ft, width);
        }

        public SmoothingMode  SmoothingMode
        {
            get { return g.SmoothingMode ; }
            set { g.SmoothingMode = value; }
        }
        public Region[] MeasureCharacterRanges(string text, Font font, Rectangle rect, System.Drawing.StringFormat format)
        {
            return g.MeasureCharacterRanges(text, font, rect, format );
        }

        public Region[] MeasureCharacterRanges(string text, Font font, RectangleF rect, StringFormat format)
        {
            return g.MeasureCharacterRanges(text, font, rect, format);
        }

        public void ResetTransform()
        {
            g.ResetTransform();
        }

        public void ScaleTransform(float widthScale, float heightScale)
        {
            g.ScaleTransform(widthScale, heightScale);
        }

        public void TranslateTransform(float dx, float dy)
        {
            g.TranslateTransform(dx, dy);
        }

        public System.Drawing.Drawing2D.Matrix Transform
        {
            get { return g.Transform; }
            set { g.Transform = value; }
        }

    }
}


