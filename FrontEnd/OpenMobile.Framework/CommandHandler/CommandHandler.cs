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

        public void Dispose()
        {

        }

        /// <summary>
        /// Adds a new command provider and returns the different instances of the commands
        /// </summary>
        /// <param name="screenSpecific"></param>
        /// <param name="command"></param>
        public Command[] AddCommand(bool screenSpecific, Command command)
        {
            Command[] retVal = new Command[OM.Host.ScreenCount];

            if (!screenSpecific)
                retVal = AddCommand(command);
            else
            {
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                {
                    Command specificCommand = (Command)((ICloneable)command).Clone();
                    specificCommand.Screen = i;
                    retVal[i] = AddCommand_Internal(specificCommand, false)[0];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Adds a new command provider and returns the different instances of the commands
        /// </summary>
        /// <param name="command"></param>
        public Command[] AddCommand(Command command)
        {
            return AddCommand_Internal(command);
        }

        private Command[] AddCommand_Internal(Command command, bool addMultiScreen = true)
        {
            Command[] retVal = new Command[OM.Host.ScreenCount];
            // Check for valid command
            if (!command.Valid)
                return retVal;

            if (command.ScreenSpecific && addMultiScreen)
            {
                return AddCommand(true, command);
            }

            lock (_Commands)
            {
                if (!IsExactCommandPresent(command))
                {
                    _Commands.Add(command);
                    for (int i = 0; i < retVal.Length; i++)
                        retVal[i] = command;
                }
            }
            return retVal;
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

                // Get command
                return _Commands.Find(x => (x.FullName.ToLower().Contains(name.ToLower()) && x.Provider.ToLower() == provider.ToLower()));
            }
            
            // Get normal command
            Command command = _Commands.Find(x => (x.FullName.ToLower().Contains(name.ToLower())));

            // if command is not found directly, try to remove screen part to find command
            if (command == null)
                command = _Commands.Find(x => (x.FullName.ToLower().Contains(DataNameBase.GetNameWithoutScreen(name).ToLower())));

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
        /// Executes a command
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public object ExecuteCommand(int screen, string name, object[] param, out bool result)
        {
            if (name.Contains(";"))
            {
                string provider = name.Substring(0, name.IndexOf(';'));
                name = name.Remove(0, provider.Length+1);
                return ExecuteCommand(String.Format("{0};{1}", provider, String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name)), param, out result);
            }
            else
            {
                return ExecuteCommand(String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name), param, out result);
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
        /// Executes a command with no parameters and converts the returned data to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(string name)
        {
            bool result = false;
            object reply = ExecuteCommand(name, null, out result); ;
            try
            {
                return (T)reply;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Executes a command with no parameters and converts the returned data to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(int screen, string name)
        {
            bool result = false;
            object reply = ExecuteCommand(screen, name, null, out result); ;
            try
            {
                return (T)reply;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Executes a command with parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public object ExecuteCommand(string name, params object[] param)
        {
            bool result = false;
            return ExecuteCommand(name, param, out result);
        }
        /// <summary>
        /// Executes a command with parameters
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public object ExecuteCommand(int screen, string name, params object[] param)
        {
            bool result = false;

            if (name.Contains(";"))
            {
                string provider = name.Substring(0, name.IndexOf(';'));
                name = name.Remove(0, provider.Length+1);
                return ExecuteCommand(String.Format("{0};{1}", provider, String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name)), param, out result);
            }
            else
            {
                return ExecuteCommand(String.Format("{0}.{1}", DataNameBase.GetScreenString(screen), name), param, out result);
            }
        }

        /// <summary>
        /// Executes a command with no parameters and converts the returned data to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(int screen, string name, params object[] param)
        {
            bool result = false;
            object reply = ExecuteCommand(screen, name, param, out result); ;
            try
            {
                return (T)reply;
            }
            catch
            {
                return default(T);
            }
        }
        /// <summary>
        /// Executes a command with parameters and converts the returned data to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(string name, params object[] param)
        {
            bool result = false;
            object reply = ExecuteCommand(name, param, out result);;
            try
            {
                return (T)reply;
            }
            catch
            {
                return default(T);
            }
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

        #region CommandGroups

        private List<CommandGroup> _CommandGroups = new List<CommandGroup>();

        /// <summary>
        /// Gets a CommandGroup
        /// </summary>
        /// <param name="CommandGroupName"></param>
        /// <returns></returns>
        private CommandGroup GetCommandGroup(string CommandGroupName)
        {
            return _CommandGroups.Find(x => x.FullNameWithoutScreen == CommandGroupName);
        }

        /// <summary>
        /// True = CommandGroup is active
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool IsCommandGroupActive(CommandGroup commandGroup)
        {
            return _CommandGroups.Contains(commandGroup);
        }

        /// <summary>
        /// True = command is present
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool IsCommandPresent(Command command)
        {
            return _Commands.Find(x => x.FullNameWithoutScreen == command.FullNameWithoutScreen) != null;
        }

        /// <summary>
        /// True = command is present
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool IsExactCommandPresent(Command command)
        {
            return _Commands.Find(x => x.FullNameWithProvider == command.FullNameWithProvider) != null;
        }

        /// <summary>
        /// Activates a screen specific command group on a specific screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool ActivateCommandGroup(int screen, CommandGroupScreenSpecific commandGroup)
        {
            return ActivateCommandGroup(commandGroup.Instances[screen]);
        }

        /// <summary>
        /// Activates a screen specific command group on all available screens
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool ActivateCommandGroup(CommandGroupScreenSpecific commandGroup)
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                ActivateCommandGroup(commandGroup.Instances[i]);
            }
            return true;
        }

        /// <summary>
        /// Activates a command group and replaces nameLevels at the same time.
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <returns></returns>
        public bool ActivateCommandGroup(CommandGroup commandGroup, int screen = -1, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
        {
            commandGroup.SetScreen(screen);
            commandGroup.SetNameLevels(nameLevel1, nameLevel2, nameLevel3);
            return ActivateCommandGroup(commandGroup);
        }
        /// <summary>
        /// Activates a commandGroup
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <returns></returns>
        public bool ActivateCommandGroup(CommandGroup commandGroup)
        {
            lock (commandGroup)
            {
                CommandGroup currentGroup = GetCommandGroup(commandGroup.FullName);
                int currentIndex = _CommandGroups.IndexOf(currentGroup);

                if (currentGroup == commandGroup)
                    return false;

                if (currentGroup != null)
                {   // Replace existing group
                    CommandGroup.ReplaceGroup(currentGroup, commandGroup);
                    _CommandGroups[currentIndex] = commandGroup;
                }
                else
                {   // New group
                    _CommandGroups.Add(commandGroup);

                    // Activate new group
                    CommandGroup.ActivateCommandsInGroup(this, commandGroup);
                }
                return true;
            }
        }

        /// <summary>
        /// Replaces one datasource with another one
        /// </summary>
        /// <param name="oldCommand"></param>
        /// <param name="newCommand"></param>
        /// <returns></returns>
        public bool ReplaceCommand(Command oldCommand, Command newCommand)
        {
            // Remap subscriptions
            Command.Remap(oldCommand, newCommand);

            // Remap list data
            int index = _Commands.IndexOf(oldCommand);
            if (index >= 0)
                _Commands[index] = newCommand;

            return true;
        }

        #endregion
    }
}
