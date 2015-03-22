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
using System.Linq;

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
    /// A playlist class
    /// </summary>
    public class Playlist : INotifyPropertyChanged
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

        /// <summary>
        /// Set's the display name for this playlist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        static public void SetDisplayName(ref string name, string displayName)
        {
            string currentDisplayName = GetDisplayName(name);

            if (name.Contains("|"))
                name.Replace(String.Format("|{0}", currentDisplayName), String.Format("|{0}", displayName));
            else
                name += String.Format("|{0}", displayName);
        }

        private enum QueueModes
        {
            Normal,
            Shuffle
        }

        private ListChangedEventHandler _ListChangedEventHandler;
        private QueueModes _QueueMode = QueueModes.Normal;
        private bool _BlockListUpdateEvents = false;

        /// <summary>
        /// The display name of this list
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this._DisplayName;
            }
            set
            {
                if (this._DisplayName != value)
                {
                    this._DisplayName = value;
                }
            }
        }
        private string _DisplayName;

        /// <summary>
        /// The name of the list
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
                }
            }
        }
        private string _Name;

        /// <summary>
        /// The items in the buffers. Index 0 to BufferSize is history, index at BufferSize is current item, index BufferSize+1 to (BufferSize * 2 + 1) is queue
        /// </summary>
        public BindingList<mediaInfo> BufferItems
        {
            get
            {
                return this._BufferItems;
            }
            set
            {
                if (this._BufferItems != value)
                {
                    this._BufferItems = value;
                }
            }
        }
        private BindingList<mediaInfo> _BufferItems;

        /// <summary>
        /// Is shuffle (random) mode active
        /// </summary>
        public bool Shuffle
        {
            get
            {
                return _QueueMode == QueueModes.Shuffle;
            }
            set
            {
                if (value)
                    _QueueMode = QueueModes.Shuffle;
                else
                    _QueueMode = QueueModes.Normal;
                
                GenerateQueue(true, true);
            }
        }
        /// <summary>
        /// Is shuffle (random) mode active
        /// </summary>
        public bool Random
        {
            get
            {
                return _QueueMode == QueueModes.Shuffle;
            }
            set
            {
                if (value)
                    _QueueMode = QueueModes.Shuffle;
                else
                    _QueueMode = QueueModes.Normal;

                GenerateQueue(true, true);
            }
        }

        /// <summary>
        /// The index of the current item
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return Items_CurrentItemIndex;
            }
            set
            {
                if (value >= 0 && value < _Items.Count)
                    CurrentItem = _Items[value];
            }
        }

        /// <summary>
        /// The currently active item
        /// </summary>
        public mediaInfo CurrentItem
        {
            get
            {
                if (_Items.Count == 0 || BufferItems_CurrentItemIndex < 0)
                    return new mediaInfo();

                return _BufferItems[BufferItems_CurrentItemIndex];
            }
            set
            {
                _BufferItems[BufferItems_CurrentItemIndex] = value;
                RegenerateQueue();
                this.OnPropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// The amount of items to keep in a queue
        /// </summary>
        public int BufferSize
        {
            get
            {
                return this._BufferSize;
            }
            set
            {
                if (this._BufferSize != value)
                {
                    this._BufferSize = value;
                }
            }
        }
        private int _BufferSize = 20;

        /// <summary>
        /// The repeat mode
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
        /// The amount of items
        /// </summary>
        public int Count
        {
            get
            {
                if (_Items == null)
                    return 0;
                return _Items.Count;
            }
        }        

        #region Constructor

        /// <summary>
        /// Creates a new playlist
        /// </summary>
        public Playlist()
        {
            _ListChangedEventHandler = new ListChangedEventHandler(_Items_ListChanged);
            SetItemsList(new BindingList<mediaInfo>());
            _BufferItems = new BindingList<mediaInfo>();
        }

        /// <summary>
        /// Creates a new playlist
        /// </summary>
        /// <param name="name"></param>
        public Playlist(string name)
            : this()
        {
            _Name = name;
        }

        #endregion        

        #region Playlist item control

        /// <summary>
        /// Adds an item to the playlist
        /// </summary>
        /// <param name="item"></param>
        public void Add(mediaInfo item)
        {
            if (item == null)
                return;

            //_BlockListUpdateEvents = true;
            //_Items.Add(item);
            //BufferItems_IncrementCurrentItemIndex();
            //_BlockListUpdateEvents = false;
            //GenerateQueue(true, true);

            _BlockListUpdateEvents = true;
            _Items.Add(item);

            if (_BufferItems_CurrentItemIndex < 0)
            {
                _BufferItems.Add(_Items[_Items.Count - 1]);
                _BufferItems_CurrentItemIndex++;
            }
            _BlockListUpdateEvents = false;
            RegenerateQueue();
            this.OnPropertyChanged("CurrentItem");

        }

        /// <summary>
        /// Adds a range of items to the playlist
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<mediaInfo> items)
        {
            _BlockListUpdateEvents = true;
            foreach (var item in items)
                _Items.Add(item);

            if (_BufferItems_CurrentItemIndex < 0)
            {
                _BufferItems.Add(_Items[_Items.Count - 1]);
                _BufferItems_CurrentItemIndex++;
            }
            _BlockListUpdateEvents = false;
            RegenerateQueue();
            this.OnPropertyChanged("CurrentItem");
        }

        /// <summary>
        /// Adds an item to the playlist. If an item is already contained in the playlist it's skipped
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddDistinct(mediaInfo item)
        {
            if (item == null)
                return false;

            if (!_Items.Any(x => x.IsContentEqual(item)))
            {
                Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an item to the playlist. If an item has the same name as one already in the playlist it's skipped.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddDistinctName(mediaInfo item)
        {
            if (item == null)
                return false;

            if (!_Items.Any(x => x.Name == item.Name))
            {
                Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an item to the playlist. Uses a function to evaluate whether it should add the item. Item is NOT added if function returns true.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool AddDistinct(mediaInfo item, Func<mediaInfo, bool> predicate)
        {
            if (item == null)
                return false;

            if (!_Items.Any(predicate))
            {
                Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a range of items. If an item is already contained in the playlist it's skipped
        /// </summary>
        /// <param name="items"></param>
        public void AddRangeDistinct(IEnumerable<mediaInfo> items)
        {
            AddRange(items.Where(x => !_Items.Any(y => y == x)));
        }

        /// <summary>
        /// Adds a range of items. If an item has the same name as one already in the playlist it's skipped.
        /// </summary>
        /// <param name="items"></param>
        public void AddRangeDistinctName(IEnumerable<mediaInfo> items)
        {
            AddRange(items.Where(x => !_Items.Any(y => y.Name == x.Name)));
        }

        /// <summary>
        /// Adds a range of items. Uses a function to evaluate whether it should add the item. Item is NOT added if function returns true.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        public void AddRangeDistinct(IEnumerable<mediaInfo> items, Func<mediaInfo, bool> predicate)
        {
            AddRange(items.Where(x => !_Items.Any(predicate)));
        }

        /// <summary>
        /// Removes an item from this playlist
        /// </summary>
        /// <param name="item"></param>
        public void Remove(mediaInfo item)
        {
            if (!_Items.Remove(item))
            {
                var itemToRemove = _Items.Where(x => x.IsContentEqual(item)).FirstOrDefault();
                if (itemToRemove != null)
                    _Items.Remove(itemToRemove);                
            }

            if (_Items.Count < BufferItems_QueueMaxCount)
                BufferItems_DecrementCurrentItemIndex();
        }

        /// <summary>
        /// Removes the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _Items.RemoveAt(index);

            if (_Items.Count < BufferItems_QueueMaxCount)
                BufferItems_DecrementCurrentItemIndex();
        }

        /// <summary>
        /// Removes all items from this playlist
        /// </summary>
        public void Clear()
        {
            _Items.Clear();
            _BufferItems.Clear();
            _BufferItems_CurrentItemIndex = -1;
        }

        /// <summary>
        /// The playlist items
        /// </summary>
        public BindingList<mediaInfo> Items
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
                }
            }
        }
        private BindingList<mediaInfo> _Items = new BindingList<mediaInfo>();
        
        private void SetItemsList(BindingList<mediaInfo> list)
        {
            if (_Items != null)
                _Items.ListChanged -= _ListChangedEventHandler;

            _Items = list;
            _Items.ListChanged += _ListChangedEventHandler;
        }

        void _Items_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (_BlockListUpdateEvents)
                return;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        GenerateQueue(true, false);
                    }
                    break;
                case ListChangedType.ItemChanged:
                    {
                        GenerateQueue(true, false);
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        GenerateQueue(true, false);
                    }
                    break;
                case ListChangedType.ItemMoved:
                    {
                        GenerateQueue(true, false);
                    }
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                    break;
                case ListChangedType.PropertyDescriptorChanged:
                    break;
                case ListChangedType.PropertyDescriptorDeleted:
                    break;
                case ListChangedType.Reset:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Amount of items in the buffer items queue
        /// </summary>
        public int BufferItems_QueueCount
        {
            get
            {
                return (BufferItems_CurrentItemIndex - BufferItems_QueueStartIndex);
            }
        }

        /// <summary>
        /// The start index of the buffer items queue
        /// </summary>
        public int BufferItems_QueueStartIndex
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// The end index of the buffer items queue
        /// </summary>
        public int BufferItems_QueueEndIndex
        {
            get
            {
                return BufferItems_CurrentItemIndex - 1;
            }
        }

        /// <summary>
        /// The maximum amount of items that can be placed in the buffer queue
        /// </summary>
        public int BufferItems_QueueMaxCount
        {
            get
            {
                return _BufferSize;
            }
        }

        /// <summary>
        /// The index of the currently active item in the buffer
        /// </summary>
        public int BufferItems_CurrentItemIndex
        {
            get
            {
                return this._BufferItems_CurrentItemIndex;
            }
            set
            {
                if (this._BufferItems_CurrentItemIndex != value)
                {
                    this._BufferItems_CurrentItemIndex = value;
                }
            }
        }
        private int _BufferItems_CurrentItemIndex = -1;

        /// <summary>
        /// Amount of items in the history buffer
        /// </summary>
        public int BufferItems_HistoryCount
        {
            get
            {
                return (BufferItems_HistoryEndIndex - BufferItems_HistoryStartIndex) + 1;
            }
        }

        /// <summary>
        /// The start index of the history items queue
        /// </summary>
        public int BufferItems_HistoryStartIndex
        {
            get
            {
                return BufferItems_CurrentItemIndex + 1;
            }
        }

        /// <summary>
        /// The end index of the history items queue
        /// </summary>
        public int BufferItems_HistoryEndIndex
        {
            get
            {
                return _BufferItems.Count - 1;
            }
        }

        /// <summary>
        /// The maximum amount of items that can be placed in the history queue
        /// </summary>
        public int BufferItems_MaxCount
        {
            get
            {
                return _BufferSize + 1 + _BufferSize;
            }
        }

        /// <summary>
        /// The index of the currently selected item in the playlist
        /// </summary>
        public int Items_CurrentItemIndex
        {
            get
            {
                try
                {
                    return _Items.IndexOf(_BufferItems[BufferItems_CurrentItemIndex]);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private void BufferItems_IncrementCurrentItemIndex()
        {
            if (BufferItems_QueueCount < BufferItems_QueueMaxCount)
                _BufferItems_CurrentItemIndex++;
            this.OnPropertyChanged("CurrentItem");
        }

        private void BufferItems_DecrementCurrentItemIndex()
        {
            if (_BufferItems_CurrentItemIndex >= 0)
                _BufferItems_CurrentItemIndex--;
            this.OnPropertyChanged("CurrentItem");
        }

        private mediaInfo GetNextQueueItem(bool listContentChanged)
        {
            // Next item to add to queue
            if (_BufferItems.Count == 0)
                return _Items[0];

            var indexOfNextItem = 0;

            switch (_QueueMode)
            {
                case QueueModes.Normal:
                    {
                        //if (BufferItems_CurrentItemIndex >= 0 && BufferItems_QueueCount < BufferItems_QueueMaxCount)
                        //    indexOfNextItem = _Items.IndexOf(_BufferItems[BufferItems_CurrentItemIndex]);
                        //else
                            indexOfNextItem = _Items.IndexOf(_BufferItems[0]);
                        indexOfNextItem++;
                        if (indexOfNextItem >= _Items.Count)
                            indexOfNextItem = 0;

                    }
                    break;
                case QueueModes.Shuffle:
                    {
                        Random rnd = new Random();

                        var unusedItems = _Items.Where(x => !_BufferItems.Any(y => y == x));
                        if (!unusedItems.Any())
                            unusedItems = _BufferItems.Select(x=>x);

                        // Return random item
                        var unusedItemsIndex = rnd.Next(0, unusedItems.Count());
                        var nextItem = unusedItems.Skip(unusedItemsIndex).First();
                        return nextItem;
                    }
                default:
                    break;
            }

            return _Items[indexOfNextItem];
        }

        private void RegenerateQueue()
        {
            if (_Items.Count == 0)
                return;

            // Set queue size
            int itemsToAddToQueue = 0;
            if (BufferItems_QueueCount < BufferItems_QueueMaxCount)
            {
                if (BufferItems_QueueCount < 0)
                {
                    itemsToAddToQueue = _Items.Count; 
                }
                else
                {
                    if (BufferItems_QueueCount < _Items.Count)
                        itemsToAddToQueue = _Items.Count - BufferItems_QueueCount;
                }
            }
            if (itemsToAddToQueue > BufferItems_QueueMaxCount)
                itemsToAddToQueue = BufferItems_QueueMaxCount;

            // Fill missing items in queue
            _BlockListUpdateEvents = true;
            for (int i = 0; i < itemsToAddToQueue; i++)
            {
                _BufferItems.Insert(0, _Items[0]);
                _BufferItems_CurrentItemIndex++;
            }
            _BlockListUpdateEvents = false;

            switch (_QueueMode)
            {
                case QueueModes.Normal:
                    {
                        var currentItemIndex = Items_CurrentItemIndex;

                        // Regenerate queue
                        int loopCount = 0;                        
                        for (int i = BufferItems_QueueEndIndex; i >= BufferItems_QueueStartIndex; i--)
                        {
                            var indexToAdd = currentItemIndex + 1 + loopCount;
                            if (indexToAdd > _Items.Count - 1)
                            {
                                loopCount = 0;
                                indexToAdd = 0 + loopCount;
                            }

                            _BufferItems[i] = _Items[indexToAdd];

                            loopCount++;
                        }
                    }
                    break;
                case QueueModes.Shuffle:
                     {
                        var currentItemIndex = Items_CurrentItemIndex;

                        // Regenerate queue
                        int loopCount = 0;
                        Random rnd = new Random();
                        for (int i = BufferItems_QueueEndIndex; i >= BufferItems_QueueStartIndex; i--)
                        {
                            var indexToAdd = rnd.Next(0, _Items.Count - 1);
                            _BufferItems[i] = _Items[indexToAdd];
                            loopCount++;
                        }
                    }
                   break;
                default:
                    break;
            }

        }

        private void GenerateQueue(bool listContentChanged, bool forceQueueRegeneration)
        {
            if (_Items.Count == 0)
                return;

            if (_QueueMode == QueueModes.Shuffle)
            {
                // Shuffle existing items
                if (forceQueueRegeneration || (listContentChanged && _Items.Count <= (BufferItems_QueueMaxCount * 2)))
                {
                    try
                    {
                        var queueCount = BufferItems_QueueCount;

                        // Remove current queue items
                        for (int i = 0; i < queueCount; i++)
                            _BufferItems.RemoveAt(BufferItems_QueueStartIndex);

                        // Get items not already in buffer
                        Random rnd = new Random();
                        var unusedItems = _Items.Where(x => !_BufferItems.Any(y => y == x));
                        if (!unusedItems.Any())
                            unusedItems = _BufferItems.Select(x => x);

                        // Refill list with random items
                        for (int i = 0; i < queueCount; i++)
                        {
                            var unusedItemsIndex = rnd.Next(0, unusedItems.Count());
                            var nextItem = unusedItems.Skip(unusedItemsIndex).First();
                            _BufferItems.Insert(BufferItems_QueueStartIndex, nextItem);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                if (forceQueueRegeneration)
                {
                    try
                    {
                        var queueCount = BufferItems_QueueCount;

                        // Remove current queue items
                        for (int i = 0; i < queueCount; i++)
                            _BufferItems.RemoveAt(BufferItems_QueueStartIndex);

                        // Get items not already in buffer
                        Random rnd = new Random();
                        var unusedItems = _Items.Where(x => !_BufferItems.Any(y => y == x));
                        if (!unusedItems.Any())
                            unusedItems = _BufferItems.Select(x => x);

                        // Refill list with random items
                        for (int i = 0; i < queueCount; i++)
                        {
                            var nextItem = unusedItems.Skip(i).First();
                            _BufferItems.Insert(BufferItems_QueueStartIndex, nextItem);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            // Do we need to fill the queue
            if (BufferItems_QueueCount < _BufferSize)
                BufferItems_InsertInQueue(GetNextQueueItem(listContentChanged));
        }

        private void BufferItems_InsertInQueue(mediaInfo item)
        {
            _BufferItems.Insert(0, item);

            // Limit list size
            while (_BufferItems.Count > BufferItems_MaxCount)
                _BufferItems.RemoveAt(_BufferItems.Count - 1);
        }

        /// <summary>
        /// Shifts to the next media index in the playlist
        /// </summary>
        public void GotoNextMedia()
        {
            // Push a new item to the queue to move to the next index
            //BufferItems_IncrementCurrentItemIndex();
            BufferItems_InsertInQueue(GetNextQueueItem(false));
            this.OnPropertyChanged("CurrentItem");
        }

        /// <summary>
        /// Shifts to the previous media index in the playlist (first history items)
        /// </summary>
        public void GotoPreviousMedia()
        {
            // Remove the oldest item in queue to go back in history
            //BufferItems_DecrementCurrentItemIndex();
            if (BufferItems_HistoryCount > 0)
                _BufferItems.RemoveAt(0);
            this.OnPropertyChanged("CurrentItem");
        }

        /// <summary>
        /// Sets the index of the next item to play
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool SetNextMedia(int index)
        {
            if (index < 0 || index >= _Items.Count)
                return false;

            if (BufferItems_CurrentItemIndex >= 1)
            {
                _BufferItems[BufferItems_CurrentItemIndex - 1] = _Items[index];
                return true;
            }
            return false;
        }

        #endregion

        #region Playlist handling

        /// <summary>
        /// Saves this playlist to the DB
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {   // Save current playlist order to the DB

            // Convert current queue to list of indexes to save to the db
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _BufferItems.Count; i++)
            {
                if (i == BufferItems_CurrentItemIndex)
                    sb.Append("~");
                sb.Append(_Items.IndexOf(_BufferItems[i]));
                sb.Append("|");
            }
            writePlayListSettingToDB(_Name, "Queue", sb.ToString());

            // Save settings to database
            writePlayListSettingToDB(_Name, "Random", Shuffle.ToString());

            // Save actual items to the database
            return writePlaylistToDB(_Name, _Items.ToList());
        }

        /// <summary>
        /// Loads this playlist from the DB
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            try
            {
                SetItemsList(new BindingList<mediaInfo>(readPlaylistFromDB(_Name)));

                Shuffle = false;

                try
                {
                    // Load queue from DB
                    string gueueString = readPlayListSettingFromDB(_Name, "Queue");
                    if (gueueString != null)
                    {
                        string[] gueueStringSplit = gueueString.Split('|');
                        if (gueueStringSplit.Length > 0)
                        {
                            _BufferItems.Clear();
                            for (int i = 0; i < gueueStringSplit.Length; i++)
                            {
                                var sTmp = gueueStringSplit[i];
                                if (sTmp.Contains('~'))
                                {
                                    sTmp = sTmp.Replace("~", "");
                                    _BufferItems_CurrentItemIndex = i;
                                }

                                int index = 0;
                                if (int.TryParse(sTmp, out index))
                                {
                                    _BufferItems.Add(_Items[index]);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                // Load settings from database
                bool result = false;
                bool.TryParse(readPlayListSettingFromDB(_Name, "Random"), out result);
                if (result)
                    _QueueMode = QueueModes.Shuffle;
                else
                    _QueueMode = QueueModes.Normal;

                this.OnPropertyChanged("CurrentItem");
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
        public static bool writePlaylistToDB(string name, List<mediaInfo> playlist)
        {
            //// Do we have something to write?
            //if (playlist.Count == 0)
            //    return false;

            // Get database object
            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;

            return db.writePlaylist(playlist, name, false);
        }

        /// <summary>
        /// Writes the playlist to the database as an object by serialization
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static bool writePlaylistToDBasObject(string name, List<mediaInfo> playlist)
        {
            if (playlist.Count == 0)
                return false;

            IMediaDatabase db = BuiltInComponents.Host.getData(eGetData.GetMediaDatabase, helperFunctions.StoredData.Get(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase")) as IMediaDatabase;
            if (db == null)
                return false;

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
            return String.Format("{0}|{1}|{2}", info.Artist, info.Album, info.Name);
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

        /// <summary>
        /// Retrieves a playlist
        /// </summary>
        /// <param name="location">The playlist location</param>
        /// <returns></returns>
        public static List<mediaInfo> readPlaylist(string location)
        {
            string ext = location; 

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
}

