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
using System.Data.SQLite;
using OpenMobile.helperFunctions;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using OpenMobile.Plugin;

namespace OpenMobile.Data
{
    /// <summary>
    /// Stores and retrieves settings from the database
    /// </summary>
    public sealed class PluginSettings : IDisposable
    {
        /// <summary>
        /// Returns a fully qualified setting name
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string SettingNameBuilder(IBasePlugin plugin, string name)
        {
            return String.Format("{0};{1}", (plugin == null ? "" : plugin.pluginName), name);
        }

        /// <summary>
        /// True = Database tables created
        /// </summary>
        public bool DBCreated { get; set; }

        SQLiteConnection asyncCon;
        /// <summary>
        /// Stores and retrieves settings from the database
        /// </summary>
        public PluginSettings()
        {
            bool CreateDatabase = false;

            // If DB file is present, then create database inside
            if (!System.IO.File.Exists(Path.Combine(BuiltInComponents.Host.DataPath, "OMData")))
                CreateDatabase = true;

            // Connect to SQLite and create DB file
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;temp_store=2");
            asyncCon.Open();

            // If DB file is present, then create tables
            if (CreateDatabase)
                createDB();

        }
        /// <summary>
        /// Called automatically by the UI if a database does not yet exist
        /// </summary>
        private void createDB()
        {
            if (!DBCreated)
            {
                SQLiteCommand cmd = new SQLiteCommand(OpenMobile.Framework.Data.SQL.OMData, asyncCon);
                cmd.ExecuteNonQuery();
                DBCreated = true;
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info,"Database created (OMData)");
            }
        }
        /// <summary>
        /// Gets the setting with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string getSetting(IBasePlugin plugin, string name)
        {
            name = SettingNameBuilder(plugin, name);
            try
            {
                if (asyncCon.State == System.Data.ConnectionState.Closed)
                    asyncCon.Open();
                using (SQLiteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.CommandText = "SELECT Value FROM PluginSettings WHERE Name='" + name + "'";
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                        return String.Empty;
                    return result.ToString();
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        /// <summary>
        /// Sets the setting with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool setSetting(IBasePlugin plugin,  string name, string value)
        {
            name = SettingNameBuilder(plugin, name);
            using (SQLiteCommand cmd = asyncCon.CreateCommand())
            {
                cmd.CommandText = "UPDATE OR REPLACE PluginSettings SET Value='" + General.escape(value) + "' WHERE Name='" + General.escape(name) + "'";
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)//Row doesn't exist yet..has to be two queries since Sqlite doesn't support IF statements
                {
                    cmd.CommandText = "INSERT INTO PluginSettings (Name,Value)VALUES('" + General.escape(name) + "','" + General.escape(value) + "')";
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                return (rowsAffected > 0);
            }
        }

        /// <summary>
        /// Deletes the setting with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool DeleteSetting(IBasePlugin plugin, string name)
        {
            name = SettingNameBuilder(plugin, name);
            using (SQLiteCommand cmd = asyncCon.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM PluginSettings WHERE Name='" + General.escape(name) + "'";
                int rowsAffected = cmd.ExecuteNonQuery();
                return (rowsAffected > 0);
            }
        }

        /// <summary>
        /// Gets an array of values (saved using screen notation)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screenCount"></param>
        /// <returns></returns>
        public string[] getAllInstances(IBasePlugin plugin, string name, int screenCount)
        {
            string[] ret = new string[screenCount];
            for (int i = 0; i < screenCount; i++)
                ret[i] = getSetting(plugin, name + ".Screen" + (i + 1).ToString());
            return ret;
        }
        /// <summary>
        /// Sets an array of values (saved using screen notation)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool setAllInstances(IBasePlugin plugin, string name, string[] values)
        {
            if (values == null)
                return false;
            for (int i = 0; i < values.Length; i++)
                setSetting(plugin, name + ".Screen" + (i + 1).ToString(), values[i]);
            return true;
        }

        /// <summary>
        /// Gets all the settings with a given prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public Dictionary<String, String> getAllPluginSettings(string prefix)
        {
            try
            {
                Dictionary<String, String> Dic = new Dictionary<String, String>();

                if (asyncCon.State == System.Data.ConnectionState.Closed)
                    asyncCon.Open();
                using (SQLiteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name, Value FROM PluginSettings WHERE Name like '" + prefix + "%'";
                    SQLiteDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        Dic.Add(rdr.GetString(0), rdr.GetString(1));

                    rdr.Close();
                }

                return Dic;
            }
            catch (Exception)
            {
                return new Dictionary<String, String>();
            }
        }

        /// <summary>
        /// Gets the object with the given name from the DB
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetObject<T>(IBasePlugin plugin, string name)
        {
            T value;
            TryGetObject<T>(plugin, name, out value);
            return value;
        }

        /// <summary>
        /// Tries to get the object with the given name from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetObject<T>(IBasePlugin plugin, string name, out T value)
        {
            value = default(T);

            name = SettingNameBuilder(plugin, name);
            try
            {
                if (asyncCon.State == System.Data.ConnectionState.Closed)
                    asyncCon.Open();
                using (SQLiteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.CommandText = "SELECT Value FROM Objects WHERE Name='" + name + "'";
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                        return false;

                    // convert byte array to memory stream
                    using (MemoryStream ms = new MemoryStream((byte[])result))
                    {
                        // create new BinaryFormatter
                        BinaryFormatter _BinaryFormatter = new BinaryFormatter();

                        // set memory stream position to starting point
                        ms.Position = 0;

                        // Deserializes a stream into an object graph and return as a object.
                        value = (T)_BinaryFormatter.Deserialize(ms);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the object with the given name in DB
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetObject(IBasePlugin plugin, string name, object value)
        {
            name = SettingNameBuilder(plugin, name);

            // Convert object to memorystream
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, value);
                }
                catch (Exception e)
                {
                    BuiltInComponents.Host.DebugMsg("ObjectDataBase: Unable to save object to database", e);
                    return false;
                }

                // Save object to database
                using (SQLiteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.Parameters.Add(new SQLiteParameter("@name", General.escape(name)));
                    cmd.Parameters.Add(new SQLiteParameter("@data", ms.ToArray()));

                    cmd.CommandText = "UPDATE OR REPLACE Objects SET Value=@data WHERE Name=@name";
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)//Row doesn't exist yet..has to be two queries since Sqlite doesn't support IF statements
                    {
                        cmd.CommandText = "INSERT INTO Objects (Name,Value)VALUES(@name,@data)";
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                    return (rowsAffected > 0);
                }
            }
        }

        /// <summary>
        /// Saves an object to the DB as an xml string
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetObjectXML(IBasePlugin plugin, string name, object value)
        {
            name = SettingNameBuilder(plugin, name);

            string xml;
            try
            {
                xml = OpenMobile.helperFunctions.XML.Serializer.toXML(value);
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg("ObjectDataBase: Unable to save object(XML) to database", e);
                return false;
            }

            // Save object to database
            using (SQLiteCommand cmd = asyncCon.CreateCommand())
            {
                cmd.Parameters.Add(new SQLiteParameter("@name", General.escape(name)));
                cmd.Parameters.Add(new SQLiteParameter("@data", System.Text.UTF8Encoding.UTF8.GetBytes(xml)));

                cmd.CommandText = "UPDATE OR REPLACE Objects SET Value=@data WHERE Name=@name";
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)//Row doesn't exist yet..has to be two queries since Sqlite doesn't support IF statements
                {
                    cmd.CommandText = "INSERT INTO Objects (Name,Value)VALUES(@name,@data)";
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                return (rowsAffected > 0);
            }

        }

        /// <summary>
        /// Tries to get the object saved as an xml string with the given name from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetObjectXML<T>(IBasePlugin plugin, string name, out T value)
        {
            value = default(T);

            name = SettingNameBuilder(plugin, name);
            try
            {
                if (asyncCon.State == System.Data.ConnectionState.Closed)
                    asyncCon.Open();
                using (SQLiteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.CommandText = "SELECT Value FROM Objects WHERE Name='" + name + "'";
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                        return false;

                    string xml = System.Text.UTF8Encoding.UTF8.GetString((byte[])result);
                    value = (T)OpenMobile.helperFunctions.XML.Serializer.fromXML<T>(xml);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the object saved as an xml string with the given name from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetObjectXML<T>(IBasePlugin plugin, string name)
        {
            T value;
            TryGetObjectXML<T>(plugin, name, out value);
            return value;
        }

        /// <summary>
        /// Deletes the object with the given name from the DB
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool DeleteObject(IBasePlugin plugin, string name)
        {
            name = SettingNameBuilder(plugin, name);
            using (SQLiteCommand cmd = asyncCon.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Objects WHERE Name='" + General.escape(name) + "'";
                int rowsAffected = cmd.ExecuteNonQuery();
                return (rowsAffected > 0);
            }
        }



        #region IDisposable Members

        /// <summary>
        /// Cleans up any objects in use
        /// </summary>
        public void Dispose()
        {
            asyncCon.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
