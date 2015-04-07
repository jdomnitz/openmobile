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
using System.Text;
using System.Collections.Generic;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Media;
using OpenMobile.helperFunctions;
using OpenMobile.Controls;
using System.Drawing;
using OpenMobile.Graphics;
using System.Linq;
using System.IO;
using OMRadio_MonkeyBoard.Hardware.Monkeyboard;
using System.Threading;

namespace OMRadio_MonkeyBoard
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows)]
    public sealed class OMRadio_MonkeyBoard : MediaProviderBase
    {
        private enum RadioCommands
        {
            None,
            PowerOn,
            Stop,
            SetMode_DAB,
            SetMode_FM,
            ScanDABChannels,
            TuneChannel,
            NextStream,
            PreviousStream,
            Mute,
            Unmute
        }

        private enum ListSource
        {
            Live = 0,
            Preset = 1
        }

        private string _AudioInputDeviceName;
        private Dictionary<Zone, AudioRoute> _AudioRoutes = new Dictionary<Zone, AudioRoute>();
        private int _RadioVolume = 50;
        private string _RadioComPort = "COM1";
        private Thread _RadioThread;
        private bool _RadioThread_Enable = true;
        private RadioCommands _RadioCommand = RadioCommands.None;
        
        private HW_Monkeyboard.PlayStatus _Radio_PlayStatus;
        private HW_Monkeyboard.RADIO_TUNE_BAND _Radio_CurrentBand;
        private string _Radio_ProgramText;
        private int _Radio_SignalStrength;
        private HW_Monkeyboard.StereoStatus _Radio_StereoStatus;
        private int _Radio_Channel;
        private HW_Monkeyboard.ProgramType _Radio_ProgramType;
        private string _Radio_ProgramNameShort;
        private string _Radio_ProgramNameLong;
        private int _Radio_DAB_LastChannel;
        private int _Radio_FM_LastChannel;
        private object _Radio_CommandData;
            
        private List<Zone> _ActiveZonesForRadio = new List<Zone>();

        private List<MediaSource> _MediaSources = new List<MediaSource>();
        private MediaSource_TunedContent _MediaSource_DAB;
        private MediaSource_TunedContent _MediaSource_FM;
        private MediaSource_TunedContent _Radio_MediaSource;
        private Playlist _Radio_DAB_Channels_Preset;
        private Playlist _Radio_DAB_Channels_Live = new Playlist("Channels.DAB.Live");
        private Playlist _Radio_FM_Channels_Preset;
        private Playlist _Radio_FM_Channels_Live = new Playlist("Channels.FM.Live");
        private mediaInfo _Radio_CurrentMedia = new mediaInfo();
        private ListSource _Radio_DAB_ListSource = ListSource.Live;
        private ListSource _Radio_FM_ListSource = ListSource.Preset;

        #region Settings

        protected override void Settings()
        {
            var ports = OpenMobile.helperFunctions.SerialAccess.GetPortNames();
            string defaultPort = "";
            if (ports != null && ports.Count() > 0)
                defaultPort = ports[0];
            base.Setting_Add(Setting.TextList<string>(String.Format("{0}.ComPort", this.pluginName), String.Empty, "Select comport for MonkeyBoard", StoredData.Get(this, String.Format("{0}.ComPort", this.pluginName)), ports), defaultPort);
            base.Setting_Add(Setting.TextList<AudioDevice>("AudioInputDevice", "Select input device", "Audio input device", StoredData.Get(this, "AudioInputDevice"), OM.Host.AudioDeviceHandler.InputDevices));
        }

        private void Settings_MapVariables()
        {
            _RadioComPort = StoredData.Get(this, String.Format("{0}.ComPort", this.pluginName));
            _AudioInputDeviceName = StoredData.Get(this, "AudioInputDevice");
        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, String.Format("{0}.ComPort", this.pluginName), "COM1");
            StoredData.SetDefaultValue(this, "AudioInputDevice", "");

            Settings_MapVariables();
        }

        protected override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);
            Settings_MapVariables();
        }

        #endregion

        #region Constructor / Plugin config

        public OMRadio_MonkeyBoard()
            : base("OMRadio_MonkeyBoard", OM.Host.getSkinImage("Radio"), 1f, "OM Radio interface for MonkeyBoard DAB/RDS", "OM Dev team/Borte", "")
        {
        }

        #endregion

        public override eLoadStatus initialize(IPluginHost host)
        {
            if (!Configuration.RunningOnWindows)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // Do async startup so we don't block the main OM thread
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
                Settings_SetDefaultValues();

                // Load playlists from DB
                _Radio_DAB_Channels_Preset = new Playlist(String.Format("{0}.Channels.DAB.Preset", this.pluginName), String.Format("DAB Channels presets ({0})", this.pluginName));
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Starting to loading items for playlist {0} from DB", _Radio_DAB_Channels_Preset.Name));
                _Radio_DAB_Channels_Preset.Load();
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Loaded {1} items for playlist {0} from DB", _Radio_DAB_Channels_Preset.Name, _Radio_DAB_Channels_Preset.Count));

                _Radio_FM_Channels_Preset = new Playlist(String.Format("{0}.Channels.FM.Preset", this.pluginName), String.Format("FM Channels presets ({0})", this.pluginName));
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Starting to loading items for playlist {0} from DB", _Radio_FM_Channels_Preset.Name));
                _Radio_FM_Channels_Preset.Load();
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Loaded {1} items for playlist {0} from DB", _Radio_FM_Channels_Preset.Name, _Radio_FM_Channels_Preset.Count));

                // Check for updated cover art for items in the FM preset list
                foreach (var media in _Radio_FM_Channels_Preset.Items)
                    TryUpdateCoverArt(media);

                // Initialize media sources
                _MediaSource_DAB = new MediaSource_TunedContent(this, "DAB", "DAB Radio", OM.Host.getSkinImage("Icons|Icon-RadioDAB"));
                _MediaSource_DAB.ChannelsPreset = _Radio_DAB_Channels_Preset;
                _MediaSource_DAB.ChannelsLive = _Radio_DAB_Channels_Live;
                _MediaSource_DAB.OnCommand_PresetSet += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_PresetSet);
                _MediaSource_DAB.OnCommand_SetListSource += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_SetListSource);
                _MediaSource_DAB.OnCommand_DirectTune += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_DirectTune);
                _MediaSource_DAB.OnCommand_PresetRemove += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_PresetRemove);
                _MediaSource_DAB.OnCommand_PresetRename += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_PresetRename);
                _MediaSource_DAB.OnCommand_SearchChannels += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_SearchChannels);
                _MediaSource_DAB.OnCommand_PresetSetCoverArt += new MediaSourceCommandDelegate(_MediaSource_DAB_OnCommand_PresetSetCoverArt);
                _MediaSources.Add(_MediaSource_DAB);

                _MediaSource_FM = new MediaSource_TunedContent(this, "FM", "FM RDS Radio", OM.Host.getSkinImage("Icons|Icon-RadioFM"));
                _MediaSource_FM.ChannelsPreset = _Radio_FM_Channels_Preset;
                _MediaSource_FM.ChannelsLive = _Radio_FM_Channels_Live;
                _MediaSource_FM.OnCommand_PresetSet += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_PresetSet);
                _MediaSource_FM.OnCommand_SetListSource += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_SetListSource);
                _MediaSource_FM.OnCommand_DirectTune += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_DirectTune);
                _MediaSource_FM.OnCommand_PresetRemove += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_PresetRemove);
                _MediaSource_FM.OnCommand_PresetRename += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_PresetRename);
                _MediaSource_FM.OnCommand_SearchChannels += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_SearchChannels);
                _MediaSource_FM.OnCommand_PresetSetCoverArt += new MediaSourceCommandDelegate(_MediaSource_FM_OnCommand_PresetSetCoverArt);
                _MediaSources.Add(_MediaSource_FM);

                // Set default settings data
                StoredData.SetDefaultValue(this, "_Radio_DAB_LastChannel", 0);
                StoredData.SetDefaultValue(this, "_Radio_FM_LastChannel", 100700);
                StoredData.SetDefaultValue(this, "_Radio_MediaSource", "DAB");

                // Load settings data
                _Radio_DAB_LastChannel = StoredData.GetInt(this, "_Radio_DAB_LastChannel");
                _Radio_FM_LastChannel = StoredData.GetInt(this, "_Radio_FM_LastChannel");
                if (_Radio_FM_LastChannel == 0)
                    _Radio_FM_LastChannel = 100700;

                var mediaSourceName = StoredData.Get(this, "_Radio_MediaSource");
                if (!String.IsNullOrWhiteSpace(mediaSourceName))
                    _Radio_MediaSource = (MediaSource_TunedContent)_MediaSources.Find(x => x.Name == mediaSourceName);
            });         
            return eLoadStatus.LoadSuccessful;
        }


        #region DAB Media source

        object _MediaSource_DAB_OnCommand_SearchChannels(Zone zone, object[] param)
        {
            // Search dab channels
            _RadioCommand = RadioCommands.ScanDABChannels;
            return "";
        }

        object _MediaSource_DAB_OnCommand_PresetRename(Zone zone, object[] param)
        {
            return "Rename is not supported for DAB channels";
        }

        object _MediaSource_DAB_OnCommand_PresetRemove(Zone zone, object[] param)
        {
            mediaInfo channelItem = null;
            if (Params.IsParamsValid(param, 1))
            {   // Remove preset using specific channel ID
                if ((param[0] is string) | (param[0] is int))
                {   // Channel ID as string or int
                    var channelID = 0;
                    if (param[0] is string)
                    {
                        var stringValue = param[0] as string;
                        if (stringValue != null)
                            int.TryParse(stringValue.Replace("DAB:", ""), out channelID);
                    }
                    else if (param[0] is int)
                    {
                        channelID = (int)param[0];
                    }

                    channelItem = _Radio_DAB_Channels_Preset.Items.Where(x => x.Location.Replace("DAB:", "").Equals(channelID.ToString())).FirstOrDefault();
                }
                else if (param[0] is mediaInfo)
                {
                    // Find channel in presets channels list
                    mediaInfo media = param[0] as mediaInfo;
                    channelItem = _Radio_DAB_Channels_Preset.Items.Where(x => x.Name.Equals(media.Name)).FirstOrDefault();
                }
            }
            else
            {   // Missing parameter
                return "Missing parameter";
            }

            if (channelItem != null)
            {
                _Radio_DAB_Channels_Preset.Remove(channelItem);

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
            else
            {
                return "Unable to remove preset";
            }

        }

        object _MediaSource_DAB_OnCommand_DirectTune(Zone zone, object[] param)
        {
            if (Params.IsParamsValid(param, 1))
            {   // Add preset using specific channel ID
                if ((param[0] is string) | (param[0] is int))
                {   // Channel ID as string or int
                    var channelID = 0;
                    if (param[0] is string)
                    {
                        var stringValue = param[0] as string;
                        if (stringValue != null)
                            int.TryParse(stringValue.Replace("DAB:", ""), out channelID);
                    }
                    else if (param[0] is int)
                    {
                        channelID = (int)param[0];
                    }
                    else
                    {
                        return "Invalid parameter";
                    }

                    _RadioCommand = RadioCommands.TuneChannel;
                    _Radio_CommandData = channelID;
                    return "";
                }
                else if (param[0] is mediaInfo)
                {
                    // Tune to media info
                    mediaInfo media = param[0] as mediaInfo;
                    return Radio_TuneMedia(media);
                }
                else
                {
                    return "Invalid parameter";
                }
            }
            else
            {
                return "Missing parameter";
            }
        }

        object _MediaSource_DAB_OnCommand_SetListSource(Zone zone, object[] param)
        {
            // Parameter should be requested list source
            if (Params.IsParamsValid(param, 1))
            {
                if (param[0] is int)
                {   // Parameter is list source
                    var listsource = (int)param[0];

                    switch (listsource)
                    {
                        case 0:
                            _Radio_DAB_ListSource = ListSource.Live;
                            break;

                        case 1:
                            _Radio_DAB_ListSource = ListSource.Preset;
                            break;

                        default:
                            return "Invalid list source";
                    }

                    // Also update data field for list source
                    _MediaSource_DAB.ListSource = (int)_Radio_DAB_ListSource;

                    // Also refresh current playlist source
                    foreach (var zoneLocal in _ActiveZonesForRadio)
                        base.MediaProviderData_RefreshPlaylist(zoneLocal);

                    return "";
                }
                else
                {
                    return "Invalid parameters";
                }
            }
            else
            {   // Toggle list sources
                switch (_Radio_DAB_ListSource)
                {
                    case ListSource.Live:
                        _Radio_DAB_ListSource = ListSource.Preset;
                        break;
                    case ListSource.Preset:
                        _Radio_DAB_ListSource = ListSource.Live;
                        break;
                    default:
                        break;
                }

                // Also update data field for list source
                _MediaSource_DAB.ListSource = (int)_Radio_DAB_ListSource;

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
        }

        object _MediaSource_DAB_OnCommand_PresetSet(Zone zone, object[] param)
        {
            mediaInfo channelItem = null;
            if (Params.IsParamsValid(param, 1))
            {   // Add preset using specific channel ID
                if ((param[0] is string) | (param[0] is int))
                {   // Channel ID as string or int
                    var channelID = 0;
                    if (param[0] is string)
                    {
                        var stringValue = param[0] as string;
                        if (stringValue != null)
                            int.TryParse(stringValue.Replace("DAB:", ""), out channelID);
                    }
                    else if (param[0] is int)
                    {
                        channelID = (int)param[0];
                    }

                    channelItem = _Radio_DAB_Channels_Live.Items.Where(x => x.Location.Replace("DAB:", "").Equals(channelID.ToString())).FirstOrDefault();
                }
                else if (param[0] is mediaInfo)
                {
                    // Find current channel in live channels list
                    mediaInfo media = param[0] as mediaInfo;
                    channelItem = _Radio_DAB_Channels_Live.Items.Where(x => x.Name.Equals(media.Name)).FirstOrDefault();
                }

                if (Params.IsParamsValid(param, 2) && param[1] is string)
                {   // Use specific preset name. This is not supported for DAB Channels

                }
            }
            else
            {   // Add current channel as preset
                // Find current channel in live channels list
                channelItem = _Radio_DAB_Channels_Live.Items.Where(x => x.Location.Replace("DAB:", "").Equals(_Radio_Channel.ToString())).FirstOrDefault();
            }

            if (channelItem != null)
            {
                _Radio_DAB_Channels_Preset.AddDistinct(channelItem);

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
            else
            {
                return "Unable to add preset";
            }

        }

        object _MediaSource_DAB_OnCommand_PresetSetCoverArt(Zone zone, object[] param)
        {
            if (Params.IsParamsValid(param, 2))
            {   // Set preset cover art
                if (!(param[0] is mediaInfo) || (!(param[1] is OImage) & !(param[1] is imageItem)))
                    return "Invalid parameters";

                mediaInfo media = param[0] as mediaInfo;

                OImage image = null;
                if (param[1] is OImage)
                    image = param[1] as OImage;
                else if (param[1] is imageItem)
                    image = ((imageItem)param[1]).image;

                if (media == null || image == null)
                    return "Invalid parameters";

                var presetMedia = _Radio_DAB_Channels_Preset.Items.Where(x => x.Location == media.Location).FirstOrDefault();
                if (presetMedia == null)
                    return "Unable to set cover art";

                presetMedia.coverArt = image;

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
            else
                return "Missing required parameters";
        }

        #endregion

        #region FM Media source

        object _MediaSource_FM_OnCommand_SearchChannels(Zone zone, object[] param)
        {
            return "";
        }

        object _MediaSource_FM_OnCommand_PresetRename(Zone zone, object[] param)
        {
            if (Params.IsParamsValid(param, 2))
            {   // Remove preset using specific channel ID
                if (!(param[0] is mediaInfo) || !(param[1] is string))
                    return "Invalid parameters";

                mediaInfo media = param[0] as mediaInfo;
                string name = param[1] as string;
                if (media == null || String.IsNullOrWhiteSpace(name))
                    return "Invalid parameters";

                var presetMedia = _Radio_FM_Channels_Preset.Items.Where(x => x.Location == media.Location).FirstOrDefault();
                if (presetMedia == null)
                    return "Unable to rename preset";

                presetMedia.Name = name;

                return "";                
            }
            else
                return "Missing required parameters";
        }

        object _MediaSource_FM_OnCommand_PresetRemove(Zone zone, object[] param)
        {
            List<mediaInfo> channelItems = null;
            if (Params.IsParamsValid(param, 1))
            {   // Remove preset using specific channel ID
                if ((param[0] is string) | (param[0] is int))
                {   // Channel ID as string or int
                    var channelID = 0;
                    if (param[0] is string)
                    {
                        var stringValue = param[0] as string;
                        if (stringValue != null)
                            int.TryParse(stringValue.Replace("FM:", ""), out channelID);
                    }
                    else if (param[0] is int)
                    {
                        channelID = (int)param[0];
                    }

                    channelItems = _Radio_FM_Channels_Preset.Items.Where(x => x.Location.Replace("FM:", "").Equals(channelID.ToString())).ToList();
                }
                else if (param[0] is mediaInfo)
                {
                    // Find channel in presets channels list
                    mediaInfo media = param[0] as mediaInfo;
                    channelItems = _Radio_FM_Channels_Preset.Items.Where(x => x.Location.Equals(media.Location)).ToList();
                }
            }
            else
            {   // Missing parameter
                return "Missing parameter";
            }

            if (channelItems != null && channelItems.Count() > 0)
            {
                foreach (var channelItem in channelItems)
                    _Radio_FM_Channels_Preset.Remove(channelItem);

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
            else
            {
                return "Unable to remove preset";
            }

        }

        object _MediaSource_FM_OnCommand_DirectTune(Zone zone, object[] param)
        {
            if (Params.IsParamsValid(param, 1))
            {   // Add preset using specific channel ID
                if ((param[0] is string) | (param[0] is int))
                {   // Channel ID as string or int (this is MHz directly)
                    double channelID = 0;
                    if (param[0] is string)
                    {
                        var stringValue = param[0] as string;
                        if (stringValue != null)
                            double.TryParse(stringValue.Replace("FM:", "").Replace(",","."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out channelID);

                    }
                    else if (param[0] is int)
                    {
                        channelID = (int)param[0];
                    }
                    else
                    {
                        return "Invalid parameter";
                    }

                    if (channelID < 110)
                        channelID *= 1000.0;

                    _RadioCommand = RadioCommands.TuneChannel;
                    _Radio_CommandData = (int)channelID;
                    return "";
                }
                else if (param[0] is mediaInfo)
                {
                    // Tune to media info
                    mediaInfo media = param[0] as mediaInfo;
                    return Radio_TuneMedia(media);
                }
                else
                {
                    return "Invalid parameter";
                }
            }
            else
            {
                return "Missing parameter";
            }
        }

        object _MediaSource_FM_OnCommand_SetListSource(Zone zone, object[] param)
        {
            // Parameter should be requested list source
            if (Params.IsParamsValid(param, 1))
            {
                if (param[0] is int)
                {   // Parameter is list source
                    var listsource = (int)param[0];

                    if (_Radio_MediaSource == _MediaSource_FM)
                    {
                        switch (listsource)
                        {
                            case 0:
                                _Radio_FM_ListSource = ListSource.Live;
                                break;

                            case 1:
                                _Radio_FM_ListSource = ListSource.Preset;
                                break;

                            default:
                                return "Invalid list source";
                        }
                    }

                    // Also update data field for list source
                    _MediaSource_FM.ListSource = (int)_Radio_FM_ListSource;

                    // Also refresh current playlist source
                    foreach (var zoneLocal in _ActiveZonesForRadio)
                        base.MediaProviderData_RefreshPlaylist(zoneLocal);

                    return "";
                }
                else
                {
                    return "Invalid parameters";
                }
            }
            else
            {   // Toggle list sources
                if (_Radio_MediaSource == _MediaSource_FM)
                {
                    switch (_Radio_FM_ListSource)
                    {
                        case ListSource.Live:
                            _Radio_FM_ListSource = ListSource.Preset;
                            break;
                        case ListSource.Preset:
                            _Radio_FM_ListSource = ListSource.Live;
                            break;
                        default:
                            break;
                    }
                }

                // Also update data field for list source
                _MediaSource_FM.ListSource = (int)_Radio_FM_ListSource;

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
        }

        object _MediaSource_FM_OnCommand_PresetSet(Zone zone, object[] param)
        {
            // Add current channel as preset
            mediaInfo media = (mediaInfo)_Radio_CurrentMedia.Clone();

            // Try to set cover art
            TryUpdateCoverArt(media);
            
            var result = _Radio_FM_Channels_Preset.AddDistinct(media);

            // Also refresh current playlist source
            foreach (var zoneLocal in _ActiveZonesForRadio)
                base.MediaProviderData_RefreshPlaylist(zoneLocal);

            if (result)
                return "";
            else
                return "Preset already exists";
        }

        object _MediaSource_FM_OnCommand_PresetSetCoverArt(Zone zone, object[] param)
        {
            if (Params.IsParamsValid(param, 2))
            {   // Set preset cover art
                if (!(param[0] is mediaInfo) || (!(param[1] is OImage) & !(param[1] is imageItem)))
                    return "Invalid parameters";

                mediaInfo media = param[0] as mediaInfo;

                OImage image = null;
                if (param[1] is OImage)
                    image = param[1] as OImage;
                else if (param[1] is imageItem)
                    image = ((imageItem)param[1]).image;

                if (media == null || image == null)
                    return "Invalid parameters";

                var presetMedia = _Radio_FM_Channels_Preset.Items.Where(x => x.Location == media.Location).FirstOrDefault();
                if (presetMedia == null)
                    return "Unable to set cover art";

                presetMedia.coverArt = image;

                // Also refresh current playlist source
                foreach (var zoneLocal in _ActiveZonesForRadio)
                    base.MediaProviderData_RefreshPlaylist(zoneLocal);

                return "";
            }
            else
                return "Missing required parameters";
        }

        #endregion

        #region Dispose

        private bool _Disposing = false;
        public override void Dispose()
        {
            _Disposing = true;
            DeactivateRadio(null);
            base.Dispose();
        }

        ~OMRadio_MonkeyBoard()
        {
        }

        #endregion

        #region Radio functions

        private void RadioThread_Start()
        {
            if (_RadioThread == null)
            {
                _RadioThread = new Thread(RadioThread_Run);
                _RadioThread.IsBackground = true;
                _RadioThread.Name = String.Format("{0}_RadioThread", this.pluginName);
            }
            _RadioThread_Enable = true;
            _RadioThread.Start();
        }

        private void RadioThread_Stop()
        {
            _RadioThread_Enable = false;
            if (!_RadioThread.Join(1000))
            {
                // Abort thread
                _RadioThread.Abort();
            }
            _RadioThread = null;
        }

        private void RadioThread_Run()
        {
            // Initialize startup values
            foreach (var zone in _ActiveZonesForRadio)
                base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Stopped);

            // Set default data
            _RadioCommand = RadioCommands.PowerOn;
            _Radio_CurrentMedia.coverArt = OM.Host.getSkinImage("Icons|Icon-Glass-Radio").image;

            // Initialize temporary variables
            HW_Monkeyboard.PlayStatus playStatus_Stored = HW_Monkeyboard.PlayStatus.Stop;
            string programTextTemp = new string(' ', 400);
            string programNameTemp = new string(' ', 400);
            int bitErrorTemp = 0;
            mediaInfo _CurrentMedia_Temp = new mediaInfo();

            while (_RadioThread_Enable)
            {
                try
                {
                    // Read radio status
                    _Radio_PlayStatus = HW_Monkeyboard.GetPlayStatus();
                    #region Report radio status back to media handler

                    if (_Radio_PlayStatus != playStatus_Stored)
                    {   // Radio playback state changed, report status
                        playStatus_Stored = _Radio_PlayStatus;

                        PlaybackState state = PlaybackState.Busy;
                        switch (_Radio_PlayStatus)
                        {
                            case HW_Monkeyboard.PlayStatus.Playing:
                                state = PlaybackState.Playing;
                                break;
                            case HW_Monkeyboard.PlayStatus.Searching:
                            case HW_Monkeyboard.PlayStatus.Tuning:
                            case HW_Monkeyboard.PlayStatus.Sorting:
                            case HW_Monkeyboard.PlayStatus.Reconfiguring:
                                state = PlaybackState.Busy;
                                break;
                            case HW_Monkeyboard.PlayStatus.Stop:
                                state = PlaybackState.Stopped;
                                break;
                            default:
                                break;
                        }

                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_ReportState_Playback(zone, state);
                    }

                    #endregion

                    #region Read radio data

                    // Read current radio band
                    _Radio_CurrentBand = HW_Monkeyboard.GetPlayMode();
                    var mediaSourceTemp = _Radio_MediaSource;
                    switch (_Radio_CurrentBand)
                    {
                        case HW_Monkeyboard.RADIO_TUNE_BAND.DAB:
                            mediaSourceTemp = _MediaSource_DAB;
                            break;
                        case HW_Monkeyboard.RADIO_TUNE_BAND.FM:
                            mediaSourceTemp = _MediaSource_FM;
                            break;
                    }
                    // Is this a new media source?
                    if (mediaSourceTemp != _Radio_MediaSource)
                    {
                        _Radio_MediaSource = mediaSourceTemp;
                        // Yes, inform media handler
                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_ReportMediaSource(zone, _Radio_MediaSource);

                        // Also refresh current playlist source
                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_RefreshPlaylist(zone);
                    }

                    // Read radio data
                    _Radio_SignalStrength = HW_Monkeyboard.GetSignalStrength(ref bitErrorTemp);
                    _Radio_StereoStatus = HW_Monkeyboard.GetStereo();
                    _Radio_Channel = HW_Monkeyboard.GetPlayIndex();
                    _Radio_ProgramType = HW_Monkeyboard.GetProgramType(_Radio_CurrentBand, _Radio_Channel);

                    // Read program name (if any)
                    string stringTemp = new string(' ', 400);
                    if (HW_Monkeyboard.GetProgramName(_Radio_CurrentBand, _Radio_Channel, HW_Monkeyboard.DABNameMode.Short, stringTemp))
                        _Radio_ProgramNameShort = stringTemp.Trim();
                    else
                        _Radio_ProgramNameShort = "";
                    stringTemp = new string(' ', 400);
                    if (HW_Monkeyboard.GetProgramName(_Radio_CurrentBand, _Radio_Channel, HW_Monkeyboard.DABNameMode.Long, stringTemp))
                        _Radio_ProgramNameLong = stringTemp.Trim();
                    else
                        _Radio_ProgramNameLong = "";

                    // Read program text (if any)
                    stringTemp = new string(' ', 400);
                    if (HW_Monkeyboard.GetProgramText(stringTemp) == 0)
                        _Radio_ProgramText = stringTemp.Trim().Replace("\r\n", "      ");

                    #endregion

                    #region Read data into current media source

                    // Report data into current media source
                    _Radio_MediaSource.ChannelNameLong = _Radio_ProgramNameLong;
                    _Radio_MediaSource.ChannelNameShort = _Radio_ProgramNameShort;
                    _Radio_MediaSource.ProgramText = _Radio_ProgramText;
                    _Radio_MediaSource.SignalStrength = _Radio_SignalStrength;
                    _Radio_MediaSource.Stereo = _Radio_StereoStatus == HW_Monkeyboard.StereoStatus.SingleChannel ? false : true;                    
                    if (_Radio_ProgramType >= 0)
                        _Radio_MediaSource.ProgramType = _Radio_ProgramType.ToString();
                    else
                        _Radio_MediaSource.ProgramType = "";
                    _Radio_MediaSource.DeviceStatus = _Radio_PlayStatus.ToString();

                    _Radio_CurrentMedia.Name = _Radio_ProgramNameLong;
                    if (String.IsNullOrWhiteSpace(_Radio_ProgramNameLong))
                        _Radio_CurrentMedia.Name = _Radio_ProgramNameShort;

                    #endregion

                    // Read data into current media info
                    _Radio_CurrentMedia.Artist = _Radio_ProgramText;
                    if ((int)_Radio_ProgramType > 0)
                        _Radio_CurrentMedia.Genre = _Radio_ProgramType.ToString();
                    else
                        _Radio_CurrentMedia.Genre = "";

                    // Get DAB Specific data
                    if (_Radio_MediaSource == _MediaSource_DAB)
                    {
                        #region DAB Specific data

                        // Save current station
                        //_Radio_DAB_LastChannel = _Radio_Channel;

                        //byte serviceComponentID = 0;
                        //Int32 serviceID = 0;
                        //Int16 ensembleID = 0;
                        //HW_Monkeyboard.GetProgramInfo(_Radio_Channel, ref serviceComponentID, ref serviceID, ref ensembleID);
                        //_MediaSource_DAB.AdditionalData["ProgramInfo.ServiceComponentID"] = serviceComponentID;
                        //_MediaSource_DAB.AdditionalData["ProgramInfo.ServiceID"] = serviceID;
                        //_MediaSource_DAB.AdditionalData["ProgramInfo.EnsembleID"] = ensembleID;

                        //_MediaSource_DAB.AdditionalData["DABChannelType"] = HW_Monkeyboard.GetApplicationType(_Radio_Channel).ToString();
                        
                        //stringTemp = new string(' ', 400);
                        //if (HW_Monkeyboard.GetEnsembleName(_Radio_Channel, HW_Monkeyboard.DABNameMode.Long, stringTemp))
                        //    _MediaSource_DAB.AdditionalData["EnsembleName.Long"] = stringTemp.Trim();
                        //stringTemp = new string(' ', 400);
                        //if (HW_Monkeyboard.GetEnsembleName(_Radio_Channel, HW_Monkeyboard.DABNameMode.Short, stringTemp))
                        //    _MediaSource_DAB.AdditionalData["EnsembleName.Short"] = stringTemp.Trim();

                        //_MediaSource_DAB.AdditionalData["DataRate"] = HW_Monkeyboard.GetDataRate();
                        //_MediaSource_DAB.AdditionalData["SignalQuality"] = HW_Monkeyboard.GetDABSignalQuality();

                        _MediaSource_DAB.ListSource = (int)_Radio_DAB_ListSource;
                        _MediaSource_DAB.SignalBitRate = HW_Monkeyboard.GetDataRate().ToString();
                        _MediaSource_DAB.HD = true;
                        _MediaSource_DAB.ChannelID = "";

                        // Set media texts
                        string mediaText1 = "";
                        string mediaText2 = "";

                        if (!String.IsNullOrWhiteSpace(_Radio_ProgramNameLong))
                            mediaText1 = _Radio_ProgramNameLong;
                        else if (!String.IsNullOrWhiteSpace(_Radio_ProgramNameShort))
                            mediaText1 = _Radio_ProgramNameShort;
                        else
                            mediaText1 = String.Format("DAB: {0}", _Radio_Channel);

                        if (!String.IsNullOrWhiteSpace(_Radio_ProgramText))
                            mediaText2 = _Radio_ProgramText;

                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_ReportMediaText(zone, mediaText1, mediaText2);

                        if (_Radio_DAB_Channels_Live != null && _Radio_DAB_Channels_Live.Items != null && _Radio_DAB_Channels_Live.Items.Count > _Radio_Channel)
                            _Radio_CurrentMedia.coverArt = _Radio_DAB_Channels_Live.Items[_Radio_Channel].coverArt;

                        _Radio_CurrentMedia.Location = String.Format("DAB:{0}", _Radio_Channel);

                        #endregion
                    }

                    // FM Specific
                    else if (_Radio_MediaSource == _MediaSource_FM)
                    {
                        #region FM Specific data

                        // Save current station
                        //_Radio_FM_LastChannel = _Radio_Channel;
                        _Radio_CurrentMedia.Location = String.Format("FM:{0}", _Radio_Channel);

                        // Use channel name and cover from preset if info is preset
                        var presetMedia = _Radio_FM_Channels_Preset.Items.Where(x => x.Location == _Radio_CurrentMedia.Location).FirstOrDefault();


                        // Set media texts
                        string mediaText1 = "";
                        string mediaText2 = "";

                        if (presetMedia != null && !String.IsNullOrWhiteSpace(presetMedia.Name) && !presetMedia.Name.Contains("MHz"))
                            mediaText1 = presetMedia.Name;
                        else if (!String.IsNullOrWhiteSpace(_Radio_ProgramNameLong))
                            mediaText1 = _Radio_ProgramNameLong;
                        else if (!String.IsNullOrWhiteSpace(_Radio_ProgramNameShort))
                            mediaText1 = _Radio_ProgramNameShort;
                        else
                            mediaText1 = String.Format("FM: {0:0.00}MHz", _Radio_Channel / 1000.0);

                        if (!String.IsNullOrWhiteSpace(_Radio_ProgramText))
                            mediaText2 = _Radio_ProgramText;

                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_ReportMediaText(zone, mediaText1, mediaText2);

                        if (presetMedia != null && !String.IsNullOrWhiteSpace(presetMedia.Name) && !presetMedia.Name.Contains("MHz"))
                            _Radio_CurrentMedia.Name = presetMedia.Name;
                        else if (String.IsNullOrWhiteSpace(_Radio_ProgramNameLong))
                            _Radio_CurrentMedia.Name = String.Format("FM: {0:0.00}MHz", _Radio_Channel / 1000.0);

                        if (presetMedia != null && presetMedia.coverArt != null && _Radio_CurrentMedia.coverArt != presetMedia.coverArt)
                            _Radio_CurrentMedia.coverArt = presetMedia.coverArt;
                        else if (_Radio_CurrentMedia.coverArt == null)
                            _Radio_CurrentMedia.coverArt = OM.Host.getSkinImage("Icons|Icon-Glass-Radio").image;


                        #region Also fill new information into items stored in presets

                        var mediaItemInPresets = _Radio_FM_Channels_Preset.Items.Where(x => x.Location == _Radio_CurrentMedia.Location).FirstOrDefault();
                        if (mediaItemInPresets != null)
                        {
                            bool updated = false;
                            if (mediaItemInPresets.Name != _Radio_CurrentMedia.Name)
                            {
                                if (String.IsNullOrWhiteSpace(mediaItemInPresets.Name))
                                {
                                    mediaItemInPresets.Name = _Radio_CurrentMedia.Name;
                                    updated = true;
                                }

                                if (!_Radio_CurrentMedia.Name.Contains("MHz"))
                                {
                                    mediaItemInPresets.Name = _Radio_CurrentMedia.Name;
                                    updated = true;
                                }
                            }
                            if (mediaItemInPresets.Artist != _Radio_CurrentMedia.Artist)
                            {
                                mediaItemInPresets.Artist = _Radio_CurrentMedia.Artist;
                                updated = true;
                            }
                            if (mediaItemInPresets.Album != _Radio_CurrentMedia.Album)
                            {
                                mediaItemInPresets.Album = _Radio_CurrentMedia.Album;
                                updated = true;
                            }
                            //if (mediaItemInPresets.coverArt != _Radio_CurrentMedia.coverArt)
                            //{
                            //    mediaItemInPresets.coverArt = _Radio_CurrentMedia.coverArt;
                            //    updated = true;
                            //}
                            if (mediaItemInPresets.Genre != _Radio_CurrentMedia.Genre)
                            {
                                mediaItemInPresets.Genre = _Radio_CurrentMedia.Genre;
                                updated = true;
                            }
                            if (mediaItemInPresets.Length != _Radio_CurrentMedia.Length)
                            {
                                mediaItemInPresets.Length = _Radio_CurrentMedia.Length;
                                updated = true;
                            }
                            if (mediaItemInPresets.Location != _Radio_CurrentMedia.Location)
                            {
                                mediaItemInPresets.Location = _Radio_CurrentMedia.Location;
                                updated = true;
                            }
                            if (mediaItemInPresets.Lyrics != _Radio_CurrentMedia.Lyrics)
                            {
                                mediaItemInPresets.Lyrics = _Radio_CurrentMedia.Lyrics;
                                updated = true;
                            }
                            if (mediaItemInPresets.Rating != _Radio_CurrentMedia.Rating)
                            {
                                mediaItemInPresets.Rating = _Radio_CurrentMedia.Rating;
                                updated = true;
                            }
                            if (mediaItemInPresets.TrackNumber != _Radio_CurrentMedia.TrackNumber)
                            {
                                mediaItemInPresets.TrackNumber = _Radio_CurrentMedia.TrackNumber;
                                updated = true;
                            }
                            if (mediaItemInPresets.Type != _Radio_CurrentMedia.Type)
                            {
                                mediaItemInPresets.Type = _Radio_CurrentMedia.Type;
                                updated = true;
                            }

                            if (updated)
                            {
                                // Also refresh current playlist source
                                foreach (var zoneLocal in _ActiveZonesForRadio)
                                    base.MediaProviderData_RefreshPlaylist(zoneLocal);
                            }
                        }

                        #endregion

                        _MediaSource_FM.ListSource = (int)_Radio_FM_ListSource;
                        _MediaSource_FM.SignalBitRate = "";
                        _MediaSource_FM.HD = false;
                        _MediaSource_FM.ChannelID = String.Format("FM: {0:0.00}MHz", _Radio_Channel / 1000.0);

                        #endregion
                    }

                    // Has mediaInfo changed?
                    if (!_Radio_CurrentMedia.IsContentEqual(_CurrentMedia_Temp))
                    {   // Yes. Inform media handler
                        _CurrentMedia_Temp.ReplaceMissingInfo(_Radio_CurrentMedia);

                        foreach (var zone in _ActiveZonesForRadio)
                            base.MediaProviderData_ReportMediaInfo(zone, _Radio_CurrentMedia);
                    }

                    switch (_RadioCommand)
                    {
                        case RadioCommands.None:
                            {
                                // Set radio poll speed
                                Thread.Sleep(100);
                            }
                            break;

                        case RadioCommands.PowerOn:
                            {
                                #region PowerOn

                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaText(zone, "Tuning radio", "Please wait...");

                                // Set initial values and start radio
                                HW_Monkeyboard.SetStereoMode(HW_Monkeyboard.StereoMode.Stereo);
                                HW_Monkeyboard.SetVolumePercent(_RadioVolume);

                                // Restore radio mode
                                if (_Radio_MediaSource == _MediaSource_DAB)
                                {
                                    HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, _Radio_DAB_LastChannel);
                                    // Set default list source for this source
                                    _Radio_DAB_ListSource = ListSource.Live;
                                }
                                else if (_Radio_MediaSource == _MediaSource_FM)
                                {
                                    HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.FM, _Radio_FM_LastChannel);
                                    // Set default list source for this source
                                    _Radio_FM_ListSource = ListSource.Preset;
                                }

                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaText(zone, "Radio tuned", "");

                                ReadDABChannels_Live();

                                _RadioCommand = RadioCommands.None;

                                // Also refresh current playlist source
                                foreach (var zoneLocal in _ActiveZonesForRadio)
                                    base.MediaProviderData_RefreshPlaylist(zoneLocal);
                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaSource(zone, _Radio_MediaSource);

                                #endregion
                            }
                            break;

                        case RadioCommands.Stop:
                            {
                                #region Stop

                                HW_Monkeyboard.StopStream();
                                _RadioCommand = RadioCommands.None;

                                #endregion
                            }
                            break;

                        case RadioCommands.SetMode_DAB:
                            {
                                #region SetMode_DAB

                                HW_Monkeyboard.StopStream();

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, _Radio_DAB_LastChannel);
                                _RadioCommand = RadioCommands.None;

                                // Also refresh current playlist source
                                foreach (var zoneLocal in _ActiveZonesForRadio)
                                    base.MediaProviderData_RefreshPlaylist(zoneLocal);
                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaSource(zone, _Radio_MediaSource);

                                #endregion
                            }
                            break;

                        case RadioCommands.TuneChannel:
                            {
                                #region TuneChannel

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                HW_Monkeyboard.PlayStream(_Radio_CurrentBand, (int)_Radio_CommandData);

                                if (_Radio_MediaSource == _MediaSource_DAB)
                                    _Radio_DAB_LastChannel = (int)_Radio_CommandData;
                                else if (_Radio_MediaSource == _MediaSource_FM)
                                    _Radio_FM_LastChannel = (int)_Radio_CommandData;
                                
                                _RadioCommand = RadioCommands.None;

                                #endregion
                            }
                            break;

                        case RadioCommands.SetMode_FM:
                            {
                                #region SetMode_FM
                                
                                HW_Monkeyboard.StopStream();

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.FM, _Radio_FM_LastChannel);

                                // Set default list source for this source
                                _Radio_FM_ListSource = ListSource.Preset;

                                _RadioCommand = RadioCommands.None;

                                // Also refresh current playlist source
                                foreach (var zoneLocal in _ActiveZonesForRadio)
                                    base.MediaProviderData_RefreshPlaylist(zoneLocal);
                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaSource(zone, _Radio_MediaSource);

                                #endregion
                            }
                            break;

                        case RadioCommands.ScanDABChannels:
                            {
                                #region ScanDABChannels

                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaText(zone, "Searching DAB channels...", "Please wait.");

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                if (HW_Monkeyboard.DABAutoSearch(0, 71))
                                {   // Scan started

                                    // Wait for scan to complete 
                                    int channelCount = 0;
                                    while (HW_Monkeyboard.GetPlayStatus() == HW_Monkeyboard.PlayStatus.Searching)
                                    {
                                        Thread.Sleep(50);

                                        // Get channel count
                                        channelCount = HW_Monkeyboard.GetTotalProgram();

                                        // Report channel count
                                        if (channelCount > 0)
                                        {
                                            foreach (var zone in _ActiveZonesForRadio)
                                                base.MediaProviderData_ReportMediaText(zone, "Searching DAB channels...", String.Format("Please wait. {0} channels found", channelCount));
                                        }
                                    }

                                    // Re initialize stereo mode as this is reset when the DAB scan starts
                                    HW_Monkeyboard.SetStereoMode(HW_Monkeyboard.StereoMode.Stereo);
                                    HW_Monkeyboard.SetVolumePercent(_RadioVolume);

                                    // Scan completed, update channel list
                                    ReadDABChannels_Live();

                                    // Also update data field for list source
                                    _MediaSource_DAB.ListSource = (int)_Radio_DAB_ListSource;

                                    // Also refresh current playlist source
                                    foreach (var zoneLocal in _ActiveZonesForRadio)
                                        base.MediaProviderData_RefreshPlaylist(zoneLocal);

                                    _Radio_CommandData = 0;
                                    _RadioCommand = RadioCommands.TuneChannel;

                                    foreach (var zone in _ActiveZonesForRadio)
                                        base.MediaProviderData_ReportMediaText(zone, "DAB Channel search finished", "");

                                }
                                #endregion
                            }
                            break;

                        case RadioCommands.NextStream:
                            {
                                #region NextStream

                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaText(zone, "Searching for next channel", "Please wait...");

                                HW_Monkeyboard.NextStream();

                                _Radio_Channel = HW_Monkeyboard.GetPlayIndex();
                                if (_Radio_MediaSource == _MediaSource_DAB)
                                    _Radio_DAB_LastChannel = _Radio_Channel;
                                else if (_Radio_MediaSource == _MediaSource_FM)
                                    _Radio_FM_LastChannel = _Radio_Channel;

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                _RadioCommand = RadioCommands.None;

                                #endregion
                            }
                            break;

                        case RadioCommands.PreviousStream:
                            {
                                #region PreviousStream

                                foreach (var zone in _ActiveZonesForRadio)
                                    base.MediaProviderData_ReportMediaText(zone, "Searching for previous channel", "Please wait...");

                                HW_Monkeyboard.PrevStream();

                                _Radio_Channel = HW_Monkeyboard.GetPlayIndex();
                                if (_Radio_MediaSource == _MediaSource_DAB)
                                    _Radio_DAB_LastChannel = _Radio_Channel;
                                else if (_Radio_MediaSource == _MediaSource_FM)
                                    _Radio_FM_LastChannel = _Radio_Channel;

                                // Reset media values
                                _Radio_CurrentMedia = new mediaInfo();
                                _Radio_ProgramNameLong = "";
                                _Radio_ProgramNameShort = "";
                                _Radio_ProgramText = "";

                                _RadioCommand = RadioCommands.None;

                                #endregion
                            }
                            break;

                        case RadioCommands.Mute:
                            {
                                HW_Monkeyboard.VolumeMute();
                                _RadioCommand = RadioCommands.None;

                            }
                            break;
                        case RadioCommands.Unmute:
                            {
                                HW_Monkeyboard.SetVolumePercent(_RadioVolume);
                                _RadioCommand = RadioCommands.None;
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch
                {
                    // Slow down loop in case of error
                    Thread.Sleep(100);
                }
            }

        }

        private void ReadDABChannels_Live()
        {
            _Radio_DAB_Channels_Live.Clear();

            // Get channel count
            int channelCount = HW_Monkeyboard.GetTotalProgram();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Missing icon files for radio channels is reported in this file. Add a file with the exact filename to this folder to create channel logo files");

            // Read channel data
            for (int i = 0; i < channelCount - 1; i++)
            {
                mediaInfo media = new mediaInfo();

                // Read program name (if any)
                var stringTemp = new string(' ', 400);
                if (HW_Monkeyboard.GetProgramName(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, i, HW_Monkeyboard.DABNameMode.Short, stringTemp))
                    media.Artist = stringTemp.Trim();
                stringTemp = new string(' ', 400);
                if (HW_Monkeyboard.GetProgramName(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, i, HW_Monkeyboard.DABNameMode.Long, stringTemp))
                    media.Name = stringTemp.Trim();
                var genre = HW_Monkeyboard.GetProgramType(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, i).ToString();
                if (genre == "-1" || genre == HW_Monkeyboard.ProgramType.NotAvailable.ToString())
                    genre = "";
                media.Genre = genre;
                stringTemp = new string(' ', 400);
                if (HW_Monkeyboard.GetEnsembleName(_Radio_Channel, HW_Monkeyboard.DABNameMode.Long, stringTemp))
                    media.Album = stringTemp.Trim();
                media.Location = String.Format("DAB:{0}", i.ToString());

                //// Try to find radio channels graphics from plugin folder. 
                //// File name should match either long name or short name
                //OImage image = OM.Host.getPluginImage(this, String.Format("Graphics|{0}", media.Artist)).image;
                //if (image == null)
                //    image = OM.Host.getPluginImage(this, String.Format("Graphics|{0}", media.Name)).image;

                //// Report missing covers
                //if (image == null)
                //    sb.AppendLine(String.Format("Missing file: '{0}' or '{1}' ", media.Artist, media.Name));

                //// Set default media image 
                //if (image == null)
                //{
                //    //image = OM.Host.getSkinImage("Icons|Icon-Glass-Radio").image;
                //    image = OM.Host.getPluginImage(this, String.Format("Graphics|RadioBase", media.Name)).image.Copy();
                //    //image.RenderText(0, 0, image.Width, image.Height, media.Name, OpenMobile.Graphics.Font.Arial, eTextFormat.Normal, Alignment.CenterCenter, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.Black, FitModes.Fit);
                //    var mediaString = String.IsNullOrWhiteSpace(media.Name) ? media.Artist : media.Name;
                //    var font = OpenMobile.Graphics.Font.Arial;
                //    font.Size = 28;
                //    image.RenderText(0, 0, image.Width, image.Height, mediaString, font, eTextFormat.Bold, Alignment.CenterCenter | Alignment.WordWrap, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.Black, FitModes.FitFill);
                //}
                //media.coverArt = image;

                // Set cover art
                if (!TryUpdateCoverArt(media))
                {   // Report missing covers
                    sb.AppendLine(String.Format("Missing file: '{0}' or '{1}' ", media.Artist, media.Name));
                }

                // Save dab channel index as location
                media.Location = String.Format("DAB:{0}", i);

                media.TrackNumber = i;

                _Radio_DAB_Channels_Live.Add(media);
            }

            // Save missing covers file
            try
            {
                System.IO.File.WriteAllText(base.GetPluginFilePath(System.IO.Path.Combine("Graphics", "MissingIcons.txt")), sb.ToString());
            }
            catch { }
        }

        private string DeactivateRadio(Zone zone)
        {
            // Unregister radio as used for this zone
            if (_ActiveZonesForRadio.Contains(zone))
                _ActiveZonesForRadio.Remove(zone);

            try
            {
                // Deactivate audio route
                //var inputDevice = OM.Host.AudioDeviceHandler.GetInputDeviceByName(_AudioInputDeviceName);
                //var outputDevice = zone.AudioDevice;
                //OM.Host.AudioDeviceHandler.DeactivateRoute(inputDevice, outputDevice);

                // Deactivate audio route
                if (_AudioRoutes.ContainsKey(zone))
                {
                    OM.Host.AudioDeviceHandler.DeactivateRoute(_AudioRoutes[zone]);
                    _AudioRoutes.Remove(zone);
                }

                // Should we stop radio? We can't stop radio if any zones are still using it
                if (_ActiveZonesForRadio.Count == 0)
                {   // No zones active, stop radio
                    HW_Monkeyboard.StopStream();
                    RadioThread_Stop();
                    HW_Monkeyboard.CloseRadioPort();

                    // Save radio data
                    StoredData.Set(this, "_Radio_DAB_LastChannel", _Radio_DAB_LastChannel);
                    StoredData.Set(this, "_Radio_FM_LastChannel", _Radio_FM_LastChannel);
                    StoredData.Set(this, "_Radio_MediaSource", _Radio_MediaSource.Name);
                }
                return "";
            }
            catch
            {
                return "Communication problem with radio";
            }
        }

        private string ActivateRadio(Zone zone)
        {
            // Check for missing serial port name
            if (String.IsNullOrWhiteSpace(_RadioComPort))
            {
                OM.Host.DebugMsg(DebugMessageType.Error, "Unable to activate radio. No serial port specified!");
                return "Unable to activate radio. No serial port specified!";
            }

            // Register radio as used for this zone
            if (!_ActiveZonesForRadio.Contains(zone))
                _ActiveZonesForRadio.Add(zone);

            // Check if radio is already active
            if (_RadioThread != null)
            {
                // Just add a new audio route and exit
                if (!_AudioRoutes.ContainsKey(zone))
                    _AudioRoutes.Add(zone, OM.Host.AudioDeviceHandler.ActivateRoute(_AudioInputDeviceName, zone));
                return "";
            }

            base.MediaProviderData_ReportMediaText(zone, "Starting radio", "Please wait...");

            try
            {
                OM.Host.DebugMsg(DebugMessageType.Info, "Requesting serial port access...");
                if (OpenMobile.helperFunctions.SerialAccess.GetAccess(this))
                {
                    OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access granted...");

                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Trying to connect to radio at {0}...", _RadioComPort));
                    // Connect radio and set initial radio values
                    if (HW_Monkeyboard.OpenRadioPort(String.Format(@"\\.\{0}", _RadioComPort), false))
                    {
                        OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);

                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Successfully connected to radio at {0}!", _RadioComPort));

                        //// Activate audio route
                        //var inputDevice = OM.Host.AudioDeviceHandler.GetInputDeviceByName(_AudioInputDeviceName);
                        //var outputDevice = zone.AudioDevice;
                        //OM.Host.AudioDeviceHandler.ActivateRoute(inputDevice, outputDevice);

                        // Activate audio route
                        if (!_AudioRoutes.ContainsKey(zone))
                            _AudioRoutes.Add(zone, OM.Host.AudioDeviceHandler.ActivateRoute(_AudioInputDeviceName, zone));

                        RadioThread_Start();

                        //HW_Monkeyboard.SetStereoMode(HW_Monkeyboard.StereoMode.Stereo);
                        //HW_Monkeyboard.SetVolumePercent(_RadioVolume);
                        ////HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.FM, 100700);
                        //HW_Monkeyboard.PlayStream(HW_Monkeyboard.RADIO_TUNE_BAND.DAB, 0);
                    }
                    else
                    {
                        OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Unable to connect to radio at {0}", _RadioComPort));
                        base.MediaProviderData_ReportMediaText(zone, "Failed to start radio", "Unable to connect to radio device");
                        OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);

                        return "Unable to connect to radio";
                    }
                }
                else
                {
                    OM.Host.DebugMsg(DebugMessageType.Error, "Serial port access timed out!");
                    base.MediaProviderData_ReportMediaText(zone, "Failed to start radio", "Unable to connect to radio device");
                    return "Unable to connect to radio";
                }

                return "";
            }
            catch
            {
                return "Communication problem with radio";
            }
        }

        private string Radio_TuneMedia(mediaInfo media)
        {
            object commandData = null;
            if (_Radio_MediaSource == _MediaSource_DAB)
            {
                var channelObject = _Radio_DAB_Channels_Live.Items.Where(x => x.Name == media.Name).FirstOrDefault();
                if (channelObject != null)
                {
                    var index = _Radio_DAB_Channels_Live.Items.IndexOf(channelObject);
                    if (index >= 0)
                        commandData = index;
                    else
                        return "Unable to tune to channel";
                }
            }
            else if (_Radio_MediaSource == _MediaSource_FM)
            {
                int channelID = 0;
                if (media.Location != null)
                {
                    if (int.TryParse(media.Location.Replace("FM:", ""), out channelID))
                        commandData = channelID;
                    else
                        return "Unable to tune to channel";
                }
            }

            if (commandData != null)
            {
                _RadioCommand = RadioCommands.TuneChannel;
                _Radio_CommandData = commandData;
                return "";
            }
            else
            {
                return "Unable to tune to channel";
            }
        }

        private bool TryUpdateCoverArt(mediaInfo media)
        {
            bool coverFound = true;
            // Try to find radio channels graphics from plugin folder. 
            // File name should match either long name or short name
            OImage image = OM.Host.getPluginImage(this, String.Format("Graphics|{0}", media.Artist)).image;
            if (image == null)
                image = OM.Host.getPluginImage(this, String.Format("Graphics|{0}", media.Name)).image;

            // Set default media image 
            if (image == null)
            {
                //image = OM.Host.getSkinImage("Icons|Icon-Glass-Radio").image;
                image = OM.Host.getPluginImage(this, "Graphics|RadioBase").image.Copy();
                //image.RenderText(0, 0, image.Width, image.Height, media.Name, OpenMobile.Graphics.Font.Arial, eTextFormat.Normal, Alignment.CenterCenter, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.Black, FitModes.Fit);
                var mediaString = String.IsNullOrWhiteSpace(media.Name) ? media.Artist : media.Name;
                if (!String.IsNullOrWhiteSpace(mediaString))
                {
                    if (mediaString.Contains(':'))
                    {
                        if (media.Location.Contains(":"))
                        {
                            var band = media.Location.Substring(0, media.Location.IndexOf(':'));
                            var freqString = mediaString.Replace("DAB: ", "").Replace("FM: ", "").Replace("MHz", "");
                            //"FM: {0:0.00}MHz"

                            var bandfont = OpenMobile.Graphics.Font.Arial;
                            bandfont.Size = 100;
                            image.RenderText(0, 0, image.Width, image.Height, band, bandfont, eTextFormat.Normal, Alignment.CenterCenter | Alignment.WordWrap, OpenMobile.Graphics.Color.DarkGray, OpenMobile.Graphics.Color.DarkGray, FitModes.FitFillSingleLine);

                            var font = OpenMobile.Graphics.Font.Arial;
                            font.Size = 100;
                            image.RenderText(0, 0, image.Width, image.Height, freqString, font, eTextFormat.Bold, Alignment.CenterCenter | Alignment.WordWrap, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.White, FitModes.FitFillSingleLine);
                        }
                        else
                        {
                            var font = OpenMobile.Graphics.Font.Arial;
                            font.Size = 28;
                            image.RenderText(0, 0, image.Width, image.Height, mediaString, font, eTextFormat.Bold, Alignment.CenterCenter | Alignment.WordWrap, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.Black, FitModes.FitFill);
                        }
                    }
                    else
                    {
                        var font = OpenMobile.Graphics.Font.Arial;
                        font.Size = 28;
                        image.RenderText(0, 0, image.Width, image.Height, mediaString, font, eTextFormat.Bold, Alignment.CenterCenter | Alignment.WordWrap, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.Black, FitModes.FitFill);
                    }
                }
                else
                {   // Use default image
                    image = OM.Host.getSkinImage("Icons|Icon-Glass-Radio").image;
                }
                coverFound = false;
            }
            media.coverArt = image;

            return coverFound;
        }

        private bool TryGetIDFromChannelIDString(string channelIDString, out object channelID)
        {
            int channelIDInt = 0;
            if (_Radio_MediaSource == _MediaSource_DAB)
            {
                if (int.TryParse(channelIDString.Replace("DAB:", ""), out channelIDInt))
                {
                    channelID = channelIDInt;
                    return true;
                }
                else
                {
                    channelID = channelIDInt;
                    return false;
                }
            }
            else if (_Radio_MediaSource == _MediaSource_FM)
            {
                if (int.TryParse(channelIDString.Replace("FM:", ""), out channelIDInt))
                {
                    channelID = channelIDInt;
                    return true;
                }
                else
                {
                    channelID = channelIDInt;
                    return false;
                }
            }
            channelID = 0;
            return false;
        }

        #endregion

        #region Media provider interface

        public override Playlist MediaProviderData_GetPlaylist(Zone zone)
        {
            if (_Radio_MediaSource == _MediaSource_DAB)
            {
                switch (_Radio_DAB_ListSource)
                {
                    case ListSource.Live:
                        return _Radio_DAB_Channels_Live;
                    case ListSource.Preset:
                        return _Radio_DAB_Channels_Preset;
                    default:
                        break;
                }
            }
            else if (_Radio_MediaSource == _MediaSource_FM)
            {
                switch (_Radio_FM_ListSource)
                {
                    case ListSource.Live:
                        return _Radio_FM_Channels_Live;
                    case ListSource.Preset:
                        return _Radio_FM_Channels_Preset;
                    default:
                        break;
                }
            }
            return null;
        }

        public override Playlist MediaProviderData_SetPlaylist(Zone zone, Playlist playlist)
        {
            throw new NotImplementedException();
        }

        public override string MediaProviderCommand_Activate(Zone zone)
        {
            // TODO: Add audio routing
            return ActivateRadio(zone);
        }

        public override string MediaProviderCommand_Deactivate(Zone zone)
        {
            // Save playlists to DB
            _Radio_DAB_Channels_Preset.Save();
            _Radio_FM_Channels_Preset.Save();

            // TODO: Add audio routing
            return DeactivateRadio(zone);
        }

        public override string MediaProviderCommand_Playback_Start(Zone zone, object[] param)
        {
            if (param == null || param.Length == 0)
            {
                if (_Radio_MediaSource == _MediaSource_DAB)
                {
                    _RadioCommand = RadioCommands.SetMode_DAB;
                }
                else if (_Radio_MediaSource == _MediaSource_FM)
                {
                    _RadioCommand = RadioCommands.SetMode_FM;
                }
                return "";
            }
            else
            {
                if (param == null || param.Length < 1)
                    return "Invalid parameters";

                if (!(param[0] is int))
                    return "Invalid parameter type";

                int index = (int)param[0];

                // Update current playlist
                var playlist = MediaProviderData_GetPlaylist(zone);
                playlist.CurrentIndex = index;


                if (_Radio_MediaSource == _MediaSource_DAB)
                {
                    _Radio_CommandData = playlist.CurrentIndex;
                    _RadioCommand = RadioCommands.TuneChannel;
                }
                else if (_Radio_MediaSource == _MediaSource_FM)
                {
                    int channelID = 0;
                    if (int.TryParse(playlist.CurrentItem.Location.Replace("FM:", ""), out channelID))
                    {
                        _Radio_CommandData = channelID;
                        _RadioCommand = RadioCommands.TuneChannel;
                    }
                    else
                    {
                        return "Unable to play channel";
                    }
                }

                return "";
            }
        }

        public override string MediaProviderCommand_Playback_Pause(Zone zone, object[] param)
        {
            _RadioCommand = RadioCommands.Stop;
            return "";

            //var vol = HW_Monkeyboard.GetVolumePercent();
            //if (vol == 0)
            //    _RadioCommand = RadioCommands.Unmute;
            //else
            //    _RadioCommand = RadioCommands.Mute;
            //return "";
        }

        public override string MediaProviderCommand_Playback_Stop(Zone zone, object[] param)
        {
            _RadioCommand = RadioCommands.Stop;
            return "";
        }

        public override string MediaProviderCommand_Playback_Next(Zone zone, object[] param)
        {
            object commandData = null;
            if (_Radio_MediaSource == _MediaSource_DAB)
            {
                switch (_Radio_DAB_ListSource)
                {
                    case ListSource.Live:
                        _Radio_DAB_Channels_Live.GotoNextMedia();
                        commandData = _Radio_DAB_Channels_Live.CurrentIndex;
                        _RadioCommand = RadioCommands.TuneChannel;
                        break;
                    case ListSource.Preset:
                        _Radio_DAB_Channels_Preset.GotoNextMedia();
                        try
                        {
                            var channelObject = _Radio_DAB_Channels_Live.Items.Where(x => x.Name == _Radio_DAB_Channels_Preset.CurrentItem.Name).FirstOrDefault();
                            if (channelObject != null)
                            {
                                var index = _Radio_DAB_Channels_Live.Items.IndexOf(channelObject);
                                if (index >= 0)
                                    commandData = index;
                                else
                                    return "Unable to go to next channel";
                            }
                            else
                            {
                                return "Unable to go to next channel";
                            }
                        }
                        catch
                        {
                            return "Unable to go to next channel";
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (_Radio_MediaSource == _MediaSource_FM)
            {
                switch (_Radio_FM_ListSource)
                {
                    case ListSource.Live:
                        {
                            _Radio_FM_Channels_Live.GotoNextMedia();
                            if (TryGetIDFromChannelIDString(_Radio_FM_Channels_Live.CurrentItem.Location, out _Radio_CommandData))
                                _RadioCommand = RadioCommands.TuneChannel;
                        }
                        break;
                    case ListSource.Preset:
                        {
                            _Radio_FM_Channels_Preset.GotoNextMedia();
                            if (TryGetIDFromChannelIDString(_Radio_FM_Channels_Preset.CurrentItem.Location, out _Radio_CommandData))
                                _RadioCommand = RadioCommands.TuneChannel;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (commandData != null)
            {
                _RadioCommand = RadioCommands.TuneChannel;
                _Radio_CommandData = commandData;
                return "";
            }
            else
            {
                return "Unable to go to next channel";
            }
        }

        public override string MediaProviderCommand_Playback_Previous(Zone zone, object[] param)
        {
            object commandData = null;
            if (_Radio_MediaSource == _MediaSource_DAB)
            {
                switch (_Radio_DAB_ListSource)
                {
                    case ListSource.Live:
                        _RadioCommand = RadioCommands.TuneChannel;
                        _Radio_DAB_Channels_Live.GotoPreviousMedia();
                        commandData = _Radio_DAB_Channels_Live.CurrentIndex;
                        break;
                    case ListSource.Preset:
                        _Radio_DAB_Channels_Preset.GotoPreviousMedia();
                        try
                        {
                            var channelObject = _Radio_DAB_Channels_Live.Items.Where(x => x.Name == _Radio_DAB_Channels_Preset.CurrentItem.Name).FirstOrDefault();
                            if (channelObject != null)
                            {
                                var index = _Radio_DAB_Channels_Live.Items.IndexOf(channelObject);
                                if (index >= 0)
                                    commandData = index;
                                else
                                    return "Unable to go to next channel";
                            }
                            else
                            {
                                return "Unable to go to next channel";
                            }
                        }
                        catch
                        {
                            return "Unable to go to next channel";
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (_Radio_MediaSource == _MediaSource_FM)
            {
                switch (_Radio_FM_ListSource)
                {
                    case ListSource.Live:
                        {
                            _Radio_FM_Channels_Live.GotoPreviousMedia();
                            if (TryGetIDFromChannelIDString(_Radio_FM_Channels_Live.CurrentItem.Location, out _Radio_CommandData))
                                _RadioCommand = RadioCommands.TuneChannel;
                        }
                        break;
                    case ListSource.Preset:
                        {
                            _Radio_FM_Channels_Preset.GotoPreviousMedia();
                            if (TryGetIDFromChannelIDString(_Radio_FM_Channels_Preset.CurrentItem.Location, out _Radio_CommandData))
                                _RadioCommand = RadioCommands.TuneChannel;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (commandData != null)
            {
                _RadioCommand = RadioCommands.TuneChannel;
                _Radio_CommandData = commandData;
                return "";
            }
            else
            {
                return "Unable to go to next channel";
            }
        }

        public override string MediaProviderCommand_Playback_SeekForward(Zone zone, object[] param)
        {
            if (_Radio_MediaSource == _MediaSource_FM)
            {
                _RadioCommand = RadioCommands.NextStream;
                return "";
            }
            else
            {
                return "Not supported on DAB";
            }
        }

        public override string MediaProviderCommand_Playback_SeekBackward(Zone zone, object[] param)
        {
            if (_Radio_MediaSource == _MediaSource_FM)
            {
                _RadioCommand = RadioCommands.PreviousStream;
                return "";
            }
            else
            {
                return "Not supported on DAB";
            }
        }

        public override bool MediaProviderData_SetShuffle(Zone zone, bool state)
        {
            return false;
        }

        public override bool MediaProviderData_GetShuffle(Zone zone)
        {
            return false;
        }

        public override bool MediaProviderData_SetRepeat(Zone zone, bool state)
        {
            return false;
        }

        public override bool MediaProviderData_GetRepeat(Zone zone)
        {
            return false;
        }

        public override MediaProviderAbilities MediaProviderData_GetMediaSourceAbilities(string mediaSource)
        {
            var abilities = new MediaProviderAbilities();
            abilities.CanGotoNext = true;
            abilities.CanGotoPrevious = true;
            abilities.CanPause = true;
            abilities.CanPlay = true;
            abilities.CanRepeat = false;
            abilities.CanSeek = true;
            abilities.CanShuffle = false;
            abilities.CanStop = true;

            return abilities;
        }

        public override List<MediaSource> MediaProviderData_GetMediaSources()
        {
            return _MediaSources;
        }

        public override MediaSource MediaProviderData_GetMediaSource(Zone zone)
        {
            return _Radio_MediaSource;
        }

        public override string MediaProviderCommand_ActivateMediaSource(Zone zone, string mediaSourceName)
        {
            var mediaSource = _MediaSources.Find(x => x.Name == mediaSourceName);
            if (mediaSource == null)
                return "Media source not available";

            // Change media source
            if (mediaSource == _MediaSource_DAB)
                _RadioCommand = RadioCommands.SetMode_DAB;
            else if (mediaSource == _MediaSource_FM)
                _RadioCommand = RadioCommands.SetMode_FM;

            return "";
        }

        #endregion
    }
}
