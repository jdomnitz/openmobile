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

namespace OpenMobile.Controls
{
    public class OMEventArgs : EventArgs
    {
        /// <summary>
        /// The screen number
        /// </summary>
        public int Screen
        {
            get
            {
                return this._Screen;
            }
            internal set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                }
            }
        }
        private int _Screen;

        /// <summary>
        /// The control that raised this event
        /// </summary>
        public OMControl Sender
        {
            get
            {
                return this._Sender;
            }
            set
            {
                if (this._Sender != value)
                {
                    this._Sender = value;
                }
            }
        }
        private OMControl _Sender;

        /// <summary>
        /// Creates new OMEventArgs
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="sender"></param>
        public OMEventArgs(int screen, OMControl sender)
        {
            _Screen = screen;
            _Sender = sender;
        }
        
    }
}
