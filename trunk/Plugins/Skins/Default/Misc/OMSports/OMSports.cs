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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;
using OpenMobile.Data;

namespace OMSports
{
    public class OMSports : HighLevelCode
    {
        string[] dataSourceDisplayed;
        List<string> sideButtons = new List<string>() { "Teams", "Scores", "News", "Schedules", "Stats", "Rosters" };
        ButtonStrip[] PopUpMenuStrip;
        bool[] alreadyAddedStrip;
        string[] sideButtonSelected;
        string[] sportShowing;
        bool[] notShown;

        public OMSports()
            : base("OMSports", imageItem.NONE, 1.0f, "Sports", "Sports", "Peter Yeaney", "peter.yeaney@outlook.com")
        {

        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            dataSourceDisplayed = new string[OM.Host.ScreenCount];
            PopUpMenuStrip = new ButtonStrip[OM.Host.ScreenCount];
            alreadyAddedStrip = new bool[OM.Host.ScreenCount];
            sideButtonSelected = new string[OM.Host.ScreenCount];
            sportShowing = new string[OM.Host.ScreenCount];
            notShown = new bool[OM.Host.ScreenCount];
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                PopUpMenuStrip[i] = new ButtonStrip(this.pluginName, "OMSports", "PopUpMenuStrip");
                alreadyAddedStrip[i] = false;
                string selectedSport = StoredData.Get(this, String.Format("OMSports.SelectedSport.{0}", i.ToString()));
                if (String.IsNullOrEmpty(selectedSport))
                {
                    sportShowing[i] = "";
                    notShown[i] = true;
                }
                else
                    sportShowing[i] = selectedSport;
                string selectedSideButton = StoredData.Get(this, String.Format("OMSports.SelectedSideButton.{0}", i.ToString()));
                if (String.IsNullOrEmpty(selectedSideButton))
                    sideButtonSelected[i] = "";
                else
                    sideButtonSelected[i] = selectedSideButton;

            }
            base.PanelManager.QueuePanel("mainPanel", InitMainPanel, true);
            return eLoadStatus.LoadSuccessful;
        }

        private OMPanel InitMainPanel()
        {
            OMPanel p = new OMPanel("mainPanel", "Sports");

            //side buttons?
            int middleY = OM.Host.ClientArea[0].Height / 2;
            int sideButtonHeight = OM.Host.ClientArea[0].Height / (sideButtons.Count + 4);
            for (int i = 0; i < sideButtons.Count; i++)
            {
                //pretend to add "2 buttons" to top and bottom for padding
                OMButton sideButton = OMButton.PreConfigLayout_BasicStyle(sideButtons[i], OM.Host.ClientArea[0].Left - 17, ((i + 2) * sideButtonHeight), 125, sideButtonHeight, GraphicCorners.All);
                sideButton.Text = sideButtons[i];
                sideButton.Visible = false;
                sideButton.OnClick += new userInteraction(selection_OnClick);
                p.addControl(sideButton);
            }

            OMContainer mainContainer = new OMContainer("mainContainer", OM.Host.ClientArea[0].Left + 125, OM.Host.ClientArea[0].Top, OM.Host.ClientArea[0].Width - 125, OM.Host.ClientArea[0].Height);
            p.addControl(mainContainer);

            OMBasicShape updateProgressBackground = new OMBasicShape("updateProgressBackground", OM.Host.ClientArea[0].Left, sideButtonHeight * (sideButtons.Count + 2), 125, sideButtonHeight * 2, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
            updateProgressBackground.Visible = false;
            p.addControl(updateProgressBackground);
            OMImage updateProgressImage = new OMImage("updateProgressImage", OM.Host.ClientArea[0].Left, updateProgressBackground.Top, 50, updateProgressBackground.Height);
            updateProgressImage.Visible = false;
            updateProgressImage.Image = OM.Host.getSkinImage("BusyAnimationTransparent.gif");
            p.addControl(updateProgressImage);
            OMLabel updateProgressLabel = new OMLabel("updateProgressLabel", updateProgressImage.Left + updateProgressImage.Width + 1, updateProgressBackground.Top, 74, updateProgressBackground.Height);
            updateProgressLabel.Text = "Updating...";
            updateProgressLabel.AutoFitTextMode = FitModes.FitSingleLine;
            updateProgressLabel.Visible = false;
            p.addControl(updateProgressLabel);

            SubscribeToDatasources();

            p.Entering += new PanelEvent(p_Entering);
            return p;
        }

        private void SubscribeToDatasources()
        {
            List<DataSource> listDS = OM.Host.DataHandler.GetDataSources("");
            for (int i = 0; i < listDS.Count; i++)
            {
                if (listDS[i].Provider == "OMDSSports")
                {
                    OM.Host.DataHandler.SubscribeToDataSource(listDS[i].FullNameWithProvider, Subscription_Updated);
                }
            }
        }

        private void p_Entering(OMPanel p, int screen)
        {
            if (!alreadyAddedStrip[screen])
            {

                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popUpMLB", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "MLB", false, changeSports_OnClick, null, null));
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popUpNBA", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "NBA", false, changeSports_OnClick, null, null));
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popUpNHL", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "NHL", false, changeSports_OnClick, null, null));
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popUpNFL", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "NFL", false, changeSports_OnClick, null, null));

                alreadyAddedStrip[screen] = true;
                //if ((sportShowing[screen] == null) || (sportShowing[screen] == ""))
                //    sportShowing[screen] = "NFL";
                //if ((sideButtonSelected[screen] == null) || (sideButtonSelected[screen] == ""))
                //    sideButtonSelected[screen] = "Scores";
                ////every time we enter the panel, attempt an update...
                //RefreshContainer(sportShowing[screen], sideButtonSelected[screen], screen);
            }
            if (!notShown[screen])
            {
                //make sure the side buttons ARE visible
                for (int i = 0; i < sideButtons.Count; i++)
                {
                    ((OMButton)base.PanelManager[screen, "mainPanel"][sideButtons[i]]).Visible = true;
                }
            }
            OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip[screen]);
        }

