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
using OpenMobile.Media;
using OpenMobile.Plugin;
using System.Drawing;

namespace OMPlayer
{
    public sealed class OMPlayer : IAVPlayer, IDisposable
{
    // Fields
    private static IntPtr hDrain = IntPtr.Zero;
    private AVPlayer[] player;
    private static IPluginHost theHost;
    static int WM_Graph_Notify = 0x8001;
    static int WM_LBUTTONUP = 0x0202;

    // Events
    public event MediaEvent OnMediaEvent;

    private void checkInstance(int instance)
    {
        if (player[instance] == null)
        {
            player[instance] = new AVPlayer(instance);
            player[instance].OnMediaEvent+= new MediaEvent(forwardEvent);
        }
    }
    public void Dispose()
    {
        if (player != null)
        {
            for (int i = 0; i < player.Length; i++)
            {
                if (player[i] != null)
                    player[i].CloseClip();
            }
        }
        GC.SuppressFinalize(this);
    }

    public void forwardEvent(eFunction function, int instance, string arg)
    {
        if (OnMediaEvent != null)
        {
            OnMediaEvent(function, instance, arg);
        }
    }

    public float getCurrentPosition(int instance)
    {
        checkInstance(instance);
        if (player[instance].mediaPosition == null)
        {
            return -1F;
        }
        if ((player[instance].currentState == ePlayerStatus.Stopped) || (player[instance].currentState == ePlayerStatus.Ready))
        {
            return 0F;
        }
        return (float) player[instance].pos;
    }

    public static IMoniker getDevMoniker(int dev)
    {
        DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
        List<IMoniker> lst = new List<IMoniker>();
        for (int i = 0; i < d.Length; i++)
        {
            if (d[i].Name.Contains("DirectSound"))
                lst.Add(d[i].Mon);
        }
        if ((dev < lst.Count)&&(dev>=0))
            return lst[dev];
        return lst[0];
    }

    public mediaInfo getMediaInfo(int instance)
    {
        checkInstance(instance);
        return player[instance].nowPlaying;
    }

    public float getPlaybackSpeed(int instance)
    {
        checkInstance(instance);
        return (float) player[instance].currentPlaybackRate;
    }

    public ePlayerStatus getPlayerStatus(int instance)
    {
        checkInstance(instance);
        return player[instance].currentState;
    }

    public int getVolume(int instance)
    {
        checkInstance(instance);
        if (player[instance].currentVolume<0)
            return -1; //volume muted
        else
            return player[instance].currentVolume;
    }
    private void saveState()
    {
        using (PluginSettings settings = new PluginSettings())
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                int k = theHost.instanceForScreen(i);
                if ((player[k] != null) && (player[k].nowPlaying != null))
                {
                    if ((getPlayerStatus(k) == ePlayerStatus.Stopped) || (getPlayerStatus(k) == ePlayerStatus.Ready))
                    {
                        settings.setSetting("Music.Instance" + k.ToString() + ".LastPlayingURL", "");
                        continue;
                    }
                    settings.setSetting("Music.Instance" + k.ToString() + ".LastPlayingURL", player[k].nowPlaying.Location);
                    double position;
                    if (player[k].mediaPosition != null)
                    {
                        player[k].mediaPosition.get_CurrentPosition(out position);
                        settings.setSetting("Music.Instance" + k.ToString() + ".LastPlayingPosition", position.ToString());
                    }
                    else
                    {
                        settings.setSetting("Music.Instance" + k.ToString() + ".LastPlayingURL", "");
                    }
                }
                else
                {
                    settings.setSetting("Music.Instance" + k.ToString() + ".LastPlayingURL", "");
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
                player[inst].Resize(1, 1);
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
    public eLoadStatus initialize(IPluginHost host)
    {
        theHost = host;
        player = new AVPlayer[theHost.InstanceCount];
        host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
        host.OnPowerChange += new PowerEvent(host_OnPowerChange);
        using (PluginSettings setting = new PluginSettings())
        {
            setting.setSetting("Default.AVPlayer.Files", "OMPlayer");
            if (setting.getSetting("Music.AutoResume") == "True")
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    int k = theHost.instanceForScreen(i);
                    host.execute(eFunction.loadAVPlayer, k.ToString(), "OMPlayer");
                    checkInstance(k);
                    player[k].currentVolume = 0;
                    play(k, setting.getSetting("Music.Instance"+k.ToString()+".LastPlayingURL"), eMediaType.Local);
                    player[k].currentVolume = 100;
                    if (player[k].mediaPosition == null)
                        return eLoadStatus.LoadSuccessful;
                    double pos;
                    if (double.TryParse(setting.getSetting("Music.Instance" + k.ToString() + ".LastPlayingPosition"), out pos) == true)
                    {
                        if (pos > 1)
                            pos -= 1;
                        player[k].mediaPosition.put_CurrentPosition(pos);
                    }
                }
                fadeIn();
            }
        }
        return eLoadStatus.LoadSuccessful;
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
        else if (type == ePowerEvent.ShutdownPending)
        {
            saveState();
            fadeout();
        }
    }

