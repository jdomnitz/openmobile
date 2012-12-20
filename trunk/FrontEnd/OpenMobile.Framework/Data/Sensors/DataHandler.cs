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
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpenMobile.Data
{
    /// <summary>
    /// A handler for sensors
    /// </summary>
    public class DataHandler
    {
        private List<DataSource> _DataSources = new List<DataSource>();
        private Thread PollThread;
        private int PollEngine_Resolution = 100;
        private bool PollEngine_Run = false;
        private bool PollEngine_Enable = false;
        private EventWaitHandle PollEngine_WaitHandle;

        /// <summary>
        /// Creates a new datahandler
        /// </summary>
        public DataHandler()
        {
            // Spawn the thread for the polling engine
            PollEngine_Enable = true;
            PollThread = new Thread(PollEngine);
            PollThread.IsBackground = true;
            PollThread.Priority = ThreadPriority.BelowNormal;
            PollThread.Name = "SensorHandler.PollEngine";
            PollThread.Start();
            PollEngine_WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        /// <summary>
        /// Disposer
        /// </summary>
        ~DataHandler()
        {
            if (PollThread != null)
            {
                PollEngine_Enable = false;
                PollEngine_WaitHandle.Set();
                PollThread.Abort();
                PollThread = null;
            }
        }

        /// <summary>
        /// The polling engine for datasources
        /// </summary>
        private void PollEngine()
        {
            while (PollEngine_Enable)
	        {
                if (PollEngine_WaitHandle.WaitOne())
                {
                    if (PollEngine_Enable)
                    {
                        PollEngine_Run = true;
                        while (PollEngine_Run)
                        {
                            // Check if we have some data that want's to be polled
                            List<DataSource> DataSourcesToPoll = _DataSources.FindAll(x => (x.PollRate > 0));
                            if (DataSourcesToPoll != null)
                            {
                                // Get a list of data to refersh
                                List<DataSource> DataSourcesToRefresh = DataSourcesToPoll.FindAll(x => (x.IsPollRequired()));
                                if (DataSourcesToRefresh.Count != 0)
                                {
                                    // Loop trough data and get an updated value
                                    for (int i = 0; i < DataSourcesToRefresh.Count; i++)
                                        DataSourcesToRefresh[i].RefreshValue(null, true);
                                }
                            }
                            // Limit thread speed to not overload cpu
                            Thread.Sleep(PollEngine_Resolution);
                        }
                    }
                }
	        }
            
        }

        /// <summary>
        /// Adds a new dataprovider
        /// </summary>
        /// <param name="dataSource"></param>
        public void AddDataProvider(DataSource dataSource)
        {
            lock (_DataSources)
            {
                _DataSources.Add(dataSource);        // Add to internal sensor list
                // Get a fresh value from the sensor
                dataSource.RefreshValue(null, true);
            }
            PollEngine_WaitHandle.Set();
        }

        /// <summary>
        /// Gets all available datasources
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<DataSource> GetDataSources(string name)
        {
            return _DataSources.FindAll(x => x.FullName.ToLower().Contains(name.ToLower()));
        }
        /// <summary>
        /// Gets a datasource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataSource GetDataSource(string name)
        {
            string provider = String.Empty;
            if (name.Contains(DataSource.ProviderSeparator))
            {   // Extract provider
                provider = name.Substring(0, name.IndexOf(DataSource.ProviderSeparator));

                // Get the name part only
                name = name.Substring(provider.Length + 1);

                // Get datasource
                return _DataSources.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Provider.ToLower() == provider.ToLower() && x.Valid));
            }

            // Get normal datasource
            return _DataSources.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Valid));
        }

        /// <summary>
        /// Gets a datasource and supports sending parameters along in the request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetDataSourceValue(string name, object[] param, out object value)
        {
            value = null;
            DataSource source = GetDataSource(name);
            if (source != null)
                return source.GetValue(param, out value);
            return false;
        }

        /// <summary>
        /// Subscribes to updates for a datasource
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onDataSourceChanged"></param>
        /// <returns></returns>
        public bool SubscribeToDataSource(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            DataSource dataSource = GetDataSource(name);
            if (dataSource == null)
                return false;

            dataSource.OnDataSourceChanged += onDataSourceChanged;
            return true;
        }
        /// <summary>
        /// Unsubscribes to updates for a datasource
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onDataSourceChanged"></param>
        /// <returns></returns>
        public bool UnsubscribeFromDataSource(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            DataSource dataSource = GetDataSource(name);
            if (dataSource == null)
                return false;

            dataSource.OnDataSourceChanged -= onDataSourceChanged;
            return true;
        }

        /// <summary>
        /// Prosesses an InLine datasource string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string GetInLineDataSource(string s)
        {
            string StringToProsess = s;

            // Find matches for datasources
            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(@"\{.+?\}");
            System.Text.RegularExpressions.MatchCollection Matches = regEx.Matches(StringToProsess);

            // Loop trough the matches and get the data
            for (int i = 0; i < Matches.Count; i++)
            {
                string match = Matches[i].ToString();
                string dataSourceName = match.Replace("{", "").Replace("}", "");
                DataSource dataSource = GetDataSource(dataSourceName);
                if (dataSource != null)
                    StringToProsess = StringToProsess.Replace(string.Format("{{{0}}}", dataSource.FullName), dataSource.FormatedValue);
            }

            return StringToProsess;
        }

    }
}
