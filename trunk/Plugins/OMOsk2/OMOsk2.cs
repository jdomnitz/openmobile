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
using System.IO;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Media;
using OpenMobile.Threading;
using System.Threading;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using System.Collections.Generic;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.Input;
using OpenMobile.helperFunctions.Graphics;

namespace OMOsk2
{
    [SkinIcon("*@")]
    [PluginLevel(PluginLevels.System)]
    public class OMOsk2 : IHighLevel
    {
        const int CharSets = 3;

        /// <summary>
        /// Thy key definition class (local to this OSK implementation)
        /// </summary>
        class KeyInfo
        {
            public string[] Symbol = new string[CharSets];
            public string[] Icon = new string[CharSets];
            public imageItem[] Image = new imageItem[CharSets];
            public bool[] GlowState = new bool[CharSets];
            public Key KeyCode = Key.Unknown; 
            public int Width = 85;
            public int Height = 85;

            #region Constructors

            public KeyInfo() { }
            public KeyInfo(string Symbol1, string Symbol2, string Symbol3, Key KeyCode)
                : this(new string[CharSets] { Symbol1, Symbol2, Symbol3 }, KeyCode, 85, 85) { }
            public KeyInfo(string[] Symbol, Key KeyCode)
                : this(Symbol, KeyCode, 85, 85) { }
            public KeyInfo(string Symbol1, string Symbol2, string Symbol3, Key KeyCode, int Width)
                : this(new string[CharSets] { Symbol1, Symbol2, Symbol3 }, KeyCode, Width, 85) { }
            public KeyInfo(string[] Symbol, Key KeyCode, int Width)
                : this(Symbol, KeyCode, Width, 85) {}
            public KeyInfo(string Symbol1, string Symbol2, string Symbol3, Key KeyCode, int Width, int Height)
                : this(new string[CharSets] { Symbol1, Symbol2, Symbol3 }, KeyCode, Width, Height) { }
            public KeyInfo(string[] Symbol, Key KeyCode, int Width, int Height)
            {
                this.Symbol = Symbol;
                this.KeyCode = KeyCode;
                this.Width = Width;
                this.Height = Height;
            }

            #endregion

            public override string ToString()
            {
                string Result = "";
                foreach (string s in Symbol)
                    Result += s + "|";
                return Result;
            }
        }

        OpenMobile.Timer[] MaskInputTimer = null;

        // Character selection
        int CharSet = 0;

        #region KeyPad definitions

        KeyInfo[] KeyPadKeys = new KeyInfo[]
            {
                // Row 1
                new KeyInfo("1","1","!", Key.Number1),
                new KeyInfo("2","2","@", Key.Number2),
                new KeyInfo("3","3","#", Key.Number3),
                new KeyInfo("4","4","$", Key.Number4),
                new KeyInfo("5","5","%", Key.Number5),
                new KeyInfo("6","6","^", Key.Number6),
                new KeyInfo("7","7","&", Key.Number7),
                new KeyInfo("8","8","*", Key.Number8),
                new KeyInfo("9","9","(", Key.Number9),
                new KeyInfo("0","0",")", Key.Number0),
                new KeyInfo("Del","Del","Del", Key.Back, 100),

                // Row 2
                new KeyInfo("q","Q","-", Key.Q),
                new KeyInfo("w","W","=", Key.W),
                new KeyInfo("e","E","[", Key.E),
                new KeyInfo("r","R","]", Key.R),
                new KeyInfo("t","T","\\", Key.T),
                new KeyInfo("y","Y",";", Key.Y),
                new KeyInfo("u","U","'", Key.U),
                new KeyInfo("i","I",",", Key.I),
                new KeyInfo("o","O","/", Key.O), 
                new KeyInfo("p","P","`", Key.P),

                // Row 3
                new KeyInfo("a","A","_", Key.A),
                new KeyInfo("s","S","+", Key.S),
                new KeyInfo("d","D","{", Key.D),
                new KeyInfo("f","F","}", Key.F),
                new KeyInfo("g","G","|", Key.G),
                new KeyInfo("h","H",":", Key.H),
                new KeyInfo("j","J","\"", Key.J),
                new KeyInfo("k","K","<", Key.K),
                new KeyInfo("l","L",">", Key.L), 

                // Row 4
                new KeyInfo("","","", Key.ShiftLeft, 110) {Icon = new string[CharSets] {"5","5",""}, GlowState = new bool[CharSets] {false, true, false}},
                new KeyInfo("z","Z","?", Key.Z),
                new KeyInfo("x","D","~", Key.X),
                new KeyInfo("c","C","|", Key.C),
                new KeyInfo("v","V","§", Key.V),
                new KeyInfo("b","B","¨", Key.B),
                new KeyInfo("n","N","~", Key.N),
                new KeyInfo("m","M","´", Key.M),
                new KeyInfo(".",".",",", Key.Period), 
                new KeyInfo("","","", Key.ShiftRight, 110) {Icon = new string[CharSets] {"5","5",""}, GlowState = new bool[CharSets] {false, true, false}},

                 // Row 5
                new KeyInfo("Cancel","Cancel","Cancel", Key.Escape, 199),
                new KeyInfo("Sym","Sym","ABC", Key.LAlt, 110),
                new KeyInfo("Space","Space","Space", Key.Space, 300),
                new KeyInfo("Sym","Sym","ABC", Key.RAlt, 110),
                new KeyInfo("Enter","Enter","Enter", Key.Enter, 199), 
            };

