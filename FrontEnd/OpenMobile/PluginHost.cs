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
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.Media;

namespace OpenMobile
{
    public sealed class PluginHost : IPluginHost
    {
        #region Private Vars
        private IAVPlayer[] currentMediaPlayer;
        private ITunedContent[] currentTunedContent;
        private int renderfirst;
        private static int screenCount = Screen.AllScreens.Length;
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
        private bool vehicleInMotion = false;
        private eGraphicsLevel level;
        private Rectangle videoPosition;
        private bool suspending = false;

        historyCollection history = new historyCollection(screenCount);
        #endregion

        private IBasePlugin getPluginByName(string name)
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
        private eMediaType classifySource(string source)
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
                        result = Framework.Math.Calculation.RandomNumber(0, queued[instance].Count);
                    }
                    while (nextPosition[instance] == result);
                    nextPosition[instance] = result;
                }
                else
                    nextPosition[instance] = currentPosition[instance] + 1;
                if (nextPosition[instance] == queued[instance].Count)
                    nextPosition[instance] = 0;
                if ((queued[instance].Count>nextPosition[instance])&& (getPlayingMedia(instance).Location == queued[instance][nextPosition[instance]].Location))
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
            currentPosition[instance] = -1;
            queued[instance].Clear();
            queued[instance].AddRange(source.GetRange(0, source.Count));
            if (queued[instance].Count > 0)
                generateNext(instance);
            SandboxedThread.Asynchronous(delegate() { raiseMediaEvent(eFunction.playlistChanged, instance, ""); });
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

        private int getInstance(int screen)
        {
            string str;
            using (PluginSettings settings = new PluginSettings())
                str = settings.getSetting("Screen" + (screen + 1).ToString() + ".SoundCard");
            if (str == "")
                return 0;
            string[] devs;
            try
            {
                devs = ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices;
            }
            catch (NullReferenceException) { return 0; }
            return Array.FindIndex(devs, p => p.Replace("  ", " ") == str) + 1;
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
                        if (skinpath == "")
                        {
                            skinpath = "Default";
                            s.setSetting("UI.Skin", skinpath);
                        }
                    }
                    skinpath = Path.Combine(Application.StartupPath, "Skins", skinpath);
                }
                return skinpath;
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
        private OMPanel getPanelByName(string name, string panelName, int screen)
        {
            if (name == "About")
                return BuiltInComponents.AboutPanel();
            try
            {
                IBasePlugin plugin = getPluginByName(name);
                if (plugin != null)
                    return ((IHighLevel)plugin).loadPanel(panelName, screen);
                else
                    return null;
            }
            catch (NotImplementedException) { return null; }
        }
        public IntPtr UIHandle(int screen)
        {
            if ((screen < 0) || (screen >= Core.RenderingWindows.Count))
                return (IntPtr)(-1); //Out of bounds
            if (Core.RenderingWindows[screen].InvokeRequired == true)
            {
                RenderingWindow.getVal val = new RenderingWindow.getVal(Core.RenderingWindows[screen].getHandle);
                return (IntPtr)Core.RenderingWindows[screen].Invoke(val);
            }
            else
                return Core.RenderingWindows[screen].Handle;
        }

        public Int32 RenderFirst
        {
            get
            {
                return renderfirst;
            }
            set
            {
                renderfirst = value;
            }
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
        }
        public Rectangle VideoPosition
        {
            get
            {
                return videoPosition;
            }
            set
            {
                videoPosition = value;
            }
        }
        public PluginHost()
        {
            queued = new List<mediaInfo>[8];
            currentMediaPlayer = new IAVPlayer[8];
            currentTunedContent = new ITunedContent[8];
            currentPosition = new int[8];
            nextPosition = new int[8];
            for (int i = 0; i < screenCount; i++)
                history.Enqueue(i, "MainMenu", "", false);
            for (int i = 0; i < 8; i++)
                queued[i] = new List<mediaInfo>();
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
                            raiseSystemEvent(eFunction.networkConnectionsAvailable, ipAddress, "", "");
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
                        raiseSystemEvent(eFunction.connectedToInternet, "", "", "");
                    else
                        raiseSystemEvent(eFunction.disconnectedFromInternet, "", "", "");
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
            catch (Exception) { }
        }

        #region Power Events
        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (e.Reason == SessionEndReasons.Logoff)
                try
                {
                    raisePowerEvent(ePowerEvent.LogoffPending);
                }
                catch (Exception) { }
            if (e.Reason == SessionEndReasons.SystemShutdown)
                try
                {
                    raisePowerEvent(ePowerEvent.ShutdownPending);
                }
                catch (Exception) { }
        }
        public void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                try
                {
                    raisePowerEvent(ePowerEvent.SystemResumed);
                    suspending = false;
                }
                catch (Exception) { }
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                try
                {
                    if (!suspending)
                        raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                }
                catch (Exception) { }
            }
            else
            {
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                {
                    try
                    {
                        if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Low)
                        {
                            raisePowerEvent(ePowerEvent.BatteryLow);
                        }
                        else if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Critical)
                        {
                            raisePowerEvent(ePowerEvent.BatteryCritical);
                        }
                        else
                        {
                            raisePowerEvent(ePowerEvent.SystemOnBattery);
                        }
                    }
                    catch (Exception) { }
                }
                else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                {
                    try
                    {
                        raisePowerEvent(ePowerEvent.SystemPluggedIn);
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        raisePowerEvent(ePowerEvent.Unknown);
                    }
                    catch (Exception) { }
                }

            }
        }
        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (screenCount != Screen.AllScreens.Length)
            {
                if (screenCount < Screen.AllScreens.Length)
                {
                    raiseSystemEvent(eFunction.screenAdded, "", "", "");
                    execute(eFunction.restartProgram);
                }
                else
                {
                    screenCount = Screen.AllScreens.Length;
                    raiseSystemEvent(eFunction.screenRemoved, "", "", "");
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
                        RenderingWindow.closeRenderer();
                        if (hal != null)
                            hal.snd("44");
                        savePlaylists();
                        SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.closeProgram, "", "", ""); });
                    }
                    return true;
                case eFunction.restartProgram:
                    savePlaylists();
                    raiseSystemEvent(eFunction.closeProgram, "", "", "");
                    hal.close();
                    Application.Restart();
                    return true;
                case eFunction.stopListeningForSpeech:
                    raiseSystemEvent(eFunction.stopListeningForSpeech, "", "", "");
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
                    suspending = true;
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                    return true;
                case eFunction.standby:
                    hal.snd("48");
                    suspending = true;
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
                for (int i = 0; i < 8; i++)
                {
                    setPlaylist(Playlist.readPlaylistFromDB(this, "Current" + i.ToString()), i);
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
        }
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
        public int InstanceCount
        {
            get
            {
                if (instanceCount == -1)
                    try
                    {
                        instanceCount = ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices.Length;
                    }
                    catch (Exception) { return -1; }
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
                    raiseSystemEvent(eFunction.promptDialNumber, arg, "", "");
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
                case eFunction.TransitionToPanel:
                    if (int.TryParse(arg, out ret))
                        return execute(eFunction.TransitionToPanel, arg, history.CurrentItem(ret).pluginName, "");
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
                        currentMediaPlayer[ret] = null;
                        raiseMediaEvent(eFunction.unloadAVPlayer, ret, "");
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
                        currentTunedContent[ret] = null;
                        raiseMediaEvent(eFunction.unloadTunedContent, ret, "");
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
                            return sendMessage(currentTunedContent[ret].pluginName, "PluginHost", "PlayPause"); //TODO: IExtendedTuner
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
                    raiseSystemEvent(eFunction.dataUpdated, arg, "", "");
                    return true;
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg, "", "");
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
                //case eFunction.systemVolumeChanged:
                //    raiseSystemEvent(eFunction.systemVolumeChanged, arg, "0", "");
                //    return true;
                case eFunction.setSystemVolume:
                    return execute(eFunction.setSystemVolume, arg, "0");
                case eFunction.TransitionFromAny:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= Core.RenderingWindows.Count))
                            return false;
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].transitionOutEverything();
                        }
                        history.setDisabled(ret, false);
                        raiseSystemEvent(eFunction.TransitionFromAny, arg, "", "");
                        return true;
                    }
                    return false;
                case eFunction.ejectDisc:
                    if ((arg == null) || (arg == ""))
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
                    raiseSystemEvent(eFunction.navigateToAddress, arg, "", "");
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
                case eFunction.settingsChanged:
                    raiseSystemEvent(eFunction.settingsChanged, arg, "", "");
                    return true;
            }
            return false;
        }
        public bool execute(eFunction function, string arg1, string arg2)
        {
            int ret;
            switch (function)
            {
                case eFunction.TransitionToPanel:
                    return execute(eFunction.TransitionToPanel, arg1, arg2, "");
                case eFunction.TransitionFromPanel:
                    return execute(eFunction.TransitionFromPanel, arg1, arg2, "");
                case eFunction.ExecuteTransition:
                    eGlobalTransition effect;

                    if (level == eGraphicsLevel.Minimal)
                        effect = eGlobalTransition.None;
                    else
                        try
                        {
                            effect = (eGlobalTransition)Enum.Parse(typeof(eGlobalTransition), arg2);
                        }
                        catch (Exception)
                        {
                            effect = eGlobalTransition.None;
                        }
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].executeTransition(effect);
                        }
                        raiseSystemEvent(eFunction.ExecuteTransition, arg1, effect.ToString(), "");
                        return true;
                    }
                    return false;
                case eFunction.goBack:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (history.getDisabled(ret) == true)
                        {
                            raiseSystemEvent(eFunction.goBack, arg1, "", "");
                            return false;
                        }
                        if (history.Peek(ret).pluginName == null)
                            return false;
                        execute(eFunction.TransitionFromPanel, arg1, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                        raiseSystemEvent(eFunction.TransitionFromPanel, arg1, history.CurrentItem(ret).pluginName, history.CurrentItem(ret).panelName);
                        while((history.Count(ret)>0)&&(history.Peek(ret).Equals(history.CurrentItem(ret))))
                            history.Dequeue(ret);
                        //This part is done manually to prevent adding it to the history
                        OMPanel k = getPanelByName(history.Peek(ret).pluginName, history.Peek(ret).panelName, ret);
                        if (k == null)
                            return false;
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].transitionInPanel(k);
                        }
                        raiseSystemEvent(eFunction.TransitionToPanel, arg1, history.Peek(ret).pluginName, history.Peek(ret).panelName);
                        history.Dequeue(ret);
                        execute(eFunction.ExecuteTransition, arg1, arg2);
                        return true;
                    }
                    return false;
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2, "");
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
                        int ret2;
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
                            return currentMediaPlayer[ret].setPosition(ret, float.Parse(arg2));
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
                            return currentMediaPlayer[ret].setPlaybackSpeed(ret, float.Parse(arg2));
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
                            return currentTunedContent[ret].setVolume(ret, int.Parse(arg2));
                        }
                        else
                        {
                            raiseMediaEvent(eFunction.setPlayerVolume, ret, arg2);
                            try
                            {
                                return currentMediaPlayer[ret].setVolume(ret, int.Parse(arg2));
                            }
                            catch (Exception) { return false; }
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
                    SandboxedThread.Asynchronous(delegate() { raiseSystemEvent(eFunction.backgroundOperationStatus, arg1, arg2, ""); });
                    return true;
                case eFunction.sendKeyPress:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((arg2 == null) || (arg2 == ""))
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
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this, arg1,arg2);
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
                        raiseSystemEvent(eFunction.systemVolumeChanged, arg1, arg2, "");
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
                        if (int.TryParse(arg2, out ret) == false)
                            return false;
                        hal.snd("40|" + arg1 + "|" + arg2);
                        return true;
                    }
                    return false;
                case eFunction.setSystemBalance:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < 0) || (ret >= instanceCount))
                            return false;
                        if (int.TryParse(arg2, out ret) == false)
                            return false;
                        hal.snd("42|" + arg1 + "|" + arg2);
                        return true;
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
                    if ((currentTunedContent[ret]!=null)&&(currentTunedContent[ret].tuneTo(ret, arg2) == true))
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
                        lock (Core.RenderingWindows[ret])
                        {
                            Core.RenderingWindows[ret].transitionInPanel(panel);
                            raiseSystemEvent(eFunction.TransitionToPanel, arg1, arg2, arg3);
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
                            Core.RenderingWindows[ret].transitionOutPanel(panel);
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
                        Core.RenderingWindows[i].Invoke(Core.RenderingWindows[i].identify);
                if (message == "Redraw")
                    Core.RenderingWindows[0].Invoke(Core.RenderingWindows[0].redraw);
                if (message == "ToggleCursor")
                    for (int i = 0; i < screenCount; i++)
                        Core.RenderingWindows[i].hideCursor();
                return true;
            }
            else if (to == "OMHal")
            {
                hal.snd("-1|" + from + "|" + message);
                return true;
            }
            try
            {
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
                SandboxedThread.handle(data as Exception);
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
            if (OnSystemEvent != null)
                OnSystemEvent(e, arg1, arg2, arg3);
        }
        public void raisePowerEvent(ePowerEvent e)
        {
            if (OnPowerChange != null)
                OnPowerChange(e);
        }
        private void raiseNavigationEvent(eNavigationEvent type, string arg)
        {
            try
            {
                OnNavigationEvent(type, arg);
            }
            catch (Exception) { }
        }
        public void raiseWirelessEvent(eWirelessEvent type, string arg)
        {
            if (OnWirelessEvent != null)
                OnWirelessEvent(type, arg);
        }
        private void raiseMediaEvent(eFunction e, int instance, string arg)
        {
            if (OnMediaEvent != null)
                OnMediaEvent(e, instance, arg);
            if (e == eFunction.nextMedia)
                if (!execute(eFunction.nextMedia, instance.ToString()))
                    raiseMediaEvent(eFunction.Stop, instance, "");
        }
        public bool raiseKeyPressEvent(eKeypressType type, KeyEventArgs arg)
        {
            try
            {
                if (OnKeyPress != null)
                    return OnKeyPress(type, arg);
            }
            catch (Exception) { }
            return false;
        }

        public event SystemEvent OnSystemEvent;
        public event MediaEvent OnMediaEvent;
        public event PowerEvent OnPowerChange;
        public event StorageEvent OnStorageEvent;
        public event NavigationEvent OnNavigationEvent;
        public event KeyboardEvent OnKeyPress;
        public event WirelessEvent OnWirelessEvent;

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
                        im.image = Image.FromFile(Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png"));
                        im.name = imageName;
                        imageCache.Add(im);
                        return im;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        try
                        {
                            im.image = Image.FromFile(Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".gif"));
                            im.name = imageName;
                            imageCache.Add(im);
                            return im;
                        }
                        catch (System.IO.FileNotFoundException)
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
                    imageItem item = new imageItem(Image.FromFile(Path.Combine(SkinPath, imageName.Replace('|', System.IO.Path.DirectorySeparatorChar) + ".png")));
                    item.name = imageName;
                    return item;
                }
                catch (Exception)
                {
                    return new imageItem();
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
        public void getData(eGetData dataType, string name, out object data)
        {
            IBasePlugin plugin;
            data = null;
            switch (dataType)
            {
                case eGetData.GetMap:
                    plugin = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (plugin == null)
                        return;
                    data = ((INavigation)plugin).getMap;
                    return;
                case eGetData.GetMediaDatabase:
                    plugin = Core.pluginCollection.Find(t => t.pluginName == name);
                    if (plugin == null)
                        return;
                    data = ((IMediaDatabase)plugin).getNew();
                    break;
                case eGetData.GetPlugins:
                    if (name == "")
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
                    if (name != "")
                    {
                        plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (plugin == null)
                            return;
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
                    data = ((IRawHardware)plugin).firmwareVersion;
                    return;
                case eGetData.GetDeviceInfo:
                    plugin = Core.pluginCollection.Find(p => p.pluginName == name);
                    if (plugin == null)
                        return;
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
                    try
                    {
                        data = ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices;
                    }
                    catch (Exception) { }
                    return;
                case eGetData.GetAvailableSkins:
                    string[] ret = Directory.GetDirectories(Path.Combine(Application.StartupPath, "Skins"));
                    for (int i = 0; i < ret.Length; i++)
                        ret[i] = ret[i].Replace(Path.Combine(Application.StartupPath, "Skins", ""), "");
                    data = ret;
                    return;
            }
        }
        public void getData(eGetData dataType, string name, string param, out object data)
        {
            data = null;
            int ret;
            IBasePlugin plugin;
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
                                data = int.Parse(hal.volume[1]);
                                hal.volume = null;
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
                        if (name == "")
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
                    plugin = Core.pluginCollection.Find(s => s.pluginName == name);
                    if (plugin == null)
                        return;
                    data = ((INavigation)plugin).Destination;
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
            }
        }
    }
}
