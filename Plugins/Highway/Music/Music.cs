using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Media;

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

        #region IBasePlugin Members
        IPluginHost theHost;
        string[] dbName;
        int[] level;
        OpenMobile.OMListItem.subItemFormat format = new OMListItem.subItemFormat();
        OImage noCover;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
            dbName = new string[theHost.ScreenCount];
            level = new int[theHost.ScreenCount];
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
            MidButton genre = new MidButton(0, 381, 210, 95);
            genre.Image = mid;
            genre.DownImage = midSL;
            genre.FocusImage = midHL;
            genre.LeftAlign = true;
            genre.Icon = theHost.getSkinImage("Genre", true);
            genre.Text = "Genre";

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
            MidButton playlists = new MidButton(790, 296, 210, 95);
            playlists.Image = mid;
            playlists.DownImage = midSL;
            playlists.FocusImage = midHL;
            playlists.Text = "Playlists";
            playlists.Icon = theHost.getSkinImage("Playlist", true);
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
            OMList artistList = new OMList(225, 162, 563, 318);
            artistList.ListStyle = eListStyle.MultiList;
            artistList.ItemColor1 = artistList.ItemColor2= Color.Transparent;
            artistList.Font = new Font(Font.GenericSansSerif, 29F);
            artistList.OnClick += new userInteraction(artistList_OnClick);
            OMList mainList = new OMList(225, 162, 563, 318);
            mainList.ListStyle = eListStyle.MultiList;
            mainList.ItemColor1 = artistList.ItemColor2 = Color.Transparent;
            mainList.Visible = false;
            mainList.Font= new Font(Font.GenericSansSerif, 21F);
            mainList.ListItemHeight = 70;
            format.color = Color.FromArgb(200, Color.White);
            format.highlightColor = Color.Black;
            format.font = new Font(Font.GenericSansSerif, 18F);
            noCover = theHost.getSkinImage("AlbumArt").image;
            using (PluginSettings settings = new PluginSettings())
                for (int i = 0; i < theHost.ScreenCount; i++)
                    dbName[i] = settings.getSetting("Default.MusicDatabase");
            OpenMobile.Media.MediaLoader.loadArtists(theHost, artistList, dbName[0]);
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
            p.addControl(artistList);
            p.addControl(mainList);
            manager.loadPanel(p);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void play_OnClick(OMControl sender, int screen)
        {
            if (level[screen] == 0) //play all
            {
                //play all
            }
            else if (level[screen] == 1) //play artist/album
            {
                OMList l = (OMList)manager[screen][12];
                if (l.SelectedIndex < 0)
                {
                    if (l.Count == 0)
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
                    }
                }
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

        void artistList_OnClick(OMControl sender, int screen)
        {
            level[screen] = 1;
            OMList l = (OMList)manager[screen][12];
            l.Clear();
            ((OMLabel)manager[screen][10]).Text = ((OMList)sender).SelectedItem.text + " Albums";
            
            OpenMobile.Media.MediaLoader.loadAlbums(theHost, ((OMList)sender).SelectedItem.text, l, format, true, dbName[screen], noCover);
            manager[screen][12].Visible = true;
            manager[screen][11].Visible = false;
        }

        void artists_OnClick(OMControl sender, int screen)
        {
            level[screen] = 0;
            ((OMLabel)manager[screen][10]).Text = "All Artists";
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
