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
using System.Text;
using System.Windows.Forms;
using OpenMobile.helperFunctions;

namespace OpenMobile.Data
{
    /// <summary>
    /// For storing Messages
    /// </summary>
    public sealed class Messages:IDisposable
    {
        /// <summary>
        /// Message Flags
        /// </summary>
        [Flags]
        public enum flags {
            /// <summary>
            /// Message Unread
            /// </summary>
            messageUnread = 0,
            /// <summary>
            /// Message Read
            /// </summary>
            messageRead = 1,
            /// <summary>
            /// Message is Important
            /// </summary>
            messageImportant = 2,
            /// <summary>
            /// Message has attachment
            /// </summary>
            messageHasAttachment = 4,
            /// <summary>
            /// Message has been responded to
            /// </summary>
            messageRespondedTo = 8,
            /// <summary>
            /// Message is an SMS
            /// </summary>
            messageIsSMS=16
        };
        /// <summary>
        /// A message
        /// </summary>
        public struct message
        {
            /// <summary>
            /// Message Subject
            /// </summary>
            public string subject;
            /// <summary>
            /// Message Content
            /// </summary>
            public string content;
            /// <summary>
            /// Message From Address
            /// </summary>
            public string fromAddress;
            /// <summary>
            /// Message From Name
            /// </summary>
            public string fromName;
            /// <summary>
            /// Message Flags
            /// </summary>
            public flags messageFlags;
            /// <summary>
            /// Message Received
            /// </summary>
            public DateTime messageReceived;
            /// <summary>
            /// Message Attachment
            /// </summary>
            public byte[] attachment;
            /// <summary>
            /// Message Guid
            /// </summary>
            public Int64 guid;
        }

        /// <summary>
        /// Load (or reload) each Message into memory.
        /// </summary>
        /// <returns>Number of Messages loaded</returns>
        public static int readMessages()
        {
            SQLiteConnection con = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM Message";
            con.Open();
            SQLiteDataReader reader = cmd.ExecuteReader();
            Collections.Messages.Clear();
            while (reader.Read())
            {
                message info = new message();
                info.content = reader["Content"].ToString();
                info.fromAddress = reader["fromEmailOrNum"].ToString();
                info.fromName = reader["fromName"].ToString();
                info.guid = Convert.ToInt64(reader["ID"]);
                info.messageFlags = (flags)reader.GetByte(reader.GetOrdinal("Flags"));
                info.messageReceived = reader.GetDateTime(reader.GetOrdinal("Date"));
                info.subject = reader["Subject"].ToString();
                Collections.Messages.Add(info);
            }
            reader.Close();
            con.Close();
            return Collections.Messages.Count;
        }

        static SQLiteConnection asyncCon;
        static SQLiteCommand asyncCmd;
        static SQLiteDataReader asyncReader;

        /// <summary>
        /// Begins an asynchronous connection to the Message database
        /// </summary>
        /// <returns>Was the call successful</returns>
        public bool beginReadMessages()
        {
            try{
                asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
                asyncCmd = asyncCon.CreateCommand();
                asyncCmd.CommandText = "SELECT * FROM Message";
                asyncCon.Open();
                asyncReader = asyncCmd.ExecuteReader();
            }catch(Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Retrieves the message with the given guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool beginReadSingleMessage(long guid)
        {
            try
            {
                asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
                asyncCmd = asyncCon.CreateCommand();
                asyncCmd.CommandText = "SELECT * FROM Message WHERE ID='"+guid+"'";
                asyncCon.Open();
                asyncReader = asyncCmd.ExecuteReader();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read the next message.  Used after beginReadMessages.
        /// </summary>
        /// <param name="storeAlso">Also keep a copy in memory</param>
        /// <returns>A message or null if out of messages</returns>
        public message readNext(bool storeAlso){
            if (asyncReader.Read() == false)
                return new message();
            message info = new message();
            info.content = asyncReader["Content"].ToString();
            info.fromAddress = asyncReader["fromEmailOrNum"].ToString();
            info.fromName = asyncReader["fromName"].ToString();
            info.guid = Convert.ToInt64(asyncReader["ID"]);
            info.messageFlags = (flags)Enum.Parse(typeof(flags), asyncReader["Flags"].ToString());
            info.messageReceived = DateTime.Parse(asyncReader["Date"].ToString());
            info.subject = asyncReader["Subject"].ToString();
            if (storeAlso == true)
                Collections.Messages.Add(info);
            return info;
        }
        /// <summary>
        /// Closes the database connection
        /// </summary>
        public void Close()
        {
            if (asyncReader!=null)
                asyncReader.Dispose();
            if (asyncCon != null)
                asyncCon.Dispose();
            if (asyncCmd != null)
                asyncCmd.Dispose();
            asyncReader = null;
            asyncCon = null;
            asyncCmd = null;
        }

        /// <summary>
        /// Writes the message to the database
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool writeNext(message m)
        {
            if (asyncCmd == null)
                return false;
            StringBuilder query = new StringBuilder("INSERT INTO Message ('Content','fromEmailOrNum','fromName','Flags','Date','Subject')VALUES('");
            {
                query.Append(General.escape(m.content));
                query.Append("','");
                query.Append(m.fromAddress);
                query.Append("','");
                query.Append(General.escape(m.fromName));
                query.Append("','");
                query.Append((int)m.messageFlags);
                query.Append("','");
                query.Append(m.messageReceived);
                query.Append("','");
                query.Append(General.escape(m.subject));
                query.Append("')");
            }
            asyncCmd.CommandText = query.ToString();
            return (asyncCmd.ExecuteNonQuery() == 1);
        }
        /// <summary>
        /// Begins an asynchronous connection to the database
        /// </summary>
        /// <returns></returns>
        public bool beginWriteMessages()
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            asyncCmd = asyncCon.CreateCommand();
            return true;
        }
        /// <summary>
        /// Deletes the message with the given ID.  beginWriteMessages should be called first
        /// </summary>
        /// <param name="guid"></param>
        public bool deleteMessage(long guid)
        {
            if (asyncCmd == null)
                return false;
            asyncCmd.CommandText = "DELETE FROM Message WHERE ID='" + guid + "'";
            return (asyncCmd.ExecuteNonQuery() > 0);
        }
        #region IDisposable Members
        /// <summary>
        /// Closes the database connection and releases used resources
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
