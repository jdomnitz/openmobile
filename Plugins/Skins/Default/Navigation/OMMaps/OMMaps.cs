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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using GMap.NET.ImageRender;
using System.Collections.Generic;
using OpenMobile.helperFunctions;
using System.Text;

namespace OMSkinPluginSample
{
    public sealed class OMSkinPluginSample : HighLevelCode
    {
        #region Private variables

        private GMapImageRender _Map;
        private GMapOverlay _Map_Overlay = new GMapOverlay();
        private GMapOverlay _Map_Overlay_Objects = new GMapOverlay("objects");
        private GMapOverlay _Map_Overlay_Routes = new GMapOverlay("routes");
        private GMapOverlay _Map_Overlay_Polygons = new GMapOverlay("polygons");
        private PointLatLng _Map_Point_RouteStart = new PointLatLng();
        private PointLatLng _Map_Point_RouteEnd = new PointLatLng();
        private GMarkerGoogle _Map_Marker_CurrentPosition;
        private GMarkerGoogle _Map_Marker_HomePosition;
        private GMapMarker _Map_Marker_RouteStart;
        private GMapMarker _Map_Marker_RouteEnd;
        private bool _Map_FollowsCurrentLoc = true;
        private List<GMapProvider> _Map_AvailableProviders;
        private GMapProvider _Map_CurrentProvider;
        private AccessMode _Map_AccessMode;
        private bool _SettingsChanged = false;
        private NavigationRoute _Map_ActiveNavigationRoute;

        #endregion

        #region Skin Init and Dispose

        public OMSkinPluginSample()
            : base("OMMaps", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "A map provider for OpenMobile", "OMMaps", "OM Dev team", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Settings
            Settings_SetDefaultValues();
            Settings_MapVariables();

            // Queue panels
            base.PanelManager.QueuePanel("OMMaps", InitializeMapPanel, true);

            OM.Host.OnNavigationEvent += new NavigationEvent(Host_OnNavigationEvent);

            // Return
            return eLoadStatus.LoadSuccessful;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region Settings

        public override void Settings()
        {            
            base.MySettings.Add(Setting.TextList<GMapProvider>("OMMaps.MapProvider", "Select provider", "Available map providers", StoredData.Get(this, "OMMaps.MapProvider"), _Map_AvailableProviders));
            base.MySettings.Add(Setting.EnumSetting<AccessMode>("OMMaps.MapMode", "Map mode", "NB! Server requires internet connection", StoredData.Get(this, "OMMaps.MapMode")));
        }

        private void Settings_MapVariables()
        {
            if (_Map_AvailableProviders == null)
                _Map_AvailableProviders = GMapProviders.List;

            _Map_CurrentProvider = _Map_AvailableProviders.Find(x => x.Name == StoredData.Get(this, "OMMaps.MapProvider"));
            _Map_AccessMode = StoredData.GetEnum<AccessMode>(this, "OMMaps.MapMode", AccessMode.ServerAndCache);
        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, "OMMaps.MapProvider", GMapProviders.OpenStreetMap);
            StoredData.SetDefaultValue(this, "OMMaps.MapMode", AccessMode.ServerAndCache);
            Settings_MapVariables();
        }

        public override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);

            // Update local variables
            Settings_MapVariables();

