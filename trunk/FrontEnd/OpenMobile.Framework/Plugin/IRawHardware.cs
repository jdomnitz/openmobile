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
    /// Event raised when the value of the sensor has changed
    /// </summary>
    /// <param name="sender"></param>
    public delegate void sensorDataReceived(Sensor sender); 

    /// <summary>
    /// Represents a sensor
    /// </summary>
    public class Sensor
    {
        /// <summary>
        /// New data value event
        /// </summary>
        public event sensorDataReceived newSensorDataReceived;
        /// <summary>
        /// Sensor Name
        /// </summary>
        public string Name;
        /// <summary>
        /// Senor value
        /// </summary>
        object sensorValue = "";
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
        /// <param name="Type"></param>
        /// <param name="ShortName"></param>
        /// <param name="DataType"></param>
        public Sensor(string Name, eSensorType Type, string ShortName, eSensorDataType DataType)
        {
            this.Name = Name;
            this.Type = Type;
            this.ShortName = ShortName;
            this.DataType = DataType; 
        }
        /// <summary>
        /// The current value of the sensor
        /// </summary>
        public object Value
        {
            get
            {
                return sensorValue;
            }
            set
            {
                if (!sensorValue.Equals(value))
                {
                    sensorValue = value;
                    if (newSensorDataReceived != null)
                        newSensorDataReceived(this);
                }
            }
        }
        /// <summary>
        /// Format the sensor value to a pretty string
        /// </summary>
        /// <returns></returns>
        public string FormatedValue()
        {
            double dec;

            switch (this.DataType )
            {
                case eSensorDataType.Amps:
                    return sensorValue + "Amps";

                case eSensorDataType.binary:
                    if (sensorValue.ToString() == "1")
                    {
                        return "On";
                    }
                    return "Off";

                case eSensorDataType.bytes:
                    dec = double.Parse(sensorValue.ToString());
                    if (dec > 1099511627776L)
                    {
                        return (dec / 1099511627776L).ToString("#.#") + "T";
                    }
                    if (dec > 1073741824)
                    {
                        return (dec / 1073741824).ToString("#.#") + "G";
                    }
                    if (dec > 1048576)
                    {
                        return (dec / 1048576).ToString("#.#") + "M";
                    }
                    if (dec > 1024)
                    {
                        return (dec / 1024).ToString("#.#") + "K";
                    }
                    return sensorValue + "B";

                case eSensorDataType.bytespersec:
                    dec = double.Parse(sensorValue.ToString());
                    if (dec > 1048576)
                    {
                        return (dec / 1048576).ToString("#.#") + "Mb";
                    }
                    if (dec > 1024)
                    {
                        return (dec / 1024).ToString("#.#") + "Kb";
                    }
                    return sensorValue + "Bp";

                case eSensorDataType.degrees:
                    return sensorValue + "°";

                case eSensorDataType.degreesC:
                    dec = double.Parse(sensorValue.ToString());
                    return Framework.Globalization.convertToLocalTemp(dec, true).Replace(" ", "");

                case eSensorDataType.Gs:
                    return sensorValue + "Gs";
                case eSensorDataType.kilometers:
                    dec = double.Parse(sensorValue.ToString());
                    return Framework.Globalization.convertDistanceToLocal(dec, true);

                case eSensorDataType.kph:
                    dec = double.Parse(sensorValue.ToString());
                    return Framework.Globalization.convertSpeedToLocal(dec, true);

                case eSensorDataType.meters:
                    return sensorValue + "m";

                case eSensorDataType.percent:
                    return sensorValue + "%";

                case eSensorDataType.psi:
                    return sensorValue + "psi";

                case eSensorDataType.volts:
                    return sensorValue + "V";
            }
            return sensorValue.ToString();
        }
    }
    /// <summary>
    /// Interface with I/O devices and other raw hardware
    /// </summary>
    public interface IRawHardware : IBasePlugin
    {
        /// <summary>
        /// List available sensors of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<Sensor> getAvailableSensors(eSensorType type);
        /// <summary>
        /// Set a sensor value (for output devices)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool setValue(string name, object value);
        /// <summary>
        /// Get a sensor value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object getValue(string name);
        /// <summary>
        /// Reset the hardware device.
        /// </summary>
        void resetDevice();
        /// <summary>
        /// Plugin specific (vehicle information, hardware name, etc.)
        /// </summary>
        /// <returns></returns>
        string deviceInfo { get; }
        /// <summary>
        /// Firmware version of currently connected hardware
        /// </summary>
        /// <returns></returns>
        string firmwareVersion { get; }
    }
}
