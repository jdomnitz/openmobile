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

namespace OMPlayer2
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows | OSConfigurationFlags.Linux)]
    public sealed class OMPlayer2 : MediaProviderCode
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

        #region Constructor / Plugin config

        public OMPlayer2()
            : base("OMPlayer2", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "OM Official Audio/Video player", "OM Dev team/Borte", "")
        {
            _VideoPlayers = new mPlayer[BuiltInComponents.Host.ScreenCount];
            _VideoWindows = new VideoWindow[BuiltInComponents.Host.ScreenCount];
        }

        #endregion

        public override eLoadStatus initialize(IPluginHost host)
        {
            if (Configuration.RunningOnMacOS)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnMediaEvent += new MediaEvent(host_OnMediaEvent);

            // Preload media players and connect them to the each zone (one player per zone, one video player per screen)
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
                for (int i = 0; i < BuiltInComponents.Host.ZoneHandler.Zones.Count; i++)
                    ZonePlayer_CheckInstance(BuiltInComponents.Host.ZoneHandler.Zones[i]);
            });

            // Preload videoplayers (one per screen)
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                VideoPlayer_CheckInstance(i);

            return eLoadStatus.LoadSuccessful;
        }

        /// <summary>
        /// Local instances of the mPlayer used as main player for zone. Each player is mapped to a zone and is set as a master sending sync data on the network
        /// </summary>
        private Dictionary<Zone, mPlayer> _ZonePlayers = new Dictionary<Zone, mPlayer>();

        /// <summary>
        /// Local instances of mPlayer used as video players (One player per screen, these are used as slaves to the zoneplayers)
        /// </summary>
        private mPlayer[] _VideoPlayers = null;

        /// <summary>
        /// Saved reference to the different video windows (OMTargetWindow)
        /// </summary>
        private VideoWindow[] _VideoWindows = null;

        private MediaProviderInfo _MediaProviderInfo = new MediaProviderInfo();
        public override MediaProviderInfo GetMediaProviderInfo(Zone zone)
        {
            return _MediaProviderInfo;
        }

        public override bool ExecuteCommand(Zone zone, MediaProvider_CommandWrapper command)
        {
            switch (command.Command)
            {
                case MediaProvider_Commands.Stop:
                    {  // Stop playback
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Stop();
                    }
                    return true;
                case MediaProvider_Commands.Play:
                    {   // Start playback
                        if (Params.IsParamsValid(command.Parameters, 1))
                        {
                            mediaInfo media = Params.GetParam<mediaInfo>(command.Parameters, 0);
                            try
                            {
                                ZonePlayer_CheckInstance(zone);

                                // Classify media source
                                switch (media.Type)
                                {
                                    default:
                                        {
                                            _ZonePlayers[zone].Identify = true;
                                            _ZonePlayers[zone].Start(true, true);
                                            _ZonePlayers[zone].PlayFile(media.Location);
                                        }
                                        return true;
                                    case eMediaType.AudioCD:
                                        {
                                            string Drive = "";
                                            if (Configuration.RunningOnWindows)
                                                Drive = media.Location.Substring(0, 2);
                                            //TODO: Add other platforms

                                            if (_ZonePlayers[zone].DiscDevice == null || !_ZonePlayers[zone].DiscDevice.Equals(Drive))
                                                _ZonePlayers[zone].DiscDevice = Drive;

                                            _ZonePlayers[zone].Identify = true;
                                            _ZonePlayers[zone].Start(true, true);

                                            _ZonePlayers[zone].PlayCD(media.TrackNumber);
                                        }
                                        return true;
                                    case eMediaType.HDDVD:
                                    case eMediaType.DVD:
                                        {
                                            string Drive = "";
                                            if (Configuration.RunningOnWindows)
                                                Drive = media.Location.Substring(0, 2);
                                            //TODO: Add other platforms

                                            if (_ZonePlayers[zone].DiscDevice == null || !_ZonePlayers[zone].DiscDevice.Equals(Drive))
                                                _ZonePlayers[zone].DiscDevice = Drive;

                                            //_AudioPlayers[zone].Identify = false;
                                            _ZonePlayers[zone].Start(true, true);

                                            bool discDeviceBusy = false;
                                            ExecuteOnAllPlayers((mPlayer player) =>
                                            {
                                                if (player.PlayerState != mPlayer.PlayerStates.Idle)
                                                    discDeviceBusy = true;
                                            });

                                            if (!discDeviceBusy)
                                                _ZonePlayers[zone].PlayDVD();
                                            else
                                                command.ErrorText = "Unable to play DVD!\nDVD drive is used by another zone/screen";
                                        }
                                        return true;
                                    case eMediaType.BluRay:
                                        {
                                            // Todo: find bluray drive
                                            _ZonePlayers[zone].PlayBluRay();
                                        }
                                        return true;
                                }
                            }
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                            }

                        }
                    }
                    return true;
                case MediaProvider_Commands.Pause:
                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Pause();
                    }
                    return true;
                case MediaProvider_Commands.PlaybackPosition_Set:
                    break;
                case MediaProvider_Commands.PlaybackPosition_Get:
                    break;
                case MediaProvider_Commands.Forward:
                    break;
                case MediaProvider_Commands.Backward:
                    break;
                case MediaProvider_Commands.StepForward:
                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Next();
                    }
                    return true;
                case MediaProvider_Commands.StepBackward:
                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].Previous();
                    }
                    return true;
                case MediaProvider_Commands.SubTitle_Cycle:
                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].SubTitles_Cycle();
                    }
                    return true;

                case MediaProvider_Commands.SubTitle_Disable:
                    {
                        if (_ZonePlayers.ContainsKey(zone))
                            _ZonePlayers[zone].SubTitles_Disable();
                    }
                    return true;

                case MediaProvider_Commands.SetVideoTarget:
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
                case MediaProvider_Commands.Custom:
                    break;
                default:
                    break;
            }

            return false;
        }

        public override object GetData(Zone zone, MediaProvider_DataWrapper data)
        {
            return null;
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

            foreach (mPlayer player in _ZonePlayers.Values)
                function(player);
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
                    _ZonePlayers.Add(zone, ZonePlayer_Create(zone, basePort));
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
                        ZonePlayer_Configure(_ZonePlayers[zone], _VideoWindows[screens[0]]);

                // Set rest of screens as video slaves
                if (screens.Length > 1)
                {
                    for (int i = 1; i < screens.Length; i++)
                    {
                        VideoPlayer_CheckInstance(i);
                        if (_VideoWindows[i] != null)
                            VideoPlayer_Configure(_VideoPlayers[i], _ZonePlayers[zone], _VideoWindows[i], i);
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

        void player_OnPlaybackStopped(mPlayer sender)
        {

        }

        void player_OnPlaybackStarted(mPlayer sender)
        {

        }

        void player_OnPlaybackPaused(mPlayer sender)
        {

        }

        void player_OnMediaInfoUpdated(mPlayer sender)
        {

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

        #region MediaProvider control

        public override bool ActivateMediaProvider(Zone zone)
        {
            ZonePlayer_CheckInstance(zone);
            return base.ActivateMediaProvider(zone);
        }

        public override bool DeactivateMediaProvider(Zone zone)
        {
            // Stop playback
            if (_ZonePlayers.ContainsKey(zone))
                _ZonePlayers[zone].Stop();

            return base.DeactivateMediaProvider(zone);
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
                            _ZonePlayers[key].Dispose();

                // Dispose videoplayers
                foreach (mPlayer player in _VideoPlayers)
                    player.Dispose();
            }
            catch { }
            base.Dispose();
        }
        ~OMPlayer2()
        {
            Dispose();
        }

        #endregion

    }
}
