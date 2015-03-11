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
using System.Text;
using System.Threading;
using OpenMobile.Plugin;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Controls access to serial ports to stop plugins trying to gain access to the serial ports at the same time
    /// </summary>
    static public class SerialAccess
    {
        static private EventWaitHandle _SerialReleased = new EventWaitHandle(true, EventResetMode.AutoReset);

        static private IBasePlugin _CurrentLockOwner;
        static private DateTime _LockTime;

        /// <summary>
        /// Requests serial ports access. Returns true if request was successfull, returns false if request timed out (15 seconds)
        /// <para>Remember to release the request with ReleaseAccess<seealso cref="ReleaseAccess"/></para>
        /// </summary>
        /// <returns></returns>
        static public bool GetAccess(IBasePlugin requestingPlugin)
        {

            // Check if lock has been held to long
            if (!_LockTime.Equals(new DateTime()))
            {
                if ((DateTime.Now - _LockTime) > new TimeSpan(0, 0, 15))
                {
                    // Lock is held to long
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Current lock held by {0} to long, forcing a release", _CurrentLockOwner));
                    _SerialReleased.Set();
                    _CurrentLockOwner = null;
                }
            }

            // If someone else holds the lock we just return false
            if (_CurrentLockOwner != null)
            {

                // Wait for access
                if (!_SerialReleased.WaitOne(15000))
                {
                    // Operation timed out, reset waitHandle
                    _SerialReleased.Set();
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Access denied to {0}, timed out waiting for {1}", requestingPlugin.pluginName, _CurrentLockOwner.pluginName));
                    return false;
                }

                //OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Access denied to {0}, Access currently held by {1}", requestingPlugin.pluginName, _CurrentLockOwner.pluginName));
                //return false;

            }

            // Set the current owner
            _CurrentLockOwner = requestingPlugin;
            _LockTime = DateTime.Now;
            _SerialReleased.Reset();

            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Access granted to {0}", requestingPlugin.pluginName));
            return true;

        }

        /// <summary>
        /// Releases the serial port access request.
        /// </summary>
        static public void ReleaseAccess(IBasePlugin requestingPlugin)
        {
            // Do we have anything to release?
            if (_CurrentLockOwner != null)
            {   // Yes
                if (_CurrentLockOwner.Equals(requestingPlugin))
                {
                    _CurrentLockOwner = null;
                    _SerialReleased.Set();
                    _LockTime = new DateTime();
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Access released by {0}", requestingPlugin));
                }
                else
                {
                    //if (_CurrentLockOwner != null)
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("SerialAccess: Release by {0} denied, Access currently held by {1}", requestingPlugin.pluginName, _CurrentLockOwner.pluginName));
                }
            }
        }

        /// <summary>
        /// Returns an string array with port names
        /// </summary>
        /// <returns></returns>
        static public string[] GetPortNames()
        {
            try
            {
                var ports = System.IO.Ports.SerialPort.GetPortNames();
                if (ports == null)
                    return new string[] { "No ports available" };
                return ports;
            }
            catch
            {
                return new string[] { "No ports available" };
            }
        }
    }
}