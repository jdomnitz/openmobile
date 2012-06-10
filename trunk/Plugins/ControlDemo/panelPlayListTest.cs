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
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.Media;


namespace ControlDemo
{
    public static class panelPlayListTest
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static OMListItem.subItemFormat subItemformat;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            OMPanel p = new OMPanel("PlayListTest");

            // Playlist test
            OMButton btnPlayList = DefaultControls.GetButton("btnPlayList", 10, 200, 300, 90, "", "Load list");
            btnPlayList.OnClick += new userInteraction(btnPlayList_OnClick);
            p.addControl(btnPlayList);
            OMButton btnPrev = DefaultControls.GetButton("btnPrev", 10, 300, 150, 90, "", "Prev");
            btnPrev.OnClick += new userInteraction(btnPrev_OnClick);
            p.addControl(btnPrev);
            OMButton btnNext = DefaultControls.GetButton("btnNext", 170, 300, 150, 90, "", "Next");
            btnNext.OnClick += new userInteraction(btnNext_OnClick);
            p.addControl(btnNext);
            OMButton btnShuffleOn = DefaultControls.GetButton("btnShuffleOn", 10, 400, 150, 90, "", "S.On");
            btnShuffleOn.OnClick += new userInteraction(btnShuffleOn_OnClick);
            p.addControl(btnShuffleOn);
            OMButton btnShuffleOff = DefaultControls.GetButton("btnShuffleOff", 170, 400, 150, 90, "", "S.Off");
            btnShuffleOff.OnClick += new userInteraction(btnShuffleOff_OnClick);
            p.addControl(btnShuffleOff);

            OMLabel lblIndex = new OMLabel("lblIndex", 350, 200, 50, 50);
            lblIndex.FontSize = 24;
            p.addControl(lblIndex);

            //OMImage imgNoCover = new OMImage("imgNoCover", 330, 100, new imageItem(MediaLoader.MissingCoverImage));
            //p.addControl(imgNoCover);

            //OMImage imgBackground = new OMImage("imgBackground", 330, 100, host.getSkinImage("OMIconBlack_Transparent"));
            //imgBackground.BackgroundColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            //imgBackground.Left = 500 - (imgBackground.Image.image.Width / 2);
            //imgBackground.Top = 300 - (imgBackground.Image.image.Height / 2);
            //p.addControl(imgBackground);            

            subItemformat = new OMListItem.subItemFormat();
            subItemformat.color = Color.FromArgb(175, Color.Black);
            subItemformat.highlightColor = Color.LightGray;
            subItemformat.font = new Font(Font.GenericSansSerif, 20F);

            //OMImage imgReflectionTestSource = new OMImage("imgReflectionTestSource", 700, 100, host.getSkinImage("OM.png"));
            ////imgReflectionTestSource.Rotation = new OpenMobile.Math.Vector3(0, 0, 45);
            //p.addControl(imgReflectionTestSource);

            //OMLabel lblRotationX = new OMLabel("lblRotationX", 525, 388, 200, 40);
            //lblRotationX.Text = "X rotation:";
            //p.addControl(lblRotationX);

            //OMSlider Slider_RotationX = new OMSlider("Slider_RotationX", 700, 400, 250, 25, 12, 40);
            //Slider_RotationX.Slider = Host.getSkinImage("Slider");
            //Slider_RotationX.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationX.Maximum = 180;
            //Slider_RotationX.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationX);

            //OMLabel lblRotationY = new OMLabel("lblRotationY", 525, 438, 200, 40);
            //lblRotationY.Text = "Y rotation:";
            //p.addControl(lblRotationY);

            //OMSlider Slider_RotationY = new OMSlider("Slider_RotationY", 700, 450, 250, 25, 12, 40);
            //Slider_RotationY.Slider = Host.getSkinImage("Slider");
            //Slider_RotationY.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationY.Maximum = 180;
            //Slider_RotationY.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationY);

            //OMLabel lblRotationZ = new OMLabel("lblRotationZ", 525, 488, 200, 40);
            //lblRotationZ.Text = "Z rotation:";
            //p.addControl(lblRotationZ);

