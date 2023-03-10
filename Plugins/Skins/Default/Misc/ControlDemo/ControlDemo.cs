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
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.Media;

namespace ControlDemo
{
    [SkinIcon("*#")]
    public sealed class ControlDemo : HighLevelCode
    {
        public ControlDemo()
            : base("ControlDemo", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "Control demo", "Demo", "OM Dev team/Borte", "")
        {
        }

        imageItem imgBtnIconOn;
        imageItem imgBtnIconOff;
        MenuPopup PopupMenu;


        public override eLoadStatus initialize(IPluginHost host)
        {
            #region Disabled code

            //OMLabel lblDataSourceTest1 = new OMLabel("lblDataSourceTest1", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, 250, 50);
            //lblDataSourceTest1.Text = "cpu load is {System.CPU.Load} / mem usage is {System.Memory.UsedPercent}";
            //lblDataSourceTest1.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
            //lblDataSourceTest1.AutoFitTextMode = FitModes.Fit;
            //lblDataSourceTest1.FontSize = 30;
            ////lblDataSourceTest1.SkinDebug = true;
            //p.addControl(lblDataSourceTest1);
            //OMLabel lblDataSourceTest2 = new OMLabel("lblDataSourceTest2", lblDataSourceTest1.Region.Right, lblDataSourceTest1.Region.Top, lblDataSourceTest1.Region.Width, lblDataSourceTest1.Region.Height / 2);
            //lblDataSourceTest2.Text = "cpu load is {System.CPU.Load} / mem usage is {System.Memory.UsedPercent}";
            //lblDataSourceTest2.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
            //lblDataSourceTest2.AutoFitTextMode = FitModes.Fit;
            //lblDataSourceTest2.FontSize = 30;
            ////lblDataSourceTest2.SkinDebug = true;
            //p.addControl(lblDataSourceTest2);
            //OMLabel lblDataSourceTest3 = new OMLabel("lblDataSourceTest3", lblDataSourceTest2.Region.Right, lblDataSourceTest2.Region.Top, lblDataSourceTest1.Region.Width / 2, lblDataSourceTest1.Region.Height);
            //lblDataSourceTest3.Text = "cpu load is {System.CPU.Load} / mem usage is {System.Memory.UsedPercent}";
            //lblDataSourceTest3.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
            //lblDataSourceTest3.AutoFitTextMode = FitModes.Fit;
            //lblDataSourceTest3.FontSize = 30;
            ////lblDataSourceTest3.SkinDebug = true;
            //p.addControl(lblDataSourceTest3);


            /*
            OMAnimatedLabel label1 = new OMAnimatedLabel(50, 100, 200, 30);
            label1.Text = "This is an example of scrolling text how well does it work";
            label1.ContiuousAnimation = eAnimation.BounceScroll;
            OMAnimatedLabel label2 = new OMAnimatedLabel(50, 200, 200, 30);
            label2.Text = label1.Text;
            label2.ContiuousAnimation = eAnimation.Scroll;
            label2.Format = eTextFormat.Glow;
            label2.OutlineColor = Color.Blue;
            OMAnimatedLabel label3 = new OMAnimatedLabel(50, 250, 200, 30);
            label3.Text = "Unveil Me Right";
            label3.ContiuousAnimation = eAnimation.UnveilRight;
            OMAnimatedLabel label4 = new OMAnimatedLabel(50, 300, 200, 30);
            label4.Text = "Unveil Me Left";
            label4.ContiuousAnimation = eAnimation.UnveilLeft;
            OMAnimatedLabel label5 = new OMAnimatedLabel(50, 350, 200, 30);
            label5.Text = "This is the starting text";
            ReflectedImage gauge = new ReflectedImage();
            gauge.Left = 300;
            gauge.Top = 150;
            gauge.Width = 300;
            gauge.Height = 200;
            gauge.Image=theHost.getSkinImage("OMGenius");
            OMButton button = new OMButton(50, 400, 200, 50);
            button.Text = "Toggle Buffer";
            button.Image = imageItem.MISSING;
            button.OnClick += new userInteraction(button_OnClick);
            p.addControl(label5);
            p.addControl(button);
            p.addControl(gauge);

            OMButton btnDialog = new OMButton(450, 450, 200, 50);
            btnDialog.Name = "btnDialog";
            btnDialog.Text = "Dialog";
            btnDialog.Image = imageItem.MISSING;
            btnDialog.OnClick += new userInteraction(btnDialog_OnClick);
            p.addControl(btnDialog);

            OMLabel lblTtest = new OMLabel(300, 200, 200, 50);
            lblTtest.Name = "Test";
            lblTtest.Text = "Test";
            lblTtest.SkinDebug = true;
            p.addControl(lblTtest);

            OMButton OK = new OMButton(200, 200, 200, 110);
            OK.Image = theHost.getSkinImage("Full");
            OK.FocusImage = theHost.getSkinImage("Full.Highlighted");
            OK.Text = "OK";
            OK.Name = "OK";
            OK.Transition = eButtonTransition.None;
            OK.SkinDebug = true;
            OK.OnClick += new userInteraction(OK_OnClick);
            p.addControl(OK);

            OMImage AnimatedImage = new OMImage();
            AnimatedImage.Name = "Ani";
            AnimatedImage.Left = 600;
            AnimatedImage.Top = 150;
            AnimatedImage.Image = theHost.getSkinImage("OM");
            AnimatedImage.Width = AnimatedImage.Image.image.Width;
            AnimatedImage.Height = AnimatedImage.Image.image.Height;
            AnimatedImage.Animate = AnimatedImage.CanAnimate;   // Activate animation if possible
            p.addControl(AnimatedImage);

            OMList List1 = new OMList(550,200,200,200);
            List1.Name = "List1";
            List1.ListStyle = eListStyle.MultiList;
            List1.Add(new OMListItem("gggg"));
            List1.Add(new OMListItem("gg"));
            List1.Add(new OMListItem("bbbb"));
            List1.Add(new OMListItem("cccc"));
            List1.Add(new OMListItem("cc"));
            List1.Add(new OMListItem("dddd"));
            List1.Add(new OMListItem("ffff"));
            List1.Add(new OMListItem("ff"));

            OMListItem ListItem = new OMListItem("Line1", "linemulti");
            ListItem.image = theHost.getSkinImage("OM").image;
            List1.Add(ListItem);

            List1.SelectedIndexChanged += new OMList.IndexChangedDelegate(List1_SelectedIndexChanged);
            p.addControl(List1);
            OMList List2 = new OMList(750, 200, 200, 200);
            List2.Name = "List2";
            p.addControl(List2);
            */

            /*
            OMImage TestImg = new OMImage("TestImg", 50, 200);
            TestImg.SkinDebug = true;
            TestImg.FitControlToImage = true;
            TestImg.Image = new imageItem(@"C:\Borte\Programming\c#\Projects\OpenMobile\SVN\SVN Rev 661\trunk\FrontEnd\OpenMobile\bin\Debug\Skins\Default\ArtistIcon.png");
            //TestImg.Image = new imageItem(OImage.FromFile(@"C:\Borte\Programming\c#\Projects\OpenMobile\SVN\SVN Rev 661\trunk\FrontEnd\OpenMobile\bin\Debug\Skins\Default\ArtistIcon.png"));
            p.addControl(TestImg);
            */

            // Generate custom image
            /*
            imageItem img1 = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(300, 90, OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackground));
            imageItem img2 = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(300, 90, OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused));
            imgBtnIconOn = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(300, 90, ButtonGraphic.ImageTypes.ButtonForegroundFocused, "¯", "Music"));
            imgBtnIconOff = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(300, 90, ButtonGraphic.ImageTypes.ButtonForeground, "¯", "Music"));
            */
            /*
            OMButton OK = new OMButton("A", 325, 200, 300, 120);
            OK.Image = theHost.getSkinImage("Full");
            OK.Text = "A";
            p.addControl(OK);

            OMButton OK2 = new OMButton("A", 325, 350, 300, 120);
            OK2.Image = theHost.getSkinImage("menuButton");
            OK2.FocusImage = theHost.getSkinImage("menuButtonHighlighted");
            OK2.Text = "A";
            p.addControl(OK2);
            */

            /*
                    OMButton tstButton = new OMButton("A", 650, 365, 300, 90);
                    tstButton.Image = img1;
                    tstButton.FocusImage = img2;
                    tstButton.OverlayImage = imgBtnIconOff;
                    tstButton.Transition = eButtonTransition.None;
                    tstButton.Font = new OpenMobile.Graphics.Font(OpenMobile.Graphics.Font.Arial, 36);
                    //tstButton.Format = eTextFormat.GlowBig;
                    tstButton.OutlineColor = OpenMobile.Graphics.Color.Red;
                    //tstButton.Text = "Music";
                    tstButton.OnClick += new userInteraction(tstButton_OnClick);
                    p.addControl(tstButton);
            */

            /*
            #region Menu popup

            // Popup menu
            PopupMenu = new MenuPopup("ZoneMenu", MenuPopup.ReturnTypes.Index);

            // Popup menu items
            OMListItem ListItem = new OMListItem("New", "mnuItemNewZone" as object);
            ListItem.image = OImage.FromFont(50, 50, "+", new Font(Font.Arial, 40F), eTextFormat.Outline, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem);

            OMListItem ListItem2 = new OMListItem("Edit", "mnuItemEditZone" as object);
            ListItem2.image = OImage.FromWebdingsFont(50, 50, "@", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem2);

            OMListItem ListItem3 = new OMListItem("Remove", "mnuItemRemoveZone" as object);
            ListItem3.image = OImage.FromWebdingsFont(50, 50, "r", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem3);

            OMListItem ListItem4 = new OMListItem("Default", "mnuItemSetDefaultZone" as object);
            ListItem4.image = OImage.FromWebdingsFont(50, 50, "a", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem4);

            OMListItem ListItem5 = new OMListItem("Set active", "mnuItemSetActive" as object);
            ListItem5.image = OImage.FromWebdingsFont(50, 50, "a", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem5);

            OMListItem ListItem6 = new OMListItem("Restore defaults", "mnuItemRestoreDefaults" as object);
            ListItem6.image = OImage.FromWebdingsFont(50, 50, "Ó", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem6);

            #endregion

            // OSK Buttons
            OMButton OSKButton = OMButton.PreConfigLayout_BasicStyle("OSKButton", 650, 100, 300, 90, GraphicCorners.Top, ">", "Keypad");
            OSKButton.Tag = OSKInputTypes.Keypad;
            OSKButton.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton);
            OMButton OSKButton2 = OMButton.PreConfigLayout_BasicStyle("OSKButton2", OSKButton.Region.Left, OSKButton.Region.Bottom - 1, 300, 90, GraphicCorners.None, ">", "Numpad");
            OSKButton2.Tag = OSKInputTypes.Numpad;
            OSKButton2.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton2);
            OMButton OSKButton3 = OMButton.PreConfigLayout_BasicStyle("OSKButton3", OSKButton.Region.Left, OSKButton2.Region.Bottom - 1, 300, 90, GraphicCorners.Bottom, ">", "Password");
            //OMButton OSKButton3 = DefaultControls.GetButton("OSKButton2", 650, 365, 300, 90, ">", "Password");
            OSKButton3.OnClick += new userInteraction(OSKButton3_OnClick);
            p.addControl(OSKButton3);

            OMButton btn3D = OMButton.PreConfigLayout_BasicStyle("btn3D", OSKButton.Region.Left, OSKButton3.Region.Bottom + 10, 300, 90, GraphicCorners.All, "", "3D");
            btn3D.OnClick += new userInteraction(btn3D_OnClick);
            p.addControl(btn3D);

            // Add default button
            OMButton btnDialog = OMButton.PreConfigLayout_BasicStyle("btnDialog", 10, 100, 300, 90, GraphicCorners.All, "", "DialogTest");
            btnDialog.OnClick += new userInteraction(btnDialog_OnClick);
            p.addControl(btnDialog);

            // Add default button
            OMButton btnPopupMenu = OMButton.PreConfigLayout_BasicStyle("btnPopupMenu", 10, 200, 300, 90, GraphicCorners.All, "", "PopupMenu");
            btnPopupMenu.OnClick += new userInteraction(btnPopupMenu_OnClick);
            p.addControl(btnPopupMenu);


            OMBasicShape shp = new OMBasicShape("shp", 0, 350, 400, 200,
                new ShapeData(shapes.Rectangle, Color.FromArgb(0xFF, 25, 25, 25)));
            p.addControl(shp);

            OMImage TestImg = new OMImage("TestImg", 10, 400);
            TestImg.FitControlToImage = true;
            TestImg.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(180, 107, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            p.addControl(TestImg);

            OMImage TestImg2 = new OMImage("TestImg2", 210, 400);
            TestImg2.FitControlToImage = true;
            TestImg2.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(100, 60, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            p.addControl(TestImg2);

            // OMList2 test
            OMButton btnOMList2Test = OMButton.PreConfigLayout_BasicStyle("btnOMList2Test", 320, OM.Host.ClientArea_Init.Top+10, 300, 90, GraphicCorners.Top, "", "OMList2");
            btnOMList2Test.Tag = "OMList2Test";
            btnOMList2Test.OnClick += new userInteraction(btnCommonTransitionToPanel_OnClick);
            p.addControl(btnOMList2Test);
            // Load OMList2 panel
            panelOMList2Test.Initialize(this.pluginName, manager, theHost);

            // Playlist test
            OMButton btnPlayList = OMButton.PreConfigLayout_BasicStyle("btnPlayList", btnOMList2Test.Region.Left, btnOMList2Test.Region.Bottom-1, 300, 90, GraphicCorners.None, "", "PlayList");
            btnPlayList.OnClick += new userInteraction(btnPlayList_OnClick);
            p.addControl(btnPlayList);

            // Images test
            OMButton btnImages = OMButton.PreConfigLayout_BasicStyle("btnImages", btnOMList2Test.Region.Left, btnPlayList.Region.Bottom-1, 300, 90, GraphicCorners.None, "", "Images");
            btnImages.OnClick += new userInteraction(btnImages_OnClick);
            p.addControl(btnImages);

            // SlideIn test
            OMButton btnSlideInTest = OMButton.PreConfigLayout_BasicStyle("btnSlideInTest", btnOMList2Test.Region.Left, btnImages.Region.Bottom-1, 300, 90, GraphicCorners.None, "", "SlideIn");
            btnSlideInTest.OnClick += new userInteraction(btnSlideInTest_OnClick);
            p.addControl(btnSlideInTest);

            // Datasources
            OMButton btnDataSources = OMButton.PreConfigLayout_BasicStyle("btnDataSources", btnOMList2Test.Region.Left, btnSlideInTest.Region.Bottom-1, 300, 90, GraphicCorners.Bottom, "", "DataSources");
            btnDataSources.Command_Click = "Screen{:S:}.Panel.Goto.ControlDemo.DataSources";
            p.addControl(btnDataSources);
            */

            /*
            OMImage TestImg = new OMImage("TestImg", 10, 100);
            TestImg.FitControlToImage = true;
            TestImg.Image = img1;
            p.addControl(TestImg);
            OMImage TestImg2 = new OMImage("TestImg2", 10, 200);
            TestImg2.FitControlToImage = true;
            TestImg2.Image = img2;
            p.addControl(TestImg2);
            OMImage TestImg3 = new OMImage("TestImg3", 10, 300);
            TestImg3.FitControlToImage = true;
            TestImg3.Image = imgBtnIconOff;
            p.addControl(TestImg3);
            OMImage TestImg4 = new OMImage("TestImg4", 10, 400);
            TestImg4.FitControlToImage = true;
            TestImg4.Image = imgBtnIconOn;
            p.addControl(TestImg4);
            */

            //OMTestControl tstControl = new OMTestControl();
            //tstControl.Left = 200;
            //tstControl.Top = OM.Host.ClientArea_Init.Top + 0;
            //tstControl.Width = 400;
            //tstControl.Height = 400; // 35;
            //p.addControl(tstControl);

            #endregion

            // Queue any loading of panels as these panels are likely to not being used so we don't have to load them into memory

            // Load main panel (default)
            base.PanelManager.QueuePanel("ControlDemo", Initialize, true);

            // Load images panel
            base.PanelManager.QueuePanel("Images", panelImages.Initialize);

            // Load 3D panel
            base.PanelManager.QueuePanel("panel3D", panel3D.Initialize);

            // Load SlideIn panel
            base.PanelManager.QueuePanel("SlideInTest", panelSlideInTest.Initialize);

            // Load DataSources panel
            base.PanelManager.QueuePanel("DataSources", panelDataSources.Initialize);

            // Load Playlist panel
            base.PanelManager.QueuePanel("PlaylistTest", new panelPlayListTest(this).Initialize);

            return eLoadStatus.LoadSuccessful;
        }

