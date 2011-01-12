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
using System.Threading;
using System.Drawing;

namespace OpenMobile.Graphics
{
    public sealed class OAnimatedImage
    {
        int[] frameDelay;
        Bitmap img;
        OImage[] images;
        int frameTimer;
        int currentFrame;
        public delegate void redraw(OAnimatedImage image);
        private event redraw OnRedrawInternal;
        Thread t;
        public event redraw OnRedraw
        {
            add
            {
                OnRedrawInternal += value;
                t = new Thread(animationLoop);
                t.Start();
            }
            remove
            {
                if (t != null)
                    t.Abort();
                OnRedrawInternal -= value;
            }
        }
        public OImage getFrame()
        {
            if (images[currentFrame] == null)
            {
                img.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, currentFrame);
                images[currentFrame] = new OImage(img);
            }
            return images[currentFrame];
        }
        public OAnimatedImage(System.Drawing.Bitmap b)
        {
            img = b;
            int count = b.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
            if (count == 0)
                count = 1;
            frameDelay = new int[count];
            images = new OImage[count];
            if (count > 1)
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] buffer = b.GetPropertyItem(0x5100).Value;
                    frameDelay[i] = ((buffer[i * 4] + (0x100 * buffer[(i * 4) + 1])) + (0x10000 * buffer[(i * 4) + 2])) + (0x1000000 * buffer[(i * 4) + 3]);
                }
            }
        }
        private void animationLoop()
        {
            while (true)
            {
                frameTimer += 5;
                if (frameTimer >= frameDelay[currentFrame])
                {
                    frameTimer = 0;
                    if (currentFrame + 1 < frameDelay.Length)
                        currentFrame++;
                    else
                        currentFrame = 0;
                    if (OnRedrawInternal!=null)
                        OnRedrawInternal(this);
                }
                Thread.Sleep(50);
            }
        }
    }
}
