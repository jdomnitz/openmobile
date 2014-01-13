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
using GMap.NET;
using DotSpatial.Positioning;

namespace OpenMobile.NavEngine
{
    /// <summary>
    /// A location with a known address
    /// </summary>
    public class KnownPosition
    {
        /// <summary>
        /// The human readable address for this location
        /// </summary>
        public string Address
        {
            get
            {
                return this._Address;
            }
            set
            {
                if (this._Address != value)
                {
                    this._Address = value;
                }
            }
        }
        private string _Address;

        /// <summary>
        /// The latitude / longitude for this location
        /// </summary>
        public Position Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                if (this._Position != value)
                {
                    this._Position = value;
                }
            }
        }
        private Position _Position;

        #region Constructors

        public KnownPosition()
        {
        }
        public KnownPosition(Latitude latitude, Longitude longitude, string address)
        {
            _Position = new Position(latitude, longitude);
            _Address = address;
        }
        public KnownPosition(Position position, string address)
        {
            _Position = position;
            _Address = address;
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0} {1}", _Address, _Position);
        }
    }
}
