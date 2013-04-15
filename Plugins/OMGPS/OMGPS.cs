/**** ToDo

- Alphabetized countryList (Being Worked on)
- Take "disconnects" from the internet into account (if a file is currently being d/l'ed)
  - store name/progress and attempt to re-enable on internet connected event
- Fix auto d/l of updated timestamped country zip files (coded, not tested)
- Icons ('down arrow' for Downloading, ??? for processing)
- Code cleanup...
- Bugs?

****/

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Timers;
using System.Net;
using System.IO;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Plugins;
using OpenMobile.Framework;
using Mono.Data.Sqlite;

namespace OMGPS
{
    //[PluginLevel(PluginLevels.System)]
    public class OMGPS : IHighLevel, IDataSource
    {
        ScreenManager manager;
        public static IBasePlugin ibp;
        private delegate void GPSAddedEventHandler(SerialPort port);
        private event GPSAddedEventHandler GPSAdded;
        private delegate void SentenceFoundEventHandler(string sentence);
        private event SentenceFoundEventHandler SentenceFound;
        private delegate void RunGPSThreadEventHandler();
        private event RunGPSThreadEventHandler RunGPSThread;
        public static IPluginHost theHost;
        public static SerialPort GPSPort;
        List<SerialPort> portsToTest;
        Thread gpsThread;
        bool killThread;
        public static Notification GPSNotification;
        public static Notification DLNotification;
        public static Notification ProcessingNotification;
        private string dataStream;
        System.Threading.AutoResetEvent checkIt;
        List<string> baudRates = new List<string>() { "115200", "57600", "38400", "19200", "9600", "4800", "2400" };
        bool gpsadded = false;
        Dictionary<string, string> downloadQueue;
        string currentRange;
        string currentDownloading;
        Dictionary<string, string> geoNamesList;

        List<DownloadItem> queueList;
        List<DownloadItem> downloadList;
        Settings settings;


        bool checkboxesmade;

        public eLoadStatus initialize(IPluginHost host)
        {
            ibp = this;
            manager = new ScreenManager(this);
            if (StoredData.Get(this, "GPS.Port.BaudRate") == "")
                StoredData.Set(this, "GPS.Port.Name", "");
            theHost = host;
            killThread = false;
            portsToTest = new List<SerialPort>();
            downloadQueue = new Dictionary<string, string>();
            geoNamesList = new Dictionary<string, string>();
            queueList = new List<DownloadItem>();
            downloadList = new List<DownloadItem>();

            //events
            this.GPSAdded += new GPSAddedEventHandler(this_GPSAdded);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);

            //dataSources
            CreateDataSources();

            //notification
            //GPSNotification = new Notification(Notification.Styles.Normal, this, "GPS", OM.Host.getSkinImage("Icons|Icon-GPS").image);
            //GPSNotification.Header = "GPS Status";
            //GPSNotification.Text = "Sats () -- Lat: * , Lon: *";
            GPSNotification = new Notification(this, "GPS", OM.Host.getSkinImage("Icons|Icon-GPS").image, OM.Host.getSkinImage("AIcons|10-device-access-location-off").image, "Connecting to GPS", "Detecting device");
            GPSNotification.ClearAction += new Notification.NotificationAction(GPSNotification_ClearAction);
            theHost.UIHandler.AddNotification(GPSNotification);
            //DLNotification = new Notification(this, "GPSDL", imageItem.NONE, imageItem.None, """, "");
            //theHost.UIHandler.AddNotification(DLNotifiction);

            //start threads
            checkIt = new AutoResetEvent(false);
            new Thread(Checker).Start();
            gpsThread = new Thread(GPSThread);
            gpsThread.Start();

            DownloadItem.DownloadFinished += new DownloadItem.DownloadFinishedEventHandler(DownloadItem_DownloadFinished);
            ProcessingItem.ProcessingFinished += new ProcessingItem.ProcessingFinishedEventHandler(ProcessingItem_ProcessingFinished);

            if (ProcessingQueue.processingList == null)
                ProcessingQueue.processingList = new List<ProcessingItem>();
            if (StoredData.Get(this, "GPS.DL.Processing") != "")
            {
                string[] filenames = StoredData.Get(this, "GPS.DL.Processing").Split('|');
                for (int i = 0; i < filenames.Length; i++)
                {
                    ProcessingQueue.processingList.Add(new ProcessingItem(filenames[i]));
                    ProcessingQueue.processingList[i].Start();
                }
            }

            //Create the panel
            OMPanel gpsPanel = new OMPanel("gpsPanel");
            OMObjectList countryList = new OMObjectList("countryList", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            gpsPanel.addControl(countryList);


            manager.loadPanel(gpsPanel);

            //StartNetworkTimer();
            checkboxesmade = false;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);

            return eLoadStatus.LoadSuccessful;

        }

