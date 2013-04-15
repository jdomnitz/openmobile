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

namespace OMMusicSkin
{
    public sealed class OMMusicSkin : HighLevelCode
    {
        public OMMusicSkin()
            : base("OMMusicSkin", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "Music player", "Music player", "OM Dev team/Borte", "")
        {
        }

        string[] _MediaDBName;

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Create a new panel
            OMPanel panel = new OMPanel("Panel");

            OMButton btnSource = OMButton.PreConfigLayout_BasicStyle("btnSource", 0, 100, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Source");
            btnSource.OnClick += new userInteraction(btnSource_OnClick);
            panel.addControl(btnSource);

            OMButton btnPlay = OMButton.PreConfigLayout_BasicStyle("btnPlay", 0, 200, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Play");
            btnPlay.OnClick += new userInteraction(btnPlay_OnClick);
            panel.addControl(btnPlay);

            OMButton btnStop = OMButton.PreConfigLayout_BasicStyle("btnStop", 0, 270, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Stop");
            btnStop.OnClick += new userInteraction(btnStop_OnClick);
            panel.addControl(btnStop);

            OMButton btnPause = OMButton.PreConfigLayout_BasicStyle("btnPause", 0, 340, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Pause");
            btnPause.OnClick += new userInteraction(btnPause_OnClick);
            panel.addControl(btnPause);

            OMButton btnFwd = OMButton.PreConfigLayout_BasicStyle("btnFwd", 100, 480, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Fwd");
            btnFwd.OnClick += new userInteraction(btnFwd_OnClick);
            btnFwd.OnHoldClick += new userInteraction(btnFwd_OnHoldClick);
            btnFwd.OnLongClick += new userInteraction(btnFwd_OnLongClick);
            panel.addControl(btnFwd);

            OMButton btnBwd = OMButton.PreConfigLayout_BasicStyle("btnBwd", 0, 480, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Bwd");
            btnBwd.OnClick += new userInteraction(btnBwd_OnClick);
            btnBwd.OnHoldClick += new userInteraction(btnBwd_OnHoldClick);
            btnBwd.OnLongClick += new userInteraction(btnBwd_OnLongClick);
            panel.addControl(btnBwd);

            // Create a target window
            OMTargetWindow VideoWindow = new OMTargetWindow("VideoWindow", 150, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Right - 180, OM.Host.ClientArea[0].Height - 20);
            VideoWindow.OnWindowCreated += new OMTargetWindow.WindowArgs(VideoWindow_OnWindowCreated);
            VideoWindow.Visible = false;
            panel.addControl(VideoWindow);

            // Cowerflow
            //OMObjectList lstCoverFlow = OMObjectList.PreConfigLayout_ImageFlow("lstCoverFlow", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Top+30, OM.Host.ClientArea[0].Width, 150,
            //    new Size(125, 125), 50);
            //panel.addControl(lstCoverFlow);

            OMImageFlow lstImgFlow = new OMImageFlow("lstImgFlow", OM.Host.ClientArea[0].Left+100, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Width-100, OM.Host.ClientArea[0].Height - 70);
            lstImgFlow.overlay = OM.Host.getSkinImage("Images|Overlay-BlackBottom").image;
            lstImgFlow.overlay.RenderText(0, lstImgFlow.overlay.Height - 110, lstImgFlow.overlay.Width, 100, "OpenMobile Coverflow sample", new Font(Font.Arial, 25), eTextFormat.Normal, Alignment.CenterCenter, Color.White, Color.White, FitModes.Fit);
            lstImgFlow.overlay.AddImageOverlay(lstImgFlow.overlay.Width - 64, lstImgFlow.overlay.Height - 64, OM.Host.getSkinImage("AIcons|9-av-play-over-video").image);
            
            //lstImgFlow.Control_PlacementOffsetY = -50;
            //lstImgFlow.ImageSize = new Size(350, 350);
            panel.addControl(lstImgFlow);

            // Connect panel events
            panel.Entering += new PanelEvent(panel_Entering);

            // Load the panel into the local manager for panels
            PanelManager.loadPanel(panel, true);

            _MediaDBName = new string[host.ScreenCount];
            for (int i = 0; i < _MediaDBName.Length; i++)
                _MediaDBName[i] = BuiltInComponents.SystemSettings.DefaultDB_Music;

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        void btnBwd_OnLongClick(OMControl sender, int screen)
        {
            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            //lstImgFlow.CleanupAnimation();
        }

        void btnFwd_OnLongClick(OMControl sender, int screen)
        {
            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            //lstImgFlow.CleanupAnimation();
        }

        void btnBwd_OnHoldClick(OMControl sender, int screen)
        {
            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            lstImgFlow.JumpTo(0);
        }

        void btnFwd_OnHoldClick(OMControl sender, int screen)
        {
            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            lstImgFlow.JumpTo(1000);
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Set default music indexer as MediaDb source
            IMediaDatabase db = OM.Host.getData(eGetData.GetMediaDatabase, _MediaDBName[screen]) as IMediaDatabase;
            if (db == null) return;

            // Initialize list with items
            db.beginGetAlbums(true);
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

            //OMObjectList lstCoverFlow = sender[screen, "lstCoverFlow"] as OMObjectList;
            OMImageFlow lstImgFlow = sender[screen, "lstImgFlow"] as OMImageFlow;
            for (int i = 0; i < 50; i++)
            {
                foreach (mediaInfo m in mediaList)
                {
                    //lstImgFlow.Add(m.coverArt);
                    lstImgFlow.Insert("cover", m.coverArt);
                    //lstCoverFlow.AddItemFromItemBase(ControlDirections.Right, m.coverArt);
                }
            }

        }

        void btnBwd_OnClick(OMControl sender, int screen)
        {
            //Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            ////zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.SubTitle_Cycle));
            //zone.MediaHandler.Previous();

            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            lstImgFlow.MoveLeft();
            //lstImgFlow.CleanupAnimation();
        }

        void btnFwd_OnClick(OMControl sender, int screen)
        {
            //Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            //zone.MediaHandler.Next();
            ////zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepForward));

            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            lstImgFlow.MoveRight();
            //lstImgFlow.CleanupAnimation();
        }

        void VideoWindow_OnWindowCreated(OMTargetWindow sender, int screen, GameWindow window, IntPtr handle)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.SetVideoTarget, sender, screen));
        }

        void btnPause_OnClick(OMControl sender, int screen)
        {
            //Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            ////zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Pause));
            //zone.MediaHandler.Play(3);

            OMImageFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMImageFlow;
            lstImgFlow.MoveRight();
        }

        void btnSource_OnClick(OMControl sender, int screen)
        {
            //_MediaDBName[screen] = BuiltInComponents.SystemSettings.DefaultDB_Music;
            _MediaDBName[screen] = BuiltInComponents.SystemSettings.DefaultDB_CD;
        }

        void btnStop_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Stop));
        }

