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

namespace OpenMobile.Data
{
    /// <summary>
    /// Vehicle
    /// </summary>
    public static class Vehicle
    {
        /// <summary>
        /// The vehicle identification number
        /// </summary>
        public static string VIN;
        /// <summary>
        /// The current driver (used for multi-user systems)
        /// </summary>
        public static string Driver;
        /// <summary>
        /// The Fuel Type (INCOMPLETE)
        /// </summary>
        public static int FuelType;
        /// <summary>
        /// The current vehicle milage
        /// </summary>
        public static int Mileage;
        /// <summary>
        /// Loads Vehicle Information
        /// </summary>
        public static void LoadData()
        {
            //TODO
        }
        /// <summary>
        /// Stores Vehicle Infomation
        /// </summary>
        public static void SaveData()
        {
            //TODO
        }
    }
}

/* TASKS
To Do:
vehicle  read/write
phone    read/write

Done:
gas stations   read/write
weather        read/write
pluginsettings read/write
personal       read/write
messages       read/write
contacts       read/write
tasks          read/write
calendar       read/write
*/