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
using System.Drawing.Imaging;

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
                t.Name = "OAnimatedImage";
                t.IsBackground = true;
                t.Start();
            }
            remove
            {
                if (t != null)
                    t.Abort();
                OnRedrawInternal -= value;
                Dispose();
            }
        }

        private int TimeStamp;
        private int Frame = 0;
        public OImage getFrame()
        {
            // Check if we stopped animation
            if (t == null)
            {
                t = new Thread(animationLoop);
                t.Name = "OAnimatedImage";
                t.IsBackground = true;
                t.Start();
            }
            return getFrame(currentFrame);
        }
        public OImage getFrame(int frame)
        {
            TimeStamp = Environment.TickCount;
            if (images[frame] == null)
            {
                img.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, frame);
                images[frame] = new OImage(img);
            }
            return images[frame];
        }

        public OAnimatedImage(OImage b)
        {
            Initialize(b.image);
        }
        public OAnimatedImage(System.Drawing.Bitmap b)
        {
            Initialize(b);
        }
        public void Initialize(System.Drawing.Bitmap b)
        {
            img = b;
            FrameDimension frameDimension = new FrameDimension(img.FrameDimensionsList[0]);
            int count = img.GetFrameCount(frameDimension);
            if (count == 0)
                count = 1;
            frameDelay = new int[count];
            images = new OImage[count];
            int this_delay = 0;
            int index = 0;

            if (count > 1)
            {
                for (int i = 0; i < frameDelay.Length; i++)
                {
                    // Set animation delay
                    this_delay = BitConverter.ToInt32(img.GetPropertyItem(0x5100).Value, index) * 10;
                    frameDelay[i] = (this_delay < 100 ? 100 : this_delay);  // Minimum delay is 100 ms
                    index += 4;

                    // Generate frame images
                    img.SelectActiveFrame(frameDimension, i);
                    images[i] = new OImage((System.Drawing.Bitmap)img.Clone());
                }
            }
        }
        
        private void animationLoop()
        {
            bool Animate = true;
            while (Animate)
            {
                frameTimer += 10;
                if (frameTimer >= frameDelay[currentFrame])
                {
                    frameTimer = 0;
                    if (currentFrame + 1 < frameDelay.Length)
                        currentFrame++;
                    else
                        currentFrame = 0;

                    if (OnRedrawInternal!=null)
                        OnRedrawInternal(this);

                    // Is this image still being redrawn (shown)?
                    int Elapsed = Environment.TickCount - TimeStamp;
                    if (Elapsed > 5000)
                    {   // No; too long since last redraw, abort thread
                        Animate = false;
                    }
                }
                Thread.Sleep(10);
            }
            t = null;
        }

        public void Dispose()
        {
            if (t != null)
            {
                t.Abort();
                t = null;
            }
            if (img != null)
            {
                img.Dispose();
                img = null;
            }
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                {
                    images[i].Dispose();
                    images[i] = null;
                }
            }
        }

        ~OAnimatedImage()
        {
            Dispose();
        }

    }
}