        private OMPanel Initialize()
        {
            OMPanel p = new OMPanel("ControlDemo");

            #region Menu popup

            // Popup menu
            PopupMenu = new MenuPopup("ZoneMenu", MenuPopup.ReturnTypes.Index);

            // Popup menu items
            OMListItem ListItem = new OMListItem("New", "mnuItemNewZone" as object);
            ListItem.image = OImage.FromFont(50, 50, "+", new Font(Font.Arial, 40F), eTextFormat.Outline, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem);

            OMListItem ListItem2 = new OMListItem("Edit", "mnuItemEditZone" as object);
            ListItem2.image = OImage.FromWebdingsFont(50, 50, "@", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem2);

            OMListItem ListItem3 = new OMListItem("Remove", "mnuItemRemoveZone" as object);
            ListItem3.image = OImage.FromWebdingsFont(50, 50, "r", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem3);

            OMListItem ListItem4 = new OMListItem("Default", "mnuItemSetDefaultZone" as object);
            ListItem4.image = OImage.FromWebdingsFont(50, 50, "a", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem4);

            OMListItem ListItem5 = new OMListItem("Set active", "mnuItemSetActive" as object);
            ListItem5.image = OImage.FromWebdingsFont(50, 50, "a", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem5);

            OMListItem ListItem6 = new OMListItem("Restore defaults", "mnuItemRestoreDefaults" as object);
            ListItem6.image = OImage.FromWebdingsFont(50, 50, "Ó", BuiltInComponents.SystemSettings.SkinTextColor);
            PopupMenu.AddMenuItem(ListItem6);

            #endregion

            // OSK Buttons
            OMButton OSKButton = OMButton.PreConfigLayout_BasicStyle("OSKButton", 650, OM.Host.ClientArea_Init.Top + 10, 300, 90, GraphicCorners.Top, ">", "Keypad");
            OSKButton.Tag = OSKInputTypes.Keypad;
            OSKButton.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton);
            OMButton OSKButton2 = OMButton.PreConfigLayout_BasicStyle("OSKButton2", OSKButton.Region.Left, OSKButton.Region.Bottom - 1, 300, 90, GraphicCorners.None, ">", "Numpad");
            OSKButton2.Tag = OSKInputTypes.Numpad;
            OSKButton2.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton2);
            OMButton OSKButton3 = OMButton.PreConfigLayout_BasicStyle("OSKButton3", OSKButton.Region.Left, OSKButton2.Region.Bottom - 1, 300, 90, GraphicCorners.Bottom, ">", "Password");
            //OMButton OSKButton3 = DefaultControls.GetButton("OSKButton2", 650, 365, 300, 90, ">", "Password");
            OSKButton3.OnClick += new userInteraction(OSKButton3_OnClick);
            p.addControl(OSKButton3);

