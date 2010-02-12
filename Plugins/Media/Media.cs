﻿using System;
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
        ScreenManager settingsManager;
        IPluginHost theHost;
        string dbname = "";

        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel();
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
            Slider1.OnClick += new userInteraction(Slider1_OnClick);
            OMLabel artists = new OMLabel(30, 220, 30, 300);
            artists.Text = "Artists";
            artists.Font = new Font(FontFamily.GenericSansSerif, 26F);
            artists.Format = textFormat.BoldShadow;
            artists.Name = "Media.Artists";
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
            List2.SelectedIndexChanged += new OMList.IndexChangedDelegate(List2_SelectedIndexChanged);
            List2.OnClick += new userInteraction(List2_OnClick);
            List2.OnLongClick += new userInteraction(List2_OnLongClick);
            OMList List1 = new OMList(137,120,0,395);
            List1.ListStyle = eListStyle.DroidStyleImage;
            List1.ItemColor1 = Color.FromArgb(0, 0, 66);
            List1.ItemColor2 = Color.FromArgb(0, 0, 10);
            List1.SelectedItemColor1 = Color.FromArgb(192, 192, 192);
            List1.SelectedItemColor2 = Color.FromArgb(38, 37, 37);
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
            OpenMobile.Threading.TaskManager.QueueTask(loadList,priority.High);//<-Where the music gets loaded
            p.DoubleClickable = false;
            manager.loadPanel(p);
            settingsManager=new ScreenManager(theHost.ScreenCount);
            OMPanel settings = new OMPanel();
            imageItem opt2=theHost.getSkinImage("Full.Highlighted");
		    imageItem opt1=theHost.getSkinImage("Full");
		    OMLabel Title=new OMLabel(270,110,500,80);
		    Title.Font= new Font("Poor Richard",48F);
		    Title.Text="Music Settings";
		    Title.Format=textFormat.DropShadow;
		    Title.Name="Media.Title";
            Title.TextAlignment = Alignment.CenterCenter;
		    OMCheckbox chkIndex=new OMCheckbox(225,195,800,70);
		    chkIndex.OutlineColor=Color.FromArgb(255,142,0,0);
		    chkIndex.Font= new Font("Microsoft Sans Serif",31F);
		    chkIndex.Text="Index new music on every startup";
		    chkIndex.Name="Media.chkIndex";
            OMCheckbox AutoStart = new OMCheckbox(225, 450, 800, 70);
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
		    Save.Mode=modeType.Resizing;
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
            OMButton scrollRight = new OMButton(882, 364, 110, 80);
            scrollRight.Image = opt3;
            scrollRight.DownImage = opt4;
            scrollRight.Name = "Media.scrollRight";
            scrollRight.OnClick += new userInteraction(scrollRight_OnClick);
            OMButton scrollLeft = new OMButton(353, 365, 110, 80);
            scrollLeft.Image = opt3;
            scrollLeft.DownImage = opt4;
            scrollLeft.Name = "Media.scrollLeft";
            scrollLeft.Orientation = Angle.FlipHorizontal;
            scrollLeft.OnClick += new userInteraction(scrollLeft_OnClick);
            OMTextBox dbSelection = new OMTextBox(471, 380, 400, 50);
            dbSelection.Flags = textboxFlags.EllipsisEnd;
            dbSelection.Text = "None";
            dbSelection.Font = new Font("Microsoft Sans Serif", 32.25F);
            dbSelection.Name = "Textbox3";
            OMLabel dbTitle = new OMLabel(109,356,240,100);
            dbTitle.Font = new Font("Microsoft Sans Serif", 32.25F, FontStyle.Bold);
            dbTitle.Text = "Database:";
            dbTitle.Name = "Media.dbTitle";
            settings.addControl(Title);
		    settings.addControl(chkIndex);
		    settings.addControl(pathCaption);
		    settings.addControl(path);
		    settings.addControl(select);
		    settings.addControl(Save);
		    settings.addControl(Cancel);
            settings.addControl(scrollRight);
            settings.addControl(scrollLeft);
            settings.addControl(dbTitle);
            settings.addControl(dbSelection);
            settings.addControl(AutoStart);
            settingsManager.loadPanel(settings);
            return eLoadStatus.LoadSuccessful;
        }
        private List<string> dbPlugins;

        void scrollRight_OnClick(object sender, int screen)
        {
            //Right
            if (dbPlugins == null)
                return;
            int i = dbPlugins.FindIndex(w => w == ((OMTextBox)settingsManager[screen][10]).Text);
            i++;
            if (i == dbPlugins.Count)
                return;
            ((OMTextBox)settingsManager[screen][10]).Text=dbPlugins[i];
        }

        void scrollLeft_OnClick(object sender, int screen)
        {   //Left
            if (dbPlugins == null)
                return;
            int i = dbPlugins.FindIndex(w => w == ((OMTextBox)settingsManager[screen][10]).Text);
            i--;
            if (i < 1)
                ((OMTextBox)settingsManager[screen][10]).Text = "None";
            else
                ((OMTextBox)settingsManager[screen][10]).Text = dbPlugins[i];
        }

        void Save_OnClick(object sender, int screen)
        {
            using (OpenMobile.Data.PluginSettings set = new OpenMobile.Data.PluginSettings())
            {
                set.setSetting("Music.Path", ((OMTextBox)settingsManager[screen][3]).Text);
                dbname=((OMTextBox)settingsManager[screen][10]).Text;
                if (dbname == "None")
                    dbname = "";
                set.setSetting("Default.MusicDatabase",dbname);
                if ((set.getSetting("Music.AutoIndex") != "True") && (((OMCheckbox)settingsManager[screen][1]).Checked == true))
                {
                    object o = new object();
                    theHost.getData(eGetData.GetMediaDatabase, dbname, out o);
                    if (o != null)
                    {
                        IMediaDatabase db = (IMediaDatabase)o;
                        db.indexDirectory(set.getSetting("Music.Path"), true);
                    }
                }
                set.setSetting("Music.AutoIndex", ((OMCheckbox)settingsManager[screen][1]).Checked.ToString());
                set.setSetting("Music.AutoResume", ((OMCheckbox)settingsManager[screen][11]).Checked.ToString());
            }
            theHost.execute(eFunction.TransitionFromSettings, screen.ToString(), "Media");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideDown.ToString());
        }

        void select_OnClick(object sender, int screen)
        {
            OpenMobile.helperFunctions.General.getFilePath OpenFolder= new OpenMobile.helperFunctions.General.getFilePath(theHost);
            ((OMTextBox)settingsManager[screen][3]).Text= OpenFolder.getFolder(screen,"Media",true);
        }

        void Cancel_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromSettings, screen.ToString(),"Media");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "Media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideDown.ToString());
        }

        void bSettings_OnClick(object sender, int screen)
        {
            using(PluginSettings settings=new PluginSettings())
            {
                ((OMTextBox)settingsManager[screen][3]).Text=settings.getSetting("Music.Path");
                ((OMCheckbox)settingsManager[screen][1]).Checked=(settings.getSetting("Music.AutoIndex")=="True");
                ((OMCheckbox)settingsManager[screen][11]).Checked = (settings.getSetting("Music.AutoResume") == "True");
                string txt=settings.getSetting("Default.MusicDatabase");
                if (txt=="")
                    txt="None";
                ((OMTextBox)settingsManager[screen][10]).Text = txt;
            }
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString());
            theHost.execute(eFunction.TransitionToSettings, screen.ToString(), "Media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideUp.ToString());
        }

        void List2_OnLongClick(object sender, int screen)
        {
            OMList l = (OMList)sender;
            if (l.SelectedIndex < 0)
                return;
            loadSongs(currentAlbums[l.SelectedIndex].Artist,currentAlbums[l.SelectedIndex].Album, screen);
            theHost.setPlaylist(currentSongs,theHost.instanceForScreen(screen));
            theHost.execute(eFunction.loadAVPlayer, screen.ToString(), "OMPlayer"); //More efficient then checking first
            theHost.execute(eFunction.nextMedia, theHost.instanceForScreen(screen).ToString());
        }

        void List3_OnLongClick(object sender, int screen)
        {
            OMList l=(OMList)sender;
            if (l.SelectedIndex < 0)
                return;
            loadSongs(l[l.SelectedIndex].text, screen);
            theHost.setPlaylist(currentSongs,theHost.instanceForScreen(screen));
        }

        private void loadSongs(string artist, int screen)
        {
            object o;
            lock (this)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetSongsByArtist(artist, true);
                currentSongs.Clear();
                mediaInfo info = db.getNextMedia();
                if (info != null)
                {
                    theHost.execute(eFunction.loadAVPlayer, screen.ToString(), "OMPlayer"); //More efficient then checking first
                    theHost.execute(eFunction.Play, screen.ToString(), info.Location);
                }
                while (info != null)
                {
                    currentSongs.Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }

        void List1_OnClick(object sender, int screen)
        {
            if (currentSongs.Count == 0)
                return;
            if (theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), currentSongs[((OMList)sender).SelectedIndex].Location) == false)
            {
                theHost.execute(eFunction.loadAVPlayer, screen.ToString(), "OMPlayer");
                theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString(), currentSongs[((OMList)sender).SelectedIndex].Location);
            }
        }

        void List2_OnClick(object sender, int screen)
        {
            slider2left(screen);
        }

        void List3_OnClick(object sender, int screen)
        {
            slider1left(screen);
        }

        void List2_SelectedIndexChanged(OMList sender, int screen)
        {
            if ((sender.SelectedIndex < 0)||(sender.SelectedIndex>=currentAlbums.Count))
                return;
            loadSongs(currentAlbums[sender.SelectedIndex].Artist,currentAlbums[sender.SelectedIndex].Album, screen);
        }

        private void loadSongs(string artist,string album, int screen)
        {
            object o;
            lock (this)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetSongsByAlbum(artist, album, true);
                OMList l = ((OMList)manager[screen]["Media.List1"]);
                l.Clear();
                currentSongs.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (l.AddDistinct(new OMListItem(info.Name, info.coverArt)) == true)
                        currentSongs.Add(info);
                    info = db.getNextMedia();
                }
                db.endSearch();
            }
        }

        void Slider3_OnClick(object sender, int screen)
        {
            if (slider2left(screen) == false) //performance hack - only the correct one will execute
                slider2right(screen);
        }

        void Slider1_OnClick(object sender, int screen)
        {
                slider1right(screen);
        }

        void List3_SelectedIndexChanged(OMList sender,int screen)
        {
            if (sender.SelectedIndex<0)
                return;
            loadAlbums(sender[sender.SelectedIndex].text,screen);
        }
        private List<mediaInfo> currentSongs = new List<mediaInfo>();
        private List<mediaInfo> currentAlbums = new List<mediaInfo>();
        private void loadAlbums(string artist,int screen)
        {
            object o;
            lock (this)
                {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                if (o == null)
                    return;
                IMediaDatabase db = (IMediaDatabase)o;
                db.beginGetAlbums(artist, true);
                OMList l = ((OMList)manager[screen]["Media.List2"]);
                l.Clear();
                currentAlbums.Clear();
                currentSongs.Clear();
                mediaInfo info = db.getNextMedia();
                while (info != null)
                {
                    if (l.AddDistinct(new OMListItem(info.Album, info.coverArt)) == true)
                        currentAlbums.Add(info);
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

        void Slider2_OnClick(object sender, int screen)
        {
                if (slider1left(screen)==false) //performance hack - only the correct one will execute
                    slider1right(screen);
        }

        public OMPanel loadPanel(string name,int screen)
        {
            if (manager == null)
                return null;
            return manager[screen];
        }

        private void loadList()
        {
            object o;
            lock (this)
            {
                theHost.getData(eGetData.GetMediaDatabase, dbname, out  o);
                if (o == null)
                    return;
                IMediaDatabase db = (IMediaDatabase)o;
                {
                    db.beginGetArtists(false);
                    mediaInfo info = db.getNextMedia();
                    OMList[] list = new OMList[theHost.ScreenCount];
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        list[i] = ((OMList)manager[i][3]);
                        list[i].Clear();
                    }
                    list[0].Add("TOP0");
                    list[1].Add("TOP1");
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

        public OMPanel loadSettings(string name,int screen)
        {
            if (dbPlugins == null)
                dbPlugins = General.getPluginsOfType(typeof(IMediaDatabase), theHost);
            return settingsManager[screen];
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
            get { return "Media"; }
        }
        public float pluginVersion
        {
            get { return 1.0F; }
        }

        public string pluginDescription
        {
            get { return "Plugin Description"; }
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
            manager.Dispose();
            settingsManager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
