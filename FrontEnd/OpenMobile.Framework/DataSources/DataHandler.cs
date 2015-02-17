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
    public class DataHandler: IDisposable
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

        static private List<DataSourceSubscriptionCacheItem> _DataSourceSubscriptionCache = new List<DataSourceSubscriptionCacheItem>();
        static private List<DataSource> _DataSources = new List<DataSource>();
        static private Thread PollThread;
        static private int PollEngine_Resolution = 500;
        static private bool PollEngine_Run = false;
        static private bool PollEngine_Enable = false;
        static private EventWaitHandle PollEngine_WaitHandle;
        static private List<DataSourceGroup> _DataSourceGroups = new List<DataSourceGroup>();

        /// <summary>
        /// Creates a new data handler
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

        public void Dispose()
        {
            PollEngine_Run = false;
            if (PollThread != null)
            {
                PollEngine_Enable = false;
                PollEngine_WaitHandle.Set();
                PollThread.Abort();
                PollThread = null;
            }
        }

        /// <summary>
        /// Disposer
        /// </summary>
        ~DataHandler()
        {
            Dispose();
        }

        /// <summary>
        /// Get's or set's the value of a datasource (NB! The value returned is "null" if the datasource is not available).
        /// NB! When setting a datasource value the full name with provider must be used
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                object value;
                GetDataSourceValue(name, null, out value);
                return value;
            }
            set
            {
                PushDataSourceValue(name, value, true);
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
        /// Gets a DataSourceGroup
        /// </summary>
        /// <param name="dataSourceGroupName"></param>
        /// <returns></returns>
        private DataSourceGroup GetDataSourceGroup(string dataSourceGroupName)
        {
            return _DataSourceGroups.Find(x => x.FullNameWithoutScreen == dataSourceGroupName);
        }

        /// <summary>
        /// Activates a command group and replaces nameLevels at the same time.
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <returns></returns>
        public bool ActivateDataSourceGroup(DataSourceGroup dataSourceGroup, int screen = -1, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
        {
            dataSourceGroup.SetScreen(screen);
            dataSourceGroup.SetNameLevels(nameLevel1, nameLevel2, nameLevel3);
            return ActivateDataSourceGroup(dataSourceGroup);
        }

        /// <summary>
        /// Activates a screen specific datasource group on a specific screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool ActivateDataSourceGroup(int screen, DataSourceGroupScreenSpecific dataSourceGroup)
        {
            return ActivateDataSourceGroup(dataSourceGroup.Instances[screen]);
        }

        /// <summary>
        /// Activates a screen specific datasource group on all available screens
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool ActivateDataSourceGroup(DataSourceGroupScreenSpecific dataSourceGroup)
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                ActivateDataSourceGroup(dataSourceGroup.Instances[i]);
            }
            return true;
        }

        /// <summary>
        /// Activates a DataSourceGroup
        /// </summary>
        /// <param name="dataSourceGroup"></param>
        /// <returns></returns>
        public bool ActivateDataSourceGroup(DataSourceGroup dataSourceGroup)
        {
            lock (dataSourceGroup)
            {
                DataSourceGroup currentGroup = GetDataSourceGroup(dataSourceGroup.FullName);
                int currentIndex = _DataSourceGroups.IndexOf(currentGroup);

                if (currentGroup == dataSourceGroup)
                    return false;

                if (currentGroup != null)
                {   // Replace existing group
                    DataSourceGroup.ReplaceGroup(currentGroup, dataSourceGroup);
                    _DataSourceGroups[currentIndex] = dataSourceGroup;
                }
                else
                {   // New datasource group
                    _DataSourceGroups.Add(dataSourceGroup);

                    // Activate dataSources in group
                    DataSourceGroup.ActivateDataSourcesInGroup(this, dataSourceGroup);
                }
                return true;
            }
        }

        /// <summary>
        /// True = DataSourceGroup is active
        /// </summary>
        /// <param name="dataSourceGroup"></param>
        /// <returns></returns>
        public bool IsDataSourceGroupActive(DataSourceGroup dataSourceGroup)
        {
            return _DataSourceGroups.Contains(dataSourceGroup);
        }

        /// <summary>
        /// True = datasource is present
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool IsDataSourcePresent(DataSource dataSource)
        {
            return _DataSources.Find(x => x.FullNameWithoutScreen == dataSource.FullNameWithoutScreen) != null;
        }

        /// <summary>
        /// True = datasource is present
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool IsExactDataSourcePresent(DataSource dataSource)
        {
            return _DataSources.Find(x => x.FullNameWithProvider == dataSource.FullNameWithProvider) != null;
        }

        /// <summary>
        /// Adds a new datasource
        /// </summary>
        /// <param name="screenSpecific"></param>
        /// <param name="dataSource"></param>
        public DataSource[] AddDataSource(bool screenSpecific, DataSource dataSource)
        {
            DataSource[] retVal = new DataSource[OM.Host.ScreenCount];

            if (!screenSpecific)
                retVal = AddDataSource(dataSource);
            else
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                {
                    DataSource specificSource = (DataSource)((ICloneable)dataSource).Clone();
                    specificSource.Screen = i;
                    retVal[i] = AddDataSource_Internal(specificSource, false)[0];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Adds a new datasource
        /// </summary>
        /// <param name="screenSpecific"></param>
        /// <param name="dataSource"></param>
        /// <param name="initialValue"></param>
        public DataSource[] AddDataSource(bool screenSpecific, DataSource dataSource, object initialValue)
        {
            DataSource[] retVal = new DataSource[OM.Host.ScreenCount];

            if (!screenSpecific)
                retVal = AddDataSource(dataSource, initialValue);
            else
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                {
                    DataSource specificSource = (DataSource)((ICloneable)dataSource).Clone();
                    specificSource.Screen = i;
                    retVal[i] = AddDataSource_Internal(specificSource, initialValue, false)[0];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Adds a new dataprovider
        /// </summary>
        /// <param name="dataSource"></param>
        public DataSource[] AddDataSource(DataSource dataSource)
        {
            return AddDataSource_Internal(dataSource);
        }
        private DataSource[] AddDataSource_Internal(DataSource dataSource, bool addMultiScreen = true)
        {
            DataSource[] retVal = new DataSource[OM.Host.ScreenCount];

            if (dataSource.ScreenSpecific & addMultiScreen)
            {
                retVal = AddDataSource(true, dataSource);
                return retVal;
            }

            lock (_DataSources)
            {
                _DataSources.Add(dataSource);        // Add to internal sensor list
                
                // Get a fresh value from the sensor (Not needed if value is already valid and present)
                if (dataSource.Value != null)
                    dataSource.RefreshValue(dataSource.Value, false, true, true);
                else
                    dataSource.RefreshValue(null, true, true);

                // Prosess queue
                SubscriptionCache_Prosess(dataSource);
            }
            PollEngine_WaitHandle.Set();

            for (int i = 0; i < retVal.Length; i++)
                retVal[i] = dataSource;

            return retVal;
        }

        /// <summary>
        /// Adds a new dataprovider with initial value
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="initialValue"></param>
        public DataSource[] AddDataSource(DataSource dataSource, object initialValue)
        {
            return AddDataSource_Internal(dataSource, initialValue);
        }
        public DataSource[] AddDataSource_Internal(DataSource dataSource, object initialValue, bool addMultiScreen = true)
        {
            DataSource[] retVal = new DataSource[OM.Host.ScreenCount];
            
            if (dataSource.ScreenSpecific & addMultiScreen)
            {
                retVal = AddDataSource(true, dataSource, initialValue);
                return retVal;
            }

            lock (_DataSources)
            {
                if (!IsExactDataSourcePresent(dataSource))
                {
                    _DataSources.Add(dataSource);        // Add to internal sensor list

                    // Get a fresh value from the sensor
                    dataSource.RefreshValue(initialValue, false, true);

                    // Prosess queue
                    SubscriptionCache_Prosess(dataSource);
                }
            }
            PollEngine_WaitHandle.Set();

            for (int i = 0; i < retVal.Length; i++)
                retVal[i] = dataSource;

            return retVal;
        }

        ///// <summary>
        ///// Adds a new dataprovider with initial value
        ///// </summary>
        ///// <param name="dataSourceGroup"></param>
        ///// <param name="dataSource"></param>
        ///// <param name="initialValue"></param>
        //public DataSource[] AddDataSource(DataSourceGroup dataSourceGroup, DataSource dataSource, object initialValue)
        //{
        //    lock (dataSourceGroup)
        //    {
        //        // Replace name levels with information from DataSourceGroup
        //        dataSource = DataSourceGroup.ReplaceNameLevels(dataSourceGroup, dataSource);

        //        dataSourceGroup.DataSources.Add(dataSource);        // Add to internal sensor list

        //        // Get a fresh value from the sensor
        //        dataSource.RefreshValue(initialValue, false, true);

        //        // Prosess queue
        //        SubscriptionCache_Prosess(dataSource);
        //    }
        //    PollEngine_WaitHandle.Set();

        //    for (int i = 0; i < retVal.Length; i++)
        //        retVal[i] = dataSource;

        //    return retVal;
        //}

        /// <summary>
        /// Replaces one datasource with another one
        /// </summary>
        /// <param name="oldDataSource"></param>
        /// <param name="newDataSource"></param>
        /// <returns></returns>
        public bool ReplaceDataSource(DataSource oldDataSource, DataSource newDataSource)
        {
            // Remap subscriptions
            DataSource.Remap(oldDataSource, newDataSource);

            // Remap list data
            int index = _DataSources.IndexOf(oldDataSource);
            if (index >= 0)
                _DataSources[index] = newDataSource;

            return true;
        }

        /// <summary> 
        /// Removes a dataprovider 
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary> 
        /// <param name="name"></param>
        public bool RemoveDataSource(string name)
        {
            // Check for included provider reference
            if (!name.Contains(DataSource.ProviderSeparator))
                return false;

            lock (_DataSources)
            {
                DataSource toRemove = GetDataSource(name);
                if (toRemove == null)
                    return false;

                DataSourceSubscriptionCacheItem queuedItem = _DataSourceSubscriptionCache.Find(x => (x.Name == name));
                if (queuedItem != null)
                    _DataSourceSubscriptionCache.Remove(queuedItem);
                _DataSources.Remove(toRemove);
            }

            return true;
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
                return _DataSources.Find(x => (x.FullName.ToLower().Equals(name.ToLower()) && x.Provider.ToLower() == provider.ToLower() && x.Valid));
            }

            // Get normal datasource
            DataSource dataSource = _DataSources.Find(x => (x.FullName.ToLower().Equals(name.ToLower()) && x.Valid));

            if (dataSource == null && log)
            {
                // Log data
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "DataHandler", String.Format("Datasource not available: {0}", name));
            }
            return dataSource;
        }

        /// <summary>
        /// Forces a refresh of a datasource from the source (Don't use this method unless required as it can cause high system load if used to often)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RefreshDataSource(int screen, OpenMobile.Plugin.IBasePlugin provider, string name)
        {
            object value = null;
            return RefreshDataSource(String.Format("{0};{1}.{2}", provider.pluginName, DataNameBase.GetScreenString(screen), name), out value);
        }

        /// <summary>
        /// Forces a refresh of a datasource from the source (Don't use this method unless required as it can cause high system load if used to often)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RefreshDataSource(OpenMobile.Plugin.IBasePlugin provider, string name)
        {
            object value = null;
            return RefreshDataSource(String.Format("{0};{1}", provider.pluginName, name), out value);
        }

        /// <summary>
        /// Forces a refresh of a datasource from the source (Don't use this method unless required as it can cause high system load if used to often)
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RefreshDataSource(string name)
        {
            object value = null;
            return RefreshDataSource(name, out value);
        }
        /// <summary>
        /// Forces a refresh of a datasource from the source (Don't use this method unless required as it can cause high system load if used to often)
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RefreshDataSource(string name, out object value)
        {
            value = null;

            var dataSource = GetDataSource(name);
            if (dataSource == null)
                return false;

            var result = dataSource.RefreshValue(dataSource.Value, true, false, true);
            value = dataSource.Value;
            return result;
        }


        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or a full datasource name WITHOUT a provider (example: Zone.Volume)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T GetDataSourceValue<T>(string name, object[] param = null)
        {
            return GetDataSourceValue<T>(name, param, default(T));
        }

        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or a full datasource name WITHOUT a provider (example: Zone.Volume)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetDataSourceValue<T>(string name, object[] param, T defaultValue)
        {
            try
            {
                object returnValue;
                if (!GetDataSourceValue(name, param, out returnValue))
                    return defaultValue;
                return (T)returnValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or a full datasource name WITHOUT a provider (example: Zone.Volume)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T GetDataSourceValue<T>(int screen, string name, object[] param = null)
        {
            return GetDataSourceValue<T>(screen, name, param, default(T));
        }

        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or a full datasource name WITHOUT a provider (example: Zone.Volume)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetDataSourceValue<T>(int screen, string name, object[] param, T defaultValue)
        {
            try
            {
                object returnValue;
                if (!GetDataSourceValue(screen, name, param, out returnValue))
                    return defaultValue;
                return (T)returnValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
        /// <para>Name can be part of a datasources name or a full datasource name WITHOUT a provider (example: Zone.Volume)</para>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetDataSourceValue(int screen, string name, object[] param, out object value)
        {
            return GetDataSourceValue(String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name), param, out value);
        }
        /// <summary>
        /// Gets a datasource's value and supports sending parameters along in the request
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
        /// <param name="screen"></param>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="forcedUpdate">Set to true to force an update of the datasource regardless of the actual value has changed or not</param>
        /// <returns></returns>
        public bool PushDataSourceValue(int screen, string provider, string name, object value, bool forcedUpdate = false)
        {
            return PushDataSourceValue(String.Format("{0};{1}.{2}", provider, DataNameBase.GetScreenString(screen), name), value, forcedUpdate);
        }
        /// <summary>
        /// Pushes a value to a datasource without sending the value back to the source of the datasource (only if new value differs from the current one)
        /// <para>Update events will still be sent to anyone subscribing to the data</para>
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="forcedUpdate">Set to true to force an update of the datasource regardless of the actual value has changed or not</param>
        /// <returns></returns>
        public bool PushDataSourceValue(int screen, OpenMobile.Plugin.IBasePlugin provider, string name, object value, bool forcedUpdate = false)
        {
            return PushDataSourceValue(String.Format("{0};{1}.{2}", provider.pluginName, DataNameBase.GetScreenString(screen), name), value, forcedUpdate);
        }
        /// <summary>
        /// Pushes a value to a datasource without sending the value back to the source of the datasource (only if new value differs from the current one)
        /// <para>Update events will still be sent to anyone subscribing to the data</para>
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="forcedUpdate">Set to true to force an update of the datasource regardless of the actual value has changed or not</param>
        /// <returns></returns>
        public bool PushDataSourceValue(string provider, string name, object value, bool forcedUpdate = false)
        {
            return PushDataSourceValue(String.Format("{0};{1}", provider, name), value, forcedUpdate);
        }

        /// <summary>
        /// Pushes a value to a datasource without sending the value back to the source of the datasource (only if new value differs from the current one)
        /// <para>Update events will still be sent to anyone subscribing to the data</para>
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="forcedUpdate">Set to true to force an update of the datasource regardless of the actual value has changed or not</param>
        /// <returns></returns>
        public bool PushDataSourceValue(OpenMobile.Plugin.IBasePlugin provider, string name, object value, bool forcedUpdate = false)
        {
            return PushDataSourceValue(String.Format("{0};{1}", provider.pluginName, name), value, forcedUpdate);
        }

        /// <summary>
        /// Pushes a value to a datasource without sending the value back to the source of the datasource (only if new value differs from the current one)
        /// <para>Update events will still be sent to anyone subscribing to the data</para>
        /// <para>NB! This method requires the name to include a provider reference (example: OM;Screen0.Zone.Volume)</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="forcedUpdate">Set to true to force an update of the datasource regardless of the actual value has changed or not</param>
        /// <returns></returns>
        public bool PushDataSourceValue(string name, object value, bool forcedUpdate=false)
        {
            // Ensure we don't process empty data
            if (String.IsNullOrEmpty(name))
                return false;

            // Check for included provider reference
            if (!name.Contains(DataSource.ProviderSeparator))
                return false;

            DataSource source = GetDataSource_Internal(name, true);
            if (source != null)
                return source.RefreshValue(value, false, true, forcedUpdate);
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
                if (name.StartsWith(OpenMobile.Data.DataSource.DataTag_Screen))
                {
                    if (name.Substring(OpenMobile.Data.DataSource.DataTag_Screen.Length + 1, 1).Equals(".")) 
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
                // Look for multiscreen datasource
                bool multiScreen = false;
                for (int i = 0; i < OM.Host.ScreenCount; i++)
                {
                    string tmpName = String.Format("{0}.{1}", DataNameBase.GetScreenString(i), name);
                    dataSource = GetDataSource_Internal(tmpName, false);
                    if (dataSource != null)
                    {
                        dataSource.OnDataSourceChanged += onDataSourceChanged;
                        multiScreen = true;
                    }
                }
                if (multiScreen)
                    return true;

                if (dataSource == null)
                {
                    // Add this item to the subscription queue
                    if (UseQueue)
                        SubscriptionCache_Add(name, onDataSourceChanged);
                    return false;
                }
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
                        BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "DataHandler", String.Format("Datasource subscription cache, Cached item connected and removed from cache: {0}", ItemsToSubscribe[i].Name));
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
