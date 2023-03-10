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
#if LINUX
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.ES11;
using System.Drawing.Imaging;
using OpenTK;

namespace OpenMobile.Graphics
{
    public sealed class ESGraphics : IGraphics
    {
        #region private vars
        int screen;
        static List<List<uint>> textures = new List<List<uint>>();
        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        float wscale;
        float hscale;
        int width;
        int height;
        #endregion

        internal static void DeleteTexture(int screen, uint texture)
        {
            if (screen < textures.Count)
                textures[screen].Remove(texture);
        }

        public ESGraphics(int screen)
        {
            this.screen = screen;
            lock (textures)
            {
                if (textures.Count == 0)
                    for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                        textures.Add(new List<uint>());
            }
        }


        public bool LoadTexture(ref OImage image)
        {
            if (image == null)
                return false;
            if (image.Size == Size.Empty)
                return false;

            uint texture;
            GL.GenTextures(1, out texture);
            GL.BindTexture(All.Texture2D, texture);

            Bitmap img = image.image;
            bool kill = false;
            lock (image.image)
            {
                if ((image.Width > maxTextureSize) || (image.Height > maxTextureSize))
                {
                    kill = true;
                    fixImage(ref img);
                }
                BitmapData data;
                switch (img.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        try
                        {
                            data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, data.Width, data.Height, 0,
                                        All.Rgba, All.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        try
                        {
                            data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        GL.TexImage2D(All.Texture2D, 0, (int)All.Rgb, data.Width, data.Height, 0,
                                        All.Rgb, All.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                        Bitmap tmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        try
                        {
                            if (img.HorizontalResolution == tmp.HorizontalResolution)
                                System.Drawing.Graphics.FromImage(tmp).DrawImageUnscaled(img, 0, 0);
                            else
                                System.Drawing.Graphics.FromImage(tmp).DrawImage(img, 0, 0, img.Width, img.Height);
                            data = tmp.LockBits(new System.Drawing.Rectangle(0, 0, tmp.Width, tmp.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        GL.TexImage2D(All.Texture2D, 0, (int)All.Rgb, data.Width, data.Height, 0,
                                        All.Rgb, All.UnsignedByte, data.Scan0);
                        tmp.UnlockBits(data);
                        tmp.Dispose();
                        break;
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(screen, texture);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            return true;
        }

        private void fixImage(ref Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            if (image.Width > maxTextureSize)
                width = maxTextureSize;
            if (image.Height > maxTextureSize)
                height = maxTextureSize;
            Bitmap TextureBitmap = new Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(TextureBitmap))
                g.DrawImage(image, 0, 0, width, height);
            image = TextureBitmap;
        }
        public void Clear(Color color)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(color.R, color.G, color.B, color.A);
        }
        private Rectangle _clip = NoClip;
        public Rectangle Clip
        {
            get
            {
                return _clip;
            }
            set
            {
                _clip = value;
                if (_clip == NoClip)
                    GL.Disable(EnableCap.ScissorTest);
                else
                {
                    if (_clip.Height < 0)
                        _clip.Height = 0;
                    if (_clip.Width < 0)
                        _clip.Width = 0;
                    GL.Enable(EnableCap.ScissorTest);
                    try
                    {
                        GL.Scissor((int)(_clip.X * wscale), (int)((600 - _clip.Y - _clip.Height) * hscale), (int)(_clip.Width * wscale), (int)(_clip.Height * hscale));
                    }
                    catch (Exception)
                    {
                        _clip = NoClip;
                        GL.Disable(EnableCap.ScissorTest);
                    }
                }
            }
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);

            float yrad = height / 2F;
            float xrad = width / 2F;
            float[] arr = new float[(int)((sweepAngle + 0.5F) * 4)];
            int i = 0;
            for (float t = startAngle; t <= (startAngle + sweepAngle); t = t + 0.5F)
            {
                double rad = MathHelper.DegreesToRadians(t);
                arr[i] = x + xrad + (float)(xrad * System.Math.Cos(rad));
                arr[i + 1] = y + yrad + (float)(yrad * System.Math.Sin(rad));
                i += 2;
            }
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Float, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, arr.Length / 2);
            GL.DisableClientState(All.VertexArray);

            GL.Disable(EnableCap.LineSmooth);
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);

            float yrad = height / 2F;
            float xrad = width / 2F;
            float[] arr = new float[720];
            for (int t = 0; t < 360; t++)
            {
                double rad = MathHelper.DegreesToRadians(t);
                arr[2 * t] = x + xrad + (float)(xrad * System.Math.Cos(rad));
                arr[(2 * t) + 1] = y + yrad + (float)(yrad * System.Math.Sin(rad));
            }
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Float, 0, arr);
            GL.DrawArrays(BeginMode.LineLoop, 0, 360);
            GL.DisableClientState(All.VertexArray);

            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawPoint(Color color, int x, int y, int width, int height)
        {
            // Not implemented
        }


        public void DrawImage(OImage image, Rectangle rect, Rectangle srcRect)
        {
            DrawImage(image, rect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 1F);
        }

        public void DrawImage(OImage img, Rectangle rect, int x, int y, int width, int height)
        {
            DrawImage(img, rect, x, y, width, height, 1F);
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency, eAngle.Normal);
        }

        public void DrawImage(OImage image, Rectangle rect, float transparency, eAngle angle)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height, transparency, angle);
        }

