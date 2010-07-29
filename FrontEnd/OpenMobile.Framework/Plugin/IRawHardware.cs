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
namespace OpenMobile.Plugin
{
    [Flags]
    public enum eSensorType
    {
        Unknown=0,
        Input=1,
        Output=2,
        Other=4,
        All=0xFF
    }
    public struct Sensor
    {
        public string Name;
        public int PID;
        public eSensorType Type;
    }
    /// <summary>
    /// Interface with I/O devices and other raw hardware
    /// </summary>
    public abstract class IRawHardware:IBasePlugin
    {
        private static int lastMask;
        private static int generateMask()
        {
            lastMask++;
            return lastMask;
        }
        private static bool CheckPID(ref int PID, int mask)
        {
            if ((PID / 100000) == mask)
            {
                PID = PID % (mask * 100000);
                return true;
            }
            return false;
        }
        private static int ApplyMask(int PID, int mask)
        {
            return (mask * 100000) + PID;
        }
        /// <summary>
        /// List available sensors of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract List<Sensor> getAvailableSensors(eSensorType type);
        /// <summary>
        /// Set a sensor value (for output devices)
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool setValue(int PID, double value);
        /// <summary>
        /// Get a sensor value
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        public abstract double getValue(int PID);
        /// <summary>
        /// Reset the hardware device.
        /// </summary>
        public abstract void resetDevice();
        /// <summary>
        /// Plugin specific (vehicle information, hardware name, etc.)
        /// </summary>
        /// <returns></returns>
        public abstract string deviceInfo { get; }
        /// <summary>
        /// Firmware version of currently connected hardware
        /// </summary>
        /// <returns></returns>
        public abstract string firmwareVersion { get; }
        public abstract eLoadStatus initialize(IPluginHost host);
        public abstract Settings loadSettings();
        public abstract string authorName { get; }
        public abstract string authorEmail { get; }
        public abstract string pluginName{get;}
        public abstract float pluginVersion{get;}
        public abstract string pluginDescription{get;}
        public abstract bool incomingMessage(string message, string source);
        public abstract bool incomingMessage<T>(string message, string source, ref T data);
        public abstract void Dispose();
    }
}
