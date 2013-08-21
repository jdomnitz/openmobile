using System;
using OpenMobile.Plugin;
using OpenMobile;
using OpenMobile.Data;
using System.Xml;
using System.Collections.Generic;

namespace DPYTraffic
{
    public class Traffic:IDataProvider
    {
        public bool refreshData()
        {
            string loc;
            using (PluginSettings settings = new PluginSettings())
                loc = settings.getSetting("Data.DefaultLocation");
            if (loc != "")
                return refreshData(loc);
            status = -1;
            return false;
        }
        private static string updateLocation = "";
        public bool refreshData(string arg)
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                if ((DateTime.Now - lastUpdated) < TimeSpan.FromMinutes(30))
                {
                    status = 1;
                    return false;
                }
                status = 0;
                updateLocation = arg;
                OpenMobile.Threading.TaskManager.QueueTask(processItems, OpenMobile.ePriority.MediumHigh, "Sync Traffic");
                return true;
            }
            else
            {
                status = -1;
                return false;
            }
        }

        private void processItems()
        {
            status = 2;
            try
            {
                XmlDocument reader = new XmlDocument();
                reader.Load(@"http://local.yahooapis.com/MapsService/V1/trafficData?appid=" + AppID + "&location=" + updateLocation);
                OpenMobile.Data.Traffic storage = new OpenMobile.Data.Traffic();
                float val;
                foreach (XmlNode node in reader.DocumentElement.ChildNodes)
                {
                    if (node.Name == "Result")
                    {
                        TrafficItem current = new TrafficItem();
                        current.Location = new Location();
                        XmlNode type = node.Attributes["type"];
                        if ((type != null) && (type.InnerText == "construction"))
                            current.Type = eTrafficType.Construction;
                        foreach (XmlNode item in node.ChildNodes)
                        {
                            switch (item.Name)
                            {
                                case "Title":
                                    current.Title = item.InnerText.Replace(",", "");
                                    current.Location.Name = parseLocation(item.InnerText);
                                    break;
                                case "Description":
                                    current.Description = item.InnerText;
                                    break;
                                case "Latitude":
                                    float.TryParse(item.InnerText, out val);
                                    current.Location.Latitude = val;
                                    break;
                                case "Longitude":
                                    float.TryParse(item.InnerText, out val);
                                    current.Location.Longitude = val;
                                    break;
                                case "Direction":
                                    current.Direction = parseDirection(item.InnerText);
                                    break;
                                case "Severity":
                                    current.severity = int.Parse(item.InnerText);
                                    break;
                                case "ReportDate":
                                    current.Start = parseDate(item.InnerText);
                                    break;
                                case "EndDate":
                                    current.End = parseDate(item.InnerText);
                                    break;
                            }
                        }
                        if (current.Type == eTrafficType.Unknown)
                        {
                            string title = current.Title.ToLower();
                            if (title.Contains("accident"))
                                current.Type = eTrafficType.Accident;
                            else if (title.Contains("lane closed"))
                                current.Type = eTrafficType.LaneClosure;
                            else if (title.Contains("closed"))
                                current.Type = eTrafficType.RoadClosure;
                        }
                        storage.writeTraffic(current);
                    }
                }
                status = 1;
                using (PluginSettings setting = new PluginSettings())
                    setting.setSetting("Plugins.DPYTraffic.LastUpdate", DateTime.Now.ToString());
                theHost.execute(eFunction.dataUpdated, "DPYTraffic");
            }
            catch (Exception)
            {
                status = -1;
                theHost.execute(eFunction.dataUpdated, "DPYTraffic");
            }
        }

        private string parseLocation(string p)
        {
            if (!p.Contains(","))
                return p;
            p = p.Substring(p.IndexOf(',')+1);
            p = p.Replace(" on ", "");
            return p;
        }

        private DateTime parseDate(string p)
        {
            double ticks;
            if (!double.TryParse(p, out ticks))
                return DateTime.MinValue;
            return new DateTime(1970,1,1,0,0,0).AddSeconds(ticks);
        }

        private eDirection parseDirection(string p)
        {
            switch (p)
            {
                case "WB":
                    return eDirection.W;
                case "EB":
                    return eDirection.E;
                case "SB":
                    return eDirection.S;
                case "NB":
                    return eDirection.N;
                default:
                    return eDirection.NotSet;
            }
        }

        public bool refreshData(string arg1, string arg2)
        {
            return false;
        }
        private static int status = 0;
        //-1 = Failed
        //0  = No Update
        //1=Update Succeessful
        //2=Update in progress
        public int updaterStatus()
        {
            return status;
        }

        public DateTime lastUpdated
        {
            get
            {
                DateTime ret;
                using (PluginSettings s = new PluginSettings())
                    if (DateTime.TryParse(s.getSetting("Plugins.DPYTraffic.LastUpdate"), out ret))
                        return ret;
                    else
                        return DateTime.MinValue;
            }
        }
        
        public string pluginType()
        {
            return "Traffic";
        }
        const string AppID = "B9FntbPV34FKJ2nSuDXA8lrU0aK3JVC_LGdraubEBCVtEJG8RTKn5zzf5GDqVHbzHa7r";
        IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
                refreshData();
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if ((function == eFunction.hourChanged) || (function == eFunction.connectedToInternet))
                refreshData();
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
            get { return "DPYTraffic"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Yahoo Traffic"; }
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
            GC.SuppressFinalize(this);
        }
    }
}
