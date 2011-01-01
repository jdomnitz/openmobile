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
using System.ComponentModel;
using OpenMobile.Graphics;
using System.Drawing.Design;
using System.IO;
using System.Collections.Specialized;

namespace OpenMobile
{
    /// <summary>
    /// The rendering mode
    /// </summary>
    public enum eModeType:byte
    {
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
    /// The style list to render
    /// </summary>
    public enum eListStyle:byte
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
        DroidStyleImage=7,
        /// <summary>
        /// A custom text list style with subitem support
        /// </summary>
        MultiListText=8,
        /// <summary>
        /// A custom text list style with subitem support
        /// </summary>
        MultiList=9
    }
    /// <summary>
    /// An item in an OMList
    /// </summary>
    public sealed class OMListItem:IComparable
    {
        /// <summary>
        /// Format information for a list subitem
        /// </summary>
        public sealed class subItemFormat
        {
            /// <summary>
            /// Text Formatting
            /// </summary>
            public OpenMobile.Graphics.eTextFormat textFormat = OpenMobile.Graphics.eTextFormat.Normal;
            /// <summary>
            /// The text alignment
            /// </summary>
            public OpenMobile.Graphics.Alignment textAlignment = OpenMobile.Graphics.Alignment.BottomLeft;
            /// <summary>
            /// The ForeColor
            /// </summary>
            public Color color = Color.White;
            /// <summary>
            /// The color when highlighted
            /// </summary>
            public Color highlightColor = Color.White;
            /// <summary>
            /// The Text Font
            /// </summary>
            public Font font = new Font(Font.GenericSansSerif, 18F);
            /// <summary>
            /// The outline/secondary color
            /// </summary>
            public Color outlineColor = Color.Black;
            /// <summary>
            /// The horizontal ofset of the item's text
            /// </summary>
            public int Offset = 0;
        }
        /// <summary>
        /// The text to display
        /// </summary>
        public string text;
        /// <summary>
        /// Text texture
        /// </summary>
        internal OImage textTex;
        /// <summary>
        /// The icon
        /// </summary>
        public OImage image;
        /// <summary>
        /// An optional subitem for the list
        /// </summary>
        public string subItem;
        /// <summary>
        /// sub item texture
        /// </summary>
        internal OImage subitemTex;
        /// <summary>
        /// An optional subitem format for the list
        /// </summary>
        public subItemFormat subitemFormat;
        /// <summary>
        /// An optional tag for the list item
        /// </summary>
        public object tag; //Added by Borte
        /// <summary>
        /// A tag to sort by (defaults to text if not set)
        /// </summary>
        public string sort;
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
        public OMListItem(string text,OImage image)
        {
            this.text = text;
            this.image = image;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitm"></param>
        public OMListItem(string text, string subitm)
        {
            this.text = text;
            this.subItem=subitm;
            if (this.subItem != null)
                this.subitemFormat = new subItemFormat();
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tag"></param>
        public OMListItem(string text, object tag)//Added by Borte
        {
            this.text = text;
            this.tag = tag;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="tag"></param>
        public OMListItem(string text, string subitem, object tag)//Added by Borte
        {
            this.text = text;
            this.subItem = subitem;
            this.tag = tag;
            if (this.subItem!=null)
                this.subitemFormat = new subItemFormat();
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="image"></param>
        public OMListItem(string text, string subitem, OImage image)//Added by Borte
        {
            this.text = text;
            this.subItem = subitem;
            this.image = image;
            this.subitemFormat = new subItemFormat();
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitm"></param>
        /// <param name="subitemFormat"></param>
        public OMListItem(string text, string subitm, subItemFormat subitemFormat)
        {
            this.text = text;
            this.subItem = subitm;
            this.subitemFormat = subitemFormat;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="tag"></param>
        /// <param name="subitemFormat"></param>
        public OMListItem(string text, string subitem, subItemFormat subitemFormat, object tag)//Added by Borte
        {
            this.text = text;
            this.subItem = subitem;
            this.tag = tag;
            this.subitemFormat = subitemFormat;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="img"></param>
        /// <param name="subitemFormat"></param>
        public OMListItem(string text, string subitem, OImage img, subItemFormat subitemFormat)//Added by Borte
        {
            this.text = text;
            this.subItem = subitem;
            this.image = img;
            this.subitemFormat = subitemFormat;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="img"></param>
        /// <param name="subitemFormat"></param>
        /// <param name="tag"></param>
        public OMListItem(string text, string subitem, OImage img, subItemFormat subitemFormat,object tag)//Added by Borte
        {
            this.text = text;
            this.subItem = subitem;
            this.image = img;
            this.subitemFormat = subitemFormat;
            this.tag = tag;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        /// <param name="img"></param>
        /// <param name="subitemFormat"></param>
        /// <param name="tag"></param>
        /// <param name="sort"></param>
        public OMListItem(string text, string subitem, OImage img, subItemFormat subitemFormat, object tag,string sort)
        {
            this.text = text;
            this.subItem = subitem;
            this.image = img;
            this.subitemFormat = subitemFormat;
            this.tag = tag;
            this.sort = sort;
        }
        #region IComparable Members
        /// <summary>
        /// Compare list items
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            OMListItem two = obj as OMListItem;
            if (this.sort != null)
                return this.text.CompareTo(two.sort);
            return this.text.CompareTo(two.text);
        }

        #endregion
    }
    
    
    /// <summary>
    /// An Open Mobile representation of an image
    /// </summary>
    public struct imageItem
    {
        /// <summary>
        /// The image
        /// </summary>
        public OImage image;
        /// <summary>
        /// The image name in the skin folder
        /// </summary>
        public string name;
        /// <summary>
        /// Construct an image item from an image
        /// </summary>
        /// <param name="i"></param>
        public imageItem(OImage i)
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
        public imageItem(OImage i, string Name)
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
            if (obj is imageItem)
                return (this == (imageItem)obj);
            return false;
        }
        /// <summary>
        /// Serves as a hash function for a particular type
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return image.GetHashCode();
        }
        /// <summary>
        /// Represents an image that the framework can't find
        /// </summary>
        public static imageItem MISSING = new imageItem("MISSING");
        /// <summary>
        /// Represents an empty image item
        /// </summary>
        public static imageItem NONE = new imageItem();
    }
    /// <summary>
    /// The button click transition
    /// </summary>
    public enum eButtonTransition:byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Box Out Effect
        /// </summary>
        BoxOut = 1,
        /// <summary>
        /// Move forward into the screen
        /// </summary>
        IntoScreen=2
        };

    /// <summary>
    /// The type of transition between panels
    /// </summary>
    public enum eGlobalTransition:short
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
        SlideRight = 5,
        /// <summary>
        /// Panel A fades out while Panel B fades in (twice as fast as a regular crossfade)
        /// </summary>
        CrossfadeFast=6,
        /// <summary>
        /// Cube Right
        /// </summary>
        CubeRight=7
    }

    /// <summary>
    /// The type of keyboard event that just occured
    /// </summary>
    public enum eKeypressType
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
    public enum eFunction:short
    {
        /// <summary>
        /// Unknown Function or Function Handled
        /// </summary>
        None=0,
        /// <summary>
        /// Rolls back and cancels a transition
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// </summary>
        CancelTransition=1,
        /// <summary>
        /// A data provider has completed an update
        /// <para>---------------------------------------</para>
        /// <para>Arg1: (Optional) Provider Name</para>
        /// </summary>
        dataUpdated=2,
        /// <summary>
        /// Unloads all loaded panels except the UI
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        TransitionFromAny=3,
        /// <summary>
        /// Occurs each time the hour changed
        /// </summary>
        hourChanged=4,
        /// <summary>
        /// Load a panel and prepare to transition to it
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionToPanel=5,
        /// <summary>
        /// Occurs at the start of a new day
        /// </summary>
        dateChanged=6,
        /// <summary>
        /// Transition between the previously specified panels
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Transition Name(Default: Crossfade)</para>
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
        //Media Events
        /// <summary>
        /// Occurs when openMobile is unable to playback a media location
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Location</para>
        /// </summary>
        playbackFailed = 9,
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
        /// <para>Arg2: Position [Seconds]</para>
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
        setPlayerVolume=15,
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
        /// <para>Arg2: Instance</para>
        /// </summary>
        systemVolumeChanged=22,
        /// <summary>
        /// The playlist has been modified
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance</para>
        /// </summary>
        playlistChanged=23,
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
        /// <para>Arg1: (Optional) Network UID</para>
        /// </summary>
        connectToInternet=25,
        /// <summary>
        /// Disconnect from the internet
        /// <para>-----------------------------</para>
        /// <para>Arg1: (Optional) Network UID</para>
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
        /// <para>Note: -2 May be used as unmute</para>
        /// <para>Arg2: (Optional) Instance</para>
        /// </summary>
        setSystemVolume=34,
        /// <summary>
        /// Ejects a CD/DVD/Blu-Ray disc
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Drive Path</para>
        /// </summary>
        ejectDisc=35,
        /// <summary>
        /// Hide the video rendering window
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        hideVideoWindow=36,
        /// <summary>
        /// Show the video rendering window
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        showVideoWindow=37,
        /// <summary>
        /// Blocks the GoBack function from executing...useful for notifications
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        blockGoBack=38,
        /// <summary>
        /// Unblocks the GoBack function from executing
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        unblockGoBack=39,
        /// <summary>
        /// Sets the brightness of the given screen.
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Brightness [1-100] [0=Monitor Off]</para>
        /// </summary>
        setMonitorBrightness=40,
        /// <summary>
        /// Occurs when a TunedContent plugin updates its station list
        /// </summary>
        stationListUpdated=41,
        /// <summary>
        /// Sets the system volume balance
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Balance [0-100] [0=Left,50=Even]</para>
        /// </summary>
        setSystemBalance=42,
        /// <summary>
        /// Restart this application
        /// </summary>
        restartProgram=43,
        /// <summary>
        /// Close this program
        /// </summary>
        closeProgram=44,
        /// <summary>
        /// Hibernate the computer
        /// </summary>
        hibernate=45,
        /// <summary>
        /// Shutdown the computer
        /// </summary>
        shutdown=46,
        /// <summary>
        /// Restart the computer
        /// </summary>
        restart=47,
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
        loadSpeechContext=49,
        /// <summary>
        /// Unload a speech recognition context
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Context Name</para>
        /// </summary>
        unloadSpeechContext = 50,
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
        /// Stop listening for speech commands
        /// Also occurs as an event when speech recognition times out
        /// </summary>
        stopListeningForSpeech=53,
        /// <summary>
        /// Speak the indicated text
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Text To Speak</para>
        /// </summary>
        Speak=54,
        /// <summary>
        /// Stop speaking and purge all queued speech from the buffer
        /// </summary>
        StopSpeaking=55,
        /// <summary>
        /// Occurs when the rendering window is resized (useful for embedded forms and video windows)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        RenderingWindowResized=56,
        /// <summary>
        /// Occurs when the play order is changed to/from Random
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: "Enabled"/"Disabled"</para>
        /// </summary>
        RandomChanged=57,
        /// <summary>
        /// Occurs when siganl strength, status, # of channels, etc changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        tunerDataUpdated=58,
        /// <summary>
        /// Minimize the rendering window
        /// </summary>
        minimize=59,
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
        /// Clear the panel (go back) history
        /// </summary>
        clearHistory=65,
        /// <summary>
        /// Set subwoofer channel volume
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Volume</para>
        /// </summary>
        setSubVolume=66,
        /// <summary>
        /// Sends a keypress to the target UI window
        /// Possible keys: Up, Down, Left, Right, Enter, ScrollUp, ScrollDown
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Key</para>
        /// </summary>
        sendKeyPress=67,
        /// <summary>
        /// Occurs when a new monitor is detected by the system
        /// </summary>
        screenAdded=68,
        /// <summary>
        /// Occurs when a monitor is removed from the system
        /// </summary>
        screenRemoved=69,
        /// <summary>
        /// Set the Band of a Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: tunedContentBand</para>
        /// </summary>
        setBand=70,
        /// <summary>
        /// Manually sets the instance for a given zone
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Instance Number</para>
        /// </summary>
        impersonateInstanceForScreen=71,
        /// <summary>
        /// Alerts skins that the screen orientation has changed
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Orientation ["Landscape"/"Portrait"]</para>
        /// </summary>
        screenOrientationChanged=72,
        /// <summary>
        /// Go back to the previous panel
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) TransitionType</para>
        /// </summary>
        goBack=80,
        /// <summary>
        /// Occurs when important settings have changed (used to trigger a settings refresh)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: (Optional) Type (Plugin Specific)</para>
        /// </summary>
        settingsChanged=81,
        /// <summary>
        /// Used to manually change the current position in a playlist
        /// <para>---------------------------------------</para>
        /// <para>Arg1: The instance [int]</para>
        /// <para>Arg2: The position [int]</para>
        /// </summary>
        setPlaylistPosition=82,
        /// <summary>
        /// Scan complete band (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        scanBand = 83,
        /// <summary>
        /// Load a plugin from the given path
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Path</para>
        /// </summary>
        loadPlugin=84,
        /// <summary>
        /// Occurs when the vehicle is in motion
        /// </summary>
        vehicleMoving=85,
        /// <summary>
        /// Occurs when a vehicle comes to a complete stop
        /// </summary>
        vehicleStopped=86,
        /// <summary>
        /// Occurs when the VideoPosition property changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance</para>
        /// </summary>
        videoAreaChanged=87,
        /// <summary>
        /// A gesture has been recognized
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// <para>Arg2: Character</para>
        /// <para>Arg3: Plugin (The name of the plugin with the top most panel)</para>
        /// </summary>
        gesture=100,
        /// <summary>
        /// A multi-touch gesture has been recognized
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// <para>Arg2: Gesture</para>
        /// <para>---Possible Gestures:---</para>
        /// <para>Pan|Distance Between Fingers|Current Position</para>
        /// <para>Zoom|Distance Between Fingers|Center of gesture</para>
        /// <para>Rotate|Radians|Center of gesture</para>
        /// <para>----Gesture Complete:---</para>
        /// <para>EndPan|Distance Between Fingers|Current Position</para>
        /// <para>EndZoom|Distance Between Fingers|Center of gesture</para>
        /// <para>EndRotate|Radians|Center of gesture</para>
        /// <para>Arg3: Plugin (The name of the plugin with the top most panel)</para>
        /// </summary>
        multiTouchGesture=101,
        /// <summary>
        /// Instructs the navigation engine to construct a route to the given address
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Address</para>
        /// <para>Arg2: (Optional) Routing Type (Plugin Specific)</para>
        /// </summary>
        navigateToAddress=200,
        /// <summary>
        /// Instructs the navigation engine to construct a route to the given point of interest
        /// <para>---------------------------------------</para>
        /// <para>Arg1: POI Name</para>
        /// <para>Arg2: (Optional) location to search from (default is current)</para>
        /// </summary>
        navigateToPOI=201,
        ///// <summary>
        ///// Occurs when the navigation engine calculates a route
        ///// </summary>
        //routeCalculated=202,
        /// <summary>
        /// Occurs when a turn is approaching (distince dependent on road type)
        /// </summary>
        turnApproaching=203,
        /// <summary>
        /// Shows the requested nav page (plugin specific)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Nav Page</para>
        /// </summary>
        showNavPanel=204,
        /// <summary>
        /// Dials the given number
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Phone Number (without seperators)</para>
        /// <para>Arg2: (Optional) Display Name</para>
        /// </summary>
        dialNumber=300,
        /// <summary>
        /// Prompts the user to dial the given number
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Phone Number (without seperators)</para>
        /// <para>Arg2: (Optional) Display Name</para>
        /// </summary>
        promptDialNumber=301
        
    }
    /// <summary>
    /// The status of a plugins initialization
    /// </summary>
    public enum eLoadStatus:byte
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
        /// Load failed but this was not the result of a crash.  Plugin will be unloaded.
        /// </summary>
        LoadFailedGracefulUnloadRequested=3,
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
    public enum eWirelessEvent:byte
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
        /// Connecting to a wireless connection
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Network Name</para>
        /// </summary>
        ConnectingToWirelessNetwork=2,
        /// <summary>
        /// A successful connection has been established to a wireless connection
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Network Name</para>
        /// </summary>
        ConnectedToWirelessNetwork=3,
        /// <summary>
        /// Wireless (wifi) signal strength has changed
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Signal Value (0-100)</para>
        /// </summary>
        WirelessSignalStrengthChanged=4,
        /// <summary>
        /// The network connection has been established but an internet connection
        /// requires Access Point credentials be entered in the web browser
        /// </summary>
        WirelessNetworkRequiresLogin=5,
        /// <summary>
        /// Disconnected from the wireless network
        /// </summary>
        DisconnectedFromWirelessNetwork=6,
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
    public enum eMediaType:short
    {
        /// <summary>
        /// A device has been removed
        /// </summary>
        DeviceRemoved=-1,
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
        RTSPUrl	=	9,
        /// <summary>
        /// A YouTube video
        /// </summary>
        YouTube	=	10,
        /// <summary>
        /// An iPod
        /// </summary>
        AppleDevice	=	11,
        /// <summary>
        /// A bluetooth audio device
        /// </summary>
        BluetoothResource	=	12,
        /// <summary>
        /// An openDrive device
        /// </summary>
        OpenDrive 	=	13,
        /// <summary>
        /// Digital Camera
        /// </summary>
        Camera	=	14,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved	=	15,
        /// <summary>
        /// Multimedia Streaming Protocol
        /// </summary>
        MMSUrl=16,
        /// <summary>
        /// Radio
        /// </summary>
        Radio=17,
        /// <summary>
        /// Internet Radio
        /// </summary>
        InternetRadio=18,
        /// <summary>
        /// One or more live camera streams
        /// </summary>
        LiveCamera=19,
        /// <summary>
        /// Smartphone
        /// </summary>
        Smartphone=20,
        /// <summary>
        /// Other
        /// </summary>
        Other	=	25
    }
    /// <summary>
    /// Textbox Options
    /// </summary>
    [Flags]
    public enum textboxFlags:byte
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
    public enum backgroundStyle:byte
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
    public enum ePriority:byte
    {
        /// <summary>
        /// Low Priority
        /// </summary>
        Low=0,
        /// <summary>
        /// Medium-Low Priority
        /// </summary>
        MediumLow=1,
        /// <summary>
        /// Normal Priority
        /// </summary>
        Normal=2,
        /// <summary>
        /// Medium-High Priority
        /// </summary>
        MediumHigh=3,
        /// <summary>
        /// High Priority
        /// </summary>
        High=4,
        /// <summary>
        /// Urgent Priority
        /// </summary>
        Urgent=5
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
        /// Get the position (in seconds) of the currently playing media [float]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaPosition=2,
        /// <summary>
        /// Gets the volume (0-100) [int]
        /// </summary>
        GetSystemVolume=3,
        /// <summary>
        /// Gets an array of availble networks [string[]]
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
        /// Gets the status of the currently loaded media player [ePlayerStatus]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaStatus=8,
        /// <summary>
        /// Returns an IMediaDatabase object
        /// </summary>
        GetMediaDatabase=9,
        /// <summary>
        /// Returns the loaded plugin collection. [List(IBasePlugin)]
        /// </summary>
        GetPlugins=10,
        /// <summary>
        /// Get the playback speed [float] of the currently playing media
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetPlaybackSpeed=11,
        /// <summary>
        /// Returns a list of audio devices [string[]]available on the system
        /// </summary>
        GetAudioDevices=12,
        /// <summary>
        /// Gets the height and width scale factors [Point]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        GetScaleFactors=13,
        /// <summary>
        /// Returns the OMControl that draws a map
        /// </summary>
        GetMap=14,
        /// <summary>
        /// Returns a string[] listing available navigation panels
        /// </summary>
        GetAvailableNavPanels=15,
        /// <summary>
        /// Returns the current GPS position [Position].  May be 0,0 if no GPS signal is available.
        /// </summary>
        GetCurrentPosition=16,
        /// <summary>
        /// Returns the nearest Address [Location]
        /// </summary>
        GetNearestAddress=17,
        /// <summary>
        /// Returns the route destination [Location].  Will return null if no route calculated
        /// </summary>
        GetDestination=18,
        /// <summary>
        /// Returns a [string[]] containing all of the available skins
        /// </summary>
        GetAvailableSkins=19,
        /// <summary>
        /// Gets station list from the currently loaded tunedcontent plugin
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetStationList = 20,
        /// <summary>
        /// Gets a list of supported bands from the currently loaded tunedcontent plugin
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetSupportedBands = 21,
        /// <summary>
        /// Gets the volume from a tuner or av player [0-100]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetPlayerVolume=22,
        /// <summary>
        /// Gets the list of available sensors
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Optional Plugin Name [string]</para>
        /// </summary>
        GetAvailableSensors = 23,
        /// <summary>
        /// Gets a list of available keyboards
        /// </summary>
        GetAvailableKeyboards=24,
        /// <summary>
        /// Gets a list of available mice
        /// </summary>
        GetAvailableMice = 25
    }
    /// <summary>
    /// Information on Tuned Content
    /// </summary>
    public sealed class tunedContentInfo
    {
        /// <summary>
        /// Current PowerState (True = powered on, false = powered off)
        /// </summary>
        public bool powerState;
        /// <summary>
        /// Current audio mode
        /// </summary>
        public int channels;
        /// <summary>
        /// Current band
        /// </summary>
        public eTunedContentBand band;
        /// <summary>
        /// Status
        /// </summary>
        public eTunedContentStatus status;
        /// <summary>
        /// List of current available stations
        /// </summary>
        public stationInfo[] stationList;
        /// <summary>
        /// Info for currently tuned station
        /// </summary>
        public stationInfo currentStation;
    }
    /// <summary>
    /// The current status of the Tuned Content Plugin
    /// </summary>
    public enum eTunedContentStatus:byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Scanning for a station
        /// </summary>
        Scanning = 1,
        /// <summary>
        /// Tuned to a station
        /// </summary>
        Tuned = 2,
        /// <summary>
        /// No service / no signal
        /// </summary>
        NoSignal = 3,
        /// <summary>
        /// Powering Up/Initializing
        /// </summary>
        Initializing=4,
        /// <summary>
        /// Powered Off
        /// </summary>
        PoweredOff=5,
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
    /// Type of Tuned Content
    /// </summary>
    public enum eTunedContentBand:byte
    {
        /// <summary>
        /// None or Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// AM
        /// </summary>
        AM = 1,
        /// <summary>
        /// FM
        /// </summary>
        FM = 2,
        /// <summary>
        /// DAB 
        /// </summary>
        DAB = 3,
        /// <summary>
        /// HD Radio
        /// </summary>
        HD=4,
        /// <summary>
        /// Satellite Radio
        /// </summary>
        XM=5,
        /// <summary>
        /// Over the air TV
        /// </summary>
        OTATV=6,
        /// <summary>
        /// Everything else
        /// </summary>
        Other=7,
        /// <summary>
        /// Internet Radio
        /// </summary>
        Internet=8,
        /// <summary>
        /// Satellite Radio
        /// </summary>
        Sirius=9
    }
#pragma warning disable 0659
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
        /// Retrieves the signal strength of the network connection. Strength should be represented as positive numbers where higher is stronger. Range is plugin specific (0-x). Returns 0 for not applicable.
        /// </summary>
        public uint signalStrength;
        /// <summary>
        /// The connection is currently connected
        /// </summary>
        public bool IsConnected;
        /// <summary>
        /// If true, credentials must be set before connecting
        /// </summary>
        public ePassType requiresPassword;
        /// <summary>
        /// Username/Password or just Password
        /// </summary>
        public string Credentials;
        /// <summary>
        /// Plugin Specific (describes the connection)
        /// </summary>
        public string ConnectionType="";
        /// <summary>
        /// Creates a new connection info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="signal"></param>
        /// <param name="passwordRequired"></param>
        /// <param name="type"></param>
        public connectionInfo(string name, string id, int speed, uint signal,string type,ePassType passwordRequired)
        {
            NetworkName = name;
            UID = id;
            potentialSpeed = speed;
            signalStrength = signal;
            ConnectionType = type;
            requiresPassword = passwordRequired;
        }
        /// <summary>
        /// Compare connection info by UID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            connectionInfo tmp = obj as connectionInfo;
            if (tmp == null)
                return false;
            return (tmp.UID == this.UID);
        }
    }
    /// <summary>
    /// Password Type
    /// </summary>
    public enum ePassType:byte
    {
        /// <summary>
        /// None
        /// </summary>
        None=0,
        /// <summary>
        /// Single string shared key
        /// </summary>
        SharedKey=1,
        /// <summary>
        /// Username and Password
        /// </summary>
        UserPass=2,
        /// <summary>
        /// Username, Password and Domain
        /// </summary>
        UserPassDomain=3
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
        /// <para>---------------------------------------</para>
        /// <para>Arg: No GPS, No Signal, 2D Fix, 3D Fix</para>
        /// </summary>
        GPSStatusChange=1,
        /// <summary>
        /// Route has changed
        /// <para>---------------------------------------</para>
        /// <para>Arg: Navigation Started, Navigation Cancelled, Waypoint Added, Detouring</para>
        /// </summary>
        RouteChanged=2,
        /// <summary>
        /// A turn is approaching...query the nextTurn parameter for more info
        /// </summary>
        TurnApproaching=3,
        /// <summary>
        /// Occurs when the current town/city changes.  Useful for refreshing data for the new location
        /// <para>---------------------------------------</para>
        /// <para>Arg: City/Town Name</para>
        /// </summary>
        LocationChanged=4
    }
    /// <summary>
    /// Represents a GPS Position
    /// </summary>
    public struct Position
    {
        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude;
        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude;
        /// <summary>
        /// Altitude
        /// </summary>
        public double Altitude;
        /// <summary>
        /// Creates a new GPS Position
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        public Position(double lat, double lon, double alt)
        {
            Longitude = lon;
            Latitude = lat;
            Altitude = alt;
        }
    }

