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
#if WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using IMAPI2.Interop;
using Microsoft.Win32;
using System.Text;
namespace OpenMobile.Framework
{
    internal static class Windows
    {
        static Guid disk = new Guid("53F56307-B6BF-11D0-94F2-00A0C91EFB8B");
        internal static eDriveType detectType(string path, DriveType type)
        {
            if ((type == DriveType.Network) || (type == DriveType.CDRom))
                return (eDriveType)type;

            try
            {
                IntPtr ptrDevs = SetupDiGetClassDevs(ref disk, IntPtr.Zero, IntPtr.Zero, 0x12);
                SP_DEVICE_INTERFACE_DATA interf = new SP_DEVICE_INTERFACE_DATA();
                interf.cbSize = (uint)Marshal.SizeOf(interf);
                int deviceNumber = GetInstanceNumber("\\\\.\\" + path.Substring(0, 2));
                bool success = true;
                uint i = 0;
                while (success)
                {
                    success = SetupDiEnumDeviceInterfaces(ptrDevs, IntPtr.Zero, ref disk, i, ref interf);
                    if (success)
                    {
                        SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                        da.cbSize = (uint)Marshal.SizeOf(da);
                        SP_DEVICE_INTERFACE_DETAIL_DATA did = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                        if (IntPtr.Size == 8)
                            did.cbSize = 8;
                        else
                            did.cbSize = (uint)(4 + Marshal.SystemDefaultCharSize);
                        uint requiredSize = 0;
                        if (SetupDiGetDeviceInterfaceDetail(ptrDevs, ref interf, ref did, 256, out requiredSize, ref da))
                        {
                            if (GetInstanceNumber(did.DevicePath) == deviceNumber)
                            {
                                uint instance;
                                CM_Get_Parent(out instance, da.DevInst, 0);
                                IntPtr buffer = Marshal.AllocHGlobal(256);
                                CM_Get_Device_ID(instance, buffer, 256, 0);
                                string deviceID = Marshal.PtrToStringAuto(buffer);
                                Marshal.FreeHGlobal(buffer);
                                if (deviceID.StartsWith("USB"))
                                {
                                    uint tmp;
                                    IntPtr buff = Marshal.AllocHGlobal(256);
                                    SetupDiGetDeviceRegistryProperty(ptrDevs, ref da, 0xC, out tmp, buff, 256, out tmp);
                                    string model = Marshal.PtrToStringAnsi(buff);
                                    Marshal.FreeHGlobal(buff);
                                    if ((model != null) && (model.Contains("iPod")) || (model.Contains("Apple Mobile Device")))
                                        return eDriveType.iPod;
                                    else if ((model != null) && (model.Contains("Phone")))
                                        return eDriveType.Phone;
                                    else
                                        return eDriveType.Removable;
                                }
                                else
                                    return (eDriveType)type;
                            }
                        }
                    }
                    i++;
                }
            }
            catch (Exception) { }
            return (eDriveType)type;
        }
        private static int GetInstanceNumber(string drive)
        {
            int ret = -1;
            IntPtr file = CreateFile(drive, 0, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if ((long)file <= 0)
                return ret;
            try
            {
                STORAGE_DEVICE_NUMBER num = new STORAGE_DEVICE_NUMBER();
                int size = Marshal.SizeOf(num);
                IntPtr buff = Marshal.AllocHGlobal(size);
                uint length;
                if (DeviceIoControl(file, 0x2D1080, IntPtr.Zero, 0, buff, (uint)size, out length, IntPtr.Zero))
                {
                    num = (STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(buff, typeof(STORAGE_DEVICE_NUMBER));
                    ret = (num.DeviceType << 8) + num.DeviceNumber;
                }
                Marshal.FreeHGlobal(buff);
            }
            catch (Exception) { }
            CloseHandle(file);
            return ret;
        }
        #region structs
        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }
        #endregion
        #region PInvokes
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out UInt32 PropertyRegDataType,
            IntPtr PropertyBuffer, // the difference between this signature and the one above.
            uint PropertyBufferSize,
            out UInt32 RequiredSize
            );
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
        IntPtr lpInBuffer, uint nInBufferSize,
        IntPtr lpOutBuffer, uint nOutBufferSize,
        out uint lpBytesReturned, IntPtr lpOverlapped);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr CreateFile(
              string lpFileName,
              uint dwDesiredAccess,
              uint dwShareMode,
              IntPtr SecurityAttributes,
              uint dwCreationDisposition,
              uint dwFlagsAndAttributes,
              IntPtr hTemplateFile
              );
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern int CM_Get_Device_ID(UInt32 dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("setupapi.dll")]
        static extern int CM_Get_Parent(out UInt32 pdnDevInst, UInt32 dnDevInst, int ulFlags);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, UInt32 deviceInterfaceDetailDataSize, out UInt32 requiredSize, ref SP_DEVINFO_DATA deviceInfoData);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr enumerator, IntPtr hwndParent, UInt32 Flags);
        #endregion
        internal static string getCDType(string path, DriveInfo info)
        {
            try
            {
                var discMaster = new MsftDiscMaster2();

                if (!discMaster.IsSupportedEnvironment)
                    return "CD/DVD Drive (" + info.Name + ")";
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
                            return "BD-ROM (" + path + ")";
                        else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE))
                            return "DVD RW  (" + path + ")";
                        else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE))
                            return "DVD R  (" + path + ")";
                        else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVDROM))
                            return "DVD-ROM  (" + path + ")";
                        else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_REWRITABLE))
                            return "CD RW  (" + path + ")";
                        else if (profiles.Contains(IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_RECORDABLE))
                            return "CD R (" + path + ")";
                        else
                            return "CD-ROM  (" + path + ")";
                    }
                }
            }
            catch (COMException) { }
            return "CD/DVD Drive (" + info.Name + ")";
        }
        internal static string GetVolumeSerial()
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder VolLabel = new StringBuilder(256); // Label
            UInt32 VolFlags = new UInt32();
            StringBuilder FSName = new StringBuilder(256); // File System Name
            long Ret = GetVolumeInformation("C:\\", VolLabel, (UInt32)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);

            return Convert.ToString(serNum);
        }
        internal static string getNetFramework()
        {
            RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Net Framework Setup\NDP\");
            string[] lst = componentsKey.GetSubKeyNames();
            object SP = componentsKey.OpenSubKey(lst[lst.Length - 1]).GetValue("SP");
            string servicePack = String.Empty;
            if (SP != null)
                servicePack = SP.ToString();
            if ((servicePack.Length == 0) || (servicePack == "0"))
                servicePack = string.Empty;
            else
                servicePack = " SP" + servicePack;
            return "Microsoft .Net " + lst[lst.Length - 1] + servicePack;
        }
        internal static string getOSVersion()
        {
            OSVERSIONINFOEX ex = new OSVERSIONINFOEX();
            ex.dwOSVersionInfoSize = Marshal.SizeOf(ex);
            GetVersionEx(ref ex);

            string osVersion = "";
            if (ex.wProductType == 1)
            {
                switch (ex.dwMajorVersion)
                {
                    case 5:
                        if (ex.dwMinorVersion == 0)
                            osVersion = "Windows 2000";
                        else
                            osVersion = "Windows XP";
                        break;
                    case 6:
                        if (ex.dwMinorVersion == 0)
                            osVersion = "Windows Vista";
                        else
                            osVersion = "Windows 7";
                        break;
                    case 7:
                        if (ex.dwMinorVersion == 0)
                            osVersion = "Windows 8";
                        break;
                }
                if (Configuration.TabletPC)
                    osVersion += " Tablet";
                if ((ex.wSuiteMask & 0x40) == 0x40)
                    osVersion += " Embedded";
            }
            else
            {
                switch (ex.dwMajorVersion)
                {
                    case 5:
                        if ((ex.wSuiteMask & 0x8000) == 0x8000)
                            osVersion = "Windows Home Server";
                        else if (Platform.Windows.Functions.GetSystemMetrics(89) != 0)
                            osVersion = "Windows Server 2003 R2";
                        else
                            osVersion = "Windows Server 2003";
                        break;
                    case 6:
                        if (ex.dwMinorVersion == 0)
                            osVersion = "Windows Server 2008";
                        else
                            osVersion = "Windows Server 2008 R2";
                        break;
                    case 7:
                        if (ex.dwMinorVersion == 0)
                            osVersion = "Windows Server 2012";
                        break;
                }
            }
            if (ex.wServicePackMajor > 0)
                osVersion += " SP" + ex.wServicePackMajor.ToString();
            return osVersion;
        }
        [DllImport("kernel32"), System.Security.SuppressUnmanagedCodeSecurity]
        static extern bool GetVersionEx(ref OSVERSIONINFOEX osvi);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] szCSDVersion;
            public UInt16 wServicePackMajor;
            public UInt16 wServicePackMinor;
            public UInt16 wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }
        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);
        internal static class windowsEmbedder
        {
            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll")]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll")]
            public static extern IntPtr SetFocus(IntPtr hWnd);

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            public static int removeWindowBorder(IntPtr WindowsHandle)
            {
                return SetWindowLong(WindowsHandle, -16, GetWindowLong(WindowsHandle, -16) & ~(0xc00000) & ~(0x800000));
            }
        }
    }
}
#endif