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
using System.Collections.Generic;
using System.Threading;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using OpenMobile.Input;
using OpenMobile.Framework;

namespace OpenMobile.helperFunctions
{
        public class OSK
        {
            public class OSKOnKeyPressData
            {
                public OMPanel Panel { get; set; }
                public eKeypressType Type { get; set; }
                public KeyboardKeyEventArgs Arg { get; set; }
                public int Screen { get; set; }
            }

            public class OSKData
            {
                public OSKData()
                {   // OSK Defaults
                    Text = "";
                    HelpText = "";
                    Header = "";
                    Style = OSKInputTypes.Keypad;
                    MaskInput = false;
                    BackgroundOpacity = 215;
                }
                public string Header { get; set; }
                public string HelpText { get; set; }
                public string Text { get; set; }
                public OSKInputTypes Style { get; set; }
                public bool MaskInput { get; set; }
                public ScreenManager Manager { get; set; }

                /// <summary>
                /// The opacity level of the background (0 - 255)
                /// </summary>
                public byte BackgroundOpacity { get; set; }
            }

            private string Handler = "OMOsk2";
            private string PanelName = "";
            private OSKData _ConfigData = new OSKData();
            private OMPanel Panel = null;
            private EventWaitHandle CloseOSK = new EventWaitHandle(false, EventResetMode.ManualReset);
            private string Result = "";
            private int Screen;

            #region Properties

            /// <summary>
            /// Header
            /// </summary>
            public string Header
            {
                get
                {
                    return _ConfigData.Header;
                }
                set
                {
                    _ConfigData.Header = value;
                }
            }

            /// <summary>
            /// Help text
            /// </summary>
            public string HelpText
            {
                get
                {
                    return _ConfigData.HelpText;
                }
                set
                {
                    _ConfigData.HelpText = value;
                }
            }

            /// <summary>
            /// Text (Initial value)
            /// </summary>
            public string Text
            {
                get
                {
                    return _ConfigData.Text;
                }
                set
                {
                    _ConfigData.Text = value;
                }
            }

             /// <summary>
            /// Style of OSK
            /// </summary>
            public OSKInputTypes Style
            {
                get
                {
                    return _ConfigData.Style;
                }
                set
                {
                    _ConfigData.Style = value;
                }
            }

            /// <summary>
            /// Mask or show text input
            /// </summary>
            public bool MaskInput
            {
                get
                {
                    return _ConfigData.MaskInput;
                }
                set
                {
                    _ConfigData.MaskInput = value;
                }
            }