        void btnPlay_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);

            // Set default music indexer as MediaDb source
            IMediaDatabase db = OM.Host.getData(eGetData.GetMediaDatabase, _MediaDBName[screen]) as IMediaDatabase;
            if (db == null) return;

            db.beginGetSongs(false, eMediaField.Artist);
            List<mediaInfo> media = new List<mediaInfo>();
            while (true)
            {
                mediaInfo info = db.getNextMedia();
                if (info != null)
                    media.Add(info);
                else
                    break;
            }
            db.endSearch();
            db.Dispose();

            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(MediaProvider_Commands.Play, zone, new mediaInfo(eMediaType.InternetRadio, @"http://mms-live.online.no/p4_bandit_ogg_lq"));


            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, media[7]));
            //zone.MediaHandler.Playlist.AddDistinct(media[1]);
            //zone.MediaHandler.Playlist.AddDistinct(media[2]);
            //zone.MediaHandler.Playlist.AddDistinct(media[3]);
            //zone.MediaHandler.Playlist.AddDistinct(media[4]);
            //zone.MediaHandler.Playlist.AddDistinct(media[5]);
            //zone.MediaHandler.Playlist.AddDistinct(media[6]);
            //zone.MediaHandler.Playlist.AddDistinct(media[7]);
            zone.MediaHandler.Play();

            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, new mediaInfo(@"D:\Video\Sample.mkv")));

            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, new mediaInfo(eMediaType.DVD, @"G:")));


            //object o;
            //theHost.getData(eGetData.GetMediaDatabase, dbname[screen], out  o);
            //if (o == null)
            //    return;
            //lock (manager[screen][12])
            //{
            //    Artists[screen].Clear();
            //    using (IMediaDatabase db = (IMediaDatabase)o)
            //    {
            //        db.beginGetArtists(false);
            //        mediaInfo info = db.getNextMedia();
            //        while (info != null)
            //        {
            //            Artists[screen].Add(info.Artist);
            //            info = db.getNextMedia();
            //        }
            //        db.endSearch();
            //    }
            //}

            

            //zone.MediaProvider.ExecuteCommand(MediaProvider_Commands.Play,
        }
    }
}
