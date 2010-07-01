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
using System.Drawing;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace OMSettings
{
    public class Settings:IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            manager = new ScreenManager(host.ScreenCount);
            #region menu
            OMPanel main = new OMPanel("Main");
            main.BackgroundColor1 = Color.Black;
            main.BackgroundType = backgroundStyle.SolidColor;
            OMList menu = new OMList(10, 100, 980, 433);
            menu.ListStyle = eListStyle.MultiList;
            menu.Background = Color.Silver;
            menu.ItemColor1 = Color.Black;
            menu.Font = new Font(FontFamily.GenericSansSerif, 30F);
            menu.Color = Color.White;
            menu.HighlightColor = Color.White;
            menu.SelectedItemColor1 = Color.DarkBlue;
            OMListItem.subItemFormat format=new OMListItem.subItemFormat();
            format.color=Color.FromArgb(128,Color.White);
            format.font = new Font(FontFamily.GenericSansSerif, 21F);
            menu.Add(new OMListItem("General Settings", "User Interface and System Settings",format));
            menu.Add(new OMListItem("Personal Settings", "Usernames and Passwords", format));
            menu.Add(new OMListItem("Data Settings", "Settings for each of the Data Providers", format));
            menu.Add(new OMListItem("Multi-Zone Settings", "Displays, Sound Cards and other zone specific settings", format));
            menu.Add(new OMListItem("Plugin Settings", "Settings for all low level plugins", format));
            menu.OnClick += new userInteraction(menu_OnClick);
            main.addControl(menu);
            manager.loadPanel(main);
            #endregion
            #region multiZone
            OMPanel MultiZone = new OMPanel("MultiZone");
            imageItem opt1 = theHost.getSkinImage("Monitor");
            imageItem opt4 = theHost.getSkinImage("Monitor.Highlighted");
            Font fnt = new Font("Microsoft Sans Serif", 99.75F);
            OMButton Button1 = new OMButton(47, 132, 200, 200);
            Button1.Image = opt1;
            Button1.FocusImage = opt4;
            Button1.Name = "Button1";
            Button1.Font = fnt;
            Button1.Text = "1";
            Button1.Format = eTextFormat.Outline;
            Button1.OnClick+=new userInteraction(Button_OnClick);
            OMButton Button2 = new OMButton(280, 132, 200, 200);
            Button2.Image = opt1;
            Button2.FocusImage = opt4;
            Button2.Name = "Button2";
            Button2.Font = fnt;
            Button2.Text = "2";
            Button2.Format = eTextFormat.Outline;
            Button2.OnClick += new userInteraction(Button_OnClick);
            OMButton Button3 = new OMButton(517, 132, 200, 200);
            Button3.Image = opt1;
            Button3.FocusImage = opt4;
            Button3.Name = "Button3";
            Button3.Font = fnt;
            Button3.Text = "3";
            Button3.Format = eTextFormat.Outline;
            Button3.OnClick += new userInteraction(Button_OnClick);
            OMButton Button4 = new OMButton(764, 132, 200, 200);
            Button4.Image = opt1;
            Button4.FocusImage = opt4;
            Button4.Name = "Button4";
            Button4.Font = fnt;
            Button4.Text = "4";
            Button4.Format = eTextFormat.Outline;
            Button4.OnClick += new userInteraction(Button_OnClick);
            OMButton Button5 = new OMButton(47, 335, 200, 200);
            Button5.Image = opt1;
            Button5.FocusImage = opt4;
            Button5.Name = "Button5";
            Button5.Font = fnt;
            Button5.Text = "5";
            Button5.Format = eTextFormat.Outline;
            Button5.OnClick += new userInteraction(Button_OnClick);
            OMButton Button6 = new OMButton(280, 335, 200, 200);
            Button6.Image = opt1;
            Button6.FocusImage = opt4;
            Button6.Name = "Button6";
            Button6.Font = fnt;
            Button6.Text = "6";
            Button6.Format = eTextFormat.Outline;
            Button6.OnClick += new userInteraction(Button_OnClick);
            OMButton Button7 = new OMButton(517, 335, 200, 200);
            Button7.Image = opt1;
            Button7.FocusImage = opt4;
            Button7.Name = "Button7";
            Button7.Font = fnt;
            Button7.Text = "7";
            Button7.Format = eTextFormat.Outline;
            Button7.OnClick += new userInteraction(Button_OnClick);
            OMButton Button8 = new OMButton(764, 335, 200, 200);
            Button8.Image = opt1;
            Button8.FocusImage = opt4;
            Button8.Name = "Button8";
            Button8.Font = fnt;
            Button8.Text = "8";
            Button8.Format = eTextFormat.Outline;
            Button8.OnClick += new userInteraction(Button_OnClick);
            OMButton identify = new OMButton(204,540,200,60);
            identify.Image = theHost.getSkinImage("Tab");
            identify.Font = new Font("Microsoft Sans Serif", 18F);
            identify.Text = "Identify";
            identify.Format = eTextFormat.DropShadow;
            identify.Name = "Button3";
            identify.OnClick += new userInteraction(identify_OnClick);
            OMLabel title = new OMLabel(383,68,250,100);
            title.Font = new Font("Microsoft Sans Serif", 21.75F);
            title.Text = "Select a Screen";
            title.Format = eTextFormat.BoldShadow;
            title.Name = "Label4";
            MultiZone.addControl(Button1);
            MultiZone.addControl(Button2);
            MultiZone.addControl(Button3);
            MultiZone.addControl(Button4);
            MultiZone.addControl(Button5);
            MultiZone.addControl(Button6);
            MultiZone.addControl(Button7);
            MultiZone.addControl(Button8);
            MultiZone.addControl(identify);
            MultiZone.addControl(title);
            manager.loadPanel(MultiZone);
            #endregion
            #region zone
            OMPanel zone = new OMPanel("zone");
            imageItem opt2 = theHost.getSkinImage("Play.Highlighted");
            imageItem opt3 = theHost.getSkinImage("Play");

            OMButton Save = new OMButton(13, 136, 200, 110);
            Save.Image = theHost.getSkinImage("Full");
            Save.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Save.Text = "Save";
            Save.Name = "Media.Save";
            Save.OnClick += new userInteraction(Save_OnClick);
            Save.Transition = eButtonTransition.None;
            OMButton Cancel = new OMButton(14, 255, 200, 110);
            Cancel.Image = Save.Image;
            Cancel.FocusImage = Save.FocusImage;
            Cancel.Text = "Cancel";
            Cancel.Name = "Media.Cancel";
            Cancel.Transition = eButtonTransition.None;
            Cancel.OnClick += new userInteraction(Cancel_OnClick);
            OMButton right = new OMButton(842, 230, 120, 80);
            right.Image = opt3;
            right.FocusImage = opt2;
            right.Name = "Button1";
            right.OnClick += new userInteraction(right_OnClick);
            right.Transition = eButtonTransition.None;
            OMButton left = new OMButton(255, 230, 120, 80);
            left.Image = opt3;
            left.FocusImage = opt2;
            left.Orientation = eAngle.FlipHorizontal;
            left.Name = "Button1";
            left.Transition = eButtonTransition.None;
            left.OnClick += new userInteraction(left_OnClick);
            OMLabel Label = new OMLabel(383,160,450,100);
            Label.Font = new Font("Microsoft Sans Serif", 36F);
            Label.Text = "Audio Zone";
            Label.Name = "Label";
            OMTextBox Textbox3 = new OMTextBox(383,245,450,50);
            Textbox3.Flags = textboxFlags.EllipsisEnd;
            Textbox3.Font = new Font("Microsoft Sans Serif", 21.75F);
            Textbox3.Name = "Textbox3";
            OMLabel caption = new OMLabel(0, 100, 1000, 60);
            caption.OutlineColor = Color.Green;
            caption.Font = new Font(FontFamily.GenericSansSerif, 36);
            caption.Format = eTextFormat.Glow;
            caption.Name = "caption";
            zone.addControl(Save);
            zone.addControl(Cancel);
            zone.addControl(right);
            zone.addControl(left);
            zone.addControl(Label);
            zone.addControl(Textbox3);
            zone.addControl(caption);
            manager.loadPanel(zone);
            object o;
            theHost.getData(eGetData.GetAudioDevices, "", out o);
            if (o!=null)
                devices=new List<string>((string[])o);
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
            Heading.Font = new Font("Microsoft Sans Serif", 36F);
            Heading.Text = "Google Account Info";
            Label.Name = "Label";
            OMLabel udesc = new OMLabel(220, 180, 200, 50);
            udesc.Text = "Username:";
            udesc.Font = new Font("Microsoft Sans Serif", 24F);
            OMLabel pdesc = new OMLabel(220, 240, 200, 50);
            pdesc.Text = "Password:";
            pdesc.Font= new Font("Microsoft Sans Serif", 24F);
            OMTextBox user = new OMTextBox(450, 180, 500, 50);
            user.Font = new Font("Microsoft Sans Serif", 28F);
            user.TextAlignment = Alignment.CenterLeft;
            user.OnClick += new userInteraction(user_OnClick);
            OMTextBox pass = new OMTextBox(450, 240, 500, 50);
            pass.Font = new Font("Microsoft Sans Serif", 28F);
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
            OMPanel general = generalLayout.layout(theHost, BuiltInComponents.GlobalSettings(host));
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
            Heading3.Font = new Font("Microsoft Sans Serif", 36F);
            Heading3.Text = "Data Provider Settings";
            Label.Name = "Label";
            OMLabel ldesc = new OMLabel(210, 180, 280, 50);
            ldesc.Text = "Default Location:";
            ldesc.Font = new Font("Microsoft Sans Serif", 24F);
            OMTextBox location = new OMTextBox(525, 180, 450, 50);
            location.Font = new Font("Microsoft Sans Serif", 28F);
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
            #region Hardware
            OMPanel hardware = new OMPanel("Hardware");
            hardware.BackgroundColor1 = Color.Black;
            hardware.BackgroundType = backgroundStyle.SolidColor;
            OMList lsthardware = new OMList(10, 100, 980, 433);
            lsthardware.ListStyle = eListStyle.MultiList;
            lsthardware.Background = Color.Silver;
            lsthardware.ItemColor1 = Color.Black;
            lsthardware.Font = new Font(FontFamily.GenericSansSerif, 30F);
            lsthardware.Color = Color.White;
            lsthardware.HighlightColor = Color.White;
            lsthardware.SelectedItemColor1 = Color.DarkBlue;
            lsthardware.OnClick += new userInteraction(lsthardware_OnClick);
            lsthardware.Add("Loading . . .");
            hardware.addControl(lsthardware);
            manager.loadPanel(hardware);
            #endregion
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
            OMList lsthardware = (OMList)manager[0, "Hardware"][0];
            lsthardware.Clear();
            foreach (IBasePlugin b in plugins)
            {
                LayoutManager lm = new LayoutManager();
                OMPanel panel;
                try
                {
                    panel = lm.layout(theHost, b.loadSettings());
                }
                catch (NotImplementedException) { continue; }
                catch (Exception e)
                {
                    problems.Add(e);
                    continue;
                }
                if (panel != null)
                {
                    panel.Name = b.pluginName + " Settings";
                    lsthardware.Add(new OMListItem(b.pluginName + " Settings", b.pluginDescription, format));
                    manager.loadPanel(panel);
                }
            }
            foreach (Exception e in problems)
            {
                Exception ex = e;
                theHost.sendMessage("SandboxedThread", "OMSettings", "", ref ex);
            }
        }

        void lsthardware_OnClick(OMControl sender, int screen)
        {
            OMList lst=(OMList)sender;
            if (lst.SelectedItem.text == "Loading . . .")
                return;
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"OMSettings",lst.SelectedItem.text)==false)
                return;
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(),"OMSettings","Hardware");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"SlideLeft");
        }

        void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.settingsChanged)
                loadProviders();
            if (function==eFunction.pluginLoadingComplete)
                OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(loadPluginSettings), ePriority.Normal);
        }

        private void loadProviders()
        {
            object o;
            theHost.getData(eGetData.GetPlugins, "", out o);
            if (o == null)
                return;
            List<IBasePlugin> plugins = (List<IBasePlugin>)o;
            plugins=plugins.FindAll(p => typeof(IDataProvider).IsInstanceOfType(p));
            OMList list = (OMList)manager[0, "data"][7];
            Image img = null;
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
                list.Add(new OMListItem(d.pluginDescription, d.lastUpdated.ToString(),img,format));   
            }
        }

        void menu_OnClick(OMControl sender, int screen)
        {
            switch (((OMList)sender).SelectedIndex)
            {
                case 0:
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "General Settings");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
                case 1:
                    Personal.readInfo();
                    ((OMTextBox)manager[screen, "personal"][6]).Text = Personal.getPassword(Personal.ePassword.google, "GOOGLEPW");
                    ((OMTextBox)manager[screen, "personal"][5]).Text = Collections.personalInfo.googleUsername;

                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "personal");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
                case 2:
                    using (PluginSettings s = new PluginSettings())
                        ((OMTextBox)manager[screen, "data"][4]).Text = s.getSetting("Data.DefaultLocation");
                    loadProviders();
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "data");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
                case 3:
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "MultiZone");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
                case 4:
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "Hardware");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
            }
            ((OMList)sender).Select(-1);
        }

        void location_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", "data");
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
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text = input.getText(screen, "OMSettings", "personal");
        }

        void Save2_OnClick(OMControl sender, int screen)
        {
            Personal.setPassword(Personal.ePassword.google,((OMTextBox)manager[screen,"personal"][6]).Text,"GOOGLEPW");
            Collections.personalInfo.googleUsername = ((OMTextBox)manager[screen, "personal"][5]).Text;
            if (Collections.personalInfo.googleUsername.EndsWith("@gmail.com") == false)
                Collections.personalInfo.googleUsername += "@gmail.com";
            Personal.writeInfo();
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void identify_OnClick(OMControl sender, int screen)
        {
            theHost.sendMessage("RenderingWindow", "OMSettings", "Identify");
        }

        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void Save_OnClick(OMControl sender, int screen)
        {
            string scr=((OMLabel)manager[screen, "zone"][6]).Text;
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Screen" + scr.Substring(5) + ".SoundCard", ((OMLabel)manager[screen, "zone"][5]).Text);
            theHost.execute(eFunction.goBack, screen.ToString());
            theHost.execute(eFunction.settingsChanged);
        }

        void right_OnClick(OMControl sender, int screen)
        {
            if (devices == null)
                return;
            string current = ((OMTextBox)manager[screen, "zone"][5]).Text;
            int pos = devices.FindIndex(p => p.Replace("  ", " ") == current);
            if (pos == devices.Count - 1)
                return;
            ((OMTextBox)manager[screen, "zone"][5]).Text = devices[pos + 1].Replace("  ", " ");
        }
        List<string> devices;
        void left_OnClick(OMControl sender, int screen)
        {
            if (devices == null)
                return;
            string current=((OMTextBox)manager[screen, "zone"][5]).Text;
            if (current.Replace("  "," ") == "Default Device")
                return;
            int pos = devices.FindIndex(p => p.Replace("  ", " ") == current);
            ((OMTextBox)manager[screen, "zone"][5]).Text = devices[pos - 1].Replace("  ", " ");
        }

        void Button_OnClick(OMControl sender, int screen)
        {
            ((OMLabel)manager[screen, "zone"][6]).Text = "Zone " + ((OMButton)sender).Text;
            string s;
            using(PluginSettings settings=new PluginSettings())
                s=settings.getSetting("Screen"+((OMButton)sender).Text+".SoundCard");
            if (s != "")
                ((OMLabel)manager[screen, "zone"][5]).Text = s;
            else
                ((OMLabel)manager[screen, "zone"][5]).Text = "Default Device";
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings", "MultiZone");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "zone");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"SlideLeft");
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
            throw new NotImplementedException();
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
