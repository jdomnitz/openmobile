#Region "GPL"
'    This file is part of OpenMobile.

'    OpenMobile is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.

'    OpenMobile is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.

'    You should have received a copy of the GNU General Public License
'    along with OpenMobile.  If not, see <http://www.gnu.org/licenses/>.

'    Copyright 2010 Jonathan Heizer jheizer@gmail.com
#End Region

Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports OpenMobile.Threading
Imports System.Data
Imports Mono.Data.Sqlite

Public Class OMSerialGPS
    Implements IRawHardware

    Private Enum PIDs As Integer
        Fix = 1
        Longitude = 2
        Latitude = 3
        FixQuality = 4
        NumberOfSats = 5
        Altitude = 6
        Bearing = 7
        Speed = 8
        HorzDilution = 9
        PDOP = 10
        HDOP = 11
        VDOP = 12
        SatList = 13
        ZipCode = 20
        City = 21
        StateCode = 22
        State = 23
    End Enum

    Private WithEvents m_Host As IPluginHost
    Private m_ComPort As String = "AUTO"
    Private WithEvents m_Settings As Settings
    Private m_Sensors As Generic.List(Of Sensor)

    Private WithEvents m_GPS As SerialGPS.NMEAParser

    Private m_Icon As OpenMobile.Graphics.OImage
    Private m_IconShowing As Boolean = False

    Private m_Conn As Mono.Data.Sqlite.SqliteConnection

    Private m_GPGGA As New SerialGPS.GPGGA
    Private m_GPGLL As New SerialGPS.GPGLL
    Private m_GPGSA As New SerialGPS.GPGSA
    Private m_GPGSV As New SerialGPS.GPGSV
    Private m_GPRMC As New SerialGPS.GPRMC
    Private m_Longitude As Double
    Private m_Latitude As Double

    Private m_LastLocationUpdate As DateTime = DateTime.MinValue
    Private m_Zip As String
    Private m_City As String
    Private m_StateCode As String
    Private m_State As String


    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements IBasePlugin.initialize
        m_Host = host
        LoadGPSSettings()

        SafeThread.Asynchronous(AddressOf BackgroundInit, m_Host)
        Return eLoadStatus.LoadSuccessful
    End Function

    Private Sub BackgroundInit()
        Try
            AutoDetectGPS()

            m_GPS = New SerialGPS.NMEAParser(m_ComPort)
            If Not m_GPS.StartGPS() Then
                m_ComPort = "AUTO"
                AutoDetectGPS()
                m_GPS = New SerialGPS.NMEAParser(m_ComPort)
                m_GPS.StartGPS()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub AutoDetectGPS()
        If m_ComPort = "AUTO" Then
            m_ComPort = SerialGPS.ScanPorts.ScanPorts

            If Not String.IsNullOrEmpty(m_ComPort) Then
                Dim Settings As New OpenMobile.Data.PluginSettings
                Settings.setSetting("OMSerialGPS.ComPort", m_ComPort)
                Settings.Dispose()
            End If
        End If
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements IBasePlugin.loadSettings
        If m_Settings Is Nothing Then
            LoadGPSSettings()

            m_Settings = New Settings("GPS Receiver")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.Add("AUTO")
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            Dim COMValues As New Generic.List(Of String)
            COMValues.Add("AUTO")
            COMValues.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMSerialGPS.ComPort", "COM", "Com Port", COMOptions, COMValues, m_ComPort.ToString))
            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)
        Dim PlugSet As New OpenMobile.Data.PluginSettings
        PlugSet.setSetting(St.Name, St.Value)
    End Sub

    Private Sub LoadGPSSettings()
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting("OMSerialGPS.ComPort")) Then
                SetDefaultSettings()
            End If
            m_ComPort = Settings.getSetting("OMSerialGPS.ComPort")
        End Using
    End Sub

    Private Sub SetDefaultSettings()
        Dim Settings As New OpenMobile.Data.PluginSettings
        Settings.setSetting("OMSerialGPS.ComPort", "AUTO")
        Settings.Dispose()
    End Sub

    Public Function getAvailableSensors(ByVal type As OpenMobile.Plugin.eSensorType) As System.Collections.Generic.List(Of OpenMobile.Plugin.Sensor) Implements IRawHardware.getAvailableSensors
        If Not m_Sensors Is Nothing Then
            Return m_Sensors
        End If

        m_Sensors = New Generic.List(Of Sensor)

        'If Not m_GPS.GPSConnected Then
        '    Return m_Sensors
        'End If

        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Fix.ToString, eSensorType.deviceSuppliesData, "Fix", eSensorDataType.binary))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Longitude.ToString, eSensorType.deviceSuppliesData, "Lng", eSensorDataType.degrees))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Latitude.ToString, eSensorType.deviceSuppliesData, "Lat", eSensorDataType.degrees))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.FixQuality.ToString, eSensorType.deviceSuppliesData, "Qual", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.NumberOfSats.ToString, eSensorType.deviceSuppliesData, "Sats", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Altitude.ToString, eSensorType.deviceSuppliesData, "Alt", eSensorDataType.meters))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Bearing.ToString, eSensorType.deviceSuppliesData, "Dir", eSensorDataType.degrees))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.Speed.ToString, eSensorType.deviceSuppliesData, "Spd", eSensorDataType.kph))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.HorzDilution.ToString, eSensorType.deviceSuppliesData, "Horz", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.PDOP.ToString, eSensorType.deviceSuppliesData, "PDOP", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.HDOP.ToString, eSensorType.deviceSuppliesData, "HDOP", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.VDOP.ToString, eSensorType.deviceSuppliesData, "VDOP", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.SatList.ToString, eSensorType.deviceSuppliesData, "Lst", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.ZipCode.ToString, eSensorType.deviceSuppliesData, "Zip", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.City.ToString, eSensorType.deviceSuppliesData, "City", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.StateCode.ToString, eSensorType.deviceSuppliesData, "St", eSensorDataType.raw))
        m_Sensors.Add(New Sensor("OMSerialGPS." & PIDs.State.ToString, eSensorType.deviceSuppliesData, "St", eSensorDataType.raw))

        Return m_Sensors
    End Function

    Public Function getValue(ByVal Name As String) As Object Implements IRawHardware.getValue
        Return ""
        Select Case Name.Substring(12) 'Cut off the OMSerialGPS. 
            Case Is = PIDs.Fix.ToString
                Return m_GPRMC.SatFix

            Case Is = PIDs.Longitude.ToString
                Return m_Longitude

            Case Is = PIDs.Latitude.ToString
                Return m_Latitude

            Case Is = PIDs.FixQuality.ToString
                Return m_GPGGA.FixQuality

            Case Is = PIDs.NumberOfSats.ToString
                Return m_GPGGA.NumberOfSats

            Case Is = PIDs.Altitude.ToString
                Return m_GPGGA.Altitude

            Case Is = PIDs.Bearing.ToString
                Return m_GPRMC.Bearing

            Case Is = PIDs.Speed.ToString
                Return OpenMobile.Framework.Math.Calculation.convertSpeed(m_GPRMC.Speed, Framework.Math.speedTypes.knots, Framework.Math.speedTypes.kilometersPerHour)

            Case Is = PIDs.HorzDilution.ToString
                Return m_GPGGA.HorzDilution

            Case Is = PIDs.PDOP.ToString
                Return m_GPGSA.PDOP

            Case Is = PIDs.HDOP.ToString
                Return m_GPGSA.HDOP

            Case Is = PIDs.VDOP.ToString
                Return m_GPGSA.VDOP

            Case Is = PIDs.SatList
                Return "<Sats>Not Implemented</Sats>"

                'Zip DB
            Case Is = PIDs.ZipCode.ToString
                Return m_Zip

            Case Is = PIDs.City.ToString
                Return m_City

            Case Is = PIDs.StateCode.ToString
                Return m_StateCode

            Case Is = PIDs.State.ToString
                Return m_State

        End Select
        Return ""
    End Function

    Private Sub ShowIcon()
        Dim Icon As New IconManager.UIIcon(m_Host.getSkinImage("gps"), ePriority.MediumHigh, True, "OMSerialGPS")
        m_Host.sendMessage("UI", "OMSerialGPS", "AddIcon", Icon)
    End Sub

    Private Sub RemoveIcon()
        Dim Icon As New IconManager.UIIcon(m_Host.getSkinImage("gps"), ePriority.MediumHigh, True, "OMSerialGPS")
        m_Host.sendMessage("UI", "OMSerialGPS", "RemoveIcon", Icon)
    End Sub

    Private Sub RefreshLocation(ByVal Latitude As Double, ByVal Longitude As Double)

        m_Longitude = Longitude
        m_Latitude = Latitude

        'Restrict this to once a minute.  Not changing often so give the DB a break.
        Try
            If m_IconShowing Then
                If DateTime.Now.Subtract(m_LastLocationUpdate).TotalMinutes > 1 Then
                    If m_Conn Is Nothing Then
                        m_Conn = New SqliteConnection("Data Source=" & OpenMobile.Path.Combine(m_Host.PluginPath, "zips.sqlite") & ";Pooling=false;synchronous=0;")
                    End If

                    If Not m_Conn.State = ConnectionState.Open Then
                        m_Conn.Open()
                    End If

                    Dim Rdr As SqliteDataReader

                    Dim Cmd As SqliteCommand = m_Conn.CreateCommand
                    Cmd.CommandText = "SELECT zip, state, city, full_state FROM zip_codes ORDER BY ((" & m_Latitude & "-latitude) * (" & m_Latitude & "-latitude))+((" & m_Longitude & "-longitude) * (" & m_Longitude & "-longitude)) LIMIT 1"
                    Rdr = Cmd.ExecuteReader()

                    Rdr.Read()

                    m_Zip = Rdr(0).ToString
                    m_State = Rdr(1).ToString
                    m_City = Rdr(2).ToString
                    m_StateCode = Rdr(3).ToString
                    m_LastLocationUpdate = DateTime.Now

                    Rdr.Close()
                End If
            End If

            Dim Loc As Location = m_Host.CurrentLocation
            If Loc Is Nothing Then
                Loc = New Location
            End If
            Loc.Latitude = m_Latitude
            Loc.Longitude = m_Longitude
            Loc.City = m_City
            Loc.Zip = m_Zip
            Loc.State = m_State
            m_Host.CurrentLocation = Loc

        Catch ex As Exception
        End Try
    End Sub

    Public Sub resetDevice() Implements IRawHardware.resetDevice

    End Sub

    Public Function setValue(ByVal Name As String, ByVal value As Object) As Boolean Implements IRawHardware.setValue
        Return False
    End Function

    Public ReadOnly Property authorEmail() As String Implements IRawHardware.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName() As String Implements IRawHardware.authorName
        Get
            Return "Jon Heizer"
        End Get
    End Property

    Public ReadOnly Property deviceInfo() As String Implements IRawHardware.deviceInfo
        Get
            Return "Serial GPS"
        End Get
    End Property

    Public Sub Dispose() Implements IBasePlugin.Dispose
        Try
            If Not m_Conn.State = ConnectionState.Closed Then
                m_Conn.Clone()
                m_Conn.Dispose()
            End If
            m_GPS.StopGPS()
            m_GPS.Dispose()
        Catch ex As Exception
        End Try
    End Sub

    Public ReadOnly Property firmwareVersion() As String Implements IRawHardware.firmwareVersion
        Get
            Return 1
        End Get
    End Property


    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements IBasePlugin.incomingMessage

    End Function


    Public ReadOnly Property pluginDescription() As String Implements IBasePlugin.pluginDescription
        Get
            Return "OMSerialGPS"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements IBasePlugin.pluginName
        Get
            Return "OMSerialGPS"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Private Sub m_GPS_NewGPGGA(ByVal Data As SerialGPS.GPGGA) Handles m_GPS.NewGPGGA
        m_GPGGA = Data
        RefreshLocation(m_GPGGA.Latitude, m_GPGGA.Longitude)
    End Sub

    Private Sub m_GPS_NewGPGLL(ByVal Data As SerialGPS.GPGLL) Handles m_GPS.NewGPGLL
        m_GPGLL = Data
        RefreshLocation(m_GPGLL.Latitude, m_GPGLL.Longitude)
    End Sub

    Private Sub m_GPS_NewGPGSA(ByVal Data As SerialGPS.GPGSA) Handles m_GPS.NewGPGSA
        m_GPGSA = Data
    End Sub

    Private Sub m_GPS_NewGPGSV(ByVal Data As SerialGPS.GPGSV) Handles m_GPS.NewGPGSV
        m_GPGSV = Data
    End Sub

    Private Sub m_GPS_NewGPRMC(ByVal Data As SerialGPS.GPRMC) Handles m_GPS.NewGPRMC
        m_GPRMC = Data
        RefreshLocation(m_GPRMC.Latitude, m_GPRMC.Longitude)

        If m_GPRMC.SatFix AndAlso Not m_IconShowing Then
            ShowIcon()
            m_IconShowing = True
        ElseIf Not m_GPRMC.SatFix AndAlso m_IconShowing Then
            RemoveIcon()
            m_IconShowing = False
        End If
    End Sub

    'Private Sub m_Host_OnSystemEvent(ByVal fun As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
    '    If fun = eFunction.closeProgram Then
    '        m_Host.sendMessage("OMDebug", "OMVisteonRadio", "closeProgram")

    '        m_GPS.StopGPS()
    '    End If
    'End Sub
End Class
