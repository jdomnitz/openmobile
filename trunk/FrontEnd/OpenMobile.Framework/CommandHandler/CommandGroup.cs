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

namespace OpenMobile
{
    public class CommandGroupScreenSpecific : DataNameBase
    {
        /// <summary>
        /// The different command group instances
        /// </summary>
        public CommandGroup[] Instances
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
        private CommandGroup[] _Instances;

        /// <summary>
        /// The name of this command group
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
        public CommandGroupScreenSpecific(string name, string provider, string nameLevel1, string nameLevel2, string nameLevel3)
        {
            this._Name = name;
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;

            _Instances = new CommandGroup[OM.Host.ScreenCount];
            for (int i = 0; i < OM.Host.ScreenCount; i++)
                _Instances[i] = new CommandGroup(name, i, provider, nameLevel1, nameLevel2, nameLevel3);
        }

        /// <summary>
        /// Adds a screen specfic instance to this command group
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(Command[] command)
        {
            for (int i = 0; i < command.Length; i++)
                _Instances[i].AddCommand(command[i]);
        }
    }

    /// <summary>
    /// A command group
    /// </summary>
    public class CommandGroup : DataNameBase
    {
        #region Static methods

        /// <summary>
        /// Replaces one datasource group with another
        /// </summary>
        /// <param name="currentGroup"></param>
        /// <param name="newGroup"></param>
        static public void ReplaceGroup(CommandGroup currentGroup, CommandGroup newGroup)
        {
            // Inform old group that it's about to be deactivated
            currentGroup.Raise_OnDeactivate();

            List<Command> unMappedData = new List<Command>(currentGroup._Commands);
            for (int i = 0; i < newGroup._Commands.Count; i++)
            {
                // Try to find match in old group
                Command matchedData = unMappedData.Find(x => x.FullNameWithoutScreen == newGroup._Commands[i].FullNameWithoutScreen);
                if (matchedData != null)
                {   // We found a match, remap this command
                    OM.Host.CommandHandler.ReplaceCommand(matchedData, newGroup._Commands[i]);

                    // Remove this command from unmapped list
                    unMappedData.Remove(matchedData);
                }
            }

            // Set old group to non active
            currentGroup._Active = false;

            // Set new group to active
            newGroup._Active = true;

            // Disable all commands still in the unmapped list
            for (int i = 0; i < unMappedData.Count; i++)
                unMappedData[i].Enabled = false;

            // Enable all commands in the new group
            for (int i = 0; i < newGroup._Commands.Count; i++)
            {
                newGroup._Commands[i].Enabled = true;

                if (!OM.Host.CommandHandler.IsCommandPresent(newGroup._Commands[i]))
                    OM.Host.CommandHandler.AddCommand(newGroup._Commands[i]);
            }

            // Inform new group that it's been activated
            newGroup.Raise_OnActivate();
        }

        /// <summary>
        /// Activates commands in a group
        /// </summary>
        /// <param name="commandHandler"></param>
        /// <param name="group"></param>
        static public void ActivateCommandsInGroup(CommandHandler commandHandler, CommandGroup group)
        {
            foreach (var command in group._Commands)
                commandHandler.AddCommand(command);

            group.Active = true;

            // Inform group that it's been activated
            group.Raise_OnActivate();
        }

        /// <summary>
        /// Replaces namelevels in a Command with namelevels from a CommandGroup
        /// </summary>
        /// <param name="commandGroup"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        static public Command ReplaceNameLevels(CommandGroup commandGroup, Command command)
        {
            // Transfer name levels from dataSourceGroup to datasource
            if (commandGroup.Provider != null)
                command.Provider = commandGroup.Provider;
            if (commandGroup.NameLevel1 != null)
                command.NameLevel1 = commandGroup.NameLevel1;
            if (commandGroup.NameLevel2 != null)
                command.NameLevel2 = commandGroup.NameLevel2;
            if (commandGroup.NameLevel3 != null)
                command.NameLevel3 = commandGroup.NameLevel3;
            return command;
        }

