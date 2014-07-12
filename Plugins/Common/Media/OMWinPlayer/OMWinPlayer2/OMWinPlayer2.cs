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
using OpenMobile.Graphics;

namespace OMWinPlayer
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows)]
    public sealed class OMWinPlayer2 : BasePluginCode
    {
        private enum PlaybackModes
        {
            Stop,
            Play,
            Pause,
            Restart
        }

        // Zone specific data
        private Dictionary<Zone, mPlayer> _ZonePlayer = new Dictionary<Zone, mPlayer>();
        private Dictionary<Zone, CommandGroup> _Zone_CommandGroup_Media = new Dictionary<Zone, CommandGroup>();
        private Dictionary<Zone, DataSourceGroup> _Zone_DataSourceGroup_Media = new Dictionary<Zone, DataSourceGroup>();
        private Dictionary<Zone, mediaInfo> _Zone_MediaInfo = new Dictionary<Zone, mediaInfo>();
        private Dictionary<Zone, PlayList2> _Zone_Playlist = new Dictionary<Zone, PlayList2>();
        private Dictionary<Zone, Timer> _Zone_DelayedCommand = new Dictionary<Zone, Timer>();
        private Dictionary<Zone, PlaybackModes> _Zone_PlaybackMode = new Dictionary<Zone, PlaybackModes>();
        private Dictionary<Zone, TimeSpan> _Zone_PlaybackPos = new Dictionary<Zone, TimeSpan>();
        private Dictionary<Zone, DateTime> _Zone_TimeStampLastCommand = new Dictionary<Zone, DateTime>();

        // Text constants
        private const string _Text_PleaseWaitLoadingMedia = "Please wait, loading media...";

        #region Constructor / Plugin config

        public OMWinPlayer2()
            : base("OMWinPlayer2", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "OM Official Audio/Video player", "OM Dev team/Borte", "")
        {
            // Kill any active mPlayers at startup
            OpenMobile.Framework.OSSpecific.Process_Close("mplayer");
        }

        #endregion

        public override eLoadStatus initialize(IPluginHost host)
        {
            if (!Configuration.RunningOnWindows)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // Do async startup so we don't block the main OM thread
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
                {
                    OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

                    foreach (var zone in OM.Host.ZoneHandler.Zones)
                    {
                        // Initialize local variables
                        _Zone_MediaInfo.Add(zone, new mediaInfo());

                        // Restore playback mode from DB
                        _Zone_PlaybackMode.Add(zone, StoredData.GetEnum<PlaybackModes>(this, String.Format("{0}_PlaybackMode", zone), PlaybackModes.Stop));

                        // Restore playback pos from DB
                        _Zone_PlaybackPos.Add(zone, TimeSpan.FromSeconds(StoredData.GetFloat(this, String.Format("{0}_PlaybackPos", zone), 0)));

                        // Initialize delayed command timer
                        Timer tmr = new Timer(750);
                        tmr.Enabled = false;
                        tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmrDelayedCommand_Elapsed);
                        tmr.Tag = zone;
                        _Zone_DelayedCommand.Add(zone, tmr);

                        // Initialize timing variables
                        _Zone_TimeStampLastCommand.Add(zone, new DateTime());

                        // Initialize playlists
                        string playlistName = String.Format("{0}.Zone{1}.PlayList_Current", this.pluginName, zone.Index);
                        PlayList2.SetDisplayName(ref playlistName, String.Format("[{0}] Current playlist ({1})", zone.Name, this.pluginName));
                        PlayList2 playlist = new PlayList2(playlistName);
                        playlist.Load();
                        _Zone_Playlist.Add(zone, playlist);

                        // Preload one player per zone
                        ZonePlayer_CheckInstance(zone);
                    }

                    DataSources_Register();
                    Commands_Register();

                    // Restart any playing players
                    foreach (var zone in OM.Host.ZoneHandler.Zones)
                    {
                        if (_Zone_PlaybackMode[zone] == PlaybackModes.Restart)
                            PlayerControl_Restart(zone, _Zone_PlaybackPos[zone]);
                    }

                });

            return eLoadStatus.LoadSuccessful;
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.CloseProgramPreview)
            {
                SavePlayerStates();
            }
        }


        #region DataSources 

        private void DataSources_Register()
        {
            // Create one set of datasources per zone
            for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
            {
                Zone zone = OM.Host.ZoneHandler.Zones[i];

                DataSourceGroup dataSourceGroup = new DataSourceGroup(String.Format("{0}.Zone{1}", this.pluginName, i), this.pluginName, "Zone", "MediaProvider");
                _Zone_DataSourceGroup_Media.Add(OM.Host.ZoneHandler.Zones[i], dataSourceGroup);

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Playing", 0, DataSource.DataTypes.binary, null, "Mediaplayer is playing as bool"), false));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Stopped", 0, DataSource.DataTypes.binary, null, "Mediaplayer has stopped as bool"), false));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Paused", 0, DataSource.DataTypes.binary, null, "Mediaplayer has paused as bool"), false));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Repeat", 0, DataSource.DataTypes.binary, null, "Repeat is enabled as bool"), false));
                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Shuffle", 0, DataSource.DataTypes.binary, null, "Shuffle is enabled as bool"), false));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Artist", 0, DataSource.DataTypes.text, null, "Artist as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Album", 0, DataSource.DataTypes.text, null, "Album as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Genre", 0, DataSource.DataTypes.text, null, "Genre as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Rating", 0, DataSource.DataTypes.text, null, "Rating 0-5 (-1 for not set) as int"), -1));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Length.TimeSpan", 0, DataSource.DataTypes.text, null, "Total length as TimeSpan"), TimeSpan.Zero));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Length.Seconds", 0, DataSource.DataTypes.text, null, "Total length in seconds as int"), 0));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Length.Text", 0, DataSource.DataTypes.text, null, "Total length as string"), String.Empty));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Location", 0, DataSource.DataTypes.text, null, "Media location/url as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Name", 0, DataSource.DataTypes.text, null, "Name as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Rating", 0, DataSource.DataTypes.text, null, "Rating as int 0 - 5 (-1 is not set)"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.TrackNumber", 0, DataSource.DataTypes.text, null, "TrackNumber as int"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.Type", 0, DataSource.DataTypes.text, null, "Type of media as string"), ""));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo.CoverArt", 0, DataSource.DataTypes.image, null, "CoverArt of media as OImage"), null));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaInfo", 0, DataSource.DataTypes.raw, DataSourceGetter, "Current media info as mediaInfo"), _Zone_MediaInfo[zone]));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playlist", 0, DataSource.DataTypes.raw, DataSourceGetter, "Current playlist as PlayList2"), _Zone_Playlist[zone]));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Pos.TimeSpan", 0, DataSource.DataTypes.text, null, "Current playback position as TimeSpan"), TimeSpan.Zero));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Pos.Seconds", 0, DataSource.DataTypes.text, null, "Current playback position in seconds as int"), 0));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Pos.Text", 0, DataSource.DataTypes.text, null, "Current playback position as string"), String.Empty));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "Playback.Pos.Percent", 0, DataSource.DataTypes.percent, null, "Current playback position as percentage of total length as integer"), 0));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaText1", 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 1 as string"), String.Empty));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(this, this.pluginName, String.Format("Zone{0}", i), "MediaText2", 0, DataSource.DataTypes.text, null, "MediaProvider: Media text string 2 as string"), String.Empty));
            }

            OM.Host.ForEachScreen((screen) =>
            {
                Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
                OM.Host.DataHandler.ActivateDataSourceGroup(_Zone_DataSourceGroup_Media[zone], screen);
            });

        }

        private object DataSourceGetter(DataSource datasource, out bool result, object[] param)
        {
            result = false;

            string datasourceName = datasource.FullNameWithoutScreen.Replace(this.pluginName, "");
            
            Zone zone = null;
            if (datasource.NameLevel2.Contains("Zone"))
            {
                int zoneIndex = int.Parse(datasource.NameLevel2.Replace("Zone", ""));
                zone = OM.Host.ZoneHandler.Zones[zoneIndex];

                // Strip away unwanted information from the name
                datasourceName = datasourceName.Replace(String.Format(".{0}.", datasource.NameLevel2), "");
            }

            result = true;

            try
            {
                switch (datasourceName)
                {
                    #region Playlist

                    case "Playlist":
                        {
                            return _Zone_Playlist[zone];
                        }

                    #endregion

                    #region MediaInfo

                    case "MediaInfo":
                        {
                            return _Zone_MediaInfo[zone];
                        }

                    #endregion

                    default:
                        break;
                }
            }
            catch
            {
                result = false;
            }

            return null;
        }

        #endregion

        #region Commands

        void Commands_Register()
        {

            // Create one set of commands per zone
            for (int i = 0; i < OM.Host.ZoneHandler.Zones.Count; i++)
            {
                CommandGroup commandGroup = new CommandGroup(String.Format("{0}.Zone{1}", this.pluginName, i), this.pluginName, "Zone", "MediaProvider");
                _Zone_CommandGroup_Media.Add(OM.Host.ZoneHandler.Zones[i], commandGroup);

                // Don't create any commands if a zone has sub zones 
                if (OM.Host.ZoneHandler.Zones[i].HasSubZones)
                    continue;

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Play", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Stop", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Pause", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Next", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Previous", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "SeekForward", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "SeekBackward", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Playlist.Set", CommandExecutor, 1, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Repeat.Enable", CommandExecutor, 1, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Repeat.Disable", CommandExecutor, 1, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Repeat.Toggle", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Shuffle.Enable", CommandExecutor, 1, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Shuffle.Disable", CommandExecutor, 1, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(this, this.pluginName, String.Format("Zone{0}", i), "Shuffle.Toggle", CommandExecutor, 0, false, "")));
            }

            OM.Host.ForEachScreen((screen) =>
                {
                    Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
                    OM.Host.CommandHandler.ActivateCommandGroup(_Zone_CommandGroup_Media[zone], screen);
                });

        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = false;

            string commandName = command.FullNameWithoutScreen.Replace(this.pluginName, "");
            
            Zone zone = null;
            if (command.NameLevel2.Contains("Zone"))
            {
                int zoneIndex = int.Parse(command.NameLevel2.Replace("Zone", ""));
                zone = OM.Host.ZoneHandler.Zones[zoneIndex];

                // Strip away unwanted information from the command name
                commandName = commandName.Replace(String.Format(".{0}.", command.NameLevel2), "");
            }

            try
            {

                TimeSpan timeSinceLastCommand = DateTime.Now - _Zone_TimeStampLastCommand[zone];
                _Zone_TimeStampLastCommand[zone] = DateTime.Now;

                switch (commandName)
                {
                    #region Playlist.Set

                    case "Playlist.Set":
                        {
                            result = true;
                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is PlayList2)
                                {   // Set playlist
                                    _Zone_Playlist[zone] = param[0] as PlayList2;

                                    // Push update for datasource for playlist
                                    OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playlist", this.pluginName, zone.Index), _Zone_Playlist[zone]);
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

                    #endregion

                    #region Stop

                    case "Stop":
                        {
                            PlayerControl_DelayedPlay_Cancel(zone);

                            if (_ZonePlayer.ContainsKey(zone))
                            {
                                _Zone_PlaybackMode[zone] = PlaybackModes.Stop;
                                _ZonePlayer[zone].Stop();
                            }
                            return "";
                        }

                    #endregion

                    #region Play

                    case "Play":
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
                                    MediaInfo_Set(zone, media);
                                }
                            }

                            // Do we have a minmum of one parameter?
                            else if (Params.IsParamsValid(param, 1))
                            {   // Yes

                                // Check for media info present
                                if (param[0] is mediaInfo)
                                {   // Play media info
                                    media = Params.GetParam<mediaInfo>(param, 0);
                                }

                                else if (param[0] is int)
                                {   // Play index in playlist
                                    _Zone_Playlist[zone].SetCurrentStartIndex((int)param[0]);
                                    media = _Zone_Playlist[zone].CurrentItem;
                                    MediaInfo_Set(zone, media);
                                }

                                else if (param[0] is string)
                                {   // Play media location directly
                                    media = new mediaInfo(eMediaType.Local, Params.GetParam<string>(param, 0, ""));
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

                    #region Pause

                    case "Pause":
                        {
                            PlayerControl_DelayedPlay_Cancel(zone);

                            if (_ZonePlayer.ContainsKey(zone))
                            {
                                _Zone_PlaybackMode[zone] = PlaybackModes.Pause;
                                _ZonePlayer[zone].Pause();
                            }
                            return "";
                        }

                    #endregion

                    #region Next

                    case "Next":
                        {
                            // Abort any ongoing delayed commands
                            PlayerControl_DelayedPlay_Cancel(zone);

                            _Zone_Playlist[zone].GotoNextMedia();
                            
                            // Update media info
                            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
                            MediaInfo_Set(zone, media);

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

                    #endregion

                    #region Previous

                    case "Previous":
                        {
                            // Abort any ongoing delayed commands
                            PlayerControl_DelayedPlay_Cancel(zone);

                            _Zone_Playlist[zone].GotoPreviousMedia();
                            
                            // Update media info
                            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
                            MediaInfo_Set(zone, media);

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

                    #endregion

                    #region Forward

                    case "SeekForward":
                    {   // Seek forward the specified seconds
                        if (Params.IsParamsValid(param, 1))
                        {
                            int seconds = Params.GetParam<int>(param, 0);
                            try
                            {
                                _ZonePlayer[zone].SeekFwd(seconds);
                                return "";
                            }
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                                return e.Message; 
                            }
                        }
                        else
                        {   // Seek forward default length of 10 seconds
                            _ZonePlayer[zone].SeekFwd(5);
                            return "";
                        }
                    }

                    #endregion

                    #region Backward

                    case "SeekBackward":
                    {   // Seek forward the specified seconds
                        if (Params.IsParamsValid(param, 1))
                        {
                            int seconds = Params.GetParam<int>(param, 0);
                            try
                            {
                                _ZonePlayer[zone].SeekBwd(seconds);
                                return "";
                            }
                            catch (Exception e)
                            {
                                OM.Host.DebugMsg(this.pluginName, e);
                                return e.Message;
                            }
                        }
                        else
                        {   // Seek forward default length of 10 seconds
                            _ZonePlayer[zone].SeekBwd(5);
                            return "";
                        }
                    }

                    #endregion

                    #region Repeat Enable

                    case "Repeat.Enable":
                    {
                        result = true;
                        _Zone_Playlist[zone].Repeat = true;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Repeat", this.pluginName, zone.Index), _Zone_Playlist[zone].Repeat);
                        return "";
                    }

                    #endregion

                    #region Repeat Disable

                    case "Repeat.Disable":
                    {
                        result = true;
                        _Zone_Playlist[zone].Repeat = false;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Repeat", this.pluginName, zone.Index), _Zone_Playlist[zone].Repeat);
                        return "";
                    }

                    #endregion

                    #region Repeat Toggle

                    case "Repeat.Toggle":
                    {
                        result = true;
                        _Zone_Playlist[zone].Repeat = !_Zone_Playlist[zone].Repeat;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Repeat", this.pluginName, zone.Index), _Zone_Playlist[zone].Repeat);
                        return "";
                    }

                    #endregion

                    #region Shuffle Enable

                    case "Shuffle.Enable":
                    {
                        result = true;
                        _Zone_Playlist[zone].Random = true;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Shuffle", this.pluginName, zone.Index), _Zone_Playlist[zone].Random);
                        return "";
                    }

                    #endregion

                    #region Shuffle Disable

                    case "Shuffle.Disable":
                    {
                        result = true;
                        _Zone_Playlist[zone].Random = false;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Shuffle", this.pluginName, zone.Index), _Zone_Playlist[zone].Random);
                        return "";
                    }

                    #endregion

                    #region Shuffle Toggle

                    case "Shuffle.Toggle":
                    {
                        result = true;
                        _Zone_Playlist[zone].Random = !_Zone_Playlist[zone].Random;

                        // Push update for datasource for playlist
                        OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Repeat", this.pluginName, zone.Index), _Zone_Playlist[zone].Random);
                        return "";
                    }

                    #endregion

                    default:
                        break;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        private void tmrDelayedCommand_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer tmr = sender as Timer;
            tmr.Enabled = false;

            mediaInfo media = (mediaInfo)tmr.Tag2;
            System.Diagnostics.Debug.Write(String.Format("Sending delayed play {0}\r\n", media));
            PlayerControl_Play((Zone)tmr.Tag, media);
        }

        private void PlayerControl_Restart(Zone zone, TimeSpan playbackPos)
        {
            // Abort any ongoing delayed commands
            PlayerControl_DelayedPlay_Cancel(zone);

            // Update media info
            mediaInfo media = _Zone_Playlist[zone].CurrentItem;
            MediaInfo_Set(zone, media);

            // Start playback of new media
            PlayerControl_Play(zone, media);

            // Goto to restart point
            _ZonePlayer[zone].SetPlaybackPos(playbackPos);

            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Restarted playback of '{2}' from position {3}sec", zone, zone.Name, media, playbackPos.TotalSeconds));
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

                                _ZonePlayer[zone].Identify = true;
                                _ZonePlayer[zone].Start(true, true);

                                if (!System.IO.File.Exists(media.Location))
                                    return "media file not found";

                                if (!_ZonePlayer[zone].PlayFile(media.Location))
                                    return "Invalid media";

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.URLStream:
                            {
                                #region URL Stream

                                _ZonePlayer[zone].Identify = true;
                                _ZonePlayer[zone].Start(true, true);
                                if (!_ZonePlayer[zone].PlayFile(media.Location))
                                    return "Invalid url";

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                // Inform user that we're loading media
                                MediaInfo_MediaText_Set(zone, _Text_PleaseWaitLoadingMedia, media.Location);

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.AudioCD:
                            {
                                #region AudioCD

                                string Drive = "";
                                if (Configuration.RunningOnWindows)
                                    Drive = media.Location.Substring(0, 2);
                                //TODO: Add other platforms

                                if (_ZonePlayer[zone].DiscDevice == null || !_ZonePlayer[zone].DiscDevice.Equals(Drive))
                                    _ZonePlayer[zone].DiscDevice = Drive;

                                _ZonePlayer[zone].Identify = true;
                                _ZonePlayer[zone].Start(true, true);

                                _ZonePlayer[zone].PlayCD(media.TrackNumber);

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.HDDVD:
                        case eMediaType.DVD:
                            {
                                #region DVD

                                string Drive = "";
                                if (Configuration.RunningOnWindows)
                                    Drive = media.Location.Substring(0, 2);
                                //TODO: Add other platforms

                                if (_ZonePlayer[zone].DiscDevice == null || !_ZonePlayer[zone].DiscDevice.Equals(Drive))
                                    _ZonePlayer[zone].DiscDevice = Drive;

                                //_AudioPlayers[zone].Identify = false;
                                _ZonePlayer[zone].Start(true, true);

                                bool discDeviceBusy = false;
                                //ExecuteOnAllPlayers((mPlayer player) =>
                                //{
                                //    if (player.PlayerState != mPlayer.PlayerStates.Idle)
                                //        discDeviceBusy = true;
                                //});

                                if (!discDeviceBusy)
                                    _ZonePlayer[zone].PlayDVD();
                                else
                                    return "DVD drive is used by another zone/screen";

                                _Zone_PlaybackMode[zone] = PlaybackModes.Play;

                                #endregion
                            }
                            return ""; // No error

                        case eMediaType.BluRay:
                            {
                                // Todo: find bluray drive
                                _ZonePlayer[zone].PlayBluRay();

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

        private void MediaInfo_MediaText_Set(Zone zone, string text1, string text2)
        {
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaText1", this.pluginName, zone.Index), text1);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaText2", this.pluginName, zone.Index), text2);
        }

        private void MediaInfo_Set(Zone zone, mediaInfo media, TimeSpan playbackPos = new TimeSpan())
        {
            TimeSpan length = TimeSpan.FromSeconds(media.Length);

            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Artist", this.pluginName, zone.Index), media.Artist);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Album", this.pluginName, zone.Index), media.Album);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Genre", this.pluginName, zone.Index), media.Genre);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Rating", this.pluginName, zone.Index), media.Rating);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Length.Seconds", this.pluginName, zone.Index), media.Length);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Length.TimeSpan", this.pluginName, zone.Index), length);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Length.Text", this.pluginName, zone.Index), TimeSpanToString(length));
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Location", this.pluginName, zone.Index), media.Location);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Name", this.pluginName, zone.Index), media.Name);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Rating", this.pluginName, zone.Index), media.Rating);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.TrackNumber", this.pluginName, zone.Index), media.TrackNumber);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.Type", this.pluginName, zone.Index), media.Type.ToString());
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo.CoverArt", this.pluginName, zone.Index), media.coverArt);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.MediaInfo", this.pluginName, zone.Index), media);

            int playbackPercent = (int)((playbackPos.TotalSeconds / (double)media.Length) * 100d);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Pos.Seconds", this.pluginName, zone.Index), playbackPos.TotalSeconds);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Pos.TimeSpan", this.pluginName, zone.Index), playbackPos);
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Pos.Text", this.pluginName, zone.Index), TimeSpanToString(playbackPos));
            OM.Host.DataHandler.PushDataSourceValue(this, String.Format("{0}.Zone{1}.Playback.Pos.Percent", this.pluginName, zone.Index), playbackPercent);

        }

        private void MediaInfo_Clear(Zone zone)
        {
            _Zone_MediaInfo[zone] = new mediaInfo();
            MediaInfo_Set(zone, _Zone_MediaInfo[zone]);
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
                    int basePort = 24000 + ((BuiltInComponents.Host.ScreenCount + 1) * _ZonePlayer.Count);
                    _ZonePlayer.Add(zone, ZonePlayer_Create(zone, basePort));
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
            //player.OnVideoPresent += new mPlayer.PlayerEventHandler(player_OnVideoPresent);
            player.OnMediaInfoUpdated += new mPlayer.PlayerEventHandler(player_OnMediaInfoUpdated);
            player.OnPlaybackPaused += new mPlayer.PlayerEventHandler(player_OnPlaybackPaused);
            player.OnPlaybackStarted += new mPlayer.PlayerEventHandler(player_OnPlaybackStarted);
            player.OnPlaybackStopped += new mPlayer.PlayerEventHandler(player_OnPlaybackStopped);

            //player.SessionLog = _Setting_Debug_Enabled;

            // Let's start the player
            player.Start(true, true);

            return player;
        }

        #endregion

        #region Player events

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
        /// Retrieves a folder image located in the same folder as the music specificed by url
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
        #endregion

        private void SavePlayerStates()
        {
            try
            {
                lock (_ZonePlayer)
                {
                    // Save playlists
                    foreach (var zone in _Zone_Playlist.Keys)
                    {
                        if (_Zone_Playlist.ContainsKey(zone))
                        {
                            _Zone_Playlist[zone].Save();
                            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playlist {2}", zone, zone.Name, _Zone_Playlist[zone].Name));
                        }

                        // Save current playback pos
                        if (_Zone_PlaybackPos.ContainsKey(zone) && _ZonePlayer.ContainsKey(zone))
                        {
                            _Zone_PlaybackPos[zone] = _ZonePlayer[zone].MediaInfo.PlaybackPos;
                            StoredData.Set(this, String.Format("{0}_PlaybackPos", zone), _Zone_PlaybackPos[zone].TotalSeconds.ToString().Replace(",", "."));
                            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Zone: {0}({1}) -> Saving playback position {2}sec", zone, zone.Name, _Zone_PlaybackPos[zone].TotalSeconds.ToString()));
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
                lock (_ZonePlayer)
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

                    // Dispose players
                    foreach (var key in _ZonePlayer.Keys)
                        if (_ZonePlayer != null)
                            if (_ZonePlayer.ContainsKey(key))
                                _ZonePlayer[key].Dispose();
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
        ~OMWinPlayer2()
        {
            Dispose();
        }

        #endregion

    }
}
