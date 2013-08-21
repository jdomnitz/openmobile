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
using System.Linq;
using System.Text;

namespace OpenMobile.mPlayer
{
    public enum Commands
    {
        Quit,
        LoadFile,
        LoadList,
        Pause,
        Stop,
        Get_PlaybackPos,
        Get_PlaybackPos_Percent,
        Get_Length,
        SeekRelative,
        SetPosSeconds,
        SetPosPercent,
        PlayDVD,
        PlayCD,
        PlayBluRay,
        Next,
        Previous,
        Loop,
        NextInPlayList,
        PreviousInPlayList,
        SubTitlesCycle,
        SubTitlesDisable
    }
    class mPlayerCommands
    {
        private List<mPlayerCommand> _Commands = new List<mPlayerCommand>();
        public mPlayerCommands()
        {
            // Register commands
            _Commands.Add(new mPlayerCommand(Commands.Quit, false, "pausing_toggle quit", "ID_EXIT="));
            _Commands.Add(new mPlayerCommand(Commands.LoadFile, false, "pausing_toggle loadfile \"{0}\"", "Playing ", 0));
            _Commands.Add(new mPlayerCommand(Commands.LoadList, false, "pausing_toggle loadlist \"{0}\"", "Playing ", 0));
            _Commands.Add(new mPlayerCommand(Commands.Pause, false, "pause"));
            _Commands.Add(new mPlayerCommand(Commands.Stop, false, "pausing_keep_force Stop", "EOF code: 4"));
            _Commands.Add(new mPlayerCommand(Commands.Get_PlaybackPos, false, "get_property time_pos", "ANS_time_pos="));
            _Commands.Add(new mPlayerCommand(Commands.Get_PlaybackPos_Percent, false, "get_property percent_pos", "ANS_percent_pos="));
            _Commands.Add(new mPlayerCommand(Commands.Get_Length, false, "get_property length", "ANS_length="));
            _Commands.Add(new mPlayerCommand(Commands.SeekRelative, false, "pausing_keep seek {0} 0"));
            _Commands.Add(new mPlayerCommand(Commands.SetPosPercent, false, "pausing_keep seek {0} 1"));
            _Commands.Add(new mPlayerCommand(Commands.SetPosSeconds, false, "pausing_keep seek {0} 2"));
            _Commands.Add(new mPlayerCommand(Commands.PlayDVD, true, "pausing_toggle loadfile dvd://", "Playing "));
            _Commands.Add(new mPlayerCommand(Commands.PlayCD, true, "pausing_toggle loadfile cdda://", "Starting playback"));
            _Commands.Add(new mPlayerCommand(Commands.PlayBluRay, true, "pausing_toggle loadfile br://", "Playing "));
            _Commands.Add(new mPlayerCommand(Commands.Next, false, "seek_chapter 1 0"));
            _Commands.Add(new mPlayerCommand(Commands.Previous, false, "seek_chapter -1 0"));
            _Commands.Add(new mPlayerCommand(Commands.Loop, false, "loop {0}"));
            _Commands.Add(new mPlayerCommand(Commands.NextInPlayList, false, "pausing_keep_force pt_step 1"));
            _Commands.Add(new mPlayerCommand(Commands.PreviousInPlayList, false, "pausing_keep_force pt_step -1"));
            _Commands.Add(new mPlayerCommand(Commands.SubTitlesCycle, false, "pausing_keep sub_select"));
            _Commands.Add(new mPlayerCommand(Commands.SubTitlesDisable, false, "pausing_keep sub_select -1"));
        }

        /// <summary>
        /// Get the command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public mPlayerCommand this[Commands command]
        {
            get
            {
                return _Commands.Find(x => x.Command == command);
            }
        }
    }
}
