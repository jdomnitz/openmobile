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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;

namespace OMSettings
{
    [SkinIcon("*@")] //*'")]
    public class Settings : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            manager = new ScreenManager(host.ScreenCount);

            OMLabel Label = new OMLabel(383, 160, 450, 100);
            Label.Font = new Font(Font.GenericSansSerif, 36F);
            Label.Text = "Audio Zone";
            Label.Name = "Label";

            OMButton Cancel = new OMButton(14, 255, 200, 110);
            Cancel.Image = theHost.getSkinImage("Full");
            Cancel.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Cancel.Text = "Cancel";
            Cancel.Name = "Media.Cancel";
            Cancel.Transition = eButtonTransition.None;
            Cancel.OnClick += new userInteraction(Cancel_OnClick);

            #region menu
            OMPanel main = new OMPanel("Main");
            main.BackgroundColor1 = Color.Black;
            main.BackgroundType = backgroundStyle.SolidColor;
            OMList menu = new OMList(10, 100, 980, 433);
            menu.ListStyle = eListStyle.MultiListText;
            menu.Background = Color.Silver;
            menu.ItemColor1 = Color.Black;
            menu.Font = new Font(Font.GenericSansSerif, 30F);
            menu.Color = Color.White;
            menu.HighlightColor = Color.White;
            menu.SelectedItemColor1 = Color.DarkBlue;
            menu.SoftEdgeData.Color1 = Color.Black;
            menu.SoftEdgeData.Sides[0] = true;
            menu.SoftEdgeData.Sides[1] = false;
            menu.SoftEdgeData.Sides[2] = true;
            menu.SoftEdgeData.Sides[3] = false;
            menu.UseSoftEdges = true;
            OMListItem.subItemFormat format=new OMListItem.subItemFormat();
            format.color=Color.FromArgb(128,Color.White);
            format.font = new Font(Font.GenericSansSerif, 21F);
            menu.Add(new OMListItem("General Settings", "User Interface and System Settings", format, "General Settings"));
            menu.Add(new OMListItem("Personal Settings", "Usernames and Passwords", format, "personal"));
            menu.Add(new OMListItem("Data Settings", "Settings for each of the Data Providers", format, "data"));
            menu.Add(new OMListItem("Input routing", "Routing of data between a input device and a screen", format, "InputRouterScreenSelection"));
            menu.Add(new OMListItem("Multi-Zone Settings", "Displays, Sound Cards and other zone specific settings", format, "Zones"));
            menu.Add(new OMListItem("Plugin Settings", "Settings for all low level plugins", format, "Plugins"));
            menu.OnClick += new userInteraction(menu_OnClick);
            main.addControl(menu);
            manager.loadPanel(main);
            #endregion
            #region personal
            OMPanel personal = new OMPanel("personal");
            OMButton Save2 = new OMButton(13, 136, 200, 110);
            Save2.Image = theHost.getSkinImage("Full");
            Save2.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Save2.Text = "Save";
            Save2.Name = "Media.Save";
            Save2.OnClick += new userInteraction(Save2_OnClick);
            Save2.Transition = eButtonTransition.None;
            OMLabel Heading = new OMLabel(300, 80, 600, 100);
            Heading.Font = new Font(Font.GenericSansSerif, 36F);
            Heading.Text = "Google Account Info";
            Label.Name = "Label";
            OMLabel udesc = new OMLabel(220, 180, 200, 50);
            udesc.Text = "Username:";
            udesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMLabel pdesc = new OMLabel(220, 240, 200, 50);
            pdesc.Text = "Password:";
            pdesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMTextBox user = new OMTextBox(450, 180, 500, 50);
            user.Font = new Font(Font.GenericSansSerif, 28F);
            user.TextAlignment = Alignment.CenterLeft;
            user.OnClick += new userInteraction(user_OnClick);
            OMTextBox pass = new OMTextBox(450, 240, 500, 50);
            pass.Font = new Font(Font.GenericSansSerif, 28F);
            pass.Flags = textboxFlags.Password;
            pass.OnClick += new userInteraction(user_OnClick);
            pass.TextAlignment = Alignment.CenterLeft;
            personal.addControl(Save2);
            personal.addControl(Cancel);
            personal.addControl(Heading);
            personal.addControl(udesc);
            personal.addControl(pdesc);
            personal.addControl(user);
            personal.addControl(pass);
            manager.loadPanel(personal);
            #endregion
            #region general
            LayoutManager generalLayout = new LayoutManager();
            OMPanel[] general = generalLayout.layout(theHost, BuiltInComponents.GlobalSettings(host));
            manager.loadPanel(general);
            #endregion
            #region data
            OMPanel data = new OMPanel("data");
            OMButton Save4 = new OMButton(13, 136, 200, 110);
            Save4.Image = theHost.getSkinImage("Full");
            Save4.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Save4.Text = "Save";
            Save4.Name = "Media.Save";
            Save4.OnClick += new userInteraction(Save4_OnClick);
            Save4.Transition = eButtonTransition.None;
            OMLabel Heading3 = new OMLabel(300, 80, 600, 100);
            Heading3.Font = new Font(Font.GenericSansSerif, 36F);
            Heading3.Text = "Data Provider Settings";
            Label.Name = "Label";
            OMLabel ldesc = new OMLabel(210, 180, 280, 50);
            ldesc.Text = "Default Location:";
            ldesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMTextBox location = new OMTextBox(525, 180, 450, 50);
            location.Font = new Font(Font.GenericSansSerif, 28F);
            location.TextAlignment = Alignment.CenterLeft;
            location.OnClick += new userInteraction(location_OnClick);
            OMLabel explanation = new OMLabel(525,230,450,30);
            explanation.Text = "Postcode, city and state, etc.";
            OMList providers = new OMList(275, 280, 650, 240);
            providers.ListStyle = eListStyle.MultiList;
            providers.Background = Color.Transparent;
            providers.ItemColor1 = Color.Transparent;
            providers.SelectedItemColor1 = Color.Blue;
            providers.HighlightColor = Color.White;
            providers.ListItemOffset = 80;
            OMBasicShape backdrop = new OMBasicShape(270, 275, 660, 250);
            backdrop.Shape = shapes.RoundedRectangle;
            backdrop.BorderColor = Color.WhiteSmoke;
            backdrop.FillColor = Color.Black;
            backdrop.CornerRadius = 10;
            backdrop.BorderSize = 2;
            data.addControl(Save4);
            data.addControl(Cancel);
            data.addControl(Heading3);
            data.addControl(ldesc);
            data.addControl(location);
            data.addControl(explanation);
            data.addControl(backdrop);
            data.addControl(providers);
            manager.loadPanel(data);
            #endregion
            #region Plugins
            OMPanel hardware = new OMPanel("Plugins");
            hardware.BackgroundColor1 = Color.Black;
            hardware.BackgroundType = backgroundStyle.SolidColor;
            OMList lstplugins = new OMList(10, 100, 980, 433);
            lstplugins.ListStyle = eListStyle.MultiListText;
            lstplugins.Background = Color.Silver;
            lstplugins.ItemColor1 = Color.Black;
            lstplugins.Font = new Font(Font.GenericSansSerif, 30F);
            lstplugins.Color = Color.White;
            lstplugins.HighlightColor = Color.White;
            lstplugins.SelectedItemColor1 = Color.DarkBlue;
            lstplugins.OnClick += new userInteraction(lstplugins_OnClick);
            lstplugins.Add(new OMListItem("Loading . . .","",format));
            lstplugins.Scrollbars = true;
            hardware.addControl(lstplugins);
            manager.loadPanel(hardware);
            #endregion

