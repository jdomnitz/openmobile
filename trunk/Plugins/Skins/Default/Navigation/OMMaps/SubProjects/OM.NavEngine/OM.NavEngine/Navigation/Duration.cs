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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OpenMobile.NavEngine
{
    public struct Duration : IComparable<Duration>, IEquatable<Duration>, IXmlSerializable
    {
        /// <summary>
        /// The duration value
        /// </summary>
        public TimeSpan Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (this._Value != value)
                {
                    this._Value = value;
                }
            }
        }
        private TimeSpan _Value;

        public Duration(TimeSpan timespan)
        {
            _Value = timespan;
        }

        public Duration(uint seconds)
        {
            _Value = TimeSpan.FromSeconds(seconds);
        }

        public Duration(string duration)
            : this(duration, CultureInfo.CurrentCulture)
        {
        }

        public Duration(string duration, CultureInfo culture)
        {
            // Parse string to a set of hours, minutes and seconds
            // Possible values: 
            //      5 days 17 hours
            //      52 mins
            //      1 min
            //      1 sec
            //      3 seconds
            //      1 day 10 hours

            // Anything to do?
            if (string.IsNullOrEmpty(duration))
            {
                // Return a blank distance in Imperial or English system
                _Value = TimeSpan.Zero;
                return;
            }

            // Default to the current culture if none is given
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            try
            {
                // Trim surrounding spaces and switch to uppercase
                duration = duration.Trim().ToUpper(CultureInfo.InvariantCulture).Replace(culture.NumberFormat.NumberGroupSeparator, string.Empty);

                uint totalSeconds = 0;
                string[] durationParts = duration.Split(' ');
                for (int i = 0; i < durationParts.Length; i++)
                {
                    uint value = 0;
                    if (uint.TryParse(durationParts[i], out value))
                    {   // This is a number, extract unit
                        if (i == durationParts.Length - 1)
                            break;

                        // Let's compare against next available part to get the unit 
                        i++;

                        if (durationParts[i] == "DAYS" || durationParts[i] == "DAY")
                            totalSeconds += value * 86400;
                        else if (durationParts[i] == "HOURS" || durationParts[i] == "HOUR")
                            totalSeconds += value * 3600;
                        else if (durationParts[i] == "MINS" || durationParts[i] == "MIN")
                            totalSeconds += value * 60;
                        else if (durationParts[i] == "SECONDS" || durationParts[i] == "SECOND")
                            totalSeconds += value;
                    }
                }
                _Value = TimeSpan.FromSeconds(totalSeconds);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid format", "value", ex);
            }
        }

        public override string ToString()
        {
            return GetReadableDuration(this);
        }

        static string GetReadableDuration(Duration durationValue)
        {
            string duration;
            TimeSpan value = durationValue._Value;

            if (value.TotalMinutes < 1)
                duration = value.Seconds + " Seconds";
            else if (value.TotalHours < 1)
            {
                if (value.Seconds > 0)
                    duration = value.Minutes + " Minutes, " + value.Seconds + " Seconds";
                else
                    duration = value.Minutes + " Minutes";
            }
            else if (value.TotalDays < 1)
            {
                if (value.Minutes > 0)
                    duration = value.Hours + " Hours, " + value.Minutes + " Minutes";
                else
                    duration = value.Hours + " Hours";
            }
            else
            {
                if (value.Hours > 0)
                    duration = value.Days + " Days, " + value.Hours + " Hours";
                else
                    duration = value.Days + " Days";
            }

            if (duration.StartsWith("1 Seconds") || duration.EndsWith(" 1 Seconds"))
                duration = duration.Replace("1 Seconds", "1 Second");

            if (duration.StartsWith("1 Minutes") || duration.EndsWith(" 1 Minutes"))
                duration = duration.Replace("1 Minutes", "1 Minute");

            if (duration.StartsWith("1 Hours") || duration.EndsWith(" 1 Hours"))
                duration = duration.Replace("1 Hours", "1 Hour");

            if (duration.StartsWith("1 Days"))
                duration = duration.Replace("1 Days", "1 Day");

            return duration;
        }

        #region Operators

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static Duration operator +(Duration left, Duration right)
        {
            return left.Add(right);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static Duration operator -(Duration left, Duration right)
        {
            return left.Subtract(right);
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <(Duration left, Duration right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <=(Duration left, Duration right)
        {
            return left.CompareTo(right) < 0 || left.Equals(right);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Duration left, Duration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Duration left, Duration right)
        {
            return !(left.Equals(right));
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >=(Duration left, Duration right)
        {
            return left.CompareTo(right) > 0 || left.Equals(right);
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >(Duration left, Duration right)
        {
            return left.CompareTo(right) > 0;
        }

        #endregion Operators

        #region Math Methods

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Duration Add(Duration value)
        {
            return new Duration(_Value + value.Value);
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Duration Add(TimeSpan value)
        {
            return new Duration(_Value + value);
        }

        /// <summary>
        /// Subtracts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Duration Subtract(Duration value)
        {
            return new Duration(_Value - value.Value);
        }

        /// <summary>
        /// Subtracts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Duration Subtract(TimeSpan value)
        {
            return new Duration(_Value - value);
        }

        /// <summary>
        /// Determines whether [is less than] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [is less than] [the specified value]; otherwise, <c>false</c>.</returns>
        public bool IsLessThan(Duration value)
        {
            return CompareTo(value) < 0;
        }

        /// <summary>
        /// Determines whether [is less than or equal to] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [is less than or equal to] [the specified value]; otherwise, <c>false</c>.</returns>
        public bool IsLessThanOrEqualTo(Duration value)
        {
            return CompareTo(value) < 0 || Equals(value);
        }

        /// <summary>
        /// Determines whether [is greater than] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [is greater than] [the specified value]; otherwise, <c>false</c>.</returns>
        public bool IsGreaterThan(Duration value)
        {
            return CompareTo(value) > 0;
        }

        /// <summary>
        /// Determines whether [is greater than or equal to] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [is greater than or equal to] [the specified value]; otherwise, <c>false</c>.</returns>
        public bool IsGreaterThanOrEqualTo(Duration value)
        {
            return CompareTo(value) > 0 || Equals(value);
        }

        #endregion Math Methods


        public int CompareTo(Duration other)
        {
            return _Value.CompareTo(other.Value);
        }

        public bool Equals(Duration other)
        {
            return _Value.Equals(other.Value);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // Move to the <Value> element
            if (!reader.IsStartElement("Value"))
                reader.ReadToDescendant("Value");

            //_units = (DistanceUnit)Enum.Parse(typeof(DistanceUnit), reader.ReadElementContentAsString(), true);
            _Value = TimeSpan.FromMilliseconds(reader.ReadElementContentAsDouble());
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteElementString("Units", _units.ToString());
            writer.WriteElementString("Value", _Value.TotalMilliseconds.ToString());
        }
    }
}
