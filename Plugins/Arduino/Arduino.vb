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

'    Copyright 2010-2012 Jonathan Heizer jheizer@gmail.com
#End Region

Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports OpenMobile.Threading
Imports System.Data

Public Class Ardunio
    Implements IRawHardware

    Private WithEvents m_Host As IPluginHost
    Private m_ComPort As String = "AUTO"
    Private WithEvents m_Settings As Settings
    Private m_Sensors As Generic.List(Of SensorWrapper)

    Private m_Eval = New OpenMobile.Framework.Math.Calculation

    Private WithEvents m_Arduino As FirmataDotNet.FirmataDotNet

    Private m_VRef As Decimal = 5

    Private m_Analog As New Generic.Dictionary(Of Integer, SensorWrapper)
    Private m_Digital As New Generic.Dictionary(Of Integer, SensorWrapper)

    Private m_AnalogShortName As New Generic.Dictionary(Of Integer, String)
    Private m_DigitalShortName As New Generic.Dictionary(Of Integer, String)

    Private m_AnalogType As New Generic.Dictionary(Of Integer, Integer)
    Private m_DigitalType As New Generic.Dictionary(Of Integer, Integer)

    Private m_AnalogFormula As New Generic.Dictionary(Of Integer, String)
    Private m_DigitalFormula As New Generic.Dictionary(Of Integer, String)

    Private m_AnalogPinCount As Integer = 6
    Private m_DigitalPinCount As Integer = 14

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host
        LoadSavedSettings()

        SafeThread.Asynchronous(AddressOf BackgroundInit, m_Host)
        Return eLoadStatus.LoadSuccessful
    End Function

    Private Sub BackgroundInit()
        Try
            m_Arduino = New FirmataDotNet.FirmataDotNet
            m_Arduino.SetPortName(m_ComPort)
            m_Arduino.Initalize()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub m_Arduino_InitComplete() Handles m_Arduino.InitComplete
        m_Arduino.ToggleAnalogPinReporting(0, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleAnalogPinReporting(1, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleAnalogPinReporting(2, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleAnalogPinReporting(3, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleAnalogPinReporting(4, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleAnalogPinReporting(5, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)

        m_Arduino.ToggleDigitalPortReporting(0, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
        m_Arduino.ToggleDigitalPortReporting(1, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)

        For i As Integer = 2 To 15
            m_Arduino.WriteData(244, i, 1)
        Next
        m_Arduino.WriteData(244, 4, 0)
        'm_Arduino.DigitalWrite(6, FirmataDotNet.FirmataDotNet.OnOff.TurnOn)
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings
        If m_Settings Is Nothing Then
            m_Settings = New Settings("Arduino")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "Arduino.ComPort", "COM", "Com Port", COMOptions, COMOptions, m_ComPort.ToString))

            m_Settings.Add(New Setting(SettingTypes.Text, "Arduino.VRef", "VRef", "VRef", m_VRef))

            Dim DataDirection As New Generic.List(Of String)
            DataDirection.Add("Read Pin")
            DataDirection.Add("Set Pin")

            Dim DataDirectionVal As New Generic.List(Of String)
            DataDirectionVal.Add("0")
            DataDirectionVal.Add("1")

            Dim TypeNames As New Generic.List(Of String)
            TypeNames.AddRange([Enum].GetNames(GetType(eSensorDataType)))
            Dim TypeValues As New Generic.List(Of String)
            For Each TP As eSensorDataType In [Enum].GetValues(GetType(eSensorDataType))
                TypeValues.Add(CInt(TP).ToString)
            Next

            For i As Integer = 1 To m_AnalogPinCount
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "Arduino.A" & i & ".Dir", "A" & i & " Dir", "Data Direction", DataDirection, DataDirectionVal, "0"))
                m_Settings.Add(New Setting(SettingTypes.Text, "Arduino.A" & i & ".ShortName", "A" & i & " Name", "Short Name", m_AnalogShortName(i)))
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "Arduino.A" & i & ".Type", "A" & i & " Type", "Data Type", TypeNames, TypeValues, m_AnalogType(i)))
                m_Settings.Add(New Setting(SettingTypes.Text, "Arduino.A" & i & ".Formula", "A" & i & " Formula", "Formula", m_AnalogFormula(i)))
            Next

            For i As Integer = 1 To m_DigitalPinCount
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "Arduino.D" & i & ".Dir", "Dir", "Data Direction", DataDirection, DataDirectionVal, "0"))
                m_Settings.Add(New Setting(SettingTypes.Text, "Arduino.D" & i & ".ShortName", "Name", "Short Name", m_DigitalShortName(i)))
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "Arduino.D" & i & ".Type", "Type", "Data Type", TypeNames, TypeValues, m_DigitalType(i)))
                m_Settings.Add(New Setting(SettingTypes.Text, "Arduino.D" & i & ".Formula", "", "Formula", m_DigitalFormula(i)))
            Next

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)
        Dim PlugSet As New OpenMobile.Data.PluginSettings
        PlugSet.setSetting(St.Name, St.Value)
    End Sub

    Private Sub LoadSavedSettings()
        m_ComPort = GetSetting("Arduino.ComPort", "")
        m_VRef = GetSetting("Arduino.VRef", 5)

        For i As Integer = 1 To m_AnalogPinCount
            m_AnalogShortName.Add(i, GetSetting("Arduino.A" & i & ".ShortName", ""))
            m_AnalogType.Add(i, GetSetting("Arduino.A" & i & ".Type", 0))
            m_AnalogFormula.Add(i, GetSetting("Arduino.A" & i & ".Formula", ""))
        Next

        For i As Integer = 1 To m_DigitalPinCount
            m_DigitalShortName.Add(i, GetSetting("Arduino.D" & i & ".ShortName", ""))
            m_DigitalType.Add(i, GetSetting("Arduino.D" & i & ".Type", 0))
            m_DigitalFormula.Add(i, GetSetting("Arduino.D" & i & ".Forumla", ""))
        Next
    End Sub

    Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting(Name)) Then
                Settings.setSetting(Name, DefaultValue)
            End If
            Return Settings.getSetting(Name)
        End Using
    End Function


    Public Function getAvailableSensors(ByVal type As OpenMobile.Plugin.eSensorType) As System.Collections.Generic.List(Of OpenMobile.Plugin.Sensor) Implements OpenMobile.Plugin.ISensorProvider.getAvailableSensors
        If Not m_Sensors Is Nothing Then
            Return helperFunctions.Sensors.sensorWrappersToSensors(m_Sensors)
        End If

        m_Sensors = New Generic.List(Of SensorWrapper)

        If Not m_Arduino.IsConnected Then
            Return helperFunctions.Sensors.sensorWrappersToSensors(m_Sensors)
        End If

        SyncLock m_Sensors
            'For now just uno 6 analog, 14 digital (skip first 2)
            For i As Integer = 1 To m_AnalogPinCount
                m_Sensors.Add(New SensorWrapper("Arduino.A" & i, m_AnalogShortName(i), m_AnalogType(i), Nothing))
                m_Analog.Add(i, m_Sensors.Last)
            Next

            For i As Integer = 3 To m_DigitalPinCount
                m_Sensors.Add(New SensorWrapper("Arduino.D" & i + 20, m_DigitalShortName(i), m_DigitalType(i), Nothing))
                m_Digital.Add(i, m_Sensors.Last)
            Next
        End SyncLock

        Return helperFunctions.Sensors.sensorWrappersToSensors(m_Sensors)
    End Function

    Private Sub m_Arduino_AnalogValueReceived(ByVal Pin As Integer, ByVal Value As Integer) Handles m_Arduino.AnalogValueReceived
        Try
            UpdateValue(Pin, Value)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub m_Arduino_DigitalValueReceived(ByVal Pin As Integer, ByVal Value As Integer) Handles m_Arduino.DigitalValueReceived
        Try
            'UpdateValue(Pin, Value)
        Catch ex As Exception
        End Try
    End Sub

    Public Sub UpdateValue(ByVal Pin As Integer, ByVal Value As Integer)
        Dim Ret As Decimal = 0
        Dim Form As String
        If Pin < 20 Then

            Ret = Value
            Form = m_AnalogFormula(Pin)

            If Not String.IsNullOrEmpty(Form) Then
                If Form.StartsWith("=") Then
                    Ret = m_Eval.evaluateFormula(m_AnalogFormula(Pin).Replace("Value", Value).Replace("VRef", m_VRef))
                Else
                    Select Case Form
                        Case Is = "lm35"
                            Ret = m_Eval.evaluateFormula(m_VRef * Value * 100 / 1024)

                        Case Is = "volts"
                            Ret = m_Eval.evaluateFormula(m_VRef * Value / 1024)

                    End Select
                End If
            End If

            m_Analog(Pin).UpdateSensorValue(Ret)
        Else
            m_Digital(Pin).UpdateSensorValue(Ret)

        End If
    End Sub

    Public Sub resetDevice() Implements OpenMobile.Plugin.IRawHardware.resetDevice

    End Sub
    
    Public ReadOnly Property authorEmail As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName As String Implements OpenMobile.Plugin.IBasePlugin.authorName
        Get
            Return "Jon Heizer"
        End Get
    End Property

    Public ReadOnly Property deviceInfo As String Implements OpenMobile.Plugin.IRawHardware.deviceInfo
        Get
            Return "Adruino"
        End Get
    End Property

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            m_Arduino.CloseConnection()
        Catch ex As Exception
        End Try
    End Sub

    Public ReadOnly Property firmwareVersion As String Implements OpenMobile.Plugin.IRawHardware.firmwareVersion
        Get
            Return 1
        End Get
    End Property


    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function



    Public ReadOnly Property pluginDescription As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "Arduino"
        End Get
    End Property

    Public ReadOnly Property pluginName As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "Arduino"
        End Get
    End Property

    Public ReadOnly Property pluginVersion As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property


    'Private Sub m_Host_OnSystemEvent(ByVal fun As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
    '    If fun = eFunction.closeProgram Then
    '        m_Host.sendMessage("OMDebug", "OMVisteonRadio", "closeProgram")

    '        m_GPS.StopGPS()
    '    End If
    'End Sub


End Class
