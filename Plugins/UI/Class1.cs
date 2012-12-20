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
using OpenMobile.UI;

namespace OpenMobile
{
    [PluginLevel(PluginLevels.UI | PluginLevels.System)]
    public sealed class MainUI : IHighLevel
    {
        private IPluginHost theHost;
        private System.Timers.Timer tick = new System.Timers.Timer();
        private System.Timers.Timer statusReset = new System.Timers.Timer(2100);

        //private StatusBarHandler.DropDownButtonStripContainer DropDown_MainButtonStrip = null;
        //private StatusBarHandler.DropDownButtonStripContainer DropDown_PowerOptionsStrip = null;

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Borte"; }
        }

        public string authorEmail
        {
            get { return "boorte@gmail.com"; }
        }

        public string pluginName
        {
            get { return "UI"; }
        }

        public float pluginVersion
        {
            get { return 1.0F; }
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
            return true;
            //if (message == "AddIcon")
            //{
            //    IconManager.UIIcon icon = data as IconManager.UIIcon;
            //    icons.AddIcon(icon);
            //    return true;
            //}
            //if (message == "RemoveIcon")
            //{
            //    IconManager.UIIcon icon = data as IconManager.UIIcon;
            //    icons.RemoveIcon(icon);
            //    return true;
            //}
            //return false;
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
        OMPanel panelNotifyDropDown = null;
        //ButtonStripContainer ButtonStrip_UIBottomBar;
        //ButtonStripContainer ButtonStrip_NotifyDropdown;        
        //ButtonStrip btnStrip_Media;
        ButtonStrip btnStrip_Media2;
        //ButtonStrip btnStrip_NotifyMain;
        ButtonStrip btnStrip_NotifyPower;

        imageItem imgPopUpMenuButton_Collapse_Focus;
        imageItem imgPopUpMenuButton_Collapse_Down;
        imageItem imgPopUpMenuButton_Collapse_Overlay;
        imageItem imgPopUpMenuButton_Expand_Focus;
        imageItem imgPopUpMenuButton_Expand_Down;
        imageItem imgPopUpMenuButton_Expand_Overlay;

        public eLoadStatus initialize(IPluginHost host)
        {
            UIPanel = new OMPanel("UI");
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);

            //tick.BeginInit();
            //tick.EndInit();
            //tick.Elapsed += new ElapsedEventHandler(tick_Elapsed);
            //tick.Interval = 500;
            //tick.Enabled = true;

            //statusReset.BeginInit();
            //statusReset.EndInit();
            //statusReset.Elapsed += new ElapsedEventHandler(statusReset_Elapsed);

            //MediaBarVisible = new bool[theHost.ScreenCount];
            //VolumeBarVisible = new bool[theHost.ScreenCount];
            //VolumeBarTimer = new Timer[theHost.ScreenCount];

            OMBasicShape Shape_VideoBackground = new OMBasicShape("Shape_VideoBackground", 0, 0, 1000, 600,
                new ShapeData(shapes.Rectangle, Color.Black));
            Shape_VideoBackground.Visible = false;
            UIPanel.addControl(Shape_VideoBackground);

            #region Infobanner

            // A general information label in the center of the screen
            OMBasicShape Shape_Info_Background = new OMBasicShape("Shape_Info_Background", -10, 250, 1020, 100,
                new ShapeData(shapes.Rectangle, Color.FromArgb(210, Color.Black), Color.White, 1));
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

            #region PopUp Menu

            // PopUp menu buttons container
            ButtonStripContainer PopUpButtonStripContainer = new ButtonStripContainer("Container_PopUp_ButtonStrip", 0, 0, 1000, 600);
            PopUpButtonStripContainer.Alignment = ButtonStripContainer.Alignments.Up;
            PopUpButtonStripContainer.AutoSizeMode = OMContainer.AutoSizeModes.AutoSize;
            PopUpButtonStripContainer.ButtonSize = new Size(300, 64);

            // Configure the actual container to match the skin
            PopUpButtonStripContainer.Container.ShapeData = new ShapeData(shapes.Rectangle, Color.FromArgb(235, Color.Black), Color.FromArgb(50, Color.White), 2);
            PopUpButtonStripContainer.Container.ScrollBar_Vertical_Enabled = true;
            PopUpButtonStripContainer.Container.ScrollBar_Horizontal_Enabled = true;
            PopUpButtonStripContainer.Container.Visible = false;
            UIPanel.addControl(PopUpButtonStripContainer.Container);

            // Save a reference to the buttonstrip container
            theHost.UIHandler.PopUpMenu = PopUpButtonStripContainer;

            // Temp button strip
            //ButtonStrip PopUpMenuStrip = new ButtonStrip("PopUpMenuStrip");
            //PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Item1",
            //    theHost.UIHandler.BottomBar_PopUpButtonStripContainer.ButtonSize,
            //    178,
            //    theHost.getSkinImage("Aicons|5-content-email"),
            //    "Send email",
            //    false,
            //    null, null, null));

            //for (int i = 2; i < 9; i++)
            //{
            //    PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Item" + i.ToString(),
            //        theHost.UIHandler.BottomBar_PopUpButtonStripContainer.ButtonSize,
            //        178,
            //        theHost.getSkinImage("Aicons|5-content-email"),
            //        "Send email" + i.ToString(),
            //        true,
            //        null, null, null));
            //}
            //PopUpButtonStripContainer.SetButtonStrip(PopUpMenuStrip);           

            #endregion           

            #region BottomBar (Dynamic menu)

            OMImage Image_UIBottomBar_Background = new OMImage("Image_UIBottomBar_Background", 0, 535, 1000, 65, new imageItem(Color.Black, 1000, 65));
            UIPanel.addControl(Image_UIBottomBar_Background);

            OMImage Image_UIBottomBar_Separator = new OMImage("Image_UIBottomBar_Separator", 0, Image_UIBottomBar_Background.Region.Top, 1000, 1, new imageItem(Color.FromArgb(50, Color.White), 1000, 1));
            Image_UIBottomBar_Separator.Opacity = 178;
            UIPanel.addControl(Image_UIBottomBar_Separator);

            // Volume control Down button 
            OMButton Button_UIBottomBar_VolDown = new OMButton("Button_UIBottomBar_VolDown", 10, Image_UIBottomBar_Background.Region.Top, 50, Image_UIBottomBar_Background.Region.Height);
            ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_VolDown, theHost.getSkinImage("Icons|Icon-Speaker-Down").image);
            Button_UIBottomBar_VolDown.GraphicDrawMode = OMButton.DrawModes.FixedSizeLeft;
            Button_UIBottomBar_VolDown.Opacity = 178;
            Button_UIBottomBar_VolDown.OnClick += new userInteraction(Button_UIBottomBar_VolDown_OnClick);
            UIPanel.addControl(Button_UIBottomBar_VolDown);

