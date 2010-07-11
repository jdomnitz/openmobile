﻿/*********************************************************************************
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
using Mono.Data.Sqlite;
using System;
using OpenMobile;
using System.Text;
using OpenMobile.helperFunctions;

namespace OpenMobile.Data
{
    public enum eTrafficType
    {
        Unknown=0,
        Construction=1,
        Accident=2,
        LaneClosure=3,
        RoadClosure=4,
        Traffic=5,
        Event=6,
        Other=10
    }
    public struct TrafficItem
    {
        public DateTime Start;
        public DateTime End;
        public string Title;
        public string Description;
        public eTrafficType Type;
        public int severity;
        public eDirection Direction;
        public Location Location;
        public TrafficItem(string location)
        {
            Location = new Location();
            Location.Name = location;
            Title = "";
            Description = "";
            Start = DateTime.MinValue;
            End = DateTime.MinValue;
            Type = eTrafficType.Unknown;
            severity = 0;
            Direction = eDirection.NotSet;
        }
    }

    public class Traffic:IDisposable
    {
        private SqliteConnection con;
        private SqliteDataReader asyncReader;
        private bool tableCreated;
        public Traffic()
        {
            con = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;FailIfMissing=True;temp_store=2");
            con.Open();
        }
        public bool createTable()
        {
            using(SqliteCommand cmd = new SqliteCommand(con))
            {
                cmd.CommandText = "CREATE TABLE Traffic (startDate Julian, endDate Julian, title TEXT, description TEXT, type INTEGER, severity INTEGER, direction INTEGER, longitude NUMERIC, latitude NUMERIC, location TEXT);";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqliteException) { return false; }
                return true;
            }
        }

        public bool writeTraffic(TrafficItem item)
        {
            using (SqliteCommand cmd = new SqliteCommand(con))
            {
                StringBuilder query=new StringBuilder("INSERT INTO Traffic (startDate, endDate, title, description, type, severity, direction, longitude, latitude, location)VALUES('");
                query.Append(item.Start.ToString());
                query.Append("','");
                query.Append(item.End.ToString());
                query.Append("','");
                query.Append(General.escape(item.Title));
                query.Append("','");
                query.Append(General.escape(item.Description));
                query.Append("','");
                query.Append(((int)item.Type).ToString());
                query.Append("','");
                query.Append(item.severity.ToString());
                query.Append("','");
                query.Append(((int)item.Direction).ToString());
                query.Append("','");
                query.Append(item.Location.Longitude.ToString());
                query.Append("','");
                query.Append(item.Location.Latitude.ToString());
                query.Append("','");
                query.Append(General.escape(item.Location.Name));
                query.Append("')");
                cmd.CommandText = query.ToString();
                try
                {
                    return (cmd.ExecuteNonQuery() != 0);
                }
                catch (SqliteException e)
                {
                    if (e.Message.Contains("no such table"))
                    {
                        if (tableCreated)
                            return false;
                        tableCreated = true;
                        if(createTable())
                            return (cmd.ExecuteNonQuery() != 0);
                    }
                }
                return false;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            con.Dispose();
            if (asyncReader != null)
                asyncReader.Dispose();
        }

        #endregion
    }
}
