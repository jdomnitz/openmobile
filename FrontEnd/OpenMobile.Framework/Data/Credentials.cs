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
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Data.SQLite;
using OpenMobile.helperFunctions;

namespace OpenMobile.Data
{
    /// <summary>
    /// Stores secure user credentials
    /// </summary>
    public static class Credentials
    {
        private static SQLiteConnection con;
        /// <summary>
        /// Reserved
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="requestedAccess"></param>
        /// <returns></returns>
        public delegate bool Authorization(string pluginName, string requestedAccess);
        /// <summary>
        /// Reserved
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static event Authorization OnAuthorizationRequested;
        private static void createDB()
        {
            SQLiteCommand cmd = new SQLiteCommand(con);
            cmd.CommandText = "BEGIN TRANSACTION;CREATE TABLE tblCache(UID INTEGER PRIMARY KEY, EncryptedName TEXT, Value TEXT);CREATE TABLE tblAccess(UID INTEGER, AssemblyHash TEXT);COMMIT;";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            { }
            finally { cmd.Dispose(); }
        }
        /// <summary>
        /// Used internally
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public static void Open()
        {
            try
            {
                if (uid == null)
                {
                    uid = String.Empty;
                    for (int i = 0; i < Environment.UserName.Length; i++)
                        uid += (char)(Environment.UserName[i] << Environment.ProcessorCount);
                }
                if (con == null)
                    con = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMSecure") + ";Version=3;FailIfMissing=True");
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA locking_mode='Exclusive';BEGIN EXCLUSIVE;DELETE FROM tblCache WHERE UID=0;INSERT INTO tblCache (UID,EncryptedName,Value)VALUES('0','Lock',time('now'));COMMIT", con))
                        cmd.ExecuteNonQuery();
                }
            }
            catch (SQLiteException)
            {
                if (con != null)
                    con.Dispose();
                con = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMSecure") + ";Version=3;FailIfMissing=False;");
                con.Open();
                createDB();
                using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA locking_mode='Exclusive';BEGIN EXCLUSIVE;DELETE FROM tblCache WHERE UID=0;INSERT INTO tblCache (UID,EncryptedName,Value)VALUES('0','Lock',time('now'));COMMIT", con))
                    cmd.ExecuteNonQuery();
            }
        }
        private static List<string> blockedHashes = new List<string>();
        private static List<string> allowedHashes = new List<string>();
        private static string uid;
        /// <summary>
        /// Retrieves the given credential
        /// </summary>
        /// <param name="credentialName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string getCredential(string credentialName)
        {
            if (String.IsNullOrEmpty(credentialName))
                return null;
            Open();
            lock (con)
            {
                string hash = Assembly.GetCallingAssembly().GetModules()[0].ModuleVersionId.ToString();
                if (blockedHashes.Contains(hash))
                    return null;
                string md5 = Encryption.md5Encode(credentialName + uid);
                SQLiteCommand cmd = new SQLiteCommand(con);
                cmd.CommandText = "SELECT Value from tblCache where EncryptedName='" + md5 + "'";
                object ret = cmd.ExecuteScalar();
                string value;
                if (ret != null)
                    value = ret.ToString();
                else
                    return null;
                if (allowedHashes.Contains(hash + credentialName))
                    return Encryption.AESDecrypt(value, credentialName);
                cmd.CommandText = "SELECT UID from tblCache WHERE EncryptedName='" + md5 + "'";
                ret = cmd.ExecuteScalar();
                string UID;
                if (ret != null)
                    UID = ret.ToString();
                else
                    return null;
                cmd.CommandText = "SELECT Count(*) from tblAccess WHERE UID='" + UID + "' AND AssemblyHash='" + hash + "'";
                int count = (int)(Int64)cmd.ExecuteScalar();
                allowedHashes.Add(hash + credentialName);
                if (count > 0)
                    return Encryption.AESDecrypt(value, credentialName);
                else
                    if (promptAccess(Assembly.GetCallingAssembly().GetModules()[0].Name, credentialName))
                    {
                        cmd.CommandText = "INSERT INTO tblAccess (UID,AssemblyHash)VALUES('" + UID + "','" + hash + "')";
                        cmd.ExecuteNonQuery();
                        return Encryption.AESDecrypt(value, credentialName);
                    }
                    else
                        blockedHashes.Add(hash);
                return null;
            }
        }

        private static bool promptAccess(string pluginName, string access)
        {
            if (OnAuthorizationRequested == null)
                return false;
            if (OnAuthorizationRequested.GetInvocationList().Length > 1)
                throw new System.Security.SecurityException(OnAuthorizationRequested.GetInvocationList()[1].Method.Module.Name + " attempted to circumvent OM security!  Protection mode enabled!");
            if (OnAuthorizationRequested(pluginName, access))
                return true;
            return false;
        }
        /// <summary>
        /// Sets the given credential to the given value
        /// </summary>
        /// <param name="credentialName"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void setCredential(string credentialName, string value)
        {
            if (value == null)
                return;
            if (String.IsNullOrEmpty(credentialName))
                return;
            Open();
            lock (con)
            {
                string hash = Assembly.GetCallingAssembly().GetModules()[0].ModuleVersionId.ToString();
                if (blockedHashes.Contains(hash))
                    return;
                SQLiteCommand cmd = new SQLiteCommand(con);
                string md5 = Encryption.md5Encode(credentialName + uid);
                cmd.CommandText = "SELECT Count(*) from tblCache WHERE EncryptedName='" + md5 + "'";
                int count = (int)(Int64)cmd.ExecuteScalar();
                string UID;
                if (count > 0)
                {
                    cmd.CommandText = "SELECT UID from tblCache WHERE EncryptedName='" + md5 + "'";
                    object ret = cmd.ExecuteScalar();
                    if (ret != null)
                        UID = ret.ToString();
                    else
                        return;
                    if (allowedHashes.Contains(hash + credentialName))
                        count = 1;
                    else
                    {
                        cmd.CommandText = "SELECT Count(*) from tblAccess WHERE UID='" + UID + "' AND AssemblyHash='" + hash + "'";
                        count = (int)(Int64)cmd.ExecuteScalar();
                    }
                    if (count > 0)
                    {
                        cmd.CommandText = "UPDATE tblCache SET Value='" + Encryption.AESEncrypt(value, credentialName) + "' WHERE EncryptedName='" + md5 + "'";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        if (promptAccess(Assembly.GetCallingAssembly().GetModules()[0].Name, credentialName))
                        {
                            cmd.CommandText = "SELECT UID from tblCache WHERE EncryptedName='" + md5 + "'";
                            UID = cmd.ExecuteScalar().ToString();
                            cmd.CommandText = "INSERT OR REPLACE INTO tblAccess (UID,AssemblyHash)VALUES('" + UID + "','" + hash + "')";
                            cmd.ExecuteNonQuery();
                        }
                        else
                            blockedHashes.Add(hash);
                    }
                }
                else
                {
                    if (promptAccess(Assembly.GetCallingAssembly().GetModules()[0].Name, credentialName))
                    {
                        cmd.CommandText = "INSERT INTO tblCache (Value,EncryptedName)VALUES('" + Encryption.AESEncrypt(value, credentialName) + "','" + md5 + "')";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT UID from tblCache WHERE EncryptedName='" + md5 + "'";
                        UID = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "INSERT OR REPLACE INTO tblAccess (UID,AssemblyHash)VALUES('" + UID + "','" + hash + "')";
                        cmd.ExecuteNonQuery();
                    }
                    else
                        blockedHashes.Add(hash);
                }
            }
        }
    }
}