        #endregion

        #region NumPad definitions

        KeyInfo[] NumPadKeys = new KeyInfo[]
            {
                // Row 1
                new KeyInfo("1","1","/", Key.Number1, 95),
                new KeyInfo("2","2","*", Key.Number2, 95),
                new KeyInfo("3","3","-", Key.Number3, 95),
                new KeyInfo("Del","Del","Del", Key.Back, 95),

                // Row 2
                new KeyInfo("4","4","+", Key.Number4, 95),
                new KeyInfo("5","5","$", Key.Number5, 95),
                new KeyInfo("6","6","£", Key.Number6, 95),
                new KeyInfo("","","", Key.Unknown, 95),

                // Row 3
                new KeyInfo("7","7",@"\", Key.Number7, 95),
                new KeyInfo("8","8","+", Key.Number8, 95),
                new KeyInfo("9","9","%", Key.Number9, 95),
                new KeyInfo("","","", Key.Unknown, 95),

                 // Row 4
                new KeyInfo("0","0","#", Key.Number2, 198),
                new KeyInfo(".",".",",", Key.Period, 95), 
                new KeyInfo("Sym","Sym","123", Key.LAlt, 95),
               
                // Row 5
                new KeyInfo("Cancel","Cancel","Cancel", Key.Escape, 198),
                new KeyInfo("Enter","Enter","Enter", Key.Enter, 198), 

            };

        #endregion

        /// <summary>
        /// Set the decimal sign on the main page of the keypad to the correct one for the current localization
        /// </summary>
        private void SetDecimalDelimiter(KeyInfo[] Keys)
        {
            // Keypad decimal is located at index 38
            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ",")
            {
                foreach (KeyInfo key in Keys)
                    if (key.KeyCode == Key.Period)
                        key.Symbol = new string[CharSets] { ",", ",", "." };
            }

        }

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return null;
        }
        public Settings loadSettings()
        {
            return null;
        }

