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
using OpenMobile.Controls;
using OpenMobile.Input;
using OpenMobile.Data;
using System;
using System.IO;
using System.Collections.Generic;

namespace OpenMobile
{
    public static class InputRouter
    {
        static IInputDriver driver;

        public const string DefaultUnit = "OS Default";
        public const string DisabledUnit = "Disabled";

        /// <summary>
        /// Current device mappings
        /// </summary>
        static string[] KeyboardMapping;

        /// <summary>
        /// Current device mappings
        /// </summary>
        static string[] MouseMapping;

        public static string[] Keyboards
        {
            get
            {
                if (driver == null)
                    return new string[] { "" };
                string[] ret = new string[driver.Keyboard.Count];
                for (int i = 0; i < driver.Keyboard.Count; i++)
                    ret[i] = driver.Keyboard[i].ToString();
                return ret;
            }
        }
        public static string[] KeyboardsRequestedMapping
        {
            get
            {
                string[] Units = new string[Core.theHost.ScreenCount];
                using (PluginSettings s = new PluginSettings())
                    for (int i = 0; i < Core.theHost.ScreenCount; i++)
                        Units[i] = s.getSetting("Screen" + (i+1).ToString() + ".Keyboard");
                return Units;
            }
        }
        public static string[] KeyboardsMapped
        {
            get
            {
                return KeyboardMapping;
            }
        }
        public static string[] KeyboardsUnMapped
        {
            get
            {
                // Unmapped units list
                List<string> Unmapped = new List<string>();

                // Get current device list
                string[] Units = Keyboards;

                // Get current mapped list
                string[] Mapped = KeyboardsMapped;

                // Loop trough units and check for mapping
                for (int i = 0; i < Units.Length; i++)
                {
                    bool Used = false;
                    for (int i2 = 0; i2 < Mapped.Length; i2++)
                    {   // Check if this unit is mapped
                        if (Units[i] == Mapped[i2])
                        {
                            Used = true;
                            break;
                        }
                    }

                    // No it is not mapped, add to list
                    if (!Used)
                        Unmapped.Add(Units[i]);
                
                }

                // Return list
                return Unmapped.ToArray();
            }
        }
        public static string[] KeyboardsUnRequested
        {
            get
            {
                // Unmapped units list
                List<string> Unmapped = new List<string>();

                // Get current device list
                string[] Units = Keyboards;

                // Get current mapped list
                string[] Mapped = KeyboardsRequestedMapping;

                // Loop trough units and check for mapping
                for (int i = 0; i < Units.Length; i++)
                {
                    bool Used = false;
                    for (int i2 = 0; i2 < Mapped.Length; i2++)
                    {   // Check if this unit is mapped
                        if (Units[i] == Mapped[i2])
                        {
                            Used = true;
                            break;
                        }
                    }

                    // No it is not mapped, add to list
                    if (!Used)
                        Unmapped.Add(Units[i]);

                }

                // Return list
                return Unmapped.ToArray();
            }
        }
        public static string[] GetKeyboardsDeviceListForScreen(int screen)
        {
            // Add all unrequested units to list
            List<string> Devices = new List<string>(KeyboardsUnRequested);

            // Add current mapping for screen to list (if not already present)
            if (Devices.Find(x => x == KeyboardMapping[screen]) == null)
                Devices.Add(KeyboardMapping[screen]);

            // Sort list
            Devices.Sort();

            // Add option for no mapping
            if (Devices.Find(x => x == DisabledUnit) == null)
                Devices.Insert(0, DisabledUnit);

            // Add option for default OS unit
            Devices.Insert(0, DefaultUnit);

            // Return data
            return Devices.ToArray();
        }
        public static string GetKeyboardsCurrentDeviceForScreen(int screen)
        {
            // Get current mapping
            return KeyboardMapping[screen];
        }
        public static bool KeyboardsUnitAvailable(string Device)
        {
            
            if (Device == DisabledUnit)
                return true;

            foreach (string s in KeyboardsMapped)
                if ((s != DisabledUnit) && (s == Device))
                    return false;
            return true;
        }
        public static string KeyboardDefaultUnit()
        {
            return driver.Keyboard[0].ToString();
        }

