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
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;

namespace OpenMobile
{
    /// <summary>
    /// Built in plugins
    /// </summary>
    public static class BuiltInComponents
    {
        /// <summary>
        /// A screenmanager for panels created and handled by the framework
        /// NB! Panels from the framework is always common for all screens
        /// </summary>
        public static OpenMobile.Framework.ScreenManager Panels = null;

        private static IPluginHost _Host = null;
        /// <summary>
        /// A reference to the pluginhost to use for the framework.
        /// NB! This reference is not valid unless it's initialized from the core at startup
        /// </summary>
        public static IPluginHost Host
        {
            get 
            {
                if (_Host == null)
                    throw new Exception("Programming error: BuiltInComponents.Host is not initialized from the core at startup");
                return _Host; 
            }
            set
            {
                _Host = value;
                Panels = new OpenMobile.Framework.ScreenManager(_Host.ScreenCount);
            }
        }

        /// <summary>
        /// The copyright information to be displayed on the about screen
        /// </summary>
        public static string AboutText
        {
            get
            {
                //WARNING: REMOVING ANY OF THE BELOW DESCRIPTION IS A VIOLATION OF THE LICENSE AGREEMENT
                string Text = "OpenMobile is copyright the openMobile Foundation and its contributors.\r\n\r\n";
                Text += "This program in full or in part is protected under a clarified version of the GPLv3 license which can be found in the application directory.\r\n\r\n";
                Text += "Contributors:\r\n";
                Text += "Justin Domnitz (justchat_1) - Founder / Developer\r\n";
                Text += "UnusuallyGenius - Graphics\r\n";
                Text += "ws6vert - openOBD and Garmin Mobile PC Projects\r\n";
                Text += "Borte - Developer\r\n";
                Text += "jheizer - Developer\r\n";
                Text += "jmullan99@gmail.com - Tester\r\n";
                Text += "Efess - Navit developer\r\n";
                Text += "\r\nSupporting Projects:\r\n";
                Text += "TagLib Sharp, The Mono Project, iPod Sharp, DBusSharp, Sqlite, Aqua Gauge, CoreAudio and the Open ToolKit Project";
                return Text;
            }
        }
        private static OMPanel about;
        /// <summary>
        /// Returns an About panel for display
        /// </summary>
        /// <returns></returns>
        public static OMPanel AboutPanel()
        {
            if (about == null)
            {
                about = new OMPanel("About");
                OMLabel description = new OMLabel(30, 55, 940, 540);
                description.TextAlignment = OpenMobile.Graphics.Alignment.WordWrap | OpenMobile.Graphics.Alignment.TopCenter;
                description.Text = AboutText;
                about.addControl(description);
            }
            return about;
        }

        static IPluginHost theHost;

        /// <summary>
        /// Returns a settings object with OM system settings
        /// </summary>
        /// <returns></returns>
        public static Settings GlobalSettings()
        {
            theHost = BuiltInComponents.Host;

            // Set default values
            StoredData.SetDefaultValue("UI.SkinFocusColor", "00,00,FF");
            StoredData.SetDefaultValue("UI.SkinTextColor", "FF,FF,FF");
            StoredData.SetDefaultValue("UI.MinGraphics", false.ToString());
            StoredData.SetDefaultValue("UI.VolumeChangesVisible", true.ToString());
            StoredData.SetDefaultValue("UI.ShowCursor", false.ToString());

            Settings gl = new Settings("General Settings");
            Setting graphics = new Setting(SettingTypes.MultiChoice, "UI.MinGraphics", String.Empty, "Disable Enhanced Graphics", Setting.BooleanList, Setting.BooleanList);
            Setting volume = new Setting(SettingTypes.MultiChoice, "UI.VolumeChangesVisible", "", "Show Volume Level when adjusting volume", Setting.BooleanList, Setting.BooleanList);
            Setting ShowCursor = new Setting(SettingTypes.MultiChoice, "UI.ShowCursor", String.Empty, "Show OM mouse/pointer cursors", Setting.BooleanList, Setting.BooleanList);
            //Setting SkinColor = new Setting(SettingTypes.Text, "UI.SkinColor", "Foreground", "Skin foreground color (R,G,B)");
            Setting SkinFocusColor = new Setting(SettingTypes.Text, "UI.SkinFocusColor", "Focus color", "Skin focus color (R,G,B)");
            Setting SkinTextColor = new Setting(SettingTypes.Text, "UI.SkinTextColor", "Text color", "Skin text color (R,G,B)");
            using (PluginSettings settings = new PluginSettings())
            {
                graphics.Value = settings.getSetting("UI.MinGraphics");
                volume.Value = settings.getSetting("UI.VolumeChangesVisible");
                ShowCursor.Value = settings.getSetting("UI.ShowCursor");
                //SkinColor.Value = settings.getSetting("UI.SkinColor");
                SkinFocusColor.Value = settings.getSetting("UI.SkinFocusColor");
                SkinTextColor.Value = settings.getSetting("UI.SkinTextColor");
            }
            gl.Add(graphics);
            gl.Add(volume);
            gl.Add(ShowCursor);
            //gl.Add(SkinColor);
            gl.Add(SkinFocusColor);
            gl.Add(SkinTextColor);
            gl.OnSettingChanged += new SettingChanged(SettingsChanged);
            return gl;
        }

