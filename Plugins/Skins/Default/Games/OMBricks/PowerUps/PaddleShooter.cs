using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using System.Diagnostics;

namespace OMBricks.PowerUps
{
    public class PaddleShooter
    {

        public int ShotsToFire = 4;
        private Stopwatch sw;
        
        public PaddleShooter()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public bool FireShot()
        {
            bool Result = false;
            if (sw.ElapsedMilliseconds > 1500)
            {
                sw.Reset();
                ShotsToFire--;
                Result = true;
            }
            sw.Start();
            return Result;
        }

    }

    public class PaddleShots : OMImage
    {

        public enum ShotSide { Left, LeftCenter, Right, RightCenter }

        private int _Angle;
        private Single PixelsToMove = 1.0F;
        private Single _Speed = 4.0F;
        private int _MaxHeight = 25;
        private bool shotHit = false;

        public PaddleShots(Paddle p, List<Bricks> _Bricks, int Angle, ShotSide ShootingSide)
        {
            Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Laser");
            _Angle = Angle;
            width = 5;
            if (ShootingSide == ShotSide.Left)
                Left = p.Left + 19;
            else if (ShootingSide == ShotSide.LeftCenter)
                Left = p.Left + (p.Width / 3);
            else if (ShootingSide == ShotSide.Right)
                Left = p.Left + p.Width - Width - 19;
            else
                Left = p.Left + ((p.Width / 3) * 2);
            Top = p.Top;
            Height = 0;
        }

        public bool Move(List<Bricks> _Bricks)
        {
            bool Result = true;
            Single newPixelsToMove = PixelsToMove * _Speed;
            double angle = Math.PI * _Angle / 180;
            double test = Math.Sin(angle);
            double test2 = Math.Cos(angle);
            var RelY = (newPixelsToMove * test) * -1;
            var RelX = newPixelsToMove * test2;
            Result = Collision((int)Math.Round(RelX), (int)Math.Round(RelY), _Bricks);
            return Result;
        }

        private bool Collision(int newX, int newY, List<Bricks> _Bricks)
        {
            bool Result = true;

            if (Height < _MaxHeight)
            {
                Height -= newY;
                if (Height > _MaxHeight)
                    Height = _MaxHeight;
                Top += newY;
            }
            else if (!shotHit)
            {
                Left += newX;
                Top += newY;
            }

            //test shot against outer client area
            if (Top + Height < OM.Host.ClientArea[0].Top || (shotHit && Height == 0))
                Result = false;
            else
            {
                //test brick collisions...
                for (int i = 0; i < _Bricks.Count; i++)
                {
                    if ((_Bricks[i].Visible) && (!_Bricks[i].isDestroying) && (!_Bricks[i].IsHit) && (_Bricks[i].Breakable))
                    {
                        if (Left + (Width / 2) > _Bricks[i].Left && Left + (Width / 2) < _Bricks[i].Left + _Bricks[i].Width && Top + (Height / 2) > _Bricks[i].Top && Top + (Height / 2) < _Bricks[i].Top + _Bricks[i].Height)
                        {
                            _Bricks[i].IsHit = true;
                            shotHit = true;
                        }
                    }
                    else if (!_Bricks[i].Breakable || shotHit)
                    {
                        //de-extend shot into this brick and when we can't anymore, return false
                        shotHit = true;
                        Height += newY;
                        if (Height < 0)
                            Height = 0;
                    }
                }
            }
            return Result;
        }

    }

}