        public static string[] Mice
        {
            get
            {
                if (driver == null)
                    return new string[] { "" };

                string[] ret = new string[driver.Mouse.Count];
                for (int i = 0; i < driver.Mouse.Count; i++)
                    ret[i] = driver.Mouse[i].ToString();
                return ret;
            }
        }
        public static string[] MiceRequestedMapping
        {
            get
            {
                string[] Units = new string[Core.theHost.ScreenCount];
                using (PluginSettings s = new PluginSettings())
                    for (int i = 0; i < Core.theHost.ScreenCount; i++)
                        Units[i] = s.getSetting("Screen" + (i+1).ToString() + ".Mouse");
                return Units;
            }
        }
        public static string[] MiceMapped
        {
            get
            {
                return MouseMapping;
            }
        }
        public static string[] MiceUnMapped
        {
            get
            {
                // Unmapped units list
                List<string> Unmapped = new List<string>();

                // Get current device list
                string[] Units = Mice;

                // Get current mapped list
                string[] Mapped = MiceMapped;

                // Loop trough units and check for mapping
                for (int i = 0; i < Units.Length; i++)
                {
                    bool Used = false;
                    for (int i2 = 0; i2 < Mapped.Length; i2++)
                    {   // Check if this unit is mapped
                        if (Units[i] == Mapped[i2])
                        {
                            Used = true;
                            break;
                        }
                    }

                    // No it is not mapped, add to list
                    if (!Used)
                        Unmapped.Add(Units[i]);
                }

                // Return list
                return Unmapped.ToArray();
            }
        }
        public static string[] MiceUnRequested
        {
            get
            {
                // Unmapped units list
                List<string> Unmapped = new List<string>();

                // Get current device list
                string[] Units = Mice;

                // Get current mapped list
                string[] Mapped = MiceRequestedMapping;

                // Loop trough units and check for mapping
                for (int i = 0; i < Units.Length; i++)
                {
                    bool Used = false;
                    for (int i2 = 0; i2 < Mapped.Length; i2++)
                    {   // Check if this unit is mapped
                        if (Units[i] == Mapped[i2])
                        {
                            Used = true;
                            break;
                        }
                    }

                    // No it is not mapped, add to list
                    if (!Used)
                        Unmapped.Add(Units[i]);
                }

                // Return list
                return Unmapped.ToArray();
            }
        }
        public static string MiceDefaultUnit()
        {
            for (int i = driver.Mouse.Count-1; i >= 0; i--)
            {
                if (driver.Mouse[i].ToString().Contains(" mouse "))
                    return driver.Mouse[i].ToString();
            }
            return "";
        }
        public static string[] GetMiceDeviceListForScreen(int screen)
        {
            // Add all unrequested units to list
            List<string> Devices = new List<string>(MiceUnRequested);

            // Add current mapping for screen to list (if not already present)
            if (Devices.Find(x => x == MouseMapping[screen]) == null)
                Devices.Add(MouseMapping[screen]);

            // Sort list
            Devices.Sort();

            // Add option for no mapping
            if (Devices.Find(x => x == DisabledUnit) == null)
                Devices.Insert(0,DisabledUnit);

            // Add option for default OS unit
            Devices.Insert(0, DefaultUnit);

            // Return data
            return Devices.ToArray();
        }
        public static string GetMiceDeviceCurrentForScreen(int screen)
        {
            // Get current mapping
            return MouseMapping[screen];
        }
        public static bool MiceUnitAvailable(string Device)
        {
            if (Device == DisabledUnit)
                return true;

            foreach (string s in MiceMapped)
                if ((s != DisabledUnit) && (s == Device))
                    return false;
            return true;
        }

        /// <summary>
        /// Gets mouse device name from mapping index
        /// </summary>
        /// <param name="MapIndex">Index to get device name for</param>
        /// <returns>Device name</returns>
        public static string GetMouseDeviceName(int MapIndex)
        {
            // Check for default values
            switch (MapIndex)
            {
                case -1:
                    return "Default Mouse";
                case -2:
                    return "Disabled";
                case -3:
                    return "Not Found";
            }
            try
            {
                return driver.Mouse[MapIndex].ToString();
            }
            catch
            {
                return "Not Found";
            }
        }

        /// <summary>
        /// Gets mouse mapping index from devicename (mapping name)
        /// "Default mouse" and "Disabled" are also valid strings.
        /// </summary>
        /// <param name="DeviceName">Device name</param>
        /// <returns>Mapping index</returns>
        public static int GetMouseMapIndex(string DeviceName)
        {
            // Ignore case
            //DeviceName = DeviceName.ToLower();

            // Check for default values
            switch (DeviceName)
            {
                case DefaultUnit:
                    return -1;
                case DisabledUnit:
                    return -2;
            }

            // Find and return index
            for (int i = 0; i < driver.Mouse.Count; i++)
            {
                if (driver.Mouse[i].ToString().ToLower() == DeviceName.ToLower())
                    return i;
            }

            // Return -3 if not found
            return -3;
        }

        /// <summary>
        /// Gets keyboard device name from mapping index
        /// </summary>
        /// <param name="MapIndex">Index to get device name for</param>
        /// <returns>Device name</returns>
        public static string GetKeyboardDeviceName(int MapIndex)
        {
            // Check for default values
            switch (MapIndex)
            {
                case -1:
                    return "Default Keyboard";
                case -2:
                    return "Disabled";
                case -3:
                    return "Not found";
            }
            try
            {
                return driver.Keyboard[MapIndex].ToString();
            }
            catch
            {
                return "Not Found";
            }
        }

