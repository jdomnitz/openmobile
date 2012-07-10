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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.Plugin;

namespace OMPlayer
{
    public sealed class OMPlayer : IAVPlayer, IDisposable
{
    // Fields
    private AVPlayer[] player;
    private static IPluginHost theHost;
    static int WM_Graph_Notify = 0x8001;
    static int WM_LBUTTONUP = 0x0202;

    static int crossfade = 0;

    // Events
    public event MediaEvent OnMediaEvent;
    Settings settings;
    public Settings loadSettings()
    {
        if (settings == null)
        {
            if (vistaMode)
                return null;
            settings = new Settings("OMPlayer Settings");
            using (PluginSettings s = new PluginSettings())
            {
                settings.Add(new Setting(SettingTypes.MultiChoice, "Music.AutoResume", "", "Resume Playback at startup", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.AutoResume")));
                settings.Add(new Setting(SettingTypes.MultiChoice, "Music.PlayProtected", "", "Attempt to play protected files", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.PlayProtected")));
                List<string> crossfadeo = new List<string>(new string[] { "None", "1 Second", "2 Seconds", "3 Seconds", "4 Seconds", "5 Seconds" });
                List<string> crossfadev = new List<string>(new string[] { "", "1000", "2000", "3000", "4000", "5000" });
                settings.Add(new Setting(SettingTypes.MultiChoice, "Music.CrossfadeDuration", "Crossfade", "Crossfade between songs",crossfadeo,crossfadev, s.getSetting("Music.CrossfadeDuration")));
                settings.Add(new Setting(SettingTypes.MultiChoice, "Music.MediaKeys", "", "Respond to media keys", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.MediaKeys")));
            }
            settings.OnSettingChanged += new SettingChanged(changed);
        }
        return settings;
    }

    void changed(int screen,Setting s)
    {
        if (s.Name == "Music.CrossfadeDuration")
        {
            if (s.Value != "")
                crossfade = int.Parse(s.Value);
        }
        using (PluginSettings setting = new PluginSettings())
            setting.setSetting(s.Name, s.Value);
    }
    private void checkInstance(Zone zone)
    {
        lock (player)
        {
            if (player[zone.AudioDeviceInstance] == null)
                sink.Invoke(OnPlayerRequested, new object[] { zone });
        }
    }
    private void createInstance(Zone zone)
    {
        player[zone.AudioDeviceInstance] = new AVPlayer(zone);
        player[zone.AudioDeviceInstance].OnMediaEvent += forwardEvent;
    }
    public void Dispose()
    {
        if (player != null)
        {
            int k = 0;
            while ((fading) && (k < 16))
            {
                k++;
                Thread.Sleep(100);
            }
            for (int i = 0; i < player.Length; i++)
            {
                if (player[i] != null)
                    player[i].CloseClip();
            }
            if (oldPlayer != null)
                oldPlayer.CloseClip();
        }
        GC.SuppressFinalize(this);
    }
    AVPlayer oldPlayer;
    public void forwardEvent(eFunction function, Zone zone, string arg)
    {
        if (function == eFunction.nextMedia)
        {
            if (crossfade > 0)
            {
                lock (player)
                {
                    AVPlayer tmp = oldPlayer;
                    oldPlayer = player[zone.AudioDeviceInstance];
                    player[zone.AudioDeviceInstance] = tmp;
                }
                if (player[zone.AudioDeviceInstance] != null)
                {
                    player[zone.AudioDeviceInstance].zone = zone;
                    player[zone.AudioDeviceInstance].OnMediaEvent += forwardEvent;
                }
                oldPlayer.zone = null;
                if (OnMediaEvent != null)
                    OnMediaEvent(function, zone, arg);
                for (int i = 10; i >= 0; i--)
                {
                    if (player[zone.AudioDeviceInstance] != null)
                        player[zone.AudioDeviceInstance].setVolume((10 - i) * 10);
                    oldPlayer.setVolume(i * 10);
                    Thread.Sleep(crossfade/10);
                }
                oldPlayer.OnMediaEvent -= forwardEvent;
                return;
            }
        }
        if (OnMediaEvent != null)
            OnMediaEvent(function, zone, arg);
    }
    public float getCurrentPosition(Zone zone)
    {
        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].mediaPosition == null)
            return -1F;
        if ((player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Stopped) || (player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Ready))
            return -1F;
        return (float)player[zone.AudioDeviceInstance].pos;
    }
    public static IMoniker getDevMoniker(int dev)
    {
        DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
        List<IMoniker> lst = new List<IMoniker>();
        System.Diagnostics.Debug.WriteLine("DirectSound list start:");
        for (int i = 0; i < d.Length; i++)
        {
            if (d[i].Name.Contains("DirectSound"))
            {
                System.Diagnostics.Debug.WriteLine("\t" + i.ToString() + " : " + d[i].Name);
                lst.Add(d[i].Mon);
            }
        }
        System.Diagnostics.Debug.WriteLine("DirectSound list end");
        if ((dev < lst.Count) && (dev >= 0))
            return lst[dev];
        return lst[0];
    }
    
        /// <summary>
        /// Select AudioDevice moniker based on audio device name
        /// </summary>
        /// <param name="dev">AudioDevice name</param>
        /// <returns></returns>
        public static IMoniker getDevMoniker(string dev)
        {
            //System.Diagnostics.Debug.WriteLine("Requested directsound unit: " + dev);
            DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
            IMoniker DefaultUnit = null;
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i].Name.Contains("DirectSound"))
                {
                    if (d[i].Name.Contains(dev))
                    {
                        //System.Diagnostics.Debug.WriteLine("Selected directsound unit: " + d[i].Name);
                        return d[i].Mon;
                    }
                    else
                    {
                        if (DefaultUnit == null)
                        {
                            DefaultUnit = d[i].Mon;
                            //System.Diagnostics.Debug.WriteLine("Directsound default unit : " + d[i].Name);
                        }
                    }
                }
            }
            //System.Diagnostics.Debug.WriteLine("Using directsound default unit!");
            return DefaultUnit;
        }

        public mediaInfo getMediaInfo(Zone zone)
    {
        checkInstance(zone);
        if ((player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Stopped) || (player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Ready))
            return new mediaInfo();
        else
            return player[zone.AudioDeviceInstance].nowPlaying;
    }

        public float getPlaybackSpeed(Zone zone)
    {
        checkInstance(zone);
        return (float)player[zone.AudioDeviceInstance].currentPlaybackRate;
    }

        public ePlayerStatus getPlayerStatus(Zone zone)
    {
        checkInstance(zone);
        return player[zone.AudioDeviceInstance].currentState;
    }

        public int getVolume(Zone zone)
    {
        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].currentVolume < 0)
            return -1; //volume muted
        else
            return player[zone.AudioDeviceInstance].currentVolume;
    }
    private void saveState()
    {
        using (PluginSettings settings = new PluginSettings())
        {
            foreach (Zone zone in theHost.ZoneHandler.Zones)
            {
                if ((player!=null)&&(player[zone.AudioDeviceInstance] != null) && (player[zone.AudioDeviceInstance].nowPlaying != null) && (player[zone.AudioDeviceInstance].mediaPosition != null))
                {
                    if ((getPlayerStatus(zone) == ePlayerStatus.Stopped) || (getPlayerStatus(zone) == ePlayerStatus.Ready))
                    {
                        settings.setSetting("Music.Instance" + zone.AudioDeviceInstance.ToString() + ".LastPlayingURL", "");
                        continue;
                    }
                    settings.setSetting("Music.Instance" + zone.AudioDeviceInstance.ToString() + ".LastPlayingURL", player[zone.AudioDeviceInstance].nowPlaying.Location);
                    settings.setSetting("Music.Instance" + zone.AudioDeviceInstance.ToString() + ".LastPlayingPosition", player[zone.AudioDeviceInstance].pos.ToString());
                }
                else
                {
                    settings.setSetting("Music.Instance" + zone.AudioDeviceInstance.ToString() + ".LastPlayingURL", "");
                }
            }
        }
    }
    private void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
    {
        if (function == eFunction.closeProgram)
        {
            saveState();
            fadeout();
        }
        else if (function == eFunction.RenderingWindowResized)
        {
            foreach (Zone zone in theHost.ZoneHandler.GetZonesForScreen(int.Parse(arg1)))
            {
                if (zone.AudioDeviceInstance >= player.Length)
                    return;
                if (player[zone.AudioDeviceInstance] != null)
                    player[zone.AudioDeviceInstance].Resize();
            }

            /*
            int inst = theHost.ZoneHandler.GetZone(int.Parse(arg1)).AudioDeviceInstance;
            if (inst >= player.Length)
                return;
            if (player[inst] != null)
                player[inst].Resize();
            */
        }
        else if (function == eFunction.videoAreaChanged)
        {
            foreach (Zone zone in theHost.ZoneHandler.GetZonesForScreen(int.Parse(arg1)))
            {
                if (zone.AudioDeviceInstance < 0 | zone.AudioDeviceInstance >= player.Length)
                    continue;
                if (player[zone.AudioDeviceInstance] != null)
                    player[zone.AudioDeviceInstance].Resize();
            }

            /*
            int inst = int.Parse(arg1);
            if (inst >= player.Length)
                return;
            if (player[inst] != null)
                player[inst].Resize();
            */
        }
    }

