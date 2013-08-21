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
using OpenMobile.helperFunctions;

namespace OpenMobile.Media
{
    /// <summary>
    /// A media handler engine
    /// </summary>
    public class MediaProviderHandler
    {
        private Zone _Zone = null;

        /// <summary>
        /// Creates a new mediaHandler and assigns it to a zone
        /// </summary>
        /// <param name="assignedZone"></param>
        public MediaProviderHandler(Zone assignedZone)
        {
            _Zone = assignedZone;
            BuiltInComponents.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
            BuiltInComponents.Host.OnMediaEvent += new MediaEvent(Host_OnMediaEvent);
        }

        void Host_OnMediaEvent(eFunction function, Zone zone, string arg)
        {
        }

        public void Dispose()
        {
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                if (!this._Zone.HasSubZones)
                {
                    ActivateDefaultMediaProvider();
                }
            }
        }

        #region Activate provider

        /// <summary>
        /// Currently active media provider
        /// </summary>
        public IMediaProvider ActiveMediaProvider
        {
            get
            {
                return this._ActiveMediaProvider;
            }
            set
            {
                if (this._ActiveMediaProvider != value)
                    ActivateMediaProvider(value);
            }
        }
        private IMediaProvider _ActiveMediaProvider;

        /// <summary>
        /// Activate default provider. Default is the first available audio playback provider.
        /// </summary>
        /// <returns></returns>
        public MediaProviderState ActivateDefaultMediaProvider()
        {
            return ActivateMediaProvider(MediaProviderTypes.AudioPlayback);
        }

        /// <summary>
        /// Activates the first available provider of the given type
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public MediaProviderState ActivateMediaProvider(MediaProviderTypes providerType)
        {
            return ActivateMediaProvider(providerType, String.Empty);
        }

        /// <summary>
        /// Activates a specific provider. If no name is given it will use the first provider that matches the types
        /// </summary>
        /// <param name="providerType">Type of provider to activate. Multiple types can be specified</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public MediaProviderState ActivateMediaProvider(MediaProviderTypes providerType, string name)
        {
            List<IBasePlugin> providers = helperFunctions.Plugins.Plugins.getPluginsOfType<IMediaProvider>();
            IMediaProvider provider = null;
            if (String.IsNullOrEmpty(name))
                provider = (IMediaProvider)providers.Find(x => (((IMediaProvider)x).MediaProviderType & providerType) != 0);
            else
                provider = (IMediaProvider)providers.Find(x => (((IMediaProvider)x).MediaProviderType & providerType) != 0 && x.pluginName == name);

            // Activate provider
            return ActivateMediaProvider(provider);
        }

        /// <summary>
        /// Activates a provider 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public MediaProviderState ActivateMediaProvider(IMediaProvider provider)
        {
            // Error check
            if (provider == null)
                return new MediaProviderState(false, String.Empty);

            // Uninitialize current provider
            if (_ActiveMediaProvider != null)
            {
                _ActiveMediaProvider.DeactivateMediaProvider(_Zone);

                // Disconnect events
                _ActiveMediaProvider.OnDataChanged -= new MediaProviderData_EventHandler(_ActiveMediaProvider_OnDataChanged);
                _ActiveMediaProvider.OnProviderInfoChanged -= new MediaProviderInfo_EventHandler(_ActiveMediaProvider_OnProviderInfoChanged);
            }

            // Initialize provider
            MediaProviderState providerState = provider.ActivateMediaProvider(_Zone);
            if (providerState.Activated)
            {
                // Map provider
                _ActiveMediaProvider = provider;
                
                // Connect events
                _ActiveMediaProvider.OnDataChanged += new MediaProviderData_EventHandler(_ActiveMediaProvider_OnDataChanged);
                _ActiveMediaProvider.OnProviderInfoChanged += new MediaProviderInfo_EventHandler(_ActiveMediaProvider_OnProviderInfoChanged);

                Raise_OnProviderChanged(_Zone);
                Raise_OnProviderInfoChanged(_Zone, _ActiveMediaProvider.GetMediaProviderInfo(_Zone));

                // Log events
                OM.Host.DebugMsg(DebugMessageType.Info, "MediaProviderHandler", String.Format("Media provider {0} activated on zone {1}", provider.pluginName, _Zone.Name));
            }
            else
            {   // Initialize failed

                // Write states to error log and notification bar
                OM.Host.DebugMsg( DebugMessageType.Error, "MediaProviderHandler", String.Format("Unable to activate media provider {0} on zone {1}, Reason from provider: {2}", provider.pluginName, _Zone.Name, providerState.InfoMessage));
                OM.Host.UIHandler.AddNotification(new Notification(Notification.Styles.Warning, provider, null, null, String.Format("Activation of provider {0} failed on zone {1}", provider.pluginName, _Zone.Name), providerState.InfoMessage));

                providerState = provider.DeactivateMediaProvider(_Zone);
            }

            return providerState;
        }

