﻿/*********************************************************************************
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

namespace OpenMobile
{
    public static class InputRouter
    {
        static IInputDriver driver;
        public static string[] Keyboards
        {
            get
            {
                if (driver==null)
                    return new string[]{"Default Keyboard"};
                string[] ret=new string[driver.Keyboard.Count+1];
                ret[0] = "Default Keyboard";
                for (int i = 0; i < driver.Keyboard.Count; i++)
                    ret[i+1]=driver.Keyboard[i].Description;
                return ret;
            }
        }
        public static void Initialize()
        {
            if (Configuration.RunningOnWindows)
                driver = new Platform.Windows.WinRawInput();
            if (driver != null)
            {
                foreach (KeyboardDevice dev in driver.Keyboard)
                {
                    dev.KeyDown += new System.EventHandler<KeyboardKeyEventArgs>(SourceDown);
                    dev.KeyUp += new System.EventHandler<KeyboardKeyEventArgs>(SourceUp);
                }
                mapKeyboards();
            }
        }
        static int[] deviceMap;
        private static void mapKeyboards()
        {
            deviceMap = new int[Core.theHost.ScreenCount];
            using (PluginSettings s = new PluginSettings())
            {
                for (int i = 0; i < deviceMap.Length; i++)
                {
                    string val = s.getSetting("Screen" + (i+1).ToString() + ".Keyboard");
                    for(int j=0;j<driver.Keyboard.Count;j++)
                        if (driver.Keyboard[j].Description == val)
                        {
                            deviceMap[i] = j;
                            break;
                        }
                    
                }
            }
        }
        public static event userInteraction OnHighlightedChanged;
        public static void SourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            KeyboardDevice dev=(KeyboardDevice)sender;
            if (dev.Instance >= 0)
                for (int i = 0; i < deviceMap.Length; i++)
                {
                    e.Screen = i;
                    if (deviceMap[i] == dev.Instance)
                        raiseSourceUp(sender, e);
                }
            else
                raiseSourceUp(sender, e);
        }
        private static void raiseSourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyUp, e) == true)
                return; //If an app handles it first don't show the UI

            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
                    Core.RenderingWindows[i].RenderingWindow_KeyUp(sender, e);
            }
            else if (e.Screen < Core.RenderingWindows.Count)
                Core.RenderingWindows[e.Screen].RenderingWindow_KeyUp(sender, e);
        }
        internal static void raiseHighlightChanged(OMControl sender, int screen)
        {
            if (OnHighlightedChanged != null)
                OnHighlightedChanged(sender, screen);
        }
        public static void SourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            KeyboardDevice dev = (KeyboardDevice)sender;
            if (dev.Instance >= 0)
                for (int i = 0; i < deviceMap.Length; i++)
                {
                    e.Screen = i;
                    if (deviceMap[i] == dev.Instance)
                        raiseSourceDown(sender, e);
                }
            else
                raiseSourceDown(sender, e);
        }
        private static void raiseSourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyDown,e)==true)
                return; //If an app handles it first don't show the UI
            if (e.Screen == -1)
            {
                for (int i = 0; i < Core.RenderingWindows.Count; i++)
                    Core.RenderingWindows[i].RenderingWindow_KeyDown(sender, e);
            }else
                Core.RenderingWindows[e.Screen].RenderingWindow_KeyDown(sender, e);
        }
        public static bool SendKeyUp(int screen, string Key)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyUp(null, new KeyboardKeyEventArgs(getKey(Key)));
            return true;
        }
        public static bool SendKeyDown(int screen, string Key)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[screen].RenderingWindow_KeyDown(null, new KeyboardKeyEventArgs(getKey(Key)));
            return true;
        }
        public static OMControl getHighlighted(int screen)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return null;
            return Core.RenderingWindows[screen].highlighted;
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
            }
            return Key.Unknown;
        }
    }
}