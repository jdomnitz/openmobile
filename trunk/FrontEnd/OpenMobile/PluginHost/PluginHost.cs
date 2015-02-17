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
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.Plugin;
using OpenMobile.Zones;
using OpenMobile;
using OpenMobile.helperFunctions;
using OpenMobile.Net;

namespace OpenMobile
{
    public sealed class PluginHost : IPluginHost
    {
        #region Private Vars

        /// <summary>
        /// Internal imageChace storage
        /// </summary>
        //private List<imageItem> imageCache = new List<imageItem>();
        private Dictionary<string,imageItem> imageCache = new Dictionary<string,imageItem>();

        private Timer tmrCurrentClock;
        private DateTime CurrentClock = DateTime.MinValue;

        /// <summary>
        /// Holds the current "random" setting for each audio device
        /// </summary>
        private bool[] random;

        /// <summary>
        /// List of all loaded media players (Index = Audio Instance)
        /// </summary>
        private IAVPlayer[] currentMediaPlayer;

        /// <summary>
        /// List of all loaded tuned content providers (Index = Audio Instance)
        /// </summary>
        private ITunedContent[] currentTunedContent;

        /// <summary>
        /// Current playlist
        /// </summary>
        private List<mediaInfo>[] currentPlaylist;

        /// <summary>
        /// List of current playlist positions (Index = Audio Instance)
        /// </summary>
        private int[] currentPlaylistIndex;

        /// <summary>
        /// List of next playlist positions (Index = Audio Instance)
        /// </summary>
        private int[] nextPlaylistIndex;

        /// <summary>
        /// List of location of video playback windows (Index = Audio Instance)
        /// </summary>
        private Rectangle[] videoPlaybackPosition;

        /// <summary>
        /// Amount of active screens
        /// </summary>
        private static int _ScreenCount = OpenTK.DisplayDevice.AvailableDisplays.Count;

        /// <summary>
        /// The amount of available audio devices (instances)
        /// </summary>
        private int _AudioDeviceCount = -1;

        /// <summary>
        /// Current skinpath
        /// </summary>
        private string _SkinPath;

        /// <summary>
        /// Current datapath
        /// </summary>
        private string _DataPath;

        /// <summary>
        /// Current pluginpath
        /// </summary>
        private string _PluginPath;

        /// <summary>
        /// Connection state to internet
        /// </summary>
        public bool InternetAccess
        {
            get
            {
                return this._InternetAccess;
            }
            set
            {
                if (this._InternetAccess != value)
                {
                    this._InternetAccess = value;
                    // Update datasource
                    DataSource_NetWork_Internet_Online.SetValue(value);
                }
            }
        }
        private bool _InternetAccess;

        /// <summary>
        /// State of network
        /// </summary>
        public bool NetWorkAvailable
        {
            get
            {
                return this._NetWorkAvailable;
            }
            set
            {
                if (this._NetWorkAvailable != value)
                {
                    this._NetWorkAvailable = value;
                    // Update datasource
                    DataSource_NetWork_Available.SetValue(value);   
                }
            }
        }
        private bool _NetWorkAvailable;        

        /// <summary>
        /// Current IP address
        /// </summary>
        public string IPAddress
        {
            get
            {
                return this._IPAddress;
            }
            private set
            {
                if (this._IPAddress != value)
                {
                    this._IPAddress = value;
                    // Update datasource
                    DataSource_NetWork_IP.SetValue(value);   
                }
            }
        }
        private string _IPAddress;        

        /// <summary>
        /// TRUE = Vehicle is moving
        /// </summary>
        private bool vehicleInMotion;

        /// <summary>
        /// Current active graphics level
        /// </summary>
        private eGraphicsLevel _GraphicsLevel;

        /// <summary>
        /// System is currently suspending
        /// </summary>
        private bool suspending;

        /// <summary>
        /// System is currently closing
        /// </summary>
        private bool closing;

        /// <summary>
        /// Panel history
        /// </summary>
        private historyCollection history = new historyCollection(_ScreenCount);

        /// <summary>
        /// Controller for zones
        /// </summary>
        public ZoneHandler ZoneHandler 
        {
            get
            {
                return _ZoneHandler;
            }
        }
        private ZoneHandler _ZoneHandler = null;

        #endregion

        #region UIHandler

        /// <summary>
        /// UIHandler
        /// </summary>
        public OpenMobile.UI.UIHandler UIHandler
        {
            get
            {
                return _UIHandler;
            }
        }
        private OpenMobile.UI.UIHandler _UIHandler = null;

        #endregion

        #region Datahandler

        /// <summary>
        /// Datahandler
        /// </summary>
        public OpenMobile.Data.DataHandler DataHandler
        {
            get
            {
                return _DataHandler;
            }
        }
        private OpenMobile.Data.DataHandler _DataHandler = null;

        #endregion

        #region CommandHandler

        /// <summary>
        /// CommandHandler
        /// </summary>
        public OpenMobile.CommandHandler CommandHandler
        {
            get
            {
                return _CommandHandler;
            }
        }
        private OpenMobile.CommandHandler _CommandHandler = null;

        #endregion

        #region MediaProviderHandler

        /// <summary>
        /// The mediaprovider handler
        /// </summary>
        public OpenMobile.Media.MediaProviderHandler MediaProviderHandler
        {
            get
            {
                return this._MediaProviderHandler;
            }
            set
            {
                if (this._MediaProviderHandler != value)
                {
                    this._MediaProviderHandler = value;
                }
            }
        }
        private OpenMobile.Media.MediaProviderHandler _MediaProviderHandler = null;
        

        #endregion

        #region Constructor and initialization

        public PluginHost()
        {
            /*
            // TODO_: number 8 is maximum number of audio devices OM supports, this number should be dynamic
            queued = new List<mediaInfo>[8];
            for (int i = 0; i < queued.Length; i++)
                queued[i] = new List<mediaInfo>();
            currentMediaPlayer = new IAVPlayer[8];
            currentTunedContent = new ITunedContent[8];
            currentPosition = new int[8];
            nextPosition = new int[8];
            */

            // Save reference to the pluginhost for the framework (Do not remove this line!)
            BuiltInComponents.Host = this;

            // Initialize variables
            _ClientArea = new Rectangle[_ScreenCount];
            for (int i = 0; i < _ClientArea.Length; i++)
                _ClientArea[i] = new Rectangle(0, 0, 1000, 600);

            // Create local data folder for OM (under users local data)
            if (Directory.Exists(DataPath) == false)
                Directory.CreateDirectory(DataPath);

            // Connect security request events
            Credentials.OnAuthorizationRequested += new Credentials.Authorization(Credentials_OnAuthorizationRequested);
            Credentials.Open();

            // Start a timer to get the current time (fired each minute)
            //tmrCurrentClock = new System.Threading.Timer(tmrCurrentClock_time, null, 60000, 60000);
            tmrCurrentClock = new Timer(1000);
            tmrCurrentClock.Elapsed += new System.Timers.ElapsedEventHandler(tmrCurrentClock_Elapsed);
            tmrCurrentClock.Enabled = true;

            // Connect screen events
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

            // Initialize system settings
            BuiltInComponents.SystemSettings.Init();

            // Initialize zonehandler
            _ZoneHandler = new ZoneHandler();

            // Initialize UIhandler
            _UIHandler = new OpenMobile.UI.UIHandler();

            // Initialize DataHandler
            _DataHandler = new OpenMobile.Data.DataHandler();

            // Initialize CommandHandler
            _CommandHandler = new OpenMobile.CommandHandler();

            // Initialize panel transition effects handler
            OpenMobile.Controls.PanelTransitionEffectHandler.Init();
        }

        public void Dispose()
        {
            // Stop zone handler
            ZoneHandler.Stop();

            if (tmrCurrentClock != null)
                tmrCurrentClock.Dispose();
            _DataHandler.Dispose();
            _CommandHandler.Dispose();
            _UIHandler.Dispose();
            _ZoneHandler.Dispose();
        }

        ~PluginHost()
        {
            Dispose();
        }

        void tmrCurrentClock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            tmrCurrentClock_time(null);
        }

        /// <summary>
        /// Initializes and starts all pluginhost features (including HAL)
        /// </summary>
        public void Start()
        {
            // Start zone handler
            ZoneHandler.Start();

            // Load system datasources
            BuiltInComponents.DataSources.Init();

            // Load datasource from the pluginHost
            RegisterDataSources_Early();

            // Initialize data
            IPAddress = GetLocalIPAddress();
            NetWorkAvailable = GetNetWorkAvailable();

            // Connect network events
            //NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
            NetworkStatus.AvailabilityChanged += new NetworkStatusChangedHandler(NetworkStatus_AvailabilityChanged);
        }

        /// <summary>
        /// Load data into pluginHost (playlists)
        /// </summary>
        public void LateInit()
        {            
            //// Raise screen orientation event in case it differs from what we expected
            //for (int i = 0; i < ScreenCount; i++)
            //    if (!OpenTK.DisplayDevice.AvailableDisplays[i].Landscape)
            //        raiseSystemEvent(eFunction.screenOrientationChanged, i.ToString(), "Portrait", string.Empty);

            // Initialize device arrays
            currentPlaylist = new List<mediaInfo>[AudioDeviceCount];
            for (int i = 0; i < currentPlaylist.Length; i++)
                currentPlaylist[i] = new List<mediaInfo>();

            currentMediaPlayer = new IAVPlayer[AudioDeviceCount];
            currentTunedContent = new ITunedContent[AudioDeviceCount];
            currentPlaylistIndex = new int[AudioDeviceCount];
            nextPlaylistIndex = new int[AudioDeviceCount];

            videoPlaybackPosition = new Rectangle[AudioDeviceCount];

            //// Start loading playlist data
            //SandboxedThread.Asynchronous(loadPlaylists);

            // Initialize built in helper functions
            SandboxedThread.Asynchronous(OpenMobile.helperFunctions.OSK.Init);

            // Register commands and datasources
            RegisterCommands_Late();
            RegisterDataSources_Late();
        }

        #endregion

        #region Media -> Playlist control

        /// <summary>
        /// Generates the next (and following) item for media players based on current playlist
        /// </summary>
        /// <param name="instance"></param>
        private void generateNext(Zone zone, bool BlockRandom)
        {
            lock (nextPlaylistIndex)
            {
                if ((random != null) && (random[zone.AudioDevice.Instance] == true) && (currentPlaylist[zone.AudioDevice.Instance].Count > 1) && (!BlockRandom))
                {
                    int result;
                    do
                        result = OpenMobile.Framework.Math.Calculation.RandomNumber(0, currentPlaylist[zone.AudioDevice.Instance].Count);
                    while (nextPlaylistIndex[zone.AudioDevice.Instance] == result);
                    nextPlaylistIndex[zone.AudioDevice.Instance] = result;
                }
                else
                    nextPlaylistIndex[zone.AudioDevice.Instance] = currentPlaylistIndex[zone.AudioDevice.Instance] + 1;
                if (nextPlaylistIndex[zone.AudioDevice.Instance] == currentPlaylist[zone.AudioDevice.Instance].Count)
                    nextPlaylistIndex[zone.AudioDevice.Instance] = 0;
                if ((currentPlaylist[zone.AudioDevice.Instance].Count > nextPlaylistIndex[zone.AudioDevice.Instance]) && (getPlayingMedia(zone).Location == currentPlaylist[zone.AudioDevice.Instance][nextPlaylistIndex[zone.AudioDevice.Instance]].Location))
                {
                    nextPlaylistIndex[zone.AudioDevice.Instance]++;
                    if (nextPlaylistIndex[zone.AudioDevice.Instance] == currentPlaylist[zone.AudioDevice.Instance].Count)
                        nextPlaylistIndex[zone.AudioDevice.Instance] = 0;
                }
            }
        }

        /// <summary>
        /// Gets the current playlist for a specific screen
        /// </summary>
        /// <param name="instance">Audio device</param>
        /// <returns></returns>
        public List<mediaInfo> getPlaylist(int Screen)
        {
            return getPlaylist(ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Gets the current playlist for a specific zone
        /// </summary>
        /// <param name="instance">Audio device</param>
        /// <returns></returns>
        public List<mediaInfo> getPlaylist(Zone zone)
        {
            if (zone == null)
                return null;
            return currentPlaylist[zone.AudioDevice.Instance];
        }
        /// <summary>
        /// Gets the current playlist for a specific audio device (instance)
        /// </summary>
        /// <param name="instance">Audio device</param>
        /// <returns></returns>
        public List<mediaInfo> getPlaylistForAudioDeviceInstance(int AudioDeviceInstance)
        {
            if (AudioDeviceInstance < 0 | AudioDeviceInstance > currentPlaylist.Length)
                return null;
            return currentPlaylist[AudioDeviceInstance];
        }
        /// <summary>
        /// Activates a playlist for a specific screen
        /// </summary>
        /// <param name="source">List to activate</param>
        /// <param name="Zone">Zone</param>
        /// <returns></returns>
        public bool setPlaylist(List<mediaInfo> source, int Screen)
        {
            return setPlaylist(source, ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Activates a playlist for a specific Zone
        /// </summary>
        /// <param name="source">List to activate</param>
        /// <param name="Zone">Zone</param>
        /// <returns></returns>
        public bool setPlaylist(List<mediaInfo> source, Zone zone)
        {
            if (zone == null)
                return false;
            if (source == null)
                return false;
            currentPlaylistIndex[zone.AudioDevice.Instance] = -1;
            currentPlaylist[zone.AudioDevice.Instance].Clear();
            currentPlaylist[zone.AudioDevice.Instance].AddRange(source.GetRange(0, source.Count));
            if (currentPlaylist[zone.AudioDevice.Instance].Count > 0)
                generateNext(zone, false);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, zone, string.Empty); });
            return true;
        }
        /// <summary>
        /// Activates a playlist for a specific Zone
        /// </summary>
        /// <param name="source">List to activate</param>
        /// <param name="Zone">Zone</param>
        /// <returns></returns>
        public bool setPlaylistForAudioDeviceInstance(List<mediaInfo> source, int AudioDeviceInstance)
        {
            if (AudioDeviceInstance < 0 | AudioDeviceInstance > currentPlaylist.Length)
                return false;
            if (source == null)
                return false;
            currentPlaylistIndex[AudioDeviceInstance] = -1;
            currentPlaylist[AudioDeviceInstance].Clear();
            currentPlaylist[AudioDeviceInstance].AddRange(source.GetRange(0, source.Count));
            //if (queued[AudioDeviceInstance].Count > 0)
            //    generateNext(zone);
            //SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, zone, string.Empty); });
            return true;
        }


        /// <summary>
        /// Appends a playlist to the current playlist for a specific Zone
        /// </summary>
        /// <param name="source">List to append</param>
        /// <param name="Screen">Screen</param>
        /// <returns></returns>
        public bool appendPlaylist(List<mediaInfo> source, int Screen)
        {
            return appendPlaylist(source, ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Appends a playlist to the current playlist for a specific Zone
        /// </summary>
        /// <param name="source">List to append</param>
        /// <param name="Zone">Zone</param>
        /// <returns></returns>
        public bool appendPlaylist(List<mediaInfo> source, Zone Zone)
        {
            if (Zone == null)
                return false;
            if (currentPlaylist[Zone.AudioDevice.Instance].Count == 0)
                return setPlaylist(source, Zone);
            bool single = (currentPlaylist[Zone.AudioDevice.Instance].Count == 1);
            currentPlaylist[Zone.AudioDevice.Instance].AddRange(source.GetRange(0, source.Count));
            if (single)
                generateNext(Zone, false);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, Zone, string.Empty); });
            return true;
        }