            //OMSlider Slider_RotationZ = new OMSlider("Slider_RotationZ", 700, 500, 250, 25, 12, 40);
            //Slider_RotationZ.Slider = Host.getSkinImage("Slider");
            //Slider_RotationZ.SliderBar = Host.getSkinImage("Slider.Bar");
            //Slider_RotationZ.Maximum = 180;
            //Slider_RotationZ.OnSliderMoved += new OMSlider.slidermoved(Slider_Rotation_OnSliderMoved);
            //p.addControl(Slider_RotationZ);

            //OMImage imgReflectionTestTarget = new OMImage("", 700, 100 + imgReflectionTestSource.Height);
            //if (imgReflectionTestSource.Image != null)
            //    imgReflectionTestTarget.Image = OpenMobile.Graphics.GDI.Reflection.GetReflection(imgReflectionTestSource.Image, 1.4F, true);
            //p.addControl(imgReflectionTestTarget);


            //OMList List_BufferItems = new OMList("List_BufferItems", 410, 100, 500, 430);
            //List_BufferItems.Scrollbars = true;
            //List_BufferItems.ListStyle = eListStyle.MultiList;
            //List_BufferItems.Background = Color.Transparent;
            //List_BufferItems.ItemColor1 = Color.Transparent;
            //List_BufferItems.Font = new Font(Font.GenericSansSerif, 28F);
            //List_BufferItems.Color = Color.White;
            //List_BufferItems.HighlightColor = Color.White;
            //List_BufferItems.SelectedItemColor1 = Color.DarkBlue;
            //List_BufferItems.ListItemHeight = 70;
            //List_BufferItems.ListItemOffset = 80;
            //p.addControl(List_BufferItems);

            OMList List_HistoryItems = new OMList("List_HistoryItems", 410, 100, 500, 190);
            List_HistoryItems.Scrollbars = true;
            List_HistoryItems.ListStyle = eListStyle.MultiList;
            List_HistoryItems.Background = Color.Transparent;
            List_HistoryItems.ItemColor1 = Color.Transparent;
            List_HistoryItems.Font = new Font(Font.GenericSansSerif, 28F);
            List_HistoryItems.Color = Color.White;
            List_HistoryItems.HighlightColor = Color.White;
            List_HistoryItems.SelectedItemColor1 = Color.DarkBlue;
            List_HistoryItems.ListItemHeight = 70;
            List_HistoryItems.ListItemOffset = 80;
            p.addControl(List_HistoryItems);

            OMLabel lblCurrentItem = new OMLabel("lblCurrentItem", 410, 290, 500, 50);
            lblCurrentItem.FontSize = 24;
            lblCurrentItem.TextAlignment = Alignment.CenterLeft;
            p.addControl(lblCurrentItem);

            OMList List_QueueItems = new OMList("List_QueueItems", 410, 350, 500, 190);
            List_QueueItems.Scrollbars = true;
            List_QueueItems.ListStyle = eListStyle.TextList;
            List_QueueItems.Background = Color.Silver;
            List_QueueItems.ItemColor1 = Color.Red;
            List_QueueItems.Font = new Font(Font.GenericSansSerif, 28F);
            List_QueueItems.Color = Color.White;
            List_QueueItems.HighlightColor = Color.White;
            List_QueueItems.SelectedItemColor1 = Color.DarkBlue;
            List_QueueItems.ListItemHeight = 70;
            p.addControl(List_QueueItems);

