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
using System.Data.SQLite;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.SQLite;
using OpenMobile.Media;
using OpenMobile.Plugin;
using Dapper;

namespace OMMediaDB2
{
    public sealed class OMMediaDB2 : BasePluginCode, IMediaDatabase
    {
        public OMMediaDB2()
            : base("OMMediaDB2", OM.Host.getSkinImage("Icons|Icon-MusicIndexer"), 1f, "The default media database for OM", "OM Dev. Team", "")
        {
        }

        #region Private variables

        /// <summary>
        /// The different types of objects the database can store
        /// </summary>
        private enum DBObjectTypes
        {
            /// <summary>
            /// Item indexed by the database (Default type)
            /// </summary>
            IndexedItem = 0,

            /// <summary>
            /// Playlist item
            /// </summary>
            PlaylistItem = 1,

            /// <summary>
            /// A playlist setting
            /// </summary>
            PlaylistSetting = 2
        }

        /// <summary>
        /// Required dataBase version, a higher number here than what's currently in the DB will cause the DB to be rebuilt (possible loosing data)
        /// </summary>
        private const double _DBVersion = 2.1d;

        /// <summary>
        /// The full filepath to the database file
        /// </summary>
        static private string _DBFile = String.Empty;

        /// <summary>
        /// The name of the database file
        /// </summary>
        static private string _DBName = String.Empty;

        /// <summary>
        /// True = The database file is valid
        /// </summary>
        static private bool _DBValid = false;

        /// <summary>
        /// The SQLite database connection object
        /// </summary>
        private SQLiteConnection _DBConnection;

        /// <summary>
        /// Notification for indexing status
        /// </summary>
        private Notification _NotificationIndexingStatus = null;

        /// <summary>
        /// The local reader object
        /// </summary>
        private SQLiteDataReader _DBReader;

        /// <summary>
        /// Is cover retrival from folders enabled?
        /// </summary>
        private bool _FolderCovers_Enabled = true;

        /// <summary>
        /// Is LastFM enabled?
        /// </summary>
        private bool _LastFM_Enabled = true;

        /// <summary>
        /// Coversize to prioritize when retreving data from LastFM
        /// </summary>
        private TagReader.LastFMCoverSize _LastFM_CoverSize = TagReader.LastFMCoverSize.XLarge;

        /// <summary>
        /// The skin relative path to the missing covers image
        /// </summary>
        private string _Cover_MissingImagePath = String.Empty;

        /// <summary>
        /// Automatically index files at startup
        /// </summary>
        private bool _Index_AutoIndex = false;

        #endregion

        #region Base plugin overrides

        public override eLoadStatus initialize(IPluginHost host)
        {
            _DBName = base.pluginName;
            _DBFile = OpenMobile.Path.Combine(OM.Host.DataPath, _DBName);

            // Set default database 
            StoredData.SetDefaultValue(OM.GlobalSetting, "Default.MusicDatabase", _DBName);
            StoredData.Set(OM.GlobalSetting, "Default.MusicDatabase", _DBName);
            
            // Set default music location?
            if (StoredData.Get(this, "Music.Path") == "")
            {   // Yes
                foreach (DeviceInfo device in DeviceInfo.EnumerateDevices(OM.Host))
                {
                    if (device.systemDrive)
                    {
                        if (device.MusicFolders.Length > 0)
                        {   // Save reference to first available source
                            StoredData.Set(this, "Music.Path", device.MusicFolders[0]);
                            break;
                        }
                    }
                }
            }

            // Check for available DB file, if not create it
            if (!File.Exists(_DBFile))
            {   // This is a fresh start, let's create the file and set it as default
                StoredData.Set(OM.GlobalSetting, "Default.MusicDatabase", _DBName);
                DB_Create(_DBVersion);
            }

            if (!DB_CheckVersion(_DBVersion))
            {
                _DBValid = false;
                OM.Host.UIHandler.AddNotification(new Notification( Notification.Styles.Warning, this, "DB_Invalid", null, String.Format("Invalid database for {0}", base.pluginName), String.Format("The database file was was recreated! Please check settings/data for {0}.", base.pluginName)));

                // Delete current DB file
                DB_Delete();

                // Recreate DB
                StoredData.Set(OM.GlobalSetting, "Default.MusicDatabase", _DBName);
                DB_Create(_DBVersion);
            }

            if (!_DBValid)
                return eLoadStatus.LoadFailedUnloadRequested;

            // Connect to host events
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            // Set default settings values
            Settings_SetDefaultValues();

            return eLoadStatus.LoadSuccessful;
        }

        public override void Dispose()
        {
            endSearch();
            DB_Close();
            base.Dispose();
        }

        #endregion

        #region Settings

        public override void Settings()
        {
            base.MySettings.Add(Setting.FolderSelection("Music.Path", String.Empty, "Music Path", StoredData.Get(this, "Music.Path")));
            base.MySettings.Add(Setting.BooleanSetting("FolderCovers.Enabled", String.Empty, "Enable cover retrival from folders", StoredData.Get(this, "FolderCovers.Enabled")));
            base.MySettings.Add(Setting.BooleanSetting("LastFM.Enabled", String.Empty, "Enable LastFM meta data retrival", StoredData.Get(this, "LastFM.Enabled")));
            base.MySettings.Add(Setting.EnumSetting<TagReader.LastFMCoverSize>("LastFM.Cover.Size", String.Empty, "Prioritized cover size", StoredData.Get(this, "LastFM.Cover.Size")));
            base.MySettings.Add(Setting.TextEntry("Cover.MissingImage", "Missing cover image", "Image is relative to the skin's folder (Use '|' as folder separator)", StoredData.Get(this, "Cover.MissingImage")));
            base.MySettings.Add(Setting.BooleanSetting("Index.AutoIndex", String.Empty, "Automatically index files at startup", StoredData.Get(this, "Index.AutoIndex")));
            base.MySettings.Add(Setting.ButtonSetting("Index.IndexNow", "Index files now"));
            base.MySettings.Add(Setting.ButtonSetting("Index.Clear", "Clear database"));
        }

