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

namespace OpenMobile.NavEngine
{
    /// <summary>
    /// The different modes of travel
    /// </summary>
    public enum NavigationTravelMode
    {
        /// <summary>
        /// Driving
        /// </summary>
        Driving,
        /// <summary>
        /// Bicycle
        /// </summary>
        Bicycling,
        /// <summary>
        /// Public transit routes
        /// </summary>
        Transit,
        /// <summary>
        /// Walking
        /// </summary>
        Walking
    }
}
