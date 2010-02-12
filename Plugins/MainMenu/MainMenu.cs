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

//All High Level plugins should implement IHighLevel
[InitialTransition(eGlobalTransition.SlideDown)]
public sealed class MainMenu : IHighLevel
    {

        #region IHighLevel Members
        //Here we create two panels, one for the main menu and one to assign button actions

        OMPanel setButton = new OMPanel();
        IPluginHost theHost;

        //Heres where the magic happens
        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel mainPanel = new OMPanel();

            //We save a reference to the plugin host for use later
            theHost = host;
            imageItem mainMenu = theHost.getSkinImage("menuButton");
            imageItem mainMenuFocus = theHost.getSkinImage("menuButtonHighlighted");

            //Lets load some settings
            PluginSettings settings = new PluginSettings();
            if (settings.getSetting("MainMenu.MainMenu11.Display") == "")
                createDefaultSettings(settings);
            //Main Menu Controls are named by adding the row and column number to the end
            OMButton MainMenu11 = new OMButton(25,116);
            MainMenu11.Image = mainMenu;
            MainMenu11.FocusImage = mainMenuFocus;
            MainMenu11.Name = "MainMenu.MainMenu11";
            MainMenu11.Mode = modeType.Highlighted;
            MainMenu11.OnLongClick += new userInteraction(OnLongClick);
            MainMenu11.Tag = settings.getSetting("MainMenu.MainMenu11");
            MainMenu11.Text = settings.getSetting("MainMenu.MainMenu11.Display");
            MainMenu11.OnClick += new userInteraction(MainMenu_OnClick);
            OMButton MainMenu12 = new OMButton(353,116);
            MainMenu12.Image = mainMenu;
            MainMenu12.FocusImage = mainMenuFocus;
            MainMenu12.Name = "MainMenu.MainMenu12";
            MainMenu12.OnLongClick += new userInteraction(OnLongClick);
            MainMenu12.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu12.Tag = settings.getSetting("MainMenu.MainMenu12");
            MainMenu12.Text = settings.getSetting("MainMenu.MainMenu12.Display");
            OMButton MainMenu22 = new OMButton(353,260);
            MainMenu22.Image = mainMenu;
            MainMenu22.FocusImage = mainMenuFocus;
            MainMenu22.Name = "MainMenu.MainMenu22";
            MainMenu22.OnLongClick += new userInteraction(OnLongClick);
            MainMenu22.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu22.Tag = settings.getSetting("MainMenu.MainMenu22");
            MainMenu22.Text = settings.getSetting("MainMenu.MainMenu22.Display");
            OMButton MainMenu32 = new OMButton(353,400);
            MainMenu32.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu32.Image = mainMenu;
            MainMenu32.FocusImage = mainMenuFocus;
            MainMenu32.Name = "MainMenu.MainMenu32";
            MainMenu32.OnLongClick += new userInteraction(OnLongClick);
            MainMenu32.Tag = settings.getSetting("MainMenu.MainMenu32");
            MainMenu32.Text = settings.getSetting("MainMenu.MainMenu32.Display");
            OMButton MainMenu21 = new OMButton(25,260);
            MainMenu21.Image = mainMenu;
            MainMenu21.FocusImage = mainMenuFocus;
            MainMenu21.Name = "MainMenu.MainMenu21";
            MainMenu21.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu21.OnLongClick += new userInteraction(OnLongClick);
            MainMenu21.Tag = settings.getSetting("MainMenu.MainMenu21");
            MainMenu21.Text = settings.getSetting("MainMenu.MainMenu21.Display");
            OMButton MainMenu31 = new OMButton(25,400);
            MainMenu31.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu31.Image = mainMenu;
            MainMenu31.FocusImage = mainMenuFocus;
            MainMenu31.Name = "MainMenu.MainMenu31";
            MainMenu31.OnLongClick += new userInteraction(OnLongClick);
            MainMenu31.Tag = settings.getSetting("MainMenu.MainMenu31");
            MainMenu31.Text = settings.getSetting("MainMenu.MainMenu31.Display");
            OMButton MainMenu13 = new OMButton(680,116);
            MainMenu13.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu13.Image = mainMenu;
            MainMenu13.FocusImage = mainMenuFocus;
            MainMenu13.Name = "MainMenu.MainMenu13";
            MainMenu13.OnLongClick += new userInteraction(OnLongClick);
            MainMenu13.Tag = settings.getSetting("MainMenu.MainMenu13");
            MainMenu13.Text = settings.getSetting("MainMenu.MainMenu13.Display");
            OMButton MainMenu23 = new OMButton(680,260);
            MainMenu23.Image = mainMenu;
            MainMenu23.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu23.FocusImage = mainMenuFocus;
            MainMenu23.Name = "MainMenu.MainMenu23";
            MainMenu23.OnLongClick += new userInteraction(OnLongClick);
            MainMenu23.Tag = settings.getSetting("MainMenu.MainMenu23");
            MainMenu23.Text = settings.getSetting("MainMenu.MainMenu23.Display");
            OMButton MainMenu33 = new OMButton(680,400);
            MainMenu33.Image = mainMenu;
            MainMenu33.OnClick += new userInteraction(MainMenu_OnClick);
            MainMenu33.FocusImage = mainMenuFocus;
            MainMenu33.Name = "MainMenu.MainMenu33";
            MainMenu33.Tag = settings.getSetting("MainMenu.MainMenu33");
            MainMenu33.Text = settings.getSetting("MainMenu.MainMenu33.Display");
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
            
            //We can do more then one panel too
            OMList list = new OMList(235,150,450,295);
            list.Font = new Font(FontFamily.GenericSerif, 26F);
            list.ListStyle = eListStyle.RoundedTextList;
            OMLabel title = new OMLabel(208,120,500,25);
            title.Text = "Select the panel to assign to this button";
            title.Format = textFormat.BoldShadow;
            OMButton okButton = new OMButton(550, 460, 135, 50);
            okButton.Image = theHost.getSkinImage("Full");
            okButton.FocusImage = theHost.getSkinImage("Full.Highlighted");
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
            screens = new ScreenManager(theHost.ScreenCount);
            screens.loadPanel(mainPanel);
            return eLoadStatus.LoadSuccessful;
        }

        void MainMenu_OnClick(object sender, int screen)
        {
            switch(((OMButton)sender).Tag)
            {
                case "Exit":
                    theHost.execute(eFunction.closeProgram);
                    break;
                case "":
                    return;
                default:
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(),"MainMenu");
                    if (theHost.execute(eFunction.TransitionToPanel,screen.ToString(), ((OMButton)sender).Tag)==false)
                        theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "MainMenu");
                    theHost.execute(eFunction.ExecuteTransition,screen.ToString());
                    break;
            }
        }

        void okButton_OnClick(object sender, int screen)
        {
            OMList theList=(OMList)setButton[screen];
            if (theList.SelectedIndex >= 0) //FS#3
            {
                using (PluginSettings setting = new PluginSettings())
                {
                    if (theList[theList.SelectedIndex].text == "Not Set")
                        theList[theList.SelectedIndex].text = "";
                    setting.setSetting(currentlySetting, theList[theList.SelectedIndex].text);
                    ((OMButton)screens[screen][currentlySetting]).Tag = theList[theList.SelectedIndex].text;
                    setting.setSetting(currentlySetting + ".Display", getDisplayName(theList[theList.SelectedIndex].text));
                    ((OMButton)screens[screen][currentlySetting]).Text = getDisplayName(theList[theList.SelectedIndex].text);
                }
            }
            theHost.execute(eFunction.TransitionFromSettings,screen.ToString(),"MainMenu");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu");
            theHost.execute(eFunction.ExecuteTransition,screen.ToString(),"None");
        }
        private string getDisplayName(string name)
        {
            if (name == "")
                return name;
            if (name == "Exit")
                return name;
            object pl = new object();
            theHost.getData(eGetData.GetPlugins, name, out pl);
            if (pl == null)
                return null;
            return ((IHighLevel)pl).displayName;
        }
        private void createDefaultSettings(PluginSettings settings)
        {
            settings.setSetting("MainMenu.MainMenu21", "Radio");
            settings.setSetting("MainMenu.MainMenu12", "MainMenu");
            settings.setSetting("MainMenu.MainMenu22", "Exit");
            settings.setSetting("MainMenu.MainMenu11", "Media");
            settings.setSetting("MainMenu.MainMenu13", "OBDII");
            settings.setSetting("MainMenu.MainMenu21.Display", "Radio");
            settings.setSetting("MainMenu.MainMenu12.Display", "Main Menu");
            settings.setSetting("MainMenu.MainMenu22.Display", "Exit");
            settings.setSetting("MainMenu.MainMenu11.Display", "Media");
            settings.setSetting("MainMenu.MainMenu13.Display", "OBDII");
        }

        void cancel_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.goBack,screen.ToString());
        }

        string currentlySetting;
        void OnLongClick(object sender, int screen)
        {
            currentlySetting = ((OMButton)sender).Name;
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(), "MainMenu");
            theHost.execute(eFunction.TransitionToSettings,screen.ToString(), "MainMenu");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
        }

        ScreenManager screens;
        public OMPanel loadPanel(string name,int screen)
        {
            return screens[screen];
        }

        public OMPanel loadSettings(string name,int screen)
        {
            object o = new object();
            theHost.getData(eGetData.GetPlugins, "", out o);
            if (o == null)
                return null;
            OMList list = ((OMList)setButton[0]);
            list.Clear();
            list.Add("Not Set");
            list.Add("Exit");
            Type hl = typeof(IHighLevel); //performance improvement
            foreach (IBasePlugin b in (List<IBasePlugin>)o)
            {
                if (hl.IsInstanceOfType(b))
                    if ((b.pluginName != "UI") && (b.pluginName != "MainMenu")) //FS#7
                        list.Add(b.pluginName);
            }
            return setButton;
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
            screens.Dispose();
            setButton = null;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
