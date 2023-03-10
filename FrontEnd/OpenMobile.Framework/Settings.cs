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
        /// Same as text except uses a password char
        /// </summary>
        Password,
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
        Range,
        /// <summary>
        /// Represents a button (when clicked raises an OnSettingChanged event with a null value)
        /// </summary>
        Button
    }

    /// <summary>
    /// Represents a Setting for a plugin
    /// </summary>
    public sealed class Setting
    {
        /// <summary>
        /// A list of options or values for a setting
        /// </summary>
        public class OptionValueList : List<string>
        {
            /// <summary>
            /// Creates a new list
            /// </summary>
            public OptionValueList()
            {
            }

            /// <summary>
            /// Creates a new list and adds items to it
            /// </summary>
            /// <param name="param"></param>
            public OptionValueList(params string[] param)
            {
                foreach (string s in param)
                    this.Add(s);
            }

            /// <summary>
            /// Creates a new list with the names in the referenced enum
            /// </summary>
            /// <param name="enumType"></param>
            public OptionValueList(Type enumType)
            {
                if (enumType.BaseType != typeof(Enum))
                    return;

                string[] strings = Enum.GetNames(enumType);
                foreach (string s in strings)
                    this.Add(s);
            }

            /// <summary>
            /// Creates a new list with the names in the referenced enum
            /// </summary>
            /// <param name="enumType"></param>
            public OptionValueList(Enum enumData)
            {
                string[] strings = Enum.GetNames(enumData.GetType());
                foreach (string s in strings)
                    this.Add(s);
            }

            public OptionValueList(IEnumerable<string> texts)
            {
                this.AddRange(texts);
            }


        }

        #region static setting types

        /// <summary>
        /// Creates a new boolean setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static Setting BooleanSetting(string name, string header, string description, string currentValue)
        {
            return new Setting(SettingTypes.MultiChoice, name, header, description, Setting.BooleanList, Setting.BooleanList, currentValue);
        }
        /// <summary>
        /// Creates a new boolean setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static Setting BooleanSetting(string name, string header, string description, bool currentValue)
        {
            return new Setting(SettingTypes.MultiChoice, name, header, description, Setting.BooleanList, Setting.BooleanList, currentValue.ToString());
        }


        /// <summary>
        /// Creates a new folder selection setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static Setting FolderSelection(string name, string header, string description, string currentValue)
        {
            return new Setting(SettingTypes.Folder, name, header, description, currentValue);
        }

        /// <summary>
        /// Creates a new text entry setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static Setting TextEntry(string name, string header, string description, string currentValue)
        {
            return new Setting(SettingTypes.Text, name, header, description, currentValue);
        }

        /// <summary>
        /// Creates a new enum setting using the enum names as available options to the user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static Setting EnumSetting<T>(string name, string header, string description, string currentValue)
        {
            return new Setting(SettingTypes.MultiChoice, name,header, description, new Setting.OptionValueList(typeof(T)), currentValue);
        }

        /// <summary>
        /// Creates a new text list setting using the ToString representation for each item in the values enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="currentValue"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Setting TextList<T>(string name, string header, string description, string currentValue, IEnumerable<T> values)
        {
            List<string> stringValues = new List<string>();
            foreach (var value in values)
                stringValues.Add(value.ToString());
            return new Setting(SettingTypes.MultiChoice, name, header, description, stringValues, currentValue);
        }

        /// <summary>
        /// Creates a new button setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buttonText"></param>
        /// <returns></returns>
        public static Setting ButtonSetting(string name, string buttonText)
        {
            return new Setting(SettingTypes.Button, name, String.Empty, buttonText);
        }


        #endregion

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
        private string[] CurrentValues = new string[1];
        /// <summary>
        /// Options for a MultiChoice setting
        /// </summary>
        public List<string> Options = null;
        /// <summary>
        /// Possible value for a MultiChoice or range setting
        /// </summary>
        public List<string> Values = null;
        /// <summary>
        /// The actual value to be displayed (screen specific)
        /// </summary>
        public string[] CurrentValue
        {
            get
            {
                if (CurrentValues == null)
                    return new string[0];
                else
                    return CurrentValues;
            }
        }
        /// <summary>
        /// The actual value to be displayed (default) or the value to be set
        /// </summary>
        public string Value
        {
            get
            {
                if ((CurrentValues == null) || (CurrentValues.Length == 0))
                    return null;
                return CurrentValues[0];
            }
            set
            {
                CurrentValues = new string[] { value };
            }
        }
        /// <summary>
        /// Gets a value for the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public string getInstanceValue(int screen)
        {
            if ((CurrentValues == null) || (CurrentValues.Length == 0))
                return null;
            if ((screen < 0) || (screen > CurrentValues.Length - 1))
                return CurrentValues[0];
            return CurrentValues[screen];
        }
        /// <summary>
        /// Sets a value for the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="value"></param>
        public void setInstanceValue(int screen, string value)
        {
            if (screen < 0)
                return;
            if ((CurrentValues == null) || (CurrentValues.Length == 0))
                CurrentValues = new string[1];
            if (screen >= CurrentValues.Length)
                if (CurrentValues.Length == 1)
                {
                    CurrentValues[0] = value;
                    return;
                }
                else
                    Array.Resize<string>(ref CurrentValues, screen + 1);
            CurrentValues[screen] = value;
        }
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
            if ((Type == SettingTypes.MultiChoice) || (Type == SettingTypes.Range))
                throw new ArgumentException("MutiChoice requires Options and Values");
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
        }
        /// <summary>
        /// Creates a new Text, Numeric, File or Folder Setting
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <param name="Header"></param>
        /// <param name="Description"></param>
        /// <param name="Value"></param>
        public Setting(SettingTypes Type, string Name, string Header, string Description, string Value)
        {
            if ((Type == SettingTypes.MultiChoice) || (Type == SettingTypes.Range))
                throw new ArgumentException("MutiChoice requires Options and Values");
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
            this.Value = Value;
        }
        /// <summary>
        /// Creates a new MultiChoice or Range setting
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
        public Setting(SettingTypes Type, string Name, string Header, string Description, List<string> Options, string currentValue)
        {
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
            this.Options = Options;
            this.Values = Options;
            this.Value = currentValue;
        }

        /// <summary>
        /// Creates a new MultiChoice or Range setting
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <param name="Header"></param>
        /// <param name="Description"></param>
        /// <param name="Options"></param>
        /// <param name="Values"></param>
        /// <param name="currentValue"></param>
        public Setting(SettingTypes Type, string Name, string Header, string Description, List<string> Options, List<string> Values, string currentValue)
        {
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
            this.Options = Options;
            this.Values = Values;
            this.Value = currentValue;
        }
        /// <summary>
        /// Creates a new MultiChoice or Range setting
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Name"></param>
        /// <param name="Header"></param>
        /// <param name="Description"></param>
        /// <param name="Options"></param>
        /// <param name="Values"></param>
        /// <param name="currentValues"></param>
        public Setting(SettingTypes Type, string Name, string Header, string Description, List<string> Options, List<string> Values, string[] currentValues)
        {
            this.Type = Type;
            this.Name = Name;
            this.Header = Header;
            this.Description = Description;
            this.Options = Options;
            this.Values = Values;
            this.CurrentValues = currentValues;
        }

        /// <summary>
        /// Returns a string describing this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}({1})", this.Name, this.Value);
        }

        /// <summary>
        /// A List containing True and False
        /// </summary>
        public static List<string> BooleanList = new List<string>(new string[] { "True", "False" });
    }
    /// <summary>
    /// A setting has changed
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="screen"></param>
    public delegate void SettingChanged(int screen, Setting setting);
    /// <summary>
    /// A collection of Settings
    /// </summary>
    public sealed class Settings : List<Setting>
    {
        /// <summary>
        /// Occurs when a setting inside the collection is changed
        /// </summary>
        public event SettingChanged OnSettingChanged;
        /// <summary>
        /// Raises the OnSettingChanged event
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="screen"></param>
        public void changeSetting(int screen, Setting setting)
        {
            if (OnSettingChanged != null)
                OnSettingChanged(screen, setting);
        }
        /// <summary>
        /// The title for this settings collection
        /// </summary>
        public string Title = "Settings";
        /// <summary>
        /// Creates a new settings collection with the given title
        /// </summary>
        /// <param name="title"></param>
        public Settings(string title)
        {
            Title = title;
        }
        /// <summary>
        /// Returns the setting matching the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Setting this[string name]
        {
            get
            {
                return this.Find(p => p.Name == name);
            }
        }
    }
}
