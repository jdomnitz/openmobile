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
using System.Text;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Helperfunctions for object array (params or args)
    /// </summary>
    public static class Params
    {
        /// <summary>
        /// Is param valid?
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool IsParamsValid(object[] param)
        {
            return param != null;
        }
        /// <summary>
        /// Is param valid?
        /// </summary>
        /// <param name="param"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static bool IsParamsValid(object[] param, int minCount)
        {
            return param != null && param.Length >= minCount;
        }

        /// <summary>
        /// Gets a param as specified type, if param is not present default data for data type is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="paramCount"></param>
        /// <returns></returns>
        public static T GetParam<T>(object[] param, int paramCount)
        {
            return GetParam<T>(param, paramCount, default(T));
        }

        /// <summary>
        /// Gets a param as specified type, if param is not present default data is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="paramNumber"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetParam<T>(object[] param, int paramNumber, T defaultValue)
        {
            if (IsParamsValid(param, paramNumber))
            {
                try
                {
                    return (T)Convert.ChangeType(param[paramNumber], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Extracts the screen number from a string like this "Screen0" returns 0 if it fails
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetScreenFromString(string s)
        {
            int screen = 0;
            int.TryParse(s.Substring(6), out screen);
            return screen;
        }
    }
}
