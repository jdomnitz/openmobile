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
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// An audio route object
    /// </summary>
    public class AudioRoute
    {
        /// <summary>
        /// The source device 
        /// </summary>
        public AudioDevice SourceDevice
        {
            get
            {
                return this._SourceDevice;
            }
            set
            {
                if (this._SourceDevice != value)
                {
                    this._SourceDevice = value;
                }
            }
        }
        private AudioDevice _SourceDevice;

        /// <summary>
        /// The target device
        /// </summary>
        public AudioDevice TargetDevice
        {
            get
            {
                return this._TargetDevice;
            }
            set
            {
                if (this._TargetDevice != value)
                {
                    this._TargetDevice = value;
                }
            }
        }
        private AudioDevice _TargetDevice;

        /// <summary>
        /// Creates a new audio route
        /// </summary>
        public AudioRoute()
        {
        }

        /// <summary>
        /// Creates a new audio route
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        public AudioRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            _SourceDevice = sourceDevice;
            _TargetDevice = targetDevice;
        }
    }
}
