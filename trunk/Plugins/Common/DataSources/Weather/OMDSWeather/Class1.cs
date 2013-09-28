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
        Dictionary<string, string> currentDictionary;
        Dictionary<int, Dictionary<string, object>> searchedDictionary;
        OImage currentWeatherNotificationImage;
        Notification currentWeatherNotification;
        bool[] fromCurrent;
        string[] searchedArea;

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
            searchedDictionary = new Dictionary<int, Dictionary<string, object>>();
            fromCurrent = new bool[theHost.ScreenCount];
            searchedArea = new string[theHost.ScreenCount];

            polling = new Thread(poller);
            //polling.Start();
            //tmr = new System.Timers.Timer[theHost.ScreenCount];

            //theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Searched", "List", DataSource.DataTypes.raw, "Searched Weather Results"), searchedDictionary);
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Search", "SearchLocation", SearchWeather2, 2, false, "Searches for weather at the specified search area"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Search", "CurrentLocation", SearchCurrentWeather, 1, false, "Searches for weather at the current location"));

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                //each screen gets its own Dictionary<string,string> of data

                //if (searchedDictionary[i] == null)
                searchedDictionary.Add(i, new Dictionary<string, object>());
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Searched", "List" + i.ToString(), DataSource.DataTypes.raw, "Searched Weather Results"));

                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressImage", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressLabel", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressBackground", DataSource.DataTypes.binary, ""), false);

            }

            //power event
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);

            // Initialize default data
            StoredData.SetDefaultValue(this, "OMDSWeather.RefreshRate", 5);

            tmr = new OpenMobile.Timer[OM.Host.ScreenCount];

            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Refresh", "All", RefreshWeather, 0, false, "Refreshes Searched and Current Location Weather"));

            //OM.Host.DataHandler.RemoveDataSource("Provider;name.name.name.name");

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
                if (polling.ThreadState == ThreadState.Running)
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
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        VisibleSearchProgress(true, i);
                    if (OM.Host.InternetAccess)
                    {
                        theHost.CommandHandler.ExecuteCommand("OMGPS;GPS.Location.ReverseGeocode", new object[] { });
                        Location loc = OM.Host.CurrentLocation;
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            if (!fromCurrent[i])
                            {
                                Dictionary<string, object> Results = new Dictionary<string, object>();
                                Dictionary<string, string> searchResults = SearchArea(searchedArea[i], i);
                                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                                {
                                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                                    Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"], i));
                                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"], i));
                                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"], i));
                                    GetSearchedForecast(searchResults["ForecastHtml"], i, Results);
                                }
                                else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                                {
                                    for (int j = 0; j < searchResults.Count; j++)
                                        Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                                }
                                else
                                {
                                    //problem with connections or no results
                                    refreshQueue = true;
                                }
                                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + i.ToString(), Results);
                            }
                            else
                            {
                                //current
                                Dictionary<string, object> Results = new Dictionary<string, object>();
                                Dictionary<string, string> searchResults = new Dictionary<string, string>();
                                if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
                                {
                                    if (loc.Country == "US")
                                        searchResults = WeatherInfo.SearchArea(loc.Zip, i);
                                    else
                                        searchResults = WeatherInfo.SearchArea(loc.City + ", " + loc.State, i);

                                    if (searchResults.Keys.ElementAt(0) == "AreaFound")
                                    {
                                        Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                                        Results.Add("Temp", GetCurrentTemp(searchResults["SearchHtml"], i));
                                        Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"], i));
                                        Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"], i));
                                        GetSearchedForecast(searchResults["ForecastHtml"], i, Results);
                                    }
                                    else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                                    {
                                        for (int j = 0; j < searchResults.Count; j++)
                                            Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                                        //searchedDictionary[screen].Add(searchResults.Keys.ElementAt(i), searchResults[searchResults.Keys.ElementAt(i)]);
                                    }
                                    else
                                    {
                                        //problem with connections or no results
                                        refreshQueue = true;
                                    }
                                }
                                else
                                {
                                    //something is blank in loc

                                }
                                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + i.ToString(), Results);
                            }


                            //if (searchString.Keys.Contains(i))
                            //{
                            //    //perform update

                            //    polledSearchedWeather[i] = WeatherInfo.SearchArea(searchString[i].ToString(), i);
                            //    if (polledSearchedWeather[i].Keys.ElementAt(0) == "AreaFound")
                            //    {
                            //        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + i.ToString() + ".Weather.Searched.Area", polledSearchedWeather[i]["AreaFound"] + "\n(Click To Search Weather)");
                            //        WeatherInfo.GetSearchedTemp(GetSearchHTML(i), i);
                            //        WeatherInfo.GetSearchedImage(GetSearchHTML(i), i);
                            //        WeatherInfo.GetSearchedDescription(GetSearchHTML(i), i);
                            //        WeatherInfo.GetSearchedForecast(GetForecastHTML(i), i);
                            //    }
                            //    else
                            //    {
                            //        refreshQueue = true;
                            //    }
                            //}
                        }



                        //Location loc = OM.Host.CurrentLocation;
                        //if ((loc.Latitude.ToString() != "") && (loc.Longitude.ToString() != ""))
                        //{
                        //    bool found = false;
                        //    string state = "";
                        //    string zip = "";
                        //    string city = "";
                        //    string country = "";
                        //    try
                        //    {
                        //        if (System.IO.File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMGPS"))) //OMGPS db exists, attempt a lookup in it first
                        //        {
                        //            using (SqliteConnection con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMGPS") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0;journal_mode=WAL"))
                        //            {
                        //                if (con.State != System.Data.ConnectionState.Open)
                        //                    con.Open();
                        //                using (SqliteCommand command = new SqliteCommand("SELECT * FROM GeoNames WHERE Latitude = '" + loc.Latitude.ToString() + "' AND Longitude = '" + loc.Longitude.ToString() + "'", con))
                        //                {
                        //                    using (SqliteDataReader reader = command.ExecuteReader())
                        //                    {
                        //                        if (reader.HasRows)
                        //                        {
                        //                            found = true;
                        //                       }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    catch { }
                        //    if (!found) //attempt a lookup on net if possible
                        //    {
                        //        if (OM.Host.InternetAccess)
                        //        {
                        //            //use geonames website
                        //            try
                        //            {
                        //                string html;
                        //                using (WebClient wc = new WebClient())
                        //                {
                        //                    //http://api.geonames.org/findNearbyPostalCodes?lat=47&lng=9&username=demo
                        //                    //http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=36&lng=-79.08
                        //                    html = wc.DownloadString("http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=" + loc.Latitude.ToString() + "&lng=" + loc.Longitude.ToString());
                        //                }
                        //                if (html != "{\"postalCodes\": []}")
                        //                {
                        //                    html = html.Remove(0, html.IndexOf("postalCode") + 11);
                        //                    zip = html.Remove(0, html.IndexOf("postalCode") + 11);
                        //                    zip = zip.Remove(0, zip.IndexOf("\"") + 1);
                        //                    zip = zip.Substring(0, zip.IndexOf("\""));
                        //                    country = html.Remove(0, html.IndexOf("countryCode") + 12);
                        //                    country = country.Remove(0, country.IndexOf("\"") + 1);
                        //                    country = country.Substring(0, country.IndexOf("\""));
                        //                    city = html.Remove(0, html.IndexOf("placeName") + 10);
                        //                    city = city.Remove(0, city.IndexOf("\"") + 1);
                        //                    city = city.Substring(0, city.IndexOf("\""));
                        //                    state = html.Remove(0, html.IndexOf("adminName1") + 11);
                        //                    state = state.Remove(0, state.IndexOf("\"") + 1);
                        //                    state = state.Substring(0, state.IndexOf("\""));
                        //                }
                        //            }
                        //            catch { }
                        //        }
                        //    }
                        //    //here we take the values from above and do a search...
                        //    Dictionary<string, string> searchResults;
                        //    if ((state != "") && (zip != "") && (city != "") && (country != ""))
                        //    {
                        //        if (country == "US") //zip
                        //        {
                        //            searchResults = WeatherInfo.SearchArea(zip);
                        //        }
                        //        else //city+state
                        //        {
                        //            searchResults = WeatherInfo.SearchArea(city + ", " + state);
                        //        }
                        //        if (searchResults.Keys.ElementAt(0) == "AreaFound")
                        //        {
                        //            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                        //            WeatherInfo.GetCurrentTemp(searchResults["SearchHtml"]);
                        //            WeatherInfo.GetCurrentImage(searchResults["SearchHtml"]);
                        //            WeatherInfo.GetCurrentDescription(searchResults["SearchHtml"]);
                        //            WeatherInfo.GetCurrentForecast(searchResults["ForecastHtml"]);
                        //        }
                        //        else
                        //        {
                        //            refreshQueue = true;
                        //        }
                        //    }
                        //}





                    }
                    else
                    {
                        refreshQueue = true;
                    }
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        VisibleSearchProgress(false, i);

                    pollsw.Reset();
                    pollsw.Start();
                }
                Thread.Sleep(1);
            }
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

        private object SearchCurrentWeather(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);

            Dictionary<string, object> Results = new Dictionary<string, object>();
            VisibleSearchProgress(true, screen);
            //here we need to get the city,state,zip from OMGPS...
            theHost.CommandHandler.ExecuteCommand("OMGPS;GPS.Location.ReverseGeocode");
            Location loc = OM.Host.CurrentLocation;
            Dictionary<string, string> searchResults = new Dictionary<string, string>();
            if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
            {
                if (loc.Country == "US")
                    searchResults = WeatherInfo.SearchArea(loc.Zip, screen);
                else
                    searchResults = WeatherInfo.SearchArea(loc.City + ", " + loc.State, screen);

                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                {
                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                    Results.Add("Temp", GetCurrentTemp(searchResults["SearchHtml"], screen));
                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"], screen));
                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"], screen));
                    GetSearchedForecast(searchResults["ForecastHtml"], screen, Results);
                    Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"], screen));
                }
                else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                {
                    for (int i = 0; i < searchResults.Count; i++)
                        Results.Add(searchResults.Keys.ElementAt(i), searchResults[searchResults.Keys.ElementAt(i)]);
                    //searchedDictionary[screen].Add(searchResults.Keys.ElementAt(i), searchResults[searchResults.Keys.ElementAt(i)]);
                }
                else
                {
                    //problem with connections or no results
                    refreshQueue = true;
                }
            }
            else
            {
                //something is blank in loc

            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + screen.ToString(), Results);
            VisibleSearchProgress(false, screen);
            return null;
        }

        private object SearchWeather2(OpenMobile.Command command, object[] param, out bool result)
        {
            //param[0] = searchtext
            //param[1] = screen
            result = true;
            int screen = Convert.ToInt32(param[1]);

            Dictionary<string, object> Results = new Dictionary<string, object>();
            VisibleSearchProgress(true, screen);
            //Dictionary<string, string> searchResults = WeatherInfo.SearchArea(param[0].ToString(), Convert.ToInt32(param[1]));
            Dictionary<string, string> searchResults = SearchArea(param[0].ToString(), screen);
            if (searchResults.Keys.ElementAt(0) == "AreaFound")
            {
                Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                searchedArea[screen] = Results["Area"].ToString();
                Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"], screen));
                Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"], screen));
                Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"], screen));
                GetSearchedForecast(searchResults["ForecastHtml"], screen, Results);
                Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"], screen));
            }
            else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
            {
                for (int i = 0; i < searchResults.Count; i++)
                    Results.Add(searchResults.Keys.ElementAt(i), searchResults[searchResults.Keys.ElementAt(i)]);
            }
            else
            {
                //problem with connections or no results
                refreshQueue = true;
            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + screen.ToString(), Results);
            VisibleSearchProgress(false, screen);
            return null;
        }

        private string getUpdatedTime(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("Updated:") + 9);
            string time = html.Substring(0, html.IndexOf("<"));
            if (String.IsNullOrEmpty(time))
            {
                html = html.Remove(0, html.IndexOf("Updated:") + 9);
                time = html.Substring(0, html.IndexOf("<"));
            }
            return time;
        }

        private string GetSearchedTemp(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature\">") + 20);
            string degreecheck = html.Substring(0, html.IndexOf(">"));
            html = html.Remove(0, html.IndexOf(">") + 1);
            string temp = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
            return temp;
        }

        private string GetCurrentTemp(string html, int screen)
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
                currentWeatherNotification = new Notification(Notification.Styles.IconOnly, this, "CurrentWeatherNotification", DateTime.Now, null, currentWeatherNotificationImage, "CurrentWeatherNotification", "");
                currentWeatherNotification.State = Notification.States.Active;
                currentWeatherNotification.IconSize_Width = currentWeatherNotificationImage.Width;
                OM.Host.UIHandler.AddNotification(currentWeatherNotification);
            }
            else
            {
                currentWeatherNotification.Icon = currentWeatherNotificationImage;
            }

            return temp;
        }

        private imageItem GetSearchedImage(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-data-part wx-first"));
            html = html.Remove(0, html.IndexOf("src=") + 5);
            string imageurl = html.Substring(0, html.IndexOf(" ") - 1);
            try
            {
                imageItem image = new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl));
                return image;
            }
            catch { }
            return imageItem.NONE;
        }

        private string GetSearchedDescription(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-phrase") + 12);
            if (html.Contains("weather-phrase"))
                html = html.Remove(0, html.IndexOf(">") + 1);
            string currentdescstr = html.Substring(0, html.IndexOf("<"));
            return currentdescstr;
        }

        private void GetSearchedForecast(string html, int screen, Dictionary<string, object> Results)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature-") + 3);
            string degreecheck = html.Substring(0, html.IndexOf(">") - 1);
            int i = 1;
            while (html.Contains("wx-daypart"))
            {
                html = html.Remove(0, html.IndexOf("wx-daypart") + 10);
                html = html.Remove(0, html.IndexOf("wx-label") + 10);
                string forecastdaystr = html.Substring(0, html.IndexOf("<"));
                Results.Add("ForecastDay" + i.ToString(), forecastdaystr);

                html = html.Remove(0, html.IndexOf("src") + 5);
                string forecastimageurl = html.Substring(0, html.IndexOf(" ") - 1);
                imageItem forecastimage = new imageItem(OpenMobile.Net.Network.imageFromURL(forecastimageurl));
                Results.Add("ForecastImage" + i.ToString(), forecastimage);

                html = html.Remove(0, html.IndexOf("wx-temp") + 10);
                string forecasthighstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                Results.Add("ForecastHigh" + i.ToString(), forecasthighstr);

                html = html.Remove(0, html.IndexOf("wx-temp-alt") + 14);
                string forecastlowstr = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
                Results.Add("ForecastLow" + i.ToString(), forecastlowstr);

                html = html.Remove(0, html.IndexOf("wx-phrase") + 11);
                string forecastdescstr = html.Substring(0, html.IndexOf("<"));
                if (forecastdescstr.Contains("x-severe"))
                    forecastdescstr = forecastdescstr.Remove(0, 10);
                Results.Add("ForecastDescription" + i.ToString(), forecastdescstr);

                i++;
            }
        }

        private Dictionary<string, string> SearchArea(string searchText, int screen)
        {

            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "http://www.weather.com/search/enhancedlocalsearch?where=" + searchText + "&loctypes=1";
            //string url = "http://www.weather.com/localsearch/morelocations/?where=" + searchText; //ONLY GETS 1-20 results???
            //url = url.Replace(",", "+");
            url = url.Replace(" ", ",");
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
                            searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("Cities"));
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
                            //string newurl = searchhtml.Remove(0, searchhtml.IndexOf("/weather/5-day/"));
                            string newurl = searchhtml.Remove(0, searchhtml.IndexOf("/weather/tenday/"));
                            url = "http://www.weather.com" + newurl.Substring(0, newurl.IndexOf(" ") - 1);
                            forecasthtml = wc.DownloadString(url);
                            //asynch alerts???
                            Alerts(searchhtml, area, screen);
                            //asynch alerts???
                        }
                        wi.Add("SearchHtml", searchhtml);
                        if (forecasthtml != "")
                            wi.Add("ForecastHtml", forecasthtml);
                    }
                    catch (Exception ex)
                    {
                        wi.Clear();
                        wi.Add("Problem With Internet Connection", "");
                    }
                }
            }

            return wi;
        }

        private Dictionary<string, string> MultipleAreas(string searchhtml)
        {
            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "";
            searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("searchResultsTxt"));
            searchhtml = searchhtml.Substring(0, searchhtml.LastIndexOf("enhsearch"));

            while (searchhtml.Contains("enhsearch"))
            {
                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("a href=") + 8);
                url = searchhtml.Substring(0, searchhtml.IndexOf(" ") - 1);
                searchhtml = searchhtml.Remove(0, searchhtml.IndexOf("enhsearch") + 12);
                string test = searchhtml.Substring(0, searchhtml.IndexOf("<")).Trim();
                if (!wi.Keys.Contains(test))
                    wi.Add(searchhtml.Substring(0, searchhtml.IndexOf("<")).Trim(), "http://www.weather.com" + url);
            }
            return wi;
        }

        List<Notification> AlertNotifications = new List<Notification>();
        private void Alerts(string html, string area, int screen)
        {
            int alertCount = 0;
            if (html.Contains("wx-show-alert-links")) //more than 1 alert
            {
                html = html.Remove(0, html.IndexOf("wx-show-alert-links"));
                while (html.Contains("wx-alert-link-"))
                {
                    html = html.Remove(0, html.IndexOf("local_alert_list_today") + 24);
                    string alertDescription = html.Substring(0, html.IndexOf("<"));
                    string alertTime = "";
                    if (html.Contains("wx-alert-time"))
                    {
                        html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                        alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                    }
                    if (alertCount < AlertNotifications.Count)
                    {
                        AlertNotifications[alertCount].Header = "**** " + alertDescription + " **** ";
                        AlertNotifications[alertCount].Text = area + alertTime;
                    }
                    else
                    {
                        AlertNotifications.Add(new Notification(this, "Weather Alerts", OM.Host.getSkinImage("").image, OM.Host.getSkinImage("").image, "**** " + alertDescription + " ****", area + alertTime));
                        OM.Host.UIHandler.AddNotification(screen, AlertNotifications[alertCount]);
                    }
                    alertCount += 1;
                    //currentWeatherNotification = new Notification(Notification.Styles.IconOnly, ibp, "CurrentWeatherNotification", DateTime.Now, null, currentWeatherNotificationImage, "CurrentWeatherNotification", "");
                    //currentWeatherNotification.State = Notification.States.Active;
                    //currentWeatherNotification.IconSize_Width = currentWeatherNotificationImage.Width;
                    //OM.Host.UIHandler.AddNotification(currentWeatherNotification);
                }
            }
            else if (html.Contains("wx-alert-text")) //1 alert
            {
                html = html.Remove(0, html.IndexOf("local_alert_primary_today") + 27);
                string alertDescription = html.Substring(0, html.IndexOf("<"));
                string alertTime = "";
                if (html.Contains("wx-alert-time"))
                {
                    html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                    alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                }
                if (alertCount < AlertNotifications.Count)
                {
                    AlertNotifications[alertCount].Header = "**** " + alertDescription + " **** ";
                    AlertNotifications[alertCount].Text = area + alertTime;
                }
                else
                {
                    AlertNotifications.Add(new Notification(this, "Weather Alerts", OM.Host.getSkinImage("Images|Alert.gif").image, OM.Host.getSkinImage("Images|Alert.gif").image, "**** " + alertDescription + " ****", area + alertTime));
                    OM.Host.UIHandler.AddNotification(screen, AlertNotifications[alertCount]);
                }
                alertCount += 1;


            }
            else //no alerts
            {

            }
            List<int> toRemove = new List<int>();
            if (alertCount < AlertNotifications.Count)
            {
                while (alertCount < AlertNotifications.Count)
                {
                    OM.Host.UIHandler.RemoveNotification(screen, AlertNotifications[alertCount]);
                    toRemove.Add(alertCount);
                    alertCount += 1;
                }
            }
            if (toRemove.Count > 0)
            {
                for (int i = 0; i < toRemove.Count; i++)
                {
                    AlertNotifications.RemoveAt(toRemove[i]);
                }
            }
        }

        private string DegreeCheck(string temp, string newdegree)
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
                if ((ps.getSetting(this, "OMDSWeather.Degree") == "") || (ps.getSetting(this, "OMDSWeather.Degree") == null))
                    ps.setSetting(this, "OMDSWeather.Degree", newdegree);
                currentdegree = ps.getSetting(this, "OMDSWeather.Degree");
            }
            if (newdegree == "Fahrenheit")
                newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
            else if (newdegree == "Celcius")
                newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), true).ToString();
            return newtemp;
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
                        Dictionary<string, string> searchResults = WeatherInfo.SearchArea(searchText, screen);
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
            //    using(SqlDataReader reader = command.ExecuteQuery())
            //    {
            //        if(reader.HasRows)
            //        {

            //        }
            //    }
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

        public static Dictionary<string, string> SearchArea(string searchText)
        {



            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "http://www.weather.com/search/enhancedlocalsearch?where=" + searchText;
            //url = url.Replace(",", "+");
            url = url.Replace(" ", ",");
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
                            //DON"T DO ALERTS FOR CURRENT LOCATION
                            //asynch alerts???
                            //Alerts(searchhtml, area);
                            //asynch alerts???
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
            //    SearchedArea(wi);

            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressImage", false);
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressLabel", false);
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + param[1].ToString() + ".Weather.Visible.SearchProgressBackground", false);

            return wi;
        }

        //** TRY/CATCH BLOCK IN CASE OF TIMEOUTS
        //** OR OTHER ERRORS THAT COME UP
        //** WRAPPED ENTIRETY TO PREVENT HALF DATA GATHERED
        public static Dictionary<string, string> SearchArea(string searchText, int screen)
        {



            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "http://www.weather.com/search/enhancedlocalsearch?where=" + searchText;
            //url = url.Replace(",", "+");
            url = url.Replace(" ", ",");
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
                            //asynch alerts???
                            Alerts(searchhtml, area, screen);
                            //asynch alerts???
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
            //    SearchedArea(wi);

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

        static List<Notification> AlertNotifications = new List<Notification>();
        private static void Alerts(string html, string area, int screen)
        {
            int alertCount = 0;
            if (html.Contains("wx-show-alert-links")) //more than 1 alert
            {
                html = html.Remove(0, html.IndexOf("wx-show-alert-links"));
                while (html.Contains("wx-alert-link-"))
                {
                    html = html.Remove(0, html.IndexOf("local_alert_list_today") + 24);
                    string alertDescription = html.Substring(0, html.IndexOf("<"));
                    string alertTime = "";
                    if (html.Contains("wx-alert-time"))
                    {
                        html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                        alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                    }
                    if (alertCount < AlertNotifications.Count)
                    {
                        AlertNotifications[alertCount].Header = "**** " + alertDescription + " **** ";
                        AlertNotifications[alertCount].Text = area + alertTime;
                    }
                    else
                    {
                        AlertNotifications.Add(new Notification(ibp, "Weather Alerts", OM.Host.getSkinImage("").image, OM.Host.getSkinImage("").image, "**** " + alertDescription + " ****", area + alertTime));
                        OM.Host.UIHandler.AddNotification(screen, AlertNotifications[alertCount]);
                    }
                    alertCount += 1;
                    //currentWeatherNotification = new Notification(Notification.Styles.IconOnly, ibp, "CurrentWeatherNotification", DateTime.Now, null, currentWeatherNotificationImage, "CurrentWeatherNotification", "");
                    //currentWeatherNotification.State = Notification.States.Active;
                    //currentWeatherNotification.IconSize_Width = currentWeatherNotificationImage.Width;
                    //OM.Host.UIHandler.AddNotification(currentWeatherNotification);
                }
            }
            else if (html.Contains("wx-alert-text")) //1 alert
            {
                html = html.Remove(0, html.IndexOf("local_alert_primary_today") + 27);
                string alertDescription = html.Substring(0, html.IndexOf("<"));
                string alertTime = "";
                if (html.Contains("wx-alert-time"))
                {
                    html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                    alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                }
                if (alertCount < AlertNotifications.Count)
                {
                    AlertNotifications[alertCount].Header = "**** " + alertDescription + " **** ";
                    AlertNotifications[alertCount].Text = area + alertTime;
                }
                else
                {
                    AlertNotifications.Add(new Notification(ibp, "Weather Alerts", OM.Host.getSkinImage("Images|Alert.gif").image, OM.Host.getSkinImage("Images|Alert.gif").image, "**** " + alertDescription + " ****", area + alertTime));
                    OM.Host.UIHandler.AddNotification(screen, AlertNotifications[alertCount]);
                }
                alertCount += 1;


            }
            else //no alerts
            {

            }
            List<int> toRemove = new List<int>();
            if (alertCount < AlertNotifications.Count)
            {
                while (alertCount < AlertNotifications.Count)
                {
                    OM.Host.UIHandler.RemoveNotification(screen, AlertNotifications[alertCount]);
                    toRemove.Add(alertCount);
                    alertCount += 1;
                }
            }
            if (toRemove.Count > 0)
            {
                for (int i = 0; i < toRemove.Count; i++)
                {
                    AlertNotifications.RemoveAt(toRemove[i]);
                }
            }
        }

    }

}