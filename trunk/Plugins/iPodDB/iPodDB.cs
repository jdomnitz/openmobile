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
using System.Text;
using Mono.Data.Sqlite;
using OpenMobile.Plugin;
using OpenMobile;
using System.Threading;
using OpenMobile.helperFunctions;
using OpenMobile.Media;
using OpenMobile.Threading;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using System.IO;
using OpenMobile.Data;
using OpenPOD;
using System.Drawing;
using System.Diagnostics;

namespace iPodDB
{
    public class iPodDB:IMediaDatabase
    {
        public SqliteDataReader reader;
        public SqliteConnection con;

        public Settings loadSettings()
        {
            return null;
        }
        public void Dispose()
        {
            if (this.reader != null)
                try
                {
                    this.reader.Dispose();
                }
                catch (InvalidOperationException) { }
        }
        public string databaseType
        {
            get { return "iPod"; }
        }

        public bool indexDirectory(string location, bool subdirectories)
        {
            OpenMobile.Threading.TaskManager.QueueTask(delegate { makeItSo(location, subdirectories); }, ePriority.MediumLow, "Index Music Directory");
            return true;
        }
        public bool clearIndex()
        {
            lock (con)
                using (SqliteCommand cmd = new SqliteCommand("DELETE FROM tblSongs; DELETE FROM tblAlbum", con))
                    return (cmd.ExecuteNonQuery() > 0);
        }
        private bool clearIndex(string path)
        {
            lock (con)
                using (SqliteCommand cmd = new SqliteCommand("DELETE FROM tblSongs WHERE Device='"+path+"'; DELETE FROM tblAlbum WHERE Device='"+path+"'", con))
                    return (cmd.ExecuteNonQuery() > 0);
        }
        public List<string> listPlaylists()
        {
            if (db == null)
                return new List<string>();
            List<string> ret = new List<string>();
            for (int i = 0; i < db.playlists.Count; i++)
                ret.Add(db.playlists[i].title);
            return ret;
        }
        public IMediaDatabase getNew()
        {
            if (theHost == null)
                return null;
            return this;
        }

        private void makeItSo(string location, bool subdirectories)
        {
            currentURL = location;
            theHost.execute(eFunction.backgroundOperationStatus, "Syncing with device...");
            doWork();
            theHost.execute(eFunction.backgroundOperationStatus, "Indexing Complete!");
        }
        private string currentURL = "";
        ArtReader art;
        DBReader db;
        private void doWork()
        {
            db = new DBReader(currentURL);
            try
            {
                art = new ArtReader(OpenMobile.Path.Combine(currentURL, "iPod_Control", "Artwork"));
            }
            catch (Exception) { }
            for (int i = 0; i < db.songs.Count; i++)
            {
                processFile(db.songs[i]);
            }
        }

        public bool indexFile(string file)
        {
            return false;
        }
        private string album;
        private string artist;
        private bool hasCover;
        private long albumNum;
        private void processFile(Song info)
        {
            lock (con)
            {
                if ((info.Album == null) || (info.Album == ""))
                    info.Album = "Unknown Album";
                if ((info.Artist == null) || (info.Artist == ""))
                    info.Artist = "Unknown Artist";
                if ((info.Album == album) && (info.Artist == artist))
                {
                    if ((hasCover == false) && (info.ArtID>0))
                        updateCover(info);
                    writeSong(info);
                }
                else
                {
                    Bitmap bmp=null;
                    if (albumExists(info) == true)
                        writeSong(info);
                    else
                    {
                        if (info.ArtID == 0)
                            bmp=TagReader.getInfo(info.Location).coverArt.image;
                        writeAlbum(info,bmp);
                        writeSong(info);
                    }
                }
            }
        }

        private void updateCover(Song info)
        {
            Bitmap bmp = null;
            if (art != null)
            {
                AlbumImage aimg = art.images.Find(p => p.ArtworkID == info.ArtID);
                if (aimg.size == 0)
                    return;
                bmp = art.getCoverArt(aimg);
            }
            using (SqliteCommand command = con.CreateCommand())
            {
                command.CommandText = "UPDATE tblAlbum SET Cover=@cover WHERE ID='" + albumNum + "'";
                System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                command.Parameters.Add(new SqliteParameter("@cover", img.ConvertTo(bmp, typeof(byte[]))));
                command.ExecuteNonQuery();
                hasCover = true;
            }
        }

        private bool albumExists(Song info)
        {
            using (SqliteCommand command = con.CreateCommand())
            {
                StringBuilder s = new StringBuilder("SELECT ID FROM tblAlbum WHERE UPPER(Artist)='");
                s.Append(General.escape(info.Artist.ToUpper()) + "' AND UPPER(Album)='");
                s.Append(General.escape(info.Album.ToUpper()) + "'");
                command.CommandText = s.ToString();
                object id = command.ExecuteScalar();
                if (id == null)
                    return false;
                albumNum = (long)id;
                album = info.Album;
                artist = info.Artist;
                return true;
            }
        }

