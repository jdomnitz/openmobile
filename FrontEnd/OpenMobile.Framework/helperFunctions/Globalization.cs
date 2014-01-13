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
                    return String.Format("{0:0.#}°C", temp); //temp.ToString("0.0") + " °C";
                else
                    return String.Format("{0:0.#}°F", temp); //temp.ToString("0.0") + " °F";
            else if (celsius == true)
                return String.Format("{0:0.#}°F", Calculation.CtoF(temp)); //Calculation.CtoF(temp).ToString("0.0") + " °F";
            else
                return String.Format("{0:0.#}°C", Calculation.FtoC(temp)); //Calculation.FtoC(temp).ToString("0.0") + " °C";
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
                return Calculation.convertDistance(distance, distanceTypes.kilometers, distanceTypes.miles) + " mi.";
            else
                return Calculation.convertDistance(distance, distanceTypes.miles, distanceTypes.kilometers) + " km.";
        }
        /// <summary>
        /// Converts to the local unit of Speed Measurement
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="kph"></param>
        /// <returns></returns>
        public static string convertSpeedToLocal(double speed, bool kph, int decimals = 1)
        {
            if (kph == isMetric())
                if (kph == true)
                    return speed + "Km/h";
                else
                    return speed + "mph";
            else if (kph == true)
                return System.Math.Round(Calculation.convertSpeed(speed, speedTypes.kilometersPerHour, speedTypes.milesPerHour), decimals).ToString("0") + "mph";
            else
                return System.Math.Round(Calculation.convertSpeed(speed, speedTypes.milesPerHour, speedTypes.kilometersPerHour), decimals).ToString("0") + "Km/h";
        }
        /// <summary>
        /// Removes any localized phone number formatting from the string so that it can be dialed
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string removePhoneLocalization(string number)
        {
            string ret = String.Empty;
            for (int i = 0; i < number.Length; i++)
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
            string ret = String.Empty;
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
                    else if ((number.Length == 11) || (number.Length == 13))
                        return long.Parse(removePhoneLocalization(number)).ToString("+### (###) ###-####");
                    break;
                case "MX":
                    if (number.Length == 10)
                        if ((number.StartsWith("55")) || (number.StartsWith("33")) || (number.StartsWith("81")))
                            return long.Parse(removePhoneLocalization(number)).ToString("(##) ####-####");
                        else
                            return long.Parse(removePhoneLocalization(number)).ToString("(###) ###-####");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("+### (###) ###-####");
                case "GB":
                    if (number.Length == 10)
                        return long.Parse(removePhoneLocalization(number)).ToString("(#####) #####");
                    else if (number.Length == 11)
                        return long.Parse(removePhoneLocalization(number)).ToString("(#####) ######");
                    break;
                case "ES":
                    if (number.Length == 9)
                        return long.Parse(removePhoneLocalization(number)).ToString("## ### ## ##");
                    else if (number.Length == 11)
                        return long.Parse(removePhoneLocalization(number)).ToString("+## ## ### ####");
                    break;
                case "PL":
                    if (number.Length >= 9)
                    {
                        ret = long.Parse(removePhoneLocalization(number)).ToString("## ## ### ## ##").Trim();
                        if (prefix == true)
                            return '+' + ret;
                        else
                            return ret;
                    }
                    break;
                case "NO":
                    if (number.StartsWith("4") || number.StartsWith("9"))
                        return long.Parse(removePhoneLocalization(number)).ToString("### ## ###");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("## ## ## ##");
                case "FR":
                    if (prefix == true)
                        return '+' + long.Parse(removePhoneLocalization(number)).ToString("### ### ### ###");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("0# ## ## ## ##");
                case "RU":
                    if (number.Length == 5)
                        return long.Parse(removePhoneLocalization(number)).ToString("#-##-##");
                    else if (number.Length == 6)
                        return long.Parse(removePhoneLocalization(number)).ToString("##-##-##");
                    else if (number.Length == 7)
                        return long.Parse(removePhoneLocalization(number)).ToString("###-##-##");
                    else if (number.Length == 10)
                        return long.Parse(removePhoneLocalization(number)).ToString("###.###-##-##");
                    else
                        return long.Parse(removePhoneLocalization(number)).ToString("##.###.###-##-##");
                case "IN":
                    return long.Parse(removePhoneLocalization(number)).ToString("###-########");
                case "CN":
                    if (number.Length == 11)
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
                    if ((number.Length == 9) || (number.Length == 10))
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
            return ret.Substring(0, ret.IndexOf(" ("));
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

        /// <summary>
        /// Converts a iso country code (2 characters) to a full country name
        /// </summary>
        /// <param name="isoCountryCode">iso country code (2 characters)</param>
        /// <returns></returns>
        public static string CountryISOCodeToFullName(string isoCountryCode)
        {
            switch (isoCountryCode.ToUpper())
            {
                case "AD":
                    return "Andorra";
                case "AE":
                    return "United Arab Emirates";
                case "AF":
                    return "Afghanistan";
                case "AG":
                    return "Antigua and Barbuda";
                case "AI":
                    return "Anguilla";
                case "AL":
                    return "Albania";
                case "AM":
                    return "Armenia";
                //case "AN":
                //    return "";
                case "AO":
                    return "Angola";
                case "AQ":
                    return "Antarctica";
                case "AR":
                    return "Argentina";
                case "AS":
                    return "American Samoa";
                case "AT":
                    return "Austria";
                case "AU":
                    return "Australia";
                case "AW":
                    return "Aruba";
                case "AX":
                    return "Åland";
                case "AZ":
                    return "Azerbaijan";
                case "BA":
                    return "Bosnia and Herzegovina";
                case "BB":
                    return "Barbados";
                case "BD":
                    return "Bangladesh";
                case "BE":
                    return "Belgium";
                case "BF":
                    return "Burkina Faso";
                case "BG":
                    return "Bulgaria";
                case "BH":
                    return "Bahrain";
                case "BI":
                    return "Burundi";
                case "BJ":
                    return "Benin";
                case "BL":
                    return "Saint Barthélemy";
                case "BM":
                    return "Bermuda";
                case "BN":
                    return "Brunei";
                case "BO":
                    return "Bolivia";
                case "BQ":
                    return "Bonaire";
                case "BR":
                    return "Brazil";
                case "BS":
                    return "Bahamas";
                case "BT":
                    return "Bhutan";
                case "BV":
                    return "Bouvet Island";
                case "BW":
                    return "Botswana";
                case "BY":
                    return "Belarus";
                case "BZ":
                    return "Belize";
                case "CA":
                    return "Canada";
                case "CC":
                    return "Cocos [Keeling] Islands";
                case "CD":
                    return "Congo";
                case "CF":
                    return "Central African Republic";
                case "CG":
                    return "Republic of the Congo";
                case "CH":
                    return "Switzerland";
                case "CI":
                    return "Ivory Coast";
                case "CK":
                    return "Cook Islands";
                case "CL":
                    return "Chile";
                case "CM":
                    return "Cameroon";
                case "CN":
                    return "China";
                case "CO":
                    return "Colombia";
                case "CR":
                    return "Costa Rica";
                //case "CS":
                //    return "";
                case "CU":
                    return "Cuba";
                case "CV":
                    return "Cape Verde";
                case "CW":
                    return "Curacao";
                case "CX":
                    return "Christmas Island";
                case "CY":
                    return "Cyprus";
                case "CZ":
                    return "Czechia";
                case "DE":
                    return "Germany";
                case "DJ":
                    return "Djibouti";
                case "DK":
                    return "Denmark";
                case "DM":
                    return "Dominica";
                case "DO":
                    return "Dominican Republic";
                case "DZ":
                    return "Algeria";
                case "EC":
                    return "Ecuador";
                case "EE":
                    return "Estonia";
                case "EG":
                    return "Egypt";
                case "EH":
                    return "Western Sahara";
                case "ER":
                    return "Eritrea";
                case "ES":
                    return "Spain";
                case "ET":
                    return "Ethiopia";
                case "FI":
                    return "Finland";
                case "FJ":
                    return "Fiji";
                case "FK":
                    return "Falkland Islands";
                case "FM":
                    return "Micronesia";
                case "FO":
                    return "Faroe Islands";
                case "FR":
                    return "France";
                case "GA":
                    return "Gabon";
                case "GB":
                    return "United Kingdom";
                case "GD":
                    return "Grenada";
                case "GE":
                    return "Georgia";
                case "GF":
                    return "French Guiana";
                case "GG":
                    return "Guernsey";
                case "GH":
                    return "Ghana";
                case "GI":
                    return "Gibraltar";
                case "GL":
                    return "Greenland";
                case "GM":
                    return "Gambia";
                case "GN":
                    return "Guinea";
                case "GP":
                    return "Guadeloupe";
                case "GQ":
                    return "Equatorial Guinea";
                case "GR":
                    return "Greece";
                case "GS":
                    return "South Georgia and the South Sandwich Islands";
                case "GT":
                    return "Guatemala";
                case "GU":
                    return "Guam";
                case "GW":
                    return "Guinea-Bissau";
                case "GY":
                    return "Guyana";
                case "HK":
                    return "Hong Kong";
                case "HM":
                    return "Heard Island and McDonald Islands";
                case "HN":
                    return "Honduras";
                case "HR":
                    return "Croatia";
                case "HT":
                    return "Haiti";
                case "HU":
                    return "Hungary";
                case "ID":
                    return "Indonesia";
                case "IE":
                    return "Ireland";
                case "IL":
                    return "Israel";
                case "IM":
                    return "Isle of Man";
                case "IN":
                    return "India";
                case "IO":
                    return "British Indian Ocean Territory";
                case "IQ":
                    return "Iraq";
                case "IR":
                    return "Iran";
                case "IS":
                    return "Iceland";
                case "IT":
                    return "Italy";
                case "JE":
                    return "Jersey";
                case "JM":
                    return "Jamaica";
                case "JO":
                    return "Jordan";
                case "JP":
                    return "Japan";
                case "KE":
                    return "Kenya";
                case "KG":
                    return "Kyrgyzstan";
                case "KH":
                    return "Cambodia";
                case "KI":
                    return "Kiribati";
                case "KM":
                    return "Comoros";
                case "KN":
                    return "Saint Kitts and Nevis";
                case "KP":
                    return "North Korea";
                case "KR":
                    return "South Korea";
                case "KW":
                    return "Kuwait";
                case "KY":
                    return "Cayman Islands";
                case "KZ":
                    return "Kazakhstan";
                case "LA":
                    return "Laos";
                case "LB":
                    return "Lebanon";
                case "LC":
                    return "Saint Lucia";
                case "LI":
                    return "Liechtenstein";
                case "LK":
                    return "Sri Lanka";
                case "LR":
                    return "Liberia";
                case "LS":
                    return "Lesotho";
                case "LT":
                    return "Lithuania";
                case "LU":
                    return "Luxembourg";
                case "LV":
                    return "Latvia";
                case "LY":
                    return "Libya";
                case "MA":
                    return "Morocco";
                case "MC":
                    return "Monaco";
                case "MD":
                    return "Moldova";
                case "ME":
                    return "Montenegro";
                case "MF":
                    return "Saint Martin";
                case "MG":
                    return "Madagascar";
                case "MH":
                    return "Marshall Islands";
                case "MK":
                    return "Macedonia";
                case "ML":
                    return "Mali";
                case "MM":
                    return "Myanmar [Burma]";
                case "MN":
                    return "Mongolia";
                case "MO":
                    return "Macao";
                case "MP":
                    return "Northern Mariana Islands";
                case "MQ":
                    return "Martinique";
                case "MR":
                    return "Mauritania";
                case "MS":
                    return "Montserrat";
                case "MT":
                    return "Malta";
                case "MU":
                    return "Mauritius";
                case "MV":
                    return "Maldives";
                case "MW":
                    return "Malawi";
                case "MX":
                    return "Mexico";
                case "MY":
                    return "Malaysia";
                case "MZ":
                    return "Mozambique";
                case "NA":
                    return "Namibia";
                case "NC":
                    return "New Caledonia";
                case "NE":
                    return "Niger";
                case "NF":
                    return "Norfolk Island";
                case "NG":
                    return "Nigeria";
                case "NI":
                    return "Nicaragua";
                case "NL":
                    return "Netherlands";
                case "NO":
                    return "Norway";
                case "NP":
                    return "Nepal";
                case "NR":
                    return "Nauru";
                case "NU":
                    return "Niue";
                case "NZ":
                    return "New Zealand";
                case "OM":
                    return "Oman";
                case "PA":
                    return "Panama";
                case "PE":
                    return "Peru";
                case "PF":
                    return "French Polynesia";
                case "PG":
                    return "Papua New Guinea";
                case "PH":
                    return "Philippines";
                case "PK":
                    return "Pakistan";
                case "PL":
                    return "Poland";
                case "PM":
                    return "Saint Pierre and Miquelon";
                case "PN":
                    return "Pitcairn Islands";
                case "PR":
                    return "Puerto Rico";
                case "PS":
                    return "Palestine";
                case "PT":
                    return "Portugal";
                case "PW":
                    return "Palau";
                case "PY":
                    return "Paraguay";
                case "QA":
                    return "Qatar";
                case "RE":
                    return "Réunion";
                case "RO":
                    return "Romania";
                case "RS":
                    return "Serbia";
                case "RU":
                    return "Russia";
                case "RW":
                    return "Rwanda";
                case "SA":
                    return "Saudi Arabia";
                case "SB":
                    return "Solomon Islands";
                case "SC":
                    return "Seychelles";
                case "SD":
                    return "Sudan";
                case "SE":
                    return "Sweden";
                case "SG":
                    return "Singapore";
                case "SH":
                    return "Saint Helena";
                case "SI":
                    return "Slovenia";
                case "SJ":
                    return "Svalbard and Jan Mayen";
                case "SK":
                    return "Slovakia";
                case "SL":
                    return "Sierra Leone";
                case "SM":
                    return "San Marino";
                case "SN":
                    return "Senegal";
                case "SO":
                    return "Somalia";
                case "SR":
                    return "Suriname";
                case "SS":
                    return "South Sudan";
                case "ST":
                    return "São Tomé and Príncipe";
                case "SV":
                    return "El Salvador";
                case "SX":
                    return "Sint Maarten";
                case "SY":
                    return "Syria";
                case "SZ":
                    return "Swaziland";
                case "TC":
                    return "Turks and Caicos Islands";
                case "TD":
                    return "Chad";
                case "TF":
                    return "French Southern Territories";
                case "TG":
                    return "Togo";
                case "TH":
                    return "Thailand";
                case "TJ":
                    return "Tajikistan";
                case "TK":
                    return "Tokelau";
                case "TL":
                    return "East Timor";
                case "TM":
                    return "Turkmenistan";
                case "TN":
                    return "Tunisia";
                case "TO":
                    return "Tonga";
                case "TR":
                    return "Turkey";
                case "TT":
                    return "Trinidad and Tobago";
                case "TV":
                    return "Tuvalu";
                case "TW":
                    return "Taiwan";
                case "TZ":
                    return "Tanzania";
                case "UA":
                    return "Ukraine";
                case "UG":
                    return "Uganda";
                case "UM":
                    return "U.S. Minor Outlying Islands";
                case "US":
                    return "United States";
                case "UY":
                    return "Uruguay";
                case "UZ":
                    return "Uzbekistan";
                case "VA":
                    return "Vatican City";
                case "VC":
                    return "Saint Vincent and the Grenadines";
                case "VE":
                    return "Venezuela";
                case "VG":
                    return "British Virgin Islands";
                case "VI":
                    return "U.S. Virgin Islands";
                case "VN":
                    return "Vietnam";
                case "VU":
                    return "Vanuatu";
                case "WF":
                    return "Wallis and Futuna";
                case "WS":
                    return "Samoa";
                case "XK":
                    return "Kosovo";
                case "YE":
                    return "Yemen";
                case "YT":
                    return "Mayotte";
                case "ZA":
                    return "South Africa";
                case "ZM":
                    return "Zambia";
                case "ZW":
                    return "Zimbabwe";
                default:
                    return "";
            }
        }


    }
}
