using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using OpenMobile;
using OpenMobile.Graphics;

namespace UI
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
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(host.ScreenCount);
            OMPanel background = new OMPanel("background");
            background.UIPanel = true;
            background.Priority = ePriority.Low;
            background.BackgroundType = backgroundStyle.Image;
            background.BackgroundImage = theHost.getSkinImage("Backgrounds|Highway 1", true);
            manager.loadSharedPanel(background);
            OMPanel p = new OMPanel("");
            OMPanel media = new OMPanel("media");
            p.Priority = ePriority.High;
            p.UIPanel = true;
            media.UIPanel = true;
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
            OMButton random = new OMButton(786, 526, 112, 84);
            random.Image = theHost.getSkinImage("Shuffle");
            random.FocusImage = theHost.getSkinImage("Shuffle_HL");
            random.DownImage = theHost.getSkinImage("Shuffle_Selected");
            OMButton voldown = new OMButton(99, 526, 112, 84);
            voldown.Image = theHost.getSkinImage("VolDown");
            voldown.FocusImage = theHost.getSkinImage("VolDown_HL");
            voldown.DownImage = theHost.getSkinImage("VolDown_Selected");
            OMButton volup = new OMButton(-11, 525, 112, 84);
            volup.Image = theHost.getSkinImage("VolUp");
            volup.FocusImage = theHost.getSkinImage("VolUp_HL");
            volup.DownImage = theHost.getSkinImage("VolUp_Selected");
            OMButton mute = new OMButton(180, 477, 178, 146);
            mute.Image = theHost.getSkinImage("Mute");
            mute.FocusImage = theHost.getSkinImage("Mute_HL");
            mute.DownImage = theHost.getSkinImage("Mute_Selected");
            OMButton rw = new OMButton(291, 479, 178, 146);
            rw.Image = theHost.getSkinImage("RW");
            rw.FocusImage = theHost.getSkinImage("RW_HL");
            rw.DownImage = theHost.getSkinImage("RW_Selected");
            OMButton ff = new OMButton(529, 478, 178, 146);
            ff.Image = theHost.getSkinImage("FF");
            ff.FocusImage = theHost.getSkinImage("FF_HL");
            ff.DownImage = theHost.getSkinImage("FF_Selected");
            OMButton stop = new OMButton(639, 477,178,146);
            stop.Image = theHost.getSkinImage("Stop");
            stop.FocusImage = theHost.getSkinImage("Stop_HL");
            stop.DownImage = theHost.getSkinImage("Stop_Selected");
            OMButton play = new OMButton(393, 465, 204, 172);
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
            OMButton back = new OMButton(745, -48, 272, 160);
            back.Image = theHost.getSkinImage("Back");
            back.FocusImage = theHost.getSkinImage("Back_HL");
            back.DownImage = theHost.getSkinImage("Back_Selected");
            back.Font = speech.Font;
            back.Format = eTextFormat.Bold;
            back.Text = "    BACK";
            back.Transition = eButtonTransition.None;
            OMButton favorites = new OMButton(899, 526, 112, 84);
            favorites.Image = theHost.getSkinImage("Favorites");
            favorites.FocusImage = theHost.getSkinImage("Favorites_HL");
            favorites.DownImage = theHost.getSkinImage("Favorites_Selected");
            p.addControl(topBar);
            p.addControl(bottomBar);
            p.addControl(random);
            p.addControl(voldown);
            p.addControl(volup);
            p.addControl(mute);
            p.addControl(ff);
            p.addControl(stop);
            p.addControl(play);
            p.addControl(rw);
            p.addControl(speech);
            p.addControl(back);
            p.addControl(favorites);
            manager.loadPanel(p);
            
            media.addControl(mediaAccent);
            media.addControl(track);
            manager.loadPanel(media);
            return eLoadStatus.LoadSuccessful;
        }

        void play_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "UI", "media");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString(), "SlideDown");
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
            return false;
        }

        public void Dispose()
        {
            //
        }
    }
}
