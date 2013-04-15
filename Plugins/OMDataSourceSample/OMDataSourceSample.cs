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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OMDataSourceSample
{
    public sealed class OMDataSourceSample : IBasePlugin, IDataSource
    {
        #region IBasePlugin Members

        public eLoadStatus initialize(IPluginHost host)
        {
            // Create a datasource
            host.DataHandler.AddDataSource(new DataSource(this.pluginName, "DateTime", "Current", "", 1000, DataSource.DataTypes.text, DataSource_Get_DateTime, "A sample data source"));

            return eLoadStatus.LoadSuccessful;
        }

        /// <summary>
        /// The "Get" method for the dataSource defined in the initialize method
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="result"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private object DataSource_Get_DateTime(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            return DateTime.Now.ToString();
        }

        public Settings loadSettings()
        {
            return null;
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-DataSource"); }
        }

        public string authorName
        {
            get { return "OM DevTeam/Borte"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "OMDataSourceSample"; }
        }

        public float pluginVersion
        {
            get { return 1.0f; }
        }

        public string pluginDescription
        {
            get { return "Sample code for a datasource"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
