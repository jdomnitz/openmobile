using System;
using OpenMobile.Plugin;
using OpenMobile;
using AudioRouter;
using XMComm;
using System.Collections.Generic;
using OpenMobile.Data;
using System.Threading;
namespace XMRadio
{
    public class XMRadio:ITunedContent
    {
        public event MediaEvent OnMediaEvent;

        #region private vars
        bool queuePowerOn;
        bool suspending;
        mediaInfo nowPlaying = new mediaInfo();
        XM radio;
        AudioManager router;
        Settings settings;
        eTunedContentStatus status = eTunedContentStatus.Unknown;
        stationInfo[] stations;
        IPluginHost theHost;
        #endregion

        public bool tuneTo(int instance, string station)
        {
            if (!station.StartsWith("XM:"))
                return false;
            int channel;
            if (!int.TryParse(station.Substring(3),out channel))
                return false;
            if (radio == null)
                return false;
            radio.TuneToChannel(channel);
            return true;
        }
        public bool scanBand(int instance)
        {
            if (radio == null)
                return false;
            radio.StartFetch();
            while (radio.IsFetching)
                Thread.Sleep(10);
            return true;
        }

        public bool scanForward(int instance)
        {
            return false; //Not Supported
        }

        public bool scanReverse(int instance)
        {
            return false; //Not Supported
        }

        public bool stepForward(int instance)
        {
            if (radio == null)
                return false;
            radio.TuneUp();
            return true;
        }

        public bool stepBackward(int instance)
        {
            if (radio == null)
                return false;
            radio.TuneDown();
            return true;
        }

        public stationInfo getStationInfo(int instance)
        {
            if (radio == null)
                return new stationInfo();
            if (stations[radio.CurrentTunedChannel] == null)
                return new stationInfo();
            return stations[radio.CurrentTunedChannel];
        }

        public eTunedContentBand[] getSupportedBands(int instance)
        {
            return new eTunedContentBand[]{eTunedContentBand.SiriusXM};
        }

        public bool setPowerState(int instance, bool powerState)
        {
            if ((radio==null)||(router == null)||(status==eTunedContentStatus.Error))
                return false;
            return router.setZonePower(instance, powerState);
        }

        public stationInfo[] getStationList(int instance)
        {
            return Array.FindAll(stations,p=>p!=null);
        }

        public bool setBand(int instance, eTunedContentBand band)
        {
            return (band==eTunedContentBand.SiriusXM);
        }

        public tunedContentInfo getStatus(int instance)
        {
            tunedContentInfo info = new tunedContentInfo();
            info.band = eTunedContentBand.SiriusXM;
            info.channels = 2;
            info.currentStation = getStationInfo(instance);
            if (router == null)
                info.powerState = false;
            else
                info.powerState = Array.Exists(router.ActiveInstances, i => i == instance);
            info.stationList = getStationList(instance);
            info.status = status;
            return info;
        }

        public int playbackPosition
        {
            get { return -1; }
        }

        public bool setVolume(int instance, int percent)
        {
            if (router == null)
                return false;
            return router.setVolume(instance, percent);
        }

        public int getVolume(int instance)
        {
            if (router == null)
                return 0;
            return router.getVolume(instance);
        }
        
        public mediaInfo getMediaInfo(int instance)
        {
            return nowPlaying;
        }
        public string[] OutputDevices
        {
            get { return AudioRouter.AudioRouter.listZones(); }
        }

        public bool SupportsAdvancedFeatures
        {
            get { return false; }
        }

        public bool SetVideoVisible(int instance, bool visible)
        {
            throw new NotImplementedException();
        }

