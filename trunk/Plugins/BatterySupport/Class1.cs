﻿/*********************************************************************************
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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Plugin;

namespace BatterySupport
{
    public sealed class BatterySupport:IHighLevel
    {
        Thread batteryWatcher;
        public Settings loadSettings()
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
            throw new NotImplementedException();
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            theHost.OnPowerChange += new PowerEvent(theHost_OnPowerChange);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        
        imageItem batteryImage;
        void theHost_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemOnBattery)
            {
                RemoveIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                batteryImage = theHost.getSkinImage("BatteryCharged", true);
                Graphics g = Graphics.FromImage(batteryImage.image);
                Renderer.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(FontFamily.GenericSansSerif, 36), textFormat.Glow, Alignment.CenterCenter, 0.9F, 0, Color.White, Color.Black);
                g.Dispose();
                AddIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                startWatching();
            }
            else if (type == ePowerEvent.BatteryLow)
            {
                RemoveIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                batteryImage = theHost.getSkinImage("BatteryWarning", true);
                Graphics g = Graphics.FromImage(batteryImage.image);
                Renderer.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(FontFamily.GenericSansSerif, 36), textFormat.Glow, Alignment.CenterCenter, 0.9F, Color.White, Color.Black);
                g.Dispose();
                AddIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                startWatching();
            }
            else if (type == ePowerEvent.BatteryCritical)
            {
                RemoveIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                batteryImage = theHost.getSkinImage("BatteryCritical", true);
                Graphics g = Graphics.FromImage(batteryImage.image);
                Renderer.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(FontFamily.GenericSansSerif, 36), textFormat.Glow, Alignment.CenterCenter, 0.9F, Color.White, Color.Black);
                g.Dispose();
                AddIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                startWatching();
            }
            else if (type == ePowerEvent.SystemPluggedIn)
            {
                RemoveIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                if (batteryWatcher != null)
                {
                    batteryWatcher.Abort();
                    batteryWatcher = null;
                }
            }
        }
        private void startWatching()
        {
            batteryWatcher = new Thread(delegate() { updateBattery(); });
            batteryWatcher.Start();
        }
        int lastBatt;
        private void updateBattery()
        {
            while (true)
            {
                Thread.Sleep(60000);
                int newbatt = (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
                if (newbatt != lastBatt)
                {
                    RemoveIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
                    if (batteryImage.name == "BatteryCharged")
                        batteryImage = theHost.getSkinImage("BatteryCharged", true);
                    else if (batteryImage.name == "BatteryWarning")
                        batteryImage = theHost.getSkinImage("BatteryWarning", true);
                    else if (batteryImage.name == "BatteryCritical")
                        batteryImage = theHost.getSkinImage("BatteryCritical", true);
                    Graphics g = Graphics.FromImage(batteryImage.image);
                    Renderer.renderText(g, 0, 0, batteryImage.image.Width, batteryImage.image.Height, ((int)(SystemInformation.PowerStatus.BatteryLifePercent * 100)).ToString() + "%", new Font(FontFamily.GenericSansSerif, 36), textFormat.Glow, Alignment.CenterCenter, 0.9F, 0, Color.White, Color.Black);
                    g.Dispose();
                    AddIcon(new IconManager.UIIcon(batteryImage.image, ePriority.MediumLow, false));
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