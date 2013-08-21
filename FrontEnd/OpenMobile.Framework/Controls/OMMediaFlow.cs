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
using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile;
using System.Collections.Generic;
using OpenMobile.Media;

namespace OpenMobile.Controls
{
    /// <summary>
    /// An "coverflow" style control for images
    /// </summary>
    public class OMMediaFlow: OMImageFlow
    {
        #region Constructor

        /// <summary>
        /// Creates a new media flow control
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Left"></param>
        /// <param name="Top"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public OMMediaFlow(string name, int left, int top, int width, int height)
            : base(name, left, top, width, height)
        {
        }

        #endregion

        /// <summary>
        /// The image to use if media image is missing
        /// </summary>
        public OImage NoMediaImage
        {
            get
            {
                if (this._NoMediaImage == null)
                    this._NoMediaImage = MediaLoader.MissingCoverImage;
                return this._NoMediaImage;
            }
            set
            {
                if (this._NoMediaImage != value)
                {
                    this._NoMediaImage = value;
                }
            }
        }
        private OImage _NoMediaImage;

        /// <summary>
        /// List sources
        /// </summary>
        public enum ListSources
        {
            /// <summary>
            /// All items in the playlist
            /// </summary>
            Items,

            /// <summary>
            /// The active buffer in the playlist (history + currentItem + queue)
            /// </summary>
            Buffer
        }

        /// <summary>
        /// The source to use for the list visualization
        /// </summary>
        public ListSources ListSource
        {
            get
            {
                return this._ListSource;
            }
            set
            {
                if (this._ListSource != value)
                {
                    this._ListSource = value;
                }
            }
        }
        private ListSources _ListSource = ListSources.Items;        

