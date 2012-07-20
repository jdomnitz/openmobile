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

    Copyright 2011-2012 Jonathan Heizer jheizer@gmail.com
*********************************************************************************/

using System;
using System.Collections.Generic;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// The type of sensor
    /// </summary>
    [Flags]
    [System.Serializable]
    public enum eSensorType
    {
        /// <summary>
        /// Unknown Sensor
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Device receives data from the PC
        /// </summary>
        deviceReceivesData = 1,
        /// <summary>
        /// Device supplies data to the PC
        /// </summary>
        deviceSuppliesData = 2,
        /// <summary>
        /// Reserved
        /// </summary>
        Other = 4,
        /// <summary>
        /// All types
        /// </summary>
        All = 0xFF
    }

    /// <summary>
    /// Data type sensor returns
    /// </summary>
    [Flags]
    [System.Serializable]
    public enum eSensorDataType
    {
        /// <summary>
        /// Raw data
        /// </summary>
        raw = 0,
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
        binary,
        /// <summary>
        /// OMImage
        /// </summary>
        image
    }

    /// <summary>
    /// Represents a sensor
    /// </summary>
    [System.Serializable]
    public class Sensor
    {
        /// <summary>
        /// New data value event
        /// </summary>
        public event SensorDataReceived newSensorDataReceived;
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
        SensorForceReadDelegate ReadDelegate;
        SensorWriteDelegate WriteDelegate;

        /// <summary>
        /// Creates a new sensor that supplies data to the PC
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="ShortName"></param>
        /// <param name="DataType"></param>
        /// <param name="UpdateValueDel">Delegate the hardware plugin will call to update the sensor value</param>
        /// <param name="ForceReadDel">Optional Delegate to be called on a force read request</param>
        public Sensor(string Name, string ShortName, eSensorDataType DataType, out SensorSetNewValue UpdateValueDel, SensorForceReadDelegate ForceReadDel)
        {
            this.Name = Name;
            this.Type = eSensorType.deviceSuppliesData;
            this.ShortName = ShortName;
            this.DataType = DataType;
            this.ReadDelegate = ForceReadDel;
            UpdateValueDel = delegate(object NewValue)
            {
                if (!(sensorValue.Equals(NewValue)))
                {
                    sensorValue = NewValue;
                    if (newSensorDataReceived != null)
                        newSensorDataReceived(this);
                }
            };

        }

        /// <summary>
        /// Creates a new sensor that sends data to the hardware device
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="ShortName"></param>
        /// <param name="DataType"></param>
        /// <param name="WriteDel">Delegate the sensor will call to send the value to the hardware</param>
        public Sensor(string Name, string ShortName, eSensorDataType DataType, SensorWriteDelegate WriteDel)
        {
            this.Name = Name;
            this.Type = eSensorType.deviceReceivesData;
            this.ShortName = ShortName;
            this.DataType = DataType;
            this.WriteDelegate = WriteDel;
        }

        /// <summary>
        /// The current value of the sensor
        /// </summary>
        public object Value
        {
            get
            {
                if (Type == eSensorType.deviceSuppliesData)
                    if (sensorValue == null && ReadDelegate != null)
                        return ReadDelegate.Invoke(this);
                return sensorValue;
            }
            set
            {
                if (Type == eSensorType.deviceReceivesData)
                {
                    if (!sensorValue.Equals(value))
                    {
                        if (this.WriteDelegate != null)
                        {
                            if (WriteDelegate.Invoke(this, value))
                            {
                                sensorValue = value;
                                if (newSensorDataReceived != null)
                                    newSensorDataReceived(this);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Force the IRawHardware to read a new value from the device and return it instead of waiting for it to poll the device again
        /// </summary>
        /// <returns></returns>
        public object ForceReadValue()
        {
            if (this.Type == eSensorType.deviceSuppliesData)
            {
                if (ReadDelegate != null) //If the IRawHardware allows a force read update call it
                    sensorValue = ReadDelegate.Invoke(this);
                return sensorValue;
            }
            return null;
        }

        /// <summary>
        /// Format the sensor value to a pretty string
        /// </summary>
        /// <returns></returns>
        public string FormatedValue()
        {
            return FormatedValue(this);
        }
        /// <summary>
        /// Format the sensor value to a pretty string
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns></returns>
        public static string FormatedValue(Sensor sensor)
        {
            double dec;
            object val = sensor.Value;

            switch (sensor.DataType)
            {
                case eSensorDataType.Amps:
                    return val + "A";

                case eSensorDataType.binary:
                    if (val.ToString() == "1")
                    {
                        return "On";
                    }
                    return "Off";

                case eSensorDataType.bytes:
                    if (double.TryParse(val.ToString(), out dec))
                    {
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
                        return val.ToString() + "B";
                    }
                    return val.ToString();

                case eSensorDataType.bytespersec:
                    if (double.TryParse(val.ToString(), out dec))
                    {
                        if (dec > 1048576)
                        {
                            return (dec / 1048576).ToString("#.#") + "Mb";
                        }
                        if (dec > 1024)
                        {
                            return (dec / 1024).ToString("#.#") + "Kb";
                        }
                        return val.ToString() + "Bp";
                    }
                    return val.ToString();

                case eSensorDataType.degrees:
                    return val.ToString() + "°";

                case eSensorDataType.degreesC:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertToLocalTemp(dec, true).Replace(" ", "");
                    return val.ToString();

                case eSensorDataType.Gs:
                    return val.ToString() + "Gs";
                case eSensorDataType.kilometers:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertDistanceToLocal(dec, true);
                    return val.ToString();

                case eSensorDataType.kph:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertSpeedToLocal(dec, true);
                    return val.ToString();

                case eSensorDataType.meters:
                    return val.ToString() + "m";

                case eSensorDataType.percent:
                    return val.ToString() + "%";

                case eSensorDataType.psi:
                    return val.ToString() + "psi";

                case eSensorDataType.volts:
                    return val.ToString() + "V";
            }
            return val.ToString();
        }
    }

    /// <summary>
    /// Event raised when the value of the sensor has changed
    /// </summary>
    /// <param name="sender"></param>
    public delegate void SensorDataReceived(Sensor sender);

    /// <summary>
    /// Delegate that gets called to tell the IRawHardware to force request a new value and not wait for the next poll
    /// </summary>
    /// <param name="sensor"></param>
    /// <returns></returns>
    public delegate object SensorForceReadDelegate(Sensor sensor);

    /// <summary>
    /// Delegate that gets called to tell the IRawHardware to write out a new value to the device
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public delegate bool SensorWriteDelegate(Sensor sensor, object newValue);

    /// <summary>
    /// Called to send the new value from the hardware class to the sensor
    /// </summary>
    /// <param name="NewValue"></param>
    public delegate void SensorSetNewValue(object NewValue);


    /// <summary>
    /// Represents a sensor on the hardware plugin side to control access to setting the sensor value
    /// </summary>
    [System.Serializable]
    public class SensorWrapper
    {
        public Sensor sensor;
        public SensorSetNewValue UpdateValueDelegate;

        /// <summary>
        /// Creates a new sensor that supplies data to the PC
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="ShortName"></param>
        /// <param name="DataType"></param>
        /// <param name="ForceReadDel">Optional Delegate to be called on a force read request</param>
        public SensorWrapper(string Name, string ShortName, eSensorDataType DataType, SensorForceReadDelegate ForceReadDel)
        {
            this.sensor = new Sensor(Name, ShortName, DataType, out this.UpdateValueDelegate, ForceReadDel);
        }

        /// <summary>
        /// Creates a new sensor that sends data to the hardware device
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="ShortName"></param>
        /// <param name="WriteDel">Delegate the sensor will call to send the value to the hardware</param>
        /// <param name="DataType"></param>
        public SensorWrapper(string Name, string ShortName, SensorWriteDelegate WriteDel, eSensorDataType DataType)
        {
            this.sensor = new Sensor(Name, ShortName, DataType, WriteDel);
        }

        /// <summary>
        /// Returns the name of the sensor it holds
        /// </summary>
        public string Name
        {
            get { return sensor.Name; }
        }

        /// <summary>
        /// Updates the value of the underlying sensor
        /// </summary>
        /// <param name="Value">New Value</param>
        public void UpdateSensorValue(object Value)
        {
            if (UpdateValueDelegate != null)
                UpdateValueDelegate.Invoke(Value);
        }
    }

    /// <summary>
    /// Interface with I/O devices and other raw hardware
    /// </summary>
    public interface ISensorProvider : IBasePlugin
    {
        /// <summary>
        /// List available sensors of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<Sensor> getAvailableSensors(eSensorType type);
    }
}
