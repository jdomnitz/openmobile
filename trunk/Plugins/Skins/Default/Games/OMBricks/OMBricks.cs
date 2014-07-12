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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.Graphics;
using OpenMobile.Data;
using System.Threading;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OMBricks.PowerUps;

namespace OMBricks
{

    public enum XDirection { Left, Right }
    public enum YDirection { Up, Down }
    public enum GameEventsStatus { Beginning, Running, LostLife, GameOver, GameWon, NextLevel, GameQuit }

    public class OMBricks : HighLevelCode
    {

        public OMBricks()
            : base("OMBricks", OM.Host.getPluginImage<OMBricks>("Images|Icon-OMBricks"), 1.0f, "OM Brick Breaker", "OMBricks", "Peter Yeaney", "peter.yeaney@outlook.com")
        {
        }

        public static bool[] isPaused;
        bool[] isRunning;
        bool[] animating;
        bool[] gameSleep;
        static int[] currentScore;
        public int highScore = 0;
        public static GameEventsStatus[] gameStatus;
        public static int[] gameLives;
        int[] gameLevel;
        public static List<Laser>[] _Lasers;
        public static List<PaddleShooter>[] _PaddleShooter;
        public static List<PaddleShots>[] _PaddleShots;
        private bool[] moreToHit;
        private bool[] randomLevel;
        private ButtonStrip PopUpMenuStrip;
        private bool alreadyAdded = false;

        public override eLoadStatus initialize(IPluginHost host)
        {
            isPaused = new bool[OM.Host.ScreenCount];
            isRunning = new bool[OM.Host.ScreenCount];
            animating = new bool[OM.Host.ScreenCount];
            gameSleep = new bool[OM.Host.ScreenCount];
            currentScore = new int[OM.Host.ScreenCount];
            gameStatus = new GameEventsStatus[OM.Host.ScreenCount];
            gameLives = new int[OM.Host.ScreenCount];
            gameLevel = new int[OM.Host.ScreenCount];
            _Lasers = new List<Laser>[OM.Host.ScreenCount];
            _PaddleShooter = new List<PaddleShooter>[OM.Host.ScreenCount];
            _PaddleShots = new List<PaddleShots>[OM.Host.ScreenCount];
            moreToHit = new bool[OM.Host.ScreenCount];
            randomLevel = new bool[OM.Host.ScreenCount];
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                isPaused[i] = false;
                isRunning[i] = false;
                animating[i] = false;
                gameSleep[i] = false;
                currentScore[i] = 0;
                gameStatus[i] = GameEventsStatus.Beginning;
                gameLives[i] = 3;
                gameLevel[i] = 1;
                _Lasers[i] = new List<Laser>();
                _PaddleShooter[i] = new List<PaddleShooter>();
                _PaddleShots[i] = new List<PaddleShots>();
                moreToHit[i] = true;
                randomLevel[i] = false;
            }

            PopUpMenuStrip = new ButtonStrip(this.pluginName, "OMBricks", "PopUpMenuStrip");

            string scoreResult = StoredData.Get(this, "OMBricks.HighScore"); //NOT screen independent
            if (scoreResult == "")
                highScore = 0;
            else
                highScore = Convert.ToInt16(scoreResult);

            OMPanel p = new OMPanel("mainPanel", this.displayName, this.pluginIcon);

