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

namespace OpenMobile
{
    public sealed class PluginHost : IPluginHost
    {
        #region Private Vars

        /// <summary>
        /// Internal imageChace storage
        /// </summary>
        private List<imageItem> imageCache = new List<imageItem>();

        private System.Threading.Timer tmrCurrentClock;
        private DateTime CurrentClock = DateTime.MinValue;

        /// <summary>
        /// The current GPS position and/or city/state/zip
        /// </summary>
        private Location _location = new Location();

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
        private static int screenCount = DisplayDevice.AvailableDisplays.Count;

        /// <summary>
        /// The amount of available audio devices (instances)
        /// </summary>
        private int _AudioDeviceCount = -1;

        /// <summary>
        /// Current skinpath
        /// </summary>
        private string skinpath;

        /// <summary>
        /// Current datapath
        /// </summary>
        private string datapath;

        /// <summary>
        /// Current pluginpath
        /// </summary>
        private string pluginpath;

        /// <summary>
        /// Connection state to internet
        /// </summary>
        private bool internetAccess;

        /// <summary>
        /// Current IP address
        /// </summary>
        private string ipAddress;

        /// <summary>
        /// Hardware abstraction layer
        /// </summary>
        private HalInterface hal;

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
        private historyCollection history = new historyCollection(screenCount);

        /// <summary>
        /// Controller for zones
        /// </summary>
        private ZoneHandler _ZoneHandler = null;
        public ZoneHandler ZoneHandler 
        {
            get
            {
                return _ZoneHandler;
            }
        }
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

            // Create local data folder for OM (under users local data)
            if (Directory.Exists(DataPath) == false)
                Directory.CreateDirectory(DataPath);

            // Connect security request events
            Credentials.OnAuthorizationRequested += new Credentials.Authorization(Credentials_OnAuthorizationRequested);
            Credentials.Open();

            // Start a timer to get the current time (fired each minute)
            tmrCurrentClock = new System.Threading.Timer(tmrCurrentClock_time, null, 60000, 60000);

            // Save reference to the pluginhost for the framework (Do not remove this line!)
            BuiltInComponents.Host = this;

            // Connect network events
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);

            // Connect screen events
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

