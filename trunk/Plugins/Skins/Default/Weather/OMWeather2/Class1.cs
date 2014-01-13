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
        Dictionary<string, object>[] searchResults;

        public eLoadStatus initialize(IPluginHost host)
        {

            theHost = host;
            manager = new ScreenManager(this);
            //settings = new PluginSettings();
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

            searchResults = new Dictionary<string, object>[theHost.ScreenCount];

            OMPanel omweather = new OMPanel("OMWeather2", "Weather");

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                searchResults[i] = new Dictionary<string, object>();
                displayed[i] = "";
                degrees[i] = "";
                alreadyaddedstrip[i] = false;
                favoritesList[i] = new List<string>();
                //currenttemp.Text = "{Weather.Searched-" + i.ToString() + ".SearchedTemp}";
                if (StoredData.Get(this, String.Format("Favorites.{0}.Count", i.ToString())) != "")
                {
                    favoritesCounter[i] = Convert.ToInt32(StoredData.Get(this, String.Format("Favorites.{0}.Count", i.ToString())));
                    for (int j = 0; j < favoritesCounter[i]; j++)
                    {
                        favoritesList[i].Add(StoredData.Get(this, "Favorites." + i.ToString() + "." + j.ToString()));
                    }
                }
                else
                    favoritesCounter[i] = 0;
                PopUpMenuStrip[i] = new ButtonStrip(this.pluginName, omweather.Name, "PopUpMenuStrip");
                theHost.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Searched.List{0}", i.ToString()), Subscription_Updated);
                theHost.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Info.Status{0}", i.ToString()), Subscription_Updated);
            }







            OMButton current_background = OMButton.PreConfigLayout_BasicStyle("currentbackground", (int)(float)(theHost.ClientArea[0].Width * .15), theHost.ClientArea[0].Top - 6, (int)(float)(theHost.ClientArea[0].Width * .70), (int)(float)(theHost.ClientArea[0].Height * .213) + 6, GraphicCorners.All);
            current_background.BorderColor = Color.White;
            current_background.BorderSize = 1;
            current_background.FocusImage = imageItem.NONE;
            current_background.DownImage = imageItem.NONE;

            OMBasicShape currentbackground = new OMBasicShape("currentbackground", (int)(float)(theHost.ClientArea[0].Width * .15), theHost.ClientArea[0].Top - 6, (int)(float)(theHost.ClientArea[0].Width * .70), (int)(float)(theHost.ClientArea[0].Height * .213) + 6);
            currentbackground.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.Gray, Color.White, 1);

            OMButton currentposition = new OMButton("currentposition", 0, 0, 0, 0);
            currentposition.Text = "Current Position Weather";
            currentposition.OnClick += new userInteraction(currentposition_onclick);
            OMButton searchedposition = new OMButton("searchedposition", 0, 0, 0, 0);
            searchedposition.Text = "Searched Weather";
            searchedposition.OnClick += new userInteraction(searchedposition_onclick);

            //OMBasicShape locationbackground = new OMBasicShape("locationbackground", 350, 35, 300, 55);
            //OMBasicShape locationbackground = new OMBasicShape("locationbackground",(int)(float)(theHost.ClientArea[0].Width*.35),theHost.ClientArea[0].Top-6,(int)(float)(theHost.ClientArea[0].Width*.3),55);
            OMBasicShape locationbackground = new OMBasicShape("locationbackground", (int)(float)(theHost.ClientArea[0].Width * .35), theHost.ClientArea[0].Top - 6, (int)(float)(theHost.ClientArea[0].Width * .3), (int)(float)(theHost.ClientArea[0].Height * .112) + 6);
            locationbackground.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.Gray, Color.White, 1);
            //OMImage currentimage = new OMImage("currentimage", 151, 41, 198, 98);

            //OMBasicShape shpDay1Background = new OMBasicShape("shpDay1Background", 10, 150, 180, 380,
            //    new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            //shpDay1Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh1";
            ////omweather.addControl(shpDay1Background);
            //OMBasicShape shpDay2Background = new OMBasicShape("shpDay2Background", shpDay1Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
            //    new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            //shpDay2Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh2";
            ////omweather.addControl(shpDay2Background);
            //OMBasicShape shpDay3Background = new OMBasicShape("shpDay4Background", shpDay2Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
            //    new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            //shpDay3Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh3";
            ////omweather.addControl(shpDay3Background);
            //OMBasicShape shpDay4Background = new OMBasicShape("shpDay5Background", shpDay3Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
            //    new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            //shpDay4Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh4";
            ////omweather.addControl(shpDay4Background);
            //OMBasicShape shpDay5Background = new OMBasicShape("shpDay6Background", shpDay4Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
            //    new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            //shpDay5Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh5";
            ////omweather.addControl(shpDay5Background);

            OMButton location_button = new OMButton("locationbutton", currentbackground.Region.Left, currentbackground.Region.Top + 10, currentbackground.Region.Width, currentbackground.Region.Height - 15);
            //OMButton location_button = new OMButton("locationbutton", 351, 41, 298, 48);
            ////////location_button.DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
            location_button.Text = "Click To Search Weather";
            location_button.TextAlignment = Alignment.TopCenter;
            location_button.AutoFitTextMode = FitModes.Fit;
            location_button.Opacity = 178;
            //location_button.BorderColor = Color.White;
            //location_button.BorderSize = 0;
            location_button.OnClick += new userInteraction(locationbutton_onclick);

            //OMImage currentimage = new OMImage("currentimage", current_background.Left + 1, theHost.ClientArea[0].Top, location_button.Left - current_background.Left, current_background.Height - 5);
            OMImage currentimage = new OMImage("currentimage", currentbackground.Region.Left + 40, currentbackground.Region.Top - 5, 120, 120); //currentbackground.Region.Height - 15);
            ////////currentimage.DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);

            //OMLabel currenttemp = new OMLabel("currenttemp", location_button.Left + location_button.Width, theHost.ClientArea[0].Top, 198, 98);
            OMLabel currenttemp = new OMLabel("currenttemp", currentbackground.Region.Right - 200, currentbackground.Region.Top + 10, 200, currentbackground.Region.Height - 15);
            currenttemp.TextAlignment = Alignment.CenterCenter;
            currenttemp.AutoFitTextMode = FitModes.FitSingleLine;
            currenttemp.FontSize = 40;
            ////////currenttemp.DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);

            //OMLabel currentdesc = new OMLabel("currentdesc", 351, 91, 289, 48);
            OMLabel currentdesc = new OMLabel("currentdesc", currentbackground.Region.Left, currentbackground.Region.Top, currentbackground.Region.Width, currentbackground.Region.Height - 7);
            currentdesc.TextAlignment = Alignment.WordWrap | Alignment.BottomCenter;
            currentdesc.AutoFitTextMode = FitModes.FitSingleLine;
            currentdesc.FontSize = 32;
            ////////currentdesc.DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
            //currentdesc.Text = "Cloudy";



            omweather.addControl(currentposition);
            omweather.addControl(searchedposition);
            omweather.addControl(current_background);
            omweather.addControl(currentimage);
            omweather.addControl(currentdesc);
            omweather.addControl(location_button);
            omweather.addControl(currenttemp);

            OMContainer mainContainer = new OMContainer("mainContainer", theHost.ClientArea[0].Left, currentimage.Top + currentimage.Height + 5, theHost.ClientArea[0].Width, theHost.ClientArea[0].Top + theHost.ClientArea[0].Height - (currentimage.Top + currentimage.Height + 40)); //341=height
            mainContainer.Visible_DataSource = "OMWeather.mainContainer.Visible";
            mainContainer.ScrollBar_ColorNormal = Color.Transparent;
            mainContainer.BackgroundColor = Color.Transparent;
            mainContainer.Opacity = 0;
            omweather.addControl(mainContainer);

            OMLabel updatedTime = new OMLabel("updatedTime", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top + theHost.ClientArea[0].Height - 35, theHost.ClientArea[0].Width, 35);
            updatedTime.Text = "";
            updatedTime.Opacity = 80;
            updatedTime.FontSize = 14;
            omweather.addControl(updatedTime);

            OMBasicShape searchProgressBackground = new OMBasicShape("searchProgressBackground", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
            //searchProgressBackground.Visible_DataSource = String.Format("Screen{0}.Weather.Visible.SearchProgressBackground", DataSource.DataTag_Screen);
            searchProgressBackground.Visible = false;
            omweather.addControl(searchProgressBackground);
            OMImage searchProgressImage = new OMImage("searchProgressImage", 250, 200, 100, 80);
            //searchProgressImage.Visible_DataSource = String.Format("Screen{0}.Weather.Visible.SearchProgressImage", DataSource.DataTag_Screen);
            searchProgressImage.Visible = false;
            searchProgressImage.Image = theHost.getSkinImage("BusyAnimationTransparent.gif");
            omweather.addControl(searchProgressImage);
            OMLabel searchProgressLabel = new OMLabel("searchProgressLabel", 350, 200, 400, 80);
            searchProgressLabel.Text = "Please wait, refreshing data...";
            //searchProgressLabel.Visible_DataSource = String.Format("Screen{0}.Weather.Visible.SearchProgressLabel", DataSource.DataTag_Screen);
            searchProgressLabel.Visible = false;
            omweather.addControl(searchProgressLabel);


            omweather.Entering += new PanelEvent(omweather_entering);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            //theHost.DataHandler.SubscribeToDataSource(String.Format("OMDSWeather;Weather.Searched.List{0}", DataSource.DataTag_Screen), Subscription_Updated);
            //theHost.DataHandler.SubscribeToDataSource("OMDSWeather;Weather.Searched.List{:S:}", Subscription_Updated);

            manager.loadPanel(omweather, true);



            return eLoadStatus.LoadSuccessful;

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
                searchResults[screen] = (Dictionary<string, object>)sensor.Value;
                if (needToUpdate)
                {
                    Update(screen);
                    needToUpdate = false;
                }
                else
                    needToUpdate = true;
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
                }
                StoredData.Set(this, "Favorites." + screen.ToString() + ".Count", favoritesCounter[screen].ToString());
                StoredData.Set(this, "Favorites." + screen.ToString() + ".Searched", lastSearched[screen]);
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
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupcurrentposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Current Position", false, currentgps_onclick, null, null));

                if (StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched") != "")
                {
                    lastSearched[screen] = StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched");
                    if (StoredData.Get(this, "Favorites." + screen.ToString() + ".Current") == "True")
                        current[screen] = true;
                    else
                        current[screen] = false;
                    SafeThread.Asynchronous(delegate { asynchedLastSearch(screen); }, theHost);
                }
                else
                    lastSearched[screen] = "";

                if (lastSearched[screen] != "")
                {
                    if (!favoritesList[screen].Contains(lastSearched[screen]))
                        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), lastSearched[screen] + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null));
                    else
                        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), lastSearched[screen], false, currentsearched_onclick, null, null));
                }
                else
                    PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Click To Search Weather", false, currentsearched_onclick, null, null));

                //get favorites that were saved
                for (int i = 0; i < favoritesList[screen].Count; i++)
                {
                    PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + screen.ToString() + "." + favoritesList[screen][i], theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), favoritesList[screen][i], false, favorite_onclick, favorite_onholdclick, null));
                }
                alreadyaddedstrip[screen] = true;
            }
            theHost.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip[screen]);

            return;
        }

        private void asynchedLastSearch(int screen)
        {
            //theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { lastSearched[screen], screen });
            theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { lastSearched[screen], screen });
            return;


            if (theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { lastSearched[screen], screen }) != null)
            {
                searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                Update(screen);
            }
            return;



            if (current[screen])
            {
                ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = "Weather.Current.Current.Area";
                ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = "Weather.Current.Current.Description";
                ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = "Weather.Current.Current.Temp";
                ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = "Weather.Current.Current.Image";
                for (int k = 0; k < 5; k++)
                {
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastDay" + (k + 1).ToString();
                    ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastImage" + (k + 1).ToString();
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastDescription" + (k + 1).ToString();
                    ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastHigh" + (k + 1).ToString();
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastLow" + (k + 1).ToString();
                }
            }
            else
            {
                ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);
                ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);
                for (int k = 0; k < 5; k++)
                {
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
                }
            }
            object value;
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);
            //update it here
            if (current[screen])
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.CurrentWeather", new object[] { screen.ToString() }, out value);
            else
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { lastSearched[screen], screen.ToString() }, out value);
            string[] split = ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Split('\n');
            //lastSearched[screen] = split[0].Trim();
            if (!favoritesList[screen].Contains(split[0].Trim()))
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
            else
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);
        }

        private void currentgps_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = true;

            theHost.CommandHandler.ExecuteCommand("Weather.Search.CurrentLocation", new object[] { screen });
            return;




            if (theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { screen }) != null)
            {
                searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                Update(screen);
            }
            return;

            //theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { lastSearched[screen], screen });


            ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = "Weather.Current.Current.Area";
            ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = "Weather.Current.Current.Description";
            ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = "Weather.Current.Current.Temp";
            ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = "Weather.Current.Current.Image";
            for (int k = 0; k < 5; k++)
            {
                ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastDay" + (k + 1).ToString();
                ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastImage" + (k + 1).ToString();
                ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastDescription" + (k + 1).ToString();
                ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastHigh" + (k + 1).ToString();
                ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = "Weather.Current.Current.ForecastLow" + (k + 1).ToString();
            }
            object value;
            //make sure controls are visible
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);
            //update it here
            theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.CurrentWeather", new object[] { screen }, out value);
            string[] split = ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Split('\n');
            lastSearched[screen] = split[0].Trim();
            if (!favoritesList[screen].Contains(split[0].Trim()))
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
            else
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);
            return;



            if (displayed[screen] != "forecast")
            {
                if (displayed[screen] != "")
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
                displayed[screen] = "forecast";
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }
            //update it here
            //object value;
            theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.CurrentWeather", new object[] { }, out value);

        }

        private void currentsearched_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = false;

            if (((OMButton)sender).Text != "Click To Search Weather") //successful search at least once
            {
                theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { ((OMButton)sender).Text, screen });
            }
            else
            {
                locationbutton_onclick(sender, screen);
            }
            return;





            if (((OMButton)sender).Text != "Click To Search Weather") //successful search at least once
            {
                if (theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { ((OMButton)sender).Text, screen }) != null)
                {
                    searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                    Update(screen);
                }
            }
            else
            {
                locationbutton_onclick(sender, screen);

            }
            return;


            object value;
            ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
            ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
            ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);
            ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);
            for (int k = 0; k < 5; k++)
            {
                ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
            }
            if (((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Contains("\n")) //successful search at least once
            {
                string[] split = ((OMButton)sender).Text.Split('\n');
                theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
                theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);
                //update it here
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { split[0].Trim(), screen.ToString() }, out value);
                return;


                if (displayed[screen] != "forecast")
                {
                    if (displayed[screen] != "")
                        theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
                    displayed[screen] = "forecast";
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                }
                //update it here

                //object value;
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { split[0].Trim(), screen.ToString() }, out value);
            }
            else
            {

                //object value;
                //make sure controls are visible
                theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { false, screen }, out value);
                theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { true, screen }, out value);
                return;


                //to locations panel so we can search something
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "locations");
                displayed[screen] = "locations";
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }
        }

        private void currentsearched_onholdclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            //add to favorites
            if (((OMButton)sender).Text == "Click To Search Weather")
                return;
            string[] split = ((OMButton)sender).Text.Split('\n');
            if (favoritesList[screen].Contains(split[0]))
                return;

            PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupfavoriteposition." + screen.ToString() + "." + split[0].Trim(), theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), split[0].Trim(), false, favorite_onclick, favorite_onholdclick, null));
            favoritesList[screen].Add(split[0].Trim());
            OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(InfoBanner.Styles.AnimatedBanner, "Favorite Location Weather Added:\n" + split[0].Trim(), 3000));
            favoritesCounter[screen] += 1;
        }

        private void favorite_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            current[screen] = false;

            theHost.CommandHandler.ExecuteCommand("Weather.Search.SearchLocation", new object[] { ((OMButton)sender).Text, screen });
            return;





            if (theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { ((OMButton)sender).Text, screen }) != null)
            {
                searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                Update(screen);
            }
            return;

            object value;
            ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
            ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
            ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);
            ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);
            for (int k = 0; k < 5; k++)
            {
                ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
            }

            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);
            //update it here
            theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { ((OMButton)sender).Text, screen.ToString() }, out value);
            OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), ((OMButton)sender).Text, false, favorite_onclick, favorite_onholdclick, null);
            //theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Replace(",", "+").Replace(" ", "+"), screen.ToString() }, out value);
            return;


            if (displayed[screen] != "forecast")
            {
                if (displayed[screen] != "")
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
                displayed[screen] = "forecast";
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }
        }

        private void favorite_onholdclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            //remove
            PopUpMenuStrip[screen].Buttons.Remove("popupfavoriteposition." + screen.ToString() + "." + ((OMButton)sender).Text);
            favoritesList[screen].Remove(((OMButton)sender).Text);
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
                return;

                if (theHost.CommandHandler.ExecuteCommand("OMDSWeather.Weather.Search", new object[] { searchResult, screen }) != null)
                {
                    searchResults[screen] = (Dictionary<string, object>)theHost.DataHandler.GetDataSource("Weather.Searched.List" + screen.ToString()).Value;
                    Update(screen);
                }
            }

            return;

            object value;
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { false, screen }, out value);
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { true, screen }, out value);

            //theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { false, screen }, out value);
            //theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { true, screen }, out value);
            return;

            if (displayed[screen] != "")
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "locations");
            displayed[screen] = "locations";
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
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
                return;





                object value;
                //theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { false, screen }, out value);

                //bring up searching stuff
                //searching.Visible = true;
                string searchtext = locationtextbox.Text;
                searchtext.Replace(" ", "+");
                searchtext.Replace(",", "+");
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { searchtext, screen.ToString() }, out value);
                //searching.Visible = false;
                if (value == null)
                {
                    //make sure we have the correct datasources hooked up
                    ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
                    ((OMLabel)manager[screen, "OMWeather2"]["currentdesc"]).DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
                    ((OMLabel)manager[screen, "OMWeather2"]["currenttemp"]).DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);
                    ((OMImage)manager[screen, "OMWeather2"]["currentimage"]).DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);
                    for (int k = 0; k < 5; k++)
                    {
                        ((OMLabel)manager[screen, "OMWeather2"]["forecastday" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                        ((OMImage)manager[screen, "OMWeather2"]["forecastimage" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                        ((OMLabel)manager[screen, "OMWeather2"]["forecastdesc" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                        ((OMLabel)manager[screen, "OMWeather2"]["forecasthigh" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                        ((OMLabel)manager[screen, "OMWeather2"]["forecastlow" + (k + 1).ToString()]).DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    }
                    theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
                    theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);

                    string[] split = ((OMButton)manager[screen, "OMWeather2"]["locationbutton"]).Text.Split('\n');
                    lastSearched[screen] = split[0].Trim();
                    if (!favoritesList[screen].Contains(split[0].Trim()))
                        OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
                    else
                        OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);
                    return;


                    displayed[screen] = "forecast";
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", "locations");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
                    //searching.Visible = false;
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    //}
                }
                else
                {
                    Dictionary<string, string> wi = (Dictionary<string, string>)value;
                    switch (wi.Keys.ElementAt(0))
                    {
                        case "MultipleResults": //get locationslist ready
                            OMList locationslist = (OMList)manager[screen, "OMWeather2"]["locationslist"];
                            locationslist.Clear();
                            locationslist.Add(new OMListItem("Clear Search Results", "Clear"));
                            ((OMLabel)manager[screen, "OMWeather2"]["locationslabel"]).Text = "Multiple Locations Found:\nPlease choose from the list below...";
                            //get results from wi (dictionary), key = area
                            for (int i = 1; i < wi.Count; i++)
                            {
                                locationslist.Add(new OMListItem(wi.Keys.ElementAt(i), wi[wi.Keys.ElementAt(i)]));
                            }
                            locationslist.Visible = true;
                            locationslist.Visible = true;
                            //searching.Visible = false;
                            break;
                        default: //nothing found
                            ((OMLabel)manager[screen, "OMWeather2"]["locationslabel"]).Text = "No Search Results Found:\nPlease enter zip code or city and state...";
                            break;
                    }
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
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), split[0].Trim() + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null);
            else
                OM.Host.UIHandler.PopUpMenu.GetButtonStrip(screen).Buttons["popupsearchedposition"] = Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), split[0].Trim(), false, currentsearched_onclick, null, null);

            //create the forecastdata
            int i = 1;
            int xcord = 10;
            while (searchResults[screen].Keys.Contains("ForecastDay" + i.ToString()))
            {
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), 10, 150, 180, 380, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 150, 180, 341, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 0, 180, 336, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                //OMBasicShape shapeBackground = new OMBasicShape("shapeBackground" + i.ToString(), xcord, 0, 180, 300, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
                OMLabel forecastday = new OMLabel("forecastday" + i.ToString(), shapeBackground.Left + 5, shapeBackground.Top + 5, shapeBackground.Width - 10, 38);
                forecastday.Text = searchResults[screen]["ForecastDay" + i.ToString()].ToString();
                forecastday.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMImage forecastimage = new OMImage("forecastimage" + i.ToString(), shapeBackground.Left + 5, forecastday.Top + forecastday.Height, shapeBackground.Width - 10, 125);
                forecastimage.Image = (imageItem)searchResults[screen]["ForecastImage" + i.ToString()];
                OMLabel forecastdesc = new OMLabel("forecastdesc" + i.ToString(), shapeBackground.Left + 5, forecastimage.Top + forecastimage.Height, shapeBackground.Width - 10, 92);
                forecastdesc.Text = searchResults[screen]["ForecastDescription" + i.ToString()].ToString();
                forecastdesc.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
                forecastdesc.AutoFitTextMode = FitModes.FitFill;
                OMLabel forecasthigh = new OMLabel("forecasthigh" + i.ToString(), shapeBackground.Left + 5, forecastdesc.Top + forecastdesc.Height, shapeBackground.Width - 10, 38);
                forecasthigh.Text = searchResults[screen]["ForecastHigh" + i.ToString()].ToString();
                forecasthigh.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMLabel forecastlow = new OMLabel("forecastlow" + i.ToString(), shapeBackground.Left + 5, forecasthigh.Top + forecasthigh.Height, shapeBackground.Width - 10, 38);
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