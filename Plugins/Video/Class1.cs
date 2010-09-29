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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.helperFunctions;
using OpenMobile.Plugin;

namespace Video
{
    public class Video:IHighLevel
    {
        #region IHighLevel Members

        public OMPanel loadPanel(string name, int screen)
        {
            if (theHost == null)
                return null;
            if (theHost.execute(eFunction.showVideoWindow, theHost.instanceForScreen(screen).ToString()) == true)
                return null;
            General.getFilePath path = new General.getFilePath(theHost);
            string url=path.getFile(screen, "MainMenu", "");
            if (url!=null)
                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), url);
            return null;
        }

        public string displayName
        {
            get { return "Video"; }
        }

        #endregion

        #region IBasePlugin Members
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
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
            get { return "Video"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Video Browser"; }
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
            // Nothing to do
        }

        #endregion
    }
}
