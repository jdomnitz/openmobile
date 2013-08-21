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
using System.Text;
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// State of media provider
    /// </summary>
    public class MediaProviderState
    {
        /// <summary>
        /// True = mediaprovider activated
        /// </summary>
        public bool Activated
        {
            get
            {
                return this._Activated;
            }
            set
            {
                if (this._Activated != value)
                {
                    this._Activated = value;
                }
            }
        }
        private bool _Activated;

        /// <summary>
        /// A message that can provide status messages back to the skin
        /// <para>Example: If this provider failed to connect to a com device then this string can contain the error message</para>
        /// </summary>
        public string InfoMessage
        {
            get
            {
                return this._InfoMessage;
            }
            set
            {
                if (this._InfoMessage != value)
                {
                    this._InfoMessage = value;
                }
            }
        }
        private string _InfoMessage;

        /// <summary>
        /// Creates a new MediaProviderState
        /// </summary>
        /// <param name="activated"></param>
        /// <param name="infoMessage"></param>
        public MediaProviderState(bool activated, string infoMessage)
        {
            this._Activated = activated;
            this._InfoMessage = infoMessage;
        }

    }

    /// <summary>
    /// Media type info
    /// </summary>
    public class MediaTypeInfo
    {
        /// <summary>
        /// Icon representing this media type
        /// </summary>
        public imageItem Icon
        {
            get
            {
                return this._Icon;
            }
            private set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                }
            }
        }
        private imageItem _Icon;

        /// <summary>
        /// The text representing this media type (Example: MP3, CD, DVD)
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            private set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                }
            }
        }
        private string _Text;

        /// <summary>
        /// Creates a new MediaTypeInfo
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        public MediaTypeInfo(imageItem icon, string text)
        {
            this._Icon = icon;
            this._Text = text;
        }
    }

    /// <summary>
    /// Zone specific media provider data
    /// </summary>
    public class MediaProviderInfo
    {
        /// <summary>
        /// The current zone this info belongs to
        /// </summary>
        public Zone Zone
        {
            get
            {
                return this._zone;
            }
            set
            {
                if (this._zone != value)
                {
                    this._zone = value;
                }
            }
        }
        private Zone _zone;

        /// <summary>
        /// The current media source
        /// </summary>
        public MediaSource MediaSource
        {
            get
            {
                return this._mediaSource;
            }
            set
            {
                if (this._mediaSource != value)
                {
                    this._mediaSource = value;
                }
            }
        }
        private MediaSource _mediaSource;

        /// <summary>
        /// The current media type
        /// </summary>
        public MediaTypeInfo MediaType
        {
            get
            {
                return this._MediaType;
            }
            set
            {
                if (this._MediaType != value)
                {
                    this._MediaType = value;
                }
            }
        }
        private MediaTypeInfo _MediaType;

        /// <summary>
        /// Text describing the current media (could be artist, song, album and so on)
        /// </summary>
        public string MediaText1
        {
            get
            {
                return this._MediaText1;
            }
            set
            {
                if (this._MediaText1 != value)
                {
                    this._MediaText1 = value;
                }
            }
        }
        private string _MediaText1;

        /// <summary>
        /// Text describing the current media (could be artist, song, album and so on)
        /// </summary>
        public string MediaText2
        {
            get
            {
                return this._MediaText2;
            }
            set
            {
                if (this._MediaText2 != value)
                {
                    this._MediaText2 = value;
                }
            }
        }
        private string _MediaText2;

        /// <summary>
        /// Current Playback data
        /// </summary>
        public MediaProvider_PlaybackData PlaybackData
        {
            get
            {
                return this._PlaybackData;
            }
            set
            {
                if (this._PlaybackData != value)
                {
                    this._PlaybackData = value;
                }
            }
        }
        private MediaProvider_PlaybackData _PlaybackData = new MediaProvider_PlaybackData();

        /// <summary>
        /// Current active media
        /// </summary>
        public mediaInfo MediaInfo
        {
            get
            {
                return this._MediaInfo;
            }
            set
            {
                if (this._MediaInfo != value)
                {
                    this._MediaInfo = value;
                }
            }
        }
        private mediaInfo _MediaInfo = new mediaInfo();
    }

}
