using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using OpenMobile;

namespace OMHal
{
    static class DriveHandler
    {
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
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
        }

        private static char DriveMaskToLetter(int mask)
        {
            char letter;
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int cnt = 0;
            int pom = mask / 2;
            while (pom != 0)
            {
                // while there is any bit set in the mask
                // shift it to the right...                
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
    }
}
