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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Threading;

namespace OMPluginSample
{
    public class OMPluginSample : IHighLevel
    {
        private ScreenManager manager;
        private IPluginHost theHost;
        private Settings settings;

        /// <summary>
        /// loadPanel is called everytime a panel is loaded or unloaded.
        /// It should return the panel containing your controls.
        /// </summary>
        /// <param name="name">The name of the panel (or an empty string for the default panel)</param>
        /// <param name="screen">The screen number requesting a panel</param>
        /// <returns></returns>
        public OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen, name];
        }

        public Settings loadSettings()
        {
            //If a plugin does not expose settings null may be returned
            if (settings == null)
            {
                settings = new Settings("PluginSample settings");
                using (PluginSettings setting = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "OMPluginSample.Setting1", "", "Setting description goes here", Setting.BooleanList, Setting.BooleanList, setting.getSetting("OMPluginSample.Setting1")));
                }
                settings.OnSettingChanged += new SettingChanged(Setting_Changed);
            }
            //Settings collections are automatically laid out by the framework
            return settings;
        }

        private void Setting_Changed(int scr,Setting s)
        {
            //When a setting changes you should update your plugin with
            //the new setting information

            //Then the setting should be saved to the database
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting(s.Name, s.Value);
        }
        /// <summary>
        /// The name that will be displayed on a main menu button
        /// </summary>
        public string displayName
        {
            get { return "OMPluginSample"; }
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "OM Dev Team"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMPluginSample"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "OpenMobile Sample plugin"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel panelMain = new OMPanel("");
            theHost = host;

            //NOTE: OM uses a 1000x600 scale for all controls and measurements
            OMButton Button_Sample = new OMButton(150, 200, 250, 100);
            //Each control has a wide array of properties to configure how it looks and acts
            Button_Sample.Name = "Button_Sample";
            Button_Sample.Image = theHost.getSkinImage("Full");
            Button_Sample.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_Sample.OnClick += new userInteraction(Button_Sample_OnClick);
            Button_Sample.OnLongClick += new userInteraction(Button_Sample_OnLongClick);
            Button_Sample.Transition = eButtonTransition.None;
            Button_Sample.Text = "Sample button";
            panelMain.addControl(Button_Sample);

            OMLabel Label_Sample = new OMLabel(100, 300, 350, 40);
            Label_Sample.Name = "Label_Sample";
            Label_Sample.TextAlignment = Alignment.CenterCenter;
            Label_Sample.Font = new Font(Font.GenericSansSerif, 24F);
            Label_Sample.Format = eTextFormat.Bold;
            Label_Sample.Text = "Not clicked";
            panelMain.addControl(Label_Sample);


            OMBasicShape Shape_SettingBorder = new OMBasicShape(500, 180, 350, 200);
            Shape_SettingBorder.Name = "Shape_SettingBorder";
            Shape_SettingBorder.Shape = shapes.RoundedRectangle;
            Shape_SettingBorder.FillColor = Color.FromArgb(58, 58, 58);
            Shape_SettingBorder.BorderColor = Color.Gray;
            Shape_SettingBorder.BorderSize = 2;
            panelMain.addControl(Shape_SettingBorder);

            OMButton Button_Setting = new OMButton(550, 200, 250, 100);
            Button_Setting.Name = "Button_Sample";
            Button_Setting.Image = theHost.getSkinImage("Full");
            Button_Setting.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_Setting.OnClick += new userInteraction(Button_Setting_OnClick);
            Button_Setting.Transition = eButtonTransition.None;
            Button_Setting.Text = "Read setting\nfrom DB";
            panelMain.addControl(Button_Setting);

            OMLabel Label_Setting = new OMLabel(500, 320, 350, 40);
            Label_Setting.Name = "Label_Setting";
            Label_Setting.TextAlignment = Alignment.CenterCenter;
            Label_Setting.Font = new Font(Font.GenericSansSerif, 24F);
            Label_Setting.Format = eTextFormat.Bold;
            Label_Setting.Text = "Setting value";
            panelMain.addControl(Label_Setting);

            //Screen managers take care of ensuring your panel is multi-monitor safe
            manager = new ScreenManager(theHost.ScreenCount);
            manager.loadPanel(panelMain);
            //Should something go wrong during initialization
            //you can return an error otherwise return Successful
            return eLoadStatus.LoadSuccessful;
        }

        void Button_Setting_OnClick(OMControl sender, int screen)
        {
            // Read settings
            using (PluginSettings setting = new PluginSettings())
                ((OMLabel)manager[screen]["Label_Setting"]).Text = setting.getSetting("OMPluginSample.Setting1");
        }

        void Button_Sample_OnLongClick(OMControl sender, int screen)
        {
            ((OMLabel)manager[screen]["Label_Sample"]).Text = "Long click detected";
        }

        void Button_Sample_OnClick(OMControl sender, int screen)
        {
            ((OMLabel)manager[screen]["Label_Sample"]).Text = "Click detected";
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
           //
        }

        #endregion

    }
}
