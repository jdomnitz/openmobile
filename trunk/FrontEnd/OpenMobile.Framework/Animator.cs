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
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Threading;
using System.Threading;

namespace OpenMobile
{
    /// <summary>
    /// Animates controls
    /// </summary>
    public static class Animator
    {
        /// <summary>
        /// Slide the control to the given position
        /// </summary>
        /// <param name="control"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public static void Move(OMControl control, int X,int Y)
        {
            if (control == null)
                return;
            int xsteps = System.Math.Abs((control.Left - X) / 50);
            int ysteps = System.Math.Abs((control.Top - Y) / 50);
            Move(control, X, Y, (xsteps > ysteps) ? xsteps : ysteps, 50);
        }
        /// <summary>
        /// Slide the control to the given position
        /// </summary>
        /// <param name="control"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="steps"></param>
        /// <param name="stepSpeed"></param>
        public static void Move(OMControl control, int X,int Y, int steps, int stepSpeed)
        {
            SafeThread.Asynchronous(delegate()
            {
                if (steps > 0)
                {
                    int changeX = (X - control.Left) / steps;
                    int changeY = (Y - control.Top) / steps;
                    for (int i = 0; i < steps; i++)
                    {
                        control.Left += changeX;
                        control.Top += changeY;
                        Thread.Sleep(stepSpeed);
                    }
                }
                control.Left = X;
                control.Top = Y;
            }, null);
        }
    }
}
