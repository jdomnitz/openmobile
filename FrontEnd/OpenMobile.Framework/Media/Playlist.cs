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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using OpenMobile.Net;
using OpenMobile.Plugin;
using System.Threading;
using OpenMobile.Data;

namespace OpenMobile.Media
{
    /// <summary>
    /// Playlist types
    /// </summary>
    public enum ePlaylistType
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
    public static class Playlist
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
            writePlaylist(location, type, Convert(playlist));
        }
        /// <summary>
        /// Writes a playlist to a file
        /// </summary>
        /// <param name="location">The location to write to</param>
        /// <param name="type">The type of playlist to write</param>
        /// <param name="playlist">The playlist</param>
        /// <returns></returns>
        [Obsolete("NOT YET IMPLEMENTED",true)]
        public static bool writePlaylist(string location, ePlaylistType type, List<mediaInfo> playlist)
        {
            //TODO
            throw new NotImplementedException();
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
            object o;
            using (PluginSettings s = new PluginSettings())
                theHost.getData(eGetData.GetMediaDatabase, s.getSetting("Default.MusicDatabase"), out o);
            if (o == null)
                return false;
            IMediaDatabase db = (IMediaDatabase)o;
            return db.writePlaylist(playlist.ConvertAll<string>(convertMediaInfo),name,false);
        }

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="theHost"></param>
        /// <param name="name"></param>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static bool deletePlaylistFromDB(IPluginHost theHost, string name)
        {
            object o;
            using (PluginSettings s = new PluginSettings())
                theHost.getData(eGetData.GetMediaDatabase, s.getSetting("Default.MusicDatabase"), out o);
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
            List<string> ret=new List<string>();
            string[] filter = new string[] { "*.m3u", "*.wpl", "*.pls", "*.asx", "*.wax", "*.wvx", "*.xspf","*.pcast" };
            for (int i = 0; i < filter.Length; i++)
                ret.AddRange(Directory.GetFiles(directory, filter[i]));
            ret.Sort();
            ret.TrimExcess();
            return ret;
        }
        public static List<string> listPlaylistsFromDB(IPluginHost theHost)
        {
            string dbName = "";
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting("Default.MusicDatabase");
            return listPlaylistsFromDB(theHost, dbName);
        }
        public static List<string> listPlaylistsFromDB(IPluginHost theHost,string dbName)
        {
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
            object o=null;
            List<mediaInfo> playlist = new List<mediaInfo>();
            string dbName="";
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting("Default.MusicDatabase");
            for (int i = 0; ((i < 35)&&(o==null)); i++)
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
            string url=db.getNextPlaylistItem();
            while (url != null)
            {
                playlist.Add(new mediaInfo(url));
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
            
            switch(ext.Substring(ext.Length-4,4))
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
                    switch(n.Name.ToLower())
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
                mediaInfo info=new mediaInfo();
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
                string s=node.Attributes["src"].Value;
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
            StreamReader reader=new StreamReader(location);
            while(reader.EndOfStream==false)
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
                    str = str.Substring(str.IndexOf('=')+1);
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
