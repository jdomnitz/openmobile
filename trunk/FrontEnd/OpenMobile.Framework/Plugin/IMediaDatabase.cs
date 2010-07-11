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
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetArtists(bool covers);
        /// <summary>
        /// Begin listing albums
        /// </summary>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetAlbums(bool covers);
        /// <summary>
        /// List albums from given artist
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetAlbums(string artist, bool covers);
        /// <summary>
        /// List all songs (Added by Borte)
        /// </summary>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongs(bool covers,eMediaField sortBy);
        /// <summary>
        /// List songs by artist
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongsByArtist(string artist, bool covers, eMediaField sortBy);
        /// <summary>
        /// List songs that are in the given Genre
        /// </summary>
        /// <param name="genre"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongsByGenre(string genre, bool covers, eMediaField sortBy);
        /// <summary>
        /// List songs that have the given rating
        /// </summary>
        /// <param name="genre"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongsByRating(string genre, bool covers, eMediaField sortBy);
        /// <summary>
        /// Get songs that match the given artist and album
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongsByAlbum(string artist, string album, bool covers, eMediaField sortBy);
        /// <summary>
        /// List songs that contain the given lyrics
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="covers"></param>
        /// <returns></returns>
        bool beginGetSongsByLyrics(string phrase, bool covers, eMediaField sortBy);
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

        #region PlaylistSearch
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
        string getNextPlaylistItem();
        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="URLs"></param>
        /// <param name="name"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        bool writePlaylist(List<string> URLs,string name,bool append);
        /// <summary>
        /// Returns a list of playlists available in the database
        /// </summary>
        /// <returns></returns>
        List<string> listPlaylists();
        #endregion

    }
}
