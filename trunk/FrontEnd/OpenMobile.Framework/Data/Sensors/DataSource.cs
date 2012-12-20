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
using System.Text;
using OpenMobile;
using OpenMobile.Plugin;

namespace OpenMobile.Data
{
    /// <summary>
    /// Get data delegate, returns the current data
    /// <para>Set out variable "result" to true if operation was sucessfull otherwise set to false</para>
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="result"></param>
    /// <param name="param">An array with parameters to pass along to the source</param>
    /// <returns></returns>
    public delegate object DataSourceGetDelegate(DataSource sensor, out bool result, object[] param);

    /// <summary>
    /// Set data delegate. New value is passed in the value field
    /// <para>Set out variable "result" to true if operation was sucessfull otherwise set to false</para>
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="value"></param>
    /// <param name="param"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public delegate void DataSourceSetDelegate(DataSource sensor, ref object value, object[] param, out bool result);

    /// <summary>
    /// Datasource value changed or updated
    /// </summary>
    /// <param name="sensor"></param>
    public delegate void DataSourceChangedDelegate(DataSource sensor);


    /// <summary>
    /// Data source
    /// </summary>
    public class DataSource
    {
        /// <summary>
        /// The separator character used for a provider
        /// </summary>
        public static string ProviderSeparator = @";";

        /// <summary>
        /// The separator character used for type, category and name
        /// </summary>
        public static string Separator = @".";

        /// <summary>
        /// Event is raised when the data in the datasource changes
        /// </summary>
        public event DataSourceChangedDelegate OnDataSourceChanged
        {
            add
            {
                // Check if data is too old
                if (IsValueOld())
                    RefreshValue(null, true);

                //Raise the connected event to ensure the newly connected event is updated
                value(this);

                _OnDataSourceChanged += value;
            }
            remove
            {
                _OnDataSourceChanged -= value;
            }
        }
        private event DataSourceChangedDelegate _OnDataSourceChanged;

        private void Raise_OnDataSourceChanged()
        {
            DataSourceChangedDelegate handler = _OnDataSourceChanged;
            if (handler != null)
                handler(this);
        }

        #region Properties

        /// <summary>
        /// Does this source provide valid data?
        /// </summary>
        public bool Valid
        {
            get
            {
                return this._Valid;
            }
            set
            {
                if (this._Valid != value)
                {
                    this._Valid = value;
                }
            }
        }
        private bool _Valid;

        /// <summary>
        /// The full name of this DataSource (Provider.Category.Name)
        /// </summary>
        public string FullName
        {
            get
            {
                if (String.IsNullOrEmpty(_Name))
                    return String.Format("{0}{1}{2}", _Type, Separator, _Category);
                else
                    return String.Format("{0}{1}{2}{3}{4}", _Type, Separator, _Category, Separator, _Name);
            }
        }

        /// <summary>
        /// The current value for this DataSource
        /// </summary>
        public object Value
        {
            get
            {
                // Check if data is too old
                if (IsValueOld())
                    RefreshValue(null, true);

                return this._Value;
            }
            set
            {
                if (_AccessType == AccessTypes.Write || _AccessType == AccessTypes.Both)
                {
                    if (this._Value != value)
                    {
                        RefreshValue(value, false);
                    }
                }
            }
        }
        private object _Value;

        /// <summary>
        /// Returns the formated value as a string
        /// </summary>
        /// <returns></returns>
        public string FormatedValue
        {
            get
            {
                return GetFormatedValue(this);
            }
        }

        /// <summary>
        /// The datatype this datasource should be formated to
        /// </summary>
        public enum DataTypes
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
        /// The formated datatype of this datasource
        /// </summary>
        public DataTypes DataType
        {
            get
            {
                return this._DataType;
            }
            set
            {
                if (this._DataType != value)
                {
                    this._DataType = value;
                }
            }
        }
        private DataTypes _DataType;        

        /// <summary>
        /// The accesstypes for the DataSource
        /// </summary>
        public enum AccessTypes 
        { 
            /// <summary>
            /// Read and write access
            /// </summary>
            Both, 
            /// <summary>
            /// Read access only
            /// </summary>
            Read, 
            /// <summary>
            /// Write access only
            /// </summary>
            Write 
        }

