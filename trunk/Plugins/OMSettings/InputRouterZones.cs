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

namespace OMSettings
{
    internal static class InputRouterScreens 
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static int SelectedScreen = -1;
        static string[] KeyboardDevices = null;
        static string[] MouseDevices = null;
        static string[] AudioDevices = null;
        static OMPanel panelScreen = new OMPanel("InputRouterScreen");
        static OMPanel panelMultiScreen = new OMPanel("InputRouterScreenSelection");
        static bool MZSettingsChanged = false;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            #region Screen selection

            imageItem opt1 = Host.getSkinImage("Monitor");
            imageItem opt4 = Host.getSkinImage("Monitor.Highlighted");
            Font fnt = new Font(Font.GenericSansSerif, 99.75F);

            #region Screen buttons

            // Buttons will only be available for the available screens in the system
            OMButton[] ScreenSelection_Button = new OMButton[8];
            for (int i = 0; i < (ScreenSelection_Button.Length<Host.ScreenCount ? ScreenSelection_Button.Length : Host.ScreenCount); i++)
            {
                switch (i)
                {
                    case 0:
                        ScreenSelection_Button[i] = new OMButton(47, 132, 200, 200);
                        break;
                    case 1:
                        ScreenSelection_Button[i] = new OMButton(280, 132, 200, 200);
                        break;
                    case 2:
                        ScreenSelection_Button[i] = new OMButton(517, 132, 200, 200);
                        break;
                    case 3:
                        ScreenSelection_Button[i] = new OMButton(764, 132, 200, 200);
                        break;
                    case 4:
                        ScreenSelection_Button[i] = new OMButton(47, 335, 200, 200);
                        break;
                    case 5:
                        ScreenSelection_Button[i] = new OMButton(280, 335, 200, 200);
                        break;
                    case 6:
                        ScreenSelection_Button[i] = new OMButton(517, 335, 200, 200);
                        break;
                    case 7:
                        ScreenSelection_Button[i] = new OMButton(764, 335, 200, 200);
                        break;
                }

                ScreenSelection_Button[i].Image = opt1;
                ScreenSelection_Button[i].FocusImage = opt4;
                ScreenSelection_Button[i].Name = "ScreenSelection_Button" + i.ToString();
                ScreenSelection_Button[i].Font = fnt;
                ScreenSelection_Button[i].Text = (i+1).ToString();
                ScreenSelection_Button[i].Tag = i+1;
                ScreenSelection_Button[i].Format = eTextFormat.Outline;
                ScreenSelection_Button[i].OnClick += new userInteraction(ScreenSelection_Button_OnClick);
                panelMultiScreen.addControl(ScreenSelection_Button[i]);
            }

            #endregion

            OMButton ScreenSelection_ButtonIdentify = new OMButton(204, 540, 200, 60);
            ScreenSelection_ButtonIdentify.Image = Host.getSkinImage("Tab");
            ScreenSelection_ButtonIdentify.Font = new Font(Font.GenericSansSerif, 18F);
            ScreenSelection_ButtonIdentify.Text = "Identify";
            ScreenSelection_ButtonIdentify.Format = eTextFormat.DropShadow;
            ScreenSelection_ButtonIdentify.Name = "ScreenSelection_ButtonIdentify";
            ScreenSelection_ButtonIdentify.OnClick += new userInteraction(ScreenSelection_ButtonIdentify_OnClick);
            panelMultiScreen.addControl(ScreenSelection_ButtonIdentify);

            OMLabel ScreenSelection_LabelTitle = new OMLabel(383, 68, 250, 100);
            ScreenSelection_LabelTitle.Font = new Font(Font.GenericSansSerif, 21.75F);
            ScreenSelection_LabelTitle.Text = "Select a Screen";
            ScreenSelection_LabelTitle.Format = eTextFormat.BoldShadow;
            ScreenSelection_LabelTitle.Name = "ScreenSelection_LabelTitle";
            panelMultiScreen.addControl(ScreenSelection_LabelTitle);

            panelMultiScreen.Leaving += new PanelEvent(panelMultiScreen_Leaving);
            Manager.loadPanel(panelMultiScreen);
            #endregion

