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
using System.Threading;
using System.Xml;
using OpenMobile.Data;
using OpenMobile.Net;
using OpenMobile.Plugin;
using OpenMobile.Graphics;

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
    /// A media playlist 
    /// </summary>
    public class PlayList
    {
        private const int _BufferSize = 20;

        #region Properties

        private bool _Random = false;
        /// <summary>
        /// Use random playback
        /// </summary>
        public bool Random
        {
            get
            {
                return _Random;
            }
            set
            {
                if (value == _Random)
                    return;

                _Random = value;

                // Regenerate queue
                GenerateQueue(true);
            }
        }

        private List<mediaInfo> _Items = new List<mediaInfo>();
        /// <summary>
        /// PlayList media items
        /// </summary>
        public List<mediaInfo> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                _Items = value;
            }
        }

        private LinkedList<int> _HistoryItems = new LinkedList<int>();
        /// <summary>
        /// PlayList history items
        /// </summary>
        public mediaInfo[] HistoryItems
        {
            get
            {
                List<mediaInfo> LocalItems = new List<mediaInfo>();
                foreach (int index in _HistoryItems)
                    LocalItems.Add(_Items[index]);
                return LocalItems.ToArray();
            }
        }

        private LinkedList<int> _QueuedItems = new LinkedList<int>();
        /// <summary>
        /// PlayList history items
        /// </summary>
        public mediaInfo[] QueuedItems
        {
            get
            {
                List<mediaInfo> LocalItems = new List<mediaInfo>();
                foreach (int index in _QueuedItems)
                    LocalItems.Add(_Items[index]);
                return LocalItems.ToArray();
            }
        }

        /// <summary>
        /// An array with all buffered items (history -> CurrentItem -> Queued)
        /// <para>CurrentItem index is read from property BufferListIndex</para>
        /// </summary>
        public mediaInfo[] BufferList
        {
            get
            {
                List<mediaInfo> LocalItems = new List<mediaInfo>();

                // Add history items
                foreach (int index in _HistoryItems)
                    LocalItems.Add(_Items[index]);

                // Add current item
                LocalItems.Add(CurrentItem);

                // Set location of current item
                _BufferListIndex = LocalItems.Count - 1;

                // Add queued items
                foreach (int index in _QueuedItems)
                    LocalItems.Add(_Items[index]);

                return LocalItems.ToArray();
            }
        }
        
        /// <summary>
        /// The index where the currentitem is located in the BufferList array
        /// <para>This property indicates the transition from historyitems to queued items</para>
        /// </summary>
        public int BufferListIndex
        {
            get
            {
                return _BufferListIndex;
            }
        }
        private int _BufferListIndex = 0;

        private string _Name = "";
        /// <summary>
        /// PlayList name
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        #endregion

        #region Current item

        private int _CurrentIndex = 0;
        /// <summary>
        /// Gets or sets the currently active index in the list
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return _CurrentIndex;
            }
            set
            {
                if (value == _CurrentIndex)
                    return;

                // Save old item to history
                SaveToHistory(_CurrentIndex);

                // Activate new item
                _CurrentIndex = value;

                // Remove items from the queue to match the selected item (if present in queue)
                if (_QueuedItems.Contains(_CurrentIndex))
                {
                    while (_QueuedItems.Count > 0)
                    {
                        int i = _QueuedItems.First.Value;
                        _QueuedItems.RemoveFirst();

                        // Cancel when current index is found
                        if (i == _CurrentIndex)
                            break;
                    }
                }

                GenerateQueue(false);
            }
        }
        /// <summary>
        /// Gets or sets the currently active mediaInfo item (must be an item already loaded into the playlist)
        /// </summary>
        public mediaInfo CurrentItem
        {
            get
            {
                if (_CurrentIndex > (_Items.Count - 1))
                    return null;
                return _Items[_CurrentIndex];
            }
            set
            {
                if (value == _Items[_CurrentIndex])
                    return;

                if (!_Items.Contains(value))
                    return;
                
                CurrentIndex = _Items.FindIndex(x => x == value);
            }
        }

        #endregion

        #region Next item

        private int _NextIndex = 0;
        /// <summary>
        /// Next index to be activated in the list
        /// </summary>
        public int NextIndex
        {
            get
            {
                return _NextIndex;
            }
            set
            {
                _NextIndex = value;
            }
        }
        /// <summary>
        /// Gets the next mediaInfo item to be activated
        /// </summary>
        public mediaInfo NextItem
        {
            get
            {
                if (_NextIndex > (_Items.Count - 1))
                    return null;
                return _Items[_NextIndex];
            }
        }

        #endregion

        #region Add items

        /// <summary>
        /// Adds a new media item to the playlist
        /// </summary>
        /// <param name="item"></param>
        public void Add(mediaInfo item)
        {
            _Items.Add(item);
            GenerateQueue(false);
        }
        /// <summary>
        /// Adds a new media item to the playlist from a given location
        /// </summary>
        /// <param name="item"></param>
        public void Add(string location)
        {
            _Items.Add(new mediaInfo(location));
            GenerateQueue(false);
        }
        /// <summary>
        /// Adds a new media item to the playlist if it's not already in the list
        /// </summary>
        /// <param name="item"></param>
        public void AddDistinct(mediaInfo item)
        {
            // Only add item if not already present in items
            if (_Items.Find(x => x.Location == item.Location) == null)
            {
                _Items.Add(item);
                GenerateQueue(false);
            }
        }

        #endregion

        #region Clear items

        public void Clear()
        {
            _QueuedItems.Clear();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new playlist
        /// </summary>
        public PlayList()
        {
        }
        /// <summary>
        /// Create a new playlist
        /// </summary>
        /// <param name="Name"></param>
        public PlayList(string name)
            : this()
        {
            this.Name = name;
        }

        #endregion

        #region Queue control 

        /// <summary>
        /// Save an index to the history
        /// </summary>
        /// <param name="Index"></param>
        private void SaveToHistory(int Index)
        {
            // Save old item to history
            _HistoryItems.AddLast(_CurrentIndex);

            // Limit size of history 
            if (_HistoryItems.Count > _BufferSize)
                _HistoryItems.RemoveFirst();
        }

        /// <summary>
        /// Get and remove an item from the history
        /// </summary>
        /// <returns></returns>
        private int GetFromHistory()
        {
            if (_HistoryItems.Count == 0)
                return -1;

            int Index = _HistoryItems.Last.Value;
            _HistoryItems.RemoveLast();

            return Index;
        }

        /// <summary>
        /// Changes active item to next media 
        /// </summary>
        public bool GotoNextMedia()
        {
            lock (this)
            {
                // Save old item to history
                SaveToHistory(_CurrentIndex);

                // Just move next index to current index
                _CurrentIndex = _QueuedItems.First.Value;

                // Remove item from queue
                _QueuedItems.RemoveFirst();

                // Update next items
                GenerateQueue(false);
            }
            return true;
        }

        /// <summary>
        /// Changes active item to previous media 
        /// </summary>
        public bool GotoPreviousMedia()
        {
            lock (this)
            {
                // Move back trough history
                int HistoryItem = GetFromHistory();
                if (HistoryItem < 0)
                    return false;

                // Push current item back into active queue
                _QueuedItems.AddFirst(_CurrentIndex);

                // Set history item as active
                _CurrentIndex = HistoryItem;
            }
            return true;
        }

        private void GenerateQueue(bool ClearQueue)
        {
            lock (this)
            {
                if (_Items.Count == 0)
                    return;

                if (ClearQueue)
                    _QueuedItems.Clear();

                // Fill buffer with items (calculates remaining space)
                int ItemsToAdd = _BufferSize - _QueuedItems.Count;

                // Is free space higher than available items, if so limit it
                if (ItemsToAdd >= _Items.Count) ItemsToAdd = _Items.Count;

                // Check if we already have added everything we can
                if ((_Items.Count - _QueuedItems.Count) > 0)
                    ItemsToAdd = _Items.Count - _QueuedItems.Count;

                int newIndex = -1;
                for (int i = 0; i < ItemsToAdd; i++)
                {
                    if (Random)
                    {   // Random playback
                        do
                            newIndex = OpenMobile.Framework.Math.Calculation.RandomNumber(0, _Items.Count - 1);
                        while (_QueuedItems.Contains(newIndex) || newIndex == _CurrentIndex);
                    }
                    else
                    {   // Sequencial playback

                        if (_QueuedItems.Count > 0)
                            // Use the next available index after the last item in the queue as the index to add to queue
                            newIndex = _QueuedItems.Last.Value + 1;
                        else
                            newIndex = _CurrentIndex + 1;

                        // Ensure we don't go outside the limits, if so wrap index to start of items
                        if (newIndex >= _Items.Count)
                            newIndex = 0;
                    }

                    // Add items to queue
                    _QueuedItems.AddLast(newIndex);
                }
            }
        }

        #endregion

        #region Playlist handling

        /// <summary>
        /// Saves this playlist to the DB
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return writePlaylistToDB(_Name, _Items);
        }

        /// <summary>
        /// Loads this playlist from the FB
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            _Items = readPlaylistFromDB(_Name);
            GenerateQueue(true);
            return true;
        }

        #endregion

        
        #region Static methods

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
        /// <param name="name"></param>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static bool writePlaylistToDB(string name, List<mediaInfo> playlist)
        {
            if (playlist.Count == 0)
                return false;
            object o;
            using (PluginSettings s = new PluginSettings())
                BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"), out o);
            if (o == null)
                return false;
            IMediaDatabase db = (IMediaDatabase)o;
            return db.writePlaylist(playlist.ConvertAll<string>(convertMediaInfo), name, false);
        }

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool deletePlaylistFromDB(string name)
        {
            object o;
            using (PluginSettings s = new PluginSettings())
                BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"), out o);
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
        /// <returns></returns>
        public static List<string> listPlaylistsFromDB()
        {
            string dbName = String.Empty;
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
            return listPlaylistsFromDB(dbName);
        }
        /// <summary>
        /// Lists playlists available in the given media database
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<string> listPlaylistsFromDB(string dbName)
        {
            object o = null;
            for (int i = 0; ((i < 35) && (o == null)); i++)
            {
                BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName, out o);
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
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(string name)
        {
            string dbName = String.Empty;
            using (PluginSettings s = new PluginSettings())
                dbName = s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
            return readPlaylistFromDB(name, dbName);
        }
        /// <summary>
        /// Reads the given playlist from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(string name, string dbName)
        {
            object o = null;
            List<mediaInfo> playlist = new List<mediaInfo>();
            for (int i = 0; ((i < 35) && (o == null)); i++)
            {
                BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName, out o);
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

        #endregion
    }
}

