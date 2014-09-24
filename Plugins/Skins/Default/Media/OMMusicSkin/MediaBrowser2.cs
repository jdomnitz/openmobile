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
using OpenMobile.Threading;
using System.Linq;

namespace OMMusicSkin
{
    class MediaBrowser2
    {
        private HighLevelCode _MainPlugin;
        private OMListItem.subItemFormat _MediaListSubItemFormat = new OMListItem.subItemFormat();
        private IMediaDatabase _DB = null;
        private IEnumerable<mediaInfo> _DBItems;

        public OMPanel Initialize(HighLevelCode mainPlugin)
        {
            _MainPlugin = mainPlugin;

            // Create a new panel
            OMPanel panel = new OMPanel("MusicBrowser", "Music > Music browser", OM.Host.getSkinImage("AIcons|4-collections-view-as-list"));

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            OMList lstMedia = new OMList("lstMedia", shapeBackground.Region.Left + 10, shapeBackground.Region.Top + 10, shapeBackground.Region.Width - 80, shapeBackground.Region.Height - 20);
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
            //lstMedia.OnClick += new userInteraction(lstMedia_OnClick);
            //lstMedia.OnHoldClick += new userInteraction(lstMedia_OnHoldClick);
            //lstMedia.OnScrollStart += new userInteraction(lstMedia_OnScrollStart);
            //lstMedia.OnScrolling += new userInteraction(lstMedia_OnScrolled);
            //lstMedia.OnScrollEnd += new userInteraction(lstMedia_OnScrollEnd);
            panel.addControl(lstMedia);

            _MediaListSubItemFormat.color = Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinTextColor);
            _MediaListSubItemFormat.textAlignment = Alignment.BottomLeft;

            panel.Entering += panel_Entering;

            return panel;
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Save reference to db
            _DB = OM.Host.getData(eGetData.GetMediaDatabase, "") as IMediaDatabase;

            SafeThread.Asynchronous(() =>
                {
                    _DBItems = _DB.getSongs();
                    List_ShowSongs(sender, screen);
                });
        }

        private void List_ShowSongs(OMPanel panel, int screen)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.Distinct().OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Name);
                foreach (var item in items)
                    lst.Add(new OMListItem(item.Name, item.Artist, item.coverArt, _MediaListSubItemFormat, item));
            }
        }

        private void List_ShowArtists(OMPanel panel, int screen)
        {
            //OMList lst = panel[screen, "lstMedia"] as OMList;
            //if (lst != null)
            //{
            //    lst.Clear();
            //    var items = _DBItems.GroupBy(x=>x.Artist).Select(x=>x.
            //    foreach (var item in items)
            //        lst.Add(new OMListItem(item.Name, item.Artist, item.coverArt, _MediaListSubItemFormat, item));
            //}
        }

    }
}