            // Inform skin that settings was changed
            _SettingsChanged = true;
        }

        #endregion

        #region Map panel

        private OMPanel InitializeMapPanel()
        {
            OMPanel panel = new OMPanel("OMMaps", "Map", OM.Host.getSkinImage("Icons|Icon-GPS"));

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(255, Color.Black)));
            panel.addControl(shapeBackground);

            OMBasicShape shp_InfoGroup_Header = new OMBasicShape("shp_InfoGroup_Header", shapeBackground.Region.Left + (shapeBackground.Region.Width / 2), shapeBackground.Region.Top + 30, shapeBackground.Region.Width / 2, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Empty))
            });
            panel.addControl(shp_InfoGroup_Header);

            OMLabel lbl_InfoGroup_Header = new OMLabel("lbl_InfoGroup_Header", shp_InfoGroup_Header.Region.Left + 10, shp_InfoGroup_Header.Region.Top - 23, shp_InfoGroup_Header.Region.Width - 20, 25);
            lbl_InfoGroup_Header.Text = "Route summary";
            panel.addControl(lbl_InfoGroup_Header);

            OMContainer container_InfoGroup_Text = new OMContainer("container_InfoGroup_Text", shp_InfoGroup_Header.Region.Left + 10, shp_InfoGroup_Header.Region.Bottom + 10, shp_InfoGroup_Header.Region.Width - 20, shapeBackground.Region.Height - 47 - 70);
            panel.addControl(container_InfoGroup_Text);

            //OMLabel lbl_InfoGroup_Text = new OMLabel("lbl_InfoGroup_Text", shp_InfoGroup_Header.Region.Left + 10, shp_InfoGroup_Header.Region.Bottom + 10, shp_InfoGroup_Header.Region.Width - 20, shapeBackground.Region.Height - 47);
            OMLabel lbl_InfoGroup_Text = new OMLabel("lbl_InfoGroup_Text", 0, 0, 100, 100);
            lbl_InfoGroup_Text.Text = "Route info...";
            lbl_InfoGroup_Text.FontSize = 12;
            lbl_InfoGroup_Text.TextAlignment = Alignment.TopLeft;
            lbl_InfoGroup_Text.AutoSizeMode = ControlSizeMode.GrowBoth;
            container_InfoGroup_Text.addControl(lbl_InfoGroup_Text);

            OMButton btn_InfoGroup_Hide = OMButton.PreConfigLayout_BasicStyle("btn_InfoGroup_Hide", container_InfoGroup_Text.Region.Left, container_InfoGroup_Text.Region.Bottom, container_InfoGroup_Text.Region.Width, 60, GraphicCorners.All, "", "Hide info");
            btn_InfoGroup_Hide.OnClick += new userInteraction(btn_InfoGroup_Hide_OnClick);
            panel.addControl(btn_InfoGroup_Hide);

            OMImage img_MapGroup_Map = new OMImage("img_MapGroup_Map", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height);
            img_MapGroup_Map.OnResize += new userInteraction(imgMap_OnResize);
            panel.addControl(img_MapGroup_Map);

            OMMouseHandler mouse_MapGroup_MapHandler = new OMMouseHandler("mouse_MapGroup_MapHandler", img_MapGroup_Map);
            mouse_MapGroup_MapHandler.OnMouseDown += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseDown);
            mouse_MapGroup_MapHandler.OnMouseMove += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseMove);
            mouse_MapGroup_MapHandler.OnMouseUp += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseUp);
            mouse_MapGroup_MapHandler.OnClick += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnClick);
            panel.addControl(mouse_MapGroup_MapHandler);

            OMButton btn_MapGroup_ZoomIn = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_ZoomIn", img_MapGroup_Map.Region.Left + 10, img_MapGroup_Map.Region.Top + 10, 60, 60, null, null, GraphicCorners.All, "", null, "+", null, null, null, "Arial", 75, BuiltInComponents.SystemSettings.SkinTextColor);
            btn_MapGroup_ZoomIn.Opacity = 100;
            btn_MapGroup_ZoomIn.OnClick += new userInteraction(btnZoomIn_OnClick);
            panel.addControl(btn_MapGroup_ZoomIn);

            OMButton btn_MapGroup_ZoomOut = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_ZoomOut", img_MapGroup_Map.Region.Right - 70, img_MapGroup_Map.Region.Top + 10, 60, 60, null, null, GraphicCorners.All, "", null, "-", null, null, null, "Arial", 75, BuiltInComponents.SystemSettings.SkinTextColor);
            btn_MapGroup_ZoomOut.Opacity = 100;
            btn_MapGroup_ZoomOut.OnClick += new userInteraction(btnZoomOut_OnClick);
            panel.addControl(btn_MapGroup_ZoomOut);

            OMButton btn_MapGroup_CurrentLoc = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_CurrentLoc", img_MapGroup_Map.Region.Left + 10, img_MapGroup_Map.Region.Bottom - 70, 60, 60, null, null, GraphicCorners.All, "", OM.Host.getSkinImage("AIcons|10-device-access-location-found").image.Copy().Overlay(BuiltInComponents.SystemSettings.SkinTextColor), "", null, null, null);
            btn_MapGroup_CurrentLoc.Opacity = 100;
            btn_MapGroup_CurrentLoc.OnClick += new userInteraction(btn_MapGroup_CurrentLoc_OnClick);
            panel.addControl(btn_MapGroup_CurrentLoc);

            OMImage img_MapGroup_Updating = new OMImage("img_MapGroup_Updating", OM.Host.ClientArea_Init.Left + (OM.Host.ClientArea_Init.Width / 2), OM.Host.ClientArea_Init.Top + (OM.Host.ClientArea_Init.Height / 2), OM.Host.getSkinImage("BusyAnimationTransparent.gif"));
            img_MapGroup_Updating.Left -= img_MapGroup_Updating.Image.image.Width / 2;
            img_MapGroup_Updating.Top -= img_MapGroup_Updating.Image.image.Height / 2;
            img_MapGroup_Updating.Visible = false;
            panel.addControl(img_MapGroup_Updating);

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(pluginName, panel.Name, "OMMaps_PopUpMenuStrip");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShowSettings", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Settings", true, cmdOnClick: base.GetCmdString_GotoMySettingsPanel()));
            //PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Render", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Render", true, OnClick: btnTest_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Goto", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Goto", true, OnClick: mnuItem_Goto_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Route", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Route", true, OnClick: mnuItem_Route_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_RouteByMapMarkers", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Route from map", true, OnClick: mnuItem_RouteByMapMarkers_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShowRouteInfo", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Show route info", true, OnClick: mnuItem_ShowRouteInfo_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_CurrentLocation", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Current location", true, OnClick: mnuItem_CurrentLocation_OnClick));
            panel.PopUpMenu = PopUpMenuStrip;

            OpenMobile.Threading.MessagePump.CreateMessagePump(this.pluginName, () =>
            {
                Map_Initialize(img_MapGroup_Map.Width, img_MapGroup_Map.Height);
            });

            panel.Entering += new PanelEvent(panel_Entering);

            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            return panel;
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Error handling
            if (_Map == null)
                return;

            // Update map settings
            if (_SettingsChanged)
            {
                _SettingsChanged = false;
                _Map.MapProvider = _Map_CurrentProvider;
                _Map.Manager.Mode = _Map_AccessMode;

                // Force a redraw of the map with the new data
                _Map.Refresh();
            }

            // Update mapdata
            _Map_Marker_HomePosition.Position = ConvertOMLocationToLatLng(BuiltInComponents.SystemSettings.Location_Home);
            _Map_Marker_CurrentPosition.Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
            if (_Map_FollowsCurrentLoc)
                _Map.Refresh();
        }

        void btn_InfoGroup_Hide_OnClick(OMControl sender, int screen)
        {
            GUI_RoutePanel(screen, false);
        }

        void btn_MapGroup_CurrentLoc_OnClick(OMControl sender, int screen)
        {
            _Map_FollowsCurrentLoc = true;
            _Map_Marker_CurrentPosition.Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
            _Map.Position = _Map_Marker_CurrentPosition.Position;
            _Map.Zoom = CalculateZoomLevel();
        }

        void btnZoomIn_OnClick(OMControl sender, int screen)
        {
            _Map.Zoom++;
        }
        void btnZoomOut_OnClick(OMControl sender, int screen)
        {
            _Map.Zoom--;
        }

        void btnTest_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            _Map.Refresh();
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
                        if (_Map_FollowsCurrentLoc)
                        {
                            _Map_Marker_CurrentPosition.Position = ConvertOMLocationToLatLng(OM.Host.CurrentLocation);
                            _Map.Position = _Map_Marker_CurrentPosition.Position;
                            _Map.Zoom = CalculateZoomLevel();
                        }
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
                // Ensure we scale the map when the window is resized so whe have "pixel perfect" map
                PointF scaleFactors = (PointF)args[3];
                OM.Host.ForEachScreen((screen) =>
                    {
                        OMPanel panel = base.PanelManager[screen, "OMMaps"];
                        if (panel != null)
                        {
                            OMImage imgMap = panel["img_MapGroup_Map"] as OMImage;
                            if (imgMap != null)
                            {
                                _Map.Width = (int)(imgMap.Width * scaleFactors.X);
                                _Map.Height = (int)(imgMap.Height * scaleFactors.Y);
                            }
                        }
                    });
            }
        }

        #endregion

        #region Map

        void Map_Initialize(int mapWidth, int mapHeight)
        {
            _Map = new GMapImageRender(mapWidth, mapHeight);
            _Map.Position = new PointLatLng(OM.Host.CurrentLocation.Latitude, OM.Host.CurrentLocation.Longitude);
            _Map.MinZoom = 1;
            _Map.MaxZoom = 17;
            //MainMap.Zoom = 1;
            //MainMap.Position = new PointLatLng(0, 0);
            _Map.MapProvider = _Map_CurrentProvider;
            _Map.Manager.Mode = _Map_AccessMode;
            _Map.OnImageUpdated += new GMapImageUpdatedDelegate(Map_OnImageUpdated);
            _Map.OnImageAssigned += new GMapImageUpdatedDelegate(Map_OnImageAssigned);
            _Map.Overlays.Add(_Map_Overlay_Routes);
            _Map.Overlays.Add(_Map_Overlay_Polygons);
            _Map.Overlays.Add(_Map_Overlay_Objects);
            _Map.Overlays.Add(_Map_Overlay);
            if (_Map_Overlay_Objects.Markers.Count > 0)
                _Map.ZoomAndCenterMarkers(null);
            //MainMap.EmptyMapBackground = System.Drawing.Color.Transparent;
            //MainMap.EmptyTileColor = 
            _Map.EmptyTileText = "Missing images on this level";
            _Map.EmptyTileBorders.Color = BuiltInComponents.SystemSettings.SkinTextColor.ToSystemColor();
            _Map.OnTileLoadStart += new TileLoadStart(Map_OnTileLoadStart);
            _Map.OnTileLoadComplete += new TileLoadComplete(Map_OnTileLoadComplete);

            // set current marker
            _Map_Marker_CurrentPosition = new GMarkerGoogle(ConvertOMLocationToLatLng(OM.Host.CurrentLocation), GMarkerGoogleType.blue_small);
            _Map_Marker_CurrentPosition.IsHitTestVisible = false;
            _Map_Overlay.Markers.Add(_Map_Marker_CurrentPosition);

            _Map_Marker_HomePosition = new GMarkerGoogle(ConvertOMLocationToLatLng(BuiltInComponents.SystemSettings.Location_Home), GMarkerGoogleType.green_pushpin);
            //markerHomePosition.IsHitTestVisible = false;
            _Map_Marker_HomePosition.ToolTipText = "Home\r\nAs set in settings";
            _Map_Marker_HomePosition.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            _Map_Overlay_Objects.Markers.Add(_Map_Marker_HomePosition);

            _Map.Position = _Map_Marker_CurrentPosition.Position;
            _Map.Zoom = CalculateZoomLevel();
        }

        void Map_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            OM.Host.ForEachScreen((screen) =>
            {
                Map_GUI_RefreshIcon(screen, false);
            });
        }

        void Map_OnTileLoadStart()
        {
            OM.Host.ForEachScreen((screen) =>
            {
                Map_GUI_RefreshIcon(screen, true);
            });
        }

        void Map_GUI_RefreshIcon(int screen, bool show)
        {
            OMImage imgMap = base.PanelManager[screen, "OMMaps"]["img_MapGroup_Updating"] as OMImage;
            if (imgMap != null)
                imgMap.Visible = show;
        }

        void Map_OnImageAssigned(object sender, GMapImageUpdatedEventArgs e)
        {
            OM.Host.ForEachScreen((screen) =>
            {
                OMImage imgMap = base.PanelManager[screen, "OMMaps"]["img_MapGroup_Map"] as OMImage;
                if (imgMap != null)
                    imgMap.Image = new imageItem(new OImage(_Map.TargetImage));
            });
        }
        void Map_OnImageUpdated(object sender, GMapImageUpdatedEventArgs e)
        {
            OM.Host.ForEachScreen((screen) =>
            {
                OMImage imgMap = base.PanelManager[screen, "OMMaps"]["img_MapGroup_Map"] as OMImage;
                if (imgMap != null)
                {
                    if (imgMap.Image == imageItem.NONE)
                        imgMap.Image = new imageItem(new OImage(_Map.TargetImage));
                    imgMap.Image.Refresh();
                }
            });
        }

        void mouseMapHandler_OnClick(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            _Map_Point_RouteStart = _Map_Point_RouteEnd;
            _Map_Point_RouteEnd = _Map.FromLocalToLatLng(e.MouseLocalPosition.X, e.MouseLocalPosition.Y);

            // Initialize markers
            Map_UpdateRouteMarkers();

            _Map.EmulateMouseClick(System.Windows.Forms.MouseButtons.Left, e.MouseLocalPosition.X, e.MouseLocalPosition.Y);
        }
        void mouseMapHandler_OnMouseUp(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            _Map.EmulateMouseUp(_Map.DragButton, e.MouseLocalPosition.X, e.MouseLocalPosition.Y);
        }
        void mouseMapHandler_OnMouseMove(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            if (e.Button == OpenMobile.Input.MouseButton.Left)
            {   // Map is being dragged, disable current location following
                _Map_FollowsCurrentLoc = false;
            }
            _Map.EmulateMouseMove(_Map.DragButton, e.MouseLocalPosition.X, e.MouseLocalPosition.Y);
        }
        void mouseMapHandler_OnMouseDown(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            _Map.EmulateMouseDown(_Map.DragButton, e.MouseLocalPosition.X, e.MouseLocalPosition.Y);
        }

        void imgMap_OnResize(OMControl sender, int screen)
        {
            // Ensure mouse handler follows the map size
            OMMouseHandler mouseMapHandler = sender.Parent[screen, "mouse_MapGroup_MapHandler"] as OMMouseHandler;
            if (mouseMapHandler != null)
            {
                mouseMapHandler.Region = sender.Region;
            }
        }

        #endregion

        #region Menu items

        void mnuItem_Goto_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            string location = OSK.ShowDefaultOSK(screen, "", "location keyword", "Enter location/adress to go to", OSKInputTypes.Keypad, false);

            GeoCoderStatusCode status = _Map.SetPositionByKeywords(location);
            if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this.pluginName, sender.Parent.Name);
                dialog.Header = "Unknown location";
                dialog.Text = String.Format("{0} can't find: '{1}'\r\nreason: {2}", this.pluginName, location, status);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
            else
                _Map.Zoom = CalculateZoomLevel();
        }
        
        void mnuItem_Route_OnClick(OMControl sender, int screen)
        {
            //Route_ShowRoute(sender.Parent, screen, ConvertOMLocationToLatLng(OM.Host.CurrentLocation).ToGoogleLatLngString(), "Nedre torvgate 1 gjøvik");

            string startLocation = OSK.ShowDefaultOSK(screen, "", "Route start location (leave empty to use current)", "Enter start location or leave empty to use current", OSKInputTypes.Keypad, false);
            string endLocation = OSK.ShowDefaultOSK(screen, "", "Route end location", "Enter end location or adress", OSKInputTypes.Keypad, false);

            if (String.IsNullOrEmpty(startLocation))
                startLocation = ConvertOMLocationToLatLng(OM.Host.CurrentLocation).ToGoogleLatLngString();
            if (String.IsNullOrEmpty(endLocation))
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, sender.Parent);
                dialog.Header = "Invalid input";
                dialog.Text = "Route end location can't be nothing!\r\nTry again";
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
                return;
            }
            //GeoCoderStatusCode geoCodeResult = GetPositionByKeywords(endLocation, out _Map_Point_RouteEnd);
            //if (geoCodeResult != GeoCoderStatusCode.G_GEO_SUCCESS)
            //{
            //    OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, sender.Parent);
            //    dialog.Header = "Unable to get route end position data";
            //    dialog.Text = String.Format("{0} can't get position for: '{1}'\r\nreason: {2}", this.pluginName, endLocation, geoCodeResult);
            //    dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
            //    dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
            //    dialog.ShowMsgBox(screen);
            //    return;
            //}

            Route_ShowRoute(sender.Parent, screen, startLocation, endLocation);
        }

        void mnuItem_ShowRouteInfo_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            GUI_RoutePanel(screen, true);
        }

        void mnuItem_RouteByMapMarkers_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            Route_ShowRoute(sender.Parent, screen, _Map_Point_RouteStart, _Map_Point_RouteEnd);
        }

        void mnuItem_CurrentLocation_OnClick(OMControl sender, int screen)
        {
            _Map.Position = _Map_Marker_CurrentPosition.Position;
            _Map.Zoom = CalculateZoomLevel();
        }

        #endregion

        #region private helpers

        private PointLatLng ConvertOMLocationToLatLng(Location location)
        {
            return new PointLatLng(location.Latitude, location.Longitude);
        }

        /// <summary>
        /// This method is supposed to adjust zoom level based on speed
        /// </summary>
        /// <returns></returns>
        private int CalculateZoomLevel()
        {   // TODO
            return 15;
        }

        private GeoCoderStatusCode GetPositionByKeywords(string keys, out PointLatLng position)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            position = PointLatLng.Empty;

            GeocodingProvider gp = _Map.MapProvider as GeocodingProvider;
            if (gp == null)
                gp = GMapProviders.OpenStreetMap as GeocodingProvider;

            if (gp != null)
            {
                var pt = gp.GetPoint(keys, out status);
                if (status == GeoCoderStatusCode.G_GEO_SUCCESS && pt.HasValue)
                {
                    position = pt.Value;
                }
            }

            return status;
        }

        #endregion

        #region Route

        private void Route_ShowRoute(OMPanel panel, int screen, object routeStart, object routeEnd)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            Map_GUI_RefreshIcon(screen, true);
            Map_ClearRoute();

            RoutingProvider rp = GMapProviders.GoogleMap as RoutingProvider;
            MapRoute route = null;
            if (routeStart is PointLatLng)
                route = rp.GetRoute((PointLatLng)routeStart, (PointLatLng)routeEnd, false, false, (int)_Map.Zoom);
            else if (routeStart is string)
                route = rp.GetRoute((string)routeStart, (string)routeEnd, false, false, (int)_Map.Zoom);
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
                if (routeStart is PointLatLng)
                    directionResult = GMapProviders.GoogleMap.GetDirections(out s, (PointLatLng)routeStart, (PointLatLng)routeEnd, false, false, false, false, true);
                else if (routeStart is string)
                    directionResult = GMapProviders.GoogleMap.GetDirections(out s, (string)routeStart, (string)routeEnd, false, false, false, false, true);
                if (directionResult == DirectionsStatusCode.OK)
                {
                    _Map_ActiveNavigationRoute = new NavigationRoute(r, s);

                    Map_AddRoute(r);
                    Map_UpdateRouteMarkers();

                    GUI_RoutePanel(screen, true, header: "Route summary", text: _Map_ActiveNavigationRoute.RouteText);
                    _Map.ZoomAndCenterRoute(r);
                }
                else
                {
                    Map_GUI_RefreshIcon(screen, false);
                    OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, panel);
                    dialog.Header = "Unable to get directions";
                    dialog.Text = String.Format("{0} is unable to get directions!\r\nReason: {1}", this.pluginName, directionResult);
                    dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                    dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                    dialog.ShowMsgBox(screen);
                }
            }
            else
            {
                Map_GUI_RefreshIcon(screen, false);
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, panel);
                dialog.Header = "Unable to get route";
                dialog.Text = String.Format("{0} is unable to get route!\r\nNo additional info is available", this.pluginName);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
            Map_GUI_RefreshIcon(screen, false);
        }

        private void Map_ClearRoute()
        {
            _Map_Overlay_Routes.Routes.Clear();
        }
        private void Map_AddRoute(GMapRoute route)
        {
            _Map_Overlay_Routes.Routes.Add(route);
        }

        private void Map_UpdateRouteMarkers()
        {
            // Initialize markers
            if (_Map_Marker_RouteStart == null)
                _Map_Marker_RouteStart = new GMarkerGoogle(_Map_Point_RouteStart, GMarkerGoogleType.green_big_go);
            if (_Map_Marker_RouteEnd == null)
                _Map_Marker_RouteEnd = new GMarkerGoogle(_Map_Point_RouteEnd, GMarkerGoogleType.red_big_stop);

            // Add to map
            if (!_Map_Point_RouteStart.IsEmpty)
            {
                _Map_Marker_RouteStart.Position = _Map_Point_RouteStart;
                if (!_Map_Overlay_Objects.Markers.Contains(_Map_Marker_RouteStart))
                    _Map_Overlay_Objects.Markers.Add(_Map_Marker_RouteStart);
            }
            if (!_Map_Point_RouteEnd.IsEmpty)
            {
                _Map_Marker_RouteEnd.Position = _Map_Point_RouteEnd;
                if (!_Map_Overlay_Objects.Markers.Contains(_Map_Marker_RouteEnd))
                    _Map_Overlay_Objects.Markers.Add(_Map_Marker_RouteEnd);
            }
        }

        private void GUI_RoutePanel(int screen, bool show, bool fast = false, string header = null, string text = null)
        {
            OMPanel panel = base.PanelManager[screen, "OMMaps"];

            OMLabel lblHeader = panel["lbl_InfoGroup_Header"] as OMLabel;
            if (lblHeader != null && header != null)
                lblHeader.Text = header;

            OMContainer container = panel["container_InfoGroup_Text"] as OMContainer;
            OMLabel lblText = container["lbl_InfoGroup_Text"]["lbl_InfoGroup_Text"] as OMLabel;
            if (lblText != null && text != null)
                lblText.Text = text;

            OMImage imgMap = panel["img_MapGroup_Map"] as OMImage;
            if (imgMap != null)
            {
                OMButton btn_MapGroup_ZoomOut = panel[screen, "btn_MapGroup_ZoomOut"] as OMButton;
                OMBasicShape shp_InfoGroup_Header = panel[screen, "shp_InfoGroup_Header"] as OMBasicShape;

                SmoothAnimator animator = new SmoothAnimator(1.5f);
                if (show)
                {
                    #region Animation -> Show

                    int targetWidth = imgMap.Region.Width - (imgMap.Region.Right - shp_InfoGroup_Header.Region.Left);
                    animator.Animate((int AnimationStep, float AnimationStepF, double AnimationDurationMS) =>
                    {
                        bool animationComplete = true;
                        if (imgMap.Width > targetWidth)
                        {
                            imgMap.Width -= AnimationStep;
                            animationComplete = false;
                        }
                        else
                        {
                            imgMap.Width = targetWidth;
                        }
                        if (btn_MapGroup_ZoomOut.Region.Right > (imgMap.Region.Right - 10))
                        {
                            btn_MapGroup_ZoomOut.Left -= AnimationStep;
                            animationComplete = false;
                        }
                        else
                        {
                            btn_MapGroup_ZoomOut.Left = (imgMap.Region.Right - 10 - btn_MapGroup_ZoomOut.Width);
                        }
                        return !animationComplete;
                    });
                    _Map.Width = imgMap.Region.Width;
                    _Map.Height = imgMap.Region.Height;

                    #endregion
                }
                else
                {
                    #region Animation -> Hide

                    int targetWidth = OM.Host.ClientArea[screen].Width;
                    animator.Animate((int AnimationStep, float AnimationStepF, double AnimationDurationMS) =>
                    {
                        bool animationComplete = true;
                        if (imgMap.Width < targetWidth)
                        {
                            imgMap.Width += AnimationStep;
                            animationComplete = false;
                        }
                        else
                        {
                            imgMap.Width = targetWidth;
                        }
                        if (btn_MapGroup_ZoomOut.Region.Right < (imgMap.Region.Right - 10))
                        {
                            btn_MapGroup_ZoomOut.Left += AnimationStep;
                            animationComplete = false;
                        }
                        else
                        {
                            btn_MapGroup_ZoomOut.Left = (imgMap.Region.Right - 10 - btn_MapGroup_ZoomOut.Width);
                        }
                        return !animationComplete;
                    });
                    _Map.Width = imgMap.Region.Width;
                    _Map.Height = imgMap.Region.Height;

                    #endregion
                }

                _Map.Refresh();
            }
        }

        #endregion
    }
}
