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
Last Modified: Nov. 8, 2009
Created by: Justin Domnitz
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

            screens = new ScreenManager(theHost.ScreenCount);

            imageItem mainMenu = theHost.getSkinImage("menuButton");
            imageItem mainMenuFocus = theHost.getSkinImage("menuButtonHighlighted");

            #region Main menu panel

            //Lets load some settings
            PluginSettings settings = new PluginSettings();
            //Main Menu Controls are named by adding the row and column number to the end

            for (int screen = 0; screen < theHost.ScreenCount; screen++)
            {
                if (settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu1.Plugin") == "")
                    createDefaultSettings(settings, screen);
                
                OMPanel mainPanel = new OMPanel();
                OMButton[] MainMenuButtons = new OMButton[9];

                #region Row 1

                MainMenuButtons[0] = new OMButton(25, 116);
                MainMenuButtons[0].Image = mainMenu;
                MainMenuButtons[0].FocusImage = mainMenuFocus;
                MainMenuButtons[0].Name = "MainMenu." + screen.ToString() + ".MainMenu1";
                MainMenuButtons[0].Mode = eModeType.Highlighted;
                MainMenuButtons[0].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[0].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu1.Plugin");
                MainMenuButtons[0].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu1.Display");
                MainMenuButtons[0].OnClick += new userInteraction(MainMenu_OnClick);
                mainPanel.addControl(MainMenuButtons[0]);

                MainMenuButtons[1] = new OMButton(353, 116);
                MainMenuButtons[1].Image = mainMenu;
                MainMenuButtons[1].FocusImage = mainMenuFocus;
                MainMenuButtons[1].Name = "MainMenu." + screen.ToString() + ".MainMenu2";
                MainMenuButtons[1].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[1].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[1].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu2.Plugin");
                MainMenuButtons[1].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu2.Display");
                mainPanel.addControl(MainMenuButtons[1]);

                MainMenuButtons[2] = new OMButton(680, 116);
                MainMenuButtons[2].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[2].Image = mainMenu;
                MainMenuButtons[2].FocusImage = mainMenuFocus;
                MainMenuButtons[2].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[2].Name = "MainMenu." + screen.ToString() + ".MainMenu3";
                MainMenuButtons[2].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu3.Plugin");
                MainMenuButtons[2].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu3.Display");
                mainPanel.addControl(MainMenuButtons[2]);


                #endregion

                #region Row 2

                MainMenuButtons[3] = new OMButton(25, 260);
                MainMenuButtons[3].Image = mainMenu;
                MainMenuButtons[3].FocusImage = mainMenuFocus;
                MainMenuButtons[3].Name = "MainMenu." + screen.ToString() + ".MainMenu4";
                MainMenuButtons[3].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[3].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[3].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu4.Plugin");
                MainMenuButtons[3].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu4.Display");
                mainPanel.addControl(MainMenuButtons[3]);

                MainMenuButtons[4] = new OMButton(353, 260);
                MainMenuButtons[4].Image = mainMenu;
                MainMenuButtons[4].FocusImage = mainMenuFocus;
                MainMenuButtons[4].Name = "MainMenu." + screen.ToString() + ".MainMenu5";
                MainMenuButtons[4].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[4].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[4].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu5.Plugin");
                MainMenuButtons[4].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu5.Display");
                mainPanel.addControl(MainMenuButtons[4]);

                MainMenuButtons[5] = new OMButton(680, 260);
                MainMenuButtons[5].Image = mainMenu;
                MainMenuButtons[5].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[5].FocusImage = mainMenuFocus;
                MainMenuButtons[5].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[5].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu6.Plugin");
                MainMenuButtons[5].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu6.Display");
                MainMenuButtons[5].Name = "MainMenu." + screen.ToString() + ".MainMenu6";
                mainPanel.addControl(MainMenuButtons[5]);

                #endregion

                #region Row 3

                MainMenuButtons[6] = new OMButton(25, 400);
                MainMenuButtons[6].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[6].Image = mainMenu;
                MainMenuButtons[6].FocusImage = mainMenuFocus;
                MainMenuButtons[6].Name = "MainMenu." + screen.ToString() + ".MainMenu7";
                MainMenuButtons[6].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[6].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu7.Plugin");
                MainMenuButtons[6].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu7.Display");
                mainPanel.addControl(MainMenuButtons[6]);

                MainMenuButtons[7] = new OMButton(353, 400);
                MainMenuButtons[7].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[7].Image = mainMenu;
                MainMenuButtons[7].FocusImage = mainMenuFocus;
                MainMenuButtons[7].Name = "MainMenu." + screen.ToString() + ".MainMenu8";
                MainMenuButtons[7].OnLongClick += new userInteraction(OnLongClick);
                MainMenuButtons[7].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu8.Plugin");
                MainMenuButtons[7].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu8.Display");
                mainPanel.addControl(MainMenuButtons[7]);

                MainMenuButtons[8] = new OMButton(680, 400);
                MainMenuButtons[8].Image = mainMenu;
                MainMenuButtons[8].OnClick += new userInteraction(MainMenu_OnClick);
                MainMenuButtons[8].FocusImage = mainMenuFocus;
                MainMenuButtons[8].Tag = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu9.Plugin");
                MainMenuButtons[8].Text = settings.getSetting("MainMenu." + screen.ToString() + ".MainMenu9.Display");
                MainMenuButtons[8].Name = "MainMenu." + screen.ToString() + ".MainMenu9";
                MainMenuButtons[8].OnLongClick += new userInteraction(OnLongClick);
                mainPanel.addControl(MainMenuButtons[8]);

                #endregion

                #region Button Icons

                // Add icons
                string Identifier = "MainMenu." + screen.ToString() + ".MainMenu";
                int Count = 1;
                for (int i = 0; i < MainMenuButtons.Length; i++)
                {
                    // Set row identifier
                    //if (i == 3) { Identifier = "MainMenu." + screen.ToString() + ".MainMenu2"; Count = 1; }
                    //if (i == 6) { Identifier = "MainMenu." + screen.ToString() + ".MainMenu3"; Count = 1; }

                    OMImage MainMenu_Icon = new OMImage(MainMenuButtons[i].Left + 30, MainMenuButtons[i].Top, 40, 40);
                    MainMenu_Icon.Top = (MainMenuButtons[i].Top + (MainMenuButtons[i].Height / 2)) - (MainMenu_Icon.Height / 2);
                    MainMenu_Icon.Name = Identifier + (Count).ToString() + "_Icon";
                    mainPanel.addControl(MainMenu_Icon);
                    Count++;
                }

                #endregion
                
                //screens.loadPanel(mainPanel);
                screens.loadSinglePanel(mainPanel, screen);
            }

            settings.Dispose();

            #endregion

            imageItem opt2 = theHost.getSkinImage("Full.Highlighted");
            imageItem opt1 = theHost.getSkinImage("Full");

            #region Button function selection panel

            OMPanel panelSettings = new OMPanel("Settings");
            panelSettings.Forgotten = true;
            
            OMBasicShape Shape_AccessBlock = new OMBasicShape(0, 0, 1000, 600);
            Shape_AccessBlock.Name = "Settings_Shape_AccessBlock";
            Shape_AccessBlock.Shape = shapes.Rectangle;
            Shape_AccessBlock.FillColor = Color.FromArgb(150, Color.Black);
            panelSettings.addControl(Shape_AccessBlock);
            OMButton Button_Cancel2 = new OMButton(0, 0, 1000, 600);
            Button_Cancel2.Name = "Settings_Button_Cancel";
            panelSettings.addControl(Button_Cancel2);

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

            OMList List_FunctionList = new OMList(235, 150, 535, 295);
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

            OMButton Settings_Button_Ok = new OMButton(635, 460, 135, 50);
            Settings_Button_Ok.Name = "Settings_Button_Ok";
            Settings_Button_Ok.Image = opt1;
            Settings_Button_Ok.FocusImage = opt2;
            Settings_Button_Ok.Text = "OK";
            Settings_Button_Ok.OnClick += new userInteraction(Settings_Button_Ok_OnClick);
            panelSettings.addControl(Settings_Button_Ok);

            OMButton Settings_Button_Cancel = new OMButton(235, 460, 135, 50);
            Settings_Button_Cancel.Name = "Settings_Button_Cancel";
            Settings_Button_Cancel.Image = Settings_Button_Ok.Image;
            Settings_Button_Cancel.FocusImage = Settings_Button_Ok.FocusImage;
            Settings_Button_Cancel.Text = "Cancel";
            Settings_Button_Cancel.OnClick += new userInteraction(Settings_Button_Cancel_OnClick);
            panelSettings.addControl(Settings_Button_Cancel);

            screens.loadPanel(panelSettings);

            #endregion

            #region Exit menu

            OMPanel exit = new OMPanel("Quit");
            exit.Forgotten = true;
            OMImage Image1 = new OMImage(220, 115, 560, 400);
            Image1.Image = theHost.getSkinImage("MediaBorder");
            Image1.Name = "Image1";
            OMButton Quit = new OMButton(245,145,250,80);
            Quit.Image = opt1;
            Quit.FocusImage = opt2;
            Quit.Text = "Quit";
            Quit.Name = "UI.Quit";
            Quit.OnClick += new userInteraction(Quit_OnClick);
            Quit.OnLongClick += new userInteraction(Quit_OnLongClick);
            OMButton Sleep = new OMButton(245,229,250,80);
            Sleep.Image = opt1;
            Sleep.FocusImage = opt2;
            Sleep.Text = "Sleep";
            Sleep.Name = "UI.Sleep";
            Sleep.OnClick += new userInteraction(Sleep_OnClick);
            OMButton Hibernate = new OMButton(245,313,250,80);
            Hibernate.Image = opt1;
            Hibernate.FocusImage = opt2;
            Hibernate.Text = "Hibernate";
            Hibernate.Name = "UI.Hibernate";
            Hibernate.OnClick += new userInteraction(Hibernate_OnClick);
            OMButton Shutdown = new OMButton(245,397,250,80);
            Shutdown.Image = opt1;
            Shutdown.FocusImage = opt2;
            Shutdown.Text = "Shutdown";
            Shutdown.Name = "UI.Shutdown";
            Shutdown.OnClick += new userInteraction(Shutdown_OnClick);
            OMButton Restart = new OMButton(505, 145, 250, 80);
            Restart.Image = opt1;
            Restart.FocusImage = opt2;
            Restart.Text = "Restart";
            Restart.Name = "UI.Restart";
            Restart.OnClick += new userInteraction(Restart_OnClick);
            OMButton Reload = new OMButton(505, 229, 250, 80);
            Reload.Image = opt1;
            Reload.FocusImage = opt2;
            Reload.Text = "Reload";
            Reload.Name = "UI.Reload";
            Reload.OnClick += new userInteraction(Reload_OnClick);
            OMButton Screen = new OMButton(505, 313, 250, 80);
            Screen.Image = opt1;
            Screen.FocusImage = opt2;
            Screen.Text = "Screen Off";
            Screen.Name = "UI.Screen";
            Screen.OnClick += new userInteraction(Screen_OnClick);
            OMButton Cancel = new OMButton(505, 397, 250, 80);
            Cancel.Image = opt1;
            Cancel.FocusImage = opt2;
            Cancel.Text = "Cancel";
            Cancel.Name = "UI.Cancel";
            Cancel.OnClick += new userInteraction(Cancel_OnClick);
            exit.BackgroundType = backgroundStyle.SolidColor;
            exit.BackgroundColor1 = Color.FromArgb(125, Color.Black);
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

        void Quit_OnLongClick(OMControl sender, int screen)
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
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            theHost.execute(eFunction.setMonitorBrightness, screen.ToString(), "0");
        }
        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "MainMenu", "Quit");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
        }
        void MainMenu_OnClick(OMControl sender, int screen)
        {
            switch(sender.Tag.ToString())
            {
                case "Exit":
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu", "Quit");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    ((OMButton)sender).Transition = eButtonTransition.None;
                    break;
                case "":
                    return;
                default:
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(),"MainMenu");
                    if (theHost.execute(eFunction.TransitionToPanel,screen.ToString(), ((OMButton)sender).Tag.ToString())==false)
                        theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
                    theHost.execute(eFunction.ExecuteTransition,screen.ToString());
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
                    setting.setSetting(currentlySetting + ".Plugin", ((string)theList[theList.SelectedIndex].tag));
                    setting.setSetting(currentlySetting + ".Display", theList[theList.SelectedIndex].text);
                    screens[screen][currentlySetting].Tag = ((string)theList[theList.SelectedIndex].tag);
                    ((OMButton)screens[screen][currentlySetting]).Text = theList[theList.SelectedIndex].text;
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
            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu1.Plugin", "NewMedia"); ;
            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu1.Display", "Music");

            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu2.Plugin", "About");
            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu2.Display", "About");

            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu3.Plugin", "OMSettings");
            settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu3.Display", "Settings");

            if (screen == 0)
            {   // Exit button is by default only on screen zero (main screen)
                settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu5.Plugin", "Exit");
                settings.setSetting("MainMenu." + screen.ToString() + ".MainMenu5.Display", "Exit");
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
        void OnLongClick(OMControl sender, int screen)
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
                OMButton Button = ((OMButton)screens[screen, ""]["MainMenu." + screen.ToString() + ".MainMenu" + i.ToString()]);
                OMImage Icon = ((OMImage)screens[screen, ""]["MainMenu." + screen.ToString() + ".MainMenu" + i.ToString() + "_Icon"]);
                if (Button.Tag != null)
                    if ((string)Button.Tag != "")
                    {
                        // Check for default icons
                        if ((string)Button.Tag == "Exit")
                            Icon.Image = new imageItem(OImage.FromWebdingsFont(100, 100, "~", Color.Gray));
                        else if ((string)Button.Tag == "About")
                            Icon.Image = new imageItem(OImage.FromWebdingsFont(100, 100, "i", Color.Gray));
                        else
                            Icon.Image = new imageItem(plugin.GetPluginIcon((string)Button.Tag, eTextFormat.Normal, Color.Gray, Color.Gray));
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

                List<IHighLevel> plugins = plugin.getPluginsOfType<IHighLevel>(PluginLevels.Normal);
                plugins.Sort((a, b) => a.displayName.CompareTo(b.displayName));
                for (int i = 0; i < plugins.Count; i++)
                {
                    IHighLevel b = plugins[i];
                    subItemformat = new OMListItem.subItemFormat();
                    subItemformat.color = Color.FromArgb(128, Color.White);
                    subItemformat.font = new Font(Font.GenericSansSerif, 18F);

                    OMListItem ListItem = new OMListItem(" " + b.displayName, "  " + (b.pluginDescription != "" ? b.pluginDescription : b.pluginName), subItemformat);
                    ListItem.tag = b.pluginName;
                    ListItem.image = plugin.GetPluginIcon(b, eTextFormat.Normal, Color.Gray, Color.Gray);
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
            if (RefreshData)
            {   // Refresh data
                MainMenu_UpdateIcons(screen);
            }

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