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
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.helperFunctions;
using OpenMobile.Media;
using OpenMobile.Plugin;

namespace OMMediaDB
{
    public sealed class Plugin:IMediaDatabase
    {
        public SQLiteDataReader reader;
        public SQLiteConnection con;
        public SQLiteConnection bCon;
        private List<string> toBeIndexed;
        private Queue<string> alreadyIndexed;

        public void Dispose()
        {
            this.toBeIndexed = null;
            if (this.con!=null)
                this.con.Dispose();
            if (this.bCon != null)
                this.bCon.Dispose();
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
            get { return "Music Database"; }
        }

        public bool indexDirectory(string location, bool subdirectories)
        {
            OpenMobile.Threading.TaskManager.QueueTask(delegate { makeItSo(location, subdirectories); }, ePriority.MediumLow);
            return true;
        }
        public bool clearIndex()
        {
            if (bCon == null)
                bCon = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM tblSongs; DELETE FROM tblAlbum", bCon))
                {
                    return (cmd.ExecuteNonQuery() > 0);
                }
            }
        }
        private void parseDirectory(string location, bool subdirectories)
        {
            string[] filter = new string[] { "*.mp3", "*.m4a", "*.aac", "*.aif", "*.wav", "*.m4p", "*.ogg", "*.wma" };
            for(int i=0;i<filter.Length;i++)
                parse(location, subdirectories,filter[i]);
        }
        public IMediaDatabase getNew()
        {
            return new Plugin(theHost);
        }
        private void parse(string location, bool subdirectories,string filter)
        {
            foreach (string file in Directory.GetFiles(location, filter))
                if (toBeIndexed!=null)
                    toBeIndexed.Add(file);
            if (subdirectories == true)
                foreach (string folder in Directory.GetDirectories(location))
                    parse(folder, true,filter);
        }

        private int startCount;
        ManualResetEvent task1 = new ManualResetEvent(false);
        private void makeItSo(string location, bool subdirectories)
        {
            //Phase 1 - Find out what needs to be indexed
            Thread Worker = new Thread(new ThreadStart(getURLList));
            Worker.Priority = ThreadPriority.BelowNormal;
            Worker.IsBackground = true;
            Worker.Start();
            parseDirectory(location, subdirectories);
            //Phase 2 - Remove everything already indexed from the queue
            task1.WaitOne();
            task1.Reset();
            while (alreadyIndexed.Count > 0)
            {
                toBeIndexed.Remove(alreadyIndexed.Dequeue());
            }
            //Phase 3 - Extract info from everything unindexed
            startCount = toBeIndexed.Count;
            abc +=new TimerCallback(def);
            tmr=new System.Threading.Timer(abc,null,0,2000);
            doWork();
        }