        /// <summary>
        /// The accesstype for the DataSource
        /// </summary>
        public AccessTypes AccessType
        {
            get
            {
                return this._AccessType;
            }
            internal set
            {
                if (this._AccessType != value)
                {
                    this._AccessType = value;
                }
            }
        }
        private AccessTypes _AccessType = AccessTypes.Both;

        /// <summary>
        /// The Provider of this DataSource (usually plugin name, OM for data provided by OpenMobile framework)
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string Provider
        {
            get
            {
                return this._Provider;
            }
            internal set
            {
                if (this._Provider != value)
                {
                    this._Provider = value;
                }
            }
        }
        private string _Provider;

        /// <summary>
        /// The main type of this DataSource (System for OM provided data, could be weather for weather data)
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string Type
        {
            get
            {
                return this._Type;
            }
            internal set
            {
                if (this._Type != value)
                {
                    this._Type = value;
                }
            }
        }
        private string _Type;

        /// <summary>
        /// The category for this DataSource
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string Category
        {
            get
            {
                return this._Category;
            }
            internal set
            {
                if (this._Category != value)
                {
                    this._Category = value;
                }
            }
        }
        private string _Category;

        /// <summary>
        /// The name of this DataSource
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get
            {
                return this._Name;
            }
            internal set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;


        /// <summary>
        /// The pollrate for this DataSource in milliseconds
        /// <para>The minimum pollrate is 100 milliseconds</para>
        /// </summary>
        public double PollRate
        {
            get
            {
                return this._PollRate;
            }
            internal set
            {
                if (this._PollRate != value)
                {
                    this._PollRate = value;
                }
            }
        }
        private double _PollRate;

        /// <summary>
        /// The full name of this DataSource (Provider.Category.Name)
        /// </summary>
        public string FullNameWithProvider
        {
            get
            {
                return String.Format("{0}{1}{2}", _Provider, ProviderSeparator, FullName);
            }
        }

        /// <summary>
        /// The set delegate for this DataSource
        /// <para>This delegate provides a method for setting the value of the DataSource</para>
        /// </summary>
        internal DataSourceSetDelegate Setter
        {
            get
            {
                return this._Setter;
            }
            set
            {
                if (this._Setter != value)
                {
                    this._Setter = value;
                }
            }
        }
        private DataSourceSetDelegate _Setter;

        /// <summary>
        /// The get delegate for this DataSource
        /// <para>This delegate provides a method for getting the value of the DataSource</para>
        /// </summary>
        internal DataSourceGetDelegate Getter
        {
            get
            {
                return this._Getter;
            }
            set
            {
                if (this._Getter != value)
                {
                    this._Getter = value;
                }
            }
        }
        private DataSourceGetDelegate _Getter;

