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

namespace OpenMobile
{
    /// <summary>
    /// A class for handling commands in OM
    /// </summary>
    public class CommandHandler
    {
        /// <summary>
        /// A delegate used for monitoring commands
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public delegate void CommandMonitorDelegate(Command command, object[] param, string name);

        private List<Command> _Commands = new List<Command>();

        /// <summary>
        /// Adds a new dataprovider
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(Command command)
        {
            // Check for valid command
            if (!command.Valid)
                return;

            lock (_Commands)
            {
                _Commands.Add(command);
            }
        }

        /// <summary>
        /// Returns a list of all available commands
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Command> GetCommands(string name)
        {
            return GetCommands(name, null, null);
        }

        /// <summary>
        /// Returns a list of all available commands where the returned list can be filtered
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiresParameters"></param>
        /// <param name="returnsValue"></param>
        /// <returns></returns>
        public List<Command> GetCommands(string name, bool? requiresParameters, bool? returnsValue)
        {
            return _Commands.FindAll(x => (x.FullName.ToLower().Contains(name.ToLower())
                && (requiresParameters != null ? x.RequiresParameters == requiresParameters : true)
                && (returnsValue != null ? x.ReturnsValue == returnsValue : true)
                ));
        }

        /// <summary>
        /// Gets a command
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Command GetCommand(string name)
        {
            string provider = String.Empty;
            if (name.Contains(Command.ProviderSeparator))
            {   // Extract provider
                provider = name.Substring(0, name.IndexOf(Command.ProviderSeparator));

                // Get the name part only
                name = name.Substring(provider.Length + 1);

                // Get datasource
                return _Commands.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Provider.ToLower() == provider.ToLower() && x.Valid));
            }
            
            // Get normal datasource
            Command command = _Commands.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Valid));

            // Log data
            if (command == null)
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Warning, "CommandHandler", String.Format("Command not available: {0}", name));

            return command;
        }

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public object ExecuteCommand(string name, object[] param, out bool result)
        {
            result = false;

            string[] names = name.Split(Command.CommandSeparator);
            if (names.Length > 1)
            {   // Execute multiple commands
                for (int i = 0; i < names.Length; i++)
                {
                    Command command = GetCommand(names[i]);
                    if (command == null)
                        continue;
                    command.Execute(param, out result);
                    Raise_CommandExecMonitor(command, param, name);
                }
                return null;
            }
            else
            {   // Execute single command
                Command command = GetCommand(names[0]);
                if (command == null)
                    return null;
                object retVal = command.Execute(param, out result);
                Raise_CommandExecMonitor(command, param, name);
                return retVal;
            }
        }

        /// <summary>
        /// Executes a command with no parameters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object ExecuteCommand(string name)
        {
            bool result = false;
            return ExecuteCommand(name, null, out result);
        }

        /// <summary>
        /// Executes a command with parameters but without result check
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public object ExecuteCommand(string name, object[] param)
        {
            bool result = false;
            return ExecuteCommand(name, param, out result);
        }

        /// <summary>
        /// Fires when a command is executed
        /// </summary>
        public event CommandMonitorDelegate CommandExecMonitor;

        /// <summary>
        /// Raises the command monitor event
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <param name="name"></param>
        private void Raise_CommandExecMonitor(Command command, object[] param, string name)
        {
            if (CommandExecMonitor != null)
                CommandExecMonitor(command, param, name);
        }
    }
}
