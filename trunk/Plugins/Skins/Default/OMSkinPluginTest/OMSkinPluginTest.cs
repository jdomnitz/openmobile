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
using OpenMobile.Data;
using OpenMobile.Controls;
using System.Collections.Generic;

namespace OMSkinPluginTest
{
    public sealed class OMSkinPluginTest : HighLevelCode
    {
        public OMSkinPluginTest()
            : base("OMSkinPluginTest", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "A test plugin", "OMSkinPluginTest", "OM Dev team/Borte", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Create a new panel
            OMPanel panel = new OMPanel("Panel");

            // Create a new label
            OMLabel lblHelloWorld = new OMLabel("lblHelloWorld", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height, "Hello World!");
            lblHelloWorld.DataSource = "TestGroup.TestSource.Source1";
            
            // Add the new label to the panel
            panel.addControl(lblHelloWorld);

            OMButton btnPlay1 = OMButton.PreConfigLayout_BasicStyle("btnPlay1", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Play1");
            btnPlay1.OnClick += new userInteraction(btnPlay1_OnClick);
            panel.addControl(btnPlay1);

            OMButton btnPlay2 = OMButton.PreConfigLayout_BasicStyle("btnPlay2", 0, 0, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Play2");
            btnPlay2.OnClick += new userInteraction(btnPlay2_OnClick);
            panel.addControl(btnPlay2, ControlDirections.Down);

            panel.Entering += new PanelEvent(panel_Entering);

            // Load the panel into the local manager for panels
            base.PanelManager.loadPanel(panel, true);


            // Use this to goto your local panels:
            // base.GotoPanel(screen, panelName);

            // Use this to show additional local panels:
            // base.ShowPanel(screen, panelName);

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            object playlistObject;
            if (OM.Host.DataHandler.GetDataSourceValue(screen, "Zone.MediaProvider.Playlist", null, out playlistObject))
            {
                OpenMobile.Media.Playlist playlist = playlistObject as OpenMobile.Media.Playlist;
                if (playlist != null)
                {
                    if (!playlist.HasItems)
                    {
                        IMediaDatabase db = OM.Host.getData(eGetData.GetMediaDatabase, BuiltInComponents.SystemSettings.DefaultDB_Music) as IMediaDatabase;

                        if (db != null)
                        {
                            // Initialize list with items
                            db.beginGetSongs("", "", "", "", "", -1, true, eMediaField.Album);
                            //db.beginGetArtists(true);
                            //db.beginGetAlbums(true);
                            List<mediaInfo> mediaList = new List<mediaInfo>();
                            while (true)
                            {
                                mediaInfo info = db.getNextMedia();
                                if (info != null)
                                    mediaList.Add(info);
                                else
                                    break;
                            }
                            db.endSearch();
                            db.Dispose();

                            // Add items to current playlist
                            foreach (mediaInfo m in mediaList)
                                playlist.Add(m);

                            //foreach (mediaInfo m in mediaList)
                            //    lstImgFlow.Insert(m.coverArt);
                        }
                    }
                }
            }
        }

        void btnPlay1_OnClick(OMControl sender, int screen)
        {
            mediaInfo media;
            media = new mediaInfo(@"D:\Music2\W\W.A.S.P\Dominator\W.A.S.P-Dominator-03-Take Me Up.mp3");
            //media = new mediaInfo(eMediaType.URLStream, @"http://mms-live.online.no/p4_bandit_ogg_lq");
            string reply = OM.Host.CommandHandler.ExecuteCommand<string>("Zone.MediaProvider.Play", media);
            if (reply != String.Empty)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, "Panel");
                dialog.Header = "Playback error";
                dialog.Text = String.Format("{0} is unable to play media: '{1}'\nreason: {2}", this.pluginName, media.Location, reply);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
        }

        void btnPlay2_OnClick(OMControl sender, int screen)
        {
            mediaInfo media;
            //media = new mediaInfo(@"D:\Music2\A\ACDC\Who Made Who\AC-DC - DC - Who Made Who - Who Made Who.mp3");
            media = new mediaInfo(eMediaType.URLStream, @"http://mms-live.online.no/p4_bandit_ogg_lq");
            string reply = OM.Host.CommandHandler.ExecuteCommand<string>("OMWinPlayer2.Zone1.Play", media);
            if (reply != String.Empty)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, "Panel");
                dialog.Header = "Playback error";
                dialog.Text = String.Format("{0} is unable to play media: '{1}'\nreason: {2}", this.pluginName, media.Location, reply);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
        }

    }
}
