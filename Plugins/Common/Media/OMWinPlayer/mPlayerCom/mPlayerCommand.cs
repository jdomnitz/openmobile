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
using System.Threading;
using System.Text;

namespace OpenMobile.mPlayer
{
    /// <summary>
    /// A mPlayer command
    /// </summary>
    /// <typeparam name="ReplyType"></typeparam>
    public class mPlayerCommand
    {
        /// <summary>
        /// The command
        /// </summary>
        public Commands Command
        {
            get
            {
                return this._Command;
            }
        }
        private Commands _Command;        

        /// <summary>
        /// The text to send to mPlayer (Example: quit)
        /// </summary>
        public string CommandText
        {
            get
            {
                return this._CommandText;
            }
        }
        private string _CommandText;

        /// <summary>
        /// The ID text of the reply (Example: ID_EXIT)
        /// </summary>
        public string ReplyID
        {
            get
            {
                return this._ReplyID;
            }
        }
        private string _ReplyID;

        /// <summary>
        /// The data part of the reply
        /// </summary>
        public string ReplyData
        {
            get
            {
                return this._ReplyData;
            }
            set
            {
                if (this._ReplyData != value)
                {
                    this._ReplyData = value;
                }
            }
        }
        private string  _ReplyData;

        /// <summary>
        /// Gets and sets error data (Get will clear internal data as well)
        /// </summary>
        public string ErrorData
        {
            get
            {
                string s = this._ErrorData;
                this._ErrorData = String.Empty;
                return s;
            }
            set
            {
                if (this._ErrorData != value)
                {
                    this._ErrorData = value;

                    if (_DetectErrors)
                    {
                        // Trigg reply recevied wait handle
                        TriggReplyReceived.Set();
                    }
                }
            }
        }
        private string _ErrorData;

        /// <summary>
        /// Does this command require a reply
        /// </summary>
        public bool RequiresReply 
        {
            get
            {
                return !string.IsNullOrEmpty(_ReplyID);
            }
        }

        /// <summary>
        /// Does this command take parameters
        /// </summary>
        public bool RequiresParameters
        {
            get
            {
                return _CommandText.Contains("{");
            }
        }

        /// <summary>
        /// True = errors thrown while command is active will cancel command
        /// </summary>
        public bool DetectErrors
        {
            get
            {
                return this._DetectErrors;
            }
            set
            {
                if (this._DetectErrors != value)
                {
                    this._DetectErrors = value;
                }
            }
        }
        private bool _DetectErrors;        

        /// <summary>
        /// The index of the parameter to fix path for
        /// </summary>
        private int _FixPathParamNo = -1;

        /// <summary>
        /// Get's the full command text to send (including parameters)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetCommandText(params string[] param)
        {
            if (param == null || !RequiresParameters)
                return _CommandText;

            // Fix path errors
            if (_FixPathParamNo >= 0 && param.Length >= _FixPathParamNo)
                param[_FixPathParamNo] = FixFilePath(param[_FixPathParamNo]);

            return String.Format(_CommandText, param);
        }

        /// <summary>
        /// Indicates that a reply to this command was received
        /// </summary>
        public EventWaitHandle TriggReplyReceived = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// Waits for and returns the reply from the command
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string GetReplyData(int timeout)
        {
            if (string.IsNullOrEmpty(_ReplyID))
                return String.Empty;

            TriggReplyReceived.WaitOne(timeout);
            string result = _ReplyData;
            _ReplyData = String.Empty;
            return result;
        }

        /// <summary>
        /// Create a new command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandText"></param>
        public mPlayerCommand(Commands command, bool detectErrors, string commandText)
        {
            this._Command = command;
            this._CommandText = commandText;
            this._DetectErrors = detectErrors;
        }

        /// <summary>
        /// Create a new command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandText"></param>
        /// <param name="replyID"></param>
        public mPlayerCommand(Commands command, bool detectErrors, string commandText, string replyID)
        {
            this._Command = command;
            this._CommandText = commandText;
            this._ReplyID = replyID;
            this._DetectErrors = detectErrors;
        }

        /// <summary>
        /// Create a new command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandText"></param>
        /// <param name="replyID"></param>
        /// <param name="fixPathParamNo"></param>
        public mPlayerCommand(Commands command, bool detectErrors, string commandText, string replyID, int fixPathParamNo)
        {
            this._Command = command;
            this._CommandText = commandText;
            this._ReplyID = replyID;
            this._FixPathParamNo = fixPathParamNo;
            this._DetectErrors = detectErrors;
        }

        /// <summary>
        /// Gets a string representation of this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _Command.ToString();
        }

        private string FixFilePath(string path)
        {
            return path.Replace("" + System.IO.Path.DirectorySeparatorChar, "" + System.IO.Path.DirectorySeparatorChar + System.IO.Path.DirectorySeparatorChar);
        }
    }
}
