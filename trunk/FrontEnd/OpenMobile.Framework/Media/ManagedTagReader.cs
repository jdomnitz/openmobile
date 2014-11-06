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
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Net;
using OpenMobile.Plugin;
using TagLib;

namespace OpenMobile.Media
{
    /// <summary>
    /// Reads tags from files
    /// </summary>
    public static class TagReader
    {
        /// <summary>
        /// Reads tags from the given file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static mediaInfo getInfo(string filename)
        {
            TagLib.File f;
            try
            {
                f = TagLib.File.Create(filename);
            }
            catch (UnsupportedFormatException)
            {
                mediaInfo i = new mediaInfo(filename);
                i.Name = Path.GetFileNameWithoutExtension(filename).Replace('_', ' ');
                return i;
            }
            catch (Exception)
            {
                return null;
            }

            // Do we have an empty tag? 
            if (f.Tag.IsEmpty)
            {   // Yes, use filename instead
                mediaInfo i = new mediaInfo(filename);
                i.Name = Path.GetFileNameWithoutExtension(filename).Replace('_', ' ');
                return i;
            }

            Tag t = f.Tag;
            mediaInfo info = new mediaInfo();
            info.Album = t.Album;
            info.Artist = t.JoinedAlbumArtists;
            if (string.IsNullOrEmpty(info.Artist))
                info.Artist = t.JoinedPerformers;
            if (info.Artist != null)
                info.Artist = info.Artist.Trim().TrimEnd(new char[] { ',' });
            if (t.Pictures.Length > 0)
            {
                MemoryStream m = new MemoryStream(t.Pictures[0].Data.Data);
                try
                {
                    if (m.Length > 4)
                    {
                        info.coverArt = OImage.FromStream(m);
                        if ((info.coverArt.Height > 600) || (info.coverArt.Width > 600))
                        {
                            OImage newimg = new OImage(new Bitmap(600, (int)(600 * (info.coverArt.Height / (float)info.coverArt.Width))));
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newimg.image);
                            GraphicsUnit unit = GraphicsUnit.Pixel;
                            g.DrawImage(info.coverArt.image, newimg.image.GetBounds(ref unit));
                            g.Dispose();
                            info.coverArt = newimg;
                        }
                    }
                }
                catch (ArgumentException) { }
            }
            info.Genre = t.FirstGenre;
            info.Location = filename;
            info.Name = t.Title;
            info.Lyrics = t.Lyrics;
            info.Length = (int)f.Properties.Duration.TotalSeconds;
            info.TrackNumber = (int)t.Track;
            return info;
        }
        /// <summary>
        /// Retrieves a folder image located in the same folder as the music specificed by url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static OImage getFolderImage(string url)
        {
            string path = OpenMobile.Path.Combine(System.IO.Path.GetDirectoryName(url), "Folder.jpg");
            if (System.IO.File.Exists(path) == true)
            {
                OImage img = OImage.FromFile(path);
                //if ((img.Height > 600) || (img.Width > 600))
                //{
                //    OImage newimg = new OImage(new Bitmap(600, (int)(600 * (img.Height / (float)img.Width))));
                //    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newimg.image);
                //    GraphicsUnit unit = GraphicsUnit.Pixel;
                //    g.DrawImage(img.image, newimg.image.GetBounds(ref unit));
                //    g.Dispose();
                //    return newimg;
                //}
                return img;
            }
            return null;
        }
        /// <summary>
        /// Extracts a file icon or preview image from the requested file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static OImage getFileThumbnail(string path, int size)
        {
#if WINDOWS
            if (OpenTK.Configuration.RunningOnWindows)
            {
                return IconExtractor.GetFileIcon(path, size);
            }
#endif
#if LINUX
#if WINDOWS
            else
#endif
                if (OpenTK.Configuration.RunningOnLinux)
                {
                    return GnomeIcon.GetFileIcon(path);
                }
#endif
            return null;
        }
        private static string cacheArtist;
        private static string cacheAlbum;
        private static OImage cacheArt;
        private static DateTime lastCheck;

        /// <summary>
        /// Coversizes to proritize when using LastFM
        /// </summary>
        public enum LastFMCoverSize
        {
            /// <summary>
            /// Extra large covers
            /// </summary>
            XLarge,

            /// <summary>
            /// Mega size covers
            /// </summary>
            Mega
        }

        /// <summary>
        /// Retrieves metadata from LastFM
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public static OImage getLastFMImage(string artist, string album)
        {
            return getLastFMImage(artist, album, LastFMCoverSize.XLarge);
        }

        /// <summary>
        /// Retrieves metadata from LastFM
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="coverSize"></param>
        /// <returns></returns>
        public static OImage getLastFMImage(string artist, string album, LastFMCoverSize coverSize)
        {
            if (Net.Network.IsAvailable == false)
                return null;
            if (string.IsNullOrEmpty(artist))
                return null;
            if (string.IsNullOrEmpty(album))
                return null;
            if ((artist == cacheArtist) && (album == cacheAlbum))
                return cacheArt;
            cacheAlbum = album;
            cacheArtist = artist;
            cacheArt = null;
            TimeSpan ts = DateTime.Now - lastCheck;
            if (ts.TotalSeconds < 205)
                Thread.Sleep(205 - unchecked((int)ts.TotalSeconds)); //prevent TOS violation
            XmlDocument reader = new XmlDocument();
            if (album == "Unknown Album")
                return null;
            if (artist == "Unknown Artist")
                return null;
            try
            {
                reader.Load("http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=280b54c321127c4647d05ead85cf5fdc&artist=" + Network.urlEncode(artist) + "&album=" + Network.urlEncode(album));
                lastCheck = DateTime.Now;
            }
            catch (WebException) { return null; }
            catch (XmlException) { return null; }
            XmlNodeList nodes = reader.DocumentElement.ChildNodes[0].ChildNodes;

            switch (coverSize)
            {
                case LastFMCoverSize.XLarge:
                    {
                        // try the second largest image
                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name == "image")
                            {
                                if (node.Attributes[0].Value == "extralarge")
                                {
                                    try
                                    {
                                        cacheArt = Net.Network.imageFromURL(node.InnerText);
                                    }
                                    catch (WebException) { }
                                    return cacheArt;
                                }
                            }
                        }
                    }
                    break;
                case LastFMCoverSize.Mega:
                    {
                        // Try to get the biggest image
                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name == "image")
                            {
                                if (node.Attributes[0].Value == "mega")
                                {
                                    try
                                    {
                                        cacheArt = Net.Network.imageFromURL(node.InnerText);
                                    }
                                    catch (WebException) { }
                                    return cacheArt;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }



            return null;
        }
        /// <summary>
        /// Retrieves a cover from the database
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="pluginHost"></param>
        /// <returns></returns>
        public static OImage getCoverFromDB(string artist, string album)
        {
            using (PluginSettings s = new PluginSettings())
                return getCoverFromDB(artist, album, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        }
        /// <summary>
        /// Retrieves the cover art from the given database
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="pluginHost"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static OImage getCoverFromDB(string artist, string album, string dbName)
        {
            object o = new object();
            BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName, out o);
            if (o == null)
                return null;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetSongs("", artist, album, "", "", -1, true, eMediaField.Title);
                mediaInfo result = db.getNextMedia();
                if (result == null)
                    return null;
                return result.coverArt;
            }
        }
    }
}