        private void writeSong(Song info)
        {
            using (SqliteCommand command = con.CreateCommand())
            {
                if ((info.Location == null) || (info.Location == ""))
                    return;
                StringBuilder s = new StringBuilder("BEGIN;INSERT INTO tblSongs(Title,URL,AlbumNum,Track,Rating,Genre,Lyrics,Device)VALUES('");
                s.Append(General.escape(info.Title));
                s.Append("','");
                s.Append(General.escape(info.Location));
                s.Append("','");
                s.Append(albumNum.ToString());
                s.Append("','");
                s.Append("0");//stub
                s.Append("','");
                s.Append(info.Rating.ToString());
                s.Append("','");
                s.Append(General.escape(info.Genre));
                s.Append("','");
                s.Append("");
                s.Append("','");
                s.Append(General.escape(System.IO.Path.GetPathRoot(info.Location)));
                s.Append("');COMMIT");
                command.CommandText = s.ToString();
                command.ExecuteNonQuery();
            }
        }
        private void writeAlbum(Song info,Bitmap bmp)
        {
            if (art != null)
            {
                AlbumImage aimg = art.images.Find(p => p.ArtworkID == info.ArtID);
                if (aimg.size > 0)
                    bmp = art.getCoverArt(aimg);
            }
            using (SqliteCommand command = con.CreateCommand())
            {
                StringBuilder s;
                if (bmp != null)
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Device,Artist,Cover)VALUES('");
                else
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Device,Artist)VALUES('");
                s.Append(General.escape(info.Album));
                s.Append("','");
                s.Append(General.escape(System.IO.Path.GetPathRoot(info.Location)));
                s.Append("','");
                info.Artist = correctArtist(info.Artist);
                if (bmp != null)
                {
                    s.Append(General.escape(info.Artist));
                    s.Append("',@cover)");
                    System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                    command.Parameters.Add(new SqliteParameter("@cover", img.ConvertTo(bmp, typeof(byte[]))));
                    hasCover = true;
                }
                else
                {
                    s.Append(General.escape(info.Artist));
                    s.Append("')");
                    hasCover = false;
                }
                command.CommandText = s.ToString();
                command.ExecuteNonQuery();
                command.CommandText = "SELECT MAX(ID) FROM tblAlbum";
                albumNum = (long)command.ExecuteScalar();
            }
            album = info.Album;
            artist = info.Artist;
        }

        private string correctArtist(string p)
        {
            using (SqliteCommand command = con.CreateCommand())
            {
                StringBuilder s = new StringBuilder("SELECT Artist FROM tblAlbum WHERE UPPER(Artist)='");
                s.Append(General.escape(p.Replace("  ", " ").ToUpper()) + "'");
                command.CommandText = s.ToString();
                object id = command.ExecuteScalar();
                if (id == null)
                    return p;
                else
                    return id.ToString();
            }
        }

        public bool supportsFileIndexing { get { return true; } }

