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
using System.Drawing;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OpenMobile.Plugin;

namespace Media
{
    public sealed class Media : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;
        string dbname = "";

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel("");
            if (host.InstanceCount == -1)
                return eLoadStatus.LoadFailedRetryRequested;
            currentAlbums = new List<mediaInfo>[host.InstanceCount];
            currentSongs = new List<mediaInfo>[host.InstanceCount];
            kickDown = new bool[host.InstanceCount];
            for (int i = 0; i < host.InstanceCount; i++)
            {
                currentAlbums[i] = new List<mediaInfo>();
                currentSongs[i] = new List<mediaInfo>();
            }
            manager = new ScreenManager(host.ScreenCount);
            theHost = host;
            imageItem item = theHost.getSkinImage("MediaSlider");
            OMImage Border = new OMImage(0,99,1000,440);
            Border.Name = "Border";
            Border.Image = theHost.getSkinImage("MediaBorder");
            OMButton Slider3 = new OMButton(920, 119, 60, 400);
            Slider3.Name = "Media.Slider3";
            Slider3.Image = item;
            Slider3.OnClick += new userInteraction(Slider3_OnClick);
            OMButton Slider1 = new OMButton(17,119,60,400);
            Slider1.Name = "Media.Slider1";
            Slider1.Image = item;
            OMLabel artists = new OMLabel(30, 220, 30, 300);
            artists.Text = "Artists";
            artists.Font = new Font(FontFamily.GenericSansSerif, 26F);
            artists.Format = eTextFormat.BoldShadow;
            artists.Name = "Media.Artists";
            artists.TextAlignment = Alignment.WordWrap;
            OMButton Slider2 = new OMButton(858,119,60,400);
            Slider2.Name = "Media.Slider2";
            Slider2.Image = item;
            Slider2.OnClick += new userInteraction(Slider2_OnClick);
            Font f = new Font("Microsoft Sans Serif", 24F);
            OMList List3 = new OMList(77,120,775,395);
            List3.ListStyle = eListStyle.RoundedTextList;
            List3.ItemColor1 = Color.FromArgb(0, 0, 66);
            List3.ItemColor2 = Color.FromArgb(0, 0, 10);
            List3.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List3.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
            List3.Name = "Media.List3";
            List3.Font = f;
            List3.ClickToSelect = true;
            List3.Add("Loading...");
            List3.SelectedIndexChanged += new OMList.IndexChangedDelegate(List3_SelectedIndexChanged);
            List3.OnClick += new userInteraction(List3_OnClick);
            List3.OnLongClick += new userInteraction(List3_OnLongClick);
            OMList List2 = new OMList(137,120,0,395);
            List2.ListStyle = eListStyle.DroidStyleImage;
            List2.ItemColor1 = Color.FromArgb(0, 0, 66);
            List2.ItemColor2 = Color.FromArgb(0, 0, 10);
            List2.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List2.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
            List2.Name = "Media.List2";
            List2.Font = f;
            List2.ClickToSelect = true;
            List2.ListItemHeight = 60;
            List2.SelectedIndexChanged += new OMList.IndexChangedDelegate(List2_SelectedIndexChanged);
            List2.OnClick += new userInteraction(List2_OnClick);
            List2.OnLongClick += new userInteraction(List2_OnLongClick);
            OMList List1 = new OMList(137,120,0,395);
            List1.ListStyle = eListStyle.DroidStyleImage;
            List1.ItemColor1 = Color.FromArgb(0, 0, 66);
            List1.ItemColor2 = Color.FromArgb(0, 0, 10);
            List1.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List1.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
            List1.ListItemHeight = 60;
            List1.Name = "Media.List1";
            List1.Font = f;
            List1.OnClick += new userInteraction(List1_OnClick);
            OMButton bSettings = new OMButton(200, 533,180,70);
            bSettings.Image = theHost.getSkinImage("SettingsButton",false);
            bSettings.FocusImage= theHost.getSkinImage("SettingsButtonFocus",false);
            bSettings.OnClick += new userInteraction(bSettings_OnClick);
            bSettings.Transition = eButtonTransition.None;
            p.addControl(Border);
            p.addControl(List1);
            p.addControl(List2);
            p.addControl(List3);
            p.addControl(Slider3);
            p.addControl(Slider1);
            p.addControl(Slider2);
            p.addControl(artists);
            p.addControl(bSettings);
            using (PluginSettings ps = new PluginSettings())
                dbname = ps.getSetting("Default.MusicDatabase");
            OpenMobile.Threading.TaskManager.QueueTask(loadList,ePriority.High,"Load Artists");//<-Where the music gets loaded
            p.DoubleClickable = false;
            manager.loadPanel(p);
            OMPanel settings = new OMPanel("Settings");
            imageItem opt2=theHost.getSkinImage("Full.Highlighted");
		    imageItem opt1=theHost.getSkinImage("Full");
		    OMLabel Title=new OMLabel(270,110,500,80);
		    Title.Font= new Font("Poor Richard",48F);
		    Title.Text="Music Settings";
		    Title.Format=eTextFormat.DropShadow;
		    Title.Name="Media.Title";
            Title.TextAlignment = Alignment.CenterCenter;
		    OMCheckbox chkIndex=new OMCheckbox(225,195,800,70);
		    chkIndex.OutlineColor=Color.FromArgb(255,142,0,0);
		    chkIndex.Font= new Font("Microsoft Sans Serif",31F);
		    chkIndex.Text="Index new music on every startup";
		    chkIndex.Name="Media.chkIndex";
            OMCheckbox AutoStart = new OMCheckbox(225, 370, 800, 70);
            AutoStart.OutlineColor = chkIndex.OutlineColor;
            AutoStart.Font = chkIndex.Font;
            AutoStart.Name = "Media.AutoStart";
            AutoStart.Text = "Resume music playback on startup";
		    OMLabel pathCaption=new OMLabel();
		    pathCaption.Font= new Font("Microsoft Sans Serif",32F);
		    pathCaption.Top=270;
		    pathCaption.Left=207;
		    pathCaption.Text="Path:";
		    pathCaption.Name="Media.pathCaption";
		    OMTextBox path=new OMTextBox(349,293,520,50);
		    path.Flags=textboxFlags.EllipsisCenter;
		    path.Font= new Font("Microsoft Sans Serif",32.25F);
		    path.Name="Media.path";
		    OMButton select=new OMButton(882,292,100,55);
		    select.Image=opt1;
		    select.FocusImage=opt2;
		    select.Text="Select";
		    select.Name="Media.select";
            select.OnClick += new userInteraction(select_OnClick);
		    OMButton Save=new OMButton(13,136,200,110);
		    Save.Image=opt1;
		    Save.FocusImage=opt2;
		    Save.Text="Save";
		    Save.Name="Media.Save";
            Save.OnClick += new userInteraction(Save_OnClick);
		    OMButton Cancel=new OMButton(14,255,200,110);
		    Cancel.Image=opt1;
		    Cancel.FocusImage=opt2;
		    Cancel.Text="Cancel";
		    Cancel.Name="Media.Cancel";
            Cancel.OnClick += new userInteraction(Cancel_OnClick);
            imageItem opt4 = theHost.getSkinImage("Play.Highlighted");
            imageItem opt3 = theHost.getSkinImage("Play");
            settings.addControl(Title);
		    settings.addControl(chkIndex);
		    settings.addControl(pathCaption);
		    settings.addControl(path);
		    settings.addControl(select);
		    settings.addControl(Save);
		    settings.addControl(Cancel);
            settings.addControl(AutoStart);
            manager.loadPanel(settings);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.backgroundOperationStatus)
            {
                if (arg1 == "Indexing Complete!")
                {
                    loadList();
                }
            }
        }

        void Save_OnClick(OMControl sender, int screen)
        {
            using (OpenMobile.Data.PluginSettings set = new OpenMobile.Data.PluginSettings())
            {
                set.setSetting("Music.Path", ((OMTextBox)manager[screen,"Settings"][3]).Text);
                if ((set.getSetting("Music.AutoIndex") != "True") && (((OMCheckbox)manager[screen, "Settings"][1]).Checked == true))
                {
                    object o = new object();
                    theHost.getData(eGetData.GetMediaDatabase, dbname, out o);
                    if (o != null)
                    {
                        IMediaDatabase db = (IMediaDatabase)o;
                        db.indexDirectory(set.getSetting("Music.Path"), true);
                    }
                }
                set.setSetting("Music.AutoIndex", ((OMCheckbox)manager[screen, "Settings"][1]).Checked.ToString());
                set.setSetting("Music.AutoResume", ((OMCheckbox)manager[screen, "Settings"][7]).Checked.ToString());
            }
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "Media","Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideDown.ToString());
        }

        void select_OnClick(OMControl sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath OpenFolder= new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)manager[screen, "Settings"][3]).Text = OpenFolder.getFolder(screen, "Media", "Settings");
        }

        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(),"Media","Settings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideDown.ToString());
        }

        void bSettings_OnClick(OMControl sender, int screen)
        {
            using(PluginSettings settings=new PluginSettings())
            {
                ((OMTextBox)manager[screen, "Settings"][3]).Text = settings.getSetting("Music.Path");
                ((OMCheckbox)manager[screen, "Settings"][1]).Checked = (settings.getSetting("Music.AutoIndex") == "True");
                ((OMCheckbox)manager[screen, "Settings"][7]).Checked = (settings.getSetting("Music.AutoResume") == "True");
            }
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString());
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media","Settings");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideUp.ToString());
        }

        void List2_OnLongClick(OMControl sender, int screen)
        {
            OMList l = (OMList)sender;
            if (l.SelectedIndex < 0)
                return;
            loadSongs(currentAlbums[theHost.instanceForScreen(screen)][l.SelectedIndex].Artist, currentAlbums[theHost.instanceForScreen(screen)][l.SelectedIndex].Album, screen);
            theHost.setPlaylist(currentSongs[theHost.instanceForScreen(screen)],theHost.instanceForScreen(screen));
            theHost.execute(eFunction.nextMedia, theHost.instanceForScreen(screen).ToString());
        }

        void List3_OnLongClick(OMControl sender, int screen)
        {
            OMList l=(OMList)sender;
            if (l.SelectedIndex < 0)
                return;
            playSongs(l[l.SelectedIndex].text, screen);
            theHost.setPlaylist(currentSongs[theHost.instanceForScreen(screen)], theHost.instanceForScreen(screen));
        }
        private void loadSongs(string artist, int screen)
        {
            kickDown[screen] = true;
            lock (currentSongs[theHost.instanceForScreen(screen)])
            {
                object o;
                kickDown[screen] = false;
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetSongsByArtist(artist, true);
                currentSongs[theHost.instanceForScreen(screen)].Clear();
                mediaInfo info = db.getNextMedia();
                OMList l = ((OMList)manager[screen]["Media.List1"]);
                l.Clear();
                while ((info != null)&&(kickDown[screen]==false))
                {
                    if (l.AddDistinct(new OMListItem(info.Name,info.coverArt))==true)
                        currentSongs[theHost.instanceForScreen(screen)].Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }

        private void playSongs(string artist, int screen)
        {
            object o;
            lock (currentSongs[theHost.instanceForScreen(screen)])
            {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetSongsByArtist(artist, false);
                currentSongs[theHost.instanceForScreen(screen)].Clear();
                mediaInfo info = db.getNextMedia();
                if (info != null)
                {
                    theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), info.Location);
                }
                while (info != null)
                {
                    currentSongs[theHost.instanceForScreen(screen)].Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }

        void List1_OnClick(OMControl sender, int screen)
        {
            if (currentSongs[theHost.instanceForScreen(screen)].Count == 0)
                return;
            if (((OMList)sender).SelectedIndex == -1)
                return;
            theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), currentSongs[theHost.instanceForScreen(screen)][((OMList)sender).SelectedIndex].Location);
        }

        void List2_OnClick(OMControl sender, int screen)
        {
            slider2left(screen);
        }

        void List3_OnClick(OMControl sender, int screen)
        {
            slider1left(screen);
        }

        void List2_SelectedIndexChanged(OMList sender, int screen)
        {
            if ((sender.SelectedIndex < 0) || (sender.SelectedIndex >= currentAlbums[theHost.instanceForScreen(screen)].Count))
                return;
            loadSongs(currentAlbums[theHost.instanceForScreen(screen)][sender.SelectedIndex].Artist, currentAlbums[theHost.instanceForScreen(screen)][sender.SelectedIndex].Album, screen);
        }

        private void loadSongs(string artist,string album, int screen)
        {
            kickDown[screen] = true;
            lock (currentSongs[theHost.instanceForScreen(screen)])
            {
                object o;
                kickDown[screen] = false;
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                if (o == null)
                    return;
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetSongsByAlbum(artist, album, true);
                OMList l = ((OMList)manager[screen]["Media.List1"]);
                l.Clear();
                currentSongs[theHost.instanceForScreen(screen)].Clear();
                mediaInfo info = db.getNextMedia();
                while ((info != null)&&(kickDown[screen]==false))
                {
                    if (l.AddDistinct(new OMListItem(info.Name, info.coverArt)) == true)
                        currentSongs[theHost.instanceForScreen(screen)].Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }

        void Slider3_OnClick(OMControl sender, int screen)
        {
            if (slider2left(screen) == false) //performance hack - only the correct one will execute
                slider2right(screen);
        }

        void List3_SelectedIndexChanged(OMList sender,int screen)
        {
            int index = sender.SelectedIndex; //store it first because it can actually change between the -1 check and the load function
            if (index<0)
                return;
            loadAlbums(sender[index].text,screen);
            loadSongs(sender[index].text, screen);
        }
        private List<mediaInfo>[] currentSongs;
        private bool[] kickDown; //Kicks a thread out of processing songs if another is waiting
        private List<mediaInfo>[] currentAlbums;
        private void loadAlbums(string artist,int screen)
        {
            lock (currentAlbums[theHost.instanceForScreen(screen)])
                {
                object o;
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                if (o == null)
                    return;
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetAlbums(artist, true);
                OMList l = ((OMList)manager[screen]["Media.List2"]);
                l.Clear();
                currentAlbums[theHost.instanceForScreen(screen)].Clear();
                currentSongs[theHost.instanceForScreen(screen)].Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (l.AddDistinct(new OMListItem(info.Album, info.coverArt)) == true)
                        currentAlbums[theHost.instanceForScreen(screen)].Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }


        #region Movement
        private bool slider1left(int screen)
        {
            OMPanel p = manager[screen];
            OMControl b = p["Media.Slider2"];
            if (b.Left == 858)
            {
                OMControl l = p["Media.List3"];
                OMControl l2 = p["Media.List2"];

                for (int i = 858; i > 70; i -= 87)
                {
                    b.Left = i;
                    l.Width = i - 77;
                    l2.Width = 775 - l.Width;
                    l2.Left = i + 60;
                    Thread.Sleep(25);
                }
                return true;
            }
            return false;
        }
        private bool slider1right(int screen)
        {
            OMPanel p = manager[screen];
            OMControl b2 = p["Media.Slider2"];
            OMControl b3 = p["Media.Slider3"];
            bool both = (b3.Left == 137);
            if (b2.Left != 858)
            {
                OMControl l3 = p["Media.List3"];
                OMControl l2 = p["Media.List2"];
                OMControl l1 = p["Media.List1"];
                for (int i = b2.Left; i < 859; i += 87)
                {
                    b2.Left = i;
                    if (both == true)
                    {
                        l1.Width = 858-i;
                        l1.Left = 920 - i;
                        b3.Left = i + 62;
                        l3.Width = 775 - l1.Width;
                    }
                    else
                    {
                        l3.Width = i - 77;
                        l2.Width = 775 - l3.Width;
                    }
                    l2.Left = i + 60;
                    Thread.Sleep(25);
                }
                return true;
            }
            return false;
        }
        private bool slider2left(int screen)
        {
            OMPanel p = manager[screen];
            OMControl b = p["Media.Slider3"];
            if (b.Left == 920)
            {
                OMControl l1 = p["Media.List3"];
                OMControl b2 = p["Media.Slider2"];
                bool both = (b2.Left == 858);
                OMControl l = p["Media.List2"];
                OMControl l2 = p["Media.List1"];
                for (int i = 920; i > 70; i -= 87)
                {
                    b.Left = i;
                    lock (l)
                    {
                        l.Width = i - 137;
                    }
                    l2.Width = 775 - l.Width;
                    l2.Left = i + 60;
                    if (both)
                    {
                        b2.Left = i - 62;
                        lock (l1)
                        {
                            l1.Width = i - 137;
                        }
                        lock (l)
                        {
                            l.Left = i - 2;
                        }
                        //OMList c=(OMList)l1;
                        //loadSongs(c[c.SelectedIndex].text, screen);
                    }
                    Thread.Sleep(25);
                }
                return true;
            }
            return false;
        }
        private bool slider2right(int screen)
        {
            OMPanel p = manager[screen];
            OMControl b3 = p["Media.Slider3"];
            if (b3.Left != 920)
            {
                OMControl l2 = p["Media.List2"];
                OMControl l = p["Media.List1"];
                for (int i = b3.Left; i < 921; i += 87)
                {
                    b3.Left = i;
                    l2.Width = i - 77;
                    l.Width = 775 - l2.Width;
                    l.Left = i + 60;
                    Thread.Sleep(25);
                }
                return true;
            }
            return false;
        } 
        #endregion

        void Slider2_OnClick(OMControl sender, int screen)
        {
                if (slider1left(screen)==false) //performance hack - only the correct one will execute
                    slider1right(screen);
        }

        public OMPanel loadPanel(string name,int screen)
        {
            if (manager == null)
                return null;
            return manager[screen,name];
        }

        private void loadList()
        {
            object o;
            lock (this)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                if (o == null)
                    return;
                using(IMediaDatabase db = (IMediaDatabase)o)
                {
                    db.beginGetArtists(false);
                    mediaInfo info = db.getNextMedia();
                    OMList[] list = new OMList[theHost.ScreenCount];
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        list[i] = ((OMList)manager[i][3]);
                        list[i].Clear();
                    }
                    while (info != null)
                    {
                        for(int i=0;i<theHost.ScreenCount;i++)
                            list[i].Add(info.Artist);
                        info = db.getNextMedia();
                    }
                    db.endSearch();
                }
            }
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
            get { return "Media"; }
        }
        public string displayName
        {
            get { return "Music"; }
        }
        public float pluginVersion
        {
            get { return 1.0F; }
        }

        public string pluginDescription
        {
            get { return "Music Player Skin"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
