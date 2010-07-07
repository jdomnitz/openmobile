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
using OpenMobile.Controls;
using System.Drawing;
using OpenMobile.Framework;
using System.Collections.Generic;

namespace Networking
{
    public sealed class Class1:IHighLevel
    {
        IPluginHost theHost;
        #region IHighLevel Members

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen];
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }
        ScreenManager manager;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            host.OnWirelessEvent += new WirelessEvent(host_OnWirelessEvent);
            OMPanel p = new OMPanel("");
            OMBasicShape border=new OMBasicShape(20,110,620,410);
            border.CornerRadius=15F;
            border.Shape=shapes.RoundedRectangle;
            border.BorderSize = 4F;
            border.BorderColor=Color.Silver;
            border.FillColor=Color.Black;
            OMBasicShape border2 = new OMBasicShape(660, 110, 320, 410);
            border2.CornerRadius = 15F;
            border2.Shape = shapes.RoundedRectangle;
            border2.BorderSize = 4F;
            border2.BorderColor = Color.Silver;
            border2.FillColor = Color.Black;
            OMButton connect = new OMButton(680, 300, 280, 50);
            connect.Text = "Connect";
            connect.Image = imageItem.MISSING;
            connect.OnClick += new userInteraction(connect_OnClick);
            OMList networks = new OMList(22, 121, 616, 390);
            networks.ListStyle = eListStyle.MultiList;
            networks.ListItemHeight = 80;
            networks.Font = new Font(FontFamily.GenericSansSerif, 30F);
            networks.OnLongClick += new userInteraction(networks_OnLongClick);
            networks.OnClick += new userInteraction(networks_OnClick);
            networks.ItemColor1 = Color.Transparent;
            networks.SelectedItemColor1 = Color.Blue;
            networks.HighlightColor = Color.White;
            OMImage signalStrength = new OMImage(670, 120, 75, 75);
            OMLabel networkName = new OMLabel(745, 120, 200, 50);
            networkName.Format = eTextFormat.BoldShadow;
            OMLabel networkType = new OMLabel(660, 190, 320, 50);
            p.addControl(border);
            p.addControl(networks);
            p.addControl(border2);
            p.addControl(signalStrength);
            p.addControl(networkName);
            p.addControl(networkType);
            p.addControl(connect);
            manager.loadPanel(p);
            OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(UpdateList), ePriority.MediumLow, "Refresh Networks");
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void connect_OnClick(OMControl sender, int screen)
        {
            OMList list = (OMList)sender.Parent[1];
            if (list.SelectedItem != null)
            {
                theHost.execute(eFunction.connectToInternet, list.SelectedItem.tag.ToString());
            }
        }

        void networks_OnClick(OMControl sender, int screen)
        {
            OMList list=(OMList)sender;
            connectionInfo info = networks.Find(p => p.UID == list.SelectedItem.tag.ToString());
            ((OMImage)manager[screen][3]).Image = new imageItem(list.SelectedItem.image);
            ((OMLabel)manager[screen][4]).Text = list.SelectedItem.text;
            ((OMLabel)manager[screen][5]).Text = info.ConnectionType.Replace('_',' ');
        }

        void networks_OnLongClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.connectToInternet, ((OMList)sender).SelectedItem.tag.ToString());
        }
        static List<connectionInfo> networks;
        private void UpdateList()
        {
            object o;
            theHost.getData(eGetData.GetAvailableNetworks, "", out o);
            if (o==null)
                return;
            for (int i = 0; i < theHost.ScreenCount; i++)
                ((OMList)manager[i][1]).Clear();
            networks=(List<connectionInfo>)o;
            foreach(connectionInfo c in networks)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                    ((OMList)manager[i][1]).Add(getListItem(c));
            }
        }

        private OMListItem getListItem(connectionInfo c)
        {
            Image icon;
            if (c.signalStrength>=75)
                icon = theHost.getSkinImage("Wifi3").image;
            else if (c.signalStrength >= 50)
                icon = theHost.getSkinImage("Wifi2").image;
            else if (c.signalStrength >= 25)
                icon = theHost.getSkinImage("Wifi1").image;
            else
                icon = theHost.getSkinImage("Wifi0").image;
            OMListItem.subItemFormat format = new OMListItem.subItemFormat();
            format.color = Color.FromArgb(100, Color.White);
            string name = c.NetworkName;
            if (name == "")
                name = "Other Network";
            OMListItem ret= new OMListItem(name, c.ConnectionType.Replace('_',' '),icon,format);
            ret.tag = c.UID;
            return ret;
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
                    OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(UpdateList), ePriority.Normal, "Refresh Networks");
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
                    OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(UpdateList), ePriority.Normal, "Refresh Networks");
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
                theHost.execute(eFunction.TransitionFromAny, source.Substring(2));
                theHost.execute(eFunction.TransitionToPanel, source.Substring(2), "Networking");
                theHost.execute(eFunction.ExecuteTransition, source.Substring(2), "None");
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
