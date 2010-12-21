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
namespace OpenMobile.Controls
{
    /// <summary>
    /// Used for controls which react specially to keyboard input
    /// </summary>
    public interface IKeyboard
    {
        /// <summary>
        /// Occurs when keyboard focus is set to the control
        /// </summary>
        /// <param name="screen"></param>
        void KeyboardEnter(int screen);
        /// <summary>
        /// Occurs when keyboard focus leaves the control
        /// </summary>
        /// <param name="screen"></param>
        void KeyboardExit(int screen);
    }
}
