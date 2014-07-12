using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;


namespace OMDSSports.Providers
{
    class ProviderNFL
    {
        static List<DateTime> gameTimesCompared = new List<DateTime>();
        public static Dictionary<int, Dictionary<string, string>> DisplayAllGames(int screen)
        {
            Dictionary<int, Dictionary<string, string>> Results = new Dictionary<int, Dictionary<string, string>>();
            string html = "";
            //VisibleSearchProgress(true, screen);
            //gamesPlaying = false;
            gameTimesCompared.Clear();
            using (WebClient wc = new WebClient())
            {
                html = wc.DownloadString("http://scores.espn.go.com/nfl/scoreboard");
            }
            //parse it now
            int gameCount = 0;

            //if (!html.Contains("PRO BOWL"))
            //{
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
                    if (!gametime.Contains("Final"))
                    {
                        if (gametime.Contains("Qtr"))
                        {
                            //gamesPlaying = true;

                        }
                        else
                        {
                            //compare time of game
                            //gameDate = Sunday, January 26, 2014
                            //7:30 PM ET
                            //DateTime.Now
                            //DateTime gameUTC = TimeZoneInfo.ConvertTimeToUtc(
                            string gameTimeTogether = gameDate.Replace(",", "") + " " + gametime.Substring(0, gametime.LastIndexOf(" "));
                            DateTime localUTC = DateTime.Now.ToUniversalTime();
                            CultureInfo cultureInfo = new CultureInfo("en-US");
                            DateTime gameUTC = DateTime.ParseExact(gameTimeTogether, "dddd MMMM d yyyy h:mm tt", cultureInfo).ToUniversalTime();
                            //if (localUTC >= gameUTC.AddMilliseconds(-Convert.ToDouble(refreshRate)))
                            //{
                            //gameTimesCompared.Add(new Dictionary<DateTime, DateTime>());
                            //gameTimesCompared[gameTimesCompared.Count - 1].Add(localUTC, gameUTC);
                            gameTimesCompared.Add(gameUTC);
                            //}
                        }
                    }
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("<p>") + 3);
                    string gameChannel = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    //away team
                    string gameTeamAwayImageUrl = gameHtml.Substring(0, gameHtml.IndexOf("team-name"));
                    gameTeamAwayImageUrl = gameTeamAwayImageUrl.Remove(0, gameTeamAwayImageUrl.LastIndexOf("<img") + 10);
                    gameTeamAwayImageUrl = gameTeamAwayImageUrl.Substring(0, gameTeamAwayImageUrl.IndexOf(">") - 2);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("team-name") + 12);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("\">") + 2);
                    string gameTeamAway = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamAway == "")
                    {
                        gameTeamAway = gameHtml.Substring(0, gameHtml.IndexOf("</a"));
                        gameTeamAway = gameTeamAway.Remove(0, gameTeamAway.LastIndexOf(">") + 1);
                    }
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
                    string gameTeamHomeImageUrl = gameHtml.Substring(0, gameHtml.IndexOf("team-name"));
                    gameTeamHomeImageUrl = gameTeamHomeImageUrl.Remove(0, gameTeamHomeImageUrl.LastIndexOf("<img") + 10);
                    gameTeamHomeImageUrl = gameTeamHomeImageUrl.Substring(0, gameTeamHomeImageUrl.IndexOf(">") - 2);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("team-name") + 12);
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("\">") + 2);
                    string gameTeamHome = gameHtml.Substring(0, gameHtml.IndexOf("<"));
                    if (gameTeamHome == "")
                    {
                        gameTeamHome = gameHtml.Substring(0, gameHtml.IndexOf("</a"));
                        gameTeamHome = gameTeamHome.Remove(0, gameTeamHome.LastIndexOf(">") + 1);
                    }
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
                    string gameURL = "";
                    if ((gametime.Contains("Final")) || (gametime.Contains("Qtr")))
                    {
                        //game is over, get "box score" url
                        gameURL = gameHtml.Substring(0, gameHtml.IndexOf("Box Score") - 2);
                        gameURL = "scores.espn.go.com/nfl" + gameURL.Remove(0, gameURL.LastIndexOf("\"") + 1);
                    }
                    else
                    {
                        //game has not started, get "intel" url
                        gameURL = gameHtml.Substring(0, gameHtml.IndexOf("Intel") - 2);
                        gameURL = "scores.espn.go.com/nfl" + gameURL.Remove(0, gameURL.LastIndexOf("\"") + 1);
                    }
                    gameHtml = gameHtml.Remove(0, gameHtml.IndexOf("-lastPlayText") + 15);
                    string gameLastPlay = gameHtml.Substring(0, gameHtml.IndexOf("<"));



                    gameResults.Add("Game Date", gameDate);
                    gameResults.Add("Game Time", gametime);
                    gameResults.Add("Game Channel", gameChannel);
                    gameResults.Add("Home Team Name", gameTeamHome);
                    gameResults.Add("Home Team Image Url", gameTeamHomeImageUrl);
                    gameResults.Add("Home Team Record", gameTeamHomeRecord);
                    gameResults.Add("Home Team Quarter One", gameTeamHomeQuarterOne);
                    gameResults.Add("Home Team Quarter Two", gameTeamHomeQuarterTwo);
                    gameResults.Add("Home Team Quarter Three", gameTeamHomeQuarterThree);
                    gameResults.Add("Home Team Quarter Four", gameTeamHomeQuarterFour);
                    gameResults.Add("Home Team Quarter OT", gameTeamHomeQuarterOT);
                    gameResults.Add("Home Team Total", gameTeamHomeTotal);
                    gameResults.Add("Away Team Name", gameTeamAway);
                    gameResults.Add("Away Team Image Url", gameTeamAwayImageUrl);
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
            //}
            if (Results.Count == 0)
            {
                Results.Add(0, new Dictionary<string, string>());
                Results[0].Add("No Games", "No Games To Display");
            }


            //push the new Results now
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSNFL;NFL.Games.List" + screen.ToString(), Results);
            //VisibleSearchProgress(false, screen);
            //CompareGames();
            return Results;
        }

        
    }
}
