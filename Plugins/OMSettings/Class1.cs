using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Framework;
using System.Drawing;
using OpenMobile;
using OpenMobile.Data;

namespace OMSettings
{
    public class Settings:IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            #region menu
            OMPanel main = new OMPanel("Main");
            main.BackgroundColor1 = Color.Black;
            main.BackgroundType = backgroundStyle.SolidColor;
            OMList menu = new OMList(10, 97, 980, 433);
            menu.ListStyle = eListStyle.MultiList;
            menu.Background = Color.Silver;
            menu.ItemColor1 = Color.Black;
            menu.Font = new Font(FontFamily.GenericSansSerif, 30F);
            menu.Color = Color.White;
            menu.HighlightColor = Color.White;
            menu.SelectedItemColor1 = Color.DarkBlue;
            menu.Add(new OMListItem("General Settings", ""));
            menu.Add(new OMListItem("Personal Settings", "Usernames and Passwords"));
            menu.Add(new OMListItem("Data Settings", "Settings for each of the Data Providers"));
            menu.Add(new OMListItem("Multi-Zone Settings", "Displays, Sound Cards and other zone specific settings"));
            menu.Add(new OMListItem("Hardware Settings", "Hardware devices like the Fusion Brain, OBDII readers and game pads"));
            menu.SelectedIndexChanged += new OMList.IndexChangedDelegate(menu_SelectedIndexChanged);
            main.addControl(menu);
            manager.loadPanel(main);
            #endregion
            #region multiZone
            OMPanel MultiZone = new OMPanel("MultiZone");
            imageItem opt1 = theHost.getSkinImage("Monitor");
            Font fnt = new Font("Microsoft Sans Serif", 99.75F);
            OMButton Button1 = new OMButton(47, 132, 200, 200);
            Button1.Image = opt1;
            Button1.Name = "Button1";
            Button1.Font = fnt;
            Button1.Text = "1";
            Button1.Format = textFormat.Outline;
            Button1.OnClick+=new userInteraction(Button_OnClick);
            OMButton Button2 = new OMButton(280, 132, 200, 200);
            Button2.Image = opt1;
            Button2.Name = "Button2";
            Button2.Font = fnt;
            Button2.Text = "2";
            Button2.Format = textFormat.Outline;
            Button2.OnClick += new userInteraction(Button_OnClick);
            OMButton Button3 = new OMButton(517, 132, 200, 200);
            Button3.Image = opt1;
            Button3.Name = "Button3";
            Button3.Font = fnt;
            Button3.Text = "3";
            Button3.Format = textFormat.Outline;
            Button3.OnClick += new userInteraction(Button_OnClick);
            OMButton Button4 = new OMButton(764, 132, 200, 200);
            Button4.Image = opt1;
            Button4.Name = "Button4";
            Button4.Font = fnt;
            Button4.Text = "4";
            Button4.Format = textFormat.Outline;
            Button4.OnClick += new userInteraction(Button_OnClick);
            OMButton Button5 = new OMButton(47, 335, 200, 200);
            Button5.Image = opt1;
            Button5.Name = "Button5";
            Button5.Font = fnt;
            Button5.Text = "5";
            Button5.Format = textFormat.Outline;
            Button5.OnClick += new userInteraction(Button_OnClick);
            OMButton Button6 = new OMButton(280, 335, 200, 200);
            Button6.Image = opt1;
            Button6.Name = "Button6";
            Button6.Font = fnt;
            Button6.Text = "6";
            Button6.Format = textFormat.Outline;
            Button6.OnClick += new userInteraction(Button_OnClick);
            OMButton Button7 = new OMButton(517, 335, 200, 200);
            Button7.Image = opt1;
            Button7.Name = "Button7";
            Button7.Font = fnt;
            Button7.Text = "7";
            Button7.Format = textFormat.Outline;
            Button7.OnClick += new userInteraction(Button_OnClick);
            OMButton Button8 = new OMButton(764, 335, 200, 200);
            Button8.Image = opt1;
            Button8.Name = "Button8";
            Button8.Font = fnt;
            Button8.Text = "8";
            Button8.Format = textFormat.Outline;
            Button8.OnClick += new userInteraction(Button_OnClick);
            OMButton identify = new OMButton(204,540,200,60);
            identify.Image = theHost.getSkinImage("Tab");
            identify.Font = new Font("Microsoft Sans Serif", 18F);
            identify.Text = "Identify";
            identify.Format = textFormat.DropShadow;
            identify.Name = "Button3";
            identify.OnClick += new userInteraction(identify_OnClick);
            OMLabel title = new OMLabel(383,68,250,100);
            title.Font = new Font("Microsoft Sans Serif", 21.75F);
            title.Text = "Select a Screen";
            title.Format = textFormat.BoldShadow;
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
            left.Orientation = Angle.FlipHorizontal;
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
            caption.Format = textFormat.Glow;
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
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void identify_OnClick(object sender, int screen)
        {
            theHost.sendMessage("RenderingWindow", "MultiZone", "Identify");
        }

        void Cancel_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.goBack,screen.ToString());
        }

        void Save_OnClick(object sender, int screen)
        {
            string scr=((OMLabel)manager[screen, "zone"][6]).Text;
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Screen" + scr.Substring(5) + ".SoundCard", ((OMLabel)manager[screen, "zone"][5]).Text);
            theHost.execute(eFunction.goBack,screen.ToString());
            theHost.execute(eFunction.settingsChanged);
        }

        void right_OnClick(object sender, int screen)
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
        void left_OnClick(object sender, int screen)
        {
            if (devices == null)
                return;
            string current=((OMTextBox)manager[screen, "zone"][5]).Text;
            if (current.Replace("  "," ") == "Default Device")
                return;
            int pos = devices.FindIndex(p => p.Replace("  ", " ") == current);
            ((OMTextBox)manager[screen, "zone"][5]).Text = devices[pos - 1].Replace("  ", " ");
        }

        void Button_OnClick(object sender, int screen)
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
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
        }

        void menu_SelectedIndexChanged(OMList sender, int screen)
        {
            switch (sender.SelectedIndex)
            {
                case 3:
                    bool b=theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                    b = theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "MultiZone");
                    b = theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
                    break;
            }
        }

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
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

        public OpenMobile.Controls.OMPanel loadSettings(string name, int screen)
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
            manager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
