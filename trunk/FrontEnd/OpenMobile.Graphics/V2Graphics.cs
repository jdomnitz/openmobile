using System;
using System.Collections.Generic;
using System.Drawing;
using OpenMobile.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenMobile.Math;

namespace OpenMobile.Graphics
{
    public sealed class V2Graphics:IGraphics
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
                textures[screen].Add(texture);
        }

        public V2Graphics(int screen)
        {
            this.screen = screen;
            lock (textures)
            {
                if (textures.Count == 0)
                    for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                        textures.Add(new List<uint>());
            }
        }


        private bool loadTexture(ref OImage image)
        {
            if (image == null)
                return false;
            if (image.Size == Size.Empty)
                return false;

            uint texture;
            Raw.GenTextures(1, out texture);
            Raw.BindTexture(TextureTarget.Texture2D, texture);
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
                if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    try
                    {
                        data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    catch (InvalidOperationException) { return false; }
                    Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                                    OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    img.UnlockBits(data);
                }
                else if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    try
                    {
                        data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    catch (InvalidOperationException) { return false; }
                    Raw.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                    OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    img.UnlockBits(data);
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(screen, texture);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            Raw.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            return true;
        }

        private void fixImage(ref Bitmap image)
        {
            int width=image.Width;
            int height=image.Height;
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
            Raw.Clear(ClearBufferMask.ColorBufferBit);
            Raw.ClearColor(color);
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
                    Raw.Disable(EnableCap.ScissorTest);
                else
                {
                    if (_clip.Height < 0)
                        _clip.Height = 0;
                    if (_clip.Width < 0)
                        _clip.Width = 0;
                    Raw.Enable(EnableCap.ScissorTest);
                    try
                    {
                        Raw.Scissor((int)(_clip.X * wscale), (int)((600 - _clip.Y - _clip.Height) * hscale), (int)(_clip.Width * wscale), (int)(_clip.Height * hscale));
                    }
                    catch (Exception)
                    {
                        _clip = NoClip;
                        Raw.Disable(EnableCap.ScissorTest);
                    }
                }
            }
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.Multisample);

            float yrad = height / 2F;
            float xrad = width / 2F;
            float[] arr = new float[(int)((sweepAngle + 0.5F) * 4)];
            int i = 0;
            for (float t = startAngle; t <= (startAngle + sweepAngle); t = t + 0.5F)
            {
                float rad = MathHelper.DegreesToRadians(t);
                arr[i] = x + xrad + (float)(xrad * System.Math.Cos(rad));
                arr[i + 1] = y + yrad + (float)(yrad * System.Math.Sin(rad));
                i += 2;
            }
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Float, 0, arr);
            Raw.DrawArrays(BeginMode.LineStrip, 0, arr.Length / 2);
            Raw.DisableClientState(ArrayCap.VertexArray);

            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.Multisample);
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);

            float yrad = height / 2F;
            float xrad = width / 2F;
            float[] arr = new float[720];
            for (int t = 0; t < 360; t++)
            {
                float rad = MathHelper.DegreesToRadians(t);
                arr[2*t]=x + xrad + (float)(xrad * System.Math.Cos(rad));
                arr[(2*t)+1]=y + yrad + (float)(yrad * System.Math.Sin(rad));
            }
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Float, 0, arr);
            Raw.DrawArrays(BeginMode.LineLoop, 0, 360);
            Raw.DisableClientState(ArrayCap.VertexArray);

            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
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
            DrawImage(image, rect.X - (int)(xs*x), rect.Y - (int)(y*ys), (int)(xs*(image.Width-x)), (int)(ys*(image.Height-y)), transparency,eAngle.Normal);
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
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            if (image == null)
                return;
            Raw.Enable(EnableCap.Texture2D);
            if (image.Texture(screen) == 0)
                if (!loadTexture(ref image))
                {
                    Raw.Disable(EnableCap.Texture2D);
                    return;
                }
            Raw.BindTexture(TextureTarget.Texture2D, image.Texture(screen));
            Raw.Color4(1F, 1F, 1F, transparency);

            int[] tex = new int[] { X, Height + Y, Width + X, Height + Y, X, Y, Width + X, Y };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, tex);
            Raw.EnableClientState(ArrayCap.TextureCoordArray);
            switch(angle)
            {
                case eAngle.FlipHorizontal:
                    Raw.TexCoordPointer(2, TexCoordPointerType.Int, 0, horizTex);
                    break;
                case eAngle.FlipVertical:
                    Raw.TexCoordPointer(2, TexCoordPointerType.Int, 0, vertTex);
                    break;
                default:
                    Raw.TexCoordPointer(2, TexCoordPointerType.Int, 0, normalTex);
                    break;
            }
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            Raw.DisableClientState(ArrayCap.TextureCoordArray);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.Texture2D);
        }

        public void DrawImage(OImage image, Point[] destPoints)
        {
            //throw new NotImplementedException();
        }

        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency)
        {
            DrawImage(image, X, Y, Width, Height, transparency, eAngle.Normal);
        }
        public void DrawLine(Pen pen, Point[] points)
        {
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Color4(pen.Color);
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer<Point>(2,VertexPointerType.Int, 0, points);
            Raw.DrawArrays(BeginMode.LineStrip, 0, points.Length);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
        }
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            int[] arr = new int[] { x1, y1, x2, y2 };
            Raw.Color4(pen.Color);
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.Lines, 0, 2);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            float[] arr = new float[] { x1, y1, x2, y2 };
            Raw.Color4(pen.Color);
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Float, 0, arr);
            Raw.DrawArrays(BeginMode.Lines, 0, 2);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            //throw new NotImplementedException();
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            float[] ar = new float[] { x, y, x + width, y, x + width, y + height, x, y + height };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.Color4(pen.Color);
            Raw.VertexPointer(2, VertexPointerType.Float, 0, ar);
            Raw.DrawArrays(BeginMode.LineLoop, 0, 4);
            Raw.DisableClientState(ArrayCap.VertexArray);
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
            Raw.Color4(pen.Color);
            Raw.LineWidth(pen.Width);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            int[] arr = new int[] { x, y + radius, x, y + height - radius, x + radius-1, y, x + width - radius, y, x + width, y + radius-1, x + width, y + height - radius, x + radius, y + height, x + width - radius, y + height };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.Lines, 0, 8);
            float cX = x + radius, cY = y + radius;
            arr = new int[64];
            int count = 0;
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.LineStrip, 0, 32);

            cX = x + width - radius;
            count = 0;
            for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.LineStrip, 0, 32);
            cY = y + height - radius;
            count = 0;
            for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            cX = x + radius;
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.LineStrip, 0, 32);
            count = 0;
            for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.LineStrip, 0, 32);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }

        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            DrawRoundRectangle(p, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
            Raw.Enable(EnableCap.Multisample);
            Raw.Color4(brush.Color);
            int[] arr=new int[722];
            arr[720]=x + (width / 2);
            arr[721]=y + (height / 2);
            for (int angle = 0; angle < 360; angle++)
            {
                arr[angle*2]=(int)(x + (width / 2) + System.Math.Sin(angle) * (width / 2));
                arr[(angle*2)+1]= (int)(y + (height / 2) + System.Math.Cos(angle) * (height / 2));
            }
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 361);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
            Raw.Disable(EnableCap.Multisample);
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
                arr[(2 * i)+1] = points[i].Y;
            }
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, points.Length);
            Raw.DisableClientState(ArrayCap.VertexArray);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            //TODO-Gradient
            FillRectangleSolid(brush.Color, x, y, width, height);
        }

        private void FillRectangleSolid(Color c, float x, float y, float width, float height)
        {
            float[] ar = new float[] { x, y, x + width, y, x, y + height, x + width, y + height };
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.EnableClientState(ArrayCap.ColorArray);
            Raw.ColorPointer(4, ColorPointerType.UnsignedByte, 0, new byte[] { c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A, c.R, c.G, c.B, c.A });
            Raw.VertexPointer(2, VertexPointerType.Float, 0, ar);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            Raw.DisableClientState(ArrayCap.ColorArray);
            Raw.DisableClientState(ArrayCap.VertexArray);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            if (brush.Gradient == Gradient.None)
                FillSolidRoundRectangle(brush.Color, x, y, width, height, radius);
            else //if(brush.Gradient==Gradient.Horizontal)
                FillVertRoundRectangle(brush, x, y, width, height, radius);
        }

        private void FillVertRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
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
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.EnableClientState(ArrayCap.ColorArray);
            Byte[] colors = new Byte[] { brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A, brush.SecondColor.R, brush.SecondColor.G, brush.SecondColor.B, brush.SecondColor.A };
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.ColorPointer(4, ColorPointerType.UnsignedByte, 0, colors);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 8);
            Raw.DisableClientState(ArrayCap.ColorArray);
            float cX = x + radius, cY = y + radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            int count = 2;
            Raw.Color4(brush.Color);
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count] = (int)(radius * System.Math.Cos(ang) + cX);
                arr[count + 1] = (int)(radius * System.Math.Sin(ang) + cY);
                count += 2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);

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
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            Raw.Color4(brush.SecondColor);
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
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
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
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }

        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            if (brush.Gradient == Gradient.None)
                FillSolidRoundRectangle(brush.Color, rect.X, rect.Y, rect.Width, rect.Height, radius);
            else
                FillRoundRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        public void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            double ang = 0;
            Raw.Color4(color);
            Raw.Enable(EnableCap.LineSmooth);
            Raw.Enable(EnableCap.PointSmooth);
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
            Raw.EnableClientState(ArrayCap.VertexArray);
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleStrip, 0, 8);
            float cX = x + radius, cY = y + radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            int count = 2;
            for (ang = System.Math.PI; ang <= (1.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count]=(int)(radius * System.Math.Cos(ang) + cX);
                arr[count+1]=(int)(radius * System.Math.Sin(ang) + cY);
                count+=2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            
            cX = x + width - radius;
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (1.5 * System.Math.PI); ang <= (2 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count]=(int)(radius * System.Math.Cos(ang) + cX);
                arr[count+1]=(int)(radius * System.Math.Sin(ang) + cY);
                count+=2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            arr = new int[66];
            arr[0] = (int)cX;
            cY = y + height - radius;
            arr[1] = (int)cY;
            count = 2;
            for (ang = 0; ang <= (0.5 * System.Math.PI); ang = ang + 0.05)
            {
                arr[count]=(int)(radius * System.Math.Cos(ang) + cX);
                arr[count+1]=(int)(radius * System.Math.Sin(ang) + cY);
                count+=2;
            }
            cX = x + radius;
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            arr = new int[66];
            arr[0] = (int)cX;
            arr[1] = (int)cY;
            count = 2;
            for (ang = (0.5 * System.Math.PI); ang <= System.Math.PI; ang = ang + 0.05)
            {
                arr[count]=(int)(radius * System.Math.Cos(ang) + cX);
                arr[count+1]=(int)(radius * System.Math.Sin(ang) + cY);
                count+=2;
            }
            Raw.VertexPointer(2, VertexPointerType.Int, 0, arr);
            Raw.DrawArrays(BeginMode.TriangleFan, 0, 33);
            Raw.DisableClientState(ArrayCap.VertexArray);
            Raw.Disable(EnableCap.LineSmooth);
            Raw.Disable(EnableCap.PointSmooth);
        }

        public void Finish()
        {
            if (textures[screen].Count > 0)
            {
                Raw.DeleteTextures(textures[screen].Count, textures[screen].ToArray());
                textures[screen].Clear();
            }
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
            Raw.Disable(EnableCap.DepthTest);
            Raw.Disable(EnableCap.Multisample);
            Raw.Enable(EnableCap.Blend);
            Raw.Disable(EnableCap.Dither); //Necessary?
            Raw.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            Raw.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Raw.MatrixMode(MatrixMode.Projection);
            Raw.LoadIdentity();
            Raw.Ortho(0, 1000, 600, 0, 0, 1);
            Raw.MatrixMode(MatrixMode.Modelview);
            Raw.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
            //Init arrays
            normalTex = new int[] { 0, 1, 1, 1, 0, 0, 1, 0 };
            vertTex = new int[] { 0, 0, 1, 0, 0, 1, 1, 1 };
            horizTex = new int[] { 1, 1, 0, 1, 1, 0, 0, 0 };
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
            Raw.LoadIdentity();
        }

        public void Resize(int Width, int Height)
        {
            width = Width;
            height = Height;
            wscale = (width / 1000F);
            hscale = (height / 600F);
            Raw.Viewport(0, 0, width, height);
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
            Raw.Enable(EnableCap.ScissorTest);
            Raw.Scissor((int)(x * wscale), (int)((600 - y - height) * hscale), (int)(width * wscale), (int)(height * hscale));
        }

        public void TranslateTransform(float dx, float dy)
        {
            Raw.Translate(dx, dy, 0);
        }

        public void TranslateTransform(float dx, float dy, float dz)
        {
            Raw.Translate(dx, dy, dz);
        }
    }
}