        public void DrawImage(OImage image, Rectangle rect, int x, int y, int Width, int Height, float transparency)
        {
            if ((Width == 0) || (Height == 0))
                return;
            Rectangle clp = Clip;
            SetClipFast(rect.X, rect.Y, rect.Width, rect.Height);
            float xs = ((float)rect.Width / Width), ys = ((float)rect.Height / Height);
            DrawImage(image, rect.X - (int)(xs * x), rect.Y - (int)(y * ys), (int)(xs * (image.Width - x)), (int)(ys * (image.Height - y)), transparency, eAngle.Normal);
            Clip = clp;
        }

        public void DrawImage(OImage image, Rectangle rect)
        {
            DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height)
        {
            DrawImage(image, X, Y, Width, Height, 1F, eAngle.Normal);
        }
        int[] normalTex;
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle, Vector3 rotation)
        {
        }
        public void DrawImage(OImage image, int X, int Y, int Z, int Width, int Height, float transparency, eAngle angle, Vector3 rotation, ReflectionsData reflectionData)
        {
        }
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            if (image == null)
                return;
            GL.Enable(EnableCap.Texture2D);
            if (image.GetTexture(screen) == 0)
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(All.Texture2D, image.GetTexture(screen));
            GL.Color4(1F, 1F, 1F, transparency);