            #region Screen
            
            imageItem imgPlayHighlighted = Host.getSkinImage("Play.Highlighted");
            imageItem imgPlay = Host.getSkinImage("Play");
            imageItem imgFull = Host.getSkinImage("Full");
            imageItem imgFullHighlighted = Host.getSkinImage("Full.Highlighted");
            imageItem imgBtn = Host.getSkinImage("Btn");
            imageItem imgBtnHighlighted = Host.getSkinImage("Btn.Highlighted");

            OMLabel Screen_LabelCaption = new OMLabel(0, 110, 1000, 70);
            Screen_LabelCaption.Font = new Font(Font.GenericSansSerif, 34);
            Screen_LabelCaption.Name = "Screen_LabelCaption";
            Screen_LabelCaption.TextAlignment = Alignment.CenterCenter;
            panelScreen.addControl(Screen_LabelCaption);

            OMButton Screen_ButtonSave = new OMButton(10, 105, 150, 70);
            Screen_ButtonSave.Image = imgFull;
            Screen_ButtonSave.FocusImage = imgFullHighlighted;
            Screen_ButtonSave.Text = "Save";
            Screen_ButtonSave.Name = "Screen_ButtonSave";
            Screen_ButtonSave.Transition = eButtonTransition.None;
            Screen_ButtonSave.OnClick += new userInteraction(Screen_ButtonSave_OnClick);
            panelScreen.addControl(Screen_ButtonSave);

            OMButton Screen_ButtonCancel = new OMButton(840, 105, 150, 70);
            Screen_ButtonCancel.Image = imgFull;
            Screen_ButtonCancel.FocusImage = imgFullHighlighted;
            Screen_ButtonCancel.Text = "Cancel";
            Screen_ButtonCancel.Name = "Screen_ButtonCancel";
            Screen_ButtonCancel.Transition = eButtonTransition.None;
            Screen_ButtonCancel.OnClick += new userInteraction(Screen_ButtonCancel_OnClick);

            panelScreen.addControl(Screen_ButtonCancel);

            /*
            #region Audiounit

            OMButton Screen_ButtonAudioUnitRight = new OMButton(750, 215, 120, 80);
            Screen_ButtonAudioUnitRight.Image = imgPlay;
            Screen_ButtonAudioUnitRight.FocusImage = imgPlayHighlighted;
            Screen_ButtonAudioUnitRight.Name = "Screen_ButtonAudioUnitRight";
            Screen_ButtonAudioUnitRight.Tag = 1;
            Screen_ButtonAudioUnitRight.Transition = eButtonTransition.None;
            Screen_ButtonAudioUnitRight.OnClick += new userInteraction(Screen_ButtonAudioUnitLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonAudioUnitRight);

            OMButton Screen_ButtonAudioUnitLeft = new OMButton(163, 215, 120, 80);
            Screen_ButtonAudioUnitLeft.Image = imgPlay;
            Screen_ButtonAudioUnitLeft.FocusImage = imgPlayHighlighted;
            Screen_ButtonAudioUnitLeft.Orientation = eAngle.FlipHorizontal;
            Screen_ButtonAudioUnitLeft.Name = "Screen_ButtonAudioUnitLeft";
            Screen_ButtonAudioUnitLeft.Transition = eButtonTransition.None;
            Screen_ButtonAudioUnitLeft.Tag = -1;
            Screen_ButtonAudioUnitLeft.OnClick += new userInteraction(Screen_ButtonAudioUnitLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonAudioUnitLeft);

            OMButton Screen_ButtonAudioUnitTest = new OMButton(875, 215, 120, 80);
            Screen_ButtonAudioUnitTest.Image = imgBtn;
            Screen_ButtonAudioUnitTest.FocusImage = imgBtnHighlighted;
            Screen_ButtonAudioUnitTest.Text = "Test";
            Screen_ButtonAudioUnitTest.Font = new Font(Font.GenericSansSerif, 22F);
            Screen_ButtonAudioUnitTest.Name = "Screen_ButtonAudioUnitTest";
            Screen_ButtonAudioUnitTest.Transition = eButtonTransition.None;
            Screen_ButtonAudioUnitTest.OnClick += new userInteraction(Screen_ButtonAudioUnitTest_OnClick);
            panelScreen.addControl(Screen_ButtonAudioUnitTest);

            OMLabel Screen_LabelAudioUnit = new OMLabel(-5, 208, 160, 100);
            Screen_LabelAudioUnit.Font = new Font(Font.GenericSansSerif, 26F);
            Screen_LabelAudioUnit.Text = "Audio";
            Screen_LabelAudioUnit.Name = "Screen_LabelAudioUnit";
            Screen_LabelAudioUnit.TextAlignment = Alignment.CenterRight;
            panelScreen.addControl(Screen_LabelAudioUnit);

            OMTextBox Screen_TextBoxAudioUnit = new OMTextBox(291, 215, 450, 50);
            Screen_TextBoxAudioUnit.Flags = textboxFlags.EllipsisEnd;
            Screen_TextBoxAudioUnit.Font = new Font(Font.GenericSansSerif, 21.75F);
            Screen_TextBoxAudioUnit.Name = "Screen_TextBoxAudioUnit";
            panelScreen.addControl(Screen_TextBoxAudioUnit);

            #endregion 
            */

