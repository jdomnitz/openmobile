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
    /// Keep information about video or audio mode
    /// </summary>
    public class MPlayerModeCommand
    {
        public MPlayerVideoModeName VideoMode { get; internal set; }
        public MPlayerAudioModeName AudioMode { get; internal set; }
        public string Command { get; internal set; }
        internal List<PlatformID> Platforms = new List<PlatformID>();

        public MPlayerModeCommand(MPlayerVideoModeName mode, string commandName, IEnumerable<PlatformID> platformIds)
        {
            VideoMode = mode;
            Platforms.AddRange(platformIds);
            Command = commandName;
        }

        public MPlayerModeCommand(MPlayerAudioModeName mode, string commandName, IEnumerable<PlatformID> platformIds)
        {
            AudioMode = mode;
            Platforms.AddRange(platformIds);
            Command = commandName;
        }
    }
}
