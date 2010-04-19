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
using System.Collections.Generic;
using System;

namespace OpenMobile.Plugin
{
    /// <summary>
    /// Represents the type of setting to be displayed/set
    /// </summary>
    public enum SettingTypes 
    {
        /// <summary>
        /// Any type of string based setting
        /// </summary>
        Text,
        /// <summary>
        /// Any type of integer based setting
        /// </summary>
        Numeric,
        /// <summary>
        /// Any setting that requires a file path
        /// </summary>
        File,
        /// <summary>
        /// Any setting that requires a folder path
        /// </summary>
        Folder,
        /// <summary>
        /// Any setting which requires a user to select from a list of options
        /// </summary>
        MultiChoice,
        /// <summary>
        /// Any setting which requires a user to select from a range of values (ex: 1-100)
        /// </summary>
        Range
    }

    /// <summary>
    /// Represents a Setting for a plugin
    /// </summary>
    public sealed class Setting
    {
        /// <summary>
        /// The type of setting to be displayed/set
        /// </summary>
        public SettingTypes Type = SettingTypes.Text;
        /// <summary>
        /// The name of the setting to be set
        /// </summary>
        public string Name = "";
        /// <summary>
        /// A header/title for the setting
        /// </summary>
        public string Header = "";
        /// <summary>
        /// A description for the setting
        /// </summary>
        public string Description = "";
        /// <summary>
        /// The actual value to be displayed (default) or the value to be set
        /// </summary>
        public string Value = null;
        /// <summary>
        /// Options for a MultiChoice setting
        /// </summary>
        public List<string> Options = null;
        /// <summary>
        /// Possible value for a MultiChoice or range setting
        /// </summary>
        public List<string> Values = null;

        /// <summary>
        /// Creates a new Text, Numeric, File or Folder Setting
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <param name="Header"></param>
        /// <param name="Description"></param>
        /// <exception cref="ArgumentException"></exception>
        public Setting(SettingTypes Type, string Name, string Header, string Description)
        {
            if ((Type == SettingTypes.MultiChoice)||(Type==SettingTypes.Range))
                throw new ArgumentException("MutiChoice requires Options and Values");
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
        }
        /// <summary>
        /// Creates a new MultiChoice setting
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <param name="Header"></param>
        /// <param name="Description"></param>
        /// <param name="Options"></param>
        /// <param name="Values"></param>
        public Setting(SettingTypes Type, string Name, string Header, string Description, List<string> Options, List<string> Values)
        {
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
            this.Options = Options;
            this.Values = Values;
        }
        public static List<string> BooleanList = new List<string>(new string[] { "True", "False" });
    }
    /// <summary>
    /// A setting has changed
    /// </summary>
    /// <param name="setting"></param>
    public delegate void SettingChanged(Setting setting);
    public sealed class Settings:List<Setting>
    {
        public SettingChanged OnSettingChanged;

        public void changeSetting(Setting setting)
        {
            if (OnSettingChanged != null)
                OnSettingChanged(setting);
        }
        public string Title = "Settings";
    }
}
