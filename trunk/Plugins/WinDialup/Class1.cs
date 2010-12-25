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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dialup.RAS;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Net;

namespace WinDialup
{
    public class Class1:INetwork
    {
        #region INetwork Members
#pragma warning disable 0067
        public event WirelessEvent OnWirelessEvent;
#pragma warning restore 0067
        private IntPtr conPtr=IntPtr.Zero;

        public connectionInfo[] getAvailableNetworks()
        {
            RASCONN lprasConn = new RASCONN();
            
            lprasConn.dwSize = Marshal.SizeOf(typeof(RASCONN));
            lprasConn.hrasconn = IntPtr.Zero;

            int lpcb = Marshal.SizeOf(typeof(RASCONN));
            int lpcConnections = 0;

            int nRet = RAS.RasEnumConnections(ref lprasConn, ref lpcb,ref lpcConnections);
            if (lpcConnections > 0)
            {
                conPtr = lprasConn.hrasconn;
            }

            int lpNames = 10;
            int entryNameSize = Marshal.SizeOf(typeof(RasEntryName));
            int lpSize = lpNames * entryNameSize;
            RasEntryName[] names = new RasEntryName[lpNames];
            for (int i = 0; i < names.Length; i++)
            {
                names[i].dwSize = entryNameSize;
            }

            uint retval = RAS.RasEnumEntries(IntPtr.Zero, IntPtr.Zero, names,ref lpSize, ref lpNames);
            List<connectionInfo> info = new List<connectionInfo>();
            if (lpNames > 0)
                for (int i = 0; i < lpNames; i++)
                    info.Add(new connectionInfo(names[i].szEntryName,names[i].szEntryName,56,0,"Dial-Up Connection",ePassType.None));
            return info.ToArray();
        }
        public bool refresh()
        {
            return false;
        }
        public bool connect(connectionInfo connection)
        {
            int con = 0;
            int ret=RAS.InternetDial(IntPtr.Zero, connection.NetworkName, 2, ref con, 0);
            if (con > 0)
                if (Network.checkForInternet() == Network.connectionStatus.InternetAccess)
                    theHost.execute(eFunction.connectedToInternet);
            return con != 0;
        }

        public bool disconnect(connectionInfo connection)
        {
            int ret=(int)RAS.RasHangUp(conPtr);
            if (ret == 0)
                theHost.execute(eFunction.disconnectedFromInternet);
            return (ret == 0);
        }

        public eConnectionStatus getConnectionStatus()
        {
            RasStats stats=new RasStats();
            if (RAS.RasGetConnectionStatistics(conPtr, stats) == 0)
                return eConnectionStatus.Connected;
            else
                return eConnectionStatus.Disconnected;
        }

        #endregion

        #region IBasePlugin Members
        public Settings loadSettings()
        {
            return null;
        }
        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return String.Empty; }
        }

        public string pluginName
        {
            get { return "WinDialup"; }
        }

        public float pluginVersion
        {
            get { return 0.9F; }
        }

        public string pluginDescription
        {
            get { return "Dialup Support for Windows"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
