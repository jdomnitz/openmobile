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
                    s.Add(new Setting(SettingTypes.MultiChoice, "Music.AutoIndex", "", "Index New Music on Every Startup", Setting.BooleanList, Setting.BooleanList, settings.getSetting(this, "Music.AutoIndex")));
                    s.Add(new Setting(SettingTypes.Folder, "Music.Path", "", "Music Path", settings.getSetting(this, "Music.Path")));
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
                if ((s.Name == "Music.AutoIndex") || (s.Name == "Music.Path"))
                    settings.setSetting(this, s.Name, s.Value);
                else if (s.Name == "ClearDB")
                {
                    this.clearIndex();
                }
                else if (s.Name == "Index")
                {
                    this.indexDirectory(settings.getSetting(this, "Music.Path"), true);
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

                // Get amount of records to be deleted
                long RecordCountStart = 0;
                using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM tblSongs", bCon))
                    RecordCountStart = (long)cmd.ExecuteScalar();

                using (SqliteCommand cmd = new SqliteCommand("DELETE FROM tblSongs; DELETE FROM tblAlbum", bCon))
                    status = (cmd.ExecuteNonQuery() > 0);

                // Get amount of records after deletion
                long RecordCountEnd = 0;
                using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM tblSongs", bCon))
                    RecordCountEnd = (long)cmd.ExecuteScalar();

                theHost.raiseMediaEvent(eFunction.MediaDBCleared, null, this.pluginName);
                //theHost.SendStatusData(eDataType.PopUp, this, "", "Database Cleared!");
                theHost.UIHandler.AddNotification(new Notification(this, null, theHost.getSkinImage("Icons|Icon-MusicIndexer").image, "Database Cleared!", String.Format("{0} records removed from database", RecordCountStart - RecordCountEnd)));

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
            abc +=new TimerCallback(UpdateIndexStatus);
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
        private Notification NotificationIndexingStatus = null;
        long RecordCountStart = 0;
        private bool IndexingCompleted = false;

        private void UpdateIndexStatus(object state)
        {

            if (IndexingCompleted)
            {
                if (NotificationIndexingStatus == null)
                {   // Create new notification
                    NotificationIndexingStatus = new Notification(this, "NotificationIndexingStatus", theHost.getSkinImage("Icons|Icon-MusicIndexer").image, theHost.getSkinImage("Icons|Icon-MusicIndexer").image, "Media search completed", "");
                    NotificationIndexingStatus.Global = true;
                    NotificationIndexingStatus.State = Notification.States.Active;
                    theHost.UIHandler.AddNotification(NotificationIndexingStatus);
                }

                if (tmr!=null)
                    tmr.Dispose();
                theHost.raiseMediaEvent(eFunction.MediaIndexingCompleted, null, this.pluginName);

                long RecordCountEnd = 0;
                try
                {
                    // Get amount of records
                    using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM tblSongs", bCon))
                        RecordCountEnd = (long)cmd.ExecuteScalar();
                }
                catch { }
                // Update notification
                NotificationIndexingStatus.Header = "Media search completed";
                long RecordCountNew = RecordCountEnd - RecordCountStart;

                // Set notification as passive to allow user to clear the message
                NotificationIndexingStatus.State = Notification.States.Passive;

                if (RecordCountNew > 0)
                {
                    if (RecordCountNew > 1)
                        NotificationIndexingStatus.Text = String.Format("{0} new songs indexed.", RecordCountEnd - RecordCountStart);
                    else
                        NotificationIndexingStatus.Text = String.Format("{0} new song indexed.", RecordCountEnd - RecordCountStart);
                }
                else
                {   // Remove notification
                    NotificationIndexingStatus.Text = "All known locations searched, no new songs found.";
                    theHost.UIHandler.RemoveNotification(NotificationIndexingStatus, true);
                }

                RecordCountStart = 0;
                return;
            }
            if (lastURL == currentURL)
            {   // Progress is slow, show name of file we're working with
                NotificationIndexingStatus.Text = String.Format("Prosessing: {0}", System.IO.Path.GetFileName(currentURL));
            }
            else
            {   // Normal status update
                string IndexStatusString = String.Format("Progress: {0}", (((startCount - toBeIndexed.Count) / (double)startCount) * 100).ToString("0.00") + "% (" + (((startCount - toBeIndexed.Count) - lastCount) / 2).ToString() + " songs/sec)");

                if (NotificationIndexingStatus == null)
                {   // Create new notification
                    NotificationIndexingStatus = new Notification(this, "NotificationIndexingStatus", theHost.getSkinImage("Icons|Icon-MusicIndexer").image, theHost.getSkinImage("Icons|Icon-MusicIndexer").image, "Searching for new media", IndexStatusString);
                    NotificationIndexingStatus.Global = true;
                    NotificationIndexingStatus.State = Notification.States.Active;
                    theHost.UIHandler.AddNotification(NotificationIndexingStatus);
                }
                else
                {
                    NotificationIndexingStatus.Text = IndexStatusString;
                }
            }
            lastURL = currentURL;
            lastCount=(startCount-toBeIndexed.Count);
        }
        private void doWork()
        {
            IndexingCompleted = false;
            // Get amount of records
            if (RecordCountStart == 0)
            {
                using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM tblSongs", bCon))
                    RecordCountStart = (long)cmd.ExecuteScalar();
            }

            while (true)
            {
                if (toBeIndexed == null)
                    break;
                
                lock (toBeIndexed)
                {
                    if (toBeIndexed.Count == 0)
                        break;
                    currentURL = toBeIndexed[toBeIndexed.Count-1];
                    toBeIndexed.RemoveAt(toBeIndexed.Count - 1);
                }
                processFile(currentURL);
            }

            IndexingCompleted = true;
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
                    // Set default image if image is missing
                    if (info.coverArt == null)
                        info.coverArt = BuiltInComponents.Host.getSkinImage("Images|Image-UnknownAlbum").image;
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
                        //Try to get image from folder
                        if (info.coverArt == null)
                            info.coverArt = TagReader.getFolderImage(info.Location);
                        // Try to get cover image from lastFM
                        if (info.coverArt == null)
                            info.coverArt = TagReader.getLastFMImage(info.Artist, info.Album);
                        // Set default image if image is missing
                        if (info.coverArt == null)
                            info.coverArt = BuiltInComponents.Host.getSkinImage("Images|Image-UnknownAlbum").image;
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
                    command.CommandText = "SELECT Artist FROM tblAlbum GROUP BY Artist";
                else
                    command.CommandText = "SELECT Artist, Cover FROM tblAlbum GROUP BY Artist"; 
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
                    command.CommandText = "SELECT Artist,Album FROM tblAlbum GROUP BY Album";
                else
                    command.CommandText = "SELECT Artist,Album,Cover FROM tblAlbum GROUP BY Album"; //"SELECT Distinct Artist,Album,Cover FROM tblAlbum";
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
                    command.CommandText = "SELECT Artist,Album FROM tblAlbum WHERE Artist='" + General.escape(artist) + "' GROUP BY Album";//"SELECT Distinct Artist,Album FROM tblAlbum WHERE Artist='" + General.escape(artist) + "'";
                else
                    command.CommandText = "SELECT Artist,Album,Cover FROM tblAlbum WHERE Artist='" + General.escape(artist) + "' GROUP BY Album";
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

        public mediaInfo GetMediaInfoByUrl(string url)
        {
            if (con == null)
                con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
            if (con.State != ConnectionState.Open)
                con.Open();
            lock (con)
            {
                SqliteCommand command = con.CreateCommand();
                command.CommandText = String.Format("SELECT Artist,Rating,Album,Title,URL,Genre,Track,Cover FROM tblAlbum JOIN tblSongs ON ID=AlbumNum WHERE URL = '{0}'", General.escape(url));
                reader = command.ExecuteReader();
            }

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
                i.Artist = reader.GetString(0); // ["Artist"].ToString();
                i.Rating = reader.GetInt16(1); // (int)reader["Rating"];
                i.Album = reader.GetString(2); //["Album"].ToString();
                i.Name = reader.GetString(3); //["Title"].ToString();
                i.Location = reader.GetString(4); //["URL"].ToString();
                i.Genre = reader.GetString(5); //["Genre"].ToString();
                i.TrackNumber = reader.GetInt16(6); // (int)reader["Rating"];

                object vl = reader.GetValue(reader.GetOrdinal("Cover"));
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

/*
        public bool writePlaylist(List<mediaInfo> mediaList, string name, bool append)
        {
            // Each item in the playlist is stored as MediaType|Artist|Album|Name|Location
            StringBuilder query = new StringBuilder("BEGIN;");
            {
                if (append == false)
                {
                    query.Append("DELETE FROM Playlists WHERE Name='");
                    query.Append(General.escape(name));
                    query.Append("';");
                }
                foreach (mediaInfo info in mediaList)
                {
                    string mediaString = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", info.Type, info.Artist, info.Album, info.Name, info.Location);
                    query.Append("INSERT INTO Playlists VALUES('");
                    query.Append(General.escape(name));
                    query.Append("','");
                    query.Append(General.escape(mediaString));
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
*/
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

            // old style
            return new mediaInfo(reader[0].ToString());
        }

        //public List<mediaInfo> getPlayList(string name)
        //{
        //    if (con == null)
        //        con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;");
        //    if (con.State != ConnectionState.Open)
        //        con.Open();
        //    SqliteCommand command = con.CreateCommand();
        //    command.CommandText = "SELECT URL FROM Playlists WHERE Name='" + General.escape(name) + "'";
        //    reader = command.ExecuteReader();
            
        //    // Cancel if list not found
        //    if ((reader == null) || (reader.Read() == false))
        //        return new List<mediaInfo>();

        //    // New or old style playlist?

        //    while (reader.Read())
        //    {

        //        if (reader[0].ToString().Contains("|"))
        //        {   // New style: Each item in the playlist is stored as MediaType|Artist|Album|Name|Location
        //            mediaInfo media = new mediaInfo();

        //            // Extract media string
        //            string[] mediaString = reader[0].ToString().Split('|');

        //            // Extract media type
        //            if (mediaString.Length >= 1 && !String.IsNullOrEmpty(mediaString[0]))
        //                media.Type = (eMediaType)Enum.Parse(typeof(eMediaType), mediaString[0], true);

        //            // Extract artist
        //            if (mediaString.Length >= 2 && !String.IsNullOrEmpty(mediaString[1]))
        //                media.Artist = mediaString[2];

        //            // Extract artist
        //            if (mediaString.Length >= 2 && !String.IsNullOrEmpty(mediaString[1]))
        //                media.Artist = mediaString[2];

        //            // Extract artist
        //            if (mediaString.Length >= 2 && !String.IsNullOrEmpty(mediaString[1]))
        //                media.Artist = mediaString[2];


        //        }
        //        else
        //        {
        //            // old style
        //            return new mediaInfo(reader[0].ToString());
        //        }
        //    }


        //}

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

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-MusicIndexer"); }
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
                settings.setSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase", "OMMediaDB");
                createDB();
            }

            // Should we set default settings?
            if (settings.getSetting(BuiltInComponents.OMInternalPlugin, "Music.Path") == "")
            {   // Yes
                foreach (DeviceInfo device in DeviceInfo.EnumerateDevices(theHost))
                    if (device.systemDrive)
                    {
                        if (device.MusicFolders.Length > 0)
                        {   // Save reference to first available source
                            settings.setSetting(BuiltInComponents.OMInternalPlugin, "Music.Path", device.MusicFolders[0]);
                            settings.setSetting(this, "OMMediaDB.FirstRun", "True");
                        }
                    }
            }

            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                string path = null;
                using (PluginSettings settings = new PluginSettings())
                {
                    if (settings.getSetting(this, "Music.AutoIndex") == "True")
                        path = settings.getSetting(BuiltInComponents.OMInternalPlugin, "Music.Path");
                    settings.setSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase", "OMMediaDB");
                    if (settings.getSetting(this, "OMMediaDB.FirstRun") == "True")
                    {
                        path = settings.getSetting(this, "Music.Path");
                        settings.setSetting(this, "OMMediaDB.FirstRun", "False");
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
