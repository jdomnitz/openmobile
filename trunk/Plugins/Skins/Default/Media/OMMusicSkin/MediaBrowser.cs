﻿/*********************************************************************************
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

namespace OMMusicSkin
{
    public class MediaBrowser
    {
        IMediaDatabase db = null;

        private enum ListModes
        {
            Song,
            Artist,
            Album
        }

        private StoredData.ScreenInstanceData ScreenSpecificData = new StoredData.ScreenInstanceData();

        public OMPanel Initialize()
        {
            ScreenSpecificData.SetProperty("ListMode", ListModes.Song);
            ScreenSpecificData.SetProperty("ArtistFilter", "");
            ScreenSpecificData.SetProperty("AlbumFilter", "");
            ScreenSpecificData.SetProperty("SongFilter", "");

            // Create a new panel
            OMPanel panel = new OMPanel("PlaylistEditor", "Music > Media browser", OM.Host.getSkinImage("AIcons|4-collections-view-as-list"));

            #region media list

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 5, 900, OM.Host.ClientArea_Init.Height - 70,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            // Media list
            OMObjectList2 lstMedia = new OMObjectList2("lstMedia", shapeBackground.Region.Left + 10, shapeBackground.Region.Top + 10, shapeBackground.Region.Width - 80, shapeBackground.Region.Height - 20);
            lstMedia.OnSelectedIndexChanged += new OMObjectList2.IndexChangedDelegate(lstMedia_OnSelectedIndexChanged);
            panel.addControl(lstMedia);
            // Media list item
            #region Configure list item

            #region List item controls

            OMObjectList2.ListControlItem ItemBase = new OMObjectList2.ListControlItem();
            OMBasicShape shpListItemBackground = new OMBasicShape("shpListItemBackground", 0, 0, lstMedia.Width, 86,
                new ShapeData(shapes.Rectangle, Color.Empty, Color.Empty, 1));
            ItemBase.Add(shpListItemBackground);
            OMImage imgListCover = new OMImage("imgListCover", 1, 3, 80, 80);
            ItemBase.Add(imgListCover);
            OMLabel lblListItemHeader = new OMLabel("lblListItemHeader", imgListCover.Region.Right + 5, 0, lstMedia.Width - imgListCover.Region.Right - 5, 36, "lblListItemHeader");
            lblListItemHeader.Font = new Font(Font.GenericSansSerif, 29F);
            lblListItemHeader.AutoFitTextMode = FitModes.FitSingleLine;
            lblListItemHeader.TextAlignment = Alignment.CenterLeft;
            ItemBase.Add(lblListItemHeader);
            OMLabel lblListItemDescription = new OMLabel("lblListItemDescription", imgListCover.Region.Right, lblListItemHeader.Region.Bottom, lblListItemHeader.Region.Width, 23, "lblListItemDescription");
            lblListItemDescription.TextAlignment = Alignment.CenterLeft;
            lblListItemDescription.Color = Color.FromArgb(128, lblListItemHeader.Color);
            lblListItemDescription.Font = new Font(Font.GenericSansSerif, 18F);
            ItemBase.Add(lblListItemDescription);
            OMLabel lblListItemDescription2 = new OMLabel("lblListItemDescription2", lblListItemDescription.Region.Left, lblListItemDescription.Region.Bottom, lblListItemHeader.Region.Width, 23, "lblListItemDescription2");
            lblListItemDescription2.TextAlignment = Alignment.CenterLeft;
            lblListItemDescription2.Color = Color.FromArgb(128, lblListItemHeader.Color);
            lblListItemDescription2.Font = new Font(Font.GenericSansSerif, 18F);
            ItemBase.Add(lblListItemDescription2);
            OMImage imgListItem_Rating = new OMImage("imgListItem_Rating", shpListItemBackground.Region.Right - 110, shpListItemBackground.Region.Bottom - 25, 80, 16);
            //imgListItem_Rating.Image = OM.Host.getSkinImage("Icons|Icon-Ratings-5");
            imgListItem_Rating.Opacity = 100;
            ItemBase.Add(imgListItem_Rating);

            //OMCheckbox chkListItemCheckBox = new OMCheckbox("chkListItemCheckBox", lstListControl.Width - 40, 5, 30, 30);
            //chkListItemCheckBox.OutlineColor = lblListItemHeader.Color;
            //chkListItemCheckBox.CheckedColor = lblListItemDescription.Color;
            //chkListItemCheckBox.OnClick += new userInteraction(chkListItemCheckBox_OnClick);
            //ItemBase.Add(chkListItemCheckBox);
            OMBasicShape shpListItemSeparator = new OMBasicShape("shpListItemSeparator", 0, shpListItemBackground.Region.Bottom, lstMedia.Width, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Empty))
            });
            //new ShapeData(shapes.Rectangle, lblListItemDescription.Color, Color.Empty, 1));
            ItemBase.Add(shpListItemSeparator);

            #endregion

            #region Action: Add list items 

            // Method for adding list items
            ItemBase.Action_SetItemInfo = delegate(OMObjectList2 sender, int screen, OMObjectList2.ListControlItem item, object[] values)
            {
                if (values == null || values.Length == 0)
                    return;

                if (!(values[0] is mediaInfo))
                    return;

                mediaInfo media = values[0] as mediaInfo;
                item.Tag = media;

                // Cover image
                OMImage imgItemCover = item["imgListCover"] as OMImage;
                imgItemCover.Image = new imageItem(media.coverArt);

                // Header text
                OMLabel lblItemHeader = item["lblListItemHeader"] as OMLabel;
                lblItemHeader.Text = media.Name;

                // Description text
                OMLabel lblItemDescription = item["lblListItemDescription"] as OMLabel;
                lblItemDescription.Text = media.Artist;

                // Description2 text
                OMLabel lblItemDescription2 = item["lblListItemDescription2"] as OMLabel;
                lblItemDescription2.Text = media.Album;

                // Rating image
                OMImage imglItemRating = item["imgListItem_Rating"] as OMImage;
                imgListItem_Rating.Image = OM.Host.getSkinImage(String.Format("Icons|Icon-Ratings-{0}", media.Rating));

                // Update item count text
                OMLabel lblItemCount = sender.Parent[screen, "lblMediaListItemCount"] as OMLabel;
                lblItemCount.Text = sender.Items.Count.ToString();
            };

            #endregion

            ShapeData itemShapeData_Highlighted = new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Empty))
            };
            ShapeData itemShapeData_Selected = new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(50, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(50, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Empty))
            };
            ShapeData itemShapeData_Normal = new ShapeData(shapes.Rectangle, Color.Empty, Color.Empty, 1);

            #region Action:Select / deselect

            // Method for selecting list items
            ItemBase.Action_Select = delegate(OMObjectList2 sender, int screen, OMObjectList2.ListControlItem item)
            {
                //OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                //ShapeData shpData = shpItemBackground.ShapeData;
                //shpData.FillColor = Color.FromArgb(78, BuiltInComponents.SystemSettings.SkinFocusColor);
                //shpItemBackground.ShapeData = shpData;

                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                shpItemBackground.ShapeData = itemShapeData_Selected;
                //shpItemBackground.ShapeData = new ShapeData(shapes.Rectangle)
                //{
                //    GradientData = GradientData.CreateHorizontalGradient(
                //        new GradientData.ColorPoint(0.0, 0, Color.Empty),
                //        new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                //        new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                //        new GradientData.ColorPoint(0.5, 0, Color.Empty))
                //};
            };
            // Method for deselecting list items
            ItemBase.Action_Deselect = delegate(OMObjectList2 sender, int screen, OMObjectList2.ListControlItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                shpItemBackground.ShapeData = itemShapeData_Normal;
                //ShapeData shpData = shpItemBackground.ShapeData;
                //shpData.FillColor = Color.Empty;
                //shpItemBackground.ShapeData = shpData;
            };

            #endregion

            #region Action: Highlight/unhighlight

            // Method for highlighting list items
            ItemBase.Action_Highlight = delegate(OMObjectList2 sender, int screen, OMObjectList2.ListControlItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                shpItemBackground.ShapeData = itemShapeData_Highlighted;
            };
            // Method for unhighlighting list items
            ItemBase.Action_Unhighlight = delegate(OMObjectList2 sender, int screen, OMObjectList2.ListControlItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                if (sender.SelectedItem == item)
                    shpItemBackground.ShapeData = itemShapeData_Selected;
                else
                    shpItemBackground.ShapeData = itemShapeData_Normal;
            };
            lstMedia.ItemBase = ItemBase;

            #endregion

            #endregion

            OMLabel lblMediaListItemCount = new OMLabel("lblMediaListItemCount", lstMedia.Region.Right, shapeBackground.Region.Top+5, 60, 20);
            lblMediaListItemCount.Color = Color.FromArgb(128, Color.White);
            lblMediaListItemCount.Text = lstMedia.Items.Count.ToString();
            lblMediaListItemCount.FontSize = 15;
            panel.addControl(lblMediaListItemCount);

            // List control buttons
            OMButton btnList_ScrollUp = new OMButton("btnList_ScrollUp", lstMedia.Region.Right, lstMedia.Region.Top, 60, 60);
            btnList_ScrollUp.OverlayImage = (imageItem)OM.Host.getSkinImage("AIcons|1-navigation-collapse").Clone();
            btnList_ScrollUp.FocusImage = (imageItem)btnList_ScrollUp.OverlayImage.Clone();
            btnList_ScrollUp.OverlayImage.image.SetAlpha(0.5f);
            btnList_ScrollUp.OnClick += new userInteraction(btnList_ScrollUp_OnClick);
            btnList_ScrollUp.OnHoldClick += new userInteraction(btnList_ScrollUp_OnHoldClick);
            panel.addControl(btnList_ScrollUp);

            OMButton btnList_ScrollDown = new OMButton("btnList_ScrollDown", lstMedia.Region.Right, lstMedia.Region.Bottom - 60, 60, 60);
            btnList_ScrollDown.OverlayImage = (imageItem)OM.Host.getSkinImage("AIcons|1-navigation-expand").Clone();
            btnList_ScrollDown.FocusImage = (imageItem)btnList_ScrollDown.OverlayImage.Clone();
            btnList_ScrollDown.OverlayImage.image.SetAlpha(0.5f);
            btnList_ScrollDown.OnClick += new userInteraction(btnList_ScrollDown_OnClick);
            btnList_ScrollDown.OnHoldClick += new userInteraction(btnList_ScrollDown_OnHoldClick);
            panel.addControl(btnList_ScrollDown);

            #endregion

            OMBasicShape shapeMediaFilterBackground = new OMBasicShape("shapeMediaFilterBackground", shapeBackground.Region.Left, shapeBackground.Region.Bottom + 5, shapeBackground.Width, 50,
               new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            panel.addControl(shapeMediaFilterBackground);

            OMLabel lblMediaFilterInfo = new OMLabel("lblMediaFilterInfo", shapeMediaFilterBackground.Region.Left, shapeMediaFilterBackground.Region.Top, shapeMediaFilterBackground.Width - 230, 50);
            lblMediaFilterInfo.Color = Color.FromArgb(78, Color.White);
            lblMediaFilterInfo.Text = "Enter filter text here";
            panel.addControl(lblMediaFilterInfo);

            OMTextBox txtMediaFilter = new OMTextBox("txtMediaFilter", lblMediaFilterInfo.Region.Left, lblMediaFilterInfo.Region.Top, lblMediaFilterInfo.Region.Width, lblMediaFilterInfo.Region.Height);
            txtMediaFilter.OutlineColor = Color.FromArgb(100, Color.White);
            txtMediaFilter.BackgroundColor = Color.Empty;
            txtMediaFilter.HighlightColor = Color.White;
            txtMediaFilter.Color = Color.FromArgb(128, Color.White);
            txtMediaFilter.Text = "";
            txtMediaFilter.OSKType = OSKInputTypes.KeypadSmall;
            txtMediaFilter.OnOSKShow += txtMediaFilter_OnOSKShow;
            txtMediaFilter.OnOSKHide += txtMediaFilter_OnOSKHide;
            txtMediaFilter.OnTextChange += txtMediaFilter_OnTextChange;
            panel.addControl(txtMediaFilter);

            OMButton btnMediaFilterSong = OMButton.PreConfigLayout_BasicStyle("btnMediaFilterSong", shapeMediaFilterBackground.Region.Right - 238, shapeMediaFilterBackground.Region.Top, 80, shapeMediaFilterBackground.Region.Height, GraphicCorners.None, "", "Song", 22);
            btnMediaFilterSong.OnClick += new userInteraction(btnMediaFilterSong_OnClick);
            panel.addControl(btnMediaFilterSong);
            OMButton btnMediaFilterArtist = OMButton.PreConfigLayout_BasicStyle("btnMediaFilterArtist", 0, 0, btnMediaFilterSong.Region.Width, btnMediaFilterSong.Region.Height, GraphicCorners.None, "", "Artist", 22);
            btnMediaFilterArtist.OnClick += new userInteraction(btnMediaFilterArtist_OnClick);
            panel.addControl(btnMediaFilterArtist, ControlDirections.Right, -1, 0);
            OMButton btnMediaFilterAlbums = OMButton.PreConfigLayout_BasicStyle("btnMediaFilterAlbums", 0, 0, btnMediaFilterSong.Region.Width, btnMediaFilterSong.Region.Height, 255, 10, GraphicCorners.Right, "", null, "Album", null, null,null,null, 22);
            btnMediaFilterAlbums.OnClick += new userInteraction(btnMediaFilterAlbums_OnClick);
            panel.addControl(btnMediaFilterAlbums, ControlDirections.Right, -1, 0);

            OMButton btnMediaFilterClear = new OMButton("btnMediaFilterClear", btnMediaFilterSong.Region.Left - 50, btnMediaFilterSong.Region.Top, 50, btnMediaFilterSong.Region.Height);
            btnMediaFilterClear.FocusImage = OM.Host.getSkinImage("AIcons|5-content-remove");
            btnMediaFilterClear.Image = new imageItem(((OImage)btnMediaFilterClear.FocusImage.image.Clone()).SetAlpha(0.3f));
            btnMediaFilterClear.OnClick += new userInteraction(btnMediaFilterClear_OnClick);
            panel.addControl(btnMediaFilterClear);

            panel.Entering += panel_Entering;

            return panel;
        }

        void lstMedia_OnSelectedIndexChanged(OMObjectList2 sender, int screen)
        {
            mediaInfo media = sender.SelectedItem.Tag as mediaInfo;
            if (media == null)
                return;

            switch (MediaList_ListMode_Get(screen))
            {
                case ListModes.Song:
                    break;
                case ListModes.Artist:
                    MediaList_ListMode_Set(ListModes.Album, screen);
                    MediaList_SongFilter_Set("", media.Artist, "", screen);
                    MediaList_Search(sender.Parent, screen);
                    break;
                case ListModes.Album:
                    MediaList_ListMode_Set(ListModes.Song, screen);
                    MediaList_SongFilter_Set("", media.Artist, media.Album, screen);
                    MediaList_Search(sender.Parent, screen);
                    break;
                default:
                    break;
            }
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Save reference to db
            db = OM.Host.getData(eGetData.GetMediaDatabase, "") as IMediaDatabase;

            OMObjectList2 lst = sender[screen, "lstMedia"] as OMObjectList2;
            if (lst != null)
            {
                if (lst.Items.Count == 0)
                {
                    MediaList_ListMode_Set(ListModes.Artist, screen);
                    MediaList_Search(sender, screen);
                }
            }
        }

        #region MediaFilter buttons

        void btnMediaFilterAlbums_OnClick(OMControl sender, int screen)
        {
            MediaList_SongFilter_Clear(screen);
            MediaList_ListMode_Set(ListModes.Album, screen); 
            MediaList_Search(sender.Parent, screen);
        }

        void btnMediaFilterArtist_OnClick(OMControl sender, int screen)
        {
            MediaList_SongFilter_Clear(screen);
            MediaList_ListMode_Set(ListModes.Artist, screen);
            MediaList_Search(sender.Parent, screen);
        }

        void btnMediaFilterClear_OnClick(OMControl sender, int screen)
        {
            OMTextBox txt = sender.Parent[screen, "txtMediaFilter"] as OMTextBox;
            txt.Text = "";
        }

        void btnMediaFilterSong_OnClick(OMControl sender, int screen)
        {
            MediaList_SongFilter_Clear(screen);
            MediaList_ListMode_Set(ListModes.Song, screen);
            MediaList_Search(sender.Parent, screen);
        }

        #endregion

        #region MediaList Searching

        void MediaList_SearchSongs(OMPanel panel, int screen)
        {
            if (db == null) return;

            OMObjectList2 lst = panel[screen, "lstMedia"] as OMObjectList2;
            OMLabel lbl = panel[screen, "lblMediaFilterInfo"] as OMLabel;
            OMTextBox txt = panel[screen, "txtMediaFilter"] as OMTextBox;
            if (txt.Tag == null || !(txt.Tag is int))
                txt.Tag = 0;
            int id = (int)txt.Tag;
            id++;
            txt.Tag = id;
            lbl.Visible = String.IsNullOrEmpty(txt.Text);
            if (lst != null)
            {
                lst.Clear();
                db.beginGetSongs(songFilter: String.Format("*{0}*", txt.Text), artistFilter: String.Format("*{0}*", ScreenSpecificData.GetProperty<string>(screen, "ArtistFilter")), albumFilter: String.Format("*{0}*", ScreenSpecificData.GetProperty<string>(screen, "AlbumFilter")));
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if ((int)txt.Tag != id)
                        return;
                    lst.AddItemFromItemBase(ControlDirections.Down, info);
                    info = db.getNextMedia();
                }
            }
        }

        void MediaList_SearchArtist(OMPanel panel, int screen)
        {
            if (db == null) return;

            OMObjectList2 lst = panel[screen, "lstMedia"] as OMObjectList2;
            OMLabel lbl = panel[screen, "lblMediaFilterInfo"] as OMLabel;
            OMTextBox txt = panel[screen, "txtMediaFilter"] as OMTextBox;
            if (txt.Tag == null || !(txt.Tag is int))
                txt.Tag = 0;
            int id = (int)txt.Tag;
            id++;
            txt.Tag = id;
            lbl.Visible = String.IsNullOrEmpty(txt.Text);
            if (lst != null)
            {
                lst.Clear();
                db.beginGetArtists(artistFilter: String.Format("*{0}*", txt.Text));
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if ((int)txt.Tag != id)
                        return;
                    lst.AddItemFromItemBase(ControlDirections.Down, info);
                    info = db.getNextMedia();
                }
            }
        }

        void MediaList_SearchAlbums(OMPanel panel, int screen)
        {
            if (db == null) return;

            OMObjectList2 lst = panel[screen, "lstMedia"] as OMObjectList2;
            OMLabel lbl = panel[screen, "lblMediaFilterInfo"] as OMLabel;
            OMTextBox txt = panel[screen, "txtMediaFilter"] as OMTextBox;
            if (txt.Tag == null || !(txt.Tag is int))
                txt.Tag = 0;
            int id = (int)txt.Tag;
            id++;
            txt.Tag = id;
            lbl.Visible = String.IsNullOrEmpty(txt.Text);
            if (lst != null)
            {
                lst.Clear();
                db.beginGetAlbums(artistFilter: String.Format("*{0}*", ScreenSpecificData.GetProperty<string>(screen, "ArtistFilter")), albumFilter: String.Format("*{0}*", txt.Text));
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if ((int)txt.Tag != id)
                        return;
                    lst.AddItemFromItemBase(ControlDirections.Down, info);
                    info = db.getNextMedia();
                }
            }
        }

        void MediaList_Search(OMPanel panel, int screen)
        {
            switch (MediaList_ListMode_Get(screen))
            {
                case ListModes.Song:
                    MediaList_SearchSongs(panel, screen);
                    break;
                case ListModes.Artist:
                    MediaList_SearchArtist(panel, screen);
                    break;
                case ListModes.Album:
                    MediaList_SearchAlbums(panel, screen);
                    break;
                default:
                    break;
            }
        }

        void MediaList_SongFilter_Set(string songFilter, string artistFilter, string albumFilter, int screen)
        {
            ScreenSpecificData.SetProperty(screen, "SongFilter", songFilter);
            ScreenSpecificData.SetProperty(screen, "AlbumFilter", albumFilter);
            ScreenSpecificData.SetProperty(screen, "ArtistFilter", artistFilter);
        }
        void MediaList_SongFilter_Clear(int screen)
        {
            ScreenSpecificData.SetProperty(screen, "SongFilter", "");
            ScreenSpecificData.SetProperty(screen, "AlbumFilter", "");
            ScreenSpecificData.SetProperty(screen, "ArtistFilter", "");
        }

        void MediaList_ListMode_Set(ListModes mode, int screen)
        {
            ScreenSpecificData.SetProperty(screen, "ListMode", mode);
        }
        ListModes MediaList_ListMode_Get(int screen)
        {
            return ScreenSpecificData.GetProperty<ListModes>(screen, "ListMode");
        }

        #endregion

        #region TextBox MediaFilter

        int txtMediaFilterOSKOffset = 220;
        void txtMediaFilter_OnOSKHide(OMControl sender, int screen)
        {            
            OMTextBox txtBox = sender.Parent[screen, "txtMediaFilter"] as OMTextBox;
            txtBox.Top += txtMediaFilterOSKOffset;

            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            lst.Height += txtMediaFilterOSKOffset;

            OMButton btn = sender.Parent[screen, "btnList_ScrollDown"] as OMButton;
            btn.Top += txtMediaFilterOSKOffset;
        }

        void txtMediaFilter_OnOSKShow(OMControl sender, int screen)
        {
            OMTextBox txtBox = sender.Parent[screen, "txtMediaFilter"] as OMTextBox;
            txtBox.Top -= txtMediaFilterOSKOffset;

            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            lst.Height -= txtMediaFilterOSKOffset;

            OMButton btn = sender.Parent[screen, "btnList_ScrollDown"] as OMButton;
            btn.Top -= txtMediaFilterOSKOffset;
        }

        void txtMediaFilter_OnTextChange(OMControl sender, int screen)
        {
            //MediaList_SearchSongs(sender.Parent, screen);

            MediaList_Search(sender.Parent, screen);

            //if (db == null) return;

            //OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            //OMLabel lbl = sender.Parent[screen, "lblMediaFilterInfo"] as OMLabel;
            //OMTextBox txt = sender as OMTextBox;
            //if (txt.Tag == null || !(txt.Tag is int))
            //    txt.Tag = 0;
            //int id = (int)txt.Tag;
            //id++;
            //txt.Tag = id;
            //lbl.Visible = String.IsNullOrEmpty(txt.Text);
            //if (lst != null)
            //{
            //    lst.Clear();
            //    db.beginGetSongs(songFilter: String.Format("*{0}*", ((OMTextBox)sender).Text));
            //    mediaInfo info = db.getNextMedia();
            //    while (info != null)
            //    {
            //        if ((int)txt.Tag != id)
            //            return;
            //        lst.AddItemFromItemBase(ControlDirections.Down, info);
            //        info = db.getNextMedia();
            //    }
            //}
        }

        void txtMediaFilter_SetText(OMPanel panel, int screen, string text)
        {
            OMTextBox txt = panel[screen, "txtMediaFilter"] as OMTextBox;
            if (txt == null)
                return;

            txt.Text = text;
        }

        #endregion

        #region ListControl buttons

        void btnList_ScrollUp_OnClick(OMControl sender, int screen)
        {
            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            lst.ScrollToIndex(lst.GetCenterVisibleIndex() - 1, true, 25f);
        }
        void btnList_ScrollUp_OnHoldClick(OMControl sender, int screen)
        {
            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            OpenMobile.Threading.SafeThread.Asynchronous(() => { lst.ScrollToIndex(0, true, 100f, 10000f); });
            while (sender.Mode == eModeType.Clicked)
                System.Threading.Thread.Sleep(100);
            lst.ScrollCancel();
        }
        void btnList_ScrollDown_OnClick(OMControl sender, int screen)
        {
            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            lst.ScrollToIndex(lst.GetCenterVisibleIndex() + 1, true, 25f);
        }
        void btnList_ScrollDown_OnHoldClick(OMControl sender, int screen)
        {
            OMObjectList2 lst = sender.Parent[screen, "lstMedia"] as OMObjectList2;
            OpenMobile.Threading.SafeThread.Asynchronous(() => { lst.ScrollToIndex(lst.Items.Count - 1, true, 100f, 10000f); });
            while (sender.Mode == eModeType.Clicked)
                System.Threading.Thread.Sleep(100);
            lst.ScrollCancel();
        }

        #endregion


    }
}
