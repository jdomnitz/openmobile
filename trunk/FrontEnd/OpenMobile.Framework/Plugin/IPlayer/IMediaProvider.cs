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
        /// Start playback. Param0: file as mediaInfo
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
        /// Seek forward. 
        /// </summary>
        Forward,

        /// <summary>
        /// Seek backward. 
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
    }

    /// <summary>
    /// Media provider commands
    /// </summary>
    public enum MediaProvider_Data
    {

    }

    /// <summary>
    /// A wrapper around a MediaProvider data
    /// </summary>
    public class MediaProvider_DataWrapper : MediaProvider_WrapperBase
    {
        /// <summary>
        /// Data
        /// </summary>
        public MediaProvider_Data Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                if (this._Data != value)
                {
                    this._Data = value;
                }
            }
        }
        private MediaProvider_Data _Data;

        /// <summary>
        /// Creates a new datawrapper
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parameters"></param>
        public MediaProvider_DataWrapper(MediaProvider_Data data, params object[] parameters)
        {
            this._Data = data;
            this.Parameters = parameters;
        }
    }


    /// <summary>
    /// Interface for media providers
    /// </summary>
    public interface IMediaProvider
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
        /// <param name="mediaSource"></param>
        /// <returns></returns>
        bool SetMediaSource(Zone zone, MediaSource mediaSource);

        /// <summary>
        /// Activate media provider (use this to power on)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        bool ActivateMediaProvider(Zone zone);

        /// <summary>
        /// Deactivate media provider (use this to power off)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        bool DeactivateMediaProvider(Zone zone);

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
    }
}
