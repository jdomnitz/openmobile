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

namespace OMWeather2
{
    //[SkinIcon("Icons|Icon-OMWeather2")]

    public class OMWeather : IHighLevel
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
        Dictionary<string, object>[] searchResults;
        List<string>[] realSearchedArea;

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            manager = new ScreenManager(this);

            OpenMobile.Threading.SafeThread.Asynchronous(BackgroundPanelLoad);
            

            //OMBasicShape currentbackground = new OMBasicShape("currentbackground", (int)(float)(theHost.ClientArea_Init.Width * .15), theHost.ClientArea_Init.Top - 6, (int)(float)(theHost.ClientArea_Init.Width * .70), (int)(float)(theHost.ClientArea_Init.Height * .213) + 6);
            //currentbackground.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.Gray, Color.White, 1);

            //OMButton currentposition = new OMButton("currentposition", 0, 0, 0, 0);
            //currentposition.Text = "Current Position Weather";
            //currentposition.OnClick += new userInteraction(currentposition_onclick);
            //OMButton searchedposition = new OMButton("searchedposition", 0, 0, 0, 0);
            //searchedposition.Text = "Searched Weather";
            //searchedposition.OnClick += new userInteraction(searchedposition_onclick);

            ////OMBasicShape locationbackground = new OMBasicShape("locationbackground", 350, 35, 300, 55);
            ////OMBasicShape locationbackground = new OMBasicShape("locationbackground",(int)(float)(theHost.ClientArea_Init.Width*.35),theHost.ClientArea_Init.Top-6,(int)(float)(theHost.ClientArea_Init.Width*.3),55);
            //OMBasicShape locationbackground = new OMBasicShape("locationbackground", (int)(float)(theHost.ClientArea_Init.Width * .35), theHost.ClientArea_Init.Top - 6, (int)(float)(theHost.ClientArea_Init.Width * .3), (int)(float)(theHost.ClientArea_Init.Height * .112) + 6);
            //locationbackground.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.Gray, Color.White, 1);
            ////OMImage currentimage = new OMImage("currentimage", 151, 41, 198, 98);

