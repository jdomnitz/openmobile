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
    public delegate object CommandExecDelegate(Command command, object[] param, out bool result);

    /// <summary>
    /// A openmobile command 
    /// </summary>
    public class Command : DataNameBase
    {
        /// <summary>
        /// Does this command require parameters to be passed to it
        /// </summary>
        public bool RequiresParameters
        {
            get
            {
                return this._RequiresParameters;
            }
            set
            {
                if (this._RequiresParameters != value)
                {
                    this._RequiresParameters = value;
                }
            }
        }
        private bool _RequiresParameters;

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
                return _ExecDelegate != null;
            }
        }

        /// <summary>
        /// The execute delegate
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public CommandExecDelegate ExecDelegate
        {
            get
            {
                return this._ExecDelegate;
            }
            set
            {
                if (this._ExecDelegate != value)
                {
                    this._ExecDelegate = value;
                }
            }
        }
        private CommandExecDelegate _ExecDelegate;

        /// <summary>
        /// Executes the command and returns the state of the command in the result parameter
        /// </summary>
        /// <param name="param"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public object Execute(object[] param, out bool result)
        {
            // Check if parameters is required
            if (_RequiresParameters && param == null)
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
        /// Creates a new command
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="nameLevel1"></param>
        /// <param name="nameLevel2"></param>
        /// <param name="nameLevel3"></param>
        /// <param name="execDelegate"></param>
        public Command(string provider, string nameLevel1, string nameLevel2, string nameLevel3, CommandExecDelegate execDelegate, bool requiresParameters, bool returnsValue, string description)
        {
            this._Provider = provider;
            this._NameLevel1 = nameLevel1;
            this._NameLevel2 = nameLevel2;
            this._NameLevel3 = nameLevel3;
            this._ExecDelegate = execDelegate;
            this._RequiresParameters = requiresParameters;
            this._ReturnsValue = returnsValue;
            this._Description = description;
        }
        
    }
}
