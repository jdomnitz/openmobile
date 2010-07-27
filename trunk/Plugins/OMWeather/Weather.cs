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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using System;
using System.Reflection;

[assembly: AssemblyTitle("OMWeather")]
[assembly: AssemblyDescription("Weather plugin for openMobile")]
[assembly: AssemblyCompany("Justin Domnitz")]
[assembly: AssemblyProduct("OMWeather")]
[assembly: AssemblyCopyright("GPLv3")]
[assembly: AssemblyVersion("0.2")]
namespace ControlDemo
{
    public class AutoGeneratedClass : IHighLevel
    {
        OMPanel p;
        OMPanel widget;
        IPluginHost theHost;

        public eLoadStatus initialize(OpenMobile.Plugin.IPluginHost host)
        {
            p = new OMPanel();
            widget = new OMPanel();
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            imageItem opt1 = theHost.getSkinImage("Weather|NotSet");
            Font f = new Font(Font.GenericSansSerif, 36F);
            OMLabel Title = new OMLabel();
            Title.Font = new Font(Font.Impact, 36F);
            Title.Width = 1000;
            Title.Top = 83;
            Title.Format = eTextFormat.DropShadow;
            Title.TextAlignment = Alignment.CenterRight;
            Title.Name = "Title";
            OMLabel day1high = new OMLabel(235, 213, 310, 50);
            day1high.Color = Color.Red;
            day1high.OutlineColor = Color.White;
            day1high.Font = f;
            day1high.Format = eTextFormat.Glow;
            day1high.Name = "day1high";
            day1high.TextAlignment = Alignment.CenterLeft;
            OMLabel day1low = new OMLabel(247, 267, 310, 50);
            day1low.Color = Color.Blue;
            day1low.OutlineColor = Color.White;
            day1low.Font = f;
            day1low.Format = eTextFormat.Glow;
            day1low.Name = "day1low";
            day1low.TextAlignment = Alignment.CenterLeft;
            OMImage day1img = new OMImage(-2, 100, 250, 250);
            day1img.Image = opt1;
            day1img.Name = "day1img";
            OMLabel now = new OMLabel(240,133,310,100);
            now.Color = Color.SlateGray;
            now.OutlineColor = Color.White;
            now.Font = f;
            now.Format = eTextFormat.Glow;
            now.Name = "now";
            now.TextAlignment = Alignment.CenterLeft;
            OMLabel humidity = new OMLabel(545,184,550,100);
            humidity.Color = Color.DarkGoldenrod;
            humidity.OutlineColor = Color.White;
            humidity.Font = f;
            humidity.Format = eTextFormat.Glow;
            humidity.Mode = eModeType.Highlighted;
            humidity.Name = "humidity";
            humidity.TextAlignment = Alignment.CenterLeft;
            OMLabel wind = new OMLabel(625, 261, 420, 60);
            wind.Color = Color.DarkOrange;
            wind.OutlineColor = Color.White;
            wind.Font = f;
            wind.Format = eTextFormat.Glow;
            wind.Name = "wind";
            wind.TextAlignment = Alignment.CenterLeft;
            OMLabel feelsLike = new OMLabel(515, 158, 550, 50);
            feelsLike.Color = Color.MediumSlateBlue;
            feelsLike.OutlineColor = Color.White;
            feelsLike.Font = f;
            feelsLike.Format = eTextFormat.Glow;
            feelsLike.Name = "feelsLike";
            feelsLike.TextAlignment = Alignment.CenterLeft;
            OMLabel day2 = new OMLabel();
            day2.Top = 285;
            day2.Left = 77;
            day2.Text = "Tomorrow";
            day2.Name = "day2";
            OMLabel day3 = new OMLabel();
            day3.Top = 285;
            day3.Left = 295;
            day3.Width = 150;
            day3.Text = DateTime.Today.AddDays(2).DayOfWeek.ToString();
            day3.Mode = eModeType.Resizing;
            day3.Name = "day3";
            OMLabel day4 = new OMLabel();
            day4.Top = 285;
            day4.Left = 515;
            day4.Width = 150;
            day4.Text = DateTime.Today.AddDays(3).DayOfWeek.ToString();
            day4.Name = "day4";
            OMLabel day5 = new OMLabel();
            day5.Top = 285;
            day5.Left = 769;
            day5.Text = DateTime.Today.AddDays(4).DayOfWeek.ToString();
            day5.Name = "day5";
            day5.Width = 150;
            OMImage day2img = new OMImage();
            day2img.Image = opt1;
            day2img.Top = 350;
            day2img.Left = 93;
            day2img.Name = "day2img";
            OMImage day3img = new OMImage();
            day3img.Image = opt1;
            day3img.Top = 350;
            day3img.Left = 309;
            day3img.Name = "day3img";
            OMImage day5img = new OMImage();
            day5img.Image = opt1;
            day5img.Top = 350;
            day5img.Left = 787;
            day5img.Name = "day5img";
            OMImage day4img = new OMImage();
            day4img.Image = opt1;
            day4img.Top = 350;
            day4img.Left = 531;
            day4img.Name = "day4img";
            Font f2 = new Font(Font.GenericSansSerif, 24F);
            OMLabel day2high = new OMLabel(72, 450, 200, 50);
            day2high.Color = Color.Red;
            day2high.OutlineColor = Color.White;
            day2high.Font = f2;
            day2high.Text = "High: N/A";
            day2high.Format = eTextFormat.Glow;
            day2high.TextAlignment = Alignment.CenterLeft;
            day2high.Name = "day2high";
            Font f3 = new Font(Font.GenericSansSerif, 26.25F);
            OMLabel day2low = new OMLabel(72, 490, 200, 50);
            day2low.Color = Color.Blue;
            day2low.OutlineColor = Color.White;
            day2low.Font = f3;
            day2low.Text = "Low: N/A";
            day2low.Format = eTextFormat.Glow;
            day2low.TextAlignment = Alignment.CenterLeft;
            day2low.Name = "day2low";
            OMLabel day3high = new OMLabel(297, 450, 200, 50);
            day3high.Color = Color.Red;
            day3high.OutlineColor = Color.White;
            day3high.Font = f2;
            day3high.Text = "High: N/A";
            day3high.Format = eTextFormat.Glow;
            day3high.TextAlignment = Alignment.CenterLeft;
            day3high.Name = "day3high";
            OMLabel day4high = new OMLabel(520, 450, 200, 50);
            day4high.Color = Color.Red;
            day4high.OutlineColor = Color.White;
            day4high.Font = f2;
            day4high.Text = "High: N/A";
            day4high.Format = eTextFormat.Glow;
            day4high.TextAlignment = Alignment.CenterLeft;
            day4high.Name = "day4high";
            OMLabel day5high = new OMLabel(775, 450, 200, 50);
            day5high.Color = Color.Red;
            day5high.OutlineColor = Color.White;
            day5high.Font = f2;
            day5high.Text = "High: N/A";
            day5high.Format = eTextFormat.Glow;
            day5high.TextAlignment = Alignment.CenterLeft;
            day5high.Name = "day5high";
            OMLabel day4low = new OMLabel(521, 490, 200, 50);
            day4low.Color = Color.Blue;
            day4low.OutlineColor = Color.White;
            day4low.Font = f3;
            day4low.Text = "Low: N/A";
            day4low.Format = eTextFormat.Glow;
            day4low.TextAlignment = Alignment.CenterLeft;
            day4low.Name = "day4low";
            OMLabel day3low = new OMLabel(299, 490, 200, 50);
            day3low.Color = Color.Blue;
            day3low.OutlineColor = Color.White;
            day3low.Font = f3;
            day3low.Text = "Low: N/A";
            day3low.Format = eTextFormat.Glow;
            day3low.TextAlignment = Alignment.CenterLeft;
            day3low.Name = "day3low";
            OMLabel day5low = new OMLabel(775,490,200,50);
            day5low.Color = Color.Blue;
            day5low.OutlineColor = Color.White;
            day5low.Font = new Font(Font.GenericSansSerif, 26.25F);
            day5low.Text = "Low: N/A";
            day5low.Format = eTextFormat.Glow;
            day5low.TextAlignment = Alignment.CenterLeft;
            day5low.Name = "day5low";
            OMLabel provider = new OMLabel(300, 560, 120, 30);
            provider.Text = "Provided by:";
            provider.Font = new Font(Font.GenericSansSerif, 12F);
            OMImage attrib = new OMImage(420,550, 120, 50);
            p.addControl(Title);
            p.addControl(day1high);
            p.addControl(day1low);
            p.addControl(day1img);
            p.addControl(now);
            p.addControl(humidity);
            p.addControl(wind);
            p.addControl(feelsLike);
            p.addControl(day2);
            p.addControl(day3);
            p.addControl(day4);//10
            p.addControl(day5);
            p.addControl(day2img);
            p.addControl(day3img);
            p.addControl(day4img);
            p.addControl(day5img);
            p.addControl(day2high);
            p.addControl(day2low);
            p.addControl(day3high);
            p.addControl(day4high);
            p.addControl(day5high);//20
            p.addControl(day3low);
            p.addControl(day4low);
            p.addControl(day5low);
            p.addControl(provider);
            p.addControl(attrib);
            OMImage cache=new OMImage(0,0,1000,600);
            widget.addControl(cache);
            OpenMobile.Threading.TaskManager.QueueTask(Weather.PurgeOld, ePriority.Low,"Delete Old Weather");
            return eLoadStatus.LoadSuccessful;
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function==eFunction.TransitionToPanel)
                if (arg2 == "OMWeather")
                {
                    refresh();
                    ((OMImage)widget[0]).Image = new imageItem(Widget.generate("OMWeather", theHost));
                }
        }
        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }
        public void refresh()
        {
            string loc;
            using (PluginSettings settings = new PluginSettings())
                loc = settings.getSetting("Data.DefaultLocation");
            if (loc == "")
                return;
            OpenMobile.Data.Weather provider = new OpenMobile.Data.Weather();
            OpenMobile.Data.Weather.weather data;
            try
            {
                data = provider.readWeather(loc, DateTime.Today);
            }
            catch (ArgumentException) { return; }
            ((OMLabel)p[0]).Text = "Currently in " + data.location;
            ((OMLabel)p[1]).Text = "High: "+Globalization.convertToLocalTemp(data.highTemp,true);
            ((OMLabel)p[2]).Text = "Low: " + Globalization.convertToLocalTemp(data.lowTemp, true);
            ((OMImage)p[3]).Image = theHost.getSkinImage("Weather|"+data.conditions.ToString());
            ((OMLabel)p[4]).Text = "Now: " + Globalization.convertToLocalTemp(data.temp, true);
            ((OMLabel)p[5]).Text = "Humidity: "+data.humidity + '%';
            ((OMLabel)p[6]).Text = "Wind: "+Globalization.convertSpeedToLocal(data.windSpeed,true) + ' '+data.windDirection;
            ((OMLabel)p[7]).Text = "Feels Like: " + Globalization.convertToLocalTemp(data.feelsLike, true);
            ((OMImage)p[25]).Image = theHost.getSkinImage("Providers|" + data.contrib, true);
            try
            {
                data = provider.readWeather(loc, DateTime.Today.AddDays(1)); //Tomorrow
                ((OMLabel)p[16]).Text = "High: " + Globalization.convertToLocalTemp(data.highTemp, true);
                ((OMLabel)p[17]).Text = "Low: " + Globalization.convertToLocalTemp(data.lowTemp, true);
                ((OMImage)p[12]).Image = theHost.getSkinImage("Weather|" + data.conditions.ToString());
            }
            catch (Exception) { return; }
            try
            {
                data = provider.readWeather(loc, DateTime.Today.AddDays(2));
                ((OMLabel)p[18]).Text = "High: " + Globalization.convertToLocalTemp(data.highTemp, true);
                ((OMLabel)p[21]).Text = "Low: " + Globalization.convertToLocalTemp(data.lowTemp, true);
                ((OMImage)p[13]).Image = theHost.getSkinImage("Weather|" + data.conditions.ToString());
            }
            catch (Exception) { return; }
            try
            {
                data = provider.readWeather(loc, DateTime.Today.AddDays(3));
                ((OMLabel)p[19]).Text = "High: " + Globalization.convertToLocalTemp(data.highTemp, true);
                ((OMLabel)p[22]).Text = "Low: " + Globalization.convertToLocalTemp(data.lowTemp, true);
                ((OMImage)p[14]).Image = theHost.getSkinImage("Weather|" + data.conditions.ToString());
            }
            catch (Exception) { return; }
            try
            {
                data = provider.readWeather(loc, DateTime.Today.AddDays(4));
                ((OMLabel)p[20]).Text = "High: " + Globalization.convertToLocalTemp(data.highTemp, true);
                ((OMLabel)p[23]).Text = "Low: " + Globalization.convertToLocalTemp(data.lowTemp, true);
                ((OMImage)p[15]).Image = theHost.getSkinImage("Weather|" + data.conditions.ToString());
            }
            catch (Exception) { return; }
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return p;
        }

        public OMPanel loadSettings(string name, int screen)
        {
            throw new NotImplementedException();
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "OMWeather"; }
        }
        public string displayName
        {
            get { return "Weather"; }
        }
        public float pluginVersion
        {
            get { return 1.0F; }
        }

        public string pluginDescription
        {
            get { return "Weather plugin for openMobile"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            p = null;
            widget = null;
        }
    }
}