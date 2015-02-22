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
Imports OpenMobile.Media
Imports HDRadioComm
Imports System.Collections.Generic

Public Class RadioComm
    Inherits MediaProviderBase

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

    Private m_MediaSources As New List(Of MediaSource)
    Private m_Radio_FM_MediaSource As MediaSource_TunedContent
    Private m_Radio_AM_MediaSource As MediaSource_TunedContent

    Private m_LastAMstation As Integer = 88100
    Private m_LastFMstation As Integer = 105700

    Private waitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)
    Private comWaitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)
    Private pwrWaitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)

    Public Sub New()

        MyBase.New("OMVisteonRadio", OM.Host.getSkinImage("Radio"), 2.0, "Visteon/Directed Radio Hardware Interface", "jmullan", "jmullan99@gmail.com")

    End Sub

    Public Overrides Function initialize(host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus
        If (Not Configuration.RunningOnWindows) Then
            Return eLoadStatus.LoadFailedGracefulUnloadRequested
        End If

        m_Host = host

        m_verbose = GetSetting("OMVisteonRadio.VerboseDebug", False)
        m_ComPort = GetSetting("OMVisteonRadio.ComPort", m_ComPort)

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Initializing..."))
        End If

        ' Register media sources 
        m_Radio_AM_MediaSource = New MediaSource_TunedContent(Me, "AM", "AM Radio", OM.Host.getSkinImage("Radio"))
        m_MediaSources.Add(m_Radio_AM_MediaSource)
        m_Radio_FM_MediaSource = New MediaSource_TunedContent(Me, "FM", "FM Radio", OM.Host.getSkinImage("Radio"))
        m_MediaSources.Add(m_Radio_FM_MediaSource)

        Array.Sort(available_ports)

        m_Host.UIHandler.AddNotification(myNotification)
        myNotification.Text = "Plugin initializing..."

        'If m_verbose Then
        'm_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Invoking background loading..."))
        'End If
        'SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

        find_radio()

        If Not m_Radio.IsOpen Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Unloading..."))
            End If
            Return eLoadStatus.LoadFailedGracefulUnloadRequested
        End If
        If Not m_Radio.IsPowered Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Unloading..."))
            End If
            Return eLoadStatus.LoadFailedGracefulUnloadRequested
        End If

        Return eLoadStatus.LoadSuccessful


    End Function

    Private Sub BackgroundLoad()

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Background initialization."))
        End If

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

    Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String

        If String.IsNullOrEmpty(helperFunctions.StoredData.Get(Me, Name)) Then
            helperFunctions.StoredData.Set(Me, Name, DefaultValue)
        End If
        Return helperFunctions.StoredData.Get(Me, Name)

    End Function

    Private Sub find_radio()

        Dim access_granted As Boolean = False
        Dim starttime As DateTime

        'Dim mediaProviders As OpenMobile.Plugin.IBasePlugin = OM.Host.MediaProviderHandler.MediaProviders

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Locating radio."))
        End If

        m_Radio.AutoSearch = False

        If String.IsNullOrEmpty(m_ComPort) Then
            m_ComPort = "Auto"
        End If

        If m_ComPort <> "Auto" Then
            ' Try the user specified COM port
            myNotification.Text = String.Format("Waiting for lock on {0}", m_ComPort)
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Searching on last used port {0}.", m_ComPort))
            End If
            ' Try a couple times to get serial access
            For x = 1 To m_Retries
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - find_radio()", String.Format("Waiting for serial access lock."))
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

    Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

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
            Case "OMVisteonRadio.LastAMstation"
            Case "OMVisteonRadio.LastFMstation"
        End Select

    End Sub

    Public Function tuneTo(ByVal zone As OpenMobile.Zone, ByVal station As String) As Boolean

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

                If Chan(0) = "FM" OrElse Chan(0) = "HD" Then
                    m_LastFMstation = Chan(1)
                    helperFunctions.StoredData.Set(Me, "OMVisteonRadio.LastFMstation", Chan(1))
                Else
                    helperFunctions.StoredData.Set(Me, "OMVisteonRadio.LastAMstation", Chan(1))
                End If

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

    Public Overrides Function MediaProviderCommand_Activate(zone As OpenMobile.Zone) As String
        ' Radio is turned on at plugin initialize

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Activate()", String.Format("Invoking background loading..."))
        End If

        If Not m_Radio.IsPowered Then
            ' Radio is not already on
            If m_Radio.IsOpen Then
                ' But it IS connected, so try to turn it on
                m_Radio.PowerOn()
                If pwrWaitHandle.WaitOne(4000) = False Then
                    ' Timed out waiting for radio to power on
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Activate()", String.Format("HDRadio did not power on."))
                    End If
                    Return "HDRadio did not power on"
                End If
            Else
                ' We are not even connected to a radio, so
                find_radio()
            End If
        End If

        If Not m_Radio.IsPowered Then
            ' Not connected to a radio
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Activate()", String.Format("HDRadio did not power on."))
            Return "HDRadio did not power on"
        End If

        ' Radio is on
        ' Ensure the audio is not muted
        m_Radio.MuteOff()

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_ActivateMediaSource(zone As OpenMobile.Zone, mediaSourceName As String) As String
        ' Select band

        If mediaSourceName = "AM" Then
            ' helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastAMstation")
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting AM band."))
            End If
            m_Radio.TuneToChannel(0, HDRadio.HDRadioBands.AM)
        ElseIf mediaSourceName = "FM" Or mediaSourceName = "HD" Then
            ' helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastFMstation")
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting FM band."))
            End If
            m_Radio.TuneToChannel(0, HDRadio.HDRadioBands.FM)
        Else
            ' helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastFMstation")
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting FM band by default."))
            End If
            m_Radio.TuneToChannel(0, HDRadio.HDRadioBands.FM)
            End If

            Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Deactivate(zone As OpenMobile.Zone) As String
        ' Turns radio off (we actually leave radio powered up, just mute the output

        If m_Radio.IsPowered Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Deactivate()", String.Format("Deselecting HDRadio."))
            End If
            m_Radio.MuteOn()
        End If

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_Next(zone As OpenMobile.Zone, param() As Object) As String
        ' Next preset
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Next()", String.Format("Select Next Preset."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Pause(zone As OpenMobile.Zone, param() As Object) As String
        ' Radio mute toggle
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Pause()", String.Format("Select Pause/Mute."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Previous(zone As OpenMobile.Zone, param() As Object) As String
        ' Previous preset
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Previous()", String.Format("Select Previous Preset."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekBackward(zone As OpenMobile.Zone, param() As Object) As String
        ' Previous available channel
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_SeekBackward()", String.Format("Seek Back to previous live channel."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekForward(zone As OpenMobile.Zone, param() As Object) As String
        ' Next available channel
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_SeekForward()", String.Format("Seek forward to next live channel."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Start(zone As OpenMobile.Zone, param() As Object) As String
        ' Un-mute radio
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Start()", String.Format("Unmute radio playback."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Stop(zone As OpenMobile.Zone, param() As Object) As String
        ' Mute radio
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Stop()", String.Format("Mute radio playback."))
        End If
        Return ""
    End Function

    Public Overrides Function MediaProviderData_GetMediaSource(zone As OpenMobile.Zone) As OpenMobile.Media.MediaSource
        ' Return current Band
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSource()", String.Format("Report on current band."))
        End If
        Return m_Radio_FM_MediaSource
    End Function

    Public Overrides Function MediaProviderData_GetMediaSourceAbilities(mediaSource As String) As OpenMobile.Media.MediaProviderAbilities
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSourceAbilities()", String.Format("Report on our capabilities."))
        End If
        Return Nothing
    End Function

    Public Overrides Function MediaProviderData_GetMediaSources() As List(Of OpenMobile.Media.MediaSource)
        ' Return available bands
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSources()", String.Format("Report on available bands."))
        End If
        Return m_MediaSources
    End Function

    Public Overrides Function MediaProviderData_GetPlaylist(zone As OpenMobile.Zone) As OpenMobile.Media.Playlist
        ' Return current Presets/Live
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Return playlist (presets/live)."))
        End If
        Return Nothing
    End Function

    Public Overrides Function MediaProviderData_GetRepeat(zone As OpenMobile.Zone) As Boolean

    End Function

    Public Overrides Function MediaProviderData_GetShuffle(zone As OpenMobile.Zone) As Boolean

    End Function

    Public Overrides Function MediaProviderData_SetPlaylist(zone As OpenMobile.Zone, playlist As OpenMobile.Media.Playlist) As OpenMobile.Media.Playlist
        ' Accept playlist (presets)
        Return Nothing
    End Function

    Public Overrides Function MediaProviderData_SetRepeat(zone As OpenMobile.Zone, state As Boolean) As Boolean
        ' N/A
        Return False
    End Function

    Public Overrides Function MediaProviderData_SetShuffle(zone As OpenMobile.Zone, state As Boolean) As Boolean
        ' N/A
        Return False
    End Function

    Public Function getStatus(ByVal zone As OpenMobile.Zone) As OpenMobile.tunedContentInfo

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

    Public Function getStationInfo(ByVal zone As OpenMobile.Zone) As OpenMobile.stationInfo

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

    Public Function getMediaInfo(ByVal zone As OpenMobile.Zone) As OpenMobile.mediaInfo
        Return m_CurrentMedia
    End Function

    Public Function getStationList(ByVal zone As OpenMobile.Zone) As Object
        Return m_StationList
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
                m_CurrentMedia.Name = m_HDCallSign & " HD-" & m_Radio.CurrentHDSubChannel
            End If

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
                        'RaiseMediaEvent(eFunction.stationListUpdated, "")
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

            If m_StationList.Count = getStatus(m_CurrentInstance).stationList.Length Then
                If m_verbose Then
                    m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), "Raising media event: stationListUpdated")
                End If
            End If

            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.LastPlaying", m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100)
            helperFunctions.StoredData.Set(Me, "OMVisteonRadio.Last" & m_Radio.CurrentBand.ToString, m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100)
            helperFunctions.StoredData.Set(Me, String.Format("OMVisteonRadio.Last{0}station", m_Radio.CurrentBand), m_Radio.CurrentFrequency * 100)

        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSGenre(ByVal Message As String) Handles m_Radio.HDRadioEventRDSGenre

        If Not m_CurrentMedia.Genre = Message Then
            m_CurrentMedia.Genre = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSGenre({0})", Message), "Raising media event: Play")
            End If
        End If

        SyncLock m_StationList
            If m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                If Not m_StationList(m_Radio.CurrentFrequency).stationGenre = Message Then
                    m_StationList(m_Radio.CurrentFrequency).stationGenre = Message
                    If m_verbose Then
                        m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSGenre({0})", Message), "Raising media event: stationListUpdated")
                    End If
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
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramService(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramService
        ' PS 8 character Call leters or ID

        If Not m_CurrentMedia.Album = Message Then
            m_CurrentMedia.Album = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSProgramIdentification({0})", Message), "Raising media event: Play")
            End If
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventRDSRadioText(ByVal Message As String) Handles m_Radio.HDRadioEventRDSRadioText
        ' RT 64 character free form text

        If Not m_CurrentMedia.Artist = Message Then
            m_CurrentMedia.Artist = Message
            If m_verbose Then
                m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventRDSRadioText({0})", Message), "Raising media event: Play")
            End If
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
                End If
            End If

        End SyncLock

    End Sub

    Private Sub m_Radio_HDRadioEventTunerSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventTunerSignalStrength

    End Sub

    Private Sub m_Radio_HDRadioEventHDSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSignalStrength

    End Sub

    Private Class SubChannelData
        Public Station As String
        Public Artist As String
        Public Title As String
    End Class

End Class
