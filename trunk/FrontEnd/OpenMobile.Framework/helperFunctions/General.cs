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

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Miscellaneous helper functions
    /// </summary>
    public static class General
    {
        /* Code removed due to invalid API
        /// <summary>
        /// Translates a place name, location name or address string into a full Location
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Location lookupLocation(string name)
        {
            Location ret = new Location();
            XmlDocument doc = new XmlDocument();
            string locale = System.Globalization.CultureInfo.CurrentCulture.Name;
            //doc.Load(@"http://where.yahooapis.com/geocode?q=" + Net.Network.urlEncode(name) + "&locale=" + locale);
            doc.Load(@"http://where.yahooapis.com/geocode?q=" + name + "&locale=" + locale);
            ret.Name = name;
            foreach (XmlNode n in doc.DocumentElement.ChildNodes)
            {
                switch (n.Name)
                {
                    case "Error":
                        if (n.InnerText != "0")
                            return ret;
                        break;
                    case "Found":
                        if (n.InnerText != "1")
                            return ret;
                        break;
                    case "Result":
                        foreach (XmlNode n2 in n.ChildNodes)
                        {
                            switch (n2.Name)
                            {
                                case "latitude":
                                    if (!string.IsNullOrEmpty(n2.InnerText))
                                        ret.Latitude = float.Parse(n2.InnerText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "longitude":
                                    if (!string.IsNullOrEmpty(n2.InnerText))
                                        ret.Longitude = float.Parse(n2.InnerText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "house":
                                    ret.Street = n2.InnerText + " " + ret.Street;
                                    break;
                                case "street":
                                    ret.Street += n2.InnerText;
                                    break;
                                case "postal":
                                    if (ret.Zip == "")
                                        ret.Zip = n2.InnerText;
                                    break;
                                case "uzip":
                                    if (ret.Zip == "")
                                        ret.Zip = n2.InnerText;
                                    break;
                                case "city":
                                    ret.City = n2.InnerText;
                                    break;
                                case "state":
                                    ret.State = n2.InnerText;
                                    break;
                                case "country":
                                    ret.Country = n2.InnerText;
                                    break;
                            }
                        }
                        break;
                }
            }
            ret.Street = ret.Street.Trim();
            return ret;
        }
        */

        /// <summary>
        /// Synchronously get keyboard input from the On Screen Keyboard
        /// </summary>
        /// 
        [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
        public sealed class getKeyboardInput
        {
            string theText = null;
            EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            int screen = -1;
            bool bail;
            bool armed;

            /// <summary>
            /// Initializes the Keyboard Input class
            /// </summary>
            public getKeyboardInput()
            {
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginname"></param>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getText(int screen, string pluginname)
            {
                return getText(screen, pluginname, String.Empty);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getText(int screen, string pluginname, string panelName)
            {
                return getText(screen, pluginname, new string[] { panelName });
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getPassword(int screen, string pluginname, string defaultValue)
            {
                return getPassword(screen, pluginname, new string[] { String.Empty }, defaultValue);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            public string getText(int screen, string pluginname, string panelName, string defaultValue)
            {
                return getText(screen, pluginname, new string[] { panelName }, defaultValue);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getPassword(int screen, string pluginname, string panelName, string defaultValue)
            {
                return getPassword(screen, pluginname, new string[] { panelName }, defaultValue);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getText(int screen, string pluginname, string[] panelNames)
            {
                return getText(screen, pluginname, panelNames, "OSK");
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getPassword(int screen, string pluginname, string[] panelNames, string defaultValue)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                BuiltInComponents.Host.OnSystemEvent += ev;
                this.screen = screen;

                bool error = false;
                BuiltInComponents.Host.execute(eFunction.TransitionFromAny, screen.ToString());
                error = !BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OSK", "PASSWORD|" + defaultValue);
                wait.Reset();
                armed = true;
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                if (!error)
                    wait.WaitOne();
                BuiltInComponents.Host.OnSystemEvent -= ev;
                if (bail)
                    return null;
                BuiltInComponents.Host.execute(eFunction.TransitionFromAny, screen.ToString());
                foreach (string panelName in panelNames)
                    BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), pluginname, panelName);
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                return theText;
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getText(int screen, string pluginname, string[] panelNames, string defaultValue)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                BuiltInComponents.Host.OnSystemEvent += ev;
                this.screen = screen;
                bool error = false;
                BuiltInComponents.Host.execute(eFunction.TransitionFromAny, screen.ToString());
                error = !BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OSK", defaultValue);
                wait.Reset();
                armed = true;
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                if (!error)
                    wait.WaitOne();
                BuiltInComponents.Host.OnSystemEvent -= ev;
                if (bail)
                    return null;
                BuiltInComponents.Host.execute(eFunction.TransitionFromAny, screen.ToString());
                foreach (string panelName in panelNames)
                    BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), pluginname, panelName);
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                return theText;
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the number entered
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginname"></param>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getNumber(int screen, string pluginname)
            {
                return getNumber(screen, pluginname, String.Empty);
            }

            /// <summary>
            /// Loads the On Screen Keyboard and then returns the number entered
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use OpenMobile.helperFunctions.OSK instead")]
            public string getNumber(int screen, string pluginname, string panelName)
            {
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                BuiltInComponents.Host.OnSystemEvent += ev;
                this.screen = screen;
                wait.Reset();
                armed = true;
                if (BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OSK", "NUMOSK") == false)
                {
                    return null;
                }
                BuiltInComponents.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), pluginname, panelName);
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                wait.WaitOne();
                BuiltInComponents.Host.OnSystemEvent -= ev;
                if (bail)
                    return null;
                BuiltInComponents.Host.execute(eFunction.TransitionFromAny, screen.ToString());
                BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), pluginname, panelName);
                BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString());
                return theText;
            }

            void theHost_OnSystemEvent(eFunction function, object[] args)
            {
                if (function == eFunction.userInputReady)
                {
                    if (args != null && args.Length >= 3)
                    {
                        if (args[0] as string == screen.ToString())
                        {
                            if (args[1] as string == "OSK")
                            {
                                theText = args[2] as string;
                                wait.Set();
                            }
                        }
                    }
                }
                else if ((function == eFunction.TransitionFromAny) && (int.Parse(args[0] as string) == screen))
                {
                    if (armed)
                    {
                        bail = true;
                        wait.Set();
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
            int screen = -1;

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
                return getFile(screen, pluginName, panelName, String.Empty);
            }
            /// <summary>
            /// Loads a file selection plugin
            /// </summary>
            /// <returns></returns>
            public string getFile(int screen, string pluginName, string panelName, string startPath)
            {
                if (host == null)
                    return null;
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                wait.Reset();
                this.screen = screen;
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMDir", startPath) == false)
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
            /// <summary>
            /// Loads a folder selection plugin and returns the selected folder
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="pluginName"></param>
            /// <param name="panelName"></param>
            /// <returns></returns>
            public string getFolder(int screen, string pluginName, string panelName)
            {
                if (host == null)
                    return null;
                SystemEvent ev = new SystemEvent(theHost_OnSystemEvent);
                host.OnSystemEvent += ev;
                wait.Reset();
                this.screen = screen;
                if (host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMDir", "Folder") == false)
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

            void theHost_OnSystemEvent(eFunction function, object[] args)
            {
                if (function == eFunction.userInputReady)
                {
                    if (OpenMobile.helperFunctions.Params.IsParamsValid(args, 3))
                    {
                        if (OpenMobile.helperFunctions.Params.GetParam<string>(args, 0) == screen.ToString())
                        {
                            if (OpenMobile.helperFunctions.Params.GetParam<string>(args, 1) == "Dir")
                            {
                                thePath = OpenMobile.helperFunctions.Params.GetParam<string>(args, 2);
                                wait.Set();
                            }
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
            if (Hex == null)
                return new byte[0];
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
                return String.Empty;
            return s.Replace("'", "''");
        }

        /// <summary>
        /// Convert a string representation of an eFunction into an eFunction
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static eFunction stringToFunction(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
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

        /// <summary>
        /// Extracts info from an exception into a string
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }

        public static void SetUTCTime(DateTime time)
        {
            if (Configuration.RunningOnWindows)
                OpenMobile.Framework.Windows.SetTime(time);
        }

    }

    public static class DataHelpers
    {
        /// <summary>
        /// Gets a specified type from an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T GetDataFromObject<T>(object o)
        {
            return GetDataFromObject<T>(o, default(T));
        }

        /// <summary>
        /// Gets a specified type from an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetDataFromObject<T>(object o, object defaultValue)
        {
            if (o is T)
            {   // Object is of correct type, return data
                return (T)o;
            }
            else
            {
                // Convert data
                try
                {
                    return (T)Convert.ChangeType(o, typeof(T));
                }
                catch
                {
                    return (T)Convert.ChangeType(defaultValue, typeof(T));
                }
            }
        }

        /// <summary>
        /// Extracts the screen number from a string like this "Screen0" returns 0 if it fails
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetScreenFromString(string s)
        {
            int screen = 0;
            int.TryParse(s.Substring(6), out screen);
            return screen;
        }
    }

    public static class Arguments
    {
        /// <summary>
        /// Extracts and removes the screen number part from an argument string.
        /// <para>NB! Returns -1 if no screen number is present</para>
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static int GetScreenFromArg(ref string arg)
        {
            // Extract screen number (if any). Must be written as {0} in the first three caracthers in the string
            // where 0 is the screen number, missing command means send to all screens.
            string Command = arg.Substring(0, 3);
            int Screen = -1;
            if ((Command.Contains("{")) && (Command.Contains("}")))
            {   // Screen exists extract info
                if (!int.TryParse(Command.Substring(1, 1), out Screen))
                    return -1;
                // Remove command from orginal message
                arg = arg.Substring(3, arg.Length - 3);
            }
            return Screen;
        }

        /// <summary>
        /// Extracts the DataType from an argument string
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static eDataType GetDataTypeFromArg(string arg)
        {
            return EnumUtils.ParseEnum<eDataType>(arg);
        }
    }

    public static class EnumUtils
    {
        public static T ParseEnum<T>(string value, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return defaultValue;

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
            }
            return defaultValue;
        }
        public static T ParseEnum<T>(string value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return default(T);

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
            }
            return default(T); 
        }
    }

    /// <summary>
    /// Bit access methods
    /// </summary>
    namespace BitAccess
    {
        public static class bit
        {
            /// <summary>
            /// Returns the amount of bit set in a uint
            /// </summary>
            /// <param name="x">value to count bits in</param>
            /// <returns>amount of bit set</returns>
            public static int Count(uint x)
            {
                int b = 0;
                if (x > 1)
                    b = 1;
                while ((x &= (x - 1)) != 0)
                    b++;
                return b;
            }
        }
    }

    /// <summary>
    /// Plugin methods
    /// </summary>
    namespace Plugins
    {
        static public class Plugins
        {
            /// <summary>
            /// Returns a List of plugins matching the given type
            /// </summary>
            /// <param name="t"></param>
            /// <param name="host"></param>
            /// <returns>List of plugin names</returns>
            public static List<string> getPluginsOfType(Type t, IPluginHost host)
            {
                object o = new object();
                host.getData(eGetData.GetPlugins, String.Empty, out o);
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
            /// Returns a List of plugins names matching the given type
            /// </summary>
            /// <param name="PluginLevel"></param>
            /// <returns>List of plugins</returns>
            public static List<T> getPluginsOfType<T>(PluginLevels PluginLevel)
            {
                object o = new object();
                BuiltInComponents.Host.getData(eGetData.GetPlugins, String.Empty, out o);
                if (o == null)
                    return null;
                List<T> ret = new List<T>();
                PluginLevels Level = PluginLevels.Normal;
                foreach (IBasePlugin b in (List<IBasePlugin>)o)
                {
                    if (typeof(T).IsInstanceOfType(b))
                    {
                        // Check plugin level attribute flags
                        PluginLevel[] a = (PluginLevel[])b.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                        Level = PluginLevels.Normal; // Default
                        if (a.Length > 0)
                            Level = a[0].TypeOfPlugin;
                        if ((Level | PluginLevel) == PluginLevel)
                            ret.Add((T)b);
                    }
                }
                return ret;
            }
            /// <summary>
            /// Returns a List of plugins names matching the given type
            /// </summary>
            /// <returns>List of plugins</returns>
            public static List<IBasePlugin> getPluginsOfType<T>()
            {
                List<IBasePlugin> plugins = BuiltInComponents.Host.getData(eGetData.GetPlugins, String.Empty) as List<IBasePlugin>;
                if (plugins == null)
                    return null;
                return plugins.FindAll(x => typeof(T).IsInstanceOfType(x));
            }

            /// <summary>
            /// Returns a List of plugins names matching the given type
            /// </summary>
            /// <param name="t"></param>
            /// <param name="host"></param>
            /// <param name="PluginLevel"></param>
            /// <returns>List of plugin names</returns>
            public static List<string> getPluginsNameOfType(Type t, IPluginHost host, PluginLevels PluginLevel)
            {
                object o = new object();
                host.getData(eGetData.GetPlugins, String.Empty, out o);
                if (o == null)
                    return null;
                List<string> ret = new List<string>();
                PluginLevels Level = PluginLevels.Normal;
                foreach (IBasePlugin b in (List<IBasePlugin>)o)
                {
                    if (t.IsInstanceOfType(b))
                    {
                        // Check plugin level attribute flags
                        PluginLevel[] a = (PluginLevel[])b.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                        Level = PluginLevels.Normal; // Default
                        if (a.Length > 0)
                            Level = a[0].TypeOfPlugin;
                        if ((Level | PluginLevel) == PluginLevel)
                            ret.Add(b.pluginName);
                    }
                }
                return ret;
            }

            /// <summary>
            /// Returns a List of plugins display names (only normal type plugins)
            /// </summary>
            /// <returns>List of plugin names</returns>
            public static List<string> getPluginsDisplayName()
            {
                return getPluginsDisplayName(PluginLevels.Normal);
            }
            /// <summary>
            /// Returns a List of plugins display names matching the given plugin level
            /// </summary>
            /// <param name="PluginLevel">Level/type of plugin to get</param>
            /// <returns>List of plugin names</returns>
            public static List<string> getPluginsDisplayName(PluginLevels PluginLevel)
            {
                object o = new object();
                BuiltInComponents.Host.getData(eGetData.GetPlugins, String.Empty, out o);
                if (o == null)
                    return null;
                List<string> ret = new List<string>();
                PluginLevels Level = PluginLevels.Normal;
                Type t = typeof(IHighLevel);
                foreach (IBasePlugin b in (List<IBasePlugin>)o)
                {
                    if (t.IsInstanceOfType(b))
                    {
                        // Check plugin level attribute flags
                        PluginLevel[] a = (PluginLevel[])b.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                        Level = PluginLevels.Normal; // Default
                        if (a.Length > 0)
                            Level = a[0].TypeOfPlugin;
                        if ((Level | PluginLevel) == PluginLevel)
                            ret.Add(((IHighLevel)b).displayName);
                    }
                }
                return ret;
            }

            /// <summary>
            /// Gets the pluginlevel for a specific plugin
            /// </summary>
            /// <param name="PluginName"></param>
            /// <returns></returns>
            public static PluginLevels getPluginLevel(string PluginName)
            {
                IBasePlugin plugin = BuiltInComponents.Host.getData(eGetData.GetPlugins, PluginName) as IBasePlugin;
                if (plugin == null)
                    return PluginLevels.Normal;

                // Check plugin level attribute flags
                PluginLevel[] a = (PluginLevel[])plugin.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                if (a == null || a.Length == 0)
                    return PluginLevels.Normal;
                return a[0].TypeOfPlugin;
            }

            /// <summary>
            /// Checks if a plugins required OS configuration flags (set as attributes) is valid when compared to the current configuration flags
            /// </summary>
            /// <param name="Plugin"></param>
            /// <returns></returns>
            public static bool IsPluginOSConfigurationFlagsValid(IBasePlugin Plugin)
            {
                return IsPluginOSSupportedConfigurationFlagsValid(Plugin) && IsPluginOSRequiredConfigurationFlagsValid(Plugin);
            }

            /// <summary>
            /// Checks if a plugins required OS configuration flags (set as attributes) is valid when compared to the current configuration flags
            /// </summary>
            /// <param name="Plugin"></param>
            /// <returns></returns>
            public static bool IsPluginOSRequiredConfigurationFlagsValid(IBasePlugin Plugin)
            {
                // Check OS Configuration attribute flags
                RequiredOSConfigurations[] a = (RequiredOSConfigurations[])Plugin.GetType().GetCustomAttributes(typeof(RequiredOSConfigurations), true);
                OSConfigurationFlags OSConfFlags = OSConfigurationFlags.Any; // Default
                if (a.Length > 0)
                    OSConfFlags = a[0].ConfigurationFlags;
                else
                    return true;
                bool result = true;
                if ((OSConfFlags & OSConfigurationFlags.Embedded) == OSConfigurationFlags.Embedded)
                    if (!Configuration.RunningOnEmbedded) result = false;
                if ((OSConfFlags & OSConfigurationFlags.Linux) == OSConfigurationFlags.Linux)
                    if (!Configuration.RunningOnLinux) result = false;
                if ((OSConfFlags & OSConfigurationFlags.MacOS) == OSConfigurationFlags.MacOS)
                    if (!Configuration.RunningOnMacOS) result = false;
                if ((OSConfFlags & OSConfigurationFlags.TabletPC) == OSConfigurationFlags.TabletPC)
                    if (!Configuration.TabletPC) result = false;
                if ((OSConfFlags & OSConfigurationFlags.Unix) == OSConfigurationFlags.Unix)
                    if (!Configuration.RunningOnUnix) result = false;
                if ((OSConfFlags & OSConfigurationFlags.Windows) == OSConfigurationFlags.Windows)
                    if (!Configuration.RunningOnWindows) result = false;
                if ((OSConfFlags & OSConfigurationFlags.X11) == OSConfigurationFlags.X11)
                    if (!Configuration.RunningOnX11) result = false;
                return result;
            }

            /// <summary>
            /// Checks if a plugins supported OS configuration flags (set as attributes) is valid when compared to the current configuration flags
            /// </summary>
            /// <param name="Plugin"></param>
            /// <returns></returns>
            public static bool IsPluginOSSupportedConfigurationFlagsValid(IBasePlugin Plugin)
            {
                // Check OS Configuration attribute flags
                SupportedOSConfigurations[] a = (SupportedOSConfigurations[])Plugin.GetType().GetCustomAttributes(typeof(SupportedOSConfigurations), true);
                OSConfigurationFlags OSConfFlags = OSConfigurationFlags.Any; // Default
                
                if (a.Length > 0)
                    OSConfFlags = a[0].ConfigurationFlags;
                else
                    return true;

                bool result = true;
                if ((OSConfFlags & OSConfigurationFlags.Embedded) == OSConfigurationFlags.Embedded)
                    if (Configuration.RunningOnEmbedded) result = true;
                if ((OSConfFlags & OSConfigurationFlags.Linux) == OSConfigurationFlags.Linux)
                    if (Configuration.RunningOnLinux) result = true;
                if ((OSConfFlags & OSConfigurationFlags.MacOS) == OSConfigurationFlags.MacOS)
                    if (Configuration.RunningOnMacOS) result = true;
                if ((OSConfFlags & OSConfigurationFlags.TabletPC) == OSConfigurationFlags.TabletPC)
                    if (Configuration.TabletPC) result = true;
                if ((OSConfFlags & OSConfigurationFlags.Unix) == OSConfigurationFlags.Unix)
                    if (Configuration.RunningOnUnix) result = true;
                if ((OSConfFlags & OSConfigurationFlags.Windows) == OSConfigurationFlags.Windows)
                    if (Configuration.RunningOnWindows) result = true;
                if ((OSConfFlags & OSConfigurationFlags.X11) == OSConfigurationFlags.X11)
                    if (Configuration.RunningOnX11) result = true;
                return result;
            }

            /// <summary>
            /// Get the OS configuration flags for this system
            /// </summary>
            /// <returns></returns>
            public static OSConfigurationFlags GetSystemOSConfigurationFlags()
            {
                // Get OS Configuration attribute flags
                OSConfigurationFlags OSConfFlags = OSConfigurationFlags.Any; // Default

                if (Configuration.RunningOnEmbedded) OSConfFlags |= OSConfigurationFlags.Embedded;
                if (Configuration.RunningOnLinux) OSConfFlags |= OSConfigurationFlags.Linux;
                if (Configuration.RunningOnMacOS) OSConfFlags |= OSConfigurationFlags.MacOS;
                if (Configuration.RunningOnUnix) OSConfFlags |= OSConfigurationFlags.Unix;
                if (Configuration.RunningOnWindows) OSConfFlags |= OSConfigurationFlags.Windows;
                if (Configuration.RunningOnX11) OSConfFlags |= OSConfigurationFlags.X11;
                if (Configuration.TabletPC) OSConfFlags |= OSConfigurationFlags.TabletPC;

                return OSConfFlags;
            }


            /// <summary>
            /// Gets the OS Configuration flags
            /// </summary>
            /// <param name="Plugin"></param>
            /// <returns></returns>
            public static OSConfigurationFlags GetPluginOSConfigurationFlags(IBasePlugin Plugin)
            {
                // Check OS Configuration attribute flags
                SupportedOSConfigurations[] a = (SupportedOSConfigurations[])Plugin.GetType().GetCustomAttributes(typeof(SupportedOSConfigurations), true);
                OSConfigurationFlags OSConfFlags = OSConfigurationFlags.Any; // Default
                if (a.Length > 0)
                    OSConfFlags = a[0].ConfigurationFlags;
                return OSConfFlags;
            }

            /// <summary>
            /// Get icon for plugin (Either image or generated from font)
            /// </summary>
            /// <param name="PluginName">Name of plugin</param>
            /// <returns>Icon</returns>
            public static OImage GetPluginIcon(string PluginName)
            {
                return GetPluginIcon(PluginName, eTextFormat.Normal, Color.White, Color.White);
            }
            /// <summary>
            /// Get icon for plugin (Either image or generated from font)
            /// </summary>
            /// <param name="PluginName">Name of plugin</param>
            /// <param name="SymbolFormat">String format paramters for autogeneration of icon</param>
            /// <param name="color">Color to create the icon in</param>
            /// <param name="secondColor">Color to use for format effects</param>
            /// <returns>Icon</returns>
            public static OImage GetPluginIcon(string PluginName, eTextFormat SymbolFormat, Color color, Color secondColor)
            {
                object o = new object();
                BuiltInComponents.Host.getData(eGetData.GetPlugins, PluginName, out o);
                if (o == null)
                    return null;
                return GetPluginIcon((IBasePlugin)o, SymbolFormat, color, secondColor);
            }
            /// <summary>
            /// Get icon for plugin (Either image or generated from font)
            /// </summary>
            /// <param name="Plugin">Plugin reference</param>
            /// <returns>Icon</returns>
            public static OImage GetPluginIcon(IBasePlugin Plugin)
            {
                return GetPluginIcon(Plugin, eTextFormat.Normal, Color.White, Color.White);
            }
            /// <summary>
            /// Get icon for plugin (Either image or generated from font)
            /// </summary>
            /// <param name="Plugin">Plugin reference</param>
            /// <param name="SymbolFormat">String format paramters for autogeneration of icon</param>
            /// <param name="color">Color to create the icon in</param>
            /// <param name="secondColor">Color to use for format effects</param>
            /// <returns>Icon</returns>
            public static OImage GetPluginIcon(IBasePlugin Plugin, eTextFormat SymbolFormat, Color color, Color secondColor)
            {
                string Icon = "";
                PluginLevel[] c = (PluginLevel[])Plugin.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                SkinIcon[] a = (SkinIcon[])Plugin.GetType().GetCustomAttributes(typeof(SkinIcon), false);
                if (a.Length > 0)
                {   // Icon name is provided    
                    Icon = a[0].SkinImageName;

                    // If first character in name is * then this is a request to use a symbol font instead (Webdings)
                    if (Icon.Substring(0, 1) == "*")
                    {   // Use symbol font
                        Icon = Icon.Substring(1, Icon.Length - 1);
                        return OImage.FromWebdingsFont(100, 100, Icon, SymbolFormat, Alignment.CenterCenter, color, secondColor);
                    }
                    else if (Icon.Substring(0, 1) == "#")
                    {   // Use symbol font
                        Icon = Icon.Substring(1, Icon.Length - 1);
                        return OImage.FromFont(100, 100, Icon, Font.Wingdings, SymbolFormat, Alignment.CenterCenter, color, secondColor);
                    }
                    else
                    {   // Get image from file
                        imageItem it = BuiltInComponents.Host.getSkinImage(Icon);
                        if ((it != null) && (it.image != null))
                        {
                            OImage i = new OImage(it.image.image);
                            i.Overlay(color);//Color.FromArgb(10,0,0));
                            return i; //BuiltInComponents.Host.getSkinImage(Icon).image;
                        }
                        return null;
                    }
                }
                else
                {   // No icon specified
                    return null;
                }
            }

            /// <summary>
            /// Gets the string describing the IMAGE to use as a Icon for the given PluginName
            /// </summary>
            /// <param name="PluginName"></param>
            /// <returns></returns>
            public static string GetPluginIconString(string PluginName)
            {
                object o = new object();
                BuiltInComponents.Host.getData(eGetData.GetPlugins, PluginName, out o);
                if (o == null)
                    return "";
                IBasePlugin Plugin = o as IBasePlugin;
                string Icon = "";
                PluginLevel[] c = (PluginLevel[])Plugin.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                SkinIcon[] a = (SkinIcon[])Plugin.GetType().GetCustomAttributes(typeof(SkinIcon), false);
                if (a.Length > 0)
                {   // Icon name is provided    
                    Icon = a[0].SkinImageName;
                    // If first character in name is * then this is a request to use a symbol font instead (Webdings)
                    if (Icon.Substring(0, 1) == "*")
                    {
                        return "";
                    }
                    else
                        return Icon;
                }
                return "";
            }

            /// <summary>
            /// Gets the string describing the SYMBOL to use as a Icon for the given PluginName
            /// <para>The font type is returned in the font ref param</para>
            /// </summary>
            /// <param name="PluginName"></param>
            /// <param name="font"></param>
            /// <returns></returns>
            public static string GetPluginIconSymbol(string PluginName, ref Font font)
            {
                object o = new object();
                BuiltInComponents.Host.getData(eGetData.GetPlugins, PluginName, out o);
                if (o == null)
                    return "";
                IBasePlugin Plugin = o as IBasePlugin;
                string Icon = "";
                PluginLevel[] c = (PluginLevel[])Plugin.GetType().GetCustomAttributes(typeof(PluginLevel), false);
                SkinIcon[] a = (SkinIcon[])Plugin.GetType().GetCustomAttributes(typeof(SkinIcon), false);
                if (a.Length > 0)
                {   // Icon name is provided    
                    Icon = a[0].SkinImageName;
                    // If first character in name is * then this is a request to use a symbol font instead (Webdings)
                    if (Icon.Substring(0, 1) == "*")
                    {   // Use symbol font
                        Icon = Icon.Substring(1, Icon.Length - 1);
                        font = Font.Webdings;
                        return Icon;
                    }
                    else if (Icon.Substring(0, 1) == "#")
                    {   // Use symbol font
                        Icon = Icon.Substring(1, Icon.Length - 1);
                        font = Font.Wingdings;
                        return Icon;
                    }
                    else
                        return "";
                }
                return "";
            }

            /// <summary>
            /// Gets a plugin
            /// </summary>
            /// <param name="PluginName"></param>
            /// <returns></returns>
            public static IBasePlugin GetPlugin(string PluginName)
            {
                return BuiltInComponents.Host.getData(eGetData.GetPlugins, PluginName) as IBasePlugin;
            }

            /// <summary>
            /// Gets the plugin enabled setting for a specific plugin
            /// </summary>
            /// <param name="pluginName"></param>
            /// <returns></returns>
            public static bool GetPluginEnabled(string pluginName)
            {
                List<IBasePlugin> plugins = BuiltInComponents.Host.getData(eGetData.GetPlugins, String.Empty) as List<IBasePlugin>;
                if (plugins == null)
                    return false;
                IBasePlugin plugin = plugins.Find(x => x.pluginName == pluginName);
                
                PluginLevels pluginLevel = getPluginLevel(plugin.pluginName);

                // Also report any system levels as enabled
                if ((pluginLevel & PluginLevels.System) == PluginLevels.System)
                    return true;
                if ((pluginLevel & PluginLevels.UI) == PluginLevels.UI)
                    return true;
                if ((pluginLevel & PluginLevels.MainMenu) == PluginLevels.MainMenu)
                    return true;
                if ((pluginLevel & PluginLevels.NoDisable) == PluginLevels.NoDisable)
                    return true;

                // Get setting
                return StoredData.GetBool(OM.GlobalSetting, String.Format("PluginSetting_{0}_Enabled", plugin.pluginName), true);
            }

            /// <summary>
            /// Sets the plugin enabled setting for a specific plugin 
            /// </summary>
            /// <param name="pluginName"></param>
            /// <param name="enabled"></param>
            /// <returns></returns>
            public static bool SetPluginEnabled(string pluginName, bool enabled)
            {
                List<IBasePlugin> plugins = BuiltInComponents.Host.getData(eGetData.GetPlugins, String.Empty) as List<IBasePlugin>;
                if (plugins == null)
                    return false;
                IBasePlugin plugin = plugins.Find(x => x.pluginName == pluginName);
                if (plugin == null)
                {
                    plugins = BuiltInComponents.Host.getData(eGetData.GetPluginsDisabled, String.Empty) as List<IBasePlugin>;
                    if (plugins == null)
                        return false;
                    plugin = plugins.Find(x => x.pluginName == pluginName);
                }

                if (plugin != null)
                {
                    // Set setting
                    return StoredData.Set(OM.GlobalSetting, String.Format("PluginSetting_{0}_Enabled", plugin.pluginName), enabled);
                }
                return false;
            }

            /// <summary>
            /// Gets the plugin base folder
            /// </summary>
            /// <param name="plugin"></param>
            /// <returns></returns>
            public static string GetPluginFolder(IBasePlugin plugin)
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(plugin.GetType()).Location);
            }
        }
    }


    /// <summary>
    /// Panel helper functions
    /// </summary>
    namespace Panels
    {

    }


    namespace XML
    {
        public class Serializer
        {
            public static string toXML(object DataToSerialize)
            {
                StringWriter textWriter = new StringWriter();
                XmlSerializer xmlSerializer = new XmlSerializer(DataToSerialize.GetType());
                xmlSerializer.Serialize(textWriter, DataToSerialize);
                return textWriter.ToString();
            }

            public static T fromXML<T>(string XML)
            {
                T serializedObject = default(T);
                using (StringReader textReader = new StringReader(XML))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    serializedObject = (T)xmlSerializer.Deserialize(textReader);
                }
                return serializedObject;
            }
        }
    }

    namespace Strings
    {
        /// <summary>
        /// Class that can automatically wrap a given string.
        /// </summary>
        public class StringWrap
        {
            /// <summary>
            /// List of characters to break a string around.  A <c>char</c> key points to an
            /// <c>int</c> weight.  Higher weights indicate a higher preference to break at
            /// that character.
            /// </summary>
            public readonly System.Collections.Hashtable Breakable = new System.Collections.Hashtable();

            /// <summary>
            /// Class that can automatically wrap a given string.
            /// </summary>
            public StringWrap()
            {
                Breakable[' '] = 10;
                Breakable[','] = 20;
                Breakable[';'] = 30;
                Breakable['.'] = 40;
            }

            /// <summary>
            /// Cleanse the given text from duplicate (two or more in a row) characters, as
            /// specified by <c>Breakable</c>.  Will also remove all existing newlines.
            /// </summary>
            /// <param name="text">Text to be cleansed.</param>
            /// <returns>Cleansed text.</returns>
            protected string Cleanse(string text)
            {
                text = text.Replace('\n', ' ');
                string find, replace;
                foreach (char c in Breakable.Keys)
                {
                    find = new string(c, 2);
                    replace = new string(c, 1);
                    while (text.IndexOf(find) != -1)
                        text = text.Replace(find, replace);
                }
                return text;
            }

            /// <summary>
            /// Perform an automatic wrap given text, a target ratio, and a font ratio.
            /// </summary>
            /// <param name="text">Text to automatically wrap.</param>
            /// <param name="targetRatio">Ratio (Height / Width) of the target area
            /// for this text.</param>
            /// <param name="fontRatio">Average ratio (Height / Width) of a single
            /// character.</param>
            /// <returns>Automatically wrapped text.</returns>
            public string PerformWrap(string text, float targetRatio, float fontRatio)
            {
                string wrap = "";
                text = Cleanse(text);

                int rows = (int)System.Math.Sqrt(targetRatio * text.Length / fontRatio),
                    cols = text.Length / rows,
                    start = cols, index = 0, last;

                for (int i = 0; i < rows - 1; i++)
                {
                    last = index;
                    index = BestBreak(text, start, cols * 2);
                    wrap += text.Substring(last, index - last).Trim() + "\n";
                    start = index + cols;
                }

                wrap += text.Substring(index);
                return wrap;
            }

            /// <summary>
            /// Find the best place to break text given a starting index and a search radius.
            /// </summary>
            /// <param name="text">Full text to search through.</param>
            /// <param name="start">Index in string to start searching at.  Will be used
            /// as the center of the radius.</param>
            /// <param name="radius">Radius (in characters) to search around the given
            /// starting index.</param>
            /// <returns>Optimal index to break the text at.</returns>
            protected int BestBreak(string text, int start, int radius)
            {
                int bestIndex = start;
                float bestWeight = 0, examWeight;

                radius = System.Math.Min(System.Math.Min(start + radius, text.Length - 1) - start,
                    start - System.Math.Max(start - radius, 0));
                for (int i = start - radius; i <= start + radius; i++)
                {
                    object o = Breakable[text[i]];
                    if (o == null) continue;

                    examWeight = (int)o / (float)System.Math.Abs(start - i);
                    if (examWeight > bestWeight)
                    {
                        bestIndex = i;
                        bestWeight = examWeight;
                    }
                }

                return System.Math.Min(bestIndex + 1, text.Length - 1);
            }
        }
    }


}
