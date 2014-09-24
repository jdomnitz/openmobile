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
using System.Collections.Generic;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Threading;
using OpenMobile.helperFunctions.Forms;

namespace OMRadio
{
    [SkinIcon("*»")] //"*»"
    public class OMRadio : IHighLevel
    {
        private ScreenManager manager;
        private IPluginHost theHost;
        private enum StationListSources { Live, Presets }
        private StationListSources StationListSource = StationListSources.Live;
        private List<stationInfo> Presets = new List<stationInfo>();
        private Settings settings;
        private bool SourceSelected = false;
        private int OpacityLevel = 128;
        private int OpacityButtons = 170;

        #region IHighLevel Members

        public OMPanel loadPanel(string name, int screen)
        {

            //theHost.DebugMsg(string.Format("Radio - loadPanel()"), string.Format("name = {0}, screen = {1}", name, screen)); 

            if (manager == null)
                return null;

            if (name == "ListView")
                ConfigureListView(screen);
            if (name == "MessageBox")
                ConfigureMessageBox(screen);

            // Default panel
            if (name == "") 
            {
                ConfigureMainPanel(screen);
                return manager[screen, "Radio"];
            }
                
            return manager[screen, name];
        }

        private void ConfigureMainPanel(int screen)
        {
            // Default data
            using (PluginSettings setting = new PluginSettings())
            {
                if (!SourceSelected)
                {
                    //theHost.DebugMsg(string.Format("Radio - ConfigureMainPanel()"), "Source not set. Fetching DefaultTunedContentSource...");
                    string Source = setting.getSetting(this, "Radio.DefaultTunedContentSource");
                    if ((Source != "None") && (Source != ""))
                    {
                        //theHost.DebugMsg(string.Format("Radio - ConfigureMainPanel()"), "DefaultTunedContentSource: " + Source);
                        tunedContentInfo o = theHost.getData(eGetData.GetTunedContentInfo, "", screen.ToString()) as tunedContentInfo;
                        if (o == null)
                        {
                            for (int i = 0; i < theHost.ScreenCount; i++)
                                ((OMLabel)manager[i]["Radio_StationName"]).Text = "Loading source: " + Source;

                            //theHost.DebugMsg(string.Format("Radio - ConfigureMainPanel()"), "Loading source: " + Source);

                            SourceSelected = true;

                            SafeThread.Asynchronous(delegate() { AutoLoadSource(screen, Source); }, theHost);

                        }
                    }
                }
                else
                {
                    //theHost.DebugMsg(string.Format("Radio - ConfigureMainPanel()"), "Source is already selected.");
                }
            }
        }

        private void ConfigureListView(int screen)
        {
            OMPanel panel = manager[screen, "ListView"];
            switch ((string)panel.Tag)
            {
                case "source":
                    {
                        //theHost.DebugMsg(string.Format("Radio - ConfigureListView()"), "Build list of available sources.");
                        ((OMLabel)panel["Label_Header"]).Text = "Select tuned content source:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        List<IBasePlugin> lst = theHost.getData(eGetData.GetPlugins, "") as List<IBasePlugin>;
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(Font.GenericSansSerif, 21F);
                        if (lst != null)
                        {
                            lst = lst.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
                            foreach (IBasePlugin b in lst)
                                List_Source.Add(new OMListItem(b.pluginDescription, b.pluginName, format, b.pluginName));

                            List_Source.Add(new OMListItem("None", "", format, "unload"));
                        }
                        else
                        {
                            //theHost.DebugMsg(string.Format("Radio - ConfigureListView()"), "No available sources found.");
                            List_Source.Add(new OMListItem("No source available", "", format,"unload"));
                        }
                    }
                    break;
                case "band":
                    {
                        //theHost.DebugMsg(string.Format("Radio - ConfigureListView()"), "Build list of supported bands.");
                        ((OMLabel)panel["Label_Header"]).Text = "Select band:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        eTunedContentBand[] lst = theHost.getData(eGetData.GetSupportedBands, "", screen.ToString()) as eTunedContentBand[];
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(Font.GenericSansSerif, 21F);
                        if (lst != null)
                        {
                            foreach (eTunedContentBand b in lst)
                                List_Source.Add(new OMListItem(b.ToString(), "", format, b));
                        }
                        else
                        {
                            //theHost.DebugMsg(string.Format("Radio - ConfigureListView()"), "No supported bands found.");
                            List_Source.Add(new OMListItem("No band available", "", format,"unload"));
                        }
                    }
                    break;
                case "stationlist":
                    {
                        //theHost.DebugMsg(string.Format("Radio - ConfigureListView()"), "Build list of station sources.");
                        ((OMLabel)panel["Label_Header"]).Text = "Select station list source:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(Font.GenericSansSerif, 21F);
                        List_Source.Add(new OMListItem("Live channels", "Channels reported by the plugin", format, "live"));
                        List_Source.Add(new OMListItem("Presets", "Previously saved channels", format, "preset"));
                    }
                    break;
                default:
                    break;
            }

        }
        private void ConfigureMessageBox(int screen)
        {
            OMPanel panel = manager[screen, "MessageBox"];
            switch ((string)panel.Tag)
            {
                case "presets":
                    {
                        ((OMLabel)panel["MessageBox_Label_Header"]).Text = "Delete preset?";
                        string preset = ((OMList)manager[screen]["List_RadioStations"]).SelectedItem.text;
                        ((OMLabel)panel["MessageBox_Label_Info"]).Text = "Delete preset '" + preset + "'?";
                    }
                    break;
                default:
                    break;
            }

        }

