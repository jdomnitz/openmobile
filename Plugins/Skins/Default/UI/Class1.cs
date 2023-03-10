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
using OpenMobile.Media;

namespace OpenMobile
{
    [PluginLevel(PluginLevels.UI | PluginLevels.System)]
    public sealed class MainUI : IHighLevel
    {
        private IPluginHost theHost;
        //private System.Timers.Timer tick = new System.Timers.Timer();
        //private System.Timers.Timer statusReset = new System.Timers.Timer(2100);
        private Notification notificationInternetOnline;

        private StoredData.ScreenInstanceData _ScreenSpecificData = new StoredData.ScreenInstanceData();

        private enum OpacityModes
        {
            None,
            All,
            BackgroundOnly
        }

        private OpacityModes _OpacityMode = OpacityModes.None;
        private int _OpacityLevel = 0;

        //private StatusBarHandler.DropDownButtonStripContainer DropDown_MainButtonStrip = null;
        //private StatusBarHandler.DropDownButtonStripContainer DropDown_PowerOptionsStrip = null;

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Borte"; }
        }

        public string authorEmail
        {
            get { return ""; }
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

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-OM"); }
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
            theHost.OnSystemEvent -= theHost_OnSystemEvent;
            //tick.Dispose();
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

        #endregion

        #region Settings

        public Settings loadSettings()
        {
            Settings settings = new Settings("UI Skin");
            settings.Add(new Setting(SettingTypes.File, "Background.Image", "", "Background image", StoredData.Get(this, "Background.Image")));
            settings.Add(new Setting(SettingTypes.Button, "Background.Clear", String.Empty, "Clear background image"));
            settings.Add(new Setting(SettingTypes.Range, "System.Opacitylevel", "Brightness at night", "The opacity of the screen at night", null, new List<string>() { "0", "255" }, _OpacityLevel.ToString()));

            // Setting for what the opacity setting should affect
            List<string> Texts = new List<string>();
            List<string> Values = new List<string>();
            Texts.Add("Nothing"); Values.Add(((int)OpacityModes.None).ToString());
            Texts.Add("All"); Values.Add(((int)OpacityModes.All).ToString());
            Texts.Add("Background only"); Values.Add(((int)OpacityModes.BackgroundOnly).ToString());
            settings.Add(new Setting(SettingTypes.MultiChoice, "System.OpacityMode", "Brightness affects", "What the brightness adjustment affects", Texts, Values, ((int)_OpacityMode).ToString()));

            settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);

            return settings;
        }

        void settings_OnSettingChanged(int screen, Setting setting)
        {

            if (setting.Name == "Background.Image")
            {
                StoredData.Set(this, setting.Name, setting.Value);
                BackgroundImage_Change(screen, setting.Value);
            }
            else if (setting.Name == "Background.Clear")
            {
                StoredData.Set(this, "Background.Image", String.Empty);
                BackgroundImage_Change(screen, String.Empty);
            }
            else if (setting.Name == "System.Opacitylevel")
            {
                StoredData.Set(this, setting.Name, setting.Value);
                _OpacityLevel = StoredData.GetInt(this, "System.Opacitylevel");
                SetDayNightMode_SetOpacityManually(screen, _OpacityLevel);
            }
            else if (setting.Name == "System.OpacityMode")
            {
                StoredData.Set(this, setting.Name, setting.Value);
                _OpacityMode = (OpacityModes)StoredData.GetInt(this, "System.OpacityMode", 0);
            }
        }

