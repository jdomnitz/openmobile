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
using OpenMobile;
using OpenMobile.Plugin;

namespace OpenMobile
{
    /// <summary>
    /// The delagate that holds the actual code for the command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="param"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public delegate object CommandExecDelegate(Command command, object[] param, out bool result);

    /// <summary>
    /// A openmobile command 
    /// </summary>
    public class Command : DataNameBase, ICloneable
    {
        #region Static methods

        /// <summary>
        /// The separator char to use when sending multiple commands
        /// </summary>
        public const char CommandSeparator = '|';

        /// <summary>
        /// Extracts the screen number from a string like this "Screen0" returns 0 if it fails
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetScreenNumber(string s)
        {
            int screen = 0;
            int.TryParse(s.Substring(6), out screen);
            return screen;
        }

        /// <summary>
        /// Writes the screen number to a string 
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public static string GetScreenName(int screen)
        {
            return String.Format("Screen{0}", screen);
        }

        /// <summary>
        /// Remaps the command executors from one command to another
        /// </summary>
        /// <param name="fromCommand"></param>
        /// <param name="toCommand"></param>
        static public void Remap(Command fromCommand, Command toCommand)
        {
            // Copy executor to the new command
            toCommand._ExecDelegate = fromCommand._ExecDelegate;

            // Clear executor from old command
            fromCommand._ExecDelegate = null;
        }

        /// <summary>
        /// Clears all execute delegates from a command
        /// </summary>
        /// <param name="dataSource"></param>
        static public void ClearExecutor(Command command)
        {
            command._ExecDelegate = null;
        }

        #endregion

        /// <summary>
        /// Does this command require parameters to be passed to it
        /// </summary>
        public bool RequiresParameters
        {
            get
            {
                return _RequiredParamCount > 0;
            }
        }

        /// <summary>
        /// Does this command return a value
        /// </summary>
        public bool ReturnsValue
        {
            get
            {
                return this._ReturnsValue;
            }
            set
            {
                if (this._ReturnsValue != value)
                {
                    this._ReturnsValue = value;
                }
            }
        }
        private bool  _ReturnsValue;        

        /// <summary>
        /// Is this a valid command
        /// </summary>
        public bool Valid
        {
            get
            {
                if (_LinkedSource != null)
                    return _LinkedSource._ExecDelegate != null;

                return _ExecDelegate != null;
            }
        }

        /// <summary>
        /// Is this command enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                if (this._Enabled != value)
                {
                    this._Enabled = value;
                }
            }
        }
        private bool _Enabled = true;

        /// <summary>
        /// The group this command belongs to
        /// </summary>
        public CommandGroup Group
        {
            get
            {
                return this._Group;
            }
            set
            {
                if (this._Group != value)
                {
                    this._Group = value;
                }
            }
        }
        private CommandGroup _Group;

        /// <summary>
        /// The linked command target
        /// </summary>
        public Command LinkedTarget
        {
            get
            {
                return this._LinkedTarget;
            }
            set
            {
                if (this._LinkedTarget != value)
                {
                    this._LinkedTarget = value;
                }
            }
        }
        private Command _LinkedTarget;

        /// <summary>
        /// The linked Command source
        /// </summary>
        public Command LinkedSource
        {
            get
            {
                return this._LinkedSource;
            }
            set
            {
                if (this._LinkedSource != value)
                {
                    this._LinkedSource = value;
                }
            }
        }
        private Command _LinkedSource;      

        /// <summary>
        /// The amount of required parameters
        /// </summary>
        public int RequiredParamCount
        {
            get
            {
                return this._RequiredParamCount;
            }
            set
            {
                if (this._RequiredParamCount != value)
                {
                    this._RequiredParamCount = value;
                }
            }
        }
        private int _RequiredParamCount;

        /// <summary>
        /// The execute delegate
        /// </summary>
        private CommandExecDelegate _ExecDelegate;

        /// <summary>
        /// Executes the command and returns the state of the command in the result parameter
        /// </summary>
        /// <param name="param"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public object Execute(object[] param, out bool result)
        {
            // Is this command enabled?
            if (!_Enabled)
            {   // Command disabled unable to process command
                result = false;
                return null;
            }

            // Use linked command?
            if (_LinkedSource != null)
                return _LinkedSource.Execute(param, out result);

            // Check if parameters is required
            if (RequiresParameters && (param == null || param.Length < _RequiredParamCount))
            {   // Missing parameters unable to process command
                result = false;
                return null;
            }

            if (_ExecDelegate != null)
                return _ExecDelegate(this, param, out result);
            result = false;
            return null;
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object Execute(object[] param)
        {
            bool result = false;
            return Execute(param, out result);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <returns></returns>
        public object Execute()
        {
            bool result = false;
            return Execute(null, out result);
        }


        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="execDelegate"></param>
        /// <param name="requiredParamCount"></param>
        /// <param name="returnsValue"></param>
        /// <param name="description"></param>
        public Command(IBasePlugin provider, string nameLevel1, string nameLevel2, string nameLevel3, CommandExecDelegate execDelegate, int requiredParamCount, bool returnsValue, string description)
        {
            this._Provider = provider.pluginName;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
            this._ExecDelegate = execDelegate;
            this._RequiredParamCount = requiredParamCount;
            this._ReturnsValue = returnsValue;
            this._Description = description;
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="screenSpecific"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="execDelegate"></param>
        /// <param name="requiredParamCount"></param>
        /// <param name="returnsValue"></param>
        /// <param name="description"></param>
        public Command(bool screenSpecific, IBasePlugin provider, string nameLevel1, string nameLevel2, string nameLevel3, CommandExecDelegate execDelegate, int requiredParamCount, bool returnsValue, string description)
            : this(provider, nameLevel1, nameLevel2, nameLevel3, execDelegate, requiredParamCount, returnsValue, description)
        {
            ScreenSpecific = screenSpecific;
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="execDelegate"></param>
        /// <param name="requiredParamCount"></param>
        /// <param name="returnsValue"></param>
        /// <param name="description"></param>
        public Command(string provider, string nameLevel1, string nameLevel2, string nameLevel3, CommandExecDelegate execDelegate, int requiredParamCount, bool returnsValue, string description)
        {
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
            this._ExecDelegate = execDelegate;
            this._RequiredParamCount = requiredParamCount;
            this._ReturnsValue = returnsValue;
            this._Description = description;
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="screenSpecific"></param>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="execDelegate"></param>
        /// <param name="requiredParamCount"></param>
        /// <param name="returnsValue"></param>
        /// <param name="description"></param>
        public Command(bool screenSpecific, string provider, string nameLevel1, string nameLevel2, string nameLevel3, CommandExecDelegate execDelegate, int requiredParamCount, bool returnsValue, string description)
            : this(provider, nameLevel1, nameLevel2, nameLevel3, execDelegate, requiredParamCount, returnsValue, description)
        {
            ScreenSpecific = screenSpecific;
        }

        public override string ToString()
        {
            return FullNameWithProvider;
        }

        /// <summary>
        /// Clones this command
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
