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
using System.Text;
using System.IO;
using OpenMobile.Graphics;
using System.Timers;
using System.Threading;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using OpenMobile.helperFunctions;
using OpenMobile.Threading;
using OpenMobile.Data;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OMNFL
{
    public class Class1 : IHighLevel
    {

        ScreenManager manager;
        IPluginHost theHost;
        //PluginSettings settings;
        string[] displayed;
        string[] degrees;
        string[] degreessettings;
        bool[] currentpositiondisplayed;
        Settings settings;
        PluginSettings ps;
        ButtonStrip[] PopUpMenuStrip;
        bool[] alreadyaddedstrip;
        int[] favoritesCounter;
        List<string>[] favoritesList;
        string[] lastSearched;
        bool[] current;
        Dictionary<int, Dictionary<string, string>>[] searchResults;
        Dictionary<string, string>[] newsResults;
        Dictionary<string, string>[] gameResults;
        string[] Selection;
        bool[] selectionInitialized;
        bool[] watchingGame;
        string[] gameURL;
        //List<string> teamList = new List<string>();
        Dictionary<string, List<string>> teamList = new Dictionary<string, List<string>>();
        List<string> sideButtons = new List<string>() { "Teams", "Scores", "News", "Schedules", "Stats", "Rosters" };

        public eLoadStatus initialize(IPluginHost host)
        {

            theHost = host;
            manager = new ScreenManager(this);

            OpenMobile.Threading.SafeThread.Asynchronous(BackgroundPanelLoad);

            return eLoadStatus.LoadSuccessful;

        }

        private void BackgroundPanelLoad()
        {
            //make sure this panel is first because it gives time for the asynch'd team filling to run
            OMPanel teamPanel = new OMPanel("teamPanel", "NFL -> Teams");
            OMBasicShape teamBackground = new OMBasicShape("teamBackground", theHost.ClientArea[0].Left + ((int)((theHost.ClientArea[0].Width / 7) * 1.5)), theHost.ClientArea[0].Top + (theHost.ClientArea[0].Height / 8), (theHost.ClientArea[0].Width / 7) * 5, (theHost.ClientArea[0].Height / 8) * 6, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(230, Color.Black), Color.Transparent, 0, 5));
            teamPanel.addControl(teamBackground);
            OMContainer teamContainer = new OMContainer("teamContainer", teamBackground.Left + 5, teamBackground.Top + 5, teamBackground.Width - 10, teamBackground.Height - 10);
            teamPanel.addControl(teamContainer);
            manager.loadPanel(teamPanel, false);
            OpenMobile.Threading.SafeThread.Asynchronous(AsynchTeamFill);

            searchResults = new Dictionary<int, Dictionary<string, string>>[theHost.ScreenCount];
            newsResults = new Dictionary<string, string>[theHost.ScreenCount];
            gameResults = new Dictionary<string, string>[theHost.ScreenCount];
            Selection = new string[theHost.ScreenCount];
            selectionInitialized = new bool[theHost.ScreenCount];
            watchingGame = new bool[theHost.ScreenCount];
            gameURL = new string[theHost.ScreenCount];

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                if (StoredData.Get(this, "OMNFL.Selection." + i.ToString()) == "")
                {
                    StoredData.Set(this, "OMNFL.Selection." + i.ToString(), "Scores");
                    Selection[i] = "Scores";
                }
                else
                {
                    Selection[i] = StoredData.Get(this, "OMNFL.Selection." + i.ToString());
                }
                searchResults[i] = new Dictionary<int, Dictionary<string, string>>();
                newsResults[i] = new Dictionary<string, string>();
                gameResults[i] = new Dictionary<string, string>();
                selectionInitialized[i] = false;
                theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.Games.List" + i.ToString(), Subscription_Updated);
                theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.News.List" + i.ToString(), Subscription_Updated);
                //theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.SpecificGame.List" + i.ToString(), Subscription_Updated);
                theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.Info.Status" + i.ToString(), Subscription_Updated);
            }

            OMPanel nflPanel = new OMPanel("nflPanel", "NFL Game info", this.pluginIcon);

            OMContainer mainContainer = new OMContainer("mainContainer", theHost.ClientArea[0].Left + 125, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width - 125, theHost.ClientArea[0].Height);
            nflPanel.addControl(mainContainer);

            int middleY = theHost.ClientArea[0].Height / 2;
            int sideButtonHeight = theHost.ClientArea[0].Height / (sideButtons.Count + 4);
            for (int i = 0; i < sideButtons.Count; i++)
            {
                //pretend to add "2 buttons" to top and bottom for padding
                OMButton sideButton = null;
                if (i == 0)
                    sideButton = OMButton.PreConfigLayout_BasicStyle(sideButtons[i], theHost.ClientArea[0].Left - 17, ((i + 2) * sideButtonHeight), 125, sideButtonHeight, GraphicCorners.Top);
                else if (i == sideButtons.Count - 1)
                    sideButton = OMButton.PreConfigLayout_BasicStyle(sideButtons[i], theHost.ClientArea[0].Left - 17, ((i + 2) * sideButtonHeight-1), 125, sideButtonHeight, GraphicCorners.Bottom);
                else
                    sideButton = OMButton.PreConfigLayout_BasicStyle(sideButtons[i], theHost.ClientArea[0].Left - 17, ((i + 2) * sideButtonHeight-1), 125, sideButtonHeight, GraphicCorners.None);
                sideButton.Text = sideButtons[i];
                sideButton.OnClick += new userInteraction(selection_OnClick);
                nflPanel.addControl(sideButton);
            }

            //OMButton newsButton = OMButton.PreConfigLayout_BasicStyle("News", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY - 175, 125, 70, GraphicCorners.All);
            ////OMButton newsButton = new OMButton("News", theHost.ClientArea[0].Left - 17, middleY - 105, 125, 70);
            //newsButton.Text = "News";
            //newsButton.FillColor = Color.FromArgb(128, Color.Black);
            //newsButton.OnClick += new userInteraction(selection_OnClick);
            //nflPanel.addControl(newsButton);
            //OMButton schedulesButton = OMButton.PreConfigLayout_BasicStyle("Schedules", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY - 105, 125, 70, GraphicCorners.All);
            //schedulesButton.Text = "Schedules";
            //schedulesButton.OnClick += new userInteraction(selection_OnClick);
            //nflPanel.addControl(schedulesButton);
            //OMButton scoresButton = OMButton.PreConfigLayout_BasicStyle("Scores", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY - 35, 125, 70, GraphicCorners.All);
            ////OMButton scoresButton = new OMButton("Scores", theHost.ClientArea[0].Left - 17, middleY - 35, 125, 70);
            //scoresButton.Text = "Scores";
            //scoresButton.FillColor = Color.FromArgb(128, Color.Black);
            //scoresButton.OnClick += new userInteraction(selection_OnClick);
            //nflPanel.addControl(scoresButton);
            //OMButton statisticsButton = OMButton.PreConfigLayout_BasicStyle("Statistics", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY + 35, 125, 70, GraphicCorners.All);
            ////OMButton statisticsButton = new OMButton("Statistics", theHost.ClientArea[0].Left - 17, middleY + 35, 125, 70);
            //statisticsButton.Text = "Statistics";
            //statisticsButton.FillColor = Color.FromArgb(128, Color.Black);
            //statisticsButton.OnClick += new userInteraction(selection_OnClick);
            //nflPanel.addControl(statisticsButton);
            //OMButton rostersButton = OMButton.PreConfigLayout_BasicStyle("Rosters", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY + 105, 125, 70, GraphicCorners.All);
            //rostersButton.Text = "Rosters";
            //rostersButton.OnClick += new userInteraction(selection_OnClick);
            //nflPanel.addControl(rostersButton);


            OMBasicShape updateProgressBackground = new OMBasicShape("updateProgressBackground", theHost.ClientArea[0].Left, sideButtonHeight * (sideButtons.Count + 2), 125, sideButtonHeight * 2, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
            updateProgressBackground.Visible = false;
            nflPanel.addControl(updateProgressBackground);
            OMImage updateProgressImage = new OMImage("updateProgressImage", theHost.ClientArea[0].Left, updateProgressBackground.Top, 50, updateProgressBackground.Height);
            updateProgressImage.Visible = false;
            updateProgressImage.Image = theHost.getSkinImage("BusyAnimationTransparent.gif");
            nflPanel.addControl(updateProgressImage);
            OMLabel updateProgressLabel = new OMLabel("updateProgressLabel", updateProgressImage.Left + updateProgressImage.Width + 1, updateProgressBackground.Top, 74, updateProgressBackground.Height);
            updateProgressLabel.Text = "Updating...";
            updateProgressLabel.AutoFitTextMode = FitModes.FitSingleLine;
            updateProgressLabel.Visible = false;
            nflPanel.addControl(updateProgressLabel);

            nflPanel.Entering += new PanelEvent(nflPanel_Entering);
            nflPanel.Leaving += new PanelEvent(nflPanel_Leaving);
            manager.loadPanel(nflPanel, true);

            OMPanel gamePanel = new OMPanel("gamePanel");
            OMBasicShape gameBackground = new OMBasicShape("gameBackground", theHost.ClientArea[0].Left + ((int)((theHost.ClientArea[0].Width / 7) * 1.5)), theHost.ClientArea[0].Top + (theHost.ClientArea[0].Height / 8), (theHost.ClientArea[0].Width / 7) * 5, (theHost.ClientArea[0].Height / 8) * 6, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(230, Color.Black), Color.Transparent, 0, 5));
            gamePanel.addControl(gameBackground);
            OMContainer gameContainer = new OMContainer("gameContainer", gameBackground.Left + 5, gameBackground.Top + 5, gameBackground.Width - 10, gameBackground.Height - 10);
            gamePanel.addControl(gameContainer);

            gamePanel.Entering += new PanelEvent(gamePanel_Entering);
            gamePanel.Leaving += new PanelEvent(gamePanel_Leaving);
            manager.loadPanel(gamePanel, false);

        }

        private void AsynchTeamFill()
        {
            teamList = GetTeamList();

            //OMButton.PreConfigLayout_BasicStyle("Rosters", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY + 105, 125, 70, GraphicCorners.All);
            for (int i = 0; i < teamList.Count; i++)
            {

                for (int j = 0; j < theHost.ScreenCount; j++)
                {
                    OMPanel p = manager[j, "teamPanel"];
                    OMContainer teamContainer = (OMContainer)p["teamContainer"];

                    OMCheckbox teamCheck = new OMCheckbox(String.Format("teamCheck.{0}", teamList.Keys.ElementAt(i)), teamContainer.Left + 5, teamContainer.Top + 5 + (i * 100), 75, 100);
                    if (StoredData.Get(this, String.Format("OMNFL.Favorites.{0}.{1}", teamList.Keys.ElementAt(i), j.ToString())) == "")
                        teamCheck.Checked = false;
                    else
                        teamCheck.Checked = true;
                    teamCheck.OnClick += new userInteraction(teamCheck_OnClick);
                    teamContainer.addControl(teamCheck);

                    OMImage teamImage = new OMImage(String.Format("teamImage.{0}", teamList.Keys.ElementAt(i)), teamCheck.Left + teamCheck.Width + 5, teamContainer.Top + 5 + (i * 100), 75, 100);
                    teamImage.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(teamList[teamList.Keys.ElementAt(i)][0]));
                    teamContainer.addControl(teamImage);

                    OMLabel teamLabel = new OMLabel(String.Format("teamLabel.{0}", teamList.Keys.ElementAt(i)), teamImage.Left + teamImage.Width + 5, teamContainer.Top + 5 + (i * 100), teamContainer.Width - 35, 100);
                    teamLabel.Text = teamList.Keys.ElementAt(i);
                    teamLabel.TextAlignment = Alignment.CenterCenter;
                    teamLabel.AutoFitTextMode = FitModes.FitFillSingleLine;
                    teamContainer.addControl(teamLabel);



                    //OMButton teamButton = OMButton.PreConfigLayout_BasicStyle(String.Format("teambutton.{0}", i.ToString()), -2, 55 + (70 * i), 125, 70, GraphicCorners.All);
                    ////OMButton teamButton = new OMButton(String.Format("teambutton.{0}", i.ToString()), -2, 55 + (55 * i), 125, 70);
                    ////teamButton.TextAlignment = Alignment.WordWrap;
                    //teamButton.AutoFitTextMode = FitModes.FitFillSingleLine;
                    //teamButton.TextAlignment = Alignment.CenterCenter;
                    //teamButton.Text = teamList.Keys.ElementAt(i);
                    //teamButton.OnClick += new userInteraction(teamButton_OnClick);
                    //teamContainer.addControl(teamButton);
                    ////((OMContainer)manager[j, "nflPanel"]["teamContainer"]).addControl(teamButton);
                }
            }
        }

        private void teamCheck_OnClick(OMControl sender, int screen)
        {
            //
        }

        private void teamButton_OnClick(object sender, int screen)
        {

        }

        private Dictionary<string, List<string>> GetTeamList()
        {
            Dictionary<string, List<string>> Results = new Dictionary<string, List<string>>();
            //List<string> Results = new List<string>();
            string teamImageUrl = "";
            string teamHtml = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    teamHtml = wc.DownloadString("http://www.nfl.com/teams");
                }
                while (teamHtml.Contains("teamslandinggridgroup"))
                {
                    teamHtml = teamHtml.Remove(0, teamHtml.IndexOf("src=") + 5);
                    teamImageUrl = teamHtml.Substring(0, teamHtml.IndexOf(" ") - 1);
                    //teamHtml = teamHtml.Remove(0, teamHtml.IndexOf("a href=") + 7);
                    teamHtml = teamHtml.Remove(0, teamHtml.IndexOf("a href=") + 7);
                    teamHtml = teamHtml.Remove(0, teamHtml.IndexOf("a href=") + 7);
                    teamHtml = teamHtml.Remove(0, teamHtml.IndexOf(">") + 1);
                    //string teamName = teamHtml.Substring(0, teamHtml.IndexOf("<"));
                    //int lastIndex = teamName.LastIndexOf(" ");
                    //teamName = teamName.Substring(0, lastIndex) + Environment.NewLine + teamName.Remove(0, lastIndex + 1);
                    //Results.Add(teamName);
                    Results.Add(teamHtml.Substring(0, teamHtml.IndexOf("<")), new List<string>() { teamImageUrl });
                    teamHtml = teamHtml.Remove(0, teamHtml.IndexOf("teamslandinggridgroup") + 21);
                }

            }
            catch
            {
                //problem downloading the initial team list...

            }
            Results.Remove(Results.Keys.ElementAt(0));
            return Results;
        }

        private void gamePanel_Entering(OMPanel sender, int screen)
        {
            theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.SpecificGame.List" + screen.ToString(), Subscription_Updated);
            theHost.CommandHandler.ExecuteCommand("NFL.SpecificGame.Refresh", new object[] { screen, gameURL[screen] });
        }

        private void gamePanel_Leaving(OMPanel sender, int screen)
        {
            theHost.CommandHandler.ExecuteCommand("NFL.SpecificGame.StopRefresh", new object[] { screen });
            OMPanel p = manager[screen, "gamePanel"];
            OMContainer gameContainer = (OMContainer)p["gameContainer"];
            gameContainer.ClearControls();
            watchingGame[screen] = false;
        }

        private void nflPanel_Entering(OMPanel sender, int screen)
        {
            //theHost.CommandHandler.ExecuteCommand("NFL.Games.Refresh", new object[] { screen });

            if (!selectionInitialized[screen])
            {
                AnimateSelection((OMButton)manager[screen, "nflPanel"][Selection[screen]]);
                selectionInitialized[screen] = true;
            }
            switch (Selection[screen])
            {
                case "Teams":
                    theHost.execute(eFunction.TransitionToPanel, screen, this.pluginName, "teamPanel");
                    theHost.execute(eFunction.ExecuteTransition, screen);
                    break;
                case "News":
                    theHost.CommandHandler.ExecuteCommand("NFL.News.Refresh", new object[] { screen });
                    break;
            //    case "Schedules":

            //        break;
                case "Scores":
                    theHost.CommandHandler.ExecuteCommand("NFL.Games.Refresh", new object[] { screen });
                    break;
            //    case "Statistics":

            //        break;
            //    case "Rosters":

            //        break;
            }
        }

        private void nflPanel_Leaving(OMPanel sender, int screen)
        {
            //watchingGame[screen] = false;
        }

        private void selection_OnClick(OMControl sender, int screen)
        {
            string text = ((OMButton)sender).Text;
            if (Selection[screen] == text)
                return;
            //animate left Selection[screen] button
            //animate right sender button
            //change Selection[screen] text to sender text
            lock (sender)
            {
                AnimateSelections((OMButton)manager[screen, "nflPanel"][Selection[screen]], (OMButton)sender);
                Selection[screen] = text;
                nflPanel_Entering(manager[screen, "nflPanel"], screen);
            }


        }

        private void AnimateSelection(OMButton controlRight)
        {
            SmoothAnimator Animation = new SmoothAnimator(0.2f); //3.0f
            int showLeft = controlRight.Left;
            Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
            {
                showLeft += AnimationStep;
                if (showLeft > -2)
                {
                    controlRight.Left = theHost.ClientArea[0].Left - 2;
                    return false;
                }
                else
                {
                    controlRight.Left = showLeft;
                }
                return true;
            });
        }

        private void AnimateSelections(OMButton controlLeft, OMButton controlRight)
        {
            SmoothAnimator Animation = new SmoothAnimator(0.2f); //3.0f
            int hideLeft = controlLeft.Left;
            int showLeft = controlRight.Left;
            Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
            {
                hideLeft -= AnimationStep;
                showLeft += AnimationStep;
                if (hideLeft < theHost.ClientArea[0].Left - 17)
                {
                    controlLeft.Left = theHost.ClientArea[0].Left - 17;
                    controlRight.Left = theHost.ClientArea[0].Left - 2;
                    return false;
                }
                else
                {
                    controlLeft.Left = hideLeft;
                    controlRight.Left = showLeft;
                }
                return true;
            });
        }

        bool needToUpdate = true;
        private void Subscription_Updated(DataSource sensor)
        {
            if (manager == null)
                return;
            if (sensor.Value == null)
                return;
            int screen = 0;
            if (sensor.NameLevel3.Contains("Status"))
                screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 6));
            else if (sensor.NameLevel3.Contains("List"))
                screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 4));

            Update(screen, sensor);
        }

        private void Update(int screen, DataSource sensor)
        {
            switch (sensor.NameLevel2)
            {
                case "News":
                    newsResults[screen] = (Dictionary<string, string>)sensor.Value;
                    UpdateNews(screen);
                    break;
                case "Games":
                    searchResults[screen] = (Dictionary<int, Dictionary<string, string>>)sensor.Value;
                    UpdateScores(screen);
                    break;
                case "SpecificGame":
                    gameResults[screen] = (Dictionary<string, string>)sensor.Value;
                    UpdateGame(screen);
                    break;
                case "Info":
                    if (sensor.NameLevel3 == "Status")
                    {
                        if (sensor.Value.ToString() == "Searching")
                        {
                            ((OMBasicShape)manager[screen, "nflPanel"]["updateProgressBackground"]).Visible = true;
                            ((OMImage)manager[screen, "nflPanel"]["updateProgressImage"]).Visible = true;
                            ((OMLabel)manager[screen, "nflPanel"]["updateProgressLabel"]).Visible = true;
                        }
                        else
                        {
                            ((OMBasicShape)manager[screen, "nflPanel"]["updateProgressBackground"]).Visible = false;
                            ((OMImage)manager[screen, "nflPanel"]["updateProgressImage"]).Visible = false;
                            ((OMLabel)manager[screen, "nflPanel"]["updateProgressLabel"]).Visible = false;
                        }
                    }
                    break;
            }
        }

        private void UpdateTeamList()
        {

        }

        private void UpdateScores(int screen)
        {
            OMPanel p = manager[screen, "nflPanel"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];

            mainContainer.ClearControls();
            //attempt to update all of the games without clearing at some point...

            string previousGameDate = "";
            bool addGameDateLabel = true;
            int ycoord = 10;
            //string tabChar = (Char)'\t';
            if (searchResults[screen][0].Keys.Contains("No Games"))
            {
                OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, +(mainContainer.Height / 2) - 30, mainContainer.Width - 20, 60, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMLabel noGamesLabel = new OMLabel("noGamesLabel", 15, (mainContainer.Height / 2) - 25, shapeBackground.Width - 10, 50);
                noGamesLabel.Text = searchResults[screen][0]["No Games"];
                mainContainer.addControlRelative(shapeBackground);
                mainContainer.addControlRelative(noGamesLabel);
            }
            else
            {
                for (int i = 0; i < searchResults[screen].Count; i++)
                {
                    if (searchResults[screen][i]["Game Date"].ToString() != previousGameDate)
                    {
                        addGameDateLabel = true;
                    }
                    else
                    {
                        addGameDateLabel = false;
                    }
                    //if (searchResults[screen][i]["Game Date"].ToString() != previousGameDate)
                    //{
                    //    addGameDateLabel = true;

                    //    OMLabel gameDateLabel = new OMLabel("gameDateLabel", 0, ycoord, mainContainer.Width, 30);
                    //    gameDateLabel.Text = searchResults[screen][i]["Game Date"].ToString();
                    //    previousGameDate = gameDateLabel.Text;

                    //    OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, gameDateLabel.Top + gameDateLabel.Height + 5, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));

                    //    OMLabel statusLabel = new OMLabel("statusLabel", 125, shapeBackground.Top, shapeBackground.Width - 125, 30);
                    //    statusLabel.Text = searchResults[screen][i]["Game Channel"].ToString() + " --- " + searchResults[screen][i]["Game Time"].ToString();


                    //    int averageWidth = (mainContainer.Width - 125) / 6;
                    //    OMLabel scoresLabel1 = new OMLabel("scoreslabel1", 125, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel1.Text = "1st\n" + searchResults[screen][i]["Home Team Quarter One"] + "\n" + searchResults[screen][i]["Away Team Quarter One"];
                    //    OMLabel scoresLabel2 = new OMLabel("scoreslabel2", scoresLabel1.Left + scoresLabel1.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel2.Text = "2nd\n" + searchResults[screen][i]["Home Team Quarter Two"] + "\n" + searchResults[screen][i]["Away Team Quarter Two"];
                    //    OMLabel scoresLabel3 = new OMLabel("scoreslabel3", scoresLabel2.Left + scoresLabel2.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel3.Text = "3rd\n" + searchResults[screen][i]["Home Team Quarter Three"] + "\n" + searchResults[screen][i]["Away Team Quarter Three"];
                    //    OMLabel scoresLabel4 = new OMLabel("scoreslabel4", scoresLabel3.Left + scoresLabel3.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel4.Text = "4th\n" + searchResults[screen][i]["Home Team Quarter Four"] + "\n" + searchResults[screen][i]["Away Team Quarter Four"];
                    //    OMLabel scoresLabel5 = new OMLabel("scoreslabel5", scoresLabel4.Left + scoresLabel4.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel5.Text = "OT\n" + searchResults[screen][i]["Home Team Quarter OT"] + "\n" + searchResults[screen][i]["Away Team Quarter OT"];
                    //    OMLabel scoresLabel6 = new OMLabel("scoreslabel6", scoresLabel5.Left + scoresLabel5.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel6.Text = "Total\n" + searchResults[screen][i]["Home Team Total"] + "\n" + searchResults[screen][i]["Away Team Total"];
                    //    int winnerHeight = scoresLabel1.Height / 3;
                    //    bool toAdd = false;
                    //    OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, 0, shapeBackground.Width - 125, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Yellow, 2, 5));
                    //    if (searchResults[screen][i]["Home Team Total"] != searchResults[screen][i]["Away Team Total"])
                    //    {
                    //        toAdd = true;
                    //        if (Convert.ToInt32(searchResults[screen][i]["Home Team Total"]) > Convert.ToInt32(searchResults[screen][i]["Away Team Total"]))
                    //        {
                    //            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + winnerHeight, scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                    //            shapeWinner.Top = scoresLabel1.Top + winnerHeight;
                    //        }
                    //        else
                    //        {
                    //            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + (winnerHeight * 2), scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                    //            shapeWinner.Top = scoresLabel1.Top + (winnerHeight * 2);
                    //        }
                    //    }

                    //    //name/record/image
                    //    OMLabel teamName1 = new OMLabel("teamName1", 0, shapeBackground.Top, 125, 25);
                    //    teamName1.Text = searchResults[screen][i]["Home Team Name"];
                    //    OMLabel teamRecord1 = new OMLabel("teamRecord1", 0, teamName1.Top + teamName1.Height, 125, 10);
                    //    teamRecord1.Text = searchResults[screen][i]["Home Team Record"];
                    //    teamRecord1.AutoFitTextMode = FitModes.FitFillSingleLine;
                    //    OMImage teamImage1 = new OMImage("teamImage1", shapeBackground.Left + 5, teamRecord1.Top + teamRecord1.Height, 110, 40);
                    //    teamImage1.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Home Team Image Url"]));
                    //    //teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    //    //teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    //    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    //    teamImage2.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Away Team Image Url"]));
                    //    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    //    teamRecord2.Text = searchResults[screen][i]["Away Team Record"];
                    //    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                    //    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    //    teamName2.Text = searchResults[screen][i]["Away Team Name"];
                    //    //teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    //    //teamImage2.Image.image.MakeTransparentFromPixel(1, 1);


                    //    //if last play
                    //    if ((searchResults[screen][i]["Last Play"] != "") && (searchResults[screen][i]["Last Play"] != "&nbsp;"))
                    //    {
                    //        OMLabel lastPlay = new OMLabel("lastPlay", 125, teamRecord2.Top, shapeBackground.Width - 125, 35);
                    //        lastPlay.Text = "Last Play: " + searchResults[screen][i]["Last Play"];
                    //        lastPlay.TextAlignment = Alignment.WordWrap;
                    //        lastPlay.AutoFitTextMode = FitModes.FitFill;
                    //        mainContainer.addControlRelative(lastPlay);
                    //    }

                    //    //button overlay
                    //    OMButton buttonOverlay = new OMButton("buttonOverlay", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, shapeBackground.Height);
                    //    buttonOverlay.FillColor = Color.Transparent;
                    //    buttonOverlay.Tag = searchResults[screen][i]["Game Url"];
                    //    buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

                    //    mainContainer.addControlRelative(shapeBackground);
                    //    mainContainer.addControlRelative(gameDateLabel);
                    //    mainContainer.addControlRelative(statusLabel);
                    //    mainContainer.addControlRelative(scoresLabel1);
                    //    mainContainer.addControlRelative(scoresLabel2);
                    //    mainContainer.addControlRelative(scoresLabel3);
                    //    mainContainer.addControlRelative(scoresLabel4);
                    //    mainContainer.addControlRelative(scoresLabel5);
                    //    mainContainer.addControlRelative(scoresLabel6);
                    //    mainContainer.addControlRelative(teamName1);
                    //    mainContainer.addControlRelative(teamRecord1);
                    //    mainContainer.addControlRelative(teamImage1);
                    //    mainContainer.addControlRelative(teamRecord2);
                    //    mainContainer.addControlRelative(teamName2);
                    //    mainContainer.addControlRelative(teamImage2);
                    //    if (toAdd)
                    //        mainContainer.addControlRelative(shapeWinner);
                    //    mainContainer.addControlRelative(buttonOverlay);

                    //    ycoord += shapeBackground.Height + 55;
                    //}
                    //else
                    //{
                    //    addGameDateLabel = false;

                    //    OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, ycoord, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));

                    //    OMLabel statusLabel = new OMLabel("statusLabel", 125, shapeBackground.Top, shapeBackground.Width - 125, 30);
                    //    statusLabel.Text = searchResults[screen][i]["Game Channel"].ToString() + " --- " + searchResults[screen][i]["Game Time"].ToString();

                    //    int averageWidth = (mainContainer.Width - 125) / 6;
                    //    OMLabel scoresLabel1 = new OMLabel("scoreslabel1", 125, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel1.Text = "1st\n" + searchResults[screen][i]["Home Team Quarter One"] + "\n" + searchResults[screen][i]["Away Team Quarter One"];
                    //    OMLabel scoresLabel2 = new OMLabel("scoreslabel2", scoresLabel1.Left + scoresLabel1.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel2.Text = "2nd\n" + searchResults[screen][i]["Home Team Quarter Two"] + "\n" + searchResults[screen][i]["Away Team Quarter Two"];
                    //    OMLabel scoresLabel3 = new OMLabel("scoreslabel3", scoresLabel2.Left + scoresLabel2.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel3.Text = "3rd\n" + searchResults[screen][i]["Home Team Quarter Three"] + "\n" + searchResults[screen][i]["Away Team Quarter Three"];
                    //    OMLabel scoresLabel4 = new OMLabel("scoreslabel4", scoresLabel3.Left + scoresLabel3.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel4.Text = "4th\n" + searchResults[screen][i]["Home Team Quarter Four"] + "\n" + searchResults[screen][i]["Away Team Quarter Four"];
                    //    OMLabel scoresLabel5 = new OMLabel("scoreslabel5", scoresLabel4.Left + scoresLabel4.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel5.Text = "OT\n" + searchResults[screen][i]["Home Team Quarter OT"] + "\n" + searchResults[screen][i]["Away Team Quarter OT"];
                    //    OMLabel scoresLabel6 = new OMLabel("scoreslabel6", scoresLabel5.Left + scoresLabel5.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    //    scoresLabel6.Text = "Total\n" + searchResults[screen][i]["Home Team Total"] + "\n" + searchResults[screen][i]["Away Team Total"];
                    //    int winnerHeight = scoresLabel1.Height / 3;
                    //    bool toAdd = false;
                    //    OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, 0, shapeBackground.Width - 125, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Yellow, 2, 5));
                    //    if (searchResults[screen][i]["Home Team Total"] != searchResults[screen][i]["Away Team Total"])
                    //    {
                    //        toAdd = true;
                    //        if (Convert.ToInt32(searchResults[screen][i]["Home Team Total"]) > Convert.ToInt32(searchResults[screen][i]["Away Team Total"]))
                    //        {
                    //            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + winnerHeight, scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                    //            shapeWinner.Top = scoresLabel1.Top + winnerHeight;
                    //        }
                    //        else
                    //        {
                    //            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + (winnerHeight * 2), scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                    //            shapeWinner.Top = scoresLabel1.Top + (winnerHeight * 2);
                    //        }
                    //    }

                    //    //name/record/image
                    //    OMLabel teamName1 = new OMLabel("teamName1", 0, shapeBackground.Top, 125, 25);
                    //    teamName1.Text = searchResults[screen][i]["Home Team Name"];
                    //    OMLabel teamRecord1 = new OMLabel("teamRecord1", 0, teamName1.Top + teamName1.Height, 125, 10);
                    //    teamRecord1.Text = searchResults[screen][i]["Home Team Record"];
                    //    teamRecord1.AutoFitTextMode = FitModes.FitFillSingleLine;
                    //    OMImage teamImage1 = new OMImage("teamImage1", shapeBackground.Left + 5, teamRecord1.Top + teamRecord1.Height, 110, 40);
                    //    teamImage1.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Home Team Image Url"]));
                    //    //teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    //    //teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    //    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    //    teamImage2.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Away Team Image Url"]));
                    //    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    //    teamRecord2.Text = searchResults[screen][i]["Away Team Record"];
                    //    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                    //    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    //    teamName2.Text = searchResults[screen][i]["Away Team Name"];
                    //    //teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    //    //teamImage2.Image.image.MakeTransparentFromPixel(1, 1);

                    //    //if last play
                    //    if ((searchResults[screen][i]["Last Play"] != "") && (searchResults[screen][i]["Last Play"] != "&nbsp;"))
                    //    {
                    //        OMLabel lastPlay = new OMLabel("lastPlay", 125, teamRecord2.Top, shapeBackground.Width - 125, 35);
                    //        lastPlay.Text = "Last Play: " + searchResults[screen][i]["Last Play"];
                    //        lastPlay.TextAlignment = Alignment.WordWrap;
                    //        lastPlay.AutoFitTextMode = FitModes.FitFill;
                    //        mainContainer.addControlRelative(lastPlay);
                    //    }

                    //    //button overlay
                    //    OMButton buttonOverlay = new OMButton("buttonOverlay", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, shapeBackground.Height);
                    //    buttonOverlay.FillColor = Color.Transparent;
                    //    buttonOverlay.Tag = searchResults[screen][i]["Game Url"];
                    //    buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

                    //    mainContainer.addControlRelative(shapeBackground);
                    //    mainContainer.addControlRelative(statusLabel);
                    //    mainContainer.addControlRelative(scoresLabel1);
                    //    mainContainer.addControlRelative(scoresLabel2);
                    //    mainContainer.addControlRelative(scoresLabel3);
                    //    mainContainer.addControlRelative(scoresLabel4);
                    //    mainContainer.addControlRelative(scoresLabel5);
                    //    mainContainer.addControlRelative(scoresLabel6);
                    //    mainContainer.addControlRelative(teamName1);
                    //    mainContainer.addControlRelative(teamRecord1);
                    //    mainContainer.addControlRelative(teamImage1);
                    //    mainContainer.addControlRelative(teamRecord2);
                    //    mainContainer.addControlRelative(teamName2);
                    //    mainContainer.addControlRelative(teamImage2);
                    //    if (toAdd)
                    //        mainContainer.addControlRelative(shapeWinner);
                    //    mainContainer.addControlRelative(buttonOverlay);

                    //    ycoord += shapeBackground.Height + 20;
                    //}

                    //create controls
                    OMLabel gameDateLabel = null;
                    OMBasicShape shapeBackground;
                    if (addGameDateLabel)
                    {
                        gameDateLabel = new OMLabel("gameDateLabel", 0, ycoord, mainContainer.Width, 30);
                        gameDateLabel.Text = searchResults[screen][i]["Game Date"].ToString();
                        previousGameDate = gameDateLabel.Text;
                        shapeBackground = new OMBasicShape("shapeBackground", 10, gameDateLabel.Top + gameDateLabel.Height + 5, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                    }
                    else
                    {
                        shapeBackground = new OMBasicShape("shapeBackground", 10, ycoord, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                    }
                    OMLabel statusLabel = new OMLabel("statusLabel", 125, shapeBackground.Top, shapeBackground.Width - 125, 30);
                    statusLabel.Text = searchResults[screen][i]["Game Channel"].ToString() + " --- " + searchResults[screen][i]["Game Time"].ToString();
                    int averageWidth = (mainContainer.Width - 125) / 6;
                    OMLabel scoresLabel1 = new OMLabel("scoreslabel1", 125, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel1.Text = "1st\n" + searchResults[screen][i]["Home Team Quarter One"] + "\n" + searchResults[screen][i]["Away Team Quarter One"];
                    OMLabel scoresLabel2 = new OMLabel("scoreslabel2", scoresLabel1.Left + scoresLabel1.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel2.Text = "2nd\n" + searchResults[screen][i]["Home Team Quarter Two"] + "\n" + searchResults[screen][i]["Away Team Quarter Two"];
                    OMLabel scoresLabel3 = new OMLabel("scoreslabel3", scoresLabel2.Left + scoresLabel2.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel3.Text = "3rd\n" + searchResults[screen][i]["Home Team Quarter Three"] + "\n" + searchResults[screen][i]["Away Team Quarter Three"];
                    OMLabel scoresLabel4 = new OMLabel("scoreslabel4", scoresLabel3.Left + scoresLabel3.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel4.Text = "4th\n" + searchResults[screen][i]["Home Team Quarter Four"] + "\n" + searchResults[screen][i]["Away Team Quarter Four"];
                    OMLabel scoresLabel5 = new OMLabel("scoreslabel5", scoresLabel4.Left + scoresLabel4.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel5.Text = "OT\n" + searchResults[screen][i]["Home Team Quarter OT"] + "\n" + searchResults[screen][i]["Away Team Quarter OT"];
                    OMLabel scoresLabel6 = new OMLabel("scoreslabel6", scoresLabel5.Left + scoresLabel5.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel6.Text = "Total\n" + searchResults[screen][i]["Home Team Total"] + "\n" + searchResults[screen][i]["Away Team Total"];
                    int winnerHeight = scoresLabel1.Height / 3;
                    bool toAdd = false;
                    OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, 0, shapeBackground.Width - 125, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Yellow, 2, 5));
                    if (searchResults[screen][i]["Home Team Total"] != searchResults[screen][i]["Away Team Total"])
                    {
                        toAdd = true;
                        if (Convert.ToInt32(searchResults[screen][i]["Home Team Total"]) > Convert.ToInt32(searchResults[screen][i]["Away Team Total"]))
                        {
                            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + winnerHeight, scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                            shapeWinner.Top = scoresLabel1.Top + winnerHeight;
                        }
                        else
                        {
                            //OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, statusLabel.Top + (winnerHeight * 2), scoresLabel1.Width, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Transparent, 0, 5));
                            shapeWinner.Top = scoresLabel1.Top + (winnerHeight * 2);
                        }
                    }

                    //name/record/image
                    OMLabel teamName1 = new OMLabel("teamName1", 0, shapeBackground.Top, 125, 25);
                    teamName1.Text = searchResults[screen][i]["Home Team Name"];
                    OMLabel teamRecord1 = new OMLabel("teamRecord1", 0, teamName1.Top + teamName1.Height, 125, 10);
                    teamRecord1.Text = searchResults[screen][i]["Home Team Record"];
                    teamRecord1.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMImage teamImage1 = new OMImage("teamImage1", shapeBackground.Left + 5, teamRecord1.Top + teamRecord1.Height, 110, 40);
                    teamImage1.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Home Team Image Url"]));
                    //teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    //teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    teamImage2.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[screen][i]["Away Team Image Url"]));
                    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    teamRecord2.Text = searchResults[screen][i]["Away Team Record"];
                    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    teamName2.Text = searchResults[screen][i]["Away Team Name"];
                    //teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    //teamImage2.Image.image.MakeTransparentFromPixel(1, 1);


                    //if last play
                    if ((searchResults[screen][i]["Last Play"] != "") && (searchResults[screen][i]["Last Play"] != "&nbsp;"))
                    {
                        OMLabel lastPlay = new OMLabel("lastPlay", 125, teamRecord2.Top, shapeBackground.Width - 125, 35);
                        lastPlay.Text = "Last Play: " + searchResults[screen][i]["Last Play"];
                        lastPlay.TextAlignment = Alignment.WordWrap;
                        lastPlay.AutoFitTextMode = FitModes.FitFill;
                        mainContainer.addControlRelative(lastPlay);
                    }

                    //button overlay
                    OMButton buttonOverlay = new OMButton("buttonOverlay", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, shapeBackground.Height);
                    buttonOverlay.FillColor = Color.Transparent;
                    buttonOverlay.Tag = searchResults[screen][i]["Game Url"];
                    buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

                    //add controls
                    mainContainer.addControlRelative(shapeBackground);
                    if (addGameDateLabel)
                        mainContainer.addControlRelative(gameDateLabel);
                    mainContainer.addControlRelative(statusLabel);
                    mainContainer.addControlRelative(scoresLabel1);
                    mainContainer.addControlRelative(scoresLabel2);
                    mainContainer.addControlRelative(scoresLabel3);
                    mainContainer.addControlRelative(scoresLabel4);
                    mainContainer.addControlRelative(scoresLabel5);
                    mainContainer.addControlRelative(scoresLabel6);
                    mainContainer.addControlRelative(teamName1);
                    mainContainer.addControlRelative(teamRecord1);
                    mainContainer.addControlRelative(teamImage1);
                    mainContainer.addControlRelative(teamRecord2);
                    mainContainer.addControlRelative(teamName2);
                    mainContainer.addControlRelative(teamImage2);
                    if (toAdd)
                        mainContainer.addControlRelative(shapeWinner);
                    mainContainer.addControlRelative(buttonOverlay);

                    if (addGameDateLabel)
                        ycoord += shapeBackground.Height + 55;
                    else
                        ycoord += shapeBackground.Height + 20;

                }
            }
        }

        private void buttonOverlay_OnClick(OMControl sender, int screen)
        {
            string url = ((OMButton)sender).Tag.ToString();
            gameURL[screen] = url;
            watchingGame[screen] = true;
            //clear any previous results before transitioning in
            OMPanel p = manager[screen, "gamePanel"];
            OMContainer gameContainer = (OMContainer)p["gameContainer"];
            gameContainer.ClearControls();
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMNFL", "gamePanel");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void UpdateGame(int screen)
        {
            OMPanel p = manager[screen, "gamePanel"];
            OMContainer gameContainer = (OMContainer)p["gameContainer"];

            gameContainer.ClearControls();

            if (gameResults[screen]["Type"] == "Box Score")
            {

                //add results or change previous results

                //add

                OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 0, 0, gameContainer.Width, gameContainer.Height, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));

                int averageWidth = shapeBackground.Width / 5;
                OMLabel gameDate = new OMLabel("gameDate", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, 20);
                gameDate.Text = gameResults[screen]["Game Date"];

                OMLabel gameTime = new OMLabel("gameTime", shapeBackground.Left, gameDate.Top + gameDate.Height, shapeBackground.Width, 20);
                gameTime.Text = gameResults[screen]["Game Time"];

                //team 1
                OMLabel teamName1 = new OMLabel("teamName1", shapeBackground.Left, gameTime.Top + gameTime.Height, averageWidth, 20);
                teamName1.Text = gameResults[screen]["Team Name 1"];
                OMLabel teamRecord1 = new OMLabel("teamRecord1", shapeBackground.Left, teamName1.Top + teamName1.Height, averageWidth, 15);
                teamRecord1.Text = gameResults[screen]["Team Record 1"];
                teamRecord1.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMImage teamImage1 = new OMImage("teamImage1", shapeBackground.Left, teamRecord1.Top + teamRecord1.Height, averageWidth, 50);
                teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                //scores
                int scoresWidth = (averageWidth * 3) / 7;

                OMImage scoresImage1 = new OMImage("scoresImage1", teamImage1.Left + teamImage1.Width + 5, 0, scoresWidth - 10, 0);
                scoresImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                OMLabel scores1 = new OMLabel("scores1", scoresImage1.Left + scoresImage1.Width, teamName1.Top, scoresWidth, teamName1.Height + teamRecord1.Height + teamImage1.Height);
                scores1.Text = "1st\n" + gameResults[screen]["Team 1 Score 1"] + "\n" + gameResults[screen]["Team 2 Score 1"];
                OMLabel scores2 = new OMLabel("scores2", scores1.Left + scores1.Width, scores1.Top, scores1.Width, scores1.Height);
                scores2.Text = "2nd\n" + gameResults[screen]["Team 1 Score 2"] + "\n" + gameResults[screen]["Team 2 Score 2"];
                OMLabel scores3 = new OMLabel("scores3", scores2.Left + scores2.Width, scores1.Top, scores1.Width, scores1.Height);
                scores3.Text = "3rd\n" + gameResults[screen]["Team 1 Score 3"] + "\n" + gameResults[screen]["Team 2 Score 3"];
                OMLabel scores4 = new OMLabel("scores4", scores3.Left + scores3.Width, scores1.Top, scores1.Width, scores1.Height);
                scores4.Text = "4th\n" + gameResults[screen]["Team 1 Score 4"] + "\n" + gameResults[screen]["Team 2 Score 4"];
                OMLabel scores5 = new OMLabel("scores5", scores4.Left + scores4.Width, scores1.Top, scores1.Width, scores1.Height);
                scores5.Text = "OT\n" + gameResults[screen]["Team 1 Score OT"] + "\n" + gameResults[screen]["Team 2 Score OT"];
                OMLabel scores6 = new OMLabel("scores6", scores5.Left + scores5.Width, scores1.Top, scores1.Width, scores1.Height);
                scores6.Text = "F\n" + gameResults[screen]["Team 1 Score F"] + "\n" + gameResults[screen]["Team 2 Score F"];

                //redo scoreimage1 top/height
                scoresImage1.Top = scores1.Top + (scores1.Height / 3) + 5;
                scoresImage1.Height = (scores1.Height / 3) - 10;

                OMImage scoresImage2 = new OMImage("scoresImage2", scoresImage1.Left, scoresImage1.Top + scoresImage1.Height + 5, scoresWidth - 10, scoresImage1.Height);

                //team 2
                OMLabel teamName2 = new OMLabel("teamName2", scores6.Left + scores6.Width, gameTime.Top + gameTime.Height, averageWidth, 20);
                teamName2.Text = gameResults[screen]["Team Name 2"];
                OMLabel teamRecord2 = new OMLabel("teamRecord2", teamName2.Left, teamName2.Top + teamName2.Height, averageWidth, 15);
                teamRecord2.Text = gameResults[screen]["Team Record 2"];
                teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMImage teamImage2 = new OMImage("teamImage2", teamName2.Left, teamRecord2.Top + teamRecord2.Height, averageWidth, 50);
                teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                teamImage2.Image.image.MakeTransparentFromPixel(1, 1);
                scoresImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                scoresImage2.Image.image.MakeTransparentFromPixel(1, 1);


                OMLabel lastPlay = new OMLabel("lastPlay", shapeBackground.Left, teamImage2.Top + teamImage2.Height, shapeBackground.Width, 40);
                lastPlay.Text = "Last Play Here...";
                lastPlay.TextAlignment = Alignment.WordWrap;
                lastPlay.AutoFitTextMode = FitModes.FitFill;

                //now we create yet another container to hold the "labels" for the stats
                OMContainer statsContainer = new OMContainer("statsContainer", shapeBackground.Left, lastPlay.Top + lastPlay.Height, shapeBackground.Width, shapeBackground.Height - (shapeBackground.Top + lastPlay.Top + lastPlay.Height));
                OMLabel teamStats1 = new OMLabel("teamStats1", statsContainer.Left, statsContainer.Top, statsContainer.Width / 6, 20 * ((gameResults[screen].Count - 7) / 2));
                OMLabel teamStats2 = new OMLabel("teamStats2", statsContainer.Left + ((statsContainer.Width / 6) * 5), statsContainer.Top, statsContainer.Width / 6, 20 * ((gameResults[screen].Count - 7) / 2));
                OMLabel statTitles = new OMLabel("statTitles", teamStats1.Left + teamStats1.Width, teamStats1.Top, (statsContainer.Width / 6) * 4, teamStats1.Height);
                for (int i = 21; i < gameResults[screen].Count; i++)
                {

                    if (teamStats1.Text == "")
                        teamStats1.Text = gameResults[screen][gameResults[screen].Keys.ElementAt(i)];
                    else
                        teamStats1.Text += "\n" + gameResults[screen][gameResults[screen].Keys.ElementAt(i)];
                    if (statTitles.Text == "")
                        statTitles.Text = gameResults[screen].Keys.ElementAt(i).Substring(0, gameResults[screen].Keys.ElementAt(i).Length - 7);
                    else
                        statTitles.Text += "\n" + gameResults[screen].Keys.ElementAt(i).Substring(0, gameResults[screen].Keys.ElementAt(i).Length - 7);
                    i += 1;
                    if (teamStats2.Text == "")
                        teamStats2.Text = gameResults[screen][gameResults[screen].Keys.ElementAt(i)];
                    else
                        teamStats2.Text += "\n" + gameResults[screen][gameResults[screen].Keys.ElementAt(i)];


                }
                //OMLabel teamStats2 = new OMLabel("teamStats2", statsContainer.Left + ((statsContainer.Width / 6) * 5), statsContainer.Top, statsContainer.Width / 6, 20 * (gameResults[screen].Count - 1));
                //for (int i = 1; i < gameResults[screen].Count; i++)
                //{
                //    if (teamStats2.Text == "")
                //        teamStats2.Text = gameResults[screen][gameResults[screen].Keys.ElementAt(i)];
                //    else
                //        teamStats2.Text += "\n" + gameResults[screen][gameResults[screen].Keys.ElementAt(i)];
                //}
                //OMLabel statTitles = new OMLabel("statTitles", teamStats1.Left + teamStats1.Width, teamStats1.Top, (statsContainer.Width / 6) * 4, teamStats1.Height);
                //for (int i = 1; i < gameResults[screen].Count; i++)
                //{
                //    if (statTitles.Text == "")
                //        statTitles.Text = gameResults[screen].Keys.ElementAt(i).Substring(0, gameResults[screen].Keys.ElementAt(i).Length - 7);
                //    else
                //        statTitles.Text += "\n" + gameResults[screen].Keys.ElementAt(i).Substring(0, gameResults[screen].Keys.ElementAt(i).Length - 7);
                //}


                gameContainer.addControlRelative(gameDate);
                gameContainer.addControlRelative(gameTime);
                gameContainer.addControlRelative(teamName1);
                gameContainer.addControlRelative(teamRecord1);
                gameContainer.addControlRelative(teamImage1);
                gameContainer.addControlRelative(scoresImage1);
                gameContainer.addControlRelative(scoresImage2);
                gameContainer.addControlRelative(scores1);
                gameContainer.addControlRelative(scores2);
                gameContainer.addControlRelative(scores3);
                gameContainer.addControlRelative(scores4);
                gameContainer.addControlRelative(scores5);
                gameContainer.addControlRelative(scores6);
                gameContainer.addControlRelative(teamName2);
                gameContainer.addControlRelative(teamRecord2);
                gameContainer.addControlRelative(teamImage2);
                gameContainer.addControlRelative(lastPlay);
                gameContainer.addControlRelative(statsContainer);

                //statsContainer.addControlRelative(teamStats1);
                //statsContainer.addControlRelative(teamStats2);
                //statsContainer.addControlRelative(statTitles);

                statsContainer.addControl(teamStats1);
                statsContainer.addControl(teamStats2);
                statsContainer.addControl(statTitles);
            }
            else if (gameResults[screen]["Type"] == "Intel")
            {

            }

        }

        private void UpdateNews(int screen)
        {
            OMPanel p = manager[screen, "nflPanel"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];
            mainContainer.ClearControls();

            int ycoord = 10;
            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, ycoord, mainContainer.Width - 20, mainContainer.Height - 20, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            OMList newsList = new OMList("newsList", shapeBackground.Left + 5, shapeBackground.Top + 5, shapeBackground.Width - 10, shapeBackground.Height - 10);
            newsList.ItemColor1 = Color.Transparent;
            newsList.ItemColor2 = Color.Transparent;
            newsList.ListStyle = eListStyle.MultiListText;
            //newsList.TextAlignment = Alignment.WordWrap;
            //newsList.FontSize = 24;

            for (int i = 0; i < newsResults[screen].Count; i++)
            {
                newsList.Add(newsResults[screen].Keys.ElementAt(i));
            }

            mainContainer.addControlRelative(shapeBackground);
            mainContainer.addControlRelative(newsList);
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen, name];
        }
        public Settings loadSettings()
        {
            return null;
            /*
            if (settings == null)
                settings = new Settings("Weather Settings");
            if (ps == null)
                ps = new PluginSettings();
            List<string> fc = new List<string>();
            fc.Add("Fahrenheit");
            fc.Add("Celcius");
            settings.Add(new Setting(SettingTypes.MultiChoice, "OMWeather2.Degree", "", "Fahrenheit / Celcius", fc, fc, ps.getSetting(this, "OMWeather2.Degree")));
            settings.OnSettingChanged += new SettingChanged(setting_changed);
            return settings;
            */
        }

        private void setting_changed(int screen, Setting s)
        {
            ps.setSetting(this, s.Name, s.Value);
            //degrees[screen] = s.Value;
            degreessettings[screen] = s.Value;
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
            get { return "OMNFL"; }
        }
        public string displayName
        {
            get { return "NFL"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "NFL"; }
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
            get { return OM.Host.getPluginImage(this, "Icon-OMNFL"); }
        }

        public void Dispose()
        {
            //Killing();
        }

    } //end class1
} //end namespace