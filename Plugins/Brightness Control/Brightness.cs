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
using System.Threading;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Plugin;
using OpenMobile.Threading;

namespace Brightness_Control
{
    public class Brightness:IOther
    {
        private Settings settings;
        private IPluginHost theHost;

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnSystemEvent += new SystemEvent(theHost_OnSystemEvent);
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            TaskManager.QueueTask(loadBrightness, ePriority.Low, "Calculate Screen Brightness");
            TaskManager.QueueTask(updateBrightness, ePriority.Low, "Calculate Sunset");
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        private void loadBrightness()
        {
            for (int i = 0; i < theHost.ScreenCount; i++)
                getBrightness(i);
        }
        void theHost_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
                updateBrightness();
        }

        void theHost_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.hourChanged)
                updateBrightness();
        }
        DateTime sunset;
        DateTime sunrise;
        private void updateBrightness()
        {
            if ((settings == null) || (settings.Count < 3))
                return;
            for (int i = 0; i < theHost.ScreenCount; i++)
                if (settings[i].getInstanceValue(i) == "True")
                {
                    if ((sunset == DateTime.MinValue)||(sunrise<DateTime.Now)||(sunset.Date<DateTime.Now.Date))
                        updateSunset();
                    if (sunset == DateTime.MinValue)
                        return;
                    TimeSpan rise = sunrise-DateTime.Now;
                    TimeSpan set = sunset-DateTime.Now;
                    if ((DateTime.Now < sunset) && (DateTime.Now.Date < sunrise.Date))
                    {
                        if (rise.TotalHours >= 23)
                            rampBrightness(i, 50);
                        else if(rise.TotalHours==22)
                            rampBrightness(i, 100);
                        else
                            setBrightness(i, 100);
                    }
                    else
                    {
                        if (set.TotalHours == 0)
                            rampBrightness(i, 50);
                        else if(set.TotalHours==-1)
                            rampBrightness(i, 1);
                        else
                            setBrightness(i, 1);
                    }
                }
        }
        private void getBrightness(int screen)
        {
            object o;
            theHost.getData(eGetData.GetScreenBrightness, "", screen.ToString(), out o);
            if (o!=null)
                if ((settings != null) && (settings.Count >= 3))
                {
                    settings[2].setInstanceValue(screen, ((int)o).ToString());
                    settings.changeSetting(screen,settings[2]);
                }
        }
        private void setBrightness(int screen, int value)
        {
            if ((settings != null) && (settings.Count >= 3))
            {
                settings[2].setInstanceValue(screen, value.ToString());
                settings.changeSetting(screen, settings[2]);
            }
            else
                theHost.execute(eFunction.setMonitorBrightness, screen.ToString(), value.ToString());
        }
        private void rampBrightness(int screen, int end)
        {
            int start=0;
            if ((settings != null) && (settings.Count >= 3))
                int.TryParse(settings[2].Value, out start);
            for (int i = start; i < end; i++)
            {
                setBrightness(screen, i);
                Thread.Sleep(100);
            }
        }
        private void updateSunset()
        {
            if (!OpenMobile.Net.Network.IsAvailable)
                return;
            string loc;
            using (PluginSettings s = new PluginSettings())
                loc = s.getSetting("Data.DefaultLocation");
            if (string.IsNullOrEmpty(loc))
                return;
            Location current = OpenMobile.helperFunctions.General.lookupLocation(loc);
            if (current.Latitude == 0)
                return;
            sunset = OpenMobile.Framework.Math.Calculation.getSunset(current.Longitude, current.Latitude);
            sunrise = OpenMobile.Framework.Math.Calculation.getSunrise(current.Longitude, current.Latitude);
        }
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Brightness Control");
                using (PluginSettings s = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "BrightnessControl.Sun", "", "Automatically dim at sunset", Setting.BooleanList, Setting.BooleanList, s.getAllInstances("BrightnessControl.Sun",theHost.ScreenCount)));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "BrightnessControl.Ambient", "", "Automatically dim with ambient lighting", Setting.BooleanList, Setting.BooleanList, s.getAllInstances("BrightnessControl.Ambient",theHost.ScreenCount)));
                    List<string> range=new List<string>();
                    range.Add("1");
                    range.Add("100");
                    settings.Add(new Setting(SettingTypes.Range, "BrightnessControl.Level", "Level", "Current Brightness %value%%",null , range, s.getAllInstances("BrightnessControl.Level",theHost.ScreenCount)));
                    settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
                }
            }
            return settings;
        }

        void settings_OnSettingChanged(int screen,Setting setting)
        {
            if (setting.Name == "BrightnessControl.Level")
            {
                if (theHost != null)
                    theHost.execute(eFunction.setMonitorBrightness, screen.ToString(), setting.getInstanceValue(screen));
                return;
            }
            using (PluginSettings s = new PluginSettings())
                s.setAllInstances(setting.Name, setting.CurrentValue);
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return string.Empty; }
        }

        public string pluginName
        {
            get { return "BrightnessControl"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "BrightnessControl"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public void Dispose()
        {
            if (settings != null)
            {
                using (PluginSettings s = new PluginSettings())
                    s.setAllInstances("BrightnessControl.Level", settings[2].CurrentValue);
            }
        }
    }
}