        #region IBasePlugin Members
        public eLoadStatus initialize(IPluginHost host)
        {
            radio = new XMComm.XM();
            theHost = host;
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            radio.XMEventChannelChanged += new ChannelDataParamsEvent(radio_XMEventChannelChanged);
            radio.XMEventChannelData += new ChannelDataParamsEvent(radio_XMEventChannelData);
            radio.XMEventRadioReady += new NoParamsEvent(radio_XMEventRadioReady);
            radio.XMEventPortOpened += new NoParamsEvent(radio_XMEventPortOpened);
            radio.XMEventErrorRadioNotFound += new NoParamsEvent(radio_XMEventErrorRadioNotFound);
            radio.XMEventSignalStrength += new SignalParamsEvent(radio_XMEventSignalStrength);
            radio.XMEventSongChanged += new SongInfoDataParamsEvent(radio_XMEventSongChanged);
            using (PluginSettings s = new PluginSettings())
            {
                if (s.getSetting("XMRadio.InstantOn") == "True")
                    queuePowerOn = true;
                string src=s.getSetting("XMRadio.Source");
                if (src == "")
                    router = new AudioManager(0);
                else
                    router = new AudioManager(int.Parse(src));
                router.FadeIn = router.FadeOut = (s.getSetting("XMRadio.Fade") == "True");
            }
            router.PowerOnHardware += new AudioManager.PowerEvent(router_PowerOnHardware);
            router.PowerOffHardware += new AudioManager.PowerEvent(router_PowerOffHardware);
            radio.AutoSearch = true; //TODO: Specify comport once supported by driver
            radio.Open();
            status = eTunedContentStatus.Scanning;
            loadPrevious();
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SleepOrHibernatePending)
            {
                suspending = true;
                if (router != null)
                    router.Suspend(); //Powers off the hardware
            }
            else if (type == ePowerEvent.SystemResumed)
            {
                queuePowerOn = true;
                if (!radio.IsOpen)
                    radio.Open();
            }
        }

        void radio_XMEventSongChanged(ref XM.SongInfoDataStructure SongInfo)
        {
            nowPlaying.Artist = SongInfo.Artist;
            nowPlaying.Name = SongInfo.Title;
            foreach (int i in router.ActiveInstances)
                raiseMediaEvent(eFunction.Play, i, "");
        }

        private void loadPrevious()
        {
            using (PluginSettings s = new PluginSettings())
            {
                if (s.getSetting("XMRadio.Resume") != "True")
                    return;
                for (int i = 0; i < AudioRouter.AudioRouter.listZones().Length; i++)
                {
                    string tmp = s.getSetting("Radio.LastPlaying.Instance" + i.ToString());
                    if (tmp != "")
                        theHost.execute(eFunction.tuneTo, i.ToString(), tmp);
                }
            }
        }

        void radio_XMEventSignalStrength(int SatelliteSignal, int TerrestrialSignal)
        {
            if (stations == null)
                return;
            int signal = 0;
            if (TerrestrialSignal > SatelliteSignal)
                signal = (int)(TerrestrialSignal * (5.0 / 3));
            else
                signal = (int)(SatelliteSignal * (5.0 / 3));
            for(int i=0;i<stations.Length;i++)
                if (stations[i]!=null)
                    stations[i].signal = signal;
            foreach (int i in router.ActiveInstances)
                raiseMediaEvent(eFunction.tunerDataUpdated, i, "");
        }
        void radio_XMEventErrorRadioNotFound()
        {
            if (radio.AutoSearch == false)
            {
                radio.AutoSearch = true;
                radio.Open();
                return;
            }
            status = eTunedContentStatus.Error;
        }

        void radio_XMEventPortOpened()
        {
            if (queuePowerOn)
                radio.PowerOn();
        }

        void radio_XMEventRadioReady()
        {
            stations = new stationInfo[250];
            byte autoMessage = 0;
            //autoMessage |= (byte)XMComm.XM.AutoMessageConstants.AUTO_SONG_CHANGE_ALL;
            autoMessage |= (byte)XMComm.XM.AutoMessageConstants.AUTO_SONG_CHANGE_THIS;
            autoMessage |= (byte)XMComm.XM.AutoMessageConstants.AUTO_SIGNAL;
            radio.SetAutoMessages(autoMessage);
            if (radio.LastTunnedChannel != 0)
                radio.TuneToChannel(radio.LastTunnedChannel);
            if (suspending)
            {
                suspending = false;
                if (router != null)
                    router.Resume(); //Fades everything back in
            }
            radio.StartFetch();
            //radio.AutoSearch = false; //We know the port from now on
        }
        void radio_XMEventChannelData(ref XM.ChannelDataStructure ChannelData)
        {
            stationInfo info = new stationInfo(ChannelData.LongChannelName, "XM:" + ChannelData.ChannelNumber.ToString());
            info.stationGenre = ChannelData.LongGenreName;
            info.Channels = 2;
            stations[ChannelData.ChannelNumber]=info;
            if (ChannelData.ChannelNumber == radio.CurrentTunedChannel)
                setNowPlaying(ChannelData);
            foreach (int i in router.ActiveInstances)
                raiseMediaEvent(eFunction.stationListUpdated, i, "");
        }
        private void raiseMediaEvent(eFunction function, int instance,string arg)
        {
            if (OnMediaEvent != null)
                OnMediaEvent(function, instance, arg);
        }
        private void setNowPlaying(XMComm.XM.ChannelDataStructure ChannelData)
        {
            nowPlaying.Type = eMediaType.Radio;
            nowPlaying.Genre = ChannelData.LongGenreName;
            nowPlaying.Name = ChannelData.Title;
            nowPlaying.TrackNumber = ChannelData.ChannelNumber;
            nowPlaying.coverArt = theHost.getSkinImage("XM|" + ChannelData.ChannelNumber.ToString(), true).image;
            if (nowPlaying.coverArt==null)
                nowPlaying.coverArt = theHost.getSkinImage("siriusxm").image;
            nowPlaying.Artist = ChannelData.Artist;
            foreach (int i in router.ActiveInstances)
                raiseMediaEvent(eFunction.Play, i, "");
        }
        void radio_XMEventChannelChanged(ref XM.ChannelDataStructure ChannelData)
        {
            status = eTunedContentStatus.Tuned;
            foreach (int i in router.ActiveInstances)
            {
                raiseMediaEvent(eFunction.tuneTo, i, "XM:" + ChannelData.ChannelNumber.ToString());
            }
            setNowPlaying(ChannelData);
        }

