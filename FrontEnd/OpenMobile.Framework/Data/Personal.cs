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
using System.Windows.Forms;
using System.Text;

namespace OpenMobile.Data
{
    /// <summary>
    /// Users Personal Information
    /// </summary>
    public static class Personal
    {
        /// <summary>
        /// Personal info
        /// </summary>
        public struct personalInfo
        {
            /// <summary>
            /// Connected Service ID
            /// </summary>
            public string connectedServices;
            /// <summary>
            /// Email Address
            /// </summary>
            public string emailAddress;
            /// <summary>
            /// Email Username
            /// </summary>
            public string emailUsername;
            /// <summary>
            /// Email Password
            /// </summary>
            internal string emailPassword;
            /// <summary>
            /// Google Username
            /// </summary>
            public string googleUsername;
            /// <summary>
            /// Google Password
            /// </summary>
            internal string googlePassword;
            /// <summary>
            /// Other Password
            /// </summary>
            internal string otherPassword;
            /// <summary>
            /// Pop Server
            /// </summary>
            public string popServer;
            /// <summary>
            /// Smtp Server
            /// </summary>
            public string smtpServer;
        }

        /// <summary>
        /// Load (or reload) a users personal info.
        /// </summary>
        /// <returns>If successful</returns>
        public static bool readInfo()
        {
            SQLiteConnection con = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Pooling=True;Max Pool Size=6;");
            con.Open();
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM Personal";
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() == true)
            {
                personalInfo info = new personalInfo();
                info.connectedServices = reader["connectedServicesID"].ToString();
                info.emailAddress = reader["emailAddress"].ToString();
                info.emailPassword = reader["emailPassword"].ToString();
                info.emailUsername = reader["emailUsername"].ToString();
                info.googlePassword = reader["googlePassword"].ToString();
                info.googleUsername = reader["googleUsername"].ToString();
                info.otherPassword = reader["otherPassword"].ToString();
                info.popServer = reader["popServer"].ToString();
                info.smtpServer = reader["smtpServer"].ToString();
                Collections.personalInfo = info;
                reader.Close();
                con.Close();
                return true;
            }
            else
            {
                reader.Close();
                con.Close();
                return false;
            }
        }
        /// <summary>
        /// Password to retrieve
        /// </summary>
        public enum ePassword
        {
            /// <summary>
            /// Email Password
            /// </summary>
            email,
            /// <summary>
            /// Google Password
            /// </summary>
            google,
            /// <summary>
            /// Reserved
            /// </summary>
            reserved1,
            /// <summary>
            /// Reserved
            /// </summary>
            reserved2,
            /// <summary>
            /// Reserved
            /// </summary>
            reserved3,
            /// <summary>
            /// Reserved
            /// </summary>
            reserved4
        };
        /// <summary>
        /// Retrieve a password from the encrypted database
        /// </summary>
        /// <param name="passType">Password to retrieve</param>
        /// <param name="appKey">Application Decryption Identifier</param>
        /// <returns>The decrypted password</returns>
        public static string getPassword(ePassword passType, string appKey)
        {
            if (appKey.Length != 8)
                return "";
            switch (passType)
            {
                case ePassword.email:
                    return Encryption.AESDecrypt(Collections.personalInfo.emailPassword, appKey);
                case ePassword.google:
                    return Encryption.AESDecrypt(Collections.personalInfo.googlePassword, appKey);
                case ePassword.reserved1:
                    return Encryption.AESDecrypt(Collections.personalInfo.otherPassword, appKey);
                default:
                    return "NotYetImplemented!";
            }
        }
        /// <summary>
        /// Send a password to encrypted storage using the given appkey
        /// </summary>
        /// <param name="passType"></param>
        /// <param name="password"></param>
        /// <param name="appKey"></param>
        public static void setPassword(ePassword passType, string password, string appKey)
        {
            switch (passType)
            {

                case ePassword.email:
                    Collections.personalInfo.emailPassword= Encryption.AESEncrypt(password, appKey);
                    return;
                case ePassword.google:
                    Collections.personalInfo.googlePassword= Encryption.AESEncrypt(password, appKey);
                    return;
                case ePassword.reserved1:
                    Collections.personalInfo.otherPassword= Encryption.AESEncrypt(password, appKey);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Writes the given personal info to the database
        /// </summary>
        /// <returns></returns>
        public static bool writeInfo()
        {
            personalInfo info = Collections.personalInfo;
            SQLiteConnection con = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Pooling=True;Max Pool Size=6;");
            con.Open();
            SQLiteCommand cmd = con.CreateCommand();
            StringBuilder query = new StringBuilder("DELETE FROM Personal;INSERT INTO Personal ('connectedServicesID','emailAddress','emailPassword','emailUsername','googlePassword','googleUsername','otherPassword','popServer','smtpServer')VALUES('");
            {
                query.Append(info.connectedServices);
                query.Append("','");
                query.Append(info.emailAddress);
                query.Append("','");
                query.Append(info.emailPassword);
                query.Append("','");
                query.Append(info.emailUsername);
                query.Append("','");
                query.Append(info.googlePassword);
                query.Append("','");
                query.Append(info.googleUsername);
                query.Append("','");
                query.Append(info.otherPassword);
                query.Append("','");
                query.Append(info.popServer);
                query.Append("','");
                query.Append(info.smtpServer);
                query.Append("')");
            }
            cmd.CommandText = query.ToString();
            bool result = (cmd.ExecuteNonQuery() == 1);
            con.Close();
            return result;
        }
    }
}