    private void fadeout()
    {
        for (int k = 10; k >0; k--)
            for (int i = 0; i < player.Length; i++)
                if (player[i] != null)
                {
                    setVolume(i, (k * 10));
                    Thread.Sleep(150);
                }
    }
    private void fadeIn()
    {
        for (int k = 0; k < 10; k++)
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
        if (player[instance].mediaControl == null)
            return false;
        if ((player[instance].currentState == ePlayerStatus.Paused) || (player[instance].currentState == ePlayerStatus.Stopped))
        {
            if (player[instance].mediaControl.Run() >= 0)
            {
                player[instance].currentState = ePlayerStatus.Playing;
                OnMediaEvent(eFunction.Play, instance, "");
                return true;
            }
        }
        else
        {
            if (player[instance].mediaControl.Pause() >= 0)
            {
                player[instance].currentState = ePlayerStatus.Paused;
                OnMediaEvent(eFunction.Pause, instance, "");
                return true;
            }
        }
        return false;
    }
    public bool SetVideoVisible(int instance, bool visible)
    {
        checkInstance(instance);
        if (visible == false)
            return (player[instance].videoWindow.put_Visible(OABool.False)==0);
        else
            if ((player[instance].currentState!=ePlayerStatus.Ready)&&(player[instance].currentState==ePlayerStatus.Stopped))
                return (player[instance].videoWindow.put_Visible(OABool.True)==0);
        return false;
    }
    public bool play(int instance)
    {
        checkInstance(instance);
        if ((player[instance].mediaControl != null) && (player[instance].mediaControl.Run() >= 0))
        {
            player[instance].currentState = ePlayerStatus.Playing;
            OnMediaEvent(eFunction.Play, instance, "");
            return true;
        }
        return false;
    }

