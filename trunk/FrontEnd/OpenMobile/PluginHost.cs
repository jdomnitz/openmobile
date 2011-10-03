﻿/*********************************************************************************
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

namespace OpenMobile
{
    public sealed class PluginHost : IPluginHost
    {
        #region Private Vars
        private IAVPlayer[] currentMediaPlayer;
        private ITunedContent[] currentTunedContent;
        private static int screenCount = DisplayDevice.AvailableDisplays.Count;
        private int instanceCount = -1;
        private int[] instance;
        private string skinpath;
        private string datapath;
        private string pluginpath;
        private bool internetAccess;
        private string ipAddress;
        private List<mediaInfo>[] queued;
        private int[] currentPosition;
        private int[] nextPosition;
        public HalInterface hal;
        private bool vehicleInMotion;
        private eGraphicsLevel level;
        private Rectangle[] videoPosition;
        private bool suspending;
        private bool closing;
        historyCollection history = new historyCollection(screenCount);
        #endregion

        private static IBasePlugin getPluginByName(string name)
        {
            foreach (IBasePlugin c in Core.pluginCollection)
            {
                if (c.pluginName == name)
                {
                    return c;
                }
            }
            return null;
        }
        private static eMediaType classifySource(string source)
        {
            if (source.StartsWith("bluetooth") == true)
                return eMediaType.BluetoothResource;
            if (source.ToLower().StartsWith("http:") == true)
            {
                if (source.ToLower().Contains("youtube") == true)
                    return eMediaType.YouTube;
                return eMediaType.HTTPUrl;
            }
            if ((source.ToLower().StartsWith("udp:") == true) || (source.ToLower().StartsWith("rtsp:") == true))
                return eMediaType.RTSPUrl;
            if (source.ToLower().StartsWith("mms:") == true)
                return eMediaType.MMSUrl;
            if (source.ToLower().StartsWith("cam") == true)
                return eMediaType.LiveCamera;
            if (source.ToLower().StartsWith("cdda:") == true)
                return eMediaType.AudioCD;
            if (source.Contains(".") == true) //Check if its a file
            {
                if (source.StartsWith(@"\\") == true)
                    return eMediaType.Network;
                if (source.StartsWith("smb:") == true)
                    return eMediaType.Network;
                if (source.EndsWith(".cda") == true)
                    return eMediaType.AudioCD;
                return eMediaType.Local;
            }
            else
            {
                return StorageAnalyzer.Analyze(source);
            }
        }

        private void generateNext(int instance)
        {
            lock (nextPosition)
            {
                if ((random != null) && (random[instance] == true) && (queued[instance].Count > 1))
                {
                    int result;
                    do
                    {
                        result = OpenMobile.Framework.Math.Calculation.RandomNumber(0, queued[instance].Count);
                    }
                    while (nextPosition[instance] == result);
                    nextPosition[instance] = result;
                }
                else
                    nextPosition[instance] = currentPosition[instance] + 1;
                if (nextPosition[instance] == queued[instance].Count)
                    nextPosition[instance] = 0;
                if ((queued[instance].Count > nextPosition[instance]) && (getPlayingMedia(instance).Location == queued[instance][nextPosition[instance]].Location))
                {
                    nextPosition[instance]++;
                    if (nextPosition[instance] == queued[instance].Count)
                        nextPosition[instance] = 0;
                }
            }
        }

        public List<mediaInfo> getPlaylist(int instance)
        {
            if ((instance < 0) || (instance >= 8))
                return new List<mediaInfo>();
            else
                return queued[instance];
        }
        public bool setPlaylist(List<mediaInfo> source, int instance)
        {
            if ((instance < 0) || (instance >= 8))
                return false;
            if (source == null)
                return false;
            currentPosition[instance] = -1;
            queued[instance].Clear();
            queued[instance].AddRange(source.GetRange(0, source.Count));
            if (queued[instance].Count > 0)
                generateNext(instance);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, instance, string.Empty); });
            return true;
        }
        public bool appendPlaylist(List<mediaInfo> source, int instance)
        {
            if ((instance < 0) || (instance >= 8))
                return false;
            if (queued[instance].Count == 0)
                return setPlaylist(source, instance);
            bool single = (queued[instance].Count == 1);
            queued[instance].AddRange(source.GetRange(0, source.Count));
            if (single)
                generateNext(instance);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, instance, string.Empty); });
            return true;
        }
        public int instanceForScreen(int screen)
        { //Instance numbers are incremented by 1 so that "not set"==0
            if (instance == null)
                instance = new int[ScreenCount];
            if (instance[screen] == 0)
                instance[screen] = getInstance(screen);
            if (instance[screen] == 0)
                return 0;
            return instance[screen] - 1;
        }
        static string[] devices;
        private static int getInstance(int screen)
        {
            string str;
            using (PluginSettings settings = new PluginSettings())
                str = settings.getSetting("Screen" + (screen + 1).ToString() + ".SoundCard");
            if (str.Length == 0)
                return 0;
            if (devices == null)
                if (!refreshDevices())
                    return 0;
            return Array.FindIndex(devices, p => (p != null) && (p.Replace("  ", " ") == str)) + 1;
        }
        private static bool refreshDevices()
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
                    devices = devs;
                    return true;
                }
            }
            return false;
        }
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
        public string DataPath
        {
            get
            {
                if (datapath == null)
                    datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openMobile");
                return datapath;
            }
        }
        public string PluginPath
        {
            get
            {
                if (pluginpath == null)
                    pluginpath = Path.Combine(Application.StartupPath, "Plugins");
                return pluginpath;
            }
        }
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
        public object UIHandle(int screen)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return (IntPtr)(-1); //Out of bounds
            return Core.RenderingWindows[screen].WindowHandle;
        }
        public eGraphicsLevel GraphicsLevel
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }
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
        private Location _location = new Location();
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
        public void SetVideoPosition(int instance, Rectangle videoArea)
        {
            if ((instance < 0) || (instance >= instanceCount))
                return;
            if (videoPosition[instance] == videoArea)
                return;
            videoPosition[instance] = videoArea;
            raiseSystemEvent(eFunction.videoAreaChanged, instance.ToString(), String.Empty, String.Empty);
        }
        public Rectangle GetVideoPosition(int instance)
        {
            if ((instance < 0) || (instance >= instanceCount))
                return Rectangle.Empty;
            return videoPosition[instance];
        }
        Timer tmr;
        public PluginHost()
        {
            queued = new List<mediaInfo>[8];
            currentMediaPlayer = new IAVPlayer[8];
            currentTunedContent = new ITunedContent[8];
            currentPosition = new int[8];
            nextPosition = new int[8];
            for (int i = 0; i < screenCount; i++)
                history.Enqueue(i, "MainMenu", String.Empty, false);
            for (int i = 0; i < 8; i++)
                queued[i] = new List<mediaInfo>();
            if (Directory.Exists(DataPath) == false)
                Directory.CreateDirectory(DataPath);
            InputRouter.OnHighlightedChanged += new userInteraction(InputRouter_OnHighlightedChanged);
            Credentials.OnAuthorizationRequested += new Credentials.Authorization(Credentials_OnAuthorizationRequested);
            Credentials.Open();
            tmr = new Timer(tmr_time, null, 60000, 60000);
        }
        DateTime current;
        void tmr_time(object state)
        {
            if (current == DateTime.MinValue)
                current = DateTime.Now;
            if (current.Hour != DateTime.Now.Hour)
            {
                raiseSystemEvent(eFunction.hourChanged, String.Empty, String.Empty, String.Empty);
                if (current.Day != DateTime.Now.Day)
                    raiseSystemEvent(eFunction.dateChanged, String.Empty, String.Empty, String.Empty);
                current = DateTime.Now;
            }
        }
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

        private void InputRouter_OnHighlightedChanged(OMControl sender, int screen)
        {
            if (OnHighlightedChanged != null)
                OnHighlightedChanged(sender, screen);
        }
        public void Load()
        {
            for (int i = 0; i < ScreenCount; i++)
                if (!DisplayDevice.AvailableDisplays[i].Landscape)
                    raiseSystemEvent(eFunction.screenOrientationChanged, i.ToString(), "Portrait", string.Empty);
            SandboxedThread.Asynchronous(loadPlaylists);
        }
        public void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
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

        public void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
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

        #region Power Events
        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (screenCount != DisplayDevice.AvailableDisplays.Count)
            {
                if (screenCount < DisplayDevice.AvailableDisplays.Count)
                {
                    raiseSystemEvent(eFunction.screenAdded, String.Empty, String.Empty, String.Empty);
                    execute(eFunction.restartProgram);
                }
                else
                {
                    raiseSystemEvent(eFunction.screenRemoved, String.Empty, String.Empty, String.Empty);
                }
            }
        }
        #endregion
        public bool execute(eFunction function)
        {
            switch (function)
            {
                case eFunction.closeProgram:
                    //Don't lock this to prevent deadlock
                    {
                        if (!closing)
                        {
                            closing = true;
                            RenderingWindow.CloseRenderer();
                            if (hal != null)
                                hal.snd("44");
                            savePlaylists();
                            SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty); });
                        }
                    }
                    return true;
                case eFunction.restartProgram:
                    try
                    {
                        savePlaylists();
                        raiseSystemEvent(eFunction.closeProgram, String.Empty, String.Empty, String.Empty);
                        if (hal != null)
                            hal.close();
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
                case eFunction.stopListeningForSpeech:
                    raiseSystemEvent(eFunction.stopListeningForSpeech, String.Empty, String.Empty, String.Empty);
                    return true;
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
                case eFunction.shutdown:
                    hal.snd("46");
                    return true;
                case eFunction.restart:
                    hal.snd("47");
                    return true;
                case eFunction.hibernate:
                    hal.snd("45");
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    return true;
                case eFunction.standby:
                    hal.snd("48");
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    return true;
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this);
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this);
                case eFunction.settingsChanged:
                    for (int i = 0; i < screenCount; i++)
                        instance[i] = getInstance(i);
                    return true;
                case eFunction.refreshData:
                    bool result = true;
                    foreach (IBasePlugin p in Core.pluginCollection.FindAll(i => typeof(IDataProvider).IsInstanceOfType(i)))
                    {
                        result = result & execute(eFunction.refreshData, p.pluginName);
                    }
                    return result;

            }
            return false;
        }

        internal void savePlaylists()
        {
            using (PluginSettings s = new PluginSettings())
            {
                for (int i = 0; i < 8; i++)
                {
                    Playlist.writePlaylistToDB(this, "Current" + i.ToString(), getPlaylist(i));
                    s.setSetting("Media.Instance" + i.ToString() + ".Random", getRandom(i).ToString());
                }
            }
        }

        private void loadPlaylists()
        {
            using (PluginSettings s = new PluginSettings())
            {
                bool res;
                string dbname = s.getSetting("Default.MusicDatabase");
                for (int i = 0; i < 8; i++)
                {
                    setPlaylist(Playlist.readPlaylistFromDB(this, "Current" + i.ToString(), dbname), i);
                    if (bool.TryParse(s.getSetting("Media.Instance" + i.ToString() + ".Random"), out res))
                        setRandom(i, res);
                }
            }
        }

        private class historyCollection
        {
            public struct historyItem
            {
                public bool forgettable;
                public string pluginName;
                public string panelName;
                public historyItem(string pluginName, string panelName, bool forgetMe)
                {
                    this.pluginName = pluginName;
                    this.panelName = panelName;
                    this.forgettable = forgetMe;
                }
            }
            bool[] disabled;
            Stack<historyItem>[] items;
            historyItem[] currentItem;
            public void setDisabled(int screen, bool isDisabled)
            {
                disabled[screen] = isDisabled;
            }
            public bool getDisabled(int screen)
            {
                return disabled[screen];
            }
            public historyCollection(int count)
            {
                items = new Stack<historyItem>[count];
                currentItem = new historyItem[count];
                disabled = new bool[count];
                for (int i = 0; i < count; i++)
                    items[i] = new Stack<historyItem>(10);
            }

            public void Enqueue(int screen, string pluginName, string panelName, bool forgetMe)
            {
                if (items[screen].Count == 10)
                {
                    Stack<historyItem> tmp = new Stack<historyItem>(10);
                    for (int i = 0; i < 6; i++)
                    {
                        tmp.Push(items[screen].Pop());
                    }
                }
                if ((currentItem[screen].pluginName != null) && (currentItem[screen].forgettable == false))
                    items[screen].Push(currentItem[screen]);
                currentItem[screen] = new historyItem(pluginName, panelName, forgetMe);
            }
            public historyItem CurrentItem(int screen)
            {
                return currentItem[screen];
            }
            public historyItem Peek(int screen)
            {
                return items[screen].Peek();
            }
            public historyItem Dequeue(int screen)
            {
                historyItem tmp = currentItem[screen];
                currentItem[screen] = items[screen].Pop();
                return tmp;
            }
            public int Count(int screen)
            {
                return items[screen].Count;
            }
            public void Clear(int screen)
            {
                items[screen].Clear();
                currentItem[screen] = new historyItem();
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
            set // Added by Borte to be able to control how many screens to use
            {
                screenCount = value;
            }
        }
        
        /// <summary>
        /// Startup screen of OM (0 is normal screen, any other value means a specific startup screen is requested)
        /// </summary>
        public int StartupScreen { get; set; }

        public int InstanceCount
        {
            get
            {
                if (instanceCount == -1)
                {
                    if (refreshDevices())
                        instanceCount = devices.Length;
                    else
                        return -1;
                    videoPosition = new Rectangle[instanceCount];
                }
                return instanceCount;
            }
        }

        public bool execute(eFunction function, string arg)
        {
            int ret;
            IBasePlugin plugin;
            switch (function)
            {
                case eFunction.promptDialNumber:
                    raiseSystemEvent(eFunction.promptDialNumber, arg, String.Empty, String.Empty);
                    return true;
                case eFunction.showNavPanel:
                    plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (plugin == null)
                        return false;
                    return ((INavigation)plugin).switchTo(arg);
                case eFunction.navigateToPOI:
                    plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (plugin == null)
                        return false;
                    return ((INavigation)plugin).findPOI(arg);
                case eFunction.ExecuteTransition:
                    return execute(eFunction.ExecuteTransition, arg, "Crossfade");
                case eFunction.CancelTransition:
                    if (int.TryParse(arg, out ret))
                    {
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].Rollback();
                        }
                    }
                    return false;
                case eFunction.TransitionToPanel:
                    if (int.TryParse(arg, out ret))
                        return execute(eFunction.TransitionToPanel, arg, history.CurrentItem(ret).pluginName, String.Empty);
                    return false;
                case eFunction.TransitionFromPanel:
                    if (int.TryParse(arg, out ret))
                        return execute(eFunction.TransitionFromPanel, arg, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                    return false;
                case eFunction.goBack:
                    return execute(eFunction.goBack, arg, "SlideRight");
                case eFunction.unloadAVPlayer:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentMediaPlayer[ret] == null)
                            return false;
                        try
                        {
                            if (currentMediaPlayer[ret].getPlayerStatus(ret) == ePlayerStatus.Playing)
                                currentMediaPlayer[ret].stop(ret);
                        }
                        catch (Exception) { }
                        currentMediaPlayer[ret].OnMediaEvent -= raiseMediaEvent;
                        string name = currentMediaPlayer[ret].pluginName;
                        currentMediaPlayer[ret] = null;
                        raiseMediaEvent(eFunction.unloadAVPlayer, ret, name);
                        return true;
                    }
                    return false;
                case eFunction.unloadTunedContent:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        currentTunedContent[ret].setPowerState(ret, false);
                        currentTunedContent[ret].OnMediaEvent -= raiseMediaEvent;
                        string name = currentTunedContent[ret].pluginName;
                        currentTunedContent[ret] = null;
                        raiseMediaEvent(eFunction.unloadTunedContent, ret, name);
                        return true;
                    }
                    return false;
                case eFunction.Play:
                    if (int.TryParse(arg, out ret) == false)
                        return false;
                    if (currentMediaPlayer[ret] == null)
                        return false;
                    return currentMediaPlayer[ret].play(ret);
                case eFunction.showVideoWindow:
                    if (int.TryParse(arg, out ret) == false)
                        return false;
                    if (currentMediaPlayer[ret] == null)
                        return false;
                    return currentMediaPlayer[ret].SetVideoVisible(ret, true);
                case eFunction.hideVideoWindow:
                    if (int.TryParse(arg, out ret) == false)
                        return false;
                    if (currentMediaPlayer[ret] == null)
                        return false;
                    return currentMediaPlayer[ret].SetVideoVisible(ret, false);
                case eFunction.Pause:
                    if (int.TryParse(arg, out ret) == false)
                        return false;
                    if (currentMediaPlayer[ret] == null)
                    {
                        if (currentTunedContent[ret] != null)
                        {
                            if (typeof(IPausable).IsInstanceOfType(currentTunedContent[ret]))
                                return ((IPausable)currentTunedContent[ret]).pause(ret);
                        }
                        return false;
                    }
                    return currentMediaPlayer[ret].pause(ret);
                case eFunction.Stop:
                    if (int.TryParse(arg, out ret) == false)
                        return false;
                    if (currentMediaPlayer[ret] == null)
                        return false;
                    return currentMediaPlayer[ret].stop(ret);
                case eFunction.nextMedia:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (queued[ret].Count == 0)
                            return false;
                        currentPosition[ret] = nextPosition[ret];
                        bool b = execute(eFunction.Play, arg, queued[ret][currentPosition[ret]].Location);
                        generateNext(ret);
                        return b;
                    }
                    return false;
                case eFunction.previousMedia:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (queued[ret].Count == 0)
                            return false;
                        lock (currentPosition)
                        {
                            currentPosition[ret]--;
                            if (currentPosition[ret] <= -1)
                                currentPosition[ret] = queued[ret].Count - 1;
                        }
                        bool b = execute(eFunction.Play, arg, queued[ret][currentPosition[ret]].Location);
                        generateNext(ret);
                        return b;
                    }
                    return false;
                case eFunction.scanBackward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].scanReverse(ret);
                    }
                    return false;
                case eFunction.scanForward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].scanForward(ret);
                    }
                    return false;
                case eFunction.scanBand:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].scanBand(ret);
                    }
                    return false;
                case eFunction.stepBackward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].stepBackward(ret);
                    }
                    return false;
                case eFunction.stepForward:

                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].stepForward(ret);
                    }
                    return false;
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
                case eFunction.dataUpdated:
                    raiseSystemEvent(eFunction.dataUpdated, arg, String.Empty, String.Empty);
                    return true;
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg, String.Empty, String.Empty);
                    return true;
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
                case eFunction.loadSpeechContext:
                    plugin = Core.pluginCollection.Find(q => typeof(ISpeech).IsInstanceOfType(q));
                    if (plugin == null)
                        return false;
                    return ((ISpeech)plugin).loadContext(arg);
                case eFunction.Speak:
                    plugin = Core.pluginCollection.Find(q => typeof(ISpeech).IsInstanceOfType(q));
                    if (plugin == null)
                        return false;
                    return ((ISpeech)plugin).speak(arg);
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this, arg);
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this, arg);
                case eFunction.setSystemVolume:
                    return execute(eFunction.setSystemVolume, arg, "0");
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
                case eFunction.ejectDisc:
                    if (string.IsNullOrEmpty(arg))
                        return false;
                    hal.snd("35|" + arg);
                    return true;
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
                case eFunction.blockGoBack:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        history.setDisabled(ret, true);
                        return true;
                    }
                    return false;
                case eFunction.unblockGoBack:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        history.setDisabled(ret, false);
                        return true;
                    }
                    return false;
                case eFunction.clearHistory:
                    if (int.TryParse(arg, out ret))
                    {
                        history.Clear(ret);
                        return true;
                    }
                    return false;
                case eFunction.settingsChanged:
                    raiseSystemEvent(eFunction.settingsChanged, arg, String.Empty, String.Empty);
                    return true;
                case eFunction.minimize:
                    if (int.TryParse(arg, out ret))
                    {
                        Core.RenderingWindows[ret].WindowState = WindowState.Minimized;
                        return true;
                    }
                    return false;
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
        public bool execute(eFunction function, string arg1, string arg2)
        {
            int ret, ret2;
            switch (function)
            {
                case eFunction.TransitionToPanel:
                    return execute(eFunction.TransitionToPanel, arg1, arg2, String.Empty);
                case eFunction.TransitionFromPanel:
                    return execute(eFunction.TransitionFromPanel, arg1, arg2, String.Empty);
                case eFunction.ExecuteTransition:
                    eGlobalTransition effect;

                    if (level == eGraphicsLevel.Minimal)
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
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, String.Empty);
                    return true;
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2);
                case eFunction.loadAVPlayer:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentMediaPlayer[ret] != null)
                            return false;
                        if (currentTunedContent[ret] != null)
                            execute(eFunction.unloadTunedContent, arg1);
                        IAVPlayer player = (IAVPlayer)Core.pluginCollection.FindAll(i => typeof(IAVPlayer).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                        if (player == null)
                            return false;
                        //Hook it if its not already hooked
                        if (Array.Exists<IAVPlayer>(currentMediaPlayer, a => a == player) == false)
                            player.OnMediaEvent += raiseMediaEvent;
                        currentMediaPlayer[ret] = player;
                        raiseMediaEvent(eFunction.loadAVPlayer, ret, arg2);
                        return true;
                    }
                    return false;
                case eFunction.loadTunedContent:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentTunedContent[ret] != null)
                            return false;
                        if (currentMediaPlayer[ret] != null)
                            execute(eFunction.unloadAVPlayer, arg1);
                        ITunedContent player = (ITunedContent)Core.pluginCollection.FindAll(i => typeof(ITunedContent).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                        if (player == null)
                            return false;
                        //Only hook it once
                        if (Array.Exists<ITunedContent>(currentTunedContent, a => a == player) == false)
                            player.OnMediaEvent += raiseMediaEvent;
                        currentTunedContent[ret] = player;
                        if (!currentTunedContent[ret].setPowerState(ret, true))
                        {
                            execute(eFunction.unloadTunedContent, arg1);
                            return false;
                        }
                        raiseMediaEvent(eFunction.loadTunedContent, ret, arg2);
                        return true;
                    }
                    return false;
                case eFunction.setPlaylistPosition:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= 8))
                            return false;
                        if (int.TryParse(arg2, out ret2) == true)
                        {
                            if (queued[ret].Count > ret2)
                            {
                                currentPosition[ret] = ret2;
                                generateNext(ret);
                                return true;
                            }
                            return false;
                        }
                        currentPosition[ret] = queued[ret].FindIndex(p => p.Location == arg2);
                        if (queued[ret].Count > 0)
                            generateNext(ret);
                        return true;
                    }
                    return false;
                case eFunction.Play:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (arg2 == null)
                            return false;
                        if (currentMediaPlayer[ret] == null)
                            return findAlternatePlayer(ret, arg2, classifySource(arg2));
                        if (currentMediaPlayer[ret].play(ret, arg2, classifySource(arg2)) == false)
                            return findAlternatePlayer(ret, arg2, classifySource(arg2));
                        else
                            return true;
                    }
                    return false;
                case eFunction.setPosition:
                    try
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if (currentMediaPlayer[ret] == null)
                                return false;
                            float pos;
                            if (float.TryParse(arg2, out pos))
                                return currentMediaPlayer[ret].setPosition(ret, pos);
                        }
                        return false;
                    }
                    catch (Exception) { return false; }
                case eFunction.setPlaybackSpeed:
                    try
                    {
                        if (int.TryParse(arg1, out ret) == true)
                        {
                            if (currentMediaPlayer[ret] == null)
                                return false;
                            float speed;
                            if (float.TryParse(arg2, out speed))
                                return currentMediaPlayer[ret].setPlaybackSpeed(ret, speed);
                        }
                        return false;
                    }
                    catch (Exception) { return false; }
                case eFunction.setPlayerVolume:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentMediaPlayer[ret] == null)
                        {
                            if (currentTunedContent[ret] == null)
                                return false;
                            raiseMediaEvent(eFunction.setPlayerVolume, ret, arg2);
                            if (int.TryParse(arg2, out ret2))
                                return currentTunedContent[ret].setVolume(ret, ret2);
                        }
                        else
                        {
                            raiseMediaEvent(eFunction.setPlayerVolume, ret, arg2);
                            if (int.TryParse(arg2, out ret2))
                                return currentMediaPlayer[ret].setVolume(ret, ret2);
                        }
                    }
                    return false;
                case eFunction.tuneTo:

                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (arg2 == null)
                            return false;
                        if (currentTunedContent[ret] == null)
                            return findAlternateTuner(ret, arg2);
                        if (currentTunedContent[ret].tuneTo(ret, arg2) == false)
                            return findAlternateTuner(ret, arg2);
                        else
                            return true;
                    }
                    return false;
                case eFunction.backgroundOperationStatus:
                    SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.backgroundOperationStatus, arg1, arg2, String.Empty); });
                    return true;
                case eFunction.sendKeyPress:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (string.IsNullOrEmpty(arg2))
                            return false;
                        return (InputRouter.SendKeyDown(ret, arg2) && InputRouter.SendKeyUp(ret, arg2));
                    }
                    return false;
                case eFunction.gesture:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.gesture, arg1, arg2, history.CurrentItem(ret).pluginName); });
                        return true;
                    }
                    return false;
                case eFunction.multiTouchGesture:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        raiseSystemEvent(eFunction.multiTouchGesture, arg1, arg2, history.CurrentItem(ret).pluginName);
                        return true;
                    }
                    return false;
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this, arg1, arg2);
                case eFunction.setSystemVolume:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < -2) || (ret > 100))
                            return false;
                        if (int.TryParse(arg2, out ret) == false)
                            return false;
                        if ((ret < 0) || (ret >= instanceCount))
                            return false;
                        hal.snd("34|" + arg1 + "|" + arg2);
                        raiseSystemEvent(eFunction.systemVolumeChanged, arg1, arg2, String.Empty);
                        return true;
                    }
                    return false;
                case eFunction.setSubVolume:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < -2) || (ret > 100))
                            return false;
                        if (int.TryParse(arg2, out ret2) == false)
                            return false;
                        if ((ret2 < 0) || (ret2 >= instanceCount))
                            return false;
                        hal.snd("66|" + arg1 + "|" + arg2);
                        return true;
                    }
                    return false;
                case eFunction.setBand:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].setBand(ret, (eTunedContentBand)Enum.Parse(typeof(eTunedContentBand), arg2, false));
                    }
                    return false;
                case eFunction.setMonitorBrightness:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= screenCount))
                            return false;
                        if (int.TryParse(arg2, out ret2) == false)
                            return false;
                        if ((ret2 == 0) && ((Environment.OSVersion.Version.Major < 6) || ret > 0))
                            Core.RenderingWindows[ret].FadeOut();
                        hal.snd("40|" + arg1 + "|" + arg2);
                        return true;
                    }
                    return false;
                case eFunction.setSystemBalance:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= instanceCount))
                            return false;
                        if (int.TryParse(arg2, out ret2) == false)
                            return false;
                        hal.snd("42|" + arg1 + "|" + arg2);
                        return true;
                    }
                    return false;
                case eFunction.impersonateInstanceForScreen:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= screenCount))
                            return false;
                        if (int.TryParse(arg2, out ret2))
                        {
                            if ((ret2 < 0) || (ret2 >= instanceCount))
                                return false;
                            instance[ret] = ret2 + 1;
                            raiseSystemEvent(eFunction.impersonateInstanceForScreen, arg1, arg2, String.Empty);
                        }
                    }
                    return false;
            }
            return false;
        }

        private bool findAlternateTuner(int ret, string arg2)
        {
            if (currentMediaPlayer[ret] != null)
                execute(eFunction.unloadAVPlayer, ret.ToString());
            ITunedContent tmp = currentTunedContent[ret];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadTunedContent, ret.ToString());
                    execute(eFunction.loadTunedContent, ret.ToString(), plugins[i].pluginName);
                    if ((currentTunedContent[ret] != null) && (currentTunedContent[ret].tuneTo(ret, arg2) == true))
                        return true;
                }
            }
            if (tmp == null)
                execute(eFunction.unloadTunedContent, ret.ToString());
            else
                execute(eFunction.loadTunedContent, ret.ToString(), tmp.pluginName);
            return false;
        }

        private bool findAlternatePlayer(int ret, string arg2, eMediaType type)
        {
            if (currentTunedContent[ret] != null)
                execute(eFunction.unloadTunedContent, ret.ToString());
            IAVPlayer tmp = currentMediaPlayer[ret];
            List<IBasePlugin> plugins = Core.pluginCollection.FindAll(p => typeof(IAVPlayer).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadAVPlayer, ret.ToString());
                    execute(eFunction.loadAVPlayer, ret.ToString(), plugins[i].pluginName);
                    if (currentMediaPlayer[ret].play(ret, arg2, type) == true)
                        return true;
                }
            }
            if (tmp == null)
                execute(eFunction.unloadAVPlayer, ret.ToString());
            else
                execute(eFunction.loadAVPlayer, ret.ToString(), tmp.pluginName);
            raiseMediaEvent(eFunction.playbackFailed, ret, arg2);
            return false;
        }
        public bool execute(eFunction function, string arg1, string arg2, string arg3)
        {
            OMPanel panel;
            int ret;
            switch (function)
            {
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2, arg3);
                case eFunction.TransitionToPanel:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        panel = getPanelByName(arg2, arg3, ret);
                        if (panel == null)
                            return false;
                        if (panel.Priority > ePriority.Urgent) //prevent panels from entering the security layer
                            panel.Priority = ePriority.Urgent;
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
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, arg3);
                    return true;
            }
            return false;
        }

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
        public bool sendMessage(string to, string from, string message)
        {
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
            else if (to == "OMHal")
            {
                hal.snd("-1|" + from + "|" + message);
                return true;
            }
            try
            {
                if (to.StartsWith("Screen"))
                {
                    int screen;
                    if ((to.Length > 6) && (int.TryParse(to.Substring(6), out screen)))
                        to = history.CurrentItem(screen).pluginName;
                }
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
        public bool sendMessage<T>(string to, string from, string message, ref T data)
        {
            if (to == "SandboxedThread")
            {
                SandboxedThread.Handle(data as Exception);
                return true;
            }
            try
            {
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
        public void raiseSystemEvent(eFunction e, string arg1, string arg2, string arg3)
        {
            try
            {
                if (OnSystemEvent != null)
                    OnSystemEvent(e, arg1, arg2, arg3);
            }
            catch (Exception error) { SandboxedThread.Handle(error); }
        }
        public void raisePowerEvent(ePowerEvent e)
        {
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
        private void raiseNavigationEvent(eNavigationEvent type, string arg)
        {
            try
            {
                if (OnNavigationEvent != null)
                    OnNavigationEvent(type, arg);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
        }
        public void raiseWirelessEvent(eWirelessEvent type, string arg)
        {
            try
            {
                if (OnWirelessEvent != null)
                    OnWirelessEvent(type, arg);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
        }
        private void raiseMediaEvent(eFunction e, int instance, string arg)
        {
            if (OnMediaEvent != null)
                SandboxedThread.Asynchronous(delegate() { OnMediaEvent(e, instance, arg); });
            if (e == eFunction.nextMedia)
                while ((!execute(eFunction.nextMedia, instance.ToString())) && (queued[instance].Count > 1))
                    Thread.Sleep(200);
            else if (e == eFunction.showVideoWindow)
            {
                for (int i = 0; i < screenCount; i++)
                    if (instanceForScreen(i) == instance)
                        Core.RenderingWindows[i].VideoPlaying = true;
            }
            else if (e == eFunction.hideVideoWindow)
            {
                for (int i = 0; i < screenCount; i++)
                    if (instanceForScreen(i) == instance)
                        Core.RenderingWindows[i].VideoPlaying = false;
            }
        }
        public bool raiseKeyPressEvent(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg)
        {
            try
            {
                if (OnKeyPress != null)
                    return OnKeyPress(type, arg);
            }
            catch (Exception e) { SandboxedThread.Handle(e); }
            return false;
        }

        public event SystemEvent OnSystemEvent;
        public event MediaEvent OnMediaEvent;
        public event PowerEvent OnPowerChange;
        public event StorageEvent OnStorageEvent;
        public event NavigationEvent OnNavigationEvent;
        public event KeyboardEvent OnKeyPress;
        public event WirelessEvent OnWirelessEvent;
        public event userInteraction OnHighlightedChanged;
        private List<imageItem> imageCache = new List<imageItem>();
        public imageItem getSkinImage(string imageName)
        {
            return getSkinImage(imageName, false);
        }
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
                        im.name = imageName;
                        imageCache.Add(im);
                        return im;
                    }
                    catch (System.ArgumentException)
                    {
                        try
                        {
                            string file = Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".gif");
                            if (!File.Exists(file))
                            {
                                file = Path.Combine(Path.Combine(Application.StartupPath, "Skins", "Default"), imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png");
                            }
                            im.image = OImage.FromFile(file);
                            im.name = imageName;
                            imageCache.Add(im);
                            return im;
                        }
                        catch (System.ArgumentException)
                        {
                            return imageItem.MISSING;
                        }
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
        private bool[] random;
        public bool getRandom(int instance)
        {
            if (instanceCount == -1)
                return false;
            if ((instance < 0) || (instance >= instanceCount))
                return false;
            if (random == null)
                random = new bool[instanceCount];
            return random[instance];
        }
        public bool setRandom(int instance, bool value)
        {
            if (instanceCount == -1)
                return false;
            if ((instance < 0) || (instance >= instanceCount))
                return false;
            if (random == null)
                random = new bool[instanceCount];
            random[instance] = value;
            if (value)
                raiseMediaEvent(eFunction.RandomChanged, instance, "Enabled");
            else
                raiseMediaEvent(eFunction.RandomChanged, instance, "Disabled");
            generateNext(instance);
            return true;
        }
        public mediaInfo getPlayingMedia(int instance)
        {
            if (currentMediaPlayer[instance] != null)
                return currentMediaPlayer[instance].getMediaInfo(instance);
            if (currentTunedContent[instance] != null)
                return currentTunedContent[instance].getMediaInfo(instance);
            return new mediaInfo();
        }
        public mediaInfo getNextMedia(int instance)
        {
            if (currentMediaPlayer[instance] == null)
                return new mediaInfo();
            return queued[instance][nextPosition[instance]];
        }

        public object getData(eGetData dataType, string name)
        {
            object o;
            getData(dataType, name, out o);
            return o;
        }

        public void getData(eGetData dataType, string name, out object data)
        {
            IBasePlugin plugin;
            data = null;

            try
            {
                switch (dataType)
                {
                    case eGetData.GetMap:
                        plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                        if (plugin == null)
                            return;
                        if (plugin is INavigation)
                            data = ((INavigation)plugin).getMap;
                        return;
                    case eGetData.GetMediaDatabase:
                        plugin = Core.pluginCollection.Find(t => t.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IMediaDatabase)
                            data = ((IMediaDatabase)plugin).getNew();
                        break;
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
                    case eGetData.GetSystemVolume:
                        getData(eGetData.GetSystemVolume, name, "0", out data);
                        return;
                    case eGetData.GetFirmwareInfo:
                        plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IRawHardware)
                            data = ((IRawHardware)plugin).firmwareVersion;
                        return;
                    case eGetData.GetDeviceInfo:
                        plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (plugin == null)
                            return;
                        if (plugin is IRawHardware)
                            data = ((IRawHardware)plugin).deviceInfo;
                        return;
                    case eGetData.GetAvailableNetworks:
                        List<connectionInfo> cInf = new List<connectionInfo>();
                        foreach (IBasePlugin g in Core.pluginCollection.FindAll(p => typeof(INetwork).IsInstanceOfType(p)))
                        {
                            foreach (connectionInfo c in ((INetwork)g).getAvailableNetworks())
                                cInf.Add(c);
                        }
                        data = cInf;
                        return;
                    case eGetData.GetAudioDevices:
                        if (devices == null)
                            refreshDevices();
                        data = devices;
                        return;
                    case eGetData.GetAvailableSkins:
                        string[] ret = Directory.GetDirectories(Path.Combine(Application.StartupPath, "Skins"));
                        for (int i = 0; i < ret.Length; i++)
                            ret[i] = ret[i].Replace(Path.Combine(Application.StartupPath, "Skins", String.Empty), String.Empty);
                        data = ret;
                        return;
                    case eGetData.GetAvailableSensors:
                        {
                            if (string.IsNullOrEmpty(name))
                            {
                                List<Sensor> Sensors = new List<Sensor>();
                                foreach (IRawHardware g in Core.pluginCollection.FindAll(p => typeof(IRawHardware).IsInstanceOfType(p)))
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
                    case eGetData.GetAvailableKeyboards:
                        data = InputRouter.Keyboards;
                        return;
                    case eGetData.GetAvailableMice:
                        data = InputRouter.Mice;
                        return;
                    case eGetData.GetMappedKeyboards:
                        data = InputRouter.KeyboardsMapped;
                        return;
                    case eGetData.GetUnMappedKeyboards:
                        data = InputRouter.KeyboardsUnMapped;
                        return;
                    case eGetData.GetMappedMice:
                        data = InputRouter.MiceMapped;
                        return;
                    case eGetData.GetUnMappedMice:
                        data = InputRouter.MiceUnMapped;
                        return;
                    case eGetData.GetMouseDetectedUnit:
                        data = InputRouter.DetectMouseDevice();
                        return;
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

        public object getData(eGetData dataType, string name, string param)
        {
            object o;
            getData(dataType, name, param, out o);
            return o;
        }

        public void getData(eGetData dataType, string name, string param, out object data)
        {
            data = null;
            int ret;
            IBasePlugin plugin;
            try
            {
                switch (dataType)
                {
                    case eGetData.GetMediaPosition:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if (currentMediaPlayer[ret] == null)
                                if (currentTunedContent[ret] == null)
                                    return;
                                else
                                    data = currentTunedContent[ret].playbackPosition;
                            else
                                data = currentMediaPlayer[ret].getCurrentPosition(ret);
                        }
                        return;
                    case eGetData.GetPlayerVolume:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if (currentMediaPlayer[ret] == null)
                                if (currentTunedContent[ret] == null)
                                    return;
                                else
                                    data = currentTunedContent[ret].getVolume(ret);
                            else
                                data = currentMediaPlayer[ret].getVolume(ret);
                        }
                        return;
                    case eGetData.GetSystemVolume:
                        hal.snd("3|" + param);
                        bool res = true;
                        while (res == true)
                        {
                            Thread.Sleep(5);
                            res = (hal.volume == null);
                            if (res == false)
                            {
                                if (hal.volume[0] == param)
                                {
                                    if (int.TryParse(hal.volume[1], out ret))
                                        data = ret;
                                    hal.volume = null;
                                }
                            }
                        }
                        return;
                    case eGetData.GetScreenBrightness:
                        hal.snd("1|" + param);
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
                    case eGetData.GetScaleFactors:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if ((ret >= 0) && (ret < ScreenCount))
                                data = Core.RenderingWindows[ret].ScaleFactors;
                        }
                        return;
                    case eGetData.GetPlaybackSpeed:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if (currentMediaPlayer[ret] == null)
                                return;
                            else
                                data = currentMediaPlayer[ret].getPlaybackSpeed(ret);
                        }
                        return;
                    case eGetData.GetMediaStatus:
                        if (int.TryParse(param, out ret) == true)
                        {
                            if (string.IsNullOrEmpty(name))
                            {
                                if (currentMediaPlayer[ret] == null)
                                    if (currentTunedContent[ret] == null)
                                        return;
                                    else
                                        data = currentTunedContent[ret].getStationInfo(ret);
                                else
                                    data = currentMediaPlayer[ret].getPlayerStatus(ret);
                            }
                            else
                            {
                                plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                                if (plugin == null)
                                    return;
                                else if (typeof(IAVPlayer).IsInstanceOfType(plugin))
                                    data = ((IAVPlayer)plugin).getPlayerStatus(ret);
                                else if (typeof(ITunedContent).IsInstanceOfType(plugin))
                                    data = ((ITunedContent)plugin).getStationInfo(ret);
                            }
                        }
                        return;
                    case eGetData.GetTunedContentInfo:
                        {
                            int q;
                            if (int.TryParse(param, out q) == true)
                            {
                                if (currentTunedContent[q] != null)
                                    data = currentTunedContent[q].getStatus(q);
                            }
                        }
                        return;
                    case eGetData.GetStationList:
                        {
                            int q;
                            if (int.TryParse(param, out q) == true)
                            {
                                if (currentTunedContent[q] != null)
                                    data = currentTunedContent[q].getStationList(q);
                            }
                        }
                        return;
                    case eGetData.GetAvailableNavPanels:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).availablePanels;
                        return;
                    case eGetData.GetCurrentPosition:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).Position;
                        return;
                    case eGetData.GetNearestAddress:
                        plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                        if (plugin == null)
                            return;
                        data = ((INavigation)plugin).Location;
                        return;
                    case eGetData.GetDestination:
                        {
                            plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                            if (plugin == null)
                                return;
                            data = ((INavigation)plugin).Destination;
                        }
                        return;
                    case eGetData.GetSupportedBands:
                        {
                            if (int.TryParse(param, out ret) == true)
                            {
                                if (currentTunedContent[ret] != null)
                                    data = currentTunedContent[ret].getSupportedBands(ret);
                            }
                        }
                        return;
                    case eGetData.GetMiceUnitsForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetMiceDeviceListForScreen(ret);
                        }
                        return;
                    case eGetData.GetKeyboardUnitsForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetKeyboardsDeviceListForScreen(ret);
                        }
                        return;
                    case eGetData.GetKeyboardCurrentUnitForScreen:
                        {
                            if (int.TryParse(param, out ret) == true)
                                data = InputRouter.GetKeyboardsCurrentDeviceForScreen(ret);
                        }
                        return;
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
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].PaintIdentity();
        }
        public void ScreenShowIdentity(int MS)
        {
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(true);
            Thread.Sleep(MS);
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(false);
        }
        public void ScreenShowIdentity(bool Show)
        {
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].PaintIdentity(Show);
        }
        public void ScreenShowIdentity(int Screen, bool Show)
        {
            Core.RenderingWindows[Screen].PaintIdentity(Show);
        }

        #endregion

        public void SetWindowState(int Screen, WindowState windowState)
        {
            Core.RenderingWindows[Screen].WindowState = windowState;
        }
        public void SetAllWindowState(WindowState windowState)
        {
            for (int i = 0; i < screenCount; i++)
                Core.RenderingWindows[i].WindowState = windowState;
        }

    }
}
