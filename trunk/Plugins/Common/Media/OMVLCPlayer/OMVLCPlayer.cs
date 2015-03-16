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

#region VLC mappings

using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;

#endregion

namespace OMVLCPlayer
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows)]
    public sealed class OMVLCPlayer : MediaProviderBase
    {
        private class VLCMetaData
        {
            public string Album { get; set; }
            public string Artist { get; set; }
            public string ArtworkURL { get; set; }
            public string Copyright { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
            public string EncodedBy { get; set; }
            public string Genre { get; set; }
            public string Language { get; set; }
            public string NowPlaying { get; set; }
            public string Publisher { get; set; }
            public string Rating { get; set; }
            public string Setting { get; set; }
            public string Title { get; set; }
            public string TrackID { get; set; }
            public string TrackNumber { get; set; }
            public string URL { get; set; }
            public long Duration { get; set; }

            public VLCMetaData()
            {
            }

            public VLCMetaData(IMedia media)
            {
                if (media is IMediaFromFile)
                {
                    var currentMedia = (IMediaFromFile)media;
                    Album = currentMedia.GetMetaData(MetaDataType.Album);
                    Artist = currentMedia.GetMetaData(MetaDataType.Artist);
                    ArtworkURL = currentMedia.GetMetaData(MetaDataType.ArtworkURL);
                    Copyright = currentMedia.GetMetaData(MetaDataType.Copyright);
                    Date = currentMedia.GetMetaData(MetaDataType.Date);
                    Description = currentMedia.GetMetaData(MetaDataType.Description);
                    EncodedBy = currentMedia.GetMetaData(MetaDataType.EncodedBy);
                    Genre = currentMedia.GetMetaData(MetaDataType.Genre);
                    Language = currentMedia.GetMetaData(MetaDataType.Language);
                    NowPlaying = currentMedia.GetMetaData(MetaDataType.NowPlaying);
                    Publisher = currentMedia.GetMetaData(MetaDataType.Publisher);
                    Rating = currentMedia.GetMetaData(MetaDataType.Rating);
                    Setting = currentMedia.GetMetaData(MetaDataType.Setting);
                    Title = currentMedia.GetMetaData(MetaDataType.Title);
                    TrackID = currentMedia.GetMetaData(MetaDataType.TrackID);
                    TrackNumber = currentMedia.GetMetaData(MetaDataType.TrackNumber);
                    URL = currentMedia.GetMetaData(MetaDataType.URL);
                    Duration = currentMedia.Duration;
                }
            }
        }

        private enum PlaybackModes
        {
            Stop,
            Play,
            Pause,
            Restart
        }

        private Notification _NotificationIndexingStatus = null;
        private IMediaPlayerFactory _PlayerFactory;
        private AudioOutputModuleInfo _Player_DefaultAudioModule;
        private List<AudioOutputDeviceInfo> _Player_AudioOutputDevices = new List<AudioOutputDeviceInfo>();
        private List<MediaSource> _MediaSources = new List<MediaSource>();

        // Zone specific data
        private Dictionary<Zone, IDiskPlayer> _ZonePlayer = new Dictionary<Zone, IDiskPlayer>();
        private Dictionary<Zone, CommandGroup> _Zone_CommandGroup_Media = new Dictionary<Zone, CommandGroup>();
        private Dictionary<Zone, DataSourceGroup> _Zone_DataSourceGroup_Media = new Dictionary<Zone, DataSourceGroup>();
        private Dictionary<Zone, mediaInfo> _Zone_MediaInfo = new Dictionary<Zone, mediaInfo>();
        private Dictionary<Zone, Playlist> _Zone_Playlist = new Dictionary<Zone, Playlist>();
        private Dictionary<Zone, Timer> _Zone_DelayedCommand = new Dictionary<Zone, Timer>();
        private Dictionary<Zone, PlaybackModes> _Zone_PlaybackMode = new Dictionary<Zone, PlaybackModes>();
        private Dictionary<Zone, float> _Zone_PlaybackPos = new Dictionary<Zone, float>();
        private Dictionary<Zone, DateTime> _Zone_TimeStampLastCommand = new Dictionary<Zone, DateTime>();
        private Dictionary<Zone, VLCMetaData> _Zone_Player_MetaData = new Dictionary<Zone, VLCMetaData>();
        private Dictionary<Zone, MediaSource> _Zone_MediaSource = new Dictionary<Zone, MediaSource>();

        // Text constants
        private const string _Text_PleaseWaitLoadingMedia = "Please wait, loading media...";



        #region Constructor / Plugin config

        public OMVLCPlayer()
            : base("OMVLCPlayer", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "OM Official Audio/Video player (MediaProvider)", "OM Dev team/Borte", "")
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
                    OM.Host.DebugMsg(DebugMessageType.Info, "Starting initialization");

                    // Initialize media sources
                    _MediaSources.Add(new MediaSource_FileBasedMedia(this, "File", "File based media", OM.Host.getSkinImage("Icons|Icon-File")));
                    _MediaSources.Add(new MediaSource_DiscBasedMedia(this, "CD", "CompactDisc", OM.Host.getSkinImage("Icons|Icon-CD")));

                    SetVLCFileEnvironment();

                    OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

                    // Create player factory
                    _PlayerFactory = new MediaPlayerFactory(false);

                    // Read VLC audio modules
                    var audioModules = _PlayerFactory.AudioOutputModules.ToArray();
                    _Player_AudioOutputDevices = new List<AudioOutputDeviceInfo>();
                    foreach (var module in audioModules)
                    {
                        var outputDevices = _PlayerFactory.GetAudioOutputDevices(module);
                        if (outputDevices.Count() > 0)
                        {
                            _Player_AudioOutputDevices.AddRange(outputDevices);
                            _Player_DefaultAudioModule = module;
                        }
                    }

                    OM.Host.DebugMsg(DebugMessageType.Info, "Starting initialization of zone specific data");
                    foreach (var zone in OM.Host.ZoneHandler.Zones)
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Starting initialization of zone specific data (Zone:{0})", zone.Name));
                        
                        // Initialize local variables
                        _Zone_MediaInfo.Add(zone, new mediaInfo());
                       
                        // Default media source
                        _Zone_MediaSource.Add(zone, _MediaSources[0]);

                        // Restore playback mode from DB
                        _Zone_PlaybackMode.Add(zone, StoredData.GetEnum<PlaybackModes>(this, String.Format("{0}_PlaybackMode", zone), PlaybackModes.Stop));

                        // Restore playback pos from DB
                        _Zone_PlaybackPos.Add(zone, StoredData.GetFloat(this, String.Format("{0}_PlaybackPos", zone), 0));

                        // Initialize delayed command timer
                        Timer tmr = new Timer(750);
                        tmr.Enabled = false;
                        tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmrDelayedCommand_Elapsed);
                        tmr.Tag = zone;
                        _Zone_DelayedCommand.Add(zone, tmr);

                        // Initialize timing variables
                        _Zone_TimeStampLastCommand.Add(zone, new DateTime());

                        // Initialize media variables
                        _Zone_Player_MetaData.Add(zone, new VLCMetaData());

                        // Initialize play lists
                        string playlistName = String.Format("{0}.Zone{1}.PlayList_Current", this.pluginName, zone.Index);
                        Playlist.SetDisplayName(ref playlistName, String.Format("[{0}] Current playlist ({1})", zone.Name, this.pluginName));
                        Playlist playlist = new Playlist(playlistName);
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Playlist initialized ({0})", playlistName));
                        _Zone_Playlist.Add(zone, playlist);

                        // Preload one player per zone
                        ZonePlayer_CheckInstance(zone);

                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Completed initialization of zone specific data (Zone:{0})", zone.Name));
                    }

                    // Load playlists
                    OM.Host.DebugMsg(DebugMessageType.Info, "Starting to load playlist items");
                    foreach (var playlist in _Zone_Playlist.Values)
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Starting to loading items for playlist {0} from DB", playlist.Name));
                        playlist.Load();
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Loaded {1} items for playlist {0} from DB", playlist.Name, playlist.Count));
                    }
                });

            return eLoadStatus.LoadSuccessful;
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            //if (function == eFunction.CloseProgramPreview)
            //{
            //    SavePlayerStates();
            //}
        }

        private void SetVLCFileEnvironment()
        {
            // Read VLC core ref file
            bool vlcCore_Installed = false;
            bool vlcCore_x64Installed = false;
            bool vlcCore_x32Installed = false;
            bool vlcCore_x64InstallRequired = false;
            bool vlcCore_x32InstallRequired = false;

            string vlcCoreRef_FileName = base.GetPluginFilePath("VLCCore.ref");
            string vlcCoreRef_Content = "";

            // Check for vlc core ref file
            if (!File.Exists(vlcCoreRef_FileName))
            {
                // No file exists this is a new install or invalid install
                vlcCore_Installed = false;
                OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core not installed, trying to detect type of install required");
            }
            else
            {   // Read file data to get installed version
                vlcCore_Installed = true;
                vlcCoreRef_Content = File.ReadAllText(vlcCoreRef_FileName);
                if (vlcCoreRef_Content.Contains("32"))
                {
                    vlcCore_x32Installed = true;
                    OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core 32bit installed");
                }
                else if (vlcCoreRef_Content.Contains("64"))
                {
                    vlcCore_x64Installed = true;
                    OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core 64bit installed");
                }
            }

            // Do we have to reinstall or install the vlc core?
            //if (!vlcCore_Installed)
            {
                if (!vlcCore_x64Installed & OpenMobile.Framework.OSSpecific.Is64BitProcess)
                {
                    vlcCore_x64InstallRequired = true;
                    OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core not installed, 64bit installation required");
                }
                else if (!vlcCore_x32Installed & !OpenMobile.Framework.OSSpecific.Is64BitProcess)
                {
                    vlcCore_x32InstallRequired = true;
                    OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core not installed, 32bit installation required");
                }
            }
            //else
            //{
            //    if (vlcCore_x32Installed & OpenMobile.Framework.OSSpecific.Is64BitRequired)
            //    {
            //        vlcCore_x64InstallRequired = true;
            //        OM.Host.DebugMsg(DebugMessageType.Info, "Wrong VLC Core 32bit installed, 64bit installation required");
            //    }
            //    else if (vlcCore_x64Installed)
            //    {
            //        vlcCore_x32InstallRequired = true;
            //        OM.Host.DebugMsg(DebugMessageType.Info, "Wrong VLC Core 32bit installed, 64bit installation required");
            //    }
            //}

            // install core?
            if (vlcCore_x64InstallRequired | vlcCore_x32InstallRequired)
            {
                _NotificationIndexingStatus = new Notification(this, "NotificationIndexingStatus", this.pluginIcon.image, this.pluginIcon.image, "Installing VLC core", "Clearing existing files...");
                _NotificationIndexingStatus.Global = true;
                _NotificationIndexingStatus.State = Notification.States.Active;
                OM.Host.UIHandler.AddNotification(_NotificationIndexingStatus);

                // Yes install core, but first try to delete existing vlc core
                try
                {
                    Directory.Delete(base.GetPluginFilePath("lua"), true);
                    Directory.Delete(base.GetPluginFilePath("plugins"), true);
                    File.Delete(base.GetPluginFilePath("libvlc.dll"));
                    File.Delete(base.GetPluginFilePath("libvlccore.dll"));
                    File.Delete(base.GetPluginFilePath("COPYING.txt"));
                    File.Delete(base.GetPluginFilePath("VLCCore.ref"));
                }
                catch (Exception ex)
                {
                    if (!(ex is DirectoryNotFoundException) & !(ex is FileNotFoundException))
                        OM.Host.DebugMsg("Error while removing current VLC Core files", ex);
                }

                // Unzip core
                if (vlcCore_x64InstallRequired)
                {
                    try
                    {
                        Action<int, string> progressCallback = new Action<int,string>((int progress, string name) =>
                        {
                            _NotificationIndexingStatus.Text = String.Format("Extracting 64bit core {0}%, please wait...", progress);
                        });

                        _NotificationIndexingStatus.Text = "Extracting 64bit core, please wait...";
                        OpenMobile.helperFunctions.FileHelpers.ExtractZipFile(System.IO.Path.Combine(base.GetPluginFilePath("VLC_Core"), "VLC_Core_x64.zip"), "", base.GetPluginPath(), progressCallback);
                        File.WriteAllText(vlcCoreRef_FileName, "64");
                        OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core 64bit installation completed");
                        _NotificationIndexingStatus.Text = "Installation of 64bit core completed";
                    }
                    catch (Exception ex)
                    {
                        OM.Host.DebugMsg("Error while extracting VLC Core files for 64bit", ex);
                    }
                }
                else if (vlcCore_x32InstallRequired)
                {
                    try
                    {
                        Action<int, string> progressCallback = new Action<int, string>((int progress, string name) =>
                        {
                            _NotificationIndexingStatus.Text = String.Format("Extracting 32bit core {0}%, please wait...", progress);
                        });

                        _NotificationIndexingStatus.Text = "Extracting 32bit core, please wait...";
                        OpenMobile.helperFunctions.FileHelpers.ExtractZipFile(System.IO.Path.Combine(base.GetPluginFilePath("VLC_Core"), "VLC_Core_x32.zip"), "", base.GetPluginPath(), progressCallback);
                        File.WriteAllText(vlcCoreRef_FileName, "32");
                        OM.Host.DebugMsg(DebugMessageType.Info, "VLC Core 32bit installation completed");
                        _NotificationIndexingStatus.Text = "Installation of 32bit core completed";
                    }
                    catch (Exception ex)
                    {
                        OM.Host.DebugMsg("Error while extracting VLC Core files for 32bit", ex);
                    }
                }
                _NotificationIndexingStatus.State = Notification.States.Passive;
            }
        }

        /*
                    #region PlayURL

                    case "PlayURL":
                        {
                            PlayerControl_DelayedPlay_Cancel(zone);

                            mediaInfo media = null;

                            // Do we have a minimum of one parameter?
                            if (Params.IsParamsValid(param, 1))
                            {   // Yes
                                if (param[0] is string)
                                {   // Play media location directly
                                    media = new mediaInfo(eMediaType.URLStream, Params.GetParam<string>(param, 0, ""));
                                }
                            }

                            if (media != null)
                            {
                                result = true;
                                return PlayerControl_Play(zone, media);
                            }
                            return "Invalid media";
                        }

                    #endregion

        */

        private void tmrDelayedCommand_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer tmr = sender as Timer;
            tmr.Enabled = false;

            mediaInfo media = (mediaInfo)tmr.Tag2;
            System.Diagnostics.Debug.Write(String.Format("Sending delayed play {0}\r\n", media));
            PlayerControl_Play((Zone)tmr.Tag, media);
        }

        private void PlayerControl_Restart(Zone zone, float playbackPos)
        {
            // Abort any ongoing delayed commands
            PlayerControl_DelayedPlay_Cancel(zone);

            // Update media info
            mediaInfo media = _Zone_Playlist[zone].CurrentItem;

            if (media != null)
            {
                base.MediaProviderData_ReportMediaInfo(zone, media);

                // Start playback of new media
                PlayerControl_Play(zone, media);

                // Goto to restart point
                _ZonePlayer[zone].Position = playbackPos;

                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Restarted playback of '{2}' from position {3}%", zone, zone.Name, media, playbackPos));
            }
            else
            {
                OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Zone: {0}({1}) -> Unable to restart playback. No media item", zone, zone.Name));
            }
        }

        private void PlayerControl_DelayedPlay_Cancel(Zone zone)
        {
            _Zone_DelayedCommand[zone].Enabled = false;
        }

        private void PlayerControl_DelayedPlay_Enable(Zone zone, mediaInfo media)
        {
            _Zone_DelayedCommand[zone].Tag2 = media;
            _Zone_DelayedCommand[zone].Enabled = true;
        }

        private string PlayerControl_Play(Zone zone, mediaInfo media)
        {
            if (media == null)
                return "No media to play";

            System.Diagnostics.Debug.Write(String.Format("Sending play {0}\r\n", media));
            if (media != null)
            {
                _Zone_MediaInfo[zone] = media;
                try
                {
                    // Classify media source
                    switch (media.Type)
                    {
                        default:
                            {
                                #region default media

                                if (!System.IO.File.Exists(media.Location))
                                    return "media file not found";

                                try
                                {
                                    var VLCmedia = _PlayerFactory.CreateMedia<IMediaFromFile>(media.Location);
                                    VLCmedia.Parse(false);
                                    _Zone_Player_MetaData[zone] = new VLCMetaData(VLCmedia);
                                    _ZonePlayer[zone].Open(VLCmedia);
                                    VLCmedia.Dispose();
                                    _ZonePlayer[zone].Play();
                                }
                                catch
                                {
                                    return "Invalid media";
                                }

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.URLStream:
                            {
                                #region URL Stream

                                // TODO: Implement URL streaming

                                try
                                {
                                    var VLCmedia = _PlayerFactory.CreateMedia<IMedia>(media.Location);
                                    VLCmedia.Parse(false);
                                    _Zone_Player_MetaData[zone] = new VLCMetaData(VLCmedia);
                                    _ZonePlayer[zone].Open(VLCmedia);
                                    VLCmedia.Dispose();
                                    _ZonePlayer[zone].Play();
                                }
                                catch
                                {
                                    return "Invalid media";
                                }


                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                // Inform user that we're loading media
                                base.MediaProviderData_ReportMediaText(zone, _Text_PleaseWaitLoadingMedia, media.Location);

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.AudioCD:
                            {
                                // TODO: Implement audio CD

                                #region AudioCD

                                string Drive = "";
                                if (Configuration.RunningOnWindows)
                                    Drive = media.Location.Substring(0, 2);
                                //TODO: Add other platforms

                                //if (_ZonePlayer[zone].DiscDevice == null || !_ZonePlayer[zone].DiscDevice.Equals(Drive))
                                //    _ZonePlayer[zone].DiscDevice = Drive;

                                //_ZonePlayer[zone].Identify = true;
                                //_ZonePlayer[zone].Start(true, true);

                                //_ZonePlayer[zone].PlayCD(media.TrackNumber);

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.HDDVD:
                        case eMediaType.DVD:
                            {
                                // TODO: Implement audio DVD

                                #region DVD

                                //string Drive = "";
                                //if (Configuration.RunningOnWindows)
                                //    Drive = media.Location.Substring(0, 2);
                                ////TODO: Add other platforms

                                //if (_ZonePlayer[zone].DiscDevice == null || !_ZonePlayer[zone].DiscDevice.Equals(Drive))
                                //    _ZonePlayer[zone].DiscDevice = Drive;

                                ////_AudioPlayers[zone].Identify = false;
                                //_ZonePlayer[zone].Start(true, true);

                                //bool discDeviceBusy = false;
                                ////ExecuteOnAllPlayers((mPlayer player) =>
                                ////{
                                ////    if (player.PlayerState != mPlayer.PlayerStates.Idle)
                                ////        discDeviceBusy = true;
                                ////});

                                //if (!discDeviceBusy)
                                //    _ZonePlayer[zone].PlayDVD();
                                //else
                                //    return "DVD drive is used by another zone/screen";

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.BluRay:
                            {
                                // TODO: Implement audio BluRay

                                //// Todo: find bluray drive
                                //_ZonePlayer[zone].PlayBluRay();

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                            }
                            return ""; // No error

                    }
                }
                catch (Exception e)
                {
                    OM.Host.DebugMsg(this.pluginName, e);
                    _Zone_PlaybackMode[zone] = PlaybackModes.Stop;
                    return e.Message; // Playback error
                }
            }
            return ""; // No error
        }

        private void MediaInfo_Clear(Zone zone)
        {
            _Zone_MediaInfo[zone] = new mediaInfo();
            base.MediaProviderData_ReportMediaInfo(zone, _Zone_MediaInfo[zone]);
        }

        private string TimeSpanToString(TimeSpan timespan)
        {
            if (timespan.Hours > 0)
                return String.Format("{0}:{1}:{2}", timespan.Hours, timespan.Minutes, timespan.Seconds);
            else
                return String.Format("{0}:{1}", timespan.Minutes, timespan.Seconds);
        }

        #region ZonePlayers instance control

        private void ZonePlayer_CheckInstance(Zone zone)
        {
            // Don't create instances for zone's with subzones
            if (zone.HasSubZones)
                return;

            // Initialize new instance of player for this zone (if not already done)
            lock (_ZonePlayer)
            {
                if (!_ZonePlayer.ContainsKey(zone))
                {   // Player does not exist, create a new zonePlayer
                  
                    var player = _PlayerFactory.CreatePlayer<IDiskPlayer>();

                    // Set audio device
                    if (!zone.AudioDevice.IsDefault)
                    {   // Audio device is not default, let's set it specifically
                        AudioOutputDeviceInfo device = GetVLCAudioDeviceFromZone(zone);
                        if (device != null)
                            player.SetAudioOutputModuleAndDevice(_Player_DefaultAudioModule, device);
                    }

                    // Connect events
                    player.Events.PlayerPositionChanged += new EventHandler<MediaPlayerPositionChanged>(Events_PlayerPositionChanged);
                    player.Events.TimeChanged += new EventHandler<MediaPlayerTimeChanged>(Events_TimeChanged);
                    player.Events.MediaEnded += new EventHandler(Events_MediaEnded);
                    player.Events.PlayerStopped += new EventHandler(Events_PlayerStopped);
                    player.Events.MediaChanged += new EventHandler<MediaPlayerMediaChanged>(Events_MediaChanged);
                    player.Events.PlayerOpening += new EventHandler(Events_PlayerOpening);
                    player.Events.PlayerPaused += new EventHandler(Events_PlayerPaused);
                    player.Events.PlayerPlaying += new EventHandler(Events_PlayerPlaying);

                    _ZonePlayer.Add(zone, player);
                }
            }
        }

        private AudioOutputDeviceInfo GetVLCAudioDeviceFromZone(Zone zone)
        {
            var device = _Player_AudioOutputDevices.FirstOrDefault(x => x.Longname.Contains(zone.AudioDevice.Name.Substring(0,(zone.AudioDevice.Name.Length < 31 ? zone.AudioDevice.Name.Length : 31))));
            return device;
        }

        #endregion

        #region Player events

        private Zone GetPlayerZone(object player)
        {
            var zones = _ZonePlayer.Keys.ToArray();
            var players =_ZonePlayer.Values.ToArray();
            var matchedPlayerIndex = Array.IndexOf(players, player);
            return zones[matchedPlayerIndex];
        }

        private TimeSpan GetPlayer_CurrentPosition(Zone zone)
        {
            return TimeSpan.FromMilliseconds(_ZonePlayer[zone].Length * _ZonePlayer[zone].Position);
        }

        void Events_PlayerPlaying(object sender, EventArgs e)
        {
            Zone zone = GetPlayerZone(sender);
            base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Playing);
            base.MediaProviderData_ReportMediaInfo(zone, _Zone_MediaInfo[zone], GetPlayer_CurrentPosition(zone));
        }

        void Events_PlayerPaused(object sender, EventArgs e)
        {
            Zone zone = GetPlayerZone(sender);
            base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Paused);
            base.MediaProviderData_ReportMediaInfo(zone, _Zone_MediaInfo[zone], GetPlayer_CurrentPosition(zone));
        }

        void Events_PlayerOpening(object sender, EventArgs e)
        {
            Zone zone = GetPlayerZone(sender);
        }

        void Events_MediaChanged(object sender, MediaPlayerMediaChanged e)
        {
            Zone zone = GetPlayerZone(sender);

            // Get media info from VLC
            var mediaFromVLC = _Zone_Player_MetaData[zone];

            // Update mediaInfo
            mediaInfo media = _Zone_MediaInfo[zone];

            UpdateMissingCoverImage(media);

            //// Update mediaType if available
            //eMediaType playerType = eMediaType.NotSet;
            //switch (sender.MediaInfo.MediaType)
            //{
            //    case mPlayer.MediaTypes.None:
            //        playerType = eMediaType.NotSet;
            //        break;
            //    case mPlayer.MediaTypes.File:
            //        playerType = eMediaType.Local;
            //        break;
            //    case mPlayer.MediaTypes.CD:
            //        playerType = eMediaType.AudioCD;
            //        break;
            //    case mPlayer.MediaTypes.DVD:
            //        playerType = eMediaType.DVD;
            //        break;
            //    case mPlayer.MediaTypes.BluRay:
            //        playerType = eMediaType.BluRay;
            //        break;
            //    case mPlayer.MediaTypes.Stream_HTTP:
            //        playerType = eMediaType.URLStream;
            //        break;
            //    case mPlayer.MediaTypes.Other:
            //        playerType = eMediaType.Other;
            //        break;
            //    default:
            //        break;
            //}

            // Push updates if needed
            int trackNumber;
            int? trackNumberDecoded = null;
            if (int.TryParse(mediaFromVLC.TrackNumber, out trackNumber))
                trackNumberDecoded = trackNumber;

            media.UpdateMissingInfo(mediaFromVLC.Album, mediaFromVLC.Artist, mediaFromVLC.Title, mediaFromVLC.Genre, (int)(mediaFromVLC.Duration / 1000), trackNumberDecoded);

            base.MediaProviderData_ReportMediaInfo(zone, _Zone_MediaInfo[zone], GetPlayer_CurrentPosition(zone));

            // Also update the provider info fields
            switch (media.Type)
            {
                case eMediaType.AudioCD:
                case eMediaType.DVD:
                case eMediaType.BluRay:
                    base.MediaProviderData_ReportMediaText(zone, String.Format("{0}", media.Name), String.Format("{0}", media.TrackNumber));
                    break;
                default:
                    base.MediaProviderData_ReportMediaText(zone, String.Format("{0}", media.Name), String.Format("{0} - {1}", media.Artist, media.Album));
                    break;
            }
        }

        void Events_PlayerStopped(object sender, EventArgs e)
        {
            Zone zone = GetPlayerZone(sender);
            base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Stopped);
        }

        void Events_MediaEnded(object sender, EventArgs e)
        {
            Zone zone = GetPlayerZone(sender);

            _Zone_Playlist[zone].GotoNextMedia();

            // Abort any ongoing delayed commands
            PlayerControl_DelayedPlay_Cancel(zone);

            // Update media info
            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
            base.MediaProviderData_ReportMediaInfo(zone, media);

            // Start playback of new media
            PlayerControl_Play(zone, media);
        }

        void Events_TimeChanged(object sender, MediaPlayerTimeChanged e)
        {
            Zone zone = GetPlayerZone(sender);
            base.MediaProviderData_ReportMediaInfo(zone, _Zone_MediaInfo[zone], GetPlayer_CurrentPosition(zone));
        }

        void Events_PlayerPositionChanged(object sender, MediaPlayerPositionChanged e)
        {
            Zone zone = GetPlayerZone(sender);
            _Zone_PlaybackPos[zone] = e.NewPosition;
        }

        #endregion

        /*
        void player_OnPlaybackStopped(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Playing", this.pluginName, zone.Index), false);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Stopped", this.pluginName, zone.Index), true);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Paused", this.pluginName, zone.Index), false);

            // Continue on next track in playlist
            if (_Zone_PlaybackMode[zone] == PlaybackModes.Play)
            {
                _Zone_Playlist[zone].GotoNextMedia();

                // Abort any ongoing delayed commands
                PlayerControl_DelayedPlay_Cancel(zone);

                // Update media info
                mediaInfo media = _Zone_Playlist[zone].CurrentItem;
                MediaInfo_Set(zone, media);

                // Start playback of new media
                //PlayerControl_DelayedPlay_Enable(zone, media);
                PlayerControl_Play(zone, media);
            }

            //MediaInfo_Clear(zone);
        }

        void player_OnPlaybackStarted(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Playing", this.pluginName, zone.Index), true);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Stopped", this.pluginName, zone.Index), false);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Paused", this.pluginName, zone.Index), false);

            MediaInfo_Set(zone, _Zone_MediaInfo[zone], sender.MediaInfo.PlaybackPos);
        }

        void player_OnPlaybackPaused(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Playing", this.pluginName, zone.Index), false);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Stopped", this.pluginName, zone.Index), false);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Paused", this.pluginName, zone.Index), true);

            MediaInfo_Set(zone, _Zone_MediaInfo[zone], sender.MediaInfo.PlaybackPos);
        }

        void player_OnMediaInfoUpdated(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;

            // Update mediaInfo
            mediaInfo media = _Zone_MediaInfo[zone];

            // Update mediaType if available
            eMediaType playerType = eMediaType.NotSet;
            switch (sender.MediaInfo.MediaType)
            {
                case mPlayer.MediaTypes.None:
                    playerType = eMediaType.NotSet;
                    break;
                case mPlayer.MediaTypes.File:
                    playerType = eMediaType.Local;
                    break;
                case mPlayer.MediaTypes.CD:
                    playerType = eMediaType.AudioCD;
                    break;
                case mPlayer.MediaTypes.DVD:
                    playerType = eMediaType.DVD;
                    break;
                case mPlayer.MediaTypes.BluRay:
                    playerType = eMediaType.BluRay;
                    break;
                case mPlayer.MediaTypes.Stream_HTTP:
                    playerType = eMediaType.URLStream;
                    break;
                case mPlayer.MediaTypes.Other:
                    playerType = eMediaType.Other;
                    break;
                default:
                    break;
            }

            UpdateMissingCoverImage(media);

            // Push updates if needed
            media.UpdateMissingInfo(sender.MediaInfo.Album, sender.MediaInfo.Artist, sender.MediaInfo.Title, sender.MediaInfo.Genre, (int)sender.MediaInfo.Length.TotalSeconds, sender.MediaInfo.Track, type: playerType);

            MediaInfo_Set(zone, media, sender.MediaInfo.PlaybackPos);

            // Also update the provider info fields
            switch (media.Type)
            {
                case eMediaType.AudioCD:
                case eMediaType.DVD:
                case eMediaType.BluRay:
                    MediaInfo_MediaText_Set(zone, String.Format("{0}", media.Name), String.Format("{0}", media.TrackNumber));
                    break;
                default:
                    MediaInfo_MediaText_Set(zone, String.Format("{0}", media.Name), String.Format("{0} - {1}", media.Artist, media.Album));
                    break;
            }
        }
        */

        /// <summary>
        /// Updates media info with an image that's loaded from the plugin folder if not already present
        /// </summary>
        /// <param name="media"></param>
        private void UpdateMissingCoverImage(mediaInfo media)
        {
            // Try to find local cover images if image is missing (loaded from plugin folder)
            if (media.coverArt == null || media.coverArt.image == null)
            {
                bool coverScanDone = false;
                if (!String.IsNullOrEmpty(media.Name) && _LastMissingCoverImageSearchText != media.Location)
                {
                    media.coverArt = OM.Host.getPluginImage(this, String.Format("CoverArt|{0}", media.Name)).image;
                    coverScanDone = true;
                }
                if (media.coverArt == null || media.coverArt.image == null)
                {
                    if (!String.IsNullOrEmpty(media.Location) && _LastMissingCoverImageSearchText != media.Location)
                    {
                        media.coverArt = getFolderImage(media.Location);
                        coverScanDone = true;
                    }
                }

                if (coverScanDone)
                {
                    if (media.coverArt == null || media.coverArt.image == null)
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Unable to find local media image in folder '{0}\\CoverArt\\' named '{1}'",
                            OpenMobile.helperFunctions.Plugins.Plugins.GetPluginFolder(this), media.Name));

                        if (media.Type == eMediaType.URLStream)
                            media.coverArt = OM.Host.getSkinImage("Icons|Icon-Stream").image;
                        else
                            media.coverArt = OM.Host.getSkinImage("Images|Image-UnknownAlbum").image;
                    }
                }
            }

            if (media.coverArt != null && media.coverArt.image != null)
                _LastMissingCoverImageSearchText = media.Location;
        }
        private string _LastMissingCoverImageSearchText = String.Empty;

        /// <summary>
        /// Retrieves a folder image located in the same folder as the music specified by URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static OImage getFolderImage(string url)
        {
            string path = OpenMobile.Path.Combine(System.IO.Path.GetDirectoryName(url), "Folder.jpg");
            if (System.IO.File.Exists(path) == true)
            {
                OImage img = OImage.FromFile(path);
                return img;
            }
            return null;
        }

        private void SavePlayerStates()
        {
            try
            {
                lock (_ZonePlayer)
                {
                    // Save playlists
                    foreach (var zone in _Zone_Playlist.Keys)
                    {
                        //if (_Zone_Playlist.ContainsKey(zone))
                        //{
                        //    _Zone_Playlist[zone].Save();
                        //    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playlist {2}", zone, zone.Name, _Zone_Playlist[zone].Name));
                        //}

                        //// Save current playback pos
                        //if (_Zone_PlaybackPos.ContainsKey(zone) && _ZonePlayer.ContainsKey(zone))
                        //{
                        //    StoredData.Set(this, String.Format("{0}_PlaybackPos", zone), _Zone_PlaybackPos[zone].ToString().Replace(",", "."));
                        //    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playback position {2}%", zone, zone.Name, _Zone_PlaybackPos[zone].ToString()));
                        //}

                        //// Save playback mode to DB
                        //if (_Zone_PlaybackMode.ContainsKey(zone))
                        //{
                        //    if (_Zone_PlaybackMode[zone] == PlaybackModes.Play || _Zone_PlaybackMode[zone] == PlaybackModes.Pause)
                        //        StoredData.Set(this, String.Format("{0}_PlaybackMode", zone), (int)PlaybackModes.Restart);
                        //    else
                        //        StoredData.Set(this, String.Format("{0}_PlaybackMode", zone), (int)_Zone_PlaybackMode[zone]);
                        //    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving PlaybackMode {2}", zone, zone.Name, _Zone_PlaybackMode[zone]));
                        //}

                        SavePlayerStates(zone);
                    }
                }
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg("Exception while saving player states", ex);
            }
        }

        private void SavePlayerStates(Zone zone)
        {
            try
            {
                lock (_ZonePlayer)
                {
                    if (_Zone_Playlist.ContainsKey(zone))
                    {
                        _Zone_Playlist[zone].Save();
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playlist {2}", zone, zone.Name, _Zone_Playlist[zone].Name));
                    }

                    // Save current playback pos
                    if (_Zone_PlaybackPos.ContainsKey(zone) && _ZonePlayer.ContainsKey(zone))
                    {
                        StoredData.Set(this, String.Format("{0}_PlaybackPos", zone), _Zone_PlaybackPos[zone].ToString().Replace(",", "."));
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playback position {2}%", zone, zone.Name, _Zone_PlaybackPos[zone].ToString()));
                    }

                    // Save playback mode to DB
                    if (_Zone_PlaybackMode.ContainsKey(zone))
                    {
                        if (_Zone_PlaybackMode[zone] == PlaybackModes.Play || _Zone_PlaybackMode[zone] == PlaybackModes.Pause)
                            StoredData.Set(this, String.Format("{0}_PlaybackMode", zone), (int)PlaybackModes.Restart);
                        else
                            StoredData.Set(this, String.Format("{0}_PlaybackMode", zone), (int)_Zone_PlaybackMode[zone]);
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving PlaybackMode {2}", zone, zone.Name, _Zone_PlaybackMode[zone]));
                    }
                }
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg("Exception while saving player states", ex);
            }
        }


        #region Dispose
        private bool _Disposing = false;
        public override void Dispose()
        {
            _Disposing = true;
            try
            {
                //lock (_ZonePlayer)
                {
                    //SavePlayerStates();

                    //// Save playlists
                    //foreach (var key in _Zone_Playlist.Keys)
                    //{
                    //    if (_Zone_Playlist.ContainsKey(key))
                    //        _Zone_Playlist[key].Save();

                    //    // Save current playback pos
                    //    if (_Zone_PlaybackPos.ContainsKey(key) && _ZonePlayer.ContainsKey(key))
                    //        _Zone_PlaybackPos[key] = _ZonePlayer[key].MediaInfo.PlaybackPos;

                    //    // Save playback pos to DB
                    //    if (_Zone_PlaybackPos.ContainsKey(key))
                    //        StoredData.Set(this, String.Format("{0}_PlaybackPos", key), _Zone_PlaybackPos[key].TotalSeconds.ToString().Replace(",", "."));

                    //    // Save playback mode to DB
                    //    if (_Zone_PlaybackMode.ContainsKey(key))
                    //    {
                    //        if (_Zone_PlaybackMode[key] == PlaybackModes.Play || _Zone_PlaybackMode[key] == PlaybackModes.Pause)
                    //            StoredData.Set(this, String.Format("{0}_PlaybackMode", key), (int)PlaybackModes.Restart);
                    //        else
                    //            StoredData.Set(this, String.Format("{0}_PlaybackMode", key), (int)_Zone_PlaybackMode[key]);
                    //    }
                    //}

                    // Dispose factory
                    _PlayerFactory.Dispose();

                    // Dispose players
                    foreach (var key in _ZonePlayer.Keys)
                        if (_ZonePlayer != null)
                            if (_ZonePlayer.ContainsKey(key))
                            {
                                _ZonePlayer[key].Stop();
                                _ZonePlayer[key].Dispose();
                            }
                }

                foreach (var key in _Zone_DelayedCommand.Keys)
                {
                        if (_Zone_DelayedCommand != null)
                            if (_Zone_DelayedCommand.ContainsKey(key))
                                _Zone_DelayedCommand[key].Dispose();
                }
            }
            catch 
            {
            }
            base.Dispose();
        }
        ~OMVLCPlayer()
        {
            //Dispose();
        }

        #endregion

        #region MediaProvider

        public override string MediaProviderCommand_Activate(Zone zone)
        {
            base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Stopped);
            base.MediaProviderData_ReportMediaText(zone, String.Format("Activating {0}", this.pluginName), "");

            // Restore playback mode from DB
            var storedPlaybackMode = StoredData.GetEnum<PlaybackModes>(this, String.Format("{0}_PlaybackMode", zone), PlaybackModes.Stop);
            if (!_Zone_PlaybackMode.ContainsKey(zone))
                _Zone_PlaybackMode.Add(zone, storedPlaybackMode);
            else
                _Zone_PlaybackMode[zone] = storedPlaybackMode;

            // Restore playback pos from DB
            var storedPlaybackPos = StoredData.GetFloat(this, String.Format("{0}_PlaybackPos", zone), 0);
            if (!_Zone_PlaybackPos.ContainsKey(zone))
                _Zone_PlaybackPos.Add(zone, storedPlaybackPos);
            else
                _Zone_PlaybackPos[zone] = storedPlaybackPos;

            // Restart any playing players
            if (_Zone_PlaybackMode[zone] == PlaybackModes.Restart)
                PlayerControl_Restart(zone, _Zone_PlaybackPos[zone]);

            return String.Empty;
        }

        public override string MediaProviderCommand_Deactivate(Zone zone)
        {
            base.MediaProviderData_ReportState_Playback(zone, PlaybackState.Stopped);
            base.MediaProviderData_ReportMediaText(zone, String.Format("Deactivating {0}", this.pluginName), "");

            SavePlayerStates(zone);

            MediaProviderCommand_Playback_Stop(zone, null);
            base.MediaProviderData_ReportMediaText(zone, String.Format("Deactivated {0}", this.pluginName), "");

            return String.Empty;
        }

        public override string MediaProviderCommand_Playback_Start(Zone zone, object[] param)
        {
            PlayerControl_DelayedPlay_Cancel(zone);

            mediaInfo media = null;

            // Process parameters
            if (param == null || param.Length == 0)
            {   // No parameters available, play current playlist item

                if (_Zone_PlaybackMode[zone] == PlaybackModes.Pause)
                {
                    _Zone_PlaybackMode[zone] = PlaybackModes.Play;
                    _ZonePlayer[zone].Pause();
                }
                else
                {
                    media = _Zone_Playlist[zone].CurrentItem;
                    base.MediaProviderData_ReportMediaInfo(zone, media);
                }
            }

            // Do we have a minimum of one parameter?
            else if (Params.IsParamsValid(param, 1))
            {   // Yes

                // Check for media info present
                if (param[0] is mediaInfo)
                {   // Play media info
                    media = Params.GetParam<mediaInfo>(param, 0);
                }

                else if (param[0] is int)
                {   // Play index in playlist
                    _Zone_Playlist[zone].CurrentIndex = (int)param[0];
                    media = _Zone_Playlist[zone].CurrentItem;
                    base.MediaProviderData_ReportMediaInfo(zone, media);
                }

                else if (param[0] is string)
                {   // Play media location directly
                    media = new mediaInfo(eMediaType.Local, Params.GetParam<string>(param, 0, ""));
                }

            }

            if (media != null)
            {
                return PlayerControl_Play(zone, media);
            }
            return "Invalid media";
        }

        public override Playlist MediaProviderData_GetPlaylist(Zone zone)
        {
            return _Zone_Playlist[zone];
        }

        public override Playlist MediaProviderData_SetPlaylist(Zone zone, Playlist playlist)
        {
            _Zone_Playlist[zone] = playlist;
            return _Zone_Playlist[zone];
        }

        public override string MediaProviderCommand_Playback_Stop(Zone zone, object[] param)
        {
            PlayerControl_DelayedPlay_Cancel(zone);

            if (_ZonePlayer.ContainsKey(zone))
            {
                _Zone_PlaybackMode[zone] = PlaybackModes.Stop;
                _ZonePlayer[zone].Stop();
            }
            return "";
        }

        public override string MediaProviderCommand_Playback_Pause(Zone zone, object[] param)
        {
            PlayerControl_DelayedPlay_Cancel(zone);

            if (_ZonePlayer.ContainsKey(zone))
            {
                _Zone_PlaybackMode[zone] = PlaybackModes.Pause;
                _ZonePlayer[zone].Pause();
            }
            return "";
        }

        public override string MediaProviderCommand_Playback_Next(Zone zone, object[] param)
        {
            TimeSpan timeSinceLastCommand = DateTime.Now - _Zone_TimeStampLastCommand[zone];
            _Zone_TimeStampLastCommand[zone] = DateTime.Now;

            // Abort any ongoing delayed commands
            PlayerControl_DelayedPlay_Cancel(zone);

            _Zone_Playlist[zone].GotoNextMedia();

            // Update media info
            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
            base.MediaProviderData_ReportMediaInfo(zone, media);

            // Start playback of new media
            if (_Zone_PlaybackMode[zone] == PlaybackModes.Play)
            {
                if (timeSinceLastCommand > new TimeSpan(0, 0, 0, 1, 0))
                {
                    PlayerControl_Play(zone, media);
                }
                else
                {
                    PlayerControl_DelayedPlay_Enable(zone, media);
                }
            }

            return "";
        }

        public override string MediaProviderCommand_Playback_Previous(Zone zone, object[] param)
        {
            TimeSpan timeSinceLastCommand = DateTime.Now - _Zone_TimeStampLastCommand[zone];
            _Zone_TimeStampLastCommand[zone] = DateTime.Now;

            // Abort any ongoing delayed commands
            PlayerControl_DelayedPlay_Cancel(zone);

            // Check if we are close enough to go to previous item, otherwise start at beginning of current media
            if (GetPlayer_CurrentPosition(zone) <= TimeSpan.FromSeconds(5))
                _Zone_Playlist[zone].GotoPreviousMedia();

            // Update media info
            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
            base.MediaProviderData_ReportMediaInfo(zone, media);

            // Start playback of new media
            if (_Zone_PlaybackMode[zone] == PlaybackModes.Play)
            {
                if (timeSinceLastCommand > new TimeSpan(0, 0, 0, 1, 0))
                    PlayerControl_Play(zone, media);
                else
                    PlayerControl_DelayedPlay_Enable(zone, media);
            }

            return "";
        }

        public override string MediaProviderCommand_Playback_SeekForward(Zone zone, object[] param)
        {
            if (!_ZonePlayer[zone].IsSeekable)
                return "media not seekable";

            if (Params.IsParamsValid(param, 1))
            {
                int seconds = Params.GetParam<int>(param, 0);
                try
                {
                    float percentage = ((float)seconds * 1000f) / (float)_ZonePlayer[zone].Length;
                    _ZonePlayer[zone].Position += percentage;
                    return "";
                }
                catch (Exception e)
                {
                    OM.Host.DebugMsg(this.pluginName, e);
                    return e.Message;
                }
            }
            else
            {   // Seek forward default length
                float percentage = (5f * 1000f) / (float)_ZonePlayer[zone].Length;
                _ZonePlayer[zone].Position += percentage;
                return "";
            }
        }

        public override string MediaProviderCommand_Playback_SeekBackward(Zone zone, object[] param)
        {
            if (!_ZonePlayer[zone].IsSeekable)
                return "media not seekable";

            if (Params.IsParamsValid(param, 1))
            {
                int seconds = Params.GetParam<int>(param, 0);
                try
                {
                    float percentage = ((float)seconds * 1000f) / (float)_ZonePlayer[zone].Length;
                    _ZonePlayer[zone].Position -= percentage;
                    return "";
                }
                catch (Exception e)
                {
                    OM.Host.DebugMsg(this.pluginName, e);
                    return e.Message;
                }
            }
            else
            {   // Seek forward default length of 5 seconds
                float percentage = (5f * 1000f) / (float)_ZonePlayer[zone].Length;
                _ZonePlayer[zone].Position -= percentage;
                return "";
            }
        }

        public override bool MediaProviderData_GetShuffle(Zone zone)
        {
            return _Zone_Playlist[zone].Shuffle;
        }

        public override bool MediaProviderData_SetShuffle(Zone zone, bool state)
        {
            _Zone_Playlist[zone].Shuffle = state;
            return _Zone_Playlist[zone].Shuffle;
        }

        public override bool MediaProviderData_GetRepeat(Zone zone)
        {
            return _Zone_Playlist[zone].Repeat;
        }

        public override bool MediaProviderData_SetRepeat(Zone zone, bool state)
        {
            _Zone_Playlist[zone].Repeat = state;
            return _Zone_Playlist[zone].Repeat;
        }

        public override MediaProviderAbilities MediaProviderData_GetMediaSourceAbilities(string mediaSource)
        {
            return new MediaProviderAbilities();
        }

        public override List<MediaSource> MediaProviderData_GetMediaSources()
        {
            return _MediaSources;
        }

        public override MediaSource MediaProviderData_GetMediaSource(Zone zone)
        {
            return _Zone_MediaSource[zone];
        }

        public override string MediaProviderCommand_ActivateMediaSource(Zone zone, string mediaSourceName)
        {
            return "";
        }

        #endregion

    }
}
