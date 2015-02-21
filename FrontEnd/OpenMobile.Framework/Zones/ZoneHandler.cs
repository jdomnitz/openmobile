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
using OpenMobile.Media;

namespace OpenMobile.Zones
{
    /// <summary>
    /// Methods for controlling zones
    /// </summary>
    public class ZoneHandler
    {
        private const string _ZoneAllName = "All";

        private List<Zone> _Zones = new List<Zone>();

        /// <summary>
        /// Creates a new handler for zones
        /// </summary>
        public ZoneHandler()
        {
            zone_OnVolumeChangedDelegate = new Zone.VolumeChangedDelegate(zone_OnVolumeChanged);
        }

        public void Dispose()
        {

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
        /// Gets a zone with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Zone GetZone(string name)
        {
            return _Zones.Find(x => x.Name == name);
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
        /// Returns true if the given zone is active on the specified screen
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool IsZoneActiveOnScreen(Zone zone, int screen)
        {
            return _ActiveZones[screen] == zone.ID;
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
                {
                    oldZone.OnVolumeChanged -= zone_OnVolumeChangedDelegate;
                }
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
                    OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.Name", BuiltInComponents.OMInternalPlugin.pluginName, zone.Index), zone.Name);
                    OM.Host.DataHandler.PushDataSourceValue(BuiltInComponents.OMInternalPlugin, String.Format("{0}.Zone{1}.AudioDevice", BuiltInComponents.OMInternalPlugin.pluginName, zone.Index), zone.AudioDevice.Name);
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
        /// Gets the index of the specified zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public int GetIndexForZone(Zone zone)
        {
            return _Zones.IndexOf(zone);
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
            {   // Restore successful
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
                if (_ActiveZones.Length > BuiltInComponents.Host.ScreenCount)
                    Array.Resize<int>(ref _ActiveZones, BuiltInComponents.Host.ScreenCount);

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

        private Dictionary<Zone, CommandGroup> _Zone_CommandGroup_Device = new Dictionary<Zone, CommandGroup>();

        private void Commands_Register()
        {
            // Create one set of commands per zone
            for (int i = 0; i < _Zones.Count; i++)
            {
                CommandGroup commandGroup = new CommandGroup(String.Format("{0}.Zone{1}", BuiltInComponents.OMInternalPlugin.pluginName, i), BuiltInComponents.OMInternalPlugin.pluginName, "Zone", "Device");
                _Zone_CommandGroup_Device.Add(_Zones[i], commandGroup);

                // Don't create any commands if a zone has sub zones 
                if (OM.Host.ZoneHandler.Zones[i].HasSubZones)
                    continue;

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Increment", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Decrement", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Mute", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Unmute", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Mute.Toggle", CommandExecutor, 0, false, "")));

                commandGroup.AddCommand(
                    OM.Host.CommandHandler.AddCommand(
                        new Command(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Set", CommandExecutor, 1, false, "")));
            }

            OM.Host.ForEachScreen((screen) =>
            {
                Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
                OM.Host.CommandHandler.ActivateCommandGroup(_Zone_CommandGroup_Device[zone], screen);
            });


            /*
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

                // Start media playback
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Play", CommandExecutor, 0, false, "Starts playback of current media"));

                // Start specific media playback
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.PlayURL", CommandExecutor, 1, false, "Starts playback of a specific url/file. Param0: url as string"));

                // Pause media playback
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Pause", CommandExecutor, 0, false, "Pauses playback of current media"));

                // Stop media playback
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Stop", CommandExecutor, 0, false, "Stops playback of current media"));

                // Goto to next item in current playlist
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Next", CommandExecutor, 0, false, "Goto to next item in playlist"));

                // Goto to previous item in current playlist
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Previous", CommandExecutor, 0, false, "Goto to previous item in playlist"));

                // Seek forward
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.SeekForward", CommandExecutor, 0, false, "Seek forward"));

                // Seek backward
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.SeekBackward", CommandExecutor, 0, false, "Seek backward"));

                // Activate shuffle
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Shuffle.Activate", CommandExecutor, 0, false, "Shuffles the current playlist"));

                // Deactivate shuffle
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Shuffle.Deactivate", CommandExecutor, 0, false, "Unshuffles the current playlist"));

                // Toggle shuffle
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Shuffle.Toggle", CommandExecutor, 0, false, "Toggles the shuffle state of the current playlist"));

                // Activate repeat
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Repeat.Activate", CommandExecutor, 0, false, "Activates repeat of the current playlist"));

                // Deactivate repeat
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Repeat.Deactivate", CommandExecutor, 0, false, "Deactivates repeat of the current playlist"));

                // Toggle repeat
                BuiltInComponents.Host.CommandHandler.AddCommand(new Command("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Repeat.Toggle", CommandExecutor, 0, false, "Toggles the repeat state of the current playlist"));
            }
            */
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = false;

            string commandName = command.FullNameWithoutScreen.Replace(BuiltInComponents.OMInternalPlugin.pluginName, "");

            Zone zone = null;
            if (command.NameLevel2.Contains("Zone"))
            {
                int zoneIndex = int.Parse(command.NameLevel2.Replace("Zone", ""));
                zone = OM.Host.ZoneHandler.Zones[zoneIndex];

                // Strip away unwanted information from the command name
                commandName = commandName.Replace(String.Format(".{0}.", command.NameLevel2), "");
            }

            try
            {
                switch (commandName)
                {
                    #region Volume.Increment

                    case "Volume.Increment":
                        {
                            result = true;

                            // Toggle mute if mute is active instead of adjusting volume
                            if (zone.AudioDevice.Mute)
                            {
                                zone.AudioDevice.Mute = false;
                                return "";
                            }

                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is int)
                                {   // increment step is given
                                    zone.AudioDevice.Volume += (int)param[0];
                                    return "";
                                }
                            }

                            // Fallback if no parameter is given
                            zone.AudioDevice.Volume += 5;
                            return "";
                        }

                    #endregion

                    #region Volume.Decrement

                    case "Volume.Decrement":
                        {
                            result = true;

                            // Toggle mute if mute is active instead of adjusting volume
                            if (zone.AudioDevice.Mute)
                            {
                                zone.AudioDevice.Mute = false;
                                return "";
                            }

                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is int)
                                {   // increment step is given
                                    zone.AudioDevice.Volume -= (int)param[0];
                                    return "";
                                }
                            }

                            // Fallback if no parameter is given
                            zone.AudioDevice.Volume -= 5;
                            return "";
                        }

                    #endregion

                    #region Volume.Mute

                    case "Volume.Mute":
                        {
                            result = true;
                            zone.AudioDevice.Mute = true;
                            return "";
                        }

                    #endregion

                    #region Volume.Unmute

                    case "Volume.Unmute":
                        {
                            result = true;
                            zone.AudioDevice.Mute = false;
                            return "";
                        }

                    #endregion

                    #region Volume.Mute.Toggle

                    case "Volume.Mute.Toggle":
                        {
                            result = true;
                            zone.AudioDevice.Mute = !zone.AudioDevice.Mute;
                            return "";
                        }

                    #endregion

                    #region Volume.Set

                    case "Volume.Set":
                        {
                            result = true;
                            if (Params.IsParamsValid(param, 1))
                            {
                                if (param[0] is int)
                                {   // Set volume
                                    zone.AudioDevice.Volume = (int)param[0];
                                    return "";
                                }
                                else
                                {
                                    return "Invalid datatype for Volume.Set";
                                }
                            }
                            else
                            {
                                return "Minimum one parameter is required";
                            }
                        }

                    #endregion

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        /*
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


            // Media handler: Play specific url
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.PlayURL")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Play(new mediaInfo(helperFunctions.DataHelpers.GetDataFromObject<string>(param[0])));

            // Media handler: Play current
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Play")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Play();

            // Media handler: Pause current
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Pause")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Pause();

            // Media handler: Stop
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Stop")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Stop();

            // Media handler: Next
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Next")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Next();

            // Media handler: Previous
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Previous")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Previous();

            // Media handler: Seek forward
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.SeekForward")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.SeekFwd();

            // Media handler: Seek backward
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.SeekBackward")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.SeekBwd();

            // Media handler: Shuffle
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Shuffle.Activate")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Shuffle = true;

            // Media handler: Unshuffle
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Shuffle.Deactivate")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Shuffle = false;

            // Media handler: Toggle shuffle
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Shuffle.Toggle")
            {
                int screen = GetScreenFromString(command.NameLevel1);
                Zone zone = GetZone(screen);
                if (zone != null)
                    zone.MediaHandler.Shuffle = !zone.MediaHandler.Shuffle;
            }

            // Media handler: Activate repeat
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Repeat.Activate")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Repeat = true;

            // Media handler: Deactivate repeat
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Repeat.Deactivate")
                GetZone(GetScreenFromString(command.NameLevel1)).MediaHandler.Repeat = false;

            // Media handler: Toggle repeat
            else if (command.NameLevel2 == "Zone" && command.NameLevel3 == "MediaProvider.Repeat.Toggle")
            {
                int screen = GetScreenFromString(command.NameLevel1);
                Zone zone = GetZone(screen);
                if (zone != null)
                    zone.MediaHandler.Repeat = !zone.MediaHandler.Repeat;
            }

            else
            {   // Default action
                result = false;
            }
            return result;
        }
        */

        private Dictionary<Zone, DataSourceGroup> _Zone_DataSourceGroup_Device = new Dictionary<Zone, DataSourceGroup>();

        private void DataSources_Register()
        {
            // Create one set of datasources per zone
            for (int i = 0; i < _Zones.Count; i++)
            {
                Zone zone = OM.Host.ZoneHandler.Zones[i];

                DataSourceGroup dataSourceGroup = new DataSourceGroup(String.Format("{0}.Zone{1}", BuiltInComponents.OMInternalPlugin.pluginName, i), BuiltInComponents.OMInternalPlugin.pluginName, "Zone", "Device");
                _Zone_DataSourceGroup_Device.Add(OM.Host.ZoneHandler.Zones[i], dataSourceGroup);

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume", 0, DataSource.DataTypes.percent, DataSourceGetter, "Current volume for this zone as integer")));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Volume.Mute", 0, DataSource.DataTypes.binary, DataSourceGetter, "Current mute state for this zone as bool")));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "Name", 0, DataSource.DataTypes.text, DataSourceGetter, "Name for this zone as string")));

                dataSourceGroup.AddDataSource(
                    OM.Host.DataHandler.AddDataSource(
                        new DataSource(BuiltInComponents.OMInternalPlugin, BuiltInComponents.OMInternalPlugin.pluginName, String.Format("Zone{0}", i), "AudioDevice", 0, DataSource.DataTypes.text, DataSourceGetter, "Name of Audiodevice for this zone as string")));

            }
            OM.Host.ForEachScreen((screen) =>
            {
                Zone zone = OM.Host.ZoneHandler.GetActiveZone(screen);
                OM.Host.DataHandler.ActivateDataSourceGroup(_Zone_DataSourceGroup_Device[zone], screen);
            });


            /*
            // Create data sources for zones at all available screens
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                //// Volume
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Volume", 0, DataSource.DataTypes.percent, ZoneDataProvider, "Volume for currently active zone at the screen"));

                //// Mute
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Volume.Mute", 0, DataSource.DataTypes.binary, ZoneDataProvider, "Mute state for currently active zone at the screen"));

                //// Zone name
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "Name", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name for currently active zone at the screen"));

                //// Zone audiodevice
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "AudioDevice", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name of Audiodevice for currently active zone at the screen"));

                //// Mediahandler: mediaInfo.Artist
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Artist", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Artist as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Album", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Album as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Genre", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Genre as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Length", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Length in seconds", -1));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Location", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Media location as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Name", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Name as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Rating", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Rating as int 0 - 5 (-1 is not set)", -1));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.TrackNumber", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: TrackNumber as int", -1));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Type", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Type of media as text", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.CoverArt", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: CoverArt of media as OImage", null));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Playback.Pos", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Current playback position as timespan", 0));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Playback.Length", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Length of current playback as timespan", 0));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaInfo.Playback.PosPercent", DataSource.DataTypes.percent, "MediaInfo from current mediaSource: Current playback position as percentage completed (int)", 0));

                //// MediaHandler: ProviderInfo
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaText1", DataSource.DataTypes.text, "MediaProvider: Media text string 1 as string", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaText2", DataSource.DataTypes.text, "MediaProvider: Media text string 2 as string", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaSource.Name", DataSource.DataTypes.text, "MediaProvider: Name of current media source as string", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaSource.Icon", DataSource.DataTypes.raw, "MediaProvider: Icon of current media source as OImage", null));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaType.Text", DataSource.DataTypes.raw, "MediaProvider: Text representing the current media type as string", String.Empty));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.MediaType.Icon", DataSource.DataTypes.raw, "MediaProvider: Icon of current media type as OImage", null));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Playback.Stopped", DataSource.DataTypes.binary, "MediaProvider: Playback is stopped as bool", false));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Playback.Playing", DataSource.DataTypes.binary, "MediaProvider: Playback is active as bool", false));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Playback.Paused", DataSource.DataTypes.binary, "MediaProvider: Playback is paused as bool", false));

                //// MediaHandler: Misc data
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Shuffle", DataSource.DataTypes.binary, "MediaProvider: Suffle state", false));
                //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource("OM", String.Format("Screen{0}", i), "Zone", "MediaProvider.Repeat", DataSource.DataTypes.binary, "MediaProvider: Repeat state", false));


                // Volume
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "Volume", "", 0, DataSource.DataTypes.percent, ZoneDataProvider, "Volume for currently active zone at the screen"));

                // Mute
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "Volume", "Mute", 0, DataSource.DataTypes.binary, ZoneDataProvider, "Mute state for currently active zone at the screen"));

                // Zone name
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "Name", "", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name for currently active zone at the screen"));

                // Zone audiodevice
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "AudioDevice", "", 0, DataSource.DataTypes.text, ZoneDataProvider, "Name of Audiodevice for currently active zone at the screen"));

                // Mediahandler: mediaInfo.Artist
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Artist", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Artist as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Album", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Album as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Genre", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Genre as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Length", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Length in seconds", -1));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Location", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Media location as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Name", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Name as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Rating", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Rating as int 0 - 5 (-1 is not set)", -1));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "TrackNumber", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: TrackNumber as int", -1));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Type", DataSource.DataTypes.text, "MediaInfo from current mediaSource: Type of media as text", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "CoverArt", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: CoverArt of media as OImage", null));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Playback.Pos", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Current playback position as timespan", 0));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Playback.Length", DataSource.DataTypes.raw, "MediaInfo from current mediaSource: Length of current playback as timespan", 0));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaInfo", "Playback.PosPercent", DataSource.DataTypes.percent, "MediaInfo from current mediaSource: Current playback position as percentage completed (int)", 0));

                // MediaHandler: ProviderInfo
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaText1", DataSource.DataTypes.text, "MediaProvider: Media text string 1 as string", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaText2", DataSource.DataTypes.text, "MediaProvider: Media text string 2 as string", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaSource.Name", DataSource.DataTypes.text, "MediaProvider: Name of current media source as string", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaSource.Icon", DataSource.DataTypes.raw, "MediaProvider: Icon of current media source as OImage", null));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaType.Text", DataSource.DataTypes.raw, "MediaProvider: Text representing the current media type as string", String.Empty));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "MediaType.Icon", DataSource.DataTypes.raw, "MediaProvider: Icon of current media type as OImage", null));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "Playback.Stopped", DataSource.DataTypes.binary, "MediaProvider: Playback is stopped as bool", false));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "Playback.Playing", DataSource.DataTypes.binary, "MediaProvider: Playback is active as bool", false));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "Playback.Paused", DataSource.DataTypes.binary, "MediaProvider: Playback is paused as bool", false));

                // MediaHandler: Misc data
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "Shuffle", DataSource.DataTypes.binary, "MediaProvider: Suffle state", false));
                BuiltInComponents.Host.DataHandler.AddDataSource(true, new DataSource("OM", "Zone", "MediaProvider", "Repeat", DataSource.DataTypes.binary, "MediaProvider: Repeat state", false));
            }
            */
        }

        private object DataSourceGetter(DataSource datasource, out bool result, object[] param)
        {
            result = true;

            string datasourceName = datasource.FullNameWithoutScreen.Replace(BuiltInComponents.OMInternalPlugin.pluginName, "");

            Zone zone = null;
            if (datasource.NameLevel2.Contains("Zone"))
            {
                int zoneIndex = int.Parse(datasource.NameLevel2.Replace("Zone", ""));
                zone = _Zones[zoneIndex];

                // Strip away unwanted information from the name
                datasourceName = datasourceName.Replace(String.Format(".{0}.", datasource.NameLevel2), "");
            }

            try
            {
                switch (datasourceName)
                {
                    #region Volume

                    case "Volume":
                        {
                            return zone.AudioDevice.Volume;
                        }

                    #endregion

                    #region Volume.Mute

                    case "Volume.Mute":
                        {
                            return zone.AudioDevice.Mute;
                        }

                    #endregion

                    #region Name

                    case "Name":
                        {
                            return zone.Name;
                        }

                    #endregion

                    #region AudioDevice

                    case "AudioDevice":
                        {
                            return zone.AudioDevice.Name;
                        }

                    #endregion

                    default:
                        break;
                }
            }
            catch
            {
                result = false;
            }

            return null;
        }


/*
        private object ZoneDataProvider(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;

            switch (dataSource.FullNameWithoutScreen)
            {
                case "Zone.Volume":
                    return GetZone(dataSource.Screen).AudioDevice.Volume;
                case "Zone.Volume.Mute":
                    return GetZone(dataSource.Screen).AudioDevice.Mute;
                case "Zone.Name":
                    return GetZone(dataSource.Screen).Name;
                case "Zone.AudioDevice":
                    return GetZone(dataSource.Screen).AudioDevice.Name;
                default:
                    break;
            }

            //// Update volume data
            //if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Volume")
            //    return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Volume;

            //// Update mute data
            //else if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Volume.Mute")
            //    return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Mute;

            //// Update Name data
            //else if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "Name")
            //    return GetZone(GetScreenFromString(dataSource.NameLevel1)).Name;

            //// Update AudioDevice data
            //else if (dataSource.NameLevel2 == "Zone" && dataSource.NameLevel3 == "AudioDevice")
            //    return GetZone(GetScreenFromString(dataSource.NameLevel1)).AudioDevice.Name;

            result = false;
            return null;
        }
*/
        private Zone.VolumeChangedDelegate zone_OnVolumeChangedDelegate;
        private void zone_OnVolumeChanged(Zone zone, AudioDevice audioDevice)
        {
            // Loop trough all active zones in case the same zone is active on more than one screen
            for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
            {
                if (_ActiveZones[i] == zone.ID)
                {
                    Raise_OnZoneUpdated(zone, i, false);
                }
            }
        }

        private string Timespan_Format(TimeSpan ts)
        {
            if (ts.TotalSeconds < 3600)
                return String.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            else
                return String.Format("{0:00}:{1:00}:{2:00}", ts.TotalHours, ts.Minutes, ts.Seconds);
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

                // Raise event
                BuiltInComponents.Host.raiseMediaEvent(eFunction.ZonesLoaded, null, String.Empty);
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
            // Try to find zone named "All", if found remove it so we can recreate it
            Zone ZoneAll = _Zones.Find(a => a.Name == _ZoneAllName);
            if (ZoneAll != null)
            {
                ZoneAll.Dispose();
                _Zones.Remove(ZoneAll);
            }

            Zone ScreenZone = new Zone(_ZoneAllName, "Autogenerated zone that includes all other zones", 0, BuiltInComponents.Host.GetAudioDeviceDefault());
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
            // Remove zone "all" before saving as this is regenerated at startup
            Zone ZoneAll = _Zones.Find(a => a.Name == _ZoneAllName);
            if (ZoneAll != null)
            {
                ZoneAll.Dispose();
                _Zones.Remove(ZoneAll);
            }

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
