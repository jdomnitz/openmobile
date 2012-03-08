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
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;

namespace OpenMobile.helperFunctions.Forms
{
    /// <summary>
    /// Dialog buttons and return values
    /// </summary>
    [Flags]
    public enum buttons : byte
    {
        /// <summary>
        /// Button none
        /// </summary>
        None = 0,
        /// <summary>
        /// Button Abort
        /// </summary>
        Abort = 1,
        /// <summary>
        /// Button Retry
        /// </summary>
        Retry = 2,
        /// <summary>
        /// Button Ignore
        /// </summary>
        Ignore = 4,
        /// <summary>
        /// Button Cancel
        /// </summary>
        Cancel = 8,
        /// <summary>
        /// Button No
        /// </summary>
        No = 16,
        /// <summary>
        /// Button Yes
        /// </summary>
        Yes = 32,
        /// <summary>
        /// Button OK
        /// </summary>
        OK = 64
    }

    /// <summary>
    /// Icons for built in forms
    /// </summary>
    public enum icons
    {
        /// <summary>
        /// No icon
        /// </summary>
        None,
        /// <summary>
        /// Error icon
        /// </summary>
        Error,
        /// <summary>
        /// Exclamation icon
        /// </summary>
        Exclamation,
        /// <summary>
        /// Questionmark
        /// </summary>
        Question,
        /// <summary>
        /// Checkmark
        /// </summary>
        Checkmark,
        /// <summary>
        /// Information icon
        /// </summary>
        Information,
        /// <summary>
        /// Custom icon: Specify icon to use in property CustomIcon
        /// </summary>
        Custom,
        /// <summary>
        /// Animated icon: Busy / Wait 
        /// </summary>
        Busy,
        /// <summary>
        /// OpenMobile logo
        /// </summary>
        OM,
        /// <summary>
        /// Work in progress logo
        /// </summary>
        WorkInProgress
    }

    /// <summary>
    /// The equivalent of a dialog with options for message box and so on...
    /// </summary>
    public class dialog
    {
        public class DialogData
        {
            public string Header { get; set; }
            public string Text { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public icons Icon { get; set; }
            public buttons Button { get; set; }
            public string CustomIcon { get; set; }
            public OpenMobile.Framework.ScreenManager Manager { get; set; }
        }

        private string DialogHandler = "OMDialog";
        private string OwnerPlugin, OwnerPanel, OwnerScreen;
        private EventWaitHandle ButtonPress = new EventWaitHandle(false, EventResetMode.ManualReset);
        private buttons Result = buttons.None;
        private string PanelName = "";
        private List<string> ActiveDialogs = new List<string>();
        private bool ReOpenDialog = false;
        private bool OpenAtExecute = false;
        private bool Visible = true;

        /// <summary>
        /// Header text
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Dialog text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Left position (absolute)
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Top position (absolute)
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Height of dialog
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Width of dialog
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Icon to show
        /// </summary>
        public icons Icon { get; set; }

        /// <summary>
        /// Buttons to show
        /// </summary>
        public buttons Button { get; set; }

