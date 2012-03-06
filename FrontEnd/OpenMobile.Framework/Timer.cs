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
using OpenMobile.Controls;

namespace OpenMobile
{
    /// <summary>
    /// A OpenMobile version of the system.Timer object
    /// </summary>
    public class Timer : System.Timers.Timer
    {
        /// <summary>
        /// Initializes a new openmobile timer
        /// </summary>
        /// <param name="interval"></param>
        public Timer(double interval)
            : base(interval)
        {
        }

        /// <summary>
        /// A general purpose tag object
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// The screen assosiated with this timer (must be set manually)
        /// </summary>
        public int Screen { get; set; }

        /// <summary>
        /// The panel assosiated with this timer (must be set manually)
        /// </summary>
        public OMPanel Panel { get; set; }
    }
}
