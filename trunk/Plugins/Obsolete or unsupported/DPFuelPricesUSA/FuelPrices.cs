using System;
using OpenMobile;
using OpenMobile.Plugin;
using System.Net;
using OpenMobile.Data;

namespace DPFuelPricesUSA
{
    public class FuelPrices:IDataProvider
    {
        IPluginHost theHost;
        int status;

        private void getPrices()
        {
            status = 2;
            if (getInfo())
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("Plugins.DPFuelPricesUSA.LastUpdate", DateTime.Now.ToString("s"));
                status = 1;
                theHost.execute(eFunction.dataUpdated, "DPFuelPricesUSA");
            }
            else
                status = -1;
        }
        private bool getInfo()
        {
            string loc;
            using (PluginSettings settings = new PluginSettings())
                loc = settings.getSetting("Data.DefaultLocation");
            if (loc == "")
                return false;
            loc = OpenMobile.helperFunctions.General.lookupLocation(loc).Zip;
            if (loc == "")
                return false;
            WebClient client = new WebClient();
            string data;
            try
            {
                data = client.DownloadString("http://www.motortrend.com/gas_prices/34/" + loc + "/index.html");
            }
            catch (Exception)
            {
                return false;
            }
            int x = data.IndexOf("w800 brdr1 gp_data_tbl");
            if (x < 0)
                return false;
            data = data.Substring(x);
            x = data.IndexOf("b clr20'>") + 9;
            int end = data.IndexOf("</table>");
            int y;
            int z;
            GasStations stations = new GasStations();
            if (!stations.beginWrite())
                return false;
            while (x >= 0)
            {
                GasStations.gasStation station = new GasStations.gasStation();
                station.postalCode = loc;
                y = data.IndexOf('<', x);
                station.name = data.Substring(x, y - x).Trim();
                x = data.IndexOf("pad brdr1_b\">",x) + 13;
                if (x == 12)
                    return true;
                y = data.IndexOf("</", x);
                station.location = data.Substring(x, y - x).Trim().Replace("<br>", "\n");
                y = data.IndexOf("'lft sub' >",x) + 11;
                z = data.IndexOf("pad brdr1_b\">",x) + 13;
                if ((y > z)||(y==10))
                    x = z;
                else
                    x = y;
                y = data.IndexOf('<', x);
                station.priceRegular = data.Substring(x, y - x).Trim();
                y = data.IndexOf("'lft sub' >", x) + 11;
                z = data.IndexOf("pad brdr1_b\">", x) + 13;
                if ((y > z) || (y == 10))
                    x = z;
                else
                    x = y;
                y = data.IndexOf('<', x);
                station.pricePlus = data.Substring(x, y - x).Trim();
                y = data.IndexOf("'lft sub' >", x) + 11;
                z = data.IndexOf("pad brdr1_b\">", x) + 13;
                if ((y > z) || (y == 10))
                    x = z;
                else
                    x = y;
                y = data.IndexOf('<', x);
                station.pricePremium = data.Substring(x, y - x).Trim();
                y = data.IndexOf("'lft sub' >", x) + 11;
                z = data.IndexOf("pad brdr1_b\">", x) + 13;
                if ((y > z) || (y == 10))
                    x = z;
                else
                    x = y;
                y = data.IndexOf('<', x);
                station.priceDiesel = data.Substring(x, y - x);
                station.dateAdded = DateTime.Now;
                if (station.priceRegular == "N/A")
                    station.priceRegular = null;
                if (station.pricePlus == "N/A")
                    station.pricePlus = null;
                if (station.pricePremium == "N/A")
                    station.pricePremium = null;
                if (station.priceDiesel == "N/A")
                    station.priceDiesel = null;
                stations.writeNext(station);
                x = data.IndexOf("b clr20'>", x) + 9;
            }
            return true;
        }

        public bool refreshData()
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                if ((DateTime.Now - lastUpdated).TotalMinutes > 60)
                {
                    status = 0;
                    OpenMobile.Threading.TaskManager.QueueTask(getPrices, OpenMobile.ePriority.Normal, "Update Fuel Prices");
                    return true;
                }
            }
            status = -1;
            theHost.execute(eFunction.dataUpdated, "DPFuelPricesUSA");
            return false;
        }

        public bool refreshData(string arg)
        {
            return refreshData();
        }

        public bool refreshData(string arg1, string arg2)
        {
            return refreshData();
        }

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
                    if (DateTime.TryParse(s.getSetting("Plugins.DPFuelPricesUSA.LastUpdate"), out ret))
                        return ret;
                    else
                        return DateTime.MinValue;
            }
        }

        public string pluginType()
        {
            return "FuelPrices";
        }
        #region base
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            return eLoadStatus.LoadSuccessful;
        }

        void host_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
                refreshData();
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if ((function == eFunction.connectedToInternet) || (function == eFunction.hourChanged))
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
            get { return "DPFuelPricesUSA"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "US Fuel Prices"; }
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
        #endregion
    }
}
