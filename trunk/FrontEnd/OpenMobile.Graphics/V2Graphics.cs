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
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenTK;

namespace OpenMobile.Graphics
{
    public sealed class V2Graphics:IGraphics
    {
        #region private vars
        int _Screen;
        static List<List<uint>> _Textures = new List<List<uint>>();
        public static Rectangle NoClip = new Rectangle(0, 0, 1000, 600);
        float _WidthScale;
        float _HeightScale;
        int _Width;
        int _Height;

        GameWindow _TargetWindow;
        MouseData _MouseData;

        #endregion

        internal static void DeleteTexture(int screen, uint texture)
        {
            if (screen < _Textures.Count)
                _Textures[screen].Add(texture);
                //textures[screen].Remove(texture);
            GL.DeleteTexture(texture);
        }

        public V2Graphics(int screen, GameWindow targetWindow, MouseData mouseData)
        {
            this._TargetWindow = targetWindow;
            this._MouseData = mouseData;
            this._Screen = screen;
            lock (_Textures)
            {
                if (_Textures.Count == 0)
                    for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                        _Textures.Add(new List<uint>());
            }
        }
        public bool LoadTexture(ref OImage image)
        {
            if (image == null || image.Size == Size.Empty)
                return false;

            // Load texture
            uint texture = image.GetTexture(_Screen);

            // Delete old texture before generating a new one
            if (texture != 0)
                GL.DeleteTexture(texture);

            // Create new texture name
            GL.GenTextures(1, out texture);

            // Bind texture to opengl
            GL.BindTexture(TextureTarget.Texture2D, texture);

            Bitmap img = image.image;
            bool kill = false;
            lock (image.image)
            {
                // Ensure we don't go outside graphic cards limits
                if ((image.Width > maxTextureSize) || (image.Height > maxTextureSize))
                {
                    kill = true;
                    fixImage(ref img);
                }

                // Extract data from bitmap and load to opengl
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
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;

                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        try
                        {
                            data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                        OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        img.UnlockBits(data);
                        break;

                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                        Bitmap tmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        try
                        {
                            if (img.HorizontalResolution==tmp.HorizontalResolution)
                                System.Drawing.Graphics.FromImage(tmp).DrawImageUnscaled(img, 0, 0);
                            else
                                System.Drawing.Graphics.FromImage(tmp).DrawImage(img, 0, 0,img.Width,img.Height);
                            data = tmp.LockBits(new System.Drawing.Rectangle(0, 0, tmp.Width, tmp.Height),
                            ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        catch (InvalidOperationException) { return false; }
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                                        OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        tmp.UnlockBits(data);
                        tmp.Dispose();
                        break;
                }
            }
            if (kill)
                img.Dispose();
            image.SetTexture(_Screen, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToBorder);
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
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(color.ToSystemColor());
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
                        GL.Scissor((int)(_clip.X * _WidthScale), (int)((600 - _clip.Y - _clip.Height) * _HeightScale), (int)(_clip.Width * _WidthScale), (int)(_clip.Height * _HeightScale));
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
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.Color4(pen.Color.ToSystemColor());
            GL.LineWidth(pen.Width);

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
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Float, 0, arr);
            GL.DrawArrays(BeginMode.LineStrip, 0, arr.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();
        }
        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }


        int[] normalTex;
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
            if (image == null)
                return;
            
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
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle)
        {
            DrawImage(image, X, Y, Width, Height, transparency, angle, Vector3.Zero);
        }
        public void DrawImage(OImage image, int X, int Y, int Width, int Height, float transparency, eAngle angle, Vector3 rotation)
        {
            DrawImage(image, X, Y, 0, Width, Height, transparency, angle, rotation, null);
        }
        public void DrawImage(OImage image, int X, int Y, int Z, int Width, int Height, float transparency, eAngle angle, Vector3 rotation, ReflectionsData reflectionData)
        {
            if (image == null)
                return;

            // Activate reflection?
            bool useReflection = reflectionData != null;

            GL.Enable(EnableCap.Texture2D);
            if (image.TextureGenerationRequired(_Screen))
                if (!LoadTexture(ref image))
                {
                    GL.Disable(EnableCap.Texture2D);
                    return;
                }
            GL.BindTexture(TextureTarget.Texture2D, image.GetTexture(_Screen));

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToBorder);

            double width2 = Width / 2.0;
            double height2 = Height / 2.0;

            // Should we use a shader effect?
            if (image.ShaderEffect != OMShaders.None)
            {   // Yes
                V2Shaders.ActivateShader(_TargetWindow, _MouseData, image.ShaderEffect, _Width, _Height);
            }

