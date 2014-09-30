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
using System.Threading;
using System.Xml;
using OpenMobile.Data;
using OpenMobile.Net;
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// Playlist types
    /// </summary>
    public enum ePlaylistType_old
    {
        /// <summary>
        /// M3U Playlist (aka MP3 URL)
        /// </summary>
        M3U,
        /// <summary>
        ///  Windows Media Player Playlist
        /// </summary>
        WPL,
        /// <summary>
        /// PLS Playlist
        /// </summary>
        PLS,
        /// <summary>
        /// Advanced Stream Redirector
        /// </summary>
        ASX,
        /// <summary>
        /// XML Shareable Playlist Format
        /// </summary>
        XSPF,
        /// <summary>
        /// A PodCast
        /// </summary>
        PCAST
    }
    /// <summary>
    /// Provides functions for reading and writing playlists
    /// </summary>
    public static class Playlist_old
    {
        /// <summary>
        /// Writes a playlist to a file
        /// </summary>
        /// <param name="location">The location to write to</param>
        /// <param name="type">The type of playlist to write</param>
        /// <param name="playlist">The playlist</param>
        /// <returns></returns>
        public static bool writePlaylist(string location, ePlaylistType type, List<string> playlist)
        {
            return writePlaylist(location, type, Convert(playlist));
        }
        /// <summary>
        /// Writes a playlist to a file
        /// </summary>
        /// <param name="location">The location to write to</param>
        /// <param name="type">The type of playlist to write</param>
        /// <param name="playlist">The playlist</param>
        /// <returns></returns>
        public static bool writePlaylist(string location, ePlaylistType type, List<mediaInfo> playlist)
        {
            switch (type)
            {

                case ePlaylistType.PLS:
                    return writePLS(location, playlist);
                case ePlaylistType.M3U:
                    return writeM3U(location, playlist);
                case ePlaylistType.XSPF:
                    return writeXSPF(location, playlist);
                case ePlaylistType.WPL:
                    return writeWPL(location, playlist);
            }
            return false;
        }
        private static bool writeWPL(string location, List<mediaInfo> playlist)
        {

            if (!location.ToLower().EndsWith(".wpl"))
                location += ".wpl";
            string folder = Directory.GetParent(location).FullName;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                XmlWriter writer = XmlWriter.Create(location, settings);
                writer.WriteRaw("<?wpl version=\"1.0\"?>");
                writer.WriteStartElement("smil");
                writer.WriteStartElement("head");
                {
                    writer.WriteStartElement("meta");
                    writer.WriteAttributeString("name", "Generator");
                    writer.WriteAttributeString("content", "OpenMobile v0.8");
                    writer.WriteEndElement();
                    writer.WriteStartElement("meta");
                    writer.WriteAttributeString("name", "ItemCount");
                    writer.WriteAttributeString("content", playlist.Count.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("body");
                writer.WriteStartElement("seq");
                for (int i = 0; i < playlist.Count; i++)
                {
                    writer.WriteStartElement("media");
                    writer.WriteAttributeString("src", Path.getRelativePath(folder, playlist[i].Location));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Close();
                return true;
            }
            catch (Exception) { return false; }
        }
        private static bool writeXSPF(string location, List<mediaInfo> playlist)
        {
            if (!location.ToLower().EndsWith(".xspf"))
                location += ".xspf";
            string folder = Directory.GetParent(location).FullName;
            try
            {
                XmlWriter writer = XmlWriter.Create(location);
                writer.WriteStartDocument();
                writer.WriteStartElement("playlist");
                writer.WriteAttributeString("Version", "1");
                writer.WriteStartElement("trackList");
                for (int i = 0; i < playlist.Count; i++)
                {
                    writer.WriteStartElement("track");
                    if (playlist[i].Name != null)
                        writer.WriteElementString("title", playlist[i].Name);
                    if (playlist[i].Artist != null)
                        writer.WriteElementString("creator", playlist[i].Artist);
                    if (playlist[i].Album != null)
                        writer.WriteElementString("album", playlist[i].Album);
                    writer.WriteElementString("location", "file://" + Path.getRelativePath(folder, playlist[i].Location));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
                return true;
            }
            catch (Exception) { return false; }
        }
        private static bool writeM3U(string location, List<mediaInfo> playlist)
        {
            if (!location.ToLower().EndsWith(".m3u"))
                location += ".m3u";
            string folder = Directory.GetParent(location).FullName;
            try
            {
                FileStream f = File.Create(location);
                StreamWriter writer = new StreamWriter(f);
                writer.WriteLine("#EXTM3U\r\n");
                for (int i = 0; i < playlist.Count; i++)
                {
                    writer.Write("#EXTINF:");
                    writer.Write(((playlist[i].Length == 0) ? -1 : playlist[i].Length).ToString());
                    if (playlist[i].Name != null)
                        writer.WriteLine("," + playlist[i].Artist + " - " + playlist[i].Name);
                    writer.WriteLine(Path.getRelativePath(folder, playlist[i].Location));
                    writer.WriteLine(string.Empty);
                }
                f.Close();
                return true;
            }
            catch (Exception) { return false; }
        }
        private static bool writePLS(string location, List<mediaInfo> p)
        {
            if (!location.ToLower().EndsWith(".pls"))
                location += ".pls";
            string folder = Directory.GetParent(location).FullName;
            try
            {
                FileStream f = File.Create(location);
                StreamWriter writer = new StreamWriter(f);
                writer.WriteLine("[playlist]\r\n");
                for (int i = 0; i < p.Count; i++)
                {
                    writer.WriteLine("File" + (i + 1).ToString() + "=" + Path.getRelativePath(folder, p[i].Location));
                    if (p[i].Name != null)
                        writer.WriteLine("Title" + (i + 1).ToString() + "=" + p[i].Name);
                    if (p[i].Length > 0)
                        writer.WriteLine("Length" + (i + 1).ToString() + "=" + p[i].Length.ToString());
                    writer.WriteLine(string.Empty);
                }
                writer.WriteLine("NumberOfEntries=" + p.Count.ToString());
                writer.WriteLine("Version=2");
                f.Close();
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="name"></param>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static bool writePlaylistToDB(IPluginHost theHost, string name, List<mediaInfo> playlist)
        {
            if (theHost == null)
                return false;
            if (playlist.Count == 0)
                return false;
            object o;
            using (PluginSettings s = new PluginSettings())
                theHost.getData(eGetData.GetMediaDatabase, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"), out o);
            if (o == null)
                return false;
            IMediaDatabase db = (IMediaDatabase)o;
            return db.writePlaylist(playlist, name, false);
        }

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool deletePlaylistFromDB(IPluginHost theHost, string name)
        {
            if (theHost == null)
                return false;
            object o;
            using (PluginSettings s = new PluginSettings())
                theHost.getData(eGetData.GetMediaDatabase, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"), out o);
            if (o == null)
                return false;
            IMediaDatabase db = (IMediaDatabase)o;
            return db.removePlaylist(name);
        }

        private static string convertMediaInfo(mediaInfo info)
        {
            return info.Location;
        }

        /// <summary>
        /// Returns a list of all available playlists in a directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<string> listPlaylists(string directory)
        {
            List<string> ret = new List<string>();
            string[] filter = new string[] { "*.m3u", "*.wpl", "*.pls", "*.asx", "*.wax", "*.wvx", "*.xspf", "*.pcast" };
            for (int i = 0; i < filter.Length; i++)
                ret.AddRange(Directory.GetFiles(directory, filter[i]));
            ret.Sort();
            ret.TrimExcess();
            return ret;
        }
        /// <summary>
        /// Lists playlists available in the media database
        /// </summary>
        /// <param name="theHost"></param>
        /// <returns></returns>
        public static List<string> listPlaylistsFromDB(IPluginHost theHost)
        {
            string dbName = String.Empty;
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
            return listPlaylistsFromDB(theHost, dbName);
        }
        /// <summary>
        /// Lists playlists available in the given media database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<string> listPlaylistsFromDB(IPluginHost theHost, string dbName)
        {
            if (theHost == null)
                return new List<string>();
            object o = null;
            for (int i = 0; ((i < 35) && (o == null)); i++)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbName, out o);
                if (i > 0)
                    Thread.Sleep(200);
            }
            if (o == null)
                return new List<string>();
            IMediaDatabase db = (IMediaDatabase)o;
            if (db.supportsPlaylists)
                return db.listPlaylists();
            return new List<string>();
        }
        /// <summary>
        /// Reads a playlist from the database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(IPluginHost theHost, string name)
        {
            string dbName = String.Empty;
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
            return readPlaylistFromDB(theHost, name, dbName);
        }
        /// <summary>
        /// Reads the given playlist from the database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="name"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(IPluginHost theHost, string name, string dbName)
        {
            object o = null;
            List<mediaInfo> playlist = new List<mediaInfo>();
            for (int i = 0; ((i < 35) && (o == null)); i++)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbName, out o);
                if (i > 0)
                    Thread.Sleep(200);
            }
            if (o == null)
                return playlist;
            IMediaDatabase db = (IMediaDatabase)o;
            if (db.beginGetPlaylist(name) == false)
                return playlist;
            mediaInfo url = db.getNextPlaylistItem();
            while (url != null)
            {
                playlist.Add(url);
                url = db.getNextPlaylistItem();
            }
            return playlist;
        }

        /// <summary>
        /// Retrieves a playlist
        /// </summary>
        /// <param name="location">The playlist location</param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylist(string location)
        {
            string ext = location.ToLower(); //linux is case sensative so dont alter location

            switch (ext.Substring(ext.Length - 4, 4))
            {
                case ".m3u":
                    return readM3U(location);
                case ".wpl":
                    return readWPL(location);
                case ".pls":
                    return readPLS(location);
                case ".asx":
                case ".wax":
                case ".wvx":
                    return readASX(location);
                case "xspf":
                    return readXSPF(location);
                case "cast": //podcast (.pcast)
                    return readPCAST(location);
            }
            return null;
        }

        private static List<mediaInfo> readPCAST(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            XmlDocument reader = new XmlDocument();
            reader.XmlResolver = null;
            reader.Schemas.XmlResolver = null;
            reader.Load(location);
            XmlNodeList nodes = reader.DocumentElement.SelectNodes("/pcast");
            foreach (XmlNode channel in nodes[0].ChildNodes)
            {
                mediaInfo info = new mediaInfo();
                foreach (XmlNode node in channel.ChildNodes)
                {
                    if (node.Name == "link")
                        info.Location = node.Attributes["href"].Value;
                    else if (node.Name == "title")
                        info.Name = node.InnerText;
                    else if (node.Name == "category")
                        info.Genre = node.InnerText;
                    else if (node.Name == "subtitle")
                        info.Album = node.InnerText;
                }
                playlist.Add(info);
            }
            return playlist;
        }
        private static List<mediaInfo> readXSPF(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            XmlDocument reader = new XmlDocument();
            reader.Load(location);
            XmlNodeList nodes = reader.ChildNodes[1].ChildNodes[1].ChildNodes;
            foreach (XmlNode node in nodes)
            {
                mediaInfo info = new mediaInfo();
                foreach (XmlNode n in node.ChildNodes)
                    switch (n.Name.ToLower())
                    {
                        case "title":
                            info.Name = n.InnerText;
                            break;
                        case "location":
                            info.Location = n.InnerText;
                            break;
                        case "creator":
                            info.Artist = n.InnerText;
                            break;
                        case "album":
                            info.Album = n.InnerText;
                            break;
                        case "tracknum":
                            if (n.InnerText.Length > 0)
                                info.TrackNumber = int.Parse(n.InnerText);
                            break;
                        case "image":
                            if (n.InnerText.Length > 0)
                                info.coverArt = Network.imageFromURL(n.InnerText.Substring(0, n.InnerText.IndexOf("%3B")));
                            break;
                    }
                playlist.Add(info);
            }
            return playlist;
        }
        private static List<mediaInfo> readASX(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            XmlDocument reader = new XmlDocument();
            reader.Load(location);
            XmlNodeList nodes = reader.DocumentElement.SelectNodes("/asx/entry");
            foreach (XmlNode node in nodes)
            {
                mediaInfo info = new mediaInfo();
                foreach (XmlNode n in node.ChildNodes)
                    if (n.Name.ToLower() == "title")
                        info.Name = n.InnerText;
                    else if (n.Name.ToLower() == "ref")
                        info.Location = n.Attributes["href"].Value;
                    else if (n.Name.ToLower() == "author")
                        info.Artist = n.InnerText;
                playlist.Add(info);
            }
            return playlist;
        }
        private static List<mediaInfo> readWPL(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            XmlDocument reader = new XmlDocument();
            reader.Load(location);
            XmlNodeList nodes = reader.DocumentElement.SelectNodes("/smil/body/seq");
            foreach (XmlNode node in nodes[0].ChildNodes)
            {
                string s = node.Attributes["src"].Value;
                if (System.IO.Path.IsPathRooted(s) == true)
                {
                    playlist.Add(new mediaInfo(s));
                }
                else
                {
                    if (s.StartsWith("..") == true)
                        s = System.IO.Directory.GetParent(location).Parent.FullName + s.Substring(2);
                    playlist.Add(new mediaInfo(s));
                }
            }
            return playlist;
        }
        private static List<mediaInfo> readM3U(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            StreamReader reader = new StreamReader(location);
            while (reader.EndOfStream == false)
            {
                string str = reader.ReadLine();
                if (str.StartsWith("#") == false)
                {
                    playlist.Add(new mediaInfo(System.IO.Path.GetFullPath(str)));
                }
            }
            return playlist;
        }
        private static List<mediaInfo> readPLS(string location)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            StreamReader reader = new StreamReader(location);
            while (reader.EndOfStream == false)
            {
                string str = reader.ReadLine();
                if (str.StartsWith("File") == true)
                {
                    str = str.Substring(str.IndexOf('=') + 1);
                    playlist.Add(new mediaInfo(System.IO.Path.GetFullPath(str)));
                }
            }
            return playlist;
        }
        /// <summary>
        /// Convert a list to a Playlist
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<mediaInfo> Convert(List<string> source)
        {
            List<mediaInfo> ret = new List<mediaInfo>(source.Count);
            for (int i = 0; i < source.Count; i++)
                ret.Add(new mediaInfo(source[i]));
            return ret;
        }
        /// <summary>
        /// Convert an array of strings to a Playlist
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<mediaInfo> Convert(string[] source)
        {
            List<mediaInfo> ret = new List<mediaInfo>(source.Length);
            for (int i = 0; i < source.Length; i++)
                ret.Add(new mediaInfo(source[i]));
            return ret;
        }
    }
}