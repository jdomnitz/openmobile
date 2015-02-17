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
using OpenMobile.Graphics;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.Plugin;
using OpenMobile.Media;
using OpenMobile.Data;
using OpenMobile.helperFunctions.Forms;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OMRadio2
{
    internal class panelMain
    {
        private HighLevelCode _MainPlugin;
        private MenuPopup PopupMenu;
        public const string PanelName = "Main";
        private OMListItem.subItemFormat _MediaListSubItemFormat = new OMListItem.subItemFormat();

        public panelMain(HighLevelCode mainPlugin)
        {
            _MainPlugin = mainPlugin;
        }

        public OMPanel Initialize()
        {
            // Create a new panel
            OMPanel panel = new OMPanel(PanelName, "Radio", _MainPlugin.pluginIcon);

            //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
            //    new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            //shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            //panel.addControl(shapeBackground);

            #region Control buttons

            var yOffset = 20;
            var size = new Size(230, 90);

            OMButton Button_ListSource = OMButton.PreConfigLayout_CleanStyle("Button_ListSource", OM.Host.ClientArea_Init.Left - 40, OM.Host.ClientArea_Init.Top + 25, size.Width, size.Height);
            Button_ListSource.Text = "Preset/Live";
            Button_ListSource.Command_Click = "Screen{:S:}.Zone.MediaSource.Command.SetListSource";
            panel.addControl(Button_ListSource);

            OMButton Button_Scan = OMButton.PreConfigLayout_CleanStyle("Button_Scan", 0, 0, size.Width, size.Height);
            Button_Scan.Text = "Search\nChannels";
            Button_Scan.OnClick += new userInteraction(Button_Scan_OnClick);
            panel.addControl(Button_Scan, ControlDirections.Down, 0, yOffset);

            OMButton Button_TuneTo = OMButton.PreConfigLayout_CleanStyle("Button_TuneTo", 0, 0, size.Width, size.Height);
            Button_TuneTo.Text = "Direct\nTune";
            Button_TuneTo.OnClick += new userInteraction(Button_TuneTo_OnClick);
            panel.addControl(Button_TuneTo, ControlDirections.Down, 0, yOffset);

            OMButton Button_Band = OMButton.PreConfigLayout_CleanStyle("Button_Band", 0, 0, size.Width, size.Height);
            Button_Band.Text = "Band";
            Button_Band.OnClick += new userInteraction(Button_Band_OnClick);
            panel.addControl(Button_Band, ControlDirections.Down, 0, yOffset);

            #endregion

            #region RadioInfo field

            OMBasicShape Shape_StationInfoBorder = new OMBasicShape("Shape_StationInfoBorder", OM.Host.ClientArea_Init.Left + 225, OM.Host.ClientArea_Init.Top + 10, OM.Host.ClientArea_Init.Width - 245, 100,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            panel.addControl(Shape_StationInfoBorder);

            OMLabel Radio_StationName = new OMLabel("Radio_StationName", Shape_StationInfoBorder.Left + 10, Shape_StationInfoBorder.Top + 5, Shape_StationInfoBorder.Width - 20, 40);
            Radio_StationName.TextAlignment = Alignment.CenterLeft;
            Radio_StationName.Font = new Font(Font.GenericSansSerif, 28F);
            Radio_StationName.Format = eTextFormat.Normal;
            Radio_StationName.Text = "";
            Radio_StationName.DataSource = "Screen{:S:}.Zone.MediaInfo.Name";
            panel.addControl(Radio_StationName);

            OMLabel Radio_StationText = new OMLabel("Radio_StationText", Radio_StationName.Left + 10, Radio_StationName.Region.Bottom, Radio_StationName.Width - 10, 30);
            Radio_StationText.TextAlignment = Alignment.CenterLeft;
            Radio_StationText.Font = new Font(Font.GenericSansSerif, 18F);
            Radio_StationText.Format = eTextFormat.Normal;
            Radio_StationText.Text = "";
            Radio_StationText.Opacity = 127;
            Radio_StationText.DataSource = "Screen{:S:}.Zone.MediaInfo.Artist";
            panel.addControl(Radio_StationText);

            // Small text info along bottom of box
            OMLabel Radio_Infobar = new OMLabel("Radio_Infobar", Shape_StationInfoBorder.Region.Right - 210, Shape_StationInfoBorder.Region.Top + 5, 200, 20);
            Radio_Infobar.TextAlignment = Alignment.CenterRight;
            Radio_Infobar.Font = new Font(Font.GenericSansSerif, 14F);
            Radio_Infobar.Text = "{Screen{:S:}.Zone.MediaSource.Data.ChannelID} {Screen{:S:}.Zone.MediaSource}";
            Radio_Infobar.Visible_DataSource = "Screen{:S:}.Zone.MediaSource";
            Radio_Infobar.Opacity = 127;
            panel.addControl(Radio_Infobar);

            OMLabel Radio_Infobar2 = new OMLabel("Radio_Infobar2", Shape_StationInfoBorder.Region.Right - 405, Shape_StationInfoBorder.Region.Bottom - 20, 400, 20);
            Radio_Infobar2.TextAlignment = Alignment.CenterRight;
            Radio_Infobar2.Font = new Font(Font.GenericSansSerif, 12F);
            Radio_Infobar2.Opacity = 50;
            Radio_Infobar2.Text = "Hold channel name to save current channel to presets";
            panel.addControl(Radio_Infobar2);

            // Add invisible button over radio station to save channel to presets
            OMButton Button_SavePreset = new OMButton("Button_SavePreset", Shape_StationInfoBorder.Region.Left, Shape_StationInfoBorder.Region.Top, Shape_StationInfoBorder.Region.Width, Shape_StationInfoBorder.Region.Height);
            Button_SavePreset.OnHoldClick += new userInteraction(Button_SavePreset_OnHoldClick);
            panel.addControl(Button_SavePreset);

            //OMLabel Radio_StationText2 = new OMLabel("Radio_StationText2", Radio_StationText.Left + 10, Radio_StationText.Top + Radio_StationText.Height, Radio_StationText.Width - 10, 30);
            //Radio_StationText2.Name = "Radio_StationText2";
            //Radio_StationText2.TextAlignment = Alignment.CenterLeft;
            //Radio_StationText2.Font = new Font(Font.GenericSansSerif, 14F);
            //Radio_StationText2.Format = eTextFormat.Normal;
            //Radio_StationText2.Text = "Text2";
            //panel.addControl(Radio_StationText2);

            //// Small text info along bottom of box
            //OMLabel Radio_StationGenre = new OMLabel("Radio_StationGenre", Shape_StationInfoBorder.Left + 20, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 200, 20);
            //Radio_StationGenre.Name = "Radio_StationGenre";
            //Radio_StationGenre.TextAlignment = Alignment.CenterLeft;
            //Radio_StationGenre.Font = new Font(Font.GenericSansSerif, 12F);
            //Radio_StationGenre.Format = eTextFormat.Normal;
            //Radio_StationGenre.Text = "";
            //Radio_StationGenre.DataSource = "Screen{:S:}.Zone.MediaInfo.Genre";
            //panel.addControl(Radio_StationGenre);

            //OMLabel Radio_StationBitRate = new OMLabel("Radio_StationBitRate", Shape_StationInfoBorder.Left + Shape_StationInfoBorder.Width - 580, (Shape_StationInfoBorder.Top + Shape_StationInfoBorder.Height) - 25, 80, 20);
            //Radio_StationBitRate.Name = "Radio_StationBitRate";
            //Radio_StationBitRate.TextAlignment = Alignment.CenterCenter;
            //Radio_StationBitRate.Font = new Font(Font.GenericSansSerif, 12F);
            //Radio_StationBitRate.Format = eTextFormat.Normal;
            //Radio_StationBitRate.DataSource = "Screen{:S:}.Zone.MediaSource.Data.SignalBitRate";
            //panel.addControl(Radio_StationBitRate);

            //OMLabel Radio_StationHD = (OMLabel)Radio_StationBitRate.Clone();
            //Radio_StationHD.Left = Radio_StationBitRate.Left + Radio_StationBitRate.Width + 10;
            //Radio_StationHD.Width = 50;
            //Radio_StationHD.Name = "Radio_StationHD";
            ////Radio_StationHD.Visible = false;
            //Radio_StationHD.Text = "HD";
            //Radio_StationHD.Visible_DataSource = "Screen{:S:}.Zone.MediaSource.Data.HD";
            //panel.addControl(Radio_StationHD);

            //OMLabel Radio_StationSignal = (OMLabel)Radio_StationBitRate.Clone();
            //Radio_StationSignal.Left = Radio_StationHD.Left + Radio_StationHD.Width + 10;
            //Radio_StationSignal.Width = 140;
            //Radio_StationSignal.Name = "Radio_StationSignal";
            //Radio_StationSignal.Text = "";
            //Radio_StationSignal.DataSource = "Screen{:S:}.Zone.MediaSource.Data.SignalStrength";
            //panel.addControl(Radio_StationSignal);

            //OMLabel Radio_StationFrequency = (OMLabel)Radio_StationBitRate.Clone();
            //Radio_StationFrequency.Left = Radio_StationSignal.Left + Radio_StationSignal.Width + 10;
            //Radio_StationFrequency.Width = 110;
            //Radio_StationFrequency.Name = "Radio_StationFrequency";
            //Radio_StationFrequency.Text = "";
            //Radio_StationFrequency.DataSource = "Screen{:S:}.Zone.MediaSource.Data.ChannelID";
            //panel.addControl(Radio_StationFrequency);

            //OMLabel Radio_Status = (OMLabel)Radio_StationBitRate.Clone();
            //Radio_Status.Left = Radio_StationFrequency.Left + Radio_StationFrequency.Width + 10;
            //Radio_Status.Width = 110;
            //Radio_Status.Name = "Radio_Status";
            //Radio_Status.Text = "";
            //Radio_Status.DataSource = "Screen{:S:}.Zone.MediaSource.Data.DeviceStatus";
            //panel.addControl(Radio_Status);

            //OMLabel Radio_Band = (OMLabel)Radio_StationBitRate.Clone();
            //Radio_Band.Left = Shape_StationInfoBorder.Left + Shape_StationInfoBorder.Width - 85;
            //Radio_Band.TextAlignment = Alignment.CenterRight;
            //Radio_Band.Width = 80;
            //Radio_Band.Name = "Radio_Source";
            //Radio_Band.Text = "";
            //Radio_Band.DataSource = "Screen{:S:}.Zone.MediaSource";
            //panel.addControl(Radio_Band);



            //OMLabel Radio_StationFrequency = new OMLabel("Radio_StationFrequency", Shape_StationInfoBorder.Region.Right - 200, Shape_StationInfoBorder.Region.Bottom - 20, 100, 20);
            //Radio_StationFrequency.TextAlignment = Alignment.CenterLeft;
            //Radio_StationFrequency.Font = new Font(Font.GenericSansSerif, 14F);
            //Radio_StationFrequency.Text = "<Freq>";
            //Radio_StationFrequency.DataSource = "Screen{:S:}.Zone.MediaSource.Data.ChannelID";
            //panel.addControl(Radio_StationFrequency);

            //OMLabel Radio_Source = new OMLabel("Radio_Source");
            //Radio_Source.TextAlignment = Alignment.CenterLeft;
            //Radio_Source.Font = new Font(Font.GenericSansSerif, 14F);
            //Radio_Source.Text = "<Source>";
            //Radio_Source.DataSource = "Screen{:S:}.Zone.MediaSource";
            //panel.addControl(Radio_Source, ControlDirections.Right, ControlSizeControl.SameSize);

            #endregion

            #region Channel List

            OMBasicShape Shape_ChannelListBorder = new OMBasicShape("Shape_ChannelListBorder", Shape_StationInfoBorder.Left, Shape_StationInfoBorder.Region.Bottom + 10, Shape_StationInfoBorder.Width, OM.Host.ClientArea_Init.Bottom - 30 - Shape_StationInfoBorder.Region.Bottom + 10,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            panel.addControl(Shape_ChannelListBorder);

            OMLabel Label_StationListHeader = new OMLabel("Label_StationListHeader", Shape_ChannelListBorder.Left + 5, Shape_ChannelListBorder.Top, Shape_ChannelListBorder.Width - 10, 30);
            Label_StationListHeader.Name = "Label_StationListHeader";
            Label_StationListHeader.TextAlignment = Alignment.CenterLeft;
            Label_StationListHeader.Font = new Font(Font.GenericSansSerif, 14F);
            Label_StationListHeader.Format = eTextFormat.Normal;
            Label_StationListHeader.Text = "Channel list:";
            Label_StationListHeader.Opacity = 127;
            panel.addControl(Label_StationListHeader);

            OMLabel Label_StationListSource = new OMLabel("Label_StationListSource", Shape_ChannelListBorder.Left + Shape_ChannelListBorder.Width - 205, Shape_ChannelListBorder.Top, 200, 30);
            Label_StationListSource.Name = "Label_StationListSource";
            Label_StationListSource.TextAlignment = Alignment.CenterRight;
            Label_StationListSource.Font = new Font(Font.GenericSansSerif, 14F);
            Label_StationListSource.Format = eTextFormat.Normal;
            Label_StationListSource.Text = "Source: ";
            Label_StationListSource.Opacity = 127;
            panel.addControl(Label_StationListSource);

            // Presets list
            OMList lstMedia = new OMList("lstMedia", Shape_ChannelListBorder.Left + 5, Label_StationListHeader.Top + Label_StationListHeader.Height + 10, Shape_ChannelListBorder.Width - 20, Shape_ChannelListBorder.Height - 60);
            lstMedia.ListStyle = eListStyle.MultiList;
            lstMedia.Color = BuiltInComponents.SystemSettings.SkinTextColor;
            lstMedia.ScrollbarColor = Color.FromArgb(120, Color.White);
            lstMedia.SeparatorColor = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstMedia.ItemColor1 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 66);
            lstMedia.ItemColor2 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 10);
            lstMedia.SelectedItemColor1 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstMedia.SelectedItemColor2 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            //lstMedia.SelectedItemColor1 = Color.Red;
            lstMedia.HighlightColor = Color.White;
            lstMedia.Font = new Font(Font.GenericSansSerif, 26F);
            lstMedia.Scrollbars = true;
            lstMedia.TextAlignment = Alignment.TopLeft;
            lstMedia.ListItemOffset = 85;
            lstMedia.OnHoldClick += new userInteraction(lstMedia_OnHoldClick);
            lstMedia.OnClick += new userInteraction(lstMedia_OnClick);
            //lstMedia.DataSource = "Screen{:S:}.Zone.Playlist";
            panel.addControl(lstMedia);

            _MediaListSubItemFormat.color = Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinTextColor);
            _MediaListSubItemFormat.textAlignment = Alignment.BottomLeft;

            #endregion

            #region Access block

            OMLabel lblAccessBlock = new OMLabel("lblAccessBlock", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            lblAccessBlock.BackgroundColor = Color.FromArgb(230, Color.Black);
            lblAccessBlock.Text = "Radio skin can only be used for tuned content providers (Radio's).\r\nPlease select a provider before continuing";
            lblAccessBlock.TextAlignment = Alignment.CenterCenter;
            lblAccessBlock.Visible = false;
            panel.addControl(lblAccessBlock);

            #endregion

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(_MainPlugin.pluginName, panel.Name, "PopUpMenuStrip_Main");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Provider", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-view-as-list"), "Providers", true, mnuItem_Provider_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_MediaSources", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-view-as-list"), "MediaSources", true, mnuItem_MediaSources_OnClick));
            panel.PopUpMenu = PopUpMenuStrip;

            panel.Entering += new PanelEvent(panel_Entering);

            // Load the panel into the local manager for panels
            _MainPlugin.PanelManager.loadPanel(panel, true);

            // Subscribe to datasources
            OM.Host.ForEachScreen((screen)=>
                {
                    OM.Host.DataHandler.SubscribeToDataSource(screen, "Zone.Playlist", DataSourceChanged);
                });

            return panel;
        }

        void Button_Band_OnClick(OMControl sender, int screen)
        {
            SelectMediaSource(screen);
            AccessBlock_ShowHide(sender.Parent, screen);
        }

        void Button_SavePreset_OnHoldClick(OMControl sender, int screen)
        {
            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.Preset.Set");
            if (!HandleStatusMessage(sender, screen, "Unable to set preset", statusMessage))
            {   // Inform user that we added the channel to presets
                OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Current channel added to presets", 5));
            }
        }

        void Button_Scan_OnClick(OMControl sender, int screen)
        {
            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.SearchChannels");
            HandleStatusMessage(sender, screen, "Unable to search channels", statusMessage);
        }

        void Button_TuneTo_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.OSK osk = new OpenMobile.helperFunctions.OSK("", "tune parameters", "Enter tune parameters", OSKInputTypes.Keypad, false);
            string stationID = osk.ShowOSK(screen);

            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.DirectTune", stationID);
            HandleStatusMessage(sender, screen, "Unable to tune to channel", statusMessage);
        }

        void lstMedia_OnClick(OMControl sender, int screen)
        {
            OMList lst = sender as OMList;
            if (lst.SelectedItem == null || !(lst.SelectedItem.tag is mediaInfo))
                return;
            mediaInfo media = lst.SelectedItem.tag as mediaInfo;
            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.DirectTune", media);
            HandleStatusMessage(sender, screen, "Unable to tune to channel", statusMessage);
        }

        void lstMedia_OnHoldClick(OMControl sender, int screen)
        {
            OMList lst = sender as OMList;
            if (lst.SelectedItem == null || !(lst.SelectedItem.tag is mediaInfo))
                return;
            mediaInfo media = lst.SelectedItem.tag as mediaInfo;
            
            var listSource = OM.Host.DataHandler.GetDataSourceValue<int>(screen, "Zone.MediaSource.Data.ListSource");
            
            // If list source is presets show a menu, if it's live we add the channel to the presets
            if (listSource == 0)
            {   // Live channel list, add channel to presets
                var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.Preset.Set", media);
                if (!HandleStatusMessage(sender, screen, "Unable to set preset", statusMessage))
                {   // Inform user that we added the channel to presets
                    OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Channel added to presets", 5));
                }
            }

            else if (listSource == 1)
            {   // Presets list, show menu of what to do
                #region Menu popup

                // Popup menu
                MenuPopup PopupMenu = new MenuPopup("Presets menu", MenuPopup.ReturnTypes.Tag);

                // Popup menu items
                OMListItem ListItem1 = new OMListItem("Rename", "mnuItemRenamePreset" as object);
                ListItem1.image = OM.Host.getSkinImage("AIcons|5-content-edit").image;
                PopupMenu.AddMenuItem(ListItem1);

                OMListItem ListItem2 = new OMListItem("Set cover art", "mnuItemSetCoverArt" as object);
                ListItem2.image = OM.Host.getSkinImage("AIcons|5-content-picture").image;
                PopupMenu.AddMenuItem(ListItem2);

                OMListItem ListItem3 = new OMListItem("Delete", "mnuItemDeletePreset" as object);
                ListItem3.image = OM.Host.getSkinImage("AIcons|5-content-remove").image;
                PopupMenu.AddMenuItem(ListItem3);

                #endregion

                switch ((string)PopupMenu.ShowMenu(screen))
                {
                    case "mnuItemRenamePreset":
                        {
                            OpenMobile.helperFunctions.OSK osk = new OpenMobile.helperFunctions.OSK(media.Name, "Enter new preset name", "Enter preset name", OSKInputTypes.Keypad, false);
                            string name = osk.ShowOSK(screen);

                            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.Preset.Rename", media, name);
                            HandleStatusMessage(sender, screen, "Unable to rename preset", statusMessage);
                        }
                        break;
                    case "mnuItemSetCoverArt":
                        {
                            var fileDialog = new OpenMobile.helperFunctions.General.getFilePath();
                            var path = fileDialog.getFile(screen, this._MainPlugin.pluginName, PanelName); //, this._MainPlugin.GetPluginPath());
                            if (!String.IsNullOrWhiteSpace(path))
                            {
                                var image = OM.Host.getImageFromFile(path);
                                if (image.image != null)
                                {
                                    var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.Preset.SetCoverArt", media, image.image);
                                    HandleStatusMessage(sender, screen, "Unable to set cover art", statusMessage);
                                }
                            }
                        }
                        break;
                    case "mnuItemDeletePreset":
                        {
                            var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Command.Preset.Remove", media);
                            HandleStatusMessage(sender, screen, "Unable to remove preset", statusMessage);
                        }
                        break;
                }

            }
        }

        /// <summary>
        /// Handles any status/error messages returned by the command. If no data is present it indicates a sucessfull command and returns false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        /// <param name="message"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private bool HandleStatusMessage(OMControl sender, int screen, string message, object statusMessage)
        {
            // Handle error messages
            if (statusMessage is string && !String.IsNullOrWhiteSpace(statusMessage as string))
            {
                dialog dialog = new dialog(_MainPlugin.pluginName, sender.Parent.Name);
                dialog.Header = "Failed to execute command";
                dialog.Text = String.Format("{0}\r\n{1}", message, statusMessage as string);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
                return true;
            }
            return false;
        }

        private void DataSourceChanged(DataSource datasource)
        {
            switch (datasource.FullNameWithoutScreen)
            {
                case "Zone.Playlist":
                    {
                        // Get current list source
                        var listSource = OM.Host.DataHandler.GetDataSourceValue<int>(datasource.Screen, "Zone.MediaSource.Data.ListSource");

                        // Update list source
                        OMLabel lbl = _MainPlugin.PanelManager[datasource.Screen, PanelName][datasource.Screen, "Label_StationListSource"] as OMLabel;
                        if (lbl != null)
                        {
                            if (listSource == 0)
                                lbl.Text = String.Format("Source: Live");
                            else if (listSource == 1)
                                lbl.Text = String.Format("Source: Presets");
                            else
                                lbl.Text = String.Format("Source: Other");
                        }

                        // Bind playlist to list control
                        OMList list = _MainPlugin.PanelManager[datasource.Screen, PanelName][datasource.Screen, "lstMedia"] as OMList;

                        // Have list source changed?
                        if (list.Tag == null || (int)list.Tag != listSource)
                        {   // Yes, new list source. Clear list
                            list.Clear();
                        }

                        // Save list source so we can handle changes in list source later
                        list.Tag = listSource;

                        Playlist playlist = datasource.Value as Playlist;
                        if (list != null)
                        {
                            if (playlist != null)
                            {
                                foreach (var item in playlist.Items)
                                {
                                    // Check if item is already present in list, if so try to merge changes
                                    var listItem = list.Items.Where(x => ((mediaInfo)x.tag).Location == item.Location).FirstOrDefault();
                                    if (listItem != null)
                                    {   // Merge changes
                                        listItem.text = item.Name;
                                        listItem.subItem = item.Genre;
                                        listItem.image = item.coverArt;
                                        listItem.tag = item;
                                    }
                                    else
                                    {   // Insert new item
                                        list.Add(new OMListItem(item.Name, item.Genre, item.coverArt, _MediaListSubItemFormat, item));
                                    }
                                }

                                // Remove invalid items
                                list.Items.RemoveAll(x => !playlist.Items.Contains(x.tag as mediaInfo));
                            }
                        }

                        OMLabel lbl2 = _MainPlugin.PanelManager[datasource.Screen, PanelName][datasource.Screen, "Label_StationListHeader"] as OMLabel;
                        if (lbl2 != null)
                        {
                            if (playlist != null)
                            {
                                if (playlist.Count == 0)
                                    lbl2.Text = "Channel list:";
                                else if (playlist.Count == 1)
                                    lbl2.Text = "Channel list (1 item):";
                                if (playlist.Count > 1)
                                    lbl2.Text = String.Format("Channel list ({0} items):", playlist.Count);

                            }
                        }
                    }
                    break;

                default:
                    break;
            }


        }

        void panel_Entering(OMPanel sender, int screen)
        {
            OMLabel lbl = sender[screen, "lblInputBlockMessage"] as OMLabel;

            // Check if current media provider is a tuned content provider
            var currentProvider = OM.Host.MediaProviderHandler.GetMediaProviderForZoneOrScreen(screen);
            var zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            bool isTunedContentProvider = false;
            if (currentProvider != null)
                isTunedContentProvider = OM.Host.MediaProviderHandler.IsMediaSourceOfType(currentProvider.GetMediaSource(zone), MediaSourceTypes.TunedContent.ToString());
            if (currentProvider == null || !isTunedContentProvider)
            {   // Tuned content provider is not active, show dialog to activate provider
                SelectMediaProvider(sender, screen, "Select a radio source", MediaSourceTypes.TunedContent.ToString());
            }

            AccessBlock_ShowHide(sender, screen);
        }

        private void AccessBlock_ShowHide(OMPanel sender, int screen)
        {
            OMLabel lbl = _MainPlugin.PanelManager[screen, PanelName][screen, "lblAccessBlock"] as OMLabel;

            var zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            var isTunedContentProvider = false;
            var currentProvider = OM.Host.MediaProviderHandler.GetMediaProviderForZoneOrScreen(screen);
            if (currentProvider != null)
                isTunedContentProvider = OM.Host.MediaProviderHandler.IsMediaSourceOfType(currentProvider.GetMediaSource(zone), MediaSourceTypes.TunedContent.ToString());
            if (currentProvider == null || !isTunedContentProvider)
            {
                if (lbl != null)
                    lbl.Visible = true;
            }
            else
            {
                if (lbl != null)
                    lbl.Visible = false;
            }
        }

        private void SelectMediaSource(int screen)
        {
            // Get a list of media sources
            var currentProvider = OM.Host.MediaProviderHandler.GetMediaProviderForZoneOrScreen(screen);
            var mediaSources = OM.Host.MediaProviderHandler.GetMediaSourcesForProvider(currentProvider);

            // Popup menu
            PopupMenu = new MenuPopup("Select media source", MenuPopup.ReturnTypes.Tag);
            PopupMenu.Top = 75;
            PopupMenu.Height = 450;
            PopupMenu.Width = 700;
            PopupMenu.FontSize = 26;

            foreach (var source in mediaSources)
            {
                OMListItem listItem = new OMListItem(source.Name, source.Description, source.Name);
                listItem.image = source.Icon.image;
                PopupMenu.AddMenuItem(listItem);
            }

            // Show menu
            var selectedSource = PopupMenu.ShowMenu(screen);

            // Activate source
            if (selectedSource != null)
            {
                OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaSource.Activate", selectedSource as string);
            }
        }

        private void mnuItem_MediaSources_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            SelectMediaSource(screen);
        }

        private void mnuItem_Provider_OnClick(OMControl sender, int screen)
        {
            SelectMediaProvider(sender.Parent, screen, "Select a radio source", MediaSourceTypes.TunedContent.ToString());
            AccessBlock_ShowHide(sender.Parent, screen);
        }

        private bool SelectMediaProvider(OMPanel panel, int screen, string header, string mediaSourceType)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            // Get a list of providers
            var providerList = OM.Host.MediaProviderHandler.GetMediaProvidersByMediaSourceType(mediaSourceType);

            // Popup menu
            MenuPopup PopupMenu = new MenuPopup(header, MenuPopup.ReturnTypes.Tag);
            PopupMenu.Top = 75;
            PopupMenu.Height = 450;
            PopupMenu.Width = 700;
            PopupMenu.FontSize = 26;

            foreach (var provider in providerList)
            {
                IBasePlugin baseProvider = provider as IBasePlugin;
                OMListItem listItem = new OMListItem(baseProvider.pluginName, baseProvider.pluginDescription, baseProvider.pluginName);
                listItem.image = baseProvider.pluginIcon.image;
                PopupMenu.AddMenuItem(listItem);
            }

            // Show menu
            var selectedProvider = PopupMenu.ShowMenu(screen);

            // Activate provider
            if (selectedProvider != null)
            {
                var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaProvider.Activate", selectedProvider as string) as string;

                // Handle error messages
                if (statusMessage is string && !String.IsNullOrWhiteSpace(statusMessage as string))
                {
                    dialog dialog = new dialog(_MainPlugin.pluginName, panel.Name);
                    dialog.Header = "Failed to activate provider";
                    dialog.Text = String.Format("Unable to activate provider '{0}'\r\n{1}", selectedProvider as string, statusMessage as string);
                    dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                    dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                    dialog.ShowMsgBox(screen);
                    return false;
                }
            }
            else
                return false;

            return true;
        }
    }
}