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

namespace OpenMobile.Framework
{
    /// <summary>
    /// Base class for OS interaction, override methods to add supported OS functions
    /// </summary>
    public class OSInteractionBase
    {
        public enum CallbackEvents
        {
            Unspecified,
            System_LogOffPending,
            System_ShutdownPending,
            System_SystemResumed,
            System_SleepOrHibernatePending,
            Power_Battery_Low,
            Power_Battery_Critical,
            Power_Battery_RunningOnBattery,
            Power_Battery_RunningOnLine
        }

        /// <summary>
        /// The event callback action (set from the main application)
        /// </summary>
        private Action<CallbackEvents> _EventCallback;        

        public OSInteractionBase(Action<CallbackEvents> eventCallback)
        {
            _EventCallback = eventCallback;
        }

        /// <summary>
        /// Shutdowns the computer
        /// </summary>
        /// <returns></returns>
        public virtual bool Shutdown()
        {
            return false;
        }

        /// <summary>
        /// Restarts the computer
        /// </summary>
        /// <returns></returns>
        public virtual bool Restart()
        {
            return false;
        }

        /// <summary>
        /// Hibernates the computer
        /// </summary>
        /// <returns></returns>
        public virtual bool Hibernate()
        {
            return false;
        }
        
        /// <summary>
        /// Suspends the computer
        /// </summary>
        /// <returns></returns>
        public virtual bool Suspend()
        {
            return false;
        }

        /// <summary>
        /// Raises the event call back handler for OS level events
        /// </summary>
        /// <param name="ev"></param>
        protected void RaiseEvent(CallbackEvents ev)
        {
            _EventCallback(ev);
        }
    }
}
