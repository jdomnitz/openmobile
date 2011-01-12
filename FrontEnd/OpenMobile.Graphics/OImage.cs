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

namespace OpenMobile.Graphics
{
    public class OImage : IDisposable, ICloneable
    {
        Bitmap img;
        public bool persist;
        private uint[] texture;
        int height, width;

        public uint Texture(int screen)
        {
            if (screen < texture.Length)
                return texture[screen];
            return 0;
        }
        public void rotateTexture()
        {
            for (int i = 0; i < texture.Length; i++)
                if (texture[i] > 0)
                {
                    Graphics.DeleteTexture(i, texture[i]);
                    texture[i] = 0;
                }
        }
        internal void SetTexture(int screen, uint texture)
        {
            if (screen < this.texture.Length)
                this.texture[screen] = texture;
            for (int i = 0; i < this.texture.Length; i++)
                if (this.texture[i] == 0)
                    return;
            if (!persist)
            {
                img.Dispose();
                img = null;
            }
        }
        public OImage(System.Drawing.Bitmap i)
        {
            texture = new uint[DisplayDevice.AvailableDisplays.Count];
            img = i;
            height = img.Height;
            width = img.Width;
        }
        public Bitmap image
        {
            get { return img; }
        }
        public Size Size
        {
            get
            {
                if (img == null)
                    return new Size();
                lock (img)
                    return new Size(width, height);
            }
        }
        public int Height
        {
            get
            {
                return height;
            }
        }
        public int Width
        {
            get
            {
                return width;
            }
        }
        public bool CanAnimate
        {
            get
            {
                if (img == null)
                    return false;
                return (img.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) > 1);
            }
        }
        public static OImage FromFile(string filename)
        {
            return new OImage(new Bitmap(filename));
        }
        public static OImage FromStream(System.IO.Stream stream)
        {
            return new OImage(new Bitmap(stream));
        }
        public static OImage FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement)
        {
            return new OImage(new Bitmap(stream, useEmbeddedColorManagement));
        }
        ~OImage()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (texture == null)
                return;
            for (int i = 0; i < texture.Length; i++)
                if (texture[i] > 0)
                    Graphics.DeleteTexture(i, texture[i]);
            texture = null;
            if (img != null)
                img.Dispose();
            img = null;
            GC.SuppressFinalize(this);
        }

        public object Clone()
        {
            lock (img)
            {
                OImage ret = new OImage((Bitmap)img.Clone());
                ret.texture = (uint[])this.texture.Clone();
                return ret;
            }
        }
    }
}
