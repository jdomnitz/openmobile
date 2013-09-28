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

namespace OMNFL
{
    //[SkinIcon("Icons|Icon-OMWeather2")]

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
        string[] Selection;
        bool[] selectionInitialized;

        public eLoadStatus initialize(IPluginHost host)
        {

            theHost = host;
            manager = new ScreenManager(this);

            searchResults = new Dictionary<int, Dictionary<string, string>>[theHost.ScreenCount];
            newsResults = new Dictionary<string, string>[theHost.ScreenCount];
		Selection = new string[theHost.ScreenCount];
		selectionInitialized = new bool[theHost.ScreenCount];

        OMPanel nflPanel = new OMPanel("nflPanel", "NFL Game info", this.pluginIcon);

		for(int i=0;i<theHost.ScreenCount;i++)
		{
			if(StoredData.Get(this, "OMNFL.Selection." + i.ToString()) == "")
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
			selectionInitialized[i] = false;
			theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.Games.List" + i.ToString(), Subscription_Updated);
            theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.News.List" + i.ToString(), Subscription_Updated);
		}
			

            OMContainer mainContainer = new OMContainer("mainContainer", theHost.ClientArea[0].Left + 125, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width - 125, theHost.ClientArea[0].Height);
            nflPanel.addControl(mainContainer);

		//animate 15px left/right for selections
		int middleY = theHost.ClientArea[0].Height / 2;
        OMButton newsButton = OMButton.PreConfigLayout_BasicStyle("News", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY - 105, 125, 70, GraphicCorners.All);
        //OMButton newsButton = new OMButton("News", theHost.ClientArea[0].Left - 17, middleY - 105, 125, 70);
		newsButton.Text = "News";
		newsButton.FillColor = Color.FromArgb(128, Color.Black);
		newsButton.OnClick += new userInteraction(selection_OnClick);
		nflPanel.addControl(newsButton);
        OMButton scoresButton = OMButton.PreConfigLayout_BasicStyle("Scores", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY - 35, 125, 70, GraphicCorners.All);
        //OMButton scoresButton = new OMButton("Scores", theHost.ClientArea[0].Left - 17, middleY - 35, 125, 70);
		scoresButton.Text = "Scores";
		scoresButton.FillColor = Color.FromArgb(128, Color.Black);
		scoresButton.OnClick += new userInteraction(selection_OnClick);
		nflPanel.addControl(scoresButton);
        OMButton testButton = OMButton.PreConfigLayout_BasicStyle("Test", theHost.ClientArea[0].Left - 17, theHost.ClientArea[0].Top + middleY + 35, 125, 70, GraphicCorners.All);
        //OMButton testButton = new OMButton("Test", theHost.ClientArea[0].Left - 17, middleY + 35, 125, 70);
		testButton.Text = "Test";
		testButton.FillColor = Color.FromArgb(128, Color.Black);
		testButton.OnClick += new userInteraction(selection_OnClick);
		nflPanel.addControl(testButton);

            //for (int i = 0; i < theHost.ScreenCount; i++)
            //{
                //searchResults[i] = new Dictionary<int, Dictionary<string, string>>();
                //theHost.DataHandler.SubscribeToDataSource("OMDSNFL;NFL.Games.List" + i.ToString(), Subscription_Updated);
            //}

            nflPanel.Entering += new PanelEvent(nflPanel_Entering);
            manager.loadPanel(nflPanel, true);

            return eLoadStatus.LoadSuccessful;

        }

        private void nflPanel_Entering(OMPanel sender, int screen)
        {
            if (!selectionInitialized[screen])
            {
                AnimateSelection((OMButton)manager[screen, "nflPanel"][Selection[screen]]);
                selectionInitialized[screen] = true;
            }
            switch (Selection[screen])
            {
                case "News":
                    theHost.CommandHandler.ExecuteCommand("NFL.News.Refresh", new object[] { screen });
                    break;
                case "Scores":
                    theHost.CommandHandler.ExecuteCommand("NFL.Games.Refresh", new object[] { screen });
                    break;
            }
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
            int screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 4));
            
            Update(screen, sensor);
            //if (needToUpdate)
            //{
            //    Update(screen);
            //    needToUpdate = false;
            //}
            //else
            //    needToUpdate = true;
        }

        private void Update(int screen, DataSource sensor)
        {
            switch (Selection[screen])
            {
                case "News":
                    newsResults[screen] = (Dictionary<string, string>)sensor.Value;
                    UpdateNews(screen);
                    break;
                case "Scores":
                    searchResults[screen] = (Dictionary<int, Dictionary<string, string>>)sensor.Value;
                    UpdateScores(screen);
                    break;
            }
        }

        private void UpdateScores(int screen)
        {
            OMPanel p = manager[screen, "nflPanel"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];

            mainContainer.ClearControls();
            //attempt to update all of the games without clearing at some point...

            string previousGameDate = "";
            int ycoord = 10;
            //string tabChar = (Char)'\t';
            for (int i = 0; i < searchResults[screen].Count; i++)
            {
                if (searchResults[screen][i]["Game Date"].ToString() != previousGameDate)
                {
                    OMLabel gameDateLabel = new OMLabel("gameDateLabel", 0, ycoord, mainContainer.Width, 30);
                    gameDateLabel.Text = searchResults[screen][i]["Game Date"].ToString();
                    previousGameDate = gameDateLabel.Text;


                    OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, gameDateLabel.Top + gameDateLabel.Height + 5, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));

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
                    teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    teamRecord2.Text = searchResults[screen][i]["Away Team Record"];
                    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;

                    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    teamName2.Text = searchResults[screen][i]["Away Team Name"];
                    
                    teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    teamImage2.Image.image.MakeTransparentFromPixel(1, 1);


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
                    //buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

                    mainContainer.addControlRelative(shapeBackground);
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

                    ycoord += shapeBackground.Height + 55;
                }
                else
                {
                    OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, ycoord, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));

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
                    teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    teamRecord2.Text = searchResults[screen][i]["Away Team Record"];
                    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    teamName2.Text = searchResults[screen][i]["Away Team Name"];
                    teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    teamImage2.Image.image.MakeTransparentFromPixel(1, 1);

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
                    //buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

                    mainContainer.addControlRelative(shapeBackground);
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

                    ycoord += shapeBackground.Height + 20;
                }
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
            get 
            { 
                //return OM.Host.getSkinImage("Icons|Icon-NFL"); 
                return OM.Host.getPluginImage(this, "Icon-OMNFL"); 
            }
        }

        public void Dispose()
        {
            //Killing();
        }

    } //end class1
} //end namespace