            manager.loadPanel(p);
        }

        static void Slider_Rotation_OnSliderMoved(OMSlider sender, int screen)
        {
            OMImage img = sender.Parent[screen, "imgReflectionTestSource"] as OMImage;
            OMLabel lblX = sender.Parent[screen, "lblRotationX"] as OMLabel;
            OMLabel lblY = sender.Parent[screen, "lblRotationY"] as OMLabel;
            OMLabel lblZ = sender.Parent[screen, "lblRotationZ"] as OMLabel;
            OMSlider sldrX = sender.Parent[screen, "Slider_RotationX"] as OMSlider;
            OMSlider sldrY = sender.Parent[screen, "Slider_RotationY"] as OMSlider;
            OMSlider sldrZ = sender.Parent[screen, "Slider_RotationZ"] as OMSlider;

            int XValue = sldrX.Value;
            int YValue = sldrY.Value;
            int ZValue = sldrZ.Value;

            img.Rotation = new OpenMobile.Math.Vector3(XValue, YValue, ZValue);
            lblX.Text = String.Format("X Rotation: {0}", XValue);
            lblY.Text = String.Format("Y Rotation: {0}", YValue);
            lblZ.Text = String.Format("Z Rotation: {0}", ZValue);
        }

        static void btnShuffleOff_OnClick(OMControl sender, int screen)
        {
            playlist.Random = false;
            UpdatePlayListData(screen, sender.Parent);
        }

        static void btnShuffleOn_OnClick(OMControl sender, int screen)
        {
            playlist.Random = true;
            UpdatePlayListData(screen, sender.Parent);
        }

        static void UpdatePlayListData(int screen, OMPanel p)
        {
            ((OMLabel)p[screen, "lblIndex"]).Text = playlist.CurrentIndex.ToString();

            ((OMLabel)p[screen, "lblCurrentItem"]).Text = playlist.CurrentItem.Name;//.Location.Substring(playlist.CurrentItem.Location.LastIndexOf('\\'));

            /*
            OMList BufferListControl = p[screen, "List_BufferItems"] as OMList;
            BufferListControl.Clear();
            mediaInfo[] BufferList = playlist.BufferList;
            for (int i = 0; i < BufferList.Length; i++)
                BufferListControl.Add(new OMListItem(BufferList[i].Name, BufferList[i].Artist, BufferList[i].coverArt, subItemformat, BufferList[i].Location));
            BufferListControl.SelectedIndex = playlist.BufferListIndex;
            */

            OMList HistoryList = p[screen, "List_HistoryItems"] as OMList;
            HistoryList.Clear();
            for (int i = 0; i < playlist.HistoryItems.Length; i++)
                //HistoryList.Add(new OMListItem(playlist.HistoryItems[i].Name));//playlist.HistoryItems[i].Location.Substring(playlist.HistoryItems[i].Location.LastIndexOf('\\')))));
                HistoryList.Add(new OMListItem(playlist.HistoryItems[i].Name, playlist.HistoryItems[i].Artist, playlist.HistoryItems[i].coverArt, subItemformat, playlist.HistoryItems[i].Location));
            HistoryList.SelectedIndex = playlist.HistoryItems.Length - 1;

            OMList QueueList = p[screen, "List_QueueItems"] as OMList;
            QueueList.Clear();
            for (int i = 0; i < playlist.QueuedItems.Length; i++)
                QueueList.Add(new OMListItem(playlist.QueuedItems[i].Name));//playlist.QueuedItems[i].Location.Substring(playlist.QueuedItems[i].Location.LastIndexOf('\\')))));
            QueueList.SelectedIndex = 0;
        }

        static void btnNext_OnClick(OMControl sender, int screen)
        {
            if (playlist != null)
                playlist.GotoNextMedia();

            UpdatePlayListData(screen, sender.Parent);
        }

        static void btnPrev_OnClick(OMControl sender, int screen)
        {
            if (playlist != null)
                playlist.GotoPreviousMedia();

            UpdatePlayListData(screen, sender.Parent);
        }

        static PlayList playlist = null;

        static void btnPlayList_OnClick(OMControl sender, int screen)
        {
            playlist = new PlayList("TestList");

            List<string> PlayLists = PlaylistHandler.listPlaylistsFromDB();
            if (PlayLists.Count > 0)
            {
                foreach (mediaInfo MediaItem in PlaylistHandler.readPlaylistFromDB(PlayLists[PlayLists.Count-1]))
                {
                    playlist.Add(MediaLoader.UpdateMissingMediaInfo(MediaItem));

                    /*
                    mediaInfo tmp = info; // <-stupid .Net limitation
                    if (tmp.Name == null)
                        tmp = OpenMobile.Media.TagReader.getInfo(info.Location);
                    if (tmp == null)
                        continue;
                    if (tmp.coverArt == null)
                        tmp.coverArt = TagReader.getCoverFromDB(tmp.Artist, tmp.Album, theHost, dbname[screen]);
                    if (tmp.coverArt == null)
                        tmp.coverArt = theHost.getSkinImage("Unknown Album").image;
                    l.Add(new OMListItem(tmp.Name, tmp.Artist, tmp.coverArt, format, tmp.Location));
                    */

                }
                UpdatePlayListData(screen, sender.Parent);
            }
        }

    }
}
