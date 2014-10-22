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
    internal class PlayListTestSkin
    {
        private OMMusicSkin _MainPlugin;
        private IMediaDatabase _DB = null;
        private IEnumerable<mediaInfo> _DBItems;
        private Playlist _Playlist = new Playlist("TestList");
        private OMListItem.subItemFormat _MediaListSubItemFormat = new OMListItem.subItemFormat();

        public OMPanel Initialize(OMMusicSkin mainPlugin)
        {
            _MainPlugin = mainPlugin;

            // Create a new panel
            OMPanel panel = new OMPanel("PlaylistSkin", "Music > Playlist", OM.Host.getSkinImage("AIcons|4-collections-view-as-list"));

            /*
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
            lstMedia.AssignListItemAction = item =>
                {
                    if (item is mediaInfo)
                    {
                        var mediaItem = item as mediaInfo;
                        return new OMListItem(mediaItem.Name, mediaItem.Artist, mediaItem.coverArt, _MediaListSubItemFormat, mediaItem);
                    }
                    return new OMListItem(item.ToString());
                };
            lstMedia.OnListChanged += new OMList.IndexChangedDelegate(lstMedia_OnListChanged);
            lstMedia.OnScrollStart += new userInteraction(lstMedia_OnScrollStart);
            lstMedia.OnScrollEnd += new userInteraction(lstMedia_OnScrollEnd);
            panel.addControl(lstMedia);
            */

            // Coverflow
            //OMImageFlow2 lst_ViewPlaylist_ImgFlow = new OMImageFlow2("lst_ViewPlaylist_ImgFlow", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            ////lst_ViewPlaylist_ImgFlow.ListSource = _Playlist;            
            //lst_ViewPlaylist_ImgFlow.Animation_InsertionOfNewItems = false;
            //lst_ViewPlaylist_ImgFlow.AssignListItemAction = item =>
            //{
            //    if (item is mediaInfo)
            //    {
            //        var mediaItem = item as mediaInfo;
            //        return mediaItem.coverArt;
            //    }
            //    if (item is OImage)
            //        return (OImage)item;

            //    return null;
            //};

            //panel.addControl(lst_ViewPlaylist_ImgFlow);

            OMMediaFlow lst_ViewPlaylist_ImgFlow = new OMMediaFlow("lst_ViewPlaylist_ImgFlow", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);

            OMImageFlow.PreConfigLayout_Flat_ApplyToControl(lst_ViewPlaylist_ImgFlow);
            lst_ViewPlaylist_ImgFlow.MediaInfoFormatString = "{1} - {0} - {6}";
            lst_ViewPlaylist_ImgFlow.FontSize = 20;
            lst_ViewPlaylist_ImgFlow.Color = Color.FromArgb(178, Color.White);
            lst_ViewPlaylist_ImgFlow.TextAlignment = Alignment.TopCenter;
            lst_ViewPlaylist_ImgFlow.Animation_FadeOutDistance = 6;
            lst_ViewPlaylist_ImgFlow.ReflectionsEnabled = false;
            lst_ViewPlaylist_ImgFlow.NoUserInteraction = true;
            lst_ViewPlaylist_ImgFlow.Control_PlacementOffsetY = 5;
            lst_ViewPlaylist_ImgFlow.ImageSize = new Size(lst_ViewPlaylist_ImgFlow.Height * 0.6f, lst_ViewPlaylist_ImgFlow.Height * 0.6f);

            lst_ViewPlaylist_ImgFlow.MediaListSource = OMMediaFlow.ListSources.Buffer;
            panel.addControl(lst_ViewPlaylist_ImgFlow);

            //OMMediaFlow lst_ViewPlaylist_ImgFlow = new OMMediaFlow("lst_ViewPlaylist_ImgFlow", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            //lst_ViewPlaylist_ImgFlow.ClipRendering = false;
            //lst_ViewPlaylist_ImgFlow.Animation_FadeOutDistance = 2f;
            //lst_ViewPlaylist_ImgFlow.ReflectionsEnabled = false;
            ////lstImgFlow.overlay = OM.Host.getSkinImage("Images|Overlay-BlackBottom").image;
            ////lstImgFlow.overlay.RenderText(0, lstImgFlow.overlay.Height - 110, lstImgFlow.overlay.Width, 100, "OpenMobile Coverflow sample", new Font(Font.Arial, 25), eTextFormat.Normal, Alignment.CenterCenter, Color.White, Color.White, FitModes.Fit);
            ////lst_ViewPlaylist_ImgFlow.overlay.AddImageOverlay(lst_ViewPlaylist_ImgFlow.overlay.Width - 64, lst_ViewPlaylist_ImgFlow.overlay.Height - 64, OM.Host.getSkinImage("AIcons|9-av-play-over-video").image);
            //lst_ViewPlaylist_ImgFlow.NoMediaImage = MediaLoader.MissingCoverImage;
            //lst_ViewPlaylist_ImgFlow.MediaInfoFormatString = "{1} - {0}\n{6}";
            //lst_ViewPlaylist_ImgFlow.OnClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnClick);
            //lst_ViewPlaylist_ImgFlow.OnHoldClick += new userInteraction(lst_ViewPlaylist_ImgFlow_OnHoldClick);
            //panel.addControl(lst_ViewPlaylist_ImgFlow);


            OMButton btnNext = OMButton.PreConfigLayout_CleanStyle("btnNext", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, 150, 90, text: "Next");
            btnNext.OnClick += new userInteraction(btnNext_OnClick);
            panel.addControl(btnNext);
            OMButton btnPrevious = OMButton.PreConfigLayout_CleanStyle("btnPrevious", 0, 0, 150, 90, text: "Previous");
            btnPrevious.OnClick += new userInteraction(btnPrevious_OnClick);
            panel.addControl(btnPrevious, ControlDirections.Down);
            OMButton btnToggleShuffle = OMButton.PreConfigLayout_CleanStyle("btnToggleShuffle", 0, 0, 150, 90, text: "Shuffle");
            btnToggleShuffle.OnClick += new userInteraction(btnToggleShuffle_OnClick);
            panel.addControl(btnToggleShuffle, ControlDirections.Down);
            OMButton btnSave = OMButton.PreConfigLayout_CleanStyle("btnSave", 0, 0, 150, 90, text: "Save");
            btnSave.OnClick += new userInteraction(btnSave_OnClick);
            panel.addControl(btnSave, ControlDirections.Down);
            OMButton btnLoad = OMButton.PreConfigLayout_CleanStyle("btnSave", 0, 0, 150, 90, text: "Load");
            btnLoad.OnClick += new userInteraction(btnLoad_OnClick);
            panel.addControl(btnLoad, ControlDirections.Down);

            OMButton btnClear = OMButton.PreConfigLayout_CleanStyle("btnClear", OM.Host.ClientArea_Init.Right - 150, OM.Host.ClientArea_Init.Top, 150, 90, text: "Clear");
            btnClear.OnClick += new userInteraction(btnClear_OnClick);
            panel.addControl(btnClear);
            OMButton btnAdd1 = OMButton.PreConfigLayout_CleanStyle("btnAdd1", 0, 0, 150, 90, text: "Add 1");
            btnAdd1.OnClick += new userInteraction(btnAdd1_OnClick);
            panel.addControl(btnAdd1, ControlDirections.Down);
            OMButton btnAdd10 = OMButton.PreConfigLayout_CleanStyle("btnAdd10", 0, 0, 150, 90, text: "Add 10");
            btnAdd10.OnClick += new userInteraction(btnAdd10_OnClick);
            panel.addControl(btnAdd10, ControlDirections.Down);
            OMButton btnAdd100 = OMButton.PreConfigLayout_CleanStyle("btnAdd100", 0, 0, 150, 90, text: "Add 100");
            btnAdd100.OnClick += new userInteraction(btnAdd100_OnClick);
            panel.addControl(btnAdd100, ControlDirections.Down);
            OMButton btnSelect = OMButton.PreConfigLayout_CleanStyle("btnSelect", 0, 0, 150, 90, text: "Select");
            btnSelect.OnClick += new userInteraction(btnSelect_OnClick);
            panel.addControl(btnSelect, ControlDirections.Down);

            OMLabel lblCurrentItem = new OMLabel("lblCurrentItem", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Bottom - 50, OM.Host.ClientArea_Init.Width, 50);
            panel.addControl(lblCurrentItem);

            panel.Entering += new PanelEvent(panel_Entering);

            return panel;
        }

        int selectIndex = 5;
        void btnSelect_OnClick(OMControl sender, int screen)
        {
            if (selectIndex > _Playlist.Count)
                selectIndex = _Playlist.Count - 1;
            _Playlist.CurrentIndex = selectIndex;
            selectIndex += 5;
        }

        void btnAdd100_OnClick(OMControl sender, int screen)
        {
            _Playlist.AddRange(_DBItems.Skip(_Playlist.Count).Take(100));
        }

        void btnAdd10_OnClick(OMControl sender, int screen)
        {
            _Playlist.AddRange(_DBItems.Skip(_Playlist.Count).Take(10));
        }

        void btnAdd1_OnClick(OMControl sender, int screen)
        {
            _Playlist.AddRange(_DBItems.Skip(_Playlist.Count).Take(1));
        }

        void btnClear_OnClick(OMControl sender, int screen)
        {
            _Playlist.Clear();
            selectIndex = 0;
        }

        void btnLoad_OnClick(OMControl sender, int screen)
        {
            _Playlist.Load();
        }

        void btnSave_OnClick(OMControl sender, int screen)
        {
            _Playlist.Save();
        }

        void btnToggleShuffle_OnClick(OMControl sender, int screen)
        {
            _Playlist.Shuffle = !_Playlist.Shuffle;
        }

        void lstMedia_OnScrollEnd(OMControl sender, int screen)
        {
            ((OMList)sender).Select(_Playlist.CurrentIndex, false, screen);            
        }

        void lstMedia_OnScrollStart(OMControl sender, int screen)
        {
            
        }

        void lstMedia_OnListChanged(OMList sender, int screen)
        {
            sender.Select(_Playlist.CurrentIndex, false, screen);
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

                //_Playlist.Shuffle = true;
                //_Playlist.AddRange(_DBItems.Take(100));

                GUI_Refresh(sender, screen);

                //var list = sender[screen, "lstMedia"] as OMList;
                //list.ListSource = queue;

                var list = sender[screen, "lst_ViewPlaylist_ImgFlow"] as OMMediaFlow;
                list.ListSource = _Playlist;

                
            });
        }

        void GUI_Refresh(OMPanel sender, int screen)
        {
            //var lbl = sender[screen, "lblCurrentItem"] as OMLabel;
            //if (lbl == null)
            //    return;            

            //lbl.Text = _Playlist.CurrentItem.Name.ToString();
        }
    }
}
