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
using OpenMobile.mPlayer;
using OpenMobile.Controls;
using System.Drawing;

namespace OMPlayer2
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows | OSConfigurationFlags.Linux)]
    public sealed class OMWinPlayer : MediaProviderBase
    {
        private class VideoWindow
        {
            public OMTargetWindow TargetWindow { get; set; }
            public mPlayer ActivePlayer { get; set; }

            public VideoWindow(OMTargetWindow targetWindow, mPlayer activePlayer)
            {
                this.TargetWindow = targetWindow;
                this.ActivePlayer = activePlayer;
            }
        }

        private enum PlaybackModes
        {
            Stop,
            Play,
            Pause
        }

        private class mPlayerWrapper
        {
            public mPlayer Player { get; set; }

            public PlaybackModes RequestedPlayBackMode { get; set; }

            public Timer tmrDelayedCommand { get; set; }

            public mPlayerWrapper(mPlayer player)
            {
                this.Player = player;
            }
        }

        MediaSource _MediaSource_File;

        #region Constructor / Plugin config

        public OMWinPlayer()
            : base("OMPlayer2", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "OM Official Audio/Video player", "OM Dev team/Borte", "")
        {
            _VideoPlayers = new mPlayer[BuiltInComponents.Host.ScreenCount];
            _VideoWindows = new VideoWindow[BuiltInComponents.Host.ScreenCount];

            // Kill any active mPlayers at startup
            OpenMobile.Framework.OSSpecific.Process_Close("mplayer");
        }

        #endregion

        public override eLoadStatus initialize(IPluginHost host)
        {
            if (Configuration.RunningOnMacOS)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnMediaEvent += new MediaEvent(host_OnMediaEvent);

            //// Preload media players and connect them to the each zone (one player per zone, one video player per screen)
            //OpenMobile.Threading.SafeThread.Asynchronous(() =>
            //{
            //    for (int i = 0; i < BuiltInComponents.Host.ZoneHandler.Zones.Count; i++)
            //        ZonePlayer_CheckInstance(BuiltInComponents.Host.ZoneHandler.Zones[i]);
            //});

            // Preload videoplayers (one per screen)
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                    VideoPlayer_CheckInstance(i);
            });

            // Initialize media sources
            _MediaSource_File = new MediaSource(OM.Host.getSkinImage("Icons|Icon-File").image, "File");

            // List this source as available to the host
            base.MediaSource_RegisterNew(_MediaSource_File);

            return eLoadStatus.LoadSuccessful;
        }

        /// <summary>
        /// Local instances of the mPlayer used as main player for zone. Each player is mapped to a zone and is set as a master sending sync data on the network
        /// </summary>
        private Dictionary<Zone, mPlayerWrapper> _ZonePlayers = new Dictionary<Zone, mPlayerWrapper>();

        /// <summary>
        /// Local instances of mPlayer used as video players (One player per screen, these are used as slaves to the zoneplayers)
        /// </summary>
        private mPlayer[] _VideoPlayers = null;

        /// <summary>
        /// Saved reference to the different video windows (OMTargetWindow)
        /// </summary>
        private VideoWindow[] _VideoWindows = null;

        public override bool ExecuteCommand(Zone zone, MediaProvider_CommandWrapper command)
        {
            switch (command.Command)
            {
                case MediaProvider_Commands.Stop:
                    #region Stop

                    {  // Stop playback
                        _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Stop;
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Player.Stop();
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.Play:
                    #region Play

                    {   // Start playback
                        mediaInfo media = null;
                        if (Params.IsParamsValid(command.Parameters, 1))
                        {   // Specific media
                            if (command.Parameters[0] is mediaInfo)
                                media = Params.GetParam<mediaInfo>(command.Parameters, 0);
                            else if (command.Parameters[0] is int)
                            {   // Play index in playlist
                                base.GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist.SetCurrentStartIndex(Params.GetParam<int>(command.Parameters, 0));
                                media = base.GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist.CurrentItem;
                            }
                        }
                        else
                        {
                            media = base.GetZoneSpecificDataInstance(zone).ProviderInfo.MediaSource.Playlist.CurrentItem;
                        }

                        if (media != null)
                        {
                            try
                            {
                                ZonePlayer_CheckInstance(zone);

                                // Update media info data
                                base.MediaInfo_Set(zone, media);

                                // Classify media source
                                switch (media.Type)
                                {
                                    default:
                                        {
                                            _ZonePlayers[zone].Player.Identify = true;
                                            _ZonePlayers[zone].Player.Start(true, true);
                                            if (_ZonePlayers[zone].RequestedPlayBackMode == PlaybackModes.Pause)
                                            {
                                                _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Play;
                                                _ZonePlayers[zone].Player.Pause();
                                            }
                                            else
                                            {
                                                _ZonePlayers[zone].Player.PlayFile(media.Location);
                                            }
                                            _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Play;
                                        }
                                        return true;
                                    case eMediaType.AudioCD:
                                        {
                                            string Drive = "";
                                            if (Configuration.RunningOnWindows)
                                                Drive = media.Location.Substring(0, 2);
                                            //TODO: Add other platforms

                                            if (_ZonePlayers[zone].Player.DiscDevice == null || !_ZonePlayers[zone].Player.DiscDevice.Equals(Drive))
                                                _ZonePlayers[zone].Player.DiscDevice = Drive;

                                            _ZonePlayers[zone].Player.Identify = true;
                                            _ZonePlayers[zone].Player.Start(true, true);

                                            _ZonePlayers[zone].Player.PlayCD(media.TrackNumber);
                                            _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Play;
                                        }
                                        return true;
                                    case eMediaType.HDDVD:
                                    case eMediaType.DVD:
                                        {
                                            string Drive = "";
                                            if (Configuration.RunningOnWindows)
                                                Drive = media.Location.Substring(0, 2);
                                            //TODO: Add other platforms

                                            if (_ZonePlayers[zone].Player.DiscDevice == null || !_ZonePlayers[zone].Player.DiscDevice.Equals(Drive))
                                                _ZonePlayers[zone].Player.DiscDevice = Drive;

                                            //_AudioPlayers[zone].Identify = false;
                                            _ZonePlayers[zone].Player.Start(true, true);

                                            bool discDeviceBusy = false;
                                            ExecuteOnAllPlayers((mPlayer player) =>
                                            {
                                                if (player.PlayerState != mPlayer.PlayerStates.Idle)
                                                    discDeviceBusy = true;
                                            });

                                            if (!discDeviceBusy)
                                                _ZonePlayers[zone].Player.PlayDVD();
                                            else
                                                command.ErrorText = "Unable to play DVD!\nDVD drive is used by another zone/screen";
                                            _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Play;
                                        }
                                        return true;
                                    case eMediaType.BluRay:
                                        {
                                            // Todo: find bluray drive
                                            _ZonePlayers[zone].Player.PlayBluRay();
                                            _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Play;
                                        }
                                        return true;
                                }
                            }
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                                return false;
                            }
                        }
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.Pause:
                    #region Pause

                    {
                        _ZonePlayers[zone].RequestedPlayBackMode = PlaybackModes.Pause;
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Player.Pause();
                    }
                    return true;
 
                    #endregion
                case MediaProvider_Commands.PlaybackPosition_Set:
                    #region PlaybackPosition_Set

                    break;

                    #endregion
                case MediaProvider_Commands.PlaybackPosition_Get:
                    #region PlaybackPosition_Get

                    break;

                    #endregion
                case MediaProvider_Commands.Forward:
                    #region Forward
                    {   // Seek forward the specified seconds
                        if (Params.IsParamsValid(command.Parameters, 1))
                        {
                            int seconds = Params.GetParam<int>(command.Parameters, 0);
                            try
                            {
                                ZonePlayer_CheckInstance(zone);
                                _ZonePlayers[zone].Player.SeekFwd(seconds);
                                return true;
                            }                            
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                                return false;
                            }
                        }
                    }
                    break;

                    #endregion
                case MediaProvider_Commands.Backward:
                    #region Backward
                    {   // Seek backwards the specified seconds
                        if (Params.IsParamsValid(command.Parameters, 1))
                        {
                            int seconds = Params.GetParam<int>(command.Parameters, 0);
                            try
                            {
                                ZonePlayer_CheckInstance(zone);
                                _ZonePlayers[zone].Player.SeekBwd(seconds);
                                return true;
                            }
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                                return false;
                            }
                        }
                    }
                    break;

                    #endregion
                case MediaProvider_Commands.StepForward:
                    #region StepForward

                    {
                        MediaProviderInfo providerInfo = base.GetZoneSpecificDataInstance(zone).ProviderInfo;
                        if (providerInfo.MediaSource == _MediaSource_File)
                        {
                            if (providerInfo.MediaSource.Playlist != null)
                            {
                                providerInfo.MediaSource.Playlist.GotoNextMedia();
                            }
                            if (_ZonePlayers[zone].RequestedPlayBackMode == PlaybackModes.Play)
                                DelayedCommand_Set(zone, 500, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play));                                
                        }
                        else
                            if (_ZonePlayers.ContainsKey(zone))
                                _ZonePlayers[zone].Player.Next();
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.StepBackward:
                    #region StepBackward

                    {
                        MediaProviderInfo providerInfo = base.GetZoneSpecificDataInstance(zone).ProviderInfo;
                        if (providerInfo.MediaSource == _MediaSource_File)
                        {
                            if (providerInfo.MediaSource.Playlist != null)
                            {
                                providerInfo.MediaSource.Playlist.GotoPreviousMedia();
                            }
                            if (_ZonePlayers[zone].RequestedPlayBackMode == PlaybackModes.Play)
                                DelayedCommand_Set(zone, 500, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play));
                        }
                        else
                            if (_ZonePlayers.ContainsKey(zone))
                                _ZonePlayers[zone].Player.Previous();
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.SubTitle_Cycle:
                    #region SubTitle_Cycle

                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Player.SubTitles_Cycle();
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.SubTitle_Disable:
                    #region SubTitle_Disable

                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Player.SubTitles_Disable();
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.SetVideoTarget:
                    #region SetVideoTarget

                    {   // Set video target for the player
                        if (Params.IsParamsValid(command.Parameters, 2))
                        {
                            OMTargetWindow targetWindow = Params.GetParam<OMTargetWindow>(command.Parameters, 0, null);
                            int screen = Params.GetParam<int>(command.Parameters, 1);
                            lock (_VideoWindows)
                            {
                                if (targetWindow != null)
                                    _VideoWindows[screen] = new VideoWindow(targetWindow, null);
                            }
                            // Configure players
                            ZonePlayer_ConfigureVideoPlayers(zone);
                        }
                    }
                    return true;

                    #endregion
                case MediaProvider_Commands.Suffle:
                    #region Suffle
                    {
                        MediaProviderInfo providerInfo = base.GetZoneSpecificDataInstance(zone).ProviderInfo;
                        if (providerInfo.MediaSource == _MediaSource_File)
                        {
                            if (providerInfo.MediaSource.Playlist != null)
                            {
                                if (Params.IsParamsValid(command.Parameters, 1))
                                {   // Specific value
                                    providerInfo.MediaSource.Playlist.Random = Params.GetParam<bool>(command.Parameters, 0);
                                }
                                else
                                {
                                    providerInfo.MediaSource.Playlist.Random = !providerInfo.MediaSource.Playlist.Random;
                                }
                            }
                        }
                        base.Raise_OnDataChanged(zone, MediaProvider_Data.Suffle, providerInfo.MediaSource.Playlist.Random);
                    }
                    break;

                    #endregion
                case MediaProvider_Commands.Repeat:
                    #region Repeat
                    {
                        MediaProviderInfo providerInfo = base.GetZoneSpecificDataInstance(zone).ProviderInfo;
                        if (providerInfo.MediaSource.Playlist != null)
                        {
                            if (Params.IsParamsValid(command.Parameters, 1))
                            {   // Specific value
                                providerInfo.MediaSource.Playlist.Repeat = Params.GetParam<bool>(command.Parameters, 0);
                            }
                            else
                            {
                                providerInfo.MediaSource.Playlist.Repeat = !providerInfo.MediaSource.Playlist.Repeat;
                            }
                        }
                        base.Raise_OnDataChanged(zone, MediaProvider_Data.Repeat, providerInfo.MediaSource.Playlist.Repeat);
                    }
                    break;

                    #endregion
                case MediaProvider_Commands.PlayList_Set:
                    #region PlayList_Set

                    {
                        MediaProviderInfo providerInfo = base.GetZoneSpecificDataInstance(zone).ProviderInfo;
                        if (providerInfo.MediaSource == _MediaSource_File)
                        {
                            if (Params.IsParamsValid(command.Parameters, 1))
                            {   // Specific media
                                providerInfo.MediaSource.Playlist = Params.GetParam<PlayList2>(command.Parameters, 0);
                            }
                        }
                    }
                    break;

                    #endregion
                case MediaProvider_Commands.Custom:
                    #region Custom

                    break;

                    #endregion
                default:
                    break;
            }

            return false;
        }

        private void DelayedCommand_Set(Zone zone, int delay, MediaProvider_CommandWrapper command)
        {
            // Create new timer?
            if (_ZonePlayers[zone].tmrDelayedCommand == null || (MediaProvider_CommandWrapper)_ZonePlayers[zone].tmrDelayedCommand.Tag != command)
            {   // Yes
                _ZonePlayers[zone].tmrDelayedCommand = new Timer(delay);
                _ZonePlayers[zone].tmrDelayedCommand.AutoReset = false;
                _ZonePlayers[zone].tmrDelayedCommand.Tag = command;
                _ZonePlayers[zone].tmrDelayedCommand.Tag2 = zone;
                _ZonePlayers[zone].tmrDelayedCommand.Enabled = true;
                _ZonePlayers[zone].tmrDelayedCommand.Elapsed += new System.Timers.ElapsedEventHandler(tmrDelayedCommand_Elapsed);

            }
            else
            {   // No, Extend delay of current timer
                _ZonePlayers[zone].tmrDelayedCommand.Enabled = false;
                _ZonePlayers[zone].tmrDelayedCommand.Enabled = true;
            }
        }

        void tmrDelayedCommand_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer tmr = sender as Timer;
            if (tmr.Tag != null && tmr.Tag2 != null)
                ExecuteCommand((Zone)tmr.Tag2, (MediaProvider_CommandWrapper)tmr.Tag);
        }

        #region Host events

        void host_OnMediaEvent(eFunction function, Zone zone, string arg)
        {
            if (function == eFunction.ZoneSetActive || function == eFunction.ZoneAdded)
            {
                int screen = -1;
                if (int.TryParse(arg, out screen))
                {
                    OpenMobile.Threading.SafeThread.Asynchronous(() =>
                        {
                            ZonePlayer_CheckInstance(zone);
                            ZonePlayer_ConfigureVideoPlayers(zone);
                        });
                }
            }
        }

        void host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.pluginLoadingComplete)
            {
            }
        }

        #endregion

        /// <summary>
        /// Is the current disc device in use on another player?
        /// </summary>
        /// <returns></returns>
        private bool IsDiscDeviceBusy()
        {
            bool discDeviceBusy = false;
            ExecuteOnAllPlayers((mPlayer player) =>
            {
                if (player.PlayerState != mPlayer.PlayerStates.Idle)
                    discDeviceBusy = true;
            });
            return discDeviceBusy;
        }

        #region Player events

        void player_OnVideoPresent(mPlayer sender)
        {
            Zone zone = sender.Tag as Zone;
            if (zone != null)
                ZonePlayer_ConfigureVideoPlayers(zone);

            switch (sender.MediaInfo.MediaType)
            {
                case mPlayer.MediaTypes.None:
                    break;
                case mPlayer.MediaTypes.File:
                    ExecuteOnVideoPlayers(sender, (mPlayer player) => { player.PlayFile(sender.MediaInfo.Url); });
                    break;
                //case mPlayer.MediaTypes.CD:
                //    {
                //        if (!IsDiscDeviceBusy())
                //        {
                //            ExecuteOnVideoPlayers(sender, (mPlayer player) =>
                //            {
                //                player.PlayCD(sender.MediaInfo.Track);
                //            });
                //        }
                //    }
                //    break;
                //case mPlayer.MediaTypes.DVD:
                //    {
                //        if (!IsDiscDeviceBusy())
                //        {
                //            ExecuteOnVideoPlayers(sender, (mPlayer player) =>
                //            {
                //                if (player.DiscDevice != sender.DiscDevice)
                //                {
                //                    player.DiscDevice = sender.DiscDevice;
                //                    player.Start(true, true);
                //                }
                //                player.PlayDVD();
                //            });
                //        }
                //    }
                //    break;
                //case mPlayer.MediaTypes.BluRay:
                //    {
                //        if (!IsDiscDeviceBusy())
                //        {
                //            ExecuteOnVideoPlayers(sender, (mPlayer player) => { player.PlayBluRay(); });
                //        }
                //    }
                //    break;
                default:
                    break;
            }
        }

        #endregion

        private delegate void Function(mPlayer player);

        /// <summary>
        /// Executes code on the slave video players
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="function"></param>
        private void ExecuteOnVideoPlayers(mPlayer sender, Function function)
        {
            int[] screens = BuiltInComponents.Host.ZoneHandler.GetScreensForActiveZone(sender.Tag as Zone);
            foreach (int screen in screens)
            {
                if (_VideoPlayers[screen].Tag != null && _VideoPlayers[screen].Tag == sender)
                    function(_VideoPlayers[screen]);
            }
        }

        /// <summary>
        /// Exacutes code on all player instances (both ZonePlayers and VideoPlayers)
        /// </summary>
        /// <param name="function"></param>
        private void ExecuteOnAllPlayers(Function function)
        {
            foreach (mPlayer player in _VideoPlayers)
                function(player);

            foreach (mPlayerWrapper player in _ZonePlayers.Values)
                function(player.Player);
        }

        #region Player instance control

        private void ZonePlayer_CheckInstance(Zone zone)
        {
            // Don't create instances for zone's with subzones
            if (zone.HasSubZones)
                return;

            // Initialize new instance of player for this zone (if not already done)
            lock (_ZonePlayers)
            {
                if (!_ZonePlayers.ContainsKey(zone))
                {   // Player does not exist, create a new zonePlayer
                    int basePort = 24000 + ((BuiltInComponents.Host.ScreenCount + 1) * _ZonePlayers.Count);
                    _ZonePlayers.Add(zone, new mPlayerWrapper(ZonePlayer_Create(zone, basePort)));
                }
            }
        }

        private void ZonePlayer_ConfigureVideoPlayers(Zone zone)
        {
            // Check if instace is created
            ZonePlayer_CheckInstance(zone);

            // Ensure player exists
            if (_ZonePlayers.ContainsKey(zone))
            {
                // Get the screens this zone is active on
                int[] screens = BuiltInComponents.Host.ZoneHandler.GetScreensForActiveZone(zone);

                // Find first screen this zone is active on and use this as the main video target
                if (screens.Length > 0)
                    if (_VideoWindows[screens[0]] != null)
                        ZonePlayer_Configure(_ZonePlayers[zone].Player, _VideoWindows[screens[0]]);

                // Set rest of screens as video slaves
                if (screens.Length > 1)
                {
                    for (int i = 1; i < screens.Length; i++)
                    {
                        VideoPlayer_CheckInstance(i);
                        if (_VideoWindows[i] != null)
                            VideoPlayer_Configure(_VideoPlayers[i], _ZonePlayers[zone].Player, _VideoWindows[i], i);
                    }
                }
            }
        }

        private mPlayer ZonePlayer_Create(Zone zone, int syncBasePort)
        {
            if (_Disposing)
                return null;

            // Init new player
            mPlayer player = new mPlayer(String.Format("ZonePlayer_{0}", zone.Name));

            // Set audio system
            if (Configuration.RunningOnWindows)
                player.AudioMode = MPlayerAudioModeName.DirectSound;
            else if (Configuration.RunningOnLinux)
                player.AudioMode = MPlayerAudioModeName.Alsa;

            // Set audio device
            if (!zone.AudioDevice.IsDefault)
                player.AudioDevice = zone.AudioDevice.Name;
            else
                player.AudioDevice = "";

            // Set video system
            player.VideoMode = MPlayerVideoModeName.GL;

            // Create master sync zone 
            player.SetSyncMaster(syncBasePort, BuiltInComponents.Host.ScreenCount);

            // Save a reference to the zone this player belongs to
            player.Tag = zone;

            // Connect events
            player.OnVideoPresent += new mPlayer.PlayerEventHandler(player_OnVideoPresent);
            player.OnMediaInfoUpdated += new mPlayer.PlayerEventHandler(player_OnMediaInfoUpdated);
            player.OnPlaybackPaused += new mPlayer.PlayerEventHandler(player_OnPlaybackPaused);
            player.OnPlaybackStarted += new mPlayer.PlayerEventHandler(player_OnPlaybackStarted);
            player.OnPlaybackStopped += new mPlayer.PlayerEventHandler(player_OnPlaybackStopped);

            // Let's start the player
            player.Start(true, true);

            return player;
        }

        private void ZonePlayer_Configure(mPlayer player, VideoWindow videoWindow)
        {
            lock (_ZonePlayers)
            {
                // Only reconfigure if needed
                if (player.WindowHandle != videoWindow.TargetWindow.Handle || player != videoWindow.ActivePlayer)
                {
                    // Stop any playback on the currently attached player
                    if (videoWindow.ActivePlayer != null && player != videoWindow.ActivePlayer)
                    {
                        videoWindow.ActivePlayer.Stop();
                        videoWindow.ActivePlayer.WindowHandle = IntPtr.Zero;
                    }

                    // Save reference to this player in the targetwindow
                    videoWindow.ActivePlayer = player;

                    // Set video target 
                    player.WindowHandle = videoWindow.TargetWindow.Handle;

                    // Let's start the player
                    player.Start(true, true);
                }
            }
        }

        private void VideoPlayer_CheckInstance(int screen)
        {
            lock (_VideoPlayers)
            {
                if (_VideoPlayers[screen] == null)
                    _VideoPlayers[screen] = VideoPlayer_Create(screen);
            }
        }

        private mPlayer VideoPlayer_Create(int screen)
        {
            if (_Disposing)
                return null;

            // Init new player
            mPlayer player = new mPlayer(String.Format("VideoSlave_{0}", screen), false);

            // Block audio output as this comes from the zonePlayer
            player.AudioMode = MPlayerAudioModeName.Null;

            // Set video system
            player.VideoMode = MPlayerVideoModeName.GL;

            // Disable the identify features of the player (this is enabled on the master)
            player.Identify = false;

            // Let's start the player
            player.Start(true, true);

            return player;
        }

        private void VideoPlayer_Configure(mPlayer player, mPlayer masterPlayer, VideoWindow videoWindow, int screen)
        {
            lock (_VideoPlayers)
            {
                // Only reconfigure if needed
                if (player.WindowHandle != videoWindow.TargetWindow.Handle || player != videoWindow.ActivePlayer)
                {
                    // Stop any playback on the currently attached player
                    if (videoWindow.ActivePlayer != null)
                    {
                        videoWindow.ActivePlayer.Stop();
                        videoWindow.ActivePlayer.WindowHandle = IntPtr.Zero;

                        // Save reference to this player in the targetwindow
                        videoWindow.ActivePlayer = player;
                    }

                    // Set video target 
                    player.WindowHandle = videoWindow.TargetWindow.Handle;
                }

                // Set as sync slave 
                player.SetSyncSlave(masterPlayer.SyncBasePort, screen);

                // Save a reference to the master this player belongs to
                player.Tag = masterPlayer;

                // Set the disc device
                player.DiscDevice = masterPlayer.DiscDevice;

                // Let's start the player
                player.Start(true, true);

                // Check if master is already playing
                if (masterPlayer.MediaInfo.HasVideo && masterPlayer.PlayerState == mPlayer.PlayerStates.Paused || masterPlayer.PlayerState == mPlayer.PlayerStates.Playing)
                {   // Master is active, let's load media
                    switch (masterPlayer.MediaInfo.MediaType)
                    {
                        case mPlayer.MediaTypes.None:
                            break;
                        case mPlayer.MediaTypes.File:
                            player.PlayFile(masterPlayer.MediaInfo.Url);
                            break;
                        case mPlayer.MediaTypes.CD:
                            // Disc media doesn't support playback on multiple devices
                            //player.PlayCD(masterPlayer.MediaInfo.Track);
                            break;
                        case mPlayer.MediaTypes.DVD:
                            // Disc media doesn't support playback on multiple devices
                            //player.PlayDVD();
                            break;
                        case mPlayer.MediaTypes.BluRay:
                            // Disc media doesn't support playback on multiple devices
                            //player.PlayBluRay();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /*
        private mPlayer CreateVideoPlayer(Zone zone, mPlayer masterPlayer, IntPtr handle, int screen)
        {
            // Init new player
            mPlayer player = new mPlayer(String.Format("VideoSlave_{0}_", screen));

            // Block audio output as this comes from the audio player
            player.AudioMode = MPlayerAudioModeName.Null;

            // Set video system
            player.VideoMode = MPlayerVideoModeName.GL;

            // Set video target 
            player.WindowHandle = handle;

            // Set as sync slave 
            player.SetSyncSlave(masterPlayer.SyncBasePort, screen);

            // Save a reference to the master this player belongs to
            player.Tag = masterPlayer;

            // Set the reference to the disc player assosiated with this player
            player.DiscDevice = masterPlayer.DiscDevice;

            // Disable the identify features of the player (this is enabled on the master)
            player.Identify = false;

            // Let's start the player
            player.Start(true, true);

            // Check if master is already playing
            if (masterPlayer.MediaInfo.HasVideo && masterPlayer.PlayerState == mPlayer.PlayerStates.Paused || masterPlayer.PlayerState == mPlayer.PlayerStates.Playing)
            {   // Master is active, let's load media
                switch (masterPlayer.MediaInfo.MediaType)
                {
                    case mPlayer.MediaTypes.None:
                        break;
                    case mPlayer.MediaTypes.File:
                        player.PlayFile(masterPlayer.MediaInfo.Url);
                        break;
                    case mPlayer.MediaTypes.CD:
                        player.PlayCD(masterPlayer.MediaInfo.Track);
                        break;
                    case mPlayer.MediaTypes.DVD:
                        player.PlayDVD();
                        break;
                    case mPlayer.MediaTypes.BluRay:
                        player.PlayBluRay();
                        break;
                    default:
                        break;
                }
            }

            return player;
        }
*/
        #endregion

        #region Player events

        void player_OnPlaybackStopped(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;

            // move to next track in playlist
            if (_ZonePlayers.ContainsKey(zone) && _ZonePlayers[zone].RequestedPlayBackMode == PlaybackModes.Play)
            {
                ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.StepForward));
                System.Threading.Thread.Sleep(50);
                ExecuteCommand(zone, new MediaProvider_CommandWrapper(MediaProvider_Commands.Play));
            }

            base.MediaInfo_PlayBackState_Set((Zone)sender.Tag, MediaProvider_PlaybackState.Stopped);

            // Clear media info
            base.MediaInfo_Clear(zone);
            base.MediaInfo_MediaText_Set(zone, String.Empty, String.Empty);
        }

        void player_OnPlaybackStarted(mPlayer sender)
        {
            Zone zone = (Zone)sender.Tag;
            base.MediaInfo_PlayBackState_Set(zone, MediaProvider_PlaybackState.Playing);
        }

        void player_OnPlaybackPaused(mPlayer sender)
        {
            base.MediaInfo_PlayBackState_Set((Zone)sender.Tag, MediaProvider_PlaybackState.Paused);
        }

        void player_OnMediaInfoUpdated(mPlayer sender)
        {
            if (_ZonePlayers.ContainsKey((Zone)sender.Tag))
            {
                Zone zone = (Zone)sender.Tag;

                // Update missing media info
                switch (sender.MediaInfo.MediaType)
	            {
                    case mPlayer.MediaTypes.Stream_HTTP:
                        base.MediaInfo_UpdateMissingInfo(zone, sender.MediaInfo.Genre, sender.MediaInfo.Comment, sender.MediaInfo.Title, sender.MediaInfo.Genre, (int)sender.MediaInfo.Length.TotalSeconds, sender.MediaInfo.Track);
                        break;
                    default:
                        base.MediaInfo_UpdateMissingInfo(zone, sender.MediaInfo.Album, sender.MediaInfo.Artist, sender.MediaInfo.Title, sender.MediaInfo.Genre, (int)sender.MediaInfo.Length.TotalSeconds, sender.MediaInfo.Track);
                        break;
	            }

                // Update playback position data
                base.MediaInfo_PlaybackPositionData_Set(zone, sender.MediaInfo.PlaybackPos, sender.MediaInfo.Length);

                // Also update the provider info fields
                mediaInfo media = base.GetZoneSpecificDataInstance(zone).ProviderInfo.MediaInfo;
                switch (media.Type)
                {
                    case eMediaType.AudioCD:
                    case eMediaType.DVD:
                    case eMediaType.BluRay:
                        base.MediaInfo_MediaText_Set(zone, String.Format("{0}", media.Name), String.Format("{0}", media.TrackNumber));
                        break;
                    default:
                        base.MediaInfo_MediaText_Set(zone, String.Format("{0}", media.Name), String.Format("{0} - {1}", media.Artist, media.Album));
                        break;
                }
                UpdateMissingCoverImage(media);
            }
        }

        /// <summary>
        /// Updates media info with an image that's loaded from the plugin folder if not already present
        /// </summary>
        /// <param name="media"></param>
        private void UpdateMissingCoverImage(mediaInfo media)
        {
            // Try to find local cover images if image is missing (loaded from plugin folder)
            if (media.coverArt == null || media.coverArt.image == null)
            {
                if (!String.IsNullOrEmpty(media.Name) && _LastMissingCoverImageSearchText != media.Name)
                {
                    media.coverArt = OM.Host.getPluginImage(this, String.Format("CoverArt|{0}", media.Name)).image;

                    if (media.coverArt == null || media.coverArt.image == null)
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Unable to find local media image in folder '{0}\\CoverArt\\' named '{1}'", 
                            OpenMobile.helperFunctions.Plugins.Plugins.GetPluginFolder(this), media.Name));
                        media.coverArt = OM.Host.getSkinImage("Icons|Icon-Stream").image;
                    }

                    _LastMissingCoverImageSearchText = media.Name;
                }
            }
        }
        private string _LastMissingCoverImageSearchText = String.Empty;

        #endregion

        #region MediaProvider control

        public override MediaProviderState ActivateMediaProvider(Zone zone)
        {
            ZonePlayer_CheckInstance(zone);

            bool PlayerReady = false;
            string PlayerLoadState = "";
            switch (_ZonePlayers[zone].Player.PlayerLoadState)
            {
                case mPlayer.PlayerLoadStates.NotReady:
                    PlayerLoadState = "Not ready";
                    break;
                case mPlayer.PlayerLoadStates.Ready:
                    PlayerLoadState = "";
                    PlayerReady = true;
                    break;
                case mPlayer.PlayerLoadStates.Failed_mPlayerFileNotAvailable:
                    PlayerLoadState = String.Format(@"mPlayer exe not found at ""{0}""", _ZonePlayers[zone].Player.mPlayerFileName);
                    break;
                case mPlayer.PlayerLoadStates.Failed_mPlayerUnableToStart:
                    PlayerLoadState = "Unable to start mPlayer process";
                    break;
                default:
                    break;
            }

            // Activate default media source (or last used)
            base.MediaSource_Set(zone, _MediaSources[0]);

            return new MediaProviderState(PlayerReady, PlayerLoadState);
        }

        public override MediaProviderState DeactivateMediaProvider(Zone zone)
        {
            // Stop playback
            if (_ZonePlayers.ContainsKey(zone))
                _ZonePlayers[zone].Player.Stop();

            return new MediaProviderState(false, String.Empty);
        }

        /// <summary>
        /// Report the supported provider types
        /// </summary>
        public override MediaProviderTypes MediaProviderType
        {
            get { return MediaProviderTypes.AudioPlayback | MediaProviderTypes.VideoPlayback; }
        }

        #endregion

        #region Dispose
        private bool _Disposing = false;
        public override void Dispose()
        {
            _Disposing = true;
            try
            {
                // Dispose players
                foreach (var key in _ZonePlayers.Keys)
                    if (_ZonePlayers != null)
                        if (_ZonePlayers.ContainsKey(key))
                            _ZonePlayers[key].Player.Dispose();

                // Dispose videoplayers
                foreach (mPlayer player in _VideoPlayers)
                    player.Dispose();
            }
            catch { }
            base.Dispose();
        }
        ~OMWinPlayer()
        {
            Dispose();
        }

        #endregion

    }
}
