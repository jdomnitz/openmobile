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
*********************************************************************************
Example Name: Main Menu Example
Example Created: Sept. 10, 2009
Last Modified: Feb. 18, 2012
Created by: Justin Domnitz / Bjørn Morten Orderløkken
*********************************************************************************/
using System;
using System.Collections.Generic;
using OpenMobile.Graphics;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;
using OpenMobile.Media;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.Plugins;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.helperFunctions;

namespace OpenMobile
{

    //All High Level plugins should implement IHighLevel
    [InitialTransition(eGlobalTransition.SlideDown)]
    [PluginLevel(PluginLevels.MainMenu | PluginLevels.System)]
    public sealed class MainMenu : IHighLevel
    {
        #region IHighLevel Members

        IPluginHost theHost;
        OMListItem.subItemFormat subItemformat;
        bool RefreshData = true;

        //Heres where the magic happens
        public eLoadStatus initialize(IPluginHost host)
        {
            //We save a reference to the plugin host for use later
            theHost = host;

            screens = new ScreenManager(this);

            imageItem mainMenu = theHost.getSkinImage("menuButton");
            imageItem mainMenuFocus = theHost.getSkinImage("menuButtonHighlighted");

            #region Main menu panel

            //Lets load some settings
            PluginSettings settings = new PluginSettings();
            //Main Menu Controls are named by adding the row and column number to the end

            for (int screen = 0; screen < theHost.ScreenCount; screen++)
            {
                if (settings.getSetting(this, "MainMenu." + screen.ToString() + ".MainMenu1.Plugin") == "")
                    createDefaultSettings(settings, screen);

                OMPanel mainPanel = new OMPanel("MainMenu");

                //// Screen background image
                //OMImage backgroundImage = new OMImage("backgroundImage", OM.Host.ClientFullArea.Left, OM.Host.ClientFullArea.Top, OM.Host.ClientFullArea.Width, OM.Host.ClientFullArea.Height);
                //backgroundImage.Image = OM.Host.getSkinImage("Backgrounds|Highway 1");
                //mainPanel.addControl(backgroundImage);             

                OMButton[] MainMenuButtons = new OMButton[9];

                for (int i = 0; i < MainMenuButtons.Length; i++)
                {
                    string Name = String.Format("MainMenu.{0}.MainMenu{1}", screen.ToString(), (i + 1));
                    string Text = settings.getSetting(this, String.Format("{0}.Display", Name));

                    #region Calculate left placement

                    int Left = 0;
                    switch (i)
                    {
                        case 1:
                        case 4:
                        case 7:
                            Left = 353;
                            break;
                        case 2:
                        case 5:
                        case 8:
                            Left = 680;
                            break;
                        default:
                            Left = 25;
                            break;
                    }

                    #endregion

                    #region Calculate top placement

                    int Top = 0;
                    switch (i)
                    {
                        case 3:
                        case 4:
                        case 5:
                            Top = 228;
                            break;
                        case 6:
                        case 7:
                        case 8:
                            Top = 368;
                            break;
                        default:
                            Top = 84;
                            break;
                    }

                    #endregion

                    //MainMenuButtons[i] = DefaultControls.GetButton(Name, Left, Top, 300, 120, "", Text);
                    MainMenuButtons[i] = OMButton.PreConfigLayout_BasicStyle(Name, Left, Top, 300, 120, GraphicCorners.All, "", Text);
                    MainMenuButtons[i].OnHoldClick += new userInteraction(OnHoldClick);
                    MainMenuButtons[i].Tag = settings.getSetting(this, String.Format("{0}.Plugin", Name));
                    MainMenuButtons[i].OnClick += new userInteraction(MainMenu_OnClick);
                    mainPanel.addControl(MainMenuButtons[i]);                    
                }

                //// Clock and date
                //OMLabel labelClockTime = new OMLabel("labelClockTime", 350, 522, 300, 60);
                //labelClockTime.TextAlignment = Alignment.CenterCenter;
                //labelClockTime.Font = new Font(Font.GenericSansSerif, 32F);
                //labelClockTime.Format = eTextFormat.Normal;
                //labelClockTime.sensorName = "SystemSensors.Time";
                //labelClockTime.Text = "Clock";
                //mainPanel.addControl(labelClockTime);     
                //OMLabel labelClockdate = new OMLabel("labelClockdate", 350, 572, 300, 30);
                //labelClockdate.TextAlignment = Alignment.CenterCenter;
                //labelClockdate.Font = new Font(Font.GenericSansSerif, 20F);
                //labelClockdate.Format = eTextFormat.Normal;
                //labelClockdate.sensorName = "SystemSensors.Date";
                //labelClockdate.Text = "Date";
                //mainPanel.addControl(labelClockdate);       

                mainPanel.Entering += new PanelEvent(mainPanel_Entering);

                screens.loadSinglePanel(mainPanel, screen, true);
            }

            settings.Dispose();

            #endregion

            imageItem opt2 = theHost.getSkinImage("Full.Highlighted");
            imageItem opt1 = theHost.getSkinImage("Full");

            #region Button function selection panel

            OMPanel panelSettings = new OMPanel("Settings");
            panelSettings.Forgotten = true;
            
            OMBasicShape Shape_AccessBlock = new OMBasicShape("Settings_Shape_AccessBlock", 0, 0, 1000, 600,
                new ShapeData(shapes.Rectangle, Color.FromArgb(150, Color.Black)));
            panelSettings.addControl(Shape_AccessBlock);
            OMButton Button_Cancel2 = new OMButton(0, 0, 1000, 600);
            Button_Cancel2.Name = "Settings_Button_Cancel";
            panelSettings.addControl(Button_Cancel2);

            OMImage Image_panelSettings_Background = new OMImage("TestImg", 200, 110);
            Image_panelSettings_Background.FitControlToImage = true;
            PanelPopupOutlineGraphic.GraphicData gd = new PanelPopupOutlineGraphic.GraphicData();
            gd.Width = 614;
            gd.Height = 434;
            gd.TextFont = new Font(Font.Arial, 24);
            gd.Type = PanelPopupOutlineGraphic.Types.RoundedRectangle;
            gd.Text = "Select the panel to assign to this button";
            Image_panelSettings_Background.Image = new imageItem(PanelPopupOutlineGraphic.GetImage(gd));
            panelSettings.addControl(Image_panelSettings_Background);


            /*
            OMBasicShape Shape_Border = new OMBasicShape(200, 110, 600, 420);
            Shape_Border.Name = "Settings_Shape_Border";
            Shape_Border.Shape = shapes.RoundedRectangle;
            Shape_Border.FillColor = Color.FromArgb(58, 58, 58);
            Shape_Border.BorderColor = Color.Gray;
            Shape_Border.BorderSize = 2;
            panelSettings.addControl(Shape_Border);

            OMLabel Label_Header = new OMLabel(Shape_Border.Left + 5, Shape_Border.Top + 5, Shape_Border.Width - 10, 30);
            Label_Header.Name = "Settings_Label_Header";
            Label_Header.Text = "Select the panel to assign to this button";
            panelSettings.addControl(Label_Header);

            OMBasicShape Shape_Line = new OMBasicShape(Shape_Border.Left, Label_Header.Top + Label_Header.Height, Shape_Border.Width, 2);
            Shape_Line.Name = "Settings_Shape_Line";
            Shape_Line.Shape = shapes.Rectangle;
            Shape_Line.FillColor = Color.Gray;
            Shape_Line.BorderColor = Color.Transparent;
            panelSettings.addControl(Shape_Line);

            OMBasicShape Shape_Background = new OMBasicShape();
            Shape_Background.Top = Shape_Line.Top + Shape_Line.Height;
            Shape_Background.Left = Shape_Border.Left + (int)Shape_Border.BorderSize;
            Shape_Background.Width = Shape_Border.Width - ((int)Shape_Border.BorderSize * 2);
            Shape_Background.Height = Shape_Border.Height - (Shape_Background.Top - Shape_Border.Top)-8;
            Shape_Background.Name = "Settings_Shape_Background";
            Shape_Background.Shape = shapes.Rectangle;
            Shape_Background.FillColor = Color.Black;
            panelSettings.addControl(Shape_Background);
            OMBasicShape Shape_Background_Lower = new OMBasicShape(Shape_Background.Left, Shape_Background.Top + Shape_Background.Height - 13, Shape_Background.Width, 20);
            Shape_Background_Lower.Name = "Settings_Shape_Background_Lower";
            Shape_Background_Lower.Shape = shapes.RoundedRectangle;
            Shape_Background_Lower.FillColor = Color.Black;
            panelSettings.addControl(Shape_Background_Lower);
            */

            OMList List_FunctionList = new OMList(235, 155, 535, 290);
            List_FunctionList.SoftEdgeData.Color1 = Color.Black;
            List_FunctionList.SoftEdgeData.Sides = FadingEdge.GraphicSides.Top | FadingEdge.GraphicSides.Bottom;
            List_FunctionList.UseSoftEdges = true;
            List_FunctionList.Name = "Settings_List_FunctionList";
            List_FunctionList.Scrollbars = true;
            List_FunctionList.ListStyle = eListStyle.MultiList;
            List_FunctionList.Background = Color.Silver;
            List_FunctionList.ItemColor1 = Color.Black;
            List_FunctionList.Font = new Font(Font.GenericSansSerif, 28F);
            List_FunctionList.Color = Color.White;
            List_FunctionList.HighlightColor = Color.White;
            List_FunctionList.SelectedItemColor1 = Color.DarkBlue;
            List_FunctionList.ListItemHeight = 70;
            subItemformat = new OMListItem.subItemFormat();
            subItemformat.color = Color.FromArgb(128, Color.White);
            subItemformat.font = new Font(Font.GenericSansSerif, 15F);            
            panelSettings.addControl(List_FunctionList);

            OMButton Settings_Button_Ok = DefaultControls.GetButton("Settings_Button_Ok", 605, 450, 165, 70, "", "OK");
            Settings_Button_Ok.OnClick += new userInteraction(Settings_Button_Ok_OnClick);
            panelSettings.addControl(Settings_Button_Ok);

            OMButton Settings_Button_Cancel = DefaultControls.GetButton("Settings_Button_Cancel", 235, 450, 165, 70, "", "Cancel");
            Settings_Button_Cancel.OnClick += new userInteraction(Settings_Button_Cancel_OnClick);
            panelSettings.addControl(Settings_Button_Cancel);

            screens.loadPanel(panelSettings);

            #endregion

            #region Exit menu

            OMPanel exit = new OMPanel("Quit");
            exit.PanelType = OMPanel.PanelTypes.Modal;
            exit.Forgotten = true;
            exit.BackgroundType = backgroundStyle.SolidColor;
            exit.BackgroundColor1 = Color.FromArgb(125, Color.Black);

            OMImage Image1 = new OMImage("Image1", 220, 115, 560, 400, theHost.getSkinImage("MediaBorder"));

            OMButton Quit = DefaultControls.GetButton("UI.Quit", 245, 145, 250, 80, "", "Quit");
            Quit.OnClick += new userInteraction(Quit_OnClick);
            Quit.OnHoldClick += new userInteraction(Quit_OnHoldClick);

            OMButton Sleep = DefaultControls.GetButton("UI.Sleep", 245, 229, 250, 80, "", "Sleep");
            Sleep.OnClick += new userInteraction(Sleep_OnClick);

            OMButton Hibernate = DefaultControls.GetButton("UI.Hibernate", 245, 313, 250, 80, "", "Hibernate");
            Hibernate.OnClick += new userInteraction(Hibernate_OnClick);

            OMButton Shutdown = DefaultControls.GetButton("UI.Shutdown", 245, 397, 250, 80, "", "Shutdown");
            Shutdown.OnClick += new userInteraction(Shutdown_OnClick);

            OMButton Restart = DefaultControls.GetButton("UI.Restart", 505, 145, 250, 80, "", "Restart");
            Restart.OnClick += new userInteraction(Restart_OnClick);

            OMButton Reload = DefaultControls.GetButton("UI.Reload", 505, 229, 250, 80, "", "Reload");
            Reload.OnClick += new userInteraction(Reload_OnClick);

            OMButton Screen = DefaultControls.GetButton("UI.Screen", 505, 313, 250, 80, "", "Screen Off");
            Screen.OnClick += new userInteraction(Screen_OnClick);

            OMButton Cancel = DefaultControls.GetButton("UI.Cancel", 505, 397, 250, 80, "", "Cancel");
            Cancel.OnClick += new userInteraction(Cancel_OnClick);

            exit.addControl(Image1);
            exit.addControl(Quit);
            exit.addControl(Sleep);
            exit.addControl(Hibernate);
            exit.addControl(Shutdown);
            exit.addControl(Reload);
            exit.addControl(Restart);
            exit.addControl(Screen);
            exit.addControl(Cancel);
            exit.Priority = ePriority.Urgent;
            screens.loadSharedPanel(exit);

            #endregion

            return eLoadStatus.LoadSuccessful;
        }

