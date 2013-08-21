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
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OpenMobile;
using OpenMobile.Data;
using Mono.Data.Sqlite;
using OpenMobile.Graphics;

namespace OMDSWeather
{
    public class Class1 : IBasePlugin, IDataSource
    {
        public delegate void UpdatedSearchedWeatherEventHandler(Dictionary<int, Dictionary<string, string>> polledsearchedweather);
        public static event UpdatedSearchedWeatherEventHandler UpdatedSearchedWeather;
        //Dictionary<int, string> polling;
        Dictionary<int, Dictionary<string, string>> polledSearchedWeather;
        static IPluginHost theHost;
        //PluginSettings ps;
        Dictionary<int, Dictionary<string, string>> polledWeather;
        Dictionary<int, string> searchString;
        Settings settings;
        //System.Threading.Timer polltmr;
        System.Diagnostics.Stopwatch pollsw;
        private SqliteConnection sqlConn;
        Thread polling;
        private bool stillpolling;
        bool refreshQueue;
        bool manualRefresh;

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            WeatherInfo.ibp = this;
            searchString = new Dictionary<int, string>();
            //polling = new Dictionary<int, string>();
            polledSearchedWeather = new Dictionary<int, Dictionary<string, string>>();
            polledWeather = new Dictionary<int, Dictionary<string, string>>();
            //polltmr = new System.Threading.Timer(new TimerCallback(polltmr_tick),null,System.Threading.Timeout.Infinite,Convert.ToInt32(StoredData.Get(this, "OMDSWeather.RefreshRate")) * 60 * 1000);
            pollsw = new System.Diagnostics.Stopwatch();
            stillpolling = true;
            refreshQueue = false;
            manualRefresh = false;

            polling = new Thread(poller);
            polling.Start();
            //tmr = new System.Timers.Timer[theHost.ScreenCount];