        ///// <summary>
        ///// Saves all current playlists to the media database
        ///// </summary>
        //private void savePlaylists()
        //{
        //    using (PluginSettings s = new PluginSettings())
        //    {
        //        // TODO: ReEnable PlayList load and save
        //        for (int i = 0; i < AudioDeviceCount; i++)
        //        {
        //            PlayList.writePlaylistToDB("Current" + i.ToString(), getPlaylistForAudioDeviceInstance(i));
        //            s.setSetting(BuiltInComponents.OMInternalPlugin, "Media.Instance" + i.ToString() + ".Random", getRandomForAudioDeviceInstance(i).ToString());
        //        }
        //    }
        //}

        ///// <summary>
        ///// Loads playlists from media database and activates them
        ///// </summary>
        //private void loadPlaylists()
        //{
        //    using (PluginSettings s = new PluginSettings())
        //    {
        //        bool res;
        //        string dbname = s.getSetting(BuiltInComponents.OMInternalPlugin, "Default.MusicDatabase");
        //        // TODO: ReEnable PlayList load and save
        //        for (int i = 0; i < AudioDeviceCount; i++)
        //        {
        //            setPlaylistForAudioDeviceInstance(PlayList.readPlaylistFromDB("Current" + i.ToString(), dbname), i);
        //            if (bool.TryParse(s.getSetting(BuiltInComponents.OMInternalPlugin, "Media.Instance" + i.ToString() + ".Random"), out res))
        //                setRandomForAudioDeviceInstance(i, res);
        //        }
        //    }
        //}

        #endregion

        #region Audio device -> Instance control

        /// <summary>
        /// Gets the assosiated audio device for a screen
        /// </summary>
        /// <param name="screen">Screen number</param>
        /// <returns>Audio device (instance)</returns>
        /// 
        /*
        public int GetDefaultZoneForScreen(int screen)
        { //Instance numbers are incremented by 1 so that "not set"==0
            if (_AudioDeviceInstanceMapping == null)
                _AudioDeviceInstanceMapping = new int[ScreenCount];
            if (_AudioDeviceInstanceMapping[screen] == 0)
                _AudioDeviceInstanceMapping[screen] = getAudioDeviceInstance(screen);
            if (_AudioDeviceInstanceMapping[screen] == 0)
                return 0;
            return _AudioDeviceInstanceMapping[screen] - 1;
        }
        */
        static AudioDevice[] AudioDevices = null;

        /// <summary>
        /// Get the audio device instance for a specific screen
        /// <para>The instance number is converted from the current DB setting</para>
        /// </summary>
        /// <param name="screen">Screen number</param>
        /// <returns>Audio device (instance)</returns>
        private AudioDevice getAudioDeviceByScreen(int screen)
        {
            string str;
            // TODO: Remove screen + 1
            using (PluginSettings settings = new PluginSettings())
                str = settings.getSetting(BuiltInComponents.OMInternalPlugin, "Screen" + (screen + 1).ToString() + ".SoundCard");
            if (str.Length == 0)
                return null;
            if (AudioDevices == null)
                if (!refreshAudioDevices())
                    return null;
            return Array.Find(AudioDevices, p => (p != null) && (p.Name.Replace("  ", " ") == str)); // +1;
        }

        /// <summary>
        /// Get's the audio device name for a instance number
        /// </summary>
        /// <param name="instance">Audio device (instance)</param>
        /// <returns>Audio device name</returns>
        public AudioDevice getAudioDevice(int instance)
        {
            if (AudioDevices == null)
                if (!refreshAudioDevices())
                    return null;

            if (instance < AudioDevices.GetLowerBound(0) | instance > AudioDevices.GetUpperBound(0))
                return null;

            return AudioDevices[instance];
        }


        /// <summary>
        /// Get's the name of the default audio device 
        /// </summary>
        /// <returns></returns>
        public AudioDevice GetAudioDeviceDefault()
        {
            return AudioDevice.DefaultDevice;
        }

        /// <summary>
        /// Get's the audio device from a device name
        /// </summary>
        /// <param name="name">Audio device name</param>
        /// <returns>Audio device instance</returns>
        public AudioDevice getAudioDevice(string name)
        {
            if (AudioDevices == null)
                if (!refreshAudioDevices())
                    return null;

            if (string.IsNullOrEmpty(name))
                return null;

            return Array.Find(AudioDevices, p => (p != null) && p.Name == name);//(p.Replace("  ", " ") == name)); // +1;
        }

