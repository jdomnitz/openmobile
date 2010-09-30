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
Example Name: Serialization Example (On Screen Keyboard)
Example Created: Sept. 10, 2009
Last Modified: Nov. 8, 2009
Created by: Justin Domnitz
*********************************************************************************/
using System;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Input;

namespace OpenMobile
{   //All High Level plugins should implement the IHighLevel interface
    public sealed class MainClass:IHighLevel
    {
        //Here we create the panel that we will load the controls onto (like a windows form)
        ScreenManager manager;
        private bool caps = false;

        public OMPanel loadPanel(string name,int screen)
        {
            if (manager == null)
                return null;
            if (Console.CapsLock)
                setUppercase(screen);
            else
                setLowercase(screen);
            if (name == "")
            {
                name = "OSK";
                ((OMTextBox)manager[screen, "OSK"]["Text"]).Flags = textboxFlags.None;
            }
            if ((name != "OSK") && (name != "NUMOSK") && (name != "SYM"))
            {
                OMTextBox tb=((OMTextBox)manager[screen, "OSK"]["Text"]);
                if ((name!=null)&&(name.StartsWith("PASSWORD|")))
                {
                    tb.Flags = textboxFlags.Password;
                    name = name.Substring(9);
                }
                else
                    tb.Flags = textboxFlags.None;
                tb.Text = name;
                name = "OSK";
            }
            return manager[screen,name];
        }