            #region Keyboard

            OMButton Screen_ButtonKeyboardRight = new OMButton(750, 315, 120, 80);
            Screen_ButtonKeyboardRight.Image = imgPlay;
            Screen_ButtonKeyboardRight.FocusImage = imgPlayHighlighted;
            Screen_ButtonKeyboardRight.Name = "Screen_ButtonKeyboardRight";
            Screen_ButtonKeyboardRight.Tag = 1;
            Screen_ButtonKeyboardRight.Transition = eButtonTransition.None;
            Screen_ButtonKeyboardRight.OnClick += new userInteraction(Screen_ButtonKeyboardLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonKeyboardRight);

            OMButton Screen_ButtonKeyboardLeft = new OMButton(163, 315, 120, 80);
            Screen_ButtonKeyboardLeft.Image = imgPlay;
            Screen_ButtonKeyboardLeft.FocusImage = imgPlayHighlighted;
            Screen_ButtonKeyboardLeft.Orientation = eAngle.FlipHorizontal;
            Screen_ButtonKeyboardLeft.Name = "Screen_ButtonKeyboardLeft";
            Screen_ButtonKeyboardLeft.Transition = eButtonTransition.None;
            Screen_ButtonKeyboardLeft.Tag = -1;
            Screen_ButtonKeyboardLeft.OnClick += new userInteraction(Screen_ButtonKeyboardLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonKeyboardLeft);

            OMButton Screen_ButtonKeyboardDetect = new OMButton(875, 315, 120, 80);
            Screen_ButtonKeyboardDetect.Image = imgBtn;
            Screen_ButtonKeyboardDetect.FocusImage = imgBtnHighlighted;
            Screen_ButtonKeyboardDetect.Text = "Detect";
            Screen_ButtonKeyboardDetect.Font = new Font(Font.GenericSansSerif, 22F);
            Screen_ButtonKeyboardDetect.Name = "Screen_ButtonKeyboardDetect";
            Screen_ButtonKeyboardDetect.Transition = eButtonTransition.None;
            Screen_ButtonKeyboardDetect.OnClick += new userInteraction(Screen_ButtonKeyboardDetect_OnClick);
            panelScreen.addControl(Screen_ButtonKeyboardDetect);

            OMLabel Screen_LabelKeyboard = new OMLabel(-5, 308, 160, 100);
            Screen_LabelKeyboard.Font = new Font(Font.GenericSansSerif, 26F);
            Screen_LabelKeyboard.Text = "Keyboard";
            Screen_LabelKeyboard.Name = "Screen_LabelKeyboard";
            Screen_LabelKeyboard.TextAlignment = Alignment.CenterRight;
            panelScreen.addControl(Screen_LabelKeyboard);

