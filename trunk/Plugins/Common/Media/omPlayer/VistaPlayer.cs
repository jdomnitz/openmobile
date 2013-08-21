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
using System.Threading;
using System.Windows.Forms;
using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.Plugin;
using OpenMobile.Threading;
using VistaAudio.CoreAudioApi;

namespace OMPlayer
{
    public sealed class VistaPlayer : IAVPlayer, IAudioDeviceProvider
    {
        // Fields
        internal static AVPlayer[] player;
        private static IPluginHost theHost;
        static int WM_LBUTTONUP = 0x0202;

        static int crossfade = 0;

        // Events
        public event MediaEvent OnMediaEvent;
        Settings settings;
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("VistaPlayer Settings");
                using (PluginSettings s = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.AutoResume", String.Empty, "Resume Playback at startup", Setting.BooleanList, Setting.BooleanList, s.getSetting(this, "Music.AutoResume")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.PlayProtected", String.Empty, "Attempt to play protected files", Setting.BooleanList, Setting.BooleanList, s.getSetting(this, "Music.PlayProtected")));
                    List<string> crossfadeo = new List<string>(new string[] { "None", "1 Second", "2 Seconds", "3 Seconds", "4 Seconds", "5 Seconds" });
                    List<string> crossfadev = new List<string>(new string[] { String.Empty, "1000", "2000", "3000", "4000", "5000" });
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.CrossfadeDuration", "Crossfade", "Crossfade between songs", crossfadeo, crossfadev, s.getSetting(this, "Music.CrossfadeDuration")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.MediaKeys", String.Empty, "Respond to media keys", Setting.BooleanList, Setting.BooleanList, s.getSetting(this, "Music.MediaKeys")));
                }
                settings.OnSettingChanged += new SettingChanged(changed);
            }
            return settings;
        }
        /*
        public static int getFirstScreen(Zone zone)
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
                if (theHost.GetDefaultZoneForScreen(i).AudioDevice.Instance == instance)
                    return i;
            return 0;
        }
        */
        void changed(int screen, Setting setting)
        {
            if (setting.Name == "Music.CrossfadeDuration")
            {
                if (!string.IsNullOrEmpty(setting.Value))
                    crossfade = int.Parse(setting.Value);
            }
            OpenMobile.helperFunctions.StoredData.Set(this, setting.Name, setting.Value);
        }
        private void checkInstance(Zone zone)
        {
            lock (player)
            {
                int AudioDeviceInstance = zone.AudioDevice.Instance;
                // Errorcheck
                if (AudioDeviceInstance < 0)
                    return;
                if (player[zone.AudioDevice.Instance] == null)
                        sink.Invoke(OnPlayerRequested, new object[] { zone });
            }
        }
        private void createInstance(Zone zone)
        {
            player[zone.AudioDevice.Instance] = new AVPlayer(zone);
            player[zone.AudioDevice.Instance].OnMediaEvent += forwardEvent;
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
                        player[i].CloseSession(false);
                }
                if (oldPlayer != null)
                    oldPlayer.CloseSession(false);
                MFExtern.MFShutdown();
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
                        oldPlayer = player[zone.AudioDevice.Instance];
                        player[zone.AudioDevice.Instance] = tmp;
                    }
                    if (player[zone.AudioDevice.Instance] != null)
                    {
                        player[zone.AudioDevice.Instance].zone = zone;
                        player[zone.AudioDevice.Instance].OnMediaEvent += forwardEvent;
                    }
                    oldPlayer.zone = null;
                    if (OnMediaEvent != null)
                        OnMediaEvent(function, zone, arg);
                    for (int i = 9; i >= 0; i--)
                    {
                        if (player[zone.AudioDevice.Instance] != null)
                            player[zone.AudioDevice.Instance].setVolume((10 - i) * 10);
                        oldPlayer.setVolume(i * 10);
                        Thread.Sleep(crossfade / 9);
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
            if (player[zone.AudioDevice.Instance].currentState == ePlayerStatus.Stopped)
                return -1F;
            return (float)player[zone.AudioDevice.Instance].pos;
        }

        public mediaInfo getMediaInfo(Zone zone)
        {
            checkInstance(zone);
            if (player[zone.AudioDevice.Instance].currentState == ePlayerStatus.Stopped)
                return new mediaInfo();
            else
                return player[zone.AudioDevice.Instance].nowPlaying;
        }

        public float getPlaybackSpeed(Zone zone)
        {
            checkInstance(zone);
            return (float)player[zone.AudioDevice.Instance].currentPlaybackRate;
        }

        public ePlayerStatus getPlayerStatus(Zone zone)
        {
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].currentState;
        }

        public int getVolume(Zone zone)
        {
            checkInstance(zone);
            if (player[zone.AudioDevice.Instance].currentVolume < 0)
                return -1; //volume muted
            else
                return player[zone.AudioDevice.Instance].currentVolume;
        }
        private void saveState()
        {
            using (PluginSettings settings = new PluginSettings())
            {
                foreach (Zone zone in theHost.ZoneHandler.Zones)
                {
                    if (zone.AudioDevice != null)
                    {
                        if ((player != null) && (player[zone.AudioDevice.Instance] != null) && (player[zone.AudioDevice.Instance].nowPlaying != null) && (player[zone.AudioDevice.Instance].pos > 0))
                        {
                            if (getPlayerStatus(zone) == ePlayerStatus.Stopped)
                            {
                                settings.setSetting(this, "Music.Instance" + zone.AudioDevice.Instance.ToString() + ".LastPlayingURL", "");
                                continue;
                            }
                            settings.setSetting(this, "Music.Instance" + zone.AudioDevice.Instance.ToString() + ".LastPlayingURL", player[zone.AudioDevice.Instance].nowPlaying.Location);
                            settings.setSetting(this, "Music.Instance" + zone.AudioDevice.Instance.ToString() + ".LastPlayingPosition", player[zone.AudioDevice.Instance].pos.ToString());
                        }
                        else
                        {
                            settings.setSetting(this, "Music.Instance" + zone.AudioDevice.Instance.ToString() + ".LastPlayingURL", "");
                        }
                    }
                }
            }
        }
        private void host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.closeProgram)
            {
                saveState();
                fadeout();
            }
            else if (function == eFunction.RenderingWindowResized)
            {
                if (OpenMobile.helperFunctions.Params.IsParamsValid(args, 1))
                {
                    foreach (Zone zone in theHost.ZoneHandler.GetZonesForScreen(int.Parse(OpenMobile.helperFunctions.Params.GetParam<string>(args,0))))
                    {
                        if (zone.AudioDevice.Instance >= player.Length)
                            return;
                        if (player[zone.AudioDevice.Instance] != null)
                            player[zone.AudioDevice.Instance].Resize();
                    }
                }
            }
            else if (function == eFunction.videoAreaChanged)
            {
                /*
                int inst = int.Parse(arg1);
                if (inst >= player.Length)
                    return;
                if (player[inst] != null)
                    player[inst].Resize();
                */
                if (OpenMobile.helperFunctions.Params.IsParamsValid(args, 1))
                {
                    foreach (Zone zone in theHost.ZoneHandler.GetZonesForScreen(int.Parse(OpenMobile.helperFunctions.Params.GetParam<string>(args, 0))))
                    {
                        if (zone.AudioDevice.Instance < 0 | zone.AudioDevice.Instance >= player.Length)
                            continue;
                        if (player[zone.AudioDevice.Instance] != null)
                            player[zone.AudioDevice.Instance].Resize();
                    }
                }
            }
        }

        public imageItem pluginIcon
        {
            get { return imageItem.NONE; }
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
        public eLoadStatus initialize(IPluginHost host)
        {
            if (Environment.OSVersion.Version.Major < 6)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;
            theHost = host;
            sink = new MessageProc("VistaPlayer(PluginInitialize)");
            IntPtr tmp = sink.Handle;
            player = new AVPlayer[theHost.AudioDeviceCount];
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            host.OnKeyPress += new KeyboardEvent(host_OnKeyPress);
            OnPlayerRequested += new CreateNewPlayer(createInstance);
            using (PluginSettings setting = new PluginSettings())
            {
                setting.setSetting(this, "Default.AVPlayer.Files", "OMPlayer2");
                string cf = setting.getSetting(this, "Music.CrossfadeDuration");
                if (cf != "")
                    crossfade = int.Parse(cf);
                _AudioDevices = OutputDevices;
                /*
                if (setting.getSetting("Music.AutoResume") == "True")
                {
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            int k = theHost.GetDefaultZoneForScreen(i);
                            host.execute(eFunction.loadAVPlayer, k.ToString(), "OMPlayer2");
                            checkInstance(k);
                            player[k].currentVolume = 0;
                            string lastUrl = setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingURL");
                            if (lastUrl.Length == 0)
                                return;
                            play(k, lastUrl, eMediaType.Local);
                            theHost.execute(eFunction.setPlaylistPosition, k.ToString(), lastUrl);
                            player[k].currentVolume = 100;
                            if (player[k].pos <= 0)
                                return;
                            if (double.TryParse(setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingPosition"), out player[k].pos) == true)
                            {
                                if (player[k].pos > 1)
                                    player[k].pos -= 1;
                                player[k].setPosition((float)player[k].pos);
                            }
                        }
                        fadeIn();
                    }, theHost);
                }
                */
            }
            return eLoadStatus.LoadSuccessful;
        }

        bool host_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg, ref bool handled)
        {
            if (type != eKeypressType.KeyDown)
                return false;
            if ((settings == null) || (settings.Count < 4))
                return false;
            if (settings[3].Value == "True")
            {
                // TODO: Move this code into the pluginhost
                
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
            for (int k = 100; k > 0; k -= 10)
                foreach (Zone zone in theHost.ZoneHandler.Zones)
                {
                    if (zone.AudioDevice != null)
                    {
                        if ((player[zone.AudioDevice.Instance] != null) && (player[zone.AudioDevice.Instance].currentVolume > k))
                        {
                            setVolume(zone, k);
                            Thread.Sleep(150);
                        }
                    }
                }
            fading = false;
        }
        private void fadeIn()
        {
            for (int k = 1; k < 10; k++)
                foreach (Zone zone in theHost.ZoneHandler.Zones)
                {
                    if (zone.AudioDevice != null)
                    {
                        if (player[zone.AudioDevice.Instance] != null)
                        {
                            setVolume(zone, (k * 10));
                            Thread.Sleep(150);
                        }
                    }
                }
        }
        public bool pause(Zone zone)
        {
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].pause();
        }
        public bool SetVideoVisible(Zone zone, bool visible)
        {
            // Errorcheck
            if (zone == null) return false;
            if (zone.AudioDevice.Instance < 0) return false;

            checkInstance(zone);
            if (player[zone.AudioDevice.Instance].m_pVideoDisplay == null)
                return false;
            bool currentlyVisible = OpenMobile.Platform.Windows.Functions.IsWindowVisible(player[zone.AudioDevice.Instance].drain);
            if (visible == currentlyVisible)
                return false;
            if (visible)
            {
                if (visible && !player[zone.AudioDevice.Instance].fullscreen)
                {
                    player[zone.AudioDevice.Instance].fullscreen = true;
                    player[zone.AudioDevice.Instance].Resize();
                    return true;
                }
                OnMediaEvent(eFunction.showVideoWindow, zone, "");
                return OpenMobile.Platform.Windows.Functions.ShowWindow(player[zone.AudioDevice.Instance].drain, OpenMobile.Platform.Windows.ShowWindowCommand.SHOW);
            }
            else if (OpenMobile.Platform.Windows.Functions.ShowWindow(player[zone.AudioDevice.Instance].drain, OpenMobile.Platform.Windows.ShowWindowCommand.HIDE))
            {
                OnMediaEvent(eFunction.hideVideoWindow, zone, "");
                return true;
            }
            return false;
        }
        public bool play(Zone zone)
        {
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].play();
        }

        public bool play(Zone zone, string url, eMediaType type)
        {
            switch (type)
            {
                case eMediaType.BluetoothResource:
                case eMediaType.BluRay:
                case eMediaType.HDDVD:
                case eMediaType.AppleDevice:
                case eMediaType.Smartphone:
                case eMediaType.MMSUrl:
                case eMediaType.DVD:
                case eMediaType.AudioCD:
                    return false;
            }
            try
            {
                url = url.ToLower();
                if (url.EndsWith(".m4p"))
                {
                    using (PluginSettings s = new PluginSettings())
                        if (s.getSetting(this, "Music.PlayProtected") != "True")
                            return false;
                }
                if (url.EndsWith(".ifo"))
                {
                    theHost.execute(eFunction.Play, zone.ToString(), System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(url)));
                    return false;
                }
                checkInstance(zone);
                return player[zone.AudioDevice.Instance].play(zone, url);
            }
            catch (Exception e)
            {
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Error, ErrorHandling.spewException(e));
                return false;
            }
        }

        public bool setPlaybackSpeed(Zone zone, float speed)
        {
            checkInstance(zone);
            if (!player[zone.AudioDevice.Instance].SetRate(speed))
                return false;
            if (speed > 1.0)
                player[zone.AudioDevice.Instance].currentState = ePlayerStatus.FastForwarding;
            else if (speed < 1.0)
                player[zone.AudioDevice.Instance].currentState = ePlayerStatus.Rewinding;
            else if ((player[zone.AudioDevice.Instance].currentState == ePlayerStatus.FastForwarding) || (player[zone.AudioDevice.Instance].currentState == ePlayerStatus.Rewinding))
                player[zone.AudioDevice.Instance].currentState = ePlayerStatus.Playing;
            OnMediaEvent(eFunction.setPlaybackSpeed, zone, speed.ToString());
            return true;
        }

        public bool setPosition(Zone zone, float seconds)
        {
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].setPosition(seconds);
        }

        public bool setVolume(Zone zone, int percent)
        {
            if ((percent>100)||(percent<-2))
                return false;
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].setVolume(percent);
        }

        public bool stop(Zone zone)
        {
            checkInstance(zone);
            return player[zone.AudioDevice.Instance].stop(false);
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

        private static AudioDevice[] _AudioDevices;

        private static MMDeviceCollection devices
        {
            get
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

                // Set default device
                devices_Default = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                return enumerator.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
            }
        }
        private static MMDevice devices_Default = null;

        public AudioDevice[] OutputDevices
        {
            get
            {
                // Return audio devices if already initialized
                if (_AudioDevices != null)
                    return _AudioDevices;

                // Initialize audio devices
                _AudioDevices = new AudioDevice[devices.Count + 1];

                // Set first unit as default one
                _AudioDevices[0] = new AudioDevice(0, AudioDevice_Get, AudioDevice_Set, devices_Default, true);

                // Hook events
                devices_Default.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);

                // Initialize rest of the units
                for (int i = 0; i < devices.Count; i++)
                {   
                    // Try to get the name of the device
                    PropertyStoreProperty prop = devices[i].Properties[PKEY.FriendlyName];
                    int instance = i + 1;
                    if (prop == null)
                        _AudioDevices[instance] = new AudioDevice("Unknown device", instance, AudioDevice_Get, AudioDevice_Set, devices[i]);
                    else
                        _AudioDevices[instance] = new AudioDevice(prop.Value.ToString(), instance, AudioDevice_Get, AudioDevice_Set, devices[i]);

                    // Hook events
                    devices[i].AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
                }
                return _AudioDevices;
            }
        }

        static AudioVolumeNotificationData AudioEvent_PreviousData = null;
        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (AudioEvent_PreviousData != null)
                if (data.MasterVolume == AudioEvent_PreviousData.MasterVolume &&
                    data.Channels == AudioEvent_PreviousData.Channels &&
                    data.EventContext == AudioEvent_PreviousData.EventContext &&
                    data.Muted == AudioEvent_PreviousData.Muted)
                    return;
            AudioEvent_PreviousData = data;

            for (int i = 0; i < _AudioDevices.Length; i++)
            {
                MMDevice device = _AudioDevices[i].Tag as MMDevice;
                if (device != null)
                {
                    // Skip this device is data doesnt match to a device
                    if ((data.MasterVolume != device.AudioEndpointVolume.MasterVolumeLevelScalar) || (device.AudioEndpointVolume.Mute != data.Muted))
                        continue;

                    // Match found, raise event on audiodevice (we raise both the mute and the volume event as separating between them is not easy)
                    _AudioDevices[i].Raise_DataUpdated(AudioDevice.Actions.Mute, data.Muted);
                    _AudioDevices[i].Raise_DataUpdated(AudioDevice.Actions.Volume, ((int)Math.Round(data.MasterVolume * 100)));
                }
            }
        }

        private void AudioDevice_Set(AudioDevice audioDevice, AudioDevice.Actions action, object value)
        {
            MMDevice device = audioDevice.Tag as MMDevice;
            if (device == null)
                return;

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    device.AudioEndpointVolume.Mute = (bool)value;
                    break;
                case AudioDevice.Actions.Volume:
                    {
                        double volumeLevel = Convert.ToDouble(value);
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volumeLevel / 100.0f);
                        device.AudioEndpointVolume.Mute = false;
                    }
                    break;
                default:
                    break;
            }
        }
        private object AudioDevice_Get(AudioDevice audioDevice, AudioDevice.Actions action)
        {
            MMDevice device = audioDevice.Tag as MMDevice;

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    {
                        if (device == null)
                            return false;
                        return device.AudioEndpointVolume.Mute;
                    }

                case AudioDevice.Actions.Volume:
                    {
                        if (device == null)
                            return 0;
                        return (int)Math.Round(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    }
                default:
                    break;
            }

            return null;
        }

        public string pluginDescription
        {
            get
            {
                return "MediaFoundation Media Player";
            }
        }

        public string pluginName
        {
            get
            {
                return "VistaPlayer";
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
        public sealed class AVPlayer : COMBase, IMFAsyncCallback
        {
            // Fields
            //MF
            IMFMediaSession session;
            IMFMediaSource m_pSource;
            public IMFVideoDisplayControl m_pVideoDisplay;
            AutoResetEvent m_hCloseEvent;
            IMFRateControl m_rateControl;
            IMFAudioStreamVolume m_volume;
            //
            public float currentPlaybackRate = 1F;
            public ePlayerStatus currentState = ePlayerStatus.Stopped;
            public int currentVolume = 100;
            //public int instance = -1;
            public Zone zone = null;
            public bool isAudioOnly = true;
            public mediaInfo nowPlaying = new mediaInfo();
            public MediaEvent OnMediaEvent;
            public double pos = -1.0;
            private Thread t;
            private MessageProc sink;
            public bool fullscreen = false;
            public IntPtr drain = IntPtr.Zero;
            private delegate bool PositionCallback(float seconds);
            private delegate bool SpeedCallback(float rate);
            private delegate bool VoidCallback();
            private delegate bool StopCallback(bool BlockStopEvent);
            private delegate bool PlayCallback(Zone zone, string filename);
            private delegate void GetPositionCallback();
            private event PositionCallback OnSetPosition;
            private event StopCallback OnStop;
            private event PlayCallback OnPlay;
            private event SpeedCallback OnSetRate;
            private event GetPositionCallback OnGetPosition;
            private bool _BlockStopEvent = false;

            // Methods
            public AVPlayer(Zone zone)
            {
                //instance = instanceNum;
                this.zone = zone;
                sink = new MessageProc("VistaPlayer:" + zone.Name);
                System.Diagnostics.Debug.WriteLine("VistaPlayer:" + zone.Name.ToString() + " (VistaPlayer) AudioInstance: " + zone.AudioDevice.Instance.ToString() + "[" + zone.AudioDevice.Name + "]");
                
                drain = sink.Handle;
                sink.OnClick += new MessageProc.Click(clicked);
                OnSetPosition += new PositionCallback(AVPlayer_OnSetPosition);
                OnStop += new StopCallback(stop);
                OnSetRate += new SpeedCallback(SetRate);
                OnPlay += new PlayCallback(PlayMovieInWindow);
                OnGetPosition += new GetPositionCallback(getCurrentPos);
                m_hCloseEvent = new AutoResetEvent(false);
                MFExtern.MFStartup(0x10070, MFStartup.Full);
            }

            bool AVPlayer_OnSetPosition(float seconds)
            {
                if (session == null)
                    return false;
                if (session.Start(Guid.Empty, new PropVariant((long)(10000000 * seconds))) != S_Ok)
                    return false;
                if (this.currentState == ePlayerStatus.Paused)
                    session.Pause();
                return true;
            }

            void clicked()
            {
                fullscreen = !fullscreen;
                Resize();
            }

            public bool play(Zone zone, string url)
            {
                stop(true);
                lock (this)
                {
                    //object o = sink.Invoke(OnPlay, new object[] { zone, url });
                    object o = PlayMovieInWindow(zone, url);
                    if ((bool)o == false)
                        return false;
                }
                pos = 0;
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
                if (nowPlaying.Length== 0)
                    nowPlaying.Length = (int)streamDuration;
                OnMediaEvent(eFunction.Play, zone, url);
                if (t == null)
                {
                    t = new Thread(waitForStop);
                    t.Name = String.Format("OMPlayer.VistaPlayer.AVPlayer.{0}", zone);
                    t.Start();
                }
                return true;
            }
            public bool stop(bool BlockStopEvent)
            {
                //if (sink.InvokeRequired)
                //{
                //    object ret = sink.Invoke(OnStop);
                //    if (ret == null)
                //        return false;
                //    return (bool)ret;
                //}
                if (session==null)
                    return false;
                if (isAudioOnly == false)
                {
                    if (m_pVideoDisplay != null)
                        m_pVideoDisplay.SetVideoWindow(IntPtr.Zero);
                    sink.Hide();
                    OnMediaEvent(eFunction.hideVideoWindow, zone, "");
                }
                CloseSession(BlockStopEvent);
                currentState = ePlayerStatus.Stopped;
                return true;
            }
            IMFPresentationClock clock;
            public void getCurrentPos()
            {
                long time = 0;
                if (session != null)
                {
                    if (clock == null)
                    {
                        IMFClock tmpClock;
                        session.GetClock(out tmpClock);
                        clock = tmpClock as IMFPresentationClock;
                        if (clock == null)
                            return;
                    }
                    try
                    {
                        clock.GetTime(out time);
                    }
                    catch (COMException)
                    {
                        time = 0;
                    }
                    // Convert nanoseconds to seconds
                    pos = time / 10000000;
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
                if (fullscreen == true)
                {
                    OpenMobile.Platform.Windows.WindowInfo info = new OpenMobile.Platform.Windows.WindowInfo();
                    OpenMobile.Platform.Windows.Functions.GetWindowInfo((IntPtr)VistaPlayer.theHost.GetWindowHandle(zone.Screen), ref info);
                    if ((info.Style & OpenMobile.Platform.Windows.WindowStyle.ThickFrame) == OpenMobile.Platform.Windows.WindowStyle.ThickFrame)
                        return ResizeVideo(0, 0, info.Window.Width - (info.Window.Width - info.Client.Width), info.Window.Height - (info.Window.Height - info.Client.Height));
                    else
                        return ResizeVideo(0, 0, info.Window.Width + 1, info.Window.Height);
                }
                else
                {
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", zone.Screen.ToString(), out o);
                    if (o == null)
                        return -1;
                    PointF sf = (PointF)o;
                    Rectangle pos = theHost.GetVideoPosition(zone.Screen);
                    return ResizeVideo((int)(pos.Left * sf.X), (int)(pos.Top * sf.Y), (int)(pos.Width * sf.X), (int)(pos.Height * sf.Y));
                }
            }
            private int ResizeVideo(int x,int y,int width, int height)
            {
                MFRect rcDest = new MFRect();
                rcDest.left = 0;
                rcDest.top = 0;
                rcDest.right = width;
                rcDest.bottom = height;
                if (!OpenMobile.Platform.Windows.Functions.SetWindowPos(drain, IntPtr.Zero, x, y, width, height, OpenMobile.Platform.Windows.SetWindowPosFlags.NOACTIVATE))
                    return S_False;
                if (m_pVideoDisplay != null)
                {
                    MFVideoNormalizedRect nRect = new MFVideoNormalizedRect();

                    nRect.left = 0;
                    nRect.right = 1;
                    nRect.top = 0;
                    nRect.bottom = 1;
                    return m_pVideoDisplay.SetVideoPosition(nRect, rcDest);
                }
                return S_False;
            }
            public bool PlayMovieInWindow(Zone zone, string filename)
            {
                try
                {
                    // Create the media session.
                    if (!CreateSession())
                        return false;

                    // Create the media source.
                    if (!CreateMediaSource(filename))
                        return false;

                    IMFTopology pTopology;
                    // Create a partial topology.
                    if (!CreateTopologyFromSource(zone, out pTopology))
                        return false;

                    // Set the topology on the media session.
                    if (session.SetTopology(0, pTopology)!=S_Ok)
                        return false;
                }
                catch (Exception) { return false; }
                currentState = ePlayerStatus.Playing;
                return true;
            }

            private bool CreateTopologyFromSource(Zone zone, out IMFTopology ppTopology)
            {
                IMFTopology pTopology = null;
                IMFPresentationDescriptor pSourcePD = null;
                int cSourceStreams = 0;
                int hr;
                try
                {
                    ppTopology = null;
                    hr=MFExtern.MFCreateTopology(out pTopology);
                    if (hr != S_Ok) return false;
                    hr=m_pSource.CreatePresentationDescriptor(out pSourcePD);
                    if (hr != S_Ok)
                    {
                        SafeRelease(pTopology);
                        return false;
                    }
                    IMFAttributes att = pSourcePD as IMFAttributes;
                    if (att != null)
                    {
                        streamDuration = 0;
                        att.GetUINT64(MediaFoundation.MFAttributesClsid.MF_PD_DURATION, out streamDuration);
                        streamDuration = streamDuration / 10000000;
                    }
                    hr=pSourcePD.GetStreamDescriptorCount(out cSourceStreams);
                    if (hr != S_Ok)
                    {
                        SafeRelease(pTopology);
                        return false;
                    }
                    for (int i = 0; i < cSourceStreams; i++)
                    {
                        if (!AddBranchToPartialTopology(zone, pTopology, pSourcePD, i))
                        {
                            SafeRelease(pTopology);
                            return false;
                        }
                    }
                    ppTopology = pTopology;
                    return true;
                }
                finally
                {
                    SafeRelease(pSourcePD);
                }
            }
            long streamDuration;
            private bool AddBranchToPartialTopology(Zone zone, IMFTopology pTopology, IMFPresentationDescriptor pSourcePD, int iStream)
            {
                IMFStreamDescriptor pSourceSD = null;
                IMFTopologyNode pSourceNode = null;
                IMFTopologyNode pOutputNode = null;
                bool fSelected = false;
                int hr;
                try
                {
                    // Get the stream descriptor for this stream.
                    hr=pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);
                    if (hr != S_Ok) return false;
                    // Create the topology branch only if the stream is selected.
                    // Otherwise, do nothing.
                    if (fSelected)
                    {
                        // Create a source node for this stream.
                        if (!CreateSourceStreamNode(pSourcePD, pSourceSD, out pSourceNode))
                            return false;

                        // Create the output node for the renderer.
                        if (!CreateOutputNode(zone, pSourceSD, out pOutputNode))
                            return false;

                        // Add both nodes to the topology.
                        hr=pTopology.AddNode(pSourceNode);
                        if (hr != S_Ok) return false;
                        hr=pTopology.AddNode(pOutputNode);
                        if (hr != S_Ok) return false;
                        // Connect the source node to the output node.
                        hr=pSourceNode.ConnectOutput(0, pOutputNode, 0);
                        if (hr != S_Ok)
                            return false;
                    }
                    return true;
                }
                finally
                {
                    // Clean up.
                    SafeRelease(pSourceSD);
                    SafeRelease(pSourceNode);
                    SafeRelease(pOutputNode);
                }
            }

            private bool CreateOutputNode(Zone zone, IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
            {
                IMFTopologyNode pNode = null;
                IMFMediaTypeHandler pHandler = null;
                IMFActivate pRendererActivate = null;

                Guid guidMajorType = Guid.Empty;
                int hr;

                // Get the stream ID.
                int streamID = 0;

                try
                {
                    ppNode = null;
                    hr = pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
                    if (hr != S_Ok) return false;
                    // Get the media type handler for the stream.
                    hr = pSourceSD.GetMediaTypeHandler(out pHandler);
                    if (hr != S_Ok) return false;
                    // Get the major media type.
                    hr = pHandler.GetMajorType(out guidMajorType);
                    if (hr != S_Ok) return false;
                    // Create a downstream node.
                    hr = MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pNode);
                    if (hr != S_Ok) return false;
                    // Create an IMFActivate object for the renderer, based on the media type.
                    if (MFMediaType.Audio == guidMajorType)
                    {
                        // Create the audio renderer.
                        hr = MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);
                        if (hr != S_Ok)
                        {
                            SafeRelease(pNode);
                            return false;
                        }
                        if ((zone.AudioDevice.Instance > 0) && (zone.AudioDevice.Instance <= devices.Count))
                            pRendererActivate.SetString(MediaFoundation.MFAttributesClsid.MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ID, devices[zone.AudioDevice.Instance - 1].ID);
                    }
                    else if (MFMediaType.Video == guidMajorType)
                    {
                        OnMediaEvent(eFunction.showVideoWindow, zone, String.Empty);
                        // Create the video renderer.
                        IntPtr rw = (IntPtr)VistaPlayer.theHost.GetWindowHandle(zone.Screen);
                        OpenMobile.Platform.Windows.Functions.SetParent(drain, rw);
                        Resize();
                        sink.Show();
                        hr = MFExtern.MFCreateVideoRendererActivate(drain, out pRendererActivate);
                        if (hr != S_Ok)
                        {
                            SafeRelease(pNode);
                            return false;
                        }
                    }
                    else
                        return false;

                    // Set the IActivate object on the output node.
                    hr = pNode.SetObject(pRendererActivate);
                    if (hr != S_Ok)
                    {
                        SafeRelease(pNode);
                        return false;
                    }
                    // Return the IMFTopologyNode pointer to the caller.
                    ppNode = pNode;
                    return true;
                }
                catch 
                {
                    SafeRelease(pNode);
                    ppNode = pNode;
                    return false;
                }
                finally
                {
                    // Clean up.
                    SafeRelease(pHandler);
                    SafeRelease(pRendererActivate);
                }
            }

            private bool CreateSourceStreamNode(IMFPresentationDescriptor pSourcePD, IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
            {
                IMFTopologyNode pNode = null;
                int hr;
                ppNode = null;
                // Create the source-stream node.
                hr=MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);
                if (hr == S_Ok)
                {
                    // Set attribute: Pointer to the media source.
                    hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);
                    if (hr == S_Ok)
                    {
                        // Set attribute: Pointer to the presentation descriptor.
                        hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);
                        if (hr == S_Ok)
                        {
                            // Set attribute: Pointer to the stream descriptor.
                            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);
                            if (hr == S_Ok)
                            {
                                // Return the IMFTopologyNode pointer to the caller.
                                ppNode = pNode;
                                return true;
                            }
                        }
                    }
                }
                // If we failed, release the pnode
                SafeRelease(pNode);
                return false;
            }
            
            private bool CreateMediaSource(string filename)
            {
                IMFSourceResolver pSourceResolver;
                object pSource;

                // Create the source resolver.
                MFExtern.MFCreateSourceResolver(out pSourceResolver);

                try
                {
                    // Use the source resolver to create the media source.
                    MFObjectType ObjectType = MFObjectType.Invalid;

                    int hr=pSourceResolver.CreateObjectFromURL(
                            filename,                       // URL of the source.
                            MFResolution.MediaSource,   // Create a source object.
                            null,                       // Optional property store.
                            out ObjectType,             // Receives the created object type.
                            out pSource                 // Receives a pointer to the media source.
                        );
                    if (hr != S_Ok)
                        return false;
                    // Get the IMFMediaSource interface from the media source.
                    m_pSource = pSource as IMFMediaSource;
                    object o;
                    MFExtern.MFGetService(session, MFServices.MF_RATE_CONTROL_SERVICE, typeof(IMFRateControl).GUID, out o);
                    m_rateControl = o as IMFRateControl;
                }
                finally
                {
                    // Clean up
                    Marshal.ReleaseComObject(pSourceResolver);
                }
                return true;
            }

            private bool CreateSession()
            {
                CloseSession(false);
                int hr=MFExtern.MFCreateMediaSession(null, out session);
                if (hr != S_Ok) return false;
                hr=session.BeginGetEvent(this, null);
                return (hr == S_Ok);
            }
            int IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
            {
                pdwFlags = MFASync.FastIOProcessingCallback;
                pdwQueue = MFAsyncCallbackQueue.Standard;
                return S_Ok;
            }
            int IMFAsyncCallback.Invoke(IMFAsyncResult pResult)
            {
                int hr;
                IMFMediaEvent pEvent = null;
                MediaEventType meType = MediaEventType.MEUnknown;  // Event type
                int hrStatus = 0;           // Event status
                MFTopoStatus TopoStatus = MFTopoStatus.Invalid; // Used with MESessionTopologyStatus event.

                try
                {
                    // Get the event from the event queue.
                    hr=session.EndGetEvent(pResult, out pEvent);
                    MFError.ThrowExceptionForHR(hr);
                    // Get the event type.
                    hr=pEvent.GetType(out meType);
                    MFError.ThrowExceptionForHR(hr);
                    // Get the event status. If the operation that triggered the event did
                    // not succeed, the status is a failure code.
                    hr=pEvent.GetStatus(out hrStatus);
                    MFError.ThrowExceptionForHR(hr);
                    // Check if the async operation succeeded.
                    if (Succeeded(hrStatus))
                    {
                        // Switch on the event type. Update the internal state of the CPlayer as needed.
                        switch (meType)
                        {
                            case MediaEventType.MESessionTopologyStatus:
                                // Get the status code.
                                int i;
                                pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out i);
                                TopoStatus = (MFTopoStatus)i;
                                switch (TopoStatus)
                                {
                                    case MFTopoStatus.Ready:
                                        OnTopologyReady(pEvent);
                                        break;
                                    default:
                                        // Nothing to do.
                                        break;
                                }
                                break;
                            case MediaEventType.MESessionStarted:
                                OnSessionStarted(pEvent);
                                break;
                            case MediaEventType.MEAudioSessionDeviceRemoved:
                            case MediaEventType.MEAudioSessionServerShutdown:
                                stop(false);
                                break;
                            case MediaEventType.MESessionRateChanged:
                                bool thin=true;
                                m_rateControl.GetRate(ref thin, out currentPlaybackRate);
                                break;
                            case MediaEventType.MESessionPaused:
                                OnSessionPaused(pEvent);
                                break;
                            case MediaEventType.MESessionClosed:
                                OnSessionClosed(pEvent);
                                break;

                            case MediaEventType.MEEndOfPresentation:
                                OnPresentationEnded(pEvent);
                                break;
                        }
                    }
                    else
                    {
                        // The async operation failed. Notify the application
                        //NotifyError(hrStatus);
                    }
                }
                finally
                {
                    // Request another event.
                    if ((meType != MediaEventType.MESessionClosed)&&(session!=null))
                    {
                        hr=session.BeginGetEvent(this, null);
                        MFError.ThrowExceptionForHR(hr);
                    }

                    SafeRelease(pEvent);
                }
                return S_Ok; //<-this feels wrong
            }

            private void OnPresentationEnded(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Transitioning;
                if (crossfade == 0) //blocking events causes problems
                    SafeThread.Asynchronous(delegate() { OnMediaEvent(eFunction.nextMedia, zone, String.Empty); }, VistaPlayer.theHost);
                else
                    stop(false);
                if (!isAudioOnly)
                    OpenMobile.Platform.Windows.Functions.ShowWindow(drain, OpenMobile.Platform.Windows.ShowWindowCommand.HIDE);
            }

            private void OnSessionStarted(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Playing;
                clock = null;
                if (m_volume == null)
                {
                    object o;
                    MFExtern.MFGetService(session, MFServices.MR_STREAM_VOLUME_SERVICE, typeof(IMFAudioStreamVolume).GUID, out o);
                    m_volume = o as IMFAudioStreamVolume;
                }
            }

            private void OnTopologyReady(IMFMediaEvent pEvent)
            {
                object o;
                int hr=MFExtern.MFGetService(
                    session,
                    MFServices.MR_VIDEO_RENDER_SERVICE,
                    typeof(IMFVideoDisplayControl).GUID,
                    out o
                    );
                if (hr == S_Ok)
                {
                    isAudioOnly = false;
                    m_pVideoDisplay = o as IMFVideoDisplayControl;
                    Resize();
                }
                else
                    isAudioOnly = true;
                currentPlaybackRate = 1F;
                session.Start(Guid.Empty, new PropVariant());
            }

            private void OnSessionClosed(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Stopped;
                if (!_BlockStopEvent)
                    OnMediaEvent(eFunction.Stop, zone, String.Empty);
                _BlockStopEvent = false;
                clock = null;
                m_hCloseEvent.Set();
            }

            private void OnSessionPaused(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Paused;
                OnMediaEvent(eFunction.Pause, zone, String.Empty);
            }

            public void CloseSession(bool blockStopEvent)
            {
                _BlockStopEvent = blockStopEvent;
                if (m_pVideoDisplay != null)
                {
                    Marshal.ReleaseComObject(m_pVideoDisplay);
                    m_pVideoDisplay = null;
                }

                if (session != null)
                {
                    if (session.Close()==S_Ok)
                        m_hCloseEvent.WaitOne(3000, true);
                }

                // Complete shutdown operations
                if (m_pSource != null)
                {
                    m_pSource.Shutdown();
                    SafeRelease(m_pSource);
                    m_pSource = null;
                }

                if (m_rateControl != null)
                {
                    SafeRelease(m_rateControl);
                    m_rateControl = null;
                }

                if (m_volume != null)
                {
                    SafeRelease(m_volume);
                    m_volume = null;
                }

                if (session != null)
                {
                    session.Shutdown();
                    SafeRelease(session);
                    Marshal.ReleaseComObject(session);
                    session = null;
                }

                if (clock != null)
                {
                    SafeRelease(clock);
                    clock = null;
                }
            }

            public bool SetRate(float rate)
            {
                //if (sink.InvokeRequired)
                //{
                //    return (bool)sink.Invoke(OnSetRate, new object[] { rate });
                //}
                //else
                {
                    return (m_rateControl.SetRate(true, rate) == S_Ok);
                }
            }
            public bool setVolume(int percent)
            {
                bool negate = false;
                if (m_volume == null)
                    return false;
                if (percent == -1)
                {
                    if (currentVolume < 0)
                        return false;
                    else
                    {
                        negate = true;
                        percent = 0;
                    }
                }
                else if (percent == -2)
                {
                    if (currentVolume < 0)
                        percent = currentVolume * -1;
                    else
                        return false;
                }
                int count;
                try
                {
                    if (m_volume.GetChannelCount(out count) != S_Ok)
                        return false;
                    if (count == 0)
                        return false;
                    float[] vols = new float[count];
                    for (int i = 0; i < vols.Length; i++)
                        vols[i] = percent / 100F;
                    IntPtr volPtr;
                    unsafe
                    {
                        fixed(float* vol = vols)
                        volPtr = new IntPtr((void*)vol);
                    }
                    if (m_volume.SetAllVolumes(count, volPtr) == S_Ok)
                    {
                        if (negate)
                            currentVolume *= -1;
                        else
                            currentVolume = percent;
                        return true;
                    }
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
            private void waitForStop()
            {
                if (m_pSource != null)
                {
                    while (zone != null && m_pSource != null)
                    {
                        Thread.Sleep(1000);
                        if (m_pSource != null)
                        {
                            if (currentState != ePlayerStatus.Stopped)
                                //sink.BeginInvoke(OnGetPosition);
                                getCurrentPos();
                            if ((crossfade > 0) && (isAudioOnly) && ((int)pos == nowPlaying.Length - (crossfade / 1000)))
                            {
                                OnMediaEvent(eFunction.nextMedia, zone, String.Empty);
                            }
                        }
                    }
                }
                t = null;
            }

            internal bool setPosition(float seconds)
            {
                if ((currentState == ePlayerStatus.Stopped) || (currentState == ePlayerStatus.Error))
                    return false;
                if (session==null)
                    return false;
                double old = pos;
                pos = seconds;
                //bool success = (bool)sink.Invoke(OnSetPosition, new object[] { seconds });
                bool success = (bool)AVPlayer_OnSetPosition(seconds);
                if (!success)
                    pos = old;
                return success;
            }

            internal bool pause()
            {
                if ((currentState == ePlayerStatus.Paused) || (currentState == ePlayerStatus.Stopped))
                    return (session.Start(Guid.Empty,new PropVariant())==S_Ok);
                else
                    return (session.Pause()==S_Ok);
            }

            internal bool play()
            {
                if (currentState==ePlayerStatus.Paused)
                {
                    if (session.Start(Guid.Empty, new PropVariant()) == S_Ok)
                        OnMediaEvent(eFunction.Play, zone, String.Empty);
                }
                else if(currentState==ePlayerStatus.Stopped)
                {
                    if (!string.IsNullOrEmpty(nowPlaying.Location))
                        return theHost.execute(eFunction.Play, zone.ToString(), nowPlaying.Location);
                }
                return false;
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
            public new delegate void Click();
            public new Click OnClick;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_LBUTTONUP)
                {
                    OnClick();
                }
                base.WndProc(ref m);
            }
        }
    }
}
