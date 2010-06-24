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
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using OpenMobile.Plugin;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Miscellaneous helper functions
    /// </summary>
    public static class General
    {
        /// <summary>
        /// Synchronously get keyboard input from the On Screen Keyboard
        /// </summary>
        public sealed class getKeyboardInput
        {
            string theText = null;
            IPluginHost host;
            EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            int screen = -1;

            /// <summary>
            /// Initializes the Keyboard Input class
            /// </summary>
            /// <param name="theHost"></param>
            public getKeyboardInput(IPluginHost theHost)
            {
                host = theHost;
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginname"></param>
            /// <returns></returns>
            public string getText(int screen, string pluginname)
            {
                return getText(screen, pluginname, "");
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            public string getText(int screen,string pluginname,string panelName)
            {
                SystemEvent ev=new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                this.screen = screen;
                wait.Reset();
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(),"OSK","OSK") == false)
                {
                    return null;
                }
                host.execute(eFunction.TransitionFromPanel, screen.ToString(), pluginname,panelName);
                host.execute(eFunction.ExecuteTransition,screen.ToString());
                wait.WaitOne();
                host.OnSystemEvent -= ev;
                host.execute(eFunction.TransitionFromAny, screen.ToString());
                host.execute(eFunction.TransitionToPanel, screen.ToString(), pluginname, panelName);
                host.execute(eFunction.ExecuteTransition, screen.ToString());
                return theText;
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the number entered
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginname"></param>
            /// <returns></returns>
            public string getNumber(int screen, string pluginname)
            {
                return getNumber(screen, pluginname, "");
            }

            /// <summary>
            /// Loads the On Screen Keyboard and then returns the number entered
            /// </summary>
            /// <returns></returns>
            public string getNumber(int screen, string pluginname, string panelName)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                this.screen = screen;
                wait.Reset();
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(), "OSK", "NUMOSK") == false)
                {
                    return null;
                }
                host.execute(eFunction.TransitionFromPanel, screen.ToString(), pluginname,panelName);
                host.execute(eFunction.ExecuteTransition, screen.ToString());
                wait.WaitOne();
                host.OnSystemEvent -= ev;
                host.execute(eFunction.TransitionFromAny, screen.ToString());
                host.execute(eFunction.TransitionToPanel, screen.ToString(), pluginname, panelName);
                host.execute(eFunction.ExecuteTransition, screen.ToString());
                return theText;
            }
            void theHost_OnSystemEvent(eFunction function, string arg1, string arg2,string arg3)
            {
                if (function == eFunction.userInputReady)
                {
                    if (arg1 == screen.ToString())
                    {
                        if (arg2 == "OSK")
                        {
                            theText = arg3;
                            wait.Set();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// The equivalent of an Open File Dialog
        /// </summary>
        public sealed class getFilePath
        {
            string thePath = null;
            IPluginHost host;
            EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            int screen=-1;

            /// <summary>
            /// Initializes the Directory Browser class
            /// </summary>
            /// <param name="theHost"></param>
            public getFilePath(IPluginHost theHost)
            {
                host = theHost;
            }
            /// <summary>
            /// Loads a file selection plugin
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginName"></param>
            /// <param name="panelName"></param>
            /// <returns></returns>
            public string getFile(int screen, string pluginName, string panelName)
            {
                return getFile(screen, pluginName, panelName, "");
            }
            /// <summary>
            /// Loads a file selection plugin
            /// </summary>
            /// <returns></returns>
            public string getFile(int screen,string pluginName,string panelName,string startPath)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                wait.Reset();
                this.screen = screen;
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMDir",startPath) == false)
                {
                    return null;
                }
                host.execute(eFunction.TransitionFromPanel, screen.ToString(),pluginName);
                host.execute(eFunction.ExecuteTransition, screen.ToString());
                wait.WaitOne();
                host.OnSystemEvent -= ev;
                host.execute(eFunction.goBack, screen.ToString());
                return thePath;
            }
            /// <summary>
            /// Loads a folder selection plugin and returns the selected folder
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginName"></param>
            /// <param name="panelName"></param>
            /// <returns></returns>
            public string getFolder(int screen, string pluginName,string panelName)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                wait.Reset();
                this.screen = screen;
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMDir","Folder") == false)
                {
                    return null;
                }
                host.execute(eFunction.TransitionFromPanel, screen.ToString(), pluginName);
                host.execute(eFunction.ExecuteTransition, screen.ToString());
                wait.WaitOne();
                host.OnSystemEvent -= ev;
                host.execute(eFunction.goBack, screen.ToString());
                return thePath;
            }

            void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
            {
                if (function == eFunction.userInputReady)
                {
                    if (arg1 == screen.ToString())
                    {
                        if (arg2 == "Dir")
                        {
                            thePath = arg3;
                            wait.Set();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert a string of bytes represented in hex into a byte array
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string Hex)
        {
            byte[] Bytes = new byte[Hex.Length / 2];
            int[] HexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };

            for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
            {
                Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |
                                  HexValue[Char.ToUpper(Hex[i + 1]) - '0']);
            }

            return Bytes;
        }
        /// <summary>
        /// Escapes a string for sending to an SQL Database
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string escape(string s)
        {
            if (s == null)
                return "";
            return s.Replace("'", "''");
        }
        /// <summary>
        /// Returns a List of plugins matching the given type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static List<string> getPluginsOfType(Type t, IPluginHost host)
        {
            object o=new object();
            host.getData(eGetData.GetPlugins, "", out o);
            if (o == null)
                return null;
            List<string> ret = new List<string>();
            foreach (IBasePlugin b in (List<IBasePlugin>)o)
            {
                if (t.IsInstanceOfType(b))
                {
                    ret.Add(b.pluginName);
                }
            }
            return ret;
        }
        /// <summary>
        /// Convert a string representation of an eFunction into an eFunction
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static eFunction stringToFunction(string functionName)
        {
            if ((functionName == null) || (functionName.Length == 0))
                return eFunction.None;
            try
            {
                return (eFunction)Enum.Parse(typeof(eFunction), functionName, true);
            }
            catch (System.ArgumentException)
            {
                return eFunction.None;
            }
        }
    }
}