            OMImage imgBackground = new OMImage("imgBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            imgBackground.BackgroundColor = Color.Black;
            imgBackground.Transparency = 75;
            p.addControl(imgBackground);

            OMButton singlePlayer = OMButton.PreConfigLayout_BasicStyle("singlePlayer", 400, 200, 200, 100, GraphicCorners.All);
            singlePlayer.Text = "Arcade";
            singlePlayer.OnClick += new userInteraction(singlePlayer_OnClick);
            p.addControl(singlePlayer);

            OMButton singleEndless = OMButton.PreConfigLayout_BasicStyle("singleEndless", 400, 300, 200, 100, GraphicCorners.All);
            singleEndless.Text = "Endless";
            singleEndless.OnClick += new userInteraction(singleEndless_OnClick);
            p.addControl(singleEndless);

            OMLabel gameLivesLabel = new OMLabel("gameLivesLabel", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Width, 15);
            gameLivesLabel.AutoFitTextMode = FitModes.FitFill;
            gameLivesLabel.TextAlignment = Alignment.CenterLeft;
            gameLivesLabel.Text = "Lives: 3";
            gameLivesLabel.Visible = false;
            p.addControl(gameLivesLabel);

            OMLabel gameScores = new OMLabel("gameScores", OM.Host.ClientArea[0].Left, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Width, 30);
            gameScores.AutoFitTextMode = FitModes.FitFill;
            gameScores.TextAlignment = Alignment.CenterRight;
            gameScores.Visible = false;
            p.addControl(gameScores);

            AngleOverlay angleOverlay = new AngleOverlay();
            angleOverlay.Name = "angleOverlay";
            angleOverlay.Visible = false;
            angleOverlay.FillColor = Color.Transparent;
            angleOverlay.Left = OM.Host.ClientArea[0].Left;
            angleOverlay.Width = OM.Host.ClientArea[0].Width;
            angleOverlay.Top = OM.Host.ClientArea[0].Top;
            p.addControl(angleOverlay);

            ScoreUpdated += new ScoreUpdatedEventHandler(OMBricks_ScoreUpdated);
            p.Entering += new PanelEvent(p_Entering);
            p.Leaving += new PanelEvent(p_Leaving);
            //OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
            OM.Host.OnPowerChange += new PowerEvent(Host_OnPowerChange);

            base.PanelManager.loadPanel(p, true);

            return eLoadStatus.LoadSuccessful;
        }

