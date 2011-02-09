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
Imports OpenMobile.Controls
Imports OpenMobile.Plugin
Imports OpenMobile.Framework
Imports HDRadioComm
Imports OpenMobile.Threading

Public Class RadioComm
    Implements OpenMobile.Plugin.ITunedContent

    Private WithEvents m_Host As IPluginHost

    Private WithEvents m_Radio As New HDRadio
    Private m_ComPort As Integer = 0
    Private m_SourceDevice As Integer = 0
    Private m_Fade As Boolean = True
    Private m_RouteVolume As Integer = 100

    Private WithEvents m_Audio As AudioRouter.AudioManager

    Private WithEvents m_Settings As Settings

    Private m_StationList As New Generic.SortedDictionary(Of Integer, stationInfo)
    Private m_CurrentMedia As New mediaInfo
    Private m_CurrentInstance As Integer = 0
    Private m_IsScanning As Boolean = False

    Public Event MediaEvent(ByVal [function] As OpenMobile.eFunction, ByVal instance As Integer, ByVal arg As String) Implements OpenMobile.Plugin.IPlayer.OnMediaEvent

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host
        SafeThread.Asynchronous(AddressOf BackgroundLoad, host)
        Return eLoadStatus.LoadSuccessful
    End Function

    Private Sub BackgroundLoad()
        Try
            LoadRadioSettings()

            'Check to see if the source audio device still exists, if not switch to default
            If Not Array.IndexOf(AudioRouter.AudioRouter.listSources, m_SourceDevice) > -1 Then
                m_SourceDevice = 0
            End If

            m_Audio = New AudioRouter.AudioManager(m_SourceDevice)
            m_Audio.FadeIn = m_Fade
            m_Audio.FadeOut = m_Fade

            If m_ComPort > 0 Then
                m_Radio.AutoSearch = False
                m_Radio.ComPort = m_ComPort
            Else
                m_Radio.AutoSearch = True
            End If

            m_Radio.Open()

        Catch ex As Exception
            m_Host.sendMessage("OMDebug", ex.Source, ex.ToString)

        End Try
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

        If m_Settings Is Nothing Then
            LoadRadioSettings()

            m_Settings = New Settings("HD Radio")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.Add("Auto")
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            Dim COMValues As New Generic.List(Of String)
            For Each Port As String In COMOptions
                If Port = "Auto" Then
                    COMValues.Add("0")
                Else
                    COMValues.Add(Port.Replace("COM", ""))
                End If
            Next

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.ComPort", "COM", "Com Port", COMOptions, COMValues, m_ComPort.ToString))

            Dim SourceOptions As New Generic.List(Of String)
            SourceOptions.AddRange(AudioRouter.AudioRouter.listSources)
            Dim SourceValues As New Generic.List(Of String)
            For i As Integer = 0 To SourceOptions.Count - 1
                SourceValues.Add(i)
            Next

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.SourceDevice", "Input", "Input Source", SourceOptions, SourceValues, m_SourceDevice))

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.FadeAudio", "Fade", "Fade Audio", Setting.BooleanList, Setting.BooleanList, m_Fade))

            Dim Range As New Generic.List(Of String)
            Range.Add("0")
            Range.Add("100")
            m_Settings.Add(New Setting(SettingTypes.Range, "OMVisteonRadio.AudioVolume", "Input Vol", "(0-100%)", Nothing, Range, m_RouteVolume))


            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)
        Using PlugSet As New OpenMobile.Data.PluginSettings
            PlugSet.setSetting(St.Name, St.Value)
        End Using
        LoadRadioSettings()
    End Sub

    Private Sub LoadRadioSettings()
        m_ComPort = GetSetting("OMVisteonRadio.ComPort", 0)
        m_SourceDevice = GetSetting("OMVisteonRadio.SourceDevice", 0)
        m_Fade = GetSetting("OMVisteonRadio.FadeAudio", True)
        m_RouteVolume = GetSetting("OMVisteonRadio.AudioVolume", 100)
    End Sub

    Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting(Name)) Then
                Settings.setSetting(Name, DefaultValue)
            End If
            Return Settings.getSetting(Name)
        End Using
    End Function

    Public Function setPowerState(ByVal instance As Integer, ByVal powerState As Boolean) As Boolean Implements OpenMobile.Plugin.ITunedContent.setPowerState
        Try

            If m_Audio Is Nothing Then
                Return False
            End If
            m_Audio.setVolume(instance, m_RouteVolume)
            m_Audio.setZonePower(instance, powerState)

            If powerState Then
                'Hardware powers on ahead of time since it is slow...
                Return True
            Else
                If m_Audio.ActiveInstances.Length > 0 Then
                    Return True
                End If

                Using St As New OpenMobile.Data.PluginSettings
                    St.setSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString, "")
                End Using

                m_Radio.MuteOn()
                m_Radio.PowerOff()

                Return True
            End If

        Catch ex As Exception
            m_Host.sendMessage("OMDebug", ex.Source, ex.ToString)
            Return False
        End Try
    End Function

    Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles m_Host.OnPowerChange
        Select Case type
            Case Is = ePowerEvent.SystemResumed
                'Give things a second to get settled
                System.Threading.Thread.Sleep(2000)

                If Not m_Radio Is Nothing Then
                    If Not m_Radio.IsOpen Then
                        m_Radio.Open()
                    Else
                        'Open will power on.  This is only if the com port is open but the radio is off
                        If Not m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
                            m_Radio.PowerOn()
                        End If
                    End If
                End If

                Using St As New OpenMobile.Data.PluginSettings
                    If Not String.IsNullOrEmpty(St.getSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString)) Then
                        tuneTo(m_CurrentInstance, St.getSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString))
                    End If
                End Using

                If Not m_Audio Is Nothing Then
                    m_Audio.Resume()
                End If
        
            Case Is = ePowerEvent.SleepOrHibernatePending
                If Not m_Audio Is Nothing Then
                    m_Audio.Suspend()
                End If

        End Select
    End Sub

    Public Function getStatus(ByVal instance As Integer) As OpenMobile.tunedContentInfo Implements OpenMobile.Plugin.ITunedContent.getStatus
        Dim tc As New tunedContentInfo

        Select Case m_Radio.PowerState
            Case Is = HDRadio.PowerStatus.PowerOn
                tc.status = eTunedContentStatus.Tuned
            Case Else
                tc.status = eTunedContentStatus.Unknown
        End Select

        tc.band = CInt(m_Radio.CurrentBand) + 1
        If m_Radio.IsHDActive Then
            tc.band = eTunedContentBand.HD
        End If

        tc.powerState = True
        tc.channels = 2

        tc.currentStation = getStationInfo(instance)
        tc.stationList = getStationList(instance)

        Return tc
    End Function

    Public Function getStationInfo(ByVal instance As Integer) As OpenMobile.stationInfo Implements OpenMobile.Plugin.ITunedContent.getStationInfo
        Dim Info As stationInfo

        If m_Radio.CurrentFrequency > 100 Then

            If Not m_StationList.ContainsKey(m_Radio.CurrentFrequency) Then
                Info = New stationInfo
                Info.stationID = m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100
                Info.Bitrate = 0
                Info.Channels = 2
                Info.stationGenre = ""
                Info.signal = 1
                m_StationList.Add(m_Radio.CurrentFrequency, Info)
            End If

            Info = m_StationList(m_Radio.CurrentFrequency)
            Info.HD = m_Radio.IsHDActive
            Info.stationName = m_Radio.CurrentFormattedChannel

            Return Info
        End If

        Return New stationInfo
    End Function

    Public Function getMediaInfo(ByVal instance As Integer) As OpenMobile.mediaInfo Implements OpenMobile.Plugin.IPlayer.getMediaInfo
        Return m_CurrentMedia
    End Function

    Public Function getStationList(ByVal instance As Integer) As OpenMobile.stationInfo() Implements OpenMobile.Plugin.ITunedContent.getStationList
        Dim info(m_StationList.Values.Count) As stationInfo
        m_StationList.Values.CopyTo(info, 0) 'Faster then ToArray :)
        Return info
    End Function

    Public Function tuneTo(ByVal instance As Integer, ByVal station As String) As Boolean Implements OpenMobile.Plugin.ITunedContent.tuneTo
        Dim Chan() As String
        Dim Band As HDRadioComm.HDRadio.HDRadioBands = HDRadio.HDRadioBands.FM
        Dim Freq As Decimal = 0
        Dim SubChan As Integer

        If station.Length > 0 Then
            m_CurrentInstance = instance

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
                    System.Threading.Thread.Sleep(500)
                    If m_Radio.HDSubChannelCount > 0 Then
                        If Array.Exists(m_Radio.HDSubChannelList, Function(p) p = SubChan) Then
                            m_Radio.HDSubChannelSelect(SubChan)
                        End If
                    End If
                End If

                Return True
            End If
        End If

        Return False
    End Function

    Public Function stepBackward(ByVal instance As Integer) As Boolean Implements OpenMobile.Plugin.ITunedContent.stepBackward
        m_CurrentInstance = instance
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.TuneDown()
        End If
        Return True
    End Function

    Public Function stepForward(ByVal instance As Integer) As Boolean Implements OpenMobile.Plugin.ITunedContent.stepForward
        m_CurrentInstance = instance
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.TuneUp()
        End If
        Return True
    End Function

    Public Function scanForward(ByVal instance As Integer) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanForward
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.SeekUp(HDRadio.HDRadioSeekType.ALL)
        End If
        Return True
    End Function

    Public Function scanReverse(ByVal instance As Integer) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanReverse
        m_CurrentInstance = instance
        If m_Radio.PowerState = HDRadio.PowerStatus.PowerOn Then
            m_Radio.SeekDown(HDRadio.HDRadioSeekType.ALL)
        End If
        Return True
    End Function

    Public Function scanBand(ByVal instance As Integer) As Boolean Implements OpenMobile.Plugin.ITunedContent.scanBand
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

    Public Function setBand(ByVal instance As Integer, ByVal band As OpenMobile.eTunedContentBand) As Boolean Implements OpenMobile.Plugin.ITunedContent.setBand
        m_CurrentInstance = instance
        If band = eTunedContentBand.AM Then
            Return tuneTo(instance, "AM:53000")
        ElseIf band = eTunedContentBand.FM OrElse band = eTunedContentBand.HD Then
            Return tuneTo(instance, "FM:88100")
        End If
    End Function

    Public Function getSupportedBands(ByVal instance As Integer) As OpenMobile.eTunedContentBand() Implements OpenMobile.Plugin.ITunedContent.getSupportedBands
        Return New eTunedContentBand() {eTunedContentBand.AM, eTunedContentBand.FM, eTunedContentBand.HD}
    End Function

    Private Sub m_Radio_HDRadioEventHDSubChannelCount(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSubChannelCount
        If State > 1 Then
            For i As Integer = 1 To State
                If Not m_StationList.ContainsKey(m_Radio.CurrentFrequency & "-" & i) Then
                    Dim Info As New stationInfo
                    Info.stationID = m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100 & ":" & State
                    Info.Bitrate = 0
                    Info.Channels = 2
                    Info.stationGenre = ""
                    Info.signal = 1
                    m_StationList.Add(m_Radio.CurrentFrequency & "-" & i, Info)
                End If
            Next
            RaiseMediaEvent(eFunction.stationListUpdated, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventSysPortOpened() Handles m_Radio.HDRadioEventSysPortOpened
        m_Radio.PowerOn()
        Using Settings As New OpenMobile.Data.PluginSettings
            Settings.setSetting("OMVisteonRadio.ComPort", m_Radio.ComPort)
        End Using
    End Sub

    Private Sub m_Radio_HDRadioEventHWPoweredOn() Handles m_Radio.HDRadioEventHWPoweredOn
        m_Radio.SetVolume(&H4B)

        Using St As New OpenMobile.Data.PluginSettings
            If Not String.IsNullOrEmpty(St.getSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString)) Then
                tuneTo(m_CurrentInstance, St.getSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString))
            End If
        End Using
    End Sub

    Private Sub m_Radio_HDRadioEventTunerTuned(ByVal Message As String) Handles m_Radio.HDRadioEventTunerTuned
        If m_Radio.CurrentFrequency > 100 Then
            m_CurrentMedia = New mediaInfo
            m_CurrentMedia.Name = m_Radio.CurrentFormattedChannel
            m_CurrentMedia.Artist = " "
            m_CurrentMedia.Album = " "

            RaiseMediaEvent(eFunction.tunerDataUpdated, "")
            RaiseMediaEvent(eFunction.Play, "")

            If m_StationList.Count = getStatus(m_CurrentInstance).stationList.Length Then
                RaiseMediaEvent(eFunction.stationListUpdated, "")
            End If

            Using St As New OpenMobile.Data.PluginSettings
                St.setSetting("OMVisteonRadio.LastPlaying" & m_CurrentInstance.ToString, m_Radio.CurrentBand.ToString & ":" & m_Radio.CurrentFrequency * 100)
            End Using
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventRDSGenre(ByVal Message As String) Handles m_Radio.HDRadioEventRDSGenre
        If Not m_CurrentMedia.Genre = Message Then
            m_CurrentMedia.Genre = Message
            RaiseMediaEvent(eFunction.Play, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramIdentification(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramIdentification
        If Not m_CurrentMedia.Album = Message Then
            m_CurrentMedia.Album = Message
            RaiseMediaEvent(eFunction.Play, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramService(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramService
        'Changing/scrolling song info etc
        'If Not m_CurrentMedia.Artist = Message Then
        '    m_CurrentMedia.Artist = Message
        '    RaiseMediaEvent(eFunction.Play, "")
        'End If
    End Sub

    Private Sub m_Radio_HDRadioEventRDSRadioText(ByVal Message As String) Handles m_Radio.HDRadioEventRDSRadioText
        If Not m_CurrentMedia.Artist = Message Then
            m_CurrentMedia.Artist = Message
            RaiseMediaEvent(eFunction.Play, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventHDActive(ByVal State As Integer) Handles m_Radio.HDRadioEventHDActive
        Dim IsHD As Boolean = (State = 1)
        If Not m_StationList(m_Radio.CurrentFrequency).HD = IsHD Then
            m_StationList(m_Radio.CurrentFrequency).HD = IsHD
            RaiseMediaEvent(eFunction.tunerDataUpdated, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventHDCallSign(ByVal Message As String) Handles m_Radio.HDRadioEventHDCallSign
        If m_StationList(m_Radio.CurrentFrequency).stationName = Message Then
            m_StationList(m_Radio.CurrentFrequency).stationName = Message
            RaiseMediaEvent(eFunction.Play, "")
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventHDArtist(ByVal Message As String) Handles m_Radio.HDRadioEventHDArtist
        'Don't have any stations with this to test it with
        'If Not m_CurrentMedia.Artist = Message Then
        '    m_CurrentMedia.Artist = Message
        '    RaiseMediaEvent(eFunction.Play, "")
        'End If
    End Sub

    Private Sub m_Radio_HDRadioEventTunerSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventTunerSignalStrength
        UpdateSignal(State, 5)
    End Sub

    Private Sub m_Radio_HDRadioEventHDSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSignalStrength
        UpdateSignal(State, 819)
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
            RaiseMediaEvent(eFunction.tunerDataUpdated, "")
        End If
    End Sub

    Private Sub RaiseMediaEvent(ByVal Fun As eFunction, ByVal arg As String)
        For Each Inst As Integer In m_Audio.ActiveInstances
            RaiseEvent MediaEvent(Fun, Inst, arg)
        Next
    End Sub

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public ReadOnly Property OutputDevices() As String() Implements OpenMobile.Plugin.IPlayer.OutputDevices
        Get
            Return AudioRouter.AudioRouter.listZones
        End Get
    End Property

    Public Function getVolume(ByVal instance As Integer) As Integer Implements OpenMobile.Plugin.IPlayer.getVolume
        Return m_Audio.getVolume(instance)
    End Function

    Public Function setVolume(ByVal instance As Integer, ByVal percent As Integer) As Boolean Implements OpenMobile.Plugin.IPlayer.setVolume
        m_Audio.setVolume(instance, percent)
    End Function

    'Pre powering the radio on and off so return true
    Private Function m_Audio_PowerOffHardware() As Boolean Handles m_Audio.PowerOffHardware
        Return True
    End Function

    Private Function m_Audio_PowerOnHardware() As Boolean Handles m_Audio.PowerOnHardware
        Return True
    End Function

    Public ReadOnly Property SupportsAdvancedFeatures() As Boolean Implements OpenMobile.Plugin.IPlayer.SupportsAdvancedFeatures
        Get

        End Get
    End Property

    Public Function SetVideoVisible(ByVal instance As Integer, ByVal visible As Boolean) As Boolean Implements OpenMobile.Plugin.IPlayer.SetVideoVisible
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
            Return "Jon Heizer"
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


End Class