        private void getURLList()
        {
            if (bCon == null)
                bCon = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT URL FROM tblSongs", bCon))
                using (SQLiteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read() == true)
                    {
                        alreadyIndexed.Enqueue(r[0].ToString());
                    }
                }
            }
            task1.Set();
        }
        private System.Threading.Timer tmr;
        private TimerCallback abc;
        private int lastCount;

        private void def(object state)
        {
            if ((toBeIndexed == null)||(toBeIndexed.Count==0))
            {
                tmr.Dispose();
                theHost.execute(eFunction.backgroundOperationStatus, "Indexing Complete!");
                return;
            }
            theHost.execute(eFunction.backgroundOperationStatus, (((startCount - toBeIndexed.Count) / (double)startCount) * 100).ToString("0.00") + "% (" + (((startCount - toBeIndexed.Count)-lastCount) / 2).ToString() + " songs/sec)");
            lastCount=(startCount-toBeIndexed.Count);
        }
        private void doWork()
        {
            string s;
            while (true)
            {
                if (toBeIndexed == null)
                    return;
                lock (toBeIndexed)
                {
                    if (toBeIndexed.Count == 0)
                        return;
                    s = toBeIndexed[toBeIndexed.Count-1];
                    toBeIndexed.RemoveAt(toBeIndexed.Count - 1);
                }
                processFile(s);
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
            if (bCon == null)
                lock (this)
                {
                    bCon = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
                }
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                mediaInfo info = TagReader.getInfo(filepath);
                if (info == null)
                    return;
                if ((info.Album == null) || (info.Album == ""))
                    info.Album = "Unknown Album";
                if ((info.Artist == null) || (info.Artist == ""))
                    info.Artist = "Unknown Artist";
                if ((info.Album == album)&&(info.Artist==artist))
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
                        if (info.coverArt == null)
                            info.coverArt = TagReader.getLastFMImage(info.Artist, info.Album);
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
            using (SQLiteCommand command = bCon.CreateCommand())
            {
                command.CommandText="UPDATE tblAlbum SET Cover=@cover WHERE ID='"+albumNum+"'";
                System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                command.Parameters.Add(new SQLiteParameter("@cover", img.ConvertTo(info.coverArt, typeof(byte[]))));
                command.ExecuteNonQuery();
                hasCover = true;
            }
        }

        private bool albumExists(mediaInfo info)
        {
            using (SQLiteCommand command = bCon.CreateCommand())
            {
                StringBuilder s = new StringBuilder("SELECT ID FROM tblAlbum WHERE UPPER(Artist)='");
                s.Append(General.escape(info.Artist.ToUpper()) + "' AND UPPER(Album)='");
                s.Append(General.escape(info.Album.ToUpper()) + "'");
                command.CommandText = s.ToString();
                object id=command.ExecuteScalar();
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
            using (SQLiteCommand command = bCon.CreateCommand())
            {
                if ((info.Location == null) || (info.Location == ""))
                    return;
                StringBuilder s = new StringBuilder("INSERT INTO tblSongs(Title,URL,AlbumNum)VALUES('");
                s.Append(General.escape(info.Name));
                s.Append("','");
                s.Append(General.escape(info.Location));
                s.Append("','");
                s.Append(albumNum.ToString());
                s.Append("')");
                command.CommandText = s.ToString();
                command.ExecuteNonQuery();
            }
        }
        private void writeAlbum(mediaInfo info)
        {
            using (SQLiteCommand command = bCon.CreateCommand())
            {
                StringBuilder s;
                if (info.coverArt != null)
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Artist,Cover)VALUES('");
                else
                    s = new StringBuilder("INSERT INTO tblAlbum(Album,Artist)VALUES('");
                s.Append(General.escape(info.Album));
                s.Append("','");
                info.Artist = correctArtist(info.Artist);
                if (info.coverArt != null)
                {
                    s.Append(General.escape(info.Artist));
                    s.Append("',@cover)");
                    System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                    command.Parameters.Add(new SQLiteParameter("@cover", img.ConvertTo(info.coverArt, typeof(byte[]))));
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
                albumNum=(long)command.ExecuteScalar();
            }
            album = info.Album;
            artist = info.Artist;
        }

        private string correctArtist(string p)
        {
            using (SQLiteCommand command = bCon.CreateCommand())
            {
                StringBuilder s = new StringBuilder("SELECT Artist FROM tblAlbum WHERE UPPER(Artist)='");
                s.Append(General.escape(p.Replace("  "," ").ToUpper()) + "'");
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
            if (File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMMedia")) == false)
                createDB();
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            lock (con)
            {
                if (con.State != ConnectionState.Open)
                con.Open();
                SQLiteCommand command = con.CreateCommand();
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
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
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
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
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
        public bool beginGetSongs(bool covers)
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByArtist(string artist, bool covers) 
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByGenre(string genre, bool covers)
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'";
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'";
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByRating(string rating, bool covers)
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'";
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'";
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByAlbum(string artist, string album, bool covers)
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "'";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "'";
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByLyrics(string phrase, bool covers)
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'";
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'";
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetGenres()
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = "SELECT Distinct Genres FROM tblSongs";
            reader = command.ExecuteReader();
            field = eMediaField.Genre;
            return (reader != null);
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
            catch (Exception) {
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
                    object vl=reader.GetValue(reader.GetOrdinal("Cover"));
                    
                    if (vl.GetType() != typeof(DBNull))
                    {
                        MemoryStream m = new MemoryStream((byte[])vl);
                        i.coverArt = Image.FromStream(m);
                    }
                }
            return i;
            }
        }

        public bool endSearch()
        {
            if (reader!=null)
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
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = "SELECT URL FROM Playlist WHERE Name='" + General.escape(name) + "'";
            reader = command.ExecuteReader();
            return (reader != null);
        }
        public bool supportsPlaylists { get { return true; } }

        public bool writePlaylist(List<string> URLs, string name, bool append)
        {
            StringBuilder query = new StringBuilder("BEGIN;");
            {
                if (append == false)
                {
                    query.Append("DELETE FROM Playlists WHERE Name='");
                    query.Append(name);
                    query.Append("';");
                }
                foreach (string url in URLs)
                {
                    query.Append("INSERT INTO Playlists VALUES('");
                    query.Append(General.escape(name));
                    query.Append("','");
                    query.Append(General.escape(url));
                    query.Append("');");
                }
                query.Append("END;");
            }
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SQLiteCommand command = con.CreateCommand();
                command.CommandText = query.ToString();
                reader = command.ExecuteReader();
            }
            return true;
        }

        public string getNextPlaylistItem()
        {
            if ((reader==null)||(reader.Read() == false))
            {
                return null;
            }
            return reader[0].ToString();
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
            get { return "OMMediaDB"; }
        }

        public float pluginVersion
        {
            get { return 0.3F; }
        }

        public string pluginDescription
        {
            get { return "The default Media Database (SQLITE)"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        public Plugin(){}
        public Plugin(IPluginHost host)
        {
            theHost = host;
            alreadyIndexed = new Queue<string>();
            toBeIndexed = new List<string>();
        }
        private IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            alreadyIndexed = new Queue<string>();
            toBeIndexed = new List<string>();
            string path=null;
            using(PluginSettings settings=new PluginSettings())
            {
                if (settings.getSetting("Music.AutoIndex") == "True")
                    path = settings.getSetting("Music.Path");
            }
            if ((path!=null)&&(path.Length>0))
                indexDirectory(path, true);
            if (File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMMedia")) == false)
                createDB();
            return eLoadStatus.LoadSuccessful;
        }

        private void createDB()
        {
            if (con == null)
                con = new SQLiteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SQLiteCommand cmd = new SQLiteCommand("BEGIN TRANSACTION;CREATE TABLE tblAlbum(ID INTEGER PRIMARY KEY,Album TEXT, Artist TEXT, Cover BLOB);CREATE TABLE tblSongs (AlbumNum INTEGER, Device INTEGER, Genre TEXT, Rating NUMERIC, Title TEXT, URL TEXT);CREATE TABLE Playlists (Name TEXT, URL TEXT);COMMIT;", con);
            cmd.ExecuteNonQuery();
        }
        #endregion
    }
}
