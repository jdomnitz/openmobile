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
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenMobile.Plugin;
using System.Drawing;
using System.Text;
using System.Net;

namespace OpenMobile.Framework
{
    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class OSSpecific
    {
        static IPluginHost theHost;
        static IntPtr[] lastHandle;
        private static OperatingSystem os = System.Environment.OSVersion;
        /// <summary>
        /// Embed an application in the UI
        /// </summary>
        /// <param name="name">Window Name</param>
        /// <param name="host"></param>
        /// <param name="position"></param>
        /// <param name="screen"></param>
        /// <returns>Returns true if the application was found and embedded.</returns>
        public static bool embedApplication(string name, int screen, IPluginHost host, Rectangle position)
        {
            theHost = host;
            {
                try
                {
                    Process[] p = Process.GetProcessesByName(name);
                    if (lastHandle == null)
                        lastHandle = new IntPtr[theHost.ScreenCount];
                    lastHandle[screen] = p[0].MainWindowHandle;
                    windowsEmbedder.SetParent(p[0].MainWindowHandle, theHost.UIHandle(screen));
                    windowsEmbedder.SetWindowPos(p[0].MainWindowHandle, (IntPtr)0, position.X, position.Y, position.Width, position.Height, 0x20);
                    return true;
                }
                catch (Exception) { return false; }
            }
        }
        /// <summary>
        /// Un-embed an application from the UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screen"></param>
        /// <returns>Returns true if the application was unembedded</returns>
        public static bool unEmbedApplication(string name, int screen)
        {
            if (lastHandle == null)
            return false;
            {
                try
                {
                      windowsEmbedder.SetParent(lastHandle[screen], IntPtr.Zero);
                      windowsEmbedder.SetWindowPos(lastHandle[screen], (IntPtr)0, 10, 10, 400, 300, 0x20);
                      return true;
                }
                catch (Exception) { return false; }
            }
        }
        /// <summary>
        /// Returns the name of the operating system
        /// </summary>
        /// <returns></returns>
        public static string getOS()
        {
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return "Windows";
                case PlatformID.Unix:
                    if (Environment.GetEnvironmentVariable("not_supported_MONO_MWF_USE_QUARTZ_BACKEND") != null)
                        return "Mac OSX";
                    else
                        return "Linux/Unix";
                //case PlatformID.MacOSX:
                //    return "MacOS";
                //case PlatformID.Xbox:
                //    return "Xbox";
                // ^ Not Supported by Mono ^
                case PlatformID.WinCE:
                    return "WinCE";
                default:
                    if ((int)os.Platform == 128)
                        return "Unix";
                    if ((int)os.Platform == 6)
                        return "Mac";
                    return "Unknown";
            }
        }
        /// <summary>
        /// Returns the version of the operating system
        /// </summary>
        /// <returns></returns>
        public static string getOSVersion()
        {
            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (os.Version.Minor)
                {
                    case 0:
                        return "Windows 95";
                    case 10:
                        if (os.Version.Revision.ToString() == "2222A")
                            return "Windows 98SE";
                        else
                            return "Windows 98";
                    case 90:
                        return "Windows Me";
                    default:
                        return "Unknown";
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (os.Version.Major)
                {
                    case 3:
                        return "Windows NT 3.51";
                    case 4:
                        return "Windows NT 4.0";
                    case 5:
                        if (os.Version.Minor == 0)
                            return "Windows 2000";
                        else
                            return "Windows XP";
                    case 6:
                        if (os.Version.Minor == 0)
                            return "Windows Vista";
                        else
                            return "Windows 7";
                    default:
                        break;
                }
            }
            return getOS() + " " + os.Version.ToString();
        }
        /// <summary>
        /// Is the program running on the Mono Framework
        /// </summary>
        /// <returns></returns>
        public static bool IsMono()
        {
            return (Type.GetType("System.MonoType", false) != null);
        }
        /// <summary>
        /// Get the Framework Name and Version
        /// </summary>
        /// <returns></returns>
        public static string getFramework()
        {
            return (IsMono() ? "Mono v" : "Microsoft .Net v") + Environment.Version.ToString();
        }
        /// <summary>
        /// Run a manged process using mono if necessary
        /// </summary>
        /// <param name="path"></param>
        /// <param name="param"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public static bool runManagedProcess(string path, string param, bool wait)
        {
            Process p;
            try
            {
                if (IsMono() == true)
                    p = Process.Start("mono", (path + " " + param).Trim());
                else
                    p = Process.Start(path, param);
                if (wait == true)
                    p.WaitForExit();
                return true;
            }
            catch (Exception) { return false; }
        }
        private sealed class windowsEmbedder
        {
            [DllImport("user32.dll")]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        }
    }
}
