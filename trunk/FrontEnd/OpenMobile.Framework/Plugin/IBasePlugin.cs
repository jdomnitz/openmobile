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
namespace OpenMobile.Plugin
{
    /// <summary>
    /// The plugin base interface
    /// </summary>
    public interface IBasePlugin:IDisposable
    {
        /// <summary>
        /// Initialize controls and get everything ready
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        eLoadStatus initialize(IPluginHost host);
        /// <summary>
        /// Returns the settings panel for the UI to load
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">System.NotImplementedException</exception>
        Settings loadSettings();
        /// <summary>
        /// Name of the plugin author
        /// </summary>
        string authorName{get;}
        /// <summary>
        /// Email address of the author
        /// </summary>
        string authorEmail{get;}
        /// <summary>
        /// Name of the plugin (should be gloabally unique)
        /// </summary>
        string pluginName{get;}
        /// <summary>
        /// Plugins version
        /// </summary>
        float pluginVersion{get;}
        /// <summary>
        /// Description of the plugin
        /// </summary>
        string pluginDescription { get; }
        /// <summary>
        /// Interprocess messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source">Source Plugins Name</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">System.NotImplementedException</exception>
        bool incomingMessage(string message, string source);
        /// <summary>
        /// Interprocess messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source">Source Plugins Name</param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">System.NotImplementedException</exception>
        bool incomingMessage<T>(string message, string source,ref T data);
    }
}
