using System.Collections.Generic;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// Fields in Media Info
    /// </summary>
    public enum eMediaField
    {
        /// <summary>
        /// Used as a parameter
        /// </summary>
        None,
        /// <summary>
        /// Title
        /// </summary>
        Title,
        /// <summary>
        /// Artist
        /// </summary>
        Artist,
        /// <summary>
        /// Album
        /// </summary>
        Album,
        /// <summary>
        /// File location
        /// </summary>
        URL,
        /// <summary>
        /// Rating
        /// </summary>
        Rating,
        /// <summary>
        /// Lyrics
        /// </summary>
        Lyrics,
        /// <summary>
        /// Genre
        /// </summary>
        Genre,
        /// <summary>
        /// Track
        /// </summary>
        Track
    }
    /// <summary>
    /// Provides a media information (tag) database
    /// </summary>
    public interface IMediaDatabase:IBasePlugin
    {
        /// <summary>
        /// A unique name for this data type. (Ex: Winamp or iTunes)
        /// </summary>
        string databaseType { get; }
        #region MediaSearch
        /// <summary>
        /// Begin listing artists
        /// </summary>
        /// <param name="artistFilter"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetArtists(string artistFilter = "", bool covers = true);
        /// <summary>
        /// List albums from given artist
        /// </summary>
        /// <param name="artistFilter"></param>
        /// <param name="albumFilter"></param>
        /// <param name="genreFilter"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetAlbums(string artistFilter = "", string albumFilter = "", string genreFilter = "", bool covers = true);
        /// <summary>
        /// List all songs
        /// </summary>
        /// <param name="covers"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        bool beginGetSongs(string songFilter = "", string artistFilter = "", string albumFilter = "", string genreFilter = "", string lyricsFilter = "", int minRating = -1, bool covers = true, eMediaField sortBy = eMediaField.Artist);
        /// <summary>
        /// List available Genres
        /// </summary>
        /// <returns></returns>
        bool beginGetGenres();
        /// <summary>
        /// Sets the rating for the given media
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool setRating(mediaInfo info);
        /// <summary>
        /// Returns the next record in an asynchronous query
        /// </summary>
        /// <returns>The next record in an asynchronous query</returns>
        mediaInfo getNextMedia();

        /// <summary>
        /// Ends an asynchronous search
        /// </summary>
        /// <returns>True if successful</returns>
        bool endSearch();

        /// <summary>
        /// Whether or not the plugin can search multiple fields simultaneously
        /// </summary>
        bool supportsNaturalSearches { get; }

        /// <summary>
        /// Asynchronously searchs multiple fields to find matching records
        /// </summary>
        /// <param name="query">the natural language query using field names from eMediaField</param>
        /// <returns>True if successful</returns>
        bool beginNaturalSearch(string query);
        #endregion
        /// <summary>
        /// Returns a new instance of the plugin
        /// </summary>
        /// <returns></returns>
        IMediaDatabase getNew();

        #region Indexing
        /// <summary>
        /// Whether or not the plugin supports index commands
        /// </summary>
        bool supportsFileIndexing { get; }

        /// <summary>
        /// Indexes the given the directory
        /// </summary>
        /// <param name="directory">The directory to index</param>
        /// <param name="subdirectories">Recurse subdirectories</param>
        /// <returns></returns>
        bool indexDirectory(string directory, bool subdirectories);

        /// <summary>
        /// Adds media information from the given file to the database
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        bool indexFile(string filename);
        /// <summary>
        /// Delete all records from the index/database
        /// </summary>
        /// <returns></returns>
        bool clearIndex();
        #endregion

        #region Playlist

        /// <summary>
        /// If the plugin can read/write playlists
        /// </summary>
        bool supportsPlaylists { get; }
        /// <summary>
        /// Opens the database and begins items from the given playlist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool beginGetPlaylist(string name);

        /// <summary>
        /// Returns the URL of the next file in the playlist
        /// </summary>
        /// <returns></returns>
        mediaInfo getNextPlaylistItem();

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="mediaList"></param>
        /// <param name="name"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        bool writePlaylist(List<mediaInfo> mediaList, string name, bool append);

        /// <summary>
        /// Deletes a playlist from the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool removePlaylist(string name);
        /// <summary>
        /// Returns a list of playlists available in the database
        /// </summary>
        /// <returns></returns>
        List<string> listPlaylists();

        /// <summary>
        /// Sets a media setting
        /// </summary>
        /// <param name="mediaTag"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        string getMediaSetting(string mediaTag, string settingName);

        /// <summary>
        /// Gets a media setting
        /// </summary>
        /// <param name="mediaTag"></param>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void setMediaSetting(string mediaTag, string settingName, string value);



        #endregion

    }
}