    public bool incomingMessage(string message, string source)
    {
        return false;
    }

    public bool incomingMessage<T>(string message, string source, ref T data)
    {
        return false;
    }
    MessageProc sink;
    private delegate void CreateNewPlayer(Zone zone);
    private event CreateNewPlayer OnPlayerRequested;
    bool vistaMode = false;
    bool sevenMode = false;
    public eLoadStatus initialize(IPluginHost host)
    {
        vistaMode=Environment.OSVersion.Version.Major > 5;
        if (vistaMode)
            sevenMode = (Environment.OSVersion.Version.Minor == 1);
        theHost = host;
        sink = new MessageProc("MainForm(PluginInitialize)");
        IntPtr tmp = sink.Handle;
        player = new AVPlayer[theHost.AudioDeviceCount];
        host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
        host.OnPowerChange += new PowerEvent(host_OnPowerChange);
        host.OnKeyPress += new KeyboardEvent(host_OnKeyPress);
        OnPlayerRequested += new CreateNewPlayer(createInstance);
        using (PluginSettings setting = new PluginSettings())
        {
            setting.setSetting("Default.AVPlayer.Files", "OMPlayer");
            string cf = setting.getSetting("Music.CrossfadeDuration");
            if (cf != "")
                crossfade = int.Parse(cf);
            if (!vistaMode)
            {
                /*
                if (setting.getSetting("Music.AutoResume") == "True")
                {
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            int k = theHost.GetDefaultZoneForScreen(i).AudioDeviceInstance;
                            host.execute(eFunction.loadAVPlayer, k.ToString(), "OMPlayer");
                            checkInstance(k);
                            player[k].currentVolume = 0;
                            string lastUrl = setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingURL");
                            play(k, lastUrl, eMediaType.Local);
                            theHost.execute(eFunction.setPlaylistPosition, k.ToString(), lastUrl);
                            player[k].currentVolume = 100;
                            if (player[k].mediaPosition == null)
                                return;
                            if (double.TryParse(setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingPosition"), out player[k].pos) == true)
                            {
                                if (player[k].pos > 1)
                                    player[k].pos -= 1;
                                player[k].mediaPosition.put_CurrentPosition(player[k].pos);
                            }
                        }
                        fadeIn();
                    }, theHost);
                }
                */
            }
        }
        return eLoadStatus.LoadSuccessful;
    }

