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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenMobile.Graphics
{
    public static class Application
    {
        public static string StartupPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        public static void Dispose()
        {
            _Run = false;
        }

        private static bool _Run = true;

        public static void Run()
        {
            #if WINDOWS
            int ret;

            // Start message pump
            if (OpenTK.Configuration.RunningOnWindows)
                System.Windows.Forms.Application.Run();
            #endif
        }
        public static void ShowError(object window, string text, string title)
        {
            if (OpenTK.Configuration.RunningOnWindows)
            {
                if (window == null)
                    MessageBox(IntPtr.Zero, text, title, 0x10);
                else
                    MessageBox((IntPtr)window, text, title, 0x10);
            }
            else if (OpenTK.Configuration.RunningOnLinux)
            {
                #if LINUX
                if (File.Exists("/usr/bin/zenity"))
                {
                    try
                    {
                        Process p = Process.Start("zenity", "--error --text='" + text + "' --title='" + title + "'");
                        p.WaitForExit();
                    }
                    catch (Exception) { }
                }
                else if (File.Exists("/usr/bin/kdialog"))
                {
                    try
                    {
                        Process p = Process.Start("kdialog", "--caption '" + title + "' --error '" + text + "'");
                        p.WaitForExit();
                    }
                    catch (Exception) { }
                }
                #endif
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    public class TouchEventArgs : EventArgs
    {
        string _name;
        double _arg1;
        string _arg2;
        Point _pos;
        bool _complete;
        public TouchEventArgs(string name, Point pos, double arg1, string arg2, bool complete)
        {
            _name = name;
            _arg1 = arg1;
            _arg2 = arg2;
            _pos = pos;
            _complete = complete;
        }
        public bool GestureComplete
        {
            get { return _complete; }
        }
        public Point Position
        {
            get { return _pos; }
        }
        public string Name
        {
            get { return _name; }
        }
        public double Arg1
        {
            get { return _arg1; }
        }
        public string Arg2
        {
            get { return _arg2; }
        }
    }
    public class ResolutionChange : EventArgs
    {
        int _width, _height;
        bool _landscape;
        public ResolutionChange(int width, int height, bool landscape)
        {
            _width = width;
            _height = height;
            _landscape = landscape;
        }
        public bool Landscape
        {
            get { return _landscape; }
        }
        public int Width
        {
            get { return _width; }
        }
        public int Height
        {
            get { return _height; }
        }
    }
    public class KeyPressEventArgs : EventArgs
    {
        private bool handled;
        private char keychar;
        public KeyPressEventArgs(char keyChar)
        {
            keychar = keyChar;
        }
        public bool Handled
        {
            get { return handled; }
            set { handled = value; }
        }
        public char KeyChar
        {
            get { return keychar; }
            set { keychar = value; }
        }
    }
}
