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
using System.Threading;

namespace OpenMobile.Data
{
    /// <summary>
    /// A handler for sensors
    /// </summary>
    public class DataHandler
    {
        /// <summary>
        /// An dataholder for queued datasource subscriptions
        /// </summary>
        private class DataSourceSubscriptionCacheItem
        {
            public DataSourceChangedDelegate Delegate { get; set; }
            public string Name { get; set; }

            public DataSourceSubscriptionCacheItem(string name, DataSourceChangedDelegate d)
            {
                this.Delegate = d;
                this.Name = name;
            }
        }

        private List<DataSourceSubscriptionCacheItem> _DataSourceSubscriptionCache = new List<DataSourceSubscriptionCacheItem>();
        private List<DataSource> _DataSources = new List<DataSource>();
        private Thread PollThread;
        private int PollEngine_Resolution = 500;
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
            PollEngine_WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            PollThread.Start();
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
                            List<DataSource> DataSourcesToPoll = _DataSources.FindAll(x => (x.PollRate > 0 && x.Getter != null));
                            if (DataSourcesToPoll != null)
                            {
                                // Get a list of data to refresh
                                List<DataSource> DataSourcesToRefresh = DataSourcesToPoll.FindAll(x => (x.IsPollRequired()));
                                if (DataSourcesToRefresh.Count != 0)
                                {
                                    // Loop trough data and get an updated value
                                    for (int i = 0; i < DataSourcesToRefresh.Count; i++)
                                    {
                                        // Spawn new thread for event update
                                        //OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                                        //{
                                            try
                                            {
                                                DataSourcesToRefresh[i].RefreshValue(null, true, true);
                                            }
                                            catch
                                            { 
                                            }
                                        //});
                                    }
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
        public void AddDataSource(DataSource dataSource)
        {
            lock (_DataSources)
            {
                _DataSources.Add(dataSource);        // Add to internal sensor list
                
                // Get a fresh value from the sensor
                dataSource.RefreshValue(null, true, true);

                // Prosess queue
                SubscriptionCache_Prosess(dataSource);
            }
            PollEngine_WaitHandle.Set();
        }

        /// <summary>
        /// Adds a new dataprovider with initial value
        /// </summary>
        /// <param name="dataSource"></param>
        public void AddDataSource(DataSource dataSource, object initialValue)
        {
            lock (_DataSources)
            {
                _DataSources.Add(dataSource);        // Add to internal sensor list
                
                // Get a fresh value from the sensor
                dataSource.RefreshValue(initialValue, false, true);

                // Prosess queue
                SubscriptionCache_Prosess(dataSource);
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
        /// <para>Name can be part of a datasources name or it can be a full reference including a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataSource GetDataSource(string name)
        {
            // Ensure we don't prosess empty data
            if (String.IsNullOrEmpty(name))
                return null;

            return GetDataSource_Internal(name, true);
        }

        private DataSource GetDataSource_Internal(string name, bool log)
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
            DataSource dataSource = _DataSources.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Valid));

            if (dataSource == null && log)
            {
                // Log data
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "DataHandler", String.Format("Datasource not available: {0}", name));
            }
            return dataSource;
        }


        /// <summary>
        /// Gets a datasource and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or it can be a full reference including a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetDataSourceValue(string name, object[] param, out object value)
        {
            value = null;

            // Ensure we don't prosess empty data
            if (String.IsNullOrEmpty(name))
                return false;

            DataSource source = GetDataSource_Internal(name, true);
            if (source != null)
                return source.GetValue(param, out value);
            return false;
        }

        /// <summary>
        /// Pushes a value to a datasource without sending the value back to the source of the datasource (only if new value differs from the current one)
        /// <para>Update events will still be sent to anyone subscribing to the data</para>
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool PushDataSourceValue(string name, object value)
        {
            // Ensure we don't prosess empty data
            if (String.IsNullOrEmpty(name))
                return false;

            // Check for included provider reference
            if (!name.Contains(DataSource.ProviderSeparator))
                return false;

            DataSource source = GetDataSource_Internal(name, true);
            if (source != null)
                return source.RefreshValue(value, false, true);
            return false;
        }