            //create the dataSources for searching weather
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "SearchedWeather", "SearchWeather", 0, DataSource.DataTypes.raw, SearchWeather, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "SearchedWeather", "CurrentWeather", 0, DataSource.DataTypes.raw, CurrentWeather, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.LastUpdate", DataSource.DataTypes.raw, "Date/Time of last update"), "");
            //current
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.Area", DataSource.DataTypes.raw, "Temp for the current position"), "Current Area");
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.Temp", DataSource.DataTypes.raw, "Temp for the current position"), "");
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.Image", DataSource.DataTypes.raw, "Image for the current position"), imageItem.NONE);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.Description", DataSource.DataTypes.raw, "Description for the current position"), "");
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Visible", "Forecast", 0, DataSource.DataTypes.raw, VisibleForecast, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Visible", "Locations", 0, DataSource.DataTypes.raw, VisibleLocations, ""));
		BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Visible", "SearchProgress", 0, DataSource.DataTypes.raw, VisibleSearchProgress, ""));
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                //searched
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.LastUpdate", DataSource.DataTypes.raw, "Date/Time of last update"), "");
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.Area", DataSource.DataTypes.raw, "Name for the searched/last searched area found"), "Click To Search Weather");
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.Temp", DataSource.DataTypes.raw, "Temp for the searched/last searched position per screen"), "");
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.Image", DataSource.DataTypes.raw, "Image for the searched/last searched position per screen"), imageItem.NONE);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.Description", DataSource.DataTypes.raw, "Description for the searched/last searched position per screen"), "");
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.LocationTextBox", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.LocationSearchButton", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.LocationMultipleAreaList", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.LocationLabel", DataSource.DataTypes.binary, ""), false);
		BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressImage", DataSource.DataTypes.binary, ""), false);
		BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressLabel", DataSource.DataTypes.binary, ""), false);
		BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressBackground", DataSource.DataTypes.binary, ""), false);
                for (int j = 1; j < 6; j++)
                {
                    //create all the forecast sources per screen...
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastDay" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.ForecastDay" + j.ToString(), DataSource.DataTypes.binary, ""), false);
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastImage" + j.ToString(), DataSource.DataTypes.raw, ""), imageItem.NONE);
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.ForecastImage" + j.ToString(), DataSource.DataTypes.binary, ""), false);
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastDescription" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.ForecastDescription" + j.ToString(), DataSource.DataTypes.binary, ""), false);
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastHigh" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.ForecastHigh" + j.ToString(), DataSource.DataTypes.binary, ""), false);
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastLow" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.ForecastLow" + j.ToString(), DataSource.DataTypes.binary, ""), false);
                    if (i == 0)
                    {
                        BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.ForecastDay" + j.ToString(), DataSource.DataTypes.raw, ""), "");

                        BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.ForecastImage" + j.ToString(), DataSource.DataTypes.raw, ""), imageItem.NONE);

                        BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.ForecastDescription" + j.ToString(), DataSource.DataTypes.raw, ""), "");

                        BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.ForecastHigh" + j.ToString(), DataSource.DataTypes.raw, ""), "");

                        BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Current", "Current.ForecastLow" + j.ToString(), DataSource.DataTypes.raw, ""), "");

                    }
                }
                polledSearchedWeather.Add(i, new Dictionary<string, string>());
            }

            //power event
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);

            // Initialize default data
            StoredData.SetDefaultValue(this, "OMDSWeather.RefreshRate", 5);

            tmr = new OpenMobile.Timer[OM.Host.ScreenCount];
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Refresh", "All", RefreshWeather, 0, false, "Refreshes Searched and Current Location Weather"));

            return eLoadStatus.LoadSuccessful;
        }

        private object RefreshWeather(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            manualRefresh = true;
            return null;
        }

        void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            switch (function)
            {
                //case eFunction.connectedToInternet:
                //    if (refreshQueue)
                //    {
                //        refreshQueue = false;
                //        manualRefresh = true;
                //    }
                //    break;
                //case eFunction.disconnectedFromInternet:
                //    break;
                case eFunction.closeProgram:
                    Killing();
                    break;
            }
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
            if (!stillpolling)
            {
                stillpolling = true;
                polling = new Thread(poller);
                polling.Start();
            }
        }

        private void Killing()
        {
            stillpolling = false;
            if (polling != null)
                polling.Join();

            if (sqlConn != null)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                sqlConn.Dispose();
            }
        }
        int refreshRate;
        private void poller()
        {
            //attempt updates of all polledSearchWeather...
            refreshRate = StoredData.GetInt(this, "OMDSWeather.RefreshRate") * 1000 * 60;
            pollsw.Start();
            while (stillpolling)
            {
                if ((pollsw.ElapsedMilliseconds >= refreshRate) || (manualRefresh))
                {
                    manualRefresh = false;
                    for(int i=0;i<theHost.ScreenCount;i++)
                        VisibleSearchProgress(true, i);
                    if (OM.Host.InternetAccess)
                    {

                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            if (searchString.Keys.Contains(i))
                            {
                                //perform update
                                
                                polledSearchedWeather[i] = WeatherInfo.SearchArea(searchString[i].ToString());
                                if (polledSearchedWeather[i].Keys.ElementAt(0) == "AreaFound")
                                {
                                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + i.ToString() + ".Weather.Searched.Area", polledSearchedWeather[i]["AreaFound"] + "\n(Click To Search Weather)");
                                    WeatherInfo.GetSearchedTemp(GetSearchHTML(i), i);
                                    WeatherInfo.GetSearchedImage(GetSearchHTML(i), i);
                                    WeatherInfo.GetSearchedDescription(GetSearchHTML(i), i);
                                    WeatherInfo.GetSearchedForecast(GetForecastHTML(i), i);
                                }
                                else
                                {
                                    refreshQueue = true;
                                }
                            }
                        }
                        Location loc = OM.Host.CurrentLocation;
                        if ((loc.Latitude.ToString() != "") && (loc.Longitude.ToString() != ""))
                        {
                            bool found = false;
                            string state = "";
                            string zip = "";
                            string city = "";
                            string country = "";
                            try
                            {
                                if (System.IO.File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMGPS"))) //OMGPS db exists, attempt a lookup in it first
                                {
                                    using (SqliteConnection con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMGPS") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0;journal_mode=WAL"))
                                    {
                                        if (con.State != System.Data.ConnectionState.Open)
                                            con.Open();
                                        using (SqliteCommand command = new SqliteCommand("SELECT * FROM GeoNames WHERE Latitude = '" + loc.Latitude.ToString() + "' AND Longitude = '" + loc.Longitude.ToString() + "'", con))
                                        {
                                            using (SqliteDataReader reader = command.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    found = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                            if (!found) //attempt a lookup on net if possible
                            {
                                if (OM.Host.InternetAccess)
                                {
                                    //use geonames website
                                    try
                                    {
                                        string html;
                                        using (WebClient wc = new WebClient())
                                        {
                                            //http://api.geonames.org/findNearbyPostalCodes?lat=47&lng=9&username=demo
                                            //http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=36&lng=-79.08
                                            html = wc.DownloadString("http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=" + loc.Latitude.ToString() + "&lng=" + loc.Longitude.ToString());
                                        }
                                        if (html != "{\"postalCodes\": []}")
                                        {
                                            html = html.Remove(0, html.IndexOf("postalCode") + 11);
                                            zip = html.Remove(0, html.IndexOf("postalCode") + 11);
                                            zip = zip.Remove(0, zip.IndexOf("\"") + 1);
                                            zip = zip.Substring(0, zip.IndexOf("\""));
                                            country = html.Remove(0, html.IndexOf("countryCode") + 12);
                                            country = country.Remove(0, country.IndexOf("\"") + 1);
                                            country = country.Substring(0, country.IndexOf("\""));
                                            city = html.Remove(0, html.IndexOf("placeName") + 10);
                                            city = city.Remove(0, city.IndexOf("\"") + 1);
                                            city = city.Substring(0, city.IndexOf("\""));
                                            state = html.Remove(0, html.IndexOf("adminName1") + 11);
                                            state = state.Remove(0, state.IndexOf("\"") + 1);
                                            state = state.Substring(0, state.IndexOf("\""));
                                        }
                                    }
                                    catch { }
                                }
                            }
                            //here we take the values from above and do a search...
                            Dictionary<string, string> searchResults;
                            if ((state != "") && (zip != "") && (city != "") && (country != ""))
                            {
                                if (country == "US") //zip
                                {
                                    searchResults = WeatherInfo.SearchArea(zip);
                                }
                                else //city+state
                                {
                                    searchResults = WeatherInfo.SearchArea(city + ", " + state);
                                }
                                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                                {
                                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                                    WeatherInfo.GetCurrentTemp(searchResults["SearchHtml"]);
                                    WeatherInfo.GetCurrentImage(searchResults["SearchHtml"]);
                                    WeatherInfo.GetCurrentDescription(searchResults["SearchHtml"]);
                                    WeatherInfo.GetCurrentForecast(searchResults["ForecastHtml"]);
                                }
                                else
                                {
                                    refreshQueue = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        refreshQueue = true;
                    }
                    for(int i=0;i<theHost.ScreenCount;i++)
                        VisibleSearchProgress(false,i);

                    pollsw.Reset();
                    pollsw.Start();
                }
                Thread.Sleep(1);
            }
        }

        private object VisibleForecast(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            if ((bool)param[0])
            {
                for (int j = 1; j < 6; j++)
                {
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastDay" + j.ToString(), true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastImage" + j.ToString(), true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastDescription" + j.ToString(), true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastHigh" + j.ToString(), true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastLow" + j.ToString(), true);
                }
            }
            else
            {
                for (int j = 1; j < 6; j++)
                {
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastDay" + j.ToString(), false);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastImage" + j.ToString(), false);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastDescription" + j.ToString(), false);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastHigh" + j.ToString(), false);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.ForecastLow" + j.ToString(), false);
                }
            }
            return null;
        }

        private object VisibleLocations(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            if ((bool)param[0])
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationTextBox", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationSearchButton", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationMultipleAreaList", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationLabel", true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationTextBox", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationSearchButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationMultipleAreaList", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.LocationLabel", false);
            }
            return null;
        }

        private object VisibleSearchProgress(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            if ((bool)param[0])
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressImage", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressLabel", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressBackground", true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressImage", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressLabel", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressBackground", false);
            }
            return null;
        }

        private void VisibleSearchProgress(bool visible, int screen)
        {
            if (visible)
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressImage", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressLabel", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressBackground", true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressImage", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressLabel", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressBackground", false);
            }
        }

        private object SearchWeather(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            if (tmr[Convert.ToInt32(param[1])] != null)
            {
                if (tmr[Convert.ToInt32(param[1])] != null)
                    tmr[Convert.ToInt32(param[1])].Dispose();
            }
            //get results of searching for the text
            VisibleSearchProgress(true, Convert.ToInt32(param[1]));
            Dictionary<string, string> searchResults = WeatherInfo.SearchArea(param[0].ToString());

            if (searchResults.Keys.ElementAt(0) == "AreaFound")
            {
                //update area dataSource with areafound value
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Searched.Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                if (searchString.Keys.Contains(Convert.ToInt16(param[1])))
                    searchString[Convert.ToInt16(param[1])] = param[0].ToString();
                else
                    searchString.Add(Convert.ToInt16(param[1]), param[0].ToString());
                polledSearchedWeather[Convert.ToInt16(param[1])] = searchResults;
                WeatherInfo.GetSearchedTemp(GetSearchHTML(Convert.ToInt16(param[1])), Convert.ToInt16(param[1]));
                WeatherInfo.GetSearchedImage(GetSearchHTML(Convert.ToInt16(param[1])), Convert.ToInt16(param[1]));
                WeatherInfo.GetSearchedDescription(GetSearchHTML(Convert.ToInt16(param[1])), Convert.ToInt16(param[1]));
                WeatherInfo.GetSearchedForecast(GetForecastHTML(Convert.ToInt16(param[1])), Convert.ToInt16(param[1]));
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("DSWeather;Weather.Searched" + param[1].ToString() + ".SearchedTemp", GetSearchedTemp(dataSource,Convert.ToInt16(param[1])));
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("DSWeather;Weather.Searched" + param[1].ToString() + ".SearchedImage", GetSearchedImage(dataSource,Convert.ToInt16(param[1])));
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("DSWeather;Weather.Searched" + param[1].ToString() + ".SearchedDescription", GetSearchedDescription(dataSource, Convert.ToInt16(param[1])));
                VisibleSearchProgress(false, Convert.ToInt32(param[1]));
                return null;
            }
            else
            {
                if ((searchResults.Keys.ElementAt(0) == "Problem With Internet Connection") || (searchResults.Keys.ElementAt(0) == "No Internet Connection Found"))
                {
                    //start a timer on 15 seconds to retry with the searchText
                    if (tmr[Convert.ToInt32(param[1])] == null)
                    {
                        tmr[Convert.ToInt32(param[1])] = new OpenMobile.Timer(15000);
                        tmr[Convert.ToInt32(param[1])].Screen = Convert.ToInt32(param[1]);
                        tmr[Convert.ToInt32(param[1])].Tag = param[0].ToString();
                        tmr[Convert.ToInt32(param[1])].Interval = 15000;
                        tmr[Convert.ToInt32(param[1])].Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
                        tmr[Convert.ToInt32(param[1])].Enabled = true;
                    }
                }
                VisibleSearchProgress(false, Convert.ToInt32(param[1]));
                return searchResults;
            }
        }

        private object CurrentWeather(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            for(int i=0;i<theHost.ScreenCount;i++)
                VisibleSearchProgress(true, i);
            Location loc = OM.Host.CurrentLocation;
            if ((loc.Latitude.ToString() != "") && (loc.Longitude.ToString() != ""))
            {
                bool found = false;
                string state = "";
                string zip = "";
                string city = "";
                string country = "";
                try
                {
                    if (System.IO.File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMGPS"))) //OMGPS db exists, attempt a lookup in it first
                    {
                        using (SqliteConnection con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMGPS") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0;journal_mode=WAL"))
                        {
                            if (con.State != System.Data.ConnectionState.Open)
                                con.Open();
                            using (SqliteCommand command = new SqliteCommand("SELECT * FROM GeoNames WHERE Latitude = '" + loc.Latitude.ToString() + "' AND Longitude = '" + loc.Longitude.ToString() + "'", con))
                            {
                                using (SqliteDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
                if (!found) //attempt a lookup on net if possible
                {
                    if (OM.Host.InternetAccess)
                    {
                        //use geonames website
                        try
                        {
                            string html;
                            using (WebClient wc = new WebClient())
                            {
                                //http://api.geonames.org/findNearbyPostalCodes?lat=47&lng=9&username=demo
                                //http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=36&lng=-79.08
                                html = wc.DownloadString("http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=" + loc.Latitude.ToString() + "&lng=" + loc.Longitude.ToString());
                            }
                            if (html != "{\"postalCodes\": []}")
                            {
                                html = html.Remove(0, html.IndexOf("postalCode") + 11);
                                zip = html.Remove(0, html.IndexOf("postalCode") + 11);
                                zip = zip.Remove(0, zip.IndexOf("\"") + 1);
                                zip = zip.Substring(0, zip.IndexOf("\""));
                                country = html.Remove(0, html.IndexOf("countryCode") + 12);
                                country = country.Remove(0, country.IndexOf("\"") + 1);
                                country = country.Substring(0, country.IndexOf("\""));
                                city = html.Remove(0, html.IndexOf("placeName") + 10);
                                city = city.Remove(0, city.IndexOf("\"") + 1);
                                city = city.Substring(0, city.IndexOf("\""));
                                state = html.Remove(0, html.IndexOf("adminName1") + 11);
                                state = state.Remove(0, state.IndexOf("\"") + 1);
                                state = state.Substring(0, state.IndexOf("\""));
                            }
                        }
                        catch { }
                    }
                }
                //here we take the values from above and do a search...
                Dictionary<string, string> searchResults;
                if ((state != "") && (zip != "") && (city != "") && (country != ""))
                {
                    if (country == "US") //zip
                    {
                        searchResults = WeatherInfo.SearchArea(zip);
                    }
                    else //city+state
                    {
                        searchResults = WeatherInfo.SearchArea(city + ", " + state);
                    }
                    if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    {
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                        WeatherInfo.GetCurrentTemp(searchResults["SearchHtml"]);
                        WeatherInfo.GetCurrentImage(searchResults["SearchHtml"]);
                        WeatherInfo.GetCurrentDescription(searchResults["SearchHtml"]);
                        WeatherInfo.GetCurrentForecast(searchResults["ForecastHtml"]);
                    }
                    else
                    {
                        refreshQueue = true;
                    }
                }
            }
            else
            {
                //NO CURRENT LOCATION INFORMATION FOUND TO SEARCH
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Area", "No current GPS LOcation Available" + "\n(Click To Search Weather)");
            }
            for(int i=0;i<theHost.ScreenCount;i++)
                VisibleSearchProgress(false,i);
            return null;
        }

        private string GetSearchHTML(int screen)
        {
            Dictionary<string, string> inner = polledSearchedWeather[screen];
            return inner["SearchHtml"];
        }

        private string GetForecastHTML(int screen)
        {
            Dictionary<string, string> inner = polledSearchedWeather[screen];
            return inner["ForecastHtml"];
        }

        private OpenMobile.Timer[] tmr;
        private void tmr_Elapsed(object sender, EventArgs e)
        {
            for (int i = 0; i < tmr.Length; i++)
            {
                if (tmr[i] != null)
                {
                    if (tmr[i] == (OpenMobile.Timer)sender)
                    {
                        string searchText = tmr[i].Tag.ToString();
                        int screen = tmr[i].Screen;
                        tmr[i].Dispose();
                        VisibleSearchProgress(true, screen);
                        Dictionary<string, string> searchResults = WeatherInfo.SearchArea(searchText);
                        if (searchResults.Keys.ElementAt(0) == "AreaFound")
                        {
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Area", searchResults["AreaFound"]);
                            if (searchString.Keys.Contains(screen))
                                searchString[screen] = searchText;
                            else
                                searchString.Add(screen, searchText);
                            polledSearchedWeather[screen] = searchResults;
                            WeatherInfo.GetSearchedTemp(GetSearchHTML(screen), screen);
                            WeatherInfo.GetSearchedImage(GetSearchHTML(screen), screen);
                            WeatherInfo.GetSearchedDescription(GetSearchHTML(screen), screen);
                            WeatherInfo.GetSearchedForecast(GetForecastHTML(screen), screen);
                            VisibleSearchProgress(true, screen);
                            return;
                        }
                        else
                        {
                            VisibleSearchProgress(false, screen);
                            if ((searchResults.Keys.ElementAt(0) == "Problem With Internet Connection") || (searchResults.Keys.ElementAt(0) == "No Internet Connection Found"))
                            {
                                if (tmr[screen] == null)
                                {
                                    tmr[screen] = new OpenMobile.Timer(15000);
                                    tmr[screen].Screen = screen;
                                    tmr[screen].Tag = searchText;
                                    tmr[screen].Interval = 15000;
                                    tmr[screen].Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
                                    tmr[screen].Enabled = true;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        //private string GetSearchedTemp(OpenMobile.Data.DataSource dataSource, int screen)
        //{
        //return WeatherInfo.GetTemp(DictionariesToSearchHtml(dataSource, screen));
        //}

        //private OpenMobile.imageItem GetSearchedImage(OpenMobile.Data.DataSource dataSource)
        //{
        //return WeatherInfo.GetImage(DictionariesToSearchHtml(dataSource, screen));
        //}

        //private string GetSearchedDescription(OpenMobile.Data.DataSource dataSource, int screen)
        //{
        //return WeatherInfo.GetDescription(DictionariesToSearchHtml(dataSource, screen));
        //}

        private string DictionariesToSearchHtml(OpenMobile.Data.DataSource dataSource, int screen)
        {
            //int screen = Convert.ToInt16(dataSource.NameLevel2.Remove(0, dataSource.NameLevel2.IndexOf("-") + 1));
            Dictionary<string, string> inner = polledSearchedWeather[screen];
            return inner["SearchHtml"];
        }

        private string DictionariesToForecastHtml(OpenMobile.Data.DataSource dataSource)
        {
            int screen = Convert.ToInt16(dataSource.NameLevel3.Remove(0, dataSource.NameLevel3.IndexOf("-") + 1));
            Dictionary<string, string> inner = polledSearchedWeather[screen];
            return inner["ForecastHtml"];
        }

        private object GetCurrentPositionWeather(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            //if (param == null)
            //    return null;
            Dictionary<string, string> weatherinfo = new Dictionary<string, string>();

            return weatherinfo;
        }

        private Dictionary<string, string> DictionaryAddRange(Dictionary<string, string> destination, Dictionary<string, string> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source.Keys.ElementAt(i).ToString(), source[source.Keys.ElementAt(i).ToString()]);
            }
            return destination;
        }

        private void Starting()
        {
            if (sqlConn == null)
                sqlConn = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMMedia2") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0");
            if (sqlConn.State != System.Data.ConnectionState.Open)
                sqlConn.Open();
        }

        private void Closing()
        {
            if (sqlConn != null)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                sqlConn.Dispose();
            }
        }

        public Settings loadSettings()
        {
            if (settings == null)
                settings = new Settings("Weather Settings");
            List<string> refreshRates = new List<string>() { "5", "15", "30", "60" };
            settings.Add(new Setting(SettingTypes.MultiChoice, "OMDSWeather.RefreshRate", "", "Refresh Rate (Minutes)", refreshRates, refreshRates, StoredData.Get(this, "OMDSWeather.RefreshRate")));
            settings.OnSettingChanged += new SettingChanged(setting_changed);
            return settings;
        }

        private void setting_changed(int screen, Setting s)
        {
            StoredData.Set(this, s.Name, s.Value);

            // Update refreshrate
            refreshRate = StoredData.GetInt(this, "OMDSWeather.RefreshRate") * 1000 * 60;
        }

        public string authorName
        {
            get { return "Peter Yeaney"; }
        }

        public string authorEmail
        {
            get { return "peter.yeaney@outlook.com"; }
        }

        public string pluginName
        {
            get { return "OMDSWeather"; }
        }
        public string displayName
        {
            get { return "Weather Data"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Weather Data"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-Weather2"); }
        }

        public void Dispose()
        {
            //Closing();
            Killing();
        }

    }

    public class WeatherInfo
    {
        public static IBasePlugin ibp;
        static OImage currentWeatherNotificationImage;
        static Notification currentWeatherNotification;

        public static void GetSearchedTemp(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature\">") + 20);
            string degreecheck = html.Substring(0, html.IndexOf(">"));
            html = html.Remove(0, html.IndexOf(">") + 1);
            //return DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
            //just update the dataSource from here?
            string temp = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);


            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Temp", temp);
        }

        public static void GetCurrentTemp(string html)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature\">") + 20);
            string degreecheck = html.Substring(0, html.IndexOf(">"));
            html = html.Remove(0, html.IndexOf(">") + 1);

            string temp = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
            if (currentWeatherNotificationImage != null)
                currentWeatherNotification.Dispose();
            currentWeatherNotificationImage = new OImage(Color.Transparent, 85, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height);
            Font f = Font.Arial;
            f.Size = 24;
            currentWeatherNotificationImage.RenderText(0, 0, currentWeatherNotificationImage.Width, OM.Host.UIHandler.StatusBar_DefaultIconSize.Height, temp, f, eTextFormat.Normal, Alignment.CenterCenter, BuiltInComponents.SystemSettings.SkinTextColor, BuiltInComponents.SystemSettings.SkinFocusColor, FitModes.FitFillSingleLine);
            if (currentWeatherNotification == null)
            {
                currentWeatherNotification = new Notification(Notification.Styles.IconOnly, ibp, "CurrentWeatherNotification", DateTime.Now, null, currentWeatherNotificationImage, "CurrentWeatherNotification", "");
                currentWeatherNotification.State = Notification.States.Active;
                currentWeatherNotification.IconSize_Width = currentWeatherNotificationImage.Width;
                OM.Host.UIHandler.AddNotification(currentWeatherNotification);
            }
            else
            {
                currentWeatherNotification.Icon = currentWeatherNotificationImage;
            }

            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Temp", temp);
        }

        public static void GetSearchedImage(string html, int screen)
        {
            //return an OImage
            //html = html.Remove(0, html.IndexOf("wx-weather-icon") + 22);
            html = html.Remove(0, html.IndexOf("wx-data-part wx-first"));
            html = html.Remove(0, html.IndexOf("src=") + 5);
            string imageurl = html.Substring(0, html.IndexOf(" ") - 1);
            //string imagedesc = html.Remove(0,html.IndexOf("alt=")+5);
            //imagedesc = imagedesc.Substring(0,imagedesc.IndexOf(">")-1);
            //COMPARE DESC IN SQLITE
            //** IS SAVING TO A FOLDER EASIER, MORE EFFICIENT, MORE FLEXIBLE??? **
            //using(SqlCommand command = new SqlCommand("SELECT ImageDesc FROM WeatherImages WHERE ImageDesc='"+imagedesc+"'",sqlconnection))
            //{
            //	using(SqlDataReader reader = command.ExecuteQuery())
            //	{
            //		if(reader.HasRows)
            //		{

            //		}
            //	}
            //}
            //IF NO RESULT, TRY TO GET FROM NET
            try
            {
                //check for network connection first
                //return new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl));
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Image", new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl)));
                return;
            }
            catch { } //wrapped in case of timeout or something else....
            //IF CAN'T GET FROM NET AND DOESN'T EXIST IN SQLITE, RETURN A BLANK IMAGE
            //return imageItem.NONE;
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Image", imageItem.NONE);
        }

        public static void GetCurrentImage(string html)
        {
            html = html.Remove(0, html.IndexOf("wx-data-part wx-first"));
            html = html.Remove(0, html.IndexOf("src=") + 5);
            string imageurl = html.Substring(0, html.IndexOf(" ") - 1);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Image", new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl)));
        }

        public static void GetSearchedDescription(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-phrase") + 12);
            string currentdescstr = html.Substring(0, html.IndexOf("<"));
            //return html.Substring(0, html.IndexOf("<"));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Description", html.Substring(0, html.IndexOf("<")));
        }

        public static void GetCurrentDescription(string html)
        {
            html = html.Remove(0, html.IndexOf("wx-phrase") + 12);
            string currentdescstr = html.Substring(0, html.IndexOf("<"));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Description", html.Substring(0, html.IndexOf("<")));
        }

        public static void GetSearchedForecast(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature-") + 3);
            string degreecheck = html.Substring(0, html.IndexOf(">") - 1);
            int i = 1;
            while (html.Contains("wx-daypart"))
            {
                html = html.Remove(0, html.IndexOf("wx-daypart") + 10);
                html = html.Remove(0, html.IndexOf("wx-label") + 10);
                string forecastdaystr = html.Substring(0, html.IndexOf("<"));
                //wi.Add("ForecastDate" + i.ToString(), forecastdaystr);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.ForecastDay" + i.ToString(), forecastdaystr);
                html = html.Remove(0, html.IndexOf("src") + 5);
                string forecastimageurl = html.Substring(0, html.IndexOf(" ") - 1);
                //wi.Add("ForecastImage" + i.ToString(), forecastimageurl);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.ForecastImage" + i.ToString(), new imageItem(OpenMobile.Net.Network.imageFromURL(forecastimageurl)));
                html = html.Remove(0, html.IndexOf("wx-temp") + 10);
                string forecasthighstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                //wi.Add("ForecastHigh" + i.ToString(), forecasthighstr);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.ForecastHigh" + i.ToString(), forecasthighstr);
                html = html.Remove(0, html.IndexOf("wx-temp-alt") + 14);
                string forecastlowstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                //wi.Add("ForecastLow" + i.ToString(), forecastlowstr);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.ForecastLow" + i.ToString(), forecastlowstr);
                html = html.Remove(0, html.IndexOf("wx-phrase") + 11);
                string forecastdescstr = html.Substring(0, html.IndexOf("<"));
                if (forecastdescstr.Contains("x-severe"))
                    forecastdescstr = forecastdescstr.Remove(0, 10);
                //wi.Add("ForecastDescription" + i.ToString(), forecastdescstr);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.ForecastDescription" + i.ToString(), forecastdescstr);
                i++;
            }
        }

        public static void GetCurrentForecast(string html)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature-") + 3);
            string degreecheck = html.Substring(0, html.IndexOf(">") - 1);
            int i = 1;
            while (html.Contains("wx-daypart"))
            {
                html = html.Remove(0, html.IndexOf("wx-daypart") + 10);
                html = html.Remove(0, html.IndexOf("wx-label") + 10);
                string forecastdaystr = html.Substring(0, html.IndexOf("<"));
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Forecast" + i.ToString(), forecastdaystr);
                html = html.Remove(0, html.IndexOf("src") + 5);
                string forecastimageurl = html.Substring(0, html.IndexOf(" ") - 1);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.ForecastImage" + i.ToString(), new imageItem(OpenMobile.Net.Network.imageFromURL(forecastimageurl)));
                html = html.Remove(0, html.IndexOf("wx-temp") + 10);
                string forecasthighstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.ForecastHigh" + i.ToString(), forecasthighstr);
                html = html.Remove(0, html.IndexOf("wx-temp-alt") + 14);
                string forecastlowstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.ForecastLow" + i.ToString(), forecastlowstr);
                html = html.Remove(0, html.IndexOf("wx-phrase") + 11);
                string forecastdescstr = html.Substring(0, html.IndexOf("<"));
                if (forecastdescstr.Contains("x-severe"))
                    forecastdescstr = forecastdescstr.Remove(0, 10);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.ForecastDescription" + i.ToString(), forecastdescstr);
                i++;
            }
        }

        //** TRY/CATCH BLOCK IN CASE OF TIMEOUTS
        //** OR OTHER ERRORS THAT COME UP
        //** WRAPPED ENTIRETY TO PREVENT HALF DATA GATHERED
        public static Dictionary<string, string> SearchArea(string searchText)
        {



            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "http://www.weather.com/search/enhancedlocalsearch?where=" + searchText;
            url = url.Replace(",", "+");
            url = url.Replace(" ", "+");
            if (url.Contains("("))
                url = url.Substring(0, url.IndexOf("(")).Trim();
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                if (!OM.Host.InternetAccess)
                {
                    wi.Clear();
                    wi.Add("No Internet Connection Found", "");
                }
                else
                {
                    try
                    {
                        string searchhtml = wc.DownloadString(url);
                        string forecasthtml = "";
                        if (searchhtml.Contains("No results found. Try your search again."))
                        {
                            wi.Add("NothingFound", "");
                        }
                        else if (searchhtml.Contains("Search Results for")) //return wi with areas to choose from
                        {
                            wi.Add("MultipleResults", "");
                            //check if enough results to launch different url
                            searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("searchResultsTxt"));
                            string results = searchhtml.Substring(0, searchhtml.IndexOf("<") - 1);
                            results = results.Remove(0, results.LastIndexOf(" ") + 1);
                            if (Convert.ToInt16(results) > 3)
                            {
                                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("searchResultsMoreLink"));
                                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("a href=") + 8);
                                url = "http://www.weather.com" + searchhtml.Substring(0, searchhtml.IndexOf(" ") - 1);
                                searchhtml = wc.DownloadString(url);
                            }
                            wi = DictionaryAddRange(wi, MultipleAreas(searchhtml));
                        }
                        else
                        {
                            string area = searchhtml.Remove(0, searchhtml.IndexOf("<h1>") + 4).Trim();
                            area = area.Substring(0, area.IndexOf("Weather")).Trim();
                            wi.Add("AreaFound", area);
                            string newurl = searchhtml.Remove(0, searchhtml.IndexOf("/weather/5-day/"));
                            url = "http://www.weather.com" + newurl.Substring(0, newurl.IndexOf(" ") - 1);
                            forecasthtml = wc.DownloadString(url);
                        }
                        wi.Add("SearchHtml", searchhtml);
                        if (forecasthtml != "")
                            wi.Add("ForecastHtml", forecasthtml);
                    }
                    catch (Exception ex)
                    {
                        wi.Clear();
                        wi.Add("Problem With Internet Connection", "");
                        //searchArea = searchText;
                    }
                }
            }
            //if(SearchedArea!=null)
            //	SearchedArea(wi);

            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressImage", false);
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressLabel", false);
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressBackground", false);

            return wi;
        }

        private static Dictionary<string, string> MultipleAreas(string searchhtml)
        {
            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "";
            searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("searchResultsTxt"));
            searchhtml = searchhtml.Substring(0, searchhtml.LastIndexOf("enhsearch"));

            while (searchhtml.Contains("enhsearch"))
            {
                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("a href=") + 8);
                url = searchhtml.Substring(0, searchhtml.IndexOf(" ") - 1);
                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("enhsearch") + 13);
                wi.Add(searchhtml.Substring(0, searchhtml.IndexOf("<")).Trim(), "http://www.weather.com" + url);
            }
            return wi;
        }

        public static Dictionary<string, string> DictionaryAddRange(Dictionary<string, string> destination, Dictionary<string, string> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source.Keys.ElementAt(i).ToString(), source[source.Keys.ElementAt(i).ToString()]);
            }
            return destination;
        }

        private static string DegreeCheck(string temp, string newdegree)
        {
            string newtemp = temp;
            string currentdegree = "";
            string degreesymbol = ((Char)176).ToString();
            if (newdegree.Contains("temperature-f"))
                newdegree = "Fahrenheit";
            else
                newdegree = "Celcius";
            using (PluginSettings ps = new PluginSettings())
            {
                if ((ps.getSetting(ibp, "OMDSWeather.Degree") == "") || (ps.getSetting(ibp, "OMDSWeather.Degree") == null))
                    ps.setSetting(ibp, "OMDSWeather.Degree", newdegree);
                currentdegree = ps.getSetting(ibp, "OMDSWeather.Degree");
            }
            if (newdegree == "Fahrenheit")
                newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
            else if (newdegree == "Celcius")
                newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), true).ToString();
            return newtemp;


            if (currentdegree != newdegree)
            {
                if (currentdegree == "Fahrenheit")
                    //newtemp = (Convert.ToInt32(OpenMobile.Framework.Math.Calculation.CtoF(Convert.ToDouble(temp)))).ToString() + degreesymbol + "F";
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
                else if (currentdegree == "Celcius")
                    //newtemp = (Convert.ToInt32(OpenMobile.Framework.Math.Calculation.FtoC(Convert.ToDouble(temp)))).ToString() + degreesymbol + "C";
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
            }
            else
            {
                if (currentdegree == "Fahrenheit")
                    //newtemp = (Convert.ToInt32(newtemp)).ToString() + degreesymbol + "F";
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
                else
                    //newtemp = (Convert.ToInt32(newtemp)).ToString() + degreesymbol + "C";
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
            }
            return newtemp;
        }

    }

}