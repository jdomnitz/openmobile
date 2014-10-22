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
using System.IO;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Media;
using OpenMobile.Plugin;
using System.Collections.Generic;

namespace ControlDemo
{
    [PluginLevel(PluginLevels.System)]
    public class AutoGeneratedClass : IHighLevel
    {
        IPluginHost theHost;
        OMPanel p;
        OMList List3;
        private static string lastPath;
        public eLoadStatus initialize(OpenMobile.Plugin.IPluginHost host)
        {
            theHost = host;
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            p = new OMPanel();
            OMImage Image1 = new OMImage(275, 115, 400, 400);
            Image1.Image = theHost.getSkinImage("MediaBorder");
            OMLabel Label2 = new OMLabel(430, 150, 250, 40);
            Label2.Format = eTextFormat.Bold;
            Label2.TextAlignment = Alignment.CenterLeft;
            OMImage icon = new OMImage(350, 135, 60, 60);
            List3 = new OMList(286, 200, 378, 290);
            List3.Background = Color.Silver;
            List3.ListStyle = eListStyle.DroidStyleImage;
            List3.ItemColor1 = Color.Black;
            List3.ListItemHeight = 70;
            List3.OnClick += new userInteraction(List3_OnClick);
            p.addControl(Image1);
            p.addControl(Label2);
            p.addControl(List3);
            p.addControl(icon);
            p.Forgotten = true;
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.promptDialNumber)
            {
                if (OpenMobile.helperFunctions.Params.IsParamsValid(args, 1))
                {
                    List3.Clear();
                    lastPath = OpenMobile.helperFunctions.Params.GetParam<string>(args, 0);
                    ((OMLabel)p[1]).Text = OpenMobile.Framework.Globalization.formatPhoneNumber(OpenMobile.helperFunctions.Params.GetParam<string>(args, 0));
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|Phone", true);
                    imageItem itm = theHost.getSkinImage("Discs|Dial", true);
                    List3.Add(new OMListItem("Dial Number", itm.image));
                    itm = theHost.getSkinImage("Discs|Add", true);
                    List3.Add(new OMListItem("Add To Contacts", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        theHost.execute(eFunction.TransitionToPanel, i.ToString(), "OMNotify", "notify");
                        theHost.execute(eFunction.ExecuteTransition, i.ToString(), "SlideDown");
                    }
                }
            }
        }

