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
        #region Static methods

        /// <summary>
        /// Gets the name without a screen reference
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public string GetNameWithoutScreen(string name)
        {
            if (name.ToLower().StartsWith("screen"))
                return name.Substring(name.IndexOf('.') + 1);
            return name;
        }

        #endregion

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
        public const string DataTag_Screen = "{:S:}";

        /// <summary>
        /// The string ID used as a placeholder for indicating that the command should be looped, exact functionallity of this command is up to the different implementations
        /// </summary>
        public const string DataTag_Loop = "{:L:}";

        /// <summary>
        /// The string ID used in front of screen specific names
        /// </summary>
        public const string ScreenString = "Screen";

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
        /// The currently assigned screen number for this command (or -1 if not used)
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public virtual int Screen
        {
            get
            {
                return this._Screen;
            }
            set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                }
            }
        }
        private int _Screen = -1;

        /// <summary>
        /// True = A screen is specified for this object
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool IsScreenSpecified
        {
            get
            {
                return _Screen >= 0;
            }
        }

        /// <summary>
        /// Sets or gets wheater this object is specific to a screen or not
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool ScreenSpecific
        {
            get
            {
                return this._ScreenSpecific;
            }
            set
            {
                if (this._ScreenSpecific != value)
                {
                    this._ScreenSpecific = value;
                }
            }
        }
        private bool _ScreenSpecific = false;

        /// <summary>
        /// The hashcode for this objet
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public int HashCode
        {
            get
            {
                return this.GetHashCode();
            }
        }
        

        /// <summary>
        /// The full name of this command with screen reference (if present) (Screen.Level1.Level2.Level3)
        /// </summary>
        public virtual string FullName
        {
            get
            {
                if (_Screen >= 0)
                    return String.Format("{0}.{1}", GetScreenString(_Screen), FullNameWithoutScreen);
                else
                    return FullNameWithoutScreen;
            }
        }

        /// <summary>
        /// Returns the formatted screen string
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        static public string GetScreenString(int screen)
        {
            return String.Format("{0}{1}", DataNameBase.ScreenString, screen);
        }

        /// <summary>
        /// Gets the full dataname with a screen reference in front
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static public string GetDataNameWithScreen(int screen, string name)
        {
            // Check for special dataref of screen present 
            if (name.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
            {   // Present, replace with screen reference
                if (name.StartsWith(OpenMobile.Data.DataSource.DataTag_Screen))
                {
                    if (name.Substring(OpenMobile.Data.DataSource.DataTag_Screen.Length, 1).Equals("."))
                        name = name.Replace(OpenMobile.Data.DataSource.DataTag_Screen, DataNameBase.GetScreenString(screen));
                    else
                        name = name.Replace(OpenMobile.Data.DataSource.DataTag_Screen, String.Format("{0}.", DataNameBase.GetScreenString(screen)));
                }
                else
                    name = name.Replace(OpenMobile.Data.DataSource.DataTag_Screen, screen.ToString());
            }
            else
            {   // Add screen tag in front
                name = String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name);
            }
            return name;
        }

        /// <summary>
        /// The full name of this command without screen reference (Level1.Level2.Level3)
        /// </summary>
        public virtual string FullNameWithoutScreen
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
        public virtual string Provider
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
        public virtual string NameLevel1
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
        public virtual string NameLevel2
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
        public virtual string NameLevel3
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
        public virtual string Description
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
        public virtual string FullNameWithProvider
        {
            get
            {
                return String.Format("{0}{1}{2}", _Provider, ProviderSeparator, FullName);
            }
        }
    }
}
