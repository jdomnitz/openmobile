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
Imports OpenMobile.Plugin
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.Data
Imports OpenMobile.Threading
Imports OpenMobile.helperFunctions
Imports OpenMobile.Controls
Imports HDRadioComm

Public Class RadioComm
    Implements OpenMobile.Plugin.ITunedContent
    Implements OpenMobile.Plugin.IPlayer
    Implements OpenMobile.Plugin.IPlayerVolumeControl

    Private WithEvents m_Host As IPluginHost

    Private WithEvents m_Radio As New HDRadio
    Private m_ComPort As String = "Auto"
    Private m_SourceDevice As Integer = 0
    Private m_Fade As Boolean = True
    Private m_verbose As Boolean = False        ' Controls logging of extra info to debug log
    Private m_RouteVolume As Integer = 100
    Private connect_timeout As Integer = 20     ' Time to wait for connect and powerup of Radio in seconds
    Private access_wait As Integer = 1000       ' How long to wait for serial access (ms)
    Private m_Retries As Integer = 5            ' How many times to have the driver check for a radio
    Private m_Tries As Integer = 0              ' How many tries left to test for a radio
    Private port_index As Integer = -1          ' Which port in the array we are working with

    Private WithEvents m_Audio As AudioRouter.AudioManager

    Private WithEvents m_Settings As Settings

    'Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, "HD Radio", "", AddressOf myNotification_clickAction, AddressOf myNotification_clearAction)
    Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, "HD Radio", "")

    Private available_ports() As String = System.IO.Ports.SerialPort.GetPortNames

    Private m_StationList As New Generic.SortedDictionary(Of Integer, stationInfo)
    Private m_SubStationData As New Generic.SortedDictionary(Of Integer, SubChannelData)
    Private m_CurrentMedia As New mediaInfo
    Private m_CurrentInstance As Zone = Nothing
    Private m_HDCallSign As String = ""
    Private m_IsScanning As Boolean = False
    Private m_CommError As Boolean = False
    Private m_NotFound As Boolean = False
    Private initialized As Boolean = False

    Private waitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)
    Private comWaitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)
    Private pwrWaitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)

    Public Event OnMediaEvent(ByVal [function] As OpenMobile.eFunction, ByVal zone As OpenMobile.Zone, ByVal arg As String) Implements OpenMobile.Plugin.IPlayer.OnMediaEvent

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize

        Array.Sort(available_ports)

        m_Host = host

        m_Host.UIHandler.AddNotification(myNotification)
        myNotification.Text = "Plugin initializing..."

        m_ComPort = GetSetting("OMVisteonRadio.ComPort", m_ComPort)

        m_Host.CommandHandler.AddCommand(New Command(Me, "HDRadio", "Seek", "Up", AddressOf command_scanForward, 0, False, "Tune up to next available channel"))
        m_Host.CommandHandler.AddCommand(New Command(Me, "HDRadio", "Seek", "Down", AddressOf command_scanReverse, 0, False, "Tune down to next available channel"))
        m_Host.CommandHandler.AddCommand(New Command(Me, "HDRadio", "Tune", "Up", AddressOf command_tuneUp, 0, False, "Tune up one step"))
        m_Host.CommandHandler.AddCommand(New Command(Me, "HDRadio", "Tune", "Down", AddressOf command_tuneDown, 0, False, "Tune down one step"))

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Invoking background loading..."))
        End If
        SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

        Return eLoadStatus.LoadSuccessful

    End Function

    Private Sub myNotification_clickAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
        ' Not currently used
        cancel = False
    End Sub

    Private Sub myNotification_clearAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
        ' Not currently used
        cancel = False
    End Sub

    Private Sub BackgroundLoad()

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Background initialization."))
        End If

        LoadRadioSettings()

        Try

            'Check to see if the source audio device still exists, if not switch to default
            If Not Array.IndexOf(AudioRouter.AudioRouter.listSources, m_SourceDevice) > -1 Then
                m_SourceDevice = 0
            End If

            m_Audio = New AudioRouter.AudioManager(m_SourceDevice)
            m_Audio.FadeIn = m_Fade
            m_Audio.FadeOut = m_Fade

            find_radio()

        Catch ex As Exception
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("{0} - {1}", ex.Source, ex.ToString))

        End Try

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Background initialization complete."))
        End If

    End Sub

    Private Sub find_radio()

        Dim access_granted As Boolean = False
        Dim starttime As DateTime

        m_Radio.AutoSearch = False

        If String.IsNullOrEmpty(m_ComPort) Then
            m_ComPort = "Auto"
        End If

        If m_ComPort <> "Auto" Then
            ' Try the user specified COM port
            myNotification.Text = String.Format("Waiting for lock on {0}", m_ComPort)
            ' Try a couple times to get serial access
            For x = 1 To m_Retries
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Waiting for serial access."))
                End If
                access_granted = OpenMobile.helperFunctions.SerialAccess.GetAccess(Me)
                If access_granted Then
                    Exit For
                End If
                System.Threading.Thread.Sleep(100)
            Next
            If access_granted Then
                myNotification.Text = String.Format("Searching for radio on {0}", m_ComPort)
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Serial access granted")
                End If
                m_NotFound = False
                m_Radio.ComPortString = m_ComPort
                m_Radio.Open()
                If comWaitHandle.WaitOne(4000) = False Then
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Com Port did not open.")
                    End If
                    ' comm port did not open
                    OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
                    If m_CommError Then
                        myNotification.Text = String.Format("Comm error probing {0}.", m_ComPort)
                        If m_verbose Then
                            m_Host.DebugMsg(OpenMobile.DebugMessageType.Error, "OMVisteonRadio - find_radio()", String.Format("Comm error probing {0}.", m_ComPort))
                        End If
                        m_CommError = False
                        m_Radio.Close()
                    End If
                Else
                    OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
                    m_Radio.PowerOn()
                    If pwrWaitHandle.WaitOne(2000) = False Then
                        ' radio did not power on
                        If m_verbose Then
                            m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Radio did not power on.")
                        End If
                    End If
                End If ' Comm Wait
            End If ' Access granted
        End If ' Auto

        If m_Radio.IsPowered Then
            m_ComPort = m_Radio.ComPortString
            myNotification.Text = String.Format("Found radio on {0}.", m_Radio.ComPortString)
            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.ComPort", m_Radio.ComPortString)
            Exit Sub
        End If

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("No radio found on {0}.", m_ComPort))
            m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Switching to Auto.")
        End If

        ' At this point we did not see a radio on the user specified port
        '  so now we will auto-scan ports to see if we find it

        For x = 0 To UBound(available_ports)
            ' Try the user specified COM port
            myNotification.Text = String.Format("Searching for radio on {0}", available_ports(x))
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Searching for radio on {0}", available_ports(x)))
            End If
            ' Try a couple times to get serial access
            For y = 1 To m_Retries
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Waiting for serial access (attempt {0}).", y))
                End If
                access_granted = OpenMobile.helperFunctions.SerialAccess.GetAccess(Me)
                If access_granted Then
                    Exit For
                End If
                System.Threading.Thread.Sleep(50)
            Next
            If access_granted Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Serial access granted")
                End If
                m_NotFound = False
                m_Radio.ComPortString = available_ports(x)
                m_Radio.Open()
                starttime = Now()
                If comWaitHandle.WaitOne(2000) = False Then
                    ' comm port did not open
                    OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Com Port did not open.")
                    End If
                    If m_CommError Then
                        myNotification.Text = String.Format("Comm error probing {0}.", available_ports(x))
                        If m_verbose Then
                            m_Host.DebugMsg(OpenMobile.DebugMessageType.Error, "OMVisteonRadio - find_radio()", String.Format("Comm error probing {0}.", available_ports(x)))
                        End If
                        m_CommError = False
                    End If
                    Continue For
                Else
                    OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
                    m_Radio.PowerOn()
                    If pwrWaitHandle.WaitOne(2000) = False Then
                        ' radio did not power on
                        If m_verbose Then
                            m_Host.DebugMsg("OMVisteonRadio - find_radio()", "Radio did not power on.")
                        End If
                        m_Radio.Close()
                        Continue For
                    End If
                    Exit For
                End If
            End If ' Access granted
        Next

        If Not m_Radio.IsPowered Then
            myNotification.Text = "Radio not found."
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - find_radio()", "No radio found on any port.")
            End If
            Exit Sub
        End If

        myNotification.Text = String.Format("Radio found on {0}.", m_Radio.ComPortString)
        helperFunctions.StoredData.Set(Me, "OMVisteonRadio.ComPort", m_Radio.ComPortString)

    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

        If m_Settings Is Nothing Then

            'LoadRadioSettings()

            m_Settings = New Settings("HD Radio")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.Add("Auto")
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.ComPort", "COM", "Com Port", COMOptions, COMOptions, m_ComPort))

            Dim SourceOptions As New Generic.List(Of String)
            SourceOptions.AddRange(AudioRouter.AudioRouter.listSources)
            Dim SourceValues As New Generic.List(Of String)
            For i As Integer = 0 To SourceOptions.Count - 1
                SourceValues.Add(i)
            Next

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.SourceDevice", "Input", "Input Source", SourceOptions, SourceValues, m_SourceDevice))

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.FadeAudio", "Fade", "Fade Audio", Setting.BooleanList, Setting.BooleanList, m_Fade))

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_verbose))

            Dim Range As New Generic.List(Of String)
            Range.Add("0")
            Range.Add("100")
            m_Settings.Add(New Setting(SettingTypes.Range, "OMVisteonRadio.AudioVolume", "Input Vol", "(0-100%)", Nothing, Range, m_RouteVolume))

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed

        End If

        Return m_Settings

    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)

        helperFunctions.StoredData.Set(Me, St.Name, St.Value)

        Select Case St.Name
            Case "OMVisteonRadio.ComPort"
            Case "OMVisteonRadio.SourceDevice"
            Case "OMVisteonRadio.FadeAudio"
                m_Fade = St.Value
            Case "OMVisteonRadio.VerboseDebug"
                m_verbose = St.Value
            Case "OMVisteonRadio.AudioVolume"

        End Select

    End Sub

    Private Sub LoadRadioSettings()

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.ComPort", m_ComPort)
        m_ComPort = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.ComPort")
        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.SourceDevice", m_SourceDevice)
        m_SourceDevice = helperFunctions.StoredData.GetInt(Me, "OMVisteonRadio.SourceDevice")
        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.FadeAudio", m_Fade)
        m_Fade = helperFunctions.StoredData.GetBool(Me, "OMVisteonRadio.FadeAudio")
        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.VerboseDebug", m_verbose)
        m_verbose = helperFunctions.StoredData.GetBool(Me, "OMVisteonRadio.VerboseDebug")
        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.AudioVolume", m_RouteVolume)
        m_RouteVolume = helperFunctions.StoredData.GetInt(Me, "OMVisteonRadio.AudioVolume")

    End Sub

    Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String

        If String.IsNullOrEmpty(helperFunctions.StoredData.Get(Me, Name)) Then
            helperFunctions.StoredData.Set(Me, Name, DefaultValue)
        End If
        Return helperFunctions.StoredData.Get(Me, Name)

    End Function

    Public Function setPowerState(ByVal zone As OpenMobile.Zone, ByVal powerState As Boolean) As Boolean Implements OpenMobile.Plugin.ITunedContent.setPowerState

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), String.Format("setPowerState: {0}", powerState))
        End If

        m_CurrentInstance = zone

        If Not powerState Then
            ' Request to power off, but we ignore it
            m_Audio.Suspend()
            Return True
        Else
            If Not m_Audio Is Nothing Then
                m_Audio.Resume()
            End If
        End If

        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            ' Radio is already on
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), String.Format("Radio device is already powered on."))
            End If
            Return True
        End If

        If (waitHandle.WaitOne(60000) = False) Then
            ' We wait here up to 1 minute to block loadTunedContent if find_radio is still running
            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), String.Format("find_radio() did not complete in the allotted time."))
            Return False
        End If

        ' Okay, find_radio() is done
        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), String.Format("Thread block is released."))
        End If

        ' If waithandle has been release, we should be connected to a radio
        '  if not, get out of here
        If Not m_Radio.IsOpen Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), String.Format("COM Port is not open."))
            Return False
        End If

        Try

            If m_Audio Is Nothing Then
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState({0}, {1})", zone.Description, powerState), "m_Audio is NOTHING")
                End If
                Return False
            End If

            m_Audio.setVolume(zone.AudioDevice.Instance, m_RouteVolume)
            m_Audio.setZonePower(zone.AudioDevice.Instance, powerState)

            If powerState Then

                Dim retval As Boolean = False
                Dim startTime As DateTime
                Dim elapsedTime As TimeSpan
                Dim maxSeconds As Integer = 20
                startTime = Now

                ' Initializing -> BackgroundLoad -> find_radio should result in radio being powered up (if one exists)
                If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
                    ' radio is already on, so get outta here
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState()"), "Radio is already powered on.")
                    End If
                    Return True
                End If

                m_Radio.PowerOn()

                ' Check and/or wait for the radio to come on
                '  This will block until we are done

                Do While True

                    ' We will only wait maxSeconds for radio to come on
                    elapsedTime = Now().Subtract(startTime)

                    If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
                        ' Radio powered up, so lets get outta here
                        If m_verbose Then
                            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState({0}, {1})", zone.Description, powerState), String.Format("Radio powered ON in {0} seconds.", elapsedTime.Seconds))
                        End If
                        retval = True
                        Exit Do
                    End If

                    If elapsedTime.Seconds > maxSeconds Then
                        ' Time is up. We didn't get the radio powered up
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState({0}, {1})", zone.Description, powerState), String.Format("Radio did not power on within {0} seconds.", maxSeconds))
                        retval = False
                        Exit Do
                    End If

                Loop

                Return retval

            Else

                ' This was already here.  Not sure why since it
                '  returns TRUE on all paths.
                If m_Audio.ActiveInstances.Length > 0 Then
                    Return True
                End If

                Return True

            End If

        Catch ex As Exception

            m_Host.DebugMsg(String.Format("OMVisteonRadio - setPowerState({0}, {1})", zone.Description, powerState), String.Format("{0} {1}", ex.Source, ex.Message))
            Return False

        End Try

    End Function
    Private Sub m_Host_OnSystemEvent(ByVal type As OpenMobile.eFunction, args As Object) Handles m_Host.OnSystemEvent

        Select Case type

            Case Is = eFunction.shutdown
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - m_Host_OnSystemEvent()"), "System Event: Shutdown")
                End If
                If m_Radio.IsPowered Then
                    m_Radio.PowerOff()
                End If
                If m_Radio.IsOpen Then
                    m_Radio.Close()
                End If
            Case Is = eFunction.closeProgram
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - m_Host_OnSystemEvent()"), "System Event: closeProgram")
                End If
                If m_Radio.IsPowered Then
                    m_Radio.PowerOff()
                End If
                If m_Radio.IsOpen Then
                    m_Radio.Close()
                End If
            Case Is = eFunction.restartProgram
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - m_Host_OnSystemEvent()"), "System Event: restartProgram")
                End If
                If m_Radio.IsPowered Then
                    m_Radio.PowerOff()
                End If
                If m_Radio.IsOpen Then
                    m_Radio.Close()
                End If
            Case Is = eFunction.restart
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - m_Host_OnSystemEvent()"), "System Event: restart")
                End If
                If m_Radio.IsPowered Then
                    m_Radio.PowerOff()
                End If
                If m_Radio.IsOpen Then
                    m_Radio.Close()
                End If

        End Select

    End Sub

    Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles m_Host.OnPowerChange

        Select Case type

            Case Is = ePowerEvent.SystemResumed
                m_Host.DebugMsg("OMVisteonRadio - m_Host_OnPowerChange", "HD Radio resuming from sleep/hibernate.")
                ' Just in case we left a message up on the bar
                ' Give things a second to get settled
                System.Threading.Thread.Sleep(2000)
                If Not m_Radio Is Nothing Then
                    m_Radio.MuteOff()
                    If Not m_Audio Is Nothing Then
                        m_Audio.Resume()
                    End If
                    m_Radio.Open()
                End If

            Case Is = ePowerEvent.SleepOrHibernatePending
                m_Host.DebugMsg("OMVisteonRadio - m_Host_OnPowerChange", "HD Radio sleep/hibernate pending.")
                If m_Radio.IsOpen Then
                    m_Host.DebugMsg("OMVisteonRadio - m_Host_OnPowerChange", String.Format("Closing device connection."))
                    m_Radio.Close()
                End If
                If Not m_Audio Is Nothing Then
                    m_Audio.Suspend()
                End If

            Case Is = ePowerEvent.ShutdownPending
                m_Host.DebugMsg("OMVisteonRadio - m_Host_OnPowerChange", "HD Radio System Shutdown pending.")
                If m_Radio.IsOpen Then
                    m_Host.DebugMsg("OMVisteonRadio - m_Host_OnPowerChange", String.Format("Closing device connection."))
                    m_Radio.Close()
                End If
                If Not m_Audio Is Nothing Then
                    m_Audio.Suspend()
                End If
        End Select

    End Sub

    Public Function getStatus(ByVal zone As OpenMobile.Zone) As OpenMobile.tunedContentInfo Implements OpenMobile.Plugin.ITunedContent.getStatus

        Dim tc As New tunedContentInfo

        Select Case m_Radio.PowerState
            Case Is = HDRadio.PowerStatus.PowerOn
                tc.status = eTunedContentStatus.Tuned
            Case Else
                tc.status = eTunedContentStatus.Unknown
        End Select

        If m_verbose Then
            ' m_Host.DebugMsg(String.Format("OMVisteonRadio - getStatus()"), String.Format("TunedContentStatus set to {0}.", tc.status))
        End If

        tc.band = CInt(m_Radio.CurrentBand) + 1

        tc.powerState = True
        tc.channels = 2

        tc.currentStation = getStationInfo(zone)
        tc.stationList = getStationList(zone)

        Return tc

    End Function

    Public Function getStationInfo(ByVal zone As OpenMobile.Zone) As OpenMobile.stationInfo Implements OpenMobile.Plugin.ITunedContent.getStationInfo

        Dim Info As stationInfo

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - getStationInfo()"), String.Format("CurrentFrequency: {0}.", m_Radio.CurrentFrequency))
        End If

        If m_Radio.CurrentFrequency > 100 Then

            SyncLock m_StationList
                If Not m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                    Info = New stationInfo
                    Info.stationID = m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100
                    Info.Bitrate = 0
                    Info.Channels = 2
                    Info.stationGenre = ""
                    Info.signal = 1
                    m_StationList.Add(m_Radio.CurrentFrequency, Info)
                End If
            End SyncLock

            Info = m_StationList(m_Radio.CurrentFrequency)
            Info.HD = m_Radio.IsHDActive
            Info.stationName = m_Radio.CurrentFormattedChannel

            Return Info

        End If

        Return New stationInfo

    End Function

    Public Function getMediaInfo(ByVal zone As OpenMobile.Zone) As OpenMobile.mediaInfo Implements OpenMobile.Plugin.IPlayer.getMediaInfo
        Return m_CurrentMedia
    End Function

    Public Function getStationList(ByVal zone As OpenMobile.Zone) As OpenMobile.stationInfo() Implements OpenMobile.Plugin.ITunedContent.getStationList
        Dim info(m_StationList.Values.Count) As stationInfo
        m_StationList.Values.CopyTo(info, 0) 'Faster then ToArray :)
        Return info
    End Function

    Public Function tuneTo(ByVal zone As OpenMobile.Zone, ByVal station As String) As Boolean Implements OpenMobile.Plugin.ITunedContent.tuneTo

        Dim Chan() As String
        Dim Band As HDRadioComm.HDRadio.HDRadioBands = HDRadio.HDRadioBands.FM
        Dim Freq As Decimal = 0
        Dim SubChan As Integer

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - tuneTo()"), String.Format("Tuning to {0}", station))
        End If

        If station.Length > 0 Then
            m_CurrentInstance = zone

            Chan = station.Split(":")
            If Chan.Length > 1 Then

                If Chan(0) = "FM" OrElse Chan(0) = "HD" Then
                    Band = HDRadio.HDRadioBands.FM
                ElseIf Chan(0) = "AM" Then
                    Band = HDRadio.HDRadioBands.AM
                Else
                    Return False
                End If

                Freq = CInt(Chan(1) / 100)

                m_Radio.TuneToChannel(Freq, Band)

                If Chan.Length = 3 Then
                    Dim LockoutCount As Integer = 0
                    While m_Radio.HDSubChannelCount = 0 AndAlso LockoutCount < 60
                        System.Threading.Thread.Sleep(500)
                        LockoutCount += 1
                    End While
                    If m_Radio.HDSubChannelCount > 0 Then
                        If Array.Exists(m_Radio.HDSubChannelList, Function(p) p = SubChan) Then
                            m_Radio.HDSubChannelSelect(SubChan)
                        End If
                    End If
                End If

                Return (True)

            End If
        End If

        Return False

    End Function

    Private Function command_tuneDown()

        ' Command to step backward one frequency step

        For Each zone In m_Host.ZoneHandler.Zones
            stepBackward(zone)
        Next
        Return True

    End Function

    Public Function stepBackward(ByVal zone As OpenMobile.Zone) As Boolean Implements OpenMobile.Plugin.ITunedContent.stepBackward

        ' Backward one frequency step

        m_CurrentInstance = zone
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.TuneDown()
        End If
        Return True

    End Function

    Private Function command_tuneUp()

        ' Command to step forward one frequency step

        For Each zone In m_Host.ZoneHandler.Zones
            stepForward(zone)
        Next
        Return True

    End Function

    Public Function stepForward(ByVal zone As OpenMobile.Zone) As Boolean Implements OpenMobile.Plugin.ITunedContent.stepForward

        ' Forward one frequency step

        m_CurrentInstance = zone
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.TuneUp()
        End If

        Return True

    End Function

    Private Function command_scanReverse()

        ' Command to scan down to next found station

        For Each zone In m_Host.ZoneHandler.Zones
            scanReverse(zone)
        Next

        Return True

    End Function

    Public Function scanReverse(ByVal zone As OpenMobile.Zone) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanReverse

        ' Scan down to next found station

        m_CurrentInstance = zone
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.SeekDown(HDRadio.HDRadioSeekType.ALL)
        End If

        Return True

    End Function

    Private Function command_scanForward()

        ' Command to scan up to next found station

        For Each zone In m_Host.ZoneHandler.Zones
            scanForward(zone)
        Next

        Return True

    End Function

    Public Function scanForward(ByVal zone As OpenMobile.Zone) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanForward

        ' Scan up to next found station

        m_CurrentInstance = zone
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.SeekUp(HDRadio.HDRadioSeekType.ALL)
        End If

        Return True

    End Function

    Public Function scanBand(ByVal zone As OpenMobile.Zone) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanBand

        ' Toggles Scanning through available stations, pausing between

        If m_IsScanning Then
            m_IsScanning = False
        Else
            m_IsScanning = True
            SafeThread.Asynchronous(AddressOf Scan, m_Host)
        End If

        Return True

    End Function

    Private Sub Scan()
        Try
            Dim StartFreq As Integer = m_Radio.CurrentFrequency
            While m_IsScanning
                If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
                    m_Radio.SeekUp(HDRadio.HDRadioSeekType.ALL)
                End If

                System.Threading.Thread.Sleep(5000)

                If m_Radio.CurrentFrequency = StartFreq Then
                    m_IsScanning = False
                End If
            End While

        Catch ex As Exception
        End Try
    End Sub

    Public Function setBand(ByVal zone As OpenMobile.Zone, ByVal band As OpenMobile.eTunedContentBand) As Boolean Implements OpenMobile.Plugin.ITunedContent.setBand

        m_CurrentInstance = zone
        Dim m_LastPlaying As String = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastPlaying")

        If band = eTunedContentBand.AM Then
            Dim m_Last As String = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastAM")
            If String.IsNullOrEmpty(m_Last) Then
                Return tuneTo(zone, "AM:53000")
            Else
                Return tuneTo(zone, m_Last)
            End If
        ElseIf band = eTunedContentBand.FM OrElse band = eTunedContentBand.HD Then
            Dim m_Last As String = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastFM")
            If String.IsNullOrEmpty(m_Last) Then
                Return tuneTo(zone, "FM:88100")
            Else
                Return tuneTo(zone, m_Last)
            End If
        End If

    End Function

    Public Function getSupportedBands(ByVal zone As OpenMobile.Zone) As OpenMobile.eTunedContentBand() Implements OpenMobile.Plugin.ITunedContent.getSupportedBands
        Return New eTunedContentBand() {eTunedContentBand.AM, eTunedContentBand.FM, eTunedContentBand.HD}
    End Function

    Private Sub m_Radio_HDRadioEventHDSubChannel(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSubChannel

        SyncLock m_SubStationData
            If m_SubStationData.ContainsKey(m_Radio.CurrentHDSubChannel) Then
                Dim Data As SubChannelData = m_SubStationData(m_Radio.CurrentHDSubChannel)
                m_CurrentMedia.Artist = Data.Artist
                m_CurrentMedia.Album = Data.Title

                If String.IsNullOrEmpty(Data.Station) Then
                    m_CurrentMedia.Name = m_HDCallSign + " HD-" + m_Radio.CurrentHDSubChannel
                Else
                    m_CurrentMedia.Name = Data.Station
                End If

            Else
                m_CurrentMedia.Artist = " "
                m_CurrentMedia.Album = " "
                m_CurrentMedia.Name = m_HDCallSign & "HD-" & m_Radio.CurrentHDSubChannel
            End If

            RaiseMediaEvent(eFunction.Play, "")
        End SyncLock
    End Sub

    Private Sub m_Radio_HDRadioEventHDSubChannelCount(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSubChannelCount
        If State > 1 Then
            SyncLock m_SubStationData
                SyncLock m_StationList
                    For i As Integer = 1 To State
                        If Not m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                            Dim Info As New stationInfo
                            Info.stationID = "HD" + ":" + (m_Radio.CurrentFrequency * 100) + ":" + i
                            Info.stationName = m_Radio.CurrentFormattedChannel & "HD-" & i
                            Info.Bitrate = 0
                            Info.Channels = 2
                            Info.stationGenre = ""
                            Info.signal = 1
                            m_StationList.Add(m_Radio.CurrentFrequency & ":" & i, Info)
                            m_SubStationData.Add(i, New SubChannelData)
                        End If
                    Next

                    ' I previously skipped this test.  I don't remember why
                    If m_StationList.Count = getStatus(m_CurrentInstance).stationList.Length Then
                        RaiseMediaEvent(eFunction.stationListUpdated, "")
                    End If

                    'RaiseMediaEvent(eFunction.stationListUpdated, "")

                End SyncLock
            End SyncLock
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventCommError() Handles m_Radio.HDRadioEventCommError

        m_Host.DebugMsg(OpenMobile.DebugMessageType.Error, "OMVisteonRadio - m_Radio_HDRadioEventCommError()", String.Format("HD Radio communication error."))
        m_Radio.Close()
        m_CommError = True

    End Sub

    Private Sub m_Radio_HDRadioEventSysErrorRadioNotFound() Handles m_Radio.HDRadioEventSysErrorRadioNotFound
        ' Radio Not Found

        If m_verbose Then
            m_Host.DebugMsg(OpenMobile.DebugMessageType.Info, "OMVisteonRadio", String.Format("Radio not found. Closing port."))
        End If

        m_Radio.Close()
        m_NotFound = True

        ' allow waiting threads to continue
        waitHandle.Set()
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventSysErrorRadioNotFound()", String.Format("waitHandle signalled."))
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventSysPortOpened() Handles m_Radio.HDRadioEventSysPortOpened

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventSysPortOpened()", String.Format("Opened port {0}", m_ComPort))
        End If

        comWaitHandle.Set()

    End Sub

    Private Sub m_Radio_HDRadioEventHWPoweredOn() Handles m_Radio.HDRadioEventHWPoweredOn

        Dim sLastPlaying As String = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastPlaying")
        Dim sFormatted As String = ""

        m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("HD Radio Powered on."))
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Last Playing: {0}", sLastPlaying))
        End If

        If m_Radio.CurrentFrequency = 0 Then
            ' default set to my regular station
            sFormatted = "FM:105700"
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Radio not tuned, using {0}.", sFormatted))
            End If
        Else
            sFormatted = m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Radio already tuned to {0}.", sFormatted))
            End If
        End If

        m_Radio.SetVolume(&H4B)

        If Not String.IsNullOrEmpty(sLastPlaying) Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Tuning to LastPlaying ({0}, {1}).", m_CurrentInstance, sLastPlaying))
            End If
            tuneTo(m_CurrentInstance, sLastPlaying)
        Else
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Tuning to CurrentFormattedChannel ({0}).", sFormatted))
            End If
            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.LastPlaying", sFormatted)
            tuneTo(m_CurrentInstance, sFormatted)
        End If

        waitHandle.Set()
        pwrWaitHandle.Set()

    End Sub

    Private Sub m_Radio_HDRadioEventTunerTuned(ByVal Message As String) Handles m_Radio.HDRadioEventTunerTuned

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), String.Format("Tuner tuned to {0}.", m_Radio.CurrentFormattedChannel))
        End If

        If m_Radio.CurrentFrequency > 100 Then

            m_CurrentMedia = New mediaInfo
            m_CurrentMedia.Name = m_Radio.CurrentFormattedChannel
            m_CurrentMedia.Artist = " "
            m_CurrentMedia.Album = " "

            SyncLock m_SubStationData
                m_SubStationData.Clear()
            End SyncLock

            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), "Raising media event: tunerDataUpdated")
            End If
            RaiseMediaEvent(eFunction.tunerDataUpdated, "")
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")

            If m_StationList.Count = getStatus(m_CurrentInstance).stationList.Length Then
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), "Raising media event: stationListUpdated")
                End If
                RaiseMediaEvent(eFunction.stationListUpdated, "")
            End If

            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.LastPlaying", m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100)
            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.Last" & m_Radio.CurrentBand.ToString, m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100)

        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSGenre(ByVal Message As String) Handles m_Radio.HDRadioEventRDSGenre

        If Not m_CurrentMedia.Genre = Message Then
            m_CurrentMedia.Genre = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSGenre({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")
        End If

        SyncLock m_StationList
            If m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                If Not m_StationList(m_Radio.CurrentFrequency).stationGenre = Message Then
                    m_StationList(m_Radio.CurrentFrequency).stationGenre = Message
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSGenre({0})", Message), "Raising media event: stationListUpdated")
                    End If
                    RaiseMediaEvent(eFunction.stationListUpdated, "")
                End If
            End If
        End SyncLock

    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramIdentification(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramIdentification
        ' PI for station identification

        If Not m_CurrentMedia.Album = Message Then
            m_CurrentMedia.Album = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSProgramIdentification({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramService(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramService
        ' PS 8 character Call leters or ID

        If Not m_CurrentMedia.Album = Message Then
            m_CurrentMedia.Album = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSProgramIdentification({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSRadioText(ByVal Message As String) Handles m_Radio.HDRadioEventRDSRadioText
        ' RT 64 character free form text

        If Not m_CurrentMedia.Artist = Message Then
            m_CurrentMedia.Artist = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSRadioText({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventHDActive(ByVal State As Integer) Handles m_Radio.HDRadioEventHDActive

        Dim IsHD As Boolean = (State = 1)
        SyncLock m_StationList
            If m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                If Not m_StationList(m_Radio.CurrentFrequency).HD = IsHD Then
                    SyncLock m_StationList
                        m_StationList(m_Radio.CurrentFrequency).HD = IsHD
                    End SyncLock
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventHDActive({0})", State), "Raising media event: tunerDataUpdated")
                    End If
                    RaiseMediaEvent(eFunction.tunerDataUpdated, "")
                End If
            End If
        End SyncLock

    End Sub

    Private Sub m_Radio_HDRadioEventHDCallSign(ByVal Message As String) Handles m_Radio.HDRadioEventHDCallSign

        If Not m_StationList(m_Radio.CurrentFrequency).stationName = Message Then
            SyncLock m_StationList
                m_StationList(m_Radio.CurrentFrequency).stationName = Message
            End SyncLock
            m_HDCallSign = Message
            m_CurrentMedia.Name = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventHDCallSign({0})", Message), "Raising media event: Play")
            End If
            RaiseMediaEvent(eFunction.Play, "")
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventHDArtist(ByVal Message As String) Handles m_Radio.HDRadioEventHDArtist

        SyncLock m_SubStationData
            Dim Data() As String = Message.Split("|")

            If Data.Length = 2 Then
                Dim SubChan As Integer = Integer.Parse(Data(0))

                If Not m_SubStationData.ContainsKey(SubChan) Then
                    m_SubStationData.Add(SubChan, New SubChannelData)
                End If

                m_SubStationData(SubChan).Artist = Data(1)

                If SubChan = m_Radio.CurrentHDSubChannel Then
                    m_CurrentMedia.Artist = m_SubStationData(SubChan).Artist
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventHDArtist({0})", Message), "Raising media event: Play")
                    End If
                    RaiseMediaEvent(eFunction.Play, "")
                End If
            End If

        End SyncLock

    End Sub

    Private Sub m_Radio_HDRadioEventHDTitle(ByVal Message As String) Handles m_Radio.HDRadioEventHDTitle

        SyncLock m_SubStationData
            Dim Data() As String = Message.Split("|")

            If Data.Length = 2 Then
                Dim SubChan As Integer = Integer.Parse(Data(0))

                If Not m_SubStationData.ContainsKey(SubChan) Then
                    m_SubStationData.Add(SubChan, New SubChannelData)
                End If

                m_SubStationData(SubChan).Title = Data(1)
                m_SubStationData(SubChan).Station = m_HDCallSign + " HD-" + SubChan.ToString

                If SubChan = m_Radio.CurrentHDSubChannel Then
                    m_CurrentMedia.Name = m_SubStationData(SubChan).Station
                    m_CurrentMedia.Album = m_SubStationData(SubChan).Title
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventHDTitle({0})", Message), "Raising media event: Play")
                    End If
                    RaiseMediaEvent(eFunction.Play, "")
                End If
            End If

        End SyncLock

    End Sub

    Private Sub m_Radio_HDRadioEventTunerSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventTunerSignalStrength
        UpdateSignal(State, 3)
    End Sub

    Private Sub m_Radio_HDRadioEventHDSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSignalStrength
        UpdateSignal(State, 600)
    End Sub

    Private Sub UpdateSignal(ByVal State As Integer, ByVal DivideBy As Integer)

        State /= DivideBy
        If State = 0 Then
            State = 1
        ElseIf State > 5 Then
            State = 5
        End If

        If Not m_StationList(m_Radio.CurrentFrequency).signal = State Then
            m_StationList(m_Radio.CurrentFrequency).signal = State
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - UpdateSignal({0}, {1})", State, DivideBy), "Raising media event: tunerDataUpdated")
            End If
            RaiseMediaEvent(eFunction.tunerDataUpdated, "")
        End If
    End Sub

    Private Sub RaiseMediaEvent(ByVal Fun As eFunction, ByVal arg As String)
        'Just for now
        'For Each z As Object In m_Host.ZoneHandler.
        RaiseEvent OnMediaEvent(Fun, m_CurrentInstance, arg)
        'Next
    End Sub

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function getVolume(ByVal zone As OpenMobile.Zone) As Integer Implements OpenMobile.Plugin.IPlayerVolumeControl.getVolume
        Return m_Audio.getVolume(zone.ID)
    End Function

    Public Function setVolume(ByVal zone As OpenMobile.Zone, ByVal percent As Integer) As Boolean Implements OpenMobile.Plugin.IPlayerVolumeControl.setVolume
        m_Audio.setVolume(zone.AudioDevice.Instance, percent)
    End Function

    'Pre powering the radio on and off so return true
    Private Function m_Audio_PowerOffHardware() As Boolean Handles m_Audio.PowerOffHardware
        Return True
    End Function

    Private Function m_Audio_PowerOnHardware() As Boolean Handles m_Audio.PowerOnHardware
        Return True
    End Function

    Public Function SetVideoVisible(ByVal zone As OpenMobile.Zone, ByVal visible As Boolean) As Boolean Implements OpenMobile.Plugin.IPlayer.SetVideoVisible
        Return False
    End Function

    Public ReadOnly Property playbackPosition() As Integer Implements OpenMobile.Plugin.ITunedContent.playbackPosition
        Get
            Return 0
        End Get
    End Property

    Public ReadOnly Property authorEmail() As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName() As String Implements OpenMobile.Plugin.IBasePlugin.authorName
        Get
            Return "Jon Heizer / John Mullan"
        End Get
    End Property

    Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "HD Radio"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "HD Radio"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public ReadOnly Property pluginIcon() As imageItem Implements OpenMobile.Plugin.IBasePlugin.pluginIcon
        Get
            Return OM.Host.getPluginImage(Me, "Icon-Radio")
        End Get
    End Property

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
                Try
                    m_Radio.PowerOff()
                    m_Audio.Suspend()
                    m_Audio.Dispose()
                    m_Radio.Close()
                Catch ex As Exception
                End Try
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

    Private Class SubChannelData
        Public Station As String
        Public Artist As String
        Public Title As String
    End Class

End Class

