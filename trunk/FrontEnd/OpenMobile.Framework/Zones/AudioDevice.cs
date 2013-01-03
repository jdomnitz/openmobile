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
using System.Xml.Serialization;

namespace OpenMobile
{
    /// <summary>
    /// A audiodevice
    /// </summary>
    public class AudioDevice
    {
        #region Static methods and properties

        /// <summary>
        /// The name for the default audio device
        /// </summary>
        static public string DefaultDeviceName = "Default Device";

        /// <summary>
        /// Get's the default audio device
        /// </summary>
        static public AudioDevice DefaultDevice
        {
            get
            {
                return _DefaultDevice;
            }
        }
        static private AudioDevice _DefaultDevice = new AudioDevice(DefaultDeviceName, 0, null, null, null);

        #endregion

        /// <summary>
        /// True = this is a valid audio device
        /// </summary>
        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return (_Action_Get != null && _Action_Set != null);
            }
        }

        /// <summary>
        /// True = this is the default audio device
        /// </summary>
        [XmlIgnore]
        public bool IsDefault
        {
            get
            {
                return (this._Name.Equals(DefaultDeviceName));
            }
        }

        /// <summary>
        /// The name of the audiodevice
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
        /// The audio device instance number
        /// </summary>
        [XmlIgnore]
        public int Instance
        {
            get
            {
                return this._Instance;
            }
            set
            {
                if (this._Instance != value)
                {
                    this._Instance = value;
                }
            }
        }
        private int _Instance = -999;

        /// <summary>
        /// A general usage tag
        /// </summary>
        [XmlIgnore]
        public object Tag
        {
            get
            {
                return this._Tag;
            }
            set
            {
                if (this._Tag != value)
                {
                    this._Tag = value;
                }
            }
        }
        private object _Tag;        

        #region Actions

        /// <summary>
        /// Audiodevice actions
        /// </summary>
        public enum Actions
        {
            /// <summary>
            /// Mute as bool value
            /// </summary>
            Mute,

            /// <summary>
            /// Volume as int (percent)
            /// </summary>
            Volume
        }

        /// <summary>
        /// A delegate for a simple action
        /// </summary>
        /// <param name="audioDevice"></param>
        public delegate void ActionDelegate(AudioDevice audioDevice);
        /// <summary>
        /// A delegate for a get operation
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        public delegate object GetDelegate(AudioDevice audioDevice, Actions action);
        /// <summary>
        /// A delegate for a set operation
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="Action"></param>
        /// <param name="value"></param>
        public delegate void SetDelegate(AudioDevice audioDevice, Actions action, object value);

        private GetDelegate _Action_Get;
        private SetDelegate _Action_Set;

        /// <summary>
        /// Mutes the audio device (true = muted)
        /// </summary>
        /// <returns></returns>
        public bool Mute
        {
            get
            {
                if (_Action_Get != null)
                    try
                    {
                        return (bool)_Action_Get(this, Actions.Mute);
                    }
                    catch { }
                return false;
            }
            set
            {
                if (_Action_Set != null)
                    try
                    {
                        _Action_Set(this, Actions.Mute, value);
                    }
                    catch { }                
            }
        }

        /// <summary>
        /// Volume for the audio device in the range of 0 - 100 percent
        /// </summary>
        /// <returns></returns>
        public int Volume
        {
            get
            {
                if (_Action_Get != null)
                    try
                    {
                        return (int)_Action_Get(this, Actions.Volume);
                    }
                    catch { }
                return 0;
            }
            set
            {
                if (value > 100)
                    value = 100;
                if (value < 0)
                    value = 0;

                if (_Action_Set != null)
                    try
                    {
                        _Action_Set(this, Actions.Volume, value);
                    }
                    catch { }
            }
        }

        #endregion

        public delegate void UpdatedDelegate(AudioDevice audioDevice, Actions action, object value);
        public event UpdatedDelegate OnUpdated;

        public void Raise_DataUpdated(Actions action, object value)
        {   
            if (OnUpdated != null)
                OnUpdated(this, action, value);
        }

        #region Constructors

        /// <summary>
        /// Creates a new audio device
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        /// <param name="action_Get"></param>
        /// <param name="action_Set"></param>
        /// <param name="tag"></param>
        public AudioDevice(string name, int instance, GetDelegate action_Get, SetDelegate action_Set, object tag)
        {
            _Name = name;
            _Instance = instance;
            _Action_Get = action_Get;
            _Action_Set = action_Set;
            _Tag = tag;
        }

        /// <summary>
        /// Creates a new audio device
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action_Get"></param>
        /// <param name="action_Set"></param>
        /// <param name="tag"></param>
        /// <param name="UseAsDefault"></param>
        public AudioDevice(int instance, GetDelegate action_Get, SetDelegate action_Set, object tag, bool UseAsDefault)
            : this(DefaultDeviceName, instance, action_Get, action_Set, tag)
        {
            if (UseAsDefault)
                _DefaultDevice = this;
        }


        ///// <summary>
        ///// Creates a new audio device
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="instanceID"></param>
        //public AudioDevice(string name, int instanceID)
        //{
        //    _Name = name;
        //    _Instance = instanceID;
        //}

        /// <summary>
        /// Do not use this constructor, included only for serialization
        /// </summary>
        private AudioDevice()
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string describing this object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:[{1}]", _Name, _Instance);
        }

        #endregion
    }
}