        #region Attributes
        public string displayName
        {
            get { return "OMOsk2"; }
        }
        public string authorName
        {
            get { return "Bjørn Morten Orderløkken"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMOsk2"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "On screen keyboard"; }
        }
        #endregion

        public bool incomingMessage(string message, string source)
        {
            return false; // No action
        }

        private int GetTotaltWidth(KeyInfo[] Keys, int StartKey, int EndKey, int Spacing)
        {
            int TotalWidth = 0;
            for (int i = StartKey; i < EndKey; i++)
                TotalWidth += Keys[i].Width + Spacing;
            return TotalWidth;
        }
        private int GetCenteredStartPoint(KeyInfo[] Keys, int StartKey, int EndKey, int Spacing)
        {
            return 500 - (GetTotaltWidth(Keys, StartKey, EndKey, Spacing) / 2);
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // What should we do?
            switch (message.ToLower())
            {
                case "init":
                    {
                        //CharSet = 0;
                        //UpdateButtonGraphics(Panel, Screen, CharSet);
                    }
                    break;

                case "onkeypress":
                    {   // Keypress events are sent from the OSK helperfunctions to provide the correct panel for updating controls
                        OSK.OSKOnKeyPressData KeyPressData = data as OSK.OSKOnKeyPressData; // Convert input data to local data object
                        KeyPressData.KeyHandled = OSK_OnKeyPress(KeyPressData.Panel, KeyPressData.Screen, KeyPressData.Type, KeyPressData.Arg);
                        // Return data
                        data = (T)Convert.ChangeType(KeyPressData, typeof(T));
                    }
                    break;

                case "ongesture":
                    {   // Gesture events are sent from the OSK helperfunctions to provide the correct panel for updating controls
                        OSK.OSKOnGestureData GestureData = data as OSK.OSKOnGestureData; // Convert input data to local data object
                        OSK_OnGesture(GestureData.Panel, GestureData.Screen, GestureData.Character);
                        // No return data
                    }
                    break;

                case "osk":
                    {
                        // Convert input data to local data object
                        OMPanel Panel = data as OMPanel;

                        // Extract Menu data
                        OSK.OSKData DT = (OSK.OSKData)Panel.Tag;

                        /* Items has to be named according to this:
                         *  OSK_Button_OK       : OK button
                         *  OSK_Button_?        : OSK keys
                         *  OSK_TextBox_Text    : Text input box
                        */

                        #region Create osk

                        // Reset charset to default
                        CharSet = 0;

                        OMBasicShape Shape_AccessBlock2 = new OMBasicShape("Shape_AccessBlock2", 0, 0, 1000, 600);
                        Shape_AccessBlock2.Shape = shapes.Rectangle;
                        Shape_AccessBlock2.FillColor = Color.FromArgb(DT.BackgroundOpacity, Color.Black);
                        if (DT.MaskInput)
                            Shape_AccessBlock2.Tag = DT.MaskInput;
                        Panel.addControl(Shape_AccessBlock2);

                        OMLabel OSK_Label_Header = new OMLabel("OSK_Label_Header", 0, 0, 1000, 40);
                        OSK_Label_Header.Font = new Font(Font.GenericSansSerif, 20F);
                        OSK_Label_Header.Text = DT.Header;
                        Panel.addControl(OSK_Label_Header);

                        OMTextBox OSK_TextBox_Text = new OMTextBox("OSK_TextBox_Text", 17, 40, 966, 60);
                        OSK_TextBox_Text.Font = new Font(Font.GenericSansSerif, 30F);
                        if (DT.MaskInput)
                        {
                            if (!String.IsNullOrEmpty(DT.Text))
                                OSK_TextBox_Text.Text = new string('*', DT.Text.Length);
                        }
                        else
                        {
                            OSK_TextBox_Text.Text = DT.Text;
                        }
                        OSK_TextBox_Text.Tag = DT.Text;
                        Panel.addControl(OSK_TextBox_Text);

                        OMLabel OSK_Label_HelpText = new OMLabel("OSK_Label_HelpText", 17, 40, 966, 60);
                        OSK_Label_HelpText.Font = new Font(Font.GenericSansSerif, 30F);
                        OSK_Label_HelpText.Tag = DT.HelpText;
                        if (String.IsNullOrEmpty(OSK_TextBox_Text.Text))
                            OSK_Label_HelpText.Text = DT.HelpText;
                        OSK_Label_HelpText.Color = Color.Gray;
                        OSK_Label_HelpText.NoUserInteraction = true;
                        Panel.addControl(OSK_Label_HelpText);

                        switch (DT.Style)
                        {
                            case OSKInputTypes.Keypad:
                                {
                                    #region KeyPad

                                    // Set the correct decimal delimiter
                                    SetDecimalDelimiter(KeyPadKeys);

                                    int SpacingX = 4;
                                    int SpacingY = 4;

                                    // Place controls on row 1
                                    int Top = 120;
                                    int KeyCount = 9;
                                    int Left = 0;

                                    int CurrentKey = 0;

                                    imageItem imgBack = new imageItem();
                                    imageItem imgFocus = new imageItem();

                                    // Rows
                                    for (int Row = 0; Row < 5; Row++)
                                    {
                                        // Set keys pr row
                                        if (Row == 0)
                                            KeyCount = 11;
                                        else if (Row == 1)
                                            KeyCount = 10;
                                        else if (Row == 2)
                                            KeyCount = 9;
                                        else if (Row == 3)
                                            KeyCount = 10;
                                        else if (Row == 4)
                                            KeyCount = 5;

                                        Left = GetCenteredStartPoint(KeyPadKeys, CurrentKey, CurrentKey + KeyCount, SpacingX);

                                        // Add keys to row
                                        for (int i = 0; i < KeyCount; i++)
                                        {
                                            OMButton btn = new OMButton(String.Format("OSK_Button_{0}", CurrentKey), Left, Top, KeyPadKeys[CurrentKey].Width, KeyPadKeys[CurrentKey].Height);

                                            // Set background image
                                            if (imgBack.image == null || imgBack.image.Width != KeyPadKeys[CurrentKey].Width || imgBack.image.Height != KeyPadKeys[CurrentKey].Height)
                                                imgBack = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(KeyPadKeys[CurrentKey].Width, KeyPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonBackground));
                                            btn.Image = imgBack;
                                            // Set focus image
                                            if (imgFocus.image == null || imgFocus.image.Width != KeyPadKeys[CurrentKey].Width || imgFocus.image.Height != KeyPadKeys[CurrentKey].Height)
                                                imgFocus = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(KeyPadKeys[CurrentKey].Width, KeyPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonBackgroundFocused));
                                            btn.FocusImage = imgFocus;

                                            // Generate overlay images (save it to KeyInfo)
                                            for (int i2 = 0; i2 < KeyPadKeys[CurrentKey].Image.Length; i2++)
                                            {
                                                if (KeyPadKeys[CurrentKey].GlowState[i2])
                                                    KeyPadKeys[CurrentKey].Image[i2] = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(KeyPadKeys[CurrentKey].Width, KeyPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonForegroundFocused, KeyPadKeys[CurrentKey].Icon[i2], KeyPadKeys[CurrentKey].Symbol[i2]));
                                                else
                                                    KeyPadKeys[CurrentKey].Image[i2] = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(KeyPadKeys[CurrentKey].Width, KeyPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonForeground, KeyPadKeys[CurrentKey].Icon[i2], KeyPadKeys[CurrentKey].Symbol[i2]));
                                            }

                                            // Set overlay image for button
                                            btn.OverlayImage = KeyPadKeys[CurrentKey].Image[CharSet];

                                            btn.Tag = KeyPadKeys[CurrentKey];
                                            btn.Transition = eButtonTransition.None;
                                            btn.OnClick += new userInteraction(btn_OnClick);
                                            btn.OnHoldClick += new userInteraction(btn_OnHoldClick);

                                            // Set special names (OK and cancel buttons)
                                            if (KeyPadKeys[CurrentKey].KeyCode == Key.Escape)
                                                btn.Name = "OSK_Button_Cancel";
                                            else if (KeyPadKeys[CurrentKey].KeyCode == Key.Enter)
                                                btn.Name = "OSK_Button_OK";

                                            Panel.addControl(btn);

                                            Left += KeyPadKeys[CurrentKey].Width + SpacingX;
                                            CurrentKey++;
                                        }
                                        if (CurrentKey < KeyPadKeys.Length)
                                            Top += KeyPadKeys[CurrentKey].Height + SpacingY;
                                    }

                                    #endregion
                                }
                                break;
                            case OSKInputTypes.Numpad:
                                {
                                    #region NumPad

                                    // Set the correct decimal delimiter
                                    SetDecimalDelimiter(NumPadKeys);

                                    int SpacingX = 8;
                                    int SpacingY = 4;

                                    // Place controls on row 1
                                    int Top = 120;
                                    int KeyCount = 3;
                                    int Left = 0;

                                    int CurrentKey = 0;

                                    imageItem imgBack = new imageItem();
                                    imageItem imgFocus = new imageItem();
                                    
                                    // Rows
                                    for (int Row = 0; Row < 5; Row++)
                                    {
                                        // Set keys pr row
                                        if (Row == 0)
                                            KeyCount = 4;
                                        else if (Row == 1)
                                            KeyCount = 4;
                                        else if (Row == 2)
                                            KeyCount = 4;
                                        else if (Row == 3)
                                            KeyCount = 3;
                                        else if (Row == 4)
                                            KeyCount = 2;

                                        Left = GetCenteredStartPoint(NumPadKeys, CurrentKey, CurrentKey + KeyCount, SpacingX);

                                        // Add keys to row
                                        for (int i = 0; i < KeyCount; i++)
                                        {
                                            //OMButton btn = DefaultControls.GetButton(String.Format("OSK_Button_{0}", CurrentKey), Left, Top, Keys[CurrentKey].Width, Keys[CurrentKey].Height, Keys[CurrentKey].Icon[CharSet], Keys[CurrentKey].Symbol[CharSet]);
                                            if (!String.IsNullOrEmpty(NumPadKeys[CurrentKey].Symbol[0]) || !String.IsNullOrEmpty(NumPadKeys[CurrentKey].Symbol[1]) || !String.IsNullOrEmpty(NumPadKeys[CurrentKey].Symbol[2]))
                                            {
                                                OMButton btn = new OMButton(String.Format("OSK_Button_{0}", CurrentKey), Left, Top, NumPadKeys[CurrentKey].Width, NumPadKeys[CurrentKey].Height);

                                                // Set background image
                                                if (imgBack.image == null || imgBack.image.Width != NumPadKeys[CurrentKey].Width || imgBack.image.Height != NumPadKeys[CurrentKey].Height)
                                                    imgBack = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(NumPadKeys[CurrentKey].Width, NumPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonBackground));
                                                btn.Image = imgBack;
                                                // Set focus image
                                                if (imgFocus.image == null || imgFocus.image.Width != NumPadKeys[CurrentKey].Width || imgFocus.image.Height != NumPadKeys[CurrentKey].Height)
                                                    imgFocus = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(NumPadKeys[CurrentKey].Width, NumPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonBackgroundFocused));
                                                btn.FocusImage = imgFocus;

                                                // Generate overlay images (save it to KeyInfo)
                                                for (int i2 = 0; i2 < NumPadKeys[CurrentKey].Image.Length; i2++)
                                                {
                                                    if (NumPadKeys[CurrentKey].GlowState[i2])
                                                        NumPadKeys[CurrentKey].Image[i2] = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(NumPadKeys[CurrentKey].Width, NumPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonForegroundFocused, NumPadKeys[CurrentKey].Icon[i2], NumPadKeys[CurrentKey].Symbol[i2]));
                                                    else
                                                        NumPadKeys[CurrentKey].Image[i2] = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(NumPadKeys[CurrentKey].Width, NumPadKeys[CurrentKey].Height, ButtonGraphic.ImageTypes.ButtonForeground, NumPadKeys[CurrentKey].Icon[i2], NumPadKeys[CurrentKey].Symbol[i2]));
                                                }

                                                // Set overlay image for button
                                                btn.OverlayImage = NumPadKeys[CurrentKey].Image[CharSet];

                                                btn.Tag = NumPadKeys[CurrentKey];
                                                btn.Transition = eButtonTransition.None;
                                                btn.OnClick += new userInteraction(btn_OnClick);
                                                btn.OnHoldClick += new userInteraction(btn_OnHoldClick);

                                                // Set special names (OK and cancel buttons)
                                                if (NumPadKeys[CurrentKey].KeyCode == Key.Escape)
                                                    btn.Name = "OSK_Button_Cancel";
                                                else if (NumPadKeys[CurrentKey].KeyCode == Key.Enter)
                                                    btn.Name = "OSK_Button_OK";

                                                Panel.addControl(btn);
                                            }

