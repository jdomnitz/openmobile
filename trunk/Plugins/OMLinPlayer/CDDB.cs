using System;
using System.Collections.Generic;
using System.Text;
using OpenMobile.Plugin;
using OpenMobile;
using OpenMobile.Framework;
using System.IO;
using OpenMobile.Threading;
using OpenMobile.Data;
using OpenMobile.Media;

namespace OMLinPlayer
{
    public class CDDB:IMediaDatabase
    {
        public string databaseType
        {
            get { return "CDDB"; }
        }
        string type;
        public bool beginGetArtists(bool covers)
        {
            foreach (mediaInfo info in media)
                if (!ret.Exists(p => p.Artist == info.Artist))
                    ret.Add(info);
            return true;
        }

        public bool beginGetAlbums(bool covers)
        {
            foreach (mediaInfo info in media)
                if (!ret.Exists(p => p.Album == info.Album))
                    ret.Add(info);
            return true;
        }
        public bool beginGetAlbums(string artist, bool covers)
        {
            foreach (mediaInfo info in media.FindAll(p=>p.Artist==artist))
                if (!ret.Exists(p => p.Album == info.Album))
                    ret.Add(info);
            return true;
        }

        public bool beginGetSongs(bool covers, eMediaField sortBy)
        {
            foreach (mediaInfo info in media)
                            if (!ret.Exists(p => p.Name== info.Name))
                                ret.Add(info);
            return true;
        }

        public bool beginGetSongsByArtist(string artist, bool covers, eMediaField sortBy)
        {
            foreach (mediaInfo info in media.FindAll(p => p.Artist == artist))
                            if (!ret.Exists(p => p.Name == info.Name))
                                ret.Add(info);
            return true;
        }

        public bool beginGetSongsByGenre(string genre, bool covers, eMediaField sortBy)
        {
            throw new NotImplementedException();
        }

        public bool beginGetSongsByRating(string genre, bool covers, eMediaField sortBy)
        {
            throw new NotImplementedException();
        }

        public bool beginGetSongsByAlbum(string artist, string album, bool covers, eMediaField sortBy)
        {
            foreach (mediaInfo info in media.FindAll(p => (p.Artist == artist) && (p.Album == album)))
                if (!ret.Exists(p => p.Name == info.Name))
                    ret.Add(info);
            return true;
        }

        public bool beginGetSongsByLyrics(string phrase, bool covers, eMediaField sortBy)
        {
            throw new NotImplementedException();
        }

        public bool beginGetGenres()
        {
            throw new NotImplementedException();
        }

        public bool setRating(OpenMobile.mediaInfo info)
        {
            throw new NotImplementedException();
        }
        List<mediaInfo> media=new List<mediaInfo>();
        List<mediaInfo> ret = new List<mediaInfo>();
        int currentItem = -1;
        public OpenMobile.mediaInfo getNextMedia()
        {
            currentItem++;
            if (currentItem >= ret.Count)
            {
                currentItem = -1;
                return null;
            }
            return ret[currentItem];
        }

        public bool endSearch()
        {
            return true;
        }

        public bool supportsNaturalSearches
        {
            get { return false; }
        }

        public bool beginNaturalSearch(string query)
        {
            throw new NotImplementedException();
        }

        public IMediaDatabase getNew()
        {
            return this;
        }

        public bool supportsFileIndexing
        {
            get { return true; }
        }

        public bool indexDirectory(string directory, bool subdirectories)
        {
            mediaInfo[] info = CDDBClient.getTracks("/dev/sr0");
            List<mediaInfo> list=new List<mediaInfo>();
            if (info == null)
            {
                if (!Directory.Exists(directory))
                    return false;
                string name=DeviceInfo.get(directory).VolumeLabel;
                foreach (string track in Directory.GetFiles(directory))
                {
                    mediaInfo currentTrack = new mediaInfo(track);
                    currentTrack.Name = OpenMobile.Path.GetFileNameWithoutExtension(track);
                    currentTrack.Album = name;
                    currentTrack.Artist = "Unknown Artist";
                    list.Add(currentTrack);
                }
            }
            media= new List<mediaInfo>(info);
            return true;
        }

        public bool indexFile(string filename)
        {
            return false;
        }

        public bool clearIndex()
        {
            media.Clear();
            return true;
        }

        public bool supportsPlaylists
        {
            get { return false; }
        }

        public bool beginGetPlaylist(string name)
        {
            throw new NotImplementedException();
        }

        public mediaInfo getNextPlaylistItem()
        {
            throw new NotImplementedException();
        }

        public bool writePlaylist(List<string> URLs, string name, bool append)
        {
            throw new NotImplementedException();
        }

        public bool removePlaylist(string name)
        {
            return false;
        }

        public List<string> listPlaylists()
        {
            return new List<string>();
        }

        #region IBasePlugin Members

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            host.OnStorageEvent += new StorageEvent(host_OnStorageEvent);
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Default.CDDatabase", "CDDB");
            foreach (DeviceInfo drive in DeviceInfo.EnumerateDevices(host))
                if (drive.DriveType == eDriveType.CDRom)
                    TaskManager.QueueTask(delegate() { indexDirectory(drive.path, false); }, ePriority.Normal, "Lookup CD Info");
            return eLoadStatus.LoadSuccessful;
        }

        void host_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (type == eMediaType.AudioCD)
                indexDirectory(arg, false);
            else if (type == eMediaType.DeviceRemoved)
                if (DeviceInfo.get(arg).DriveType == eDriveType.CDRom)
                    clearIndex();
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "CDDB"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "CDDB Database Provider"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
