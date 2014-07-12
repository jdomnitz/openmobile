using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OMBricks.PowerUps;
using OpenMobile.Framework;
using OpenMobile.Graphics;

namespace OMBricks
{
    public class Bricks : OMImage
    {

        public bool isDestroying = false;
        public int Score = 0;


        private int _BrickLevel;
        private bool _Breakable;
        private bool _IsHit = false;
        private bool _Split = false;
        private PowerUps.PowerUps _PowerUp;

        private OImage imgPowerUp_Laser;
        private OImage imgPowerUp_Life;
        private OImage imgPowerUp_MultiBall;

        public Bricks()
        {
            Width = 75;
            Height = 25;
            imgPowerUp_Laser = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_Lasers").image;
            imgPowerUp_Life = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_XLife").image;
            imgPowerUp_MultiBall = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_MBall").image;
        }

        public PowerUps.PowerUps PowerUp
        {
            get
            {
                return _PowerUp;
            }
            set
            {
                _PowerUp = value;

                // Error handling
                if (Image == null || Image.image == null)
                    return;

                switch (_PowerUp)
                {
                    case PowerUps.PowerUps.Laser:
                        //Image = new imageItem(Image.image.Copy().AddImageOverlay(0, 0, OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_Lasers").image));
                        //Image = new imageItem(Image.image.Copy().AddImageOverlay(0, 0, OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_Lasers").image));
                        Image = new imageItem(Image.image.Copy().AddImageOverlay(0, 0, imgPowerUp_Laser));
                        break;
                    case PowerUps.PowerUps.Life:
                        //Image = new imageItem(Image.Copy().image.AddImageOverlay(0, 0, OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_XLife").image));
                        Image = new imageItem(Image.image.Copy().AddImageOverlay(0, 0, imgPowerUp_Life));
                        break;
                    case PowerUps.PowerUps.MultiBall:
                        //Image = new imageItem(Image.Copy().image.AddImageOverlay(0, 0, OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_OL_MBall").image));
                        Image = new imageItem(Image.image.Copy().AddImageOverlay(0, 0, imgPowerUp_MultiBall));

                        break;
                }
            }
        }

        public void RemovePowerUpOverlayImage()
        {

        }

        public bool IsHit
        {
            get
            {
                return _IsHit;
            }
            set
            {
                _IsHit = value;
            }
        }

        public bool Split
        {
            get
            {
                return _Split;
            }
            set
            {
                _Split = value;
            }
        }
        
        public int BrickLevel
        {
            get
            {
                return _BrickLevel;
            }
            set
            {
                _BrickLevel = value;
                SetImage();
            }
        }

        public void Hit(Ball b)
        {
            IsHit = true;
            b.Speed += 0.45F;
        }

        private void SetScore(int newScore)
        {
            if (Score == 0)
                Score = newScore;
        }

        private void SetImage()
        {
            if (!Breakable)
            {
                Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_Gray7");
            }
            else
            {
                if (BrickLevel < 1)
                {
                    BrickLevel = 1;
                }
                switch (BrickLevel)
                {
                    case 1:
                        Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_Yellow");
                        SetScore(10);
                        break;
                    case 2:
                        Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_Blue");
                        SetScore(20);
                        break;
                    case 3:
                        Image = OM.Host.getPluginImage("OMBricks", "Images|OMBricks-Bricks_Red");
                        SetScore(30);
                        break;
                }
            }
        }

        
        public bool Breakable
        {
            get
            {
                return _Breakable;
            }
            set
            {
                _Breakable = value;
                SetImage();
            }
        }

        
        public delegate void BrickDestroyedEventHandler(Bricks Brick, int screen);
        public event BrickDestroyedEventHandler BricksDestroyed;
        
        public int Destroyed(int screen, Ball b, Paddle p, List<Bricks> _Bricks)
        {
            if (BrickLevel == 1)
            {
                isDestroying = true;
            }
            else
            {
                BrickLevel -= 1;
            }
            return Score;
        }

        public void UpdateDestroy()
        {
            Opacity -= 10;
            if(Opacity <= 0)
                Visible = false;
        }

    }
}
