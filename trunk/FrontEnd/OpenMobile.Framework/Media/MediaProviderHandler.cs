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
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;

namespace OpenMobile.Media
{
    /// <summary>
    /// A class for handling media providers
    /// </summary>
    public class MediaProviderHandler: IMediaProviderHandler
    {
        /// <summary>
        /// The currently active media providers per zone
        /// </summary>
        public Dictionary<Zone, IBasePlugin> CurrentMediaProviders
        {
            get
            {
                return this._CurrentMediaProviders;
            }
            set
            {
                if (this._CurrentMediaProviders != value)
                {
                    this._CurrentMediaProviders = value;
                }
            }
        }
        private Dictionary<Zone, IBasePlugin> _CurrentMediaProviders = new Dictionary<Zone,IBasePlugin>();

        /// <summary>
        /// Available media providers
        /// </summary>
        public List<IBasePlugin> MediaProviders
        {
            get
            {
                return this._MediaProviders;
            }
            set
            {
                if (this._MediaProviders != value)
                {
                    this._MediaProviders = value;
                }
            }
        }
        private List<IBasePlugin> _MediaProviders;

        private Dictionary<Zone, MediaSource> _CurrentMediaSources = new Dictionary<Zone, MediaSource>();

        /// <summary>
        /// Creates a handler for media providers
        /// </summary>
        public MediaProviderHandler()
        {
            _MediaProviders = OM.Host.GetPlugins<IMediaProvider>();

            // Register datasources
            DataSources_Default_Register();

            // Register commands
            Commands_Default_Register();

            // Restore active media provider per screen
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                var zone = OM.Host.ZoneHandler.GetActiveZone(i);

                // Set default provider if no provider is currently selected
                StoredData.SetDefaultValue(BuiltInComponents.OMInternalPlugin, GetSettingNameForCurrentMediaProvider(zone), _MediaProviders[0].pluginName);

                // Read setting from DB
                var mediaProviderName = StoredData.Get(BuiltInComponents.OMInternalPlugin, GetSettingNameForCurrentMediaProvider(zone));
                if (mediaProviderName != null)
                {
                    // Activate provider
                    var mediaProvider = GetMediaProviderByName(mediaProviderName);
                    if (mediaProvider != null)
                        ActivateMediaProvider((IBasePlugin)mediaProvider, zone);
                }
            }

            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
        }

        /// <summary>
        /// Returns the media provider object with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IMediaProvider GetMediaProviderByName(string name)
        {
            return _MediaProviders.Find(x => x.pluginName == name) as IMediaProvider;
        }

        /// <summary>
        /// Returns a list of all media providers which provides the specified media source type
        /// </summary>
        /// <param name="mediaSourceType"></param>
        /// <returns></returns>
        public List<IMediaProvider> GetMediaProvidersByMediaSourceType(string mediaSourceType)
        {
            var providers = _MediaProviders.Where(x => 
                {
                    var mediaSources = ((IMediaProvider)x).GetMediaSources();
                    if (mediaSources == null)
                        return false;

                    return mediaSources.Any(y => y.MediaSourceType == mediaSourceType);
                }                
                );
            return providers.Cast<IMediaProvider>().ToList();
        }

        public List<MediaSource> GetMediaSourcesForProvider(IMediaProvider provider)
        {
            return provider.GetMediaSources();                
        }

        public IMediaProvider GetMediaProviderForZoneOrScreen(int id)
        {
            var zone = OM.Host.ZoneHandler.GetZone(id);

            IBasePlugin provider = null;
            if (!_CurrentMediaProviders.ContainsKey(zone))
                return null;

            provider = _CurrentMediaProviders[zone];
            return provider as IMediaProvider;
        }

        public bool IsMediaSourceOfType(MediaSource mediaSource, params string[] mediaSourceTypes)
        {
            foreach (var mediaSourceType in mediaSourceTypes)
            {
                if (mediaSource.MediaSourceType == mediaSourceType)
                    return true;
            }
            return false;
        }

