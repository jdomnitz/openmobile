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
using System.Text;
using OpenMobile.helperFunctions;

namespace OpenMobile.Data
{
    /// <summary>
    /// For storing Messages
    /// </summary>
    public sealed class Messages:IDisposable
    {
        public delegate void newMessage(message msg);
        public event newMessage newOutboundMessage;

        /// <summary>
        /// Message Flags
        /// </summary>
        [Flags]
        public enum flags {
            /// <summary>
            /// No Flags
            /// </summary>
            None = 0,
            /// <summary>
            /// Message Read
            /// </summary>
            Read = 1,
            /// <summary>
            /// Message is Important
            /// </summary>
            Important = 2,
            /// <summary>
            /// Message has attachment
            /// </summary>
            HasAttachment = 4,
            /// <summary>
            /// Message has been responded to
            /// </summary>
            RespondedTo = 8,
            /// <summary>
            /// Message is an SMS/MMS
            /// </summary>
            SMS=16,
            /// <summary>
            /// SPAM/Junk
            /// </summary>
            Spam=32,
            /// <summary>
            /// Message is an outbound message (may or may not be sent)
            /// </summary>
            Outbound=64,
            /// <summary>
            /// Message has been sent (should be combined with outbound)
            /// </summary>
            Sent=128,
            /// <summary>
            /// Message is a Draft (not yet sent)
            /// </summary>
            Draft=256
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
            /// Recipient
            /// </summary>
            public string toName;
            /// <summary>
            /// Message Flags
            /// </summary>
            public flags messageFlags;
            /// <summary>
            /// Message Received
            /// </summary>
            public DateTime messageReceived;
            /// <summary>
            /// Message Attachment (local URLs)
            /// </summary>
            public string[] attachment;
            /// <summary>
            /// Source Name (ex: facebook, twitter, email)
            /// </summary>
            public string sourceName;
            /// <summary>
            /// Message Guid
            /// </summary>
            public UInt64 guid;
        }

        /// <summary>
        /// Load (or reload) each Message into memory.
        /// </summary>
        /// <returns>Number of Messages loaded</returns>
        public static int readMessages()
        {
            using (Messages m = new Messages())
            {
                if (!m.beginReadMessages())
                    return 0;
                Collections.Messages.Clear();
                message msg = m.readNext(true);
                while (msg.guid > 0)
                {
                    msg = m.readNext(true);
                }
            }
            return Collections.Messages.Count;
        }

        static SqliteConnection asyncCon;
        static SqliteCommand asyncCmd;
        static SqliteDataReader asyncReader;

        /// <summary>
        /// Begins an asynchronous connection to the Message database
        /// </summary>
        /// <returns>Was the call successful</returns>
        public bool beginReadMessages()
        {
            try{
                asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
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
                asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
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
            info.guid = Convert.ToUInt64(asyncReader["ID"]);
            info.messageFlags = (flags)Enum.Parse(typeof(flags), asyncReader["Flags"].ToString());
            info.messageReceived = DateTime.Parse(asyncReader["Date"].ToString());
            info.subject = asyncReader["Subject"].ToString();
            if (asyncReader.GetOrdinal("toName") < 0)
                updateDB();
            info.sourceName = asyncReader["Source"].ToString();
            info.toName = asyncReader["toName"].ToString();
            info.attachment = asyncReader["Attachment"].ToString().Split(new char[] { '|' });
            if (storeAlso == true)
                Collections.Messages.Add(info);
            return info;
        }

        private void updateDB()
        {
            throw new NotImplementedException();
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
            if ((m.messageFlags & flags.Outbound) == flags.Outbound)
                if (newOutboundMessage != null)
                    newOutboundMessage(m);
            StringBuilder query = new StringBuilder("INSERT OR REPLACE INTO Message (");
            if (m.guid > 0)
                query.Append("'ID',");
            query.Append("'Content','fromEmailOrNum','fromName','Flags','Date','Subject','Source','toName','Attachment')VALUES('");
            {
                if (m.guid > 0)
                {
                    query.Append(m.guid.ToString());
                    query.Append("','");
                }
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
                query.Append("','");
                query.Append(General.escape(m.sourceName));
                query.Append("','");
                query.Append(General.escape(m.toName));
                query.Append("','");
                StringBuilder sb=new StringBuilder();
                if (m.attachment!=null)
                    foreach(string s in m.attachment)
                    {
                        sb.Append(s);
                        sb.Append('|');
                    }
                query.Append(General.escape((sb.Length==0) ? "":sb.ToString(0,sb.Length-1)));
                query.Append("')");
            }
            asyncCmd.CommandText = query.ToString();
            try
            {
                return (asyncCmd.ExecuteNonQuery() == 1);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// Begins an asynchronous connection to the database
        /// </summary>
        /// <returns></returns>
        public bool beginWriteMessages()
        {
            asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
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
