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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace OMNavStub
{
    public class Class1:IHighLevel
    {
        #region IHighLevel Members
        OMPanel p = new OMPanel();
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return p;
        }
        private Image getImage(string location)
        {
            string request=@"http://maps.google.com/maps/api/staticmap?center="+location+"&zoom=16&size=512x512&maptype=roadmap&sensor=false&markers=color:red|color:red|label:A|"+location;
            return OpenMobile.Net.Network.imageFromURL(request);
        }
        public OpenMobile.Controls.OMPanel loadSettings(string name, int screen)
        {
            throw new NotImplementedException();
        }

        public string displayName
        {
            get { return "Nav Stub"; }
        }

        #endregion

        #region IBasePlugin Members

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
            get { return "OMNavStub"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Navigation Stub"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            OMImage img = new OMImage(200, 70, 400, 400);
            OMLabel desc = new OMLabel(650, 200, 300, 250);
            p.addControl(desc);
            p.addControl(img);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.navigateToAddress)
            {
                if (arg1 == "Recognition Failed")
                    return;
                ((OMImage)p[1]).Image = new imageItem(getImage(arg1));
                ((OMLabel)p[0]).Text = formatMe(arg1);
                bool b=theHost.execute(eFunction.TransitionFromAny, "0");
                b=theHost.execute(eFunction.TransitionToPanel, "0","OMNavStub");
                b=theHost.execute(eFunction.ExecuteTransition, "0","None");
            }
        }

        private string formatMe(string arg1)
        {
            int x = arg1.IndexOf(',');
            return arg1.Remove(x) + "\r\n" + arg1.Substring(x+1);
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
