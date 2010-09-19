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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using IMAPI2.Interop;
using System.Collections.Generic;

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
            if (os.Platform == PlatformID.Win32NT)
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
                    if (windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0), (int)(position.Width * scale.X), (int)(position.Height * scale.Y), 0x20) == false)
                        return false;
                    if (windowsEmbedder.SetParent(lastHandle[screen].handle, theHost.UIHandle(screen)) == IntPtr.Zero)
                        return false;
                    windowsEmbedder.SetFocus(lastHandle[screen].handle);
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
                if (os.Platform == PlatformID.Win32NT)
                {
                    int screen = int.Parse(arg1);
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", arg1, out o);
                    if (o == null)
                        return;
                    PointF scale = (PointF)o;
                    if (lastHandle[screen].handle != IntPtr.Zero)
                        windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, (int)(lastHandle[screen].position.X * scale.X + 1.0), (int)(lastHandle[screen].position.Y * scale.Y + 1.0), (int)(lastHandle[screen].position.Width * scale.X), (int)(lastHandle[screen].position.Height * scale.Y), 0x20);
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
            if ((lastHandle != null) && (os.Platform == PlatformID.Win32NT))
            {
                try
                {
                    windowsEmbedder.SetParent(lastHandle[screen].handle, IntPtr.Zero);
                    windowsEmbedder.SetWindowPos(lastHandle[screen].handle, (IntPtr)0, 10, 10, 400, 300, 0x20);
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
                    default:
                        break;
                }
                return osVersion;
            }
            else if (os.Platform == PlatformID.Unix)
            {
                ProcessStartInfo info = new ProcessStartInfo("lsb_release", "-d");
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                Process p = Process.Start(info);
                StreamReader reader = p.StandardOutput;
                osVersion = reader.ReadLine().Replace("Description:", "").Trim();
                return osVersion;
            }
            osVersion = getOS() + " " + os.Version.ToString();
            return osVersion;
        }

		static string translateLocal(string path)
		{
			StreamReader r=new StreamReader(File.OpenRead("/proc/mounts"));
			string[] lines=r.ReadToEnd().Split(new char[]{'\r','\n'});
			foreach(string line in lines)
				if (line.Contains(path))
					return Path.GetFileName(line.Substring(0,line.IndexOf(' ')));
			return null;
		}
		public static DriveType getDriveType(string path)
		{
			if ((Configuration.RunningOnWindows)||(Configuration.RunningOnMacOS)) //OSX - Untested
				return new DriveInfo(path).DriveType;
			else
			{
				string local=translateLocal(path);
				if (string.IsNullOrEmpty(local))
					return DriveType.Unknown;
				if(local=="rootfs")
					return DriveType.Fixed;
				string response=null;
				ProcessStartInfo info=new ProcessStartInfo("qdbus","--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/"+local+" org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveIsMediaEjectable'");
				info.RedirectStandardOutput=true;info.UseShellExecute=false;
				info.WindowStyle=ProcessWindowStyle.Hidden;
				Process p=new Process();
				p.StartInfo=info;
				p.Start();
				p.WaitForExit();
				response=p.StandardOutput.ReadToEnd().Trim();
				if(p.ExitCode!=0)
					response=null;
				if (response=="true")
					return DriveType.CDRom;
				info=new ProcessStartInfo("qdbus","--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/"+local+" org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'DriveCanDetach'");
				info.RedirectStandardOutput=true;info.UseShellExecute=false;
				info.WindowStyle=ProcessWindowStyle.Hidden;
				p=new Process();
				p.StartInfo=info;
				p.Start();
				p.WaitForExit();
				response=p.StandardOutput.ReadToEnd().Trim();
				if(p.ExitCode!=0)
					response=null;
				if (response=="true")
					return DriveType.Removable;
				return DriveType.Fixed;
			}
		}
        public static string getVolumeLabel(string path)
        {
            if (Configuration.RunningOnWindows)
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
                        var discMaster = new MsftDiscMaster2();

                        if (!discMaster.IsSupportedEnvironment)
                            return path;
                        foreach (string uniqueRecorderId in discMaster)
                        {
                            var discRecorder2 = new MsftDiscRecorder2();
                            discRecorder2.InitializeDiscRecorder(uniqueRecorderId);
                            if (discRecorder2.VolumePathNames[0].ToString() == path)
                            {
                                List<IMAPI_PROFILE_TYPE> profiles = new List<IMAPI_PROFILE_TYPE>();
                                foreach (IMAPI_PROFILE_TYPE t in discRecorder2.SupportedProfiles)
                                    profiles.Add(t);
                                if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_R_SEQUENTIAL))
                                    return "BD-R (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_REWRITABLE))
                                    return "BD-RW (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_ROM))
                                    return "BD (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE))
                                    return "DVD RW  (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE))
                                    return "DVD R  (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVDROM))
                                    return "DVD  (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_REWRITABLE))
                                    return "CD RW  (" + path + ")";
                                else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_RECORDABLE))
                                    return "CD R (" + path + ")";
                                else
                                    return "CD  (" + path + ")";
                            }
                        }
                        return "CD/DVD Drive (" + info.Name + ")";
                    }
                    else if (info.DriveType == DriveType.Removable)
                        return "Removable Disk (" + info.Name + ")";
                    return info.Name;
                }
                if (string.IsNullOrEmpty(info.VolumeLabel))
                {
                    if (info.DriveType == DriveType.Fixed)
                        return "Local Disk (" + info.Name + ")";
                    else if (info.DriveType == DriveType.Removable)
                        return "Removable Disk (" + info.Name + ")";
                    else if (info.DriveType == DriveType.Network)
                        return "Network Drive (" + info.Name + ")";
                    return info.Name;
                }
                return info.VolumeLabel + " (" + info.Name + ")";
            }
			else if(Configuration.RunningOnUnix)
			{
				if (path=="/")
					return "File System (/)";
				string local=translateLocal(path);
				string response=null;
				if (local!=null)
				{
				ProcessStartInfo info=new ProcessStartInfo("qdbus","--system org.freedesktop.UDisks /org/freedesktop/UDisks/devices/"+local+" org.freedesktop.DBus.Properties.Get 'org.freedesktop.UDisks.Device' 'IdLabel'");
				info.RedirectStandardOutput=true;info.UseShellExecute=false;
				info.WindowStyle=ProcessWindowStyle.Hidden;
				Process p=new Process();
				p.StartInfo=info;
				p.Start();
				p.WaitForExit();
				response=p.StandardOutput.ReadToEnd().Trim();
				if(p.ExitCode!=0)
					response=null;
				}
				if (string.IsNullOrEmpty(response))
					return Path.GetFileName(new DriveInfo(path).VolumeLabel);
				else
					return response.Replace("_"," ");
			}
            return path;
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
                return getNetFramework();
        }

        private static string getNetFramework()
        {
            RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Net Framework Setup\NDP\");
            string[] lst = componentsKey.GetSubKeyNames();
            object SP = componentsKey.OpenSubKey(lst[lst.Length - 1]).GetValue("SP");
            string servicePack = "";
            if (SP != null)
                servicePack = SP.ToString();
            if ((servicePack == "") || (servicePack == "0"))
                servicePack = "";
            else
                servicePack = " SP" + servicePack;
            return "Microsoft .Net " + lst[lst.Length - 1] + servicePack;
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
        private sealed class windowsEmbedder
        {
            [DllImport("user32.dll")]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            public static extern IntPtr SetFocus(IntPtr hWnd);
        }
    }
}
