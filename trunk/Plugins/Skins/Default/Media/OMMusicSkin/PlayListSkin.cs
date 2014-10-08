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
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenMobile.Threading;

namespace OMMusicSkin
{
    internal class PlayListSkin
    {
        private OMMusicSkin _MainPlugin;
        private IMediaDatabase _DB = null;
        private IEnumerable<mediaInfo> _DBItems;
        private Playlist3 _Playlist = new Playlist3();
        private OMListItem.subItemFormat _MediaListSubItemFormat = new OMListItem.subItemFormat();

        public OMPanel Initialize(OMMusicSkin mainPlugin)
        {
            _MainPlugin = mainPlugin;

            // Create a new panel
            OMPanel panel = new OMPanel("PlaylistSkin", "Music > Playlist", OM.Host.getSkinImage("AIcons|4-collections-view-as-list"));

            // List subitem format
            _MediaListSubItemFormat.color = Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinTextColor);
            _MediaListSubItemFormat.textAlignment = Alignment.BottomLeft;

            OMList lstMedia = new OMList("lstMedia", OM.Host.ClientArea_Init.Left + 190, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width - 380, OM.Host.ClientArea_Init.Height - 50);
            lstMedia.ListStyle = eListStyle.MultiList;
            lstMedia.Color = BuiltInComponents.SystemSettings.SkinTextColor;
            lstMedia.ScrollbarColor = Color.FromArgb(120, Color.White);
            lstMedia.SeparatorColor = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstMedia.ItemColor1 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 66);
            lstMedia.ItemColor2 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 10);
            lstMedia.SelectedItemColor1 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstMedia.SelectedItemColor2 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            //lstMedia.SelectedItemColor1 = Color.Red;
            lstMedia.HighlightColor = Color.White;
            lstMedia.Font = new Font(Font.GenericSansSerif, 26F);
            lstMedia.Scrollbars = true;
            lstMedia.TextAlignment = Alignment.TopLeft;
            lstMedia.ListItemOffset = 85;
            panel.addControl(lstMedia);

            OMButton btnNext = OMButton.PreConfigLayout_CleanStyle("btnNext", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, 150, 90, text: "Next");
            btnNext.OnClick += new userInteraction(btnNext_OnClick);
            panel.addControl(btnNext);
            OMButton btnPrevious = OMButton.PreConfigLayout_CleanStyle("btnPrevious", 0, 0, 150, 90, text: "Previous");
            btnPrevious.OnClick += new userInteraction(btnPrevious_OnClick);
            panel.addControl(btnPrevious, ControlDirections.Down);

            OMLabel lblCurrentItem = new OMLabel("lblCurrentItem", lstMedia.Region.Left, lstMedia.Region.Bottom, lstMedia.Region.Width, 50);
            panel.addControl(lblCurrentItem);

            panel.Entering += new PanelEvent(panel_Entering);

            return panel;
        }

        void btnNext_OnClick(OMControl sender, int screen)
        {
            _Playlist.GotoNextMedia();
            GUI_Refresh(sender.Parent, screen);
        }

        void btnPrevious_OnClick(OMControl sender, int screen)
        {
            _Playlist.GotoPreviousMedia();
            GUI_Refresh(sender.Parent, screen);
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Save reference to db
            _DB = OM.Host.getData(eGetData.GetMediaDatabase, "") as IMediaDatabase;

            SafeThread.Asynchronous(() =>
            {
                if (_DBItems == null)
                {
                    _DBItems = _DB.getSongs();
                }

                _Playlist.AddRange(_DBItems.Take(30));

                var queue = _Playlist.Queue;
                GUI_Refresh(sender, screen);
            });
        }

        void GUI_Refresh(OMPanel sender, int screen)
        {
            var lbl = sender[screen, "lblCurrentItem"] as OMLabel;
            if (lbl == null)
                return;            

            lbl.Text = _Playlist.CurrentItem.Name.ToString();
        }
    }
}
