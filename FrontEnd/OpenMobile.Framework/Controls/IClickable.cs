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
    /// Occurs when a user interacts with a control (like a mouse click)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screen"></param>
    public delegate void userInteraction(OMControl sender,int screen);
    /// <summary>
    /// Controls that support being clicked
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// Fires the OnClick Event
        /// </summary>
        void clickMe(int screen);
        /// <summary>
        /// Fires the OnDoubleClick Event
        /// </summary>
        void doubleClickMe(int screen);
        /// <summary>
        /// Fires the OnLongClick event
        /// </summary>
        void longClickMe(int screen);
    }
}
