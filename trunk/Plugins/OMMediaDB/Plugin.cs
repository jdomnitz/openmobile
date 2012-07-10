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
using System.IO;
using System.Text;
using System.Threading;
using Mono.Data.Sqlite;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions;
using OpenMobile.Media;
using OpenMobile.Plugin;

namespace OMMediaDB
{
    public sealed class Plugin:IMediaDatabase
    {
        public SqliteDataReader reader;
        public SqliteConnection con;
        public SqliteConnection bCon;
        private List<string> toBeIndexed;
        private Queue<string> alreadyIndexed;
        Settings s;
        public Settings loadSettings()
        {
            if (s == null)
            {
                s = new Settings("Media Database");
                using (PluginSettings settings = new PluginSettings())
                {
                    s.Add(new Setting(SettingTypes.MultiChoice, "Music.AutoIndex", "", "Index New Music on Every Startup", Setting.BooleanList, Setting.BooleanList, settings.getSetting("Music.AutoIndex")));
                    s.Add(new Setting(SettingTypes.Folder, "Music.Path", "", "Music Path", settings.getSetting("Music.Path")));
                    s.Add(new Setting(SettingTypes.Button,"ClearDB","","Clear Database"));
                    s.Add(new Setting(SettingTypes.Button, "Index", "", "Index Music Now"));
                    s.OnSettingChanged += new SettingChanged(changed);
                }
            }
            return s;
        }
        private void changed(int screen,Setting s)
        {
            using (PluginSettings settings = new PluginSettings())
            {
                if ((s.Name == "Music.AutoIndex")|| (s.Name == "Music.Path"))
                    settings.setSetting(s.Name, s.Value);
                else if (s.Name == "ClearDB")
                {
                    this.clearIndex();
                }
                else if (s.Name == "Index")
                {
                    this.indexDirectory(settings.getSetting("Music.Path"), true);
                }
            }
        }
        public void Dispose()
        {
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
            get { return "Fixed"; }
        }

