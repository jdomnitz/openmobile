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


        private int[] _ActiveZones = null;

        /// <summary>
        /// List of currently active zone id's (Array index is screen number)(Read only)
        /// </summary>
        public int[] ActiveZones
        {
            get
            {
                return _ActiveZones;
            }
        }

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
        /// A wrapper for executing code on each zone (can be used with anonymous delegates)
        /// </summary>
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
                {
                    result = ForEachZoneInZone(GetZone(zone.SubZones[i]), d);
                    //if (!d(GetZone(zone.SubZones[i])))
                    //    result = false;
                    //
                }
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
            for (int i = 0; i < _ActiveZones.Length; i++)
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
            if ((Screen < 0) && (Screen > BuiltInComponents.Host.ScreenCount))
                return false;

            // Does the zone allow us to activate it on this screen?
            if (!zone.CanZoneBeActivated(Screen))
                return false;

            _ActiveZones[Screen] = zone.ID;

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneSetActive, zone, Screen.ToString());

            return true;
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
        /// <param name="Name"></param>
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
        /// <param name="Name"></param>
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
        /// <param name="Name"></param>
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
            // Initialize data
            //DefaultZones = new int[BuiltInComponents.Host.ScreenCount];
            _ActiveZones = new int[BuiltInComponents.Host.ScreenCount];

            // Restore settings from database (deserialize)
            bool SetToDefault = false;
            string XML = StoredData.Get("System.Zones");
            if (!string.IsNullOrEmpty(XML))
            {
                try
                {
                    _Zones = OpenMobile.helperFunctions.XML.Serializer.fromXML<List<Zone>>(XML);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Zones loaded from database");
                }
                catch
                {   // Error while loading, reset to defaults
                    SetToDefault = true;
                }
                if (_Zones.Count == 0)
                    SetToDefault = true;
            }
            else
            {   // No setting can be read from database, set to default
                SetToDefault = true;
            }

            // Restore active zones
            XML = StoredData.Get("System.Zones.Active");
            if (!string.IsNullOrEmpty(XML))
            {
                try
                {
                    _ActiveZones = OpenMobile.helperFunctions.XML.Serializer.fromXML<int[]>(XML);
                    BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Active Zones loaded from database");
                }
                catch
                {   // Error while loading, reset to defaults
                    SetToDefault = true;
                }
            }
            else
                // Error detected, force default settings 
                SetToDefault = true;

            // Ensure we have valid active zones
            for (int i = 0; i < _ActiveZones.Length; i++)
            {
                if (GetZone(_ActiveZones[i]) == null)
                    SetToDefault = true;
            }

            if (!SetToDefault)
            {
                // Ensure we have a active zone for all screens
                for (int i = 0; i < BuiltInComponents.Host.ScreenCount; i++)
                {
                    try
                    {
                        if (_ActiveZones[i] == null)
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
                CreateDefaultZones();

            // Create zone for "all"
            UpdateZoneAll();

            // Trigg events for activated zones
            for (int i = 0; i < _ActiveZones.Length; i++)
                BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneSetActive, GetZone(_ActiveZones[i]), i.ToString());

            //Log data
            List<string> Texts = new List<string>();
            foreach (Zone zone in _Zones)
            {
                string s = "";
                foreach (int subZoneID in zone.SubZones)
                    s += subZoneID.ToString() + ";";
                string a = zone.AudioDeviceName;
                if (String.IsNullOrEmpty(a))
                    a = zone.AudioDeviceInstance.ToString();
                Texts.Add(String.Format("{0}[{1}, Screen:{2}, Audio:{3}, SubZones:{4}] - {5}", zone.Name, zone, zone.Screen, a, s, zone.Description));
            }
            BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "Available zones:", Texts.ToArray());
        }

        public void CreateDefaultZones()
        {
            //TODO: Figure out a way to handle multiple screens and audiodevices

            // Log text
            List<string> Texts = new List<string>();

            // Remove current zones
            _Zones.Clear();
            _ActiveZones = new int[BuiltInComponents.Host.ScreenCount];

            // Currently only one screen/device pr zone is supported
            // Create one default zone pr screen (default zone uses default audiodevice)
            for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
            {
                Zone zone = new Zone("Zone " + screen.ToString(), "Autogenerated zone for screen " + screen.ToString(), screen, BuiltInComponents.Host.GetAudioDeviceDefaultName());
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
        }

        private void UpdateZoneAll()
        {
            // Try to find zone named "All"
            Zone ZoneAll = _Zones.Find(a => a.Name == "All");
            _Zones.Remove(ZoneAll);

            Zone ScreenZone = new Zone("All", "Autogenerated zone that includes all other zones", 0, BuiltInComponents.Host.GetAudioDeviceDefaultName());
            ScreenZone.BlockSubZoneUsage = true;
            ScreenZone.AutoGenerated = true;
            ScreenZone.AllowedScreens = "0"; // This zone can only be activated on screen 0

            // Add all other zones to this zone
            foreach (Zone z in _Zones)
                ScreenZone.Add(z);

            // Add new zone for All
            _Zones.Add(ScreenZone);

            // Raise event
            BuiltInComponents.Host.raiseMediaEvent(eFunction.ZoneAdded, ScreenZone, "");
        }

        /// <summary>
        /// Save zone to database
        /// </summary>
        public void Save()
        {
            // Save data
            StoredData.Set("System.Zones", OpenMobile.helperFunctions.XML.Serializer.toXML(_Zones));
            StoredData.Set("System.Zones.Active", OpenMobile.helperFunctions.XML.Serializer.toXML(_ActiveZones));
        }

        /// <summary>
        /// Stop and clean zones
        /// </summary>
        public void Stop()
        {
            // Save data
            Save();
        }
    }
}
