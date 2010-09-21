using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenMobile.Graphics;

namespace ControlDemo
{
    public class NewMedia : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;

        public eLoadStatus initialize(OpenMobile.Plugin.IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel("Media");
            imageItem opt1 = theHost.getSkinImage("DownTab");
            OMImage Image1 = new OMImage(150, 140, 700, 400);
            Image1.Image = theHost.getSkinImage("Center");
            OMImage Image2 = new OMImage(-18, 154, 225, 150);
            Image2.Image = theHost.getSkinImage("LeftSlidingSelect");
            OMButton Image3 = new OMButton(24, 168, 130, 130);
            Image3.Image = theHost.getSkinImage("ArtistIcon_Selected");
            Image3.Transition = eButtonTransition.None;
            OMButton Image4 = new OMButton(29, 281, 130, 130);
            Image4.Image = theHost.getSkinImage("AlbumIcon");
            Image4.Transition = eButtonTransition.None;
            OMButton Image5 = new OMButton(33, 375, 130, 130);
            Image5.Transition = eButtonTransition.None;
            Image5.Image = theHost.getSkinImage("TracksIcon");
            OMButton Image6 = new OMButton(28, 458, 140, 120);
            Image6.Image = theHost.getSkinImage("DownArrowIcon");
            Image6.DownImage = theHost.getSkinImage("DownArrowIcon_Selected");
            Image6.Transition = eButtonTransition.None;
            OMLabel Label8 = new OMLabel(250, 111, 500, 50);
            Label8.Font = new Font(Font.GenericSansSerif, 26F);
            Label8.Text = "Example Artist";
            OMButton Image9 = new OMButton(830, 145, 150, 150);
            Image9.Image = theHost.getSkinImage("PlayIcon");
            Image9.FocusImage = theHost.getSkinImage("PlayIcon_Selected");
            Image9.Transition = eButtonTransition.None;
            OMButton Image10 = new OMButton(829, 254, 150, 150);
            Image10.Image = theHost.getSkinImage("EnqueIcon");
            Image10.FocusImage = theHost.getSkinImage("EnqueIcon_Selected");
            Image10.Transition = eButtonTransition.None;
            OMImage Image13 = new OMImage(789, 375, 225, 150);
            Image13.Image = theHost.getSkinImage("RightSlidingSelect");
            OMButton Image11 = new OMButton(819, 376, 150, 150);
            Image11.Transition = eButtonTransition.None;
            Image11.Image = theHost.getSkinImage("PlaylistIcon_Selected");
            OMImage Image14 = new OMImage(817, 100, 175, 75);
            Image14.Image = opt1;
            Image14.Left = 4;
            p.addControl(Image1);
            p.addControl(Image2);
            p.addControl(Image3);
            p.addControl(Image4);
            p.addControl(Image5);
            p.addControl(Image6);
            p.addControl(Label8);
            p.addControl(Image9);
            p.addControl(Image10);
            p.addControl(Image13);
            p.addControl(Image11);
            p.addControl(Image14);
            manager.loadPanel(p);
            return eLoadStatus.LoadSuccessful;
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen];
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
            get { return "NewMedia"; }
        }
        public string displayName
        {
            get { return "NewMedia"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "New Media Skin"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        public void Dispose()
        {
            if (manager != null)
            {
                manager.Dispose();
            }
        }
    }
}