        /// <summary>
        /// Gets keyboard mapping index from devicename (mapping name)
        /// "Default keyboard" and "Disabled" are also valid strings.
        /// </summary>
        /// <param name="DeviceName">Device name</param>
        /// <returns>Mapping index</returns>
        public static int GetKeyboardMapIndex(string DeviceName)
        {
            // Ignore case
            //DeviceName = DeviceName.ToLower();

            // Check for default values
            switch (DeviceName)
            {
                case DefaultUnit:
                    return -1;
                case DisabledUnit:
                    return -2;
            }

            // Find and return index
            for (int i = 0; i < driver.Keyboard.Count; i++)
            {
                if (driver.Keyboard[i].ToString().ToLower() == DeviceName.ToLower())
                    return i;
            }

            // Return -3 if not found
            return -3;
        }


        public static void Dispose()
        {
            // Unhide default OS mouse
            Core.RenderingWindows[0].DefaultMouse.ShowCursor(Core.RenderingWindows[0].WindowInfo);
            if (driver != null)
            {
                driver.Dispose();
            }
        }
        public static void Initialize()
        {
#if WINDOWS
            if (Configuration.RunningOnWindows)
                driver = new Platform.Windows.WinRawInput();
#endif
#if LINUX
#if WINDOWS
            else
#endif
                if (Configuration.RunningOnX11)
                    driver = new Platform.X11.X11RawInput();
#endif

            KeyboardMapping = new string[Core.theHost.ScreenCount];
            for (int i = 0; i < KeyboardMapping.Length; i++)
                KeyboardMapping[i] = "";
                
            MouseMapping = new string[Core.theHost.ScreenCount];
            for (int i = 0; i < MouseMapping.Length; i++)
                MouseMapping[i] = "";

            // Map units
            mapKeyboards();
            mapMice();

            // Connect events 
            #region Keyboard
            
            if (driver != null)
            {
                // Connect default OS keyboard
                for (int i = 0; i < Core.theHost.ScreenCount; i++)
                {
                    if (Core.RenderingWindows[i].DefaultKeyboard != null)
                    {
                        Core.RenderingWindows[i].DefaultKeyboard.KeyUp += new System.EventHandler<KeyboardKeyEventArgs>(SourceDown);
                        Core.RenderingWindows[i].DefaultKeyboard.KeyDown += new System.EventHandler<KeyboardKeyEventArgs>(SourceUp);
                        Core.RenderingWindows[i].DefaultKeyboard.Instance = (i * -1) - 1; // Instance is negative screen number for default units with an offset of one
                    }
                }
                // Only connect what's needed
                for (int i = 0; i < KeyboardMapping.Length; i++)
                {
                    if (GetKeyboardMapIndex(KeyboardMapping[i])>=0)
                    {
                        driver.Keyboard[GetKeyboardMapIndex(KeyboardMapping[i])].KeyDown += new System.EventHandler<KeyboardKeyEventArgs>(SourceDown);
                        driver.Keyboard[GetKeyboardMapIndex(KeyboardMapping[i])].KeyUp += new System.EventHandler<KeyboardKeyEventArgs>(SourceUp);
                    }
                }
            }

            #endregion
            #region Mouse

            if (driver != null)
            {
                // Connect default OS Mouse
                for (int i = 0; i < Core.theHost.ScreenCount; i++)
                {
                    if (Core.RenderingWindows[i].DefaultMouse != null)
                    {
                        Core.RenderingWindows[i].DefaultMouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(dev_ButtonUp);
                        Core.RenderingWindows[i].DefaultMouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(dev_ButtonDown);
                        Core.RenderingWindows[i].DefaultMouse.MouseClick += new EventHandler<MouseButtonEventArgs>(dev_MouseClick);
                        Core.RenderingWindows[i].DefaultMouse.Move += new EventHandler<MouseMoveEventArgs>(dev_Move);

                        // Set mouse instance number (negative instance number = default os unit)
                        Core.RenderingWindows[i].DefaultMouse.Instance = (i * -1) - 1; // Instance is negative screen number for default units with an offset of one

                        // Limit mouse area to the size of the screen
                        Core.RenderingWindows[i].DefaultMouse.SetBounds(DisplayDevice.AvailableDisplays[BuiltInComponents.Host.StartupScreen > 0 ? BuiltInComponents.Host.StartupScreen : i].Width, DisplayDevice.AvailableDisplays[BuiltInComponents.Host.StartupScreen > 0 ? BuiltInComponents.Host.StartupScreen : i].Height);
                    }
                }

                // Only connect what's needed
                for (int i = 0; i < MouseMapping.Length; i++)
                {
                    if (GetMouseMapIndex(MouseMapping[i]) >= 0)
                    {
                        driver.Mouse[GetMouseMapIndex(MouseMapping[i])].ButtonDown += new EventHandler<MouseButtonEventArgs>(dev_ButtonDown);
                        driver.Mouse[GetMouseMapIndex(MouseMapping[i])].ButtonUp += new EventHandler<MouseButtonEventArgs>(dev_ButtonUp);
                        driver.Mouse[GetMouseMapIndex(MouseMapping[i])].MouseClick += new EventHandler<MouseButtonEventArgs>(dev_MouseClick);
                        driver.Mouse[GetMouseMapIndex(MouseMapping[i])].Move += new EventHandler<MouseMoveEventArgs>(dev_Move);
                    }
                }
            }

            #endregion

            // Connect to resize events of the main window (All windows follows the same windowstate) to be able to 
            // connect and disconnect the default mouse and keyboard devices
            Core.RenderingWindows[0].Resize += new EventHandler<EventArgs>(InputRouter_Resize);

            // Raise system event, informing that inputrouter has completed
            BuiltInComponents.Host.raiseSystemEvent(eFunction.inputRouterInitialized, driver.Keyboard.Count.ToString(), driver.Mouse.Count.ToString(), String.Empty);

            BuiltInComponents.Host.OnSystemEvent += new OpenMobile.Plugin.SystemEvent(Host_OnSystemEvent);

        }