    bool host_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg, ref bool handled)
    {
        if (type != eKeypressType.KeyDown)
            return false;
        if ((settings == null)||(settings.Count<4))
            return false;
        if (settings[3].Value == "True")
        {
            switch (arg.Key)
            {
                case OpenMobile.Input.Key.PlayPause:
                    object o = new object();
                    theHost.getData(eGetData.GetMediaStatus, "", theHost.ZoneHandler.GetZone(0).ToString(), out o);
                    if (o == null)
                        return false;
                    if (o.GetType() == typeof(ePlayerStatus))
                    {
                        ePlayerStatus status = (ePlayerStatus)o;
                        if (status == ePlayerStatus.Playing)
                            theHost.execute(eFunction.Pause, theHost.ZoneHandler.GetZone(0).ToString());
                        else
                        {
                            if ((status == ePlayerStatus.FastForwarding) || (status == ePlayerStatus.Rewinding))
                                theHost.execute(eFunction.setPlaybackSpeed, theHost.ZoneHandler.GetZone(0).ToString(), "1");
                            else
                                theHost.execute(eFunction.Play, theHost.ZoneHandler.GetZone(0).ToString());
                        }
                    }
                    else if (o.GetType() == typeof(stationInfo))
                        theHost.execute(eFunction.Pause, theHost.ZoneHandler.GetZone(0).ToString());
                    return true;
                case OpenMobile.Input.Key.Pause:
                    theHost.execute(eFunction.Pause, theHost.ZoneHandler.GetZone(0).ToString());
                    return true;
                case OpenMobile.Input.Key.TrackNext:
                    if (theHost.execute(eFunction.stepForward, theHost.ZoneHandler.GetZone(0).ToString()) == false)
                        theHost.execute(eFunction.nextMedia, theHost.ZoneHandler.GetZone(0).ToString());
                    return true;
                case OpenMobile.Input.Key.TrackPrevious:
                    if (theHost.execute(eFunction.stepBackward, theHost.ZoneHandler.GetZone(0).ToString()) == false)
                        theHost.execute(eFunction.previousMedia, theHost.ZoneHandler.GetZone(0).ToString());
                    return true;
                case OpenMobile.Input.Key.Stop:
                    theHost.execute(eFunction.Stop, theHost.ZoneHandler.GetZone(0).ToString());
                    theHost.execute(eFunction.unloadTunedContent, theHost.ZoneHandler.GetZone(0).ToString());
                    return true;
            }
        }
        return false;
    }
    void host_OnPowerChange(ePowerEvent type)
    {
        if (type == ePowerEvent.SleepOrHibernatePending)
        {
            fadeout();
        }
        else if (type == ePowerEvent.SystemResumed)
        {
            fadeIn();
        }
    }
    private bool fading = false;
    private void fadeout()
    {
        fading = true;
        for (int k = 100; k >0; k-=10)
            foreach (Zone zone in theHost.ZoneHandler.Zones)
            {
                if ((player[zone.AudioDeviceInstance] != null) && (player[zone.AudioDeviceInstance].currentVolume > k))
                {
                    setVolume(zone, k);
                    Thread.Sleep(150);
                }
            }
        fading = false;
    }
    private void fadeIn()
    {
        for (int k = 1; k < 10; k++)
            foreach (Zone zone in theHost.ZoneHandler.Zones)
            {
                if (player[zone.AudioDeviceInstance] != null)
                {
                    setVolume(zone, (k * 10));
                    Thread.Sleep(150);
                }
            }
    }
    public bool pause(Zone zone)
    {
        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].mediaControl == null)
            return false;
        if ((player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Paused) || (player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Stopped))
        {
            if (player[zone.AudioDeviceInstance].mediaControl.Run() >= 0)
            {
                player[zone.AudioDeviceInstance].currentState = ePlayerStatus.Playing;
                OnMediaEvent(eFunction.Play, zone, "");
                return true;
            }
        }
        else
        {
            if (player[zone.AudioDeviceInstance].mediaControl.Pause() >= 0)
            {
                player[zone.AudioDeviceInstance].currentState = ePlayerStatus.Paused;
                OnMediaEvent(eFunction.Pause, zone, "");
                return true;
            }
        }
        return false;
    }
    public bool SetVideoVisible(Zone zone, bool visible)
    {
        // Errorcheck
        if (zone == null) return false;
        if (zone.AudioDeviceInstance < 0) return false;

        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].videoWindow == null)
            return false;
        if (visible == false)
        {
            if (player[zone.AudioDeviceInstance].videoWindow.put_Visible(OABool.False) == 0)
            {
                OnMediaEvent(eFunction.hideVideoWindow, zone, "");
                return true;
            }
        }
        else if ((player[zone.AudioDeviceInstance].currentState != ePlayerStatus.Ready) && (player[zone.AudioDeviceInstance].currentState != ePlayerStatus.Stopped))
        {
            if (!player[zone.AudioDeviceInstance].fullscreen)
            {
                player[zone.AudioDeviceInstance].fullscreen = true;
                player[zone.AudioDeviceInstance].Resize();
                return true;
            }
            OnMediaEvent(eFunction.showVideoWindow, zone, "");
            return (player[zone.AudioDeviceInstance].videoWindow.put_Visible(OABool.True) == 0);
        }
        return false;
    }
    public bool play(Zone zone)
    {
        checkInstance(zone);
        if ((player[zone.AudioDeviceInstance].mediaControl != null) && (player[zone.AudioDeviceInstance].mediaControl.Run() >= 0))
        {
            player[zone.AudioDeviceInstance].currentState = ePlayerStatus.Playing;
            OnMediaEvent(eFunction.Play, zone, "");
            return true;
        }
        else
        {
            if (!string.IsNullOrEmpty(player[zone.AudioDeviceInstance].nowPlaying.Location))
                theHost.execute(eFunction.Play, zone.ToString(), player[zone.AudioDeviceInstance].nowPlaying.Location);
        }
        return false;
    }

    public bool play(Zone zone, string url, eMediaType type)
    {
        switch (type)
        {
            case eMediaType.BluetoothResource:
            case eMediaType.BluRay:
            case eMediaType.HDDVD:
            case eMediaType.HTTPUrl:
            case eMediaType.AppleDevice:
            case eMediaType.RTSPUrl:
            case eMediaType.Smartphone:
            case eMediaType.MMSUrl:
            case eMediaType.DVD:
                return false;
        }
        try
        {
            url = url.ToLower();
            if (url.EndsWith(".m4p"))
            {
                using (PluginSettings s = new PluginSettings())
                    if (s.getSetting("Music.PlayProtected") != "True")
                        return false;
            }
            if (url.EndsWith(".ifo"))
            {
                theHost.execute(eFunction.Play, zone.ToString(), System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(url)));
                return false;
            }
            if (vistaMode)
            {
                if ((url.EndsWith("mp3")) || (url.EndsWith("wmv")) || (url.EndsWith("wma")) || (url.EndsWith("asf")))
                    return false;
                if (sevenMode)
                    if ((url.EndsWith("mp4")) || (url.EndsWith("m4a"))||(url.EndsWith("avi"))||(url.EndsWith("mkv"))||(url.EndsWith("mov")))
                        return false;
            }
            checkInstance(zone);
            return player[zone.AudioDeviceInstance].play(zone, url);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool setPlaybackSpeed(Zone zone, float speed)
    {
        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].SetRate((double)speed) != 0)
            return false;
        if (player[zone.AudioDeviceInstance].currentPlaybackRate > 1.0)
        {
            player[zone.AudioDeviceInstance].currentState = ePlayerStatus.FastForwarding;
        }
        else if (player[zone.AudioDeviceInstance].currentPlaybackRate < 1.0)
        {
            player[zone.AudioDeviceInstance].currentState = ePlayerStatus.Rewinding;
        }
        else if ((player[zone.AudioDeviceInstance].currentState == ePlayerStatus.FastForwarding) || (player[zone.AudioDeviceInstance].currentState == ePlayerStatus.Rewinding))
        {
            player[zone.AudioDeviceInstance].currentState = ePlayerStatus.Playing;
        }
        OnMediaEvent(eFunction.setPlaybackSpeed, zone, speed.ToString());
        return true;
    }

    public bool setPosition(Zone zone, float seconds)
    {
        checkInstance(zone);
        return player[zone.AudioDeviceInstance].setPosition(seconds);
    }

    public bool setVolume(Zone zone, int percent)
    {
        checkInstance(zone);
        return player[zone.AudioDeviceInstance].setVolume(percent);
    }

    public bool stop(Zone zone)
    {
        checkInstance(zone);
        if (player[zone.AudioDeviceInstance].stop())
        {
            OnMediaEvent(eFunction.Stop, zone, "");
            return true;
        }
        return false;
    }

    // Properties
    public string authorEmail
    {
        get
        {
            return "jdomnitz@gmail.com";
        }
    }

    public string authorName
    {
        get
        {
            return "Justin Domnitz";
        }
    }

    static string[] array;
    public string[] OutputDevices
    {
        get
        {
            if (array != null)
                return array;
            if (theHost == null)
                vistaMode = (Environment.OSVersion.Version.Major > 5);
            if (vistaMode)
                return new string[0];
            DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
            List<string> lst = new List<string>();
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i].Name.Contains("DirectSound"))
                {
                    lst.Add(d[i].Name.Replace("DirectSound", "").Replace(":", "").Replace("  ", " "));
                }
            }
            array = lst.ToArray();
            return array;
        }
    }

    public string pluginDescription
    {
        get
        {
            return "DirectShow Media Player";
        }
    }

    public string pluginName
    {
        get
        {
            return "OMPlayer";
        }
    }

    public float pluginVersion
    {
        get
        {
            return 0.5f;
        }
    }

    public bool SupportsAdvancedFeatures
    {
        get
        {
            return false;
        }
    }
    public sealed class AVPlayer
    {
        // Fields
        public IBasicAudio basicAudio = null;
        private IBasicVideo basicVideo = null;
        public double currentPlaybackRate = 1.0;
        public ePlayerStatus currentState = ePlayerStatus.Ready;
        public int currentVolume = 100;
        public IGraphBuilder graphBuilder = null;
        //public int instance = -1;
        public Zone zone = null;
        public bool isAudioOnly = true;
        public IMediaControl mediaControl = null;
        private IMediaEventEx mediaEventEx = null;
        public IMediaPosition mediaPosition = null;
        public IMediaSeeking mediaSeeking = null;
        public mediaInfo nowPlaying = new mediaInfo();
        public MediaEvent OnMediaEvent;
        public double pos = -1.0;
        public IVideoWindow videoWindow = null;
        private Thread t;
        private MessageProc sink;
        public bool fullscreen = false;
        IntPtr drain = IntPtr.Zero;
        private delegate bool PositionCallback(float seconds);
        private delegate int SpeedCallback(double rate);
        private delegate bool VoidCallback();
        private delegate bool PlayCallback(Zone zone, string filename);
        private delegate void GetPositionCallback();
        private event PositionCallback OnSetPosition;
        private event VoidCallback OnStop;
        private event PlayCallback OnPlay;
        private event SpeedCallback OnSetRate;
        private event GetPositionCallback OnGetPosition;
        // Methods
        public AVPlayer(Zone zone)
        {
            //instance = instanceNum;
            this.zone = zone;
            sink = new MessageProc("AVPlayer:"+zone.Name.ToString());
            Console.WriteLine("AVPlayer:"+zone.Name.ToString() + " (MainForm) AudioInstance: " + zone.AudioDeviceInstance.ToString() + "[" + zone.AudioDeviceName + "]");
            drain = sink.Handle;
            sink.OnClick += new MessageProc.Click(clicked);
            sink.OnEvent += new MessageProc.eventOccured(eventOccured);
            OnSetPosition += new PositionCallback(AVPlayer_OnSetPosition);
            OnStop+=new VoidCallback(stop);
            OnSetRate+=new SpeedCallback(SetRate);
            OnPlay+=new PlayCallback(PlayMovieInWindow);
            OnGetPosition+=new GetPositionCallback(getCurrentPos);
        }

        bool AVPlayer_OnSetPosition(float seconds)
        {
            int hr = -1;
            lock (mediaControl)
            {
                mediaControl.Pause();
                double s = seconds;
                hr = mediaPosition.put_CurrentPosition(s);
                if (currentState == ePlayerStatus.Playing)
                    mediaControl.Run();
            }
            return (hr==0);
        }

        void clicked()
        {
            fullscreen = !fullscreen;
            Resize();
        }

        public void eventOccured()
        {
            EventCode e;
            IntPtr arg1;
            IntPtr arg2;
            FilterState fs;
            if (mediaEventEx == null)
                return;
            mediaEventEx.GetEvent(out e, out arg1, out arg2, 0);
            switch (e)
            {
                case EventCode.Complete:
                    if (crossfade==0)
                        OnMediaEvent(eFunction.nextMedia, zone, "");
                    else
                        stop();
                    break;
                case EventCode.DeviceLost:
                case EventCode.ErrorAbort:
                case EventCode.ErrorStPlaying:
                //case EventCode.FileClosed:
                case EventCode.StErrStopped:
                    currentState = ePlayerStatus.Error;
                    break;
                case EventCode.Paused:
                    if ((mediaControl!=null)&&(mediaControl.GetState(50,out fs)==0))
                        if (fs == FilterState.Paused)
                        {
                            currentState = ePlayerStatus.Paused;
                            OnMediaEvent(eFunction.Pause, zone, "");
                        }
                    break;
            }
        }
        private void CheckVisibility()
        {
            int hr = 0;
            if ((videoWindow == null) || (basicVideo == null))
            {
                isAudioOnly = true;
            }
            else
            {
                OABool lVisible;
                isAudioOnly = false;
                hr = videoWindow.get_Visible(out lVisible);
                if (hr < 0)
                {
                    if (hr == -2147467262)
                    {
                        isAudioOnly = true;
                        videoWindow = null;
                        basicVideo = null;
                    }
                    else
                    {
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
            }
        }
        public bool play(Zone zone, string url)
        {
            lock (this)
            {
                if ((currentState != ePlayerStatus.Stopped) && (currentState != ePlayerStatus.Ready))
                    stop();
                object o = sink.Invoke(OnPlay, new object[] { zone, url });
                if ((bool)o == false)
                    return false;
            }
            pos = 0;
            if (url.EndsWith(".cda")==true)
            {
                nowPlaying = CDDB.getSongInfo(theHost,url);
                if (nowPlaying == null)
                    nowPlaying = new mediaInfo();
                if (nowPlaying.coverArt==null)
                    nowPlaying.coverArt = theHost.getSkinImage("Discs|AudioCD").image;
                nowPlaying.Type = eMediaType.AudioCD;
            }
            else
            {
                nowPlaying = TagReader.getInfo(url);
                if (nowPlaying == null)
                    nowPlaying = new mediaInfo(url);
                nowPlaying.Type = eMediaType.Local;
                if (nowPlaying.coverArt == null)
                    nowPlaying.coverArt = TagReader.getCoverFromDB(nowPlaying.Artist, nowPlaying.Album);
                if (nowPlaying.coverArt == null)
                    nowPlaying.coverArt = TagReader.getFolderImage(nowPlaying.Location);
                if (nowPlaying.coverArt == null)
                    nowPlaying.coverArt = TagReader.getLastFMImage(nowPlaying.Artist, nowPlaying.Album);
            }
            if (nowPlaying.Length<=0)
            {
                double dur;
                mediaPosition.get_Duration(out dur);
                nowPlaying.Length = (int)dur;
            }
            OnMediaEvent(eFunction.Play, zone, url);
            if (t == null)
            {
                t = new Thread(waitForStop);
                t.Name = String.Format("OMPlayer.OMPlayer.AVPlayer.{0}", zone);
                t.Start();
            }
            return true;
        }
        public bool stop()
        {
            if ((currentState == ePlayerStatus.Paused) || (currentState == ePlayerStatus.Playing) || (currentState == ePlayerStatus.FastForwarding) || (currentState == ePlayerStatus.Rewinding))
            {
                if (sink.InvokeRequired)
                {
                    object ret = sink.Invoke(OnStop);
                    if (ret == null)
                        return false;
                    return (bool)ret;
                }
                if ((mediaControl == null) || (mediaSeeking == null))
                    return false;
                try
                {
                    currentState = ePlayerStatus.Ready;
                    mediaControl.Stop();
                }
                catch (AccessViolationException) { return false; }
            }
            currentState = ePlayerStatus.Stopped;
            mediaControl = null;
            basicAudio = null;
            if (isAudioOnly == false)
            {
                DsError.ThrowExceptionForHR(videoWindow.put_Visible(OABool.False));
                DsError.ThrowExceptionForHR(videoWindow.put_Owner(IntPtr.Zero));
                OnMediaEvent(eFunction.hideVideoWindow, zone, "");
            }
            isAudioOnly = true;
            return true;
        }
        public void CloseClip()
        {
            if (mediaControl != null)
            {
                stop();
                currentState = ePlayerStatus.Stopped;
                CloseInterfaces();
                isAudioOnly = true;
                currentState = ePlayerStatus.Ready;
            }
        }
        private void CloseInterfaces()
        {
            try
            {
                if (!isAudioOnly)
                {
                    videoWindow.put_Visible(OABool.False);
                    videoWindow.put_Owner(IntPtr.Zero);
                }
                if (mediaEventEx != null)
                {
                    mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                    mediaEventEx = null;
                }
                    mediaSeeking = null;
                    mediaPosition = null;
                    mediaControl = null;
                    basicAudio = null;
                    basicVideo = null;
                    videoWindow = null;
                if (graphBuilder != null)
                {
                    try
                    {
                        lock(graphBuilder)
                            Marshal.ReleaseComObject(graphBuilder);
                    }
                    catch (Exception){}
                }
                graphBuilder = null;
            }
            catch
            {
            }
        }
        public void getCurrentPos()
        {
            double time;
            if (mediaPosition != null)
            {
                mediaPosition.get_CurrentPosition(out time);
                pos = time;
            }
        }
        /*
        private int screen()
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                if (theHost.GetDefaultZoneForScreen(i) == instance)
                    return i;
            }
            return 0;
        }
        */
        public int Resize()
        {
            if (videoWindow == null)
                return -1;
            if (fullscreen == true)
            {
                OpenMobile.Platform.Windows.WindowInfo info = new OpenMobile.Platform.Windows.WindowInfo();
                OpenMobile.Platform.Windows.Functions.GetWindowInfo((IntPtr)OMPlayer.theHost.GetWindowHandle(zone.Screen),ref info);
                if ((info.Style & OpenMobile.Platform.Windows.WindowStyle.ThickFrame) == OpenMobile.Platform.Windows.WindowStyle.ThickFrame)
                    return videoWindow.SetWindowPosition(0, 0, info.Window.Width - (info.Window.Width-info.Client.Width), info.Window.Height - (info.Window.Height-info.Client.Height));
                else
                    return videoWindow.SetWindowPosition(0, 0, info.Window.Width + 1, info.Window.Height);
            }
            else
            {
                object o;
                theHost.getData(eGetData.GetScaleFactors, "", zone.Screen.ToString(), out o);
                if (o == null)
                    return -1;
                PointF sf = (PointF)o;
                Rectangle pos = theHost.GetVideoPosition(zone.Screen);
                return videoWindow.SetWindowPosition((int)(pos.Left * sf.X), (int)(pos.Top * sf.Y), (int)(pos.Width * sf.X), (int)(pos.Height * sf.Y));
            }
        }

        public bool PlayMovieInWindow(Zone zone, string filename)
        {
            int hr = 0;
            if (filename == string.Empty)
                return false;
            currentState = ePlayerStatus.Transitioning;
            graphBuilder = (IGraphBuilder) new FilterGraph();
            IBaseFilter source = null;
            if ((zone != null) && (zone.AudioDeviceInstance>0))
                hr = ((IFilterGraph2)graphBuilder).AddSourceFilterForMoniker(OMPlayer.getDevMoniker(zone.AudioDeviceName), null, "OutputDevice", out source);
            hr = graphBuilder.RenderFile(filename, null);
            if (hr == 262744)
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, "Failed to render audio for: " + filename);

            if (hr < 0)
                return false;
            mediaControl = (IMediaControl) graphBuilder;
            mediaEventEx = (IMediaEventEx) graphBuilder;
            mediaSeeking = (IMediaSeeking) graphBuilder;
            mediaPosition = (IMediaPosition) graphBuilder;
            videoWindow = graphBuilder as IVideoWindow;
            basicVideo = graphBuilder as IBasicVideo;
            basicAudio = graphBuilder as IBasicAudio;
            setVolume(currentVolume);
            CheckVisibility();
            if (!isAudioOnly)
            {

                int ScreenInstance = zone.Screen;//getFirstScreen(instance);
                System.Diagnostics.Debug.WriteLine("AVPlayer ScreenInstance: " + ScreenInstance.ToString());
                System.Diagnostics.Debug.WriteLine("AVPlayer AudioInstance: " + zone.AudioDeviceInstance.ToString() + "[" + zone.AudioDeviceName + "]");
                OnMediaEvent(eFunction.showVideoWindow, zone, "");
                DsError.ThrowExceptionForHR(videoWindow.put_Owner((IntPtr)OMPlayer.theHost.GetWindowHandle(ScreenInstance)));//(getFirstScreen(instance))));
                DsError.ThrowExceptionForHR(videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren));
                DsError.ThrowExceptionForHR(Resize());
                DsError.ThrowExceptionForHR(videoWindow.put_MessageDrain(drain));
            }
            currentPlaybackRate = 1.0;
            if (mediaControl.Run()<0)
                return false;
            mediaEventEx.SetNotifyWindow(drain, WM_Graph_Notify, new IntPtr(zone.AudioDeviceInstance));
            currentState = ePlayerStatus.Playing;
            return true;
        }
        /*
        private int getFirstScreen(Zone zone)
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
                if (theHost.GetDefaultZoneForScreen(i) == instance)
                    return i;
            return 0;
        }
        */
        public int SetRate(double rate)
        {
            int hr = 0;
            if (mediaPosition != null)
            {
                if (sink.InvokeRequired)
                {
                    return (int)sink.Invoke(OnSetRate, new object[] { rate });
                }
                hr = mediaSeeking.SetRate(rate);
                if (hr == 0)
                    currentPlaybackRate = rate;
            }
            return hr;
        }
        public bool setVolume(int percent)
        {
            if ((graphBuilder != null) && (basicAudio != null))
            {
                if ((percent > 0) && (percent <= 100))
                {
                    currentVolume = percent;
                    return (basicAudio.put_Volume((int)(2000 * Math.Log10(Math.Pow((percent / 100.0), 2)))) == 0);
                }
                else if ((percent == 0) || (percent == -1))
                {
                    if (percent == -1)
                    {
                        if (currentVolume > 0)
                            currentVolume *= -1;
                        else
                            return false;
                    }
                    return (basicAudio.put_Volume(-10000) == 0);
                }
                else if (percent == -2)
                {
                    if (currentVolume < 0)
                    {
                        currentVolume *= -1;
                        return (basicAudio.put_Volume((int)(2000 * Math.Log10(Math.Pow((currentVolume / 100.0), 2)))) == 0);
                    }
                }
            }
            return false;
        }
        private void waitForStop()
        {
            if (mediaControl != null)
            {
                while(zone != null)
                {
                    Thread.Sleep(1000);
                    if (mediaPosition != null)
                    {
                        // Update pos with currect playback position
                        sink.Invoke(OnGetPosition);
                        if ((crossfade>0)&&(isAudioOnly)&&((int)pos == nowPlaying.Length - (crossfade/1000)))
                        {
                            OnMediaEvent(eFunction.nextMedia, zone, "");
                        }
                    }
                }
            }
            t = null;
        }

        internal bool setPosition(float seconds)
        {
            if ((currentState == ePlayerStatus.Stopped) || (currentState == ePlayerStatus.Ready) || (currentState == ePlayerStatus.Error))
                return false;
            if ((mediaControl == null) || (mediaPosition == null))
                return false;
            double old=pos;
            pos = seconds;
            bool success=(bool)sink.Invoke(OnSetPosition, new object[] { seconds });
            if (!success)
                pos=old;
            return success;
        }
    }

    public sealed class MessageProc : Form
    {
        
        public MessageProc(string Name)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.Black;
            OpenMobile.Platform.Windows.Functions.ShowCursor(false);

            // Ensure handle is created
            this.Handle.ToInt32();

            //this.Text = Name;
            //this.Show();
        }

        public delegate void eventOccured();
        public new delegate void Click();
        public new Click OnClick;
        public eventOccured OnEvent;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_Graph_Notify)
                //OnEvent((int)m.LParam);
                OnEvent();
            else if (m.Msg == WM_LBUTTONUP)
            {
                OnClick();
                return;
            }
            else
                base.WndProc(ref m);
        }
    }
}
}