        private void Settings_MapVariables()
        {
            _FolderCovers_Enabled = StoredData.GetBool(this, "FolderCovers.Enabled");
            _LastFM_Enabled = StoredData.GetBool(this, "LastFM.Enabled");
            _LastFM_CoverSize = (TagReader.LastFMCoverSize)StoredData.GetInt(this, "LastFM.Enabled");
            _Cover_MissingImagePath = StoredData.Get(this, "Cover.MissingImage");
            _Index_AutoIndex = StoredData.GetBool(this, "Index.AutoIndex");
        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, "Index.AutoIndex", true);
            StoredData.SetDefaultValue(this, "FolderCovers.Enabled", true);
            StoredData.SetDefaultValue(this, "LastFM.Enabled", true);
            StoredData.SetDefaultValue(this, "LastFM.Cover.Size", TagReader.LastFMCoverSize.XLarge);
            StoredData.SetDefaultValue(this, "Cover.MissingImage", "Images|Image-UnknownAlbum");

            Settings_MapVariables();
        }

        public override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);

            Settings_MapVariables();

            if (setting.Name.Equals("Index.IndexNow"))
                IndexStart();
            else if (setting.Name.Equals("Index.Clear"))
                DB_ClearMediaInfoData(DBObjectTypes.IndexedItem);
        }

        #endregion

        #region Host methods

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                IndexStart();
            }
        }

        #endregion

        #region File Indexing methods

        private void IndexStart()
        {
            OpenMobile.Threading.TaskManager.QueueTask(delegate
            {
                IndexFiles(StoredData.Get(this, "Music.Path"));
            }, ePriority.MediumLow, String.Format("{0}:Index Music", base.pluginName));
        }

        /// <summary>
        /// Indexes all files in a given location to the database
        /// </summary>
        /// <param name="location"></param>
        private bool IndexFiles(string location, bool subdirectories = true, bool showNotifications = true)
        {
            Queue<string> knownLocations = DB_GetKnownLocations(DBObjectTypes.IndexedItem);
            List<string> locationsToIndex = IndexFiles_ParseLocation(location, subdirectories);

            // Remove already index locations
            while (knownLocations.Count > 0)
                locationsToIndex.Remove(knownLocations.Dequeue());

            // Create new notification
            if (showNotifications)
            {
                _NotificationIndexingStatus = new Notification(this, "NotificationIndexingStatus", OM.Host.getSkinImage("Icons|Icon-MusicIndexer").image, OM.Host.getSkinImage("Icons|Icon-MusicIndexer").image, "Searching for new media", "");
                _NotificationIndexingStatus.Global = true;
                _NotificationIndexingStatus.State = Notification.States.Active;
                OM.Host.UIHandler.AddNotification(_NotificationIndexingStatus);
            }

            int amountToBeIndexed = locationsToIndex.Count;
            int amountNew = 0;
            // Index files
            while (locationsToIndex.Count > 0)
            {
                if (showNotifications)
                {
                    string IndexStatusString = String.Format("Progress: {0}", ((amountToBeIndexed - locationsToIndex.Count) / (double)amountToBeIndexed));
                    _NotificationIndexingStatus.Text = IndexStatusString;
                }

                if (DB_LoadFileData(locationsToIndex[locationsToIndex.Count - 1]))
                    amountNew++;
                locationsToIndex.RemoveAt(locationsToIndex.Count - 1);
            }

            if (showNotifications)
            {
                // Update notification text to user
                if (amountNew > 0)
                {
                    _NotificationIndexingStatus.Text = String.Format("Found {0} items", amountNew);
                    _NotificationIndexingStatus.Header = "Media search completed";
                    _NotificationIndexingStatus.State = Notification.States.Passive;
                }
                else
                {
                    _NotificationIndexingStatus.Text = String.Format("No new items found");
                    _NotificationIndexingStatus.Header = "Media search completed";
                    _NotificationIndexingStatus.State = Notification.States.Passive;
                    OM.Host.UIHandler.RemoveNotification(_NotificationIndexingStatus, true);
                }
            }

            return true;
        }

        /// <summary>
        /// Pareses a location for media files (including subfolders if specified when callin the method)
        /// </summary>
        /// <param name="location"></param>
        /// <param name="subdirectories"></param>
        /// <returns>A list of media files</returns>
        private List<string> IndexFiles_ParseLocation(string location, bool subdirectories)
        {
            List<string> locations = new List<string>();
            string[] filters = new string[] { "*.mp3", "*.m4a", "*.mpc", "*.flac", "*.wv", "*.aac", "*.aif", "*.aiff", "*.asf", "*.ape", "*.wav", "*.m4p", "*.ogg", "*.wma", "*.oga", "*.spx", "*.m4b", "*.rma", "*.mpp" };
            foreach (string filter in filters)
                foreach (string file in Directory.GetFiles(location, filter))
                    locations.Add(file);
            if (subdirectories == true)
                foreach (string folder in Directory.GetDirectories(location))
                    locations.AddRange(IndexFiles_ParseLocation(folder, true));
            return locations;
        }

        #endregion

        #region DataAccess methods

        private bool DB_LoadFileData(string filepath)
        {
            if (!DB_ConnectAndOpen())
                return false;

            mediaInfo info = TagReader.getInfo(filepath);
            if (info == null)
                return false;

            // Try to get cover image from lastFM
            if (info.coverArt == null && _LastFM_Enabled)
                info.coverArt = TagReader.getLastFMImage(info.Artist, info.Album, _LastFM_CoverSize);
            //Try to get image from folder
            if (info.coverArt == null && _FolderCovers_Enabled)
                info.coverArt = TagReader.getFolderImage(info.Location);
            // Set default image if image is missing
            if (info.coverArt == null && !string.IsNullOrEmpty(_Cover_MissingImagePath))
                info.coverArt = BuiltInComponents.Host.getSkinImage(_Cover_MissingImagePath).image;

            if (!DB_InsertMediaInfo(DBObjectTypes.IndexedItem, info))
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if a coverArt with the given ID is already loaded in the database
        /// </summary>
        /// <param name="coverArtID"></param>
        /// <returns></returns>
        private bool DB_ContainsMediaInfo(DBObjectTypes objectType, mediaInfo media, string objectTag = null)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT Location FROM tblMediaInfo WHERE ObjectType=@ObjectType AND Location=@Location AND ObjectTag=@ObjectTag";
                    command.Parameters.AddWithValue("@ObjectType", (int)objectType);
                    command.Parameters.AddWithValue("@Location", media.Location);
                    command.Parameters.AddWithValue("@ObjectTag", objectTag);
                    string Location = command.ExecuteScalar() as string;
                    if (media.Location == Location)
                        return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Writes a mediaInfo object to the database
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        private bool DB_InsertMediaInfo(DBObjectTypes objectType, mediaInfo media, string objectTag = null)
        {
            if (!DB_ConnectAndOpen())
                return false;

            if (DB_ContainsMediaInfo(objectType, media, objectTag))
                return false;

            // Add cover to database (if not already present)
            int coverArtID = DB_InsertCoverArt(media);

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "INSERT INTO tblMediaInfo (ObjectType, ObjectTag, Name, Artist, Album, Location, TrackNumber, Genre, Lyrics, Length, Rating, Type, CoverArtID) " +
                                      "VALUES (@ObjectType, @ObjectTag, @Name, @Artist, @Album, @Location, @TrackNumber, @Genre, @Lyrics, @Length, @Rating, @Type, @CoverArtID)";
                    cmd.Parameters.AddWithValue("@ObjectType", (int)objectType);
                    cmd.Parameters.AddWithValue("@ObjectTag", objectTag);
                    cmd.Parameters.AddWithValue("@Name", media.Name);
                    cmd.Parameters.AddWithValue("@Artist", media.Artist);
                    cmd.Parameters.AddWithValue("@Album", media.Album);
                    cmd.Parameters.AddWithValue("@Location", media.Location);
                    cmd.Parameters.AddWithValue("@TrackNumber", media.TrackNumber);
                    cmd.Parameters.AddWithValue("@Genre", media.Genre);
                    cmd.Parameters.AddWithValue("@Lyrics", media.Lyrics);
                    cmd.Parameters.AddWithValue("@Length", media.Length);
                    cmd.Parameters.AddWithValue("@Rating", media.Rating);
                    cmd.Parameters.AddWithValue("@Type", media.Type);
                    cmd.Parameters.AddWithValue("@CoverArtID", coverArtID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Writes a coverArt image from a mediaInfo object to the database (if not already present)
        /// </summary>
        /// <param name="media"></param>
        /// <returns>ID of coverArt</returns>
        private int DB_InsertCoverArt(mediaInfo media)
        {
            if (!DB_ConnectAndOpen())
                return 0;

            // Ensure we have a valid image
            if (media.coverArt == null || media.coverArt.image == null)
                return 0;

            // Get coverart ID text
            string coverArtIDText = String.Empty;
            if (media.Album != null && media.Album != null && (!String.IsNullOrEmpty(media.Album) || !String.IsNullOrEmpty(media.Artist)))
                coverArtIDText = String.Format("{0}|{1}", media.Album, media.Artist);
            else
                coverArtIDText = String.Format("{0}", media.Location);

            // Get coverart ID
            int coverArtID = coverArtIDText.GetHashCode();

            if (DB_ContainsCoverArt(coverArtID))
                return coverArtID;

            // Create a converter to store the images as byte
            System.Drawing.ImageConverter imgConverter = new System.Drawing.ImageConverter();

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "INSERT INTO tblMediaInfoCoverArt (ID, IDText, CoverArt) VALUES (@ID, @IDText, @CoverArt)";
                    cmd.Parameters.AddWithValue("@ID", coverArtID);
                    cmd.Parameters.AddWithValue("@IDText", coverArtIDText);
                    cmd.Parameters.AddWithValue("@CoverArt", imgConverter.ConvertTo(media.coverArt.image, typeof(byte[])));                    
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return 0;
            }
            return coverArtID;
        }

        /// <summary>
        /// Returns true if a coverArt with the given ID is already loaded in the database
        /// </summary>
        /// <param name="coverArtID"></param>
        /// <returns></returns>
        private bool DB_ContainsCoverArt(int coverArtID)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT ID FROM tblMediaInfoCoverArt WHERE ID =@ID";
                    command.Parameters.AddWithValue("@ID", coverArtID);
                    long? ID = command.ExecuteScalar() as long?;
                    if (ID == coverArtID)
                        return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Get's a list of all locations stored in the database
        /// </summary>
        /// <returns></returns>
        private Queue<string> DB_GetKnownLocations(DBObjectTypes objectType)
        {
            Queue<string> locations = new Queue<string>();

            if (!DB_ConnectAndOpen())
                return locations;

            using (SQLiteCommand cmd = new SQLiteCommand(String.Format("SELECT Location FROM tblMediaInfo WHERE ObjectType='{0}'", (int)objectType), _DBConnection))
                using (SQLiteDataReader r = cmd.ExecuteReader())
                    while (r.Read() == true)
                        locations.Enqueue(r[0].ToString());

            return locations;
        }

        /// <summary>
        /// Gets the SQL string to query for media info based on object type
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private string DB_GetMediaInfoSQLBase(DBObjectTypes objectType)
        {
            return String.Format("SELECT Name, Artist, Album, Location, TrackNumber, Genre, Lyrics, Length, Rating, Type, CoverArt FROM tblMediaInfo JOIN tblMediaInfoCoverArt ON ID=CoverArtID WHERE ObjectType='{0}' ", (int)objectType);
            //return String.Format("SELECT Name, Artist, Album, Location, TrackNumber, Genre, Lyrics, Length, Rating, Type FROM tblMediaInfo WHERE ObjectType='{0}' ", (int)objectType);
        }

        private List<OImage> DB_GetCoversForArtist(string artist, int limit)
        {
            List<OImage> covers = new List<OImage>();

            if (!DB_ConnectAndOpen())
                return covers;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "SELECT Artist, CoverArt, CoverArtID FROM tblMediaInfo JOIN tblMediaInfoCoverArt ON ID=CoverArtID WHERE ObjectType =@ObjectType AND Artist =@Artist GROUP BY CoverArtID";
                    if (limit > 0)
                        cmd.CommandText += " LIMIT " + limit.ToString();
                    cmd.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.IndexedItem);
                    cmd.Parameters.AddWithValue("@Artist", artist);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        // Extract cover art
                        if (reader["CoverArt"] != DBNull.Value)
                        {
                            object coverArt = reader.GetValue(reader.GetOrdinal("CoverArt"));
                            if (coverArt.GetType() != typeof(DBNull))
                            {
                                try
                                {
                                    MemoryStream m = new MemoryStream((byte[])coverArt);
                                    covers.Add(OImage.FromStream(m));
                                }
                                catch (OutOfMemoryException)
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return covers;

        }

        /// <summary>
        /// Extracts a mediaInfo item from a SQLite data reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private mediaInfo DB_GetMediaInfoItem(SQLiteDataReader reader, MediaDataBaseCommands commandType)
        {

            if (reader == null || !reader.Read())
                return null;

            mediaInfo media = new mediaInfo();
            try
            {
                // This setup must match the SQL structure in DB_GetMediInfoSQLBase            
                if (reader[0] != DBNull.Value)
                    media.Name = reader.GetString(0); // Name
                if (reader[1] != DBNull.Value)
                    media.Artist = reader.GetString(1); // Artist
                if (reader[2] != DBNull.Value)
                    media.Album = reader.GetString(2); // Album
                if (reader[3] != DBNull.Value)
                    media.Location = reader.GetString(3); // Location
                if (reader[4] != DBNull.Value)
                    media.TrackNumber = reader.GetInt16(4); // TrackNumber
                if (reader[5] != DBNull.Value)
                    media.Genre = reader.GetString(5); //Genre
                if (reader[6] != DBNull.Value)
                    media.Lyrics = reader.GetString(6); //Lyrics
                if (reader[7] != DBNull.Value)
                    media.Length = reader.GetInt16(7); // Length
                if (reader[8] != DBNull.Value)
                    media.Rating = reader.GetInt16(8); // Rating
                if (reader[9] != DBNull.Value)
                    media.Type = (eMediaType)reader.GetInt16(9); // Type

                // Extract cover art
                if (reader["CoverArt"] != DBNull.Value)
                {
                    object coverArt = reader.GetValue(reader.GetOrdinal("CoverArt"));
                    if (coverArt.GetType() != typeof(DBNull))
                    {
                        try
                        {
                            MemoryStream m = new MemoryStream((byte[])coverArt);
                            media.coverArt = OImage.FromStream(m);
                        }
                        catch (OutOfMemoryException)
                        {
                        }
                    }
                }
            }
            catch
            {
            }

            // Additional processing based on command type
            switch (commandType)
            {
                case MediaDataBaseCommands.GetArtist:
                    {
                        // Replace coverArt with mosaic of artist
                        int albumCount = DB_GetAlbumByArtistCount(media.Artist);
                        List<OImage> covers = DB_GetCoversForArtist(media.Artist, 10);
                        media.coverArt = OpenMobile.helperFunctions.Graphics.Images.CreateMosaic(covers, 200, 200);
                        if (albumCount == 0)
                            media.Name = String.Format("{0} (No albums)", media.Artist);
                        else if (albumCount == 1)
                            media.Name = String.Format("{0} ({1} album)", media.Artist, albumCount);
                        else
                            media.Name = String.Format("{0} ({1} albums)", media.Artist, albumCount);
                        media.Album = String.Empty;
                    }
                    break;
                case MediaDataBaseCommands.GetGenres:
                    {
                        // Replace coverArt with mosaic of artist
                        int albumCount = DB_GetAlbumByArtistCount(media.Artist);
                        List<OImage> covers = DB_GetCoversForArtist(media.Artist, 10);
                        media.coverArt = OpenMobile.helperFunctions.Graphics.Images.CreateMosaic(covers, 200, 200);
                        if (albumCount == 0)
                            media.Album = "(No albums)";
                        else if (albumCount == 1)
                            media.Album = String.Format("{0} album", albumCount);
                        else
                            media.Album = String.Format("{0} albums", albumCount);
                        media.Name = media.Genre;
                        media.Artist = String.Empty;

                        if (String.IsNullOrEmpty(media.Genre))
                            media.Name = "No genre";
                    }
                    break;
                case MediaDataBaseCommands.GetAlbum:
                    {
                        media.Name = media.Album;
                    }
                    break;
            }

            return media;
        }

        /// <summary>
        /// Gets the amount of albums by an artist
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        private int DB_GetAlbumByArtistCount(string artist)
        {
            if (!DB_ConnectAndOpen())
                return 0;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "Select Count(DISTINCT Album) FROM tblMediaInfo WHERE ObjectType =@ObjectType AND Artist LIKE @Artist";
                    command.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.IndexedItem);
                    command.Parameters.AddWithValue("@Artist", artist);
                    long? hits = command.ExecuteScalar() as long?;
                    if (hits.HasValue)
                        return (int)hits;
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks if a DB contains a playlist
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        private bool DB_ContainsPlayList(string playlistName)
        {
            if (DB_GetPlayListItemCount(playlistName) > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the amount of items in a playlist stored in the DB
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        private int DB_GetPlayListItemCount(string playlistName)
        {
            if (!DB_ConnectAndOpen())
                return 0;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT Count(ObjectTag) FROM tblMediaInfo WHERE ObjectType =@ObjectType AND ObjectTag =@ObjectTag GROUP BY ObjectTag";
                    command.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistItem);
                    command.Parameters.AddWithValue("@ObjectTag", playlistName);
                    long? hits = command.ExecuteScalar() as long?;
                    if (hits.HasValue)
                        return (int)hits;
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Reads a setting value from the database
        /// </summary>
        /// <param name="objectTag"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        private string DB_GetMediaSetting(string objectTag, string settingName)
        {
            if (!DB_ConnectAndOpen())
                return null;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT Value FROM tblMediaSettings WHERE ObjectType =@ObjectType AND ObjectTag =@ObjectTag AND Name =@Name ORDER BY ROWID ASC LIMIT 1";
                    command.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistSetting);
                    command.Parameters.AddWithValue("@ObjectTag", objectTag);
                    command.Parameters.AddWithValue("@Name", settingName);
                    return command.ExecuteScalar() as string;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sets a setting value in the database
        /// </summary>
        /// <param name="objectTag"></param>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        private void DB_SetMediaSetting(string objectTag, string settingName, string settingValue)
        {
            if (!DB_ConnectAndOpen())
                return;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    // Try to update
                    cmd.CommandText = "UPDATE tblMediaSettings SET Value=@Value WHERE Name=@Name AND ObjectTag=@ObjectTag AND ObjectType=@ObjectType";
                    cmd.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistSetting);
                    cmd.Parameters.AddWithValue("@ObjectTag", objectTag);
                    cmd.Parameters.AddWithValue("@Name", settingName);
                    cmd.Parameters.AddWithValue("@Value", settingValue);
                    int updateCount = cmd.ExecuteNonQuery();

                    if (updateCount == 0)
                    {   // Update failed, insert instead
                        cmd.CommandText = "INSERT INTO tblMediaSettings (ObjectType, ObjectTag, Name, Value) VALUES (@ObjectType, @ObjectTag, @Name, @Value)";
                        cmd.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistSetting);
                        cmd.Parameters.AddWithValue("@ObjectTag", objectTag);
                        cmd.Parameters.AddWithValue("@Name", settingName);
                        cmd.Parameters.AddWithValue("@Value", settingValue);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                return;
            }
        }


        /// <summary>
        /// Deletes all indexed data from the DB
        /// </summary>
        /// <returns></returns>
        private bool DB_ClearMediaInfoData(DBObjectTypes objectType)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "DELETE FROM tblMediaInfo WHERE ObjectType =@ObjectType";
                    cmd.Parameters.AddWithValue("@ObjectType", (int)objectType);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }

            DB_RemoveUnusedCoverArts();

            return true;

        }

        /// <summary>
        /// Deletes a playlist from the DB
        /// </summary>
        /// <returns></returns>
        private bool DB_DeletePlaylist(string playlistName)
        {
            if (!DB_ConnectAndOpen())
                return false;

            int recordCount = 0;
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "DELETE FROM tblMediaInfo WHERE ObjectType =@ObjectType AND ObjectTag =@ObjectTag";
                    cmd.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistItem);
                    cmd.Parameters.AddWithValue("@ObjectTag", playlistName);
                    recordCount = cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }

            DB_RemoveUnusedCoverArts();

            return true;

        }

        /// <summary>
        /// Removes unused covers from the database
        /// </summary>
        /// <returns></returns>
        private bool DB_RemoveUnusedCoverArts()
        {
            if (!DB_ConnectAndOpen())
                return false;

            List<int> unusedCovers = DB_GetUnusedCoverArtIDs();
            if (unusedCovers.Count == 0)
                return false;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "DELETE FROM tblMediaInfoCoverArt WHERE ID IN (";
                    foreach (int i in unusedCovers)
                        cmd.CommandText += String.Format("{0},", i);
                    cmd.CommandText = cmd.CommandText.TrimEnd(',');
                    cmd.CommandText += ")";
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get's a list of unused covers is'd
        /// </summary>
        /// <returns></returns>
        private List<int> DB_GetUnusedCoverArtIDs()
        {
            List<int> IDs = new List<int>();

            if (!DB_ConnectAndOpen())
                return IDs;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(_DBConnection))
                {
                    cmd.CommandText = "SELECT t1.ID, t1.IDText FROM tblMediaInfoCoverArt t1 LEFT JOIN tblMediaInfo t2 ON t1.ID = t2.CoverArtID WHERE t2.CoverArtID IS NULL";
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                        IDs.Add(reader.GetInt32(0));
                }
            }
            catch
            {                
            }

            return IDs;
        }

        #endregion

        #region DB methods

        /// <summary>
        /// Creates the database file and it's tables
        /// </summary>
        private void DB_Create(double dbVersion)
        {
            if (!DB_ConnectAndOpen())
                return;

            if (!_DBValid)
                return;

            SQLiteCommand cmd = new SQLiteCommand(
                "BEGIN TRANSACTION;" +
                "CREATE TABLE tblDBInfo (Owner TEXT, Description TEXT, Version TEXT);" +
                String.Format("INSERT INTO tblDBInfo VALUES('{0}', '{1}', '{2}');", base.pluginName, base.pluginDescription, dbVersion.ToString().Replace(',','.')) +
                "CREATE TABLE tblMediaInfoCoverArt (ID INTEGER UNIQUE, IDText TEXT, CoverArt BLOB);" +
                "CREATE TABLE tblMediaInfo (ObjectType INTEGER, ObjectTag TEXT NULL, Name TEXT, Artist TEXT, Album TEXT, Location TEXT, TrackNumber INTEGER, Genre TEXT, Lyrics TEXT, Length NUMERIC, Rating NUMERIC, Type NUMERIC, CoverArtID INTEGER);" +
                "CREATE TABLE tblMediaSettings (ObjectType INTEGER, ObjectTag TEXT NULL, Name TEXT, Value TEXT NULL);" +
                "COMMIT;"
                , _DBConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                _DBValid = false;
            }
        }

        /// <summary>
        /// Connects to the database and opens it
        /// </summary>
        private bool DB_ConnectAndOpen()
        {
            try
            {
                if (_DBConnection == null)
                    _DBConnection = new SQLiteConnection(String.Format(@"Data Source={0};Pooling=false;synchronous=0;", _DBFile));
                if (_DBConnection.State != ConnectionState.Open)
                {
                    _DBConnection.Open();
                    _DBValid = true;
                }
                
                if (_DBConnection.State == ConnectionState.Open)
                    _DBValid = true;
            }
            catch
            {
                _DBValid = false;
            }

            return _DBValid;
        }

        /// <summary>
        /// Checks if the required version of the database is correct
        /// </summary>
        /// <param name="minVersion"></param>
        private bool DB_CheckVersion(double minVersion)
        {
            if (!DB_ConnectAndOpen())
                return false;

            using (SQLiteCommand command = _DBConnection.CreateCommand())
            {
                command.CommandText = "SELECT Version FROM tblDBInfo"; 
                object returnValue = command.ExecuteScalar();
                double dec = 0;
                Double.TryParse((string)returnValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dec);
                //double dec = Convert.ToDouble(returnValue, System.Globalization.CultureInfo.InvariantCulture);
                if (dec >= minVersion)
                    return true;
            }
            return false;
        }

        private SQLiteDataReader DB_ExecuteCommand(string SQL)
        {
            SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection);
            try
            {
                return cmd.ExecuteReader();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Closes the DB connection
        /// </summary>
        private void DB_Close()
        {
            _DBConnection.Close();
        }

        /// <summary>
        /// Deletes the DB file
        /// </summary>
        private void DB_Delete()
        {
            DB_Close();
            try
            {
                File.Delete(_DBFile);
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg(String.Format("Unable to delete database file {0}", _DBFile), ex);
            }
        }

        #endregion

        #region IMediaDatabase

        private enum MediaDataBaseCommands
        {
            None,
            GetSongs,
            GetArtist,
            GetAlbum,
            GetGenres,
            GetPlaylists
        }
        private MediaDataBaseCommands _BeginCommand = MediaDataBaseCommands.None;

        public string databaseType
        {
            get { return "Fixed"; }
        }

        public bool beginGetArtists(string artistFilter = "", bool covers = true)
        {
            if (!DB_ConnectAndOpen())
                return false;

            string SQL = DB_GetMediaInfoSQLBase(DBObjectTypes.IndexedItem);
            if (!String.IsNullOrEmpty(artistFilter))
                SQL += " AND Artist LIKE @Artist";
            SQL += " GROUP BY Upper(Artist)";

            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
            {
                cmd.Parameters.AddWithValue("@Artist", artistFilter.Replace('*', '%'));
                _DBReader = cmd.ExecuteReader();
            }

            _BeginCommand = MediaDataBaseCommands.GetArtist;

            return _DBReader != null;
        }

        public bool beginGetAlbums(string artistFilter = "", string albumFilter = "", string genreFilter = "", bool covers = true)
        {
            if (!DB_ConnectAndOpen())
                return false;

            string SQL = DB_GetMediaInfoSQLBase(DBObjectTypes.IndexedItem);
            if (!String.IsNullOrEmpty(artistFilter))
                SQL += " AND Artist LIKE @Artist";
            if (!String.IsNullOrEmpty(albumFilter))
                SQL += " AND Album LIKE @Album";
            if (!String.IsNullOrEmpty(genreFilter))
                SQL += " AND Genre LIKE @Genre";
            SQL += " GROUP BY Upper(Album), Upper(Artist), Upper(Genre)";

            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
            {
                cmd.Parameters.AddWithValue("@Artist", artistFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Album", albumFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Genre", genreFilter.Replace('*', '%'));
                _DBReader = cmd.ExecuteReader();
            }

            _BeginCommand = MediaDataBaseCommands.GetAlbum;

            return _DBReader != null;
        }

        public IEnumerable<mediaInfo> getSongs(string songFilter = "", string artistFilter = "", string albumFilter = "", string genreFilter = "", string lyricsFilter = "", int minRating = -1, bool covers = true, eMediaField sortBy = eMediaField.Album)
        {
            if (!DB_ConnectAndOpen())
                return new List<mediaInfo>();

            string SQL = String.Format("SELECT Name, Artist, Album, Location, TrackNumber, Genre, Lyrics, Length, Rating, Type, CoverArt AS CoverArtBytes FROM tblMediaInfo JOIN tblMediaInfoCoverArt ON ID=CoverArtID WHERE ObjectType='{0}' ", (int)DBObjectTypes.IndexedItem);

            if (!String.IsNullOrEmpty(songFilter))
                SQL += " AND Name LIKE @Name";
            if (!String.IsNullOrEmpty(artistFilter))
                SQL += " AND Artist LIKE @Artist";
            if (!String.IsNullOrEmpty(albumFilter))
                SQL += " AND Album LIKE @Album";
            if (!String.IsNullOrEmpty(genreFilter))
                SQL += " AND Genre LIKE @Genre";
            if (!String.IsNullOrEmpty(lyricsFilter))
                SQL += " AND Lyrics LIKE @Lyrics";
            if (minRating >= 0)
                SQL += " AND Rating > @Rating";

            #region OrderBy

            switch (sortBy)
            {
                case eMediaField.None:
                    break;
                case eMediaField.Title:
                    SQL += " ORDER BY Name";
                    break;
                case eMediaField.Artist:
                    SQL += " ORDER BY Artist, Album, TrackNumber";
                    break;
                case eMediaField.Album:
                    SQL += " ORDER BY Album, TrackNumber";
                    break;
                case eMediaField.URL:
                    SQL += " ORDER BY Location";
                    break;
                case eMediaField.Rating:
                    SQL += " ORDER BY Rating";
                    break;
                case eMediaField.Lyrics:
                    SQL += " ORDER BY Lyrics";
                    break;
                case eMediaField.Genre:
                    SQL += " ORDER BY Genre";
                    break;
                case eMediaField.Track:
                    SQL += " ORDER BY TrackNumber";
                    break;
                default:
                    break;
            }

            #endregion


            //SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection);
            //cmd.Parameters.AddWithValue("@Name", songFilter.Replace('*', '%'));
            //cmd.Parameters.AddWithValue("@Artist", artistFilter.Replace('*', '%'));
            //cmd.Parameters.AddWithValue("@Album", albumFilter.Replace('*', '%'));
            //cmd.Parameters.AddWithValue("@Genre", genreFilter.Replace('*', '%'));
            //cmd.Parameters.AddWithValue("@Lyrics", lyricsFilter.Replace('*', '%'));
            //cmd.Parameters.AddWithValue("@Rating", minRating);

            var dbData = _DBConnection.Query<mediaInfo>(SQL, 
                new { 
                    Name = songFilter.Replace('*', '%'),
                    Artist = artistFilter.Replace('*', '%'),
                    Album = albumFilter.Replace('*', '%'),
                    Genre = genreFilter.Replace('*', '%'),
                    Lyrics = lyricsFilter.Replace('*', '%'),
                    Rating = minRating
                });

            return dbData;
        }


        public bool beginGetSongs(string songFilter = "", string artistFilter = "", string albumFilter = "", string genreFilter = "", string lyricsFilter = "", int minRating = -1, bool covers = true, eMediaField sortBy = eMediaField.Album)
        {
            if (!DB_ConnectAndOpen())
                return false;

            string SQL = DB_GetMediaInfoSQLBase(DBObjectTypes.IndexedItem);

            if (!String.IsNullOrEmpty(songFilter))
                SQL += " AND Name LIKE @Name";
            if (!String.IsNullOrEmpty(artistFilter))
                SQL += " AND Artist LIKE @Artist";
            if (!String.IsNullOrEmpty(albumFilter))
                SQL += " AND Album LIKE @Album";
            if (!String.IsNullOrEmpty(genreFilter))
                SQL += " AND Genre LIKE @Genre";
            if (!String.IsNullOrEmpty(lyricsFilter))
                SQL += " AND Lyrics LIKE @Lyrics";
            if (minRating >= 0)
                SQL += " AND Rating > @Rating";

            #region OrderBy

            switch (sortBy)
            {
                case eMediaField.None:
                    break;
                case eMediaField.Title:
                    SQL += " ORDER BY Name";
                    break;
                case eMediaField.Artist:
                    SQL += " ORDER BY Artist, Album, TrackNumber";
                    break;
                case eMediaField.Album:
                    SQL += " ORDER BY Album, TrackNumber";
                    break;
                case eMediaField.URL:
                    SQL += " ORDER BY Location";
                    break;
                case eMediaField.Rating:
                    SQL += " ORDER BY Rating";
                    break;
                case eMediaField.Lyrics:
                    SQL += " ORDER BY Lyrics";
                    break;
                case eMediaField.Genre:
                    SQL += " ORDER BY Genre";
                    break;
                case eMediaField.Track:
                    SQL += " ORDER BY TrackNumber";
                    break;
                default:
                    break;
            }

            #endregion

            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
            {
                cmd.Parameters.AddWithValue("@Name", songFilter.Replace('*','%'));
                cmd.Parameters.AddWithValue("@Artist", artistFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Album", albumFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Genre", genreFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Lyrics", lyricsFilter.Replace('*', '%'));
                cmd.Parameters.AddWithValue("@Rating", minRating);
                _DBReader = cmd.ExecuteReader();
            }

            _BeginCommand = MediaDataBaseCommands.GetSongs;

            return _DBReader != null;
        }

        public bool beginGetGenres()
        {
            if (!DB_ConnectAndOpen())
                return false;

            string SQL = DB_GetMediaInfoSQLBase(DBObjectTypes.IndexedItem);
            SQL += " GROUP BY Genre";

            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
            {
                _DBReader = cmd.ExecuteReader();
            }

            _BeginCommand = MediaDataBaseCommands.GetGenres;

            return _DBReader != null;
        }

        public bool setRating(mediaInfo info)
        {
            if (!DB_ConnectAndOpen())
                return false;

            string SQL = "UPDATE tblMediaInfo SET Rating=@Rating WHERE Location=@Location";
            
            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
            {
                cmd.Parameters.AddWithValue("@Rating", info.Rating);
                cmd.Parameters.AddWithValue("@Location", info.Location);
                return (cmd.ExecuteNonQuery() >= 1);
            }
        }

        public mediaInfo getNextMedia()
        {
            if (_DBReader == null)
                return null;

            lock (_DBReader)
            {
                return DB_GetMediaInfoItem(_DBReader, _BeginCommand);
            }
        }

        public bool endSearch()
        {
            if (_DBReader != null)
                _DBReader.Dispose();
            _DBReader = null;
            return true;
        }

        public bool supportsNaturalSearches
        {
            get { return false; }
        }

        public bool beginNaturalSearch(string query)
        {
            return true;
        }

        public IMediaDatabase getNew()
        {
            return new OMMediaDB2(); 
        }

        public bool supportsFileIndexing
        {
            get { return true; }
        }

        public bool indexDirectory(string directory, bool subdirectories)
        {
            return IndexFiles(directory, subdirectories);
        }

        public bool indexFile(string filename)
        {
            return indexFile(filename);
        }

        public bool clearIndex()
        {
            return DB_ClearMediaInfoData(DBObjectTypes.IndexedItem);
        }

        public bool supportsPlaylists
        {
            get { return true; }
        }

        public bool beginGetPlaylist(string name)
        {
            if (!DB_ConnectAndOpen())
                return false;

            // Is playlist present in DB?
            if (!DB_ContainsPlayList(name))
                return false;

            string SQL = DB_GetMediaInfoSQLBase(DBObjectTypes.PlaylistItem);
            SQL += String.Format(" AND ObjectTag='{0}'", name);

            using (SQLiteCommand cmd = new SQLiteCommand(SQL, _DBConnection))
                _DBReader = cmd.ExecuteReader();

            _BeginCommand = MediaDataBaseCommands.GetPlaylists;

            return _DBReader != null;
        }

        public mediaInfo getNextPlaylistItem()
        {
            if (_DBReader == null)
                return null;

            lock (_DBReader)
            {
                return DB_GetMediaInfoItem(_DBReader, _BeginCommand);
            }
        }

        public bool writePlaylist(List<mediaInfo> mediaList, string name, bool append)
        {
            if (!DB_ConnectAndOpen())
                return false;
            
            foreach (mediaInfo media in mediaList)
                DB_InsertMediaInfo(DBObjectTypes.PlaylistItem, media, name);

            return true;
        }

        public bool removePlaylist(string name)
        {
            return DB_DeletePlaylist(name);
        }

        public int getPlayListCount(string playlistName)
        {
            return DB_GetPlayListItemCount(playlistName);
        }

        public List<string> listPlaylists()
        {
            List<string> playlists = new List<string>();

            if (!DB_ConnectAndOpen())
                return playlists;

            try
            {
                using (SQLiteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT ObjectTag FROM tblMediaInfo WHERE ObjectType =@ObjectType AND ObjectTag IS NOT NULL GROUP BY ObjectTag";
                    command.Parameters.AddWithValue("@ObjectType", (int)DBObjectTypes.PlaylistItem);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        playlists.Add(reader.GetString(0));
                }
            }
            catch
            {
            }
            return playlists;
        }

        public string getMediaSetting(string mediaTag, string settingName)
        {
            return DB_GetMediaSetting(mediaTag, settingName);
        }

        public void setMediaSetting(string mediaTag, string settingName, string value)
        {
            DB_SetMediaSetting(mediaTag, settingName, value);
        }

        #endregion
    }
}
