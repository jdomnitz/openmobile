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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using DirectShowLib.Dvd;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Graphics;

namespace OMDVD
{
    public sealed class OMDVD : IAVPlayer, IDisposable
    {
        // Fields
        private static IntPtr hDrain = IntPtr.Zero;
        private AVPlayer[] player;
        private static IPluginHost theHost;
        static int WM_Graph_Notify = 0x8001;
        static int WM_LBUTTONDOWN = 0x0201;
        static int WM_LBUTTONUP = 0x0202;
        private static MessageProc Invoker;

        // Events
        public event MediaEvent OnMediaEvent;

        public Settings loadSettings()
        {
            return null;
        }

        private void checkInstance(int instance)
        {
            if (player[instance] == null)
            {
                player[instance] = new AVPlayer(instance);
                player[instance].OnMediaEvent += new MediaEvent(forwardEvent);
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
            if (player[instance].graphBuilder == null)
            {
                return -1F;
            }
            if ((player[instance].currentState == ePlayerStatus.Stopped) || (player[instance].currentState == ePlayerStatus.Ready))
            {
                return 0F;
            }
            return (float)player[instance].pos;
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
            if ((dev < lst.Count) && (dev >= 0))
                return lst[dev];
            return lst[0];
        }

        public mediaInfo getMediaInfo(int instance)
        {
            checkInstance(instance);
            player[instance].nowPlaying.Type = eMediaType.DVD;
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
            return player[instance].currentVolume;
        }
        private void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.RenderingWindowResized)
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
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            player = new AVPlayer[theHost.InstanceCount];
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            OnPlay += new stringRet(invokePlay);
            OnSpeed += new Speed(InvokeOnSpeed);
            OnStop += new delVoid(invokeStop);
            Invoker = new MessageProc();
            IntPtr tmp = Invoker.Handle; //stupid hack
            return eLoadStatus.LoadSuccessful;
        }

