using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenMobile.Media
{
    /// <summary>
    /// Properties used to report media provider abilities back to the media handler
    /// </summary>
    public class MediaProviderAbilities
    {
        /// <summary>
        /// Return true if the current provider can play/stream media
        /// </summary>
        public bool CanPlay
        {
            get
            {
                return this._CanPlay;
            }
            set
            {
                if (this._CanPlay != value)
                {
                    this._CanPlay = value;
                }
            }
        }
        private bool _CanPlay = true;

        /// <summary>
        /// Return true if the current provider can stop playback/stream
        /// </summary>
        public bool CanStop
        {
            get
            {
                return this._CanStop;
            }
            set
            {
                if (this._CanStop != value)
                {
                    this._CanStop = value;
                }
            }
        }
        private bool _CanStop = true;

        /// <summary>
        /// Return true if the current provider can pause playback/stream
        /// </summary>
        public bool CanPause
        {
            get
            {
                return this._CanPause;
            }
            set
            {
                if (this._CanPause != value)
                {
                    this._CanPause = value;
                }
            }
        }
        private bool _CanPause = true;

        /// <summary>
        /// Return true if the current provider can seek forward/backwards in the playback/stream
        /// </summary>
        public bool CanSeek
        {
            get
            {
                return this._CanSeek;
            }
            set
            {
                if (this._CanSeek != value)
                {
                    this._CanSeek = value;
                }
            }
        }
        private bool _CanSeek = true;

        /// <summary>
        /// Return true if the current provider can shuffle playback order
        /// </summary>
        public bool CanShuffle
        {
            get
            {
                return this._CanShuffle;
            }
            set
            {
                if (this._CanShuffle != value)
                {
                    this._CanShuffle = value;
                }
            }
        }
        private bool _CanShuffle = true;

        /// <summary>
        /// Return true if the current provider can repeat playback order
        /// </summary>
        public bool CanRepeat
        {
            get
            {
                return this._CanRepeat;
            }
            set
            {
                if (this._CanRepeat != value)
                {
                    this._CanRepeat = value;
                }
            }
        }
        private bool _CanRepeat = true;

        /// <summary>
        /// Return true if the current provider can goto next item in playback order
        /// </summary>
        public bool CanGotoNext
        {
            get
            {
                return this._CanGotoNext;
            }
            set
            {
                if (this._CanGotoNext != value)
                {
                    this._CanGotoNext = value;
                }
            }
        }
        private bool _CanGotoNext = true;

        /// <summary>
        /// Return true if the current provider can goto previous item in playback order
        /// </summary>
        public bool CanGotoPrevious
        {
            get
            {
                return this._CanGotoPrevious;
            }
            set
            {
                if (this._CanGotoPrevious != value)
                {
                    this._CanGotoPrevious = value;
                }
            }
        }
        private bool _CanGotoPrevious = true;

        /// <summary>
        /// A string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Play:{0}, Stop:{1}, Pause:{2}, Seek:{3}, Shuffle:{4}, Repeat:{5}, Next:{6}, Previous:{7}", _CanPlay, _CanStop, _CanPause, _CanSeek, _CanShuffle, _CanRepeat, _CanGotoNext, _CanGotoPrevious);
        }
    }
}