            OMBasicShape Screen_ShapeKeyboardCurrent = new OMBasicShape(308, 335, 416, 58);
            Screen_ShapeKeyboardCurrent.Shape = shapes.RoundedRectangle;
            Screen_ShapeKeyboardCurrent.Name = "Screen_ShapeKeyboardCurrent";
            Screen_ShapeKeyboardCurrent.BorderColor = Color.White;
            Screen_ShapeKeyboardCurrent.BorderSize = 1;
            panelScreen.addControl(Screen_ShapeKeyboardCurrent);

            OMTextBox Screen_TextBoxKeyboard = new OMTextBox(291, 315, 450, 50);
            Screen_TextBoxKeyboard.Flags = textboxFlags.EllipsisEnd;
            Screen_TextBoxKeyboard.Font = new Font(Font.GenericSansSerif, 21.75F);
            Screen_TextBoxKeyboard.Name = "Screen_TextBoxKeyboard";
            panelScreen.addControl(Screen_TextBoxKeyboard);

            OMLabel Screen_LabelKeyboardCurrent = new OMLabel(291, 369, 450, 20);
            Screen_LabelKeyboardCurrent.Font = new Font(Font.GenericSansSerif, 18F);
            Screen_LabelKeyboardCurrent.Text = "current keyboard mapping";
            Screen_LabelKeyboardCurrent.Name = "Screen_LabelKeyboardCurrent";
            Screen_LabelKeyboardCurrent.TextAlignment = Alignment.CenterCenter;
            panelScreen.addControl(Screen_LabelKeyboardCurrent);

            #endregion 

            #region Mouse

            OMButton Screen_ButtonMouseRight = new OMButton(750, 415, 120, 80);
            Screen_ButtonMouseRight.Image = imgPlay;
            Screen_ButtonMouseRight.FocusImage = imgPlayHighlighted;
            Screen_ButtonMouseRight.Name = "Screen_ButtonMouseRight";
            Screen_ButtonMouseRight.Tag = 1;
            Screen_ButtonMouseRight.Transition = eButtonTransition.None;
            Screen_ButtonMouseRight.OnClick += new userInteraction(Screen_ButtonMouseLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonMouseRight);

            OMButton Screen_ButtonMouseLeft = new OMButton(163, 415, 120, 80);
            Screen_ButtonMouseLeft.Image = imgPlay;
            Screen_ButtonMouseLeft.FocusImage = imgPlayHighlighted;
            Screen_ButtonMouseLeft.Orientation = eAngle.FlipHorizontal;
            Screen_ButtonMouseLeft.Name = "Screen_ButtonMouseLeft";
            Screen_ButtonMouseLeft.Transition = eButtonTransition.None;
            Screen_ButtonMouseLeft.Tag = -1;
            Screen_ButtonMouseLeft.OnClick += new userInteraction(Screen_ButtonMouseLeftRight_OnClick);
            panelScreen.addControl(Screen_ButtonMouseLeft);

            OMButton Screen_ButtonMouseDetect = new OMButton(875, 415, 120, 80);
            Screen_ButtonMouseDetect.Image = imgBtn;
            Screen_ButtonMouseDetect.FocusImage = imgBtnHighlighted;
            Screen_ButtonMouseDetect.Text = "Detect";
            Screen_ButtonMouseDetect.Font = new Font(Font.GenericSansSerif, 22F);
            Screen_ButtonMouseDetect.Name = "Screen_ButtonMouseDetect";
            Screen_ButtonMouseDetect.Transition = eButtonTransition.None;
            Screen_ButtonMouseDetect.OnClick += new userInteraction(Screen_ButtonMouseDetect_OnClick);
            panelScreen.addControl(Screen_ButtonMouseDetect);

            OMLabel Screen_LabelMouse = new OMLabel(-5, 408, 160, 100);
            Screen_LabelMouse.Font = new Font(Font.GenericSansSerif, 26F);
            Screen_LabelMouse.Text = "Mouse";
            Screen_LabelMouse.Name = "Screen_LabelMouse";
            Screen_LabelMouse.TextAlignment = Alignment.CenterRight;
            panelScreen.addControl(Screen_LabelMouse);

