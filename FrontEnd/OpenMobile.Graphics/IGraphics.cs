using System;
namespace OpenMobile.Graphics
{
    interface IGraphics
    {
        void Clear(Color color);
        Rectangle Clip { get; set; }
        void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
        void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle);
        void DrawEllipse(Pen pen, int x, int y, int width, int height);
        void DrawEllipse(Pen pen, Rectangle rect);
        void DrawImage(OImage image, Rectangle rect, Rectangle srcRect);
        void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height);
        void DrawImage(OImage image, Rectangle rect, float transparency);
        void DrawImage(OImage image, Rectangle rect, float transparency, eAngle angle);
        void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency);
        void DrawImage(OImage image, Rectangle rect);
        void DrawImage(OImage image, int X, int Y, int Width, int Height);
        void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle);
        void DrawImage(OImage image, Point[] destPoints);
        void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency);
        void DrawLine(Pen pen, int x1, int y1, int x2, int y2);
        void DrawLine(Pen pen, Point pt1, Point pt2);
        void DrawLine(Pen pen, float x1, float y1, float x2, float y2);
        void DrawLine(Pen pen, Point[] points);
        void DrawPolygon(Pen pen, Point[] points);
        void DrawRectangle(Pen pen, int x, int y, int width, int height);
        void DrawRectangle(Pen pen, Rectangle rect);
        void DrawReflection(int X, int Y, int Width, int Height, OImage image, float percent, float angle);
        void DrawRoundRectangle(Pen pen, int x, int y, int width, int height, int radius);
        void DrawRoundRectangle(Pen p, Rectangle rect, int radius);
        void FillEllipse(Brush brush, int x, int y, int width, int height);
        void FillEllipse(Brush brush, Rectangle rect);
        void FillPolygon(Brush brush, Point[] points);
        void FillRectangle(Brush brush, float x, float y, float width, float height);
        void FillRectangle(Brush brush, Rectangle rect);
        void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius);
        void FillRoundRectangle(Brush brush, Rectangle rect, int radius);
        void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius);
        void Finish();
        OImage GenerateStringTexture(string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format);
        OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor);
        void Initialize(int screen);
        void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC);
        void ResetClip();
        void ResetTransform();
        void Resize(int Width, int Height);
        void SetClip(Rectangle Rect);
        void SetClipFast(int x, int y, int width, int height);
        void TranslateTransform(float dx, float dy);
        void TranslateTransform(float dx, float dy, float dz);
    }
}