        /// <summary>
        /// Gets or sets the databounded playlist 
        /// </summary>
        public PlayList2 PlayListSource
        {
            get
            {
                return this._PlayListSource;
            }
            set
            {
                if (this._PlayListSource != value)
                {
                    // Disconnect current event
                    if (this._PlayListSource != null)
                    {
                        this._PlayListSource.PropertyChanged -= new PropertyChangedEventHandler(_PlayListSource_PropertyChanged);
                        this._PlayListSource.OnList_Items_Changed -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListChanged);
                        this._PlayListSource.OnList_Items_Cleared -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListCleared);
                        this._PlayListSource.OnList_Items_ItemInserted -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItemInserted);
                        this._PlayListSource.OnList_Items_ItemRemoved -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItemRemoved);
                        this._PlayListSource.OnList_Buffer_Changed -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnList_Buffer_Changed);
                        this._PlayListSource.OnList_Buffer_Cleared -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnList_Buffer_Cleared);
                        this._PlayListSource.OnList_Buffer_ItemInserted -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItem_Buffer_Inserted);
                        this._PlayListSource.OnList_Buffer_ItemRemoved -= new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItem_Buffer_Removed);
                    }

                    // Map source
                    this._PlayListSource = value;

                    // Connect to new event
                    this._PlayListSource.PropertyChanged += new PropertyChangedEventHandler(_PlayListSource_PropertyChanged);
                    this._PlayListSource.OnList_Items_Changed +=new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListChanged);
                    this._PlayListSource.OnList_Items_Cleared += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListCleared);
                    this._PlayListSource.OnList_Items_ItemInserted += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItemInserted);
                    this._PlayListSource.OnList_Items_ItemRemoved += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItemRemoved);
                    this._PlayListSource.OnList_Buffer_Changed += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnList_Buffer_Changed);
                    this._PlayListSource.OnList_Buffer_Cleared += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnList_Buffer_Cleared);
                    this._PlayListSource.OnList_Buffer_ItemInserted += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItem_Buffer_Inserted);
                    this._PlayListSource.OnList_Buffer_ItemRemoved += new PlayList2.MediaListItemChangedEventHandler(_PlayListSource_OnListItem_Buffer_Removed);

                    // Update covers and other media info for the playlist
                    //this._PlayListSource.ReadMediaTags();
                    LoadImagesFromPlayList();
                    SelectedIndex = (_ListSource == ListSources.Buffer ? _PlayListSource.BufferIndex : _PlayListSource.CurrentIndex);
                    this.Text = ExtractLabelText();
                }
            }
        }
        private PlayList2 _PlayListSource;

        void _PlayListSource_OnListItem_Buffer_Removed(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Buffer)
                return;

            this.RemoveAt(index);
            SelectedIndex = _PlayListSource.BufferIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnListItem_Buffer_Inserted(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Buffer)
                return;

            this.Insert(GetCoverImage(list[index].coverArt), index);
            SelectedIndex = _PlayListSource.BufferIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnList_Buffer_Cleared(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Buffer)
                return;

            this.Clear();
            SelectedIndex = _PlayListSource.BufferIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnList_Buffer_Changed(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Buffer)
                return;

            LoadImagesFromPlayList();
            SelectedIndex = _PlayListSource.BufferIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnListItemRemoved(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Items)
                return;

            this.RemoveAt(index);
            SelectedIndex = _PlayListSource.CurrentIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnListItemInserted(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Items)
                return;

            this.Insert(GetCoverImage(list[index].coverArt), index);
            SelectedIndex = _PlayListSource.CurrentIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnListCleared(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Items)
                return;

            this.Clear();
            SelectedIndex = _PlayListSource.CurrentIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_OnListChanged(List<mediaInfo> list, int index)
        {
            if (_ListSource != ListSources.Items)
                return;

            LoadImagesFromPlayList();
            SelectedIndex = _PlayListSource.CurrentIndex;
            this.Text = ExtractLabelText();
        }

        void _PlayListSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_ListSource != ListSources.Items)
                return;

            switch (e.PropertyName)
            {
                case "Items":
                case "CurrentItem":
                case "Random":
                    {
                        LoadImagesFromPlayList();
                        SelectedIndex = _PlayListSource.CurrentIndex;
                    }
                    break;
                default:
                    break;
            }
        }

        private void LoadImagesFromPlayList()
        {
            switch (_ListSource)
            {
                case ListSources.Items:
                    {   // Show all items in the playlist
                        lock (_Images)
                        {
                            mediaInfo[] playListItems = _PlayListSource.Items.ToArray();

                            // Resize images list to match playlist length
                            if (_Images.Count > playListItems.Length)
                                _Images.RemoveRange(playListItems.Length, _Images.Count - playListItems.Length);

                            for (int i = 0; i < playListItems.Length; i++)
                            {
                                if (playListItems[i] != null)
                                {
                                    if (_Images.Count <= i)
                                        Insert(playListItems[i].coverArt == null ? NoMediaImage : playListItems[i].coverArt);
                                    else
                                    {
                                        if (_Images[i].Image != playListItems[i].coverArt)
                                        {
                                            if (playListItems[i].coverArt == null)
                                            {
                                                _Images[i].Image = NoMediaImage;
                                            }
                                            else
                                            {
                                                _Images[i].Image = playListItems[i].coverArt;
                                            }
                                        }
                                    }
                                }
                            }
                            SelectedIndex = _PlayListSource.CurrentIndex;
                        }
                    }
                    break;
                case ListSources.Buffer:
                     {   // Show the buffer list
                        lock (_Images)
                        {
                            mediaInfo[] playListItems = _PlayListSource.Buffer.ToArray();

                            // Resize images list to match playlist length
                            if (_Images.Count > playListItems.Length)
                                _Images.RemoveRange(playListItems.Length, _Images.Count - playListItems.Length);

                            for (int i = 0; i < playListItems.Length; i++)
                            {
                                if (playListItems[i] != null)
                                {
                                    if (_Images.Count <= i)
                                        Insert(playListItems[i].coverArt == null ? NoMediaImage : playListItems[i].coverArt);
                                    else
                                    {
                                        if (_Images[i].Image != playListItems[i].coverArt)
                                        {
                                            if (playListItems[i].coverArt == null)
                                            {
                                                _Images[i].Image = NoMediaImage;
                                            }
                                            else
                                            {
                                                _Images[i].Image = playListItems[i].coverArt;
                                            }
                                        }
                                    }
                                }
                            }
                            SelectedIndex = _PlayListSource.BufferIndex;
                        }
                    }
                   break;
                default:
                    break;
            }
        }

        private OImage GetCoverImage(OImage image)
        {
            if (image == null)
                return NoMediaImage;
            else
                return image;
        }

        internal override string ExtractLabelText()
        {
            lock (this._Images)
            {
                try
                {
                    switch (_ListSource)
                    {
                        case ListSources.Items:
                            {
                                if (_MediaInfoFormatString != null && _PlayListSource != null && _PlayListSource.Items != null)
                                {
                                    return String.Format(_MediaInfoFormatString,
                                        _PlayListSource.Items[this.SelectedIndex].Album,
                                        _PlayListSource.Items[this.SelectedIndex].Artist,
                                        _PlayListSource.Items[this.SelectedIndex].Genre,
                                        _PlayListSource.Items[this.SelectedIndex].Length,
                                        _PlayListSource.Items[this.SelectedIndex].Location,
                                        _PlayListSource.Items[this.SelectedIndex].Lyrics,
                                        _PlayListSource.Items[this.SelectedIndex].Name,
                                        _PlayListSource.Items[this.SelectedIndex].Rating,
                                        _PlayListSource.Items[this.SelectedIndex].TrackNumber,
                                        _PlayListSource.Items[this.SelectedIndex].Type);
                                }
                                else
                                    return String.Empty;
                            }
                        case ListSources.Buffer:
                            {
                                if (_MediaInfoFormatString != null && _PlayListSource != null && _PlayListSource.Buffer != null)
                                {
                                    return String.Format(_MediaInfoFormatString,
                                        _PlayListSource.Buffer[this.SelectedIndex].Album,
                                        _PlayListSource.Buffer[this.SelectedIndex].Artist,
                                        _PlayListSource.Buffer[this.SelectedIndex].Genre,
                                        _PlayListSource.Buffer[this.SelectedIndex].Length,
                                        _PlayListSource.Buffer[this.SelectedIndex].Location,
                                        _PlayListSource.Buffer[this.SelectedIndex].Lyrics,
                                        _PlayListSource.Buffer[this.SelectedIndex].Name,
                                        _PlayListSource.Buffer[this.SelectedIndex].Rating,
                                        _PlayListSource.Buffer[this.SelectedIndex].TrackNumber,
                                        _PlayListSource.Buffer[this.SelectedIndex].Type);
                                }
                                else
                                    return String.Empty;
                            }
                        default:
                            return String.Empty;
                    }
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// The format string for media info
        /// <para>Available strings:</para>
        /// <para>{0} : Album</para>
        /// <para>{1} : Artist</para>
        /// <para>{2} : Genre</para>
        /// <para>{3} : Length</para>
        /// <para>{4} : Location</para>
        /// <para>{5} : Lyrics</para>
        /// <para>{6} : Name</para>
        /// <para>{7} : Rating</para>
        /// <para>{8} : TrackNumber</para>
        /// <para>{9} : Type</para>
        /// </summary>
        public string MediaInfoFormatString
        {
            get
            {
                return this._MediaInfoFormatString;
            }
            set
            {
                if (this._MediaInfoFormatString != value)
                {
                    this._MediaInfoFormatString = value;
                }
            }
        }
        private string _MediaInfoFormatString;
    }
}
