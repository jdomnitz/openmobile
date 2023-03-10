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
        public class OSK : ICloneable
        {
            static OSK OSK_Default_Keypad;
            static OSK OSK_Default_Keypad_Masked;
            static OSK OSK_Default_KeypadSmall;
            static OSK OSK_Default_KeypadSmall_Masked;
            static OSK OSK_Default_Numpad;
            static OSK OSK_Default_Numpad_Masked;

            public static void Init()
            {
                // Generate default OSK panels
                OSK_Default_Keypad = new OSK("", "", "", OSKInputTypes.Keypad, false);
                OSK_Default_Keypad.GeneratePanel();

                OSK_Default_Keypad_Masked = new OSK("", "", "", OSKInputTypes.Keypad, true);
                OSK_Default_Keypad_Masked.GeneratePanel();

                OSK_Default_KeypadSmall = new OSK("", "", "", OSKInputTypes.KeypadSmall, false);
                OSK_Default_KeypadSmall.GeneratePanel();

                OSK_Default_KeypadSmall_Masked = new OSK("", "", "", OSKInputTypes.KeypadSmall, true);
                OSK_Default_KeypadSmall_Masked.GeneratePanel();

                OSK_Default_Numpad = new OSK("", "", "", OSKInputTypes.Numpad, false);
                OSK_Default_Numpad.GeneratePanel();

                OSK_Default_Numpad_Masked = new OSK("", "", "", OSKInputTypes.Numpad, true);
                OSK_Default_Numpad_Masked.GeneratePanel();
            }

            public static string ShowDefaultOSK(int screen, string Text, string HelpText, string Header, OSKInputTypes Style, bool MaskInput, OMTextBox textBoxTarget = null)
            {
                if (Style == OSKInputTypes.None)
                    return "";

                //OMTextBox"OSK_TextBox_Text"

                OSK osk = null;

                switch (Style)
                {
                    case OSKInputTypes.Keypad:
                        {
                            if (MaskInput)
                                osk = (OSK)OSK_Default_Keypad_Masked.Clone();
                            else
                                osk = (OSK)OSK_Default_Keypad.Clone();
                        }
                        break;
                    case OSKInputTypes.KeypadSmall:
                        {
                            if (MaskInput)
                                osk = (OSK)OSK_Default_KeypadSmall_Masked.Clone();
                            else
                                osk = (OSK)OSK_Default_KeypadSmall.Clone();
                        }
                        break;
                    case OSKInputTypes.Numpad:
                        {
                            if (MaskInput)
                                osk = (OSK)OSK_Default_Numpad_Masked.Clone();
                            else
                                osk = (OSK)OSK_Default_Numpad.Clone();
                        }
                        break;
                }

                // Configure configdata
                OSKData oskData = (OSKData)osk.Panel.Tag;

                // Configure text and other items
                OMTextBox txt = (OMTextBox)osk.Panel.Controls.Find(x => x.Name == "OSK_TextBox_Text");
                if (txt != null)
                {
                    if (!MaskInput)
                    {
                        txt.Tag = Text;
                        txt.Text = Text;
                    }
                    oskData.Text = Text;
                }
                OMLabel lbl = (OMLabel)osk.Panel.Controls.Find(x => x.Name == "OSK_Label_HelpText");
                if (lbl != null)
                {
                    if (String.IsNullOrWhiteSpace(Text))
                        lbl.Text = HelpText;
                    oskData.HelpText = HelpText;
                }
                lbl = (OMLabel)osk.Panel.Controls.Find(x => x.Name == "OSK_Label_Header");
                if (lbl != null)
                {
                    lbl.Tag = textBoxTarget;
                    lbl.Text = Header;
                    oskData.Header = Header;
                }

                return osk.ShowPreloadedOSK(screen);
            }

            public class OSKOnKeyPressData
            {
                public OMPanel Panel { get; set; }
                public eKeypressType Type { get; set; }
                public KeyboardKeyEventArgs Arg { get; set; }
                public int Screen { get; set; }
                public bool KeyHandled { get; set; }
            }

            public class OSKOnGestureData
            {
                public OMPanel Panel { get; set; }
                public int Screen { get; set; }
                public string Character { get; set; }
            }

            public class OSKData : ICloneable
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

                /// <summary>
                /// The opacity level of the background (0 - 255)
                /// </summary>
                public byte BackgroundOpacity { get; set; }

                /// <summary>
                /// The Y placement of the OSK
                /// </summary>
                public int Left
                {
                    get
                    {
                        return this._Left;
                    }
                    set
                    {
                        if (this._Left != value)
                        {
                            this._Left = value;
                        }
                    }
                }
                private int _Left=-1;

                /// <summary>
                /// The X placement of the OSK
                /// </summary>
                public int Top
                {
                    get
                    {
                        return this._Top;
                    }
                    set
                    {
                        if (this._Top != value)
                        {
                            this._Top = value;
                        }
                    }
                }
                private int _Top = -1;        

                #region ICloneable Members

                public object Clone()
                {
                    return this.MemberwiseClone();
                }

                #endregion
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

            public OSK()
            {
            }
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

            private void CreatePanel()
            {
                // Create panel
                Panel = new OMPanel("");

                // Pack paneldata into tag property
                Panel.Tag = _ConfigData;

                if (!BuiltInComponents.Host.sendMessage<OMPanel>(Handler, "OpenMobile.helperFunctions.OSK", "OSK", ref Panel))
                {   // Log this error to the debug log
                    BuiltInComponents.Host.DebugMsg("Unable to get OSK panel, plugin " + Handler + " not available");
                    return;
                }

            }

            void CancelButton_OnClick(OMControl sender, int screen)
            {
                //Result = ((OMTextBox)sender.Parent["OSK_TextBox_Text"]).Text;
                Result = ((OSKData)sender.Parent.Tag).Text;
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
                    Result = ((OSKData)sender.Parent.Tag).Text;
                CloseOSK.Set();
            }

            void txtBox_OnTextChange(OMControl sender, int screen)
            {
                OMLabel lbl = sender.Parent["OSK_Label_Header"] as OMLabel;
                if (lbl != null)
                {
                    if (lbl.Tag != null && (lbl.Tag is OMTextBox))
                    {
                        OMTextBox textBoxTarget = lbl.Tag as OMTextBox;
                        OMTextBox txt = sender.Parent["OSK_TextBox_Text"] as OMTextBox;
                        if (txt != null)
                        {
                            if (String.IsNullOrEmpty(txt.Tag as string))
                                textBoxTarget.Text = txt.Text;
                            else
                                textBoxTarget.Text = txt.Tag as string;
                        }
                        else
                            textBoxTarget.Text = ((OSKData)sender.Parent.Tag).Text;
                    }
                }
            }

            /// <summary>
            /// Shows a preloaded OSK (fast)
            /// </summary>
            /// <param name="screen"></param>
            /// <returns></returns>
            public string ShowPreloadedOSK(int screen)
            {
                // Initialize the panel
                if (!BuiltInComponents.Host.sendMessage<OMPanel>(Handler, "OpenMobile.helperFunctions.OSK", "init", ref Panel))
                {   // Log this error to the debug log
                    BuiltInComponents.Host.DebugMsg("Unable to get OSK panel, plugin " + Handler + " not available");
                    return "";
                }

                return ShowOSK(screen, true);
            }

            /// <summary>
            /// Shows the OSK and returns the text string from the OSK
            /// </summary>
            /// <param name="screen"></param>
            /// <returns></returns>
            public string ShowOSK(int screen)
            {
                return ShowOSK(screen, false);
            }
            /// <summary>
            /// Shows the OSK and returns the text string from the OSK
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="UsePreloadedPanel"></param>
            /// <returns></returns>
            public string ShowOSK(int screen, bool UsePreloadedPanel)
            {
                // Save screen manager
                DateTime start = DateTime.Now;
                Screen = screen;


                // Create panel
                if (!UsePreloadedPanel)
                    CreatePanel();

                // Set panel name
                PanelName = Panel.Name = String.Format("{0}_{1}", Handler, Panel.GetHashCode());

                // Attach events
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
                        object control = Panel.getControl(i);
                        if (control is OMButton)
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
                        else if (control is OMTextBox)
                        {
                            OMTextBox txtBox = Panel.getControl(i) as OMTextBox;
                            if (txtBox != null)
                            {
                                if (txtBox.Name == "OSK_TextBox_Text")
                                    txtBox.OnTextChange += new userInteraction(txtBox_OnTextChange);
                            }
                        }
                    }
                }

                // loadpanel
                BuiltInComponents.Panels.loadSinglePanel(Panel, screen, false);

                CloseOSK.Reset();

                // Connect to system events (to detect "goback" event)
                SystemEvent SysEv = new SystemEvent(theHost_OnSystemEvent);
                KeyboardEvent KeyEv = new KeyboardEvent(Host_OnKeyPress);
                GestureEvent GestureEv = new GestureEvent(Host_OnGesture);
                BuiltInComponents.Host.OnSystemEvent += SysEv;
                BuiltInComponents.Host.OnKeyPress += KeyEv;
                BuiltInComponents.Host.OnGesture += GestureEv;

                // Show the panel
                if (BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen, "", Panel.Name) == false)
                    return "";
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen);

                // Wait for close
                System.Diagnostics.Debug.WriteLine("OSK Showed in :" + (DateTime.Now - start).TotalMilliseconds.ToString() + "ms");
                CloseOSK.WaitOne();

                // Disconnect events
                BuiltInComponents.Host.OnSystemEvent -= SysEv;
                BuiltInComponents.Host.OnKeyPress -= KeyEv;
                BuiltInComponents.Host.OnGesture -= GestureEv;

                // Remove menu and clean up
                BuiltInComponents.Host.execute(eFunction.goBack, screen);
                BuiltInComponents.Panels.unloadPanel(Panel.Name);

                return Result;
            }


            public void GeneratePanel()
            {
                CreatePanel();
            }

            bool Host_OnGesture(int screen, string character, string pluginName, string panelName, ref bool handled)
            {
                if (panelName.Contains(PanelName))
                {
                    // Pass keypress on to the OSK plugin along with the current panel
                    OSKOnGestureData Data = new OSKOnGestureData();
                    Data.Character = character;
                    Data.Screen = screen;
                    Data.Panel = BuiltInComponents.Panels[Screen, PanelName];
                    if (!BuiltInComponents.Host.sendMessage<OSKOnGestureData>(Handler, "OpenMobile.helperFunctions.OSK", "ongesture", ref Data))
                    {   // Log this error to the debug log
                        BuiltInComponents.Host.DebugMsg("Unable to send gesture event to OSK, plugin " + Handler + " not available");
                        return false;
                    }
                    return true;
                }
                return false;
            }

            bool Host_OnKeyPress(eKeypressType type, KeyboardKeyEventArgs arg, ref bool handled)
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
                if (Data.KeyHandled)
                    handled = true;
                return Data.KeyHandled;
            }
            

            /// <summary>
            /// Close the dialog
            /// </summary>
            public void Close()
            {
                Result = "";
                CloseOSK.Set();
            }

            void theHost_OnSystemEvent(eFunction function, object[] args)
            {
                if (function == eFunction.goBack)
                {
                    if (OpenMobile.helperFunctions.Params.IsParamsValid(args, 3))
                    {
                        if (OpenMobile.helperFunctions.Params.GetParam<string>(args,2) == PanelName)
                        {
                            Result = "";
                            CloseOSK.Set();
                        }
                    }
                }
            }

            #region ICloneable Members

            public object Clone()
            {
                OSK newOSK = new OSK();
                newOSK.Panel = this.Panel.Clone();
                newOSK.PanelName = this.PanelName;
                newOSK.Panel.Tag = this._ConfigData.Clone();
                return newOSK;
            }

            #endregion
        }
}
