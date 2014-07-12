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
using System.Linq;
using System.Text;

namespace OpenMobile.Data
{
    public class DataSourceGroupScreenSpecific : DataNameBase
    {
        /// <summary>
        /// The different group instances
        /// </summary>
        public DataSourceGroup[] Instances
        {
            get
            {
                return this._Instances;
            }
            set
            {
                if (this._Instances != value)
                {
                    this._Instances = value;
                }
            }
        }
        private DataSourceGroup[] _Instances;

        /// <summary>
        /// The name of this group
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        /// <summary>
        /// Creates a new command group
        /// </summary>
        /// <param name="name"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        public DataSourceGroupScreenSpecific(string name, string provider, string nameLevel1, string nameLevel2, string nameLevel3)
        {
            this._Name = name;
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;

            _Instances = new DataSourceGroup[OM.Host.ScreenCount];
            for (int i = 0; i < OM.Host.ScreenCount; i++)
                _Instances[i] = new DataSourceGroup(name, i, provider, nameLevel1, nameLevel2, nameLevel3);
        }

        /// <summary>
        /// Adds a screen specfic instance to this command group
        /// </summary>
        /// <param name="command"></param>
        public void AddDataSource(DataSource[] dataSource)
        {
            for (int i = 0; i < dataSource.Length; i++)
                _Instances[i].AddDataSource(dataSource[i]);
        }
    }

    /// <summary>
    /// A group handler for DataSources
    /// </summary>
    public class DataSourceGroup : DataNameBase
    {
        #region Static methods

        /// <summary>
        /// Replaces one datasource group with another
        /// </summary>
        /// <param name="currentGroup"></param>
        /// <param name="newGroup"></param>
        static public void ReplaceGroup(DataSourceGroup currentGroup, DataSourceGroup newGroup)
        {
            // Inform old group that it's about to be deactivated
            currentGroup.Raise_OnDeactivate();

            List<DataSource> unMappedDataSources = new List<DataSource>(currentGroup._DataSources);
            for (int i = 0; i < newGroup._DataSources.Count; i++)
            {
                // Try to find match in old group
                DataSource matchedDataSource = unMappedDataSources.Find(x => x.FullNameWithoutScreen == newGroup._DataSources[i].FullNameWithoutScreen);
                if (matchedDataSource != null)
                {   // We found a match, remap this data source
                    OM.Host.DataHandler.ReplaceDataSource(matchedDataSource, newGroup._DataSources[i]);
                    
                    // Remove this datasource from unmapped list
                    unMappedDataSources.Remove(matchedDataSource);
                }
            }

            // Set old group to non active
            currentGroup._Active = false;

            // Set new group to active
            newGroup._Active = true;

            // Disable all datasources still in the unmapped list
            for (int i = 0; i < unMappedDataSources.Count; i++)
                unMappedDataSources[i].Enabled = false;

            // Enable all datasources in the new group and push updates
            for (int i = 0; i < newGroup._DataSources.Count; i++)
            {
                newGroup._DataSources[i].Enabled = true;

                if (!OM.Host.DataHandler.IsDataSourcePresent(newGroup._DataSources[i]))
                    OM.Host.DataHandler.AddDataSource(newGroup._DataSources[i]);
                else
                    DataSource.PushUpdate(newGroup._DataSources[i].LinkedSource);
            }

            // Inform new group that it's been activated
            newGroup.Raise_OnActivate();
        }

        /// <summary>
        /// Activates datasources in a group
        /// </summary>
        /// <param name="dataHandler"></param>
        /// <param name="group"></param>
        static public void ActivateDataSourcesInGroup(DataHandler dataHandler, DataSourceGroup group)
        {
            foreach (var dataSource in group._DataSources)
                dataHandler.AddDataSource(dataSource);

            group.Active = true;

            // Inform group that it's been activated
            group.Raise_OnActivate();
        }

        /// <summary>
        /// Replaces namelevels in a DataSource with namelevels from a DataSourceGroup
        /// </summary>
        /// <param name="dataSourceGroup"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        static public DataSource ReplaceNameLevels(DataSourceGroup dataSourceGroup, DataSource dataSource)
        {
            // Transfer name levels from dataSourceGroup to datasource
            if (!String.IsNullOrEmpty(dataSourceGroup.Provider))
                dataSource.Provider = dataSourceGroup.Provider;
            if (!String.IsNullOrEmpty(dataSourceGroup.NameLevel1))
                dataSource.NameLevel1 = dataSourceGroup.NameLevel1;
            if (!String.IsNullOrEmpty(dataSourceGroup.NameLevel2))
                dataSource.NameLevel2 = dataSourceGroup.NameLevel2;
            if (!String.IsNullOrEmpty(dataSourceGroup.NameLevel3))
                dataSource.NameLevel3 = dataSourceGroup.NameLevel3;
            return dataSource;
        }

        #endregion

        /// <summary>
        /// Deactivation callback delegate
        /// </summary>
        public delegate void DataSourceGroupCallback(DataSourceGroup dataSourceGroup);

