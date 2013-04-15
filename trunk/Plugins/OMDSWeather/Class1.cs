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
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OpenMobile;
using OpenMobile.Data;

namespace OMDSWeather
{
    public class Class1 : IBasePlugin, IDataSource
    {
        public delegate void UpdatedSearchedWeatherEventHandler(Dictionary<int, Dictionary<string, string>> polledsearchedweather);
        public static event UpdatedSearchedWeatherEventHandler UpdatedSearchedWeather;
        Dictionary<int, string> polling;
        Dictionary<int, Dictionary<string, string>> polledSearchedWeather;
        static IPluginHost theHost;
        //PluginSettings ps;
        Dictionary<int, Dictionary<string, string>> polledWeather;
        Dictionary<int, string> searchString;
        Settings settings;
        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            searchString = new Dictionary<int, string>();
            polling = new Dictionary<int, string>();
            polledSearchedWeather = new Dictionary<int, Dictionary<string, string>>();
            polledWeather = new Dictionary<int, Dictionary<string, string>>();
            //create the dataSources for searching weather
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "SearchedWeather", "SearchWeather", 0, DataSource.DataTypes.raw, SearchWeather, ""));
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen"+i.ToString(), "Weather", "Searched.Temp", DataSource.DataTypes.raw, "Temp for the searched/last searched position per screen"), "");
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Weather", "Searched" + i.ToString(), "SearchedImage", DataSource.DataTypes.raw, "Image for the searched/last searched position per screen"), imageItem.NONE);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen"+i.ToString(),"Weather", "Searched.Description", DataSource.DataTypes.raw, "Description for the searched/last searched position per screen"), "");
                for (int j = 1; j < 6; j++)
                {
                    //create all the forecast sources per screen...
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastDay" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen"+i.ToString(), "Weather", "Searched.ForecastImage" + j.ToString(), DataSource.DataTypes.raw, ""), OM.Host.getSkinImage("Icons|Icon-OM"));
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastDescription" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastHigh" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Searched.ForecastLow" + j.ToString(), DataSource.DataTypes.raw, ""), "");
                }
                polledSearchedWeather.Add(i, new Dictionary<string, string>());
            }
            //create the dataSources for current weather

            return eLoadStatus.LoadSuccessful;
        }

        private void polltmr_tick(object sender, EventArgs e)
        {
            //attempt updates of all polledSearchWeather...

        }

        private object SearchWeather(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            //get results of searching for the text
            Dictionary<string, string> searchResults = WeatherInfo.SearchArea(param[0].ToString());
            if (searchResults.Keys.ElementAt(0) == "AreaFound")
            {
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
                return null;
            }
            else
                return searchResults;
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

        public Settings loadSettings()
        {
            if (settings == null)
                settings = new Settings("Weather Settings");
            List<string> degrees = new List<string>() { "Fahrenheit", "Celcius" };
            StoredData.Get(this, "OMDSWeather.Degree");
            if (StoredData.Get(this, "OMDSWeather.Degree") == "")
                StoredData.Set(this, "OMDSWeather.Degree", "Fahrenheit");
            settings.Add(new Setting(SettingTypes.MultiChoice, "OMDSWeather.Degree", "", "Fahrenheit / Celcius", degrees, degrees, StoredData.Get(this, "OMDSWeather.Degree")));
            List<string> refreshRates = new List<string>() { "5", "15", "30", "60" };
            if (StoredData.Get(this, "OMDSWeather.RefreshRate") == "")
                StoredData.Set(this, "OMDSWeather.RefreshRate", "5");
            settings.Add(new Setting(SettingTypes.MultiChoice, "OMDSWeather.RefreshRate", "", "Refresh Rate (Minutes)", refreshRates, refreshRates, StoredData.Get(this, "OMDSWeather.RefreshRate")));
            settings.OnSettingChanged += new SettingChanged(setting_changed);
            return settings;
        }

        private void setting_changed(int screen, Setting s)
        {
            StoredData.Set(this, s.Name, s.Value);
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
            //
        }

    }

    public class WeatherInfo
    {
        public static IBasePlugin ibp;

        public static void GetSearchedTemp(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("wx-temp\">") + 15);
            string degreecheck = html.Substring(0, html.IndexOf(">"));
            html = html.Remove(0, html.IndexOf(">") + 1);
            //return DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck);
            //just update the dataSource from here?
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen"+screen.ToString()+ ".Weather.Searched.Temp", DegreeCheck(html.Substring(0, html.IndexOf("<")), degreecheck));

        }

        public static void GetSearchedImage(string html, int screen)
        {
            //return an OImage
            html = html.Remove(0, html.IndexOf("wx-weather-icon") + 22);
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
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched" + screen.ToString() + ".SearchedImage", new imageItem(OpenMobile.Net.Network.imageFromURL(imageurl)));
                return;
            }
            catch { } //wrapped in case of timeout or something else....
            //IF CAN'T GET FROM NET AND DOESN'T EXIST IN SQLITE, RETURN A BLANK IMAGE
            //return imageItem.NONE;
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Weather.Searched" + screen.ToString() + ".SearchedImage", imageItem.NONE);
        }

        public static void GetSearchedDescription(string html, int screen)
        {
            html = html.Remove(0, html.IndexOf("weather-phrase") + 16);
            string currentdescstr = html.Substring(0, html.IndexOf("<"));
            //return html.Substring(0, html.IndexOf("<"));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSWeather;Screen" + screen.ToString() + ".Weather.Searched.Description", html.Substring(0, html.IndexOf("<")));
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

        public static Dictionary<string, string> SearchArea(string searchText)
        {
            Dictionary<string, string> wi = new Dictionary<string, string>();
            string url = "http://www.weather.com/search/enhancedlocalsearch?where=" + searchText;
            url = url.Replace(",", "+");
            url = url.Replace(" ", "+");
            using (System.Net.WebClient wc = new System.Net.WebClient())
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
            //if(SearchedArea!=null)
            //	SearcheArea(wi);
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
            if (currentdegree != newdegree)
            {
                if (currentdegree == "Fahrenheit")
                    newtemp = OpenMobile.Framework.Math.Calculation.CtoF(Convert.ToDouble(temp)).ToString() + degreesymbol + "F";
                else if (currentdegree == "Celcius")
                    newtemp = OpenMobile.Framework.Math.Calculation.FtoC(Convert.ToDouble(temp)).ToString() + degreesymbol + "C";
            }
            else
            {
                if (currentdegree == "Fahrenheit")
                    newtemp = newtemp + degreesymbol + "F";
                else
                    newtemp = newtemp + degreesymbol + "C";
            }
            return newtemp;
        }

    }
}
