using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Framework;
using System.Diagnostics;

namespace OMBricks.PowerUps
{
    public class Laser : OMImage
    {
        public enum LaserDirection { Left, Up, Right, Down }

        private int Angle;
        private Single PixelsToMove = 1.0F;
        private Single _Speed = 4.0F;
        private LaserDirection _LaserDirection;
        private int _MaxWidth = 30;
        private int _MaxHeight = 30;
        private List<Bricks> HitBricks = new List<Bricks>();
        private bool canMove = true;

        public Single Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                if (value > 24.0F)
                    _Speed = 24.0F;
                else if (value < 1.0F)
                    _Speed = 1.0F;
                else
                    _Speed = value;
            }
        }

        public Laser(Bricks Brick, LaserDirection laserDirection, int angle)
        {
            _LaserDirection = laserDirection;
            switch (laserDirection)
            {
                case LaserDirection.Left:
                    Angle = 180;
                    break;
                case LaserDirection.Up:
                    Angle = 90;
                    break;
                case LaserDirection.Right:
                    Angle = 0;
                    break;
                case LaserDirection.Down:
                    Angle = 270;
                    break;
            }
            Width = 4;
            Height = 4;
            Left = Brick.Left + (Brick.Width / 2) - (Width / 2);
            Top = Brick.Top + (Brick.Height / 2) - (Height / 2);
            Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Laser");
        }

        public bool Move(List<Bricks> _Bricks)
        {
            bool Result = true;
            Single newPixelsToMove = PixelsToMove * Speed;
            double angle = Math.PI * Angle / 180;
            double test = Math.Sin(angle);
            double test2 = Math.Cos(angle);
            var RelY = (newPixelsToMove * test) * -1;
            var RelX = newPixelsToMove * test2;
            Result = Collision((int)Math.Round(RelX), (int)Math.Round(RelY), _Bricks);
            //Speed += .025F;
            return Result;
            //return totalBrickScore = Collision(_Bricks, (int)Math.Round(RelX), (int)Math.Round(RelY), p);
        }

        private bool Collision(int newX, int newY, List<Bricks> _Bricks)
        {
            bool Result = true;
            if(Width < _MaxWidth && _LaserDirection == LaserDirection.Right)
            {
                Width += newX;
                if(Width > _MaxWidth)
                    Width = _MaxWidth;
            }
            else if (Width < _MaxWidth && _LaserDirection == LaserDirection.Left)
            {
                Left += newX;
                Width -= newX;
                if (Width > _MaxWidth)
                {
                    Left += Width - _MaxWidth;
                    Width = _MaxWidth;
                }
            }
            else if (Height < _MaxHeight && _LaserDirection == LaserDirection.Down)
            {
                Height += newY;
                if (Height > _MaxHeight)
                    Height = _MaxHeight;
            }
            else if (Height < _MaxHeight && _LaserDirection == LaserDirection.Up)
            {
                Top += newY;
                Height -= newY;
                if (Height > _MaxHeight)
                {
                    Top += Height - _MaxHeight;
                    Height = _MaxHeight;
                }
            }
            else if (canMove)
            {
                Left += newX;
                Top += newY;
            }

            //test laser against outer client area
            if (Left + Width < OM.Host.ClientArea[0].Left || Left > OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width)
                Result = false;
            else if (Top + Height < OM.Host.ClientArea[0].Top || Top > OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height)
                Result = false;
            else
            {
                //test brick collisions...
                for (int i = 0; i < _Bricks.Count; i++)
                {
                    if ((_Bricks[i].Visible) && (!_Bricks[i].isDestroying) && (!_Bricks[i].IsHit) && (_Bricks[i].Breakable))
                    {
                        //if (Left + Width > _Bricks[i].Left && Left < _Bricks[i].Left + _Bricks[i].Width && Top + Height > _Bricks[i].Top && Top < _Bricks[i].Top + _Bricks[i].Height && !HitBricks.Contains(_Bricks[i]))
                        //{
                        //    _Bricks[i].IsHit = true;
                        //    HitBricks.Add(_Bricks[i]);
                        //}
                        if (Left <= _Bricks[i].Left + _Bricks[i].Width && Left + Width >= _Bricks[i].Left && Top <= _Bricks[i].Top + _Bricks[i].Height && Top + Height >= _Bricks[i].Top)
                        {

                            _Bricks[i].IsHit = true;
                            HitBricks.Add(_Bricks[i]);
                        }
                    }
                    else if (!_Bricks[i].Breakable)
                    {
                        if (Left <= _Bricks[i].Left + _Bricks[i].Width && Left + Width >= _Bricks[i].Left && Top <= _Bricks[i].Top + _Bricks[i].Height && Top + Height >= _Bricks[i].Top)
                        {
                            //de-extend lasers into this brick and when we can't anymore, return false
                            canMove = false;
                            if (_LaserDirection == LaserDirection.Down)
                            {
                                Top += newY;
                                Height -= newY * 2;
                                if (Height < 0)
                                    Result = false;
                            }
                            else if (_LaserDirection == LaserDirection.Up)
                            {
                                Height += newY;
                                if (Height < 0)
                                    Result = false;
                            }
                            else if (_LaserDirection == LaserDirection.Left)
                            {
                                Width += newX;
                                if (Width < 0)
                                    Result = false;
                            }
                            else if (_LaserDirection == LaserDirection.Right)
                            {
                                Left += newX;
                                Width -= newX * 2;
                                if (Width < 0)
                                    Result = false;
                            }
                        }
                    }
                }
            }
            return Result;
        }

    }
}
