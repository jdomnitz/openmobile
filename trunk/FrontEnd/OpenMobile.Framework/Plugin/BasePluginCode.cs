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
using OpenMobile.helperFunctions;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// A common code base for the IBasePlugin interface
    /// </summary>
    public abstract class BasePluginCode: IBasePlugin
    {
        public Settings MySettings = null;

        /// <summary>
        /// Initializes a new base plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="pluginIcon"></param>
        /// <param name="pluginVersion"></param>
        /// <param name="pluginDescription"></param>
        /// <param name="authorName"></param>
        /// <param name="authorEmail"></param>
        public BasePluginCode(string pluginName, imageItem pluginIcon, float pluginVersion, string pluginDescription, string authorName, string authorEmail)
        {
            _authorName = authorName;
            _authorEmail = authorEmail;
            _pluginName = pluginName;
            _pluginIcon = pluginIcon;
            _pluginVersion = pluginVersion;
            _pluginDescription = pluginDescription;
        }

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        abstract public eLoadStatus initialize(IPluginHost host);

        /// <summary>
        /// Loads the settings for this plugin
        /// </summary>
        /// <returns></returns>
        virtual public Settings loadSettings()
        {
            MySettings = new Settings(pluginName);
            MySettings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
            Settings();
            return MySettings;
        }

        /// <summary>
        /// Sets the settings for this plugin and lets the base code handle the rest
        /// </summary>
        /// <returns></returns>
        virtual public void Settings()
        {
        }

        /// <summary>
        /// Raised when a setting is changed
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="setting"></param>
        private void settings_OnSettingChanged(int screen, Setting setting)
        {
            StoredData.Set(this, setting.Name, setting.Value);
            setting_OnSettingChanged(screen, setting);
        }

        /// <summary>
        /// Override this to be informed when a setting is changed
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="setting"></param>
        virtual public void setting_OnSettingChanged(int screen, Setting setting)
        {            
        }

        /// <summary>
        /// Name of the author of this plugin
        /// </summary>
        virtual public string authorName
        {
            get { return _authorName; }
        }
        private string _authorName;

        /// <summary>
        /// Email to the author of this plugin
        /// </summary>
        virtual public string authorEmail
        {
            get { return _authorEmail; }
        }
        private string _authorEmail;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        virtual public string pluginName
        {
            get { return _pluginName; }
        }
        private string _pluginName;

        /// <summary>
        /// Version of the plugin
        /// </summary>
        virtual public float pluginVersion
        {
            get { return _pluginVersion; }
        }
        private float _pluginVersion;

        /// <summary>
        /// Description of the plugin
        /// </summary>
        virtual public string pluginDescription
        {
            get { return _pluginDescription; }
        }
        private string _pluginDescription;

        /// <summary>
        /// Icon for the plugin
        /// </summary>
        virtual public imageItem pluginIcon
        {
            get { return _pluginIcon; }
        }
        private imageItem _pluginIcon;

        /// <summary>
        /// Handles an incoming message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        virtual public bool incomingMessage(string message, string source)
        {
            return false;
        }

        /// <summary>
        /// Handles an incoming message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        virtual public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        /// <summary>
        /// Disposes of the plugin
        /// </summary>
        virtual public void Dispose()
        {
            //
        }
    }
}
