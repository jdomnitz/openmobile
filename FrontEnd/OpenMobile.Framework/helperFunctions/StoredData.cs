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
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Plugin;


namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Gives access to the database in OM
    /// </summary>
    public static class StoredData
    {

        /// <summary>
        /// Gets an enum setting from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetEnum<T>(IBasePlugin plugin, string name)
        {
            return GetEnum<T>(plugin, name, default(T));
        }

        /// <summary>
        /// Gets an enum setting from the database, if setting is not present in DB it returns the defaultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetEnum<T>(IBasePlugin plugin, string name, T defaultValue)
        {
            string value = "";
            using (PluginSettings setting = new PluginSettings())
                value = setting.getSetting(plugin, name);

            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Gets a setting as a bool value from the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(IBasePlugin plugin, string name)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.getSetting(plugin, name).ToLower().Equals("true");
        }
        /// <summary>
        /// Gets a setting as a bool value from the database, if setting is not present in DB it returns the defaultValue
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(IBasePlugin plugin, string name, bool defaultValue)
        {
            using (PluginSettings setting = new PluginSettings())
            {
                string value = setting.getSetting(plugin, name).ToLower();
                if (String.IsNullOrEmpty(value))
                    return defaultValue;
                return value.Equals("true");
            }
        }

        /// <summary>
        /// Sets a setting as a bool value in the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetBool(IBasePlugin plugin, string name, bool value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(plugin, name, value.ToString());
        }

        /// <summary>
        /// Returns an int value from the settings database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue">Value to use in case of conversion error or missing setting</param>
        /// <returns></returns>
        public static int GetInt(IBasePlugin plugin, string name, int defaultValue)
        {
            using (PluginSettings setting = new PluginSettings())
            {
                int i = 0;
                if (!int.TryParse(setting.getSetting(plugin, name), out i))
                    return defaultValue;
                return i;
            }
        }
        /// <summary>
        /// Returns an int value from the settings database (0 is used as default value)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetInt(IBasePlugin plugin, string name)
        {
            return GetInt(plugin, name, 0);
        }

        /// <summary>
        /// Returns a float value from the settings database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue">Value to use in case of conversion error or missing setting</param>
        /// <returns></returns>
        public static float GetFloat(IBasePlugin plugin, string name, float defaultValue)
        {
            using (PluginSettings setting = new PluginSettings())
            {
                float i = 0;
                if (!float.TryParse(setting.getSetting(plugin, name), out i))
                    return defaultValue;
                return i;
            }
        }
        /// <summary>
        /// Returns a float value from the settings database (0 is used as default value)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static float GetFloat(IBasePlugin plugin, string name)
        {
            return GetFloat(plugin, name, 0);
        }

        /// <summary>
        /// Gets a setting as a color from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Color GetColor(IBasePlugin plugin, string name, Color defaultValue)
        {
            try
            {
                // Extract color values from string data
                string[] ColorValues = Get(plugin, name).Split(new char[] { ',' });
                return OpenMobile.Graphics.Color.FromArgb(255,
                    int.Parse(ColorValues[0], System.Globalization.NumberStyles.AllowHexSpecifier),
                    int.Parse(ColorValues[1], System.Globalization.NumberStyles.AllowHexSpecifier),
                    int.Parse(ColorValues[2], System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            catch
            {   // Default fallback color in case of conversion error
                return defaultValue;
            }
        }

        /// <summary>
        /// Get's a setting from the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Get(IBasePlugin plugin, string name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(plugin, name);
        }

        /// <summary>
        /// Get's an object from the database. The data being read out from the DB must be a serialized xml object saved via Set<T>"/>.
        /// <para>If the extraction fails it will return the default value for the requested type</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static T GetXML<T>(IBasePlugin plugin, string name)
        {
            try
            {
                string XML = Get(plugin, name);
                return OpenMobile.helperFunctions.XML.Serializer.fromXML<T>(XML);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Saves an object to the database as a serialized XML string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetXML(IBasePlugin plugin, string name, object value)
        {
            try
            {
                StoredData.Set(plugin, name, OpenMobile.helperFunctions.XML.Serializer.toXML(value));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Saves a setting as a string to the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Set(IBasePlugin plugin, string name, string value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(plugin, name, value);
        }
        /// <summary>
        /// Saves a setting as an object to the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Set(IBasePlugin plugin, string name, object value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(plugin, name, value.ToString());
        }
        /// <summary>
        /// Saves a setting as a color's R,G,B values to the database as hex values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static bool Set(IBasePlugin plugin, string name, Color rgb)
        {
            return Set(plugin, name, rgb, false);
        }
        /// <summary>
        /// Saves a setting as a color's R,G,B values to the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rgb"></param>
        /// <param name="useDec">Use decimal values instead of hex values for the R,G,B values</param>
        /// <returns></returns>
        public static bool Set(IBasePlugin plugin, string name, Color rgb, bool useDec)
        {
            using (PluginSettings setting = new PluginSettings())
            if (useDec)
                return setting.setSetting(plugin, name, String.Format("{0},{1},{2}", rgb.R, rgb.G, rgb.B));
            else
                return setting.setSetting(plugin, name, String.Format("{0},{1},{2}", rgb.R.ToString("X2"), rgb.G.ToString("X2"), rgb.B.ToString("X2")));
        }

        /// <summary>
        /// Sets the default value for a setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Value"></param>
        public static void SetDefaultValue(IBasePlugin plugin, string name, string Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(plugin, name)))
                StoredData.Set(plugin, name, Value);
        }
        /// <summary>
        /// Sets the default value for a setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Value"></param>
        public static void SetDefaultValue(IBasePlugin plugin, string name, object Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(plugin, name)))
                StoredData.Set(plugin, name, Value);
        }

        /// <summary>
        /// Clears the value of a setting
        /// </summary>
        /// <param name="name"></param>
        public static void Clear(IBasePlugin plugin, string name)
        {
            Set(plugin, name, String.Empty);
        }

        /// <summary>
        /// Deletes a setting from the database
        /// </summary>
        /// <param name="name"></param>
        public static bool Delete(IBasePlugin plugin, string name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.DeleteSetting(plugin,name);
        }

        /// <summary>
        /// Deletes a object from the database
        /// </summary>
        /// <param name="name"></param>
        public static bool DeleteObject(IBasePlugin plugin, string name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.DeleteObject(plugin, name);
        }

        /// <summary>
        /// Saves an object to the in the database
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetObject(IBasePlugin plugin, string name, object value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.SetObject(plugin, name, value);
        }

        /// <summary>
        /// Gets an object from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetObject<T>(IBasePlugin plugin, string name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.GetObject<T>(plugin, name);
        }

        /// <summary>
        /// Tries to get the object with the given name from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryGetObject<T>(IBasePlugin plugin, string name, out T value)
        {            
            using (PluginSettings settings = new PluginSettings())
                return settings.TryGetObject<T>(plugin, name, out value);
        }

        /// <summary>
        /// Saves an object to the in the database
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetObjectXML(IBasePlugin plugin, string name, object value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.SetObjectXML(plugin, name, value);
        }

        /// <summary>
        /// Gets an object from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetObjectXML<T>(IBasePlugin plugin, string name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.GetObjectXML<T>(plugin, name);
        }

        /// <summary>
        /// Tries to get the object with the given name from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryGetObjectXML<T>(IBasePlugin plugin, string name, out T value)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.TryGetObjectXML<T>(plugin, name, out value);
        }

        /// <summary>
        /// An collection of key/value data where value can be any type of data
        /// <para>This collection can also be saved and loaded to/from the db</para>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class ObjectCollection<TKey>
        {
            private Dictionary<TKey, object> _data = new Dictionary<TKey, object>();

            /// <summary>
            /// Add an object to the collection
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Add(TKey key, object value)
            {
                _data.Add(key, value);
            }

            /// <summary>
            /// Tries to get the value from the specified key as a specific datatype, returns true if successfull otherwise false
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryGetValue<T>(TKey key, out T value)
            {
                object o;
                if (_data.TryGetValue(key, out o))
                {
                    try
                    {
                        value = (T)Convert.ChangeType(o, typeof(T)); ;
                    }
                    catch
                    {
                        value = default(T);
                        return false;
                    }
                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }

            /// <summary>
            /// Gets the value from the specified key as a specific datatype, returns default data for the object if it fails
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <returns></returns>
            public T GetValue<T>(TKey key)
            {
                T value;
                if (TryGetValue<T>(key, out value))
                    return value;
                else
                    return default(T);
            }

            /// <summary>
            /// Gets the value from the specified key, method fails with an exception if key is not present
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public object this[TKey key]
            {
                get
                {
                    return _data[key];
                }
                set
                {
                    _data[key] = value;
                }
            }

            /// <summary>
            /// Removes a key/object item from the collection
            /// </summary>
            /// <param name="key"></param>
            public void Remove(TKey key)
            {
                _data.Remove(key);
            }

            /// <summary>
            /// Removes all items from the collection
            /// </summary>
            public void Clear()
            {
                _data.Clear();
            }

            /// <summary>
            /// Saves the collection to the database
            /// </summary>
            /// <param name="plugin"></param>
            /// <param name="Name"></param>
            /// <returns></returns>
            public bool Save(IBasePlugin plugin, string Name)
            {
                using (PluginSettings setting = new PluginSettings())
                    return setting.SetObject(plugin, Name, _data);
            }

            /// <summary>
            /// Fills the collection with data loaded from the database
            /// </summary>
            /// <param name="plugin"></param>
            /// <param name="Name"></param>
            /// <returns></returns>
            public bool Load(IBasePlugin plugin, string Name)
            {
                try
                {
                    using (PluginSettings setting = new PluginSettings())
                        _data = setting.GetObject<Dictionary<TKey, object>>(plugin, Name);
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }

        /// <summary>
        /// A data collection that saves data on a per screen basis
        /// <para>This collection can also be saved and loaded to/from the db</para>
        /// </summary>
        public class ScreenInstanceData
        {
            private Dictionary<string, object> _Data = new Dictionary<string, object>();

            /// <summary>
            /// Creates a new data collection
            /// </summary>
            public ScreenInstanceData()
            {
            }

            /// <summary>
            /// Sets a property for ALL screens
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void SetProperty(string name, object value)
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                    SetProperty(i, name, value);
            }

            /// <summary>
            /// Sets a property for a specific screen
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void SetProperty(int screen, string name, object value)
            {
                name = GetPropertyName(screen, name);
                if (_Data.ContainsKey(name))
                    _Data[name] = value;
                else
                    _Data.Add(name, value);
            }


            private object GetProperty_Internal(int screen, string name, object defaultValue)
            {
                name = GetPropertyName(screen, name);
                if (_Data.ContainsKey(name))
                    return _Data[name];
                else
                    return defaultValue;
            }

            /// <summary>
            /// Gets a property for a given screen, returns valie given in defaultValue if property is not available
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="name"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public object GetProperty(int screen, string name, object defaultValue)
            {
                return GetProperty_Internal(screen, name, defaultValue);
            }

            /// <summary>
            /// Gets a property for a given screen and returns it as the given type. Returns default value for the given type if property is not available
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="screen"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            public T GetProperty<T>(int screen, string name)
            {
                return (T)GetProperty_Internal(screen, name, default(T));
            }
            /// <summary>
            /// Gets a property for a given screen and returns it as the given type. Returns value given in defaultValue for the given type if property is not available
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="screen"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            public T GetProperty<T>(int screen, string name, T defaultValue)
            {
                return (T)GetProperty_Internal(screen, name, defaultValue);
            }

            private string GetPropertyName(int screen, string name)
            {
                return string.Format("{0}_{1}", screen, name);
            }

            /// <summary>
            /// Removes a property from all screens
            /// </summary>
            /// <param name="name"></param>
            public void RemoveProperty(string name)
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                    _Data.Remove(GetPropertyName(i, name));
            }

            /// <summary>
            /// Removes all items from the collection
            /// </summary>
            public void Clear()
            {
                _Data.Clear();
            }

            /// <summary>
            /// Saves the collection to the database
            /// </summary>
            /// <param name="plugin"></param>
            /// <param name="Name"></param>
            /// <returns></returns>
            public bool Save(IBasePlugin plugin, string Name)
            {
                using (PluginSettings setting = new PluginSettings())
                    return setting.SetObject(plugin, Name, _Data);
            }

            /// <summary>
            /// Fills the collection with data loaded from the database
            /// </summary>
            /// <param name="plugin"></param>
            /// <param name="Name"></param>
            /// <returns></returns>
            public bool Load(IBasePlugin plugin, string Name)
            {
                try
                {
                    using (PluginSettings setting = new PluginSettings())
                        _Data = setting.GetObject<Dictionary<string, object>>(plugin, Name);
                    return true;
                }
                catch
                {
                    return false;
                }

            }

        }



    }
}
