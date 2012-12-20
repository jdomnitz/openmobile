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
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.BitAccess;
using System.Collections.Generic;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;

namespace OMDialog
{
    [SkinIcon("*2")]
    [PluginLevel(PluginLevels.System)]
    public class OMDialog : IHighLevel
    {
        IPluginHost theHost;
        ScreenManager Manager;
        string name;
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
            get { return "OMDialog"; }
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
            get { return "OMDialog"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Dialogs for the framework"; }
        }
        #endregion
        public bool incomingMessage(string message, string source)
        {
            return false; // No action
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // Convert input data to local data object
            OMPanel Panel = (OMPanel)Convert.ChangeType(data, typeof(T));

            // Extract dialog data
            dialog.DialogData DT = ((dialog.DialogData)Panel.Tag);
            Manager = DT.Manager;
            name = Panel.Name;

            /* Buttons has to be named according to this:
             *  Dialog_Button_Cancel    : Cancel button
             *  Dialog_Button_Yes       : Yes button
             *  Dialog_Button_No        : No button
             *  Dialog_Button_OK        : OK button
             *  Dialog_Button_Abort     : Abort button
             *  Dialog_Button_Retry     : Retry button
             *  Dialog_Button_Ignore    : Ignore button
             * 
             * Labels has to be named like this:
             *  Dialog_Label_Header     : Dialog header
             *  Dialog_Label_Text       : Dialog text
            */

            // What type of dialog is this?
            switch (message.ToLower())
            {
                case "messagebox":
                    {
                        #region Create messagebox panel

                        // Default dialog data
                        DT.Left = (DT.Left == 0 ? 250 : DT.Left);
                        DT.Top = (DT.Top == 0 ? 175 : DT.Top);
                        DT.Width = (DT.Width == 0 ? 500 : DT.Width);
                        DT.Height = (DT.Height == 0 ? 250 : DT.Height);

                        // Calculate amount of buttons to show
                        int ButtonCount = bit.Count((uint)DT.Button);
                        if (ButtonCount == 0)
                            DT.Height -= 50;

                        Font f = new Font(Font.GenericSansSerif, 18F);
                        // Calculate and set width of dialogbox
                        int Width = (int)Graphics.MeasureString(DT.Text, f).Width;
                        DT.Width = 500;
                        if (Width > 500)
                            DT.Width = Width;
                        if (DT.Width > 800)
                            DT.Width = 800;

                        // Position dialog box horisontally on screen (unless provided as a parameter)
                        if ((DT.Width > 500) && (DT.Left == 250))
                        {
                            DT.Left = (500 - (DT.Width / 2));
                        }

                        OMBasicShape Shape_AccessBlock2 = new OMBasicShape("Dialog_Shape_AccessBlock", 0, 0, 1000, 600,
                            new ShapeData(shapes.Rectangle, Color.FromArgb(150, Color.Black)));
                        Panel.addControl(Shape_AccessBlock2);
                        OMButton Button_Cancel2 = new OMButton(0, 0, 1000, 600);
                        Button_Cancel2.Name = "Dialog_Button_None";
                        Panel.addControl(Button_Cancel2);

                        OMImage Image_panel_Background = new OMImage("Dialog_Image_Background", DT.Left, DT.Top);
                        Image_panel_Background.FitControlToImage = true;
                        PanelPopupOutlineGraphic.GraphicData gd = new PanelPopupOutlineGraphic.GraphicData();
                        gd.Width = DT.Width;
                        gd.Height = DT.Height;
                        // TODO: Calculate text width to match dialog size
                        gd.TextFont = new Font(Font.Arial, 24);
                        gd.Type = PanelPopupOutlineGraphic.Types.RoundedRectangle;
                        gd.Text = DT.Header;
                        Image_panel_Background.Image = new imageItem(PanelPopupOutlineGraphic.GetImage(ref gd));
                        Panel.addControl(Image_panel_Background);

                        // Save reference to client area data given from graphics generation
                        Rectangle ClientArea = gd.ClientArea;
                        ClientArea.X += DT.Left;
                        ClientArea.Y += DT.Top;

                        OMImage Image_Icon = new OMImage();
                        Image_Icon.Name = "Dialog_Image_Icon";
                        Image_Icon.Left = ClientArea.Left + 20;
                        Image_Icon.Top = ClientArea.Top + 20;
                        Image_Icon.Width = 0;
                        Image_Icon.Height = 0;
                        if (DT.Icon != icons.None)
                        {
                            Image_Icon.Width = 100;
                            Image_Icon.Height = 100;

                            switch (DT.Icon)
                            {
                                case icons.Error:
                                    Image_Icon.Image = theHost.getSkinImage("Error");
                                    break;
                                case icons.Exclamation:
                                    Image_Icon.Image = theHost.getSkinImage("Exclamation");
                                    break;
                                case icons.Question:
                                    Image_Icon.Image = theHost.getSkinImage("questionMark");
                                    break;
                                case icons.Checkmark:
                                    Image_Icon.Image = theHost.getSkinImage("Checkmark");
                                    break;
                                case icons.Information:
                                    Image_Icon.Image = theHost.getSkinImage("Information");
                                    break;
                                case icons.Custom:
                                    Image_Icon.Image = theHost.getSkinImage(DT.CustomIcon);
                                    break;                                    
                                case icons.Busy:
                                    Image_Icon.Image = theHost.getSkinImage("BusyAnimation");
                                    break;
                                case icons.OM:
                                    Image_Icon.Image = theHost.getSkinImage("OM");
                                    break;
                                case icons.WorkInProgress:
                                    Image_Icon.Image = theHost.getSkinImage("WorkInProgress");
                                    break;
                                default:
                                    break;
                            }
                            Panel.addControl(Image_Icon);
                           
                        }

                        OMLabel Label_Text = new OMLabel();
                        Label_Text.Left = Image_Icon.Left + Image_Icon.Width + 5;
                        Label_Text.Top = ClientArea.Top + 5;
                        Label_Text.Width = ClientArea.Width - (Label_Text.Left - ClientArea.Left) - 10;
                        Label_Text.Height = ClientArea.Height - 75;//135;
                        Label_Text.Name = "Dialog_Label_Text";
                        Label_Text.Text = "";
                        Label_Text.Font = f;
                        Label_Text.TextAlignment = OpenMobile.Graphics.Alignment.WordWrap | Alignment.TopCenter;
                        Panel.addControl(Label_Text);

                        #region Assign buttons

                        int ButtonWidth = 150;
                        if (ButtonCount > 0)
                        {   // Calculate width of buttons
                            ButtonWidth = (ClientArea.Width - 10) / ButtonCount;
                            if (ButtonWidth > 150)
                                ButtonWidth = 150;
                        }

                        OMButton[] Buttons = new OMButton[ButtonCount];

                        // Activate required buttons
                        int Btn = 0;
                        foreach (buttons b in Enum.GetValues(typeof(buttons)))
                        {
                            if (b != buttons.None)
                            {
                                if ((DT.Button & b) == b)
                                {   // Bit is set, update button
                                    Buttons[Btn] = new OMButton("Dialog_Button_" + b.ToString(), 0, 0, 0, 0);
                                    Buttons[Btn].Text = b.ToString();   // Set text
                                    //Buttons[Btn].Name = "Dialog_Button_" + b.ToString();    // Set name of button (name indicates dialog result)
                                    Btn++;
                                }
                            }
                        }

                        // Generate first button
                        Buttons[0] = DefaultControls.GetButton(
                            Buttons[0].Name,
                            ClientArea.Left + ClientArea.Width - ButtonWidth - 5,
                            ClientArea.Top + ClientArea.Height - 80,
                            ButtonWidth, 
                            70, 
                            "",
                            Buttons[0].Text);
                        Buttons[0].Transition = eButtonTransition.None;
                        Buttons[0].Text = "";
                        Panel.addControl(Buttons[0]);

                        /*
                        Buttons[0].Height = 70;
                        Buttons[0].Width = ButtonWidth;
                        Buttons[0].Left = ClientArea.Left + ClientArea.Width - Buttons[0].Width - 5;
                        Buttons[0].Top = ClientArea.Top + ClientArea.Height - 80;
                        Buttons[0].Image = theHost.getSkinImage("Full");
                        Buttons[0].FocusImage = theHost.getSkinImage("Full.Highlighted");
                        Buttons[0].Transition = eButtonTransition.None;
                        Panel.addControl(Buttons[0]);
                        */

                        for (int i = 1; i < Buttons.Length; i++)
                        {
                            Buttons[i] = DefaultControls.GetButton(
                                Buttons[i].Name,
                                Buttons[0].Left - (Buttons[0].Width * i),
                                Buttons[0].Top,
                                Buttons[0].Width,
                                Buttons[0].Height,
                                "",
                                Buttons[i].Text);
                            Buttons[i].Transition = Buttons[0].Transition;
                            Buttons[i].Text = "";
                            Panel.addControl(Buttons[i]);
                            /*
                            Buttons[i] = new OMButton();
                            Buttons[i].Top = Buttons[0].Top;
                            Buttons[i].Width = Buttons[0].Width;
                            Buttons[i].Left = Buttons[0].Left - (Buttons[0].Width*i);
                            Buttons[i].Height = Buttons[0].Height;
                            Buttons[i].Image = Buttons[0].Image;
                            Buttons[i].FocusImage = Buttons[0].FocusImage;
                            Buttons[i].Transition = Buttons[0].Transition;
                            Panel.addControl(Buttons[i]);
                            */
                        }

                        // Configure button function and text
                        /*
                        int Btn = 0;
                        foreach (buttons b in Enum.GetValues(typeof(buttons)))
                        {
                            if (b != buttons.None)
                            {
                                if ((DT.Button & b) == b)
                                {   // Bit is set, update button
                                    Buttons[Btn].Text = b.ToString();   // Set text
                                    Buttons[Btn].Name = "Dialog_Button_" + b.ToString();    // Set name of button (name indicates dialog result)
                                    Btn++;
                                }
                            }
                        }
                        */

                        #endregion

                        Panel.Forgotten = true;
                        Panel.PanelType = OMPanel.PanelTypes.Modal;
                        //Panel.Priority = ePriority.High;
                        
                        #endregion
                    }
                    break;
                default:
                    break;
            }

            // Return data
            data = (T)Convert.ChangeType(Panel, typeof(T));
            return true;
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            return eLoadStatus.LoadSuccessful;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