        /// <summary>
        /// Custom icon to use instead of predefined ones
        /// </summary>
        public string CustomIcon { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes the messagebox class
        /// </summary>
        /// <param name="Plugin"></param>
        /// <param name="Panel"></param>
        public dialog()
        {
            // Default dialog data
            Icon = icons.None;
            CustomIcon = "";
        }
        /// <summary>
        /// Initializes the messagebox class
        /// </summary>
        /// <param name="Plugin"></param>
        /// <param name="Panel"></param>
        public dialog(string Plugin, string Panel)
            : this()
        {
            OwnerPlugin = Plugin;
            OwnerPanel = Panel;
        }
        /// <summary>
        /// Initializes the messagebox class
        /// </summary>
        /// <param name="DialogHandler">Name of plugin that should provide the dialog panel</param>
        /// <param name="Plugin"></param>
        /// <param name="Panel"></param>
        public dialog(string DialogHandler, string Plugin, string Panel)
            : this(Plugin,Panel)
        {
            this.DialogHandler = DialogHandler;
        }
        #endregion

        /// <summary>
        /// Show a non thread blocking messagebox 
        /// </summary>
        /// <param name="screen">Screen number to show on</param>
        /// <param name="Delay">Delay (MS) before showing dialog (will not show if close is called within timeframe)</param>
        public void ShowMsgBoxNonBlocking(int screen, int Delay)
        {
            Visible = true;
            Thread t = new Thread(
                delegate()
                {
                    Thread.Sleep(Delay);
                    if (Visible)
                        ShowMsgBox(screen);
                });
            t.IsBackground = true;
            t.Start();
        }
        /// <summary>
        /// Show a non thread blocking messagebox 
        /// </summary>
        /// <param name="screen"></param>
        public void ShowMsgBoxNonBlocking(int screen)
        {
            Visible = true;
            Thread t = new Thread(
                delegate()
                {
                    ShowMsgBox(screen);
                });
            t.IsBackground = true;
            t.Start();
        }
        /// <summary>
        /// Show a messagebox style dialog
        /// </summary>
        public buttons ShowMsgBox(int screen)
        {
            Visible = true;

            // Errorcheck
            if (BuiltInComponents.Host == null)
                throw new Exception("Core error; BuiltInComponents.Host not initialized");

            // Create panel
            OMPanel Panel = new OMPanel("");

            // Set panel name
            PanelName = Panel.Name = String.Format("{0}_{1}", DialogHandler, Panel.GetHashCode());

            OwnerScreen = screen.ToString();

            // Pack paneldata into tag property
            DialogData DT = new DialogData();
            DT.Left = Left; DT.Top = Top; DT.Height = Height; DT.Width = Width; DT.Icon = Icon; DT.CustomIcon = CustomIcon; DT.Button = Button; DT.Text = Text; DT.Header = Header; ; DT.Manager = BuiltInComponents.Panels;
            Panel.Tag = DT;

            if (!BuiltInComponents.Host.sendMessage<OMPanel>(DialogHandler, "OpenMobile.helperFunctions.Dialog", "MessageBox", ref Panel))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get MessageBox panel, plugin " + DialogHandler + " not available");
                return buttons.None;
            }

            if (Panel != null)
            {
                Panel.Priority = ePriority.High;

                ButtonPress.Reset();
                OwnerScreen = screen.ToString();

                // Add to list of active dialogs
                ActiveDialogs.Add(OwnerPlugin + "." + OwnerPanel + "." + OwnerScreen + "." + Panel.Name);
                ReOpenDialog = false;

                #region Map buttons
                /* Attach to any buttons (button names are set according to this:
                     * None, OK, Cancel, Abort, Retry, Ignore, Yes, No
                     *  Dialog_Button_Cancel    : Cancel button
                     *  Dialog_Button_Yes       : Yes button
                     *  Dialog_Button_No        : No button
                     *  Dialog_Button_OK        : OK button
                     *  Dialog_Button_Abort     : Abort button
                     *  Dialog_Button_Retry     : Retry button
                     *  Dialog_Button_Ignore    : Ignore button
                     * 
                    */

                OMButton btn = null;
                foreach (buttons dr in Enum.GetValues(typeof(buttons)))
                {
                    btn = (OMButton)Panel["Dialog_Button_" + dr.ToString()];
                    if (btn != null)
                    {
                        btn.OnClick += new userInteraction(Button_OnClick);
                        btn.Tag = dr;
                    }
                }
                #endregion

                #region Set texts

                OMLabel lbl = null;
                // Set header text
                /*
                lbl = (OMLabel)Panel["Dialog_Label_Header"];
                if (lbl != null)
                    if (lbl.Text == "")
                        lbl.Text = Header;
                */

                // Set dialog text
                lbl = (OMLabel)Panel["Dialog_Label_Text"];
                if (lbl != null)
                    if (lbl.Text == "")
                        lbl.Text = Text;

                #endregion

                // Connect to system events (to detect goback event)
                SystemEvent SysEv = new SystemEvent(theHost_OnSystemEvent);
                BuiltInComponents.Host.OnSystemEvent += SysEv;

                // load and show panel
                BuiltInComponents.Panels.loadPanel(Panel);
                if (BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "", Panel.Name) == false)
                    return buttons.None;
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());

                // Wait for buttons
                ButtonPress.WaitOne();

                // Remove messagebox and clean up
                ActiveDialogs.Remove(OwnerPlugin + "." + OwnerPanel + "." + OwnerScreen + "." + Panel.Name);
                BuiltInComponents.Host.OnSystemEvent -= SysEv;
                BuiltInComponents.Host.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                //BuiltInComponents.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), "", Panel.Name);
                //BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                BuiltInComponents.Panels.unloadPanel(Panel.Name);
            }
            return Result;
        }

        /// <summary>
        /// Close the dialog
        /// </summary>
        public void Close()
        {
            Visible = false;
            Result = buttons.None;
            ButtonPress.Set();
        }

