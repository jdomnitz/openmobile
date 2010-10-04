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

namespace OpenMobile.Framework
{
	public enum eDriveType
	{
		Unknown = 0,
		NoRootDirectory = 1,
		Removable = 2,
		Fixed = 3,
		Network = 4,
		CDRom = 5,
		Phone = 7,
		iPod = 8
	}
	public enum eArchType
	{
		Unknown,
		x86,
		x8664,
		ARM,
		MIPS,
		PowerPC,
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
		private static OperatingSystem os = System.Environment.OSVersion;
		/// <summary>
		/// Embed an application in the UI
		/// </summary>
		/// <param name="name">Window Name</param>
		/// <param name="host"></param>
		/// <param name="position"></param>
		/// <param name="screen"></param>
		/// <returns>Returns true if the application was found and embedded.</returns>
		public static bool embedApplication (string name, int screen, IPluginHost host, Rectangle position)
		{
			if (theHost == null) {
				theHost = host;
				theHost.OnSystemEvent += new SystemEvent (theHost_OnSystemEvent);
			}
			if (Configuration.RunningOnWindows) 
            {
				try {
					Process[] p = Process.GetProcessesByName (name);
					if (lastHandle == null)
						lastHandle = new embedInfo[theHost.ScreenCount];
					lastHandle[screen].handle = p[0].MainWindowHandle;
					lastHandle[screen].position = position;
					object o;
					theHost.getData (eGetData.GetScaleFactors, "", screen.ToString (), out o);
					if (o == null)
						return false;
					PointF scale = (PointF)o;
					if (Windows.windowsEmbedder.SetWindowPos (lastHandle[screen].handle, (IntPtr)0, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0), (int)(position.Width * scale.X), (int)(position.Height * scale.Y), 0x20) == false)
						return false;
					if (Windows.windowsEmbedder.SetParent (lastHandle[screen].handle, theHost.UIHandle (screen)) == IntPtr.Zero)
						return false;
					Windows.windowsEmbedder.SetFocus (lastHandle[screen].handle);
					return true;
				} catch (Exception) {
					return false;
				}
            }
            else if (Configuration.RunningOnX11)
            {
                if (lastHandle == null)
                    lastHandle = new embedInfo[theHost.ScreenCount];
                IntPtr ourWindow = theHost.UIHandle(screen);
                OpenMobile.Platform.X11.X11WindowInfo info =new OpenMobile.Platform.X11.X11WindowInfo();
                Marshal.PtrToStructure(ourWindow,info);
                IntPtr tmp1; IntPtr tmp2;
                int count;
                IntPtr[] windows;
                OpenMobile.Platform.X11.Functions.XQueryTree(info.Display,info.RootWindow,out tmp1,out tmp2,out windows,out count);
                name = name.ToLower();
                foreach (IntPtr window in windows)
                {
                    IntPtr windowName=new IntPtr();
                    OpenMobile.Platform.X11.Functions.XFetchName(info.Display, window, ref windowName);
                    if (Marshal.PtrToStringAuto(windowName).ToLower().StartsWith(name))
                    {
                        object o;
                        theHost.getData(eGetData.GetScaleFactors, "", screen.ToString(), out o);
                        if (o == null)
                            return false;
                        PointF scale = (PointF)o;
                        OpenMobile.Platform.X11.Functions.XResizeWindow(info.Display, window, (int)(position.Width * scale.X), (int)(position.Height * scale.Y));
                        OpenMobile.Platform.X11.Functions.XReparentWindow(info.Display, window, info.WindowHandle, (int)(position.X * scale.X + 1.0), (int)(position.Y * scale.Y + 1.0));
                        lastHandle[screen].handle = window;
                        lastHandle[screen].position = position;
                        return true;
                    }
                }
            }
			return false;
		}

