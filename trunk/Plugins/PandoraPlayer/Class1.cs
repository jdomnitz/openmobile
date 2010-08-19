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
using Client;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Plugin;
using System.Diagnostics;

namespace PandoraPlayer
{
    public class Pandora:ITunedContent,IPausable
    {
        PClient client;
        int instance=0;
        public bool tuneTo(int instance, string station)
        {
            lock (this)
            {
                if (client == null)
                    return false;
                if (station.StartsWith("Pandora:") == false)
                    return false;
                if (tuning == true)
                    return false;
                tuning = true;
                new Thread(delegate()
                {
                    for (int i = 0; i < 100; i++)
                        if ((!loggedIn) || (stationsAvailable))
                            Thread.Sleep(50);
                    station = station.Substring(8);
                    client.ChangeStation(station);
                    client.GetPlaylist();
                }).Start();
                return true;
            }
        }
        public bool scanBand(int instance)
        {
            return false; //Not Supported
        }
        public bool scanForward(int instance)
        {
            if (client != null)
            {
                client.RateGood();
                return true;
            }
            return false;
        }

        public bool scanReverse(int instance)
        {
            if (client != null)
            {
                client.RateBad();
                return true;
            }
            return false;
        }
        List<DateTime> skips = new List<DateTime>();
        public bool stepForward(int instance)
        {
            if (client != null)
            {
                lock (this)
                {
                    if (!checkSkip())
                        return true; //lie
                    client.SkipSong();
                    return true;
                }
            }
            return false;
        }

        private bool checkSkip()
        {
            skips.RemoveAll(p => p.AddHours(1) < DateTime.Now);
            if (skips.Count >= 6)
                return false;
            skips.Add(DateTime.Now);
            return true;
        }

        public bool stepBackward(int instance)
        {
            return false;
        }

        public stationInfo getStationInfo(int instance)
        {
            stationInfo info=new stationInfo();
            info.HD=true;
            info.signal=5;
            if (client.CurrentStation != null)
            {
                info.stationName = client.CurrentStation.Name;
                info.stationID = "Pandora:"+client.CurrentStation.SID;
            }
            info.Channels = 2;
            info.Bitrate = 196;
            return info;
        }

        public eTunedContentBand[] getSupportedBands(int instance)
        {
            return new eTunedContentBand[]{eTunedContentBand.Other};
        }

        public bool setPowerState(int instance, bool powerState)
        {
            tuning = false;
            if (powerState == true)
            {
                if (client == null)
                {
                    this.instance = instance;
                    int vol = getVolume(instance);
                    client = new PClient(false);
                    stationsAvailable=loggedIn=false;
                    client.LoggedIn += new StringEventHandler(client_LoggedIn);
                    client.StationChanged += new StringEventHandler(client_StationChanged);
                    client.SongPlayed += new SongEventHandler(client_SongPlayed);
                    client.StationsAvailable += new StringEventHandler(client_StationsAvailable);
                    client.Error += new StringEventHandler(client_Error);
                    client.Volume = vol;
                    initialize();
                    return true;
                }
            }
            else
            {
                if (client != null)
                    client.Dispose();
                loggedIn = false;
                currentSong = new mediaInfo();
                client = null;
                raiseMediaEvent(eFunction.Stop, "");
                return true;
            }
            return false;
        }

        void client_Error(object o, StringEventArgs e)
        {
            if (theHost != null)
                theHost.sendMessage("OMDebug", "Pandora", e.Value);
            Debug.Print(e.Value);
        }
        bool stationsAvailable;
        void client_StationsAvailable(object o, StringEventArgs e)
        {
            stationsAvailable = true;
            raiseMediaEvent(eFunction.stationListUpdated, "");
        }

        private void initialize()
        {
            client.username = Credentials.getCredential("Pandora Username");
            client.password = Credentials.getCredential("Pandora Password");
            if ((client.password != null) && (client.username != null))
                client.startWorker();
        }
        bool loggedIn = false;
        void client_LoggedIn(object o, StringEventArgs e)
        {
            lock (this)
            {
                if ((client!=null)&&(!loggedIn))
                    client.startFetch();
                loggedIn = true;
            }
        }
        bool tuning=false;
        void client_SongPlayed(object o, SongEventArgs e)
        {
            lock (this)
            {
                tuning = false;
                currentSong = new mediaInfo();
                if (e.Song.AArtUrl != null)
                {
                    currentSong.coverArt = OpenMobile.Net.Network.imageFromURL(e.Song.AArtUrl);
                    if (currentSong.coverArt != null)
                    {
                        Graphics g = Graphics.FromImage(currentSong.coverArt.image);
                        imageItem p = theHost.getSkinImage("Pandora");
                        if (p != null)
                        {
                            g.DrawImage(theHost.getSkinImage("Pandora").image.image, new Rectangle(currentSong.coverArt.Width - 30, currentSong.coverArt.Height - 30, 30, 30));
                        }
                        g.Dispose();
                    }
                    else
                    {
                        currentSong.coverArt = theHost.getSkinImage("Pandora").image;
                    }
                }
                else
                {
                    currentSong.coverArt = theHost.getSkinImage("Pandora").image;
                }
                currentSong.Name = e.Song.Title;
                currentSong.Artist = e.Song.Artist;
                currentSong.Album = e.Song.Album;
                currentSong.Genre = e.Song.Genre;
                currentSong.Type = eMediaType.InternetRadio;
                if (client!=null)
                    currentSong.Length = client.PlaybackLength;
                raiseMediaEvent(eFunction.Play, "");
            }
        }
        private void raiseMediaEvent(eFunction function, string arg)
        {
            if (OnMediaEvent!=null)
                OnMediaEvent(function, instance, arg);
        }
        void client_StationChanged(object o, StringEventArgs e)
        {
            raiseMediaEvent(eFunction.tuneTo, e.Value);
        }