        static void Host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.closeProgram)
            {
                // Unhide default OS mouse (in case it was hidden at time of crash)
                Core.RenderingWindows[0].DefaultMouse.ShowCursor(Core.RenderingWindows[0].WindowInfo);
            }

        }

        #region Handle resize and change in windowstates

        static WindowState PreviousWindowState = WindowState.Minimized;

        static void InputRouter_Resize(object sender, EventArgs e)
        {
            // Save current state so we only execute this code once pr state change
            WindowState CurrentState = ((RenderingWindow)sender).WindowState;
            if (PreviousWindowState != CurrentState)
            {
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen" + ((RenderingWindow)sender).Screen + " Windowstate change: " + ((RenderingWindow)sender).WindowState);

                // Save current windowstate
                PreviousWindowState = CurrentState;

                if (((RenderingWindow)sender).WindowState == WindowState.Fullscreen)
                {   // Full screen, Hide task bar
                    Core.RenderingWindows[0].DefaultMouse.HideCursor(Core.RenderingWindows[0].WindowInfo);
                }
                else
                {   // windowed mode, connect default devices
                    Core.RenderingWindows[0].DefaultMouse.ShowCursor(Core.RenderingWindows[0].WindowInfo);
                }
            }
        }

        #endregion

        #region Mouse Device Events

        static internal void dev_Move(object sender, MouseMoveEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            if (Core.RenderingWindows[0].WindowState == WindowState.Fullscreen)
            {
                
                // Did this event come from the default OS unit?
                if ((int)sender < 0)
                {
                    int SenderScreen = ((int)sender + 1) * -1;
                    if (MouseMapping[SenderScreen] == DefaultUnit)
                    {
                        Core.RenderingWindows[SenderScreen].defaultMouse = true;
                        Core.RenderingWindows[SenderScreen].RenderingWindow_MouseMove(SenderScreen, e);
                    }
                }
                else
                {
                    for (int i = 0; i < MouseMapping.Length; i++)
                    {
                        if (GetMouseMapIndex(MouseMapping[i]) == (int)sender)
                        {
                            // Reset mouse location
                            ResetMouseLocation();

                            // Blocks device input if video is playing
                            if (Core.RenderingWindows[i].VideoPlaying && Core.theHost.GetVideoPosition(i).Contains(e.Location))
                                return;
                            Core.RenderingWindows[i].defaultMouse = (MouseMapping[i] == DefaultUnit);
                            Core.RenderingWindows[i].RenderingWindow_MouseMove(i, e);
                        }
                    }
                }
            }
            else if ((int)sender < 0)
            {
                int i = ((int)sender + 1) * -1;
                Core.RenderingWindows[i].defaultMouse = (MouseMapping[i] == DefaultUnit);
                Core.RenderingWindows[i].RenderingWindow_MouseMove(i, e);
            }
        }

        static internal void dev_MouseClick(object sender, MouseButtonEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            if (Core.RenderingWindows[0].WindowState == WindowState.Fullscreen)
            {
                // Did this event come from the default OS unit?
                if ((int)sender < 0)
                {
                    int SenderScreen = ((int)sender + 1) * -1;
                    if (MouseMapping[SenderScreen] == DefaultUnit)
                    {
                        Core.RenderingWindows[SenderScreen].defaultMouse = true;
                        Core.RenderingWindows[SenderScreen].RenderingWindow_MouseClick(SenderScreen, e);
                    }
                }
                else
                {

                    // Return if event comes from a default unit (indicated by a negative sender)
                    //if ((int)sender < 0)
                    //    return;

                    for (int i = 0; i < MouseMapping.Length; i++)
                    {
                        if (GetMouseMapIndex(MouseMapping[i]) == (int)sender)
                        {
                            // TODO: CLEANUP! This should be skin dependent!
                            //if (Core.RenderingWindows[i].VideoPlaying && Core.theHost.GetVideoPosition(Core.theHost.instanceForScreen(i)).Contains(e.Location))
                            //{
                            //    Core.theHost.execute(eFunction.showVideoWindow, Core.theHost.instanceForScreen(i).ToString());
                            //    return;
                            //}
                            Core.RenderingWindows[i].RenderingWindow_MouseClick(i, e);
                        }
                    }
                }
            }
            else if ((int)sender < 0)
            {
                int i = ((int)sender + 1) * -1;
                Core.RenderingWindows[i].RenderingWindow_MouseClick(i, e);
            }
        }

        static internal void dev_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            if (Core.RenderingWindows[0].WindowState == WindowState.Fullscreen)
            {
                // Return if event comes from a default unit (indicated by a negative sender)
                //if ((int)sender < 0)
                //    return;

                // Did this event come from the default OS unit?
                if ((int)sender < 0)
                {
                    int SenderScreen = ((int)sender + 1) * -1;
                    if (MouseMapping[SenderScreen] == DefaultUnit)
                    {
                        Core.RenderingWindows[SenderScreen].defaultMouse = true;
                        Core.RenderingWindows[SenderScreen].RenderingWindow_MouseUp(SenderScreen, e);
                    }
                }
                else
                {
                    for (int i = 0; i < MouseMapping.Length; i++)
                    {
                        if (GetMouseMapIndex(MouseMapping[i]) == (int)sender)
                            Core.RenderingWindows[i].RenderingWindow_MouseUp(i, e);
                    }
                }
            }
            else if ((int)sender < 0)
            {
                int i = ((int)sender + 1) * -1;
                Core.RenderingWindows[i].RenderingWindow_MouseUp(i, e);
            }
        }

        static internal void dev_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            if (Core.RenderingWindows[0].WindowState == WindowState.Fullscreen)
            {
                
                // Return if event comes from a default unit (indicated by a negative sender)
                //if ((int)sender < 0)
                //    return;

                // Did this event come from the default OS unit?
                if ((int)sender < 0)
                {
                    int SenderScreen = ((int)sender + 1) * -1;
                    if (MouseMapping[SenderScreen] == DefaultUnit)
                    {
                        Core.RenderingWindows[SenderScreen].defaultMouse = true;
                        Core.RenderingWindows[SenderScreen].RenderingWindow_MouseDown(SenderScreen, e);
                    }
                }
                else
                {
                    // Reset mouse location
                    ResetMouseLocation();
                    
                    for (int i = 0; i < MouseMapping.Length; i++)
                    {
                        if (GetMouseMapIndex(MouseMapping[i]) == (int)sender)
                        {
                            if (Core.RenderingWindows[i].VideoPlaying && Core.theHost.GetVideoPosition(i).Contains(e.Location))
                                return;

                            Core.RenderingWindows[i].RenderingWindow_MouseDown(i, e);
                        }
                    }
                }
            }
            else if ((int)sender < 0)
            {
                int i = ((int)sender + 1) * -1;
                Core.RenderingWindows[i].RenderingWindow_MouseDown(i, e);
            }
        }

        #endregion

        #region Mouse Reset location

        private static OpenMobile.Graphics.Point ResetMousePoint = new OpenMobile.Graphics.Point(0,0);
        /// <summary>
        /// Reset OS mouse location to stop touch screens from moving the mouse
        /// </summary>
        private static void ResetMouseLocation()
        {
            Core.RenderingWindows[0].DefaultMouse.Location = ResetMousePoint;
        }

        #endregion

        private static void mapKeyboards()
        {
            // TODO_ : Add support for default keyboard based on focus (How do we detect focus?)

            // exit if there is no keyboards available
            if ((driver == null) || (driver.Keyboard == null) || (driver.Keyboard.Count == 0))
                return;

            // Select default unit
            string DefaultKeyboard = driver.Keyboard[0].ToString();
            
            // List available devices
            string[] Devices = new string[driver.Keyboard.Count+1];
            for (int i = 0; i < driver.Keyboard.Count; i++)
                Devices[i] = i.ToString() + " > " + driver.Keyboard[i].ToString();
            Devices[Devices.Length-1] = "Default keyboard > " + DefaultKeyboard;
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Available keyboard devices:", Devices);

            // List requested mappings
            Devices = KeyboardsRequestedMapping;
            for (int i = 0; i < Devices.Length; i++)
                Devices[i] = i.ToString() + " > " + Devices[i];
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Requested keyboard mappings:", Devices);                

            string[] RequestedMapping = KeyboardsRequestedMapping;

            // Loop trough each screen and do mapping
            for (int screen = 0; screen < Core.theHost.ScreenCount; screen++)
            {
                if (RequestedMapping[screen] == DisabledUnit)
                {   // Disabled mapping                
                    KeyboardMapping[screen] = DisabledUnit;
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to keyboard: " + KeyboardMapping[screen]);
                }
                else if ((RequestedMapping[screen] == DefaultUnit) || (RequestedMapping[screen] == String.Empty))
                {   // Default unit mapping requested or mapping missing
                    if (KeyboardsUnitAvailable(DefaultKeyboard))
                    {
                        KeyboardMapping[screen] = DefaultKeyboard;
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to keyboard: " + KeyboardMapping[screen]);
                    }
                    else
                    {
                        KeyboardMapping[screen] = DisabledUnit;
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to keyboard: " + KeyboardMapping[screen]);
                    }
                }
                else
                {   // Try to find a match in the available units list
                    bool Mapped = false;
                    for (int i = 0; i < driver.Keyboard.Count; i++)
                    {
                        if (driver.Keyboard[i].ToString() == RequestedMapping[screen])
                        {   // Unit found, Check if it's available
                            if (KeyboardsUnitAvailable(driver.Keyboard[i].ToString()))
                            {
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Trying to map keyboard index " + i.ToString() + " to screen " + screen.ToString() + "...");
                                KeyboardMapping[screen] = driver.Keyboard[i].ToString();
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to keyboard: " + KeyboardMapping[screen]);
                            }
                            else
                            {
                                KeyboardMapping[screen] = DisabledUnit;
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Requested map for screen " + screen.ToString() + " already used, mapping to " + KeyboardMapping[screen] + " instead");
                            }
                            Mapped = true;
                            break;
                        }
                    }

                    // Was mapping successful?
                    if (!Mapped)
                    {   // No, remap to default keyboard if available
                        KeyboardMapping[screen] = DefaultUnit;

                        // Write debug information to log
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Screen " + screen.ToString() + " unable to map requested keyboard (" + RequestedMapping[screen] + "), mapped to " + KeyboardMapping[screen] + " instead");
                    }

                }
            }
        }

        private static void mapMice()
        {
            // exit if there is no mice available
            if ((driver == null) || (driver.Mouse == null) || (driver.Mouse.Count == 0))
                return;

            // List available mouse devices
            string[] Devices = new string[driver.Mouse.Count];
            for (int i = 0; i < driver.Mouse.Count; i++)
                Devices[i] = i.ToString() + " > " + driver.Mouse[i].ToString();
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Available mouse devices:",Devices);

            // List requested mappings
            Devices = MiceRequestedMapping;
            for (int i = 0; i < Devices.Length; i++)
                Devices[i] = i.ToString() + " > " + Devices[i];
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Requested mouse mappings:", Devices);   
            
            // Check for available mouse
            if (driver.Mouse[0] == null)
            {
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "This program requires one or more mice (touch) input units, no unit was detected. Terminating program");
                BuiltInComponents.Host.execute(eFunction.closeProgram);
            }
 
            string[] RequestedMapping = MiceRequestedMapping;

            // Set mouse device instance and bounds
            OpenMobile.Graphics.Rectangle DesktopBounds = getVirtualBounds();
            
            // Loop trough each screen and do mapping
            for (int screen = 0; screen < Core.theHost.ScreenCount; screen++)
            {
                if ((RequestedMapping[screen] == DisabledUnit))
                {   // Disabled mapping
                    MouseMapping[screen] = DisabledUnit;
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to Mouse: " + MouseMapping[screen]);
                }
                else if ((RequestedMapping[screen] == DefaultUnit) || (RequestedMapping[screen] == String.Empty) || (RequestedMapping[screen] == ""))
                {   // Default unit mapping requested or mapping missing

                    MouseMapping[screen] = DefaultUnit;
                    Core.RenderingWindows[screen].currentMouse = driver.Mouse[0];
                    Core.RenderingWindows[screen].defaultMouse = true;
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to Mouse: " + MouseMapping[screen]);
                }
                else
                {   // Try to find a match in the available units list
                    bool Mapped = false;
                    for (int i = 0; i < driver.Mouse.Count; i++)
                    {
                        if (driver.Mouse[i].ToString() == RequestedMapping[screen])
                        {   // Unit found, Check if it's available
                            if (MiceUnitAvailable(driver.Mouse[i].ToString()))
                            {
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Trying to map mouse index " + i + " to screen " + screen.ToString() + "...");
                                MouseMapping[screen] = driver.Mouse[i].ToString();
                                Core.RenderingWindows[screen].currentMouse = driver.Mouse[i];
                                driver.Mouse[i].SetBounds(DisplayDevice.AvailableDisplays[i].Width, DisplayDevice.AvailableDisplays[i].Height);
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Screen " + screen.ToString() + " mapped to Mouse: " + MouseMapping[screen]);
                            }
                            else
                            {
                                MouseMapping[screen] = DisabledUnit;
                                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Requested map for screen " + screen.ToString() + " already used, mapping to " + MouseMapping[screen] + " instead");
                            }
                            Mapped = true;
                            break;
                        }
                    }

                    // Was mapping successful?
                    if (!Mapped)
                    {   // No, remap to default Mouse if available
                        MouseMapping[screen] = DefaultUnit;
                        Core.RenderingWindows[screen].currentMouse = driver.Mouse[0];
                        Core.RenderingWindows[screen].defaultMouse = true;

                        // Write debug information to log
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Screen " + screen.ToString() + " unable to map requested Mouse (" + RequestedMapping[screen] + "), mapped to " + MouseMapping[screen] + " instead");
                    }

                }

                int MouseMapIndex = GetMouseMapIndex(MouseMapping[screen]);
                if (MouseMapIndex >= 0)    // Let's make sure we only prosess valid devices
                {
                    // Update device environment
                    driver.Mouse[MouseMapIndex].SetBounds(DisplayDevice.AvailableDisplays[screen].Width, DisplayDevice.AvailableDisplays[screen].Height);
                    driver.Mouse[MouseMapIndex].SetDeviceEnvironmentInfo(DisplayDevice.AvailableDisplays[screen].Bounds, DesktopBounds);
                }
            }

            // Write display debug info
            List<string> Texts = new List<string>();
            for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
                Texts.Add(i.ToString() + " > " + DisplayDevice.AvailableDisplays[i].ToString() + " | Bounds:" + DisplayDevice.AvailableDisplays[i].Bounds.ToString() + " | Primary:" + DisplayDevice.AvailableDisplays[i].IsPrimary.ToString());
            Texts.Add("Desktop > Bounds:" + DesktopBounds.ToString());
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Display devices:", Texts.ToArray());
        }

        #region Get desktop data

        private static OpenMobile.Graphics.Rectangle getVirtualBounds()
        {
            //OpenMobile.Graphics.Rectangle bounds = new OpenMobile.Graphics.Rectangle();

            int Left=0, Right=0, Top=0, Bottom=0;
            for (int i = 0; i < DisplayDevice.AvailableDisplays.Count; i++)
            {
                //if (bounds.Left > DisplayDevice.AvailableDisplays[i].Bounds.Left) 
                if (Left > DisplayDevice.AvailableDisplays[i].Bounds.Left) 
                    Left = DisplayDevice.AvailableDisplays[i].Bounds.Left; 
                if (Right < DisplayDevice.AvailableDisplays[i].Bounds.Right) 
                    Right = DisplayDevice.AvailableDisplays[i].Bounds.Right; 
                if (Top > DisplayDevice.AvailableDisplays[i].Bounds.Top) 
                    Top = DisplayDevice.AvailableDisplays[i].Bounds.Top; 
                if (Bottom < DisplayDevice.AvailableDisplays[i].Bounds.Bottom)
                    Bottom = DisplayDevice.AvailableDisplays[i].Bounds.Bottom; 
            }
            return new OpenMobile.Graphics.Rectangle(Left, Top, Right - Left, Bottom - Top);
        }

        #endregion

        #region Keyboard Device Events

        public static void SourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            KeyboardDevice dev = (KeyboardDevice)sender;

            // Did this event come from the default OS unit?
            int SenderScreen = (dev.Instance + 1) * -1;
            if (dev.Instance < 0)
            {
                if (KeyboardMapping[SenderScreen] == DefaultUnit)
                    raiseSourceUp(SenderScreen, e);
            }
            else
            {
                for (int i = 0; i < KeyboardMapping.Length; i++)
                {
                    e.Screen = i;
                    if (GetKeyboardMapIndex(KeyboardMapping[i]) == dev.Instance)
                        raiseSourceUp(sender, e);
                }
            }
        }
        private static void raiseSourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyUp, e) == true)
                return;
            //If an app handles it first don't tell the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
                    Core.RenderingWindows[i].RenderingWindow_KeyUp(sender, e);
            }
            else if (e.Screen < Core.RenderingWindows.Count)
            {
                if (Core.RenderingWindows[e.Screen].WindowState != WindowState.Minimized)
                    Core.RenderingWindows[e.Screen].RenderingWindow_KeyUp(sender, e);
            }
        }
        public static void SourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            KeyboardDevice dev = (KeyboardDevice)sender;
