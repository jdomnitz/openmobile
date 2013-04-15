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
using OpenMobile.Graphics;
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.Graphics.OpenGL;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.Media;

namespace OMClock
{
    [SkinIcon("#¸")]
    public sealed class OMClock : IHighLevel
    {

        #region IHighLevel Members
        ScreenManager manager;
        IPluginHost theHost;

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel("Clock");
            theHost = host;
            manager = new ScreenManager(this);

            Rectangle ClientArea = theHost.ClientArea[0];

            // Create a new controlgroup
            ControlGroup clockGroup = new ControlGroup();

            // Clock and date
            OMLabel labelClockTime = new OMLabel("labelClockTime", 0, 0, theHost.ClientArea[0].Width, 200);
            labelClockTime.TextAlignment = Alignment.CenterCenter;
            labelClockTime.Font = new Font(Font.LED, 130F);
            labelClockTime.AutoFitTextMode = FitModes.FitFillSingleLine;
            labelClockTime.Text = "{System.Time}";
            clockGroup.Add(labelClockTime);
            OMLabel labelClockdate = new OMLabel("labelClockdate", labelClockTime.Region.Left, labelClockTime.Region.Bottom + 20, labelClockTime.Region.Width, 100);
            labelClockdate.TextAlignment = Alignment.CenterCenter;
            labelClockdate.Font = new Font(Font.LED, 50F);
            labelClockdate.AutoFitTextMode = FitModes.FitFillSingleLine;
            labelClockdate.Text = "{System.Date.Long}";
            clockGroup.Add(labelClockdate);

            // Place group in center
            p.addControlGroup(ClientArea.Left, ClientArea.Center.Y - clockGroup.Region.Center.Y, clockGroup);

            p.Entering += new PanelEvent(p_Entering);
            p.Leaving += new PanelEvent(p_Leaving);
            manager.loadPanel(p, true);

            return eLoadStatus.LoadSuccessful;
        }


        void p_Leaving(OMPanel sender, int screen)
        {
        }

        void p_Entering(OMPanel sender, int screen)
        {
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen, name];
        }
        public Settings loadSettings()
        {
            return null;
        }
        #endregion
        #region IBasePlugin Members

        public string authorName
        {
            get { return "Borte"; }
        }

        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }

        public string pluginName
        {
            get { return "OMClock"; }
        }
        public string displayName
        {
            get { return "Clock display"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "A large clock"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-Clock2"); }
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
    }
}
