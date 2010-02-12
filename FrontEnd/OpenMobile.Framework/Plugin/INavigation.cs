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
using System.Drawing;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// Navigation Interface
    /// </summary>
    public interface INavigation:IBasePlugin
    {
        //***INCOMPLETE ROUGH DRAFT***
        /// <summary>
        /// Signals a navigation event has occured
        /// </summary>
        event NavigationEvent OnNavigationEvent;

        /// <summary>
        /// The current GPS position
        /// </summary>
        Point Position { get; }
        /// <summary>
        /// Returns the closest address
        /// </summary>
        string Location { get; }
        /// <summary>
        /// Gets/Sets the destination
        /// </summary>
        string Destination { get; set; }
        /// <summary>
        /// Detours from the current route by the given distance
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        bool Detour(int distance);

    }
}
