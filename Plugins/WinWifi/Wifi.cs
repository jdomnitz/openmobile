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
using NativeWifi;
using OpenMobile;
using OpenMobile.Plugin;

namespace WinWifi
{
    public sealed class Wifi:INetwork
    {
        private WlanClient client;
        public event WirelessEvent OnWirelessEvent;

        public connectionInfo[] getAvailableNetworks()
        {
            Wlan.WlanAvailableNetwork[] networks=client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            List<connectionInfo> connections=new List<connectionInfo>();
            for (int i = 0; i < networks.Length; i++)
                if (networks[i].networkConnectable==true)
                    connections.Add(new connectionInfo(networks[i].profileName,networks[i].GetHashCode().ToString(), 11000, networks[i].wlanSignalQuality));
            return connections.ToArray();
        }

        public void refresh()
        {
            client.Interfaces[0].Scan();
        }

        public bool connect(OpenMobile.connectionInfo connection)
        {
            Wlan.WlanAvailableNetwork[] networks = client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            Wlan.WlanAvailableNetwork network = Array.Find(networks, p => p.profileName == connection.NetworkName);
            try
            {
                return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
            }
            catch (Exception) { return false; }
        }

        public bool disconnect(OpenMobile.connectionInfo connection)
        {
            try
            {
                return client.Interfaces[0].Disconnect();
            }
            catch (Exception) { return false; }
        }

        public eConnectionStatus getConnectionStatus()
        {
            switch (client.Interfaces[0].CurrentConnection.isState)
            {
                case Wlan.WlanInterfaceState.Associating:
                case Wlan.WlanInterfaceState.Authenticating:
                case Wlan.WlanInterfaceState.Discovering:
                    return eConnectionStatus.Connecting;
                case Wlan.WlanInterfaceState.Disconnected:
                case Wlan.WlanInterfaceState.Disconnecting:
                    return eConnectionStatus.Disconnected;
                case Wlan.WlanInterfaceState.Connected:
                    return eConnectionStatus.Connected;
            }
            return eConnectionStatus.Error;
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            client = new WlanClient();
            if (client.Interfaces.Length == 0)
                return eLoadStatus.LoadFailedUnloadRequested;
            client.Interfaces[0].WlanConnectionNotification += new WlanClient.WlanInterface.WlanConnectionNotificationEventHandler(Wifi_WlanConnectionNotification);
            client.Interfaces[0].WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(Wifi_WlanNotification);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        private void raiseWirelessEvent(eWirelessEvent e, string arg)
        {
            if (OnWirelessEvent!=null)
                OnWirelessEvent(e, arg);
        }
        void Wifi_WlanNotification(Wlan.WlanNotificationData notifyData)
        {
            if (notifyData.NotificationCode.ToString() == "NetworkAvailable")
                raiseWirelessEvent(eWirelessEvent.WirelessNetworksAvailable, "");
            else if (notifyData.NotificationCode.ToString() == "SignalQualityChange")
                raiseWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
        }

        void Wifi_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            if (notifyData.NotificationCode.ToString() == "ConnectionStart")
                raiseWirelessEvent(eWirelessEvent.ConnectingToWirelessNetwork, connNotifyData.profileName);
            else if (notifyData.NotificationCode.ToString() == "Connected")
            {
                raiseWirelessEvent(eWirelessEvent.ConnectedToWirelessNetwork, connNotifyData.profileName);
                if (OpenMobile.Net.Network.checkForInternet() == OpenMobile.Net.Network.connectionStatus.LoginRequired)
                    raiseWirelessEvent(eWirelessEvent.WirelessNetworkRequiresLogin, connNotifyData.profileName);
            }
            else if (notifyData.NotificationCode.ToString() == "Disconnected")
                raiseWirelessEvent(eWirelessEvent.DisconnectedFromWirelessNetwork, "");
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "WinWifi"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Wifi Support"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