        private void changeSports_OnClick(OMControl sender, int screen)
        {
            //change the sport
            sportShowing[screen] = ((OMButton)sender).Text;
            if (notShown[screen])
            {
                notShown[screen] = false;
                for (int i = 0; i < sideButtons.Count; i++)
                {
                    ((OMButton)base.PanelManager[screen, "mainPanel"][sideButtons[i]]).Visible = true;
                }
            }
            if(sideButtonSelected[screen] != "")
                selection_OnClick(((OMButton)base.PanelManager[screen, "mainPanel"][sideButtonSelected[screen]]), screen);
        }

        private void selection_OnClick(OMControl sender, int screen)
        {
            if (sideButtonSelected[screen] != ((OMButton)sender).Text)
            {
                sideButtonSelected[screen] = ((OMButton)sender).Text;
                dataSourceDisplayed[screen] = String.Format("{0}.{1}", sportShowing[screen], sideButtonSelected[screen]);
            }
            else
                return;
            RefreshContainer(sportShowing[screen], sideButtonSelected[screen], screen);
        }

        private void RefreshContainer(string sportToShow, string sideButton, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(String.Format("{0}.{1}.Refresh", sportToShow, sideButton), new object[] { screen });
            return;
            switch (sideButtonSelected[screen])
            {

                //case "Teams":
                //    OM.Host.execute(eFunction.TransitionToPanel, screen, this.pluginName, "teamPanel");
                //    OM.Host.execute(eFunction.ExecuteTransition, screen);
                //    break;
                case "News":
                    OM.Host.CommandHandler.ExecuteCommand(String.Format("{0}.News.Refresh", sportShowing[screen]), new object[] { screen });
                    break;
                //    case "Schedules":

                //        break;
                case "Scores":
                    OM.Host.CommandHandler.ExecuteCommand(String.Format("{0}.Scores.Refresh", sportShowing[screen]), new object[] { screen });
                    break;
                //    case "Statistics":

                //        break;
                //    case "Rosters":

                //        break;
            }
        }

