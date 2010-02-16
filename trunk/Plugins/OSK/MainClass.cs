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
*********************************************************************************
Example Name: Serialization Example (On Screen Keyboard)
Example Created: Sept. 10, 2009
Last Modified: Nov. 8, 2009
Created by: Justin Domnitz
*********************************************************************************/
using System;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace OpenMobile
{   //All High Level plugins should implement the IHighLevel interface
    public sealed class MainClass:IHighLevel
    {
        //Here we create the panel that we will load the controls onto (like a windows form)
        ScreenManager manager;
        ScreenManager symManager;
        private bool caps = true;

        public OMPanel loadPanel(string name,int screen)
        {
            //When requested we supply the panel
            //NOTE: if it has not yet finished initializing it will be null
            switch(name)
            {
                case "SYM":
                    return symManager[screen];
                default:
                    return manager[screen];
            }
        }

        public OMPanel loadSettings(string name,int screen)
        {
            //Throwing a not implemented exception is ok for settings panels
            throw new NotImplementedException();
        }

        #region IBasePlugin Members
        //Here we supply all the basic info about the plugin
        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            //All on screen keyboards should use this name (as recommended in the tips section of the wiki)
            get { return "OSK"; }
        }
        public string displayName
        {
            get { return "On Screen Keyboard"; }
        }
        public float pluginVersion
        {
            get { return 0.3F; }
        }

        public string pluginDescription
        {
            get { return "An English On Screen Keyboard"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        //This is where the magic happens
        private IPluginHost theHost;
        
        //When initialize is called we need to get everything ready
        public eLoadStatus initialize(IPluginHost host)
        {
            //This includes saving a reference to the Plugin Host if we need it later
            theHost=host;
            manager = new ScreenManager(theHost.ScreenCount);
            //And loading up the controls.  In this example we load the controls using serialization.
            OMPanel regularKeyboard = OpenMobile.Framework.Serializer.deserializePanel(Path.Combine(theHost.SkinPath, "OSK.xml"), host);
            if (regularKeyboard == null)
                return eLoadStatus.LoadFailedUnloadRequested;
            symManager = new ScreenManager(theHost.ScreenCount);
            OMPanel symKeyboard = OpenMobile.Framework.Serializer.deserializePanel(Path.Combine(theHost.SkinPath, "SYM.xml"),host);
            if (symKeyboard == null)
                return eLoadStatus.LoadFailedUnloadRequested;
            //Here we create a handle to hook the button presses
            userInteraction handler = new userInteraction(MainClass_OnClick);
            //Now we loop through each control and hook its event
            for (int i=0;i<regularKeyboard.controlCount;i++)
                if (regularKeyboard[i].GetType()==typeof(OMButton))
                    ((OMButton)regularKeyboard[i]).OnClick += handler;
            for (int i = 0; i < symKeyboard.controlCount; i++)
                if (regularKeyboard[i].GetType() == typeof(OMButton))
                    ((OMButton)symKeyboard[i]).OnClick += handler;
            //Here we hook the keyboard in case characters are typed
            theHost.OnKeyPress += new KeyboardEvent(theHost_OnKeyPress);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            //And then load the panel into the screen manager
            manager.loadPanel(regularKeyboard);
            symManager.loadPanel(symKeyboard);
            //And when everything is all ready to go...we return true
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
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
        }

        //Add physical keyboard input to the textbox
        bool theHost_OnKeyPress(keypressType type, System.Windows.Forms.KeyEventArgs arg)
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                OMTextBox text = (OMTextBox)manager[i]["Text"];
                if (text.hooked() == false)
                    continue;
                if (type == keypressType.KeyUp)
                {
                    if ((arg.KeyValue > 64) && (arg.KeyValue < 91))
                        if ((caps == true) || (arg.Shift == true))
                            text.Text += arg.KeyCode.ToString();
                        else
                            text.Text += arg.KeyCode.ToString().ToLower();
                    else{
                        switch (arg.KeyValue)
                        {
                            case 13:
                                theHost.execute(eFunction.userInputReady, i.ToString(), "OSK", text.Text);
                                break;
                            case 32:
                                text.Text += " ";
                                break;
                            case 8:
                                if (text.Text.Length > 0)
                                    text.Text = text.Text.Remove(text.Text.Length - 1);
                                break;
                            case 190:
                                text.Text += '.';
                                break;
                            case 186:
                                if (arg.Shift == true)
                                    text.Text += ':';
                                else
                                    text.Text += ';';
                                break;
                            case 188:
                                text.Text += ',';
                                break;
                            case 191:
                                if (arg.Shift == true)
                                    text.Text += '?';
                                else
                                    text.Text += '/';
                                break;
                            case 220:
                                if (arg.Shift == false)
                                    text.Text += '\\';
                                break;
                            default:
                                return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        //Each time a button is pressed we add its character to the textbox
        void MainClass_OnClick(object sender, int screen)
        {
            OMTextBox text = (OMTextBox)manager[screen]["Text"];
            string Text=((OMButton)sender).Text;
            if ((Text == "OK") || (Text == "ok"))
            {
                theHost.execute(eFunction.userInputReady, screen.ToString(), "OSK", text.Text);
                text.Text = "";
            }
            else if ((Text == "DEL") || (Text == "del"))
            {
                if (text.Text.Length > 0)
                    text.Text = text.Text.Remove(text.Text.Length - 1);
            }
            else if ((Text == "SPACE") || (Text == "space"))
                text.Text += ' ';
            else if (Text.ToLower() == "sym")
            {
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OSK", "SYM");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
            }
            else if (Text.ToLower() == "abc")
            {
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(),"OSK","SYM");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(),"None");
            }
            else
            {
                OMPanel regularKeyboard = manager[screen];
                if ((Text == "CAPS") || (Text == "Caps"))
                {
                    for (int i = 0; i < regularKeyboard.controlCount; i++)
                    {
                        if (regularKeyboard[i].GetType() == typeof(OMButton))
                        {
                            ((OMButton)regularKeyboard[i]).Text = ((OMButton)regularKeyboard[i]).Text.ToLower();
                            caps = false;
                        }
                    }
                    return;
                }
                if (Text == "caps")
                {
                    for (int i = 0; i < regularKeyboard.controlCount; i++)
                    {
                        if (regularKeyboard[i].GetType() == typeof(OMButton))
                        {
                            ((OMButton)regularKeyboard[i]).Text = ((OMButton)regularKeyboard[i]).Text.ToUpper();
                            caps = true;
                        }
                    }
                    return;
                }
                text.Text += Text;
            }
        }

        #endregion

        #region IDisposable Members

        //Dispose of any objects you created
        public void Dispose()
        {
            if (symManager!=null)
                symManager.Dispose();
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
