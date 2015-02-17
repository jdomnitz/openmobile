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
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenMobile.Threading;

namespace OMMusicSkin
{
    internal class panelPlaylistView
    {
        private HighLevelCode _MainPlugin;
        private MenuPopup PopupMenu;
        public const string PanelName = "Playlist";

        public panelPlaylistView(HighLevelCode mainPlugin)
        {
            _MainPlugin = mainPlugin;
        }

        public OMPanel Initialize()
        {
            // Create a new panel
            OMPanel panel = new OMPanel(PanelName, "Music > Current playlist", _MainPlugin.pluginIcon);

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            // Cover flow
            OMMediaFlow lst_ViewPlaylist_ImgFlow = new OMMediaFlow("lst_ViewPlaylist_ImgFlow", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            lst_ViewPlaylist_ImgFlow.ClipRendering = false;
            lst_ViewPlaylist_ImgFlow.Animation_FadeOutDistance = 2f;
            lst_ViewPlaylist_ImgFlow.ReflectionsEnabled = true;
            lst_ViewPlaylist_ImgFlow.NoMediaImage = MediaLoader.MissingCoverImage;
            lst_ViewPlaylist_ImgFlow.MediaInfoFormatString = "{1} - {0}\n{6}";
            lst_ViewPlaylist_ImgFlow.OnClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnClick);
            lst_ViewPlaylist_ImgFlow.OnHoldClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnHoldClick);
            lst_ViewPlaylist_ImgFlow.DataSource = "Screen{:S:}.Zone.Playlist";
            panel.addControl(lst_ViewPlaylist_ImgFlow);

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(_MainPlugin.pluginName, panel.Name, "PopUpMenuStrip_Playlist");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Browser", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-view-as-list"), "Music browser", true, cmdOnClick: "Screen{:S:}.Panel.Goto.OMMusicSkin.MediaBrowser"));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_NowPlaying", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|6-social-person"), "Now playing", true, cmdOnClick: "Screen{:S:}.Panel.Goto.OMMusicSkin.NowPlaying"));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_OpenURL", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Stream"), "Open URL", true, mnuItem_OpenURL_OnClick, null, null));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShuffleToggle", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|9-av-shuffle"), "Toggle shuffle", false, mnuItem_ShuffleToggle_OnClick, null, null));
            PopUpMenuStrip.OnShowing += new ButtonStrip.MenuEventHandler(PopUpMenuStrip_OnShowing);
            panel.PopUpMenu = PopUpMenuStrip;

            // Connect panel events
            panel.Entering += new PanelEvent(panel_Entering);
            panel.Leaving += new PanelEvent(panel_Leaving);

            // Load the panel into the local manager for panels
            _MainPlugin.PanelManager.loadPanel(panel, true);

            #region Menu popup

            // Popup menu
            PopupMenu = new MenuPopup("Playlist Menu", MenuPopup.ReturnTypes.Tag);
            PopupMenu.Top = 75;
            PopupMenu.Height = 450;
            PopupMenu.Width = 600;

            // Popup menu items
            {
                OMListItem ListItem = new OMListItem("Play now", "mnuItemPlayNow" as object);
                ListItem.image = OM.Host.getSkinImage("AIcons|9-av-play").image;
                PopupMenu.AddMenuItem(ListItem);
            }
            {
                OMListItem ListItem = new OMListItem("Play next", "mnuItemPlayNext" as object);
                ListItem.image = OM.Host.getSkinImage("AIcons|5-content-new").image;
                PopupMenu.AddMenuItem(ListItem);
            }
            //{
            //    OMListItem ListItem = new OMListItem("Show artist", "mnuItemShowArtist" as object);
            //    ListItem.image = OM.Host.getSkinImage("AIcons|6-social-person").image;
            //    PopupMenu.AddMenuItem(ListItem);
            //}
            //{
            //    OMListItem ListItem = new OMListItem("Show album", "mnuItemShowAlbum" as object);
            //    ListItem.image = OM.Host.getSkinImage("AIcons|4-collections-view-as-list").image;
            //    PopupMenu.AddMenuItem(ListItem);
            //}
            {
                OMListItem ListItem = new OMListItem("Remove", "mnuItemRemove" as object);
                ListItem.image = OM.Host.getSkinImage("AIcons|5-content-remove").image;
                PopupMenu.AddMenuItem(ListItem);
            }
            {
                OMListItem ListItem = new OMListItem("Toggle shuffle", "mnuItemToggleShuffle" as object);
                ListItem.image = OM.Host.getSkinImage("AIcons|9-av-shuffle").image;
                PopupMenu.AddMenuItem(ListItem);
            }

            #endregion

            return panel;
        }

        void lst_ViewPlaylist_ImgFlow_OnClick(OMControl sender, int screen)
        {
            // Show NowPlaying panel
            _MainPlugin.GotoPanel(screen, "NowPlaying");
        }

        void lst_ViewPlaylist_ImgFlow_OnHoldClick(OMControl sender, int screen)
        {
            switch (PopupMenu.ShowMenu(screen) as string)
            {
                case "mnuItemPlayNow":
                    {
                        OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
                        string reply = OM.Host.CommandHandler.ExecuteCommand<string>("Zone.Play", lstImgFlow.SelectedIndex);
                        if (reply != String.Empty)
                        {
                            OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(_MainPlugin, "Playlist");
                            dialog.Header = "Playback error";
                            dialog.Text = String.Format("{0} is unable to play media\nreason: {2}", _MainPlugin.pluginName, reply);
                            dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                            dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                            dialog.ShowMsgBox(screen);
                        }
                        else
                        {
                            _MainPlugin.GotoPanel(screen, "NowPlaying");
                        }
                    }
                    break;

                case "mnuItemPlayNext":
                    {
                        OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
                        var playlist = lstImgFlow.ListSource as Playlist;
                        playlist.SetNextMedia(lstImgFlow.SelectedIndex);
                    }
                    break;

                case "mnuItemShowArtist":
                    {

                    }
                    break;

                case "mnuItemShowAlbum":
                    {

                    }
                    break;

                case "mnuItemRemove":
                    {
                        OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
                        var playlist = lstImgFlow.ListSource as Playlist;
                        playlist.RemoveAt(lstImgFlow.SelectedIndex);
                    }
                    break;

                case "mnuItemToggleShuffle":
                    {
                        OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Zone.Shuffle.Toggle", screen));
                    }
                    break;

                default:
                    break;
            }
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            OM.Host.UIHandler.ControlButtons_Show(screen, false);
            //OpenMobile.Media.Playlist playlist = OM.Host.DataHandler.GetDataSourceValue<OpenMobile.Media.Playlist>(screen, "Zone.Playlist");
            //if (playlist != null)
            //{
            //    OMMediaFlow lstImgFlow = sender[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
            //    lstImgFlow.ListSource = playlist;
            //}
        }

        void panel_Leaving(OMPanel sender, int screen)
        {
            //OM.Host.UIHandler.InfoBar_Hide(screen);
            OM.Host.UIHandler.ControlButtons_Hide(screen, false);
        }

        void mnuItem_ShuffleToggle_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Zone.Shuffle.Toggle", screen));
        }

        void mnuItem_OpenURL_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            string url = OSK.ShowDefaultOSK(screen, "", "Enter URL", "Enter URL to open", OSKInputTypes.Keypad, false);
            //string url = @"http://mms-live.online.no/p4_bandit_ogg_lq";

            if (!String.IsNullOrEmpty(url))
            {
                OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.PlayURL", url);
                _MainPlugin.GotoPanel(screen, "NowPlaying");
            }
        }

        void btnBrowser_OnClick(OMControl sender, int screen)
        {
            //OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.PlayURL", @"http://mms-live.online.no/p4_bandit_ogg_lq");
            ////OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.PlayURL", @"http://stream.sbsradio.no:8000/radiorock.mp3");
            //base.GotoPanel(screen, "NowPlaying");
            //base.GotoPanel(screen, "PlaylistEditor");
            //_MainPlugin.GotoPanel(screen, "MusicBrowser");


            //OM.Host.CommandHandler.ExecuteCommand("Screen0.Panel.Goto.OMMusicSkin.PlaylistEditor");
            //OM.Host.execute(eFunction.ShowPanel, screen, "OMMusicSkin;PlaylistEditor");
        }

        void PopUpMenuStrip_OnShowing(ButtonStrip sender, int screen, OMContainer menuContainer)
        {   // Configure items to show the current state of the list hold mode
            ButtonStrip popup = sender as ButtonStrip;

            Playlist playlist = OM.Host.DataHandler.GetDataSourceValue<Playlist>(screen, "Zone.Playlist");
            if (playlist == null)
                return;

            var menuItem = ((OMButton)menuContainer["mnuItem_ShuffleToggle"][0]);
            if (menuItem == null)
                return;

            menuItem.Checked = playlist.Shuffle;
        }

    }
}