            OMButton btn3D = OMButton.PreConfigLayout_BasicStyle("btn3D", OSKButton.Region.Left, OSKButton3.Region.Bottom + 10, 300, 90, GraphicCorners.All, "", "3D");
            btn3D.OnClick += new userInteraction(btn3D_OnClick);
            p.addControl(btn3D);

            OMButton btnTest = OMButton.PreConfigLayout_BasicStyle("btnTest", OSKButton.Region.Left, btn3D.Region.Bottom + 10, 300, 90, GraphicCorners.All, "", "Test");
            btnTest.OnClick += new userInteraction(btnTest_OnClick);
            p.addControl(btnTest);

            // Add default button
            OMButton btnDialog = OMButton.PreConfigLayout_BasicStyle("btnDialog", 10, 100, 300, 90, GraphicCorners.All, "", "DialogTest");
            btnDialog.OnClick += new userInteraction(btnDialog_OnClick);
            p.addControl(btnDialog);

            // Add default button
            OMButton btnPopupMenu = OMButton.PreConfigLayout_BasicStyle("btnPopupMenu", 10, 200, 300, 90, GraphicCorners.All, "", "PopupMenu");
            btnPopupMenu.OnClick += new userInteraction(btnPopupMenu_OnClick);
            p.addControl(btnPopupMenu);


