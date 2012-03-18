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
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// Retrieves information on available drives
    /// </summary>
    public sealed class DeviceInfo
    {
        private static List<DeviceInfo> AllDevices = new List<DeviceInfo>();
        /// <summary>
        /// List all available devices
        /// </summary>
        /// <param name="theHost"></param>
        /// <returns></returns>
        public static List<DeviceInfo> EnumerateDevices(IPluginHost theHost)
        {
            lock (AllDevices)
            {
                if (AllDevices.Count > 0)
                    return AllDevices;

                theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
                foreach (string drive in Environment.GetLogicalDrives())
                    AllDevices.Add(getDeviceInfo(drive));

                return AllDevices;
            }
        }
        /// <summary>
        /// Get information for the given drive path
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public static DeviceInfo get(string drive)
        {
            DeviceInfo ret = AllDevices.Find(p => p.path == drive);
            if (ret == null)
                return new DeviceInfo(drive, null, null, null, null, eDriveType.Unknown, false, null);
            return ret;
        }
        static void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (type == eMediaType.DeviceRemoved)
                AllDevices.RemoveAll(p => p.path == arg);
            else if (type == eMediaType.NotSet)
            {
                AllDevices.RemoveAll(p => p.path == arg);
                AllDevices.Add(getDeviceInfo(arg));
            }
        }
        private static bool isSystemDrive(string path)
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
        private static DeviceInfo getDeviceInfo(string path)
        {
            List<string> playlists = new List<string>();
            List<string> music = new List<string>();
            List<string> video = new List<string>();
            List<string> pictures = new List<string>();
            bool sys = isSystemDrive(path);
            eDriveType type;
            if (sys)
                type = eDriveType.Fixed;
            else
                type = OSSpecific.getDriveType(path);

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
                // TODO: Add non english support

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
            return new DeviceInfo(path, music.ToArray(), playlists.ToArray(), video.ToArray(), pictures.ToArray(), type, sys, OSSpecific.getVolumeLabel(path));
        }
        internal DeviceInfo(string path, string[] music, string[] playlist, string[] video, string[] picture, eDriveType type, bool sys, string label)
        {
            MusicFolders = music;
            PlaylistFolders = playlist;
            VideoFolders = video;
            PictureFolders = picture;
            DriveType = type;
            systemDrive = sys;
            this.path = path;
            VolumeLabel = label;
        }
        /// <summary>
        /// The device path
        /// </summary>
        public string path;
        /// <summary>
        /// Folders which contain music
        /// </summary>
        public string[] MusicFolders;
        /// <summary>
        /// Folders which contain Playlists
        /// </summary>
        public string[] PlaylistFolders;
        /// <summary>
        /// Folders which contain movies or video
        /// </summary>
        public string[] VideoFolders;
        /// <summary>
        /// Folders which contain Pictures
        /// </summary>
        public string[] PictureFolders;
        /// <summary>
        /// The type of device
        /// </summary>
        public eDriveType DriveType;
        /// <summary>
        /// Is it the system drive
        /// </summary>
        public bool systemDrive;
        /// <summary>
        /// The Volume Label
        /// </summary>
        public string VolumeLabel;
    }
}
