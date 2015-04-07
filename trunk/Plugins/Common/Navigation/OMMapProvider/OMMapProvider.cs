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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.NavEngine;
using OpenMobile.helperFunctions;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using GMap.NET.ImageRender;
using System.Collections.Generic;
using System.Text;

namespace OMMapProvider
{
    public sealed class OMMapProvider : BasePluginCode, IDataSource
    {
        #region Private variables

        private enum MapDrawMode
        {
            /// <summary>
            /// Automatic changeover between day/night
            /// </summary>
            Auto,

            /// <summary>
            /// Always daytime rendering
            /// </summary>
            Day,

            /// <summary>
            /// Always nighttime rendering
            /// </summary>
            Night
        }

        private enum MapBearingMode
        {
            North,
            Travel
        }

        private GMapImageRender[] _Map;
        private GMapOverlay[] _Map_Overlay;// = new GMapOverlay();
        private GMapOverlay[] _Map_Overlay_Objects;// = new GMapOverlay("objects");
        private GMapOverlay[] _Map_Overlay_Routes;// = new GMapOverlay("routes");
        private GMapOverlay[] _Map_Overlay_Polygons;// = new GMapOverlay("polygons");
        private Dictionary<string, GMapOverlay[]> _Map_Overlay_External;
        private PointLatLng[] _Map_Point_RouteStart;// = new PointLatLng();
        private PointLatLng[] _Map_Point_RouteEnd;// = new PointLatLng();
        private GMapMarker[] _Map_Marker_CurrentPosition;
        private GMapMarker[] _Map_Marker_HomePosition;
        private GMapMarker[] _Map_Marker_RouteStart;
        private GMapMarker[] _Map_Marker_RouteEnd;
        private bool[] _Map_FollowsCurrentLoc;// = true;
        private List<GMapProvider> _Map_AvailableProviders;
        private GMapProvider _Map_CurrentProvider;
        private AccessMode _Map_AccessMode;
        private bool _SettingsChanged = false;
        private NavigationRoute[] _Map_ActiveNavigationRoute;

        private OImage[] _MapImage;
        private Size[] _MapSize;

        private bool _MapNightMode = false;
        private MapDrawMode _MapDrawMode = MapDrawMode.Auto;
        private MapBearingMode _Map_BearingMode = MapBearingMode.North;

        #endregion

        #region Constructors and init

        public OMMapProvider()
            : base("OMMapProvider", OM.Host.getSkinImage("Icons|Icon-DataSource"), 1f, "Backend provider for Map/Navigation", "OM Dev team", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Init variables to support multiple screens
            _Map = new GMapImageRender[OM.Host.ScreenCount];
            _Map_Overlay = new GMapOverlay[OM.Host.ScreenCount];
            _Map_Overlay_Objects = new GMapOverlay[OM.Host.ScreenCount];
            _Map_Overlay_Routes = new GMapOverlay[OM.Host.ScreenCount];
            _Map_Overlay_Polygons = new GMapOverlay[OM.Host.ScreenCount];
            _Map_Overlay_External = new Dictionary<string, GMapOverlay[]>();
            _Map_Point_RouteStart = new PointLatLng[OM.Host.ScreenCount];
            _Map_Point_RouteEnd = new PointLatLng[OM.Host.ScreenCount];
            _Map_Marker_CurrentPosition = new GMapMarker[OM.Host.ScreenCount];
            _Map_Marker_HomePosition = new GMapMarker[OM.Host.ScreenCount];
            _Map_Marker_RouteStart = new GMapMarker[OM.Host.ScreenCount];
            _Map_Marker_RouteEnd = new GMapMarker[OM.Host.ScreenCount];
            _Map_FollowsCurrentLoc = new bool[OM.Host.ScreenCount];
            _Map_ActiveNavigationRoute = new NavigationRoute[OM.Host.ScreenCount];
            _MapImage = new OImage[OM.Host.ScreenCount];
            _MapSize = new Size[OM.Host.ScreenCount];
            OM.Host.ForEachScreen((screen) =>
                {
                    _Map_Overlay[screen] = new GMapOverlay();
                    _Map_Overlay_Objects[screen] = new GMapOverlay("objects");
                    _Map_Overlay_Routes[screen] = new GMapOverlay("routes");
                    _Map_Overlay_Polygons[screen] = new GMapOverlay("polygons");
                    _Map_Point_RouteStart[screen] = new PointLatLng();
                    _Map_Point_RouteEnd[screen] = new PointLatLng();
                    _Map_FollowsCurrentLoc[screen] = true;
                    _MapSize[screen] = new Size(OM.Host.ClientFullArea.Width, OM.Host.ClientFullArea.Height);
                });

            // Settings
            Settings_SetDefaultValues();
            Settings_MapVariables();

            Datasources_Register();
            DataSources_Subscribe();

            Commands_Register();

            OM.Host.ForEachScreen((screen) =>
                {
                    OpenMobile.OSSpecific.Windows.Threading.MessagePump.CreateMessagePump(this.pluginName, () =>
                    {
                        // The following two lines is a "hack" to make threading work without affecting the content of variables between threads
                        int s = new int();
                        s = screen;
                        Map_Initialize(s, _MapSize[s].Width, _MapSize[s].Height);
                    });
                });
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            return eLoadStatus.LoadSuccessful;
        }

        #endregion

        #region Settings

        protected override void Settings()
        {
            base.MySettings.Add(Setting.TextList<GMapProvider>("OMMaps.MapProvider", "Select provider", "Available map providers", StoredData.Get(this, "OMMaps.MapProvider"), _Map_AvailableProviders));
            base.MySettings.Add(Setting.EnumSetting<AccessMode>("OMMaps.MapMode", "Map mode", "NB! Server requires internet connection", StoredData.Get(this, "OMMaps.MapMode")));
            base.MySettings.Add(Setting.EnumSetting<MapDrawMode>("OMMaps.MapDrawMode", "Map rendering", "Day / night changeover mode", StoredData.Get(this, "OMMaps.MapDrawMode")));
            base.MySettings.Add(Setting.EnumSetting<MapBearingMode>("OMMaps.MapBearingMode", "Map up direction", "Bearing mode of map", StoredData.Get(this, "OMMaps.MapBearingMode")));
        }

