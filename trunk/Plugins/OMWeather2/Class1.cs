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
        ButtonStrip PopUpMenuStrip;

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

            OMPanel omweather = new OMPanel("OMWeather2");
            omweather.Entering += new PanelEvent(omweather_entering);
            PopUpMenuStrip = new ButtonStrip(this.pluginName, omweather.Name, "PopUpMenuStrip");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("popupcurrentposition", theHost.UIHandler.PopUpMenu.ButtonSize, 255, imageItem.NONE, "Current Position (GPS)", false, null, null, null));

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

            OMLabel currentdesc = new OMLabel("currentdesc", 351, 91, 289, 48);
            currentdesc.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
            currentdesc.AutoFitTextMode = FitModes.FitFillSingleLine;
            currentdesc.DataSource = string.Format("Screen{0}.Weather.Searched.Description", DataSource.DataTag_Screen);

            OMButton location_button = OMButton.PreConfigLayout_BasicStyle("locationbutton", 351, 41, 298, 48, GraphicCorners.All);
            location_button.Text = "No Location Found";
            location_button.AutoFitTextMode = FitModes.FitFillSingleLine;
            location_button.BorderColor = Color.White;
            location_button.BorderSize = 1;
            location_button.OnClick += new userInteraction(locationbutton_onclick);

            OMImage currentimage = new OMImage("currentimage", current_background.Left + 1, theHost.ClientArea[0].Top, location_button.Left - current_background.Left, current_background.Height - 5);
            currentimage.DataSource = string.Format("Screen{0}.Weather.Searched.Image", DataSource.DataTag_Screen);

            OMButton locationbutton = new OMButton("locationbutton", 351, 41, 298, 48);
            locationbutton.Transition = eButtonTransition.None;
            locationbutton.Text = "No Location Found";
            locationbutton.BorderColor = Color.White;
            locationbutton.BorderSize = 1;
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

            OMPanel locations = new OMPanel("locations");
            locations.Forgotten = true;
            OMLabel locationslabel = new OMLabel("locationslabel", theHost.ClientArea[0].Left, currentbackground.Top + currentbackground.Height + 1, theHost.ClientArea[0].Width, (int)(float)(theHost.ClientArea[0].Height * .153));
            locationslabel.Text = "Please enter zip code or city and state...";
            //OMTextBox locationstextbox = new OMTextBox("locationstextbox", 250, 276, 249, 75);
            OMTextBox locationstextbox = new OMTextBox("locationstextbox", (int)(float)(theHost.ClientArea[0].Width / 5), locationslabel.Top + locationslabel.Height + 1, ((int)(float)(theHost.ClientArea[0].Width / 2) - 1) - (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .153));
            locationstextbox.OSKType = OSKInputTypes.Keypad;
            //locationstextbox.OnClick += new userInteraction(locationstextbox_onclick);
            //OMButton search = new OMButton("search", 501, 276, 249, 75);
            OMButton search = new OMButton("search", ((int)(float)(theHost.ClientArea[0].Width / 2) + 1), locationstextbox.Top, locationstextbox.Width, locationstextbox.Height);
            search.Text = "Search";
            search.OnClick += new userInteraction(search_onclick);
            //OMList locationslist = new OMList("locationslist", 250, 276, 499, 150);
            OMList locationslist = new OMList("locationslist", locationstextbox.Left, locationstextbox.Top, locationstextbox.Width + 2 + search.Width, (int)(float)(theHost.ClientArea[0].Height * .645));
            locationslist.ListItemHeight = 32;
            locationslist.Visible = false;
            locationslist.OnClick += new userInteraction(locationslist_onclick);
            OMLabel searching = new OMLabel("searching", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            searching.BackgroundColor = Color.FromArgb(128, Color.Black);
            searching.TextAlignment = Alignment.CenterCenter;
            searching.Text = "Searching, Please Wait...";
            searching.Visible = false;

            locations.addControl(locationslabel);
            locations.addControl(locationstextbox);
            locations.addControl(search);
            locations.addControl(locationslist);
            locations.addControl(searching);

            OMPanel forecast = new OMPanel("forecast");
            forecast.Forgotten = true;
            //int smallheight = (int)(float)(theHost.ClientArea[0].Height*.088);
            for (int k = 0; k < 5; k++)
            {
                OMLabel forecastday = new OMLabel("forecastday" + (k + 1).ToString(), 1 + (200 * k), 145, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .088));
                forecastday.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDay" + (k + 1).ToString(), DataSource.DataTag_Screen);
                //forecastday.Font = new Font(forecastday.Font.Name, 17);
                forecastday.AutoFitTextMode = FitModes.FitFillSingleLine;
                OMImage forecastimage = new OMImage("forecastimage" + (k + 1).ToString(), 1 + (200 * k), 186, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .307));
                forecastimage.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastImage" + (k + 1).ToString(), DataSource.DataTag_Screen);
                OMLabel forecastdesc = new OMLabel("forecastdesc" + (k + 1).ToString(), 1 + (200 * k), 339, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .211));
                forecastdesc.TextAlignment = Alignment.WordWrap | Alignment.CenterCenter;
                forecastdesc.AutoFitTextMode = FitModes.Fit;
                forecastdesc.FontSize = 20;
                forecastdesc.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastDescription" + (k + 1).ToString(), DataSource.DataTag_Screen);
                OMLabel forecasthigh = new OMLabel("forecasthigh" + (k + 1).ToString(), 1 + (200 * k), 445, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .088));
                forecasthigh.AutoFitTextMode = FitModes.FitFillSingleLine;
                forecasthigh.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastHigh" + (k + 1).ToString(), DataSource.DataTag_Screen);
                //forecasthigh.Font = new Font(forecasthigh.Font.Name, 18);
                OMLabel forecastlow = new OMLabel("forecastlow" + (k + 1).ToString(), 1 + (200 * k), 490, (int)(float)(theHost.ClientArea[0].Width / 5), (int)(float)(theHost.ClientArea[0].Height * .088));
                forecastlow.AutoFitTextMode = FitModes.FitFillSingleLine;
                forecastlow.DataSource = string.Format("Screen{0}.Weather.Searched.ForecastLow" + (k + 1).ToString(), DataSource.DataTag_Screen);
                //forecastlow.Font = new Font(forecastlow.Font.Name, 18);
                forecast.addControl(forecastday);
                forecast.addControl(forecastimage);
                forecast.addControl(forecastdesc);
                forecast.addControl(forecasthigh);
                forecast.addControl(forecastlow);
            }

            OMPanel hourly = new OMPanel("hourly");
            hourly.Forgotten = true;

            //currentpanels
            OMPanel currentpositionforecast = new OMPanel("currentpositionforecast");

            OMPanel currentpositionhourly = new OMPanel("currentpositionhourly");


            OMLabel currenttemp = new OMLabel("currenttemp", location_button.Left + location_button.Width, theHost.ClientArea[0].Top, 198, 98);
            currenttemp.TextAlignment = Alignment.CenterCenter;
            currenttemp.AutoFitTextMode = FitModes.FitFillSingleLine;
            currenttemp.DataSource = string.Format("Screen{0}.Weather.Searched.Temp", DataSource.DataTag_Screen);
            omweather.addControl(currenttemp);
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                displayed[i] = "";
                degrees[i] = "";
                //currenttemp.Text = "{Weather.Searched-" + i.ToString() + ".SearchedTemp}";


            }

            manager.loadPanel(omweather, true);
            //manager.loadSinglePanel(omweather, i, true);
            manager.loadPanel(locations);
            manager.loadPanel(forecast);
            //}
            return eLoadStatus.LoadSuccessful;

        }
        private void omweather_entering(OMPanel omweather, int screen)
        {
            theHost.UIHandler.PopUpMenu.SetButtonStrip(screen, PopUpMenuStrip);
            
            if (displayed[screen] == "")
                return;
            //else if (displayed[screen] == "locations")
            //{
            //    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", displayed[screen]);
            //    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            //}
            else
            {
                //check if settings were changed...
                //if (degreessettings[screen] != degrees[screen])
                //    flipdegrees(screen);
                theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", displayed[screen]);
                theHost.execute(eFunction.ExecuteTransition, screen.ToString());
            }

        }

        private void currentposition_onclick(OMControl sender, int screen)
        {
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
            if (displayed[screen] != "")
                theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", displayed[screen]);
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "locations");
            displayed[screen] = "locations";
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void locationstextbox_onclick(OMControl sender, int screen)
        {
            //launches osk
        }

        private void search_onclick(OMControl sender, int screen)
        {
            OMTextBox locationtextbox = (OMTextBox)manager[screen, "locations"]["locationstextbox"];
            OMLabel searching = (OMLabel)manager[screen, "locations"]["searching"];
            if (locationtextbox.Text == "")
                return;
            //else if not connected to internet
            else
            {
                //bring up searching stuff
                searching.Visible = true;
                string searchtext = locationtextbox.Text;
                searchtext.Replace(" ", "+");
                searchtext.Replace(",", "+");
                object value;
                //theHost.DataHandler.GetDataSourceValue("Weather.SearchedPosition.Full", new object[] { searchtext, screen.ToString() }, out value);
                theHost.DataHandler.GetDataSourceValue("Weather.SearchedWeather.SearchWeather", new object[] { searchtext, screen.ToString() }, out value);
                if (value == null)
                {
                    //Dictionary<string, string> wi = (Dictionary<string, string>)value;
                    //if (wi.Keys.ElementAt(0) == "AreaFound")
                    //{
                    displayed[screen] = "forecast";
                    theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMWeather2", "locations");
                    theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMWeather2", "forecast");
                    searching.Visible = false;
                    theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                    //}
                }
                else
                {
                    Dictionary<string, string> wi = (Dictionary<string, string>)value;
                    switch (wi.Keys.ElementAt(0))
                    {
                        case "MultipleResults": //get locationslist ready
                            OMList locationslist = (OMList)manager[screen, "locations"]["locationslist"];
                            locationslist.Clear();
                            locationslist.Add(new OMListItem("Clear Search Results", "Clear"));
                            ((OMLabel)manager[screen, "locations"]["locationslabel"]).Text = "Multiple Locations Found:\nPlease choose from the list below...";
                            //get results from wi (dictionary), key = area
                            for (int i = 1; i < wi.Count; i++)
                            {
                                locationslist.Add(new OMListItem(wi.Keys.ElementAt(i), wi[wi.Keys.ElementAt(i)]));
                            }
                            locationslist.Visible = true;
                            locationslist.Visible = true;
                            searching.Visible = false;
                            break;
                        default: //nothing found
                            ((OMLabel)manager[screen, "locations"]["locationslabel"]).Text = "No Search Results Found:\nPlease enter zip code or city and state...";
                            break;
                    }
                }
            }
        }

        private void locationslist_onclick(OMControl sender, int screen)
        {
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
            get { return "peter.yeaney@gmail.com"; }
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
            //
        }

    } //end class1
} //end namespace