        /// <summary>
        /// Subscribes to updates for a datasource
        /// <para>Name can be part of a datasources name or it can be a full reference including a provider reference (example: OM;Screen{:S:}.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onDataSourceChanged"></param>
        /// <returns></returns>
        public bool SubscribeToDataSource(int screen, string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            // Ensure we don't prosess empty data
            if (String.IsNullOrEmpty(name))
                return false;

            // Check for special dataref of screen present 
            if (name.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
            {   // Present, replace with screen reference
                name = name.Replace(OpenMobile.Data.DataSource.DataTag_Screen, screen.ToString());
            }

            return SubscribeToDataSource_Internal(name, onDataSourceChanged, true);
        }

        /// <summary>
        /// Subscribes to updates for a datasource
        /// <para>Name can be part of a datasources name or it can be a full reference including a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onDataSourceChanged"></param>
        /// <returns></returns>
        public bool SubscribeToDataSource(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            // Ensure we don't prosess empty data
            if (String.IsNullOrEmpty(name))
                return false;

            return SubscribeToDataSource_Internal(name, onDataSourceChanged, true);
        }

        private bool SubscribeToDataSource_Internal(string name, DataSourceChangedDelegate onDataSourceChanged, bool UseQueue)
        {
            DataSource dataSource = GetDataSource_Internal(name, true);
            if (dataSource == null)
            {                
                // Add this item to the subscription queue
                if (UseQueue)
                    SubscriptionCache_Add(name, onDataSourceChanged);
                return false;
            }

            dataSource.OnDataSourceChanged += onDataSourceChanged;
            return true;
        }

        /// <summary>
        /// Unsubscribes to updates for a datasource
        /// <para>Name can be part of a datasources name or it can be a full reference including a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onDataSourceChanged"></param>
        /// <returns></returns>
        public bool UnsubscribeFromDataSource(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            SubscriptionCache_Remove(name, onDataSourceChanged);

            DataSource dataSource = GetDataSource_Internal(name, false);
            if (dataSource == null)
                return false;

            dataSource.OnDataSourceChanged -= onDataSourceChanged;
            return true;
        }

        private void SubscriptionCache_Remove(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            // Remove item from subscription queue (if present)
            DataSourceSubscriptionCacheItem queuedItem = _DataSourceSubscriptionCache.Find(x => (x.Name == name && x.Delegate == onDataSourceChanged));
            if (queuedItem != null)
            {
                _DataSourceSubscriptionCache.Remove(queuedItem);
            }
        }

        private void SubscriptionCache_Add(string name, DataSourceChangedDelegate onDataSourceChanged)
        {
            // Add this item to the subscription queue
            _DataSourceSubscriptionCache.Add(new DataSourceSubscriptionCacheItem(name, onDataSourceChanged));

            // Log data
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "DataHandler", String.Format("Datasource subscription cache, Item added: {0}", name));
        }

        private void SubscriptionCache_Prosess(DataSource datasource)
        {
            lock (_DataSourceSubscriptionCache)
            {
                
                // Subscribe to items
                List<DataSourceSubscriptionCacheItem> ItemsToSubscribe = _DataSourceSubscriptionCache.FindAll(x => datasource.FullName.ToLower().Contains(x.Name.ToLower()));
                for (int i = 0; i < ItemsToSubscribe.Count; i++)
                {
                    if (SubscribeToDataSource_Internal(ItemsToSubscribe[i].Name, ItemsToSubscribe[i].Delegate, false))
                    {
                        // Remove items from queue
                        _DataSourceSubscriptionCache.Remove(ItemsToSubscribe[i]);
                        // Log data
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "DataHandler", String.Format("Datasource subscription cache, Cached item connected and removed from cache: {0}", ItemsToSubscribe[i].Name));
                    }
                }
            }
        }

        /// <summary>
        /// Prosesses an InLine datasource string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string GetInLineDataSource(string s)
        {
            string result = String.Empty;
            GetInLineDataSource(s, out result);
            return result;
        }
        
        /// <summary>
        /// Prosesses an InLine datasource string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="prosessedString"></param>
        /// <returns></returns>
        public bool GetInLineDataSource(string s, out string prosessedString)
        {
            string StringToProsess = s;
            prosessedString = String.Empty;

            // Find matches for datasources
            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(@"\{.+?\}");
            System.Text.RegularExpressions.MatchCollection Matches = regEx.Matches(StringToProsess);

            // Loop trough the matches and get the data
            for (int i = 0; i < Matches.Count; i++)
            {
                string match = Matches[i].ToString();
                string dataSourceName = match.Replace("{", "").Replace("}", "");
                DataSource dataSource = GetDataSource_Internal(dataSourceName, true);
                if (dataSource != null)
                    prosessedString = StringToProsess.Replace(string.Format("{{{0}}}", dataSource.FullName), dataSource.FormatedValue);
                else
                {   // Error, abort
                    break;
                }
            }

            return prosessedString != String.Empty;
        }

    }
}
