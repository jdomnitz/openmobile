﻿/*********************************************************************************
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
using OpenMobile.Data;
using OpenMobile.Graphics;

namespace OpenMobile.helperFunctions
{
    public static class StoredData
    {

        public static bool GetBool(string Name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(Name).ToLower().Equals("true");
        }
        public static bool SetBool(string Name, bool Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value.ToString());
        }

        public static string Get(string Name)
        {
            using (PluginSettings settings = new PluginSettings())
                return settings.getSetting(Name);
        }

        public static bool Set(string Name, string Value)
        {
            using (PluginSettings setting = new PluginSettings())
                return setting.setSetting(Name, Value);
        }

        public static void SetDefaultValue(string Setting, string Value)
        {
            // Set default values
            if (String.IsNullOrEmpty(StoredData.Get(Setting)))
                StoredData.Set(Setting, Value);
        }

    }
}
