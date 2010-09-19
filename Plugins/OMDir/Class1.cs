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
using OpenMobile.Graphics;
using System.IO;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OMDir
{
    public class Dir : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        int[] type;
        OImage folder;

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (name == "Folder")
            {
                ((OMLabel)manager[screen][2]).Text = "Select a Folder";
                type[screen] = 1;  //Type=0: Select File; Type=1: Select Folder; Multi-Select Files=2;
            }
            else
            {
                ((OMLabel)manager[screen][2]).Text = "Select a File";
                type[screen] = 0;
            }
            if ((name != "Folder") && (name != ""))
            {
                if (Directory.Exists(name))
                    loadPath(screen, name);
            }
            return manager[screen];
        }

        public Settings loadSettings()
        {
            return null;
        }
        #region Attributes
        public string displayName
        {
            get { return "File Picker"; }
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
            get { return "OMDir"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Provides File/Folder Selection for the framework"; }
        }
        #endregion
        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnStorageEvent += new StorageEvent(on_storageEvent);
            OMPanel p = new OMPanel();
            manager = new ScreenManager(theHost.ScreenCount);
            OMLabel caption = new OMLabel(275, 95, 400, 60);
            caption.OutlineColor = Color.FromArgb(120, Color.PowderBlue);
            caption.Font = new Font(Font.GenericSansSerif, 34F);
            caption.Format = eTextFormat.Glow;
            OMButton top = new OMButton(795, 101, 180, 40);
            top.Text = "Up One Level";
            top.Image = theHost.getSkinImage("Full");
            top.FocusImage = theHost.getSkinImage("Full.Highlighted");
            top.OnClick += new userInteraction(top_OnClick);
            top.Transition = eButtonTransition.None;
            top.Visible = false;
            OMButton select = new OMButton(20, 101, 230, 40);
            select.Text = "Select this Folder";
            select.Image = top.Image;
            select.FocusImage = top.FocusImage;
            select.OnClick += new userInteraction(select_OnClick);
            select.Visible = false;
            OMList right = new OMList(510, 150, 470, 375);
            right.Font = new Font(Font.GenericSansSerif, 28F);
            right.OnClick += new userInteraction(right_OnClick);
            right.ListStyle = eListStyle.DroidStyleImage;
            right.Background = Color.FromArgb(180, Color.LightGray);
            right.ItemColor1 = Color.FromArgb(0, 0, 16);
            right.ClickToSelect = true;
            OMList left = new OMList(15, 150, 470, 375);
            left.Font = right.Font;
            left.OnClick += new userInteraction(left_OnClick);
            left.SelectedIndexChanged += new OMList.IndexChangedDelegate(left_SelectedIndexChanged);
            left.ListStyle = eListStyle.DroidStyleImage;
            left.Background = right.Background;
            left.ItemColor1 = right.ItemColor1;
            folder = theHost.getSkinImage("Folder", true).image;
            loadRoot(left);
            OMBasicShape border = new OMBasicShape(10, 146, 975, 383);
            border.BorderColor = Color.Silver;
            border.BorderSize = 4F;
            border.CornerRadius = 10;
            border.FillColor = right.ItemColor1;
            border.Shape = shapes.RoundedRectangle;
            OMBasicShape back = new OMBasicShape(0, 0, 1000, 600);
            back.Shape = shapes.Rectangle;
            back.FillColor = Color.Black;
            p.addControl(back);
            p.addControl(border);
            p.addControl(caption);
            p.addControl(left);
            p.addControl(right);
            p.addControl(top);
            p.addControl(select);
            manager.loadPanel(p);
            type = new int[theHost.ScreenCount];
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Default.DirectoryBrowser", "OMDir");
            return eLoadStatus.LoadSuccessful;
        }

        private void on_storageEvent(eMediaType type, bool justInserted, string path)
        {
            if (type == eMediaType.NotSet)
                return;
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                OMList l = (OMList)manager[i][3];
                if (l.Tag == null)
                    return;
                if (l.Tag.ToString() == "")
                    loadRoot(l);
            }
        }

        void top_OnClick(object sender, int screen)
        {
            OMList l = (OMList)manager[screen][3];
            OMList r = (OMList)manager[screen][4];
            if (l.Tag == null)
                return;
            r.Tag = l.Tag;
            l.Clear();
            if (System.IO.Path.GetPathRoot(l.Tag.ToString()) == l.Tag.ToString())
            {
                l.Tag = "";
                loadRoot(l);
                ((OMButton)manager[screen][5]).Visible = false;
            }
            else
            {
                l.Tag = Directory.GetParent(l.Tag.ToString()).FullName;
                DirectoryInfo fInfo = new DirectoryInfo(l.Tag.ToString());
                foreach (DirectoryInfo s in fInfo.GetDirectories())
                    if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        l.Add(new OMListItem(s.Name, folder));
            }
            l.Select(l.indexOf(new DirectoryInfo(r.Tag.ToString()).Name));
            left_OnClick(l, screen); //refresh the right screen
        }

        void select_OnClick(object sender, int screen)
        {
            OMList l = (OMList)manager[screen][3];
            theHost.execute(eFunction.userInputReady, screen.ToString(), "Dir", translateLocal(l));
        }

        private string translateLocal(OMList l)
        {
            string source = "";
            if (l.Tag == null)
            {
                //source = OpenMobile.Path.Combine(l.Tag.ToString(), l[l.SelectedIndex].text);
                source = l[l.SelectedIndex].text;
                switch (source)
                {
                    case "Desktop":
                        return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    case "Documents":
                        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    case "Pictures":
                        return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    case "Music":
                        return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    case "Videos":
                        return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).Replace("Music", "Videos");
                }
                if (l[l.SelectedIndex].subItem != null)
                    return l[l.SelectedIndex].subItem;
            }
            else
            {
                if (l[l.SelectedIndex] == null)
                    throw new Exception("DAMN!");
                source = OpenMobile.Path.Combine(l.Tag.ToString(), l[l.SelectedIndex].text);
                if (l.Tag.ToString() == "")
                    l.Tag = null;
            }
            return source;
        }

        void left_SelectedIndexChanged(OMList sender, int screen)
        {
            if (sender.SelectedIndex != -1)
                if ((screen >= 0) && (type[screen] == 1))
                    ((OMButton)manager[screen][6]).Visible = true;
        }
        private void loadRoot(OMList l)
        {
            l.Add(new OMListItem("Desktop", folder));
            l.Add(new OMListItem("Documents", folder));
            l.Add(new OMListItem("Pictures", folder));
            l.Add(new OMListItem("Music", folder));
            l.Add(new OMListItem("Videos", folder));
            foreach (string drive in Environment.GetLogicalDrives())
            {
                switch (OSSpecific.getDriveType(drive))
                {
                    case OSSpecific.eDriveType.CDRom:
                        l.Add(new OMListItem(OSSpecific.getVolumeLabel(drive), drive, theHost.getSkinImage("Drives|CD-ROM Drive").image));
                        break;
                    case OSSpecific.eDriveType.Fixed:
                    case OSSpecific.eDriveType.Unknown:
                        l.Add(new OMListItem(OSSpecific.getVolumeLabel(drive), drive, theHost.getSkinImage("Drives|Local Drive").image));
                        break;
                    case OSSpecific.eDriveType.Network:
                        l.Add(new OMListItem(OSSpecific.getVolumeLabel(drive), drive, theHost.getSkinImage("Drives|Network Drive").image));
                        break;
                    case OSSpecific.eDriveType.Removable:
                        l.Add(new OMListItem(OSSpecific.getVolumeLabel(drive), drive, theHost.getSkinImage("Drives|Removable Drive").image));
                        break;
                    case OSSpecific.eDriveType.Phone:
                        l.Add(new OMListItem(OSSpecific.getVolumeLabel(drive), drive, theHost.getSkinImage("Discs|Phone").image));
                        break;
                }
            }
        }

        private void loadPath(int screen, string path)
        {
            OMList l = (OMList)manager[screen][3];
            OMList r = (OMList)manager[screen][4];
            r.Tag = path;
            l.Clear();
            if (System.IO.Path.GetPathRoot(path) == path)
            {
                l.Tag = "";
                loadRoot(l);
                ((OMButton)manager[screen][5]).Visible = false;
            }
            else
            {
                l.Tag = Directory.GetParent(path).FullName;
                DirectoryInfo fInfo = new DirectoryInfo(path);
                foreach (DirectoryInfo s in fInfo.GetDirectories())
                    if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        l.Add(new OMListItem(s.Name, folder));
            }
            l.Select(l.indexOf(new DirectoryInfo(path).Name));
            left_OnClick(l, screen); //refresh the right screen
        }
        void right_OnClick(object sender, int screen)
        {
            OMList r = (OMList)manager[screen][4];
            OMList l = ((OMList)manager[screen][3]);
            if (r.SelectedIndex == -1)
                return;
            string source = OpenMobile.Path.Combine(r.Tag.ToString(), r[r.SelectedIndex].text);
            if (System.IO.Path.HasExtension(source) == true)
            {
                theHost.execute(eFunction.userInputReady, screen.ToString(), "Dir", source);
                return;
            }
            l.Clear();
            DirectoryInfo fInfo = new DirectoryInfo(r.Tag.ToString());
            foreach (DirectoryInfo s in fInfo.GetDirectories())
                if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    l.Add(new OMListItem(s.Name, folder));
            l.Select(r.SelectedIndex);
            l.Tag = r.Tag;
            r.Clear();
            r.Tag = source;
            DirectoryInfo info = new DirectoryInfo(source);
            try
            {
                foreach (DirectoryInfo s in info.GetDirectories())
                    if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        r.Add(new OMListItem(s.Name, folder));
            }
            catch (Exception) { }
            try
            {
                if (type[screen] == 0)
                {
                    foreach (FileInfo s in info.GetFiles())
                        if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                            r.Add(s.Name);
                }
            }
            catch (Exception) { }
            ((OMButton)manager[screen][5]).Visible = true;
        }

        void left_OnClick(object sender, int screen)
        {
            OMList r = (OMList)manager[screen][4];
            OMList l = ((OMList)manager[screen][3]);
            r.Clear();
            string source = translateLocal(l);
            r.Tag = source;
            DirectoryInfo info = new DirectoryInfo(source);
            try
            {
                foreach (DirectoryInfo s in info.GetDirectories())
                    if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        r.Add(new OMListItem(s.Name, folder));
            }
            catch (Exception) { }
            try
            {
                if (type[screen] == 0)
                {
                    foreach (FileInfo s in info.GetFiles())
                        if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                            r.Add(s.Name);
                }
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            if (manager != null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
