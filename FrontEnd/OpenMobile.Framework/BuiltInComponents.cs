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

namespace OpenMobile
{
    /// <summary>
    /// Built in plugins
    /// </summary>
    public static class BuiltInComponents
    {
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
                Text += "Justin Domnitz (justchat_1) - Lead Developer\r\n";
                Text += "UnusuallyGenius - Graphics Designer\r\n";
                Text += "ws6vert - openOBD and Garmin Mobile PC Projects\r\n";
                Text += "Borte - Developer\r\n";
                Text += "jheizer - Developer\r\n";
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
        /// Returns the Non-Skin Specific Settings
        /// </summary>
        /// <returns></returns>
        public static Settings GlobalSettings(IPluginHost host)
        {
            theHost = host;
            Settings gl = new Settings("General Settings");
            Setting graphics = new Setting(SettingTypes.MultiChoice, "UI.MinGraphics", String.Empty, "Disable Enhanced Graphics", Setting.BooleanList, Setting.BooleanList);
            Setting volume = new Setting(SettingTypes.MultiChoice, "UI.VolumeChangesVisible", "", "Show Volume Level when adjusting volume", Setting.BooleanList, Setting.BooleanList);
            using (PluginSettings settings = new PluginSettings())
            {
                graphics.Value = settings.getSetting("UI.MinGraphics");
                volume.Value = settings.getSetting("UI.VolumeChangesVisible");
            }
            gl.Add(graphics);
            gl.Add(volume);
            gl.OnSettingChanged += new SettingChanged(SettingsChanged);
            return gl;
        }

        static void SettingsChanged(int screen, Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
            {
                switch (setting.Name)
                {
                    case "UI.MinGraphics":
                        if (setting.Value == "True")
                            theHost.GraphicsLevel = eGraphicsLevel.Minimal;
                        else
                            theHost.GraphicsLevel = eGraphicsLevel.Standard;
                        break;
                    case "UI.VolumeChangesVisible":
                        s.setSetting(setting.Name, setting.Value);
                        theHost.execute(eFunction.settingsChanged, "UI.VolumeChangesVisible");
                        return;
                }
                s.setSetting(setting.Name, setting.Value);
            }
        }
        /// <summary>
        /// Gets zone specific settings for the given instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Settings getZoneSettings(int instance)
        {
            return new Settings("Zone " + (instance + 1).ToString() + " Settings"); //Not Yet Implemented
        }
    }
}
