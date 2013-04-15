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
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.Data;
using OpenMobile.helperFunctions;

namespace OpenMobile.Zones
{
    /// <summary>
    /// Methods for controlling zones
    /// </summary>
    public class ZoneHandler
    {
        private List<Zone> _Zones = new List<Zone>();

        /// <summary>
        /// Creates a new handler for zones
        /// </summary>
        public ZoneHandler()
        {
            zone_OnVolumeChangedDelegate = new Zone.VolumeChangedDelegate(zone_OnVolumeChanged);
        }

        /// <summary>
        /// List of available zones (Read only)
        /// </summary>
        public List<Zone> Zones
        {
            get
            {
                return _Zones;
            }
        }

        /// <summary>
        /// List of available base zones (a base sone is a sone without any subzones)
        /// </summary>
        public List<Zone> BaseZones
        {
            get
            {
                return _Zones.FindAll(x => x.SubZones.Count == 0);
            }
        }

        /// <summary>
        /// List of available zones (Read only)
        /// </summary>
        public List<Zone> GetAvailableZonesForScreen(int screen)
        {
            return _Zones.FindAll(x => x.CanZoneBeActivated(screen));
        }
        
        /// <summary>
        /// List of currently active zone id's per screen (Array index is screen number)(Read only)
        /// </summary>
        public int[] ActiveZones
        {
            get
            {
                return _ActiveZones;
            }
        }
        private int[] _ActiveZones = null;

        /// <summary>
        /// Gets a specific zone or the currently active zone for a screen
        /// <para>Param = Zone ID or screen number</para>
        /// </summary>
        /// <param name="ID">Zone ID or screen number</param>
        /// <returns>a zone</returns>
        public Zone GetZone(int ID)
        {
            // If ID is 99 or below then it is a screen number, otherwise it should be a zone ID
            if (System.Math.Abs(ID) < 99)
            {   // Get currently active zone for given screen number
                return GetActiveZone(ID);
            }
            else
            {   // Get zone based on ID
                return this[ID];
            }
        }

        /// <summary>
        /// Gets the first zone that uses the specified audio device instance
        /// </summary>
        /// <param name="AudioDeviceInstance">Instance of audio device</param>
        /// <returns></returns>
        public Zone GetZoneByAudioDeviceInstance(int AudioDeviceInstance)
        {
            return Zones.Find(x => x.AudioDevice.Instance == AudioDeviceInstance);
        }

        /// <summary>
        /// A wrapper for executing code on each zone (can be used with anonymous delegates)
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="d">delegate</param>
        public bool ForEachZoneInZone(Zone zone, ForEachZoneDelegate d)
        {
            // Errorcheck
            if (zone == null || d == null)
                return false;

            bool result = true;
            if (zone.SubZones.Count > 0)
            {   // Loop trough sub zones
                for (int i = 0; i < zone.SubZones.Count; i++)
                    result = ForEachZoneInZone(GetZone(zone.SubZones[i]), d);
            }
            else
            {   // No subzones available, use the original zone instead
                result = d(zone);
            }
            return result;
        }

        /// <summary>
        /// Gets the screen (if any) this zone is currently assigned to
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public int[] GetScreensForActiveZone(Zone zone)
        {
            List<int> Screens = new List<int>();
            for (int i = 0; i < System.Math.Min(_ActiveZones.Length, OM.Host.ScreenCount); i++)
            {
                if (_ActiveZones[i] == zone.ID)
                    Screens.Add(i);
            }
            return Screens.ToArray();
        }

        /// <summary>
        /// Gets the currently active zone for a screen
        /// </summary>
        /// <param name="Screen"></param>
        /// <returns>Null = no zone found for given screen or invalid screen number</returns>
        public Zone GetActiveZone(int Screen)
        {
            if ((Screen < 0) && (Screen > _ActiveZones.Length))
                return null;
            return this[_ActiveZones[Screen]];
        }

        /// <summary>
        /// Sets the active zone for a screen
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="zone"></param>
        /// <returns>True = sucessfully set</returns>
        public bool SetActiveZone(int Screen, Zone zone)
        {
            return _SetActiveZone_Internal(Screen, zone, false);
        }

