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
#if WINDOWS
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenMobile.Platform.Windows
{
    sealed class WinDisplayDeviceDriver : IDisplayDeviceDriver
    {
        readonly object display_lock = new object();

        #region Constructors

        public WinDisplayDeviceDriver()
        {
            RefreshDisplayDevices();
        }

        #endregion

        #region IDisplayDeviceDriver Members

        #region Private Members

        #region RefreshDisplayDevices

        public override void RefreshDisplayDevices()
        {
            DateTime start = DateTime.Now;
            lock (display_lock)
            {
                AvailableDevices.Clear();

                // We save all necessary parameters in temporary variables
                // and construct the device when every needed detail is available.
                // The main DisplayDevice constructor adds the newly constructed device
                // to the list of available devices.
                DisplayDevice opentk_dev;
                DisplayResolution opentk_dev_current_res = null;
                bool opentk_dev_primary = false;
                int device_count = 0;

                // Get available video adapters and enumerate all monitors
                WindowsDisplayDevice dev1 = new WindowsDisplayDevice(), dev2 = new WindowsDisplayDevice();
                while (Functions.EnumDisplayDevices(null, device_count++, dev1, 0))
                {
                    if ((dev1.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == DisplayDeviceStateFlags.None)
                        continue;

                    DeviceMode monitor_mode = new DeviceMode();
                    bool landscape=false;
                    // The second function should only be executed when the first one fails
                    // (e.g. when the monitor is disabled)
                    if (Functions.EnumDisplaySettingsEx(dev1.DeviceName.ToString(), DisplayModeSettingsEnum.CurrentSettings, monitor_mode, 0))
                    {
                        opentk_dev_current_res = new DisplayResolution(
                            monitor_mode.Position.X, monitor_mode.Position.Y,
                            monitor_mode.PelsWidth, monitor_mode.PelsHeight,
                            monitor_mode.BitsPerPel);
                        opentk_dev_primary =
                            (dev1.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != DisplayDeviceStateFlags.None;
                        landscape = ((monitor_mode.DisplayOrientation == 0) || (monitor_mode.DisplayOrientation == 2));
                    }

                    // Construct the OpenTK DisplayDevice through the accumulated parameters.
                    // The constructor will automatically add the DisplayDevice to the list
                    // of available devices.
                    opentk_dev = new DisplayDevice(
                        opentk_dev_current_res,
                        opentk_dev_primary,
                        opentk_dev_current_res.Bounds,
                        dev1.DeviceName);
                    opentk_dev.Landscape = landscape;
                    AvailableDevices.Add(opentk_dev);

                    if (opentk_dev_primary)
                        Primary = opentk_dev;
                }
            }
            Debug.Print("Displays enumerated in " + (DateTime.Now - start).Milliseconds.ToString() + "ms.");
        }

        #region HandleDisplaySettingsChanged

        void HandleDisplaySettingsChanged(object sender, EventArgs e)
        {
            RefreshDisplayDevices();
        }

        #endregion

        ~WinDisplayDeviceDriver()
        {
        }

        #endregion
        #endregion
        #endregion
    }
}
#endif