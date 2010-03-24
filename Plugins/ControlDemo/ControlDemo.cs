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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ControlDemo
{
    public sealed class ControlDemo: IHighLevel
    {

    #region IHighLevel Members
    OMPanel p = new OMPanel();
    ScreenManager manager;
    IPluginHost theHost;
    bool[] embedded;
    public eLoadStatus initialize(IPluginHost host)
    {
        theHost = host;
        OMImage ss = new OMImage(0, 100, 1000, 500);
        p.addControl(ss);
        manager = new ScreenManager(theHost.ScreenCount);
        manager.loadPanel(p);
        embedded = new bool[theHost.ScreenCount];
        return eLoadStatus.LoadSuccessful;
    }

    public OMPanel loadPanel(string name,int screen)
    {
        return manager[screen];
    }
    public OMPanel loadSettings(string name,int screen)
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
        get { return "ControlDemo"; }
    }
    public string displayName
        {
            get { return "Control Demo"; }
        }
    public float pluginVersion
    {
        get { return 0.1F; }
    }

    public string pluginDescription
    {
        get { return "Demonstrate inter-panel transitions and the larger control set"; }
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
        //
    }

    #endregion
    }
}
