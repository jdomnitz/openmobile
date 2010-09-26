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
using System.Data;
using Mono.Data.Sqlite;
using System.Drawing;
using OpenMobile.Graphics;
using System.IO;
using System.Text;
using System.Threading;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.helperFunctions;
using OpenMobile.Media;
using OpenMobile.Plugin;
using OpenMobile.Framework;

namespace RemovableDB
{
    public sealed class RemovableDB : IMediaDatabase
    {
        public SqliteDataReader reader;
        public SqliteConnection con;
        private List<string> toBeIndexed;

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
            //GC.SuppressFinalize(this);
        }
        public string databaseType
        {
            get { return "Removable"; }
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
            return new List<string>();
        }
        private void parseDirectory(string location, bool subdirectories)
        {
            if ((location == null) || (location == ""))
                return;
            parse(location, subdirectories);
        }
        public IMediaDatabase getNew()
        {
            if (theHost == null)
                return null;
            return this;
        }
        private void parse(string location, bool subdirectories)
        {
            string[] filters = new string[] { "*.mp3", "*.m4a", "*.mpc", "*.flac", "*.wv", "*.aac", "*.aif", "*.aiff", "*.asf", "*.ape", "*.wav", "*.m4p", "*.ogg", "*.wma", "*.oga", "*.spx", "*.m4b", "*.rma", "*.mpp" };
            foreach (string filter in filters)
                foreach (string file in Directory.GetFiles(location, filter))
                    if (toBeIndexed != null)
                        toBeIndexed.Add(file);
            if (subdirectories == true)
                foreach (string folder in Directory.GetDirectories(location))
                    parse(folder, true);
        }

        private int startCount;
        ManualResetEvent task1 = new ManualResetEvent(false);
        private void makeItSo(string location, bool subdirectories)
        {
            //Phase 1 - Find out what needs to be indexed
            parseDirectory(location, subdirectories);
            //Phase 2 - Extract info from everything unindexed
            startCount = toBeIndexed.Count;
            abc += def;
            tmr = new System.Threading.Timer(abc, null, 0, 2000);
            doWork();
        }
        private System.Threading.Timer tmr;
        private TimerCallback abc;
        private int lastCount;
        private string lastURL;
        private string currentURL = "";
        private void def(object state)
        {
            if ((toBeIndexed == null) || (toBeIndexed.Count == 0))
            {
                if (tmr == null)
                    return;
                else
                {
                    tmr.Dispose();
                    tmr = null;
                }
                theHost.execute(eFunction.backgroundOperationStatus, "Indexing Complete!");
                return;
            }
            if (lastURL == currentURL)
                theHost.execute(eFunction.backgroundOperationStatus, "FAILURE:" + System.IO.Path.GetFileName(currentURL));
            else
                theHost.execute(eFunction.backgroundOperationStatus, (((startCount - toBeIndexed.Count) / (double)startCount) * 100).ToString("0.00") + "% (" + (((startCount - toBeIndexed.Count) - lastCount) / 2).ToString() + " songs/sec)");
            lastURL = currentURL;
            lastCount = (startCount - toBeIndexed.Count);
        }
        private void doWork()
        {
            while (true)
            {
                if (toBeIndexed == null)
                    return;
                lock (toBeIndexed)
                {
                    if (toBeIndexed.Count == 0)
                        return;
                    currentURL = toBeIndexed[toBeIndexed.Count - 1];
                    toBeIndexed.RemoveAt(toBeIndexed.Count - 1);
                }
                processFile(currentURL);
            }
        }