            if (useReflection)
            {
                double reflectionImageHeight = height2 * reflectionData.ReflectionLength;
                double reflectionImageHeight2 = reflectionImageHeight / 2;
                double orgImageHeight = Height - reflectionImageHeight;
                double orgImageHeight2 = orgImageHeight / 2;

                double height4 = Height / 4.0;
                double reflectionLength = Height * reflectionData.ReflectionLength;
                double reflectionLength2 = reflectionLength / 2;

                // Save current matrix
                GL.PushMatrix();

                // Translate base point from upper left corner to place graphics correctly
                _3D_Translate(X + width2, Y + orgImageHeight2, Z);

                // Rotate graphics
                _3D_Rotate(rotation.X, rotation.Y, rotation.Z);

                // Scale graphics to correct dimensions
                GL.Scale(width2, orgImageHeight2, 1);

                // Render texture
                GL.Color4(1F, 1F, 1F, transparency);
                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1, -1, 0); //TOP-LEFT
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1, -1, 0); //TOP-RIGHT
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, 1, 0); //BOTTOM-RIGHT
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, 1, 0); //BOTTOM-LEFT
                GL.End();

                // Restore matrix
                GL.PopMatrix();

                // Render reflection

                // Save current matrix
                GL.PushMatrix();

                // Translate base point from upper left corner to place graphics correctly
                _3D_Translate(X + width2, Y + orgImageHeight + orgImageHeight2 + reflectionData.ReflectionOffset, Z);

                // Rotate graphics
                _3D_Rotate(-rotation.X, rotation.Y, -rotation.Z);

                // Scale graphics to correct dimensions
                GL.Scale(width2, orgImageHeight2, 1);

                // Render texture
                //GL.Begin(BeginMode.Quads);
                //GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                //GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, -1, 0); //TOP-LEFT
                //GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                //GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, -1, 0); //TOP-RIGHT
                //GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                //GL.TexCoord2(1.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(1, 1, 0); //BOTTOM-RIGHT
                //GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                //GL.TexCoord2(0.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(-1, 1, 0); //BOTTOM-LEFT
                //GL.End();
                //GL.Begin(BeginMode.Quads);
                //GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                //GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, -reflectionData.ReflectionLength, 0); //TOP-LEFT
                //GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                //GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, -reflectionData.ReflectionLength, 0); //TOP-RIGHT
                //GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                //GL.TexCoord2(1.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(1, reflectionData.ReflectionLength, 0); //BOTTOM-RIGHT
                //GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                //GL.TexCoord2(0.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(-1, reflectionData.ReflectionLength, 0); //BOTTOM-LEFT
                //GL.End();

                double ReflectionLength2 = reflectionData.ReflectionLength * 2;

                GL.Begin(BeginMode.Quads);
                GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, -1, 0); //TOP-LEFT
                GL.Color4(reflectionData.ReflectionColorStart.Rd, reflectionData.ReflectionColorStart.Gd, reflectionData.ReflectionColorStart.Bd, transparency * reflectionData.ReflectionColorStart.Ad);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, -1, 0); //TOP-RIGHT
                GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                GL.TexCoord2(1.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(1, -1 + ReflectionLength2, 0); //BOTTOM-RIGHT
                GL.Color4(reflectionData.ReflectionColorEnd.Rd, reflectionData.ReflectionColorEnd.Gd, reflectionData.ReflectionColorEnd.Bd, transparency * reflectionData.ReflectionColorEnd.Ad);
                GL.TexCoord2(0.0f, 0.0f + (1 - reflectionData.ReflectionLength)); GL.Vertex3(-1, -1 + ReflectionLength2, 0); //BOTTOM-LEFT
                GL.End();

                // Restore matrix
                GL.PopMatrix();
            }
            else
            {
                // Save current matrix
                GL.PushMatrix();

                // Translate base point from upper left corner to place graphics correctly
                _3D_Translate(X + width2, Y + height2, Z);

                // Rotate graphics
                _3D_Rotate(rotation.X, rotation.Y, rotation.Z);

                // Scale graphics to correct dimensions
                GL.Scale(width2, height2, 1);

                // Render texture
                GL.Color4(1F, 1F, 1F, transparency);
                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1, -1, 0); //TOP-LEFT
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1, -1, 0); //TOP-RIGHT
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, 1, 0); //BOTTOM-RIGHT
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, 1, 0); //BOTTOM-LEFT
                GL.End();

                // Restore matrix
                GL.PopMatrix();
            }

            //// Render reflection (only if rotation is around Y axis)
            //if (useReflection && rotation.Z == 0 && rotation.X == 0)
            //{
            //    GL.PopMatrix();

            //    // Render reflection (flipped around the Y axis)
            //    GL.Scale(1.0, -1.0, 1.0);

            //    // Rotate image
            //    GL.Rotate(rotation.Z, 0, 0, 1);
            //    GL.Rotate(rotation.Y, 0, 1, 0);
            //    GL.Rotate(rotation.X, 1, 0, 0);

            //    // Scale dimensions to correct values
            //    GL.Scale(width2, height2, 1);

            //    // Shift image down to place the reflection correctly
            //    GL.Translate(0.0, -2.0, 0.0);

            //    // Render texture

            //    GL.Begin(BeginMode.Quads);
            //    GL.Color4(reflectionData.FadeColorEnd);
            //    GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1, -1, 0); //TOP-LEFT
            //    GL.Color4(reflectionData.FadeColorEnd);
            //    GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1, -1, 0); //TOP-RIGHT
            //    GL.Color4(reflectionData.FadeColorStart);
            //    GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, 1, 0); //BOTTOM-RIGHT
            //    GL.Color4(reflectionData.FadeColorStart);
            //    GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1, 1, 0); //BOTTOM-LEFT
            //    GL.End();
            //    GL.PopMatrix();
            //}
            //else

            // Remove any shader effect
            if (image.ShaderEffect != OMShaders.None)
            {   // Yes
                V2Shaders.DeactivateShader(image.ShaderEffect);
            }

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
            GL.PushMatrix();

            // Move reference to upper left corner 
            GL.Translate(-500, -300, 0);

            // Move cube to requested location
            GL.Translate(x + (width / 2), y + (height / 2), z);

            // Scale dimensions to correct values
            GL.Scale(width / 2, height / 2, depth / 2.0);

            // Translate backwards to ensure front of cube is located as zero as this is the calibrated size for the coordinate system
            GL.Translate(0, 0, 1);

            // Rotate image
            GL.Rotate(rotation.X, 1, 0, 0);
            GL.Rotate(rotation.Y, 0, 1, 0);
            GL.Rotate(rotation.Z, 0, 0, 1);

            bool useTexture = false;
            if (image != null && image.Length > 0)
            {
                for (int i = 0; i < image.Length; i++)
                {
                    useTexture = true;
                    if (image[i].TextureGenerationRequired(_Screen))
                        if (!LoadTexture(ref image[i]))
                        {
                            GL.Disable(EnableCap.Texture2D);
                            return;
                        }
                }
                GL.Enable(EnableCap.Texture2D);
                // Set texture parameters
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToBorder);
            }

            if (!useTexture)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            }


            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Blue
            if (useTexture && image[0] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[0].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);	// Top Right Of The Quad (Top)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);	// Top Left Of The Quad (Top)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);	// Bottom Left Of The Quad (Top)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);	// Bottom Right Of The Quad (Top)
            GL.End();

            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Orange
            if (useTexture && image[1] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[1].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);	// Top Right Of The Quad (Bottom)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);	// Top Left Of The Quad (Bottom)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);	// Bottom Left Of The Quad (Bottom)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);	// Bottom Right Of The Quad (Bottom)
            GL.End();

            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Red	
            if (useTexture && image[2] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[2].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);	// Top Right Of The Quad (Front)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);	// Top Left Of The Quad (Front)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);	// Bottom Left Of The Quad (Front)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);	// Bottom Right Of The Quad (Front)
            GL.End();

            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Yellow
            if (useTexture && image[3] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[3].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);	// Top Right Of The Quad (Back)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);	// Top Left Of The Quad (Back)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);	// Bottom Left Of The Quad (Back)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);	// Bottom Right Of The Quad (Back)
            GL.End();

            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Blue
            if (useTexture && image[4] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[4].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);	// Top Right Of The Quad (Left)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);	// Top Left Of The Quad (Left)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);	// Bottom Left Of The Quad (Left)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);	// Bottom Right Of The Quad (Left)
            GL.End();

            GL.Color3(1.0f, 1.0f, 1.0f);	// Color Violet
            if (useTexture && image[5] != null)
                GL.BindTexture(TextureTarget.Texture2D, image[5].GetTexture(_Screen));
            GL.Begin(BeginMode.Quads);
            if (useTexture) GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);	// Top Right Of The Quad (Right)
            if (useTexture) GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);	// Top Left Of The Quad (Right)
            if (useTexture) GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);	// Bottom Left Of The Quad (Right)
            if (useTexture) GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);	// Bottom Right Of The Quad (Right)
            GL.End();

            if (useTexture)
                GL.Disable(EnableCap.Texture2D);
            else
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }

            GL.PopMatrix();

        }


        public void DrawLine(Pen pen, Point[] points)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.LineWidth(pen.Width);
            GL.Color4(pen.Color.ToSystemColor());
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer<Point>(2, VertexPointerType.Int, 0, points);
            GL.DrawArrays(BeginMode.LineStrip, 0, points.Length);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();

        }
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.LineWidth(pen.Width);
            int[] arr = new int[] { x1, y1, x2, y2 };
            GL.Color4(pen.Color.ToSystemColor());
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Int, 0, arr);
            GL.DrawArrays(BeginMode.Lines, 0, 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();
        }
        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.LineWidth(pen.Width);
            float[] arr = new float[] { x1, y1, x2, y2 };
            GL.Color4(pen.Color.ToSystemColor());
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Float, 0, arr);
            GL.DrawArrays(BeginMode.Lines, 0, 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            DrawLine(pen, points);
        }
        public void FillPolygon(Brush brush, Point[] points)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.Color4(brush.Color.ToSystemColor());
            GL.Begin(BeginMode.Polygon);
            for (int i = 0; i < points.Length; i++)
                GL.Vertex2(points[i].X, points[i].Y);
            GL.End();

            // Restore matrix
            GL.PopMatrix();
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            // Calculate base points
            // Point order:
            //  1               2   
            //    -------------
            //  0 | 11      4 | 3
            //    |           |
            //    |           |
            //  9 | 10      5 | 6
            //    -------------
            //  8               7

            #region Point calculations

            PointF[] p = new PointF[12];
            p[0] = new PointF(x, y + pen.Width);
            p[1] = new PointF(x, y);
            p[2] = new PointF(x + width, y);
            p[3] = new PointF(x + width, y + pen.Width);
            p[4] = new PointF(x + width - pen.Width, y + pen.Width);
            p[5] = new PointF(x + width - pen.Width, y + height - pen.Width);
            p[6] = new PointF(x + width, y + height - pen.Width);
            p[7] = new PointF(x + width, y + height);
            p[8] = new PointF(x, y + height);
            p[9] = new PointF(x, y + height - pen.Width);
            p[10] = new PointF(x + pen.Width, y + height - pen.Width);
            p[11] = new PointF(x + pen.Width, y + pen.Width);

            #endregion

            GL.Color4(pen.Color.ToSystemColor());
            //GL.LineWidth(pen.Width);

             GL.Begin(BeginMode.QuadStrip);

            // Top line
            GL.Vertex3(p[0].X, p[0].Y, 0);
            GL.Vertex3(p[1].X, p[1].Y, 0);
            GL.Vertex3(p[3].X, p[3].Y, 0);
            GL.Vertex3(p[2].X, p[2].Y, 0);

            // Bottom line
            //GL.Color4(Color.Yellow);
            GL.Vertex3(p[6].X, p[6].Y, 0);
            GL.Vertex3(p[7].X, p[7].Y, 0);
            GL.Vertex3(p[9].X, p[9].Y, 0);
            GL.Vertex3(p[8].X, p[8].Y, 0);

            // Right line
            //GL.Color4(Color.Blue);
            GL.Vertex3(p[4].X, p[4].Y, 0);
            GL.Vertex3(p[3].X, p[3].Y, 0);
            GL.Vertex3(p[5].X, p[5].Y, 0);
            GL.Vertex3(p[6].X, p[6].Y, 0);

            // Left line
            GL.Vertex3(p[10].X, p[10].Y, 0);
            GL.Vertex3(p[9].X, p[9].Y, 0);
            GL.Vertex3(p[11].X, p[11].Y, 0);
            GL.Vertex3(p[0].X, p[0].Y, 0);

            GL.End();

            // Restore matrix
            GL.PopMatrix();
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
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            // Calculate base points
            // Point order:
            //      1           2
            //      |-----------|
            // 11 |-| 0       3 |-| 4
            //    |               |
            //    |               |
            // 10 |-| 9       6 |-| 5
            //      |-----------|
            //      8           7
            PointF[,] p = new PointF[12,2];
            p[0, 0] = new PointF(x + radius, y + radius);
            p[1, 0] = new PointF(x + radius, y);
            p[2, 0] = new PointF(x + width - radius, y);
            p[3, 0] = new PointF(x + width - radius, y + radius);
            p[4, 0] = new PointF(x + width, y + radius);
            p[5, 0] = new PointF(x + width, y + height - radius);
            p[6, 0] = new PointF(x + width - radius, y + height - radius);
            p[7, 0] = new PointF(x + width - radius, y + height);
            p[8, 0] = new PointF(x + radius, y + height);
            p[9, 0] = new PointF(x + radius, y + height - radius);
            p[10, 0] = new PointF(x, y + height - radius);
            p[11, 0] = new PointF(x, y + radius);

            // Adjust points to offset the inner circle
            p[0, 1] = new PointF(x + radius + pen.Width, y + radius + pen.Width);
            p[1, 1] = new PointF(x + radius + pen.Width, y + pen.Width);
            p[2, 1] = new PointF(x + width - radius - pen.Width, y + pen.Width);
            p[3, 1] = new PointF(x + width - radius - pen.Width, y + radius + pen.Width);
            p[4, 1] = new PointF(x + width - pen.Width, y + radius + pen.Width);
            p[5, 1] = new PointF(x + width - pen.Width, y + height - radius - pen.Width);
            p[6, 1] = new PointF(x + width - radius - pen.Width, y + height - radius - pen.Width);
            p[7, 1] = new PointF(x + width - radius - pen.Width, y + height - pen.Width);
            p[8, 1] = new PointF(x + radius + pen.Width, y + height - pen.Width);
            p[9, 1] = new PointF(x + radius + pen.Width, y + height - radius - pen.Width);
            p[10, 1] = new PointF(x + pen.Width, y + height - radius - pen.Width);
            p[11, 1] = new PointF(x + pen.Width, y + radius + pen.Width);

            GL.Color4(pen.Color.ToSystemColor());
            //GL.LineWidth(pen.Width);

            int steps = 12;
            double step = MathHelper.DegreesToRadians(90) / steps;
            double startAngle = MathHelper.DegreesToRadians(0);
            PointF cornerPoint;

            GL.Begin(BeginMode.QuadStrip);

            // Starting point
            GL.Vertex3(p[11, 1].X, p[11, 0].Y, 0);
            GL.Vertex3(p[11, 0].X, p[11, 0].Y, 0);

            #region Draw upper left corner

            startAngle = MathHelper.DegreesToRadians(180);
            cornerPoint = p[0, 0];
            for (int i = 0; i < steps; i++)
            {
                // Inner point
                GL.Vertex3(
                    cornerPoint.X + ((radius - pen.Width) * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + ((radius - pen.Width) * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);

                // Outer point
                GL.Vertex3(
                    cornerPoint.X + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);
            }

            #endregion

            // Top line
            GL.Vertex3(p[1, 0].X, p[1, 1].Y, 0);
            GL.Vertex3(p[1, 0].X, p[1, 0].Y, 0);
            GL.Vertex3(p[2, 0].X, p[2, 1].Y, 0);
            GL.Vertex3(p[2, 0].X, p[2, 0].Y, 0);

            #region Draw upper right corner

            startAngle = MathHelper.DegreesToRadians(270);
            cornerPoint = p[3, 0];
            for (int i = 0; i < steps; i++)
            {
                // Inner point
                GL.Vertex3(
                    cornerPoint.X + ((radius - pen.Width) * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + ((radius - pen.Width) * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);

                // Outer point
                GL.Vertex3(
                    cornerPoint.X + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);
            }

            #endregion

            // Right line
            GL.Vertex3(p[4, 1].X, p[4, 0].Y, 0);
            GL.Vertex3(p[4, 0].X, p[4, 0].Y, 0);
            GL.Vertex3(p[5, 1].X, p[5, 0].Y, 0);
            GL.Vertex3(p[5, 0].X, p[5, 0].Y, 0);

            #region Draw lower right corner

            startAngle = MathHelper.DegreesToRadians(0);
            cornerPoint = p[6, 0];
            for (int i = 0; i < steps; i++)
            {
                // Inner point
                GL.Vertex3(
                    cornerPoint.X + ((radius - pen.Width) * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + ((radius - pen.Width) * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);

                // Outer point
                GL.Vertex3(
                    cornerPoint.X + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);
            }

            #endregion

            // Bottom line
            GL.Vertex3(p[7, 0].X, p[7, 1].Y, 0);
            GL.Vertex3(p[7, 0].X, p[7, 0].Y, 0);
            GL.Vertex3(p[8, 0].X, p[8, 1].Y, 0);
            GL.Vertex3(p[8, 0].X, p[8, 0].Y, 0);

            #region Draw lower left corner

            startAngle = MathHelper.DegreesToRadians(90);
            cornerPoint = p[9, 0];
            for (int i = 0; i < steps; i++)
            {
                // Inner point
                GL.Vertex3(
                    cornerPoint.X + ((radius - pen.Width) * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + ((radius - pen.Width) * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);

                // Outer point
                GL.Vertex3(
                    cornerPoint.X + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1)))),
                    cornerPoint.Y + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1)))),
                    0);
            }

            #endregion

            // Left line
            GL.Vertex3(p[10, 1].X, p[10, 0].Y, 0);
            GL.Vertex3(p[10, 0].X, p[10, 0].Y, 0);
            GL.Vertex3(p[11, 1].X, p[11, 0].Y, 0);
            GL.Vertex3(p[11, 0].X, p[11, 0].Y, 0);

            GL.End();

            // Restore matrix
            GL.PopMatrix();
        }
        public void DrawRoundRectangle(Pen p, Rectangle rect, int radius)
        {
            DrawRoundRectangle(p, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            // Save matrix
            GL.PushMatrix();

            // Set color
            GL.Color4(brush.Color.ToSystemColor());

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            int steps = 100;
            double step = MathHelper.DegreesToRadians(360) / steps;
            double startAngle = MathHelper.DegreesToRadians(0);
            double radiusW = width / 2;
            double radiusH = height / 2;

            // Inital array definition
            double[] points = new double[(steps + 3) * 2];
            // Base point      
            points[0] = x + radiusW; points[1] = y + radiusH;
            // Start point
            points[2] = x; points[3] = y + radiusH;
            // Corner points
            int index = 4;
            for (int i = 0; i <= steps; i++)
            {
                points[index] = points[0] + (radiusW * (float)System.Math.Cos(startAngle + (step * (i + 1))));
                points[index + 1] = points[1] + (radiusH * (float)System.Math.Sin(startAngle + (step * (i + 1))));
                index += 2;
            }

            // Render triangles
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Double, 0, points);
            GL.DrawArrays(BeginMode.TriangleFan, 0, points.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();
        }
        public void FillEllipse(Brush brush, Rectangle rect)
        {
            FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }
        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            GL.Color4(pen.Color.ToSystemColor());
            GL.LineWidth(pen.Width);

            float yrad = height / 2F;
            float xrad = width / 2F;
            float[] arr = new float[720];
            for (int t = 0; t < 360; t++)
            {
                double rad = MathHelper.DegreesToRadians(t);
                arr[2 * t] = x + xrad + (float)(xrad * System.Math.Cos(rad));
                arr[(2 * t) + 1] = y + yrad + (float)(yrad * System.Math.Sin(rad));
            }
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Float, 0, arr);
            GL.DrawArrays(BeginMode.LineLoop, 0, 360);
            GL.DisableClientState(ArrayCap.VertexArray);

            // Restore matrix
            GL.PopMatrix();
        }
        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawPoint(Color color, int x, int y, int width, int height)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            float radius = System.Math.Min(width, height) / 2f;

            GL.Color4(color.ToSystemColor());
            GL.PointSize(radius * 2);

            GL.Begin(BeginMode.Points);
            GL.Vertex3(x + radius, y + radius, 0);
            GL.End();

            // Restore matrix
            GL.PopMatrix();
        }


        public void FillRectangle(GradientData gradient, Rectangle rect, float opacity)
        {
            double width2 = rect.Width / 2.0;
            double height2 = rect.Height / 2.0;

            // Save current matrix
            GL.PushMatrix();

            // Translate base point from center of screen to upper left corner 
            _3D_Translate(rect.Left + width2, rect.Top + height2, 0);

            // Rotate graphics
            _3D_Rotate(0, 0, 0);

            // Scale graphics to correct dimensions
            GL.Scale(width2, height2, 1);

            for (int i = 0; i < gradient.Count; i++)
            {
                GL.Begin(BeginMode.Quads);
                GL.Color4(Color.FromArgb((int)(255 * (gradient[i].ColorPointV1.Color.A / 255.0f) * opacity), gradient[i].ColorPointV1.Color).ToSystemColor()); GL.Vertex3(gradient[i].ColorPointV1.Point.ToOpenTKVector3());
                GL.Color4(Color.FromArgb((int)(255 * (gradient[i].ColorPointV2.Color.A / 255.0f) * opacity), gradient[i].ColorPointV2.Color).ToSystemColor()); GL.Vertex3(gradient[i].ColorPointV2.Point.ToOpenTKVector3());
                GL.Color4(Color.FromArgb((int)(255 * (gradient[i].ColorPointV3.Color.A / 255.0f) * opacity), gradient[i].ColorPointV3.Color).ToSystemColor()); GL.Vertex3(gradient[i].ColorPointV3.Point.ToOpenTKVector3());
                GL.Color4(Color.FromArgb((int)(255 * (gradient[i].ColorPointV4.Color.A / 255.0f) * opacity), gradient[i].ColorPointV4.Color).ToSystemColor()); GL.Vertex3(gradient[i].ColorPointV4.Point.ToOpenTKVector3());
                GL.End();                
            }

            // Restore matrix
            GL.PopMatrix();
        }
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            double width2 = width / 2.0;
            double height2 = height / 2.0;

            // Save current matrix
            GL.PushMatrix();

            // Translate base point from center of screen to upper left corner 
            _3D_Translate(x + width2, y + height2, 0);

            // Rotate graphics
            _3D_Rotate(0, 0, 0);

            // Scale graphics to correct dimensions
            GL.Scale(width2, height2, 1);

            GL.Begin(BeginMode.Quads);
            GL.Color4(brush.ColorData_C1.ToSystemColor());
            GL.Vertex3(-1, -1, 0); //TOP-LEFT
            GL.Color4(brush.ColorData_C2.ToSystemColor());
            GL.Vertex3(1, -1, 0); //TOP-RIGHT
            GL.Color4(brush.ColorData_C3.ToSystemColor());
            GL.Vertex3(1, 1, 0); //BOTTOM-RIGHT
            GL.Color4(brush.ColorData_C4.ToSystemColor());
            GL.Vertex3(-1, 1, 0); //BOTTOM-LEFT
            GL.End();

            // Restore matrix
            GL.PopMatrix();
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRoundRectangle(Brush brush, int x, int y, int width, int height, int radius)
        {
            FillSolidRoundRectangle(brush.Color, x, y, width, height, radius);
        }
        public void FillRoundRectangle(Brush brush, Rectangle rect, int radius)
        {
            FillSolidRoundRectangle(brush.Color, rect.X, rect.Y, rect.Width, rect.Height, radius);
        }
        private void FillSolidRoundRectangle(Color color, int x, int y, int width, int height, int radius)
        {
            // Save matrix
            GL.PushMatrix();

            // Move base point to 0,0,0
            _3D_Translate(0, 0, 0);

            // Calculate base points
            // Point order:
            //      1           2
            //      |-----------|
            // 11 |-| 0       3 |-| 4
            //    |               |
            //    |               |
            // 10 |-| 9       6 |-| 5
            //      |-----------|
            //      8           7
            PointF[] p = new PointF[12];
            p[0] = new PointF(x + radius, y + radius);
            p[1] = new PointF(x + radius, y);
            p[2] = new PointF(x + width - radius, y);
            p[3] = new PointF(x + width - radius, y + radius);
            p[4] = new PointF(x + width, y + radius);
            p[5] = new PointF(x + width, y + height - radius);
            p[6] = new PointF(x + width - radius, y + height - radius);
            p[7] = new PointF(x + width - radius, y + height);
            p[8] = new PointF(x + radius, y + height);
            p[9] = new PointF(x + radius, y + height - radius);
            p[10] = new PointF(x, y + height - radius);
            p[11] = new PointF(x, y + radius);

            // Set color
            GL.Color4(color.ToSystemColor());

            #region Draw rectangles

            // Draw center rectangle
            GL.Begin(BeginMode.Quads);
            GL.Vertex3(p[10].X, p[10].Y, 0); //BOTTOM-LEFT
            GL.Vertex3(p[11].X, p[11].Y, 0); //TOP-LEFT
            GL.Vertex3(p[4].X, p[4].Y, 0); //TOP-RIGHT
            GL.Vertex3(p[5].X, p[5].Y, 0); //BOTTOM-RIGHT

            // Draw top rectangle
            GL.Vertex3(p[0].X, p[0].Y, 0); //BOTTOM-LEFT
            GL.Vertex3(p[1].X, p[1].Y, 0); //TOP-LEFT
            GL.Vertex3(p[2].X, p[2].Y, 0); //TOP-RIGHT
            GL.Vertex3(p[3].X, p[3].Y, 0); //BOTTOM-RIGHT

            // Draw bottom rectangle
            GL.Vertex3(p[8].X, p[8].Y, 0); //BOTTOM-LEFT
            GL.Vertex3(p[9].X, p[9].Y, 0); //TOP-LEFT
            GL.Vertex3(p[6].X, p[6].Y, 0); //TOP-RIGHT
            GL.Vertex3(p[7].X, p[7].Y, 0); //BOTTOM-RIGHT
            GL.End();

            #endregion

            // Calculate corner points
            int steps = 12;
            double step = MathHelper.DegreesToRadians(90) / steps;
            double startAngle = MathHelper.DegreesToRadians(0);

            #region Draw upper left corner

            // Starting angle
            startAngle = MathHelper.DegreesToRadians(180);

            // Inital array definition
            double[] points = new double[(steps + 3) * 2];
            // Base point      
            points[0] = p[0].X; points[1] = p[0].Y;
            // Start point
            points[2] = p[11].X; points[3] = p[11].Y; 
            // Corner points
            int index = 4;
            for (int i = 0; i < steps; i++)
            {
                points[index] = points[0] + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1))));
                points[index + 1] = points[1] + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1))));
                index += 2;
            }
            // End point
            points[index] = p[1].X; points[index + 1] = p[1].Y; 
            
            // Render triangles
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Double, 0, points);
            GL.DrawArrays(BeginMode.TriangleFan, 0, points.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            #endregion

            #region Draw upper right corner
            
            // Starting angle
            startAngle = MathHelper.DegreesToRadians(270);
            // Inital array definition
            points = new double[(steps + 3) * 2];
            // Base point      
            points[0] = p[3].X; points[1] = p[3].Y;
            // Start point
            points[2] = p[2].X; points[3] = p[2].Y;
            // Corner points
            index = 4;
            for (int i = 0; i < steps; i++)
            {
                points[index] = points[0] + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1))));
                points[index + 1] = points[1] + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1))));
                index += 2;
            }
            // End point
            points[index] = p[4].X; points[index + 1] = p[4].Y;

            // Render triangles
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Double, 0, points);
            GL.DrawArrays(BeginMode.TriangleFan, 0, points.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            #endregion

            #region Draw lower right corner

            // Starting angle
            startAngle = MathHelper.DegreesToRadians(0);
            // Inital array definition
            points = new double[(steps + 3) * 2];
            // Base point      
            points[0] = p[6].X; points[1] = p[6].Y;
            // Start point
            points[2] = p[5].X; points[3] = p[5].Y;
            // Corner points
            index = 4;
            for (int i = 0; i < steps; i++)
            {
                points[index] = points[0] + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1))));
                points[index + 1] = points[1] + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1))));
                index += 2;
            }
            // End point
            points[index] = p[7].X; points[index + 1] = p[7].Y;

            // Render triangles
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Double, 0, points);
            GL.DrawArrays(BeginMode.TriangleFan, 0, points.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            #endregion

            #region Draw lower left corner

            // Starting angle
            startAngle = MathHelper.DegreesToRadians(90);
            // Inital array definition
            points = new double[(steps + 3) * 2];
            // Base point      
            points[0] = p[9].X; points[1] = p[9].Y;
            // Start point
            points[2] = p[8].X; points[3] = p[8].Y;
            // Corner points
            index = 4;
            for (int i = 0; i < steps; i++)
            {
                points[index] = points[0] + (radius * (float)System.Math.Cos(startAngle + (step * (i + 1))));
                points[index + 1] = points[1] + (radius * (float)System.Math.Sin(startAngle + (step * (i + 1))));
                index += 2;
            }
            // End point
            points[index] = p[10].X; points[index + 1] = p[10].Y;

            // Render triangles
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(2, VertexPointerType.Double, 0, points);
            GL.DrawArrays(BeginMode.TriangleFan, 0, points.Length / 2);
            GL.DisableClientState(ArrayCap.VertexArray);

            #endregion

            // Restore matrix
            GL.PopMatrix();

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
            // Set viewport
            GL.Viewport(0, 0, _Width, _Height);

            // Set rendering options
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            //GL.Enable(EnableCap.PolygonSmooth); // This causes artifacts with certain videocards
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // Shading
            GL.ShadeModel(ShadingModel.Smooth);

            // 3D options
            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.CullFace);

            // Enable blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Get texure sizes
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);

            //Init arrays
            normalTex = new int[] { 0, 1, 1, 1, 0, 0, 1, 0 };
            vertTex = new int[] { 0, 0, 1, 0, 0, 1, 1, 1 };
            horizTex = new int[] { 1, 1, 0, 1, 1, 0, 0, 0 };

            //// Enable 2D as default
            //_2D_SetProjection();
            //_2D_SetModelView();

            _3D_ProjectionSet(Vector3d.Zero);
            _3D_ModelView_Set(Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, 0, true);
        }

        public void Begin()
        {
            //_3D_SetProjection(Vector3d.Zero);
            //_3D_SetModelView(Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, 0);
            _3D_ModelView_Activate();
        }
        public void End()
        {
            if (_Textures[_Screen].Count > 0)
            {
                GL.DeleteTextures(_Textures[_Screen].Count, _Textures[_Screen].ToArray());
                _Textures[_Screen].Clear();
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
            //throw new NotImplementedException();
        }

        public void ResetClip()
        {
            Clip = NoClip;
        }

        public void ResetTransform()
        {
            //GL.LoadIdentity();
            _3D_ProjectionSet(Vector3d.Zero);
            _3D_ModelView_Set(Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, 0, true);
        }

        public void Resize(int Width, int Height)
        {
            _Width = Width;
            _Height = Height;
            _WidthScale = (_Width / 1000F);
            _HeightScale = (_Height / 600F);
            GL.Viewport(0, 0, _Width, _Height);
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
            GL.Scissor((int)(x * _WidthScale), (int)((600 - y - height) * _HeightScale), (int)(width * _WidthScale), (int)(height * _HeightScale));
        }

        public void Translate(double dx, double dy)
        {
            GL.Translate(dx, dy, 0);
        }

        public void Translate(double dx, double dy, double dz)
        {
            GL.Translate(dx, dy, dz);
        }

        public void Rotate(double angle, Graphics.Axis axis)
        {
            GL.Rotate(angle, (axis == Graphics.Axis.X ? 1 : 0), (axis == Graphics.Axis.Y ? 1 : 0), (axis == Graphics.Axis.Z ? 1 : 0));
        }

        public void Rotate(double x, double y, double z)
        {
            GL.Rotate(x, 1, 0, 0);
            GL.Rotate(y, 0, 1, 0);
            GL.Rotate(z, 0, 0, 1);
        }

        public void Scale(double sx, double sy, double sz)
        {
            GL.Scale(sx, sy, sz);
        }

        public void Transform(Matrix4d m)
        {
            GL.MultMatrix(ref m);
        }

        private void _2D_SetProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1000, 600, 0, 0, 1);
        }

        private void _2D_SetModelView()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        /// <summary>
        /// Translates the graphics relative from a base point in the upper left corner
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void _3D_Translate(double x, double y, double z)
        {
            // Translate base point from center to upper left corner of screen
            GL.Translate(-500, -300, 0);

            // Translate graphics to correct placement
            GL.Translate(x, y, z);
        }

        /// <summary>
        /// Rotates the graphics (rotation order X, Y and then Z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void _3D_Rotate(double x, double y, double z)
        {
            GL.Rotate(x, 1, 0, 0);
            GL.Rotate(y, 0, 1, 0);
            GL.Rotate(z, 0, 0, 1);
        }

        /// <summary>
        /// Sets the projection matrix
        /// </summary>
        /// <param name="renderLocation"></param>
        private void _3D_ProjectionSet(Vector3d renderLocation)
        {
            // Create a perspective frustrum
            Matrix4d projection = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), 1000.0 / 600.0, 1.0, 2000.0);

            if (renderLocation != Vector3d.Zero)
            {
                // Move zero point for rendering to upper left corner
                projection *= Matrix4d.CreateTranslation(-1, 1, 0);

                // Translate rendering point to the specified location
                projection *= Matrix4d.CreateTranslation(_3D_ProjectionSpace_NormalizeVector(new Vector3d(renderLocation.X, renderLocation.Y, 0)));
            }

            // Load matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref projection);
        }

        /// <summary>
        /// Sets the model matrix
        /// </summary>
        /// <param name="cameraLocation">Moves the camera location while looking at 0,0,0</param>
        /// <param name="cameraRotation">Rotates the camera while looking at 0,0,0</param>
        /// <param name="cameraOffset">Offsets the camera</param>
        /// <param name="zoom">Zoom level</param>
        /// <param name="activateMatrix">Activates the matrix immediately. NB! This can only be done from the rendering thread</param>
        public void _3D_ModelView_Set(Vector3d cameraLocation, Vector3d cameraRotation, Vector3d cameraOffset, double zoom, bool activateMatrix)
        {
            // Rotate world to get the correct axes in 3D (X going right, Y going down)
            modelview = Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(180));

            // Rotate camera
            modelview *= Matrix4d.CreateRotationX(MathHelper.DegreesToRadians(cameraRotation.X));
            modelview *= Matrix4d.CreateRotationY(MathHelper.DegreesToRadians(cameraRotation.Y));
            modelview *= Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(cameraRotation.Z));

            // Offset camera
            modelview *= Matrix4d.CreateTranslation(cameraOffset);

            // Zoom 
            if (zoom != 0)
                modelview *= Matrix4d.Scale(zoom);

            // Set camera with fixed zoom to make the 1000x600 frame match the 1000x600 pixels
            modelview *= Matrix4d.LookAt(new Vector3d(-cameraLocation.X, -cameraLocation.Y, cameraLocation.Z + (-724)), Vector3d.UnitZ, Vector3d.UnitY);

            // Load matrix
            if (activateMatrix)
                _3D_ModelView_Activate();
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
                GL.LoadMatrix(ref modelview);
            }
        }

        /// <summary>
        /// The matrix for the global modelview
        /// </summary>
        private Matrix4d modelview;

        /// <summary>
        /// Activates the model view
        /// </summary>
        public void _3D_ModelView_Activate()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref modelview);
        }

        /// <summary>
        /// Resets the model view
        /// </summary>
        public void _3D_ModelView_ResetAndPlaceCamera(Vector3d cameraLocation)
        {
            // Rotate world to get the correct axes in 3D (X going right, Y going down)
            modelview = Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(180));

            // Set camera with fixed zoom to make the 1000x600 frame match the 1000x600 pixels
            modelview *= Matrix4d.LookAt(new Vector3d(-cameraLocation.X, -cameraLocation.Y, cameraLocation.Z + (-724)), Vector3d.UnitZ, Vector3d.UnitY);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref modelview);
        }

        
        /// <summary>
        /// Normalizes a vector in projection space 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private Vector3d _3D_ProjectionSpace_NormalizeVector(Vector3d v)
        {
            // Normalize vectors and swap Y vector to get the correct coordinate system
            return new Vector3d(v.X / (1000.0 / 2f), -(v.Y / (600.0 / 2f)), v.Z / (600.0 / 2f));
        }
        
        /// <summary>
        /// The modelview stack
        /// </summary>
        Stack<Matrix4d> modelViewStack = new Stack<Matrix4d>();

        /// <summary>
        /// Pushes the modelview onto the stack
        /// </summary>
        public void _3D_ModelView_Push()
        {
            GL.PushMatrix();
            modelViewStack.Push(modelview);
        }

        /// <summary>
        /// Pops the modelview back from the stack
        /// </summary>
        public void _3D_ModelView_Pop()
        {
            GL.PopMatrix();
            modelview = modelViewStack.Pop();
        }
    }
}
