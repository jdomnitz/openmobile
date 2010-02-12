using System;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using System.Drawing;
using OpenMobile;
using System.IO;

namespace OMDir
{
    public class Dir:IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        int[] type;
        Image folder;

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (name == "Folder")
            {
                type[screen] = 1;  //Type=0: Select File; Type=1: Select Folder; Multi-Select Files=2;
            }
            else
                ((OMLabel)manager[screen][0]).Text = "Select a File";
            return manager[screen];
        }

        public OpenMobile.Controls.OMPanel loadSettings(string name, int screen)
        {
            throw new NotImplementedException();
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
            OMPanel p = new OMPanel();
            manager = new ScreenManager(theHost.ScreenCount);
            OMLabel caption = new OMLabel(275, 100, 400, 60);
            caption.Text = "Select a Folder";
            caption.OutlineColor = Color.FromArgb(120, Color.PowderBlue);
            caption.Font = new Font(FontFamily.GenericSansSerif, 36F);
            caption.Format = textFormat.Glow;
            OMButton top = new OMButton(20, 105, 0, 50);
            top.Text = "Up One Level";
            top.Image = theHost.getSkinImage("Full");
            top.FocusImage = theHost.getSkinImage("Full.Highlighted");
            top.OnClick += new userInteraction(top_OnClick);
            top.Transition = eButtonTransition.None;
            OMButton select = new OMButton(140, 105,0, 50);
            select.Text = "Select this Folder";
            select.Image = top.Image;
            select.FocusImage = top.FocusImage;
            select.OnClick += new userInteraction(select_OnClick);
            OMList right = new OMList(510, 150, 470, 375);
            right.Font = new Font(FontFamily.GenericSansSerif, 28F);
            right.OnClick += new userInteraction(right_OnClick);
            right.ListStyle = eListStyle.DroidStyleImage;
            right.Background = Color.FromArgb(180,Color.LightGray);
            right.ItemColor1 = Color.FromArgb(0,0,16);
            OMList left = new OMList(15, 150, 470, 375);
            left.Font = right.Font;
            left.OnClick+=new userInteraction(left_OnClick);
            left.SelectedIndexChanged += new OMList.IndexChangedDelegate(left_SelectedIndexChanged);
            left.ListStyle = eListStyle.DroidStyleImage;
            left.Background = right.Background;
            left.ItemColor1 = right.ItemColor1;
            folder = theHost.getSkinImage("Folder", true).image;
            loadRoot(left);
            p.addControl(caption);
            p.addControl(left);
            p.addControl(right);
            p.addControl(top);
            p.addControl(select);
            manager.loadPanel(p);
            type = new int[theHost.ScreenCount];
            return eLoadStatus.LoadSuccessful;
        }

        void top_OnClick(object sender, int screen)
        {
            OMList l=(OMList)manager[screen][1];
            OMList r=(OMList)manager[screen][2];
            r.Tag = l.Tag;
            l.Clear();
            if (System.IO.Path.GetPathRoot(l.Tag) == l.Tag)
            {
                l.Tag = "";
                loadRoot(l);
                ((OMButton)manager[screen][3]).Width = 0;
            }
            else
            {
                l.Tag = Directory.GetParent(l.Tag).FullName;
                DirectoryInfo fInfo = new DirectoryInfo(l.Tag);
                foreach (DirectoryInfo s in fInfo.GetDirectories())
                    if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        l.Add(new OMListItem(s.Name, folder));
            }
            l.Select(l.indexOf(new DirectoryInfo(r.Tag).Name));
            left_OnClick(l, screen); //refresh the right screen
        }

        void select_OnClick(object sender, int screen)
        {
            OMList l=(OMList)manager[screen][1];
            theHost.execute(eFunction.userInputReady, screen.ToString(),"Dir", translateLocal(l));
        }

        private string translateLocal(OMList l)
        {
            string source = OpenMobile.Path.Combine(l.Tag, l[l.SelectedIndex].text);
            if (l.Tag == "")
            {
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
            }
            return source;
        }

        void left_SelectedIndexChanged(OMList sender, int screen)
        {
            if (sender.SelectedIndex != -1)
                if (type[screen] == 1)
                    ((OMButton)manager[screen][4]).Width = 150;
        }
        private void loadRoot(OMList l)
        {
            l.Add(new OMListItem("Desktop",folder));
            l.Add(new OMListItem("Documents",folder));
            l.Add(new OMListItem("Pictures",folder));
            l.Add(new OMListItem("Music",folder));
            l.Add(new OMListItem("Videos", folder));
            l.AddRange(Environment.GetLogicalDrives());
        }

        void right_OnClick(object sender, int screen)
        {
            OMList r = (OMList)manager[screen][2];
            OMList l = ((OMList)manager[screen][1]);
            if (r.SelectedIndex==-1)
                return;
            string source = OpenMobile.Path.Combine(r.Tag, r[r.SelectedIndex].text);
            if (System.IO.Path.HasExtension(source) == true)
            {
                theHost.execute(eFunction.userInputReady, screen.ToString(), "Dir", source);
                return;
            }
            l.Clear();
            DirectoryInfo fInfo = new DirectoryInfo(r.Tag);
            foreach (DirectoryInfo s in fInfo.GetDirectories())
                if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    l.Add(new OMListItem(s.Name, folder));
            l.Select(r.SelectedIndex);
            l.Tag = r.Tag;
            r.Clear();
            r.Tag = source;
            DirectoryInfo info =new DirectoryInfo(source);
            foreach (DirectoryInfo s in info.GetDirectories())
                if ((s.Attributes&FileAttributes.Hidden)!=FileAttributes.Hidden)
                    r.Add(new OMListItem(s.Name,folder));
            foreach (FileInfo s in info.GetFiles())
                if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    r.Add(s.Name);
            ((OMButton)manager[screen][3]).Width = 150;
        }

        void left_OnClick(object sender, int screen)
        {
            OMList r = (OMList)manager[screen][2];
            OMList l=((OMList)manager[screen][1]);
            r.Clear();
            string source=translateLocal(l);
            r.Tag = source;
            DirectoryInfo info = new DirectoryInfo(source);
            foreach (DirectoryInfo s in info.GetDirectories())
                if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    r.Add(new OMListItem(s.Name, folder));
            foreach (FileInfo s in info.GetFiles())
                //if ((s.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    r.Add(s.Name);
        }

        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            manager = null;
            theHost = null;
            GC.SuppressFinalize(this);
        }
    }
}