            OMBasicShape Screen_ShapeMouseCurrent = new OMBasicShape(308, 435, 416, 58);
            Screen_ShapeMouseCurrent.Shape = shapes.RoundedRectangle;
            Screen_ShapeMouseCurrent.Name = "Screen_ShapeMouseCurrent";
            Screen_ShapeMouseCurrent.BorderColor = Color.White;
            Screen_ShapeMouseCurrent.BorderSize = 1;
            panelScreen.addControl(Screen_ShapeMouseCurrent);

            OMTextBox Screen_TextBoxMouse = new OMTextBox(291, 415, 450, 50);
            Screen_TextBoxMouse.Flags = textboxFlags.EllipsisEnd;
            Screen_TextBoxMouse.Font = new Font(Font.GenericSansSerif, 21.75F);
            Screen_TextBoxMouse.Name = "Screen_TextBoxMouse";
            panelScreen.addControl(Screen_TextBoxMouse);

            OMLabel Screen_LabelMouseCurrent = new OMLabel(291, 469, 450, 20);
            Screen_LabelMouseCurrent.Font = new Font(Font.GenericSansSerif, 18F);
            Screen_LabelMouseCurrent.Text = "current mouse mapping";
            Screen_LabelMouseCurrent.Name = "Screen_LabelMouseCurrent";
            Screen_LabelMouseCurrent.TextAlignment = Alignment.CenterCenter;
            panelScreen.addControl(Screen_LabelMouseCurrent);

            #endregion 

            panelScreen.Loaded += new PanelEvent(Screen_Entering);
            panelScreen.Leaving += new PanelEvent(Screen_Leaving);

            manager.loadPanel(panelScreen);

