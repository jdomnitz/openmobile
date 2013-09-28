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

namespace OpenMobile
{
    /// <summary>
    /// Infobanner data
    /// </summary>
    public class InfoBar
    {
        #region Properties

        /// <summary>
        /// The text to show
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                }
            }
        }
        private string _Text;

        /// <summary>
        /// The icon to show
        /// </summary>
        public imageItem Icon
        {
            get
            {
                return this._Icon;
            }
            set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                }
            }
        }
        private imageItem _Icon;        

        /// <summary>
        /// Time in milliseconds the text and icon is shown (from start animation to close)
        /// </summary>
        public int Timeout
        {
            get
            {
                return this._Timeout;
            }
            set
            {
                if (this._Timeout != value)
                {
                    this._Timeout = value;
                }
            }
        }
        private int _Timeout = 0;

        #endregion

        #region constructors

        /// <summary>
        /// Creates new InfoBar data
        /// </summary>
        /// <param name="text"></param>
        public InfoBar(string text)
        {
            _Text = text;
        }

        /// <summary>
        /// Creates new InfoBar data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="icon"></param>
        public InfoBar(string text, imageItem icon)
        {
            _Text = text;
            _Icon = icon;
        }

        /// <summary>
        /// Creates new InfoBar data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="timeout"></param>
        public InfoBar(string text, int timeout)
            : this(text)
        {
            _Timeout = timeout;
        }

        /// <summary>
        /// Creates new InfoBar data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="icon"></param>
        /// <param name="timeout"></param>
        public InfoBar(string text, imageItem icon, int timeout)
            : this(text, icon)
        {
            _Timeout = timeout;
        }

        #endregion
    }
}