            //OMBasicShape shp = new OMBasicShape("shp", 0, 350, 400, 200,
            //    new ShapeData(shapes.Rectangle, Color.FromArgb(0xFF, 25, 25, 25)));
            //p.addControl(shp);

            //OMImage TestImg = new OMImage("TestImg", 10, 400);
            //TestImg.FitControlToImage = true;
            //TestImg.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(180, 107, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            //p.addControl(TestImg);

            //OMImage TestImg2 = new OMImage("TestImg2", 210, 400);
            //TestImg2.FitControlToImage = true;
            //TestImg2.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(100, 60, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            //p.addControl(TestImg2);

            // OMList2 test
            OMButton btnOMList2Test = OMButton.PreConfigLayout_CleanStyle("btnOMList2Test", 320, OM.Host.ClientArea_Init.Top + 10, 300, 90, corners:GraphicCorners.Top, icon:"", text:"OMList2");
            btnOMList2Test.Tag = "OMList2Test";
            btnOMList2Test.OnClick += new userInteraction(btnCommonTransitionToPanel_OnClick);
            p.addControl(btnOMList2Test);
            // Load OMList2 panel
            panelOMList2Test.Initialize(this.pluginName, base.PanelManager, OM.Host);

            // Playlist test
            OMButton btnPlaylist = OMButton.PreConfigLayout_CleanStyle("btnPlaylist", btnOMList2Test.Region.Left, btnOMList2Test.Region.Bottom - 1, 300, 90, corners: GraphicCorners.None, icon: "", text: "Playlist");
            btnPlaylist.OnClick += new userInteraction(btnPlaylist_OnClick);
            p.addControl(btnPlaylist);

