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

namespace OpenMobile
{
    /// <summary>
    /// A base class for dataname usage in OpenMobile
    /// </summary>
    public abstract class DataNameBase
    {
        /// <summary>
        /// The separator character used for a provider
        /// </summary>
        public const string ProviderSeparator = @";";

        /// <summary>
        /// The separator character used for name levels
        /// </summary>
        public const string Separator = @".";

        /// <summary>
        /// The string ID used as a placeholder for screen numbers
        /// </summary>
        public const string DataTag_Screen = "{:screen:}";

        /// <summary>
        /// Returns a fully referenced name from the separate parts
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <returns></returns>
        public static string NameBuilder(string provider, string nameLevel1, string nameLevel2, string nameLevel3)
        {
            return String.Format("{0}{1}{2}{3}{4}{5}{6}", provider, ProviderSeparator, nameLevel1, Separator, nameLevel2, Separator, nameLevel3);
        }

        /// <summary>
        /// The full name of this command (Provider;Level1.Level2.Level3)
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
        /// The Provider of this command (usually plugin name, OM if provided by OpenMobile framework)
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
        protected string _Provider;

        /// <summary>
        /// The level 1 name (usally main type) of this command (System for OM provided, could be weather for weather data)
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string NameLevel1
        {
            get
            {
                return this._NameLevel1;
            }
            internal set
            {
                if (this._NameLevel1 != value)
                {
                    this._NameLevel1 = value;
                }
            }
        }
        protected string _NameLevel1;

        /// <summary>
        /// The level 2 name (usually category) for this command
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string NameLevel2
        {
            get
            {
                return this._NameLevel2;
            }
            internal set
            {
                if (this._NameLevel2 != value)
                {
                    this._NameLevel2 = value;
                }
            }
        }
        protected string _NameLevel2;

        /// <summary>
        /// The level 3 name (usally name) of this command
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public string NameLevel3
        {
            get
            {
                return this._NameLevel3;
            }
            internal set
            {
                if (this._NameLevel3 != value)
                {
                    this._NameLevel3 = value;
                }
            }
        }
        protected string _NameLevel3;

        /// <summary>
        /// Description 
        /// </summary>
        public string  Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                }
            }
        }
        protected string _Description;

        /// <summary>
        /// The full name of this data (Provider.Category.Name)
        /// </summary>
        public string FullNameWithProvider
        {
            get
            {
                return String.Format("{0}{1}{2}", _Provider, ProviderSeparator, FullName);
            }
        }
    }
}
