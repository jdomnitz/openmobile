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
using OMGPS2.GEOCoding;

namespace OMGPS2
{
    public sealed class OMGPS2 : HighLevelCode, IDataSource
    {
        private enum GPSDiscoverySpeeds
        {
            Slow,
            Medium,
            Fast
        }

        private GPSDiscoverySpeeds _GPSDiscoverySpeed = GPSDiscoverySpeeds.Slow;
        private int _GPSAutoConnectDelay = 1200;
        private string _GPSPortName;
        private int _GPSBaudRate = 0;
        private bool _GPSParseData = true;
        private int[] _GPSBaudRates = new int[7] { 115200, 57600, 38400, 19200, 9600, 4800, 2400 };
        private bool _GPSSetLocalSystemTime = false;
        private NMEAProtocol _GPSData = new NMEAProtocol();
        private string _GPSRawString;
        private Notification _GPSStatusNotification;
        private imageItem _IconGPSFixed;
        private imageItem _IconGPSNotFixed;
        private imageItem _IconGPSSearching;
        private bool _GPSLocalSystemTimeSet = false;
        private GeoCoding _GeoCoding;

        public OMGPS2()
            : base("OMGPS2", OM.Host.getPluginImage("OMGPS2", "Icon-GPS"), 0.1f, "GPS Data", "GPS Data", "Peter Yeaney", "peter.yeaney@gmail.com")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // Initialize GeoCoding
            _GeoCoding = new GeoCoding(this);

            // Settings
            Settings_SetDefaultValues();
            Settings_MapVariables();

            DataSources_Register();

            // Setup common image resources
            _IconGPSFixed = OM.Host.getSkinImage("AIcons|10-device-access-location-found");
            _IconGPSNotFixed = OM.Host.getSkinImage("AIcons|10-device-access-location-off");
            _IconGPSSearching = OM.Host.getSkinImage("AIcons|10-device-access-location-searching");

            // Start communicating to the GPS 
            OpenMobile.Threading.SafeThread.Asynchronous(() => GPS_Run());

            // Queue panels
            base.PanelManager.QueuePanel("GPSInfo", InitializeGPSPanel, true);

            return eLoadStatus.LoadSuccessful;
        }

        public override void Dispose()
        {
            _GPSParseData = false;
            base.Dispose();
        }


        #region Notifications

        void Notifications_InitAndShow()
        {
            // Initialize GPS Notification
            _GPSStatusNotification = new Notification(this, "GPS_AutoDiscover", OM.Host.getSkinImage("Icons|Icon-GPS").image, OM.Host.getSkinImage("AIcons|10-device-access-location-searching").image.Copy(), "Connecting to GPS", "Detecting device");
            _GPSStatusNotification.Global = true;
            _GPSStatusNotification.ClearAction += new Notification.NotificationAction(_GPSStatusNotification_ClearAction);
            _GPSStatusNotification.ClickAction += new Notification.NotificationAction(_GPSStatusNotification_ClickAction);

            // Show notification
            OM.Host.UIHandler.AddNotification(_GPSStatusNotification);
        }