        void p_Entering(OMPanel sender, int screen)
        {
            if (!alreadyAdded)
            {
                PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("popupExitGame", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Quit Game", false, QuitGame_OnClick, null, null));
                alreadyAdded = true;
            }
            
        }

        private void QuitGame_OnClick(object sender, int screen)
        {
            gameStatus[screen] = GameEventsStatus.GameOver;
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
        }

        void Host_OnPowerChange(ePowerEvent type)
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                switch (type)
                {
                    case ePowerEvent.SleepOrHibernatePending:
                        gameSleep[i] = true;
                        PauseGame(i);
                        break;
                    case ePowerEvent.ShutdownPending:
                        SaveHighScore(i, false);
                        break;
                    case ePowerEvent.SystemResumed:
                        if (gameSleep[i])
                        {
                            isRunning[i] = true;
                            b.Moving = true;
                            new Thread(new ParameterizedThreadStart(Start)).Start(i);
                        }
                        break;
                }
            }
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                switch (function)
                {
                    case eFunction.closeProgram:
                        SaveHighScore(i, false);
                        break;
                }
            }
        }

        void p_Leaving(OMPanel sender, int screen)
        {
            for (int i = 0; i < _Balls.Count; i++)
            {
                if(_Balls[i].Moving)
                    PauseGame(screen);
            }
        }

        private void PauseGame(int screen)
        {
            isPaused[screen] = true;
        }

        void continueOverlay_OnClick(OMControl sender, int screen)
        {
            base.PanelManager[screen, "mainPanel"].Remove(sender);
            isPaused[screen] = false;
        }

        private void EndGame(int screen)
        {
            animating[screen] = false;
            isRunning[screen] = false;
            base.PanelManager[screen, "mainPanel"].Remove(p);
            base.PanelManager[screen, "mainPanel"].Remove(b);
            ClearBricks(_Bricks, screen);
            base.PanelManager[screen, "mainPanel"].Remove(_Balls[0]);
            _Balls.RemoveAt(0);
            //generateLevel = true;
            base.PanelManager[screen, "mainPanel"]["singlePlayer"].Visible = true;
            base.PanelManager[screen, "mainPanel"]["singleEndless"].Visible = true;
            base.PanelManager[screen, "mainPanel"]["gameLivesLabel"].Visible = false;
            base.PanelManager[screen, "mainPanel"]["gameScores"].Visible = false;
            base.PanelManager[screen, "mainPanel"]["angleOverlay"].Visible = false;
            SaveHighScore(screen);
            OM.Host.UIHandler.PopUpMenu_Clear(screen);
        }

        private void SaveHighScore(int screen, bool RedisplayScore = true)
        {
            if (currentScore[screen] > highScore)
                StoredData.Set(this, "OMBricks.HighScore", currentScore[screen].ToString());
            if (RedisplayScore)
                SetScore(screen);
        }

        private delegate void ScoreUpdatedEventHandler(int screen);
        private static event ScoreUpdatedEventHandler ScoreUpdated;
        public static void UpdateScore(int screen, int score)
        {
            currentScore[screen] += score;
            if (ScoreUpdated != null)
                ScoreUpdated(screen);
        }

        void OMBricks_ScoreUpdated(int screen)
        {
            SetScore(screen);
        }

        private void SetScore(int screen, bool ResetScore = false)
        {
            if (ResetScore)
            {
                currentScore[screen] = 0;
                ((OMLabel)base.PanelManager[screen, "mainPanel"]["gameScores"]).Text = String.Format("High Score: {0}{1}Current Score: {2}", highScore, Environment.NewLine, currentScore[screen].ToString());
            }
            else
                ((OMLabel)base.PanelManager[screen, "mainPanel"]["gameScores"]).Text = String.Format("High Score: {0}{1}Current Score: {2}", highScore, Environment.NewLine, currentScore[screen].ToString());
        }

        public enum ModifyingLives { AddLives, SubtractLives, SetLives }
        private void ModifyLives(int screen, ModifyingLives LivesChange, int Lives = 1)
        {
            if (gameLives[0] == 0 && LivesChange == ModifyingLives.SubtractLives)
            {
                OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Game Over!"));
                gameStatus[screen] = GameEventsStatus.GameOver;
                return;
            }
            if (LivesChange == ModifyingLives.SetLives)
                gameLives[screen] = Lives;
            else if (LivesChange == ModifyingLives.AddLives)
                gameLives[screen] += Lives;
            else
                gameLives[screen] -= Lives;
            ((OMLabel)base.PanelManager[screen, "mainPanel"]["gameLivesLabel"]).Text = String.Format("Lives: {0}", gameLives[screen].ToString());
        }

        Paddle p;
        Ball b;
        void singlePlayer_OnClick(OMControl sender, int screen)
        {
            randomLevel[screen] = false;
            NewGame(screen);
        }

        private void singleEndless_OnClick(object sender, int screen)
        {
            randomLevel[screen] = true;
            NewGame(screen);
        }

        private void NewGame(int screen)
        {
            ((OMButton)base.PanelManager[screen, "mainPanel"]["singlePlayer"]).Visible = false;
            ((OMButton)base.PanelManager[screen, "mainPanel"]["singleEndless"]).Visible = false;
            OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip);
            gameLevel[screen] = 0;
            p = new Paddle();
            p.Image = OM.Host.getPluginImage(this, "Images|OMBricks-Paddle2");
            p.Name = "Paddle";
            base.PanelManager[screen, "mainPanel"].addControl(p);
            //add the first main ball to the list
            b = new Ball(p);
            b.Image = OM.Host.getPluginImage(this, "Images|OMBricks-Ball2");
            b.Name = "Ball";
            b.screen = screen;
            b.Angle = 40;
            _Balls.Add(b);
            p.Balls = _Balls;
            ((AngleOverlay)base.PanelManager[screen, "mainPanel"]["angleOverlay"]).Height = b.Top - OM.Host.ClientArea[0].Top;
            ((AngleOverlay)base.PanelManager[screen, "mainPanel"]["angleOverlay"]).Balls = _Balls;
            ((AngleOverlay)base.PanelManager[screen, "mainPanel"]["angleOverlay"]).p = p;
            base.PanelManager[screen, "mainPanel"].addControl(b);
            p.UnPaused -= p_UnPaused; //this is here so we don't continue tacking on multiple unpause events
            p.UnPaused += new Paddle.UnPauseEventHandler(p_UnPaused);
            isRunning[screen] = true;
            gameOver = false;
            ModifyLives(screen, ModifyingLives.SetLives, 3);
            base.PanelManager[screen, "mainPanel"]["gameLivesLabel"].Visible = true;
            SetScore(screen, true);
            base.PanelManager[screen, "mainPanel"]["gameScores"].Visible = true;
            base.PanelManager[screen, "mainPanel"]["angleOverlay"].Visible = true;
            gameStatus[screen] = GameEventsStatus.NextLevel;
            new Thread(new ParameterizedThreadStart(Start)).Start(screen);
        }

        void p_UnPaused(int screen)
        {
            if (isPaused[screen])
                isPaused[screen] = false;
        }

        void b_OnClick(OMControl sender, int screen)
        {
            if (!b.Moving)
            {
                b.Moving = true;
                b.OnClick -= b_OnClick;
            }
        }



        //public static int gameLives = 3;
        public static int toMoveY = 4;
        public static int toMoveX = 4;
        List<Bricks> _Bricks = new List<Bricks>();
        //public static int gameLevel = 1;
        public static bool generateLevel = true;
        DateTime startTime;
        //public static List<PowerUps.PowerUp> _PowerUps = new List<PowerUps.PowerUp>();
        public static bool gameOver = false;
        public static bool lostLife = false;
        public List<Ball> _Balls = new List<Ball>();

        private void ResetGame(int screen)
        {
            p.MouseUp(screen, null, new Point(0, 0), new Point(0, 0));
            moreToHit[screen] = true;
            ClearBalls(screen);
            ClearLasers(screen);
            ClearShots(screen);
            p.Reset();
            _Balls[0].Reset(p);
            base.PanelManager[screen, "mainPanel"].MoveControlToFront(base.PanelManager[screen, "mainPanel"]["angleOverlay"]);
            base.PanelManager[screen, "mainPanel"]["angleOverlay"].Visible = true;
        }

        private void Start(object _screen)
        {
            int screen = Convert.ToInt16(_screen);

            while (isRunning[screen])
            {
                if (!animating[screen])
                {
                    animating[screen] = true;
                    float initialAnimationSpeed = 0.175f;
                    SmoothAnimator Animation = new SmoothAnimator(initialAnimationSpeed);
                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                    {
                        if (!isPaused[screen])
                        {
                            if (gameStatus[screen] == GameEventsStatus.NextLevel)
                            {
                                gameLevel[screen] += 1;
                                if (gameLevel[screen] > LevelGenerator.MaxLevel)
                                {
                                    OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Game Complete!"));
                                    gameStatus[screen] = GameEventsStatus.GameWon;
                                    //remove the ball, paddle, etc..

                                }
                                else
                                {
                                    OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("Level {0}", gameLevel[screen].ToString())));
                                    //_Bricks = new List<Bricks>(); //keep making new ones so as levels increase we won't have to loop all old bricks as well
                                    LevelGenerator.GenerateLevel(this, base.PanelManager, screen, gameLevel[screen], _Bricks);
                                    ResetGame(screen);
                                    gameStatus[screen] = GameEventsStatus.Running;
                                }
                            }
                            if (gameStatus[screen] == GameEventsStatus.LostLife)
                            {
                                ModifyLives(screen, ModifyingLives.SubtractLives);
                                ResetGame(screen);
                                if (gameStatus[screen] != GameEventsStatus.GameOver)
                                    gameStatus[screen] = GameEventsStatus.Running;
                            }
                            if (gameStatus[screen] == GameEventsStatus.GameOver)
                            {
                                EndGame(screen);
                                return false; //stops the animation/main loop, a new game starts the animation/loop again
                            }
                            else if (gameStatus[screen] == GameEventsStatus.GameWon)
                            {
                                //remove ball, paddle, bricks (in case), etc...

                            }
                            else if (moreToHit[screen])
                            {
                                for (int ballCount = 0; ballCount < _Balls.Count; ballCount++)
                                {
                                    if (_Balls[ballCount].Moving)
                                    {
                                        if (!_Balls[ballCount].MoveBall(_Bricks, p, _Balls.Count))
                                        {
                                            //remove that ball
                                            if (_Balls.Count > 1)
                                            {
                                                base.PanelManager[screen, "mainPanel"].Remove(_Balls[ballCount]);
                                                _Balls.RemoveAt(ballCount);
                                                ballCount--;
                                            }
                                        }
                                    }
                                }
                            }
                            //brick logic
                            int BricksLeftToDestroy = 0;
                            for (int i = 0; i < _Bricks.Count; i++)
                            {
                                if ((_Bricks[i].IsHit) && (!_Bricks[i].isDestroying))
                                {
                                    if (_Bricks[i].BrickLevel == 1)
                                    {
                                        //update score
                                        currentScore[screen] += _Bricks[i].Score;
                                        _Bricks[i].isDestroying = true;
                                    }
                                    else
                                    {
                                        _Bricks[i].BrickLevel -= 1;
                                    }
                                    //are there any "more" to "hit"
                                    for (int j = 0; j < _Bricks.Count; j++)
                                    {
                                        if (_Bricks[j].Breakable && !_Bricks[i].IsHit)
                                        {
                                            moreToHit[screen] = false;
                                            break;
                                        }
                                    }
                                    if (_Bricks[i].BrickLevel > 0)
                                        _Bricks[i].IsHit = false;
                                    //take care of any powerups here
                                    switch (_Bricks[i].PowerUp)
                                    {
                                        case PowerUps.PowerUps.Laser:
                                            Laser newLaser = new Laser(_Bricks[i], Laser.LaserDirection.Right, 0);
                                            _Lasers[screen].Add(newLaser);
                                            base.PanelManager[screen, "mainPanel"].addControl(newLaser);
                                            newLaser = new Laser(_Bricks[i], Laser.LaserDirection.Up, 90);
                                            _Lasers[screen].Add(newLaser);
                                            base.PanelManager[screen, "mainPanel"].addControl(newLaser);
                                            newLaser = new Laser(_Bricks[i], Laser.LaserDirection.Left, 180);
                                            _Lasers[screen].Add(newLaser);
                                            base.PanelManager[screen, "mainPanel"].addControl(newLaser);
                                            newLaser = new Laser(_Bricks[i], Laser.LaserDirection.Down, 270);
                                            _Lasers[screen].Add(newLaser);
                                            base.PanelManager[screen, "mainPanel"].addControl(newLaser);
                                            break;
                                        case PowerUps.PowerUps.MultiBall:
                                            Ball newBall = new Ball(p);
                                            newBall.Left = b.Left;
                                            newBall._Left = b._Left;
                                            newBall.Top = b.Top;
                                            newBall._Top = b._Top;
                                            newBall.Width = b.Width;
                                            newBall.Height = b.Height;
                                            newBall.Image = b.Image;
                                            //newBall.dummyBall = true;
                                            newBall.Angle = b.Angle + 45;
                                            newBall.Moving = true;
                                            _Balls.Add(newBall);
                                            base.PanelManager[screen, "mainPanel"].addControl(newBall);
                                            newBall = new Ball(p);
                                            newBall.Left = b.Left;
                                            newBall._Left = b._Left;
                                            newBall.Top = b.Top;
                                            newBall._Top = b._Top;
                                            newBall.Width = b.Width;
                                            newBall.Height = b.Height;
                                            newBall.Image = b.Image;
                                            //newBall.dummyBall = true;
                                            newBall.Angle = b.Angle - 45;
                                            newBall.Moving = true;
                                            _Balls.Add(newBall);
                                            base.PanelManager[screen, "mainPanel"].addControl(newBall);
                                            break;
                                        case PowerUps.PowerUps.Life:
                                            ModifyLives(screen, ModifyingLives.AddLives);
                                            break;
                                        case PowerUps.PowerUps.PaddleShooter:
                                            PaddleShooter ps = new PaddleShooter();
                                            _PaddleShooter[screen].Add(ps);
                                            p.AddShootingOverlayImage();
                                            break;
                                    }
                                    //_Bricks[i].RemovePowerUpOverlayImage(); //maybe this will just fade away with the brick???
                                    _Bricks[i].PowerUp = PowerUps.PowerUps.None;
                                }
                                if (!_Bricks[i].Visible)
                                {
                                    _Bricks.RemoveAt(i);
                                    if (randomLevel[screen])
                                    {
                                        LevelGenerator.GenerateRandom(this, base.PanelManager, screen, _Bricks, _Balls);
                                    }
                                    i--;
                                }
                                else if (_Bricks[i].isDestroying)
                                {
                                    _Bricks[i].UpdateDestroy();
                                    BricksLeftToDestroy += 1;
                                }
                                else if ((_Bricks[i].Visible) && (_Bricks[i].Breakable))
                                {
                                    BricksLeftToDestroy += 1;
                                }
                            }
                            if (BricksLeftToDestroy == 0)
                            {
                                ClearBricks(_Bricks, screen); //makes sure the last brick(s) displayed really is/are cleared out
                                gameStatus[screen] = GameEventsStatus.NextLevel;
                            }
                            else
                            {
                                //loop animating any power ups
                                for (int i = 0; i < _Lasers[screen].Count; i++)
                                {
                                    if (!_Lasers[screen][i].Move(_Bricks))
                                    {
                                        base.PanelManager[screen, "mainPanel"].Remove(_Lasers[screen][i]);
                                        _Lasers[screen].RemoveAt(i);
                                        i--;
                                    }
                                }
                                for (int i = 0; i < _PaddleShooter[screen].Count; i++)
                                {
                                    //test for a shot, if yes, create 4 new shots
                                    if (_PaddleShooter[screen][i].FireShot())
                                    {
                                        PaddleShots pShots = new PaddleShots(p, _Bricks, 90, PaddleShots.ShotSide.Left);
                                        _PaddleShots[screen].Add(pShots);
                                        base.PanelManager[screen, "mainPanel"].addControl(pShots);
                                        pShots = new PaddleShots(p, _Bricks, 90, PaddleShots.ShotSide.LeftCenter);
                                        _PaddleShots[screen].Add(pShots);
                                        base.PanelManager[screen, "mainPanel"].addControl(pShots);
                                        pShots = new PaddleShots(p, _Bricks, 90, PaddleShots.ShotSide.Right);
                                        _PaddleShots[screen].Add(pShots);
                                        base.PanelManager[screen, "mainPanel"].addControl(pShots);
                                        pShots = new PaddleShots(p, _Bricks, 90, PaddleShots.ShotSide.RightCenter);
                                        _PaddleShots[screen].Add(pShots);
                                        base.PanelManager[screen, "mainPanel"].addControl(pShots);
                                    }
                                    if (_PaddleShooter[screen][i].ShotsToFire == 0)
                                    {
                                        _PaddleShooter[screen].RemoveAt(i);
                                        i--;
                                    }
                                }
                                if (_PaddleShooter[screen].Count == 0)
                                {
                                    //remove the paddle shooting image
                                    p.RemoveShootingOverlayImage();
                                }
                                for (int i = 0; i < _PaddleShots[screen].Count; i++)
                                {
                                    if (!_PaddleShots[screen][i].Move(_Bricks))
                                    {
                                        base.PanelManager[screen, "mainPanel"].Remove(_PaddleShots[screen][i]);
                                        _PaddleShots[screen].RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                            //end brick logic
                            SetScore(screen);
                        }
                        return true;
                    });
                }
            }
        }

        private void ClearBricks(List<Bricks> _Bricks, int screen)
        {
            for (int i = 0; i < _Bricks.Count; i++)
            {
                base.PanelManager[screen, "mainPanel"].Remove(_Bricks[i]);
                _Bricks.RemoveAt(i);
                i--;
            }
        }

        private void ClearBalls(int screen)
        {
            for (int i = 1; i < _Balls.Count; i++)
            {
                base.PanelManager[screen, "mainPanel"].Remove(_Balls[i]);
                _Balls.RemoveAt(i);
                i--;
            }
        }

        private void ClearLasers(int screen)
        {
            for (int i = 0; i < _Lasers[screen].Count; i++)
            {
                base.PanelManager[screen, "mainPanel"].Remove(_Lasers[screen][i]);
                _Lasers[screen].RemoveAt(i);
                i--;
            }
        }

        private void ClearShots(int screen)
        {
            for (int i = 0; i < _PaddleShooter[screen].Count; i++)
            {
                _PaddleShooter[screen].RemoveAt(i);
                i--;
            }
            for (int i = 0; i < _PaddleShots[screen].Count; i++)
            {
                base.PanelManager[screen, "mainPanel"].Remove(_PaddleShots[screen][i]);
                _PaddleShots[screen].RemoveAt(i);
                i--;
            }

        }

    }
}
