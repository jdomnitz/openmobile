using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Graphics;

namespace OMBricks
{
    public class Paddle : OMImage, IMouse, IHighlightable
    {

        public delegate void UnPauseEventHandler(int screen);
        public event UnPauseEventHandler UnPaused;

        public Ball b;
        int bInitLeft;
        public int InitialWidth = 175;
        public List<Ball> Balls;

        private imageItem mainPaddleImage;
        
        public void Reset()
        {
            initLeft = (OM.Host.ClientArea[0].Width / 2) - 75;
            Left = initLeft;
            Width = InitialWidth;
            Top = OM.Host.ClientArea[0].Bottom - 50;
            Height = 40;
        }

        public Paddle()
        {
            Left = (OM.Host.ClientArea[0].Width / 2) - 75;
            Width = InitialWidth;
            Top = OM.Host.ClientArea[0].Bottom - 50;
            Height = 40;
            mainPaddleImage = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Paddle2");
            Image = mainPaddleImage;
        }

        int moveX;
        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            if (movingPaddle)
            {
                moveX = e.X - initX;
                //Left += moveX;
                Left = initLeft + moveX;
                if (!Balls[0].Moving)
                {
                    Balls[0].Left = bInitLeft + moveX;
                    Balls[0]._Left = Balls[0].Left;
                }
            }
        }

        int initX;
        int initLeft;
        bool movingPaddle = false;
        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation)
        {
            if (UnPaused != null)
                UnPaused(screen);
            movingPaddle = true;
            initX = e.X;
            initLeft = Left;
            bInitLeft = Balls[0].Left;
        }

        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation, Point TotalDistance)
        {
            movingPaddle = false;
        }

        public void AddShootingOverlayImage()
        {
            Image = new imageItem(Image.Copy().image.AddImageOverlay(0, 0, OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Paddle_OL").image));
        }

        public void RemoveShootingOverlayImage()
        {
            //remove the previously added image overlay
            Image = mainPaddleImage;
        }

        public bool ShrinkPaddle()
        {
            bool Result = true;

            return Result;
        }


        public bool GrowPaddle()
        {
            bool Result = true;

            return Result;
        }

    }
}
