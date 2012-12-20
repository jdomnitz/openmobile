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
    /// <summary>
    /// Gives access to the database in OM
    /// </summary>
    public static class StoredData
    {
        /// <summary>
        /// Gets a setting as a bool value from the database
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool GetBool(string Name)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.getSetting(Name).ToLower().Equals("true");
        }
        /// <summary>
        /// Sets a setting as a bool value in the database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool SetBool(string Name, bool Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value.ToString());
        }

        /// <summary>
        /// Returns an int value from the settings database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="DefaultValue">Value to use in case of conversion error or missing setting</param>
        /// <returns></returns>
        public static int GetInt(string Name, int DefaultValue)
        {
            using (PluginSettings setting = new PluginSettings())
            {
                int i = 0;
                if (!int.TryParse(setting.getSetting(Name), out i))
                    return DefaultValue;
                return i;
            }
        }
        /// <summary>
        /// Returns an int value from the settings database (0 is used as default value)
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static int GetInt(string Name)
        {
            return GetInt(Name, 0);
        }

        /// <summary>
        /// Returns a float value from the settings database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="DefaultValue">Value to use in case of conversion error or missing setting</param>
        /// <returns></returns>
        public static float GetFloat(string Name, float DefaultValue)
        {
            using (PluginSettings setting = new PluginSettings())
            {
                float i = 0;
                if (!float.TryParse(setting.getSetting(Name), out i))
                    return DefaultValue;
                return i;
            }
        }
        /// <summary>
        /// Returns a float value from the settings database (0 is used as default value)
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static float GetFloat(string Name)
        {
            return GetInt(Name, 0);
        }

        /// <summary>
        /// Gets a setting as a color from the database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Color GetColor(string Name, Color DefaultValue)
        {
            try
            {
                // Extract color values from string data
                string[] ColorValues = Get(Name).Split(new char[] { ',' });
                return OpenMobile.Graphics.Color.FromArgb(255,
                    int.Parse(ColorValues[0], System.Globalization.NumberStyles.AllowHexSpecifier),
                    int.Parse(ColorValues[1], System.Globalization.NumberStyles.AllowHexSpecifier),
                    int.Parse(ColorValues[2], System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            catch
            {   // Default fallback color in case of conversion error
                return DefaultValue;
            }
        }

        /// <summary>
        /// Get's a setting from the database
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static string Get(string Name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(Name);
        }
        /// <summary>
        /// Saves a setting as a string to the database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool Set(string Name, string Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value);
        }
        /// <summary>
        /// Saves a setting as an object to the database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool Set(string Name, object Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value.ToString());
        }
        /// <summary>
        /// Saves a setting as a color's R,G,B values to the database as hex values
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="RGB"></param>
        /// <returns></returns>
        public static bool Set(string Name, Color RGB)
        {
            return Set(Name, RGB, false);
        }
        /// <summary>
        /// Saves a setting as a color's R,G,B values to the database
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="RGB"></param>
        /// <param name="UseDec">Use decimal values instead of hex values for the R,G,B values</param>
        /// <returns></returns>
        public static bool Set(string Name, Color RGB, bool UseDec)
        {
            using (PluginSettings setting = new PluginSettings())
            if (UseDec)
                return setting.setSetting(Name, String.Format("{0},{1},{2}", RGB.R, RGB.G, RGB.B));
            else
                return setting.setSetting(Name, String.Format("{0},{1},{2}", RGB.R.ToString("X2"), RGB.G.ToString("X2"), RGB.B.ToString("X2")));
        }

        /// <summary>
        /// Sets the default value for a setting
        /// </summary>
        /// <param name="Setting"></param>
        /// <param name="Value"></param>
        public static void SetDefaultValue(string Setting, string Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(Setting)))
                StoredData.Set(Setting, Value);
        }
        /// <summary>
        /// Sets the default value for a setting
        /// </summary>
        /// <param name="Setting"></param>
        /// <param name="Value"></param>
        public static void SetDefaultValue(string Setting, object Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(Setting)))
                StoredData.Set(Setting, Value);
        }

    }
}