        private void Subscription_Updated(DataSource sensor)
        {
            if (sensor.Value == null)
                return;
            int screen = Convert.ToInt16(sensor.NameLevel3.Remove(0, sensor.NameLevel3.IndexOf(".") + 1));
            if (sensor.NameLevel3 == "Status")
            {
                if (sensor.Value.ToString() == "Searching")
                {
                    ((OMBasicShape)base.PanelManager[screen, "mainPanel"]["updateProgressBackground"]).Visible = true;
                    ((OMImage)base.PanelManager[screen, "mainPanel"]["updateProgressImage"]).Visible = true;
                    ((OMLabel)base.PanelManager[screen, "mainPanel"]["updateProgressLabel"]).Visible = true;
                }
                else
                {
                    ((OMBasicShape)base.PanelManager[screen, "mainPanel"]["updateProgressBackground"]).Visible = false;
                    ((OMImage)base.PanelManager[screen, "mainPanel"]["updateProgressImage"]).Visible = false;
                    ((OMLabel)base.PanelManager[screen, "mainPanel"]["updateProgressLabel"]).Visible = false;
                }
            }
            else
            {
                if (String.Format("{0}.{1}", sensor.NameLevel1, sensor.NameLevel2) == dataSourceDisplayed[screen])
                {
                    switch (sensor.NameLevel1) //sport
                    {
                        case "NFL":
                            switch (sensor.NameLevel2) //type of data...
                            {
                                case "Scores":
                                    UpdateNFLScores(screen, (Dictionary<int, Dictionary<string, string>>)sensor.Value);
                                    break;
                                case "News":

                                    break;
                                case "SpecificGame":

                                    break;
                            }
                            break;
                    }
                }
            }
        }

        #region NFL