        private IBasePlugin GetMediaProvider(string name)
        {
            return _MediaProviders.Find(x => x.pluginName == name);
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.closeProgram)
            {
                foreach (var provider in _CurrentMediaProviders)
                {
                    DeactivateMediaProvider(provider.Value, provider.Key);

                    // Save providers to DB
                    StoredData.Set(BuiltInComponents.OMInternalPlugin, GetSettingNameForCurrentMediaProvider(provider.Key), provider.Value.pluginName);
                }                
            }
        }

        private string GetSettingNameForCurrentMediaProvider(Zone zone)
        {
            return String.Format("MediaProviderHandler.CurrentMediaProvider.Zone:{0}", zone);
        }

        private string ActivateMediaProvider(IBasePlugin mediaProvider, Zone zone)
        {
            try
            {
                _CurrentMediaProviders.Add(zone, mediaProvider);

                // Push data source
                PushMediaProviderToDataSource(mediaProvider, zone);

                IMediaProvider provider = (IMediaProvider)mediaProvider;
                provider.MediaProviderHandler = this;

                InitializeDataSources(provider, zone);

                return provider.Activate(zone);
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg(String.Format("MediaProviderHandler: Unable to activate provider:{0}", mediaProvider), ex);
            }
            return "Unable to activate provider";
        }

        private string DeactivateMediaProvider(IBasePlugin mediaProvider, Zone zone)
        {
            if (mediaProvider == null)
                return "Unable to deactivate provider";

            try
            {
                IMediaProvider provider = (IMediaProvider)mediaProvider;
                var result = provider.Deactivate(zone);

                _CurrentMediaProviders.Remove(zone);
                // Push data source
                PushMediaProviderToDataSource(null, zone);

                return result;
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg(String.Format("MediaProviderHandler: Unable to deactivate provider:{0}", mediaProvider), ex);
            }
            return "Unable to deactivate provider";
        }

        private void InitializeDataSources(IMediaProvider mediaProvider, Zone zone)
        {
            ((IMediaProviderHandler)(this)).SetMediaTextCallback(mediaProvider, zone, "", "");
            ((IMediaProviderHandler)(this)).IsPausedCallback(mediaProvider, zone, false);
            ((IMediaProviderHandler)(this)).IsStoppedCallback(mediaProvider, zone, true);
            ((IMediaProviderHandler)(this)).IsPlayingCallback(mediaProvider, zone, false);
            ((IMediaProviderHandler)(this)).SetMediaInfoCallback(mediaProvider, zone, new mediaInfo(), TimeSpan.Zero);
            ((IMediaProviderHandler)(this)).RefreshPlaylistCallback(mediaProvider, zone);

            MediaSource_RefreshDataSources(zone);
        }

        private void MediaSource_RefreshDataSources(Zone zone)
        {
            // Refresh datasources for zone
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSources", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource.Abilities", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource.Name", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource.Description", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource.Icon", zone.Index));
            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.MediaSource.AdditionalData", zone.Index));

            // Refresh datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
            {
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSources");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource.Abilities");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource.Name");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource.Description");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource.Icon");
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaSource.AdditionalData");
            }
        }

        #region DataSources

        private void DataSources_Default_Register()
        {
            // Create one set of datasources per zone
            for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
            {
                Zone zone = OM.Host.ZoneHandler.Zones[i];

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Playing", 0, DataSource.DataTypes.binary, null, "MediaSource is playing as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Stopped", 0, DataSource.DataTypes.binary, null, "MediaSource has stopped as bool"), true);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Paused", 0, DataSource.DataTypes.binary, null, "MediaSource has paused as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Busy", 0, DataSource.DataTypes.binary, null, "MediaSource is busy as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Repeat", 0, DataSource.DataTypes.binary, DataSourceGetter, "Repeat is enabled as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Shuffle", 0, DataSource.DataTypes.binary, DataSourceGetter, "Shuffle is enabled as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Pos.TimeSpan", 0, DataSource.DataTypes.raw, null, "Current playback position as TimeSpan"), TimeSpan.Zero);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Pos.Seconds", 0, DataSource.DataTypes.raw, null, "Current playback position in seconds as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Pos.Text", 0, DataSource.DataTypes.text, null, "Current playback position as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playback", "Pos.Percent", 0, DataSource.DataTypes.percent, null, "Current playback position as percentage of total length as integer"), 0);

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Artist", 0, DataSource.DataTypes.text, null, "Artist as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Album", 0, DataSource.DataTypes.text, null, "Album as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Genre", 0, DataSource.DataTypes.text, null, "Genre as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Rating", 0, DataSource.DataTypes.text, null, "Rating 0-5 (-1 for not set) as int"), -1);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Length.TimeSpan", 0, DataSource.DataTypes.raw, null, "Total length as TimeSpan"), TimeSpan.Zero);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Length.Seconds", 0, DataSource.DataTypes.raw, null, "Total length in seconds as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Length.Text", 0, DataSource.DataTypes.text, null, "Total length as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Location", 0, DataSource.DataTypes.text, null, "Media location/url as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Name", 0, DataSource.DataTypes.text, null, "Name as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "TrackNumber", 0, DataSource.DataTypes.text, null, "TrackNumber as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "Type", 0, DataSource.DataTypes.text, null, "Type of media as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", "CoverArt", 0, DataSource.DataTypes.image, null, "CoverArt of media as OImage"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaInfo", String.Empty, 0, DataSource.DataTypes.raw, null, "Current media info as mediaInfo"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playlist", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "Current playlist as Playlist"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaText1", String.Empty, 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 1 as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaText2", String.Empty, 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 2 as string"), String.Empty);

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaProvider", String.Empty, 0, DataSource.DataTypes.raw, null, "Current MediaProvider as full iBasePlugin object"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSources", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "List of available media sources from current media provider as List<MediaSource>"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "Current media source as MediaSource"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Abilities", 0, DataSource.DataTypes.raw, DataSourceGetter, "Abilities of the current media source as MediaProviderAbilities"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Name", 0, DataSource.DataTypes.text, DataSourceGetter, "Name of current media source as string"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Description", 0, DataSource.DataTypes.text, DataSourceGetter, "Description of current media source as string"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Icon", 0, DataSource.DataTypes.image, DataSourceGetter, "Icon of current media source as ImageItem"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Data", 0, DataSource.DataTypes.raw, DataSourceGetter, "Misc additional data from media source. Datatype is specific to the datasource, please consult documentation from the media provider for overview of data"), null);
            }

            // Create one set of datasources per screen
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Playing", 0, DataSource.DataTypes.binary, null, "MediaSource is playing as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Stopped", 0, DataSource.DataTypes.binary, null, "MediaSource has stopped as bool"), true);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Paused", 0, DataSource.DataTypes.binary, null, "MediaSource has paused as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Busy", 0, DataSource.DataTypes.binary, null, "MediaSource is busy as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Repeat", 0, DataSource.DataTypes.binary, DataSourceGetter, "Repeat is enabled as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Shuffle", 0, DataSource.DataTypes.binary, DataSourceGetter, "Shuffle is enabled as bool"), false);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Pos.TimeSpan", 0, DataSource.DataTypes.raw, null, "Current playback position as TimeSpan"), TimeSpan.Zero);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Pos.Seconds", 0, DataSource.DataTypes.raw, null, "Current playback position in seconds as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Pos.Text", 0, DataSource.DataTypes.text, null, "Current playback position as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playback", "Pos.Percent", 0, DataSource.DataTypes.percent, null, "Current playback position as percentage of total length as integer"), 0);

                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Artist", 0, DataSource.DataTypes.text, null, "Artist as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Album", 0, DataSource.DataTypes.text, null, "Album as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Genre", 0, DataSource.DataTypes.text, null, "Genre as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Rating", 0, DataSource.DataTypes.text, null, "Rating 0-5 (-1 for not set) as int"), -1);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Length.TimeSpan", 0, DataSource.DataTypes.raw, null, "Total length as TimeSpan"), TimeSpan.Zero);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Length.Seconds", 0, DataSource.DataTypes.raw, null, "Total length in seconds as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Length.Text", 0, DataSource.DataTypes.text, null, "Total length as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Location", 0, DataSource.DataTypes.text, null, "Media location/url as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Name", 0, DataSource.DataTypes.text, null, "Name as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "TrackNumber", 0, DataSource.DataTypes.text, null, "TrackNumber as int"), 0);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "Type", 0, DataSource.DataTypes.text, null, "Type of media as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", "CoverArt", 0, DataSource.DataTypes.image, null, "CoverArt of media as OImage"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaInfo", String.Empty, 0, DataSource.DataTypes.raw, null, "Current media info as mediaInfo"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playlist", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "Current playlist as Playlist"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaText1", String.Empty, 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 1 as string"), String.Empty);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaText2", String.Empty, 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 2 as string"), String.Empty);

                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaProvider", String.Empty, 0, DataSource.DataTypes.raw, null, "Current MediaProvider as full iBasePlugin object"), null);

                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSources", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "List of available media sources from current media provider as List<MediaSource>"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", String.Empty, 0, DataSource.DataTypes.raw, DataSourceGetter, "Current media source as MediaSource"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Abilities", 0, DataSource.DataTypes.raw, DataSourceGetter, "Abilities of the current media source as MediaProviderAbilities"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Name", 0, DataSource.DataTypes.text, DataSourceGetter, "Name of current media source as string"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Description", 0, DataSource.DataTypes.text, DataSourceGetter, "Description of current media source as string"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Icon", 0, DataSource.DataTypes.image, DataSourceGetter, "Icon of current media source as ImageItem"), null);
                OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Data", 0, DataSource.DataTypes.image, DataSourceGetter, "Misc additional data from media source. Datatype is specific to the datasource, please consult documentation from the media provider for overview of data"), null);
            }
        }

        private void CreateDataSourceFromDictionary(Dictionary<string, object> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                // Create one set of datasources per zone
                for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
                {
                    Zone zone = OM.Host.ZoneHandler.Zones[i];
                    OM.Host.DataHandler.AddDataSource(new DataSource(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", String.Format("Data.{0}", key), 1000, DataSource.DataTypes.raw, DataSourceGetter, "Additional data from media source"), dictionary[key]);
                }
            // Create one set of datasources per screen
                for (int i = 0; i < OM.Host.ScreenCount; i++)
                {
                    OM.Host.DataHandler.AddDataSource(new DataSource(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", String.Format("Data.{0}", key), 1000, DataSource.DataTypes.raw, DataSourceGetter, "Additional data from media source"), dictionary[key]);
                }
            }
        }

        private object DataSourceGetter(DataSource datasource, out bool result, object[] param)
        {
            result = false;

            string datasourceName = datasource.FullNameWithoutScreen.Replace(BuiltInComponents.OMInternalPlugin.pluginName, "");
            
            Zone zone = null;
            if (datasource.NameLevel1.Equals("Zone"))
            {
                zone = OM.Host.ZoneHandler.GetActiveZone(datasource.Screen);

                // Strip away unwanted information from the command name
                datasourceName = datasourceName.Replace("Zone.", "");
            }
            else if (datasource.NameLevel1.Contains("Zone"))
            {
                int zoneIndex = int.Parse(datasource.NameLevel1.Replace("Zone", ""));
                zone = OM.Host.ZoneHandler.Zones[zoneIndex];

                // Strip away unwanted information from the name
                datasourceName = datasourceName.Replace(String.Format("{0}.", datasource.NameLevel1), "");
            }

            // Check for a valid media provider, if not cancel request
            if (zone == null || !_CurrentMediaProviders.ContainsKey(zone))
                return null;

            result = true;

            var currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];

            try
            {
                switch (datasourceName)
                {
                    case "Playlist":
                        {
                            return currentMediaProvider.GetPlaylist(zone);
                        }
                    case "Shuffle":
                        {
                            return currentMediaProvider.GetShuffle(zone);
                        }
                    case "Repeat":
                        {
                            return currentMediaProvider.GetRepeat(zone);
                        }
                    case "MediaSources":
                        {
                            var mediaSources = currentMediaProvider.GetMediaSources();
                            if (mediaSources == null)
                                return null;
                            return mediaSources;
                        }
                    case "MediaSource":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;

                            if (!_CurrentMediaSources.ContainsKey(zone) || _CurrentMediaSources[zone] != mediaSource)
                            {
                                if (!_CurrentMediaSources.ContainsKey(zone))
                                    _CurrentMediaSources.Add(zone, mediaSource);

                                _CurrentMediaSources[zone] = mediaSource;
                                // TODO: Remap datasources
                                CreateDataSourceFromDictionary(mediaSource.AdditionalData);
                                CreateCommandFromDictionary(mediaSource.AdditionalCommands);
                            }

                            return mediaSource;
                        }
                    case "MediaSource.Abilities":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;
                            return currentMediaProvider.GetMediaSourceAbilities(mediaSource.Name);
                        }
                    case "MediaSource.Name":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;
                            return mediaSource.Name;
                        }
                    case "MediaSource.Description":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;
                            return mediaSource.Description;
                        }
                    case "MediaSource.Icon":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;
                            return mediaSource.Icon;
                        }
                    case "MediaSource.AdditionalData":
                        {
                            var mediaSource = currentMediaProvider.GetMediaSource(zone);
                            if (mediaSource == null)
                                return null;
                            return mediaSource.AdditionalData;
                        }

                    default:
                        {
                            // Check if this is a generic datasource
                            if (datasourceName.Contains("MediaSource.Data."))
                            {
                                var mediaSource = currentMediaProvider.GetMediaSource(zone);
                                if (mediaSource == null)
                                    return null;

                                // Mask out common part to get the individual part
                                datasourceName = datasourceName.Replace("MediaSource.Data.", "");
                                if (mediaSource.AdditionalData.ContainsKey(datasourceName))
                                    return mediaSource.AdditionalData[datasourceName];
                                else
                                    return null;
                            }
                        }
                        break;
                }
            }
            catch (NotImplementedException ex)
            {
                OM.Host.DebugMsg(String.Format("MediaProviderHandler: DataSource:{0} not implemented in provider:{1}", datasource.FullName, _CurrentMediaProviders[zone]), ex);
            }
            catch
            {
                result = false;
            }

            return null;
        }

        void IMediaProviderHandler.IsPlayingCallback(IMediaProvider provider, Zone zone, bool state)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.Playback.Playing", zone.Index), state);
            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playback.Playing", state);
        }

        void IMediaProviderHandler.IsStoppedCallback(IMediaProvider provider, Zone zone, bool state)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.Playback.Stopped", zone.Index), state);
            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playback.Stopped", state);
        }

        void IMediaProviderHandler.IsPausedCallback(IMediaProvider provider, Zone zone, bool state)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.Playback.Paused", zone.Index), state);
            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playback.Paused", state);
        }

        void IMediaProviderHandler.IsBusyCallback(IMediaProvider provider, Zone zone, bool state)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.Playback.Busy", zone.Index), state);
            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playback.Busy", state);
        }

        void IMediaProviderHandler.SetMediaInfoCallback(IMediaProvider provider, Zone zone, mediaInfo media, TimeSpan playbackPos)
        {
            // Set datasource for zone
            PushMediaDataToDataSource(String.Format("Zone{0}", zone.Index), media, playbackPos);

            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                PushMediaDataToDataSource("Zone", media, playbackPos, screen);
        }

        void IMediaProviderHandler.SetMediaTextCallback(IMediaProvider provider, Zone zone, string text1, string text2)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.MediaText1", zone.Index), text1);
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.MediaText2", zone.Index), text2);

            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
            {
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaText1", text1);
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaText2", text2);
            }
        }

        void IMediaProviderHandler.RefreshPlaylistCallback(IMediaProvider provider, Zone zone)
        {
            //// Set datasource for zone
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.Playlist", zone.Index), playlist);

            //// Set datasource for screen
            //foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
            //{
            //    OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playlist", playlist);
            //}

            OM.Host.DataHandler.RefreshDataSource(BuiltInComponents.OMInternalPlugin, string.Format("Zone{0}.Playlist", zone.Index));

            // Refresh datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
            {
                OM.Host.DataHandler.RefreshDataSource(screen, BuiltInComponents.OMInternalPlugin, "Zone.Playlist");
            }

        }

        void IMediaProviderHandler.SetMediaSourceCallback(IMediaProvider provider, Zone zone, MediaSource mediaSource)
        {
            MediaSource_RefreshDataSources(zone);
        }

        private void PushMediaDataToDataSource(string datasourceBaseName, mediaInfo media, TimeSpan playbackPos, int? screen = null)
        {
            TimeSpan length = TimeSpan.FromSeconds(media.Length);
            if (screen.HasValue)
            {
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Artist", datasourceBaseName), media.Artist);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Album", datasourceBaseName), media.Album);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Genre", datasourceBaseName), media.Genre);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Rating", datasourceBaseName), media.Rating);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.Seconds", datasourceBaseName), media.Length);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.TimeSpan", datasourceBaseName), length);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.Text", datasourceBaseName), TimeSpanToString(length));
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Location", datasourceBaseName), media.Location);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Name", datasourceBaseName), media.Name);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Rating", datasourceBaseName), media.Rating);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.TrackNumber", datasourceBaseName), media.TrackNumber);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Type", datasourceBaseName), media.Type.ToString());
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.CoverArt", datasourceBaseName), media.coverArt);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo", datasourceBaseName), media);

                int playbackPercent = (int)((playbackPos.TotalSeconds / (double)media.Length) * 100d);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Seconds", datasourceBaseName), playbackPos.TotalSeconds);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.TimeSpan", datasourceBaseName), playbackPos);
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Text", datasourceBaseName), TimeSpanToString(playbackPos));
                OM.Host.DataHandler.PushDataSourceValue(screen.Value, BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Percent", datasourceBaseName), playbackPercent);
            }
            else
            {
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Artist", datasourceBaseName), media.Artist);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Album", datasourceBaseName), media.Album);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Genre", datasourceBaseName), media.Genre);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Rating", datasourceBaseName), media.Rating);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.Seconds", datasourceBaseName), media.Length);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.TimeSpan", datasourceBaseName), length);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Length.Text", datasourceBaseName), TimeSpanToString(length));
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Location", datasourceBaseName), media.Location);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Name", datasourceBaseName), media.Name);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Rating", datasourceBaseName), media.Rating);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.TrackNumber", datasourceBaseName), media.TrackNumber);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.Type", datasourceBaseName), media.Type.ToString());
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo.CoverArt", datasourceBaseName), media.coverArt);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.MediaInfo", datasourceBaseName), media);

                int playbackPercent = (int)((playbackPos.TotalSeconds / (double)media.Length) * 100d);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Seconds", datasourceBaseName), playbackPos.TotalSeconds);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.TimeSpan", datasourceBaseName), playbackPos);
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Text", datasourceBaseName), TimeSpanToString(playbackPos));
                OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Playback.Pos.Percent", datasourceBaseName), playbackPercent);
            }
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Artist", this.pluginName, zone.Index), media.Artist);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Album", this.pluginName, zone.Index), media.Album);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Genre", this.pluginName, zone.Index), media.Genre);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Rating", this.pluginName, zone.Index), media.Rating);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Length.Seconds", this.pluginName, zone.Index), media.Length);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Length.TimeSpan", this.pluginName, zone.Index), length);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Length.Text", this.pluginName, zone.Index), TimeSpanToString(length));
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Location", this.pluginName, zone.Index), media.Location);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Name", this.pluginName, zone.Index), media.Name);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Rating", this.pluginName, zone.Index), media.Rating);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.TrackNumber", this.pluginName, zone.Index), media.TrackNumber);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.Type", this.pluginName, zone.Index), media.Type.ToString());
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo.CoverArt", this.pluginName, zone.Index), media.coverArt);
            //OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.MediaInfo", this.pluginName, zone.Index), media);
        }

        private string TimeSpanToString(TimeSpan timespan)
        {
            if (timespan.Hours > 0)
                return String.Format("{0:00}:{1:00}:{2:00}", timespan.Hours, timespan.Minutes, timespan.Seconds);
            else
                return String.Format("{0:00}:{1:00}", timespan.Minutes, timespan.Seconds);
        }

        private void PushMediaProviderToDataSource(IBasePlugin provider, Zone zone)
        {
            // Set datasource for zone
            OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}.MediaProvider", zone.Index), provider);

            // Set datasource for screen
            foreach (var screen in OM.Host.ZoneHandler.GetScreensForActiveZone(zone))
                OM.Host.DataHandler.PushDataSourceValue(screen, BuiltInComponents.OMInternalPlugin, "Zone.MediaProvider", provider);
        }

        #endregion

        #region Commands

        private void Commands_Default_Register()
        {
            // Create one set of datasources per zone
            for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
            {
                Zone zone = OM.Host.ZoneHandler.Zones[i];
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Play", "", CommandExecutor, 0, false, "Activates media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Stop", "", CommandExecutor, 0, false, "Stops media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Pause", "", CommandExecutor, 0, false, "Pause media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Next", "", CommandExecutor, 0, false, "Goto to next media"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Previous", "", CommandExecutor, 0, false, "Goto previous media"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "SeekForward", "", CommandExecutor, 0, false, "Seek forward"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "SeekBackward", "", CommandExecutor, 0, false, "Seek backward"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Shuffle", "Enable", CommandExecutor, 0, false, "Enable shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Shuffle", "Disable", CommandExecutor, 0, false, "Disable shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Shuffle", "Toggle", CommandExecutor, 0, false, "Toggle shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Repeat", "Enable", CommandExecutor, 0, false, "Enable repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Repeat", "Disable", CommandExecutor, 0, false, "Disable repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Repeat", "Toggle", CommandExecutor, 0, false, "Toggle repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "Playlist", "Set", CommandExecutor, 0, false, "Set current playlist"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaProvider", "Activate", CommandExecutor, 1, false, "Activate a media provider, Param0: plugin name of media provider"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaProvider", "Deactivate", CommandExecutor, 0, false, "Deactivates the current media provider"));
                OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", "Activate", CommandExecutor, 1, false, "Activate a media source, Param0: Name of media source (available names can be found in the MediaSources datasource)"));
            }

            // Create one set of datasources per screen
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Play", "", CommandExecutor, 0, false, "Activates media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Stop", "", CommandExecutor, 0, false, "Stops media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Pause", "", CommandExecutor, 0, false, "Pause media playback"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Next", "", CommandExecutor, 0, false, "Goto to next media"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Previous", "", CommandExecutor, 0, false, "Goto previous media"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "SeekForward", "", CommandExecutor, 0, false, "Seek forward"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "SeekBackward", "", CommandExecutor, 0, false, "Seek backward"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Shuffle", "Enable", CommandExecutor, 0, false, "Enable shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Shuffle", "Disable", CommandExecutor, 0, false, "Disable shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Shuffle", "Toggle", CommandExecutor, 0, false, "Toggle shuffle"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Repeat", "Enable", CommandExecutor, 0, false, "Enable repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Repeat", "Disable", CommandExecutor, 0, false, "Disable repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Repeat", "Toggle", CommandExecutor, 0, false, "Toggle repeat"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "Playlist", "Set", CommandExecutor, 0, false, "Set current playlist"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaProvider", "Activate", CommandExecutor, 1, false, "Activate a media provider, Param0: plugin name of media provider"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaProvider", "Deactivate", CommandExecutor, 0, false, "Deactivates the current media provider"));
                OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", "Activate", CommandExecutor, 1, false, "Activate a media source, Param0: Name of media source (available names can be found in the MediaSources datasource)"));
            }

        }

        private void CreateCommandFromDictionary(Dictionary<string, MediaSourceCommandDelegate> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                // Create one set of commands per zone
                for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
                {
                    Zone zone = OM.Host.ZoneHandler.Zones[i];
                    OM.Host.CommandHandler.AddCommand(new Command(BuiltInComponents.OMInternalPlugin, String.Format("Zone{0}", i), "MediaSource", String.Format("Command.{0}", key), CommandExecutor, 0, false, "Additional commands from media source"));
                }
                // Create one set of datasources per screen
                for (int i = 0; i < OM.Host.ScreenCount; i++)
                {
                    OM.Host.CommandHandler.AddCommand(new Command(true, BuiltInComponents.OMInternalPlugin, "Zone", "MediaSource", String.Format("Command.{0}", key), CommandExecutor, 0, false, "Additional commands from media source"));
                }
            }
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = false;

            string commandName = command.FullNameWithoutScreen.Replace(BuiltInComponents.OMInternalPlugin.pluginName, "");

            Zone zone = null;
            if (command.NameLevel1.Equals("Zone"))
            {
                zone = OM.Host.ZoneHandler.GetActiveZone(command.Screen);

                // Strip away unwanted information from the command name
                commandName = commandName.Replace("Zone.", "");
            }
            else if (command.NameLevel1.Contains("Zone"))
            {
                int zoneIndex = int.Parse(command.NameLevel1.Replace("Zone", ""));
                zone = OM.Host.ZoneHandler.Zones[zoneIndex];

                // Strip away unwanted information from the command name
                commandName = commandName.Replace(String.Format("{0}.", command.NameLevel1), "");
            }

            // Cancel if no media provider is active
            if (zone == null)
                return null;

            result = true;

            try
            {
                switch (commandName)
                {
                    case "Play":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_Start(zone, param);
                        }

                    case "Stop":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_Stop(zone, param);
                        }

                    case "Pause":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_Pause(zone, param);
                        }

                    case "Next":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_Next(zone, param);
                        }

                    case "Previous":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_Previous(zone, param);
                        }

                    case "SeekForward":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_SeekForward(zone, param);
                        }

                    case "SeekBackward":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.Playback_SeekBackward(zone, param);
                        }

                    case "Shuffle.Enable":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetShuffle(zone, true);
                        }

                    case "Shuffle.Disable":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetShuffle(zone, false);
                        }

                    case "Shuffle.Toggle":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetShuffle(zone, !currentMediaProvider.GetShuffle(zone));
                        }

                    case "Repeat.Enable":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetRepeat(zone, true);
                        }

                    case "Repeat.Disable":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetRepeat(zone, false);
                        }

                    case "Repeat.Toggle":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            return currentMediaProvider.SetRepeat(zone, !currentMediaProvider.GetRepeat(zone));
                        }

                    case "Playlist.Set":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];
                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is Playlist)
                                {   // Set playlist
                                    currentMediaProvider.SetPlaylist(zone, param[0] as Playlist);
                                    return "";
                                }
                                else
                                {
                                    return "Invalid datatype for playlist";
                                }
                            }
                            else
                            {
                                return "Minimum one parameter is required";
                            }
                        }

                    case "MediaProvider.Activate":
                        {
                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is string)
                                {   // Activate media provider
                                    var provider = GetMediaProvider((string)param[0]);
                                    if (provider == null)
                                        return "MediaProvider not found";

                                    if (_CurrentMediaProviders.ContainsKey(zone))
                                    {
                                        // Cancel request if provider is already active
                                        if (_CurrentMediaProviders[zone] == provider)
                                            return "";

                                        // Deactivate current provider
                                        DeactivateMediaProvider(_CurrentMediaProviders[zone], zone);
                                    }

                                    return ActivateMediaProvider(provider, zone);
                                }
                                else
                                {
                                    return "Invalid MediaProvider name (Invalid datatype)";
                                }
                            }
                            else
                            {
                                return "Minimum one parameter is required";
                            }
                        }

                    case "MediaProvider.Deactivate":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "Unable to deactivate provider. No active provider for zone";

                            return DeactivateMediaProvider(_CurrentMediaProviders[zone], zone);
                        }

                    case "MediaSource.Activate":
                        {
                            if (!_CurrentMediaProviders.ContainsKey(zone))
                                return "No mediaProvider active";
                            IMediaProvider currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone];

                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is string)
                                {   // Activate media source

                                    var mediaSource = currentMediaProvider.GetMediaSources().Find(x => x.Name == (string)param[0]);
                                    if (mediaSource == null)
                                        return "Invalid MediaSource name";

                                    var mediaSource_Result = currentMediaProvider.ActivateMediaSource(zone, (string)param[0]);
                                    MediaSource_RefreshDataSources(zone);
                                    CreateDataSourceFromDictionary(mediaSource.AdditionalData);
                                    CreateCommandFromDictionary(mediaSource.AdditionalCommands);
                                    return mediaSource_Result;
                                }
                                else
                                {
                                    return "Invalid MediaSource name (Invalid datatype)";
                                }
                            }
                            else
                            {
                                return "Minimum one parameter is required";
                            }
                        }

                    default:
                        {
                            // Check if this is a generic command
                            if (commandName.Contains("MediaSource.Command."))
                            {
                                var currentMediaProvider = (IMediaProvider)_CurrentMediaProviders[zone]; 
                                var mediaSource = currentMediaProvider.GetMediaSource(zone);
                                if (mediaSource == null)
                                    return null;

                                // Mask out common part to get the individual part
                                commandName = commandName.Replace("MediaSource.Command.", "");
                                if (mediaSource.AdditionalCommands.ContainsKey(commandName))
                                {
                                    try
                                    {
                                        var commandDelegate = mediaSource.AdditionalCommands[commandName];
                                        if (commandDelegate != null)
                                            return commandDelegate(zone, param);
                                        else
                                            return "Command not supported";
                                    }
                                    catch (NotImplementedException ex)
                                    {
                                        OM.Host.DebugMsg(String.Format("MediaProviderHandler: Command:{0} not implemented in MediaSource:{1} for provider:{2}", command.FullName, mediaSource, _CurrentMediaProviders[zone]), ex); 
                                        return "Command not supported";
                                    }
                                    catch (Exception ex)
                                    {
                                        OM.Host.DebugMsg(String.Format("MediaProviderHandler: Exception while executing command:{0} in MediaSource:{1} for provider:{2}", command.FullName, mediaSource, _CurrentMediaProviders[zone]), ex);
                                        return "Command not supported";
                                    }
                                }
                                else
                                    return null;
                            }
                        }
                        break;
                }
            }
            catch (NotImplementedException ex)
            {
                OM.Host.DebugMsg(String.Format("MediaProviderHandler: Command:{0} not implemented in provider:{1}", command.FullName, _CurrentMediaProviders[zone]), ex); 
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg(String.Format("MediaProviderHandler: Exception while executing command:{0} in provider:{1}", command.FullName, _CurrentMediaProviders[zone]), ex);
            }

            return null;
        }


/*
        void Commands_Register()
        {
            new Command(this, this.pluginName, String.Format("Zone{0}", i), "PlayURL", CommandExecutor, 0, false, "")));
        }
*/


        #endregion

    }
}