        void List3_OnClick(OMControl sender, int screen)
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                theHost.execute(eFunction.TransitionFromPanel, i.ToString(), "OMNotify", "notify");
                theHost.execute(eFunction.ExecuteTransition, i.ToString(), "None");
            }
            switch (List3[List3.SelectedIndex].text)
            {
                case "Play CD":
                    if (Configuration.RunningOnWindows)
                    {
                        //string[] songs = Directory.GetFiles(lastPath);
                        //if (theHost.setPlaylist(PlayList.Convert(songs), screen))
                        //    theHost.execute(eFunction.nextMedia, screen.ToString());
                    }
                    else if (Configuration.RunningOnLinux)
                    {
                        //string name = DeviceInfo.get(lastPath).VolumeLabel;
                        //string[] arg = name.Split(new char[] { '|' });
                        //List<string> songs = new List<string>();
                        //int tracks = int.Parse(arg[1]);
                        //for (int i = 1; i <= tracks; i++)
                        //    songs.Add("cdda://" + i.ToString());
                        //if (theHost.setPlaylist(PlayList.Convert(songs), screen))
                        //    theHost.execute(eFunction.nextMedia, screen.ToString());
                    }
                    break;
                case "Play DVD":
                case "Play Blu-Ray":
                case "Play HDDVD":
                    if (theHost.execute(eFunction.Play, screen.ToString(), lastPath))
                        theHost.sendMessage("UI", "OMNotify", "ShowMediaControls" + screen.ToString());
                    break;
                case "Play Playlists":
                    //DeviceInfo info = DeviceInfo.get(lastPath);
                    //List<mediaInfo> media = new List<mediaInfo>();
                    ////theHost.SendStatusData(eDataType.Info, this, "", "Loading playlists...");
                    //if (info.PlaylistFolders.Length == 0)
                    //    return;
                    //foreach (string playlist in PlayList.listPlaylists(info.PlaylistFolders[0]))
                    //    media.AddRange(PlayList.readPlaylist(playlist));
                    //theHost.setPlaylist(media, screen);
                    //theHost.execute(eFunction.Play, screen.ToString(), media[0].Location);
                    break;
                case "Eject":
                    theHost.execute(eFunction.ejectDisc, lastPath);
                    break;
                case "View Files":
                    theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMDir", lastPath);
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "None");
                    break;
                case "Dial Number":
                    theHost.execute(eFunction.dialNumber, lastPath);
                    break;
                case "View Pictures":
                    DeviceInfo info2 = DeviceInfo.get(lastPath);
                    string picPath = string.Empty;
                    for (int i = 0; i < info2.PictureFolders.Length; i++)
                    {
                        picPath = info2.PictureFolders[i];
                        if (Directory.Exists(picPath) && Directory.GetFiles(picPath).Length > 0)
                            break;
                    }
                    if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Slideshow", picPath))
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    break;
            }
            List3.SelectedIndex = -1;
        }

        void theHost_OnStorageEvent(eMediaType type, bool justInserted, string arg)
        {
            lastPath = arg;
            List3.Clear();
            switch (type)
            {
                case eMediaType.DeviceRemoved:
                    //TODO-better way
                    IconManager.UIIcon removeMe = new IconManager.UIIcon(theHost.getSkinImage("Discs|AudioCD"), ePriority.MediumHigh, true, "OMNotify");
                    removeMe.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "RemoveIcon", ref removeMe);
                    removeMe = new IconManager.UIIcon(theHost.getSkinImage("Discs|DVD"), ePriority.MediumHigh, true, "OMNotify");
                    removeMe.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "RemoveIcon", ref removeMe);
                    removeMe = new IconManager.UIIcon(theHost.getSkinImage("Discs|BluRay"), ePriority.MediumHigh, true, "OMNotify");
                    removeMe.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "RemoveIcon", ref removeMe);
                    return;
                case eMediaType.NotSet:
                    ((OMLabel)p[1]).Text = "Identifying . . .";
                    ((OMImage)p[3]).Image = new imageItem();
                    break;
                case eMediaType.AudioCD:
                    ((OMLabel)p[1]).Text = "Audio CD";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|AudioCD", true);
                    imageItem itm = theHost.getSkinImage("Discs|Play", true);
                    List3.Add(new OMListItem("Play CD", itm.image));
                    itm = theHost.getSkinImage("Discs|Rip", true);
                    List3.Add(new OMListItem("Rip CD", itm.image));
                    itm = theHost.getSkinImage("Discs|Eject", true);
                    List3.Add(new OMListItem("Eject", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    IconManager.UIIcon audiocd = new IconManager.UIIcon(theHost.getSkinImage("Discs|AudioCD"), ePriority.MediumHigh, true, "OMNotify");
                    audiocd.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "AddIcon", ref audiocd);
                    return;
                case eMediaType.DVD:
                    ((OMLabel)p[1]).Text = "DVD";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|DVD");
                    itm = theHost.getSkinImage("Discs|Play", true);
                    List3.Add(new OMListItem("Play DVD", itm.image));
                    itm = theHost.getSkinImage("Discs|Eject", true);
                    List3.Add(new OMListItem("Eject", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    IconManager.UIIcon dvd = new IconManager.UIIcon(theHost.getSkinImage("Discs|DVD"), ePriority.MediumHigh, true, "OMNotify");
                    dvd.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "AddIcon", ref dvd);
                    return;
                case eMediaType.HDDVD:
                    ((OMLabel)p[1]).Text = "HDDVD";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|HDDVD");
                    itm = theHost.getSkinImage("Discs|Play", true);
                    List3.Add(new OMListItem("Play HDDVD", itm.image));
                    itm = theHost.getSkinImage("Discs|Eject", true);
                    List3.Add(new OMListItem("Eject", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    IconManager.UIIcon hddvd = new IconManager.UIIcon(theHost.getSkinImage("Discs|HDDVD"), ePriority.MediumHigh, true, "OMNotify");
                    hddvd.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "AddIcon", ref hddvd);
                    return;
                case eMediaType.BluRay:
                    ((OMLabel)p[1]).Text = "Blu-Ray";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|BluRay");
                    itm = theHost.getSkinImage("Discs|Play", true);
                    List3.Add(new OMListItem("Play Blu-Ray", itm.image));
                    itm = theHost.getSkinImage("Discs|Eject", true);
                    List3.Add(new OMListItem("Eject", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    IconManager.UIIcon bluray = new IconManager.UIIcon(theHost.getSkinImage("Discs|BluRay"), ePriority.MediumHigh, true, "OMNotify");
                    bluray.tag = lastPath;
                    theHost.sendMessage("UI", "OMNotify", "AddIcon", ref bluray);
                    return;
                case eMediaType.Camera:
                    ((OMLabel)p[1]).Text = "Camera";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|Camera", true);
                    itm = theHost.getSkinImage("Discs|Slideshow", true);
                    List3.Add(new OMListItem("View Pictures", itm.image));
                    itm = theHost.getSkinImage("Discs|Add", true);
                    List3.Add(new OMListItem("Copy Photos to Disk", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    return;
                case eMediaType.LocalHardware:
                    ((OMLabel)p[1]).Text = "USB Drive";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|LocalHardware", true);
                    itm = theHost.getSkinImage("Folder", true);
                    List3.Add(new OMListItem("View Files", itm.image));
                    itm = theHost.getSkinImage("Discs|Copy", true);
                    List3.Add(new OMListItem("Copy Files to Disk", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    return;
                case eMediaType.AppleDevice:
                    ((OMLabel)p[1]).Text = "Apple Device";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|iPodiPhone", true);
                    itm = theHost.getSkinImage("Discs|Play", true);
                    List3.Add(new OMListItem("Play All Music", itm.image));
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    return;
                case eMediaType.Smartphone:
                    ((OMLabel)p[1]).Text = "Phone";
                    ((OMImage)p[3]).Image = theHost.getSkinImage("Discs|Phone", true);
                    DeviceInfo info = DeviceInfo.get(arg);
                    if (info.PlaylistFolders.Length > 0)
                    {
                        itm = theHost.getSkinImage("Discs|Play", true);
                        List3.Add(new OMListItem("Play Playlists", itm.image));
                    }
                    if (info.MusicFolders.Length > 0)
                    {
                        itm = theHost.getSkinImage("Discs|Play", true);
                        List3.Add(new OMListItem("Play All Music", itm.image));
                    }
                    if (info.PictureFolders.Length > 0)
                    {
                        itm = theHost.getSkinImage("Discs|Slideshow", true);
                        List3.Add(new OMListItem("View Pictures", itm.image));
                    }
                    itm = theHost.getSkinImage("Discs|Close", true);
                    List3.Add(new OMListItem("Close", itm.image));
                    return;
            }
            if (justInserted == false)
                return;
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                theHost.execute(eFunction.TransitionToPanel, i.ToString(), "OMNotify", "notify");
                theHost.execute(eFunction.ExecuteTransition, i.ToString(), "SlideDown");
            }
        }



        public OMPanel loadPanel(string name, int screen)
        {
            if (name == "notify")
                return p;
            return null;
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
            get { return "OMNotify"; }
        }
        public string displayName
        {
            get { return "Device Notifier"; }
        }
        public float pluginVersion
        {
            get { return 1.0F; }
        }

        public string pluginDescription
        {
            get { return "Displays AutoPlay Dialogs"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-OM"); }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            IconManager.UIIcon ui = data as IconManager.UIIcon;
            if (data != null)
            {
                theHost_OnStorageEvent(eMediaType.NotSet, true, ui.tag);
                if (ui.image == theHost.getSkinImage("Discs|AudioCD"))
                    theHost_OnStorageEvent(eMediaType.AudioCD, true, ui.tag);
                else if (ui.image == theHost.getSkinImage("Discs|DVD"))
                    theHost_OnStorageEvent(eMediaType.DVD, true, ui.tag);
                else if (ui.image == theHost.getSkinImage("Discs|BluRay"))
                    theHost_OnStorageEvent(eMediaType.BluRay, true, ui.tag);
                else if (ui.image == theHost.getSkinImage("Discs|HDDVD"))
                    theHost_OnStorageEvent(eMediaType.HDDVD, true, ui.tag);
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            //
        }
    }
}