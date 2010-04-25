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
using System.Drawing;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;

//All High Level plugins should implement IHighLevel
[InitialTransition(eGlobalTransition.SlideDown)]
public sealed class MainMenu : IHighLevel
    {

        #region IHighLevel Members
        //Here we create two panels, one for the main menu and one to assign button actions

        IPluginHost theHost;

        //Heres where the magic happens
        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel mainPanel = new OMPanel("");

            //We save a reference to the plugin host for use later
            theHost = host;
            imageItem mainMenu = theHost.getSkinImage("menuButton");
            imageItem mainMenuFocus = theHost.getSkinImage("menuButtonHighlighted");

            //Lets load some settings
            PluginSettings settings = new PluginSettings();
            if (settings.getSetting("MainMenu.MainMenu11.Plugin") == "")
                createDefaultSettings(settings);
            //Main Menu Controls are named by adding the row and column number to the end
            OMButton MainMenu11 = new OMButton(25,116);
            MainMenu11.Image = mainMenu;
            MainMenu11.FocusImage = mainMenuFocus;
            MainMenu11.Name = "MainMenu.MainMenu11";
            MainMenu11.Mode = modeType.Highlighted;
            MainMenu11.OnLongClick += new userInteraction(OnLongClick);
            MainMenu11.Tag = settings.getSetting("MainMenu.MainMenu11.Plugin");
            MainMenu11.Text = settings.getSetting("MainMenu.MainMenu11.Display");
            MainMenu11.OnClick += new userInteraction(MainMenu_OnClick);
            OMButton MainMenu12 = new OMButton(353,116);
            MainMenu12.Image = mainMenu;
            MainMenu12.FocusImage = mainMenuFocus;
            MainMenu12.Name = "MainMenu.MainMenu12";
            MainMenu12.OnLongClick += new userInteraction(OnLongClick);
            MainMenu12.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu12.Tag = settings.getSetting("MainMenu.MainMenu12.Plugin");
            MainMenu12.Text = settings.getSetting("MainMenu.MainMenu12.Display");
            OMButton MainMenu22 = new OMButton(353,260);
            MainMenu22.Image = mainMenu;
            MainMenu22.FocusImage = mainMenuFocus;
            MainMenu22.Name = "MainMenu.MainMenu22";
            MainMenu22.OnLongClick += new userInteraction(OnLongClick);
            MainMenu22.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu22.Tag = settings.getSetting("MainMenu.MainMenu22.Plugin");
            MainMenu22.Text = settings.getSetting("MainMenu.MainMenu22.Display");
            OMButton MainMenu32 = new OMButton(353,400);
            MainMenu32.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu32.Image = mainMenu;
            MainMenu32.FocusImage = mainMenuFocus;
            MainMenu32.Name = "MainMenu.MainMenu32";
            MainMenu32.OnLongClick += new userInteraction(OnLongClick);
            MainMenu32.Tag = settings.getSetting("MainMenu.MainMenu32.Plugin");
            MainMenu32.Text = settings.getSetting("MainMenu.MainMenu32.Display");
            OMButton MainMenu21 = new OMButton(25,260);
            MainMenu21.Image = mainMenu;
            MainMenu21.FocusImage = mainMenuFocus;
            MainMenu21.Name = "MainMenu.MainMenu21";
            MainMenu21.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu21.OnLongClick += new userInteraction(OnLongClick);
            MainMenu21.Tag = settings.getSetting("MainMenu.MainMenu21.Plugin");
            MainMenu21.Text = settings.getSetting("MainMenu.MainMenu21.Display");
            OMButton MainMenu31 = new OMButton(25,400);
            MainMenu31.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu31.Image = mainMenu;
            MainMenu31.FocusImage = mainMenuFocus;
            MainMenu31.Name = "MainMenu.MainMenu31";
            MainMenu31.OnLongClick += new userInteraction(OnLongClick);
            MainMenu31.Tag = settings.getSetting("MainMenu.MainMenu31.Plugin");
            MainMenu31.Text = settings.getSetting("MainMenu.MainMenu31.Display");
            OMButton MainMenu13 = new OMButton(680,116);
            MainMenu13.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu13.Image = mainMenu;
            MainMenu13.FocusImage = mainMenuFocus;
            MainMenu13.OnLongClick += new userInteraction(OnLongClick);
            MainMenu13.Name = "MainMenu.MainMenu13";
            MainMenu13.Tag = settings.getSetting("MainMenu.MainMenu13.Plugin");
            MainMenu13.Text = settings.getSetting("MainMenu.MainMenu13.Display");
            OMButton MainMenu23 = new OMButton(680,260);
            MainMenu23.Image = mainMenu;
            MainMenu23.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu23.FocusImage = mainMenuFocus;
            MainMenu23.OnLongClick += new userInteraction(OnLongClick);
            MainMenu23.Tag = settings.getSetting("MainMenu.MainMenu23.Plugin");
            MainMenu23.Text = settings.getSetting("MainMenu.MainMenu23.Display");
            MainMenu23.Name = "MainMenu.MainMenu23";
            OMButton MainMenu33 = new OMButton(680,400);
            MainMenu33.Image = mainMenu;
            MainMenu33.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu33.FocusImage = mainMenuFocus;
            MainMenu33.Tag = settings.getSetting("MainMenu.MainMenu33.Plugin");
            MainMenu33.Text = settings.getSetting("MainMenu.MainMenu33.Display");
            MainMenu33.Name = "MainMenu.MainMenu33";
            MainMenu33.OnLongClick += new userInteraction(OnLongClick);

            settings.Dispose();
            //And then we add them to the main panel (Main Menu)
            mainPanel.addControl(MainMenu11);
            mainPanel.addControl(MainMenu12);
            mainPanel.addControl(MainMenu13);
            mainPanel.addControl(MainMenu21);
            mainPanel.addControl(MainMenu22);
            mainPanel.addControl(MainMenu23);
            mainPanel.addControl(MainMenu31);
            mainPanel.addControl(MainMenu32);
            mainPanel.addControl(MainMenu33);
            screens = new ScreenManager(theHost.ScreenCount);
            screens.loadPanel(mainPanel);

            //We can do more then one panel too
            OMPanel setButton = new OMPanel("Settings");
            imageItem opt2 = theHost.getSkinImage("Full.Highlighted");
            imageItem opt1 = theHost.getSkinImage("Full");
            OMList list = new OMList(235,150,450,295);
            list.Font = new Font(FontFamily.GenericSerif, 26F);
            list.ListStyle = eListStyle.RoundedTextList;
            OMLabel title = new OMLabel(208,120,500,25);
            title.Text = "Select the panel to assign to this button";
            title.Format = textFormat.BoldShadow;
            OMButton okButton = new OMButton(550, 460, 135, 50);
            okButton.Image = opt1;
            okButton.FocusImage = opt2;
            okButton.Text = "OK";
            okButton.OnClick += new userInteraction(okButton_OnClick);
            OMButton cancel = new OMButton(235, 460, 135, 50);
            cancel.Image = okButton.Image;
            cancel.FocusImage = okButton.FocusImage;
            cancel.Text = "Cancel";
            cancel.OnClick += new userInteraction(cancel_OnClick);
            //In this example we have a seperate panel with a list on it
            setButton.addControl(list);
            setButton.addControl(title);
            setButton.addControl(okButton);
            setButton.addControl(cancel);
            screens.loadPanel(setButton);
            
            OMPanel exit = new OMPanel("Quit");
            OMImage Image1 = new OMImage(275, 140, 400, 330);
            Image1.Image = theHost.getSkinImage("MediaBorder");
            Image1.Name = "Image1";
            OMButton Quit = new OMButton(330,171,300,60);
            Quit.Image = opt1;
            Quit.FocusImage = opt2;
            Quit.Text = "Quit";
            Quit.Name = "UI.Quit";
            Quit.OnClick += new userInteraction(Quit_OnClick);
            OMButton Sleep = new OMButton(330,239,300,60);
            Sleep.Image = opt1;
            Sleep.FocusImage = opt2;
            Sleep.Text = "Sleep";
            Sleep.Name = "UI.Sleep";
            Sleep.OnClick += new userInteraction(Sleep_OnClick);
            OMButton Hibernate = new OMButton(330,308,300,60);
            Hibernate.Image = opt1;
            Hibernate.FocusImage = opt2;
            Hibernate.Text = "Hibernate";
            Hibernate.Name = "UI.Hibernate";
            Hibernate.OnClick += new userInteraction(Hibernate_OnClick);
            OMButton Shutdown = new OMButton(330,377,300,60);
            Shutdown.Image = opt1;
            Shutdown.FocusImage = opt2;
            Shutdown.Text = "Shutdown";
            Shutdown.Name = "UI.Shutdown";
            Shutdown.OnClick += new userInteraction(Shutdown_OnClick);
            Shutdown.OnLongClick += new userInteraction(Shutdown_OnLongClick);
            OMBasicShape visibleShape = new OMBasicShape(0, 0, 1000, 600);
            visibleShape.Shape = shapes.Rectangle;
            visibleShape.FillColor = Color.FromArgb(130, Color.Black);
            exit.addControl(visibleShape);
            exit.addControl(Image1);
            exit.addControl(Quit);
            exit.addControl(Sleep);
            exit.addControl(Hibernate);
            exit.addControl(Shutdown);
            screens.loadSharedPanel(exit);
            return eLoadStatus.LoadSuccessful;
        }

        void Shutdown_OnLongClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.restart);
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
        void okButton_OnClick(OMControl sender, int screen)
        {
            OMList theList=(OMList)screens[screen,"Settings"][0];
            if (theList.SelectedIndex >= 0) //FS#3
            {
                using (PluginSettings setting = new PluginSettings())
                {
                    if (theList[theList.SelectedIndex].text == "Not Set")
                        theList[theList.SelectedIndex].text = "";
                    setting.setSetting(currentlySetting + ".Plugin", theList[theList.SelectedIndex].text);
                    screens[screen][currentlySetting].Tag = theList[theList.SelectedIndex].text;
                    setting.setSetting(currentlySetting + ".Display", getDisplayName(theList[theList.SelectedIndex].text));
                    ((OMButton)screens[screen][currentlySetting]).Text = getDisplayName(theList[theList.SelectedIndex].text);
                }
            }
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(),"MainMenu","Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu");
            theHost.execute(eFunction.ExecuteTransition,screen.ToString(),"None");
        }
        private string getDisplayName(string name)
        {
            if (name == "")
                return name;
            if (name == "Exit")
                return name;
            if (name == "About")
                return name;
            object pl = new object();
            theHost.getData(eGetData.GetPlugins, name, out pl);
            if (pl == null)
                return null;
            return ((IHighLevel)pl).displayName;
        }
        private void createDefaultSettings(PluginSettings settings)
        {
            settings.setSetting("MainMenu.MainMenu11.Plugin", "Media");;
            settings.setSetting("MainMenu.MainMenu12.Plugin", "About");
            settings.setSetting("MainMenu.MainMenu13.Plugin", "OMSettings");
            settings.setSetting("MainMenu.MainMenu22.Plugin", "Exit");
            
            settings.setSetting("MainMenu.MainMenu11.Display", "Music");
            settings.setSetting("MainMenu.MainMenu12.Display", "About");
            settings.setSetting("MainMenu.MainMenu13.Display", "Settings");
            settings.setSetting("MainMenu.MainMenu22.Display", "Exit");
        }

        void cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(),"MainMenu","Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
        }

        string currentlySetting;
        void OnLongClick(OMControl sender, int screen)
        {
            currentlySetting = sender.Name;
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(), "MainMenu");
            theHost.execute(eFunction.TransitionToPanel,screen.ToString(), "MainMenu","Settings");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
        }

        ScreenManager screens;
        public OMPanel loadPanel(string name,int screen)
        {
            if (name == "Settings")
            {
                object o;
                theHost.getData(eGetData.GetPlugins, "", out o);
                if (o == null)
                    return null;
                OMList list = ((OMList)screens[screen,"Settings"][0]);
                list.Clear();
                list.Add("Not Set");
                list.Add("Exit");
                list.Add("About");
                Type hl = typeof(IHighLevel); //performance improvement
                foreach (IBasePlugin b in (List<IBasePlugin>)o)
                {
                    if (hl.IsInstanceOfType(b))
                        if ((b.pluginName != "UI") && (b.pluginName != "MainMenu") && (b.pluginName != "OMNotify")) //FS#7
                            list.Add(b.pluginName);
                }
            }
            return screens[screen,name];
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBasePlugin Members
        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "admin@domnitzsolutions.com"; }
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
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
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
