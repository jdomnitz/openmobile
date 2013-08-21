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
Imports System.Text.RegularExpressions

Public Class CharacterDisplay
    Implements IOther

    Private WithEvents m_Host As IPluginHost
    Private m_ComPort As String
    Private WithEvents m_LCD As CharacterLCD.BaseCharacterLCD
    Private m_CycleDelay As Integer = 3
    Private m_BackRed As Integer = 255
    Private m_BackGreen As Integer = 255
    Private m_BackBlue As Integer = 255
    Private m_BackBrightness As Integer = 255
    Private m_BackContrast As Integer = 125
    Private m_KeypadBrightness As Integer = 255

    Private m_AllSensors As New Generic.List(Of Sensor)

    Private m_Settings As Settings
    Private m_SetRememberSettings As Boolean = False
    Private m_Zone As Integer = 0

    Private m_SensorWidth As Integer = 10
    Private m_ScreenWidth As Integer = 20
    Private m_ScreenHeight As Integer = 4

    Private m_Index = 0
    Private m_StartingIndex As Integer

    Private m_ShowAlerts As String = "Show"
    Private m_Alert As New Alert
    Private m_AlertDisplayCount As Integer = 4

    Private m_KeyPadButtons As New Hashtable

    Private m_StaticSensors As New Generic.SortedList(Of Integer, Sensor)
    Private m_CycleSensors As New Generic.List(Of Sensor)

    Private WithEvents m_ScreenTmr As New Timers.Timer
    Private WithEvents m_GPOTmr As New Timers.Timer

    Private m_GPOs As New Hashtable
    Private m_HasInternet As Boolean = False
    Private m_HasBT As Boolean = False
    Private m_HasWireless As Boolean = False
    Private m_IsSpeechListening As Boolean = False

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host
        SafeThread.Asynchronous(AddressOf BackgroundLoad, m_Host)

        Return eLoadStatus.LoadSuccessful
    End Function

    Private Sub BackgroundLoad()
        LoadLCDSettings()

        If String.IsNullOrEmpty(m_ComPort) Then
            Exit Sub
        End If

        m_Alert.Message = ""
        m_Alert.DisplayCount = 4

        Try

            m_LCD = New CharacterLCD.MatrixObrital
            m_LCD.SetPortName(m_ComPort)
            m_LCD.Initalize()
            InitLCDStartupData()
            InitLCDOptions()

            System.Threading.Thread.Sleep(2000)

            InitSensorsList()

            m_ScreenTmr.Interval = m_CycleDelay * 1000
            m_ScreenTmr.Enabled = True

            m_GPOTmr.Interval = m_CycleDelay * 1000
            m_GPOTmr.Enabled = True

        Catch ex As Exception
        End Try
    End Sub


    Private Sub m_ScreenTmr_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_ScreenTmr.Elapsed

       Dim Text As String = ""
        Dim SpotsAvailable As Integer = (m_ScreenHeight * m_ScreenWidth) / m_SensorWidth
        Dim ToDisplay As String = ""

        If m_Index < m_StartingIndex Then 'It rotated back around.  Clear the screen just in case anything got messed up
            m_LCD.ClearScreen()
        End If

        If m_ShowAlerts = "Show" Then
            SyncLock m_Alert
                If m_Alert.DisplayCount > 0 AndAlso (Not String.IsNullOrEmpty(m_Alert.Message)) Then

                    If Not m_Alert.Message.Length = m_ScreenWidth * 2 Then
                        m_Alert.Message &= "                                        "
                        m_Alert.Message = m_Alert.Message.Substring(0, m_ScreenWidth * 2)

                        If m_Alert.Message.Length > m_ScreenWidth * 2 Then
                            m_Alert.Message = m_Alert.Message.Substring(0, m_ScreenWidth * 2)
                        End If
                    End If

                    ToDisplay &= m_Alert.Message
                    m_Alert.DisplayCount -= 1

                    SpotsAvailable -= 4
                End If
            End SyncLock
        End If

        While (m_CycleSensors.Count + m_StaticSensors.Count) < SpotsAvailable
            ToDisplay &= "                                                                   ".Substring(0, m_SensorWidth)
            SpotsAvailable -= 1
        End While

        m_StartingIndex = m_Index
        While m_Index < m_StartingIndex + SpotsAvailable - m_StaticSensors.Count
            Try
                ToDisplay &= FormatSensor(m_CycleSensors(m_Index Mod (m_CycleSensors.Count)))
            Catch ex As Exception
                ToDisplay &= "Err " & m_CycleSensors(m_Index Mod (m_CycleSensors.Count)).Name
            End Try
            m_Index += 1
        End While

        SpotsAvailable -= (m_Index - m_StartingIndex)

        For i As Integer = 0 To SpotsAvailable - 1
            Try
                ToDisplay &= FormatSensor(m_StaticSensors(m_StaticSensors.Keys(i)))
            Catch ex As Exception
                ToDisplay &= "Err " & m_StaticSensors(m_StaticSensors.Keys(i)).Name
            End Try
        Next

        m_LCD.DisplayText(ToDisplay)

        If Not m_CycleSensors.Count = 0 Then
            m_Index = m_Index Mod m_CycleSensors.Count
        End If

    End Sub

    Private Function FormatSensor(ByVal s As Sensor) As String
        Dim Val As String = s.Value
        Dim len As Integer
        Dim Name As String = s.ShortName

        len = m_SensorWidth - (Name.Length + Val.Length)

        If len < 0 Then 'Try dropping the Formatting
            Val = GetVal(s, False)
            len = m_SensorWidth - (Name.Length + Val.Length)

            If len < 0 Then 'See if it is a decimal we can shorten
                Dim PeriodIndex As Integer = Val.LastIndexOf(".")
                If PeriodIndex > 0 Then

                    If Name.Length + PeriodIndex < m_SensorWidth Then 'We can drop decimal places to make it fit
                        Val = Val.Substring(0, m_SensorWidth - Name.Length)
                        len = m_SensorWidth - (Name.Length + Val.Length)
                    End If
                End If
            End If

            If len < 0 Then 'Make the name shorter
                If Name.Length > 0 Then
                    Name = Name.Substring(0, 1)
                End If
            End If
        End If

        If m_SensorWidth - (Name.Length + Val.Length) > -1 Then
            Return Name & "                    ".Substring(0, len) & Val
        End If

        If m_SensorWidth - Val.Length > -1 Then
            Return "                    ".Substring(0, len) & Val
        End If

        If Val.Length > m_SensorWidth Then
            Val = Val.Substring(0, m_SensorWidth)
        End If
        Return Val
    End Function

    Private Function GetVal(ByVal Sen As Sensor, ByVal Format As Boolean) As String
        Dim Val As String = ""

        If Sen.Name.StartsWith("CharacterDisplay.InternalSensor.") Then
            Try
                Select Case Sen.Name
                    Case Is = "CharacterDisplay.InternalSensor.Artist"
                        Val = m_Host.getPlayingMedia(m_Zone).Artist

                    Case Is = "CharacterDisplay.InternalSensor.Album"
                        Val = m_Host.getPlayingMedia(m_Zone).Album

                    Case Is = "CharacterDisplay.InternalSensor.Name"
                        Val = m_Host.getPlayingMedia(m_Zone).Name

                    Case Is = "CharacterDisplay.InternalSensor.Date"
                        Val = DateTime.Now.ToShortDateString

                    Case Is = "CharacterDisplay.InternalSensor.Time"
                        Val = DateTime.Now.ToShortTimeString

                    Case Is = "CharacterDisplay.InternalSensor.DPWeatherTemp"
                        Val = GetWeatherTemp()

                    Case Is = "CharacterDisplay.InternalSensor.Volume"
                        Val = "100"
                End Select

                If String.IsNullOrEmpty(Val) Then
                    Val = ""
                End If
                Return Val

            Catch ex As Exception
                Return ""
            End Try
        End If

        Val = Sen.Value

        If Not Format Then
            Return Val
        End If

        Select Case Sen.DataType
            Case Is = eSensorDataType.Amps
                Return Val & "Amps"

            Case Is = eSensorDataType.binary
                If Val = "1" Then
                    Return "On"
                End If
                Return "Off"

            Case Is = eSensorDataType.bytes
                Dim dec As Double = Val
                If dec > 1099511627776 Then
                    Return (dec / 1099511627776).ToString("#.#") & "T"
                End If
                If dec > 1073741824 Then
                    Return (dec / 1073741824).ToString("#.#") & "G"
                End If
                If dec > 1048576 Then
                    Return (dec / 1048576).ToString("#.#") & "M"
                End If
                If dec > 1024 Then
                    Return (dec / 1024).ToString("#.#") & "K"
                End If
                Return Val & "B"

            Case Is = eSensorDataType.bytespersec
                Dim dec As Double = Val
                If dec > 1048576 Then
                    Return (dec / 1048576).ToString("#.#") & "Mb"
                End If
                If dec > 1024 Then
                    Return (dec / 1024).ToString("#.#") & "Kb"
                End If
                Return Val & "Bp"

            Case Is = eSensorDataType.degrees
                Return Val & "°"

            Case Is = eSensorDataType.degreesC
                Return Globalization.convertToLocalTemp(Val, True).Replace(" ", "")

            Case Is = eSensorDataType.Gs
                Return Val & "Gs"

            Case Is = eSensorDataType.kilometers
                Return Globalization.convertDistanceToLocal(Val, True)

            Case Is = eSensorDataType.kph
                Return Globalization.convertSpeedToLocal(Val, True)

            Case Is = eSensorDataType.meters
                Return Val & "m"

            Case Is = eSensorDataType.percent
                Return Val & "%"

            Case Is = eSensorDataType.psi
                Return Val & "psi"

            Case Is = eSensorDataType.volts
                Return Val & "V"
        End Select

        Return Val
    End Function


    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

        If m_Settings Is Nothing Then
            LoadLCDSettings()
            m_Settings = New Settings("Character Display")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            Dim COMValues As New Generic.List(Of String)
            COMValues.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.ComPort", "COM", "Com Port", COMOptions, COMValues, m_ComPort.ToString))

            Dim Width As New Generic.List(Of String)
            Width.Add("16")
            Width.Add("20")
            Width.Add("40")
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.ScreenWidth", "Width", "Display Width", Width, Width, m_ScreenWidth.ToString))

            Dim Height As New Generic.List(Of String)
            Height.Add("1")
            Height.Add("2")
            Height.Add("4")
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.ScreenHeight", "Height", "Display Height", Height, Height, m_ScreenHeight.ToString))

            Dim SenWidth As New Generic.List(Of String)
            SenWidth.Add("8")
            SenWidth.Add("10")
            SenWidth.Add("16")
            SenWidth.Add("20")
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.SensorWidth", "Width", "Sensor Width", SenWidth, SenWidth, m_SensorWidth.ToString))


            Dim Zones As New Generic.List(Of String)
            For i As Integer = 0 To m_Host.ZoneHandler.ActiveZones.Count
                Zones.Add(i)
            Next
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.Zone", "Zone", "Zone", Zones, Zones, m_Zone))

            Dim Range As New Generic.List(Of String)
            Range.Add("1")
            Range.Add("60")
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.Delay", "Cycle Display", "(1-60sec)", Nothing, Range, m_CycleDelay))

            Dim ShowAlertsList As New Generic.List(Of String)
            ShowAlertsList.Add("Show")
            ShowAlertsList.Add("Disable")
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.ShowAlerts", "Alerts", "Show Alerts on Display", ShowAlertsList, ShowAlertsList, m_ShowAlerts))

            Dim Range255 As New Generic.List(Of String)
            Range255.Add("0")
            Range255.Add("255")
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.BackRed", "Red", "Backlight Red", Nothing, Range255, m_BackRed))
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.BackGreen", "Green", "Backlight Green", Nothing, Range255, m_BackGreen))
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.BackBlue", "Blue", "Backlight Blue", Nothing, Range255, m_BackBlue))
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.BackBrightness", "Brightness", "Backlight Brightness", Nothing, Range255, m_BackBrightness))
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.BackContrast", "Contrast", "Backlight Contrast", Nothing, Range255, m_BackContrast))
            m_Settings.Add(New Setting(SettingTypes.Range, "CharacterDisplay.KeypadBrightness", "Keypad", "Keypad Brightness", Nothing, Range255, m_KeypadBrightness))

            Dim Sett As String
            m_AllSensors = m_Host.getData(eGetData.GetAvailableSensors, "")
            m_AllSensors.AddRange(GetInternalSensors)

            Dim GPOpt As New Generic.List(Of String)
            GPOpt.Add("None")
            GPOpt.Add("Bluetooth On")
            GPOpt.Add("Bluetooth Off")
            GPOpt.Add("Wireless On")
            GPOpt.Add("Wireless Off")
            GPOpt.Add("Internet On")
            GPOpt.Add("Internet Off")
            GPOpt.Add("SpeechListening On")
            GPOpt.Add("SpeechListening Off")

            For Each BitSensor As Sensor In m_AllSensors.FindAll(Function(p) p.Type = eSensorType.deviceSuppliesData AndAlso p.DataType = eSensorDataType.binary)
                GPOpt.Add(BitSensor.Name & " On")
                GPOpt.Add(BitSensor.Name & " Off")
            Next

            For i As Integer = 1 To 6
                Sett = GetSetting("CharacterDisplay.GPO" & i.ToString, "None")
                m_GPOs.Add(i, Sett)
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.GPO" & i.ToString, "GPO" & i, "GPO" & i, GPOpt, GPOpt, Sett))
            Next

            Dim KeyButtonNames As New Generic.List(Of String)
            KeyButtonNames.AddRange([Enum].GetNames(GetType(ButtonEvents)))
            Dim KeyButtonValues As New Generic.List(Of String)
            For Each Btn As ButtonEvents In [Enum].GetValues(GetType(ButtonEvents))
                KeyButtonValues.Add(CInt(Btn).ToString)
            Next

            For Each Btn As CharacterLCD.LCDKey In [Enum].GetValues(GetType(CharacterLCD.LCDKey))
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.Keypad." & Btn.ToString, "Btn", Btn.ToString, KeyButtonNames, KeyButtonValues, GetSetting("CharacterDisplay.Keypad." & Btn.ToString, CInt(ButtonEvents.None).ToString)))
            Next

            Dim SenOpt As New Generic.List(Of String)
            SenOpt.Add("Never")
            SenOpt.Add("Cycle")
            SenOpt.Add("Static 1")
            SenOpt.Add("Static 2")
            SenOpt.Add("Static 3")
            SenOpt.Add("Static 4")
            SenOpt.Add("Static 5")
            SenOpt.Add("Static 6")
            SenOpt.Add("Static 7")
            SenOpt.Add("Static 8")

            For Each S As Sensor In m_AllSensors
                Sett = GetSetting("CharacterDisplay.Sensor." & S.Name, "Cycle")
                m_Settings.Add(New Setting(SettingTypes.MultiChoice, "CharacterDisplay.Sensor." & S.Name, S.ShortName, S.Name, SenOpt, SenOpt, Sett))
            Next

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed

        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)
        Using PlugSet As New OpenMobile.Data.PluginSettings
            PlugSet.setSetting(St.Name, St.Value)
        End Using

        Select Case St.Name
            Case Is = "CharacterDisplay.BackRed", "CharacterDisplay.BackGreen", "CharacterDisplay.BackBlue", "CharacterDisplay.BackBrightness", "CharacterDisplay.BackContrast", "CharacterDisplay.KeypadBrightness"
                m_SetRememberSettings = True
        End Select

        LoadLCDSettings()
        InitLCDOptions()
    End Sub

    Private Sub LoadLCDSettings()
        m_ComPort = GetSetting("CharacterDisplay.ComPort", "COM1")
        m_Zone = GetSetting("CharacterDisplay.Zone", "0")
        m_ShowAlerts = GetSetting("CharacterDisplay.ShowAlerts", "Show")
        m_ScreenWidth = GetSetting("CharacterDisplay.ScreenWidth", "20")
        m_ScreenHeight = GetSetting("CharacterDisplay.ScreenHeight", "4")
        m_SensorWidth = GetSetting("CharacterDisplay.SensorWidth", "10")
        m_CycleDelay = GetSetting("CharacterDisplay.Delay", "10")
        m_BackRed = GetSetting("CharacterDisplay.BackRed", "255")
        m_BackGreen = GetSetting("CharacterDisplay.BackGreen", "255")
        m_BackBlue = GetSetting("CharacterDisplay.BackBlue", "255")
        m_BackBrightness = GetSetting("CharacterDisplay.BackBrightness", "255")
        m_BackContrast = GetSetting("CharacterDisplay.BackContrast", "125")
        m_KeypadBrightness = GetSetting("CharacterDisplay.KeypadBrightness", "255")

        If m_KeyPadButtons.Count = 0 Then
            For Each Btn As CharacterLCD.LCDKey In [Enum].GetValues(GetType(CharacterLCD.LCDKey))
                m_KeyPadButtons.Add(Btn, GetSetting("CharacterDisplay.Keypad." & Btn.ToString, ButtonEvents.None))
            Next
        End If
    End Sub

    Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting(Name)) Then
                Settings.setSetting(Name, DefaultValue)
            End If
            Return Settings.getSetting(Name)
        End Using
    End Function

    Private Sub InitLCDStartupData()
        m_LCD.SaveStartupCustomCharater(0, New Byte() {0, 1, 6, 8, 9, 18, 28, 28})
        m_LCD.SaveStartupCustomCharater(1, New Byte() {15, 16, 7, 24, 0, 0, 24, 12})
        m_LCD.SaveStartupCustomCharater(2, New Byte() {30, 1, 28, 3, 0, 0, 6, 12})
        m_LCD.SaveStartupCustomCharater(3, New Byte() {0, 16, 12, 2, 18, 9, 5, 7})
        m_LCD.SaveStartupCustomCharater(4, New Byte() {28, 4, 2, 1, 0, 0, 0, 0})
        m_LCD.SaveStartupCustomCharater(5, New Byte() {0, 0, 0, 3, 24, 7, 0, 0})
        m_LCD.SaveStartupCustomCharater(6, New Byte() {0, 0, 24, 0, 3, 28, 0, 0})
        m_LCD.SaveStartupCustomCharater(7, New Byte() {7, 4, 8, 48, 0, 0, 0, 0})

        m_LCD.SaveStartupMessage(New Byte() {32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, _
                                           32, 32, 32, 0, 1, 2, 3, 32, 32, 79, 80, 69, 78, 32, 32, 32, 32, 32, 32, 32, _
                                           32, 32, 32, 4, 5, 6, 7, 32, 32, 32, 32, 77, 79, 66, 73, 76, 69, 32, 32, 32, _
                                           32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32})

        m_LCD.SetGPOStartupState(1, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPOStartupState(2, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPOStartupState(3, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPOStartupState(4, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPOStartupState(5, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPOStartupState(6, CharacterLCD.GPOState.OffState)

        m_LCD.SetGPO(1, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPO(2, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPO(3, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPO(4, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPO(5, CharacterLCD.GPOState.OffState)
        m_LCD.SetGPO(6, CharacterLCD.GPOState.OffState)


    End Sub

    Private Sub InitLCDOptions()
        m_LCD.SetBackgroundColor(m_BackRed, m_BackGreen, m_BackBlue)
        m_LCD.SetBackgroundBrightness(m_BackBrightness)
        m_LCD.SetContrast(m_BackContrast)
        m_LCD.SetKeypadBrightness(m_KeypadBrightness)
    End Sub

    Private Sub InitSensorsList()
        Dim Sensors As Generic.List(Of Sensor) = m_Host.getData(eGetData.GetAvailableSensors, "")
        Sensors.AddRange(GetInternalSensors)

        Dim Dic As Generic.Dictionary(Of String, String)

        Using Settings As New OpenMobile.Data.PluginSettings
            Dic = Settings.getAllPluginSettings("CharacterDisplay.Sensor.")
        End Using

        Dim SenVal As String
        Dim StaticNum As Integer = 0
        For Each S As Sensor In Sensors
            If Dic.ContainsKey("CharacterDisplay.Sensor." & S.Name) Then
                SenVal = Dic("CharacterDisplay.Sensor." & S.Name)

                Select Case SenVal
                    Case Is = "Cycle"
                        m_CycleSensors.Add(S)

                    Case Is = "Static 1", "Static 2", "Static 3", "Static 4", "Static 5", "Static 6", "Static 7", "Static 8"
                        StaticNum = CInt(SenVal.Replace("Static ", ""))
                        While m_StaticSensors.ContainsKey(StaticNum)
                            StaticNum += 1
                        End While
                        m_StaticSensors.Add(StaticNum, S)

                End Select
            End If
        Next

    End Sub

    Private Function GetInternalSensors() As Generic.List(Of Sensor)
        Dim lst As New Generic.List(Of Sensor)
        'lst.Add(New Sensor("CharacterDisplay.InternalSensor.Artist", eSensorType.deviceSuppliesData, "", eSensorDataType.raw))
        'lst.Add(New Sensor("CharacterDisplay.InternalSensor.Album", eSensorType.deviceSuppliesData, "", eSensorDataType.raw))
        'lst.Add(New Sensor("CharacterDisplay.InternalSensor.Name", eSensorType.deviceSuppliesData, "", eSensorDataType.raw))
        'lst.Add(New Sensor("CharacterDisplay.InternalSensor.DPWeatherTemp", eSensorType.deviceSuppliesData, "Out", eSensorDataType.degreesC))
        'lst.Add(New Sensor("CharacterDisplay.InternalSensor.Volume", eSensorType.deviceSuppliesData, "Vol", eSensorDataType.percent))

        Return lst
    End Function


    Private Enum ButtonEvents As Byte
        None = 1
        NextSong
        PreviousSong
        PausePlay
        VolumeUp
        VolumeDown
        Mute
        RestartOM
        Reboot
        Radio
        Home
        Music
        Navigation
    End Enum

    Private Sub m_LCD_KeyPressReceived(ByVal Key As CharacterLCD.LCDKey) Handles m_LCD.KeyPressReceived
        Try
            Dim WhatToDo As ButtonEvents = m_KeyPadButtons(Key)
            Select Case WhatToDo
                Case Is = ButtonEvents.Mute
                    Dim Vol As Integer
                    m_Host.getData(eGetData.GetSystemVolume, "", m_Zone, Vol)
                    If Not Vol = -1 Then
                        m_Host.execute(eFunction.setSystemVolume, -1, m_Zone)
                    Else
                        m_Host.execute(eFunction.setSystemVolume, -2, m_Zone)
                    End If

                Case Is = ButtonEvents.NextSong
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, " ")

                Case Is = ButtonEvents.PausePlay
                    m_Host.execute(eFunction.Pause, m_Zone)

                Case Is = ButtonEvents.PreviousSong
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, "back")

                Case Is = ButtonEvents.Reboot
                    m_Host.execute(eFunction.restart)

                Case Is = ButtonEvents.RestartOM
                    m_Host.execute(eFunction.restartProgram)

                Case Is = ButtonEvents.VolumeDown
                    Dim Vol As Integer
                    m_Host.getData(eGetData.GetSystemVolume, "", m_Zone, Vol)
                    If Vol > 0 Then
                        m_Host.execute(eFunction.setSystemVolume, Vol - 5, m_Zone)
                    End If

                Case Is = ButtonEvents.VolumeUp
                    Dim Vol As Integer
                    m_Host.getData(eGetData.GetSystemVolume, "", m_Zone, Vol)
                    If Vol < 100 Then
                        m_Host.execute(eFunction.setSystemVolume, Vol + 5, m_Zone)
                    End If

                Case Is = ButtonEvents.Radio
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, "R")

                Case Is = ButtonEvents.Navigation
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, "N")

                Case Is = ButtonEvents.Home
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, "H")

                Case Is = ButtonEvents.Music
                    m_Host.execute(eFunction.multiTouchGesture, m_Zone, "M")

            End Select

        Catch ex As Exception
        End Try
    End Sub


    Public ReadOnly Property authorEmail() As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName() As String Implements OpenMobile.Plugin.IBasePlugin.authorName
        Get
            Return "Jon Heizer"
        End Get
    End Property

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "Character Display"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "CharacterDisplay"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If m_SetRememberSettings Then
                    SaveRememberSettings()
                End If
            End If
            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

#Region " IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Private Sub m_Host_OnMediaEvent(ByVal fun As OpenMobile.eFunction, ByVal zone As OpenMobile.Zone, ByVal arg As String) Handles m_Host.OnMediaEvent
        Dim Msg As String = ""
        Select Case fun
            Case Is = eFunction.Play, eFunction.tunerDataUpdated
                If zone.ID = m_Zone Then
                    Dim media As mediaInfo = m_Host.getPlayingMedia(m_Zone)
                    Msg = media.Name & "                    "
                    Msg = Msg.Substring(0, m_ScreenWidth)
                    Msg &= media.Artist & "                    "
                    Msg = Msg.Substring(0, m_ScreenWidth * 2)
                End If
        End Select
        If Not String.IsNullOrEmpty(Msg) Then
            SyncLock m_Alert
                m_Alert.Message = Msg
                m_Alert.DisplayCount = m_AlertDisplayCount
            End SyncLock
        End If
    End Sub

    Private Sub m_Host_OnNavigationEvent(ByVal type As OpenMobile.eNavigationEvent, ByVal arg As String) Handles m_Host.OnNavigationEvent

    End Sub

    Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles m_Host.OnPowerChange
        Dim Msg As String = type.ToString
        Msg = Msg.Replace("ePowerEvent.", "")
        Msg = Trim(Regex.Replace(Msg, "[A-Z][^A-Z].*?(?=[A-Z\d]|$)|[A-Z]+(?=[A-Z\d][a-z]|$)|\d+(?=[\w-\d])", " ${0}"))
        SetAlertText(Msg)

        Select Case type
            Case Is = ePowerEvent.SleepOrHibernatePending
                SleepTime()
            Case Is = ePowerEvent.SystemResumed
                Resumed()
        End Select
    End Sub

    Private Sub m_Host_OnStorageEvent(ByVal type As OpenMobile.eMediaType, ByVal justInserted As Boolean, ByVal arg As String) Handles m_Host.OnStorageEvent
        If justInserted Then
            Dim Msg As String = type.ToString
            Msg = Msg.Replace("eMediaType", "")
            Msg = Trim(Regex.Replace(Msg, "[A-Z][^A-Z].*?(?=[A-Z\d]|$)|[A-Z]+(?=[A-Z\d][a-z]|$)|\d+(?=[\w-\d])", " ${0}"))
            SetAlertText(Msg & "Plugged In")
        End If
    End Sub

    Private Sub m_Host_OnSystemEvent(ByVal fun As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
        Dim Msg As String = ""
        Select Case fun
            Case Is = eFunction.connectedToInternet
                Msg = "Internet Connected"
                m_HasInternet = True
            Case Is = eFunction.disconnectedFromInternet
                Msg = "Internet Lost"
                m_HasInternet = False
            Case Is = eFunction.backgroundOperationStatus
                Msg = arg1
        End Select
        SetAlertText(Msg)
    End Sub

    Private Sub m_Host_OnWirelessEvent(ByVal type As OpenMobile.eWirelessEvent, ByVal arg As String) Handles m_Host.OnWirelessEvent
        Dim Msg As String = type.ToString
        If Not Msg.EndsWith("Changed") Then
            Msg = Msg.Replace("eWirelessEvent.", "")
            Msg = Trim(Regex.Replace(Msg, "[A-Z][^A-Z].*?(?=[A-Z\d]|$)|[A-Z]+(?=[A-Z\d][a-z]|$)|\d+(?=[\w-\d])", " ${0}"))
            SetAlertText(Msg)
        End If
        Select Case type
            Case Is = eWirelessEvent.BluetoothDeviceConnected
                m_HasBT = True
            Case Is = eWirelessEvent.ConnectedToWirelessNetwork
                m_HasWireless = True
            Case Is = eWirelessEvent.DisconnectedFromWirelessNetwork
                m_HasWireless = False
        End Select
    End Sub

    Private Sub SleepTime()
        If m_SetRememberSettings Then
            SaveRememberSettings()
        End If
        m_ScreenTmr.Enabled = False
        m_GPOTmr.Enabled = False
        m_LCD.CloseConnection()
    End Sub

    Private Sub Resumed()
        System.Threading.Thread.Sleep(2000)

        m_LCD = New CharacterLCD.MatrixObrital
        m_LCD.SetPortName(m_ComPort)
        m_LCD.Initalize()
        InitLCDStartupData()
        InitLCDOptions()

        m_ScreenTmr.Enabled = True
        m_GPOTmr.Enabled = True
    End Sub


    Private Sub SetAlertText(ByVal Msg As String)
        If Not String.IsNullOrEmpty(Msg) Then
            SyncLock m_Alert
                m_Alert.Message = Msg
                m_Alert.DisplayCount = m_AlertDisplayCount
            End SyncLock
        End If
    End Sub

    Private Class Alert
        Public Message As String
        Public DisplayCount As Integer
    End Class

    Private Function GetWeatherTemp() As String
        Dim provider As New OpenMobile.Data.Weather
        Dim data As OpenMobile.Data.Weather.weather

        Try
            Using PlugSet As New OpenMobile.Data.PluginSettings
                Dim loc As String = PlugSet.getSetting("Data.DefaultLocation")
                data = provider.readWeather(loc, DateTime.Today)
            End Using
        Catch ex As Exception
            Return ""
        End Try

        Return Globalization.convertToLocalTemp(data.temp, True).Replace(" ", "")
    End Function

    Private Sub SaveRememberSettings()
        If m_SetRememberSettings Then
            m_LCD.SetRemember(True)
            InitLCDOptions()
        End If
    End Sub

    Private Sub m_GPOTmr_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_GPOTmr.Elapsed
        For i As Integer = 1 To 6
            Dim Sen As String = m_GPOs(i)
            If Not Sen = "None" Then
                Dim Bt As String = Sen.Substring(Sen.Length - 3).Trim
                Dim Name As String = Sen.Substring(0, Sen.Length - 3).Trim
                Dim Val As String = "0"
                Select Case Name
                    Case Is = "Internet"
                        If m_HasInternet Then Val = "1"

                    Case Is = "Bluetooth"
                        If m_HasBT Then Val = "1"

                    Case Is = "Wireless"
                        If m_HasWireless Then Val = "1"

                    Case Is = "SpeechListening"
                        If m_IsSpeechListening Then Val = "1"

                    Case Else
                        Dim Sensor As Sensor = m_AllSensors.Find(Function(s) s.Name = Name)
                        If Not Sensor Is Nothing Then
                            Val = Sensor.Value.ToString
                        End If
                End Select

                If (Val = "1" AndAlso Bt = "On") OrElse (Val = "0" AndAlso Bt = "Off") Then
                    m_LCD.SetGPO(i, CharacterLCD.GPOState.OnState)
                Else
                    m_LCD.SetGPO(i, CharacterLCD.GPOState.OffState)
                End If
            End If
        Next
    End Sub

End Class
