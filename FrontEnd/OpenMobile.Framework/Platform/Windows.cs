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
using System.Management;
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using IMAPI2.Interop;
using System.Collections.Generic;
namespace OpenMobile.Framework
{
    public sealed class Windows
    {
        internal static OSSpecific.eDriveType detectType(string path,DriveType type)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    var wmiDiskPartitions = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" +
                    wmi_HD["DeviceID"].ToString() + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition");
                    foreach (ManagementObject partition in wmiDiskPartitions.Get())
                    {
                        var wmiLogicalDisks = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" +
                        partition["DeviceID"].ToString() + "'} WHERE AssocClass = Win32_LogicalDiskToPartition");
                        foreach (ManagementObject disk in wmiLogicalDisks.Get())
                            if ((wmi_HD["DeviceID"] != null) && (disk["DeviceID"].ToString() == path.TrimEnd(new char[] { '\\' })))
                            {
                                if ((wmi_HD["Model"] != null) && ((wmi_HD["Model"].ToString().Contains("iPod")) || (wmi_HD["Model"].ToString().Contains("Apple Mobile Device"))))
                                    return OSSpecific.eDriveType.iPod;
                                else if ((wmi_HD["Model"] != null) && (wmi_HD["Model"].ToString().Contains("Phone")))
                                    return OSSpecific.eDriveType.Phone;
                                else if (wmi_HD["InterfaceType"].ToString() == "USB")
                                    return OSSpecific.eDriveType.Removable;
                                else
                                    return (OSSpecific.eDriveType)type;
                            }
                    }
                }
                return (OSSpecific.eDriveType)type;
            }
            catch (Exception)
            {
                return (OSSpecific.eDriveType)type;
            }
        }

        internal static string getCDType(string path,DriveInfo info)
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
            return "CD/DVD Drive (" + info.Name + ")";
        }

        internal static string getNetFramework()
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
        internal sealed class windowsEmbedder
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
