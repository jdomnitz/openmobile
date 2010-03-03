using System;
using OpenMobile.Plugin;
using OpenMobile.Data;
using System.Collections.Generic;
using System.Xml;
using OpenMobile;
using System.Globalization;

namespace DPGWeather
{
    public class DPGWeather:IDataProvider
    {

        #region IDataProvider Members

        public bool refreshData()
        {
            string loc;
            using (PluginSettings settings = new PluginSettings())
                loc = settings.getSetting("Data.DefaultLocation");
            if (loc != "")
                return refreshData(loc);
            return false;
        }

        public static void processItems()
        {
            status = 2;
            try
            {
                List<OpenMobile.Data.Weather.weather> items = getItems(updateLocation);
                if (items.Count < 4)
                {
                    status = -1;
                    return;
                }
                using (OpenMobile.Data.Weather w = new OpenMobile.Data.Weather())
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[0].date == items[1].date)
                        {
                            if (i == 1)
                                continue;
                            Weather.weather current = items[0];
                            current.highTemp = items[1].highTemp;
                            current.lowTemp = items[1].lowTemp;
                            items[0] = current;
                        }
                        w.writeWeather(items[i]);
                    }
                }
                status = 1;
            }
            catch (Exception) { status = -1; }
        }

        static List<OpenMobile.Data.Weather.weather> getItems(string location)
        {
            List<OpenMobile.Data.Weather.weather> lst = new List<OpenMobile.Data.Weather.weather>();
            XmlDocument reader = new XmlDocument();
            reader.Load("http://www.google.com/ig/api?weather=" + location);
            OpenMobile.Data.Weather.weather ret;
            foreach (XmlNode n in reader.SelectSingleNode("/xml_api_reply/weather").ChildNodes)
            {
                ret = new OpenMobile.Data.Weather.weather();
                switch (n.Name)
                {
                    case "current_conditions":
                        RegionInfo regionInfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
                        if (regionInfo.IsMetric==true)
                            ret.temp = float.Parse(n.ChildNodes[2].Attributes[0].Value);
                        else
                            ret.temp = float.Parse(n.ChildNodes[1].Attributes[0].Value);
                        ret.humidity = int.Parse(n.ChildNodes[3].Attributes[0].Value.Replace("Humidity: ", "").Replace("%", ""));
                        parseWind(n.ChildNodes[5].Attributes[0].Value.Replace("Wind: ",""),ref ret);
                        ret.conditions = parseCondition(n.ChildNodes[0].Attributes[0].Value);
                        ret.date = DateTime.Today;
                        break;
                    case "forecast_conditions":
                        ret.lowTemp = float.Parse(n.ChildNodes[1].Attributes[0].Value);
                        ret.highTemp= float.Parse(n.ChildNodes[2].Attributes[0].Value);
                        ret.conditions = parseCondition(n.ChildNodes[4].Attributes[0].Value);
                        ret.date = getDate(n.ChildNodes[0].Attributes[0].Value);
                        break;
                    default:
                        continue;
                }
                ret.contrib = "Google";
                ret.location = location;
                lst.Add(ret);
            }
            return lst;
        }

        private static DateTime getDate(string p)
        {
            for (int i = 0; i < 7; i++)
                if (DateTime.Today.AddDays(i).ToString("ddd") == p)
                    return DateTime.Today.AddDays(i);
            return DateTime.MinValue;
        }

        private static Weather.weatherConditions parseCondition(string p)
        {
            switch (p)
            {
                case "Clear":
                    return Weather.weatherConditions.Clear;
                case "Cloudy":
                    return Weather.weatherConditions.Cloudy;
                case "Fog":
                case "Haze":
                    return Weather.weatherConditions.Foggy;
                case "Light Rain":
                case "Chance of Showers":
                case "Rain":
                case "Rain Showers":
                case "Showers":
                case "Scattered Showers":
                    return Weather.weatherConditions.Showers;
                case "Mostly Cloudy":
                case "Overcast":
                case "Partly Cloudy":
                case "Partly Sunny":
                    return Weather.weatherConditions.PartlyCloudy;
                case "Thunderstorm":
                case "Chance of Storm":
                    return Weather.weatherConditions.TStrorms;
                case "Sunny":
                    return Weather.weatherConditions.Sunny;
                case "Mostly Sunny":
                    return Weather.weatherConditions.MostlySunny;
                case "Snow":
                    return Weather.weatherConditions.Snow;
                case "Windy":
                    return Weather.weatherConditions.Windy;
                case "Snow Showers":
                case "Freezing Rain":
                    return Weather.weatherConditions.RainSnowMix;
                default:
                    return Weather.weatherConditions.NotSet;
            }
        }

        private static void parseWind(string p, ref Weather.weather ret)
        {
            string[] arg = p.Split(' ');
            if (arg.Length!=4)
                return;
            switch (arg[0])
            {
                case "N":
                    ret.windDirection = Weather.direction.N;
                    break;
                case "W":
                    ret.windDirection = Weather.direction.W;
                    break;
                case "E":
                    ret.windDirection = Weather.direction.E;
                    break;
                case "S":
                    ret.windDirection = Weather.direction.S;
                    break;
                case "NW":
                    ret.windDirection = Weather.direction.NW;
                    break;
                case "SW":
                    ret.windDirection = Weather.direction.SW;
                    break;
                case "NE":
                    ret.windDirection = Weather.direction.NE;
                    break;
                case "SE":
                    ret.windDirection = Weather.direction.SE;
                    break;
            }
            ret.windSpeed = int.Parse(arg[2]);
        }

        private static string updateLocation = "";
        public bool refreshData(string arg)
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                status = 0;
                updateLocation = arg;
                OpenMobile.Threading.TaskManager.QueueTask(processItems, OpenMobile.priority.MediumHigh);
                return true;
            }
            else
            {
                status = -1;
                return false;
            }
        }

        public bool refreshData(string arg1, string arg2)
        {
            return false;
        }
        private static int status=0;
        public int updaterStatus()
        {
            return status;
        }

        public string pluginType()
        {
            return "Weather";
        }

        #endregion

        #region IBasePlugin Members

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
            get { return "DPGWeather"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Google Weather Provider"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.connectedToInternet)
            {
                string loc;
                using (PluginSettings settings = new PluginSettings())
                    loc = settings.getSetting("Data.DefaultLocation");
                if (loc != "")
                    refreshData(loc);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