        private void Settings_MapVariables()
        {
            if (_Map_AvailableProviders == null)
                _Map_AvailableProviders = GMapProviders.List;

            _Map_CurrentProvider = _Map_AvailableProviders.Find(x => x.Name == StoredData.Get(this, "OMMaps.MapProvider"));
            _Map_AccessMode = StoredData.GetEnum<AccessMode>(this, "OMMaps.MapMode", AccessMode.ServerAndCache);
            _MapDrawMode = StoredData.GetEnum<MapDrawMode>(this, "OMMaps.MapDrawMode", MapDrawMode.Auto);
            _Map_BearingMode = StoredData.GetEnum<MapBearingMode>(this, "OMMaps.MapBearingMode", MapBearingMode.North);
        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, "OMMaps.MapProvider", GMapProviders.OpenStreetMap);
            StoredData.SetDefaultValue(this, "OMMaps.MapMode", AccessMode.ServerAndCache);
            StoredData.SetDefaultValue(this, "OMMaps.MapDrawMode", MapDrawMode.Auto);
            StoredData.SetDefaultValue(this, "OMMaps.MapBearingMode", MapBearingMode.North);
            Settings_MapVariables();
        }

        protected override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);

            // Update local variables
            Settings_MapVariables();

            // Inform skin that settings was changed
            _SettingsChanged = true;

            if (setting.Name == "OMMaps.MapDrawMode")
            {
                // Refresh maps
                OM.Host.ForEachScreen((x) =>
                {
                    if (_Map[x] != null)
                        _Map[x].Refresh();
                });
            }

            if (setting.Name == "OMMaps.MapBearingMode")
            {
                // Refresh maps
                OM.Host.ForEachScreen((x) =>
                {
                    if (_Map[x] != null)
                    {
                        if (_Map_BearingMode == MapBearingMode.North)
                            _Map[x].Bearing = 0;
                        else
                            _Map[x].Bearing = (float)((double)OM.Host.DataHandler["GPS.Bearing.Angle"]);

                        _Map[x].Refresh();

                    }
                });
            }

        }

        #endregion

        #region Datasources

        void Datasources_Register()
        {
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "Image", "", 0, DataSource.DataTypes.image, null, "Map image as OImage"), null);
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "Busy", "", 0, DataSource.DataTypes.binary, null, "Map is busy (refreshing) as bool"), true);
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "RouteData", "", 0, DataSource.DataTypes.raw, null, "Map route object as OpenMobile.NavEngine.NavigationRoute"), null);
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "Size", "", 0, DataSource.DataTypes.raw, null, "Map size as Size"), new Size(OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height));
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "Show", "MapPlugin", 0, DataSource.DataTypes.binary, null, "True indicates that the map panel should be shown"), false);
            OM.Host.DataHandler.AddDataSource(true, new DataSource(this, "Map", "Zoom", "Level", 0, DataSource.DataTypes.raw, null, "The current zoom level of the map (1 (no zoom) - 17 (max zoom)) as double"), (int)17);
        }

        Location _LastLookupLocation;

        void DataSources_Subscribe()
        {
            // Subscribe to day / night change over datasource
            OM.Host.DataHandler.SubscribeToDataSource("OM;Location.Current.Daytime", (x) =>
                {
                    // Set day/night mode
                    _MapNightMode = !(bool)x.Value;

                    // Refresh maps
                    OM.Host.ForEachScreen((screen) =>
                        {
                            if (_Map[screen] != null)
                                _Map[screen].Refresh();
                        });
                });

            // Subscribe to bearing changes

            OM.Host.DataHandler.SubscribeToDataSource("GPS.Bearing.Angle", (x) =>
                {
                    // Only accept bearing changes when moving
                    try
                    {
                        object value = OM.Host.DataHandler["GPS.Speed.KMH"];
                        if (value != null)
                        {
                            double speed = (double)value;
                            if (speed > 2.0)
                            {
                                OM.Host.ForEachScreen((screen) =>
                                    {
                                        if (_Map[screen] != null)
                                            _Map[screen].Bearing = (float)((double)x.Value);
                                    });
                            }
                        }
                    }
                    catch
                    {
                    }

                });

            // Subscribe to datasource updates for user's home locations
            OM.Host.DataHandler.SubscribeToDataSource("OM;Location.Favorite.Home", (x) =>
            {   // Reverse geocoding lookup (to get additional data for home location)
                if (OM.Host.InternetAccess)
                {
                    Location loc = x.Value as Location;

                    if (loc != _LastLookupLocation)
                    {
                        try
                        {
                            // First do a lookup to get lat/long
                            PointLatLng position;
                            GeoCoderStatusCode statusCode = GetPositionByKeywords(loc.Keyword, out position);
                            if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                // Save lat/long to OM location
                                loc.Latitude = (float)position.Lat;
                                loc.Longitude = (float)position.Lng;

                                // Do additional lookup to get more data
                                Placemark placemark;
                                statusCode = GetPlacemark(position, out placemark);
                                if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                                {
                                    loc.Country = placemark.CountryName;
                                    loc.City = placemark.LocalityName;
                                    loc.Address = placemark.Address;
                                    loc.State = placemark.AdministrativeAreaName;
                                    loc.Street = placemark.ThoroughfareName;
                                    loc.Zip = placemark.PostalCodeNumber;
                                }
                            }
                        }
                        catch
                        {   // Mask errors
                        }

                        // Push data back to OM
                        _LastLookupLocation = loc;
                        BuiltInComponents.SystemSettings.Location_Home = loc;
                    }
                }
            });
        }

        #endregion

        #region Commands

        void Commands_Register()
        {
            // Provide a command to go straight to the wininfo panel
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Show", "MapPlugin", CommandExecutor, 0, true, "Tells the map plugin to goto the map view panel"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Goto", "Location", CommandExecutor, 1, true, "Moves the map to the specified location. A location can be either a keyword (New York), full address or a latitude / longitude (with this format: 0.0,0.0). Returns an empty string if command was successfull, otherwise an error code."));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Goto", "Current", CommandExecutor, 0, true, "Moves the map to the current location."));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Size", "Set", CommandExecutor, 2, false, "Set's the current size of the map graphic. Param0: Map size as Size / Param0: Map width as int, Param1: Map height as int"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Zoom", "Level", CommandExecutor, 1, false, "Set's the current zoom level of the map (1 (no zoom) - 17 (max zoom))"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Zoom", "In", CommandExecutor, 0, false, "Zoom in on the map"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Zoom", "Out", CommandExecutor, 0, false, "Zoom out on the map"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Route", "", CommandExecutor, 2, true, "Routes from a start location (leave empty for current location) to a end location. A location can be either a keyword (New York), full address or a latitude / longitude (with this format: 0.0,0.0). Returns an empty string if command was successfull, otherwise an error code."));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Route", "ByMarkers", CommandExecutor, 0, true, "Routes from start to end location. Locations are read from the current start and end markers at the map. Returns an empty string if command was successfull, otherwise an error code."));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Mouse", "EmulateMouseDown", CommandExecutor, 2, false, "Emulates a mouse action on the map. Param0: Mouse button as System.Windows.Forms.MouseButtons, Param1: Mouse position as Point"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Mouse", "EmulateMouseUp", CommandExecutor, 2, false, "Emulates a mouse action on the map. Param0: Mouse button as System.Windows.Forms.MouseButtons, Param1: Mouse position as Point"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Mouse", "EmulateMouseMove", CommandExecutor, 2, false, "Emulates a mouse action on the map. Param0: Mouse button as System.Windows.Forms.MouseButtons, Param1: Mouse position as Point"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Mouse", "EmulateMouseClick", CommandExecutor, 2, false, "Emulates a mouse action on the map. Param0: Mouse button as System.Windows.Forms.MouseButtons, Param1: Mouse position as Point"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Refresh", "", CommandExecutor, 0, false, "Forces a refresh of the map and a reload of settings"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Marker", "Add", CommandExecutor, 4, false, "Adds a marker to the map. Param0: Location, Param1: OwnerPluginName, Param2:MarkerName, Param3: Marker type 1-37 or use OImage for custom marker image, Param4: Marker text "));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Marker", "Delete", CommandExecutor, 2, false, "Removes a marker from the map. Param0: OwnerPluginName, Param1:MarkerName"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Marker", "DeleteAll", CommandExecutor, 1, false, "Removes all markers from the map that belongs to the specific plugin. Param0: OwnerPluginName"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Marker", "ZoomAll", CommandExecutor, 1, false, "Zoomes out or in to show all markers on the map. Param0: OwnerPluginName"));
            OM.Host.CommandHandler.AddCommand(true, new Command(this, "Map", "Markers", "Clear", CommandExecutor, 0, false, "Removes all markers from the map regardless of who added it."));
            OM.Host.CommandHandler.AddCommand(new Command(this, "Map", "Lookup", "Location", CommandExecutor, 1, true, "Fills in additional data to a Location by GeoLookup"));
        }

        private object CommandExecutor(Command command, object[] param, out bool result)
        {
            result = false;

            try
            {
                switch (command.FullNameWithoutScreen)
                {
                    #region Show map
                    case "Map.Show.MapPlugin":
                        {
                            result = true;
                            OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Show.MapPlugin", true, true);
                        }
                        break;

                    #endregion

                    #region Lookup

                    case "Map.Lookup.Location":
                        {
                            if (Params.IsParamsValid(param, 1))
                            {
                                result = true;
                                Location loc = Params.GetParam<Location>(param, 0);

                                if (loc.Latitude == 0 && loc.Longitude == 0)
                                {
                                    if (!String.IsNullOrEmpty(loc.Keyword))
                                    {
                                        // First do a lookup to get lat/long
                                        PointLatLng position;
                                        GeoCoderStatusCode statusCode = GetPositionByKeywords(loc.Keyword, out position);
                                        if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                                        {
                                            // Save lat/long to location
                                            loc.Latitude = (float)position.Lat;
                                            loc.Longitude = (float)position.Lng;
                                        }
                                    }
                                    else
                                    {
                                        // Try to build keywords from already present data
                                        string keywords = loc.ToKeyword();

                                        // First do a lookup to get lat/long
                                        PointLatLng position;
                                        GeoCoderStatusCode statusCode = GetPositionByKeywords(keywords, out position);
                                        if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                                        {
                                            // Save lat/long to location
                                            loc.Latitude = (float)position.Lat;
                                            loc.Longitude = (float)position.Lng;
                                        }
                                    }
                                }

                                if (loc.Latitude != 0 && loc.Longitude != 0)
                                {
                                    // Do additional lookup to get more data
                                    Placemark placemark;
                                    GeoCoderStatusCode statusCode = GetPlacemark(ConvertOMLocationToLatLng(loc), out placemark);
                                    if (statusCode == GeoCoderStatusCode.G_GEO_SUCCESS)
                                    {
                                        loc.Country = placemark.CountryName;
                                        loc.City = placemark.LocalityName;
                                        loc.Address = placemark.Address;
                                        loc.State = placemark.AdministrativeAreaName;
                                        loc.Street = placemark.ThoroughfareName;
                                        loc.Zip = placemark.PostalCodeNumber;
                                    }
                                }
                                return loc;
                            }

                        }
                        break;


                    #endregion

                    #region markers

                    case "Map.Marker.ZoomAll":
                        {
                            if (Params.IsParamsValid(param, 1))
                            {
                                // Extract parameters
                                string ownerPlugin = Params.GetParam<string>(param, 0, String.Empty);

                                // Cancel command if we're missing parameters
                                if (String.IsNullOrEmpty(ownerPlugin))
                                    return null;

                                // Mark command as accepted
                                result = true;

                                if (_Map_Overlay_External.ContainsKey(ownerPlugin))
                                {
                                    GMapOverlay[] markerOverlays = _Map_Overlay_External[ownerPlugin];
                                    _Map[command.Screen].ZoomAndCenterMarkers(markerOverlays[command.Screen].Id);
                                }
                                _Map[command.Screen].Refresh();
                            }
                        }
                        break;

                    case "Map.Markers.Clear":
                        {
                            result = true;
                            // Clear ALL markers
                            foreach (var key in _Map_Overlay_External.Keys)
                            {
                                GMapOverlay[] markerOverlays = _Map_Overlay_External[key];
                                markerOverlays[command.Screen].Markers.Clear();
                            }
                            _Map[command.Screen].Refresh();
                        }
                        break;

                    case "Map.Marker.DeleteAll":
                        {
                            if (Params.IsParamsValid(param, 1))
                            {
                                // Extract parameters
                                string ownerPlugin = Params.GetParam<string>(param, 0, String.Empty);

                                // Cancel command if we're missing parameters
                                if (String.IsNullOrEmpty(ownerPlugin))
                                    return null;

                                // Mark command as accepted
                                result = true;

                                if (_Map_Overlay_External.ContainsKey(ownerPlugin))
                                {
                                    GMapOverlay[] markerOverlays = _Map_Overlay_External[ownerPlugin];
                                    markerOverlays[command.Screen].Markers.Clear();
                                }
                                _Map[command.Screen].Refresh();
                            }
                        }
                        break;

                    case "Map.Marker.Delete":
                        {
                            if (Params.IsParamsValid(param, 2))
                            {
                                // Extract parameters
                                string ownerPlugin = Params.GetParam<string>(param, 0, String.Empty);
                                string markerName = Params.GetParam<string>(param, 1, String.Empty);

                                // Cancel command if we're missing parameters
                                if (String.IsNullOrEmpty(ownerPlugin) || String.IsNullOrEmpty(markerName))
                                    return null;

                                // Mark command as accepted
                                result = true;

                                string markerFullName = String.Format("{0}_{1}", ownerPlugin, markerName);

                                // Check for marker already present
                                if (_Map_Overlay_External.ContainsKey(ownerPlugin))
                                {
                                    GMapOverlay[] markerOverlays = _Map_Overlay_External[ownerPlugin];

                                    // Try to find marker
                                    GMapMarker marker = null;
                                    foreach (var tmpMarker in markerOverlays[command.Screen].Markers)
                                    {
                                        string tmpName = tmpMarker.Tag as string;
                                        if (tmpName.Equals(markerName))
                                        {   // Marker is found, update data
                                            marker = tmpMarker;
                                            break;
                                        }
                                    }
                                    if (marker != null)
                                    {   // Remove marker
                                        markerOverlays[command.Screen].Markers.Remove(marker);
                                    }
                                    _Map[command.Screen].Refresh();

                                }
                            }
                        }
                        break;

                    case "Map.Marker.Add":
                        {   // Param0: Location, Param1: OwnerPluginName, Param2:MarkerName, Param3: Marker type 1-37 or use OImage for custom marker image, Param4: Marker text
                            if (Params.IsParamsValid(param, 3))
                            {
                                // Extract location
                                Location loc;
                                if (param[0] is string)
                                {
                                    string location = param[0] as string;
                                    if (!Location.TryParseLatLng(location, out loc))
                                        // Cancel command
                                        return null;
                                }
                                else if (param[0] is Location)
                                {
                                    loc = Params.GetParam<Location>(param, 0, null);
                                }
                                else
                                {   // Cancel command
                                    return null;
                                }
                                if (loc == null || loc.IsEmpty())
                                    return null;

                                // Extract parameters
                                string ownerPlugin = Params.GetParam<string>(param, 1, String.Empty);
                                string markerName = Params.GetParam<string>(param, 2, String.Empty);

                                // Cancel command if we're missing parameters
                                if (String.IsNullOrEmpty(ownerPlugin) || String.IsNullOrEmpty(markerName))
                                    return null;

                                // Mark command as accepted
                                result = true;

                                string markerFullName = String.Format("{0}_{1}", ownerPlugin, markerName);

                                GMapOverlay[] markerOverlays = null;
                                // Check for marker layer already present
                                if (!_Map_Overlay_External.ContainsKey(ownerPlugin))
                                {   // Add new overlay
                                    markerOverlays = new GMapOverlay[OM.Host.ScreenCount];
                                    for (int i = 0; i < OM.Host.ScreenCount; i++)
                                    {
                                        GMapOverlay overlay = new GMapOverlay(ownerPlugin);
                                        markerOverlays[i] = overlay;
                                        _Map[i].Overlays.Add(overlay);
                                        _Map_Overlay_External.Add(ownerPlugin, markerOverlays);
                                    }
                                }
                                else
                                {   // Overlay already present
                                    markerOverlays = _Map_Overlay_External[ownerPlugin];
                                }

                                // Try to find marker
                                GMapMarker marker = null;
                                foreach (var tmpMarker in markerOverlays[command.Screen].Markers)
                                {
                                    string tmpName = tmpMarker.Tag as string;
                                    if (tmpName.Equals(markerName))
                                    {   // Marker is found, update data
                                        marker = tmpMarker;
                                        break;
                                    }
                                }

                                // Is this a new marker?
                                if (marker == null)
                                {   // New marker
                                    if (param[3] is OImage)
                                    {   // Custom marker image
                                        OImage img = Params.GetParam<OImage>(param, 3, null);
                                        if (img == null)
                                            marker = new GMarkerGoogle(ConvertOMLocationToLatLng(loc), GMarkerGoogleType.arrow);
                                        else
                                            marker = new GMarkerGoogle(ConvertOMLocationToLatLng(loc), img.image);
                                    }
                                    else
                                    {   // Marker with basic image
                                        marker = new GMarkerGoogle(ConvertOMLocationToLatLng(loc), Params.GetParam<GMarkerGoogleType>(param, 3, GMarkerGoogleType.arrow));
                                    }
                                    marker.Tag = markerName;
                                }
                                else
                                {
                                    marker.Position = ConvertOMLocationToLatLng(loc);
                                }
                                marker.ToolTipText = Params.GetParam<string>(param, 4, String.Empty);
                                if (!String.IsNullOrEmpty(marker.ToolTipText))
                                    marker.ToolTipMode = MarkerTooltipMode.Always;
                                else
                                    marker.ToolTipMode = MarkerTooltipMode.Never;

                                // Add marker to map
                                if (!markerOverlays[command.Screen].Markers.Contains(marker))
                                    markerOverlays[command.Screen].Markers.Add(marker);
                                _Map[command.Screen].Refresh();
                            }
                        }
                        break;

                    #endregion

                    #region Map Utilities

                    case "Map.Size.Set":
                        {
                            if (Params.IsParamsValid(param, 1))
                            {
                                _MapSize[command.Screen] = Params.GetParam<Size>(param, 0, _MapSize[command.Screen]);
                                _Map[command.Screen].Width = _MapSize[command.Screen].Width;
                                _Map[command.Screen].Height = _MapSize[command.Screen].Height;
                                OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Size", _MapSize);
                            }
                            if (Params.IsParamsValid(param, command.RequiredParamCount))
                            {
                                _MapSize[command.Screen].Width = Params.GetParam<int>(param, 0, _MapSize[command.Screen].Width);
                                _MapSize[command.Screen].Height = Params.GetParam<int>(param, 1, _MapSize[command.Screen].Height);
                                _Map[command.Screen].Width = _MapSize[command.Screen].Width;
                                _Map[command.Screen].Height = _MapSize[command.Screen].Height;
                                OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Size", _MapSize);
                            }
                        }
                        break;

                    case "Map.Refresh":
                        {
                            // Error handling
                            if (_Map == null)
                                return null;

                            // Update map settings
                            if (_SettingsChanged)
                            {
                                _SettingsChanged = false;
                                _Map[command.Screen].MapProvider = _Map_CurrentProvider;
                                _Map[command.Screen].Manager.Mode = _Map_AccessMode;

                                // Force a redraw of the map with the new data
                                _Map[command.Screen].Refresh();
                            }

                            // Update mapdata
                            _Map_Marker_HomePosition[command.Screen].Position = ConvertOMLocationToLatLng(BuiltInComponents.SystemSettings.Location_Home);
                            _Map_Marker_CurrentPosition[command.Screen].Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
                            if (_Map_FollowsCurrentLoc[command.Screen])
                                _Map[command.Screen].Refresh();
                        }
                        break;

                    #endregion

                    #region Zoom

                    case "Map.Zoom.Level":
                        {
                            double level;
                            if (param[0] is string)
                            {
                                if (double.TryParse(param[0] as string, out level))
                                    _Map[command.Screen].Zoom = level;
                            }
                            else
                            {
                                if (double.TryParse(param[0].ToString(), out level))
                                    _Map[command.Screen].Zoom = level;
                            }

                            OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Zoom.Level", (int)_Map[command.Screen].Zoom);
                        }
                        break;
                    case "Map.Zoom.In":
                        {
                            _Map[command.Screen].Zoom++;
                            OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Zoom.Level", (int)_Map[command.Screen].Zoom);
                        }
                        break;
                    case "Map.Zoom.Out":
                        {
                            _Map[command.Screen].Zoom--;
                            OM.Host.DataHandler.PushDataSourceValue(command.Screen, this, "Map.Zoom.Level", (int)_Map[command.Screen].Zoom);
                        }
                        break;

                    #endregion

                    #region Map goto

                    case "Map.Goto.Current":
                        {
                            _Map_FollowsCurrentLoc[command.Screen] = true;
                            _Map_Marker_CurrentPosition[command.Screen].Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
                            _Map[command.Screen].Position = _Map_Marker_CurrentPosition[command.Screen].Position;
                            _Map[command.Screen].Zoom = CalculateZoomLevel(command.Screen);
                        }
                        break;

                    case "Map.Goto.Location":
                        {
                            result = true;
                            string location;
                            if (param[0] is string)
                            {
                                location = param[0] as string;
                                Location loc;
                                if (Location.TryParseLatLng(location, out loc))
                                    location = ConvertOMLocationToLatLng(loc).ToGoogleLatLngString();
                            }
                            else if (param[0] is Location)
                            {
                                location = ConvertOMLocationToLatLng((Location)param[0]).ToGoogleLatLngString();
                            }
                            else
                            {   // Cancel command
                                return null;
                            }

                            // Run map command
                            return Map_GotoPositionFromKeyword(command.Screen, location);

                        }
                        break;

                    #endregion

                    #region Mouse movement

                    case "Map.Mouse.EmulateMouseDown":
                        {
                            if (Params.IsParamsValid(param, command.RequiredParamCount))
                            {
                                OpenMobile.Input.MouseButton btn = Params.GetParam<OpenMobile.Input.MouseButton>(param, 0, OpenMobile.Input.MouseButton.Left);
                                Point p = Params.GetParam<Point>(param, 1, Point.Empty);
                                _Map[command.Screen].EmulateMouseDown(_Map[command.Screen].DragButton, p.X, p.Y);
                            }
                        }
                        break;

                    case "Map.Mouse.EmulateMouseUp":
                        {
                            if (Params.IsParamsValid(param, command.RequiredParamCount))
                            {
                                OpenMobile.Input.MouseButton btn = Params.GetParam<OpenMobile.Input.MouseButton>(param, 0, OpenMobile.Input.MouseButton.Left);
                                Point p = Params.GetParam<Point>(param, 1, Point.Empty);
                                _Map[command.Screen].EmulateMouseUp(_Map[command.Screen].DragButton, p.X, p.Y);
                            }
                        }
                        break;

                    case "Map.Mouse.EmulateMouseMove":
                        {
                            if (Params.IsParamsValid(param, command.RequiredParamCount))
                            {
                                OpenMobile.Input.MouseButton btn = Params.GetParam<OpenMobile.Input.MouseButton>(param, 0, OpenMobile.Input.MouseButton.Left);
                                Point p = Params.GetParam<Point>(param, 1, Point.Empty);

                                if (btn == OpenMobile.Input.MouseButton.Left)
                                {   // Map is being dragged, disable current location following
                                    _Map_FollowsCurrentLoc[command.Screen] = false;
                                    _Map[command.Screen].EmulateMouseMove(_Map[command.Screen].DragButton, p.X, p.Y);
                                }
                                else
                                {
                                    _Map[command.Screen].EmulateMouseMove(System.Windows.Forms.MouseButtons.None, p.X, p.Y);
                                }

                            }
                        }
                        break;

                    case "Map.Mouse.EmulateMouseClick":
                        {
                            if (Params.IsParamsValid(param, command.RequiredParamCount))
                            {
                                OpenMobile.Input.MouseButton btn = Params.GetParam<OpenMobile.Input.MouseButton>(param, 0, OpenMobile.Input.MouseButton.Left);
                                Point p = Params.GetParam<Point>(param, 1, Point.Empty);

                                // Routing data
                                _Map_Point_RouteStart[command.Screen] = _Map_Point_RouteEnd[command.Screen];
                                _Map_Point_RouteEnd[command.Screen] = _Map[command.Screen].FromLocalToLatLng(p.X, p.Y);

                                // Initialize markers
                                Map_UpdateRouteMarkers(command.Screen);

                                _Map[command.Screen].EmulateMouseClick(System.Windows.Forms.MouseButtons.Left, p.X, p.Y);
                            }
                        }
                        break;

                    #endregion

                    #region Map route

                    case "Map.Route.ByMarkers":
                        {
                            return Map_Route(command.Screen, _Map_Point_RouteStart[command.Screen], _Map_Point_RouteEnd[command.Screen]);
                        }
                        break;

                    case "Map.Route":
                        {
                            result = true;

                            string startLocation = null;
                            string endLocation = null;

                            if (param.Length >= 2)
                            {
                                if (param[0] is string)
                                {
                                    startLocation = param[0] as string;
                                    Location loc;
                                    if (Location.TryParseLatLng(startLocation, out loc))
                                        startLocation = ConvertOMLocationToLatLng(loc).ToGoogleLatLngString();
                                }
                                else if (param[0] is Location)
                                {
                                    startLocation = ConvertOMLocationToLatLng((Location)param[0]).ToGoogleLatLngString();
                                }
                                else
                                {   // Use empty start as this indicates current location
                                    startLocation = null;
                                }

                                if (param[1] is string)
                                {
                                    endLocation = param[1] as string;
                                    Location loc;
                                    if (Location.TryParseLatLng(endLocation, out loc))
                                        endLocation = ConvertOMLocationToLatLng(loc).ToGoogleLatLngString();
                                }
                                else if (param[1] is Location)
                                {
                                    endLocation = ConvertOMLocationToLatLng((Location)param[1]).ToGoogleLatLngString();
                                }
                                else
                                {   // No info
                                    endLocation = null;
                                }
                            }
                            else if (param.Length == 1)
                            {
                                if (param[0] is string)
                                {
                                    endLocation = param[0] as string;
                                    Location loc;
                                    if (Location.TryParseLatLng(endLocation, out loc))
                                        endLocation = ConvertOMLocationToLatLng(loc).ToGoogleLatLngString();
                                }
                                else if (param[0] is Location)
                                {
                                    endLocation = ConvertOMLocationToLatLng((Location)param[0]).ToGoogleLatLngString();
                                }
                                else
                                {   // No info
                                    endLocation = null;
                                }
                            }

                            // Cancel command if end location is missing
                            if (String.IsNullOrEmpty(endLocation))
                                return null;

                            // Run map command
                            return Map_Route(command.Screen, startLocation, endLocation);
                        }
                        break;

                    #endregion

                    default:
                        break;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region Host events

        void Host_OnNavigationEvent(eNavigationEvent type, string arg)
        {
            switch (type)
            {
                case eNavigationEvent.Unknown:
                    break;
                case eNavigationEvent.GPSStatusChange:
                    break;
                case eNavigationEvent.RouteChanged:
                    break;
                case eNavigationEvent.TurnApproaching:
                    break;
                case eNavigationEvent.LocationChanged:
                    if (_Map != null)
                    {
                        OM.Host.ForEachScreen((screen) =>
                            {
                                _Map_Marker_CurrentPosition[screen].Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
                                if (_Map_FollowsCurrentLoc[screen])
                                {
                                    _Map[screen].Position = _Map_Marker_CurrentPosition[screen].Position;
                                    _Map[screen].Zoom = CalculateZoomLevel(screen);
                                }
                            });
                    }
                    break;
                default:
                    break;
            }
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.RenderingWindowResized)
            {
                if (_Map != null)
                {
                    int screen = int.Parse(args[0] as string);
                    // Ensure we scale the map when the window is resized so whe have "pixel perfect" map
                    PointF scaleFactors = (PointF)args[3];
                    if (_Map[screen] != null)
                    {
                        _Map[screen].Width = (int)(_MapSize[screen].Width * scaleFactors.X);
                        _Map[screen].Height = (int)(_MapSize[screen].Height * scaleFactors.Y);
                    }
                }
            }
        }

        #endregion

        #region Map
        object locker = 1;
        void Map_Initialize(int s, int mapWidth, int mapHeight)
        {
            //if (RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully)
            {
                lock (locker)
                {
                    int screen = s;
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Map initializing for screen {0} started", screen));

                    _Map[screen] = new GMapImageRender(mapWidth, mapHeight);
                    //_Map[screen].NegativeMode = true;
                    _Map[screen].Tag = screen;
                    _Map[screen].Position = new PointLatLng(OM.Host.CurrentLocation.Latitude, OM.Host.CurrentLocation.Longitude);
                    _Map[screen].MinZoom = 1;
                    _Map[screen].MaxZoom = 17;
                    //MainMap.Zoom = 1;
                    //MainMap.Position = new PointLatLng(0, 0);
                    _Map[screen].MapProvider = _Map_CurrentProvider;
                    _Map[screen].Manager.Mode = _Map_AccessMode;
                    _Map[screen].OnImageUpdated += new GMapImageUpdatedDelegate(Map_OnImageUpdated);
                    _Map[screen].OnImageAssigned += new GMapImageUpdatedDelegate(Map_OnImageAssigned);
                    _Map[screen].Overlays.Add(_Map_Overlay_Routes[screen]);
                    _Map[screen].Overlays.Add(_Map_Overlay_Polygons[screen]);
                    _Map[screen].Overlays.Add(_Map_Overlay_Objects[screen]);
                    _Map[screen].Overlays.Add(_Map_Overlay[screen]);
                    if (_Map_Overlay_Objects[screen].Markers.Count > 0)
                        _Map[screen].ZoomAndCenterMarkers(null);
                    //MainMap.EmptyMapBackground = System.Drawing.Color.Transparent;
                    //MainMap.EmptyTileColor = 
                    _Map[screen].EmptyTileText = "Missing images on this level";
                    _Map[screen].EmptyTileBorders.Color = BuiltInComponents.SystemSettings.SkinTextColor.ToSystemColor();
                    _Map[screen].OnTileLoadStart += new TileLoadStart(Map_OnTileLoadStart);
                    _Map[screen].OnTileLoadComplete += new TileLoadComplete(Map_OnTileLoadComplete);

                    // set current marker
                    _Map_Marker_CurrentPosition[screen] = new GMarkerGoogle(ConvertOMLocationToLatLng(OM.Host.CurrentLocation), GMarkerGoogleType.blue_small);
                    _Map_Marker_CurrentPosition[screen].IsHitTestVisible = false;
                    _Map_Overlay[screen].Markers.Add(_Map_Marker_CurrentPosition[screen]);

                    _Map_Marker_HomePosition[screen] = new GMarkerGoogle(ConvertOMLocationToLatLng(BuiltInComponents.SystemSettings.Location_Home), GMarkerGoogleType.green_pushpin);
                    //markerHomePosition.IsHitTestVisible = false;
                    _Map_Marker_HomePosition[screen].ToolTipText = "Home\r\nAs set in settings";
                    _Map_Marker_HomePosition[screen].ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    _Map_Overlay_Objects[screen].Markers.Add(_Map_Marker_HomePosition[screen]);

                    _Map[screen].Position = _Map_Marker_CurrentPosition[screen].Position;
                    _Map[screen].Zoom = CalculateZoomLevel(screen);

                    // Host events
                    OM.Host.OnNavigationEvent += new NavigationEvent(Host_OnNavigationEvent);

                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Map initializing for screen {0} completed", screen));
                }
            }
        }

        void Map_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            OM.Host.ForEachScreen((screen) =>
                {
                    OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", false);
                });
        }
        void Map_OnTileLoadStart()
        {
            OM.Host.ForEachScreen((screen) =>
                {
                    OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", true);
                });
        }

        void Map_OnImageAssigned(object sender, GMapImageUpdatedEventArgs e)
        {
            try
            {
                GMapImageRender map = sender as GMapImageRender;
                int screen = (int)map.Tag;
                _MapImage[screen] = new OImage(map.TargetImage);//.Invert();

                switch (_MapDrawMode)
                {
                    case MapDrawMode.Auto:
                        if (_MapNightMode)
                            _MapImage[screen].ShaderEffect = OMShaders.Negative;
                        break;
                    case MapDrawMode.Day:
                        _MapImage[screen].ShaderEffect = OMShaders.None;
                        break;
                    case MapDrawMode.Night:
                        _MapImage[screen].ShaderEffect = OMShaders.Negative;
                        break;
                    default:
                        break;
                }

                OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Image", _MapImage[screen]);
            }
            catch
            {   // Mask errors
            }
        }
        void Map_OnImageUpdated(object sender, GMapImageUpdatedEventArgs e)
        {
            GMapImageRender map = sender as GMapImageRender;
            int screen = (int)map.Tag;
            if (_MapImage[screen] == null)
                _MapImage[screen] = new OImage(map.TargetImage);//.Invert();

            switch (_MapDrawMode)
            {
                case MapDrawMode.Auto:
                    if (_MapNightMode)
                        _MapImage[screen].ShaderEffect = OMShaders.Negative;
                    break;
                case MapDrawMode.Day:
                    _MapImage[screen].ShaderEffect = OMShaders.None;
                    break;
                case MapDrawMode.Night:
                    _MapImage[screen].ShaderEffect = OMShaders.Negative;
                    break;
                default:
                    break;
            }

            _MapImage[screen].Refresh();
            OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Image", _MapImage[screen]);
        }

        #endregion

        #region private helpers

        string Map_GotoPositionFromKeyword(int screen, string location)
        {
            GeoCoderStatusCode status = _Map[screen].SetPositionByKeywords(location);
            if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
            {
                return status.ToString();
            }
            else
                _Map[screen].Zoom = CalculateZoomLevel(screen);

            return String.Empty;
        }

        private PointLatLng ConvertOMLocationToLatLng(Location location)
        {
            return new PointLatLng(location.Latitude, location.Longitude);
        }

        /// <summary>
        /// This method is supposed to adjust zoom level based on speed
        /// </summary>
        /// <returns></returns>
        private int CalculateZoomLevel(int screen)
        {   // TODO
            //return 15;
            if (_Map[screen].Zoom <= 0)
                _Map[screen].Zoom = 15;

            int speed = OM.Host.DataHandler.GetDataSourceValue<int>("GPS.Speed.KMH");
            if (speed > 5)
            {
                if (speed <= 50)
                    _Map[screen].Zoom = 17;
                else if (speed > 50 && speed <= 80)
                    _Map[screen].Zoom = 16;
                else
                    _Map[screen].Zoom = 15;
            }

            OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Zoom.Level", (int)_Map[screen].Zoom);

            return (int)_Map[screen].Zoom;
        }

        private GeoCoderStatusCode GetPositionByKeywords(string keys, out PointLatLng position)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            position = PointLatLng.Empty;

            if (_Map != null)
            {
                GeocodingProvider gp = GMapProviders.GoogleMap as GeocodingProvider;
                if (gp != null)
                {
                    var pt = gp.GetPoint(keys, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && pt.HasValue)
                    {
                        position = pt.Value;
                    }
                }
            }

            return status;
        }

        private GeoCoderStatusCode GetPlacemark(PointLatLng position, out Placemark placemark)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            placemark = new Placemark();

            if (_Map != null)
            {
                GeocodingProvider gp = GMapProviders.GoogleMap as GeocodingProvider;
                if (gp != null)
                {
                    Placemark? p = gp.GetPlacemark(position, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS)
                    {
                        placemark = p.Value;
                    }
                }
            }
            return status;
        }

        #endregion

        #region Route

        private string Map_Route(int screen, object routeStart, object routeEnd)
        {
            // Use current location as start if start info is missing
            if (routeStart is string)
            {
                if (String.IsNullOrEmpty(routeStart as string))
                    routeStart = ConvertOMLocationToLatLng(OM.Host.CurrentLocation).ToGoogleLatLngString();
            }
            else if (routeStart == null)
                routeStart = ConvertOMLocationToLatLng(OM.Host.CurrentLocation).ToGoogleLatLngString();

            // Set map as busy
            OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", true);

            Map_ClearRoute(screen);

            RoutingProvider rp = GMapProviders.GoogleMap as RoutingProvider;
            MapRoute route = null;
            if (routeStart is GMap.NET.PointLatLng)
                route = rp.GetRoute((PointLatLng)routeStart, (PointLatLng)routeEnd, false, false, (int)_Map[screen].Zoom);
            else if (routeStart is string)
                route = rp.GetRoute((string)routeStart, (string)routeEnd, false, false, (int)_Map[screen].Zoom);
            if (route != null)
            {
                // Get start point info
                //GeoCoderStatusCode geoCodeStatus;
                //var routeStartInfo = GMapProviders.GoogleMap.GetPlacemark(routeStart, out geoCodeStatus);
                //var routeEndInfo = GMapProviders.GoogleMap.GetPlacemark(routeEnd, out geoCodeStatus);

                // add route
                GMapRoute r = new GMapRoute(route.Points, route.Name);
                GDirections s = null;
                DirectionsStatusCode directionResult = DirectionsStatusCode.UNKNOWN_ERROR;
                directionResult = GMapProviders.GoogleMap.GetDirections(out s, route.From.Value.ToGoogleLatLngString(), route.To.Value.ToGoogleLatLngString(), false, false, false, false, true);
                //if (routeStart is PointLatLng)
                //    directionResult = GMapProviders.GoogleMap.GetDirections(out s, (PointLatLng)routeStart, (PointLatLng)routeEnd, false, false, false, false, true);
                //else if (routeStart is string)
                //    directionResult = GMapProviders.GoogleMap.GetDirections(out s, (string)routeStart, (string)routeEnd, false, false, false, false, true);
                if (directionResult == DirectionsStatusCode.OK)
                {
                    _Map_ActiveNavigationRoute[screen] = new NavigationRoute(r, s);

                    _Map_Point_RouteStart[screen] = s.StartLocation;
                    _Map_Point_RouteEnd[screen] = s.EndLocation;

                    Map_AddRoute(screen, r);
                    Map_UpdateRouteMarkers(screen);

                    // Push route object update
                    OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.RouteData", _Map_ActiveNavigationRoute[screen]);

                    _Map[screen].ZoomAndCenterRoute(r);
                }
                else
                {
                    OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", false);
                    return directionResult.ToString();
                }
            }
            else
            {
                OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", false);
                return "Uknown error";
            }

            OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.Busy", false);
            return String.Empty;  // Empty string indicates all OK
        }

        private void Map_ClearRoute(int screen)
        {
            _Map_Overlay_Routes[screen].Routes.Clear();
            _Map_ActiveNavigationRoute[screen] = null;
            OM.Host.DataHandler.PushDataSourceValue(screen, this, "Map.RouteData", _Map_ActiveNavigationRoute[screen], true);
        }
        private void Map_AddRoute(int screen, GMapRoute route)
        {
            _Map_Overlay_Routes[screen].Routes.Add(route);
        }

        private void Map_UpdateRouteMarkers(int screen)
        {
            // Initialize markers
            if (_Map_Marker_RouteStart[screen] == null)
                _Map_Marker_RouteStart[screen] = new GMarkerGoogle(_Map_Point_RouteStart[screen], GMarkerGoogleType.green_big_go);
            if (_Map_Marker_RouteEnd[screen] == null)
                _Map_Marker_RouteEnd[screen] = new GMarkerGoogle(_Map_Point_RouteEnd[screen], GMarkerGoogleType.red_big_stop);

            // Add to map
            if (!_Map_Point_RouteStart[screen].IsEmpty)
            {
                _Map_Marker_RouteStart[screen].Position = _Map_Point_RouteStart[screen];
                if (!_Map_Overlay_Objects[screen].Markers.Contains(_Map_Marker_RouteStart[screen]))
                    _Map_Overlay_Objects[screen].Markers.Add(_Map_Marker_RouteStart[screen]);
            }
            if (!_Map_Point_RouteEnd[screen].IsEmpty)
            {
                _Map_Marker_RouteEnd[screen].Position = _Map_Point_RouteEnd[screen];
                if (!_Map_Overlay_Objects[screen].Markers.Contains(_Map_Marker_RouteEnd[screen]))
                    _Map_Overlay_Objects[screen].Markers.Add(_Map_Marker_RouteEnd[screen]);
            }
        }

        #endregion
    }
}
