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
    public enum SettingTypes { Text, Numeric, File, Folder, MultiChoice,Range }
    public sealed class Setting
    {
        public SettingTypes Type = SettingTypes.Text;
        public string Name = "";
        public string Header = "";
        public string Description = "";
        public string Value = null;
        public List<string> Options = null;
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
    }
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
