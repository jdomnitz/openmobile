using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;

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
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(theHost.ScreenCount);
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
            OMList list = new OMList(225, 162, 563, 325);
            list.ListStyle = eListStyle.MultiList;
            list.ItemColor1 = list.ItemColor2= Color.Transparent;
            list.Font = new Font(Font.GenericSansSerif, 24F);
            string dbName;
            using(PluginSettings settings=new PluginSettings())
                dbName=settings.getSetting("Default.MusicDatabase");
            OpenMobile.Media.MediaLoader.loadArtists(theHost, list, dbName);
            p.addControl(background);
            p.addControl(source);
            p.addControl(artists);
            p.addControl(albums);
            p.addControl(genre);
            p.addControl(search);
            p.addControl(playlists);
            p.addControl(play);
            p.addControl(add);
            p.addControl(separator);
            p.addControl(caption);
            p.addControl(list);
            manager.loadPanel(p);
            return OpenMobile.eLoadStatus.LoadSuccessful;
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
