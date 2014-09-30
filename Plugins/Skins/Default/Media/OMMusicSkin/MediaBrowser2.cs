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
        private enum ListState
        {
            Artist,
            Album,
            Song
        }

        private HighLevelCode _MainPlugin;
        private OMListItem.subItemFormat _MediaListSubItemFormat = new OMListItem.subItemFormat();
        private IMediaDatabase _DB = null;
        private IEnumerable<mediaInfo> _DBItems;
        private ControlGroup _cgProgress;
        private ListState[] _ListState = new ListState[OM.Host.ScreenCount];
        

        public OMPanel Initialize(HighLevelCode mainPlugin)
        {
            _MainPlugin = mainPlugin;

            // Create a new panel
            OMPanel panel = new OMPanel("MusicBrowser", "Music > Music browser", OM.Host.getSkinImage("AIcons|4-collections-view-as-list"));

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left + 190, OM.Host.ClientArea_Init.Top + 25, OM.Host.ClientArea_Init.Width - 380, OM.Host.ClientArea_Init.Height - 50,
                new ShapeData(shapes.Rectangle, Color.FromArgb(140, Color.Black), Color.Transparent, 0));
            //shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            OMList lstMedia = new OMList("lstMedia", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, shapeBackground.Height);
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
            lstMedia.OnClick += new userInteraction(lstMedia_OnClick);
            //lstMedia.OnHoldClick += new userInteraction(lstMedia_OnHoldClick);
            //lstMedia.OnScrollStart += new userInteraction(lstMedia_OnScrollStart);
            //lstMedia.OnScrolling += new userInteraction(lstMedia_OnScrolled);
            //lstMedia.OnScrollEnd += new userInteraction(lstMedia_OnScrollEnd);
            panel.addControl(lstMedia);

            #region Left side buttons

            OMButton btnListFilter_Artist = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnListFilter_Artist",
                    -1,
                    OM.Host.ClientArea_Init.Center.Y - (125 / 2) - 125 - 20, 
                    175, 
                    125,
                    corners: GraphicCorners.Right,
                    //borderColor: Color.Transparent,                    
                    text: "  Artist",
                    iconImg: OM.Host.getSkinImage("AIcons|6-social-cc-bcc").image
                );
            btnListFilter_Artist.OnClick += new userInteraction(btnListFilter_Artist_OnClick);
            panel.addControl(btnListFilter_Artist);

            OMButton btnListFilter_Album = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnListFilter_Album",
                    0,
                    0,
                    btnListFilter_Artist.Width,
                    btnListFilter_Artist.Height,
                    corners: GraphicCorners.Right,
                    //borderColor: Color.Transparent,                    
                    text: "  Album",
                    iconImg: OM.Host.getSkinImage("AIcons|6-social-person").image
               );
            btnListFilter_Album.OnClick += new userInteraction(btnListFilter_Album_OnClick);
            panel.addControl(btnListFilter_Album, ControlDirections.Down, 0, 20);

            OMButton btnListFilter_Song = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnListFilter_Song",
                    0,
                    0,
                    btnListFilter_Artist.Width,
                    btnListFilter_Artist.Height,
                    corners: GraphicCorners.Right,
                    //borderColor: Color.Transparent,
                    text: "  Song",
                    iconImg: OM.Host.getSkinImage("Icons|Icon-Music").image                    
                );
            btnListFilter_Song.OnClick += new userInteraction(btnListFilter_Song_OnClick);
            panel.addControl(btnListFilter_Song, ControlDirections.Down, 0, 20);

            #endregion

            #region Right side buttons

            OMButton btnRight1 = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnRight1",
                    OM.Host.ClientArea_Init.Right - 174,
                    OM.Host.ClientArea_Init.Center.Y - (125 / 2) - 125 - 20,
                    175,
                    125,
                    corners: GraphicCorners.Left,
                //borderColor: Color.Transparent,
                    text: "  Play",
                    iconImg: OM.Host.getSkinImage("AIcons|9-av-play").image
                    
                );
            panel.addControl(btnRight1);

            OMButton btnRight2 = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnRight2",
                    0,
                    0,
                    btnListFilter_Artist.Width,
                    btnListFilter_Artist.Height,
                    corners: GraphicCorners.Left,
                //borderColor: Color.Transparent,
                    text: "  Add",
                    iconImg: OM.Host.getSkinImage("AIcons|9-av-add-to-queue").image
                );
            panel.addControl(btnRight2, ControlDirections.Down, 0, 20);

            OMButton btnRight3 = OMButton.PreConfigLayout_CleanStyle
                (
                    "btnRight3",
                    0,
                    0,
                    btnListFilter_Artist.Width,
                    btnListFilter_Artist.Height,
                    corners: GraphicCorners.Left,
                //borderColor: Color.Transparent,
                    text: "  Playlist",
                    iconImg: OM.Host.getSkinImage("AIcons|4-collections-view-as-list").image
               );
            panel.addControl(btnRight3, ControlDirections.Down, 0, 20);

            #endregion

            #region ControlGroup: Updating

            _cgProgress = new ControlGroup();

            OMImage imgUpdatingBackground = new OMImage("imgUpdatingBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            imgUpdatingBackground.BackgroundColor = Color.FromArgb(150, Color.Black);
            _cgProgress.Add(imgUpdatingBackground);

            OMImage imgUpdating = new OMImage("imgUpdating", 0, 0, OM.Host.getSkinImage("BusyAnimationTransparent.gif"));
            imgUpdating.PlaceRelativeToControl(imgUpdatingBackground, ControlDirections.CenterHorizontally);
            imgUpdating.PlaceRelativeToControl(imgUpdatingBackground, ControlDirections.CenterVertically);
            _cgProgress.Add(imgUpdating);

            OMLabel lblUpdating = new OMLabel("lblUpdating", 0, 0, imgUpdatingBackground.Width, 25, "Loading items, please wait...");
            lblUpdating.PlaceRelativeToControl(imgUpdating, ControlDirections.Down);
            lblUpdating.PlaceRelativeToControl(imgUpdating, ControlDirections.CenterHorizontally);
            _cgProgress.Add(lblUpdating);

            panel.addControlGroup(0, 0, _cgProgress);

            #endregion

            panel.Entering += panel_Entering;

            _MediaListSubItemFormat.color = Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinTextColor);
            _MediaListSubItemFormat.textAlignment = Alignment.BottomLeft;

            return panel;
        }

        void btnListFilter_Song_OnClick(OMControl sender, int screen)
        {
            List_SetState(sender.Parent, screen, ListState.Song);
            List_ShowSongs(sender.Parent, screen);
        }

        void btnListFilter_Album_OnClick(OMControl sender, int screen)
        {
            List_SetState(sender.Parent, screen, ListState.Album);
            List_ShowAlbums(sender.Parent, screen);           
        }

        void btnListFilter_Artist_OnClick(OMControl sender, int screen)
        {
            List_SetState(sender.Parent, screen, ListState.Artist);
            List_ShowArtists(sender.Parent, screen);
        }

        void lstMedia_OnClick(OMControl sender, int screen)
        {
            OMList lst = sender as OMList;
            mediaInfo mediaItem = lst.SelectedItem.tag as mediaInfo;

            switch (_ListState[screen])
            {
                case ListState.Artist:
                    List_SetState(sender.Parent, screen, ListState.Album);
                    List_ShowAlbumsForArtist(sender.Parent, screen, mediaItem.Artist);
                    break;
                case ListState.Album:
                    List_SetState(sender.Parent, screen, ListState.Song);
                    List_ShowSongsForAlbum(sender.Parent, screen, mediaItem.Artist, mediaItem.Album);
                    break;
                case ListState.Song:
                    break;
                default:
                    break;
            }
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Save reference to db
            _DB = OM.Host.getData(eGetData.GetMediaDatabase, "") as IMediaDatabase;

            SafeThread.Asynchronous(() =>
                {
                    if (_DBItems == null)
                    {
                        _cgProgress.SetVisible(sender, screen, true);
                        _DBItems = _DB.getSongs();
                        _cgProgress.SetVisible(sender, screen, false);
                    }
                    List_SetState(sender, screen, ListState.Artist);
                    List_ShowArtists(sender, screen);
                });
        }

        private void List_SetState(OMPanel panel, int screen, ListState state)
        {
            _ListState[screen] = state;

            OMButton btnListFilter_Artist = panel[screen, "btnListFilter_Artist"] as OMButton;
            OMButton btnListFilter_Album = panel[screen, "btnListFilter_Album"] as OMButton;
            OMButton btnListFilter_Song = panel[screen, "btnListFilter_Song"] as OMButton;

            if (btnListFilter_Album == null || btnListFilter_Artist == null || btnListFilter_Song == null)
                return;

            switch (state)
            {
                case ListState.Artist:
                    btnListFilter_Album.Checked = false;
                    btnListFilter_Artist.Checked = true;
                    btnListFilter_Song.Checked = false;
                    break;
                case ListState.Album:
                    btnListFilter_Album.Checked = true;
                    btnListFilter_Artist.Checked = false;
                    btnListFilter_Song.Checked = false;
                    break;
                case ListState.Song:
                    btnListFilter_Album.Checked = false;
                    btnListFilter_Artist.Checked = false;
                    btnListFilter_Song.Checked = true;
                    break;
                default:
                    break;
            }

        }

        private void List_ShowAlbums(OMPanel panel, int screen)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.GroupBy(x => x.Album, (album, item) =>
                    new
                    {
                        covers = item.Select(x=>x.coverArt),
                        album = item.First()
                    });

                foreach (var item in items)
                {
                    lst.Add(new OMListItem(item.album.Album, item.album.Artist, item.album.coverArt, _MediaListSubItemFormat, item.album));
                }
            }
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

        private void List_ShowSongsForArtist(OMPanel panel, int screen, string artist)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.Where(x=>x.Artist == artist).OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Name);
                foreach (var item in items)
                    lst.Add(new OMListItem(item.Name, item.Artist, item.coverArt, _MediaListSubItemFormat, item));
            }
        }

        private void List_ShowSongsForAlbum(OMPanel panel, int screen, string artist, string album)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.Where(x => x.Album == album && x.Artist == artist).OrderBy(x => x.TrackNumber).ThenBy(x => x.Name);
                foreach (var item in items)
                    lst.Add(new OMListItem(item.Name, item.Artist, item.coverArt, _MediaListSubItemFormat, item));
            }
        }

        private void List_ShowAlbumsForArtist(OMPanel panel, int screen, string artist)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.Where(x=>x.Artist == artist).GroupBy(x => x.Album, (album, item) =>
                    new
                    {
                        albums = item.Select(x => x).First(),
                        album = album
                    });

                foreach (var item in items)
                {
                    lst.Add(new OMListItem(item.albums.Album, item.albums.Artist, item.albums.coverArt, _MediaListSubItemFormat, item.albums));
                }
            }
        }

        private void List_ShowArtists(OMPanel panel, int screen)
        {
            OMList lst = panel[screen, "lstMedia"] as OMList;
            if (lst != null)
            {
                lst.Clear();
                var items = _DBItems.GroupBy(x => x.Artist, (artist, item) =>
                    new
                    {
                        albums = item.Select(x=>x),
                        albumCount = item.GroupBy(x => x.Album).Count(),
                        covers = item.GroupBy(x => x.Album).Select(x=>x.First()).Select(x=>x.coverArt),
                        artist = artist
                    });

                foreach (var item in items)
                {
                    string artistInfo = "";
                    if (item.albumCount == 0)
                        artistInfo = String.Format("{0} (No albums)", item.artist);
                    else if (item.albumCount == 1)
                        artistInfo = String.Format("{0} ({1} album)", item.artist, item.albumCount);
                    else
                        artistInfo = String.Format("{0} ({1} albums)", item.artist, item.albumCount);
                    var coverArtMosaic = OpenMobile.helperFunctions.Graphics.Images.CreateMosaic(item.covers.ToList(), 200, 200);
                    lst.Add(new OMListItem(item.artist, artistInfo, coverArtMosaic, _MediaListSubItemFormat, item.albums.First()));
                }
            }
        }

    }
}
