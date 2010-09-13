﻿using System;
using OpenMobile.Plugin;
using OpenMobile;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenMobile.Graphics;

namespace WinCam
{
    public sealed class WinCam:IAVPlayer
    {

        public bool play(int instance)
        {
            return false;
        }

        public bool stop(int instance)
        {
            foreach (stream s in streams[instance])
            {
                s.Dispose();
            }
            streams[instance].Clear();
            if (OnMediaEvent != null)
                OnMediaEvent(eFunction.Stop, instance,"");
            return true;
        }

        public bool setPosition(int instance, float seconds)
        {
            return false;
        }

        public bool setPlaybackSpeed(int instance, float speed)
        {
            return false;
        }
        public bool play(int instance, string url, OpenMobile.eMediaType type)
        {
            if (type == eMediaType.LiveCamera)
            {
                if (streams[instance] == null)
                    streams[instance] = new List<stream>();
                else
                    streams[instance].Clear();
                streams[instance].Add(new stream(instance));
                int cam;
                if (!int.TryParse(url.Substring(3),out cam))
                    return false;
                PointF scale;
                object o;
                theHost.getData(eGetData.GetScaleFactors,"","0",out o);
                if (o==null)
                    return false;
                scale=(PointF)o;
                Rectangle r = theHost.VideoPosition;
                if (!streams[instance][0].start(cam, (int)(r.X*scale.X), (int)(r.Y*scale.Y), (int)(r.Width*scale.X), (int)(r.Height*scale.Y)))
                    return false;
                info = new mediaInfo(url);
                info.Type = eMediaType.LiveCamera;
                info.Name = "Camera " + (cam+1).ToString();
                streaming = true;
                if (OnMediaEvent != null)
                    OnMediaEvent(eFunction.Play, instance, url);
                return true;
            }
            return false;
        }

        public float getCurrentPosition(int instance)
        {
            return streaming ? 0:-1;
        }
        bool streaming;
        public ePlayerStatus getPlayerStatus(int instance)
        {

            return streaming ? ePlayerStatus.Playing:ePlayerStatus.Ready;
        }

        public float getPlaybackSpeed(int instance)
        {
            return 1F;
        }

        public bool setVolume(int instance, int percent)
        {
            return false;
        }

        public int getVolume(int instance)
        {
            return 100;
        }
        mediaInfo info=new mediaInfo();
        public mediaInfo getMediaInfo(int instance)
        {
            return info;
        }

        public event MediaEvent OnMediaEvent;
        public string[] OutputDevices
        {
            get
            {
                //todo
                throw new NotImplementedException(); 
            }
        }

        public bool SupportsAdvancedFeatures
        {
            get { return false; }
        }

        public bool SetVideoVisible(int instance, bool visible)
        {
            foreach (stream s in streams[instance])
                s.visible(visible);
            return true;
        }

        #region IBasePlugin Members
        List<stream>[] streams;
        public static IPluginHost theHost;
        static MessageProc Invoker;
        public delegate bool DelStart(int id, int x, int y, int w, int h);
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            Invoker = new MessageProc();
            IntPtr tmp=Invoker.Handle;
            streams = new List<stream>[theHost.InstanceCount];
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "WinCam"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Windows video camera support"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion

        public bool pause(int instance)
        {
            return false;
        }

        public sealed class stream:IDisposable
        {
            IGraphBuilder graph;
            IPin outputPin;
            IAMVideoControl vcontrol;
            IVideoWindow window;
            IMediaControl control;
            int instance;
            event DelStart go;
            public bool scale;
            public stream(int inst)
            {
                instance = inst;
                go += startInternal;
            }
            public bool start(int id, int x, int y, int w, int h)
            {
                return (bool)Invoker.Invoke(go,new object[]{id,x,y,w,h});
            }
            bool startInternal(int id, int x, int y, int w, int h)
            {
                graph = (IGraphBuilder)new FilterGraph();
                IBaseFilter source;
                int hr = ((IFilterGraph2)graph).AddSourceFilterForMoniker(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)[id].Mon, null, "CAM", out source);
                vcontrol = (IAMVideoControl)source;
                outputPin = DsFindPin.ByDirection(source, PinDirection.Output, 0);
                ICaptureGraphBuilder2 builder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                hr = builder.SetFiltergraph(graph);
                hr = builder.RenderStream(null, null, source, null, null);
                window = (IVideoWindow)graph;
                //TODO - fix screen lookup
                hr = window.put_Owner(theHost.UIHandle(instance));
                hr = window.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
                control = (IMediaControl)graph;
                hr=control.Run();
                if (scale)
                {
                    int testx;
                    int testy;
                    window.GetMaxIdealImageSize(out testx, out testy);
                    int newWidth = (int)((testx / (float)testy) * h);
                    window.SetWindowPosition(x + ((w - newWidth) / 2), y, newWidth, h);
                }
                else
                    window.SetWindowPosition(x, y, w, h);
                return (hr >= 0);
            }
            public void resize(int x, int y, int w, int h)
            {
                window.SetWindowPosition(x, y, w, h);
            }
            public void visible(bool isTrue)
            {
                if (isTrue)
                    window.put_Visible(OABool.True);
                else
                    window.put_Visible(OABool.False);
            }
            ~stream() {
                Dispose(); }
            public void Dispose()
            {
                if (graph == null)
                    return;
                try
                {
                    if (control!=null)
                        control.Stop();
                    if (window != null)
                        window.put_Visible(OABool.False);
                    Marshal.ReleaseComObject(graph);
                }
                catch (Exception) { }
                graph = null;
                GC.SuppressFinalize(this);
            }
        }
        static int WM_Graph_Notify = 0x8001;
        static int WM_LBUTTONUP = 0x0202;
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