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
using System.ComponentModel;
using System.Text;
using System.Xml;
using NativeWifi;
using OpenMobile;
using OpenMobile.Data;
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
                if (networks[i].networkConnectable == true)
                {
                    connectionInfo info = new connectionInfo(System.Text.ASCIIEncoding.ASCII.GetString(networks[i].dot11Ssid.SSID, 0, (int)networks[i].dot11Ssid.SSIDLength), networks[i].GetHashCode().ToString(), getSpeed(networks[i].Dot11PhyTypes), networks[i].wlanSignalQuality, getType(networks[i]) + " (" + getSecurity(networks[i]) + ")", getPass(networks[i]));
                    info.IsConnected=((networks[i].flags&Wlan.WlanAvailableNetworkFlags.Connected)==Wlan.WlanAvailableNetworkFlags.Connected);
                    if (!connections.Contains(info))
                        connections.Add(info);
                }
            return connections.ToArray();
        }

        private ePassType getPass(Wlan.WlanAvailableNetwork wlanAvailableNetwork)
        {
            if (wlanAvailableNetwork.securityEnabled && string.IsNullOrEmpty(wlanAvailableNetwork.profileName))
            {
                if ((wlanAvailableNetwork.dot11DefaultAuthAlgorithm == Wlan.Dot11AuthAlgorithm.WPA) || (wlanAvailableNetwork.dot11DefaultAuthAlgorithm == Wlan.Dot11AuthAlgorithm.RSNA))
                    return ePassType.UserPass;
                else
                    return ePassType.SharedKey;
            }
            else
                return ePassType.None;
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

        private string getType(Wlan.WlanAvailableNetwork network)
        {
            if (network.Dot11PhyTypes.Length == 0)
                return "Unknown Device";
            if (network.dot11BssType==Wlan.Dot11BssType.Infrastructure)
                return getRadio(network.Dot11PhyTypes);
            else
                return "Ad-Hoc " + getRadio(network.Dot11PhyTypes);
        }
        private string getRadio(Wlan.Dot11PhyType[] dot11PhyTypes)
        {
            if (dot11PhyTypes.Length == 1)
                if (dot11PhyTypes[0]==Wlan.Dot11PhyType.IrBaseband)
                    return "IR";
            string ret = "802.11";
            if (Array.Exists(dot11PhyTypes,t=>t== Wlan.Dot11PhyType.OFDM))
                ret+= "a";
            if (Array.Exists(dot11PhyTypes,t=>t==Wlan.Dot11PhyType.HRDSSS))
                ret += "b";
            if (Array.Exists(dot11PhyTypes,t=>t==Wlan.Dot11PhyType.ERP))
                ret+= "g";
            if (Array.Exists(dot11PhyTypes,t=>t==Wlan.Dot11PhyType.HT))
                ret += "n";
            if (ret == "802.11")
                return "Unknown Type (" + dot11PhyTypes[0].ToString() + ")";
            else
                return ret;
        }

        private string getSecurity(Wlan.WlanAvailableNetwork an)
        {
            switch (an.dot11DefaultAuthAlgorithm)
            {
                case Wlan.Dot11AuthAlgorithm.WPA:
                    return "WPA-Enterprise";
                case Wlan.Dot11AuthAlgorithm.WPA_PSK:
                    return "WPA-PSK";
                case Wlan.Dot11AuthAlgorithm.RSNA:
                    return "WPA2-Enterprise";
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

        public bool refresh()
        {
            try
            {
                client.Interfaces[0].Scan();
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false; //Adapter can't scan (powered off, broken, etc.)
            }
        }

        public bool connect(OpenMobile.connectionInfo connection)
        {
            Wlan.WlanAvailableNetwork[] networks = client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            Wlan.WlanAvailableNetwork network = Array.Find(networks, p => p.GetHashCode().ToString() == connection.UID);
            try
            {
                if (network.profileName == "")
                    return connectToNew(network,connection.Credentials,connection.NetworkName);
                else
                    return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
            }
            catch (Exception) { return false; }
        }

        private bool connectToNew(Wlan.WlanAvailableNetwork info,string Password,string givenName)
        {
            StringBuilder xmlProfileString = new StringBuilder(100);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            string networkName = System.Text.ASCIIEncoding.ASCII.GetString(info.dot11Ssid.SSID, 0, (int)info.dot11Ssid.SSIDLength);
            if (info.dot11BssType==Wlan.Dot11BssType.Independent)
            {
                networkName+="-adhoc";
                givenName+="-adhoc";
            }
            XmlWriter writer = XmlWriter.Create(xmlProfileString, settings);
            writer.WriteStartElement("WLANProfile", @"http://www.microsoft.com/networking/WLAN/profile/v1");
            writer.WriteElementString("name", networkName);
            writer.WriteStartElement("SSIDConfig");
            writer.WriteStartElement("SSID");
            writer.WriteElementString("name", networkName);
            writer.WriteEndElement();
            if (networkName!=givenName)
                writer.WriteElementString("nonBroadcast", "true");
            writer.WriteEndElement();
            if (info.dot11BssType==Wlan.Dot11BssType.Infrastructure)
                writer.WriteElementString("connectionType", "ESS");
            else
                writer.WriteElementString("connectionType", "IBSS");
            writer.WriteStartElement("MSM");
            writer.WriteStartElement("security");
            writer.WriteStartElement("authEncryption");
            switch(info.dot11DefaultAuthAlgorithm)
            {
                case Wlan.Dot11AuthAlgorithm.Open_Network:
                    writer.WriteElementString("authentication", "open");
                    break;
                case Wlan.Dot11AuthAlgorithm.WPA: //probably doesn't work yet
                    writer.WriteElementString("authentication", "WPA");
                    break;
                case Wlan.Dot11AuthAlgorithm.WPA_PSK:
                    writer.WriteElementString("authentication", "WPAPSK");
                    break;
                case Wlan.Dot11AuthAlgorithm.RSNA: //probably doesn't work yet
                    writer.WriteElementString("authentication", "WPA2");
                    break;
                case Wlan.Dot11AuthAlgorithm.RSNA_PSK:
                    writer.WriteElementString("authentication", "WPA2PSK");
                    break;
                default:
                    writer.WriteElementString("authentication", "shared");
                    break;
            }
            switch(info.dot11DefaultCipherAlgorithm)
            {
                case Wlan.Dot11CipherAlgorithm.None:
                    writer.WriteElementString("encryption", "none");
                    break;
                case Wlan.Dot11CipherAlgorithm.TKIP:
                    writer.WriteElementString("encryption", "TKIP");
                    break;
                case Wlan.Dot11CipherAlgorithm.CCMP:
                    writer.WriteElementString("encryption", "AES");
                    break;
                case Wlan.Dot11CipherAlgorithm.WEP:
                case Wlan.Dot11CipherAlgorithm.WEP104:
                case Wlan.Dot11CipherAlgorithm.WEP40:
                    writer.WriteElementString("encryption", "WEP");
                    break;
            }
            switch (info.dot11DefaultAuthAlgorithm)
            {
                case Wlan.Dot11AuthAlgorithm.RSNA:
                case Wlan.Dot11AuthAlgorithm.WPA:
                    writer.WriteElementString("useOneX", "true");
                    writer.WriteEndElement();
                    writeEAP(ref writer, Password);
                    break;
                case Wlan.Dot11AuthAlgorithm.Open_Network:
                    writer.WriteElementString("useOneX", "false");
                    writer.WriteEndElement();
                    break;
                default:
                    writer.WriteElementString("useOneX", "false");
                    writer.WriteEndElement();
                    writer.WriteStartElement("sharedKey");
                    switch (info.dot11DefaultCipherAlgorithm)
                    {
                        case Wlan.Dot11CipherAlgorithm.TKIP:
                        case Wlan.Dot11CipherAlgorithm.CCMP: //AES
                            writer.WriteElementString("keyType", "passPhrase");
                            break;
                        default:
                            writer.WriteElementString("keyType", "networkKey");
                            break;
                    }
                    writer.WriteElementString("protected", "false");
                    if (Password == null)
                        Password = "";
                    writer.WriteElementString("keyMaterial", Password);
                    writer.WriteEndElement();
                    break;
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
            client.Interfaces[0].SetProfile(Wlan.WlanProfileFlags.AllUser, xmlProfileString.ToString(), true);
            if ((info.dot11DefaultAuthAlgorithm == Wlan.Dot11AuthAlgorithm.RSNA) || (info.dot11DefaultAuthAlgorithm == Wlan.Dot11AuthAlgorithm.WPA))
            {
                string[] args = Password.Split(new char[] { ':' }, 2);
                if (args.Length < 2)
                    return false;
                client.Interfaces[0].SetPEAPXML(networkName, args[0], args[1]); 
            }
            return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, networkName, 500);
        }

        private void writeEAP(ref XmlWriter writer,string Password)
        {
            writer.WriteStartElement("OneX", "http://www.microsoft.com/networking/OneX/v1");
            writer.WriteElementString("authMode", "machineOrUser");
            writer.WriteStartElement("EAPConfig");
            writer.WriteStartElement("EapHostConfig", "http://www.microsoft.com/provisioning/EapHostConfig");
                writer.WriteStartElement("EapMethod");
                        writer.WriteElementString("Type", "http://www.microsoft.com/provisioning/EapCommon","25");
                        writer.WriteElementString("AuthorId", "http://www.microsoft.com/provisioning/EapCommon","0");
                    writer.WriteEndElement();
                    writer.WriteStartElement("Config", "http://www.microsoft.com/provisioning/EapHostConfig");
                    writer.WriteStartElement("Eap", "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1");
                            writer.WriteElementString("Type", "25");
                            writer.WriteStartElement("EapType", "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1");
                                writer.WriteElementString("FastReconnect", "true");
                                writer.WriteElementString("InnerEapOptional", "0");
                                writer.WriteStartElement("Eap", "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1");
                                    writer.WriteElementString("Type", "26");
                                    writer.WriteStartElement("EapType", "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1");
                                        writer.WriteElementString("UseWinLogonCredentials", "false");
                                    writer.WriteEndElement();
                                writer.WriteEndElement();
                                writer.WriteElementString("EnableQuarantineChecks", "false");
                                writer.WriteElementString("RequireCryptoBinding","false");
                                writer.WriteElementString("PeapExtensions", null);
                            writer.WriteEndElement();
                        writer.WriteEndElement();
                    writer.WriteEndElement();
                writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
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
            try
            {
                client = new WlanClient();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return eLoadStatus.LoadFailedGracefulUnloadRequested; //No Wifi Service Available
            }
            if (client.Interfaces.Length == 0)
                return eLoadStatus.LoadFailedGracefulUnloadRequested; //No Wifi Adapters Present
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Default.Wifi", "WinWifi");
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
            try
            {
                if (client.Interfaces[0].InterfaceState == Wlan.WlanInterfaceState.NotReady)
                    return;
                if (notifyData.NotificationCode.ToString() == "NetworkAvailable")
                    raiseWirelessEvent(eWirelessEvent.WirelessNetworksAvailable, "");
                else if (notifyData.NotificationCode.ToString() == "SignalQualityChange")
                    raiseWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
            }
            catch (Win32Exception) { }
        }

        void Wifi_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            try
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
            catch (Win32Exception) { }
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
            return null;
        }
        public void Dispose()
        {
            //
        }
        #endregion
    }
}