    /// <summary>
    /// Represents a Navigation Address
    /// </summary>
    public class Location:ICloneable
    {
        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name;
        /// <summary>
        /// The street portion of an address
        /// </summary>
        public string Street;
        /// <summary>
        /// The city portion of an address
        /// </summary>
        public string City;
        /// <summary>
        /// The state portion of an address
        /// </summary>
        public string State;
        /// <summary>
        /// The country portion of an address
        /// </summary>
        public string Country;
        /// <summary>
        /// The zip/postcode portion of an address
        /// </summary>
        public string Zip;
        /// <summary>
        /// Latitude
        /// </summary>
        public float Latitude;
        /// <summary>
        /// Longitude
        /// </summary>
        public float Longitude;
        /// <summary>
        /// Creates a new Address
        /// </summary>
        public Location()
        {
            Name=String.Empty;
            Street = String.Empty;
            City = String.Empty;
            State = String.Empty;
            Country = String.Empty;
            Zip = String.Empty;
        }
        /// <summary>
        /// Creates a new Address from the given string
        /// </summary>
        /// <param name="address"></param>
        public Location(string address)
        {
            Location ret;
            if (TryParse(address, out ret) == true)
            {
                Street = ret.Street;
                City = ret.City;
                State = ret.State;
            }
            else
            {
                Street = String.Empty;
                City = String.Empty;
                State = String.Empty;
            }
            Country = String.Empty;
            Zip = String.Empty;
        }
        /// <summary>
        /// Creates a new Address for the given coordinates
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public Location(float lat, float lng)
        {
            Longitude = lng;
            Latitude = lat;
        }
        /// <summary>
        /// Creates a new Address
        /// </summary>
        /// <param name="street"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        public Location(string street, string city, string state)
        {
            Street = street;
            City = city;
            State = state;
            Country = String.Empty;
            Zip = String.Empty;
        }
        /// <summary>
        /// Creates a new Address
        /// </summary>
        /// <param name="street"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="zip"></param>
        /// <param name="country"></param>
        public Location(string street, string city, string state,string zip,string country)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            Zip = zip;
        }
        /// <summary>
        /// Trys to parse a string into an Address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string address, out Location result)
        {
            if (address == null)
            {
                result = null;
                return false;
            }
            result = null;
            string[] args = address.Split(new char[] { ',','\n' },StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 3)
            {
                result = new Location(args[0], args[1].Trim(), args[2].Trim());
                return true;
            }
            else if (args.Length == 2)
            {
                float val1;
                float val2;
                if (float.TryParse(args[0], out val1) == true)
                    if (float.TryParse(args[1], out val2) == true)
                    {
                        result = new Location(val1, val2);
                        return true;
                    }
            }
            return false;
        }
        /// <summary>
        /// Creates a copy of this Address
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Location(String.Copy(Street), String.Copy(City), String.Copy(State), String.Copy(Zip), String.Copy(Country));
        }
        /// <summary>
        /// Converts an Address to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Street) && (Latitude != 0))
                return Latitude.ToString() + "," + Longitude.ToString();
            return Street+"\n"+City+", "+State;
        }
    }
    /// <summary>
    /// A/V Player statue
    /// </summary>
    public enum ePlayerStatus:byte
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
            if (part3 == null)
                return Combine(part1,part2);
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
        /// <summary>
        /// Gets the relative path to a file
        /// </summary>
        /// <param name="fromDirectory"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        public static string getRelativePath(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = System.IO.Path.IsPathRooted(fromDirectory)
                && System.IO.Path.IsPathRooted(toPath);

            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(
                    System.IO.Path.GetPathRoot(fromDirectory),
                    System.IO.Path.GetPathRoot(toPath), true) != 0;

                if (isDifferentRoot)
                    return toPath;
            }

            StringCollection relativePath = new StringCollection();
            string[] fromDirectories = fromDirectory.Split(
                System.IO.Path.DirectorySeparatorChar);

            string[] toDirectories = toPath.Split(
                System.IO.Path.DirectorySeparatorChar);

            int length = System.Math.Min(
                fromDirectories.Length,
                toDirectories.Length);

            int lastCommonRoot = -1;

            // find common root
            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x],
                    toDirectories[x], true) != 0)
                    break;

                lastCommonRoot = x;
            }
            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
                relativePath.Add(toDirectories[x]);

            // create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join(
                System.IO.Path.DirectorySeparatorChar.ToString(),
                relativeParts);

            return newPath;
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
        /// Lyrics
        /// </summary>
        public string Lyrics;
        /// <summary>
        /// Length in seconds
        /// </summary>
        public int Length=-1;
        /// <summary>
        /// Rating 0-5.  -1 for not set.
        /// </summary>
        public int Rating=-1;
        /// <summary>
        /// Optional - The cover art for the selected media
        /// </summary>
        public OImage coverArt;
        /// <summary>
        /// Source Type
        /// </summary>
        public eMediaType Type;
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
        /// Station Number (aka frequency or channel or station)
        /// </summary>
        public string stationID;
        /// <summary>
        /// Value: 0(no signal) - 5(full signal)
        /// </summary>
        public int signal;
        /// <summary>
        /// Channel is broadcast in HD
        /// </summary>
        public bool HD;
        /// <summary>
        /// Station Genre (Empty String if unknown)
        /// </summary>
        public string stationGenre;
        /// <summary>
        /// Number of Audio Channels
        /// </summary>
        public int Channels;
        /// <summary>
        /// Bitrate (bits/second)
        /// </summary>
        public int Bitrate;
        /// <summary>
        /// Creates a new stationInfo
        /// </summary>
        public stationInfo() { }
        /// <summary>
        /// Creates a new stationInfo
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        public stationInfo(string name, string id)
        {
            stationName = name;
            stationID = id;
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
        /// <summary>
        /// The current rendering mode of the rendering window
        /// </summary>
        public eModeType currentMode=eModeType.Normal;
    }
}
