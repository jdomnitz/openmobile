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

namespace OMMaps2
{
    public sealed class OMMaps : HighLevelCode
    {
        #region Private variables

        private NavigationRoute[] _Map_ActiveNavigationRoute;

        #endregion

        #region Skin Init and Dispose

        public OMMaps()
            : base("OMMaps", OM.Host.getSkinImage("Icons|Icon-Navigation"), 1f, "A map frontend for OpenMobile", "OMMaps", "OM Dev team", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            _Map_ActiveNavigationRoute = new NavigationRoute[OM.Host.ScreenCount];

            // Datasources
            DataSources_Subscribe();

            // Queue panels
            base.PanelManager.QueuePanel("OMMaps", InitializeMapPanel, true);
            
            // Return
            return eLoadStatus.LoadSuccessful;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region Datasources

        void DataSources_Subscribe()
        {
            // Subscribe to the multiscreen datasource (no use in looping over screens anymore, framework automatically takes care of this if no screen is specified)
            OM.Host.DataHandler.SubscribeToDataSource("Map.Show.MapPlugin", (x) =>
            {
                if ((bool)x.Value)
                    base.GotoPanel(x.Screen, "OMMaps");
            });

            // Subscribe to datasource updates for map route data
            OM.Host.DataHandler.SubscribeToDataSource("Map.RouteData", (x) =>
            {   // Map route object as OpenMobile.NavEngine.NavigationRoute
                _Map_ActiveNavigationRoute[x.Screen] = (NavigationRoute)x.Value;
                if (_Map_ActiveNavigationRoute[x.Screen] != null)
                    GUI_RoutePanel(x.Screen, true, header: "Route summary", text: _Map_ActiveNavigationRoute[x.Screen].RouteText);
                else
                    GUI_RoutePanel(x.Screen, false, header: "Route summary", text: "No route active");
            });
        }

        #endregion

        #region Map panel and GUI

        private OMPanel InitializeMapPanel()
        {
            OMPanel panel = new OMPanel("OMMaps", "Map", this.pluginIcon);

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(255, Color.Black)));
            panel.addControl(shapeBackground);

            OMBasicShape shp_InfoGroup_Header = new OMBasicShape("shp_InfoGroup_Header", shapeBackground.Region.Left + (shapeBackground.Region.Width / 2), shapeBackground.Region.Top + 30, shapeBackground.Region.Width / 2, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0f, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0f, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5f, 0, Color.Empty))
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
            img_MapGroup_Map.BackgroundColor = Color.Black;
            img_MapGroup_Map.DataSource = "Screen{:S:}.Map.Image";
            img_MapGroup_Map.OnResize += new userInteraction(imgMap_OnResize);
            panel.addControl(img_MapGroup_Map);

