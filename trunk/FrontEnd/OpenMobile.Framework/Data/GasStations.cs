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
    /// Gas Stations and Fuel Prices
    /// </summary>
    public sealed class GasStations : IDisposable
    {
        /// <summary>
        /// Gas Station Information
        /// </summary>
        public struct gasStation
        {
            /// <summary>
            /// Gas Station Name
            /// </summary>
            public string name;
            /// <summary>
            /// Gas Station Location
            /// </summary>
            public string location;
            /// <summary>
            /// Price for Regular
            /// </summary>
            public string priceRegular;
            /// <summary>
            /// Price for Plus
            /// </summary>
            public string pricePlus;
            /// <summary>
            /// Price for Premium
            /// </summary>
            public string pricePremium;
            /// <summary>
            /// The highest level octane (Note: only called ultimate by one brand, other names may vary)
            /// </summary>
            public string priceUltimate;
            /// <summary>
            /// Price for Diesel
            /// </summary>
            public string priceDiesel;
            /// <summary>
            /// Zipcode
            /// </summary>
            public string zipcode;
            /// <summary>
            /// Date Added
            /// </summary>
            public DateTime dateAdded;
        }

        /// <summary>
        /// Remove all items older then 4 days from the database
        /// </summary>
        public void purgeOld()
        {
            SqliteConnection con = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM gasRegions WHERE  StationID IN (SELECT GUID FROM gasStations WHERE dateAdded<date('now','-4 days'))";
            con.Open();
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DELETE FROM gasStations WHERE dateAdded<date('now','-4 days')";
            cmd.ExecuteNonQuery();
        }
        SqliteConnection asyncCon;
        SqliteCommand asyncCmd;
        SqliteDataReader asyncReader;

        /// <summary>
        /// Begins an asynchronous connection to the Message database
        /// </summary>
        /// <param name="zipcode">Zipcode to search in</param>
        /// <returns>Was the call successful</returns>
        public bool beginRead(string zipcode)
        {
            try
            {
                if (asyncCon == null)
                    asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;FailIfMissing=True;");
                asyncCmd = asyncCon.CreateCommand();
                asyncCmd.CommandText = "SELECT Zip,dateAdded,Location,Name,priceDiesel,priceRegular,pricePlus,pricePremium,priceUltimate FROM gasRegions JOIN gasStations ON StationID=GUID WHERE Zip=" + zipcode;
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
        /// Closes the connection to the database
        /// </summary>
        public void Close()
        {
            try
            {
                if (asyncReader != null)
                    asyncReader.Close();
                if (asyncCon != null)
                    asyncCon.Close();
                asyncReader = null;
                asyncCon = null;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Read the next gas station.  Used after beginRead.
        /// </summary>
        /// <param name="storeAlso">Also keep a copy in memory</param>
        /// <returns>A gas station or an exmpty struct if done reading</returns>
        public gasStation readNext(bool storeAlso)
        {
            if (asyncReader.Read() == false)
                return new gasStation();
            gasStation info = new gasStation();
            info.location = asyncReader["Location"].ToString();
            info.name = asyncReader["Name"].ToString();
            info.priceDiesel = asyncReader["priceDiesel"].ToString();
            info.priceRegular = asyncReader["priceRegular"].ToString();
            info.pricePlus = asyncReader["pricePlus"].ToString();
            info.pricePremium = asyncReader["pricePremium"].ToString();
            info.priceUltimate = asyncReader["priceUltimate"].ToString();
            info.dateAdded = DateTime.Parse(asyncReader["dateAdded"].ToString());
            info.zipcode = asyncReader["Zip"].ToString();
            if (storeAlso == true)
                Collections.gasStations.Add(info);
            return info;
        }
        /// <summary>
        /// Begins writing gas station data to the database
        /// </summary>
        /// <returns></returns>
        public bool beginWrite()
        {
            try
            {
                if (asyncCon == null)
                    asyncCon = new SqliteConnection(@"Data Source=" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
                asyncCon.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Write the next gas station entry to the database
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public bool writeNext(gasStation station)
        {
            asyncCmd = asyncCon.CreateCommand();
            asyncCmd.CommandText = "SELECT guid from gasStations WHERE Location='" + station.location + "'";
            object result = asyncCmd.ExecuteScalar();
            long guid = -1;
            if (result != null)
                guid = (long)asyncCmd.ExecuteScalar();
            StringBuilder query;
            if (guid != -1)
                query = new StringBuilder("REPLACE INTO gasStations (dateAdded,GUID,Location,priceRegular,pricePlus,pricePremium,priceUltimate,priceDiesel,Name)VALUES('");
            else
                query = new StringBuilder("INSERT INTO gasStations (dateAdded,Location,priceRegular,pricePlus,pricePremium,priceUltimate,priceDiesel,Name)VALUES('");
            query.Append(station.dateAdded.ToString());
            query.Append("','");
            if (guid != -1)
            {
                query.Append(guid.ToString());
                query.Append("','");
            }
            query.Append(General.escape(station.location));
            query.Append("','");
            query.Append(station.priceRegular);
            query.Append("','");
            query.Append(station.pricePlus);
            query.Append("','");
            query.Append(station.pricePremium);
            query.Append("','");
            query.Append(station.priceUltimate);
            query.Append("','");
            query.Append(station.priceDiesel);
            query.Append("','");
            query.Append(General.escape(station.name));
            query.Append("')");
            asyncCmd.CommandText = query.ToString();
            if (asyncCmd.ExecuteNonQuery() == 0)
                return false;
            if (guid == -1)
            {
                asyncCmd.CommandText = "INSERT INTO gasRegions (Zip, StationID)VALUES('" + station.zipcode + "',(SELECT GUID FROM gasStations WHERE Location='" + station.location + "'))";
                asyncCmd.ExecuteNonQuery();
            }
            return true;
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
