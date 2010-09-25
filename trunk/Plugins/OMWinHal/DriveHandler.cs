using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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
            unsafe
            {
                SHNOTIFYSTRUCT nfy = new SHNOTIFYSTRUCT();
                nfy = (SHNOTIFYSTRUCT)Marshal.PtrToStructure(m.WParam, typeof(SHNOTIFYSTRUCT));
                if ((int)m.LParam == (int)SHCNE.SHCNE_MEDIAINSERTED)
                {
                    StringBuilder path=new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DeviceArrived(path.ToString());
                }
                else if ((int)m.LParam == (int)SHCNE.SHCNE_MEDIAREMOVED)
                {
                    StringBuilder path = new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DeviceRemoved(path.ToString());
                }else if((int)m.LParam == (int)SHCNE.SHCNE_DRIVEADD)
                {
                    StringBuilder path=new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DeviceArrived(path.ToString());
                }
                else if ((int)m.LParam == (int)SHCNE.SHCNE_DRIVEREMOVED)
                {
                    StringBuilder path = new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DeviceRemoved(path.ToString());
                }
            }
        }
        
        public const int WM_MEDIA_CHANGE = 0x0801;
        public static uint id;
        public static void hook(IntPtr form)
        {
            SHChangeNotifyEntry entry =new SHChangeNotifyEntry();
            entry.Recursively=true;
            id=SHChangeNotifyRegister(form, 3, SHCNE.SHCNE_DRIVEADD| SHCNE.SHCNE_DRIVEREMOVED | SHCNE.SHCNE_MEDIAINSERTED | SHCNE.SHCNE_MEDIAREMOVED, WM_MEDIA_CHANGE, 1, ref entry);
        }
        public static void unhook()
        {
            SHChangeNotifyUnregister(id);
        }

        [DllImport("shell32.dll", SetLastError = true, EntryPoint = "#4", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern Boolean SHChangeNotifyUnregister(UInt32 hNotify);

        [DllImport("shell32.dll", SetLastError = true, EntryPoint = "#2", CharSet = CharSet.Auto)]
        static extern UInt32 SHChangeNotifyRegister(IntPtr hWnd,int fSources,SHCNE fEvents,uint wMsg,int cEntries,ref SHChangeNotifyEntry pFsne);

        [DllImport("shell32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHNOTIFYSTRUCT
        {
            public IntPtr dwItem1;
            public IntPtr dwItem2;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SHChangeNotifyEntry
        {
            public IntPtr pIdl;
            [MarshalAs(UnmanagedType.Bool)]
            public Boolean Recursively;
        }
        [Flags]
        enum SHCNE {
            SHCNE_RENAMEITEM      = 0x00000001,
            SHCNE_CREATE          = 0x00000002,
            SHCNE_DELETE          = 0x00000004,
            SHCNE_MKDIR           = 0x00000008,
            SHCNE_RMDIR           = 0x00000010,
            SHCNE_MEDIAINSERTED       = 0x00000020,
            SHCNE_MEDIAREMOVED    = 0x00000040,
            SHCNE_DRIVEREMOVED    = 0x00000080,
            SHCNE_DRIVEADD        = 0x00000100,
            SHCNE_NETSHARE        = 0x00000200,
            SHCNE_NETUNSHARE      = 0x00000400,
            SHCNE_ATTRIBUTES      = 0x00000800,
            SHCNE_UPDATEDIR       = 0x00001000,
            SHCNE_UPDATEITEM      = 0x00002000,
            SHCNE_SERVERDISCONNECT    = 0x00004000,
            SHCNE_UPDATEIMAGE     = 0x00008000,
            SHCNE_DRIVEADDGUI     = 0x00010000,
            SHCNE_RENAMEFOLDER    = 0x00020000,
            SHCNE_FREESPACE       = 0x00040000,
            SHCNE_EXTENDED_EVENT      = 0x04000000,
            SHCNE_ASSOCCHANGED    = 0x08000000,
            SHCNE_DISKEVENTS      = 0x0002381F,
            SHCNE_GLOBALEVENTS    = 0x0C0581E0,
            SHCNE_ALLEVENTS       = 0x7FFFFFFF,
        }
        private static void DeviceArrived(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            if (info.IsReady == true)
            {
                Form1.raiseStorageEvent(eMediaType.NotSet,true, drive);
            }
        }
        private static void DeviceRemoved(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            Form1.raiseStorageEvent(eMediaType.DeviceRemoved,true, drive);
        }
    }
}
