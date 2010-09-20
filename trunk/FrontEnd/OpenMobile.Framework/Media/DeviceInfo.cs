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
using System.Collections.Generic;
using System.IO;

namespace OpenMobile.Media
{
    public sealed class DeviceInfo
    {
        public static DeviceInfo getDeviceInfo(string path)
        {
            List<string>playlists=new List<string>();
            List<string>music=new List<string>();
            List<string> video = new List<string>();
            List<string> pictures = new List<string>();
            if (Directory.Exists(Path.Combine(path, "Playlists")))
                playlists.Add(Path.Combine(path, "Playlists"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "playlists")))
                    playlists.Add(Path.Combine(path, "playlists"));
            }
            if (Directory.Exists(Path.Combine(path,"Music", "Playlists")))
                playlists.Add(Path.Combine(path, "Music","Playlists"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path,"Music", "playlists")))
                    playlists.Add(Path.Combine(path, "Music","playlists"));
                if (Directory.Exists(Path.Combine(path, "music", "playlists")))
                    playlists.Add(Path.Combine(path, "music", "playlists"));
            }
            if (Directory.Exists(Path.Combine(path, "Music")))
                music.Add(Path.Combine(path, "Music"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "music")))
                    music.Add(Path.Combine(path, "music"));
            }
            if (Directory.Exists(Path.Combine(path, "AmazonMP3")))
                music.Add(Path.Combine(path, "AmazonMP3"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "amazonmp3")))
                    music.Add(Path.Combine(path, "amazonmp3"));
            }
            if (Directory.Exists(Path.Combine(path, "Video")))
                video.Add(Path.Combine(path, "Video"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "video")))
                    video.Add(Path.Combine(path, "video"));
            }
            if (Directory.Exists(Path.Combine(path, "Movies")))
                video.Add(Path.Combine(path, "Movies"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "movies")))
                    video.Add(Path.Combine(path, "movies"));
            }
            if (Directory.Exists(Path.Combine(path, "Pictures")))
                pictures.Add(Path.Combine(path, "Pictures"));
            if (Configuration.RunningOnLinux)
            {
                if (Directory.Exists(Path.Combine(path, "pictures")))
                    pictures.Add(Path.Combine(path, "pictures"));
            }
            if (Directory.Exists(Path.Combine(path, "DCIM")))
            {
                string[] sub = Directory.GetDirectories(Path.Combine(path, "DCIM"));
                foreach (string dir in sub)
                    if (!dir.Contains(".thumbnails"))
                        pictures.Add(dir);
            }
            if (Configuration.RunningOnLinux)
            {
                string[] sub = Directory.GetDirectories(Path.Combine(path, "dcim"));
                foreach (string dir in sub)
                    if (!dir.Contains(".thumbnails"))
                        pictures.Add(dir);
            }
            return new DeviceInfo(music.ToArray(), playlists.ToArray(), video.ToArray(),pictures.ToArray());
        }
        public DeviceInfo(string[] music, string[] playlist,string[] video,string[] picture)
        {
            MusicFolders = music;
            PlaylistFolders = playlist;
            VideoFolders = video;
            PictureFolders = picture;
        }
        public string[] MusicFolders;
        public string[] PlaylistFolders;
        public string[] VideoFolders;
        public string[] PictureFolders;
    }
}