        private int loadingCount = 0;
        private void AutoLoadSource(int screen, string Source)
        {
            loadingCount += 1;
            //theHost.DebugMsg(string.Format("Radio - AutoLoadSource()"), string.Format("Attempting load of tuned content {0} (Attempt #: {1})", Source, loadingCount));
            if (theHost.execute(eFunction.loadTunedContent, screen.ToString(), Source))
            {
                theHost.execute(eFunction.Stop, screen.ToString());
                theHost.DebugMsg(string.Format("Radio - AutoLoadSource()"), "Loading of tuned content successful");
                UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);
                // Force update of station info?
                UpdateStationInfo(theHost.ZoneHandler.GetZone(screen));
            }
            else
            {
                theHost.DebugMsg(string.Format("Radio - AutoLoadSource()"), "Loading of tuned content failed, checking if currently playing");
                if (theHost.getPlayingMedia(screen).Type == eMediaType.Radio)
                {
                    //theHost.DebugMsg(string.Format("Radio - AutoLoadSource()"), "Currently playing media is Radio");
                    UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);
                    // Force update of station info?
                    UpdateStationInfo(theHost.ZoneHandler.GetZone(screen));
                }
                else
                {
                    theHost.DebugMsg(string.Format("Radio - AutoLoadSource()"), "No tunedcontent and radio is not currently playing");
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        ((OMLabel)manager[i]["Radio_StationName"]).Text = "Auto load failed!";
                }
            }
        }

        public Settings loadSettings()
        {

            //theHost.DebugMsg("Radio - loadSettings()", "Load radio settings.");

            if ((settings == null)&&(theHost!=null))
            {
                settings = new Settings("Radio settings");
                using (PluginSettings setting = new PluginSettings())
                {
                    // Default station list source
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Radio.UsePresetAsStartupListSource", "", "Use presets as default list source", Setting.BooleanList, Setting.BooleanList, setting.getSetting(this, "Radio.UsePresetAsStartupListSource")));

                    // Default tuned content source
                    List<string> TunedContentList = new List<string>();
                    // Minimum of 1 item
                    TunedContentList.Add("None");

                    List<IBasePlugin> lst = theHost.getData(eGetData.GetPlugins, "") as List<IBasePlugin>;

                    if (lst != null)
                    {
                        lst = lst.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
                        foreach (IBasePlugin b in lst)
                            TunedContentList.Add(b.pluginName);
                    }
                    else
                    {
                        //theHost.DebugMsg(string.Format("Radio - loadSettings()"), "There were no ITunedContent plugins found.");
                    }
                    // Show default source option
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Radio.DefaultTunedContentSource", "Source", "Default radio source", TunedContentList, TunedContentList, setting.getSetting(this, "Radio.DefaultTunedContentSource")));
                }
                settings.OnSettingChanged += new SettingChanged(Setting_Changed);
            }
            return settings;
        }

        private void Setting_Changed(int screen,Setting s)
        {
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting(this, s.Name, s.Value);
        }

        #endregion

        #region IBasePlugin Members

        public string displayName
        {
            get { return "Radio"; }
        }

        public string authorName
        {
            get { return "Bjørn Morten Orderløkken"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "Radio"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Radio control panel"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-Radio"); }
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

            OMPanel panelMain = new OMPanel("Radio");
            theHost = host;
            //theHost.DebugMsg("Radio - initialize()", "Initializing....");
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);

            // Default data
            using (PluginSettings setting = new PluginSettings())
            {
                if (setting.getSetting(this, "Radio.UsePresetAsStartupListSource") == "True")
                {
                    //theHost.DebugMsg(string.Format("Radio - Initialize()"), "Presets are default list.");
                    StationListSource = StationListSources.Presets;
                }
                else
                {
                    //theHost.DebugMsg(string.Format("Radio - Initialize()"), "LIVE is default list.");
                    StationListSource = StationListSources.Live;
                }

                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "Frequency", DataSource.DataTypes.text, "Radio tuned frequency"), "");
                //theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "Signal", DataSource.DataTypes.raw, "Radio signal strength"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "StationName", DataSource.DataTypes.text, "RDS Station Name"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "StationText", DataSource.DataTypes.text, "RDS Text"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "Status", DataSource.DataTypes.text, "Radio status"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "Genre", DataSource.DataTypes.text, "Station Genre"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "Band", DataSource.DataTypes.text, "Tuned band"), "");
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Radio", "Data", "HD", DataSource.DataTypes.raw, "Is radio in HD mode"), 0);

                /*
                if (string.IsNullOrEmpty(setting.getSetting(this, "Radio.DefaultTunedContentSource")))
                {
                    //theHost.DebugMsg(string.Format("Radio - Initialize()"), "No default source is set.");
                }
                else
                {
                    //theHost.DebugMsg(string.Format("Radio - Initialize()"), string.Format("Default source is set to: {0}.", setting.getSetting(this, "Radio.DefaultTunedContentSource")));
                }
                */
            }

            #region Main panel

            #region Buttons 

            int vgap = 5;

            OMButton Button_Source = OMButton.PreConfigLayout_CleanStyle("Button_Source", 10, 80, 150, 80);
            Button_Source.Text = "Source";
            Button_Source.OnClick += new userInteraction(Button_Source_OnClick);
            panelMain.addControl(Button_Source);

            OMButton Button_RadioBand = OMButton.PreConfigLayout_CleanStyle("Button_Band", Button_Source.Region.Left, Button_Source.Region.Bottom + vgap, Button_Source.Region.Width, Button_Source.Region.Height);
            Button_RadioBand.Text = "Band";
            Button_RadioBand.OnClick += new userInteraction(Button_RadioBand_OnClick);
            panelMain.addControl(Button_RadioBand);

            OMButton Button_RadioScanUP = OMButton.PreConfigLayout_CleanStyle("Button_RadioScanUP", Button_Source.Region.Left, Button_RadioBand.Region.Bottom + vgap, Button_Source.Region.Width, Button_Source.Region.Height);
            Button_RadioScanUP.Text = "Scan UP";
            Button_RadioScanUP.OnClick += new userInteraction(Button_RadioAutoScan_OnClick);
            //Button_RadioScanUP.OnLongClick += new userInteraction(Button_RadioAutoScan_OnLongClick);
            Button_RadioScanUP.OnHoldClick += new userInteraction(Button_RadioAutoScan_OnLongClick);
            Button_RadioScanUP.Visible = true;
            panelMain.addControl(Button_RadioScanUP);

            OMButton Button_RadioScanDN = OMButton.PreConfigLayout_CleanStyle("Button_RadioScanDN", Button_Source.Region.Left, Button_RadioBand.Region.Bottom + vgap, Button_Source.Region.Width, Button_Source.Region.Height);
            Button_RadioScanDN.Text = "Scan DN";
            Button_RadioScanDN.OnClick += new userInteraction(Button_RadioAutoScan_OnClick);
            //Button_RadioScanDN.OnLongClick += new userInteraction(Button_RadioAutoScan_OnLongClick);
            Button_RadioScanDN.OnHoldClick += new userInteraction(Button_RadioAutoScan_OnLongClick);
            Button_RadioScanDN.Visible = false;
            panelMain.addControl(Button_RadioScanDN);

            OMButton Button_ChannelListSource = OMButton.PreConfigLayout_CleanStyle("Button_ChannelListSource", Button_Source.Region.Left, Button_RadioScanUP.Region.Bottom + vgap, Button_Source.Region.Width, Button_Source.Region.Height);
            Button_ChannelListSource.Text = "List\nsource";
            Button_ChannelListSource.OnClick += new userInteraction(Button_ChannelListSource_OnClick);
            panelMain.addControl(Button_ChannelListSource);

            OMButton Button_TuneTo = OMButton.PreConfigLayout_CleanStyle("Button_TuneTo", Button_Source.Region.Left, Button_ChannelListSource.Region.Bottom + vgap, Button_Source.Region.Width, Button_Source.Region.Height);
            Button_TuneTo.Text = "Direct\nTune";
            Button_TuneTo.OnClick += new userInteraction(Button_TuneTo_OnClick);
            panelMain.addControl(Button_TuneTo);
            #endregion

            #region RadioInfo field

            OMBasicShape Shape_StationInfoBorder = new OMBasicShape("Shape_StationInfoBorder", 180, 80, 800, 140,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            Shape_StationInfoBorder.Opacity = OpacityLevel;
            panelMain.addControl(Shape_StationInfoBorder);

            OMLabel Radio_StationName = new OMLabel("Radio_StationName", Shape_StationInfoBorder.Left + 10, Shape_StationInfoBorder.Top + 5, Shape_StationInfoBorder.Width - 20 , 40);
            Radio_StationName.Name = "Radio_StationName";
            Radio_StationName.TextAlignment = Alignment.CenterLeft;
            Radio_StationName.Font = new Font(Font.GenericSansSerif, 24F);
            Radio_StationName.Format = eTextFormat.Bold;
            Radio_StationName.Text = "Select source...";
            panelMain.addControl(Radio_StationName);

            OMLabel Info = (OMLabel)Radio_StationName.Clone();
            Info.Name = "Info";
            Info.Text = "";
            Info.Visible = false;
            panelMain.addControl(Info);

            OMLabel Radio_StationText = new OMLabel("Radio_StationText", Radio_StationName.Left + 10, Radio_StationName.Top + Radio_StationName.Height, Radio_StationName.Width - 10, 30);
            Radio_StationText.Name = "Radio_StationText";
            Radio_StationText.TextAlignment = Alignment.CenterLeft;
            Radio_StationText.Font = new Font(Font.GenericSansSerif, 14F);
            Radio_StationText.Format = eTextFormat.Normal;
            Radio_StationText.Text = "";
            panelMain.addControl(Radio_StationText);

            OMLabel Radio_StationText2 = new OMLabel("Radio_StationText2", Radio_StationText.Left + 10, Radio_StationText.Top + Radio_StationText.Height, Radio_StationText.Width - 10, 30);
            Radio_StationText2.Name = "Radio_StationText2";
            Radio_StationText2.TextAlignment = Alignment.CenterLeft;
            Radio_StationText2.Font = new Font(Font.GenericSansSerif, 14F);
            Radio_StationText2.Format = eTextFormat.Normal;
            Radio_StationText2.Text = "";
            panelMain.addControl(Radio_StationText2);

            // Small text info along bottom of box
            OMLabel Radio_StationGenre = new OMLabel("Radio_StationGenre", Shape_StationInfoBorder.Left + 20, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 200, 20);
            Radio_StationGenre.Name = "Radio_StationGenre";
            Radio_StationGenre.TextAlignment = Alignment.CenterLeft;
            Radio_StationGenre.Font = new Font(Font.GenericSansSerif, 12F);
            Radio_StationGenre.Format = eTextFormat.Normal;
            Radio_StationGenre.Text = "";
            panelMain.addControl(Radio_StationGenre);

            OMLabel Radio_StationBitRate = new OMLabel("Radio_StationBitRate", Shape_StationInfoBorder.Left + Shape_StationInfoBorder.Width - 580, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 80, 20);
            Radio_StationBitRate.Name = "Radio_StationBitRate";
            Radio_StationBitRate.TextAlignment = Alignment.CenterCenter;
            Radio_StationBitRate.Font = new Font(Font.GenericSansSerif, 12F);
            Radio_StationBitRate.Format = eTextFormat.Normal;
            Radio_StationBitRate.Text = "";
            panelMain.addControl(Radio_StationBitRate);

            OMLabel Radio_StationHD = (OMLabel)Radio_StationBitRate.Clone();
            Radio_StationHD.Left = Radio_StationBitRate.Left + Radio_StationBitRate.Width + 10;
            Radio_StationHD.Width = 50;
            Radio_StationHD.Name = "Radio_StationHD";
            Radio_StationHD.Text = "";
            panelMain.addControl(Radio_StationHD);

            OMLabel Radio_StationSignal = (OMLabel)Radio_StationBitRate.Clone();
            Radio_StationSignal.Left = Radio_StationHD.Left + Radio_StationHD.Width + 10;
            Radio_StationSignal.Width = 140;
            Radio_StationSignal.Name = "Radio_StationSignal";
            Radio_StationSignal.Text = "";
            panelMain.addControl(Radio_StationSignal);

            OMLabel Radio_StationFrequency = (OMLabel)Radio_StationBitRate.Clone();
            Radio_StationFrequency.Left = Radio_StationSignal.Left + Radio_StationSignal.Width + 10;
            Radio_StationFrequency.Width = 110;
            Radio_StationFrequency.Name = "Radio_StationFrequency";
            Radio_StationFrequency.Text = "";
            panelMain.addControl(Radio_StationFrequency);

            OMLabel Radio_Status = (OMLabel)Radio_StationBitRate.Clone();
            Radio_Status.Left = Radio_StationFrequency.Left + Radio_StationFrequency.Width + 10;
            Radio_Status.Width = 110;
            Radio_Status.Name = "Radio_Status";
            Radio_Status.Text = "";
            panelMain.addControl(Radio_Status);

            OMLabel Radio_Band = (OMLabel)Radio_StationBitRate.Clone();
            Radio_Band.Left = Shape_StationInfoBorder.Left + Shape_StationInfoBorder.Width - 85;
            Radio_Band.TextAlignment = Alignment.CenterRight;
            Radio_Band.Width = 80;
            Radio_Band.Name = "Radio_Band";
            Radio_Band.Text = "";
            panelMain.addControl(Radio_Band);

            #endregion

            #region Channel List

            OMBasicShape Shape_ChannelListBorder = new OMBasicShape("Shape_ChannelListBorder", Shape_StationInfoBorder.Left, Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height + 10, Shape_StationInfoBorder.Width, 285,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            Shape_ChannelListBorder.Opacity = OpacityLevel;
            panelMain.addControl(Shape_ChannelListBorder);

            OMLabel Label_StationListHeader = new OMLabel("Label_StationListHeader", Shape_ChannelListBorder.Left + 5, Shape_ChannelListBorder.Top, Shape_ChannelListBorder.Width - 10, 30);
            Label_StationListHeader.Name = "Label_StationListHeader";
            Label_StationListHeader.TextAlignment = Alignment.CenterLeft;
            Label_StationListHeader.Font = new Font(Font.GenericSansSerif, 14F);
            Label_StationListHeader.Format = eTextFormat.Bold;
            Label_StationListHeader.Text = "Station list:";
            panelMain.addControl(Label_StationListHeader);

            OMLabel Label_StationListSource = new OMLabel("Label_StationListSource", Shape_ChannelListBorder.Left + Shape_ChannelListBorder .Width - 205, Shape_ChannelListBorder.Top, 200, 30);
            Label_StationListSource.Name = "Label_StationListSource";
            Label_StationListSource.TextAlignment = Alignment.CenterRight;
            Label_StationListSource.Font = new Font(Font.GenericSansSerif, 14F);
            Label_StationListSource.Format = eTextFormat.Normal;
            Label_StationListSource.Text = "Source: " + StationListSource.ToString();
            panelMain.addControl(Label_StationListSource);

            OMList List_RadioStations = new OMList(Shape_ChannelListBorder.Left + 5, Label_StationListHeader.Top + Label_StationListHeader.Height + 10, Shape_ChannelListBorder.Width - 10, Shape_ChannelListBorder.Height - 60);
            List_RadioStations.ListStyle = eListStyle.RoundedTextList;
            List_RadioStations.ItemColor1 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 66);
            List_RadioStations.ItemColor2 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 10);
            List_RadioStations.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List_RadioStations.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
            List_RadioStations.Name = "List_RadioStations";
            List_RadioStations.Font = new Font(Font.GenericSansSerif, 30F);
            List_RadioStations.Add("No channels available");
            List_RadioStations.Scrollbars = true;
            List_RadioStations.TextAlignment = Alignment.CenterLeft;
            List_RadioStations.OnClick += new userInteraction(List_RadioStations_OnClick);
            List_RadioStations.OnHoldClick += new userInteraction(List_RadioStations_OnHoldClick);
            panelMain.addControl(List_RadioStations);

            #endregion

            #endregion

            #region ListView panel

            OMPanel panelListView = new OMPanel("ListView");
            panelListView.Tag = "source";

            OMBasicShape Shape_AccessBlock = new OMBasicShape("Shape_AccessBlock", 0, 0, 1000, 600,
                new ShapeData(shapes.Rectangle, Color.FromArgb(150, Color.Black)));
            panelListView.addControl(Shape_AccessBlock);
            OMButton Button_Cancel = new OMButton("Button_Cancel", 0, 0, 1000, 600);
            Button_Cancel.Name = "Button_Cancel";
            Button_Cancel.OnClick += new userInteraction(Button_Cancel_OnClick);
            panelListView.addControl(Button_Cancel);

            OMBasicShape Shape_Border = new OMBasicShape("Shape_Border", 200, 120, 600, 400,
                new ShapeData(shapes.Rectangle, Color.FromArgb(58, 58, 58), Color.Gray, 2));
            panelListView.addControl(Shape_Border);

            OMLabel Label_Header = new OMLabel("Label_Header", Shape_Border.Left + 5, Shape_Border.Top + 5, Shape_Border.Width - 10, 30);
            Label_Header.Name = "Label_Header";
            Label_Header.Text = "Select tuned content source:";
            panelListView.addControl(Label_Header);

            OMBasicShape Shape_Line = new OMBasicShape("Shape_Line", Shape_Border.Left, Label_Header.Top + Label_Header.Height, Shape_Border.Width, 2,
                new ShapeData(shapes.Rectangle, Color.Gray));
            panelListView.addControl(Shape_Line);

            OMBasicShape Shape_Background = new OMBasicShape("Shape_Background", Shape_Border.Left + (int)Shape_Border.ShapeData.BorderSize, Shape_Line.Top + Shape_Line.Height, Shape_Border.Width - ((int)Shape_Border.ShapeData.BorderSize * 2), 355,
                new ShapeData(shapes.Rectangle, Color.Black));
            panelListView.addControl(Shape_Background);
            OMBasicShape Shape_Background_Lower = new OMBasicShape("Shape_Background_Lower", Shape_Background.Left, Shape_Background.Top + Shape_Background.Height - 13, Shape_Background.Width, 20,
                new ShapeData(shapes.RoundedRectangle, Color.Black));
            panelListView.addControl(Shape_Background_Lower);

            OMList ListView_List = new OMList(Label_Header.Left, Shape_Background.Top + 10, Label_Header.Width, Shape_Background.Height - 10);
            ListView_List.Name = "ListView_List";
            ListView_List.ListStyle = eListStyle.MultiList;
            ListView_List.Background = Color.Silver;
            ListView_List.ItemColor1 = Color.Black;
            ListView_List.Font = new Font(Font.GenericSansSerif, 30F);
            ListView_List.Color = Color.White;
            ListView_List.HighlightColor = Color.White;
            ListView_List.SelectedItemColor1 = Color.DarkBlue;
            OMListItem.subItemFormat format = new OMListItem.subItemFormat();
            format.color = Color.FromArgb(128, Color.White);
            format.font = new Font(Font.GenericSansSerif, 21F);
            ListView_List.Add(new OMListItem("No source available!", "", format));
            ListView_List.OnClick += new userInteraction(ListView_List_OnClick);
            panelListView.addControl(ListView_List);

            #endregion

            #region MessageBox panel

            OMPanel panelMessageBox = new OMPanel("MessageBox");
            panelMessageBox.Tag = "preset";

            OMBasicShape Shape_AccessBlock2 = new OMBasicShape("MessageBox_Shape_AccessBlock", 0, 0, 1000, 600,
                new ShapeData(shapes.Rectangle, Color.FromArgb(150, Color.Black)));
            panelMessageBox.addControl(Shape_AccessBlock2);
            OMButton Button_Cancel2 = new OMButton("MessageBox_Button_Cancel", 0, 0, 1000, 600);
            Button_Cancel2.Name = "MessageBox_Button_Cancel";
            Button_Cancel2.OnClick += new userInteraction(Button_Cancel_OnClick);
            panelMessageBox.addControl(Button_Cancel2);

            OMBasicShape Shape_Border2 = new OMBasicShape("MessageBox_Shape_Border", 250, 175, 500, 250,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(58, 58, 58), Color.Gray, 2));
            panelMessageBox.addControl(Shape_Border2);

            OMLabel Label_Header2 = new OMLabel("MessageBox_Label_Header", Shape_Border2.Left + 5, Shape_Border2.Top + 5, Shape_Border2.Width - 10, 30);
            Label_Header2.Name = "MessageBox_Label_Header";
            Label_Header2.Text = "Delete preset?";
            panelMessageBox.addControl(Label_Header2);

            OMBasicShape Shape_Line2 = new OMBasicShape("MessageBox_Shape_Line", Shape_Border2.Left, Label_Header2.Top + Label_Header2.Height, Shape_Border2.Width, 2,
                new ShapeData(shapes.Rectangle, Color.Gray));
            panelMessageBox.addControl(Shape_Line2);

            OMBasicShape Shape_Background2 = new OMBasicShape("MessageBox_Shape_Background", Shape_Border2.Left + (int)Shape_Border2.ShapeData.BorderSize, Shape_Line2.Top + Shape_Line2.Height, Shape_Border2.Width - ((int)Shape_Border2.ShapeData.BorderSize * 2), 205,
                new ShapeData(shapes.Rectangle, Color.Black));
            panelMessageBox.addControl(Shape_Background2);
            
            OMBasicShape Shape_Background_Lower2 = new OMBasicShape("MessageBox_Shape_Background_Lower", Shape_Background2.Left, Shape_Background2.Top + Shape_Background2.Height - 13, Shape_Background2.Width, 20,
                new ShapeData(shapes.RoundedRectangle, Color.Black));
            panelMessageBox.addControl(Shape_Background_Lower2);

            OMLabel Label_Info = new OMLabel("MessageBox_Label_Info", Shape_Border2.Left + 5, Shape_Background2.Top + 5, Shape_Border2.Width - 10, 135);
            Label_Info.Name = "MessageBox_Label_Info";
            Label_Info.Text = "";
            panelMessageBox.addControl(Label_Info);

            OMButton Button_Yes = OMButton.PreConfigLayout_CleanStyle("MessageBox_Button_Yes", Shape_Border2.Left + Shape_Border2.Width - 320, Shape_Border2.Top + Shape_Border2.Height - 80, 150, 70);
            //OMButton Button_Yes = new OMButton("MessageBox_Button_Yes", Shape_Border2.Left + Shape_Border2.Width - 320, Shape_Border2.Top + Shape_Border2.Height - 80, 150, 70);
            Button_Yes.Image = theHost.getSkinImage("Full");
            Button_Yes.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_Yes.OnClick += new userInteraction(Button_Yes_OnClick);
            Button_Yes.Transition = eButtonTransition.None;
            //Button_Yes.Text = "Yes";
            panelMessageBox.addControl(Button_Yes);

            OMButton Button_No = OMButton.PreConfigLayout_CleanStyle("MessageBox_Button_No", Button_Yes.Left + Button_Yes.Width + 10, Button_Yes.Top, Button_Yes.Width, Button_Yes.Height);
            //OMButton Button_No = new OMButton("MessageBox_Button_No", Button_Yes.Left + Button_Yes.Width + 10, Button_Yes.Top, Button_Yes.Width, Button_Yes.Height);
            Button_No.Image = theHost.getSkinImage("Full");
            Button_No.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_No.OnClick += new userInteraction(Button_No_OnClick);
            Button_No.Transition = eButtonTransition.None;
            //Button_No.Text = "No";
            panelMessageBox.addControl(Button_No);

            #endregion

            manager = new ScreenManager(this);
            manager.loadPanel(panelMain, true);
            manager.loadPanel(panelListView);
            manager.loadPanel(panelMessageBox);
            return eLoadStatus.LoadSuccessful;
        }

        void Button_TuneTo_OnClick(OMControl sender, int screen)
        {
            //theHost.DebugMsg(string.Format("Radio - Button_TuneTo_OnClick()"), "Direct Tune...");

            OpenMobile.helperFunctions.OSK osk = new OpenMobile.helperFunctions.OSK("", "tune parameters", "Enter tune parameters", OSKInputTypes.Keypad, false);
            string newStation = osk.ShowOSK(screen);

            stationInfo info = theHost.getData(eGetData.GetMediaStatus, "", screen.ToString()) as stationInfo;

            if ((info != null)&&(info.stationID!=null)&&(info.stationID.Contains(":")))
            {   //this method is more reliable for undefined bands (ex pandora)
                newStation = info.stationID.Substring(0, info.stationID.IndexOf(':') + 1) + newStation;
                theHost.execute(eFunction.tuneTo, screen.ToString(), newStation);
            }
            else
            {   //this method is plan b - if the radio hasn't tuned anything yet
                tunedContentInfo tc = theHost.getData(eGetData.GetTunedContentInfo, "", screen.ToString()) as tunedContentInfo;
                if (tc != null)
                {
                    newStation = tc.band.ToString() + ":" + newStation;
                    theHost.execute(eFunction.tuneTo, screen.ToString(), newStation);
                }
            }
        }

        void Button_No_OnClick(OMControl sender, int screen)
        {
            string PanelTag = (string)((OMPanel)manager[screen, "MessageBox"]).Tag;
            switch (PanelTag)
            {
                case "presets":
                        theHost.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.None.ToString());
                    break;
            }
        }

        void Button_Yes_OnClick(OMControl sender, int screen)
        {
            string PanelTag = (string)((OMPanel)manager[screen, "MessageBox"]).Tag;
            switch (PanelTag)
            {
                case "presets":
                    {
                        theHost.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.None.ToString());
                        stationInfo station = new stationInfo(((OMList)manager[screen]["List_RadioStations"]).SelectedItem.text, (string)((OMList)manager[screen]["List_RadioStations"]).SelectedItem.tag);
                        DeletePreset(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance, station);
                        UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);
                    }
                    break;
            }
        }

        void List_RadioStations_OnHoldClick(OMControl sender, int screen)
        {
            if (StationListSource == StationListSources.Live)
            {
                OMList List = (OMList)sender;
                if (List.SelectedItem.tag == null)
                    return;
                System.Media.SystemSounds.Beep.Play();
                Message(screen, "Preset saved", 1000);
                stationInfo station = new stationInfo(List.SelectedItem.text, (string)List.SelectedItem.tag);
                SaveToPresets(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance, station);
            }
            else if (StationListSource == StationListSources.Presets)
            {
                ((OMPanel)manager[screen, "MessageBox"]).Tag = "presets";
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "MessageBox");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            }
            
        }

        void Button_ChannelListSource_OnClick(OMControl sender, int screen)
        {
            ((OMPanel)manager[screen, "ListView"]).Tag = "stationlist";
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "ListView");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            ((OMButton)sender).Transition = eButtonTransition.None;
        }

        void Button_Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.None.ToString());
        }

        void ListView_List_OnClick(OMControl sender, int screen)
        {
            string PanelTag = (string)((OMPanel)manager[screen, "ListView"]).Tag;
            string SelectedItemTag;
            //string StationInfoMessage = "Select a station";

            if (((OMList)sender).SelectedItem.tag == null)
                return;
            else
                SelectedItemTag = ((OMList)sender).SelectedItem.tag.ToString();
            theHost.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.None.ToString());
            switch (PanelTag)
            {
                case "source":
                    {
                        SourceSelected = true;
                        if (SelectedItemTag == "unload")
                        {
                            theHost.execute(eFunction.unloadTunedContent, screen.ToString());
                            for (int i = 0; i < theHost.ScreenCount; i++)
                                ClearStationInfo(i);
                        }
                        else
                        {
                            for (int i = 0; i < theHost.ScreenCount; i++)
                                ((OMLabel)manager[i]["Radio_StationName"]).Text = "Loading " + SelectedItemTag;

                            dialog d = new dialog(this.pluginName, "");
                            d.Header = "Loading";
                            d.Text = "Please wait, loading " + SelectedItemTag;
                            d.Icon = OpenMobile.helperFunctions.Forms.icons.Busy;
                            d.ShowMsgBoxNonBlocking(screen,250);
                            if (theHost.execute(eFunction.loadTunedContent,screen.ToString(), SelectedItemTag))
                            {
                                //theHost.DebugMsg(string.Format("Radio - ListView_List_OnClick()"), "Loaded " + SelectedItemTag + ", select a station...");
                                d.Close();
                                UpdateStationInfo(theHost.ZoneHandler.GetZone(screen));
                                UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);
                                /*
                                if (UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance))
                                    for (int i = 0; i < theHost.ScreenCount; i++)
                                        ((OMLabel)manager[i]["Radio_StationName"]).Text = StationInfoMessage ;
                                */
                            }
                            else
                            {
                                //theHost.DebugMsg(string.Format("Radio - ListView_List_OnClick()"), "Loading " + SelectedItemTag + " failed!"); 
                                d.Close();
                                d = new dialog(this.pluginName, "");
                                d.Header = "Load failed";
                                d.Text = "Loading \"" + SelectedItemTag + "\" failed!";
                                d.Icon = icons.Error;
                                d.Button = buttons.OK;
                                d.ShowMsgBox(screen);
                            }
                        }
                    }
                    break;
                case "band":
                    {
                        
                        if (SelectedItemTag == "unload")
                            return;
                        for (int i = 0; i < theHost.ScreenCount; i++)
                            ClearStationInfo(i);
                        if (theHost.execute(eFunction.setBand, screen.ToString(), SelectedItemTag))
                            UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);

                    }
                    break;
                case "stationlist":
                    {
                        switch (SelectedItemTag)
                        {
                            case "live":
                                StationListSource = StationListSources.Live;
                                break;
                            case "preset":
                                StationListSource = StationListSources.Presets;
                                break;
                            default:
                                break;
                        }
                        ((OMLabel)manager[screen]["Label_StationListSource"]).Text = "Source: " + StationListSource.ToString();
                        UpdateStationList(theHost.ZoneHandler.GetZone(screen).AudioDevice.Instance);
                    }
                    break;
                default:
                    break;
            }
        }

        void List_RadioStations_OnClick(OMControl sender, int screen)
        {
            OMList List = (OMList)sender;
            if (List.SelectedItem.tag == null)
                return;
            for (int i = 0; i < theHost.ScreenCount; i++)
                if (theHost.ZoneHandler.GetZone(i) == theHost.ZoneHandler.GetZone(screen))
                    ((OMLabel)manager[i]["Radio_StationName"]).Text = "Tuning . . .";
            theHost.execute(eFunction.tuneTo, screen.ToString(), (string)List.SelectedItem.tag);
        }

        void Button_RadioAutoScan_OnClick(OMControl sender, int screen)
        {
            //theHost.execute(eFunction.scanBand, screen.ToString());
            //theHost.execute(eFunction.scanForward, screen.ToString());

            //OMPanel panelMain = ((OMPanel)manager[screen, "Radio"]);
            //OMButton button = ((OMButton)manager[screen]["Button_RadioScan"]);

            switch (sender.Name)
            {
                case "Button_RadioScanUP":
                    theHost.execute(eFunction.scanForward, screen.ToString());
                    break;
                case "Button_RadioScanDN":
                    theHost.execute(eFunction.scanBackward, screen.ToString());
                    break;
            }

        }

        void Button_RadioAutoScan_OnLongClick(OMControl sender, int screen)
        {

            OMPanel panelMain = ((OMPanel)manager[screen, "Radio"]);
            OMButton buttonUP = panelMain["Button_RadioScanUP"] as OMButton;
            OMButton buttonDN = panelMain["Button_RadioScanDN"] as OMButton;

            switch (sender.Name)
            {
                case "Button_RadioScanDN":
                    buttonDN.Visible = false;
                    buttonUP.Visible = true;
                    theHost.execute(eFunction.scanForward, screen.ToString());
                    break;
                case "Button_RadioScanUP":
                    buttonDN.Visible = true;
                    buttonUP.Visible = false;
                    theHost.execute(eFunction.scanBackward, screen.ToString());
                    break;
            }

        }

        void Button_RadioBand_OnClick(OMControl sender, int screen)
        {
            ((OMPanel)manager[screen, "ListView"]).Tag = "band";
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "ListView");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            ((OMButton)sender).Transition = eButtonTransition.None;
        }

        void theHost_OnMediaEvent(eFunction function, Zone zone, string arg)
        {

            tunedContentInfo info = theHost.getData(eGetData.GetTunedContentInfo, "", 0.ToString()) as tunedContentInfo;
            mediaInfo mediaInfo = theHost.getPlayingMedia(0);

            try
            {
                // Push current datasource data
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.Frequency", mediaInfo.Name, true);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.Signal", mediaInfo.Signal, true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.StationName", mediaInfo.Album, true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.StationText", mediaInfo.Artist, true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.Status", info.status.ToString(), true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.Genre", info.currentStation.stationGenre, true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.Band", info.band.ToString(), true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue(this.pluginName + ";Radio.Data.HD", info.currentStation.HD, true);
            }
            catch
            {
            }
            
            if ((function == eFunction.Play)||(function==eFunction.tunerDataUpdated))
            {
                UpdateStationInfo(zone);
                try
                {
                   UpdateStationList(zone.AudioDevice.Instance);
                }
                catch
                {
                    theHost.DebugMsg(string.Format("Radio - OnMediaEvent({0}, {1}, {2})", function, zone.Description, arg), string.Format("UpdateStationList failed"));
                }
            }
            else if (function == eFunction.Stop)
            {
                // Somehow stop the radio playing
            }
            else if (function == eFunction.stationListUpdated)
            {
                try
                {
                    UpdateStationList(zone.AudioDevice.Instance);
                }
                catch 
                {
                    //theHost.DebugMsg(string.Format("Radio - OnMediaEvent({0}, {1}, {2})", function, zone.Description, arg), string.Format("UpdateStationList failed"));
                }
            }
            else if (function == eFunction.unloadTunedContent)
            {
                //theHost.DebugMsg(string.Format("Radio - OnMediaEvent({0}, {1}, {2})", function, zone.Description, arg), "Unloading tuned content.");
                for (int i = 0; i < theHost.ScreenCount; i++)
                    if (theHost.ZoneHandler.GetZone(i) == zone)
                        killStationInfo(i);
            }

        }

        void Button_Source_OnClick(OMControl sender, int screen)
        {
            ((OMPanel)manager[screen, "ListView"]).Tag = "source";
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "ListView");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            ((OMButton)sender).Transition = eButtonTransition.None;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
           //
        }

        #endregion

        private void ClearStationInfo(int screen)
        {
            //theHost.DebugMsg(string.Format("Radio - ClearStationInfo({0})", screen), "Clearing station info.");
            ((OMLabel)manager[screen]["Radio_StationName"]).Text = "Select source...";
            ((OMLabel)manager[screen]["Radio_StationFrequency"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationSignal"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationGenre"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationBitRate"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationText"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationText2"]).Text = "";
            ((OMLabel)manager[screen]["Radio_Status"]).Text = "";
            ((OMLabel)manager[screen]["Radio_Band"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationHD"]).Text = "";
            OMList List = (OMList)manager[screen]["List_RadioStations"];
            List.Clear();
        }
        private void UpdateStationInfo(Zone zone)
        {
            //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), "UpdateStationInfo().");
            
            if (zone == null)
            {
                //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), "Zone was null.");
                return;
            }

            //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), "Fetching tuned content info.");

            tunedContentInfo  info = theHost.getData(eGetData.GetTunedContentInfo, "", zone.ToString()) as tunedContentInfo;

            if (info == null)
            {
                //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo({0})", zone.Description), "object 'info' was empty.");
                return;
            }

            if (info.currentStation == null)
            {
                //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), "currentStation data was null.");
                return;
            }
            
            mediaInfo mediaInfo = theHost.getPlayingMedia(zone);

            if (mediaInfo == null)
            {
                return;
            }

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                ((OMLabel)manager[i]["Radio_StationName"]).Text = info.currentStation.stationName;

                #region Frequency

                try
                {
                    if (((info.band == eTunedContentBand.AM) | (info.band == eTunedContentBand.FM)))
                    {
                        if ((info.currentStation.stationID.Contains(":")) && (info.currentStation.stationID.IndexOf(':') < (info.currentStation.stationID.Length - 1)))
                        {
                            int Frequency = int.Parse(info.currentStation.stationID.Substring(info.currentStation.stationID.IndexOf(':') + 1));
                            if (info.band == eTunedContentBand.AM)
                            {
                                ((OMLabel)manager[i]["Radio_StationFrequency"]).Text = (Frequency / 100.0F).ToString("####") + "KHz";
                            }
                            else
                            {
                                ((OMLabel)manager[i]["Radio_StationFrequency"]).Text = (Frequency / 1000.0F).ToString("##.00") + "MHz";
                            }
                        }
                        else
                        {
                            ((OMLabel)manager[i]["Radio_StationFrequency"]).Text = "";
                        }
                    }
                }
                catch
                {
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), "There were errors fetcing updated station info.");
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("StationID: {0}", info.currentStation.stationID));
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("StationName: {0}", info.currentStation.stationName));
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("StationArtist: {0}", info.currentStation.stationArtist));
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("Status: {0}", info.status));
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("Band: {0}", info.band));
                    //theHost.DebugMsg(string.Format("Radio - UpdateStationInfo()"), string.Format("mediaInfoName: {0}", mediaInfo.Name));
                }

                #endregion

                #region Signal strength

                switch (info.currentStation.signal)
                {
                    default:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "";
                        break;
                    case 0:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "No signal!";
                        break;
                    case 1:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "Signal: ▌";
                        break;
                    case 2:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "Signal: ▌▌";
                        break;
                    case 3:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "Signal: ▌▌▌";
                        break;
                    case 4:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "Signal: ▌▌▌▌";
                        break;
                    case 5:
                        ((OMLabel)manager[i]["Radio_StationSignal"]).Text = "Signal: ▌▌▌▌▌";
                        break;
                }

                #endregion

                ((OMLabel)manager[i]["Radio_StationText"]).Text = mediaInfo.Album;
                ((OMLabel)manager[i]["Radio_StationText2"]).Text = mediaInfo.Artist;

                ((OMLabel)manager[i]["Radio_StationGenre"]).Text = info.currentStation.stationGenre;
                ((OMLabel)manager[i]["Radio_StationBitRate"]).Text = (info.currentStation.Bitrate > 0 ? info.currentStation.Bitrate.ToString() + "Kbps" : "");
                ((OMLabel)manager[i]["Radio_Status"]).Text = info.status.ToString();
                ((OMLabel)manager[i]["Radio_Band"]).Text = info.band.ToString();
                ((OMLabel)manager[i]["Radio_StationHD"]).Text = (info.currentStation.HD ? "HD" : "");

            }
        }

        private void killStationInfo(int screen)
        {
            //theHost.DebugMsg("Radio - killStationInfo()", "Killing station info.");
            ((OMLabel)manager[screen]["Radio_StationText"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_StationText2"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_StationSignal"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_StationGenre"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_StationBitRate"]).Text = string.Empty;
            OMList list = (OMList)manager[screen]["List_RadioStations"];
            if (list.Count > 0)
            {
                list.SelectedIndex = -1;
                ((OMLabel)manager[screen]["Radio_StationName"]).Text = "Select a Station";
            }
            else
                ((OMLabel)manager[screen]["Radio_StationName"]).Text = "Select source...";
            ((OMLabel)manager[screen]["Radio_Status"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_Band"]).Text = string.Empty;
            ((OMLabel)manager[screen]["Radio_StationHD"]).Text = string.Empty;
        }

        private bool UpdateStationList(int instance)
        {
            //theHost.DebugMsg("Radio - UpdateStationList", "Updating station list.");

            // Get radio info
            tunedContentInfo info = theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString()) as tunedContentInfo;

            if (info == null)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                    ((OMLabel)manager[i]["Radio_StationName"]).Text = "No data available!";
                return false;
            }

            // Update list source label
            for (int i = 0; i < theHost.ScreenCount; i++)
                ((OMLabel)manager[i]["Label_StationListSource"]).Text = "Source: " + StationListSource.ToString();

            // Errorcheck
            if ((info.currentStation == null) || (info.stationList == null))
                return false;

            lock (this)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    // Update station list data
                    OMList List = (OMList)manager[i]["List_RadioStations"];
                    stationInfo[] Stations;
                    if ((List.Count == info.stationList.Length) && (StationListSource == StationListSources.Live))
                    {
                        Stations = info.stationList;
                        for (int j = 0; j < Stations.Length; j++)
                        {
                            List[j].text = Stations[j].stationName;
                            List[j].tag = Stations[j].stationID;
                        }
                        return true;
                    }
                    List.Clear();
                    if (StationListSource == StationListSources.Live)
                        Stations = info.stationList;
                    else
                    {
                        LoadPresets(instance);
                        Stations = Presets.ToArray();
                    }
                    if (Stations.Length == 0)
                    {
                        List.Add(new OMListItem("Loading . . ."));
                    }
                    else
                    {
                        foreach (stationInfo station in Stations)
                            List.Add(new OMListItem(station.stationName, (object)station.stationID));
                    }
                }
            }
            return true;
        }

        private void Message(int Screen, string Msg, int Time)
        {
            //theHost.DebugMsg("Radio - Message()", string.Format("Msg = {0}, Screen = {1}", Msg, Time));
            OMLabel Radio_StationName = (OMLabel)manager[Screen]["Radio_StationName"];
            OMLabel Info = (OMLabel)manager[Screen]["Info"];
            Radio_StationName.Visible = false;
            Info.Text = Msg;
            Info.Visible = true;
            Thread.Sleep(Time);
            Info.Text = "";
            Info.Visible = false;
            Radio_StationName.Visible = true;
        }

        #region Preset handling

        private void LoadPresets(int instance)
        {
            lock (Presets)
            {
                //object o = new object();
                //theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
                tunedContentInfo o = theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString()) as tunedContentInfo;
                if (o == null)
                    return;
                tunedContentInfo info = (tunedContentInfo)o;

                // Read from database
                Presets.Clear();
                using (PluginSettings setting = new PluginSettings())
                {
                    int PresetCount = 0;
                    while (PresetCount >= 0)
                    {
                        string DataName = "TunedContent." + info.band.ToString() + ".Preset" + PresetCount.ToString();
                        stationInfo station = new stationInfo();
                        station.stationName = setting.getSetting(this, DataName + ".StationName");
                        station.stationID = setting.getSetting(this, DataName + ".StationID");
                        if ((station.stationName == "") | (station.stationID == ""))
                            // No more presets available, exit
                            PresetCount = -1;
                        else
                        {
                            Presets.Add(station);
                            PresetCount++;
                        }
                    }
                }
            }
        }

        private void SaveToPresets(int instance, stationInfo Station)
        {
            lock (Presets)
            {
                // Save to local preset list
                stationInfo it = Presets.Find(i => i.stationName == Station.stationName);
                if (it != null)
                    Presets.Remove(it);
                Presets.Add(Station);

                //object o = new object();
                //theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
                tunedContentInfo o = theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString()) as tunedContentInfo;
                if (o == null)
                    return;
                tunedContentInfo info = (tunedContentInfo)o;

                // Sync list to dB
                SyncPresetsToDB(info.band.ToString());
            }
        }
        private void DeletePreset(int instance, stationInfo Station)
        {
            // Save to local preset list
            stationInfo it = Presets.Find(i => i.stationName == Station.stationName);
            if (it == null)
                return;
            Presets.Remove(it);            
            //object o = new object();
            //theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
            tunedContentInfo o = theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString()) as tunedContentInfo;
            if (o == null)
                return;
            tunedContentInfo info = (tunedContentInfo)o;

            // Sync list to dB
            SyncPresetsToDB(info.band.ToString());
        }
        private void SyncPresetsToDB(string band)
        {
            // Sync list to dB
            ClearPresetsInDB(band);
            DumpPresetListToDB(band);
        }
        private void ClearPresetsInDB(string band)
        {
            lock (Presets)
            {
                // Read from database
                using (PluginSettings setting = new PluginSettings())
                {
                    int PresetCount = 0;
                    while (PresetCount >= 0)
                    {
                        string DataName = "TunedContent." + band + ".Preset" + PresetCount.ToString();
                        stationInfo station = new stationInfo();
                        station.stationName = setting.getSetting(this, DataName + ".StationName");
                        station.stationID = setting.getSetting(this, DataName + ".StationID");
                        if ((station.stationName != "") | (station.stationID != ""))
                        {   // Clear data
                            setting.setSetting(this, DataName + ".StationName", "");
                            setting.setSetting(this, DataName + ".StationID", "");
                            // Continue to next data
                            PresetCount++;
                        }
                        else
                            // Cancel loop
                            PresetCount = -1;
                    }
                }
            }
        }
        private void DumpPresetListToDB(string band)
        {
            // Dump preset list to database
            using (PluginSettings setting = new PluginSettings())
            {
                for (int i = 0; i < Presets.Count; i++)
                {
                    string DataName = "TunedContent." + band + ".Preset" + i.ToString();
                    setting.setSetting(this, DataName + ".StationName", Presets[i].stationName);
                    setting.setSetting(this, DataName + ".StationID", Presets[i].stationID);
                }
            }
        }

        #endregion
    }
}
