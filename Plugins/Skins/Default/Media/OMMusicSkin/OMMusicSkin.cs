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
using System.Collections.Generic;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.helperFunctions;
using System.Drawing.Drawing2D;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.MenuObjects;

namespace OMMusicSkin
{
    public sealed class OMMusicSkin : HighLevelCode
    {
        /// <summary>
        /// Setting: Preload music browser items
        /// </summary>
        public bool Setting_PreloadMusicBrowserEnabled
        {
            get
            {
                return this._Setting_PreloadMusicBrowserEnabled;
            }
            set
            {
                if (this._Setting_PreloadMusicBrowserEnabled != value)
                {
                    this._Setting_PreloadMusicBrowserEnabled = value;
                }
            }
        }
        private bool _Setting_PreloadMusicBrowserEnabled = true;
        

        public OMMusicSkin()
            : base("OMMusicSkin", OM.Host.getSkinImage("Icons|Icon-Music"), 1f, "Music player", "Music", "OM Dev team/Borte", "")
        {
        }


        public override OMPanel loadPanel(string name, int screen)
        {
            //if (String.IsNullOrEmpty(name))
            //{
            //    // Show now playing as default screen if coming from the main menu if media is playing, otherwise show the default screen
            //    if (OM.Host.DataHandler.GetDataSourceValue<bool>(screen, "Zone.Playback.Playing"))
            //        return base.loadPanel("NowPlaying", screen);
            //}

            return base.loadPanel(name, screen);
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Queue panels
            //PanelManager.QueuePanel(panelPlaylistView.PanelName, new panelPlaylistView(this).Initialize, true);
            //PanelManager.QueuePanel(panelNowPlaying.PanelName, new panelNowPlaying(this).Initialize);
            PanelManager.QueuePanel("MediaBrowser", new panelMediaBrowser(this).Initialize, true);

            // Settings
            Settings_MapVariables();

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        protected override void Settings()
        {
            base.Setting_Add(Setting.BooleanSetting("MusicBrowser.Preload.Enabled", String.Empty, "Preload music browser items", StoredData.Get(this, "MusicBrowser.Preload.Enabled")), _Setting_PreloadMusicBrowserEnabled);
        }

        private void Settings_MapVariables()
        {
            _Setting_PreloadMusicBrowserEnabled = StoredData.GetBool(this, "MusicBrowser.Preload.Enabled");
        }

        protected override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);
            Settings_MapVariables();
        }

    }
}