        void Button_OnClick(OMControl sender, int screen)
        {
            Result = (buttons)sender.Tag;
            ButtonPress.Set();
        }
        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (!String.IsNullOrEmpty(OwnerPanel) && !String.IsNullOrEmpty(OwnerPlugin))
            {
                if (function == eFunction.TransitionFromAny)
                {
                    // Reopen dialog
                    ReOpenDialog = true;
                }
                if (function == eFunction.TransitionFromPanel)
                {
                    if ((arg1 == OwnerScreen) & (arg2 == OwnerPlugin) & (arg3 == OwnerPanel))
                    {
                        // Reopen dialog
                        ReOpenDialog = true;
                    }
                }
                if (function == eFunction.ExecuteTransition)
                {
                    if (OpenAtExecute)
                    {
                        OpenAtExecute = false;
                        BuiltInComponents.Host.execute(eFunction.TransitionToPanel, OwnerScreen, "", PanelName);
                        BuiltInComponents.Host.execute(eFunction.ExecuteTransition, OwnerScreen);
                    }

                }
                if (function == eFunction.TransitionToPanel)
                {
                    if (ReOpenDialog)
                    {
                        if ((arg1 == OwnerScreen) & (arg2 == OwnerPlugin) & (arg3 == OwnerPanel))
                        {
                            // Reopen dialog
                            OpenAtExecute = true;
                            ReOpenDialog = false;
                        }
                    }
                }
            }

            if (function == eFunction.goBack)
            {
                if (!String.IsNullOrEmpty(PanelName))
                {
                    if (arg3 == PanelName)
                    {
                        Result = buttons.None;
                        ButtonPress.Set();
                    }
                }
                else
                {   // React to any goback event
                    Result = buttons.None;
                    ButtonPress.Set();
                }
            }
        }
    }

    public class DialogPanel : OMPanel
    {
        private OMBasicShape Shape_AccessBlock = null;
        public OMButton Button_Ok = null;
        public OMButton Button_Cancel = null;
        public Rectangle ClientArea = new Rectangle(0, 0, 0, 0);
        public Rectangle HeaderArea = new Rectangle(0, 0, 0, 0);

        public DialogPanel(string name, int x, int y, int w, int h, bool ShowButtons, bool Forgotten)
            : this(name, name, x, y, w, h, ShowButtons, Forgotten)
        {
        }

        public DialogPanel(string name, string Header, int x, int y, int w, int h, bool ShowButtons, bool Forgotten)
            : base(name)
        {
            this.Forgotten = Forgotten;

            Shape_AccessBlock = new OMBasicShape(0, 0, 1000, 600);
            Shape_AccessBlock.Name = name + "_Shape_AccessBlock";
            Shape_AccessBlock.Shape = shapes.Rectangle;
            Shape_AccessBlock.FillColor = Color.FromArgb(150, Color.Black);
            this.addControl(Shape_AccessBlock);
            OMButton Button_Cancel2 = new OMButton(name + "_Button_Cancel", 0, 0, 1000, 600);
            this.addControl(Button_Cancel2);

            OMImage Image_panelSettings_Background = new OMImage(name + "_Image_Background",x,y);
            Image_panelSettings_Background.FitControlToImage = true;
            PanelOutlineGraphic.GraphicData gd = new PanelOutlineGraphic.GraphicData();
            gd.Width = w;
            gd.Height = h;
            gd.TextFont = new Font(Font.Arial, 24);
            gd.Type = PanelOutlineGraphic.Types.RoundedRectangle;
            gd.Text = Header;
            Image_panelSettings_Background.Image = new imageItem(PanelOutlineGraphic.GetImage(ref gd));
            this.addControl(Image_panelSettings_Background);

            // Save reference to area data given from graphics generation
            ClientArea = gd.ClientArea;
            ClientArea.X += x;
            ClientArea.Y += y;
            HeaderArea = gd.HeaderArea;
            HeaderArea.X += x;
            HeaderArea.Y += y;

            if (ShowButtons)
            {
                Button_Ok = DefaultControls.GetButton(name + "_Button_Ok", ClientArea.Right - 165, ClientArea.Bottom - 75, 135, 70, "", "OK");
                this.addControl(Button_Ok);

                Button_Cancel = DefaultControls.GetButton(name + "_Button_Cancel", ClientArea.Left + 35, ClientArea.Bottom - 75, 135, 70, "", "Cancel");
                this.addControl(Button_Cancel);
                ClientArea.Height -= Button_Ok.Height;
            }
        }
    }

}
