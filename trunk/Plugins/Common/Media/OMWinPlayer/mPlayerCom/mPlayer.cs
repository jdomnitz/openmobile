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
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace OpenMobile.mPlayer
{
    public class mPlayer
    {
        public enum MediaTypes { None, File, CD, DVD, BluRay, Stream_HTTP, Other }

        public class MediaData
        {
            public TimeSpan Length { get; set; }
            public string LengthText
            {
                get
                {
                    return String.Format("{0}:{1}:{2}", Length.Hours, Length.Minutes, Length.Seconds);
                }
            }
            public TimeSpan PlaybackPos { get; set; }
            public string PlaybackPosText
            {
                get
                {
                    return String.Format("{0}:{1}:{2}", PlaybackPos.Hours, PlaybackPos.Minutes, PlaybackPos.Seconds);
                }
            }
            public int PlaybackPos_Percent { get; set; }
            public bool HasAudio { get; set; }
            public bool HasVideo { get; set; }
            public string Url { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string Year { get; set; }
            public string Comment { get; set; }
            public int Track { get; set; }
            public string Genre { get; set; }
            public MediaTypes MediaType { get; set; }
        }

        #region CDDA Data

        /// <summary>
        /// CDDA Data
        /// </summary>
        public class CDDAData
        {
            /// <summary>
            /// Track details
            /// </summary>
            public class CDDATrackData
            {
                /// <summary>
                /// Start time on CD for track
                /// </summary>
                public double StartTime { get; set; }

                /// <summary>
                /// Length of track
                /// </summary>
                public double Length { get; set; }

                public CDDATrackData(double startTime, double length)
                {
                    this.StartTime = startTime;
                    this.Length = length;
                }

                /// <summary>
                /// String representation
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    return String.Format("{0} [{1}]", StartTime, Length);
                }
            }

            /// <summary>
            /// Total amount of tracks
            /// </summary>
            public int TrackCount 
            {
                get
                {
                    return Tracks.Length;
                }
            }

            /// <summary>
            /// Current track number
            /// </summary>
            public int CurrentTrack
            {
                get
                {
                    return this._CurrentTrack;
                }
                set
                {
                    if (this._CurrentTrack != value)
                    {
                        if (value < 0)
                            this._CurrentTrack = TrackCount - 1;
                        else if (value >= TrackCount)
                            this._CurrentTrack = 0;
                        else
                            this._CurrentTrack = value;
                    }
                }
            }
            private int _CurrentTrack;

            /// <summary>
            /// Total album length in seconds
            /// </summary>
            public double TotalLengthSec
            {
                get
                {
                    return this._TotalLengthSec;
                }
                set
                {
                    if (this._TotalLengthSec != value)
                    {
                        this._TotalLengthSec = value;
                        AdjustTrackAligments();
                    }
                }
            }
            private double _TotalLengthSec;        

            /// <summary>
            /// Tracks
            /// </summary>
            public CDDATrackData[] Tracks
            {
                get
                {
                    return this._Tracks;
                }
                set
                {
                    if (this._Tracks != value)
                    {
                        this._Tracks = value;
                    }
                }
            }
            private CDDATrackData[] _Tracks = new CDDATrackData[0];        

            /// <summary>
            /// Adds a new track 
            /// </summary>
            /// <param name="length"></param>
            internal void AddTrack(double length)
            {
                // Calculate total track length up to now
                double totalSec = 0;
                for (int i = 0; i < _Tracks.Length; i++)
                    totalSec += Tracks[i].Length;
                Array.Resize<CDDATrackData>(ref _Tracks, _Tracks.Length + 1);
                _Tracks[_Tracks.Length-1] = new CDDATrackData(totalSec, length);
            }

            private double GetTotalTracksTime()
            {
                double t = 0;
                for (int i = 0; i < _Tracks.Length; i++)
                    t += _Tracks[i].Length;
                return t;
            }

            private void AdjustTrackAligments()
            {
                double OffsetPerTrack = (_TotalLengthSec - GetTotalTracksTime()) / TrackCount;
                for (int i = 0; i < _Tracks.Length; i++)
                    _Tracks[i].StartTime += OffsetPerTrack * i;
            }
        }

        #endregion

        #region Player state

        /// <summary>
        /// Playerstates
        /// </summary>
        public enum PlayerStates
        {
            /// <summary>
            /// No state
            /// </summary>
            None,
            /// <summary>
            /// Audio detection is active
            /// </summary>
            AudioDetection,
            /// <summary>
            /// Starting up
            /// </summary>
            Startup,
            /// <summary>
            /// Idle, waiting for something to do
            /// </summary>
            Idle,
            /// <summary>
            /// Playing media
            /// </summary>
            Playing,
            /// <summary>
            /// Paused
            /// </summary>
            Paused,
            /// <summary>
            /// Processing a command
            /// </summary>
            ProsessingCommand
        }

        /// <summary>
        /// The current state of the player
        /// </summary>
        public PlayerStates PlayerState
        {
            get
            {
                return this._PlayerState;
            }
            set
            {
                if (this._PlayerState != value)
                {
                    this._PlayerState = value;
                }
            }
        }
        private PlayerStates _PlayerState = PlayerStates.None;

        private void SetPlayerState(PlayerStates state)
        {
            // Skip updates of same state
            if (_PlayerState == state)
                return;

            _PlayerState = state;

            switch (state)
            {
                case PlayerStates.None:
                    _tmrMediaPoll.Enabled = false;
                    break;
                case PlayerStates.AudioDetection:
                    _tmrMediaPoll.Enabled = false;
                    break;
                case PlayerStates.Startup:
                    _tmrMediaPoll.Enabled = false;
                    Raise_OnStartupCompleted();
                    break;
                case PlayerStates.Idle:
                    _tmrMediaPoll.Enabled = false;
                    ClearMediaData(MediaTypes.None);
                    Raise_OnPlaybackStopped();
                    break;
                case PlayerStates.Playing:
                    _tmrMediaPoll.Enabled = true;
                    Raise_OnPlaybackStarted();
                    break;
                case PlayerStates.Paused:
                    _tmrMediaPoll.Enabled = false;
                    Raise_OnPlaybackPaused();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region SessionLog

        /// <summary>
        /// Create a text file that contains the output from mPlayer for this session
        /// </summary>
        public bool SessionLog
        {
            get
            {
                return this._SessionLog;
            }
            set
            {
                if (this._SessionLog != value)
                {
                    this._SessionLog = value;
                }
            }
        }
        private bool _SessionLog;

        /// <summary>
        /// Name of the session log file
        /// </summary>
        public string SessionLogName
        {
            get
            {
                return this._SessionLogName;
            }
            set
            {
                if (this._SessionLogName != value)
                {
                    this._SessionLogName = value;
                }
            }
        }
        private string _SessionLogName = "sessionlog.txt";
        private string _SessionLogNameFullPath;

        /// <summary>
        /// Dump the text to the session log
        /// </summary>
        /// <param name="text"></param>
        private void WriteToSessionLog(string text, bool header, string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
                filePath = _SessionLogNameFullPath;

            // Dump output to temp file
            if (_SessionLog)
            {
                try
                {
                    if (header)
                        File.AppendAllText(filePath, String.Format("mPlayerCom: {0}\r\n", text));
                    else
                        File.AppendAllText(filePath, String.Format("{0}\r\n", text));
                }
                catch
                {
                    //_SessionLog = false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Load states for player
        /// </summary>
        public enum PlayerLoadStates
        {
            /// <summary>
            /// Player not loaded
            /// </summary>
            NotReady,
            /// <summary>
            /// Player loaded without errors
            /// </summary>
            Ready,
            /// <summary>
            /// Player failed to load: mPlayer exe file not available
            /// </summary>
            Failed_mPlayerFileNotAvailable,
            /// <summary>
            /// Player failed to load: Unable to start mPlayer process
            /// </summary>
            Failed_mPlayerUnableToStart
        }

        #region Player properties

        /// <summary>
        /// Full path to exe of mPlayer
        /// </summary>
        public string mPlayerFileName
        {
            get
            {
                return this._mPlayerFileName;
            }
            set
            {
                if (this._mPlayerFileName != value)
                {
                    this._mPlayerFileName = value;
                }
            }
        }
        private string _mPlayerFileName = @"mplayer.exe";
        private string _mPlayerFileNameFullPath;
        private string _mPlayerFolderName = @"mPlayer";

        /// <summary>
        /// The basic arguments used for starting mPlayer
        /// </summary>
        public string BaseArgs
        {
            get
            {
                return this._BaseArgs;
            }
            set
            {
                if (this._BaseArgs != value)
                {
                    this._BaseArgs = value;
                }
            }
        }
        //private string _BaseArgs = @"-input nodefault-bindings -noconfig all -slave -quiet -idle -nomouseinput -v -osdlevel 0 -cache 8192 −cache−min 0 ";
        private string _BaseArgs = @"-input nodefault-bindings -noconfig all -slave -quiet -idle -nomouseinput -osdlevel 0 -cache 8192";

        /// <summary>
        /// Gets or sets the active window handle (used for Video output)
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                return this._WindowHandle;
            }
            set
            {
                if (this._WindowHandle != value)
                {
                    this._WindowHandle = value;
                    _ReSpawnRequired = true;
                }
            }
        }
        private IntPtr _WindowHandle;

        /// <summary>
        /// General purpose tag
        /// </summary>
        public object Tag
        {
            get
            {
                return this._Tag;
            }
            set
            {
                if (this._Tag != value)
                {
                    this._Tag = value;
                }
            }
        }
        private object _Tag;

        /// <summary>
        /// Gets or sets the identify method for the player. Identify is used to read out details of the playing media (Should not be used on DVD players)
        /// </summary>
        public bool Identify
        {
            get
            {
                return this._Identify;
            }
            set
            {
                if (this._Identify != value)
                {
                    this._Identify = value;
                    _ReSpawnRequired = true;
                }
            }
        }
        private bool _Identify = true;

        /// <summary>
        /// The name of the player instance (used for referencing in the log file)
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        /// <summary>
        /// Gets the player load state
        /// </summary>
        public PlayerLoadStates PlayerLoadState
        {
            get
            {
                return this._PlayerLoadState;
            }
         }
        private PlayerLoadStates _PlayerLoadState;

        #endregion

        #region Sync (Master / Slave)

        public enum SyncTypes { None, Master, Slave }
        private SyncTypes _SyncType;
        private int _SyncSlaveID = 0;
        private UDPDuplicator UDPServer = null;

        /// <summary>
        /// The base port for network sync
        /// </summary>
        public int SyncBasePort
        {
            get
            {
                return this._SyncBasePort;
            }
            set
            {
                if (this._SyncBasePort != value)
                {
                    this._SyncBasePort = value;
                }
            }
        }
        private int _SyncBasePort = 23867;        

        /// <summary>
        /// Sets this player as a sync master (requires a restart of the mPlayer process)
        /// </summary>
        /// <param name="slaveCount"></param>
        public void SetSyncMaster(int slaveCount)
        {
            SetSyncMaster(_SyncBasePort, slaveCount);
        }
        /// <summary>
        /// Sets this player as a sync master (requires a restart of the mPlayer process)
        /// </summary>
        /// <param name="syncBasePort">The base port to use for sync messages, slaves will be assigned to the ports above this one</param>
        /// <param name="slaveCount"></param>
        public void SetSyncMaster(int syncBasePort, int slaveCount)
        {
            _SyncBasePort = syncBasePort;
            UDPServer = new UDPDuplicator(_SyncBasePort, slaveCount);
            _SyncType = SyncTypes.Master;
            _ReSpawnRequired = true;
            System.Diagnostics.Debug.WriteLine(String.Format("mPlayer master sending on port {0}", _SyncBasePort));
        }

        /// <summary>
        /// Sets this player as a sync slave (requires a restart of the mPlayer process)
        /// </summary>
        /// <param name="slaveID"></param>
        public void SetSyncSlave(int slaveID)
        {
            SetSyncSlave(_SyncBasePort, slaveID);
        }
         /// <summary>
        /// Sets this player as a sync slave (requires a restart of the mPlayer process)
        /// </summary>
        /// <param name="syncBasePort">The base port to use for sync messages, slaves will be assigned to the ports above this one</param>
        /// <param name="slaveID"></param>
        public void SetSyncSlave(int syncBasePort, int slaveID)
        {
            _SyncBasePort = syncBasePort;
            if (slaveID < 0)
                slaveID = 0;

            _SyncSlaveID = slaveID;
            _SyncType = SyncTypes.Slave;
            _ReSpawnRequired = true;
            System.Diagnostics.Debug.WriteLine(String.Format("mPlayer slave listening to port {0}", _SyncBasePort + _SyncSlaveID + 1));
        }

        /// <summary>
        /// Disables sync capabilities for this player
        /// </summary>
        public void ClearSyncMaster()
        {
            if (UDPServer != null)
            {
                UDPServer.Dispose();
                UDPServer = null;
            }
        }

        #endregion

        #region Audio and video

        /// <summary>
        /// Audio mode
        /// </summary>
        public MPlayerAudioModeName AudioMode
        {
            get
            {
                if (this._AudioMode != null)
                    return this._AudioMode.Mode;
                else
                    return MPlayerAudioModeName.Null;
            }
            set
            {
                if (this._AudioMode != null)
                {
                    this._AudioMode.Mode = value;
                    _ReSpawnRequired = true;
                }
            }
        }
        private MPlayerAudioMode _AudioMode;
        
        /// <summary>
        /// Video mode
        /// </summary>
        public MPlayerVideoModeName VideoMode
        {
            get
            {
                if (this._VideoMode != null)
                    return this._VideoMode.Mode;
                else
                    return MPlayerVideoModeName.Null;
            }
            set
            {
                if (this._VideoMode != null)
                {
                    this._VideoMode.Mode = value;
                    _ReSpawnRequired = true;
                }
            }
        }
        private MPlayerVideoMode _VideoMode;      

        /// <summary>
        /// Audio devices reported by the mPlayer
        /// </summary>
        public Dictionary<string, int> AudioDevices
        {
            get
            {
                return this._AudioDevices;
            }
            private set
            {
                if (this._AudioDevices != value)
                {
                    this._AudioDevices = value;
                }
            }
        }
        private Dictionary<string, int> _AudioDevices = new Dictionary<string, int>();

        /// <summary>
        /// Set or get the full name of the active AudioDevice
        /// </summary>
        public string AudioDevice
        {
            get
            {
                return this._AudioDevice;
            }
            set
            {
                if (this._AudioDevice != value)
                {
                    // Disable audio
                    if (string.IsNullOrEmpty(value))
                    {
                        _AudioDevice = value;
                        _ReSpawnRequired = true;
                    }
                    // Check for valid audioDevice
                    else if (_AudioDevices.Count == 0)
                    {
                        this._AudioDevice = value;
                        _ReSpawnRequired = true;
                    }
                    else if (_AudioDevices.ContainsKey(value))
                    {
                        this._AudioDevice = value;
                        _ReSpawnRequired = true;
                    }
                    else
                        // Log error
                        WriteToSessionLog(String.Format("Unable to set active AudioDevice -> {0}", value), true);
                }
            }
        }
        private string _AudioDevice = "";        
        
        /// <summary>
        /// Get's an updated list of audio devices
        /// </summary>
        private void GetAudioDevices()
        {
            string AudioDriver = @"dsound";
            //mPlayerProcess.StartInfo.Arguments = @"-v -vo null -ao dsound -frames 0 0 ""point1sec.mp3""";//@"-slave -quiet -v -vo null -ao dsound ""point1sec.mp3""";

            // Clear existing audio devices
            _AudioDevices.Clear();

            string localPath = GetLocalPath();

            // Start get the required info from mPlayer
            // @"-input nodefault-bindings -noconfig all -slave -quiet -idle -nomouseinput -osdlevel 0 -cache 8192";
            string args = @"-v -vo null -ao dsound -frames 0 0 ";
            args += String.Format(" \"{0}{1}{2}{1}{3}\"", localPath, System.IO.Path.DirectorySeparatorChar, _mPlayerFolderName, "mPlayerCom.res");
            StringReader ProcessOutput = SpawnAndGetMPlayerOutput(args); //@"-v -vo null -ao dsound -frames 0 0 ""point1sec.mp3""");

            string sessionLogPath = localPath + System.IO.Path.DirectorySeparatorChar + _mPlayerFolderName + System.IO.Path.DirectorySeparatorChar + "AudioDetection.txt";

            // Process output from mPlayer
            if (ProcessOutput != null)
            {
                string ProcessOutputLine = "";
                int ProcessOutputMode = 0;
                while ((ProcessOutputLine = ProcessOutput.ReadLine()) != null)
                {
                    //WriteToSessionLog(String.Format("AudioDetection output: {0}", ProcessOutputLine), false, sessionLogPath);

                    // Skip empty lines
                    if (ProcessOutputLine.Trim() == "")
                        continue;

                    switch (ProcessOutputMode)
                    {
                        case 1: // Read audio devices
                            {
                                int instance;
                                if (int.TryParse(ProcessOutputLine.Substring(0, ProcessOutputLine.IndexOf(' ')), out instance))
                                {
                                    string name = ProcessOutputLine.Substring(ProcessOutputLine.IndexOf(' ')).Replace("<--", "").Trim();
                                    _AudioDevices.Add(name, instance);
                                }
                                else
                                {
                                    // Processing completed
                                    ProcessOutputMode = 0;
                                }
                            }
                            break;
                        default:
                            {
                                // Get audio devices
                                if (ProcessOutputLine.StartsWith(String.Format("ao_{0}: Output Devices:", AudioDriver)))
                                {
                                    // Continue reading lines that starts with numbers
                                    ProcessOutputMode = 1;
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// The device to use as a source for CD, DVD and other discs
        /// </summary>
        public string DiscDevice
        {
            get
            {
                return this._DiscDevice;
            }
            set
            {
                if (this._DiscDevice != value)
                {
                    this._DiscDevice = value;
                    _ReSpawnRequired = true;
                }
            }
        }
        private string _DiscDevice;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new mPlayer class
        /// </summary>
        public mPlayer(string name)
        {
            _Name = name;
            Init(true);
        }
        /// <summary>
        /// Initialize a new mPlayer class
        /// </summary>
        public mPlayer(string name, bool spawnPlayer)
        {
            _Name = name;
            Init(spawnPlayer);
        }
        /// <summary>
        /// Initialize a new mPlayer class
        /// </summary>
        public mPlayer()
        {
            Init(true);
        }
        /// <summary>
        /// Initialize a new mPlayer class
        /// </summary>
        public mPlayer(bool spawnPlayer)
        {
            Init(spawnPlayer);
        }

        private void Init(bool spawnPlayer)
        {
            _PlayerLoadState = PlayerLoadStates.NotReady;

            // Initialize video and audio modes
            _VideoMode = new MPlayerVideoMode();
            _AudioMode = new MPlayerAudioMode();

            // Activate session log if running in debugger
            _SessionLog = System.Diagnostics.Debugger.IsAttached;
            _SessionLogName = String.Format("mPlayerCom_SessionLog_{0}.txt", _Name, this.GetHashCode());

            // Media state polling
            _tmrMediaPoll = new System.Timers.Timer(1000);
            _tmrMediaPoll.Enabled = false;
            _tmrMediaPoll.Elapsed += new System.Timers.ElapsedEventHandler(_tmrMediaPoll_Elapsed);
            ClearMediaData(MediaTypes.None);

            // Spawn player with audio detection
            //if (spawnPlayer)
            //    SpawnPlayer(true, true);
            GetAudioDevices();
        }
        
        ~mPlayer()
        {
            killPlayer();
            if (_tmrMediaPoll != null)
            {
                _tmrMediaPoll.Dispose();
                _tmrMediaPoll = null;
            }
        }

        #endregion

        #region Player mode

        public enum PlaybackModes { Normal, CDDA, PlayList }

        /// <summary>
        /// Mode of playback
        /// </summary>
        public PlaybackModes PlaybackMode
        {
            get
            {
                return this._PlaybackMode;
            }
            set
            {
                if (this._PlaybackMode != value)
                {
                    this._PlaybackMode = value;
                }
            }
        }
        private PlaybackModes _PlaybackMode = PlaybackModes.Normal;

        private void SetPlayerMode(PlaybackModes playermode)
        {
            _PlaybackMode = playermode;
        }

        #endregion

        #region Player process

        Process _PlayerProcess = null;
        DataReceivedEventHandler _PlayerProcess_DataReceivedHandler = null;
        DataReceivedEventHandler _PlayerProcess_ErrorReceivedHandler = null;
        EventWaitHandle _PlayerProcess_StartupWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private void killPlayer()
        {
            if (_PlayerProcess != null)
            {
                try
                {
                    SendCommand(Commands.Quit);
                    _PlayerProcess.OutputDataReceived -= _PlayerProcess_DataReceivedHandler;
                    _PlayerProcess.ErrorDataReceived -= _PlayerProcess_ErrorReceivedHandler;
                    _PlayerProcess.WaitForExit(1000);
                    if (!_PlayerProcess.HasExited)
                        _PlayerProcess.Kill();
                    _PlayerProcess.Dispose();
                }
                catch { }
                _PlayerProcess = null;
            }
            _PlayerLoadState = PlayerLoadStates.NotReady;
        }

        private string GetLocalPath()
        {
            string PluginPath = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            return System.IO.Path.GetDirectoryName(PluginPath);
        }

        private bool _ReSpawnRequired = false;

        private void SpawnPlayer(bool audioDetection, bool waitForStartup)
        {
            // Create full path names
            string localPath = GetLocalPath();
            _mPlayerFileNameFullPath = localPath + System.IO.Path.DirectorySeparatorChar + _mPlayerFolderName + System.IO.Path.DirectorySeparatorChar + mPlayerFileName;
            _SessionLogNameFullPath = localPath + System.IO.Path.DirectorySeparatorChar + _mPlayerFolderName + System.IO.Path.DirectorySeparatorChar + _SessionLogName;

            // Clear output from tempfile
            if (_SessionLog)
            {
                try
                {
                    if (audioDetection)
                    {
                        File.Delete(_SessionLogNameFullPath);
                        File.AppendAllText(_SessionLogNameFullPath, String.Format("mPlayerCom ({0}) sessionlog started at {1}\r\n------------------------------------------------------------------\r\n", _Name, DateTime.Now));
                    }
                    else
                    {
                        File.AppendAllText(_SessionLogNameFullPath, String.Format("\r\n\r\n\r\nmPlayerCom ({0}) sessionlog started at {1}\r\n------------------------------------------------------------------\r\n", _Name, DateTime.Now));
                    }
                }
                catch { }
            }

            // Create event handlers
            if (_PlayerProcess_DataReceivedHandler == null)
                _PlayerProcess_DataReceivedHandler = new DataReceivedEventHandler(_PlayerProcess_OutputDataReceived);
            if (_PlayerProcess_ErrorReceivedHandler == null)
                _PlayerProcess_ErrorReceivedHandler = new DataReceivedEventHandler(_PlayerProcess_ErrorDataReceived);

            killPlayer();

            // Check for player file present
            if (!File.Exists(_mPlayerFileNameFullPath))
            {
                _PlayerLoadState = PlayerLoadStates.Failed_mPlayerFileNotAvailable;
                return;
            }

            // Create arguments
            string args = _BaseArgs;

            // Enable identify process?
            if (_Identify)
                args += " -identify";

            // Set disc devices (only one common is used for now)
            if (!String.IsNullOrEmpty(_DiscDevice))
                args += String.Format(" -dvd-device {0} -cdrom-device {0} −bluray−device {0}", _DiscDevice);

            #region Set Video

            // Set video mode
            if (_VideoMode.Mode != MPlayerVideoModeName.Null && !audioDetection)
            {   // Video outout is active
                args += String.Format(" -vo {0}:swapinterval=0", _VideoMode.ModeCommand);

                // Is a target window set?
                if (_WindowHandle != IntPtr.Zero)
                    args += String.Format(" -wid {0}", _WindowHandle.ToInt32());
            }
            else
                args += " -vo null";

            #endregion

            #region Set Audio

            /*
                Available audio output drivers are:
                    alsa                        ALSA 0.9/1.x audio output driver
                            noblock             Sets noblock-mode.
                            device=<device>     Sets the device name. Replace any ’,’ with ’.’ and any ’:’ with ’=’ in the ALSA device name. 
                                                For hwac3 output via S/PDIF, use an "iec958" or "spdif" device, unless you really know how to set it correctly.
 
                    dsound (Windows only)       DirectX DirectSound audio output driver
                            device=<devicenum>  Sets the device number to use. Playing a file with −v will show a list of available devices.
             
                    coreaudio (Mac OS X only)   native Mac OS X audio output driver
                            device_id=<id>      ID of output device to use (0 = default device)
	                        help                List all available output devices with their IDs.
            */

            // Set audio mode
            if (_AudioMode.Mode != MPlayerAudioModeName.Null)
            {   // Audio output is active
                args += String.Format(" -ao {0}", _AudioMode.ModeCommand);

                if (!audioDetection)
                {
                    if (!string.IsNullOrEmpty(_AudioDevice))
                    {
                        if (_AudioDevices.ContainsKey(_AudioDevice))
                        {
                            switch (Environment.OSVersion.Platform)
                            {
                                case PlatformID.MacOSX:
                                    {   // OSX
                                        args += String.Format(":device={0},", _AudioDevices[_AudioDevice]);
                                    }
                                    break;
                                case PlatformID.Unix:
                                    {   // Linux
                                        args += String.Format(":device={0},", _AudioDevice.Replace(',', '.').Replace(':', '='));
                                    }
                                    break;
                                default:
                                    {   // Windows
                                        args += String.Format(":device={0},", _AudioDevices[_AudioDevice]);
                                    }
                                    break;
                            }
                        }
                    }
                    else if (_AudioDevice == null)
                        args += " -ao null";
                }
            }
            else
                args += " -ao null";

            #endregion

            if (!audioDetection)
            {
                #region Sync Mode

                switch (_SyncType)
                {
                    case SyncTypes.None:
                        break;
                    case SyncTypes.Master:
                        args += String.Format(" -udp-master -udp-port {0}", _SyncBasePort);
                        break;
                    case SyncTypes.Slave:
                        args += String.Format(" -udp-slave -udp-port {0}", _SyncBasePort + _SyncSlaveID + 1);
                        break;
                    default:
                        break;
                }

                #endregion
            }

            // Add args so we start to play the dummy MP3 file
            if (audioDetection)
                args += String.Format(" \"{0}{1}{2}{1}{3}\"",localPath,System.IO.Path.DirectorySeparatorChar, _mPlayerFolderName, "mPlayerCom.res");

            // Start new process
            _PlayerProcess = new Process();
            _PlayerProcess.StartInfo.FileName = _mPlayerFileNameFullPath;
            _PlayerProcess.StartInfo.CreateNoWindow = true;
            _PlayerProcess.StartInfo.RedirectStandardOutput = true;
            _PlayerProcess.StartInfo.RedirectStandardError = true;
            _PlayerProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            _PlayerProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            _PlayerProcess.StartInfo.UseShellExecute = false;
            _PlayerProcess.StartInfo.RedirectStandardInput = true;
            _PlayerProcess.StartInfo.ErrorDialog = false;
            _PlayerProcess.StartInfo.Arguments = args;
            _PlayerProcess.OutputDataReceived += _PlayerProcess_DataReceivedHandler;
            _PlayerProcess.ErrorDataReceived += _PlayerProcess_ErrorReceivedHandler;

            WriteToSessionLog(String.Format("Args: {0}", args), true);

            try
            {
                // Start player
                _PlayerProcess.Start();
                if (audioDetection)
                    SetPlayerState(PlayerStates.AudioDetection);
                else
                    SetPlayerState(PlayerStates.Startup);
                _PlayerProcess.BeginOutputReadLine();
                _PlayerProcess.BeginErrorReadLine();
                _ReSpawnRequired = false;
            }
            catch
            {
                killPlayer();
                _PlayerLoadState = PlayerLoadStates.Failed_mPlayerUnableToStart;
                return;
            }

            // if we're detecting audio then we're waiting for startup, if not we're just starting
            if (waitForStartup)
            {
                // Wait for startup completed
                if (_PlayerProcess_StartupWaitHandle.WaitOne(5000))
                {

                }
            }
        }

        /// <summary>
        /// Starts the player process (will also be done automatically if required by any other command)
        /// </summary>
        public void Start()
        {
            SpawnPlayer(false, false);
        }

        /// <summary>
        /// Starts the player process (will also be done automatically if required by any other command)
        /// </summary>
        public void Start(bool onlyIfNeeded, bool WaitForStartup)
        {
            if (onlyIfNeeded)
            {
                if (_ReSpawnRequired)
                    SpawnPlayer(false, WaitForStartup);
            }
            else
                SpawnPlayer(false, WaitForStartup);
        }

        /// <summary>
        /// Disposes the player
        /// </summary>
        public void Dispose()
        {
            //SendCommand(Commands.Quit);
            //_PlayerProcess.WaitForExit(5000);
            killPlayer();
        }

        private void RestartPlayer()
        {
            WriteToSessionLog("Forced Restart of mPlayer", true);
            PlayerStates storedState = _PlayerState;
            string storedURL = _MediaInfo.Url;
            int playbackPos = (int)_MediaInfo.PlaybackPos.TotalSeconds;
            MediaData storedMedia = _MediaInfo;
            _PlayerState = PlayerStates.None;

            // Respawn player
            SpawnPlayer(false, false);

            if (storedState == PlayerStates.Playing)
            {   // Restart previously playing media
                _MediaInfo = storedMedia;
                PlayFile(storedURL);
                SeekFwd(playbackPos);
            }

        }

        /// <summary>
        /// Executes mPlayer in a separate process to grab the output
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private StringReader SpawnAndGetMPlayerOutput(string args)
        {
            string localPath = GetLocalPath();
            string mPlayerFileNameFullPath = localPath + System.IO.Path.DirectorySeparatorChar + _mPlayerFolderName + System.IO.Path.DirectorySeparatorChar + mPlayerFileName;
            
            Process mPlayerProcess = new Process();
            mPlayerProcess.StartInfo.FileName = mPlayerFileNameFullPath;
            mPlayerProcess.StartInfo.CreateNoWindow = true;
            mPlayerProcess.StartInfo.RedirectStandardOutput = true;
            mPlayerProcess.StartInfo.RedirectStandardError = true;
            mPlayerProcess.StartInfo.UseShellExecute = false;
            mPlayerProcess.StartInfo.RedirectStandardInput = true;
            try
            {
                mPlayerProcess.StartInfo.Arguments = args;
                mPlayerProcess.Start();
                //Thread.Sleep(2000);
                return new StringReader(mPlayerProcess.StandardOutput.ReadToEnd());
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Media data

        private int _HeartBeatErrorCount = 0;
        private System.Timers.Timer _tmrMediaPoll;
        void _tmrMediaPoll_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {   // Poll player for media information

            // Safety: Cancel timer if invalid state
            if (_PlayerState != PlayerStates.Playing)
                return;

            _tmrMediaPoll.Enabled = false;

            switch (_PlaybackMode)
            {
                default:
                case PlaybackModes.PlayList:
                case PlaybackModes.Normal:
                    {
                        // Get length of current media
                        if (_MediaInfo.Length == TimeSpan.Zero)
                            _MediaInfo.Length = TimeSpan.FromSeconds(SendCommand<double>(Commands.Get_Length));

                        // Get current playback position 
                        _MediaInfo.PlaybackPos = TimeSpan.FromSeconds(SendCommand<double>(Commands.Get_PlaybackPos));

                        // Get current playback position as percent
                        _MediaInfo.PlaybackPos_Percent = SendCommand<int>(Commands.Get_PlaybackPos_Percent);
                    }
                    break;
                case PlaybackModes.CDDA:
                    {
                        // Get length of current media
                        if (_CDDAData.TotalLengthSec == 0)
                            _CDDAData.TotalLengthSec = SendCommand<double>(Commands.Get_Length);

                        if (_CDDAData.Tracks == null || _CDDAData.Tracks.Length == 0 || _CDDAData.Tracks.Length < _CDDAData.CurrentTrack+1)
                            return;

                        // Get track length
                        _MediaInfo.Length = TimeSpan.FromSeconds(_CDDAData.Tracks[_CDDAData.CurrentTrack].Length);

                        // Get current playback position 
                        _MediaInfo.PlaybackPos = TimeSpan.FromSeconds(SendCommand<double>(Commands.Get_PlaybackPos) - _CDDAData.Tracks[_CDDAData.CurrentTrack].StartTime);

                        // Get current playback position as percent
                        _MediaInfo.PlaybackPos_Percent = (int)((_MediaInfo.PlaybackPos.TotalSeconds / _CDDAData.Tracks[_CDDAData.CurrentTrack].Length) * 100d);

                        // Should we loop?
                        if (_CDRepeatMode == RepeatModes.Album)
                        {
                            if (_CDDAData.CurrentTrack == (_CDDAData.TrackCount - 1))
                            {
                                if ((_MediaInfo.Length - _MediaInfo.PlaybackPos) <= TimeSpan.FromSeconds(2))
                                    this.Next();
                            }
                        }
                    }
                    break;
            }

            string filename = SendCommand<string>(Commands.Get_Filename);
            if (String.IsNullOrEmpty(filename))
            {
                _HeartBeatErrorCount++;
                WriteToSessionLog(String.Format("Heartbeat Error {0}", _HeartBeatErrorCount), true);
                if (_HeartBeatErrorCount >= 2)
                {
                    _HeartBeatErrorCount = 0;
                    WriteToSessionLog(String.Format("Too many heartbeat errors {0}, restarting player", _HeartBeatErrorCount), true);
                    RestartPlayer();
                }
            }
            else
            {
                _HeartBeatErrorCount = 0;
            }

            Raise_OnMediaInfoUpdated();

            _tmrMediaPoll.Enabled = true;
        }

        /// <summary>
        /// Media properties loaded from the player
        /// </summary>
        public Dictionary<string, string> MediaProperties
        {
            get
            {
                return this._MediaProperties;
            }
            set
            {
                if (this._MediaProperties != value)
                {
                    this._MediaProperties = value;
                }
            }
        }
        private Dictionary<string, string> _MediaProperties = new Dictionary<string,string>();

        private void ClearMediaData(MediaTypes mediaType)
        {
            _MediaProperties.Clear();
            _HasAudio = false;
            _HasVideo = false;
            _MediaInfo = new MediaData();
            _MediaInfo.MediaType = mediaType;
            CDDA_ClearData();
            Raise_OnMediaInfoUpdated();
        }

        private void AddMediaData(string info)
        {
            // Save data in media properties without the ID_ part
            string[] parts = info.Split('=');
            string name = parts[0].Substring(3);
            string value = parts[1];
            if (_MediaProperties.ContainsKey(name))
                _MediaProperties[name] = value;
            else
                _MediaProperties.Add(name, value);

            // Check for length data
            if (name.Contains("LENGTH"))
            {
                double s = 0;
                double.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out s);
                _MediaInfo.Length = TimeSpan.FromSeconds(s);
            }

            // Prosess CLIP_INFO properties
            bool prossessClipInfo = true;
            int ClipInfoID = 0;
            while (prossessClipInfo)
	        {
                if (_MediaProperties.ContainsKey(string.Format("CLIP_INFO_NAME{0}", ClipInfoID)) && 
                    _MediaProperties.ContainsKey(string.Format("CLIP_INFO_VALUE{0}", ClipInfoID)))
                {
                    // Get matching property name and value
                    string propertyName = _MediaProperties[string.Format("CLIP_INFO_NAME{0}", ClipInfoID)];
                    string propertyValue = _MediaProperties[string.Format("CLIP_INFO_VALUE{0}", ClipInfoID)];

                    switch (propertyName)
                    {
                        case "Title":
                            _MediaInfo.Title = propertyValue;
                            break;
                        case "Artist":
                            _MediaInfo.Artist = propertyValue;
                            break;
                        case "Album":
                            _MediaInfo.Album = propertyValue;
                            break;
                        case "Year":
                            _MediaInfo.Year = propertyValue;
                            break;
                        case "Comment":
                            _MediaInfo.Comment = propertyValue;
                            break;
                        case "Track":
                            int track = 0;
                            if (int.TryParse(propertyValue, out track))
                                _MediaInfo.Track = track;
                            break;
                        case "Genre":
                            _MediaInfo.Genre = propertyValue;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    prossessClipInfo = false;
                }
                ClipInfoID++;
	        }

            Raise_OnMediaInfoUpdated();
        }

        /// <summary>
        /// Video present in current media
        /// </summary>
        public bool HasVideo
        {
            get
            {
                return this._HasVideo;
            }
            private set
            {
                if (this._HasVideo != value)
                {
                    this._HasVideo = value;
                    _MediaInfo.HasVideo = value;
                    Raise_OnVideoPresent();
                }
            }
        }
        private bool _HasVideo;

        /// <summary>
        /// Audio present in current media
        /// </summary>
        public bool HasAudio
        {
            get
            {
                return this._HasAudio;
            }
            private set
            {
                if (this._HasAudio != value)
                {
                    this._HasAudio = value;
                    _MediaInfo.HasAudio = value;
                    Raise_OnAudioPresent();
                }
            }
        }
        private bool _HasAudio;

        /// <summary>
        /// Current media info
        /// </summary>
        public MediaData MediaInfo
        {
            get
            {
                return this._MediaInfo;
            }
            set
            {
                if (this._MediaInfo != value)
                {
                    this._MediaInfo = value;
                }
            }
        }
        private MediaData _MediaInfo;

        /// <summary>
        /// CDDA info (Only available when playing back CD's)
        /// </summary>
        public CDDAData CDDAInfo
        {
            get
            {
                return this._CDDAData;
            }
            set
            {
                if (this._CDDAData != value)
                {
                    this._CDDAData = value;
                }
            }
        }
        private CDDAData _CDDAData = new CDDAData();

        private void CDDA_ClearData()
        {
            _CDDAData = new CDDAData();
        }
        
        #endregion

        #region Player output data

        void _PlayerProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Dump output to tempfile
            WriteToSessionLog(String.Format("ERROR: {0}", e.Data), false);

            if (string.IsNullOrEmpty(e.Data))
                return;

            if (_PlayerState == PlayerStates.AudioDetection || _PlayerState == PlayerStates.Startup || _PlayerState == PlayerStates.None)
                return;

            // Skip certain errors
            if (e.Data.Contains("Fontconfig"))
                return;

            // Check for active commands
            for (int i = 0; i < _ActiveCommands.Count; i++)
            {
                mPlayerCommand cmd = _ActiveCommands[i];
                cmd.ErrorData = e.Data;
            }
        }

        /// <summary>
        /// Finds the first occurance of the searchString and returns the rest of the string
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        private string FindAndGetSubString(string sourceString, string searchString)
        {
            if (sourceString.Contains(searchString))
                return sourceString.Substring(sourceString.IndexOf(searchString) + searchString.Length).Trim();
            return string.Empty;
        }

        int _ProcessOutputMode = 0;
        void _PlayerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            bool raiseMediaEvent = false;

            // Dump output to tempfile
            WriteToSessionLog(e.Data, false);

            string line = e.Data;

            // Skip empty lines
            if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()))
                return;

            // Detect player ready
            if (line.Contains("MPlayer "))
                _PlayerLoadState = PlayerLoadStates.Ready;

            // Detect startup completed
            if (_PlayerState == PlayerStates.AudioDetection && line.Contains("EOF code: 1"))
            {
                SetPlayerState(PlayerStates.None);
                _PlayerProcess_StartupWaitHandle.Set();
            }
            if (_PlayerState == PlayerStates.Startup && line.Contains("CommandLine: "))
            {
                SetPlayerState(PlayerStates.Idle);
                _PlayerProcess_StartupWaitHandle.Set();
            }

            // Detect playback completed
            if (_PlayerState == PlayerStates.Playing && (line.Contains("EOF code: 1") | line.Contains("ANS_percent_pos=100")))
            {
                SetPlayerState(PlayerStates.Idle);
            }

            // Get currently playing url
            if (line.StartsWith("Playing "))
            {
                _MediaInfo.Url = line.Substring(8);
                //SetPlayerState(PlayerStates.Playing);
                raiseMediaEvent = true;
            }

            // Detect audio present
            if (line.StartsWith("ID_AUDIO_"))
                HasAudio = true;

            // Detect video present
            if (line.StartsWith("ID_VIDEO_"))
                HasVideo = true;

            // Get media data (Starts with ID_)
            if (line.StartsWith("ID_"))
            {   // Save data in media properties 
                AddMediaData(line);
            }

            // Get streaming data (if any)
                // Genre
            if (line.Contains("- icy-name:"))
            {
                _MediaInfo.Title = FindAndGetSubString(line, "- icy-name:");
                raiseMediaEvent = true;
            }
            if (line.Contains("- icy-genre:"))
            {
                _MediaInfo.Genre = FindAndGetSubString(line, "- icy-genre:");
                raiseMediaEvent = true;
            }
            if (line.Contains("- icy-description:"))
            {
                _MediaInfo.Comment = FindAndGetSubString(line, "- icy-description:");
                raiseMediaEvent = true;
            }
            if (line.Contains("- icy-url:"))
            {
                _MediaInfo.Comment += String.Format(" ({0})", FindAndGetSubString(line, "- icy-url:"));
                raiseMediaEvent = true;
            }

            // Set media type (if streaming)
            if (line.StartsWith("STREAM_HTTP("))
            {
                _MediaInfo.MediaType = MediaTypes.Stream_HTTP;
                raiseMediaEvent = true;
            }

            switch (_PlaybackMode)
            {
                case PlaybackModes.Normal:
                    break;
                case PlaybackModes.CDDA:
                    // CDDA Mode
                    if (line.StartsWith("ID_CDDA_TRACK="))
                    {  // New track detected, extract track number
                        int trackNo = 0;
                        if (int.TryParse(line.Substring(14), out trackNo))
                        {                            
                            trackNo = trackNo - 1;
                            // Handle repeat modes
                            if (_CDRepeatMode == RepeatModes.Track)
                            {
                                if (trackNo != _CDDAData.CurrentTrack)
                                {   // Go back to current track
                                    SendCommand(Commands.SetPosSeconds, _CDDAData.Tracks[_CDDAData.CurrentTrack].StartTime.ToString());
                                }
                            }
                            else
                                _CDDAData.CurrentTrack = trackNo;                        
                        }
                    }
                    break;
                default:
                    break;
            }

            // Check for command replies
            for (int i = 0; i < _ActiveCommands.Count; i++)
            {
                mPlayerCommand cmd = _ActiveCommands[i];
                if (line.StartsWith(cmd.ReplyID))
                {
                    cmd.ReplyData = line.Substring(cmd.ReplyID.Length);
                    cmd.TriggReplyReceived.Set();
                }
            }

            switch (_ProcessOutputMode)
            {
                case 1: // Read audio devices
                    {
                        int instance;
                        if (int.TryParse(line.Substring(0, line.IndexOf(' ')), out instance))
                        {
                            string name = line.Substring(line.IndexOf(' ')).Replace("<--", "").Trim();
                            _AudioDevices.Add(name, instance);
                        }
                        else
                        {
                            // Processing completed
                            _ProcessOutputMode = 0;
                        }
                    }
                    break;
                default:
                    {
                        // Get audio devices (Only if not already done)
                        if (_AudioDevices.Count == 0)
                        {
                            if (line.StartsWith(String.Format("ao_{0}: Output Devices:", _AudioMode.ModeCommand)))
                            {
                                // Continue reading lines that starts with numbers
                                _ProcessOutputMode = 1;
                            }
                        }
                    }
                    break;
            }

            if (raiseMediaEvent)
                Raise_OnMediaInfoUpdated();
        }

        #endregion

        #region Command handling (SendCommand)

        private static mPlayerCommands _PlayerCommand = new mPlayerCommands();
        private static List<mPlayerCommand> _ActiveCommands = new List<mPlayerCommand>();

        /// <summary>
        /// Sends a string 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool SendCommand(string cmd)
        {
            //if (_ReSpawnRequired)
            //    SpawnPlayer(false, false);

            try
            {
                if (_PlayerProcess != null && _PlayerProcess.HasExited == false)
                {
                    _PlayerProcess.StandardInput.Write(cmd + "\n");
                    _PlayerProcess.StandardInput.Flush();
                    WriteToSessionLog(String.Format("Command sent -> {0}", cmd), true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a predefined command with default timeout (1 sec)
        /// </summary>
        /// <typeparam name="ReplyType">Type of data to return from command</typeparam>
        /// <param name="command"></param>
        /// <returns></returns>        
        public ReplyType SendCommand<ReplyType>(Commands command)
        {
            try
            {
                return (ReplyType)Convert.ChangeType(SendCommand(command, 1000, null), typeof(ReplyType), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return default(ReplyType);
            }
        }

        /// <summary>
        /// Sends a predefined command with default timeout (1 sec)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string SendCommand(Commands command)
        {
            return SendCommand(command, 1000, null);
        }
        /// <summary>
        /// Sends a predefined command with parameters using the default timeout (1 sec)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string SendCommand(Commands command, params string[] param)
        {
            return SendCommand(command, 2000, param);
        }

        /// <summary>
        /// Sends a predefined command with parameters and a configurable timeout
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string SendCommand(Commands command, int timeout, params string[] param)
        {
            mPlayerCommand cmd = _PlayerCommand[command];
            
            if(cmd.RequiresReply)
                lock (_ActiveCommands)
                    _ActiveCommands.Add(cmd);
            
            // Send command
            SendCommand(cmd.GetCommandText(param));

            string reply = String.Empty;
            if (cmd.RequiresReply)
            {
                // Wait for and return reply
                reply = cmd.GetReplyData(timeout);

                lock (_ActiveCommands)
                    _ActiveCommands.Remove(cmd);

                if (cmd.DetectErrors)
                {
                    string err = cmd.ErrorData;
                    if (!String.IsNullOrEmpty(err))
                        throw new Exception(err);
                }
            }

            return reply;
        }

        #endregion

        #region Commands 

        public enum RepeatModes { None, Track, Album }
        /// <summary>
        /// Repeat mode for CD's
        /// </summary>
        public RepeatModes CDRepeatMode
        {
            get
            {
                return this._CDRepeatMode;
            }
            set
            {
                if (this._CDRepeatMode != value)
                {
                    this._CDRepeatMode = value;
                }
            }
        }
        private RepeatModes _CDRepeatMode = RepeatModes.Album;        

        public void SetPlaybackPos(TimeSpan position)
        {
            SendCommand(Commands.SetPosSeconds, position.TotalSeconds.ToString());
        }

        public void SetPlaybackPos(int percent)
        {
            SendCommand(Commands.SetPosPercent, percent.ToString());
        }

        public void SeekFwd(int seconds)
        {
            SendCommand(Commands.SeekRelative, seconds.ToString());
        }

        public void SeekBwd(int seconds)
        {
            SendCommand(Commands.SeekRelative, (-seconds).ToString());
        }

        /// <summary>
        /// Load and play the set file (MediaFile)
        /// </summary>
        /// <returns></returns>
        public bool PlayFile()
        {
            if (_MediaInfo != null)
                return PlayFile(_MediaInfo.Url);
            return false;
        }

        /// <summary>
        /// Load and play a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool PlayFile(string file)
        {
            // Ensure this is an existing file
            if (String.IsNullOrEmpty(file))
                return false;

            return Play_Internal(MediaTypes.File, Commands.LoadFile, file); 
        }

        private bool Play_Internal(MediaTypes mediaType, Commands command, string param)
        {
            SetPlayerState(PlayerStates.ProsessingCommand);

            //Stop();

            SetPlayerMode(PlaybackModes.Normal);
            
            // Start with fresh media properties
            ClearMediaData(mediaType);

            string reply = SendCommand(command, 5000, param);
            SetPlayerState(PlayerStates.Playing);
            _MediaInfo.Url = param;

            //string reply = String.Empty;
            //try
            //{
            //    reply = SendCommand(command, param);
            //}
            //catch { }

            //if (!String.IsNullOrEmpty(reply))
            //    SetPlayerState(PlayerStates.Playing);
            //else
            //{
            //    SetPlayerState(PlayerStates.Idle);
            //    return false;
            //}

            //// Remove trailing dot
            //if (reply.EndsWith("."))
            //    reply = reply.Substring(0, reply.Length - 1);

            //_MediaInfo.Url = reply;
            return true;
        }

        public bool PlayList(string file)
        {
            SetPlayerState(PlayerStates.ProsessingCommand);
            
            Stop();

            SetPlayerMode(PlaybackModes.PlayList);

            // Start with fresh media properties
            ClearMediaData(MediaTypes.File);

            SendCommand(Commands.LoadList, file);

            SetPlayerState(PlayerStates.Playing);
            return true;
        }

        /// <summary>
        /// Starts playback of a DVD
        /// </summary>
        /// <returns></returns>
        public bool PlayDVD()
        {
            Play_Internal(MediaTypes.DVD, Commands.PlayDVD, String.Empty);
            return true;
        }

        /// <summary>
        /// Starts playback of a BluRay disc
        /// </summary>
        /// <returns></returns>
        public bool PlayBluRay()
        {
            Play_Internal(MediaTypes.BluRay, Commands.PlayBluRay, String.Empty);
            return true;
        }

        /// <summary>
        /// Starts playback of a audio CD
        /// </summary>
        /// <returns></returns>
        public bool PlayCD(int trackNo)
        {
            SetPlayerState(PlayerStates.ProsessingCommand);
            Stop();

            SetPlayerMode(PlaybackModes.CDDA);

            // Start with fresh media properties
            ClearMediaData(MediaTypes.CD);

            string reply = String.Empty;
            reply = SendCommand(Commands.PlayCD, 20000);
            if (String.IsNullOrEmpty(reply))
            {
                SetPlayerState(PlayerStates.Idle);
                return false;
            }

            _MediaInfo.Url = DiscDevice;

            // Extract CD info
            if (_MediaProperties.ContainsKey("CDDA_TRACKS"))
            {
                int trackCount = 0;
                int.TryParse(_MediaProperties["CDDA_TRACKS"], out trackCount);

                for (int i = 1; i <= trackCount; i++)
                {
                    string key = String.Format("CDDA_TRACK_{0}_MSF",i);
                    if (_MediaProperties.ContainsKey(key))
                    {
                        string[] tsStringSplit = _MediaProperties[key].Split(':');
                        TimeSpan ts = TimeSpan.Zero;
                        if (tsStringSplit.Length == 1)
                            ts = new TimeSpan(0, 0, 0, 0, int.Parse(tsStringSplit[0]));
                        else if (tsStringSplit.Length == 2)
                            ts = new TimeSpan(0, 0, 0, int.Parse(tsStringSplit[0]), int.Parse(tsStringSplit[1]));
                        else if (tsStringSplit.Length == 3)
                            ts = new TimeSpan(0, 0, int.Parse(tsStringSplit[0]), int.Parse(tsStringSplit[1]), int.Parse(tsStringSplit[2]));
                        else if (tsStringSplit.Length == 4)
                            ts = new TimeSpan(0, int.Parse(tsStringSplit[0]), int.Parse(tsStringSplit[1]), int.Parse(tsStringSplit[2]), int.Parse(tsStringSplit[3]));
                        if (ts != TimeSpan.Zero)
                            _CDDAData.AddTrack(ts.TotalSeconds);
                    }
                }
                _CDDAData.CurrentTrack = 0;
            }

            SetPlayerState(PlayerStates.Playing);

            // Goto correct audio track
            _CDDAData.CurrentTrack = trackNo;
            double sec = _CDDAData.Tracks[_CDDAData.CurrentTrack].StartTime;
            SendCommand(Commands.SetPosSeconds, sec.ToString());                        

            return true;
        }

        /// <summary>
        /// Goto to next chapter/track/file in playlist
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            switch (_PlaybackMode)
            {
                case PlaybackModes.Normal:
                    SendCommand(Commands.Next);
                    break;
                case PlaybackModes.PlayList:
                    SendCommand(Commands.NextInPlayList);
                    break;
                case PlaybackModes.CDDA:
                    {
                        _CDDAData.CurrentTrack++;
                        double sec = _CDDAData.Tracks[_CDDAData.CurrentTrack].StartTime;
                        SendCommand(Commands.SetPosSeconds, sec.ToString());                        
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        /// <summary>
        /// Goto to previous chapter/track/file in playlist
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            switch (_PlaybackMode)
            {
                case PlaybackModes.Normal:
                    SendCommand(Commands.Previous);
                    break;
                case PlaybackModes.PlayList:
                    SendCommand(Commands.PreviousInPlayList);
                    break;
                case PlaybackModes.CDDA:
                    {
                        _CDDAData.CurrentTrack--;
                        SendCommand(Commands.SetPosSeconds, _CDDAData.Tracks[_CDDAData.CurrentTrack].StartTime.ToString());
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            // No use in pausing if we're not playing
            if (_PlayerState == PlayerStates.Paused || _PlayerState == PlayerStates.Playing)
            {
                // Send pause command
                SendCommand(Commands.Pause);

                // Update status
                if (_PlayerState == PlayerStates.Playing)
                    SetPlayerState(PlayerStates.Paused);
                else if (_PlayerState == PlayerStates.Paused)
                    SetPlayerState(PlayerStates.Playing);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Stop playback
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            // Send command
            SendCommand(Commands.Stop);

            // Update status
            SetPlayerState(PlayerStates.Idle);

            return true;
        }

        /// <summary>
        /// Cycle trough available subtitles
        /// </summary>
        /// <returns></returns>
        public bool SubTitles_Cycle()
        {
            // Send command
            SendCommand(Commands.SubTitlesCycle);
            return true;
        }

        /// <summary>
        /// Disable subtitles, enable with "SubTitles_Cycle"
        /// </summary>
        /// <returns></returns>
        public bool SubTitles_Disable()
        {
            // Send command
            SendCommand(Commands.SubTitlesDisable);
            return true;
        }

        #endregion

        #region Events

        public delegate void PlayerEventHandler(mPlayer sender);

        /// <summary>
        /// Occurs when media playback has started in the player
        /// </summary>
        public event PlayerEventHandler OnPlaybackStarted;
        private void Raise_OnPlaybackStarted()
        {
            if (OnPlaybackStarted != null)
                OnPlaybackStarted(this);
        }

        /// <summary>
        /// Occurs when media playback is stopped in the player
        /// </summary>
        public event PlayerEventHandler OnPlaybackStopped;
        private void Raise_OnPlaybackStopped()
        {
            if (OnPlaybackStopped != null)
                OnPlaybackStopped(this);
        }

        /// <summary>
        /// Occurs when media playback is paused in the player
        /// </summary>
        public event PlayerEventHandler OnPlaybackPaused;
        private void Raise_OnPlaybackPaused()
        {
            if (OnPlaybackPaused != null)
                OnPlaybackPaused(this);
        }

        /// <summary>
        /// Occurs when audio is detected in the playback
        /// </summary>
        public event PlayerEventHandler OnAudioPresent;
        private void Raise_OnAudioPresent()
        {
            if (OnAudioPresent != null)
                OnAudioPresent(this);
        }

        /// <summary>
        /// Occurs when video is detected in the playback
        /// </summary>
        public event PlayerEventHandler OnVideoPresent;
        private void Raise_OnVideoPresent()
        {
            if (OnVideoPresent != null)
                OnVideoPresent(this);
        }

        /// <summary>
        /// Occurs when media information is updated
        /// </summary>
        public event PlayerEventHandler OnMediaInfoUpdated;
        private void Raise_OnMediaInfoUpdated()
        {
            if (OnMediaInfoUpdated != null)
                OnMediaInfoUpdated(this);
        }

        /// <summary>
        /// Occurs when player has completed startup
        /// </summary>
        public event PlayerEventHandler OnStartupCompleted;
        private void Raise_OnStartupCompleted()
        {
            if (OnStartupCompleted != null)
                OnStartupCompleted(this);
        }

        #endregion

    }
}