        private void settingsLoadValuesAndSetDefault()
        {
            // Set default settings values
            StoredData.SetDefaultValue(this, "Background.Image", Path.Combine(OM.Host.SkinPath, "Backgrounds", "Highway 1.png"));
            StoredData.SetDefaultValue(this, "System.Opacitylevel", "100");
            StoredData.SetDefaultValue(this, "System.OpacityMode", 0);

            _OpacityMode = (OpacityModes)StoredData.GetInt(this, "System.OpacityMode", 0);
            _OpacityLevel = StoredData.GetInt(this, "System.Opacitylevel", (_OpacityMode == OpacityModes.BackgroundOnly ? 0 : 255));
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
        //ButtonStrip btnStrip_Media2;
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
            manager = new ScreenManager(this);

            settingsLoadValuesAndSetDefault();


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
            OMBasicShape Shape_InfoBanner_Background = new OMBasicShape("Shape_InfoBanner_Background", -10, 250, 1020, 100,
                new ShapeData(shapes.Rectangle, Color.FromArgb(210, Color.Black), Color.White, 1));
            Shape_InfoBanner_Background.Visible = false;
            Shape_InfoBanner_Background.NoUserInteraction = true;
            UIPanel.addControl(Shape_InfoBanner_Background);

            OMLabel Label_InfoBanner_Background = new OMLabel("Label_InfoBanner_Background", -500, 250, 2000, 100);
            Label_InfoBanner_Background.NoUserInteraction = true;
            Label_InfoBanner_Background.Font = new Font(Font.Arial, 45);
            Label_InfoBanner_Background.Visible = false;
            UIPanel.addControl(Label_InfoBanner_Background);

            OMLabel Label_InfoBanner = new OMLabel("Label_InfoBanner_Text", 0, 250, 1000, 100);
            Label_InfoBanner.NoUserInteraction = true;
            Label_InfoBanner.Font = new Font(Font.Arial, 45);
            Label_InfoBanner.Visible = false;
            UIPanel.addControl(Label_InfoBanner);

            #endregion

            #region PopUp Menu

            // PopUp menu buttons container
            ButtonStripContainer PopUpButtonStripContainer = new ButtonStripContainer("Container_PopUp_ButtonStrip", 0, 0, 1000, 600);
            PopUpButtonStripContainer.Alignment = ButtonStripContainer.Alignments.Down;
            PopUpButtonStripContainer.AutoSizeMode = OMContainer.AutoSizeModes.AutoSize;
            PopUpButtonStripContainer.ButtonSize = new Size(300, 64);

            // Configure the actual container to match the skin
            PopUpButtonStripContainer.Container.ShapeData = new ShapeData(shapes.Rectangle, Color.FromArgb(230, Color.Black), Color.FromArgb(50, Color.White), 2);
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

            int bottomBarHeight = 80;
            int bottomBarPlacement = OM.Host.ClientFullArea.Bottom - bottomBarHeight;

            /*
            #region CoverFlow

            // Coverflow background
            int coverFlowHeight = 100;
            OMBasicShape shape_UIBottomBar_MediaImages_Background = new OMBasicShape("shape_UIBottomBar_MediaImages_Background", 0, bottomBarPlacement - coverFlowHeight, 1000, coverFlowHeight,
                new ShapeData(shapes.Rectangle, Color.Black));
            shape_UIBottomBar_MediaImages_Background.Transparency = 15;
            shape_UIBottomBar_MediaImages_Background.NoUserInteraction = true;
            shape_UIBottomBar_MediaImages_Background.Visible = false;
            UIPanel.addControl(shape_UIBottomBar_MediaImages_Background);

            // Separator
            OMBasicShape Shape_UIBottomBar_MediaImages_Background_Separator = new OMBasicShape("Shape_UIBottomBar_MediaImages_Background_Separator", shape_UIBottomBar_MediaImages_Background.Region.Left, shape_UIBottomBar_MediaImages_Background.Region.Top, shape_UIBottomBar_MediaImages_Background.Region.Width, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Black),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Black))
            });
            Shape_UIBottomBar_MediaImages_Background_Separator.Visible = false;
            UIPanel.addControl(Shape_UIBottomBar_MediaImages_Background_Separator);

            // Create coverflow popup when changing media
            OMMediaFlow mediaFlow_UIBottomBar_MediaImages_CoverFlow = new OMMediaFlow("mediaFlow_UIBottomBar_MediaImages_CoverFlow", shape_UIBottomBar_MediaImages_Background.Region.Left, shape_UIBottomBar_MediaImages_Background.Region.Top, shape_UIBottomBar_MediaImages_Background.Region.Width, shape_UIBottomBar_MediaImages_Background.Region.Height);
            OMImageFlow.PreConfigLayout_Flat_ApplyToControl(mediaFlow_UIBottomBar_MediaImages_CoverFlow);
            //mediaFlow_UIBottomBar_MediaImages.MediaInfoFormatString = "{1} - {0}\n{6}";
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.Animation_FadeOutDistance = 6;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.ReflectionsEnabled = false;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.NoUserInteraction = true;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.ImageSize = new Size(mediaFlow_UIBottomBar_MediaImages_CoverFlow.Height * 0.7f, mediaFlow_UIBottomBar_MediaImages_CoverFlow.Height * 0.7f);
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.Visible = false;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.ListSource = OMMediaFlow.ListSources.Buffer;
            UIPanel.addControl(mediaFlow_UIBottomBar_MediaImages_CoverFlow);

            // Mediasource icon
            OMImage image_UIBottomBar_MediaImages_Icon_MediaSource = new OMImage("image_UIBottomBar_MediaImages_Icon_MediaSource",
                mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Left,
                mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Top + 10, 40, 40);
            image_UIBottomBar_MediaImages_Icon_MediaSource.DataSource = "Screen{:S:}.Zone.MediaSource.Icon";
            image_UIBottomBar_MediaImages_Icon_MediaSource.Opacity = 178;
            //image_UIBottomBar_MediaImages_Icon_MediaSource.Visible = false;
            UIPanel.addControl(image_UIBottomBar_MediaImages_Icon_MediaSource);

            // Suffle icon
            OMImage image_UIBottomBar_MediaImages_Icon_Suffle = new OMImage("image_UIBottomBar_MediaImages_Icon_Suffle",
                mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Right - 40,
                mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Top + 10, 40, 40, OM.Host.getSkinImage("AIcons|9-av-shuffle").Copy());
            //image_UIBottomBar_MediaImages_Icon_Suffle.Image.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor, 17);
            image_UIBottomBar_MediaImages_Icon_Suffle.DataSource = "Screen{:S:}.Zone.Shuffle";
            image_UIBottomBar_MediaImages_Icon_Suffle.DataSourceControlsVisibility = true;
            image_UIBottomBar_MediaImages_Icon_Suffle.Opacity = 178;
            image_UIBottomBar_MediaImages_Icon_Suffle.Visible = false;
            UIPanel.addControl(image_UIBottomBar_MediaImages_Icon_Suffle);

            // Repeat icon
            OMImage image_UIBottomBar_MediaImages_Icon_Repeat = new OMImage("image_UIBottomBar_MediaImages_Icon_Repeat",
                image_UIBottomBar_MediaImages_Icon_Suffle.Region.Left,
                image_UIBottomBar_MediaImages_Icon_Suffle.Region.Bottom, 40, 40, OM.Host.getSkinImage("AIcons|9-av-repeat").Copy());
            //image_UIBottomBar_MediaImages_Icon_Repeat.Image.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor, 17);
            image_UIBottomBar_MediaImages_Icon_Repeat.DataSource = "Screen{:S:}.Zone.Repeat";
            image_UIBottomBar_MediaImages_Icon_Repeat.DataSourceControlsVisibility = true;
            image_UIBottomBar_MediaImages_Icon_Repeat.Opacity = 178;
            image_UIBottomBar_MediaImages_Icon_Repeat.Visible = false;
            UIPanel.addControl(image_UIBottomBar_MediaImages_Icon_Repeat);

            #endregion
            */
            OMImage Image_UIBottomBar_Background = new OMImage("Image_UIBottomBar_Background", 0, bottomBarPlacement, 1000, bottomBarHeight, new imageItem(Color.Black, 1000, bottomBarHeight));
            Image_UIBottomBar_Background.Opacity = 100;
            UIPanel.addControl(Image_UIBottomBar_Background);

            //OMBasicShape Shape_UIBottomBar_Background = new OMBasicShape("Shape_UIBottomBar_Background", 0, bottomBarPlacement, 1000, bottomBarHeight,
            //new ShapeData(shapes.Rectangle)
            //{
            //    GradientData = GradientData.CreateVerticalGradient
            //    (
            //        new GradientData.ColorPoint(0, 0.0f, Color.FromArgb(255, 0, 0, 0)),
            //        new GradientData.ColorPoint(0, 1.0f, Color.FromArgb(255, 0, 0, 0)),
            //        new GradientData.ColorPoint(0, 1.0f, Color.FromArgb(255, 30, 30, 50))
            //        )
            //});
            //Shape_UIBottomBar_Background.Opacity = 100;
            //UIPanel.addControl(Shape_UIBottomBar_Background);

            OMBasicShape Shape_UIBottomBar_Separator = new OMBasicShape("Shape_UIBottomBar_Separator", 0, Image_UIBottomBar_Background.Region.Top, 1000, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0f, 0, Color.Black),
                    new GradientData.ColorPoint(0.5f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5f, 0, Color.Black))
            });
            //Shape_UIBottomBar_Separator.Opacity = 128;
            UIPanel.addControl(Shape_UIBottomBar_Separator);


            //OMImage Image_UIBottomBar_Separator = new OMImage("Image_UIBottomBar_Separator", 0, Image_UIBottomBar_Background.Region.Top, 1000, 1);
            //Image_UIBottomBar_Separator.BackgroundColor = Color.FromArgb(50, Color.White);
            //Image_UIBottomBar_Separator.Opacity = 178;
            //Image_UIBottomBar_Separator.Visible = false;
            //UIPanel.addControl(Image_UIBottomBar_Separator);

            #region Volume control

            // Volume button background
            OMButton Button_UIBottomBar_VolBackground = OMButton.PreConfigLayout_BasicStyle_NoBackground("Button_UIBottomBar_VolBackground", 5, Image_UIBottomBar_Background.Region.Top, 140, Image_UIBottomBar_Background.Region.Height, GraphicCorners.None, Color.Transparent);
            Button_UIBottomBar_VolBackground.Opacity = 128;
            UIPanel.addControl(Button_UIBottomBar_VolBackground);

            // Volume down button
            OMButton Button_UIBottomBar_VolDown = new OMButton("Button_UIBottomBar_VolDown", 5, Image_UIBottomBar_Background.Region.Top + 10, 70, Image_UIBottomBar_Background.Region.Height - 15);
            Button_UIBottomBar_VolDown.OverlayImage = theHost.getSkinImage("Icons|Icon-Minus");
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
            {
                Button_UIBottomBar_VolDown.OverlayImage = (imageItem)Button_UIBottomBar_VolDown.OverlayImage.Clone();
                Button_UIBottomBar_VolDown.OverlayImage.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            }
            Button_UIBottomBar_VolDown.FocusImage = (imageItem)Button_UIBottomBar_VolDown.OverlayImage.Clone();
            Button_UIBottomBar_VolDown.FocusImage.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
            Button_UIBottomBar_VolDown.ImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
            Button_UIBottomBar_VolDown.Opacity = 128;
            Button_UIBottomBar_VolDown.OnClick += new userInteraction(Button_UIBottomBar_VolDown_OnClick);
            Button_UIBottomBar_VolDown.OnHoldClick += new userInteraction(Button_UIBottomBar_VolUpDown_OnHoldClick);
            Button_UIBottomBar_VolDown.OnModeChange += new ModeChange(Button_UIBottomBar_VolUpDown_OnModeChange);
            UIPanel.addControl(Button_UIBottomBar_VolDown);

            // Volume up button
            OMButton Button_UIBottomBar_VolUp = new OMButton("Button_UIBottomBar_VolUp", Button_UIBottomBar_VolDown.Region.Right - 1, Button_UIBottomBar_VolDown.Region.Top, 70, Button_UIBottomBar_VolDown.Region.Height);
            Button_UIBottomBar_VolUp.OverlayImage = theHost.getSkinImage("Icons|Icon-Pluss");
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
            {
                Button_UIBottomBar_VolUp.OverlayImage = (imageItem)Button_UIBottomBar_VolUp.OverlayImage.Clone();
                Button_UIBottomBar_VolUp.OverlayImage.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            }
            Button_UIBottomBar_VolUp.FocusImage = (imageItem)Button_UIBottomBar_VolUp.OverlayImage.Clone();
            Button_UIBottomBar_VolUp.FocusImage.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor);
            Button_UIBottomBar_VolUp.ImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
            Button_UIBottomBar_VolUp.Opacity = 128;

            Button_UIBottomBar_VolUp.OnClick += new userInteraction(Button_UIBottomBar_VolUp_OnClick);
            Button_UIBottomBar_VolUp.OnHoldClick += new userInteraction(Button_UIBottomBar_VolUpDown_OnHoldClick);
            Button_UIBottomBar_VolUp.OnModeChange += new ModeChange(Button_UIBottomBar_VolUpDown_OnModeChange);
            UIPanel.addControl(Button_UIBottomBar_VolUp);

            // Volume icon
            OMImage Image_UIBottomBar_VolIcon = new OMImage("Image_UIBottomBar_VolIcon", 40, Image_UIBottomBar_Background.Region.Top+10);
            Image_UIBottomBar_VolIcon.Image = theHost.getSkinImage("Icons|Icon-Speaker3");
            Image_UIBottomBar_VolIcon.NoUserInteraction = true;
            //Image_UIBottomBar_VolIcon.Opacity = 128;
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                Image_UIBottomBar_VolIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            UIPanel.addControl(Image_UIBottomBar_VolIcon);

            // Mute icon
            OMImage Image_UIBottomBar_VolMuted = new OMImage("Image_UIBottomBar_VolMuted", Image_UIBottomBar_VolIcon.Region.Left-15, Image_UIBottomBar_VolIcon.Region.Top);
            Image_UIBottomBar_VolMuted.Image = theHost.getSkinImage("Icons|Icon-SpeakerMuted");
            Image_UIBottomBar_VolMuted.Image.image.Overlay(Color.Red);
            Image_UIBottomBar_VolMuted.NoUserInteraction = true;
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                Image_UIBottomBar_VolMuted.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            else
                Image_UIBottomBar_VolMuted.Image.image.Overlay(Color.Red);
            Image_UIBottomBar_VolMuted.DataSource = "Screen{:S:}.Zone.Device.Volume.Mute";
            UIPanel.addControl(Image_UIBottomBar_VolMuted);

            #endregion

            // Back button
            //OMButton Button_UIBottomBar_Back = new OMButton("Button_UIBottomBar_Back", 920, Image_UIBottomBar_Background.Region.Top, 80, Image_UIBottomBar_Background.Region.Height);
            //ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_Back, theHost.getSkinImage("AIcons|5-content-undo").image);
            //Button_UIBottomBar_Back.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            //Button_UIBottomBar_Back.GraphicSize = new Size(Image_UIBottomBar_Background.Region.Height, Image_UIBottomBar_Background.Region.Height);
            //Button_UIBottomBar_Back.Opacity = 178;
            //Button_UIBottomBar_Back.OnClick += new userInteraction(Button_UIBottomBar_Back_OnClick);
            //Button_UIBottomBar_Back.OnHoldClick += new userInteraction(Button_UIBottomBar_Back_OnHoldClick);
            //UIPanel.addControl(Button_UIBottomBar_Back);


            //OMButton Button_UIBottomBar_Back = new OMButton("Button_UIBottomBar_Back", 925, Image_UIBottomBar_Background.Region.Top+5, 70, Image_UIBottomBar_Background.Region.Height-10);
            //OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            //gd.BackgroundColor = Color.Transparent;
            //gd.BorderColor = Color.FromArgb(50, Color.White);
            //gd.Width = Button_UIBottomBar_Back.Width;
            //gd.Height = Button_UIBottomBar_Back.Height;
            //gd.CornerRadius = 8;
            //gd.Opacity = 178;
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            //Button_UIBottomBar_Back.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            //Button_UIBottomBar_Back.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            //gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackground;
            //Button_UIBottomBar_Back.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            OMButton Button_UIBottomBar_Back = OMButton.PreConfigLayout_BasicStyle_NoBackground("Button_UIBottomBar_Back", 925, Image_UIBottomBar_Background.Region.Top + 5, 70, Image_UIBottomBar_Background.Region.Height - 10, GraphicCorners.None, Color.Transparent);
            Button_UIBottomBar_Back.OverlayImage = theHost.getSkinImage("AIcons|5-content-undo");
            Button_UIBottomBar_Back.OverlayImageDrawMode = OMButton.DrawModes.FixedSizeCentered;
            //Button_UIBottomBar_Back.GraphicSize = new Size(Image_UIBottomBar_Background.Region.Height, Image_UIBottomBar_Background.Region.Height);

            //Button_UIBottomBar_Back.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            //Button_UIBottomBar_Back.GraphicSize = new Size(Image_UIBottomBar_Background.Region.Height, Image_UIBottomBar_Background.Region.Height);
            Button_UIBottomBar_Back.Opacity = 178;
            Button_UIBottomBar_Back.OnClick += new userInteraction(Button_UIBottomBar_Back_OnClick);
            Button_UIBottomBar_Back.OnHoldClick += new userInteraction(Button_UIBottomBar_Back_OnHoldClick);
            UIPanel.addControl(Button_UIBottomBar_Back);

            // Popup menu icon
            OMButton Button_UIBottomBar_MenuPopUp = OMButton.PreConfigLayout_BasicStyle_NoBackground("Button_UIBottomBar_MenuPopUp", Button_UIBottomBar_Back.Region.Left - 69, Image_UIBottomBar_Background.Region.Top + 5, 70, Image_UIBottomBar_Background.Region.Height - 10, GraphicCorners.None, Color.Transparent);
            Button_UIBottomBar_MenuPopUp.OverlayImage = theHost.getSkinImage("AIcons|1-navigation-expand");
            imgPopUpMenuButton_Expand_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            imgPopUpMenuButton_Expand_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            imgPopUpMenuButton_Expand_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            Button_UIBottomBar_MenuPopUp.OverlayImage = theHost.getSkinImage("AIcons|1-navigation-collapse");
            imgPopUpMenuButton_Collapse_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            imgPopUpMenuButton_Collapse_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            imgPopUpMenuButton_Collapse_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            Button_UIBottomBar_MenuPopUp.FocusImage = imageItem.NONE;
            Button_UIBottomBar_MenuPopUp.OverlayImage = imageItem.NONE;
            Button_UIBottomBar_MenuPopUp.DownImage = imageItem.NONE;
            Button_UIBottomBar_MenuPopUp.OnClick += new userInteraction(Button_UIBottomBar_MenuPopUp_OnClick);
            Button_UIBottomBar_MenuPopUp.Opacity = 178;
            //Button_UIBottomBar_MenuPopUp.Visible = false;
            UIPanel.addControl(Button_UIBottomBar_MenuPopUp);

            //// Button to fill the area between the outmost controls in the bottombar
            //OMButton Button_UIBottomBar_Background = OMButton.PreConfigLayout_BasicStyle("Button_UIBottomBar_Background", Button_UIBottomBar_VolBackground.Region.Right - 1, Button_UIBottomBar_VolBackground.Region.Top, Button_UIBottomBar_MenuPopUp.Region.Left - Button_UIBottomBar_VolBackground.Region.Right + 2, Button_UIBottomBar_VolBackground.Region.Height, GraphicCorners.None);
            //Button_UIBottomBar_Background.FocusImage = imageItem.NONE;
            //Button_UIBottomBar_Background.DownImage = imageItem.NONE;
            //Button_UIBottomBar_Background.Opacity = 128;
            //Button_UIBottomBar_Background.Visible = false;
            //UIPanel.addControl(Button_UIBottomBar_Background);



            //OMButton Button_UIBottomBar_MenuPopUp = new OMButton("Button_UIBottomBar_MenuPopUp", Button_UIBottomBar_Back.Region.Left - 80, Image_UIBottomBar_Background.Region.Top, 80, Image_UIBottomBar_Background.Region.Height);
            //ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_MenuPopUp, theHost.getSkinImage("AIcons|1-navigation-expand").image);
            //imgPopUpMenuButton_Expand_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            //imgPopUpMenuButton_Expand_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            //imgPopUpMenuButton_Expand_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            //ButtonGraphics_SetGlowingFocusImages(Button_UIBottomBar_MenuPopUp, theHost.getSkinImage("AIcons|1-navigation-collapse").image);
            //imgPopUpMenuButton_Collapse_Focus = Button_UIBottomBar_MenuPopUp.FocusImage;
            //imgPopUpMenuButton_Collapse_Down = Button_UIBottomBar_MenuPopUp.DownImage;
            //imgPopUpMenuButton_Collapse_Overlay = Button_UIBottomBar_MenuPopUp.OverlayImage;
            //Button_UIBottomBar_MenuPopUp.GraphicDrawMode = OMButton.DrawModes.FixedSizeCentered;
            //Button_UIBottomBar_MenuPopUp.GraphicSize = new Size(Button_UIBottomBar_MenuPopUp.Region.Height, Button_UIBottomBar_MenuPopUp.Region.Height);
            //Button_UIBottomBar_MenuPopUp.Opacity = 178;
            //Button_UIBottomBar_MenuPopUp.OnClick += new userInteraction(Button_UIBottomBar_MenuPopUp_OnClick);
            ////Button_UIBottomBar_MenuPopUp.Visible = false;
            //UIPanel.addControl(Button_UIBottomBar_MenuPopUp);

            // Menu buttons container
            theHost.UIHandler.ControlButtons = new ButtonStripContainer("Container_UIBottomBar_ButtonStrip",0,0,0,0);
            theHost.UIHandler.ControlButtons.Container.Left = theHost.ClientFullArea.Width - Button_UIBottomBar_MenuPopUp.Region.Left;
            theHost.UIHandler.ControlButtons.Container.Top = Button_UIBottomBar_VolBackground.Region.Top;
            theHost.UIHandler.ControlButtons.Container.Width = Button_UIBottomBar_MenuPopUp.Region.Left - theHost.UIHandler.ControlButtons.Container.Left;
            theHost.UIHandler.ControlButtons.Container.Height = Button_UIBottomBar_VolBackground.Region.Height;
            theHost.UIHandler.ControlButtons.Alignment = ButtonStripContainer.Alignments.CenterLR;
            theHost.UIHandler.ControlButtons.Container.Visible = false;
            UIPanel.addControl(theHost.UIHandler.ControlButtons.Container);

            // Create a buttonstrip
            theHost.UIHandler.ControlButtons.ButtonSize = new Size(80, theHost.UIHandler.ControlButtons.SuggestedButtonSize.Height);
            ButtonStrip btnStrip_Media = new ButtonStrip(this.pluginName, UIPanel.Name, "MediaControl");
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn1", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-previous"), ControlButton_Previous_OnClick, null, null, false, false));
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn2", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-rewind"), ControlButton_SeekBackward_OnClick, ControlButton_SeekBackward_OnHoldClick, null, false, false));
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), ControlButton_Play_OnClick, ControlButton_Play_OnHoldClick, null, false, false));
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn4", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-stop"), ControlButton_Stop_OnClick, null, null, false, false));
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn5", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9_av_fast_forward"), ControlButton_SeekForward_OnClick, ControlButton_SeekForward_OnHoldClick, null, false, false));
            btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn6", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-next"), ControlButton_Next_OnClick, null, null, false, false));
            //btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn1", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-previous"), String.Format("Screen{0}.Zone.Previous", DataSource.DataTag_Screen), null, null, false, false));
            //btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn2", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-rewind"), String.Format("Screen{0}.Zone.SeekBackward", DataSource.DataTag_Screen), String.Format("Screen{0}.Zone.SeekBackward{1}", DataSource.DataTag_Screen, DataSource.DataTag_Loop), null, false, false));
            //btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), String.Format("Screen{0}.Zone.Play", DataSource.DataTag_Screen), String.Format("Screen{0}.Zone.Stop", DataSource.DataTag_Screen), null, false, false));
            //btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn4", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9_av_fast_forward"), String.Format("Screen{0}.Zone.SeekForward", DataSource.DataTag_Screen), String.Format("Screen{0}.Zone.SeekForward{1}", DataSource.DataTag_Screen, DataSource.DataTag_Loop), null, false, false));
            //btnStrip_Media.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn5", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-next"), String.Format("Screen{0}.Zone.Next", DataSource.DataTag_Screen), null, null, false, false));
            theHost.UIHandler.ControlButtons.SetButtonStrip(btnStrip_Media);

            // Connect to dataSources for media provider to change button icons according to playback state
            OM.Host.ForEachScreen(delegate(int screen)
            {
                OM.Host.DataHandler.SubscribeToDataSource(screen, "Zone.Playback.Stopped", ControlButtonDataSourceChanged);
                OM.Host.DataHandler.SubscribeToDataSource(screen, "Zone.Playback.Playing", ControlButtonDataSourceChanged);
                OM.Host.DataHandler.SubscribeToDataSource(screen, "Zone.Playback.Paused", ControlButtonDataSourceChanged);
            });
            
            //OM.Host.DataHandler.SubscribeToDataSource("Screen0.Zone.Playback.Stopped"

            

            // Set the main bottombar buttonstrip in OM
            theHost.UIHandler.ControlButtons_MainButtonStrip = btnStrip_Media;            

            //// Create a second buttonstrip
            //btnStrip_Media2 = new ButtonStrip(this.pluginName, UIPanel.Name, "MediaControl2");
            //btnStrip_Media2.Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn1", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), MediaButtonStrip_Play_OnClick, null, null, false, false));

            #region Zone info bar (including volume bar)

            //// Create a background for the zoneinfo bar to allow it to be faded in over other controls)
            //OMImage Image_UIBottomBar_ZoneInfo_Background = new OMImage("Image_UIBottomBar_ZoneInfo_Background",
            //    theHost.UIHandler.ControlButtons.Container.Region.Left,
            //    theHost.UIHandler.ControlButtons.Container.Region.Top,
            //    theHost.UIHandler.ControlButtons.Container.Region.Width,
            //    theHost.UIHandler.ControlButtons.Container.Region.Height);
            //Image_UIBottomBar_ZoneInfo_Background.BackgroundColor = Color.Black;
            //Image_UIBottomBar_ZoneInfo_Background.Visible = false;
            //UIPanel.addControl(Image_UIBottomBar_ZoneInfo_Background);

            //OMImage Image_UIBottomBar_ZoneInfo_Background = new OMImage("Image_UIBottomBar_ZoneInfo_Background", 246, theHost.UIHandler.ControlButtons.Container.Region.Top + 5);
            //Image_UIBottomBar_ZoneInfo_Background.Image = theHost.getSkinImage("Objects|Field-Background-507_45");
            //Image_UIBottomBar_ZoneInfo_Background.Visible = false;
            //UIPanel.addControl(Image_UIBottomBar_ZoneInfo_Background);

            // Preconfigured progressbar used as a volume bar
            OMProgress progress_UIBottomBar_VolumeBar = OMProgress.PreConfigLayout_Triangle("progress_UIBottomBar_ZoneInfo_VolumeBar", 295, theHost.UIHandler.ControlButtons.Container.Region.Top + 12, 410, 60,
                Color.FromArgb(50, BuiltInComponents.SystemSettings.SkinTextColor), BuiltInComponents.SystemSettings.SkinFocusColor);
            progress_UIBottomBar_VolumeBar.DataSource = "Screen{:S:}.Zone.Device.Volume";
            progress_UIBottomBar_VolumeBar.Visible = false;
            UIPanel.addControl(progress_UIBottomBar_VolumeBar);

            OMLabel label_UIBottomBar_VolumeBarLabel = new OMLabel("label_UIBottomBar_ZoneInfo_VolumeBarLabel", progress_UIBottomBar_VolumeBar.Region.Left, progress_UIBottomBar_VolumeBar.Region.Top - 2, progress_UIBottomBar_VolumeBar.Region.Width, 25);
            label_UIBottomBar_VolumeBarLabel.Color = Color.FromArgb(150, BuiltInComponents.SystemSettings.SkinTextColor);
            label_UIBottomBar_VolumeBarLabel.FontSize = 14;
            label_UIBottomBar_VolumeBarLabel.TextAlignment = Alignment.TopLeft;
            label_UIBottomBar_VolumeBarLabel.Text = "Volume:";
            label_UIBottomBar_VolumeBarLabel.Visible = false;
            UIPanel.addControl(label_UIBottomBar_VolumeBarLabel);

            //OMLabel label_UIBottomBar_ZoneName = new OMLabel("label_UIBottomBar_ZoneInfo_ZoneName", progress_UIBottomBar_VolumeBar.Region.Left, progress_UIBottomBar_VolumeBar.Region.Bottom + 2, progress_UIBottomBar_VolumeBar.Region.Width, 20);
            //label_UIBottomBar_ZoneName.Color = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinTextColor);
            //label_UIBottomBar_ZoneName.FontSize = 16;
            //label_UIBottomBar_ZoneName.TextAlignment = Alignment.CenterCenter;
            //label_UIBottomBar_ZoneName.Text = String.Format("{{Screen{0}.Zone.Name}}", DataSource.DataTag_Screen);
            //label_UIBottomBar_ZoneName.Visible = false;
            //UIPanel.addControl(label_UIBottomBar_ZoneName);

            #endregion

            // Bottom bar media info
            OMImage Image_UIBottomBar_MediaInfo_CoverArt = new OMImage("Image_UIBottomBar_MediaInfo_CoverArt", Button_UIBottomBar_VolBackground.Region.Right + 20, Image_UIBottomBar_Background.Region.Top + 2, Image_UIBottomBar_Background.Region.Height - 4, Image_UIBottomBar_Background.Region.Height - 4);
            Image_UIBottomBar_MediaInfo_CoverArt.DataSource = "Screen{:S:}.Zone.MediaInfo.CoverArt";
            UIPanel.addControl(Image_UIBottomBar_MediaInfo_CoverArt);

            OMAnimatedLabel2 Label_UIBottomBar_MediaInfo_Line1 = new OMAnimatedLabel2("Label_UIBottomBar_MediaInfo_Line1", 
                Image_UIBottomBar_MediaInfo_CoverArt.Region.Right, 
                Image_UIBottomBar_MediaInfo_CoverArt.Region.Top, 
                theHost.UIHandler.ControlButtons.Container.Region.Right - Image_UIBottomBar_MediaInfo_CoverArt.Region.Right,
                Image_UIBottomBar_MediaInfo_CoverArt.Height / 2);
            Label_UIBottomBar_MediaInfo_Line1.DataSource = "Screen{:S:}.Zone.MediaText1";
            Label_UIBottomBar_MediaInfo_Line1.FontSize = 22;
            Label_UIBottomBar_MediaInfo_Line1.TextAlignment = Alignment.CenterLeft;
            Label_UIBottomBar_MediaInfo_Line1.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            Label_UIBottomBar_MediaInfo_Line1.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            Label_UIBottomBar_MediaInfo_Line1.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            UIPanel.addControl(Label_UIBottomBar_MediaInfo_Line1);

            OMAnimatedLabel2 Label_UIBottomBar_MediaInfo_Line2 = new OMAnimatedLabel2("Label_UIBottomBar_MediaInfo_Line2",
                Label_UIBottomBar_MediaInfo_Line1.Region.Left,
                Label_UIBottomBar_MediaInfo_Line1.Region.Bottom,
                Label_UIBottomBar_MediaInfo_Line1.Region.Width,
                Label_UIBottomBar_MediaInfo_Line1.Region.Height);
            Label_UIBottomBar_MediaInfo_Line2.DataSource = "Screen{:S:}.Zone.MediaText2";
            Label_UIBottomBar_MediaInfo_Line2.FontSize = 22;
            Label_UIBottomBar_MediaInfo_Line2.TextAlignment = Alignment.CenterLeft;
            Label_UIBottomBar_MediaInfo_Line2.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            Label_UIBottomBar_MediaInfo_Line2.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            Label_UIBottomBar_MediaInfo_Line2.AnimationSingle = OMAnimatedLabel2.eAnimation.CrossFade;
            UIPanel.addControl(Label_UIBottomBar_MediaInfo_Line2);

            //OMLabel Label_UIBottomBar_MediaInfo_Line3 = new OMLabel("Label_UIBottomBar_MediaInfo_Line3",
            //    Label_UIBottomBar_MediaInfo_Line2.Region.Left,
            //    Label_UIBottomBar_MediaInfo_Line2.Region.Bottom,
            //    Label_UIBottomBar_MediaInfo_Line2.Region.Width,
            //    Label_UIBottomBar_MediaInfo_Line2.Region.Height,
            //    "{Screen{:S:}.Zone.MediaInfo.TrackNumber} - {Screen{:S:}.Zone.MediaInfo.Genre}");
            //Label_UIBottomBar_MediaInfo_Line3.FontSize = 14;
            //Label_UIBottomBar_MediaInfo_Line3.TextAlignment = Alignment.CenterLeft;
            //UIPanel.addControl(Label_UIBottomBar_MediaInfo_Line3);            

            // Add invisible button over media info to show control buttons
            OMButton Button_UIBottomBar_ShowMediaInfo = new OMButton("Button_UIBottomBar_ShowMediaInfo", theHost.UIHandler.ControlButtons.Container.Region.Left, theHost.UIHandler.ControlButtons.Container.Region.Top, theHost.UIHandler.ControlButtons.Container.Region.Width, theHost.UIHandler.ControlButtons.Container.Region.Height);
            Button_UIBottomBar_ShowMediaInfo.OnClick += new userInteraction(Button_UIBottomBar_MediaInfo_OnClick);
            UIPanel.addControl(Button_UIBottomBar_ShowMediaInfo);

            // Add invisible button over cover art to goto now playing skin
            OMButton Button_UIBottomBar_MediaInfo_CoverArt = new OMButton("Button_UIBottomBar_MediaInfo_CoverArt", Button_UIBottomBar_VolBackground.Region.Right + 20, Image_UIBottomBar_Background.Region.Top + 2, Image_UIBottomBar_Background.Region.Height - 4, Image_UIBottomBar_Background.Region.Height - 4);
            Button_UIBottomBar_MediaInfo_CoverArt.Command_Click = "Screen{:S:}.Panel.Goto.OMNowPlaying.NowPlaying";
            UIPanel.addControl(Button_UIBottomBar_MediaInfo_CoverArt);

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

            OMImage Image_UITopBar_Background = new OMImage("Image_UITopBar_Background", 0, 0, 1000, 50, new imageItem(Color.Black, 1000,50));
            Image_UITopBar_Background.BackgroundColor = Color.Black;
            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            gd.BackgroundColor1 = Color.Transparent;
            gd.BorderColor = Color.Transparent;
            gd.Width = Image_UITopBar_Background.Width;
            gd.Height = Image_UITopBar_Background.Height;
            gd.CornerRadius = 0;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackground;
            Image_UITopBar_Background.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            Image_UITopBar_Background.Opacity = 100;
            UIPanel.addControl(Image_UITopBar_Background);

            //OMImage Image_UITopBar_Separator = new OMImage("Image_UITopBar_Separator", 0, Image_UITopBar_Background.Region.Bottom, 1000, 1);
            //Image_UITopBar_Separator.BackgroundColor = Color.FromArgb(50, Color.White);
            //Image_UITopBar_Separator.Visible = false;
            //UIPanel.addControl(Image_UITopBar_Separator);

            OMBasicShape Shape_UITopBar_Separator = new OMBasicShape("Shape_UITopBar_Separator", 0, Image_UITopBar_Background.Region.Bottom, 1000, 1, 
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0f, 0, Color.Black),
                    new GradientData.ColorPoint(0.5f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5f, 0, Color.Black))
            });
            UIPanel.addControl(Shape_UITopBar_Separator);
            //OMTestControl tstControl = new OMTestControl();
            //tstControl.Name = "tstControl";
            //tstControl.Left = 0;
            //tstControl.Top = Image_UITopBar_Background.Region.Bottom;
            //tstControl.Width = 1000;
            //tstControl.Height = 1; // 35;
            //UIPanel.addControl(tstControl);


            // OM icon
            OMImage Image_UITopBar_OMIcon = new OMImage("Image_UITopBar_OMIcon", 5, Image_UITopBar_Background.Region.Top, Image_UITopBar_Background.Region.Height, Image_UITopBar_Background.Region.Height);
            Image_UITopBar_OMIcon.Image = theHost.getSkinImage("Icons|Icon-OM");
            if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                Image_UITopBar_OMIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            Image_UITopBar_OMIcon.Opacity = 178;
            UIPanel.addControl(Image_UITopBar_OMIcon);

            // Clock indicator
            OMLabel Label_UITopBar_Clock = new OMLabel("Label_UITopBar_Clock", 880, Image_UITopBar_Background.Region.Top-2, 120, 27);
            Label_UITopBar_Clock.Text = "{System.Time}";
            Label_UITopBar_Clock.Format = eTextFormat.Normal;
            Label_UITopBar_Clock.Opacity = 178;
            Label_UITopBar_Clock.Font = new Font(Font.Arial, 21);
            Label_UITopBar_Clock.TextAlignment = Alignment.TopRight;
            UIPanel.addControl(Label_UITopBar_Clock);

            // Date indicator
            OMLabel Label_UITopBar_Date = new OMLabel("Label_UITopBar_Date", Label_UITopBar_Clock.Region.Left, Label_UITopBar_Clock.Region.Bottom, Label_UITopBar_Clock.Region.Width, 20);
            Label_UITopBar_Date.Text = "{System.Date}";
            Label_UITopBar_Date.Format = eTextFormat.Normal;
            Label_UITopBar_Date.Opacity = 178;
            Label_UITopBar_Date.Font = new Font(Font.Arial, 16);
            Label_UITopBar_Date.TextAlignment = Alignment.TopRight;
            UIPanel.addControl(Label_UITopBar_Date);

            // Animated infobar 
            OMAnimatedLabel2 label_UITopBar_Info = new OMAnimatedLabel2("label_UITopBar_Info", Image_UITopBar_OMIcon.Region.Right+5, Image_UITopBar_Background.Region.Top, Label_UITopBar_Clock.Region.Left - Image_UITopBar_OMIcon.Region.Right, Image_UITopBar_Background.Region.Height);
            label_UITopBar_Info.ActivationType = OMAnimatedLabel2.AnimationActivationTypes.TextToLong;
            label_UITopBar_Info.Animation = OMAnimatedLabel2.eAnimation.ScrollSmooth_LR;
            label_UITopBar_Info.Font = new Font(Font.Arial, 22);
            label_UITopBar_Info.TextAlignment = Alignment.CenterLeft;
            label_UITopBar_Info.Format = eTextFormat.Normal;
            label_UITopBar_Info.Opacity = 178;
            label_UITopBar_Info.Text = "";
            UIPanel.addControl(label_UITopBar_Info);

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

            theHost.UIHandler.OnHideBars += new UIHandler.ShowHideBarsControlDelegate(UIHandler_OnHideBars);
            theHost.UIHandler.OnShowBars += new UIHandler.ShowHideBarsControlDelegate(UIHandler_OnShowBars);

            theHost.UIHandler.OnHidePopUpMenu += new UIHandler.ShowHideControlDelegate(UIHandler_OnHidePopUpMenu);
            theHost.UIHandler.OnShowPopUpMenu += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowPopUpMenu);
            theHost.UIHandler.OnPopupMenuChanged += new UIHandler.PopupMenuEventHandler(UIHandler_OnPopupMenuChanged);

            theHost.UIHandler.OnHideControlButtons += new UIHandler.ShowHideControlDelegate(UIHandler_OnHideControlButtons);
            theHost.UIHandler.OnShowControlButtons += new UIHandler.ShowHideControlDelegate(UIHandler_OnShowControlButtons);
            theHost.UIHandler.OnControlButtonsChanged += new UIHandler.PopupMenuEventHandler(UIHandler_OnControlButtonsChanged);

            theHost.UIHandler.OnHideInfoBanner += new UIHandler.HideInfoBannerDelegate(UIHandler_OnHideInfoBanner);
            theHost.UIHandler.OnShowInfoBanner += new UIHandler.ShowInfoBannerDelegate(UIHandler_OnShowInfoBanner);

            theHost.UIHandler.OnHideInfoBar += new UIHandler.HideInfoBarDelegate(UIHandler_OnHideInfoBar);
            theHost.UIHandler.OnShowInfoBar += new UIHandler.ShowInfoBarDelegate(UIHandler_OnShowInfoBar);

            theHost.UIHandler.OnHideMediaBanner += new UIHandler.HideMediaBannerDelegate(UIHandler_OnHideMediaBanner);
            theHost.UIHandler.OnShowMediaBanner += new UIHandler.ShowMediaBannerDelegate(UIHandler_OnShowMediaBanner);
            theHost.UIHandler.OnEnableMediaBanner += new UIHandler.EnableMediaBannerDelegate(UIHandler_OnEnableMediaBanner);

            UIPanel.Priority = ePriority.UI;
            UIPanel.UIPanel = true;

            UIPanel.Entering += new PanelEvent(UIPanel_Entering);

            #region Brightness adjustment graphics

            OMBasicShape shpBrightnessAdjustment = new OMBasicShape("shpBrightnessAdjustment", OM.Host.ClientFullArea.Left, OM.Host.ClientFullArea.Top, OM.Host.ClientFullArea.Width, OM.Host.ClientFullArea.Height,
                new ShapeData(shapes.Rectangle, Color.Black));
            shpBrightnessAdjustment.Opacity = (OM.Host.CurrentLocation_Daytime ? 0 : 255 - _OpacityLevel);
            shpBrightnessAdjustment.NoUserInteraction = true;
            if (_OpacityMode != OpacityModes.All)
                shpBrightnessAdjustment.Visible = false;
            UIPanel.addControl(shpBrightnessAdjustment);

            #endregion

            manager.loadPanel(UIPanel, true);

            #region Notification drop down panel

            panelNotifyDropDown = new OMPanel("UI_NotifyDropDown");

            OMImage Image_NotifyDropdown_Background = new OMImage("Image_NotifyDropdown_Background", 0, 0, 1000, 134);//new imageItem(Color.FromArgb(26, 30, 40), 1000, 80));
            Image_NotifyDropdown_Background.BackgroundColor = Color.Black;
            Image_NotifyDropdown_Background.Opacity = 230;
            panelNotifyDropDown.addControl(Image_NotifyDropdown_Background);

            OMImage Image_NotifyDropdown_Separator = new OMImage("Image_NotifyDropdown_Separator", 0, Image_NotifyDropdown_Background.Region.Top + 80 + 50, 1000, 2, new imageItem(Color.FromArgb(50, Color.White), 1000, 2));//new imageItem(Color.Black, 1000, 1));
            panelNotifyDropDown.addControl(Image_NotifyDropdown_Separator);

            //OMLabel label_NotifyDropdown_InfoLine = new OMLabel("label_NotifyDropdown_InfoLine", 0, Image_NotifyDropdown_Background.Region.Top, Image_NotifyDropdown_Background.Region.Width, 20);
            //label_NotifyDropdown_InfoLine.FontSize = 12;
            //label_NotifyDropdown_InfoLine.Text = "Infoline";
            //label_NotifyDropdown_InfoLine.TextAlignment = Alignment.TopLeft;
            //label_NotifyDropdown_InfoLine.Color = Color.FromArgb(178, Color.White);
            //panelNotifyDropDown.addControl(label_NotifyDropdown_InfoLine);

            #region Dropdown buttons

            // Menu buttons container
            ButtonStripContainer ButtonStrip_NotifyDropdown = new ButtonStripContainer("Container_NotifyDropdown_ButtonStrip",
                0,
                Image_NotifyDropdown_Background.Region.Top + 50,
                Image_NotifyDropdown_Background.Region.Width,
                80);
            ButtonStrip_NotifyDropdown.Alignment = ButtonStripContainer.Alignments.Left;
            ButtonStrip_NotifyDropdown.ButtonSize = new Size(100, ButtonStrip_NotifyDropdown.SuggestedButtonSize.Height);
            panelNotifyDropDown.addControl(ButtonStrip_NotifyDropdown.Container);
            
            // Set as main container
            theHost.UIHandler.DropDown_ButtonStripContainer = ButtonStrip_NotifyDropdown;

            // Create notify dropdown main buttons
            ButtonStrip btnStrip_NotifyMain = new ButtonStrip(this.pluginName, panelNotifyDropDown.Name, "NotifyMain");
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn0", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Home"), "Home", DropDownButton_Home_OnClick, DropDownButton_Home_OnHoldClick, DropDownButton_Home_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn1", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-MediaZone2"), "Zone", DropDownButton_Zone_OnClick, null, null));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn2", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-OM"), "About", DropDownButton_About_OnClick, DropDownButton_About_OnHoldClick, DropDownButton_About_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn3", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Settings"), "Settings", DropDownButton_Settings_OnClick, DropDownButton_Settings_OnHoldClick, DropDownButton_Settings_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn4", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Power"), "Power", DropDownButton_Power_OnClick, DropDownButton_Power_OnHoldClick, DropDownButton_Power_OnLongClick));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_ButtonDummy_Style1("Btn5", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_ButtonDummy_Style1("Btn6", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_ButtonDummy_Style1("Btn7", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_ButtonDummy_Style1("Btn8", ButtonStrip_NotifyDropdown.ButtonSize));
            btnStrip_NotifyMain.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn9", ButtonStrip_NotifyDropdown.ButtonSize, 178, theHost.getSkinImage("Icons|Icon-Clear"), "Clear", DropDownButton_Clear_OnClick, DropDownButton_Clear_OnHoldClick, DropDownButton_Clear_OnLongClick));
            theHost.UIHandler.DropDown_ButtonStripContainer.SetButtonStrip(btnStrip_NotifyMain);

            // Set main drop down button strip
            theHost.UIHandler.DropDown_MainButtonStrip = btnStrip_NotifyMain;

            // Create notify dropdown power buttons
            btnStrip_NotifyPower = new ButtonStrip(this.pluginName, panelNotifyDropDown.Name, "NotifyPower");
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn0", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|1-navigation-cancel"), "Quit", PowerOptionsStrip_Quit_OnClick, PowerOptionsStrip_Quit_OnHoldClick, PowerOptionsStrip_Quit_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn1", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-pause-over-video"), "Sleep", PowerOptionsStrip_Sleep_OnClick, PowerOptionsStrip_Sleep_OnHoldClick, PowerOptionsStrip_Sleep_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn2", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-pause-over-video"), "Hibernate", PowerOptionsStrip_Hibernate_OnClick, PowerOptionsStrip_Hibernate_OnHoldClick, PowerOptionsStrip_Hibernate_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn3", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Power"), "Shutdown", PowerOptionsStrip_ShutDown_OnClick, PowerOptionsStrip_ShutDown_OnHoldClick, PowerOptionsStrip_ShutDown_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn4", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|9-av-replay"), "Restart", PowerOptionsStrip_Restart_OnClick, PowerOptionsStrip_Restart_OnHoldClick, PowerOptionsStrip_Restart_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn5", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-RestartOM"), "Reload", PowerOptionsStrip_Reload_OnClick, PowerOptionsStrip_Reload_OnHoldClick, PowerOptionsStrip_Reload_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn6", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Monitor-off"), "Screen off", null, null, null));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn7", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|1-navigation-expand"), "Minimize", PowerOptionsStrip_Minimize_OnClick, PowerOptionsStrip_Minimize_OnHoldClick, PowerOptionsStrip_Minimize_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn8", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("AIcons|1-navigation-collapse"), "Fullscreen", PowerOptionsStrip_FullScreen_OnClick, PowerOptionsStrip_FullScreen_OnHoldClick, PowerOptionsStrip_FullScreen_OnLongClick));
            btnStrip_NotifyPower.Buttons.Add(Button.PreConfigLayout_Button_Style1("Btn9", ButtonStrip_NotifyDropdown.ButtonSize, 178, BuiltInComponents.Host.getSkinImage("Icons|Icon-Clear"), "Cancel", PowerOptionsStrip_Cancel_OnClick, PowerOptionsStrip_Cancel_OnHoldClick, PowerOptionsStrip_Cancel_OnLongClick));

            #endregion            

            // Drop down notification list
            OMContainer NotificationList = theHost.UIHandler.NotificationListControl;
            NotificationList.Name = "Container_NotifyDropdown_NotificationList";
            NotificationList.BackgroundColor = Color.Black;
            NotificationList.Opacity = 230;
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
            background.BackgroundType = backgroundStyle.None;
            background.BackgroundColor1 = Color.Black;
            background.Priority = ePriority.Low;
            background.UIPanel = true;
            background.Forgotten = true;

            // Screen background image
            OMImage backgroundImage = new OMImage("backgroundImage", OM.Host.ClientFullArea.Left, OM.Host.ClientFullArea.Top, OM.Host.ClientFullArea.Width, OM.Host.ClientFullArea.Height);
            backgroundImage.Image = OM.Host.getImageFromFile(StoredData.Get(this, "Background.Image"));//OM.Host.getSkinImage("Backgrounds|Highway 1");
            
            // Set opacity based on day/night setting
            if (_OpacityMode == OpacityModes.BackgroundOnly)
                backgroundImage.Opacity = (OM.Host.CurrentLocation_Daytime ? 255 : _OpacityLevel);
            background.addControl(backgroundImage);             

            manager.loadPanel(background);

            #endregion

            theHost.OnSystemEvent += theHost_OnSystemEvent;
            theHost.OnGesture += new GestureEvent(theHost_OnGesture);

            // Set startup client area
            Rectangle ClientArea = new Rectangle();
            ClientArea.Left = 0;
            ClientArea.Top = Shape_UITopBar_Separator.Region.Bottom;
            ClientArea.Right = 1000;
            ClientArea.Bottom = Image_UIBottomBar_Background.Region.Top; //Image_UIBottomBar_Separator.Region.Top;
            BuiltInComponents.Host.SetClientArea(ClientArea);

            CreateAndLoadPanel_MediaBar();

            // Connect to zone events
            theHost.ZoneHandler.OnZoneUpdated += new Zones.ZoneHandler.ZoneUpdatedDelegate(ZoneHandler_OnZoneUpdated);


            return eLoadStatus.LoadSuccessful;
        }

        private void CreateAndLoadPanel_MediaBar()
        {
            // Create a new panel
            OMPanel panel = new OMPanel("MediaBar");

            // Set transition effects
            panel.TransitionEffect_Show = eGlobalTransition.SlideUp;
            panel.TransitionEffect_Hide = eGlobalTransition.SlideDown;

            // Coverflow background
            int coverFlowHeight = 200;
            OMBasicShape shape_UIBottomBar_MediaImages_Background = new OMBasicShape("shape_Background", 0, OM.Host.ClientArea_Init.Bottom - coverFlowHeight, 1000, coverFlowHeight,
                new ShapeData(shapes.Rectangle, Color.Black));
            shape_UIBottomBar_MediaImages_Background.Transparency = 10;
            //shape_UIBottomBar_MediaImages_Background.NoUserInteraction = true;
            //shape_UIBottomBar_MediaImages_Background.Visible = false;
            panel.addControl(shape_UIBottomBar_MediaImages_Background);

            // Separator
            OMBasicShape Shape_UIBottomBar_MediaImages_Background_Separator = new OMBasicShape("Shape_Background_Separator", shape_UIBottomBar_MediaImages_Background.Region.Left, shape_UIBottomBar_MediaImages_Background.Region.Top, shape_UIBottomBar_MediaImages_Background.Region.Width, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0f, 0, Color.Black),
                    new GradientData.ColorPoint(0.5f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5f, 0, Color.Black))
            });
            //Shape_UIBottomBar_MediaImages_Background_Separator.Visible = false;
            panel.addControl(Shape_UIBottomBar_MediaImages_Background_Separator);

            // Create coverflow popup when changing media
            OMMediaFlow mediaFlow_UIBottomBar_MediaImages_CoverFlow = new OMMediaFlow("mediaFlow_CoverFlow", shape_UIBottomBar_MediaImages_Background.Region.Left, shape_UIBottomBar_MediaImages_Background.Region.Top, shape_UIBottomBar_MediaImages_Background.Region.Width, shape_UIBottomBar_MediaImages_Background.Region.Height);
            OMImageFlow.PreConfigLayout_Flat_ApplyToControl(mediaFlow_UIBottomBar_MediaImages_CoverFlow);
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.MediaInfoFormatString = "{1} - {0} - {6}";
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.FontSize = 20;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.Color = Color.FromArgb(178, Color.White);
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.TextAlignment = Alignment.TopCenter;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.Animation_FadeOutDistance = 6;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.ReflectionsEnabled = true;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.NoUserInteraction = true;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.Control_PlacementOffsetY = 5;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.ImageSize = new Size(mediaFlow_UIBottomBar_MediaImages_CoverFlow.Height * 0.6f, mediaFlow_UIBottomBar_MediaImages_CoverFlow.Height * 0.6f);
            //mediaFlow_UIBottomBar_MediaImages_CoverFlow.Visible = false;
            mediaFlow_UIBottomBar_MediaImages_CoverFlow.MediaListSource = OMMediaFlow.ListSources.Buffer;
            panel.addControl(mediaFlow_UIBottomBar_MediaImages_CoverFlow);

            //// Mediasource icon
            //OMImage image_UIBottomBar_MediaImages_Icon_MediaSource = new OMImage("image_Icon_MediaSource",
            //    mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Left,
            //    mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Top + 10, 40, 40);
            //image_UIBottomBar_MediaImages_Icon_MediaSource.DataSource = "Screen{:S:}.Zone.MediaSource.Icon";
            //image_UIBottomBar_MediaImages_Icon_MediaSource.Opacity = 178;
            ////image_UIBottomBar_MediaImages_Icon_MediaSource.Visible = false;
            //panel.addControl(image_UIBottomBar_MediaImages_Icon_MediaSource);

            //OMLabel label_Icon_MediaSource_Text = new OMLabel("label_Icon_MediaSource_Text",
            //    image_UIBottomBar_MediaImages_Icon_MediaSource.Region.Right-7,
            //    image_UIBottomBar_MediaImages_Icon_MediaSource.Region.Top,
            //    100,
            //    image_UIBottomBar_MediaImages_Icon_MediaSource.Region.Height,
            //    "{Screen{:S:}.Zone.MediaSource.Name}");
            //label_Icon_MediaSource_Text.TextAlignment = Alignment.CenterLeft;
            //label_Icon_MediaSource_Text.FontSize = 16;
            //label_Icon_MediaSource_Text.Color = Color.White;
            //label_Icon_MediaSource_Text.Opacity = 178;
            //panel.addControl(label_Icon_MediaSource_Text);

            //// Suffle icon
            //OMImage image_UIBottomBar_MediaImages_Icon_Suffle = new OMImage("image_Icon_Suffle",
            //    mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Right - 40,
            //    mediaFlow_UIBottomBar_MediaImages_CoverFlow.Region.Top + 10, 40, 40, OM.Host.getSkinImage("AIcons|9-av-shuffle").Copy());
            ////image_UIBottomBar_MediaImages_Icon_Suffle.Image.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor, 17);
            //image_UIBottomBar_MediaImages_Icon_Suffle.DataSource = "Screen{:S:}.Zone.Shuffle";
            //image_UIBottomBar_MediaImages_Icon_Suffle.DataSourceControlsVisibility = true;
            //image_UIBottomBar_MediaImages_Icon_Suffle.Opacity = 178;
            ////image_UIBottomBar_MediaImages_Icon_Suffle.Visible = false;
            //panel.addControl(image_UIBottomBar_MediaImages_Icon_Suffle);

            //// Repeat icon
            //OMImage image_UIBottomBar_MediaImages_Icon_Repeat = new OMImage("image_Icon_Repeat",
            //    image_UIBottomBar_MediaImages_Icon_Suffle.Region.Left,
            //    image_UIBottomBar_MediaImages_Icon_Suffle.Region.Bottom, 40, 40, OM.Host.getSkinImage("AIcons|9-av-repeat").Copy());
            ////image_UIBottomBar_MediaImages_Icon_Repeat.Image.image.Glow(BuiltInComponents.SystemSettings.SkinFocusColor, 17);
            //image_UIBottomBar_MediaImages_Icon_Repeat.DataSource = "Screen{:S:}.Zone.Repeat";
            //image_UIBottomBar_MediaImages_Icon_Repeat.DataSourceControlsVisibility = true;
            //image_UIBottomBar_MediaImages_Icon_Repeat.Opacity = 178;
            ////image_UIBottomBar_MediaImages_Icon_Repeat.Visible = false;
            //panel.addControl(image_UIBottomBar_MediaImages_Icon_Repeat);

            panel.Forgotten = true;
            panel.Priority = ePriority.UI;

            // Load the panel into the local manager for panels
            manager.loadPanel(panel);

            // Subscribe to playlist updates for the current zones
            OM.Host.ForEachScreen((screen) =>
            {
                OM.Host.DataHandler.SubscribeToDataSource(screen, "Zone.Playlist", (dataSource) =>
                {
                    OMMediaFlow mediaFlow = manager[dataSource.Screen, "MediaBar"]["mediaFlow_CoverFlow"] as OMMediaFlow;
                    if (mediaFlow != null)
                    {
                        if (dataSource.Value is Playlist)
                            mediaFlow.ListSource = dataSource.Value as Playlist;
                    }
                });
            });
        
        }

        private void BottomBar_MediaInfo_Show(int screen, bool fast)
        {
            // skip if media banner is not enabled
            if (!OM.Host.UIHandler.MediaBanner_IsEnabled(screen))
                return;

            OMPanel panel = manager[screen, "UI"];
            ControlLayout ZoneInfo = new ControlLayout(panel, "_UIBottomBar_MediaInfo_");
            ZoneInfo.Visible = true;
            if (!fast)
                SmoothAnimator.PresetAnimation_FadeIn(ZoneInfo, screen, 2f);
        }
        private void BottomBar_MediaInfo_Hide(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout ZoneInfo = new ControlLayout(panel, "_UIBottomBar_MediaInfo_");
            if (!fast)
                SmoothAnimator.PresetAnimation_FadeOut(ZoneInfo, screen, 2f);
            ZoneInfo.Visible = false;
        }

        private void BottomBar_MediaInfo_ButtonOverlay_Show(int screen)
        {
            OMPanel panel = manager[screen, "UI"];
            OMButton btn = panel[screen, "Button_UIBottomBar_ShowMediaInfo"] as OMButton;
            if (btn != null)
                btn.Visible = true;
        }
        private void BottomBar_MediaInfo_ButtonOverlay_Hide(int screen)
        {
            OMPanel panel = manager[screen, "UI"];
            OMButton btn = panel[screen, "Button_UIBottomBar_ShowMediaInfo"] as OMButton;
            if (btn != null)
                btn.Visible = false;
        }



        void Button_UIBottomBar_MediaInfo_OnClick(OMControl sender, int screen)
        {
            theHost.UIHandler.ControlButtons_Show(screen, false, 5);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
        }

        void ZoneHandler_OnZoneUpdated(Zone zone, int screen)
        {
            ZoneInfo_ShowHide(screen, false, zone);
        }

        #region Bottombar ZoneInfo


        private int ZoneInfo_ShowDelay;
        private SmoothAnimator.AnimatorControl AnimationControl = new SmoothAnimator.AnimatorControl();
        private void ZoneInfo_ShowHide(int screen, bool fast, Zone zone)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout ZoneInfo = new ControlLayout(panel, "_UIBottomBar_ZoneInfo_");

            // Initial data states
            ZoneInfo_ShowDelay = 3000;
            AnimationControl.Cancel = true;

            if (!_ScreenSpecificData.GetProperty<bool>(screen, "ZoneInfoVisible", false))
            {
                _ScreenSpecificData.SetProperty(screen, "ZoneInfoVisible", true);
                // Spawn a new thread for this animation so we don't block the events thread
                OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                {
                    OMAnimatedLabel2 InfoLabel = panel["label_UITopBar_Info"] as OMAnimatedLabel2;
                    if (InfoLabel != null)
                        InfoLabel.TransitionInText(OMAnimatedLabel2.eAnimation.SlideDownSmooth, OMAnimatedLabel2.eAnimation.SlideUpSmooth, String.Format("Active zone: {0}",zone.Name), 2.0f, 3000);

                    // Hide button bar
                    bool Visible = false;
                    OMContainer ButtonStrip = panel["Container_UIBottomBar_ButtonStrip"] as OMContainer;
                    if (ButtonStrip != null)
                    {
                        Visible = ButtonStrip.Visible;
                        BuiltInComponents.Host.UIHandler.ControlButtons_Hide(screen, true);
                    }

                    BottomBar_MediaInfo_Hide(screen, true);

                    ZoneInfo.Visible = true;
                    while (AnimationControl.Cancel)
                    {
                        AnimationControl.Cancel = false;
                        SmoothAnimator.PresetAnimation_FadeIn(ZoneInfo, screen, 0.9f, null);

                        // delay to allow user to see info
                        while (ZoneInfo_ShowDelay > 0)
                        {
                            ZoneInfo_ShowDelay -= 1000;
                            Thread.Sleep(1000);
                        }

                        SmoothAnimator.PresetAnimation_FadeOut(ZoneInfo, screen, 0.7f, AnimationControl);
                    }

                    // Restore buttonbar
                    if (Visible)
                    {
                        BuiltInComponents.Host.UIHandler.ControlButtons_Show(screen, true);
                    }
                    else
                    {
                        BottomBar_MediaInfo_Show(screen, true);
                    }

                    _ScreenSpecificData.SetProperty(screen, "ZoneInfoVisible", false);
                });
                
            }
            else
            {   // Already visible, cancel animation going out
                AnimationControl.Cancel = true;
            }
        }

        #endregion

        #region Infobanner

        void UIHandler_OnShowInfoBanner(int screen, InfoBanner bannerData)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout InfoBanner = new ControlLayout(panel, "_InfoBanner_");

            // Get controls
            OMBasicShape shp = InfoBanner["Shape_InfoBanner_Background"] as OMBasicShape;
            OMLabel lblBack = InfoBanner["Label_InfoBanner_Background"] as OMLabel;
            OMLabel lblText = InfoBanner["Label_InfoBanner_Text"] as OMLabel;

            // Scale size of graphics to match lines
            string[] lines = bannerData.Text.Split('\n');
            int calculatedSize = 100;
            if (lines.Length > 1)
            {   // Dynamic size
                calculatedSize = (int)((100 * 0.75f) * lines.Length);
                if (shp.Region.Height < calculatedSize)
                {
                    int diff = calculatedSize - shp.Region.Height;
                    shp.Height += diff;
                    lblBack.Height += diff;
                    lblText.Height += diff;
                }
            }
            else
            {   // Default size
                shp.Height = 100;
                lblBack.Height = 100;
                lblText.Height = 100;
            }

            if (!InfoBanner.Visible)
            {
                // Spawn a new thread to make this animation async from other code
                lock (bannerData)
                {
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                        {
                            lblText.Text = lblBack.Text = bannerData.Text;
                            lblBack.FontSize = 45F;
                            lblBack.Color = Color.FromArgb(120, lblBack.Color);
                            lblText.Color = Color.FromArgb(120, lblBack.Color);

                            shp.Opacity = lblBack.Opacity = lblText.Opacity = 0;

                            lblText.Visible = true;
                            lblBack.Visible = true;
                            shp.Visible = true;


                            // Animation where text starts centered and fades outwards
                            int AnimationState = 0;
                            int Delay = bannerData.Timeout;
                            int TotalDisplayTimeMS = 0;
                            SmoothAnimator Animation = new SmoothAnimator(0.2f);
                            Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                            {
                                TotalDisplayTimeMS += AnimationStep;
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
                                            if (lblText.Opacity < 255)
                                            {
                                                lblText.Opacity += AnimationStep * 10;
                                                shp.Opacity = lblBack.Opacity = lblText.Opacity;
                                                ContinueAnimation = true;
                                            }
                                            else
                                            {
                                                shp.Opacity = lblBack.Opacity = lblText.Opacity = 255;
                                            }

                                            // Text effect opacity level
                                            int Opacity = lblBack.Color.A - AnimationStep;
                                            if (Opacity > 0)
                                            {
                                                lblBack.Color = Color.FromArgb(Opacity, lblBack.Color);
                                                ContinueAnimation = true;
                                            }
                                            else
                                            {
                                                lblBack.Color = Color.FromArgb(0, lblBack.Color);
                                            }

                                            // Text effect font size
                                            if (lblBack.FontSize < 200)
                                            {
                                                lblBack.FontSize += AnimationStep;
                                                ContinueAnimation = true;
                                            }
                                            else
                                            {
                                                // No final value
                                            }

                                            // Message label opacity
                                            Opacity = lblText.Color.A + (int)(AnimationStep * 8F);
                                            if (Opacity < 255)
                                            {
                                                lblText.Color = Color.FromArgb(Opacity, lblBack.Color);
                                                ContinueAnimation = true;
                                            }
                                            else
                                            {
                                                lblText.Color = Color.FromArgb(255, lblBack.Color);
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

                                            if (AnimationDurationMS < Delay || Delay == 0)
                                                ContinueAnimation = true;

                                            if (lblText.Tag != null)
                                            {
                                                Delay += bannerData.Timeout;
                                                lblText.Tag = null;
                                            }

                                            #endregion

                                            // Maximum duration
                                            if (AnimationDurationMS > 30000)
                                                ContinueAnimation = false;
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
                                            if (lblText.Opacity > 0)
                                            {
                                                lblText.Opacity -= AnimationStep * 8;
                                                shp.Opacity = lblBack.Opacity = lblText.Opacity;
                                                ContinueAnimation = true;
                                            }
                                            else
                                            {
                                                shp.Opacity = lblBack.Opacity = lblText.Opacity = 0;
                                            }

                                            #endregion
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                // Cancel animation if controls are no longer visible
                                if (!lblText.Visible)
                                    return false;

                                return ContinueAnimation;
                            });
                            InfoBanner.Visible = false;
                        });
                }
            }
            else
            {   // Direct update, animation already in progress
                lblText.Tag = true;
                lblBack.Text = bannerData.Text;
                lblText.Text = lblBack.Text;
            }            
        }

        void UIHandler_OnHideInfoBanner(int screen)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout InfoBanner = new ControlLayout(panel, "_InfoBanner_");
            InfoBanner.Visible = false;
        }

        #endregion 

        #region Infobar (topbar info field)

        void UIHandler_OnShowInfoBar(int screen, InfoBar barData)
        {
            OMPanel panel = manager[screen, "UI"];
            OMAnimatedLabel2 InfoLabel = panel[screen, "label_UITopBar_Info"] as OMAnimatedLabel2;
            OMImage img = panel[screen, "Image_UITopBar_OMIcon"] as OMImage;
            if (img != null)
            {
                if (barData.Icon == null || barData.Icon.image == null)
                    img.Image = theHost.getSkinImage("Icons|Icon-OM");
                else
                {
                    ControlLayout controls = new ControlLayout(panel, "Image_UITopBar_OMIcon");
                    SmoothAnimator.PresetAnimation_FadeOut(controls, screen, 2);
                    img.Image = barData.Icon;
                    SmoothAnimator.PresetAnimation_FadeIn(screen, 2, null, 178, true, controls);
                }
            }
            if (InfoLabel != null)
            {
                if (barData.Timeout > 0)
                    InfoLabel.TransitionInText(OMAnimatedLabel2.eAnimation.SlideDownSmooth, OMAnimatedLabel2.eAnimation.SlideUpSmooth, barData.Text, 2.0f, barData.Timeout);
                else
                    InfoLabel.TransitionInText(OMAnimatedLabel2.eAnimation.SlideDownSmooth, barData.Text, 2.0f);
            }
        }

        void UIHandler_OnHideInfoBar(int screen)
        {
            OMPanel panel = manager[screen, "UI"];
            OMAnimatedLabel2 InfoLabel = panel[screen, "label_UITopBar_Info"] as OMAnimatedLabel2;
            OMImage img = panel[screen, "Image_UITopBar_OMIcon"] as OMImage;
            if (img != null)
            {
                if (img.Image.image != theHost.getSkinImage("Icons|Icon-OM").image)
                {
                    ControlLayout controls = new ControlLayout(panel, "Image_UITopBar_OMIcon");
                    SmoothAnimator.PresetAnimation_FadeOut(controls, screen, 2);
                    img.Image = theHost.getSkinImage("Icons|Icon-OM");
                    SmoothAnimator.PresetAnimation_FadeIn(screen, 2, null, 178, true, controls);
                }
            }
            if (InfoLabel != null)
                InfoLabel.TransitionInText(OMAnimatedLabel2.eAnimation.SlideUpSmooth, String.Empty, 2.0f);
        }

        #endregion 

        #region MediaBanner

        private void UIHandler_OnShowMediaBanner(int screen, bool fast)
        {
            // Check for valid data before showing media banner
            Playlist playlist = OM.Host.DataHandler.GetDataSourceValue<Playlist>(screen, "Zone.Playlist");
            if (playlist == null || !playlist.HasItems)
                return;

            OM.Host.execute(eFunction.ShowPanel, screen, String.Format("{0};{1}", this.pluginName, "MediaBar"));//, eGlobalTransition.SlideUp);
        }
        private void UIHandler_OnHideMediaBanner(int screen, bool fast)
        {
            OM.Host.execute(eFunction.HidePanel, screen, String.Format("{0};{1}", this.pluginName, "MediaBar"));//, eGlobalTransition.SlideDown);
        }

        void UIHandler_OnEnableMediaBanner(int screen, bool fast)
        {

        }

        #endregion

        #region Control Buttons

        void UIHandler_OnControlButtonsChanged(object sender, int screen, bool popupAvailable)
        {
            // No action
        }

        void UIHandler_OnShowControlButtons(int screen, bool fast)
        {
            BottomBar_MediaInfo_Hide(screen, fast);
            BottomBar_MediaInfo_ButtonOverlay_Hide(screen);
            OMPanel panel = manager[screen, "UI"];
            OMContainer container = panel["Container_UIBottomBar_ButtonStrip"] as OMContainer;
            if (container != null)
            {
                if (fast)
                {
                    container.Visible = true;
                    container.Opacity = 255;
                }
                else
                {
                    //ControlLayout CoverFlowGroup = new ControlLayout(panel, "_UIBottomBar_MediaImages_");
                    //CoverFlowGroup.Visible_Force(true);
                    ControlLayout ButtonStripGroup = new ControlLayout(panel, "Container_UIBottomBar_ButtonStrip");
                    ButtonStripGroup.Visible = true;
                    SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, ButtonStripGroup);

                    //// Animate multiple groups
                    //SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, ButtonStripGroup, CoverFlowGroup);

                }
            }
            //BottomBar_CoverFlow_Show(screen, fast);
            //OM.Host.UIHandler.MediaBanner_Show(screen, fast);
        }

        void UIHandler_OnHideControlButtons(int screen, bool fast)
        {
            // OM.Host.UIHandler.MediaBanner_Hide(screen, fast);

            OMPanel panel = manager[screen, "UI"];
            OMContainer container = panel["Container_UIBottomBar_ButtonStrip"] as OMContainer;
            if (container != null)
            {
                if (fast)
                    container.Visible = false;
                else
                {
                    //ControlLayout CoverFlowGroup = new ControlLayout(panel, "_UIBottomBar_MediaImages_");
                    ControlLayout ButtonStripGroup = new ControlLayout(panel, "Container_UIBottomBar_ButtonStrip");

                    // Animate multiple groups
                    //SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, ButtonStripGroup, CoverFlowGroup);
                    SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, ButtonStripGroup);
                }
            }
            BottomBar_MediaInfo_Show(screen, fast);
            BottomBar_MediaInfo_ButtonOverlay_Show(screen);
        }

        void ControlButton_Pause_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Pause");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }
        void ControlButton_Play_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Play");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }
        void ControlButton_Play_OnHoldClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Pause");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }

        void ControlButton_Stop_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Stop");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }

        void ControlButton_SeekForward_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.SeekForward");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }
        void ControlButton_SeekForward_OnHoldClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            while (sender.Mode == eModeType.Clicked)
            {
                OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.SeekForward");
                OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
                OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
                System.Threading.Thread.Sleep(100);
            }
        }

        void ControlButton_SeekBackward_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.SeekBackward");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }
        void ControlButton_SeekBackward_OnHoldClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            while (sender.Mode == eModeType.Clicked)
            {
                OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.SeekBackward");
                OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
                OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
                System.Threading.Thread.Sleep(100);
            }
        }

        void ControlButton_Previous_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Previous");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4);
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }
        void ControlButton_Next_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.Next");
            OM.Host.UIHandler.ControlButtons_AutoHideTimer_Reset(screen);
            OM.Host.UIHandler.MediaBanner_Show(screen, false, 4); 
            OM.Host.UIHandler.MediaBanner_AutoHideTimer_Reset(screen);
        }

        private void ControlButtonDataSourceChanged(OpenMobile.Data.DataSource dataSource)
        {
            if (dataSource.Value == null)
                return;

            if (dataSource.NameLevel2 == "Playback" & dataSource.NameLevel3 == "Stopped")
            {
                if ((bool)dataSource.Value == true)
                {
                    OM.Host.UIHandler.ControlButtons.GetButtonStrip(dataSource.Screen).Buttons["Btn3"] =
                        Button.PreConfigLayout_MenuBarStyle("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), ControlButton_Play_OnClick, null, null, false, false);
                }
            }
            else if (dataSource.NameLevel2 == "Playback" & dataSource.NameLevel3 == "Paused") 
            {
                if ((bool)dataSource.Value == true)
                {
                    OM.Host.UIHandler.ControlButtons.GetButtonStrip(dataSource.Screen).Buttons["Btn3"] =
                        Button.PreConfigLayout_MenuBarStyle("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), ControlButton_Play_OnClick, null, null, false, false);
                }
            }
            else if (dataSource.NameLevel2 == "Playback" & dataSource.NameLevel3 == "Playing") 
            {
                if ((bool)dataSource.Value == true)
                {
                    OM.Host.UIHandler.ControlButtons.GetButtonStrip(dataSource.Screen).Buttons["Btn3"] =
                        Button.PreConfigLayout_MenuBarStyle("Btn3", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-pause"), ControlButton_Pause_OnClick, null, null, false, false);
                }
            }
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
                if (show)
                {
                    btnPopUp.FocusImage = imgPopUpMenuButton_Collapse_Focus;
                    btnPopUp.DownImage = imgPopUpMenuButton_Collapse_Down;
                    btnPopUp.OverlayImage = imgPopUpMenuButton_Collapse_Overlay;
                    //btnPopUp.Visible = true;
                }
                else
                {
                    //btnPopUp.Visible = false;
                    ControlLayout PopUpMenu = new ControlLayout(panel, "_PopUp_");
                    PopUpMenu.Visible = false;
                    btnPopUp.FocusImage = imageItem.NONE;
                    btnPopUp.DownImage = imageItem.NONE;
                    btnPopUp.OverlayImage = imageItem.NONE;
                }
            }
        }

        void UIHandler_OnPopupMenuChanged(object sender, int screen, bool popupAvailable)
        {
            PopUpMenuIcon_ShowHide(screen, popupAvailable);
        }

        void Button_UIBottomBar_MenuPopUp_OnClick(OMControl sender, int screen)
        {
            if (!theHost.UIHandler.PopUpMenu.Available(screen))
                return;

            OMPanel panel = manager[screen, "UI"];
            ControlLayout PopUpMenu = new ControlLayout(panel, "_PopUp_");
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;

            if (!PopUpMenu.Visible)
                theHost.UIHandler.PopUpMenu_Show(screen, false);
            else
                theHost.UIHandler.PopUpMenu_Hide(screen, false);
        }

        void UIHandler_OnShowPopUpMenu(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout PopUpMenu = new ControlLayout(panel, "_PopUp_");
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

            // Inform UI handler that popupmenu is about to be shown
            ((IObjectShowing)OM.Host.UIHandler.PopUpMenu).Showing(container, screen);

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
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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

            // Ensure popup menu items are placed correctly
            container.ScrollToControl(container.Controls[0]);
        }

        void UIHandler_OnHidePopUpMenu(int screen, bool fast)
        {
            OMPanel panel = manager[screen, "UI"];
            ControlLayout PopUpMenu = new ControlLayout(panel, "_PopUp_");
            OMButton btnPopUp = panel["Button_UIBottomBar_MenuPopUp"] as OMButton;

            if (!fast)
            {
                // Calculate animation distance
                int AnimationDistance = theHost.ClientArea[screen].Bottom - PopUpMenu.Region.Top;

                // Cancel animation if not needed
                if (AnimationDistance != 0)
                {
                    // Animate
                    SmoothAnimator Animation = new SmoothAnimator(4f * BuiltInComponents.SystemSettings.TransitionSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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
            PopUpMenuIcon_ShowHide(screen, theHost.UIHandler.PopUpMenu.Available(screen));
        }

        #endregion
        
        #region Show / Hide Bars

        void UIHandler_OnShowBars(int screen, bool fast, OpenMobile.UI.UIHandler.Bars executeOnBars)
        {
            theHost.UIHandler.PopUpMenu_Hide(screen, true);

            bool AnimateTopBar = (executeOnBars & OpenMobile.UI.UIHandler.Bars.Top) == OpenMobile.UI.UIHandler.Bars.Top;
            bool AnimateBottomBar = (executeOnBars & OpenMobile.UI.UIHandler.Bars.Bottom) == OpenMobile.UI.UIHandler.Bars.Bottom;

            OMPanel panel = manager[screen, "UI"];
            ControlLayout TopBar = new ControlLayout(panel, "_UITopBar_");
            ControlLayout BottomBar = new ControlLayout(panel, "_UIBottomBar_");

            if (AnimateTopBar)
                TopBar.Visible = true;
            if (AnimateBottomBar)
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
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                    {
                        // Animate top bar
                        if (AnimateTopBar && AnimationDistance_TopBar > 0)
                        {
                            AnimationDistance_TopBar -= AnimationStep;

                            if (AnimationDistance_TopBar < 0)
                                TopBar.Offset(0, AnimationStep + AnimationDistance_TopBar);
                            else
                                TopBar.Offset(0, AnimationStep);
                        }

                        // Animate bottom bar
                        if (AnimateBottomBar && AnimationDistance_BottomBar > 0)
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
                        if ((!AnimateTopBar || (AnimationDistance_TopBar <= 0)) && (!AnimateBottomBar || (AnimationDistance_BottomBar <= 0)))
                            //if (AnimationDistance_TopBar <= 0 && AnimationDistance_BottomBar <= 0)
                            return false;

                        // Continue animation
                        return true;
                    });

                    // Ensure controls is placed correctly
                    int Placement = TopBar.Region.Top;
                    if (AnimateTopBar && Placement != 0)
                    {
                        if (Placement < 0)
                            TopBar.Offset(0, Placement); // To short
                        else
                            TopBar.Offset(0, -Placement); // To long
                    }
                    Placement = BottomBar.Region.Bottom;
                    if (AnimateBottomBar && Placement != 600)
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

        void UIHandler_OnHideBars(int screen, bool fast, OpenMobile.UI.UIHandler.Bars executeOnBars)
        {
            theHost.UIHandler.PopUpMenu_Hide(screen, true);

            bool AnimateTopBar = (executeOnBars & OpenMobile.UI.UIHandler.Bars.Top) == OpenMobile.UI.UIHandler.Bars.Top;
            bool AnimateBottomBar = (executeOnBars & OpenMobile.UI.UIHandler.Bars.Bottom) == OpenMobile.UI.UIHandler.Bars.Bottom;

            OMPanel panel = manager[screen, "UI"];
            ControlLayout TopBar = new ControlLayout(panel, "_UITopBar_");
            ControlLayout BottomBar = new ControlLayout(panel, "_UIBottomBar_");
            if (fast)
            {
                if (AnimateTopBar)
                    TopBar.Visible = false;
                if (AnimateBottomBar)
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
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                    {
                        // Animate top bar
                        if (AnimateTopBar && AnimationDistance_TopBar > 0)
                        {
                            AnimationDistance_TopBar -= AnimationStep;
                            TopBar.Offset(0, -AnimationStep);
                        }

                        // Animate bottom bar
                        if (AnimateBottomBar && AnimationDistance_BottomBar > 0)
                        {
                            AnimationDistance_BottomBar -= AnimationStep;
                            BottomBar.Offset(0, AnimationStep);
                        }

                        // Request a screen redraw
                        panel.Refresh();

                        // Exit animation
                        if ((!AnimateTopBar || (AnimationDistance_TopBar <= 0)) && (!AnimateBottomBar || (AnimationDistance_BottomBar <= 0)))
                            return false;

                        // Continue animation
                        return true;
                    });
                }
                if (AnimateTopBar)
                    TopBar.Visible = false;
                if (AnimateBottomBar)
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
            OM.Host.execute(eFunction.closeProgram);
        }
        void PowerOptionsStrip_Quit_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip Sleep

        void PowerOptionsStrip_Sleep_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Sleep_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.standby);
        }
        void PowerOptionsStrip_Sleep_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_Minimize 

        void PowerOptionsStrip_Minimize_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Minimize_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.minimize, screen);
        }
        void PowerOptionsStrip_Minimize_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_FullScreen

        void PowerOptionsStrip_FullScreen_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_FullScreen_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.ToggleFullscreen, screen);
        }
        void PowerOptionsStrip_FullScreen_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_ShutDown

        void PowerOptionsStrip_ShutDown_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_ShutDown_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.shutdown);
        }
        void PowerOptionsStrip_ShutDown_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_Hibernate

        void PowerOptionsStrip_Hibernate_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Hibernate_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.hibernate);
        }
        void PowerOptionsStrip_Hibernate_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_Restart 

        void PowerOptionsStrip_Restart_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Restart_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.restart);
        }
        void PowerOptionsStrip_Restart_OnLongClick(OMControl sender, int screen)
        {
        }

        #endregion

        #region PowerOptionsStrip_Reload

        void PowerOptionsStrip_Reload_OnClick(OMControl sender, int screen)
        {
        }
        void PowerOptionsStrip_Reload_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.restartProgram);
        }
        void PowerOptionsStrip_Reload_OnLongClick(OMControl sender, int screen)
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

        void Button_UIBottomBar_VolUpDown_OnModeChange(OMButton sender, int screen, eModeType mode)
        {
            OMPanel panel = manager[screen, "UI"];
            OMButton btn = panel["Button_UIBottomBar_VolBackground"] as OMButton;
            if (btn != null)
                btn.Mode = sender.Mode;
        }

        void Button_UIBottomBar_VolUpDown_OnHoldClick(OMControl sender, int screen)
        {
            theHost.CommandHandler.ExecuteCommand(screen, "Zone.Device.Volume.Mute.Toggle");


            //object MuteState = false;
            //theHost.DataHandler.GetDataSourceValue(String.Format("OM;Screen{0}.Zone.Volume.Mute", screen), null, out MuteState);
            //if ((bool)MuteState)
            //    theHost.CommandHandler.ExecuteCommand(String.Format("OM;Screen{0}.Zone.Volume.Unmute", screen));
            //else
            //    theHost.CommandHandler.ExecuteCommand(String.Format("OM;Screen{0}.Zone.Volume.Mute", screen));
        }

        void Button_UIBottomBar_VolDown_OnClick(OMControl sender, int screen)
        {
            //theHost.StatusBarHandler.RemoveAllMyNotifications(this);
            //theHost.StatusBarHandler.RemoveNotification(this, "1");

            //theHost.StatusBarHandler.UpdateNotification(this, "1", "Sample update text");

            //theHost.UIHandler.Bars_Hide(screen, false);
            //Thread.Sleep(2000);
            //theHost.UIHandler.Bars_Show(screen, false);

            //theHost.CommandHandler.ExecuteCommand(String.Format("OM;Screen{0}.Zone.Volume.Decrement", screen));
            theHost.CommandHandler.ExecuteCommand(screen, "Zone.Device.Volume.Decrement");
        }

        void Button_UIBottomBar_VolUp_OnClick(OMControl sender, int screen)
        {
            // Add new notification
            //Notification notification = new Notification(this, "1", theHost.getSkinImage("Icons|Icon-MusicIndexer").image, "Media indexing completed", "17 new tracks found");
            //notification.Global = true;
            //theHost.StatusBarHandler.AddNotification(notification);

            //theHost.StatusBarHandler.AddNotification(screen, new Notification(Notification.Styles.Warning, this, "2", theHost.getSkinImage("Icons|Icon - Online").image, "Internet connection detected", "Internet connection is available for plugins"));

            //theHost.UIHandler.Bars_Show(screen, false);

            //theHost.CommandHandler.ExecuteCommand(String.Format("OM;Screen{0}.Zone.Volume.Increment", screen));
            //theHost.CommandHandler.ExecuteCommand("OM;Screen0.Zone.Volume.Set", new object[1]{50});
            theHost.CommandHandler.ExecuteCommand(screen, "Zone.Device.Volume.Increment");
        }

        void Button_UIBottomBar_Back_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goHome, screen);
        }

        void Button_UIBottomBar_Back_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