        public bool indexFile(string file)
        {
            try
            {
                processFile(file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string album;
        private string artist;
        private bool hasCover;
        private long albumNum;
        private void processFile(string filepath)
        {
            lock (con)
            {
                mediaInfo info = TagReader.getInfo(filepath);
                if (info == null)
                    return;
                if ((info.Album == null) || (info.Album == ""))
                    info.Album = "Unknown Album";
                if ((info.Artist == null) || (info.Artist == ""))
                    info.Artist = "Unknown Artist";
                if ((info.Album == album) && (info.Artist == artist))
                {
                    if ((hasCover == false) && (info.coverArt != null))
                        updateCover(info);
                    writeSong(info);
                }
                else
                {
                    if (albumExists(info) == true)
                        writeSong(info);
                    else
                    {
                        if (info.coverArt == null)
                            info.coverArt = TagReader.getFolderImage(info.Location);
                        //Too slow for a removable database
                        //if (info.coverArt == null)
                        //    info.coverArt = TagReader.getLastFMImage(info.Artist, info.Album);
                        writeAlbum(info);
                        writeSong(info);
                    }
                }
            }
        }

        private void updateCover(mediaInfo info)
        {
            if (info.coverArt == null)
                return;
            using (SqliteCommand command = con.CreateCommand())
            {
                command.CommandText = "UPDATE tblAlbum SET Cover=@cover WHERE ID='" + albumNum + "'";
                System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                command.Parameters.Add(new SqliteParameter("@cover", img.ConvertTo(info.coverArt, typeof(byte[]))));
                command.ExecuteNonQuery();
                hasCover = true;
            }
        }

        private bool albumExists(mediaInfo info)
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

        private void writeSong(mediaInfo info)
        {
            using (SqliteCommand command = con.CreateCommand())
            {
                if ((info.Location == null) || (info.Location == ""))
                    return;
                StringBuilder s = new StringBuilder("BEGIN;INSERT INTO tblSongs(Title,URL,AlbumNum,Track,Rating,Genre,Lyrics,Device)VALUES('");
                s.Append(General.escape(info.Name));
                s.Append("','");
                s.Append(General.escape(info.Location));
                s.Append("','");
                s.Append(albumNum.ToString());
                s.Append("','");
                s.Append(info.TrackNumber.ToString());
                s.Append("','");
                s.Append(info.Rating.ToString());
                s.Append("','");
                s.Append(General.escape(info.Genre));
                s.Append("','");
                s.Append(General.escape(info.Lyrics));
                s.Append("','");
                s.Append(General.escape(System.IO.Path.GetPathRoot(info.Location)));
                s.Append("');COMMIT");
                command.CommandText = s.ToString();
                command.ExecuteNonQuery();
            }
        }
        private void writeAlbum(mediaInfo info)
        {
            using (SqliteCommand command = con.CreateCommand())
            {
                StringBuilder s;
                if (info.coverArt != null)
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Device,Artist,Cover)VALUES('");
                else
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Device,Artist)VALUES('");
                s.Append(General.escape(info.Album));
                s.Append("','");
                s.Append(General.escape(System.IO.Path.GetPathRoot(info.Location)));
                s.Append("','");
                info.Artist = correctArtist(info.Artist);
                if (info.coverArt != null)
                {
                    s.Append(General.escape(info.Artist));
                    s.Append("',@cover)");
                    System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                    command.Parameters.Add(new SqliteParameter("@cover", img.ConvertTo(info.coverArt.image, typeof(byte[]))));
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
                // Comment from Borte: This SQL query contains a bug since it returns multiple rows for one artist if the images are different
                //command.CommandText = "SELECT Distinct Artist,Cover FROM tblAlbum";
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
                    object vl = reader.GetValue(reader.GetOrdinal("Cover"));

                    if (vl.GetType() != typeof(DBNull))
                    {
                        MemoryStream m = new MemoryStream((byte[])vl);
                        i.coverArt = OImage.FromStream(m);
                    }
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

        public bool beginGetPlaylist(string name)
        {
            return false;
        }
        public bool supportsPlaylists { get { return false; } }

        public bool writePlaylist(List<string> URLs, string name, bool append)
        {
            return false;
        }

        public bool removePlaylist(string name)
        {
            return false;
        }

        public string getNextPlaylistItem()
        {
            return null;
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
            get { return "RemovableDB"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "The Default Removable Media Database (Sqlite)"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        public RemovableDB() { }
        public RemovableDB(IPluginHost host)
        {
            theHost = host;
            toBeIndexed = new List<string>();
        }
        private IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            toBeIndexed = new List<string>();
            using (PluginSettings settings = new PluginSettings())
            {
                settings.setSetting("Default.RemovableDatabase", "RemovableDB");
            }
            //TODO - index existing removable drives
            lock(this)
                createDB();
            foreach (string drive in Environment.GetLogicalDrives())
            {
                eDriveType type = OSSpecific.getDriveType(drive);
                if ((type == eDriveType.Removable) || (type == eDriveType.Phone) || (type==eDriveType.iPod))
                {
                    foreach (string path in DeviceInfo.getDeviceInfo(drive).MusicFolders)
                        indexDirectory(path, true);
                }
            }
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (!justInserted)
                return;
            if ((type == eMediaType.Smartphone) || (type==eMediaType.AppleDevice) || (type == eMediaType.LocalHardware))
            {
                foreach (string path in DeviceInfo.getDeviceInfo(arg).MusicFolders)
                    indexDirectory(path, true);
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