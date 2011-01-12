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
using OpenMobile.Data;

namespace Skin_Switcher
{
    public class Class1:IHighLevel
    {

        #region IHighLevel Members
        IPluginHost theHost;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (theHost.SkinPath.EndsWith("Default"))
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Highway");
            }
            else
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Default");
            }
            theHost.execute(OpenMobile.eFunction.restartProgram);
            return null;
        }

        public string displayName
        {
            get { return "Skin Switcher"; }
        }

        #endregion

        #region IBasePlugin Members

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
            get { throw new NotImplementedException(); }
        }

        public string authorEmail
        {
            get { throw new NotImplementedException(); }
        }

        public string pluginName
        {
            get { return "SkinSwitcher"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { throw new NotImplementedException(); }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
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
