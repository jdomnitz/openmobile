/*********************************************************************************
This file is part of Open Mobile.
Open Mobile is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
Open Mobile is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with Open Mobile. If not, see <http://www.gnu.org/licenses/>.
There is one additional restriction when using this framework regardless of modifications to it.
The About Panel or its contents must be easily accessible by the end users.
This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using NativeWifi;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;
namespace WinWifi
{
    [PluginLevel(PluginLevels.System)]
    public sealed class Wifi : INetwork
    {
        private WlanClient client;
        private IPluginHost theHost;
        public event WirelessEvent OnWirelessEvent;
        public connectionInfo[] getAvailableNetworks()
        {
            List<connectionInfo> connections = new List<connectionInfo>();
            for (int j = 0; j < client.Interfaces.Length; j++)
            {
                Wlan.WlanAvailableNetwork[] networks = client.Interfaces[j].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                for (int i = 0; i < networks.Length; i++)
                    if (networks[i].networkConnectable == true)
                    {
                        connectionInfo info = new connectionInfo(System.Text.ASCIIEncoding.ASCII.GetString(networks[i].dot11Ssid.SSID, 0, (int)networks[i].dot11Ssid.SSIDLength), networks[i].GetHashCode().ToString(), getSpeed(networks[i].Dot11PhyTypes), networks[i].wlanSignalQuality, getType(networks[i]) + " (" + getSecurity(networks[i]) + ")", getPass(networks[i]));
                        info.IsConnected = ((networks[i].flags & Wlan.WlanAvailableNetworkFlags.Connected) == Wlan.WlanAvailableNetworkFlags.Connected);
                        if (!connections.Contains(info))
                            connections.Add(info);
                    }
            }
            return connections.ToArray();
        }
        public connectionInfo[] getAvailableNetworks(string InterfaceName)
        {
            List<connectionInfo> connections = new List<connectionInfo>();
            for (int j = 0; j < client.Interfaces.Length; j++)
            {
                if (client.Interfaces[j].InterfaceName == InterfaceName)
                {
                    Wlan.WlanAvailableNetwork[] networks = client.Interfaces[j].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                    for (int i = 0; i < networks.Length; i++)
                        if (networks[i].networkConnectable == true)
                        {
                            connectionInfo info = new connectionInfo(System.Text.ASCIIEncoding.ASCII.GetString(networks[i].dot11Ssid.SSID, 0, (int)networks[i].dot11Ssid.SSIDLength), networks[i].GetHashCode().ToString(), getSpeed(networks[i].Dot11PhyTypes), networks[i].wlanSignalQuality, getType(networks[i]) + " (" + getSecurity(networks[i]) + ")", getPass(networks[i]));
                            info.IsConnected = ((networks[i].flags & Wlan.WlanAvailableNetworkFlags.Connected) == Wlan.WlanAvailableNetworkFlags.Connected);
                            if (!connections.Contains(info))
                                connections.Add(info);
                        }
                    break;
                }
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
            if (network.dot11BssType == Wlan.Dot11BssType.Infrastructure)
                return getRadio(network.Dot11PhyTypes);
            else
                return "Ad-Hoc " + getRadio(network.Dot11PhyTypes);
        }
        private string getRadio(Wlan.Dot11PhyType[] dot11PhyTypes)
        {
            if (dot11PhyTypes.Length == 1)
                if (dot11PhyTypes[0] == Wlan.Dot11PhyType.IrBaseband)
                    return "IR";
            string ret = "802.11";
            if (Array.Exists(dot11PhyTypes, t => t == Wlan.Dot11PhyType.OFDM))
                ret += "a";
            if (Array.Exists(dot11PhyTypes, t => t == Wlan.Dot11PhyType.HRDSSS))
                ret += "b";
            if (Array.Exists(dot11PhyTypes, t => t == Wlan.Dot11PhyType.ERP))
                ret += "g";
            if (Array.Exists(dot11PhyTypes, t => t == Wlan.Dot11PhyType.HT))
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
            return "Unknown Security (" + an.dot11DefaultAuthAlgorithm.ToString() + ", " + an.dot11DefaultCipherAlgorithm.ToString() + ")";
        }

        public bool refresh()
        {
            //try
            //{
                for (int i = 0; i < client.Interfaces.Length; i++)
                {
                    try
                    {
                        client.Interfaces[i].Scan();
                        
                        //getAvailableNetworks();
                    }
                    catch { }
                }
                return true;
            //}
            //catch (System.ComponentModel.Win32Exception)
            //{
            //    return false; //Adapter can't scan (powered off, broken, etc.)
            //}
        }

        public bool connect(OpenMobile.connectionInfo connection)
        {
            return false;
        }

        public bool connect(OpenMobile.connectionInfo connection, string InterfaceNameToConnectTo = "")
        {
            Wlan.WlanAvailableNetwork[] networks;
            Wlan.WlanAvailableNetwork network;
            if ((client.Interfaces.Length > 0) && (InterfaceNameToConnectTo == ""))
            {
                //connect to the first/default interface
                networks = client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                network = Array.Find(networks, p => p.GetHashCode().ToString() == connection.UID);
                try
                {
                    if (network.profileName == "")
                        return connectToNew(network, connection.Credentials, connection.NetworkName);
                    else
                        return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
                }
                catch (Exception) { }
            }
            else
            {
                for (int i = 0; i < client.Interfaces.Length; i++)
                {
                    if (client.Interfaces[i].InterfaceName == InterfaceNameToConnectTo)
                    {
                        networks = client.Interfaces[i].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                        network = Array.Find(networks, p => p.GetHashCode().ToString() == connection.UID);
                        try
                        {
                            if (network.profileName == "")
                                return connectToNew(network, connection.Credentials, connection.NetworkName);
                            else
                                return client.Interfaces[i].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
                        }
                        catch (Exception) { }
                    }
                }
            }

            //networks = client.Interfaces[0].GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            //network = Array.Find(networks, p => p.GetHashCode().ToString() == connection.UID);
            //try
            //{
            //    if (network.profileName == "")
            //        return connectToNew(network, connection.Credentials, connection.NetworkName);
            //    else
            //        return client.Interfaces[0].ConnectSynchronously(Wlan.WlanConnectionMode.Profile, network.dot11BssType, network.profileName, 500);
            //}
            //catch (Exception) { return false; }
            return false;
        }
        private bool connectToNew(Wlan.WlanAvailableNetwork info, string Password, string givenName)
        {
            StringBuilder xmlProfileString = new StringBuilder(100);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            string networkName = System.Text.ASCIIEncoding.ASCII.GetString(info.dot11Ssid.SSID, 0, (int)info.dot11Ssid.SSIDLength);
            if (info.dot11BssType == Wlan.Dot11BssType.Independent)
            {
                networkName += "-adhoc";
                givenName += "-adhoc";
            }
            XmlWriter writer = XmlWriter.Create(xmlProfileString, settings);
            writer.WriteStartElement("WLANProfile", @"http://www.microsoft.com/networking/WLAN/profile/v1");
            writer.WriteElementString("name", networkName);
            writer.WriteStartElement("SSIDConfig");
            writer.WriteStartElement("SSID");
            writer.WriteElementString("name", networkName);
            writer.WriteEndElement();
            if (networkName != givenName)
                writer.WriteElementString("nonBroadcast", "true");
            writer.WriteEndElement();
            if (info.dot11BssType == Wlan.Dot11BssType.Infrastructure)
                writer.WriteElementString("connectionType", "ESS");
            else
                writer.WriteElementString("connectionType", "IBSS");
            writer.WriteStartElement("MSM");
            writer.WriteStartElement("security");
            writer.WriteStartElement("authEncryption");
            switch (info.dot11DefaultAuthAlgorithm)
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
            switch (info.dot11DefaultCipherAlgorithm)
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
        private void writeEAP(ref XmlWriter writer, string Password)
        {
            writer.WriteStartElement("OneX", "http://www.microsoft.com/networking/OneX/v1");
            writer.WriteElementString("authMode", "machineOrUser");
            writer.WriteStartElement("EAPConfig");
            writer.WriteStartElement("EapHostConfig", "http://www.microsoft.com/provisioning/EapHostConfig");
            writer.WriteStartElement("EapMethod");
            writer.WriteElementString("Type", "http://www.microsoft.com/provisioning/EapCommon", "25");
            writer.WriteElementString("AuthorId", "http://www.microsoft.com/provisioning/EapCommon", "0");
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
            writer.WriteElementString("RequireCryptoBinding", "false");
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

        //List<Dictionary<string, string>> wifiInterfaces;
        List<Notification> wifiNotifications = new List<Notification>();
        Dictionary<string, string> initConnections = new Dictionary<string, string>();
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            try
            {
                client = new WlanClient();
                theHost = host;

                //commands
                OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Networking", "WiFi", "Refresh", RefreshWifi, 2, false, "Refreshes the interfaces and networks"));
                OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Networking", "WiFi", "Connect", ConnectToWiFi, 2, false, "Connect to the specified wifi network: param={int screen, string UID}"));
                OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Networking", "WiFi", "Disconnect", DisconnectFromWiFi, 0, false, "Disconnects from wifi or the specified wifi network(s): OPTIONAL param={string UID, string UID, string UID...}"));

                //datasource
                OM.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Networking", "WiFi", "Networks", DataSource.DataTypes.raw, "Provides the wifi networks in range"));
                OM.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Networking", "WiFi", "StatusChange", DataSource.DataTypes.raw, "Provides the wifi network status changes (connecting, connected, disconnected, etc)"));

                //OM.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Networking", "WiFi", "Interfaces", DataSource.DataTypes.raw, "Provides the wifi interfaces"));
                //OM.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Networking", "WiFi", "Interfaces", 0, DataSource.DataTypes.text, WiFiInterfaces, "Returns the wireless interfaces (No parameters, returns: List<Dictionary<string, object>>)"));
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return eLoadStatus.LoadFailedGracefulUnloadRequested; //No Wifi Service Available
            }
            if (client.Interfaces.Length == 0)
                return eLoadStatus.LoadFailedGracefulUnloadRequested; //No Wifi Adapters Present
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting(this, "Default.Wifi", "WinWifi");
            //wifiInterfaces = new List<Dictionary<string, string>>();
            for (int i = 0; i < client.Interfaces.Length; i++)
            {
                //use profile name = name of connected network
                OM.Host.DebugMsg("Interface name: " + client.Interfaces[i].InterfaceName);
                //string test1 = client.Interfaces[i].CurrentConnection.ToString();
                try
                {
                    if (client.Interfaces[i].CurrentConnection.isState == Wlan.WlanInterfaceState.Connected)
                    {
                        initConnections.Add(client.Interfaces[i].InterfaceName, client.Interfaces[i].CurrentConnection.profileName);
                    }
                }
                catch { }
                //wifiInterfaces.Add(new Dictionary<string, string>());
                //wifiInterfaces[wifiInterfaces.Count - 1].Add("InterfaceDescription", client.Interfaces[i].InterfaceDescription);
                //wifiInterfaces[wifiInterfaces.Count - 1].Add("InterfaceGUID", client.Interfaces[i].InterfaceGuid.ToString());
                //wifiInterfaces[wifiInterfaces.Count - 1].Add("InterfaceName", client.Interfaces[i].InterfaceName);
                client.Interfaces[i].WlanConnectionNotification += new WlanClient.WlanInterface.WlanConnectionNotificationEventHandler(Wifi_WlanConnectionNotification);
                client.Interfaces[i].WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(Wifi_WlanNotification);
            }

            OM.Host.OnWirelessEvent += new WirelessEvent(Host_OnWirelessEvent);
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            //OpenMobile.Threading.SafeThread.Asynchronous(Updater);
            tmr.Tag = "";
            tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
            tmr.Enabled = true;

            //BuiltInComponents.Host.DataHandler.PushDataSourceValue(this, "Networking.WiFi.Interfaces", Results);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        bool canUseNotifications = false;
        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            switch (function)
            {
                case eFunction.pluginLoadingComplete:
                    canUseNotifications = true;
                    //tmr.Enabled = false;
                    //tmr.Interval = 500;
                    //tmr.Enabled = true;
                    break;
            }
        }

        void Host_OnWirelessEvent(eWirelessEvent type, string arg)
        {
            switch (type)
            {
                case eWirelessEvent.DisconnectedFromWirelessNetwork:

                    break;
            }
        }

        bool initStatusUpdater = false;
        Timer tmr = new Timer(1000);
        void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OpenMobile.Timer tmr = ((OpenMobile.Timer)sender);
            tmr.Enabled = false;
            if (tmr.Tag.ToString() == "Running")
                return; //don't enable again as the end of this method enables the timer again
            tmr.Tag = "Running";
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            for (int i = 0; i < client.Interfaces.Length; i++)
            {
                try
                {
                    client.Interfaces[i].Scan();
                    foreach (connectionInfo c in getAvailableNetworks(client.Interfaces[i].InterfaceName))
                    {
                        results.Add(new Dictionary<string, object>());
                        results[results.Count - 1].Add("IsConnected", c.IsConnected.ToString());
                        if ((!initStatusUpdater) && (initConnections.ContainsKey(client.Interfaces[i].InterfaceName)))
                        {
                            results[results.Count - 1].Add("InterfaceConnectedName", initConnections.Keys.ElementAt(i));
                            if (!InterfacesToConnections.ContainsKey(client.Interfaces[i].InterfaceName))
                            {
                                InterfacesToConnections.Add(client.Interfaces[i].InterfaceName, c);
                                //CheckNotification(client.Interfaces[i].InterfaceName, c, "Connected");
                                string interfaceName = client.Interfaces[i].InterfaceName;
                                connectionInfo cNew = c;
                                OpenMobile.Threading.SafeThread.Asynchronous(delegate() { CheckNotificationDelayed(interfaceName, cNew, "Connected"); });
                                //CheckNotificationDelayed(client.Interfaces[i].InterfaceName, c, "Connected");
                            }
                        }
                        else
                            results[results.Count - 1].Add("InterfaceConnectedName", getInterfaceName(c));
                        results[results.Count - 1].Add("SignalImage", getImage(c));
                        results[results.Count - 1].Add("SignalQuality", c.signalStrength.ToString());
                        results[results.Count - 1].Add("NetworkName", getName(c));
                        results[results.Count - 1].Add("UID", c.UID);
                        results[results.Count - 1].Add("ConnectionType", c.ConnectionType);
                    }
                    //getAvailableNetworks();
                }
                catch { }
            }
            initStatusUpdater = true;
            results = CustomSort(results);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.Networks", results, true);
            tmr.Tag = "";
            tmr.Enabled = false;
            tmr.Interval = 30000;
            tmr.Enabled = true;
        }

        private void Updater()
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            for (int i = 0; i < client.Interfaces.Length; i++)
            {
                try
                {
                    client.Interfaces[i].Scan();
                    foreach (connectionInfo c in getAvailableNetworks(client.Interfaces[i].InterfaceName))
                    {
                        results.Add(new Dictionary<string, object>());
                        results[results.Count - 1].Add("IsConnected", c.IsConnected.ToString());
                        if ((!initStatusUpdater) && (initConnections.ContainsKey(client.Interfaces[i].InterfaceName)))
                        {
                            results[results.Count - 1].Add("InterfaceConnectedName", initConnections[client.Interfaces[i].InterfaceName]);
                            InterfacesToConnections.Add(client.Interfaces[i].InterfaceName, c);
                        }
                        else
                            results[results.Count - 1].Add("InterfaceConnectedName", getInterfaceName(c));
                        results[results.Count - 1].Add("SignalImage", getImage(c));
                        results[results.Count - 1].Add("SignalQuality", c.signalStrength.ToString());
                        results[results.Count - 1].Add("NetworkName", getName(c));
                        results[results.Count - 1].Add("UID", c.UID);
                        results[results.Count - 1].Add("ConnectionType", c.ConnectionType);
                    }
                    //getAvailableNetworks();
                }
                catch { }
            }
            initStatusUpdater = true;
            //results = CustomSort(results);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.Networks", results, true);
            System.Threading.Thread.Sleep(30000);
            OpenMobile.Threading.SafeThread.Asynchronous(Updater);
        }

        private List<Dictionary<string, object>> CustomSort(List<Dictionary<string, object>> results)
        {
            List<Dictionary<string, object>> newResults = new List<Dictionary<string, object>>();
            while (results.Count > 0)
            {
                int highestIndex = 0;
                for (int i = 0; i < results.Count; i++)
                {
                    if (Convert.ToInt32(results[i]["SignalQuality"]) > Convert.ToInt32(results[highestIndex]["SignalQuality"]))
                    {
                        highestIndex = i;
                    }
                }
                newResults.Add(results[highestIndex]);
                results.RemoveAt(highestIndex);
            }
            return newResults;
        }

        private imageItem getImage(connectionInfo c)
        {
            imageItem result = imageItem.NONE;

            if (c.signalStrength > 0)
            {
                result = new imageItem((OImage)OM.Host.getSkinImage("AIcons|10_device_access_network_wifi").image.Copy().Overlay(GetBlendedColor((int)c.signalStrength)));
            }
            else
            {
                OM.Host.DebugMsg("Wifi -> no signal strength higher than 0");
            }
            return result;
        }

        public Color GetBlendedColor(int percentage)
        {
            if (percentage < 50)
                return Interpolate(Color.Red, Color.Yellow, percentage / 50.0);
            return Interpolate(Color.Green, Color.Yellow, (percentage - 50) / 50.0);
        }

        private Color Interpolate(Color color1, Color color2, double fraction)
        {
            double r = Interpolate(color1.R, color2.R, fraction);
            double g = Interpolate(color1.G, color2.G, fraction);
            double b = Interpolate(color1.B, color2.B, fraction);
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        private double Interpolate(double d1, double d2, double fraction)
        {
            return d1 + (d1 - d2) * fraction;
        }

        private string getName(connectionInfo c)
        {
            string name = c.NetworkName;
            if ((String.IsNullOrEmpty(name)) || (name == "\0"))
                name = "Other Network";
            return name;
        }

        private string getInterfaceName(connectionInfo c)
        {
            string result = "";
            if (c.IsConnected)
            {
                for (int i = 0; i < InterfacesToConnections.Count; i++)
                {
                    if(InterfacesToConnections[InterfacesToConnections.Keys.ElementAt(i)] == c)
                    {
                        result = InterfacesToConnections.Keys.ElementAt(i);
                        break;
                    }
                }
            }
            return result;
        }

        private void RefreshWifiInternal()
        {
            tmr.Enabled = false;
            tmr.Interval = 500;
            tmr.Enabled = true;
        }

        private object RefreshWifi(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            tmr.Enabled = false;
            tmr.Interval = 500;
            tmr.Enabled = true;
            return null;
        }

        Dictionary<string, connectionInfo> InterfacesToConnections = new Dictionary<string, connectionInfo>();
        private object ConnectToWiFi(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            //param 0 = screen
            int screen = Convert.ToInt32(param[0]);
            string UID = param[1].ToString();
            string interfaceNameToConnect = client.Interfaces[0].InterfaceName;
            if (client.Interfaces.Length > 1)
            {
                //option to pick interface
                MenuPopup PopupMenu = new MenuPopup("WiFiInterface", MenuPopup.ReturnTypes.Index);
                PopupMenu.ReturnType = MenuPopup.ReturnTypes.SelectedItem;
                for (int i = 0; i < client.Interfaces.Length; i++)
                {
                    OMListItem ListItem;
                    PopupMenu.FontSize = 15;
                    try
                    {
                        ListItem = new OMListItem(client.Interfaces[i].InterfaceName, client.Interfaces[i].CurrentConnection.isState.ToString(), client.Interfaces[i].InterfaceGuid as object);
                        
                    }
                    catch
                    {
                        ListItem = new OMListItem(client.Interfaces[i].InterfaceName, client.Interfaces[i].InterfaceGuid as object);
                    }
                    //ListItem.image = OImage.FromFont(50, 50, "+", new Font(Font.Arial, 40F), eTextFormat.Outline, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinTextColor);
                    PopupMenu.AddMenuItem(ListItem);
                }
                object strSelected = PopupMenu.ShowMenu(screen);
                if (strSelected == null)
                    return null;
                OM.Host.DebugMsg("Wifi popup interface selection: " + strSelected.ToString());
                interfaceNameToConnect = strSelected.ToString();

                //int Selection = (int)PopupMenu.ShowMenu(screen);
                //if (Selection == -1)
                //    return null;
            }
            connectionInfo info;
            info = Array.Find<connectionInfo>(getAvailableNetworks(), p => p.UID == UID);
            if (info != null)
            {
                OM.Host.DebugMsg("Wifi -> Connecting to " + info.NetworkName + " [ " + interfaceNameToConnect + " ]");
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.StatusChange", info.NetworkName + ".Connecting" + "." + interfaceNameToConnect, true);
                CheckNotification(interfaceNameToConnect, info, "Connecting");
                
                string credentials = "";
                switch (info.requiresPassword)
                {
                    case ePassType.UserPass:
                    case ePassType.UserPassDomain:
                    case ePassType.SharedKey:
                        string password = OSK.ShowDefaultOSK(screen, "", "Type something", "Please input something now", OSKInputTypes.Keypad, true);
                        if (password == null)
                            return null;
                        credentials = password;
                        break;
                }
                info.Credentials = credentials;
                if (connect(info, interfaceNameToConnect))
                {
                    OM.Host.DebugMsg("Wifi -> Connected to " + info.NetworkName + " [ " + interfaceNameToConnect + " ]");
                    bool found = false;
                    for (int i = 0; i < InterfacesToConnections.Count; i++)
                    {
                        if (InterfacesToConnections.Keys.ElementAt(i) == interfaceNameToConnect)
                        {
                            InterfacesToConnections[interfaceNameToConnect] = info;
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        InterfacesToConnections.Add(interfaceNameToConnect, info);
                    }
                    //status change = connected
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.StatusChange", info.NetworkName + ".Connected" + "." + interfaceNameToConnect, true);
                    CheckNotification(interfaceNameToConnect, info, "Connected");
                }
                else
                {
                    //could not connect
                    //InterfacesToConnections.Remove(interfaceNameToConnect);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.StatusChange", info.NetworkName + ".NotConnected" + "." + interfaceNameToConnect, true);
                    CheckNotification(interfaceNameToConnect, info, "NotConnected");
                }
                RefreshWifiInternal();
            }
            return null;
        }

        private void CheckNotificationDelayed(string interfaceName, connectionInfo c, string Action)
        {
            while (true)
            {
                if (canUseNotifications)
                {
                    CheckNotification(interfaceName, c, Action);
                    break;
                }
                System.Threading.Thread.Sleep(500);
            }
        }

        private void RemoveAllNotifications()
        {
            OM.Host.DebugMsg("Wifi notification (Removing All)");
            OM.Host.UIHandler.RemoveAllMyNotifications(this);
        }

        private void CheckNotification(string interfaceName, connectionInfo c, string Action)
        {
            if (!canUseNotifications)
                return;
            bool notificationFound = false;
            for (int i = 0; i < wifiNotifications.Count; i++)
            {
                if (wifiNotifications[i].Tag.ToString() == c.NetworkName + "." + c.UID)
                {
                    notificationFound = true;
                    if ((Action == "NotConnected") || (Action == "Disconnected"))
                    {
                        //remove this notification as we couldn't connect
                        OM.Host.DebugMsg("Wifi notification (Removing): " + Action);
                        OM.Host.UIHandler.RemoveNotification(wifiNotifications[i], true);
                        wifiNotifications.RemoveAt(i);
                        //i--;
                        break;
                    }
                    else
                    {
                        
                        //change this notification to match
                        OM.Host.DebugMsg("Wifi notification (Modifying): " + Action);
                        OImage testImage = new OImage(Color.Transparent, 100, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height);
                        Font f = Font.Arial;
                        f.Size = 7;
                        testImage.image = getImage(c).image.image;
                        testImage.Refresh();
                        //testImage.image = OM.Host.getSkinImage("WiFiConnecting.gif").image.image;

                        testImage.RenderText(0, 0, testImage.Width, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height, Action + Environment.NewLine + c.NetworkName, f, eTextFormat.Normal, Alignment.WordWrap, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinFocusColor, FitModes.None);
                        wifiNotifications[i].IconStatusBar.Dispose();
                        wifiNotifications[i].IconStatusBar = testImage;
                        //wifiNotifications[i].Icon.Dispose();
                        //wifiNotifications[i].Icon = testImage;
                        wifiNotifications[wifiNotifications.Count - 1].IconSize_Width = 100;
                        break;
                    }
                    
                }
            }
            if (!notificationFound)
            {
                //add notification
                OM.Host.DebugMsg("Wifi notification (Adding): " + Action);
                OImage testImage = new OImage(Color.Transparent, 100, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height);
                Font f = Font.Arial;
                f.Size = 7;
                testImage.image = getImage(c).image.image;
                
                testImage.Refresh();
                //testImage.image = OM.Host.getSkinImage("WiFiConnecting.gif").image.image;

                testImage.RenderText(0, 0, testImage.Width, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height, Action + Environment.NewLine + c.NetworkName, f, eTextFormat.Normal, Alignment.WordWrap, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinFocusColor, FitModes.None);
                wifiNotifications.Add(new Notification(Notification.Styles.IconOnly, this, c.UID, imageItem.NONE.image));
                wifiNotifications[wifiNotifications.Count - 1].Tag = c.NetworkName + "." + c.UID;
                wifiNotifications[wifiNotifications.Count - 1].IconStatusBar = testImage;
                //wifiNotifications[wifiNotifications.Count - 1].Icon = testImage;
                wifiNotifications[wifiNotifications.Count - 1].IconSize_Width = 100;
                OM.Host.UIHandler.AddNotification(wifiNotifications[wifiNotifications.Count - 1]);
            }
        }

        private object DisconnectFromWiFi(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param.Length > 0)
            {
                for (int i = 0; i < param.Length; i++)
                {
                    //OM.Host.execute(eFunction.disconnectFromInternet, param[0].ToString());
                    string UID = param[i].ToString();
                    connectionInfo info;
                    info = Array.Find<connectionInfo>(getAvailableNetworks(), p => p.UID == UID);
                    if (info == null)
                        return null;
                    OM.Host.DebugMsg("Wifi -> disconnect from " + info.NetworkName);
                    disconnect(info);
                    for (int j = 0; j < InterfacesToConnections.Count; j++)
                    {
                        int test1 = info.GetHashCode();
                        int test2 = InterfacesToConnections[InterfacesToConnections.Keys.ElementAt(j)].GetHashCode();
                        if (InterfacesToConnections[InterfacesToConnections.Keys.ElementAt(j)].NetworkName == info.NetworkName)
                        {
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.StatusChange", info.NetworkName + ".Disconnected" + "." + InterfacesToConnections.Keys.ElementAt(j), true);
                            CheckNotification(InterfacesToConnections.Keys.ElementAt(j), info, "Disconnected");
                            InterfacesToConnections.Remove(InterfacesToConnections.Keys.ElementAt(j));
                            break;
                        }
                    }
                    //status change disconnected

                }
            }
            else
            {
                //OM.Host.execute(eFunction.disconnectFromInternet);
                //remove all the wifi notifications
                for (int i = 0; i < client.Interfaces.Length; i++)
                {
                    client.Interfaces[i].Disconnect();
                    //status change disconnected
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("WinWifi;Networking.WiFi.StatusChange", "All.DisConnected" + "." + InterfacesToConnections.Keys.ElementAt(i), true);
                }
                InterfacesToConnections.Clear();
                RemoveAllNotifications();
            }
            RefreshWifiInternal();
            return null;
        }

        //private object WiFiInterfaces(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        //{
        //    result = true;
        //    return wifiInterfaces;
        //}

        private void raiseWirelessEvent(eWirelessEvent e, string arg)
        {
            if (OnWirelessEvent != null)
                OnWirelessEvent(e, arg);
        }
        void Wifi_WlanNotification(Wlan.WlanNotificationData notifyData)
        {
            try
            {
                for (int i = 0; i < client.Interfaces.Length; i++)
                {
                    if ((client.Interfaces[i].InterfaceGuid == notifyData.interfaceGuid) && (client.Interfaces[i].InterfaceState != Wlan.WlanInterfaceState.NotReady))
                    {
                        if (notifyData.NotificationCode.ToString() == "NetworkAvailable")
                            raiseWirelessEvent(eWirelessEvent.WirelessNetworksAvailable, "");
                        else
                        {
                            if ((client.Interfaces[i].InterfaceGuid == notifyData.interfaceGuid) && (notifyData.NotificationCode.ToString() == "SignalQualityChange"))
                            {
                                raiseWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, client.Interfaces[i].CurrentConnection.profileName + "." + client.Interfaces[i].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
                            }
                        }
                    }
                }
                //if (client.Interfaces.Length == 0)
                //    return;
                //if (client.Interfaces[0].InterfaceState == Wlan.WlanInterfaceState.NotReady)
                //    return;
                //if (notifyData.NotificationCode.ToString() == "NetworkAvailable")
                //    raiseWirelessEvent(eWirelessEvent.WirelessNetworksAvailable, "");
                //else if (notifyData.NotificationCode.ToString() == "SignalQualityChange")
                //{
                //    raiseWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, client.Interfaces[0].CurrentConnection.profileName + "." + client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
                //    //raiseWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
                //}
            }
            catch (Win32Exception) { }
            catch (Exception e)
            {
                if (theHost != null)
                    theHost.sendMessage("SandboxedThread", "WinWifi", "", ref e);
            }
        }
        void Wifi_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            try
            {
                if (notifyData.NotificationCode.ToString() == "ConnectionStart")
                    raiseWirelessEvent(eWirelessEvent.ConnectingToWirelessNetwork, connNotifyData.profileName);
                else if (notifyData.NotificationCode.ToString() == "Connected")
                {
                    raiseWirelessEvent(eWirelessEvent.ConnectedToWirelessNetwork, connNotifyData.profileName + "." + client.Interfaces[0].CurrentConnection.wlanAssociationAttributes.wlanSignalQuality.ToString());
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
        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("AIcons|10_device_access_network_wifi"); }
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