                                            Left += NumPadKeys[CurrentKey].Width + SpacingX;
                                            CurrentKey++;
                                        }
                                        if (CurrentKey < NumPadKeys.Length)
                                            Top += NumPadKeys[CurrentKey].Height + SpacingY;
                                    }

                                    #endregion
                                }
                                break;
                            default:
                                break;
                        }

                        // Configure return data
                        //Panel.Forgotten = true;
                        
                        // Make sure that the OSK will cover the whole screen
                        Panel.Priority = ePriority.High;
                        Panel.PanelType = OMPanel.PanelTypes.Modal;

                        #endregion

                        // Return data
                        data = (T)Convert.ChangeType(Panel, typeof(T));
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        void btn_OnHoldClick(OMControl sender, int screen)
        {
            // Repeat button press
            OMButton btn = sender as OMButton;
            int loopCount = 0;
            while (btn.Mode == eModeType.Clicked)
	        {
                loopCount++;
                btn_OnClick(sender, screen);
                Thread.Sleep(100);

                // Loop safety (also limits the amount of repeating characters to 100)
                if (loopCount >= 100)
                    break;
	        }            
        }

        void btn_OnClick(OMControl sender, int screen)
        {
            // Get text control
            OMTextBox txt = sender.Parent[screen, "OSK_TextBox_Text"] as OMTextBox;
            if (txt == null)
                return;

            // Extract keyinfo data from tag
            KeyInfo keyInfo;
            try
            {
                keyInfo = (KeyInfo)((OMButton)sender).Tag;
            }
            catch
            {   // If we find a tag we can't convert to key we'll exit this method
                return;
            }

            // Handle special keys
            if (!HandleSpecialKeys(sender.Parent, screen, keyInfo.KeyCode))
            {   // Add other keys
                OSKValueAddCharacter(sender.Parent, screen, keyInfo.Symbol[CharSet]);
            }
        }

        private void StartMaskInputTimer(OMPanel Panel, int Screen)
        {
            // Initialize timer
            if (MaskInputTimer[Screen] == null)
            {
                MaskInputTimer[Screen] = new OpenMobile.Timer(1000);
                MaskInputTimer[Screen].Elapsed += new System.Timers.ElapsedEventHandler(OMOsk2_Elapsed);
            }

            MaskInputTimer[Screen].Enabled = false;
            MaskInputTimer[Screen].Enabled = true;
            MaskInputTimer[Screen].Panel = Panel;
            MaskInputTimer[Screen].Screen = Screen;
        }

        void OMOsk2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OpenMobile.Timer tmr = sender as OpenMobile.Timer;
            tmr.Enabled = false;
            if (tmr.Panel == null)
                return;

            // Get text control
            OMTextBox txt = tmr.Panel[tmr.Screen, "OSK_TextBox_Text"] as OMTextBox;

            // Mask text
            string Text = txt.Tag as string;
            if (Text == null)
                return;
            txt.Text = new string('*', Text.Length);

        }

        private void OSKValueAddCharacter(OMPanel Panel, int Screen, string Character)
        {
            // Get text control
            OMTextBox txt = Panel[Screen, "OSK_TextBox_Text"] as OMTextBox;

            // Add character
            string Text = txt.Tag as string;
            Text += Character;

            // Save value back to textcontrol
            txt.Tag = Text;

            // Masked input?
            if (Panel[Screen, "Shape_AccessBlock2"].Tag != null)
            {
                txt.Text = String.Format("{0}{1}", new string('*', Text.Length - 1), Character);
                StartMaskInputTimer(Panel, Screen);
            }
            else
                txt.Text = Text;

            // Show or hide helptext
            ShowHideHelpText(Panel, Screen);
        }

        private void OSKValueRemoveCharacter(OMPanel Panel, int Screen)
        {
            // Get text control
            OMTextBox txt = Panel[Screen, "OSK_TextBox_Text"] as OMTextBox;

            // Remove character
            string Text = txt.Tag as string;
            if (!String.IsNullOrEmpty(Text))
                Text = Text.Remove(Text.Length - 1);

            // Save value back to textcontrol
            txt.Tag = Text;

            // Masked input?
            if (Panel[Screen, "Shape_AccessBlock2"].Tag != null)
                txt.Text = new string('*', Text.Length);
            else
                txt.Text = Text;
            

            // Show or hide helptext
            ShowHideHelpText(Panel, Screen);
        }

        private void ShowHideHelpText(OMPanel Panel, int Screen)
        {
            OMTextBox txt = Panel[Screen, "OSK_TextBox_Text"] as OMTextBox;
            OMLabel lbl = Panel[Screen, "OSK_Label_HelpText"] as OMLabel;
            // Errorcheck
            if (txt == null || lbl == null)
                return;
            
            if (String.IsNullOrEmpty(txt.Text))
                lbl.Text = lbl.Tag as string;
            else
                lbl.Text = "";
        }

        private bool HandleSpecialKeys(OMPanel Panel, int Screen, Key key)
        {
            OMTextBox txt = Panel[Screen, "OSK_TextBox_Text"] as OMTextBox;
            if (txt == null)
                return false;

            switch (key)
            {
                case Key.Back:
                    {   // Remove a character
                        OSKValueRemoveCharacter(Panel, Screen);
                    }
                    break;
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    // Change between upper and lower case
                    if (CharSet == 0)
                        CharSet = 1;
                    else if (CharSet == 1)
                        CharSet = 0;
                    UpdateButtonGraphics(Panel, Screen, CharSet);
                    break;
                case Key.AltLeft:
                case Key.AltRight:
                    if (CharSet == 2)
                        CharSet = 0;
                    else
                        CharSet = 2;
                    UpdateButtonGraphics(Panel, Screen, CharSet);
                    break;
                case Key.Escape:
                case Key.Enter:
                    CharSet = 0;
                    break;
                case Key.Space:
                    OSKValueAddCharacter(Panel, Screen, " ");
                    break;
                default:    
                    return false;
            }
            return true;
        }

        private void UpdateButtonGraphics(OMPanel Panel, int Screen, int CharSet)
        {
            for (int i = 0; i < KeyPadKeys.Length; i++)
            {
                OMButton btn = Panel[Screen, String.Format("OSK_Button_{0}", i)] as OMButton;
                if (btn != null)
                {
                    KeyInfo keyInfo = (KeyInfo)btn.Tag;                    
                    btn.OverlayImage = keyInfo.Image[CharSet];
                }
            }
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            MaskInputTimer = new OpenMobile.Timer[host.ScreenCount];
            return eLoadStatus.LoadSuccessful;
        }

        bool OSK_OnKeyPress(OMPanel panel, int screen, eKeypressType type, KeyboardKeyEventArgs arg)
        {
            // Handle esc button (this is done on key up to prevent it from exiting the application)
            if (type == eKeypressType.KeyUp)
                if (arg.Key == Key.Escape)
                {
                    ((OMButton)panel["OSK_Button_Cancel"]).clickMe(screen);
                    return true;
                }
            
            if (type != eKeypressType.KeyDown)
                return false;
            if (panel == null)
                return false;
            OMTextBox txt = panel["OSK_TextBox_Text"] as OMTextBox;
            if (txt == null)
                return false;

            // Handle special keys
            switch (arg.Key)
            {
                case Key.Back:
                    {   // Remove a character
                        OSKValueRemoveCharacter(panel, screen);
                    }
                    return true;
                case Key.Escape:
                    return true;
                case Key.Enter:
                    {
                        ((OMButton)panel["OSK_Button_OK"]).clickMe(screen);
                    }
                    return true;
                default:
                    if (!String.IsNullOrEmpty(arg.Character))
                    {   // Add character 
                        OSKValueAddCharacter(panel, screen, arg.Character);
                        return true;
                    }
                    return false;
            }
            return false;        
        }

        void OSK_OnGesture(OMPanel panel, int screen, string character)
        {
            if (panel == null)
                return;
            OMTextBox txt = panel["OSK_TextBox_Text"] as OMTextBox;
            if (txt == null)
                return;

            switch (character)
            {
                case "back":
                    {   // Remove a character
                        OSKValueRemoveCharacter(panel, screen);
                    }
                    break;
                default:
                    if (!String.IsNullOrEmpty(character))
                    {   // Add character 
                        OSKValueAddCharacter(panel, screen, character);
                        return;
                    }
                    break;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}

/*
            if (function == eFunction.gesture)
            {
                if (arg3 == "OSK")
                {
                    OMTextBox tb = (OMTextBox)manager[int.Parse(arg1)]["Text"];
                    if (arg2=="back")
                        tb.Text=tb.Text.Remove(tb.Text.Length-1);
                    else
                        tb.Text+=arg2;
                }
            }
*/