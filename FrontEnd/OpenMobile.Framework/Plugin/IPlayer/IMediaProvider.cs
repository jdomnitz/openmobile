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
using OpenMobile;
using OpenMobile.Media;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// The different types of media supported by the MediaProvider classes
    /// </summary>
    [Flags]
    public enum MediaProviderTypes
    {
        /// <summary>
        /// Invalid/No type
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Audio provider
        /// </summary>
        AudioSource = 1,

        /// <summary>
        /// Video provider
        /// </summary>
        VideoSource = 2,

        /// <summary>
        /// Audio playback
        /// </summary>
        AudioPlayback = 4,

        /// <summary>
        /// Video playback
        /// </summary>
        VideoPlayback = 8,

        /// <summary>
        /// Some other provider
        /// </summary>
        OtherSource = 16,

        /// <summary>
        /// Some other provider
        /// </summary>
        OtherPlayback = 32
    }

    /// <summary>
    /// Media provider commands
    /// </summary>
    public enum MediaProvider_Commands
    {
        /// <summary>
        /// Stop playback.
        /// </summary>
        Stop,

        /// <summary>
        /// Start playback. Param0: file as mediaInfo (If param0 is missing the current playlist location will be played instead)
        /// <para>Param0 can also be a integer describing a index in the current playlist to play (zerobased)</para>
        /// </summary>
        Play,

        /// <summary>
        /// Pause playback.
        /// </summary>
        Pause,

        /// <summary>
        /// Set current playback position. Param0: Position as int (0 - 100%)
        /// </summary>
        PlaybackPosition_Set,

        /// <summary>
        /// Return current playback position. 
        /// </summary>
        PlaybackPosition_Get,

        /// <summary>
        /// Seek forward. Param0: Seconds as int 
        /// </summary>
        Forward,

        /// <summary>
        /// Seek backward. Param0: Seconds as int  
        /// </summary>
        Backward,

        /// <summary>
        /// Step forward. 
        /// </summary>
        StepForward,

        /// <summary>
        /// Step backward. 
        /// </summary>
        StepBackward,

        /// <summary>
        /// Sets the current OMTargetWindow for video output. Param0: targetWindow as OMTargetWindow, Param2: Screen
        /// </summary>
        SetVideoTarget,

        /// <summary>
        /// Cycle trough available subtitles (if any). 
        /// </summary>
        SubTitle_Cycle,

        /// <summary>
        /// Disable subtitles. 
        /// </summary>
        SubTitle_Disable,

        /// <summary>
        /// Sets the suffle state. Param0: Shuffle state as bool (if param is missing suffle is toggled)
        /// </summary>
        Suffle,

        /// <summary>
        /// Sets the current playlist. Param0: new Playlist as Playlist
        /// </summary>
        PlayList_Set,

        /// <summary>
        /// Sets the repeat state. Param0: Repeat state as bool (if param is missing repeat is toggled)
        /// </summary>
        Repeat,
        
        /// <summary>
        /// Custom command, Param0: Command name, Param1..ParamX: Command parameters
        /// </summary>
        Custom
    }

    /// <summary>
    /// A base for MediaProvider command/data wrappers
    /// </summary>
    public class MediaProvider_WrapperBase
    {
        /// <summary>
        /// A string decribing the error that occured (if any)
        /// </summary>
        public string ErrorText
        {
            get
            {
                return this._ErrorText;
            }
            set
            {
                if (this._ErrorText != value)
                {
                    this._ErrorText = value;
                    this.Error = true;
                }
            }
        }
        private string _ErrorText;

        /// <summary>
        /// Set to true if an error occured
        /// </summary>
        public bool Error
        {
            get
            {
                return this._Error;
            }
            set
            {
                if (this._Error != value)
                {
                    this._Error = value;
                }
            }
        }
        private bool _Error;

        /// <summary>
        /// The command was accepted by the media interface
        /// </summary>
        public bool Accepted
        {
            get
            {
                return this._Accepted;
            }
            set
            {
                if (this._Accepted != value)
                {
                    this._Accepted = value;
                }
            }
        }
        private bool _Accepted;

        /// <summary>
        /// Command parameters
        /// </summary>
        public object[] Parameters
        {
            get
            {
                return this._Parameters;
            }
            set
            {
                if (this._Parameters != value)
                {
                    this._Parameters = value;
                }
            }
        }
        private object[] _Parameters;
    }

    /// <summary>
    /// A wrapper around a MediaProvider command
    /// </summary>
    public class MediaProvider_CommandWrapper : MediaProvider_WrapperBase
    {
        /// <summary>
        /// Command
        /// </summary>
        public MediaProvider_Commands Command
        {
            get
            {
                return this._Command;
            }
            set
            {
                if (this._Command != value)
                {
                    this._Command = value;
                }
            }
        }
        private MediaProvider_Commands _Command;

        /// <summary>
        /// Creates a new commandwrapper
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public MediaProvider_CommandWrapper(MediaProvider_Commands command, params object[] parameters)
        {
            this._Command = command;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Compares two commandwrappers by looking at their commands and parameters
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(MediaProvider_CommandWrapper a, MediaProvider_CommandWrapper b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            if (a.Command.Equals(b.Command) &&
                a.Parameters.Length.Equals(b.Parameters.Length))
            {
                // Also verify each parameter
                for (int i = 0; i < a.Parameters.Length; i++)
                    if (!a.Parameters[i].Equals(b.Parameters[i]))
                        return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Negates two commandwrappers by looking at their commands and parameters
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(MediaProvider_CommandWrapper a, MediaProvider_CommandWrapper b)
        {
            return !(a == b);
        }

    }

    /// <summary>
    /// Mediaprovider playback state
    /// </summary>
    public enum MediaProvider_PlaybackState
    {
        /// <summary>
        /// Stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// Playing
        /// </summary>
        Playing,
        /// <summary>
        /// Paused
        /// </summary>
        Paused
    }

    /// <summary>
    /// Mediaprovider playback data
    /// </summary>
    public class MediaProvider_PlaybackData
    {
        /// <summary>
        /// Total length of available media 
        /// </summary>
        public TimeSpan Length { get; set; }

        /// <summary>
        /// Current playback position
        /// </summary>
        public TimeSpan CurrentPos { get; set; }

        /// <summary>
        /// Current playback position in percentage
        /// </summary>
        public int CurrentPosPercent { get; set; }

        /// <summary>
        /// Current playback state
        /// </summary>
        public MediaProvider_PlaybackState State { get; set; }
    }

    /// <summary>
    /// Media provider data
    /// </summary>
    public enum MediaProvider_Data
    {
        /// <summary>
        /// State of playback. Param0: State as MediaProvider_PlaybackState
        /// </summary>
        PlaybackState,

        /// <summary>
        /// Media info updated. Param0: media as mediaInfo
        /// </summary>
        MediaInfo,

        /// <summary>
        /// Playback position data. Param0: Current position as timespan, Param1: Media length as timespan, Param2: Current Position in percent as int.
        /// </summary>
        PlaybackPositionData,

        /// <summary>
        /// Suffle state as bool.
        /// </summary>
        Suffle,

        /// <summary>
        /// Repeat state as bool.
        /// </summary>
        Repeat,

        /// <summary>
        /// Current Playlist as Playlist.
        /// </summary>
        PlayList,

        /// <summary>
        /// Custom data, Param0: Data name, Param1..ParamX: Data parameters
        /// </summary>
        Custom
    }

    /// <summary>
    /// A wrapper around a MediaProvider data
    /// </summary>
    public class MediaProvider_DataWrapper : MediaProvider_WrapperBase
    {
        /// <summary>
        /// Type of data
        /// </summary>
        public MediaProvider_Data DataType
        {
            get
            {
                return this._DataType;
            }
            set
            {
                if (this._DataType != value)
                {
                    this._DataType = value;
                }
            }
        }
        private MediaProvider_Data _DataType;

        /// <summary>
        /// Creates a new datawrapper
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="parameters"></param>
        public MediaProvider_DataWrapper(MediaProvider_Data dataType, params object[] parameters)
        {
            this._DataType = dataType;
            this.Parameters = parameters;
        }
    }
    
    /// <summary>
    /// Event arguments for mediaProvider events
    /// </summary>
    public class MediaProviderData_EventArgs
    {
        /// <summary>
        /// Data
        /// </summary>
        public MediaProvider_DataWrapper Data { get; set; }

        /// <summary>
        /// Creates a new instance of event arguments
        /// </summary>
        /// <param name="data"></param>
        public MediaProviderData_EventArgs(MediaProvider_DataWrapper data)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// Media provider Data event handler
    /// </summary>
    /// <param name="mediaProvider"></param>
    /// <param name="zone"></param>
    /// <param name="e"></param>
    public delegate void MediaProviderData_EventHandler(object sender, Zone zone, MediaProviderData_EventArgs e);

    public delegate void MediaProviderChanged_EventHandler(object sender, Zone zone);

    /// <summary>
    /// Media provider Info event handler
    /// </summary>
    /// <param name="mediaProvider"></param>
    /// <param name="zone"></param>
    /// <param name="providerInfo"></param>
    public delegate void MediaProviderInfo_EventHandler(object sender, Zone zone, MediaProviderInfo providerInfo);

    /// <summary>
    /// Interface for media providers
    /// </summary>
    public interface IMediaProvider : IBasePlugin
    {
        /// <summary>
        /// Gets the media provider types supported by this plugin
        /// </summary>
        /// <returns></returns>
        MediaProviderTypes MediaProviderType { get; }

        /// <summary>
        /// Gets all media sources supported by this MediaProvider
        /// </summary>
        /// <returns></returns>
        List<MediaSource> GetMediaSources();

        /// <summary>
        /// Sets the currently active media source
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="name">Name of mediasource to activate</param>
        /// <returns></returns>
        bool SetMediaSource(Zone zone, string name);

        /// <summary>
        /// Activate media provider (use this to power on)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        MediaProviderState ActivateMediaProvider(Zone zone);

        /// <summary>
        /// Deactivate media provider (use this to power off)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        MediaProviderState DeactivateMediaProvider(Zone zone);

        /// <summary>
        /// Gets the current information from the media provider
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        MediaProviderInfo GetMediaProviderInfo(Zone zone);

        /// <summary>
        /// Executes the specified command
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="command"></param>
        /// <returns>Return TRUE if command executed successfully</returns>
        bool ExecuteCommand(Zone zone, MediaProvider_CommandWrapper command);

        /// <summary>
        /// Gets data from the mediaprovider
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        object GetData(Zone zone, MediaProvider_DataWrapper data);

        /// <summary>
        /// Data has changed
        /// </summary>
        event MediaProviderData_EventHandler OnDataChanged;

        /// <summary>
        /// Provider info has changed
        /// </summary>
        event MediaProviderInfo_EventHandler OnProviderInfoChanged;
        
    }
}