            // Images test
            OMButton btnImages = OMButton.PreConfigLayout_CleanStyle("btnImages", btnOMList2Test.Region.Left, btnPlaylist.Region.Bottom - 1, 300, 90, corners: GraphicCorners.None, icon: "", text: "Images");
            btnImages.OnClick += new userInteraction(btnImages_OnClick);
            p.addControl(btnImages);

            // SlideIn test
            OMButton btnSlideInTest = OMButton.PreConfigLayout_CleanStyle("btnSlideInTest", btnOMList2Test.Region.Left, btnImages.Region.Bottom - 1, 300, 90, corners: GraphicCorners.None, icon: "", text: "SlideIn");
            btnSlideInTest.OnClick += new userInteraction(btnSlideInTest_OnClick);
            p.addControl(btnSlideInTest);

            // Datasources
            OMButton btnDataSources = OMButton.PreConfigLayout_CleanStyle("btnDataSources", btnOMList2Test.Region.Left, btnSlideInTest.Region.Bottom - 1, 300, 90, corners: GraphicCorners.Bottom, icon: "", text: "DataSources");
            btnDataSources.Command_Click = "Screen{:S:}.Panel.Goto.ControlDemo.DataSources";
            p.addControl(btnDataSources);

            p.Entering += new PanelEvent(p_Entering);
            p.Leaving += new PanelEvent(p_Leaving);