        void _ActiveMediaProvider_OnProviderInfoChanged(object sender, Zone zone, MediaProviderInfo providerInfo)
        {
            // Propagate the events to a higher level
            Raise_OnProviderInfoChanged(zone, providerInfo);
        }

        void _ActiveMediaProvider_OnDataChanged(object sender, Zone zone, MediaProviderData_EventArgs e)
        {
            // Ignore updates that's not for this zone
            if (zone.ID != this._Zone.ID)
                return;

            // Prosess the different data returned by the media handler
            switch (e.Data.DataType)
            {
                case MediaProvider_Data.PlaybackState:
                    break;

                case MediaProvider_Data.MediaInfo:
                    if (Params.IsParamsValid(e.Data.Parameters, 1))
                        _MediaInfo = Params.GetParam<mediaInfo>(e.Data.Parameters, 0);
                    break;

                case MediaProvider_Data.PlaybackPositionData:
                    if (Params.IsParamsValid(e.Data.Parameters, 3))
                    {
                        _PlaybackData.CurrentPos = Params.GetParam<TimeSpan>(e.Data.Parameters, 0);
                        _PlaybackData.Length = Params.GetParam<TimeSpan>(e.Data.Parameters, 1);
                        _PlaybackData.CurrentPosPercent = Params.GetParam<int>(e.Data.Parameters, 2);
                    }
                    break;

                case MediaProvider_Data.Custom:
                    break;
                default:
                    break;
            }

            // Propagate the events to a higher level
            Raise_OnDataChanged(zone, e);
        }

        /// <summary>
        /// Cycles trough available providers of the specified type. Each call to this method goes to next available provider
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public MediaProviderState CycleMediaProviders(MediaProviderTypes providerType)
        {
            List<IBasePlugin> providers = helperFunctions.Plugins.Plugins.getPluginsOfType<IMediaProvider>();
            providers = providers.FindAll(x => (((IMediaProvider)x).MediaProviderType & providerType) != 0);

            // Find index of current provider
            int index = providers.IndexOf((IBasePlugin)_ActiveMediaProvider);
            index++;
            if (index >= providers.Count)
                index = 0;

            // Get next provider
            IMediaProvider provider = (IMediaProvider)providers[index];

            // Activate provider
            return ActivateMediaProvider(provider);
        }

        #endregion

        #region PlayList2

