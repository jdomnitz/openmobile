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

namespace OpenMobile.Media
{
    /// <summary>
    /// A media source (Example: For a radio it could be AM, FM or DAB)
    /// </summary>
    public class MediaSource
    {
        /// <summary>
        /// Icon representing this media source
        /// </summary>
        public imageItem Icon
        {
            get
            {
                return this._Icon;
            }
            private set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                }
            }
        }
        private imageItem _Icon;

        /// <summary>
        /// The text representing this media source (Example: For a radio it could be AM, FM or DAB)
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            private set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                }
            }
        }
        private string _Text;

        /// <summary>
        /// Creates a new MediaSource
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        public MediaSource(imageItem icon, string text)
        {
            this._Icon = icon;
            this._Text = text;
        }
        
    }
}
