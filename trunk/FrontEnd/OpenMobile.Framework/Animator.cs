using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Threading;
using System.Threading;

namespace OpenMobile
{
    public static class Animator
    {
        public static void Move(OMControl control, int X,int Y)
        {
            int xsteps = System.Math.Abs((control.Left - X) / 50);
            int ysteps = System.Math.Abs((control.Top - Y) / 50);
            Move(control, X, Y, (xsteps > ysteps) ? xsteps : ysteps, 50);
        }
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
