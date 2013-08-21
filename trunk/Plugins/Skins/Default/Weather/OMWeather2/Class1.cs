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

            OMPanel omweather = new OMPanel("OMWeather2", "Weather");
            //omweather.Forgotten = true;
            omweather.Entering += new PanelEvent(omweather_entering);


            OMButton current_background = OMButton.PreConfigLayout_BasicStyle("currentbackground", (int)(float)(theHost.ClientArea[0].Width * .15), theHost.ClientArea[0].Top - 6, (int)(float)(theHost.ClientArea[0].Width * .70), (int)(float)(theHost.ClientArea[0].Height * .213) + 6, GraphicCorners.All);
            current_background.BorderColor = Color.White;
            current_background.BorderSize = 1;
            current_background.FocusImage = imageItem.NONE;
            current_background.DownImage = imageItem.NONE;
            OMBasicShape currentbackground = new OMBasicShape("currentbackground", (int)(float)(theHost.ClientArea[0].Width * .15), theHost.ClientArea[0].Top - 6, (int)(float)(theHost.ClientArea[0].Width * .70), (int)(float)(theHost.ClientArea[0].Height * .213) + 6);
            currentbackground.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.Gray, Color.White, 1);
            //currentbackground.FillColor = Color.Gray;
            //currentbackground.BorderColor = Color.White;
            //currentbackground.BorderSize = 1;
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

            //OMButton location_button = OMButton.PreConfigLayout_BasicStyle("locationbutton", 351, 41, 298, 48, GraphicCorners.All);
            ////location_button.Text = "No Location Found";
            //location_button.DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
            //location_button.AutoFitTextMode = FitModes.FitFill;
            //location_button.BorderColor = Color.White;
            //location_button.BorderSize = 1;
            //location_button.OnClick += new userInteraction(locationbutton_onclick);

            OMBasicShape shpDay1Background = new OMBasicShape("shpDay1Background", 10, 150, 180, 380,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            shpDay1Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh1";
            omweather.addControl(shpDay1Background);
            OMBasicShape shpDay2Background = new OMBasicShape("shpDay2Background", shpDay1Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            shpDay2Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh2";
            omweather.addControl(shpDay2Background);
            OMBasicShape shpDay3Background = new OMBasicShape("shpDay4Background", shpDay2Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            shpDay3Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh3";
            omweather.addControl(shpDay3Background);
            OMBasicShape shpDay4Background = new OMBasicShape("shpDay5Background", shpDay3Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            shpDay4Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh4";
            omweather.addControl(shpDay4Background);
            OMBasicShape shpDay5Background = new OMBasicShape("shpDay6Background", shpDay4Background.Region.Right + 20, shpDay1Background.Region.Top, shpDay1Background.Region.Width, shpDay1Background.Region.Height,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5));
            shpDay5Background.Visible_DataSource = "Screen{:S:}.Weather.Visible.ForecastHigh5";
            omweather.addControl(shpDay5Background);

            OMButton location_button = new OMButton("locationbutton", currentbackground.Region.Left, currentbackground.Region.Top + 10, currentbackground.Region.Width, currentbackground.Region.Height - 15);
            //OMButton location_button = new OMButton("locationbutton", 351, 41, 298, 48);
            location_button.DataSource = string.Format("Screen{0}.Weather.Searched.Area", DataSource.DataTag_Screen);
            location_button.TextAlignment = Alignment.TopCenter;
            location_button.AutoFitTextMode = FitModes.Fit;
            location_button.Opacity = 178;
            //location_button.BorderColor = Color.White;
            //location_button.BorderSize = 0;
            location_button.OnClick += new userInteraction(locationbutton_onclick);

            //OMImage currentimage = new OMImage("currentimage", current_background.Left + 1, theHost.ClientArea[0].Top, location_button.Left - current_background.Left, current_background.Height - 5);
            OMImage currentimage = new OMImage("currentimage", currentbackground.Region.Left + 40, currentbackground.Region.Top - 5, 120, 120); //currentbackground.Region.Height - 15);
            currentimage.DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);

            //OMLabel currenttemp = new OMLabel("currenttemp", location_button.Left + location_button.Width, theHost.ClientArea[0].Top, 198, 98);
            OMLabel currenttemp = new OMLabel("currenttemp", currentbackground.Region.Right - 200, currentbackground.Region.Top + 10, 200, currentbackground.Region.Height - 15);
            currenttemp.TextAlignment = Alignment.CenterCenter;
            currenttemp.AutoFitTextMode = FitModes.FitSingleLine;
            currenttemp.FontSize = 40;
            currenttemp.DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);

            //OMLabel currentdesc = new OMLabel("currentdesc", 351, 91, 289, 48);
            OMLabel currentdesc = new OMLabel("currentdesc", currentbackground.Region.Left, currentbackground.Region.Top, currentbackground.Region.Width, currentbackground.Region.Height - 7);
            currentdesc.TextAlignment = Alignment.WordWrap | Alignment.BottomCenter;
            currentdesc.AutoFitTextMode = FitModes.FitSingleLine;
            currentdesc.FontSize = 32;
            currentdesc.DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);
            //currentdesc.Text = "Cloudy";

            OMButton locationbutton = new OMButton("locationbutton", 351, 41, 298, 48);
            locationbutton.Transition = eButtonTransition.None;
            locationbutton.Text = "No Location Found";
            locationbutton.BorderColor = Color.White;
            locationbutton.BorderSize = 1;
            locationbutton.Opacity = 178;
            locationbutton.AutoFitTextMode = FitModes.FitFillSingleLine;
            //locationbutton.FillColor = Color.Black;
            //locationbutton.Color = Color.White;
            locationbutton.OnClick += new userInteraction(locationbutton_onclick);

            omweather.addControl(currentposition);
            omweather.addControl(searchedposition);
            omweather.addControl(current_background);
            //omweather.addControl(currentbackground);
            //omweather.addControl(locationbackground);
            omweather.addControl(currentimage);
            omweather.addControl(currentdesc);
            //omweather.addControl(currenttemp);
            omweather.addControl(location_button);
            //omweather.addControl(locationbutton);
            omweather.addControl(currenttemp);

            OMPanel locations = new OMPanel("locations", "Weather location");
            locations.Forgotten = true;

            OMLabel searching = new OMLabel("searching", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            searching.BackgroundColor = Color.FromArgb(128, Color.Black);
            searching.TextAlignment = Alignment.CenterCenter;
            searching.Text = "Searching, Please Wait...";
            searching.Visible = false;

            //locations.addControl(locationslabel);
            //locations.addControl(locationstextbox);
            //locations.addControl(search);
            //locations.addControl(locationslist);
            //locations.addControl(searching);
            omweather.addControl(searching);

            OMPanel forecast = new OMPanel("forecast");
            forecast.Forgotten = true;
            //int smallheight = (int)(float)(theHost.ClientArea[0].Height*.088);


            OMPanel hourly = new OMPanel("hourly");
            hourly.Forgotten = true;

            //currentpanels
            OMPanel currentpositionforecast = new OMPanel("currentpositionforecast");

            OMPanel currentpositionhourly = new OMPanel("currentpositionhourly");


            for (int i = 0; i < theHost.ScreenCount; i++)
            {
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

                for (int k = 0; k < 5; k++)
                {
                    OMLabel forecastday = new OMLabel("forecastday" + (k + 1).ToString(), 1 + (200 * k), 155, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .082));
                    forecastday.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.ForecastDay" + (k + 1).ToString();
                    forecastday.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    forecastday.Opacity = 178;
                    //forecastday.Font = new Font(forecastday.Font.Name, 17);
                    forecastday.AutoFitTextMode = FitModes.FitFillSingleLine;
                    OMImage forecastimage = new OMImage("forecastimage" + (k + 1).ToString(), 1 + (200 * k), 186, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .307));
                    forecastimage.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.ForecastImage" + (k + 1).ToString();
                    forecastimage.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    OMLabel forecastdesc = new OMLabel("forecastdesc" + (k + 1).ToString(), 1 + (200 * k), 339, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .211));
                    forecastdesc.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
                    forecastdesc.AutoFitTextMode = FitModes.Fit;
                    forecastdesc.FontSize = 20;
                    forecastdesc.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.ForecastDescription" + (k + 1).ToString();
                    forecastdesc.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    OMLabel forecasthigh = new OMLabel("forecasthigh" + (k + 1).ToString(), 1 + (200 * k), 445, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .080));
                    forecasthigh.AutoFitTextMode = FitModes.FitFillSingleLine;
                    forecasthigh.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.ForecastHigh" + (k + 1).ToString();
                    forecasthigh.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    forecasthigh.Opacity = 178;
                    //forecasthigh.Font = new Font(forecasthigh.Font.Name, 18);
                    OMLabel forecastlow = new OMLabel("forecastlow" + (k + 1).ToString(), 1 + (200 * k), 490, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .080));
                    forecastlow.AutoFitTextMode = FitModes.FitFillSingleLine;
                    forecastlow.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.ForecastLow" + (k + 1).ToString();
                    forecastlow.Opacity = 178;
                    forecastlow.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
                    //forecastlow.Font = new Font(forecastlow.Font.Name, 18);
                    omweather.addControl(forecastday);
                    omweather.addControl(forecastimage);
                    omweather.addControl(forecastdesc);
                    omweather.addControl(forecasthigh);
                    omweather.addControl(forecastlow);
                }

                OMBasicShape searchProgressBackground = new OMBasicShape("searchProgressBackground", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
                searchProgressBackground.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.SearchProgressBackground";
                OMImage searchProgressImage = new OMImage("searchProgressImage", 250, 200, 100, 80);
                searchProgressImage.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.SearchProgressImage";
                searchProgressImage.Image = theHost.getSkinImage("BusyAnimationTransparent.gif");
                
                //searchProgressImage.Animate = 
                OMLabel searchProgressLabel = new OMLabel("searchProgressLabel", 350, 200, 400, 80);
                searchProgressLabel.Text = "Please wait, refreshing data...";
                searchProgressLabel.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.SearchProgressLabel";

                OMLabel locationslabel = new OMLabel("locationslabel", theHost.ClientArea[0].Left, currentbackground.Top + currentbackground.Height + 1, theHost.ClientArea[0].Width, (int)(float)(theHost.ClientArea[0].Height * .153));
                locationslabel.Text = "Please enter zip code or city and state...";
                locationslabel.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.LocationLabel";
                //OMTextBox locationstextbox = new OMTextBox("locationstextbox", 250, 276, 249, 75);
                OMTextBox locationstextbox = new OMTextBox("locationstextbox", (int)(float)(theHost.ClientArea[0].Width / 5), locationslabel.Top + locationslabel.Height + 1, ((int)(float)(theHost.ClientArea[0].Width / 2) - 1) - (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .153));
                locationstextbox.OSKType = OSKInputTypes.Keypad;
                locationstextbox.OnOSKShow += new userInteraction(search_onclick);
                locationstextbox.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.LocationTextBox";
                //locationstextbox.OnClick += new userInteraction(locationstextbox_onclick);
                //OMButton search = new OMButton("search", 501, 276, 249, 75);
                OMButton search = OMButton.PreConfigLayout_BasicStyle("search", ((int)(float)(theHost.ClientArea[0].Width / 2) + 1), locationstextbox.Top, locationstextbox.Width, locationstextbox.Height, GraphicCorners.All);
                search.Text = "Search";
                search.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.LocationSearchButton";
                search.OnClick += new userInteraction(search_onclick);
                //OMList locationslist = new OMList("locationslist", 250, 276, 499, 150);
                OMList locationslist = new OMList("locationslist", locationstextbox.Left, locationstextbox.Top, locationstextbox.Width + 2 + search.Width, (int)(float)(theHost.ClientArea[0].Height * .645));
                locationslist.ListItemHeight = 32;
                locationslist.Visible_DataSource = "Screen" + i.ToString() + ".Weather.Visible.LocationMultipleAreaList";
                locationslist.OnClick += new userInteraction(locationslist_onclick);
                omweather.addControl(locationslabel);
                omweather.addControl(locationstextbox);
                omweather.addControl(search);
                //omweather.addControl(locationslist);
                omweather.addControl(searchProgressBackground);
                omweather.addControl(searchProgressImage);
                omweather.addControl(searchProgressLabel);
            }

            manager.loadPanel(omweather, true);
            //manager.loadSinglePanel(omweather, i, true);
            //manager.loadPanel(locations);
            //manager.loadPanel(forecast);
            //}

            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);

            return eLoadStatus.LoadSuccessful;

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
            }

        }

        private void omweather_entering(OMPanel omweather, int screen)
        {

            if (!alreadyaddedstrip[screen])
            {
                PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupcurrentposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Current Position", false, currentgps_onclick, null, null));
                if (lastSearched[screen] != "")
                {
                    if (!favoritesList[screen].Contains(lastSearched[screen]))
                        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), lastSearched[screen] + "\n(Hold To Add As Favorite)", false, currentsearched_onclick, currentsearched_onholdclick, null));
                    else
                        PopUpMenuStrip[screen].Buttons.Add(Button.CreateMenuItem("popupsearchedposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), lastSearched[screen], false, currentsearched_onclick, null, null));
                    //asynch the request for setting DataSources and running the search for the weather...

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
            if (StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched") != "")
            {
                lastSearched[screen] = StoredData.Get(this, "Favorites." + screen.ToString() + ".Searched");
                SafeThread.Asynchronous(delegate { asynchedLastSearch(screen); }, theHost);
            }
            else
                lastSearched[screen] = "";
            return;
        }

        private void asynchedLastSearch(int screen)
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
            object value;
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Locations", new object[] { false, screen }, out value);
            theHost.DataHandler.GetDataSourceValue("Weather.Visible.Forecast", new object[] { true, screen }, out value);
            //update it here
            theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { lastSearched[screen], screen.ToString() }, out value);
        }

        private void currentgps_onclick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

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
            theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.CurrentWeather", new object[] { }, out value);
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

            object value;
            //make sure controls are visible

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

            OMTextBox locationtextbox = (OMTextBox)manager[screen, "OMWeather2"]["locationstextbox"];
            //OMLabel searching = (OMLabel)manager[screen, "locations"]["searching"];
            if (locationtextbox.Text == "")
                return;
            else
            {
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