            /// <summary>
            /// The opacity level of the background (0 - 255)
            /// </summary>
            public byte BackgroundOpacity
            {
                get
                {
                    return _ConfigData.BackgroundOpacity;
                }
                set
                {
                    _ConfigData.BackgroundOpacity = value;
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes the OSK
            /// </summary>
            /// <param name="Header"></param>
            public OSK(string Text, string HelpText, string Header, OSKInputTypes Style, bool MaskInput)
            {
                this.Text = Text;
                this.HelpText = HelpText;
                this.Header = Header;
                this.Style = Style;
                this.MaskInput = MaskInput;
            }
            /// <summary>
            /// Initializes the OSK
            /// </summary>
            /// <param name="Header"></param>
            public OSK(string Text, string HelpText, string Header, OSKInputTypes Style, bool MaskInput, string Handler)
            {
                this.Text = Text;
                this.HelpText = HelpText;
                this.Header = Header;
                this.Style = Style;
                this.MaskInput = MaskInput;
                this.Handler = Handler;
            }

            #endregion


            private void CreatePanel(int screen)
            {
                // Create panel
                Panel = new OMPanel(this.GetHashCode().ToString());

                // Set panel name
                PanelName = Panel.Name;

                // Pack paneldata into tag property
                Panel.Tag = _ConfigData;

                if (!BuiltInComponents.Host.sendMessage<OMPanel>(Handler, "OpenMobile.helperFunctions.OSK", "OSK", ref Panel))
                {   // Log this error to the debug log
                    BuiltInComponents.Host.DebugMsg("Unable to get OSK panel, plugin " + Handler + " not available");
                    return;
                }

                if (Panel != null)
                {
                    //Panel.Priority = ePriority.High;
                    /* Items has to be named according to this:
                     *  OSK_Button_OK       : OK button
                     *  OSK_Button_?        : OSK keys
                     *  OSK_TextBox_Text    : Text input box
                    */

                    // Attach events
                    for (int i = 0; i < Panel.controlCount; i++)
                    {
                        OMButton btn = Panel.getControl(i) as OMButton;
                        if (btn != null)
                        {
                            if (btn.Name == "OSK_Button_Cancel")
                                btn.OnClick += new userInteraction(CancelButton_OnClick);
                            else if (btn.Name == "OSK_Button_OK")
                                btn.OnClick += new userInteraction(OKButton_OnClick);
                        }
                    }
                }
            }

            void CancelButton_OnClick(OMControl sender, int screen)
            {
                //Result = ((OMTextBox)sender.Parent["OSK_TextBox_Text"]).Text;
                Result = _ConfigData.Text;
                CloseOSK.Set();
            }

            void OKButton_OnClick(OMControl sender, int screen)
            {
                OMTextBox txt = sender.Parent["OSK_TextBox_Text"] as OMTextBox;
                if (txt != null)
                {
                    if (String.IsNullOrEmpty(txt.Tag as string))
                        Result = txt.Text;
                    else
                        Result = txt.Tag as string;
                }
                else
                    Result = _ConfigData.Text;
                CloseOSK.Set();
            }

            /// <summary>
            /// Shows the menu and returns the zero based index (if not changed by the returntype property) of the selected menu option
            /// <para>A negative number indicates no selection</para>
            /// </summary>
            /// <param name="screen"></param>
            /// <returns></returns>
            public string ShowOSK(int screen)
            {
                // Save screen manager
                _ConfigData.Manager = BuiltInComponents.Panels;
                Screen = screen;
                
                CreatePanel(screen);
                CloseOSK.Reset();

                // loadpanel
                BuiltInComponents.Panels.loadPanel(Panel);

                // Connect to system events (to detect "goback" event)
                SystemEvent SysEv = new SystemEvent(theHost_OnSystemEvent);
                KeyboardEvent KeyEv = new KeyboardEvent(Host_OnKeyPress);
                BuiltInComponents.Host.OnSystemEvent += SysEv;
                BuiltInComponents.Host.OnKeyPress += KeyEv;

                // Show the panel
                if (BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "", Panel.Name) == false)
                    return "";
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());

                // Wait for close
                CloseOSK.WaitOne();

                // Disconnect events
                BuiltInComponents.Host.OnSystemEvent -= SysEv;
                BuiltInComponents.Host.OnKeyPress -= KeyEv;

                // Remove menu and clean up
                //BuiltInComponents.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), "", Panel.Name);
                //BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                BuiltInComponents.Host.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                BuiltInComponents.Panels.unloadPanel(Panel.Name);

                return Result;
            }

            bool Host_OnKeyPress(eKeypressType type, KeyboardKeyEventArgs arg)
            {
                // Make sure that we only pass the keypresses that belongs to the correct screen
                if (arg.Screen != Screen)
                    return false;
                
                // Pass keypress on to the OSK plugin along with the current panel
                OSKOnKeyPressData Data = new OSKOnKeyPressData();
                Data.Arg = arg;
                Data.Type = type;
                Data.Screen = arg.Screen;
                Data.Panel = BuiltInComponents.Panels[Screen, PanelName];
                if (!BuiltInComponents.Host.sendMessage<OSKOnKeyPressData>(Handler, "OpenMobile.helperFunctions.OSK", "onkeypress", ref Data))
                {   // Log this error to the debug log
                    BuiltInComponents.Host.DebugMsg("Unable to send keypress event to OSK, plugin " + Handler + " not available");
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Close the dialog
            /// </summary>
            public void Close()
            {
                Result = "";
                CloseOSK.Set();
            }

            void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
            {
                if (function == eFunction.goBack)
                {
                    if (arg3 == PanelName)
                    {
                        Result = "";
                        CloseOSK.Set();
                    }
                }
            }
        }
}