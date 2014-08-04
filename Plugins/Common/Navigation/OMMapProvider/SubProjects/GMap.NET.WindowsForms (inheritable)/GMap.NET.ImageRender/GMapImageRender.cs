/*********************************************************************************
    This file is part of Open Mobile as a wrapper around GMap.Net's GMapControl.
    See GMap.Net section below for licensing for GMap.Net.

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
/*********************************************************************************
   *** GMap.NET - Great Maps for Windows Forms & Presentation ***

   GMap.NET is great and Powerful, Free, cross platform, open source
   .NET control. Enable use routing, geocoding, directions and maps
   from Coogle, Yahoo!, Bing, OpenStreetMap, ArcGIS, Pergo, SigPac,
   Yandex, Mapy.cz, Maps.lt, iKarte.lv, NearMap, OviMap, CloudMade,
     WikiMapia in Windows Forms & Presentation, supports caching
                     and runs on windows mobile!!


                    License: The MIT License (MIT)
-------------------------------------------------------------------
Copyright (c) 2008-2011 Universe, WARNING: This software can access some
map providers and may viotile their Terms of Service, you use it at your
own risk, nothing is forcing you to accept this ;} Source itself is legal!

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
-------------------------------------------------------------------
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using GMap.NET.WindowsForms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.ObjectModel;
using System.Drawing.Text;
using GMap.NET.MapProviders;
using System.ComponentModel;
using System.Windows.Forms;

namespace GMap.NET.ImageRender
{
    public class GMapImageUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The updated image
        /// </summary>
        public Image Image
        {
            get
            {
                return this._Image;
            }
            set
            {
                if (this._Image != value)
                {
                    this._Image = value;
                }
            }
        }
        private Image _Image;

        public GMapImageUpdatedEventArgs(Image image)
        {
            _Image = image;
        }

    }

    public delegate void GMapImageUpdatedDelegate(object sender, GMapImageUpdatedEventArgs e);

    public enum GMapPanDirections
    {
        Up,
        Down,
        Left,
        Right
    }

    public class GMapImageRender : GMapControl
    {
        Graphics _TargetImageGraphics;
        PaintEventArgs paintEventArgs;

        public event GMapImageUpdatedDelegate OnImageUpdated;
        public event GMapImageUpdatedDelegate OnImageAssigned;

        /// <summary>
        /// The target image
        /// </summary>
        public Bitmap TargetImage
        {
            get
            {
                return this._TargetImage;
            }
            set
            {
                if (this._TargetImage != value)
                {
                    this._TargetImage = value;
                }
            }
        }
        private Bitmap _TargetImage;

        protected override void OnResize(EventArgs e)
        {
            ConfigureTargetGraphics();
            base.OnResize(e);
            OnPaint(null);
        }

        /// <summary>
        /// construct
        /// </summary>
        public GMapImageRender(int width, int height)
            : base()
        {
            base.Width = width;
            base.Height = height;
            OnSizeChanged(new EventArgs());

            // Setup graphics
            ConfigureTargetGraphics();
            base.OnLoad(new EventArgs());
        }

        protected override void invalidatorEngage(object sender, ProgressChangedEventArgs e)
        {
            base.invalidatorEngage(sender, e);
            OnPaint(null);
        }

        ~GMapImageRender()
        {
            Dispose();
        }

        public new void Dispose()
        {
            try
            {
                Dispose(false);
            }
            catch
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            Manager.CancelTileCaching();
            base.Dispose(disposing);
        }

        void ConfigureTargetGraphics()
        {
            if (_TargetImage == null)
            {
                _TargetImage = new Bitmap(base.Width, base.Height);
                lock (_TargetImage)
                {
                    _TargetImageGraphics = Graphics.FromImage(_TargetImage);
                    paintEventArgs = new System.Windows.Forms.PaintEventArgs(_TargetImageGraphics, new Rectangle(0, 0, base.Width, base.Height));
                    Raise_OnImageAssigned();
                }
            }
            else
            {
                lock (_TargetImage)
                {
                    _TargetImage.Dispose();
                    _TargetImage = new Bitmap(base.Width, base.Height);
                    _TargetImageGraphics = Graphics.FromImage(_TargetImage);
                    paintEventArgs = new System.Windows.Forms.PaintEventArgs(_TargetImageGraphics, new Rectangle(0, 0, base.Width, base.Height));
                    Raise_OnImageAssigned();
                }
            }
        }

        void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnPaint(null);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            lock (_TargetImageGraphics)
            {
                lock (_TargetImage)
                {
                    base.OnPaint(paintEventArgs);
                    Raise_OnImageUpdated();
                }
            }
        }

        private void Raise_OnImageUpdated()
        {
            if (OnImageUpdated != null)
                OnImageUpdated(this, new GMapImageUpdatedEventArgs(this._TargetImage));
        }
        private void Raise_OnImageAssigned()
        {
            if (OnImageAssigned != null)
                OnImageAssigned(this, new GMapImageUpdatedEventArgs(this._TargetImage));
        }

        public override void Refresh()
        {
            base.Refresh();
            OnPaint(null);
        }

        public void Pan(GMapPanDirections direction, int distanceInPixels = 50)
        {
            Point center = new Point(Width / 2, Height / 2);

            base.OnMouseDown(new MouseEventArgs(base.DragButton, 1, center.X, center.Y, 0));
            switch (direction)
            {
                case GMapPanDirections.Up:
                    base.OnMouseMove(new MouseEventArgs(base.DragButton, 1, center.X, center.Y + distanceInPixels, 0));
                    break;
                case GMapPanDirections.Down:
                    base.OnMouseMove(new MouseEventArgs(base.DragButton, 1, center.X, center.Y - distanceInPixels, 0));
                    break;
                case GMapPanDirections.Left:
                    base.OnMouseMove(new MouseEventArgs(base.DragButton, 1, center.X + distanceInPixels, center.Y, 0));
                    break;
                case GMapPanDirections.Right:
                    base.OnMouseMove(new MouseEventArgs(base.DragButton, 1, center.X - distanceInPixels, center.Y, 0));
                    break;
                default:
                    break;
            }
            base.OnMouseUp(new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 1, center.X, center.Y, 0));
            OnPaint(null);
        }

        public void EmulateMouseDown(MouseButtons button, int x, int y)
        {
            base.OnMouseDown(new MouseEventArgs(button, 1, x, y, 0));
            OnPaint(null);
        }

        public void EmulateMouseMove(MouseButtons button, int x, int y)
        {
            base.OnMouseMove(new MouseEventArgs(button, 1, x, y, 0));
            if (button == base.DragButton)
                OnPaint(null);
        }

        public void EmulateMouseUp(MouseButtons button, int x, int y)
        {
            base.OnMouseUp(new MouseEventArgs(button, 1, x, y, 0));
            OnPaint(null);
        }

        public void EmulateMouseClick(MouseButtons button, int x, int y)
        {
            base.OnMouseClick(new MouseEventArgs(button, 1, x, y, 0));
            //OnPaint(null);
        }

        public void EmulateMouseWheel(int x, int y, int wheelMotion = 1)
        {
            base.OnMouseWheel(new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 1, x, y, wheelMotion));
            //OnPaint(null);
        }
    }
}
