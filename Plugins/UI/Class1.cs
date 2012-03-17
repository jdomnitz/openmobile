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
using OpenMobile.Graphics;
using System.Threading;
using System.Timers;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.Plugins;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions;
using System.Collections;
using System.Collections.Generic;

namespace OpenMobile
{
    [PluginLevel(PluginLevels.UI | PluginLevels.System)]
    public sealed class UI : IHighLevel
    {
        private IPluginHost theHost;
        private System.Timers.Timer tick = new System.Timers.Timer();
        private System.Timers.Timer statusReset = new System.Timers.Timer(2100);

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            get { return "UI"; }
        }

        public float pluginVersion
        {
            get { return 0.6F; }
        }

        public string pluginDescription
        {
            get { return "The User Interface"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return true;
        }
        public bool incomingMessage<T>(string message,string source, ref T data)
        {
            if (message == "AddIcon")
            {
                IconManager.UIIcon icon = data as IconManager.UIIcon;
                icons.AddIcon(icon);
                return true;
            }
            if (message == "RemoveIcon")
            {
                IconManager.UIIcon icon = data as IconManager.UIIcon;
                icons.RemoveIcon(icon);
                return true;
            }
            return false;
        }
        public string displayName
        {
            get { return "User Interface"; }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            theHost.OnMediaEvent -= theHost_OnMediaEvent;
            theHost.OnSystemEvent -= theHost_OnSystemEvent;
            tick.Dispose();
            manager.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IHighLevel Members

        public OMPanel loadPanel(string name,int screen)
        {
            if (String.IsNullOrEmpty(name))
                return manager[screen, "UI"];

            return manager[screen,name];
        }

        public Settings loadSettings()
        {
            return null;
        }

        #endregion

        #region IBasePlugin Members
        IconManager icons = new IconManager();
        ScreenManager manager;
        OMPanel UIPanel = null;
        public eLoadStatus initialize(IPluginHost host)
        {
            UIPanel = new OMPanel("UI");
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);

            tick.BeginInit();
            tick.EndInit();
            tick.Elapsed += new ElapsedEventHandler(tick_Elapsed);
            tick.Interval = 500;
            tick.Enabled = true;

            statusReset.BeginInit();
            statusReset.EndInit();
            statusReset.Elapsed += new ElapsedEventHandler(statusReset_Elapsed);

            MediaBarVisible = new bool[theHost.ScreenCount];
            VolumeBarVisible = new bool[theHost.ScreenCount];
            VolumeBarTimer = new Timer[theHost.ScreenCount];

            OMBasicShape Shape_VideoBackground = new OMBasicShape("Shape_VideoBackground", 0, 0, 1000, 600);
            Shape_VideoBackground.Shape = shapes.Rectangle;
            Shape_VideoBackground.FillColor = Color.Black;
            Shape_VideoBackground.Visible = false;
            UIPanel.addControl(Shape_VideoBackground);

            #region BottomBar (Media)

            //OMButton Button_UIBottomBar_Back = DefaultControls.GetHorisontalEdgeButton("Button_UIBottomBar_Back", 831, 533, 160, 70, "", "Back");
            OMButton Button_UIBottomBar_Back = new OMButton("Button_UIBottomBar_Back", 831, 533, 160, 70);
            Button_UIBottomBar_Back.Image = theHost.getSkinImage("BackButton", true);
            Button_UIBottomBar_Back.FocusImage = theHost.getSkinImage("BackButtonFocus", true);
            Button_UIBottomBar_Back.OnClick += new userInteraction(Back_OnClick);
            Button_UIBottomBar_Back.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIBottomBar_Back);

            OMImage Image_UIMediaBar_Background = new OMImage("Image_UIMediaBar_Background", 0, 604, 1000, 140);
            Image_UIMediaBar_Background.Image = theHost.getSkinImage("mediaBar", true);
            Image_UIMediaBar_Background.Transparency = 80;
            UIPanel.addControl(Image_UIMediaBar_Background);

            OMButton Button_UIMediaBar_Media = new OMButton("Button_UIMediaBar_Media", 9, 534, 160, 70);
            Button_UIMediaBar_Media.Image = theHost.getSkinImage("MediaButton", true);
            Button_UIMediaBar_Media.Transition = eButtonTransition.None;
            Button_UIMediaBar_Media.FocusImage = theHost.getSkinImage("MediaButtonFocus", true);
            Button_UIMediaBar_Media.OnClick += new userInteraction(mediaButton_OnClick);
            UIPanel.addControl(Button_UIMediaBar_Media);

            OMSlider Slider_UIMediaBar_Slider = new OMSlider("Slider_UIMediaBar_Slider", 20, 615, 820, 25, 12, 40);
            Slider_UIMediaBar_Slider.Slider = theHost.getSkinImage("Slider");
            Slider_UIMediaBar_Slider.SliderBar = theHost.getSkinImage("Slider.Bar");
            Slider_UIMediaBar_Slider.Maximum = 0;
            Slider_UIMediaBar_Slider.OnSliderMoved += new OMSlider.slidermoved(slider_OnSliderMoved);
            UIPanel.addControl(Slider_UIMediaBar_Slider);

            OMButton Button_UIMediaBar_Play = new OMButton("Button_UIMediaBar_Play", 287, 633, 135, 100);
            Button_UIMediaBar_Play.Image = theHost.getSkinImage("Play");
            Button_UIMediaBar_Play.DownImage = theHost.getSkinImage("Play.Highlighted");
            Button_UIMediaBar_Play.OnClick += new userInteraction(playButton_OnClick);
            Button_UIMediaBar_Play.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_Play);

