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
            get { return 0.6F; }
        }

        public string pluginDescription
        {
            get { return "The User Interface"; }
        }

        public bool incomingMessage(string message, string source)
        {
            if (message.StartsWith("ShowMediaControls"))
            {
                int screen;
                if (int.TryParse(message.Substring(17),out screen)==true)
                    if (manager[screen][2].Top == 533)
                    {
                        timerForward = false;
                        moveMediaBar(screen);
                    }
                return true;
            }
            return false;
        }
        public bool incomingMessage<T>(string message,string source, ref T data)
        {
            if (message == "AddIcon")
            {
                IconManager.UIIcon icon = data as IconManager.UIIcon;
                icons.AddIcon(icon);
                return true;
            }
            if (message == "RemoveIcon")
            {
                IconManager.UIIcon icon = data as IconManager.UIIcon;
                icons.RemoveIcon(icon);
                return true;
            }
            return false;
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

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBasePlugin Members
        IconManager icons = new IconManager();
        ScreenManager manager;
        public eLoadStatus initialize(IPluginHost host)
        {
            OMPanel p = new OMPanel();
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            //blank.name = "EMPTY";
            tick.BeginInit();
            tick.EndInit();
            tick.Elapsed += new ElapsedEventHandler(tick_Elapsed);
            tick.Interval = 500;
            tick.Enabled = true;
            statusReset.BeginInit();
            statusReset.EndInit();
            statusReset.Elapsed += new ElapsedEventHandler(statusReset_Elapsed);
            OMLabel trackTitle = new OMLabel(240,3,390,28);
            trackTitle.TextAlignment = Alignment.CenterLeftEllipsis;
            trackTitle.Format = textFormat.BoldShadow;
            OMLabel trackAlbum = new OMLabel(240,34,390,28);
            trackAlbum.TextAlignment = Alignment.CenterLeftEllipsis;
            trackAlbum.Format = textFormat.BoldShadow;
            OMLabel trackArtist = new OMLabel(240,64,390,28);
            trackArtist.TextAlignment = Alignment.CenterLeftEllipsis;
            trackArtist.Format = textFormat.DropShadow;
            OMImage cover = new OMImage(150,2,90,85);
            cover.Image = blank;
            OMButton mediaButton = new OMButton(9,533,160,70);
            mediaButton.Image = theHost.getSkinImage("MediaButton", true);
            mediaButton.Transition = eButtonTransition.None;
            mediaButton.FocusImage = theHost.getSkinImage("MediaButtonFocus", true);
            mediaButton.Name = "UI.mediaButton";
            mediaButton.OnClick += new userInteraction(mediaButton_OnClick);
            OMButton Back = new OMButton(831,533,160,70);
            Back.Image = theHost.getSkinImage("BackButton", true);
            Back.FocusImage = theHost.getSkinImage("BackButtonFocus", true);
            Back.OnClick += new userInteraction(Back_OnClick);
            Back.Transition = eButtonTransition.None;
            OMButton speech = new OMButton(670, 533, 160, 70);
            speech.Image = theHost.getSkinImage("Speak",true);
            speech.FocusImage = theHost.getSkinImage("SpeakFocus", true);
            speech.Name = "UI.speech";
            speech.Visible = false;
            speech.OnClick += new userInteraction(speech_OnClick);
            OMButton HomeButton = new OMButton(863,0,130,90);
            HomeButton.Image = theHost.getSkinImage("HomeButton",true);
            HomeButton.FocusImage = theHost.getSkinImage("HomeButtonFocus", true);
            HomeButton.Name = "UI.HomeButton";
            HomeButton.OnClick += new userInteraction(HomeButton_OnClick);
            OMButton vol = new OMButton(6,0,130,90);
            vol.Image = theHost.getSkinImage("VolumeButton");
            vol.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
            vol.Name = "UI.vol";
            vol.Mode = modeType.Resizing;
            vol.Transition = eButtonTransition.None;
            OMImage Image2 = new OMImage(0,0,1000,99);
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
            //Speech
            OMImage imgSpeak = new OMImage(350, 200, 300, 300);
            imgSpeak.Name = "UI.imgSpeak";
            imgSpeak.Visible = false;
            OMLabel caption = new OMLabel(300, 150, 400, 50);
            caption.Font = new Font(FontFamily.GenericSerif, 48F);
            caption.Format = textFormat.BoldShadow;
            caption.Visible = false;
            caption.Name = "UI.caption";
            OMBasicShape shape = new OMBasicShape(0, 0, 1000, 600);
            shape.Shape = shapes.Rectangle;
            shape.FillColor = Color.FromArgb(130, Color.Black);
            shape.Visible = false;
            OMButton icon1 = new OMButton(778, 4, 80, 85);
            icon1.Name = "UI.Icon1";
            icon1.OnClick += new userInteraction(icon_OnClick);
            OMButton icon2 = new OMButton(727, 1, 50, 90);
            icon2.Name = "UI.Icon2";
            icon2.OnClick += new userInteraction(icon_OnClick);
            OMButton icon3 = new OMButton(676, 1, 50, 90);
            icon3.Name = "UI.Icon3";
            icon3.OnClick += new userInteraction(icon_OnClick);
            OMButton icon4 = new OMButton(625, 1, 50, 90);
            icon4.Name = "UI.Icon4";
            icon4.OnClick += new userInteraction(icon_OnClick);
            //***
            p.addControl(Back);
            p.addControl(speech);
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
            p.addControl(shape);
            p.addControl(caption);
            p.addControl(imgSpeak);
            p.addControl(icon1);
            p.addControl(icon2);
            p.addControl(icon3);
            p.addControl(icon4);
            icons.OnIconsChanged += new IconManager.IconsChanged(icons_OnIconsChanged);

            p.BackgroundType = backgroundStyle.Gradiant;
            p.BackgroundColor1 = Color.FromArgb(0, 0, 4);
            p.BackgroundColor2 = Color.FromArgb(0, 0, 20);
            theHost.RenderFirst = p.controlCount;
            manager.loadPanel(p);
            theHost.OnMediaEvent += theHost_OnMediaEvent;
            theHost.OnSystemEvent += theHost_OnSystemEvent;
            theHost.VideoPosition = new Rectangle(0, 100, 1000, 368);
            return eLoadStatus.LoadSuccessful;
        }

        void icon_OnClick(OMControl sender, int screen)
        {
            IconManager.UIIcon icon;
            switch(sender.Name)
            {
                case "UI.Icon1":
                    icon=icons.getIcon(1,true);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI", "IconClicked", ref icon);
                    break;
                case "UI.Icon2":
                    icon = icons.getIcon(1, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI", "IconClicked", ref icon);
                    break;
                case "UI.Icon3":
                    icon = icons.getIcon(2, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI", "IconClicked", ref icon);
                    break;
                case "UI.Icon4":
                    icon = icons.getIcon(3, false);
                    if (icon.plugin == null)
                        return;
                    theHost.sendMessage(icon.plugin, "UI", "IconClicked", ref icon);
                    break;
            }
        }

        void icons_OnIconsChanged()
        {
            ((OMButton)manager[0][22]).Image = new imageItem(icons.getIcon(1, true).image);
            ((OMButton)manager[0][23]).Image = new imageItem(icons.getIcon(1, false).image);
            ((OMButton)manager[0][24]).Image = new imageItem(icons.getIcon(2, false).image);
            ((OMButton)manager[0][25]).Image = new imageItem(icons.getIcon(3, false).image);
        }

        void speech_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.listenForSpeech);
            showSpeech(screen);
        }
        private void showSpeech(int screen)
        {
            ((OMImage)manager[screen][21]).Image = theHost.getSkinImage("Speech");
            ((OMLabel)manager[screen][20]).Text = "Speak Now";
            manager[screen][21].Visible = true;
            manager[screen][20].Visible = true;
            manager[screen][19].Visible = true;
        }
        private void hideSpeech(int screen)
        {
            manager[screen][21].Visible = false;
            manager[screen][20].Visible = false;
            manager[screen][19].Visible = false;
            theHost.sendMessage("RenderingWindow", "UI", "Redraw");
        }
        void statusReset_Elapsed(object sender, ElapsedEventArgs e)
        {
            for(int i=0;i<theHost.ScreenCount;i++)
                ((OMLabel)manager[i][6]).Text=theHost.getPlayingMedia(theHost.instanceForScreen(i)).Name;
            statusReset.Enabled = false;
        }

        void skipBackwardButton_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.previousMedia,theHost.instanceForScreen(screen).ToString());
        }

        void skipForwardButton_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.nextMedia,theHost.instanceForScreen(screen).ToString());
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2,string arg3)
        {
            if (function == eFunction.backgroundOperationStatus)
            {
                if (arg2 == "Speech")
                    if (arg1 == "Engine Ready!")
                    {
                        for (int i = 0; i < theHost.ScreenCount; i++)
                            manager[i][1].Visible = true;
                        return;
                    }
                    else if (arg1 == "Processing...")
                    {
                        ((OMImage)manager[0][21]).Image = theHost.getSkinImage("Processing");
                        ((OMLabel)manager[0][20]).Text = "Processing...";
                        return;
                    }
                statusReset.Enabled = false;
                statusReset.Enabled = true;
                for (int i = 0; i < theHost.ScreenCount; i++)
                    ((OMLabel)manager[i][6]).Text = arg1;
            }
            if (function == eFunction.systemVolumeChanged)
            {
                if (arg1 == "-1")
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        if (arg2 != theHost.instanceForScreen(i).ToString())
                            continue;
                        OMButton b = ((OMButton)manager[i][5]);
                        b.Image = theHost.getSkinImage("VolumeButtonMuted");
                        b.FocusImage = theHost.getSkinImage("VolumeButtonMutedFocus");
                    }
                }
                else
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        if (arg2 != theHost.instanceForScreen(i).ToString())
                            continue;
                        OMButton b = ((OMButton)manager[i][5]);
                        b.Image = theHost.getSkinImage("VolumeButton");
                        b.FocusImage = theHost.getSkinImage("VolumeButtonFocus");
                    }
                }
            }
            if (function == eFunction.stopListeningForSpeech)
                hideSpeech(0); //ToDo - Instance specific
            if (function == eFunction.gesture)
            {
                if ((arg3!="OSK")&&(arg3!="Navigation"))
                    switch (arg2)
                    {
                        case "M":
                            theHost.execute(eFunction.TransitionFromAny, arg1);
                            theHost.execute(eFunction.TransitionToPanel, arg1, "Media");
                            theHost.execute(eFunction.ExecuteTransition, arg1);
                            break;
                        case "H":
                            theHost.execute(eFunction.TransitionFromAny, arg1);
                            theHost.execute(eFunction.TransitionToPanel, arg1, "MainMenu");
                            theHost.execute(eFunction.ExecuteTransition, arg1);
                            break;
                        case "N":
                            theHost.execute(eFunction.TransitionFromAny, arg1);
                            theHost.execute(eFunction.TransitionToPanel, arg1, "Navigation");
                            theHost.execute(eFunction.ExecuteTransition, arg1);
                            break;
                        case " ":
                            theHost.execute(eFunction.nextMedia,theHost.instanceForScreen(int.Parse(arg1)).ToString());
                            break;
                        case "I":
                            playButton_OnClick(null, int.Parse(arg1));
                            break;
                        case "back":
                            theHost.execute(eFunction.previousMedia, theHost.instanceForScreen(int.Parse(arg1)).ToString());
                            break;
                    }
            }
        }

        void HomeButton_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionFromAny,screen.ToString());
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"MainMenu");
            theHost.execute(eFunction.ExecuteTransition,screen.ToString());
        }

        void slider_OnSliderMoved(OMSlider sender,int screen)
        {
            theHost.execute(eFunction.setPosition, theHost.instanceForScreen(screen).ToString(),sender.Value.ToString());
        }

        void rewindButton_OnClick(OMControl sender, int screen)
        {
            object o;
            theHost.getData(eGetData.GetPlaybackSpeed, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null)
                return;
            float speed = (float)o;
            if (speed > 1)
            {
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), "1");
                ((OMButton)manager[screen][10]).Image = theHost.getSkinImage("Pause");
                ((OMButton)manager[screen][10]).DownImage = theHost.getSkinImage("Pause.Highlighted");
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
            for (int j = 0; j < theHost.ScreenCount; j++)
            {
                theHost.getData(eGetData.GetMediaPosition, "", theHost.instanceForScreen(j).ToString(), out o);
                if (o == null)
                    continue;
                int i = Convert.ToInt32(o);
                if (i == -1)
                {
                    ((OMLabel)manager[j]["UI.Elapsed"]).Text = "";
                    ((OMSlider)manager[j]["UI.Slider"]).Value = 0;
                }else if ((i < ((OMSlider)manager[0]["UI.Slider"]).Maximum) && (i >= 0))
                {
                    if (((OMSlider)manager[j]["UI.Slider"]).Mode == modeType.Scrolling)
                        return;
                    ((OMSlider)manager[j]["UI.Slider"]).Value = i;
                    ((OMLabel)manager[j]["UI.Elapsed"]).Text = (formatTime(i) + " " + formatTime(((OMSlider)manager[j]["UI.Slider"]).Maximum));
                }
            }
        }

        private string formatTime(int seconds)
        {
            return (seconds / 60).ToString() + ":" + (seconds % 60).ToString("00");
        }

        void fastForwardButton_OnClick(OMControl sender, int screen)
        {
            object o;
            theHost.getData(eGetData.GetPlaybackSpeed, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o==null)
                return;
            float speed = (float)o;
            if (speed < 0)
            {
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(),"1");
                ((OMButton)manager[screen][10]).Image = theHost.getSkinImage("Pause");
                ((OMButton)manager[screen][10]).DownImage = theHost.getSkinImage("Pause.Highlighted");
            }
            else
                theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), (2 * speed).ToString());
        }

        void stopButton_OnClick(OMControl sender, int screen)
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
                            p[2].Top -= 17;
                            p[3].Top -= 20;
                            p[10].Top -= 20;
                            p[11].Top -= 20;
                            p[12].Top -= 20;
                            p[13].Top -= 20;
                            p[14].Top -= 20;
                            p[15].Top -= 20;
                            p[16].Top -= 20;
                            p[17].Top -= 20;
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
                        p[2].Top += 17;
                        p[3].Top += 20;
                        p[10].Top += 20;
                        p[11].Top += 20;
                        p[12].Top += 20;
                        p[13].Top += 20;
                        p[14].Top += 20;
                        p[15].Top += 20;
                        p[16].Top += 20;
                        p[17].Top += 20;
                }
                timerIteration++;
                if (theHost.GraphicsLevel==eGraphicsLevel.Standard)
                    Thread.Sleep(50);
            }
        }

        void playButton_OnClick(OMControl sender, int screen)
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
                    theHost.execute(eFunction.Pause, theHost.instanceForScreen(screen).ToString());
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        if (theHost.instanceForScreen(i) == theHost.instanceForScreen(screen))
                        {
                            ((OMButton)manager[i][10]).Image = theHost.getSkinImage("Play");
                            ((OMButton)manager[i][10]).DownImage = theHost.getSkinImage("Play.Highlighted");
                        }
                    }
                }
                else
                {
                    if ((status == ePlayerStatus.FastForwarding) || (status == ePlayerStatus.Rewinding))
                    {
                        theHost.execute(eFunction.setPlaybackSpeed, theHost.instanceForScreen(screen).ToString(), "1");
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            if (theHost.instanceForScreen(i) == theHost.instanceForScreen(screen))
                            {
                                ((OMButton)manager[i][10]).Image = theHost.getSkinImage("Pause");
                                ((OMButton)manager[i][10]).DownImage = theHost.getSkinImage("Pause.Highlighted");
                            }
                        }
                    }
                    else
                    {
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString());
                    }
                }
            }
        }

        void mediaButton_OnClick(OMControl sender, int screen)
        {
            if (manager[screen][2].Top == 397)
            {
                timerForward = true;
                moveMediaBar(screen);
            }
            else if (manager[screen][2].Top == 533)
            {
                timerForward = false;
                moveMediaBar(screen);
            }
        }

        void Back_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack,screen.ToString());
        }

        void theHost_OnMediaEvent(eFunction function, int instance,string arg)
        {
            if (function == eFunction.Play)
            {
                mediaInfo info = theHost.getPlayingMedia(instance);
                imageItem it = new imageItem(info.coverArt);
                for(int i=0;i<theHost.ScreenCount;i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        OMPanel p = manager[i];
                        ((OMLabel)p[6]).Text = info.Name;
                        ((OMLabel)p[7]).Text = info.Artist;
                        ((OMLabel)p[8]).Text = info.Album;
                        ((OMImage)p[9]).Image = it;
                        ((OMButton)p[10]).Image = theHost.getSkinImage("Pause");
                        ((OMButton)p[10]).DownImage = theHost.getSkinImage("Pause.Highlighted");
                        ((OMSlider)p[17]).Maximum = info.Length;
                    }
                }
            }
            else if (function == eFunction.setPlaybackSpeed)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        ((OMButton)manager[i][10]).Image = theHost.getSkinImage("Play");
                        ((OMButton)manager[i][10]).DownImage = theHost.getSkinImage("Play.Highlighted");
                    }
                }
            }
            else if (function == eFunction.Stop)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        OMPanel p = manager[i];
                        ((OMLabel)p[6]).Text = "";
                        ((OMLabel)p[7]).Text = "";
                        ((OMLabel)p[8]).Text = "";
                        ((OMImage)p[9]).Image = blank;
                        ((OMButton)p[10]).Image = theHost.getSkinImage("Play");
                        ((OMButton)p[10]).DownImage = theHost.getSkinImage("Play.Highlighted");
                        ((OMLabel)p["UI.Elapsed"]).Text = "";
                        ((OMSlider)p["UI.Slider"]).Value = 0;
                    }
                }
            }
        }

        #endregion
    }
}
