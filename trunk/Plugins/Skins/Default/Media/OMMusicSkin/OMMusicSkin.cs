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
using System.Drawing.Drawing2D;


namespace OMMusicSkin
{
    public sealed class OMMusicSkin : HighLevelCode
    {
        public OMMusicSkin()
            : base("OMMusicSkin", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "Music player", "Music player", "OM Dev team/Borte", "")
        {
        }

        string[] _MediaDBName;
        ButtonStrip PopUpMenuStrip = null;

        public override eLoadStatus initialize(IPluginHost host)
        {
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
            // Create a new panel
            OMPanel panel = new OMPanel("Playlist", "Music > Current playlist");

            //OMButton btnSource = OMButton.PreConfigLayout_BasicStyle("btnSource", 0, 100, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Source");
            //btnSource.OnClick += new userInteraction(btnSource_OnClick);
            //btnSource.Visible = false;
            //panel.addControl(btnSource);

            //OMButton btnPlay = OMButton.PreConfigLayout_BasicStyle("btnPlay", 0, 200, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Play");
            //btnPlay.OnClick += new userInteraction(btnPlay_OnClick);
            //btnPlay.Visible = false;
            //panel.addControl(btnPlay);

            //OMButton btnStop = OMButton.PreConfigLayout_BasicStyle("btnStop", 0, 270, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Stop");
            //btnStop.OnClick += new userInteraction(btnStop_OnClick);
            //btnStop.Visible = false;
            //panel.addControl(btnStop);

            //OMButton btnPause = OMButton.PreConfigLayout_BasicStyle("btnPause", 0, 340, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Pause");
            //btnPause.OnClick += new userInteraction(btnPause_OnClick);
            //btnPause.Visible = false;
            //panel.addControl(btnPause);

            //OMButton btnFwd = OMButton.PreConfigLayout_BasicStyle("btnFwd", 0, 410, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Fwd");
            //btnFwd.OnClick += new userInteraction(btnFwd_OnClick);
            //btnFwd.OnHoldClick += new userInteraction(btnFwd_OnHoldClick);
            //btnFwd.Visible = false;
            //panel.addControl(btnFwd);

            //OMButton btnBwd = OMButton.PreConfigLayout_BasicStyle("btnBwd", 0, 480, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "Bwd");
            //btnBwd.OnClick += new userInteraction(btnBwd_OnClick);
            //btnBwd.OnHoldClick += new userInteraction(btnBwd_OnHoldClick);
            //btnBwd.Visible = false;
            //panel.addControl(btnBwd);

            // Create a target window
            //OMTargetWindow VideoWindow = new OMTargetWindow("VideoWindow", 150, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Right - 180, OM.Host.ClientArea_Init.Height - 20);
            //VideoWindow.OnWindowCreated += new OMTargetWindow.WindowArgs(VideoWindow_OnWindowCreated);
            //VideoWindow.Visible = false;
            //panel.addControl(VideoWindow);


            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Bottom - 140, 700, 85,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            //OMImage imgBackground = new OMImage("imgBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 100, OM.Host.getSkinImage("Objects|CoverflowBackground"));
            //imgBackground.Left = OM.Host.ClientArea_Init.Center.X - imgBackground.Region.Center.X;
            //imgBackground.Opacity = 128;
            //panel.addControl(imgBackground);

            // Cowerflow
            OMMediaFlow lst_ViewPlaylist_ImgFlow = new OMMediaFlow("lst_ViewPlaylist_ImgFlow", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height - 70);
            lst_ViewPlaylist_ImgFlow.ClipRendering = false;
            lst_ViewPlaylist_ImgFlow.Animation_FadeOutDistance = 2f;
            lst_ViewPlaylist_ImgFlow.ReflectionsEnabled = false;
            //lstImgFlow.overlay = OM.Host.getSkinImage("Images|Overlay-BlackBottom").image;
            //lstImgFlow.overlay.RenderText(0, lstImgFlow.overlay.Height - 110, lstImgFlow.overlay.Width, 100, "OpenMobile Coverflow sample", new Font(Font.Arial, 25), eTextFormat.Normal, Alignment.CenterCenter, Color.White, Color.White, FitModes.Fit);
            //lst_ViewPlaylist_ImgFlow.overlay.AddImageOverlay(lst_ViewPlaylist_ImgFlow.overlay.Width - 64, lst_ViewPlaylist_ImgFlow.overlay.Height - 64, OM.Host.getSkinImage("AIcons|9-av-play-over-video").image);
            lst_ViewPlaylist_ImgFlow.NoMediaImage = MediaLoader.MissingCoverImage;
            lst_ViewPlaylist_ImgFlow.MediaInfoFormatString = "{1} - {0}\n{6}";
            lst_ViewPlaylist_ImgFlow.OnClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnClick);
            lst_ViewPlaylist_ImgFlow.OnHoldClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnHoldClick);
            panel.addControl(lst_ViewPlaylist_ImgFlow);