        bool invokePlay(int instance, string url)
        {
            return player[instance].play(url);
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
                return (player[instance].videoWindow.put_Visible(OABool.False) == 0);
            else
                if ((player[instance].currentState != ePlayerStatus.Ready) && (player[instance].currentState == ePlayerStatus.Stopped))
                    return (player[instance].videoWindow.put_Visible(OABool.True) == 0);
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
        public delegate bool stringRet(int instance, string arg);
        public stringRet OnPlay;

        public bool play(int instance, string url, eMediaType type)
        {
            if ((type != eMediaType.DVD)&&(type!=eMediaType.LocalHardware))
                return false;
            try
            {
                checkInstance(instance);
                return (bool)Invoker.Invoke(OnPlay, new object[] { instance, url });// player[instance].play(url);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public delegate bool Speed(int instance, float speed);
        public Speed OnSpeed;

        public bool InvokeOnSpeed(int instance, float speed)
        {
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

        public bool setPlaybackSpeed(int instance, float speed)
        {
            checkInstance(instance);
            return (bool)Invoker.Invoke(OnSpeed, new object[] { instance, speed });
        }

        public bool setPosition(int instance, float seconds)
        {
            checkInstance(instance);
            /*if ((player[instance].currentState == ePlayerStatus.Stopped) || (player[instance].currentState == ePlayerStatus.Ready) || (player[instance].currentState == ePlayerStatus.Error))
                return false;
            if ((player[instance].mediaControl == null) || (player[instance].mediaPosition == null))
                return false;
            player[instance].pos = seconds;
            return (0 == player[instance].mediaPosition.put_CurrentPosition((double)seconds));
        
             */
            return false;
        }

        public bool setVolume(int instance, int percent)
        {
            checkInstance(instance);
            if ((player[instance].graphBuilder != null) && (player[instance].basicAudio != null))
            {
                if ((percent >= 0) && (percent <= 100))
                {
                    player[instance].currentVolume = percent;
                    return (player[instance].basicAudio.put_Volume((100 - player[instance].currentVolume) * -100) == 0);
                }
                if (percent == -1)
                {
                    player[instance].currentVolume = 0;
                    return (player[instance].basicAudio.put_Volume((100 - player[instance].currentVolume) * -100) == 0);
                }
            }
            return false;
        }
        public delegate bool delVoid(int instance);
        public delVoid OnStop;

        bool invokeStop(int instance)
        {
            return player[instance].stop();
        }

        public bool stop(int instance)
        {
            checkInstance(instance);
            return (bool)Invoker.Invoke(OnStop,new object[]{instance});
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
                return "DirectShow DVD Player";
            }
        }

        public string pluginName
        {
            get
            {
                return "OMDVD";
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
            public IDvdControl2 dvdctrl = null;
            private IDvdGraphBuilder dvdgraph = null;
            private IDvdInfo2 info = null;
            private int currentChapter = 0;
            public double currentPlaybackRate = 1.0;
            public ePlayerStatus currentState = ePlayerStatus.Ready;
            public int currentVolume = 100;
            public IGraphBuilder graphBuilder = null;
            private int instance = -1;
            public IMediaControl mediaControl = null;
            private IMediaEventEx mediaEventEx = null;
            public mediaInfo nowPlaying = new mediaInfo();
            public MediaEvent OnMediaEvent;
            public double pos = -1.0;
            public IVideoWindow videoWindow = null;
            private MessageProc sink;
            private bool fullscreen = false;

            // Methods
            public AVPlayer(int instanceNum)
            {
                instance = instanceNum;
                sink = new MessageProc();
                sink.OnClicked += new MessageProc.Clicked(clicked);
                sink.OnEvent += new MessageProc.eventOccured(eventOccured);
            }

            void clicked()
            {
                fullscreen = !fullscreen;
                Resize();
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
                        if ((mediaControl != null) && (mediaControl.GetState(50, out fs) == 0))
                            if (fs == FilterState.Paused)
                            {
                                currentState = ePlayerStatus.Paused;
                                OnMediaEvent(eFunction.Pause, instance, "");
                            }
                        break;
                    case EventCode.DvdChapterStart:
                        currentChapter = (int)arg1;
                        theHost.execute(eFunction.setPlaylistPosition, instance.ToString(), (currentChapter-1).ToString());
                        DvdHMSFTimeCode time = new DvdHMSFTimeCode();
                        DvdTimeCodeFlags flags;
                        info.GetTotalTitleTime(time, out flags);
                        nowPlaying.Length = (time.bHours * 3600) + (time.bMinutes * 60) + time.bSeconds;
                        break;
                    case EventCode.DvdCurrentTime:
                        byte[] part = BitConverter.GetBytes(arg1.ToInt32());
                        pos = (3600 * part[0]) + (60 * part[1]) + part[2];
                        break;
                }
            }
            public bool play(string url)
            {
                lock (this)
                {
                    if (url.Length == 3)
                    {
                        if (currentState == ePlayerStatus.Playing)
                            stop();
                        if (PlayMovieInWindow(url) == false)
                            return false;
                        pos = 0;
                    }
                    else
                    {
                        if (url.Length < 11)
                            return false;
                        DirectShowLib.Dvd.IDvdCmd cmd;
                        currentState = ePlayerStatus.Transitioning;
                        if (dvdctrl.PlayChapter(int.Parse(url.Substring(10)), DvdCmdFlags.Block, out cmd) != 0)
                            return false;
                        currentState = ePlayerStatus.Playing;
                    }
                }
                DvdHMSFTimeCode time=new DvdHMSFTimeCode();
                DvdTimeCodeFlags flags;
                info.GetTotalTitleTime(time, out flags);
                nowPlaying.Length = (time.bHours*3600)+(time.bMinutes*60)+time.bSeconds;
                OnMediaEvent(eFunction.Play, instance, url);
                return true;
            }
            public bool stop()
            {
                if ((currentState == ePlayerStatus.Paused) || (currentState == ePlayerStatus.Playing))
                {
                    lock (this)
                    {
                        if ((dvdctrl == null)||(videoWindow==null))
                            return false;
                        try
                        {
                            currentState = ePlayerStatus.Ready;
                            dvdctrl.Stop();
                        }
                        catch (AccessViolationException) { return false; }
                    }
                }
                currentState = ePlayerStatus.Stopped;
                dvdctrl = null;
                try
                {
                    Marshal.ReleaseComObject(dvdgraph);
                }
                catch (Exception) { }
                videoWindow.put_Visible(OABool.False);
                videoWindow.put_Owner(IntPtr.Zero);
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
                    catch (AccessViolationException) { }
                    currentState = ePlayerStatus.Stopped;
                    CloseInterfaces();
                    currentState = ePlayerStatus.Ready;
                }
            }
            private void CloseInterfaces()
            {
                try
                {
                    DsError.ThrowExceptionForHR(videoWindow.put_Visible(OABool.False));
                    DsError.ThrowExceptionForHR(videoWindow.put_Owner(IntPtr.Zero));
                    if (mediaEventEx != null)
                    {
                        DsError.ThrowExceptionForHR(mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero));
                    }
                    if (mediaEventEx != null)
                    {
                        mediaEventEx = null;
                    }
                    if (dvdctrl != null)
                    {
                        dvdctrl = null;
                    }
                    if (graphBuilder != null)
                    {
                       graphBuilder = null;
                    }
                    if (mediaControl != null)
                    {
                        mediaControl = null;
                    }
                    if (basicAudio != null)
                    {
                        basicAudio = null;
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
                        catch (Exception) { }
                    }
                    graphBuilder = null;
                }
                catch
                {
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
                if (videoWindow == null)
                    return -1;
                if (fullscreen == true)
                {
                    OpenMobile.Platform.Windows.WindowInfo info = new OpenMobile.Platform.Windows.WindowInfo();
                    OpenMobile.Platform.Windows.Functions.GetWindowInfo(OMDVD.theHost.UIHandle(screen()), ref info);
                    if ((info.Style & OpenMobile.Platform.Windows.WindowStyle.ThickFrame) == OpenMobile.Platform.Windows.WindowStyle.ThickFrame)
                        return videoWindow.SetWindowPosition(0, 0, info.Window.Width - (info.Window.Width - info.Client.Width), info.Window.Height - (info.Window.Height - info.Client.Height));
                    else
                        return videoWindow.SetWindowPosition(0, 0, info.Window.Width + 1, info.Window.Height);
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

            public bool PlayMovieInWindow(string path)
            {
                DirectShowLib.Dvd.IDvdCmd cmd;
                dvdgraph = (IDvdGraphBuilder)new DirectShowLib.DvdGraphBuilder();
                DirectShowLib.Dvd.AMDvdRenderStatus status;
                int hr = dvdgraph.RenderDvdVideoVolume(path + "VIDEO_TS", DirectShowLib.Dvd.AMDvdGraphFlags.HWDecPrefer, out status);
                if (hr != 0)
                    return false;
                object o;
                dvdgraph.GetDvdInterface(typeof(IVideoWindow).GUID, out o);
                videoWindow = (IVideoWindow)o;
                dvdgraph.GetDvdInterface(typeof(IAMLine21Decoder).GUID, out o);
                hr = ((IAMLine21Decoder)o).SetServiceState(AMLine21CCState.Off);
                if (hr != 0)
                    return false;
                dvdgraph.GetDvdInterface(typeof(DirectShowLib.Dvd.IDvdControl2).GUID, out o);
                dvdctrl = (IDvdControl2)o;
                hr = dvdgraph.GetFiltergraph(out graphBuilder);
                if (hr != 0)
                    return false;
                mediaControl = (IMediaControl)graphBuilder;
                mediaEventEx = (IMediaEventEx)graphBuilder;
                mediaEventEx.SetNotifyWindow(sink.Handle, WM_Graph_Notify, new IntPtr(instance));
                for(int i=0;i<theHost.ScreenCount;i++)
                    if (theHost.instanceForScreen(i)==instance)
                        hr = videoWindow.put_Owner(OMDVD.theHost.UIHandle(i));
                if (hr != 0)
                    return false;
                hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
                if (hr != 0)
                    return false;
                IBaseFilter source = null;
                hr = ((IFilterGraph2)graphBuilder).FindFilterByName("DSound Renderer", out source);
                graphBuilder.RemoveFilter(source);
                IEnumFilters fltrs;
                hr = graphBuilder.EnumFilters(out fltrs);
                IBaseFilter[] filters = new IBaseFilter[1];
                IntPtr fetched = new IntPtr();
                IPin outpin = null;
                hr = 0;
                while ((hr == 0) && (outpin == null))
                {
                    hr = fltrs.Next(1, filters, fetched);
                    outpin = DsFindPin.ByConnectionStatus(filters[0], PinConnectedStatus.Unconnected, 0);
                }
                if (hr == 0)
                {
                    hr = ((IFilterGraph2)graphBuilder).AddSourceFilterForMoniker(OMDVD.getDevMoniker(instance), null, "OutputDevice", out source);
                    IPin inpin = DsFindPin.ByDirection(source, PinDirection.Input, 0);
                    hr = graphBuilder.Connect(outpin, inpin);
                }
                hr = mediaControl.Run();
                DsError.ThrowExceptionForHR(Resize());
                DsError.ThrowExceptionForHR(videoWindow.put_MessageDrain(sink.Handle));
                hr = dvdctrl.SetSubpictureState(false, DvdCmdFlags.None, out cmd);
                //hr = dvdctrl.SelectVideoModePreference(DvdPreferredDisplayMode.Display16x9);
                hr = dvdctrl.PlayForwards(1.0, DirectShowLib.Dvd.DvdCmdFlags.None, out cmd);
                if (hr != 0)
                    return false;
                dvdgraph.GetDvdInterface(typeof(IDvdInfo2).GUID, out o);
                info = (IDvdInfo2)o;
                o = null;
                currentState = ePlayerStatus.Playing;
                currentChapter = 0;
                int chp;
                info.GetNumberOfChapters(1, out chp);
                List<mediaInfo> lst = new List<mediaInfo>();
                for (int i = 0; i < chp; i++)
                {
                    lst.Add(new mediaInfo(path + "Chapter" + (i + 1).ToString()));
                }
                theHost.setPlaylist(lst, instance);
                updateDVD();
                currentPlaybackRate = 1.0;
                OnMediaEvent(eFunction.Play, instance, path);
                return true;
            }
            private void updateDVD()
            {
                DvdPlaybackLocation2 loc;
                info.GetCurrentLocation(out loc);
                pos = new TimeSpan(loc.TimeCode.bHours, loc.TimeCode.bMinutes, loc.TimeCode.bSeconds).TotalSeconds;
                DvdHMSFTimeCode code = new DvdHMSFTimeCode();
                DvdTimeCodeFlags flags;
                info.GetTotalTitleTime(code, out flags);
                nowPlaying.Length = (3600 * (int)code.bHours) + (60 * (int)code.bMinutes) + (int)code.bSeconds;
                if (loc.ChapterNum != currentChapter)
                {
                    theHost.execute(eFunction.setPlaylistPosition, instance.ToString(), loc.ChapterNum.ToString());
                    currentChapter = loc.ChapterNum;
                }
            }
            public int SetRate(double rate)
            {
                int hr = 0;
                if (dvdctrl != null)
                {
                    IDvdCmd cmd;
                    if (rate < 0)
                        hr = dvdctrl.PlayBackwards(rate * -1, DvdCmdFlags.None, out cmd);
                    else
                        hr = dvdctrl.PlayForwards(rate, DvdCmdFlags.None, out cmd);
                    if (hr == 0)
                        currentPlaybackRate = rate;
                }
                return hr;
            }
        }

        public sealed class MessageProc : Form
        {
            public delegate void eventOccured(int instance);
            public delegate void Clicked();
            public Clicked OnClicked;
            public eventOccured OnEvent;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_Graph_Notify)
                    OnEvent((int)m.LParam);
                else if ((m.Msg == WM_LBUTTONDOWN)||(m.Msg == WM_LBUTTONUP))
                {
                    int x = m.LParam.ToInt32() & 0xFFFF;
                    int y = m.LParam.ToInt32() >> 16;
                    if ((x<0)||(y<0))
                    {
                        m.Result = new IntPtr(-1);
                        return;
                    }
                    if (m.Msg == WM_LBUTTONUP)
                        OnClicked();
                }
                else
                    base.WndProc(ref m);
            }
        }
    }
}
