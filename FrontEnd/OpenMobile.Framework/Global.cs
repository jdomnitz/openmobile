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
using OpenMobile.Math;

namespace OpenMobile
{
    /// <summary>
    /// The rendering mode
    /// </summary>
    public enum eModeType : byte
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
        ClickedAndTransitioningOut = 3,
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
        transitionLock = 9, //When the same control is loaded and unloaded - do nothing
        /// <summary>
        /// A gesture is being drawn
        /// </summary>
        gesturing = 10,
        /// <summary>
        /// About to be unloaded after transitioning out
        /// </summary>
        Unloaded = 11,
        /// <summary>
        /// Loaded and ready for transitioning in
        /// </summary>
        Loaded = 12
    };
    /// <summary>
    /// The style list to render
    /// </summary>
    public enum eListStyle : byte
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
        TransparentTextList = 4,
        /// <summary>
        /// A text and image list with a transparent background
        /// </summary>
        TransparentImageList = 5,
        /// <summary>
        /// A custom text list style
        /// </summary>
        DroidStyleText = 6,
        /// <summary>
        /// A custom text and image style
        /// </summary>
        DroidStyleImage = 7,
        /// <summary>
        /// A custom text list style with subitem support
        /// </summary>
        MultiListText = 8,
        /// <summary>
        /// A custom text list style with subitem support
        /// </summary>
        MultiList = 9
    }
    /// <summary>
    /// An item in an OMList
    /// </summary>
    public sealed class OMListItem : IComparable, ICloneable
    {
        /// <summary>
        /// Format information for a list subitem
        /// </summary>
        public sealed class subItemFormat : ICloneable
        {
            public bool Changed { get; set; }

            public bool RefreshGraphic = true;

            /// <summary>
            /// Text Formatting
            /// </summary>
            public OpenMobile.Graphics.eTextFormat textFormat
            {
                get { return _textFormat; }
                set
                {
                    _textFormat = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private OpenMobile.Graphics.eTextFormat _textFormat = OpenMobile.Graphics.eTextFormat.Normal;

            /// <summary>
            /// The text alignment
            /// </summary>
            public OpenMobile.Graphics.Alignment textAlignment
            {
                get { return _textAlignment; }
                set
                {
                    _textAlignment = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private OpenMobile.Graphics.Alignment _textAlignment = OpenMobile.Graphics.Alignment.BottomLeft;
            /// <summary>
            /// The ForeColor
            /// </summary>
            public Color color
            {
                get { return _color; }
                set
                {
                    _color = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private Color _color = Color.White;
            /// <summary>
            /// The color when highlighted
            /// </summary>
            public Color highlightColor
            {
                get { return _highlightColor; }
                set
                {
                    _highlightColor = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private Color _highlightColor = Color.White;
            /// <summary>
            /// The Text Font
            /// </summary>
            public Font font
            {
                get { return _font; }
                set
                {
                    _font = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private Font _font = new Font(Font.GenericSansSerif, 18F);
            /// <summary>
            /// The outline/secondary color
            /// </summary>
            public Color outlineColor
            {
                get { return _outlineColor; }
                set
                {
                    _outlineColor = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private Color _outlineColor = Color.Black;
            /// <summary>
            /// The horizontal ofset of the item's text
            /// </summary>
            public int Offset
            {
                get { return _Offset; }
                set
                {
                    _Offset = value;
                    //subitemTex = null;
                    Changed = true;
                    RefreshGraphic = true;
                }
            }
            private int _Offset = 0;

            #region ICloneable Members

            public object Clone()
            {
                subItemFormat ret = (subItemFormat)this.MemberwiseClone();
                ret.RefreshGraphic = true;
                return ret;
            }

            #endregion
        }

        public bool RefreshGraphic = true;

        /// <summary>
        /// The text to display
        /// </summary>
        public string text
        {
            get { return _text; }
            set
            {
                _text = value;
                RefreshGraphic = true;
            }
        }
        private string _text;

        private OImage _textTex;
        /// <summary>
        /// Text texture
        /// </summary>
        internal OImage textTex
        {
            get { return _textTex; }
            set
            {
                _textTex = value;
                RefreshGraphic = true;
            }
        }

        private OImage _subitemTex;
        /// <summary>
        /// sub item texture
        /// </summary>
        internal OImage subitemTex
        {
            get { return _subitemTex; }
            set
            {
                _subitemTex = value;
                RefreshGraphic = true;
            }
        }
        /// <summary>
        /// The icon
        /// </summary>
        public OImage image
        {
            get { return _image; }
            set
            {
                _image = value;
                RefreshGraphic = true;
            }
        }
        private OImage _image;
        /// <summary>
        /// An optional subitem for the list
        /// </summary>
        public string subItem
        {
            get { return _subItem; }
            set
            {
                _subItem = value;
                RefreshGraphic = true;
            }
        }
        private string _subItem;
        /// <summary>
        /// An optional subitem format for the list
        /// </summary>
        public subItemFormat subitemFormat
        {
            get { return _subitemFormat; }
            set
            {
                _subitemFormat = value;
                RefreshGraphic = true;
            }
        }
        private subItemFormat _subitemFormat;
        /// <summary>
        /// An optional tag for the list item
        /// </summary>
        public object tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                RefreshGraphic = true;
            }
        }
        private object _tag; 
        /// <summary>
        /// A tag to sort by (defaults to text if not set)
        /// </summary>
        public string sort
        {
            get { return _sort; }
            set
            {
                _sort = value;
                RefreshGraphic = true;
            }
        }
        private string _sort;
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
        public OMListItem(string text, OImage image)
        {
            this.text = text;
            this.image = image;
        }
        /// <summary>
        /// Creates a new list item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="subitem"></param>
        public OMListItem(string text, string subitem)
        {
            this.text = text;
            this.subItem = subitem;
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
            if (this.subItem != null)
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
        public OMListItem(string text, string subitem, OImage img, subItemFormat subitemFormat, object tag)//Added by Borte
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
        public OMListItem(string text, string subitem, OImage img, subItemFormat subitemFormat, object tag, string sort)
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

        #region ICloneable Members

        public object Clone()
        {
            OMListItem NewItem = (OMListItem)this.MemberwiseClone();
            NewItem.RefreshGraphic = true; 
            if (this.subitemFormat != null)
                NewItem.subitemFormat = (subItemFormat)this.subitemFormat.Clone();
            return NewItem;
        }

        #endregion

        public override string ToString()
        {
            return _text;
        }
    }

    /// <summary>
    /// The button click transition
    /// </summary>
    public enum eButtonTransition : byte
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
        IntoScreen = 2
    };

    /// <summary>
    /// Built in transition effects for panels
    /// </summary>
    public enum eGlobalTransition : short
    {
        /// <summary>
        /// Panel A is unloaded and Panel B is loaded (more efficient then the load and unload functions)
        /// </summary>
        None,
        /// <summary>
        /// Panel A fades out while Panel B fades in
        /// </summary>
        Crossfade,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideUp,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideDown,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideLeft,
        /// <summary>
        /// Panel A slides up and off the screen, Panel B slides up and onto the screen
        /// </summary>
        SlideRight,
        /// <summary>
        /// Panel A fades out while Panel B fades in (twice as fast as a regular crossfade)
        /// </summary>
        CrossfadeFast,
        /// <summary>
        /// Randomly select effect
        /// </summary>
        Random,
        /// <summary>
        /// Collapse panel A while growing panel B
        /// </summary>
        CollapseGrowCrossUL,
        /// <summary>
        /// Collapse panel A while growing panel B
        /// </summary>
        CollapseGrowCrossCenter,
        /// <summary>
        /// Collapse panel A fully then grow panel B
        /// </summary>
        CollapseGrowCenter
    }

    /// <summary>
    /// The type of keyboard event that just occured
    /// </summary>
    public enum eKeypressType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Key was released
        /// </summary>
        KeyUp = 1,
        /// <summary>
        /// Key was pressed
        /// </summary>
        KeyDown = 2
    }

    /// <summary>
    /// Gloabl Functions
    /// </summary>
    public enum eFunction : short
    {
        /// <summary>
        /// Unknown Function or Function Handled
        /// </summary>
        None = 0,
        /// <summary>
        /// [REMOVED] Rolls back and cancels a transition
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// </summary>
        [Obsolete("This function has been removed/disabled")]
        CancelTransition = 1,
        /// <summary>
        /// A data provider has completed an update
        /// <para>---------------------------------------</para>
        /// <para>Arg1: (Optional) Provider Name</para>
        /// </summary>
        dataUpdated = 2,
        /// <summary>
        /// Unloads all loaded panels except the UI
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        TransitionFromAny = 3,
        /// <summary>
        /// Occurs each time the hour changed
        /// </summary>
        hourChanged = 4,
        /// <summary>
        /// Load a panel and prepare to transition to it
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionToPanel = 5,
        /// <summary>
        /// Occurs at the start of a new day
        /// </summary>
        dateChanged = 6,
        /// <summary>
        /// Transition between the previously specified panels
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Transition Name(Default: Crossfade)</para>
        /// </summary>
        ExecuteTransition = 7,
        /// <summary>
        /// The panel to transition from
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) Plugin Name</para>
        /// <para>Arg3: (Optional) Panel Name</para>
        /// </summary>
        TransitionFromPanel = 8,
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
        Play = 10,
        /// <summary>
        /// Pause the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        Pause = 11,
        /// <summary>
        /// Stop the currently playing media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        Stop = 12,
        /// <summary>
        /// Set the playback position of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Position [Seconds]</para>
        /// </summary>
        setPosition = 13,
        /// <summary>
        /// Set the playback speed of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Speed [Float]</para>
        /// </summary>
        setPlaybackSpeed = 14,
        /// <summary>
        /// Set the playback volume of the current media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Volume [Int 0 to 100]</para>
        /// <para>Note: -1 should mute, -2 should unmute</para>
        /// </summary>
        setPlayerVolume = 15,
        /// <summary>
        /// Play the next media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        nextMedia = 16,
        /// <summary>
        /// Play the previous media
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        previousMedia = 17,
        /// <summary>
        /// Load an A/V Player plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// </summary>
        loadAVPlayer = 18,
        /// <summary>
        /// Load a Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// </summary>
        loadTunedContent = 19,
        /// <summary>
        /// Unload the current AV Player plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        unloadAVPlayer = 20,
        /// <summary>
        /// Unload the current Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        unloadTunedContent = 21,
        /// <summary>
        /// Occurs whenever the system volume changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Volume [int -1(mute) to 100]</para>
        /// <para>Arg2: Instance</para>
        /// </summary>
        systemVolumeChanged = 22,
        /// <summary>
        /// The playlist has been modified
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance</para>
        /// </summary>
        playlistChanged = 23,
        //Network
        /// <summary>
        /// Connect to the internet
        /// <para>-----------------------------</para>
        /// <para>Arg1: (Optional) Network UID</para>
        /// </summary>
        connectToInternet = 25,
        /// <summary>
        /// Disconnect from the internet
        /// <para>-----------------------------</para>
        /// <para>Arg1: (Optional) Network UID</para>
        /// </summary>
        disconnectFromInternet = 26,
        /// <summary>
        /// Occurs when a valid internet connection is detected
        /// </summary>
        connectedToInternet = 27,
        /// <summary>
        /// Occurs when an internet connection is disconnected/lost
        /// </summary>
        disconnectedFromInternet = 28,
        /// <summary>
        /// Occurs when network connections are available
        /// </summary>
        networkConnectionsAvailable = 29,
        //Miscellaneous
        /// <summary>
        /// Reset a hardware device
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Plugin Name</para>
        /// <para>Arg2: (Optional) Device Name</para>
        /// </summary>
        resetDevice = 30,
        /// <summary>
        /// Plugin specific. Triggered as a system event.
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Plugin Name</para>
        /// <para>Arg3: Data</para>
        /// </summary>
        userInputReady = 31,
        /// <summary>
        /// Raised when the core has finished loading and initializing all plugins
        /// </summary>
        pluginLoadingComplete = 32,
        /// <summary>
        /// A status update on a background operation (Event usage only)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Message</para>
        /// <para>Arg2: Source ("PluginName" or "PluginName.Tag" or "Tag")</para>
        /// <para>Arg3: Type of data (eDataType)</para>
        /// </summary>
        backgroundOperationStatus = 33,
        /// <summary>
        /// Sets the system volume
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Volume [int -1(mute) to 100]</para>
        /// <para>Note: -2 May be used as unmute</para>
        /// <para>Arg2: Screen number or zone ID [int]</para>
        /// </summary>
        setSystemVolume = 34,
        /// <summary>
        /// Ejects a CD/DVD/Blu-Ray disc
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Drive Path</para>
        /// </summary>
        ejectDisc = 35,
        /// <summary>
        /// Hide the video rendering window
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        hideVideoWindow = 36,
        /// <summary>
        /// Show the video rendering window
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        showVideoWindow = 37,
        /// <summary>
        /// Blocks the GoBack function from executing...useful for notifications
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        blockGoBack = 38,
        /// <summary>
        /// Unblocks the GoBack function from executing
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        unblockGoBack = 39,
        /// <summary>
        /// Sets the brightness of the given screen.
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Brightness [1-100] [0=Monitor Off]</para>
        /// </summary>
        setMonitorBrightness = 40,
        /// <summary>
        /// Occurs when a TunedContent plugin updates its station list
        /// </summary>
        stationListUpdated = 41,
        /// <summary>
        /// Sets the system volume balance
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Balance [0-100] [0=Left,50=Even]</para>
        /// </summary>
        setSystemBalance = 42,
        /// <summary>
        /// Restart this application
        /// </summary>
        restartProgram = 43,
        /// <summary>
        /// Close this program
        /// </summary>
        closeProgram = 44,
        /// <summary>
        /// Hibernate the computer
        /// </summary>
        hibernate = 45,
        /// <summary>
        /// Shutdown the computer
        /// </summary>
        shutdown = 46,
        /// <summary>
        /// Restart the computer
        /// </summary>
        restart = 47,
        /// <summary>
        /// Force the computer to enter low power mode
        /// </summary>
        standby = 48,
        //SpeechRecognition
        /// <summary>
        /// Load a speech recognition context
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Context Name</para>
        /// </summary>
        loadSpeechContext = 49,
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
        addSpeechContext = 51,
        /// <summary>
        /// Listen for a speech command
        /// </summary>
        listenForSpeech = 52,
        /// <summary>
        /// Stop listening for speech commands
        /// Also occurs as an event when speech recognition times out
        /// </summary>
        stopListeningForSpeech = 53,
        /// <summary>
        /// Speak the indicated text
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Text To Speak</para>
        /// </summary>
        Speak = 54,
        /// <summary>
        /// Stop speaking and purge all queued speech from the buffer
        /// </summary>
        StopSpeaking = 55,
        /// <summary>
        /// Occurs when the rendering window is resized (useful for embedded forms and video windows)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Point)Location of window on desktop</para>
        /// <para>Arg3: (Size)Size of window</para>
        /// <para>Arg4: (PointF)Scalefactors</para>
        /// </summary>
        RenderingWindowResized = 56,
        /// <summary>
        /// Occurs when the play order is changed to/from Random
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: "Enabled"/"Disabled"</para>
        /// </summary>
        RandomChanged = 57,
        /// <summary>
        /// Occurs when siganl strength, status, # of channels, etc changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        tunerDataUpdated = 58,
        /// <summary>
        /// Minimize the rendering window
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        minimize = 59,
        //Tuned Content
        /// <summary>
        /// Tune to the given statioin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Plugin Specific</para>
        /// </summary>
        tuneTo = 60,
        /// <summary>
        /// Scan forward (Tuned Content)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        scanForward = 61,
        /// <summary>
        /// Scan backward (Tuned Content)
        ///<para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// </summary>
        scanBackward = 62,
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
        clearHistory = 65,
        /// <summary>
        /// Set subwoofer channel volume
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: Volume</para>
        /// </summary>
        setSubVolume = 66,
        /// <summary>
        /// Sends a keypress to the target UI window
        /// Possible keys: Up, Down, Left, Right, Enter, ScrollUp, ScrollDown
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Key</para>
        /// </summary>
        sendKeyPress = 67,
        /// <summary>
        /// Occurs when a new monitor is detected by the system
        /// </summary>
        screenAdded = 68,
        /// <summary>
        /// Occurs when a monitor is removed from the system
        /// </summary>
        screenRemoved = 69,
        /// <summary>
        /// Set the Band of a Tuned Content plugin
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Instance Number</para>
        /// <para>Arg2: tunedContentBand</para>
        /// </summary>
        setBand = 70,
        /// <summary>
        /// Manually sets the instance for a given zone
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Instance Number</para>
        /// </summary>
        impersonateInstanceForScreen = 71,
        /// <summary>
        /// Alerts skins that the screen orientation has changed
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Orientation ["Landscape"/"Portrait"]</para>
        /// </summary>
        screenOrientationChanged = 72,
        /// <summary>
        /// Raised when the inputrouter finished initializing
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Amount of keyboards available</para>
        /// <para>Arg2: Amount of mice available</para>
        /// </summary>
        inputRouterInitialized = 73,
        /// <summary>
        /// Go back to the previous panel
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) TransitionType</para>
        /// </summary>
        goBack = 80,
        /// <summary>
        /// Occurs when important settings have changed (used to trigger a settings refresh)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: (Optional) Type (Plugin Specific)</para>
        /// </summary>
        settingsChanged = 81,
        /// <summary>
        /// Used to manually change the current position in a playlist
        /// <para>---------------------------------------</para>
        /// <para>Arg1: The instance [int]</para>
        /// <para>Arg2: The position [int]</para>
        /// </summary>
        setPlaylistPosition = 82,
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
        loadPlugin = 84,
        /// <summary>
        /// Occurs when the vehicle is in motion
        /// </summary>
        vehicleMoving = 85,
        /// <summary>
        /// Occurs when a vehicle comes to a complete stop
        /// </summary>
        vehicleStopped = 86,
        /// <summary>
        /// Event: Occurs when the VideoPosition property changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// <para>Arg2: Video rectangle as string: X|Y|W|H</para>
        /// </summary>
        videoAreaChanged = 87,
        /// <summary>
        /// A gesture has been recognized
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// <para>Arg2: Character</para>
        /// <para>Arg3: Plugin|Panel (The name of the plugin with the top most panel, Plugin and panel is separated with |)</para>
        /// </summary>
        //gesture = 100,
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
        multiTouchGesture = 101,
        /// <summary>
        /// Instructs the navigation engine to construct a route to the given address
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Address</para>
        /// <para>Arg2: (Optional) Routing Type (Plugin Specific)</para>
        /// </summary>
        navigateToAddress = 200,
        /// <summary>
        /// Instructs the navigation engine to construct a route to the given point of interest
        /// <para>---------------------------------------</para>
        /// <para>Arg1: POI Name</para>
        /// <para>Arg2: (Optional) location to search from (default is current)</para>
        /// </summary>
        navigateToPOI = 201,
        ///// <summary>
        ///// Occurs when the navigation engine calculates a route
        ///// </summary>
        //routeCalculated=202,
        /// <summary>
        /// Occurs when a turn is approaching (distince dependent on road type)
        /// </summary>
        turnApproaching = 203,
        /// <summary>
        /// Shows the requested nav page (plugin specific)
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Nav Page</para>
        /// </summary>
        showNavPanel = 204,
        /// <summary>
        /// Dials the given number
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Phone Number (without seperators)</para>
        /// <para>Arg2: (Optional) Display Name</para>
        /// </summary>
        dialNumber = 300,
        /// <summary>
        /// Prompts the user to dial the given number
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Phone Number (without seperators)</para>
        /// <para>Arg2: (Optional) Display Name</para>
        /// </summary>
        promptDialNumber = 301,
        /// <summary>
        /// Used as event only! Indicates a new zone has been added 
        /// <para>---------------------------------------</para>
        /// <para>Arg: None</para>
        /// </summary>
        ZoneAdded,
        /// <summary>
        /// Used as event only! Indicates a zone has been removed
        /// <para>---------------------------------------</para>
        /// <para>Arg: None</para>
        /// </summary>
        ZoneRemoved,
        /// <summary>
        /// Used as event only!
        /// <para>[Event]Indicates the active zones has changed for a screen</para>
        /// <para>---------------------------------------</para>
        /// <para>Arg: Screen that was changed</para>
        /// </summary>
        ZoneSetActive,
        /// <summary>
        /// Used as event only! Indicates a zone has been updated
        /// <para>---------------------------------------</para>
        /// <para>Arg: None</para>
        /// </summary>
        ZoneUpdated,
        /// <summary>
        /// Used as event only! Indicates zones has been loaded and initialized after OM startup
        /// <para>---------------------------------------</para>
        /// <para>Arg: None</para>
        /// </summary>
        ZonesLoaded,
        /// <summary>
        /// Used as event only! Indicates that audio devices are available
        /// </summary>
        AudioDevicesAvailable,
        /// <summary>
        /// Used as event only! Media indexing has completed (NB! This event is not bound to any zone)
        /// <para>Arg: Name of plugin that completed indexing</para>
        /// </summary>
        MediaIndexingCompleted,
        /// <summary>
        /// Used as event only! Media database has been cleared (NB! This event is not bound to any zone)
        /// <para>Arg: Name of plugin that initialized the clearing</para>
        /// </summary>
        MediaDBCleared,
        /// <summary>
        /// Event: Occurs when the ClientArea property changes
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen</para>
        /// <para>Arg2: Rectangle as string: X|Y|W|H</para>
        /// </summary>
        ClientAreaChanged,
        /// <summary>
        /// Unloads all other screens and goes back to the mainmenu screen
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: (Optional) TransitionType</para>
        /// </summary>
        goHome,
        /// <summary>
        /// Shows a panel without closing any other panels
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Full panel reference (PluginName;PanelName)</para>
        /// <para>Arg3: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Transition Name</para>
        /// </summary>
        ShowPanel,
        /// <summary>
        /// Hides a panel without closing any other panels
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Full panel reference (PluginName;PanelName)</para>
        /// <para>Arg3: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Transition Name</para>
        /// </summary>
        HidePanel,
        /// <summary>
        /// Closes all other panels and shows only the specified one
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// <para>Arg2: Full panel reference (PluginName;PanelName)</para>
        /// <para>Arg3: <i>(Optional)</i> <seealso cref="eGlobalTransition"/> Transition Name</para>
        /// </summary>
        GotoPanel,
        /// <summary>
        /// Used as event only! Entering idle mode (no user input detected in OM)
        /// <para>Arg: Screen number</para>
        /// </summary>
        IdleEntering,
        /// <summary>
        /// Used as event only! Leaving idle mode (user input detected in OM)
        /// <para>Arg: Screen number</para>
        /// </summary>
        IdleLeaving,
        /// <summary>
        /// Used as event only! Current media provider for the given zone is changed
        /// <para>Arg: Zone</para>
        /// </summary>
        MediaProviderChanged,
        /// <summary>
        /// Used as event only! Current media provider updated it's info
        /// <para>Arg: Zone</para>
        /// </summary>
        MediaProviderInfoChanged,

        /// <summary>
        /// Used as event only! Raised as soon as a closeprogram command is received
        /// </summary>
        CloseProgramPreview,

        /// <summary>
        /// Used as event only! Sunrise at current location
        /// </summary>
        CurrentLocationSunrise,

        /// <summary>
        /// Used as event only! Sunset at current location
        /// </summary>
        CurrentLocationSunset,

        /// <summary>
        /// Used as event only! Changed to day mode for current location
        /// </summary>
        CurrentLocationDay,

        /// <summary>
        /// Used as event only! Changed to night mode for current location
        /// </summary>
        CurrentLocationNight,

        /// <summary>
        /// Used as event only! A USB Device has been added to the system
        /// <para>Arg: Type of device (Unknown, PORT, VOLUME)</para>
        /// </summary>
        USBDeviceAdded,

        /// <summary>
        /// Used as event only! A USB Device has been removed from the system
        /// <para>Arg: Type of device (Unknown, PORT, VOLUME)</para>
        /// </summary>
        USBDeviceRemoved,

        /// <summary>
        /// Toggles between fullscreen and windowed mode
        /// <para>---------------------------------------</para>
        /// <para>Arg1: Screen Number</para>
        /// </summary>
        ToggleFullscreen
    }
    /// <summary>
    /// The status of a plugins initialization
    /// </summary>
    public enum eLoadStatus : byte
    {
        /// <summary>
        /// Have not attempted to load it yet
        /// </summary>
        NotLoaded = 0,
        /// <summary>
        /// Everything went OK.  Plugin is initialized
        /// </summary>
        LoadSuccessful = 1,
        /// <summary>
        /// Plugin requires default settings to continue.  Load default settings panel and then re-initialize when the user has set the settings.
        /// </summary>
        SettingsRequired = 2,
        /// <summary>
        /// A required dependency is not loaded yet or something needs to be retried.  Load the rest of the plugins and then retry the initialization.
        /// </summary>
        LoadFailedRetryRequested = 3,
        /// <summary>
        /// Load failed but this was not the result of a crash.  Plugin will be unloaded.
        /// </summary>
        LoadFailedGracefulUnloadRequested = 4,
        /// <summary>
        /// Load failed or an unknown error occured.  Plugin will be unloaded. (Occurs automatically if an uncaught error is thrown)
        /// </summary>
        LoadFailedUnloadRequested = 10
    }

    /// <summary>
    /// System power change events
    /// </summary>
    public enum ePowerEvent
    {
        /// <summary>
        /// Unknown event
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// System is shutting down
        /// </summary>
        ShutdownPending = 1,
        /// <summary>
        /// System is retarting
        /// </summary>
        LogoffPending = 2,
        /// <summary>
        /// System is going to sleep
        /// </summary>
        SleepOrHibernatePending = 3,
        /// <summary>
        /// System is resuming from sleep or hibernate
        /// </summary>
        SystemResumed = 4,
        /// <summary>
        /// System battery low
        /// </summary>
        BatteryLow = 5,
        /// <summary>
        /// System battery critical
        /// </summary>
        BatteryCritical = 6,
        /// <summary>
        /// System running on battery
        /// </summary>
        SystemOnBattery = 7,
        /// <summary>
        /// System running on AC Power
        /// </summary>
        SystemPluggedIn = 8
    }
    /// <summary>
    /// Various Wireless Events (Bluetooth, WiFi, etc.)
    /// </summary>
    public enum eWirelessEvent : byte
    {
        /// <summary>
        /// An unknown or handled event
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Wireless networks are detected and in-range
        /// </summary>
        WirelessNetworksAvailable = 1,
        /// <summary>
        /// Connecting to a wireless connection
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Network Name</para>
        /// </summary>
        ConnectingToWirelessNetwork = 2,
        /// <summary>
        /// A successful connection has been established to a wireless connection
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Network Name</para>
        /// </summary>
        ConnectedToWirelessNetwork = 3,
        /// <summary>
        /// Wireless (wifi) signal strength has changed
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Signal Value (0-100)</para>
        /// </summary>
        WirelessSignalStrengthChanged = 4,
        /// <summary>
        /// The network connection has been established but an internet connection
        /// requires Access Point credentials be entered in the web browser
        /// </summary>
        WirelessNetworkRequiresLogin = 5,
        /// <summary>
        /// Disconnected from the wireless network
        /// </summary>
        DisconnectedFromWirelessNetwork = 6,
        /// <summary>
        /// Bluetooth devices are detected and within the connection range
        /// </summary>
        BluetoothDevicesInRange = 10,
        /// <summary>
        /// A successful connection has been made to a bluetooth internet connection (DUN, PAN, etc)
        /// </summary>
        ConnectedToBluetoothInternet = 11,
        /// <summary>
        /// The bluetooth connected device's (phone's) signal strength
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Signal Value (0-100) (0=No Signal,-1=not supported)</para>
        /// </summary>
        BluetoothSignalStrengthChanged = 12,
        /// <summary>
        /// Bluetooth Battery Level
        /// <para>--------------------------------------------</para>
        /// <para>Arg: Battery Level (0-10) (0=Battery info not supported)</para>
        /// </summary>
        BluetoothBatteryLevelChanged = 13,
        /// <summary>
        /// A bluetooth pairing request has been received
        /// <para>--------------------------------------------</para>
        /// <para>Arg: Device Name</para>
        /// </summary>
        BluetoothPairingRequest = 14,
        /// <summary>
        /// A bluetooth device has been successfully paired
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Device Name</para>
        /// </summary>
        BluetoothDeviceConnected = 15,
        /// <summary>
        /// A call is incoming
        /// <para>-------------------------------------------</para>
        /// <para>Arg: 2 Lines delimited by Cr+Lf</para>
        /// <para>Line1: Caller Name (Either from contacts or callerID; May be blank if Unknown)</para>
        /// <para>Line2: Phone Number (May be blank if Unknown)</para>
        /// </summary>
        BluetoothIncomingCall = 16,
        /// <summary>
        /// An SMS message has been received
        /// <para>-------------------------------------------</para>
        /// <para>Arg: MessageID (Contents be retrieved from database)</para>
        /// </summary>
        BluetoothSMSReceived = 17,
        /// <summary>
        /// A Pairing Password is available
        /// <para>-------------------------------------------</para>
        /// <para>Arg: Pairing Password</para>
        /// </summary>
        PairingPasswordAvailable = 18
    }

    /// <summary>
    /// Mouse click types
    /// </summary>
    public enum ClickTypes 
    { 
        /// <summary>
        /// No click
        /// </summary>
        None, 

        /// <summary>
        /// Normal click
        /// </summary>
        Normal, 

        /// <summary>
        /// Long click
        /// </summary>
        Long, 

        /// <summary>
        /// Hold click
        /// </summary>
        Hold 
    }

    /// <summary>
    /// Type of media
    /// </summary>
    public enum eMediaType : short
    {
        /// <summary>
        /// A device has been removed
        /// </summary>
        DeviceRemoved = -1,
        /// <summary>
        /// Unknown Type/Not Set
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// A local file
        /// </summary>
        Local = 1,
        /// <summary>
        /// A network file
        /// </summary>
        Network = 2,
        /// <summary>
        /// Attached devices
        /// </summary>
        LocalHardware = 3,
        /// <summary>
        /// An Audio CD
        /// </summary>
        AudioCD = 4,
        /// <summary>
        /// A DVD
        /// </summary>
        DVD = 5,
        /// <summary>
        /// A Blu-Ray Disc
        /// </summary>
        BluRay = 6,
        /// <summary>
        /// An HD-DVD
        /// </summary>
        HDDVD = 7,
        /// <summary>
        /// A URL (Streaming media)
        /// </summary>
        HTTPUrl = 8,
        /// <summary>
        /// An RTP URL (Streaming media)
        /// </summary>
        RTSPUrl = 9,
        /// <summary>
        /// A YouTube video
        /// </summary>
        YouTube = 10,
        /// <summary>
        /// An iPod
        /// </summary>
        AppleDevice = 11,
        /// <summary>
        /// A bluetooth audio device
        /// </summary>
        BluetoothResource = 12,
        /// <summary>
        /// An openDrive device
        /// </summary>
        OpenDrive = 13,
        /// <summary>
        /// Digital Camera
        /// </summary>
        Camera = 14,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved = 15,
        /// <summary>
        /// Multimedia Streaming Protocol
        /// </summary>
        MMSUrl = 16,
        /// <summary>
        /// Radio
        /// </summary>
        Radio = 17,
        /// <summary>
        /// Internet Radio
        /// </summary>
        InternetRadio = 18,
        /// <summary>
        /// One or more live camera streams
        /// </summary>
        LiveCamera = 19,
        /// <summary>
        /// Smartphone
        /// </summary>
        Smartphone = 20,
        
        /// <summary>
        /// A url stream
        /// </summary>
        URLStream,

        /// <summary>
        /// Other
        /// </summary>
        Other = 25
    }
    /// <summary>
    /// Textbox Options
    /// </summary>
    [Flags]
    public enum textboxFlags : byte
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
    public enum backgroundStyle : byte
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
        SolidColor = 3
    }
    /// <summary>
    /// Priority
    /// </summary>
    public enum ePriority : byte
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
        /// UI Level
        /// </summary>
        UI,
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
        DataProviderStatus = 1,
        /// <summary>
        /// Get the position (in seconds) of the currently playing media [float]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaPosition = 2,
        /// <summary>
        /// Gets the volume [int -1(mute) to 100]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen number or zone ID [int]</para>
        /// </summary>
        GetSystemVolume = 3,
        /// <summary>
        /// Gets an array of availble networks [string[]]
        /// </summary>
        GetAvailableNetworks = 4,
        /// <summary>
        /// Gets the device info string from a raw hardware plugin
        /// </summary>
        GetDeviceInfo = 5,
        /// <summary>
        /// Gets the firmware info string from a raw hardware plugin
        /// </summary>
        GetFirmwareInfo = 6,
        /// <summary>
        /// Gets the TunedContentInfo from the currently loaded tunedcontent plugin
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetTunedContentInfo = 7,
        /// <summary>
        /// Gets the status of the currently loaded media player [ePlayerStatus]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetMediaStatus = 8,
        /// <summary>
        /// Returns an IMediaDatabase object
        /// </summary>
        GetMediaDatabase = 9,
        /// <summary>
        /// Returns the loaded plugin collection. [List(IBasePlugin)]
        /// </summary>
        GetPlugins = 10,
        /// <summary>
        /// Get the playback speed [float] of the currently playing media
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Instance [int]</para>
        /// </summary>
        GetPlaybackSpeed = 11,
        /// <summary>
        /// Returns a list of audio devices [string[]]available on the system
        /// </summary>
        GetAudioDevices = 12,
        /// <summary>
        /// Gets the height and width scale factors [Point]
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        GetScaleFactors = 13,
        /// <summary>
        /// Returns the OMControl that draws a map
        /// </summary>
        GetMap = 14,
        /// <summary>
        /// Returns a string[] listing available navigation panels
        /// </summary>
        GetAvailableNavPanels = 15,
        /// <summary>
        /// Returns the current GPS position [Position].  May be 0,0 if no GPS signal is available.
        /// </summary>
        GetCurrentPosition = 16,
        /// <summary>
        /// Returns the nearest Address [Location]
        /// </summary>
        GetNearestAddress = 17,
        /// <summary>
        /// Returns the route destination [Location].  Will return null if no route calculated
        /// </summary>
        GetDestination = 18,
        /// <summary>
        /// Returns a [string[]] containing all of the available skins
        /// </summary>
        GetAvailableSkins = 19,
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
        GetPlayerVolume = 22,
        /// <summary>
        /// Gets a list of available keyboards
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetAvailableKeyboards = 24,
        /// <summary>
        /// Gets a list of available mice
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetAvailableMice = 25,
        /// <summary>
        /// Gets the screen brightness
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        GetScreenBrightness = 26,
        /// <summary>
        /// Gets a list of currently mapped mice units
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetMappedMice = 27,
        /// <summary>
        /// Gets a list of currently unmapped mice units
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetUnMappedMice = 28,
        /// <summary>
        /// Gets a list of currently mapped keyboards units
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetMappedKeyboards = 29,
        /// <summary>
        /// Gets a list of currently unmapped keyboards units
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetUnMappedKeyboards = 30,
        /// <summary>
        /// Gets a list of valid keyboard units (and options) for the requested screen
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetKeyboardUnitsForScreen = 31,
        /// <summary>
        /// Gets a list of valid mice units (and options) for the requested screen
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetMiceUnitsForScreen = 32,
        /// <summary>
        /// Gets the currently mapped mouse for the given screen
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetMiceCurrentUnitForScreen = 33,
        /// <summary>
        /// Gets the currently mapped keyboard for the given screen
        /// <para>----------------------------------------------------</para>
        /// <para>Param: Screen [int]</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetKeyboardCurrentUnitForScreen = 34,
        /// <summary>
        /// Detect and return a mouse device (first detected click is returned as the device), 
        /// devices is returned as an integer number indicating the index of the device in the driver array.
        /// <para>This method will timeout after 10 seconds if no input is detected, this is returned as -3 (Not Found)</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetMouseDetectedUnit = 35,
        /// <summary>
        /// Detect and return a keyboard device (first detected keypress is returned as the device), 
        /// devices is returned as an integer number indicating the index of the device in the driver array.
        /// <para>This method will timeout after 10 seconds if no input is detected, this is returned as -3 (Not Found)</para>
        /// </summary>
        [Obsolete("Use datasources instead")]
        GetKeyboardDetectedUnit = 36,

        /// <summary>
        /// Returns a list of configured zones (all or one)
        /// <para>Provide a name if you want to get a specific zone</para>
        /// <para>NB! Returned object is ZoneHandling if no name is given, the list of zones can be found at the property Zones</para>
        /// <para>If a name is given the returned object will be a Zone</para>
        /// </summary>
        GetZones = 37,

        /// <summary>
        /// Returns a list of the disabled plugins
        /// </summary>
        GetPluginsDisabled

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
    public enum eTunedContentStatus : byte
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
        Initializing = 4,
        /// <summary>
        /// Powered Off
        /// </summary>
        PoweredOff = 5,
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
    public enum eTunedContentBand : byte
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
        HD = 4,
        /// <summary>
        /// Satellite Radio
        /// </summary>
        XM = 5,
        /// <summary>
        /// Over the air TV
        /// </summary>
        OTATV = 6,
        /// <summary>
        /// Everything else
        /// </summary>
        Other = 7,
        /// <summary>
        /// Internet Radio
        /// </summary>
        Internet = 8,
        /// <summary>
        /// Satellite Radio
        /// </summary>
        Sirius = 9
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
        public string ConnectionType = "";
        /// <summary>
        /// Creates a new connection info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="signal"></param>
        /// <param name="passwordRequired"></param>
        /// <param name="type"></param>
        public connectionInfo(string name, string id, int speed, uint signal, string type, ePassType passwordRequired)
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
    public enum ePassType : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Single string shared key
        /// </summary>
        SharedKey = 1,
        /// <summary>
        /// Username and Password
        /// </summary>
        UserPass = 2,
        /// <summary>
        /// Username, Password and Domain
        /// </summary>
        UserPassDomain = 3
    }

    /// <summary>
    /// Navigation Events
    /// </summary>
    public enum eNavigationEvent
    {
        /// <summary>
        /// Unknown Navigation Event
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Occurs when the GPS Fix changes
        /// <para>---------------------------------------</para>
        /// <para>Arg: No GPS, No Signal, 2D Fix, 3D Fix</para>
        /// </summary>
        GPSStatusChange = 1,
        /// <summary>
        /// Route has changed
        /// <para>---------------------------------------</para>
        /// <para>Arg: Navigation Started, Navigation Cancelled, Waypoint Added, Detouring</para>
        /// </summary>
        RouteChanged = 2,
        /// <summary>
        /// A turn is approaching...query the nextTurn parameter for more info
        /// </summary>
        TurnApproaching = 3,
        /// <summary>
        /// Occurs when the current town/city changes.  Useful for refreshing data for the new location
        /// <para>---------------------------------------</para>
        /// <para>Arg: City/Town Name</para>
        /// </summary>
        LocationChanged = 4
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
    public class Location : ICloneable
    {
        /// <summary>
        /// Is this an empty address?
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (String.IsNullOrEmpty(Name) &&
                String.IsNullOrEmpty(Address) &&
                String.IsNullOrEmpty(Street) && 
                String.IsNullOrEmpty(City) && 
                String.IsNullOrEmpty(State) && 
                String.IsNullOrEmpty(Country) && 
                String.IsNullOrEmpty(Zip) &&
                String.IsNullOrEmpty(Keyword) && 
                Latitude == 0f && 
                Longitude == 0f);
        }

        /// <summary>
        /// A freely used keyword field for this location (can be used for other lookups)
        /// </summary>
        public string Keyword;

        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name;
        /// <summary>
        /// The full address string
        /// </summary>
        public string Address;
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
        /// Altitude
        /// </summary>
        public float Altitude;

        /// <summary>
        /// Creates a new location based on a keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        static public Location FromKeyword(string keyword)
        {
            return new Location() { Keyword = keyword };
        }        
        
        /// <summary>
        /// Creates a new Address
        /// </summary>
        public Location()
        {
            Name = String.Empty;
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
        /// Creates a new Address for the given coordinates
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public Location(double lat, double lng)
        {
            Longitude = (float)lng;
            Latitude = (float)lat;
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
        public Location(string street, string city, string state, string zip, string country)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            Zip = zip;
        }
        /// <summary>
        /// Trys to parse a string into an Address 
        /// <para>The string should be in a format like this [street, city, state, zip, country] where ',' is used as a separator</para>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string address, out Location result)
        {
            if (String.IsNullOrEmpty(address))
            {
                result = null;
                return false;
            }
            result = null;
            string[] args = address.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 5)
            {
                result = new Location(args[0], args[1].Trim(), args[2].Trim(), args[3].Trim(), args[4].Trim());
                return true;
            }
            else if (args.Length == 4)
            {
                result = new Location(args[0], args[1].Trim(), args[2].Trim(), "", args[4].Trim());
                return true;
            }
            else if (args.Length == 3)
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
        /// Tries to parse a string that describes a latitude / longitude to a location data
        /// The text has to be presented in the format 0.00000,0.00000
        /// </summary>
        /// <param name="latlng"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseLatLng(string latlng, out Location result)
        {
            if (String.IsNullOrEmpty(latlng))
            {
                result = null;
                return false;
            }
            result = null;
            string[] args = latlng.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 2)
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
            return this.MemberwiseClone();
        }
        /// <summary>
        /// Converts an Address to a string
        /// </summary>
        /// <returns></returns>
        public string ToStringFormatted()
        {
            return String.Format("{0}, \n{1}, \n{2}, \n{3} \n[{4};{5}]", Street, City, State, Country, Latitude, Longitude); 
        }
        public string ToStringAddress()
        {
            return String.Format("{0}", Address); 
        }
        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}[{5};{6}]", Name, Street, City, State, Country, Latitude, Longitude); 
        }
        public string ToKeyword()
        {
            return String.Format("{0} {1} {2} {3} {4}", Name, Street, City, State, Country);
        }
        /// <summary>
        /// Returns a latitude / longitude value as a string 
        /// String example: "60.7993125915527, 10.6513748168945"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ToLatLngString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", Latitude, Longitude);
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Location a, Location b)
        {
            //// If both are null, or both are same instance, return true.
            //if (System.Object.ReferenceEquals(a, b))
            //    return true;
            if (((object)a == null) && ((object)b == null))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            string cityA = String.IsNullOrEmpty(a.City) ? String.Empty : a.City;
            string cityB = String.IsNullOrEmpty(b.City) ? String.Empty : b.City;
            if (!cityA.Equals(cityB)) return false;

            string countryA = String.IsNullOrEmpty(a.Country) ? String.Empty : a.Country;
            string countryB = String.IsNullOrEmpty(b.Country) ? String.Empty : a.Country;
            if (!countryA.Equals(countryB)) return false;

            string stateA = String.IsNullOrEmpty(a.State) ? String.Empty : a.State;
            string stateB = String.IsNullOrEmpty(b.State) ? String.Empty : b.State;
            if (!stateA.Equals(stateB)) return false;
            
            string streetA = String.IsNullOrEmpty(a.Street) ? String.Empty : a.Street;
            string streetB = String.IsNullOrEmpty(b.Street) ? String.Empty : b.Street;
            if (!streetA.Equals(streetB)) return false;
            
            string zipA = String.IsNullOrEmpty(a.Zip) ? String.Empty : a.Zip;
            string zipB = String.IsNullOrEmpty(b.Zip) ? String.Empty : b.Zip;
            if (!zipA.Equals(zipB)) return false;

            if (a.Latitude != b.Latitude) return false;
            if (a.Longitude != b.Longitude) return false;

            return true;
        }

        /// <summary>
        /// !=
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(Location A, Location B)
        {
            return !(A == B); ;
        }
    }
    /// <summary>
    /// A/V Player statue
    /// </summary>
    public enum ePlayerStatus : byte
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
            if ((part1 == null) || (part2 == null))
                throw new ArgumentNullException();
            if (part1.Length == 0)
                return part2;
            else if (part2.Length == 0)
                return part1;
            char ds = System.IO.Path.DirectorySeparatorChar;
            if (part1[part1.Length - 1] == ds)
                return part1 + part2;
            return part1 + ds + part2;
        }
        /// <summary>
        /// Combines three path strings
        /// </summary>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <param name="part3"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public static string Combine(string part1, string part2, string part3)
        {
            if (part3 == null)
                return Combine(part1, part2);
            char ds = System.IO.Path.DirectorySeparatorChar;
            char ads = System.IO.Path.AltDirectorySeparatorChar;

            if ((part1[part1.Length - 1] == ds) || (part1[part1.Length - 1] == ads) || (part1[part1.Length - 1] == System.IO.Path.VolumeSeparatorChar))
                if ((part2[part2.Length - 1] == ds) || (part1[part1.Length - 1] == ads))
                    return part1 + part2 + part3;
                else
                    return part1 + part2 + ds + part3;
            else
                if ((part2[part2.Length - 1] == ds) || (part1[part1.Length - 1] == ads))
                    return part1 + ds + part2 + part3;
                else
                    return part1 + ds + part2 + ds + part3;
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
                int ext = 0;
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
        public int TrackNumber = -1;
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
        public int Length = -1;
        /// <summary>
        /// Rating 0-5.  -1 for not set.
        /// </summary>
        public int Rating = -1;
        /// <summary>
        /// Optional - The cover art for the selected media
        /// </summary>
        public OImage coverArt;
        /// <summary>
        /// Source Type
        /// </summary>
        public eMediaType Type = eMediaType.NotSet;
        /// <summary>
        /// Create a new mediaInfo object
        /// </summary>
        /// <param name="URL"></param>
        public mediaInfo(eMediaType type, string URL)
        {
            this.Type = type;
            this.Location = URL;
        }
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

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2} [{3}]", Artist, Album, Name, Location);
        }

        /// <summary>
        /// Replaces info in this mediaInfo if newer data is available
        /// </summary>
        /// <param name="album"></param>
        /// <param name="artist"></param>
        /// <param name="name"></param>
        /// <param name="genre"></param>
        /// <param name="length"></param>
        /// <param name="tracknumber"></param>
        /// <returns></returns>
        public bool UpdateMissingInfo(string album = null, string artist = null, string name = null, string genre = null, int? length = null, int? tracknumber = null, OImage coverArt = null, eMediaType? type = null)
        {
            bool updated = false;
            if (type.HasValue)
                if (this.Type != type.Value)
                {
                    this.Type = type.Value;
                    updated = true;
                }
            if (album != null)
                if (String.IsNullOrEmpty(this.Album))
                {
                    this.Album = album;
                    updated = true;
                }
            if (artist != null)
                if (String.IsNullOrEmpty(this.Artist))
                {
                    this.Artist = artist;
                    updated = true;
                }
            if (genre != null)
                if (String.IsNullOrEmpty(this.Genre))
                {
                    this.Genre = genre;
                    updated = true;
                }
            if (length.HasValue)
                if (this.Length != length.Value)
                {
                    this.Length = length.Value;
                    updated = true;
                }
            if (tracknumber.HasValue)
                if (this.TrackNumber != tracknumber.Value)
                {
                    this.TrackNumber = tracknumber.Value;
                    updated = true;
                }
            if (name != null)
                if (String.IsNullOrEmpty(this.Name))
                {
                    this.Name = name;
                    updated = true;
                }
            if (coverArt != null)
                if (this.coverArt == null)
                {
                    this.coverArt = coverArt;
                    updated = true;
                }

            return updated;
        }

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
        /// Number of Audio Channels (1 = Mono, 2 = Stereo)
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
        ///// <summary>
        ///// A transition modifier for global transitions (Range: 0-1)
        ///// </summary>
        //public float globalTransitionIn = 0;
        ///// <summary>
        ///// A transition modifier for global transitions (Range: 1-0)
        ///// </summary>
        //public float globalTransitionOut = 1;
        ///// <summary>
        ///// Transparency of a highlighted button (Used in button click animations)
        ///// </summary>
        //public float transparency = 1;
        ///// <summary>
        ///// A size modifier for button animations
        ///// </summary>
        //public int transitionTop = 0;
        ///// <summary>
        ///// The current rendering mode of the rendering window
        ///// </summary>
        //public eModeType currentMode = eModeType.Normal;

        ///// <summary>
        ///// A offset value indicating how the control should affects it's left and top properties
        ///// </summary>
        //public Point PlacementOffset = new Point();

        /// <summary>
        /// A offset value indicating how the control should affects it's left, top, width and height properties
        /// </summary>
        public Rectangle Offset = new Rectangle();

        /// <summary>
        /// A value indicating the alpha value to use when rendering the control (Negative value = disabled)
        /// </summary>
        public float Alpha = 1.0f;

        public OpenTK.Vector3d Rotation = new OpenTK.Vector3d();
        public OpenTK.Vector3d Scale = new OpenTK.Vector3d(1, 1, 1);
        public OpenTK.Matrix4 TransformationMatrix = new OpenTK.Matrix4();

        public bool TransitionActive = false;        
    }

    /// <summary>
    /// Classification for debug messages
    /// </summary>
    public enum DebugMessageType : byte
    {
        /// <summary>
        /// Message is of type: Unspecified / default
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// Message is of type: Info
        /// </summary>
        Info = 10,
        /// <summary>
        /// Message is of type: Warning
        /// </summary>
        Warning = 20,
        /// <summary>
        /// Message is of type: Error
        /// </summary>
        Error = 30
    }

    /// <summary>
    /// Input types that can be used with controls supporting input
    /// </summary>
    public enum OSKInputTypes
    {
        /// <summary>
        /// No automatic handling of input (default) or handled manually by the skin
        /// </summary>
        None,
        /// <summary>
        /// A onscreen keypad (OSK) will be used for input
        /// </summary>
        Keypad,
        /// <summary>
        /// A onscreen numpad will be used for input
        /// </summary>
        Numpad,

        /// <summary>
        /// A small keypad that can be placed on screen with controls
        /// </summary>
        KeypadSmall
    }

    public enum OSKShowTriggers
    {
        AfterOnClickEvent,
        BeforeOnClickEvent,
        AfterOnHoldClickEvent,
        BeforeOnHoldClickEvent,
        AfterOnLongClickEvent,
        BeforeOnLongClickEvent
    }

    /// <summary>
    /// Type of information provided
    /// </summary>
    public enum eDataType
    {
        /// <summary>
        /// No datatype / normal data type
        /// </summary>
        None,
        /// <summary>
        /// This is general informational data
        /// </summary>
        Info,
        /// <summary>
        /// This is some update data 
        /// </summary>
        Update,
        /// <summary>
        /// This is warning data
        /// </summary>
        Warning,
        /// <summary>
        /// This is error data
        /// </summary>
        Error,
        /// <summary>
        /// This is internal data
        /// </summary>
        Internal,
        /// <summary>
        /// This is data indicating a completion of some sort
        /// </summary>
        Completion,
        /// <summary>
        /// This is data indicating a failure of some sort
        /// </summary>
        Failure,
        /// <summary>
        /// This is some data that should be shown as a popup to the user (or it is just urgent)
        /// </summary>
        PopUp
    }

    /// <summary>
    /// A string wrapper that can return status if a string has been modified
    /// </summary>
    public class StringWrapper
    {
        private string _Text = "";
        private int _Hash = 0;
        private int _Hash_Stored = 0; 

        /// <summary>
        /// Text
        /// </summary>
        public string Text 
        {
            get
            {
                return _Text;
            }
            set
            {
                int NewHash = value.GetHashCode();
                if (_Hash == NewHash)
                    return;
                _Text = value;
                _Hash = NewHash;
            }
        }

        /// <summary>
        /// Is string changed since last time?
        /// <para>NB! This property can not be monitored, the value is only true until it has been read (One shot only)</para>
        /// </summary>
        public bool Changed
        {
            get
            {
                if (_Hash != _Hash_Stored)
                {
                    _Hash_Stored = _Hash;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Data for the rendering window
    /// </summary>
    public class RenderingWindowData
    {
        /// <summary>
        /// Location of renderingwindow
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// Size of renderingwindow
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// ScaleFactors for the rendering window
        /// </summary>
        public PointF ScaleFactors { get; set; }

        /// <summary>
        /// Initialize a new class of RenderingWindowData
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="scaleFactors"></param>
        public RenderingWindowData(Point location, Size size, PointF scaleFactors)
        {
            this.Location = location;
            this.Size = size;
            this.ScaleFactors = scaleFactors;
        }
    }

    /// <summary>
    /// Base data for sprite.
    /// Imagesprite is a big image mad up with several small onces inside. This struct can be used to describe the placement of one of those sub images.
    /// </summary>
    public struct Sprite
    {
        /// <summary>
        /// Name of sprite
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Coordinates of a the sprite
        /// </summary>
        public Rectangle Coordinates { get; set; }

        /// <summary>
        /// Creates a new sprite
        /// </summary>
        /// <param name="name"></param>
        /// <param name="coordinates"></param>
        public Sprite(string name, Rectangle coordinates)
            : this()
        {
            Name = name;
            Coordinates = coordinates;
        }

        /// <summary>
        /// Creates a new sprite
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Sprite(string name, int left, int top, int width, int height)
            : this()
        {
            Name = name.Trim();
            Coordinates = new Rectangle(left, top, width, height);
        }
    }


}
