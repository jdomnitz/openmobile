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

namespace OpenMobile.Framework
{
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
                    if (Windows.windowsEmbedder.SetParent(lastHandle[screen].handle, theHost.UIHandle(screen)) == IntPtr.Zero)
                        return false;
                    Windows.windowsEmbedder.SetFocus(lastHandle[screen].handle);
                    return true;
                }
                catch (Exception) { return false; }
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
            if ((lastHandle != null) && (Configuration.RunningOnWindows))
            {
                try
                {
                    Windows.windowsEmbedder.SetParent(lastHandle[screen].handle, IntPtr.Zero);
                    Windows.windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, 10, 10, 400, 300, 0x20);
                    lastHandle[screen].handle = IntPtr.Zero;
                    return true;
                }
                catch (Exception) { return false; }
            }
            return false;
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
                case PlatformID.WinCE:
                    return "WinCE";
                default:
                    if ((int)os.Platform == 128) //Not Supported by .Net
                        return "Unix";
                    if ((int)os.Platform == 5) //Not Supported by Mono
                        return "Xbox";
                    if ((int)os.Platform == 6) //Not Supported by Mono
                        return "Mac";
                    return "Unknown";
            }
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
            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (os.Version.Minor)
                {
                    case 0:
                        osVersion = "Windows 95";
                        break;
                    case 10:
                        if (os.Version.Revision.ToString() == "2222A")
                            osVersion = "Windows 98SE";
                        else
                            osVersion = "Windows 98";
                        break;
                    case 90:
                        osVersion = "Windows Me";
                        break;
                    default:
                        osVersion = "Unknown";
                        break;
                }
                return osVersion;
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (os.Version.Major)
                {
                    case 3:
                        osVersion = "Windows NT 3.51";
                        break;
                    case 4:
                        osVersion = "Windows NT 4.0";
                        break;
                    case 5:
                        if (os.Version.Minor == 0)
                            osVersion = "Windows 2000";
                        else
                            osVersion = "Windows XP";
                        break;
                    case 6:
                        if (os.Version.Minor == 0)
                            osVersion = "Windows Vista";
                        else
                            osVersion = "Windows 7";
                        break;
                    case 7:
                        if (os.Version.Minor == 0)
                            osVersion = "Windows 8";
                        break;
                    default:
                        break;
                }
                return osVersion;
            }
            else if (Configuration.RunningOnLinux)
            {
                ProcessStartInfo info = new ProcessStartInfo("lsb_release", "-d");
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(info);
                StreamReader reader = p.StandardOutput;
                osVersion = reader.ReadLine().Replace("Description:", "").Trim();
                return osVersion;
            }
            osVersion = getOS() + " " + os.Version.ToString();
            return osVersion;
        }

        private static string translateLocal(string path)
        {
            StreamReader r = new StreamReader(File.OpenRead("/proc/mounts"));
            string[] lines = r.ReadToEnd().Split(new char[] { '\r', '\n' });
            foreach (string line in lines)
                if (line.Contains(path))
                    return Path.GetFileName(line.Substring(0, line.IndexOf(' ')));
            return null;
        }
        public enum eDriveType
        {
            Unknown=0,
            NoRootDirectory=1,
            Removable=2,
            Fixed=3,
            Network=4,
            CDRom=5,
            Phone=7,
            iPod=8
        }
        /// <summary>
        /// Returns the type of drive at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static eDriveType getDriveType(string path)
        {
            if ((Configuration.RunningOnWindows) || (Configuration.RunningOnMacOS)) //OSX - Untested
            {
                DriveType type= new DriveInfo(path).DriveType;
                if (type == DriveType.Ram)
                    return eDriveType.Fixed;
                if ((Configuration.RunningOnWindows) && (type == DriveType.Removable))
                {
                    eDriveType tmp = Windows.detectPhone(path);
                    if (tmp!=eDriveType.Unknown)
                        return tmp;
                }
                return (eDriveType)type;
            }
            else
            {
                string local = translateLocal(path);
                if (string.IsNullOrEmpty(local))
                    return eDriveType.Unknown;
                if (local == "rootfs")
                    return eDriveType.Fixed;
                string response = null;
                ProcessStartInfo info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveIsMediaEjectable'");
                info.RedirectStandardOutput = true; info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                response = p.StandardOutput.ReadToEnd().Trim();
                if (p.ExitCode != 0)
                    response = null;
                if (response == "true")
                    return eDriveType.CDRom;
                info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveCanDetach'");
                info.RedirectStandardOutput = true; info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                p = new Process();
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                response = p.StandardOutput.ReadToEnd().Trim();
                if (p.ExitCode != 0)
                    response = null;
                if (response == "true")
				{
					info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveModel'");
	                info.RedirectStandardOutput = true; info.UseShellExecute = false;
	                info.WindowStyle = ProcessWindowStyle.Hidden;
	                p = new Process();
	                p.StartInfo = info;
	                p.Start();
	                p.WaitForExit();
	                response = p.StandardOutput.ReadToEnd().Trim();
	                if (p.ExitCode != 0)
	                    response = null;
					if (response.Contains("Phone"))
						return eDriveType.Phone;
                    if (response.Contains("iPod")||response.Contains("Apple Mobile Device"))
                        return eDriveType.iPod;
                    return eDriveType.Removable;
				}
                return eDriveType.Fixed;
            }
        }
        /// <summary>
        /// Returns the volume label for the given drive
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getVolumeLabel(string path)
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
                    if ((info.DriveType == DriveType.Fixed)||(info.DriveType==DriveType.Ram))
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
                if (path == "/")
                    return "File System (/)";
                string local = translateLocal(path);
                string response = null;
                if (local != null)
                {
                    ProcessStartInfo info = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'IdLabel'");
                    info.RedirectStandardOutput = true; info.UseShellExecute = false;
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    Process p = new Process();
                    p.StartInfo = info;
                    p.Start();
                    p.WaitForExit();
                    response = p.StandardOutput.ReadToEnd().Trim();
                    if (p.ExitCode != 0)
                        response = null;
                }
				if (path.StartsWith("cdda:"))
				{
					local=path.Substring(7).TrimEnd(new char[]{'/'});
					path=null;
				}
				ProcessStartInfo info2 = new ProcessStartInfo("qdbus", "--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/" + local + " org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'OpticalDiscNumAudioTracks'");
                info2.RedirectStandardOutput = true; info2.UseShellExecute = false;
                info2.WindowStyle = ProcessWindowStyle.Hidden;
                Process t = new Process();
                t.StartInfo = info2;
                t.Start();
                t.WaitForExit();
                string tracks = t.StandardOutput.ReadToEnd().Trim();
				if (path==null)
					return "CD-ROM|"+tracks;
                else if (string.IsNullOrEmpty(response))
                    return Path.GetFileName(new DriveInfo(path).VolumeLabel)+"|"+tracks;
                else
                    return response.Replace("_", " ");
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
                return "Mono v" + Environment.Version.ToString();
            else
                return Windows.getNetFramework();
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
            catch (Exception) { return false; }
        }
    }
}
