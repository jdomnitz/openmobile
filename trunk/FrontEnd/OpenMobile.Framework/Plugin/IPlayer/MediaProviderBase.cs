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
using System.Linq;
using System.Text;
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// A base class for a media provider
    /// </summary>
    public abstract class MediaProviderBase : BasePluginCode, IMediaProvider
    {
        /// <summary>
        /// Initializes a new media provider plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="pluginIcon"></param>
        /// <param name="pluginVersion"></param>
        /// <param name="pluginDescription"></param>
        /// <param name="authorName"></param>
        /// <param name="authorEmail"></param>
        public MediaProviderBase(string pluginName, imageItem pluginIcon, float pluginVersion, string pluginDescription, string authorName, string authorEmail)
            : base(pluginName, pluginIcon, pluginVersion, pluginDescription, authorName, authorEmail)
        {
        }

        /// <summary>
        /// Playback states
        /// </summary>
        public enum PlaybackState
        {
            /// <summary>
            /// Playback has stopped
            /// </summary>
            Stopped,

            /// <summary>
            /// Playback is currently active
            /// </summary>
            Playing,

            /// <summary>
            /// Playback is currently paused
            /// </summary>
            Paused,

            /// <summary>
            /// Provider is busy doing a task other than stopped, playing and paused
            /// </summary>
            Busy
        }

        #region DataSources

        /// <summary>
        /// Reports the playback state back to the media handler
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="playbackState"></param>
        public virtual void MediaProviderData_ReportState_Playback(Zone zone, PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Stopped:
                    _MediaProviderHandler.IsStoppedCallback(this, zone, true);
                    _MediaProviderHandler.IsPlayingCallback(this, zone, false);
                    _MediaProviderHandler.IsPausedCallback(this, zone, false);
                    _MediaProviderHandler.IsBusyCallback(this, zone, false);
                    break;
                case PlaybackState.Playing:
                    _MediaProviderHandler.IsStoppedCallback(this, zone, false);
                    _MediaProviderHandler.IsPlayingCallback(this, zone, true);
                    _MediaProviderHandler.IsPausedCallback(this, zone, false);
                    _MediaProviderHandler.IsBusyCallback(this, zone, false);
                    break;
                case PlaybackState.Paused:
                    _MediaProviderHandler.IsStoppedCallback(this, zone, false);
                    _MediaProviderHandler.IsPlayingCallback(this, zone, false);
                    _MediaProviderHandler.IsPausedCallback(this, zone, true);
                    _MediaProviderHandler.IsBusyCallback(this, zone, false);
                    break;
                case PlaybackState.Busy:
                    _MediaProviderHandler.IsStoppedCallback(this, zone, false);
                    _MediaProviderHandler.IsPlayingCallback(this, zone, false);
                    _MediaProviderHandler.IsPausedCallback(this, zone, false);
                    _MediaProviderHandler.IsBusyCallback(this, zone, true);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reports the current media source back to the media handler
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="mediaSource"></param>
        public virtual void MediaProviderData_ReportMediaSource(Zone zone, MediaSource mediaSource)
        {
            _MediaProviderHandler.SetMediaSourceCallback(this, zone, mediaSource);
        }

        /// <summary>
        /// Reports the media info back to the media handler
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="media"></param>
        /// <param name="playbackPos"></param>
        public virtual void MediaProviderData_ReportMediaInfo(Zone zone, mediaInfo media, TimeSpan playbackPos = new TimeSpan())
        {
            _MediaProviderHandler.SetMediaInfoCallback(this, zone, media, playbackPos);
        }

        /// <summary>
        /// Reports the media text back to the media handler
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        public virtual void MediaProviderData_ReportMediaText(Zone zone, string text1, string text2)
        {
            _MediaProviderHandler.SetMediaTextCallback(this, zone, text1, text2);
        }

        /// <summary>
        /// Reports a change of playlist back to the media handler
        /// </summary>
        /// <param name="zone"></param>
        public virtual void MediaProviderData_RefreshPlaylist(Zone zone)
        {
            _MediaProviderHandler.RefreshPlaylistCallback(this, zone);
        }

        /// <summary>
        /// Gets the current playlist 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract Playlist MediaProviderData_GetPlaylist(Zone zone);

        Playlist IMediaProvider.GetPlaylist(Zone zone)
        {
            return MediaProviderData_GetPlaylist(zone);
        }

        /// <summary>
        /// Set the current playlist 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract Playlist MediaProviderData_SetPlaylist(Zone zone, Playlist playlist);

        Playlist IMediaProvider.SetPlaylist(Zone zone, Playlist playlist)
        {
            return MediaProviderData_SetPlaylist(zone, playlist);
        }


        #endregion

        #region Commands

        IMediaProviderHandler IMediaProvider.MediaProviderHandler
        {
            get
            {
                return _MediaProviderHandler;
            }
            set
            {
                _MediaProviderHandler = value;
            }
        }
        private IMediaProviderHandler _MediaProviderHandler;

        /// <summary>
        /// Callback when this provider is activated
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Activate(Zone zone);

        string IMediaProvider.Activate(Zone zone)
        {
            return MediaProviderCommand_Activate(zone);
        }

        /// <summary>
        /// Callback when this provider is deactivated
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Deactivate(Zone zone);

        string IMediaProvider.Deactivate(Zone zone)
        {
            return MediaProviderCommand_Deactivate(zone);
        }

        /// <summary>
        /// Called when a play command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_Start(Zone zone, object[] param);

        string IMediaProvider.Playback_Start(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_Start(zone, param);
        }

        /// <summary>
        /// Called when a pause command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_Pause(Zone zone, object[] param);

        string IMediaProvider.Playback_Pause(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_Pause(zone, param);
        }

        /// <summary>
        /// Called when a stop command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_Stop(Zone zone, object[] param);

        string IMediaProvider.Playback_Stop(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_Stop(zone, param);
        }

        /// <summary>
        /// Called when a next command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_Next(Zone zone, object[] param);

        string IMediaProvider.Playback_Next(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_Next(zone, param);
        }

        /// <summary>
        /// Called when a previous command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_Previous(Zone zone, object[] param);

        string IMediaProvider.Playback_Previous(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_Previous(zone, param);
        }

        /// <summary>
        /// Called when a seekForward command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_SeekForward(Zone zone, object[] param);

        string IMediaProvider.Playback_SeekForward(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_SeekForward(zone, param);
        }

        /// <summary>
        /// Called when a seekBackward command is executed
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="param"></param>
        /// <returns>Errorcode / Errorstring if an error occurs</returns>
        public abstract string MediaProviderCommand_Playback_SeekBackward(Zone zone, object[] param);

        string IMediaProvider.Playback_SeekBackward(Zone zone, object[] param)
        {
            return MediaProviderCommand_Playback_SeekBackward(zone, param);
        }

        /// <summary>
        /// Sets the shuffle state
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract bool MediaProviderData_SetShuffle(Zone zone, bool state);
        bool IMediaProvider.SetShuffle(Zone zone, bool state)
        {
            return MediaProviderData_SetShuffle(zone, state);
        }

        /// <summary>
        /// Gets the shuffle state
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract bool MediaProviderData_GetShuffle(Zone zone);
        bool IMediaProvider.GetShuffle(Zone zone)
        {
            return MediaProviderData_GetShuffle(zone);
        }

        /// <summary>
        /// Sets the repeat state
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract bool MediaProviderData_SetRepeat(Zone zone, bool state);
        bool IMediaProvider.SetRepeat(Zone zone, bool state)
        {
            return MediaProviderData_SetRepeat(zone, state);
        }

        /// <summary>
        /// Gets the repeat state
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract bool MediaProviderData_GetRepeat(Zone zone);
        bool IMediaProvider.GetRepeat(Zone zone)
        {
            return MediaProviderData_GetRepeat(zone);
        }

        /// <summary>
        /// Gets the abilities of a media source
        /// </summary>
        /// <param name="mediaSource"></param>
        /// <returns></returns>
        public abstract MediaProviderAbilities MediaProviderData_GetMediaSourceAbilities(string mediaSource);
        MediaProviderAbilities IMediaProvider.GetMediaSourceAbilities(string mediaSource)
        {
            return MediaProviderData_GetMediaSourceAbilities(mediaSource);
        }

        /// <summary>
        /// Gets a list of all available MediaSources from this provider
        /// </summary>
        /// <returns></returns>
        public abstract List<MediaSource> MediaProviderData_GetMediaSources();
        List<MediaSource> IMediaProvider.GetMediaSources()
        {
            return MediaProviderData_GetMediaSources();
        }

        /// <summary>
        /// Gets the currently active MediaSource 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public abstract MediaSource MediaProviderData_GetMediaSource(Zone zone);
        MediaSource IMediaProvider.GetMediaSource(Zone zone)
        {
            return MediaProviderData_GetMediaSource(zone);
        }

        /// <summary>
        /// Activates a MediaSource
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="mediaSourceName"></param>
        /// <returns></returns>
        public abstract string MediaProviderCommand_ActivateMediaSource(Zone zone, string mediaSourceName);
        string IMediaProvider.ActivateMediaSource(Zone zone, string mediaSourceName)
        {
            return MediaProviderCommand_ActivateMediaSource(zone, mediaSourceName);
        }

        #endregion


    }
}