            OMMouseHandler mouse_MapGroup_MapHandler = new OMMouseHandler("mouse_MapGroup_MapHandler", img_MapGroup_Map);
            mouse_MapGroup_MapHandler.OnMouseDown += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseDown);
            mouse_MapGroup_MapHandler.OnMouseMove += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseMove);
            mouse_MapGroup_MapHandler.OnMouseUp += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnMouseUp);
            mouse_MapGroup_MapHandler.OnClick += new OMMouseHandler.MouseEventArgsHandler(mouseMapHandler_OnClick);
            panel.addControl(mouse_MapGroup_MapHandler);

            OMButton btn_MapGroup_ZoomIn = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_ZoomIn", img_MapGroup_Map.Region.Left + 10, img_MapGroup_Map.Region.Top + 10, 90, 90, null, null, GraphicCorners.All, "", null, "+", null, null, null, "Arial", 75, BuiltInComponents.SystemSettings.SkinTextColor);
            btn_MapGroup_ZoomIn.Opacity = 140;
            btn_MapGroup_ZoomIn.Command_Click = "{:S:}Map.Zoom.In";
            panel.addControl(btn_MapGroup_ZoomIn);

            OMButton btn_MapGroup_ZoomOut = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_ZoomOut", img_MapGroup_Map.Region.Right - 100, img_MapGroup_Map.Region.Top + 10, 90, 90, null, null, GraphicCorners.All, "", null, "-", null, null, null, "Arial", 75, BuiltInComponents.SystemSettings.SkinTextColor);
            btn_MapGroup_ZoomOut.Opacity = 140;
            btn_MapGroup_ZoomOut.Command_Click = "{:S:}Map.Zoom.Out";
            panel.addControl(btn_MapGroup_ZoomOut);

            OMButton btn_MapGroup_CurrentLoc = OMButton.PreConfigLayout_BasicStyle("btn_MapGroup_CurrentLoc", img_MapGroup_Map.Region.Left + 10, img_MapGroup_Map.Region.Bottom - 100, 90, 90, null, null, GraphicCorners.All, "", OM.Host.getSkinImage("AIcons|10-device-access-location-found").image.Copy().Overlay(BuiltInComponents.SystemSettings.SkinTextColor), "", null, null, null);
            btn_MapGroup_CurrentLoc.Opacity = 140;
            btn_MapGroup_CurrentLoc.Command_Click = "{:S:}Map.Goto.Current";

            OMLabel lbl_MapGroup_ZoomLevel = new OMLabel("lbl_MapGroup_ZoomLevel", btn_MapGroup_ZoomIn.Region.Left, btn_MapGroup_ZoomIn.Region.Bottom, btn_MapGroup_ZoomIn.Region.Width, 35);
            lbl_MapGroup_ZoomLevel.FontSize = 30;
            lbl_MapGroup_ZoomLevel.DataSource = "{:S:}Map.Zoom.Level";
            panel.addControl(lbl_MapGroup_ZoomLevel);
     
            panel.addControl(btn_MapGroup_CurrentLoc);

            OMImage img_MapGroup_Updating = new OMImage("img_MapGroup_Updating", OM.Host.ClientArea_Init.Left + (OM.Host.ClientArea_Init.Width / 2), OM.Host.ClientArea_Init.Top + (OM.Host.ClientArea_Init.Height / 2), OM.Host.getSkinImage("BusyAnimationTransparent.gif"));
            img_MapGroup_Updating.Left -= img_MapGroup_Updating.Image.image.Width / 2;
            img_MapGroup_Updating.Top -= img_MapGroup_Updating.Image.image.Height / 2;
            img_MapGroup_Updating.DataSource = "{:S:}Map.Busy";
            img_MapGroup_Updating.DataSourceControlsVisibility = true;
            img_MapGroup_Updating.Visible = true;
            panel.addControl(img_MapGroup_Updating);

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(pluginName, panel.Name, "OMMaps_PopUpMenuStrip");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Goto", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|7-location-map"), "Goto", true, OnClick: mnuItem_Goto_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Route", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|7-location-directions"), "Route", true, OnClick: mnuItem_Route_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_RouteByMapMarkers", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|7-location-directions"), "Route from map", true, OnClick: mnuItem_RouteByMapMarkers_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShowRouteInfo", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-about"), "Show route info", true, OnClick: mnuItem_ShowRouteInfo_OnClick));
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_CurrentLocation", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|7-location-place"), "Current location", true, OnClick: mnuItem_CurrentLocation_OnClick));
            //PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Test", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Test", true, OnClick: mnuItem_Test_OnClick));
            //PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_Test2", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Test", true, OnClick: mnuItem_Test2_OnClick));
            panel.PopUpMenu = PopUpMenuStrip;

            panel.Entering += new PanelEvent(panel_Entering);
            panel.Leaving += new PanelEvent(panel_Leaving);

            return panel;
        }

        void mouseMapHandler_OnClick(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Mouse.EmulateMouseClick", e.Button, e.MouseLocalPosition);
        }
        void mouseMapHandler_OnMouseUp(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Mouse.EmulateMouseUp", e.Button, e.MouseLocalPosition);
        }
        void mouseMapHandler_OnMouseMove(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Mouse.EmulateMouseMove", e.Button, e.MouseLocalPosition);
        }
        void mouseMapHandler_OnMouseDown(int screen, OMControl sender, OMMouseHandler.MouseEventArgs e)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Mouse.EmulateMouseDown", e.Button, e.MouseLocalPosition);
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Refresh");
            OM.Host.CommandHandler.ExecuteCommand(screen, "OM;System.Idle.Disable");
        }

        void panel_Leaving(OMPanel sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand(screen, "OM;System.Idle.Enable");
        }

        void btn_InfoGroup_Hide_OnClick(OMControl sender, int screen)
        {
            GUI_RoutePanel(screen, false);
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

        private void GUI_RoutePanel(int screen, bool show, bool fast = false, string header = null, string text = null)
        {
            OMPanel panel = base.PanelManager[screen, "OMMaps"];
            if (panel == null)
                return;

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

                    OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Size.Set", imgMap.Region.Width, imgMap.Region.Height);

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
                    OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Size.Set", imgMap.Region.Width, imgMap.Region.Height);

                    #endregion
                }
                OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Refresh");
                imgMap.RefreshGraphic();
            }
        }

        #endregion

        #region Menu items

        void mnuItem_Test_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            //OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Show.MapPlugin");
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Marker.Add", new Location(60.7263800, 10.6171800), this.pluginName, "testmarker", 1, "Raufoss");
            //OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Markers.Clear");
            //Location loc = new Location("Storgaten, Raufoss, Norge");
            //Location loc = new Location();
            //loc.Keyword = "Wallmart";
            //Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", loc);
            //Location loc = OM.Host.CommandHandler.ExecuteCommand<Location>("Map.Lookup.Location", OM.Host.CurrentLocation);
        }
        void mnuItem_Test2_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            //OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Marker.Add", new Location(60.7263800, 10.6171800), this.pluginName, "testmarker", OM.Host.getSkinImage("OMFuel|Logos|Shell").image.Copy().Resize(30), "Shell\r\n$1.222 (Regular)");
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Marker.ZoomAll", this.pluginName);
        }

        void mnuItem_Goto_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            string location = OSK.ShowDefaultOSK(screen, "", "location keyword", "Enter location/adress to go to", OSKInputTypes.Keypad, false);
            string reply = OM.Host.CommandHandler.ExecuteCommand<string>(screen, "Map.Goto.Location", location);
            if (reply != String.Empty)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, "OMMaps");
                dialog.Header = "Unknown location";
                dialog.Text = String.Format("{0} can't find: '{1}'\r\nreason: {2}", this.pluginName, location, reply);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
        }

        void mnuItem_CurrentLocation_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Goto.Current");
        }

        void mnuItem_Route_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, true);

            string startLocation = OSK.ShowDefaultOSK(screen, "", "Route start location (leave empty to use current)", "Enter start location or leave empty to use current", OSKInputTypes.Keypad, false);
            string endLocation = OSK.ShowDefaultOSK(screen, "", "Route end location", "Enter end location or adress", OSKInputTypes.Keypad, false);

            if (String.IsNullOrEmpty(endLocation))
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, "OMMaps");
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

            string reply = OM.Host.CommandHandler.ExecuteCommand<string>(screen, "Map.Route", startLocation, endLocation);
            if (reply != string.Empty)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, base.PanelManager[screen, "OMMaps"]);
                dialog.Header = "Unable to get directions";
                dialog.Text = String.Format("{0} is unable to get directions!\r\nReason: {1}", this.pluginName, reply);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }
        }

        void mnuItem_RouteByMapMarkers_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            string reply = OM.Host.CommandHandler.ExecuteCommand<string>(screen, "Map.Route.ByMarkers");
            if (reply != string.Empty)
            {
                OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(this, base.PanelManager[screen, "OMMaps"]);
                dialog.Header = "Unable to get directions";
                dialog.Text = String.Format("{0} is unable to get directions!\r\nReason: {1}", this.pluginName, reply);
                dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Exclamation;
                dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                dialog.ShowMsgBox(screen);
            }

        }

        void mnuItem_ShowRouteInfo_OnClick(OMControl sender, int screen)
        {
            OM.Host.UIHandler.PopUpMenu_Hide(screen, false);
            GUI_RoutePanel(screen, true);
        }

        #endregion
    }
}