            int[] tex = new int[] { X, Height + Y, Width + X, Height + Y, X, Y, Width + X, Y };
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, tex);
            GL.EnableClientState(All.TextureCoordArray);
            switch (angle)
            {
                case eAngle.FlipHorizontal:
                    GL.TexCoordPointer(2, All.Short, 0, horizTex);
                    break;
                case eAngle.FlipVertical:
                    GL.TexCoordPointer(2, All.Short, 0, vertTex);
                    break;
                default:
                    GL.TexCoordPointer(2, All.Short, 0, normalTex);
                    break;
            }
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableClientState(All.TextureCoordArray);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.Texture2D);
        }

        public void DrawImage(OImage image, Point[] destPoints)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            DrawImage(image, X, Y, Width, Height, transparency, eAngle.Normal);
        }

        public void DrawCube(OImage[] image, int x, int y, int z, double width, double height, int depth, Vector3 rotation)
        {
        }

        public void DrawLine(Pen pen, Point[] points)
        {
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer<Point>(2, All.Short, 0, points);
            GL.DrawArrays(BeginMode.LineStrip, 0, points.Length);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
        }
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            int[] arr = new int[] { x1, y1, x2, y2 };
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.Lines, 0, 2);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            float[] arr = new float[] { x1, y1, x2, y2 };
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Float, 0, arr);
            GL.DrawArrays(BeginMode.Lines, 0, 2);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            DrawLine(pen, points);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            float[] ar = new float[] { x, y, x + width, y, x + width, y + height, x, y + height };
            GL.EnableClientState(All.VertexArray);
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.LineWidth(pen.Width);
            GL.VertexPointer(2, All.Float, 0, ar);
            GL.DrawArrays(BeginMode.LineLoop, 0, 4);
            GL.DisableClientState(All.VertexArray);
        }

        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawReflection(int X, int Y, int Width, int Height, OImage image, float percent, float angle)
        {
            //throw new NotImplementedException();
        }

        public void DrawRoundRectangle(Pen pen, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            GL.Color4(pen.Color.R, pen.Color.G, pen.Color.B, pen.Color.A);
            GL.LineWidth(pen.Width);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            int[] arr = new int[] { x, y + radius, x, y + height - radius, x + radius - 1, y, x + width - radius, y, x + width, y + radius - 1, x + width, y + height - radius, x + radius, y + height, x + width - radius, y + height };
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.Lines, 0, 8);
            float cX = x + radius, cY = y + radius;
            arr = new int[64];
            int count = 0;
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, 32);

            cX = x + width - radius;
            count = 0;
            for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, 32);
            cY = y + height - radius;
            count = 0;
            for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            cX = x + radius;
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, 32);
            count = 0;
            for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, 32);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            DrawRoundRectangle(p, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Color4(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
            int[] arr = new int[722];
            arr[720] = x + (width / 2);
            arr[721] = y + (height / 2);
            for (int angle = 0; angle < 360; angle++)
            {
                arr[angle * 2] = (int)(x + (width / 2) + System.Math.Sin(angle) * (width / 2));
                arr[(angle * 2) + 1] = (int)(y + (height / 2) + System.Math.Cos(angle) * (height / 2));
            }
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 361);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public void FillEllipse(Brush brush, Rectangle rect)
        {
            FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            int[] arr = new int[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                arr[2 * i] = points[i].X;
                arr[(2 * i) + 1] = points[i].Y;
            }
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, points.Length);
            GL.DisableClientState(All.VertexArray);
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            if (brush.Gradient == Gradient.Horizontal)
                FillHorizRectange(brush, x, y, width, height);
            else if (brush.Gradient == Gradient.Vertical)
                FillVertRectange(brush, x, y, width, height);
            else
                FillRectangleSolid(brush.Color, x, y, width, height);
        }
        private void FillHorizRectange(Brush b, float x, float y, float width, float height)
        {
            float[] ar = new float[] { x, y, x + width, y, x, y + height, x + width, y + height };
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.ColorArray);
            GL.ColorPointer(4, All.UnsignedByte, 0, new byte[] { b.Color.R, b.Color.G, b.Color.B, b.Color.A, b.SecondColor.R, b.SecondColor.G, b.SecondColor.B, b.SecondColor.A, b.Color.R, b.Color.G, b.Color.B, b.Color.A, b.SecondColor.R, b.SecondColor.G, b.SecondColor.B, b.SecondColor.A });
            GL.VertexPointer(2, All.Float, 0, ar);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableClientState(All.ColorArray);
            GL.DisableClientState(All.VertexArray);
        }
        private void FillVertRectange(Brush b, float x, float y, float width, float height)
        {
            float[] ar = new float[] { x, y, x + width, y, x, y + height, x + width, y + height };
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.ColorArray);
            GL.ColorPointer(4, All.UnsignedByte, 0, new byte[] { b.Color.R, b.Color.G, b.Color.B, b.Color.A, b.Color.R, b.Color.G, b.Color.B, b.Color.A, b.SecondColor.R, b.SecondColor.G, b.SecondColor.B, b.SecondColor.A, b.SecondColor.R, b.SecondColor.G, b.SecondColor.B, b.SecondColor.A });
            GL.VertexPointer(2, All.Float, 0, ar);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableClientState(All.ColorArray);
            GL.DisableClientState(All.VertexArray);
        }
        private void FillRectangleSolid(Color c, float x, float y, float width, float height)
        {
            float[] ar = new float[] { x, y, x + width, y, x, y + height, x + width, y + height };
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.ColorArray);
            GL.ColorPointer(4, All.UnsignedByte, 0, new byte[] { c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A });
            GL.VertexPointer(2, All.Float, 0, ar);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableClientState(All.ColorArray);
            GL.DisableClientState(All.VertexArray);
        }

        public void FillRectangle(GradientData gradient, Rectangle rect, float opacity)
        {
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            if (brush.Gradient == Gradient.None)
                FillSolidRoundRectangle(brush.Color, x, y, width, height, radius);
            else if (brush.Gradient == Gradient.Horizontal)
                FillHorizRoundRectangle(brush, x, y, width, height, radius);
            else
                FillVertRoundRectangle(brush, x, y, width, height, radius);
        }
        private void FillHorizRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            //TODO-Implement
            FillSolidRoundRectangle(brush.Color, x, y, width, height, radius);
        }
        private void FillVertRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            int[] arr = new int[] 
            { 
                x + width - radius, y,//8
                x + radius, y, //7
                x + width, y + radius, //6
                x, y + radius, //5
                x + width, y + height - radius, //4
                x, y + height - radius, //3
                x + width - radius, y + height, //1
                x + radius, y + height, //2
            };
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.ColorArray);
            Byte[] colors = new Byte[] { brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A };
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.ColorPointer(4, All.UnsignedByte, 0, colors);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 8);
            GL.DisableClientState(All.ColorArray);
            float cX = x + radius, cY = y + radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            int count = 2;
            GL.Color4(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);

            cX = x + width - radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            GL.Color4(brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A);
            arr = new int[66];
            arr[0] = (int)cX;
            cY = y + height - radius;
            arr[1] = (int)cY;
            count = 2;
            for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            cX = x + radius;
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            if (brush.Gradient == Gradient.None)
                FillSolidRoundRectangle(brush.Color, rect.X, rect.Y, rect.Width, rect.Height, radius);
            else
                FillRoundRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        private void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            GL.Color4(color.R, color.G, color.B, color.A);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            int[] arr = new int[] 
            { 
                x + width - radius, y,//8
                x + radius, y, //7
                x + width, y + radius, //6
                x, y + radius, //5
                x + width, y + height - radius, //4
                x, y + height - radius, //3
                x + width - radius, y + height, //1
                x + radius, y + height, //2
            };
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 8);
            float cX = x + radius, cY = y + radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            int count = 2;
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);

            cX = x + width - radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            arr = new int[66];
            arr[0] = (int)cX;
            cY = y + height - radius;
            arr[1] = (int)cY;
            count = 2;
            for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            cX = x + radius;
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            GL.VertexPointer(2, All.Short, 0, arr);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 33);
            GL.DisableClientState(All.VertexArray);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
        }

        public OImage GenerateStringTexture(string s, Font font, Color color, int Left, int Top, int Width, int Height, System.Drawing.StringFormat format)
        {
            throw new NotImplementedException();
        }

        public OImage GenerateTextTexture(int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color color, Color secondColor)
        {
            throw new NotImplementedException();
        }
        int maxTextureSize;
        int[] horizTex;
        int[] vertTex;
        public void Initialize(int screen)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1000, 600, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
            //Init arrays
            normalTex = new int[] { 0, 1, 1, 1, 0, 0, 1, 0 };
            vertTex = new int[] { 0, 0, 1, 0, 0, 1, 1, 1 };
            horizTex = new int[] { 1, 1, 0, 1, 1, 0, 0, 0 };
        }
        public void Begin()
        {
        }
        public void End()
        {
            if (textures[screen].Count > 0)
            {
                GL.DeleteTextures(textures[screen].Count, textures[screen].ToArray());
                textures[screen].Clear();
            }
        }
        public int MaxTextureSize
        {
            get
            {
                return maxTextureSize;
            }
        }
        public void renderText(System.Drawing.Graphics g, int x, int y, int w, int h, string text, Font font, eTextFormat format, Alignment alignment, Color c, Color sC)
        {
            throw new NotImplementedException();
        }

        public void ResetClip()
        {
            Clip = NoClip;
        }

        public void ResetTransform()
        {
            GL.LoadIdentity();
        }

        public void Resize(int left, int top, int width, int height)
        {
            this.width = width;
            this.height = height;
            wscale = (width / 1000F);
            hscale = (height / 600F);
            GL.Viewport(0, 0, width, height);
        }

        int _Left;
        int _Top;

        public void Location(int left, int top)
        {
            _Left = left;
            _Top = top;
        }

        public void SetClip(Rectangle Rect)
        {
            Clip = Rect;
        }

        public void SetClipFast(int x, int y, int width, int height)
        {
            if (height < 0)
                height = 0;
            if (width < 0)
                width = 0;
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor((int)(x * wscale), (int)((600 - y - height) * hscale), (int)(width * wscale), (int)(height * hscale));
        }

        public void Translate(double dx, double dy)
        {
            GL.Translate((float)dx, (float)dy, 0);
        }

        public void Translate(double dx, double dy, double dz)
        {
            GL.Translate((float)dx, (float)dy, (float)dz);
        }

        public void Rotate(double angle, Graphics.Axis axis)
        {
            GL.Rotate((float)angle, (axis == Graphics.Axis.X ? 1 : 0), (axis == Graphics.Axis.Y ? 1 : 0), (axis == Graphics.Axis.Z ? 1 : 0));
        }

        public void Rotate(double x, double y, double z)
        {
            GL.Rotate((float)x, 1, 0, 0);
            GL.Rotate((float)y, 0, 1, 0);
            GL.Rotate((float)z, 0, 0, 1);
        }

        public void Scale(double sx, double sy, double sz)
        {
            GL.Scale((float)sx, (float)sy, (float)sz);
        }
        public void Transform(Matrix4d m)
        {
            //Raw.MultMatrix(ref m);
        }

        public void _3D_ModelView_Set(Vector3d cameraLocation, Vector3d cameraRotation, Vector3d cameraOffset, double zoom, bool activateMatrix)
        {
            // Not implemented
        }

        public void _3D_ModelView_Push()
        {
            GL.PushMatrix();
        }

        public void _3D_ModelView_Pop()
        {
            GL.PopMatrix();
        }

        /// <summary>
        /// Sets or gets the modelview matrix
        /// </summary>
        public Matrix4d _3D_ModelViewMatrix
        {
            get
            {
                return this.modelview;
            }
            set
            {
                this.modelview = value;
            }
        }

        /// <summary>
        /// The matrix for the global modelview
        /// </summary>
        private Matrix4d modelview;

        /// <summary>
        /// Activates the model view
        /// </summary>
        public void _3D_ModelView_ResetAndPlaceCamera(Vector3d cameraLocation)
        {
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadIdentity();
            //GL.LoadMatrix(ref modelview);
        }

    }
}
#endif