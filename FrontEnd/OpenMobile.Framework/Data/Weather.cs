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
    /// Weather data
    /// </summary>
    public sealed class Weather:IDisposable
    {
        /// <summary>
        /// The cardnal directions
        /// </summary>
        public enum direction
        {
            /// <summary>
            /// Default Enum Value
            /// </summary>
            NotSet=0,
            /// <summary>
            /// North
            /// </summary>
            N,
            /// <summary>
            /// West
            /// </summary>
            W,
            /// <summary>
            /// East
            /// </summary>
            E,
            /// <summary>
            /// South
            /// </summary>
            S,
            /// <summary>
            /// North-West
            /// </summary>
            NW,
            /// <summary>
            /// North-East
            /// </summary>
            NE,
            /// <summary>
            /// South-West
            /// </summary>
            SW,
            /// <summary>
            /// South-East
            /// </summary>
            SE
        }
        /// <summary>
        /// Weather Conditions
        /// </summary>
        public enum weatherConditions
        {
            /// <summary>
            /// Default Value
            /// </summary>
            NotSet=0,
            /// <summary>
            /// Clear Skies
            /// </summary>
            Clear,
            /// <summary>
            /// Mostly Clear Skies
            /// </summary>
            MostlyClear,
            /// <summary>
            /// Sunny
            /// </summary>
            Sunny,
            /// <summary>
            /// Mostly Sunny
            /// </summary>
            MostlySunny,
            /// <summary>
            /// Partly Cloudy
            /// </summary>
            PartlyCloudy,
            /// <summary>
            /// Cloudy
            /// </summary>
            Cloudy,
            /// <summary>
            /// Showers
            /// </summary>
            Showers,
            /// <summary>
            /// Scattered Thunderstorms
            /// </summary>
            ScatteredTStorms,
            /// <summary>
            /// Thunderstorms
            /// </summary>
            TStrorms,
            /// <summary>
            /// Severe Thunderstorms
            /// </summary>
            SevereTStorms,
            /// <summary>
            /// Snow
            /// </summary>
            Snow,
            /// <summary>
            /// Rain/Snow Mix
            /// </summary>
            RainSnowMix,
            /// <summary>
            /// Hail
            /// </summary>
            Hail,
            /// <summary>
            /// Freezing Rain
            /// </summary>
            FreezingRain,
            /// <summary>
            /// Tropical Storm
            /// </summary>
            TropicalStorm,
            /// <summary>
            /// Hurricane
            /// </summary>
            Hurricane,
            /// <summary>
            /// Tornado
            /// </summary>
            Tornado,
            /// <summary>
            /// Windy
            /// </summary>
            Windy,
            /// <summary>
            /// Fog
            /// </summary>
            Foggy,
            /// <summary>
            /// Dusty
            /// </summary>
            Dust,
            /// <summary>
            /// Hazy
            /// </summary>
            Haze,
            /// <summary>
            /// Hot
            /// </summary>
            Hot,
            /// <summary>
            /// Smoke
            /// </summary>
            Smoke
        }
        /// <summary>
        /// Weather Data
        /// </summary>
        public struct weather
        {
            /// <summary>
            /// Current Conditions
            /// </summary>
            public weatherConditions conditions;
            /// <summary>
            /// Date Added
            /// </summary>
            public DateTime date;
            /// <summary>
            /// Dew Point
            /// </summary>
            public float dewPoint;
            /// <summary>
            /// Feels Like
            /// </summary>
            public float feelsLike;
            /// <summary>
            /// High Temp
            /// </summary>
            public float highTemp;
            /// <summary>
            /// Low Temp
            /// </summary>
            public float lowTemp;
            /// <summary>
            /// Humidity
            /// </summary>
            public int humidity;
            /// <summary>
            /// Temp
            /// </summary>
            public float temp;
            /// <summary>
            /// UVIndex
            /// </summary>
            public int UVIndex;
            /// <summary>
            /// Wind Direction
            /// </summary>
            public direction windDirection;
            /// <summary>
            /// Wind Speed
            /// </summary>
            public float windSpeed;
            /// <summary>
            /// % chance of precipitation
            /// </summary>
            public int precipitationPercent;
            /// <summary>
            /// Zip
            /// </summary>
            public string location;
        }
        /// <summary>
        /// Return weather for the given zip.  Use DateTime.MinValue for all days or specify the day
        /// </summary>
        /// <param name="location"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public weather readWeather(string location,DateTime day)
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM Weather WHERE Code='" + location + "'";
            if (day!=DateTime.MinValue)
                cmd.CommandText+=" AND Date='"+day.ToString()+"'";
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read() == false)
                throw new ArgumentException();
            weather w = new weather();
            {
                w.conditions = (weatherConditions)Enum.Parse(typeof(weatherConditions), reader.GetString(reader.GetOrdinal("Conditions")));
                w.dewPoint = reader.GetFloat(reader.GetOrdinal("dewPoint"));
                w.highTemp = reader.GetFloat(reader.GetOrdinal("highTemp"));
                w.lowTemp = reader.GetFloat(reader.GetOrdinal("lowTemp"));
                w.temp= reader.GetFloat(reader.GetOrdinal("Temp"));
                w.dewPoint = reader.GetFloat(reader.GetOrdinal("dewPoint"));
                w.humidity = reader.GetInt32(reader.GetOrdinal("Humidity"));
                w.UVIndex = reader.GetInt32(reader.GetOrdinal("UVIndex"));
                w.windDirection = (direction)Enum.Parse(typeof(direction), reader.GetString(reader.GetOrdinal("windDirection")));
                w.windSpeed = reader.GetFloat(reader.GetOrdinal("windSpeed"));
                w.precipitationPercent=reader.GetInt32(reader.GetOrdinal("precip"));
                w.location = location;
                w.feelsLike = reader.GetFloat(reader.GetOrdinal("feelsLike"));
                w.date =DateTime.Parse(reader["Date"].ToString());
            }
            reader.Close();
            return w;
        }
        SQLiteConnection con;
        /// <summary>
        /// Provides access to the weather database
        /// </summary>
        public Weather()
        {
            con = new SQLiteConnection(@"Data Source=" + Path.Combine(Application.StartupPath, "Data", "OMData") + ";Version=3;Pooling=True;Max Pool Size=6;");
            con.Open();
        }
        /// <summary>
        /// Write the given weather infomation to the database
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public bool writeWeather(weather w)
        {
            SQLiteCommand cmd = con.CreateCommand();
            StringBuilder query = new StringBuilder("DELETE FROM Weather WHERE Code='");
            {
                query.Append(General.escape(w.location));
                query.Append("' AND Date='");
                query.Append(w.date);
                query.Append("';INSERT INTO Weather (Conditions, Date, dewPoint, feelsLike, highTemp, lowTemp, Temp, Humidity, UVIndex, windDirection, windSpeed, precip, Code)VALUES('");
                query.Append(w.conditions);
                query.Append("','");
                query.Append(w.date);
                query.Append("','");
                query.Append(w.dewPoint);
                query.Append("','");
                query.Append(w.feelsLike);
                query.Append("','");
                query.Append(w.highTemp);
                query.Append("','");
                query.Append(w.lowTemp);
                query.Append("','");
                query.Append(w.temp);
                query.Append("','");
                query.Append(w.humidity);
                query.Append("','");
                query.Append(w.UVIndex);
                query.Append("','");
                query.Append(w.windDirection);
                query.Append("','");
                query.Append(w.windSpeed);
                query.Append("','");
                query.Append(w.precipitationPercent);
                query.Append("','");
                query.Append(General.escape(w.location));
                query.Append("')");
                cmd.CommandText = query.ToString();
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        /// <summary>
        /// Removes all weather data from before today
        /// </summary>
        /// <returns></returns>
        public static void PurgeOld()
        {
            using(Weather w = new Weather())
                w.purgeOld();
        }
        /// <summary>
        /// Removes all weather data from before today
        /// </summary>
        /// <returns></returns>
        public bool purgeOld()
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Weather WHERE Date<'" + DateTime.Today+"'";
            cmd.ExecuteNonQuery();
            return true;
        }
        /// <summary>
        /// Closes the connection and unloads the class
        /// </summary>
        public void Dispose()
        {
            if (con != null)
            {
                con.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
