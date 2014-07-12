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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OMDataSourceTest
{
    public sealed class OMDataSourceTest : BasePluginCode, IDataSource
    {
        public OMDataSourceTest()
            : base("OMDataSourceTest", OM.Host.getSkinImage("Icons|Icon-DataSource"), 1f, "A sample plugin for a datasource", "OM Dev team", "")
        {
        }

        private DataSourceGroup dataSourceGroup1;
        private DataSourceGroup dataSourceGroup2;

        private CommandGroup commandGroup1;
        private CommandGroup commandGroup2;

        public override eLoadStatus initialize(IPluginHost host)
        {
            {
                dataSourceGroup1 = new DataSourceGroup("", 0, this.pluginName, "TestGroup", "TestSource", "");

                // Create datasources
                DataSource ds1 = new DataSource(true, this, "DataSource", "TestSource1", "Source1", 1000, DataSource.DataTypes.text, DataSource1_Get_DateTime, "A sample data source");
                OM.Host.DataHandler.AddDataSource(ds1, "Source1_Value1");

                DataSource ds2 = new DataSource(true, this, "DataSource", "TestSource1", "Source2", 0, DataSource.DataTypes.text, null, "A sample data source");
                OM.Host.DataHandler.AddDataSource(ds2, "Source1_Value2");

                dataSourceGroup1.AddDataSource(ds1);
                dataSourceGroup1.AddDataSource(ds2);
                host.DataHandler.ActivateDataSourceGroup(dataSourceGroup1);

                dataSourceGroup1.OnActivation += new DataSourceGroup.DataSourceGroupCallback(dataSourceGroup1_OnActivation);
                dataSourceGroup1.OnDeactivation += new DataSourceGroup.DataSourceGroupCallback(dataSourceGroup1_OnDeactivation);

                commandGroup1 = new CommandGroup("", 0, this.pluginName, "TestGroup", "TestCommand", "");

                Command c1 = new Command(true, this, "TestCommand", "CommandTarget1", "Change", CommandExecutor1, 0, false, "");
                OM.Host.CommandHandler.AddCommand(c1);

                commandGroup1.AddCommand(c1);
                host.CommandHandler.ActivateCommandGroup(commandGroup1);
            }

            {
                dataSourceGroup2 = new DataSourceGroup("", 0, this.pluginName, "TestGroup", "TestSource", "");

                // Create datasources
                DataSource ds1 = new DataSource(true, this, "DataSource", "TestSource2", "Source1", 1000, DataSource.DataTypes.text, DataSource2_Get_DateTime, "A sample data source");
                OM.Host.DataHandler.AddDataSource(true, ds1, "Source2_Value1");

                DataSource ds2 = new DataSource(true, this, "DataSource", "TestSource2", "Source2", 0, DataSource.DataTypes.text, null, "A sample data source");
                OM.Host.DataHandler.AddDataSource(true, ds2, "Source2_Value1");
                
                dataSourceGroup2.AddDataSource(ds1);
                dataSourceGroup2.AddDataSource(ds2);

                dataSourceGroup2.OnActivation += new DataSourceGroup.DataSourceGroupCallback(dataSourceGroup2_OnActivation);
                dataSourceGroup2.OnDeactivation += new DataSourceGroup.DataSourceGroupCallback(dataSourceGroup2_OnDeactivation);

                commandGroup2 = new CommandGroup("", 0, this.pluginName, "TestGroup", "TestCommand", "");

                Command c1 = new Command(true, this, "TestCommand", "CommandTarget2", "Change", CommandExecutor2, 0, false, "");
                OM.Host.CommandHandler.AddCommand(c1);

                commandGroup2.AddCommand(c1);

            }

            //OM.Host.CommandHandler.AddCommand(true, new Command(this, "TestSource", "DataGroupTest", "Change", CommandExecutor, 0, false, ""));

            return eLoadStatus.LoadSuccessful;
        }

        void dataSourceGroup2_OnDeactivation(DataSourceGroup dataSourceGroup)
        {
        }

        void dataSourceGroup2_OnActivation(DataSourceGroup dataSourceGroup)
        {
        }

        void dataSourceGroup1_OnDeactivation(DataSourceGroup dataSourceGroup)
        {
        }

        void dataSourceGroup1_OnActivation(DataSourceGroup dataSourceGroup)
        {
        }

        private object CommandExecutor1(Command command, object[] param, out bool result)
        {
            result = false;

            switch (command.FullNameWithoutScreen)
            {
                case "TestCommand.CommandTarget1.Change":
                    OM.Host.DataHandler.ActivateDataSourceGroup(dataSourceGroup2);
                    OM.Host.CommandHandler.ActivateCommandGroup(commandGroup2);
                    break;
            }
            return null;
        }

        private object CommandExecutor2(Command command, object[] param, out bool result)
        {
            result = false;

            switch (command.FullNameWithoutScreen)
            {
                case "TestCommand.CommandTarget2.Change":
                    OM.Host.DataHandler.ActivateDataSourceGroup(dataSourceGroup1);
                    OM.Host.CommandHandler.ActivateCommandGroup(commandGroup1);
                    break;
            }
            return null;
        }

        /// <summary>
        /// The "Get" method for the dataSource defined in the initialize method
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="result"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private object DataSource1_Get_DateTime(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            return String.Format("Source 1: {0}", DateTime.Now.ToString());
        }

        private object DataSource2_Get_DateTime(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            return String.Format("Source 2: {0}", DateTime.Now.ToString());
        }
    }
}
