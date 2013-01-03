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
        }

        /// <summary>
        /// The panel manager for this plugin
        /// </summary>
        protected ScreenManager PanelManager = new ScreenManager();

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
    }
}