        public Settings loadSettings()
        {
            return null;
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
            get { return 0.7F; }
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
            OMPanel regularKeyboard = OpenMobile.Framework.Serializer.deserializePanel(Path.Combine(theHost.SkinPath, "OSK.xml"), host)[0];
            if (regularKeyboard == null)
                return eLoadStatus.LoadFailedUnloadRequested;
            OMPanel symKeyboard = OpenMobile.Framework.Serializer.deserializePanel(Path.Combine(theHost.SkinPath, "SYM.xml"),host)[0];
            if (symKeyboard == null)
                return eLoadStatus.LoadFailedUnloadRequested;
            OMPanel numKeyboard = OpenMobile.Framework.Serializer.deserializePanel(Path.Combine(theHost.SkinPath, "NUM.xml"), host)[0];
            if (numKeyboard == null)
                return eLoadStatus.LoadFailedUnloadRequested;
            //Here we create a handle to hook the button presses
            userInteraction handler = new userInteraction(MainClass_OnClick);
            //Now we loop through each control and hook its event
            for (int i=0;i<regularKeyboard.controlCount;i++)
                if (regularKeyboard[i].GetType()==typeof(OMButton))
                    ((OMButton)regularKeyboard[i]).OnClick += handler;
            for (int i = 0; i < symKeyboard.controlCount; i++)
                if (symKeyboard[i].GetType() == typeof(OMButton))
                    ((OMButton)symKeyboard[i]).OnClick += handler;
            for (int i = 0; i < numKeyboard.controlCount; i++)
                if (numKeyboard[i].GetType() == typeof(OMButton))
                    ((OMButton)numKeyboard[i]).OnClick += handler;
                else
                    ((OMTextBox)numKeyboard[i]).Flags = textboxFlags.NumericOnly;
            //Here we hook the keyboard in case characters are typed
            theHost.OnKeyPress += new KeyboardEvent(theHost_OnKeyPress);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            //And then load the panel into the screen manager
            manager.loadPanel(regularKeyboard);
            manager.loadPanel(symKeyboard);
            manager.loadPanel(numKeyboard);
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
        bool theHost_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg)
        {  
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                OMTextBox text = (OMTextBox)manager[i]["Text"];
                if (!text.hooked())
                    text = (OMTextBox)manager[i, "NUMOSK"]["Text"];
                if (text.hooked() == false)
                    continue;
                if (arg.CapsLock!=caps)
                    if (arg.CapsLock)
                        setUppercase(i);
                    else
                        setLowercase(i);
                if (type == eKeypressType.KeyUp)
                {
                    if ((arg.KeyCode > 82) && (arg.KeyCode < 109))
                        if ((caps == true) || (arg.Shift == true))
                            text.Text += arg.Key.ToString();
                        else
                            text.Text += arg.Key.ToString().ToLower();
                    else if ((arg.KeyCode >= 109) && (arg.KeyCode < 120))
                    {
                        if (arg.Shift == true)
                        {
                            switch ((Key)Enum.Parse(typeof(Key),arg.Key.ToString()))
                            {
                                case Key.Number1:
                                    text.Text += "!";
                                    break;
                                case Key.Number2:
                                    text.Text += "@";
                                    break;
                                case Key.Number3:
                                    text.Text += "#";
                                    break;
                                case Key.Number4:
                                    text.Text += "$";
                                    break;
                                case Key.Number5:
                                    text.Text += "%";
                                    break;
                                case Key.Number6:
                                    text.Text += "^";
                                    break;
                                case Key.Number7:
                                    text.Text += "&";
                                    break;
                                case Key.Number8:
                                    text.Text += "*";
                                    break;
                                case Key.Number9:
                                    text.Text += "(";
                                    break;
                                case Key.Number0:
                                    text.Text += ")";
                                    break;
                            }
                        }
                        else
                            text.Text += arg.Key.ToString().Replace("Number", "");
                    }
                    else
                    {
                        switch (arg.KeyCode)
                        {
                            case 49:
                                //Ignore enter-could be any button //theHost.execute(eFunction.userInputReady, i.ToString(), "OSK", text.Text);
                                return false;//break;
                            case 60:
                                return false;
                            case 51:
                                text.Text += " ";
                                break;
                            case 53:
                                if (text.Text.Length > 0)
                                    text.Text = text.Text.Remove(text.Text.Length - 1);
                                break;
                            case 127:
                                if(arg.Shift==true)
                                    text.Text += '>';
                                else
                                    text.Text += '.';
                                break;
                            case 124:
                                if (arg.Shift == true)
                                    text.Text += ':';
                                else
                                    text.Text += ';';
                                break;
                            case 121:
                                if (arg.Shift == true)
                                    text.Text += '+';
                                else
                                    text.Text += '=';
                                break;
                            case 122:
                                if (arg.Shift == true)
                                    text.Text += '{';
                                else
                                    text.Text += '[';
                                break;
                            case 123:
                                if (arg.Shift == true)
                                    text.Text += '}';
                                else
                                    text.Text += ']';
                                break;
                            case 125:
                                if (arg.Shift == true)
                                    text.Text += '"';
                                else
                                    text.Text += '\'';
                                break;
                            case 126:
                                if (arg.Shift == true)
                                    text.Text += '<';
                                else
                                    text.Text += ',';
                                break;
                            case 120:
                                if (arg.Shift == true)
                                    text.Text += '_';
                                else
                                    text.Text += '-';
                                break;
                            case 128:
                                if (arg.Shift == true)
                                    text.Text += '?';
                                else
                                    text.Text += '/';
                                break;
                            case 129:
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
        void MainClass_OnClick(OMControl sender, int screen)
        {
            OMTextBox text = (OMTextBox)manager[screen]["Text"];
            if (text.containingScreen() != screen)
                text = (OMTextBox)manager[screen,"NUMOSK"]["Text"];
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
                if ((Text == "CAPS") || (Text == "Caps"))
                {
                    setLowercase(screen);
                    return;
                }
                if (Text == "caps")
                {
                    setUppercase(screen);
                    return;
                }
                text.Text += Text;
            }
        }

        private void setLowercase(int screen)
        {
            OMPanel regularKeyboard = manager[screen];
            for (int i = 0; i < regularKeyboard.controlCount; i++)
            {
                if (regularKeyboard[i].GetType() == typeof(OMButton))
                {
                    ((OMButton)regularKeyboard[i]).Text = ((OMButton)regularKeyboard[i]).Text.ToLower();
                    caps = false;
                }
            }
        }

        private void setUppercase(int screen)
        {
            OMPanel regularKeyboard = manager[screen];
            for (int i = 0; i < regularKeyboard.controlCount; i++)
            {
                if (regularKeyboard[i].GetType() == typeof(OMButton))
                {
                    ((OMButton)regularKeyboard[i]).Text = ((OMButton)regularKeyboard[i]).Text.ToUpper();
                    caps = true;
                }
            }
        }

        #endregion

        #region IDisposable Members

        //Dispose of any objects you created
        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