            OMButton btnTest = OMButton.PreConfigLayout_BasicStyle("btnTest", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Bottom-100, 100, 70, OpenMobile.Graphics.GraphicCorners.All, "", "URL");
            btnTest.OnClick += new userInteraction(btnTest_OnClick);
            panel.addControl(btnTest);

            // Connect panel events
            panel.Entering += new PanelEvent(panel_Entering);
            panel.Leaving += new PanelEvent(panel_Leaving);

            // Load the panel into the local manager for panels
            PanelManager.loadPanel(panel, true);

            _MediaDBName = new string[host.ScreenCount];
            for (int i = 0; i < _MediaDBName.Length; i++)
                _MediaDBName[i] = BuiltInComponents.SystemSettings.DefaultDB_Music;

            // Create the buttonstrip popup
            PopUpMenuStrip = new ButtonStrip(pluginName, panel.Name, "PopUpMenuStrip_MusicSkin");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_OpenURL", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Stream"), "Open URL", true, mnuItem_OpenURL_OnClick, null, null));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShuffleToggle", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|9-av-shuffle"), "Toggle shuffle", false, mnuItem_ShuffleToggle_OnClick, null, null));

            // Now Playing panel
            //OpenMobile.Threading.SafeThread.Asynchronous(() =>
            //{
            //    PanelManager.loadPanel(CreateAndLoadPanel_NowPlaying());
            //});

            // Queue panels
            PanelManager.QueuePanel("NowPlaying", CreateAndLoadPanel_NowPlaying);
            PanelManager.QueuePanel("PlaylistEditor", InitializePanel_PlaylistEditor);                

            //System.Threading.Thread t = new System.Threading.Thread(CreateAndLoadPanel_NowPlaying);
           
            //CreateAndLoadPanel_NowPlaying();

            });

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        void btnTest_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.MediaProvider.PlayURL", @"http://mms-live.online.no/p4_bandit_ogg_lq");
            base.GotoPanel(screen, "NowPlaying");
        }

        #region Playlist editor panels

        private OMPanel InitializePanel_PlaylistEditor()
        {
            // Create a new panel
            OMPanel panel = new OMPanel("PlaylistEditor", "Music > Playlist editor");


            return panel;
        }


        #endregion

        private OMPanel CreateAndLoadPanel_NowPlaying()
        {
            // Create a new panel
            OMPanel panel = new OMPanel("NowPlaying", "Music > Now playing");

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 30, 900, 350,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            //OMImage Image_CoverArt_Glow = new OMImage("Image_CoverArt_Glow", OM.Host.ClientArea_Init.Right - 450, OM.Host.ClientArea_Init.Top, 450, 420);
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Image_CoverArt_Glow.Region.Width, Image_CoverArt_Glow.Region.Height);
            //using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            //{
            //    GraphicsPath gp = new GraphicsPath();
            //    gp = OpenMobile.Graphics.GDI.GraphicsPathHelpers.Path_CreateRoundedRectangle(new Rectangle(75, 50, 300, 300), 20);
            //    g.FillPath(new System.Drawing.SolidBrush(BuiltInComponents.SystemSettings.SkinFocusColor.ToSystemColor()), gp);
            //    gp.Dispose();
            //}
            //Image_CoverArt_Glow.Image = new imageItem(new OImage(bmp));
            //Image_CoverArt_Glow.Image.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor, 127);
            //Image_CoverArt_Glow.Opacity = 50;
            //panel.addControl(Image_CoverArt_Glow);

            //OMBasicShape Shape_CoverArt_Border = new OMBasicShape("Shape_CoverArt_Border", OM.Host.ClientArea_Init.Right - 377, OM.Host.ClientArea_Init.Top + 48, 304, 330,
            //    new ShapeData(shapes.Rectangle, Color.Black));
            //panel.addControl(Shape_CoverArt_Border);

            //OMBasicShape Shape_Floor = new OMBasicShape("Shape_Floor", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 347, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Bottom - (OM.Host.ClientArea_Init.Top + 380),
            //    new ShapeData(shapes.Rectangle, Color.Black));
            //panel.addControl(Shape_Floor);

            // Cover art
            OMImage Image_CoverArt = new OMImage("Image_CoverArt", OM.Host.ClientArea_Init.Right - 375, OM.Host.ClientArea_Init.Top + 50, 300, 300);
            //Image_CoverArt.ReflectionData = new ReflectionsData(Color.FromArgb(255, 150, 150, 150), Color.Black);
            Image_CoverArt.DataSource = "Screen{:S:}.Zone.MediaInfo.CoverArt";
            Image_CoverArt.NullImage = new imageItem(MediaLoader.MissingCoverImage);
            Image_CoverArt.DataSourceControlsVisibility = false;
            panel.addControl(Image_CoverArt);

            // Media labels: Title
            OMAnimatedLabel2 lbl_Title = new OMAnimatedLabel2("lbl_Title", 70, 140, Image_CoverArt.Region.Left - 70 - 20, 40);
            //OMLabel lbl_Title = new OMLabel("lbl_Title", 15, 157, Image_CoverArt.Region.Left - 15 - 15, 40);
            lbl_Title.Text = "{Screen{:S:}.Zone.MediaInfo.Name}";
            lbl_Title.FontSize = 30;
            lbl_Title.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Title.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Title.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Title.TextAlignment = Alignment.CenterRight;
            panel.addControl(lbl_Title);

            // Media labels: Artist
            OMAnimatedLabel2 lbl_Artist = new OMAnimatedLabel2("lbl_Artist", lbl_Title.Region.Left, lbl_Title.Region.Bottom + 5, lbl_Title.Region.Width, 30);
            lbl_Artist.Text = "{Screen{:S:}.Zone.MediaInfo.Artist}";
            lbl_Artist.FontSize = 24;
            lbl_Artist.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Artist.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Artist.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Artist.TextAlignment = Alignment.CenterRight;
            lbl_Artist.Opacity = 127;
            panel.addControl(lbl_Artist);

            // Media labels: Album
            OMAnimatedLabel2 lbl_Album = new OMAnimatedLabel2("lbl_Album", lbl_Artist.Region.Left, lbl_Artist.Region.Bottom + 5, lbl_Artist.Region.Width, 30);
            lbl_Album.Text = "{Screen{:S:}.Zone.MediaInfo.Album}";
            lbl_Album.FontSize = 24;
            lbl_Album.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Album.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Album.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Album.TextAlignment = Alignment.CenterRight;
            lbl_Album.Opacity = 127;
            panel.addControl(lbl_Album);

            /*
            #region Progress bar

            float colorOpacity = 0.4f;
            Color progressBackColor = Color.FromArgb(BuiltInComponents.SystemSettings.SkinFocusColor.A,
                (int)(BuiltInComponents.SystemSettings.SkinFocusColor.R * colorOpacity),
                (int)(BuiltInComponents.SystemSettings.SkinFocusColor.G * colorOpacity),
                (int)(BuiltInComponents.SystemSettings.SkinFocusColor.B * colorOpacity));

            OMBasicShape shape_Progress_Background = new OMBasicShape("shape_Progress_Background", 300, OM.Host.ClientArea_Init.Bottom - 50 , 400, 5,
                new ShapeData(shapes.Rectangle)
                {
                    GradientData = GradientData.CreateHorizontalGradient(
                        new GradientData.ColorPoint(0.0, 0, Color.Black),
                        new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, progressBackColor)),
                        new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, progressBackColor)),
                        new GradientData.ColorPoint(0.5, 0, Color.Black))
                });
            //shape_Progress_Background.Transparency = 75;
            panel.addControl(shape_Progress_Background);
            OMBasicShape shape_Progress_Bar = new OMBasicShape("shape_Progress_Bar", 300, OM.Host.ClientArea_Init.Bottom - 50, 200, 5,
                new ShapeData(shapes.Rectangle)
                {
                    GradientData = GradientData.CreateHorizontalGradient(
                        new GradientData.ColorPoint(0.0, 0, Color.Black),
                        new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                        new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                        new GradientData.ColorPoint(0.5, 0, Color.Black))
                });
            panel.addControl(shape_Progress_Bar);
            int handleSize = 10;
            OMBasicShape shape_Progress_Handle = new OMBasicShape("shape_Progress_Handle", shape_Progress_Bar.Region.Right - handleSize, shape_Progress_Bar.Region.Center.Y - (handleSize / 2), handleSize, handleSize,
                new ShapeData(shapes.Rectangle)
                {
                    GradientData = GradientData.CreateColorBorder(0.5f, 0.5f,
                    Color.Black,
                    Color.Black,
                    Color.Black,
                    Color.Black,
                    BuiltInComponents.SystemSettings.SkinFocusColor)
                });
            panel.addControl(shape_Progress_Handle);

            // Manually connect to datasources
            OM.Host.ForEachScreen(delegate(int Screen)
            {
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("Screen{0}.Zone.MediaInfo.Playback.PosPercent", Screen),
                    new OpenMobile.Data.DataSourceChangedDelegate(ProgressBar_DataSourceChanged));
            });

            OMLabel label_MediaInfo_CurrentPos = new OMLabel("label_MediaInfo_CurrentPos", shape_Progress_Background.Region.Left - 100, shape_Progress_Background.Region.Top - 6, 100, 15,
                "{Screen{:S:}.Zone.MediaInfo.Playback.Pos}");
            label_MediaInfo_CurrentPos.TextAlignment = Alignment.CenterRight;
            label_MediaInfo_CurrentPos.FontSize = 12;
            panel.addControl(label_MediaInfo_CurrentPos);
            OMLabel label_MediaInfo_Length = new OMLabel("label_MediaInfo_CurrentPos", shape_Progress_Background.Region.Right, shape_Progress_Background.Region.Top - 6, 100, 15,
                "{Screen{:S:}.Zone.MediaInfo.Playback.Length}");
            label_MediaInfo_Length.TextAlignment = Alignment.CenterLeft;
            label_MediaInfo_Length.FontSize = 12;
            panel.addControl(label_MediaInfo_Length);

            #endregion
            */

            OMButton btnGotoPlaylist = new OMButton("btnGotoPlaylist", shapeBackground.Region.Left, shapeBackground.Region.Top, shapeBackground.Region.Width, shapeBackground.Region.Height);
            btnGotoPlaylist.OnClick += delegate(OMControl sender, int screen)
            {
                base.GotoPanel(screen, "Playlist");
            };
            panel.addControl(btnGotoPlaylist);

            panel.Entering += delegate(OMPanel sender, int screen)
                {
                    OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip);
                    //OM.Host.UIHandler.ControlButtons_Show(screen, false);
                };
            panel.Leaving += delegate(OMPanel sender, int screen)
            {
                //OM.Host.UIHandler.ControlButtons_Hide(screen, false);
            };
            // Load the panel into the local manager for panels
            //PanelManager.loadPanel(panel);

            return panel;
        }

        private void ProgressBar_DataSourceChanged(DataSource datasource)
        {
            // Extract screen number from datasource name
            int screen = Params.GetScreenFromString(datasource.NameLevel1);

            if (!base.PanelManager.IsPanelLoaded(screen, "NowPlaying"))
                return;

            // Get the panel for the correct screen
            OMPanel panel = base.PanelManager[screen, "NowPlaying"];

            if (panel == null)
                return;

            // Get the correct controls
            OMBasicShape shape_Progress_Bar = panel["shape_Progress_Bar"] as OMBasicShape;
            OMBasicShape shape_Progress_Handle = panel["shape_Progress_Handle"] as OMBasicShape;
            OMBasicShape shape_Progress_Background = panel["shape_Progress_Background"] as OMBasicShape;
            
            if (shape_Progress_Bar != null && shape_Progress_Handle != null && shape_Progress_Background != null)
            {
                shape_Progress_Bar.Width = (int)(shape_Progress_Background.Width * ((int)datasource.Value) / 100.0f);
                shape_Progress_Handle.Left = shape_Progress_Bar.Region.Right - shape_Progress_Handle.Region.Width;
            }
        }

        void mnuItem_ShuffleToggle_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Zone.MediaProvider.Shuffle.Toggle", screen));
        }

        void mnuItem_OpenURL_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            string url = OSK.ShowDefaultOSK(screen, "", "Enter URL", "Enter URL to open", OSKInputTypes.Keypad, false);

            if (!String.IsNullOrEmpty(url))
            {
                OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.MediaProvider.PlayURL", url);
                base.GotoPanel(screen, "NowPlaying");
            }
        }

        void lst_ViewPlaylist_ImgFlow_OnHoldClick(OMControl sender, int screen)
        {
            OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.Play(lstImgFlow.SelectedIndex);
        }

        void lst_ViewPlaylist_ImgFlow_OnClick(OMControl sender, int screen)
        {
            //ToggleView(sender, screen);

            // Show NowPlaying panel
            base.GotoPanel(screen, "NowPlaying");
            //OM.Host.execute(eFunction.GotoPanel, screen, String.Format("{0};NowPlaying", this.pluginName));
            //OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Panel.Goto.OMMusicSkin.NowPlaying", screen));
        }

        private void ToggleView(OMControl sender, int screen)
        {
            ControlLayout viewPlaylist = new ControlLayout(sender.Parent, "_ViewPlaylist_");
            ControlLayout viewNowPlaying = new ControlLayout(sender.Parent, "_ViewNowPlaying_");
            if (viewPlaylist.Visible)
            {   // Playlist is visible, change to now playing

                // Hide playlist view
                viewPlaylist.Visible = true;
                SmoothAnimator.PresetAnimation_FadeOut(viewPlaylist, screen, 2f);

                // Show now playing view
                SmoothAnimator.PresetAnimation_FadeIn(viewNowPlaying, screen, 2f);
                viewNowPlaying.Visible = true;
            }
            else
            {   // Now playing is visible, change to playlist view

                // Hide now playing view
                viewNowPlaying.Visible = true;
                SmoothAnimator.PresetAnimation_FadeOut(viewNowPlaying, screen, 2f);

                // Show playlist view
                SmoothAnimator.PresetAnimation_FadeIn(viewPlaylist, screen, 2f);
                viewPlaylist.Visible = true;

            }
        }

        void btnBwd_OnHoldClick(OMControl sender, int screen)
        {
            OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
            lstImgFlow.JumpTo(0);
        }

        void btnFwd_OnHoldClick(OMControl sender, int screen)
        {
            OMMediaFlow lstImgFlow = sender.Parent[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
            lstImgFlow.JumpTo(1000);
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            OM.Host.execute(eFunction.unloadTunedContent, zone);
            //OM.Host.UIHandler.InfoBar_Show(screen, new InfoBar("Current PlayList"));

            //OM.Host.UIHandler.MediaBanner_Disable(screen);
            OM.Host.UIHandler.ControlButtons_Show(screen, false);

            // Load the buttonstrip
            OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip);


            // Set default music indexer as MediaDb source
            IMediaDatabase db = OM.Host.getData(eGetData.GetMediaDatabase, _MediaDBName[screen]) as IMediaDatabase;
            if (db == null) return;

            // Initialize list with items
            db.beginGetSongs(true, eMediaField.Album);
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

            if (!zone.MediaHandler.Playlist.HasItems)
            {
                zone.MediaHandler.Playlist.Load();

                if (!zone.MediaHandler.Playlist.HasItems)
                {
                    zone.MediaHandler.Playlist.Clear();

                    //zone.MediaHandler.Playlist.Random = true;

                    // Add items to current playlist
                    foreach (mediaInfo m in mediaList)
                        zone.MediaHandler.Playlist.Add(m);

                    //foreach (mediaInfo m in mediaList)
                    //    lstImgFlow.Insert(m.coverArt);

                    //zone.MediaHandler.Playlist.Save();
                }
            }

            OMMediaFlow lstImgFlow = sender[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
            lstImgFlow.PlayListSource = zone.MediaHandler.Playlist;
        }

        void panel_Leaving(OMPanel sender, int screen)
        {
            //OM.Host.UIHandler.InfoBar_Hide(screen);
            OM.Host.UIHandler.ControlButtons_Hide(screen, false);
        }

        void btnBwd_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.SubTitle_Cycle));

            //OMMediaFlow lstImgFlow = sender.Parent[screen, "lstImgFlow"] as OMMediaFlow;
            ////lstImgFlow.RemoveAt(3);
            //lstImgFlow.Insert(OM.Host.getSkinImage("questionMark").image, 3);
        }

        void btnFwd_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepForward));
        }

        void VideoWindow_OnWindowCreated(OMTargetWindow sender, int screen, GameWindow window, IntPtr handle)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            // zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.SetVideoTarget, sender, screen));
        }

        void btnPause_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            //zone.MediaHandler.ActiveMediaProvider.ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Pause));
            //zone.MediaHandler.Play(3);
            //zone.MediaHandler.Playlist.Random = !zone.MediaHandler.Playlist.Random;
            OM.Host.CommandHandler.ExecuteCommand("Screen0.Zone.MediaProvider.Shuffle.Toggle");
            

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
