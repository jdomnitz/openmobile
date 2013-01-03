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
using OpenMobile.Plugin;
using System.Windows.Forms;
using System.Threading;

namespace OMWinInfo
{
    public sealed class OMWinInfo : IBasePlugin
    {
        #region IBasePlugin Members

        public eLoadStatus initialize(IPluginHost host)
        {
            if (Configuration.RunningOnWindows)
                return eLoadStatus.LoadSuccessful;
            else
                return eLoadStatus.LoadFailedUnloadRequested;
        }

        public Settings loadSettings()
        {
            Settings settings = new Settings("OMWinInfo Settings");
            settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);

            settings.Add(new Setting(SettingTypes.Button, "OMWinInfo.Show", String.Empty, "Show info form", null, null));

            return settings;
        }

        void settings_OnSettingChanged(int screen, Setting setting)
        {
            switch (setting.Name)
            {
                case "OMWinInfo.Show":
                    {
                        // Spawn windows form
                        if (th == null)
                        {
                            th = new Thread(SpawnForm);
                            th.IsBackground = true;
                            th.SetApartmentState(ApartmentState.STA);
                            th.Start();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        Thread th = null;
        private void SpawnForm()
        {
            frmWinInfo frm = new frmWinInfo();
            frm.ShowDialog();
            th = null;            
        }

        public string authorName
        {
            get { return "OM DevTeam/Borte"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "OMWinInfo"; }
        }

        public float pluginVersion
        {
            get { return 1.0f; }
        }

        public string pluginDescription
        {
            get { return "Provides system and data info in a separate windows form"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-OM"); }
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
