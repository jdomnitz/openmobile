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
using OpenMobile.Graphics;

namespace OMDSNFL
{
    public class Class1 : IBasePlugin, IDataSource
    {

        //Dictionary<int, string> polling;

        static IPluginHost theHost;
        Settings settings;
        //Thread polling;
        private bool stillpolling;
        //bool refreshQueue;
        //bool manualRefresh;
        int refreshRate;

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            stillpolling = true;
            //refreshQueue = false;
            //manualRefresh = false;

            //polling = new Thread(poller);
            //polling.Start();


            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "NFL", "Games", "Refresh", RefreshAllGames, 1, false, "Refreshes Data For All NFL Games {param = screen}"));
            OM.Host.CommandHandler.AddCommand(new Command(this.pluginName, "NFL", "News", "Refresh", RefreshAllNews, 1, false, "Refreshes News For NFL {param = screen}"));

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                //each screen gets its own Dictionary<string,string> of data


                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "NFL", "Games", "List" + i.ToString(), DataSource.DataTypes.raw, "Searched Weather Results"));
                theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "NFL", "News", "List" + i.ToString(), DataSource.DataTypes.raw, "Searched Weather Results"));

                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "NFL", "Visible", "ProgressImage" + i.ToString(), DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressLabel", DataSource.DataTypes.binary, ""), false);
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "Screen" + i.ToString(), "Weather", "Visible.SearchProgressBackground", DataSource.DataTypes.binary, ""), false);

            }

            //power event
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);

            // Initialize default data
            StoredData.SetDefaultValue(this, "OMDSNFL.RefreshRate", 5);

            //tmr = new OpenMobile.Timer[OM.Host.ScreenCount];


            //OM.Host.DataHandler.RemoveDataSource("Provider;name.name.name.name");

            return eLoadStatus.LoadSuccessful;
        }

        private object RefreshAllNews(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            DisplayAllNews(screen);
            return null;
        }

        private object RefreshAllGames(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            DisplayAllGames(screen);
            return null;
        }

        private object RefreshSpecificGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);

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
                //polling = new Thread(poller);
                //polling.Start();
            }
        }

        private void Killing()
        {
            //stillpolling = false;
            //if (polling != null)
            //    if (polling.ThreadState == ThreadState.Running)
            //        polling.Join();
        }

        private void VisibleSearchProgress(bool visible, int screen)
        {
            if (visible)
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressImage" + screen.ToString(), true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressLabel" + screen.ToString(), true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressBackground" + screen.ToString(), true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressImage" + screen.ToString(), false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressProgressLabel" + screen.ToString(), false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Visible.ProgressProgressBackground" + screen.ToString(), false);
            }
        }

        private void DisplayAllGames(int screen)
        {
            Dictionary<int, Dictionary<string, string>> Results = new Dictionary<int, Dictionary<string, string>>();
            string html = "";
            VisibleSearchProgress(true, screen);
            using (WebClient wc = new WebClient())
            {
                html = wc.DownloadString("http://scores.espn.go.com/nfl/scoreboard");
            }
            //parse it now
            int gameCount = 0;
            while (html.Contains("games-date"))
            {
                html = html.Remove(0, html.IndexOf("games-date") + 12);
                string gameHtml = "";
                if (html.Contains("games-date"))
                    gameHtml = html.Substring(0, html.IndexOf("games-date"));
                else
                    gameHtml = html;
                //gameHtml = gameHtml.Remove(0,html.IndexOf("games-date") + 12);
                string gameDate = gameHtml.Substring(0, gameHtml.IndexOf("</"));
                while (gameHtml.Contains("game-header"))
                {
                    //start of new game data
                    //Results.Add(gameCount, new Dictionary<string, string>());
                    Dictionary<string, string> gameResults = new Dictionary<string, string>();
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("-statusText") + 13);
                    string gametime = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("<p>") + 3);
                    string gameChannel = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    //away team
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("team-name") + 12);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("\">") + 2);
                    string gameTeamAway = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("record") + 8);
                    string gameTeamAwayRecord = gameHtml.Substring(0, gameHtml.IndexOf(",")) + ")";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aScore1") + 9);
                    string gameTeamAwayQuarterOne = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAwayQuarterOne == "&nbsp;") gameTeamAwayQuarterOne = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aScore2") + 9);
                    string gameTeamAwayQuarterTwo = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAwayQuarterTwo == "&nbsp;") gameTeamAwayQuarterTwo = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aScore3") + 9);
                    string gameTeamAwayQuarterThree = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAwayQuarterThree == "&nbsp;") gameTeamAwayQuarterThree = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aScore4") + 9);
                    string gameTeamAwayQuarterFour = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAwayQuarterFour == "&nbsp;") gameTeamAwayQuarterFour = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aScore5") + 9);
                    string gameTeamAwayQuarterOT = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAwayQuarterOT == "&nbsp;") gameTeamAwayQuarterOT = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("aTotal") + 8);
                    string gameTeamAwayTotal = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    //home team
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("team-name") + 12);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("\">") + 2);
                    string gameTeamHome = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("record") + 8);
                    string gameTeamHomeRecord = gameHtml.Substring(0, gameHtml.IndexOf(",")) + ")";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hScore1") + 9);
                    string gameTeamHomeQuarterOne = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHomeQuarterOne == "&nbsp;") gameTeamHomeQuarterOne = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hScore2") + 9);
                    string gameTeamHomeQuarterTwo = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHomeQuarterTwo == "&nbsp;") gameTeamHomeQuarterTwo = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hScore3") + 9);
                    string gameTeamHomeQuarterThree = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHomeQuarterThree == "&nbsp;") gameTeamHomeQuarterThree = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hScore4") + 9);
                    string gameTeamHomeQuarterFour = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHomeQuarterFour == "&nbsp;") gameTeamHomeQuarterFour = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hScore5") + 9);
                    string gameTeamHomeQuarterOT = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHomeQuarterOT == "&nbsp;") gameTeamHomeQuarterOT = "0";
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("hTotal") + 8);
                    string gameTeamHomeTotal = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("-lastPlayText") + 15);
                    string gameLastPlay = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    string gameURL = "";
                    //if (gametime.Contains("Final"))
                    //{
                    //    //game is over, get "box score" url
                    //    gameURL = gameHtml.Substring(0, gameHtml.IndexOf("Box Score") - 2);
                    //    gameURL = "scores.espn.go.com/nfl" + gameURL.Remove(0, gameURL.LastIndexOf("\"") + 1);
                    //}
                    //else if (gametime.Contains("Qtr"))
                    //{

                    //}
                    //else
                    //{
                    //    //game has not started, get "intel" url
                    //    gameURL = gameHtml.Substring(0, gameHtml.IndexOf("Intel") - 2);
                    //    gameURL = "scores.espn.go.com/nfl" + gameURL.Remove(0, gameURL.LastIndexOf("\"") + 1);
                    //}
                    
                    gameResults.Add("Game Date", gameDate);
                    gameResults.Add("Game Time", gametime);
                    gameResults.Add("Game Channel", gameChannel);
                    gameResults.Add("Home Team Name", gameTeamHome);
                    gameResults.Add("Home Team Record", gameTeamHomeRecord);
                    gameResults.Add("Home Team Quarter One", gameTeamHomeQuarterOne);
                    gameResults.Add("Home Team Quarter Two", gameTeamHomeQuarterTwo);
                    gameResults.Add("Home Team Quarter Three", gameTeamHomeQuarterThree);
                    gameResults.Add("Home Team Quarter Four", gameTeamHomeQuarterFour);
                    gameResults.Add("Home Team Quarter OT", gameTeamHomeQuarterOT);
                    gameResults.Add("Home Team Total", gameTeamHomeTotal);
                    gameResults.Add("Away Team Name", gameTeamAway);
                    gameResults.Add("Away Team Record", gameTeamAwayRecord);
                    gameResults.Add("Away Team Quarter One", gameTeamAwayQuarterOne);
                    gameResults.Add("Away Team Quarter Two", gameTeamAwayQuarterTwo);
                    gameResults.Add("Away Team Quarter Three", gameTeamAwayQuarterThree);
                    gameResults.Add("Away Team Quarter Four", gameTeamAwayQuarterFour);
                    gameResults.Add("Away Team Quarter OT", gameTeamAwayQuarterOT);
                    gameResults.Add("Away Team Total", gameTeamAwayTotal);
                    gameResults.Add("Last Play", gameLastPlay);
                    gameResults.Add("Game Url", gameURL);
                    Results.Add(gameCount, gameResults);
                    gameCount += 1;
                }
            }
            //push the new Results now
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Games.List" + screen.ToString(), Results);
            VisibleSearchProgress(false, screen);
        }

        private void DisplayAllNews(int screen)
        {
            //Dictionary<int, Dictionary<string, string>> Results = new Dictionary<int, Dictionary<string, string>>();
            string html = "";
            VisibleSearchProgress(true, screen);
            using (WebClient wc = new WebClient())
            {
                html = wc.DownloadString("http://sports-ak.espn.go.com/nfl/");
            }
            //parse it now
            Dictionary<string, string> newsHeadlines = new Dictionary<string, string>();
            html = html.Remove(0, html.IndexOf("mod-tab-content-headlines") + 10);
            html = html.Remove(0, html.IndexOf("mod-tab-content-headlines") + 10);
            html = html.Substring(0, html.IndexOf("mod-tab-content-myheadlines"));
            while (html.Contains("a href"))
            {
                html = html.Remove(0, html.IndexOf("a href") + 8);
                string checkhtml = html.Substring(0, html.IndexOf("</li>"));
                string urlString = html.Substring(0, html.IndexOf(" ") - 1);
                html = html.Remove(0, html.IndexOf("title") + 7);
                string titleString = html.Substring(0, html.IndexOf("\""));
                titleString = titleString.Replace("&quot;", "\"");
                if(checkhtml.Contains("a href="))
                    html = html.Remove(0, html.IndexOf("a href") + 8);
                if(checkhtml.Contains("title="))
                    newsHeadlines.Add(titleString, urlString);
            }
            //push the new Results now
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.News.List" + screen.ToString(), newsHeadlines);
            VisibleSearchProgress(false, screen);

        }

        public Settings loadSettings()
        {
            if (settings == null)
                settings = new Settings("Weather Settings");
            List<string> refreshRates = new List<string>() { "5", "15", "30", "60" };
            settings.Add(new Setting(SettingTypes.MultiChoice, "OMDSNFL.RefreshRate", "", "Refresh Rate (Minutes)\n* When No Current Games Playing", refreshRates, refreshRates, StoredData.Get(this, "OMDSNFL.RefreshRate")));
            settings.OnSettingChanged += new SettingChanged(setting_changed);
            return settings;
        }

        private void setting_changed(int screen, Setting s)
        {
            StoredData.Set(this, s.Name, s.Value);

            // Update refreshrate
            refreshRate = StoredData.GetInt(this, "OMDSNFL.RefreshRate") * 1000 * 60;
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
            get { return "OMDSNFL"; }
        }
        public string displayName
        {
            get { return "NFL Data"; }
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
            get { return OM.Host.getSkinImage("Icons|Icon-OMNFL"); }
        }

        public void Dispose()
        {
            //Closing();
            Killing();
        }

    }

}