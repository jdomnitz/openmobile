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
using System.Text;
using System.Xml;

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
                if (networks[i].networkConnectable == true)
                {
                    connections.Add(new connectionInfo(System.Text.ASCIIEncoding.ASCII.GetString(networks[i].dot11Ssid.SSID, 0, (int)networks[i].dot11Ssid.SSIDLength), networks[i].GetHashCode().ToString(), getSpeed(networks[i].Dot11PhyTypes), networks[i].wlanSignalQuality, getRadio(networks[i].Dot11PhyTypes[0])+" ("+ getSecurity(networks[i])+")"));
                    if ((client.Interfaces[0].InterfaceState==Wlan.WlanInterfaceState.Connected)&&(client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.dot11Ssid.SSID == networks[i].dot11Ssid.SSID))
                        connections[connections.Count - 1].IsConnected = true;
                }
            return connections.ToArray();
        }

        private int getSpeed(Wlan.Dot11PhyType[] dot11PhyType)
        {
            if (dot11PhyType.Length == 0)
                return 11000;
            switch (dot11PhyType[0])
            {
                case Wlan.Dot11PhyType.HRDSSS:
                    return 11000;
                case Wlan.Dot11PhyType.HT:
                    return 130000;
                case Wlan.Dot11PhyType.ERP:
                    return 54000;
                case Wlan.Dot11PhyType.OFDM:
                    return 54000;
            }
            return 1000; //Unknown
        }

        private string getRadio(Wlan.Dot11PhyType dot11PhyType)
        {
            switch (dot11PhyType)
            {
                case Wlan.Dot11PhyType.OFDM:
                    return "802.11a";
                case Wlan.Dot11PhyType.ERP:
                    return "802.11g";
                case Wlan.Dot11PhyType.HT:
                    return "802.11n";
                case Wlan.Dot11PhyType.HRDSSS:
                    return "802.11b";
                case Wlan.Dot11PhyType.IrBaseband:
                    return "IR";
            }
            return "Unknown Type (" + dot11PhyType.ToString() + ")";
        }

        private string getSecurity(Wlan.WlanAvailableNetwork an)
        {
            switch (an.dot11DefaultAuthAlgorithm)
            {
                case Wlan.Dot11AuthAlgorithm.WPA:
                    return "WPA";
                case Wlan.Dot11AuthAlgorithm.WPA_PSK:
                    return "WPA-PSK";
                case Wlan.Dot11AuthAlgorithm.RSNA:
                    return "WPA2";
                case Wlan.Dot11AuthAlgorithm.RSNA_PSK:
                    return "WPA2-PSK";
                case Wlan.Dot11AuthAlgorithm.Open_Network:
                    switch (an.dot11DefaultCipherAlgorithm)
                    {
                        case Wlan.Dot11CipherAlgorithm.None:
                            return "Open Network";
                        case Wlan.Dot11CipherAlgorithm.WEP:
                        case Wlan.Dot11CipherAlgorithm.WEP104:
                        case Wlan.Dot11CipherAlgorithm.WEP40:
                            return "WEP";
                    }
                    break;
                case Wlan.Dot11AuthAlgorithm.Shared_Key_Network:
                    return "WEP-PSK";
            }
            return "Unknown Security ("+an.dot11DefaultAuthAlgorithm.ToString()+", "+an.dot11DefaultCipherAlgorithm.ToString()+")";
        }

        public void refresh()
        {
            client.Interfaces[0].Scan();
        }

        public bool connect(OpenMobile.connectionInfo connection)
        {
            Wlan.WlanAvailableNetwork[] networks = client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            Wlan.WlanAvailableNetwork network = Array.Find(networks, p => p.GetHashCode().ToString() == connection.UID);
            try
            {
                if (network.profileName == "")
                {
                    if (network.dot11DefaultAuthAlgorithm == Wlan.Dot11AuthAlgorithm.Open_Network)
                        return connectToOpen(network);
                    else
                        return false;
                }
                else
                    return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
            }
            catch (Exception) { return false; }
        }
        private bool connectToOpen(NativeWifi.Wlan.WlanAvailableNetwork info)
        {
            StringBuilder xmlProfileString = new StringBuilder(100);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            string networkName = System.Text.ASCIIEncoding.ASCII.GetString(info.dot11Ssid.SSID, 0, (int)info.dot11Ssid.SSIDLength);
            XmlWriter writer = XmlWriter.Create(xmlProfileString, settings);
            writer.WriteStartElement("WLANProfile", @"http://www.microsoft.com/networking/WLAN/profile/v1");
            writer.WriteElementString("name", networkName);
            writer.WriteStartElement("SSIDConfig");
            writer.WriteStartElement("SSID");
            writer.WriteElementString("hex", toHex(info.dot11Ssid.SSID, info.dot11Ssid.SSIDLength) );
            writer.WriteElementString("name", networkName);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteElementString("connectionType", "ESS");
            writer.WriteStartElement("MSM");
            writer.WriteStartElement("security");
            writer.WriteStartElement("authEncryption");
            writer.WriteElementString("authentication", "open");
            writer.WriteElementString("encryption", "none");
            writer.WriteElementString("useOneX", "false");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
            client.Interfaces[0].SetProfile(Wlan.WlanProfileFlags.AllUser, xmlProfileString.ToString(), true);
            return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, networkName,500);
        }

        private string toHex(byte[] p, uint length)
        {
            string ret = "";
            for (int i = 0; i < length; i++)
                ret += p[i].ToString("x2").ToUpper();
            return ret;
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
                return eLoadStatus.LoadFailedGracefulUnloadRequested;
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
        public Settings loadSettings()
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
