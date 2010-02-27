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
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using OpenMobile.helperFunctions;
namespace OpenMobile.Data
{
    /// <summary>
    /// An event on a calendar
    /// </summary>
    public struct calendarEvent
    {
        /// <summary>
        /// Event Title
        /// </summary>
        public string title;
        /// <summary>
        /// Event start time
        /// </summary>
        public DateTime startTime;
        /// <summary>
        /// Event end time
        /// </summary>
        public DateTime endTime;
        /// <summary>
        /// Event description
        /// </summary>
        public string description;
        /// <summary>
        /// Reminder time
        /// </summary>
        public DateTime reminder;
        /// <summary>
        /// Repeat
        /// </summary>
        public int repeat;
        /// <summary>
        /// An address or location description
        /// </summary>
        public string location;
        /// <summary>
        /// A GPS Location of the event
        /// </summary>
        public PointF locationLatLong;
        /// <summary>
        /// Event type
        /// </summary>
        public string type;
    }
    /// <summary>
    /// Calendar
    /// </summary>
    public sealed class Calendar:IDisposable
    {
        /// <summary>
        /// Removes past events
        /// </summary>
        public void purgeOld()
        {
            throw new NotImplementedException(); //ToDo
        }

        private SQLiteConnection asyncCon;
        private SQLiteCommand asyncCmd;
        private SQLiteDataReader asyncReader;
        /// <summary>
        /// Begins an asynchronous connection to the Message database
        /// </summary>
        /// <param name="start">Search forward from this date</param>
        /// <returns>Was the call successful</returns>
        public bool beginRead(DateTime start)
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            asyncCmd = asyncCon.CreateCommand();
            asyncCmd.CommandText = "SELECT * FROM Calendar";
            asyncReader = asyncCmd.ExecuteReader();
            return true;
        }
        /// <summary>
        /// Read the next calendar event.  Used after beginRead.
        /// </summary>
        /// <param name="storeAlso">Also keep a copy in memory</param>
        /// <returns>A calendar event or an empty struct if done reading</returns>
        public calendarEvent readNext(bool storeAlso)
        {
            calendarEvent ret = new calendarEvent();
            if ((asyncReader == null) || (asyncReader.Read() == false))
                return ret;
            ret.description = asyncReader["Description"].ToString();
            ret.endTime = DateTime.Parse(asyncReader["End"].ToString());
            ret.location = asyncReader["Location"].ToString();
            float lat = asyncReader.GetFloat(asyncReader.GetOrdinal("LocationLat"));
            float lng = asyncReader.GetFloat(asyncReader.GetOrdinal("LocationLong"));
            ret.locationLatLong = new PointF(lat, lng);
            ret.reminder = DateTime.Parse(asyncReader["Reminder"].ToString());
            ret.repeat = asyncReader.GetInt32(asyncReader.GetOrdinal("Repeat"));
            ret.startTime = DateTime.Parse(asyncReader["Start"].ToString());
            ret.title = asyncReader["Title"].ToString();
            return ret;
        }
        /// <summary>
        /// Closes the connection to the database
        /// </summary>
        public void Close()
        {
            if (asyncReader!=null)
                asyncReader.Close();
            if (asyncCon!=null)
                asyncCon.Dispose();
            asyncReader = null;
            asyncCon = null;
        }
        /// <summary>
        /// Begins writing calendar events to the database
        /// </summary>
        /// <returns></returns>
        public bool beginWrite()
        {
            asyncCon = new SQLiteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            asyncCon.Open();
            asyncCmd = asyncCon.CreateCommand();
            return true;
        }
        /// <summary>
        /// Write the next calendar entry to the database
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool writeNext(calendarEvent e)
        {
            if (asyncCmd == null)
                return false;
            StringBuilder query = new StringBuilder("INSERT INTO Calendar ('Description','End','Location','LocationLat','LocationLong','Reminder','Repeat','Start','Title')VALUES('");
            {
                query.Append(General.escape(e.description));
                query.Append("','");
                query.Append(e.endTime);
                query.Append("','");
                query.Append(General.escape(e.location));
                query.Append("','");
                query.Append(e.locationLatLong.X);
                query.Append("','");
                query.Append(e.locationLatLong.Y);
                query.Append("','");
                query.Append(e.reminder);
                query.Append("','");
                query.Append(e.repeat);
                query.Append("','");
                query.Append(e.startTime);
                query.Append("','");
                query.Append(General.escape(e.title));
                query.Append("')");
            }
            asyncCmd.CommandText = query.ToString();
            return (asyncCmd.ExecuteNonQuery() == 1);
        }
        /// <summary>
        /// Closes the connection and unloads the class
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}