            #endregion
        }

        static void Screen_ButtonMouseDetect_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelScreen.Name);
            dialog.Height = 350;
            dialog.Header = "Automatic input detection";
            dialog.Text = "Clicking [Yes] will start automatic mouse detection,\nclick [No] to cancel.\n\nClick the mouse or touch the screen of the Screen you want to assign when the Screen identificator number is showing.\nDetection will cancel after 10 seconds if no input is detected.";
            dialog.Icon = icons.Question;
            dialog.Button = buttons.Yes | buttons.No;
            if (dialog.ShowMsgBox(screen) == buttons.No)
                return;

            Host.ScreenShowIdentity(SelectedScreen-1, true);
            object o;
            Host.getData(eGetData.GetMouseDetectedUnit, "", out o);
            Host.ScreenShowIdentity(SelectedScreen-1, false);

            string DetectedUnit = (string)o;

            // Did we detect anything?
            if (DetectedUnit == "")
            {
                dialog = new dialog(PluginName, panelScreen.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "No mouse device was detected!\nMake sure that your devices are connected and working in your OS before using OM.\nPlease verify devices and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }
            else
            {   // Device detected, ask user if this is correct
                dialog = new dialog(PluginName, panelScreen.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "Detected input from\n" + DetectedUnit + "\nUse this unit for Screen " + SelectedScreen.ToString() + "?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            // Everything ok, use this new unit
            ((OMLabel)panelScreen[screen, "Screen_TextBoxMouse"]).Text = DetectedUnit;
        }

        static void Screen_ButtonKeyboardDetect_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelScreen.Name);
            dialog.Height = 350;
            dialog.Header = "Automatic input detection";
            dialog.Text = "Clicking [Yes] will start automatic keyboard detection,\nclick [No] to cancel\n\nPress a key on the keyboard you want to assign when the Screen identificator number is showing.\nDetection will cancel after 10 seconds if no input is detected.";
            dialog.Icon = icons.Question;
            dialog.Button = buttons.Yes | buttons.No;
            if (dialog.ShowMsgBox(screen) == buttons.No)
                return;

            Host.ScreenShowIdentity(SelectedScreen-1, true);
            object o;
            Host.getData(eGetData.GetKeyboardDetectedUnit, "", out o);
            Host.ScreenShowIdentity(SelectedScreen-1, false);

            string DetectedUnit = (string)o;

            // Did we detect anything?
            if (DetectedUnit == "")
            {
                dialog = new dialog(PluginName, panelScreen.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "No keyboard device was detected!\nMake sure that your devices are connected and working in your OS before using OM.\nPlease verify devices and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }
            else
            {   // Device detected, ask user if this is correct
                dialog = new dialog(PluginName, panelScreen.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "Detected input from\n" + DetectedUnit + "\nUse this unit for Screen " + SelectedScreen.ToString() + "?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            // Everything ok, use this new unit
            ((OMLabel)panelScreen[screen, "Screen_TextBoxKeyboard"]).Text = DetectedUnit;
        }

        static void Screen_ButtonAudioUnitTest_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelScreen.Name);
            dialog.Header = "OpenMobile";
            dialog.Text = "This function is not yet implemented!";
            dialog.Icon = icons.WorkInProgress;
            dialog.Button = buttons.OK;
            dialog.ShowMsgBox(screen).ToString();
        }

        #region Screen controls

        #region Screen Device selections

        static void Screen_ButtonAudioUnitLeftRight_OnClick(OMControl sender, int screen)
        {
            //OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelScreen[screen, "Screen_TextBoxAudioUnit"], AudioDevices);
        }

        static void Screen_ButtonKeyboardLeftRight_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelScreen[screen, "Screen_TextBoxKeyboard"],KeyboardDevices);
        }

        static void Screen_ButtonMouseLeftRight_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelScreen[screen, "Screen_TextBoxMouse"], MouseDevices);
        }

        #endregion

        static void Screen_ButtonCancel_OnClick(OMControl sender, int screen)
        {
            // Show warning if settings changed
            MZSettingsChanged = Screen_SettingChanged(screen);
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiScreen.Name);
                dialog.Header = "MultiScreen settings";
                dialog.Text = "MultiScreen settings was modified!\nReturn without saving?";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.No)
                    return;
                else
                {   // Don't save settings                    
                    MZSettingsChanged = false;
                    // First goback removes dialog
                    Host.execute(eFunction.goBack, screen.ToString());
                }
            }
            Host.execute(eFunction.goBack, screen.ToString());
        }

        static void Screen_ButtonSave_OnClick(OMControl sender, int screen)
        {
            // Show warning if settings changed
            MZSettingsChanged = Screen_SettingChanged(screen);
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiScreen.Name);
                dialog.Header = "MultiScreen settings";
                dialog.Text = "MultiScreen settings was modified, please restart OM to activate new settings.";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                Host.execute(eFunction.goBack, screen.ToString());
            }

            // Save new settings
            using (PluginSettings settings = new PluginSettings())
            {
                //settings.setSetting("Screen" + SelectedScreen.ToString() + ".SoundCard", ((OMLabel)panelScreen[screen, "Screen_TextBoxAudioUnit"]).Text);
                settings.setSetting("Screen" + SelectedScreen.ToString() + ".Keyboard", ((OMLabel)panelScreen[screen, "Screen_TextBoxKeyboard"]).Text);
                settings.setSetting("Screen" + SelectedScreen.ToString() + ".Mouse", ((OMLabel)panelScreen[screen, "Screen_TextBoxMouse"]).Text);
            }

            Host.execute(eFunction.goBack, screen.ToString());
            Host.execute(eFunction.settingsChanged);
        }

        static void Screen_Leaving(OMPanel sender, int screen)
        {
            SelectedScreen = -1;
        }

        static void Screen_Entering(OMPanel sender, int screen)
        {
            // Is current Screen outside a valid range?
            if ((SelectedScreen <= 0) || (SelectedScreen > Host.ScreenCount))
            {
                dialog dialog = new dialog(PluginName, panelScreen.Name);
                dialog.Header = "MultiScreen settings";
                dialog.Text = "An error occured; Selected Screen is outside of valid range!\nPlease go back and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                Host.execute(eFunction.goBack, screen.ToString());
                Host.execute(eFunction.goBack, screen.ToString());
                return;
            }

            // Set caption
            ((OMLabel)panelScreen[screen, "Screen_LabelCaption"]).Text = "Input routing for Screen " + SelectedScreen.ToString();

            // Get current settings and set textbox values
            using (PluginSettings settings = new PluginSettings())
            {
                //((OMLabel)panelScreen[screen, "Screen_TextBoxAudioUnit"]).Text = settings.getSetting("Screen" + SelectedScreen.ToString() + ".SoundCard");
                ((OMLabel)panelScreen[screen, "Screen_TextBoxKeyboard"]).Text = settings.getSetting("Screen" + SelectedScreen.ToString() + ".Keyboard");
                ((OMLabel)panelScreen[screen, "Screen_TextBoxMouse"]).Text = settings.getSetting("Screen" + SelectedScreen.ToString() + ".Mouse");
            }

            // Get Audio devices
            object o;
            Host.getData(eGetData.GetAudioDevices, "", out o);
            if (o != null)
                AudioDevices = (string[])o;

            // Get keyboard units
            o = new object();
            Host.getData(eGetData.GetKeyboardUnitsForScreen, "", (SelectedScreen-1).ToString(), out o);
            if (o != null)
                KeyboardDevices = (string[])o;

            // Get current keyboard mapping
            o = new object();
            Host.getData(eGetData.GetKeyboardCurrentUnitForScreen, "", (SelectedScreen-1).ToString(), out o);
            if (o != null)
                ((OMLabel)panelScreen[screen, "Screen_LabelKeyboardCurrent"]).Text = (string)o;

            // Get mouse units
            o = new object();
            Host.getData(eGetData.GetMiceUnitsForScreen, "", (SelectedScreen-1).ToString(), out o);
            if (o != null)
                MouseDevices = (string[])o;

            // Get current mouse mapping
            o = new object();
            Host.getData(eGetData.GetMiceCurrentUnitForScreen, "", (SelectedScreen-1).ToString(), out o);
            if (o != null)
                ((OMLabel)panelScreen[screen, "Screen_LabelMouseCurrent"]).Text = (string)o;
        }

        static bool Screen_SettingChanged(int screen)
        {
            // Get current settings and set textbox values
            string CurrentAudioSetting, CurrentKeyboardSetting, CurrentMouseSetting;

            using (PluginSettings settings = new PluginSettings())
            {
                CurrentAudioSetting = settings.getSetting("Screen" + SelectedScreen.ToString() + ".SoundCard");
                CurrentKeyboardSetting = settings.getSetting("Screen" + SelectedScreen.ToString() + ".Keyboard");
                CurrentMouseSetting = settings.getSetting("Screen" + SelectedScreen.ToString() + ".Mouse");
            }

            // Compare to current values
            //if (((OMLabel)panelScreen[screen, "Screen_TextBoxAudioUnit"]).Text != CurrentAudioSetting)
            //    return true;
            if (((OMLabel)panelScreen[screen, "Screen_TextBoxKeyboard"]).Text != CurrentKeyboardSetting)
                return true;
            if (((OMLabel)panelScreen[screen, "Screen_TextBoxMouse"]).Text != CurrentMouseSetting)
                return true;
            return false;
        }

        #endregion

        #region Screen selection controls

        static void ScreenSelection_Button_OnClick(OMControl sender, int screen)
        {
            // Is settings already in action
            if (SelectedScreen >= 0)
            {
                dialog dialog = new dialog(PluginName, panelMultiScreen.Name);
                dialog.Header = "MultiScreen settings";
                dialog.Text = "MultiScreen settings is already active on another screen!\nPlease close this first.";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }

            // Set selected multiScreen
            SelectedScreen = (int)((OMButton)sender).Tag;

            // Show panel
            Host.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings", "InputRouterScreenSelection");
            Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "InputRouterScreen");
            Host.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
        }

        static void ScreenSelection_ButtonIdentify_OnClick(OMControl sender, int screen)
        {
            Host.ScreenShowIdentity(3000);
        }

        static void panelMultiScreen_Leaving(OMPanel sender, int screen)
        {
            // Show warning if settings changed
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiScreen.Name);
                dialog.Header = "MultiScreen settings";
                dialog.Text = "MultiScreen settings was modified!\nRestart OM?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.Yes)
                    Host.execute(eFunction.restartProgram);
            }
        }
        #endregion

    }
}
