using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.Threading;

namespace Music
{
    [SkinIcon("Icons|Music")]
    public class Music:IHighLevel
    {
        #region IHighLevel Members
        ScreenManager manager;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen, name];
        }

        public string displayName
        {
            get { return "Music"; }
        }

        #endregion

        //Level:
        //0=Artists
        //1=Albums
        //2=Tracks
        //3=Playlists
        //4=Playlist Tracks
        //5=Genres
        //6=Genre Tracks
        #region IBasePlugin Members
        IPluginHost theHost;
        string[] dbName;
        int[] level;
        bool[] abortJob;
        OpenMobile.OMListItem.subItemFormat format = new OMListItem.subItemFormat();
        OImage noCover;
        List<string>[] Artists;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            dbName = new string[theHost.ScreenCount];
            level = new int[theHost.ScreenCount];
            abortJob = new bool[theHost.ScreenCount];
            Artists = new List<string>[theHost.ScreenCount];
            OMPanel p = new OMPanel("");
            imageItem mid = theHost.getSkinImage("MidButton");
            imageItem midHL = theHost.getSkinImage("MidButton_HL");
            imageItem midSL = theHost.getSkinImage("MidButton_Selected"); 
            OMImage background = new OMImage(204, 94, 611, 428);
            background.Image = theHost.getSkinImage("MediaBackground");
            MidButton source = new MidButton(0, 126, 210, 95);
            source.Image = mid;
            source.DownImage = midSL;
            source.FocusImage = midHL;
            source.LeftAlign = true;
            MidButton artists = new MidButton(0, 211, 210, 95);
            artists.Image = mid;
            artists.DownImage = midSL;
            artists.FocusImage = midHL;
            artists.LeftAlign = true;
            artists.Icon = theHost.getSkinImage("Artists", true);
            artists.Text = "Artists";
            artists.OnClick += new userInteraction(artists_OnClick);
            MidButton albums = new MidButton(0, 296, 210, 95);
            albums.Image = mid;
            albums.DownImage = midSL;
            albums.FocusImage = midHL;
            albums.LeftAlign = true;
            albums.Text = "Albums";
            albums.Icon = theHost.getSkinImage("Albums", true);
            albums.OnClick += new userInteraction(albums_OnClick);
            MidButton genre = new MidButton(0, 381, 210, 95);
            genre.Image = mid;
            genre.DownImage = midSL;
            genre.FocusImage = midHL;
            genre.LeftAlign = true;
            genre.Icon = theHost.getSkinImage("Genre", true);
            genre.Text = "Genres";
            genre.OnClick += new userInteraction(genre_OnClick);
            MidButton play = new MidButton(790, 126, 210, 95);
            play.Image = mid;
            play.DownImage = midSL;
            play.FocusImage = midHL;
            play.Icon = theHost.getSkinImage("PlayMedia", true);
            play.Text = "Play";
            play.OnClick += new userInteraction(play_OnClick);
            MidButton add = new MidButton(790, 211, 210, 95);
            add.Image = mid;
            add.DownImage = midSL;
            add.FocusImage = midHL;
            add.Icon = theHost.getSkinImage("AddMedia", true);
            add.Text = "Add";
            add.OnClick += new userInteraction(add_OnClick);
            MidButton playlists = new MidButton(790, 296, 210, 95);
            playlists.Image = mid;
            playlists.DownImage = midSL;
            playlists.FocusImage = midHL;
            playlists.Text = "Playlists";
            playlists.Icon = theHost.getSkinImage("Playlist", true);
            playlists.OnClick += new userInteraction(playlists_OnClick);
            MidButton search = new MidButton(790, 381, 210, 95);
            search.Image = mid;
            search.DownImage = midSL;
            search.FocusImage = midHL;
            search.Icon = theHost.getSkinImage("Search", true);
            search.Text = "Search";

            OMImage separator = new OMImage(279, 160, 443, 2);
            separator.Image = theHost.getSkinImage("MediaSeparator", true);
            OMLabel caption = new OMLabel(225, 115, 560, 45);
            caption.Text = "All Artists";
            caption.Format = eTextFormat.BoldShadow;
            caption.Font = new Font(Font.GenericSansSerif, 26F);
            OMList textList = new OMList(225, 162, 563, 316);
            textList.ListStyle = eListStyle.MultiList;
            textList.ItemColor1 = textList.ItemColor2= Color.Transparent;
            textList.Font = new Font(Font.GenericSansSerif, 29F);
            textList.OnClick += new userInteraction(textList_OnClick);
            OMList mainList = new OMList(225, 162, 563, 316);
            mainList.ListStyle = eListStyle.MultiList;
            mainList.ItemColor1 = textList.ItemColor2 = Color.Transparent;
            mainList.Visible = false;
            mainList.Font= new Font(Font.GenericSansSerif, 21F);
            mainList.ListItemHeight = 70;
            mainList.OnClick += new userInteraction(mainList_OnClick);
            format.color = Color.FromArgb(200, Color.White);
            format.highlightColor = Color.Black;
            format.font = new Font(Font.GenericSansSerif, 18F);
            noCover = theHost.getSkinImage("AlbumArt").image;
            using (PluginSettings settings = new PluginSettings())
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    Artists[i] = new List<string>();
                    dbName[i] = settings.getSetting("Default.MusicDatabase");
                }
            OpenMobile.Threading.TaskManager.QueueTask(() =>
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                    loadArtists(i);
            }, ePriority.High, "Load Artists");
            p.addControl(background);
            p.addControl(source);
            p.addControl(artists);
            p.addControl(albums);
            p.addControl(genre);
            p.addControl(search); //5
            p.addControl(playlists);
            p.addControl(play);
            p.addControl(add);
            p.addControl(separator);
            p.addControl(caption); //10
            p.addControl(textList);
            p.addControl(mainList);
            manager.loadPanel(p);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void albums_OnClick(OMControl sender, int screen)
        {
            ((OMLabel)manager[screen][10]).Text = "All Albums";
            manager[screen][11].Visible = false;
            manager[screen][12].Visible = true;
            SafeThread.Asynchronous(delegate() { showAlbums(screen); }, theHost);
        }

        void add_OnClick(OMControl sender, int screen)
        {
            OMList l = (OMList)manager[screen][12];
            switch (level[screen])
            {
                case 0: //play all
                case 5:
                    break;
                case 1: //play artist/album
                    if (l.SelectedIndex < 0)
                    {
                        if (l.Count == 0)
                            return;
                        List<string> ret = getSongs(l[0].subItem, l[0].text, screen);
                        for (int i = 1; i < l.Count; i++)
                            ret.AddRange(getSongs(l[i].subItem, l[i].text, screen));
                        if (ret.Count > 0)
                            theHost.appendPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                    }
                    else
                    {
                        List<string> ret = getSongs(l[l.SelectedIndex].subItem, l[l.SelectedIndex].text, screen);
                        if (ret.Count > 0)
                            theHost.appendPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                    }
                    break;
                case 2: //tracks
                case 6: //genre tracks
                    if (l.SelectedIndex < 0) //append all
                    {
                        List<mediaInfo> playlist = new List<mediaInfo>(l.Count);
                        for (int i = 0; i < l.Count; i++)
                            playlist.Add(new mediaInfo(l[i].tag.ToString()));
                        theHost.appendPlaylist(playlist, theHost.instanceForScreen(screen));
                    }
                    else
                    {
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l.SelectedItem.tag.ToString());
                        List<mediaInfo> playlist = new List<mediaInfo>(1);
                        playlist.Add(new mediaInfo(l.SelectedItem.tag.ToString()));
                        theHost.appendPlaylist(playlist, theHost.instanceForScreen(screen));
                    }
                    break;
            }
        }

        void mainList_OnClick(OMControl sender, int screen)
        {
            if (level[screen] == 1)
            {
                OMList l=(OMList)sender;
                if (l.SelectedItem.text == "All Albums")
                {
                    ((OMLabel)manager[screen][10]).Text = l.SelectedItem.subItem + " Tracks";
                    showTracks(screen, l.SelectedItem.subItem);
                }
                else
                {
                    ((OMLabel)manager[screen][10]).Text = l.SelectedItem.text + " Tracks";
                    showTracks(screen, l.SelectedItem.text, l.SelectedItem.subItem);
                }
            }
        }
        private void showTracks(int screen, string artist)
        {
            level[screen] = 2;
            OMList l = (OMList)manager[screen][12];
            abortJob[screen] = true;
            lock (manager[screen][12])
            {
                l.Clear();
                MediaLoader.loadSongs(theHost, artist, l, format, dbName[screen], noCover);
            }
        }
        private void showTracks(int screen, string album, string artist)
        {
            level[screen] = 2;
            OMList l = (OMList)manager[screen][12];
            abortJob[screen] = true;
            lock (manager[screen][12])
                l.Clear();
            l.Add("Loading . . .");
            MediaLoader.loadSongs(theHost, artist, album, l, format, dbName[screen], noCover);
        }

        #region Playlists
        private void showPlaylists(int screen)
        {
            level[screen] = 3;
            OMList l = (OMList)manager[screen][11];
            abortJob[screen] = true;
            lock (manager[screen][11])
                l.Clear();
            abortJob[screen] = false;
            l.Add(new OMListItem("Current Playlist", null, "Current Playlist"));
            foreach (string playlist in Playlist.listPlaylistsFromDB(theHost, dbName[screen]))
            {
                if (abortJob[screen])
                    return;
                if ((playlist.Length != 8) && (!playlist.StartsWith("Current")))
                    l.Add(new OMListItem(playlist, null, playlist));
            }
            //if (currentSource[screen] == null)
                //foreach (DeviceInfo info in sources)
            foreach(DeviceInfo info in DeviceInfo.EnumerateDevices(theHost))
                if (abortJob[screen])
                    return;
                else
                    loadPlaylists(info, l);
            //else
            //    loadPlaylists(currentSource[screen], l);
        }
        private void loadPlaylists(DeviceInfo info, OMList l)
        {
            foreach (string playlistPath in info.PlaylistFolders)
                foreach (string playlist in Playlist.listPlaylists(playlistPath))
                    l.Add(new OMListItem(Path.GetFileNameWithoutExtension(playlist), null, playlist));
        }
        private void showPlaylist(int screen, string path)
        {
            level[screen] = 4;
            OMList l = (OMList)manager[screen][12];
            abortJob[screen] = true;
            lock (manager[screen][12])
                l.Clear();
            l.Add("Loading...");
            lock (manager[screen][12])
            {
                abortJob[screen] = false;
                if (path == "Current Playlist")
                {
                    l.Clear();
                    foreach (mediaInfo info in theHost.getPlaylist(theHost.instanceForScreen(screen)))
                    {
                        if (abortJob[screen])
                            return;
                        mediaInfo tmp = info; // <-stupid .Net limitation
                        if (tmp.Name == null)
                            tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                        if (tmp == null)
                            continue;
                        if (tmp.coverArt == null)
                            tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost);
                        if (tmp.coverArt == null)
                            tmp.coverArt = TagReader.getFolderImage(tmp.Location);
                        if (tmp.coverArt == null)
                            tmp.coverArt = theHost.getSkinImage("Unknown Album").image;
                        l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                    }
                }
                else if (System.IO.Path.IsPathRooted(path))
                {
                    List<mediaInfo> playlist = Playlist.readPlaylist(path);
                    l.Clear();
                    foreach (mediaInfo info in playlist)
                    {
                        if (abortJob[screen])
                            return;
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
                    l.Clear();
                    foreach (mediaInfo info in Playlist.readPlaylistFromDB(theHost, path, dbName[screen]))
                    {
                        if (abortJob[screen])
                            return;
                        mediaInfo tmp = info; // <-stupid .Net limitation
                        if (tmp.Name == null)
                            tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                        if (tmp == null)
                            continue;
                        if (tmp.coverArt == null)
                            tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost, dbName[screen]);
                        if (tmp.coverArt == null)
                            tmp.coverArt = theHost.getSkinImage("Unknown Album").image;
                        l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                    }
                }
            }
        }
        #endregion
        void playlists_OnClick(OMControl sender, int screen)
        {
            ((OMLabel)manager[screen][10]).Text = "All Playlists";
            manager[screen][11].Visible = true;
            manager[screen][12].Visible = false;
            SafeThread.Asynchronous(delegate() { showPlaylists(screen); }, theHost);
        }
        private void loadArtists(int screen)
        {
            object o;
            theHost.getData(eGetData.GetMediaDatabase, dbName[screen], out  o);
            if (o == null)
                return;
            lock (manager[screen][11])
            {
                Artists[screen].Clear();
                using (IMediaDatabase db = (IMediaDatabase)o)
                {
                    db.beginGetArtists(false);
                    mediaInfo info = db.getNextMedia();
                    while (info != null)
                    {
                        Artists[screen].Add(info.Artist);
                        info = db.getNextMedia();
                    }
                    db.endSearch();
                }
                ((OMList)manager[screen][11]).AddRange(Artists[screen]);
            }
        }
        void genre_OnClick(OMControl sender, int screen)
        {
            level[screen] = 5;
            ((OMLabel)manager[screen][10]).Text = "All Genres";
            OMList l=(OMList)manager[screen][11];
            l.Clear();
            l.Add("Loading...");
            manager[screen][11].Visible = true;
            manager[screen][12].Visible = false;
            lock(manager[screen][11])
                OpenMobile.Media.MediaLoader.loadGenres(theHost, l, dbName[screen]);
        }

        void play_OnClick(OMControl sender, int screen)
        {
            OMList l = (OMList)manager[screen][12];
            switch(level[screen])
            {
                case 0: //play all
                case 5:
                    break;
                case 1: //play artist/album
                    if (l.SelectedIndex < 0)
                    {
                        if (l.Count == 0)
                            return;
                        if (l[0].text == "Loading . . .")
                            return;
                        List<string> ret = getSongs(l[0].subItem, l[0].text, screen);
                        if (ret.Count > 0)
                        {
                            if (theHost.getRandom(theHost.instanceForScreen(screen)))
                            {
                                int random = OpenMobile.Framework.Math.Calculation.RandomNumber(0, ret.Count - 1);
                                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[random]);
                            }
                            else
                                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[0]);
                        }
                        for (int i = 1; i < l.Count; i++)
                            ret.AddRange(getSongs(l[i].subItem, l[i].text, screen));
                        if (ret.Count > 0)
                            theHost.setPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                    }
                    else
                    {
                        if (l.SelectedItem.tag == null)
                            return;
                        List<string> ret = getSongs(l[l.SelectedIndex].subItem, l[l.SelectedIndex].text, screen);
                        if (ret.Count > 0)
                        {
                            if (theHost.getRandom(theHost.instanceForScreen(screen)))
                            {
                                int random = OpenMobile.Framework.Math.Calculation.RandomNumber(0, ret.Count - 1);
                                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[random]);
                            }
                            else
                                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), ret[0]);
                            theHost.setPlaylist(Playlist.Convert(ret), theHost.instanceForScreen(screen));
                            l.SelectedIndex = -1;

                        }
                    }
                    break;
                case 2:
                case 6: //genre tracks
                    if (l.SelectedIndex < 0) //play all
                    {
                        if (l.Count == 0)
                            return;
                        if (l[0].text == "Loading . . .")
                            return;
                        int index=0;
                        if (theHost.getRandom(theHost.instanceForScreen(screen)))
                        {
                            index = OpenMobile.Framework.Math.Calculation.RandomNumber(0, l.Count - 1);
                            theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l[index].tag.ToString());
                        }
                        else
                            theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l[0].tag.ToString());
                        List<mediaInfo> playlist=new List<mediaInfo>();
                        for (int i = 0; i < l.Count; i++)
                        {
                            playlist.Add(new mediaInfo(l[i].tag.ToString()));
                        }
                        theHost.setPlaylist(playlist, theHost.instanceForScreen(screen));
                        theHost.execute(eFunction.setPlaylistPosition, theHost.instanceForScreen(screen).ToString(), index.ToString());
                    }
                    else
                    {
                        if (l.SelectedItem.tag == null)
                            return;
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l.SelectedItem.tag.ToString());
                        List<mediaInfo> playlist = new List<mediaInfo>();
                        playlist.Add(new mediaInfo(l.SelectedItem.tag.ToString()));
                        theHost.setPlaylist(playlist, theHost.instanceForScreen(screen));
                        l.SelectedIndex = -1;
                    }
                    break;
                case 4:
                    if (l.SelectedIndex < 0) //play all
                    {
                        if (l.Count == 0)
                            return;
                        if (l[0].text == "Loading . . .")
                            return;
                        int index = 0;
                        if (theHost.getRandom(theHost.instanceForScreen(screen)))
                        {
                            index = OpenMobile.Framework.Math.Calculation.RandomNumber(0, l.Count - 1);
                            theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l[index].tag.ToString());
                        }
                        else
                            theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l[0].tag.ToString());
                        List<mediaInfo> playlist = new List<mediaInfo>();
                        for (int i = 0; i < l.Count; i++)
                        {
                            playlist.Add(new mediaInfo(l[i].tag.ToString()));
                        }
                        theHost.setPlaylist(playlist, theHost.instanceForScreen(screen));
                        theHost.execute(eFunction.setPlaylistPosition, theHost.instanceForScreen(screen).ToString(), index.ToString());
                    }
                    else
                    {
                        if (l.SelectedItem.tag == null)
                            return;
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), l.SelectedItem.tag.ToString());
                        List<mediaInfo> playlist = new List<mediaInfo>();
                        for (int i = 0; i < l.Count; i++)
                        {
                            playlist.Add(new mediaInfo(l[i].tag.ToString()));
                        }
                        theHost.setPlaylist(playlist, theHost.instanceForScreen(screen));
                        theHost.execute(eFunction.setPlaylistPosition, theHost.instanceForScreen(screen).ToString(), l.SelectedIndex.ToString());
                        l.SelectedIndex = -1;
                    }
                    break;
            }
        }

        private List<string> getSongs(string artist, string album, int screen)
        {
            object o;
            theHost.getData(eGetData.GetMediaDatabase, dbName[screen], out  o);
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
        private List<string> getSongs(string artist, int screen)
        {
            object o;
            theHost.getData(eGetData.GetMediaDatabase, dbName[screen], out  o);
            if (o == null)
                return new List<string>();
            IMediaDatabase db = (IMediaDatabase)o;
            db.beginGetSongsByArtist(artist, false, eMediaField.Title);
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
        private void showAlbums(int screen)
        {
            level[screen] = 1;
            OMList l = (OMList)manager[screen][12];
            abortJob[screen] = true;
            lock (manager[screen][12])
                l.Clear();
            lock (manager[screen][12])
            {
                abortJob[screen] = false;
                foreach (string artist in Artists[screen])
                {
                    if (abortJob[screen])
                        return;
                    MediaLoader.loadAlbums(theHost, artist, l, format, false, dbName[screen], noCover);
                    l.Sort();
                }
            }
        }
        void textList_OnClick(OMControl sender, int screen)
        {
            if (level[screen] == 0)
            {
                level[screen] = 1;
                OMList l = (OMList)manager[screen][12];
                abortJob[screen] = true;
                lock (manager[screen][12])
                    l.Clear();
                string artist=((OMList)sender).SelectedItem.text;
                ((OMLabel)manager[screen][10]).Text = artist + " Albums";
                l.Add(new OMListItem("All Albums",artist,noCover,format));
                lock(manager[screen][12])
                    OpenMobile.Media.MediaLoader.loadAlbums(theHost, ((OMList)sender).SelectedItem.text, l, format,false, dbName[screen], noCover);
                manager[screen][12].Visible = true;
                manager[screen][11].Visible = false;
            }
            else if (level[screen] == 3)
            {
                OMList l = (OMList)manager[screen][11];
                ((OMLabel)manager[screen][10]).Text = l.SelectedItem.text + " Tracks";
                manager[screen][11].Visible = false;
                manager[screen][12].Visible = true;
                SafeThread.Asynchronous(delegate() { showPlaylist(screen, l.SelectedItem.tag.ToString()); }, theHost);
            }
            else if (level[screen] == 5)
            {
                level[screen] = 6;
                OMList l = (OMList)manager[screen][12];
                l.Clear();
                ((OMLabel)manager[screen][10]).Text = ((OMList)sender).SelectedItem.text + " Tracks";

                OpenMobile.Media.MediaLoader.loadSongsByGenre(theHost, ((OMList)sender).SelectedItem.text, l, format, true, dbName[screen], noCover);
                manager[screen][12].Visible = true;
                manager[screen][11].Visible = false;
            }
        }

        void artists_OnClick(OMControl sender, int screen)
        {
            level[screen] = 0;
            ((OMLabel)manager[screen][10]).Text = "All Artists";
            abortJob[screen] = true;
            lock (manager[screen][11])
            {
                OMList l = (OMList)manager[screen][11];
                l.Clear();
                l.AddRange(Artists[screen]);
            }
            manager[screen][11].Visible = true;
            manager[screen][12].Visible = false;
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
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            get { return "Music"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Music"; }
        }

        public bool incomingMessage(string message, string source)
        {
            if (message == "QueryFavorite")
            {
                int screen;
                if ((source.Length < 7) || (!int.TryParse(source.Substring(6), out screen)))
                    return false;
                switch (level[screen])
                {
                    case 4:
                        string pl = ((OMLabel)manager[screen][10]).Text;
                        pl = pl.Substring(0, pl.Length - 7);
                        string msg=pl+";Playlist;"+"Playlist|"+pl;
                        theHost.sendMessage<string>("UI", "Screen" + screen.ToString(), "SetFavorite", ref msg);
                        break;
                }
            }
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
