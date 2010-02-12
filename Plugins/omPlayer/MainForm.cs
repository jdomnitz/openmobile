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
using OpenMobile.Media;
using OpenMobile.Plugin;

namespace OMPlayer
{
    public sealed class OMPlayer : IAVPlayer, IDisposable
{
    // Fields
    private static IntPtr hDrain = IntPtr.Zero;
    private AVPlayer[] player;
    private Thread[] t;
    private static IPluginHost theHost;
    private const int WMGraphNotify = 0x40d;

    // Events
    public event MediaEvent OnMediaEvent;

    private void checkInstance(int instance)
    {
        if (player[instance] == null)
        {
            player[instance] = new AVPlayer(instance);
            AVPlayer player1 = player[instance];
            player1.OnMediaEvent = (MediaEvent) Delegate.Combine(player1.OnMediaEvent, new MediaEvent(forwardEvent));
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (player[i] != null)
                player[i].CloseClip();
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
        if (dev < lst.Count)
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
        return player[instance].currentVolume;
    }

    private void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
    {
        if (function == eFunction.closeProgram)
        {/*
            using (PluginSettings settings = new PluginSettings())
            {
                if (nowPlaying != null)
                {
                    if (getPlayerStatus(0) == ePlayerStatus.Stopped)
                    {
                        settings.setSetting("Music.LastPlayingURL", "");
                        return;
                    }
                    settings.setSetting("Music.LastPlayingURL", nowPlaying.Location);
                    double position;
                    if (mediaPosition != null)
                    {
                        mediaPosition.get_CurrentPosition(out position);
                        settings.setSetting("Music.LastPlayingPosition", position.ToString());
                    }
                    else
                    {
                        settings.setSetting("Music.LastPlayingURL", "");
                    }
                }
                else
                {
                    settings.setSetting("Music.LastPlayingURL", "");
                }
            }*/
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
        t = new Thread[theHost.InstanceCount];
        host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
        /*
        using (PluginSettings setting = new PluginSettings())
        {
            if (setting.getSetting("Music.AutoResume") == "True")
            {
                host.execute(eFunction.loadAVPlayer, "0", "OMPlayer");
                play(0, setting.getSetting("Music.LastPlayingURL"), eMediaType.Local);
                if (mediaPosition == null)
                    return eLoadStatus.LoadSuccessful;
                double pos;
                if (double.TryParse(setting.getSetting("Music.LastPlayingPosition"), out pos) == true)
                {
                    if (pos > 1)
                        pos -= 1;
                    mediaPosition.put_CurrentPosition(pos);
                }
            }
        }*/
        return eLoadStatus.LoadSuccessful;
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
            }
        }
        else if (player[instance].mediaControl.Pause() >= 0)
        {
            player[instance].currentState = ePlayerStatus.Paused;
            OnMediaEvent(eFunction.Pause, instance, "");
        }
        return true;
    }

    public bool play(int instance)
    {
        checkInstance(instance);
        if ((player[instance].mediaControl != null) && (player[instance].mediaControl.Run() >= 0))
        {
            player[instance].currentState = ePlayerStatus.Playing;
            OnMediaEvent(eFunction.Play, instance, "");
            player[instance].pos = 0;
            return true;
        }
        return false;
    }

    public bool play(int instance, string url, eMediaType type)
    {
        try
        {
            checkInstance(instance);
            lock (player[instance])
            {
                if (player[instance].currentState == ePlayerStatus.Playing)
                    stop(instance);
                if (player[instance].PlayMovieInWindow(url)==false)
                    return false;
            }
            player[instance].pos = 0;
            player[instance].nowPlaying = TagReader.getInfo(url);
            if (player[instance].nowPlaying.coverArt == null)
                player[instance].nowPlaying.coverArt = TagReader.getFolderImage(player[instance].nowPlaying.Location);
            OnMediaEvent(eFunction.Play, instance, url);
            if (t[instance] != null)
                t[instance].Abort();
            t[instance] = new Thread(new ThreadStart(player[instance].waitForStop));
            t[instance].Start();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool setPlaybackSpeed(int instance, float speed)
    {
        checkInstance(instance);
        player[instance].SetRate((double) speed);
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
                goto Label_0075;
            }
            if (percent == -1)
            {
                player[instance].currentVolume = 0;
                goto Label_0075;
            }
        }
        return false;
    Label_0075:
        return (player[instance].basicAudio.put_Volume((100 - player[instance].currentVolume) * -100) == 0);
    }

