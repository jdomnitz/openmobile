#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2010 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenMobile.Graphics;

namespace OpenMobile
{
    /// <summary>
    /// Defines a display device on the underlying system, and provides
    /// methods to query and change its display parameters.
    /// </summary>
    public class DisplayDevice
    {
        // TODO: Add properties that describe the 'usable' size of the Display, i.e. the maximized size without the taskbar etc.
        // TODO: Does not detect changes to primary device.

        #region Fields

        bool primary;
		bool landscape=true;
        Rectangle bounds;
        DisplayResolution current_resolution = new DisplayResolution();

        internal object Id; // A platform-specific id for this monitor

        static readonly object display_lock = new object();
        static DisplayDevice primary_display;

        static Platform.IDisplayDeviceDriver implementation;

        #endregion

        #region Constructors

        static DisplayDevice()
        {
            implementation = Platform.Factory.Default.CreateDisplayDeviceDriver();
        }

        internal DisplayDevice(DisplayResolution currentResolution, bool primary,
             Rectangle bounds, object id)
            : this()
        {
            this.current_resolution = currentResolution;
            IsPrimary = primary;
            //this.available_resolutions.AddRange(availableResolutions);
            this.bounds = bounds == Rectangle.Empty ? currentResolution.Bounds : bounds;
            this.Id = id;
        }

        #endregion

        #region --- Public Methods ---

        #region public Rectangle Bounds

        /// <summary>
        /// Gets the bounds of this instance in pixel coordinates..
        /// </summary>
        public Rectangle Bounds
        {
            get { return bounds; }
            internal set
            {
                bounds = value;
                current_resolution.Height = bounds.Height;
                current_resolution.Width = bounds.Width;
            }
        }

        #endregion

        #region public int Width

        /// <summary>Gets a System.Int32 that contains the width of this display in pixels.</summary>
        public int Width { get { return current_resolution.Width; } }

        #endregion

        #region public int Height

        /// <summary>Gets a System.Int32 that contains the height of this display in pixels.</summary>
        public int Height { get { return current_resolution.Height; } }

        #endregion

        #region public int BitsPerPixel

        /// <summary>Gets a System.Int32 that contains number of bits per pixel of this display. Typical values include 8, 16, 24 and 32.</summary>
        public int BitsPerPixel
        {
            get { return current_resolution.BitsPerPixel; }
            internal set { current_resolution.BitsPerPixel = value; }
        }

        #endregion

		/// <summary>
        /// Returns true if the display is in landscape mode...false if the display is in portrait mode
        /// </summary>
        public bool Landscape
        {
            get { return landscape; }
            set { landscape = value; }
        }

        #region public bool IsPrimary

        /// <summary>Gets a System.Boolean that indicates whether this Display is the primary Display in systems with multiple Displays.</summary>
        public bool IsPrimary
        {
            get { return primary; }
            internal set
            {
                if (value && primary_display != null && primary_display != this)
                    primary_display.IsPrimary = false;

                lock (display_lock)
                {
                    primary = value;
                    if (value)
                        primary_display = this;
                }
            }
        }

        #endregion

        #region public static IList<DisplayDevice> AvailableDisplays

        /// <summary>
        /// Gets the list of available <see cref="DisplayDevice"/> objects.
        /// This function allocates memory.
        /// </summary>
        public static IList<DisplayDevice> AvailableDisplays
        {
            get
            {
                return implementation.AvailableDevices;
            }
        }
        internal static void RefreshDisplays()
        {
            implementation.RefreshDisplayDevices();
        }
        #endregion

        #region public static DisplayDevice Default

        /// <summary>Gets the default (primary) display of this system.</summary>
        public static DisplayDevice Default
        {
            get { return implementation.Primary; }
        }

        #endregion

        #endregion

        internal DisplayDevice()
        {
            //
        }

        #region --- Overrides ---

        #region public override string ToString()

        /// <summary>
        /// Returns a System.String representing this DisplayDevice.
        /// </summary>
        /// <returns>A System.String representing this DisplayDevice.</returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", IsPrimary ? "Primary" : "Secondary",
                Bounds.ToString());
        }

        #endregion

        #endregion
    }
}