        public stationInfo[] getStationList(int instance)
        {
            List<stationInfo> lst=new List<stationInfo>();
            if (client == null)
                return lst.ToArray();
            foreach (KeyValuePair<string,Station> s in client.Stations)
                lst.Add(new stationInfo(s.Value.Name, "Pandora:" + s.Value.SID));
            return lst.ToArray();
        }

        public bool setBand(int instance, OpenMobile.eTunedContentBand band)
        {
            return false;
        }

        public tunedContentInfo getStatus(int instance)
        {
            tunedContentInfo info = new tunedContentInfo();
            info.band = eTunedContentBand.Other;
            info.channels = 2;
            info.powerState = (client != null);
            info.currentStation = getStationInfo(instance);
            info.stationList = getStationList(instance);
            info.status = (client != null)? eTunedContentStatus.Tuned:eTunedContentStatus.Unknown;
            return info;
        }

        #region IPlayer Members

        public bool setVolume(int instance, int percent)
        {
            if (settings.Count == 3)
                settings[2].Value = percent.ToString();
            if (client == null)
                return false;
            client.Volume = percent;
            return true;
        }

        public int getVolume(int instance)
        {
            if (client == null)
                if ((settings==null)||(settings.Count<3)||(settings[2].Value == ""))
                    return 100;
                else
                    return int.Parse(settings[2].Value);
            return client.Volume;
        }
        mediaInfo currentSong=new mediaInfo();
        public mediaInfo getMediaInfo(int instance)
        {
            return currentSong;
        }

        public event MediaEvent OnMediaEvent;

        public string[] OutputDevices
        {
            get { return new string[]{"Default Device"}; }
        }

        public bool SupportsAdvancedFeatures
        {
            get { return false; }
        }

        public bool SetVideoVisible(int instance, bool visible)
        {
            return false;
        }

        #endregion

        #region IBasePlugin Members

        public string authorName
        {
            get { return ""; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "Pandora"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Pandora Player"; }
        }

        bool paused;
        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool pause(int instance)
        {
            if (client != null)
            {
                client.PlayPause();
                paused = !paused;
            }
            if (!paused)
                raiseMediaEvent(eFunction.Play, instance.ToString());
            return paused;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        IPluginHost theHost;
        Settings settings;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.closeProgram)
                fadeOut();
        }
        void changed(Setting s)
        {
            if (s.Name == "Pandora.Volume")
            {
                this.setVolume(0, int.Parse(s.Value));
                return;
            }
            if (s.Name == "Pandora.Username")
                Credentials.setCredential("Pandora Username", s.Value);
            else if (s.Name == "Pandora.Password")
                Credentials.setCredential("Pandora Password", s.Value);
            initialize();
        }
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Pandora Settings");
                using (PluginSettings setting = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.Text, "Pandora.Username", "", "Username",Credentials.getCredential("Pandora Username")));
                    settings.Add(new Setting(SettingTypes.Password, "Pandora.Password", "", "Password", Credentials.getCredential("Pandora Password")));
                    string volume=setting.getSetting("Pandora.Volume");
                    settings.Add(new Setting(SettingTypes.Range,"Pandora.Volume","Volume","Currently at %value%%",null,new List<string>(new string[]{"0","100"}), (volume=="")?"100":volume));
                }
                settings.OnSettingChanged += new SettingChanged(changed);
            }
            return settings;
        }
        void theHost_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
            {
                fadeIn();
            }
            else if (type == ePowerEvent.SleepOrHibernatePending)
            {
                if (client != null)
                    client.Volume = 0;
            }
        }

        public int playbackPosition
        {
            get
            {
                if (client == null)
                    return -1;
                return client.PlaybackPosition;
            }
        }

        private void fadeIn()
        {
            int vol=getVolume(0);
            if (client != null)
            {
                for (int i = 0; i <vol/10; i++)
                {
                    client.Volume = i * 10;
                    Thread.Sleep(150);
                }
                client.Volume = vol;
            }
        }
        private bool fading = false;
        private void fadeOut()
        {
            fading = true;
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Pandora.Volume", getVolume(0).ToString());
            if (client != null)
            {
                for (int i = getVolume(0)/10; i > 0; i--)
                {
                    client.Volume = i * 10;
                    Thread.Sleep(150);
                }
            }
            fading = false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (client != null)
            {
                int kill = 0;
                while ((fading)&&(kill<16))
                {
                    kill++;
                    Thread.Sleep(100);
                }
                client.Dispose();
            }
        }

        #endregion
    }
}
