using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenMobile.Graphics;
using System.Threading;
using OpenMobile.Data;

namespace ControlDemo
{
    public class NewMedia : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        static string dbname;
        public eLoadStatus initialize(OpenMobile.Plugin.IPluginHost host)
        {
            theHost = host;
            if (host.InstanceCount == -1)
                return eLoadStatus.LoadFailedRetryRequested;
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel("Media");
            imageItem opt1 = theHost.getSkinImage("DownTab");
            OMImage Background = new OMImage(150, 140, 700, 400);
            Background.Image = theHost.getSkinImage("Center");
            OMImage LeftTab = new OMImage(-18, 155, 225, 150);
            LeftTab.Image = theHost.getSkinImage("LeftSlidingSelect");
            OMButton Artists = new OMButton(24, 168, 130, 130);
            Artists.Image = theHost.getSkinImage("ArtistIcon_Selected");
            Artists.Transition = eButtonTransition.None;
            Artists.OnClick += new userInteraction(Artists_OnClick);
            OMButton Albums = new OMButton(29, 265, 130, 130);
            Albums.Image = theHost.getSkinImage("AlbumIcon");
            Albums.Transition = eButtonTransition.None;
            Albums.OnClick += new userInteraction(Albums_OnClick);
            OMButton Tracks = new OMButton(33, 370, 130, 130);
            Tracks.Transition = eButtonTransition.None;
            Tracks.Image = theHost.getSkinImage("TracksIcon");
            Tracks.OnClick += new userInteraction(Tracks_OnClick);
            OMButton Down = new OMButton(5, 450, 190, 120);
            Down.Image = theHost.getSkinImage("DownArrowIcon");
            Down.DownImage = theHost.getSkinImage("DownArrowIcon_Selected");
            Down.Transition = eButtonTransition.None;
            OMLabel Title = new OMLabel(250, 111, 500, 50);
            Title.Font = new Font(Font.GenericSansSerif, 26F);
            Title.Text = "Example Artist";
            OMButton PlaySelected = new OMButton(830, 165, 150, 150);
            PlaySelected.Image = theHost.getSkinImage("PlayIcon");
            PlaySelected.DownImage = theHost.getSkinImage("PlayIcon_Selected");
            PlaySelected.Transition = eButtonTransition.None;
            OMButton Enqueue = new OMButton(830, 265, 150, 150);
            Enqueue.Image = theHost.getSkinImage("EnqueIcon");
            Enqueue.DownImage = theHost.getSkinImage("EnqueIcon_Selected");
            Enqueue.Transition = eButtonTransition.None;
            OMImage RightTab = new OMImage(788, 375, 225, 150);
            RightTab.Image = theHost.getSkinImage("RightSlidingSelect");
            RightTab.Visible = false;
            OMButton Playlists = new OMButton(830, 375, 150, 150);
            Playlists.Transition = eButtonTransition.None;
            Playlists.Image = theHost.getSkinImage("PlaylistIcon");
            Playlists.FocusImage = theHost.getSkinImage("Playlists_Selected");
            Playlists.OnClick += new userInteraction(Playlists_OnClick);
            OMButton Zones = new OMButton(817, 100, 175, 75);
            Zones.Image = opt1;
            Zones.Text = "Zone 1";
            Zones.Format = eTextFormat.BoldShadow;
            Zones.Font = new Font(Font.ComicSansMS, 22);
            Zones.Transition = eButtonTransition.None;
            OMButton Sources = new OMButton(4, 100, 175, 75);
            Sources.Transition = eButtonTransition.None;
            Sources.Image = opt1;
            Sources.Font = Zones.Font;
            Sources.Text = " Source:";
            Sources.TextAlignment = Alignment.CenterLeft;
            OMImage Source = new OMImage(105, 85, 75, 100);
            Source.Image = theHost.getSkinImage("IpodIcon");
            p.addControl(Background);
            p.addControl(LeftTab);
            p.addControl(Artists);
            p.addControl(Albums);
            p.addControl(Tracks);
            p.addControl(Down);
            p.addControl(Title);
            p.addControl(PlaySelected);
            p.addControl(Enqueue);
            p.addControl(RightTab);
            p.addControl(Playlists);
            p.addControl(Sources);
            p.addControl(Zones);
            p.addControl(Source);
            manager.loadPanel(p);
            using (PluginSettings ps = new PluginSettings())
                dbname = ps.getSetting("Default.MusicDatabase");
            OpenMobile.Threading.TaskManager.QueueTask(loadArtists, ePriority.High, "Load Artists");
            return eLoadStatus.LoadSuccessful;
        }

        private void loadArtists()
        {
            //STUB
        }

        void Playlists_OnClick(OMControl sender, int screen)
        {
            ((OMButton)sender).Image = theHost.getSkinImage("PlaylistIcon_Selected");
            ((OMButton)sender.Parent[2]).Image = theHost.getSkinImage("ArtistIcon");
            ((OMButton)sender.Parent[3]).Image = theHost.getSkinImage("AlbumIcon");
            ((OMButton)sender.Parent[4]).Image = theHost.getSkinImage("TracksIcon");
            sender.Parent[9].Visible = true;
            sender.Parent[1].Visible = false;
        }

        void Tracks_OnClick(OMControl sender, int screen)
        {
            sender.Parent[9].Visible = false;
            ((OMButton)sender).Image = theHost.getSkinImage("TracksIcon_Selected");
            int diff = (360-sender.Parent[1].Top) / 5;
            for (int i = 1; i < 5; i++)
            {
                sender.Parent[1].Top += diff;
                Thread.Sleep(20);
            }
            sender.Parent[1].Top = 360;
            sender.Parent[1].Visible = true;
            ((OMButton)sender.Parent[2]).Image = theHost.getSkinImage("ArtistIcon");
            ((OMButton)sender.Parent[3]).Image = theHost.getSkinImage("AlbumIcon");
            ((OMButton)sender.Parent[10]).Image = theHost.getSkinImage("PlaylistIcon");
        }

        void Albums_OnClick(OMControl sender, int screen)
        {
            sender.Parent[9].Visible = false;
            ((OMButton)sender).Image = theHost.getSkinImage("AlbumIcon_Selected");
            int diff = (255 - sender.Parent[1].Top) / 5;
            for (int i = 1; i < 5; i++)
            {
                sender.Parent[1].Top += diff;
                Thread.Sleep(20);
            }
            sender.Parent[1].Top = 255;
            sender.Parent[1].Visible = true;
            ((OMButton)sender.Parent[2]).Image = theHost.getSkinImage("ArtistIcon");
            ((OMButton)sender.Parent[4]).Image = theHost.getSkinImage("TracksIcon");
            ((OMButton)sender.Parent[10]).Image = theHost.getSkinImage("PlaylistIcon");
        }

        void Artists_OnClick(OMControl sender, int screen)
        {
            sender.Parent[9].Visible = false;
            ((OMButton)sender).Image = theHost.getSkinImage("ArtistIcon_Selected");
            int diff = (155 - sender.Parent[1].Top) / 5;
            for (int i = 1; i < 5; i++)
            {
                sender.Parent[1].Top += diff;
                Thread.Sleep(20);
            }
            sender.Parent[1].Top = 155;
            sender.Parent[1].Visible = true;
            ((OMButton)sender.Parent[3]).Image = theHost.getSkinImage("AlbumIcon");
            ((OMButton)sender.Parent[4]).Image = theHost.getSkinImage("TracksIcon");
            ((OMButton)sender.Parent[10]).Image = theHost.getSkinImage("PlaylistIcon");
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