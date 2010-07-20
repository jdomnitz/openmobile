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
using System.Windows.Forms;
using OpenMobile.Input;

namespace OpenMobile
{
    public static class InputRouter
    {
        public static void SourceUp(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyUp, e) == true)
                return; //If an app handles it first don't show the UI
            for (int i = 0; i < Core.RenderingWindows.Count;i++ )
                Core.RenderingWindows[i].RenderingWindow_KeyUp(sender, e);
        }
        public static void SourceDown(object sender, OpenMobile.Input.KeyboardKeyEventArgs e)
        {
            if (Core.theHost.raiseKeyPressEvent(eKeypressType.KeyDown,e)==true)
                return; //If an app handles it first don't show the UI
            for (int i = 0; i < Core.RenderingWindows.Count; i++)
                Core.RenderingWindows[i].RenderingWindow_KeyDown(sender, e);
        }
        public static bool SendKeyUp(int instance, string Key)
        {
            if ((instance < 0) || (instance >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[instance].RenderingWindow_KeyUp(null, new KeyboardKeyEventArgs(getKey(Key)));
            return true;
        }
        public static bool SendKeyDown(int instance, string Key)
        {
            if ((instance < 0) || (instance >= Core.RenderingWindows.Count))
                return false;
            Core.RenderingWindows[instance].RenderingWindow_KeyDown(null, new KeyboardKeyEventArgs(getKey(Key)));
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
                    return Key.Enter;//TODO - Verify Key
                case "scrollup":
                    return Key.PageUp;
                case "scrolldown":
                    return Key.PageDown;
            }
            return Key.Unknown;
        }
    }
}
