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
using System.Threading;
using System.Windows.Forms;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Plugin;

namespace BatterySupport
{
    public sealed class BatterySupport:IHighLevel
    {
        Thread batteryWatcher;
        Settings batSettings;
        String OnBatteryAction = "Nothing";
        public Settings loadSettings()
        {
            if (batSettings == null)
            {
                batSettings = new Settings("Battery");
                System.Collections.Generic.List<String> Options = new System.Collections.Generic.List<String> { "Nothing", "Suspend", "Hibernate", "Shutdown"};
                batSettings.Add(new Setting(SettingTypes.MultiChoice, "BatterySupport.OnBatteryAction", "Action", "Action On Power Unplugged", Options, Options, OnBatteryAction));
                batSettings.OnSettingChanged += new SettingChanged(Changed);
            }
            return batSettings;
        }

        void Changed(int screen,Setting setting)
        {
            using (PluginSettings s = new PluginSettings())
                s.setSetting(setting.Name, setting.Value);
            OnBatteryAction = setting.Value;
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
            get { return "BatterySupport"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Battery Support"; }
        }
        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);

            using (PluginSettings s = new PluginSettings())
                OnBatteryAction = s.getSetting("BatterySupport.OnBatteryAction");
            if (string.IsNullOrEmpty(OnBatteryAction))
                OnBatteryAction="Nothing";
            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
            {
                theHost_OnPowerChange(ePowerEvent.SystemOnBattery);
            }
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        bool previouslyPluggedIn = true;
        imageItem batteryImage=new imageItem();
        void theHost_OnPowerChange(ePowerEvent type)
        {
            lock (this)
            {
                if (type == ePowerEvent.SystemOnBattery)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    batteryImage = theHost.getSkinImage("BatteryCharged", true);
                    batteryImage.image.persist = true;
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(batteryImage.image.image);
                    {
                        OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
                        gr.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(Font.GenericSansSerif, 36), eTextFormat.Glow, Alignment.CenterCenter, Color.White, Color.Black);
                    }
                    g.Dispose();
                    AddIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    startWatching();
                }
                else if (type == ePowerEvent.BatteryLow)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    batteryImage = theHost.getSkinImage("BatteryWarning", true);
                    batteryImage.image.persist = true;
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(batteryImage.image.image);
                    OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
                    gr.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(Font.GenericSansSerif, 36), eTextFormat.Glow, Alignment.CenterCenter, Color.White, Color.Black);
                    g.Dispose();
                    AddIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    startWatching();
                }
                else if (type == ePowerEvent.BatteryCritical)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    batteryImage = theHost.getSkinImage("BatteryCritical", true);
                    batteryImage.image.persist = true;
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(batteryImage.image.image);
                    OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
                    gr.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(Font.GenericSansSerif, 36), eTextFormat.Glow, Alignment.CenterCenter, Color.White, Color.Black);
                    g.Dispose();
                    AddIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    startWatching();
                }
                else if (type == ePowerEvent.SystemPluggedIn)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    if (batteryWatcher != null)
                    {
                        batteryWatcher.Abort();
                        batteryWatcher = null;
                    }
                }
            }
            // Added Action on Power Unplugged
            if (OnBatteryAction != "Nothing")
            {
                if (type == ePowerEvent.SystemPluggedIn || type == ePowerEvent.SystemResumed)
                {
                    previouslyPluggedIn = true;
                }
                else if(previouslyPluggedIn == true && type != ePowerEvent.SleepOrHibernatePending)
                {
                    previouslyPluggedIn = false;

                    //Added to let the laptop survive cranking
                    System.Threading.Thread.Sleep(3000);
                    if (!previouslyPluggedIn )
                    {
                        if (OnBatteryAction == "Hibernate")
                           theHost.execute(eFunction.hibernate);
                        else if (OnBatteryAction == "Suspend")
                            theHost.execute(eFunction.standby);
                        else if (OnBatteryAction == "Shutdown")
                            theHost.execute(eFunction.shutdown);
                    }
                }
            }

        }
        private void startWatching()
        {
            batteryWatcher = new Thread(delegate()
                {
                    try
                    {
                        updateBattery();
                    }
                    catch (Exception) { }
                });
            batteryWatcher.Start();
        }
        int lastBatt;
        private void updateBattery()
        {
            while (true)
            {
                Thread.Sleep(60000);
                if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Charging)
                    return;
                int newbatt = (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
                if (newbatt != lastBatt)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                    if (batteryImage.name == "BatteryCharged")
                        batteryImage = theHost.getSkinImage("BatteryCharged", true);
                    else if (batteryImage.name == "BatteryWarning")
                        batteryImage = theHost.getSkinImage("BatteryWarning", true);
                    else if (batteryImage.name == "BatteryCritical")
                        batteryImage = theHost.getSkinImage("BatteryCritical", true);
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(batteryImage.image.image);
                        OpenMobile.Graphics.Graphics gr = new OpenMobile.Graphics.Graphics(0);
                        gr.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(Font.GenericSansSerif, 36), eTextFormat.Glow, Alignment.CenterCenter, Color.White, Color.Black);
                    g.Dispose();
                    AddIcon(new IconManager.UIIcon(batteryImage, ePriority.MediumLow, false));
                }
                lastBatt = newbatt;
            }
        }
        private void RemoveIcon(IconManager.UIIcon uIIcon)
        {
            theHost.sendMessage("UI", "BatterySupport", "RemoveIcon", ref uIIcon);
        }
        private void AddIcon(IconManager.UIIcon uIIcon)
        {
            theHost.sendMessage("UI", "BatterySupport", "AddIcon", ref uIIcon);
        }

        public void Dispose()
        {
            //
        }

        public OMPanel loadPanel(string name, int screen)
        {
 	       return null;
        }

        public string  displayName
        {
	        get { return "Battery Info"; }
        }
    }
}