        /// <summary>
        /// Sets the active zone for a screen
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="zone"></param>
        /// <param name="force">forces the update even when this zone is already activated</param>
        /// <returns>True = sucessfully set</returns>
        private bool _SetActiveZone_Internal(int Screen, Zone zone, bool force)
        {
            if ((Screen < 0) && (Screen > BuiltInComponents.Host.ScreenCount))
                return false;

            // Check if this zone is already set
            if (!force && _ActiveZones[Screen] == zone.ID)
                return true;

            // Does the zone allow us to activate it on this screen?
            if (!zone.CanZoneBeActivated(Screen))
                return false;

            // Disconnect from zone events
            if (_ActiveZones[Screen] > 0 && _ActiveZones[Screen] != zone.ID)
            {
                Zone oldZone = GetZone(_ActiveZones[Screen]);
                if (oldZone != null)
                    oldZone.OnVolumeChanged -= zone_OnVolumeChangedDelegate;
            }

            _ActiveZones[Screen] = zone.ID;

            // Connect to zone events
            zone.OnVolumeChanged += zone_OnVolumeChangedDelegate;

            // Refresh events in the zone
            zone.ConnectEvents();

            // Push data sources
            for (int i = 0; i < _ActiveZones.Length; i++)
            {
                if (_ActiveZones[i] == zone.ID)
                {
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue(String.Format("OM;Screen{0}.Zone.Name", i), zone.Name);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue(String.Format("OM;Screen{0}.Zone.AudioDevice", i), zone.AudioDevice.Name);

                    // Also push audio data to ensure all data is refreshed
                    PushDataSources_AudioDevice(i, zone.AudioDevice);
                }
            }

            // Raise events
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneSetActive, zone, Screen.ToString());
            Raise_OnZoneUpdated(zone, Screen, false);

            return true;
        }

        /// <summary>
        /// Gets the screen number a zone is active on (returns -1 if zone is not active)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public int GetScreenZoneIsActiveOn(Zone zone)
        {
            for (int i = 0; i < _ActiveZones.Length; i++)
                if (_ActiveZones[i] == zone.ID)
                    return i;
            return -1;
        }

        /// <summary>
        /// Sets the active zone for a screen
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="ZoneID">ID of zone</param>
        /// <returns>True = sucessfully set</returns>
        public bool SetActiveZone(int Screen, int ZoneID)
        {
            Zone z = this[ZoneID];
            if (z != null)
                return SetActiveZone(Screen, z);
            return false;
        }

        /// <summary>
        /// Register a new zone (must be uniqe)
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool Add(Zone zone)
        {
            // See if this zone already exists
            Zone z = _Zones.Find(a => a.Name == zone.Name);
            if (z != null)
                return false;   // No, return error

            // New zone, add it
            _Zones.Add(zone);

            // Refresh zone "all"
            UpdateZoneAll();

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneAdded, zone, "");