        /// <summary>
        /// Raised when the group is deactivated
        /// </summary>
        public event DataSourceGroupCallback OnDeactivation;
        /// <summary>
        /// Raised when the group is activated
        /// </summary>
        public event DataSourceGroupCallback OnActivation;

        /// <summary>
        /// The current datasources in this group
        /// </summary>
        public List<DataSource> DataSources
        {
            get
            {
                return this._DataSources;
            }
            set
            {
                if (this._DataSources != value)
                {
                    this._DataSources = value;
                }
            }
        }
        private List<DataSource> _DataSources = new List<DataSource>();

        /// <summary>
        /// The name of this group
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;        

        /// <summary>
        /// True = this group is currently active
        /// </summary>
        public bool Active
        {
            get
            {
                return this._Active;
            }
            set
            {
                if (this._Active != value)
                {
                    this._Active = value;
                }
            }
        }
        private bool _Active;

        /// <summary>
        /// Adds a datasource to this group from an array of screen specific datasources
        /// </summary>
        /// <param name="dataSource"></param>
        public void AddDataSource(DataSource[] dataSources)
        {
            if (!this.ScreenSpecific && !this.IsScreenSpecified)
                AddDataSource(dataSources[0]);
            else
                AddDataSource(dataSources[this.Screen]);
        }

        /// <summary>
        /// Registers a DataSource with this group
        /// </summary>
        /// <param name="localDataSource"></param>
        public void AddDataSource(DataSource dataSource)
        {
            DataSource localDataSource = null;

            if (base.IsScreenSpecified && dataSource.ScreenSpecific)
                localDataSource = OM.Host.DataHandler.GetDataSource(String.Format("{0}.{1}", DataNameBase.GetScreenString(base.Screen), dataSource.FullNameWithoutScreen));
            else
                localDataSource = dataSource;

            // Cancel if already present or null
            if (localDataSource == null || ContainsDataSource(localDataSource))
                return;

            DataSource clonedSource = (DataSource)((ICloneable)localDataSource).Clone();

            // Remove any subscribers that was present when cloning it 
            DataSource.ClearSubscribers(clonedSource);

            // Add reference to this group in case it's not already done
            localDataSource.Group = null;
            localDataSource.LinkedTarget = clonedSource;

            // Replace name levels with information from DataSourceGroup
            clonedSource = DataSourceGroup.ReplaceNameLevels(this, clonedSource);

            // Add reference to this group in case it's not already done
            clonedSource.Group = this;
            clonedSource.LinkedTarget = null;
            clonedSource.LinkedSource = localDataSource;
            clonedSource.PollRate = 0;
            clonedSource.Getter = null;
            clonedSource.ScreenSpecific = false;

            // Add the new dataSource
            if (!ContainsDataSource(clonedSource))
                _DataSources.Add(clonedSource);
        }

        /// <summary>
        /// Registers a DataSource with this group
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="initialValue"></param>
        public void AddDataSource(DataSource dataSource, object initialValue)
        {
            // Cancel if already present
            if (ContainsDataSource(dataSource))
                return;

            // Get a fresh value from the sensor
            dataSource.RefreshValue(initialValue, false, true);

            // Register datasource
            AddDataSource(dataSource);
        }
        /// <summary>
        /// Unregisters a DataSource from this group
        /// </summary>
        /// <param name="dataSource"></param>
        public void RemoveDataSource(DataSource dataSource)
        {
            _DataSources.Remove(dataSource);
        }

        /// <summary>
        /// Returns true if the specified DataSource is already present in this group
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool ContainsDataSource(DataSource dataSource)
        {
            return _DataSources.Contains(dataSource);
        }

        public void SetScreen(int screen)
        {
            base.Screen = screen;
            if (screen >= 0)
            {
                for (int i = 0; i < _DataSources.Count; i++)
                    _DataSources[i].Screen = screen;
            }
        }

        public void SetNameLevels(string nameLevel1, string nameLevel2, string nameLevel3)
        {
            if (nameLevel1 != null)
                this.NameLevel1 = nameLevel1;
            if (nameLevel2 != null)
                this.NameLevel2 = nameLevel2;
            if (nameLevel3 != null)
                this.NameLevel3 = nameLevel3;

            for (int i = 0; i < _DataSources.Count; i++)
                _DataSources[i] = DataSourceGroup.ReplaceNameLevels(this, _DataSources[i]);
        }

        /// <summary>
        /// Creates a new DataSource group
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        public DataSourceGroup(string name, string provider, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
        {
            this._Name = name;
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
        }

        /// <summary>
        /// Creates a screen specific group
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        public DataSourceGroup(string name, int screen, string provider, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
            : this(name, provider, nameLevel1, nameLevel2, nameLevel3)
        {
            Screen = screen;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}", this.FullNameWithProvider);
        }

        private void Raise_OnActivate()
        {
            if (OnActivation != null)
                OnActivation(this);
        }
        private void Raise_OnDeactivate()
        {
            if (OnDeactivation != null)
                OnDeactivation(this);
        }

    }
}
