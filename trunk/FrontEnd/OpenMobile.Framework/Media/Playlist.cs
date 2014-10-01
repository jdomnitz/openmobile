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
using System.ComponentModel;
using System.Text;

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
    public class PlayList : INotifyPropertyChanged
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
                this.OnPropertyChanged("Random");
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
                this.OnPropertyChanged("Items");
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
                this.OnPropertyChanged("Name");
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
                this.OnPropertyChanged("CurrentIndex");
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
                this.OnPropertyChanged("CurrentItem");
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
                this.OnPropertyChanged("NextIndex");
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
            this.OnPropertyChanged("Items");
        }
        /// <summary>
        /// Adds a new media item to the playlist from a given location
        /// </summary>
        /// <param name="item"></param>
        public void Add(string location)
        {
            _Items.Add(new mediaInfo(location));
            GenerateQueue(false);
            this.OnPropertyChanged("Items");
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
                this.OnPropertyChanged("Items");
            }
        }

        #endregion

        #region Clear items

        public void Clear()
        {
            _QueuedItems.Clear();
            _HistoryItems.Clear();
            _Items.Clear();
            this.OnPropertyChanged("Items");
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
            this.OnPropertyChanged("CurrentIndex");
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
            this.OnPropertyChanged("CurrentIndex"); 
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

                    // Add items to queue (only if not the current one)
                    if (newIndex != CurrentIndex)
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
            return db.writePlaylist(playlist, name, false);
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

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Propertychanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    public class PlayList2 : INotifyPropertyChanged
    {
        /// <summary>
        /// Display name (if provided)
        /// </summary>
        static public string GetDisplayName(string name)
        {
            if (name.Contains("|"))
                return name.Substring(name.IndexOf("|") + 1);
            return name;
        }

            
        static public void SetDisplayName(ref string name, string displayName)
        {
            string currentDisplayName = GetDisplayName(name);

            if (name.Contains("|"))
                name.Replace(String.Format("|{0}", currentDisplayName), String.Format("|{0}", displayName));
            else
                name += String.Format("|{0}", displayName);
        }


        #region Constructor

        public PlayList2()
        {
        }
        public PlayList2(string name)
            : this()
        {
            this._Name = name;
        }
        #endregion

        /// <summary>
        /// Does this playlist contain items?
        /// </summary>
        public bool HasItems
        {
            get
            {
                return this._Items.Count > 0;
            }
        }        

        /// <summary>
        /// The amount of items in history 
        /// </summary>
        public int BufferSize
        {
            get
            {
                return this._BufferSize;
            }
        }
        private int _BufferSize = 20;

        /// <summary>
        /// Gets or sets the contained items
        /// </summary>
        public List<mediaInfo> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                if (this._Items != value)
                {
                    this._Items = value;
                    ResetListData();
                    this.OnPropertyChanged("Items");
                    Raise_OnList_Items_Changed();
                }
            }
        }
        private List<mediaInfo> _Items = new List<mediaInfo>();

        /// <summary>
        /// The indexes of the items in history
        /// </summary>
        public LinkedList<int> History
        {
            get
            {
                return this._History;
            }
            set
            {
                if (this._History != value)
                {
                    this._History = value;
                }
            }
        }
        private LinkedList<int> _History = new LinkedList<int>();

        /// <summary>
        /// The indexes of the queued items
        /// </summary>
        public LinkedList<int> Queue
        {
            get
            {
                return this._Queue;
            }
            set
            {
                if (this._Queue != value)
                {
                    this._Queue = value;
                }
            }
        }
        private LinkedList<int> _Queue = new LinkedList<int>();        
        
        /// <summary>
        /// Gets or sets the currently active index
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return this._CurrentIndex;
            }
            set
            {
                if (this._CurrentIndex != value)
                {
                    this._CurrentIndex = value;
                    this.OnPropertyChanged("CurrentItem");
                }
            }
        }
        private int _CurrentIndex;

        /// <summary>
        /// Sets the current starting point for the playlist (also resets history and regenerates the queue)
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentStartIndex(int index)
        {
            this._CurrentIndex = index;
            ResetListData(false, true);
            GenerateQueue();
            this.OnPropertyChanged("CurrentItem");
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
                this.OnPropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// Gets or sets the random playback state
        /// </summary>
        public bool Random
        {
            get
            {
                return this._Random;
            }
            set
            {
                if (this._Random != value)
                {
                    this._Random = value;
                    ResetListData();
                    GenerateQueue();
                    this.OnPropertyChanged("Random");
                }
            }
        }
        private bool _Random;

        /// <summary>
        /// Gets or sets the repeat playback state
        /// </summary>
        public bool Repeat
        {
            get
            {
                return this._Repeat;
            }
            set
            {
                if (this._Repeat != value)
                {
                    this._Repeat = value;
                }
            }
        }
        private bool _Repeat;        

        /// <summary>
        /// Sets or gets the name of the playlist
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        private string _Name;


        private void ResetListData(bool resetCurrentIndex = true, bool resetHistory = true)
        {
            if (resetHistory)
                _History.Clear();
            _Queue.Clear();
            _Buffer.Clear();
            if (resetCurrentIndex)
                CurrentIndex = 0;
            Raise_OnList_Buffer_Cleared();
        }

        private void History_Clear()
        {
            _History.Clear();
            Raise_OnList_Buffer_Cleared();
        }

        private int History_Push()
        {
            int nextIndex = _Queue.First.Value;

            // Remove item from queue
            _Queue.RemoveFirst();

            _History.AddLast(_CurrentIndex);
            if (_History.Count > _BufferSize)
                _History.RemoveFirst();

            Raise_OnList_Buffer_ItemInserted(_History.Count - 1 + 1 + _Queue.Count - 1);

            return nextIndex;
        }

        private int History_Pop()
        {
            // Move back trough history
            int nextIndex = _History.Last.Value;

            // Remove item from history
            _History.RemoveLast();

            // Push item back to queue
            _Queue.AddFirst(_CurrentIndex);

            Raise_OnList_Buffer_ItemRemoved(0);

            return nextIndex;
        }

        #region Item control

        /// <summary>
        /// Adds a new media item to the playlist
        /// </summary>
        /// <param name="item"></param>
        public void Add(mediaInfo item)
        {
            _Items.Add(item);
            GenerateQueue();
            this.OnPropertyChanged("Items");
            Raise_OnList_Items_ItemInserted(_Items.Count - 1);
        }
        /// <summary>
        /// Adds a new media item to the playlist from a given location
        /// </summary>
        /// <param name="item"></param>
        public void Add(string location)
        {
            _Items.Add(new mediaInfo(location));
            GenerateQueue();
            this.OnPropertyChanged("Items");
            Raise_OnList_Items_ItemInserted(_Items.Count - 1);
        }

        /// <summary>
        /// Adds multiple items to the playlist
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<mediaInfo> items)
        {
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        /// Adds multiple items to the playlist if it's not already in the list
        /// </summary>
        /// <param name="items"></param>
        public void AddRangeDistinct(IEnumerable<mediaInfo> items)
        {
            foreach (var item in items)
                AddDistinct(item);
        }
        /// <summary>
        /// Adds a new media item to the playlist if it's not already in the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns true if item was added</returns>
        public bool AddDistinct(mediaInfo item)
        {
            // Only add item if not already present in items
            if (_Items.Find(x => x.Location == item.Location) == null)
            {
                _Items.Add(item);
                GenerateQueue();
                this.OnPropertyChanged("Items");
                Raise_OnList_Items_ItemInserted(_Items.Count - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes an item from this playlist
        /// </summary>
        /// <param name="item"></param>
        public void Remove(mediaInfo item)
        {
            int index = _Items.IndexOf(item);
            Remove(index);
        }
        /// <summary>
        /// Removes an item from this playlist
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            _Items.RemoveAt(index);
            this.OnPropertyChanged("Items");
            GenerateQueue();
            Raise_OnList_Items_ItemRemoved(index);
        }

        public bool SetNextMedia(int index)
        {
            lock (this)
            {
                _Queue.AddFirst(index);
                Raise_OnList_Buffer_ItemInserted(_History.Count - 1 + 1 + 1);
                Buffer_Refresh(true);
            }
            return true;
        }

        #endregion

        public void Clear()
        {
            _Items.Clear();
            ResetListData();
            this.OnPropertyChanged("Items");
            Raise_OnList_Items_Cleared();
        }

        private void GenerateQueue(bool raiseEvents = true)
        {
            lock (_Items)
            {
                // Cancel if nothing is available
                if (_Items.Count == 0)
                {
                    ResetListData();
                    return;
                }

                // Calculate items to add
                int addCount = System.Math.Min(_Items.Count, _BufferSize) - _Queue.Count;

                // Fill queue starting from the first free position in the queue
                for (int i = 0; i < addCount; i++)
                {
                    if (_Random && _Items.Count > 1)
                    {
                        // Find a random items that's not already in the history
                        int newIndex = OpenMobile.Framework.Math.Calculation.RandomNumber(0, _Items.Count - 1);
                        while (_History.Contains(newIndex) || _Queue.Contains(newIndex) || newIndex == _CurrentIndex)
                            newIndex = OpenMobile.Framework.Math.Calculation.RandomNumber(0, _Items.Count - 1);

                        // Add to queue
                        _Queue.AddLast(newIndex);
                    }
                    else
                    {   // Add to queue
                        if (_Queue.Count > 0)
                        {
                            var index = _Queue.Last.Value;
                            if (index < 0)
                                index = 0;
                            if (index >= _Items.Count)
                                index = _Items.Count - 2;
                            _Queue.AddLast(index + 1);
                        }
                        else
                        {
                            if (_Items.Count == 1)
                            {
                                _CurrentIndex = 0;
                                _Queue.AddLast(_CurrentIndex);
                            }
                            else
                            {
                                if (_CurrentIndex < 0)
                                    _CurrentIndex = 0;
                                if (_CurrentIndex >= _Items.Count)
                                    _CurrentIndex = _Items.Count - 2;
                                //if (_Items.Count > _CurrentIndex + 1)
                                _Queue.AddLast(_CurrentIndex + 1);
                            }
                        }
                    }
                }
            }

            Buffer_Refresh(raiseEvents);
        }

        /// <summary>
        /// Changes active item to next media 
        /// </summary>
        public bool GotoNextMedia()
        {
            lock (this)
            {
                // Do we have a valid queue?
                if (_Queue.Count == 0)
                    return false;

                //int nextIndex = History_Push();

                int nextIndex = _Queue.First.Value;

                if (nextIndex < 0)
                    nextIndex = _Items.Count - 1;
                if (nextIndex >= _Items.Count)
                    nextIndex = 0;

                // Remove item from queue
                _Queue.RemoveFirst();

                bool historyItemRemoved = false;

                //if (_CurrentIndex < 0)
                //    _CurrentIndex = 0;
                //if (_CurrentIndex >= _Items.Count)
                //    _CurrentIndex = _Items.Count - 1;

                // Add item to history
                _History.AddLast(_CurrentIndex);
                if (_History.Count > _BufferSize)
                {
                    _History.RemoveFirst();
                    historyItemRemoved = true;
                }

                // Go to next item in queue
                CurrentIndex = nextIndex;

                // Generate new items to list
                GenerateQueue(false);

                if (historyItemRemoved)
                    Raise_OnList_Buffer_ItemRemoved(0);

                Raise_OnList_Buffer_ItemInserted(_History.Count - 1 + 1 + _Queue.Count - 1);
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
                // Do we have a valid history?
                if (_History.Count == 0)
                    return false;

                //int nextIndex = History_Pop();

                // Move back trough history
                int nextIndex = _History.Last.Value;

                if (_CurrentIndex < 0)
                    _CurrentIndex = 0;
                if (_CurrentIndex >= _Items.Count)
                    _CurrentIndex = _Items.Count - 1;

                // Remove item from history
                _History.RemoveLast();

                if (nextIndex >= 0 && nextIndex < _Items.Count)
                {
                    // Push item back to queue
                    _Queue.AddFirst(_CurrentIndex);

                    // Go to next item in queue
                    CurrentIndex = nextIndex;
                }

                GenerateQueue(false);

                Raise_OnList_Buffer_ItemInserted(_History.Count - 1 + 1 + _Queue.Count - 1);
            }
            return true;
        }


        /// <summary>
        /// The buffered list
        /// </summary>
        public List<mediaInfo> Buffer
        {
            get
            {
                return this._Buffer;
            }
            set
            {
                if (this._Buffer != value)
                {
                    this._Buffer = value;
                }
            }
        }
        private List<mediaInfo> _Buffer = new List<mediaInfo>();

        /// <summary>
        /// The active index in the buffer
        /// </summary>
        public int BufferIndex
        {
            get
            {
                return this._History.Count;
            }
        }

        /// <summary>
        /// Refresh the mediaInfo list for buffer visualization
        /// </summary>
        private void Buffer_Refresh(bool raiseEvents = true)
        {
            lock (_Buffer)
            {
                _Buffer.Clear();

                // Add history items
                foreach (int index in _History)
                    _Buffer.Add(_Items[index]);

                // Add current item
                _Buffer.Add(_Items[_CurrentIndex]);

                // Add queue items
                foreach (int index in _Queue)
                {
                    if (_Items.Count > index)
                        _Buffer.Add(_Items[index]);
                }
            }
            if (raiseEvents)
                Raise_OnList_Buffer_Changed();
        }

        #region Playlist handling

        /// <summary>
        /// Saves this playlist to the DB
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            // Save current playlist order to the DB
            StringBuilder sb = new StringBuilder();
            foreach (var index in _Queue)
            {
                sb.Append(index);
                sb.Append("|");
            }
            writePlayListSettingToDB(_Name, "Queue", sb.ToString());

            // Save settings to database
            writePlayListSettingToDB(_Name, "Random", _Random.ToString());
            writePlayListSettingToDB(_Name, "Repeat", _Repeat.ToString());
            writePlayListSettingToDB(_Name, "CurrentIndex", _CurrentIndex.ToString());

            return writePlaylistToDB(_Name, _Items);
        }

        /// <summary>
        /// Loads this playlist from the DB
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            try
            {
                _Items = readPlaylistFromDB(_Name);
                //GenerateQueue();

                // Load queue from DB
                string gueueString = readPlayListSettingFromDB(_Name, "Queue");
                if (gueueString != null)
                {
                    string[] gueueStringSplit = gueueString.Split('|');
                    if (gueueStringSplit.Length > 0)
                    {
                        _Queue.Clear();
                        for (int i = 0; i < gueueStringSplit.Length; i++)
                        {
                            int index = 0;
                            if (int.TryParse(gueueStringSplit[i], out index))
                                _Queue.AddLast(index);
                        }
                    }
                }

                // Load settings from database
                int.TryParse(readPlayListSettingFromDB(_Name, "CurrentIndex"), out _CurrentIndex);
                bool.TryParse(readPlayListSettingFromDB(_Name, "Random"), out _Random);
                bool.TryParse(readPlayListSettingFromDB(_Name, "Repeat"), out _Repeat);

                Buffer_Refresh(true);

                Raise_OnList_Items_Changed();
                return true;
            }
            catch
            {
                return false;
            }
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
        //public static bool writePlaylistToDB(string name, List<mediaInfo> playlist)
        //{
        //    if (playlist.Count == 0)
        //        return false;
        //    IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
        //    if (db == null)
        //        return false;
        //    return db.writePlaylist(playlist, name, false);
        //}

        public static bool writePlaylistToDB(string name, List<mediaInfo> playlist)
        {
            // Do we have something to write?
            if (playlist.Count == 0)
                return false;

            // Get database object
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;

            return db.writePlaylist(playlist, name, false);
        }

        //public static bool writePlaylistToDB(string name, List<mediaInfo> playlist)
        //{
        //    if (playlist.Count == 0)
        //        return false;
        //    object o;
        //    using (PluginSettings s = new PluginSettings())
        //        BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"), out o);
        //    if (o == null)
        //        return false;
        //    IMediaDatabase db = (IMediaDatabase)o;
        //    return db.writePlaylist(playlist.ConvertAll<string>(convertMediaInfo), name, false);
        //}

        public static bool writePlaylistToDBasObject(string name, List<mediaInfo> playlist)
        {
            if (playlist.Count == 0)
                return false;

            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;

            //string xml = helperFunctions.XML.Serializer.toXML(playlist);

            //return db.writePlaylist(new List<string>() { helperFunctions.XML.Serializer.toXML(playlist) }, name, false);

            return db.writePlaylist(playlist, name, false);
        }


        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool deletePlaylistFromDB(string name)
        {
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;
            return db.removePlaylist(name);
        }

        private static string convertMediaInfo(mediaInfo info)
        {
            //return info.Location;
            return String.Format("{0}|{1}|{2}",info.Artist, info.Album, info.Name);
        }

        private static string convertMedaiaInfoToXML(mediaInfo info)
        {
            return helperFunctions.XML.Serializer.toXML(info);
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
            return readPlaylistFromDB(name, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        }

        //public static List<mediaInfo> readPlaylistFromDBasObject(string name)
        //{
        //    return readPlaylistFromDBasObject(name, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        //}

        /// <summary>
        /// Reads the given playlist from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(string name, string dbName)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
            if (db == null)
                return playlist;

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
        /// Reads a playlist setting from the database
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static string readPlayListSettingFromDB(string playlistName, string settingName)
        {
            return readPlayListSettingFromDB(playlistName, settingName, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        }

        /// <summary>
        /// Reads a playlist setting from the database
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="settingName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static string readPlayListSettingFromDB(string playlistName, string settingName, string dbName)
        {
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
            if (db == null)
                return null;

            return db.getMediaSetting(playlistName, settingName);
        }

        /// <summary>
        /// Writes a playlist setting to the database
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        public static void writePlayListSettingToDB(string playlistName, string settingName, string settingValue)
        {
            writePlayListSettingToDB(playlistName, settingName, settingValue, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        }

        /// <summary>
        /// Writes a playlist setting to the database
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        /// <param name="dbName"></param>
        public static void writePlayListSettingToDB(string playlistName, string settingName, string settingValue, string dbName)
        {
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
            if (db == null)
                return;

            db.setMediaSetting(playlistName, settingName, settingValue);
        }

        ///// <summary>
        ///// Reads the given playlist from the database
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="dbName"></param>
        ///// <returns></returns>
        //public static List<mediaInfo> readPlaylistFromDB(string name, string dbName)
        //{
        //    List<mediaInfo> playlist = new List<mediaInfo>();
        //    IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
        //    if (db == null)
        //        return playlist;

        //    if (db.beginGetPlaylist(name) == false)
        //        return playlist;
        //    mediaInfo url = db.getNextPlaylistItem();
        //    while (url != null)
        //    {
        //        playlist.Add(url);
        //        url = db.getNextPlaylistItem();
        //    }

        //    // Get additional info from DB
        //    for (int i = 0; i < playlist.Count; i++)
        //    {
        //        playlist[i] = db.GetMediaInfoByUrl(playlist[i].Location);
        //    }

        //    return playlist;
        //}


        //public static List<mediaInfo> readPlaylistFromDBasObject(string name, string dbName)
        //{
        //    //List<mediaInfo> playlist = new List<mediaInfo>();
        //    //IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
        //    //if (db == null)
        //    //    return playlist;

        //    //if (db.beginGetPlaylist(name) == false)
        //    //    return playlist;

        //    //string xml = db.getNextPlaylistItemAsString();
        //    //if (string.IsNullOrEmpty(xml))
        //    //    return playlist;

        //    //try
        //    //{
        //    //    playlist = helperFunctions.XML.Serializer.fromXML<List<mediaInfo>>(xml);
        //    //}
        //    //catch
        //    //{
        //    //    playlist = new List<mediaInfo>();
        //    //}

        //    //return playlist;

        //    List<mediaInfo> playlist = new List<mediaInfo>();
        //    IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, dbName) as IMediaDatabase;
        //    if (db == null)
        //        return playlist;

        //    if (db.beginGetPlaylist(name) == false)
        //        return playlist;

        //    try
        //    {
        //        string xml = db.getNextPlaylistItemAsString();
        //        while (!string.IsNullOrEmpty(xml))
        //        {
        //            mediaInfo url = helperFunctions.XML.Serializer.fromXML<mediaInfo>(xml);
        //            playlist.Add(url);
        //            xml = db.getNextPlaylistItemAsString();
        //        }
        //    }
        //    catch
        //    {
        //        playlist = new List<mediaInfo>();
        //    }

        //    return playlist;

        //}

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

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Propertychanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Events

        public delegate void MediaListItemChangedEventHandler(List<mediaInfo> list, int index);

        public event MediaListItemChangedEventHandler OnList_Items_ItemInserted;
        public event MediaListItemChangedEventHandler OnList_Items_ItemRemoved;
        public event MediaListItemChangedEventHandler OnList_Items_Cleared;
        public event MediaListItemChangedEventHandler OnList_Items_Changed;

        private void Raise_OnList_Items_ItemInserted(int index)
        {
            if (OnList_Items_ItemInserted != null)
                OnList_Items_ItemInserted(_Items, index);
        }
        private void Raise_OnList_Items_ItemRemoved(int index)
        {
            if (OnList_Items_ItemRemoved != null)
                OnList_Items_ItemRemoved(_Items, index);
        }
        private void Raise_OnList_Items_Cleared()
        {
            if (OnList_Items_Cleared != null)
                OnList_Items_Cleared(_Items, 0);
        }
        private void Raise_OnList_Items_Changed()
        {
            if (OnList_Items_Changed != null)
                OnList_Items_Changed(_Items, 0);
        }

        public event MediaListItemChangedEventHandler OnList_Buffer_ItemInserted;
        public event MediaListItemChangedEventHandler OnList_Buffer_ItemRemoved;
        public event MediaListItemChangedEventHandler OnList_Buffer_Cleared;
        public event MediaListItemChangedEventHandler OnList_Buffer_Changed;

        private void Raise_OnList_Buffer_ItemInserted(int index)
        {
            if (OnList_Buffer_ItemInserted != null)
                OnList_Buffer_ItemInserted(_Buffer, index);
        }
        private void Raise_OnList_Buffer_ItemRemoved(int index)
        {
            if (OnList_Buffer_ItemRemoved != null)
                OnList_Buffer_ItemRemoved(_Buffer, index);
        }
        private void Raise_OnList_Buffer_Cleared()
        {
            if (OnList_Buffer_Cleared != null)
                OnList_Buffer_Cleared(_Buffer, 0);
        }
        private void Raise_OnList_Buffer_Changed()
        {
            if (OnList_Buffer_Changed != null)
                OnList_Buffer_Changed(_Buffer, 0);
        }


        #endregion

        //public void ReadMediaTags()
        //{
        //    for (int i = 0; i < _Items.Count; i++)
        //    {
        //        //if (abortJob[screen])
        //        //    return;
        //        //mediaInfo tmp = _Items[i];
        //        if (_Items[i].Name == null)
        //            _Items[i] = OpenMobile.Media.TagReader.getInfo(_Items[i].Location);
        //        if (_Items[i] == null)
        //            continue;
        //        if (_Items[i].coverArt == null)
        //            _Items[i].coverArt = TagReader.getCoverFromDB(_Items[i].Artist, _Items[i].Album);
        //        if (_Items[i].coverArt == null)
        //            _Items[i].coverArt = MediaLoader.MissingCoverImage;
        //    }
        //}

    }

/* Org code playlist 2
    public class PlayList2 : INotifyPropertyChanged
    {

        public PlayList2()
        {
        }
        public PlayList2(string name)
        {
            this._Name = name;
        }

        /// <summary>
        /// Does this playlist contain items?
        /// </summary>
        public bool HasItems
        {
            get
            {
                return this._Items.Count > 0;
            }
        }        

        /// <summary>
        /// The amount of items in history 
        /// </summary>
        public int HistorySize
        {
            get
            {
                return this._HistorySize;
            }
        }
        private int _HistorySize = 20;

        /// <summary>
        /// Index where the item list changes from history to queued items
        /// </summary>
        private int QueueIndex
        {
            get
            {
                return this._QueueIndex;
            }
            set
            {
                if (this._QueueIndex != value)
                {
                    this._QueueIndex = value;

                    // Limit index to history size
                    if (_QueueIndex >= _HistorySize)
                        _QueueIndex = _HistorySize;

                    // Limit index to list size
                    if (_QueueIndex >= _Items.Count)
                        _QueueIndex = _Items.Count;

                    // Limit index to positive values
                    if (_QueueIndex < 0)
                        _QueueIndex = 0;

                    // Set current index to match queue index
                    CurrentIndex = _QueueIndex;
                }
            }
        }
        private int _QueueIndex;        

        /// <summary>
        /// Gets or sets the contained items
        /// </summary>
        public List<mediaInfo> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                if (this._Items != value)
                {
                    this._Items = value;

                    // Reset current index
                    CurrentIndex = 0;

                    // Reset history
                    QueueIndex = 0;

                    this.OnPropertyChanged("Items");
                    Raise_OnListChanged();
                }
            }
        }
        private List<mediaInfo> _Items = new List<mediaInfo>();
        private List<mediaInfo> _ItemsCleanOrder = new List<mediaInfo>();

        /// <summary>
        /// Gets or sets the currently active index
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return this._CurrentIndex;
            }
            set
            {
                if (this._CurrentIndex != value)
                {
                    this._CurrentIndex = value;
                    this.OnPropertyChanged("CurrentItem");
                }
            }
        }
        private int _CurrentIndex;

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
                this.OnPropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// Gets or sets the random playback state
        /// </summary>
        public bool Random
        {
            get
            {
                return this._Random;
            }
            set
            {
                if (this._Random != value)
                {
                    if (this._Random & !value)
                    {   // Restore original list order to go back to linear playback
                        Items = new List<mediaInfo>(_ItemsCleanOrder);
                    }

                    this._Random = value;
                    GenerateQueue();
                    this.OnPropertyChanged("Random");
                }
            }
        }
        private bool _Random;

        /// <summary>
        /// Sets or gets the name of the playlist
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        private string _Name;        

        #region Add items

        /// <summary>
        /// Adds a new media item to the playlist
        /// </summary>
        /// <param name="item"></param>
        public void Add(mediaInfo item)
        {
            _Items.Add(item);
            _ItemsCleanOrder.Add(item);
            GenerateQueue();
            this.OnPropertyChanged("Items");
            Raise_OnListItemInserted(_Items.Count - 1);
        }
        /// <summary>
        /// Adds a new media item to the playlist from a given location
        /// </summary>
        /// <param name="item"></param>
        public void Add(string location)
        {
            _Items.Add(new mediaInfo(location));
            _ItemsCleanOrder.Add(_Items[_Items.Count - 1]);
            GenerateQueue();
            this.OnPropertyChanged("Items");
            Raise_OnListItemInserted(_Items.Count - 1);
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
                _ItemsCleanOrder.Add(item);
                GenerateQueue();
                this.OnPropertyChanged("Items");
                Raise_OnListItemInserted(_Items.Count - 1);
            }
        }

        #endregion

        public void Clear()
        {
            _Items.Clear();
            _ItemsCleanOrder.Clear();
            CurrentIndex = 0;
            QueueIndex = 0;
            this.OnPropertyChanged("Items");
            Raise_OnListCleared();
        }

        private void GenerateQueue()
        {
            lock (this)
            {
                // Cancel if nothing is available
                if (_Items.Count == 0)
                {
                    CurrentIndex = 0;
                    QueueIndex = 0;
                    return;
                }

                // Random playback? If so generate a random queue from current index and up
                if (Random)
                    _Items.Shuffle<mediaInfo>(CurrentIndex+1); //  System.Math.Min(CurrentIndex+1, _HistorySize));
            }
        }

        /// <summary>
        /// Changes active item to next media 
        /// </summary>
        public bool GotoNextMedia()
        {
            lock (this)
            {
                // Is history filled?
                if (QueueIndex < _HistorySize)
                {   // No just move queue index
                    QueueIndex++;
                }
                else
                {   // Yes, move items from history back to end of list (history is the first items in the list)
                    Items.Add(_Items[0]);
                    Items.RemoveAt(0);
                    Raise_OnListItemInserted(_Items.Count - 1);
                    Raise_OnListItemRemoved(0);
                }

            }
            this.OnPropertyChanged("CurrentItem");
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
                QueueIndex--;
            }
            return true;
        }

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
            GenerateQueue();
            Raise_OnListChanged();
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
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;
            return db.writePlaylist(playlist.ConvertAll<string>(convertMediaInfo), name, false);
        }

        /// <summary>
        /// Writes a playlist to the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool deletePlaylistFromDB(string name)
        {
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;
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
            return readPlaylistFromDB(name, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase"));
        }
        /// <summary>
        /// Reads the given playlist from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylistFromDB(string name, string dbName)
        {
            List<mediaInfo> playlist = new List<mediaInfo>();
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return playlist;

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


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Propertychanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propertychanged event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public delegate void MediaListItemChangedEventHandler(List<mediaInfo> list, int index);
        public event MediaListItemChangedEventHandler OnListItemInserted;
        public event MediaListItemChangedEventHandler OnListItemRemoved;
        public event MediaListItemChangedEventHandler OnListCleared;
        public event MediaListItemChangedEventHandler OnListChanged;

        private void Raise_OnListItemInserted(int index)
        {
            if (OnListItemInserted != null)
                OnListItemInserted(_Items, index);
        }
        private void Raise_OnListItemRemoved(int index)
        {
            if (OnListItemRemoved != null)
                OnListItemRemoved(_Items, index);
        }
        private void Raise_OnListCleared()
        {
            if (OnListCleared != null)
                OnListCleared(_Items, 0);
        }
        private void Raise_OnListChanged()
        {
            if (OnListChanged != null)
                OnListChanged(_Items, 0);
        }

        public void ReadMediaTags()
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                //if (abortJob[screen])
                //    return;
                //mediaInfo tmp = _Items[i];
                if (_Items[i].Name == null)
                    _Items[i] = OpenMobile.Media.TagReader.getInfo(_Items[i].Location);
                if (_Items[i] == null)
                    continue;
                if (_Items[i].coverArt == null)
                    _Items[i].coverArt = TagReader.getCoverFromDB(_Items[i].Artist, _Items[i].Album);
                if (_Items[i].coverArt == null)
                    _Items[i].coverArt = MediaLoader.MissingCoverImage;
            }
        }


    }

*/
}