        private bool rCover;
        private eMediaField field;
        public bool beginGetArtists(bool covers)
        {
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Distinct Artist FROM tblAlbum";
                else
                    command.CommandText = "SELECT Distinct(Artist),Cover FROM tblAlbum";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Artist;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetAlbums(bool covers)
        {
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Distinct Artist,Album FROM tblAlbum";
                else
                    command.CommandText = "SELECT Distinct Artist,Album,Cover FROM tblAlbum";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Album;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetAlbums(string artist, bool covers)
        {
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Distinct Artist,Album FROM tblAlbum WHERE Artist='" + General.escape(artist) + "'";
                else
                    command.CommandText = "SELECT Distinct Artist,Album,Cover FROM tblAlbum WHERE Artist='" + General.escape(artist) + "'";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Album;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongs(bool covers, eMediaField sortBy)
        {
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum ORDER BY title";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum ORDER BY title";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByArtist(string artist, bool covers, eMediaField sortBy)
        {
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = "ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = "ORDER BY rating";
                    break;
            }
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'" + order;
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'" + order;
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByGenre(string genre, bool covers, eMediaField sortBy)
        {
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = "ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = "ORDER BY rating";
                    break;
            }
            SqliteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'" + order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'" + order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByRating(string rating, bool covers, eMediaField sortBy)
        {
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = "ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = "ORDER BY rating";
                    break;
            }
            SqliteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'" + order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'" + order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByAlbum(string artist, string album, bool covers, eMediaField sortBy)
        {
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "' ORDER BY title";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "' ORDER BY title";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByLyrics(string phrase, bool covers, eMediaField sortBy)
        {
            SqliteCommand command = con.CreateCommand();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = "ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = "ORDER BY rating";
                    break;
            }
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'" + order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'" + order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetGenres()
        {
            SqliteCommand command = con.CreateCommand();
            command.CommandText = "SELECT Distinct Genres FROM tblSongs";
            reader = command.ExecuteReader();
            field = eMediaField.Genre;
            return (reader != null);
        }
        public bool setRating(mediaInfo info)
        {
            throw new NotImplementedException();
        }
        public mediaInfo getNextMedia()
        {
            try
            {
                if (reader.Read() == false)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            lock (reader)
            {
                mediaInfo i = new mediaInfo();
                switch (field)
                {
                    case eMediaField.Album:
                        i.Album = reader["Album"].ToString();
                        i.Artist = reader["Artist"].ToString();
                        break;
                    case eMediaField.Title:
                        i.Name = reader["Title"].ToString();
                        i.Album = reader["Album"].ToString();
                        i.Artist = reader["Artist"].ToString();
                        i.Location = reader["URL"].ToString();
                        break;
                    case eMediaField.Artist:
                        i.Artist = reader["Artist"].ToString();
                        break;
                    case eMediaField.Genre:
                        i.Genre = reader["Genre"].ToString();
                        break;
                }

                if (rCover == true)
                {
                    try
                    {
                        object vl = reader.GetValue(reader.GetOrdinal("Cover"));

                        if (vl.GetType() != typeof(DBNull))
                        {
                            MemoryStream m = new MemoryStream((byte[])vl);
                            if (m.Length>10)
                                i.coverArt = OImage.FromStream(m);
                        }
                    }
                    catch (Exception) { }
                }
                return i;
            }
        }

        public bool endSearch()
        {
            if (reader != null)
                reader.Dispose();
            reader = null;
            return true;
        }

        public bool supportsNaturalSearches
        {
            get { return false; }
        }

        public bool beginNaturalSearch(string query)
        {
            throw new NotImplementedException();
        }
        int currentplaylist;
        public bool beginGetPlaylist(string name)
        {
            if (db == null)
                return false;
            playlistPos = 0;
            for (int i = 0; i < db.playlists.Count; i++)
                if (db.playlists[i].title == name)
                {
                    currentplaylist = i;
                    return true;
                }
            return false;
        }
        public bool supportsPlaylists { get { return true; } }

        public bool writePlaylist(List<string> URLs, string name, bool append)
        {
            return false;
        }

        public bool removePlaylist(string name)
        {
            return false;
        }
        int playlistPos;
        public mediaInfo getNextPlaylistItem()
        {
            if (playlistPos >= db.playlists[currentplaylist].songs.Count)
                return null;
            int id=db.playlists[currentplaylist].songs[playlistPos];
            playlistPos++;
            Song ret= db.songs.Find(p=>p.id==id);
            if (ret.id == 0)
                return new mediaInfo();
            mediaInfo info = new mediaInfo(ret.Location);
            info.Name = ret.Title;
            info.Type = eMediaType.Local;
            info.Artist = ret.Artist;
            info.Album = ret.Album;
            return info;
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@optonline.net"; }
        }

        public string pluginName
        {
            get { return "iPodDB"; }
        }

        public float pluginVersion
        {
            get { return 0.8F; }
        }

        public string pluginDescription
        {
            get { return "The Default iPod Media Database (Sqlite)"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        public iPodDB() { }
        public iPodDB(IPluginHost host)
        {
            theHost = host;
        }
        private IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            using (PluginSettings settings = new PluginSettings())
            {
                settings.setSetting("Default.AppleDatabase", "iPodDB");
            }
            lock (this)
                createDB();
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                SafeThread.Asynchronous(() =>
                {
                    foreach (DeviceInfo info in DeviceInfo.EnumerateDevices(theHost))
                    {
                        if (info.DriveType == eDriveType.iPod)
                        {
                            indexDirectory(info.path, true);
                        }
                    }
                }, theHost);
            }
        }

        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (!justInserted)
                return;
            if (type==eMediaType.AppleDevice)
            {
                indexDirectory(arg, true);
            }
            else if (type == eMediaType.DeviceRemoved)
            {
                this.clearIndex(arg);
            }
        }

        private void createDB()
        {
            con = new SqliteConnection(@"Data Source=:memory:;Pooling=false;synchronous=0;");
            con.Open();
            SqliteCommand cmd = new SqliteCommand("BEGIN TRANSACTION;CREATE TABLE tblAlbum(ID INTEGER PRIMARY KEY,Album TEXT, Artist TEXT, Device TEXT, Cover BLOB);CREATE TABLE tblSongs (AlbumNum INTEGER, Device TEXT, Genre TEXT, Rating NUMERIC, Title TEXT, Track INTEGER, URL TEXT, Lyrics TEXT);COMMIT;", con);
            cmd.ExecuteNonQuery();
        }
        #endregion
    }
}
