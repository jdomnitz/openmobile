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
using System.Drawing;
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Windows.Forms;

namespace Navigation
{
    public sealed class OMNavigation : IHighLevel
    {

        #region IHighLevel Members
        ScreenManager manager;
        IPluginHost theHost;

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel();
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            Object o;
            host.getData(eGetData.GetMap, "", out o);
            if (o == null)
                return eLoadStatus.LoadFailedRetryRequested;
            p.addControl(o as OMControl);
            imageItem img = theHost.getSkinImage("Tab", true);
            OMButton setButton = new OMButton(175, 533, 150, 70);
            setButton.Image = img;
            setButton.Transition = eButtonTransition.None;
            setButton.Text = "Settings";
            setButton.OnClick += new userInteraction(setButton_OnClick);
            p.addControl(setButton);
            OMButton toolsButton = new OMButton(340, 533, 150, 70);
            toolsButton.Image = img;
            toolsButton.Transition = eButtonTransition.None;
            toolsButton.Text = "Tools";
            toolsButton.OnClick += new userInteraction(toolsButton_OnClick);
            p.addControl(toolsButton);
            OMButton navButton = new OMButton(510, 533, 150, 70);
            navButton.Image = img;
            navButton.Transition = eButtonTransition.None;
            navButton.Text = "Nav Back";
            navButton.OnClick += new userInteraction(navButton_OnClick);
            p.addControl(navButton);
            manager = new ScreenManager(theHost.ScreenCount);
            manager.loadPanel(p);
            return eLoadStatus.LoadSuccessful;
        }

        void toolsButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.showNavPanel, "Tools");
        }

        void navButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.showNavPanel, "NavBack");
        }

        void setButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.showNavPanel, "Settings");
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.navigateToAddress)
            {
                theHost.execute(eFunction.TransitionFromAny, "0");
                theHost.execute(eFunction.TransitionToPanel, "0", "OMNavigation");
                theHost.execute(eFunction.ExecuteTransition, "0");
            }
            if (function == eFunction.gesture)
            {
                if (arg3 == "OMNavigation")
                {
                    if (arg2 == "")
                        return;
                    if (arg2 == "/")
                        return;
                    if (arg2 == "\\")
                        return;
                    if (arg2 == " ")
                        arg2 = "Space";
                    if (arg2 == "back")
                        arg2 = "Back";
                    int screen = int.Parse(arg1);
                    Keys k = (Keys)Enum.Parse(typeof(Keys), arg2);
                    ((IKey)manager[screen][0]).KeyDown(screen, new KeyEventArgs(k), 1.0F, 1.0F);
                    Thread.Sleep(100);
                    ((IKey)manager[screen][0]).KeyUp(screen, new KeyEventArgs(k), 1.0F, 1.0F);
                }
            }
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen];
        }
        public OMPanel loadSettings(string name, int screen)
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
            get { return "admin@domnitzsolutions.com"; }
        }

        public string pluginName
        {
            get { return "OMNavigation"; }
        }
        public string displayName
        {
            get { return "Navigation"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Displays the navigation software"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
