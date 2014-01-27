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

namespace OpenMobile.NavEngine
{
    #region Work in progress...

    public enum NavigationInstructionAction
    {
        None,
        Head,
        Turn,
        Roundabout,
        Take,
        Keep,
        Continue,
        Slight,
        Merge,
        Destination,
        RestrictedUsage,
        EnteringTollZone,
        LeavingTollZone,
        UTurnAt,
        UTurn
    }

    public enum NavigationInstructionDirection
    {
        None,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
        North,
        South,
        East,
        West,
        On,
        Toward
    }

    public struct NavigationStepAction
    {
        //private bool IsInstruction_Head(string instruction, out NavigationInstructionDirection direction)
        //{
        //    direction = NavigationInstructionDirection.None;

        //    if (!instruction.StartsWith("Head "))
        //        return false;
                                    
        //    instruction.Replace("Head ",string.Empty);

        //        strRet = strRet.Replace("northeast ", "NE|");
        //        strRet = strRet.Replace("northwest ", "NW|");
        //        strRet = strRet.Replace("southeast ", "SE|");
        //        strRet = strRet.Replace("southwest ", "SW|");
        //        strRet = strRet.Replace("north ", "N|");
        //        strRet = strRet.Replace("south ", "S|");
        //        strRet = strRet.Replace("east ", "E|");
        //        strRet = strRet.Replace("west ", "W|");
        //        strRet = strRet.Replace("|on ", "|on|");
        //        strRet = strRet.Replace(" toward ", "|tw|");
        //        strRet = strRet.Replace("|toward ", "|tw|");

            
        //    if (instruction.Contains("

        //    strRet = strRet.Replace("northeast ", "NE|");
        //    strRet = strRet.Replace("northwest ", "NW|");
        //    strRet = strRet.Replace("southeast ", "SE|");
        //    strRet = strRet.Replace("southwest ", "SW|");
        //    strRet = strRet.Replace("north ", "N|");
        //    strRet = strRet.Replace("south ", "S|");
        //    strRet = strRet.Replace("east ", "E|");
        //    strRet = strRet.Replace("west ", "W|");
        //    strRet = strRet.Replace("|on ", "|on|");
        //    strRet = strRet.Replace(" toward ", "|tw|");
        //    strRet = strRet.Replace("|toward ", "|tw|");
        //}



        ///// <summary>
        ///// Parse the instruction string into an action
        ///// </summary>
        ///// <param name="instruction"></param>
        ///// <returns></returns>
        //private NavigationInstructionAction ParseAction(string instruction)
        //{
        //    if (instruction.Contains("Merge"))
        //        return NavigationInstructionAction.Merge;
        //    else if (instruction.Contains("Keep"))
        //        return NavigationInstructionAction.Keep;
        //    else if (instruction.Contains("U-Turn"))
        //        return NavigationInstructionAction.UTurn;
        //    else if (instruction.Contains("Take"))
        //        return NavigationInstructionAction.Take;
        //    else if (instruction.Contains("Turn"))
        //        return NavigationInstructionAction.Turn;
        //    else if (instruction.Contains("Go through"))
        //        return NavigationInstructionAction.GoThrough;
        //    else if (instruction.Contains("Slight"))
        //        return NavigationInstructionAction.Slight;
        //    else if (instruction.Contains("Entering"))
        //        return NavigationInstructionAction.Entering;
        //    else if (instruction.Contains("Leaving"))
        //        return NavigationInstructionAction.Leaving;
        //    else if (instruction.Contains("Restricted"))
        //        return NavigationInstructionAction.Restricted;
        //    else if (instruction.Contains("Continue"))
        //        return NavigationInstructionAction.Continue;
        //     else if (instruction.Contains("Head"))
        //        return NavigationInstructionAction.Head;
        //    else 
        //        return NavigationInstructionAction.None;
        //}
    }

    #endregion
}