            // All good, return true
            return true;
        }

        /// <summary>
        /// Remove a zone 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool Remove(Zone zone)
        {
            // See if this zone already exists
            Zone z = _Zones.Find(a => a.Name == zone.Name);
            if (z == null)
                return true;   // No, return since there is nothing to do

            bool Result = _Zones.Remove(zone);

            // remove zone
            _Zones.Remove(zone);

            // Refresh zone "all"
            UpdateZoneAll();

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneRemoved, zone, "");

            // return result
            return Result;
        }

        /// <summary>
        /// Raises an update event for the zone (Call this method after changing data for a zone)
        /// </summary>
        /// <param name="zone"></param>
        public void Update(Zone zone)
        {
            // Refresh zone "all"
            UpdateZoneAll();

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneUpdated, zone, "");
        }

        /// <summary>
        /// Get a specific zone
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Zone this[string ID]
        {
            get
            {
                int id;
                if (int.TryParse(ID, out id))
                    return _Zones.Find(z => z.ID == id);
                return null;
            }
        }

        /// <summary>
        /// Get a specific zone
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Zone this[int ID]
        {
            get
            {
                return _Zones.Find(z => z.ID == ID);
            }
        }

        /// <summary>
        /// Get list of zones for a screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public List<Zone> GetZonesForScreen(int screen)
        {
            return _Zones.FindAll(z => z.Screen == screen);
        }

        /// <summary>
        /// Initializes and starts zones
        /// </summary>
        public void Start()
        {
            // Connect to host events
            BuiltInComponents.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
            
            // Initialize data
            _Zones = new List<Zone>();
            _ActiveZones = new int[BuiltInComponents.Host.ScreenCount];

            bool SetToDefault = false;

            // Restore settings from database (deserialize)
            if (StoredData.TryGetObjectXML<List<Zone>>(BuiltInComponents.OMInternalPlugin, "System.Zones", out _Zones))
            {   // Restore successfull
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Zones loaded from database");
            }
            else
            {   // Restore failed
                SetToDefault = true;
            }

            // Force reset of zones if nothing is available
            if (_Zones == null || (_Zones != null && _Zones.Count == 0))
                SetToDefault = true;

            // Restore active zones
            if (StoredData.TryGetObjectXML<int[]>(BuiltInComponents.OMInternalPlugin, "System.Zones.Active", out _ActiveZones))
            {   // Restore successfull
                BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Active Zones loaded from database");
            }
            else
            {   // Restore failed
                SetToDefault = true;
            }

            // Ensure we have a active zone for all screens
            if (!SetToDefault)
            {
                // Ensure we have valid active zones
                if (_ActiveZones != null)
                {
                    for (int i = 0; i < _ActiveZones.Length; i++)
                    {
                        Zone z = GetZone(_ActiveZones[i]);
                        if (z == null || z.AudioDevice == null)
                        {
                            SetToDefault = true;
                            break;
                        }
                    }
                }

                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                {
                    try
                    {
                        if (_ActiveZones[i] == 0)
                        {    // Find a zone to activate
                            Zone ZoneToActivate = _Zones.Find(x => x.Screen == i);
                            if (ZoneToActivate == null)
                            {    // No zone found for screen = error in setup, lets reset to default
                                SetToDefault = true;
                                break;
                            }
                            else
                            {
                                _ActiveZones[i] = ZoneToActivate.ID;
                            }
                        }
                    }
                    catch
                    {
                        _ActiveZones = new int[BuiltInComponents.Host.ScreenCount];
                        SetToDefault = true;
                        break;
                    }
                }
            }

            if (SetToDefault)
            {
                CreateDefaultZones();
            }

            // Create zone for "all"
            UpdateZoneAll();

            // Trigg events for activated zones
            for (int i = 0; i < _ActiveZones.Length; i++)
                _SetActiveZone_Internal(i, GetZone(_ActiveZones[i]), true);

            //Log data
            List<string> Texts = new List<string>();
            foreach (Zone zone in _Zones)
            {
                string s = "";
                foreach (int subZoneID in zone.SubZones)
                    s += subZoneID.ToString() + ";";
                Texts.Add(String.Format("{0}[{1}, Screen:{2}, Audio:{3}, SubZones:{4}] - {5}", zone.Name, zone, zone.Screen, zone.AudioDevice, s, zone.Description));
            }
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Available zones:", Texts.ToArray());

            // Register commands
            Commands_Register();
        }

        private void Commands_Register()
        {
            // Create commands for zones at all available screens
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                // Increment volume
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Increment", CommandExecutor, 0, false, "Increments volume on the active zone"));

                // Decrement volume
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Decrement", CommandExecutor, 0, false, "Decrements volume on the active zone"));

                // Mute
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Mute", CommandExecutor, 0, false, "Mutes volume on the active zone"));

                // Unmute
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Unmute", CommandExecutor, 0, false, "Unmutes volume on the active zone"));

                // Toggle mute
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Mute.Toggle", CommandExecutor, 0, false, "Toggles volume mute on the active zone"));

                // Sets volume to a specific level
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "Volume.Set", CommandExecutor, 1, false, "Sets volume on the active zone. Param0: Volume in percent as int"));
            }
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = true;

            // Increase volume
            if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Increment")
                GetZone(GetScreenFromString(command.NameLevel1)).AudioDevice.Volume += 5;

            // Increase volume
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Decrement")
                GetZone(GetScreenFromString(command.NameLevel1)).AudioDevice.Volume -= 5;

            // Mute
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Mute")
                GetZone(GetScreenFromString(command.NameLevel1)).AudioDevice.Mute = true;

            // Unmute
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Unmute")
                GetZone(GetScreenFromString(command.NameLevel1)).AudioDevice.Mute = false;

            // Toggle mute
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Mute.Toggle")
            {
                int screen = GetScreenFromString(command.NameLevel1);
                Zone zone = GetZone(screen);
                if (zone != null)
                    zone.AudioDevice.Mute = !zone.AudioDevice.Mute;
            }

            // Set volume
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "Volume.Set")
                GetZone(GetScreenFromString(command.NameLevel1)).AudioDevice.Volume = helperFunctions.DataHelpers.GetDataFromObject<int>(param[0]);

            else
            {   // Default action
                result = false;
            }
            return result;
        }

        private void DataSources_Register()
        {
            // Create data sources for zones at all available screens
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                // Volume
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Volume", 0, DataSource.DataTypes.percent, ZoneDataProvider, "Volume for currently active zone at the screen"));

                // Mute
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Volume.Mute", 0, DataSource.DataTypes.binary, ZoneDataProvider, "Mute state for currently active zone at the screen"));

                // Zone name
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Name", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name for currently active zone at the screen"));

                // Zone audiodevice
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "AudioDevice", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name of Audiodevice for currently active zone at the screen"));
            }
        }

        private object ZoneDataProvider(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;

            // Update volume data
            if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Volume")
                return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Volume;

            // Update mute data
            if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Volume.Mute")
                return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Mute;

            // Update Name data
            if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Name")
                return GetZone(GetScreenFromString(dataSource.NameLevel1)).Name;

            // Update AudioDevice data
            if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "AudioDevice")
                return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Name;

            result = false;
            return null;
        }

        private Zone.VolumeChangedDelegate zone_OnVolumeChangedDelegate;
        private void zone_OnVolumeChanged(Zone zone, AudioDevice audioDevice)
        {
            // Loop trough all active zones in case the same zone is active on more than one screen
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                if (_ActiveZones[i] == zone.ID)
                {
                    PushDataSources_AudioDevice(i, audioDevice);
                    Raise_OnZoneUpdated(zone, i, false);
                }
            }
        }

        private void PushDataSources_AudioDevice(int screen, AudioDevice audioDevice)
        {
            BuiltInComponents.Host.DataHandler.PushDataSourceValue(String.Format("OM;Screen{0}.Zone.Volume", screen), audioDevice.Volume);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue(String.Format("OM;Screen{0}.Zone.Volume.Mute", screen), audioDevice.Mute);
        }

        /// <summary>
        /// Extracts the screen number from a string like this "Screen0" returns 0 if it fails
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int GetScreenFromString(string s)
        {
            int screen = 0;
            int.TryParse(s.Substring(6), out screen);
            return screen;
        }

        private List<Notification> _StoredNotifications = new List<Notification>();

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.AudioDevicesAvailable)
            {
                for (int i = 0; i < _Zones.Count; i++)
                {
                    Zone zone = _Zones[i];

                    // Reconnect audio devices to zones
                    if (!zone.AudioDevice.IsValid)
                    {
                        if (zone.AudioDevice.IsDefault)
                        {   // Remap the default zone
                            zone.AudioDevice = AudioDevice.DefaultDevice;
                            zone.Valid = true;
                        }
                        else
                        {   // Remap to other audio device
                            AudioDevice audioDevice = BuiltInComponents.Host.getAudioDevice(zone.AudioDevice.Name);
                            if (audioDevice != null)
                            {
                                zone.AudioDevice = audioDevice;
                                zone.Valid = true;
                            }
                            else
                            {   // Mapping failed, may be caused by missing audio devices. 
                                zone.Valid = false;

                                // Create notification to user
                                _StoredNotifications.Add(new Notification(Notification.Styles.Warning, BuiltInComponents.OMInternalPlugin, null, null, String.Format("Zone \"{0}\" has an invalid audio device", zone.Name), String.Format("\"{0}\" is no longer available, zone will be disabled.", zone.AudioDevice.Name)));
                            }
                        }
                    }
                }

                DataSources_Register();
            }

            if (function == eFunction.pluginLoadingComplete)
            {
                // Show stored notifications
                for (int i = 0; i < _StoredNotifications.Count; i++)
                    BuiltInComponents.Host.UIHandler.AddNotification(_StoredNotifications[i]);
                _StoredNotifications.Clear();
            }
        }

        /// <summary>
        /// Creates the default zones
        /// </summary>
        public void CreateDefaultZones()
        {
            //TODO: Figure out a way to handle multiple screens and audiodevices

            // Log text
            List<string> Texts = new List<string>();

            _Zones = new List<Zone>();
            _ActiveZones = new int[BuiltInComponents.Host.ScreenCount];

            // Currently only one screen/device pr zone is supported
            // Create one default zone pr screen (default zone uses default audiodevice)
            for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
            {
                Zone zone = new Zone("Zone " + screen.ToString(), "Autogenerated zone for screen " + screen.ToString(), screen, BuiltInComponents.Host.GetAudioDeviceDefault());
                SetActiveZone(screen, zone);
                _Zones.Add(zone);

                // Raise event
                BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneAdded, zone, "");

                Texts.Add(String.Format("Zone '{0}' autogenerated and set as active for screen {1}.", zone.Name, screen));
            }

            //Log data
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Default zones created:", Texts.ToArray());

            // Update zone "all"
            UpdateZoneAll();

            // Create notification to user
            _StoredNotifications.Add(new Notification(Notification.Styles.Warning, BuiltInComponents.OMInternalPlugin, null, BuiltInComponents.Host.getSkinImage("Icons|Icon-MediaZone2").image, "Media zones set to default configuration", "Media zone configuration has been reset to default"));
        }

        private void UpdateZoneAll()
        {
            // Try to find zone named "All"
            Zone ZoneAll = _Zones.Find(a => a.Name == "All");
            _Zones.Remove(ZoneAll);

            Zone ScreenZone = new Zone("All", "Autogenerated zone that includes all other zones", 0, BuiltInComponents.Host.GetAudioDeviceDefault());
            ScreenZone.BlockSubZoneUsage = true;
            ScreenZone.AutoGenerated = true;
            ScreenZone.AllowedScreens = "0"; // This zone can only be activated on screen 0

            // Add all other zones to this zone
            foreach (Zone z in _Zones)
                ScreenZone.Add(z);

            // Add new zone for All
            _Zones.Add(ScreenZone);

            // Connect to zone events
            ScreenZone.OnVolumeChanged -= zone_OnVolumeChangedDelegate;
            ScreenZone.OnVolumeChanged += zone_OnVolumeChangedDelegate;

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneAdded, ScreenZone, "");
        }

        /// <summary>
        /// Save zone to database
        /// </summary>
        public void Save()
        {
            // Save data
            StoredData.SetObjectXML(BuiltInComponents.OMInternalPlugin, "System.Zones", _Zones);
            StoredData.SetObjectXML(BuiltInComponents.OMInternalPlugin, "System.Zones.Active", _ActiveZones);

            // Dispose zones
            foreach (Zone zone in _Zones)
                zone.Dispose();
        }

        /// <summary>
        /// Stop and clean zones
        /// </summary>
        public void Stop()
        {
            // Save data
            Save();
        }

        /// <summary>
        /// Zone updated delegate
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="screen"></param>
        public delegate void ZoneUpdatedDelegate(Zone zone, int screen);

        /// <summary>
        /// Raised when an event for a zone is received or the active zone is changed
        /// </summary>
        public event ZoneUpdatedDelegate OnZoneUpdated;

        /// <summary>
        /// Raises the zone changed event
        /// </summary>
        private void Raise_OnZoneUpdated(Zone zone, int screen, bool spawn)
        {
            ZoneUpdatedDelegate handler = OnZoneUpdated;
            if (handler != null)
            {
                if (spawn)
                {
                    // Spawn new thread for event update
                    OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                    {
                        handler(zone, screen);
                    });
                }
                else
                    handler(zone, screen);
            }
        }


    }
}
