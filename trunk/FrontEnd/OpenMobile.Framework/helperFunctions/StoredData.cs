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
using OpenMobile.Data;
using OpenMobile.Graphics;

namespace OpenMobile.helperFunctions
{
    public static class StoredData
    {

        public static bool GetBool(string Name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(Name).ToLower().Equals("true");
        }
        public static bool SetBool(string Name, bool Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value.ToString());
        }

        public static string Get(string Name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(Name);
        }

        public static bool Set(string Name, string Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value);
        }

        public static void SetDefaultValue(string Setting, string Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(Setting)))
                StoredData.Set(Setting, Value);
        }

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
            public static Color SkinFocusColor
            {
                get
                {
                    try
                    {
                        // Extract color values from string data
                        string[] ColorValues = StoredData.Get("UI.SkinFocusColor").Split(new char[] { ',' });
                        return Color.FromArgb(255, 
                            int.Parse(ColorValues[0], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[1], System.Globalization.NumberStyles.AllowHexSpecifier),
                            int.Parse(ColorValues[2], System.Globalization.NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {   // Default fallback color in case of conversion error
                        return Color.Blue;
                    }
                }
                set
                {
                    StoredData.Set("UI.SkinFocusColor", String.Format("{0},{1},{2}",value.R, value.G,value.B));
                }
            }

        }
    }
}
