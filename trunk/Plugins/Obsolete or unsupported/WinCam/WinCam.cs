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
using OpenMobile.Plugin;
using OpenMobile;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenMobile.Graphics;
using OpenMobile.Data;

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
            if ((streams == null) || (streams.Length <= instance))
                return false;
            if (streams[instance] == null)
                return false;
            foreach (stream s in streams[instance])
            {
                s.Dispose();
            }
            streams[instance].Clear();
            if (OnMediaEvent != null)
                OnMediaEvent(eFunction.hideVideoWindow, instance, "");
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
                Rectangle r = theHost.GetVideoPosition(instance);
                string hardware=string.Empty;
                using (PluginSettings s = new PluginSettings())
                    hardware = s.getSetting("WinCam.Source" + cam.ToString() + ".Source");
                cam = getCam(hardware);
                if (cam == -1)
                    cam = 0;
                if (!streams[instance][0].start(cam, (int)(r.X*scale.X), (int)(r.Y*scale.Y), (int)(r.Width*scale.X), (int)(r.Height*scale.Y)))
                    return false;
                if (OnMediaEvent != null)
                    OnMediaEvent(eFunction.showVideoWindow, instance, "");
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
        private int getCam(string name)
        {
            DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            string[] lst = new string[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i].Name == name)
                    return i;
            }
            return -1;
        }
        private string[] getcams()
        {
            DsDevice[] d = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVideo);
            if (d.Length == 0)
                d = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
           string[] lst = new string[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                lst[i] = d[i].Name;
            }
            return lst;
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
                        lst.Add(d[i].Name.Replace("DirectSound", "").Replace(":", "").Replace("  ", " "));
                    }
                }
                return lst.ToArray();
            }
        }

        public bool SupportsAdvancedFeatures
        {
            get { return false; }
        }

        public bool SetVideoVisible(int instance, bool visible)
        {
            if (streams == null)
                return false;
            if (streams.Length <= instance)
                return false;
            if (visible)
                if (OnMediaEvent!=null)
                    OnMediaEvent(eFunction.showVideoWindow, instance, "");
            else
                if (OnMediaEvent != null)
                    OnMediaEvent(eFunction.hideVideoWindow, instance, "");
            if (streams[instance] != null)
            {
                for (int i = 0; i < streams[instance].Count; i++)
                    if (streams[instance][i] != null)
                        streams[instance][i].visible(visible);
            }
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
        Settings settings;
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Camera Settings");
                List<string> cams = new List<string>(getcams());
                List<string> choices = new List<string>(new string[] { "None", "Flip Vertically", "Flip Horizontally", "Flip Both" });
                List<string> values = new List<string>(new string[] { "", "Vertical", "Horizontal", "Both" });
                using (PluginSettings s = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source1.Source","Source", "Camera 1 Source", cams, cams, s.getSetting("WinCam.Source1.Source")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source1.Flip", "Flip", "Source 1 Flip", choices, values, s.getSetting("WinCam.Source1.Flip")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source2.Source", "Source", "Camera 2 Source", cams, cams, s.getSetting("WinCam.Source2.Source")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source2.Flip", "Flip", "Source 2 Flip", choices, values, s.getSetting("WinCam.Source2.Flip")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source3.Source", "Source", "Camera 3 Source", cams, cams, s.getSetting("WinCam.Source3.Source")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source3.Flip", "Flip", "Source 3 Flip", choices, values, s.getSetting("WinCam.Source3.Flip")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source4.Source", "Source", "Camera 4 Source", cams, cams, s.getSetting("WinCam.Source4.Source")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "WinCam.Source4.Flip", "Flip", "Source 4 Flip", choices, values, s.getSetting("WinCam.Source4.Flip")));
                    settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
                }
            }
            return settings;
        }

        void settings_OnSettingChanged(int screen,Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
                s.setSetting(setting.Name, setting.Value);
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
            public bool scale=true;
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
                for(int i=0;i<theHost.ScreenCount;i++)
                    if(instance==theHost.instanceForScreen(i))
                        window.put_Owner((IntPtr)theHost.UIHandle(i));
                hr = window.put_WindowStyle(WindowStyle.Child);
                control = (IMediaControl)graph;
                hr=control.Run();
                using(PluginSettings s=new PluginSettings())
                switch(s.getSetting("WinCam.Source"+(instance+1).ToString()+".Flip"))
                {
                    case "Vertical":
                        vcontrol.SetMode(outputPin, VideoControlFlags.FlipVertical);
                        break;
                    case "Horizontal":
                        vcontrol.SetMode(outputPin, VideoControlFlags.FlipHorizontal);
                        break;
                    case "Both":
                        vcontrol.SetMode(outputPin, VideoControlFlags.FlipVertical|VideoControlFlags.FlipHorizontal);
                        break;
                }
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
            public eventOccured OnEvent;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_Graph_Notify)
                    OnEvent((int)m.LParam);
                else
                    base.WndProc(ref m);
            }
        }
    }
}
