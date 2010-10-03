using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.Threading;

namespace Slideshow
{
    public sealed class Slideshow:IHighLevel
    {

        #region IHighLevel Members

        public OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            if (string.IsNullOrEmpty(name))
                return null;
            SafeThread.Asynchronous(delegate()
            {
                showEm(name, screen);
            }, theHost);
            return manager[screen];
        }
        ScreenManager manager;
        private void showEm(string path,int screen)
        {
            List<string> allImages = getImages(path);
            int pos=0;
            while (manager[screen].Mode != eModeType.Highlighted)
            {
                transition(allImages[pos], pos % 2,screen);
                Thread.Sleep(3000);
                pos++;
                if (pos == allImages.Count)
                    pos = 0;
            }
        }
        private void transition(string image,int num,int screen)
        {
            int transitionType = OpenMobile.Framework.Math.Calculation.RandomNumber(0, 4);
            ((OMButton)manager[screen][num]).Image = new imageItem(OImage.FromFile(image));
            for (int i = 1; i <= 10; i++)
            {
                switch(transitionType)
                {
                    case 0:
                        if (num==1)
                            ((OMButton)manager[screen][1]).Transparency = (byte)(i * 10);
                        else
                            ((OMButton)manager[screen][1]).Transparency = (byte)(100 - (i * 10));
                        break;
                    case 1:
                        if (num == 1)
                            manager[screen][1].Top = ((i * 60) - 600);
                        else
                            manager[screen][1].Top = ((i * 60));
                        break;
                    case 2:
                        if (num == 1)
                            manager[screen][1].Top = (600-(i * 60));
                        else
                            manager[screen][1].Top = (-(i * 60));
                        break;
                    case 3:
                        if (num == 1)
                            manager[screen][1].Left = (1000-(i*100));
                        else
                            manager[screen][1].Left= (-(i * 100));
                        break;
                    case 4:
                    default:
                        if (num == 1)
                            manager[screen][1].Left = ((i * 100) - 1000);
                        else
                            manager[screen][1].Left = ((i * 100));
                        break;
                }
                Thread.Sleep(50);
            }
        }
        private List<string> getImages(string path)
        {
            List<string> playlist = new List<string>();
            string[] images = Directory.GetFiles(path, "*.png");
            playlist.AddRange(images);
            images = Directory.GetFiles(path, "*.jpg");
            playlist.AddRange(images);
            images = Directory.GetFiles(path, "*.bmp");
            playlist.AddRange(images);
            images = Directory.GetFiles(path, "*.gif");
            playlist.AddRange(images);

            foreach (string dir in Directory.GetDirectories(path))
                playlist.AddRange(getImages(dir));
            return playlist;
        }
        public string displayName
        {
            get { return "Slideshow"; }
        }

        #endregion

        #region IBasePlugin Members
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel("Slideshow");
            p.Priority = ePriority.High;
            OMButton first = new OMButton(0, 0, 1000, 600);
            first.OnClick += new userInteraction(button_OnClick);
            p.addControl(first);
            OMButton second = new OMButton(0, 0, 1000, 600);
            second.OnClick += new userInteraction(button_OnClick);
            p.addControl(second);
            manager.loadPanel(p);
            return eLoadStatus.LoadSuccessful;
        }

        void button_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString(),"None");
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { throw new NotImplementedException(); }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "Slideshow"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { throw new NotImplementedException(); }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
