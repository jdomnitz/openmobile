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
using System.Xml;
using OpenMobile.Data;
using OpenMobile.Plugin;

namespace DPYWeather
{
    public class Weather:IDataProvider
    {
        #region IDataProvider Members
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == OpenMobile.eFunction.connectedToInternet)
                refreshData();
        }

        private static string getWoeid(string location)
        {
            XmlDocument reader = new XmlDocument();
            reader.Load("http://where.yahooapis.com/v1/places.q(" + location + ")?appid=" + AppID);
            XmlNode n = reader["places"].ChildNodes[0].ChildNodes[0];
            return n.InnerText;
        }
        const string AppID = "B9FntbPV34FKJ2nSuDXA8lrU0aK3JVC_LGdraubEBCVtEJG8RTKn5zzf5GDqVHbzHa7r";
        static List<OpenMobile.Data.Weather.weather> getItems(string woeid)
        {
            List<OpenMobile.Data.Weather.weather> lst = new List<OpenMobile.Data.Weather.weather>();
            XmlDocument reader=new XmlDocument();
            reader.Load("http://weather.yahooapis.com/forecastrss?w="+woeid+"&u=c");
            OpenMobile.Data.Weather.weather ret = new OpenMobile.Data.Weather.weather();
            lst.Add(ret);
            foreach(XmlNode n in reader.SelectSingleNode("/rss/channel").ChildNodes)
            {
                switch(n.Name)
                {
                    case "yweather:wind":
                        ret.feelsLike = float.Parse(n.Attributes[0].Value);
                        ret.windDirection = getDirection(int.Parse(n.Attributes[1].Value));
                        ret.windSpeed = float.Parse(n.Attributes[2].Value);
                        break;
                    case "yweather:atmosphere":
                        ret.humidity = int.Parse(n.Attributes[0].Value);
                        break;
                    case "item":
                        foreach(XmlNode node in n.ChildNodes){
                            switch (node.Name)
                            {
                                case "yweather:condition":
                                    ret.conditions = parseCondition(node.Attributes[1].Value);
                                    ret.temp = float.Parse(node.Attributes[2].Value);
                                    break;
                                case "yweather:forecast":
                                    OpenMobile.Data.Weather.weather forcast = new OpenMobile.Data.Weather.weather();
                                    forcast.date = DateTime.Parse(node.Attributes[1].Value);
                                    forcast.lowTemp = float.Parse(node.Attributes[2].Value);
                                    forcast.highTemp = float.Parse(node.Attributes[3].Value);
                                    forcast.conditions = parseCondition(node.Attributes[5].Value);
                                    forcast.location = updateLocation;
                                    forcast.contrib = "Yahoo";
                                    lst.Add(forcast);
                                    break;
                                }
                            }
                        break;
                }
            }
            ret.location = updateLocation;
            ret.contrib = "Yahoo";
            ret.date = DateTime.Today;
            lst[0] = ret;
            return lst;
        }
        private static OpenMobile.Data.Weather.direction getDirection(int p)
        {
            if (p < 23)
                return OpenMobile.Data.Weather.direction.N;
            if (p < 68)
                return OpenMobile.Data.Weather.direction.NE;
            if (p < 113)
                return OpenMobile.Data.Weather.direction.E;
            if (p < 158)
                return OpenMobile.Data.Weather.direction.SE;
            if (p < 203)
                return OpenMobile.Data.Weather.direction.S;
            if (p < 248)
                return OpenMobile.Data.Weather.direction.SW;
            if (p < 293)
                return OpenMobile.Data.Weather.direction.W;
            if (p < 338)
                return OpenMobile.Data.Weather.direction.NW;
            return OpenMobile.Data.Weather.direction.N;
        }
        private static OpenMobile.Data.Weather.weatherConditions parseCondition(string c)
        {
            switch (c)
            {
                case "0":
                    return OpenMobile.Data.Weather.weatherConditions.Tornado;
                case "1":
                    return OpenMobile.Data.Weather.weatherConditions.TropicalStorm;
                case "2":
                    return OpenMobile.Data.Weather.weatherConditions.Hurricane;
                case "3":
                    return OpenMobile.Data.Weather.weatherConditions.SevereTStorms;
                case "4":
                case "45":
                    return OpenMobile.Data.Weather.weatherConditions.TStrorms;
                case "5":
                case "6":
                case "7":
                case "14":
                case "18":
                case "42":
                case "46":
                    return OpenMobile.Data.Weather.weatherConditions.RainSnowMix;
                case "8":
                case "10":
                case "35":
                    return OpenMobile.Data.Weather.weatherConditions.FreezingRain;
                case "11":
                case "12":
                case "9":
                case "40":
                case "47":
                    return OpenMobile.Data.Weather.weatherConditions.Showers;
                case "13":
                case "15":
                case "16":
                case "41":
                case "43":
                    return OpenMobile.Data.Weather.weatherConditions.Snow;
                case "17":
                    return OpenMobile.Data.Weather.weatherConditions.Hail;
                case "25":
                case "31":
                case "33":
                    return OpenMobile.Data.Weather.weatherConditions.Clear;
                case "27":
                case "28":
                case "29":
                case "30":
                case "44":
                    return OpenMobile.Data.Weather.weatherConditions.PartlyCloudy;
                case "32":
                case "34":
                case "36":
                    return OpenMobile.Data.Weather.weatherConditions.Sunny;
                case "37":
                case "38":
                case "39":
                    return OpenMobile.Data.Weather.weatherConditions.ScatteredTStorms;
                case "26":
                    return OpenMobile.Data.Weather.weatherConditions.Cloudy;
                case "23":
                case "24":
                    return OpenMobile.Data.Weather.weatherConditions.Windy;
                default:
                    return OpenMobile.Data.Weather.weatherConditions.NotSet;
            }
        }

        public string pluginType()
        {
            return "Weather";
        }

        public bool refreshData(string arg1, string arg2)
        {
            return false;
        }

        public static void processItems()
        {
            status = 2;
            try
            {
                List<OpenMobile.Data.Weather.weather> items = getItems(getWoeid(updateLocation));
                if (items.Count < 3)
                {
                    status = -1;
                    return;
                }
                OpenMobile.Data.Weather.weather current = items[0];
                if (items[1].date == DateTime.Today)
                {
                    current.date = items[1].date;
                    current.highTemp = items[1].highTemp;
                    current.lowTemp = items[1].lowTemp;
                }
                else
                {
                    current.date = items[2].date;
                    current.highTemp = items[2].highTemp;
                    current.lowTemp = items[2].lowTemp;
                }
                using (OpenMobile.Data.Weather w = new OpenMobile.Data.Weather())
                {
                    w.writeWeather(current);
                    if (items[1].date == DateTime.Today)
                        w.writeWeather(items[2]);
                    if (items.Count>=4)
                        w.writeWeather(items[3]);
                }
                status = 1;
                using (PluginSettings setting = new PluginSettings())
                    setting.setSetting("Plugins.DPYWeather.LastUpdate", DateTime.Now.ToString());
            }
            catch (Exception) { status = -1; }
        }
        private static string updateLocation = "";
        public bool refreshData(string arg)
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {

                using (PluginSettings setting = new PluginSettings())
                {
                    DateTime lastUp = new DateTime();
                    if (DateTime.TryParse(setting.getSetting("Plugins.DPYWeather.LastUpdate"), out lastUp) == true)
                    {
                        if ((DateTime.Now - lastUp) < TimeSpan.FromMinutes(30))
                            return false;
                    }
                }
                status = 0;
                updateLocation = arg;
                OpenMobile.Threading.TaskManager.QueueTask(processItems,OpenMobile.priority.MediumHigh);
                return true;
            }
            else
            {
                status = -1;
                return false;
            }
        }

        public bool refreshData()
        {
            string loc;
            using (PluginSettings settings = new PluginSettings())
                loc = settings.getSetting("Data.DefaultLocation");
            if (loc != "")
                return refreshData(loc);
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
        #endregion

        #region IBasePlugin Members

        public string authorEmail
        {
            get { return ""; }
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public string pluginDescription
        {
            get { return "Provides weather for a given location"; }
        }

        public string pluginName
        {
            get { return "DPYWeather"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
