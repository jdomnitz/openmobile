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
    /// A delegate for manually setting a datasource
    /// </summary>
    /// <param name="value"></param>
    public delegate void DataSourceManualUpdateDelegate(object value);


    /// <summary>
    /// Data source
    /// </summary>
    public class DataSource : DataNameBase
    {
        /// <summary>
        /// Event is raised when the data in the datasource changes
        /// </summary>
        public event DataSourceChangedDelegate OnDataSourceChanged
        {
            add
            {
                // Check if data is too old
                if (IsValueOld())
                    RefreshValue(null, true, false);

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

        private void Raise_OnDataSourceChanged(bool spawn)
        {
            DataSourceChangedDelegate handler = _OnDataSourceChanged;
            if (handler != null)
            {
                if (spawn)
                {
                    // Spawn new thread for event update
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        handler(this);
                    });
                }
                else
                    handler(this);
            }
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
        /// Is someone subscribing to this data
        /// </summary>
        public bool IsSubcribed
        {
            get
            {
                return _OnDataSourceChanged != null;
            }
        }

        /// <summary>
        /// The full name of this DataSource (Provider;Level1.Level2.Level3)
        /// </summary>
        public string FullName
        {
            get
            {
                if (String.IsNullOrEmpty(_NameLevel3))
                    return String.Format("{0}{1}{2}", _NameLevel1, Separator, _NameLevel2);
                else
                    return String.Format("{0}{1}{2}{3}{4}", _NameLevel1, Separator, _NameLevel2, Separator, _NameLevel3);
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
                    RefreshValue(null, true, false);

                return this._Value;
            }
            set
            {
                if (_AccessType == AccessTypes.Write || _AccessType == AccessTypes.Both)
                {
                    if (this._Value != value)
                    {
                        RefreshValue(value, false, false);
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
            image,
            /// <summary>
            /// Plain text
            /// </summary>
            text
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
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="dataType"></param>
        /// <param name="description"></param>
        public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, DataTypes dataType, string description)
            : this(provider, nameLevel1, nameLevel2, nameLevel3, 0, dataType, null, description)
        {
        }

        /// <summary>
        /// Creates a new DataSource object
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="pollRate"></param>
        /// <param name="dataType"></param>
        /// <param name="getter"></param>
        public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, int pollRate, DataTypes dataType, DataSourceGetDelegate getter, string description)
        {
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
            this._PollRate = pollRate;
            this._Setter = null;
            this._Getter = getter;
            this._AccessType =  AccessTypes.Read;
            this._DataType = dataType;
            this._Description = description;
        }


        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="pollRate"></param>
        ///// <param name="accessType"></param>
        ///// <param name="dataType"></param>
        ///// <param name="setter"></param>
        ///// <param name="getter"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, int pollRate, AccessTypes accessType, DataTypes dataType, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        //{
        //    this._Provider = provider;
        //    this._NameLevel1 = nameLevel1;
        //    this._NameLevel2 = nameLevel2;
        //    this._NameLevel3 = nameLevel3;
        //    this._PollRate = pollRate;
        //    this._Setter = setter;
        //    this._Getter = getter;
        //    this._AccessType = accessType;
        //    this._DataType = dataType;
        //}

        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="pollRate"></param>
        ///// <param name="accessType"></param>
        ///// <param name="dataType"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, DataTypes dataType)
        //{
        //    this._Provider = provider;
        //    this._NameLevel1 = nameLevel1;
        //    this._NameLevel2 = nameLevel2;
        //    this._NameLevel3 = nameLevel3;
        //    this._PollRate = 0;
        //    this._Setter = null;
        //    this._Getter = null;
        //    this._AccessType = AccessTypes.Read;
        //    this._DataType = dataType;
        //}

        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="pollRate"></param>
        ///// <param name="dataType"></param>
        ///// <param name="setter"></param>
        ///// <param name="getter"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, int pollRate, DataTypes dataType, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        //{
        //    this._Provider = provider;
        //    this._NameLevel1 = nameLevel1;
        //    this._NameLevel2 = nameLevel2;
        //    this._NameLevel3 = nameLevel3;
        //    this._PollRate = pollRate;
        //    this._Setter = setter;
        //    this._Getter = getter;
        //    this._DataType = dataType;
        //    this._AccessType = AccessTypes.Read;
        //}
        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="pollRate"></param>
        ///// <param name="dataType"></param>
        ///// <param name="setter"></param>
        ///// <param name="getter"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, int pollRate, DataTypes dataType, DataSourceSetDelegate setter, DataSourceGetDelegate getter, out DataSourceManualUpdateDelegate manual)
        //    : this(provider, nameLevel1, nameLevel2, nameLevel3, pollRate, dataType, setter, getter)
        //{
        //    manual = new DataSourceManualUpdateDelegate(manualSetterMethod);
        //}
        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="pollRate"></param>
        ///// <param name="setter"></param>
        ///// <param name="getter"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, int pollRate, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        //{
        //    this._Provider = provider;
        //    this._NameLevel1 = nameLevel1;
        //    this._NameLevel2 = nameLevel2;
        //    this._NameLevel3 = nameLevel3;
        //    this._PollRate = pollRate;
        //    this._Setter = setter;
        //    this._Getter = getter;
        //    this._DataType = DataTypes.raw;
        //    this._AccessType = AccessTypes.Read;
        //}
        ///// <summary>
        ///// Creates a new DataSource object
        ///// </summary>
        ///// <param name="provider"></param>
        ///// <param name="nameLevel1"></param>
        ///// <param name="nameLevel2"></param>
        ///// <param name="nameLevel3"></param>
        ///// <param name="setter"></param>
        ///// <param name="getter"></param>
        //public DataSource(string provider, string nameLevel1, string nameLevel2, string nameLevel3, DataSourceSetDelegate setter, DataSourceGetDelegate getter)
        //{
        //    this._Provider = provider;
        //    this._NameLevel1 = nameLevel1;
        //    this._NameLevel2 = nameLevel2;
        //    this._NameLevel3 = nameLevel3;
        //    this._PollRate = 0;
        //    this._Setter = setter;
        //    this._Getter = getter;
        //    this._DataType = DataTypes.raw;
        //    this._AccessType = AccessTypes.Read;
        //}

        #endregion

        #region value and polling

        /// <summary>
        /// Is the current value to old and need an update?
        /// </summary>
        /// <returns></returns>
        internal bool IsValueOld()
        {
            // No poll if no pollrate is specified
            if (PollRate == 0)
                return false;
            
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
            // No poll if no pollrate is specified
            if (PollRate == 0)
                return false;

            if (_OnDataSourceChanged == null)
                return false;
            return IsValueOld();
        }

        /// <summary>
        /// Refreshes the current value with data from the getter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useGetter"></param>
        /// <returns></returns>
        internal bool RefreshValue(object value, bool useGetter, bool spawn)
        {
            try
            {
                _Valid = false;
                bool Changed = false;
                
                // Don't use getter if it's not present
                if (_Getter == null)
                    useGetter = false;

                if (useGetter)
                {
                    if (_Getter != null)
                    {
                        object newValue = _Getter(this, out _Valid, null);
                        if (_Valid)
                            if (!newValue.Equals(_Value))
                            {
                                _Value = newValue;
                                Changed = false;
                            }
                    }
                }
                else
                {
                    _Valid = true;
                    if (!value.Equals(_Value))
                    {
                        _Value = value;
                        Changed = true;
                    }
                }

                // update timestamp to block this source from being polled to often even if the result is invalid
                _Timestamp = DateTime.Now;
                
                // Trigg event if result is valid
                if (_Valid && Changed)
                    Raise_OnDataSourceChanged(spawn);

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
        /// Manually sets the value of a datasource WITHOUT passing the set value on to the datasource
        /// <para>This is provided as a way for datasources to manually update it's value. Use DataSource.Value to access it via "Setter" and "Getter"</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(object value)
        {
            return RefreshValue(value, false, true);
        }

        private void manualSetterMethod(object value)
        {
            RefreshValue(value, false, true);
        }

        #endregion

        /// <summary>
        /// Get's a string representing this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} [{1}({2})]", this.FullName, (this.Value == null ? String.Empty : this.Value), this.FormatedValue);
        }

        #region Formatted value

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
                return String.Empty;

            switch (dataSource.DataType)
            {
                case DataTypes.Amps:
                    return val + "A";

                case DataTypes.binary:
                    if (val.ToString() == "1" || (val is bool && (bool)val == true))
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

        #endregion
    }
}
