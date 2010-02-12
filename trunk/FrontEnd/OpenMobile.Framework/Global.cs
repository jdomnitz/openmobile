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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace OpenMobile
{
    /// <summary>
    /// Occurs when a panel is requested for a screen but does not exist
    /// </summary>
    [global::System.Serializable]
    public class PanelNotAvailableForThisScreenException : Exception
    {
        /// <summary>
        /// Occurs when a panel is requested for a screen but does not exist
        /// </summary>
        public PanelNotAvailableForThisScreenException() { }
        /// <summary>
        /// Occurs when a panel is requested for a screen but does not exist
        /// </summary>
        /// <param name="message"></param>
        public PanelNotAvailableForThisScreenException(string message) : base(message) { }
        /// <summary>
        /// Occurs when a panel is requested for a screen but does not exist
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public PanelNotAvailableForThisScreenException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Occurs when a panel is requested for a screen but does not exist
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PanelNotAvailableForThisScreenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// The rendering mode
    /// </summary>
    public enum modeType {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Highlighted
        /// </summary>
        Highlighted = 1,
        /// <summary>
        /// Clicked
        /// </summary>
        Clicked = 2,
        /// <summary>
        /// Clicked and Transitioning Out
        /// </summary>
        ClickedAndTransitioningOut=3,
        /// <summary>
        /// Used by the skin designer
        /// </summary>
        Draging = 4,
        /// <summary>
        /// Used by the skin designer
        /// </summary>
        Resizing = 5,
        /// <summary>
        /// Being scrolled
        /// </summary>
        Scrolling = 6,
        /// <summary>
        /// Transitioning In
        /// </summary>
        transitioningIn = 7,
        /// <summary>
        /// Transitioning out
        /// </summary>
        transitioningOut = 8,
        /// <summary>
        /// Waiting for other controls to transition
        /// </summary>
        transitionLock=9, //When the same control is loaded and unloaded - do nothing
        /// <summary>
        /// A gesture is being drawn
        /// </summary>
        gesturing=10
    };
    /// <summary>
    /// Text format arguments
    /// </summary>
    public enum textFormat {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Drop Shadow
        /// </summary>
        DropShadow = 1,
        /// <summary>
        /// Bold
        /// </summary>
        Bold = 2,
        /// <summary>
        /// Italic
        /// </summary>
        Italic = 4,
        /// <summary>
        /// Underlined
        /// </summary>
        Underline = 6,
        /// <summary>
        /// Bold and Drop Shadow
        /// </summary>
        BoldShadow = 3,
        /// <summary>
        /// Italic and Drop Shadow
        /// </summary>
        ItalicShadow = 5,
        /// <summary>
        /// Underlined and Drop Shadow
        /// </summary>
        UnderlineShadow = 7,
        /// <summary>
        /// Outlined
        /// </summary>
        Outline = 8,
        /// <summary>
        /// Glowing Text
        /// </summary>
        Glow=9
    };
    /// <summary>
    /// The style list to render
    /// </summary>
    public enum eListStyle
    {
        /// <summary>
        /// A text only list
        /// </summary>
        TextList = 0,
        /// <summary>
        /// A list of icons and text
        /// </summary>
        ImageList = 1,
        /// <summary>
        /// A rounded list of text only
        /// </summary>
        RoundedTextList = 2,
        /// <summary>
        /// A rounded list of text and icons
        /// </summary>
        RoundedImageList = 3,
        /// <summary>
        /// A Text list with a transparent background
        /// </summary>
        TransparentTextList=4,
        /// <summary>
        /// A text and image list with a transparent background
        /// </summary>
        TransparentImageList=5,
        /// <summary>
        /// A custom text list style
        /// </summary>
        DroidStyleText=6,
        /// <summary>
        /// A custom text and image style
        /// </summary>
        DroidStyleImage=7
    }

    /// <summary>
    /// Alignment arguments
    /// </summary>
    public enum Alignment { 
        /// <summary>
        /// Top Left
        /// </summary>
        TopLeft = 0,
        /// <summary>
        /// Top Center
        /// </summary>
        TopCenter = 1,
        /// <summary>
        /// Top Right
        /// </summary>
        TopRight = 2,
        /// <summary>
        /// Center Left
        /// </summary>
        CenterLeft = 10,
        /// <summary>
        /// Center Center
        /// </summary>
        CenterCenter = 11,
        /// <summary>
        /// Center Right
        /// </summary>
        CenterRight = 12,
        /// <summary>
        /// Buttom Left
        /// </summary>
        BottomLeft = 20,
        /// <summary>
        /// Bottom Center
        /// </summary>
        BottomCenter = 21,
        /// <summary>
        /// Bottom Right
        /// </summary>
        BottomRight = 22,
        /// <summary>
        /// Vertical
        /// </summary>
        VerticalCentered=111
        };
    /// <summary>
    /// The angle to rotate the control
    /// </summary>
    public enum Angle {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Flipped across the Y-axis
        /// </summary>
        FlipHorizontal = 180 };
    /// <summary>
    /// An item in an OMList
    /// </summary>
    public sealed class OMListItem
    {
        /// <summary>
        /// The text to display
        /// </summary>
        public string text;
        /// <summary>
        /// The icon
        /// </summary>
        public Image image;
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        public OMListItem(string text)
        {
            this.text = text;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="image"></param>
        public OMListItem(string text,Image image)
        {
            this.text = text;
            this.image = image;
        }
    }
    
    
    /// <summary>
    /// An Open Mobile representation of an image
    /// </summary>
    [Editor(typeof(OMImageEditor),typeof(UITypeEditor))]
    public struct imageItem
    {
        /// <summary>
        /// The image
        /// </summary>
        public Image image;
        /// <summary>
        /// The image name in the skin folder
        /// </summary>
        public String name;
        /// <summary>
        /// Construct an image item from an image
        /// </summary>
        /// <param name="i"></param>
        public imageItem(Image i)
        {
            this.name = "Unknown";
            this.image = i;
        }
        /// <summary>
        /// Construct an image item from a name
        /// </summary>
        /// <param name="imageName"></param>
        public imageItem(string imageName)
        {
            this.name = imageName;
            this.image = null;
        }
        /// <summary>
        /// Construct an image item from an image
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Name"></param>
        public imageItem(Image i, string Name)
        {
            this.name = Name;
            this.image = i;
        }
        /// <summary>
        /// Provides the name of an image
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (image == null)
                return null;
            else
                return name;
        }
        /// <summary>
        /// Value Comparison
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator ==(imageItem item1, imageItem item2)
        {
            if (item1.name != item2.name)
                return false;
            if (item1.image != item2.image)
                return false;
            return true;
        }
        /// <summary>
        /// Value comparison
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator !=(imageItem item1, imageItem item2)
        {
            if (item1.name != item2.name)
                return true;
            if (item1.image != item2.image)
                return true;
            return false;
        }
        /// <summary>
        /// Value comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return (this == (imageItem)obj);
        }
        /// <summary>
        /// Serves as a hash function for a particular type
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return image.GetHashCode();
        }
    }
    /// <summary>
    /// The button click transition
    /// </summary>
    public enum eButtonTransition {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Box Out Effect
        /// </summary>
        BoxOut = 1 };

    /// <summary>
    /// The type of transition between panels
    /// </summary>
    public enum eGlobalTransition
    {
        /// <summary>
        /// Panel A is unloaded and Panel B is loaded (more efficient then the load and unload functions)
        /// </summary>
        None=0,
        /// <summary>
        /// Panel A fades out while Panel B fades in
        /// </summary>
        Crossfade=1,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideUp=2,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideDown = 3,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideLeft = 4,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideRight = 5
    }

    /// <summary>
    /// The type of keyboard event that just occured
    /// </summary>
    public enum keypressType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown=0,
        /// <summary>
        /// Key was released
        /// </summary>
        KeyUp=1,
        /// <summary>
        /// Key was pressed
        /// </summary>
        KeyDown=2
    }

    /// <summary>
    /// Gloabl Functions
    /// </summary>
    public enum eFunction
    {
        /// <summary>
        /// Unknown Function or Function Handled
        /// </summary>
        None=0,
        //LoadPanel=1,
        /// <summary>
        /// Load a settings panel onto the UI
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        LoadSettings=2,
        /// <summary>
        /// Unloads all loaded panels except the UI
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        TransitionFromAny=3,
        /// <summary>
        /// Unload a settings panel from the UI
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        UnloadSettings=4,
        /// <summary>
        /// Load a panel and prepare to transition to it
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionToPanel=5,
        /// <summary>
        /// Load a settings panel and prepare to transition to it
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionToSettings=6,
        /// <summary>
        /// Transition between the previously specified panels
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Name</para>
        /// </summary>
        ExecuteTransition=7,
        /// <summary>
        /// The panel to transition from
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionFromPanel=8,
        /// <summary>
        /// The settings panel to transition from
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionFromSettings = 9,
        //Media Events
        /// <summary>
        /// Play the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: (Optional) File URL</para>
        /// </summary>
        Play=10,
        /// <summary>
        /// Pause the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        Pause=11,
        /// <summary>
        /// Stop the currently playing media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        Stop=12,
        /// <summary>
        /// Set the playback position of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Position [Miliseconds]</para>
        /// </summary>
        setPosition=13,
        /// <summary>
        /// Set the playback speed of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Speed [Float]</para>
        /// </summary>
        setPlaybackSpeed=14,
        /// <summary>
        /// Set the playback volume of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Volume [Int -1(mute) to 100]</para>
        /// </summary>
        setPlayingVolume=15,
        /// <summary>
        /// Play the next media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        nextMedia=16,
        /// <summary>
        /// Play the previous media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        previousMedia=17,
        /// <summary>
        /// Load an A/V Player plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// </summary>
        loadAVPlayer=18,
        /// <summary>
        /// Load a Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// </summary>
        loadTunedContent=19,
        /// <summary>
        /// Unload the current AV Player plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        unloadAVPlayer=20,
        /// <summary>
        /// Unload the current Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        unloadTunedContent=21,
        /// <summary>
        /// Occurs whenever the system volume changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Volume [int -1(mute) to 100]</para>
        /// </summary>
        systemVolumeChanged=22,
        /// <summary>
        /// The playlist has been modified
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance</para>
        /// </summary>
        playlistChanged=23,
        //ToDo - Dont forget bluetooth

        //Data Provider
        /// <summary>
        /// Refresh a data providers data
        /// <para>---------------------------------------</para>
        /// <para>Arg1: (Optional)Plugin Name (If blank refreshes all data providers)</para>
        /// <para>Arg2: (Optional)Plugin Specific First Arg</para>
        /// <para>Arg3: (Optional)Plugin Specific Second Arg</para>
        /// </summary>
        refreshData=24,
        //Network
        /// <summary>
        /// Connect to the internet
        /// <para>-----------------------------</para>
        /// <para>Arg1: (Optional) Network Name</para>
        /// </summary>
        connectToInternet=25,
        /// <summary>
        /// Disconnect from the internet
        /// <para>-----------------------------</para>
        /// <para>Arg1: (Optional) Network Name</para>
        /// </summary>
        disconnectFromInternet=26,
        /// <summary>
        /// Occurs when a valid internet connection is detected
        /// </summary>
        connectedToInternet=27,
        /// <summary>
        /// Occurs when an internet connection is disconnected/lost
        /// </summary>
        disconnectedFromInternet=28,
        /// <summary>
        /// Occurs when network connections are available
        /// </summary>
        networkConnectionsAvailable=29,
        //Miscellaneous
        /// <summary>
        /// Reset a hardware device
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Plugin Name</para>
        /// <para>Arg2: (Optional) Device Name</para>
        /// </summary>
        resetDevice=30,
        /// <summary>
        /// Plugin specific. Triggered as a system event.
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// <para>Arg3: Data</para>
        /// </summary>
        userInputReady=31,
        /// <summary>
        /// Raised when the core has finished loading and initializing all plugins
        /// </summary>
        pluginLoadingComplete=32,
        /// <summary>
        /// A status update on a background operation
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Status Message</para>
        /// <para>Arg2: (Optional) Status Source</para>
        /// </summary>
        backgroundOperationStatus=33,
        /// <summary>
        /// Sets the system volume
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Volume [int -1(mute) to 100]</para>
        /// </summary>
        setSystemVolume=34,
        /// <summary>
        /// Restart this application
        /// </summary>
        restartProgram=44,
        /// <summary>
        /// Close this program
        /// </summary>
        closeProgram=45,
        /// <summary>
        /// Hibernate the computer
        /// </summary>
        hibernate=46,
        /// <summary>
        /// Shutdown the computer
        /// </summary>
        shutdown=47,
        /// <summary>
        /// Force the computer to enter low power mode
        /// </summary>
        standby=48,
        //SpeechRecognition
        /// <summary>
        /// Load a speech recognition context
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Context Name</para>
        /// </summary>
        loadSpeechContext=50,
        /// <summary>
        /// Adds an item to a speech recognition context
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Context Name</para>
        /// <para>Arg2: Recognition String</para>
        /// </summary>
        addSpeechContext=51,
        /// <summary>
        /// Listen for a speech command
        /// </summary>
        listenForSpeech=52,
        /// <summary>
        /// Speak the indicated text
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Text To Speak</para>
        /// </summary>
        Speak=53,
        /// <summary>
        /// Stop speaking and purge all queued speech from the buffer
        /// </summary>
        StopSpeaking=54,
        //Tuned Content
        /// <summary>
        /// Tune to the given statioin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Specific</para>
        /// </summary>
        tuneTo=60,
        /// <summary>
        /// Scan forward (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        scanForward=61,
        /// <summary>
        /// Scan backward (Tuned Content)
        ///<para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        scanBackward=62,
        /// <summary>
        /// Step Forward (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        stepForward = 63,
        /// <summary>
        /// Step Backward (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        stepBackward = 64,
        /// <summary>
        /// Power on a device (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        powerOnDevice=65,
        /// <summary>
        /// Power off a device (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        powerOffDevice=66,
        /// <summary>
        /// Go back to the previous panel
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) TransitionType</para>
        /// </summary>
        goBack=80
    }
    /// <summary>
    /// The status of a plugins initialization
    /// </summary>
    public enum eLoadStatus
    {
        /// <summary>
        /// Everything went OK.  Plugin is initialized
        /// </summary>
        LoadSuccessful=0,
        /// <summary>
        /// Plugin requires default settings to continue.  Load default settings panel and then re-initialize when the user has set the settings.
        /// </summary>
        SettingsRequired=1,
        /// <summary>
        /// A required dependency is not loaded yet or something needs to be retried.  Load the rest of the plugins and then retry the initialization.
        /// </summary>
        LoadFailedRetryRequested=2,
        /// <summary>
        /// Load failed or an unknown error occured.  Plugin will be unloaded. (Occurs automatically if an uncaught error is thrown)
        /// </summary>
        LoadFailedUnloadRequested=10
    }

    /// <summary>
    /// System power change events
    /// </summary>
    public enum ePowerEvent
    {
        /// <summary>
        /// Unknown event
        /// </summary>
        Unknown=0,
        /// <summary>
        /// System is shutting down
        /// </summary>
        ShutdownPending=1,
        /// <summary>
        /// System is retarting
        /// </summary>
        LogoffPending=2,
        /// <summary>
        /// System is going to sleep
        /// </summary>
        SleepOrHibernatePending=3,
        /// <summary>
        /// System is resuming from sleep or hibernate
        /// </summary>
        SystemResumed=4,
        /// <summary>
        /// System battery low
        /// </summary>
        BatteryLow=5,
        /// <summary>
        /// System battery critical
        /// </summary>
        BatteryCritical=6,
        /// <summary>
        /// System running on battery
        /// </summary>
        SystemOnBattery=7,
        /// <summary>
        /// System running on AC Power
        /// </summary>
        SystemPluggedIn=8
    }
    /// <summary>
    /// Various Wireless Events (Bluetooth, WiFi, etc.)
    /// </summary>
    public enum eWirelessEvent
    {
        /// <summary>
        /// An unknown or handled event
        /// </summary>
        Unknown=0,
        /// <summary>
        /// Wireless networks are detected and in-range
        /// </summary>
        WirelessNetworksAvailable=1,
        /// <summary>
        /// A successful connection has been established to a wireless connection
        /// </summary>
        ConnectedToWirelessNetwork=2,
        /// <summary>
        /// Wireless (wifi) signal strength has changed
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Signal Value (0-100)</para>
        /// </summary>
        WirelessSignalStrengthChanged=3,
        /// <summary>
        /// The network connection has been established but an internet connection
        /// requires Access Point credentials be entered in the web browser
        /// </summary>
        WirelessNetworkRequiresLogin=4,
        /// <summary>
        /// Bluetooth devices are detected and within the connection range
        /// </summary>
        BluetoothDevicesInRange=10,
        /// <summary>
        /// A successful connection has been made to a bluetooth internet connection (DUN, PAN, etc)
        /// </summary>
        ConnectedToBluetoothInternet=11,
        /// <summary>
        /// The bluetooth connected device's (phone's) signal strength
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Signal Value (0-100) (0=No Signal,-1=not supported)</para>
        /// </summary>
        BluetoothSignalStrengthChanged=12,
        /// <summary>
        /// Bluetooth Battery Level
        /// <para>--------------------------------------------</para>
        /// <para>Arg: Battery Level (0-10) (0=Battery info not supported)</para>
        /// </summary>
        BluetoothBatteryLevelChanged=13,
        /// <summary>
        /// A bluetooth pairing request has been received
        /// <para>--------------------------------------------</para>
        /// <para>Arg: Device Name</para>
        /// </summary>
        BluetoothPairingRequest=14,
        /// <summary>
        /// A bluetooth device has been successfully paired
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Device Name</para>
        /// </summary>
        BluetoothDeviceConnected=15,
        /// <summary>
        /// A call is incoming
        /// <para>-------------------------------------------</para>
        /// <para>Arg: 2 Lines delimited by Cr+Lf</para>
        /// <para>Line1: Caller Name (Either from contacts or callerID; May be blank if Unknown)</para>
        /// <para>Line2: Phone Number (May be blank if Unknown)</para>
        /// </summary>
        BluetoothIncomingCall=16,
        /// <summary>
        /// An SMS message has been received
        /// <para>-------------------------------------------</para>
        /// <para>Arg: MessageID (Contents be retrieved from database)</para>
        /// </summary>
        BluetoothSMSReceived=17,
        /// <summary>
        /// A Pairing Password is available
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Pairing Password</para>
        /// </summary>
        PairingPasswordAvailable=18
    }

    /// <summary>
    /// Type of media
    /// </summary>
    public enum eMediaType
    {
        /// <summary>
        /// Unknown Type/Not Set
        /// </summary>
        NotSet	=	0,
        /// <summary>
        /// A local file
        /// </summary>
        Local	=	1,
        /// <summary>
        /// A network file
        /// </summary>
        Network	=	2,
        /// <summary>
        /// Attached devices
        /// </summary>
        LocalHardware	=	3,
        /// <summary>
        /// An Audio CD
        /// </summary>
        AudioCD	=	4,
        /// <summary>
        /// A DVD
        /// </summary>
        DVD	=	5,
        /// <summary>
        /// A Blu-Ray Disc
        /// </summary>
        BluRay	=	6,
        /// <summary>
        /// An HD-DVD
        /// </summary>
        HDDVD	=	7,
        /// <summary>
        /// A URL (Streaming media)
        /// </summary>
        HTTPUrl	=	8,
        /// <summary>
        /// An RTP URL (Streaming media)
        /// </summary>
        RTPUrl	=	9,
        /// <summary>
        /// A YouTube video
        /// </summary>
        YouTube	=	10,
        /// <summary>
        /// An iPod
        /// </summary>
        iPod	=	11,
        /// <summary>
        /// A bluetooth audio device
        /// </summary>
        BluetoothResource	=	12,
        /// <summary>
        /// An openDrive device
        /// </summary>
        OpenDrive 	=	13,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved	=	14,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved2	=	15,
        /// <summary>
        /// Other
        /// </summary>
        Other	=	16
    }
    /// <summary>
    /// Textbox Options
    /// </summary>
    [Flags]
    public enum textboxFlags
    {
        /// <summary>
        /// None Set
        /// </summary>
        None = 0,
        /// <summary>
        /// Add an Ellipsis to the center of the text if necessary
        /// </summary>
        EllipsisCenter = 1,
        /// <summary>
        /// Add an Ellipsis to the end of the text if necessary
        /// </summary>
        EllipsisEnd = 2,
        /// <summary>
        /// Truncate the text at the nearest whole word
        /// </summary>
        TrimNearestWord = 4,
        /// <summary>
        /// Allow only numbers in the textbox
        /// </summary>
        NumericOnly = 8,
        /// <summary>
        /// Do not allow numbers in the text
        /// </summary>
        AlphabeticalOnly = 16,
        /// <summary>
        /// Password box.  Will paint each letter as a *
        /// </summary>
        Password = 32
    }
    /// <summary>
    /// The style of background for an OMPanel
    /// </summary>
    public enum backgroundStyle
    {
        /// <summary>
        /// No Background (use the layer underneath)
        /// </summary>
        None = 0,
        /// <summary>
        /// Use BackgroundImage
        /// </summary>
        Image = 1,
        /// <summary>
        /// Use BackgroundColor1 and BackgroundColor2
        /// </summary>
        Gradiant = 2,
        /// <summary>
        /// Use BackgroundColor1
        /// </summary>
        SolidColor=3
    }
    /// <summary>
    /// Priority
    /// </summary>
    public enum priority
    {

        /// <summary>
        /// Low Priority
        /// </summary>
        Low,
        /// <summary>
        /// Medium-Low Priority
        /// </summary>
        MediumLow,
        /// <summary>
        /// Normal Priority
        /// </summary>
        Normal,
        /// <summary>
        /// Medium-High Priority
        /// </summary>
        MediumHigh,
        /// <summary>
        /// High Priority
        /// </summary>
        High,
        /// <summary>
        /// Urgent Priority
        /// </summary>
        Urgent
    }
    /// <summary>
    /// The type of data to retrieve
    /// </summary>
    public enum eGetData
    {
        /// <summary>
        /// Get the updater status of the data provider
        /// </summary>
        DataProviderStatus=1,
        /// <summary>
        /// Get the position (in seconds) of the currently playing media
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaPosition=2,
        /// <summary>
        /// Gets the volume (0-100)
        /// </summary>
        GetVolume=3,
        /// <summary>
        /// Gets an array of availble networks
        /// </summary>
        GetAvailableNetworks=4,
        /// <summary>
        /// Gets the device info string from a raw hardware plugin
        /// </summary>
        GetDeviceInfo=5,
        /// <summary>
        /// Gets the firmware info string from a raw hardware plugin
        /// </summary>
        GetFirmwareInfo=6,
        /// <summary>
        /// Gets the TunedContentInfo from the currently loaded tunedcontent plugin
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetTunedContentInfo=7,
        /// <summary>
        /// Gets the status of the currently loaded media player
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaStatus=8,
        /// <summary>
        /// Returns an IMediaDatabase object
        /// </summary>
        GetMediaDatabase=9,
        /// <summary>
        /// Returns the loaded plugin collection.
        /// </summary>
        GetPlugins=10,
        /// <summary>
        /// Get the playback speed [float] of the currently playing media
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetPlaybackSpeed=11
    }

    /// <summary>
    /// Information about a network connection
    /// </summary>
    public sealed class connectionInfo
    {
        /// <summary>
        /// Returns the name of the network
        /// </summary>
        public string NetworkName;
        /// <summary>
        /// Plugin Specific (com port #, BSSID, etc.)
        /// </summary>
        public string UID;
        /// <summary>
        /// Returns the theoretical connection speed in kbps (ex 56 for dialup, 144 for bluetooth, 10024 for 802.11b, etc). Returns -1 for unknown.
        /// </summary>
        public int potentialSpeed;
        /// <summary>
        /// Retrieves the signal strength of the network connection. Strength should be represented as positive numbers where higher is stronger. Range is plugin specific (0-x). Returns -1 for not applicable.
        /// </summary>
        public int signalStrength;
    }

    /// <summary>
    /// Navigation Events
    /// </summary>
    public enum eNavigationEvent
    {
        /// <summary>
        /// Unknown Navigation Event
        /// </summary>
        Unknown=0,
        /// <summary>
        /// Occurs when the GPS Fix changes
        /// Arg: No GPS, No Signal, 2D Fix, 3D Fix
        /// </summary>
        GPSStatusChange=1,
        /// <summary>
        /// Route has changed
        /// Arg: Navigation Started, Navigation Cancelled, Waypoint Added, Detouring
        /// </summary>
        RouteChanged=2,
        /// <summary>
        /// A turn is approaching...query the nextTurn parameter for more info
        /// </summary>
        TurnApproaching=3
    }

    /// <summary>
    /// A/V Player statue
    /// </summary>
    public enum ePlayerStatus
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Stopped
        /// </summary>
        Stopped = 1,
        /// <summary>
        /// Ready to play (No media loaded)
        /// </summary>
        Ready = 2,
        /// <summary>
        /// Playing
        /// </summary>
        Playing = 3,
        /// <summary>
        /// Paused
        /// </summary>
        Paused = 4,
        /// <summary>
        /// Fast Forwarding
        /// </summary>
        FastForwarding = 5,
        /// <summary>
        /// Rewinding
        /// </summary>
        Rewinding = 6,
        /// <summary>
        /// Transitioning
        /// </summary>
        Transitioning = 7,
        /// <summary>
        /// Connecting to media
        /// </summary>
        Connecting = 8,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved = 9,
        /// <summary>
        /// An error has occured
        /// </summary>
        Error = 10,
        /// <summary>
        /// Other
        /// </summary>
        Other = 11
    }
    /// <summary>
    /// A high speed System.IO.Path
    /// </summary>
    public static class Path
    {
        /// <summary>
        /// Combines two path strings
        /// </summary>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public static string Combine(string part1, string part2)
        {
            if ((part1==null)||(part2==null))
                throw new ArgumentNullException();
            if (part1.Length == 0)
                return part2;
            else if (part2.Length == 0)
                return part1;
            char ds = System.IO.Path.DirectorySeparatorChar;
            if (part1[part1.Length - 1] == ds)
                return part1 + part2;
            return part1 +ds+ part2;
        }
        /// <summary>
        /// Combines three path strings
        /// </summary>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <param name="part3"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public static string Combine(string part1, string part2,string part3)
        {
            if ((part1 == null) ||(part2==null)|| (part3 == null))
                throw new ArgumentNullException();
            char ds = System.IO.Path.DirectorySeparatorChar;
            char ads=System.IO.Path.AltDirectorySeparatorChar;

            if ((part1[part1.Length - 1] == ds) || (part1[part1.Length - 1] == ads) || (part1[part1.Length - 1] == System.IO.Path.VolumeSeparatorChar))
                if ((part2[part2.Length - 1] == ds)||(part1[part1.Length - 1]==ads))
                    return part1 + part2 + part3;
                else
                    return part1 + part2 + ds + part3;
            else
                if ((part2[part2.Length - 1] == ds)||(part1[part1.Length - 1]==ads))
                    return part1 +ds+ part2 + part3;
                else
                    return part1 +ds+ part2 + ds + part3;
        }
        /// <summary>
        /// Extracts a filename from a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            if (path != null)
            {
                int length = path.Length;
                int num2 = length;
                while (--num2 >= 0)
                {
                    char ch = path[num2];
                    if (((ch == System.IO.Path.DirectorySeparatorChar) || (ch == System.IO.Path.AltDirectorySeparatorChar)))
                    {
                        num2++;
                        return path.Substring(num2, length - num2);
                    }
                }
            }
            return path;
        }
        /// <summary>
        /// Extracts a filename without an extension from a path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(string file)
        {
            if (file != null)
            {
                int length = file.Length;
                int num2 = length;
                int ext=0;
                while (--num2 >= 0)
                {
                    char ch = file[num2];
                    if (((ch == System.IO.Path.DirectorySeparatorChar) || (ch == System.IO.Path.AltDirectorySeparatorChar)))
                    {
                        num2++;
                        return file.Substring(num2, ext - num2);
                    }
                    if ((ext == 0) && (file[num2] == '.'))
                    {
                        ext = num2;
                    }
                }
            }
            return file;
        }
 

    }

    /// <summary>
    /// Information on the currently playing media
    /// </summary>
    public sealed class mediaInfo
    {
        /// <summary>
        /// Song/Movie Name
        /// </summary>
        public string Name;
        /// <summary>
        /// Artist
        /// </summary>
        public string Artist;
        /// <summary>
        /// Album
        /// </summary>
        public string Album;
        /// <summary>
        /// Location (local path, url, etc)
        /// </summary>
        public string Location;
        /// <summary>
        /// Track Number
        /// </summary>
        public int TrackNumber;
        /// <summary>
        /// Media Genre
        /// </summary>
        public string Genre;
        /// <summary>
        /// Length in miliseconds
        /// </summary>
        public int Length;
        /// <summary>
        /// Rating 0-5.  -1 for not set.
        /// </summary>
        public int Rating;
        /// <summary>
        /// Optional - The cover art for the selected media
        /// </summary>
        public Image coverArt;
        /// <summary>
        /// Create a new mediaInfo object
        /// </summary>
        /// <param name="URL"></param>
        public mediaInfo(string URL)
        {
            this.Location = URL;
        }
        /// <summary>
        /// Create a new mediaInfo object
        /// </summary>
        public mediaInfo() { }
    }

    /// <summary>
    /// Information on a Tuned Station
    /// </summary>
    public sealed class stationInfo
    {
        /// <summary>
        /// Station Name (Empty String if unknown)
        /// </summary>
        public string stationName;
        /// <summary>
        /// Station Number
        /// </summary>
        public string stationNumber;
        /// <summary>
        /// Value: 0(no signal) - 5(full signal)
        /// </summary>
        public int signal;
        /// <summary>
        /// Channel is broadcast in HD
        /// </summary>
        public bool HD;
    }

    /// <summary>
    /// Type of Tuned Content
    /// </summary>
    public enum tunedContentType
    {
        /// <summary>
        /// None or Unknown
        /// </summary>
        None=0,
        /// <summary>
        /// AM/FM Radio - HD or Analog
        /// </summary>
        AMFMRadio=1,
        /// <summary>
        /// Satellite Radio - XM or Sirius
        /// </summary>
        SatelliteRadio=2,
        /// <summary>
        /// Over the air TV - ATSC
        /// </summary>
        OTATV=3,
        /// <summary>
        /// Cable TV
        /// </summary>
        CableTV=4,
        /// <summary>
        /// Satellite TV
        /// </summary>
        SatelliteTV=5,
        /// <summary>
        /// AM/FM Radio and Over The Air TV
        /// </summary>
        AMFMRadioAndOTATV=6,
        /// <summary>
        /// Other
        /// </summary>
        Other=10
    }
    /// <summary>
    /// Provides proper naming of semi-transparent colors
    /// </summary>
    public sealed class ColorConvertor : System.ComponentModel.TypeConverter
    {
        /// <summary>
        /// Converts colors to a string representation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(Color))
                return true;
            return false;
        }
        /// <summary>
        /// Converts colors to strings
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }
        /// <summary>
        /// Converts colors to a string representation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            Color c = (Color)value;
            if (c.IsNamedColor)
                return c.Name;
            if (c.IsEmpty == true)
                return "Empty";
            Color nc = findColorName(c);
            if (nc != Color.Empty)
                if (c.A == 255)
                    return nc.Name;
                else
                    return nc.Name + " (" + ((int)(c.A / 2.55)).ToString() + "%)";
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2") + " (" + ((int)(c.A / 2.55)).ToString() + "%)";
        }
        private Color findColorName(Color match)
        {
            foreach (object o in Enum.GetValues(typeof(KnownColor)))
            {
                Color c=Color.FromKnownColor((KnownColor)o);
                if (c.IsSystemColor == true)
                    continue;
                if (c.Name == "Transparent")
                    continue;
                if (c.R != match.R)
                    continue;
                if (c.B != match.B)
                    continue;
                if (c.G != match.G)
                    continue;
                return c;
            }
            foreach (object o in Enum.GetValues(typeof(KnownColor)))
            {
                Color c = Color.FromKnownColor((KnownColor)o);
                if (c.IsSystemColor == false)
                    continue;
                if (c.R != match.R)
                    continue;
                if (c.B != match.B)
                    continue;
                if (c.G != match.G)
                    continue;
                return c;
            }
            return Color.Empty;
        }
    }
    /// <summary>
    /// Provides an OMImageItem editor
    /// </summary>
    public sealed class OMImageEditor : UITypeEditor
    {
        IWindowsFormsEditorService service;
        /// <summary>
        /// Editor style
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        /// <summary>
        /// Edit an imageItem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            System.Windows.Forms.ListBox listBox1 = new System.Windows.Forms.ListBox();
            foreach (string s in Directory.GetFiles(Path.Combine(Application.StartupPath, "Skins"), "*.png", SearchOption.TopDirectoryOnly))
            {
                FileInfo f = new FileInfo(s);
                listBox1.Items.Add(f.Name.Remove(f.Name.Length - 4));
            }
            foreach (string s in Directory.GetFiles(Path.Combine(Application.StartupPath, "Skins"), "*.gif", SearchOption.TopDirectoryOnly))
            {
                FileInfo f = new FileInfo(s);
                listBox1.Items.Add(f.Name.Remove(f.Name.Length - 4)); 
            }
            foreach (string s in Directory.GetFiles(Path.Combine(Application.StartupPath, "Skins"), "*.jpg", SearchOption.TopDirectoryOnly))
            {
                FileInfo f = new FileInfo(s);
                listBox1.Items.Add(f.Name.Remove(f.Name.Length - 4));
            }
            listBox1.Click += new EventHandler(comboBox1_DropDownClosed);
            listBox1.Height = 150;
            service.DropDownControl(listBox1);
            return new imageItem(Image.FromFile(Path.Combine(Application.StartupPath, "Skins", listBox1.SelectedItem.ToString()) + ".png"), listBox1.SelectedItem.ToString());
        }

        void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            service.CloseDropDown();
        }
    }
    /// <summary>
    /// Provides a color editor with transparency
    /// </summary>
    public sealed class transparentColor : System.Drawing.Design.UITypeEditor
    {
        private System.Windows.Forms.Design.IWindowsFormsEditorService service;
        private Color selectedColor=Color.Empty;
        /// <summary>
        /// The style of editor
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        /// <summary>
        /// Presents an editor dialog for the given value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            service= (System.Windows.Forms.Design.IWindowsFormsEditorService) provider.GetService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService));
            OpenMobile.Controls.TransparentColorPicker picker;
            if (value == null)
                picker = new OpenMobile.Controls.TransparentColorPicker(Color.Firebrick);
            else
            {
                picker = new OpenMobile.Controls.TransparentColorPicker((Color)value);
            }
            selectedColor = Color.Empty;
            picker.SelectedColorChanged += new OpenMobile.Controls.TransparentColorPicker.ColorChanged(picker_SelectedColorChanged);
            service.DropDownControl(picker);
            if (selectedColor != Color.Empty)
                value = (object)selectedColor;
            else
                value = (object)picker.ForeColor;
            return value;
        }

        void picker_SelectedColorChanged(Color c)
        {
            selectedColor = c;
            service.CloseDropDown();
        }
        /// <summary>
        /// Can provide a color representation of itself
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return true;
        }
        /// <summary>
        /// Paints the color in the preview square
        /// </summary>
        /// <param name="e"></param>
        public override void PaintValue(PaintValueEventArgs e)
        {
            Color c = (Color)e.Value;
            e.Graphics.FillRectangle(new SolidBrush(c), e.Bounds);
        }
    }
    /// <summary>
    /// Rendering Parameters
    /// </summary>
    public sealed class renderingParams
    {
        /// <summary>
        /// A transition modifier for global transitions (Range: 0-1)
        /// </summary>
        public float globalTransitionIn = 0;
        /// <summary>
        /// A transition modifier for global transitions (Range: 1-0)
        /// </summary>
        public float globalTransitionOut = 1;
        /// <summary>
        /// Transparency of a highlighted button (Used in button click animations)
        /// </summary>
        public float transparency = 1;
        /// <summary>
        /// A size modifier for button animations
        /// </summary>
        public int transitionTop = 0;
    }
}