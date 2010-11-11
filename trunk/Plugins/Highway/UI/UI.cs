using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;
using System.Threading;
using UI;
using OpenMobile.Data;

namespace OpenMobile
{
    public sealed class UI:IHighLevel
    {
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (name == "")
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "UI", "background");
            return manager[screen,name];
        }

        public string displayName
        {
            get { return "UI"; }
        }
        IPluginHost theHost;
        ScreenManager manager;
        System.Timers.Timer tmr;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            volCount = new int[theHost.ScreenCount];
            tmr = new System.Timers.Timer(500);
            tmr.Enabled = true;
            tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            manager = new ScreenManager(host.ScreenCount);
            OMPanel background = new OMPanel("background");
            background.UIPanel = true;
            background.Priority = ePriority.Low;
            background.Forgotten = true;
            background.BackgroundType = backgroundStyle.Image;
            background.BackgroundImage = theHost.getSkinImage("Backgrounds|Highway 1", true);
            manager.loadSharedPanel(background);
            OMPanel p = new OMPanel("");
            OMPanel media = new OMPanel("media");
            p.Priority = ePriority.High;
            p.UIPanel = true;
            p.Forgotten = true;
            media.UIPanel = true;
            media.Forgotten = true;
            media.Priority = ePriority.High;
            OMImage topBar = new OMImage(0, 0, 1000, 68);
            topBar.Image = theHost.getSkinImage("TopBar");
            OMImage bottomBar = new OMImage(0, 498, 1000, 102);
            bottomBar.Image = theHost.getSkinImage("BottomBar");
            OMImage mediaAccent = new OMImage(310, 35, 454, 71);
            mediaAccent.Image = theHost.getSkinImage("MediaAccent");
            OMSlider track = new OMSlider(326, 86, 348, 28);
            track.SliderBarHeight = 14;
            track.SliderWidth = 28;
            track.SliderBar = theHost.getSkinImage("MediaSlider_Bottom");
            track.Slider = theHost.getSkinImage("MediaSlider");
            track.SliderTrackFull = theHost.getSkinImage("MediaSlider_Top");
            track.OnSliderMoved += new OMSlider.slidermoved(track_OnSliderMoved);
            OMButton random = new OMButton(786, 526, 112, 84);
            random.Image = theHost.getSkinImage("Shuffle");
            random.FocusImage = theHost.getSkinImage("Shuffle_HL");
            random.DownImage = theHost.getSkinImage("Shuffle_Selected");
            OMButton voldown = new OMButton(99, 526, 112, 84);
            voldown.Image = theHost.getSkinImage("VolDown");
            voldown.FocusImage = theHost.getSkinImage("VolDown_HL");
            voldown.DownImage = theHost.getSkinImage("VolDown_Selected");
            voldown.OnClick += new userInteraction(voldown_OnClick);
            voldown.Transition = eButtonTransition.None;
            OMButton volup = new OMButton(-11, 525, 112, 84);
            volup.Image = theHost.getSkinImage("VolUp");
            volup.FocusImage = theHost.getSkinImage("VolUp_HL");
            volup.DownImage = theHost.getSkinImage("VolUp_Selected");
            volup.OnClick += new userInteraction(volup_OnClick);
            volup.Transition = eButtonTransition.None;
            OMButton mute = new OMButton(180, 477, 178, 146);
            mute.Image = theHost.getSkinImage("Mute");
            mute.FocusImage = theHost.getSkinImage("Mute_HL");
            mute.DownImage = theHost.getSkinImage("Mute_Selected");
            mute.OnClick += new userInteraction(mute_OnClick);
            OMButton previous = new OMButton(291, 479, 178, 146);
            previous.Image = theHost.getSkinImage("RW");
            previous.FocusImage = theHost.getSkinImage("RW_HL");
            previous.DownImage = theHost.getSkinImage("RW_Selected");
            previous.OnClick += new userInteraction(previous_OnClick);
            OMButton next = new OMButton(529, 478, 178, 146);
            next.Image = theHost.getSkinImage("FF");
            next.FocusImage = theHost.getSkinImage("FF_HL");
            next.DownImage = theHost.getSkinImage("FF_Selected");
            next.OnClick += new userInteraction(next_OnClick);
            OMButton stop = new OMButton(639, 477,178,146);
            stop.Image = theHost.getSkinImage("Stop");
            stop.FocusImage = theHost.getSkinImage("Stop_HL");
            stop.DownImage = theHost.getSkinImage("Stop_Selected");
            stop.OnClick += new userInteraction(stop_OnClick);
            OMButton play = new OMButton(393, 485, 204, 152);
            play.Image = theHost.getSkinImage("Play");
            play.FocusImage = theHost.getSkinImage("Play_HL");
            play.DownImage = theHost.getSkinImage("Play_Selected");
            play.OnClick += new userInteraction(play_OnClick);
            OMButton speech = new OMButton(-9, -48, 272, 160);
            speech.Image = theHost.getSkinImage("Speech");
            speech.FocusImage = theHost.getSkinImage("Speech_HL");
            speech.DownImage = theHost.getSkinImage("Speech_Selected");
            speech.Font = new Font(Font.Verdana, 30F);
            speech.Text = " SPEAK";
            speech.TextAlignment = Alignment.CenterLeft;
            speech.Format = eTextFormat.Bold;
            speech.Transition = eButtonTransition.None;
            speech.OnClick += new userInteraction(speech_OnClick);
            OMButton back = new OMButton(745, -48, 272, 160);
            back.Image = theHost.getSkinImage("Back");
            back.FocusImage = theHost.getSkinImage("Back_HL");
            back.DownImage = theHost.getSkinImage("Back_Selected");
            back.Font = speech.Font;
            back.Format = eTextFormat.Bold;
            back.Text = "    BACK";
            back.Transition = eButtonTransition.None;
            back.OnClick += new userInteraction(back_OnClick);
            OMButton favorites = new OMButton(899, 526, 112, 84);
            favorites.Image = theHost.getSkinImage("Favorites");
            favorites.FocusImage = theHost.getSkinImage("Favorites_HL");
            favorites.OnLongClick += new userInteraction(favorites_OnLongClick);
            favorites.DownImage = theHost.getSkinImage("Favorites_Selected");
            p.addControl(topBar);
            p.addControl(bottomBar);
            p.addControl(random);
            p.addControl(voldown);
            p.addControl(volup);
            p.addControl(mute);//5
            p.addControl(next);
            p.addControl(stop);
            p.addControl(play);//8
            p.addControl(previous);
            p.addControl(speech);
            p.addControl(back);
            p.addControl(favorites);
            manager.loadPanel(p);
            OMAnimatedLabel title = new OMAnimatedLabel(335, 34, 425, 40);
            title.Font = new Font(Font.Arial, 24);
            title.Format = eTextFormat.BoldGlow;
            title.TextAlignment = Alignment.TopLeft;
            OMAnimatedLabel artist = new OMAnimatedLabel(335, 66, 275, 20);
            artist.Font = new Font(Font.Arial, 18);
            artist.Format = eTextFormat.Bold;
            artist.TextAlignment = Alignment.CenterLeft;
            OMLabel duration = new OMLabel(610, 65, 170, 20);
            duration.Font = new Font(Font.Arial, 18);
            duration.Format = eTextFormat.Bold;
            duration.TextAlignment = Alignment.CenterLeft;
            OMImage album = new OMImage(255, 38, 72, 72);
            album.Image = theHost.getSkinImage("AlbumArt");
            OMImage albumArt = new OMImage(257, 39, 64, 64);
            media.addControl(mediaAccent);
            media.addControl(track);
            media.addControl(title);//2
            media.addControl(artist);//3
            media.addControl(duration);
            media.addControl(album);
            media.addControl(albumArt);//6
            manager.loadPanel(media);
            OMPanel volume = new OMPanel("volume");
            volume.Priority = ePriority.High;
            volume.Forgotten = true;
            OMImage volBack = new OMImage(-8, 62, 257, 508);
            volBack.Image = theHost.getSkinImage("VolumeBG");
            volume.addControl(volBack);
            VolumeBar bar = new VolumeBar(31, 125, 152, 350);
            bar.Bottom = theHost.getSkinImage("VolumeSlider_Bottom");
            bar.Top = theHost.getSkinImage("VolumeSlider_Top");
            bar.OnSliderMoved += new userInteraction(bar_OnSliderMoved);
            volume.addControl(bar);
            manager.loadPanel(volume);
            return eLoadStatus.LoadSuccessful;
        }

        void favorites_OnLongClick(OMControl sender, int screen)
        {
            theHost.sendMessage("Screen" + screen, "Screen"+screen, "QueryFavorite");
        }

        void voldown_OnClick(OMControl sender, int screen)
        {
            int vol = ((VolumeBar)manager[screen, "volume"][1]).Value;
            if (vol == 0)
                return;
            
            theHost.execute(eFunction.setSystemVolume, (vol - 1).ToString(), theHost.instanceForScreen(screen).ToString());
            if (volCount[screen] == 0)
            {
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "UI", "volume");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideRight");
            }
            volCount[screen] = 4;
        }

        void bar_OnSliderMoved(OMControl sender, int screen)
        {
            theHost.execute(eFunction.setSystemVolume, ((VolumeBar)sender).Value.ToString());
        }

        void volup_OnClick(OMControl sender, int screen)
        {
            int vol=((VolumeBar)manager[screen, "volume"][1]).Value;
            if (vol==100)
                return;
            
            theHost.execute(eFunction.setSystemVolume, (vol + 1).ToString(), theHost.instanceForScreen(screen).ToString());
            if (volCount[screen] == 0)
            {
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "UI", "volume");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideRight");
            }
            volCount[screen] = 4;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.systemVolumeChanged)
            {
                if (arg1 == "-1")
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        if (arg2 != theHost.instanceForScreen(i).ToString())
                            continue;
                        OMButton b = ((OMButton)manager[i,""][5]);
                        b.Image = theHost.getSkinImage("MuteOn");
                        b.FocusImage = theHost.getSkinImage("MuteOn_HL");
                        b.DownImage = theHost.getSkinImage("MuteOn_Selected");
                    }
                }
                else
                {
                    for (int i = 0; i < theHost.ScreenCount; i++)
                    {
                        if (arg2 != theHost.instanceForScreen(i).ToString())
                            continue;
                        OMButton b = ((OMButton)manager[i, ""][5]);
                        b.Image = theHost.getSkinImage("Mute");
                        b.FocusImage = theHost.getSkinImage("Mute_HL");
                        b.DownImage = theHost.getSkinImage("Mute_Selected");
                        int instance;
                        int.TryParse(arg2,out instance);
                        ((VolumeBar)manager[i, "volume"][1]).Value = int.Parse(arg1);
                    }
                }
            }
        }

        void mute_OnClick(OMControl sender, int screen)
        {
            object o;
            theHost.getData(eGetData.GetSystemVolume, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null)
                return;
            if (((int)o) == -1)
                theHost.execute(eFunction.setSystemVolume, "-2", theHost.instanceForScreen(screen).ToString());
            else
                theHost.execute(eFunction.setSystemVolume, "-1", theHost.instanceForScreen(screen).ToString());
        }

        void next_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.stepForward, theHost.instanceForScreen(screen).ToString()) == false)
                theHost.execute(eFunction.nextMedia, theHost.instanceForScreen(screen).ToString());
        }

        void previous_OnClick(OMControl sender, int screen)
        {
            if (theHost.execute(eFunction.stepBackward, theHost.instanceForScreen(screen).ToString()) == false)
                theHost.execute(eFunction.previousMedia, theHost.instanceForScreen(screen).ToString());
        }

        void speech_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.listenForSpeech);
        }

        void back_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void track_OnSliderMoved(OMSlider sender, int screen)
        {
            theHost.execute(eFunction.setPosition, theHost.instanceForScreen(screen).ToString(), sender.Value.ToString());
        }

        void stop_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.Stop, theHost.instanceForScreen(screen).ToString());
            theHost.execute(eFunction.unloadTunedContent, theHost.instanceForScreen(screen).ToString());
        }
        int[] volCount;
        void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            object o;
            for (int j = 0; j < theHost.ScreenCount; j++)
            {
                if (volCount[j] > 0)
                {
                    volCount[j]--;
                    if (volCount[j] == 0)
                    {
                        theHost.execute(eFunction.TransitionFromPanel, j.ToString(), "UI", "volume");
                        theHost.execute(eFunction.ExecuteTransition, j.ToString(), "SlideLeft");
                    }
                }
                theHost.getData(eGetData.GetMediaPosition, "", theHost.instanceForScreen(j).ToString(), out o);
                if (o == null)
                    continue;
                int i = Convert.ToInt32(o);
                if (i == -1)
                {
                    ((OMLabel)manager[j, "media"][4]).Text = "";
                    ((OMSlider)manager[j, "media"][1]).Value = 0;
                }
                else if ((i < ((OMSlider)manager[j,"media"][1]).Maximum) && (i >= 0))
                {
                    if (((OMSlider)manager[j,"media"][1]).Mode == eModeType.Scrolling)
                        return;
                    ((OMSlider)manager[j,"media"][1]).Value = i;
                    ((OMLabel)manager[j, "media"][4]).Text = (formatTime(i) + " / " + formatTime(((OMSlider)manager[j, "media"][1]).Maximum));
                }
            }
        }
        private string formatTime(int seconds)
        {
            return (seconds / 60).ToString() + ":" + (seconds % 60).ToString("00");
        }
        void theHost_OnMediaEvent(eFunction function, int instance, string arg)
        {
            if (function == eFunction.loadTunedContent)
            {
                if (arg == "Pandora")
                {
                    //yada yada
                }
            }
            else if (function == eFunction.unloadTunedContent)
            {
                if (arg == "Pandora")
                {
                    //yada yada
                }
            }
            else if (function == eFunction.Play)
            {
                mediaInfo info = theHost.getPlayingMedia(instance);
                if (info == null)
                    return;
                imageItem it = new imageItem(info.coverArt);
                object o = new object();
                tunedContentInfo TunedContentInfo = null;
                theHost.getData(eGetData.GetTunedContentInfo, "", instance.ToString(), out o);
                if (o != null)
                    TunedContentInfo = (tunedContentInfo)o;
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        if (!string.IsNullOrEmpty(arg))
                        {
                            theHost.execute(eFunction.TransitionToPanel, i.ToString(), "UI", "media");
                            theHost.execute(eFunction.ExecuteTransition,i.ToString(),"SlideDown");
                        }
                        OMPanel p = manager[i,"media"];
                        OMPanel main = manager[i,""];
                        OMAnimatedLabel title = ((OMAnimatedLabel)p[2]);//title
                        OMAnimatedLabel artist = ((OMAnimatedLabel)p[3]);//artist
                        if ((info.Type == eMediaType.Radio) && (TunedContentInfo != null))
                        {
                            if (title.Text != TunedContentInfo.currentStation.stationName)
                                title.Transition(eAnimation.UnveilRight, TunedContentInfo.currentStation.stationName, 50);
                            if (artist.Text != info.Name)
                                artist.Transition(eAnimation.UnveilRight, info.Name, 50);
                            OMImage cover = ((OMImage)p[6]);
                            if (info.coverArt == null)
                                cover.Image = theHost.getSkinImage("Radio");
                            else
                                cover.Image = new imageItem(info.coverArt);
                        }
                        else
                        {
                            if (title.Text != info.Name)
                            {
                                ((OMSlider)p[1]).Maximum = info.Length;
                                theHost.getData(eGetData.GetMediaPosition, "", theHost.instanceForScreen(i).ToString(), out o);
                                if (o != null)
                                {
                                    int pos = Convert.ToInt32(o);
                                    ((OMSlider)p[1]).Value = pos;
                                    ((OMLabel)p[4]).Text = (formatTime(pos) + " / " + formatTime(info.Length));
                                }
                                title.Transition(eAnimation.UnveilRight, info.Name, 50);
                            }
                            if (artist.Text != info.Name)
                                artist.Transition(eAnimation.UnveilRight, info.Artist, 50);
                            OMImage cover = ((OMImage)p[6]);
                            cover.Image = it;
                        }
                        ((OMButton)main[8]).Image = theHost.getSkinImage("Pause");
                        ((OMButton)main[8]).DownImage = theHost.getSkinImage("Pause_Selected");
                        ((OMButton)main[8]).FocusImage = theHost.getSkinImage("Pause_HL");
                        ((OMSlider)p[1]).Maximum = info.Length;
                    }
                }
            }
            else if (function == eFunction.Stop)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        theHost.execute(eFunction.TransitionFromPanel, i.ToString(), "UI", "media");
                        theHost.execute(eFunction.ExecuteTransition, i.ToString(), "SlideUp");
                        OMPanel p = manager[i,"media"];
                        OMPanel main = manager[i,""];
                        ((OMLabel)p[2]).Text = "";
                        ((OMLabel)p[3]).Text = "";
                        ((OMImage)p[6]).Image = imageItem.NONE;
                        ((OMButton)main[8]).Image = theHost.getSkinImage("Play");
                        ((OMButton)main[8]).FocusImage = theHost.getSkinImage("Play_HL");
                        ((OMButton)main[8]).DownImage = theHost.getSkinImage("Pause_Selected");
                        ((OMLabel)p[4]).Text = "";
                        ((OMSlider)p[1]).Value = 0;
                    }
                }
            }
            else if (function == eFunction.Pause)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                {
                    if (theHost.instanceForScreen(i) == instance)
                    {
                        ((OMButton)manager[i,""][8]).Image = theHost.getSkinImage("Play");
                        ((OMButton)manager[i, ""][8]).FocusImage = theHost.getSkinImage("Play_HL");
                        ((OMButton)manager[i, ""][8]).DownImage = theHost.getSkinImage("Play_Selected");
                    }
                }
            }
            else if (function == eFunction.RandomChanged)
            {
                //Todo
            }
        }

        void play_OnClick(OMControl sender, int screen)
        {
            object o = new object();
            theHost.getData(eGetData.GetMediaStatus, "", theHost.instanceForScreen(screen).ToString(), out o);
            if (o == null)
                return;
            if (o.GetType() == typeof(ePlayerStatus))
            {
                ePlayerStatus status = (ePlayerStatus)o;
                if (status == ePlayerStatus.Playing)
                    theHost.execute(eFunction.Pause, theHost.instanceForScreen(screen).ToString());
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
                        theHost.execute(eFunction.Play, theHost.instanceForScreen(screen).ToString());
                }
            }
            else if (o.GetType() == typeof(stationInfo))
                theHost.execute(eFunction.Pause, theHost.instanceForScreen(screen).ToString());
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { throw new NotImplementedException(); }
        }

        public string authorEmail
        {
            get { throw new NotImplementedException(); }
        }

        public string pluginName
        {
            get { return "UI"; }
        }

        public float pluginVersion
        {
            get { throw new NotImplementedException(); }
        }

        public string pluginDescription
        {
            get {return "UI"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            if (message == "SetFavorite")
            {
                string fav = data as string;
                if (string.IsNullOrEmpty(fav))
                    return false;
                string[] parts = fav.Split(new char[] { ';' });
                if (parts.Length != 3)
                    return false;
                int screen;
                if ((source.Length < 7) || (!int.TryParse(source.Substring(6), out screen)))
                    return false;
                setFavorite(screen, parts[0], parts[1], parts[2]);
                return true;
            }
            return false;
        }
        public void setFavorite(int screen,string displayName,string icon, string favorite)
        {
            using (PluginSettings settings = new PluginSettings())
            {
                for (int i = 0; i < 7; i++)
                    if ((settings.getSetting("Favorites.Screen"+screen.ToString()+".Num" + i.ToString() + ".Value") == "")||(i==6))
                    {
                        settings.setSetting("Favorites.Screen" + screen.ToString() + ".Num" + i.ToString() + "Value", favorite);
                        settings.setSetting("Favorites.Screen" + screen.ToString() + ".Num" + i.ToString() + "Icon", icon);
                        settings.setSetting("Favorites.Screen" + screen.ToString() + ".Num" + i.ToString() + "Display", displayName);
                        break;
                    }
            }
        }
        public void Dispose()
        {
            //
        }
    }
}
