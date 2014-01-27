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

// This code sample was found here: https://github.com/aquarla/CSharp-NinePatch-Sample with no attached license

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenMobile.Graphics
{
    /// <summary>
    /// Class for handling Android 9-patch in C #
    /// Additional info can be found here: http://developer.android.com/tools/help/draw9patch.html
    /// An online editor/creator for 9patch images is available here: http://android-ui-utils.googlecode.com/hg/asset-studio/dist/nine-patches.html
    /// 
    /// How to use:
    /// NinePatch ninePatch = new NinePatch(image); // Create a 9-patch image object from the 9-patch image
    /// Image newImage = ninePatch.ImageSizeOf(500, 500) // Get the picture that is stretched to 500x500
    /// </summary>
    public class NinePatch
    {
        #region Private variables

        private class ImageWrapper
        {
            public Image Image;
            public Rectangle ContentArea;

            public ImageWrapper(Image image, Rectangle contentArea)
            {
                this.Image = image;
                this.ContentArea = contentArea;
            }

            public void Dispose()
            {
                Image.Dispose();
            }
        }

        /// <summary>
        /// 9-patch image with one pixel up, down, left and right
        /// </summary>
        private Bitmap image;


        /// <summary>
        /// Image cache for each image size
        /// </summary>
        private Dictionary<string, ImageWrapper> cache;

        /// <summary>
        /// Superficial patch
        /// </summary>
        private List<int> topPatches;

        /// <summary>
        /// Patch left-hand side
        /// </summary>
        private List<int> leftPatches;

        /// <summary>
        /// Lower side patch
        /// </summary>
        private List<int> bottomPatches;

        /// <summary>
        /// Right-hand side patch
        /// </summary>
        private List<int> rightPatches;

        /// <summary>
        /// The number of bytes per pixel
        /// </summary>
        static private int BYTES_PER_PIXEL = 4;

        #endregion

        #region Public properties

        /// <summary>
        /// Return the original image without the 9patch frameinfo (1 pixel on each side)
        /// </summary>
        public Bitmap OriginalImage
        {
            get
            {
                return GetImageWithout9PatchFrame(this.image);
            }
        }

        #endregion

        #region Constructor / deconstructor

        /// <summary>
        /// Creats a new 9Patch image
        /// </summary>
        /// <param name="image"></param>
        public NinePatch(Bitmap image)
        {
            this.image = image;
            cache = new Dictionary<string, ImageWrapper>();
            FindPatchRegion(this.image, out topPatches, out leftPatches, out bottomPatches, out rightPatches);
        }

        ~NinePatch()
        {
            this.image.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the image cache
        /// </summary>
        public void ClearCache()
        {
            foreach (KeyValuePair<string, ImageWrapper> pair in cache)
            {
                pair.Value.Dispose();
            }
            cache.Clear();
        }

        /// <summary>
        /// Returns the image scaled to the requested size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Picture object</returns>
        public Image ImageSizeOf(int width, int height)
        {
            // Check cache for this size
            if (cache.ContainsKey(String.Format("{0}x{1}", width, height)))
            {
                return cache[String.Format("{0}x{1}", width, height)].Image;
            }

            using (Bitmap src = this.OriginalImage)
            {
                int sourceWidth = src.Width;
                int sourceHeight = src.Height;
                int targetWidth = width;
                int targetHeight = height;

                // If target source is smaller than the specified force
                // I would be the size of the source.
                // The size of the target and the source if I were the same, simply return the original image
                targetWidth = System.Math.Max(sourceWidth, targetWidth);
                targetHeight = System.Math.Max(sourceHeight, targetHeight);
                if (sourceWidth == targetWidth && sourceHeight == targetHeight)
                {
                    return this.OriginalImage;
                }

                // provide a buffer of the source image
                BitmapData srcData = src.LockBits(
                    new System.Drawing.Rectangle(0, 0, sourceWidth, sourceHeight),
                    ImageLockMode.ReadOnly,
                    src.PixelFormat
                );
                byte[] srcBuf = new byte[sourceWidth * sourceHeight * BYTES_PER_PIXEL];
                Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

                // provide a buffer of the target image
                Bitmap dst = new Bitmap(targetWidth, targetHeight);
                byte[] dstBuf = new byte[dst.Width * dst.Height * BYTES_PER_PIXEL];

                // obtained for each coordinate x, coordinate y, "each of the target coordinates corresponding to the coordinates of the source"
                List<int> xMapping = XMapping(topPatches, targetWidth - sourceWidth, targetWidth);
                List<int> yMapping = YMapping(leftPatches, targetHeight - sourceHeight, targetHeight);

                // Copy the value of each pixel according to the mapping above
                for (int y = 0; y < targetHeight; y++)
                {
                    int sourceY = yMapping[y];
                    for (int x = 0; x < targetWidth; x++)
                    {
                        int sourceX = xMapping[x];

                        for (int z = 0; z < BYTES_PER_PIXEL; z++)
                        {
                            dstBuf[y * targetWidth * BYTES_PER_PIXEL + x * BYTES_PER_PIXEL + z] =
                                srcBuf[sourceY * sourceWidth * BYTES_PER_PIXEL + sourceX * BYTES_PER_PIXEL + z];
                        }
                    }
                }

                // Bitmap copy to the target image
                BitmapData dstData = dst.LockBits(
                    new System.Drawing.Rectangle(0, 0, dst.Width, dst.Height),
                    ImageLockMode.WriteOnly,
                    src.PixelFormat
                );
                IntPtr dstScan0 = dstData.Scan0;
                Marshal.Copy(dstBuf, 0, dstScan0, dstBuf.Length);

                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);

                // Calculate contentArea
                Rectangle contentArea;

                if (bottomPatches.Count > 0 && rightPatches.Count > 0)
                {
                    contentArea = new Rectangle(
                        bottomPatches[0],
                        rightPatches[0],
                        dst.Width - (this.OriginalImage.Width - bottomPatches[bottomPatches.Count - 1]) - bottomPatches[0],
                        dst.Height - (this.OriginalImage.Height - rightPatches[rightPatches.Count - 1]) - rightPatches[0]);
                }
                else
                    contentArea = new Rectangle(0, 0, dst.Width, dst.Height);

                // Stored in the cache with the size
                cache.Add(String.Format("{0}x{1}", width, height), new ImageWrapper(dst, contentArea));

                return dst;
            }
        }

        /// <summary>
        /// Returns the image scaled to the requested size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Picture object</returns>
        static public Bitmap GetImageSizeOf(Bitmap image9Patch, int width, int height, out Rectangle contentArea)
        {
            contentArea = Rectangle.Empty;

            Bitmap src = GetImageWithout9PatchFrame(image9Patch);

            int sourceWidth = src.Width;
            int sourceHeight = src.Height;
            int targetWidth = width;
            int targetHeight = height;

            // Set size of target image
            targetWidth = System.Math.Max(sourceWidth, targetWidth);
            targetHeight = System.Math.Max(sourceHeight, targetHeight);

            // if requested size is the same as what we already have, return original image
            if (sourceWidth == targetWidth && sourceHeight == targetHeight)
                return src;

            // Extract 9Patch information
            List<int> topPatches;
            List<int> leftPatches;
            List<int> rightPatches;
            List<int> bottomPatches;
            FindPatchRegion(image9Patch, out topPatches, out leftPatches, out bottomPatches, out rightPatches);

            // provide a buffer of the source image
            BitmapData srcData = src.LockBits(
                new System.Drawing.Rectangle(0, 0, sourceWidth, sourceHeight),
                ImageLockMode.ReadOnly,
                src.PixelFormat
            );
            byte[] srcBuf = new byte[sourceWidth * sourceHeight * BYTES_PER_PIXEL];
            Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

            // provide a buffer of the target image
            Bitmap dst = new Bitmap(targetWidth, targetHeight);
            byte[] dstBuf = new byte[dst.Width * dst.Height * BYTES_PER_PIXEL];

            // obtained for each coordinate x, coordinate y, "each of the target coordinates corresponding to the coordinates of the source"
            List<int> xMapping = XMapping(topPatches, targetWidth - sourceWidth, targetWidth);
            List<int> yMapping = YMapping(leftPatches, targetHeight - sourceHeight, targetHeight);

            // Copy the value of each pixel according to the mapping above
            for (int y = 0; y < targetHeight; y++)
            {
                int sourceY = yMapping[y];
                for (int x = 0; x < targetWidth; x++)
                {
                    int sourceX = xMapping[x];

                    for (int z = 0; z < BYTES_PER_PIXEL; z++)
                    {
                        dstBuf[y * targetWidth * BYTES_PER_PIXEL + x * BYTES_PER_PIXEL + z] =
                            srcBuf[sourceY * sourceWidth * BYTES_PER_PIXEL + sourceX * BYTES_PER_PIXEL + z];
                    }
                }
            }

            // Bitmap copy to the target image
            BitmapData dstData = dst.LockBits(
                new System.Drawing.Rectangle(0, 0, dst.Width, dst.Height),
                ImageLockMode.WriteOnly,
                src.PixelFormat
            );
            IntPtr dstScan0 = dstData.Scan0;
            Marshal.Copy(dstBuf, 0, dstScan0, dstBuf.Length);

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            // Calculate contentArea
            if (bottomPatches.Count > 0 && rightPatches.Count > 0)
            {
                contentArea = new Rectangle(
                    bottomPatches[0],
                    rightPatches[0],
                    dst.Width - (src.Width - bottomPatches[bottomPatches.Count - 1]) - bottomPatches[0],
                    dst.Height - (src.Height - rightPatches[rightPatches.Count - 1]) - rightPatches[0]);
            }
            else
                contentArea = new Rectangle(0, 0, dst.Width, dst.Height);

            src.Dispose();
            return dst;
        }

        static public Bitmap GetImageWithout9PatchFrame(Bitmap image9Patch)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(1, 1, image9Patch.Width - 2, image9Patch.Height - 2);
            Bitmap result = image9Patch.Clone(rect, image9Patch.PixelFormat);
            return result;
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// From all sides of the image one pixel 9-patch,
        /// I remember what went corresponds to a range stretching
        /// </summary>
        static private void FindPatchRegion(Bitmap image9Patch, out List<int> topPatches, out List<int> leftPatches, out List<int> bottomPatches, out List<int> rightPatches)
        {
            topPatches = new List<int>();
            leftPatches = new List<int>();
            bottomPatches = new List<int>();
            rightPatches = new List<int>();

            using (Bitmap src = new Bitmap(image9Patch))
            {
                BitmapData srcData = src.LockBits(
                    new System.Drawing.Rectangle(0, 0, src.Width, src.Height),
                    ImageLockMode.ReadOnly,
                    src.PixelFormat
                );
                byte[] srcBuf = new byte[src.Width * src.Height * BYTES_PER_PIXEL];
                Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

                // top
                for (int x = 1; x < srcData.Width - 1; x++)
                {
                    int index = x * BYTES_PER_PIXEL;
                    byte b = srcBuf[index];
                    byte g = srcBuf[index + 1];
                    byte r = srcBuf[index + 2];
                    byte alpha = srcBuf[index + 3];
                    if (r == 0 && g == 0 && b == 0 && alpha == 255)
                    {
                        topPatches.Add(x - 1);
                    }
                }

                // left
                for (int y = 1; y < srcData.Height - 1; y++)
                {
                    int index = y * BYTES_PER_PIXEL * srcData.Width;
                    byte b = srcBuf[index];
                    byte g = srcBuf[index + 1];
                    byte r = srcBuf[index + 2];
                    byte alpha = srcBuf[index + 3];
                    if (r == 0 && g == 0 && b == 0 && alpha == 255)
                    {
                        leftPatches.Add(y - 1);
                    }
                }

                // bottom
                for (int x = 1; x < srcData.Width - 1; x++)
                {
                    int index = (srcData.Height - 1) * BYTES_PER_PIXEL * srcData.Width + x * BYTES_PER_PIXEL;
                    byte b = srcBuf[index];
                    byte g = srcBuf[index + 1];
                    byte r = srcBuf[index + 2];
                    byte alpha = srcBuf[index + 3];
                    if (r == 0 && g == 0 && b == 0 && alpha == 255)
                    {
                        bottomPatches.Add(x - 1);
                    }
                }

                // right
                for (int y = 1; y < srcData.Height - 1; y++)
                {
                    int index = y * BYTES_PER_PIXEL * srcData.Width + (srcData.Width - 1) * BYTES_PER_PIXEL;
                    byte b = srcBuf[index];
                    byte g = srcBuf[index + 1];
                    byte r = srcBuf[index + 2];
                    byte alpha = srcBuf[index + 3];
                    if (r == 0 && g == 0 && b == 0 && alpha == 255)
                    {
                        rightPatches.Add(y - 1);
                    }
                }

            }
        }

        /// <summary>
        /// Gets a list that represents the X coordinate of the image you want generated, or where the original image corresponding to the coordinates
        /// </summary>
        /// <param name="diffWidth">Of the image and the original image that you want to generate, the difference between the width</param>
        /// <param name="targetWidth">The width of the image you want to generate</param>
        /// <returns></returns>
        static private List<int> XMapping(List<int> topPatches, int diffWidth, int targetWidth)
        {
            List<int> result = new List<int>(targetWidth);
            int src = 0;
            int dst = 0;
            while (dst < targetWidth)
            {
                int foundIndex = topPatches.IndexOf(src);
                if (foundIndex != -1)
                {
                    int repeatCount = (diffWidth / topPatches.Count) + 1;
                    if (foundIndex < diffWidth % topPatches.Count)
                    {
                        repeatCount++;
                    }
                    for (int j = 0; j < repeatCount; j++)
                    {
                        result.Insert(dst++, src);
                    }
                }
                else
                {
                    result.Insert(dst++, src);
                }
                src++;
            }
            return result;
        }

        /// <summary>
        /// Gets a list that represents the y-coordinate of the image you want to generated, or where the original image corresponding to the coordinates
        /// </summary>
        /// <param name="diffHeight"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        static private List<int> YMapping(List<int> leftPatches, int diffHeight, int targetHeight)
        {
            List<int> result = new List<int>(targetHeight);
            int src = 0;
            int dst = 0;
            while (dst < targetHeight)
            {
                int foundIndex = leftPatches.IndexOf(src);
                if (foundIndex != -1)
                {
                    int repeatCount = (diffHeight / leftPatches.Count) + 1;
                    if (foundIndex < diffHeight % leftPatches.Count)
                    {
                        repeatCount++;
                    }
                    for (int j = 0; j < repeatCount; j++)
                    {
                        result.Insert(dst++, src);
                    }
                }
                else
                {
                    result.Insert(dst++, src);
                }
                src++;
            }
            return result;
        }

        #endregion
    }
}
