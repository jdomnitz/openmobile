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
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Media;


namespace ControlDemo
{
    public static class panelSlideInTest
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static OMPanel pSlideInTest = null;
        static imageItem imgPanel_Background_Highlighted;
        static imageItem imgPanel_Background;
        static GestureEvent Gesture = new GestureEvent(Host_OnGesture);


        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            pSlideInTest = new OMPanel("SlideInTest");

            #region Bottom menu buttons

            // Calculate where to place the buttons based on amount of buttons needed
            const int MenuButtonCount = 0;
            const int MenuButtonWidth = 160;
            int MenuButtonStartLocation = 500 - ((MenuButtonWidth * MenuButtonCount) / 2);

            OMButton[] Button_BottomBar = new OMButton[MenuButtonCount];
            for (int i = 0; i < Button_BottomBar.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "5", "");
                        Button_BottomBar[i].OnClick += new userInteraction(menuButton_OnClick);
                        break;
                    default:
                        break;
                }

                pSlideInTest.addControl(Button_BottomBar[i]);
            }

            #endregion

            // Add default button
            OMButton btnMoveContainer = DefaultControls.GetButton("btnMoveContainer", 0, 100, 180, 90, "", "Move");
            btnMoveContainer.OnClick += new userInteraction(btnMoveContainer_OnClick);
            pSlideInTest.addControl(btnMoveContainer);

            OMButton btnChangeText = DefaultControls.GetButton("btnChangeText", 180, 100, 180, 90, "", "Change Text");
            btnChangeText.OnClick += new userInteraction(btnChangeText_OnClick);
            pSlideInTest.addControl(btnChangeText);

            OMButton btnScrollToControl = DefaultControls.GetButton("btnScrollToControl", 360, 100, 180, 90, "", "Scroll");
            btnScrollToControl.OnClick += new userInteraction(btnScrollToControl_OnClick);
            btnScrollToControl.Tag = 0;
            pSlideInTest.addControl(btnScrollToControl);

            OMButton btnAnimateText = DefaultControls.GetButton("btnAnimateText", 540, 100, 180, 90, "", "Animate Text");
            btnAnimateText.OnClick += new userInteraction(btnAnimateText_OnClick);
            pSlideInTest.addControl(btnAnimateText);

            OMAnimatedLabel2 AniLabel_Test = new OMAnimatedLabel2("AniLabel_Test", 50, 400, 900, 35);
            // AniLabel_Test.SkinDebug = true;
            //AniLabel_Test.Text = String.Format("Current time and date is now {0}, this is a really long string so that we can test the animated labels in OpenMobile", DateTime.Now); //"This is a test of OMAnimatedLabel2";
            AniLabel_Test.Text = "This is a test of OMAnimatedLabel2";
            AniLabel_Test.Background = Color.Black;
            AniLabel_Test.SoftEdges = FadingEdge.GraphicSides.Left | FadingEdge.GraphicSides.Right | FadingEdge.GraphicSides.Top | FadingEdge.GraphicSides.Bottom;
            AniLabel_Test.Animation = OMAnimatedLabel2.eAnimation.Flash;
            AniLabel_Test.AnimationSingle = OMAnimatedLabel2.eAnimation.SlideUpSmooth;
            pSlideInTest.addControl(AniLabel_Test);

            OMImage imgTextEffect = new OMImage("imgTextEffect", 50, 450, 900, 35);
            // Generate static text image
            imgTextEffect.Image = new imageItem(
                OpenMobile.Graphics.Graphics.GenerateTextTexture(null, 0, 
                imgTextEffect.Left,
                imgTextEffect.Top,
                imgTextEffect.Width,
                imgTextEffect.Height,
                "This is a test of text effects",
                AniLabel_Test.Font,
                eTextFormat.OutlineNoFillNarrow,
                Alignment.CenterLeft,
                BuiltInComponents.SystemSettings.SkinTextColor,
                BuiltInComponents.SystemSettings.SkinFocusColor
                )
                );
            pSlideInTest.addControl(imgTextEffect);

            OMContainer Container = new OMContainer("Container", 200, 200, 300, 150);
            Container.Image = Host.getSkinImage("MediaBorder");
            Container.ScrollBar_ColorNormal = Color.Transparent;
            pSlideInTest.addControl(Container);

            OMImage Image_ContainerTest1 = new OMImage("Image_ContainerTest1", 0, 0, Host.getSkinImage("AlbumIcon_Highlighted"));
            Container.addControlRelative(Image_ContainerTest1);
            OMImage Image_ContainerTest2 = new OMImage("Image_ContainerTest2", 250, 50, Host.getSkinImage("AlbumIcon_SelectedHighlighted"));
            Container.addControlRelative(Image_ContainerTest2);
            OMButton btn_ContainerTest3 = DefaultControls.GetButton("btn_ContainerTest3", 50, 50, 180, 90, "", "Test");
            btn_ContainerTest3.OnClick += new userInteraction(btn_ContainerTest3_OnClick);
            Container.addControlRelative(btn_ContainerTest3);

            OMButton Button_PanelSlideIn  = DefaultControls.GetHorisontalEdgeButton("Button_SlideIn", 420, 540, 160, 70, "5", "");
            Button_PanelSlideIn.OnClick += new userInteraction(menuButton_OnClick);
            pSlideInTest.addControl(Button_PanelSlideIn);

            PanelOutlineGraphic.GraphicData gd = new PanelOutlineGraphic.GraphicData();
            gd.Width = 1100;
            gd.Height = 300;
            gd.ShadowType = PanelOutlineGraphic.ShadowTypes.None;
            gd.GraphicPath = GetPath_RoundedRectangleArchedTop(new System.Drawing.Rectangle(0, 15, gd.Width, gd.Height - 15), 30, 50);
            imgPanel_Background = new imageItem(PanelOutlineGraphic.GetImage(ref gd));

            gd = new PanelOutlineGraphic.GraphicData();
            gd.Width = 1100;
            gd.Height = 300;
            gd.GraphicPath = GetPath_RoundedRectangleArchedTop(new System.Drawing.Rectangle(0, 15, gd.Width, gd.Height - 15), 30, 50);
            imgPanel_Background_Highlighted = new imageItem(PanelOutlineGraphic.GetImage(ref gd));

            OMImage Image_panel_Background = new OMImage("Image_Panel_Background", -50, 580, imgPanel_Background);
            //Image_panel_Background.FitControlToImage = true;
            //Image_panel_Background.Image = imgPanel_Background;
            pSlideInTest.addControl(Image_panel_Background);

            pSlideInTest.Entering += new PanelEvent(pSlideInTest_Entering);
            pSlideInTest.Leaving += new PanelEvent(pSlideInTest_Leaving);

            manager.loadPanel(pSlideInTest);

        }

        static void btnAnimateText_OnClick(OMControl sender, int screen)
        {
            OMAnimatedLabel2 lbl = ((OMAnimatedLabel2)sender.Parent[screen, "AniLabel_Test"]);

            lbl.TransitionInText(OMAnimatedLabel2.eAnimation.Glow, "Animation effect test");
        }

        static void btnScrollToControl_OnClick(OMControl sender, int screen)
        {
            OMContainer container = (OMContainer)sender.Parent[screen, "Container"];
            int i = (int)sender.Tag;
            switch (i)
            {
                case 0:
                    container.ScrollToControl("Image_ContainerTest2");
                    break;
                case 1:
                    container.ScrollToControl("btn_ContainerTest3");
                    break;
                case 2:
                    container.ScrollToControl("Image_ContainerTest1");
                    break;
                default:
                    i = -1;
                    break;
            }
            i++;
            sender.Tag = i;
        }

        static void btn_ContainerTest3_OnClick(OMControl sender, int screen)
        {
            Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Container test"));
        }

        static void btnChangeText_OnClick(OMControl sender, int screen)
        {
            OMAnimatedLabel2 lbl = ((OMAnimatedLabel2)sender.Parent[screen, "AniLabel_Test"]);

            int lblState;
            if (lbl.Tag == null)
                lblState = 0;
            else
                lblState = (int)lbl.Tag;
            
            switch (lblState)
	        {
                case 0:
                    lbl.Text = String.Format("Current time and date is now {0}, this is a really long string so that we can test the animated labels in OpenMobile", DateTime.Now);
                    lbl.Tag = 1;
                    break;

                case 1:
                    lbl.Text = "This is a test of OMAnimatedLabel2";
                    lbl.Tag = 0;
                    break;

                default:
                    break;
	        }
        }

        
        static void btnMoveContainer_OnClick(OMControl sender, int screen)
        {
            OMControl ctrl = sender.Parent[screen, "Container"]; 
            //ctrl.Left += 50;
            //ctrl.Top += 10;

            SmoothAnimator Animation = new SmoothAnimator(0.25f);
            int Pos = ctrl.Left;
            int EndPos = 600;

            if (ctrl.Left != EndPos)
            {
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Pos += AnimationStep;
                    if (Pos >= EndPos)
                    {
                        ctrl.Left = EndPos;
                        return false;
                    }
                    else
                    {
                        ctrl.Left = Pos;
                        return true;
                    }
                });
            }
            else
            {
                EndPos = 200;
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Pos -= AnimationStep;
                    if (Pos <= EndPos)
                    {
                        ctrl.Left = EndPos;
                        return false;
                    }
                    else
                    {
                        ctrl.Left = Pos;
                        return true;
                    }
                });
            }

        }

        static void pSlideInTest_Leaving(OMPanel sender, int screen)
        {
            // Disconnect host events
            Host.OnGesture -= Gesture;
        }

        static void pSlideInTest_Entering(OMPanel sender, int screen)
        {
            // Connect host events
            Host.OnGesture += Gesture;
        }

        static bool Host_OnGesture(int screen, string character, string pluginName, string panelName, ref bool handled)
        {
            if (pluginName == PluginName && panelName == pSlideInTest.Name)
                menuButton_OnClick(pSlideInTest[screen, "Button_SlideIn"], screen);

            return false;
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetPath_RoundedRectangle(System.Drawing.Rectangle r, int d)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);

            int TabSize = 100;
            gp.AddArc(r.X + 100, r.Y - TabSize, TabSize, TabSize, 90, -90);
            gp.AddArc(r.X + r.Width - 100 - TabSize, r.Y - TabSize, TabSize, TabSize, 180, -90);

            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);
            return gp;
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetPath_RoundedRectangleArchedTop(System.Drawing.Rectangle r, int d, int TopArchHeight)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddArc(r.X, r.Y, r.Width, TopArchHeight*2, 180, 180);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private static List<OMControl> GetMediaControls(int screen)
        {
            return pSlideInTest.getPanelAtScreen(screen).Controls.FindAll(x => x.Name.Contains("Panel"));
        }

        private static bool[] MediaBarVisible = null;
        private static void menuButton_OnClick(OMControl sender, int screen)
        {
            // Initialize variable 
            if (MediaBarVisible == null)
                MediaBarVisible = new bool[Host.ScreenCount];

            List<OMControl> Controls = pSlideInTest.getPanelAtScreen(screen).Controls.FindAll(x => x.Name.Contains("Panel"));
            OMControl MainControl = pSlideInTest[screen, "Button_SlideIn"];
            OMImage img = (OMImage)pSlideInTest[screen, "Image_Panel_Background"];

            if (!MediaBarVisible[screen])
            {   // Show media bar
                if (img != null)
                    img.Image = imgPanel_Background_Highlighted;
                AnimateAndMoveControls(true, screen, 280, 540, 1.8f, MainControl, Controls);
                if (MainControl != null)
                    DefaultControls.UpdateHorisontalEdgeButton((OMButton)MainControl, 420, 540, 160, 70, "6", "");
            }
            else
            {   // Hide media bar
                AnimateAndMoveControls(false, screen, 280, 540, 1.8f, MainControl, Controls);
                if (img != null)
                    img.Image = imgPanel_Background;
                if (MainControl != null)
                    DefaultControls.UpdateHorisontalEdgeButton((OMButton)MainControl, 420, 540, 160, 70, "5", "");
            }
        }

        private static void AnimateAndMoveControls(bool up, int screen, int TopPos, int BottomPos, float AnimationSpeed, OMControl MainControl, List<OMControl> ControlsToMove)
        {
            // Calculate relative placements of media controls
            int[] RelativePlacements = new int[ControlsToMove.Count];
            for (int i = 0; i < RelativePlacements.Length; i++)
                RelativePlacements[i] = ControlsToMove[i].Top - MainControl.Top;

            if (up)
            {   // Move media bar up                
                int EndPos = TopPos;
                int Top = MainControl.Top;

                if (AnimationSpeed == 0)
                    AnimationSpeed = 0.9f;

                SmoothAnimator Animation = new SmoothAnimator(AnimationSpeed);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Top -= AnimationStep;
                    if (Top <= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = true;
            }
            else
            {   // Move media bar down
                int EndPos = BottomPos;
                int Top = MainControl.Top;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Top += AnimationStep;
                    if (Top >= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = false;
            }
        }

    }
}