        /// <summary>
        /// The timestamp for last updatevalue
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return this._Timestamp;
            }
            internal set
            {
                if (this._Timestamp != value)
                {
                    this._Timestamp = value;
                }
            }
        }
        private DateTime _Timestamp;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        public DataSource()
        {
        }

        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="pollRate"></param>
        /// <param name="accessType"></param>
        /// <param name="dataType"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        public DataSource(string provider, string type, string category, string name, int pollRate, AccessTypes accessType, DataTypes dataType, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        {
            this._Provider = provider;
            this._Type = type;
            this._Category = category;
            this._Name = name;
            this._PollRate = pollRate;
            this._Setter = setter;
            this._Getter = getter;
            this._AccessType = accessType;
            this._DataType = dataType;
        }

        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="pollRate"></param>
        /// <param name="dataType"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        public DataSource(string provider, string type, string category, string name, int pollRate, DataTypes dataType, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        {
            this._Provider = provider;
            this._Type = type;
            this._Category = category;
            this._Name = name;
            this._PollRate = pollRate;
            this._Setter = setter;
            this._Getter = getter;
            this._DataType = dataType;
            this._AccessType = AccessTypes.Read;
        }
        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="pollRate"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        public DataSource(string provider, string type, string category, string name, int pollRate, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        {
            this._Provider = provider;
            this._Type = type;
            this._Category = category;
            this._Name = name;
            this._PollRate = pollRate;
            this._Setter = setter;
            this._Getter = getter;
            this._DataType = DataTypes.raw;
            this._AccessType = AccessTypes.Read;
        }
        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        public DataSource(string provider, string type, string category, string name, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        {
            this._Provider = provider;
            this._Type = type;
            this._Category = category;
            this._Name = name;
            this._PollRate = 0;
            this._Setter = setter;
            this._Getter = getter;
            this._DataType = DataTypes.raw;
            this._AccessType = AccessTypes.Read;
        }

        #endregion

        /// <summary>
        /// Is the current value to old and need an update?
        /// </summary>
        /// <returns></returns>
        internal bool IsValueOld()
        {
            // Get MS since last update
            double MSSinceLastUpdate = (DateTime.Now - _Timestamp).TotalMilliseconds;
            return MSSinceLastUpdate > _PollRate;
        }

        /// <summary>
        /// Does this data need to be polled?
        /// </summary>
        /// <returns></returns>
        internal bool IsPollRequired()
        {
            if (_OnDataSourceChanged == null)
                return false;
            return IsValueOld();
        }

        /// <summary>
        /// Refreshes the current value with data from the getter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="UseGetter"></param>
        /// <returns></returns>
        internal bool RefreshValue(object value, bool UseGetter)
        {
            try
            {
                _Valid = false;
                if (UseGetter)
                {
                    object newValue = Getter(this, out _Valid, null);
                    if (_Valid)
                        _Value = newValue;
                }
                else
                {
                    _Value = value;
                }

                // update timestamp to block this source from being polled to often even if the result is invalid
                _Timestamp = DateTime.Now;
                
                // Trigg event if result is valid
                if (_Valid)
                    Raise_OnDataSourceChanged();

                return _Valid;
            }
            catch
            {
                _Valid = false;
                return _Valid;
            }
        }

        /// <summary>
        /// Gets a value from the source and passes parameters along with the request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(object[] param, out object value)
        {
            bool result = false;
            value = null;
            object getterValue = Getter(this, out result, param);
            if (result)
                value = getterValue;
            return result;
        }

        /// <summary>
        /// Get's a string representing this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} [{1}({2})]", this.FullName, (this.Value == null ? "" : this.Value), this.FormatedValue);
        }

        /// <summary>
        /// Format the sensor value to a pretty string
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetFormatedValue(DataSource dataSource, object value)
        {
            double dec;
            object val = value;

            // Null check
            if (val == null)
                return "";

            switch (dataSource.DataType)
            {
                case DataTypes.Amps:
                    return val + "A";

                case DataTypes.binary:
                    if (val.ToString() == "1")
                    {
                        return "On";
                    }
                    return "Off";

                case DataTypes.bytes:
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

                case DataTypes.bytespersec:
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

                case DataTypes.degrees:
                    return val.ToString() + "°";

                case DataTypes.degreesC:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertToLocalTemp(dec, true).Replace(" ", "");
                    return val.ToString();

                case DataTypes.Gs:
                    return val.ToString() + "Gs";
                case DataTypes.kilometers:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertDistanceToLocal(dec, true);
                    return val.ToString();

                case DataTypes.kph:
                    if (double.TryParse(val.ToString(), out dec))
                        return Framework.Globalization.convertSpeedToLocal(dec, true);
                    return val.ToString();

                case DataTypes.meters:
                    return val.ToString() + "m";

                case DataTypes.percent:
                    return val.ToString() + "%";

                case DataTypes.psi:
                    return val.ToString() + "psi";

                case DataTypes.volts:
                    return val.ToString() + "V";
            }
            return val.ToString();
        }

        /// <summary>
        /// Format the sensor value to a pretty string
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static string GetFormatedValue(DataSource dataSource)
        {
            return GetFormatedValue(dataSource, dataSource.Value);
        }
    }
}
