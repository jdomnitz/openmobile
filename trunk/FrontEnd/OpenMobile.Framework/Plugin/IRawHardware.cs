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
    /// <summary>
    /// The type of sensor
    /// </summary>
    [Flags]
    public enum eSensorType
    {
        /// <summary>
        /// Unknown Sensor
        /// </summary>
        Unknown=0,
        /// <summary>
        /// Device receives data from the PC
        /// </summary>
        deviceReceivesData=1,
        /// <summary>
        /// Device supplies data to the PC
        /// </summary>
        deviceSuppliesData=2,
        /// <summary>
        /// Reserved
        /// </summary>
        Other=4,
        /// <summary>
        /// All types
        /// </summary>
        All=0xFF
    }

    /// <summary>
    /// Data type sensor returns
    /// </summary>
    [Flags]
    public enum eSensorDataType
    {
        /// <summary>
        /// Raw data
        /// </summary>
        raw=0,
        /// <summary>
        /// Temperature
        /// </summary>
        degreesC,
        /// <summary>
        /// Degrees
        /// </summary>
        degrees,
        /// <summary>
        /// Percentage
        /// </summary>
        percent,
        /// <summary>
        /// Meters
        /// </summary>
        meters,
        /// <summary>
        /// Kilometers
        /// </summary>
        kilometers,
        /// <summary>
        /// Voltage
        /// </summary>
        volts,
        /// <summary>
        /// Amperage
        /// </summary>
        Amps,
        /// <summary>
        /// Kilometers per hour
        /// </summary>
        kph,
        /// <summary>
        /// G Force
        /// </summary>
        Gs,
        /// <summary>
        /// Pressure 
        /// </summary>
        psi,
        /// <summary>
        /// Rotations Per Minute
        /// </summary>
        RPM,
        /// <summary>
        /// Bytes of data
        /// </summary>
        bytes,
        /// <summary>
        /// Transfer rate
        /// </summary>
        bytespersec,
        /// <summary>
        /// Binary On/Off as 1/0
        /// </summary>
        binary 
    }

    /// <summary>
    /// Represents a sensor
    /// </summary>
    public struct Sensor
    {
        /// <summary>
        /// Sensor Name
        /// </summary>
        public string Name;
        /// <summary>
        /// Sensor PID (masked so its globally unique)
        /// </summary>
        public int PID;
        /// <summary>
        /// Sensor Type
        /// </summary>
        public eSensorType Type;
        /// <summary>
        /// Abbreviated name
        /// </summary>
        public string ShortName;
        /// <summary>
        /// Sensor return data type
        /// </summary>
        public eSensorDataType DataType;

        /// <summary>
        /// Creates a new sensor object
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="PID"></param>
        /// <param name="Type"></param>
        public Sensor(string Name, int PID, eSensorType Type) : this(Name, PID, Type, "", eSensorDataType.raw)
        {
        }
                /// <summary>
        /// Creates a new sensor object
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="PID"></param>
        /// <param name="Type"></param>
        /// <param name="ShortName"></param>
        /// <param name="DataType"></param>
        public Sensor(string Name, int PID, eSensorType Type, string ShortName, eSensorDataType DataType)
        {
            this.Name = Name;
            this.PID = PID;
            this.Type = Type;
            this.ShortName = ShortName;
            this.DataType = DataType; 
        }
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
        private static bool CheckPID(int PID, int mask)
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
        private static int RemoveMask(int maskedPID, int mask)
        {
            return maskedPID - (mask * 100000);
        }
        
        private int myMask;
        /// <summary>
        /// Generates a new PID mask
        /// </summary>
        protected void InitPIDMask()
        {
            myMask = generateMask();
        }
        /// <summary>
        /// Masks a PID using the previously generated mask
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        protected int MaskPID(int PID)
        {
            return ApplyMask(PID, myMask);
        }
        /// <summary>
        /// Removes a PID mask
        /// </summary>
        /// <param name="maskedPID"></param>
        /// <returns></returns>
        protected int UnmaskPID(int maskedPID)
        {
            return RemoveMask(maskedPID, myMask);
        }
        /// <summary>
        /// Checks if this plugin supported a PID
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        public bool TestPID(int PID)
        {
            return CheckPID(PID, myMask);
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
        public abstract bool setValue(int PID, object value);
        /// <summary>
        /// Get a sensor value
        /// </summary>
        /// <param name="PID"></param>
        /// <returns></returns>
        public abstract object getValue(int PID);
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
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public abstract eLoadStatus initialize(IPluginHost host);
        /// <summary>
        /// Load a settings object for the plugin
        /// </summary>
        /// <returns></returns>
        public abstract Settings loadSettings();
        /// <summary>
        /// Authors Name
        /// </summary>
        public abstract string authorName { get; }
        /// <summary>
        /// Authors Email
        /// </summary>
        public abstract string authorEmail { get; }
        /// <summary>
        /// Plugin Name (must mathc filename)
        /// </summary>
        public abstract string pluginName{get;}
        /// <summary>
        /// Plugin Version
        /// </summary>
        public abstract float pluginVersion{get;}
        /// <summary>
        /// Plugin Description
        /// </summary>
        public abstract string pluginDescription{get;}
        /// <summary>
        /// An inter-plugin message has been received
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public abstract bool incomingMessage(string message, string source);
        /// <summary>
        /// An inter-plugin message has been received
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool incomingMessage<T>(string message, string source, ref T data);
        /// <summary>
        /// Clean up and dispose resources
        /// </summary>
        public abstract void Dispose();
    }
}
