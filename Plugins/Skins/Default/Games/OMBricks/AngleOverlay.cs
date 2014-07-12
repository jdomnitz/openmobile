using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile.Graphics;

namespace OMBricks
{
    class AngleOverlay : OMButton, IMouse, IHighlightable
    {

        public List<Ball> Balls;
        public Paddle p;
        Point Middle;
        Point Right;
        double sideLength1; //  _
        double sideLength2; //  /,\
        double sideLength3;
        double largestAngle;

        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.Point TotalDistance, OpenMobile.Graphics.Point RelativeDistance)
        {
            //
        }

        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, OpenMobile.Graphics.Point StartLocation)
        {
            if (!Balls[0].Moving)
            {
                //calculate angle based on point of mouse
                Middle = new Point(p.Left + (p.Width / 2), p.Top + (p.Height / 2)); //middle of paddle
                Right = new Point(OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width, p.Top + (p.Height / 2));
                //d = sqrt((x2 - x1)^2 + (y2-y1)^2)
                sideLength1 = Math.Sqrt(Math.Pow((Right.X - Middle.X), 2) + Math.Pow((Right.Y - Middle.Y), 2)); //length from bottom right length to paddle
                sideLength2 = Math.Sqrt(Math.Pow((e.X - Middle.X), 2) + Math.Pow((e.Y - Middle.Y), 2)); //length from paddle to mouse point
                sideLength3 = Math.Sqrt(Math.Pow((Right.X - e.X), 2) + Math.Pow((Right.Y - e.Y), 2)); //length from mouse point to bottom right

                Point U = new Point() { X = e.X - Middle.X, Y = e.Y - Middle.Y };
                Point V = new Point() { X = Right.X - Middle.X, Y = Right.Y - Middle.Y };
                largestAngle = Math.Acos((U.X * V.X + U.Y * V.Y) / (Math.Sqrt(Math.Pow(U.X,2) + Math.Pow(U.Y,2)) * Math.Sqrt(Math.Pow(V.X,2) + Math.Pow(V.Y,2))));
                largestAngle = Math.Acos((U.X * V.X + U.Y * V.Y) / (sideLength2 * sideLength1));

                //int largestSide = 0;
                //if (sideLength1 > sideLength2 && sideLength1 > sideLength3)
                //    largestSide = 1;
                //else if (sideLength2 > sideLength1 && sideLength2 > sideLength3)
                //    largestSide = 2;
                //else
                //    largestSide = 3;
                //if (largestSide == 1)
                //{
                //    largestAngle = Math.Acos((Math.Pow(sideLength2, 2) - (Math.Pow(sideLength1, 2) + Math.Pow(sideLength3, 2))) / (2 * sideLength2 * sideLength3));
                //}
                //else if (largestSide == 2)
                //{
                //    largestAngle = Math.Acos((Math.Pow(sideLength1, 2) - (Math.Pow(sideLength2, 2) + Math.Pow(sideLength3, 2))) / (2 * sideLength1 * sideLength3));
                //}
                //else
                //{
                //    largestAngle = Math.Acos((Math.Pow(sideLength1, 2) - (Math.Pow(sideLength3, 2) + Math.Pow(sideLength2, 2))) / (2 * sideLength1 * sideLength2));
                //}
                largestAngle = largestAngle * (180 / Math.PI);

                //now we have the 3 lengths of the triangle, find the angle between sideLength1 and sideLength2
                //c^2 = b^2 + a^2 - 2baCos(C)
                //if s3 largest
                // Cos((s1^2 + s2^2 - s3^2) / (2 * s1 * s2)) = Largest Angle
                //largestAngle = (int)(Math.Acos((Math.Pow(sideLength1, 2) + Math.Pow(sideLength2, 2) - Math.Pow(sideLength3, 2)) / (2 * sideLength1 * sideLength2)));

                //if (largestSide == 3)
                //{
                    Balls[0].Angle = (int)largestAngle;
                    Balls[0].Moving = true;
                    Visible = false;
                    //return;
                //}

            }
        }

        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.Point TotalDistance)
        {
            //
        }
    }
}
