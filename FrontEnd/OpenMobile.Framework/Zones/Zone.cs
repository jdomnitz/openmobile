﻿/*********************************************************************************
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
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Media;

namespace OpenMobile
{
    /// <summary>
    /// A zone is a object consisting of one or more Screens and AudioUnits
    /// /// </summary>
    [Serializable]
    public class Zone
    {
        /// <summary>
        /// Sets the autogenerated state of the zone
        /// <para>Autogenerated makes this zone read only (Audio device can still be changed)</para>
        /// </summary>
        public bool AutoGenerated { get; set; }

        /// <summary>
        /// Sets the master state for the zone
        /// <para>Master means that this is the default zone for the assigned screen</para>
        /// </summary>
        public bool Master { get; set; }

        /// <summary>
        /// Set the subzone state for this zone
        /// <para>False = Allow this zone to be a subzone, True = Don't allow this zone to be a subzone</para>
        /// </summary>
        public bool BlockSubZoneUsage { get; set; }

        /// <summary>
        /// Specifies wich screens this zone can be activated on. 
        /// <para>Empty string = any screen, specify each screennumber with a | separator like this 0|1|2 (alloed usage is screen 0, 1 and 2)</para>
        /// </summary>
        public string AllowedScreens { get; set; }

        /// <summary>
        /// Check if this zone can be activated on the given screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool CanZoneBeActivated(int screen)
        {
            // If empty string then everything is allowed
            if (String.IsNullOrEmpty(AllowedScreens))
                return true;

            // Check if the screen number is listed in the string, if so it's allowed
            return AllowedScreens.Contains(screen.ToString());
        }

        /// <summary>
        /// Is this a valid and usable zone?
        /// </summary>
        public bool Valid
        {
            get
            {
                if (this._AudioDevice == null)
                    this._Valid = false;
                return this._Valid;
            }
            set
            {
                if (this._Valid != value)
                {
                    this._Valid = value;
                }

                if (this._AudioDevice == null)
                    this._Valid = false;
            }
        }
        private bool _Valid;        

        private List<int> _SubZones = new List<int>();
        /// <summary>
        /// List of zone's (ID's) that's included in this zone
        /// </summary>
        public List<int> SubZones
        {
            get { return _SubZones; }
            set { _SubZones = value; }
        }

        /// <summary>
        /// Does this zone have subzones
        /// </summary>
        public bool HasSubZones
        {
            get
            {
                return _SubZones.Count > 0;
            }
        }

        /// <summary>
        /// Name of the zone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the zone
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Screen number for this zone
        /// </summary>
        public int Screen
        {
            get
            {
                return this._Screen;
            }
            set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                    this.Raise_OnPropertyChanged("Screen");
                }
            }
        }
        private int _Screen;        

        /// <summary>
        /// The currently assigned audio device
        /// </summary>
        public AudioDevice AudioDevice
        {
            get
            {
                return this._AudioDevice;
            }
            set
            {
                // Disconnect from audio device events
                if (this._AudioDevice != null)
                    this._AudioDevice.OnUpdated -= _AudioDevice_OnUpdatedDelegate;

                this._AudioDevice = value;

                // Connect to audio device events
                this._AudioDevice.OnUpdated += _AudioDevice_OnUpdatedDelegate;
            }
        }
        private AudioDevice _AudioDevice;

        /// <summary>
        /// Reconnects the events
        /// </summary>
        internal void ConnectEvents()
        {
            this._AudioDevice.OnUpdated -= _AudioDevice_OnUpdatedDelegate;
            this._AudioDevice.OnUpdated += _AudioDevice_OnUpdatedDelegate;
        }

        /// <summary>
        /// Register a zone as included in this zone (must be uniqe)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool Add(Zone zone)
        {
            // Only add zones that can be subzones
            if (zone.BlockSubZoneUsage)
                return false;

            // See if this zone already exists
            int ZoneCheck = SubZones.Find(a => a == zone.ID);
            if (ZoneCheck != 0)
                return false;   // No, return error

            // New zone, add it
            SubZones.Add(zone.ID);

            // All good, return true
            return true;
        }

        /// <summary>
        /// Create a new zone, specifying the default screen and audiodevice
        /// </summary>
        /// <param name="Name">Name of zone</param>
        /// <param name="Description">Description for zone</param>
        /// <param name="Screen">Main screen</param>
        /// <param name="AudioDevice">Main audio device</param>
        public Zone(string Name, string Description, int Screen, AudioDevice AudioDevice)
            :this()
        {
            this.Name = Name;
            this.Description = Description;
            this.Screen = Screen;
            this.AudioDevice = AudioDevice;
            ID = String.Format("{0}{1}{2}{3}", Name, Description, Screen, AudioDevice).GetHashCode();
        }

        /// <summary>
        /// Do not use this constructor, included only for serialization
        /// </summary>
        private Zone()
        {
            _AudioDevice_OnUpdatedDelegate = new AudioDevice.UpdatedDelegate(_AudioDevice_OnUpdated);
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            if (_MediaHandler != null)
                _MediaHandler.Dispose();
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ID.ToString();
        }

        /// <summary>
        /// ID for this zone
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Compares two zones by looking at audiodevice and screen
        /// NB! This compare override will return true on ANY match for audiodevice or screen in the zone or any subzones
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Zone a, Zone b)
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

            if (a.SubZones.Count > 0 & b.SubZones.Count == 0)
            {
                foreach (int i in a.SubZones)
                {
                    if (BuiltInComponents.Host.ZoneHandler.GetZone(i) == b)
                        return true;                    
                }
                return false;
            }
            else if (a.SubZones.Count == 0 & b.SubZones.Count > 0)
            {
                foreach (int i in b.SubZones)
                {
                    if (BuiltInComponents.Host.ZoneHandler.GetZone(i) == a)
                        return true;
                }
                return false;
            }
            else if (a.SubZones.Count > 0 & b.SubZones.Count > 0)
            {
                foreach (int iA in b.SubZones)
                {
                    foreach (int iB in b.SubZones)
                    {
                        if (BuiltInComponents.Host.ZoneHandler.GetZone(iA) == BuiltInComponents.Host.ZoneHandler.GetZone(iB))
                            return true;
                    }
                }
                return false;
            }
            else
                return ((a.AudioDevice.Instance == b.AudioDevice.Instance) | (a.Screen == b.Screen));
        }

        /// <summary>
        /// Negates two zones by looking at audiodevice and screen
        /// NB! This compare override will return true on ANY match for audiodevice or screen in the zone or any subzones
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Zone a, Zone b)
        {
            
            return !(a==b);
        }

        /// <summary>
        /// AudioDevice updated delegate holder
        /// </summary>
        private AudioDevice.UpdatedDelegate _AudioDevice_OnUpdatedDelegate;

        /// <summary>
        /// Called when the audio device is changed
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="action"></param>
        /// <param name="value"></param>
        private void _AudioDevice_OnUpdated(AudioDevice audioDevice, AudioDevice.Actions action, object value)
        {
            Raise_OnVolumeChanged();
        }

        /// <summary>
        /// Volume changed delegate
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="audioDevice"></param>
        public delegate void VolumeChangedDelegate(Zone zone, AudioDevice audioDevice);

        /// <summary>
        /// Raised when the volume on the audiodevice changes
        /// </summary>
        public event VolumeChangedDelegate OnVolumeChanged;

        /// <summary>
        /// Raises the volume changed event
        /// </summary>
        private void Raise_OnVolumeChanged()
        {
            if (OnVolumeChanged != null)
                OnVolumeChanged(this, this.AudioDevice);
        }

        /// <summary>
        /// Volume changed delegate
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="propertyName"></param>
        public delegate void PropertyChangedDelegate(Zone zone, string propertyName);

        /// <summary>
        /// Raised when the volume on the audiodevice changes
        /// </summary>
        public event PropertyChangedDelegate OnPropertyChanged;

        /// <summary>
        /// Raises the property changed event
        /// </summary>
        private void Raise_OnPropertyChanged(string propertyName)
        {
            if (OnPropertyChanged != null)
                OnPropertyChanged(this, propertyName);
        }

        /// <summary>
        /// MediaProvider controller
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public MediaProviderHandler MediaHandler
        {
            get
            {
                return this._MediaHandler;
            }
            set
            {
                if (this._MediaHandler != value)
                {
                    this._MediaHandler = value;
                }
            }
        }
        private MediaProviderHandler _MediaHandler;
        
    }
}
