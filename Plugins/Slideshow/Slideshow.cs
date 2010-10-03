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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions;
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
            if (theHost == null)
                return null;
            if (string.IsNullOrEmpty(name))
            {
                General.getFilePath path = new General.getFilePath(theHost);
                string url = path.getFolder(screen, "MainMenu", "");
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Slideshow", url);
                //theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                return null;
            }
            SafeThread.Asynchronous(delegate()
            {
                showEm(name, screen);
            }, theHost);
            manager[screen][0].Tag = name;
            manager[screen][1].Tag = name;
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
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel("Slideshow");
            p.Priority = ePriority.High;
            OMButton first = new OMButton(0, 0, 1000, 600);
            first.OnClick += new userInteraction(button_OnClick);
            p.addControl(first);
            OMButton second = new OMButton(0, 0, 1000, 600);
            second.OnClick += new userInteraction(button_OnClick);
            p.addControl(second);
            p.Forgotten = true;
            p.BackgroundColor1 = Color.White;
            p.BackgroundType = backgroundStyle.SolidColor;
            manager.loadPanel(p);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.gesture)
            {
                switch (arg2)
                {
                    case "back":
                        //TODO
                        break;
                    case " ":
                        //TODO
                        break;
                }
            }
        }

        void button_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(),"Slideshow",sender.Tag.ToString());
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
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
            get { return "Image Slideshow"; }
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
