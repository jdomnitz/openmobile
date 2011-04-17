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
using System.Collections.Generic;

namespace OMDialog
{
    public class Dialog : IHighLevel
    {
        IPluginHost theHost;
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
            get { return "Dialogs"; }
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
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // Convert input data to local data object
            OMPanel Panel = (OMPanel)Convert.ChangeType(data, typeof(T));

            // Extract dialog data
            OpenMobile.helperFunctions.General.Dialog.DialogData DT = ((OpenMobile.helperFunctions.General.Dialog.DialogData)Panel.Tag);

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

                        OMBasicShape Shape_AccessBlock2 = new OMBasicShape(0, 0, 1000, 600);
                        Shape_AccessBlock2.Name = "Dialog_Shape_AccessBlock";
                        Shape_AccessBlock2.Shape = shapes.Rectangle;
                        Shape_AccessBlock2.FillColor = Color.FromArgb(150, Color.Black);
                        Panel.addControl(Shape_AccessBlock2);
                        OMButton Button_Cancel2 = new OMButton(0, 0, 1000, 600);
                        Button_Cancel2.Name = "Dialog_Button_Cancel";
                        Panel.addControl(Button_Cancel2);

                        OMBasicShape Shape_Border2 = new OMBasicShape(DT.Left, DT.Top, DT.Width, DT.Height);
                        Shape_Border2.Name = "Dialog_Shape_Border";
                        Shape_Border2.Shape = shapes.RoundedRectangle;
                        Shape_Border2.FillColor = Color.FromArgb(58, 58, 58);
                        Shape_Border2.BorderColor = Color.Gray;
                        Shape_Border2.BorderSize = 2;
                        Panel.addControl(Shape_Border2);

                        OMLabel Label_Header2 = new OMLabel(Shape_Border2.Left + 5, Shape_Border2.Top + 5, Shape_Border2.Width - 10, 30);
                        Label_Header2.Name = "Dialog_Label_Header";
                        Panel.addControl(Label_Header2);

                        OMBasicShape Shape_Line2 = new OMBasicShape(Shape_Border2.Left, Label_Header2.Top + Label_Header2.Height, Shape_Border2.Width, 2);
                        Shape_Line2.Name = "Dialog_Shape_Line";
                        Shape_Line2.Shape = shapes.Rectangle;
                        Shape_Line2.FillColor = Color.Gray;
                        Shape_Line2.BorderColor = Color.Transparent;
                        Panel.addControl(Shape_Line2);

                        OMBasicShape Shape_Background2 = new OMBasicShape(Shape_Border2.Left + (int)Shape_Border2.BorderSize, Shape_Line2.Top + Shape_Line2.Height, Shape_Border2.Width - ((int)Shape_Border2.BorderSize * 2), 205);
                        Shape_Background2.Name = "Dialog_Shape_Background";
                        Shape_Background2.Shape = shapes.Rectangle;
                        Shape_Background2.FillColor = Color.Black;
                        Panel.addControl(Shape_Background2);
                        OMBasicShape Shape_Background_Lower2 = new OMBasicShape(Shape_Background2.Left, Shape_Background2.Top + Shape_Background2.Height - 13, Shape_Background2.Width, 20);
                        Shape_Background_Lower2.Name = "Dialog_Shape_Background_Lower";
                        Shape_Background_Lower2.Shape = shapes.RoundedRectangle;
                        Shape_Background_Lower2.FillColor = Color.Black;
                        Panel.addControl(Shape_Background_Lower2);

                        OMImage Image_Icon = new OMImage();
                        Image_Icon.Left = Shape_Border2.Left + 20;
                        Image_Icon.Top = Shape_Background2.Top + 20;
                        Image_Icon.Width = 0;
                        Image_Icon.Height = 0;
                        if (DT.Icon != General.Dialog.Icons.None)
                        {
                            Image_Icon.Width = 100;
                            Image_Icon.Height = 100;

                            switch (DT.Icon)
                            {
                                case General.Dialog.Icons.Error:
                                    Image_Icon.Image = theHost.getSkinImage("Error");
                                    break;
                                case General.Dialog.Icons.Exclamation:
                                    Image_Icon.Image = theHost.getSkinImage("Exclamation");
                                    break;
                                case General.Dialog.Icons.Question:
                                    Image_Icon.Image = theHost.getSkinImage("questionMark");
                                    break;
                                case General.Dialog.Icons.Checkmark:
                                    Image_Icon.Image = theHost.getSkinImage("Checkmark");
                                    break;
                                case General.Dialog.Icons.Information:
                                    Image_Icon.Image = theHost.getSkinImage("Information");
                                    break;
                                case General.Dialog.Icons.Custom:
                                    Image_Icon.Image = theHost.getSkinImage(DT.CustomIcon);
                                    break;                                    
                                default:
                                    break;
                            }
                            Panel.addControl(Image_Icon);
                           
                        }

                        OMLabel Label_Text = new OMLabel();
                        Label_Text.Left = Image_Icon.Left + Image_Icon.Width + 5;
                        Label_Text.Top = Shape_Background2.Top + 5;
                        Label_Text.Width = Shape_Border2.Width - (Label_Text.Left - Shape_Border2.Left) - 10;
                        Label_Text.Height = 135;
                        Label_Text.Name = "Dialog_Label_Text";
                        Label_Text.Text = "";
                        Label_Text.TextAlignment = OpenMobile.Graphics.Alignment.WordWrap | Alignment.CenterCenter;
                        Panel.addControl(Label_Text);

                        OMButton Button_Yes = new OMButton(Shape_Border2.Left + Shape_Border2.Width - 320, Shape_Border2.Top + Shape_Border2.Height - 80, 150, 70);
                        Button_Yes.Name = "Dialog_Button_Yes";
                        Button_Yes.Image = theHost.getSkinImage("Full");
                        Button_Yes.FocusImage = theHost.getSkinImage("Full.Highlighted");
                        Button_Yes.Transition = eButtonTransition.None;
                        Button_Yes.Text = "Yes";
                        Panel.addControl(Button_Yes);

                        OMButton Button_No = new OMButton(Button_Yes.Left + Button_Yes.Width + 10, Button_Yes.Top, Button_Yes.Width, Button_Yes.Height);
                        Button_No.Name = "Dialog_Button_No";
                        Button_No.Image = theHost.getSkinImage("Full");
                        Button_No.FocusImage = theHost.getSkinImage("Full.Highlighted");
                        Button_No.Transition = eButtonTransition.None;
                        Button_No.Text = "No";
                        Panel.addControl(Button_No);

                        //Panel.Forgotten = true;
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
