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
using OpenMobile.Graphics;
using OpenMobile.Controls;
using OpenMobile.Zones;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// A system-wide event notification
    /// </summary>
    /// <param name="function"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public delegate void SystemEvent(eFunction function, string arg1, string arg2, string arg3);
    /// <summary>
    /// A media specific event notification
    /// </summary>
    /// <param name="function"></param>
    /// <param name="arg"></param>
    /// <param name="instance"></param>
    public delegate void MediaEvent(eFunction function, Zone zone, string arg);
    /// <summary>
    /// Triggered when a key is pressed on the keyboard
    /// </summary>
    /// <param name="type"></param>
    /// <param name="arg"></param>
    public delegate bool KeyboardEvent(eKeypressType type, OpenMobile.Input.KeyboardKeyEventArgs arg, ref bool handled);
    /// <summary>
    /// Triggered when a gesture is raised
    /// </summary>
    /// <param name="screen"></param>
    /// <param name="character"></param>
    /// <param name="pluginName"></param>
    /// <param name="panelName"></param>
    /// <param name="handled"></param>
    /// <returns></returns>
    public delegate bool GestureEvent(int screen, string character, string pluginName, string panelName, ref bool handled);
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
    /// <param name="justInserted"></param>
    public delegate void StorageEvent(eMediaType type, bool justInserted, string arg);
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
    public delegate void WirelessEvent(eWirelessEvent type, string arg);
    /// <summary>
    /// Delegate for method "ForEachScreen"
    /// </summary>
    /// <param name="Screen"></param>
    public delegate void ForEachScreenDelegate(int Screen);
    /// <summary>
    /// Delegate for method "ForEachZoneInZone"
    /// </summary>
    /// <param name="Screen"></param>
    public delegate bool ForEachZoneDelegate(Zone zone);
    /// <summary>
    /// Represents the graphics capabilities of the current platform
    /// </summary>
    public enum eGraphicsLevel
    {
        /// <summary>
        /// Full Graphics
        /// </summary>
        Standard = 0,
        /// <summary>
        /// Minimal Graphics - aka no effects
        /// </summary>
        Minimal = 1,
        /// <summary>
        /// High Graphics - aka disable adaptive framerate
        /// </summary>
        High = 2
    }
    /// <summary>
    /// The default plugin host interface
    /// </summary>
    public interface IPluginHost
    {
        /// <summary>
        /// The current GPS position and/or city/state/zip
        /// </summary>
        Location CurrentLocation { get; set; }
        /// <summary>
        /// Handle for the main rendering window
        /// </summary>
        object GetWindowHandle(int screen);
        /// <summary>
        /// Returns the audio instance that matches the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        //int GetDefaultZoneForScreen(int screen);
        /// <summary>
        /// Returns the number of screens currently being rendered
        /// </summary>
        int ScreenCount { get; }
        /// <summary>
        /// Returns the number of unique audio instances
        /// </summary>
        int AudioDeviceCount { get; }
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
        /// If the vehicle is in motion returns true (returns false if unknown)
        /// </summary>
        bool VehicleInMotion { get; set; }
        /// <summary>
        /// Sets the location video should be played (based on the 1000x600 default scale)
        /// <para>NB! This is used for all screens</para>
        /// </summary>
        void SetVideoPosition(int Screen, Rectangle videoArea);
        /// <summary>
        /// Gets the location video should be played (based on the 1000x600 default scale)
        /// </summary>
        Rectangle GetVideoPosition(int Screen);
        /// <summary>
        /// Sets the graphics level the application and plugins should use (represents the computers video performance)
        /// </summary>
        eGraphicsLevel GraphicsLevel { get; set; }
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
        /// <param name="arg3">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, object arg1, object arg2, object arg3);
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
        /// <param name="arg1">Optional Argument</param>
        /// <param name="arg2">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, object arg1, object arg2);
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
        /// <param name="arg">Optional Argument</param>
        /// <returns></returns>
        bool execute(eFunction function, object arg);
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
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="Message"></param>
        /// <param name="SourceTag"></param>
        /// <returns></returns>
        void SendStatusData(int Screen, eDataType DataType, IBasePlugin SourcePlugin, string SourceTag, string Message);
        /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="Message"></param>
        /// <param name="SourceTag"></param>
        /// <returns></returns>
        void SendStatusData(eDataType DataType, IBasePlugin SourcePlugin, string SourceTag, string Message);
        /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="Message"></param>
        /// <param name="SourceTag"></param>
        /// <returns></returns>
        void SendStatusData(int Screen, eDataType DataType, IBasePlugin SourcePlugin, string Message);
        /// <summary>
        /// Sends a status data update (Internally this raises a eFunction.backgroundOperationStatus event)
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="DataType"></param>
        /// <param name="SourcePlugin"></param>
        /// <param name="Message"></param>
        /// <param name="SourceTag"></param>
        /// <returns></returns>
        void SendStatusData(eDataType DataType, IBasePlugin SourcePlugin, string Message);
        /// <summary>
        /// Execute the function on all plugins of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="arg1">Optional Argument</param>
        /// <param name="arg2">Optional Argument</param>
        /// <returns></returns>
        bool executeByType(Type type, eFunction function, string arg1, string arg2);
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
        imageItem getSkinImage(string imageName, bool noCache);
        /// <summary>
        /// Gets information on the currently playing media
        /// </summary>
        /// <returns></returns>
        mediaInfo getPlayingMedia(Zone zone);
        /// <summary>
        /// Gets information on the currently playing media
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <returns></returns>
        mediaInfo getPlayingMedia(int Screen);
        /// <summary>
        /// Gets information on the media playing next
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        mediaInfo getNextMedia(Zone zone);
        /// <summary>
        /// Gets information on the media playing next
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <param name="Screen"></param>
        /// <returns></returns>
        mediaInfo getNextMedia(int Screen);
        /// <summary>
        /// Returns true if random playback is enabled.  False otherwise.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        bool getRandom(Zone zone);
        /// <summary>
        /// Returns true if random playback is enabled.  False otherwise.
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <param name="Screen"></param>
        /// <returns></returns>
        bool getRandom(int Screen);
        /// <summary>
        /// Returns true if successful
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setRandom(Zone zone, bool value);
        /// <summary>
        /// Returns true if successful
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setRandom(int Screen, bool value);
        /// <summary>
        /// Returns a list of the currently playing media
        /// </summary>
        List<mediaInfo> getPlaylist(Zone zone);
        /// <summary>
        /// Returns a list of the currently playing media
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        List<mediaInfo> getPlaylist(int Screen);
        /// <summary>
        /// Sets a playlist
        /// </summary>
        /// <param name="source"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool setPlaylist(List<mediaInfo> source, Zone zone);
        /// <summary>
        /// Sets a playlist
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Screen"></param>
        /// <returns></returns>
        bool setPlaylist(List<mediaInfo> source, int Screen);
        /// <summary>
        /// Appends a playlist to the currently loaded playlist
        /// </summary>
        /// <param name="source"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool appendPlaylist(List<mediaInfo> source, Zone zone);
        /// <summary>
        /// Appends a playlist to the currently loaded playlist
        /// <para>Method uses the currently assigned zone for the given screen</para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool appendPlaylist(List<mediaInfo> source, int Screen);
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
        /// Triggered when a gesture happens
        /// <para>Please note that this event is called in a last in first called manner</para>
        /// </summary>
        event GestureEvent OnGesture;
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
        /// <returns></returns>
        object getData(eGetData dataType, string name);
        /// <summary>
        /// Returns the requested data
        /// </summary>
        /// <param name="dataType">The type of information to return (item specific)</param>
        /// <param name="name">Name of the plugin to retrieve data from</param>
        /// <param name="data">The returned data</param>
        [Obsolete("Use object getData(eGetData dataType, string name) instead!")]
        void getData(eGetData dataType, string name, out object data);
        /// <summary>
        /// Returns the requested data
        /// </summary>
        /// <param name="dataType">The type of information to return (item specific)</param>
        /// <param name="name">Name of the plugin to retrieve data from</param>
        /// <returns></returns>
        object getData(eGetData dataType, string name, string param);
        /// <summary>
        /// Returns the requested data
        /// </summary>
        /// <param name="dataType">The type of information to return (item specific)</param>
        /// <param name="name">Name of the plugin to retrieve data from</param>
        /// <param name="data">The returned data</param>
        /// <param name="param"></param>
        [Obsolete("Use object getData(eGetData dataType, string name, string param) instead!")]
        void getData(eGetData dataType, string name, string param, out object data);
        /// <summary>
        /// Sends a message to the debug log
        /// </summary>
        /// <param name="from">The name of the source plugin</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        bool DebugMsg(string from, string message);
        /// <summary>
        /// Sends a message array to the debug log
        /// </summary>
        /// <param name="from">The name of the source plugin</param>
        /// <param name="header">The message header</param>
        /// <param name="messages">The messages</param>
        /// <returns></returns>
        bool DebugMsg(string from, string header, string[] messages);
        /// <summary>
        /// Sends a message to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns></returns>
        bool DebugMsg(string message);
        /// <summary>
        /// Sends a message array to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="header">The message header</param>
        /// <param name="message">The messages</param>
        /// <returns></returns>
        bool DebugMsg(string header, string[] messages);
        /// <summary>
        /// Sends a message array to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="message">The messages</param>
        /// <returns></returns>
        bool DebugMsg(DebugMessageType messageType, string message);
        /// <summary>
        /// Sends a message array to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="from">The name of the source plugin</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        bool DebugMsg(DebugMessageType messageType, string from, string message);
        /// <summary>
        /// Sends a message array to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="header">The message header</param>
        /// <param name="message">The messages</param>
        /// <returns></returns>
        bool DebugMsg(DebugMessageType messageType, string header, string[] messages);
        /// <summary>
        /// Sends a message array to the debug log (with automatic source detection)
        /// </summary>
        /// <param name="header">The message header</param>
        /// <param name="e">The exception</param>
        /// <returns></returns>
        bool DebugMsg(string header, Exception e);
        /// <summary>
        /// Raises a systemwide event
        /// </summary>
        /// <param name="e">event to raise</param>
        /// <param name="arg1">event specific</param>
        /// <param name="arg2">event specific</param>
        /// <param name="arg3">event specific</param>
        void raiseSystemEvent(eFunction e, string arg1, string arg2, string arg3);

        /// <summary>
        /// Raises a wireless event
        /// </summary>
        /// <param name="type">Type of event</param>
        /// <param name="arg"></param>
        void raiseWirelessEvent(eWirelessEvent type, string arg);

        /// <summary>
        /// Raises a storage event
        /// </summary>
        /// <param name="type">Type of media</param>
        /// <param name="justInserted"></param>
        /// <param name="arg"></param>
        void RaiseStorageEvent(eMediaType type, bool justInserted, string arg);

        /// <summary>
        /// Raises a power event
        /// </summary>
        /// <param name="type">Type of event</param>
        void raisePowerEvent(ePowerEvent e);

        /// <summary>
        /// Raises a navigation event
        /// </summary>
        /// <param name="type">Type of event</param>
        /// <param name="arg"></param>
        void raiseNavigationEvent(eNavigationEvent type, string arg);

        /// <summary>
        /// Raises a media event
        /// </summary>
        /// <param name="type">Type of event</param>
        /// <param name="zone">Zone the event is valid for</param>
        /// <param name="arg"></param>
        void raiseMediaEvent(eFunction type, Zone zone, string arg);

        /// <summary>
        /// Shows the identity of each screen
        /// </summary>
        void ScreenShowIdentity();
        /// <summary>
        /// Shows the identity of each screen for the given time
        /// </summary>
        void ScreenShowIdentity(int MS);
        /// <summary>
        /// Shows or hides the identity of each screen
        /// </summary>
        void ScreenShowIdentity(bool Show);
        /// <summary>
        /// Shows or hides the identity of each screen
        /// </summary>
        void ScreenShowIdentity(bool Show, float Opacity);
        /// <summary>
        /// Shows or hides the identity of a specific screen
        /// </summary>
        void ScreenShowIdentity(int Screen, bool Show);
        /// <summary>
        /// Shows or hides the identity of a specific screen
        /// </summary>
        void ScreenShowIdentity(int Screen, bool Show, float Opacity);

        /// <summary>
        /// Startup screen of OM (0 is normal screen, any other value means a specific startup screen is requested)
        /// </summary>
        int StartupScreen { get; set; }

        /// <summary>
        /// Sets the visibility of the pointer cursors in OM
        /// </summary>
        bool ShowCursors { get; set; }

        /// <summary>
        /// Sets the visibility of DebugInfo
        /// </summary>
        bool ShowDebugInfo { get; set; }

        /// <summary>
        /// Gets the name of the assosiated audio device
        /// </summary>
        /// <param name="instance">AudioDevice index</param>
        /// <returns></returns>
        string getAudioDeviceName(int instance);

        /// <summary>
        /// Get's the name of the default audio device 
        /// </summary>
        /// <returns></returns>
        string GetAudioDeviceDefaultName();

        /// <summary>
        /// Get's the audio device instance for a device name
        /// </summary>
        /// <param name="name">Audio device name</param>
        /// <returns>Audio device instance</returns>
        int getAudioDeviceInstance(string name);

        /// <summary>
        /// Multimedia zones
        /// </summary>
        ZoneHandler ZoneHandler { get; }

        /// <summary>
        /// Wrapper for calling a method on each screen
        /// <para>--------------</para>
        /// <para>Sample usage:</para>
        /// <para>theHost.ForEachScreen(delegate(int screen)</para>
        /// <para>{</para>
        /// <para>    Code to execute... use screen param to get the screen number</para>
        /// <para>};</para>
        /// </summary>
        /// <param name="d"></param>
        void ForEachScreen(ForEachScreenDelegate d);
    }
}