        static void SettingsChanged(int screen, Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
            {
                s.setSetting(setting.Name, setting.Value);
                switch (setting.Name)
                {
                    case "UI.MinGraphics":
                        if (setting.Value == "True")
                            theHost.GraphicsLevel = eGraphicsLevel.Minimal;
                        else
                            theHost.GraphicsLevel = eGraphicsLevel.Standard;
                        break;

                    case "UI.ShowCursor":
                        if (setting.Value == "True")
                            theHost.ShowCursors = true;
                        else
                            theHost.ShowCursors = false;
                        break;

                    default:
                        theHost.execute(eFunction.settingsChanged, setting.Name);
                        break;
                }
            }
        }

        /// <summary>
        /// OpenMobile System settings
        /// </summary>
        public static class SystemSettings
        {
            /// <summary>
            /// OM System setting: True = use minimalistic graphics
            /// </summary>
            public static bool UseSimpleGraphics
            {
                get
                {
                    return StoredData.GetBool("UI.MinGraphics");
                }
                set
                {
                    StoredData.SetBool("UI.MinGraphics", value);
                }
            }

            /// <summary>
            /// OM System setting: True = Show volume changes
            /// </summary>
            public static bool VolumeChangesVisible
            {
                get
                {
                    return StoredData.GetBool("UI.VolumeChangesVisible");
                }
                set
                {
                    StoredData.SetBool("UI.VolumeChangesVisible", value);
                }
            }

            /// <summary>
            /// OM System setting: True = Show OM system cursors
            /// </summary>
            public static bool ShowCursor
            {
                get
                {
                    return StoredData.GetBool("UI.ShowCursor");
                }
                set
                {
                    StoredData.SetBool("UI.ShowCursor", value);
                }
            }

            /// <summary>
            /// OM System setting: Skin Focus color
            /// </summary>
            public static OpenMobile.Graphics.Color SkinFocusColor
            {
                get
                {
                    try
                    {
                        // Extract color values from string data
                        string[] ColorValues = StoredData.Get("UI.SkinFocusColor").Split(new char[] { ',' });
                        return OpenMobile.Graphics.Color.FromArgb(255,
                            int.Parse(ColorValues[0], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[1], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[2], System.Globalization.NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {   // Default fallback color in case of conversion error
                        return OpenMobile.Graphics.Color.Blue;
                    }
                }
                set
                {
                    StoredData.Set("UI.SkinFocusColor", String.Format("{0},{1},{2}", value.R, value.G, value.B));
                }
            }

            /// <summary>
            /// OM System setting: Skin text color
            /// </summary>
            public static OpenMobile.Graphics.Color SkinTextColor
            {
                get
                {
                    try
                    {
                        // Extract color values from string data
                        string[] ColorValues = StoredData.Get("UI.SkinTextColor").Split(new char[] { ',' });
                        return OpenMobile.Graphics.Color.FromArgb(255,
                            int.Parse(ColorValues[0], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[1], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[2], System.Globalization.NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {   // Default fallback color in case of conversion error
                        return OpenMobile.Graphics.Color.White;
                    }
                }
                set
                {
                    StoredData.Set("UI.SkinFocusColor", String.Format("{0},{1},{2}", value.R, value.G, value.B));
                }
            }

        }

    }

    public static class Timing
    {
        static DateTime start = DateTime.Now;
        static DateTime intermediate = DateTime.Now;

        /// <summary>
        /// Internal use only! 
        /// Gets timing data since last call of this method (used for timing of code)
        /// </summary>
        /// <returns></returns>
        public static string GetTiming()
        {
            double Duration = (DateTime.Now - intermediate).TotalMilliseconds;
            intermediate = DateTime.Now;
            return Duration.ToString() + " (" + (DateTime.Now - start).TotalMilliseconds.ToString() + ")";
        }
    }

    /// <summary>
    /// Provides a set of OM errorhandling methods
    /// </summary>
    public static class ErrorHandling
    {
        /// <summary>
        /// Formats all required data for an exception into a string 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nTargetSite: \r" + e.TargetSite);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nTargetSite: \r" + e.TargetSite);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }
    }
}
