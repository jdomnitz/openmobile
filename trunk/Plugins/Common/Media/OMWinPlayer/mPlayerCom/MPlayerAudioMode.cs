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
/*
* This class contains code found here: https://mplayerdotnet.codeplex.com/
* No specific license was found for this code
*/
using System;
using System.Collections.Generic;

namespace OpenMobile.mPlayer
{
    /// <summary>
    /// Contains constraits for modes
    /// </summary>
    public class MPlayerAudioMode
    {
        private readonly Dictionary<MPlayerAudioModeName, MPlayerModeCommand> _displayModes;

        public MPlayerAudioMode()
        {
            _displayModes = new Dictionary<MPlayerAudioModeName, MPlayerModeCommand>();
            var modes = new[]
            {
                new MPlayerModeCommand(MPlayerAudioModeName.Null, "null", new[] { PlatformID.MacOSX, PlatformID.Unix, PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE, PlatformID.Xbox }),
                new MPlayerModeCommand(MPlayerAudioModeName.Alsa, "alsa", new[] { PlatformID.Unix }),
                new MPlayerModeCommand(MPlayerAudioModeName.DirectSound, "dsound", new[] { PlatformID.Xbox, PlatformID.Win32NT, PlatformID.Win32Windows }),
                new MPlayerModeCommand(MPlayerAudioModeName.Win32, "win32", new[] { PlatformID.Win32NT, PlatformID.Win32Windows }),
                new MPlayerModeCommand(MPlayerAudioModeName.CoreAudio, "coreaudio", new[] { PlatformID.MacOSX }),
            };

            foreach (var mode in modes)
            {
                _displayModes.Add(mode.AudioMode, mode);
            }

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    _mode = MPlayerAudioModeName.CoreAudio;
                    break;
                case PlatformID.Unix:
                    _mode = MPlayerAudioModeName.Alsa;
                    break;
                default:
                    _mode = MPlayerAudioModeName.DirectSound;
                    break;
            }
        }

        private MPlayerAudioModeName _mode;

        public string ModeCommand
        {
            get { return _displayModes[_mode].Command; }
        }

        public MPlayerAudioModeName Mode
        {
            get { return _mode; }
            set
            {
                if (_displayModes[value].Platforms.Contains(Environment.OSVersion.Platform))
                    _mode = value;
            }
        }
    }

    /// <summary>
    /// Video output modes
    /// </summary>
    public enum MPlayerAudioModeName
    {
        /// <summary>
        /// Null output
        /// </summary>
        Null,
        /// <summary>
        /// Windows
        /// </summary>
        Win32,
        /// <summary>
        /// Windows
        /// </summary>
        DirectSound,
        /// <summary>
        /// Linux
        /// </summary>
        Alsa,
        /// <summary>
        /// MacOSX
        /// </summary>
        CoreAudio
    } 
}
