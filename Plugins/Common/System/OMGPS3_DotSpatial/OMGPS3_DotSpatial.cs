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
using OpenMobile.helperFunctions;
using System.IO.Ports;
using System.Text;
using System.Diagnostics;
using System.Threading;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using DotSpatial.Positioning;
using System.Collections.Generic;

namespace OMGPS3_DotSpatial
{
    public sealed class OMGPS3_DotSpatial : HighLevelCode, IDataSource
    {
        private enum GPSConnectMode
        {
            /// <summary>
            /// Automatically detect GPS
            /// </summary>
            Auto,

            /// <summary>
            /// Manually set com port
            /// </summary>
            Manual,
        }

        private static readonly IList<int> _GPS_BaudRates = new List<int>(new[] { 115200, 57600, 38400, 19200, 9600, 4800 });

        private bool _GPSSetLocalSystemTime = false;
        private Notification _GPSStatusNotification;
        private imageItem _IconGPSFixed;
        private imageItem _IconGPSNotFixed;
        private imageItem _IconGPSSearching;

        private Device _GPSDevice;
        private NmeaInterpreter _NmeaInterpreter = new NmeaInterpreter();

        private GPSConnectMode _GPS_Connectmode = GPSConnectMode.Auto;
        private string _GPS_PortName = "";
        private int _GPS_BaudRate = _GPS_BaudRates[5];

        public OMGPS3_DotSpatial()
            : base("OMGPS3_DotSpatial", OM.Host.getSkinImage("Icons|Icon-GPS2"), 0.1f, "GPS Data", "GPS Data", "OM DevTeam/Borte", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Settings
            Settings_SetDefaultValues();
            Settings_MapVariables();

            DataSources_Register();

            // Setup common image resources
            _IconGPSFixed = OM.Host.getSkinImage("AIcons|10-device-access-location-found");
            _IconGPSNotFixed = new imageItem(OM.Host.getSkinImage("AIcons|10-device-access-location-off").image.Copy().Overlay(Color.Yellow).Glow(Color.Yellow, 15));
            _IconGPSSearching = new imageItem(OM.Host.getSkinImage("AIcons|10-device-access-location-searching").image.Copy().Overlay(Color.Red).Glow(Color.Red, 15));

            // Start communicating to the GPS 
            OpenMobile.Threading.SafeThread.Asynchronous(() => StartGPS());

            // Queue panels
            base.PanelManager.QueuePanel("GPSInfo", InitializeGPSPanel, true);

            // Register for host events
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            return eLoadStatus.LoadSuccessful;
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.USBDeviceAdded)
            {   // Was this a port that was connected
                if ((string)args[0] == "PORT")
                {   // Yes
                    if (!Devices.IsDeviceDetected)
                        // Try to detect again
                        OpenMobile.Threading.SafeThread.Asynchronous(() => Devices.BeginDetection());
                }
            }
        }

        public override void Dispose()
        {
            Devices.CancelDetection(true);
            OM.Host.DebugMsg(DebugMessageType.Warning, "Device detection canceled (Disposed)!");
            base.Dispose();
        }