        /// <summary>
        /// Updates the devices array with Audio device names 
        /// <para>Devices are read from the currently loaded player</para>
        /// </summary>
        /// <returns></returns>
        private bool refreshAudioDevices()
        {
            AudioDevice[] devs = null;
            foreach (IBasePlugin player in Core.pluginCollection.FindAll(p => typeof(IAudioDeviceProvider).IsInstanceOfType(p) == true))
            {
                try
                {
                    devs = ((IAudioDeviceProvider)player).OutputDevices;
                }
                catch (Exception e)
                {
                    //string message = e.GetType().ToString() + "(" + e.Message + ")\r\n\r\n" + e.StackTrace + "\r\n********";
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, e.Source, spewException(e));
                }
                
                // We set it to null if we only detect the default unit as this indicates that no units was found
                // A length of 2 would indicate a default unit and the corresponding physcial unit which is minimum for detection
                if (devs.Length >= 2)
                {
                    if (AudioDevices == null || AudioDevices.Length != devs.Length)
                    {
                        AudioDevices = devs;
                        // Raise event
                        raiseSystemEvent(eFunction.AudioDevicesAvailable, String.Empty, String.Empty, String.Empty);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the amount of available audio devices (instances)
        /// </summary>
        public int AudioDeviceCount
        {
            get
            {
                if (_AudioDeviceCount == -1)
                {
                    if (refreshAudioDevices())
                        _AudioDeviceCount = AudioDevices.Length;
                    else
                        return 0;
                }
                return _AudioDeviceCount;
            }
        }

        #endregion

        #region Folders

        /// <summary>
        /// The active skin's folder
        /// </summary>
        public string SkinPath
        {
            get
            {
                if (_SkinPath == null)
                {
                    using (PluginSettings s = new PluginSettings())
                    {
                        _SkinPath = s.getSetting(BuiltInComponents.OMInternalPlugin, "UI.Skin");
                        if (_SkinPath.Length == 0)
                        {
                            _SkinPath = "Default";
                            s.setSetting(BuiltInComponents.OMInternalPlugin, "UI.Skin", _SkinPath);
                        }
                    }
                    _SkinPath = Path.Combine(Application.StartupPath, "Skins", _SkinPath);
                }
                return _SkinPath;
            }
            set
            {
                _SkinPath = Path.Combine(Application.StartupPath, "Skins", value);
            }
        }

        /// <summary>
        /// Data folder path (OM Temp data folder)
        /// </summary>
        public string DataPath
        {
            get
            {
                if (_DataPath == null)
                {
                    _DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile");

                    // Ensure path is valid, if not create it
                    if (!Directory.Exists(_DataPath))
                        Directory.CreateDirectory(_DataPath);
                }
                return _DataPath;
            }
        }

        /// <summary>
        /// Plugin data folder
        /// </summary>
        public string PluginPath
        {
            get
            {
                if (_PluginPath == null)
                {
                    _PluginPath = Path.Combine(Application.StartupPath, "Plugins");
                    // Ensure path is valid, if not create it
                    if (!Directory.Exists(_PluginPath))
                        Directory.CreateDirectory(_PluginPath);

                }
                return _PluginPath;
            }
        }

        #endregion

        #region Plugin handling

        /// <summary>
        /// Returns a IBasePlugin based on it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IBasePlugin getPluginByName(string name)
        {
            return Core.pluginCollection.Find(x => (x !=null && x.pluginName == name));            
        }

        /// <summary>
        /// Gets all plugins that matches a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        public List<IBasePlugin> GetPlugins<T>()
        {
            return Core.pluginCollection.FindAll(x => typeof(T).IsInstanceOfType(x));
        }

        #endregion

        #region Panel handling

        /// <summary>
        /// Returns a panel based on it's name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="panelName"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        private static OMPanel getPanelByName(string name, string panelName, int screen)
        {
            if (name == "About")
                return BuiltInComponents.AboutPanel();
            try
            {
                OMPanel p = null;
                IBasePlugin plugin = getPluginByName(name);
                if (plugin != null)
                    p = ((IHighLevel)plugin).loadPanel(panelName, screen);

                // Look for panel from the default panels collection in the framework if no other panel is found
                if (p == null)
                    p = BuiltInComponents.Panels[screen, panelName];

                return p;
            }
            catch (NotImplementedException) { return null; }
        }

        #endregion

        #region Graphics

        /// <summary>
        /// A wrapper for executing code on each screen (can be used with anonymous delegates)
        /// </summary>
        /// <param name="d">delegate</param>
        public void ForEachScreen(ForEachScreenDelegate d)
        {
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                d(i);
        }

        /// <summary>
        /// Get's the rendering windows handle
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public object GetWindowHandle(int screen)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return (IntPtr)(-1); //Out of bounds
            return Core.RenderingWindows[screen].WindowInfo.Handle;
        }

        /// <summary>
        /// Graphic level performance
        /// </summary>
        public eGraphicsLevel GraphicsLevel
        {
            get
            {
                return _GraphicsLevel;
            }
            set
            {
                _GraphicsLevel = value;
            }
        }

        /// <summary>
        /// Sets the location video should be played (based on the 1000x600 default scale)
        /// </summary>
        /// <param name="Screen">Screen to apply setting to</param>
        /// <param name="videoArea">Video area rectangle</param>
        public void SetVideoPosition(int Screen, Rectangle videoArea)
        {
            if ((Screen < 0) || (Screen >= ScreenCount))
                return;
            if (videoPlaybackPosition == null)
                return;
            if (videoPlaybackPosition[Screen] == videoArea)
                return;
            videoPlaybackPosition[Screen] = videoArea;
            raiseSystemEvent(eFunction.videoAreaChanged, Screen.ToString(), String.Format("{0}|{1}|{2}|{3}",videoArea.X,videoArea.Y,videoArea.Width,videoArea.Height), String.Empty);
        }

        /// <summary>
        /// Gets the location video should be played (based on the 1000x600 default scale)
        /// </summary>
        /// <param name="instance">Audio device (instance)</param>
        /// <returns></returns>
        public Rectangle GetVideoPosition(int Screen)
        {
            if ((Screen < 0) || (Screen >= ScreenCount))
                return Rectangle.Empty;
            if (videoPlaybackPosition == null)
                return Rectangle.Empty;
            return videoPlaybackPosition[Screen];
        }


        #endregion

        #region General properties

        /// <summary>
        /// If the vehicle is in motion returns true (returns false if unknown)
        /// </summary>
        public bool VehicleInMotion
        {
            get
            {
                return vehicleInMotion;
            }
            set
            {
                if (vehicleInMotion == value)
                    return;
                vehicleInMotion = value;
                if (value)
                    raiseSystemEvent(eFunction.vehicleMoving, string.Empty, string.Empty, string.Empty);
                else
                    raiseSystemEvent(eFunction.vehicleStopped, string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// The current GPS position and/or city/state/zip
        /// </summary>
        public Location CurrentLocation
        {
            get
            {
                // Try to get data from the DB
                if (_location == null || _location.IsEmpty())
                    CurrentLocation = StoredData.GetXML<Location>(BuiltInComponents.OMInternalPlugin, "Location.Current.Data");

                // If location is still empty, default to users home position
                if (_location == null || _location.IsEmpty())
                    CurrentLocation = BuiltInComponents.SystemSettings.Location_Home;

                return _location;
            }
            set
            {
                UpdateLocation(value);
            }
        }
        private Location _location = new Location();

        /// <summary>
        /// Updates any field in the current location data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="street"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="country"></param>
        /// <param name="zip"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="altitude"></param>
        public void UpdateLocation(string name = null,
            string address = null,
            string street = null,
            string city = null,
            string state = null,
            string country = null,
            string zip = null,
            float? latitude = null,
            float? longitude = null,
            float? altitude = null)
        {
            bool dataUpdated = false;
            Location currentLocation = CurrentLocation;
            if (name != null)
            {
                if (!currentLocation.Name.Equals(name))
                {
                    currentLocation.Name = name;
                    dataUpdated = true;
                }
            }
            if (address != null)
            {
                if (!currentLocation.Address.Equals(address))
                {
                    currentLocation.Address = address;
                    dataUpdated = true;
                }
            }
            if (street != null)
            {
                if (!currentLocation.Street.Equals(street))
                {
                    currentLocation.Street = street;
                    dataUpdated = true;
                }
            }
            if (city != null)
            {
                if (!currentLocation.City.Equals(city))
                {
                    currentLocation.City = city;
                    dataUpdated = true;
                }
            }
            if (state != null)
            {
                if (!currentLocation.State.Equals(state))
                {
                    currentLocation.State = state;
                    dataUpdated = true;
                }
            }
            if (country != null)
            {
                if (!currentLocation.Country.Equals(country))
                {
                    currentLocation.Country = country;
                    dataUpdated = true;
                }
            }
            if (zip != null)
            {
                if (!currentLocation.Zip.Equals(zip))
                {
                    currentLocation.Zip = zip;
                    dataUpdated = true;
                }
            }
            if (latitude.HasValue)
            {
                if (!currentLocation.Latitude.Equals(latitude.Value))
                {
                    currentLocation.Latitude = latitude.Value;
                    dataUpdated = true;
                }
            }
            if (longitude.HasValue)
            {
                if (!currentLocation.Longitude.Equals(longitude.Value))
                {
                    currentLocation.Longitude = longitude.Value;
                    dataUpdated = true;
                }
            }
            if (altitude.HasValue)
            {
                if (!currentLocation.Altitude.Equals(altitude.Value))
                {
                    currentLocation.Altitude = altitude.Value;
                    dataUpdated = true;
                }
            }

            UpdateLocation(currentLocation, dataUpdated);
        }

        /// <summary>
        /// Updates the current location data
        /// </summary>
        /// <param name="location"></param>
        public void UpdateLocation(Location location, bool forceUpdate = false)
        {
            // Ensure we have a valid value
            if (location == null)
                return;

            // Is this a new location?
            if (_location != location | forceUpdate)
            {   // Yes
                _location = location;

                // Update datasource
                DataSource_Location_Current.SetValue(_location);

                // Update database
                StoredData.SetXML(BuiltInComponents.OMInternalPlugin, "Location.Current.Data", _location);

                // Update sunrise / sunset data
                _Update_SunSetSunrise(_location);

                // Raise event
                raiseNavigationEvent(eNavigationEvent.LocationChanged, _location.ToString());
            }
        }

        private void _Update_SunSetSunrise(Location location)
        {
            _CurrentLocation_Sunrise = OpenMobile.Framework.Math.Calculation.getSunrise(location.Longitude, location.Latitude);
            _CurrentLocation_Sunset = OpenMobile.Framework.Math.Calculation.getSunset(location.Longitude, location.Latitude);

            // Update datasource
            if (DataSource_Location_Current_Sunset != null)
                DataSource_Location_Current_Sunset.SetValue(_CurrentLocation_Sunset);
            if (DataSource_Location_Current_Sunrise != null)
                DataSource_Location_Current_Sunrise.SetValue(_CurrentLocation_Sunrise);
        }

        /// <summary>
        /// The current calculated time for sunrise
        /// </summary>
        public DateTime CurrentLocation_Sunrise
        {
            get
            {
                return this._CurrentLocation_Sunrise;
            }
        }
        private DateTime _CurrentLocation_Sunrise;
        
        /// <summary>
        /// A internal helper state variable to control how the sunrise/sunset events are raised. 0 = no state, 1 = sunset , 2 = sunrise
        /// </summary>
        private int _CurrentLocation_Sunrise_EventState;

        /// <summary>
        /// The current calculated time for sunset
        /// </summary>
        public DateTime CurrentLocation_Sunset
        {
            get
            {
                return this._CurrentLocation_Sunset;
            }
        }
        private DateTime _CurrentLocation_Sunset;

        /// <summary>
        /// a bool indicating if the current location is defined as daytime
        /// </summary>
        public bool CurrentLocation_Daytime
        {
            get
            {
                return this._CurrentLocation_Daytime;
            }
            private set
            {
                if (value != _CurrentLocation_Daytime)
                {   // value changed
                    _CurrentLocation_Daytime = value;
                    if (DataSource_Location_Current_Daytime != null)
                        DataSource_Location_Current_Daytime.SetValue(_CurrentLocation_Daytime);

                    // Raise events
                    raiseSystemEvent((_CurrentLocation_Daytime ? eFunction.CurrentLocationDay : eFunction.CurrentLocationNight), String.Empty, String.Empty, String.Empty);
                }
            }
        }
        private bool _CurrentLocation_Daytime = true;
        private bool _CurrentLocation_Daytime_Temp = true;

        #endregion

        #region Time and date (System clock)
      


        /// <summary>
        /// Raises time and date related events (One minute resolution)
        /// </summary>
        /// <param name="state"></param>
        private void tmrCurrentClock_time(object state)
        {
            tmrCurrentClock.Enabled = false;

            // Initialize clock values
            if (CurrentClock == DateTime.MinValue)
                CurrentClock = DateTime.Now;

            // Check for new hour
            if (CurrentClock.Hour != DateTime.Now.Hour)
            {
                // Raise hour changed event
                raiseSystemEvent(eFunction.hourChanged, String.Empty, String.Empty, String.Empty);
                
                // Check for day changed, if so raise event
                if (CurrentClock.Day != DateTime.Now.Day)
                    raiseSystemEvent(eFunction.dateChanged, String.Empty, String.Empty, String.Empty);
            }

            // Check for sunrise / sunset times, if so raise event
            bool dayTime = CurrentClock.IsBetween(CurrentLocation_Sunrise, CurrentLocation_Sunset);
            if (_CurrentLocation_Daytime_Temp != dayTime)
            {
                _CurrentLocation_Daytime_Temp = dayTime;
                CurrentLocation_Daytime = dayTime;
                if (CurrentLocation_Daytime)
                {
                    if (_CurrentLocation_Sunrise_EventState == 0 || _CurrentLocation_Sunrise_EventState != 1)
                    {
                        _CurrentLocation_Sunrise_EventState = 1;
                        raiseSystemEvent(eFunction.CurrentLocationSunrise, String.Empty, String.Empty, String.Empty);
                    }
                }
                else
                {
                    if (_CurrentLocation_Sunrise_EventState == 0 || _CurrentLocation_Sunrise_EventState != 2)
                    {
                        _CurrentLocation_Sunrise_EventState = 2;
                        raiseSystemEvent(eFunction.CurrentLocationSunset, String.Empty, String.Empty, String.Empty);
                    }
                }
            }

            // Update current clock values (min. resolution 1 minute)
            CurrentClock = DateTime.Now;

            tmrCurrentClock.Enabled = true;
        }

        #endregion

        #region Security

        EventWaitHandle secure = new EventWaitHandle(false, EventResetMode.AutoReset);

        bool Credentials_OnAuthorizationRequested(string pluginName, string requestedAccess)
        {
            OMPanel security = new OMPanel("Security");
            security.BackgroundColor1 = Color.FromArgb(100, Color.Black);
            security.Forgotten = true;
            security.Priority = (ePriority)6;
            OMBasicShape box = new OMBasicShape("",250, 130, 500, 300,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(0, 0, 15), Color.Silver, 3, 15));
            security.addControl(box);
            OMImage img = new OMImage("", 400, 140, 225, 280);
            img.Image = Core.theHost.getSkinImage("Lock");
            security.addControl(img);
            OMLabel label = new OMLabel("", 250, 200, 500, 115);
            label.Text = pluginName + " is requesting access to the credentials cache to access your " + requestedAccess + ".";
            label.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
            label.Font = new Font(Font.Arial, 22F);
            security.addControl(label);
            OMLabel title = new OMLabel("", 260, 135, 480, 60);
            title.Text = "Authorization Required";
            title.Font = new Font(Font.Verdana, 29F);
            title.Format = eTextFormat.Glow;
            title.OutlineColor = Color.Yellow;
            security.addControl(title);
            OMButton accept = new OMButton("", 295, 340, 200, 80);
            accept.Image = Core.theHost.getSkinImage("Full");
            accept.FocusImage = Core.theHost.getSkinImage("Full.Highlighted");
            accept.Text = "Allow";
            accept.OnClick += security_OnClick;
            security.addControl(accept);
            OMButton deny = new OMButton("", 515, 340, 200, 80);
            deny.Image = Core.theHost.getSkinImage("Full");
            deny.FocusImage = Core.theHost.getSkinImage("Full.Highlighted");
            deny.Text = "Deny";
            deny.OnClick += security_OnClick;
            security.addControl(deny);
            for (int i = 0; i < ScreenCount; i++)
            {
                lock (Core.RenderingWindows[i])
                {
                    Core.RenderingWindows[i].TransitionInPanel(security);
                    Core.RenderingWindows[i].ExecuteTransition("None", 0);
                    history.setDisabled(i, true);
                    Core.RenderingWindows[i].blockHome = true;
                }
            }
            secure.WaitOne();
            for (int i = 0; i < ScreenCount; i++)
            {
                lock (Core.RenderingWindows[i])
                {
                    Core.RenderingWindows[i].TransitionOutPanel(security);
                    Core.RenderingWindows[i].ExecuteTransition("None", 0);
                    history.setDisabled(i, false);
                    Core.RenderingWindows[i].blockHome = false;
                }
            }
            if ((accept.Tag != null) && (accept.Tag.ToString() == "True"))
                return true;
            else
                return false;
        }

        void security_OnClick(OMControl sender, int screen)
        {
            sender.Tag = "True";
            secure.Set();
        }

        #endregion

        #region Network access and control

        private string GetLocalIPAddress()
        {
            System.Net.IPHostEntry host;
            string localIP = "";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private bool GetNetWorkAvailable()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        void NetworkStatus_AvailabilityChanged(object sender, NetworkStatusChangedArgs e)
        {
            if (NetWorkAvailable && !e.IsAvailable)
            {   // We disconnected from the network
                NetWorkAvailable = e.IsAvailable;
                InternetAccess = false;
                raiseSystemEvent(eFunction.disconnectedFromInternet, String.Empty, String.Empty, String.Empty);
            }
            else
            {   // We connected to the network
                NetWorkAvailable = e.IsAvailable;

                // Was this just a network connection comming alive or was it a internet connection?
                if (OpenMobile.Net.Network.checkForInternet() == OpenMobile.Net.Network.connectionStatus.InternetAccess)
                {   // Yes, network with internet available
                    InternetAccess = true;
                    raiseSystemEvent(eFunction.connectedToInternet, String.Empty, String.Empty, String.Empty);
                }
                else
                {   // Network only, no internet. Let's keep polling for 10mins to see if it comes alive (in case of a phone connecting etc...)
                    InternetAccess = false;
                    Network_PollForInternetAvailability();
                }
            }

            // Set internal state
            _NetWorkAvailable_PreviousState = e.IsAvailable;
        }

        /// <summary>
        /// Starts a thread that polls for internet availability up to a max time
        /// </summary>
        /// <param name="maxSeconds"></param>
        public void Network_PollForInternetAvailability(int maxSeconds = 600)
        {
            // Start a new thread that monitors the connection
            SandboxedThread.Asynchronous(() =>
            {
                int pollDelay = 10;
                int max = maxSeconds / pollDelay;
                // We're maximum gonna loop this 60 times (60 x 10 sec = 10min)
                for (int i = 0; i < max; i++)
                {
                    // Cancel if no network is available
                    if (!NetworkStatus.IsAvailable)
                        break;

                    // Check for available internet
                    if (OpenMobile.Net.Network.checkForInternet() == OpenMobile.Net.Network.connectionStatus.InternetAccess)
                    {   // Yes, network with internet available
                        InternetAccess = true;
                        raiseSystemEvent(eFunction.connectedToInternet, String.Empty, String.Empty, String.Empty);

                        // Break loop and cancel thread
                        break;
                    }

                    // Let's wait for 10 seconds before trying again
                    Thread.Sleep(pollDelay * 1000);
                }
            });
        }

        /// <summary>
        /// Raises system event when network address is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            foreach (System.Net.IPAddress i in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
            {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    if (i.ToString() == "127.0.0.1")
                    {
                        IPAddress = "0.0.0.0";
                    }
                    else
                    {
                        if (IPAddress != i.ToString())
                        {
                            IPAddress = i.ToString();
                            raiseSystemEvent(eFunction.networkConnectionsAvailable, IPAddress, String.Empty, String.Empty);
                        }
                    }
            }
        }

        /// <summary>
        /// Raises system event when network availability is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            NetWorkAvailable = e.IsAvailable;
            if (e.IsAvailable != _NetWorkAvailable_PreviousState)
            {
                _NetWorkAvailable_PreviousState = e.IsAvailable;
                if (NetWorkAvailable == true)
                    if (OpenMobile.Net.Network.checkForInternet() == OpenMobile.Net.Network.connectionStatus.InternetAccess)
                    {
                        InternetAccess = true;
                        raiseSystemEvent(eFunction.connectedToInternet, String.Empty, String.Empty, String.Empty);
                    }
                    else
                    {
                        InternetAccess = false;
                        raiseSystemEvent(eFunction.disconnectedFromInternet, String.Empty, String.Empty, String.Empty);
                    }
            }
        }

        private bool _NetWorkAvailable_PreviousState;

        #endregion

        #region Screen control and data

        /// <summary>
        /// Raises system events when screens has been added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (_ScreenCount != OpenTK.DisplayDevice.AvailableDisplays.Count)
            {
                if (_ScreenCount < OpenTK.DisplayDevice.AvailableDisplays.Count)
                {
                    // TODO : Better handling of screen added / removed
                    raiseSystemEvent(eFunction.screenAdded, String.Empty, String.Empty, String.Empty);
                    execute(eFunction.restartProgram);
                }
                else
                {
                    raiseSystemEvent(eFunction.screenRemoved, String.Empty, String.Empty, String.Empty);
                }
            }
        }

        /// <summary>
        /// Amount of screens available to OM
        /// </summary>
        public int ScreenCount
        {
            get
            {
                return _ScreenCount;
            }
            set
            {
                _ScreenCount = value;
            }
        }

        /// <summary>
        /// Startup screen of OM (0 is normal screen, any other value means a specific startup screen is requested)
        /// </summary>
        public int StartupScreen { get; set; }

        /// <summary>
        /// Sets the windowstate (maximized/minimized) of the specific screen
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="windowState"></param>
        public void SetWindowState(int Screen, OpenTK.WindowState windowState)
        {
            Core.RenderingWindows[Screen].WindowState = windowState;
        }

        /// <summary>
        /// Sets the windowstate (maximized/minimized) for all screens
        /// </summary>
        /// <param name="windowState"></param>
        public void SetAllWindowState(OpenTK.WindowState windowState)
        {
            for (int i = 0; i < _ScreenCount; i++)
                Core.RenderingWindows[i].WindowState = windowState;
        }

        /// <summary>
        /// Sets the visibility of the pointer cursors in OM
        /// </summary>
        public bool ShowCursors { get; set; }

        /// <summary>
        /// Sets the visibility of DebugInfo
        /// </summary>
        public bool ShowDebugInfo { get; set; }

        /// <summary>
        /// Gets the area available to plugins for placeing controls
        /// <param name="Screen"></param>
        /// </summary>
        public Rectangle GetClientArea(int Screen)
        {
            // Error check
            if (Screen > _ClientArea.GetUpperBound(0) && Screen < _ClientArea.GetUpperBound(0))
                return new Rectangle();
            return _ClientArea[Screen];
        }

        /// <summary>
        /// Sets the area available to plugins for placeing controls
        /// <param name="Screen"></param>
        /// <param name="Area"></param>
        /// </summary>
        public void SetClientArea(int Screen, Rectangle Area)
        {
            // Error check
            if (Screen > _ClientArea.GetUpperBound(0) && Screen < _ClientArea.GetUpperBound(0))
                return;
            SetClientArea_Internal(Screen, Area);
        }
        /// <summary>
        /// Sets the area available to plugins for placeing controls (NB! ALL SCREENS ARE SET TO THE SAME SIZE)
        /// <param name="Area"></param>
        /// </summary>
        public void SetClientArea(Rectangle Area)
        {
            for (int i = 0; i < _ClientArea.Length; i++)
                SetClientArea_Internal(i, Area);
        }

        private void SetClientArea_Internal(int screen, Rectangle area)
        {
            _ClientArea[screen] = area;
            raiseSystemEvent(eFunction.ClientAreaChanged, screen.ToString(), String.Format("{0}|{1}|{2}|{3}", area.X, area.Y, area.Width, area.Height), String.Empty);
        }

        /// <summary>
        /// The area available to plugins for placing controls during initialization time
        /// </summary>
        public Rectangle ClientArea_Init
        {
            get
            {
                return _ClientArea[0];
            }
        }

        /// <summary>
        /// The area available to plugins for placing controls at runtime
        /// </summary>
        public Rectangle[] ClientArea 
        { 
            get
            {
                return _ClientArea;
            }
        }
        private Rectangle[] _ClientArea;

        /// <summary>
        /// The graphical maximum region for OM to use for rectangles
        /// </summary>
        public Rectangle ClientFullArea
        { 
            get
            {
                return _ClientFullArea;
            }
        }
        private Rectangle _ClientFullArea = new Rectangle(0,0,1000,600);


        #endregion

        #region Storage events

        /// <summary>
        /// Raises a storage event
        /// </summary>
        /// <param name="type">Type of media</param>
        /// <param name="justInserted">True = New media</param>
        /// <param name="arg"></param>
        public void RaiseStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            try
            {
                if (OnStorageEvent != null)
                    OnStorageEvent(type, justInserted, arg);
                if (type == eMediaType.NotSet)
                    StorageAnalyzer.AnalyzeAsync(arg, justInserted);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
        }

        #endregion

        #region Execute function

        public void RaiseCloseProgramEvents()
        {
            raiseSystemEvent(eFunction.CloseProgramPreview, String.Empty, String.Empty, String.Empty);
            raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="function">Function to execute</param>
        /// <returns></returns>
        public bool execute(eFunction function)
        {
            switch (function)
            {
                // Exit OM
                case eFunction.closeProgram:
                    //Don't lock this to prevent deadlock
                    {
                        Core.CloseProgram(ShutdownModes.Normal);
                        //if (!closing)
                        //{
                        //    closing = true;
                        //    SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.CloseProgramPreview, String.Empty, String.Empty, String.Empty); });                            
                        //    savePlaylists();
                        //    //SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty); });
                        //    raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty);
                        //    Hal_Send("44");
                        //    Stop();
                        //    RenderingWindow.CloseRenderer();
                        //}
                    }
                    return true;

                // Restart OM
                case eFunction.restartProgram:
                    try
                    {
                        RenderingWindow.CloseAllWindows();
                    }
                    catch (Exception) { }
                    try
                    {
                        ProcessStartInfo info = Process.GetCurrentProcess().StartInfo;
                        info.FileName = Process.GetCurrentProcess().Modules[0].FileName;
                        if (Core.RenderingWindows[0].WindowState == OpenTK.WindowState.Fullscreen)
                            info.Arguments = "-fullscreen";
                        Core.RenderingWindows[0].Exit();
                        Process.Start(info);
                    }
                    catch (Exception) { }
                    Environment.Exit(0);
                    return true;

                // Deaktivate speech controls
                case eFunction.stopListeningForSpeech:
                    raiseSystemEvent(eFunction.stopListeningForSpeech, String.Empty, String.Empty, String.Empty);
                    return true;

                // Activate speech controls
                case eFunction.listenForSpeech:
                    string s;
                    using (PluginSettings set = new PluginSettings())
                        s = set.getSetting(BuiltInComponents.OMInternalPlugin, "Default.Speech");
                    if (s.Length > 0)
                    {
                        IBasePlugin b = Core.pluginCollection.Find(q => q.pluginName == s);
                        if (b == null)
                            return false;
                        ((ISpeech)b).listen();
                        return true;
                    }
                    return false;

                // Shutdown computer
                case eFunction.shutdown:
                    Core.CloseProgram(ShutdownModes.Shutdown);
                    return true;

                // Restart computer
                case eFunction.restart:
                    Core.CloseProgram(ShutdownModes.Restart);
                    return true;

                // Hibernate computer
                case eFunction.hibernate:
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    Core.CloseProgram(ShutdownModes.Hibernate);
                    return true;

                // Set computer in standby
                case eFunction.standby:
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    Core.CloseProgram(ShutdownModes.Suspend);
                   return true;

                // Connect to internet
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this);

                // Disconnect from internet
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this);

                // System settings changed 
                case eFunction.settingsChanged:
                    return true;


            }
            return false;
        }

        /// <summary>
        /// Executes a function with one argument
        /// </summary>
        /// <param name="function">Function to execute</param>
        /// <param name="arg">Argument 1</param>
        /// <returns></returns>
        public bool execute(eFunction function, object arg)
        {
            return execute(function, arg.ToString());
        }
        /// <summary>
        /// Executes a function with one argument
        /// </summary>
        /// <param name="function">Function to execute</param>
        /// <param name="arg">Argument 1</param>
        /// <returns></returns>
        public bool execute(eFunction function, string arg)
        {
            int ret;
            IBasePlugin plugin;
            switch (function)
            {
                // Load a AV player 
                case eFunction.loadAVPlayer:
                    return execute(eFunction.loadAVPlayer, arg, String.Empty);

                // Prompt user to dial
                case eFunction.promptDialNumber:
                    raiseSystemEvent(eFunction.promptDialNumber, arg, String.Empty, String.Empty);
                    return true;

                // Show navigation panel
                case eFunction.showNavPanel:
                    plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (plugin == null)
                        return false;
                    return ((INavigation)plugin).switchTo(arg);

                // Navigate to point of interest
                case eFunction.navigateToPOI:
                    plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (plugin == null)
                        return false;
                    return ((INavigation)plugin).findPOI(arg);
                
                // Execute transition between panels (default effect)
                case eFunction.ExecuteTransition:
                    return execute(eFunction.ExecuteTransition, arg, "");

                // Cancel requested transition
                //case eFunction.CancelTransition:
                //    if (int.TryParse(arg, out ret))
                //    {
                //        lock (Core.RenderingWindows[ret])
                //        {
                //            Core.RenderingWindows[ret].Rollback();
                //        }
                //    }
                //    return false;

                // Set panel that we want to load
                case eFunction.TransitionToPanel:
                    if (int.TryParse(arg, out ret))
                        return execute(eFunction.TransitionToPanel, arg, history.CurrentItem(ret).pluginName, String.Empty);
                    return false;

                // Set panel that should be removed
                case eFunction.TransitionFromPanel:
                    if (int.TryParse(arg, out ret))
                        return execute(eFunction.TransitionFromPanel, arg, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                    return false;

                // Go back to previous history item (using default transition effect)
                case eFunction.goBack:
                    return execute(eFunction.goBack, arg, "");

                // Go back to home (using default transition effect)
                case eFunction.goHome:
                    return execute(eFunction.goHome, arg, "");

                // Unload current Audio/Video player
                case eFunction.unloadAVPlayer:
                    {
                        if (int.TryParse(arg, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            if (currentMediaPlayer == null)
                                return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                    return false;
                                try
                                {
                                    if (currentMediaPlayer[subZone.AudioDevice.Instance].getPlayerStatus(subZone) == ePlayerStatus.Playing)
                                        currentMediaPlayer[subZone.AudioDevice.Instance].stop(subZone);
                                }
                                catch (Exception) { }
                                currentMediaPlayer[subZone.AudioDevice.Instance].OnMediaEvent -= raiseMediaEvent;
                                string name = currentMediaPlayer[subZone.AudioDevice.Instance].pluginName;
                                currentMediaPlayer[subZone.AudioDevice.Instance] = null;
                                raiseMediaEvent(eFunction.unloadAVPlayer, subZone, name);
                                return true;
                            });
                        }
                        return false;
                    }

                // Unload current tuned content
                case eFunction.unloadTunedContent:
                    {
                        if (int.TryParse(arg, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                    return false;
                                currentTunedContent[subZone.AudioDevice.Instance].setPowerState(subZone, false);
                                currentTunedContent[subZone.AudioDevice.Instance].OnMediaEvent -= raiseMediaEvent;
                                string name = currentTunedContent[subZone.AudioDevice.Instance].pluginName;
                                currentTunedContent[subZone.AudioDevice.Instance] = null;
                                raiseMediaEvent(eFunction.unloadTunedContent, subZone, name);
                                return true;
                            });
                        }
                        return false;
                    }

                // Start media playback
                case eFunction.Play:
                    {
                        if (int.TryParse(arg, out ret) == false)
                            return false;
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (currentMediaPlayer == null)
                            return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDevice.Instance].play(subZone);
                        });
                    }

                // Show video window
                case eFunction.showVideoWindow:
                    {
                        if (int.TryParse(arg, out ret) == false)
                            return false;
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (currentMediaPlayer == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDevice.Instance].SetVideoVisible(subZone, true);
                        });
                    }

                // Hide video window
                case eFunction.hideVideoWindow:
                    {
                        if (int.TryParse(arg, out ret) == false)
                            return false;
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (currentMediaPlayer == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if ((subZone.AudioDevice == null) || (currentMediaPlayer[subZone.AudioDevice.Instance] == null))
                                return false;
                            return currentMediaPlayer[subZone.AudioDevice.Instance].SetVideoVisible(subZone, false);
                        });
                    }

                // Pause media playback
                case eFunction.Pause:
                    {
                        if (int.TryParse(arg, out ret) == false)
                            return false;
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (currentMediaPlayer == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                            {
                                if (currentTunedContent[subZone.AudioDevice.Instance] != null)
                                {
                                    if (typeof(IPausable).IsInstanceOfType(currentTunedContent[subZone.AudioDevice.Instance]))
                                        return ((IPausable)currentTunedContent[subZone.AudioDevice.Instance]).pause(subZone);
                                }
                                return false;
                            }
                            return currentMediaPlayer[subZone.AudioDevice.Instance].pause(subZone);
                        });
                    }

                // Stop media playback
                case eFunction.Stop:
                    {
                        if (int.TryParse(arg, out ret) == false)
                            return false;
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (currentMediaPlayer == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDevice.Instance].stop(subZone);
                        });
                    }

                // Change to and play next media
                case eFunction.nextMedia:
                    {
                        if (int.TryParse(arg, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentPlaylist[subZone.AudioDevice.Instance].Count == 0)
                                    return false;
                                currentPlaylistIndex[subZone.AudioDevice.Instance] = nextPlaylistIndex[subZone.AudioDevice.Instance];
                                bool b = execute(eFunction.Play, arg, currentPlaylist[subZone.AudioDevice.Instance][currentPlaylistIndex[subZone.AudioDevice.Instance]].Location);
                                generateNext(subZone, false);
                                return b;
                            });
                        }
                        return false;
                    }

                // Change to and play previous media
                case eFunction.previousMedia:
                    {
                        if (int.TryParse(arg, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentPlaylist[subZone.AudioDevice.Instance].Count == 0)
                                    return false;
                                lock (currentPlaylistIndex)
                                {
                                    currentPlaylistIndex[subZone.AudioDevice.Instance]--;
                                    if (currentPlaylistIndex[subZone.AudioDevice.Instance] <= -1)
                                        currentPlaylistIndex[subZone.AudioDevice.Instance] = currentPlaylist[subZone.AudioDevice.Instance].Count - 1;
                                }
                                bool b = execute(eFunction.Play, arg, currentPlaylist[subZone.AudioDevice.Instance][currentPlaylistIndex[subZone.AudioDevice.Instance]].Location);
                                generateNext(subZone, true);
                                return b;
                            });
                        }
                        return false;
                    }

                // Current tuned content: Scan backwards 
                case eFunction.scanBackward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].scanReverse(subZone);
                        });
                    }
                    return false;

                // Current tuned content: Scan forwards
                case eFunction.scanForward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].scanForward(subZone);
                        });
                    }
                    return false;

                // Current tuned content: Scan band
                case eFunction.scanBand:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].scanBand(subZone);
                        });
                    }
                    return false;

                // Current tuned content: Step backwards (channels)
                case eFunction.stepBackward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].stepBackward(subZone);
                        });
                    }
                    return false;

                // Current tuned content: Step forwards (channels)
                case eFunction.stepForward:

                    if (int.TryParse(arg, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].stepForward(subZone);
                        });
                    }
                    return false;

                // Data is updated by a dataprovider
                case eFunction.dataUpdated:
                    raiseSystemEvent(eFunction.dataUpdated, arg, String.Empty, String.Empty);
                    return true;

                // Background thread status update
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg, String.Empty, String.Empty);
                    return true;

                // Reset a hardware device
                case eFunction.resetDevice:
                    plugin = Core.pluginCollection.Find(q => typeof(IRawHardware).IsInstanceOfType(q) && (q.pluginName == arg));
                    if (plugin == null)
                        return false;
                    try
                    {
                        ((IRawHardware)plugin).resetDevice();
                    }
                    catch (Exception) { return false; }
                    return true;

                // Load speech context
                case eFunction.loadSpeechContext:
                    plugin = Core.pluginCollection.Find(q => typeof(ISpeech).IsInstanceOfType(q));
                    if (plugin == null)
                        return false;
                    return ((ISpeech)plugin).loadContext(arg);
                
                // Speaks the given text
                case eFunction.Speak:
                    plugin = Core.pluginCollection.Find(q => typeof(ISpeech).IsInstanceOfType(q));
                    if (plugin == null)
                        return false;
                    return ((ISpeech)plugin).speak(arg);

                // Connect to internet
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this, arg);

                // Disconnect from internet
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this, arg);

                // Set system volume (arg = volume) on default unit
                case eFunction.setSystemVolume:
                    // TODO : Change this to use default zone
                    return execute(eFunction.setSystemVolume, arg, "0");

                // Unload all panels 
                case eFunction.TransitionFromAny:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= Core.RenderingWindows.Count))
                            return false;
                        lock (Core.RenderingWindows[ret])
                        {
                            if (!Core.RenderingWindows[ret].TransitionOutEverything())
                                return false;
                        }
                        history.setDisabled(ret, false);
                        raiseSystemEvent(eFunction.TransitionFromAny, arg, String.Empty, String.Empty);
                        return true;
                    }
                    return false;

                // Eject disc
                case eFunction.ejectDisc:
                    if (string.IsNullOrEmpty(arg))
                        return false;
                    // TODO
                    return true;

                // Navigation: Navigate to address
                case eFunction.navigateToAddress:
                    if (arg.Length <= 1)
                        return false;
                    plugin = Core.pluginCollection.Find(a => typeof(INavigation).IsInstanceOfType(a));
                    if (plugin != null)
                    {
                        Location adr;
                        if (!((Location.TryParse(arg, out adr) == true) && ((INavigation)plugin).navigateTo(adr)))
                            return false;
                    }
                    raiseSystemEvent(eFunction.navigateToAddress, arg, String.Empty, String.Empty);
                    return true;

                // Deactivate "back" function
                case eFunction.blockGoBack:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        history.setDisabled(ret, true);
                        return true;
                    }
                    return false;

                // Activate "back" function
                case eFunction.unblockGoBack:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        history.setDisabled(ret, false);
                        return true;
                    }
                    return false;

                // Clear panel history (goback)
                case eFunction.clearHistory:
                    if (int.TryParse(arg, out ret))
                    {
                        history.Clear(ret);
                        return true;
                    }
                    return false;

                // System settings changed
                case eFunction.settingsChanged:
                    raiseSystemEvent(eFunction.settingsChanged, arg, String.Empty, String.Empty);
                    return true;

                // Minimize application
                case eFunction.minimize:
                    if (int.TryParse(arg, out ret))
                    {
                        Core.RenderingWindows[ret].WindowState = OpenTK.WindowState.Minimized;
                        return true;
                    }
                    return false;

                // Toggle fullscreen / Windowed application
                case eFunction.ToggleFullscreen:
                     if (int.TryParse(arg, out ret))
                    {
                        if (Core.RenderingWindows[ret].WindowState != OpenTK.WindowState.Fullscreen)
                            Core.RenderingWindows[ret].WindowState = OpenTK.WindowState.Fullscreen;
                        else
                            Core.RenderingWindows[ret].WindowState = OpenTK.WindowState.Normal;
                        return true;
                    }
                    return false;

                // Loads a specified plugin
                case eFunction.loadPlugin:
                    if (Core.loadPlugin(arg))
                    {
                        raiseSystemEvent(eFunction.loadPlugin, arg, String.Empty, String.Empty);
                        return true;
                    }
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Executes a function with two arguments
        /// </summary>
        /// <param name="function">Function to execute</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <returns></returns>
        public bool execute(eFunction function, object arg1, object arg2)
        {
            return execute(function, arg1.ToString(), arg2.ToString());
        }
        /// <summary>
        /// Executes a function with two arguments
        /// </summary>
        /// <param name="function">Function to execute</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <returns></returns>
        public bool execute(eFunction function, string arg1, string arg2)
        {
            int ret, ret2;
            switch (function)
            {
                // Goes to a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.GotoPanel:
                    return execute(eFunction.GotoPanel, arg1, arg2, String.Empty);

                // Hides a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.HidePanel:
                    return execute(eFunction.HidePanel, arg1, arg2, String.Empty);

                // Shows a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.ShowPanel:
                    return execute(eFunction.ShowPanel, arg1, arg2, String.Empty);

                // Transition to the defult panel from a plugin
                case eFunction.TransitionToPanel:
                    return execute(eFunction.TransitionToPanel, arg1, arg2, String.Empty);

                // Transition out the defult panel from a plugin
                case eFunction.TransitionFromPanel:
                    return execute(eFunction.TransitionFromPanel, arg1, arg2, String.Empty);

                // Execute transition with the specified effect
                case eFunction.ExecuteTransition:
                    return execute(eFunction.ExecuteTransition, arg1, arg2, String.Empty);

                // Go back to previous panel in history using the specified effect
                case eFunction.goBack:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (history.getDisabled(ret) == true)
                        {
                            raiseSystemEvent(eFunction.goBack, arg1, String.Empty, String.Empty);
                            return false;
                        }
                        if ((history.Count(ret) == 0) || (history.Peek(ret).pluginName == null))
                            return false;
                        raiseSystemEvent(eFunction.goBack, arg1, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                        execute(eFunction.TransitionFromPanel, arg1, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                        //raiseSystemEvent(eFunction.TransitionFromPanel, arg1, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                        while ((history.Count(ret) > 1) && (history.Peek(ret).Equals(history.CurrentItem(ret))))
                            history.Dequeue(ret);
                        //This part is done manually to prevent adding it to the history
                        OMPanel k = getPanelByName(history.Peek(ret).pluginName, history.Peek(ret).panelName, ret);
                        if (k == null)
                            return false;
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].TransitionInPanel(k);
                        }
                        raiseSystemEvent(eFunction.TransitionToPanel, arg1, history.Peek(ret).pluginName, history.Peek(ret).panelName);
                        history.Dequeue(ret);
                        execute(eFunction.ExecuteTransition, arg1, arg2);
                        return true;
                    }
                    return false;

                // Go back to home
                case eFunction.goHome:
                    {
                        // Convert argument to screen
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if (execute(eFunction.TransitionFromAny, ret))
                            {
                                execute(eFunction.hideVideoWindow, ret);
                                execute(eFunction.clearHistory, ret);
                                execute(eFunction.TransitionToPanel, ret, "MainMenu");
                                execute(eFunction.ExecuteTransition, ret, arg2);
                                raiseSystemEvent(eFunction.goHome, arg1, arg2, String.Empty);
                                return true;
                            }
                        }
                    }
                    return false;

                // Input from user is ready (like OSK data)
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, String.Empty);
                    return true;

                // Load a AV player with the specified instance from the specified plugin
                case eFunction.loadAVPlayer:
                    {
                        //int zoneID = -1;
                        //if (int.TryParse(arg1, out zoneID))
                        //{
                        //    Zone zone = ZoneHandler.GetZone(zoneID);
                        //    // Errorcheck
                        //    if (zone == null) return false;

                        //    // Get available player
                        //    IMediaProvider player = null;
                        //    if (String.IsNullOrEmpty(arg2))
                        //    {   // Find first matching player
                        //        List<IBasePlugin> alternatePlayers = Core.pluginCollection.FindAll(i => typeof(IMediaProvider).IsInstanceOfType(i));
                        //        player = (IMediaProvider)alternatePlayers.Find(x => (((IMediaProvider)x).MediaProviderType & MediaProviderTypes.AudioPlayback) == MediaProviderTypes.AudioPlayback);
                        //    }
                        //    else
                        //        // Find specific player
                        //        player = (IMediaProvider)Core.pluginCollection.FindAll(i => typeof(IMediaProvider).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);

                        //    // Errorcheck
                        //    if (player == null) return false;

                        //    // Initialize player
                        //    if (player.ActivateMediaProvider(zone))
                        //        // Map player
                        //        zone.MediaHandler.ActiveMediaProvider = player;

                        //    return true;
                        //}
                        return false;


                        //if (int.TryParse(arg1, out ret) == true)
                        //{
                        //    Zone zone = ZoneHandler.GetZone(ret);
                        //    if (zone == null) return false;
                        //    if (currentMediaPlayer == null) return false;
                        //    return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        //    {
                        //        if (currentMediaPlayer[subZone.AudioDevice.Instance] != null)
                        //            return false;
                        //        if (currentTunedContent[subZone.AudioDevice.Instance] != null)
                        //            execute(eFunction.unloadTunedContent, arg1);
                        //        IAVPlayer player = (IAVPlayer)Core.pluginCollection.FindAll(i => typeof(IAVPlayer).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                        //        if (player == null)
                        //            return false;

                        //        //Hook events it if its not already hooked
                        //        if (Array.Exists<IAVPlayer>(currentMediaPlayer, a => a == player) == false)
                        //            player.OnMediaEvent += raiseMediaEvent;

                        //        currentMediaPlayer[subZone.AudioDevice.Instance] = player;
                        //        raiseMediaEvent(eFunction.loadAVPlayer, subZone, arg2);
                        //        return true;
                        //    });
                        //}
                        //return false;
                    }

                // Load a tuned content with the specified instance from the specified plugin
                case eFunction.loadTunedContent:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            if (currentMediaPlayer == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentTunedContent[subZone.AudioDevice.Instance] != null)
                                    return false;
                                if (currentMediaPlayer[subZone.AudioDevice.Instance] != null)
                                    execute(eFunction.unloadAVPlayer, arg1);
                                ITunedContent player = (ITunedContent)Core.pluginCollection.FindAll(i => typeof(ITunedContent).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                                if (player == null)
                                    return false;
                                //Only hook it once
                                if (Array.Exists<ITunedContent>(currentTunedContent, a => a == player) == false)
                                    player.OnMediaEvent += raiseMediaEvent;
                                currentTunedContent[subZone.AudioDevice.Instance] = player;
                                if (!currentTunedContent[subZone.AudioDevice.Instance].setPowerState(subZone, true))
                                {
                                    execute(eFunction.unloadTunedContent, arg1);
                                    return false;
                                }
                                raiseMediaEvent(eFunction.loadTunedContent, subZone, arg2);
                                return true;
                            });
                        }
                        return false;
                    }

                // Set the current position in a playlist
                case eFunction.setPlaylistPosition:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            //if ((ret < 0) || (ret >= 8))
                            //    return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (int.TryParse(arg2, out ret2) == true)
                                {
                                    if (currentPlaylist[subZone.AudioDevice.Instance].Count > ret2)
                                    {
                                        currentPlaylistIndex[subZone.AudioDevice.Instance] = ret2;
                                        generateNext(subZone, false);
                                        return true;
                                    }
                                    return false;
                                }
                                currentPlaylistIndex[subZone.AudioDevice.Instance] = currentPlaylist[subZone.AudioDevice.Instance].FindIndex(p => p.Location == arg2);
                                if (currentPlaylist[subZone.AudioDevice.Instance].Count > 0)
                                    generateNext(subZone, false);
                                return true;
                            });
                        }
                        return false;
                    }

                // Play media
                case eFunction.Play:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if (arg2 == null)
                                return false;
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            if (currentMediaPlayer == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                    return findAlternatePlayerAndStartPlay(subZone, arg2, StorageAnalyzer.classifySource(arg2));
                                if (currentMediaPlayer[subZone.AudioDevice.Instance].play(subZone, arg2, StorageAnalyzer.classifySource(arg2)) == false)
                                    return findAlternatePlayerAndStartPlay(subZone, arg2, StorageAnalyzer.classifySource(arg2));
                                else
                                    return true;
                            });
                        }
                        return false;
                    }

                // Set playback position of the current media
                case eFunction.setPosition:
                    {
                        try
                        {
                            if (int.TryParse(arg1, out ret) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(ret);
                                if (zone == null) return false;
                                if (currentMediaPlayer == null) return false;
                                return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                                {
                                    if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                        return false;
                                    float pos;
                                    if (float.TryParse(arg2, out pos))
                                        return currentMediaPlayer[subZone.AudioDevice.Instance].setPosition(subZone, pos);
                                    return false;
                                });
                            }
                            return false;
                        }
                        catch (Exception) { return false; }
                    }

                // Set playback speed
                case eFunction.setPlaybackSpeed:
                    {
                        try
                        {
                            if (int.TryParse(arg1, out ret) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(ret);
                                if (zone == null) return false;
                                if (currentMediaPlayer == null) return false;
                                return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                                {
                                    if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                        return false;
                                    float speed;
                                    if (float.TryParse(arg2, out speed))
                                        return currentMediaPlayer[subZone.AudioDevice.Instance].setPlaybackSpeed(subZone, speed);
                                    return false;
                                });
                            }
                            return false;
                        }
                        catch (Exception) { return false; }
                    }

                // Set the volume of a specific player instance (tunedcontent or AV player)
                case eFunction.setPlayerVolume:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            if (currentMediaPlayer == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentMediaPlayer[subZone.AudioDevice.Instance] == null)
                                {
                                    if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                        return false;
                                    raiseMediaEvent(eFunction.setPlayerVolume, subZone, arg2);
                                    if (int.TryParse(arg2, out ret2))
                                        if (typeof(IPlayerVolumeControl).IsInstanceOfType(currentTunedContent[zone.AudioDevice.Instance]))
                                            ((IPlayerVolumeControl)currentTunedContent[zone.AudioDevice.Instance]).setVolume(subZone, ret2);
                                }
                                else
                                {
                                    raiseMediaEvent(eFunction.setPlayerVolume, subZone, arg2);
                                    if (int.TryParse(arg2, out ret2))
                                        if (typeof(IPlayerVolumeControl).IsInstanceOfType(currentMediaPlayer[zone.AudioDevice.Instance]))
                                            ((IPlayerVolumeControl)currentMediaPlayer[zone.AudioDevice.Instance]).setVolume(subZone, ret2);
                                }
                                return false;
                            });
                        }
                        return false;
                    }

                // Tuned content: Tune to given data
                case eFunction.tuneTo:

                    if (int.TryParse(arg1, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        if (arg2 == null)
                            return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return findAlternateTuner(subZone, arg2);
                            if (currentTunedContent[subZone.AudioDevice.Instance].tuneTo(subZone, arg2) == false)
                                return findAlternateTuner(subZone, arg2);
                            else
                                return true;
                        });
                    }
                    return false;

                // Background operation status update
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg1, arg2, String.Empty); 
                    return true;

                // Sends a keypress to a specified screen
                case eFunction.sendKeyPress:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (string.IsNullOrEmpty(arg2))
                            return false;
                        return (InputRouter.SendKeyDown(ret, arg2) && InputRouter.SendKeyUp(ret, arg2));
                    }
                    return false;

                // A gesture has been recognized
                // TODO_: Remove this
                /*
                case eFunction.gesture:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        // Only raise this event if a character was recognized
                        if (!String.IsNullOrEmpty(arg2))
                        {
                            SandboxedThread.Asynchronous(delegate()
                            {
                                // TODO_ : We need a way of canceling this event if it's handled 
                                raiseSystemEvent(eFunction.gesture, arg1, arg2, String.Format("{0}|{1}", history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName));
                            });
                            return true;
                        }
                    }
                    return false;
                    */

                // A multitouch gesture has been recognized
                case eFunction.multiTouchGesture:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        raiseSystemEvent(eFunction.multiTouchGesture, arg1, arg2, history.CurrentItem(ret).pluginName);
                        return true;
                    }
                    return false;

                // Network is connected
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this, arg1, arg2);

                // Set system volume for the specified instance
                case eFunction.setSystemVolume:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if ((ret < -2) || (ret > 100))
                                return false;
                            if (int.TryParse(arg2, out ret) == false)
                                return false;
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            // TODO
                            //raiseSystemEvent(eFunction.systemVolumeChanged, arg1, zone.AudioDeviceInstance.ToString(), String.Empty);
                            return true;
                        }
                        return false;
                    }

                // Set subwoofer channel volume for specified instance
                case eFunction.setSubVolume:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if ((ret < -2) || (ret > 100))
                                return false;
                            if (int.TryParse(arg2, out ret2) == false)
                                return false;
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            //if ((ret2 < 0) || (ret2 >= _AudioDeviceCount))
                            //    return false;
                            // TODO
                            return true;
                        }
                        return false;
                    }

                // Tuned content: Set band
                case eFunction.setBand:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        Zone zone = ZoneHandler.GetZone(ret);
                        if (zone == null) return false;
                        return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                        {
                            if (currentTunedContent[subZone.AudioDevice.Instance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDevice.Instance].setBand(subZone, (eTunedContentBand)Enum.Parse(typeof(eTunedContentBand), arg2, false));
                        });
                    }
                    return false;

                // Set monitor brightness on a specified screen
                case eFunction.setMonitorBrightness:
                    // TODO
                    return false;

                // Set balance for specified instance
                case eFunction.setSystemBalance:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            //if ((ret < 0) || (ret >= _AudioDeviceCount))
                            //    return false;
                            if (int.TryParse(arg2, out ret2) == false)
                                return false;
                            // TODO
                            return true;
                        }
                        return false;
                    }

                // Manually set the audio instance for a screen
                    /* Currently replaced by zones!
                case eFunction.impersonateInstanceForScreen:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= screenCount))
                            return false;
                        if (int.TryParse(arg2, out ret2))
                        {
                            if ((ret2 < 0) || (ret2 >= _AudioDeviceCount))
                                return false;
                            _AudioDeviceInstanceMapping[ret] = ret2 + 1;
                            raiseSystemEvent(eFunction.impersonateInstanceForScreen, arg1, arg2, String.Empty);
                        }
                    }
                    return false;
                    */
            }
            return false;
        }

        /// <summary>
        /// Executes a function with three arguments
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public bool execute(eFunction function, object arg1, object arg2, object arg3)
        {
            return execute(function, arg1.ToString(), arg2.ToString(), arg3.ToString());
        }
        /// <summary>
        /// Executes a function with three arguments
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public bool execute(eFunction function, string arg1, string arg2, string arg3)
        {
            OMPanel panel;
            int ret;
            switch (function)
            {
                // Goes to a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.GotoPanel:
                    {
                        int screen;
                        if (int.TryParse(arg1, out screen) == true)
                        {
                            // Is panel name present?
                            if (arg2.Contains(";"))
                            {
                                string[] sParts = arg2.Split(';');
                                if (sParts.Length >= 2)
                                {
                                    execute(eFunction.TransitionFromAny, screen.ToString());
                                    if (execute(eFunction.TransitionToPanel, screen.ToString(), sParts[0], sParts[1]))
                                        return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                                }
                            }
                            else
                            {   // Panel name is not present
                                execute(eFunction.TransitionFromAny, screen.ToString());
                                if (execute(eFunction.TransitionToPanel, screen.ToString(), arg2))
                                    return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                            }
                        }
                    }
                    return false;

                // Shows a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.ShowPanel:
                    {
                        int screen;
                        if (int.TryParse(arg1, out screen) == true)
                        {
                            // Is panel name present?
                            if (arg2.Contains(";"))
                            {
                                string[] sParts = arg2.Split(';');
                                if (sParts.Length >= 2)
                                {
                                    if (execute(eFunction.TransitionToPanel, screen.ToString(), sParts[0], sParts[1]))
                                        return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                                }
                            }
                            else
                            {   // Panel name is not present
                                if (execute(eFunction.TransitionToPanel, screen.ToString(), arg2))
                                    return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                            }
                        }
                    }
                    return false;

                // Hides a panel (NB! full panel name is used here (PluginName;PanelName)
                case eFunction.HidePanel:
                    {
                        int screen;
                        if (int.TryParse(arg1, out screen) == true)
                        {
                            // Is panel name present?
                            if (arg2.Contains(";"))
                            {
                                string[] sParts = arg2.Split(';');
                                if (sParts.Length >= 2)
                                {
                                    if (execute(eFunction.TransitionFromPanel, screen.ToString(), sParts[0], sParts[1]))
                                        return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                                }
                            }
                            else
                            {   // Panel name is not present
                                if (execute(eFunction.TransitionFromPanel, screen.ToString(), arg2))
                                    return execute(eFunction.ExecuteTransition, screen.ToString(), arg3);
                            }
                        }
                    }
                    return false;
                
                // Execute transition with the specified effect and speed
                case eFunction.ExecuteTransition:
                    string effect;

                    if (_GraphicsLevel == eGraphicsLevel.Minimal)
                        effect = "None";
                    else if (string.IsNullOrEmpty(arg2))
                        effect = "";
                    else
                        effect = arg2;

                    // Get transition effect
                    float transSpeed = 0;
                    if (!string.IsNullOrEmpty(arg3))
                        float.TryParse(arg3, out transSpeed);

                    if (int.TryParse(arg1, out ret) == true)
                    {
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].ExecuteTransition(effect, transSpeed);
                        }
                        raiseSystemEvent(eFunction.ExecuteTransition, arg1, effect, String.Empty);
                        return true;
                    }
                    return false;

                // Load a specific panel from a specific plugin
                case eFunction.TransitionToPanel:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        panel = getPanelByName(arg2, arg3, ret);
                        if (panel == null)
                            return false;
                        if (panel.Priority > ePriority.Urgent) //prevent panels from entering the security layer
                            panel.Priority = ePriority.Urgent;

                        // Update reference to last owner of panel
                        panel.OwnerPlugin = getPluginByName(arg2);

                        lock (Core.RenderingWindows[ret])
                        {
                            if (!Core.RenderingWindows[ret].TransitionInPanel(panel))
                                return false;
                            raiseSystemEvent(eFunction.TransitionToPanel, arg1, (panel.OwnerPlugin != null ? panel.OwnerPlugin.pluginName : ""), panel.Name);
                            if (!panel.UIPanel)
                                history.Enqueue(ret, arg2, panel.Name, panel.Forgotten);
                        }
                        return true;
                    }
                    return false;

                // Unload a specific panel from a specific plugin
                case eFunction.TransitionFromPanel:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        panel = getPanelByName(arg2, arg3, ret);
                        if (panel == null)
                            return false;
                        lock (Core.RenderingWindows[ret])
                        {
                            if (!Core.RenderingWindows[ret].TransitionOutPanel(panel))
                                return false;
                        }
                        raiseSystemEvent(eFunction.TransitionFromPanel, arg1, (panel.OwnerPlugin != null ? panel.OwnerPlugin.pluginName : ""), panel.Name);
                        return true;
                    }
                    return false;

                // User input is ready (Like OSK input)
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, arg3);
                    return true;
            }
            return false;
        }

        #endregion

        #region Tuned Content

        /// <summary>
        /// Find first tuned content plugin that can tune to requested data
        /// </summary>
        /// <param name="zone">Zone</param>
        /// <param name="arg2">Data to tune to</param>
        /// <returns></returns>
        private bool findAlternateTuner(Zone zone, string arg2)
        {
            if (currentMediaPlayer == null) return false;
            if (currentMediaPlayer[zone.AudioDevice.Instance] != null)
                execute(eFunction.unloadAVPlayer, zone.ToString());
            ITunedContent tmp = currentTunedContent[zone.AudioDevice.Instance];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadTunedContent, zone.ToString());
                    execute(eFunction.loadTunedContent, zone.ToString(), plugins[i].pluginName);
                    if ((currentTunedContent[zone.AudioDevice.Instance] != null) && (currentTunedContent[zone.AudioDevice.Instance].tuneTo(zone, arg2) == true))
                        return true;
                }
            }
            if (tmp == null)
                execute(eFunction.unloadTunedContent, zone.ToString());
            else
                execute(eFunction.loadTunedContent, zone.ToString(), tmp.pluginName);
            return false;
        }

        #endregion

        #region AVPlayers

        /// <summary>
        /// Find first AV player plugin that can play requested data
        /// </summary>
        /// <param name="ret">Instance</param>
        /// <param name="arg2">Media to play</param>
        /// <param name="type">Type of media</param>
        /// <returns></returns>
        private bool findAlternatePlayerAndStartPlay(Zone zone, string arg2, eMediaType type)
        {
            if (currentTunedContent[zone.AudioDevice.Instance] != null)
                execute(eFunction.unloadTunedContent, zone.ToString());
            IAVPlayer tmp = currentMediaPlayer[zone.AudioDevice.Instance];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(IAVPlayer).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadAVPlayer, zone.ToString());
                    execute(eFunction.loadAVPlayer, zone.ToString(), plugins[i].pluginName);
                    if (currentMediaPlayer[zone.AudioDevice.Instance].play(zone, arg2, type) == true)
                        return true;
                }
            }
            if (tmp == null)
                execute(eFunction.unloadAVPlayer, zone.ToString());
            else
                execute(eFunction.loadAVPlayer, zone.ToString(), tmp.pluginName);
            raiseMediaEvent(eFunction.playbackFailed, zone, arg2);
            return false;
        }

        /// <summary>
        /// Gets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <returns></returns>
        public bool getRandom(int Screen)
        {
            return getRandom(ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Gets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <returns></returns>
        public bool getRandom(Zone zone)
        {
            if (zone == null)
                return false;
            if (_AudioDeviceCount == -1)
                return false;
            if ((zone.AudioDevice.Instance < 0) || (zone.AudioDevice.Instance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];
            return random[zone.AudioDevice.Instance];
        }
        /// <summary>
        /// Gets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <returns></returns>
        public bool getRandomForAudioDeviceInstance(int AudioDeviceInstance)
        {
            if (_AudioDeviceCount == -1)
                return false;
            if ((AudioDeviceInstance < 0) || (AudioDeviceInstance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];
            return random[AudioDeviceInstance];
        }


        /// <summary>
        /// Sets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <param name="value">random state</param>
        /// <returns></returns>
        public bool setRandom(int Screen, bool value)
        {
            return setRandom(ZoneHandler.GetActiveZone(Screen), value);
        }
        /// <summary>
        /// Sets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <param name="value">random state</param>
        /// <returns></returns>
        public bool setRandom(Zone zone, bool value)
        {
            if (zone == null)
                return false;
            if (_AudioDeviceCount == -1)
                return false;
            if ((zone.AudioDevice.Instance < 0) || (zone.AudioDevice.Instance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];

            // Only send events and update if the state was actually changed
            if (value != random[zone.AudioDevice.Instance])
            {
                random[zone.AudioDevice.Instance] = value;
                if (value)
                    raiseMediaEvent(eFunction.RandomChanged, zone, "Enabled");
                else
                    raiseMediaEvent(eFunction.RandomChanged, zone, "Disabled");
                generateNext(zone, false);
            }
            return true;
        }
        /// <summary>
        /// Sets the random state for a given AV player instance
        /// </summary>
        /// <param name="instance">AV Player instance</param>
        /// <param name="value">random state</param>
        /// <returns></returns>
        public bool setRandomForAudioDeviceInstance(int AudioDeviceInstance, bool value)
        {
            if (_AudioDeviceCount == -1)
                return false;
            if ((AudioDeviceInstance < 0) || (AudioDeviceInstance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];

            // Only send events and update if the state was actually changed
            if (value != random[AudioDeviceInstance])
            {
                random[AudioDeviceInstance] = value;

                // Raise event zones using this audio instance
                Zone zone = ZoneHandler.BaseZones.Find(x => x.AudioDevice.Instance == AudioDeviceInstance);
                if (zone != null)
                {
                    if (value)
                        raiseMediaEvent(eFunction.RandomChanged, zone, "Enabled");
                    else
                        raiseMediaEvent(eFunction.RandomChanged, zone, "Disabled");
                    generateNext(zone, false);
                }
            }
            return true;
        }

        /// <summary>
        /// Get the current playing media for the given instance (AVPlayer or tuned content)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public mediaInfo getPlayingMedia(int Screen)
        {
            return getPlayingMedia(ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Get the current playing media for the given instance (AVPlayer or tuned content)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public mediaInfo getPlayingMedia(Zone zone)
        {
            if (zone == null)
                return null;
            if (currentMediaPlayer == null) return null;
            if (currentMediaPlayer[zone.AudioDevice.Instance] != null)
                return currentMediaPlayer[zone.AudioDevice.Instance].getMediaInfo(zone);
            if (currentTunedContent[zone.AudioDevice.Instance] != null)
                return currentTunedContent[zone.AudioDevice.Instance].getMediaInfo(zone);
            return new mediaInfo();
        }

        /// <summary>
        /// Get the next playing media for the given instance (AVPlayer)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public mediaInfo getNextMedia(int Screen)
        {
            return getNextMedia(ZoneHandler.GetZone(Screen));
        }
        /// <summary>
        /// Get the next playing media for the given instance (AVPlayer)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public mediaInfo getNextMedia(Zone zone)
        {
            if (zone == null)
                return null;
            if (currentMediaPlayer == null) return null;
            if (currentMediaPlayer[zone.AudioDevice.Instance] == null)
                return new mediaInfo();
            return currentPlaylist[zone.AudioDevice.Instance][nextPlaylistIndex[zone.AudioDevice.Instance]];
        }

        #endregion

        #region Message handling

        /// <summary>
        /// Sends a message to/from a specific plugin
        /// </summary>
        /// <param name="to">Plugin to send message to</param>
        /// <param name="from">(Optional) plugin that sends the message</param>
        /// <param name="message">Message to send</param>
        /// <returns></returns>
        public bool sendMessage(string to, string from, string message)
        {
            // Is this a message for a rendering window
            if (to == "RenderingWindow")
            {
                if (message == "Identify")
                    for (int i = 0; i < _ScreenCount; i++)
                        Core.RenderingWindows[i].PaintIdentity();
                else if (message == "MakeCurrent")
                    Core.RenderingWindows[0].MakeCurrent();
                return true;
            }
            try
            {
                // ?????
                if (to.StartsWith("Screen"))
                {
                    int screen;
                    if ((to.Length > 6) && (int.TryParse(to.Substring(6), out screen)))
                        to = history.CurrentItem(screen).pluginName;
                }

                // Send message out to plugin
                IBasePlugin plugin = Core.pluginCollection.Find(i => i.pluginName == to);
                {
                    if (plugin == null)
                        return false;
                    return plugin.incomingMessage(message, from);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a message of a specific type to/from a specific plugin 
        /// </summary>
        /// <typeparam name="T">Type of data in message</typeparam>
        /// <param name="to">Plugin to send to </param>
        /// <param name="from">(Optional) plugin that sends the message</param>
        /// <param name="message">Message to send</param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        public bool sendMessage<T>(string to, string from, string message, ref T data)
        {
            // Is this a message for the thread handling internal in the core?
            if (to == "SandboxedThread")
            {
                SandboxedThread.Handle(data as Exception);
                return true;
            }
            try
            {
                // Send message to plugin
                IBasePlugin plugin = Core.pluginCollection.Find(i => i.pluginName == to);
                {
                    if (plugin == null)
                        return false;
                    return plugin.incomingMessage(message, from, ref data);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Events

        public event SystemEvent OnSystemEvent;
        public event MediaEvent OnMediaEvent;
        public event PowerEvent OnPowerChange;
        public event StorageEvent OnStorageEvent;
        public event NavigationEvent OnNavigationEvent;
        public event KeyboardEvent OnKeyPress;
        public event WirelessEvent OnWirelessEvent;
        public event GestureEvent OnGesture;

        /// <summary>
        /// Raises systemwide events
        /// </summary>
        /// <param name="e"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public void raiseSystemEvent(eFunction e, params object[] args)
        {
            // Log arguments
            string sArgs = "";
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    sArgs += String.Format(", arg{0}: {1}", i, args[i]);
                }
            }
            System.Diagnostics.Debug.WriteLine(String.Format("raiseSystemEvent( eFunction: {0}, {1}", e, sArgs));

            // Update data sources
            if (e == eFunction.connectedToInternet)
            {
                InternetAccess = true;
                InternetAvailable_Exec();
            }
            else if (e == eFunction.disconnectedFromInternet)
                InternetAccess = false;

            else if (e == eFunction.pluginLoadingComplete)
            {
                PluginLoadingCompleted_Exec();
            }

            try
            {
                if (OnSystemEvent != null)
                    OnSystemEvent(e, args);
            }
            catch (Exception error) { SandboxedThread.Handle(error); }
        }

        /// <summary>
        /// Raises systemwide power events
        /// </summary>
        /// <param name="e"></param>
        public void raisePowerEvent(ePowerEvent e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("raisePowerEvent( ePowerEvent: {0}",e));
            if (e == ePowerEvent.SleepOrHibernatePending)
            {
                if (suspending)
                    return; //Already going
                suspending = true;
                try
                {
                    if (OnPowerChange != null)
                        OnPowerChange(e);
                }
                catch (Exception error)
                {
                    SandboxedThread.Handle(error);
                }
                return;
            }
            else if (e == ePowerEvent.SystemResumed)
                suspending = false;
            if (OnPowerChange != null)
                SandboxedThread.Asynchronous(delegate() { OnPowerChange(e); });
        }

        /// <summary>
        /// Raises systemwide navigation events
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        public void raiseNavigationEvent(eNavigationEvent type, string arg)
        {
            if (type != eNavigationEvent.LocationChanged)
                System.Diagnostics.Debug.WriteLine(String.Format("raiseNavigationEvent( eNavigationEvent: {0}, arg: {1}", type, arg));
            try
            {
                if (OnNavigationEvent != null)
                    OnNavigationEvent(type, arg);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
        }

        /// <summary>
        /// Raises systemwide wireless events
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        public void raiseWirelessEvent(eWirelessEvent type, string arg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("raiseWirelessEvent( eWirelessEvent: {0}, arg: {1}", type, arg));
            try
            {
                if (OnWirelessEvent != null)
                    OnWirelessEvent(type, arg);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
        }

        /// <summary>
        /// Raises systemwide media events
        /// </summary>
        /// <param name="e"></param>
        /// <param name="instance"></param>
        /// <param name="arg"></param>
        public void raiseMediaEvent(eFunction type, Zone zone, string arg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("raiseMediaEvent( eFunction: {0}, instance: {1}({2}), arg: {3}", type, zone, (zone != null ? zone.Name : ""), arg));
            if (OnMediaEvent != null)
                SandboxedThread.Asynchronous(delegate() { OnMediaEvent(type, zone, arg); });

            if (type == eFunction.nextMedia)
                while ((!execute(eFunction.nextMedia, zone.ToString())) && (currentPlaylist[zone.AudioDevice.Instance].Count > 1))
                    Thread.Sleep(200);
        }

        /// <summary>
        /// Raises systemwide keypress events
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool raiseKeyPressEvent(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("raiseKeyPressEvent( eKeypressType: {0}, Key: {1}, KeyCode:{2}, Screen:{3}, Character:{4}", type, arg.Key, arg.KeyCode, arg.Screen, arg.Character));
            bool Handled = false;
            try
            {
                if (OnKeyPress != null)
                {
                    // Manually call each delegate, if one handles the keypress then the rest will be blocked
                    foreach (KeyboardEvent d in OnKeyPress.GetInvocationList())
                    {
                        if (d.Invoke(type, arg, ref Handled))
                            Handled = true;
                        if (Handled)
                            break;
                    }
                }
                return Handled;
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
            return false;
        }

        /// <summary>
        /// Raises systemwide gesture events
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        internal void raiseGestureEvent(int screen, string character, OMPanel panel, OMControl highlightedControl)
        {
            if (String.IsNullOrEmpty(character))
                return;
            
            string PluginName = "";
            string PanelName = "";

            // Get panel and plugin name
            if (highlightedControl != null)
            {
                PanelName = highlightedControl.Parent.Name;
                if (highlightedControl.Parent.OwnerPlugin != null)
                    PluginName = highlightedControl.Parent.OwnerPlugin.pluginName;
            }
            else
            {
                if (panel != null)
                {
                    PanelName = panel.Name;
                    PluginName = panel.OwnerPlugin.pluginName;
                }
                else
                {
                    // This is the backup method of getting the data
                    PanelName = history.CurrentItem(screen).panelName;
                    PluginName = history.CurrentItem(screen).pluginName;
                }
            }

            System.Diagnostics.Debug.WriteLine(String.Format("raiseGestureEvent( screen: {0}, character: {1}, pluginName:{2}, panelName:{3}", screen, character, PluginName, PanelName));

            bool Handled = false;
            try
            {
                if (OnGesture != null)
                {
                    SandboxedThread.Asynchronous(delegate()
                    {   // Manually call each delegate, if one handles the event then the rest will be blocked
                        Delegate[] InvocationList = OnGesture.GetInvocationList();
                        for (int i = InvocationList.Length-1; i >= 0; i--)
                        {
                            GestureEvent d = InvocationList[i] as GestureEvent;
                            if (d != null)
                            {
                                if (d.Invoke(screen, character, PluginName, PanelName, ref Handled))
                                    Handled = true;
                                if (Handled)
                                    break;
                            }
                        }
                    });
                }
                return;
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
            return;
        }

        #endregion

        #region Skin and plugin image handling

        /// <summary>
        /// Loads a sprite from the skin folder
        /// </summary>
        /// <param name="baseImageName"></param>
        /// <param name="sprites">An array of Sprite data (name and coordinates)</param>
        /// <returns></returns>
        public bool LoadSkinSprite(string baseImageName, params Sprite[] sprites)
        {
            imageItem baseImage;
            string fullImageName = Path.Combine(SkinPath, baseImageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            #region load image file without using the cache

            if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
            {
                baseImage = imageCache[fullImageName];
            }
            else
            {
                try
                {
                    // Try to load from current skin path
                    imageItem im = getImageFromFile(fullImageName, true);

                    // try to load from default skin path
                    if (im.image == null)
                        im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), baseImageName.Replace('|', System.IO.Path.DirectorySeparatorChar)), true);

                    if (im.image == null)
                    {
                        // Write log entry
                        DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin sprite: {0}", fullImageName));
                        return false;
                    }
                    else
                        baseImage = im;
                }
                catch (System.ArgumentException)
                {
                    return false;
                }
            }

            #endregion

            if (baseImage == imageItem.NONE || baseImage.image == null || baseImage.image.image == null)
                return false;

            System.Drawing.Bitmap baseBmp = baseImage.image.image;

            try
            {
                // Process sprites
                for (int i = 0; i < sprites.Length; i++)
                {
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(sprites[i].Coordinates.Width, sprites[i].Coordinates.Height);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        g.DrawImage(baseBmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), sprites[i].Coordinates.ToSystemRectangle(), System.Drawing.GraphicsUnit.Pixel);

                    if (bmp == null)
                        continue;

                    // Load to cache
                    imageItem im = new imageItem(new OImage(bmp), String.Format("{0}:{1}", fullImageName, sprites[i].Name));
                    imageCache.Add(im.name, im);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a sprite from the plugin folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="baseImageName"></param>
        /// <param name="sprites">An array of Sprite data (name and coordinates)</param>
        /// <returns></returns>
        public bool LoadPluginSprite(IBasePlugin plugin, string baseImageName, params Sprite[] sprites)
        {
            imageItem baseImage;

            // Get paths
            string PluginPath = System.Reflection.Assembly.GetAssembly(plugin.GetType()).Location;
            PluginPath = System.IO.Path.GetDirectoryName(PluginPath);
            string fullImageName = Path.Combine(PluginPath, baseImageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            #region load image file without using the cache

            if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
            {
                baseImage = imageCache[fullImageName];
            }
            else
            {
                try
                {
                    // Try to load from current skin path
                    imageItem im = getImageFromFile(fullImageName);

                    // try to load from default skin path
                    if (im.image == null)
                        im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), baseImageName.Replace('|', System.IO.Path.DirectorySeparatorChar)));

                    if (im.image == null)
                    {
                        // Write log entry
                        DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing plugin sprite: {0}", fullImageName));
                        return false;
                    }
                    else
                        baseImage = im;
                }
                catch (System.ArgumentException)
                {
                    return false;
                }
            }

            #endregion

            if (baseImage == imageItem.NONE || baseImage.image == null || baseImage.image.image == null)
                return false;

            System.Drawing.Bitmap baseBmp = baseImage.image.image;

            try
            {
                // Process sprites
                for (int i = 0; i < sprites.Length; i++)
                {
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(sprites[i].Coordinates.Width, sprites[i].Coordinates.Height);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                        g.DrawImage(baseBmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), sprites[i].Coordinates.ToSystemRectangle(), System.Drawing.GraphicsUnit.Pixel);

                    if (bmp == null)
                        continue;

                    // Load to cache
                    imageItem im = new imageItem(new OImage(bmp), String.Format("{0}:{1}", fullImageName, sprites[i].Name));
                    imageCache.Add(im.name, im);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads an image from a file located in the skin folder
        /// </summary>
        /// <param name="imageName">Name of image to return ("|" is used as path separator)</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName)
        {
            return getSkinImage(imageName, false);
        }

        /// <summary>
        /// Loads an spirite image from a file located in the skin folder
        /// </summary>
        /// <param name="imageName">Filename of base image for sprite ("|" is used as path separator)</param>
        /// <param name="spriteName">Name of sprite to return</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName, string spriteName)
        {
            return getSkinImage(imageName, false, spriteName);
        }

        /// <summary>
        /// Loads an image from a file located in the skin folder
        /// </summary>
        /// <param name="imageName">Filename of image to return ("|" is used as path separator)</param>
        /// <param name="noCache">Force a load from file rather than using the internal cache</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName, bool noCache)
        {
            return getSkinImage(imageName, false, String.Empty);
        }

        /// <summary>
        /// Loads an image sprite from a file located in the skin folder
        /// </summary>
        /// <param name="imageName">Filename of image to return ("|" is used as path separator)</param>
        /// <param name="noCache">Force a load from file rather than using the internal cache</param>
        /// <param name="spriteName">Name of sprite to return</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName, bool noCache, string spriteName)
        {
            string fullImageName = Path.Combine(SkinPath, imageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            if (!String.IsNullOrEmpty(spriteName))
            {
                // Get sprite name
                fullImageName = String.Format("{0}:{1}", fullImageName, spriteName);

                // Check for sprite in cache
                if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
                {
                    return imageCache[fullImageName];
                }
                else
                {
                    // Write log entry
                    DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin sprite: {0}", fullImageName));
                    return imageItem.MISSING;
                }
            }

            if (noCache == false)
            {
                if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
                {
                    return imageCache[fullImageName];
                }
                else
                {
                    try
                    {
                        // Try to load from current skin path
                        imageItem im = getImageFromFile(fullImageName);

                        // try to load from default skin path
                        if (im.image == null)
                            im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar)));

                        if (im.image == null)
                        {
                            // Write log entry
                            DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin image: {0}", fullImageName));
                            return imageItem.MISSING;
                        }
                        else
                            return im;
                    }
                    catch (System.ArgumentException)
                    {
                        return imageItem.MISSING;
                    }
                }
            }
            else
            {
                try
                {
                    imageItem item = new imageItem(OImage.FromFile(Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png")));
                    item.name = imageName;
                    return item;
                }
                catch (ArgumentException)
                {
                    try
                    {
                        imageItem item = new imageItem(OImage.FromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png")));
                        item.name = imageName;
                        return item;
                    }
                    catch (ArgumentException)
                    {
                        return new imageItem();
                    }
                }
            }
        }

        /// <summary>
        /// Loads an ninePatch image from a file located in the skin folder
        /// </summary>
        /// <param name="ninePatchImageName"></param>
        /// <param name="ninePatchImageSize"></param>
        /// <returns></returns>
        public imageItem getSkinImage(string ninePatchImageName, Size ninePatchImageSize)
        {
            string fullImageName = Path.Combine(SkinPath, ninePatchImageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            imageItem im = imageItem.NONE;

            // Check for size already present
            string sizedImageName = String.Format("{0}:9Patch:{1}", fullImageName, ninePatchImageSize);
            if (imageCache.ContainsKey(sizedImageName))
                return imageCache[sizedImageName];

            string baseImageName = String.Format("{0}:9Patch:base", fullImageName);
            if (imageCache.ContainsKey(baseImageName)) //(im.image != null)
            {
                im = imageCache[baseImageName];
            }
            else
            {
                try
                {
                    // Try to load from current skin path
                    im = getImageFromFile(fullImageName, true);

                    // try to load from default skin path
                    if (im.image == null)
                        im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), ninePatchImageName.Replace('|', System.IO.Path.DirectorySeparatorChar)));

                    if (im.image == null)
                    {
                        // Write log entry
                        DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin image: {0}", fullImageName));
                        return imageItem.MISSING;
                    }

                    // Save base image to cache
                    im.name = String.Format("{0}:9Patch:base", fullImageName);
                    imageCache.Add(im.name, im);
                }
                catch (System.ArgumentException)
                {
                    return imageItem.MISSING;
                }
            }

            // if requested size is the same as what we already have, return original image
            if (im.image.Width == ninePatchImageSize.Width && im.image.Height == ninePatchImageSize.Height)
                return im;

            // Create new nine patch image
            Rectangle contentArea;
            OImage img = new OImage(NinePatch.GetImageSizeOf(im.image.image, ninePatchImageSize.Width, ninePatchImageSize.Height, out contentArea));

            // Save new image size to cache
            imageItem sizedIm = new imageItem(img, String.Format("{0}:9Patch:{1}", fullImageName, ninePatchImageSize));
            imageCache.Add(sizedIm.name, sizedIm);

            return sizedIm;
        }


        /// <summary>
        /// Loads an image from a file path
        /// </summary>
        /// <param name="fullImageName"></param>
        /// <param name="noCache"></param>
        /// <returns></returns>
        public imageItem getImageFromFile(string fullImageName, bool noCache = false)
        {
            if (imageCache.ContainsKey(fullImageName))
            {
                return imageCache[fullImageName];
            }
            else
            {
                try
                {
                    // try to load a png (preferd)
                    string file = fullImageName + ".png";

                    // Fallback: try to load path directly
                    if (!File.Exists(file))
                        file = fullImageName;

                    // Fallback: try to load a jpg
                    if (!File.Exists(file))
                        file = fullImageName + ".jpg";

                    // Fallback: try to load a gif
                    if (!File.Exists(file))
                        file = fullImageName + ".gif";

                    // Load actual imagefile
                    if (File.Exists(file))
                    {
                        imageItem im = new imageItem();
                        im.image = OImage.FromFile(file);
                        if (im.image != null)
                        {
                            im.name = fullImageName;

                            if (!noCache)
                                imageCache.Add(im.name, im);

                            return im;
                        }
                    }
                }
                catch (System.ArgumentException)
                {
                    return imageItem.MISSING;
                }
            }
            return imageItem.MISSING;
        }        

        /// <summary>
        /// Loads an image from a file located in the plugin folder
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public imageItem getPluginImage(string pluginName, string imageName)
        {
            IBasePlugin plugin = getPluginByName(pluginName);
            if (plugin == null)
                return imageItem.NONE;
            return getPluginImage(plugin, imageName, String.Empty);
        }

        /// <summary>
        /// Loads an image sprite from a file located in the plugin folder
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="imageName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public imageItem getPluginImage(string pluginName, string imageName, string spriteName)
        {
            IBasePlugin plugin = getPluginByName(pluginName);
            if (plugin == null)
                return imageItem.NONE;
            return getPluginImage(plugin, imageName, spriteName);
        }

        /// <summary>
        /// Loads an image from a file located in the plugin folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public imageItem getPluginImage(IBasePlugin plugin, string imageName)
        {
            return getPluginImage(plugin, imageName, String.Empty);
        }

        /// <summary>
        /// Loads a sprite from a sprite image from a file located in the plugin folder (This method can be used in the constructor)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="imageName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public imageItem getPluginImage<T>(string imageName, string spriteName)
        {
            return getPluginImage(typeof(T), imageName, spriteName);
        }
        /// <summary>
        /// Loads an image from a file located in the plugin folder (This method can be used in the constructor)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public imageItem getPluginImage<T>(string imageName)
        {
            return getPluginImage(typeof(T), imageName, String.Empty);
        }
        /// <summary>
        /// Loads a sprite from a sprite image from a file located in the plugin folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="imageName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public imageItem getPluginImage(IBasePlugin plugin, string imageName, string spriteName)
        {
            return getPluginImage(plugin.GetType(), imageName, spriteName);
        }
        /// <summary>
        /// Loads a sprite from a sprite image from a file located in the plugin folder
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="imageName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public imageItem getPluginImage(Type pluginType, string imageName, string spriteName)
        {
            // Get paths
            string PluginPath = System.Reflection.Assembly.GetAssembly(pluginType).Location;
            PluginPath = System.IO.Path.GetDirectoryName(PluginPath);
            string fullImageName = Path.Combine(PluginPath, imageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            if (!String.IsNullOrEmpty(spriteName))
            {
                // Get sprite name
                fullImageName = String.Format("{0}:{1}", fullImageName, spriteName);

                // Check for sprite in cache
                if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
                {
                    return imageCache[fullImageName];
                }
                else
                {
                    // Write log entry
                    DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin sprite: {0}", fullImageName));
                    return imageItem.MISSING;
                }
            }

            try
            {
                if (imageCache.ContainsKey(fullImageName)) //(im.image != null)
                {
                    return imageCache[fullImageName];
                }
                else
                {
                    // Try to load from current skin path
                    imageItem im = getImageFromFile(fullImageName);

                    // try to load from default skin path
                    if (im.image == null)
                        im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar)));

                    if (im.image == null)
                    {
                        // Write log entry
                        DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing plugin image: {0}", fullImageName));
                        return imageItem.MISSING;
                    }
                    else
                        return im;
                }
            }
            catch (System.ArgumentException) { }
            return imageItem.MISSING;
        }

        /// <summary>
        /// Loads an ninePatch image from a file located in the skin folder
        /// </summary>
        /// <param name="ninePatchImageName"></param>
        /// <param name="ninePatchImageSize"></param>
        /// <returns></returns>
        public imageItem getPluginImage(IBasePlugin plugin, string ninePatchImageName, Size ninePatchImageSize)
        {
            // Get paths
            string PluginPath = System.Reflection.Assembly.GetAssembly(plugin.GetType()).Location;
            PluginPath = System.IO.Path.GetDirectoryName(PluginPath);
            string fullImageName = Path.Combine(PluginPath, ninePatchImageName).Replace('|', System.IO.Path.DirectorySeparatorChar);

            imageItem im = imageItem.NONE;

            // Check for size already present
            string sizedImageName = String.Format("{0}:9Patch:{1}", fullImageName, ninePatchImageSize);
            if (imageCache.ContainsKey(sizedImageName))
                return imageCache[sizedImageName];

            string baseImageName = String.Format("{0}:9Patch:base", fullImageName);
            if (imageCache.ContainsKey(baseImageName)) //(im.image != null)
            {
                im = imageCache[baseImageName];
            }
            else
            {
                try
                {
                    // Try to load from current skin path
                    im = getImageFromFile(fullImageName, true);

                    // try to load from default skin path
                    if (im.image == null)
                        im = getImageFromFile(Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), ninePatchImageName.Replace('|', System.IO.Path.DirectorySeparatorChar)));

                    if (im.image == null)
                    {
                        // Write log entry
                        DebugMsg(DebugMessageType.Warning, "PluginHost", String.Format("Unable to load missing skin image: {0}", fullImageName));
                        return imageItem.MISSING;
                    }

                    // Save base image to cache
                    im.name = String.Format("{0}:9Patch:base", fullImageName);
                    imageCache.Add(im.name, im);
                }
                catch (System.ArgumentException)
                {
                    return imageItem.MISSING;
                }
            }

            // if requested size is the same as what we already have, return original image
            if (im.image.Width == ninePatchImageSize.Width && im.image.Height == ninePatchImageSize.Height)
                return im;

            // Create new nine patch image
            Rectangle contentArea;
            OImage img = new OImage(NinePatch.GetImageSizeOf(im.image.image, ninePatchImageSize.Width, ninePatchImageSize.Height, out contentArea));

            // Save new image size to cache
            imageItem sizedIm = new imageItem(img, String.Format("{0}:9Patch:{1}", fullImageName, ninePatchImageSize));
            imageCache.Add(sizedIm.name, sizedIm);

            return sizedIm;
        }


        #endregion

        #region Get data

        /// <summary>
        /// Returns the data requested in dataType
        /// </summary>
        /// <param name="dataType">Data to get</param>
        /// <param name="name">Argument 1</param>
        /// <returns></returns>
        public object getData(eGetData dataType, string name)
        {
            object o;
            getData(dataType, name, out o);
            return o;
        }

        /// <summary>
        /// Returns the data requested in dataType
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public void getData(eGetData dataType, string name, out object data)
        {
            IBasePlugin plugin;
            data = null;

            try
            {
                switch (dataType)
                {
                    // Get the map control
                    case eGetData.GetMap:
                        plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                        if (plugin == null)
                            return;
                        if (plugin is INavigation)
                            data = ((INavigation)plugin).getMap;
                        return;

                    // Get a reference to the current media database
                    case eGetData.GetMediaDatabase:
                        if (String.IsNullOrEmpty(name))
                            name = BuiltInComponents.SystemSettings.DefaultDB_Music;

                        plugin = Core.pluginCollection.Find(t => t.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IMediaDatabase)
                            data = ((IMediaDatabase)plugin).getNew();
                        break;

                    // Get a list of all plugins (or a single one)
                    case eGetData.GetPlugins:
                        if (string.IsNullOrEmpty(name))
                        {
                            data = Core.pluginCollection;
                            return;
                        }
                        else
                        {
                            data = Core.pluginCollection.Find(p => p.pluginName == name);
                            return;
                        }

                    // Get a list of disabled plugins (or a single one)
                    case eGetData.GetPluginsDisabled:
                        {
                            data = Core.pluginCollectionDisabled;
                            return;
                        }

                    // Get a list of all zones (or a single one)
                    case eGetData.GetZones:
                        if (string.IsNullOrEmpty(name))
                        {
                            data = ZoneHandler;
                            return;
                        }
                        else
                        {
                            data = ZoneHandler[name];
                            return;
                        }

                    // Get the system volume of the default unit
                    case eGetData.GetSystemVolume:
                        getData(eGetData.GetSystemVolume, name, "0", out data);
                        return;

                    // Get firmware info from a hardware device
                    case eGetData.GetFirmwareInfo:
                        plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IRawHardware)
                            data = ((IRawHardware)plugin).firmwareVersion;
                        return;

                    // Get hardware device info
                    case eGetData.GetDeviceInfo:
                        plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IRawHardware)
                            data = ((IRawHardware)plugin).deviceInfo;
                        return;

                    // Get available networks
                    case eGetData.GetAvailableNetworks:
                        List<connectionInfo> cInf = new List<connectionInfo>();
                        foreach (IBasePlugin g in Core.pluginCollection.FindAll(p => typeof(INetwork).IsInstanceOfType(p)))
                        {
                            foreach (connectionInfo c in ((INetwork)g).getAvailableNetworks())
                                cInf.Add(c);
                        }
                        data = cInf;
                        return;

                    // Get audio devices
                    case eGetData.GetAudioDevices:
                        if (AudioDevices == null)
                            refreshAudioDevices();
                        data = AudioDevices;
                        return;

                    // Get a list of available skins
                    case eGetData.GetAvailableSkins:
                        string[] ret = Directory.GetDirectories(Path.Combine(Application.StartupPath, "Skins"));
                        for (int i = 0; i < ret.Length; i++)
                            ret[i] = ret[i].Replace(Path.Combine(Application.StartupPath, "Skins", String.Empty), String.Empty);
                        data = ret;
                        return;
                }
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg(dataType.ToString() + " reported an error:", e);
            }
        }

        /// <summary>
        /// Returns the data requested in dataType
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public object getData(eGetData dataType, string name, string param)
        {
            object o;
            getData(dataType, name, param, out o);
            return o;
        }

        /// <summary>
        /// Returns the data requested in dataType
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        public void getData(eGetData dataType, string name, string param, out object data)
        {
            data = null;
            int ret;
            IBasePlugin plugin;
            try
            {
                switch (dataType)
                {
                    // Get current media position (AV Player and tuned content)
                    case eGetData.GetMediaPosition:
                        data = 0;
                        if (int.TryParse(param, out ret) == true)
                        {
                            
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return;
                            if (currentMediaPlayer == null)
                                return;
                            if (currentMediaPlayer[zone.AudioDevice.Instance] == null)
                                if (currentTunedContent[zone.AudioDevice.Instance] == null)
                                    return;
                                else
                                    data = currentTunedContent[zone.AudioDevice.Instance].playbackPosition;
                            else
                                data = currentMediaPlayer[zone.AudioDevice.Instance].getCurrentPosition(zone);
                        }
                        return;

                    // Get volume from a specific player (or tuned content)
                    case eGetData.GetPlayerVolume:
                        data = 0;
                        if (int.TryParse(param, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (currentMediaPlayer == null) return;
                            if (zone == null) return;
                            if (currentMediaPlayer[zone.AudioDevice.Instance] == null)
                                if (currentTunedContent[zone.AudioDevice.Instance] == null)
                                    return;
                                else
                                    if (typeof(IPlayerVolumeControl).IsInstanceOfType(currentTunedContent[zone.AudioDevice.Instance]))
                                        data = ((IPlayerVolumeControl)currentTunedContent[zone.AudioDevice.Instance]).getVolume(zone);
                            else
                                if (typeof(IPlayerVolumeControl).IsInstanceOfType(currentMediaPlayer[zone.AudioDevice.Instance]))
                                    data = ((IPlayerVolumeControl)currentMediaPlayer[zone.AudioDevice.Instance]).getVolume(zone);
                        }
                        return;

                    // Get the system volume for a specific instance
                    case eGetData.GetSystemVolume:
                        {
                            return;
                        }

                    // Get the brightness of a screen
                    case eGetData.GetScreenBrightness:
                        return;

                    // Get current scale factors for a screen
                    case eGetData.GetScaleFactors:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if ((ret >= 0) && (ret < ScreenCount))
                                data = Core.RenderingWindows[ret].ScaleFactors;
                        }
                        return;

                    // Get current playback speed for a specific instance
                    case eGetData.GetPlaybackSpeed:
                        data = 0;
                        if (int.TryParse(param, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (currentMediaPlayer == null) return;
                            if (zone == null) return;
                            if (currentMediaPlayer[zone.AudioDevice.Instance] == null)
                                return;
                            else
                                data = currentMediaPlayer[zone.AudioDevice.Instance].getPlaybackSpeed(zone);
                        }
                        return;

                    // Get the media status for a specific instance
                    case eGetData.GetMediaStatus:
                        if (int.TryParse(param, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return;
                            if (currentMediaPlayer == null) return;
                            if (string.IsNullOrEmpty(name))
                            {
                                if (currentMediaPlayer[zone.AudioDevice.Instance] == null)
                                    if (currentTunedContent[zone.AudioDevice.Instance] == null)
                                        return;
                                    else
                                        data = currentTunedContent[zone.AudioDevice.Instance].getStationInfo(zone);
                                else
                                    data = currentMediaPlayer[zone.AudioDevice.Instance].getPlayerStatus(zone);
                            }
                            else
                            {
                                plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                                if (plugin == null)
                                    return;
                                else if (typeof(IAVPlayer).IsInstanceOfType(plugin))
                                    data = ((IAVPlayer)plugin).getPlayerStatus(zone);
                                else if (typeof(ITunedContent).IsInstanceOfType(plugin))
                                    data = ((ITunedContent)plugin).getStationInfo(zone);
                            }
                        }
                        return;

                    // Get info about currently tuned content
                    case eGetData.GetTunedContentInfo:
                        {
                            int q;
                            if (int.TryParse(param, out q) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(q);
                                if (zone == null) return;
                                if (currentTunedContent[zone.AudioDevice.Instance] != null)
                                    data = currentTunedContent[zone.AudioDevice.Instance].getStatus(zone);
                            }
                        }
                        return;

                    // Get station list from currently tuned content
                    case eGetData.GetStationList:
                        {
                            int q;
                            if (int.TryParse(param, out q) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(q);
                                if (zone == null) return;
                                if (currentTunedContent[zone.AudioDevice.Instance] != null)
                                    data = currentTunedContent[zone.AudioDevice.Instance].getStationList(zone);
                            }
                        }
                        return;

                    // Get a list of available navigation panels
                    case eGetData.GetAvailableNavPanels:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).availablePanels;
                        return;

                    // Get the current GPS position
                    case eGetData.GetCurrentPosition:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).Position;
                        return;

                    // Get the nearest address to the current location
                    case eGetData.GetNearestAddress:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).Location;
                        return;

                    // Get the current active navigation destination 
                    case eGetData.GetDestination:
                        {
                            plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                            if (plugin == null)
                                return;
                            data = ((INavigation)plugin).Destination;
                        }
                        return;

                    // Get the supported bands for a tuned content
                    case eGetData.GetSupportedBands:
                        {
                            if (int.TryParse(param, out ret) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(ret);
                                if (zone == null) return;
                                if (currentTunedContent[zone.AudioDevice.Instance] != null)
                                    data = currentTunedContent[zone.AudioDevice.Instance].getSupportedBands(zone);
                            }
                        }
                        return;
                }
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg(dataType.ToString() + " reported an error:", e);
            }

        }

        #endregion

        #region DebugMsg

        public bool DebugMsg(string from, string message)
        {
            return sendMessage("OMDebug", from, message);
        }
        public bool DebugMsg(DebugMessageType messageType, string from, string message)
        {
            return DebugMsg(from, messageType.ToString().Substring(0, 1) + "|" + message);
        }
        public bool DebugMsg(string from, string header, string[] messages)
        {
            return sendMessage<string[]>("OMDebug", from, header, ref messages);
        }
        public bool DebugMsg(string message)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(mb.DeclaringType.FullName + "." + mb.Name, message);
        }
        public bool DebugMsg(DebugMessageType messageType, string message)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(mb.DeclaringType.FullName + "." + mb.Name, messageType.ToString().Substring(0,1) + "|" + message);
        }
        public bool DebugMsg(string header, string[] messages)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(mb.DeclaringType.FullName + "." + mb.Name, header, messages);
        }
        public bool DebugMsg(DebugMessageType messageType, string header, string[] messages)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(mb.DeclaringType.FullName + "." + mb.Name, messageType.ToString().Substring(0, 1) + "|" + header, messages);
        }
        public bool DebugMsg(DebugMessageType messageType, string from, string header, string[] messages)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(from, messageType.ToString().Substring(0, 1) + "|" + header, messages);
        }
        public bool DebugMsg(string header, Exception e)
        {
            MethodBase mb = new System.Diagnostics.StackFrame(1).GetMethod();
            return DebugMsg(mb.DeclaringType.FullName + "." + mb.Name, DebugMessageType.Error.ToString().Substring(0, 1) + "|" + header + "\r\n" + spewException(e));
        }
        private static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "\r\n";
            err += ("Exception Message: " + e.Message);
            err += ("\r\nSource: " + e.Source);
            err += ("\r\nStack Trace: \r\n" + e.StackTrace);
            err += ("\r\n");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("\r\nSource: " + e.Source);
                err += ("\r\nStack Trace: \r\n" + e.StackTrace);
                err += ("\r\n");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }

        #endregion

        #region ScreenShowIdentity

        public void ScreenShowIdentity()
        {
            OpenMobile.Threading.SafeThread.Asynchronous(() => 
            {
                for (int i = 0; i < _ScreenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity();
            }); 
        }
        public void ScreenShowIdentity(int MS)
        {
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
                for (int i = 0; i < _ScreenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity(true);
                Thread.Sleep(MS);
                for (int i = 0; i < _ScreenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity(false);
            });
        }
        public void ScreenShowIdentity(bool Show)
        {
            for (int i = 0; i < _ScreenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(Show);
        }
        public void ScreenShowIdentity(bool Show, float Opacity)
        {
            for (int i = 0; i < _ScreenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(Show, Opacity);
        }
        public void ScreenShowIdentity(int Screen, bool Show)
        {
            Core.RenderingWindows[Screen].PaintIdentity(Show);
        }
        public void ScreenShowIdentity(int Screen, bool Show, float Opacity)
        {
            Core.RenderingWindows[Screen].PaintIdentity(Show, Opacity);
        }

        #endregion
        
        #region DataSources

        private DataSource DataSource_NetWork_Internet_Online;
        private DataSource DataSource_NetWork_IP;
        private DataSource DataSource_NetWork_Available;
        private DataSource DataSource_Location_Current;
        private DataSource DataSource_Location_Current_Sunset;
        private DataSource DataSource_Location_Current_Sunrise;
        private DataSource DataSource_Location_Current_Daytime;

        private void RegisterDataSources_Early()
        {
            // Internet connection available
            DataSource_NetWork_Internet_Online = new DataSource("OM", "Network", "Internet", "Online", DataSource.DataTypes.binary, "The state of the internet connection as bool");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_NetWork_Internet_Online, _InternetAccess);

            // Current ipaddress
            DataSource_NetWork_IP = new DataSource("OM", "Network", "IP", "", DataSource.DataTypes.raw, "Current IP address of the main adapter as string");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_NetWork_IP, _IPAddress);

            // Network available
            DataSource_NetWork_Available = new DataSource("OM", "Network", "Available", "", DataSource.DataTypes.binary, "The availability of a network connection as bool");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_NetWork_Available, _NetWorkAvailable);

            // Current location
            DataSource_Location_Current = new DataSource("OM", "Location", "Current", "", DataSource.DataTypes.raw, "Current location as class Location");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_Location_Current, CurrentLocation);

            // Sunset
            DataSource_Location_Current_Sunset = new DataSource("OM", "Location", "Current", "Sunset", DataSource.DataTypes.raw, "Sunset for current location as DateTime");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_Location_Current_Sunset, CurrentLocation_Sunset);

            // Sunrise
            DataSource_Location_Current_Sunrise = new DataSource("OM", "Location", "Current", "Sunrise", DataSource.DataTypes.raw, "Sunrise for current location as DateTime");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_Location_Current_Sunrise, CurrentLocation_Sunrise);

            // Daytime
            DataSource_Location_Current_Daytime = new DataSource("OM", "Location", "Current", "Daytime", DataSource.DataTypes.binary, "Is current location considered daytime as bool");
            BuiltInComponents.Host.DataHandler.AddDataSource(DataSource_Location_Current_Daytime, CurrentLocation_Daytime);
        }

        private void RegisterDataSources_Late()
        {

        }

        private void RegisterCommands_Late()
        {
            // Create commands for all available screens
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                // Go back
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", Command.GetScreenName(i), "Panel", "Goto.Previous", CommandExecutor, 0, false, "Goes to the previous panel"));

                // Go home
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", Command.GetScreenName(i), "Panel", "Goto.Home", CommandExecutor, 0, false, "Goes back to the main menu panel"));
            }

            // Toggle day/night
            BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", "System", "Daytime", "Toggle", CommandExecutor, 0, false, "Toggles between day and night mode"));
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = true;
            
            // Go back
            if (command.NameLevel2 == "Panel" && command.NameLevel3 == "Goto.Previous")
                execute(eFunction.goBack, Command.GetScreenNumber(command.NameLevel1));

            // Go back
            else if (command.NameLevel2 == "Panel" && command.NameLevel3 == "Goto.Home")
                execute(eFunction.goHome, Command.GetScreenNumber(command.NameLevel1));

            // Toggle day/night
            else if (command.NameLevel1 == "System" && command.NameLevel2 == "Daytime" && command.NameLevel3 == "Toggle")
                CurrentLocation_Daytime = !CurrentLocation_Daytime;

            //else
            //{   // Default action
            //    result = false;
            //}
            return null;
        }

        #endregion

        /// <summary>
        /// Code that should be executed after all plugins are loaded
        /// </summary>
        private void PluginLoadingCompleted_Exec()
        {
            // Bring windows to top
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                if (Core.RenderingWindows[i].WindowState == OpenTK.WindowState.Fullscreen)
                {
                    //Core.RenderingWindows[i].SetFocus();
                    var result = OpenMobile.Framework.OSSpecific.MoveWindowToTop(Core.RenderingWindows[i].WindowInfo.Handle);
                }

            // Start mediaprovider handler
            _MediaProviderHandler = new MediaProviderHandler();

            //// Preload media players
            //for (int i = 0; i < ZoneHandler.Zones.Count; i++)
            //    execute(eFunction.loadAVPlayer, ZoneHandler.Zones[i]);
            
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                // Execute actions (if present)
                string action = BuiltInComponents.SystemSettings.StartupAction(i);
                if (!string.IsNullOrEmpty(action))
                    CommandHandler.ExecuteCommand(action);
            }

        }

        /// <summary>
        /// Code that should be executed when a internet connection is available
        /// </summary>
        private void InternetAvailable_Exec()
        {
            // Check if we have enough location data available in the db
        }

        /// <summary>
        /// Gets data for the renderingwindow
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public RenderingWindowData GetRenderingWindowData(int screen)
        {
            return new RenderingWindowData(Core.RenderingWindows[screen].Location.ToOpenMobilePoint(), Core.RenderingWindows[screen].Size.ToOpenMobileSize(), Core.RenderingWindows[screen].ScaleFactors);
        }

        public iRenderingWindow RenderingWindowInterface(int screen)
        {
            if (screen < 0 || screen >= Core.RenderingWindows.Count)
                return null;
            return Core.RenderingWindows[screen] as iRenderingWindow;
        }


    }
}
