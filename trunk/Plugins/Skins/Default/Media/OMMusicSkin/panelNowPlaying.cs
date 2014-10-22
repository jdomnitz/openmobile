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
    internal class panelNowPlaying
    {
        private OMMusicSkin _MainPlugin;
        private bool dataSourcesSubscribed = false;

        public const string PanelName = "NowPlaying";

        public panelNowPlaying(OMMusicSkin mainPlugin)
        {
            _MainPlugin = mainPlugin;
        }

        public OMPanel Initialize()
        {
            // Create a new panel
            OMPanel panel = new OMPanel(PanelName, "Music > Now playing", _MainPlugin.pluginIcon);

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            // Cover art
            OMImage Image_CoverArt = new OMImage("Image_CoverArt", OM.Host.ClientArea_Init.Right - 355, OM.Host.ClientArea_Init.Top + 70, 250, 330);
            //Image_CoverArt.ReflectionData = new ReflectionsData(Color.FromArgb(255, 150, 150, 150), Color.Black);
            Image_CoverArt.Rotation = new OpenMobile.Math.Vector3(0, 45, 0);
            Image_CoverArt.ReflectionData = new ReflectionsData(Color.FromArgb(127, Color.White), Color.Transparent, 0.25f);
            Image_CoverArt.DataSource = "Screen{:S:}.Zone.MediaProvider.MediaInfo.CoverArt";
            Image_CoverArt.NullImage = new imageItem(MediaLoader.MissingCoverImage);
            Image_CoverArt.DataSourceControlsVisibility = false;
            panel.addControl(Image_CoverArt);

            // Media labels: Title
            OMAnimatedLabel2 lbl_Title = new OMAnimatedLabel2("lbl_Title", 70, 140, Image_CoverArt.Region.Left - 70 - 20, 40);
            //OMLabel lbl_Title = new OMLabel("lbl_Title", 15, 157, Image_CoverArt.Region.Left - 15 - 15, 40);
            lbl_Title.Text = "{Screen{:S:}.Zone.MediaProvider.MediaInfo.Name}";
            lbl_Title.FontSize = 30;
            lbl_Title.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Title.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Title.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Title.TextAlignment = Alignment.CenterRight;
            panel.addControl(lbl_Title);

            // Media labels: Artist
            OMAnimatedLabel2 lbl_Artist = new OMAnimatedLabel2("lbl_Artist", lbl_Title.Region.Left, lbl_Title.Region.Bottom + 5, lbl_Title.Region.Width, 30);
            lbl_Artist.Text = "{Screen{:S:}.Zone.MediaProvider.MediaInfo.Artist}";
            lbl_Artist.FontSize = 24;
            lbl_Artist.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Artist.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Artist.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Artist.TextAlignment = Alignment.CenterRight;
            lbl_Artist.Opacity = 127;
            panel.addControl(lbl_Artist);

            // Media labels: Album
            OMAnimatedLabel2 lbl_Album = new OMAnimatedLabel2("lbl_Album", lbl_Artist.Region.Left, lbl_Artist.Region.Bottom + 5, lbl_Artist.Region.Width, 30);
            lbl_Album.Text = "{Screen{:S:}.Zone.MediaProvider.MediaInfo.Album}";
            lbl_Album.FontSize = 24;
            lbl_Album.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Album.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Album.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Album.TextAlignment = Alignment.CenterRight;
            lbl_Album.Opacity = 127;
            panel.addControl(lbl_Album);

            // Media labels: Genre
            OMAnimatedLabel2 lbl_Genre = new OMAnimatedLabel2("lbl_Genre", lbl_Album.Region.Left, lbl_Album.Region.Bottom + 5, lbl_Album.Region.Width, 30);
            lbl_Genre.Text = "{Screen{:S:}.Zone.MediaProvider.MediaInfo.Genre}";
            lbl_Genre.FontSize = 24;
            lbl_Genre.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            lbl_Genre.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            lbl_Genre.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            lbl_Genre.TextAlignment = Alignment.CenterRight;
            lbl_Genre.Opacity = 127;
            panel.addControl(lbl_Genre);

            // Media labels: Genre
            OMLabel lbl_PlaybackPos = new OMLabel("lbl_PlaybackPos", lbl_Genre.Region.Left, lbl_Genre.Region.Bottom + 5, lbl_Genre.Region.Width, 30);
            lbl_PlaybackPos.Text = "{Screen{:S:}.Zone.MediaProvider.Playback.Pos.Text} / {Screen{:S:}.Zone.MediaProvider.MediaInfo.Length.Text}";
            lbl_PlaybackPos.FontSize = 24;
            lbl_PlaybackPos.TextAlignment = Alignment.CenterRight;
            lbl_PlaybackPos.Opacity = 127;
            panel.addControl(lbl_PlaybackPos);

            // Media labels: Rating
            OMImage img_Rating = new OMImage("img_Rating", lbl_PlaybackPos.Region.Right - 100, lbl_PlaybackPos.Region.Bottom + 5, 100, 24);
            img_Rating.Image = OM.Host.getSkinImage("Icons|Icon-Ratings-5");
            img_Rating.Opacity = 100;
            panel.addControl(img_Rating);

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
            btnGotoPlaylist.OnClick += (OMControl sender, int screen) => _MainPlugin.GotoPanel(screen, "Playlist");
            panel.addControl(btnGotoPlaylist);

            //OMButton btnBrowser = OMButton.PreConfigLayout_BasicStyle("btnBrowser", shapeBackground.Region.Left + 10, shapeBackground.Region.Bottom - 75, 140, 70, OpenMobile.Graphics.GraphicCorners.Left, "", "Browser", 22);
            //btnBrowser.Opacity = 150;
            //btnBrowser.OnClick += (OMControl sender, int screen) => base.GotoPanel(screen, "PlaylistEditor");
            //panel.addControl(btnBrowser);

            //OMButton btnPlaylist = OMButton.PreConfigLayout_BasicStyle("btnPlaylist", 0, 0, 140, 70, OpenMobile.Graphics.GraphicCorners.Right, "", "Playlist", 22);
            //btnPlaylist.Opacity = 150;
            //btnPlaylist.OnClick += (OMControl sender, int screen) => base.GotoPanel(screen, "Playlist");
            //panel.addControl(btnPlaylist, ControlDirections.Right);

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(_MainPlugin.pluginName, panel.Name, "PopUpMenuStrip_MusicSkin");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Browser", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-view-as-list"), "Music browser", true, cmdOnClick: "Screen{:S:}.Panel.Goto.OMMusicSkin.MediaBrowser"));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Playlist", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-view-as-grid"), "Current playlist", true, cmdOnClick: "Screen{:S:}.Panel.Goto.OMMusicSkin.Playlist"));
            //PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_OpenURL", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Stream"), "Open URL", true, mnuItem_OpenURL_OnClick, null, null));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShuffleToggle", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|9-av-shuffle"), "Toggle shuffle", false, mnuItem_ShuffleToggle_OnClick, null, null));
            PopUpMenuStrip.OnShowing += new ButtonStrip.MenuEventHandler(PopUpMenuStrip_OnShowing);
            panel.PopUpMenu = PopUpMenuStrip;

            panel.Entering += delegate(OMPanel sender, int screen)
            {
                OM.Host.UIHandler.ControlButtons_Show(screen, false);

                // Subscribe to changes to rating datasource
                if (!dataSourcesSubscribed)
                {
                    dataSourcesSubscribed = true;
                    OM.Host.DataHandler.SubscribeToDataSource("Zone.MediaProvider.MediaInfo.Rating", (x) =>
                    {
                        OMPanel localPanel = _MainPlugin.PanelManager[x.Screen, panel.Name];
                        if (localPanel != null)
                        {
                            OMImage imgRating = localPanel["img_Rating"] as OMImage;
                            if (imgRating != null && (int)x.Value >= 0)
                                imgRating.Image = OM.Host.getSkinImage(String.Format("Icons|Icon-Ratings-{0}", (int)x.Value));
                            else
                                imgRating.Image = imageItem.NONE;
                        }
                    });
                }
            };
            panel.Leaving += delegate(OMPanel sender, int screen)
            {
                OM.Host.UIHandler.ControlButtons_Hide(screen, false);
            };
            // Load the panel into the local manager for panels
            //PanelManager.loadPanel(panel);

            return panel;
        }

        void mnuItem_ShuffleToggle_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            OM.Host.CommandHandler.ExecuteCommand(String.Format("Screen{0}.Zone.MediaProvider.Shuffle.Toggle", screen));
        }

        void PopUpMenuStrip_OnShowing(ButtonStrip sender, int screen, OMContainer menuContainer)
        {   // Configure items to show the current state of the list hold mode
            ButtonStrip popup = sender as ButtonStrip;

            Playlist playlist = OM.Host.DataHandler.GetDataSourceValue<Playlist>(screen, "Zone.MediaProvider.Playlist");
            if (playlist == null)
                return;

            var menuItem = ((OMButton)menuContainer["mnuItem_ShuffleToggle"][0]);
            if (menuItem == null)
                return;

            menuItem.Checked = playlist.Shuffle;
        }

    }
}
