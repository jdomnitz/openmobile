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
using System.Text;
using OpenMobile;
using OpenMobile.Framework;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// A common codebase for the IHighLevel interface
    /// </summary>
    public abstract class HighLevelCode : BasePluginCode, IHighLevel
    {
        /// <summary>
        /// Initializes a new highlevel plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="pluginIcon"></param>
        /// <param name="pluginVersion"></param>
        /// <param name="pluginDescription"></param>
        /// <param name="displayName"></param>
        /// <param name="authorName"></param>
        /// <param name="authorEmail"></param>
        public HighLevelCode(string pluginName, imageItem pluginIcon, float pluginVersion, string pluginDescription, string displayName, string authorName, string authorEmail)
            : base(pluginName, pluginIcon, pluginVersion, pluginDescription, authorName, authorEmail)
        {
            _displayName = displayName;
            PanelManager = new ScreenManager(this);
        }

        /// <summary>
        /// The panel manager for this plugin
        /// </summary>
        public ScreenManager PanelManager = null;

        /// <summary>
        /// Loads the panels from this plugin
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        virtual public Controls.OMPanel loadPanel(string name, int screen)
        {
            return PanelManager[screen, name];
        }

        /// <summary>
        /// The displayname for this plugin
        /// </summary>
        virtual public string displayName
        {
            get { return _displayName; }
        }
        private string _displayName;

        /// <summary>
        /// Closes all other panels and shows only the specified one 
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public bool GotoPanel(int screen, string panelName)
        {
            return OM.Host.execute(eFunction.GotoPanel, screen, String.Format("{0};{1}", this.pluginName, panelName));
        }
        /// <summary>
        /// Closes all other panels and shows only the specified one using the specified transition effect
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="panelName"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public bool GotoPanel(int screen, string panelName, eGlobalTransition transition)
        {
            return OM.Host.execute(eFunction.GotoPanel, screen, String.Format("{0};{1}", this.pluginName, panelName), transition);
        }

        /// <summary>
        /// Shows a panel
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public bool ShowPanel(int screen, string panelName)
        {
            return OM.Host.execute(eFunction.ShowPanel, screen, String.Format("{0};{1}", this.pluginName, panelName));
        }
        /// <summary>
        /// Shows a panel using the specified transition effect
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="panelName"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public bool ShowPanel(int screen, string panelName, eGlobalTransition transition)
        {
            return OM.Host.execute(eFunction.ShowPanel, screen, String.Format("{0};{1}", this.pluginName, panelName), transition);
        }

        /// <summary>
        /// Goes back to the previous screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool GoBack(int screen)
        {
            return OM.Host.execute(eFunction.goBack, screen);
        }
        /// <summary>
        /// Goes back to the previous screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public bool GoBack(int screen, eGlobalTransition transition)
        {
            return OM.Host.execute(eFunction.goBack, screen, transition);
        }

        /// <summary>
        /// Unloads all panels and goes back to the main menu
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool GoHome(int screen)
        {
            return OM.Host.execute(eFunction.goHome, screen);
        }
        /// <summary>
        /// Unloads all panels and goes back to the main menu
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool GoHome(int screen, eGlobalTransition transition)
        {
            return OM.Host.execute(eFunction.goHome, screen, transition);
        }

        /// <summary>
        /// Closes all other panels and shows my settings panel
        /// </summary>
        /// <param name="screen"></param>
        public void GotoMySettingsPanel(int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(GetCmdString_GotoMySettingsPanel(screen));
        }

        /// <summary>
        /// Gets the command string to go to my settings panel
        /// </summary>
        public string GetCmdString_GotoMySettingsPanel(int screen = -1)
        {
            if (screen == -1)
                return String.Format("Screen{0}.Panel.Goto.OMSettings.{1}_Settings", Command.DataTag_Screen, this.pluginName);
            else
                return String.Format("Screen{0}.Panel.Goto.OMSettings.{1}_Settings", screen, this.pluginName);
        }

        /// <summary>
        /// Gets the command string to go to home
        /// </summary>
        public string GetCmdString_GoHome(int screen = -1)
        {
            if (screen == -1)
                return String.Format(String.Format("Screen{0}.Panel.Goto.Home", Command.DataTag_Screen));
            else
                return String.Format("Screen{0}.Panel.Goto.Home", screen);
        }

        /// <summary>
        /// Gets the command string to go back
        /// </summary>
        public string GetCmdString_GoBack(int screen = -1)
        {
            if (screen == -1)
                return String.Format(String.Format("Screen{0}.Panel.Goto.Previous", Command.DataTag_Screen));
            else
                return String.Format("Screen{0}.Panel.Goto.Previous", screen);
        }

        /// <summary>
        /// Gets the command string to go to a specific panel
        /// </summary>
        public string GetCmdString_GotoPanel(string panelName, int screen = -1)
        {
            if (screen == -1)
                return String.Format("Screen{0}.Panel.Goto.{1}.{2}", Command.DataTag_Screen, this.pluginName, panelName);
            else
                return String.Format("Screen{0}.Panel.Goto.{1}.{2}", screen, this.pluginName, panelName);
        }

    }
}