            // Load input router panels
            OMSettings.InputRouterScreens.Initialize(this.pluginName, manager, theHost);

            // Load zones panels
            OMSettings.ZoneSettings.Initialize(this.pluginName, manager, theHost);

            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        private void loadPluginSettings()
        {
            List<IBasePlugin> plugins;
            object o;
            theHost.getData(eGetData.GetPlugins, "", out o);
            if (o == null)
                return;
            plugins = (List<IBasePlugin>)o;
            List<Exception> problems = new List<Exception>();
            OMListItem.subItemFormat format = new OMListItem.subItemFormat();
            format.color = Color.FromArgb(140, Color.White);
            OMList[] lstplugins = new OMList[theHost.ScreenCount];
            //Get a reference to the list of plugins on each screen
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                lstplugins[i] = (OMList)manager[i, "Plugins"][0];
                lstplugins[i].Clear();
            }
            //now load the settings for each plugin and add a menu item to the plugin lists
            foreach (IBasePlugin b in plugins)
            {
                LayoutManager lm = new LayoutManager();
                OMPanel[] panel;
                try
                {
                    panel = lm.layout(theHost, b.loadSettings());
                }
                //only allow a not implemented exception or null as a result
                //anything else is an error
                catch (NotImplementedException) { continue; }
                catch (Exception e)
                {
                    problems.Add(e);
                    continue;
                }
                if (panel != null)
                {
                    OMListItem item = new OMListItem(b.pluginName + " Settings", b.pluginDescription, format);
                    for (int i = 0; i < panel.Length; i++)
                    {
                        lstplugins[i].Add(item);
                        panel[i].Name = item.text;
                    }
                    manager.loadPanel(panel);
                }
            }
            foreach (Exception e in problems)
            {
                Exception ex = e;
                theHost.sendMessage("SandboxedThread", "OMSettings", "", ref ex);
            }
            for (int i = 0; i < theHost.ScreenCount; i++)
                lstplugins[i].Sort();
        }

        void lstplugins_OnClick(OMControl sender, int screen)
        {
            OMList lst=(OMList)sender;
            if (lst.SelectedItem.text == "Loading . . .")
                return;
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"OMSettings",lst.SelectedItem.text)==false)
                return;
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(),"OMSettings","Plugins");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"SlideLeft");
        }

        void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.dataUpdated)
                loadProviders();
            if (function==eFunction.pluginLoadingComplete)
                OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(loadPluginSettings), ePriority.Normal,"Load Plugin Settings");
        }

        private void loadProviders()
        {
            object o;
            theHost.getData(eGetData.GetPlugins, "", out o);
            if (o == null)
                return;
            List<IBasePlugin> plugins = (List<IBasePlugin>)o;
            plugins=plugins.FindAll(p => typeof(IDataProvider).IsInstanceOfType(p));
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                OMList list = (OMList)manager[i, "data"][7];
                OImage img = null;
                list.Clear();
                OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                format.color = Color.FromArgb(140, Color.White);
                foreach (IDataProvider d in plugins)
                {
                    if (d.updaterStatus() == 1)
                        img = theHost.getSkinImage("Checkmark").image;
                    if (d.updaterStatus() == -1)
                        img = theHost.getSkinImage("Error").image;
                    if (d.updaterStatus() == 0)
                        img = theHost.getSkinImage("Waiting").image;
                    list.Add(new OMListItem(d.pluginDescription, d.lastUpdated.ToString(), img, format));
                }
            }
        }

        void menu_OnClick(OMControl sender, int screen)
        {
            OMList list = (OMList)sender;
            string Panel = (string)list.SelectedItem.tag;
            switch (Panel)
            {
                case "personal":
                    ((OMTextBox)manager[screen, "personal"][6]).Text = Credentials.getCredential("Google Password");
                    ((OMTextBox)manager[screen, "personal"][5]).Text = Credentials.getCredential("Google Username");
                    break;
                case "data":
                    using (PluginSettings s = new PluginSettings())
                        ((OMTextBox)manager[screen, "data"][4]).Text = s.getSetting("Data.DefaultLocation");
                    loadProviders();
                    break;
            }

            // Change screen
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", Panel);
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");

            ((OMList)sender).Select(-1);
        }

        void location_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput();
            ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", "data",((OMTextBox)sender).Text);
        }

        void Save4_OnClick(OMControl sender, int screen)
        {
            using (PluginSettings settings = new PluginSettings())
            {
                settings.setSetting("Data.DefaultLocation", ((OMTextBox)manager[screen, "data"][4]).Text);
                settings.setSetting("Plugins.DPYWeather.LastUpdate", DateTime.MinValue.ToString());
                settings.setSetting("Plugins.DPGWeather.LastUpdate", DateTime.MinValue.ToString());
                settings.setSetting("Plugins.DPWeather.LastUpdate", DateTime.MinValue.ToString());
            }
            theHost.execute(eFunction.refreshData);
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void user_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput();
            if ((((OMTextBox)sender).Flags&textboxFlags.Password)==textboxFlags.Password)
                ((OMTextBox)sender).Text = input.getPassword(screen, "OMSettings", "personal", ((OMTextBox)sender).Text);
            else
                ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", "personal",((OMTextBox)sender).Text);
        }

        void Save2_OnClick(OMControl sender, int screen)
        {
            Credentials.setCredential("Google Password", ((OMTextBox)manager[screen, "personal"][6]).Text);
            string googleUsername = ((OMTextBox)manager[screen, "personal"][5]).Text;
            if ((googleUsername!=null)&&(googleUsername.EndsWith("@gmail.com") == false))
                googleUsername += "@gmail.com";
            Credentials.setCredential("Google Username", googleUsername);
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;

            switch (name)
            {
                case "":
                case "Default":
                    ((OMList)manager[screen][0]).SelectedIndex = -1;
                    return manager[screen, "Main"];
                default:
                    return manager[screen, name];
            }
        }

        public OpenMobile.Plugin.Settings loadSettings()
        {
            return null;
        }

        public string displayName
        {
            get { return "Settings"; }
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
            get { return "OMSettings"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Global Settings Manager for openMobile"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
