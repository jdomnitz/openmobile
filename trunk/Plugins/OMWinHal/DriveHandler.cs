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
using OpenMobile;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace OMHal
{
    static class DriveHandler
    {
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume
        private static string Drive;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public int dbcv_unitmask;
        }

        public static void WndProc(ref Message m)
        {
            if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL)
                if (Marshal.ReadInt32(m.LParam, 4) == DBT_DEVTYP_VOLUME)
                {
                    DEV_BROADCAST_VOLUME vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME)); ;
                    Drive = DriveMaskToLetter(vol.dbcv_unitmask).ToString();
                    Drive += ":\\";
                    DeviceArrived(Drive);
                }
            if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                if (Marshal.ReadInt32(m.LParam, 4) == DBT_DEVTYP_VOLUME)
                {
                    DEV_BROADCAST_VOLUME vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME)); ;
                    Drive = DriveMaskToLetter(vol.dbcv_unitmask).ToString();
                    Drive += ":\\";
                    DeviceRemoved(Drive);
                }
        }

        private static char DriveMaskToLetter(int mask)
        {
            char letter;
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int cnt = 0;
            int pom = mask / 2;
            while (pom != 0)
            {              
                pom = pom / 2;
                cnt++;
            }

            if (cnt < drives.Length)
                letter = drives[cnt];
            else
                letter = '?';

            return letter;
        }
        private static void DeviceArrived(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            if (info.IsReady == true)
            {
                Form1.raiseStorageEvent(eMediaType.NotSet, drive);
            }
        }
        private static void DeviceRemoved(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            Form1.raiseStorageEvent(eMediaType.DeviceRemoved, drive);
        }
    }
}
