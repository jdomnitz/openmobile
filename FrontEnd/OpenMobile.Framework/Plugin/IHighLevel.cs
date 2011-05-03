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

using OpenMobile.Controls;
using System;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// A high level plugin (used for rendering graphics)
    /// </summary>
    public interface IHighLevel : IBasePlugin
    {
        /// <summary>
        /// Returns the panel for the UI to load
        /// </summary>
        /// <returns></returns>
        OMPanel loadPanel(string name,int screen);
        /// <summary>
        /// The display name for the plugin
        /// </summary>
        string displayName { get; }
    }
    /// <summary>
    /// Sets the inital transition the main menu should use when loaded
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitialTransition : Attribute
    {
        private eGlobalTransition t;
        /// <summary>
        /// Sets the inital transition the main menu should use when loaded
        /// </summary>
        /// <param name="type"></param>
        public InitialTransition(eGlobalTransition type)
        {
            t = type;
        }
        /// <summary>
        /// Gets the inital transition the main menu should use when loaded
        /// </summary>
        public eGlobalTransition Transition
        {
            get
            {
                return t;
            }
        }
    }
    /// <summary>
    /// Enables/Disables the window closing transition
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FinalTransition : Attribute
    {
        bool t;
        /// <summary>
        /// Enables/Disables the window closing transition
        /// </summary>
        /// <param name="fade"></param>
        public FinalTransition(bool fade)
        {
            t = fade;
        }
        /// <summary>
        /// Enables/Disables the window closing transition
        /// </summary>
        public bool Transition
        {
            get
            {
                return t;
            }
        }
    }
    /// <summary>
    /// Defines an icon to use to represent this plugin.
    /// If the icon name is written with * as first character then the next 
    /// characters will generate the corresponding symbol from the Webdings font instead of using an image
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SkinIcon : Attribute
    {
        string name;
        /// <summary>
        /// Defines an icon to use to represent this plugin
        /// </summary>
        /// <param name="imageName"></param>
        public SkinIcon(string imageName)
        {
            name = imageName;
        }
        /// <summary>
        /// Defines an icon to use to represent this plugin
        /// </summary>
        public string SkinImageName
        {
            get
            {
                return name;
            }
        }
    }

    /// <summary>
    /// Pluginlevels
    /// </summary>
    [Flags]
    public enum PluginLevels:byte
    {
        /// <summary>
        /// Pluginlevel normal
        /// </summary>
        Normal=1,
        /// <summary>
        /// Pluginlevel system
        /// </summary>
        System=2,
        /// <summary>
        /// Pluginlevel user input provider
        /// </summary>
        UserInput=4
    }
    /// <summary>
    /// Defines the plugin level
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginLevel : Attribute
    {
        PluginLevels type = PluginLevels.Normal;
        /// <summary>
        /// Defines the level of this plugin
        /// </summary>
        /// <param name="type"></param>
        public PluginLevel(PluginLevels type)
        {
            this.type = type;
        }
        /// <summary>
        /// Defines the type of this plugin
        /// </summary>
        public PluginLevels TypeOfPlugin
        {
            get
            {
                return type;
            }
        }
    }
}
