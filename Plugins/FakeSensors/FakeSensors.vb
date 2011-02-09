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
    Inherits OpenMobile.Plugin.IRawHardware

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

    Public Overrides ReadOnly Property authorEmail() As String
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public Overrides ReadOnly Property authorName() As String
        Get
            Return "Jon Heizer"
        End Get
    End Property

    Public Overrides ReadOnly Property deviceInfo() As String
        Get
            Return "Fake Sensors"
        End Get
    End Property

    Public Overrides Sub Dispose()

    End Sub

    Public Overrides ReadOnly Property firmwareVersion() As String
        Get
            Return 1
        End Get
    End Property

    Public Overrides Function getAvailableSensors(ByVal type As OpenMobile.Plugin.eSensorType) As System.Collections.Generic.List(Of OpenMobile.Plugin.Sensor)
        If m_Sensors Is Nothing Then
            m_Sensors = New Generic.List(Of Sensor)
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Fix.ToString, MyBase.MaskPID(PIDs.Fix), eSensorType.deviceSuppliesData, "Fix", eSensorDataType.binary))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Longitude.ToString, MyBase.MaskPID(PIDs.Longitude), eSensorType.deviceSuppliesData, "Lng", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Latitude.ToString, MyBase.MaskPID(PIDs.Latitude), eSensorType.deviceSuppliesData, "Lat", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Facebook.ToString, MyBase.MaskPID(PIDs.Facebook), eSensorType.deviceSuppliesData, "FB", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.NumberOfSats.ToString, MyBase.MaskPID(PIDs.NumberOfSats), eSensorType.deviceSuppliesData, "Sats", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Altitude.ToString, MyBase.MaskPID(PIDs.Altitude), eSensorType.deviceSuppliesData, "Alt", eSensorDataType.meters))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Bearing.ToString, MyBase.MaskPID(PIDs.Bearing), eSensorType.deviceSuppliesData, "Dir", eSensorDataType.degrees))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Speed.ToString, MyBase.MaskPID(PIDs.Speed), eSensorType.deviceSuppliesData, "Spd", eSensorDataType.kph))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.InsideTemp.ToString, MyBase.MaskPID(PIDs.InsideTemp), eSensorType.deviceSuppliesData, "In", eSensorDataType.degreesC))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.OutsideTemp.ToString, MyBase.MaskPID(PIDs.OutsideTemp), eSensorType.deviceSuppliesData, "Out", eSensorDataType.degreesC))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.Twitter.ToString, MyBase.MaskPID(PIDs.Twitter), eSensorType.deviceSuppliesData, "Twtr", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.UnreadEmails.ToString, MyBase.MaskPID(PIDs.UnreadEmails), eSensorType.deviceSuppliesData, "Email", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.UnreadIMs.ToString, MyBase.MaskPID(PIDs.UnreadIMs), eSensorType.deviceSuppliesData, "IMs", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.ZipCode.ToString, MyBase.MaskPID(PIDs.ZipCode), eSensorType.deviceSuppliesData, "Zip", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.City.ToString, MyBase.MaskPID(PIDs.City), eSensorType.deviceSuppliesData, "City", eSensorDataType.raw))
            m_Sensors.Add(New Sensor("FakeSensors." & PIDs.State.ToString, MyBase.MaskPID(PIDs.State), eSensorType.deviceSuppliesData, "St", eSensorDataType.raw))
        End If

        Return m_Sensors
    End Function

    Public Overrides Function getValue(ByVal PID As Integer) As Object
        If MyBase.UnmaskPID(PID) = PIDs.Longitude Then
            Return "1234.5678"
        End If
        If MyBase.UnmaskPID(PID) = PIDs.Fix Then
            Return m_Rnd.Next(0, 2).ToString
        End If

        Return m_Rnd.Next(0, 100).ToString
    End Function

    Public Overloads Overrides Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean

    End Function

    Public Overloads Overrides Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean

    End Function

    Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus
        InitPIDMask()
        m_Rnd = New Random(DateTime.Now.Millisecond)
    End Function

    Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

    End Function

    Public Overrides ReadOnly Property pluginDescription() As String
        Get
            Return "Fake Sensors"
        End Get
    End Property

    Public Overrides ReadOnly Property pluginName() As String
        Get
            Return "FakeSensors"
        End Get
    End Property

    Public Overrides ReadOnly Property pluginVersion() As Single
        Get
            Return 1
        End Get
    End Property

    Public Overrides Sub resetDevice()

    End Sub

    Public Overrides Function setValue(ByVal PID As Integer, ByVal value As Object) As Boolean

    End Function
End Class
