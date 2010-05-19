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
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OpenMobile
{
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
                string Text = "OpenMobile is copyright the openMobile Foundation and its contributors\r\n\r\n";
                Text += "This program in full or in part is protected under a clarified version of the GPLv3 license which can be found in the application directory.\r\n\r\n";
                Text += "Contributors:\r\n";
                Text += "Justin Domnitz (justchat_1) - Lead Developer\r\n";
                Text += "UnusuallyGenius - Graphics Designer\r\n";
                Text += "ws6vert - openOBD and Garmin Mobile PC Projects\r\n";
                Text += "Borte - Developer\r\n";
                Text += "malcom2073 - Developer\r\n";
                Text += "\r\nSupporting Projects:\r\n";
                Text += "TagLib Sharp, The Mono Project, iPod Sharp, DBusSharp, Sqlite, Aqua Gauge and CoreAudio";
                return Text;
            }
        }
        private static OMPanel about;
        public static OMPanel AboutPanel()
        {
            if (about == null)
            {
                about = new OMPanel("About");
                OMLabel description = new OMLabel(30, 40, 900, 550);
                description.TextAlignment = Alignment.TopCenter;
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
            Setting cursor=new Setting(SettingTypes.MultiChoice,"UI.HideCursor","","Hide Mouse Cursor",Setting.BooleanList,Setting.BooleanList);
            Setting graphics=new Setting(SettingTypes.MultiChoice, "UI.MinGraphics", "", "Disable Enhanced Graphics", Setting.BooleanList, Setting.BooleanList);
            using (PluginSettings settings = new PluginSettings())
            {
                cursor.Value= settings.getSetting("UI.HideCursor");
                graphics.Value=settings.getSetting("UI.MinGraphics");
            }
            gl.Add(cursor);
            gl.Add(graphics);
            gl.OnSettingChanged+=new SettingChanged(SettingsChanged);
            return gl;
        }

        static void SettingsChanged(Setting setting)
        {
            switch (setting.Name)
            {
                case "UI.HideCursor":
                    using (PluginSettings s = new PluginSettings())
                    {
                        if (setting.Value != s.getSetting("UI.HideCursor"))
                            theHost.sendMessage("RenderingWindow", "OMSettings", "ToggleCursor");
                        s.setSetting("UI.HideCursor", setting.Value);
                    }
                    break;
                case "UI.MinGraphics":
                    if (setting.Value == "True")
                        theHost.GraphicsLevel = eGraphicsLevel.Minimal;
                    else
                        theHost.GraphicsLevel = eGraphicsLevel.Standard;
                break;
            }
        }
        /// <summary>
        /// Gets zone specific settings for the given instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static Settings getZoneSettings(int instance)
        {
            return new Settings("Zone "+(instance+1).ToString()+" Settings"); //Not Yet Implemented
        }
    }
}
