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
using OpenMobile.Threading;
using OpenMobile.Media;
using System.Collections.Generic;

namespace ControlDemo
{
    public class NewMedia : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        static string dbname;
        int level=0;
        OMListItem.subItemFormat format = new OMListItem.subItemFormat();
        public eLoadStatus initialize(OpenMobile.Plugin.IPluginHost host)
        {
            theHost = host;
            if (host.InstanceCount == -1)
                return eLoadStatus.LoadFailedRetryRequested;
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel("Media");
            imageItem opt1 = theHost.getSkinImage("DownTab");
            OMImage Background = new OMImage(150, 140, 700, 400);
            Background.Image = theHost.getSkinImage("Center");
            OMList List = new OMList(180, 163, 644, 354);
            List.HighlightColor = Color.White;
            List.ListStyle = eListStyle.MultiList;
            List.SelectedItemColor1 = Color.Black;
            List.ItemColor1 = Color.Transparent;
            List.Color = Color.Black;
            List.Font = new Font(Font.GenericSansSerif, 28F);
            List.OnClick += new userInteraction(List_OnClick);
            List.OnLongClick += new userInteraction(List_OnLongClick);
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
            Title.Text = "Select an Artist";
            OMButton PlaySelected = new OMButton(830, 165, 150, 150);
            PlaySelected.Image = theHost.getSkinImage("PlayIcon");
            PlaySelected.DownImage = theHost.getSkinImage("PlayIcon_Selected");
            PlaySelected.OnClick += new userInteraction(PlaySelected_OnClick);
            PlaySelected.Transition = eButtonTransition.None;
            OMButton Enqueue = new OMButton(830, 265, 150, 150);
            Enqueue.Image = theHost.getSkinImage("EnqueIcon");
            Enqueue.DownImage = theHost.getSkinImage("EnqueIcon_Selected");
            Enqueue.Transition = eButtonTransition.None;
            Enqueue.OnClick += new userInteraction(Enqueue_OnClick);
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
            Zones.Format = eTextFormat.BoldShadow;
            Zones.Font = new Font(Font.ComicSansMS, 22);
            Zones.Transition = eButtonTransition.None;
            OMButton Sources = new OMButton(4, 100, 175, 75);
            Sources.Transition = eButtonTransition.None;
            Sources.Image = opt1;
            Sources.Font = Zones.Font;
            Sources.Text = " Source:";
            Sources.TextAlignment = Alignment.CenterLeft;
            Sources.OnClick += new userInteraction(Sources_OnClick);
            OMButton Source = new OMButton(105, 85, 75, 100);
            Source.Image = theHost.getSkinImage("Local Drive");
            Source.OnClick += new userInteraction(Sources_OnClick);
            Source.Transition = eButtonTransition.None;
            OMBasicShape shapeBar = new OMBasicShape(4, 97, 173, 0);
            shapeBar.FillColor = Color.Black;
            shapeBar.Shape = shapes.Rectangle;
            shapeBar.BorderColor = Color.Gray;
            shapeBar.BorderSize = 2F;
            Font f = new Font(Font.GenericSansSerif, 17F);
            OMButton source1 = new OMButton(37, 75, 100, 100);
            source1.Visible = false;
            source1.OnClick += new userInteraction(source_OnClick);
            OMLabel label1 = new OMLabel(2, 150, 175, 50);
            label1.Visible = false;
            label1.Font = f;
            OMButton source2 = new OMButton(37, 175, 100, 100);
            source2.Visible = false;
            source2.OnClick += new userInteraction(source_OnClick);
            OMLabel label2 = new OMLabel(2, 250, 175, 50);
            label2.Visible = false;
            label2.Font = f;
            OMButton source3 = new OMButton(37, 275, 100, 100);
            source3.Visible = false;
            source3.OnClick += new userInteraction(source_OnClick);
            OMLabel label3 = new OMLabel(2, 350, 175, 50);
            label3.Visible = false;
            label3.Font = f;
            OMButton source4 = new OMButton(37, 375, 100, 100);
            source4.Visible = false;
            source4.OnClick += new userInteraction(source_OnClick);
            OMLabel label4 = new OMLabel(2, 450, 175, 50);
            label4.Visible = false;
            label4.Font = f;
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
            p.addControl(Playlists);//10
            p.addControl(shapeBar);
            p.addControl(Zones);
            p.addControl(Sources);
            p.addControl(List);
            p.addControl(Source);
            p.addControl(source1);
            p.addControl(source2);
            p.addControl(source3);
            p.addControl(source4);
            p.addControl(label1);
            p.addControl(label2);
            p.addControl(label3);
            p.addControl(label4);
            manager.loadPanel(p);
            for (int i = 0; i < theHost.ScreenCount; i++)
                ((OMButton)manager[i][12]).Text = "Zone " + (theHost.instanceForScreen(i) + 1).ToString();
            using (PluginSettings ps = new PluginSettings())
                dbname = ps.getSetting("Default.MusicDatabase");
            SafeThread.Asynchronous(delegate() { theHost_OnStorageEvent(eMediaType.NotSet, true, null); }, theHost);
            OpenMobile.Threading.TaskManager.QueueTask(loadArtists, ePriority.High, "Load Artists");
            format.color = Color.FromArgb(175,Color.Black);
            format.highlightColor = Color.LightGray;
            format.font = new Font(Font.GenericSansSerif, 20F);
            return eLoadStatus.LoadSuccessful;
        }

