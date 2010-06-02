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
using TagLib;
using System.Net;
using System.Xml;
using System.Threading;
using OpenMobile.Net;
using OpenMobile.Plugin;
using OpenMobile.Data;
namespace OpenMobile.Media
{
    /// <summary>
    /// Reads tags from files
    /// </summary>
    public class TagReader
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
                i.Name = Path.GetFileNameWithoutExtension(filename);
                return i;
            }
            catch (Exception) {
                return null;
            }
            Tag t = f.Tag;
            mediaInfo info = new mediaInfo();
            info.Album = t.Album;
            info.Artist = t.JoinedAlbumArtists;
            if (string.IsNullOrEmpty(info.Artist))
                info.Artist = t.JoinedPerformers;
            if (info.Artist!=null)
                info.Artist=info.Artist.Trim().TrimEnd(new char[] { ',' });
            if (t.Pictures.Length > 0)
            {
                MemoryStream m = new MemoryStream(t.Pictures[0].Data.Data);
                try
                {
                    if (m.Length > 4)
                    {
                        info.coverArt = Image.FromStream(m);
                        if ((info.coverArt.Height > 600) || (info.coverArt.Width > 600))
                        {
                            Bitmap newimg = new Bitmap(600, (int)(600 * (info.coverArt.Height / (float)info.coverArt.Width)));
                            Graphics g = Graphics.FromImage(newimg);
                            GraphicsUnit unit = GraphicsUnit.Pixel;
                            g.DrawImage(info.coverArt, newimg.GetBounds(ref unit));
                            g.Dispose();
                            info.coverArt = newimg;
                        }
                    }
                }
                catch (ArgumentException) { }
            }
            info.Genre = t.Genres.ToString();
            info.Location = filename;
            info.Name = t.Title;
            info.Length=(int)f.Properties.Duration.TotalSeconds;
            info.TrackNumber = (int)t.Track;
            return info;
        }
        /// <summary>
        /// Retrieves a folder image located in the same folder as the music specificed by url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image getFolderImage(string url)
        {
            string path = OpenMobile.Path.Combine(System.IO.Path.GetDirectoryName(url), "Folder.jpg");
            if (System.IO.File.Exists(path) == true)
            {
                Image img = Image.FromFile(path);
                if ((img.Height > 600) || (img.Width > 600))
                {
                    Bitmap newimg = new Bitmap(600, (int)(600 * (img.Height / (float)img.Width)));
                    Graphics g = Graphics.FromImage(newimg);
                    GraphicsUnit unit = GraphicsUnit.Pixel;
                    g.DrawImage(img, newimg.GetBounds(ref unit));
                    g.Dispose();
                    return newimg;
                }
                return img;
            }
            return null;
        }
        private static string cacheArtist;
        private static string cacheAlbum;
        private static Image cacheArt;
        /// <summary>
        /// Retrieves metadata from LastFM
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public static Image getLastFMImage(string artist, string album)
        {
            if (Net.Network.IsAvailable==false)
                return null;
            if ((artist==null)||(artist.Length==0))
                return null;
            if ((album==null)||(album.Length==0))
                return null;
            if ((artist == cacheArtist) && (album == cacheAlbum))
                return cacheArt;
            cacheAlbum = album;
            cacheArtist = artist;
            cacheArt = null;
            Thread.Sleep(200); //prevent TOS violation
            XmlDocument reader = new XmlDocument();
            try
            {
                reader.Load("http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=280b54c321127c4647d05ead85cf5fdc&artist=" + Network.urlEncode(artist) + "&album=" + Network.urlEncode(album));
            }
            catch (WebException) { return null; }
            catch (XmlException) { return null; }
            XmlNodeList nodes = reader.DocumentElement.ChildNodes[0].ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "image")
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
            return null;
        }
        public static Image getCoverFromDB(string artist, string album, IPluginHost pluginHost)
        {
            object o = new object();
            using(PluginSettings s=new PluginSettings())
            pluginHost.getData(eGetData.GetMediaDatabase, s.getSetting("Default.MusicDatabase"), out o);
            if (o == null)
                return null;
            using (IMediaDatabase db = (IMediaDatabase)o)
            {
                db.beginGetSongsByAlbum(artist, album, true);
                mediaInfo result = db.getNextMedia();
                if (result == null)
                    return null;
                return result.coverArt;
            }
        }
    }
}