        public bool indexDirectory(string location, bool subdirectories)
        {
            OpenMobile.Threading.TaskManager.QueueTask(delegate { makeItSo(location, subdirectories); }, ePriority.MediumLow,"Index Music Directory");
            return true;
        }
        public bool clearIndex()
        {
            if (bCon == null)
                bCon = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                bool status;
                using (SqliteCommand cmd = new SqliteCommand("DELETE FROM tblSongs; DELETE FROM tblAlbum", bCon))
                {
                    status= (cmd.ExecuteNonQuery() > 0);
                }
                theHost.raiseMediaEvent(eFunction.MediaDBCleared, null, this.pluginName);
                theHost.SendStatusData(eDataType.PopUp, this, "", "Database Cleared!");
                return status;
            }
        }
        public List<string> listPlaylists()
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            List<string> names = new List<string>();
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                command.CommandText = "SELECT DISTINCT name FROM Playlists";
                reader = command.ExecuteReader();
            }
            while (reader.Read())
                names.Add(reader[0].ToString());
            reader.Close();
            return names;
        }
        private void parseDirectory(string location, bool subdirectories)
        {
            if (string.IsNullOrEmpty(location))
                return;
            parse(location, subdirectories);
        }
        public IMediaDatabase getNew()
        {
            if (theHost == null)
                return null;
            return new Plugin(theHost);
        }
        private void parse(string location, bool subdirectories)
        {
            string[] filters = new string[] { "*.mp3", "*.m4a", "*.mpc", "*.flac", "*.wv", "*.aac", "*.aif", "*.aiff", "*.asf","*.ape", "*.wav", "*.m4p", "*.ogg", "*.wma", "*.oga", "*.spx","*.m4b","*.rma","*.mpp" };
            foreach(string filter in filters)
                foreach (string file in Directory.GetFiles(location, filter))
                    if (toBeIndexed!=null)
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
            Thread Worker = new Thread(getURLList);
            Worker.Name = "OMMediaDB.makeItSo";
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
                bCon = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                using (SqliteCommand cmd = new SqliteCommand("SELECT URL FROM tblSongs", bCon))
                using (SqliteDataReader r = cmd.ExecuteReader())
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
        private string lastURL;
        private string currentURL="";
        private void def(object state)
        {
            if ((toBeIndexed == null)||(toBeIndexed.Count==0))
            {
                if (tmr!=null)
                    tmr.Dispose();
                theHost.raiseMediaEvent(eFunction.MediaIndexingCompleted, null, this.pluginName);
                theHost.SendStatusData(eDataType.Completion, this, "", "Indexing Complete!");
                return;
            }
            if (lastURL == currentURL)
                theHost.SendStatusData(eDataType.Update, this, "", String.Format("Indexing: {0}",System.IO.Path.GetFileName(currentURL)));
            else
                theHost.SendStatusData(eDataType.Update, this, "", String.Format("Indexing: {0}", (((startCount - toBeIndexed.Count) / (double)startCount) * 100).ToString("0.00") + "% (" + (((startCount - toBeIndexed.Count) - lastCount) / 2).ToString() + " songs/sec)"));
            lastURL = currentURL;
            lastCount=(startCount-toBeIndexed.Count);
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
                    currentURL = toBeIndexed[toBeIndexed.Count-1];
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
            if (bCon == null)
                lock (this)
                {
                    bCon = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
                }
            lock (bCon)
            {
                if (bCon.State != ConnectionState.Open)
                    bCon.Open();
                mediaInfo info = TagReader.getInfo(filepath);
                if (info == null)
                    return;
                if (string.IsNullOrEmpty(info.Album))
                    info.Album = "Unknown Album";
                if (string.IsNullOrEmpty(info.Artist))
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
            using (SqliteCommand command = bCon.CreateCommand())
            {
                command.CommandText="UPDATE tblAlbum SET Cover=@cover WHERE ID='"+albumNum+"'";
                System.Drawing.ImageConverter img = new System.Drawing.ImageConverter();
                command.Parameters.Add(new SqliteParameter("@cover", img.ConvertTo(info.coverArt, typeof(byte[]))));
                command.ExecuteNonQuery();
                hasCover = true;
            }
        }

        private bool albumExists(mediaInfo info)
        {
            using (SqliteCommand command = bCon.CreateCommand())
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
            using (SqliteCommand command = bCon.CreateCommand())
            {
                if (string.IsNullOrEmpty(info.Location))
                    return;
                StringBuilder s = new StringBuilder("BEGIN;INSERT INTO tblSongs(Title,URL,AlbumNum,Track,Rating,Genre,Lyrics)VALUES('");
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
                s.Append("');COMMIT");
                command.CommandText = s.ToString();
                command.ExecuteNonQuery();
            }
        }
        private void writeAlbum(mediaInfo info)
        {
            using (SqliteCommand command = bCon.CreateCommand())
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
                albumNum=(long)command.ExecuteScalar();
            }
            album = info.Album;
            artist = info.Artist;
        }

        private string correctArtist(string p)
        {
            using (SqliteCommand command = bCon.CreateCommand())
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
            if (File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2")) == false)
                createDB();
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            lock (con)
            {
                if (con.State != ConnectionState.Open)
                con.Open();
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
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
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
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
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
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum";
                command.CommandText += order;
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByArtist(string artist, bool covers, eMediaField sortBy) 
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'"+order;
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "'"+order;
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByGenre(string genre, bool covers, eMediaField sortBy)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            SqliteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'"+order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Genre='" + General.escape(genre) + "'"+order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByRating(string rating, bool covers, eMediaField sortBy)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            SqliteCommand command = con.CreateCommand();
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'"+order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Rating='" + General.escape(rating) + "'"+order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByAlbum(string artist, string album, bool covers, eMediaField sortBy)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                if (covers == false)
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "'";
                else
                    command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Artist='" + General.escape(artist) + "' AND Album='" + General.escape(album) + "'";
                command.CommandText += order;
                reader = command.ExecuteReader();
            }
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetSongsByLyrics(string phrase, bool covers, eMediaField sortBy)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SqliteCommand command = con.CreateCommand();
            string order = "";
            switch (sortBy)
            {
                case eMediaField.Title:
                    order = " ORDER BY title";
                    break;
                case eMediaField.Track:
                    order = " ORDER BY track";
                    break;
                case eMediaField.Rating:
                    order = " ORDER BY rating";
                    break;
            }
            if (covers == false)
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'"+order;
            else
                command.CommandText = "SELECT Artist,Rating,Album,Title,URL,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE Lyrics LIKE '%" + General.escape(phrase) + "%'"+order;
            reader = command.ExecuteReader();
            field = eMediaField.Title;
            rCover = covers;
            return (reader != null);
        }
        public bool beginGetGenres()
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SqliteCommand command = con.CreateCommand();
            command.CommandText = "SELECT Distinct Genre FROM tblSongs";
            reader = command.ExecuteReader();
            field = eMediaField.Genre;
            return (reader != null);
        }
        public bool setRating(mediaInfo info)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SqliteCommand command = con.CreateCommand();
            command.CommandText = "UPDATE tblSongs SET Rating='" + info.Rating.ToString() + "' WHERE URL='" + General.escape(info.Location) + "';";
            return (command.ExecuteNonQuery() > 0);
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
                        try
                        {
                            MemoryStream m = new MemoryStream((byte[])vl);
                            i.coverArt = OImage.FromStream(m);
                        }
                        catch (OutOfMemoryException)
                        {
                            return null;
                        }
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
            return false;
        }

        public bool beginGetPlaylist(string name)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SqliteCommand command = con.CreateCommand();
            command.CommandText = "SELECT URL FROM Playlists WHERE Name='" + General.escape(name) + "'";
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
                    query.Append(General.escape(name));
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
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                command.CommandText = query.ToString();
                reader = command.ExecuteReader();
            }
            return true;
        }

        public bool removePlaylist(string name)
        {
            StringBuilder query = new StringBuilder("BEGIN;");
            {
                query.Append("DELETE FROM Playlists WHERE Name='");
                query.Append(General.escape(name));
                query.Append("';");
                query.Append("END;");
            }
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                command.CommandText = query.ToString();
                command.ExecuteNonQuery();
            }
            return true;
        }
        
        public mediaInfo getNextPlaylistItem()
        {
            if ((reader==null)||(reader.Read() == false))
            {
                return null;
            }
            return new mediaInfo(reader[0].ToString());
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
            get { return 0.9F; }
        }

        public string pluginDescription
        {
            get { return "The Default Media Database (Sqlite)"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
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
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            alreadyIndexed = new Queue<string>();
            toBeIndexed = new List<string>();
            PluginSettings settings = new PluginSettings();
            if (File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2")) == false)
            {
                settings.setSetting("Default.MusicDatabase", "OMMediaDB");
                createDB();
            }

            // Should we set default settings?
            if (settings.getSetting("Music.Path") == "")
            {   // Yes
                foreach (DeviceInfo device in DeviceInfo.EnumerateDevices(theHost))
                    if (device.systemDrive)
                    {
                        if (device.MusicFolders.Length > 0)
                        {   // Save reference to first available source
                            settings.setSetting("Music.Path", device.MusicFolders[0]);
                            settings.setSetting("OMMediaDB.FirstRun", "True");
                        }
                    }
            }

            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                string path = null;
                using (PluginSettings settings = new PluginSettings())
                {
                    if (settings.getSetting("Music.AutoIndex") == "True")
                        path = settings.getSetting("Music.Path");
                    settings.setSetting("Default.MusicDatabase", "OMMediaDB");
                    if (settings.getSetting("OMMediaDB.FirstRun") == "True")
                    {
                        path = settings.getSetting("Music.Path");
                        settings.setSetting("OMMediaDB.FirstRun", "False");
                    }
                }
                if ((path != null) && (path.Length > 0))
                    indexDirectory(path, true);
            }
        }

        private void createDB()
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            SqliteCommand cmd = new SqliteCommand("BEGIN TRANSACTION;CREATE TABLE tblAlbum(ID INTEGER PRIMARY KEY,Album TEXT, Artist TEXT, Cover BLOB);CREATE TABLE tblSongs (AlbumNum INTEGER, Device INTEGER, Genre TEXT, Rating NUMERIC, Title TEXT, Track INTEGER, URL TEXT, Lyrics TEXT);CREATE TABLE Playlists (Name TEXT, URL TEXT);COMMIT;", con);
            cmd.ExecuteNonQuery();
        }
        #endregion
    }
}