        #endregion

        /// <summary>
        /// Deactivation callback delegate
        /// </summary>
        public delegate void CommandGroupCallback(CommandGroup commandGroup);

        /// <summary>
        /// Raised when the command group is deactivated
        /// </summary>
        public event CommandGroupCallback OnDeactivation;
        /// <summary>
        /// Raised when the command group is activated
        /// </summary>
        public event CommandGroupCallback OnActivation;

        /// <summary>
        /// The commands registered for this command group
        /// </summary>
        public List<Command> Commands
        {
            get
            {
                return this._Commands;
            }
            set
            {
                if (this._Commands != value)
                {
                    this._Commands = value;
                }
            }
        }
        private List<Command> _Commands = new List<Command>();

        /// <summary>
        /// The name of this command group
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
        /// Creates a new command group
        /// </summary>
        /// <param name="name"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        public CommandGroup(string name, string provider, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
        {
            this._Name = name;
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
        }

        /// <summary>
        /// Creates a new command group
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screen"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        public CommandGroup(string name, int screen, string provider, string nameLevel1 = null, string nameLevel2 = null, string nameLevel3 = null)
            : this(name, provider, nameLevel1, nameLevel2, nameLevel3)
        {
            Screen = screen;
        }

        /// <summary>
        /// Activates this command group
        /// </summary>
        public void Activate()
        {
            Raise_OnActivate();
        }

        /// <summary>
        /// Deactivates this command group
        /// </summary>
        public void Deactivate()
        {
            Raise_OnDeactivate();
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

        /// <summary>
        /// Returns true if the specified Command is already present in this group
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool ContainsCommand(Command command)
        {
            return _Commands.Contains(command);
        }

        public void SetScreen(int screen)
        {
            base.Screen = screen;
            if (screen >= 0)
            {
                for (int i = 0; i < _Commands.Count; i++)
                    _Commands[i].Screen = screen;
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

            for (int i = 0; i < _Commands.Count; i++)
                _Commands[i] = CommandGroup.ReplaceNameLevels(this, _Commands[i]);
        }

        /// <summary>
        /// Adds a command to this group from an array of screen specific commands
        /// </summary>
        /// <param name="dataSource"></param>
        public void AddCommand(Command[] commands)
        {
            if (!this.ScreenSpecific && !this.IsScreenSpecified)
                AddCommand(commands[0]);
            else
                AddCommand(commands[this.Screen]);
        }

        /// <summary>
        /// Registers a command with this group
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(Command command)
        {
            Command localCommand = null;

            if (base.IsScreenSpecified && command.ScreenSpecific)
                localCommand = OM.Host.CommandHandler.GetCommand(String.Format("{0}.{1}", DataNameBase.GetScreenString(base.Screen), command.FullNameWithoutScreen));
            else
                localCommand = command;

            // Cancel if already present
            if (ContainsCommand(localCommand))
                return;

            Command clonedSource = (Command)((ICloneable)localCommand).Clone();

            // Add a screen reference to the command in case the group is screen specific
            if (base.IsScreenSpecified && !command.ScreenSpecific)
                command.Screen = base.Screen;

            // Remove any subscribers that was present when cloning it 
            Command.ClearExecutor(clonedSource);

            // Add reference to this group in case it's not already done
            localCommand.Group = null;
            localCommand.LinkedTarget = clonedSource;

            // Replace name levels with information from DataSourceGroup
            clonedSource = CommandGroup.ReplaceNameLevels(this, clonedSource);

            // Add reference to this group in case it's not already done
            clonedSource.Group = this;
            clonedSource.LinkedTarget = null;
            clonedSource.LinkedSource = localCommand;
            clonedSource.ScreenSpecific = false;

            // Add the new dataSource
            if (!ContainsCommand(clonedSource))
                _Commands.Add(clonedSource);
        }

        /// <summary>
        /// Returns a string representation of this group
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._Name;
        }
    }
}
