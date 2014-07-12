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
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Management;
using System.Management.Instrumentation;
using System.Runtime.InteropServices; 

namespace OMHal
{
    static class DriveHandler
    {
        public const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume
        private const int DBT_DEVTYP_PORT = 0x00000003;      // serial, parallel 
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DBT_DEVTYP_HANDLE = 0x00000006;
        private const int DBT_DEVTYP_OEM = 0x00000000;      

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public int dbcv_unitmask;
        }

        //delegate enables asynchronous call for getting the list of friendly names  
        private static getFriendlyNameListDelegate mDeleg;

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
                    DriveArrived(path.ToString());
                }
                else if ((int)m.LParam == (int)SHCNE.SHCNE_MEDIAREMOVED)
                {
                    StringBuilder path = new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DriveRemoved(path.ToString());
                }else if((int)m.LParam == (int)SHCNE.SHCNE_DRIVEADD)
                {
                    StringBuilder path=new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DriveArrived(path.ToString());
                }
                else if ((int)m.LParam == (int)SHCNE.SHCNE_DRIVEREMOVED)
                {
                    StringBuilder path = new StringBuilder();
                    SHGetPathFromIDListW(nfy.dwItem1, path);
                    DriveRemoved(path.ToString());
                }
            }
        }

        public static void WndProc_Devices(ref Message m)
        {
            unsafe
            {
                int devType;
                switch (m.WParam.ToInt32())
                {
                    case DBT_DEVICEARRIVAL:
                        {
                            devType = Marshal.ReadInt32(m.LParam, 4);
                            if (devType == DBT_DEVTYP_PORT)
                                Form1.raiseSystemEvent(eFunction.USBDeviceAdded, "PORT", "", "");
                            else if (devType == DBT_DEVTYP_VOLUME)
                                Form1.raiseSystemEvent(eFunction.USBDeviceAdded, "VOLUME", "", "");
                            else 
                                Form1.raiseSystemEvent(eFunction.USBDeviceAdded, "UNKNOWN", "", "");

                                //// usb device inserted, call the query      
                                //mDeleg = new getFriendlyNameListDelegate(getFriendlyNameList);
                                //AsyncCallback callback = new AsyncCallback(getFriendlyNameListCallback);

                                //// invoke the thread that will handle getting the friendly names  
                                //mDeleg.BeginInvoke(callback, new object());

                        }
                        break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        {
                            devType = Marshal.ReadInt32(m.LParam, 4);
                            if (devType == DBT_DEVTYP_PORT)
                                Form1.raiseSystemEvent(eFunction.USBDeviceRemoved, "PORT", "", "");
                            else if (devType == DBT_DEVTYP_VOLUME)
                                Form1.raiseSystemEvent(eFunction.USBDeviceRemoved, "VOLUME", "", "");
                            else 
                                Form1.raiseSystemEvent(eFunction.USBDeviceRemoved, "UNKNOWN", "", "");                                
                            
                            // usb device removed, call the query  

                            //mDeleg = new getFriendlyNameListDelegate(getFriendlyNameList);
                            //AsyncCallback callback = new AsyncCallback(getFriendlyNameListCallback);


                            //// invoke the thread that will handle getting the friendly names   
                            //mDeleg.BeginInvoke(callback, new object());
                        }
                        break;
                }
            }
        }

        // function queries the system using WMI and gets an arraylist of all com port devices     
        static private ArrayList getFriendlyNameList()
        {
            ArrayList deviceList = new ArrayList();

            // getting a list of all available com port devices and their friendly names     
            // must add System.Management DLL resource to solution before using this     
            // Project -> Add Reference -> .Net tab, choose System.Management     

            // source:     
            // http://www.codeproject.com/KB/system/hardware_properties_c_.aspx     

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select Name from Win32_PnpEntity");

            foreach (ManagementObject devices in searcher.Get())
            {
                string name = devices.GetPropertyValue("Name").ToString();

                // only add item if the friendly names contains "COM"     
                // we only want to add COM Ports     
                if (name.Contains("(COM"))
                {
                    deviceList.Add(name);
                }
            }

            searcher.Dispose();
            return deviceList;
        }

        // delegate wrapper function for the getFriendlyNameList function  
        private delegate ArrayList getFriendlyNameListDelegate();

        // callback method when the thread returns  
        static private void getFriendlyNameListCallback(IAsyncResult ar)
        {
            // got the returned arrayList, now we can do whatever with it  
            ArrayList result = mDeleg.EndInvoke(ar);
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
        private static void DriveArrived(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            if (info.IsReady == true)
            {
                Form1.raiseStorageEvent(eMediaType.NotSet,true, drive);
            }
        }
        private static void DriveRemoved(string drive)
        {
            DriveInfo info = new DriveInfo(drive);
            Form1.raiseStorageEvent(eMediaType.DeviceRemoved,true, drive);
        }
    }
}