/*
        void MediaButtonStrip_Play_OnClick(OMControl sender, int screen)
        {
            // Add a new button
            //theHost.UIHandler.ControlButtons.GetButtonStrip(screen).Buttons.Add(Button.PreConfigLayout_MenuBarStyle("Btn6", theHost.UIHandler.ControlButtons.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-play"), MediaButtonStrip_Play_OnClick, null, null));

            // Update button
            //theHost.StatusBarHandler.BottomBar_ButtonStripContainer.GetButtonStrip(screen).Buttons["Btn3"] = Button.CreateSimpleButton("Btn6", theHost.StatusBarHandler.BottomBar_ButtonStripContainer.ButtonSize, 178, theHost.getSkinImage("AIcons|9-av-pause"), MediaButtonStrip_Play_OnClick, null, null); ;

            //// Toggle buttonstrips
            //if (theHost.StatusBarHandler.BottomBar_ButtonStripContainer.GetButtonStrip(screen) == btnStrip_Media)
            //    theHost.StatusBarHandler.BottomBar_ButtonStripContainer.SetButtonStrip(screen, btnStrip_Media2);
            //else
            //    theHost.StatusBarHandler.BottomBar_ButtonStripContainer.SetButtonStrip(screen, btnStrip_Media);

            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.Play();
        }
        void MediaButtonStrip_Play_OnHoldClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.Stop();
        }
        void MediaButtonStrip_Fwd_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.SeekFwd();
        }
        void MediaButtonStrip_Fwd_OnHoldClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            while (sender.Mode == eModeType.Clicked)
            {
                zone.MediaHandler.SeekFwd();
                Thread.Sleep(100);
            }
        }
        void MediaButtonStrip_Bwd_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
        }
        void MediaButtonStrip_Bwd_OnHoldClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            while (sender.Mode == eModeType.Clicked)
            {
                zone.MediaHandler.SeekBwd();
                Thread.Sleep(100);
            }
        }
        void MediaButtonStrip_Next_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.Next();
        }
        void MediaButtonStrip_Prev_OnClick(OMControl sender, int screen)
        {
            Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
            zone.MediaHandler.Previous();
        }
*/
        #endregion

        #region helper methods

        void ButtonGraphics_SetGlowingFocusImages(OMButton button, OImage baseimage)
        {
            if (baseimage == null)
                return;

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

        #endregion

        private void BackgroundImage_Change(int screen, string imagePath)
        {
            OMPanel panel = manager[screen, "background"];
            OMImage img = panel["backgroundImage"] as OMImage;
            if (img == null)
                return;

            ControlLayout backgroundGroup = new ControlLayout(panel, "backgroundImage");

            // Fade current image out
            SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, backgroundGroup);

            // Change image
            img.Image = OM.Host.getImageFromFile(imagePath);

            // Fade new image in
            SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, backgroundGroup);
        }


        void UIPanel_Entering(OMPanel sender, int screen)
        {   // Update initial data

            //OM.Host.UIHandler.Notifications_Enable(screen);

            //OMProgress progress_UIBottomBar_Test = new OMProgress("progress_UIBottomBar_Test", 295, theHost.UIHandler.ControlButtons.Container.Region.Top + 15, 410, 30);
            

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
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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

/*
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

                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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

                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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
            theHost.execute(eFunction.setSystemVolume, ((VolumeBar)sender).Value, screen);
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
*/
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
                        Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
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
            //tick.Enabled = false;
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

        #region Day/Night mode change over

        private void SetDayNightMode_SetOpacityManually(int screen, int opacity)
        {
            switch (_OpacityMode)
            {
                case OpacityModes.None:
                    return;
                case OpacityModes.All:
                    {
                        OMPanel panel = manager[screen, "UI"];
                        if (panel != null)
                        {
                            OMControl control = panel["shpBrightnessAdjustment"] as OMControl;
                            if (control != null)
                                control.Opacity = 255 - opacity;
                        }
                    }
                    break;
                case OpacityModes.BackgroundOnly:
                    {
                        OMPanel panel = manager[screen, "background"];
                        if (panel != null)
                        {
                            OMControl control = panel["backgroundImage"] as OMControl;
                            if (control != null)
                                control.Opacity = opacity;
                        }
                    }
                    break;
                default:
                    break;
            }

        }
        
        private void SetDayNightMode(bool night = false)
        {
            switch (_OpacityMode)
            {
                case OpacityModes.None:
                    // Ensure we have full brightness if this function is disabled
                    theHost.ForEachScreen(delegate(int screen)
                    {
                        OMPanel panel = manager[screen, "background"];
                        if (panel != null)
                        {
                            ControlLayout backgroundGroup = new ControlLayout(panel, "backgroundImage");
                            backgroundGroup.AddControls("shpBrightnessAdjustment");
                            SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, 255, true, backgroundGroup);
                        }
                        panel = manager[screen, "UI"];
                        if (panel != null)
                        {
                            ControlLayout backgroundGroup = new ControlLayout(panel, "shpBrightnessAdjustment");
                            SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, 0, true, backgroundGroup);
                            backgroundGroup.Visible = false;
                        }
                    });
                    return;
                case OpacityModes.All:
                     {
                        if (!night)
                        {   // Daytime
                            theHost.ForEachScreen(delegate(int screen)
                            {
                                OMPanel panel = manager[screen, "UI"];
                                if (panel != null)
                                {
                                    ControlLayout backgroundGroup = new ControlLayout(panel, "shpBrightnessAdjustment");
                                    backgroundGroup.Visible = true;
                                    SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, 0, false, backgroundGroup);
                                }
                            });
                        }
                        else
                        {   // Nighttime
                            theHost.ForEachScreen(delegate(int screen)
                            {
                                OMPanel panel = manager[screen, "UI"];
                                if (panel != null)
                                {
                                    ControlLayout backgroundGroup = new ControlLayout(panel, "shpBrightnessAdjustment");
                                    backgroundGroup.Visible = true;
                                    SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, 255 - _OpacityLevel, false, backgroundGroup);
                                }
                            });
                        }
                    }
                   break;
                case OpacityModes.BackgroundOnly:
                    {
                        if (!night)
                        {   // Daytime
                            theHost.ForEachScreen(delegate(int screen)
                            {
                                OMPanel panel = manager[screen, "background"];
                                if (panel != null)
                                {
                                    ControlLayout backgroundGroup = new ControlLayout(panel, "backgroundImage");
                                    SmoothAnimator.PresetAnimation_FadeIn(screen, 1.2f, null, 255, false, backgroundGroup);
                                }
                            });
                        }
                        else
                        {   // Nighttime
                            theHost.ForEachScreen(delegate(int screen)
                            {
                                OMPanel panel = manager[screen, "background"];
                                if (panel != null)
                                {
                                    ControlLayout backgroundGroup = new ControlLayout(panel, "backgroundImage");
                                    SmoothAnimator.PresetAnimation_FadeOut(screen, 1.2f, null, _OpacityLevel, false, backgroundGroup);
                                }
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        //void statusReset_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    theHost.ForEachScreen(delegate(int screen)
        //    {
        //        OMLabel lbl = UIPanel[screen, "Label_UITopBar_TrackTitle"] as OMLabel;
        //        if (lbl != null)
        //            lbl.Text = theHost.getPlayingMedia(screen).Name;
        //    });
        //    statusReset.Enabled = false;
        //}

        #region host events

        void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            #region Internet connection

            // Show icon for connected to internet
            if (function == eFunction.connectedToInternet)
            {
                if (notificationInternetOnline == null)
                {   // Create new notification
                    notificationInternetOnline = new Notification(this, "Internet_Online", theHost.getSkinImage("Icons|Icon-Internet").image, theHost.getSkinImage("Icons|Icon-Internet").image, "Internet connection detected", "");
                    notificationInternetOnline.ClearAction += new Notification.NotificationAction(notificationInternetOnline_ClearAction);
                    theHost.UIHandler.AddNotification(notificationInternetOnline);
                }
                else
                {   // Update current notification with original image
                    notificationInternetOnline.IconStatusBar = theHost.getSkinImage("Icons|Icon-Internet").image;
                }
            }

            // Update icon for internet connection
            else if (function == eFunction.disconnectedFromInternet)
            {
                //theHost.UIHandler.RemoveNotification(this, "Internet_Online");
                if (notificationInternetOnline != null)
                {
                    // Turn the current icon red to indicate status
                    notificationInternetOnline.IconStatusBar = notificationInternetOnline.IconStatusBar.Copy().Overlay(Color.Red).Glow(Color.Red, 15);
                }
            }

            #endregion

            // Shutdown program
            if (function == eFunction.CloseProgramPreview)
            {
                OM.Host.ForEachScreen((screen) =>
                    {
                        OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Closing program, please wait...", 0));
                    });
            }

            // Toggle day/night
            if (function == eFunction.CurrentLocationDay)
                SetDayNightMode(false);
            if (function == eFunction.CurrentLocationNight)
                SetDayNightMode(true);

            return;

            //if (function == eFunction.backgroundOperationStatus)
            //{
            //    return;
            //    #region backgroundOperationStatus

            //    int ArgScreen = OpenMobile.helperFunctions.Arguments.GetScreenFromArg(ref arg2);

            //    // Is this a update from the speech engine?
            //    if (arg2 == "Speech")
            //    {
            //        // Yes, check engine status
            //        if (arg1 == "Engine Ready!")
            //        {   // Engine is ready, Show speech button
            //            theHost.ForEachScreen(delegate(int screen)
            //            {
            //                UIPanel[screen, "Button_Speech_Speech"].Visible = true;
            //            });
            //            return;
            //        }
            //        else if (arg1 == "Processing...")
            //        {   // Engine is processing speech
            //            // TODO: Add parameter to speech engine that holds the active screen number
            //            ((OMImage)UIPanel[0, "Image_Speech_Speech"]).Image = theHost.getSkinImage("Processing");
            //            ((OMLabel)UIPanel[0, "Label_Speech_Caption"]).Text = "Processing...";
            //            return;
            //        }
            //    }

            //    // Reset status update timer
            //    statusReset.Enabled = false;
            //    statusReset.Enabled = true;

            //    eDataType DataType = OpenMobile.helperFunctions.Arguments.GetDataTypeFromArg(arg3);
            //    switch (DataType)
            //    {
            //        case eDataType.Update:
            //            {
            //                if (ArgScreen < 0)
            //                {
            //                    theHost.ForEachScreen(delegate(int screen)
            //                    {
            //                        ((OMLabel)UIPanel[screen, "Label_UITopBar_TrackTitle"]).Text = arg1;
            //                    });
            //                }
            //                else
            //                {   // Directly update the given screen
            //                    ((OMLabel)UIPanel[ArgScreen, "Label_UITopBar_TrackTitle"]).Text = arg1;
            //                }
            //            }
            //            break;
            //        case eDataType.PopUp:
            //            {
            //                if (ArgScreen < 0)
            //                {
            //                    theHost.ForEachScreen(delegate(int screen)
            //                    {
            //                        ShowInfoMessage(screen, arg1);
            //                    });
            //                }
            //                else
            //                {   // Directly update the given screen
            //                    ShowInfoMessage(ArgScreen, arg1);
            //                }
            //            }
            //            break;
            //        default:
            //            {
            //                if (ArgScreen < 0)
            //                {   // Global message
            //                    theHost.ForEachScreen(delegate(int screen)
            //                    {
            //                        OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[screen, "Label_UITopBar_TrackTitle"]; //6
            //                        title.Text = arg1;
            //                    });
            //                }
            //                else
            //                {   // Screen specific message
            //                    OMAnimatedLabel2 title = (OMAnimatedLabel2)UIPanel[ArgScreen, "Label_UITopBar_TrackTitle"]; //6
            //                    title.Text = arg1;
            //                }
            //            }
            //            break;
            //    }
            //    #endregion
            //}

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

            //else if (function == eFunction.stopListeningForSpeech)
            //{
            //    #region stopListeningForSpeech

            //    // hide speech controls 
            //    // TODO: Add parameter to speech engine that holds the active screen number
            //    theHost.ForEachScreen(delegate(int screen)
            //    {
            //        SpeechControls(false, screen);
            //    });

            //    #endregion
            //}
               
            //else if (function == eFunction.pluginLoadingComplete)
            //{
            //    #region pluginLoadingComplete

            //    // Set video window position according to what the local skin can use
            //    theHost.ForEachScreen(delegate(int screen)
            //    {
            //        theHost.SetVideoPosition(screen, new Rectangle(0, 100, 1000, 368));
            //    });

            //    #endregion
            //}
        }

        void notificationInternetOnline_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            // Change notification type to static
            notification.State = Notification.States.Active;
            notification.Style = Notification.Styles.IconOnly;

            // Cancel the clear request on this notification
            cancel = true;

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
