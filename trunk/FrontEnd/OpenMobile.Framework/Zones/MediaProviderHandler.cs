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
        }

        public void Dispose()
        {
            // Save current playlist to the db
            _Playlist.Save();
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
                PlayList_Initialize();
                ActivateDefaultMediaProvider();
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
                {
                    this._ActiveMediaProvider = value;
                }
            }
        }
        private IMediaProvider _ActiveMediaProvider;

        /// <summary>
        /// Activate default provider. Default is the first available audio playback provider.
        /// </summary>
        /// <returns></returns>
        public bool ActivateDefaultMediaProvider()
        {
            return ActivateMediaProvider(MediaProviderTypes.AudioPlayback);
        }

        /// <summary>
        /// Activates the first available provider of the given type
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public bool ActivateMediaProvider(MediaProviderTypes providerType)
        {
            return ActivateMediaProvider(providerType, String.Empty);
        }

        /// <summary>
        /// Activates a specific provider. If no name is given it will use the first provider that matches the types
        /// </summary>
        /// <param name="providerType">Type of provider to activate. Multiple types can be specified</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ActivateMediaProvider(MediaProviderTypes providerType, string name)
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
        public bool ActivateMediaProvider(IMediaProvider provider)
        {
            // Error check
            if (provider == null)
                return false;

            // Initialize provider
            if (provider.ActivateMediaProvider(_Zone))
            {
                // Uninitialize current provider
                if (_ActiveMediaProvider != null)
                    _ActiveMediaProvider.DeactivateMediaProvider(_Zone);

                // Map provider
                _ActiveMediaProvider = provider;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Cycles trough available providers of the specified type. Each call to this method goes to next available provider
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public bool CycleMediaProviders(MediaProviderTypes providerType)
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

        #region Playlist

        /// <summary>
        /// Current playlist
        /// </summary>
        public PlayList Playlist
        {
            get
            {
                return this._Playlist;
            }
            set
            {
                if (this._Playlist != value)
                {
                    this._Playlist = value;
                }
            }
        }
        private PlayList _Playlist;

        private void PlayList_Initialize()
        {
            // Initialize current playlist
            _Playlist = new PlayList(String.Format("{0}_PlayList_Current", _Zone));

            // Restore current playlist from the DB
            _Playlist.Load();
        }

        #endregion

        #region Media commands

        /// <summary>
        /// Starts to play the current location in the playlist
        /// </summary>
        public void Play()
        {
            _ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play, _Playlist.CurrentItem));
        }

        /// <summary>
        /// Plays the specific index in the playlist
        /// </summary>
        /// <param name="playlistIndex"></param>
        public void Play(int playlistIndex)
        {
            _Playlist.CurrentIndex = playlistIndex;
            Play();
        }

        /// <summary>
        /// Plays the specific media item from the playlist
        /// </summary>
        /// <param name="playlistIndex"></param>
        public void Play(mediaInfo media)
        {
            _Playlist.CurrentItem = media;
            Play();
        }

        /// <summary>
        /// Plays the next item in the playlist
        /// </summary>
        public void Next()
        {
            _Playlist.GotoNextMedia();
            Play();

            //_ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepForward));
        }

        /// <summary>
        /// Plays the previous item in the playlist
        /// </summary>
        public void Previous()
        {
            _Playlist.GotoPreviousMedia();
            Play();

            //_ActiveMediaProvider.ExecuteCommand(_Zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepBackward));
        }

        #endregion


    }
}
