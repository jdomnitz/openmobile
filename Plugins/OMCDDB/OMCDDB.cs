﻿/*********************************************************************************
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
using System.Collections.Generic;
using System.IO;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Threading;
using OpenMobile.Media;

namespace OMCDDB
{
    public class CDDB:IMediaDatabase
    {
        public string databaseType
        {
            get { return "CDDB"; }
        }
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
            foreach (mediaInfo info in media)
                if (info.Genre == genre)
                    ret.Add(info);
            return true;
        }

        public bool beginGetSongsByRating(string genre, bool covers, eMediaField sortBy)
        {
            return false;
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
            foreach (mediaInfo info in media)
                if (info.Lyrics.Contains(phrase))
                    ret.Add(info);
            return true;
        }

        public bool beginGetGenres()
        {
            foreach (mediaInfo info in media)
                if (!ret.Exists(p => p.Genre == info.Genre))
                    ret.Add(info);
            return true;
        }

        public bool setRating(OpenMobile.mediaInfo info)
        {
            return false;
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
            return false;
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
            mediaInfo[] info = CDDBClient.getInfo(directory);
            List<mediaInfo> list=new List<mediaInfo>();
            if (info == null)
            {
                if (!Directory.Exists(directory))
                    return false;
                string name=DeviceInfo.get(directory).VolumeLabel;

                string[] tracks = Directory.GetFiles(directory);
                for (int i = 0; i < tracks.Length; i++)
                {
                    mediaInfo currentTrack = new mediaInfo(tracks[i]);
                    currentTrack.Type = eMediaType.AudioCD;
                    currentTrack.Name = OpenMobile.Path.GetFileNameWithoutExtension(tracks[i]);
                    currentTrack.Album = name;
                    currentTrack.Artist = "Unknown Artist";
                    currentTrack.TrackNumber = i;
                    list.Add(currentTrack);
                }

                //foreach (string track in Directory.GetFiles(directory))
                //{
                //    mediaInfo currentTrack = new mediaInfo(track);
                //    currentTrack.Type = eMediaType.AudioCD;
                //    currentTrack.Name = OpenMobile.Path.GetFileNameWithoutExtension(track);
                //    currentTrack.Album = name;
                //    currentTrack.Artist = "Unknown Artist";
                //    currentTrack.TrackNumber = 
                //    list.Add(currentTrack);
                //}
            }
            if (info == null)
                media= list;
            else
                media = new List<mediaInfo>(info);

            if (media.Count > 0)
            {
                if (media[0].coverArt != null)
                {
                    OpenMobile.Graphics.OImage img = (OpenMobile.Graphics.OImage)media[0].coverArt.Clone();
                    img.AddBorder(10, OpenMobile.Graphics.Color.Transparent);
                    theHost.UIHandler.AddNotification(new Notification(this, null, img, theHost.getSkinImage("Icons|Icon-CD").image, String.Format("Detected CD '{0}' by '{1}'", media[0].Album, media[0].Artist), String.Format("Found {0} tracks at location {1} ", media.Count, directory), Notification_ClickAction, null));
                }
                else
                    theHost.UIHandler.AddNotification(new Notification(this, null, theHost.getSkinImage("Icons|Icon-CD").image, theHost.getSkinImage("Icons|Icon-CD").image, String.Format("Detected CD '{0}' by '{1}'", media[0].Album, media[0].Artist), String.Format("Found {0} tracks at location {1} ", media.Count, directory), Notification_ClickAction, null));
            }
            else
                theHost.UIHandler.AddNotification(new Notification(this, null, theHost.getSkinImage("Icons|Icon-CD").image, theHost.getSkinImage("Icons|Icon-CD").image, "Detected CD", String.Format("Found {0} tracks at location {1} ", media.Count, directory), Notification_ClickAction, null));
            //theHost.SendStatusData(eDataType.Completion, this, "", "Indexing Complete!");
            theHost.raiseMediaEvent(eFunction.MediaIndexingCompleted, null, this.pluginName);
            return true;
        }

        void Notification_ClickAction(Notification notification, int screen, ref bool cancel)
        {
            BuiltInComponents.Host.execute(eFunction.GotoPanel, screen, "NewMedia");
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

        public bool clearIndex(string drive)
        {
            media.RemoveAll(p => p.Location.StartsWith(drive));
            return true;
        }

        public bool supportsPlaylists
        {
            get { return false; }
        }

        public bool beginGetPlaylist(string name)
        {
            return false;
        }

        public mediaInfo getNextPlaylistItem()
        {
            return null;
        }

        public bool writePlaylist(List<string> URLs, string name, bool append)
        {
            return false;
        }
        public mediaInfo GetMediaInfoByUrl(string url)
        {        
            return null;
        }

        public bool removePlaylist(string name)
        {
            return false;
        }

        public List<string> listPlaylists()
        {
            return new List<string>();
        }
        internal static mediaInfo getSongInfo(IPluginHost theHost, string path)
        {
            string dbname;
            object o;
            dbname = OpenMobile.helperFunctions.StoredData.Get(OM.GlobalSetting, "Default.CDDatabase");
            theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
            if (o == null)
                return new mediaInfo(path);
            IMediaDatabase db=(IMediaDatabase)o;
            int trackNum;
            if (int.TryParse(OpenMobile.Path.GetFileNameWithoutExtension(path).ToLower().Replace("track", ""), out trackNum) == false)
                return null;
            db.beginGetSongs(true, eMediaField.Track);
            mediaInfo song=db.getNextMedia();
            int i=1;
            while (song != null)
            {
                if (i == trackNum)
                    return song;
                i++;
                song = db.getNextMedia();
            }
            return new mediaInfo(path);
        }
        #region IBasePlugin Members
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnStorageEvent += new StorageEvent(host_OnStorageEvent);
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);

            OpenMobile.helperFunctions.StoredData.Set(OM.GlobalSetting, "Default.CDDatabase", "OMCDDB");
            return eLoadStatus.LoadSuccessful;
        }

        void host_OnSystemEvent(eFunction function, object[] args)
        {
            // This is not needed storageevents are raised for each drive via HAL 
            //if (function == eFunction.pluginLoadingComplete)
            //{
            //    foreach (DeviceInfo drive in DeviceInfo.EnumerateDevices(theHost))
            //        if (drive.DriveType == eDriveType.CDRom)
            //        {
            //            string path = drive.path;
            //            TaskManager.QueueTask(delegate() { indexDirectory(path, false); }, ePriority.Normal, "Lookup CD Info");
            //        }
            //}
        }

        void host_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            if (type == eMediaType.AudioCD)
                indexDirectory(arg, false);
            else if (type == eMediaType.DeviceRemoved)
                if ((DeviceInfo.get(arg)!=null)&&(DeviceInfo.get(arg).DriveType == eDriveType.CDRom))
                    clearIndex(arg);
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
            get { return "OMCDDB"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "CDDB Database Provider"; }
        }

        public imageItem pluginIcon
        {
            get { return imageItem.NONE; }
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