            // Volume control UP button 
            OMButton Button_UIBottomBar_VolUp = new OMButton("Button_UIBottomBar_VolUp", Button_UIBottomBar_VolDown.Region.Right, Image_UIBottomBar_Background.Region.Top, 50, Image_UIBottomBar_Background.Region.Height);
            ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_VolUp, theHost.getSkinImage("Icons|Icon-Speaker-Up").image);
            Button_UIBottomBar_VolUp.GraphicDrawMode = OMButton.DrawModes.FixedSizeRight;
            Button_UIBottomBar_VolUp.Opacity = 178;
            Button_UIBottomBar_VolUp.OnClick += new userInteraction(Button_UIBottomBar_VolUp_OnClick);
            UIPanel.addControl(Button_UIBottomBar_VolUp);            

            // Back button
            OMButton Button_UIBottomBar_Back = new OMButton("Button_UIBottomBar_Back", 920, Image_UIBottomBar_Background.Region.Top, 80, Image_UIBottomBar_Background.Region.Height);
            ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_Back, theHost.getSkinImage("AIcons|5-content-undo").image);
            Button_UIBottomBar_Back.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            Button_UIBottomBar_Back.GraphicSize = new Size(Image_UIBottomBar_Background.Region.Height, Image_UIBottomBar_Background.Region.Height);
            Button_UIBottomBar_Back.Opacity = 178;
            Button_UIBottomBar_Back.OnClick += new userInteraction(Button_UIBottomBar_Back_OnClick);
            Button_UIBottomBar_Back.OnHoldClick += new userInteraction(Button_UIBottomBar_Back_OnHoldClick);
            UIPanel.addControl(Button_UIBottomBar_Back);

            // Popup menu icon
            OMButton Button_UIBottomBar_MenuPopUp = new OMButton("Button_UIBottomBar_MenuPopUp", Button_UIBottomBar_Back.Region.Left - 80, Image_UIBottomBar_Background.Region.Top, 80, Image_UIBottomBar_Background.Region.Height);
            ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_MenuPopUp, theHost.getSkinImage("AIcons|1-navigation-expand").image);
            imgPopUpMenuButton_Expand_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            imgPopUpMenuButton_Expand_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            imgPopUpMenuButton_Expand_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_MenuPopUp, theHost.getSkinImage("AIcons|1-navigation-collapse").image);
            imgPopUpMenuButton_Collapse_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            imgPopUpMenuButton_Collapse_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            imgPopUpMenuButton_Collapse_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            Button_UIBottomBar_MenuPopUp.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            Button_UIBottomBar_MenuPopUp.GraphicSize = new Size(Button_UIBottomBar_MenuPopUp.Region.Height, Button_UIBottomBar_MenuPopUp.Region.Height);
            Button_UIBottomBar_MenuPopUp.Opacity = 178;
            Button_UIBottomBar_MenuPopUp.OnClick += new userInteraction(Button_UIBottomBar_MenuPopUp_OnClick);
            Button_UIBottomBar_MenuPopUp.Visible = false;
            UIPanel.addControl(Button_UIBottomBar_MenuPopUp);

            // Menu buttons container
            theHost.UIHandler.ControlButtons = new ButtonStripContainer("Container_UIBottomBar_ButtonStrip",0,0,0,0);
            theHost.UIHandler.ControlButtons.Container.Left = theHost.ClientFullArea.Width - Button_UIBottomBar_MenuPopUp.Region.Left;
            theHost.UIHandler.ControlButtons.Container.Top = Button_UIBottomBar_VolUp.Region.Top + 1;
            theHost.UIHandler.ControlButtons.Container.Width = Button_UIBottomBar_MenuPopUp.Region.Left - theHost.UIHandler.ControlButtons.Container.Left;
            theHost.UIHandler.ControlButtons.Container.Height = Button_UIBottomBar_VolUp.Region.Height;
            theHost.UIHandler.ControlButtons.Alignment = ButtonStripContainer.Alignments.CenterLR;
            theHost.UIHandler.ControlButtons.Container.Visible = false;
            UIPanel.addControl(theHost.UIHandler.ControlButtons.Container);

            // Create a buttonstrip
            theHost.UIHandler.ControlButtons.ButtonSize = new Size(80, theHost.UIHandler.ControlButtons.SuggestedButtonSize.Height);
            ButtonStrip btnStrip_Media = new ButtonStrip(this.pluginName, UIPanel.Name, "MediaControl");
            btnStrip_Media.Buttons.Add(Button.CreateSimpleButton("Btn1", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-previous"), null, null, null));
            btnStrip_Media.Buttons.Add(Button.CreateSimpleButton("Btn2", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-rewind"), null, null, null));
            btnStrip_Media.Buttons.Add(Button.CreateSimpleButton("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), MediaButtonStrip_Play_OnClick, null, null));
            btnStrip_Media.Buttons.Add(Button.CreateSimpleButton("Btn4", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9_av_fast_forward"), null, null, null));
            btnStrip_Media.Buttons.Add(Button.CreateSimpleButton("Btn5", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-next"), null, null, null));
            theHost.UIHandler.ControlButtons.SetButtonStrip(btnStrip_Media);

            // Set the main bottombar buttonstrip in OM
            theHost.UIHandler.ControlButtons_MainButtonStrip = btnStrip_Media;            

            // Create a second buttonstrip
            btnStrip_Media2 = new ButtonStrip(this.pluginName, UIPanel.Name, "MediaControl2");
            btnStrip_Media2.Buttons.Add(Button.CreateSimpleButton("Btn1", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), MediaButtonStrip_Play_OnClick, null, null));

            // Progressbar test
            OMProgress progress_UIBottomBar_Test = new OMProgress("progress_UIBottomBar_Test", 295, theHost.UIHandler.ControlButtons.Container.Region.Top + 15, 410, 30);
            progress_UIBottomBar_Test.BackgroundColor = Color.Transparent;
            progress_UIBottomBar_Test.Minimum = 0;
            progress_UIBottomBar_Test.Maximum = 100;
            progress_UIBottomBar_Test.Value = 50;
            progress_UIBottomBar_Test.Style = OMProgress.Styles.shape;
            progress_UIBottomBar_Test.ShowProgressBarValue = true;
            progress_UIBottomBar_Test.FontSize = 10;
            progress_UIBottomBar_Test.TextOffset = new Point(0, 5);            

            // Create shapedata for progressbar background
            ShapeData ShapeData = new ShapeData(shapes.Polygon, 
                Color.Transparent, 
                Color.FromArgb(50, BuiltInComponents.SystemSettings.SkinTextColor),
                2,
                new Point[] 
                { 
                    new Point(0,progress_UIBottomBar_Test.Region.Height-2), // Slighty rounded corner A
                    new Point(0,progress_UIBottomBar_Test.Region.Height-8), // Slighty rounded corner B
                    new Point(2,progress_UIBottomBar_Test.Region.Height-10),// Slighty rounded corner B
                    new Point(progress_UIBottomBar_Test.Region.Width-3,0),  // Slighty rounded corner C
                    new Point(progress_UIBottomBar_Test.Region.Width,3),    // Slighty rounded corner C
                    new Point(progress_UIBottomBar_Test.Region.Width-0,progress_UIBottomBar_Test.Region.Height-3),  // Slighty rounded corner D
                    new Point(progress_UIBottomBar_Test.Region.Width-3,progress_UIBottomBar_Test.Region.Height-0),  // Slighty rounded corner D
                    new Point(2,progress_UIBottomBar_Test.Region.Height-0)  // Slighty rounded corner A
                });
            progress_UIBottomBar_Test.ShapeData = ShapeData;

            // Create shapedata for progressbar bar
            ShapeData BarShapeData = new ShapeData(shapes.Polygon,
                BuiltInComponents.SystemSettings.SkinFocusColor,
                BuiltInComponents.SystemSettings.SkinFocusColor,
                1,
                new Point[] 
                { 
                new Point(5,progress_UIBottomBar_Test.Region.Height-5),
                new Point(progress_UIBottomBar_Test.Region.Width-5,5),
                new Point(progress_UIBottomBar_Test.Region.Width-5,progress_UIBottomBar_Test.Region.Height-5),
                new Point(5,progress_UIBottomBar_Test.Region.Height-5)
                });
            progress_UIBottomBar_Test.ProgressBarShapeData = BarShapeData;
            //progress_UIBottomBar_Test.ImageBackground = theHost.getSkinImage("Objects|ProgressBarBack_400x30");
            //progress_UIBottomBar_Test.ImageBackground.image.SetAlpha(50);
            //progress_UIBottomBar_Test.ImageProgressBar = theHost.getSkinImage("Objects|ProgressBarBar_400x30");
            progress_UIBottomBar_Test.DataSource = "System.CPU.Load";
            UIPanel.addControl(progress_UIBottomBar_Test);
                
            ////OMButton Button_UIBottomBar_Back = DefaultControls.GetHorisontalEdgeButton("Button_UIBottomBar_Back", 831, 533, 160, 70, "", "Back");
            //OMButton Button_UIBottomBar_Back = new OMButton("Button_UIBottomBar_Back", 831, 533, 160, 70);
            //Button_UIBottomBar_Back.Image = theHost.getSkinImage("BackButton", true);
            //Button_UIBottomBar_Back.FocusImage = theHost.getSkinImage("BackButtonFocus", true);
            //Button_UIBottomBar_Back.OnClick += new userInteraction(Back_OnClick);
            //Button_UIBottomBar_Back.Transition = eButtonTransition.None;
            ////UIPanel.addControl(Button_UIBottomBar_Back);

            //OMImage Image_UIMediaBar_Background = new OMImage("Image_UIMediaBar_Background", 0, 604, 1000, 140);
            //Image_UIMediaBar_Background.Image = theHost.getSkinImage("mediaBar", true);
            //Image_UIMediaBar_Background.Transparency = 80;
            //UIPanel.addControl(Image_UIMediaBar_Background);

            //OMButton Button_UIMediaBar_Media = new OMButton("Button_UIMediaBar_Media", 9, 534, 160, 70);
            //Button_UIMediaBar_Media.Image = theHost.getSkinImage("MediaButton", true);
            //Button_UIMediaBar_Media.Transition = eButtonTransition.None;
            //Button_UIMediaBar_Media.FocusImage = theHost.getSkinImage("MediaButtonFocus", true);
            //Button_UIMediaBar_Media.OnClick += new userInteraction(mediaButton_OnClick);
            ////UIPanel.addControl(Button_UIMediaBar_Media);

            //OMSlider Slider_UIMediaBar_Slider = new OMSlider("Slider_UIMediaBar_Slider", 20, 615, 820, 25, 12, 40);
            //Slider_UIMediaBar_Slider.Slider = theHost.getSkinImage("Slider");
            //Slider_UIMediaBar_Slider.SliderBar = theHost.getSkinImage("Slider.Bar");
            //Slider_UIMediaBar_Slider.Maximum = 0;
            //Slider_UIMediaBar_Slider.OnSliderMoved += new OMSlider.slidermoved(slider_OnSliderMoved);
            //UIPanel.addControl(Slider_UIMediaBar_Slider);

            //OMButton Button_UIMediaBar_Play = new OMButton("Button_UIMediaBar_Play", 287, 633, 135, 100);
            //Button_UIMediaBar_Play.Image = theHost.getSkinImage("Play");
            //Button_UIMediaBar_Play.DownImage = theHost.getSkinImage("Play.Highlighted");
            //Button_UIMediaBar_Play.OnClick += new userInteraction(playButton_OnClick);
            //Button_UIMediaBar_Play.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_Play);

            //OMButton Button_UIMediaBar_Stop = new OMButton("Button_UIMediaBar_Stop", 425, 633, 135, 100);
            //Button_UIMediaBar_Stop.Image = theHost.getSkinImage("Stop", true);
            //Button_UIMediaBar_Stop.DownImage = theHost.getSkinImage("Stop.Highlighted", true);
            //Button_UIMediaBar_Stop.OnClick += new userInteraction(stopButton_OnClick);
            //Button_UIMediaBar_Stop.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_Stop);

            //OMButton Button_UIMediaBar_Rewind = new OMButton("Button_UIMediaBar_Rewind", 149, 633, 135, 100);
            //Button_UIMediaBar_Rewind.Image = theHost.getSkinImage("Rewind");
            //Button_UIMediaBar_Rewind.DownImage = theHost.getSkinImage("Rewind.Highlighted");
            //Button_UIMediaBar_Rewind.OnClick += new userInteraction(rewindButton_OnClick);
            //Button_UIMediaBar_Rewind.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_Rewind);

            //OMButton Button_UIMediaBar_FastForward = new OMButton("Button_UIMediaBar_FastForward", 564, 633, 135, 100);
            //Button_UIMediaBar_FastForward.OnClick += new userInteraction(fastForwardButton_OnClick);
            //Button_UIMediaBar_FastForward.Image = theHost.getSkinImage("fastForward");
            //Button_UIMediaBar_FastForward.DownImage = theHost.getSkinImage("fastForward.Highlighted");
            //Button_UIMediaBar_FastForward.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_FastForward);

            //OMButton Button_UIMediaBar_SkipForward = new OMButton("Button_UIMediaBar_SkipForward", 703, 633, 135, 100);
            //Button_UIMediaBar_SkipForward.Image = theHost.getSkinImage("SkipForward", true);
            //Button_UIMediaBar_SkipForward.DownImage = theHost.getSkinImage("SkipForward.Highlighted", true);
            //Button_UIMediaBar_SkipForward.OnClick += new userInteraction(skipForwardButton_OnClick);
            //Button_UIMediaBar_SkipForward.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_SkipForward);

            //OMButton Button_UIMediaBar_SkipBackward = new OMButton("Button_UIMediaBar_SkipBackward", 13, 633, 135, 100);
            //Button_UIMediaBar_SkipBackward.Image = theHost.getSkinImage("SkipBackward", true);
            //Button_UIMediaBar_SkipBackward.DownImage = theHost.getSkinImage("SkipBackward.Highlighted", true);
            //Button_UIMediaBar_SkipBackward.OnClick += new userInteraction(skipBackwardButton_OnClick);
            //Button_UIMediaBar_SkipBackward.Transition = eButtonTransition.None;
            //UIPanel.addControl(Button_UIMediaBar_SkipBackward);

            //OMLabel Label_UIMediaBar_Elapsed = new OMLabel("Label_UIMediaBar_Elapsed", 840, 615, 160, 22);
            //Label_UIMediaBar_Elapsed.OutlineColor = Color.Blue;
            //Label_UIMediaBar_Elapsed.Font = new Font(Font.GenericSansSerif, 16F);
            //Label_UIMediaBar_Elapsed.Format = eTextFormat.OutlineNarrow;
            //UIPanel.addControl(Label_UIMediaBar_Elapsed);

            //OMButton Button_UIMediaBar_Random = new OMButton("Button_UIMediaBar_Random", 845, 635, 55, 40);
            //Button_UIMediaBar_Random.Image = theHost.getSkinImage("random");
            //Button_UIMediaBar_Random.DownImage = theHost.getSkinImage("random.Highlighted");
            //Button_UIMediaBar_Random.OnClick += new userInteraction(random_OnClick);
            //UIPanel.addControl(Button_UIMediaBar_Random);

            //OMButton Button_UIMediaBar_Zone = DefaultControls.GetButton("Button_UIMediaBar_Zone", 845, 680, 140, 50, "", "Zone");
            //Button_UIMediaBar_Zone.OnClick += new userInteraction(Button_UIMediaBar_Zone_OnClick);
            //UIPanel.addControl(Button_UIMediaBar_Zone);

            #endregion

            #region Top bar

            OMImage Image_UITopBar_Background = new OMImage("Image_UITopBar_Background", 0, 0, 1000, 40, new imageItem(Color.Black, 1000,40));
            UIPanel.addControl(Image_UITopBar_Background);

            OMImage Image_UITopBar_Separator = new OMImage("Image_UITopBar_Separator", 0, Image_UITopBar_Background.Region.Bottom, 1000, 1, new imageItem(Color.FromArgb(50, Color.White), 1000, 1));
            UIPanel.addControl(Image_UITopBar_Separator);

            // OM icon
            OMImage Image_UITopBar_OMIcon = new OMImage("Image_UITopBar_OMIcon", 5, Image_UITopBar_Background.Region.Top, Image_UITopBar_Background.Region.Height, Image_UITopBar_Background.Region.Height);
            Image_UITopBar_OMIcon.Image = theHost.getSkinImage("Icons|Icon-OM");
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                Image_UITopBar_OMIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            Image_UITopBar_OMIcon.Opacity = 178;
            UIPanel.addControl(Image_UITopBar_OMIcon);

            // Clock indicator
            OMLabel Label_UITopBar_Clock = new OMLabel("Label_UITopBar_Clock", 880, Image_UITopBar_Background.Region.Top, 120, 22);
            Label_UITopBar_Clock.Text = "{System.Time}";
            Label_UITopBar_Clock.Format = eTextFormat.Bold;
            Label_UITopBar_Clock.Opacity = 178;
            Label_UITopBar_Clock.Font = new Font(Font.Arial, 18);
            Label_UITopBar_Clock.TextAlignment = Alignment.TopRight;
            UIPanel.addControl(Label_UITopBar_Clock);

            // Date indicator
            OMLabel Label_UITopBar_Date = new OMLabel("Label_UITopBar_Date", Label_UITopBar_Clock.Region.Left, Label_UITopBar_Clock.Region.Bottom, Label_UITopBar_Clock.Region.Width, 18);
            Label_UITopBar_Date.Text = "{System.Date}";
            Label_UITopBar_Date.Format = eTextFormat.Normal;
            Label_UITopBar_Date.Opacity = 178;
            Label_UITopBar_Date.Font = new Font(Font.Arial, 12);
            Label_UITopBar_Date.TextAlignment = Alignment.TopRight;
            UIPanel.addControl(Label_UITopBar_Date);

            // Notification icons
            OMContainer Container_UITopBar_Icons = theHost.UIHandler.StatusBarIconsContainer;
            Container_UITopBar_Icons.Name = "Container_UITopBar_Icons";
            Container_UITopBar_Icons.Left = 0;
            Container_UITopBar_Icons.Top = 0;
            Container_UITopBar_Icons.Width = Label_UITopBar_Clock.Region.Left;
            Container_UITopBar_Icons.Height = Image_UITopBar_Background.Region.Height;
            UIPanel.addControl(Container_UITopBar_Icons);

            // Notification drop down button
            OMButton Button_UITopBar_ShowNotifyDropDown = new OMButton("Button_UITopBar_ShowNotifyDropDown", Image_UITopBar_Background.Region.Left, Image_UITopBar_Background.Region.Top, Image_UITopBar_Background.Region.Width, Image_UITopBar_Background.Region.Height);
            Button_UITopBar_ShowNotifyDropDown.OnClick += new userInteraction(Button_UITopBar_ShowNotifyDropDown_OnClick);
            UIPanel.addControl(Button_UITopBar_ShowNotifyDropDown);


            //OMAnimatedLabel2 Label_UITopBar_TrackTitle = new OMAnimatedLabel2("Label_UITopBar_TrackTitle", 240, 3, 620, 28);
            //Label_UITopBar_TrackTitle.TextAlignment = Alignment.CenterLeft;
            //Label_UITopBar_TrackTitle.Format = eTextFormat.Normal;
            //Label_UITopBar_TrackTitle.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            //Label_UITopBar_TrackTitle.AnimationSingle = OMAnimatedLabel2.eAnimation.SlideRightSmooth;
            //UIPanel.addControl(Label_UITopBar_TrackTitle);

            //OMAnimatedLabel2 Label_UITopBar_TrackAlbum = new OMAnimatedLabel2("Label_UITopBar_TrackAlbum", 240, 34, 620, 28);
            //Label_UITopBar_TrackAlbum.TextAlignment = Alignment.CenterLeft;
            //Label_UITopBar_TrackAlbum.Format = eTextFormat.Normal;
            //Label_UITopBar_TrackAlbum.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            //Label_UITopBar_TrackAlbum.AnimationSingle = OMAnimatedLabel2.eAnimation.SlideRightSmooth;
            //UIPanel.addControl(Label_UITopBar_TrackAlbum);

            //OMAnimatedLabel2 Label_UITopBar_TrackArtist = new OMAnimatedLabel2("Label_UITopBar_TrackArtist", 240, 64, 620, 28);
            //Label_UITopBar_TrackArtist.TextAlignment = Alignment.CenterLeftEllipsis;
            //Label_UITopBar_TrackArtist.Format = eTextFormat.Normal;
            //Label_UITopBar_TrackArtist.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            //Label_UITopBar_TrackArtist.AnimationSingle = OMAnimatedLabel2.eAnimation.SlideRightSmooth;
            //UIPanel.addControl(Label_UITopBar_TrackArtist);

            //OMImage Image_UITopBar_Cover = new OMImage("Image_UITopBar_Cover", 143, 3, 90, 90);
            //Image_UITopBar_Cover.Image = imageItem.NONE;
            //UIPanel.addControl(Image_UITopBar_Cover);

            //OMButton Button_UITopBar_Media = new OMButton("Button_UITopBar_Media", 0, 0, 1000, 100);
            //Button_UITopBar_Media.Transition = eButtonTransition.None;
            //Button_UITopBar_Media.OnClick += new userInteraction(mediaButton_OnClick);
            //UIPanel.addControl(Button_UITopBar_Media);

            //OMButton Button_UITopBar_Home = new OMButton("Button_UITopBar_Home", 863, 0, 130, 90);
            //Button_UITopBar_Home.Image = theHost.getSkinImage("HomeButton", true);
            //Button_UITopBar_Home.FocusImage = theHost.getSkinImage("HomeButtonFocus", true);
            //Button_UITopBar_Home.OnHoldClick += new userInteraction(HomeButton_OnClick);
            //Button_UITopBar_Home.OnClick += new userInteraction(Back_OnClick);
            //UIPanel.addControl(Button_UITopBar_Home);

            //#region Volumecontrol

            ////OMButton Button_UITopBar_VolumeBar_Volume = new OMButton("Button_UITopBar_VolumeBar_Volume", 6, 0, 130, 90);
            ////Button_UITopBar_VolumeBar_Volume.Image = theHost.getSkinImage("VolumeButton");
            ////Button_UITopBar_VolumeBar_Volume.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
            ////Button_UITopBar_VolumeBar_Volume.Mode = eModeType.Resizing;
            ////Button_UITopBar_VolumeBar_Volume.Transition = eButtonTransition.None;
            ////Button_UITopBar_VolumeBar_Volume.OnClick += new userInteraction(vol_OnClick);
            ////Button_UITopBar_VolumeBar_Volume.OnHoldClick += new userInteraction(vol_OnHoldClick);
            //////Button_UITopBar_VolumeBar_Volume.Visible = false;
            ////UIPanel.addControl(Button_UITopBar_VolumeBar_Volume);

            //VolumeBar VolumeBar_UITopBar_VolumeBar_Volume = new VolumeBar("VolumeBar_UITopBar_VolumeBar_Volume", 6, -420, 130, 330);
            //VolumeBar_UITopBar_VolumeBar_Volume.Visible = false;
            //VolumeBar_UITopBar_VolumeBar_Volume.OnSliderMoved += new userInteraction(volumeChange);
            //UIPanel.addControl(VolumeBar_UITopBar_VolumeBar_Volume);

            //OMButton Button_UITopBar_VolumeBar_VolumeDown = new OMButton("Button_UITopBar_VolumeBar_VolumeDown", 6, -90, 130, 90);
            //Button_UITopBar_VolumeBar_VolumeDown.FillColor = Color.FromArgb(180, Color.White);
            //Button_UITopBar_VolumeBar_VolumeDown.OverlayImage = new imageItem(OImage.FromWebdingsFont(Button_UITopBar_VolumeBar_VolumeDown.Width, Button_UITopBar_VolumeBar_VolumeDown.Height, "6", 150, eTextFormat.Outline, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinFocusColor, Color.White));
            //Button_UITopBar_VolumeBar_VolumeDown.Transition = eButtonTransition.None;
            //Button_UITopBar_VolumeBar_VolumeDown.Tag = -2;
            //Button_UITopBar_VolumeBar_VolumeDown.OnClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnClick);
            //Button_UITopBar_VolumeBar_VolumeDown.OnHoldClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnHoldClick);
            //UIPanel.addControl(Button_UITopBar_VolumeBar_VolumeDown);

            //OMButton Button_UITopBar_VolumeBar_VolumeUp = new OMButton("Button_UITopBar_VolumeBar_VolumeUp", 6, -510, 130, 90);
            //Button_UITopBar_VolumeBar_VolumeUp.FillColor = Color.FromArgb(180, Color.White);
            //Button_UITopBar_VolumeBar_VolumeUp.OverlayImage = new imageItem(OImage.FromWebdingsFont(Button_UITopBar_VolumeBar_VolumeDown.Width, Button_UITopBar_VolumeBar_VolumeDown.Height, "5", 150, eTextFormat.Outline, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinFocusColor, Color.White));
            //Button_UITopBar_VolumeBar_VolumeUp.Transition = eButtonTransition.None;
            //Button_UITopBar_VolumeBar_VolumeUp.Tag = 2;
            //Button_UITopBar_VolumeBar_VolumeUp.OnClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnClick);
            //Button_UITopBar_VolumeBar_VolumeUp.OnHoldClick += new userInteraction(Button_UITopBar_VolumeBar_VolumeUpDown_OnHoldClick);
            //UIPanel.addControl(Button_UITopBar_VolumeBar_VolumeUp);

            //#endregion

            //#region Icons

            ////OMButton Button_UITopBar_Icon1 = new OMButton("Button_UITopBar_Icon1", 778, 4, 80, 85);
            ////Button_UITopBar_Icon1.OnClick += new userInteraction(icon_OnClick);
            ////UIPanel.addControl(Button_UITopBar_Icon1);

            ////OMButton Button_UITopBar_Icon2 = new OMButton("Button_UITopBar_Icon2", 727, 1, 50, 90);
            ////Button_UITopBar_Icon2.OnClick += new userInteraction(icon_OnClick);
            ////UIPanel.addControl(Button_UITopBar_Icon2);

            ////OMButton Button_UITopBar_Icon3 = new OMButton("Button_UITopBar_Icon3", 676, 1, 50, 90);
            ////Button_UITopBar_Icon3.OnClick += new userInteraction(icon_OnClick);
            ////UIPanel.addControl(Button_UITopBar_Icon3);

            ////OMButton Button_UITopBar_Icon4 = new OMButton("Button_UITopBar_Icon4", 625, 1, 50, 90);
            ////Button_UITopBar_Icon4.OnClick += new userInteraction(icon_OnClick);
            ////UIPanel.addControl(Button_UITopBar_Icon4);

            ////icons.OnIconsChanged += new IconManager.IconsChanged(icons_OnIconsChanged);

            //#endregion


            #endregion

            #region Speech

            //OMButton Button_Speech_Speech = new OMButton("Button_Speech_Speech", 670, 533, 160, 70);
            //Button_Speech_Speech.Image = theHost.getSkinImage("Speak", true);
            //Button_Speech_Speech.FocusImage = theHost.getSkinImage("SpeakFocus", true);
            //Button_Speech_Speech.Visible = false;
            //Button_Speech_Speech.OnClick += new userInteraction(speech_OnClick);
            //UIPanel.addControl(Button_Speech_Speech);

            //OMImage Image_Speech_Speech = new OMImage("Image_Speech_Speech", 350, 200, 300, 300);
            //Image_Speech_Speech.Visible = false;
            //UIPanel.addControl(Image_Speech_Speech);

            //OMLabel Label_Speech_Caption = new OMLabel("Label_Speech_Caption", 300, 150, 400, 60);
            //Label_Speech_Caption.Font = new Font(Font.GenericSerif, 48F);
            //Label_Speech_Caption.Format = eTextFormat.BoldShadow;
            //Label_Speech_Caption.Visible = false;
            //UIPanel.addControl(Label_Speech_Caption);

            //OMBasicShape Shape_Speech_BackgroundBlock = new OMBasicShape("Shape_Speech_BackgroundBlock", 0, 0, 1000, 600);
            //Shape_Speech_BackgroundBlock.Shape = shapes.Rectangle;
            //Shape_Speech_BackgroundBlock.FillColor = Color.FromArgb(130, Color.Black);
            //Shape_Speech_BackgroundBlock.Visible = false;
            //UIPanel.addControl(Shape_Speech_BackgroundBlock);

            #endregion

            theHost.UIHandler.OnHideDropDown += new UIHandler.ShowHideControlDelegate(UIHandler_OnHideDropDown);
            theHost.UIHandler.OnShowDropDown += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowDropDown);

            theHost.UIHandler.OnHideBars += new UIHandler.ShowHideControlDelegate(UIHandler_OnHideBars);
            theHost.UIHandler.OnShowBars += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowBars);

            theHost.UIHandler.OnHidePopUpMenu += new UIHandler.ShowHideControlDelegate(UIHandler_OnHidePopUpMenu);
            theHost.UIHandler.OnShowPopUpMenu += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowPopUpMenu);
            theHost.UIHandler.OnPopupMenuChanged += new UIHandler.PopupMenuEventHandler(UIHandler_OnPopupMenuChanged);

            theHost.UIHandler.OnHideControlButtons += new UIHandler.ShowHideControlDelegate(UIHandler_OnHideControlButtons);
            theHost.UIHandler.OnShowControlButtons += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowControlButtons);
            theHost.UIHandler.OnControlButtonsChanged += new UIHandler.PopupMenuEventHandler(UIHandler_OnControlButtonsChanged);

            UIPanel.Priority = ePriority.UI;
            UIPanel.UIPanel = true;

            UIPanel.Entering += new PanelEvent(UIPanel_Entering);

            manager.loadPanel(UIPanel);

            #region Notification drop down panel

            panelNotifyDropDown = new OMPanel("UI_NotifyDropDown");

            OMImage Image_NotifyDropdown_Background = new OMImage("Image_NotifyDropdown_Background", 0, Image_UITopBar_Separator.Region.Bottom, 1000, 84);//new imageItem(Color.FromArgb(26, 30, 40), 1000, 80));
            Image_NotifyDropdown_Background.BackgroundColor = Color.Black;
            panelNotifyDropDown.addControl(Image_NotifyDropdown_Background);

            OMImage Image_NotifyDropdown_Separator = new OMImage("Image_NotifyDropdown_Separator", 0, Image_NotifyDropdown_Background.Region.Top + 80, 1000, 2, new imageItem(Color.FromArgb(50, Color.White), 1000, 2));//new imageItem(Color.Black, 1000, 1));
            panelNotifyDropDown.addControl(Image_NotifyDropdown_Separator);

            #region Dropdown buttons

            // Menu buttons container
            ButtonStripContainer ButtonStrip_NotifyDropdown = new ButtonStripContainer("Container_NotifyDropdown_ButtonStrip",
                0,
                Image_NotifyDropdown_Background.Region.Top,
                Image_NotifyDropdown_Background.Region.Width,
                80);
            ButtonStrip_NotifyDropdown.Alignment = ButtonStripContainer.Alignments.Left;
            ButtonStrip_NotifyDropdown.ButtonSize = new Size(100, ButtonStrip_NotifyDropdown.SuggestedButtonSize.Height);
            panelNotifyDropDown.addControl(ButtonStrip_NotifyDropdown.Container);
            
            // Set as main container
            theHost.UIHandler.DropDown_ButtonStripContainer = ButtonStrip_NotifyDropdown;

            // Create notify dropdown main buttons
            ButtonStrip btnStrip_NotifyMain = new ButtonStrip(this.pluginName, panelNotifyDropDown.Name, "NotifyMain");
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn0", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Home"), "Home", DropDownButton_Home_OnClick, DropDownButton_Home_OnHoldClick, DropDownButton_Home_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn1", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-MediaZone"), "Zone", DropDownButton_Zone_OnClick, null, null));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn2", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-OM"), "About", DropDownButton_About_OnClick, DropDownButton_About_OnHoldClick, DropDownButton_About_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn3", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Settings"), "Settings", DropDownButton_Settings_OnClick, DropDownButton_Settings_OnHoldClick, DropDownButton_Settings_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn4", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Power"), "Power", DropDownButton_Power_OnClick, DropDownButton_Power_OnHoldClick, DropDownButton_Power_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButtonDummy("Btn5", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButtonDummy("Btn6", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButtonDummy("Btn7", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButtonDummy("Btn8", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.CreateButton("Btn9", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Clear"), "Clear", DropDownButton_Clear_OnClick, DropDownButton_Clear_OnHoldClick, DropDownButton_Clear_OnLongClick));
            theHost.UIHandler.DropDown_ButtonStripContainer.SetButtonStrip(btnStrip_NotifyMain);

            // Set main drop down button strip
            theHost.UIHandler.DropDown_MainButtonStrip = btnStrip_NotifyMain;

            // Create notify dropdown power buttons
            btnStrip_NotifyPower = new ButtonStrip(this.pluginName, panelNotifyDropDown.Name, "NotifyPower");
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn0", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|1-navigation-cancel"), "Quit", PowerOptionsStrip_Quit_OnClick, PowerOptionsStrip_Quit_OnHoldClick, PowerOptionsStrip_Quit_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn1", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-pause-over-video"), "Sleep", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn2", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-pause-over-video"), "Hibernate", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn3", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Power"), "Shutdown", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn4", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-replay"), "Restart", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn5", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-RestartOM"), "Reload", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn6", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Monitor-off"), "Screen off", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButtonDummy("Btn7", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButtonDummy("Btn8", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyPower.Buttons.Add(Button.CreateButton("Btn9", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Clear"), "Cancel", PowerOptionsStrip_Cancel_OnClick, PowerOptionsStrip_Cancel_OnHoldClick, PowerOptionsStrip_Cancel_OnLongClick));

            #endregion            

            // Drop down notification list
            OMContainer NotificationList = theHost.UIHandler.NotificationListControl;
            NotificationList.Name = "Container_NotifyDropdown_NotificationList";
            NotificationList.BackgroundColor = Color.Black;
            NotificationList.Left = 0;
            NotificationList.Top = Image_NotifyDropdown_Separator.Region.Bottom;
            NotificationList.Width = 1000;
            NotificationList.Height = 300;
            NotificationList.OnControlAdded += new userInteraction(NotificationList_OnControlAdded);
            NotificationList.OnControlRemoved += new userInteraction(NotificationList_OnControlRemoved);
            panelNotifyDropDown.addControl(NotificationList);

            OMImage Image_NotifyDropdown_SeparatorEnd = new OMImage("Image_NotifyDropdown_SeparatorEnd", 0, NotificationList.Region.Bottom - 2, 1000, 2, new imageItem(Color.FromArgb(127, Color.White), 1000, 2));
            panelNotifyDropDown.addControl(Image_NotifyDropdown_SeparatorEnd);

            panelNotifyDropDown.Priority = ePriority.UI;
            panelNotifyDropDown.Forgotten = true;
            panelNotifyDropDown.UIPanel = true;
            panelNotifyDropDown.Loaded += new PanelEvent(panelNotifyDropDown_Loaded);
            panelNotifyDropDown.Unloaded += new PanelEvent(panelNotifyDropDown_Unloaded);
            manager.loadPanel(panelNotifyDropDown);

            #endregion

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

            // Set startup client area
            Rectangle ClientArea = new Rectangle();
            ClientArea.Left = 0;
            ClientArea.Top = Image_UITopBar_Separator.Region.Bottom;
            ClientArea.Right = 1000;
            ClientArea.Bottom = Image_UIBottomBar_Separator.Region.Top;
            BuiltInComponents.Host.SetClientArea(ClientArea);

            return eLoadStatus.LoadSuccessful;
        }



        #region Control Buttons

        void UIHandler_OnControlButtonsChanged(object sender, int screen, bool popupAvailable)
        {
            // No action
        }

        void UIHandler_OnShowControlButtons(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            OMContainer container = panel["Container_UIBottomBar_ButtonStrip"] as OMContainer;
            if (container != null)
                container.Visible = true;
        }

        void UIHandler_OnHideControlButtons(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            OMContainer container = panel["Container_UIBottomBar_ButtonStrip"] as OMContainer;
            if (container != null)
                container.Visible = false;
        }

        #endregion

        #region PopUp Menu

        private void PopUpMenuIcon_ShowHide(int screen, bool show)
        {
            // Show or hide the popup menu icon
            OMPanel panel = manager[screen, "UI"];
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;
            if (btnPopUp != null)
            {
                btnPopUp.FocusImage = imgPopUpMenuButton_Collapse_Focus;
                btnPopUp.DownImage = imgPopUpMenuButton_Collapse_Down;
                btnPopUp.OverlayImage = imgPopUpMenuButton_Collapse_Overlay;
                if (show)
                    btnPopUp.Visible = true;
                else
                {
                    btnPopUp.Visible = false;
                    ControlGroup PopUpMenu = new ControlGroup(panel, "_PopUp_");
                    PopUpMenu.Visible = false;
                }
            }
        }

        void UIHandler_OnPopupMenuChanged(object sender, int screen, bool popupAvailable)
        {
            PopUpMenuIcon_ShowHide(screen, popupAvailable);
        }

        void Button_UIBottomBar_MenuPopUp_OnClick(OMControl sender, int screen)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlGroup PopUpMenu = new ControlGroup(panel, "_PopUp_");
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;

            if (!PopUpMenu.Visible)
                theHost.UIHandler.PopUpMenu_Show(screen, false);
            else
                theHost.UIHandler.PopUpMenu_Hide(screen, false);
        }

        void UIHandler_OnShowPopUpMenu(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlGroup PopUpMenu = new ControlGroup(panel, "_PopUp_");
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;
            OMImage Image_UIBottomBar_Background = panel["Image_UIBottomBar_Background"] as OMImage;

            // Place control on the right side of the screen
            PopUpMenu.Right = theHost.ClientArea[screen].Right;
            
            // Limit size of menu popup to the available client area
            OMContainer container = PopUpMenu["Container_PopUp_ButtonStrip"] as OMContainer;
            if (container != null)
            {
                if (container.Height > theHost.ClientArea[screen].Height)
                    container.Height = theHost.ClientArea[screen].Height;
            }

            // Show popup menu
            PopUpMenu.Visible = true;

            // Set button images
            if (btnPopUp != null)
            {
                btnPopUp.FocusImage = imgPopUpMenuButton_Expand_Focus;
                btnPopUp.DownImage = imgPopUpMenuButton_Expand_Down;
                btnPopUp.OverlayImage = imgPopUpMenuButton_Expand_Overlay;
            }
            
            if (!fast)
            {
                // Ensure controls is placed correctly
                PopUpMenu.Top = Image_UIBottomBar_Background.Region.Top;

                // Calculate animation distance
                int AnimationDistance = PopUpMenu.Region.Bottom - theHost.ClientArea[screen].Bottom;

                // Cancel animation if not needed
                if (AnimationDistance != 0)
                {
                    // Animate
                    SmoothAnimator Animation = new SmoothAnimator(4f * BuiltInComponents.SystemSettings.TransitionSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                    {
                        if (AnimationDistance > 0)
                        {
                            AnimationDistance -= AnimationStep;

                            if (AnimationDistance < 0)
                                PopUpMenu.Offset(0, -(AnimationStep + AnimationDistance));
                            else
                                PopUpMenu.Offset(0, -AnimationStep);
                        }

                        // Request a screen redraw
                        panel.Refresh();

                        // Exit animation
                        if (AnimationDistance <= 0)
                            return false;

                        // Continue animation
                        return true;
                    });

                }
            }
            // Ensure controls is placed correctly
            PopUpMenu.Bottom = Image_UIBottomBar_Background.Region.Top;
        }

        void UIHandler_OnHidePopUpMenu(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlGroup PopUpMenu = new ControlGroup(panel, "_PopUp_");
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;

            if (btnPopUp != null)
            {
                btnPopUp.FocusImage = imgPopUpMenuButton_Collapse_Focus;
                btnPopUp.DownImage = imgPopUpMenuButton_Collapse_Down;
                btnPopUp.OverlayImage = imgPopUpMenuButton_Collapse_Overlay;
            }

            if (!fast)
            {
                // Calculate animation distance
                int AnimationDistance = theHost.ClientArea[screen].Bottom - PopUpMenu.Region.Top;

                // Cancel animation if not needed
                if (AnimationDistance != 0)
                {
                    // Animate
                    SmoothAnimator Animation = new SmoothAnimator(4f * BuiltInComponents.SystemSettings.TransitionSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                    {
                        if (AnimationDistance > 0)
                        {
                            AnimationDistance -= AnimationStep;

                            if (AnimationDistance < 0)
                                PopUpMenu.Offset(0, (AnimationStep + AnimationDistance));
                            else
                                PopUpMenu.Offset(0, AnimationStep);
                        }

                        // Request a screen redraw
                        panel.Refresh();

                        // Exit animation
                        if (AnimationDistance <= 0)
                            return false;

                        // Continue animation
                        return true;
                    });

                    // Ensure controls is placed correctly
                    int Offset = theHost.ClientArea[screen].Bottom - PopUpMenu.Region.Top;
                    if (Offset != 0)
                        PopUpMenu.Offset(0, Offset);
                }
            }

            PopUpMenu.Visible = false;
        }

        #endregion

        void ButtonGraphics_SetGlowingFocusImages(OMButton button, OImage baseimage)
        {
            using (OImage oImgBase = baseimage)
            {
                #region Create focus image

                OImage oImgFocus = (OImage)oImgBase.Clone();
                oImgFocus.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                button.FocusImage = new imageItem(oImgFocus);

                #endregion
                #region Create down image

                OImage oImgDown = (OImage)oImgBase.Clone();
                oImgDown.SetAlpha(0.5F); // Slightly darken the image
                oImgDown.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
                button.DownImage = new imageItem(oImgDown);

                #endregion
                #region Create icon image

                OImage oImgOverlay = (OImage)oImgBase.Clone();
                if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                    oImgOverlay.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                button.OverlayImage = new imageItem(oImgOverlay);

                #endregion
            }
        }

        #region Show / Hide Bars

        void UIHandler_OnShowBars(int screen, bool fast)
        {
            theHost.UIHandler.PopUpMenu_Hide(screen, true);

            OMPanel panel = manager[screen, "UI"];
            ControlGroup TopBar = new ControlGroup(panel, "_UITopBar_");
            ControlGroup BottomBar = new ControlGroup(panel, "_UIBottomBar_");

            TopBar.Visible = true;
            BottomBar.Visible = true;
            PopUpMenuIcon_ShowHide(screen, theHost.UIHandler.PopUpMenu.Available(screen));

            if (!fast)
            {
                // Calculate animation distance
                int AnimationDistance_TopBar = -TopBar.Region.Top;
                if (TopBar.Region.Top == 0)
                    AnimationDistance_TopBar = 0;
                int AnimationDistance_BottomBar = BottomBar.Region.Bottom - 600;
                if (BottomBar.Region.Bottom == 600)
                    AnimationDistance_BottomBar = 0;

                // Cancel animation if not needed
                if (AnimationDistance_TopBar != 0 || AnimationDistance_BottomBar != 0)
                {
                    // Animate
                    SmoothAnimator Animation = new SmoothAnimator(0.3f * BuiltInComponents.SystemSettings.TransitionSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                    {
                        // Animate top bar
                        if (AnimationDistance_TopBar > 0)
                        {
                            AnimationDistance_TopBar -= AnimationStep;

                            if (AnimationDistance_TopBar < 0)
                                TopBar.Offset(0, AnimationStep + AnimationDistance_TopBar);
                            else
                                TopBar.Offset(0, AnimationStep);
                        }

                        // Animate bottom bar
                        if (AnimationDistance_BottomBar > 0)
                        {
                            AnimationDistance_BottomBar -= AnimationStep;

                            if (AnimationDistance_BottomBar < 0)
                                BottomBar.Offset(0, -(AnimationStep + AnimationDistance_BottomBar));
                            else
                                BottomBar.Offset(0, -AnimationStep);
                        }

                        // Request a screen redraw
                        panel.Refresh();

                        // Exit animation
                        if (AnimationDistance_TopBar <= 0 && AnimationDistance_BottomBar <= 0)
                            return false;

                        // Continue animation
                        return true;
                    });

                    // Ensure controls is placed correctly
                    int Placement = TopBar.Region.Top;
                    if (Placement != 0)
                    {
                        if (Placement < 0)
                            TopBar.Offset(0, Placement); // To short
                        else
                            TopBar.Offset(0, -Placement); // To long
                    }
                    Placement = BottomBar.Region.Bottom;
                    if (Placement != 600)
                    {
                        if (Placement > 600)
                            BottomBar.Offset(0, -(600 - Placement)); // To short
                        else
                            BottomBar.Offset(0, (600 - Placement)); // To long
                    }
                }
            }

            // Set client area based on placement of controls
            Rectangle ClientArea = new Rectangle();
            ClientArea.Left = 0;
            ClientArea.Top = TopBar.Region.Bottom+1;
            ClientArea.Right = 1000;
            ClientArea.Bottom = BottomBar.Region.Top-1;

            BuiltInComponents.Host.SetClientArea(screen, ClientArea);
        }

        void UIHandler_OnHideBars(int screen, bool fast)
        {
            theHost.UIHandler.PopUpMenu_Hide(screen, true);
            
            OMPanel panel = manager[screen, "UI"];
            ControlGroup TopBar = new ControlGroup(panel, "_UITopBar_");
            ControlGroup BottomBar = new ControlGroup(panel, "_UIBottomBar_");
            if (fast)
            {
                TopBar.Visible = false;
                BottomBar.Visible = false;

                // Set client area to fullscreen
                BuiltInComponents.Host.SetClientArea(screen, BuiltInComponents.Host.ClientFullArea);
            }
            else
            {
                // Calculate animation distance
                int AnimationDistance_TopBar = TopBar.Region.Bottom;
                if (TopBar.Region.Bottom < 0)
                    AnimationDistance_TopBar = 0;
                int AnimationDistance_BottomBar = 600 - BottomBar.Region.Top;
                if (BottomBar.Region.Top > 600)
                    AnimationDistance_BottomBar = 0;

                if (AnimationDistance_TopBar != 0 || AnimationDistance_BottomBar != 0)
                {
                    // Animate
                    SmoothAnimator Animation = new SmoothAnimator(0.3f * BuiltInComponents.SystemSettings.TransitionSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                    {
                        // Animate top bar
                        if (AnimationDistance_TopBar > 0)
                        {
                            AnimationDistance_TopBar -= AnimationStep;
                            TopBar.Offset(0, -AnimationStep);
                        }

                        // Animate bottom bar
                        if (AnimationDistance_BottomBar > 0)
                        {
                            AnimationDistance_BottomBar -= AnimationStep;
                            BottomBar.Offset(0, AnimationStep);
                        }

                        // Request a screen redraw
                        panel.Refresh();

                        // Exit animation
                        if (AnimationDistance_TopBar <= 0 && AnimationDistance_BottomBar <= 0)
                            return false;

                        // Continue animation
                        return true;
                    });
                }
                TopBar.Visible = false;
                BottomBar.Visible = false;

                // Set client area to fullscreen
                BuiltInComponents.Host.SetClientArea(screen, BuiltInComponents.Host.ClientFullArea);
            }
        }

        #endregion

        #region DropDown menu

        void UIHandler_OnShowDropDown(int screen, bool fast)
        {
            theHost.UIHandler.PopUpMenu_Hide(screen, true);
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "UI_NotifyDropDown"))
            {
                if (fast)
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None);
                else
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideDown, 0.5f);
            }
        }

        void UIHandler_OnHideDropDown(int screen, bool fast)
        {
            if (theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName, "UI_NotifyDropDown"))
            {
                if (fast)
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None);
                else
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideUp, 0.5f);
            }
        }

        void NotificationList_OnControlRemoved(OMControl sender, int screen)
        {
            NotificationList_PlaceSeparator(sender.Parent, screen);
        }

        void NotificationList_OnControlAdded(OMControl sender, int screen)
        {
            NotificationList_PlaceSeparator(sender.Parent, screen);
        }

        void NotificationList_PlaceSeparator(OMPanel panel, int screen)
        {
            OMContainer container = panel[screen, "Container_NotifyDropdown_NotificationList"] as OMContainer;
            if (container == null)
                return;

            OMImage imgSeparatorEnd = panel[screen, "Image_NotifyDropdown_SeparatorEnd"] as OMImage;
            if (imgSeparatorEnd == null)
                return;

            // Access a control from a different panel but loaded in the same manager
            OMImage imgBottomBar = panel.Manager[screen, "UI"][screen, "Image_UIBottomBar_Background"] as OMImage;
            if (imgBottomBar == null)
                return;

            Rectangle containerItemsRegion = container.GetControlsArea();

            if ((container.Region.Top + containerItemsRegion.Height) > imgBottomBar.Region.Top)
                imgSeparatorEnd.Top = imgBottomBar.Region.Top - imgSeparatorEnd.Region.Height;
            else
                imgSeparatorEnd.Top = container.Region.Top + containerItemsRegion.Height;

            container.Height = imgSeparatorEnd.Top - container.Region.Top;
        }

        void panelNotifyDropDown_Unloaded(OMPanel sender, int screen)
        {
            // Ensure we show the main menu strip
            theHost.UIHandler.DropDown_ShowMainButtonStrip(screen);
        }

        void panelNotifyDropDown_Loaded(OMPanel sender, int screen)
        {
            // Ensure we show the main menu strip
            theHost.UIHandler.DropDown_ShowMainButtonStrip(screen);
            NotificationList_PlaceSeparator(sender, screen);
        }

        void NotifyDropDown_ToggleVisible(int screen, bool Fast)
        {
            if (!panelNotifyDropDown.IsVisible(screen))
                theHost.UIHandler.DropDown_Show(screen, Fast);
            else
                theHost.UIHandler.DropDown_Hide(screen, Fast);
        }

        #endregion

        #region DropDown buttons

        #region MainButtonStrip Home

        void DropDownButton_Home_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goHome, screen, eGlobalTransition.None);
            NotifyDropDown_ToggleVisible(screen, true);
        }
        void DropDownButton_Home_OnHoldClick(OMControl sender, int screen)
        {
        }
        void DropDownButton_Home_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region MainButtonStrip Power

        void DropDownButton_Power_OnClick(OMControl sender, int screen)
        {
            // Show menu strip for power options
            theHost.UIHandler.DropDown_ButtonStripContainer.SetButtonStrip(screen, btnStrip_NotifyPower);
        }
        void DropDownButton_Power_OnHoldClick(OMControl sender, int screen)
        {
        }
        void DropDownButton_Power_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region MainButtonStrip About

        void DropDownButton_About_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "About"))
            {
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None);
                NotifyDropDown_ToggleVisible(screen, true);
            }
        }
        void DropDownButton_About_OnHoldClick(OMControl sender, int screen)
        {
        }
        void DropDownButton_About_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region MainButtonStrip Settings

        void DropDownButton_Settings_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen, "MainMenu");
            if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings"))
                theHost.execute(eFunction.TransitionToPanel, screen, "MainMenu");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.None);
            NotifyDropDown_ToggleVisible(screen, true);

        }
        void DropDownButton_Settings_OnHoldClick(OMControl sender, int screen)
        {
        }
        void DropDownButton_Settings_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region MainButtonStrip Clear

        void DropDownButton_Clear_OnClick(OMControl sender, int screen)
        {
            theHost.UIHandler.RemoveAllNotifications(screen, false);
            NotifyDropDown_ToggleVisible(screen, true);
        }
        void DropDownButton_Clear_OnHoldClick(OMControl sender, int screen)
        {
        }
        void DropDownButton_Clear_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip Quit

        void PowerOptionsStrip_Quit_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Quit_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.closeProgram);
        }
        void PowerOptionsStrip_Quit_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip Cancel

        void PowerOptionsStrip_Cancel_OnClick(OMControl sender, int screen)
        {
            // Go back to the main menu strip
            theHost.UIHandler.DropDown_ShowMainButtonStrip(screen);
        }
        void PowerOptionsStrip_Cancel_OnHoldClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Cancel_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        void DropDownButton_Zone_OnClick(OMControl sender, int screen)
        {
            NotifyDropDown_ToggleVisible(screen, true);

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

        #endregion

        #region TopBar buttons

        void Button_UITopBar_ShowNotifyDropDown_OnClick(OMControl sender, int screen)
        {
            NotifyDropDown_ToggleVisible(screen, false);
        }

        #endregion

        #region Bottom bar buttons

        void Button_UIBottomBar_VolDown_OnClick(OMControl sender, int screen)
        {
            //theHost.StatusBarHandler.RemoveAllMyNotifications(this);
            //theHost.StatusBarHandler.RemoveNotification(this, "1");

            //theHost.StatusBarHandler.UpdateNotification(this, "1", "Sample update text");

            theHost.UIHandler.Bars_Hide(screen, false);
            Thread.Sleep(2000);
            theHost.UIHandler.Bars_Show(screen, false);
        }

        void Button_UIBottomBar_VolUp_OnClick(OMControl sender, int screen)
        {
            // Add new notification
            //Notification notification = new Notification(this, "1", theHost.getSkinImage("Icons|Icon-MusicIndexer").image, "Media indexing completed", "17 new tracks found");
            //notification.Global = true;
            //theHost.StatusBarHandler.AddNotification(notification);

            //theHost.StatusBarHandler.AddNotification(screen, new Notification(Notification.Styles.Warning, this, "2", theHost.getSkinImage("Icons|Icon - Online").image, "Internet connection detected", "Internet connection is available for plugins"));

            //theHost.UIHandler.Bars_Show(screen, false);

            OMProgress progress = sender.Parent[screen, "progress_UIBottomBar_Test"] as OMProgress;
            if (progress != null)
            {
                if (progress.Tag == null)
                    progress.Value += 10;
                else
                    progress.Value -= 10;

                if (progress.Value >= progress.Maximum)
                    progress.Tag = true;
                else if (progress.Value <= progress.Minimum)
                    progress.Tag = null;
            }

        }

        void Button_UIBottomBar_Back_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goHome, screen);
        }

        void Button_UIBottomBar_Back_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void MediaButtonStrip_Play_OnClick(OMControl sender, int screen)
        {
            // Add a new button
            theHost.UIHandler.ControlButtons.GetButtonStrip(screen).Buttons.Add(Button.CreateSimpleButton("Btn6", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), MediaButtonStrip_Play_OnClick, null, null));

            // Update button
            //theHost.StatusBarHandler.BottomBar_ButtonStripContainer.GetButtonStrip(screen).Buttons["Btn3"] = Button.CreateSimpleButton("Btn6", theHost.StatusBarHandler.BottomBar_ButtonStripContainer.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-pause"), MediaButtonStrip_Play_OnClick, null, null); ;

            //// Toggle buttonstrips
            //if (theHost.StatusBarHandler.BottomBar_ButtonStripContainer.GetButtonStrip(screen) == btnStrip_Media)
            //    theHost.StatusBarHandler.BottomBar_ButtonStripContainer.SetButtonStrip(screen, btnStrip_Media2);
            //else
            //    theHost.StatusBarHandler.BottomBar_ButtonStripContainer.SetButtonStrip(screen, btnStrip_Media);
        }

        #endregion

        void UIPanel_Entering(OMPanel sender, int screen)
        {   // Update initial data

            //// Update active zone
            //Zone zone = theHost.ZoneHandler.GetActiveZone(screen);
            //OMButton b = (OMButton)UIPanel[screen, "Button_UIMediaBar_Zone"];
            //b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));

            //// Update current volume
            //object o = theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            //if (o != null)
            //{
            //    int VolValue = (int)o;
            //    VolumeBar vol = (VolumeBar)UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];
            //    vol.Value = VolValue;                
            //}
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

            // Get main control
            OMControl MainControl = UIPanel[screen, "Image_UIMediaBar_Background"];//(OMButton)UIPanel[screen, "Button_UIMediaBar_Media"];

            // Calculate relative placements of media controls
            int[] RelativePlacements = new int[MediaControls.Count];
            for (int i = 0; i < RelativePlacements.Length; i++)
                RelativePlacements[i] = MediaControls[i].Top - MainControl.Top;

            if (up)
            {   // Move media bar up                
                int EndPos = 470;
                int Top = MainControl.Top;

                // Show mediabar controls
                foreach (OMControl control in MediaControls)
                    control.Visible = true;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                {
                    Top -= AnimationStep;
                    if (Top <= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = true;
            }
            else
            {   // Move media bar down
                int EndPos = 604;
                int Top = MainControl.Top;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
                {
                    Top += AnimationStep;
                    if (Top >= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            MediaControls[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = false;

                // Hide mediabar controls
                foreach (OMControl control in MediaControls)
                    control.Visible = true;

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

                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
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

                Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
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

        void vol_OnHoldClick(OMControl sender, int screen)
        {
            // Mute and unmute volume
            int Volume = (int)theHost.getData(eGetData.GetSystemVolume, "", screen.ToString());
            if (Volume == -1)
                theHost.execute(eFunction.setSystemVolume, "-2", screen.ToString());
            else
                theHost.execute(eFunction.setSystemVolume, "-1", screen.ToString());
        }

        void Button_UITopBar_VolumeBar_VolumeUpDown_OnHoldClick(OMControl sender, int screen)
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
                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF)
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

                                        // Text effect opacity level
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

                                        // Message label opacity
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
            //theHost.ForEachScreen(delegate(int screen)
            //{
            //    object o = theHost.getData(eGetData.GetMediaPosition, "", screen.ToString());
            //    if (o != null)
            //    {
            //        int i = Convert.ToInt32(o);

            //        OMLabel lbl = (OMLabel)UIPanel[screen, "Label_UIMediaBar_Elapsed"];
            //        OMSlider sldr = (OMSlider)UIPanel[screen, "Slider_UIMediaBar_Slider"];

            //        if (i == -1)
            //        {
            //            // Reset slider and elapsed values
            //            lbl.Text = "";
            //            sldr.Value = 0;
            //        }
            //        else if ((i < sldr.Maximum) && (i >= 0))
            //        {
            //            // Don't update if a user is changing the slider
            //            if (sldr.Mode == eModeType.Scrolling)
            //                return;

            //            // Update slider position
            //            sldr.Value = i;

            //            // Update elapsed text
            //            lbl.Text = String.Format("{0} / {1}", formatTime(i), formatTime(sldr.Maximum));
            //        }
            //    }
            //});
            //tick.Enabled = true;
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
                OMLabel lbl = UIPanel[screen, "Label_UITopBar_TrackTitle"] as OMLabel;
                if (lbl != null)
                    lbl.Text = theHost.getPlayingMedia(screen).Name;
            });
            statusReset.Enabled = false;
        }

        #region host events

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            return;

            if (function == eFunction.backgroundOperationStatus)
            {
                return;
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
                                    OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                                    title.Text = arg1;
                                });
                            }
                            else
                            {   // Screen specific message
                                OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[ArgScreen, "Label_UITopBar_TrackTitle"]; //6
                                title.Text = arg1;
                            }
                        }
                        break;
                }
                #endregion
            }

            //else if (function == eFunction.systemVolumeChanged)
            //{
            //    #region systemVolumeChanged

            //    if (arg1 == "-1")
            //    {   // Volume muted
            //        theHost.ForEachScreen(delegate(int screen)
            //        {   // Make sure we update the correct screens by comparing the active zone to the zone in the event (volumeevents sends audioinstance)
            //            if (arg2 == theHost.ZoneHandler.GetZone(screen).AudioDeviceInstance.ToString())
            //            {   // Update volume button icon
            //                OMButton b = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_Volume"];
            //                b.Image = theHost.getSkinImage("VolumeButtonMuted");
            //                b.FocusImage = theHost.getSkinImage("VolumeButtonMutedFocus");
            //                // Show infomessage
            //                ShowInfoMessage(screen, "Muted");
            //            }
            //        });
            //    }
            //    else
            //    {   // Volume change
            //        if (BuiltInComponents.SystemSettings.VolumeChangesVisible)
            //        {
            //            theHost.ForEachScreen(delegate(int screen)
            //            {   // Make sure we update the correct screens by comparing the active zone to the zone in the event (volumeevents sends audioinstance)
            //                if (arg2 == theHost.ZoneHandler.GetZone(screen).AudioDeviceInstance.ToString())
            //                {   // Show volume control to indicate new volume
            //                    OMButton b = (OMButton)UIPanel[screen, "Button_UITopBar_VolumeBar_Volume"];
            //                    b.Image = theHost.getSkinImage("VolumeButton");
            //                    b.FocusImage = theHost.getSkinImage("VolumeButtonFocus");

            //                    // Extract current volume 
            //                    int VolValue = 0;
            //                    if (int.TryParse(arg1, out VolValue))
            //                    {
            //                        // Is this a volume adjustment or a unmute?
            //                        if (VolValue >= 0)
            //                        {   // Volume adjustment, Set volume bar value
            //                            VolumeBar vol = (VolumeBar)UIPanel[screen, "VolumeBar_UITopBar_VolumeBar_Volume"];
            //                            vol.Value = VolValue;
            //                            // Show volume bar
            //                            AnimateVolumeBar(true, screen);
            //                            // Show infomessage
            //                            ShowInfoMessage(screen, String.Format("Volume {0}%", VolValue));
            //                        }
            //                    }
            //                }
            //            });
            //        }
            //    }

            //    #endregion
            //}

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
            return;

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
                        OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                        OMAnimatedLabel2 artist = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackAlbum"]; //7
                        OMAnimatedLabel2 album = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackArtist"]; //8
                        OMImage cover = (OMImage)UIPanel[screen, "Image_UITopBar_Cover"]; //9
                        // Errorcheck
                        if (title == null || artist == null || album == null || cover == null)
                            return;

                        // Update radio info
                        if ((info.Type == eMediaType.Radio) && (TunedContentInfo != null))
                        {
                            #region Radio info

                            // Set texts
                            title.Text = TunedContentInfo.currentStation.stationName;
                            artist.Text = info.Name;
                            album.Text = info.Album;

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
                            title.Text = s;
                            if (String.IsNullOrEmpty(title.Text))
                                title.Text = info.Location;
                            artist.Text = info.Artist;
                            album.Text = info.Album;

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
                        OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
                        OMAnimatedLabel2 artist = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackAlbum"]; //7
                        OMAnimatedLabel2 album = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackArtist"]; //8
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
                        //UIPanel[screen, "Button_UIMediaBar_Media"].Visible = true;

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
                        //UIPanel[screen, "Button_UIMediaBar_Media"].Visible = false;

                        return;
                    }
                });

                #endregion
            }

            else if (function == eFunction.ZoneSetActive)
            {
                #region ZoneSetActive

                int Screen = -1;
                if (int.TryParse(arg, out Screen))
                {
                    theHost.UIHandler.AddNotification(Screen, new Notification(this, "notifyZoneAdded", theHost.getSkinImage("Icons|Icon-MediaZone").image, String.Format("Active zone set to {0}", zone.Name), null));
                    ShowInfoMessage(Screen, String.Format("Zone '{0}' Activated", zone.Name));
                }

                #endregion
            }


            else if ((function == eFunction.ZoneAdded) | (function == eFunction.ZoneRemoved) | (function == eFunction.ZoneSetActive) | (function == eFunction.ZoneUpdated))
            {
                //#region ZoneSetActive

                //int Screen = -1;
                //if (int.TryParse(arg, out Screen))
                //{
                //    // Add notification about zone changed
                //    theHost.StatusBarHandler.AddNotification(Screen, new Notification(this, "notifyZoneAdded", theHost.getSkinImage("Icons|Icon-MediaZone").image, "Internet connection detected", "Internet connection is available for plugins"));

                //    //OMButton b = (OMButton)UIPanel[Screen, "Button_UIMediaBar_Zone"];
                //    //b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));
                //    // Show infomessage
                //    ShowInfoMessage(Screen, String.Format("Zone '{0}' Activated", zone.Name));
                //}
                //else
                //{   // Manually update each panel
                //    theHost.ForEachScreen(delegate(int screen)
                //    {
                //        OMButton b = (OMButton)UIPanel[screen, "Button_UIMediaBar_Zone"];
                //        b.OverlayImage = new imageItem(ButtonGraphic.GetImage(b.Width, b.Height, ButtonGraphic.ImageTypes.ButtonForeground, "", zone.Name));
                //    });
                //}

                //#endregion
            }

        }

        bool theHost_OnGesture(int screen, string character, string pluginName, string panelName, ref bool handled)
        {
            return false;

            switch (character)
            {
                case "M":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    if (!theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "NewMedia"))
                    {
                        if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media"))
                            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    }
                    else
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    return true;
                case "R":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Radio"))
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
                        if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "ExternalNav"))
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

        #endregion
    }
}
