using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using System.Net;
using System.Globalization;

namespace OMDSSports.DSProviders
{
    [PluginLevel(PluginLevels.System)]
    class DSNFL
    {

        private static int refreshRate;
        private static bool gamesPlaying = false;
        private static bool manualRefresh;

        public static void CreateCommands()
        {
            OM.Host.CommandHandler.AddCommand(new Command("OMDSSports", "NFL", "Scores", "Refresh", RefreshAllGames, 1, false, "Refreshes Data For All NFL Games {param = screen}"));
            OM.Host.CommandHandler.AddCommand(new Command("OMDSSports", "NFL", "News", "Refresh", RefreshAllNews, 1, false, "Refreshes News For NFL {param = screen}"));
            OM.Host.CommandHandler.AddCommand(new Command("OMDSSports", "NFL", "SpecificGame", "Refresh", RefreshSpecificGame, 2, false, "Refreshes Score For 1 NFL Game {param = screen, url}"));
            OM.Host.CommandHandler.AddCommand(new Command("OMDSSports", "NFL", "SpecificGame", "StopRefresh", StopRefreshSpecificGame, 1, false, "Step Refreshing Score For 1 NFL Game {param = screen}"));
        }

        public static void CreateDataSources()
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                //each screen gets its own Dictionary<string,string> of data

                OM.Host.DataHandler.AddDataSource(new DataSource("OMDSSports", "NFL", "Teams", "Names", DataSource.DataTypes.raw, "NFL Team Name Results"));
                OM.Host.DataHandler.AddDataSource(new DataSource("OMDSSports", "NFL", "Scores", "List." + i.ToString(), DataSource.DataTypes.raw, "NFL Game Results"));
                OM.Host.DataHandler.AddDataSource(new DataSource("OMDSSports", "NFL", "News", "List." + i.ToString(), DataSource.DataTypes.raw, "NFL News Results"));
                OM.Host.DataHandler.AddDataSource(new DataSource("OMDSSports", "NFL", "SpecificGame", "List." + i.ToString(), DataSource.DataTypes.raw, "NFL Specific Game Results"));