        void mainPanel_Entering(OMPanel sender, int screen)
        {
            MainMenu_UpdateIcons(screen);
        }

        void Quit_OnHoldClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            theHost.execute(eFunction.minimize,screen.ToString());
        }
        void Restart_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.restart);
        }
        void Reload_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.restartProgram);
        }
        void Screen_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen, "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen, "None");
            theHost.execute(eFunction.setMonitorBrightness, screen, "0");
        }
        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen, "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen, "None");
        }
        void MainMenu_OnClick(OMControl sender, int screen)
        {
            switch(sender.Tag.ToString())
            {
                case "Exit":
                    theHost.execute(eFunction.TransitionToPanel, screen,"MainMenu", "Quit");
                    theHost.execute(eFunction.ExecuteTransition, screen);
                    ((OMButton)sender).Transition = eButtonTransition.None;
                    break;
                case "":
                    return;
                default:
                    theHost.execute(eFunction.TransitionFromPanel, screen,"MainMenu");
                    if (theHost.execute(eFunction.TransitionToPanel,screen, ((OMButton)sender).Tag)==false)
                        theHost.execute(eFunction.TransitionToPanel, screen, "MainMenu");
                    theHost.execute(eFunction.ExecuteTransition, screen);
                    break;
            }
        }
        void Quit_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.closeProgram);
        }
        void Sleep_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            theHost.execute(eFunction.standby);
        }
        void Hibernate_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            theHost.execute(eFunction.hibernate);
        }
        void Shutdown_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.shutdown);
        }
        void Settings_Button_Ok_OnClick(OMControl sender, int screen)
        {
            OMList theList=(OMList)screens[screen,"Settings"]["Settings_List_FunctionList"];
            if (theList.SelectedIndex >= 0) //FS#3
            {
                using (PluginSettings setting = new PluginSettings())
                {
                    if (theList[theList.SelectedIndex].text.Trim() == "Not Set")
                        theList[theList.SelectedIndex].text = "";
                    setting.setSetting(this, currentlySetting + ".Plugin", ((string)theList[theList.SelectedIndex].tag));
                    setting.setSetting(this, currentlySetting + ".Display", theList[theList.SelectedIndex].text);
                    screens[screen][currentlySetting].Tag = ((string)theList[theList.SelectedIndex].tag);
                    //((OMButton)screens[screen][currentlySetting]).Text = theList[theList.SelectedIndex].text;
                }
            }
            currentlySetting = "";
            MainMenu_UpdateIcons(screen);
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu");
            theHost.execute(eFunction.ExecuteTransition,screen.ToString(),"None");
        }

        private void createDefaultSettings(PluginSettings settings, int screen)
        {
            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu1.Plugin", "NewMedia"); ;
            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu1.Display", "Music");

            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu2.Plugin", "About");
            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu2.Display", "About");

            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu3.Plugin", "OMSettings");
            settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu3.Display", "Settings");

            if (screen == 0)
            {   // Exit button is by default only on screen zero (main screen)
                settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu5.Plugin", "Exit");
                settings.setSetting(this, "MainMenu." + screen.ToString() + ".MainMenu5.Display", "Exit");
            }
        }

        void Settings_Button_Cancel_OnClick(OMControl sender, int screen)
        {
            currentlySetting = "";
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
        }

        string currentlySetting = "";
        void OnHoldClick(OMControl sender, int screen)
        {
            // Is settings already in action
            // Disable check for already active since we can set it individually on each screen
            /*
            if (currentlySetting != "")
            {
                dialog dialog = new dialog(this.pluginName, sender.Parent.Name);
                dialog.Header = "Button assignment";
                dialog.Text = "Settings is already active on another screen!\nPlease close this first.";
                dialog.Icon = icons.Exclamation;
                dialog.Button = buttons.OK;
                dialog.ShowMsgBox(screen).ToString();
                return;
            }
            */

            currentlySetting = sender.Name;
            Settings_ConfigureList(screen);
            theHost.execute(eFunction.TransitionToPanel,screen.ToString(), "MainMenu","Settings");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void MainMenu_UpdateIcons(int screen)
        {   // Loop trough all button and update icons
            for (int i = 1; i <= 9; i++)
            {
                OMButton Button = ((OMButton)screens[screen, "MainMenu"]["MainMenu." + screen.ToString() + ".MainMenu" + i.ToString()]);
                //OMImage Icon = ((OMImage)screens[screen, "MainMenu"]["MainMenu." + screen.ToString() + ".MainMenu" + i.ToString() + "_Icon"]);
                if (Button.Tag != null)
                    //if ((string)Button.Tag != "")
                    {
                        // Check for default icons
                        //if ((string)Button.Tag == "Exit")
                        //    //Icon.Image = new imageItem(OImage.FromWebdingsFont(100, 100, "~", Color.Gray));
                        //    Button.OverlayImage = new imageItem(ButtonGraphic.GetImage(Button.Width, Button.Height, ButtonGraphic.ImageTypes.ButtonForeground, "~", StoredData.Get(String.Format("{0}.Display", Button.Name))));

                        //else if ((string)Button.Tag == "About")
                        //    //Icon.Image = new imageItem(OImage.FromWebdingsFont(100, 100, "i", Color.Gray));
                        //    Button.OverlayImage = new imageItem(ButtonGraphic.GetImage(Button.Width, Button.Height, ButtonGraphic.ImageTypes.ButtonForeground, "i", StoredData.Get(String.Format("{0}.Display", Button.Name))));
                            
                        //else
                        //Icon.Image = new imageItem(plugin.GetPluginIcon((string)Button.Tag, eTextFormat.Normal, Color.Gray, Color.Gray));
                        {
                            #region Configure button data

                            // Get plugin
                            IBasePlugin plugin = Plugins.GetPlugin((string)Button.Tag);

                            ButtonGraphic.GraphicData gd = new ButtonGraphic.GraphicData();

                            //if (!String.IsNullOrEmpty((string)Button.Tag))
                            {
                                // Set icon symbol
                                Font f = Font.Webdings;
                                //gd.Icon = Plugins.GetPluginIconSymbol((string)Button.Tag, ref f)               ;
                                //gd.IconFont = f;

                                // Set Icon Image
                                if (plugin != null)
                                    gd.IconImage = plugin.pluginIcon.image;
                                else
                                {
                                    if ((string)Button.Tag == "Exit")
                                        gd.IconImage = OM.Host.getSkinImage("Icons|Icon-Power").image;
                                    else if ((string)Button.Tag == "About")
                                        gd.IconImage = OM.Host.getSkinImage("Icons|Icon-OM").image;
                                }
                                //if (String.IsNullOrEmpty(gd.Icon))
                                //    gd.IconImage = Plugins.GetPluginIconString((string)Button.Tag);

                                // Set default image
                                if (String.IsNullOrEmpty(gd.Icon) && gd.IconImage == null)
                                    gd.Icon = ((char)0x85).ToString();
                            }

                            // Set Text
                            gd.Text = StoredData.Get(this, String.Format("{0}.Display", Button.Name));

                            // Set size
                            gd.Width = Button.Width;
                            gd.Height = Button.Height;

                            // Set type of graphic
                            gd.ImageType = ButtonGraphic.ImageTypes.ButtonForeground;

                            #endregion
                            Button.OverlayImage = new imageItem(ButtonGraphic.GetImage(gd));
                        }
                    }
            }
        }

        private void Settings_ConfigureList(int screen)
        {
            {   // Create list data for button selection
                OMButton Btn = (OMButton)screens[screen][currentlySetting];

                // Error check
                if (Btn == null)
                {
                    // No active button was found, display error message
                    dialog dialog = new dialog(this.pluginName, "");
                    dialog.Header = "Button assignment";
                    dialog.Text = "Error: Unable to get the currently active button!\nPlease try again.";
                    dialog.Icon = icons.Error;
                    dialog.Button = buttons.OK;
                    dialog.ShowMsgBox(screen).ToString();
                    return;
                }

                string ActiveItem = Btn.Text;
                int SelectedIndex = -1;
                
                OMList list = ((OMList)screens[screen, "Settings"]["Settings_List_FunctionList"]);
                list.Clear();
                
                // Add "Not Set" menu item
                OMListItem item1 = new OMListItem(" Not Set", "", subItemformat);
                item1.tag = "";
                list.Add(item1);
                if (ActiveItem == item1.text) SelectedIndex = 0; // Is this the selected item

                // Add "Exit" menu item
                OMListItem item2 = new OMListItem(" Exit", "  Show exit menu", subItemformat);
                item2.tag = "Exit";
                item2.image = OImage.FromWebdingsFont(100, 100, "~", Color.Gray);
                list.Add(item2);
                if (ActiveItem == item2.text) SelectedIndex = 1; // Is this the selected item

                OMListItem item3 = new OMListItem(" About", "  Show information about OM", subItemformat);
                item3.tag = "About";
                item3.image = OImage.FromWebdingsFont(100, 100, "i", Color.Gray);
                list.Add(item3);
                if (ActiveItem == item3.text) SelectedIndex = 2; // Is this the selected item

                List<IHighLevel> plugins = Plugins.getPluginsOfType<IHighLevel>(PluginLevels.Normal);
                plugins.Sort((a, b) => a.displayName.CompareTo(b.displayName));
                for (int i = 0; i < plugins.Count; i++)
                {
                    IHighLevel b = plugins[i];
                    subItemformat = new OMListItem.subItemFormat();
                    subItemformat.color = Color.FromArgb(128, Color.White);
                    subItemformat.font = new Font(Font.GenericSansSerif, 18F);

                    OMListItem ListItem = new OMListItem(" " + b.displayName, "  " + (b.pluginDescription != "" ? b.pluginDescription : b.pluginName), subItemformat);
                    ListItem.tag = b.pluginName;
                    ListItem.image = Plugins.GetPluginIcon(b, eTextFormat.Normal, Color.Gray, Color.Gray);
                    list.Add(ListItem);

                    // Is this the selected item
                    if (b.displayName == ActiveItem)
                        SelectedIndex = i+3;
                }
                list.SelectedIndex = SelectedIndex;
            }
        }

        ScreenManager screens;
        public OMPanel loadPanel(string name,int screen)
        {
            //if (RefreshData)
            //{   // Refresh data
            //    MainMenu_UpdateIcons(screen);
            //}

            //if (name == "") 
            //    return screens[screen, "MainMenu"];
            return screens[screen,name];
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
            get { return "jdomnitz@gmail.com"; }
        }
        public string displayName
        {
            get { return "Main Menu"; }
        }
        public string pluginName
        {
            get { return "MainMenu"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "The main menu"; }
        }

        public imageItem pluginIcon
        {
            get { return imageItem.NONE; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (screens!=null)
                screens.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}