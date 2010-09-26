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
using System;
using OpenMobile.Framework;

namespace OpenMobile.Media
{
    public sealed class DeviceInfo
    {
        private static bool systemDrive(string path)
        {
            if (Configuration.RunningOnWindows)
            {
                if (System.IO.Path.GetPathRoot(path) == System.IO.Path.GetPathRoot(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System)))
                    return true;
                return false;
            }
            else if (Configuration.RunningOnLinux)
            {
                return (path == "/");
            }
            else
                return false;
        }
        public static DeviceInfo getDeviceInfo(string path)
        {
            List<string> playlists = new List<string>();
            List<string> music = new List<string>();
            List<string> video = new List<string>();
            List<string> pictures = new List<string>();
            eDriveType type = OSSpecific.getDriveType(path);
            bool sys = systemDrive(path);
            if (((type == eDriveType.Fixed) || (type == eDriveType.Unknown)) && sys)
            {
                string tmp = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (Directory.Exists(tmp))
                    pictures.Add(tmp);
                tmp = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).Replace("Music", "Videos");
                if (Directory.Exists(tmp))
                    video.Add(tmp);
                tmp = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (Directory.Exists(tmp))
                    music.Add(tmp);
                string t = Path.Combine(tmp, "Playlists");
                if (Directory.Exists(t))
                    playlists.Add(t);
                t = Path.Combine(tmp, "My Playlists");
                if (Directory.Exists(t))
                    playlists.Add(t);
            }
            else if (type == eDriveType.CDRom)
            {
                if (Directory.Exists(path))
                    music.Add(path);
            }
            else if (type == eDriveType.iPod)
            {
                music.Add(Path.Combine(path, "iPod_Control", "Music"));
                video.Add(Path.Combine(path, "iPod_Control", "Music"));//video files are stored in music with an m4v extension
            }
            else
            {
                bool caseInsensitive = false;
                if (Configuration.RunningOnLinux)
                {
                    DriveInfo info = new DriveInfo(path);
                    switch (info.DriveFormat)
                    {
                        case "ntfs":
                        case "ntfs-3g":
                        case "fat":
                        case "vfat":
                        case "exfat":
                            caseInsensitive = true;
                            break;
                    }
                }
                if (Directory.Exists(Path.Combine(path, "Playlists")))
                    playlists.Add(Path.Combine(path, "Playlists"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "playlists")))
                        playlists.Add(Path.Combine(path, "playlists"));
                }
                if (Directory.Exists(Path.Combine(path, "Music", "Playlists")))
                    playlists.Add(Path.Combine(path, "Music", "Playlists"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "Music", "playlists")))
                        playlists.Add(Path.Combine(path, "Music", "playlists"));
                    if (Directory.Exists(Path.Combine(path, "music", "playlists")))
                        playlists.Add(Path.Combine(path, "music", "playlists"));
                }
                if (Directory.Exists(Path.Combine(path, "Music")))
                    music.Add(Path.Combine(path, "Music"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "music")))
                        music.Add(Path.Combine(path, "music"));
                }
                if (Directory.Exists(Path.Combine(path, "AmazonMP3")))
                    music.Add(Path.Combine(path, "AmazonMP3"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "amazonmp3")))
                        music.Add(Path.Combine(path, "amazonmp3"));
                }
                if (Directory.Exists(Path.Combine(path, "Video")))
                    video.Add(Path.Combine(path, "Video"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "video")))
                        video.Add(Path.Combine(path, "video"));
                }
                if (Directory.Exists(Path.Combine(path, "Movies")))
                    video.Add(Path.Combine(path, "Movies"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "movies")))
                        video.Add(Path.Combine(path, "movies"));
                }
                if (Directory.Exists(Path.Combine(path, "Pictures")))
                    pictures.Add(Path.Combine(path, "Pictures"));
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
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
                if ((Configuration.RunningOnLinux) && !caseInsensitive)
                {
                    if (Directory.Exists(Path.Combine(path, "dcim")))
                    {
                        string[] sub = Directory.GetDirectories(Path.Combine(path, "dcim"));
                        foreach (string dir in sub)
                            if (!dir.Contains(".thumbnails"))
                                pictures.Add(dir);
                    }
                }
            }
            return new DeviceInfo(music.ToArray(), playlists.ToArray(), video.ToArray(), pictures.ToArray(), type);
        }
        public DeviceInfo(string[] music, string[] playlist, string[] video, string[] picture, eDriveType type,bool sys)
        {
            MusicFolders = music;
            PlaylistFolders = playlist;
            VideoFolders = video;
            PictureFolders = picture;
            DriveType = type;
            systemDrive = sys;
        }
        public string[] MusicFolders;
        public string[] PlaylistFolders;
        public string[] VideoFolders;
        public string[] PictureFolders;
        public eDriveType DriveType;
        public bool systemDrive;
    }
}
