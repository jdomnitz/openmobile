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
