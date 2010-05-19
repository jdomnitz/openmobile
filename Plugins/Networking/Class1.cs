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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Media;
using System.IO;

namespace Networking
{
    public sealed class Class1:IHighLevel
    {
        IPluginHost theHost;
        #region IHighLevel Members

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return null;
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }
        
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnWirelessEvent += new WirelessEvent(host_OnWirelessEvent);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        IconManager.UIIcon icon;
        void host_OnWirelessEvent(OpenMobile.eWirelessEvent type, string arg)
        {
            switch(type)
            {
                case eWirelessEvent.WirelessNetworksAvailable:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    icon=new IconManager.UIIcon(theHost.getSkinImage("WifiNew").image,ePriority.Normal,false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    return;
                case eWirelessEvent.ConnectingToWirelessNetwork:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    icon = new IconManager.UIIcon(theHost.getSkinImage("WifiConnecting").image, ePriority.Normal, false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    return;
                case eWirelessEvent.WirelessSignalStrengthChanged:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    int strength = int.Parse(arg);
                    if (strength>=75)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("Wifi3").image, ePriority.Normal, false, "Networking");
                    else if (strength >= 50)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("Wifi2").image, ePriority.Normal, false, "Networking");
                    else if (strength >= 25)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("Wifi1").image, ePriority.Normal, false, "Networking");
                    else
                        icon = new IconManager.UIIcon(theHost.getSkinImage("Wifi0").image, ePriority.Normal, false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    return;
                case eWirelessEvent.DisconnectedFromWirelessNetwork:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    return;
            }
        }

        public string displayName
        {
            get { return "Networking"; }
        }

        #endregion

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
            get { return "Networking"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Networking"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            if (message == "IconClicked")
            {
                IconManager.UIIcon icon = data as IconManager.UIIcon;
                if (icon.image == theHost.getSkinImage("WifiNew").image)
                    theHost.execute(eFunction.connectToInternet);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
