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
using System.Drawing;
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenMobile.Data;

namespace OMRadio
{
    public class OMRadio : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        enum StationListSources { Live, Presets }
        StationListSources StationListSource = StationListSources.Live;
        List<stationInfo> Presets = new List<stationInfo>();

        #region IHighLevel Members

        public OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;

            if (name == "ListView")
                ConfigureListView(screen);
            if (name == "MessageBox")
                ConfigureMessageBox(screen);
            return manager[screen, name];
        }

        private void ConfigureListView(int screen)
        {
            OMPanel panel = manager[screen, "ListView"];
            switch ((string)panel.Tag)
            {
                case "source":
                    {
                        ((OMLabel)panel["Label_Header"]).Text = "Select tuned content source:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        object o;
                        theHost.getData(eGetData.GetPlugins, "", out o);
                        List<IBasePlugin> lst = (List<IBasePlugin>)o;
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(FontFamily.GenericSansSerif, 21F);
                        if (lst != null)
                        {
                            lst = lst.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
                            foreach (IBasePlugin b in lst)
                                //List_Source.Add(b.pluginName + " (" + b.pluginDescription + ")");
                                List_Source.Add(new OMListItem(b.pluginDescription, b.pluginName, format, b.pluginName));

                            List_Source.Add(new OMListItem("None", "", format, "unload"));
                        }
                        else
                        {
                            List_Source.Add(new OMListItem("No source available", "", format,"unload"));
                        }
                    }
                    break;
                case "band":
                    {
                        ((OMLabel)panel["Label_Header"]).Text = "Select band:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        object o;
                        theHost.getData(eGetData.GetSupportedBands, "", theHost.instanceForScreen(screen).ToString(), out o);
                        eTunedContentBand[] lst = (eTunedContentBand[])o;
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(FontFamily.GenericSansSerif, 21F);
                        if (lst != null)
                        {
                            foreach (eTunedContentBand b in lst)
                                //List_Source.Add(b.pluginName + " (" + b.pluginDescription + ")");
                                List_Source.Add(new OMListItem(b.ToString(), "", format, b));
                        }
                        else
                        {
                            List_Source.Add(new OMListItem("No band available", "", format,"unload"));
                        }
                    }
                    break;
                case "stationlist":
                    {
                        ((OMLabel)panel["Label_Header"]).Text = "Select station list source:";
                        OMList List_Source = (OMList)panel["ListView_List"];
                        List_Source.Clear();
                        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
                        format.color = Color.FromArgb(128, Color.White);
                        format.font = new Font(FontFamily.GenericSansSerif, 21F);
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

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }

        public string displayName
        {
            get { return "Radio"; }
        }

        #endregion

        #region IBasePlugin Members

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
            get { return "OMRadio"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Radio control panel"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel panelMain = new OMPanel("");
            theHost = host;
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);

            #region Main panel

            #region Buttons 

            OMButton Button_Source = new OMButton(10, 110, 150, 70);
            Button_Source.Name = "Button_Source";
            Button_Source.Image = theHost.getSkinImage("Full");
            Button_Source.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_Source.OnClick += new userInteraction(Button_Source_OnClick);
            Button_Source.Transition = eButtonTransition.None;
            Button_Source.Text = "Source";
            panelMain.addControl(Button_Source);

            OMButton Button_RadioBand = new OMButton(10, Button_Source.Top + Button_Source.Height + 10, 150, 70);
            Button_RadioBand.Name = "Button_Band";
            Button_RadioBand.Image = theHost.getSkinImage("Full");
            Button_RadioBand.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_RadioBand.OnClick += new userInteraction(Button_RadioBand_OnClick);
            Button_RadioBand.Transition = eButtonTransition.None;
            Button_RadioBand.Text = "Band";
            panelMain.addControl(Button_RadioBand);

            OMButton Button_RadioScan = new OMButton(10, Button_RadioBand.Top + Button_RadioBand.Height + 10, 150, 70);
            Button_RadioScan.Name = "Button_RadioScan";
            Button_RadioScan.Image = theHost.getSkinImage("Full");
            Button_RadioScan.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_RadioScan.OnClick += new userInteraction(Button_RadioAutoScan_OnClick);
            Button_RadioScan.Transition = eButtonTransition.None;
            Button_RadioScan.Text = "Scan";
            panelMain.addControl(Button_RadioScan);

            OMButton Button_ChannelListSource = new OMButton(10, Button_RadioScan.Top + Button_RadioScan.Height + 10, 150, 70);
            Button_ChannelListSource.Name = "Button_ChannelListSource";
            Button_ChannelListSource.Image = theHost.getSkinImage("Full");
            Button_ChannelListSource.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_ChannelListSource.OnClick += new userInteraction(Button_ChannelListSource_OnClick);
            Button_ChannelListSource.Transition = eButtonTransition.None;
            Button_ChannelListSource.Text = "List\nsource";
            panelMain.addControl(Button_ChannelListSource);

            #endregion

            #region RadioInfo field

            OMBasicShape Shape_StationInfoBorder = new OMBasicShape(180, 110, 800, 110);
            Shape_StationInfoBorder.Name = "Shape_StationInfoBorder";
            Shape_StationInfoBorder.Shape = shapes.RoundedRectangle;
            Shape_StationInfoBorder.FillColor = Color.FromArgb(58, 58, 58);
            Shape_StationInfoBorder.BorderColor = Color.Gray;
            Shape_StationInfoBorder.BorderSize = 2;
            panelMain.addControl(Shape_StationInfoBorder);

            OMLabel Radio_StationName = new OMLabel(Shape_StationInfoBorder.Left + 10, Shape_StationInfoBorder.Top + 5, Shape_StationInfoBorder.Width - 20 , 40);
            Radio_StationName.Name = "Radio_StationName";
            Radio_StationName.TextAlignment = Alignment.CenterLeft;
            Radio_StationName.Font = new Font("Microsoft Sans Serif", 24F);
            Radio_StationName.Format = eTextFormat.Bold;
            Radio_StationName.Text = "Select source...";
            panelMain.addControl(Radio_StationName);

            OMLabel Info = (OMLabel)Radio_StationName.Clone();
            Info.Name = "Info";
            Info.Text = "";
            Info.Visible = false;
            panelMain.addControl(Info);

            OMLabel Radio_StationText = new OMLabel(Radio_StationName.Left + 10, Radio_StationName.Top + Radio_StationName.Height, Radio_StationName.Width - 10, 30);
            Radio_StationText.Name = "Radio_StationText";
            Radio_StationText.TextAlignment = Alignment.CenterLeft;
            Radio_StationText.Font = new Font("Microsoft Sans Serif", 14F);
            Radio_StationText.Format = eTextFormat.Normal;
            Radio_StationText.Text = "";
            panelMain.addControl(Radio_StationText);

            OMLabel Radio_StationGenre = new OMLabel(Shape_StationInfoBorder.Left + 20, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 200, 20);
            Radio_StationGenre.Name = "Radio_StationGenre";
            Radio_StationGenre.TextAlignment = Alignment.CenterLeft;
            Radio_StationGenre.Font = new Font("Microsoft Sans Serif", 12F);
            Radio_StationGenre.Format = eTextFormat.Normal;
            Radio_StationGenre.Text = "";
            panelMain.addControl(Radio_StationGenre);

            OMLabel Radio_StationBitRate = new OMLabel(Shape_StationInfoBorder.Left + Shape_StationInfoBorder.Width - 580, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 80, 20);
            Radio_StationBitRate.TextAlignment = Alignment.CenterCenter;
            Radio_StationBitRate.Font = new Font("Microsoft Sans Serif", 12F);
            Radio_StationBitRate.Format = eTextFormat.Normal;
            Radio_StationBitRate.Name = "Radio_StationBitRate";
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

            OMBasicShape Shape_ChannelListBorder = new OMBasicShape(Shape_StationInfoBorder.Left, Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height + 10, Shape_StationInfoBorder.Width, 290);
            Shape_ChannelListBorder.Name = "Shape_ChannelListBorder";
            Shape_ChannelListBorder.Shape = shapes.RoundedRectangle;
            Shape_ChannelListBorder.FillColor = Color.FromArgb(58, 58, 58);
            Shape_ChannelListBorder.BorderColor = Color.Gray;
            Shape_ChannelListBorder.BorderSize = 2;
            panelMain.addControl(Shape_ChannelListBorder);

            OMLabel Label_StationListHeader = new OMLabel(Shape_ChannelListBorder.Left + 5, Shape_ChannelListBorder.Top, Shape_ChannelListBorder.Width - 10, 30);
            Label_StationListHeader.Name = "Label_StationListHeader";
            Label_StationListHeader.TextAlignment = Alignment.CenterLeft;
            Label_StationListHeader.Font = new Font("Microsoft Sans Serif", 14F);
            Label_StationListHeader.Format = eTextFormat.Bold;
            Label_StationListHeader.Text = "Station list:";
            panelMain.addControl(Label_StationListHeader);

            OMLabel Label_StationListSource = new OMLabel(Shape_ChannelListBorder.Left + Shape_ChannelListBorder .Width - 205, Shape_ChannelListBorder.Top, 200, 30);
            Label_StationListSource.Name = "Label_StationListSource";
            Label_StationListSource.TextAlignment = Alignment.CenterRight;
            Label_StationListSource.Font = new Font("Microsoft Sans Serif", 14F);
            Label_StationListSource.Format = eTextFormat.Normal;
            Label_StationListSource.Text = "Source: Live";
            panelMain.addControl(Label_StationListSource);

            OMList List_RadioStations = new OMList(Shape_ChannelListBorder.Left + 5, Label_StationListHeader.Top + Label_StationListHeader.Height + 10, Shape_ChannelListBorder.Width - 10, Shape_ChannelListBorder.Height - 60);
            List_RadioStations.ListStyle = eListStyle.RoundedTextList;
            List_RadioStations.ItemColor1 = Color.Transparent;//Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 66);
            List_RadioStations.ItemColor2 = Color.Transparent;//Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 10);
            List_RadioStations.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List_RadioStations.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
            List_RadioStations.Name = "List_RadioStations";
            List_RadioStations.Font = new Font("Microsoft Sans Serif", 24F);
            List_RadioStations.Add("No channels available");
            List_RadioStations.TextAlignment = Alignment.CenterLeft;
            List_RadioStations.OnClick += new userInteraction(List_RadioStations_OnClick);
            List_RadioStations.OnLongClick += new userInteraction(List_RadioStations_OnLongClick);
            panelMain.addControl(List_RadioStations);

            #endregion

            #endregion

            #region ListView panel

            OMPanel panelListView = new OMPanel("ListView");
            panelListView.Tag = "source";

            OMBasicShape Shape_AccessBlock = new OMBasicShape(0, 0, 1000, 600);
            Shape_AccessBlock.Name = "Shape_AccessBlock";
            Shape_AccessBlock.Shape = shapes.Rectangle;
            Shape_AccessBlock.FillColor = Color.FromArgb(150, Color.Black);
            panelListView.addControl(Shape_AccessBlock);
            OMButton Button_Cancel = new OMButton(0, 0, 1000, 600);
            Button_Cancel.Name = "Button_Cancel";
            Button_Cancel.OnClick += new userInteraction(Button_Cancel_OnClick);
            panelListView.addControl(Button_Cancel);

            OMBasicShape Shape_Border = new OMBasicShape(200, 120, 600, 400);
            Shape_Border.Name = "Shape_Border";
            Shape_Border.Shape = shapes.RoundedRectangle;
            Shape_Border.FillColor = Color.FromArgb(58, 58, 58);
            Shape_Border.BorderColor = Color.Gray;
            Shape_Border.BorderSize = 2;
            panelListView.addControl(Shape_Border);

            OMLabel Label_Header = new OMLabel(Shape_Border.Left + 5, Shape_Border.Top + 5, Shape_Border.Width - 10, 30);
            Label_Header.Name = "Label_Header";
            Label_Header.Text = "Select tuned content source:";
            panelListView.addControl(Label_Header);

            OMBasicShape Shape_Line = new OMBasicShape(Shape_Border.Left, Label_Header.Top + Label_Header.Height, Shape_Border.Width, 2);
            Shape_Line.Name = "Shape_Line";
            Shape_Line.Shape = shapes.Rectangle;
            Shape_Line.FillColor = Color.Gray;
            Shape_Line.BorderColor = Color.Transparent;
            panelListView.addControl(Shape_Line);

            OMBasicShape Shape_Background = new OMBasicShape(Shape_Border.Left + (int)Shape_Border.BorderSize, Shape_Line.Top + Shape_Line.Height, Shape_Border.Width - ((int)Shape_Border.BorderSize * 2), 355);
            Shape_Background.Name = "Shape_Background";
            Shape_Background.Shape = shapes.Rectangle;
            Shape_Background.FillColor = Color.Black;
            panelListView.addControl(Shape_Background);
            OMBasicShape Shape_Background_Lower = new OMBasicShape(Shape_Background.Left, Shape_Background.Top + Shape_Background.Height - 13, Shape_Background.Width, 20);
            Shape_Background_Lower.Name = "Shape_Background_Lower";
            Shape_Background_Lower.Shape = shapes.RoundedRectangle;
            Shape_Background_Lower.FillColor = Color.Black;
            panelListView.addControl(Shape_Background_Lower);

            OMList ListView_List = new OMList(Label_Header.Left, Shape_Background.Top + 10, Label_Header.Width, Shape_Background.Height - 10);
            ListView_List.Name = "ListView_List";
            ListView_List.ListStyle = eListStyle.MultiList;
            ListView_List.Background = Color.Silver;
            ListView_List.ItemColor1 = Color.Black;
            ListView_List.Font = new Font(FontFamily.GenericSansSerif, 30F);
            ListView_List.Color = Color.White;
            ListView_List.HighlightColor = Color.White;
            ListView_List.SelectedItemColor1 = Color.DarkBlue;
            OMListItem.subItemFormat format = new OMListItem.subItemFormat();
            format.color = Color.FromArgb(128, Color.White);
            format.font = new Font(FontFamily.GenericSansSerif, 21F);
            ListView_List.Add(new OMListItem("No source available!", "", format));
            ListView_List.OnClick += new userInteraction(ListView_List_OnClick);
            panelListView.addControl(ListView_List);

            #endregion

            #region MessageBox panel

            OMPanel panelMessageBox = new OMPanel("MessageBox");
            panelMessageBox.Tag = "preset";

            OMBasicShape Shape_AccessBlock2 = new OMBasicShape(0, 0, 1000, 600);
            Shape_AccessBlock2.Name = "MessageBox_Shape_AccessBlock";
            Shape_AccessBlock2.Shape = shapes.Rectangle;
            Shape_AccessBlock2.FillColor = Color.FromArgb(150, Color.Black);
            panelMessageBox.addControl(Shape_AccessBlock2);
            OMButton Button_Cancel2 = new OMButton(0, 0, 1000, 600);
            Button_Cancel2.Name = "MessageBox_Button_Cancel";
            Button_Cancel2.OnClick += new userInteraction(Button_Cancel_OnClick);
            panelMessageBox.addControl(Button_Cancel2);

            OMBasicShape Shape_Border2 = new OMBasicShape(250, 175, 500, 250);
            Shape_Border2.Name = "MessageBox_Shape_Border";
            Shape_Border2.Shape = shapes.RoundedRectangle;
            Shape_Border2.FillColor = Color.FromArgb(58, 58, 58);
            Shape_Border2.BorderColor = Color.Gray;
            Shape_Border2.BorderSize = 2;
            panelMessageBox.addControl(Shape_Border2);

            OMLabel Label_Header2 = new OMLabel(Shape_Border2.Left + 5, Shape_Border2.Top + 5, Shape_Border2.Width - 10, 30);
            Label_Header2.Name = "MessageBox_Label_Header";
            Label_Header2.Text = "Delete preset?";
            panelMessageBox.addControl(Label_Header2);

            OMBasicShape Shape_Line2 = new OMBasicShape(Shape_Border2.Left, Label_Header2.Top + Label_Header2.Height, Shape_Border2.Width, 2);
            Shape_Line2.Name = "MessageBox_Shape_Line";
            Shape_Line2.Shape = shapes.Rectangle;
            Shape_Line2.FillColor = Color.Gray;
            Shape_Line2.BorderColor = Color.Transparent;
            panelMessageBox.addControl(Shape_Line2);

            OMBasicShape Shape_Background2 = new OMBasicShape(Shape_Border2.Left + (int)Shape_Border2.BorderSize, Shape_Line2.Top + Shape_Line2.Height, Shape_Border2.Width - ((int)Shape_Border2.BorderSize * 2), 205);
            Shape_Background2.Name = "MessageBox_Shape_Background";
            Shape_Background2.Shape = shapes.Rectangle;
            Shape_Background2.FillColor = Color.Black;
            panelMessageBox.addControl(Shape_Background2);
            OMBasicShape Shape_Background_Lower2 = new OMBasicShape(Shape_Background2.Left, Shape_Background2.Top + Shape_Background2.Height - 13, Shape_Background2.Width, 20);
            Shape_Background_Lower2.Name = "MessageBox_Shape_Background_Lower";
            Shape_Background_Lower2.Shape = shapes.RoundedRectangle;
            Shape_Background_Lower2.FillColor = Color.Black;
            panelMessageBox.addControl(Shape_Background_Lower2);

            OMLabel Label_Info = new OMLabel(Shape_Border2.Left + 5, Shape_Background2.Top + 5, Shape_Border2.Width - 10, 135);
            Label_Info.Name = "MessageBox_Label_Info";
            Label_Info.Text = "";
            panelMessageBox.addControl(Label_Info);

            OMButton Button_Yes = new OMButton(Shape_Border2.Left + Shape_Border2.Width - 320, Shape_Border2.Top + Shape_Border2.Height - 80, 150, 70);
            Button_Yes.Name = "MessageBox_Button_Yes";
            Button_Yes.Image = theHost.getSkinImage("Full");
            Button_Yes.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_Yes.OnClick += new userInteraction(Button_Yes_OnClick);
            Button_Yes.Transition = eButtonTransition.None;
            Button_Yes.Text = "Yes";
            panelMessageBox.addControl(Button_Yes);

            OMButton Button_No = new OMButton(Button_Yes.Left + Button_Yes.Width + 10, Button_Yes.Top, Button_Yes.Width, Button_Yes.Height);
            Button_No.Name = "MessageBox_Button_No";
            Button_No.Image = theHost.getSkinImage("Full");
            Button_No.FocusImage = theHost.getSkinImage("Full.Highlighted");
            Button_No.OnClick += new userInteraction(Button_No_OnClick);
            Button_No.Transition = eButtonTransition.None;
            Button_No.Text = "No";
            panelMessageBox.addControl(Button_No);

            #endregion

            manager = new ScreenManager(theHost.ScreenCount);
            manager.loadPanel(panelMain);
            manager.loadPanel(panelListView);
            manager.loadPanel(panelMessageBox);
            return eLoadStatus.LoadSuccessful;
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
                        DeletePreset(theHost.instanceForScreen(screen), station);
                        UpdateStationList(theHost.instanceForScreen(screen));
                    }
                    break;
            }
        }

        void List_RadioStations_OnLongClick(OMControl sender, int screen)
        {
            if (StationListSource == StationListSources.Live)
            {
                System.Media.SystemSounds.Beep.Play();
                Message(screen, "Preset saved", 1000);
                stationInfo station = new stationInfo(((OMList)sender).SelectedItem.text, (string)((OMList)sender).SelectedItem.tag);
                SaveToPresets(theHost.instanceForScreen(screen), station);
            }

            if (StationListSource == StationListSources.Presets)
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
            if (((OMList)sender).SelectedItem.tag == null)
                return;
            else
                SelectedItemTag = ((OMList)sender).SelectedItem.tag.ToString();
            theHost.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.None.ToString());
            switch (PanelTag)
            {
                case "source":
                    {
                        if (SelectedItemTag == "unload")
                        {
                            theHost.execute(eFunction.unloadTunedContent, theHost.instanceForScreen(screen).ToString());
                            for (int i = 0; i < theHost.ScreenCount; i++)
                                ClearStationInfo(i);
                        }
                        else
                        {
                            for (int i = 0; i < theHost.ScreenCount; i++)
                                ((OMLabel)manager[i]["Radio_StationName"]).Text = "Loading...";
                            if (theHost.execute(eFunction.loadTunedContent, theHost.instanceForScreen(screen).ToString(), SelectedItemTag))
                            {
                                UpdateStationList(theHost.instanceForScreen(screen));
                            }
                            else
                            {
                                for (int i = 0; i < theHost.ScreenCount; i++)
                                    ((OMLabel)manager[i]["Radio_StationName"]).Text = "Load failed!";
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
                        if (theHost.execute(eFunction.setBand, theHost.instanceForScreen(screen).ToString(), SelectedItemTag))
                        {
                            UpdateStationList(theHost.instanceForScreen(screen));
                        }

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
                        UpdateStationList(theHost.instanceForScreen(screen));
                    }
                    break;
                default:
                    break;
            }
        }

        void List_RadioStations_OnClick(OMControl sender, int screen)
        {
            OMList List = (OMList)sender;
            if (List.SelectedItem.tag != null)
                theHost.execute(eFunction.tuneTo, theHost.instanceForScreen(screen).ToString(), (string)List.SelectedItem.tag);
        }

        void Button_RadioAutoScan_OnClick(OMControl sender, int screen)
        {
            //theHost.sendMessage("OMRadioVenice5", this.pluginName, "autoscan");
            //theHost.sendMessage("OMRadioVenice5", this.pluginName, "getstationlist");
            theHost.execute(eFunction.scanBand, theHost.instanceForScreen(screen).ToString());
        }

        void TextBox_RadioTuneTo_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
            ((OMTextBox)sender).Text = input.getText(screen, this.pluginName, "");
        }

        void Button_RadioTuneTo_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.tuneTo, theHost.instanceForScreen(screen).ToString(), ((OMLabel)manager[screen]["TextBox_RadioTuneTo"]).Text);
        }

        void Button_RadioBand_OnClick(OMControl sender, int screen)
        {
            ((OMPanel)manager[screen, "ListView"]).Tag = "band";
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "ListView");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            ((OMButton)sender).Transition = eButtonTransition.None;

            /*
            object o = new object();
            theHost.getData(eGetData.GetTunedContentInfo, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null) return;
            tunedContentInfo ContentInfo = (tunedContentInfo)o;

            theHost.getData(eGetData.GetSupportedBands, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null) return;
            tunedContentBand[] supportedBands = (tunedContentBand[])o;

            // Find current band in supportedbands array
            //supportedBands.
            
            if (ContentInfo.band == tunedContentBand.FM)
                theHost.execute(eFunction.setBand, theHost.instanceForScreen(screen).ToString(), tunedContentBand.DAB.ToString());
            else
                theHost.execute(eFunction.setBand, theHost.instanceForScreen(screen).ToString(), tunedContentBand.FM.ToString());
            */
        }

        void theHost_OnMediaEvent(eFunction function, int instance, string arg)
        {
            if ((function == eFunction.Play)||(function==eFunction.tunerDataUpdated))
            {
                UpdateStationInfo(instance);
            }

            if (function == eFunction.stationListUpdated)
            {
                UpdateStationList(instance);
            }
        }

        void Button_Source_OnClick(OMControl sender, int screen)
        {
            ((OMPanel)manager[screen, "ListView"]).Tag = "source";
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "ListView");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None.ToString());
            ((OMButton)sender).Transition = eButtonTransition.None;



            /*
            if (theHost.execute(eFunction.unloadTunedContent, theHost.instanceForScreen(screen).ToString()) == false)
            {
                if (!theHost.execute(eFunction.loadTunedContent, theHost.instanceForScreen(screen).ToString(), "OMRadioVenice5"))
                {   // Error while activating radio 
                    theHost.execute(eFunction.backgroundOperationStatus, "Unable to activate radio!");
                    ((OMButton)sender).Text = "On";
                }
                else
                {
                    ((OMButton)sender).Text = "Off";
                    theHost.execute(eFunction.backgroundOperationStatus, "Radio turned on");
                }
            }
            else
            {   // Radio turned off
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    ((OMButton)sender).Text = "On";
                    theHost.execute(eFunction.backgroundOperationStatus, "Radio turned off");
                }
            }
            */
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
            ((OMLabel)manager[screen]["Radio_StationName"]).Text = "Select source...";
            ((OMLabel)manager[screen]["Radio_StationFrequency"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationSignal"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationGenre"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationBitRate"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationText"]).Text = "";
            ((OMLabel)manager[screen]["Radio_Status"]).Text = "";
            ((OMLabel)manager[screen]["Radio_Band"]).Text = "";
            ((OMLabel)manager[screen]["Radio_StationHD"]).Text = "";
            OMList List = (OMList)manager[screen]["List_RadioStations"];
            List.Clear();
        }
        private void UpdateStationInfo(int instance)
        {
            object o = new object();
            theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
            if (o == null)
                return;
            tunedContentInfo info = (tunedContentInfo)o;

            mediaInfo mediaInfo = theHost.getPlayingMedia(instance);

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                ((OMLabel)manager[i]["Radio_StationName"]).Text = info.currentStation.stationName;

                #region Frequency

                if (((info.band == eTunedContentBand.AM) | (info.band == eTunedContentBand.FM)) && (info.currentStation.stationID.IndexOf(':') < (info.currentStation.stationID.Length-1)))
                {
                    int Frequency = int.Parse(info.currentStation.stationID.Substring(info.currentStation.stationID.IndexOf(':') + 1));
                    ((OMLabel)manager[i]["Radio_StationFrequency"]).Text = (Frequency / 1000.0F).ToString("##.00") + "MHz";
                }
                else
                {
                    ((OMLabel)manager[i]["Radio_StationFrequency"]).Text = "";
                }

                #endregion

                #region Signal strength

                switch (info.currentStation.signal)
                {
                    default:
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

                ((OMLabel)manager[i]["Radio_StationGenre"]).Text = info.currentStation.stationGenre;
                ((OMLabel)manager[i]["Radio_StationBitRate"]).Text = (info.currentStation.Bitrate > 0 ? info.currentStation.Bitrate.ToString() + "Kbps" : "");
                ((OMLabel)manager[i]["Radio_StationText"]).Text = mediaInfo.Name;
                ((OMLabel)manager[i]["Radio_Status"]).Text = info.status.ToString();
                ((OMLabel)manager[i]["Radio_Band"]).Text = info.band.ToString();
                ((OMLabel)manager[i]["Radio_StationHD"]).Text = (info.currentStation.HD ? "HD" : "");

            }
        }
        private void UpdateStationList(int instance)
        {
            object o = new object();
            theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
            if (o == null)
                return;
            tunedContentInfo info = (tunedContentInfo)o;

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                // Update station list data
                OMList List = (OMList)manager[i]["List_RadioStations"];
                stationInfo[] Stations;
                if ((List.Count == info.stationList.Length) && (StationListSource == StationListSources.Live))
                {
                    Stations=info.stationList;
                    for (int j = 0; j < Stations.Length;j++)
                    {
                        List[j].text = Stations[j].stationName;
                        List[j].tag = Stations[j].stationID;
                    }
                    return;
                }
                List.Clear();
                if (StationListSource == StationListSources.Live)
                    Stations = info.stationList;
                else
                {
                    LoadPresets(instance);
                    Stations = Presets.ToArray();
                }

                foreach (stationInfo station in Stations)
                    List.Add(new OMListItem(station.stationName, (object)station.stationID));
            }
        }

        private void Message(int Screen, string Msg, int Time)
        {
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
                object o = new object();
                theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
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
                        station.stationName = setting.getSetting(DataName + ".StationName");
                        station.stationID = setting.getSetting(DataName + ".StationID");
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

                object o = new object();
                theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
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
            object o = new object();
            theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
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
                        station.stationName = setting.getSetting(DataName + ".StationName");
                        station.stationID = setting.getSetting(DataName + ".StationID");
                        if ((station.stationName != "") | (station.stationID != ""))
                        {   // Clear data
                            setting.setSetting(DataName + ".StationName", "");
                            setting.setSetting(DataName + ".StationID", "");
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
            for (int i = 0; i < Presets.Count; i++)
            {
                string DataName = "TunedContent." + band + ".Preset" + i.ToString();
                using (PluginSettings setting = new PluginSettings())
                {
                    setting.setSetting(DataName + ".StationName", Presets[i].stationName);
                    setting.setSetting(DataName + ".StationID", Presets[i].stationID);
                }
            }
        }

        #endregion
    }
}
