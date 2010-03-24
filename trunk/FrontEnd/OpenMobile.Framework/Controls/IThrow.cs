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

using System.Drawing;
namespace OpenMobile.Controls
{
    /// <summary>
    /// Controls that support being "thrown" (meaning receiving mouseover events even when the mouse is not over the control) (Added by Borte)
    /// NOTE: All distance values will be scaled so they are uniform at all resolutions
    /// </summary>
    public interface IThrow
    {
        /// <summary>
        /// Fired when a control is "thrown" (a control will continue to receive mousemove events even when the mouse is outside the control)
        /// </summary>
        /// <param name="screen">Screen the event occured on</param>
        /// <param name="TotalDistance">Distance mouse has been moved from throw start location</param>
        /// <param name="RelativeDistance">Relative distance mouse has been moved from throw start location</param>
        void MouseThrow(int screen, System.Drawing.Point TotalDistance, System.Drawing.Point RelativeDistance);

        /// <summary>
        /// Throw is started
        /// </summary>
        /// <param name="screen">Screen the event occured on</param>
        /// <param name="StartLocation">The point that was clicked</param>
        /// <param name="Cancel">If true cancels the throw operation</param>
        /// <param name="scaleFactors"></param>
        void MouseThrowStart(int screen, System.Drawing.Point StartLocation,PointF scaleFactors, ref bool Cancel);

        /// <summary>
        /// Throw is ended
        /// </summary>
        void MouseThrowEnd(int screen, System.Drawing.Point EndLocation);
    }
}
