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
            catch (Exception) { return null; }
            Tag t = f.Tag;
            mediaInfo info = new mediaInfo();
            info.Album = t.Album;
            info.Artist = t.JoinedAlbumArtists;
            if (info.Artist == "")
                info.Artist = t.JoinedPerformers;
            info.Artist.TrimEnd(new char[] { ',' });
            if (t.Pictures.Length > 0)
            {
                MemoryStream m = new MemoryStream(t.Pictures[0].Data.Data);
                try
                {
                    if (m.Length > 0)
                        info.coverArt = Image.FromStream(m);
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
                return Image.FromFile(path);
            return null;
        }
    }
}
