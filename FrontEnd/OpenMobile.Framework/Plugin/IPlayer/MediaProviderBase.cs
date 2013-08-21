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
using OpenMobile;
using OpenMobile.Media;
using OpenMobile.Graphics;
using OpenMobile.Plugin;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// A common code base for the IMediaProvider interface
    /// </summary>
    /// <typeparam name="T">Type of data for local ZoneSpecificLocalData</typeparam>
    public abstract class MediaProviderBase : BasePluginCode, IMediaProvider
    {
        /// <summary>
        /// Zone specific local data
        /// </summary>
        protected class ZoneSpecificLocalData 
        {
            /// <summary>
            /// Provider info
            /// </summary>
            public MediaProviderInfo ProviderInfo = new MediaProviderInfo();
        }

        /// <summary>
        /// Initializes a MediaProviderCode plugin
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
        /// Base plugin for this provider
        /// </summary>
        public virtual IBasePlugin ProviderBasePlugin 
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Zone specific data
        /// </summary>
        protected Dictionary<Zone, ZoneSpecificLocalData> _ZoneSpecificData = new Dictionary<Zone, ZoneSpecificLocalData>();
        
        /// <summary>
        /// Media provider type for this plugin
        /// </summary>
        protected MediaProviderTypes _MediaProviderType;

        /// <summary>
        /// Gets the media provider types supported by this plugin
        /// </summary>
        /// <returns></returns>
        public abstract MediaProviderTypes MediaProviderType { get; }

        /// <summary>
        /// Available media sources for this plugin
        /// </summary>
        protected List<MediaSource> _MediaSources = new List<MediaSource>();

        /// <summary>
        /// Gets all media sources supported by this MediaProvider
        /// </summary>
        /// <returns></returns>
        public virtual List<MediaSource> GetMediaSources()
        {
            return _MediaSources;
        }

        /// <summary>
        /// Sets the currently active media source
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool SetMediaSource(Zone zone, string name)
        {
            ZoneSpecificLocalData dta = GetZoneSpecificDataInstance(zone);
            MediaSource mediaSource = _MediaSources.Find(x => x.Name.Equals(name));
            if (mediaSource == null)
                return false;

            mediaSource = mediaSource.Clone();
            mediaSource.InitPlayList();
            dta.ProviderInfo.MediaSource = mediaSource;

            Raise_OnProviderInfoChanged(zone, dta.ProviderInfo);
            return true;
        }

        /// <summary>
        /// Gets zone specific data
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        protected ZoneSpecificLocalData GetZoneSpecificDataInstance(Zone zone)
        {
            if (_ZoneSpecificData.ContainsKey(zone))
                return _ZoneSpecificData[zone];
            else
            {
                ZoneSpecificLocalData dta = new ZoneSpecificLocalData();
                _ZoneSpecificData.Add(zone, dta);
                return dta;
            }
        }

        /// <summary>
        /// Activate media provider (use this to power on)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual MediaProviderState ActivateMediaProvider(Zone zone)
        {
            return new MediaProviderState(true, String.Empty);
        }

        /// <summary>
        /// Deactivate media provider (use this to power off)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual MediaProviderState DeactivateMediaProvider(Zone zone)
        {
            return new MediaProviderState(false, String.Empty);
        }

        /// <summary>
        /// Gets the current information from the media provider
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public virtual MediaProviderInfo GetMediaProviderInfo(Zone zone)
        {
            return GetZoneSpecificDataInstance(zone).ProviderInfo;
        }

        /// <summary>
        /// Executes the specified command
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="command"></param>
        /// <returns>The returned data from the command</returns>
        public abstract bool ExecuteCommand(Zone zone, MediaProvider_CommandWrapper command);

        /// <summary>
        /// Gets data from the mediaprovider
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual object GetData(Zone zone, MediaProvider_DataWrapper data)
        {
            switch (data.DataType)
            {
                case MediaProvider_Data.PlaybackState:
                    return GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData.State;
                case MediaProvider_Data.MediaInfo:
                    return GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo;
                case MediaProvider_Data.PlaybackPositionData:
                    return GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData;
                case MediaProvider_Data.Suffle:
                    if (GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist != null)
                        return GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist.Random;
                    else
                        return false;
                case MediaProvider_Data.Repeat:
                    if (GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist != null)
                        return GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist.Repeat;
                    else
                        return false;
                case MediaProvider_Data.PlayList:
                    return GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Data changed event
        /// </summary>
        public virtual event MediaProviderData_EventHandler OnDataChanged;

        /// <summary>
        /// Raises the data changed event to any subscribers
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="e"></param>
        protected virtual void Raise_OnDataChanged(Zone zone, MediaProvider_Data dataType, params object[] parameters)
        {
            if (OnDataChanged != null)
            {
                MediaProviderData_EventHandler handler = OnDataChanged;
                handler(this, zone, new MediaProviderData_EventArgs(new MediaProvider_DataWrapper(dataType, parameters)));
            }
        }

        /// <summary>
        /// Provider info has changed
        /// </summary>
        public virtual event MediaProviderInfo_EventHandler OnProviderInfoChanged;

        /// <summary>
        /// Updates providerinfo
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="providerInfo"></param>
        protected virtual void MediaProviderInfo_Update(Zone zone, MediaProviderInfo providerInfo)
        {
            ZoneSpecificLocalData dta = GetZoneSpecificDataInstance(zone);
            dta.ProviderInfo.Zone = zone;
            dta.ProviderInfo = providerInfo;
            Raise_OnProviderInfoChanged(zone, dta.ProviderInfo);
        }

        /// <summary>
        /// Raises the provider info changed event to any subscribers
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="providerInfo"></param>
        protected virtual void Raise_OnProviderInfoChanged(Zone zone, MediaProviderInfo providerInfo)
        {
            if (OnProviderInfoChanged != null)
            {
                MediaProviderInfo_EventHandler handler = OnProviderInfoChanged;
                handler(this, zone, providerInfo);
            }
        }

        protected virtual void MediaInfo_MediaText_Set(Zone zone, string text1, string text2, bool raiseEvent = true)
        {
            // Clear media text
            MediaProviderInfo providerInfo = GetZoneSpecificDataInstance(zone).ProviderInfo;
            providerInfo.MediaText1 = text1;
            providerInfo.MediaText2 = text2;
            if (raiseEvent)
                Raise_OnProviderInfoChanged(zone, providerInfo);
        }

        protected virtual void MediaInfo_Clear(Zone zone)
        {
            GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo = new mediaInfo();
            MediaInfo_RaiseEvent(zone);
            Raise_OnDataChanged(zone, MediaProvider_Data.PlaybackPositionData, TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        protected virtual void MediaInfo_UpdateMissingInfo(Zone zone, string album, string artist, string name, string genre, int? length, int? tracknumber, bool raiseEvent = true)
        {
            bool updated = false;
            mediaInfo media = GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo;
            if (album != null)
                if (String.IsNullOrEmpty(media.Album))
                {
                    media.Album = album;
                    updated = true;
                }
            if (artist != null)
                if (String.IsNullOrEmpty(media.Artist))
                {
                    media.Artist = artist;
                    updated = true;
                }
            if (genre != null)
                if (String.IsNullOrEmpty(media.Genre))
                {
                    media.Genre = genre;
                    updated = true;
                }
            if (length.HasValue)
                if (media.Length != length.Value)
                {
                    media.Length = length.Value;
                    updated = true;
                }
            if (tracknumber.HasValue)
                if (media.TrackNumber != tracknumber.Value)
                {
                    media.TrackNumber = tracknumber.Value;
                    updated = true;
                }
            if (name != null)
                if (String.IsNullOrEmpty(media.Name))
                {
                    media.Name = name;
                    updated = true;
                }

            GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo = media;

            if (raiseEvent && updated)
                MediaInfo_RaiseEvent(zone);
        }
        protected virtual void MediaInfo_Set(Zone zone, mediaInfo media, bool raiseEvent = true)
        {
            GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo = media;
            if (raiseEvent)
                MediaInfo_RaiseEvent(zone);
        }
        protected virtual void MediaInfo_Set(Zone zone, string album, string artist, string name, string genre, int? length, int? tracknumber, bool raiseEvent = true)
        {
            mediaInfo media = GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo;
            if (album != null)
                media.Album = album;
            if (artist != null)
                media.Artist = artist;
            if (genre != null)
                media.Genre = genre;
            if (length.HasValue)
                media.Length = length.Value;
            if (tracknumber.HasValue)
                media.TrackNumber = tracknumber.Value;
            if (name != null)
                media.Name = name;
            GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo = media;

            if (raiseEvent)
                MediaInfo_RaiseEvent(zone);
        }
        protected virtual void MediaInfo_Set(Zone zone, string album, string artist, string name, string genre, int? length, int? tracknumber, OImage mediaImage, bool raiseEvent = true)
        {
            mediaInfo media = GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo;
            media.coverArt = mediaImage;
            GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo = media;
            MediaInfo_Set(zone, album, artist, name, genre, length, tracknumber, raiseEvent);
        }
        protected virtual void MediaInfo_RaiseEvent(Zone zone)
        {
            Raise_OnDataChanged(zone, MediaProvider_Data.MediaInfo, GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo);
        }

        protected virtual void MediaInfo_PlaybackPositionData_Set(Zone zone, TimeSpan? currentPos, TimeSpan? mediaLength)
        {
            MediaProvider_PlaybackData playbackData = GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData;
            
            TimeSpan pos;
            if (currentPos.HasValue)
                pos = currentPos.Value;
            else
                pos = TimeSpan.Zero;

            TimeSpan length;
            if (mediaLength.HasValue)
                length = mediaLength.Value;
            else
                length = TimeSpan.Zero;

            int percent = (int)((pos.TotalSeconds / length.TotalSeconds) * 100.0);

            playbackData.CurrentPos = pos;
            playbackData.CurrentPosPercent = percent;
            playbackData.Length = length;

            GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData = playbackData;

            Raise_OnDataChanged(zone, MediaProvider_Data.PlaybackPositionData, playbackData.CurrentPos, playbackData.Length, playbackData.CurrentPosPercent);
            Raise_OnProviderInfoChanged(zone, GetZoneSpecificDataInstance(zone).ProviderInfo);
        }
        protected virtual void MediaInfo_PlaybackPositionData_Set(Zone zone, int? currentPosInSeconds, int? mediaLengthInSeconds)
        {
            MediaProvider_PlaybackData playbackData = GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData;

            TimeSpan pos;
            if (currentPosInSeconds.HasValue)
                pos = TimeSpan.FromSeconds(currentPosInSeconds.Value);
            else
                pos = TimeSpan.Zero;

            TimeSpan length;
            if (mediaLengthInSeconds.HasValue)
                length = TimeSpan.FromSeconds(mediaLengthInSeconds.Value);
            else
                length = TimeSpan.Zero;

            int percent = (int)((pos.TotalSeconds / length.TotalSeconds) * 100.0);

            playbackData.CurrentPos = pos;
            playbackData.CurrentPosPercent = percent;
            playbackData.Length = length;

            GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData = playbackData;

            Raise_OnDataChanged(zone, MediaProvider_Data.PlaybackPositionData, playbackData.CurrentPos, playbackData.Length, playbackData.CurrentPosPercent);
            Raise_OnProviderInfoChanged(zone, GetZoneSpecificDataInstance(zone).ProviderInfo);
        }

        protected virtual void MediaInfo_PlayBackState_Set(Zone zone, MediaProvider_PlaybackState playbackState)
        {
            GetZoneSpecificDataInstance(zone).ProviderInfo.PlaybackData.State = playbackState;
            Raise_OnDataChanged(zone, MediaProvider_Data.PlaybackState, playbackState);
            Raise_OnProviderInfoChanged(zone, GetZoneSpecificDataInstance(zone).ProviderInfo);
        }

        protected virtual bool MediaSource_RegisterNew(MediaSource source)
        {
            MediaSource mediaSource = _MediaSources.Find(x => x.Name.Equals(source.Name));
            if (mediaSource == null)
                _MediaSources.Add(source);
            else
                return false;
            return true;
        }

        protected virtual bool MediaSource_Set(Zone zone, MediaSource source)
        {
            return SetMediaSource(zone, source.Name);
        }

    }
}