#if LINUX
            if (Configuration.RunningOnLinux)
            {
                if ((e.Key == Key.VolumeUp) || (e.Key == Key.VolumeDown) || (e.Key == Key.Mute))
                    Core.theHost.Hal_Send("-1|Keyboard|VolumeChange");
            }
#endif

            // Did this event come from the default OS unit?
            int SenderScreen = (dev.Instance + 1) * -1;
            if (dev.Instance < 0)
            {
                if (KeyboardMapping[SenderScreen] == DefaultUnit)
                    raiseSourceDown(SenderScreen, e);
            }
            else
            {
                for (int i = 0; i < KeyboardMapping.Length; i++)
                {
                    e.Screen = i;
                    if (GetKeyboardMapIndex(KeyboardMapping[i]) == dev.Instance)
                        raiseSourceDown(sender, e);
                }
            }
        }
        private static void raiseSourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            // Block events if detection is active
            if (DetectionActive)
                return;

            // TODO_: Events only returns the last instance of the result so if a event higher in the call structure handles the keypress and others don't then only the last state is reported
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyDown, e) == true)
                return;
            //If an app handles it first don't tell the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
                    Core.RenderingWindows[i].RenderingWindow_KeyDown(sender, e);
            }
            else
            {
                if (Core.RenderingWindows[e.Screen].WindowState != WindowState.Minimized)
                    Core.RenderingWindows[e.Screen].RenderingWindow_KeyDown(sender, e);
            }
        }
        public static bool SendKeyUp(int screen, string Key)
        {
            // Block events if detection is active
            if (DetectionActive)
                return false;

            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyUp(null, new KeyboardKeyEventArgs(getKey(Key)));
            return true;
        }
        public static bool SendKeyDown(int screen, string Key)
        {
            // Block events if detection is active
            if (DetectionActive)
                return false;

            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyDown(null, new KeyboardKeyEventArgs(getKey(Key)));
            return true;
        }
        private static Key getKey(string key)
        {
            switch (key.ToLower())
            {
                case "up":
                    return Key.Up;
                case "down":
                    return Key.Down;
                case "left":
                    return Key.Left;
                case "right":
                    return Key.Right;
                case "enter":
                case "return":
                    return Key.Enter;
                case "scrollup":
                    return Key.PageUp;
                case "scrolldown":
                    return Key.PageDown;
                // TODO: Add enum handling to include more keys

            }
            return Key.Unknown;
        }

        #endregion

        #region Highlight control

        public static OMControl getHighlighted(int screen)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return null;
            return Core.RenderingWindows[screen].highlighted;
        }

        #endregion

        #region AutoDetection of devices

        static bool DetectionActive = false;

        #region Mouse

        static int DetectedMouseDevice = -3;
        static System.Threading.EventWaitHandle DetectedMouseClick = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
        static EventHandler<MouseButtonEventArgs> DetectMouseEventHandler = new EventHandler<MouseButtonEventArgs>(DetectMouseDevice_MouseClick);

        static public string DetectMouseDevice()
        {
            // Initialize data
            DetectedMouseDevice = -3;
            DetectedMouseClick.Reset();

            // Signal detection as active
            DetectionActive = true;

            // Connect mouse events
            for (int i = 0; i < driver.Mouse.Count; i++)
                driver.Mouse[i].MouseClick += DetectMouseEventHandler;

            // Wait for click (timeout = 10s)
            DetectedMouseClick.WaitOne(10000);

            // Disconnect mouse events
            for (int i = 0; i < driver.Mouse.Count; i++)
                driver.Mouse[i].MouseClick -= DetectMouseEventHandler;

            // Signal detection as stopped
            DetectionActive = false;

            // Did we find anything?
            if (DetectedMouseDevice < 0)
                return "";

            // Return device name
            return driver.Mouse[DetectedMouseDevice].ToString();
        }

        static void DetectMouseDevice_MouseClick(object sender, MouseButtonEventArgs e)
        {
            DetectedMouseDevice = (int)sender;
            DetectedMouseClick.Set();
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Autodetect Mouse: Detected mouse click at X:" + e.X.ToString() + " Y:"+e.Y.ToString());
        }

        #endregion

        #region Keyboard

        static int DetectedKeyboardDevice = -3;
        static System.Threading.EventWaitHandle DetectedKeyboardClick = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
        static EventHandler<KeyboardKeyEventArgs> DetectKeyboardEventHandler = new EventHandler<KeyboardKeyEventArgs>(DetectKeyboardDevice_KeyUp);

        static public string DetectKeyboardDevice()
        {
            // Initialize data
            DetectedKeyboardDevice = -3;
            DetectedKeyboardClick.Reset();

            // Signal detection as active
            DetectionActive = true;

            // Connect events
            for (int i = 0; i < driver.Keyboard.Count; i++)
                driver.Keyboard[i].KeyUp += DetectKeyboardEventHandler;

            // Wait for click (timeout = 10s)
            DetectedKeyboardClick.WaitOne(10000);

            // Disconnect events
            for (int i = 0; i < driver.Keyboard.Count; i++)
                driver.Keyboard[i].KeyUp -= DetectKeyboardEventHandler;

            // Signal detection as stopped
            DetectionActive = false;

            // Did we find anything?
            if (DetectedKeyboardDevice < 0)
                return "";

            // Return device name
            return driver.Keyboard[DetectedKeyboardDevice].ToString();
        }

        static void DetectKeyboardDevice_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            KeyboardDevice dev = (KeyboardDevice)sender;

            // Find index of keyboard device in driver array
            for (int i = 0; i < driver.Keyboard.Count; i++)
            {
                if (driver.Keyboard[i].ToString() == dev.ToString())
                {
                    DetectedKeyboardDevice = i;
                    break;
                }
            }
            DetectedKeyboardClick.Set();
        }

        #endregion

        #endregion
    }
}
