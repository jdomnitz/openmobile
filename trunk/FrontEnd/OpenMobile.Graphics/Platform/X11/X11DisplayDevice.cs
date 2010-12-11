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
#if LINUX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.Graphics;

namespace OpenMobile.Platform.X11
{
    sealed class X11DisplayDevice : IDisplayDeviceDriver
    {
        // Store a mapping between DisplayDevices and their default resolutions.
        readonly Dictionary<DisplayDevice, int> deviceToDefaultResolution = new Dictionary<DisplayDevice, int>();
        // Store a mapping between DisplayDevices and X11 screens.
        readonly Dictionary<DisplayDevice, int> deviceToScreen = new Dictionary<DisplayDevice, int>();
        // Keep the time when the config of each screen was last updated.
        readonly List<IntPtr> lastConfigUpdate = new List<IntPtr>();

        public bool xinerama_supported, xrandr_supported, xf86_supported;

        #region Constructors

        public X11DisplayDevice()
        {
            RefreshDisplayDevices();
        }

        #endregion

        #region Private Methods

        public override void RefreshDisplayDevices()
        {
            using (new XLock(API.DefaultDisplay))
            {
                List<DisplayDevice> devices = new List<DisplayDevice>();
                xinerama_supported = false;
                try
                {
                    xinerama_supported = QueryXinerama(devices);
                }
                catch
                {
                    Debug.Print("Xinerama query failed.");
                }

                if (!xinerama_supported)
                {
                    // We assume that devices are equivalent to the number of available screens.
                    // Note: this won't work correctly in the case of distinct X servers.
                    for (int i = 0; i < API.ScreenCount; i++)
                    {
                        DisplayDevice dev = new DisplayDevice();
                        dev.IsPrimary = i == Functions.XDefaultScreen(API.DefaultDisplay);
                        dev.Id = this;
                        devices.Add(dev);
                        deviceToScreen.Add(dev, i);
                    }
                }

                try
                {
                    xrandr_supported = QueryXRandR(devices);
                }
                catch { }

                if (!xrandr_supported)
                {
                    Debug.Print("XRandR query failed, falling back to XF86.");
                    try
                    {
                        xf86_supported = QueryXF86(devices);
                    }
                    catch { }

                    if (!xf86_supported)
                    {
                        Debug.Print("XF86 query failed, creating dummy display.");
                        QueryBestGuess(devices);
                    }
                }

                AvailableDevices.Clear();
                AvailableDevices.AddRange(devices);
                Primary = FindDefaultDevice(devices);
            }
        }

        static DisplayDevice FindDefaultDevice(IEnumerable<DisplayDevice> devices)
        {
            foreach (DisplayDevice dev in devices)
                if (dev.IsPrimary)
                    return dev;

            throw new InvalidOperationException("No primary display found. Please file a bug at http://www.opentk.com");
        }

        bool QueryXinerama(List<DisplayDevice> devices)
        {
            // Try to use Xinerama to obtain the geometry of all output devices.
            int event_base, error_base;
            if (NativeMethods.XineramaQueryExtension(API.DefaultDisplay, out event_base, out error_base) &&
                NativeMethods.XineramaIsActive(API.DefaultDisplay))
            {
                IList<XineramaScreenInfo> screens = NativeMethods.XineramaQueryScreens(API.DefaultDisplay);
                bool first = true;
                foreach (XineramaScreenInfo screen in screens)
                {
                    DisplayDevice dev = new DisplayDevice();
                    dev.Bounds = new Rectangle(screen.X, screen.Y, screen.Width, screen.Height);
                    if (first)
                    {
                        // We consider the first device returned by Xinerama as the primary one.
                        // Makes sense conceptually, but is there a way to verify this?
                        dev.IsPrimary = true;
                        first = false;
                    }
                    dev.BitsPerPixel = FindCurrentDepth(devices.Count);
                    dev.Landscape = (screen.Width > screen.Height);
                    dev.Id = this;
                    devices.Add(dev);
                    // It seems that all X screens are equal to 0 is Xinerama is enabled, at least on Nvidia (verify?)
                    deviceToScreen.Add(dev, 0 /*screen.ScreenNumber*/);
                }
            }
            return (devices.Count > 0);
        }

