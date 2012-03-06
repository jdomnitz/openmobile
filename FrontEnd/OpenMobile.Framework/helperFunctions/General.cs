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
            doc.Load(@"http://where.yahooapis.com/geocode?q=" + Net.Network.urlEncode(name) + "&locale=" + locale);
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
                                        ret.Latitude = float.Parse(n2.InnerText);
                                    break;
                                case "longitude":
                                    if (!string.IsNullOrEmpty(n2.InnerText))
                                        ret.Longitude = float.Parse(n2.InnerText);
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

        /// <summary>
        /// Synchronously get keyboard input from the On Screen Keyboard
        /// </summary>
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
            public string getText(int screen, string pluginname)
            {
                return getText(screen, pluginname, String.Empty);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            public string getText(int screen, string pluginname, string panelName)
            {
                return getText(screen, pluginname, new string[] { panelName });
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
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
            public string getPassword(int screen, string pluginname, string panelName, string defaultValue)
            {
                return getPassword(screen, pluginname, new string[] { panelName }, defaultValue);
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
            public string getText(int screen, string pluginname, string[] panelNames)
            {
                return getText(screen, pluginname, panelNames, "OSK");
            }
            /// <summary>
            /// Loads the On Screen Keyboard and then returns the text entered
            /// </summary>
            /// <returns></returns>
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
            public string getNumber(int screen, string pluginname)
            {
                return getNumber(screen, pluginname, String.Empty);
            }

            /// <summary>
            /// Loads the On Screen Keyboard and then returns the number entered
            /// </summary>
            /// <returns></returns>
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

            void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
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
                else if ((function == eFunction.TransitionFromAny) && (int.Parse(arg1) == screen))
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
        static public class plugin
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
            /// Gets the string describing the WebDings SYMBOL to use as a Icon for the given PluginName
            /// </summary>
            /// <param name="PluginName"></param>
            /// <returns></returns>
            public static string GetPluginIconSymbol(string PluginName)
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
                        return Icon;
                    }
                    else
                        return "";
                }
                return "";
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

}