        void _GPSStatusNotification_SetData(string header = null, string text = null, OImage iconStatusBar = null, bool updatePanelText = true)
        {
            // Set notification data
            if (header != null)
                _GPSStatusNotification.Header = header;
            if (text != null)
                _GPSStatusNotification.Text = text;
            if (iconStatusBar != null)
                _GPSStatusNotification.IconStatusBar = iconStatusBar;

            // Update status text
            if (updatePanelText)
            {
                OM.Host.ForEachScreen((int screen) =>
                {
                    OMPanel panel = base.PanelManager[screen, "GPSInfo"];
                    if (!panel.IsVisible(screen))
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

        public override void Settings()
        {
            base.MySettings.Add(Setting.ButtonSetting("GPS.gpsButton_CountryList", "Country Lists (Downloadables)"));
            base.MySettings.Add(Setting.EnumSetting<GPSDiscoverySpeeds>("GPS.AutoConnectTimer", String.Empty, "Auto Connect Time", StoredData.Get(this, "GPS.AutoConnectTimer")));
            base.MySettings.Add(Setting.BooleanSetting("GPS.SetSystemTimeFromGPS", String.Empty, "Set systemtime from GPS", StoredData.Get(this, "GPS.SetSystemTimeFromGPS")));
        }

        private void Settings_MapVariables()
        {
            _GPSDiscoverySpeed = (GPSDiscoverySpeeds)StoredData.GetInt(this, "GPS.AutoConnectTimer");
            _GPSPortName = StoredData.Get(this, "GPS.Port.Name");
            _GPSBaudRate = StoredData.GetInt(this, "GPS.Port.BaudRate");
            _GPSSetLocalSystemTime = StoredData.GetBool(this, "GPS.SetSystemTimeFromGPS");

            // Also convert discovery speed to actual timer setting
            switch (_GPSDiscoverySpeed)
            {
                case GPSDiscoverySpeeds.Slow:
                    _GPSAutoConnectDelay = 1200;
                    break;
                case GPSDiscoverySpeeds.Medium:
                    _GPSAutoConnectDelay = 850;
                    break;
                case GPSDiscoverySpeeds.Fast:
                    _GPSAutoConnectDelay = 500;
                    break;
                default:
                    break;
            }
        }

        private void Settings_SetDefaultValues()
        {
            StoredData.SetDefaultValue(this, "GPS.Port.BaudRate", "");
            StoredData.SetDefaultValue(this, "GPS.Port.Name", "");
            StoredData.SetDefaultValue(this, "GPS.AutoConnectTimer", GPSDiscoverySpeeds.Slow);
            StoredData.SetDefaultValue(this, "GPS.SetSystemTimeFromGPS", false);
            Settings_MapVariables();
        }

        public override void setting_OnSettingChanged(int screen, Setting setting)
        {
            base.setting_OnSettingChanged(screen, setting);
            
            // Update local variables
            Settings_MapVariables();

            // Execute any button presses
            if (setting.Name.Equals("GPS.gpsButton_CountryList"))
                base.GotoPanel(screen, "GEOCodingPanel");
        }

        #endregion

        #region GPS Connection handling

        private void GPS_Run()
        {
            Notifications_InitAndShow();

            SerialPort port = GPS_Connect();
            if (port != null)
            {   // GPS is available, Save connection data
                _GPSPortName = port.PortName;
                _GPSBaudRate = port.BaudRate;
                GPS_ConnectionDataSet(_GPSPortName, _GPSBaudRate);

                _GPSStatusNotification_SetData(header: "GPS device status");

                // Parse data
                GPS_ParseData(port);
            }
            else
            {   // No device detected
                _GPSStatusNotification_SetData(text:"No gps device detected");
            }
        }

        private void GPS_ConnectionDataSet(string portName, int baudRate)
        {
            _GPSPortName = StoredData.Get(this, "GPS.Port.Name");
            _GPSBaudRate = StoredData.GetInt(this, "GPS.Port.BaudRate");

            StoredData.Set(this, "GPS.Port.Name", portName);
            StoredData.Set(this, "GPS.Port.BaudRate", baudRate);

        }

        private void GPS_ParseData(SerialPort port)
        {  
            try
            {
                // Open port
                port.Open();               

                byte[] gpsData = new byte[256];
                while (_GPSParseData)
                {
                    _GPSRawString = port.ReadLine();
                    _GPSData.ParseBuffer(Encoding.Default.GetBytes(_GPSRawString));
                    DataSources_Update(_GPSData);
                }
            }
            catch (Exception ex)
            {   // Log exception
                OM.Host.DebugMsg(String.Format("OMGPS2 exeption while parsing GPS data from port {0}", port.PortName), ex);
            }
            finally
            {
                if (port.IsOpen)
                    port.Close();
            }
        }

        private SerialPort GPS_Connect()
        {
            // Test previously used connection
            if (!String.IsNullOrEmpty(_GPSPortName))
            {
                bool portAvailable = true;
                SerialPort port = new SerialPort(_GPSPortName, _GPSBaudRate);
                if (GPS_IsGPSDataPresent(port, _GPSAutoConnectDelay, ref portAvailable))
                {   // Data available at presently used connection, use this 
                    _GPSStatusNotification_SetData(text: String.Format("GPS Detected at {0} ({1}BPS)", _GPSPortName, _GPSBaudRate));
                    return port;
                }
            }

            // No data available, autodiscover GPS
            foreach (string portName in SerialPort.GetPortNames())
            {
                for (int i = 0; i < _GPSBaudRates.Length; i++)
                {
                    int baudRate = _GPSBaudRates[i];

                    // Save data for later usage
                    _GPSPortName = portName;
                    _GPSBaudRate = baudRate;

                    // Show status while searching                    
                    int percentage = 100 - (int)(((float)(i+1) / (float)_GPSBaudRates.Length) * 100f);
                    _GPSStatusNotification.IconStatusBar.Dispose();   // Dispose off old image to prevent memory leaks

                    _GPSStatusNotification_SetData(text: String.Format("Testing {0} ({1}BPS)", portName, baudRate),
                        iconStatusBar: OM.Host.getSkinImage("AIcons|10-device-access-location-searching").image.Copy().Overlay(BuiltInComponents.SystemSettings.SkinFocusColor, percentage, OpenMobile.Graphics.OImage.OverLayDirections.BottomToTop)
                        );

                    bool portAvailable = true;
                    SerialPort port = new SerialPort(portName, baudRate);
                    if (GPS_IsGPSDataPresent(port, _GPSAutoConnectDelay, ref portAvailable))
                    {   // Data available at presently used connection, use this 
                        _GPSStatusNotification_SetData(text: String.Format("GPS Detected at {0} ({1}BPS)", portName, baudRate));
                        return port;
                    }
                    if (!portAvailable)
                        break;
                }
            }

            return null;
        }

        private bool GPS_IsGPSDataPresent(SerialPort port, int pollTimeout, ref bool portAvailable)
        {
            bool result = false;

            // Check OM For access to serial ports
            if (!SerialAccess.GetAccess())
            {
                portAvailable = false;
                return result;
            }

            try
            {
                // Access granted, open port
                port.Open();

                // Release serial access blocking to OM
                SerialAccess.ReleaseAccess();

                StringBuilder receivedData = new StringBuilder();
                bool pollForData = true;
                Stopwatch pollTime = new Stopwatch();
                pollTime.Start();
                while (pollForData)
                {
                    receivedData.Append(port.ReadExisting());
                    if (receivedData.ToString().Contains("GPGGA"))
                    {   // Valid data found
                        result = true;
                        break;
                    }

                    // Stop polling?
                    if (pollTime.ElapsedMilliseconds >= pollTimeout)
                        pollForData = false;
                    Thread.Sleep(1);
                }
                port.Close();
                portAvailable = true;
            }
            catch (Exception ex)
            {   // Log exception
                portAvailable = false;
                //OM.Host.DebugMsg(String.Format("OMGPS2 exeption while checking for GPS data present on port {0}.", port.PortName), ex);
            }
            finally
            {
                if (port.IsOpen)
                    port.Close();
                SerialAccess.ReleaseAccess();
            }
            return result;
        }

        #endregion

        #region GPS Info Panel

        private OMPanel InitializeGPSPanel()
        {
            OMPanel panel = new OMPanel("GPSInfo", "GPS Information", OM.Host.getSkinImage("Icons|Icon-GPS"));

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
                OMBasicShape shpGPSIcon = new OMBasicShape(String.Format("shpGPSIcon_{0}", i), 100, 100, 10, 10,
                    new ShapeData(shapes.Point, Color.Red));
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
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("mnuItem_ShowSettings", OM.Host.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Settings"), "Settings", true, cmdOnClick: "Screen{:S:}.Panel.Goto.OMSettings.OMGPS2_Settings"));
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

        private void UpdateGPSSatView(NMEAProtocol gpsData)
        {
            OM.Host.ForEachScreen((int screen) =>
            {
                OMPanel panel = base.PanelManager[screen, "GPSInfo"];

                if (!panel.IsVisible(screen))
                    return;

                OMImage imgGPSRadar = panel["imgGPSSatRadar"] as OMImage;

                DateTime utc = new DateTime();
                gpsData.GetUTC(ref utc);

                // GPS Info
                OMLabel lblGPSInfo = panel["lblGPSInfo"] as OMLabel;
                lblGPSInfo.Text = "GPS Data:\r\n";
                lblGPSInfo.Text += "\r\n";
                if (gpsData.GPGGA.GPSQuality != GPSQuality.FixNotAvailable)
                {
                    lblGPSInfo.Text += String.Format("Latitude: {0}\r\n", gpsData.GPGGA.Latitude);
                    lblGPSInfo.Text += String.Format("Longitude: {0}\r\n", gpsData.GPGGA.Longitude);
                    lblGPSInfo.Text += String.Format("Altitude: {0} meters\r\n", gpsData.GPGGA.Altitude);
                    lblGPSInfo.Text += "\r\n";
                    lblGPSInfo.Text += String.Format("PDOP: {0} ({1})\r\n", gpsData.GPGSA.PDOP_Rating, gpsData.GPGSA.PDOP);
                    lblGPSInfo.Text += String.Format("HDOP: {0} ({1})\r\n", gpsData.GPGSA.HDOP_Rating, gpsData.GPGSA.HDOP);
                    lblGPSInfo.Text += String.Format("VDOP: {0} ({1})\r\n", gpsData.GPGSA.VDOP_Rating, gpsData.GPGSA.VDOP);
                    lblGPSInfo.Text += String.Format("Precision: {0} meters\r\n", gpsData.GPGSA.Precision);
                }
                else
                {
                    lblGPSInfo.Text += "Waiting for GPS fix...\r\n";
                }
                lblGPSInfo.Text += "\r\n";

                lblGPSInfo.Text += String.Format("UTC: {0}\r\n", utc);
                lblGPSInfo.Text += String.Format("LocalTime: {0}\r\n", utc.ToLocalTime());
                lblGPSInfo.Text += "\r\n";
                lblGPSInfo.Text += String.Format("{0} satellites used of {1} visible\r\n", gpsData.GPGGA.NumberOfSatellitesInUse, gpsData.GPGSV.SatellitesInView);
                lblGPSInfo.Text += "\r\n";
                lblGPSInfo.Text += String.Format("Connected on {0} at {1}bps\r\n", _GPSPortName, _GPSBaudRate);

                // Calculate center point
                Point center = imgGPSRadar.Region.Center;
                double maxRadius = (System.Math.Min(imgGPSRadar.Region.Width, imgGPSRadar.Region.Height) - 5) / 2;

                int i = 0;
                foreach (Satellite sat in gpsData.GPGSV.Satellites.Values)
                {
                    // Get control
                    OMBasicShape shpSat = panel[String.Format("shpGPSIcon_{0}", i)] as OMBasicShape;

                    double h = (double)System.Math.Cos((sat.Elevation * Math.PI) / 180) * maxRadius;
                    int satX = (int)(center.X + h * Math.Sin((sat.Azimuth * Math.PI) / 180));
                    int satY = (int)(center.Y - h * Math.Cos((sat.Azimuth * Math.PI) / 180));

                    shpSat.Left = satX;
                    shpSat.Top = satY;
                    shpSat.Visible = true;

                    ShapeData shpDta = shpSat.ShapeData;
                    if (sat.Used)
                        shpDta.FillColor = Color.Green;
                    else
                        shpDta.FillColor = Color.Red;

                    shpSat.ShapeData = shpDta;
                    i++;
                }

                for (int i2 = i; i2 < 32; i2++)
                {
                    OMBasicShape shpSat = panel[String.Format("shpGPSIcon_{0}", i)] as OMBasicShape;
                    shpSat.Visible = false;
                }


            });
        }

        #endregion

        #region DataSources

        private void DataSources_Register()
        {
            //create dataSources
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "MPH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS MPH", 0));
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "KMH", OpenMobile.Data.DataSource.DataTypes.raw, "GPS KMH"), 0);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Speed", "", OpenMobile.Data.DataSource.DataTypes.raw, "GPS speed in system default format"), OpenMobile.Framework.Globalization.convertSpeedToLocal(0, true));
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Visible", OpenMobile.Data.DataSource.DataTypes.raw, "GPS Visible satelite count"), 0);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "InUse", OpenMobile.Data.DataSource.DataTypes.raw, "GPS Used satelite count"), 0);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sat", "Fix", OpenMobile.Data.DataSource.DataTypes.binary, "GPS Satelite fix state"), false);
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(this.pluginName, "GPS", "Sentence", "String", OpenMobile.Data.DataSource.DataTypes.raw, "GPS string of data"), "");
        }

        private void DataSources_Update(NMEAProtocol gpsData)
        {
            UpdateGPSSatView(gpsData);

            // Set local system time?
            if (_GPSSetLocalSystemTime && gpsData.GPGGA.GPSQuality != GPSQuality.FixNotAvailable && !_GPSLocalSystemTimeSet)
            {
                DateTime utc = new DateTime();
                if (gpsData.GetUTC(ref utc))
                {
                    OpenMobile.helperFunctions.General.SetUTCTime(utc);
                    _GPSLocalSystemTimeSet = true;
                }
                else
                {
                    _GPSLocalSystemTimeSet = false;
                }
            }

            if (gpsData.GPGGA.GPSQuality != GPSQuality.FixNotAvailable)
            {   // Push fixed data


                float altitude = (float)gpsData.GPGGA.Altitude;
                float latitude = (float)gpsData.GPGGA.Latitude;
                float longitude = (float)gpsData.GPGGA.Longitude;
                if ((OM.Host.CurrentLocation.Altitude != altitude) || (OM.Host.CurrentLocation.Latitude != latitude) || OM.Host.CurrentLocation.Longitude != longitude)
                {
                    //OM.Host.CurrentLocation = new OpenMobile.Location(latitude, longitude) { Altitude = altitude };
                    OM.Host.CurrentLocation.Latitude = latitude;
                    OM.Host.CurrentLocation.Longitude = longitude;
                }

                if (_GPSData.GPRMC.GroundSpeed > 2)
                {
                    OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.MPH", Math.Round(_GPSData.GPRMC.GroundSpeed * 1.15, 1));
                    OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.KMH", Math.Round(_GPSData.GPRMC.GroundSpeed * 1.852, 1));
                    OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed", OpenMobile.Framework.Globalization.convertSpeedToLocal(_GPSData.GPRMC.GroundSpeed * 1.852, true));
                }
                // Show gps notification status
                _GPSStatusNotification_SetData(text: String.Format("GPS Fixed ({0}sats) [{1}:{2}]", gpsData.GPGGA.NumberOfSatellitesInUse, latitude, longitude),
                iconStatusBar: _IconGPSFixed.image, updatePanelText:false);
            }
            else
            {
                // Show gps notification status
                _GPSStatusNotification_SetData(text: String.Format("{0} satellites visible. Waiting for gps fix...", gpsData.GPGSV.SatellitesInView)
                    , updatePanelText: false);

                // "Animate" searching icon to let the user know we're still wating for fix
                if (_GPSStatusNotification.IconStatusBar == _IconGPSNotFixed.image)
                    _GPSStatusNotification.IconStatusBar = _IconGPSSearching.image;
                else
                    _GPSStatusNotification.IconStatusBar = _IconGPSNotFixed.image;

                // Set datasources
                OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.MPH", 0);
                OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed.KMH", 0);
                OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Speed", OpenMobile.Framework.Globalization.convertSpeedToLocal(0, true));
            }

            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sentence.String", _GPSRawString);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.Fix", gpsData.GPGGA.GPSQuality != GPSQuality.FixNotAvailable);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.Visible", gpsData.GPGSV.SatellitesInView);
            OM.Host.DataHandler.PushDataSourceValue(this.pluginName, "GPS.Sat.InUse", gpsData.GPGGA.NumberOfSatellitesInUse);
        }

        #endregion


    }
}
