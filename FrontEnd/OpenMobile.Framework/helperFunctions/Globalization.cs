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
using OpenMobile.Framework.Math;

namespace OpenMobile.Framework
{
    /// <summary>
    /// Globalization and Localization Functions
    /// </summary>
    public static class Globalization
    {
        /// <summary>
        /// Does this locale use the metric system
        /// </summary>
        /// <returns></returns>
        public static bool isMetric()
        {
            return System.Globalization.RegionInfo.CurrentRegion.IsMetric;
        }
        /// <summary>
        /// The local name of the current region
        /// </summary>
        /// <returns></returns>
        public static string LocaleName()
        {
            return System.Globalization.RegionInfo.CurrentRegion.NativeName;
        }
        /// <summary>
        /// Converts to the local unit of temperature measurement
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="celsius"></param>
        /// <returns></returns>
        public static string convertToLocalTemp(double temp, bool celsius)
        {
            if (celsius == isMetric())
                if (celsius == true)
                    return temp + " °C";
                else
                    return System.Math.Round(temp) + " °F";
            else if (celsius == true)
                return System.Math.Round(Calculation.CtoF(temp))+" °F";
            else
                return Calculation.FtoC(temp)+" °C";
        }
        /// <summary>
        /// Converts to the local unit of distance measurement
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="kilometers"></param>
        /// <returns></returns>
        public static string convertDistanceToLocal(double distance, bool kilometers)
        {
            if (kilometers == isMetric())
                if (kilometers == true)
                    return distance + " km.";
                else
                    return distance + " mi.";
            else if (kilometers == true)
                return Calculation.convertDistance(distance,distanceTypes.kilometers,distanceTypes.miles) + " mi.";
            else
                return Calculation.convertDistance(distance, distanceTypes.miles, distanceTypes.kilometers) + " km.";
        }
        /// <summary>
        /// Converts to the local unit of Speed Measurement
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="kph"></param>
        /// <returns></returns>
        public static string convertSpeedToLocal(double speed, bool kph)
        {
            if (kph == isMetric())
                if (kph == true)
                    return speed + " kph";
                else
                    return speed + " mph";
            else if (kph == true)
                return Calculation.convertSpeed(speed,speedTypes.kilometersPerHour,speedTypes.milesPerHour).ToString("0") + " mph";
            else
                return Calculation.convertSpeed(speed, speedTypes.milesPerHour, speedTypes.kilometersPerHour).ToString("0") + " kph";
        }
        /// <summary>
        /// Returns the local language
        /// </summary>
        /// <returns></returns>
        public static string getLanguage()
        {
            string ret = System.Globalization.CultureInfo.CurrentCulture.NativeName;
            if ((ret == null) || (ret == ""))
                return null;
            return ret.Remove(ret.IndexOf(" ("));
        }
    }
}
