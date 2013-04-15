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
using System.Threading;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Controls access to serial ports to stop plugins trying to gain access to the serial ports at the same time
    /// </summary>
    static public class SerialAccess
    {
        static private EventWaitHandle _SerialReleased = new EventWaitHandle(true, EventResetMode.AutoReset);

        /// <summary>
        /// Requests serial ports access. Returns true if request was successfull, returns false if request timed out (15 seconds)
        /// <para>Remember to release the request with ReleaseAccess<seealso cref="ReleaseAccess"/></para>
        /// </summary>
        /// <returns></returns>
        static public bool GetAccess()
        {
            // Wait for access
            if (!_SerialReleased.WaitOne(15000))
            {
                // Operation timed out, reset waitHandle
                _SerialReleased.Set();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Releases the serial port access request.
        /// </summary>
        static public void ReleaseAccess()
        {
            _SerialReleased.Set();
        }
    }
}
