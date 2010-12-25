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
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;

namespace OpenMobile.Media
{
    /// <summary>
    /// Provides easy media loading for skinners
    /// </summary>
    public static class MediaLoader
    {
        /// <summary>
        /// Loads a list of genres
        /// </summary>
        /// <param name="host"></param>
        /// <param name="list"></param>
        /// <param name="dbname"></param>
        /// <returns></returns>
        public static bool loadGenres(IPluginHost host, OpenMobile.Controls.IList list, string dbname)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            list.Clear();
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                if (!db.beginGetGenres())
                    return false;
                mediaInfo info = db.getNextMedia();
                if ((info!=null)&&(info.Genre.Length==0))
                    info = db.getNextMedia();
                while (info != null)
                {
                    list.Add(info.Genre);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        
        /// <summary>
        /// Loads a list of artists
        /// </summary>
        /// <param name="host"></param>
        /// <param name="list"></param>
        /// <param name="dbname"></param>
        /// <returns></returns>
        public static bool loadArtists(IPluginHost host, OpenMobile.Controls.IList list,string dbname)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            list.Clear();
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                if (!db.beginGetArtists(false))
                    return false;
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    list.Add(info.Artist);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        /// <summary>
        /// Loads all albums from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="artist"></param>
        /// <param name="list"></param>
        /// <param name="dbname"></param>
        /// <param name="format"></param>
        /// <param name="clear"></param>
        /// <param name="noCover"></param>
        /// <returns></returns>
        public static bool loadAlbums(IPluginHost host, string artist, OpenMobile.Controls.IList list,OMListItem.subItemFormat format,bool clear,string dbname,OImage noCover)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                try
                {
                    if (!db.beginGetAlbums(artist, true))
                        return false;
                }
                catch (Mono.Data.Sqlite.SqliteException)
                {
                    return false;
                }
                if (clear)
                    list.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (info.coverArt == null)
                        info.coverArt = noCover;
                    list.AddDistinct(new OMListItem(info.Album, artist, info.coverArt,format));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        /// <summary>
        /// Loads all songs from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <param name="dbname"></param>
        /// <param name="format"></param>
        /// <param name="noCover"></param>
        public static bool loadSongs(IPluginHost host, IList list,OMListItem.subItemFormat format,string dbname,OImage noCover)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                try
                {
                    if (!db.beginGetSongs(true, eMediaField.Title))
                        return false;
                }
                catch (Mono.Data.Sqlite.SqliteException)
                {
                    return false;
                }
                list.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (info.coverArt == null)
                        info.coverArt = noCover;
                    list.AddDistinct(new OMListItem(info.Name, info.Album, info.coverArt,format,info.Location));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        /// <summary>
        /// Loads all songs from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="artist"></param>
        /// <param name="list"></param>
        /// <param name="format"></param>
        /// <param name="dbname"></param>
        /// <param name="noCover"></param>
        /// <returns></returns>
        public static bool loadSongs(IPluginHost host, string artist, OpenMobile.Controls.IList list,OMListItem.subItemFormat format,string dbname,OImage noCover)
        {
            return loadSongs(host, artist, list, format, true,dbname,noCover);
        }

        /// <summary>
        /// Loads all songs from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="list"></param>
        /// <param name="clear"></param>
        /// <param name="format"></param>
        /// <param name="dbname"></param>
        /// <param name="noCover"></param>
        /// <param name="genre"></param>
        /// <returns></returns>
        public static bool loadSongsByGenre(IPluginHost host, string genre, OpenMobile.Controls.IList list, OMListItem.subItemFormat format, bool clear, string dbname, OImage noCover)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                try
                {
                    if (!db.beginGetSongsByGenre(genre, true, eMediaField.Title))
                        return false;
                }
                catch (Mono.Data.Sqlite.SqliteException)
                {
                    return false;
                }
                mediaInfo info = db.getNextMedia();
                if (clear)
                    list.Clear();
                while (info != null)
                {
                    if (info.coverArt == null)
                        info.coverArt = noCover;
                    if (info.Type == eMediaType.AudioCD)
                        list.Add(new OMListItem(info.Name, info.Artist, info.coverArt, format, info.Location, info.TrackNumber.ToString()));
                    else
                        list.Add(new OMListItem(info.Name, info.Artist, info.coverArt, format, info.Location));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        /// <summary>
        /// Loads all songs from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="artist"></param>
        /// <param name="list"></param>
        /// <param name="clear"></param>
        /// <param name="format"></param>
        /// <param name="dbname"></param>
        /// <param name="noCover"></param>
        /// <returns></returns>
        public static bool loadSongs(IPluginHost host, string artist, OpenMobile.Controls.IList list,OMListItem.subItemFormat format,bool clear,string dbname,OImage noCover)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                try
                {
                    if (!db.beginGetSongsByArtist(artist, true, eMediaField.Title))
                        return false;
                }
                catch (Mono.Data.Sqlite.SqliteException)
                {
                    return false;
                }
                mediaInfo info = db.getNextMedia();
                if (clear)
                    list.Clear();
                while (info != null)
                {
                    if (info.coverArt == null)
                        info.coverArt = noCover;
                    if (info.Type==eMediaType.AudioCD)
                        list.Add(new OMListItem(info.Name, info.Album, info.coverArt, format, info.Location,info.TrackNumber.ToString()));
                    else
                        list.Add(new OMListItem(info.Name,info.Album, info.coverArt,format,info.Location));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
        /// <summary>
        /// Loads all songs from the given artist and album
        /// </summary>
        /// <param name="host"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="list"></param>
        /// <param name="dbname"></param>
        /// <param name="format"></param>
        /// <param name="noCover"></param>
        /// <returns></returns>
        public static bool loadSongs(IPluginHost host, string artist, string album, OpenMobile.Controls.IList list,OMListItem.subItemFormat format,string dbname,OImage noCover)
        {
            if (string.IsNullOrEmpty(dbname))
                return false;
            if ((host == null) || (list == null))
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                if (!db.beginGetSongsByAlbum(artist, album, true, eMediaField.Title))
                    return false;
                list.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (info.coverArt == null)
                        info.coverArt = noCover;
                    list.AddDistinct(new OMListItem(info.Name,artist, info.coverArt,format,info.Location));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
    }
}