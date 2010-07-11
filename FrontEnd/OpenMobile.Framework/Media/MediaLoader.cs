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

namespace OpenMobile.Media
{
    /// <summary>
    /// Provides easy media loading for skinners
    /// </summary>
    public static class MediaLoader
    {
        /// <summary>
        /// Loads a list of artists
        /// </summary>
        /// <param name="host"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool loadArtists(IPluginHost host, OpenMobile.Controls.IList list)
        {
            PluginSettings ps = new PluginSettings();
            string dbname = ps.getSetting("Default.MusicDatabase");
            ps.Dispose();
            if (dbname == "")
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            list.Clear();
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetArtists(false);
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    list.Add(info.Artist);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return false;
        }
        /// <summary>
        /// Loads all albums from the given artist
        /// </summary>
        /// <param name="host"></param>
        /// <param name="artist"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool loadAlbums(IPluginHost host, string artist, OpenMobile.Controls.IList list)
        {
            PluginSettings ps = new PluginSettings();
            string dbname = ps.getSetting("Default.MusicDatabase");
            ps.Dispose();
            if (dbname == "")
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetAlbums(artist, true);
                list.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    list.AddDistinct(new OMListItem(info.Album, artist, info.coverArt));
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
        /// <returns></returns>
        public static bool loadSongs(IPluginHost host, string artist, OpenMobile.Controls.IList list)
        {
            PluginSettings ps = new PluginSettings();
            string dbname = ps.getSetting("Default.MusicDatabase");
            ps.Dispose();
            if (dbname == "")
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetSongsByArtist(artist, true,eMediaField.Title);
                list.Clear();
                mediaInfo info = db.getNextMedia();
                list.Clear();
                while (info != null)
                {
                    list.AddDistinct(new OMListItem(info.Name,info.Album, info.coverArt));
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
        /// <returns></returns>
        public static bool loadSongs(IPluginHost host, string artist, string album, OpenMobile.Controls.IList list)
        {
            PluginSettings ps = new PluginSettings();
            string dbname = ps.getSetting("Default.MusicDatabase");
            ps.Dispose();
            if (dbname == "")
                return false;
            object o;
            host.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return false;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetSongsByAlbum(artist, album, true,eMediaField.Title);
                list.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    list.AddDistinct(new OMListItem(info.Name,artist, info.coverArt));
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
            return true;
        }
    }
}