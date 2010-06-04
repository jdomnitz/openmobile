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
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Threading;

namespace WebBrowser
{
    public sealed class Browser:IHighLevel
    {
        #region IHighLevel Members
        OMPanel p = new OMPanel();
        ScreenManager manager;
        IPluginHost theHost;
        bool[] embedded;
        bool[] queued;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            OMImage ss = new OMImage(0, 100, 1000, 500);
            p.addControl(ss);
            p.BackgroundType = backgroundStyle.SolidColor;
            p.BackgroundColor1 = Color.White;
            manager = new ScreenManager(theHost.ScreenCount);
            manager.loadPanel(p);
            embedded = new bool[theHost.ScreenCount];
            queued = new bool[theHost.ScreenCount];
            TaskManager.QueueTask(startItUp, ePriority.Normal,"Preload Web Browser");
            return eLoadStatus.LoadSuccessful;
        }

        void startItUp()
        {
            if (Process.GetProcessesByName("TouchBrowser").Length == 0)
            {
                Process p = Process.Start(theHost.PluginPath+@"\OMTouchBrowser\TouchBrowser.exe");
                p.PriorityClass = ProcessPriorityClass.BelowNormal;
                Thread.Sleep(1000);
            }
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.ExecuteTransition)
            {
                int screen = int.Parse(arg1);
                if (queued[screen] == true)
                {
                    queued[screen] = false;
                    bool b = OpenMobile.Framework.OSSpecific.embedApplication("TouchBrowser", int.Parse(arg1), theHost, new Rectangle(0, 100, 1000, 500));
                    if (b == false)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            Thread.Sleep(100);
                            b = OpenMobile.Framework.OSSpecific.embedApplication("TouchBrowser", int.Parse(arg1), theHost, new Rectangle(0, 100, 1000, 500));
                            if (b == true)
                                break;
                        }
                    }
                }
            }
            if (function == eFunction.TransitionToPanel)
            {
                if (arg2 == "WebBrowser")
                {
                    startItUp();
                    embedded[int.Parse(arg1)] = true;
                    queued[int.Parse(arg1)] = true;
                }
                else if (embedded[int.Parse(arg1)] == true)
                {
                    setScreenshot(int.Parse(arg1));
                    OpenMobile.Framework.OSSpecific.unEmbedApplication("TouchBrowser", int.Parse(arg1));
                }
            }
            if (function == eFunction.TransitionFromPanel)
            {
                if (arg2 == "WebBrowser")
                {
                    setScreenshot(int.Parse(arg1));
                    OpenMobile.Framework.OSSpecific.unEmbedApplication("TouchBrowser", int.Parse(arg1));
                    embedded[int.Parse(arg1)] = false;
                }
                else if (embedded[int.Parse(arg1)])
                    queued[int.Parse(arg1)] = true;
            }
            if ((function == eFunction.TransitionFromAny) || (function == eFunction.closeProgram))
            {
                if (arg1 == "")
                    arg1 = "0";
                if (embedded[int.Parse(arg1)] == true)
                {
                    setScreenshot(int.Parse(arg1));
                    OpenMobile.Framework.OSSpecific.unEmbedApplication("TouchBrowser", int.Parse(arg1));
                }
                embedded[int.Parse(arg1)] = false;
            }
        }

        private void setScreenshot(int screen)
        {
            Form f = (Form)Form.FromHandle(theHost.UIHandle(0));
            object o;
            theHost.getData(eGetData.GetScaleFactors, "", "0", out o);
            if (o == null)
                return;
            PointF scale = (PointF)o;
            Bitmap bmp = new Bitmap((int)(1000 * scale.X), (int)(500 * scale.Y));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                if (f.WindowState == FormWindowState.Maximized)
                    g.CopyFromScreen(new Point(f.Left, f.Top + (int)(100 * scale.Y)), Point.Empty, bmp.Size);
                else
                    g.CopyFromScreen(new Point(f.Left + 8, f.Top + 30 + (int)(100 * scale.Y)), Point.Empty, bmp.Size);
            }
            ((OMImage)manager[screen][0]).Image = new imageItem(bmp);
        }

        public OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            return manager[screen];
        }
        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            get { return "WebBrowser"; }
        }
        public string displayName
        {
            get { return "Web Browser"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "A Touch Screen Web Browser"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IDisposable Members

        public void Dispose()
        {
            Process[] p = Process.GetProcessesByName("TouchBrowser");
            if (p.Length > 0)
                p[0].CloseMainWindow();
        }

        #endregion
    }
}
