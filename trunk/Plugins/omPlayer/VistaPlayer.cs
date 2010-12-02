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
using OpenMobile.Plugin;
using OpenMobile;
using System.Threading;
using System;
using MediaFoundation;
using System.Windows.Forms;
using OpenMobile.Data;
using System.Collections.Generic;
using OpenMobile.Media;
using MediaFoundation.Misc;
using System.Runtime.InteropServices;
using MediaFoundation.EVR;
using OpenMobile.Threading;
using OpenMobile.Graphics;

namespace OMPlayer
{
    public sealed class VistaPlayer : IAVPlayer
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
                settings = new Settings("OMPlayer Settings");
                using (PluginSettings s = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.AutoResume", "", "Resume Playback at startup", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.AutoResume")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.PlayProtected", "", "Attempt to play protected files", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.PlayProtected")));
                    List<string> crossfadeo = new List<string>(new string[] { "None", "1 Second", "2 Seconds", "3 Seconds", "4 Seconds", "5 Seconds" });
                    List<string> crossfadev = new List<string>(new string[] { "", "1000", "2000", "3000", "4000", "5000" });
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.CrossfadeDuration", "Crossfade", "Crossfade between songs", crossfadeo, crossfadev, s.getSetting("Music.CrossfadeDuration")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Music.MediaKeys", "", "Respond to media keys", Setting.BooleanList, Setting.BooleanList, s.getSetting("Music.MediaKeys")));
                }
                settings.OnSettingChanged += new SettingChanged(changed);
            }
            return settings;
        }

        void changed(Setting s)
        {
            if (s.Name == "Music.CrossfadeDuration")
            {
                if (s.Value != "")
                    crossfade = int.Parse(s.Value);
            }
            using (PluginSettings setting = new PluginSettings())
                setting.setSetting(s.Name, s.Value);
        }
        private void checkInstance(int instance)
        {
            lock (player)
            {
                if (player[instance] == null)
                    sink.Invoke(OnPlayerRequested, new object[] { instance });
            }
        }
        private void createInstance(int instance)
        {
            player[instance] = new AVPlayer(instance);
            player[instance].OnMediaEvent += forwardEvent;
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
                        player[i].CloseSession();
                }
                if (oldPlayer != null)
                    oldPlayer.CloseSession();
                MFExtern.MFShutdown();
            }
            GC.SuppressFinalize(this);
        }
        AVPlayer oldPlayer;
        public void forwardEvent(eFunction function, int instance, string arg)
        {
            if (function == eFunction.nextMedia)
            {
                if (crossfade > 0)
                {
                    lock (player)
                    {
                        AVPlayer tmp = oldPlayer;
                        oldPlayer = player[instance];
                        player[instance] = tmp;
                    }
                    if (player[instance] != null)
                    {
                        player[instance].instance = instance;
                        player[instance].OnMediaEvent += forwardEvent;
                    }
                    oldPlayer.instance = -1;
                    if (OnMediaEvent != null)
                        OnMediaEvent(function, instance, arg);
                    for (int i = 10; i >= 0; i--)
                    {
                        if (player[instance] != null)
                            player[instance].setVolume((10 - i) * 10);
                        oldPlayer.setVolume(i * 10);
                        Thread.Sleep(crossfade / 10);
                    }
                    oldPlayer.OnMediaEvent -= forwardEvent;
                    return;
                }
            }
            if (OnMediaEvent != null)
                OnMediaEvent(function, instance, arg);
        }
        public float getCurrentPosition(int instance)
        {
            checkInstance(instance);
            if (player[instance].currentState == ePlayerStatus.Stopped)
                return -1F;
            return (float)player[instance].pos;
        }

        public mediaInfo getMediaInfo(int instance)
        {
            checkInstance(instance);
            if (player[instance].currentState == ePlayerStatus.Stopped)
                return new mediaInfo();
            else
                return player[instance].nowPlaying;
        }

        public float getPlaybackSpeed(int instance)
        {
            checkInstance(instance);
            return (float)player[instance].currentPlaybackRate;
        }

        public ePlayerStatus getPlayerStatus(int instance)
        {
            checkInstance(instance);
            return player[instance].currentState;
        }

        public int getVolume(int instance)
        {
            checkInstance(instance);
            if (player[instance].currentVolume < 0)
                return -1; //volume muted
            else
                return player[instance].currentVolume;
        }
        private void saveState()
        {
            using (PluginSettings settings = new PluginSettings())
            {
                for (int i = 0; i < theHost.InstanceCount; i++)
                {
                    if ((player != null) && (player[i] != null) && (player[i].nowPlaying != null) && (player[i].pos>0))
                    {
                        if (getPlayerStatus(i) == ePlayerStatus.Stopped)
                        {
                            settings.setSetting("Music.Instance" + i.ToString() + ".LastPlayingURL", "");
                            continue;
                        }
                        settings.setSetting("Music.Instance" + i.ToString() + ".LastPlayingURL", player[i].nowPlaying.Location);
                        settings.setSetting("Music.Instance" + i.ToString() + ".LastPlayingPosition", player[i].pos.ToString());
                    }
                    else
                    {
                        settings.setSetting("Music.Instance" + i.ToString() + ".LastPlayingURL", "");
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
                int inst = theHost.instanceForScreen(int.Parse(arg1));
                if (player[inst] != null)
                    player[inst].Resize();
            }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        MessageProc sink;
        private delegate void CreateNewPlayer(int instance);
        private event CreateNewPlayer OnPlayerRequested;
        public eLoadStatus initialize(IPluginHost host)
        {
            if (Environment.OSVersion.Version.Major < 6)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;
            theHost = host;
            sink = new MessageProc();
            IntPtr tmp = sink.Handle;
            player = new AVPlayer[theHost.InstanceCount];
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            host.OnKeyPress += new KeyboardEvent(host_OnKeyPress);
            OnPlayerRequested += new CreateNewPlayer(createInstance);
            using (PluginSettings setting = new PluginSettings())
            {
                setting.setSetting("Default.AVPlayer.Files", "OMPlayer2");
                string cf = setting.getSetting("Music.CrossfadeDuration");
                if (cf != "")
                    crossfade = int.Parse(cf);
                if (setting.getSetting("Music.AutoResume") == "True")
                {
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            int k = theHost.instanceForScreen(i);
                            host.execute(eFunction.loadAVPlayer, k.ToString(), "OMPlayer2");
                            checkInstance(k);
                            player[k].currentVolume = 0;
                            string lastUrl = setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingURL");
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
            }
            return eLoadStatus.LoadSuccessful;
        }

        bool host_OnKeyPress(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg)
        {
            if (type != eKeypressType.KeyDown)
                return false;
            if ((settings == null) || (settings.Count < 4))
                return false;
            if (settings[3].Value == "True")
            {
                switch (arg.Key)
                {
                    case OpenMobile.Input.Key.PlayPause:
                        object o = new object();
                        theHost.getData(eGetData.GetMediaStatus, "", theHost.instanceForScreen(0).ToString(), out o);
                        if (o == null)
                            return false;
                        if (o.GetType() == typeof(ePlayerStatus))
                        {
                            ePlayerStatus status = (ePlayerStatus)o;
                            if (status == ePlayerStatus.Playing)
                                theHost.execute(eFunction.Pause, theHost.instanceForScreen(0).ToString());
                            else
                            {
                                if ((status == ePlayerStatus.FastForwarding) || (status == ePlayerStatus.Rewinding))
                                    theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(0).ToString(), "1");
                                else
                                    theHost.execute(eFunction.Play, theHost.instanceForScreen(0).ToString());
                            }
                        }
                        else if (o.GetType() == typeof(stationInfo))
                            theHost.execute(eFunction.Pause, theHost.instanceForScreen(0).ToString());
                        return true;
                    case OpenMobile.Input.Key.Pause:
                        theHost.execute(eFunction.Pause, theHost.instanceForScreen(0).ToString());
                        return true;
                    case OpenMobile.Input.Key.TrackNext:
                        if (theHost.execute(eFunction.stepForward, theHost.instanceForScreen(0).ToString()) == false)
                            theHost.execute(eFunction.nextMedia, theHost.instanceForScreen(0).ToString());
                        return true;
                    case OpenMobile.Input.Key.TrackPrevious:
                        if (theHost.execute(eFunction.stepBackward, theHost.instanceForScreen(0).ToString()) == false)
                            theHost.execute(eFunction.previousMedia, theHost.instanceForScreen(0).ToString());
                        return true;
                    case OpenMobile.Input.Key.Stop:
                        theHost.execute(eFunction.Stop, theHost.instanceForScreen(0).ToString());
                        theHost.execute(eFunction.unloadTunedContent, theHost.instanceForScreen(0).ToString());
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
                for (int i = 0; i < player.Length; i++)
                    if ((player[i] != null) && (player[i].currentVolume > k))
                    {
                        setVolume(i, k);
                        Thread.Sleep(150);
                    }
            fading = false;
        }
        private void fadeIn()
        {
            for (int k = 1; k < 10; k++)
                for (int i = 0; i < player.Length; i++)
                    if (player[i] != null)
                    {
                        setVolume(i, (k * 10));
                        Thread.Sleep(150);
                    }
        }
        public bool pause(int instance)
        {
            checkInstance(instance);
            return player[instance].pause();
        }
        public bool SetVideoVisible(int instance, bool visible)
        {
            checkInstance(instance);
            //TODO
            return false;
        }
        public bool play(int instance)
        {
            checkInstance(instance);
            return player[instance].play();
        }

        public bool play(int instance, string url, eMediaType type)
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
                if (url.EndsWith(".m4p"))
                {
                    using (PluginSettings s = new PluginSettings())
                        if (s.getSetting("Music.PlayProtected") != "True")
                            return false;
                }
                if (url.EndsWith(".IFO"))
                {
                    theHost.execute(eFunction.Play, instance.ToString(), System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(url)));
                    return false;
                }
                checkInstance(instance);
                return player[instance].play(url);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool setPlaybackSpeed(int instance, float speed)
        {
            checkInstance(instance);
            if (player[instance].SetRate(speed) != 0)
                return false;
            if (player[instance].currentPlaybackRate > 1.0)
            {
                player[instance].currentState = ePlayerStatus.FastForwarding;
            }
            else if (player[instance].currentPlaybackRate < 1.0)
            {
                player[instance].currentState = ePlayerStatus.Rewinding;
            }
            else if ((player[instance].currentState == ePlayerStatus.FastForwarding) || (player[instance].currentState == ePlayerStatus.Rewinding))
            {
                player[instance].currentState = ePlayerStatus.Playing;
            }
            OnMediaEvent(eFunction.setPlaybackSpeed, instance, speed.ToString());
            return true;
        }

        public bool setPosition(int instance, float seconds)
        {
            checkInstance(instance);
            return player[instance].setPosition(seconds);
        }

        public bool setVolume(int instance, int percent)
        {
            checkInstance(instance);
            return player[instance].setVolume(percent);
        }

        public bool stop(int instance)
        {
            checkInstance(instance);
            return player[instance].stop();
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
                //TODO
                return new string[] { "Default Device" };
            }
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
                return "OMPlayer2";
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
            IMFVideoDisplayControl m_pVideoDisplay;
            AutoResetEvent m_hCloseEvent;
            IMFRateControl m_rateControl;
            //
            public float currentPlaybackRate = 1F;
            public ePlayerStatus currentState = ePlayerStatus.Stopped;
            public int currentVolume = 100;
            public int instance = -1;
            public bool isAudioOnly = true;
            public mediaInfo nowPlaying = new mediaInfo();
            public MediaEvent OnMediaEvent;
            public double pos = -1.0;
            private Thread t;
            private MessageProc sink;
            private bool fullscreen = false;
            IntPtr drain = IntPtr.Zero;
            private delegate bool PositionCallback(float seconds);
            private delegate int SpeedCallback(float rate);
            private delegate bool VoidCallback();
            private delegate bool PlayCallback(string filename);
            private delegate void GetPositionCallback();
            private event PositionCallback OnSetPosition;
            private event VoidCallback OnStop;
            private event PlayCallback OnPlay;
            private event SpeedCallback OnSetRate;
            private event GetPositionCallback OnGetPosition;
            // Methods
            public AVPlayer(int instanceNum)
            {
                instance = instanceNum;
                sink = new MessageProc();
                drain = sink.Handle;
                sink.OnClick += new MessageProc.Click(clicked);
                OnSetPosition += new PositionCallback(AVPlayer_OnSetPosition);
                OnStop += new VoidCallback(stop);
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
                session.Start(Guid.Empty, new PropVariant((long)(10000000 * seconds)));
                return true;
            }

            void clicked()
            {
                fullscreen = !fullscreen;
                Resize();
            }

            public bool play(string url)
            {
                stop();
                lock (this)
                {
                    object o = sink.Invoke(OnPlay, new object[] { url });
                    if ((bool)o == false)
                        return false;
                }
                pos = 0;
                if (url.EndsWith(".cda") == true)
                {
                    nowPlaying = CDDB.getSongInfo(theHost, url);
                    if (nowPlaying == null)
                        nowPlaying = new mediaInfo();
                    if (nowPlaying.coverArt == null)
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
                        nowPlaying.coverArt = TagReader.getCoverFromDB(nowPlaying.Artist, nowPlaying.Album, theHost);
                    if (nowPlaying.coverArt == null)
                        nowPlaying.coverArt = TagReader.getFolderImage(nowPlaying.Location);
                    if (nowPlaying.coverArt == null)
                        nowPlaying.coverArt = TagReader.getLastFMImage(nowPlaying.Artist, nowPlaying.Album);
                }
                OnMediaEvent(eFunction.Play, instance, url);
                if (t == null)
                {
                    t = new Thread(waitForStop);
                    t.Name = instance.ToString();
                    t.Start();
                }
                return true;
            }
            public bool stop()
            {
                if (sink.InvokeRequired)
                {
                    object ret = sink.Invoke(OnStop);
                    if (ret == null)
                        return false;
                    return (bool)ret;
                }
                if (session==null)
                    return false;
                CloseSession();
                currentState = ePlayerStatus.Stopped;
                if (isAudioOnly == false)
                {
                    //TODO
                    //DsError.ThrowExceptionForHR(videoWindow.put_Visible(OABool.False));
                    //DsError.ThrowExceptionForHR(videoWindow.put_Owner(IntPtr.Zero));
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        if (theHost.instanceForScreen(i) == this.instance)
                            theHost.sendMessage("UI", "OMPlayer2", "HideMediaControls" + i.ToString());
                }
                isAudioOnly = true;
                return true;
            }
            IMFPresentationClock clock;
            public void getCurrentPos()
            {
                long time;
                if (session != null)
                {
                    if (clock == null)
                    {
                        IMFClock tmpClock;
                        session.GetClock(out tmpClock);
                        clock = (IMFPresentationClock)tmpClock;
                        if (clock == null)
                            return;
                    }
                    clock.GetTime(out time);
                    pos = time/10000000;
                }
            }
            private int screen()
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                        return i;
                }
                return 0;
            }
            public int Resize()
            {
                if (fullscreen == true)
                {
                    OpenMobile.Platform.Windows.WindowInfo info = new OpenMobile.Platform.Windows.WindowInfo();
                    OpenMobile.Platform.Windows.Functions.GetWindowInfo((IntPtr)VistaPlayer.theHost.UIHandle(screen()), ref info);
                    if ((info.Style & OpenMobile.Platform.Windows.WindowStyle.ThickFrame) == OpenMobile.Platform.Windows.WindowStyle.ThickFrame)
                        return ResizeVideo(0, 0, info.Window.Width - (info.Window.Width - info.Client.Width), info.Window.Height - (info.Window.Height - info.Client.Height));
                    else
                        return ResizeVideo(0, 0, info.Window.Width + 1, info.Window.Height);
                }
                else
                {
                    object o;
                    theHost.getData(eGetData.GetScaleFactors, "", screen().ToString(), out o);
                    if (o == null)
                        return -1;
                    PointF sf = (PointF)o;
                    return ResizeVideo((int)(theHost.VideoPosition.Left * sf.X), (int)(theHost.VideoPosition.Top * sf.Y), (int)(theHost.VideoPosition.Width * sf.X), (int)(theHost.VideoPosition.Height * sf.Y));
                }
            }
            private int ResizeVideo(int x,int y,int width, int height)
            {
                int hr = S_Ok;
                TRACE(string.Format("ResizeVideo: {0}x{1}", width, height));

                if (m_pVideoDisplay != null)
                {
                    try
                    {
                        MFRect rcDest = new MFRect();
                        MFVideoNormalizedRect nRect = new MFVideoNormalizedRect();

                        nRect.left = 0;
                        nRect.right = 1;
                        nRect.top = 0;
                        nRect.bottom = 1;
                        rcDest.left = x;
                        rcDest.top = y;
                        rcDest.right = width;
                        rcDest.bottom = height;

                        m_pVideoDisplay.SetVideoPosition(nRect, rcDest);
                    }
                    catch (Exception ce)
                    {
                        hr = Marshal.GetHRForException(ce);
                    }
                }

                return hr;
            }
            public bool PlayMovieInWindow(string filename)
            {
                try
                {
                    // Create the media session.
                    CreateSession();

                    // Create the media source.
                    CreateMediaSource(filename);

                    IMFTopology pTopology;
                    // Create a partial topology.
                    CreateTopologyFromSource(out pTopology);

                    // Set the topology on the media session.
                    session.SetTopology(0, pTopology);
                }
                catch (Exception) { return false; }
                currentState = ePlayerStatus.Playing;
                return true;
            }

            private void CreateTopologyFromSource(out IMFTopology ppTopology)
            {
                IMFTopology pTopology = null;
                IMFPresentationDescriptor pSourcePD = null;
                int cSourceStreams = 0;

                try
                {
                    MFExtern.MFCreateTopology(out pTopology);
                    m_pSource.CreatePresentationDescriptor(out pSourcePD);
                    pSourcePD.GetStreamDescriptorCount(out cSourceStreams);
                    for (int i = 0; i < cSourceStreams; i++)
                        AddBranchToPartialTopology(pTopology, pSourcePD, i);
                    ppTopology = pTopology;
                }
                catch
                {
                    SafeRelease(pTopology);
                    throw;
                }
                finally
                {
                    SafeRelease(pSourcePD);
                }
            }

            private void AddBranchToPartialTopology(IMFTopology pTopology, IMFPresentationDescriptor pSourcePD, int iStream)
            {
                IMFStreamDescriptor pSourceSD = null;
                IMFTopologyNode pSourceNode = null;
                IMFTopologyNode pOutputNode = null;
                bool fSelected = false;

                try
                {
                    // Get the stream descriptor for this stream.
                    pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);

                    // Create the topology branch only if the stream is selected.
                    // Otherwise, do nothing.
                    if (fSelected)
                    {
                        // Create a source node for this stream.
                        CreateSourceStreamNode(pSourcePD, pSourceSD, out pSourceNode);

                        // Create the output node for the renderer.
                        CreateOutputNode(pSourceSD, out pOutputNode);

                        // Add both nodes to the topology.
                        pTopology.AddNode(pSourceNode);
                        pTopology.AddNode(pOutputNode);

                        // Connect the source node to the output node.
                        pSourceNode.ConnectOutput(0, pOutputNode, 0);
                    }
                }
                finally
                {
                    // Clean up.
                    SafeRelease(pSourceSD);
                    SafeRelease(pSourceNode);
                    SafeRelease(pOutputNode);
                }
            }

            private void CreateOutputNode(IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
            {
                IMFTopologyNode pNode = null;
                IMFMediaTypeHandler pHandler = null;
                IMFActivate pRendererActivate = null;

                Guid guidMajorType = Guid.Empty;
                int hr = S_Ok;

                // Get the stream ID.
                int streamID = 0;

                try
                {
                    try
                    {
                        pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
                    }
                    catch
                    {
                        TRACE("IMFStreamDescriptor::GetStreamIdentifier" + hr.ToString());
                    }

                    // Get the media type handler for the stream.
                    pSourceSD.GetMediaTypeHandler(out pHandler);

                    // Get the major media type.
                    pHandler.GetMajorType(out guidMajorType);

                    // Create a downstream node.
                    MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pNode);

                    // Create an IMFActivate object for the renderer, based on the media type.
                    if (MFMediaType.Audio == guidMajorType)
                        // Create the audio renderer.
                        MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);
                    else if (MFMediaType.Video == guidMajorType)
                    {
                        // Create the video renderer.
                        IntPtr video = (IntPtr)VistaPlayer.theHost.UIHandle(getFirstScreen(instance));
                        MFExtern.MFCreateVideoRendererActivate(video, out pRendererActivate);
                    }
                    else
                        throw new COMException("Unknown format", E_Fail);

                    // Set the IActivate object on the output node.
                    pNode.SetObject(pRendererActivate);

                    // Return the IMFTopologyNode pointer to the caller.
                    ppNode = pNode;
                }
                catch
                {
                    // If we failed, release the pNode
                    SafeRelease(pNode);
                    throw;
                }
                finally
                {
                    // Clean up.
                    SafeRelease(pHandler);
                    SafeRelease(pRendererActivate);
                }
            }

            private void CreateSourceStreamNode(IMFPresentationDescriptor pSourcePD, IMFStreamDescriptor pSourceSD, out IMFTopologyNode ppNode)
            {
                IMFTopologyNode pNode = null;

                try
                {
                    // Create the source-stream node.
                    MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);

                    // Set attribute: Pointer to the media source.
                    pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);

                    // Set attribute: Pointer to the presentation descriptor.
                    pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);

                    // Set attribute: Pointer to the stream descriptor.
                    pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);

                    // Return the IMFTopologyNode pointer to the caller.
                    ppNode = pNode;
                }
                catch
                {
                    // If we failed, release the pnode
                    SafeRelease(pNode);
                    throw;
                }
            }
            bool pausable;
            bool seekable;
            private void CreateMediaSource(string filename)
            {
                IMFSourceResolver pSourceResolver;
                object pSource;

                // Create the source resolver.
                MFExtern.MFCreateSourceResolver(out pSourceResolver);

                try
                {
                    // Use the source resolver to create the media source.
                    MFObjectType ObjectType = MFObjectType.Invalid;

                    pSourceResolver.CreateObjectFromURL(
                            filename,                       // URL of the source.
                            MFResolution.MediaSource,   // Create a source object.
                            null,                       // Optional property store.
                            out ObjectType,             // Receives the created object type.
                            out pSource                 // Receives a pointer to the media source.
                        );

                    // Get the IMFMediaSource interface from the media source.
                    m_pSource = (IMFMediaSource)pSource;
                    object o;
                    MFExtern.MFGetService(session, MFServices.MF_RATE_CONTROL_SERVICE, typeof(IMFRateControl).GUID, out o);
                    m_rateControl = (IMFRateControl)o;
                }
                finally
                {
                    // Clean up
                    Marshal.ReleaseComObject(pSourceResolver);
                }
            }

            private void CreateSession()
            {
                CloseSession();
                MFExtern.MFCreateMediaSession(null, out session);
                session.BeginGetEvent(this, null);
            }
            void IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
            {
                pdwFlags = MFASync.FastIOProcessingCallback;
                pdwQueue = MFAsyncCallbackQueue.Standard;
            }
            void IMFAsyncCallback.Invoke(IMFAsyncResult pResult)
            {
                IMFMediaEvent pEvent = null;
                MediaEventType meType = MediaEventType.MEUnknown;  // Event type
                int hrStatus = 0;           // Event status
                MFTopoStatus TopoStatus = MFTopoStatus.Invalid; // Used with MESessionTopologyStatus event.

                try
                {
                    // Get the event from the event queue.
                    session.EndGetEvent(pResult, out pEvent);

                    // Get the event type.
                    pEvent.GetType(out meType);

                    // Get the event status. If the operation that triggered the event did
                    // not succeed, the status is a failure code.
                    pEvent.GetStatus(out hrStatus);

                    TRACE(string.Format("Media event: " + meType.ToString()));

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
                        session.BeginGetEvent(this, null);
                    }

                    SafeRelease(pEvent);
                }
            }

            private void OnPresentationEnded(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Transitioning;
                if (crossfade == 0) //blocking events causes problems
                    SafeThread.Asynchronous(delegate() { OnMediaEvent(eFunction.nextMedia, instance, ""); }, VistaPlayer.theHost);
                else
                    stop();
            }

            private void OnSessionStarted(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Playing;
            }

            private void OnTopologyReady(IMFMediaEvent pEvent)
            {
                object o;
                try
                {
                    MFExtern.MFGetService(
                        session,
                        MFServices.MR_VIDEO_RENDER_SERVICE,
                        typeof(IMFVideoDisplayControl).GUID,
                        out o
                        );
                    if (o != null)
                    {
                        isAudioOnly = false;
                        m_pVideoDisplay = o as IMFVideoDisplayControl;
                        Resize();
                    }
                    else
                        isAudioOnly = true;
                }
                catch (InvalidCastException)
                {
                    m_pVideoDisplay = null;
                    isAudioOnly = true;
                }

                try
                {
                    session.Start(Guid.Empty, new PropVariant());
                }
                catch (Exception ce)
                {
                    int hr = Marshal.GetHRForException(ce);
                }
            }

            private void OnSessionClosed(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Stopped;
                OnMediaEvent(eFunction.Stop, instance, "");
                m_hCloseEvent.Set();
            }

            private void OnSessionPaused(IMFMediaEvent pEvent)
            {
                currentState = ePlayerStatus.Paused;
                OnMediaEvent(eFunction.Pause, instance, "");
            }

            public void CloseSession()
            {
                if (m_pVideoDisplay != null)
                {
                    Marshal.ReleaseComObject(m_pVideoDisplay);
                    m_pVideoDisplay = null;
                }

                if (session != null)
                {
                    session.Close();

                    // Wait for the close operation to complete
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

                if (session != null)
                {
                    session.Shutdown();
                    Marshal.ReleaseComObject(session);
                    session = null;
                }
            }

            private int getFirstScreen(int instance)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                    if (theHost.instanceForScreen(i) == instance)
                        return i;
                return 0;
            }
            public int SetRate(float rate)
            {
                if (sink.InvokeRequired)
                {
                    return (int)sink.Invoke(OnSetRate, new object[] { rate });
                }
                else
                {
                    bool thin = true;
                    m_rateControl.SetRate(thin, rate);
                    m_rateControl.GetRate(ref thin, out currentPlaybackRate);
                    return (rate == currentPlaybackRate)?-1:0;
                }
            }
            public bool setVolume(int percent)
            {
                return true;
                //TODO
            }
            private void waitForStop()
            {
                if (m_pSource != null)
                {
                    while (instance >= 0)
                    {
                        Thread.Sleep(1000);
                        if (m_pSource != null)
                        {
                            if (currentState!=ePlayerStatus.Stopped)
                                sink.Invoke(OnGetPosition);
                            if ((crossfade > 0) && (isAudioOnly) && ((int)pos == nowPlaying.Length - (crossfade / 1000)))
                            {
                                OnMediaEvent(eFunction.nextMedia, instance, "");
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
                bool success = (bool)sink.Invoke(OnSetPosition, new object[] { seconds });
                if (!success)
                    pos = old;
                return success;
            }

            internal bool pause()
            {
                if ((currentState == ePlayerStatus.Paused) || (currentState == ePlayerStatus.Stopped))
                {
                    session.Start(Guid.Empty,new PropVariant());
                    return true;
                }
                else
                {
                    session.Pause();
                    return true;
                }
            }

            internal bool play()
            {
                if (currentState==ePlayerStatus.Paused)
                {
                    session.Start(Guid.Empty, new PropVariant());
                    return true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(nowPlaying.Location))
                        return theHost.execute(eFunction.Play, instance.ToString(), nowPlaying.Location);
                }
                return false;
            }
        }

        public sealed class MessageProc : Form
        {
            public new delegate void Click();
            public new Click OnClick;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_LBUTTONUP)
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
