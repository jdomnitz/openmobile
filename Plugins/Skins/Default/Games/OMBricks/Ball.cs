using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Graphics;

namespace OMBricks
{
    public class Ball : OMImage, IClickable, IHighlightable
    {
        public int screen;
        public XDirection ballXDir;
        public YDirection ballYDir;
        public float Velocity = 0.5f; //how fast to move
        private int movementPixels = 3;
        public bool Moving = false;
        public float MOVEMENT_SPEED = 0.5f; //pixels per second
        private float time;
        public bool dummyBall = false;
        public int ToMoveY = 4;
        public int ToMoveX = 4;
        private Single ToMoveYSingleInitial = 1.0F;
        private Single ToMoveXSingleInitial = 1.0F;
        public Single ToMoveYSingle = 1.0F;
        public Single ToMoveXSingle = 1.0F;
        public double _Left;
        public double _Top;
        
        public Single PixelsToMove = 1.0F;
        private Single InitialSpeed = 4.0F;
        private Single _Speed = 4.0F;
        public Single MaxSpeed = 7.0F;
        private bool KnockLeft = false;
        private bool KnockUp = false;

        public Single Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                if (value > MaxSpeed)
                    _Speed = MaxSpeed;
                else if (value < 1.0F)
                    _Speed = 1.0F;
                else
                    _Speed = value;
            }
        }

        private int _Angle;
        public int Angle
        {
            get
            {
                return _Angle;
            }
            set
            {
                if (value > 359)
                {
                    if (value == 360)
                        _Angle = 0;
                    else
                    {
                        _Angle = value - 360;
                    }
                }
                else if (value < 0)
                {
                    _Angle = 360 + value;
                }
                else
                    _Angle = value;

                //if (KnockUp)
                //{
                //    if (_Angle == 0)
                //        _Angle++;
                //    else if (_Angle == 180)
                //        _Angle--;
                //    KnockUp = false;
                //}
                //else //KnockDown
                //{
                //    if (_Angle == 0)
                //        _Angle--;
                //    else if (_Angle == 180)
                //        _Angle++;
                //    KnockUp = true;
                //}
                //if (KnockLeft)
                //{
                //    if (_Angle == 90)
                //        _Angle++;
                //    else if (_Angle == 270)
                //        _Angle--;
                //    KnockLeft = false;
                //}
                //else
                //{
                //    if (_Angle == 90)
                //        _Angle--;
                //    else if (_Angle == 270)
                //        _Angle++;
                //    KnockLeft = true;
                //}

                if (_Angle < 15 && _Angle > 0)
                {
                    _Angle = 15;
                }
                else if (_Angle < 360 && _Angle > 345)
                {
                    _Angle = 345;
                }
                else if (_Angle > 165 && _Angle < 180)
                {
                    _Angle = 165;
                }
                else if (_Angle > 180 && _Angle < 195)
                {
                    _Angle = 195;
                }
                

                if (_Angle > 0 && Angle < 180)
                {
                    ballYDir = YDirection.Up;
                }
                else if (_Angle > 180 && _Angle < 360)
                {
                    ballYDir = YDirection.Down;
                }
                if ((_Angle < 90 && _Angle > 0) || (_Angle > 270 && _Angle < 360))
                {
                    ballXDir = XDirection.Right;
                }
                else if (_Angle > 90 && _Angle < 270)
                {
                    ballXDir = XDirection.Left;
                }
            }
        }
        

        public Ball(Paddle p)
        {
            this.Width = 24;
            this.Left = p.Left + ((p.Width / 2) - (Width / 2));
            _Left = Left;
            this.Height = 25;
            this.Top = p.Top - 25;
            _Top = Top;
        }

        public void FireBall()
        {
            Moving = true;
        }

        public void Reset(Paddle p)
        {
            Moving = false;
            Speed = InitialSpeed;
            Left = p.Left + ((p.Width / 2) - (Width / 2));
            _Left = Left;
            Top = p.Top - 25;
            _Top = Top;
            Width = 24;
            Height = 25;
            
        }

        public bool MoveBall(List<Bricks> _Bricks, Paddle p, int BallCount)
        {
            int totalBrickScore = 0;
            int newY = 0;
            int newX = 0;

            Single newPixelsToMove = PixelsToMove * Speed;
            double angle = Math.PI * Angle / 180;
            double test = Math.Sin(angle);
            double test2 = Math.Cos(angle);
            double RelY = (newPixelsToMove * test) * -1;
            double RelX = newPixelsToMove * test2;
            return Collision(_Bricks, RelX, RelY, p, BallCount);




            if (ballYDir == YDirection.Up)
                newY += ToMoveY * -1;
            else
                newY += ToMoveY;
            if (ballXDir == XDirection.Left)
                newX += ToMoveX * -1;
            else
                newX += ToMoveX;
            //return totalBrickScore = Collision(_Bricks, newX, newY, p);

            Single newYSingle = 0;
            Single newXSingle = 0;
            if (ballYDir == YDirection.Up)
                newYSingle += ToMoveYSingle * -1.0F * Speed;
            else
                newYSingle += ToMoveYSingle * Speed;
            if (ballXDir == XDirection.Left)
                newXSingle += ToMoveXSingle * -1.0F * Speed;
            else
                newXSingle += ToMoveXSingle * Speed;
            //return totalBrickScore = Collision(_Bricks, (int)newXSingle, (int)newYSingle, p);

            
            
        }

        private bool PaddleCollision(Paddle p)
        {
            bool Result = false;
            if (ballYDir == YDirection.Down)
            {

                if (((this.Top + this.Height + movementPixels) > p.Top) && (Left >= p.Left && Left <= p.Left + p.Width))
                {
                    this.Top = p.Top - this.Height;
                    //newY = p.Top - this.Height;
                    //start sending up
                    ballYDir = YDirection.Up;
                    Result = true;
                }
            }
            else if (ballXDir == XDirection.Right)
            {

            }
            else if (ballXDir == XDirection.Left)
            {

            }
            return Result;
        }

        private bool Collision(List<Bricks> _Bricks, double newX, double newY, Paddle p, int BallCount)
        {
            bool Result = true;
            int totalBrickScore = 0;
            List<int> index = new List<int>();
            //int newMovementY = 100000;
            //int newMovementX = 100000;
            //bool hitY = false;
            //bool hitX = false;
            //int hitMovementY = 10000000;
            //int hitMovementX = 10000000;
            //int newYTop = 1000000;
            //int newXLeft = 1000000;
            //bool alreadyMoved = false;

            int circleRadius = (Width/2);
            double circleCenterX = _Left + circleRadius;
            double circleCenterY = _Top + circleRadius;
            double circleCenterXNew = circleCenterX + newX;
            double circleCenterYNew = circleCenterY + newY;
            double collisionDistanceX = 99999999;
            double collisionDistanceY = 99999999;
            List<BallBrickCollision> _Collisions = new List<BallBrickCollision>();

            //test collisions against walls
            if (ballXDir == XDirection.Right)
            {
                if (circleCenterXNew >= OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - circleRadius)
                {
                    //bounce left
                    collisionDistanceX = circleCenterXNew - (OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - circleRadius);
                    
                }
            }
            else
            {
                if (circleCenterXNew <= OM.Host.ClientArea[0].Left + circleRadius)
                {
                    //bounce right
                    collisionDistanceX = (OM.Host.ClientArea[0].Left + circleRadius) - circleCenterXNew;
                }
            }
            if (ballYDir == YDirection.Down)
            {
                
                if (Top >= OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height)
                {
                    //too far down, lose life
                    if (BallCount == 1)
                    {
                        OMBricks.gameStatus[screen] = GameEventsStatus.LostLife;
                    }
                    Result = false;
                    //collisionDistanceY = circleCenterYNew - OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height - circleRadius;
                }
            }
            else
            {
                if (circleCenterYNew <= OM.Host.ClientArea[0].Top + circleRadius)
                {
                    //bounce down
                    collisionDistanceY = (OM.Host.ClientArea[0].Top + circleRadius) - circleCenterYNew;
                }
            }
            if (collisionDistanceX == 99999999 && collisionDistanceY == 99999999)
            {
                //same movements towards wall, no collisions with the walls
            }
            else
            {
                //collision has occured, but on which side of wall?
                if (collisionDistanceX == collisionDistanceY)
                {
                    //same distance, must've hit a corner?
                    _Collisions.Add(new BallBrickCollision(null, true, collisionDistanceX, true, collisionDistanceY));
                }
                else if (collisionDistanceX < collisionDistanceY)
                {
                    //hit side before top/bottom
                    _Collisions.Add(new BallBrickCollision(null, true, collisionDistanceX, false, collisionDistanceY));
                }
                else
                {
                    //hit top/bottom before side
                    _Collisions.Add(new BallBrickCollision(null, false, collisionDistanceX, true, collisionDistanceY));
                }
            }

            //test collisions against paddle
            if (ballYDir == YDirection.Down)
            {
                if ((circleCenterYNew >= p.Top - circleRadius) && (circleCenterXNew > p.Left - circleRadius) && (circleCenterXNew < p.Left + p.Width + circleRadius))
                {
                    collisionDistanceY = circleCenterYNew - (p.Top - circleRadius);
                    _Collisions.Add(new BallBrickCollision(null, false, collisionDistanceX, true, collisionDistanceY, p));
                }
            }
            //if (ballXDir == XDirection.Right)
            //{
            //    if (circleCenterXNew >= p.Left - circleRadius && circleCenterYNew >= p.Top - circleRadius)
            //    {
            //        collisionDistanceX = circleCenterXNew - p.Left - circleRadius;
            //        _Collisions.Add(new BallBrickCollision(null, false, collisionDistanceX, true, collisionDistanceY, p));
            //    }
            //}
            //else
            //{
            //    if (circleCenterXNew <= p.Left + p.Width + circleRadius && circleCenterYNew >= p.Top - circleRadius)
            //    {
            //        collisionDistanceX = p.Left + p.Width + circleRadius - collisionDistanceX;
            //        _Collisions.Add(new BallBrickCollision(null, false, collisionDistanceX, true, collisionDistanceY, p));
            //    }
            //}

            //test collisions against bricks
            for (int i = 0; i < _Bricks.Count; i++)
            {
                if ((_Bricks[i].Visible) && (!_Bricks[i].isDestroying))
                {
                    collisionDistanceX = 99999999;
                    collisionDistanceY = 99999999;
                    Rectangle brickCollisionBox = new Rectangle(_Bricks[i].Left - circleRadius, _Bricks[i].Top - circleRadius, _Bricks[i].Width + Width, _Bricks[i].Height + Height);
                    if (ballXDir == XDirection.Right)
                    {
                        // -->
                        if ((circleCenterXNew >= brickCollisionBox.Left) && (circleCenterXNew < brickCollisionBox.Left + brickCollisionBox.Width) && ((circleCenterYNew >= brickCollisionBox.Top) && (circleCenterYNew <= brickCollisionBox.Top + brickCollisionBox.Height)))
                        {
                            //touches or collides with left of brick by how much
                            collisionDistanceX = circleCenterXNew - brickCollisionBox.Left;
                        }
                    }
                    else
                    {
                        // <--
                        if ((circleCenterXNew <= brickCollisionBox.Left + brickCollisionBox.Width) && (circleCenterXNew > brickCollisionBox.Left) && ((circleCenterYNew >= brickCollisionBox.Top) && (circleCenterYNew <= brickCollisionBox.Top + brickCollisionBox.Height)))
                        {
                            //touches or collides with right of brick
                            collisionDistanceX = brickCollisionBox.Left + brickCollisionBox.Width - circleCenterXNew;
                        }
                    }
                    if (ballYDir == YDirection.Down)
                    {
                        // ||V
                        if ((circleCenterYNew >= brickCollisionBox.Top) && (circleCenterYNew < brickCollisionBox.Top + brickCollisionBox.Height) && ((circleCenterXNew >= brickCollisionBox.Left) && (circleCenterXNew <= brickCollisionBox.Left + brickCollisionBox.Width)))
                        {
                            //touches or collides with top of brick
                            collisionDistanceY = circleCenterYNew - brickCollisionBox.Top;
                        }
                    }
                    else
                    {
                        // ||^
                        if ((circleCenterYNew <= brickCollisionBox.Top + brickCollisionBox.Height) && (circleCenterYNew > brickCollisionBox.Top)  && ((circleCenterXNew >= brickCollisionBox.Left) && (circleCenterXNew <= brickCollisionBox.Left + brickCollisionBox.Width)))
                        {
                            //touches or collides with bottom of brick
                            collisionDistanceY = brickCollisionBox.Top + brickCollisionBox.Height - circleCenterYNew;
                        }
                    }

                    if (collisionDistanceX == 99999999 && collisionDistanceY == 99999999)
                    {
                        //same movements towards brick, no collisions with this brick
                    }
                    else
                    {
                        //collision has occured, but on which side of brick?
                        if (collisionDistanceX == collisionDistanceY)
                        {
                            //same distance, must've hit a corner?
                            _Collisions.Add(new BallBrickCollision(_Bricks[i], true, collisionDistanceX, true, collisionDistanceY));
                        }
                        else if (collisionDistanceX < collisionDistanceY)
                        {
                            //hit side before top/bottom
                            _Collisions.Add(new BallBrickCollision(_Bricks[i], true, collisionDistanceX, false, collisionDistanceY));
                        }
                        else
                        {
                            //hit top/bottom before side
                            _Collisions.Add(new BallBrickCollision(_Bricks[i], false, collisionDistanceX, true, collisionDistanceY));
                        }
                    }
                }
            }
            if (_Collisions.Count > 0)
            {
                //there was a collision
                double lowestCollisionValue = 99999;
                for (int i = 0; i < _Collisions.Count; i++)
                {
                    if (_Collisions[i].CollisionDistanceX < lowestCollisionValue)
                        lowestCollisionValue = _Collisions[i].CollisionDistanceX;
                    if (_Collisions[i].CollisionDistanceY < lowestCollisionValue)
                        lowestCollisionValue = _Collisions[i].CollisionDistanceY;
                }
                //now we have the LOWEST possible collision factor
                bool angledChanged = false;
                for (int i = 0; i < _Collisions.Count; i++)
                {
                    if ((_Collisions[i].CollisionDistanceX == lowestCollisionValue) && (_Collisions[i].CollisionDistanceY == lowestCollisionValue))
                    {
                        if (_Collisions[i].Brick != null)
                            _Collisions[i].Brick.Hit(this);
                        _Top += _Collisions[i].CollisionDistanceY;
                        _Left += _Collisions[i].CollisionDistanceX;
                        if (!angledChanged)
                        {
                            angledChanged = true;
                            Angle += 180; //complete opposite
                        }
                    }
                    else if (_Collisions[i].CollisionDistanceX == lowestCollisionValue)
                    {
                        //change x direction
                        if (_Collisions[i].Brick != null)
                            _Collisions[i].Brick.Hit(this);
                        _Top += _Collisions[i].CollisionDistanceX;
                        _Left += _Collisions[i].CollisionDistanceX;
                        if (!angledChanged)
                        {
                            angledChanged = true;
                            if (Angle < 90)
                            {
                                Angle = 180 - Angle;
                            }
                            else if (Angle < 180)
                            {
                                Angle = 180 - Angle;
                            }
                            else if (Angle < 270)
                            {
                                Angle = 360 - Angle - 180;
                            }
                            else if (Angle < 360)
                            {
                                Angle = 180 + (360 - Angle);
                            }
                        }
                    }
                    else if (_Collisions[i].CollisionDistanceY == lowestCollisionValue)
                    {
                        //change y direction
                        if (_Collisions[i].Brick != null)
                            _Collisions[i].Brick.Hit(this);
                        _Top += _Collisions[i].CollisionDistanceY;
                        _Left += _Collisions[i].CollisionDistanceY;
                        if (!angledChanged)
                        {
                            angledChanged = true;
                            if (_Collisions[i].p != null)
                            {
                                int ballHalf = Left + Width / 2;
                                int paddleHalf = p.Left + p.Width / 2;
                                int whereHit = ballHalf - paddleHalf;
                                double paddleAngles = p.Width / 2 / 45;
                                double angleChange = whereHit * paddleAngles;
                                if (90 - (int)Math.Round(angleChange) > 160)
                                    Angle = 160;
                                else if (90 - (int)Math.Round(angleChange) < 20)
                                    Angle = 20;
                                else
                                    Angle = 90 - (int)Math.Round(angleChange);
                            }
                            else if (Angle < 90)
                            {
                                Angle = 360 - Angle;
                            }
                            else if (Angle < 180)
                            {
                                Angle = 180 + (180 - Angle);
                            }
                            else if (Angle < 270)
                            {
                                Angle = 180 - (Angle - 180);
                            }
                            else if (Angle < 360)
                            {
                                Angle = 360 - Angle;
                            }
                        }
                    }
                }
            }
            else
            {
                //no collisions with walls, paddles, or bricks, just move normally now...
                _Left += newX;
                _Top += newY;
                Top = (int)Math.Round(_Top);
                Left = (int)Math.Round(_Left);
            }
            return Result;

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //        //int circleDistanceX = Math.Abs((Left + newX) - _Bricks[i].Left);
            //        //int circleDistanceY = Math.Abs((Top + newY) - _Bricks[i].Top);

            //        ////circleDistanceX >
            //        //int circleTry1 = (_Bricks[i].Width / 2 + (Width / 2));
            //        ////circleDistanceY >
            //        //int circleTry2 = (_Bricks[i].Height / 2 + (Width / 2));

            //        ////circleDistanceX <=
            //        //int circleTry3 = (_Bricks[i].Width / 2);
            //        ////circleDistanceY <=
            //        //int circleTry4 = (_Bricks[i].Height/2);

            //        ////square
            //        //int circleTry5 = (circleDistanceX - _Bricks[i].Width/2)^2 + (circleDistanceY - _Bricks[i].Height/2)^2;
            //        //int circleTry6 = (Width/2)^2;

            //        if (ballYDir == YDirection.Down)
            //        {
            //            newMovementY = newY - (_Bricks[i].Top - (Top + Height));
            //            if ((newMovementY < newY) && (Top < _Bricks[i].Top) && (newMovementY > newY * -1)) //200 - (190 + 8 + 3) = -1 < 4 ---- 3 + -1 = 2 (movement before the collision)
            //            {
            //                //hit vertically
            //                if ((Left + Width + newMovementY >= _Bricks[i].Left) && (Left < _Bricks[i].Left + _Bricks[i].Width)) //test the x-axis still to make sure this is in line
            //                {
            //                    hitY = true;
            //                    newYTop = _Bricks[i].Top - Height;
            //                    hitMovementY = newMovementY;
            //                    if (!index.Contains(i))
            //                        index.Add(i);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            newMovementY = (_Bricks[i].Top + _Bricks[i].Height) - (Top + newY);
            //            if (newMovementY > newY)// 200 + 50 = 250 - (252 + - 4) = 248 ----- 200 + 50 - 248 = 2 (movement before the collision)
            //            {
            //                //hit vertically
            //                if ((Left + Width + newMovementY >= _Bricks[i].Left) && (Left < _Bricks[i].Left + _Bricks[i].Width) && (newMovementY < newMovementY * -1) && (newMovementY < 0)) //test the x-axis still to make sure this is in line
            //                {
            //                    hitY = true;
            //                    newYTop = _Bricks[i].Top + _Bricks[i].Height;
            //                    hitMovementY = newMovementY;
            //                    if (!index.Contains(i))
            //                        index.Add(i);
            //                }
            //            }
            //        }
            //        if (ballXDir == XDirection.Right)
            //        {
            //            newMovementX = (Left + Width - _Bricks[i].Left); //(197 + 4) - 200 = 1
            //            if ((newMovementX < newX) && (newMovementX > newX * -1))
            //            {
            //                //hit horizontally
            //                if ((Top + Height >= _Bricks[i].Top) && (Top <= _Bricks[i].Top + _Bricks[i].Height))
            //                {
            //                    hitX = true;
            //                    newXLeft = _Bricks[i].Left - Width;
            //                    hitMovementX = newMovementX;
            //                    if (!index.Contains(i))
            //                        index.Add(i);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            newMovementX = (_Bricks[i].Left + _Bricks[i].Width) - (Left + newX); // 202 - 4 = 198 - (139  + 50) =   -1       150 + 50 = 200 - (202 - 4) = 200 - 198 = 2
            //            if ((newMovementX > newX) && (newMovementX < newX * -1))
            //            {
            //                //hit horizontally
            //                if ((Top <= _Bricks[i].Top + _Bricks[i].Height) && (Top + Height >= _Bricks[i].Top))
            //                {
            //                    hitX = true;
            //                    newXLeft = _Bricks[i].Left + _Bricks[i].Width;
            //                    hitMovementX = newMovementX;
            //                    if (!index.Contains(i))
            //                        index.Add(i);
            //                }
            //            }
            //        }

            //    }
            //    else if ((_Bricks[i].Visible) && (_Bricks[i].isDestroying))
            //    {
            //        //update the destroying animation
            //        _Bricks[i].UpdateDestroy();
            //    }
            //}
            ////after testing all the bricks, move the ball accordingly
            //if (hitY || hitX)
            //{
            //    if (hitMovementY < 0) hitMovementY *= -1;
            //    if (hitMovementX < 0) hitMovementX *= -1;
            //    if (hitMovementY == hitMovementX)
            //    {
            //        //collision detected in the same amount of pixel moves on both axis', change both y/x directions
            //        if (!alreadyMoved)
            //        {
            //            alreadyMoved = true;
            //            Top = newYTop;
            //            if (ballYDir == YDirection.Up)
            //                ballYDir = YDirection.Down;
            //            else
            //                ballYDir = YDirection.Up;
            //            Left = newXLeft;
            //            if (ballXDir == XDirection.Right)
            //                ballXDir = XDirection.Left;
            //            else
            //                ballXDir = XDirection.Right;
            //        }
            //    }
            //    else if (hitMovementY < hitMovementX)
            //    {
            //        //collision detected vertically before horizontally
            //        if (!alreadyMoved)
            //        {
            //            alreadyMoved = true;
            //            Top = newYTop;
            //            if (ballYDir == YDirection.Up)
            //                ballYDir = YDirection.Down;
            //            else
            //                ballYDir = YDirection.Up;
            //            if (ballXDir == XDirection.Left)
            //                Left -= hitMovementY;
            //            else
            //                Left += hitMovementY;
            //        }
            //    }
            //    else
            //    {
            //        //collision detected horizontally before vertically
            //        if (!alreadyMoved)
            //        {
            //            alreadyMoved = true;
            //            Left = newXLeft;
            //            if (ballYDir == YDirection.Up)
            //                Top -= hitMovementX;
            //            else
            //                Top += hitMovementX;
            //            if (ballXDir == XDirection.Left)
            //                ballXDir = XDirection.Right;
            //            else
            //                ballXDir = XDirection.Left;
            //        }
            //    }
            //}
            //else
            //{
            //    //paddle collision?
            //    int sectionWidth = (p.Width / 13);
            //    int ballMiddle = Left + (Width / 2);
            //    int sectionHit = 0;
            //    if (ballYDir == YDirection.Down)
            //    {
            //        newMovementY = newY - (p.Top - (Top + Height));
            //        if ((newMovementY < newY) && (Top < p.Top) && (newMovementY > newY * -1))
            //        {
            //            //hit vertically
            //            if ((Left + Width + newMovementY >= p.Left) && (Left < p.Left + p.Width)) //test the x-axis still to make sure this is in line
            //            {
            //                hitY = true;
            //                newYTop = p.Top - Height;
            //                hitMovementY = newMovementY;
            //                for (int i = 0; i < 13; i++)
            //                {
            //                    if ((ballMiddle > p.Left + (sectionWidth * i)) && (ballMiddle < p.Left + (sectionWidth * i) + sectionWidth))
            //                    {
            //                        sectionHit = i;
            //                        break;
            //                    }
            //                }

            //            }
            //        }
            //    }
            //    if (hitY)
            //    {
            //        if (!alreadyMoved)
            //        {
            //            alreadyMoved = true;
            //            switch (sectionHit)
            //            {
            //                case 0:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = ToMoveYSingleInitial / 2.0F; //2
            //                    break;
            //                case 1:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = 0.6F; //3
            //                    break;
            //                case 2:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 3:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = 0.6F; //3
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 4:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = ToMoveXSingleInitial / 2.0F; //2
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 5:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = 1;
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 6:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Left;
            //                    ToMoveXSingle = 0;
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 7:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = 1;
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 8:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = ToMoveXSingleInitial / 2.0F;
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 9:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = 3;
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 10:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = ToMoveYSingleInitial; //4
            //                    break;
            //                case 11:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = 0.6F; //3
            //                    break;
            //                case 12:
            //                    ballYDir = YDirection.Up;
            //                    ballXDir = XDirection.Right;
            //                    ToMoveXSingle = ToMoveXSingleInitial; //4
            //                    ToMoveYSingle = ToMoveYSingleInitial; //2
            //                    break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //else, move the ball normally
            //        if (ballYDir == YDirection.Up)
            //        {
            //            if (Top + newY <= OM.Host.ClientArea[0].Top)
            //            {
            //                Top = OM.Host.ClientArea[0].Top;
            //                ballYDir = YDirection.Down;
            //            }
            //            else
            //                Top += newY;
            //        }
            //        else
            //        {
            //            if (Top + Height + newY >= OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height)
            //            {
            //                //hit bottom of client area, game over/lose life/etc
            //                if (!dummyBall)
            //                {
            //                    if (OMBricks.gameLives[screen] == 0)
            //                    {
            //                        OMBricks.gameStatus[screen] = GameEventsStatus.GameOver;
            //                    }
            //                    else
            //                    {
            //                        OMBricks.gameStatus[screen] = GameEventsStatus.LostLife;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                Top += newY;
            //            }
            //        }
            //        if (ballXDir == XDirection.Left)
            //        {
            //            if (Left - newX <= OM.Host.ClientArea[0].Left)
            //            {
            //                Left = OM.Host.ClientArea[0].Left;
            //                ballXDir = XDirection.Right;
            //            }
            //            else
            //                Left += newX;
            //        }
            //        else
            //        {
            //            if (Left + Width + newX >= OM.Host.ClientArea[0].Width)
            //            {
            //                Left = OM.Host.ClientArea[0].Width - Width;
            //                ballXDir = XDirection.Left;
            //            }
            //            else
            //                Left += newX;
            //        }
            //    }
            //}
            ////now destroy the bricks that the ball hit...
            //if (index.Count > 0)
            //{
            //    for (int i = 0; i < index.Count; i++)
            //    {
            //        if (_Bricks[index[i]].Breakable)
            //        {
            //            //totalBrickScore += _Bricks[index[i]].Destroyed(screen, this, p, _Bricks);
            //            _Bricks[index[i]].Hit = true;
            //        }
            //    }
            //    //if (NoMoreBricks(_Bricks))
            //    //{
            //    //    //level is over
            //    //    OMBricks.gameStatus[screen] = GameEventsStatus.NextLevel;
            //    //}
            //}
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //return totalBrickScore;
        }

        private bool NoBrickRightSide(List<Bricks> _Bricks, int currentBrick)
        {
            bool Result = true;
            for (int i = 0; i < _Bricks.Count; i++)
            {
                if (_Bricks[i] != _Bricks[currentBrick])
                {
                    if (_Bricks[i].Left == _Bricks[currentBrick].Left + _Bricks[currentBrick].Width)
                    {
                        Result = false;
                        break;
                    }
                }
            }
            return Result;
        }

        private bool NoBrickLeftSide(List<Bricks> _Bricks, int currentBrick)
        {
            bool Result = true;
            for (int i = 0; i < _Bricks.Count; i++)
            {
                if (_Bricks[i] != _Bricks[currentBrick])
                {
                    if (_Bricks[currentBrick].Left == _Bricks[i].Left + _Bricks[i].Width)
                    {
                        Result = false;
                        break;
                    }
                }
            }
            return Result;
        }

        private bool NoMoreBricks(List<Bricks> _Bricks)
        {
            bool result = true;
            for (int i = 0; i < _Bricks.Count; i++)
            {
                if ((_Bricks[i].Visible) && (!_Bricks[i].isDestroying))
                {
                    if (_Bricks[i].Breakable)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        public void clickMe(int screen)
        {
            //if (!Moving)
            //{
            //    Moving = true;
            //}
        }

        public void longClickMe(int screen)
        {
            //
        }

        public void holdClickMe(int screen)
        {
            //
        }

        public event userInteraction OnClick;

        public event userInteraction OnLongClick;

        public event userInteraction OnHoldClick;

    }
}
