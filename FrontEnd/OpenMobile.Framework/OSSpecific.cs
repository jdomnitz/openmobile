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
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using System.Runtime.InteropServices;
using System.Reflection;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Type of drive
    /// </summary>
    public enum eDriveType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Reserved
        /// </summary>
        NoRootDirectory = 1,
        /// <summary>
        /// Removable storage
        /// </summary>
        Removable = 2,
        /// <summary>
        /// Internal
        /// </summary>
        Fixed = 3,
        /// <summary>
        /// Network drive
        /// </summary>
        Network = 4,
        /// <summary>
        /// CD/DVD/Blu-Ray
        /// </summary>
        CDRom = 5,
        /// <summary>
        /// Smart Phone
        /// </summary>
        Phone = 7,
        /// <summary>
        /// Apple Device (iPod)
        /// </summary>
        iPod = 8
    }
    /// <summary>
    /// CPU Architecture
    /// </summary>
    public enum eArchType
    {
        /// <summary>
        /// Unknown Architecture
        /// </summary>
        Unknown,
        /// <summary>
        /// x86/i386/i386/i586/i686
        /// </summary>
        x86,
        /// <summary>
        /// x86_64 and ia64
        /// </summary>
        x64,
        /// <summary>
        /// ARM
        /// </summary>
        ARM,
        /// <summary>
        /// MIPS
        /// </summary>
        MIPS,
        /// <summary>
        /// Power PC
        /// </summary>
        PowerPC,
        /// <summary>
        /// Power PC 64bit
        /// </summary>
        PowerPC64,
        /// <summary>
        /// Sparc
        /// </summary>
        Sparc
    }

    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class OSSpecific
    {
        struct embedInfo
        {
            public Rectangle position;
            public IntPtr handle;
        }
        static IPluginHost theHost;
        static embedInfo[] lastHandle;
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
            if (theHost == null)
            {
                theHost = host;
                theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            }
            if (Configuration.RunningOnWindows)
            {
                try
                {
                    Process[] p = Process.GetProcessesByName(name);
                    if (lastHandle == null)
                        lastHandle = new embedInfo[theHost.ScreenCount];
                    lastHandle[screen].handle = p[0].MainWindowHandle;
                    lastHandle[screen].position = position;
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", screen.ToString(), out o);
                    if (o == null)
                        return false;
                    PointF scale = (PointF)o;
                    if (Windows.windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0), (int)(position.Width * scale.X), (int)(position.Height * scale.Y), 0x20) == false)
                        return false;
                    if (Windows.windowsEmbedder.SetParent(lastHandle[screen].handle, (IntPtr)theHost.UIHandle(screen)) == IntPtr.Zero)
                        return false;
                    Windows.windowsEmbedder.SetFocus(lastHandle[screen].handle);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (Configuration.RunningOnX11)
            {
                if (lastHandle == null)
                    lastHandle = new embedInfo[theHost.ScreenCount];
                OpenMobile.Platform.X11.X11WindowInfo info = (OpenMobile.Platform.X11.X11WindowInfo)theHost.UIHandle(screen);
                IntPtr tmp1; IntPtr tmp2;
                int count;
                IntPtr w;
                OpenMobile.Platform.X11.Functions.XQueryTree(info.Display, info.RootWindow, out tmp1, out tmp2, out w, out count);
                name = name.ToLower();
                IntPtr[] windows = new IntPtr[count];
                Marshal.Copy(w, windows, 0, count);
                foreach (IntPtr window in windows)
                {
                    IntPtr windowName = new IntPtr();
                    OpenMobile.Platform.X11.Functions.XFetchName(info.Display, window, ref windowName);
                    if (windowName == IntPtr.Zero)
                        continue;
                    string localName = Marshal.PtrToStringAuto(windowName);
                    if (localName.ToLower().Contains(name))
                    {
                        object o;
                        theHost.getData(eGetData.GetScaleFactors, "", screen.ToString(), out o);
                        if (o == null)
                            return false;
                        PointF scale = (PointF)o;
                        OpenMobile.Platform.X11.Functions.XResizeWindow(info.Display, window, (int)(position.Width * scale.X), (int)(position.Height * scale.Y));
                        OpenMobile.Platform.X11.Functions.XReparentWindow(info.Display, window, info.WindowHandle, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0));
                        OpenMobile.Platform.X11.Functions.XMoveWindow(info.Display, window, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0));
                        lastHandle[screen].handle = window;
                        lastHandle[screen].position = position;
                        return true;
                    }
                }
            }
            return false;
        }

        static void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.RenderingWindowResized)
            {
                if (Configuration.RunningOnWindows)
                {
                    int screen = int.Parse(arg1);
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", arg1, out o);
                    if (o == null)
                        return;
                    PointF scale = (PointF)o;
                    if (lastHandle[screen].handle != IntPtr.Zero)
                        Windows.windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, (int)(lastHandle[screen].position.X * scale.X + 1.0), (int)(lastHandle[screen].position.Y * scale.Y + 1.0), (int)(lastHandle[screen].position.Width * scale.X), (int)(lastHandle[screen].position.Height * scale.Y), 0x20);
                }
                else if (Configuration.RunningOnX11)
                {
                    int screen = int.Parse(arg1);
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", arg1, out o);
                    if (o == null)
                        return;
                    OpenMobile.Platform.X11.X11WindowInfo info = (OpenMobile.Platform.X11.X11WindowInfo)theHost.UIHandle(screen);
                    PointF scale = (PointF)o;
                    OpenMobile.Platform.X11.Functions.XResizeWindow(info.Display, lastHandle[screen].handle, (int)(lastHandle[screen].position.Width * scale.X), (int)(lastHandle[screen].position.Height * scale.Y));
                }
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
            if (lastHandle != null)
            {
                if (lastHandle[screen].handle == IntPtr.Zero)
                    return false;
                if (Configuration.RunningOnWindows)
                {
                    try
                    {
                        Windows.windowsEmbedder.SetParent(lastHandle[screen].handle, IntPtr.Zero);
                        Windows.windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, 10, 10, 400, 300, 0x20);
                        lastHandle[screen].handle = IntPtr.Zero;
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else if (Configuration.RunningOnX11)
                {
                    OpenMobile.Platform.X11.X11WindowInfo info = (OpenMobile.Platform.X11.X11WindowInfo)theHost.UIHandle(screen);
                    OpenMobile.Platform.X11.Functions.XReparentWindow(info.Display, lastHandle[screen].handle, info.RootWindow, 0, 10);
                    lastHandle[screen].handle = IntPtr.Zero;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the name of the operating system
        /// </summary>
        /// <returns></returns>
        public static string getOS()
        {
            if (Configuration.RunningOnWindows)
                return "Windows";
            else if (Configuration.RunningOnMacOS)
                return "Mac OSX";
            else if (Configuration.RunningOnLinux)
                return "Linux";
            else if (Configuration.RunningOnUnix)
                return "Unix";
            else
                return "Unknown";
        }
        private static string osVersion;
        /// <summary>
        /// Returns the version of the operating system
        /// </summary>
        /// <returns></returns>
        public static string getOSVersion()
        {
            if (osVersion != null)
                return osVersion;
            if (Configuration.RunningOnWindows)
            {
                osVersion = Windows.getOSVersion();
                return osVersion;
            }
            else if (Configuration.RunningOnLinux)
            {
                ProcessStartInfo info = new ProcessStartInfo("lsb_release", "-d");
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    Process p = Process.Start(info);
                    StreamReader reader = p.StandardOutput;
                    osVersion = reader.ReadLine().Replace("Description:", "").Trim();
                }
                catch (Exception)
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo("/etc/");
                        FileInfo[] files = di.GetFiles("*release");
                        if (files.Length == 0)
                            files = di.GetFiles("*version");
                        if (files.Length > 0)
                            osVersion = File.ReadAllText(files[0].FullName).TrimEnd(new char[] { ' ', '\r', '\n' });
                        else
                            osVersion = "Unknown Linux";
                    }
                    catch (Exception)
                    {
                        osVersion = "Unknown Linux";
                    }
                }
                return osVersion;
            }
            osVersion = getOS() + " " + Environment.OSVersion.Version.ToString();
            return osVersion;
        }
        /// <summary>
        /// Returns the current system Architecture
        /// </summary>
        /// <returns></returns>
        public static eArchType getArchitecture()
        {
            if (Configuration.RunningOnUnix)
            {
                ProcessStartInfo info = new ProcessStartInfo("uname", "-m");
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(info);
                StreamReader reader = p.StandardOutput;
                string arch = reader.ReadLine().ToLower();
                switch (arch)
                {
                    case "i386":
                    case "i486":
                    case "i586":
                    case "i686":
                    case "x86":
                    case "x86_64":
                    case "x86-64":
                    case "ia64":
                    case "ia64n":
                        if (IntPtr.Size == 4)
                            return eArchType.x86;
                        else
                            return eArchType.x64;
                    case "arm":
                    case "arm26":
                        return eArchType.ARM;
                    case "mips":
                        return eArchType.MIPS;
                    case "sparc":
                        return eArchType.Sparc;
                    case "powerpc":
                    case "ppc":
                        return eArchType.PowerPC;
                    case "ppc64":
                        return eArchType.PowerPC64;
                    default:
                        return eArchType.Unknown;
                }
            }
            else if (Configuration.RunningOnWindows)
            {
                SYSTEM_INFO info;
                GetSystemInfo(out info);
                switch (info.processorArchitecture)
                {
                    case wArchitecture.PROCESSOR_ARCHITECTURE_IA64:
                    case wArchitecture.PROCESSOR_ARCHITECTURE_AMD64:
                        return eArchType.x64;
                    case wArchitecture.PROCESSOR_ARCHITECTURE_IA32_ON_WIN64:
                    case wArchitecture.PROCESSOR_ARCHITECTURE_INTEL:
                        return eArchType.x86;
                    case wArchitecture.PROCESSOR_ARCHITECTURE_ARM:
                        return eArchType.ARM;
                    case wArchitecture.PROCESSOR_ARCHITECTURE_MIPS:
                        return eArchType.MIPS;
                    case wArchitecture.PROCESSOR_ARCHITECTURE_PPC:
                        return eArchType.PowerPC;
                    case wArchitecture.PROCESSOR_ARCHITECTURE_MSIL:
                    case wArchitecture.PROCESSOR_ARCHITECTURE_UNKNOWN:
                    default:
                        return eArchType.Unknown;
                }
            }
            return eArchType.Unknown;
        }
        private enum wArchitecture : ushort
        {
            PROCESSOR_ARCHITECTURE_INTEL = 0,
            PROCESSOR_ARCHITECTURE_MIPS = 1,
            PROCESSOR_ARCHITECTURE_ALPHA = 2,
            PROCESSOR_ARCHITECTURE_PPC = 3,
            PROCESSOR_ARCHITECTURE_SHX = 4,
            PROCESSOR_ARCHITECTURE_ARM = 5,
            PROCESSOR_ARCHITECTURE_IA64 = 6,
            PROCESSOR_ARCHITECTURE_ALPHA64 = 7,
            PROCESSOR_ARCHITECTURE_MSIL = 8, //wouldn't that be cool
            PROCESSOR_ARCHITECTURE_AMD64 = 9,
            PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10,
            PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public wArchitecture processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        /// <summary>
        /// Returns the type of drive at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static internal eDriveType getDriveType(string path)
        {
            //OSX - Untested
            if ((Configuration.RunningOnWindows) || (Configuration.RunningOnMacOS))
            {
                DriveType type = new DriveInfo(path).DriveType;
                if (type == DriveType.Ram)
                    return eDriveType.Fixed;
                if (Configuration.RunningOnWindows)
                    return Windows.detectType(path, type);
                return (eDriveType)type;
            }
            else
            {
                return Linux.detectType(path);
            }
        }

        /// <summary>
        /// Returns the volume label for the given drive
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static internal string getVolumeLabel(string path)
        {
            if (!Configuration.RunningOnLinux)
            {
                DriveInfo info = null;
                foreach (DriveInfo i in DriveInfo.GetDrives())
                    if (i.Name == path)
                        info = i;
                if (info == null)
                    return path;
                if (!info.IsReady)
                {
                    if (info.DriveType == DriveType.CDRom)
                    {
                        if (Configuration.RunningOnWindows)
                            return Windows.getCDType(path, info);
                        return "CD/DVD Drive (" + info.Name + ")";
                    }
                    else if (info.DriveType == DriveType.Removable)
                        return "Removable Disk (" + info.Name + ")";
                    return info.Name;
                }
                if (string.IsNullOrEmpty(info.VolumeLabel))
                {
                    if ((info.DriveType == DriveType.Fixed) || (info.DriveType == DriveType.Ram))
                        return "Local Disk (" + info.Name + ")";
                    else if (info.DriveType == DriveType.Removable)
                        return "Removable Disk (" + info.Name + ")";
                    else if (info.DriveType == DriveType.Network)
                        return "Network Drive (" + info.Name + ")";
                    return info.Name;
                }
                return info.VolumeLabel + " (" + info.Name + ")";
            }
            else
            {
                return Linux.getVolumeLabel(path);
            }
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
            if (IsMono())
                return getMonoVersion();
            else
                return Windows.getNetFramework();
        }
        private static string getMonoVersion()
        {
            Type t = Type.GetType("Mono.Runtime");
            if (t == null)
                return "Mono v" + Environment.Version.ToString();
            MethodInfo mi = t.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
            return "Mono v" + (string)mi.Invoke(null, null);
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
                    p = Process.Start("mono", ('\"' + path + "\" " + param).Trim());
                else
                    p = Process.Start(path, param);
                if (wait)
                    p.WaitForExit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
