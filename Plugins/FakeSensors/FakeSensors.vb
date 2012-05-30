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

'    Copyright 2011-2012 Jonathan Heizer jheizer@gmail.com
#End Region

Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports OpenMobile.helperFunctions.Sensors

Public Class FakeSensors
    Implements OpenMobile.Plugin.IRawHardware

    Private m_Sensors As Generic.List(Of SensorWrapper)
    Private WithEvents m_Tmr As System.Timers.Timer
    Private m_Rnd As Random

    Private Enum PIDs As Integer
        Fix = 1
        Longitude
        Latitude
        NumberOfSats
        Altitude
        Bearing
        Speed
        ZipCode
        City
        State
        OutsideTemp
        InsideTemp
        UnreadEmails
        UnreadIMs
        Facebook
        Twitter
    End Enum

    Public ReadOnly Property authorEmail() As String Implements IBasePlugin.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName() As String Implements IBasePlugin.authorName
        Get
            Return "Jon Heizer"
        End Get
    End Property

    Public ReadOnly Property deviceInfo() As String Implements IRawHardware.deviceInfo
        Get
            Return "Fake Sensors"
        End Get
    End Property

    Public Sub Dispose() Implements IBasePlugin.Dispose

    End Sub

    Public ReadOnly Property firmwareVersion() As String Implements IRawHardware.firmwareVersion
        Get
            Return 1
        End Get
    End Property

    Public Function getAvailableSensors(ByVal type As OpenMobile.Plugin.eSensorType) As System.Collections.Generic.List(Of OpenMobile.Plugin.Sensor) Implements IRawHardware.getAvailableSensors
        Return sensorWrappersToSensors(m_Sensors)
    End Function


    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements IBasePlugin.initialize
        m_Rnd = New Random(DateTime.Now.Millisecond)
        m_Tmr = New System.Timers.Timer(500)
        AddHandler m_Tmr.Elapsed, AddressOf UpdateSensors
        m_Tmr.Enabled = True

        m_Sensors = New Generic.List(Of SensorWrapper)

        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Fix.ToString, "Fix", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Longitude.ToString, "Lng", eSensorDataType.degrees, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Latitude.ToString, "Lat", eSensorDataType.degrees, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Facebook.ToString, "FB", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.NumberOfSats.ToString, "Sats", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Altitude.ToString, "Alt", eSensorDataType.meters, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Bearing.ToString, "Dir", eSensorDataType.degrees, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Speed.ToString, "Spd", eSensorDataType.kph, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.InsideTemp.ToString, "In", eSensorDataType.degreesC, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.OutsideTemp.ToString, "Out", eSensorDataType.degreesC, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.Twitter.ToString, "Twtr", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.UnreadEmails.ToString, "Email", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.UnreadIMs.ToString, "IMs", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.ZipCode.ToString, "Zip", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.City.ToString, "City", eSensorDataType.raw, Nothing))
        m_Sensors.Add(New SensorWrapper(Me.pluginName & PIDs.State.ToString, "St", eSensorDataType.raw, Nothing))

    End Function

    Private Sub UpdateSensors(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
        For Each sen As SensorWrapper In m_Sensors
            sen.UpdateSensorValue(m_Rnd.Next.ToString)
        Next
    End Sub


    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements IBasePlugin.loadSettings
        Return Nothing
    End Function

    Public ReadOnly Property pluginDescription() As String Implements IBasePlugin.pluginDescription
        Get
            Return "Fake Sensors"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements IBasePlugin.pluginName
        Get
            Return "FakeSensors"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public Sub resetDevice() Implements IRawHardware.resetDevice

    End Sub


End Class