        private void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            switch (function)
            {
                case eFunction.connectedToInternet:
                    if (!checkboxesmade)
                    {
                        for (int i = 0; i < theHost.ScreenCount; i++)
                            MakeCheckboxes(i);
                    }
                    break;
                case eFunction.disconnectedFromInternet:

                    break;
            }
        }

        void GPSNotification_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            // Change notification type to static
            notification.State = Notification.States.Active;
            notification.Style = Notification.Styles.IconOnly;
            // Cancel the clear request on this notification
            cancel = true;
        }

        private void CreateDataSources()
        {
            //create dataSources
            BuiltInComponents.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "MPH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS MPH"), 0);
            BuiltInComponents.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "KMH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS KMH"), 0);
            BuiltInComponents.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Count", OpenMobile.Data.DataSource.DataTypes.raw, "GPS Satelite count"), 0);
            BuiltInComponents.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Fix", OpenMobile.Data.DataSource.DataTypes.binary, "GPS Satelite fix state"), false);
        }

        private void this_GPSAdded(SerialPort port)
        {
            //we have an open port which receives gps data
            //hook the ports datareceived and start receiving
            gpsadded = true;
            GPSPort.DataReceived += new SerialDataReceivedEventHandler(GPSPort_DataReceived);
        }

        private void GPSPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                dataStream += GPSPort.ReadExisting();
                checkIt.Set();
            }
            catch { }
        }

        private void Checker()
        {
            string lineToParse;
            while (true)
            {
                checkIt.WaitOne();
                while (dataStream.Contains("\r\n"))
                {
                    lineToParse = dataStream.Substring(0, dataStream.IndexOf("\r\n"));
                    if (lineToParse.Contains("$"))
                    {
                        lineToParse = lineToParse.Remove(0, lineToParse.IndexOf("$"));
                        OpenMobile.Threading.SafeThread.Asynchronous(() => { GPSParser.Parse(lineToParse); });
                    }
                    dataStream = dataStream.Remove(0, dataStream.IndexOf("\r\n") + 2);
                }
            }
        }

        private void GPSThread()
        {
            //SerialAccess.GetAccess();
            if (!TryPreviousConnection())
                AutoConnect();
            //SerialAccess.ReleaseAccess();
        }

        private bool TryPreviousConnection()
        {
            if ((GPSPort == null) && (StoredData.Get(this, "GPS.Port.Name") != ""))
            {
                GPSPort = new SerialPort(StoredData.Get(this, "GPS.Port.Name"));
                GPSPort.BaudRate = Convert.ToInt32(StoredData.Get(this, "GPS.Port.BaudRate"));
                portsToTest.Clear();
                portsToTest.Add(GPSPort);
                if (TestIncomingData())
                    return true;
            }
            return false;
        }

        private void AutoConnect()
        {
            int comCount = 0;
            portsToTest.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                foreach (string p in baudRates)
                {
                    GPSNotification.Text = "Searching: " + s.Trim() + " (" + p + ")";
                    GPSNotification.IconStatusBar = OM.Host.getSkinImage("AIcons|10-device-access-location-searching").image;
                    comCount += 1;
                    SerialPort port = new SerialPort(s);
                    port.BaudRate = Convert.ToInt32(p);
                    //test opening port, if successfull, add it to the list to test...
                    try
                    {
                        SerialAccess.GetAccess();
                        port.Open();
                        //if the port opens, test the data
                        //to make sure it IS a gps unit
                        port.Close();
                        SerialAccess.ReleaseAccess();
                        portsToTest.Add(port);

                    }
                    catch (Exception ex) { } //port not able to be opened, already in use/open???
                }
            }
            if (portsToTest.Count == 0)
            {
                theHost.UIHandler.RemoveNotification(this, "GPS");
            }
            else
                TestIncomingData();
        }

        private bool TestIncomingData()
        {
            //keep this to one at a time, so we can reliably know when to stop testing if
            //a com device with gps data is found
            SerialPort port;
            StringBuilder sb = new StringBuilder();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < portsToTest.Count; i++)
            {
                if (killThread)
                    break;
                if (portsToTest[i].PortName == "")
                    return false;
                port = portsToTest[i];
                sw.Start();
                try
                {
                    //GPSNotification.Header = "Testing: " + port.PortName.Trim() + " ("+port.BaudRate.ToString()+")";
                    GPSNotification.Text = "Testing: " + port.PortName.Trim() + " (" + port.BaudRate.ToString() + ")";
                    SerialAccess.GetAccess();
                    port.Open();
                    while (true)
                    {
                        if (killThread)
                            break;
                        sb.Append(port.ReadExisting());
                        if (sb.ToString().Contains("GPGGA"))
                        {
                            SerialAccess.ReleaseAccess();
                            //use this port for gps polling
                            GPSPort = port;
                            sw.Stop();
                            //raise GPSConnectedEvent
                            if (GPSAdded != null)
                                GPSAdded(port);
                            GPSNotification.Header = "GPS Connected: " + GPSPort.PortName + " (" + GPSPort.BaudRate.ToString() + ")";
                            GPSNotification.Text = "Sats (0) -- Lat: * , Lon: *";
                            StoredData.Set(this, "GPS.Port.Name", GPSPort.PortName);
                            StoredData.Set(this, "GPS.Port.BaudRate", GPSPort.BaudRate.ToString());
                            return true;

                        }
                        else if (sw.ElapsedMilliseconds >= 1200)
                        {
                            //test how long we've been polling and after a certain amount of time (3 sec?)
                            //close port because it doesn't contains gps data
                            port.Close();
                            SerialAccess.ReleaseAccess();
                            break;
                        }
                    }
                    if (killThread)
                    {
                        port.Close();
                    }
                    SerialAccess.ReleaseAccess();
                    sw.Reset();
                }
                catch (Exception ex) { }
            }
            //theHost.UIHandler.RemoveNotification(this, "GPS");
            GPSNotification.Header = "No COM Port Found With GPS Data";
            GPSNotification.Text = "";

            return false;
        }

        private void theHost_OnPowerChange(OpenMobile.ePowerEvent e)
        {
            switch (e)
            {
                case ePowerEvent.SystemResumed:
                    Reviving();
                    break;
                case ePowerEvent.SleepOrHibernatePending:
                    Killing();
                    break;
                case ePowerEvent.ShutdownPending:
                    Killing();
                    break;
            }
        }

        private void Reviving()
        {
            killThread = false;
            if ((gpsThread == null) || (gpsThread.ThreadState == ThreadState.Aborted) || (gpsThread.ThreadState == ThreadState.Stopped))
                gpsThread = new Thread(GPSThread);
            gpsThread.Start();
            if (ProcessingQueue.processingList == null)
                ProcessingQueue.processingList = new List<ProcessingItem>();
        }

        private void Killing(bool disposing = false)
        {
            killThread = true;
            dataStream = "";
            gpsThread.Abort();
            gpsThread.Join();
            if ((GPSPort != null) && (GPSPort.IsOpen))
            {
                GPSPort.Close();
                GPSPort.DataReceived -= GPSPort_DataReceived;
                if (disposing)
                    GPSPort.Dispose();
            }
            if (DLNotification != null)
            {
                //theHost.UIHandler.RemoveNotification(DLNotification,true);
                DLNotification.Dispose();
            }
            for (int i = 0; i < downloadList.Count; i++)
            {
                if (downloadList[i].IsDownloading)
                {
                    downloadList[i].Closing = true;
                    downloadList[i].doDownload = false;
                    downloadList[i].downloadThread.Join();
                    StoredData.Set(this, "GPS.DLName", downloadList[i].Name);
                    StoredData.Set(this, "GPS.DLProgress", downloadList[i].Progress);
                    StoredData.Set(this, "GPS.DL." + i.ToString() + ".ts", downloadList[i].ts.ToString());
                    break;
                }
            }
            if (ProcessingQueue.processingList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ProcessingQueue.processingList.Count; i++)
                {
                    ProcessingQueue.processingList[i].Stop();
                    if (ProcessingQueue.processingList[i].IsProcessing)
                    {
                        if (i == 0)
                        {
                            sb.Append(ProcessingQueue.processingList[i].Name);
                            StoredData.Set(this, "GPS.DL.Processing.Progress", ProcessingQueue.processingList[i].ProcessNumber.ToString());
                        }
                        else
                            sb.Append("|" + ProcessingQueue.processingList[i].Name);
                    }
                }
                StoredData.Set(this, "GPS.DL.Processing", sb.ToString());
            }
            else
                StoredData.Set(this, "GPS.DL.Processing", "");
        }

        public static void StoreProgress(string progress)
        {
            StoredData.Set(ibp, "GPS.DLProgress", progress);
        }

        public void Dispose()
        {
            Killing(true);
        }

        private void MakeCheckboxes(int screen1)
        {
            try
            {
                string html = "";
                using (System.Net.WebClient wc = new System.Net.WebClient())
                    html = wc.DownloadString("http://download.geonames.org/export/dump/");
                html = html.Remove(0, html.IndexOf("Parent Directory"));
                html = html.Substring(0, html.IndexOf(".txt<"));
                html = html.Substring(0, html.LastIndexOf("<img src=") + 5);
                int cnt = 0;
                OMObjectList.ListItem ItemBase = new OMObjectList.ListItem();
                OMCheckbox chk = new OMCheckbox("chk", 0, 0, theHost.ClientArea[0].Width, 40);
                ItemBase.Add(chk);
                ItemBase.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
                {
                    OMCheckbox check = item["chk"] as OMCheckbox;
                    //check.Name = values[0] as string;
                    check.Tag = values[0] as string;
                    check.Text = values[1] as string;
                    if (values[2] as string == "True")
                        check.Checked = true;
                    else
                        check.Checked = false;
                    check.OnClick += new userInteraction(chk_OnClick);
                };
                OMObjectList countryList = manager[screen1, "gpsPanel"]["countryList"] as OMObjectList;
                countryList.ItemBase = ItemBase;
                List<string[]> tempList = new List<string[]>();
                while (html.Contains("a href="))
                {
                    html = html.Remove(0, html.IndexOf("a href=") + 8);
                    string ts = html.Remove(0, html.IndexOf("a>") + 2).Trim();
                    ts = ts.Substring(0, ts.IndexOf("<i")).Trim();
                    string[] tssplit = new string[2];
                    tssplit[0] = ts.Substring(0, ts.IndexOf(" ")).Trim(); // + " " + (ts.Remove(0, ts.IndexOf(" ")).Trim()).Substring(0, ts.LastIndexOf(" ")).Trim();
                    ts = ts.Remove(0, ts.IndexOf(" ")).Trim();
                    tssplit[0] = tssplit[0] + " " + ts.Substring(0, ts.IndexOf(" ")).Trim();
                    tssplit[1] = ts.Remove(0, ts.LastIndexOf(" ")).Trim();
                    string newname = html.Substring(0, html.IndexOf(">") - 1);
                    newname = Misc.CountryCodeNames(newname.Substring(0, newname.Length - 4));
                    if (newname == "")
                        newname = html.Substring(0, html.IndexOf(">") - 1);
                    tempList.Add(new string[] { html.Substring(0, html.IndexOf(">") - 1), "download.geonames.org/export/dump/" + html.Substring(0, html.IndexOf(">") - 1), tssplit[0].Trim(), tssplit[1].Trim(), newname, cnt.ToString() });

                    //queueList.Add(new DownloadItem(html.Substring(0, html.IndexOf(">") - 1), "download.geonames.org/export/dump/" + html.Substring(0, html.IndexOf(">") - 1), Convert.ToDateTime(tssplit[0].Trim()), tssplit[1].Trim()));
                    //countryList.AddItemFromItemBase(new object[] { cnt.ToString(), newname, StoredData.Get(this, cnt.ToString()) }, ControlDirections.Down);
                    //if ((!downloadList.Contains(queueList[queueList.Count - 1])) && (StoredData.Get(this, cnt.ToString()) == "True"))
                    //    downloadList.Add(queueList[queueList.Count - 1]);
                    cnt += 1;
                }
                //tempList.Sort(new MyComparer());
                //tempList = AlphanumComparatorFast.Sorted(tempList);
                for (int i = 0; i < tempList.Count; i++)
                {
                    queueList.Add(new DownloadItem(tempList[i][0], tempList[i][1], Convert.ToDateTime(tempList[i][2]), tempList[i][3]));
                    countryList.AddItemFromItemBase(new object[] { tempList[i][5], tempList[i][4] + " ( " + tempList[i][2] + ", " + tempList[i][3] + " )", StoredData.Get(this, tempList[i][5]) }, ControlDirections.Down);
                    //"OMGPS.DL.Checked.tempList[i][5]
                    if ((!downloadList.Contains(queueList[queueList.Count - 1])) && (StoredData.Get(this, "GPS.DL.Checked." + tempList[i][5]) == "True"))
                        downloadList.Add(queueList[queueList.Count - 1]);
                }


                //countryList.ItemBase = ItemBase;

                if (downloadList.Count > 0)
                {
                    bool dling = false;
                    for (int i = 0; i < downloadList.Count; i++)
                    {
                        if ((downloadList[i].IsDownloading) || (downloadList[i].Name == StoredData.Get(this, "GPS.DLName")))
                        {
                            dling = true;
                            for (int j = 0; j < queueList.Count; j++)
                            {
                                if (downloadList[i].Name == queueList[j].Name)
                                {
                                    if (Convert.ToDateTime(StoredData.Get(this, "GPS.DL." + i.ToString() + ".ts")) < queueList[j].ts)
                                    {
                                        downloadList[i].ResetDownload();
                                        StoredData.Set(this, "GPS.DL." + i.ToString() + ".ts", queueList[j].ts.ToString());
                                        StoredData.Set(this, "GPS.DLProgress", "");
                                    }
                                    break;
                                }
                            }
                            if (StoredData.Get(this, "GPS.DLProgress") != "")
                                downloadList[i].Progress = Convert.ToInt32(StoredData.Get(this, "GPS.DLProgress"));
                            downloadList[i].Download();
                            break;
                        }
                    }
                    //if we are done looping and nothing was previously downloading, we need to test the timestamps of all the ones in the downloadList
                    if (!dling)
                    {
                        for (int i = 0; i < downloadList.Count; i++)
                        {
                            for (int j = 0; j < queueList.Count; j++)
                            {
                                if (downloadList[i].Name == queueList[j].Name)
                                {
                                    if (Convert.ToDateTime(StoredData.Get(this, "GPS.DL." + i.ToString() + ".ts")) < queueList[j].ts)
                                    {
                                        downloadList[i].ResetDownload();
                                        StoredData.Set(this, "GPS.DL." + i.ToString() + ".ts", queueList[j].ts.ToString());
                                        StoredData.Set(this, "GPS.DLProgress", "");
                                        downloadList[i].Download();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                checkboxesmade = true;
            }
            catch { }
        }

        private void chk_OnClick(object sender, int screen)
        {
            //"OMGPS.DL.Checked."tempList[i][5]
            if (StoredData.Get(this, "GPS.DL.Checked." + ((OMCheckbox)sender).Tag.ToString()) != "True")
                StoredData.Set(this, "GPS.DL.Checked." + ((OMCheckbox)sender).Tag.ToString(), "True");
            else
                StoredData.Set(this, "GPS.DL.Checked." + ((OMCheckbox)sender).Tag.ToString(), "False");
            if (StoredData.Get(this, "GPS.DL.Checked." + ((OMCheckbox)sender).Tag.ToString()) == "False")
            {
                for (int i = 0; i < downloadList.Count; i++)
                {
                    if (downloadList[i].Name == queueList[Convert.ToInt32(((OMCheckbox)sender).Tag.ToString())].Name)
                    {
                        downloadList[i].Closing = true;
                        downloadList[i].downloadThread.Join();
                        downloadList.Remove(downloadList[i]);
                        //1 -> 0, 2 -> 1
                        for (int j = i + 1; j < downloadList.Count; j++)
                            StoredData.Set(this, "GPS.DL." + (j - 1).ToString() + ".ts", StoredData.Get(this, "GPS.DL." + j.ToString() + ".ts"));
                        break;
                    }
                }
            }
            else if (StoredData.Get(this, "GPS.DL.Checked." + ((OMCheckbox)sender).Tag.ToString()) == "True")
            {
                if (!downloadList.Contains(queueList[Convert.ToInt32(((OMCheckbox)sender).Tag.ToString())]))
                {
                    downloadList.Add(queueList[Convert.ToInt32(((OMCheckbox)sender).Tag.ToString())]);
                    bool isdownloading = false;
                    for (int i = 0; i < downloadList.Count; i++)
                    {
                        if (downloadList[i].IsDownloading)
                        {
                            isdownloading = true;
                            //downloadList[downloadList.Count - 1].Download();
                            break;
                        }
                    }
                    if (!isdownloading)
                    {
                        //start downloading
                        downloadList[downloadList.Count - 1].Progress = 0;
                        downloadList[downloadList.Count - 1].Download();
                    }
                }
            }
        }

        private System.Timers.Timer networkTimer;
        private void networkTimer_Elapsed(object sender, EventArgs e)
        {
            networkTimer.Dispose();
        }

        private void testsettings()
        {
            if (settings == null)
                settings = new Settings("OMGPSSettings");

        }

        public Settings loadSettings()
        {
            //return null;
            if (settings == null)
                settings = new Settings("OMGPS Settings");
            settings.Add(new Setting(SettingTypes.Button, "GPS.gpsButton", "", "Country Lists (Downloadables)"));
            settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
            return settings;
        }
        private void StartNetworkTimer()
        {
            //try again in 5 seconds
            networkTimer = new System.Timers.Timer();
            networkTimer.Interval = 5000;
            networkTimer.Elapsed += new ElapsedEventHandler(networkTimer_Elapsed);
            networkTimer.Enabled = true;
        }

        private void settings_OnSettingChanged(int screen, Setting s)
        {
            if (s.Name == "GPS.gpsButton")
            {
                theHost.execute(eFunction.TransitionFromAny, screen.ToString());
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), this.pluginName, "gpsPanel");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                return;
            }

            StoredData.Set(this, s.Name, s.Value);
            //"OMGPS.DL.Checked.tempList[i][5]
            //s.Name = s.Name.Remove(0, s.Name.LastIndexOf(".") + 1);
            if (s.Value == "False")
            {
                for (int i = 0; i < downloadList.Count; i++)
                {
                    if (downloadList[i].Name == queueList[Convert.ToInt32(s.Name)].Name)
                    {
                        downloadList[i].Closing = true;
                        downloadList[i].downloadThread.Join();
                        downloadList.Remove(downloadList[i]);
                        theHost.UIHandler.RemoveNotification(DLNotification, true);
                        DLNotification.Dispose();
                        break;
                    }
                }
            }
            else if (s.Value == "True")
            {
                if (!downloadList.Contains(queueList[Convert.ToInt32(s.Name)]))
                {
                    downloadList.Add(queueList[Convert.ToInt32(s.Name)]);
                    bool isdownloading = false;
                    for (int i = 0; i < downloadList.Count; i++)
                    {
                        if (downloadList[i].IsDownloading)
                        {
                            isdownloading = true;
                            //downloadList[downloadList.Count - 1].Download();
                            break;
                        }
                    }
                    if (!isdownloading)
                    {
                        //start downloading
                        downloadList[downloadList.Count - 1].Progress = 0;
                        downloadList[downloadList.Count - 1].Download();
                    }
                }
            }
        }

        private void ProcessingItem_ProcessingFinished()
        {
            //bool changed = false;
            //int index = 0;
            for (int i = 0; i < ProcessingQueue.processingList.Count; i++)
            {
                if (ProcessingQueue.processingList[i].IsProcessing)
                {
                    ProcessingQueue.processingList[i].IsProcessing = false;
                    StoredData.Set(this, "GPS.DL.Processing", "");
                    StoredData.Set(this, "GPS.DL.Processing.Progress", "0");
                    ProcessingQueue.processingList.RemoveAt(i);
                    theHost.UIHandler.RemoveNotification(ProcessingNotification, true);
                    ProcessingNotification.Dispose();
                    //        changed = true;
                    //        index = i;
                    break;
                }
            }
            //if (changed)
            //{
            //    changed = false;
            if (ProcessingQueue.processingList.Count > 0)
            {
                ProcessingQueue.processingList[0].Copy();
                ProcessingQueue.processingList[0].Start();
            }
            //}
        }

        private void DownloadItem_DownloadFinished()
        {
            for (int i = 0; i < downloadList.Count; i++)
            {
                if (downloadList[i].IsDownloading)
                {
                    downloadList[i].IsDownloading = false;
                    //ProcessingQueue.CurrentlyDownloadingName = "";
                    StoredData.Set(this, "GPS.DLName", "");
                    StoredData.Set(this, "GPS.DL." + i.ToString() + ".ts", downloadList[i].ts.ToString());

                    theHost.UIHandler.RemoveNotification(DLNotification, true);
                    //ProcessFile.Start(downloadList[i].Name);
                    DLNotification.Dispose();

                    //check processing
                    bool processingfound = false;
                    for (int j = 0; j < ProcessingQueue.processingList.Count; j++)
                    {
                        if (ProcessingQueue.processingList[j].Name == downloadList[i].Name)
                        {
                            if (ProcessingQueue.processingList[j].IsProcessing)
                                ProcessingQueue.processingList[j].Stop();
                            ProcessingQueue.processingList[j].Copy();
                            ProcessingQueue.processingList[j].Start();
                            processingfound = true;
                            break;
                        }
                        //ProcessingQueue.processingList.Add(new ProcessingItem(downloadList[i].Name));
                    }
                    if (!processingfound)
                    {
                        //add
                        ProcessingQueue.processingList.Add(new ProcessingItem(downloadList[i].Name));
                        if (ProcessingQueue.processingList.Count == 1)
                        {
                            ProcessingQueue.processingList[ProcessingQueue.processingList.Count - 1].Copy();
                            ProcessingQueue.processingList[ProcessingQueue.processingList.Count - 1].Start();
                        }
                    }

                    if (i < downloadList.Count - 1)
                    {
                        downloadList[i + 1].Progress = 0;
                        downloadList[i + 1].Download();
                        break;
                    }
                }
            }
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen, name]; ;
        }
        public string authorName
        {
            get { return "Peter Yeaney"; }
        }
        public string authorEmail
        {
            get { return "peter.yeaney@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMGPS"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "GPS Data"; }
        }
        public imageItem pluginIcon
        {
            get { return OM.Host.getPluginImage(this, "Icon-GPS"); }
        }
        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        public string displayName
        {
            get { return "GPS Data"; }
        }

    }

    public class GPSParser
    {
        public static void Parse(string sentence)
        {
            string[] sentencesplit = new string[] { sentence.Substring(0, 6), sentence.Remove(0, 7) };
            switch (sentencesplit[0])
            {
                case "$GPGGA":
                    GPGGAParse(sentencesplit[1]);
                    break;
                case "$GPRMC":
                    GPRMCParse(sentencesplit[1]);
                    break;
            }
        }

        private static void GPGGAParse(string sentence)
        {
            string[] data = sentence.Split(',');
            //0=time of fix as utc

            //1=latitude
            float latitude = 0;
            float.TryParse(data[1], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out latitude);
            // Change GPS data to proper decimal format 
            latitude /= 100f;
            double decimals = latitude - Math.Floor(latitude);
            decimals *= 100;
            latitude = (float)(Math.Floor(latitude) + (decimals / 60f));

            //2=latitude direction (n or s)
            if (data[2].ToLower() == "s")
                latitude = -latitude;

            //3=longitude
            float longitude = 0;
            float.TryParse(data[3], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out longitude);
            // Change GPS data to proper decimal format 
            longitude /= 100f;
            decimals = longitude - Math.Floor(longitude);
            decimals *= 100;
            longitude = (float)(Math.Floor(longitude) + (decimals / 60f));

            //4=longitude direction (e or w)
            if (data[4].ToLower() == "w")
                longitude = -longitude;

            //5=fix (0=invalid,1=gps fix,2=dgps fix)
            bool fix = (!data[5].Equals("0"));

            //6=number of sats
            int satCount = 0;
            int.TryParse(data[6], out satCount);

            //update notifications
            if (fix)
            {
                OMGPS.GPSNotification.IconStatusBar = OM.Host.getSkinImage("AIcons|10-device-access-location-found").image;
                OMGPS.GPSNotification.Text = "Sats (" + data[6] + ") -- Lat: " + latitude.ToString() + " , Lon: " + longitude.ToString();
                Location loc = new Location(latitude, longitude);
                BuiltInComponents.Host.CurrentLocation = loc;
            }
            else
            {
                OMGPS.GPSNotification.IconStatusBar = OM.Host.getSkinImage("AIcons|10-device-access-location-searching").image;
                OMGPS.GPSNotification.Text = "Waiting for GPS fix";
            }
            //update data sources
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMGPS;GPS.Sat.Count", satCount);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMGPS;GPS.Sat.Fix", fix);
        }

        private static void GPRMCParse(string sentence)
        {
            string[] data = sentence.Split(',');
            //0=time stamp
            //1 = validity
            //2 = current latitude
            //3 = north/south
            //4 = current longitude
            //5 = east/west
            //6 = speed in knots -- to get mph, *1.15, to get kmh, *1.852
            double mph = 0.00;
            double.TryParse(data[6], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out mph);
            mph *= 1.15;
            double kmh = 0.00;
            double.TryParse(data[6], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out kmh);
            kmh *= 1.852;
            //7 = true course
            //8 = date stamp
            //9 = variation
            //10 east/west
            //12 = checksum
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMGPS;GPS.Speed.MPH", Convert.ToInt32(mph));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMGPS;GPS.Speed.KMH", Convert.ToInt32(kmh));
        }
    }

    internal class MyComparer : IComparer<string[]>
    {
        public int Compare(string[] x, string[] y)
        {
            var string1 = x[4];
            var string2 = y[4];
            return string1.CompareTo(string2);
        }
    }

    public class DownloadItem
    {
        public string Name { get; set; }
        private string Url { get; set; }
        public DateTime ts { get; set; }
        private string Size { get; set; }
        public int Progress;
        public long FileSize;
        public bool IsDownloading { get; set; }
        public bool doDownload;
        private string filename;
        public Thread downloadThread;
        public bool Closing;
        public delegate void DownloadFinishedEventHandler();
        public static event DownloadFinishedEventHandler DownloadFinished;
        public DownloadItem(string _Name, string _Url, DateTime _ts, string _Size)
        {
            Name = _Name;
            Url = _Url;
            ts = _ts;
            Size = _Size;
            IsDownloading = false;
            Progress = 0;
            filename = "" + Name;
        }
        public void ResetDownload()
        {
            Progress = 0;
        }
        public void Download()
        {
            IsDownloading = true;
            ProcessingQueue.CurrentlyDownloadingName = Name;
            doDownload = true;
            //if (OMGPS.DLNotification == null) //create a new notification
            //{
            OMGPS.DLNotification = new Notification(OMGPS.ibp, "GPSDL", OM.Host.getSkinImage("AIcons|arrow down").image, OM.Host.getSkinImage("AIcons|arrow down").image, "Initiating Download: " + Name, "1");
            OMGPS.DLNotification.Text = "";
            OMGPS.theHost.UIHandler.AddNotification(OMGPS.DLNotification);
            //}
            //else //update the existing notification
            //{
            //    OMGPS.DLNotification.Style = Notification.Styles.Normal;
            //    OMGPS.DLNotification.Header = "Initiating Download: " + Name;
            //    OMGPS.DLNotification.Text = "";
            //    OMGPS.theHost.UIHandler.AddNotification(OMGPS.DLNotification);
            //}
            downloadThread = new Thread(new ParameterizedThreadStart(downloadWorker));
            downloadThread.Start(Progress);
        }
        private void downloadWorker(object startint)
        {
            //we only get here if we had successful internet connectivity to fulfill the settings on OMGPS
            //we still need to wrap this entire method in try..catch and start a timer if needed to check network activity (like the settings)

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + Url));
            FileMode filemode;

            if (Progress == 0)
            {
                filemode = FileMode.Create;
            }
            else
            {
                filemode = FileMode.Append;
                myHttpWebRequest.AddRange(Convert.ToInt32(Progress));
            }

            if (!Directory.Exists(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Temp")))
                Directory.CreateDirectory(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Temp"));
            using (FileStream fs = new FileStream(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Temp"), filename), filemode))
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    Stream receiveStream = myHttpWebResponse.GetResponseStream();
                    FileSize = myHttpWebResponse.ContentLength + Progress;
                    byte[] read = new byte[2048];
                    int count = 0;
                    while ((count = receiveStream.Read(read, 0, read.Length)) > 0 && doDownload)
                    {
                        fs.Write(read, 0, count);
                        //count = receiveStream.Read(read, 0, 1024);
                        Progress += count;
                        //filename (% = (Convert.ToDouble(progress/filesize) * 100  + %)) \n progress / FileSize
                        OMGPS.DLNotification.Header = "Downloading: " + Name;
                        OMGPS.DLNotification.Text = "( " + Progress.ToString() + " / " + FileSize.ToString() + "  --  " + (Convert.ToInt32((Convert.ToDouble(Progress) / Convert.ToDouble(FileSize)) * 100)).ToString() + "%";
                        if (Closing)
                        {
                            //OMGPS.StoreProgress(Progress.ToString());
                            Closing = false;
                            break;
                        }
                    }
                    if (Progress >= FileSize)
                        if (DownloadFinished != null)
                            DownloadFinished();
                }
            }
        }
    }

    public class ProcessingQueue
    {
        public static List<ProcessingItem> processingList;
        public static bool running;
        public static string CurrentlyDownloadingName;

        public static void Processing()
        {
            running = true;
            while (running)
            {
                for (int i = 0; i < processingList.Count; i++)
                {
                    //we need to make sure it's not currently downloading first...
                    if (processingList[i].Name != CurrentlyDownloadingName)
                    {
                        //start processing this file -> unzip and put into sql inside here (no need to thread this)

                    }
                }
            }
        }
    }

    public class ProcessingItem
    {
        public bool IsProcessing;
        public string Name;
        public bool Running;
        public int ProcessNumber;
        public Thread ProcessingThread;
        private SqliteConnection sqlConn;
        public delegate void ProcessingFinishedEventHandler();
        public static event ProcessingFinishedEventHandler ProcessingFinished;
        public ProcessingItem(string name)
        {
            IsProcessing = false;
            Name = name;
            Running = false;
        }
        public void Copy()
        {
            //copy zip from Temp to Process folder, overwriting if necessary and unzip the contents...
            if (!Directory.Exists(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process")))
                Directory.CreateDirectory(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"));
            System.IO.File.Copy(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Temp"), Name), OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"), Name), true);
            //unzip?
            using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zis = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(System.IO.File.OpenRead(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"), Name))))
            {
                ICSharpCode.SharpZipLib.Zip.ZipEntry ze;
                try
                {
                    while ((ze = zis.GetNextEntry()) != null)
                    {
                        using (FileStream sw = File.Create(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"), ze.Name)))
                        {
                            byte[] read = new byte[2048];
                            int count = 0;
                            //while ((count = sw.Read(read, 0, read.Length)) > 0)
                            //{
                            //    sw.Write(read, 0, count);
                            //}
                            try
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zis.Read(data, 0, data.Length);
                                    if (size > 0)
                                        sw.Write(data, 0, data.Length);
                                    else
                                        break;
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }
        public void Stop()
        {
            Running = false;
            if ((ProcessingThread != null) && ((ProcessingThread.ThreadState != ThreadState.Aborted) || (ProcessingThread.ThreadState != ThreadState.Stopped)))
                ProcessingThread.Join();
        }
        public void StartOver()
        {
            Stop();
            Start();
        }
        public void Start()
        {
            Running = true;
            IsProcessing = true;
            ProcessingThread = new Thread(Processing);
            ProcessingThread.Start();
        }
        private void Processing()
        {
            //if (!File.Exists(OpenMobile.Path.Combine(OM.Host.DataPath, "OMGPS.db")))
            //    SqliteConnection.CreateFile(OpenMobile.Path.Combine(OM.Host.DataPath, "OMGPS.db"));
            if (StoredData.Get(OMGPS.ibp, "GPS.DL.Processing.Progress") != "")
                ProcessNumber = Convert.ToInt32(StoredData.Get(OMGPS.ibp, "GPS.DL.Processing.Progress"));
            else
                ProcessNumber = 0;
            //if (OMGPS.ProcessingNotification == null)
            //{  //create
            OMGPS.ProcessingNotification = new Notification(OMGPS.ibp, "GPSProcessing", imageItem.NONE.image, imageItem.NONE.image, "Initiating Processing: " + Name, "1");
            if (ProcessNumber > 0)
                OMGPS.ProcessingNotification.Text = "( Attemping To Continue, Please Wait )";
            else
                OMGPS.ProcessingNotification.Text = "Starting, Please Wait";
            OMGPS.theHost.UIHandler.AddNotification(OMGPS.ProcessingNotification);
            //}
            //else
            //{  //update
            //    OMGPS.ProcessingNotification.Header = "Initiating Processing: " + Name;
            //    OMGPS.ProcessingNotification.Text = "";
            //}
            using (SqliteConnection sqlConn = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(OM.Host.DataPath, "OMGPS") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0"))
            {
                //command
                using (SqliteCommand command = sqlConn.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS [GeoNames]([GeoNameID] INTEGER PRIMARY KEY, [Name] TEXT, [ASCIIName] TEXT, [AlternateNames] TEXT, [Latitude] TEXT, [Longitude] TEXT, [Feature Class] TEXT, [Feature Code] TEXT, [Country Code] TEXT, [CC2] TEXT, [Admin1 Code] TEXT, [Admin2 Code] TEXT, [Admin3 Code] TEXT, [Admin4 Code] TEXT, [Population] TEXT, [Elevation] TEXT, [DEM] TEXT, [TimeZone] TEXT, [Modification Date] TEXT);";
                    if (sqlConn.State != System.Data.ConnectionState.Open)
                        sqlConn.Open();
                    command.ExecuteNonQuery();
                    //stream the contents of the file...
                    string newname = Misc.CountryCodeNames(Name.Substring(0, Name.LastIndexOf(".")));
                    if (newname == "")
                        newname = Name.Substring(0, Name.LastIndexOf("."));
                    bool eof = false;
                    string line;
                    string[] linesplit = new string[19];
                    int i = 0;
                    //get total row count
                    int totalLineCount = 0;
                    using (StreamReader sr = new StreamReader(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"), Name.Substring(0, Name.LastIndexOf(".")) + ".txt")))
                    {
                        while (sr.ReadLine() != null)
                        {
                            totalLineCount += 1;
                        }
                    }
                    using (StreamReader sr = new StreamReader(OpenMobile.Path.Combine(OpenMobile.Path.Combine(OM.Host.PluginPath, "OMGPS", "Process"), Name.Substring(0, Name.LastIndexOf(".")) + ".txt")))
                    {
                        /*
                        string newline = sr.ReadToEnd();
                        string[] line = newline.Split(new char[] { '\n', '\r' },StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < line.Length; i++)
                        {
                            linesplit = line[i].Split('\t');
                            command.CommandText = "INSERT OR REPLACE INTO [GeoNames] VALUES ('" + General.escape(linesplit[0]) + "','" + General.escape(linesplit[1]) + "','" + General.escape(linesplit[2]) + "','" + General.escape(linesplit[3]) + "','" + General.escape(linesplit[4]) + "','" + General.escape(linesplit[5]) + "','" + General.escape(linesplit[6]) + "','" + General.escape(linesplit[7]) + "','" + General.escape(linesplit[8]) + "','" + General.escape(linesplit[9]) + "','" + General.escape(linesplit[10]) + "','" + General.escape(linesplit[11]) + "','" + General.escape(linesplit[12]) + "','" + General.escape(linesplit[13]) + "','" + General.escape(linesplit[14]) + "','" + General.escape(linesplit[15]) + "','" + General.escape(linesplit[16]) + "','" + General.escape(linesplit[17]) + "','" + General.escape(linesplit[18]) + "');";
                            command.ExecuteNonQuery();
                            OMGPS.ProcessingNotification.Header = "Processing: " + newname;
                            OMGPS.ProcessingNotification.Text = "( " + i.ToString() + " / " + line.Length.ToString() + " )";
                        }
                        */
                        SqliteTransaction sqltrans = sqlConn.BeginTransaction();
                        command.Transaction = sqltrans;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (sr.EndOfStream)
                                eof = true;
                            if (i >= ProcessNumber)
                            {
                                linesplit = line.Split('\t');
                                if (linesplit.Length == 19)
                                {
                                    if (Running)
                                    {
                                        ProcessNumber = i;
                                        //command.CommandText = "INSERT OR IGNORE INTO [GeoNames] VALUES ('$" + Convert.ToInt32(General.escape(linesplit[0])) + "','" + General.escape(linesplit[1]) + "','" + General.escape(linesplit[2]) + "','" + General.escape(linesplit[3]) + "','" + General.escape(linesplit[4]) + "','" + General.escape(linesplit[5]) + "','" + General.escape(linesplit[6]) + "','" + General.escape(linesplit[7]) + "','" + General.escape(linesplit[8]) + "','" + General.escape(linesplit[9]) + "','" + General.escape(linesplit[10]) + "','" + General.escape(linesplit[11]) + "','" + General.escape(linesplit[12]) + "','" + General.escape(linesplit[13]) + "','" + General.escape(linesplit[14]) + "','" + General.escape(linesplit[15]) + "','" + General.escape(linesplit[16]) + "','" + General.escape(linesplit[17]) + "','" + General.escape(linesplit[18]) + "'); UPDATE [GeoNames] SET [Name]='" + General.escape(linesplit[1]) + "', [ASCIIName]='" + General.escape(linesplit[2]) + "', [AlternateNames]='" + General.escape(linesplit[3]) + "',[Latitude]='" + General.escape(linesplit[4]) + "', [Longitude]='" + General.escape(linesplit[5]) + "', [Feature Class]='" + General.escape(linesplit[6]) + "', [Feature Code]='" + General.escape(linesplit[7]) + "', [Country Code]='" + General.escape(linesplit[8]) + "', [CC2]='" + General.escape(linesplit[9]) + "', [Admin1 Code]='" + General.escape(linesplit[10]) + "', [Admin2 Code]='" + General.escape(linesplit[11]) + "', [Admin3 Code]='" + General.escape(linesplit[12]) + "', [Admin4 Code]='" + General.escape(linesplit[13]) + "', [Population]='" + General.escape(linesplit[14]) + "', [Elevation]='" + General.escape(linesplit[15]) + "', [DEM]='" + General.escape(linesplit[16]) + "', [TimeZone]='" + General.escape(linesplit[17]) + "', [Modification Date]='" + General.escape(linesplit[18]) + "' WHERE GeoNameID='$" + linesplit[0] + "';";
                                        command.CommandText = "INSERT OR REPLACE INTO [GeoNames] VALUES ('" + General.escape(linesplit[0]) + "','" + General.escape(linesplit[1]) + "','" + General.escape(linesplit[2]) + "','" + General.escape(linesplit[3]) + "','" + General.escape(linesplit[4]) + "','" + General.escape(linesplit[5]) + "','" + General.escape(linesplit[6]) + "','" + General.escape(linesplit[7]) + "','" + General.escape(linesplit[8]) + "','" + General.escape(linesplit[9]) + "','" + General.escape(linesplit[10]) + "','" + General.escape(linesplit[11]) + "','" + General.escape(linesplit[12]) + "','" + General.escape(linesplit[13]) + "','" + General.escape(linesplit[14]) + "','" + General.escape(linesplit[15]) + "','" + General.escape(linesplit[16]) + "','" + General.escape(linesplit[17]) + "','" + General.escape(linesplit[18]) + "');";
                                        command.ExecuteNonQuery();
                                        OMGPS.ProcessingNotification.Header = "Processing: " + newname;
                                        OMGPS.ProcessingNotification.Text = "( " + i.ToString() + " / " + totalLineCount + " )";

                                        /*
                                        //if linesplit[0] in table, update else insert
                                        command.CommandText = "SELECT [GeoNameID] FROM GeoNames WHERE [GeoNameID] = '" + linesplit[0] + "'";
                                        if (sqlConn.State != System.Data.ConnectionState.Open)
                                            sqlConn.Open();
                                        using (SqliteDataReader reader = command.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                //update
                                                reader.Close();
                                                command.CommandText = @"UPDATE [GeoNames] SET [Name]='" + General.escape(linesplit[1]) + "', [ASCIIName]='" + General.escape(linesplit[2]) + "', [AlternateNames]='" + General.escape(linesplit[3]) + "',[Latitude]='" + General.escape(linesplit[4]) + "', [Longitude]='" + General.escape(linesplit[5]) + "', [Feature Class]='" + General.escape(linesplit[6]) + "', [Feature Code]='" + General.escape(linesplit[7]) + "', [Country Code]='" + General.escape(linesplit[8]) + "', [CC2]='" + General.escape(linesplit[9]) + "', [Admin1 Code]='" + General.escape(linesplit[10]) + "', [Admin2 Code]='" + General.escape(linesplit[11]) + "', [Admin3 Code]='" + General.escape(linesplit[12]) + "', [Admin4 Code]='" + General.escape(linesplit[13]) + "', [Population]='" + General.escape(linesplit[14]) + "', [Elevation]='" + General.escape(linesplit[15]) + "', [DEM]='" + General.escape(linesplit[16]) + "', [TimeZone]='" + General.escape(linesplit[17]) + "', [Modification Date]='" + General.escape(linesplit[18]) + "' WHERE GeoNameID='" + linesplit[0] + "';";
                                                command.ExecuteNonQuery();
                                            }
                                            else
                                            {
                                                //insert
                                                reader.Close();
                                                command.CommandText = @"INSERT INTO [GeoNames] VALUES ('" + General.escape(linesplit[0]) + "','" + General.escape(linesplit[1]) + "','" + General.escape(linesplit[2]) + "','" + General.escape(linesplit[3]) + "','" + General.escape(linesplit[4]) + "','" + General.escape(linesplit[5]) + "','" + General.escape(linesplit[6]) + "','" + General.escape(linesplit[7]) + "','" + General.escape(linesplit[8]) + "','" + General.escape(linesplit[9]) + "','" + General.escape(linesplit[10]) + "','" + General.escape(linesplit[11]) + "','" + General.escape(linesplit[12]) + "','" + General.escape(linesplit[13]) + "','" + General.escape(linesplit[14]) + "','" + General.escape(linesplit[15]) + "','" + General.escape(linesplit[16]) + "','" + General.escape(linesplit[17]) + "','" + General.escape(linesplit[18]) + "');";
                                                command.ExecuteNonQuery();
                                            }
                                        }
                                        */

                                    }
                                    else
                                        break;
                                }
                            }
                            else
                            {
                                OMGPS.ProcessingNotification.Text = "( Attemping To Continue, Please Wait )";
                            }
                            i += 1;
                        }
                        sqltrans.Commit();

                        //if here, done with processing file
                        if (eof)
                        {
                            //IsProcessing = false;
                            Running = false;
                            eof = false;
                            if (ProcessingFinished != null)
                                ProcessingFinished();
                            //StoredData.Set(OMGPS.ibp, "GPS.DL.Processing.Progress", "0");
                            //ProcessingQueue.processingList.Remove(this);
                        }
                    }
                }
            }
        }
    }

    public class Misc
    {
        public static string CountryCodeNames(string countryCode)
        {
            switch (countryCode.ToUpper())
            {
                case "AD":
                    return "Andorra";
                case "AE":
                    return "United Arab Emirates";
                case "AF":
                    return "Afghanistan";
                case "AG":
                    return "Antigua and Barbuda";
                case "AI":
                    return "Anguilla";
                case "AL":
                    return "Albania";
                case "AM":
                    return "Armenia";
                //case "AN":
                //    return "";
                case "AO":
                    return "Angola";
                case "AQ":
                    return "Antarctica";
                case "AR":
                    return "Argentina";
                case "AS":
                    return "American Samoa";
                case "AT":
                    return "Austria";
                case "AU":
                    return "Australia";
                case "AW":
                    return "Aruba";
                case "AX":
                    return "Åland";
                case "AZ":
                    return "Azerbaijan";
                case "BA":
                    return "Bosnia and Herzegovina";
                case "BB":
                    return "Barbados";
                case "BD":
                    return "Bangladesh";
                case "BE":
                    return "Belgium";
                case "BF":
                    return "Burkina Faso";
                case "BG":
                    return "Bulgaria";
                case "BH":
                    return "Bahrain";
                case "BI":
                    return "Burundi";
                case "BJ":
                    return "Benin";
                case "BL":
                    return "Saint Barthélemy";
                case "BM":
                    return "Bermuda";
                case "BN":
                    return "Brunei";
                case "BO":
                    return "Bolivia";
                case "BQ":
                    return "Bonaire";
                case "BR":
                    return "Brazil";
                case "BS":
                    return "Bahamas";
                case "BT":
                    return "Bhutan";
                case "BV":
                    return "Bouvet Island";
                case "BW":
                    return "Botswana";
                case "BY":
                    return "Belarus";
                case "BZ":
                    return "Belize";
                case "CA":
                    return "Canada";
                case "CC":
                    return "Cocos [Keeling] Islands";
                case "CD":
                    return "Congo";
                case "CF":
                    return "Central African Republic";
                case "CG":
                    return "Republic of the Congo";
                case "CH":
                    return "Switzerland";
                case "CI":
                    return "Ivory Coast";
                case "CK":
                    return "Cook Islands";
                case "CL":
                    return "Chile";
                case "CM":
                    return "Cameroon";
                case "CN":
                    return "China";
                case "CO":
                    return "Colombia";
                case "CR":
                    return "Costa Rica";
                //case "CS":
                //    return "";
                case "CU":
                    return "Cuba";
                case "CV":
                    return "Cape Verde";
                case "CW":
                    return "Curacao";
                case "CX":
                    return "Christmas Island";
                case "CY":
                    return "Cyprus";
                case "CZ":
                    return "Czechia";
                case "DE":
                    return "Germany";
                case "DJ":
                    return "Djibouti";
                case "DK":
                    return "Denmark";
                case "DM":
                    return "Dominica";
                case "DO":
                    return "Dominican Republic";
                case "DZ":
                    return "Algeria";
                case "EC":
                    return "Ecuador";
                case "EE":
                    return "Estonia";
                case "EG":
                    return "Egypt";
                case "EH":
                    return "Western Sahara";
                case "ER":
                    return "Eritrea";
                case "ES":
                    return "Spain";
                case "ET":
                    return "Ethiopia";
                case "FI":
                    return "Finland";
                case "FJ":
                    return "Fiji";
                case "FK":
                    return "Falkland Islands";
                case "FM":
                    return "Micronesia";
                case "FO":
                    return "Faroe Islands";
                case "FR":
                    return "France";
                case "GA":
                    return "Gabon";
                case "GB":
                    return "United Kingdom";
                case "GD":
                    return "Grenada";
                case "GE":
                    return "Georgia";
                case "GF":
                    return "French Guiana";
                case "GG":
                    return "Guernsey";
                case "GH":
                    return "Ghana";
                case "GI":
                    return "Gibraltar";
                case "GL":
                    return "Greenland";
                case "GM":
                    return "Gambia";
                case "GN":
                    return "Guinea";
                case "GP":
                    return "Guadeloupe";
                case "GQ":
                    return "Equatorial Guinea";
                case "GR":
                    return "Greece";
                case "GS":
                    return "South Georgia and the South Sandwich Islands";
                case "GT":
                    return "Guatemala";
                case "GU":
                    return "Guam";
                case "GW":
                    return "Guinea-Bissau";
                case "GY":
                    return "Guyana";
                case "HK":
                    return "Hong Kong";
                case "HM":
                    return "Heard Island and McDonald Islands";
                case "HN":
                    return "Honduras";
                case "HR":
                    return "Croatia";
                case "HT":
                    return "Haiti";
                case "HU":
                    return "Hungary";
                case "ID":
                    return "Indonesia";
                case "IE":
                    return "Ireland";
                case "IL":
                    return "Israel";
                case "IM":
                    return "Isle of Man";
                case "IN":
                    return "India";
                case "IO":
                    return "British Indian Ocean Territory";
                case "IQ":
                    return "Iraq";
                case "IR":
                    return "Iran";
                case "IS":
                    return "Iceland";
                case "IT":
                    return "Italy";
                case "JE":
                    return "Jersey";
                case "JM":
                    return "Jamaica";
                case "JO":
                    return "Jordan";
                case "JP":
                    return "Japan";
                case "KE":
                    return "Kenya";
                case "KG":
                    return "Kyrgyzstan";
                case "KH":
                    return "Cambodia";
                case "KI":
                    return "Kiribati";
                case "KM":
                    return "Comoros";
                case "KN":
                    return "Saint Kitts and Nevis";
                case "KP":
                    return "North Korea";
                case "KR":
                    return "South Korea";
                case "KW":
                    return "Kuwait";
                case "KY":
                    return "Cayman Islands";
                case "KZ":
                    return "Kazakhstan";
                case "LA":
                    return "Laos";
                case "LB":
                    return "Lebanon";
                case "LC":
                    return "Saint Lucia";
                case "LI":
                    return "Liechtenstein";
                case "LK":
                    return "Sri Lanka";
                case "LR":
                    return "Liberia";
                case "LS":
                    return "Lesotho";
                case "LT":
                    return "Lithuania";
                case "LU":
                    return "Luxembourg";
                case "LV":
                    return "Latvia";
                case "LY":
                    return "Libya";
                case "MA":
                    return "Morocco";
                case "MC":
                    return "Monaco";
                case "MD":
                    return "Moldova";
                case "ME":
                    return "Montenegro";
                case "MF":
                    return "Saint Martin";
                case "MG":
                    return "Madagascar";
                case "MH":
                    return "Marshall Islands";
                case "MK":
                    return "Macedonia";
                case "ML":
                    return "Mali";
                case "MM":
                    return "Myanmar [Burma]";
                case "MN":
                    return "Mongolia";
                case "MO":
                    return "Macao";
                case "MP":
                    return "Northern Mariana Islands";
                case "MQ":
                    return "Martinique";
                case "MR":
                    return "Mauritania";
                case "MS":
                    return "Montserrat";
                case "MT":
                    return "Malta";
                case "MU":
                    return "Mauritius";
                case "MV":
                    return "Maldives";
                case "MW":
                    return "Malawi";
                case "MX":
                    return "Mexico";
                case "MY":
                    return "Malaysia";
                case "MZ":
                    return "Mozambique";
                case "NA":
                    return "Namibia";
                case "NC":
                    return "New Caledonia";
                case "NE":
                    return "Niger";
                case "NF":
                    return "Norfolk Island";
                case "NG":
                    return "Nigeria";
                case "NI":
                    return "Nicaragua";
                case "NL":
                    return "Netherlands";
                case "NO":
                    return "Norway";
                case "NP":
                    return "Nepal";
                case "NR":
                    return "Nauru";
                case "NU":
                    return "Niue";
                case "NZ":
                    return "New Zealand";
                case "OM":
                    return "Oman";
                case "PA":
                    return "Panama";
                case "PE":
                    return "Peru";
                case "PF":
                    return "French Polynesia";
                case "PG":
                    return "Papua New Guinea";
                case "PH":
                    return "Philippines";
                case "PK":
                    return "Pakistan";
                case "PL":
                    return "Poland";
                case "PM":
                    return "Saint Pierre and Miquelon";
                case "PN":
                    return "Pitcairn Islands";
                case "PR":
                    return "Puerto Rico";
                case "PS":
                    return "Palestine";
                case "PT":
                    return "Portugal";
                case "PW":
                    return "Palau";
                case "PY":
                    return "Paraguay";
                case "QA":
                    return "Qatar";
                case "RE":
                    return "Réunion";
                case "RO":
                    return "Romania";
                case "RS":
                    return "Serbia";
                case "RU":
                    return "Russia";
                case "RW":
                    return "Rwanda";
                case "SA":
                    return "Saudi Arabia";
                case "SB":
                    return "Solomon Islands";
                case "SC":
                    return "Seychelles";
                case "SD":
                    return "Sudan";
                case "SE":
                    return "Sweden";
                case "SG":
                    return "Singapore";
                case "SH":
                    return "Saint Helena";
                case "SI":
                    return "Slovenia";
                case "SJ":
                    return "Svalbard and Jan Mayen";
                case "SK":
                    return "Slovakia";
                case "SL":
                    return "Sierra Leone";
                case "SM":
                    return "San Marino";
                case "SN":
                    return "Senegal";
                case "SO":
                    return "Somalia";
                case "SR":
                    return "Suriname";
                case "a>":
                    return "South Sudan";
                case "ST":
                    return "São Tomé and Príncipe";
                case "SV":
                    return "El Salvador";
                case "SX":
                    return "Sint Maarten";
                case "SY":
                    return "Syria";
                case "SZ":
                    return "Swaziland";
                case "TC":
                    return "Turks and Caicos Islands";
                case "TD":
                    return "Chad";
                case "TF":
                    return "French Southern Territories";
                case "TG":
                    return "Togo";
                case "TH":
                    return "Thailand";
                case "TJ":
                    return "Tajikistan";
                case "TK":
                    return "Tokelau";
                case "TL":
                    return "East Timor";
                case "TM":
                    return "Turkmenistan";
                case "TN":
                    return "Tunisia";
                case "TO":
                    return "Tonga";
                case "TR":
                    return "Turkey";
                case "TT":
                    return "Trinidad and Tobago";
                case "TV":
                    return "Tuvalu";
                case "TW":
                    return "Taiwan";
                case "TZ":
                    return "Tanzania";
                case "UA":
                    return "Ukraine";
                case "UG":
                    return "Uganda";
                case "UM":
                    return "U.S. Minor Outlying Islands";
                case "US":
                    return "United States";
                case "UY":
                    return "Uruguay";
                case "UZ":
                    return "Uzbekistan";
                case "VA":
                    return "Vatican City";
                case "VC":
                    return "Saint Vincent and the Grenadines";
                case "VE":
                    return "Venezuela";
                case "VG":
                    return "British Virgin Islands";
                case "VI":
                    return "U.S. Virgin Islands";
                case "VN":
                    return "Vietnam";
                case "VU":
                    return "Vanuatu";
                case "WF":
                    return "Wallis and Futuna";
                case "WS":
                    return "Samoa";
                case "XK":
                    return "Kosovo";
                case "YE":
                    return "Yemen";
                case "YT":
                    return "Mayotte";
                case "ZA":
                    return "South Africa";
                case "ZM":
                    return "Zambia";
                case "ZW":
                    return "Zimbabwe";
                default:
                    return "";
            }
        }
    }

    //* The following class below is free to use from dotnetperls...//
    public class AlphanumComparatorFast : IComparer
    {
        public int Compare(object x, object y)
        {
            string s1 = x as string;
            if (s1 == null)
            {
                return 0;
            }
            string s2 = y as string;
            if (s2 == null)
            {
                return 0;
            }

            int len1 = s1.Length;
            int len2 = s2.Length;
            int marker1 = 0;
            int marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                char ch1 = s1[marker1];
                char ch2 = s2[marker2];

                // Some buffers we can build up characters in for each chunk.
                char[] space1 = new char[len1];
                int loc1 = 0;
                char[] space2 = new char[len2];
                int loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = s1[marker1];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = s2[marker2];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                string str1 = new string(space1);
                string str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    int thisNumericChunk = int.Parse(str1);
                    int thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }
            return len1 - len2;
        }

        public static List<string[]> Sorted(List<string[]> tempList)
        {
            string[] toSort = new string[tempList.Count];
            for (int i = 0; i < tempList.Count; i++)
                toSort[i] = tempList[i][4];
            Array.Sort(toSort, new AlphanumComparatorFast());
            List<string[]> newTempList = new List<string[]>();
            for (int i = 0; i < toSort.Length; i++)
            {
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (tempList[j][4] == toSort[i])
                    {
                        newTempList.Add(tempList[j]);
                        break;
                    }
                }
            }
            return newTempList;
        }
    }

}