        bool QueryXRandR(List<DisplayDevice> devices)
        {
            // Get available resolutions. Then, for each resolution get all available rates.
            foreach (DisplayDevice dev in devices)
            {
                int screen = deviceToScreen[dev];

                IntPtr timestamp_of_last_update;
                Functions.XRRTimes(API.DefaultDisplay, screen, out timestamp_of_last_update);
                lastConfigUpdate.Add(timestamp_of_last_update);

                List<DisplayResolution> available_res = new List<DisplayResolution>();

                int current_depth = FindCurrentDepth(screen);
                IntPtr screen_config = Functions.XRRGetScreenInfo(API.DefaultDisplay, Functions.XRootWindow(API.DefaultDisplay, screen));
                ushort current_rotation;
                int current_resolution_index = Functions.XRRConfigCurrentConfiguration(screen_config, out current_rotation);
                dev.Landscape=!(((current_rotation&2)==2)||((current_rotation&8)==8));
                if (dev.Bounds == Rectangle.Empty)
                    dev.Bounds = new Rectangle(0, 0, available_res[current_resolution_index].Width, available_res[current_resolution_index].Height);
                dev.BitsPerPixel = current_depth;

                deviceToDefaultResolution.Add(dev, current_resolution_index);
            }

            return true;
        }

        bool QueryXF86(List<DisplayDevice> devices)
        {
            int major;
            int minor;

            try
            {
                if (!API.XF86VidModeQueryVersion(API.DefaultDisplay, out major, out minor))
                    return false;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            
            int currentScreen = 0;
            foreach (DisplayDevice dev in devices)
            {
                int x;
                int y;
                API.XF86VidModeGetViewPort(API.DefaultDisplay, currentScreen, out x, out y);
                int pixelClock;
                API.XF86VidModeModeLine currentMode;
                API.XF86VidModeGetModeLine(API.DefaultDisplay, currentScreen, out pixelClock, out currentMode);
                dev.Bounds = new Rectangle(x, y, currentMode.hdisplay, (currentMode.vdisplay == 0) ? currentMode.vsyncstart : currentMode.vdisplay);
                dev.BitsPerPixel = FindCurrentDepth(currentScreen);
                currentScreen++;
            }
            return true;
        }

        void QueryBestGuess(List<DisplayDevice> devices)
        {
            foreach (DisplayDevice device in devices)
            {
                device.BitsPerPixel = 24;
                device.Bounds = new Rectangle(0, 0, 800, 600);
                device.IsPrimary = true;
            }
        }

        #region private static int FindCurrentDepth(int screen)

        static int FindCurrentDepth(int screen)
        {
            return (int)Functions.XDefaultDepth(API.DefaultDisplay, screen);
        }

        #endregion

        #endregion

        #region NativeMethods

        static class NativeMethods
        {
            const string Xinerama = "libXinerama";

            [DllImport(Xinerama)]
            public static extern bool XineramaQueryExtension(IntPtr dpy, out int event_basep, out int error_basep);

            [DllImport(Xinerama)]
            public static extern bool XineramaIsActive(IntPtr dpy);

            [DllImport(Xinerama)]
            static extern IntPtr XineramaQueryScreens(IntPtr dpy, out int number);

            public static IList<XineramaScreenInfo> XineramaQueryScreens(IntPtr dpy)
            {
                int number;
                IntPtr screen_ptr = XineramaQueryScreens(dpy, out number);
                List<XineramaScreenInfo> screens = new List<XineramaScreenInfo>(number);

                unsafe
                {
                    XineramaScreenInfo* ptr = (XineramaScreenInfo*)screen_ptr;
                    while (--number >= 0)
                    {
                        screens.Add(*ptr);
                        ptr++;
                    }
                }

                return screens;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct XineramaScreenInfo
        {
            public int ScreenNumber;
            public short X;
            public short Y;
            public short Width;
            public short Height;
        }

        #endregion
    }
}
#endif