    public bool play(int instance, string url, eMediaType type)
    {
        switch (type)
        {
            case eMediaType.BluetoothResource:
            case eMediaType.BluRay:
            case eMediaType.HDDVD:
            case eMediaType.HTTPUrl:
            case eMediaType.iPodiPhone:
            case eMediaType.RTSPUrl:
            case eMediaType.MMSUrl:
            case eMediaType.DVD:
                return false;
        }
        try
        {
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
        if (player[instance].SetRate((double)speed) != 0)
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
        if ((player[instance].currentState == ePlayerStatus.Stopped) || (player[instance].currentState == ePlayerStatus.Ready) || (player[instance].currentState == ePlayerStatus.Error))
            return false;
        if ((player[instance].mediaControl == null) || (player[instance].mediaPosition == null))
            return false;
        player[instance].pos = seconds;
        return (0 == player[instance].mediaPosition.put_CurrentPosition((double) seconds));
    }

    public bool setVolume(int instance, int percent)
    {
        checkInstance(instance);
        if ((player[instance].graphBuilder != null) && (player[instance].basicAudio != null))
        {
            if ((percent >= 0) && (percent <= 100))
            {
                player[instance].currentVolume = percent;
                if (percent == 0) //prevent log(0)
                    return (player[instance].basicAudio.put_Volume(-10000) == 0);
                return (player[instance].basicAudio.put_Volume((int)(2000 * Math.Log10(Math.Pow((percent / 100.0),2)))) == 0);
            }
            if (percent == -1)
            {
                if (player[instance].currentVolume>0)
                    player[instance].currentVolume *= -1;
                return (player[instance].basicAudio.put_Volume(-10000)==0);
            }
        }
        return false;
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

    public string[] OutputDevices
    {
        get
        {
            DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
            List<string> lst = new List<string>();
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i].Name.Contains("DirectSound"))
                {
                    lst.Add(d[i].Name.Replace("DirectSound", "").Replace(":", ""));
                }
            }
            return lst.ToArray();
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
        private int instance = -1;
        public bool isAudioOnly = false;
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
        private bool fullscreen = false;
        IntPtr drain = IntPtr.Zero;
        // Methods
        public AVPlayer(int instanceNum)
        {
            instance = instanceNum;
            sink = new MessageProc();
            sink.OnClick += new MessageProc.Click(clicked);
            sink.OnEvent += new MessageProc.eventOccured(eventOccured);
            drain = sink.Handle;
        }

        void clicked()
        {
            fullscreen = !fullscreen;
            Resize(1, 1);
        }

        public void eventOccured(int instance)
        {
            EventCode e;
            IntPtr arg1;
            IntPtr arg2;
            FilterState fs;
            mediaEventEx.GetEvent(out e, out arg1, out arg2, 0);
            switch (e)
            {
                case EventCode.Complete:
                    stop();
                    OnMediaEvent(eFunction.nextMedia, instance, "");
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
                            OnMediaEvent(eFunction.Pause, instance, "");
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
                    }
                    else
                    {
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
            }
        }
        public bool play(string url)
        {
            lock (this)
            {
                if (currentState == ePlayerStatus.Playing)
                    stop();
                if (PlayMovieInWindow(url) == false)
                    return false;
            }
            pos = 0;
            if (url.EndsWith(".cda")==true)
            {
                nowPlaying = CDDB.getSongInfo(url);
                if (nowPlaying == null)
                    nowPlaying = new mediaInfo();
                nowPlaying.coverArt = theHost.getSkinImage("Discs|AudioCD").image;
            }
            else
            {
                nowPlaying = TagReader.getInfo(url);
                if (nowPlaying.coverArt == null)
                    nowPlaying.coverArt = TagReader.getFolderImage(nowPlaying.Location);
                if (nowPlaying.coverArt == null)
                    nowPlaying.coverArt = TagReader.getLastFMImage(nowPlaying.Artist, nowPlaying.Album);
            }
            if (nowPlaying.Length==0)
            {
                double dur;
                mediaPosition.get_Duration(out dur);
                nowPlaying.Length = (int)dur;
            }
            OnMediaEvent(eFunction.Play, instance, url);
            if ((t != null)&&(t.ExecutionContext==null)) //No idea why this works but it seems to
                try
                {
                    t.Abort(); //since we're no longer playing...stop looking for the end of the song
                }
                catch (Exception) { }
            t = new Thread(new ThreadStart(waitForStop));
            t.Name = instance.ToString();
            t.Start();
            return true;
        }
        public bool stop()
        {
            if ((currentState == ePlayerStatus.Paused) || (currentState == ePlayerStatus.Playing))
            {
            retry:
                lock (this)
                {
                    if ((mediaControl == null) || (mediaSeeking == null))
                        return false;
                    try
                    {
                        currentState = ePlayerStatus.Ready;
                        mediaControl.Stop();
                    }
                    catch (AccessViolationException) { Thread.Sleep(50); if (Thread.CurrentThread.Name != "1") { Thread.CurrentThread.Name = "1"; goto retry; } return false; }
                }
            }
            currentState = ePlayerStatus.Stopped;
            mediaControl = null;
            if (isAudioOnly == false)
            {
                DsError.ThrowExceptionForHR(videoWindow.put_Visible(OABool.False));
                DsError.ThrowExceptionForHR(videoWindow.put_Owner(IntPtr.Zero));
            }
            isAudioOnly = true;
            nowPlaying = new mediaInfo();
            OnMediaEvent(eFunction.Stop, instance, "");
            return true;
        }
        public void CloseClip()
        {
            int hr = 0;
            if (mediaControl != null)
            {
                try
                {
                    hr = mediaControl.Stop();
                }
                catch (AccessViolationException){}
                currentState = ePlayerStatus.Stopped;
                isAudioOnly = true;
                CloseInterfaces();
                currentState = ePlayerStatus.Ready;
            }
        }
        private void CloseInterfaces()
        {
            try
            {
                if (!isAudioOnly)
                {
                    DsError.ThrowExceptionForHR(videoWindow.put_Visible(OABool.False));
                    DsError.ThrowExceptionForHR(videoWindow.put_Owner(IntPtr.Zero));
                }
                if (mediaEventEx != null)
                {
                    DsError.ThrowExceptionForHR(mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero));
                }
                if (mediaEventEx != null)
                {
                    mediaEventEx = null;
                }
                if (mediaSeeking != null)
                {
                    mediaSeeking = null;
                }
                if (mediaPosition != null)
                {
                    mediaPosition = null;
                }
                if (mediaControl != null)
                {
                    mediaControl = null;
                }
                if (basicAudio != null)
                {
                    basicAudio = null;
                }
                if (basicVideo != null)
                {
                    basicVideo = null;
                }
                if (videoWindow != null)
                {
                    videoWindow = null;
                }
                if (graphBuilder != null)
                {
                    try
                    {
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
        public double getCurrentPos()
        {
            double time;
            lock (this) //changed from mediaposition
            {
                mediaPosition.get_CurrentPosition(out time);
            }
            return time;
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
        public int Resize(int nMultiplier, int nDivider)
        {
            if (videoWindow == null)
                return -1;
            if (fullscreen == true)
            {
                Form f = (Form)Control.FromHandle(OMPlayer.theHost.UIHandle(screen()));
                if (f.WindowState == FormWindowState.Maximized)
                    return videoWindow.SetWindowPosition(0, 0, f.Width + 1, f.Height);
                else
                    return videoWindow.SetWindowPosition(0, 0, f.Width - (f.Width-f.ClientSize.Width), f.Height - (f.Height-f.ClientSize.Height));
            }
            else
            {
                object o;
                theHost.getData(eGetData.GetScaleFactors, "", screen().ToString(), out o);
                if (o == null)
                    return -1;
                PointF sf = (PointF)o;
                return videoWindow.SetWindowPosition((int)(theHost.VideoPosition.Left * sf.X), (int)(theHost.VideoPosition.Top * sf.Y), (int)(theHost.VideoPosition.Width * sf.X), (int)(theHost.VideoPosition.Height * sf.Y));
            }
        }

        public bool PlayMovieInWindow(string filename)
        {
            int hr = 0;
            if (filename == string.Empty)
                return false;
            lock (this)
            {
                graphBuilder = (IGraphBuilder) new FilterGraph();
                IBaseFilter source = null;
                hr = ((IFilterGraph2) graphBuilder).AddSourceFilterForMoniker(OMPlayer.getDevMoniker(instance), null, "OutputDevice", out source);
                hr = graphBuilder.RenderFile(filename, null);
                if (hr != 0)
                {
                    return false;
                }
                mediaControl = (IMediaControl) graphBuilder;
                mediaEventEx = (IMediaEventEx) graphBuilder;
                mediaSeeking = (IMediaSeeking) graphBuilder;
                mediaPosition = (IMediaPosition) graphBuilder;
                videoWindow = graphBuilder as IVideoWindow;
                basicVideo = graphBuilder as IBasicVideo;
                basicAudio = graphBuilder as IBasicAudio;
                basicAudio.put_Volume((100 - currentVolume) * -100);
                CheckVisibility();
                DsError.ThrowExceptionForHR(hr);
                if (!isAudioOnly)
                {
                    DsError.ThrowExceptionForHR(videoWindow.put_Owner(OMPlayer.theHost.UIHandle(instance)));
                    DsError.ThrowExceptionForHR(videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren));
                    DsError.ThrowExceptionForHR(Resize(1, 1));
                    DsError.ThrowExceptionForHR(videoWindow.put_MessageDrain(drain));
                }
                currentPlaybackRate = 1.0;
                DsError.ThrowExceptionForHR(mediaControl.Run());
                mediaEventEx.SetNotifyWindow(drain, WM_Graph_Notify, new IntPtr(instance));
            }
            currentState = ePlayerStatus.Playing;
            return true;
        }

        public int SetRate(double rate)
        {
            int hr = 0;
            if (mediaPosition != null)
            {
                hr = mediaSeeking.SetRate(rate);
                if (hr == 0)
                    currentPlaybackRate = rate;
            }
            return hr;
        }

        private void waitForStop()
        {
            if (mediaControl != null)
            {
                while(true)
                {
                    Thread.Sleep(1000);
                    if (mediaPosition != null)
                        pos = getCurrentPos();
                    else
                        return;
                }
            }
        }
    }

    public sealed class MessageProc : Form
    {
        public delegate void eventOccured(int instance);
        public new delegate void Click();
        public new Click OnClick;
        public eventOccured OnEvent;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_Graph_Notify)
                OnEvent((int)m.LParam);
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
