using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Graphics;

namespace OMBricks
{
    public class LevelGenerator
    {

        public static readonly int MaxLevel = 5;

        private static Bricks br;
        private static int brickWidth;
        private static int brickHeight;
        private static int BrickAreaTop = OM.Host.ClientArea[0].Top + 30;
        private static Random _Random = new Random();

        private static void AddBrick(Bricks Brick, ScreenManager _Manager, List<Bricks> _Bricks, int screen)
        {
            _Manager[screen, "mainPanel"].addControl(Brick);
            _Bricks.Add(Brick);
        }

        public static void GenerateRandom(IBasePlugin OMBricks, ScreenManager Manager, int screen, List<Bricks> _Bricks, List<Ball> _Balls)
        {
            int minWidthHeight = 15;
            int maxWidth = 101;
            int maxHeight = 101;
            int bricksToGenerate;
            bool brickPlaced = false;
            if(_Bricks.Count > 10)
                bricksToGenerate= _Random.Next(0, 3); // or 2
            else
                bricksToGenerate = _Random.Next(1, 3); //1 or 2
            for (int i = 0; i < bricksToGenerate; i++)
            {
                Bricks br = new Bricks();
                brickPlaced = false;
                while (!brickPlaced)
                {
                    br.Left = _Random.Next(OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - minWidthHeight);
                    br.Top = _Random.Next(OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height - 150);
                    br.Height = _Random.Next(minWidthHeight, maxHeight);
                    br.Width = _Random.Next(minWidthHeight, maxWidth);
                    brickPlaced = PlaceBrick(br, _Bricks, _Balls);
                }
                br.BrickLevel = _Random.Next(1, 4);
                br.Breakable = true;
                if (GetsPowerUp() < 6)
                {
                    switch (RandomPowerUp())
                    {
                        case 1:
                            br.PowerUp = PowerUps.PowerUps.Laser;
                            break;
                        case 2:
                            br.PowerUp = PowerUps.PowerUps.MultiBall;
                            break;
                        case 3:
                            br.PowerUp = PowerUps.PowerUps.Life;
                            break;
                        case 4:
                            br.PowerUp = PowerUps.PowerUps.PaddleShooter;
                            break;
                    }
                }
                //br.PowerUp = PowerUps.PowerUps.Laser;
                AddBrick(br, Manager, _Bricks, screen);
            }
        }

        private static int GetsPowerUp()
        {
            return _Random.Next(1, 101);
        }

        private static int RandomPowerUp()
        {
            return _Random.Next(1, 5);
        }

        private static bool PlaceBrick(Bricks Brick, List<Bricks> _Bricks, List<Ball> _Balls)
        {
            bool Result = true;
            double maxBallMovement;
            for (int i = 0; i < _Bricks.Count; i++)
            {
                if (Brick.Left <= _Bricks[i].Left + _Bricks[i].Width && Brick.Left + Brick.Width >= _Bricks[i].Left && Brick.Top <= _Bricks[i].Top + _Bricks[i].Height && Brick.Top + Brick.Height >= _Bricks[i].Top)
                {
                    Result = false;
                    break;
                }
            }
            for (int i = 0; i < _Balls.Count; i++)
            {
                maxBallMovement = _Balls[i].PixelsToMove * _Balls[i].MaxSpeed;
                if (_Balls[i].Left - maxBallMovement <= _Bricks[i].Left + _Bricks[i].Width && _Balls[i].Left + _Balls[i].Width + maxBallMovement >= _Bricks[i].Left && _Balls[i].Top - maxBallMovement <= _Bricks[i].Top + _Bricks[i].Height && _Balls[i].Top + _Balls[i].Height + maxBallMovement >= _Bricks[i].Top)
                {
                    Result = false;
                    break;
                }
            }
            return Result;
        }

