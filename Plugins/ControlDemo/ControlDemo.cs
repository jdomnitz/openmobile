﻿/*********************************************************************************
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
using OpenMobile.Graphics.OpenGL;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.Media;

namespace ControlDemo
{
    [SkinIcon("*#")]
    public sealed class ControlDemo : IHighLevel
    {

        #region IHighLevel Members
        ScreenManager manager;
        IPluginHost theHost;

        imageItem imgBtnIconOn;
        imageItem imgBtnIconOff;
        MenuPopup PopupMenu;

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel("ControlDemo");
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);

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
            OMButton OSKButton = DefaultControls.GetButton("OSKButton", 650, 165, 300, 90, ">", "Keypad");
            OSKButton.Tag = OSKInputTypes.Keypad;
            OSKButton.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton);
            OMButton OSKButton2 = DefaultControls.GetButton("OSKButton2", 650, 265, 300, 90, ">", "Numpad");
            OSKButton2.Tag = OSKInputTypes.Numpad;
            OSKButton2.OnClick += new userInteraction(OSKButton_OnClick);
            p.addControl(OSKButton2);
            OMButton OSKButton3 = DefaultControls.GetButton("OSKButton2", 650, 365, 300, 90, ">", "Password");
            OSKButton3.OnClick += new userInteraction(OSKButton3_OnClick);
            p.addControl(OSKButton3);

            // Add default button
            OMButton btnDialog = DefaultControls.GetButton("btnDialog", 10, 100, 300, 90, "", "DialogTest");
            btnDialog.OnClick += new userInteraction(btnDialog_OnClick);
            p.addControl(btnDialog);

            // Add default button
            OMButton btnPopupMenu = DefaultControls.GetButton("btnPopupMenu", 10, 200, 300, 90, "", "PopupMenu");
            btnPopupMenu.OnClick += new userInteraction(btnPopupMenu_OnClick);
            p.addControl(btnPopupMenu);


            OMBasicShape shp = new OMBasicShape("shp", 0, 350, 400, 200);
            shp.Shape = shapes.Rectangle;
            shp.FillColor = Color.FromArgb(0xFF, 25, 25, 25);
            shp.BorderSize = 0;
            p.addControl(shp);

            OMImage TestImg = new OMImage("TestImg", 10, 400);
            TestImg.FitControlToImage = true;
            TestImg.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(180, 107, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            p.addControl(TestImg);

            OMImage TestImg2 = new OMImage("TestImg2", 210, 400);
            TestImg2.FitControlToImage = true;
            TestImg2.Image = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(100, 60, ButtonGraphic.ImageTypes.ButtonBackground, ButtonGraphic.GraphicStyles.Style1));
            p.addControl(TestImg2);

            // Playlist test
            OMButton btnPlayList = DefaultControls.GetButton("btnPlayList", 320, 200, 300, 90, "", "PlayList");
            btnPlayList.OnClick += new userInteraction(btnPlayList_OnClick);
            p.addControl(btnPlayList);

            // Images test
            OMButton btnImages = DefaultControls.GetButton("btnImages", 320, 300, 300, 90, "", "Images");
            btnImages.OnClick += new userInteraction(btnImages_OnClick);
            p.addControl(btnImages);

            // SlideIn test
            OMButton btnSlideInTest = DefaultControls.GetButton("btnSlideInTest", 320, 400, 300, 90, "", "SlideIn");
            btnSlideInTest.OnClick += new userInteraction(btnSlideInTest_OnClick);
            p.addControl(btnSlideInTest);

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


            // Load playlist panel
            panelPlayListTest.Initialize(this.pluginName, manager, theHost);

            // Load images panel
            panelImages.Initialize(this.pluginName, manager, theHost);

            // Load SlideIn panel
            panelSlideInTest.Initialize(this.pluginName, manager, theHost);

            System.Timers.Timer t = new System.Timers.Timer(100);
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Enabled = true;
            p.Entering += new PanelEvent(p_Entering);
            p.Leaving += new PanelEvent(p_Leaving);
            manager.loadPanel(p);
            manager.DefaultPanel = p.Name;

            return eLoadStatus.LoadSuccessful;
        }

        void btnSlideInTest_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "SlideInTest");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void btnImages_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "Images");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        PlayList playlist = null;

        void btnPlayList_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), this.pluginName);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "PlayListTest");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void OSKButton3_OnClick(OMControl sender, int screen)
        {
            //OSK osk = new OSK("", "password", "Please input password now", OSKInputTypes.Keypad, true);
            //string Result = osk.ShowOSK(screen);
            string Result = OSK.ShowDefaultOSK(screen, "", "password", "Please input password now", OSKInputTypes.Keypad, true);
            theHost.SendStatusData(screen, eDataType.PopUp, this, String.Format("OSK result: {0}", Result));
        }

        void btnPopupMenu_OnClick(OMControl sender, int screen)
        {
            int Selection = (int)PopupMenu.ShowMenu(screen);
            theHost.SendStatusData(screen, eDataType.PopUp, this, String.Format("PopupMenu selection: {0}", Selection));
        }

        bool btnSelected = false;
        void OSKButton_OnClick(OMControl sender, int screen)
        {
            //OSK osk = new OSK("", "Type something", "Please input something now", (OSKInputTypes)sender.Tag, false);
            //string Result = osk.ShowOSK(screen);
            string Result = OSK.ShowDefaultOSK(screen, "", "Type something", "Please input something now", (OSKInputTypes)sender.Tag, false);
            theHost.SendStatusData(screen, eDataType.PopUp, this, String.Format("OSK result: {0}", Result));
        }

        void List1_SelectedIndexChanged(OMList sender, int screen)
        {
            OMList List1 = (OMList)manager[screen]["List1"];
            OMList List2 = (OMList)manager[screen]["List2"];
            string find = List1.SelectedItem.text.Substring(0, 1);
            List2.Items = List1.Items.FindAll(a => a.text.StartsWith(find)).ConvertAll(x => (OpenMobile.OMListItem)x.Clone());
        }

        void OK_OnClick(OMControl sender, int screen)
        {
            OMButton btn = (OMButton)manager[screen]["btnDialog"];
            btn.Image = theHost.getSkinImage("Full");
        }


        void p_Leaving(OMPanel sender, int screen)
        {
            theHost.SendStatusData(screen, eDataType.Info, this, String.Format("Leaving panel: {0}", sender.Name));
        }

        void p_Entering(OMPanel sender, int screen)
        {
            theHost.SendStatusData(screen, eDataType.Info, this, String.Format("Entering panel: {0}", sender.Name));
        }

        int frame;
        void btnDialog_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this.pluginName, sender.Parent.Name);
            dialog.Header = "Radio message";
            dialog.Text = "Is this awesome?";
            dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Busy;
            dialog.Button = OpenMobile.helperFunctions.Forms.buttons.Yes |
                                OpenMobile.helperFunctions.Forms.buttons.No;

            theHost.SendStatusData(screen, eDataType.PopUp, this, String.Format("MsgBox result: {0}", dialog.ShowMsgBox(screen)));

        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            //((OMGauge)manager[0][2]).Value = OpenMobile.Framework.Math.Calculation.RandomNumber(0, 20);
        }

        void button_OnClick(OMControl sender, int screen)
        {
            OMGauge g = ((OMGauge)manager[screen][2]);
            if (g.BufferSize == 5)
                g.BufferSize = 0;
            else
                g.BufferSize = 5;
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen, name];
        }
        public Settings loadSettings()
        {
            return null;
        }
        #endregion
        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "admin@domnitzsolutions.com"; }
        }

        public string pluginName
        {
            get { return "ControlDemo"; }
        }
        public string displayName
        {
            get { return "Control Demo"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Demonstrate inter-panel transitions and the larger control set"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