                OM.Host.DataHandler.AddDataSource(new DataSource("OMDSSports", "NFL", "Info", "Status." + i.ToString(), DataSource.DataTypes.raw, "Status of NFL DataSource"));

            }
        }

        private static object RefreshAllNews(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            Dictionary<int, Dictionary<string, string>> Results = new Dictionary<int, Dictionary<string, string>>();
            DisplayAllNews(screen);
            return null;
        }

        private static object RefreshAllGames(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            Dictionary<int, Dictionary<string, string>> Results = Providers.ProviderNFL.DisplayAllGames(screen);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSSports;NFL.Scores.List." + screen.ToString(), Results);
            //DisplayAllGames(screen);
            return null;
        }

        private static object RefreshSpecificGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            string url = param[1].ToString();
            //start keeping a "refresh" for this method below, interval, timer, etc
            
            DisplaySpecificGame(screen, url);
            return null;
        }

        private static object StopRefreshSpecificGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            int screen = Convert.ToInt32(param[0]);
            //disable the "refresh" for interval, timer, etc from RefreshSpecificGame method
            return null;
        }

        private static void VisibleSearchProgress(bool visible, int screen)
        {
            if (visible)
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSSports;NFL.Info.Status" + screen.ToString(), "Searching");
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSSports;NFL.Info.Status" + screen.ToString(), "Done Searching");
            }
        }

        //private static void CompareGames()
        //{
        //    DateTime earliestGame = DateTime.MinValue;
        //    for (int i = 0; i < gameTimesCompared.Count; i++)
        //    {
        //        if (gamesPlaying)
        //            break;
        //        if (earliestGame == DateTime.MinValue)
        //            earliestGame = gameTimesCompared[i];
        //        else
        //        {
        //            if (gameTimesCompared[i] < earliestGame)
        //                earliestGame = gameTimesCompared[i];
        //        }
        //    }
        //    if (earliestGame != DateTime.MinValue)
        //    {
        //        if (DateTime.Now.ToUniversalTime() >= earliestGame.AddMilliseconds(-Convert.ToDouble(refreshRate)))
        //        {
        //            TimeSpan timeSpan = earliestGame - DateTime.Now.ToUniversalTime();
        //            OpenMobile.Timer tmr = new OpenMobile.Timer(timeSpan.TotalMilliseconds);
        //            tmr.Elapsed += new System.Timers.ElapsedEventHandler(gameTimer_Tick);
        //            tmr.Enabled = true;
        //        }
        //    }
        //}

        //private static void gameTimer_Tick(object sender, EventArgs e)
        //{
        //    ((OpenMobile.Timer)sender).Dispose();
        //    if (gamesPlaying)
        //        return;
        //    manualRefresh = true;
        //}

        private static void DisplayAllNews(int screen)
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
                if (checkhtml.Contains("a href="))
                    html = html.Remove(0, html.IndexOf("a href") + 8);
                if (checkhtml.Contains("title="))
                    newsHeadlines.Add(titleString, urlString);
            }
            //push the new Results now
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.News.List" + screen.ToString(), newsHeadlines);
            VisibleSearchProgress(false, screen);

        }

        private static void DisplaySpecificGame(int screen, string url)
        {
            //try box-score first, if not available ("No Boxscore Available"), try intel?
            //box-score = after game, during game, intel = before game
            //url contains gameid in it, parse out that first... scores.espn.go.com/nfl/boxscore?gameid=***, /intel?gameid=****
            Dictionary<string, string> Results = new Dictionary<string, string>();
            string gameID = url.Remove(0, url.IndexOf("=") + 1);
            string html = "";
            string htmlOf = "";
            //string newURL = "scores.espn.go.com/nfl/boxscore?gameid=" + gameID;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    html = wc.DownloadString("http://scores.espn.go.com/nfl/boxscore?gameid=" + gameID);
                    htmlOf = "Box Score";
                }
            }
            catch (Exception ex) { }
            if ((html == "") || (html.Contains("No Boxscore Available")))
            {
                using (WebClient wc = new WebClient())
                {
                    html = wc.DownloadString("http://scores.espn.go.com/nfl/intel?gameid=" + gameID);
                    htmlOf = "Intel";
                }
            }
            if (htmlOf == "Box Score") //in game or after game
            {
                Results.Add("Type", "Box Score");

                html = html.Remove(0, html.IndexOf("gameStatusBarText") + 19);
                string gameTime = html.Substring(0, html.IndexOf("<"));

                //team-info
                html = html.Remove(0, html.IndexOf("team-info") + 24);
                html = html.Remove(0, html.IndexOf(">") + 1);
                string teamName1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<") + 1);
                html = html.Remove(0, html.IndexOf("<") + 1);
                string teamScore1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<p>") + 3);
                string teamRecord1 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("team-info") + 24);
                html = html.Remove(0, html.IndexOf(">") + 1);
                string teamName2 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<") + 1);
                html = html.Remove(0, html.IndexOf("<") + 1);
                string teamScore2 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<p>") + 3);
                string teamRecord2 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("game-time-location"));
                html = html.Remove(0, html.IndexOf(",") + 2);
                string gameDate = html.Substring(0, html.IndexOf("<"));

                //quarterly scores
                html = html.Remove(0, html.IndexOf("line-score-container"));
                html = html.Remove(0, html.IndexOf("class=\"team\"") + 10);
                html = html.Remove(0, html.IndexOf("class=\"team\""));
                html = html.Remove(0, html.IndexOf("text-align:center") + 20);
                string newhtml = html.Substring(0, html.IndexOf("game-notes"));
                string team1Qtr1 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team1Qtr1 == "") team1Qtr1 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team1Qtr2 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team1Qtr2 == "") team1Qtr2 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team1Qtr3 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team1Qtr3 == "") team1Qtr3 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team1Qtr4 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team1Qtr4 == "") team1Qtr4 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 19);
                string team1QtrOT = "0"; //
                //if (team1QtrOT == "") team1QtrOT = "0";
                //newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team1QtrF = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team1QtrF == "") team1QtrF = "0";
                string othtml1 = newhtml;

                newhtml = newhtml.Remove(0, newhtml.IndexOf("class=\"team\""));
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team2Qtr1 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team2Qtr1 == "") team2Qtr1 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team2Qtr2 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team2Qtr2 == "") team2Qtr2 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team2Qtr3 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team2Qtr3 == "") team2Qtr3 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team2Qtr4 = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                if (team2Qtr4 == "") team2Qtr4 = "0";
                newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 19);
                string team2QtrOT = "0";
                //newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                string team2QtrF = newhtml.Substring(0, newhtml.IndexOf("<")).Trim();
                string othtml2 = newhtml;


                if (newhtml.Contains("text-align:center"))
                {
                    //we have more scores, need to 'move' scores for OT
                    team1QtrOT = team1QtrF.Remove(0, 1);
                    othtml1 = othtml1.Remove(0, othtml1.IndexOf("text-align:center") + 19);
                    team1QtrF = othtml1.Substring(0, othtml1.IndexOf("<")).Trim();
                    //team2Qtr1 = team2Qtr2;
                    //team2Qtr2 = team2Qtr3;
                    //team2Qtr3 = team2Qtr4;
                    //team2Qtr4 = team2QtrF.Remove(0,1);
                    //newhtml = newhtml.Remove(0, newhtml.IndexOf("text-align:center") + 20);
                    team2QtrOT = othtml2.Substring(0, othtml2.IndexOf("<")).Remove(0, 1).Trim();
                    othtml2 = othtml2.Remove(0, othtml2.IndexOf("text-align:center") + 19);
                    team2QtrF = othtml2.Substring(0, othtml2.IndexOf("<")).Trim();
                }

                //total plays
                html = html.Remove(0, html.IndexOf("Total Plays"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPlaysTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPlaysTeam2 = html.Substring(0, html.IndexOf("<"));
                //total yards
                html = html.Remove(0, html.IndexOf("Total Yards"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalYardsTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalYardsTeam2 = html.Substring(0, html.IndexOf("<"));
                //passing yards
                html = html.Remove(0, html.IndexOf("Passing"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPassingTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPassingTeam2 = html.Substring(0, html.IndexOf("<"));
                //rushing yards
                html = html.Remove(0, html.IndexOf("Rushing"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalRushingTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalRushingTeam2 = html.Substring(0, html.IndexOf("<"));
                //penalties
                html = html.Remove(0, html.IndexOf("Penalties"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPenaltiesTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPenaltiesTeam2 = html.Substring(0, html.IndexOf("<"));
                //turnovers
                html = html.Remove(0, html.IndexOf("Turnovers"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalTurnoversTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalTurnoversTeam2 = html.Substring(0, html.IndexOf("<"));
                //time of possession
                html = html.Remove(0, html.IndexOf("Possession"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPossessionTeam1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("<td>") + 4);
                string totalPossessionTeam2 = html.Substring(0, html.IndexOf("<"));

                Results.Add("Game Date", gameDate);
                Results.Add("Game Time", gameTime);

                Results.Add("Team Name 1", teamName1);
                Results.Add("Team Record 1", teamRecord1);
                Results.Add("Team Score 1", teamScore1);
                Results.Add("Team Name 2", teamName2);
                Results.Add("Team Record 2", teamRecord2);
                Results.Add("Team Score 2", teamScore2);

                //7
                Results.Add("Team 1 Score 1", team1Qtr1);
                Results.Add("Team 1 Score 2", team1Qtr2);
                Results.Add("Team 1 Score 3", team1Qtr3);
                Results.Add("Team 1 Score 4", team1Qtr4);
                Results.Add("Team 1 Score OT", team1QtrOT);
                Results.Add("Team 1 Score F", team1QtrF);

                Results.Add("Team 2 Score 1", team2Qtr1);
                Results.Add("Team 2 Score 2", team2Qtr2);
                Results.Add("Team 2 Score 3", team2Qtr3);
                Results.Add("Team 2 Score 4", team2Qtr4);
                Results.Add("Team 2 Score OT", team2QtrOT);
                Results.Add("Team 2 Score F", team2QtrF);

                Results.Add("Total Plays Team 1", totalPlaysTeam1);
                Results.Add("Total Plays Team 2", totalPlaysTeam2);
                Results.Add("Total Yards Team 1", totalYardsTeam1);
                Results.Add("Total Yards Team 2", totalYardsTeam2);
                Results.Add("Total Passing (Yards) Team 1", totalPassingTeam1);
                Results.Add("Total Passing (Yards) Team 2", totalPassingTeam2);
                Results.Add("Total Rushing (Yards) Team 1", totalRushingTeam1);
                Results.Add("Total Rushing (Yards) Team 2", totalRushingTeam2);
                Results.Add("Total Penalties Team 1", totalPenaltiesTeam1);
                Results.Add("Total Penalties Team 2", totalPenaltiesTeam2);
                Results.Add("Total Turnovers Team 1", totalTurnoversTeam1);
                Results.Add("Total Turnovers Team 2", totalTurnoversTeam2);
                Results.Add("Time Of Possession Team 1", totalPossessionTeam1);
                Results.Add("Time Of Possession Team 2", totalPossessionTeam2);
            }
            else if (htmlOf == "Intel") //before game
            {
                Results.Add("Type", "Intel");

                //time, date, team names, team records
                html = html.Remove(0, html.IndexOf("gameStatusBarText") + 19);
                string gameTime = html.Substring(0, html.IndexOf("<"));

                string teamName1 = html.Substring(0, html.IndexOf("</a>"));
                teamName1 = teamName1.Remove(0, teamName1.LastIndexOf(">") + 1);
                html = html.Remove(0, html.IndexOf("<p>") + 3);
                string teamRecord1 = html.Substring(0, html.IndexOf("<"));

                string teamName2 = html.Substring(0, html.IndexOf("</a>"));
                teamName2 = teamName2.Remove(0, teamName2.LastIndexOf(">") + 1);
                html = html.Remove(0, html.IndexOf("<p>") + 3);
                string teamRecord2 = html.Substring(0, html.IndexOf("<"));

                //average points, average points allowed
                html = html.Remove(0, html.IndexOf("Avg Points") + 15);
                string teamAvgPointsScored1 = html.Substring(0, html.IndexOf("</td>"));
                teamAvgPointsScored1 = teamAvgPointsScored1.Remove(0, teamAvgPointsScored1.LastIndexOf(">") + 1);
                html = html.Remove(0, html.IndexOf("</td>") + 4);
                string teamAvgPointsScored2 = html.Substring(0, html.IndexOf("</td>"));
                teamAvgPointsScored2 = teamAvgPointsScored2.Remove(0, teamAvgPointsScored2.LastIndexOf(">") + 1);

                html = html.Remove(0, html.IndexOf("Avg Points Allowed") + 23);
                string teamAvgPointsScoredAgainst1 = html.Substring(0, html.IndexOf("</td>"));
                teamAvgPointsScoredAgainst1 = teamAvgPointsScoredAgainst1.Remove(0, teamAvgPointsScoredAgainst1.LastIndexOf(">") + 1);
                html = html.Remove(0, html.IndexOf("</td>") + 4);
                string teamAvgPointsScoredAgainst2 = html.Substring(0, html.IndexOf("</td>"));
                teamAvgPointsScoredAgainst2 = teamAvgPointsScoredAgainst2.Remove(0, teamAvgPointsScoredAgainst2.LastIndexOf(">") + 1);

                //offensive (total yards, total passing, total rushing) *averages per game
                html = html.Remove(0, html.IndexOf("Total Yards"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamTotalYards1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamTotalYards2 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("Yards Passing"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamPassingYards1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamPassingYards2 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("Yards Rushing"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamRushingYards1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamRushingYards2 = html.Substring(0, html.IndexOf("<"));

                //defensive (total yards allowed, total passing allowed, total rushing allowed) *averages per game
                html = html.Remove(0, html.IndexOf("Yards Allowed"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamTotalYardsAllowed1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamTotalYardsAllowed2 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("Pass Yds Allowed"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamPassingYardsAllowed1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamPassingYardsAllowed2 = html.Substring(0, html.IndexOf("<"));

                html = html.Remove(0, html.IndexOf("Rush Yds Allowed"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamRushingYardsAllowed1 = html.Substring(0, html.IndexOf("<"));
                html = html.Remove(0, html.IndexOf("&nbsp;</div>&nbsp;") + 18);
                string teamRushingYardsAllowed2 = html.Substring(0, html.IndexOf("<"));

                //Results.Add("Game Date", gameDate);
                Results.Add("Game Time", gameTime);
                Results.Add("Team Name 1", teamName1);
                Results.Add("Team Record 1", teamRecord1);
                Results.Add("Team Name 2", teamName2);
                Results.Add("Team Record 2", teamRecord2);
                Results.Add("Team Average Points Scored 1", teamAvgPointsScored1);
                Results.Add("Team Average Points Scored 2", teamAvgPointsScored2);
                Results.Add("Team Total Yards 1", teamTotalYards1);
                Results.Add("Team Total Yards 2", teamTotalYards2);
                Results.Add("Team Total Passing Yards 1", teamPassingYards1);
                Results.Add("Team Total Passing Yards 2", teamPassingYards2);
                Results.Add("Team Total Rushing Yards 1", teamRushingYards1);
                Results.Add("Team Total Rushing Yards 2", teamRushingYards2);
                Results.Add("Team Total Yards Allowed 1", teamTotalYardsAllowed1);
                Results.Add("Team Total Yards Allowed 2", teamTotalYardsAllowed2);
                Results.Add("Team Total Passing Yards Allowed 1", teamPassingYardsAllowed1);
                Results.Add("Team Total Passing Yards Allowed 2", teamPassingYardsAllowed2);
                Results.Add("Team Total Rushing Yards Allowed 1", teamRushingYardsAllowed1);
                Results.Add("Team Total Rushing Yards Allowed 2", teamRushingYardsAllowed2);

            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.SpecificGame.List" + screen.ToString(), Results);
        }
        
    }
}