        /// <summary>
        /// Current PlayList2
        /// </summary>
        public PlayList2 Playlist
        {
            get
            {
                if (_ActiveMediaProvider != null)
                    return (PlayList2)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.PlayList));
                return null;
            }
            set
            {
                _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.PlayList_Set, value));
            }
        }

        #endregion

        /// <summary>
        /// Is the current mediaProvider playing any media
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                MediaProvider_PlaybackState state = (MediaProvider_PlaybackState)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.PlaybackState));
                if (state == MediaProvider_PlaybackState.Playing)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Is the current mediaProvider paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                MediaProvider_PlaybackState state = (MediaProvider_PlaybackState)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.PlaybackState));
                if (state == MediaProvider_PlaybackState.Paused)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Is the current mediaProvider stopped
        /// </summary>
        public bool IsStopped
        {
            get
            {
                MediaProvider_PlaybackState state = (MediaProvider_PlaybackState)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.PlaybackState));
                if (state == MediaProvider_PlaybackState.Stopped)
                    return true;
                return false;
            }
        }

        #region Media commands

        private Timer tmrDelayedCommand;

        /// <summary>
        /// Starts to play the current location in the PlayList
        /// </summary>
        public void Play()
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play));
        }

        /// <summary>
        /// Plays the specific index in the PlayList
        /// </summary>
        /// <param name="playlistIndex"></param>
        public void Play(int playlistIndex)
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, playlistIndex));
        }

        /// <summary>
        /// Plays the specific media item from the PlayList
        /// </summary>
        /// <param name="media"></param>
        public void Play(mediaInfo media)
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, media));
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Pause));
        }

        /// <summary>
        /// Stop playback
        /// </summary>
        public void Stop()
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Stop));
        }

        /// <summary>
        /// Plays the next item in the PlayList
        /// </summary>
        public void Next()
        {
            //    NextDelayedPlay();
            //else
                _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepForward));
        }

        /// <summary>
        /// Plays the previous item in the PlayList
        /// </summary>
        public void Previous()
        {
            //if (_PlaybackState == MediaProvider_PlaybackState.Playing)
            //    PreviousDelayedPlay();
            //else
                _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepBackward));
        }

        /// <summary>
        /// Seeks forward in the current playback media (default time is 5 seconds)
        /// </summary>
        public void SeekFwd()
        {
            SeekFwd(5);
        }

        /// <summary>
        /// Seeks forward in the current playback media with the specified seconds
        /// </summary>
        /// <param name="seconds"></param>
        public void SeekFwd(int seconds)
        {
            //_PlaybackState = MediaProvider_PlaybackState.SeekingForwards;
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Forward, seconds));
        }

        /// <summary>
        /// Seeks backwards in the current playback media (default time is 5 seconds)
        /// </summary>
        public void SeekBwd()
        {
            SeekBwd(5);
        }

        /// <summary>
        /// Seeks backwards in the current playback media with the specified seconds
        /// </summary>
        /// <param name="seconds"></param>
        public void SeekBwd(int seconds)
        {
            //_PlaybackState = MediaProvider_PlaybackState.SeekingBackwards;
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Backward, seconds));
        }

        /// <summary>
        /// The suffle state of the current playback
        /// </summary>
        public bool Shuffle
        {
            get
            {
                if (_ActiveMediaProvider == null)
                    return false;
                return (bool)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.Suffle));
            }
            set
            {
                if (_ActiveMediaProvider != null)
                    _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Suffle, value));
            }
        }

        /// <summary>
        /// The repeat state of the current playback
        /// </summary>
        public bool Repeat
        {
            get
            {
                if (_ActiveMediaProvider == null)
                    return false;
                return (bool)_ActiveMediaProvider.GetData(_Zone, new MediaProvider_DataWrapper(MediaProvider_Data.Repeat));
            }
            set
            {
                if (_ActiveMediaProvider != null)
                    _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Repeat, value));
            }
        }

        #endregion

        /// <summary>
        /// MediaInfo from the current media provider
        /// </summary>
        public mediaInfo MediaInfo
        {
            get
            {
                return this._MediaInfo;
            }
        }
        private mediaInfo _MediaInfo = new mediaInfo();

        /// <summary>
        /// Provider info for currently active media provider 
        /// </summary>
        public MediaProviderInfo ProviderInfo
        {
            get
            {
                if (_ActiveMediaProvider == null)
                    return null;
                return this._ActiveMediaProvider.GetMediaProviderInfo(_Zone);
            }
        }

        /// <summary>
        /// Playback data of current media provider
        /// </summary>
        public MediaProvider_PlaybackData PlaybackData
        {
            get
            {
                return this._PlaybackData;
            }
        }
        private MediaProvider_PlaybackData _PlaybackData = new MediaProvider_PlaybackData();

        /// <summary>
        /// Data changed event
        /// </summary>
        public virtual event MediaProviderData_EventHandler OnDataChanged;

        /// <summary>
        /// Raises the data changed event to any subscribers
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="e"></param>
        protected virtual void Raise_OnDataChanged(Zone zone, MediaProviderData_EventArgs e)
        {
            if (OnDataChanged != null)
            {
                MediaProviderData_EventHandler handler = OnDataChanged;
                handler(this, zone, e);
            }
        }

        public virtual event MediaProviderChanged_EventHandler OnProviderChanged;

        protected virtual void Raise_OnProviderChanged(Zone zone)
        {
            if (OnProviderChanged != null)
            {
                MediaProviderChanged_EventHandler handler = OnProviderChanged;
                handler(this, zone);
            }
        }

        public virtual event MediaProviderInfo_EventHandler OnProviderInfoChanged;

        protected virtual void Raise_OnProviderInfoChanged(Zone zone, MediaProviderInfo info)
        {
            if (OnProviderInfoChanged != null)
            {
                MediaProviderInfo_EventHandler handler = OnProviderInfoChanged;
                handler(this, zone, info);
            }
        }

  

    }
}