        public static void GenerateLevel(IBasePlugin OMBricks, ScreenManager Manager, int screen, int GameLevel, List<Bricks> _Bricks)
        {
            switch (GameLevel)
            {
                case 1:
                    for (int i = 0; i < 7; i++)
                    {
                        br = new Bricks();
                        br.Name = String.Format("Brick.{0}", i.ToString());
                        br.Left = 200 + (br.Width * i) + (10 * i);
                        br.Top = 200;
                        br.BrickLevel = 1;
                        br.Breakable = true;
                        if (i == 0)
                            br.PowerUp = PowerUps.PowerUps.Life;
                        else if (i == 2)
                            br.PowerUp = PowerUps.PowerUps.Laser;
                        else if (i == 3)
                            br.PowerUp = PowerUps.PowerUps.PaddleShooter;
                        else if (i == 5)
                            br.PowerUp = PowerUps.PowerUps.MultiBall;
                        AddBrick(br, Manager, _Bricks, screen);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 7; i++)
                    {

                        for (int j = 0; j < 2; j++)
                        {
                            br = new Bricks();
                            if (i == 2 && j == 1)
                            {
                                br.PowerUp = PowerUps.PowerUps.Laser;
                                br.Name = String.Format("Laser.{0}", i.ToString());
                            }
                            br.Name = String.Format("Brick.{0}.{1}", i.ToString(), j.ToString());
                            br.Left = 200 + (br.Width * i) + (10 * i);
                            br.Top = 200 - (br.Height * j) - (10 * j);
                            br.BrickLevel = 1;
                            br.Breakable = true;
                            Manager[screen, "mainPanel"].addControl(br);
                            _Bricks.Add(br);
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < 4; i++)
                    {
                        br = new Bricks();
                        if (i < 2)
                            br.Left = OM.Host.ClientArea[0].Left + (br.Width * i) + (10 * i);
                        else
                            br.Left = OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - (br.Width * (i - 1)) - (10 * (i - 1));
                        br.Top = OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height - 100;
                        br.Breakable = false;
                        Manager[screen, "mainPanel"].addControl(br);
                        _Bricks.Add(br);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        br = new Bricks();
                        if (i < 2)
                            br.Left = OM.Host.ClientArea[0].Left + (br.Width * i) + (10 * i);
                        else
                            br.Left = OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - (br.Width * (i - 1)) - (10 * (i - 2));
                        br.Top = OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height - 135;
                        br.Breakable = true;
                        br.BrickLevel = 1;
                        Manager[screen, "mainPanel"].addControl(br);
                        _Bricks.Add(br);
                    }
                    brickWidth = (OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width - 40) / 5;
                    for (int i = 0; i < 5; i++)
                    {
                        br = new Bricks();
                        if(i == 0 || i == 4)
                            br.PowerUp = PowerUps.PowerUps.Laser;
                        br.Left = OM.Host.ClientArea[0].Left + (brickWidth * i) + (10 * i);
                        br.Top = OM.Host.ClientArea[0].Top + OM.Host.ClientArea[0].Height - 170;
                        br.Width = brickWidth;
                        br.Breakable = true;
                        br.BrickLevel = 2;
                        Manager[screen, "mainPanel"].addControl(br);
                        _Bricks.Add(br);
                    }
                    break;
                case 4:
                    brickWidth = OM.Host.ClientArea[0].Width / 15;
                    brickHeight = (OM.Host.ClientArea[0].Height - 15) / 15;
                    for (int i = 0; i < 15; i++)
                    {
                        br = new Bricks();
                        br.Width = brickWidth;
                        br.Left = OM.Host.ClientArea[0].Left + (i * brickWidth);
                        br.Top = OM.Host.ClientArea[0].Top + 15 + (i * brickHeight);
                        br.Height = brickHeight;
                        br.Breakable = true;
                        if (i < 5)
                            br.BrickLevel = 3;
                        else if (i < 10)
                            br.BrickLevel = 2;
                        else
                            br.BrickLevel = 1;
                        AddBrick(br, Manager, _Bricks, screen);
                    }
                    for (int i = 0; i < 15; i++)
                    {
                            br = new Bricks();
                            br.Width = brickWidth;
                            br.Left = (OM.Host.ClientArea[0].Left + OM.Host.ClientArea[0].Width) - ((i + 1) * brickWidth);
                            br.Top = OM.Host.ClientArea[0].Top + 15 + (i * brickHeight);
                            br.Height = brickHeight;
                            br.Breakable = true;
                            if (i < 5)
                                br.BrickLevel = 3;
                            else if (i < 10)
                                br.BrickLevel = 2;
                            else
                                br.BrickLevel = 1;
                            AddBrick(br, Manager, _Bricks, screen);
                    }
                    break;
                case 5:
                    brickWidth = OM.Host.ClientArea[0].Width / 3;
                    for (int i = 0; i < 3; i++)
                    {
                        br = new Bricks();
                        br.Width = brickWidth;
                        br.Left = OM.Host.ClientArea[0].Left + (i * brickWidth);
                        br.Top = OM.Host.ClientArea[0].Top + ((OM.Host.ClientArea[0].Height / 5) * 3); //60% from top
                        br.BrickLevel = 3;
                        br.Breakable = true;
                        AddBrick(br, Manager, _Bricks, screen);
                    }
                    break;
            }
        }
    }
}
