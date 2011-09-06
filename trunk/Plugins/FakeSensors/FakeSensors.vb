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

'    Copyright 2011 Jonathan Heizer jheizer@gmail.com
#End Region

Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin

Public Class FakeSensors
    Implements OpenMobile.Plugin.IRawHardware

    Private m_Sensors As Generic.List(Of Sensor)
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
        If m_Sensors Is Nothing Then
            m_Sensors = New Generic.List(Of Sensor)
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Fix.ToString, eSensorType.deviceSuppliesData, "Fix", eSensorDataType.binary))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Longitude.ToString, eSensorType.deviceSuppliesData, "Lng", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Latitude.ToString, eSensorType.deviceSuppliesData, "Lat", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Facebook.ToString, eSensorType.deviceSuppliesData, "FB", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.NumberOfSats.ToString, eSensorType.deviceSuppliesData, "Sats", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Altitude.ToString, eSensorType.deviceSuppliesData, "Alt", eSensorDataType.meters))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Bearing.ToString, eSensorType.deviceSuppliesData, "Dir", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Speed.ToString, eSensorType.deviceSuppliesData, "Spd", eSensorDataType.kph))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.InsideTemp.ToString, eSensorType.deviceSuppliesData, "In", eSensorDataType.degreesC))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.OutsideTemp.ToString, eSensorType.deviceSuppliesData, "Out", eSensorDataType.degreesC))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Twitter.ToString, eSensorType.deviceSuppliesData, "Twtr", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.UnreadEmails.ToString, eSensorType.deviceSuppliesData, "Email", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.UnreadIMs.ToString, eSensorType.deviceSuppliesData, "IMs", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.ZipCode.ToString, eSensorType.deviceSuppliesData, "Zip", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.City.ToString, eSensorType.deviceSuppliesData, "City", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.State.ToString, eSensorType.deviceSuppliesData, "St", eSensorDataType.raw))
        End If

        Return m_Sensors
    End Function

    Public Function getValue(ByVal Name As String) As Object Implements IRawHardware.getValue
        Select Name.Substring(12)
            Case PIDs.Longitude.ToString
                Return "1234.5678"
            Case Is = PIDs.Fix.ToString
                Return m_Rnd.Next(0, 2).ToString
            Case Else
                Return m_Rnd.Next(0, 100).ToString
        End Select
    End Function

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements IBasePlugin.initialize
        m_Rnd = New Random(DateTime.Now.Millisecond)
    End Function

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

    Public Function setValue(ByVal name As String, ByVal value As Object) As Boolean Implements OpenMobile.Plugin.IRawHardware.setValue
        Return True
    End Function
End Class
