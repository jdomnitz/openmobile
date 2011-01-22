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
using Mono.Data.Sqlite;
using OpenMobile.helperFunctions;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;

namespace OpenMobile.Data
{
    /// <summary>
    /// Stores and retrieves settings from the database
    /// </summary>
    public sealed class PluginSettings : IDisposable
    {
        SqliteConnection asyncCon;
        /// <summary>
        /// Stores and retrieves settings from the database
        /// </summary>
        public PluginSettings()
        {
            asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;temp_store=2");
            asyncCon.Open();
        }
        /// <summary>
        /// Called automatically by the UI if a database does not yet exist
        /// </summary>
        public void createDB()
        {
            SqliteCommand cmd = new SqliteCommand(OpenMobile.Framework.Data.SQL.OMData, asyncCon);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Gets the setting with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string getSetting(string name)
        {
            try
            {
                if (asyncCon.State == System.Data.ConnectionState.Closed)
                    asyncCon.Open();
                using (SqliteCommand cmd = asyncCon.CreateCommand())
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
        public bool setSetting(string name, string value)
        {
            using (SqliteCommand cmd = asyncCon.CreateCommand())
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
        /// Gets an array of values (saved using screen notation)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screenCount"></param>
        /// <returns></returns>
        public string[] getAllInstances(string name, int screenCount)
        {
            string[] ret = new string[screenCount];
            for (int i = 0; i < screenCount; i++)
                ret[i] = getSetting(name + ".Screen" + (i + 1).ToString());
            return ret;
        }
        /// <summary>
        /// Sets an array of values (saved using screen notation)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool setAllInstances(string name, string[] values)
        {
            if (values == null)
                return false;
            for (int i = 0; i < values.Length; i++)
                setSetting(name + ".Screen" + (i + 1).ToString(), values[i]);
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
                using (SqliteCommand cmd = asyncCon.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name, Value FROM PluginSettings WHERE Name like '" + prefix + "%'";
                    SqliteDataReader rdr = cmd.ExecuteReader();

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