        void source_OnClick(OMControl sender, int screen)
        {
            ((OMButton)sender.Parent[15]).Image = ((OMButton)sender).Image;
            currentSource = (DeviceInfo)sender.Tag;
            Sources_OnClick(sender, screen);
        }

        void Sources_OnClick(OMControl sender, int screen)
        {
            if (sender.Parent[13].Top == 100)
            {
                sender.Parent[13].Top = 100 + (100 * sources.Count);
                sender.Parent[15].Top = 85 + (100 * sources.Count);
                sender.Parent[11].Height = 6 + (100 * sources.Count);
                for (int i = 0; i < sources.Count; i++)
                    sender.Parent[16 + i].Visible = true;
                for (int i = 0; i < sources.Count; i++)
                    sender.Parent[20 + i].Visible = true;
            }
            else
            {
                sender.Parent[13].Top = 100;
                sender.Parent[15].Top = 85;
                sender.Parent[11].Height=0;
                for (int i = 0; i < sources.Count; i++)
                    sender.Parent[16 + i].Visible = false;
                for (int i = 0; i < sources.Count; i++)
                    sender.Parent[20 + i].Visible = false;
            }
        }

        DeviceInfo currentSource=null;
        List<DeviceInfo> sources = new List<DeviceInfo>();
        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (justInserted)
            {
                List<DeviceInfo> temp = new List<DeviceInfo>();
                int source=0;
                foreach (string drive in Environment.GetLogicalDrives())
                {
                    DeviceInfo info = DeviceInfo.getDeviceInfo(drive);
                    switch (info.DriveType)
                    {
                        case OSSpecific.eDriveType.CDRom:
                            ((OMButton)manager[0][source + 16]).Image = theHost.getSkinImage("CD-ROM Drive");
                            break;
                        case OSSpecific.eDriveType.Fixed:
                        case OSSpecific.eDriveType.Network:
                        case OSSpecific.eDriveType.Unknown:
                            ((OMButton)manager[0][source + 16]).Image = theHost.getSkinImage("Local Drive");
                            break;
                        case OSSpecific.eDriveType.Removable:
                        case OSSpecific.eDriveType.Phone: //TODO - separate icon
                            ((OMButton)manager[0][source + 16]).Image = theHost.getSkinImage("Removable Drive");
                            break;
                    }
                    ((OMButton)manager[0][source + 16]).Tag = info;
                    ((OMLabel)manager[0][source + 20]).Text = OSSpecific.getVolumeLabel(drive);
                    source++;
                    temp.Add(info);
                }
                sources = temp; //do this instead of clearing to ensure the list is never empty
            }
        }

        void Enqueue_OnClick(OMControl sender, int screen)
        {
            OMList l = (OMList)manager[screen][14];
            if (level == 2)
            {
                if (l.SelectedIndex >= 0)
                {
                    theHost.appendPlaylist(new List<mediaInfo>(){new mediaInfo(l.SelectedItem.tag.ToString())},theHost.instanceForScreen(screen));
                }
                else
                {
                    if (l.Count == 0)
                        return;
                    List<string> queue = new List<string>();
                    for (int i = 0; i < l.Count; i++)
                        queue.Add(l[i].tag.ToString());
                    theHost.appendPlaylist(Playlist.Convert(queue), theHost.instanceForScreen(screen));
                }
            }
            else if (level == 1)
            {
                if (l.SelectedIndex < 0)
                {
                    if (l.Count == 0)
                        return;
                    List<string> ret = getSongs(l[0].subItem, l[0].text);
                    for (int i = 1; i < l.Count; i++)
                        ret.AddRange(getSongs(l[i].subItem, l[i].text));
                    if (ret.Count > 0)
                        theHost.appendPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                }
                else
                {
                    List<string> ret = getSongs(l[l.SelectedIndex].subItem, l[l.SelectedIndex].text);
                    if (ret.Count > 0)
                        theHost.appendPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                }
            }
        }

        void PlaySelected_OnClick(OMControl sender, int screen)
        {
            OMList l = (OMList)manager[screen][14];
            if (level == 2)
            {
                if (l.SelectedIndex >= 0)
                {
                    theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l.SelectedItem.tag.ToString());
                    theHost.setPlaylist(new List<mediaInfo>() { new mediaInfo(l.SelectedItem.tag.ToString()) }, theHost.instanceForScreen(screen));
                }
                else
                {
                    if (l.Count == 0)
                        return;
                    theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l[0].tag.ToString());
                    List<string> queue = new List<string>();
                    for (int i = 0; i < l.Count; i++)
                        queue.Add(l[i].tag.ToString());
                    theHost.setPlaylist(Playlist.Convert(queue), theHost.instanceForScreen(screen));
                }
            }
            else if (level == 1)
            {
                if (l.SelectedIndex < 0)
                {
                    if (l.Count == 0)
                        return;
                    List<string> ret = getSongs(l[0].subItem, l[0].text);
                    if (ret.Count > 0)
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[0]);
                    for (int i = 1; i < l.Count; i++)
                        ret.AddRange(getSongs(l[i].subItem, l[i].text));
                    if (ret.Count > 0)
                        theHost.setPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                }
                else
                {
                    List<string> ret = getSongs(l[l.SelectedIndex].subItem, l[l.SelectedIndex].text);
                    if (ret.Count > 0)
                    {
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[0]);
                        theHost.setPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                    }
                }
            }
        }
        private List<string> getSongs(string artist, string album)
        {
            object o;
            theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return new List<string>();
            IMediaDatabase db = (IMediaDatabase)o;
            db.beginGetSongsByAlbum(artist, album, false, eMediaField.Title);
            List<string> ret = new List<string>();
            mediaInfo info = db.getNextMedia();
            while (info != null)
            {
                ret.Add(info.Location);
                info = db.getNextMedia();
            }
            db.endSearch();
            return ret;
        }
        string currentArtist;
        string currentAlbum;
        void List_OnClick(OMControl sender, int screen)
        {
            OMList l=(OMList)sender;
            if (level == 0)
            {
                currentArtist = l.SelectedItem.text;
                currentAlbum = null;
                ((OMLabel)l.Parent[6]).Text = currentArtist + " Albums";
                SafeThread.Asynchronous(delegate() { showAlbums(screen, currentArtist); },theHost);
                moveToAlbums(screen);
            }
            else if (level == 1)
            {
                currentAlbum = l.SelectedItem.text;
                ((OMLabel)l.Parent[6]).Text = currentAlbum + " Tracks";
                SafeThread.Asynchronous(delegate() { showTracks(screen, l.SelectedItem.subItem, currentAlbum); }, theHost);
                moveToTracks(screen);
            }
            else if (level == 3)
            {
                ((OMLabel)l.Parent[6]).Text = l.SelectedItem.text + " Tracks";
                SafeThread.Asynchronous(delegate() { showPlaylist(screen, l.SelectedItem.tag.ToString()); }, theHost);
                moveToTracks(screen);
            }
        }
        void List_OnLongClick(OMControl sender, int screen)
        {
            OMList l = (OMList)sender;
            if ((l.SelectedItem != null) && (l.SelectedItem.text == "Current Playlist"))
            {
                OpenMobile.helperFunctions.General.getKeyboardInput input = new OpenMobile.helperFunctions.General.getKeyboardInput(theHost);
                string title = input.getText(screen, "NewMedia");
                if (title != null)
                {
                    Playlist.writePlaylistToDB(theHost, title, theHost.getPlaylist(theHost.instanceForScreen(screen)));
                    ((OMLabel)sender.Parent[6]).Text = title + " Tracks";
                    SafeThread.Asynchronous(delegate() { showPlaylist(screen, title); }, theHost);
                    moveToTracks(screen);
                }
            }
            else if (l.SelectedItem != null)
            {
                Playlist.deletePlaylistFromDB(theHost, l.SelectedItem.text);
                SafeThread.Asynchronous(delegate() { showPlaylists(screen); }, theHost);
            }
        }


        List<string> Artists = new List<string>();
        private void loadArtists()
        {
            object o;
            theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetArtists(false);
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    Artists.Add(info.Artist);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            for (int i = 0; i < theHost.ScreenCount; i++)
                showArtists(i);
        }
        private void showPlaylists(int screen)
        {
            level = 3;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.ListItemOffset = 0;
            l.ListItemHeight = 0;
            l.Add(new OMListItem("Current Playlist",null,"Current Playlist"));
            foreach(string playlist in Playlist.listPlaylistsFromDB(theHost))
            {
                if ((playlist.Length != 8) && (!playlist.StartsWith("Current")))
                    l.Add(new OMListItem(playlist, null, playlist));
            }
            if (currentSource == null)
                foreach (DeviceInfo info in sources)
                    loadPlaylists(info, l);
            else
                loadPlaylists(currentSource, l);
        }
        private void loadPlaylists(DeviceInfo info,OMList l)
        {
            foreach (string playlistPath in info.PlaylistFolders)
                foreach (string playlist in Playlist.listPlaylists(playlistPath))
                    l.Add(new OMListItem(Path.GetFileNameWithoutExtension(playlist), null, playlist));
        }
        private void showPlaylist(int screen, string path)
        {
            level = 2;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.ListItemOffset = 80;
            if (path == "Current Playlist")
            {
                foreach (mediaInfo info in theHost.getPlaylist(theHost.instanceForScreen(screen)))
                {
                    mediaInfo tmp = info; // <-stupid .Net limitation
                    if (tmp.Name == null)
                        tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                    if (tmp == null)
                        continue;
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost);
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getFolderImage(tmp.Location);
                    l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                }
            }
            else if (System.IO.Path.IsPathRooted(path))
            {
                foreach (mediaInfo info in Playlist.readPlaylist(path))
                {
                    mediaInfo tmp = info; // <-stupid .Net limitation
                    if (tmp.Name == null)
                        tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                    if (tmp == null)
                        continue;
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost);
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getFolderImage(tmp.Location);
                    l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                }
            }
            else
            {
                foreach (mediaInfo info in Playlist.readPlaylistFromDB(theHost, path))
                {
                    mediaInfo tmp = info; // <-stupid .Net limitation
                    if (tmp.Name == null)
                        tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                    if (tmp == null)
                        continue;
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost);
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getFolderImage(tmp.Location);
                    l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                }
            }
        }
        private void showArtists(int screen)
        {
            level = 0;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.ListItemOffset = 0;
            l.ListItemHeight = 0;
            l.AddRange(Artists);
        }
        private void showAlbums(int screen)
        {
            level = 1;
            //TODO
        }
        private void showAlbums(int screen, string artist)
        {
            level = 1;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.Add("Loading . . .");
            l.ListItemOffset = 80;
            MediaLoader.loadAlbums(theHost, artist,l, format);
        }
        private void showTracks(int screen)
        {
            level = 2;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.ListItemOffset = 80;
            foreach(string artist in Artists)
                MediaLoader.loadSongs(theHost,artist, l,format,false);
            l.Sort();
        }
        private void showTracks(int screen, string artist)
        {
            level = 2;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.Add("Loading . . .");
            l.ListItemOffset = 80;
            MediaLoader.loadSongs(theHost,artist, l,format);
        }
        private void showTracks(int screen, string artist, string album)
        {
            level = 2;
            OMList l = (OMList)manager[screen][14];
            l.Clear();
            l.Add("Loading . . .");
            l.ListItemOffset = 80;
            MediaLoader.loadSongs(theHost, artist,album,l,format);
        }

        void Playlists_OnClick(OMControl sender, int screen)
        {
            ((OMLabel)sender.Parent[6]).Text = "Select a Playlist";
            SafeThread.Asynchronous(delegate() { showPlaylists(screen); }, theHost);
            ((OMButton)sender).Image = theHost.getSkinImage("PlaylistIcon_Selected");
            ((OMButton)sender.Parent[2]).Image = theHost.getSkinImage("ArtistIcon");
            ((OMButton)sender.Parent[3]).Image = theHost.getSkinImage("AlbumIcon");
            ((OMButton)sender.Parent[4]).Image = theHost.getSkinImage("TracksIcon");
            sender.Parent[9].Visible = true;
            sender.Parent[1].Visible = false;
        }
        void moveToTracks(int screen)
        {
            lock (this)
            {
                manager[screen][9].Visible = false;
                ((OMButton)manager[screen][4]).Image = theHost.getSkinImage("TracksIcon_Selected");
                int diff = (360 - manager[screen][1].Top) / 5;
                for (int i = 1; i < 5; i++)
                {
                    manager[screen][1].Top += diff;
                    Thread.Sleep(20);
                }
                manager[screen][1].Top = 360;
                manager[screen][1].Visible = true;
                ((OMButton)manager[screen][2]).Image = theHost.getSkinImage("ArtistIcon");
                ((OMButton)manager[screen][3]).Image = theHost.getSkinImage("AlbumIcon");
                ((OMButton)manager[screen][10]).Image = theHost.getSkinImage("PlaylistIcon");
            }
        }
        void Tracks_OnClick(OMControl sender, int screen)
        {
            if (currentAlbum != null)
            {
                ((OMLabel)sender.Parent[6]).Text = currentAlbum + " Tracks";
                SafeThread.Asynchronous(delegate() { showTracks(screen,currentArtist,currentAlbum); }, theHost);
            }
            else if (currentArtist != null)
            {
                ((OMLabel)sender.Parent[6]).Text = currentArtist + " Tracks";
                SafeThread.Asynchronous(delegate() { showTracks(screen, currentArtist); }, theHost);
            }
            else
            {
                ((OMLabel)sender.Parent[6]).Text = "Select a Track";
                SafeThread.Asynchronous(delegate() { showTracks(screen); }, theHost);
            }
            moveToTracks(screen);
        }
        void moveToAlbums(int screen)
        {
            lock (this)
            {
                manager[screen][9].Visible = false;
                ((OMButton)manager[screen][3]).Image = theHost.getSkinImage("AlbumIcon_Selected");
                int diff = (255 - manager[screen][1].Top) / 5;
                for (int i = 1; i < 5; i++)
                {
                    manager[screen][1].Top += diff;
                    Thread.Sleep(20);
                }
                manager[screen][1].Top = 255;
                manager[screen][1].Visible = true;
                ((OMButton)manager[screen][2]).Image = theHost.getSkinImage("ArtistIcon");
                ((OMButton)manager[screen][4]).Image = theHost.getSkinImage("TracksIcon");
                ((OMButton)manager[screen][10]).Image = theHost.getSkinImage("PlaylistIcon");
            }
        }
        void Albums_OnClick(OMControl sender, int screen)
        {
            currentAlbum = null;
            if (currentArtist != null)
            {
                ((OMLabel)sender.Parent[6]).Text = currentArtist + " Albums";
                SafeThread.Asynchronous(delegate() { showAlbums(screen, currentArtist); }, theHost);
            }
            else
            {
                ((OMLabel)sender.Parent[6]).Text = "Select an Album";
                SafeThread.Asynchronous(delegate() { showAlbums(screen); }, theHost);
            }
            moveToAlbums(screen);
        }
        void moveToArtists(int screen)
        {
            lock (this)
            {
                manager[screen][9].Visible = false;
                ((OMButton)manager[screen][2]).Image = theHost.getSkinImage("ArtistIcon_Selected");
                int diff = (155 - manager[screen][1].Top) / 5;
                for (int i = 1; i < 5; i++)
                {
                    manager[screen][1].Top += diff;
                    Thread.Sleep(20);
                }
                manager[screen][1].Top = 155;
                manager[screen][1].Visible = true;
                ((OMButton)manager[screen][3]).Image = theHost.getSkinImage("AlbumIcon");
                ((OMButton)manager[screen][4]).Image = theHost.getSkinImage("TracksIcon");
                ((OMButton)manager[screen][10]).Image = theHost.getSkinImage("PlaylistIcon");
            }
        }
        void Artists_OnClick(OMControl sender, int screen)
        {
            currentArtist = null;
            currentAlbum = null;
            ((OMLabel)sender.Parent[6]).Text = "Select an Artist";
            SafeThread.Asynchronous(delegate() { showArtists(screen); }, theHost);
            moveToArtists(screen);
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