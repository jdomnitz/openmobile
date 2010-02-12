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
using System.Drawing;
using System.Threading;
using System.Timers;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;

namespace OpenMobile
{
    public sealed class UIPlugin:IHighLevel
    {
        private IPluginHost theHost;
        private System.Timers.Timer tick = new System.Timers.Timer();
        private imageItem blank = new imageItem();
        private System.Timers.Timer statusReset = new System.Timers.Timer(2100);
        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "admin@domnitzsolutions.com"; }
        }

        public string pluginName
        {
            get { return "UI"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "The User Interface"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message,string source, ref T data)
        {
            throw new NotImplementedException();
        }
        public string displayName
        {
            get { return "User Interface"; }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            theHost.OnMediaEvent -= theHost_OnMediaEvent;
            theHost.OnSystemEvent -= theHost_OnSystemEvent;
            tick.Dispose();
            manager.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IHighLevel Members

        public OMPanel loadPanel(string name,int screen)
        {
            return manager[screen];
        }

        public OMPanel loadSettings(string name,int screen)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBasePlugin Members

        ScreenManager manager;
        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel();
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            theHost.OnStorageEvent += new StorageEvent(theHost_OnStorageEvent);
            blank.name = "EMPTY";
            tick.BeginInit();
            tick.EndInit();
            tick.Elapsed += new ElapsedEventHandler(tick_Elapsed);
            tick.Interval = 500;
            tick.Enabled = true;
            statusReset.BeginInit();
            statusReset.EndInit();
            statusReset.Elapsed += new ElapsedEventHandler(statusReset_Elapsed);
            OMLabel trackTitle = new OMLabel(240,3,390,28);
            trackTitle.TextAlignment = Alignment.CenterLeft;
            trackTitle.Format = textFormat.BoldShadow;
            OMLabel trackAlbum = new OMLabel(240,34,390,28);
            trackAlbum.TextAlignment = Alignment.CenterLeft;
            trackAlbum.Format = textFormat.BoldShadow;
            OMLabel trackArtist = new OMLabel(240,64,390,28);
            trackArtist.TextAlignment = Alignment.CenterLeft;
            trackArtist.Format = textFormat.DropShadow;
            OMImage cover = new OMImage(150,2,90,85);
            cover.Image = blank;
            OMButton mediaButton = new OMButton(9,533,160,70);
            mediaButton.Image = theHost.getSkinImage("MediaButton");
            mediaButton.Transition = eButtonTransition.None;
            mediaButton.FocusImage = theHost.getSkinImage("MediaButtonFocus");
            mediaButton.Name = "mediaButton";
            mediaButton.OnClick += new userInteraction(mediaButton_OnClick);
            OMButton Back = new OMButton(831,533,160,70);
            Back.Image = theHost.getSkinImage("BackButton");
            Back.FocusImage = theHost.getSkinImage("BackButtonFocus");
            Back.Name = "Back";
            Back.OnClick += new userInteraction(Back_OnClick);
            OMButton HomeButton = new OMButton(863,0,130,90);
            HomeButton.Image = theHost.getSkinImage("HomeButton");
            HomeButton.FocusImage = theHost.getSkinImage("HomeButtonFocus");
            HomeButton.Name = "UI.HomeButton";
            HomeButton.OnClick += new userInteraction(HomeButton_OnClick);
            OMButton vol = new OMButton(6,0,130,90);
            vol.Image = theHost.getSkinImage("VolumeButton");
            vol.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
            vol.Name = "UI.vol";
            vol.Mode = modeType.Resizing;
            vol.Transition = eButtonTransition.None;
            OMImage Image2 = new OMImage(0,0,1000,100);
            Image2.Name = "UI.TopBar";
            Image2.Image = theHost.getSkinImage("topBar");
            OMImage mediaBar = new OMImage(0,620,1000,140);
            mediaBar.Image = theHost.getSkinImage("MediaBar", true);
            mediaBar.Transparency = 80;
            OMSlider slider = new OMSlider(20,635,940,25,12,40);
            slider.Name = "UI.Slider";
            slider.Slider = theHost.getSkinImage("Slider");
            slider.SliderBar = theHost.getSkinImage("Slider.Bar");
            slider.OnSliderMoved += new OMSlider.slidermoved(slider_OnSliderMoved);
            OMButton playButton = new OMButton(287,648,135,100);
            playButton.Image = theHost.getSkinImage("Play");
            playButton.DownImage = theHost.getSkinImage("Play.Highlighted");
            playButton.OnClick += new userInteraction(playButton_OnClick);
            playButton.Transition = eButtonTransition.None;
            OMButton stopButton = new OMButton(425, 648,135, 100);
            stopButton.Image = theHost.getSkinImage("Stop", true);
            stopButton.DownImage = theHost.getSkinImage("Stop.Highlighted", true);
            stopButton.OnClick += new userInteraction(stopButton_OnClick);
            stopButton.Transition = eButtonTransition.None;
            OMButton rewindButton = new OMButton(149, 648, 135, 100);
            rewindButton.Image = theHost.getSkinImage("Rewind", true);
            rewindButton.DownImage = theHost.getSkinImage("Rewind.Highlighted", true);
            rewindButton.OnClick += new userInteraction(rewindButton_OnClick);
            rewindButton.Transition = eButtonTransition.None;
            OMButton fastForwardButton = new OMButton(564, 648, 135, 100);
            fastForwardButton.OnClick += new userInteraction(fastForwardButton_OnClick);
            fastForwardButton.Image = theHost.getSkinImage("fastForward", true);
            fastForwardButton.DownImage = theHost.getSkinImage("fastForward.Highlighted", true);
            fastForwardButton.Transition = eButtonTransition.None;
            OMButton skipForwardButton = new OMButton(703, 648, 135, 100);
            skipForwardButton.Image = theHost.getSkinImage("SkipForward", true);
            skipForwardButton.DownImage = theHost.getSkinImage("SkipForward.Highlighted", true);
            skipForwardButton.OnClick += new userInteraction(skipForwardButton_OnClick);
            skipForwardButton.Transition = eButtonTransition.None;
            OMButton skipBackwardButton = new OMButton(13, 648, 135, 100);
            skipBackwardButton.Image = theHost.getSkinImage("SkipBackward", true);
            skipBackwardButton.DownImage = theHost.getSkinImage("SkipBackward.Highlighted", true);
            skipBackwardButton.OnClick += new userInteraction(skipBackwardButton_OnClick);
            skipBackwardButton.Transition = eButtonTransition.None;
            OMLabel elapsed = new OMLabel(840,650,140,100);
            elapsed.Name = "UI.Elapsed";
            elapsed.OutlineColor = Color.Blue;
            elapsed.Format = textFormat.Glow;
            elapsed.Font = new Font(FontFamily.GenericSansSerif,26F);
            p.addControl(Back);
            p.addControl(mediaButton);
            p.addControl(mediaBar);
            p.addControl(Image2);
            p.addControl(vol);
            p.addControl(trackTitle);
            p.addControl(trackAlbum);
            p.addControl(trackArtist);
            p.addControl(cover);
            p.addControl(playButton);
            p.addControl(stopButton);
            p.addControl(rewindButton);
            p.addControl(skipBackwardButton);
            p.addControl(skipForwardButton);
            p.addControl(fastForwardButton);
            p.addControl(elapsed);
            p.addControl(slider);
            p.addControl(HomeButton);
            p.BackgroundType = backgroundStyle.Gradiant;
            p.BackgroundColor1 = Color.FromArgb(0, 0, 4);
            p.BackgroundColor2 = Color.FromArgb(0, 0, 20);
            theHost.RenderFirst = p.controlCount;
            manager.loadPanel(p);
            theHost.OnMediaEvent += theHost_OnMediaEvent;
            theHost.OnSystemEvent += theHost_OnSystemEvent;
            return eLoadStatus.LoadSuccessful;
        }

        void statusReset_Elapsed(object sender, ElapsedEventArgs e)
        {
            for(int i=0;i<theHost.ScreenCount;i++)
                ((OMLabel)manager[i][5]).Text="";
            statusReset.Enabled = false;
        }

        void skipBackwardButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.previousMedia,theHost.instanceForScreen(screen).ToString());
        }

        void skipForwardButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.nextMedia,theHost.instanceForScreen(screen).ToString());
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2,string arg3)
        {
            if (function == eFunction.backgroundOperationStatus)
            {
                statusReset.Enabled = false;
                statusReset.Enabled = true;
                for (int i = 0; i < theHost.ScreenCount; i++)
                    ((OMLabel)manager[i][5]).Text = arg1;
            }
            if (function == eFunction.systemVolumeChanged)
            {
                if (arg1 == "-1")
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        OMButton b = ((OMButton)manager[i][4]);
                        b.Image = theHost.getSkinImage("VolumeButtonMuted");
                        b.FocusImage = theHost.getSkinImage("VolumeButtonMutedFocus");
                    }
                }
                else
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        OMButton b = ((OMButton)manager[i][4]);
                        b.Image = theHost.getSkinImage("VolumeButton");
                        b.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
                    }
                }
            }
        }

        void HomeButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromAny,screen.ToString());
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu");
            theHost.execute(eFunction.ExecuteTransition,screen.ToString());
        }

        void slider_OnSliderMoved(OMSlider sender,int screen)
        {
            theHost.execute(eFunction.setPosition, theHost.instanceForScreen(screen).ToString(),sender.Value.ToString());
        }

        void theHost_OnStorageEvent(eMediaType type, string arg)
        {
            if (type != eMediaType.NotSet)
            {
                statusReset.Enabled = false;
                statusReset.Enabled = true;
                for (int i = 0; i < theHost.ScreenCount; i++)
                    ((OMLabel)manager[i][5]).Text = "Drive Inserted: " + arg;
            }
        }

        void rewindButton_OnClick(object sender, int screen)
        {
            object o;
            theHost.getData(eGetData.GetPlaybackSpeed, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null)
                return;
            float speed = (float)o;
            if (speed > 1)
            {
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), "1");
                ((OMButton)manager[screen][9]).Image = theHost.getSkinImage("Pause");
                ((OMButton)manager[screen][9]).DownImage = theHost.getSkinImage("Pause.Highlighted");
            }
            else
            {
                if (speed > 0)
                    speed = speed * -1;
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), (2 * speed).ToString());
            }
        }

        void tick_Elapsed(object sender, ElapsedEventArgs e)
        {
            object o;
            theHost.getData(eGetData.GetMediaPosition, "","0", out o); //To Do - instance should be dynamic
            if (o == null)
                return;
            int i = Convert.ToInt32(o);
            if (i == -1)
            {
                for (int j = 0; j < theHost.ScreenCount; j++)
                {
                    ((OMLabel)manager[j]["UI.Elapsed"]).Text = "";
                    ((OMSlider)manager[j]["UI.Slider"]).Value = 0;
                }
                return;
            }
            if ((i < ((OMSlider)manager[0]["UI.Slider"]).Maximum) && (i >= 0))
                for (int j = 0; j < theHost.ScreenCount; j++)
                {
                    ((OMSlider)manager[j]["UI.Slider"]).Value = i;
                    ((OMLabel)manager[j]["UI.Elapsed"]).Text = (formatTime(i) + " " + formatTime(((OMSlider)manager[j]["UI.Slider"]).Maximum));
                }
        }

        private string formatTime(int seconds)
        {
            return (seconds / 60).ToString() + ":" + (seconds % 60).ToString("00");
        }

        void fastForwardButton_OnClick(object sender, int screen)
        {
            object o;
            theHost.getData(eGetData.GetPlaybackSpeed, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o==null)
                return;
            float speed = (float)o;
            if (speed < 0)
            {
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(),"1");
                ((OMButton)manager[screen][9]).Image = theHost.getSkinImage("Pause");
                ((OMButton)manager[screen][9]).DownImage = theHost.getSkinImage("Pause.Highlighted");
            }
            else
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), (2 * speed).ToString());
        }

        void stopButton_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.Stop, theHost.instanceForScreen(screen).ToString());
        }

        int timerIteration = 0;
        bool timerForward=false;

        private void moveMediaBar(int screen)
        {
            while (true)
            {
                if (timerForward == false)
                {
                    if (timerIteration == 8)
                    {
                        timerIteration = 0;
                        return;
                    }
                    OMPanel p = manager[screen];
                    {
                            p[1].Top -= 17;
                            p[2].Top -= 20;
                            p[9].Top -= 20;
                            p[10].Top -= 20;
                            p[11].Top -= 20;
                            p[12].Top -= 20;
                            p[13].Top -= 20;
                            p[14].Top -= 20;
                            p[15].Top -= 20;
                            p[16].Top -= 20;
                    }
                }
                else
                {
                    if (timerIteration == 8)
                    {
                        timerIteration = 0;
                        return;
                    }
                    OMPanel p = manager[screen];
                        p[1].Top += 17;
                        p[2].Top += 20;
                        p[9].Top += 20;
                        p[10].Top += 20;
                        p[11].Top += 20;
                        p[12].Top += 20;
                        p[13].Top += 20;
                        p[14].Top += 20;
                        p[15].Top += 20;
                        p[16].Top += 20;
                }
                timerIteration++;
                Thread.Sleep(50);
            }
        }

        void playButton_OnClick(object sender, int screen)
        {
            object o=new object();
            theHost.getData(eGetData.GetMediaStatus, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null)
                return;
            if (o.GetType() == typeof(ePlayerStatus))
            {
                ePlayerStatus status=(ePlayerStatus)o;
                if (status == ePlayerStatus.Playing)
                {
                    theHost.execute(eFunction.Pause, screen.ToString());
                    ((OMButton)manager[screen][9]).Image = theHost.getSkinImage("Play");
                    ((OMButton)manager[screen][9]).DownImage = theHost.getSkinImage("Play.Highlighted");
                }
                else
                {
                    if ((status == ePlayerStatus.FastForwarding) || (status == ePlayerStatus.Rewinding))
                    {
                        theHost.execute(eFunction.setPlaybackSpeed, screen.ToString(), "1");
                        ((OMButton)manager[screen][9]).Image = theHost.getSkinImage("Pause");
                        ((OMButton)manager[screen][9]).DownImage = theHost.getSkinImage("Pause.Highlighted");
                    }
                    else
                    {
                        theHost.execute(eFunction.Play, screen.ToString());
                    }
                }
            }
        }

        void mediaButton_OnClick(object sender, int screen)
        {
            if (manager[screen][1].Top == 397)
            {
                timerForward = true;
                moveMediaBar(screen);
            }
            else
            {
                timerForward = false;
                moveMediaBar(screen);
            }
        }

        void Back_OnClick(object sender, int screen)
        {
            theHost.execute(eFunction.goBack,screen.ToString());
        }

        void theHost_OnMediaEvent(eFunction function, int instance,string arg)
        {
            if (function == eFunction.Play)
            {
                mediaInfo info = theHost.getPlayingMedia(instance);
                imageItem it = new imageItem(info.coverArt);
                //for(int i=0;i<theHost.ScreenCount;i++)
                {
                    OMPanel p = manager[instance]; //ToDo - GetScreenFromInstance
                    ((OMLabel)p[5]).Text = info.Name;
                    ((OMLabel)p[6]).Text = info.Artist;
                    ((OMLabel)p[7]).Text = info.Album;
                    ((OMImage)p[8]).Image = it;
                    ((OMButton)p[9]).Image = theHost.getSkinImage("Pause");
                    ((OMButton)p[9]).DownImage = theHost.getSkinImage("Pause.Highlighted");
                    ((OMSlider)p[16]).Maximum = info.Length;
                }
            }
            else if (function == eFunction.setPlaybackSpeed)
            {
                //ToDo - GetScreenFromInstance
                //for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    ((OMButton)manager[instance][9]).Image = theHost.getSkinImage("Play");
                    ((OMButton)manager[instance][9]).DownImage = theHost.getSkinImage("Play.Highlighted");
                }
            }
            else if (function == eFunction.Stop)
            {
                //ToDo - GetScreenFromInstance
                //for(int i=0;i<theHost.ScreenCount;i++)
                {
                    OMPanel p = manager[instance];
                    ((OMLabel)p[5]).Text = "";
                    ((OMLabel)p[6]).Text = "";
                    ((OMLabel)p[7]).Text = "";
                    ((OMImage)p[8]).Image = blank;
                    ((OMButton)p[9]).Image = theHost.getSkinImage("Play");
                    ((OMButton)p[9]).DownImage = theHost.getSkinImage("Play.Highlighted");
                    ((OMLabel)p["UI.Elapsed"]).Text = "";
                    ((OMSlider)p["UI.Slider"]).Value = 0;
                }
            }
        }

        #endregion
    }
}
