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
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Plugin;

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
        public HalInterface hal;
        private bool vehicleInMotion = false;
        private eGraphicsLevel level;
        private Rectangle videoPosition;

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
        private OMPanel getPanelByName(string name,int screen)
        {
            try
            {
                return ((IHighLevel)getPluginByName(name)).loadPanel("",screen);
            }
            catch (Exception) {
                if (name == "About")
                    return BuiltInComponents.AboutPanel();
                return null;
            }
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

        public List<mediaInfo> getPlaylist(int instance)
        {
                return queued[instance];
        }
        public bool setPlaylist(List<mediaInfo> source,int instance)
        {
            currentPosition[instance] = -1;
            queued[instance].Clear();
            queued[instance].AddRange(source.GetRange(0, source.Count));
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
            return instance[screen]-1;
        }

        private int getInstance(int screen)
        {
            string str;
            using(PluginSettings settings=new PluginSettings())
                str=settings.getSetting("Screen"+(screen+1).ToString()+".SoundCard");
            if (str=="")
                return 0;
            string[] devs;
            try
            {
                devs = ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices;
            }
            catch (NullReferenceException) { return 0; }
            return Array.FindIndex(devs, p => p.Replace("  "," ") == str)+1;
        }
        public string SkinPath
        {
            get
            {
                if (skinpath==null)
                    skinpath=Path.Combine(Application.StartupPath,"Skins","Default");
                return skinpath;
            }
        }
        public string DataPath
        {
            get
            {
                if (datapath == null)
                    datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"openMobile");
                return datapath;
            }
        }
        public string PluginPath
        {
            get
            {
                if (pluginpath==null)
                    pluginpath=Path.Combine(Application.StartupPath, "Plugins");
                return pluginpath;
            }
        }
        private OMPanel getSettingsPanelByName(string name,int screen)
        {
            try{
                return ((IHighLevel)getPluginByName(name)).loadSettings("",screen);
            }
            catch (Exception) { return null; }
        }
        private OMPanel getPanelByName(string name,string panelName,int screen)
        {
            if (name == "About")
                return BuiltInComponents.AboutPanel();
            try
            {
                return ((IHighLevel)getPluginByName(name)).loadPanel(panelName,screen);
            }
            catch (Exception) { return null; }
        }
        private OMPanel getSettingsPanelByName(string name,string panelName,int screen)
        {
            try
            {
                return ((IHighLevel)getPluginByName(name)).loadSettings(panelName,screen);
            }
            catch (Exception) { return null; }
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
                if (videoPosition == null)
                    videoPosition = new Rectangle();
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
            for (int i = 0; i < screenCount; i++)
                history.Enqueue(i,false, "MainMenu",false);
            for (int i = 0; i < 8; i++)
                queued[i] = new List<mediaInfo>();
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
                            raiseSystemEvent(eFunction.connectedToInternet, ipAddress, "","");
                        }
            }
        }

        public void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable != internetAccess)
            {
                internetAccess = e.IsAvailable;
                if (internetAccess == true)
                    raiseSystemEvent(eFunction.networkConnectionsAvailable,"","","");
                else
                    raiseSystemEvent(eFunction.disconnectedFromInternet,"","","");
            }
        }

        public void RaiseStorageEvent(eMediaType type, string arg)
        {
            try
            {
                OnStorageEvent(type, arg);
                if (type == eMediaType.NotSet)
                    StorageAnalyzer.AnalyzeAsync(arg);
            }
            catch (Exception) { }
        }

        #region Power Events
        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (e.Reason == SessionEndReasons.Logoff)
                try
                {
                    OnPowerChange(ePowerEvent.LogoffPending);
                }
                catch (Exception) { }
            if (e.Reason == SessionEndReasons.SystemShutdown)
                try
                {
                    OnPowerChange(ePowerEvent.ShutdownPending);
                }
                catch (Exception) { }
        }

        public void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                try
                {
                    OnPowerChange(ePowerEvent.SystemResumed);
                }
                catch (Exception) { }
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                try
                {
                    OnPowerChange(ePowerEvent.SleepOrHibernatePending);
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
                            if (OnPowerChange != null)
                                OnPowerChange(ePowerEvent.BatteryLow);
                        }
                        else if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Critical)
                        {
                            if (OnPowerChange != null)
                                OnPowerChange(ePowerEvent.BatteryCritical);
                        }
                        else
                        {
                            if (OnPowerChange != null)
                                OnPowerChange(ePowerEvent.SystemOnBattery);
                        }
                    }
                    catch (Exception) { }
                }
                else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                {
                    try
                    {
                        if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Charging)
                        {
                            if (OnPowerChange != null)
                                OnPowerChange(ePowerEvent.SystemPluggedIn);
                        }
                        else if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.High)
                        {
                            if (OnPowerChange != null)
                                OnPowerChange(ePowerEvent.SystemPluggedIn);
                        }
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        OnPowerChange(ePowerEvent.Unknown);
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
                    lock (Core.RenderingWindows[0])
                    {
                        RenderingWindow.closeRenderer();
                        raiseSystemEvent(eFunction.closeProgram,"","","");
                        if (hal!=null)
                            hal.snd("45");
                    }
                    return true;
                case eFunction.restartProgram:
                    execute(eFunction.closeProgram);
                    Application.Restart();
                    return true;
                case eFunction.stopListeningForSpeech:
                    raiseSystemEvent(eFunction.stopListeningForSpeech, "", "", "");
                    return true;
                case eFunction.listenForSpeech:
                    string s;
                    using(PluginSettings set=new PluginSettings())
                        s=set.getSetting("Default.Speech");
                    if (s.Length>0)
                    {
                        IBasePlugin b=Core.pluginCollection.Find(q => q.pluginName == s);
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
                    return Application.SetSuspendState(PowerState.Hibernate, false, false);
                case eFunction.standby:
                    return Application.SetSuspendState(PowerState.Suspend, false, false);
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this);
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this);
                case eFunction.settingsChanged:
                    for (int i = 0; i < screenCount; i++)
                        instance[i] = getInstance(i);
                    return true;
                case eFunction.refreshData:
                    bool result=true;
                    foreach (IBasePlugin p in Core.pluginCollection.FindAll(i => typeof(IDataProvider).IsInstanceOfType(i)))
                    {
                        result = result & execute(eFunction.refreshData, p.pluginName);
                    }
                    return result;
            }
            return false;
        }
        
        
        private class historyCollection
        {
            public struct historyItem
            {
                public bool settings;
                public bool forgettable;
                public string pluginName;
                public string panelName;
                public historyItem(bool settings, string pluginName, string panelName,bool forgetMe)
                {
                    this.settings = settings;
                    this.pluginName = pluginName;
                    this.panelName = panelName;
                    this.forgettable = forgetMe;
                }
                public historyItem(bool settings, string pluginName,bool forgetMe)
                {
                    this.settings = settings;
                    this.pluginName = pluginName;
                    this.panelName = "";
                    this.forgettable = forgetMe;
                }
            }
            bool[] disabled;
            Stack<historyItem>[] items;

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
                disabled=new bool[count];
                for (int i = 0; i < count; i++)
                    items[i] = new Stack<historyItem>(10);
            }
            public void Enqueue(int screen, bool settings, string name, bool forgetMe)
            {
                Enqueue(screen, settings, name, "", forgetMe);
            }
            
            public void Enqueue(int screen, bool settings, string pluginName,string panelName, bool forgetMe)
            {
                if (items[screen].Count == 10)
                {
                    Stack<historyItem> tmp = new Stack<historyItem>(10);
                    for (int i = 0; i < 6; i++)
                    {
                        tmp.Push(items[screen].Pop());
                    }
                }
                items[screen].Push(new historyItem(settings, pluginName,panelName,forgetMe));
            }
            public historyItem Peek(int screen)
            {
                return items[screen].Peek();
            }
            public historyItem DoublePeek(int screen)
            {
                if (items[screen].Count <= 1)
                    return new historyItem();
                historyItem[] ret=new historyItem[1];
                return items[screen].ToArray()[1];
            }
            public historyItem Dequeue(int screen)
            {
                return items[screen].Pop();
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
                if (instanceCount==-1)
                    try
                    {
                        instanceCount = ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices.Length;
                    }
                    catch (Exception) { return -1; }
                 return instanceCount;
            }
        }

        public bool execute(eFunction function,string arg)
        {
            int ret;
            switch (function)
            {
                case eFunction.promptDialNumber:
                    raiseSystemEvent(eFunction.promptDialNumber, arg, "", "");
                    return true;
                case eFunction.showNavPanel:
                    IBasePlugin z = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (z == null)
                        return false;
                    return ((INavigation)z).switchTo(arg);
                case eFunction.navigateToPOI:
                    IBasePlugin n = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (n == null)
                        return false;
                    return ((INavigation)n).findPOI(arg);
                case eFunction.ExecuteTransition:
                    return execute(eFunction.ExecuteTransition,arg,"Crossfade");
                case eFunction.TransitionToPanel:
                    int tp;
                    if (int.TryParse(arg, out tp))
                        return execute(eFunction.TransitionToPanel, arg, history.Peek(tp).pluginName, "");
                    return false;
                case eFunction.TransitionFromPanel:
                    int tf;
                    if (int.TryParse(arg, out tf))
                    {
                        OMPanel k;
                        if (history.Peek(tf).settings==true)
                            k = getSettingsPanelByName(history.Peek(tf).pluginName, tf);
                        else
                            k= getPanelByName(history.Peek(tf).pluginName, tf);
                        if (k == null)
                            return false;
                        lock (Core.RenderingWindows[tf])
                        {
                            Core.RenderingWindows[tf].transitionOutPanel(k);
                        }
                        raiseSystemEvent(eFunction.TransitionFromPanel, arg, history.Peek(tf).pluginName, "");
                        return true;
                    }
                    return false;
                case eFunction.TransitionFromSettings:
                    int tfs;
                    if (int.TryParse(arg, out tfs) == true)
                        return execute(eFunction.TransitionFromSettings, arg, history.Peek(tfs).pluginName, history.Peek(tfs).panelName);
                    return false;
                case eFunction.goBack:
                    return execute(eFunction.goBack, arg, "SlideRight");
                case eFunction.unloadAVPlayer:
                    if (int.TryParse(arg,out ret)==true)
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
                        raiseSystemEvent(eFunction.unloadAVPlayer, arg, "", "");
                        return true;
                    }
                    return false;
                case eFunction.unloadTunedContent:
                    if (int.TryParse(arg,out ret)==true)
                    {
                        currentTunedContent[ret].OnMediaEvent -= raiseMediaEvent;
                        currentTunedContent[ret] = null;
                        raiseSystemEvent(eFunction.unloadTunedContent, arg, "", "");
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
                    return currentMediaPlayer[ret].SetVideoVisible(ret,true);
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
                        return false;
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
                        lock (currentPosition)
                        {
                            if ((random != null) && (random[ret] == true))
                            {
                                int result;
                                do
                                {
                                    result = Framework.Math.Calculation.RandomNumber(0, queued[ret].Count);
                                }
                                while (currentPosition[ret] == result);
                                currentPosition[ret] = result;
                            }
                            else
                                currentPosition[ret]++;
                            if (currentPosition[ret] == queued[ret].Count)
                                currentPosition[ret] = 0;
                            if (getPlayingMedia(ret).Location == queued[ret][currentPosition[ret]].Location)
                            {
                                currentPosition[ret]++;
                                if (currentPosition[ret] == queued[ret].Count)
                                    currentPosition[ret] = 0;
                            }
                        }
                        return execute(eFunction.Play, arg, queued[ret][currentPosition[ret]].Location);
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
                        return execute(eFunction.Play, arg, queued[ret][currentPosition[ret]].Location);
                    }
                    return false;
                case eFunction.scanBackward:
                    if (int.TryParse(arg, out ret) == true)
                    { 
                        if (currentTunedContent[ret] == null)
                            return false;
                        currentTunedContent[ret].scanReverse(ret);
                        return true;
                    }
                    return false;
                case eFunction.scanForward:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        currentTunedContent[ret].scanForward(ret);
                        return true;
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
                case eFunction.powerOnDevice:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].setPowerState(ret, true);
                    }
                    return false;
                case eFunction.powerOffDevice:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].setPowerState(ret, false);
                    }
                    return false;
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg)).refreshData();
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg,"","");
                    return true;
                case eFunction.resetDevice:
                    throw new NotImplementedException();
                case eFunction.loadSpeechContext:
                    string s;
                    using (PluginSettings set = new PluginSettings())
                        s = set.getSetting("Default.Speech");
                    if ((s!=null)&&(s.Length > 0))
                    {
                        IBasePlugin b = Core.pluginCollection.Find(q => q.pluginName == s);
                        if (b == null)
                            return false;
                        return ((ISpeech)b).loadContext(arg);
                    }
                    return false;
                case eFunction.Speak:
                    string str;
                    using (PluginSettings set = new PluginSettings())
                        str = set.getSetting("Default.Speech");
                    if ((str!=null)&&(str.Length > 0))
                    {
                        IBasePlugin b = Core.pluginCollection.Find(q => q.pluginName == str);
                        if (b == null)
                            return false;
                        return ((ISpeech)b).speak(arg);
                    }
                    return false;
                case eFunction.connectToInternet:
                    return Net.Connections.connect(this,arg);
                case eFunction.disconnectFromInternet:
                    return Net.Connections.disconnect(this,arg);
                case eFunction.systemVolumeChanged:
                    raiseSystemEvent(eFunction.systemVolumeChanged, arg, "0", "");
                    return true;
                case eFunction.setSystemVolume:
                    if (int.TryParse(arg, out ret) == true)
                    {
                        if ((ret < -2) || (ret > 100))
                            return false;
                        hal.snd("34|"+ret.ToString()+"|0");
                        return true;
                    }
                    return false;
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
                    if ((arg==null)||(arg==""))
                        return false;
                    hal.snd("35|" + arg);
                    return true;
                case eFunction.navigateToAddress:
                    if (arg.Length <= 1)
                        return false;
                    IBasePlugin y = Core.pluginCollection.Find(a => typeof(INavigation).IsInstanceOfType(a));
                    if (y!=null)
                    {
                        Location adr;
                        if (Location.TryParse(arg, out adr) == true)
                            ((INavigation)y).navigateTo(adr);
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
            }
            return false;
        }
        public bool execute(eFunction function,string arg1, string arg2)
        {
            int ret;
            switch (function)
            {
                case eFunction.TransitionToPanel:
                    return execute(eFunction.TransitionToPanel, arg1, arg2, "");
                case eFunction.TransitionFromPanel:
                    return execute(eFunction.TransitionFromPanel, arg1, arg2, "");
                case eFunction.TransitionToSettings:
                    return execute(eFunction.TransitionToSettings, arg1, arg2, "");
                case eFunction.TransitionFromSettings:
                    return execute(eFunction.TransitionFromSettings, arg1, arg2, "");
                case eFunction.ExecuteTransition:
                    eGlobalTransition effect;
                    try
                    {
                        if (level == eGraphicsLevel.Minimal)
                            effect = eGlobalTransition.None;
                        else
                            effect = (eGlobalTransition)Enum.Parse(typeof(eGlobalTransition), arg2);
                    }
                    catch (Exception)
                    {
                        effect = eGlobalTransition.None;
                    }
                    int ex;
                    if (int.TryParse(arg1, out ex) == true)
                    {
                        lock (Core.RenderingWindows[ex])
                        {
                            Core.RenderingWindows[ex].executeTransition(effect);
                        }
                        raiseSystemEvent(eFunction.ExecuteTransition, arg1, effect.ToString(), "");
                        return true;
                    }
                    return false;
                case eFunction.goBack:
                    int gb;
                    if (int.TryParse(arg1, out gb) == true)
                    {
                        if (history.getDisabled(gb) == true)
                        {
                            raiseSystemEvent(eFunction.goBack, arg1, "", "");
                            return false;
                        }
                        if (history.DoublePeek(gb).pluginName == null)
                            return false;
                        if (history.Peek(gb).settings == true)
                        {
                            execute(eFunction.TransitionFromSettings, arg1, history.Peek(gb).pluginName, history.Peek(gb).panelName);
                            raiseSystemEvent(eFunction.TransitionFromSettings, arg1, history.Peek(gb).pluginName, history.Peek(gb).panelName);
                        }
                        else
                        {
                            execute(eFunction.TransitionFromPanel, arg1, history.Peek(gb).pluginName, history.Peek(gb).panelName);
                            raiseSystemEvent(eFunction.TransitionFromPanel, arg1, history.Peek(gb).pluginName, history.Peek(gb).panelName);
                        }
                        historyCollection.historyItem tmp = history.DoublePeek(gb);
                        if (tmp.forgettable == true)
                        {
                            history.Dequeue(gb);
                            if ((tmp.pluginName == history.Peek(gb).pluginName) && (tmp.panelName == history.Peek(gb).panelName))
                                history.Dequeue(gb);
                            return execute(eFunction.goBack, arg1, arg2);
                        }
                        //This part is done manually to prevent adding it to the history
                        if (history.DoublePeek(gb).settings == true)
                        {
                            OMPanel k = getSettingsPanelByName(history.DoublePeek(gb).pluginName, history.DoublePeek(gb).panelName, gb);
                            if (k == null)
                                return false;
                            lock (Core.RenderingWindows[gb])
                            {
                                Core.RenderingWindows[gb].transitionInPanel(k);
                            }
                            raiseSystemEvent(eFunction.TransitionToSettings, arg1, history.DoublePeek(gb).pluginName, history.DoublePeek(gb).panelName);
                        }
                        else
                        {
                            OMPanel k = getPanelByName(history.DoublePeek(gb).pluginName, history.DoublePeek(gb).panelName, gb);
                            if (k == null)
                                return false;
                            lock (Core.RenderingWindows[gb])
                            {
                                Core.RenderingWindows[gb].transitionInPanel(k);
                            }
                            raiseSystemEvent(eFunction.TransitionToPanel, arg1, history.DoublePeek(gb).pluginName, history.DoublePeek(gb).panelName);
                        }
                        history.Dequeue(gb);
                        execute(eFunction.ExecuteTransition, arg1,arg2);
                        return true;
                    }
                    return false;
                case eFunction.userInputReady:
                    raiseSystemEvent(eFunction.userInputReady, arg1, arg2,"");
                    return true;
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2);
                case eFunction.loadAVPlayer:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentMediaPlayer[ret] != null)
                            return false;
                        IAVPlayer player = (IAVPlayer)Core.pluginCollection.FindAll(i => typeof(IAVPlayer).IsInstanceOfType(i)).Find(i => i.pluginName == arg2);
                        if (player == null)
                            return false;
                        //Hook it if its not already hooked
                        if (Array.Exists<IAVPlayer>(currentMediaPlayer, a => a == player) == false)
                            player.OnMediaEvent += raiseMediaEvent;
                        currentMediaPlayer[ret] = player;
                        raiseSystemEvent(eFunction.loadAVPlayer, arg1, arg2, "");
                        return true;
                    }
                    return false;
                case eFunction.loadTunedContent:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if (currentTunedContent[ret] != null)
                            return false;
                        using (ITunedContent player = (ITunedContent)Core.pluginCollection.FindAll(i => i.GetType() == typeof(ITunedContent)).Find(i => i.pluginName == arg2))
                        {
                            if (player == null)
                                return false;
                            //Only hook it once
                            if (Array.Exists<ITunedContent>(currentTunedContent, a => a == player) == false)
                                player.OnMediaEvent += raiseMediaEvent;
                            currentTunedContent[ret] = player;
                            raiseSystemEvent(eFunction.loadTunedContent, arg1, arg2, "");
                            return true;
                        }
                    }
                    return false;
                case eFunction.setPlaylistPosition:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        int ret2;
                        if (int.TryParse(arg2, out ret2) == true)
                        {
                            if (queued[ret].Count > ret2)
                            {
                                currentPosition[ret] = ret2;
                                return true;
                            }
                        }
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
                            {
                                return false;
                            }
                            raiseMediaEvent(eFunction.setPlayerVolume, ret, arg2);
                            return currentTunedContent[ret].setVolume(ret, int.Parse(arg2));
                        }
                        else
                        {
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
                        if (currentTunedContent[ret] == null)
                            return false;
                        return currentTunedContent[ret].tuneTo(ret, arg2);
                    }
                    return false;
                case eFunction.backgroundOperationStatus:
                    raiseSystemEvent(eFunction.backgroundOperationStatus, arg1, arg2, "");
                    return true;
                case eFunction.sendKeyPress:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((arg2 == null)||(arg2==""))
                            return false;
                        return (InputRouter.SendKeyDown(ret,arg2)&&InputRouter.SendKeyUp(ret,arg2));
                    }
                    return false;
                case eFunction.gesture:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        raiseSystemEvent(eFunction.gesture, arg1, arg2, history.Peek(ret).pluginName);
                    }
                    return false;
                case eFunction.systemVolumeChanged:
                    if (int.TryParse(arg2, out ret) == true)
                    {
                        raiseSystemEvent(eFunction.systemVolumeChanged, arg1, arg2, "");
                        return true;
                    }
                    return false;
                case eFunction.setSystemVolume:
                    if (int.TryParse(arg1, out ret) == true)
                    {
                        if ((ret < -2) || (ret > 100))
                            return false;
                        if (int.TryParse(arg2, out ret) == false)
                            return false;
                        hal.snd("34|" + ret.ToString() + "|"+arg2);
                        return true;
                    }
                    return false;
            }
            return false;
        }

        private bool findAlternatePlayer(int ret, string arg2, eMediaType type)
        {
            IAVPlayer tmp = currentMediaPlayer[ret];
            List<IBasePlugin> plugins=Core.pluginCollection.FindAll(p => typeof(IAVPlayer).IsInstanceOfType(p));
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] != tmp)
                {
                    execute(eFunction.unloadAVPlayer, ret.ToString());
                    execute(eFunction.loadAVPlayer, ret.ToString(), plugins[i].pluginName);
                    if (currentMediaPlayer[ret].play(ret, arg2,type) == true)
                        return true;
                }
            }
            if (tmp==null)
                execute(eFunction.unloadAVPlayer,ret.ToString());
            else
                execute(eFunction.loadAVPlayer, ret.ToString(), tmp.pluginName);
            return false;
        }
        public bool execute(eFunction function, string arg1, string arg2, string arg3)
        {
            switch (function)
            {
                case eFunction.refreshData:
                    return ((IDataProvider)getPluginByName(arg1)).refreshData(arg2,arg3);
                case eFunction.TransitionToSettings:
                    int ts;
                    if (int.TryParse(arg1, out ts) == true)
                    {
                        OMPanel b = getSettingsPanelByName(arg2, arg3, ts);
                        if (b == null)
                            return false;
                        lock (Core.RenderingWindows[ts])
                        {
                            Core.RenderingWindows[ts].transitionInPanel(b);
                            raiseSystemEvent(eFunction.TransitionToSettings, arg1, arg2, arg3);
                            history.Enqueue(ts,true, arg2, arg3,b.Forgotten);
                        }
                        return true;
                    } 
                    return false;
                case eFunction.TransitionFromSettings:
                    int tf;
                    if (int.TryParse(arg1, out tf) == true)
                    {
                        lock (Core.RenderingWindows[tf])
                        {
                            Core.RenderingWindows[tf].transitionOutPanel(getSettingsPanelByName(arg2, arg3, tf));
                        }
                        raiseSystemEvent(eFunction.TransitionFromSettings, arg1, arg2, arg3);
                        return true;
                    }
                    return false;
                case eFunction.TransitionToPanel:
                    int tp;
                    if (int.TryParse(arg1, out tp) == true)
                    {
                        OMPanel q = getPanelByName(arg2, arg3, tp);
                        if (q == null)
                            return false;
                        lock (Core.RenderingWindows[tp])
                        {
                            Core.RenderingWindows[tp].transitionInPanel(q);
                            raiseSystemEvent(eFunction.TransitionToPanel,arg1, arg2,arg3);
                            history.Enqueue(tp,false, arg2, arg3,q.Forgotten);
                        }
                        return true;
                    }
                    return false;
                case eFunction.TransitionFromPanel:
                    int tfp;
                    if (int.TryParse(arg1, out tfp) == true)
                    {
                        OMPanel w = getPanelByName(arg2, arg3, tfp);
                        if (w == null)
                            return false;
                        lock(Core.RenderingWindows[tfp])
                        {
                            Core.RenderingWindows[tfp].transitionOutPanel(w);
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
        
        public bool executeByType(Type type,eFunction function)
        {
                bool result=false;
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
        public bool executeByType(Type type,eFunction function,string arg)
        {
                bool result=false;
                switch (function)
                {
                    case eFunction.refreshData:

                        foreach (IBasePlugin p in Core.pluginCollection.FindAll(i => typeof(IDataProvider).IsInstanceOfType(i)))
                        {
                            result = result & execute(eFunction.refreshData, p.pluginName,arg);
                        }
                        return result;
                    default:
                        return false;
                }
        }
        public bool executeByType(Type type, eFunction function, string arg1, string arg2)
        {
                bool result=false;
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
        public bool sendMessage(string to,string from, string message)
        {
            if (to == "RenderingWindow")
            {
                if (message == "Identify")
                    for (int i = 0; i < screenCount; i++)
                        Core.RenderingWindows[i].Invoke(Core.RenderingWindows[i].identify);
                if (message=="Redraw")
                    Core.RenderingWindows[0].Invoke(Core.RenderingWindows[0].redraw);
                if (message == "ToggleCursor")
                    for (int i = 0; i < screenCount; i++)
                        Core.RenderingWindows[i].hideCursor();
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
        public bool sendMessage<T>(string to, string from, string message,ref T data)
        {
            try
            {
                IBasePlugin plugin = Core.pluginCollection.Find(i => i.pluginName == to);
                {
                    if (plugin == null)
                        return false;
                    return plugin.incomingMessage(message, from,ref data);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void raiseSystemEvent(eFunction e,string arg1,string arg2,string arg3)
        {
            if (OnSystemEvent!=null)
                OnSystemEvent(e,arg1,arg2,arg3);
        }
        private void raiseNavigationEvent(eNavigationEvent type,string arg)
        {
             try
             {
                OnNavigationEvent(type, arg);
             }catch(Exception){}
        }
        private void raiseWirelessEvent(eWirelessEvent type, string arg)
        {
            if (OnWirelessEvent != null)
                OnWirelessEvent(type, arg);
        }
        private void raiseMediaEvent(eFunction e,int instance,string arg)
        {
            if (OnMediaEvent!=null)
                OnMediaEvent(e, instance,arg);
            if (e == eFunction.nextMedia)
                execute(eFunction.nextMedia, instance.ToString());
        }
        public bool raiseKeyPressEvent(keypressType type,KeyEventArgs arg)
        {
            try
            {
                if (OnKeyPress!=null)
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
        public imageItem getSkinImage(string imageName,bool noCache)
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
                        im.image = Image.FromFile(Path.Combine(SkinPath, imageName.Replace('|',System.IO.Path.DirectorySeparatorChar) + ".png"));
                        im.name = imageName;
                        imageCache.Add(im);
                        return im;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        return new imageItem("MISSING");
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
        public bool getRandom(int screen)
        {
            if (random == null)
                random = new bool[screenCount];
            return random[screen];
        }
        public void setRandom(int screen, bool set)
        {
            if (random == null)
                random = new bool[screenCount];
            random[screen]=set;
        }
        public mediaInfo getPlayingMedia(int instance)
        {
            if (currentMediaPlayer[instance] != null)
                return currentMediaPlayer[instance].getMediaInfo(instance);
            if (currentTunedContent[instance] != null)
                return currentTunedContent[instance].getMediaInfo(instance);
            return new mediaInfo();
        }
        public void getData(eGetData dataType, string name, out object data)
        {
            data = null;
            switch (dataType)
            {
                case eGetData.GetMap:
                    IBasePlugin z = Core.pluginCollection.Find(f => typeof(INavigation).IsInstanceOfType(f));
                    if (z == null)
                        return;
                    data = ((INavigation)z).getMap;
                    return;
                case eGetData.GetMediaDatabase:
                    IBasePlugin y = Core.pluginCollection.Find(t => t.pluginName == name);
                    if (y == null)
                        return;
                    data = ((IMediaDatabase)y).getNew();
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
                    if (name.Length != 0)
                    {
                        IBasePlugin b = Core.pluginCollection.Find(p => p.pluginName == name);
                        if (b == null)
                            return;
                        data=((IDataProvider)b).updaterStatus();
                    }
                    break;
                case eGetData.GetSystemVolume:
                    hal.snd("3|0");
                    bool res = true;
                    while (res == true)
                    {
                        Thread.Sleep(5);
                        res = (hal.volume == null);
                        if (res == false)
                        {
                            if (hal.volume[0] == "0")
                            {
                                data = hal.volume[1];
                                hal.volume = null;
                            }
                        }
                    }
                    return;
                case eGetData.GetFirmwareInfo:
                    IBasePlugin d = Core.pluginCollection.Find(p => p.pluginName == name);
                    if (d == null)
                        return;
                    data = ((IRawHardware)d).firmwareVersion;
                    return;
                case eGetData.GetDeviceInfo:
                    IBasePlugin e = Core.pluginCollection.Find(p => p.pluginName == name);
                    if (e == null)
                        return;
                    data = ((IRawHardware)e).deviceInfo;
                    return;
                case eGetData.GetAvailableNetworks:
                    List<connectionInfo> cInf = new List<connectionInfo>();
                    foreach (IBasePlugin g in Core.pluginCollection.FindAll(p => p.GetType() == typeof(INetwork)))
                    {
                        foreach (connectionInfo c in ((INetwork)g).getAvailableNetworks())
                            cInf.Add(c);
                    }
                    data = cInf;
                    return;
                case eGetData.GetAudioDevices:
                    try
                    {
                        data= ((IAVPlayer)Core.pluginCollection.Find(p => typeof(IAVPlayer).IsInstanceOfType(p) == true)).OutputDevices;
                    }
                    catch (Exception) { data = null; }
                    return;
            }
        }
        public void getData(eGetData dataType, string name, string param, out object data)
        {
            data = null;
            int ret;
            switch (dataType)
            {
                case eGetData.GetMediaPosition:
                    if (int.TryParse(param, out ret) == true)
                    {
                        if (name == "")
                        {
                            if (currentMediaPlayer[ret] == null)
                                return;
                            else
                                data = currentMediaPlayer[ret].getCurrentPosition(ret);
                        }
                    }
                    return;
                case eGetData.GetScaleFactors:
                    if (int.TryParse(param,out ret) == true)
                    {
                        if ((ret >= 0) && (ret < ScreenCount))
                            data = Core.RenderingWindows[ret].ScaleFactors;
                    }
                    return;
                case eGetData.GetPlaybackSpeed:
                    if (int.TryParse(param, out ret) == true)
                    {
                        if (name == "")
                        {
                            if (currentMediaPlayer[ret] == null)
                                return;
                            else
                                data = currentMediaPlayer[ret].getPlaybackSpeed(ret);
                        }
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
                            IBasePlugin p = Core.pluginCollection.Find(s => s.pluginName == name);
                            if (p == null)
                                return;
                            else if (typeof(IAVPlayer).IsInstanceOfType(p))
                                data = ((IAVPlayer)p).getPlayerStatus(ret);
                            else if (typeof(ITunedContent).IsInstanceOfType(p))
                                data = ((ITunedContent)p).getStationInfo(ret);
                        }
                    }
                    return;
                case eGetData.GetTunedContentInfo:
                    int q;
                    if (int.TryParse(param, out q) == true)
                    {
                        if (currentTunedContent[q] != null)
                            data = currentTunedContent[q].getStationInfo(q);
                    }
                    return;
                case eGetData.GetAvailableNavPanels:
                    IBasePlugin n1 = Core.pluginCollection.Find(s => s.pluginName == name);
                    if (n1 == null)
                        return;
                    data = ((INavigation)n1).availablePanels;
                    return;
                case eGetData.GetCurrentPosition:
                    IBasePlugin n2 = Core.pluginCollection.Find(s => s.pluginName == name);
                    if (n2 == null)
                        return;
                    data = ((INavigation)n2).Position;
                    return;
                case eGetData.GetNearestAddress:
                    IBasePlugin n3 = Core.pluginCollection.Find(s => s.pluginName == name);
                    if (n3 == null)
                        return;
                    data = ((INavigation)n3).Location;
                    return;
                case eGetData.GetDestination:
                    IBasePlugin n4 = Core.pluginCollection.Find(s => s.pluginName == name);
                    if (n4 == null)
                        return;
                    data = ((INavigation)n4).Destination;
                    return;
            }
        }
    }
}
