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
    [PluginLevel(PluginLevels.System)]
    public class OMDSWeather : IBasePlugin, IDataSource
    {
        //events
        public delegate void UpdatedSearchedWeatherEventHandler(Dictionary<int, Dictionary<string, string>> polledsearchedweather);
        public static event UpdatedSearchedWeatherEventHandler UpdatedSearchedWeather;

        //variables
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
        List<string>[] favoritesList;
        List<string>[] favoritesSearchedList;
        List<Notification>[] alertNotifications;
        int refreshRate;
        private OpenMobile.Timer[] tmr;
        private bool[] fromRefresh;

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            //WeatherInfo.ibp = this;
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
            fromCurrent = new bool[OM.Host.ScreenCount];
            searchedArea = new string[OM.Host.ScreenCount];
            favoritesList = new List<string>[OM.Host.ScreenCount];
            favoritesSearchedList = new List<string>[OM.Host.ScreenCount];
            alertNotifications = new List<Notification>[OM.Host.ScreenCount];
            tmr = new OpenMobile.Timer[OM.Host.ScreenCount];
            fromRefresh = new bool[OM.Host.ScreenCount];

            // Initialize default data
            StoredData.SetDefaultValue(this, "OMDSWeather.RefreshRate", 5);
            refreshRate = StoredData.GetInt(this, "OMDSWeather.RefreshRate") * 1000 * 60;
            
            //tmr = new System.Timers.Timer[theHost.ScreenCount];

            //theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Searched", "List", DataSource.DataTypes.raw, "Searched Weather Results"), searchedDictionary);
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Search", "SearchLocation", SearchWeather2, 2, false, "Searches for weather at the specified search area: param={string searchText, int screen}"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Search", "CurrentLocation", SearchCurrentWeather, 1, false, "Searches for weather at the current location"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Refresh", "Weather", RefreshWeather, 0, false, "Refreshes Searched and Current Location Weather: OPTIONAL param={int screen, int screen, int screen, ...}"));
            
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Favorites", "Add", AddFavorite, 1, false, "Adds the searched area to the favorites list: param={int screen, string displayedFavoriteText}"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Favorites", "Remove", RemoveFavorite, 2, false, "Removes the selected area to the favorites list: param={int screen, int index}"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "Weather", "Favorites", "Show", ShowFavorite, 2, false, "Retrieves the weather for the favorite location: param={int screen, int index}"));

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                alertNotifications[i] = new List<Notification>();

                //if (searchedDictionary[i] == null)
                searchedDictionary.Add(i, new Dictionary<string, object>());
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Searched", "List" + i.ToString(), DataSource.DataTypes.raw, "Searched Weather Results"));
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Info", "Status" + i.ToString(), DataSource.DataTypes.raw, "Provides Status Of DataSource"));
                OM.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Favorites", "List" + i.ToString(), 0, DataSource.DataTypes.raw, FavoritesList, "Provides the list of favorites per screen"));

                //count
                searchedArea[i] = StoredData.Get(this, String.Format("OMDSWeather.Screen{0}.Favorites.Count", i.ToString()));
                favoritesList[i] = new List<string>();
                favoritesList[i].Add("Current Position");
                favoritesList[i].Add("Click To Search Weather");
                favoritesSearchedList[i] = new List<string>();
                favoritesSearchedList[i].Add("Current Position");
                favoritesSearchedList[i].Add("");
                if (searchedArea[i] != "")
                {
                    //favorites
                    for (int j = 2; j < Convert.ToInt32(searchedArea[i]); j++)
                    {
                        favoritesList[i].Add(StoredData.Get(this, String.Format("OMDSWeather.Screen{0}.Favorites.{1}", i.ToString(), j.ToString())));
                        favoritesSearchedList[i].Add(StoredData.Get(this, String.Format("OMDSWeather.Screen{0}.FavoritesSearched.{1}", i.ToString(), j.ToString())));
                    }
                }

                //get the last saved Searched
                searchedArea[i] = StoredData.Get(this, String.Format("OMDSWeather.Screen{0}.Searched", i.ToString()));
                if ((searchedArea[i] == "") || (searchedArea[i] == "Current"))
                    fromCurrent[i] = true;
                else
                {
                    fromCurrent[i] = false;
                }

                fromRefresh[i] = false;
                tmr[i] = new OpenMobile.Timer(refreshRate);
                tmr[i].Tag = "";
                tmr[i].Screen = i;
                tmr[i].Elapsed += new System.Timers.ElapsedEventHandler(OMDSWeather_Elapsed);
                tmr[i].Enabled = true;

            }

            //power event
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);

            //manualRefresh = true;
            //polling = new Thread(poller);
            //polling.Start();

            //OM.Host.DataHandler.RemoveDataSource("Provider;name.name.name.name");

            return eLoadStatus.LoadSuccessful;
        }

        void OMDSWeather_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OpenMobile.Timer tmr = ((OpenMobile.Timer)sender);
            tmr.Enabled = false;
            if (tmr.Tag.ToString() == "Running")
                return; //don't enable again as the end of this method enables the timer again
            tmr.Tag = "Running";
            int screen = tmr.Screen;
            VisibleSearchProgress(true, screen);

            if (OM.Host.InternetAccess)
            {
                Dictionary<string, object> Results = new Dictionary<string, object>();
                Dictionary<string, string> searchResults = new Dictionary<string, string>();
                Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", OM.Host.CurrentLocation);
                if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
                {
                    if (loc.Country == "US")
                        searchResults = SearchArea(loc.Zip);
                    else
                        searchResults = SearchArea(loc.City + ", " + loc.State);

                    if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    {
                        Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                        Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"], true));
                        Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                        Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                        GetSearchedForecast(searchResults["ForecastHtml"], Results);
                        Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
                    }
                    else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                    {
                        for (int j = 0; j < searchResults.Count; j++)
                            Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                    }
                    else
                    {
                        //problem with connections or no results
                        tmr.Interval = 15000; //15 seconds
                    }
                }
                else
                {
                    //something is blank in loc

                }
                if (!fromCurrent[screen])
                {
                    //we are using the searchedArea instead of current
                    Results.Clear();
                    searchResults.Clear();
                    searchResults = SearchArea(searchedArea[screen], screen);
                    if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    {
                        Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                        Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"]));
                        Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                        Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                        GetSearchedForecast(searchResults["ForecastHtml"], Results);
                        Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
                    }
                    else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                    {
                        for (int j = 0; j < searchResults.Count; j++)
                            Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                    }
                    else
                    {
                        //problem with connections or no results
                        tmr.Interval = 15000; //15 seconds
                    }
                }
                if (fromRefresh[screen])
                {
                    fromRefresh[screen] = false;
                    tmr.Interval = refreshRate;
                }
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + screen.ToString(), Results);
            }
            else
            {
                //refreshQueue = true;
                tmr.Interval = 15000; //15 seconds
            }

            VisibleSearchProgress(false, screen);
            //enable again
            tmr.Tag = "";
            tmr.Enabled = true;
        }

        private object AddFavorite(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            if (fromCurrent[screen])
                return null;
            favoritesSearchedList[screen].Add(searchedArea[screen]);
            favoritesList[screen].Add(param[1].ToString());
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Favorite Location Weather Added:\n" + favoritesList[screen][favoritesList[screen].Count - 1], 3000));
            UpdatedFavorites(screen);
            return null;
        }

        private object RemoveFavorite(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            int index = Convert.ToInt32(param[1]);
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Favorite Location Weather Removed:\n" + favoritesList[screen][index]));
            favoritesList[screen].RemoveAt(index);
            favoritesSearchedList[screen].RemoveAt(index);
            UpdatedFavorites(screen);
            return null;
        }

        private void UpdatedFavorites(int screen)
        {
            //push the updated favorites list
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Favorites.List" + screen.ToString(), favoritesList[screen], true);
        }

        private object ShowFavorite(OpenMobile.Command command, object[] param, out bool results)
        {
            results = true;
            int screen = GetScreenFromParam(param[0]);
            int index = Convert.ToInt32(param[1]);
            if(index == 1)
                OM.Host.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { searchedArea[screen], screen });
            else
                OM.Host.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { favoritesSearchedList[screen][index], screen });
            return null;
        }

        private object FavoritesList(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            return favoritesList[Convert.ToInt32(dataSource.NameLevel3.Remove(0, 4))];
        }



        private int GetScreenFromParam(object param)
        {
            return Convert.ToInt32(param);
        }

        private object RefreshWeather(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            //manualRefresh = true;
            if (param == null)
            {
                for (int i = 0; i < OM.Host.ScreenCount; i++)
                {
                    fromRefresh[i] = true;
                    tmr[i].Enabled = false;
                    tmr[i].Interval = 500;
                    tmr[i].Enabled = true;
                }
            }
            else
            {
                for (int i = 0; i < param.Length; i++)
                {
                    int screen = Convert.ToInt32(param[i]);
                    fromRefresh[screen] = true;
                    tmr[screen].Enabled = false;
                    tmr[screen].Interval = 500;
                    tmr[screen].Enabled = true;
                }
            }
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
            ////save
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                if (!fromCurrent[i])
                    StoredData.Set(this, String.Format("OMDSWeather.Screen{0}.Searched", i.ToString()), searchedArea[i]);
                else
                    StoredData.Set(this, String.Format("OMDSWeather.Screen{0}.Searched", i.ToString()), "Current");
                //now save down the favorites
                for (int j = 0; j < favoritesList[i].Count; j++)
                {
                    //favoritesList[i].Add(StoredData.Get(this, String.Format("Screen{0}.Favorites.{1}", i.ToString(), j.ToString())));
                    StoredData.Set(this, String.Format("OMDSWeather.Screen{0}.Favorites.{1}", i.ToString(), j.ToString()), favoritesList[i][j]);
                }
                //now save down the favorites searched terms
                for (int j = 0; j < favoritesSearchedList[i].Count; j++)
                {
                    StoredData.Set(this, String.Format("OMDSWeather.Screen{0}.FavoritesSearched.{1}", i.ToString(), j.ToString()), favoritesSearchedList[i][j]);
                }
                StoredData.Set(this, String.Format("OMDSWeather.Screen{0}.Favorites.Count", i.ToString()), favoritesList[i].Count.ToString());
            }
            if (sqlConn != null)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                sqlConn.Dispose();
            }
        }

        

        private void poller()
        {
            //attempt updates of all polledSearchWeather...
            refreshRate = StoredData.GetInt(this, "OMDSWeather.RefreshRate") * 1000 * 60;
            pollsw.Start();
            while (stillpolling)
            {
                if ((pollsw.ElapsedMilliseconds >= refreshRate) || (manualRefresh) || ((refreshQueue) && (pollsw.ElapsedMilliseconds >= 15000)))
                {
                    manualRefresh = false;
                    refreshQueue = false;
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        VisibleSearchProgress(true, i);
                    if (OM.Host.InternetAccess)
                    {
                        Dictionary<string, object> Results = new Dictionary<string, object>();
                        Dictionary<string, string> searchResults = new Dictionary<string, string>();
                        Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", OM.Host.CurrentLocation);
                        if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
                        {
                            if (loc.Country == "US")
                                searchResults = SearchArea(loc.Zip);
                            else
                                searchResults = SearchArea(loc.City + ", " + loc.State);

                            if (searchResults.Keys.ElementAt(0) == "AreaFound")
                            {
                                Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                                Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"], true));
                                Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                                Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                                GetSearchedForecast(searchResults["ForecastHtml"], Results);
                                Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
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
                        }
                        else
                        {
                            //something is blank in loc

                        } 
                        for (int i = 0; i < theHost.ScreenCount; i++)
                        {
                            if (!fromCurrent[i])
                            {
                                //we are using the searchedArea instead of current
                                Results.Clear();
                                searchResults.Clear();
                                searchResults = SearchArea(searchedArea[i], i);
                                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                                {
                                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                                    Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"]));
                                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                                    GetSearchedForecast(searchResults["ForecastHtml"], Results);
                                    Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
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
                            }
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + i.ToString(), Results);
                        }
                    }
                    else
                        refreshQueue = true;
                    for (int i = 0; i < theHost.ScreenCount; i++)
                        VisibleSearchProgress(false, i);

                    




                    //if (OM.Host.InternetAccess)
                    //{
                    //    //theHost.CommandHandler.ExecuteCommand("OMGPS;GPS.Location.ReverseGeocode", new object[] { });
                    //    //Location loc = OM.Host.CurrentLocation;
                    //    Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", OM.Host.CurrentLocation);
                    //    for (int i = 0; i < theHost.ScreenCount; i++)
                    //    {
                    //        if (!fromCurrent[i])
                    //        {
                    //            if (searchedArea[i] != null)
                    //            {
                    //                Dictionary<string, object> Results = new Dictionary<string, object>();
                    //                Dictionary<string, string> searchResults = SearchArea(searchedArea[i], i);
                    //                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    //                {
                    //                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                    //                    Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"]));
                    //                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                    //                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                    //                    GetSearchedForecast(searchResults["ForecastHtml"], Results);
                    //                    Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
                    //                }
                    //                else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                    //                {
                    //                    for (int j = 0; j < searchResults.Count; j++)
                    //                        Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                    //                }
                    //                else
                    //                {
                    //                    //problem with connections or no results
                    //                    refreshQueue = true;
                    //                }
                    //                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + i.ToString(), Results);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //current
                    //            Dictionary<string, object> Results = new Dictionary<string, object>();
                    //            Dictionary<string, string> searchResults = new Dictionary<string, string>();
                    //            if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
                    //            {
                    //                if (loc.Country == "US")
                    //                    searchResults = SearchArea(loc.Zip, i);
                    //                else
                    //                    searchResults = SearchArea(loc.City + ", " + loc.State, i);

                    //                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    //                {
                    //                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                    //                    Results.Add("Temp", GetCurrentTemp(searchResults["SearchHtml"]));
                    //                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                    //                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                    //                    GetSearchedForecast(searchResults["ForecastHtml"], Results);
                    //                    Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
                    //                }
                    //                else if (searchResults.Keys.ElementAt(0) == "MultipleResults")
                    //                {
                    //                    for (int j = 0; j < searchResults.Count; j++)
                    //                        Results.Add(searchResults.Keys.ElementAt(j), searchResults[searchResults.Keys.ElementAt(j)]);
                    //                    //searchedDictionary[screen].Add(searchResults.Keys.ElementAt(i), searchResults[searchResults.Keys.ElementAt(i)]);
                    //                }
                    //                else
                    //                {
                    //                    //problem with connections or no results
                    //                    refreshQueue = true;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                //something is blank in loc

                    //            }
                    //            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched.List" + i.ToString(), Results);
                    //        }


                    //        //if (searchString.Keys.Contains(i))
                    //        //{
                    //        //    //perform update

                    //        //    polledSearchedWeather[i] = WeatherInfo.SearchArea(searchString[i].ToString(), i);
                    //        //    if (polledSearchedWeather[i].Keys.ElementAt(0) == "AreaFound")
                    //        //    {
                    //        //        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + i.ToString() + ".Weather.Searched.Area", polledSearchedWeather[i]["AreaFound"] + "\n(Click To Search Weather)");
                    //        //        WeatherInfo.GetSearchedTemp(GetSearchHTML(i), i);
                    //        //        WeatherInfo.GetSearchedImage(GetSearchHTML(i), i);
                    //        //        WeatherInfo.GetSearchedDescription(GetSearchHTML(i), i);
                    //        //        WeatherInfo.GetSearchedForecast(GetForecastHTML(i), i);
                    //        //    }
                    //        //    else
                    //        //    {
                    //        //        refreshQueue = true;
                    //        //    }
                    //        //}
                    //    }



                    //    //Location loc = OM.Host.CurrentLocation;
                    //    //if ((loc.Latitude.ToString() != "") && (loc.Longitude.ToString() != ""))
                    //    //{
                    //    //    bool found = false;
                    //    //    string state = "";
                    //    //    string zip = "";
                    //    //    string city = "";
                    //    //    string country = "";
                    //    //    try
                    //    //    {
                    //    //        if (System.IO.File.Exists(OpenMobile.Path.Combine(theHost.DataPath, "OMGPS"))) //OMGPS db exists, attempt a lookup in it first
                    //    //        {
                    //    //            using (SqliteConnection con = new SqliteConnection(@"Data Source=" + OpenMobile.Path.Combine(theHost.DataPath, "OMGPS") + ";Pooling=false;synchronous=0;temp_store=2;count_changes=0;journal_mode=WAL"))
                    //    //            {
                    //    //                if (con.State != System.Data.ConnectionState.Open)
                    //    //                    con.Open();
                    //    //                using (SqliteCommand command = new SqliteCommand("SELECT * FROM GeoNames WHERE Latitude = '" + loc.Latitude.ToString() + "' AND Longitude = '" + loc.Longitude.ToString() + "'", con))
                    //    //                {
                    //    //                    using (SqliteDataReader reader = command.ExecuteReader())
                    //    //                    {
                    //    //                        if (reader.HasRows)
                    //    //                        {
                    //    //                            found = true;
                    //    //                       }
                    //    //                    }
                    //    //                }
                    //    //            }
                    //    //        }
                    //    //    }
                    //    //    catch { }
                    //    //    if (!found) //attempt a lookup on net if possible
                    //    //    {
                    //    //        if (OM.Host.InternetAccess)
                    //    //        {
                    //    //            //use geonames website
                    //    //            try
                    //    //            {
                    //    //                string html;
                    //    //                using (WebClient wc = new WebClient())
                    //    //                {
                    //    //                    //http://api.geonames.org/findNearbyPostalCodes?lat=47&lng=9&username=demo
                    //    //                    //http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=36&lng=-79.08
                    //    //                    html = wc.DownloadString("http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=" + loc.Latitude.ToString() + "&lng=" + loc.Longitude.ToString());
                    //    //                }
                    //    //                if (html != "{\"postalCodes\": []}")
                    //    //                {
                    //    //                    html = html.Remove(0, html.IndexOf("postalCode") + 11);
                    //    //                    zip = html.Remove(0, html.IndexOf("postalCode") + 11);
                    //    //                    zip = zip.Remove(0, zip.IndexOf("\"") + 1);
                    //    //                    zip = zip.Substring(0, zip.IndexOf("\""));
                    //    //                    country = html.Remove(0, html.IndexOf("countryCode") + 12);
                    //    //                    country = country.Remove(0, country.IndexOf("\"") + 1);
                    //    //                    country = country.Substring(0, country.IndexOf("\""));
                    //    //                    city = html.Remove(0, html.IndexOf("placeName") + 10);
                    //    //                    city = city.Remove(0, city.IndexOf("\"") + 1);
                    //    //                    city = city.Substring(0, city.IndexOf("\""));
                    //    //                    state = html.Remove(0, html.IndexOf("adminName1") + 11);
                    //    //                    state = state.Remove(0, state.IndexOf("\"") + 1);
                    //    //                    state = state.Substring(0, state.IndexOf("\""));
                    //    //                }
                    //    //            }
                    //    //            catch { }
                    //    //        }
                    //    //    }
                    //    //    //here we take the values from above and do a search...
                    //    //    Dictionary<string, string> searchResults;
                    //    //    if ((state != "") && (zip != "") && (city != "") && (country != ""))
                    //    //    {
                    //    //        if (country == "US") //zip
                    //    //        {
                    //    //            searchResults = WeatherInfo.SearchArea(zip);
                    //    //        }
                    //    //        else //city+state
                    //    //        {
                    //    //            searchResults = WeatherInfo.SearchArea(city + ", " + state);
                    //    //        }
                    //    //        if (searchResults.Keys.ElementAt(0) == "AreaFound")
                    //    //        {
                    //    //            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Current.Current.Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                    //    //            WeatherInfo.GetCurrentTemp(searchResults["SearchHtml"]);
                    //    //            WeatherInfo.GetCurrentImage(searchResults["SearchHtml"]);
                    //    //            WeatherInfo.GetCurrentDescription(searchResults["SearchHtml"]);
                    //    //            WeatherInfo.GetCurrentForecast(searchResults["ForecastHtml"]);
                    //    //        }
                    //    //        else
                    //    //        {
                    //    //            refreshQueue = true;
                    //    //        }
                    //    //    }
                    //    //}





                    //}
                    //else
                    //{
                    //    refreshQueue = true;
                    //}
                    //for (int i = 0; i < theHost.ScreenCount; i++)
                    //    VisibleSearchProgress(false, i);

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
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Info.Status" + screen.ToString(), "Searching");
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressImage", true);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressLabel", true);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressBackground", true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Info.Status" + screen.ToString(), "Done Searching");
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressImage", false);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressLabel", false);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Visible.SearchProgressBackground", false);
            }
        }

        private object SearchCurrentWeather(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            Dictionary<string, object> Results = new Dictionary<string, object>();
            fromCurrent[screen] = true;
            VisibleSearchProgress(true, screen);
            //here we need to get the city,state,zip from OMGPS...
            //theHost.CommandHandler.ExecuteCommand("OMGPS;GPS.Location.ReverseGeocode");
            //Location loc = OM.Host.CurrentLocation;
            //*** Use below for new frameworks using OMGPS3_*
            Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", OM.Host.CurrentLocation);
            //*** Use above for new frameworks using OMGPS3_*
            Dictionary<string, string> searchResults = new Dictionary<string, string>();
            if ((loc.Zip != "") && (loc.State != "") && (loc.Country != "") && (loc.City != ""))
            {
                if (loc.Country == "US")
                    searchResults = SearchArea(loc.Zip, screen);
                else
                    searchResults = SearchArea(loc.City + ", " + loc.State, screen);

                if (searchResults.Keys.ElementAt(0) == "AreaFound")
                {
                    Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                    Results.Add("Temp", GetCurrentTemp(searchResults["SearchHtml"]));
                    Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                    Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                    GetSearchedForecast(searchResults["ForecastHtml"], Results);
                    Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
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
            searchedArea[screen] = param[0].ToString();
            fromCurrent[screen] = false;
            Dictionary<string, object> Results = new Dictionary<string, object>();
            VisibleSearchProgress(true, screen);
            //Dictionary<string, string> searchResults = WeatherInfo.SearchArea(param[0].ToString(), Convert.ToInt32(param[1]));
            Dictionary<string, string> searchResults = SearchArea(param[0].ToString(), screen);
            if (searchResults.Keys.ElementAt(0) == "AreaFound")
            {
                Results.Add("Area", searchResults["AreaFound"] + "\n(Click To Search Weather)");
                //searchedArea[screen] = Results["Area"].ToString();
                Results.Add("Temp", GetSearchedTemp(searchResults["SearchHtml"]));
                Results.Add("Image", GetSearchedImage(searchResults["SearchHtml"]));
                Results.Add("Description", GetSearchedDescription(searchResults["SearchHtml"]));
                GetSearchedForecast(searchResults["ForecastHtml"], Results);
                Results.Add("UpdatedTime", getUpdatedTime(searchResults["SearchHtml"]));
                Results.Add("RealSearchedArea", searchedArea[screen]);
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

        private string getUpdatedTime(string html)
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

        private string GetSearchedTemp(string html, bool updateCurrent = false)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature\">") + 20);
            string degreecheck = html.Substring(0, html.IndexOf(">"));
            html = html.Remove(0, html.IndexOf(">") + 1);
            string temp = DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
            if (updateCurrent)
            {
                if (currentWeatherNotificationImage != null && currentWeatherNotification != null)
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
            }
            return temp;
        }

        private string GetCurrentTemp(string html)
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

        private imageItem GetSearchedImage(string html)
        {
            try
            {
                html = html.Remove(0, html.IndexOf("wx-data-part wx-first"));
                html = html.Remove(0, html.IndexOf("src=") + 5);
                string imageurl = html.Substring(0, html.IndexOf(" ") - 1);

                imageItem image = new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl));
                return image;
            }
            catch { }
            return imageItem.NONE;
        }

        private string GetSearchedDescription(string html)
        {
            html = html.Remove(0, html.IndexOf("wx-phrase") + 12);
            if (html.Contains("weather-phrase"))
                html = html.Remove(0, html.IndexOf(">") + 1);
            string currentdescstr = html.Substring(0, html.IndexOf("<"));
            return currentdescstr;
        }

        private void GetSearchedForecast(string html, Dictionary<string, object> Results)
        {
            html = html.Remove(0, html.IndexOf("wx-temperature-") + 3);
            string degreecheck = html.Substring(0, html.IndexOf(">") - 1);
            int i = 1;
            while (html.Contains("wx-daypart"))
            {
                if (i == 11)
                    break;
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

        private Dictionary<string, string> SearchArea(string searchText, int screen = -1)
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
                        string searchhtml = "";
                        string forecasthtml = "";
                        string currentUrl = "";
                        HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                        using (HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse())
                        {
                            currentUrl = myWebResponse.ResponseUri.ToString();
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(myWebResponse.GetResponseStream()))
                            {
                                searchhtml = sr.ReadToEnd();
                            }
                        }
                        //string searchhtml = wc.DownloadString(url);
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
                            //string newurl = searchhtml.Remove(0, searchhtml.IndexOf("/weather/tenday/"));
                            //url = "http://www.weather.com" + newurl.Substring(0, newurl.IndexOf(" ") - 1);
                            url = currentUrl.Replace("today", "tenday");
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

        
        private void Alerts(string html, string area, int screen = -1)
        {
            int alertCount = 0;
            if (html.Contains("wx-show-alert-links")) //more than 1 alert
            {
                html = html.Remove(0, html.IndexOf("wx-show-alert-links"));
                while (html.Contains("wx-alert-link-"))
                {
                    html = html.Remove(0, html.IndexOf("wx-alert-link-") + 10);
                    string alertHtml = html.Remove(0, html.IndexOf("a href") + 8);
                    Color overlayColor = Color.Red;
                    if (alertHtml.Contains("pollen"))
                        overlayColor = Color.Yellow;
                    string alertURL = "www.weather.com" + alertHtml.Substring(0, alertHtml.IndexOf(" ") - 1);
                    string alertDescription = alertHtml.Substring(0, alertHtml.IndexOf("</a>"));
                    alertDescription = alertDescription.Remove(0, alertDescription.LastIndexOf(">") + 1);
                    string alertTime = "";
                    if (html.Contains("wx-alert-time"))
                    {
                        html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                        alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                    }
                    Notification newNotification = new Notification(this, "Weather Alerts", OM.Host.getPluginImage(this, "Images|Icon-Weather_Conditions2").image.Copy().Overlay(overlayColor), OM.Host.getPluginImage(this, "Images|Icon-Weather_Conditions2").image.Copy().Overlay(overlayColor), "**** " + alertDescription + " ****", area + alertTime);
                    if ((screen == -1) || (fromCurrent[screen]))
                        newNotification.Tag = "Current";
                    else
                        newNotification.Tag = "";
                    if (screen == -1)
                    {
                        for (int i = 0; i < OM.Host.ScreenCount; i++)
                        {
                            AlertAddChange(i, newNotification, alertCount);
                        }
                    }
                    else
                    {
                        if (fromCurrent[screen])
                            AlertAddChange(screen, newNotification, alertCount);
                        else
                        {
                            if (alertCount == 0)
                            {
                                //get the starting point of the "non current" notifications
                                for (int i = 0; i < alertNotifications[screen].Count; i++)
                                {
                                    if (alertNotifications[screen][i].Tag.ToString() == "Current")
                                    {
                                        alertCount += 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            AlertAddChange(screen, newNotification, alertCount);
                        }
                    }
                    alertCount += 1;

                    //if (html.Contains("local_alert_list_today"))
                    //{
                    //    string alertURL = html.Substring(0, html.IndexOf("local_alert_list_today"));
                    //    alertURL = html.Remove(0, alertURL.LastIndexOf("a href=") + 8);
                    //    alertURL = "www.weather.com" + alertURL.Substring(0, alertURL.IndexOf(" ") - 1);
                    //    html = html.Remove(0, html.IndexOf("local_alert_list_today") + 24);
                    //    string alertDescription = html.Substring(0, html.IndexOf("<"));
                    //    string alertTime = "";
                    //    if (html.Contains("wx-alert-time"))
                    //    {
                    //        html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                    //        alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                    //    }
                    //    if (alertCount < AlertNotifications.Count)
                    //    {
                    //        AlertNotifications[alertCount].Header = "**** " + alertDescription + " **** ";
                    //        AlertNotifications[alertCount].Text = area + alertTime;
                    //    }
                    //    else
                    //    {
                    //        AlertNotifications.Add(new Notification(this, "Weather Alerts", OM.Host.getPluginImage(this, "Images|Alert.gif").image, OM.Host.getPluginImage(this, "Images|Alert.gif").image, "**** " + alertDescription + " ****", area + alertTime));
                    //        if (screen == -1)
                    //            OM.Host.UIHandler.AddNotification(AlertNotifications[alertCount]);
                    //        else
                    //            OM.Host.UIHandler.AddNotification(screen, AlertNotifications[alertCount]);
                    //    }
                    //    alertCount += 1;
                    //}
                    ////currentWeatherNotification = new Notification(Notification.Styles.IconOnly, ibp, "CurrentWeatherNotification", DateTime.Now, null, currentWeatherNotificationImage, "CurrentWeatherNotification", "");
                    ////currentWeatherNotification.State = Notification.States.Active;
                    ////currentWeatherNotification.IconSize_Width = currentWeatherNotificationImage.Width;
                    ////OM.Host.UIHandler.AddNotification(currentWeatherNotification);
                }
            }
            else if (html.Contains("wx-alert-text")) //1 alert
            {

                html = html.Remove(0, html.IndexOf("wx-alert-text"));

                string alertHtml = html.Remove(0, html.IndexOf("a href") + 8);
                Color overlayColor = Color.Red;
                if (alertHtml.Contains("pollen"))
                    overlayColor = Color.Yellow;
                    string alertURL = "www.weather.com" + alertHtml.Substring(0, alertHtml.IndexOf(" ") - 1);
                    string alertDescription = alertHtml.Substring(0, alertHtml.IndexOf("</a>"));
                    alertDescription = alertDescription.Remove(0, alertDescription.LastIndexOf(">") + 1);
                    string alertTime = "";
                    if (html.Contains("wx-alert-time"))
                    {
                        html = html.Remove(0, html.IndexOf("wx-alert-time") + 15);
                        alertTime = " -- " + html.Substring(0, html.IndexOf("<"));
                    }
                    Notification newNotification = new Notification(this, "Weather Alerts", OM.Host.getPluginImage(this, "Images|Icon-Weather_Conditions2").image.Copy().Overlay(overlayColor), OM.Host.getPluginImage(this, "Images|Icon-Weather_Conditions2").image.Copy().Overlay(overlayColor), "**** " + alertDescription + " ****", area + alertTime);
                    if ((screen == -1) || (fromCurrent[screen]))
                        newNotification.Tag = "Current";
                    else
                        newNotification.Tag = "";
                    if (screen == -1)
                    {
                        for (int i = 0; i < OM.Host.ScreenCount; i++)
                        {
                            AlertAddChange(i, newNotification, alertCount);
                        }
                    }
                    else
                    {
                        if(fromCurrent[screen])
                            AlertAddChange(screen, newNotification, alertCount);
                        else
                        {
                            if (alertCount == 0)
                            {
                                //get the starting point of the "non current" notifications
                                for (int i = 0; i < alertNotifications[screen].Count; i++)
                                {
                                    if (alertNotifications[screen][i].Tag.ToString() == "Current")
                                    {
                                        alertCount += 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            AlertAddChange(screen, newNotification, alertCount);
                        }
                    }
                    alertCount += 1;
                

            }
            else //no alerts
            {

            }

            if ((screen == -1) || (fromCurrent[screen]))
            {
                //current notifications added, remove the rest of the current notifications
                for (int i = 0; i < OM.Host.ScreenCount; i++)
                {
                    for (int j = alertCount; j < alertNotifications[i].Count; j++)
                    {
                        if (alertNotifications[i][j].Tag.ToString() == "Current")
                        {
                            //old current notification, remove this
                            OM.Host.UIHandler.RemoveNotification(i, alertNotifications[i][j]);
                            alertNotifications[i].RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
            else
            {
                //remove all the rest of the notifications
                for (int i = alertCount; i < alertNotifications[screen].Count; i++)
                {
                    OM.Host.UIHandler.RemoveNotification(screen, alertNotifications[screen][i]);
                    alertNotifications[i].RemoveAt(i);
                    i--;
                }
            }

            //List<int> toRemove = new List<int>();
            //if (alertCount < alertNotifications[screen].Count)
            //{
            //    while (alertCount < alertNotifications[screen].Count)
            //    {
            //        OM.Host.UIHandler.RemoveNotification(screen, alertNotifications[screen][alertCount]);
            //        toRemove.Add(alertCount);
            //        alertCount += 1;
            //    }
            //}
            //if (toRemove.Count > 0)
            //{
            //    for (int i = 0; i < toRemove.Count; i++)
            //    {
            //        alertNotifications[screen].RemoveAt(toRemove[i]);
            //    }
            //}
        }

        private void AlertAddChange(int screen, Notification newNotification, int alertCount)
        {
            if (alertNotifications[screen].Count > alertCount)
            {
                alertNotifications[screen][alertCount].Header = newNotification.Header;
                alertNotifications[screen][alertCount].Text = newNotification.Text;
            }
            else
            {
                alertNotifications[screen].Add(newNotification);
                alertNotifications[screen][alertNotifications[screen].Count - 1].ClearAction += new Notification.NotificationAction(OMDSWeather_ClearAction);
                if (screen == -1)
                    OM.Host.UIHandler.AddNotification(alertNotifications[screen][alertNotifications[screen].Count - 1]);
                else
                    OM.Host.UIHandler.AddNotification(screen, alertNotifications[screen][alertNotifications[screen].Count - 1]);
            }
        }

        void OMDSWeather_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            alertNotifications[screen].Remove(notification);
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
            {
                try
                {
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), false).ToString();
                }
                catch
                {
                    newtemp = "?";
                }
            }
            else if (newdegree == "Celcius")
            {
                try
                {
                    newtemp = Globalization.convertToLocalTemp(Convert.ToDouble(temp), true).ToString();
                }
                catch
                {
                    newtemp = "?";
                }
            }
            if (newtemp.IndexOf(".") >= 0)
            {
                string degreesymbol2 = degreesymbol + "F";
                if (newtemp.IndexOf("C") >= 0)
                    degreesymbol2 = degreesymbol + "C";
                newtemp = newtemp.Remove(newtemp.IndexOf("."));
                newtemp = newtemp + degreesymbol2;
            }
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
                        Dictionary<string, string> searchResults = SearchArea(searchText, screen);
                        if (searchResults.Keys.ElementAt(0) == "AreaFound")
                        {
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Area", searchResults["AreaFound"]);
                            if (searchString.Keys.Contains(screen))
                                searchString[screen] = searchText;
                            else
                                searchString.Add(screen, searchText);
                            polledSearchedWeather[screen] = searchResults;
                            GetSearchedTemp(GetSearchHTML(screen));
                            GetSearchedImage(GetSearchHTML(screen));
                            GetSearchedDescription(GetSearchHTML(screen));
                            //WeatherInfo.GetSearchedForecast(GetForecastHTML(screen), screen);
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
            List<string> refreshRates = new List<string>() { "5", "15", "30", "45", "60", "120" };
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

}