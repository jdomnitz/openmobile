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
using System.Collections.Generic;

namespace OpenMobile.Data
{
    /// <summary>
    /// Collections of data from the database
    /// </summary>
    public static class Collections
    {
        //public static List<Bluetooth.phoneInfo> Phones= new List<Bluetooth.phoneInfo>();
        /// <summary>
        /// Available Messages
        /// </summary>
        public static List<Messages.message> Messages = new List<Messages.message>();
        /// <summary>
        /// Gas Stations
        /// </summary>
        public static List<GasStations.gasStation> gasStations = new List<GasStations.gasStation>();
        /// <summary>
        /// Calendar Events
        /// </summary>
        public static List<calendarEvent> calendarEvents = new List<calendarEvent>();
        /// <summary>
        /// Contacts
        /// </summary>
        public static List<contact> contacts = new List<contact>();
    }
}