        bool router_PowerOffHardware()
        {
            queuePowerOn = false;
            using (PluginSettings s = new PluginSettings())
                if ((!suspending)&&(s.getSetting("XMRadio.InstantOn") == "True"))
                    return true; //Lie
            if (radio.IsPowered)
                radio.PowerOff();
            if (suspending)
                radio.Close();
            return true;
        }
        bool router_PowerOnHardware()
        {
            if (radio.IsPowered)
                return true;
            if (radio.IsOpen == false)
                queuePowerOn = true;
            else
                radio.PowerOn();
            return true;
        }
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("XM Radio");
                using(PluginSettings s=new PluginSettings())
                {
                    settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
                    settings.Add(new Setting(SettingTypes.MultiChoice, "XMRadio.Resume", "", "Resume Radio on Startup", Setting.BooleanList, Setting.BooleanList, s.getSetting("XMRadio.Resume")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "XMRadio.Source", "Source", "Input the radio is connected to", new List<string>(AudioRouter.AudioRouter.listSources()), getSourceValues(), s.getSetting("XMRadio.Source")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "XMRadio.Fade", "", "Fade in/out Radio", Setting.BooleanList, Setting.BooleanList, s.getSetting("XMRadio.Fade")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "XMRadio.InstantOn", "", "Enable Instant-On", Setting.BooleanList, Setting.BooleanList, s.getSetting("XMRadio.InstantOn")));
                }
            }
            return settings;
        }
        void settings_OnSettingChanged(Setting setting)
        {
            if (setting.Name == "XMRadio.Source")
            {
                if (router != null)
                    router.Dispose();
                router = new AudioManager(int.Parse(setting.Value));
            }
            if (setting.Name == "XMRadio.Fade")
            {
                if (router != null)
                {
                    router.FadeIn = router.FadeOut = (setting.Value == "True");
                }
            }
            using (PluginSettings s = new PluginSettings())
                s.setSetting(setting.Name, setting.Value);
        }

        private List<string> getSourceValues()
        {
            List<string>ret=new List<string>();
            int total = AudioRouter.AudioRouter.listSources().Length;
            for (int i = 0; i < total; i++)
                ret.Add(i.ToString());
            return ret;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            get { return "XM Radio"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "XM Radio Support"; }
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
            saveSettings();
            if (radio!=null)
                radio.Dispose();
            if (router!=null)
                router.Dispose();
        }

        private void saveSettings()
        {
            if ((radio == null) || (router == null))
                return;
            using (PluginSettings s = new PluginSettings())
            {
                if (s.getSetting("XMRadio.Resume") != "True")
                    return;
                for (int i = 0; i < AudioRouter.AudioRouter.listZones().Length; i++)
                    if ((Array.Exists(router.ActiveInstances,k=>k==i))&&(radio.CurrentTunedChannel>0))
                        s.setSetting("Radio.LastPlaying.Instance" + i.ToString(),"XM:"+radio.CurrentTunedChannel.ToString());
                    else
                        s.setSetting("Radio.LastPlaying.Instance" + i.ToString(),"");
            }
        }

        #endregion
    }
}