        private void UpdateNFLScores(int screen, Dictionary<int, Dictionary<string, string>> searchResults)
        {
            OMPanel p = base.PanelManager[screen, "mainPanel"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];

            mainContainer.ClearControls();

            string previousGameDate = "";
            bool addGameDateLabel = true;
            int ycoord = 10;
            if (searchResults[0].Keys.Contains("No Games"))
            {
                OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 10, +(mainContainer.Height / 2) - 30, mainContainer.Width - 20, 60, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMLabel noGamesLabel = new OMLabel("noGamesLabel", 15, (mainContainer.Height / 2) - 25, shapeBackground.Width - 10, 50);
                noGamesLabel.Text = searchResults[0]["No Games"];
                mainContainer.addControlRelative(shapeBackground);
                mainContainer.addControlRelative(noGamesLabel);
            }
            else
            {
                for (int i = 0; i < searchResults[screen].Count; i++)
                {
                    if (searchResults[i]["Game Date"].ToString() != previousGameDate)
                    {
                        addGameDateLabel = true;
                    }
                    else
                    {
                        addGameDateLabel = false;
                    }
                    OMLabel gameDateLabel = null;
                    OMBasicShape shapeBackground;
                    if (addGameDateLabel)
                    {
                        gameDateLabel = new OMLabel("gameDateLabel", 0, ycoord, mainContainer.Width, 30);
                        gameDateLabel.Text = searchResults[i]["Game Date"].ToString();
                        previousGameDate = gameDateLabel.Text;
                        shapeBackground = new OMBasicShape("shapeBackground", 10, gameDateLabel.Top + gameDateLabel.Height + 5, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                    }
                    else
                    {
                        shapeBackground = new OMBasicShape("shapeBackground", 10, ycoord, mainContainer.Width - 20, 150, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                    }
                    OMLabel statusLabel = new OMLabel("statusLabel", 125, shapeBackground.Top, shapeBackground.Width - 125, 30);
                    statusLabel.Text = searchResults[i]["Game Channel"].ToString() + " --- " + searchResults[i]["Game Time"].ToString();
                    int averageWidth = (mainContainer.Width - 125) / 6;
                    OMLabel scoresLabel1 = new OMLabel("scoreslabel1", 125, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel1.Text = "1st\n" + searchResults[i]["Home Team Quarter One"] + "\n" + searchResults[i]["Away Team Quarter One"];
                    OMLabel scoresLabel2 = new OMLabel("scoreslabel2", scoresLabel1.Left + scoresLabel1.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel2.Text = "2nd\n" + searchResults[i]["Home Team Quarter Two"] + "\n" + searchResults[i]["Away Team Quarter Two"];
                    OMLabel scoresLabel3 = new OMLabel("scoreslabel3", scoresLabel2.Left + scoresLabel2.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel3.Text = "3rd\n" + searchResults[i]["Home Team Quarter Three"] + "\n" + searchResults[i]["Away Team Quarter Three"];
                    OMLabel scoresLabel4 = new OMLabel("scoreslabel4", scoresLabel3.Left + scoresLabel3.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel4.Text = "4th\n" + searchResults[i]["Home Team Quarter Four"] + "\n" + searchResults[i]["Away Team Quarter Four"];
                    OMLabel scoresLabel5 = new OMLabel("scoreslabel5", scoresLabel4.Left + scoresLabel4.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel5.Text = "OT\n" + searchResults[i]["Home Team Quarter OT"] + "\n" + searchResults[i]["Away Team Quarter OT"];
                    OMLabel scoresLabel6 = new OMLabel("scoreslabel6", scoresLabel5.Left + scoresLabel5.Width, statusLabel.Top + statusLabel.Height, averageWidth, shapeBackground.Height - 70);
                    scoresLabel6.Text = "Total\n" + searchResults[i]["Home Team Total"] + "\n" + searchResults[i]["Away Team Total"];
                    int winnerHeight = scoresLabel1.Height / 3;
                    bool toAdd = false;
                    OMBasicShape shapeWinner = new OMBasicShape("shapeWinner", scoresLabel1.Left, 0, shapeBackground.Width - 125, winnerHeight, new ShapeData(shapes.RoundedRectangle, Color.Transparent, Color.Yellow, 2, 5));
                    if (searchResults[i]["Home Team Total"] != searchResults[i]["Away Team Total"])
                    {
                        toAdd = true;
                        if (Convert.ToInt32(searchResults[i]["Home Team Total"]) > Convert.ToInt32(searchResults[i]["Away Team Total"]))
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
                    teamName1.Text = searchResults[i]["Home Team Name"];
                    OMLabel teamRecord1 = new OMLabel("teamRecord1", 0, teamName1.Top + teamName1.Height, 125, 10);
                    teamRecord1.Text = searchResults[i]["Home Team Record"];
                    teamRecord1.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMImage teamImage1 = new OMImage("teamImage1", shapeBackground.Left + 5, teamRecord1.Top + teamRecord1.Height, 110, 40);
                    teamImage1.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[i]["Home Team Image Url"]));
                    //teamImage1.Image = theHost.getPluginImage(this, "Images|" + teamName1.Text);
                    //teamImage1.Image.image.MakeTransparentFromPixel(1, 1);

                    OMImage teamImage2 = new OMImage("teamImage2", shapeBackground.Left + 5, teamImage1.Top + teamImage1.Height, 110, 40);
                    teamImage2.Image = new imageItem(OpenMobile.Net.Network.imageFromURL(searchResults[i]["Away Team Image Url"]));
                    OMLabel teamRecord2 = new OMLabel("teamRecord2", 0, teamImage2.Top + teamImage2.Height, 125, 10);
                    teamRecord2.Text = searchResults[i]["Away Team Record"];
                    teamRecord2.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMLabel teamName2 = new OMLabel("teamName2", 0, teamRecord2.Top + teamRecord2.Height, 125, 25);
                    teamName2.Text = searchResults[i]["Away Team Name"];
                    //teamImage2.Image = theHost.getPluginImage(this, "Images|" + teamName2.Text);
                    //teamImage2.Image.image.MakeTransparentFromPixel(1, 1);


                    //if last play
                    if ((searchResults[i]["Last Play"] != "") && (searchResults[i]["Last Play"] != "&nbsp;"))
                    {
                        OMLabel lastPlay = new OMLabel("lastPlay", 125, teamRecord2.Top, shapeBackground.Width - 125, 35);
                        lastPlay.Text = "Last Play: " + searchResults[i]["Last Play"];
                        lastPlay.TextAlignment = Alignment.WordWrap;
                        lastPlay.AutoFitTextMode = FitModes.FitFill;
                        mainContainer.addControlRelative(lastPlay);
                    }

                    //button overlay
                    OMButton buttonOverlay = new OMButton("buttonOverlay", shapeBackground.Left, shapeBackground.Top, shapeBackground.Width, shapeBackground.Height);
                    buttonOverlay.FillColor = Color.Transparent;
                    buttonOverlay.Tag = searchResults[i]["Game Url"];
                    //buttonOverlay.OnClick += new userInteraction(buttonOverlay_OnClick);

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

        #endregion

    }
}
