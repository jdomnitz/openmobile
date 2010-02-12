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
using System.Windows.Forms;
using System.Collections.Generic;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// A system-wide event notification
    /// </summary>
    /// <param name="function"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public delegate void SystemEvent(eFunction function,string arg1,string arg2,string arg3);
    /// <summary>
    /// A media specific event notification
    /// </summary>
    /// <param name="function"></param>
    /// <param name="arg"></param>
    /// <param name="instance"></param>
    public delegate void MediaEvent(eFunction function, int instance,string arg);
    /// <summary>
    /// Triggered when a key is pressed on the keyboard
    /// </summary>
    /// <param name="type"></param>
    /// <param name="arg"></param>
    public delegate void KeyboardEvent(keypressType type,KeyEventArgs arg);
    /// <summary>
    /// A system power change event
    /// </summary>
    /// <param name="type"></param>
    public delegate void PowerEvent(ePowerEvent type);
    /// <summary>
    /// A storage media detection event
    /// </summary>
    /// <param name="type"></param>
    /// <param name="arg"></param>
    public delegate void StorageEvent(eMediaType type, string arg);
    /// <summary>
    /// A navigation event
    /// </summary>
    /// <param name="type"></param>
    /// <param name="arg"></param>
    public delegate void NavigationEvent(eNavigationEvent type, string arg);
    /// <summary>
    /// A wireless communications related event (Wifi, Bluetooth, etc)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="arg"></param>
    public delegate void WirelessEvent(eWirelessEvent type,string arg);
    /// <summary>
    /// The default plugin host interface
    /// </summary>
    public interface IPluginHost
    {
        /// <summary>
        /// Handle for the main rendering window
        /// </summary>
        IntPtr UIHandle(int screen);
        /// <summary>
        /// Returns the audio instance that matches the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        int instanceForScreen(int screen);
        /// <summary>
        /// Returns the number of screens currently being rendered
        /// </summary>
        int ScreenCount{get;}
        /// <summary>
        /// Returns the number of unique audio instances
        /// </summary>
        int InstanceCount { get; }
        /// <summary>
        /// The path to the plugins folder
        /// </summary>
        string PluginPath { get; }
        /// <summary>
        /// The path to the data folder
        /// </summary>
        string DataPath { get; }
        /// <summary>
        /// The path to the skins folder
        /// </summary>
        string SkinPath { get; }
        /// <summary>
        /// For UI plugins only.  Sets the number of controls to render first
        /// </summary>
        Int32 RenderFirst { get; set; }
        /// <summary>
        /// Execute the given function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arg1">Optional Argument</param>
        /// <param name="arg2">Optional Argument</param>
        /// <param name="arg3">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, string arg1, string arg2, string arg3);
        /// <summary>
        /// Execute the given function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arg1">Optional Argument</param>
        /// <param name="arg2">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, string arg1, string arg2);
        /// <summary>
        /// Execute the given function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arg">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, string arg);
        /// <summary>
        /// Execute the given function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        bool execute(eFunction function);
        /// <summary>
        /// Sends a message from one plugin to another
        /// </summary>
        /// <param name="to">The name of the destination plugin</param>
        /// <param name="from">The name of the source plugin</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        bool sendMessage(string to, string from, string message);
        /// <summary>
        /// Sends a message from one plugin to another
        /// </summary>
        /// <param name="to">The name of the destination plugin</param>
        /// <param name="from">The name of the source plugin</param>
        /// <param name="message">The message</param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool sendMessage<T>(string to, string from, string message, ref T data);
        /// <summary>
        /// Execute the function on all plugins of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        bool executeByType(Type type, eFunction function);
        /// <summary>
        /// Execute the function on all plugins of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="arg">Optional Argument</param>
        /// <returns></returns>
        bool executeByType(Type type, eFunction function, string arg);
        /// <summary>
        /// Execute the function on all plugins of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="arg1">Optional Argument</param>
        /// <param name="arg2">Optional Argument</param>
        /// <returns></returns>
        bool executeByType(Type type, eFunction function, string arg1,string arg2);
        /// <summary>
        /// Get the specified image from the skin directory
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        imageItem getSkinImage(string imageName);
        /// <summary>
        /// Get the specified image from the skin directory
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="noCache">Dont cache the image in the gloabl cache</param>
        /// <returns></returns>
        imageItem getSkinImage(string imageName,bool noCache);
        /// <summary>
        /// Gets information on the currently playing media
        /// </summary>
        /// <returns></returns>
        mediaInfo getPlayingMedia(int instance);
        /// <summary>
        /// Returns a list of the currently playing media
        /// </summary>
        List<mediaInfo> getPlaylist(int instance);
        /// <summary>
        /// Sets a playlist
        /// </summary>
        /// <param name="source"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool setPlaylist(List<mediaInfo> source, int instance);
        /// <summary>
        /// A system-wide event notification
        /// </summary>
        event SystemEvent OnSystemEvent;
        /// <summary>
        /// A media specific event notification
        /// </summary>
        event MediaEvent OnMediaEvent;
        /// <summary>
        /// A navigation event
        /// </summary>
        event NavigationEvent OnNavigationEvent;
        /// <summary>
        /// A system power change event
        /// </summary>
        event PowerEvent OnPowerChange;
        /// <summary>
        /// A storage media detection event
        /// </summary>
        event StorageEvent OnStorageEvent;
        /// <summary>
        /// Triggered when a keyboard key is pressed
        /// </summary>
        event KeyboardEvent OnKeyPress;
        /// <summary>
        /// Triggered when a wireless event occurs
        /// </summary>
        event WirelessEvent OnWirelessEvent;
        /// <summary>
        /// Returns the requested data
        /// </summary>
        /// <param name="dataType">The type of information to return (item specific)</param>
        /// <param name="name">Name of the plugin to retrieve data from</param>
        /// <param name="data">The returned data</param>
        void getData(eGetData dataType, string name, out object data);
        /// <summary>
        /// Returns the requested data
        /// </summary>
        /// <param name="dataType">The type of information to return (item specific)</param>
        /// <param name="name">Name of the plugin to retrieve data from</param>
        /// <param name="data">The returned data</param>
        /// <param name="param"></param>
        void getData(eGetData dataType, string name, string param, out object data);
    }
}