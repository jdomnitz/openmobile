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
using OpenMobile.Graphics;
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
            return manager[screen,name];
        }

        public Settings loadSettings()
        {
            return null;
        }
        ScreenManager manager;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            host.OnWirelessEvent += new WirelessEvent(host_OnWirelessEvent);
            OMPanel p = new OMPanel("");
            OMBasicShape border=new OMBasicShape(20,110,620,410);
            border.CornerRadius=15;
            border.Shape=shapes.RoundedRectangle;
            border.BorderSize = 4F;
            border.BorderColor=Color.Silver;
            border.FillColor=Color.Black;
            OMBasicShape border2 = new OMBasicShape(660, 110, 320, 410);
            border2.CornerRadius = 15;
            border2.Shape = shapes.RoundedRectangle;
            border2.BorderSize = 4F;
            border2.BorderColor = Color.Silver;
            border2.FillColor = Color.Black;
            OMButton connect = new OMButton(680, 300, 280, 50);
            connect.Text = "Scan";
            connect.Image = imageItem.MISSING;
            connect.OnClick += new userInteraction(connect_OnClick);
            OMList networks = new OMList(22, 121, 616, 390);
            networks.ListStyle = eListStyle.MultiList;
            networks.ListItemHeight = 80;
            networks.Font = new Font(Font.GenericSansSerif, 30F);
            networks.OnLongClick += new userInteraction(networks_OnLongClick);
            networks.OnClick += new userInteraction(networks_OnClick);
            networks.ItemColor1 = Color.Transparent;
            networks.SelectedItemColor1 = Color.Blue;
            networks.HighlightColor = Color.White;
            OMImage signalStrength = new OMImage(670, 120, 75, 75);
            OMLabel networkName = new OMLabel(745, 120, 200, 50);
            networkName.Format = OpenMobile.Graphics.eTextFormat.BoldShadow;
            OMLabel networkType = new OMLabel(660, 190, 320, 50);
            OMBasicShape background = new OMBasicShape(0, 0, 1000, 600);
            background.FillColor = Color.FromArgb(140, Color.Black);
            OMBasicShape password = new OMBasicShape(300, 200, 400, 175);
            password.FillColor = Color.Black;
            password.CornerRadius = 10;
            password.BorderSize = 4F;
            password.BorderColor = Color.Red;
            password.Shape = shapes.RoundedRectangle;
            OMLabel caption = new OMLabel(300, 200, 400, 50);
            caption.Text = "Enter a Password To Connect";
            caption.Color = Color.White;
            caption.Font = new Font(Font.GenericSansSerif, 20F);
            caption.OutlineColor = Color.Red;
            caption.Format = eTextFormat.Glow;
            OMTextBox textbox = new OMTextBox(310, 250, 380, 50);
            textbox.OutlineColor = Color.Red;
            textbox.OnClick += new userInteraction(textbox_OnClick);
            OMButton login = new OMButton(310, 310, 380, 50);
            login.Text = "Login";
            login.OnClick += new userInteraction(login_OnClick);
            login.Image = imageItem.MISSING;
            p.addControl(border);
            p.addControl(networks);
            p.addControl(border2);
            p.addControl(signalStrength);
            p.addControl(networkName);
            p.addControl(networkType);
            p.addControl(connect);
            OMPanel prompt = new OMPanel("Prompt");
            prompt.Forgotten = true;
            prompt.addControl(background);
            prompt.addControl(password);
            prompt.addControl(caption);
            prompt.addControl(textbox);
            prompt.addControl(login);
            manager.loadPanel(p);
            manager.loadPanel(prompt);
            OpenMobile.Threading.TaskManager.QueueTask(UpdateList, ePriority.Normal, "Refresh Networks");
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void login_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "Networking", "Prompt");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            OMList list = (OMList)manager[screen][1];
            if (list.SelectedItem == null)
                return;
            connectionInfo info = networks.Find(p => p.UID == list.SelectedItem.tag.ToString());
            if (info == null)
                return;
            OMTextBox tb = (OMTextBox)sender.Parent[3];
            theHost.execute(eFunction.connectToInternet, list.SelectedItem.tag.ToString(), tb.Text);
        }

        void textbox_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text= input.getText(screen, "Networking",new string[]{"","Prompt"});
        }

        void connect_OnClick(OMControl sender, int screen)
        {
            OMList list = (OMList)sender.Parent[1];
            if (list.SelectedItem != null)
            {
                switch (((OMButton)sender).Text)
                {
                    case "Connect":
                        connectionInfo info = networks.Find(p => p.UID == list.SelectedItem.tag.ToString());
                        if (info==null)
                            return;
                        switch(info.requiresPassword)
                        {
                            case ePassType.UserPass: //TODO
                            case ePassType.UserPassDomain:
                            case ePassType.SharedKey:
                                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Networking", "Prompt");
                                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                                return;
                            case ePassType.None:
                                theHost.execute(eFunction.connectToInternet, list.SelectedItem.tag.ToString());
                                return;
                        }
                        break;
                    case "Disconnect":
                        theHost.execute(eFunction.disconnectFromInternet, list.SelectedItem.tag.ToString());
                        return;
                    case "Scan":
                        theHost.execute(eFunction.refreshData, "WinWifi"); //TODO: Make this generic
                        UpdateList();
                        return;
                }
            }
            else
            {
                if (((OMButton)sender).Text == "Scan")
                {
                    theHost.execute(eFunction.refreshData, "WinWifi");
                    UpdateList();
                }
            }
        }

        void networks_OnClick(OMControl sender, int screen)
        {
            OMList list=(OMList)sender;
            connectionInfo info = networks.Find(p => p.UID == list.SelectedItem.tag.ToString());
            ((OMImage)manager[screen][3]).Image = new imageItem(list.SelectedItem.image);
            ((OMLabel)manager[screen][4]).Text = list.SelectedItem.text;
            ((OMLabel)manager[screen][5]).Text = info.ConnectionType;
            if (info.IsConnected)
                ((OMButton)manager[screen][6]).Text = "Disconnect";
            else
                ((OMButton)manager[screen][6]).Text = "Connect";
        }

        void networks_OnLongClick(OMControl sender, int screen)
        {
            connect_OnClick(sender.Parent[6], screen);
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
                {
                    if (c.IsConnected)
                        host_OnWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged, c.signalStrength.ToString());
                    OMList list=((OMList)manager[i][1]);
                    list.Add(getListItem(c));
                    if ((((OMLabel)manager[i][4]).Text == c.NetworkName) && (((OMLabel)manager[i][5]).Text == c.ConnectionType))
                    {
                        list.SelectedIndex = list.Count - 1;
                        if (c.IsConnected)
                            ((OMButton)manager[i][6]).Text = "Disconnect";
                        else
                            ((OMButton)manager[i][6]).Text = "Connect";
                    }
                }
            }
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                if (((OMList)manager[i][1]).SelectedIndex == -1)
                {
                    ((OMImage)manager[i][3]).Image = imageItem.NONE;
                    ((OMLabel)manager[i][4]).Text = "";
                    ((OMLabel)manager[i][5]).Text = "";
                    ((OMButton)manager[i][6]).Text = "Scan";
                }
            }
        }

        private OMListItem getListItem(connectionInfo c)
        {
            OImage icon;
            if (c.signalStrength>=75)
                icon = theHost.getSkinImage("WiFi3").image;
            else if (c.signalStrength >= 50)
                icon = theHost.getSkinImage("WiFi2").image;
            else if (c.signalStrength >= 25)
                icon = theHost.getSkinImage("WiFi1").image;
            else if(c.signalStrength>0)
                icon = theHost.getSkinImage("WiFi0").image;
            else
                icon = theHost.getSkinImage("Fixed").image;
            OMListItem.subItemFormat format = new OMListItem.subItemFormat();
            format.color = Color.FromArgb(100, Color.White);
            string name = c.NetworkName;
            if ((string.IsNullOrEmpty(name))||(name=="\0"))
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
                    icon=new IconManager.UIIcon(theHost.getSkinImage("WiFiNew").image,ePriority.Normal,false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    OpenMobile.Threading.TaskManager.QueueTask(UpdateList, ePriority.Normal, "Refresh Networks");
                    return;
                case eWirelessEvent.ConnectingToWirelessNetwork:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    icon = new IconManager.UIIcon(theHost.getSkinImage("WiFiConnecting").image, ePriority.Normal, false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    return;
                case eWirelessEvent.ConnectedToWirelessNetwork:
                    OpenMobile.Threading.TaskManager.QueueTask(UpdateList, ePriority.Normal, "Refresh Networks");
                    return;
                case eWirelessEvent.WirelessSignalStrengthChanged:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    int strength = int.Parse(arg);
                    if (strength>=75)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("WiFi3").image, ePriority.Normal, false, "Networking");
                    else if (strength >= 50)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("WiFi2").image, ePriority.Normal, false, "Networking");
                    else if (strength >= 25)
                        icon = new IconManager.UIIcon(theHost.getSkinImage("WiFi1").image, ePriority.Normal, false, "Networking");
                    else
                        icon = new IconManager.UIIcon(theHost.getSkinImage("WiFi0").image, ePriority.Normal, false, "Networking");
                    theHost.sendMessage("UI", "Networking", "AddIcon", ref icon);
                    for(int i=0;i<networks.Count;i++)
                        if (networks[i].IsConnected)
                        {
                            networks[i].signalStrength = (uint)strength;
                            for(int k=0;k<theHost.ScreenCount;k++)
                            {
                                OMList itm=((OMList)manager[k][1]);
                                for (int l = 0; l < itm.Count; l++)
                                    if (itm[l].tag.ToString() == networks[i].UID)
                                        itm[l] = getListItem(networks[i]);
                            }
                        }
                    return;
                case eWirelessEvent.DisconnectedFromWirelessNetwork:
                    theHost.sendMessage("UI", "Networking", "RemoveIcon", ref icon);
                    OpenMobile.Threading.TaskManager.QueueTask(UpdateList, ePriority.Normal, "Refresh Networks");
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
                string[] parts = source.Split(new char[] { '|' });
                if (parts.Length == 2)
                {
                    theHost.execute(eFunction.TransitionFromAny, parts[1]);
                    theHost.execute(eFunction.TransitionToPanel, parts[1], "Networking");
                    theHost.execute(eFunction.ExecuteTransition, parts[1], "None");
                    return true;
                }
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