		static void theHost_OnSystemEvent (eFunction function, string arg1, string arg2, string arg3)
		{
			if (function == eFunction.RenderingWindowResized) {
				if (Configuration.RunningOnWindows) {
					int screen = int.Parse (arg1);
					object o;
					theHost.getData (eGetData.GetScaleFactors, "", arg1, out o);
					if (o == null)
						return;
					PointF scale = (PointF)o;
					if (lastHandle[screen].handle != IntPtr.Zero)
						Windows.windowsEmbedder.SetWindowPos (lastHandle[screen].handle, (IntPtr)0, (int)(lastHandle[screen].position.X * scale.X + 1.0), (int)(lastHandle[screen].position.Y * scale.Y + 1.0), (int)(lastHandle[screen].position.Width * scale.X), (int)(lastHandle[screen].position.Height * scale.Y), 0x20);
				}
			}
		}
		/// <summary>
		/// Un-embed an application from the UI
		/// </summary>
		/// <param name="name"></param>
		/// <param name="screen"></param>
		/// <returns>Returns true if the application was unembedded</returns>
		public static bool unEmbedApplication (string name, int screen)
		{
			if (lastHandle != null)
            {
                if(Configuration.RunningOnWindows)
                {
				    try {
					    Windows.windowsEmbedder.SetParent (lastHandle[screen].handle, IntPtr.Zero);
					    Windows.windowsEmbedder.SetWindowPos (lastHandle[screen].handle, (IntPtr)0, 10, 10, 400, 300, 0x20);
					    lastHandle[screen].handle = IntPtr.Zero;
					    return true;
				    } catch (Exception) {
					    return false;
				    }
                }
                else if (Configuration.RunningOnX11)
                {
                    IntPtr ourWindow = theHost.UIHandle(screen);
                    OpenMobile.Platform.X11.X11WindowInfo info = new OpenMobile.Platform.X11.X11WindowInfo();
                    Marshal.PtrToStructure(ourWindow, info);
                    OpenMobile.Platform.X11.Functions.XReparentWindow(info.Display, lastHandle[screen].handle, info.RootWindow, 0, 10);
                }
            }
			return false;
		}
		/// <summary>
		/// Returns the name of the operating system
		/// </summary>
		/// <returns></returns>
		public static string getOS ()
		{
			switch (os.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
				return "Windows";
			case PlatformID.Unix:
				if (Environment.GetEnvironmentVariable ("not_supported_MONO_MWF_USE_QUARTZ_BACKEND") != null)
					return "Mac OSX";
				else
					return "Linux/Unix";
			case PlatformID.WinCE:
				return "WinCE";
			default:
				if ((int)os.Platform == 128)
					//Not Supported by .Net
					return "Unix";
				if ((int)os.Platform == 5)
					//Not Supported by Mono
					return "Xbox";
				if ((int)os.Platform == 6)
					//Not Supported by Mono
					return "Mac";
				return "Unknown";
			}
		}
		private static string osVersion;
		/// <summary>
		/// Returns the version of the operating system
		/// </summary>
		/// <returns></returns>
		public static string getOSVersion ()
		{
			if (osVersion != null)
				return osVersion;
			if (os.Platform == PlatformID.Win32Windows) {
				//This is a pre-NT version of Windows
				switch (os.Version.Minor) {
				case 0:
					osVersion = "Windows 95";
					break;
				case 10:
					if (os.Version.Revision.ToString () == "2222A")
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
			} else if (os.Platform == PlatformID.Win32NT) {
				switch (os.Version.Major) {
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
			} else if (Configuration.RunningOnLinux) {
				ProcessStartInfo info = new ProcessStartInfo ("lsb_release", "-d");
				info.RedirectStandardOutput = true;
				info.UseShellExecute = false;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Process p = Process.Start (info);
				StreamReader reader = p.StandardOutput;
				osVersion = reader.ReadLine ().Replace ("Description:", "").Trim ();
				return osVersion;
			}
			osVersion = getOS () + " " + os.Version.ToString ();
			return osVersion;
		}

		public static eArchType getArchitecture ()
		{
			if (Configuration.RunningOnUnix) 
			{
				ProcessStartInfo info = new ProcessStartInfo ("uname","-m");
				info.RedirectStandardOutput = true;
				info.UseShellExecute = false;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Process p = Process.Start (info);
				StreamReader reader = p.StandardOutput;
				string arch = reader.ReadLine ();
				switch (arch) {
				case "i386":
				case "i486":
				case "i586":
				case "i686":
				case "x86_64":
				case "ia64":
					if (IntPtr.Size == 4)
						return eArchType.x86;
					else
						return eArchType.x8664;
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
                        return eArchType.x8664;
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
        private enum wArchitecture:ushort
        {
            PROCESSOR_ARCHITECTURE_INTEL = 0,
            PROCESSOR_ARCHITECTURE_MIPS =1,
            PROCESSOR_ARCHITECTURE_ALPHA =2,
            PROCESSOR_ARCHITECTURE_PPC =3,
            PROCESSOR_ARCHITECTURE_SHX =4,
            PROCESSOR_ARCHITECTURE_ARM =5,
            PROCESSOR_ARCHITECTURE_IA64     = 6,
            PROCESSOR_ARCHITECTURE_ALPHA64 = 7,
            PROCESSOR_ARCHITECTURE_MSIL = 8, //wouldn't that be cool
            PROCESSOR_ARCHITECTURE_AMD64    = 9,
            PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 =10,
            PROCESSOR_ARCHITECTURE_UNKNOWN  = 0xffff 
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
        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

		/// <summary>
		/// Returns the type of drive at the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static internal eDriveType getDriveType (string path)
		{
			//OSX - Untested
			if ((Configuration.RunningOnWindows) || (Configuration.RunningOnMacOS)) {
				DriveType type = new DriveInfo (path).DriveType;
				if (type == DriveType.Ram)
					return eDriveType.Fixed;
				if (Configuration.RunningOnWindows)
					return Windows.detectType (path, type);
				return (eDriveType)type;
			} else {
				return Linux.detectType (path);
			}
		}

		/// <summary>
		/// Returns the volume label for the given drive
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static internal string getVolumeLabel (string path)
		{
			if (!Configuration.RunningOnLinux) {
				DriveInfo info = null;
				foreach (DriveInfo i in DriveInfo.GetDrives ())
					if (i.Name == path)
						info = i;
				if (info == null)
					return path;
				if (!info.IsReady) {
					if (info.DriveType == DriveType.CDRom) {
						if (Configuration.RunningOnWindows)
							return Windows.getCDType (path, info);
						return "CD/DVD Drive (" + info.Name + ")";
					} else if (info.DriveType == DriveType.Removable)
						return "Removable Disk (" + info.Name + ")";
					return info.Name;
				}
				if (string.IsNullOrEmpty (info.VolumeLabel)) {
					if ((info.DriveType == DriveType.Fixed) || (info.DriveType == DriveType.Ram))
						return "Local Disk (" + info.Name + ")"; else if (info.DriveType == DriveType.Removable)
						return "Removable Disk (" + info.Name + ")"; else if (info.DriveType == DriveType.Network)
						return "Network Drive (" + info.Name + ")";
					return info.Name;
				}
				return info.VolumeLabel + " (" + info.Name + ")";
			} else {
				return Linux.getVolumeLabel (path);
			}
		}
		/// <summary>
		/// Is the program running on the Mono Framework
		/// </summary>
		/// <returns></returns>
		public static bool IsMono ()
		{
			return (Type.GetType ("System.MonoType", false) != null);
		}
		/// <summary>
		/// Get the Framework Name and Version
		/// </summary>
		/// <returns></returns>
		public static string getFramework ()
		{
			if (IsMono ())
				return "Mono v" + Environment.Version.ToString ();
			else
				return Windows.getNetFramework ();
		}
		/// <summary>
		/// Run a manged process using mono if necessary
		/// </summary>
		/// <param name="path"></param>
		/// <param name="param"></param>
		/// <param name="wait"></param>
		/// <returns></returns>
		public static bool runManagedProcess (string path, string param, bool wait)
		{
			Process p;
			try {
				if (IsMono () == true)
					p = Process.Start ("mono", ('\"' + path + "\" " + param).Trim ());
				else
					p = Process.Start (path, param);
				if (wait)
					p.WaitForExit ();
				return true;
			} catch (Exception) {
				return false;
			}
		}
	}
}
