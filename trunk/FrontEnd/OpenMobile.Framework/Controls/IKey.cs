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
using OpenMobile.Graphics;
namespace OpenMobile.Controls
{
    /// <summary>
    /// Controls that support key handling
    /// </summary>
    public interface IKey
    {
        /// <summary>
        /// KeyDown when the control has focus but before the ui handles the keypress
        /// Return true if you don't want the ui to handle the keypress, otherwise return false
        /// </summary>
        bool KeyDown_BeforeUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors);

        /// <summary>
        /// KeyDown when the control has focus but after the ui has handled the keypress
        /// </summary>
        void KeyDown_AfterUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors);

        /// <summary>
        /// KeyUp when the control has focus but before the ui handles the keypress
        /// Return true if you don't want the ui to handle the keypress, otherwise return false
        /// </summary>
        bool KeyUp_BeforeUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors);

        /// <summary>
        /// KeyUp when the control has focus but after the ui has handled the keypress
        /// </summary>
        void KeyUp_AfterUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors);
    }
}