            OMButton Button_UIMediaBar_Stop = new OMButton("Button_UIMediaBar_Stop", 425, 633, 135, 100);
            Button_UIMediaBar_Stop.Image = theHost.getSkinImage("Stop", true);
            Button_UIMediaBar_Stop.DownImage = theHost.getSkinImage("Stop.Highlighted", true);
            Button_UIMediaBar_Stop.OnClick += new userInteraction(stopButton_OnClick);
            Button_UIMediaBar_Stop.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_Stop);

            OMButton Button_UIMediaBar_Rewind = new OMButton("Button_UIMediaBar_Rewind", 149, 633, 135, 100);
            Button_UIMediaBar_Rewind.Image = theHost.getSkinImage("Rewind");
            Button_UIMediaBar_Rewind.DownImage = theHost.getSkinImage("Rewind.Highlighted");
            Button_UIMediaBar_Rewind.OnClick += new userInteraction(rewindButton_OnClick);
            Button_UIMediaBar_Rewind.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_Rewind);

            OMButton Button_UIMediaBar_FastForward = new OMButton("Button_UIMediaBar_FastForward", 564, 633, 135, 100);
            Button_UIMediaBar_FastForward.OnClick += new userInteraction(fastForwardButton_OnClick);
            Button_UIMediaBar_FastForward.Image = theHost.getSkinImage("fastForward");
            Button_UIMediaBar_FastForward.DownImage = theHost.getSkinImage("fastForward.Highlighted");
            Button_UIMediaBar_FastForward.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_FastForward);

            OMButton Button_UIMediaBar_SkipForward = new OMButton("Button_UIMediaBar_SkipForward", 703, 633, 135, 100);
            Button_UIMediaBar_SkipForward.Image = theHost.getSkinImage("SkipForward", true);
            Button_UIMediaBar_SkipForward.DownImage = theHost.getSkinImage("SkipForward.Highlighted", true);
            Button_UIMediaBar_SkipForward.OnClick += new userInteraction(skipForwardButton_OnClick);
            Button_UIMediaBar_SkipForward.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_SkipForward);

            OMButton Button_UIMediaBar_SkipBackward = new OMButton("Button_UIMediaBar_SkipBackward", 13, 633, 135, 100);
            Button_UIMediaBar_SkipBackward.Image = theHost.getSkinImage("SkipBackward", true);
            Button_UIMediaBar_SkipBackward.DownImage = theHost.getSkinImage("SkipBackward.Highlighted", true);
            Button_UIMediaBar_SkipBackward.OnClick += new userInteraction(skipBackwardButton_OnClick);
            Button_UIMediaBar_SkipBackward.Transition = eButtonTransition.None;
            UIPanel.addControl(Button_UIMediaBar_SkipBackward);

            OMLabel Label_UIMediaBar_Elapsed = new OMLabel("Label_UIMediaBar_Elapsed", 840, 615, 160, 22);
            Label_UIMediaBar_Elapsed.OutlineColor = Color.Blue;
            Label_UIMediaBar_Elapsed.Font = new Font(Font.GenericSansSerif, 16F);
            Label_UIMediaBar_Elapsed.Format = eTextFormat.OutlineNarrow;
            UIPanel.addControl(Label_UIMediaBar_Elapsed);

            OMButton Button_UIMediaBar_Random = new OMButton("Button_UIMediaBar_Random", 845, 635, 55, 40);
            Button_UIMediaBar_Random.Image = theHost.getSkinImage("random");
            Button_UIMediaBar_Random.DownImage = theHost.getSkinImage("random.Highlighted");
            Button_UIMediaBar_Random.OnClick += new userInteraction(random_OnClick);
            UIPanel.addControl(Button_UIMediaBar_Random);

            OMButton Button_UIMediaBar_Zone = DefaultControls.GetButton("Button_UIMediaBar_Zone", 845, 680, 140, 50, "", "Zone");
            Button_UIMediaBar_Zone.OnClick += new userInteraction(Button_UIMediaBar_Zone_OnClick);
            UIPanel.addControl(Button_UIMediaBar_Zone);

            #endregion

            #region Top bar

            OMImage Image_UITopBar_Background = new OMImage("Image_UITopBar_Background", 0, 0, 1000, 99);
            Image_UITopBar_Background.Image = theHost.getSkinImage("topBar");
            UIPanel.addControl(Image_UITopBar_Background);

            OMAnimatedLabel Label_UITopBar_TrackTitle = new OMAnimatedLabel("Label_UITopBar_TrackTitle", 240, 3, 490, 28);
            Label_UITopBar_TrackTitle.TextAlignment = Alignment.CenterLeft;
            Label_UITopBar_TrackTitle.Format = eTextFormat.BoldShadow;
            Label_UITopBar_TrackTitle.ContiuousAnimation = eAnimation.Scroll;
            UIPanel.addControl(Label_UITopBar_TrackTitle);

            OMAnimatedLabel Label_UITopBar_TrackAlbum = new OMAnimatedLabel("Label_UITopBar_TrackAlbum", 240, 34, 490, 28);
            Label_UITopBar_TrackAlbum.TextAlignment = Alignment.CenterLeft;
            Label_UITopBar_TrackAlbum.Format = eTextFormat.BoldShadow;
            Label_UITopBar_TrackAlbum.ContiuousAnimation = eAnimation.Scroll;
            UIPanel.addControl(Label_UITopBar_TrackAlbum);

            OMAnimatedLabel Label_UITopBar_TrackArtist = new OMAnimatedLabel("Label_UITopBar_TrackArtist", 240, 64, 490, 28);
            Label_UITopBar_TrackArtist.TextAlignment = Alignment.CenterLeftEllipsis;
            Label_UITopBar_TrackArtist.Format = eTextFormat.DropShadow;
            Label_UITopBar_TrackArtist.ContiuousAnimation = eAnimation.Scroll;
            UIPanel.addControl(Label_UITopBar_TrackArtist);

            OMImage Image_UITopBar_Cover = new OMImage("Image_UITopBar_Cover", 143, 3, 90, 90);
            Image_UITopBar_Cover.Image = imageItem.NONE;
            UIPanel.addControl(Image_UITopBar_Cover);

            OMButton Button_UITopBar_Home = new OMButton("Button_UITopBar_Home", 863, 0, 130, 90);
            Button_UITopBar_Home.Image = theHost.getSkinImage("HomeButton", true);
            Button_UITopBar_Home.FocusImage = theHost.getSkinImage("HomeButtonFocus", true);
            Button_UITopBar_Home.OnClick += new userInteraction(HomeButton_OnClick);
            UIPanel.addControl(Button_UITopBar_Home);

            #region Volumecontrol

            OMButton Button_UITopBar_VolumeBar_Volume = new OMButton("Button_UITopBar_VolumeBar_Volume", 6, 0, 130, 90);
            Button_UITopBar_VolumeBar_Volume.Image = theHost.getSkinImage("VolumeButton");
            Button_UITopBar_VolumeBar_Volume.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
            Button_UITopBar_VolumeBar_Volume.Mode = eModeType.Resizing;
            Button_UITopBar_VolumeBar_Volume.Transition = eButtonTransition.None;
            Button_UITopBar_VolumeBar_Volume.OnClick += new userInteraction(vol_OnClick);
            Button_UITopBar_VolumeBar_Volume.OnLongClick += new userInteraction(vol_OnLongClick);
            UIPanel.addControl(Button_UITopBar_VolumeBar_Volume);

            VolumeBar VolumeBar_UITopBar_VolumeBar_Volume = new VolumeBar("VolumeBar_UITopBar_VolumeBar_Volume", 6, -420, 130, 330);
            VolumeBar_UITopBar_VolumeBar_Volume.Visible = false;
            VolumeBar_UITopBar_VolumeBar_Volume.OnSliderMoved += new userInteraction(volumeChange);
            UIPanel.addControl(VolumeBar_UITopBar_VolumeBar_Volume);

            OMButton Button_UITopBar_VolumeBar_VolumeDown = new OMButton("Button_UITopBar_VolumeBar_VolumeDown", 6, -90, 130, 90);
            Button_UITopBar_VolumeBar_VolumeDown.FillColor = Color.FromArgb(180, Color.White);
            Button_UITopBar_VolumeBar_VolumeDown.OverlayImage = new imageItem(OImage.FromWebdingsFont(Button_UITopBar_VolumeBar_VolumeDown.Width, Button_UITopBar_VolumeBar_VolumeDown.Height, "6", 150, eTextFormat.Outline, Alignment.CenterCenter, StoredData.SystemSettings.SkinFocusColor, Color.White));
            Button_UITopBar_VolumeBar_VolumeDown.Transition = eButtonTransition.None;
            Button_UITopBar_VolumeBar_VolumeDown.Tag = -2;
            Button_UITopBar_VolumeBar_VolumeDown.OnClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnClick);
            Button_UITopBar_VolumeBar_VolumeDown.OnLongClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnLongClick);
            UIPanel.addControl(Button_UITopBar_VolumeBar_VolumeDown);

            OMButton Button_UITopBar_VolumeBar_VolumeUp = new OMButton("Button_UITopBar_VolumeBar_VolumeUp", 6, -510, 130, 90);
            Button_UITopBar_VolumeBar_VolumeUp.FillColor = Color.FromArgb(180, Color.White);
            Button_UITopBar_VolumeBar_VolumeUp.OverlayImage = new imageItem(OImage.FromWebdingsFont(Button_UITopBar_VolumeBar_VolumeDown.Width, Button_UITopBar_VolumeBar_VolumeDown.Height, "5", 150, eTextFormat.Outline, Alignment.CenterCenter, StoredData.SystemSettings.SkinFocusColor, Color.White));
            Button_UITopBar_VolumeBar_VolumeUp.Transition = eButtonTransition.None;
            Button_UITopBar_VolumeBar_VolumeUp.Tag = 2;
            Button_UITopBar_VolumeBar_VolumeUp.OnClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnClick);
            Button_UITopBar_VolumeBar_VolumeUp.OnLongClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnLongClick);
            UIPanel.addControl(Button_UITopBar_VolumeBar_VolumeUp);

            #endregion

            #region Icons

            OMButton Button_UITopBar_Icon1 = new OMButton("Button_UITopBar_Icon1", 778, 4, 80, 85);
            Button_UITopBar_Icon1.OnClick += new userInteraction(icon_OnClick);
            UIPanel.addControl(Button_UITopBar_Icon1);

            OMButton Button_UITopBar_Icon2 = new OMButton("Button_UITopBar_Icon2", 727, 1, 50, 90);
            Button_UITopBar_Icon2.OnClick += new userInteraction(icon_OnClick);
            UIPanel.addControl(Button_UITopBar_Icon2);

            OMButton Button_UITopBar_Icon3 = new OMButton("Button_UITopBar_Icon3", 676, 1, 50, 90);
            Button_UITopBar_Icon3.OnClick += new userInteraction(icon_OnClick);
            UIPanel.addControl(Button_UITopBar_Icon3);

            OMButton Button_UITopBar_Icon4 = new OMButton("Button_UITopBar_Icon4", 625, 1, 50, 90);
            Button_UITopBar_Icon4.OnClick += new userInteraction(icon_OnClick);
            UIPanel.addControl(Button_UITopBar_Icon4);

            icons.OnIconsChanged += new IconManager.IconsChanged(icons_OnIconsChanged);

            #endregion


            #endregion

            #region Speech

            OMButton Button_Speech_Speech = new OMButton("Button_Speech_Speech", 670, 533, 160, 70);
            Button_Speech_Speech.Image = theHost.getSkinImage("Speak", true);
            Button_Speech_Speech.FocusImage = theHost.getSkinImage("SpeakFocus", true);
            Button_Speech_Speech.Visible = false;
            Button_Speech_Speech.OnClick += new userInteraction(speech_OnClick);
            UIPanel.addControl(Button_Speech_Speech);

            OMImage Image_Speech_Speech = new OMImage("Image_Speech_Speech", 350, 200, 300, 300);
            Image_Speech_Speech.Visible = false;
            UIPanel.addControl(Image_Speech_Speech);

            OMLabel Label_Speech_Caption = new OMLabel("Label_Speech_Caption", 300, 150, 400, 60);
            Label_Speech_Caption.Font = new Font(Font.GenericSerif, 48F);
            Label_Speech_Caption.Format = eTextFormat.BoldShadow;
            Label_Speech_Caption.Visible = false;
            UIPanel.addControl(Label_Speech_Caption);

            OMBasicShape Shape_Speech_BackgroundBlock = new OMBasicShape("Shape_Speech_BackgroundBlock", 0, 0, 1000, 600);
            Shape_Speech_BackgroundBlock.Shape = shapes.Rectangle;
            Shape_Speech_BackgroundBlock.FillColor = Color.FromArgb(130, Color.Black);
            Shape_Speech_BackgroundBlock.Visible = false;
            UIPanel.addControl(Shape_Speech_BackgroundBlock);

            #endregion

            #region Infobanner

            // A general information label in the center of the screen
            OMBasicShape Shape_Info_Background = new OMBasicShape("Shape_Info_Background", -10, 250, 1020, 100);
            Shape_Info_Background.Shape = shapes.Rectangle;
            Shape_Info_Background.BorderColor = Color.White;
            Shape_Info_Background.BorderSize = 1;
            Shape_Info_Background.FillColor = Color.FromArgb(210, Color.Black);
            Shape_Info_Background.Visible = false;
            Shape_Info_Background.NoUserInteraction = true;
            UIPanel.addControl(Shape_Info_Background);

            OMLabel Label_Info_Background = new OMLabel("Label_Info_Background", -500, 250, 2000, 100);
            Label_Info_Background.NoUserInteraction = true;
            Label_Info_Background.Font = new Font(Font.Arial, 45);
            Label_Info_Background.Visible = false;
            UIPanel.addControl(Label_Info_Background);

            OMLabel Label_Info = new OMLabel("Label_Info", 0, 250, 1000, 100);
            Label_Info.NoUserInteraction = true;
            Label_Info.Font = new Font(Font.Arial, 45);
            Label_Info.Visible = false;
            UIPanel.addControl(Label_Info);

            #endregion

            // TODO : Move clock and date to main menu skin
            //OMLabel clocktime = new OMLabel(350, 522, 0, 0);
            //OMLabel clockdate = new OMLabel(350, 572, 0, 0);

            /*
            OMLabel clocktime = new OMLabel(350, 522, 300, 60);
            clocktime.TextAlignment = Alignment.CenterCenter;
            clocktime.Font = new Font(Font.GenericSansSerif, 32F);
            clocktime.Format = eTextFormat.BoldShadow;
            clocktime.Name = "UI.clocktime";
            clocktime.sensorName = "SystemSensors.Time";
            clocktime.Visible = false;
            OMLabel clockdate = new OMLabel(350, 572, 300, 30);
            clockdate.TextAlignment = Alignment.CenterCenter;
            clockdate.Font = new Font(Font.GenericSansSerif, 20F);
            clockdate.Format = eTextFormat.BoldShadow;
            clockdate.Name = "UI.clockdate";
            clockdate.Visible = false;
            clockdate.sensorName = "SystemSensors.LongDate";
            */
            
            UIPanel.Priority = ePriority.High;
            UIPanel.UIPanel = true;

            UIPanel.Entering += new PanelEvent(UIPanel_Entering);

            manager.loadPanel(UIPanel);

            #region OM Background panel

            OMPanel background = new OMPanel("background");
            background.BackgroundType = backgroundStyle.SolidColor;
            background.BackgroundColor1 = Color.Black;
            background.Priority = ePriority.Low;
            background.UIPanel = true;
            background.Forgotten = true;
            manager.loadPanel(background);

            #endregion

            theHost.OnMediaEvent += theHost_OnMediaEvent;
            theHost.OnSystemEvent += theHost_OnSystemEvent;
            theHost.OnGesture += new GestureEvent(theHost_OnGesture);

            return eLoadStatus.LoadSuccessful;
        }

        void UIPanel_Entering(OMPanel sender, int screen)
        {   // Update initial data

            // Update active zone
            Zone zone = theHost.ZoneHandler.GetActiveZone(screen);
            OMButton b = (OMButton)UIPanel[screen, "Button_UIMediaBar_Zone"];
            b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));

            // Update current volume
            object o = theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            if (o != null)
            {
                int VolValue = (int)o;
                VolumeBar vol = (VolumeBar)UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];
                vol.Value = VolValue;                
            }
        }

        void Button_UIMediaBar_Zone_OnClick(OMControl sender, int screen)
        {
            // Popup menu - Zones. Returns the tag in the selected item which holds the zone object.
            MenuPopup Zones = new MenuPopup("Select a zone", MenuPopup.ReturnTypes.Tag);

            // Add zones to the list (Excluding if already present in the sender textbox)
            foreach (Zone zone in theHost.ZoneHandler.GetAvailableZonesForScreen(screen))
            {
                // Add zone to list
                OMListItem ZoneItem = new OMListItem(zone.Name, zone as object);
                ZoneItem.image = OImage.FromWebdingsFont(50, 50, "²", Color.Gray);
                Zones.AddMenuItem(ZoneItem);
            }

            // Show menu and get selected item
            Zone SelectedZone = Zones.ShowMenu(screen) as Zone;
            if (SelectedZone == null)
            {   // No change
                return;
            }
            else
            {   // Change to new zone
                theHost.ZoneHandler.SetActiveZone(screen, SelectedZone);
            }
        }

        void HomeButton_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.TransitionFromAny, screen.ToString()))
            {
                theHost.execute(eFunction.hideVideoWindow, screen.ToString());
                theHost.execute(eFunction.clearHistory, screen.ToString());
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }
        }

        void Back_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        #region Mediabar up/down control

        private List<OMControl> GetMediaControls(int screen)
        {
            return UIPanel.getPanelAtScreen(screen).Controls.FindAll(x => x.Name.Contains("UIMediaBar"));
        }
        private bool[] MediaBarVisible = null;
        private void AnimateMediaBar(bool up, int screen)
        {
            // Get media controls
            List<OMControl> MediaControls = GetMediaControls(screen);

            // Get media button control
            OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Media"];

            // Calculate relative placements of media controls
            int[] RelativePlacements = new int[MediaControls.Count];
            for (int i = 0; i < RelativePlacements.Length; i++)
                RelativePlacements[i] = MediaControls[i].Top - btn.Top;

            if (up)
            {   // Move media bar up                
                int EndPos = 390;
                int Top = btn.Top;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep)
                {
                    Top -= AnimationStep;
                    if (Top <= EndPos)
                    {   // Animation has completed
                        btn.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = btn.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        btn.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = btn.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = true;
            }
            else
            {   // Move media bar down
                int EndPos = 534;
                int Top = btn.Top;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep)
                {
                    Top += AnimationStep;
                    if (Top >= EndPos)
                    {   // Animation has completed
                        btn.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = btn.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        btn.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = btn.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = false;
            }
        }
        void mediaButton_OnClick(OMControl sender, int screen)
        {
            // Initialize variable 
            if (MediaBarVisible == null)
                MediaBarVisible = new bool[theHost.ScreenCount];

            if (!MediaBarVisible[screen])
            {   // Show media bar
                AnimateMediaBar(true, screen);
            }
            else 
            {   // Hide media bar
                AnimateMediaBar(false, screen);
            }
        }

        #endregion

        #region Volumecontrol

        private bool[] VolumeBarVisible = null;
        private Timer[] VolumeBarTimer = null;
        void vol_OnClick(OMControl sender, int screen)
        {
            lock (sender)
            {
                AnimateVolumeBar(!VolumeBarVisible[screen], screen);
            }
        }

        private void AnimateVolumeBar(bool show, int screen)
        {
            // Initialize visibility variable 
            if (VolumeBarVisible == null)
                VolumeBarVisible = new bool[theHost.ScreenCount];

            // Initialize timer variable 
            if (VolumeBarTimer == null)
                VolumeBarTimer = new Timer[theHost.ScreenCount];
            if (VolumeBarTimer[screen] == null)
            {
                // Activate timeout timer
                VolumeBarTimer[screen] = new Timer(2500);
                VolumeBarTimer[screen].AutoReset = false;
                VolumeBarTimer[screen].Elapsed += new ElapsedEventHandler(volTmr_Elapsed);
                VolumeBarTimer[screen].Screen = screen;
                VolumeBarTimer[screen].Tag = UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];
            }


            OMButton btn = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_Volume"];
            OMButton btnDown = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_VolumeDown"];
            OMButton btnUp = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_VolumeUp"];
            VolumeBar vol = (VolumeBar)UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];

            // Errorcheck
            if (btn == null || vol == null)
                return;

            SmoothAnimator Animation = new SmoothAnimator(2.0f);

            if (show)
            {
                // Set visibility state
                VolumeBarVisible[screen] = true;

                // Move video window so that we can draw the volumecontrol
                theHost.SetVideoPosition(screen, new Rectangle(136, 100, 864, 368));

                // Show volumebar control
                vol.Visible = true;

                // Animate control  
                int EndPos = 510;
                int Top = btn.Top;

                Animation.Animate(delegate(int AnimationStep)
                    {
                        Top += AnimationStep;
                        if (Top >= EndPos)
                        {   // Animation has completed
                            btn.Top = EndPos;
                            btnDown.Top = btn.Top - btnDown.Height;
                            vol.Top = btnDown.Top - vol.Height;
                            btnUp.Top = vol.Top - btnUp.Height;
                            return false;
                        }
                        else
                        {   // Move object down
                            btn.Top = Top;
                            btnDown.Top = btn.Top - btnDown.Height;
                            vol.Top = btnDown.Top - vol.Height;
                            btnUp.Top = vol.Top - btnUp.Height;
                        }
                        return true;
                    });

                // Activate timeout timer
                VolumeBarTimer[screen].Enabled = true;
            }
            else
            {
                // Deactivate timeout timer
                VolumeBarTimer[screen].Enabled = false;

                // Set visibility state
                VolumeBarVisible[screen] = false;

                // Animate control  
                int EndPos = 0;
                int Top = btn.Top;

                Animation.Animate(delegate(int AnimationStep)
                {
                    Top -= AnimationStep;
                    if (Top <= EndPos)
                    {   // Animation has completed
                        btn.Top = EndPos;
                        btnDown.Top = btn.Top - btnDown.Height;
                        vol.Top = btnDown.Top - vol.Height;
                        btnUp.Top = vol.Top - btnUp.Height;
                        return false;
                    }
                    else
                    {   // Move object up
                        btn.Top = Top;
                        btnDown.Top = btn.Top - btnDown.Height;
                        vol.Top = btnDown.Top - vol.Height;
                        btnUp.Top = vol.Top - btnUp.Height;
                    }
                    return true;
                });

                // Hide volumebar
                vol.Visible = false;

                // Move video window back to original location
                theHost.SetVideoPosition(screen, new Rectangle(0, 100, 1000, 368));
            }
        }

        void volTmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer tmr = (Timer)sender;
            tmr.Enabled = false;
            AnimateVolumeBar(false, tmr.Screen);
        }

        void volumeChange(OMControl sender, int screen)
        {
            // Reset autohide timer
            if (VolumeBarTimer[screen] != null)
            {   
                VolumeBarTimer[screen].Enabled = false;
                VolumeBarTimer[screen].Enabled = true;
            }

            // Set volume (only if control is fully visible, VolumeBarVisible is set to false before it starts to animate)
            theHost.execute(eFunction.setSystemVolume, ((VolumeBar)sender).Value.ToString(), screen.ToString());
        }

        void vol_OnLongClick(OMControl sender, int screen)
        {
            // Mute and unmute volume
            int Volume = (int)theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            if (Volume == -1)
                theHost.execute(eFunction.setSystemVolume, "-2", screen.ToString());
            else
                theHost.execute(eFunction.setSystemVolume, "-1", screen.ToString());
        }

        void Button_UITopBar_VolumeBar_VolumeUpDown_OnLongClick(OMControl sender, int screen)
        {
            // Get step size and direction
            int Step = 0;
            if (sender.Tag != null)
            {
                try
                {
                    Step = (int)sender.Tag;
                }
                catch
                {
                    return;
                }
            }

            // Reset autohide timer
            if (VolumeBarTimer[screen] != null)
                VolumeBarTimer[screen].Enabled = false;

            // Repeat button press
            OMButton btn = sender as OMButton;
            int loopCount = 0;

            // Update current volume
            object o = theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            if (o != null)
            {
                int VolValue = (int)o;
                while (btn.Mode == eModeType.Clicked)
                {
                    loopCount++;

                    VolValue += Step;
                    theHost.execute(eFunction.setSystemVolume, VolValue.ToString(), screen.ToString());

                    Thread.Sleep(100);

                    // Loop safety
                    if (loopCount >= 50)
                        break;
                }
            }

            // Reset autohide timer
            if (VolumeBarTimer[screen] != null)
                VolumeBarTimer[screen].Enabled = true;
        }

        void Button_UITopBar_VolumeBar_VolumeUpDown_OnClick(OMControl sender, int screen)
        {
            // Get step size and direction
            int Step = 0;
            if (sender.Tag != null)
            {
                try
                {
                    Step = (int)sender.Tag;
                }
                catch
                {
                    return;
                }
            }

            // Reset autohide timer
            if (VolumeBarTimer[screen] != null)
                VolumeBarTimer[screen].Enabled = false;

            // Update current volume
            object o = theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            if (o != null)
            {
                int VolValue = (int)o;
                VolValue += Step;
                theHost.execute(eFunction.setSystemVolume, VolValue.ToString(), screen.ToString());
            }

            // Reset autohide timer
            if (VolumeBarTimer[screen] != null)
                VolumeBarTimer[screen].Enabled = true;
        }

        #endregion

        #region InfoBanner

        private bool[] InfoMessageVisible = null;
        private bool[] InfoMessageUpdated = null;
        void ShowInfoMessage(int Screen, string Message)
        {
            // Initialize variables
            if (InfoMessageVisible == null)
                InfoMessageVisible = new bool[theHost.ScreenCount];
            if (InfoMessageUpdated == null)
                InfoMessageUpdated = new bool[theHost.ScreenCount];

            OMBasicShape shp = (OMBasicShape)UIPanel[Screen, "Shape_Info_Background"];
            OMLabel lbl = (OMLabel)UIPanel[Screen, "Label_Info_Background"];
            OMLabel lblFront = (OMLabel)UIPanel[Screen, "Label_Info"];

            if (!InfoMessageVisible[Screen])
            {
                InfoMessageVisible[Screen] = true;
                InfoMessageUpdated[Screen] = false;

                OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        lbl.Text = Message;
                        lblFront.Text = lbl.Text;
                        lbl.FontSize = 45F;
                        lbl.Color = Color.FromArgb(120, lbl.Color);
                        lblFront.Color = Color.FromArgb(120, lbl.Color);

                        shp.Opacity = lbl.Opacity = lblFront.Opacity = 0;

                        lblFront.Visible = true;
                        lbl.Visible = true;
                        shp.Visible = true;


                        // Animation where text starts centered and fades outwards
                        int AnimationState = 0;
                        int Delay = 0;
                        SmoothAnimator Animation = new SmoothAnimator(0.2f);
                        Animation.Animate(delegate(int AnimationStep)
                        {
                            bool ContinueAnimation = false;
                            switch (AnimationState)
                            {
                                case 0: // Fade controls in (fading in is done in the main loop)
                                        ContinueAnimation = true;
                                        AnimationState = 1;
                                    break;

                                case 1:
                                    {   
                                        #region Message text effect
                                        
                                        // Opacity of all controls
                                        if (lblFront.Opacity < 255)
                                        {
                                            lblFront.Opacity += AnimationStep*10;
                                            shp.Opacity = lbl.Opacity = lblFront.Opacity;
                                            ContinueAnimation = true;
                                        }
                                        else
                                        {
                                            shp.Opacity = lbl.Opacity = lblFront.Opacity = 255;
                                        }

                                        // Text effect opactity level
                                        int Opacity = lbl.Color.A - AnimationStep;
                                        if (Opacity > 0)
                                        {
                                            lbl.Color = Color.FromArgb(Opacity, lbl.Color);
                                            ContinueAnimation = true;
                                        }
                                        else
                                        {
                                            lbl.Color = Color.FromArgb(0, lbl.Color);
                                        }

                                        // Text effect font size
                                        if (lbl.FontSize < 200)
                                        {
                                            lbl.FontSize += AnimationStep;
                                            ContinueAnimation = true;
                                        }
                                        else
                                        {
                                            // No final value
                                        }

                                        // Message label opacticy
                                        Opacity = lblFront.Color.A + (int)(AnimationStep * 8F);
                                        if (Opacity < 255)
                                        {
                                            lblFront.Color = Color.FromArgb(Opacity, lbl.Color);
                                            ContinueAnimation = true;
                                        }
                                        else
                                        {
                                            lblFront.Color = Color.FromArgb(255, lbl.Color);
                                        }

                                        #endregion
                                    }
                                    if (!ContinueAnimation)
                                    {
                                        ContinueAnimation = true;
                                        AnimationState = 2;
                                    }
                                    break;
                                case 2:
                                    {   
                                        #region Delay while showing message
                                        
                                        Delay += AnimationStep;
                                        if (Delay < 100)
                                            ContinueAnimation = true;

                                        if (InfoMessageUpdated[Screen])
                                        {
                                            Delay = 0;
                                            InfoMessageUpdated[Screen] = false;
                                        }

                                        #endregion
                                    }
                                    if (!ContinueAnimation)
                                    {
                                        ContinueAnimation = true;
                                        AnimationState = 3;
                                    }
                                    break;
                                case 3:
                                    {
                                        #region Fade controls out

                                        // Opacity of all controls
                                        if (lblFront.Opacity > 0)
                                        {
                                            lblFront.Opacity -= AnimationStep * 8;
                                            shp.Opacity = lbl.Opacity = lblFront.Opacity;
                                            ContinueAnimation = true;
                                        }
                                        else
                                        {
                                            shp.Opacity = lbl.Opacity = lblFront.Opacity = 0;
                                        }

                                        #endregion
                                    }
                                    break;
                                default:
                                    break;
                            }     
                            return ContinueAnimation;
                        });

                        lbl.Visible = false;
                        lblFront.Visible = false;
                        shp.Visible = false;

                        InfoMessageVisible[Screen] = false;
                    });
            }
            else
            {   // Direct update, animation already in progress
                InfoMessageUpdated[Screen] = true;
                lbl.Text = Message;
                lblFront.Text = lbl.Text;
            }
        }

        #endregion

        #region Icons

        void icon_OnClick(OMControl sender, int screen)
        {
            IconManager.UIIcon icon;
            switch (sender.Name)
            {
                case "Button_UITopBar_Icon1":
                    icon = icons.getIcon(1, true);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI|" + screen.ToString(), "IconClicked", ref icon);
                    break;
                case "Button_UITopBar_Icon2":
                    icon = icons.getIcon(1, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI|" + screen.ToString(), "IconClicked", ref icon);
                    break;
                case "Button_UITopBar_Icon3":
                    icon = icons.getIcon(2, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI|" + screen.ToString(), "IconClicked", ref icon);
                    break;
                case "Button_UITopBar_Icon4":
                    icon = icons.getIcon(3, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI|" + screen.ToString(), "IconClicked", ref icon);
                    break;
            }
        }

        void icons_OnIconsChanged()
        {
            theHost.ForEachScreen(delegate(int screen)
            {
                ((OMButton)UIPanel[screen, "Button_UITopBar_Icon1"]).Image = new imageItem(icons.getIcon(1, true).image.image);
                ((OMButton)UIPanel[screen, "Button_UITopBar_Icon2"]).Image = new imageItem(icons.getIcon(1, false).image.image);
                ((OMButton)UIPanel[screen, "Button_UITopBar_Icon3"]).Image = new imageItem(icons.getIcon(2, false).image.image);
                ((OMButton)UIPanel[screen, "Button_UITopBar_Icon4"]).Image = new imageItem(icons.getIcon(3, false).image.image);
            });
        }

        #endregion

        #region Media controls

        void slider_OnSliderMoved(OMSlider sender, int screen)
        {   // Set playback position
            theHost.execute(eFunction.setPosition, screen.ToString(), sender.Value.ToString());
        }

        void playButton_OnClick(OMControl sender, int screen)
        {   // Start / Pause media playback

            // Get media status
            object o = theHost.getData(eGetData.GetMediaStatus, "", screen.ToString());
            
            // Errorcheck
            if (o == null)
                return;

            // Is a normal player active (mp3, video etc)
            if (o.GetType() == typeof(ePlayerStatus))
            {   // Yes this is a normal player 
                ePlayerStatus status = (ePlayerStatus)o;

                // Is any media playing in normal state?
                if (status == ePlayerStatus.Playing)
                {   // Yes, let's pause it
                    if (theHost.execute(eFunction.Pause, screen.ToString()))
                    {   // Change button icon on all screens
                        theHost.ForEachScreen(delegate(int LocalScreen)
                        {   // Let's make sure we only update the correct screens, 
                            // we therefor try to match each screens active zone to the one that initiated this command 
                            // if they are the same we update the icon (more than one screen can be set to the same zone)
                            if (theHost.ZoneHandler.GetZone(LocalScreen) == theHost.ZoneHandler.GetZone(screen))
                            {
                                OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                                btn.Image = theHost.getSkinImage("Play");
                                btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                            }
                        });
                    }
                }
                
                // Is the current state is fast playback, let's set it back to normal
                else if ((status == ePlayerStatus.FastForwarding) || (status == ePlayerStatus.Rewinding))
                {
                    theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), "1");
                    theHost.ForEachScreen(delegate(int LocalScreen)
                    {   // Let's make sure we only update the correct screens, 
                        // we therefor try to match each screens active zone to the one that initiated this command 
                        // if they are the same we update the icon (more than one screen can be set to the same zone)
                        if (theHost.ZoneHandler.GetZone(LocalScreen) == theHost.ZoneHandler.GetZone(screen))
                        {
                            OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                            btn.Image = theHost.getSkinImage("Pause");
                            btn.DownImage = theHost.getSkinImage("Pause.Highlighted");
                        }
                    });
                }

                else
                {   // Current state is stopped, let's start to play
                    if (theHost.execute(eFunction.Play, screen.ToString()))
                    {   // Change button icon on all screens
                        theHost.ForEachScreen(delegate(int LocalScreen)
                        {   // Let's make sure we only update the correct screens, 
                            // we therefor try to match each screens active zone to the one that initiated this command 
                            // if they are the same we update the icon (more than one screen can be set to the same zone)
                            if (theHost.ZoneHandler.GetZone(LocalScreen) == theHost.ZoneHandler.GetZone(screen))
                            {
                                OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                                btn.Image = theHost.getSkinImage("Play");
                                btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                            }
                        });
                    }
                }                
            }

            // Is current media TunedContent (Radio)?
            else if (o.GetType() == typeof(stationInfo))
            {   // Yes, let's pause it
                if (theHost.execute(eFunction.Pause, screen.ToString()))
                {
                    theHost.ForEachScreen(delegate(int LocalScreen)
                    {   // Let's make sure we only update the correct screens, 
                        // we therefor try to match each screens active zone to the one that initiated this command 
                        // if they are the same we update the icon (more than one screen can be set to the same zone)
                        if (theHost.ZoneHandler.GetZone(LocalScreen) == theHost.ZoneHandler.GetZone(screen))
                        {
                            OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                            btn.Image = theHost.getSkinImage("Play");
                            btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                        }
                    });
                }
            }
        }

        void stopButton_OnClick(OMControl sender, int screen)
        {   // Stop media playback
            theHost.execute(eFunction.Stop, screen.ToString());

            // Unload any tuned content
            theHost.execute(eFunction.unloadTunedContent, screen.ToString());
        }

        void rewindButton_OnClick(OMControl sender, int screen)
        {   // Rewind control

            if (theHost.execute(eFunction.scanBackward, screen.ToString()) == true)
                return;

            // Get the current playback speed
            object o = theHost.getData(eGetData.GetPlaybackSpeed, "", screen.ToString());
            if (o == null)
                return;
            float speed = (float)o;
            
            // Do we have normal speed?
            if (speed > 1)
            {   // Yes this is maximum speed

                // Update playback speed
                theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), "1");

                // Update playbutton image
                OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                btn.Image = theHost.getSkinImage("Pause");
                btn.DownImage = theHost.getSkinImage("Pause.Highlighted");
            }
            else
            {   // Let's calculate and set playback speed
                if (speed > 0)
                    speed = speed * -1;
                theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), (2 * speed).ToString());
            }
        }

        void fastForwardButton_OnClick(OMControl sender, int screen)
        {   // Fast forward

            if (theHost.execute(eFunction.scanForward, screen.ToString()) == true)
                return;
            
            object o = theHost.getData(eGetData.GetPlaybackSpeed, "", screen.ToString());
            if (o == null)
                return;
            float speed = (float)o;

            if (speed < 0)
            {
                // Set playback speed
                theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), "1");

                // Update playbutton image
                OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                btn.Image = theHost.getSkinImage("Pause");
                btn.DownImage = theHost.getSkinImage("Pause.Highlighted");
            }
            else
                // Set playback speed
                theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), (2 * speed).ToString());
        }

        void skipForwardButton_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.stepForward, screen.ToString()) == false)
                theHost.execute(eFunction.nextMedia, screen.ToString());
        }

        void skipBackwardButton_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.stepBackward, screen.ToString()) == false)
                theHost.execute(eFunction.previousMedia, screen.ToString());
        }

        void random_OnClick(OMControl sender, int screen)
        {
            theHost.setRandom(screen, (sender.Tag == null));
        }

        void tick_Elapsed(object sender, ElapsedEventArgs e)
        {
            tick.Enabled = false;
            theHost.ForEachScreen(delegate(int screen)
            {
                object o = theHost.getData(eGetData.GetMediaPosition, "", screen.ToString());
                if (o != null)
                {
                    int i = Convert.ToInt32(o);

                    OMLabel lbl = (OMLabel)UIPanel[screen, "Label_UIMediaBar_Elapsed"];
                    OMSlider sldr = (OMSlider)UIPanel[screen, "Slider_UIMediaBar_Slider"];

                    if (i == -1)
                    {
                        // Reset slider and elapsed values
                        lbl.Text = "";
                        sldr.Value = 0;
                    }
                    else if ((i < sldr.Maximum) && (i >= 0))
                    {
                        // Don't update if a user is changing the slider
                        if (sldr.Mode == eModeType.Scrolling)
                            return;

                        // Update slider position
                        sldr.Value = i;

                        // Update elapsed text
                        lbl.Text = String.Format("{0} / {1}", formatTime(i), formatTime(sldr.Maximum));
                    }
                }
            });
            tick.Enabled = true;
        }

        private string formatTime(int seconds)
        {
            return (seconds / 60).ToString() + ":" + (seconds % 60).ToString("00");
        }

        #endregion

        #region Speech

        void speech_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.listenForSpeech);
            SpeechControls(true, screen);
        }
        private void SpeechControls(bool show, int screen)
        {
            OMImage img = (OMImage)UIPanel[screen, "Image_Speech_Speech"];
            OMLabel lbl = (OMLabel)UIPanel[screen, "Label_Speech_Caption"];
            OMBasicShape shp = (OMBasicShape)UIPanel[screen, "Shape_Speech_BackgroundBlock"];

            // Errorcheck
            if (img == null || lbl == null || shp == null)
                return;

            if (show)
            {
                img.Image = theHost.getSkinImage("Speech");
                lbl.Text = "Speak Now";
                lbl.Visible = true;
                img.Visible = true;
                shp.Visible = true;
            }
            else
            {
                lbl.Visible = false;
                img.Visible = false;
                shp.Visible = false;
            }
        }
 
        #endregion

        void statusReset_Elapsed(object sender, ElapsedEventArgs e)
        {
            theHost.ForEachScreen(delegate(int screen)
            {
                ((OMLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]).Text = theHost.getPlayingMedia(screen).Name;
            });
            statusReset.Enabled = false;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.backgroundOperationStatus)
            {
                #region backgroundOperationStatus

                int ArgScreen = OpenMobile.helperFunctions.Arguments.GetScreenFromArg(ref arg2);

                // Is this a update from the speech engine?
                if (arg2 == "Speech")
                {
                    // Yes, check engine status
                    if (arg1 == "Engine Ready!")
                    {   // Engine is ready, Show speech button
                        theHost.ForEachScreen(delegate(int screen)
                        {
                            UIPanel[screen, "Button_Speech_Speech"].Visible = true;
                        });
                        return;
                    }
                    else if (arg1 == "Processing...")
                    {   // Engine is processing speech
                        // TODO: Add parameter to speech engine that holds the active screen number
                        ((OMImage)UIPanel[0, "Image_Speech_Speech"]).Image = theHost.getSkinImage("Processing");
                        ((OMLabel)UIPanel[0, "Label_Speech_Caption"]).Text = "Processing...";
                        return;
                    }
                }

                // Reset status update timer
                statusReset.Enabled = false;
                statusReset.Enabled = true;

                eDataType DataType = OpenMobile.helperFunctions.Arguments.GetDataTypeFromArg(arg3);
                switch (DataType)
                {
                    case eDataType.Update:
                        {
                            if (ArgScreen < 0)
                            {
                                theHost.ForEachScreen(delegate(int screen)
                                {
                                    ((OMLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]).Text = arg1;
                                });
                            }
                            else
                            {   // Directly update the given screen
                                ((OMLabel)UIPanel[ArgScreen, "Label_UITopBar_TrackTitle"]).Text = arg1;
                            }
                        }
                        break;
                    case eDataType.PopUp:
                        {
                            if (ArgScreen < 0)
                            {
                                theHost.ForEachScreen(delegate(int screen)
                                {
                                    ShowInfoMessage(screen, arg1);
                                });
                            }
                            else
                            {   // Directly update the given screen
                                ShowInfoMessage(ArgScreen, arg1);
                            }
                        }
                        break;
                    default:
                        {
                            if (ArgScreen < 0)
                            {   // Global message
                                theHost.ForEachScreen(delegate(int screen)
                                {
                                    OMAnimatedLabel title = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                                    title.Transition(eAnimation.UnveilRight, arg1, 25);
                                });
                            }
                            else
                            {   // Screen specific message
                                OMAnimatedLabel title = (OMAnimatedLabel)UIPanel[ArgScreen, "Label_UITopBar_TrackTitle"]; //6
                                title.Transition(eAnimation.UnveilRight, arg1, 25);
                            }
                        }
                        break;
                }
                #endregion
            }

            else if (function == eFunction.systemVolumeChanged)
            {
                #region systemVolumeChanged

                if (arg1 == "-1")
                {   // Volume muted
                    theHost.ForEachScreen(delegate(int screen)
                    {   // Make sure we update the correct screens by comparing the active zone to the zone in the event (volumeevents sends audioinstance)
                        if (arg2 == theHost.ZoneHandler.GetZone(screen).AudioDeviceInstance.ToString())
                        {   // Update volume button icon
                            OMButton b = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_Volume"];
                            b.Image = theHost.getSkinImage("VolumeButtonMuted");
                            b.FocusImage = theHost.getSkinImage("VolumeButtonMutedFocus");
                            // Show infomessage
                            ShowInfoMessage(screen, "Muted");
                        }
                    });
                }
                else
                {   // Volume change
                    if (StoredData.SystemSettings.VolumeChangesVisible)
                    {
                        theHost.ForEachScreen(delegate(int screen)
                        {   // Make sure we update the correct screens by comparing the active zone to the zone in the event (volumeevents sends audioinstance)
                            if (arg2 == theHost.ZoneHandler.GetZone(screen).AudioDeviceInstance.ToString())
                            {   // Show volume control to indicate new volume
                                OMButton b = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_Volume"];
                                b.Image = theHost.getSkinImage("VolumeButton");
                                b.FocusImage = theHost.getSkinImage("VolumeButtonFocus");

                                // Extract current volume 
                                int VolValue = 0;
                                if (int.TryParse(arg1, out VolValue))
                                {
                                    // Is this a volume adjustment or a unmute?
                                    if (VolValue >= 0)
                                    {   // Volume adjustment, Set volume bar value
                                        VolumeBar vol = (VolumeBar)UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];
                                        vol.Value = VolValue;
                                        // Show volume bar
                                        AnimateVolumeBar(true, screen);
                                        // Show infomessage
                                        ShowInfoMessage(screen, String.Format("Volume {0}%", VolValue));
                                    }
                                }
                            }
                        });
                    }
                }

                #endregion
            }

            else if (function == eFunction.stopListeningForSpeech)
            {
                #region stopListeningForSpeech

                // hide speech controls 
                // TODO: Add parameter to speech engine that holds the active screen number
                theHost.ForEachScreen(delegate(int screen)
                {
                    SpeechControls(false, screen);
                });

                #endregion
            }
               
            else if (function == eFunction.pluginLoadingComplete)
            {
                #region pluginLoadingComplete

                // Set video window position according to what the local skin can use
                theHost.ForEachScreen(delegate(int screen)
                {
                    theHost.SetVideoPosition(screen, new Rectangle(0, 100, 1000, 368));
                });

                #endregion
            }
        }

        void theHost_OnMediaEvent(eFunction function, Zone zone, string arg)
        {
            if (function == eFunction.loadTunedContent)
            {
                // TODO: This should not be here! This is specific code based on another plugin!
                #region PANDORA

                if (arg == "Pandora")
                {   // Show rating buttons
                    theHost.ForEachScreen(delegate(int screen)
                    {
                        if (theHost.ZoneHandler.GetZone(screen) == zone)
                        {
                            OMButton rw = (OMButton)UIPanel[screen, "Button_UIMediaBar_Rewind"]; //12
                            OMButton ff = (OMButton)UIPanel[screen, "Button_UIMediaBar_FastForward"]; //15;
                            rw.Image = theHost.getSkinImage("ThumbsDown");
                            rw.DownImage = imageItem.NONE;
                            ff.Image = theHost.getSkinImage("ThumbsUp");
                            ff.DownImage = imageItem.NONE;
                        }
                    });
                }

                #endregion
            }

            else if (function == eFunction.unloadTunedContent)
            {
                // TODO: This should not be here! This is specific code based on another plugin!
                #region PANDORA

                if (arg == "Pandora")
                {   // Show rating buttons
                    theHost.ForEachScreen(delegate(int screen)
                    {
                        if (theHost.ZoneHandler.GetZone(screen) == zone)
                        {
                            OMButton rw = (OMButton)UIPanel[screen, "Button_UIMediaBar_Rewind"]; //12
                            OMButton ff = (OMButton)UIPanel[screen, "Button_UIMediaBar_FastForward"]; //15;
                            rw.Image = theHost.getSkinImage("Rewind", true);
                            rw.DownImage = theHost.getSkinImage("Rewind.Highlighted");
                            ff.Image = theHost.getSkinImage("FastForward", true);
                            ff.DownImage = theHost.getSkinImage("FastForward.Highlighted");
                        }
                    });
                }
            
                #endregion
            }

            else if (function == eFunction.Play)
            {  
                #region Play

                // get media information
                mediaInfo info = theHost.getPlayingMedia(zone);
                // Errorcheck
                if (info == null)
                    return;

                // Get coverart
                imageItem it = new imageItem(info.coverArt);

                // Get tuned content info
                tunedContentInfo TunedContentInfo = theHost.getData(eGetData.GetTunedContentInfo, "", zone.ToString()) as tunedContentInfo;

                theHost.ForEachScreen(delegate(int screen)
                {
                    // Only update information on the correct screen
                    if (theHost.ZoneHandler.GetZone(screen) == zone)
                    {
                        OMAnimatedLabel title = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                        OMAnimatedLabel artist = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackAlbum"]; //7
                        OMAnimatedLabel album = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackArtist"]; //8
                        OMImage cover = (OMImage)UIPanel[screen, "Image_UITopBar_Cover"]; //9
                        // Errorcheck
                        if (title == null || artist == null || album == null || cover == null)
                            return;

                        // Update radio info
                        if ((info.Type == eMediaType.Radio) && (TunedContentInfo != null))
                        {
                            #region Radio info

                            // Set texts
                            if (title.Text != TunedContentInfo.currentStation.stationName)
                                title.Transition(eAnimation.UnveilRight, TunedContentInfo.currentStation.stationName, 50);
                            if (artist.Text != info.Name)
                                artist.Transition(eAnimation.UnveilRight, info.Name, 50);
                            if (album.Text != info.Album)
                                album.Transition(eAnimation.UnveilRight, info.Album, 50);

                            // Set cover art
                            if (info.coverArt == null)
                                // Use default radio icon if no graphic is present
                                cover.Image = theHost.getSkinImage("Radio");
                            else
                                // Use graphic from radio
                                cover.Image = new imageItem(info.coverArt);

                            // Rescale cover image
                            /*
                            if (cover.Image.image.Height < cover.Image.image.Width)
                            {
                                cover.Height = (int)(cover.Width * ((float)cover.Image.image.Height / cover.Image.image.Width));
                                cover.Top = 2 + (85 - cover.Height) / 2;
                            }
                            */
                            #endregion
                        }

                        // Update player info
                        else
                        {
                            #region Player info

                            // Update texts
                            string s = String.Format("[{0}] {1}", zone.Name, info.Name);
                            if (title.Text != s)
                                title.Transition(eAnimation.UnveilRight, s, 50);
                            if (String.IsNullOrEmpty(title.Text))
                                title.Transition(eAnimation.UnveilRight, info.Location, 50);
                            if (artist.Text != info.Name)
                                artist.Transition(eAnimation.UnveilRight, info.Artist, 50);
                            if (album.Text != info.Album)
                                album.Transition(eAnimation.UnveilRight, info.Album, 50);

                            // Update cover image
                            cover.Image = it;
                            // Use default image if no cover is present from any source
                            if ((cover.Image == null) || (cover.Image.image == null))
                                cover.Image = theHost.getSkinImage("Unknown Album");

                            // Rescale cover image
                            /*
                            if ((cover.Image.image != null) && (cover.Image.image.Height < cover.Image.image.Width))
                            {
                                cover.Height = (int)(cover.Width * ((float)cover.Image.image.Height / cover.Image.image.Width));
                                cover.Top = 2 + (85 - cover.Height) / 2;
                            }
                            else
                            {
                                cover.Height = 85;
                                cover.Top = 2;
                            }
                            */

                            #endregion
                        }

                        OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                        btn.Image = theHost.getSkinImage("Pause");
                        btn.DownImage = theHost.getSkinImage("Pause.Highlighted");
                        ((OMSlider)UIPanel[screen, "Slider_UIMediaBar_Slider"]).Maximum = info.Length;
                    }
                });

                #endregion
            }

            else if (function == eFunction.setPlaybackSpeed)
            {
                #region setPlaybackSpeed

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen) == zone)
                    {
                        OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                        btn.Image = theHost.getSkinImage("Play");
                        btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                    }
                });

                #endregion
            }

            else if (function == eFunction.Stop)
            {
                #region Stop

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen) == zone)
                    {
                        OMAnimatedLabel title = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                        OMAnimatedLabel artist = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackAlbum"]; //7
                        OMAnimatedLabel album = (OMAnimatedLabel)UIPanel[screen, "Label_UITopBar_TrackArtist"]; //8
                        OMImage cover = (OMImage)UIPanel[screen, "Image_UITopBar_Cover"]; //9
                        OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                        OMLabel lbl = (OMLabel)UIPanel[screen, "Label_UIMediaBar_Elapsed"];
                        OMSlider sldr = (OMSlider)UIPanel[screen, "Slider_UIMediaBar_Slider"];

                        title.Text = "";
                        artist.Text = "";
                        album.Text = "";
                        cover.Image = imageItem.NONE;
                        btn.Image = theHost.getSkinImage("Play");
                        btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                        lbl.Text = "";
                        sldr.Value = 0;
                    }
                });

                #endregion
            }

            else if (function == eFunction.Pause)
            {
                #region Pause

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen) == zone)
                    {
                        OMButton btn = (OMButton)UIPanel[screen, "Button_UIMediaBar_Play"];
                        btn.Image = theHost.getSkinImage("Play");
                        btn.DownImage = theHost.getSkinImage("Play.Highlighted");
                    }
                });
 
                #endregion
            }

            else if (function == eFunction.RandomChanged)
            {
                #region RandomChanged

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen) == zone)
                    {
                        OMButton b = (OMButton)UIPanel[screen, "Button_UIMediaBar_Random"];
                        if (arg == "Disabled")
                        {
                            b.Image = theHost.getSkinImage("random");
                            b.DownImage = theHost.getSkinImage("random.Highlighted");
                            b.Tag = null;
                        }
                        else
                        {
                            b.Image = theHost.getSkinImage("randomOn");
                            b.DownImage = theHost.getSkinImage("randomOn.Highlighted");
                            b.Tag = "Enabled";
                        }
                    }
                });

                #endregion
            }

            else if (function == eFunction.hideVideoWindow)
            {
                #region hideVideoWindow

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen).Screen == zone.Screen)
                    {
                        if (MediaBarVisible[screen])
                            AnimateMediaBar(false, screen);

                        // Hide background
                        UIPanel[screen, "Shape_VideoBackground"].Visible = false;

                        // Hide media bar button
                        UIPanel[screen, "Button_UIMediaBar_Media"].Visible = true;

                        return;
                    }
                });

                #endregion
            }

            else if (function == eFunction.showVideoWindow)
            {
                #region showVideoWindow

                theHost.ForEachScreen(delegate(int screen)
                {
                    if (theHost.ZoneHandler.GetZone(screen).Screen == zone.Screen)
                    {
                        if (!MediaBarVisible[screen])
                            AnimateMediaBar(true, screen);

                        // Show background
                        UIPanel[screen, "Shape_VideoBackground"].Visible = true;

                        // Show media bar button
                        UIPanel[screen, "Button_UIMediaBar_Media"].Visible = false;

                        return;
                    }
                });

                #endregion
            }

            else if ((function == eFunction.ZoneAdded) | (function == eFunction.ZoneRemoved) | (function == eFunction.ZoneSetActive) | (function == eFunction.ZoneUpdated))
            {
                #region ZoneSetActive

                int Screen = -1;
                if (int.TryParse(arg, out Screen))
                {
                    OMButton b = (OMButton)UIPanel[Screen, "Button_UIMediaBar_Zone"];
                    b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));
                    // Show infomessage
                    ShowInfoMessage(Screen, String.Format("Zone '{0}' Activated", zone.Name));
                }
                else
                {   // Manually update each panel
                    theHost.ForEachScreen(delegate(int screen)
                    {
                        OMButton b = (OMButton)UIPanel[screen, "Button_UIMediaBar_Zone"];
                        b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));
                    });
                }

                #endregion
            }

        }

        bool theHost_OnGesture(int screen, string character, string pluginName, string panelName, ref bool handled)
        {
            switch (character)
            {
                case "M":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "NewMedia"))
                    {
                        if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media"))
                            theHost.execute(eFunction.CancelTransition, screen.ToString());
                        else
                            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    }
                    else
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    return true;
                case "R":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Radio"))
                        theHost.execute(eFunction.CancelTransition, screen.ToString());
                    else
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    return true;
                case "H":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    return true;
                case "N":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Navigation"))
                    {
                        if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "ExternalNav"))
                            theHost.execute(eFunction.CancelTransition, screen.ToString());
                        else
                            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    }

                    else
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    return true;
                case " ":
                    skipForwardButton_OnClick(null, int.Parse(screen.ToString()));
                    return true;
                case "I":
                    playButton_OnClick(null, int.Parse(screen.ToString()));
                    return true;
                case "back":
                    skipBackwardButton_OnClick(null, int.Parse(screen.ToString()));
                    return true;
            }
            return false;

        }

        #endregion
    }
}
