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

namespace OpenMobile.Plugin
{
    /// <summary>
    /// Media players supporting direct volume control should implement this interface
    /// </summary>
    public interface IPlayerVolumeControl
    {
        /// <summary>
        /// Set the volume (range 0-100)
        /// <para>return false if not supported, otherwise true</para>
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="percent"></param>
        /// <returns>return false if not supported, otherwise true</returns>
        bool setVolume(Zone zone, int percent);
        /// <summary>
        /// Get the players volume
        /// <para>return false if not supported, otherwise true</para>
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>return false if not supported, otherwise true</returns>
        int getVolume(Zone zone);
    }
}
