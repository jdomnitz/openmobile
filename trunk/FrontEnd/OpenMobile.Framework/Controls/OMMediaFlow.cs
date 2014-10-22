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
using System.Linq;
using System.Collections;

namespace OpenMobile.Controls
{
    public class OMMediaFlow : OMImageFlow
    {
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

        private PropertyChangedEventHandler _PropertyChangedEventHandler;

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
        /// The source to use for the list visualization
        /// </summary>
        public ListSources MediaListSource
        {
            get
            {
                return this._MediaListSource;
            }
            set
            {
                if (this._MediaListSource != value)
                {
                    this._MediaListSource = value;
                }
            }
        }
        private ListSources _MediaListSource = ListSources.Items;

        /// <summary>
        /// Set or get the playlist source
        /// </summary>
        public override object ListSource
        {
            get
            {
                return _ListSource;
            }
            set
            {
                if (!(value is Playlist))
                    throw new Exception("Invalid list source type, must be Playlist3");

                if (_ListSource != null)
                    _ListSource.PropertyChanged -= _PropertyChangedEventHandler;

                _ListSource = value as Playlist;
                _ListSource.PropertyChanged += _PropertyChangedEventHandler;

                switch (_MediaListSource)
                {
                    case ListSources.Items:
                        base.ListSource = _ListSource.Items;
                        break;
                    case ListSources.Buffer:
                        base.ListSource = _ListSource.BufferItems;
                        break;
                }
                SelectedIndex = (_MediaListSource == ListSources.Buffer ? _ListSource.BufferItems_CurrentItemIndex : _ListSource.Items_CurrentItemIndex);
                this.Text = ExtractLabelText();
               
            }
        }
        private Playlist _ListSource;

        void _ListSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentItem":
                    {
                        if (_ListSource != null)
                        {
                            switch (MediaListSource)
                            {
                                case ListSources.Items:
                                    base.SelectedIndex = _ListSource.Items_CurrentItemIndex;
                                    break;
                                case ListSources.Buffer:
                                    var index = _ListSource.BufferItems_CurrentItemIndex;
                                    if (index >= 0)
                                        base.SelectedIndex = index;
                                    break;
                                default:
                                    break;
                            }
                        }

                        //if (_MediaListSource == ListSources.Items)
                        //    this.SelectedIndex = _ListSource.Items_CurrentItemIndex;

                        this.Text = ExtractLabelText();
                    }
                    break;
                default:
                    break;
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
        private string _MediaInfoFormatString = "{1} - {0}\n{6}";

        /// <summary>
        /// Creates a new media flow control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public OMMediaFlow(string name, int left, int top, int width, int height)
            : base(name, left, top, width, height)
        {
            _PropertyChangedEventHandler = new PropertyChangedEventHandler(_ListSource_PropertyChanged);

            base.AssignListItemAction = item =>
            {
                if (item is mediaInfo)
                {
                    var mediaItem = item as mediaInfo;
                    return mediaItem.coverArt;
                }
                if (item is OImage)
                    return (OImage)item;

                return null;
            };
            base.Animation_InsertionOfNewItems = false;
        }

        internal override string ExtractLabelText()
        {
            lock (base._Images)
            {
                try
                {
                    if (_MediaInfoFormatString != null && base.ListSource != null)
                    {
                        lock (base.ListSource)
                        {
                            var mediaInfo = ((System.Collections.IList)base.ListSource)[base.SelectedIndex] as mediaInfo;

                            return String.Format(_MediaInfoFormatString,
                                mediaInfo.Album,
                                mediaInfo.Artist,
                                mediaInfo.Genre,
                                mediaInfo.Length,
                                mediaInfo.Location,
                                mediaInfo.Lyrics,
                                mediaInfo.Name,
                                mediaInfo.Rating,
                                mediaInfo.TrackNumber,
                                mediaInfo.Type);
                        }
                    }
                    else
                        return String.Empty;
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        //internal override void ListContentChanged(ListChangedEventArgs e)
        //{
        //    if (_ListSource != null)
        //    {
        //        switch (MediaListSource)
        //        {
        //            case ListSources.Items:
        //                base.SelectedIndex = _ListSource.Items_CurrentItemIndex;
        //                break;
        //            case ListSources.Buffer:
        //                var index = _ListSource.BufferItems_CurrentItemIndex;
        //                if (index >= 0)
        //                    base.SelectedIndex = index;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    base.ListContentChanged(e);
        //}

        public override object Clone(OMPanel parent)
        {
            var newObject = (OMMediaFlow)base.Clone(parent);
            newObject._PropertyChangedEventHandler = new PropertyChangedEventHandler(newObject._ListSource_PropertyChanged);
            if (newObject._ListSource != null)
            {
                newObject._ListSource.PropertyChanged += newObject._PropertyChangedEventHandler;
            }
            return newObject;
        }

    }
}