        private void StartGPS()
        {
            Devices.DeviceDetectionAttempted += Devices_DeviceDetectionAttempted;
            Devices.DeviceDetectionAttemptFailed += Devices_DeviceDetectionAttemptFailed;
            Devices.DeviceDetectionStarted += Devices_DeviceDetectionStarted;
            Devices.DeviceDetectionCompleted += Devices_DeviceDetectionCompleted;
            Devices.DeviceDetectionCanceled += Devices_DeviceDetectionCanceled;
            Devices.DeviceDetected += Devices_DeviceDetected;
            Devices.AllowBluetoothConnections = true;
            Devices.AllowExhaustiveSerialPortScanning = false;
            Devices.AllowSerialConnections = true;
            Devices.IsOnlyFirstDeviceDetected = false;

            _NmeaInterpreter.Started += new EventHandler(_NmeaInterpreter_Started);
            _NmeaInterpreter.Starting += new EventHandler<DeviceEventArgs>(_NmeaInterpreter_Starting);
            _NmeaInterpreter.Stopping += new EventHandler(_NmeaInterpreter_Stopping);
            _NmeaInterpreter.Stopped += new EventHandler(_NmeaInterpreter_Stopped);
            _NmeaInterpreter.SpeedChanged += new EventHandler<SpeedEventArgs>(_NmeaInterpreter_SpeedChanged);
            _NmeaInterpreter.AltitudeChanged += new EventHandler<DistanceEventArgs>(_NmeaInterpreter_AltitudeChanged);
            _NmeaInterpreter.SentenceReceived += new EventHandler<NmeaSentenceEventArgs>(_NmeaInterpreter_SentenceReceived);
            _NmeaInterpreter.FixAcquired += new EventHandler(_NmeaInterpreter_FixAcquired);
            _NmeaInterpreter.FixLost += new EventHandler(_NmeaInterpreter_FixLost);
            _NmeaInterpreter.SatellitesChanged += new EventHandler<SatelliteListEventArgs>(_NmeaInterpreter_SatellitesChanged);
            _NmeaInterpreter.UtcDateTimeChanged += new EventHandler<DateTimeEventArgs>(_NmeaInterpreter_UtcDateTimeChanged);
            _NmeaInterpreter.BearingChanged += new EventHandler<AzimuthEventArgs>(_NmeaInterpreter_BearingChanged);
            _NmeaInterpreter.PositionChanged += new EventHandler<PositionEventArgs>(_NmeaInterpreter_PositionChanged);

            OM.Host.DebugMsg(DebugMessageType.Info, "Requesting serial port access...");
            if (OpenMobile.helperFunctions.SerialAccess.GetAccess(this))
                OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access granted...");
            else
                OM.Host.DebugMsg(DebugMessageType.Error, "Serial port access timed out!");

            //try
            //{
            //    Devices.Undetect();
            //    OM.Host.DebugMsg(DebugMessageType.Info, "Undetected known devices, to be ready for discovery of devices");
            //}
            //catch (Exception ex)
            //{
            //    //OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Failed to undetect GPS devices (Exception: {0})", ex.Message));
            //    OM.Host.DebugMsg("Failed to undetect GPS devices", ex);
            //}

            // Connect to GPS
            if (_GPS_Connectmode == GPSConnectMode.Auto)
            {
                OM.Host.DebugMsg(DebugMessageType.Info, "GPS Detection set to automatic, starting detection.");
                Devices.BeginDetection();
            }
            else
            {
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("GPS Detection set to manual. Configured port: {0}", _GPS_PortName));
                Notifications_InitAndShow();

                // Ensure this is a valid selection
                if (IsPortValid(_GPS_PortName))
                {   // Valid port
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Trying to manually connect to {0}", _GPS_PortName));
                    SerialDevice manualDevice = new SerialDevice(_GPS_PortName, _GPS_BaudRate);
                    manualDevice.BeginDetection();
                    manualDevice.WaitForDetection(new TimeSpan(0, 0, 5));
                    manualDevice.CancelDetection();
                    if (manualDevice.IsGpsDevice)
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Device at port {0} recognized as GPS device", _GPS_PortName));
                        Devices.Add(manualDevice);
                    }
                    else
                    {
                        OM.Host.DebugMsg(DebugMessageType.Info, String.Format("No device or device at port {0} NOT recognized as GPS device", _GPS_PortName));
                    }
                }
                Devices_Connect();
            }
        }

        private bool IsPortValid(string portname)
        {
            List<string> ports = new List<string>();
            foreach (var device in Devices.SerialDevices)
                ports.Add(device.ToString());

            return ports.Contains(portname);
        }

        #region GPS Device Detection Events

        private void Devices_DeviceDetectionCanceled(object sender, EventArgs e)
        {
            //OM.Host.DebugMsg(DebugMessageType.Warning, "Device detection canceled!");
            //_GPSStatusNotification_SetData(text: "Device detection canceled!");
            OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);
            OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access released");
        }

        private void Devices_DeviceDetected(object sender, DeviceEventArgs e)
        {
            _GPSStatusNotification_SetData(text: String.Format("GPS detected :{0}", e.Device));
            _GPSDevice = e.Device;
            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Device detected: {0}", e.Device));
        }

        private void Devices_DeviceDetectionCompleted(object sender, EventArgs e)
        {
            Devices_Connect();
            Devices.CancelDetection(true);
        }

        private void Devices_DeviceDetectionStarted(object sender, EventArgs e)
        {
            Notifications_InitAndShow();
        }

        private void Devices_DeviceDetectionAttemptFailed(object sender, DeviceDetectionExceptionEventArgs e)
        {
            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("No device detected at {0}. Message: ", e.Device, e.Exception.Message));
            //_GPSStatusNotification_SetData(text: String.Format("GPS detection failed :{0} ({1})", e.Device, e.Exception.Message));
        }

        private void Devices_DeviceDetectionAttempted(object sender, DeviceEventArgs e)
        {
            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Trying to detect device at {0}",e.Device));
            //_GPSStatusNotification_SetData(text: String.Format("GPS detecting :{0}", e.Device));
        }

        private void Devices_Connect()
        {
            if (_GPSDevice != null)
            {
                _GPSStatusNotification_SetData(header: "GPS detected", text: String.Format("Device found at {0}", _GPSDevice));
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Device found at {0}, starting GPS communication...", _GPSDevice));

                // Start gps processing
                try
                {
                    _NmeaInterpreter.Start();
                }
                catch (Exception ex)
                {
                    _GPSStatusNotification_SetData(text: String.Format("GPS connection failed ({0})", _GPSDevice));
                    OM.Host.DebugMsg(String.Format("GPS connection failed ({0})", _GPSDevice), ex);
                    OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);
                    OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access released");
                }
            }
            else
            {
                _GPSStatusNotification_SetData(header: "No GPS detected!", text: "");
                // Also remove GPS icon in status bar
                _GPSStatusNotification.IconStatusBar = null;
                OM.Host.DebugMsg(DebugMessageType.Warning, "No GPS detected!");
                OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);
                OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access released");
                // Undected devices but mask out any error being thrown while undetecting
                try
                {
                    Devices.Undetect();
                }
                catch { }
            }
        }

        #endregion
        
        #region Notifications

        void Notifications_InitAndShow()
        {
            // Initialize GPS Notification
            if (_GPSStatusNotification != null)
            {
                OM.Host.UIHandler.RemoveNotification(_GPSStatusNotification, true);
                _GPSStatusNotification = null;
            }

            _GPSStatusNotification = new Notification(this, "GPS_AutoDiscover", OM.Host.getSkinImage("Icons|Icon-GPS").image, _IconGPSSearching.image, "Connecting to GPS", "Detecting device");
            _GPSStatusNotification.Global = true;
            _GPSStatusNotification.ClearAction += new Notification.NotificationAction(_GPSStatusNotification_ClearAction);
            _GPSStatusNotification.ClickAction += new Notification.NotificationAction(_GPSStatusNotification_ClickAction);

            // Show notification
            OM.Host.UIHandler.AddNotification(_GPSStatusNotification);
        }

        void _GPSStatusNotification_SetData(string header = null, string text = null, OImage iconStatusBar = null, bool updatePanelText = true, bool hideNotificationText = false)
        {
            // Set notification data
            if (header != null)
                _GPSStatusNotification.Header = header;
            if (text != null)
                _GPSStatusNotification.Text = text;
            if (iconStatusBar != null)
                _GPSStatusNotification.IconStatusBar = iconStatusBar;

            // Hide the text part of the notification
            if (hideNotificationText)
            {
                _GPSStatusNotification.State = Notification.States.Active;
                _GPSStatusNotification.Style = Notification.Styles.IconOnly;
            }

            // Update status text
            if (updatePanelText)
            {
                OM.Host.ForEachScreen((int screen) =>
                {
                    OMPanel panel = base.PanelManager[screen, "GPSInfo"];
                    if (panel == null || !panel.IsVisible(screen))
                        return;

                    // GPS Info label
                    OMLabel lblGPSInfo = panel["lblGPSInfo"] as OMLabel;
                    lblGPSInfo.Text = _GPSStatusNotification.Header + "\r\n";
                    lblGPSInfo.Text += _GPSStatusNotification.Text;
                });
            }
        }

        void _GPSStatusNotification_ClickAction(Notification notification, int screen, ref bool cancel)
        {
            base.GotoPanel(screen, "GPSInfo");
        }
        void _GPSStatusNotification_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            // Change notification type to active
            notification.State = Notification.States.Active;
            notification.Style = Notification.Styles.IconOnly;
            // Cancel the clear request on this notification
            cancel = true;
        }

        #endregion

        #region Settings

        protected override void Settings()
        {
            //base.MySettings.Add(Setting.ButtonSetting("GPS.gpsButton_CountryList", "Country Lists (Downloadables)"));
            base.MySettings.Add(Setting.BooleanSetting("GPS.SetSystemTimeFromGPS", String.Empty, "Set systemtime from GPS", StoredData.Get(this, "GPS.SetSystemTimeFromGPS")));
            base.MySettings.Add(Setting.EnumSetting<GPSConnectMode>("GPS.ConnectMode", "GPS Connection mode", "The connection mode for the GPS", StoredData.Get(this, "GPS.ConnectMode")));
            base.MySettings.Add(Setting.TextList<SerialDevice>("GPS.SerialDevice.PortName", "Set comport", "All available serial ports", StoredData.Get(this, "GPS.SerialDevice.PortName"), Devices.SerialDevices));
            base.MySettings.Add(Setting.TextList<int>("GPS.SerialDevice.BaudRate", "Set baudrate", "Available baudrates", StoredData.Get(this, "GPS.SerialDevice.BaudRate"), _GPS_BaudRates));
        }

        private void Settings_MapVariables()
        {
            _GPSSetLocalSystemTime = StoredData.GetBool(this, "GPS.SetSystemTimeFromGPS");
            Devices.IsClockSynchronizationEnabled = _GPSSetLocalSystemTime;

            _GPS_Connectmode = StoredData.GetEnum<GPSConnectMode>(this, "GPS.ConnectMode", GPSConnectMode.Auto);
            _GPS_PortName = StoredData.Get(this, "GPS.SerialDevice.PortName");
            _GPS_BaudRate = StoredData.GetInt(this, "GPS.SerialDevice.BaudRate", _GPS_BaudRates[5]);

        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, "GPS.SetSystemTimeFromGPS", false);
            StoredData.SetDefaultValue(this, "GPS.ConnectMode", GPSConnectMode.Auto);
            StoredData.SetDefaultValue(this, "GPS.SerialDevice.PortName", "Com0");
            StoredData.SetDefaultValue(this, "GPS.SerialDevice.BaudRate", _GPS_BaudRates[5]);
            Settings_MapVariables();
        }

        protected override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);
            
            // Update local variables
            Settings_MapVariables();

            //// Execute any button presses
            //if (setting.Name.Equals("GPS.gpsButton_CountryList"))
            //    base.GotoPanel(screen, "GEOCodingPanel");
        }

        #endregion

        #region GPS Info Panel

        private OMPanel InitializeGPSPanel()
        {
            OMPanel panel = new OMPanel("GPSInfo", "GPS Information", this.pluginIcon);

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 10, 900, OM.Host.ClientArea_Init.Height - 20,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            OMImage imgGPSSatRadar = new OMImage("imgGPSSatRadar", shapeBackground.Left + 50, shapeBackground.Top + 40, OM.Host.getSkinImage("Objects|GPS-Radar"));
            imgGPSSatRadar.Opacity = 127;
            panel.addControl(imgGPSSatRadar);

            // Greate GPS satellite icons
            for (int i = 0; i < 32; i++)
            {
                OMBasicShape shpGPSIcon = new OMBasicShape(String.Format("shpGPSIcon_{0}", i), 100, 100, 15, 15,
                    new ShapeData(shapes.Oval, Color.Red, Color.Blue, 0));
                shpGPSIcon.Visible = false;
                shpGPSIcon.Opacity = 178;
                panel.addControl(shpGPSIcon);
            }

            // GPS Info text
            OMLabel lblGPSInfo = new OMLabel("lblGPSInfo", imgGPSSatRadar.Region.Right + 20, imgGPSSatRadar.Region.Top - 10, 400, imgGPSSatRadar.Region.Height + 20);
            lblGPSInfo.TextAlignment = Alignment.TopLeft;
            lblGPSInfo.Opacity = 178;
            panel.addControl(lblGPSInfo);

            // Create the buttonstrip popup
            ButtonStrip PopUpMenuStrip = new ButtonStrip(pluginName, panel.Name, "GPSInfo_PopUpMenuStrip");
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShowSettings", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Settings", true, cmdOnClick: base.GetCmdString_GotoMySettingsPanel()));
            panel.PopUpMenu = PopUpMenuStrip;

            panel.Entering += new PanelEvent(panel_Entering);

            return panel;
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            OMPanel panel = base.PanelManager[screen, "GPSInfo"];

            // GPS Info label
            OMLabel lblGPSInfo = panel["lblGPSInfo"] as OMLabel;
            lblGPSInfo.Text = _GPSStatusNotification.Header + "\r\n";
            lblGPSInfo.Text += _GPSStatusNotification.Text;
        }

        private void UpdateGPSSatView()
        {
            OM.Host.ForEachScreen((int screen) =>
            {
                try
                {
                    OMPanel panel = base.PanelManager[screen, "GPSInfo"];

                    if (!panel.IsVisible(screen))
                        return;

                    OMImage imgGPSRadar = panel["imgGPSSatRadar"] as OMImage;

                    // GPS Info
                    OMLabel lblGPSInfo = panel["lblGPSInfo"] as OMLabel;
                    lblGPSInfo.Text = "GPS Data:\r\n";
                    lblGPSInfo.Text += "\r\n";
                    if (_NmeaInterpreter.IsFixed)
                    {
                        lblGPSInfo.Text += String.Format("Position: {0}\r\n", Devices.Position);
                        lblGPSInfo.Text += String.Format("Latitude: {0}\r\n", Devices.Position.Latitude.DecimalDegrees);
                        lblGPSInfo.Text += String.Format("Longitude: {0}\r\n", Devices.Position.Longitude.DecimalDegrees);
                        lblGPSInfo.Text += String.Format("Altitude: {0} meters\r\n", Devices.Altitude);
                        lblGPSInfo.Text += String.Format("Bearing: {0}\r\n", Devices.Bearing.ToString());
                        //lblGPSInfo.Text += "\r\n";
                        //lblGPSInfo.Text += String.Format("HDOP: {0} ({1})\r\n",_NmeaInterpreter.HorizontalDilutionOfPrecision.Rating, _NmeaInterpreter.HorizontalDilutionOfPrecision.Value);
                        //lblGPSInfo.Text += String.Format("VDOP: {0} ({1})\r\n", _NmeaInterpreter.VerticalDilutionOfPrecision.Rating, _NmeaInterpreter.VerticalDilutionOfPrecision.Value);
                        lblGPSInfo.Text += String.Format("Precision: {0} meters\r\n", _NmeaInterpreter.FixPrecisionEstimate.ToMeters().Value);
                    }
                    else
                    {
                        lblGPSInfo.Text += "Waiting for GPS fix...\r\n";
                    }
                    lblGPSInfo.Text += "\r\n";

                    lblGPSInfo.Text += String.Format("UTC: {0}\r\n", _NmeaInterpreter.UtcDateTime);
                    lblGPSInfo.Text += String.Format("LocalTime: {0}\r\n", _NmeaInterpreter.UtcDateTime.ToLocalTime());
                    lblGPSInfo.Text += "\r\n";
                    lblGPSInfo.Text += String.Format("{0} satellites used of {1} visible\r\n", _NmeaInterpreter.FixedSatelliteCount, _NmeaInterpreter.Satellites.Count);
                    lblGPSInfo.Text += "\r\n";
                    lblGPSInfo.Text += String.Format("Connected to {0} ({1})\r\n", Devices.Any.Name, Devices.Any.Reliability.ToString());

                    // Calculate center point
                    Point center = imgGPSRadar.Region.Center;
                    double maxRadius = (System.Math.Min(imgGPSRadar.Region.Width, imgGPSRadar.Region.Height) - 5) / 2;

                    int i = 0;
                    foreach (Satellite sat in _NmeaInterpreter.Satellites)
                    {
                        // Get control
                        OMBasicShape shpSat = panel[String.Format("shpGPSIcon_{0}", i)] as OMBasicShape;

                        double h = (double)System.Math.Cos(sat.Elevation.ToRadians().Value) * maxRadius;
                        int satX = (int)(center.X + h * Math.Sin(sat.Azimuth.ToRadians().Value)) - (shpSat.Region.Width / 2);
                        int satY = (int)(center.Y - h * Math.Cos(sat.Azimuth.ToRadians().Value)) - (shpSat.Region.Height / 2);

                        shpSat.Left = satX;
                        shpSat.Top = satY;
                        shpSat.Visible = true;

                        ShapeData shpDta = shpSat.ShapeData;

                        if (sat.IsFixed)
                            shpDta.BorderSize = 3;
                        else
                            shpDta.BorderSize = 0;

                        switch (sat.SignalToNoiseRatio.Rating)
                        {
                            case SignalToNoiseRatioRating.Excellent:
                            case SignalToNoiseRatioRating.Good:
                                shpDta.FillColor = Color.Green;
                                break;
                            case SignalToNoiseRatioRating.Moderate:
                            case SignalToNoiseRatioRating.Poor:
                                shpDta.FillColor = Color.Yellow;
                                break;
                            case SignalToNoiseRatioRating.None:
                                shpDta.FillColor = Color.Red;
                                break;
                            default:
                                break;
                        }

                        shpSat.ShapeData = shpDta;
                        i++;
                    }

                    for (int i2 = i; i2 < 32; i2++)
                    {
                        OMBasicShape shpSat = panel[String.Format("shpGPSIcon_{0}", i)] as OMBasicShape;
                        shpSat.Visible = false;
                    }
                }
                catch
                {
                }
            });
        }

        #endregion

        #region DataSources

        private void DataSources_Register()
        {
            //create dataSources
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "MPH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS MPH"), 0.0d);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "KMH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS KMH"), 0.0d);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "", OpenMobile.Data.DataSource.DataTypes.raw, "GPS speed in system default format"), OpenMobile.Framework.Globalization.convertSpeedToLocal(0, true));
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Visible", OpenMobile.Data.DataSource.DataTypes.raw, "GPS Visible satelite count"), 0);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "InUse", OpenMobile.Data.DataSource.DataTypes.raw, "GPS Used satelite count"), 0);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Fix", OpenMobile.Data.DataSource.DataTypes.binary, "GPS Satelite fix state"), false);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sentence", "String", OpenMobile.Data.DataSource.DataTypes.raw, "GPS string of data"), "");
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Bearing", "Text", OpenMobile.Data.DataSource.DataTypes.text, "GPS bearing in text"), "");
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Bearing", "TextNum", OpenMobile.Data.DataSource.DataTypes.raw, "GPS bearing text as an enum number (0=N, 1=NNE, 2=NE, 3=ENE, 4=E, 5=ESE, 6=SE, 7=SSE, 8=S, 9=SSW, 10=SW, 11=WSW, 12=W, 13=WNW, 14=NW, 15=NNW"), "");
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Bearing", "Angle", OpenMobile.Data.DataSource.DataTypes.raw, "GPS bearing angle as double"), 0.0d);
        }

        #endregion

        #region NmeaInterpreter Events

        private void _NmeaInterpreter_SpeedChanged(object sender, SpeedEventArgs e)
        {
            Speed speed = e.Speed;
            if (e.Speed.ToKilometersPerHour().Value < 2)
                speed = new Speed(0, SpeedUnit.KilometersPerHour);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.MPH", speed.ToStatuteMilesPerHour().Value);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.KMH", speed.ToKilometersPerHour().Value);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed", OpenMobile.Framework.Globalization.convertSpeedToLocal(speed.ToKilometersPerHour().Value, true));
        }

        private void _NmeaInterpreter_AltitudeChanged(object sender, DistanceEventArgs e)
        {
            OM.Host.CurrentLocation.Altitude = (float)e.Distance.ToMeters().Value;
        }

        private void _NmeaInterpreter_PositionChanged(object sender, PositionEventArgs e)
        {
            OM.Host.UpdateLocation(latitude: (float)e.Position.Latitude.DecimalDegrees,
            longitude: (float)e.Position.Longitude.DecimalDegrees);
        }

        private void _NmeaInterpreter_BearingChanged(object sender, AzimuthEventArgs e)
        {
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Bearing.Text", e.Azimuth.Direction.ToString());
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Bearing.TextNum", (int)e.Azimuth.Direction);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Bearing.Angle", OpenMobile.Math.MathHelper.RadiansToDegrees(e.Azimuth.ToRadians().Value));
        }

        private void _NmeaInterpreter_UtcDateTimeChanged(object sender, DateTimeEventArgs e)
        {
        }

        private void _NmeaInterpreter_SatellitesChanged(object sender, SatelliteListEventArgs e)
        {
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.Visible", _NmeaInterpreter.Satellites.Count);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.InUse", _NmeaInterpreter.FixedSatelliteCount);
            UpdateGPSSatView();
        }

        private void _NmeaInterpreter_FixLost(object sender, EventArgs e)
        {
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.Fix", false, false);
            // Show gps notification status
            _GPSStatusNotification_SetData(text: "No GPS fix!", iconStatusBar: _IconGPSNotFixed.image, updatePanelText: false);
        }

        private void _NmeaInterpreter_FixAcquired(object sender, EventArgs e)
        {
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.Fix", true, false);
            // Show gps notification status
            _GPSStatusNotification_SetData(text: String.Format("GPS Fixed ({0}sats) [{1}]", _NmeaInterpreter.FixedSatelliteCount, Devices.Position.ToString()),
            iconStatusBar: _IconGPSFixed.image, updatePanelText: false, hideNotificationText: true);

            float latitude = (float)Devices.Position.Latitude.DecimalDegrees;
            float longitude = (float)Devices.Position.Longitude.DecimalDegrees;
            if (!float.IsNaN(latitude) && !float.IsNaN(longitude))
                OM.Host.UpdateLocation(latitude: latitude, longitude: longitude);
        }

        private void _NmeaInterpreter_SentenceReceived(object sender, NmeaSentenceEventArgs e)
        {
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sentence.String", e.Sentence.Sentence);
            UpdateGPSSatView();
        }

        private void _NmeaInterpreter_Starting(object sender, DeviceEventArgs e)
        {
            _GPSStatusNotification_SetData(text: String.Format("Connecting to GPS ({0})", e.Device.Name));
            OM.Host.DebugMsg(DebugMessageType.Info, String.Format("NmeaInterpreter starting for device {0}", e.Device));
        }
        private void _NmeaInterpreter_Started(object sender, EventArgs e)
        {
            _GPSStatusNotification_SetData(header: "GPS connected", text: "Waiting for GPS fix and valid data...");
            OM.Host.DebugMsg(DebugMessageType.Info, "NmeaInterpreter started");
            OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(this);
            OM.Host.DebugMsg(DebugMessageType.Info, "Serial port access released");
        }
        private void _NmeaInterpreter_Stopping(object sender, EventArgs e)
        {
            _GPSStatusNotification_SetData(text: "Stopping GPS device...");
            OM.Host.DebugMsg(DebugMessageType.Info, "NmeaInterpreter stoping");
        }
        private void _NmeaInterpreter_Stopped(object sender, EventArgs e)
        {
            _GPSStatusNotification_SetData(text: "GPS device stopped.");
            OM.Host.DebugMsg(DebugMessageType.Info, "NmeaInterpreter stopped");
        }

        #endregion
    }
}
