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
using System;

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
                    return speed + "kph";
                else
                    return speed + "mph";
            else if (kph == true)
                return Calculation.convertSpeed(speed,speedTypes.kilometersPerHour,speedTypes.milesPerHour).ToString("0") + "mph";
            else
                return Calculation.convertSpeed(speed, speedTypes.milesPerHour, speedTypes.kilometersPerHour).ToString("0") + "kph";
        }
        /// <summary>
        /// Removes any localized phone number formatting from the string so that it can be dialed
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string removePhoneLocalization(string number)
        {
            string ret="";
            for(int i=0;i<number.Length;i++)
                switch (number[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        ret += number[i];
                        break;
                    case '+':
                        if (i == 0)
                            ret += '+';
                        break;
                }
            return ret;
        }
        /// <summary>
        /// Formats a phone number into the local phone number format
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string formatPhoneNumber(string number)
        {
            string ret = "";
            bool prefix = false;
            if (number[0] == '+')
            {
                number = number.Substring(1);
                prefix = true;
            }
            switch (System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName)
            {
                case "US":
                case "CA":
                    if (number.Length == 7)
                        return long.Parse(removePhoneLocalization(number)).ToString("###-####");
                    else if (number.Length == 10)
                        return long.Parse(removePhoneLocalization(number)).ToString("(###) ###-####");
                    else if ((number.Length==11)||(number.Length==13))
                        return long.Parse(removePhoneLocalization(number)).ToString("+### (###) ###-####");
                    break;
                case "MX":
                    if (number.Length==10)
                        if ((number.StartsWith("55"))||(number.StartsWith("33"))||(number.StartsWith("81")))
                            return long.Parse(removePhoneLocalization(number)).ToString("(##) ####-####");
                        else
                            return long.Parse(removePhoneLocalization(number)).ToString("(###) ###-####");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("+### (###) ###-####");
                case "GB":
                    if (number.Length==10)
                        return long.Parse(removePhoneLocalization(number)).ToString("(#####) #####");
                    else if (number.Length == 11)
                        return long.Parse(removePhoneLocalization(number)).ToString("(#####) ######");
                    break;
                case "ES":
                    if (number.Length==9)
                        return long.Parse(removePhoneLocalization(number)).ToString("## ### ## ##");
                    else if(number.Length==11)
                        return long.Parse(removePhoneLocalization(number)).ToString("+## ## ### ####");
                    break;
                case "PL":
                    if (number.Length >= 9)
                    {
                        ret= long.Parse(removePhoneLocalization(number)).ToString("## ## ### ## ##").Trim();
                        if (prefix == true)
                            return '+' + ret;
                        else
                            return ret;
                    }
                    break;
                case "NO":
                    if (number.StartsWith("4")||number.StartsWith("9"))
                        return long.Parse(removePhoneLocalization(number)).ToString("### ## ###");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("## ## ## ##");
                case "FR":
                    if (prefix==true)
                        return '+'+long.Parse(removePhoneLocalization(number)).ToString("### ### ### ###");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("0# ## ## ## ##");
                case "RU":
                    if (number.Length==5)
                        return long.Parse(removePhoneLocalization(number)).ToString("#-##-##");
                    else if(number.Length==6)
                        return long.Parse(removePhoneLocalization(number)).ToString("##-##-##");
                    else if(number.Length==7)
                        return long.Parse(removePhoneLocalization(number)).ToString("###-##-##");
                    else if(number.Length==10)
                        return long.Parse(removePhoneLocalization(number)).ToString("###.###-##-##");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("##.###.###-##-##");
                case "IN":
                    return long.Parse(removePhoneLocalization(number)).ToString("###-########");
                case "CN":
                    if (number.Length==11)
                        return long.Parse(removePhoneLocalization(number)).ToString("###-####-####");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("####-####");
                case "AU":
                    if (number.StartsWith("04"))
                        return long.Parse(removePhoneLocalization(number)).ToString("#### ### ###");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("(##) #### ####");
                case "ZA":
                case "CH":
                    if ((number.Length == 9)||(number.Length == 10))
                        return long.Parse(removePhoneLocalization(number)).ToString("### ### ####");
                    break;
            }
            if (prefix == true)
                return '+' + number;
            return number;
        }
        /// <summary>
        /// Returns the local language
        /// </summary>
        /// <returns></returns>
        public static string getLanguage()
        {
            string ret = System.Globalization.CultureInfo.CurrentCulture.NativeName;
            if (string.IsNullOrEmpty(ret))
                return null;
            return ret.Substring(0,ret.IndexOf(" ("));
        }
        /// <summary>
        /// Converts local time to UTC time
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public static DateTime localTimeToUTC(DateTime local)
        {
            return local.ToUniversalTime();
        }
        /// <summary>
        /// Converts UTC time to local time
        /// </summary>
        /// <param name="utc"></param>
        /// <returns></returns>
        public static DateTime UTCToLocalTime(DateTime utc)
        {
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
        }
        /// <summary>
        /// Returns true if the current locale uses a 12 hour clock
        /// </summary>
        public static bool TwelveHourClock
        {
            get
            {
                return System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortTimePattern.StartsWith("h");
            }
        }
    }
}