    public bool stop(int instance)
    {
        checkInstance(instance);
        DsLong pos = new DsLong(0);

        if ((player[instance].currentState == ePlayerStatus.Paused) || (player[instance].currentState == ePlayerStatus.Playing))
        {
        retry:
            lock (player[instance])
            {
                if ((player[instance].mediaControl == null) || (player[instance].mediaSeeking == null))
                    return false;
                try
                {
                    player[instance].currentState = ePlayerStatus.Ready;
                    player[instance].mediaControl.Stop();
                    player[instance].mediaPosition.put_CurrentPosition((double)instance);
                }
                catch (AccessViolationException) { Thread.Sleep(50); if (Thread.CurrentThread.Name != "1") { Thread.CurrentThread.Name = "1"; goto retry; } return false; }
                player[instance].mediaControl = null;
                player[instance].isAudioOnly = true;
                OnMediaEvent(eFunction.Stop, instance, "");
            }
        }
        return true;
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
    public class AVPlayer
    {
        // Fields
        public IBasicAudio basicAudio = null;
        private IBasicVideo basicVideo = null;
        public double currentPlaybackRate = 1.0;
        public ePlayerStatus currentState = ePlayerStatus.Stopped;
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
        private IVideoWindow videoWindow = null;

        // Methods
        public AVPlayer(int instanceNum)
        {
            instance = instanceNum;
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
                        lock (this)
                        {
                            Marshal.ReleaseComObject(graphBuilder);
                        }
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
            lock (mediaPosition)
            {
                mediaPosition.get_CurrentPosition(out time);
            }
            return (double) ((float) time);
        }

        private void HandleGraphEvent()
        {
            int hr = 0;
            if (mediaEventEx != null)
            {
                EventCode evCode;
                IntPtr evParam1;
                IntPtr evParam2;
                while (mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
                {
                    hr = mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);
                    if (evCode == EventCode.Complete)
                    {
                        DsLong pos = new DsLong(0L);
                        if (mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning) < 0)
                        {
                            hr = mediaControl.Stop();
                            hr = mediaControl.Run();
                        }
                    }
                }
            }
        }

        private int InitVideoWindow(int nMultiplier, int nDivider)
        {
            Form f = (Form) Control.FromHandle(OMPlayer.theHost.UIHandle(0));
            return videoWindow.SetWindowPosition(0, 0, f.Width, f.Height);
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
                hr = ((IFilterGraph2) graphBuilder).AddSourceFilterForMoniker(OMPlayer.getDevMoniker(instance + 1), null, "OutputDevice", out source);
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
                CheckVisibility();
                DsError.ThrowExceptionForHR(hr);
                if (!isAudioOnly)
                {
                    DsError.ThrowExceptionForHR(videoWindow.put_Owner(OMPlayer.theHost.UIHandle(0)));
                    DsError.ThrowExceptionForHR(videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren));
                    DsError.ThrowExceptionForHR(InitVideoWindow(1, 1));
                }
                currentPlaybackRate = 1.0;
                DsError.ThrowExceptionForHR(mediaControl.Run());
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

        private int ToggleFullScreen()
        {
            OABool lMode;
            int hr = 0;
            if (isAudioOnly || (videoWindow == null))
            {
                return 0;
            }
            DsError.ThrowExceptionForHR(videoWindow.get_FullScreenMode(out lMode));
            if (lMode == OABool.False)
            {
                hr = videoWindow.get_MessageDrain(out hDrain);
                DsError.ThrowExceptionForHR(hr);
                DsError.ThrowExceptionForHR(hr);
                lMode = OABool.True;
                hr = videoWindow.put_FullScreenMode(lMode);
                DsError.ThrowExceptionForHR(hr);
            }
            else
            {
                lMode = OABool.False;
                DsError.ThrowExceptionForHR(videoWindow.put_FullScreenMode(lMode));
                DsError.ThrowExceptionForHR(videoWindow.put_MessageDrain(OMPlayer.hDrain));
                hr = videoWindow.SetWindowForeground(OABool.True);
                DsError.ThrowExceptionForHR(hr);
            }
            return hr;
        }

        public void waitForStop()
        {
            if (mediaControl != null)
            {
                FilterState s;
                mediaControl.GetState(0x3e8, out s);
                for (double lastPosition = -1.0; (s == FilterState.Running) || (s == FilterState.Paused); lastPosition = pos)
                {
                    Thread.Sleep(0x3e8);
                    pos = lastPosition;
                    if (mediaPosition != null)
                    {
                        pos = getCurrentPos();
                    }
                    if ((currentState != ePlayerStatus.Paused) && (pos == lastPosition))
                    {
                        currentState = ePlayerStatus.Stopped;
                        OnMediaEvent(eFunction.Stop, instance, "");
                        if (currentState != ePlayerStatus.Ready)
                            OnMediaEvent(eFunction.nextMedia, instance, "");
                        break;
                    }
                }
            }
        }
    }
}
}
