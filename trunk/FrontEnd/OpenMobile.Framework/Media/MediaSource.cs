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
using OpenMobile.Graphics;

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
        public OImage Icon
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
        private OImage _Icon;

        /// <summary>
        /// The text representing this media source (Example: For a radio it could be AM, FM or DAB)
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            private set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                }
            }
        }
        private string _Name;

        /// <summary>
        /// Current PlayList
        /// </summary>
        public PlayList2 Playlist
        {
            get
            {
                return this._Playlist;
            }
            set
            {
                if (this._Playlist != value)
                {
                    this._Playlist = value;
                }
            }
        }
        private PlayList2 _Playlist = null;

        /// <summary>
        /// Creates a new MediaSource
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        public MediaSource(OImage icon, string name)
        {
            this._Icon = icon;
            this._Name = name;
        }

        /// <summary>
        /// Initializes the playlist
        /// </summary>
        public void InitPlayList()
        {
            // Initialize current PlayList
            if (Playlist == null)
                Playlist = new PlayList2(String.Format("{0}_PlayList_Current", _Name));
        }

        /// <summary>
        /// Clones this media source
        /// </summary>
        /// <returns></returns>
        public MediaSource Clone()
        {
            return new MediaSource(this._Icon, this.Name);
        }

        /// <summary>
        /// Compares two MediaSource by looking at their names
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(MediaSource a, MediaSource b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            if (a.Name.Equals(b.Name))
                return true;

            return false;
        }

        /// <summary>
        /// Negates two MediaSource by looking at their names
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(MediaSource a, MediaSource b)
        {
            return !(a == b);
        }
        
    }
}
