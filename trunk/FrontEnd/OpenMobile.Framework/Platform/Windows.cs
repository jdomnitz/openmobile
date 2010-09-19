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
namespace OpenMobile.Framework
{
    public sealed class Windows
    {
        internal static bool detectPhone(string path)
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
                        if (disk["DeviceID"].ToString() == path.TrimEnd(new char[] { '\\' }))
                            if (wmi_HD["Model"].ToString().Contains("Phone"))
                                return true;
                            else
                                return false;
                }
            }
            return false;
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