            return eLoadStatus.LoadSuccessful;

        }

        private void BackgroundPanelLoad()
        {
            displayed = new string[theHost.ScreenCount];
            degrees = new string[theHost.ScreenCount];
            degreessettings = new string[theHost.ScreenCount];
            currentpositiondisplayed = new bool[theHost.ScreenCount];
            settings = new Settings("Weather Settings");
            ps = new PluginSettings();
            alreadyaddedstrip = new bool[theHost.ScreenCount];
            favoritesCounter = new int[theHost.ScreenCount];
            favoritesList = new List<string>[theHost.ScreenCount];
            PopUpMenuStrip = new ButtonStrip[theHost.ScreenCount];
            lastSearched = new string[theHost.ScreenCount];
            current = new bool[theHost.ScreenCount];
            realSearchedArea = new List<string>[theHost.ScreenCount];

            searchResults = new Dictionary<string, object>[theHost.ScreenCount];

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                searchResults[i] = new Dictionary<string, object>();
                displayed[i] = "";
                degrees[i] = "";
                alreadyaddedstrip[i] = false;
                favoritesList[i] = new List<string>();
                realSearchedArea[i] = new List<string>() {""};
                //string favoritesCount = StoredData.Get(this, String.Format("Favorites.{0}.Count", i.ToString()));
                //if(favoritesCount != "")
                //{
                //    favoritesCounter[i] = Convert.ToInt32(favoritesCount);
                //    for (int j = 0; j < favoritesCounter[i]; j++)
                //    {
                //        favoritesList[i].Add(StoredData.Get(this, "Favorites." + i.ToString() + "." + j.ToString()));
                //        realSearchedArea[i].Add(StoredData.Get(this, "Favorites.Searched" + i.ToString() + "." + j.ToString()));
                //    }
                //}
                //else
                //    favoritesCounter[i] = 0;
                PopUpMenuStrip[i] = new ButtonStrip(this.pluginName, "OMWeather2", "PopUpMenuStrip");
                
                object value;
                OM.Host.DataHandler.GetDataSourceValue("Weather.Favorites.List" + i.ToString(), new object[] { }, out value);
                if (value != null)
                    favoritesList[i] = (List<string>)value;
                for (int j = 0; j < favoritesList[i].Count; j++)
                {
                    if (j == 0)
                        PopUpMenuStrip[i].Buttons.Add(Button.CreateMenuItem("popupcurrentposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Current Position", false, currentgps_onclick, null, null));
                    else if (j == 1)
                        PopUpMenuStrip[i].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Click To Search Weather", false, currentsearched_onclick, null, null));
                    else
                        PopUpMenuStrip[i].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + j.ToString() + "." + favoritesList[i][j], theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), favoritesList[i][j], false, favorite_onclick, favorite_onholdclick, null));
                }
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Searched.List{0}", i.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Info.Status{0}", i.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Favorites.List{0}", i.ToString()), Subscription_Updated);
            }

            OMPanel weatherPanel = new OMPanel("OMWeather2", "Weather");

            OMButton currentBackground = OMButton.PreConfigLayout_BasicStyle("currentbackground", (int)(float)(theHost.ClientArea_Init.Width * .15), theHost.ClientArea_Init.Top - 6, (int)(float)(theHost.ClientArea_Init.Width * .70), (int)(float)(theHost.ClientArea_Init.Height * .213) + 6, GraphicCorners.All);
            currentBackground.BorderColor = Color.White;
            currentBackground.BorderSize = 1;
            currentBackground.FocusImage = imageItem.NONE;
            currentBackground.DownImage = imageItem.NONE;
            weatherPanel.addControl(currentBackground);
            OMImage currentimage = new OMImage("currentimage", currentBackground.Region.Left + 40, currentBackground.Region.Top - 5, 120, 120); //currentbackground.Region.Height - 15);
            weatherPanel.addControl(currentimage);
            OMLabel currenttemp = new OMLabel("currenttemp", currentBackground.Region.Right - 200, currentBackground.Region.Top + 10, 200, currentBackground.Region.Height - 15);
            currenttemp.TextAlignment = Alignment.CenterCenter;
            currenttemp.AutoFitTextMode = FitModes.FitSingleLine;
            currenttemp.FontSize = 40;
            weatherPanel.addControl(currenttemp);
            OMLabel currentdesc = new OMLabel("currentdesc", currentBackground.Region.Left, currentBackground.Region.Top, currentBackground.Region.Width, currentBackground.Region.Height - 7);
            currentdesc.TextAlignment = Alignment.WordWrap | Alignment.BottomCenter;
            currentdesc.AutoFitTextMode = FitModes.FitSingleLine;
            currentdesc.FontSize = 32;
            weatherPanel.addControl(currentdesc);
            OMButton locationButton = new OMButton("locationbutton", currentBackground.Region.Left, currentBackground.Region.Top + 10, currentBackground.Region.Width, currentBackground.Region.Height - 15);
            locationButton.Text = "Click To Search Weather";
            locationButton.TextAlignment = Alignment.TopCenter;
            locationButton.AutoFitTextMode = FitModes.Fit;
            locationButton.Opacity = 178;
            locationButton.OnClick += new userInteraction(locationbutton_onclick);
            weatherPanel.addControl(locationButton);

            OMContainer mainContainer = new OMContainer("mainContainer", theHost.ClientArea_Init.Left, currentimage.Top + currentimage.Height + 5, theHost.ClientArea_Init.Width, theHost.ClientArea_Init.Top + theHost.ClientArea_Init.Height - (currentimage.Top + currentimage.Height + 40)); //341=height
            mainContainer.Visible_DataSource = "OMWeather.mainContainer.Visible";
            mainContainer.ScrollBar_ColorNormal = Color.Transparent;
            mainContainer.BackgroundColor = Color.Transparent;
            mainContainer.Opacity = 0;
            weatherPanel.addControl(mainContainer);
            
            OMLabel updatedTime = new OMLabel("updatedTime", theHost.ClientArea_Init.Left, theHost.ClientArea_Init.Bottom - 35, theHost.ClientArea_Init.Width, 35);
            updatedTime.Text = "";
            updatedTime.Opacity = 80;
            updatedTime.FontSize = 14;
            weatherPanel.addControl(updatedTime);
            
            OMBasicShape searchProgressBackground = new OMBasicShape("searchProgressBackground", theHost.ClientArea_Init.Left, theHost.ClientArea_Init.Top, theHost.ClientArea_Init.Width, theHost.ClientArea_Init.Height, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
            searchProgressBackground.Visible = false;
            weatherPanel.addControl(searchProgressBackground);
            
            OMImage searchProgressImage = new OMImage("searchProgressImage", 250, 200, 100, 80);
            searchProgressImage.Visible = false;
            searchProgressImage.Image = theHost.getSkinImage("BusyAnimationTransparent.gif");
            weatherPanel.addControl(searchProgressImage);
            
            OMLabel searchProgressLabel = new OMLabel("searchProgressLabel", 350, 200, 400, 80);
            searchProgressLabel.Text = "Please wait, refreshing data...";
            searchProgressLabel.Visible = false;
            weatherPanel.addControl(searchProgressLabel);

            weatherPanel.Entering += new PanelEvent(omweather_entering);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);

            OMPanel alertPanel = new OMPanel("alertPanel", "Weather -> Alerts");

            manager.loadPanel(weatherPanel, true);
            asynchedLastSearch();
        }

        bool needToUpdate = true;
        private void Subscription_Updated(DataSource sensor)
        {
            if (manager == null)
                return;
            if (sensor.Value == null)
                return;
            int screen = 0;
            if (sensor.NameLevel2 == "Info")
            {
                if (sensor.Value.ToString() == "Searching")
                {
                    screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 6));
                    //visible
                    ((OMBasicShape)manager[screen, "OMWeather2"]["searchProgressBackground"]).Visible = true;
                    ((OMImage)manager[screen, "OMWeather2"]["searchProgressImage"]).Visible = true;
                    ((OMLabel)manager[screen, "OMWeather2"]["searchProgressLabel"]).Visible = true;
                }
                else
                {
                    screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 6));
                    //not visible
                    ((OMBasicShape)manager[screen, "OMWeather2"]["searchProgressBackground"]).Visible = false;
                    ((OMImage)manager[screen, "OMWeather2"]["searchProgressImage"]).Visible = false;
                    ((OMLabel)manager[screen, "OMWeather2"]["searchProgressLabel"]).Visible = false;
                }
            }
            else
            {
                screen = Convert.ToInt32(sensor.NameLevel3.Remove(0, 4));
                if (sensor.NameLevel2 == "Favorites")
                {
                    favoritesList[screen] = (List<string>)sensor.Value;
                    UpdateFavorites(screen);
                }
                else
                {
                    searchResults[screen] = (Dictionary<string, object>)sensor.Value;
                    //if (needToUpdate)
                    //{
                    Update(screen);
                    //    needToUpdate = false;
                    //}
                    //else
                    //    needToUpdate = true;
                }
            }
        }

        private void UpdateFavorites(int screen)
        {
            if(favoritesList[screen].Count > PopUpMenuStrip[screen].Buttons.Count)
            {
                //add a new favorite button
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + screen.ToString() + "." + favoritesList[screen][favoritesList[screen].Count - 1], theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), favoritesList[screen][favoritesList[screen].Count - 1], false, favorite_onclick, favorite_onholdclick, null));
            }
            else
            {
                //need to remove
                for (int i = 2; i < favoritesList[screen].Count; i++)
                {
                    //compare the current displayed list to the updated list we now have
                    //we start indexing at 2 because of the current location/last searched location in our popup strip
                    if (((Button)(PopUpMenuStrip[screen].Buttons[i])).Name != "popupfavoriteposition." + screen.ToString() + "." + favoritesList[screen][i])
                    {
                        //this is the one we remove
                        PopUpMenuStrip[screen].Buttons.RemoveAt(i);
                        break;
                    }
                }
            }
            
        }

        private void theHost_OnPowerChange(OpenMobile.ePowerEvent e)
        {
            switch (e)
            {
                case ePowerEvent.SleepOrHibernatePending:
                    Killing();
                    break;
                case ePowerEvent.ShutdownPending:
                    Killing();
                    break;
            }
        }

        private void Killing()
        {
            for (int screen = 0; screen < theHost.ScreenCount; screen++)
            {
                for (int i = 0; i < favoritesCounter[screen]; i++)
                {
                    StoredData.Set(this, "Favorites." + screen.ToString() + "." + i.ToString(), favoritesList[screen][i]);
                    StoredData.Set(this, "Favorites.Searched" + screen.ToString() + "." + i.ToString(), realSearchedArea[screen][i + 1]);
                }
                StoredData.Set(this, "Favorites." + screen.ToString() + ".Count", favoritesCounter[screen].ToString());
                StoredData.Set(this, "Favorites." + screen.ToString() + ".Searched", lastSearched[screen]);
                StoredData.Set(this, "Favorites.Searched" + screen.ToString() + ".Searched", lastSearched[screen]);
                if (current[screen])
                    StoredData.Set(this, "Favorites." + screen.ToString() + ".Current", "True");
                else
                    StoredData.Set(this, "Favorites." + screen.ToString() + ".Current", "False");
            }
        }

        private void omweather_entering(OMPanel omweather, int screen)
        {

            if (!alreadyaddedstrip[screen])
            {
                //PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupcurrentposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Current Position", false, currentgps_onclick, null, null));
                //PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Click To Search Weather", false, currentsearched_onclick, null, null));
                //favoritesList[screen] = (List<string>)(object)OM.Host.DataHandler.GetDataSource(String.Format("Weather.Favorites.List{0}", screen.ToString()));
                //for (int i = 0; i < favoritesList[screen].Count; i++)
                //{
                //    PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + screen.ToString() + "." + favoritesList[screen][i], theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), favoritesList[screen][i], false, favorite_onclick, favorite_onholdclick, null));
                //}










                //if (StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched") != "")
                //{
                    //lastSearched[screen] = StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched");
                    //realSearchedArea[screen][0] = StoredData.Get(this, "Favorites.Searched" + screen.ToString() + ".Searched");
                    //if (StoredData.Get(this, "Favorites." + screen.ToString() + ".Current") == "True")
                    //    current[screen] = true;
                    //else
                    //    current[screen] = false;
                //}
                //else
                //    lastSearched[screen] = "";

                //SafeThread.Asynchronous(delegate { asynchedLastSearch(screen); }, theHost);
                //if (lastSearched[screen] != "")
                //{
                //    if (!favoritesList[screen].Contains(lastSearched[screen]))
                //        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), lastSearched[screen] + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null));
                //    else
                //        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), lastSearched[screen], false, currentsearched_onclick, null, null));
                //}
                //else
                    

                //get favorites that were saved
                //object value = OM.Host.DataHandler.GetDataSource(String.Format("Weather.Favorites.List{0}", screen.ToString()));
                
                alreadyaddedstrip[screen] = true;
            }
            theHost.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip[screen]);
        }

        private void asynchedLastSearch()
        {
            OM.Host.CommandHandler.ExecuteCommand("Weather.Refresh.Weather");
            ////theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { lastSearched[screen], screen });
            //if(current[screen] == true)
            //    theHost.CommandHandler.ExecuteCommand("Weather.Search.CurrentLocation", new object[] { screen });
            //else
            //    theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { realSearchedArea[screen][0], screen });
        }

        private void currentgps_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = true;
            OM.Host.CommandHandler.ExecuteCommand("Weather.Search.CurrentLocation", new object[] { screen });
        }

        private void currentsearched_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = false;
            if (((OMButton)sender).Text != "Click To Search Weather") //successful search at least once
            {
                OM.Host.CommandHandler.ExecuteCommand("Weather.Favorites.Show", new object[] { screen, 1 });
                //OM.Host.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { realSearchedArea[screen][0], screen });
                //theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { ((OMButton)sender).Text, screen });
            }
            else
            {
                locationbutton_onclick(sender, screen);
            }
        }

        private void currentsearched_onholdclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            //add to favorites
            if (((OMButton)sender).Text == "Click To Search Weather")
                return;
            if (current[screen])
                return;
            string[] split = ((OMButton)sender).Text.Split('\n');
            if (favoritesList[screen].Contains(split[0]))
                return;

            //we can now add to favorites
            OM.Host.CommandHandler.ExecuteCommand("Weather.Favorites.Add", new object[] { screen, split[0].Trim() });

            //PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + screen.ToString() + "." + split[0].Trim(), theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), split[0].Trim(), false, favorite_onclick, favorite_onholdclick, null));
            //favoritesList[screen].Add(split[0].Trim());
            //realSearchedArea[screen].Add(realSearchedArea[screen][0]);
            //OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Favorite Location Weather Added:\n" + split[0].Trim(), 3000));
            //favoritesCounter[screen] += 1;
        }

        private void favorite_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = false;
            OM.Host.CommandHandler.ExecuteCommand("Weather.Favorites.Show", new object[] { screen, favoritesList[screen].IndexOf(((OMButton)sender).Text) });
            //OM.Host.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { realSearchedArea[screen][favoritesList[screen].IndexOf(((OMButton)sender).Text) + 1], screen });
            //theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { ((OMButton)sender).Text, screen });
        }

        private void favorite_onholdclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            OM.Host.CommandHandler.ExecuteCommand("Weather.Favorites.Remove", new object[] { screen, favoritesList[screen].IndexOf(((OMButton)sender).Text) });
            return;
            //remove
            PopUpMenuStrip[screen].Buttons.Remove("popupfavoriteposition." + screen.ToString() + "." + ((OMButton)sender).Text);
            favoritesList[screen].Remove(((OMButton)sender).Text);
            realSearchedArea[screen].RemoveAt(favoritesList[screen].IndexOf(((OMButton)sender).Text) + 1);
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Favorite Location Weather Removed:\n" + ((OMButton)sender).Text));
            favoritesCounter[screen] -= 1;
        }

        private void currentposition_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            if ((displayed[screen] == "currentpositionforecast") || (displayed[screen] == "currentpositionhourly"))
                return;
            else if (displayed[screen] != "")
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "currentpositionforecast");
            displayed[screen] = "currentpositionforecast";
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void searchedposition_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            if ((displayed[screen] == "forecast") || (displayed[screen] == "hourly") || (displayed[screen] == "locations"))
                return;
            else if (displayed[screen] != "")
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
            displayed[screen] = "forecast";
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void locationbutton_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            string searchResult = OSK.ShowDefaultOSK(screen, "", "Type something", "Please input something now", OSKInputTypes.Keypad, false);
            if ((searchResult != null) && (searchResult != ""))
            {
                theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { searchResult, screen });
            }
        }

        private void search_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = false;

            OMTextBox locationtextbox = (OMTextBox)manager[screen, "OMWeather2"]["locationstextbox"];
            //OMLabel searching = (OMLabel)manager[screen, "locations"]["searching"];
            if (locationtextbox.Text == "")
                return;
            else
            {
                if (theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { locationtextbox.Text, screen }) != null)
                {
                    searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                    Update(screen);
                }
            }
        }

        private void locationslist_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            OMList locationslist = (OMList)sender;
            if (locationslist.SelectedIndex == -1)
                return;
            else if (locationslist.SelectedIndex == 0) //clear
            {
                ((OMTextBox)manager[screen, "locations"]["locationstextbox"]).Text = "";
                ((OMLabel)manager[screen, "locations"]["locationslabel"]).Text = "Please enter zip code or city and state...";
                locationslist.Visible = false;
                locationslist.Clear();
            }
            else
            {
                //get weather for the url picked -> maybe search the area instead since dataSource calls SearchArea()???
                ((OMTextBox)manager[screen, "locations"]["locationstextbox"]).Text = locationslist.SelectedItem.ToString();
                locationslist.Visible = false;
                locationslist.Clear();
                search_onclick(sender, screen);
            }
        }

        private void clearsearch_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            OMTextBox locationtextbox = (OMTextBox)manager[screen, "locations"]["locationstextbox"];
            OMButton clearsearch = (OMButton)sender;
        }

        private void Update(int screen)
        {
            if (searchResults[screen].Keys.Contains("Area"))
                UpdateWeather(screen);
            else if (searchResults[screen].Keys.Contains("MultipleResults"))
                UpdateMulti(screen);

        }

        private void UpdateWeather(int screen)
        {
            OMPanel p = manager[screen, "OMWeather2"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];
            mainContainer.ClearControls();

            if(searchResults[screen].Keys.Contains("RealSearchedArea"))
                realSearchedArea[screen][0] = searchResults[screen]["RealSearchedArea"].ToString();

            //update the top controls
            ((OMButton)p["locationbutton"]).Text = searchResults[screen]["Area"].ToString();
            ((OMLabel)p["currentdesc"]).Text = searchResults[screen]["Description"].ToString();
            ((OMLabel)p["currenttemp"]).Text = searchResults[screen]["Temp"].ToString();
            ((OMImage)p["currentimage"]).Image = (imageItem)searchResults[screen]["Image"];
            ((OMLabel)p["updatedTime"]).Text = searchResults[screen]["UpdatedTime"].ToString();

            //update the bottom controls
            //((OMLabel)p["updatedTime"]).Text = searchResults[screen]["UpdatedTime"].ToString();
            string[] split = ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Split('\n');
            lastSearched[screen] = split[0].Trim();
            if (!favoritesList[screen].Contains(split[0].Trim()))
            {
                PopUpMenuStrip[screen].Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
                //OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
            }
            else
            {
                PopUpMenuStrip[screen].Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);
                //OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);
            }

            //create the forecastdata
            int i = 1;
            int xcord = 10;
            while (searchResults[screen].Keys.Contains("ForecastDay" + i.ToString()))
            {
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), 10, 150, 180, 380, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 150, 180, 341, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 0, 180, mainContainer.Height - 10, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 0, 180, 300, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMLabel forecastday = new OMLabel("forecastday" + i.ToString(), shapeBackground.Left + 5, shapeBackground.Top + 5, shapeBackground.Width - 10, (int)(shapeBackground.Height * .12));
                forecastday.Text = searchResults[screen]["ForecastDay" + i.ToString()].ToString();
                forecastday.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMImage forecastimage = new OMImage("forecastimage" + i.ToString(), shapeBackground.Left + 5, forecastday.Top + forecastday.Height, shapeBackground.Width - 10, (int)(shapeBackground.Height * .40));
                forecastimage.Image = (imageItem)searchResults[screen]["ForecastImage" + i.ToString()];
                OMLabel forecastdesc = new OMLabel("forecastdesc" + i.ToString(), shapeBackground.Left + 5, forecastimage.Top + forecastimage.Height, shapeBackground.Width - 10, (int)(shapeBackground.Height * .24));
                forecastdesc.Text = searchResults[screen]["ForecastDescription" + i.ToString()].ToString();
                forecastdesc.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
                forecastdesc.AutoFitTextMode = FitModes.FitFill;
                OMLabel forecasthigh = new OMLabel("forecasthigh" + i.ToString(), shapeBackground.Left + 5, forecastdesc.Top + forecastdesc.Height, shapeBackground.Width - 10, (int)(shapeBackground.Height * .12));
                forecasthigh.Text = searchResults[screen]["ForecastHigh" + i.ToString()].ToString();
                forecasthigh.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMLabel forecastlow = new OMLabel("forecastlow" + i.ToString(), shapeBackground.Left + 5, forecasthigh.Top + forecasthigh.Height, shapeBackground.Width - 10, (int)(shapeBackground.Height * .12));
                forecastlow.Text = searchResults[screen]["ForecastLow" + i.ToString()].ToString();
                forecastlow.AutoFitTextMode = FitModes.FitFillSingleLine;

                mainContainer.addControlRelative(shapeBackground);
                mainContainer.addControlRelative(forecastday);
                mainContainer.addControlRelative(forecastimage);
                mainContainer.addControlRelative(forecastdesc);
                mainContainer.addControlRelative(forecasthigh);
                mainContainer.addControlRelative(forecastlow);
                xcord += 190;
                i++;
            }

        }

        private void UpdateMulti(int screen)
        {
            OMPanel p = manager[screen, "OMWeather2"];
            OMContainer mainContainer = (OMContainer)p["mainContainer"];
            mainContainer.ClearControls();

            //create the list of multiple locations
            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", 150, 0, 700, 336, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            OMLabel multipleLabel = new OMLabel("multipleLabel", 155, 5, 690, 30);
            multipleLabel.Text = "Multiple Results Found, Please Choose A Location";
            OMBasicShape shapeBackgroundList = new OMBasicShape("shapeBackgroundList", 155, 35, 690, shapeBackground.Height - 40, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            OMList multipleLocations = new OMList("multipleLocations", 160, 40, 680, shapeBackgroundList.Height - 10);
            multipleLocations.ListItemHeight = 40;
            multipleLocations.FontSize = 24;
            //multipleLocations.SoftEdgeData.Color1 = Color.Black;
            //multipleLocations.SoftEdgeData.Sides = FadingEdge.GraphicSides.Top | FadingEdge.GraphicSides.Bottom;
            //multipleLocations.UseSoftEdges = true;
            //multipleLocations.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5);
            //multipleLocations.ItemColor1 = Color.FromArgb(125, Color.Black);
            multipleLocations.ItemColor1 = Color.Transparent;
            multipleLocations.ItemColor2 = Color.Transparent;
            multipleLocations.OnClick += new userInteraction(multipleLocations_OnClick);
            //multipleLocations.ListStyle = eListStyle.MultiList;
            //multipleLocations.ShapeData = new ShapeData(shapes.RoundedRectangle);



            mainContainer.addControlRelative(shapeBackground);
            mainContainer.addControlRelative(multipleLabel);
            mainContainer.addControlRelative(shapeBackgroundList);
            mainContainer.addControlRelative(multipleLocations);

            for (int i = 1; i < searchResults[screen].Keys.Count - 1; i++)
            {
                //OMListItem listItem = new OMListItem(searchResults[screen].Keys.ElementAt(i).ToString());
                //listItem.subitemFormat = new OMListItem.subItemFormat();
                //listItem.image = OImage.FromFont(690, 30, searchResults[screen].Keys.ElementAt(i).ToString(), multipleLocations.Font, eTextFormat.OutlineFat, Alignment.CenterCenter, Color.White, Color.White);
                //listItem.subitemFormat.textFormat = eTextFormat.Outline;
                //listItem.subitemFormat.textFormat = eTextFormat.OutlineFat;
                //multipleLocations.Add(listItem);

                multipleLocations.Add(searchResults[screen].Keys.ElementAt(i).ToString());
            }

        }

        private void multipleLocations_OnClick(OMControl sender, int screen)
        {
            string text = ((OMList)sender).SelectedItem.text;
            theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { text, screen });
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
            get { return "OMWeather2"; }
        }
        public string displayName
        {
            get { return "Weather2"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Weather Information"; }
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
            Killing();
        }

    } //end class1
} //end namespace