/*********************************************************************************
    This file is part of OpenMobile.

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
    public class MPlayerVideoMode
    {
        private readonly Dictionary<MPlayerVideoModeName, MPlayerModeCommand> _displayModes;

        public MPlayerVideoMode()
        {
            _displayModes = new Dictionary<MPlayerVideoModeName, MPlayerModeCommand>();
            var modes = new[]
            {
                new MPlayerModeCommand(MPlayerVideoModeName.Null, "null", new[] { PlatformID.MacOSX, PlatformID.Unix, PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE, PlatformID.Xbox }),
                new MPlayerModeCommand(MPlayerVideoModeName.X11, "x11", new[] { PlatformID.Unix }),
                new MPlayerModeCommand(MPlayerVideoModeName.Direct3D, "direct3d", new[] { PlatformID.Xbox, PlatformID.Win32NT, PlatformID.Win32Windows }),
                new MPlayerModeCommand(MPlayerVideoModeName.DirectX, "directx", new[] { PlatformID.Xbox, PlatformID.Win32NT, PlatformID.Win32Windows }),
                new MPlayerModeCommand(MPlayerVideoModeName.GL, "gl", new[] { PlatformID.Unix, PlatformID.MacOSX, PlatformID.Win32NT, PlatformID.Win32Windows }),
                new MPlayerModeCommand(MPlayerVideoModeName.GL2, "gl2", new[] { PlatformID.Unix, PlatformID.MacOSX, PlatformID.Win32NT, PlatformID.Win32Windows }),
            };

            foreach (var mode in modes)
            {
                _displayModes.Add(mode.VideoMode, mode);
            }

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    _mode = MPlayerVideoModeName.GL;
                    break;
                case PlatformID.Unix:
                    _mode = MPlayerVideoModeName.X11;
                    break;
                default:
                    _mode = MPlayerVideoModeName.Direct3D;
                    break;
            }
        }

        private MPlayerVideoModeName _mode;

        public string ModeCommand
        {
            get { return _displayModes[_mode].Command; }
        }

        public MPlayerVideoModeName Mode
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
    public enum MPlayerVideoModeName
    {
        /// <summary>
        /// Null output
        /// </summary>
        Null,
        /// <summary>
        /// Linux
        /// </summary>
        X11,
        /// <summary>
        /// Windows
        /// </summary>
        Direct3D,
        /// <summary>
        /// Windows
        /// </summary>
        DirectX,
        /// <summary>
        /// OpenGL, Simple Version
        /// </summary>
        GL,
        /// <summary>
        /// OpenGL, Simple Version.  Variant of the OpenGL  video  output  driver.   
        /// Supports  videos larger  than  the maximum texture size but lacks 
        /// many of the advanced features and optimizations of the gl driver
        /// and is unlikely to be extended further
        /// </summary>
        GL2
    } 
}