            // Initialize zonehandler
            _ZoneHandler = new ZoneHandler();
        }

        /// <summary>
        /// Initializes and starts all pluginhost features (including HAL)
        /// </summary>
        public void Start()
        {
            // Start Hal;
            Hal_Start();

            // Start zone handler
            ZoneHandler.Start();
        }
        /// <summary>
        /// Shutsdown pluginhost
        /// </summary>
        public void Stop()
        {
            // Stop Hal;
            Hal_Stop();

            // Stop zone handler
            ZoneHandler.Stop();
        }

        /// <summary>
        /// Load data into pluginHost (playlists)
        /// </summary>
        public void LateInit()
        {            
            // Raise screen orientation event in case it differs from what we expected
            for (int i = 0; i < ScreenCount; i++)
                if (!DisplayDevice.AvailableDisplays[i].Landscape)
                    raiseSystemEvent(eFunction.screenOrientationChanged, i.ToString(), "Portrait", string.Empty);

            // Initialize device arrays
            currentPlaylist = new List<mediaInfo>[AudioDeviceCount];
            for (int i = 0; i < currentPlaylist.Length; i++)
                currentPlaylist[i] = new List<mediaInfo>();

            currentMediaPlayer = new IAVPlayer[AudioDeviceCount];
            currentTunedContent = new ITunedContent[AudioDeviceCount];
            currentPlaylistIndex = new int[AudioDeviceCount];
            nextPlaylistIndex = new int[AudioDeviceCount];

            videoPlaybackPosition = new Rectangle[AudioDeviceCount];

            // Start loading playlist data
            SandboxedThread.Asynchronous(loadPlaylists);
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
                if ((random != null) && (random[zone.AudioDeviceInstance] == true) && (currentPlaylist[zone.AudioDeviceInstance].Count > 1) && (!BlockRandom))
                {
                    int result;
                    do
                        result = OpenMobile.Framework.Math.Calculation.RandomNumber(0, currentPlaylist[zone.AudioDeviceInstance].Count);
                    while (nextPlaylistIndex[zone.AudioDeviceInstance] == result);
                    nextPlaylistIndex[zone.AudioDeviceInstance] = result;
                }
                else
                    nextPlaylistIndex[zone.AudioDeviceInstance] = currentPlaylistIndex[zone.AudioDeviceInstance] + 1;
                if (nextPlaylistIndex[zone.AudioDeviceInstance] == currentPlaylist[zone.AudioDeviceInstance].Count)
                    nextPlaylistIndex[zone.AudioDeviceInstance] = 0;
                if ((currentPlaylist[zone.AudioDeviceInstance].Count > nextPlaylistIndex[zone.AudioDeviceInstance]) && (getPlayingMedia(zone).Location == currentPlaylist[zone.AudioDeviceInstance][nextPlaylistIndex[zone.AudioDeviceInstance]].Location))
                {
                    nextPlaylistIndex[zone.AudioDeviceInstance]++;
                    if (nextPlaylistIndex[zone.AudioDeviceInstance] == currentPlaylist[zone.AudioDeviceInstance].Count)
                        nextPlaylistIndex[zone.AudioDeviceInstance] = 0;
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
            return currentPlaylist[zone.AudioDeviceInstance];
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
            currentPlaylistIndex[zone.AudioDeviceInstance] = -1;
            currentPlaylist[zone.AudioDeviceInstance].Clear();
            currentPlaylist[zone.AudioDeviceInstance].AddRange(source.GetRange(0, source.Count));
            if (currentPlaylist[zone.AudioDeviceInstance].Count > 0)
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
            if (currentPlaylist[Zone.AudioDeviceInstance].Count == 0)
                return setPlaylist(source, Zone);
            bool single = (currentPlaylist[Zone.AudioDeviceInstance].Count == 1);
            currentPlaylist[Zone.AudioDeviceInstance].AddRange(source.GetRange(0, source.Count));
            if (single)
                generateNext(Zone, false);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, Zone, string.Empty); });
            return true;
        }

        /// <summary>
        /// Saves all current playlists to the media database
        /// </summary>
        private void savePlaylists()
        {
            using (PluginSettings s = new PluginSettings())
            {
                // TODO: ReEnable PlayList load and save
                for (int i = 0; i < AudioDeviceCount; i++)
                {
                    PlaylistHandler.writePlaylistToDB("Current" + i.ToString(), getPlaylistForAudioDeviceInstance(i));
                    s.setSetting("Media.Instance" + i.ToString() + ".Random", getRandomForAudioDeviceInstance(i).ToString());
                }
            }
        }

        /// <summary>
        /// Loads playlists from media database and activates them
        /// </summary>
        private void loadPlaylists()
        {
            using (PluginSettings s = new PluginSettings())
            {
                bool res;
                string dbname = s.getSetting("Default.MusicDatabase");
                // TODO: ReEnable PlayList load and save
                for (int i = 0; i < AudioDeviceCount; i++)
                {
                    setPlaylistForAudioDeviceInstance(PlaylistHandler.readPlaylistFromDB("Current" + i.ToString(), dbname), i);
                    if (bool.TryParse(s.getSetting("Media.Instance" + i.ToString() + ".Random"), out res))
                        setRandomForAudioDeviceInstance(i, res);
                }
            }
        }

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
        static string[] AudioDevices;


        /// <summary>
        /// Get the audio device instance for a specific screen
        /// <para>The instance number is converted from the current DB setting</para>
        /// </summary>
        /// <param name="screen">Screen number</param>
        /// <returns>Audio device (instance)</returns>
        private static int getAudioDeviceInstance(int screen)
        {
            string str;
            // TODO: Remove screen + 1
            using (PluginSettings settings = new PluginSettings())
                str = settings.getSetting("Screen" + (screen + 1).ToString() + ".SoundCard");
            if (str.Length == 0)
                return 0;
            if (AudioDevices == null)
                if (!refreshAudioDevices())
                    return 0;
            return Array.FindIndex(AudioDevices, p => (p != null) && (p.Replace("  ", " ") == str)); // +1;
        }

        /// <summary>
        /// Get's the audio device name for a instance number
        /// </summary>
        /// <param name="instance">Audio device (instance)</param>
        /// <returns>Audio device name</returns>
        public string getAudioDeviceName(int instance)
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
        public string GetAudioDeviceDefaultName()
        {
            string device = getAudioDeviceName(0);
            if (String.IsNullOrEmpty(device))
                return "Default Device";
            else
                return device;
        }

        /// <summary>
        /// Get's the audio device instance for a device name
        /// </summary>
        /// <param name="name">Audio device name</param>
        /// <returns>Audio device instance</returns>
        public int getAudioDeviceInstance(string name)
        {
            if (AudioDevices == null)
                if (!refreshAudioDevices())
                    return -1;

            if (string.IsNullOrEmpty(name))
                return -1;

            return Array.FindIndex(AudioDevices, p => (p != null) && p == name);//(p.Replace("  ", " ") == name)); // +1;
        }

        /// <summary>
        /// Updates the devices array with Audio device names 
        /// <para>Devices are read from the currently loaded player</para>
        /// </summary>
        /// <returns></returns>
        private static bool refreshAudioDevices()
        {
            string[] devs = new string[0];
            foreach (IBasePlugin player in Core.pluginCollection.FindAll(p => typeof(IAVPlayer).IsInstanceOfType(p) == true))
            {
                try
                {
                    devs = ((IAVPlayer)player).OutputDevices;
                }
                catch (Exception e)
                {
                    //string message = e.GetType().ToString() + "(" + e.Message + ")\r\n\r\n" + e.StackTrace + "\r\n********";
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, e.Source, spewException(e));
                }
                
                // We set it to null if we only detected default unit as this indicates that no units was found
                // A length of 2 would indicate a default unit and the corresponding physcial unit which is minimum for detection
                if (devs.Length >= 2)
                {
                    AudioDevices = devs;
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
                        return -1;
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
                if (skinpath == null)
                {
                    using (PluginSettings s = new PluginSettings())
                    {
                        skinpath = s.getSetting("UI.Skin");
                        if (skinpath.Length == 0)
                        {
                            skinpath = "Default";
                            s.setSetting("UI.Skin", skinpath);
                        }
                    }
                    skinpath = Path.Combine(Application.StartupPath, "Skins", skinpath);
                }
                return skinpath;
            }
            set
            {
                skinpath = Path.Combine(Application.StartupPath, "Skins", value);
            }
        }

        /// <summary>
        /// Data folder path (OM Temp data folder)
        /// </summary>
        public string DataPath
        {
            get
            {
                if (datapath == null)
                    datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile");
                return datapath;
            }
        }

        /// <summary>
        /// Plugin data folder
        /// </summary>
        public string PluginPath
        {
            get
            {
                if (pluginpath == null)
                    pluginpath = Path.Combine(Application.StartupPath, "Plugins");
                return pluginpath;
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
            foreach (IBasePlugin c in Core.pluginCollection)
                if (c.pluginName == name)
                    return c;
            return null;
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
            return Core.RenderingWindows[screen].WindowHandle;
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
                return _location;
            }
            set
            {
                if (value == null)
                    return;
                _location = value;
                if ((_location == null) || (_location.City != value.City) || (_location.Street != value.Street) || (_location.Zip != value.Zip))
                    raiseNavigationEvent(eNavigationEvent.LocationChanged, _location.ToString());
            }
        }

        #endregion

        #region Time and date (System clock)
      
        /// <summary>
        /// Raises time and date related events (One minute resolution)
        /// </summary>
        /// <param name="state"></param>
        private void tmrCurrentClock_time(object state)
        {
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

            // Update current clock values (min. resolution 1 minute)
            CurrentClock = DateTime.Now;
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
            OMBasicShape box = new OMBasicShape(250, 130, 500, 300);
            box.BorderColor = Color.Silver;
            box.BorderSize = 3F;
            box.CornerRadius = 15;
            box.FillColor = Color.FromArgb(0, 0, 15);
            box.Shape = shapes.RoundedRectangle;
            security.addControl(box);
            OMImage img = new OMImage(400, 140, 225, 280);
            img.Image = Core.theHost.getSkinImage("Lock");
            security.addControl(img);
            OMLabel label = new OMLabel(250, 200, 500, 115);
            label.Text = pluginName + " is requesting access to the credentials cache to access your " + requestedAccess + ".";
            label.TextAlignment = Alignment.WordWrapTC;
            label.Font = new Font(Font.Arial, 22F);
            security.addControl(label);
            OMLabel title = new OMLabel(260, 135, 480, 60);
            title.Text = "Authorization Required";
            title.Font = new Font(Font.Verdana, 29F);
            title.Format = eTextFormat.Glow;
            title.OutlineColor = Color.Yellow;
            security.addControl(title);
            OMButton accept = new OMButton(295, 340, 200, 80);
            accept.Image = Core.theHost.getSkinImage("Full");
            accept.FocusImage = Core.theHost.getSkinImage("Full.Highlighted");
            accept.Text = "Allow";
            accept.OnClick += security_OnClick;
            security.addControl(accept);
            OMButton deny = new OMButton(515, 340, 200, 80);
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
                    Core.RenderingWindows[i].ExecuteTransition(eGlobalTransition.None);
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
                    Core.RenderingWindows[i].ExecuteTransition(eGlobalTransition.None);
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
                        ipAddress = "0.0.0.0";
                    else
                        if (ipAddress != i.ToString())
                        {
                            ipAddress = i.ToString();
                            raiseSystemEvent(eFunction.networkConnectionsAvailable, ipAddress, String.Empty, String.Empty);
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
            if (e.IsAvailable != internetAccess)
            {
                internetAccess = e.IsAvailable;
                if (internetAccess == true)
                    if (OpenMobile.Net.Network.checkForInternet() == OpenMobile.Net.Network.connectionStatus.InternetAccess)
                        raiseSystemEvent(eFunction.connectedToInternet, String.Empty, String.Empty, String.Empty);
                    else
                        raiseSystemEvent(eFunction.disconnectedFromInternet, String.Empty, String.Empty, String.Empty);
            }
        }

        #endregion

        #region Screen control and data

        /// <summary>
        /// Raises system events when screens has been added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (screenCount != DisplayDevice.AvailableDisplays.Count)
            {
                if (screenCount < DisplayDevice.AvailableDisplays.Count)
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
                return screenCount;
            }
            set
            {
                screenCount = value;
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
        public void SetWindowState(int Screen, WindowState windowState)
        {
            Core.RenderingWindows[Screen].WindowState = windowState;
        }

        /// <summary>
        /// Sets the windowstate (maximized/minimized) for all screens
        /// </summary>
        /// <param name="windowState"></param>
        public void SetAllWindowState(WindowState windowState)
        {
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].WindowState = windowState;
        }

        /// <summary>
        /// Sets the visibility of the pointer cursors in OM
        /// </summary>
        public bool ShowCursors { get; set; }

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

        //TODO : REWRITE MEDIA CONTROLS TO USE SCREEN AND IN ADDITION SCREEN AND ZONE



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
                        if (!closing)
                        {
                            closing = true;
                            RenderingWindow.CloseRenderer();
                            Hal_Send("44");
                            savePlaylists();
                            SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty); });
                            Stop();
                        }
                    }
                    return true;

                // Restart OM
                case eFunction.restartProgram:
                    try
                    {
                        savePlaylists();
                        raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty);
                        Stop();
                    }
                    catch (Exception) { }
                    try
                    {
                        ProcessStartInfo info = Process.GetCurrentProcess().StartInfo;
                        info.FileName = Process.GetCurrentProcess().Modules[0].FileName;
                        if (Core.RenderingWindows[0].WindowState == WindowState.Fullscreen)
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
                        s = set.getSetting("Default.Speech");
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
                    Hal_Send("46");
                    Stop();
                    return true;

                // Restart computer
                case eFunction.restart:
                    Hal_Send("47");
                    Stop();
                    return true;

                // Hibernate computer
                case eFunction.hibernate:
                    Hal_Send("45");
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    return true;

                // Set computer in standby
                case eFunction.standby:
                    Hal_Send("48");
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
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

                // Forces a refresh of data from data providers
                case eFunction.refreshData:
                    bool result = true;
                    foreach (IBasePlugin p in Core.pluginCollection.FindAll(i => typeof(IDataProvider).IsInstanceOfType(i)))
                        result = result & execute(eFunction.refreshData, p.pluginName);
                    return result;

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
                    return execute(eFunction.ExecuteTransition, arg, "Crossfade");

                // Cancel requested transition
                case eFunction.CancelTransition:
                    if (int.TryParse(arg, out ret))
                    {
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].Rollback();
                        }
                    }
                    return false;

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
                    return execute(eFunction.goBack, arg, "SlideRight");

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
                                if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                    return false;
                                try
                                {
                                    if (currentMediaPlayer[subZone.AudioDeviceInstance].getPlayerStatus(subZone) == ePlayerStatus.Playing)
                                        currentMediaPlayer[subZone.AudioDeviceInstance].stop(subZone);
                                }
                                catch (Exception) { }
                                currentMediaPlayer[subZone.AudioDeviceInstance].OnMediaEvent -= raiseMediaEvent;
                                string name = currentMediaPlayer[subZone.AudioDeviceInstance].pluginName;
                                currentMediaPlayer[subZone.AudioDeviceInstance] = null;
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
                                if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                    return false;
                                currentTunedContent[subZone.AudioDeviceInstance].setPowerState(subZone, false);
                                currentTunedContent[subZone.AudioDeviceInstance].OnMediaEvent -= raiseMediaEvent;
                                string name = currentTunedContent[subZone.AudioDeviceInstance].pluginName;
                                currentTunedContent[subZone.AudioDeviceInstance] = null;
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
                            if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDeviceInstance].play(subZone);
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
                            if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDeviceInstance].SetVideoVisible(subZone, true);
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
                            if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDeviceInstance].SetVideoVisible(subZone, false);
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
                            if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                            {
                                if (currentTunedContent[subZone.AudioDeviceInstance] != null)
                                {
                                    if (typeof(IPausable).IsInstanceOfType(currentTunedContent[subZone.AudioDeviceInstance]))
                                        return ((IPausable)currentTunedContent[subZone.AudioDeviceInstance]).pause(subZone);
                                }
                                return false;
                            }
                            return currentMediaPlayer[subZone.AudioDeviceInstance].pause(subZone);
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
                            if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentMediaPlayer[subZone.AudioDeviceInstance].stop(subZone);
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
                                if (currentPlaylist[subZone.AudioDeviceInstance].Count == 0)
                                    return false;
                                currentPlaylistIndex[subZone.AudioDeviceInstance] = nextPlaylistIndex[subZone.AudioDeviceInstance];
                                bool b = execute(eFunction.Play, arg, currentPlaylist[subZone.AudioDeviceInstance][currentPlaylistIndex[subZone.AudioDeviceInstance]].Location);
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
                                if (currentPlaylist[subZone.AudioDeviceInstance].Count == 0)
                                    return false;
                                lock (currentPlaylistIndex)
                                {
                                    currentPlaylistIndex[subZone.AudioDeviceInstance]--;
                                    if (currentPlaylistIndex[subZone.AudioDeviceInstance] <= -1)
                                        currentPlaylistIndex[subZone.AudioDeviceInstance] = currentPlaylist[subZone.AudioDeviceInstance].Count - 1;
                                }
                                bool b = execute(eFunction.Play, arg, currentPlaylist[subZone.AudioDeviceInstance][currentPlaylistIndex[subZone.AudioDeviceInstance]].Location);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].scanReverse(subZone);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].scanForward(subZone);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].scanBand(subZone);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].stepBackward(subZone);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].stepForward(subZone);
                        });
                    }
                    return false;

                // Refresh dataprovider data
                case eFunction.refreshData:
                    plugin = getPluginByName(arg);
                    if (plugin == null)
                        return false;
                    if (typeof(IDataProvider).IsInstanceOfType(plugin))
                        return ((IDataProvider)plugin).refreshData();
                    else if (typeof(INetwork).IsInstanceOfType(plugin))
                        return ((INetwork)plugin).refresh();
                    else
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
                    Hal_Send("35|" + arg);
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
                        Core.RenderingWindows[ret].WindowState = WindowState.Minimized;
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
                // Transition to the defult panel from a plugin
                case eFunction.TransitionToPanel:
                    return execute(eFunction.TransitionToPanel, arg1, arg2, String.Empty);

                // Transition out the defult panel from a plugin
                case eFunction.TransitionFromPanel:
                    return execute(eFunction.TransitionFromPanel, arg1, arg2, String.Empty);

                // Execute transition with the specified effect
                case eFunction.ExecuteTransition:
                    eGlobalTransition effect;

                    if (_GraphicsLevel == eGraphicsLevel.Minimal)
                        effect = eGlobalTransition.None;
                    else if (string.IsNullOrEmpty(arg2))
                        effect = eGlobalTransition.None;
                    else
                        try
                        {
                            effect = (eGlobalTransition)Enum.Parse(typeof(eGlobalTransition), arg2);
                        }
                        catch (ArgumentException)
                        {
                            effect = eGlobalTransition.None;
                        }
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].ExecuteTransition(effect);
                        }
                        raiseSystemEvent(eFunction.ExecuteTransition, arg1, effect.ToString(), String.Empty);
                        return true;
                    }
                    return false;

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

                // Input from user is ready (like OSK data)
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, String.Empty);
                    return true;

                // Force a refresh of plugin data
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2);

                // Load a AV player with the specified instance from the specified plugin
                case eFunction.loadAVPlayer:
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            Zone zone = ZoneHandler.GetZone(ret);
                            if (zone == null) return false;
                            if (currentMediaPlayer == null) return false;
                            return ZoneHandler.ForEachZoneInZone(zone, delegate(Zone subZone)
                            {
                                if (currentMediaPlayer[subZone.AudioDeviceInstance] != null)
                                    return false;
                                if (currentTunedContent[subZone.AudioDeviceInstance] != null)
                                    execute(eFunction.unloadTunedContent, arg1);
                                IAVPlayer player = (IAVPlayer)Core.pluginCollection.FindAll(i => typeof(IAVPlayer).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                                if (player == null)
                                    return false;

                                //Hook events it if its not already hooked
                                if (Array.Exists<IAVPlayer>(currentMediaPlayer, a => a == player) == false)
                                    player.OnMediaEvent += raiseMediaEvent;

                                currentMediaPlayer[subZone.AudioDeviceInstance] = player;
                                raiseMediaEvent(eFunction.loadAVPlayer, subZone, arg2);
                                return true;
                            });
                        }
                        return false;
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
                                if (currentTunedContent[subZone.AudioDeviceInstance] != null)
                                    return false;
                                if (currentMediaPlayer[subZone.AudioDeviceInstance] != null)
                                    execute(eFunction.unloadAVPlayer, arg1);
                                ITunedContent player = (ITunedContent)Core.pluginCollection.FindAll(i => typeof(ITunedContent).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                                if (player == null)
                                    return false;
                                //Only hook it once
                                if (Array.Exists<ITunedContent>(currentTunedContent, a => a == player) == false)
                                    player.OnMediaEvent += raiseMediaEvent;
                                currentTunedContent[subZone.AudioDeviceInstance] = player;
                                if (!currentTunedContent[subZone.AudioDeviceInstance].setPowerState(subZone, true))
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
                                    if (currentPlaylist[subZone.AudioDeviceInstance].Count > ret2)
                                    {
                                        currentPlaylistIndex[subZone.AudioDeviceInstance] = ret2;
                                        generateNext(subZone, false);
                                        return true;
                                    }
                                    return false;
                                }
                                currentPlaylistIndex[subZone.AudioDeviceInstance] = currentPlaylist[subZone.AudioDeviceInstance].FindIndex(p => p.Location == arg2);
                                if (currentPlaylist[subZone.AudioDeviceInstance].Count > 0)
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
                                if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                    return findAlternatePlayerAndStartPlay(subZone, arg2, StorageAnalyzer.classifySource(arg2));
                                if (currentMediaPlayer[subZone.AudioDeviceInstance].play(subZone, arg2, StorageAnalyzer.classifySource(arg2)) == false)
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
                                    if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                        return false;
                                    float pos;
                                    if (float.TryParse(arg2, out pos))
                                        return currentMediaPlayer[subZone.AudioDeviceInstance].setPosition(subZone, pos);
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
                                    if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                        return false;
                                    float speed;
                                    if (float.TryParse(arg2, out speed))
                                        return currentMediaPlayer[subZone.AudioDeviceInstance].setPlaybackSpeed(subZone, speed);
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
                                if (currentMediaPlayer[subZone.AudioDeviceInstance] == null)
                                {
                                    if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                        return false;
                                    raiseMediaEvent(eFunction.setPlayerVolume, subZone, arg2);
                                    if (int.TryParse(arg2, out ret2))
                                        return currentTunedContent[subZone.AudioDeviceInstance].setVolume(subZone, ret2);
                                }
                                else
                                {
                                    raiseMediaEvent(eFunction.setPlayerVolume, subZone, arg2);
                                    if (int.TryParse(arg2, out ret2))
                                        return currentMediaPlayer[subZone.AudioDeviceInstance].setVolume(subZone, ret2);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return findAlternateTuner(subZone, arg2);
                            if (currentTunedContent[subZone.AudioDeviceInstance].tuneTo(subZone, arg2) == false)
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
                            Hal_Send("34|" + arg1 + "|" + zone.AudioDeviceInstance.ToString());
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
                            Hal_Send("66|" + zone.AudioDeviceInstance.ToString() + "|" + arg2);
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
                            if (currentTunedContent[subZone.AudioDeviceInstance] == null)
                                return false;
                            return currentTunedContent[subZone.AudioDeviceInstance].setBand(subZone, (eTunedContentBand)Enum.Parse(typeof(eTunedContentBand), arg2, false));
                        });
                    }
                    return false;

                // Set monitor brightness on a specified screen
                case eFunction.setMonitorBrightness:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= screenCount))
                            return false;
                        if (int.TryParse(arg2, out ret2) == false)
                            return false;

                        // Turn monitor off?
                        if ((ret2 == 0) && ((Environment.OSVersion.Version.Major < 6) || ret > 0))
                            Core.RenderingWindows[ret].FadeOut();

                        // Send request to HAL
                        Hal_Send("40|" + arg1 + "|" + arg2);
                        return true;
                    }
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
                            Hal_Send("42|" + zone.AudioDeviceInstance.ToString() + "|" + arg2);
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
                // Force refresh of data from a data provider
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2, arg3);

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
                            Core.RenderingWindows[ret].TransitionInPanel(panel);
                            raiseSystemEvent(eFunction.TransitionToPanel, arg1, arg2, arg3);
                            if (!panel.UIPanel)
                                history.Enqueue(ret, arg2, arg3, panel.Forgotten);
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
                            Core.RenderingWindows[ret].TransitionOutPanel(panel);
                        }
                        raiseSystemEvent(eFunction.TransitionFromPanel, arg1, arg2, arg3);
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

        /// <summary>
        /// Executes a function on a specific type of plugins
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public bool executeByType(Type type, eFunction function, string arg1, string arg2)
        {
            bool result = false;
            switch (function)
            {
                case eFunction.refreshData:
                    foreach (IBasePlugin p in Core.pluginCollection.FindAll(i => typeof(IDataProvider).IsInstanceOfType(i)))
                    {
                        result = result & execute(eFunction.refreshData, p.pluginName);
                    }
                    return result;
                default:
                    return false;
            }
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
            if (currentMediaPlayer[zone.AudioDeviceInstance] != null)
                execute(eFunction.unloadAVPlayer, zone.ToString());
            ITunedContent tmp = currentTunedContent[zone.AudioDeviceInstance];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadTunedContent, zone.ToString());
                    execute(eFunction.loadTunedContent, zone.ToString(), plugins[i].pluginName);
                    if ((currentTunedContent[zone.AudioDeviceInstance] != null) && (currentTunedContent[zone.AudioDeviceInstance].tuneTo(zone, arg2) == true))
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
            if (currentTunedContent[zone.AudioDeviceInstance] != null)
                execute(eFunction.unloadTunedContent, zone.ToString());
            IAVPlayer tmp = currentMediaPlayer[zone.AudioDeviceInstance];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(IAVPlayer).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadAVPlayer, zone.ToString());
                    execute(eFunction.loadAVPlayer, zone.ToString(), plugins[i].pluginName);
                    if (currentMediaPlayer[zone.AudioDeviceInstance].play(zone, arg2, type) == true)
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
            if ((zone.AudioDeviceInstance < 0) || (zone.AudioDeviceInstance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];
            return random[zone.AudioDeviceInstance];
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
            if ((zone.AudioDeviceInstance < 0) || (zone.AudioDeviceInstance >= _AudioDeviceCount))
                return false;
            if (random == null)
                random = new bool[_AudioDeviceCount];

            // Only send events and update if the state was actually changed
            if (value != random[zone.AudioDeviceInstance])
            {
                random[zone.AudioDeviceInstance] = value;
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
                Zone zone = ZoneHandler.BaseZones.Find(x => x.AudioDeviceInstance == AudioDeviceInstance);
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
            if (currentMediaPlayer[zone.AudioDeviceInstance] != null)
                return currentMediaPlayer[zone.AudioDeviceInstance].getMediaInfo(zone);
            if (currentTunedContent[zone.AudioDeviceInstance] != null)
                return currentTunedContent[zone.AudioDeviceInstance].getMediaInfo(zone);
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
            if (currentMediaPlayer[zone.AudioDeviceInstance] == null)
                return new mediaInfo();
            return currentPlaylist[zone.AudioDeviceInstance][nextPlaylistIndex[zone.AudioDeviceInstance]];
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
                    for (int i = 0; i < screenCount; i++)
                        Core.RenderingWindows[i].PaintIdentity();
                else if (message == "MakeCurrent")
                    Core.RenderingWindows[0].MakeCurrent();
                else if (message == "KillCurrent")
                    Core.RenderingWindows[0].MakeCurrent(null);
                return true;
            }

            // Message to HAL?
            else if (to == "OMHal")
            {
                Hal_Send("-1|" + from + "|" + message);
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
        public void raiseSystemEvent(eFunction e, string arg1, string arg2, string arg3)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("raiseSystemEvent( eFunction: {0}, arg1: {1}, arg2: {2}, arg3: {3}", e, arg1, arg2, arg3));
            try
            {
                if (OnSystemEvent != null)
                    OnSystemEvent(e, arg1, arg2, arg3);
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
            System.Diagnostics.Debug.WriteLine(String.Format("raiseMediaEvent( eFunction: {0}, instance: {1}, arg: {2}", type, zone, arg));
            if (OnMediaEvent != null)
                SandboxedThread.Asynchronous(delegate() { OnMediaEvent(type, zone, arg); });

            if (type == eFunction.nextMedia)
                while ((!execute(eFunction.nextMedia, zone.ToString())) && (currentPlaylist[zone.AudioDeviceInstance].Count > 1))
                    Thread.Sleep(200);

            #region Internal video playback status

            // Set internal status for video playing
            else if (type == eFunction.showVideoWindow)
            {
                for (int i = 0; i < screenCount; i++)
                    // TODO : Might be a problem!
                    if (ZoneHandler.GetZone(i) == zone)
                        Core.RenderingWindows[i].VideoPlaying = true;
            }

            // Reset internal status for video playing
            else if (type == eFunction.hideVideoWindow)
            {
                for (int i = 0; i < screenCount; i++)
                    // TODO : Might be a problem!
                    if (ZoneHandler.GetZone(i) == zone)
                        Core.RenderingWindows[i].VideoPlaying = false;
            }

            #endregion 
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

         /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="SourceTag"></param>
        /// <param name="Message"></param>
        public void SendStatusData(eDataType DataType, IBasePlugin SourcePlugin,string Message)
        {
            SendStatusData(-1, DataType, SourcePlugin, "", Message);
        }

        /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="SourceTag"></param>
        /// <param name="Message"></param>
        public void SendStatusData(int Screen, eDataType DataType, IBasePlugin SourcePlugin, string Message)
        {
            SendStatusData(Screen, DataType, SourcePlugin, "", Message);
        }
       /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="SourceTag"></param>
        /// <param name="Message"></param>
        public void SendStatusData(eDataType DataType, IBasePlugin SourcePlugin, string SourceTag, string Message)
        {
            SendStatusData(-1, DataType, SourcePlugin, SourceTag, Message);
        }

        /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="SourceTag"></param>
        /// <param name="Message"></param>
        public void SendStatusData(int Screen, eDataType DataType, IBasePlugin SourcePlugin, string SourceTag, string Message)
        {
            string Source = "";
            if (Screen >= 0)
                Source += "{" + Screen + "}";
            if (SourcePlugin != null)
                Source += SourcePlugin.pluginName;
            if (!String.IsNullOrEmpty(Source) & !String.IsNullOrEmpty(SourceTag))
                Source += ".";
            Source += SourceTag;
            raiseSystemEvent(eFunction.backgroundOperationStatus, Message, Source, DataType.ToString());
        }

        #endregion

        #region Skin image handling

        /// <summary>
        /// Returns the given skin image
        /// </summary>
        /// <param name="imageName">Name of image to return ("|" is used as path separator)</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName)
        {
            return getSkinImage(imageName, false);
        }

        /// <summary>
        /// Returns the given skin image
        /// </summary>
        /// <param name="imageName">Name of image to return ("|" is used as path separator)</param>
        /// <param name="noCache">Force a load from file rather than using the internal cache</param>
        /// <returns></returns>
        public imageItem getSkinImage(string imageName, bool noCache)
        {
            if (noCache == false)
            {
                imageItem im = imageCache.Find(i => i.name == imageName);
                if (im.image != null)
                {
                    return im;
                }
                else
                {
                    try
                    {
                        im.image = OImage.FromFile(Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png"));
                        if (im.image != null)
                        {
                            im.name = imageName;
                            imageCache.Add(im);
                            return im;
                        }
                        else
                        {
                            string file = Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".gif");
                            if (!File.Exists(file))
                                file = Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png");
                            if (!File.Exists(file))
                                file = Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar));
                            im.image = OImage.FromFile(file);
                            im.name = imageName;
                            imageCache.Add(im);
                            return im;
                        }
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

                    // Get status of the specified data provider
                    case eGetData.DataProviderStatus:
                        if (!string.IsNullOrEmpty(name))
                        {
                            plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                            if (plugin == null)
                                return;
                            if (plugin is IDataProvider)
                                data = ((IDataProvider)plugin).updaterStatus();
                        }
                        break;

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

                    // Get a list of available sensors
                    case eGetData.GetAvailableSensors:
                        {
                            if (string.IsNullOrEmpty(name))
                            {
                                List<Sensor> Sensors = new List<Sensor>();
                                foreach (ISensorProvider g in Core.pluginCollection.FindAll(p => typeof(ISensorProvider).IsInstanceOfType(p)))
                                {
                                    Sensors.AddRange(g.getAvailableSensors(eSensorType.All));

                                }
                                data = Sensors;
                                return;
                            }
                            else
                            {
                                plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                                if (plugin == null)
                                    return;
                                data = ((IRawHardware)plugin).getAvailableSensors(eSensorType.All);
                            }
                            return;
                        }

                    // Get a list of available keyboards
                    case eGetData.GetAvailableKeyboards:
                        data = InputRouter.Keyboards;
                        return;

                    // Get a list of available mice
                    case eGetData.GetAvailableMice:
                        data = InputRouter.Mice;
                        return;

                    // Get a list of mapped keyboards
                    case eGetData.GetMappedKeyboards:
                        data = InputRouter.KeyboardsMapped;
                        return;

                    // Get a list of unmapped keyboards
                    case eGetData.GetUnMappedKeyboards:
                        data = InputRouter.KeyboardsUnMapped;
                        return;

                    // Get a list of mapped mice
                    case eGetData.GetMappedMice:
                        data = InputRouter.MiceMapped;
                        return;

                    // Get a list of unmapped mice
                    case eGetData.GetUnMappedMice:
                        data = InputRouter.MiceUnMapped;
                        return;

                    // Detect a mouse device
                    case eGetData.GetMouseDetectedUnit:
                        data = InputRouter.DetectMouseDevice();
                        return;

                    // Detect a keyboard device
                    case eGetData.GetKeyboardDetectedUnit:
                        data = InputRouter.DetectKeyboardDevice();
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
                            if (currentMediaPlayer[zone.AudioDeviceInstance] == null)
                                if (currentTunedContent[zone.AudioDeviceInstance] == null)
                                    return;
                                else
                                    data = currentTunedContent[zone.AudioDeviceInstance].playbackPosition;
                            else
                                data = currentMediaPlayer[zone.AudioDeviceInstance].getCurrentPosition(zone);
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
                            if (currentMediaPlayer[zone.AudioDeviceInstance] == null)
                                if (currentTunedContent[zone.AudioDeviceInstance] == null)
                                    return;
                                else
                                    data = currentTunedContent[zone.AudioDeviceInstance].getVolume(zone);
                            else
                                data = currentMediaPlayer[zone.AudioDeviceInstance].getVolume(zone);
                        }
                        return;

                    // Get the system volume for a specific instance
                    case eGetData.GetSystemVolume:
                        {
                            data = 0; // Default return value
                            if (int.TryParse(param, out ret) == true)
                            {
                                Zone zone = ZoneHandler.GetZone(ret);
                                if (zone == null) return;

                                Hal_Send("3|" + zone.AudioDeviceInstance);
                                bool res = true;
                                while (res == true)
                                {
                                    Thread.Sleep(5);
                                    res = (hal.volume == null);
                                    if (res == false)
                                    {
                                        if (hal.volume[0] == zone.AudioDeviceInstance.ToString())
                                        {
                                            if (int.TryParse(hal.volume[1], out ret))
                                                data = ret;
                                            hal.volume = null;
                                        }
                                    }
                                }
                            }
                            return;
                        }

                    // Get the brightness of a screen
                    case eGetData.GetScreenBrightness:
                        Hal_Send("1|" + param);
                        bool resp = true;
                        while (resp == true)
                        {
                            Thread.Sleep(5);
                            resp = (hal.brightness == null);
                            if (resp == false)
                            {
                                if (hal.brightness[0] == param)
                                {
                                    if (int.TryParse(hal.brightness[1], out ret))
                                        data = ret;
                                    hal.brightness = null;
                                }
                            }
                        }
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
                            if (currentMediaPlayer[zone.AudioDeviceInstance] == null)
                                return;
                            else
                                data = currentMediaPlayer[zone.AudioDeviceInstance].getPlaybackSpeed(zone);
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
                                if (currentMediaPlayer[zone.AudioDeviceInstance] == null)
                                    if (currentTunedContent[zone.AudioDeviceInstance] == null)
                                        return;
                                    else
                                        data = currentTunedContent[zone.AudioDeviceInstance].getStationInfo(zone);
                                else
                                    data = currentMediaPlayer[zone.AudioDeviceInstance].getPlayerStatus(zone);
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
                                if (currentTunedContent[zone.AudioDeviceInstance] != null)
                                    data = currentTunedContent[zone.AudioDeviceInstance].getStatus(zone);
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
                                if (currentTunedContent[zone.AudioDeviceInstance] != null)
                                    data = currentTunedContent[zone.AudioDeviceInstance].getStationList(zone);
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
                                if (currentTunedContent[zone.AudioDeviceInstance] != null)
                                    data = currentTunedContent[zone.AudioDeviceInstance].getSupportedBands(zone);
                            }
                        }
                        return;

                    // Gets a list of available mice units for a specific screen
                    case eGetData.GetMiceUnitsForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetMiceDeviceListForScreen(ret);
                        }
                        return;

                    // Gets a list of available keyboard units for a specific screen
                    case eGetData.GetKeyboardUnitsForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetKeyboardsDeviceListForScreen(ret);
                        }
                        return;

                    // Gets the currently mapped keyboard for a specific screen
                    case eGetData.GetKeyboardCurrentUnitForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetKeyboardsCurrentDeviceForScreen(ret);
                        }
                        return;

                    // Gets the currently mapped mice for a specific screen
                    case eGetData.GetMiceCurrentUnitForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetMiceDeviceCurrentForScreen(ret);
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
                for (int i = 0; i < screenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity();
            }); 
        }
        public void ScreenShowIdentity(int MS)
        {
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
            {
                for (int i = 0; i < screenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity(true);
                Thread.Sleep(MS);
                for (int i = 0; i < screenCount; i++)
                    Core.RenderingWindows[i].PaintIdentity(false);
            });
        }
        public void ScreenShowIdentity(bool Show)
        {
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(Show);
        }
        public void ScreenShowIdentity(bool Show, float Opacity)
        {
            for (int i = 0; i < screenCount; i++)
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
        
        #region Hal Interface

        /// <summary>
        /// Starts up Hal
        /// </summary>
        private void Hal_Start()
        {
            // Start HAL (Done in separate thread to speed up startup)
            OpenMobile.Threading.SafeThread.Asynchronous(() => { hal = new HalInterface(); });
        }
        /// <summary>
        /// Shuts down Hal
        /// </summary>
        private void Hal_Stop()
        {
            // Stop HAL 
            if (hal != null)
                hal.close();
        }

        /// <summary>
        /// Send a message to HAL
        /// </summary>
        /// <param name="text">Message</param>
        public void Hal_Send(string text)
        {
            // Only send if hal is available
            if (hal != null)
                hal.Send(text);
            else
                DebugMsg( DebugMessageType.Warning,"Message sent to HAL when HAL was not ready (" + text + ")");
        }

        #endregion


    }
}
