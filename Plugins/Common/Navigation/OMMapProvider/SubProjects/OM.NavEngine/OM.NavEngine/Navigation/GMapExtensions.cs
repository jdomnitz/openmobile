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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotSpatial.Positioning;
using GMap.NET;

namespace OpenMobile.NavEngine
{
    public static class GMapExtensions
    {
        /// <summary>
        /// Converts the Position type to a PointLatLng type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static PointLatLng ToPointLatLng(this Position value)
        {
            return new PointLatLng(value.Latitude.DecimalDegrees, value.Longitude.DecimalDegrees);
        }

        ///// <summary>
        ///// Returns a latitude / longitude value as a string that can be navigated to by GMap
        ///// String example: "60.7993125915527, 10.6513748168945"
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static string ToGoogleLatLngString(this Position value)
        //{
        //    return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", value.Latitude.DecimalDegrees, value.Longitude.DecimalDegrees);
        //}

        /// <summary>
        /// Returns a latitude / longitude value as a string that can be navigated to by GMap
        /// String example: "60.7993125915527, 10.6513748168945"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToGoogleLatLngString(this PointLatLng value)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", value.Lat, value.Lng);
        }

        /// <summary>
        /// Converts the PointLatLng type to a Position type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Position ToPosition(this PointLatLng value)
        {
            return new Position(new Latitude(value.Lat), new Longitude(value.Lng));
        }
    }
}
