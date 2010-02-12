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
        ASPF
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
                    return readASPF(location);
            }
            return null;
        }

        private static List<mediaInfo> readASPF(string location)
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
    }
}
