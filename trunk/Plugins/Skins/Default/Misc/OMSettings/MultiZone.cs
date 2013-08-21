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
    internal static class MultiZone 
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static int SelectedZone = -1;
        static string[] KeyboardDevices = null;
        static string[] MouseDevices = null;
        static string[] AudioDevices = null;
        static OMPanel panelZone = new OMPanel("Zone");
        static OMPanel panelMultiZone = new OMPanel("ZoneSelection");
        static bool MZSettingsChanged = false;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            #region Zone selection

            imageItem opt1 = Host.getSkinImage("Monitor");
            imageItem opt4 = Host.getSkinImage("Monitor.Highlighted");
            Font fnt = new Font(Font.GenericSansSerif, 99.75F);

            #region Zone buttons

            // Buttons will only be available for the available screens in the system
            OMButton[] ZoneSelection_Button = new OMButton[8];
            for (int i = 0; i < (ZoneSelection_Button.Length<Host.ScreenCount ? ZoneSelection_Button.Length : Host.ScreenCount); i++)
            {
                switch (i)
                {
                    case 0:
                        ZoneSelection_Button[i] = new OMButton(47, 132, 200, 200);
                        break;
                    case 1:
                        ZoneSelection_Button[i] = new OMButton(280, 132, 200, 200);
                        break;
                    case 2:
                        ZoneSelection_Button[i] = new OMButton(517, 132, 200, 200);
                        break;
                    case 3:
                        ZoneSelection_Button[i] = new OMButton(764, 132, 200, 200);
                        break;
                    case 4:
                        ZoneSelection_Button[i] = new OMButton(47, 335, 200, 200);
                        break;
                    case 5:
                        ZoneSelection_Button[i] = new OMButton(280, 335, 200, 200);
                        break;
                    case 6:
                        ZoneSelection_Button[i] = new OMButton(517, 335, 200, 200);
                        break;
                    case 7:
                        ZoneSelection_Button[i] = new OMButton(764, 335, 200, 200);
                        break;
                }

                ZoneSelection_Button[i].Image = opt1;
                ZoneSelection_Button[i].FocusImage = opt4;
                ZoneSelection_Button[i].Name = "ZoneSelection_Button" + i.ToString();
                ZoneSelection_Button[i].Font = fnt;
                ZoneSelection_Button[i].Text = (i+1).ToString();
                ZoneSelection_Button[i].Tag = i+1;
                ZoneSelection_Button[i].Format = eTextFormat.Outline;
                ZoneSelection_Button[i].OnClick += new userInteraction(ZoneSelection_Button_OnClick);
                panelMultiZone.addControl(ZoneSelection_Button[i]);
            }

            #endregion

            OMButton ZoneSelection_ButtonIdentify = new OMButton(204, 540, 200, 60);
            ZoneSelection_ButtonIdentify.Image = Host.getSkinImage("Tab");
            ZoneSelection_ButtonIdentify.Font = new Font(Font.GenericSansSerif, 18F);
            ZoneSelection_ButtonIdentify.Text = "Identify";
            ZoneSelection_ButtonIdentify.Format = eTextFormat.DropShadow;
            ZoneSelection_ButtonIdentify.Name = "ZoneSelection_ButtonIdentify";
            ZoneSelection_ButtonIdentify.OnClick += new userInteraction(ZoneSelection_ButtonIdentify_OnClick);
            panelMultiZone.addControl(ZoneSelection_ButtonIdentify);

            OMLabel ZoneSelection_LabelTitle = new OMLabel(383, 68, 250, 100);
            ZoneSelection_LabelTitle.Font = new Font(Font.GenericSansSerif, 21.75F);
            ZoneSelection_LabelTitle.Text = "Select a Screen";
            ZoneSelection_LabelTitle.Format = eTextFormat.BoldShadow;
            ZoneSelection_LabelTitle.Name = "ZoneSelection_LabelTitle";
            panelMultiZone.addControl(ZoneSelection_LabelTitle);

            panelMultiZone.Leaving += new PanelEvent(panelMultiZone_Leaving);
            Manager.loadPanel(panelMultiZone);
            #endregion

            #region zone
            
            imageItem imgPlayHighlighted = Host.getSkinImage("Play.Highlighted");
            imageItem imgPlay = Host.getSkinImage("Play");
            imageItem imgFull = Host.getSkinImage("Full");
            imageItem imgFullHighlighted = Host.getSkinImage("Full.Highlighted");
            imageItem imgBtn = Host.getSkinImage("Btn");
            imageItem imgBtnHighlighted = Host.getSkinImage("Btn.Highlighted");

            OMLabel Zone_LabelCaption = new OMLabel(0, 110, 1000, 70);
            Zone_LabelCaption.Font = new Font(Font.GenericSansSerif, 44);
            Zone_LabelCaption.Name = "Zone_LabelCaption";
            Zone_LabelCaption.TextAlignment = Alignment.CenterCenter;
            panelZone.addControl(Zone_LabelCaption);

            OMButton Zone_ButtonSave = new OMButton(10, 105, 150, 70);
            Zone_ButtonSave.Image = imgFull;
            Zone_ButtonSave.FocusImage = imgFullHighlighted;
            Zone_ButtonSave.Text = "Save";
            Zone_ButtonSave.Name = "Zone_ButtonSave";
            Zone_ButtonSave.Transition = eButtonTransition.None;
            Zone_ButtonSave.OnClick += new userInteraction(Zone_ButtonSave_OnClick);
            panelZone.addControl(Zone_ButtonSave);

            OMButton Zone_ButtonCancel = new OMButton(840, 105, 150, 70);
            Zone_ButtonCancel.Image = imgFull;
            Zone_ButtonCancel.FocusImage = imgFullHighlighted;
            Zone_ButtonCancel.Text = "Cancel";
            Zone_ButtonCancel.Name = "Zone_ButtonCancel";
            Zone_ButtonCancel.Transition = eButtonTransition.None;
            Zone_ButtonCancel.OnClick += new userInteraction(Zone_ButtonCancel_OnClick);

            panelZone.addControl(Zone_ButtonCancel);

            #region Audiounit

            OMButton Zone_ButtonAudioUnitRight = new OMButton(750, 215, 120, 80);
            Zone_ButtonAudioUnitRight.Image = imgPlay;
            Zone_ButtonAudioUnitRight.FocusImage = imgPlayHighlighted;
            Zone_ButtonAudioUnitRight.Name = "Zone_ButtonAudioUnitRight";
            Zone_ButtonAudioUnitRight.Tag = 1;
            Zone_ButtonAudioUnitRight.Transition = eButtonTransition.None;
            Zone_ButtonAudioUnitRight.OnClick += new userInteraction(Zone_ButtonAudioUnitLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonAudioUnitRight);

            OMButton Zone_ButtonAudioUnitLeft = new OMButton(163, 215, 120, 80);
            Zone_ButtonAudioUnitLeft.Image = imgPlay;
            Zone_ButtonAudioUnitLeft.FocusImage = imgPlayHighlighted;
            Zone_ButtonAudioUnitLeft.Orientation = eAngle.FlipHorizontal;
            Zone_ButtonAudioUnitLeft.Name = "Zone_ButtonAudioUnitLeft";
            Zone_ButtonAudioUnitLeft.Transition = eButtonTransition.None;
            Zone_ButtonAudioUnitLeft.Tag = -1;
            Zone_ButtonAudioUnitLeft.OnClick += new userInteraction(Zone_ButtonAudioUnitLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonAudioUnitLeft);

            OMButton Zone_ButtonAudioUnitTest = new OMButton(875, 215, 120, 80);
            Zone_ButtonAudioUnitTest.Image = imgBtn;
            Zone_ButtonAudioUnitTest.FocusImage = imgBtnHighlighted;
            Zone_ButtonAudioUnitTest.Text = "Test";
            Zone_ButtonAudioUnitTest.Font = new Font(Font.GenericSansSerif, 22F);
            Zone_ButtonAudioUnitTest.Name = "Zone_ButtonAudioUnitTest";
            Zone_ButtonAudioUnitTest.Transition = eButtonTransition.None;
            Zone_ButtonAudioUnitTest.OnClick += new userInteraction(Zone_ButtonAudioUnitTest_OnClick);
            panelZone.addControl(Zone_ButtonAudioUnitTest);

            OMLabel Zone_LabelAudioUnit = new OMLabel(-5, 208, 160, 100);
            Zone_LabelAudioUnit.Font = new Font(Font.GenericSansSerif, 26F);
            Zone_LabelAudioUnit.Text = "Audio";
            Zone_LabelAudioUnit.Name = "Zone_LabelAudioUnit";
            Zone_LabelAudioUnit.TextAlignment = Alignment.CenterRight;
            panelZone.addControl(Zone_LabelAudioUnit);

            OMTextBox Zone_TextBoxAudioUnit = new OMTextBox(291, 215, 450, 50);
            Zone_TextBoxAudioUnit.Flags = textboxFlags.EllipsisEnd;
            Zone_TextBoxAudioUnit.Font = new Font(Font.GenericSansSerif, 21.75F);
            Zone_TextBoxAudioUnit.Name = "Zone_TextBoxAudioUnit";
            panelZone.addControl(Zone_TextBoxAudioUnit);

            #endregion 

            #region Keyboard

            OMButton Zone_ButtonKeyboardRight = new OMButton(750, 315, 120, 80);
            Zone_ButtonKeyboardRight.Image = imgPlay;
            Zone_ButtonKeyboardRight.FocusImage = imgPlayHighlighted;
            Zone_ButtonKeyboardRight.Name = "Zone_ButtonKeyboardRight";
            Zone_ButtonKeyboardRight.Tag = 1;
            Zone_ButtonKeyboardRight.Transition = eButtonTransition.None;
            Zone_ButtonKeyboardRight.OnClick += new userInteraction(Zone_ButtonKeyboardLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonKeyboardRight);

            OMButton Zone_ButtonKeyboardLeft = new OMButton(163, 315, 120, 80);
            Zone_ButtonKeyboardLeft.Image = imgPlay;
            Zone_ButtonKeyboardLeft.FocusImage = imgPlayHighlighted;
            Zone_ButtonKeyboardLeft.Orientation = eAngle.FlipHorizontal;
            Zone_ButtonKeyboardLeft.Name = "Zone_ButtonKeyboardLeft";
            Zone_ButtonKeyboardLeft.Transition = eButtonTransition.None;
            Zone_ButtonKeyboardLeft.Tag = -1;
            Zone_ButtonKeyboardLeft.OnClick += new userInteraction(Zone_ButtonKeyboardLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonKeyboardLeft);

            OMButton Zone_ButtonKeyboardDetect = new OMButton(875, 315, 120, 80);
            Zone_ButtonKeyboardDetect.Image = imgBtn;
            Zone_ButtonKeyboardDetect.FocusImage = imgBtnHighlighted;
            Zone_ButtonKeyboardDetect.Text = "Detect";
            Zone_ButtonKeyboardDetect.Font = new Font(Font.GenericSansSerif, 22F);
            Zone_ButtonKeyboardDetect.Name = "Zone_ButtonKeyboardDetect";
            Zone_ButtonKeyboardDetect.Transition = eButtonTransition.None;
            Zone_ButtonKeyboardDetect.OnClick += new userInteraction(Zone_ButtonKeyboardDetect_OnClick);
            panelZone.addControl(Zone_ButtonKeyboardDetect);

            OMLabel Zone_LabelKeyboard = new OMLabel(-5, 308, 160, 100);
            Zone_LabelKeyboard.Font = new Font(Font.GenericSansSerif, 26F);
            Zone_LabelKeyboard.Text = "Keyboard";
            Zone_LabelKeyboard.Name = "Zone_LabelKeyboard";
            Zone_LabelKeyboard.TextAlignment = Alignment.CenterRight;
            panelZone.addControl(Zone_LabelKeyboard);

            OMBasicShape Zone_ShapeKeyboardCurrent = new OMBasicShape(308, 335, 416, 58);
            Zone_ShapeKeyboardCurrent.Shape = shapes.RoundedRectangle;
            Zone_ShapeKeyboardCurrent.Name = "Zone_ShapeKeyboardCurrent";
            Zone_ShapeKeyboardCurrent.BorderColor = Color.White;
            Zone_ShapeKeyboardCurrent.BorderSize = 1;
            panelZone.addControl(Zone_ShapeKeyboardCurrent);

            OMTextBox Zone_TextBoxKeyboard = new OMTextBox(291, 315, 450, 50);
            Zone_TextBoxKeyboard.Flags = textboxFlags.EllipsisEnd;
            Zone_TextBoxKeyboard.Font = new Font(Font.GenericSansSerif, 21.75F);
            Zone_TextBoxKeyboard.Name = "Zone_TextBoxKeyboard";
            panelZone.addControl(Zone_TextBoxKeyboard);

            OMLabel Zone_LabelKeyboardCurrent = new OMLabel(291, 369, 450, 20);
            Zone_LabelKeyboardCurrent.Font = new Font(Font.GenericSansSerif, 18F);
            Zone_LabelKeyboardCurrent.Text = "current keyboard mapping";
            Zone_LabelKeyboardCurrent.Name = "Zone_LabelKeyboardCurrent";
            Zone_LabelKeyboardCurrent.TextAlignment = Alignment.CenterCenter;
            panelZone.addControl(Zone_LabelKeyboardCurrent);

            #endregion 

            #region Mouse

            OMButton Zone_ButtonMouseRight = new OMButton(750, 415, 120, 80);
            Zone_ButtonMouseRight.Image = imgPlay;
            Zone_ButtonMouseRight.FocusImage = imgPlayHighlighted;
            Zone_ButtonMouseRight.Name = "Zone_ButtonMouseRight";
            Zone_ButtonMouseRight.Tag = 1;
            Zone_ButtonMouseRight.Transition = eButtonTransition.None;
            Zone_ButtonMouseRight.OnClick += new userInteraction(Zone_ButtonMouseLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonMouseRight);

            OMButton Zone_ButtonMouseLeft = new OMButton(163, 415, 120, 80);
            Zone_ButtonMouseLeft.Image = imgPlay;
            Zone_ButtonMouseLeft.FocusImage = imgPlayHighlighted;
            Zone_ButtonMouseLeft.Orientation = eAngle.FlipHorizontal;
            Zone_ButtonMouseLeft.Name = "Zone_ButtonMouseLeft";
            Zone_ButtonMouseLeft.Transition = eButtonTransition.None;
            Zone_ButtonMouseLeft.Tag = -1;
            Zone_ButtonMouseLeft.OnClick += new userInteraction(Zone_ButtonMouseLeftRight_OnClick);
            panelZone.addControl(Zone_ButtonMouseLeft);

            OMButton Zone_ButtonMouseDetect = new OMButton(875, 415, 120, 80);
            Zone_ButtonMouseDetect.Image = imgBtn;
            Zone_ButtonMouseDetect.FocusImage = imgBtnHighlighted;
            Zone_ButtonMouseDetect.Text = "Detect";
            Zone_ButtonMouseDetect.Font = new Font(Font.GenericSansSerif, 22F);
            Zone_ButtonMouseDetect.Name = "Zone_ButtonMouseDetect";
            Zone_ButtonMouseDetect.Transition = eButtonTransition.None;
            Zone_ButtonMouseDetect.OnClick += new userInteraction(Zone_ButtonMouseDetect_OnClick);
            panelZone.addControl(Zone_ButtonMouseDetect);

            OMLabel Zone_LabelMouse = new OMLabel(-5, 408, 160, 100);
            Zone_LabelMouse.Font = new Font(Font.GenericSansSerif, 26F);
            Zone_LabelMouse.Text = "Mouse";
            Zone_LabelMouse.Name = "Zone_LabelMouse";
            Zone_LabelMouse.TextAlignment = Alignment.CenterRight;
            panelZone.addControl(Zone_LabelMouse);

            OMBasicShape Zone_ShapeMouseCurrent = new OMBasicShape(308, 435, 416, 58);
            Zone_ShapeMouseCurrent.Shape = shapes.RoundedRectangle;
            Zone_ShapeMouseCurrent.Name = "Zone_ShapeMouseCurrent";
            Zone_ShapeMouseCurrent.BorderColor = Color.White;
            Zone_ShapeMouseCurrent.BorderSize = 1;
            panelZone.addControl(Zone_ShapeMouseCurrent);

            OMTextBox Zone_TextBoxMouse = new OMTextBox(291, 415, 450, 50);
            Zone_TextBoxMouse.Flags = textboxFlags.EllipsisEnd;
            Zone_TextBoxMouse.Font = new Font(Font.GenericSansSerif, 21.75F);
            Zone_TextBoxMouse.Name = "Zone_TextBoxMouse";
            panelZone.addControl(Zone_TextBoxMouse);

            OMLabel Zone_LabelMouseCurrent = new OMLabel(291, 469, 450, 20);
            Zone_LabelMouseCurrent.Font = new Font(Font.GenericSansSerif, 18F);
            Zone_LabelMouseCurrent.Text = "current mouse mapping";
            Zone_LabelMouseCurrent.Name = "Zone_LabelMouseCurrent";
            Zone_LabelMouseCurrent.TextAlignment = Alignment.CenterCenter;
            panelZone.addControl(Zone_LabelMouseCurrent);

            #endregion 

            panelZone.Loaded += new PanelEvent(Zone_Entering);
            panelZone.Leaving += new PanelEvent(Zone_Leaving);

            manager.loadPanel(panelZone);

            #endregion
        }

        static void Zone_ButtonMouseDetect_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelZone.Name);
            dialog.Height = 350;
            dialog.Header = "Automatic input detection";
            dialog.Text = "Clicking [Yes] will start automatic mouse detection,\nclick [No] to cancel.\n\nClick the mouse or touch the screen of the zone you want to assign when the zone identificator number is showing.\nDetection will cancel after 10 seconds if no input is detected.";
            dialog.Icon = icons.Question;
            dialog.Button = buttons.Yes | buttons.No;
            if (dialog.ShowMsgBox(screen) == buttons.No)
                return;

            Host.ScreenShowIdentity(SelectedZone-1, true);
            object o;
            Host.getData(eGetData.GetMouseDetectedUnit, "", out o);
            Host.ScreenShowIdentity(SelectedZone-1, false);

            string DetectedUnit = (string)o;

            // Did we detect anything?
            if (DetectedUnit == "")
            {
                dialog = new dialog(PluginName, panelZone.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "No mouse device was detected!\nMake sure that your devices are connected and working in your OS before using OM.\nPlease verify devices and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }
            else
            {   // Device detected, ask user if this is correct
                dialog = new dialog(PluginName, panelZone.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "Detected input from\n" + DetectedUnit + "\nUse this unit for zone " + SelectedZone.ToString() + "?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            // Everything ok, use this new unit
            ((OMLabel)panelZone[screen, "Zone_TextBoxMouse"]).Text = DetectedUnit;
        }

        static void Zone_ButtonKeyboardDetect_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelZone.Name);
            dialog.Height = 350;
            dialog.Header = "Automatic input detection";
            dialog.Text = "Clicking [Yes] will start automatic keyboard detection,\nclick [No] to cancel\n\nPress a key on the keyboard you want to assign when the zone identificator number is showing.\nDetection will cancel after 10 seconds if no input is detected.";
            dialog.Icon = icons.Question;
            dialog.Button = buttons.Yes | buttons.No;
            if (dialog.ShowMsgBox(screen) == buttons.No)
                return;

            Host.ScreenShowIdentity(SelectedZone-1, true);
            object o;
            Host.getData(eGetData.GetKeyboardDetectedUnit, "", out o);
            Host.ScreenShowIdentity(SelectedZone-1, false);

            string DetectedUnit = (string)o;

            // Did we detect anything?
            if (DetectedUnit == "")
            {
                dialog = new dialog(PluginName, panelZone.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "No keyboard device was detected!\nMake sure that your devices are connected and working in your OS before using OM.\nPlease verify devices and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }
            else
            {   // Device detected, ask user if this is correct
                dialog = new dialog(PluginName, panelZone.Name);
                dialog.Header = "Automatic input detection";
                dialog.Text = "Detected input from\n" + DetectedUnit + "\nUse this unit for zone " + SelectedZone.ToString() + "?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            // Everything ok, use this new unit
            ((OMLabel)panelZone[screen, "Zone_TextBoxKeyboard"]).Text = DetectedUnit;
        }

        static void Zone_ButtonAudioUnitTest_OnClick(OMControl sender, int screen)
        {
            dialog dialog = new dialog(PluginName, panelZone.Name);
            dialog.Header = "OpenMobile";
            dialog.Text = "This function is not yet implemented!";
            dialog.Icon = icons.WorkInProgress;
            dialog.Button = buttons.OK;
            dialog.ShowMsgBox(screen).ToString();
        }

        #region Zone controls

        #region Zone Device selections

        static void Zone_ButtonAudioUnitLeftRight_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelZone[screen, "Zone_TextBoxAudioUnit"], AudioDevices);
        }

        static void Zone_ButtonKeyboardLeftRight_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelZone[screen, "Zone_TextBoxKeyboard"],KeyboardDevices);
        }

        static void Zone_ButtonMouseLeftRight_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.Controls.UpDown.UpDownTextBoxControl(screen, (OMButton)sender, (OMTextBox)panelZone[screen, "Zone_TextBoxMouse"], MouseDevices);
        }

        #endregion

        static void Zone_ButtonCancel_OnClick(OMControl sender, int screen)
        {
            // Show warning if settings changed
            MZSettingsChanged = Zone_SettingChanged(screen);
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiZone.Name);
                dialog.Header = "MultiZone settings";
                dialog.Text = "MultiZone settings was modified!\nReturn without saving?";
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

        static void Zone_ButtonSave_OnClick(OMControl sender, int screen)
        {
            // Show warning if settings changed
            MZSettingsChanged = Zone_SettingChanged(screen);
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiZone.Name);
                dialog.Header = "MultiZone settings";
                dialog.Text = "MultiZone settings was modified, please restart OM to activate new settings.";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                Host.execute(eFunction.goBack, screen.ToString());
            }

            // Save new settings
            using (PluginSettings settings = new PluginSettings())
            {
                settings.setSetting("Screen" + SelectedZone.ToString() + ".SoundCard", ((OMLabel)panelZone[screen, "Zone_TextBoxAudioUnit"]).Text);
                settings.setSetting("Screen" + SelectedZone.ToString() + ".Keyboard", ((OMLabel)panelZone[screen, "Zone_TextBoxKeyboard"]).Text);
                settings.setSetting("Screen" + SelectedZone.ToString() + ".Mouse", ((OMLabel)panelZone[screen, "Zone_TextBoxMouse"]).Text);
            }

            Host.execute(eFunction.goBack, screen.ToString());
            Host.execute(eFunction.settingsChanged);
        }

        static void Zone_Leaving(OMPanel sender, int screen)
        {
            SelectedZone = -1;
        }

        static void Zone_Entering(OMPanel sender, int screen)
        {
            // Is current zone outside a valid range?
            if ((SelectedZone <= 0) || (SelectedZone > Host.ScreenCount))
            {
                dialog dialog = new dialog(PluginName, panelZone.Name);
                dialog.Header = "MultiZone settings";
                dialog.Text = "An error occured; Selected zone is outside of valid range!\nPlease go back and try again.";
                dialog.Icon = icons.Error;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                Host.execute(eFunction.goBack, screen.ToString());
                Host.execute(eFunction.goBack, screen.ToString());
                return;
            }

            // Set caption
            ((OMLabel)panelZone[screen, "Zone_LabelCaption"]).Text = "Zone " + SelectedZone.ToString();

            // Get current settings and set textbox values
            using (PluginSettings settings = new PluginSettings())
            {
                ((OMLabel)panelZone[screen, "Zone_TextBoxAudioUnit"]).Text = settings.getSetting("Screen" + SelectedZone.ToString() + ".SoundCard");
                ((OMLabel)panelZone[screen, "Zone_TextBoxKeyboard"]).Text = settings.getSetting("Screen" + SelectedZone.ToString() + ".Keyboard");
                ((OMLabel)panelZone[screen, "Zone_TextBoxMouse"]).Text = settings.getSetting("Screen" + SelectedZone.ToString() + ".Mouse");
            }

            // Get Audio devices
            object o;
            Host.getData(eGetData.GetAudioDevices, "", out o);
            if (o != null)
                AudioDevices = (string[])o;

            // Get keyboard units
            o = new object();
            Host.getData(eGetData.GetKeyboardUnitsForScreen, "", (SelectedZone-1).ToString(), out o);
            if (o != null)
                KeyboardDevices = (string[])o;

            // Get current keyboard mapping
            o = new object();
            Host.getData(eGetData.GetKeyboardCurrentUnitForScreen, "", (SelectedZone-1).ToString(), out o);
            if (o != null)
                ((OMLabel)panelZone[screen, "Zone_LabelKeyboardCurrent"]).Text = (string)o;

            // Get mouse units
            o = new object();
            Host.getData(eGetData.GetMiceUnitsForScreen, "", (SelectedZone-1).ToString(), out o);
            if (o != null)
                MouseDevices = (string[])o;

            // Get current mouse mapping
            o = new object();
            Host.getData(eGetData.GetMiceCurrentUnitForScreen, "", (SelectedZone-1).ToString(), out o);
            if (o != null)
                ((OMLabel)panelZone[screen, "Zone_LabelMouseCurrent"]).Text = (string)o;
        }

        static bool Zone_SettingChanged(int screen)
        {
            // Get current settings and set textbox values
            string CurrentAudioSetting, CurrentKeyboardSetting, CurrentMouseSetting;

            using (PluginSettings settings = new PluginSettings())
            {
                CurrentAudioSetting = settings.getSetting("Screen" + SelectedZone.ToString() + ".SoundCard");
                CurrentKeyboardSetting = settings.getSetting("Screen" + SelectedZone.ToString() + ".Keyboard");
                CurrentMouseSetting = settings.getSetting("Screen" + SelectedZone.ToString() + ".Mouse");
            }

            // Compare to current values
            if (((OMLabel)panelZone[screen, "Zone_TextBoxAudioUnit"]).Text != CurrentAudioSetting)
                return true;
            if (((OMLabel)panelZone[screen, "Zone_TextBoxKeyboard"]).Text != CurrentKeyboardSetting)
                return true;
            if (((OMLabel)panelZone[screen, "Zone_TextBoxMouse"]).Text != CurrentMouseSetting)
                return true;
            return false;
        }

        #endregion

        #region Zone selection controls

        static void ZoneSelection_Button_OnClick(OMControl sender, int screen)
        {
            // Is settings already in action
            if (SelectedZone >= 0)
            {
                dialog dialog = new dialog(PluginName, panelMultiZone.Name);
                dialog.Header = "MultiZone settings";
                dialog.Text = "MultiZone settings is already active on another screen!\nPlease close this first.";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }

            // Set selected multizone
            SelectedZone = (int)((OMButton)sender).Tag;

            // Show panel
            Host.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings", "ZoneSelection");
            Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "Zone");
            Host.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideLeft");
        }

        static void ZoneSelection_ButtonIdentify_OnClick(OMControl sender, int screen)
        {
            Host.ScreenShowIdentity(3000);
        }

        static void panelMultiZone_Leaving(OMPanel sender, int screen)
        {
            // Show warning if settings changed
            if (MZSettingsChanged)
            {
                dialog dialog = new dialog(PluginName, panelMultiZone.Name);
                dialog.Header = "MultiZone settings";
                dialog.Text = "MultiZone settings was modified!\nRestart OM?";
                dialog.Icon = icons.Question;
                dialog.Button = buttons.Yes | buttons.No;
                if (dialog.ShowMsgBox(screen) == buttons.Yes)
                    Host.execute(eFunction.restartProgram);
            }
        }
        #endregion

    }
}