            return p;
        }

        void btnTest_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["Item1"] =
                Button.CreateMenuItem("Item1", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Aicons|3-rating-good"), "Changed item", false, null, null, null);
        }

        void btn3D_OnClick(OMControl sender, int screen)
        {
            OM.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            OM.Host.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "panel3D");
            OM.Host.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void btnCommonTransitionToPanel_OnClick(OMControl sender, int screen)
        {
            // Get panel name from button tag
            string panelName = sender.Tag as string;
            if (String.IsNullOrEmpty(panelName))
                return;

            // Transition to panel
            OM.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            OM.Host.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, panelName);
            OM.Host.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void btnSlideInTest_OnClick(OMControl sender, int screen)
        {
            OM.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            OM.Host.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "SlideInTest");
            OM.Host.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void btnImages_OnClick(OMControl sender, int screen)
        {
            OM.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            OM.Host.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "Images");
            OM.Host.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void btnPlaylist_OnClick(OMControl sender, int screen)
        {
            base.GotoPanel(screen, "PlaylistTest");
        }

        void OSKButton3_OnClick(OMControl sender, int screen)
        {
            //OSK osk = new OSK("", "password", "Please input password now", OSKInputTypes.Keypad, true);
            //string Result = osk.ShowOSK(screen);
            string Result = OSK.ShowDefaultOSK(screen, "", "password", "Please input password now", OSKInputTypes.Keypad, true);
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("OSK result: {0}", Result)));
        }

        void btnPopupMenu_OnClick(OMControl sender, int screen)
        {
            int Selection = (int)PopupMenu.ShowMenu(screen);
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("PopupMenu selection: {0}", Selection)));
        }

        bool btnSelected = false;
        void OSKButton_OnClick(OMControl sender, int screen)
        {
            //OSK osk = new OSK("", "Type something", "Please input something now", (OSKInputTypes)sender.Tag, false);
            //string Result = osk.ShowOSK(screen);
            string Result = OSK.ShowDefaultOSK(screen, "", "Type something", "Please input something now", (OSKInputTypes)sender.Tag, false);
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("OSK result: {0}", Result)));
        }

        void List1_SelectedIndexChanged(OMList sender, int screen)
        {
            OMList List1 = (OMList)base.PanelManager[screen]["List1"];
            OMList List2 = (OMList)base.PanelManager[screen]["List2"];
            string find = List1.SelectedItem.text.Substring(0, 1);
            List2.Items = List1.Items.FindAll(a => a.text.StartsWith(find)).ConvertAll(x => (OpenMobile.OMListItem)x.Clone());
        }

        void OK_OnClick(OMControl sender, int screen)
        {
            OMButton btn = (OMButton)base.PanelManager[screen]["btnDialog"];
            btn.Image = OM.Host.getSkinImage("Full");
        }


        void p_Leaving(OMPanel sender, int screen)
        {
            //theHost.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("Leaving panel: {0}", sender.Name)));
            OM.Host.UIHandler.RemoveAllMyNotifications(this);
        }

        void p_Entering(OMPanel sender, int screen)
        {
            //theHost.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("Entering panel: {0}", sender.Name)));

            // Create a buttonstrip
            ButtonStrip PopUpMenuStrip = new ButtonStrip(sender.OwnerPlugin.pluginName, sender.Name, "PopUpMenuStrip");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Item1", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Aicons|5-content-email"), "Send email", false, null, null, null));
            // Add some more buttons to the buttonstrip
            for (int i = 2; i < 9; i++)
            {
                PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Item" + i.ToString(),
                    OM.Host.UIHandler.PopUpMenu.ButtonSize,
                    255,
                    OM.Host.getSkinImage("Aicons|5-content-email"),
                    "Send email" + i.ToString(),
                    false,
                    null, null, null));
            }

            // Load the buttonstrip
            OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip);

            // Demo temperature notification
            OImage tempIcon = new OImage(Color.Transparent, 85, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height);
            Font f = Font.Arial;
            f.Size = 24;
            tempIcon.RenderText(0, 0, tempIcon.Width, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height, Globalization.convertToLocalTemp(10.0, true), f, eTextFormat.Normal, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinFocusColor, FitModes.FitFillSingleLine);

            //Notification notification = new Notification(Notification.Styles.IconOnly, this, "ControlDemo_Notification", DateTime.Now, null, tempIcon, "Notification test", "");
            //notification.State = Notification.States.Active;
            //notification.IconSize_Width = tempIcon.Width;
            //OM.Host.UIHandler.AddNotification(screen, notification);

            //OImage icon1 = OM.Host.getSkinImage("Icons|Icon-Weather_Conditions1").image;
            //Notification notification1 = new Notification(this, "ControlDemo_Notification1", icon1, icon1, "Notification 1", "");
            //notification1.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification1);

            //OImage icon2 = icon1.Copy().Overlay(Color.Yellow);
            //Notification notification2 = new Notification(this, "ControlDemo_Notification2", icon2, icon2, "Notification 2", "");
            //notification2.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification2);

            //OImage icon3 = icon1.Copy().Overlay(Color.Red);
            //Notification notification3 = new Notification(this, "ControlDemo_Notification3", icon3, icon3, "Notification 3", "");
            //notification3.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification3);

            //OImage icon4 = OM.Host.getSkinImage("Icons|Icon-Weather_Conditions2").image;
            //Notification notification4 = new Notification(this, "ControlDemo_Notification4", icon4, icon4, "Notification 4", "");
            //notification4.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification4);

            //OImage icon5 = icon4.Copy().Overlay(Color.Yellow);
            //Notification notification5 = new Notification(this, "ControlDemo_Notification5", icon5, icon5, "Notification 5", "");
            //notification5.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification5);

            //OImage icon6 = icon4.Copy().Overlay(Color.Red);
            //Notification notification6 = new Notification(this, "ControlDemo_Notification6", icon6, icon6, "Notification 6", "");
            //notification6.State = Notification.States.Active;
            //OM.Host.UIHandler.AddNotification(screen, notification6);

        }

        void btnDialog_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this.pluginName, sender.Parent.Name);
            dialog.Header = "Radio message";
            dialog.Text = "Is this awesome?";
            dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Busy;
            dialog.Button = OpenMobile.helperFunctions.Forms.buttons.Yes |
                                OpenMobile.helperFunctions.Forms.buttons.No;

            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("MsgBox result: {0}", dialog.ShowMsgBox(screen))));          
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            //((OMGauge)manager[0][2]).Value = OpenMobile.Framework.Math.Calculation.RandomNumber(0, 20);
        }

        void button_OnClick(OMControl sender, int screen)
        {
            OMGauge g = ((OMGauge)base.PanelManager[screen][2]);
            if (g.BufferSize == 5)
                g.BufferSize = 0;
            else
                g.BufferSize = 5;
        }
    }
}
