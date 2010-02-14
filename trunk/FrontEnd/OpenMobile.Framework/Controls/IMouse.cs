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

// This interface is added by Borte

namespace OpenMobile.Controls
{
    /// <summary>
    /// Controls that support mouse interaction (Added by Borte)
    /// </summary>
    public interface IMouse
    {
        /// <summary>
        /// Mouse moved over the control
        /// </summary>
        void MouseMove(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale);

        /// <summary>
        /// MouseDown event for this control
        /// </summary>
        void MouseDown(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale);

        /// <summary>
        /// MouseUp event for this control
        /// </summary>
        void MouseUp(int screen, System.Windows.Forms.MouseEventArgs e, float WidthScale, float HeightScale);
    }
}
