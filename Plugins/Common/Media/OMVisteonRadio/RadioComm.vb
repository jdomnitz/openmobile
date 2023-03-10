
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
    Private m_Fade As Boolean = True
    Private m_verbose As Boolean = False        ' Controls logging of extra info to debug log
    Private m_RouteVolume As Integer = 100

    Private m_Routes As Dictionary(Of Zone, AudioRoute) = New Dictionary(Of Zone, AudioRoute)      ' Active Routes

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
    Private m_CurrentMedia As mediaInfo = New mediaInfo
    Private m_HDCallSign As String = ""
    Private m_IsScanning As Boolean = False
    Private m_CommError As Boolean = False
    Private m_NotFound As Boolean = False
    Private m_InputDevice As String = ""
    Private m_MediaSources As New List(Of MediaSource)
    Private m_DefaultListSource As Integer = 1
    Private textLine1 As String = ""
    Private textLine2 As String = ""
    Private m_artist As String = ""
    Private m_album As String = ""
    Private m_track As String = ""
    Private access_granted As Boolean = False

    ' Refers to the current media source
    Private m_Radio_MediaSource As MediaSource_TunedContent
    Private m_Radio_Live As Playlist = New Playlist
    Private m_Radio_Presets As Playlist = New Playlist

    ' Defines the FM media source
    Private m_Radio_FM_MediaSource As MediaSource_TunedContent
    Private m_Radio_FM_Live As OpenMobile.Media.Playlist = New Playlist(String.Format("{0}.Channels.FM.Live", Me.pluginName), "FM Live")
    Private m_Radio_FM_Presets As OpenMobile.Media.Playlist = New Playlist(String.Format("{0}.Channels.FM.Presets", Me.pluginName), "FM Presets")
    ' Defines the AM media source
    Private m_Radio_AM_MediaSource As MediaSource_TunedContent
    Private m_Radio_AM_Live As OpenMobile.Media.Playlist = New Playlist(String.Format("{0}.Channels.AM.Live", Me.pluginName), "AM Live")
    Private m_Radio_AM_Presets As OpenMobile.Media.Playlist = New Playlist(String.Format("{0}.Channels.AM.Presets", Me.pluginName), "AM Presets")

    Private m_LastAMStation As String = "AM:61000"
    Private m_LastFMStation As String = "FM:105700"
    Private m_LastStation As String = m_LastFMStation

    Private myImage As imageItem = OM.Host.getImageFromFile("Icon-HDRadio")
    Private theZones As List(Of Zone) = New List(Of Zone)

    Public Sub New()

        MyBase.New("OMVisteonRadio", OM.Host.getPluginImage(Of RadioComm)("Icon-HDRadio"), 2.0, "Visteon/Directed Radio Hardware Interface", "jmullan", "jmullan99@gmail.com")

    End Sub

    Public Overrides Function initialize(host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

        If (Not Configuration.RunningOnWindows) Then
            Return eLoadStatus.LoadFailedGracefulUnloadRequested
        End If

        Dim chan() As String
        Dim test As Boolean = False

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Initializing..."))
        End If

        m_Host = host
        m_Host.UIHandler.AddNotification(myNotification)
        myNotification.Text = "Plugin initializing..."

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.VerboseDebug", m_verbose)
        m_verbose = helperFunctions.StoredData.GetBool(Me, "OMVisteonRadio.VerboseDebug")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.ComPort", m_ComPort)
        m_ComPort = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.ComPort")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.SourceDevice", "Default Device")
        m_InputDevice = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.SourceDevice")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.LastFMStation", m_LastFMStation)
        m_LastFMStation = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastFMStation")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.LastAMStation", m_LastAMStation)
        m_LastAMStation = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastAMStation")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.LastPlaying", m_LastStation)
        m_LastStation = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastPlaying")

        helperFunctions.StoredData.SetDefaultValue(Me, "OMVisteonRadio.DefaultList", m_DefaultListSource)
        m_DefaultListSource = helperFunctions.StoredData.Get(Me, "OMVisteonRadio.DefaultList")

        chan = m_LastStation.Split(":")

        ' Register media sources 

        ' Init FM Playlists
        m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Starting to loading items for playlist {0} from DB", m_Radio_FM_Presets.Name))
        If m_Radio_FM_Presets.Load() Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Load successful"))
            End If
        End If
        m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Loaded {1} items for playlist {0} from DB", m_Radio_FM_Presets.Name, m_Radio_FM_Presets.Count))

        ' FM media source
        m_Radio_FM_MediaSource = New MediaSource_TunedContent(Me, "FM", "FM Radio", OM.Host.getPluginImage(Of RadioComm)("Icon-HDRadio"))
        m_Radio_FM_MediaSource.HD = False
        m_Radio_FM_MediaSource.Icon = Nothing
        m_Radio_FM_MediaSource.ProgramText = ""
        m_Radio_FM_MediaSource.ProgramType = ""
        m_Radio_FM_MediaSource.Provider = Me
        m_Radio_FM_MediaSource.SignalBitRate = ""
        m_Radio_FM_MediaSource.SignalStrength = 0
        m_Radio_FM_MediaSource.Stereo = False
        m_Radio_FM_MediaSource.ChannelsLive = m_Radio_FM_Live
        m_Radio_FM_MediaSource.ChannelsPreset = m_Radio_FM_Presets
        m_Radio_FM_MediaSource.ListSource = m_DefaultListSource
        m_MediaSources.Add(m_Radio_FM_MediaSource)

        If chan(0) = "FM" Then
            m_Radio_MediaSource = m_Radio_FM_MediaSource
        End If

        AddHandler m_Radio_FM_MediaSource.OnCommand_SetListSource, AddressOf MediaSource_FM_OnCommand_SetListSource
        AddHandler m_Radio_FM_MediaSource.OnCommand_PresetSet, AddressOf MediaSource_FM_OnCommand_PresetSet
        AddHandler m_Radio_FM_MediaSource.OnCommand_PresetRemove, AddressOf MediaSource_FM_OnCommand_PresetRemove
        AddHandler m_Radio_FM_MediaSource.OnCommand_PresetRename, AddressOf MediaSource_FM_OnCommand_PresetRename
        AddHandler m_Radio_FM_MediaSource.OnCommand_DirectTune, AddressOf MediaSource_FM_OnCommand_DirectTune
        'AddHandler m_Radio_FM_MediaSource.OnCommand_SearchChannels, AddressOf MediaSource_FM_OnCommand_handler

        ' Init AM Playlists
        m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Starting to loading items for playlist {0} from DB", m_Radio_AM_Presets.Name))
        If m_Radio_AM_Presets.Load() Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Load successful"))
            End If
        End If
        m_Host.DebugMsg("OMVisteonRadio - initialize()", String.Format("Loaded {1} items for playlist {0} from DB", m_Radio_AM_Presets.Name, m_Radio_AM_Presets.Count))

        ' AM media source
        m_Radio_AM_MediaSource = New MediaSource_TunedContent(Me, "AM", "AM Radio", OM.Host.getPluginImage(Of RadioComm)("Icon-HDRadio"))
        m_Radio_AM_MediaSource.HD = False
        m_Radio_AM_MediaSource.Icon = Nothing
        m_Radio_AM_MediaSource.ProgramText = ""
        m_Radio_AM_MediaSource.ProgramType = ""
        m_Radio_AM_MediaSource.Provider = Me
        m_Radio_AM_MediaSource.SignalBitRate = ""
        m_Radio_AM_MediaSource.SignalStrength = 0
        m_Radio_AM_MediaSource.Stereo = False
        m_Radio_AM_MediaSource.ChannelsLive = m_Radio_AM_Live
        m_Radio_AM_MediaSource.ChannelsPreset = m_Radio_AM_Presets
        m_Radio_AM_MediaSource.ListSource = m_DefaultListSource
        m_MediaSources.Add(m_Radio_AM_MediaSource)

        If chan(0) = "AM" Then
            m_Radio_MediaSource = m_Radio_AM_MediaSource
        End If

        AddHandler m_Radio_AM_MediaSource.OnCommand_SetListSource, AddressOf MediaSource_AM_OnCommand_SetListSource
        AddHandler m_Radio_AM_MediaSource.OnCommand_PresetSet, AddressOf MediaSource_AM_OnCommand_PresetSet
        AddHandler m_Radio_AM_MediaSource.OnCommand_PresetRemove, AddressOf MediaSource_AM_OnCommand_PresetRemove
        AddHandler m_Radio_AM_MediaSource.OnCommand_PresetRename, AddressOf MediaSource_AM_OnCommand_PresetRename
        AddHandler m_Radio_AM_MediaSource.OnCommand_DirectTune, AddressOf MediaSource_AM_OnCommand_DirectTune
        'AddHandler m_Radio_AM_MediaSource.OnCommand_SearchChannels, AddressOf MediaSource_AM_OnCommand_handler

        Array.Sort(available_ports)

        SafeThread.Asynchronous(AddressOf BackgroundLoad, m_Host)
        'BackgroundLoad()

        Return eLoadStatus.LoadSuccessful

    End Function

    Private Sub BackgroundLoad()

        Dim myCounter As Integer = 0
        Dim chan() As String
        chan = m_LastStation.Split(":")

        access_granted = False

        If m_Radio.IsOpen Then
            m_Radio.Close()
        End If

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Searching: {0}", m_ComPort))
        End If

        ' Let the driver find the radio
        If m_ComPort = "Auto" Then
            m_Radio.AutoSearch = True
            myNotification.Text = String.Format("Searching for HD Radio")
        Else
            m_Radio.AutoSearch = False
            myNotification.Text = String.Format("Checking for HD Radio on {0}", m_ComPort)
            m_Radio.ComPortString = m_ComPort
        End If

        For y = 1 To m_Retries
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Waiting for serial access (attempt {0}).", y))
            End If
            access_granted = OpenMobile.helperFunctions.SerialAccess.GetAccess(Me, TimeSpan.FromSeconds(30))
            If access_granted Then
                Exit For
            End If
            System.Threading.Thread.Sleep(50)
        Next

        If Not access_granted Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Unable to obtain serial lock."))
            End If
            OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
            Exit Sub
        End If

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - BackgroundLoad()", String.Format("Obtained serial lock, proceeding..."))
        End If

        ' Open communication with radio

        m_Radio.Open()

    End Sub

    Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

        If m_Settings Is Nothing Then

            'LoadRadioSettings()

            m_Settings = New Settings("HD Radio")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.Add("Auto")
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)

            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.ComPort", "COM", "Com Port", COMOptions, COMOptions, m_ComPort))
            m_Settings.Add(Setting.TextList("OMVisteonRadio.SourceDevice", "Select input device", "Audio input device", StoredData.Get(Me, "SourceDevice"), OM.Host.AudioDeviceHandler.InputDevices))
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.FadeAudio", "Fade", "Fade Audio", Setting.BooleanList, Setting.BooleanList, m_Fade))
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_verbose))
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "OMVisteonRadio.DefaultList", "Presets?", "Set Presets as default list", Setting.BooleanList, Setting.BooleanList, m_DefaultListSource))
            Dim Range As New Generic.List(Of String)
            Range.Add("0")
            Range.Add("100")
            m_Settings.Add(New Setting(SettingTypes.Range, "OMVisteonRadio.AudioVolume", "Input Vol", "(0-100%)", Nothing, Range, m_RouteVolume))

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed

        End If

        Return m_Settings

    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)

        Select Case St.Name
            Case "OMVisteonRadio.ComPort"
                m_ComPort = St.Value
            Case "OMVisteonRadio.SourceDevice"
                m_InputDevice = St.Value
            Case "OMVisteonRadio.FadeAudio"
                m_Fade = St.Value
            Case "OMVisteonRadio.VerboseDebug"
                m_verbose = St.Value
            Case "OMVisteonRadio.AudioVolume"
            Case "OMVisteonRadio.LastAMStation"
                m_LastAMStation = St.Value
            Case "OMVisteonRadio.LastFMStation"
                m_LastFMStation = St.Value
        End Select

        helperFunctions.StoredData.Set(Me, St.Name, St.Value)

    End Sub

    Public Function tuneTo(ByVal station As String) As Boolean

        Dim Chan() As String
        Dim Band As HDRadioComm.HDRadio.HDRadioBands = HDRadio.HDRadioBands.FM
        Dim Freq As Decimal = 0
        Dim SubChan As Integer

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - tuneTo()"), String.Format("Tuning to {0}", station))
        End If

        If station.Length > 0 Then

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

                Return True

            End If
        End If

        Return False

    End Function

    Private Sub m_Radio_HDRadioEventTunerTuned(ByVal Message As String) Handles m_Radio.HDRadioEventTunerTuned

        Dim success As Boolean = False
        Dim test As Boolean = False
        Dim zone As Zone

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), String.Format("Tuner tuned to {0}.", m_Radio.CurrentFormattedChannel))
        End If

        SyncLock m_SubStationData
            m_SubStationData.Clear()
        End SyncLock

        m_LastStation = String.Format("{0}:{1}", m_Radio_MediaSource.Name, m_Radio.CurrentFrequency * 100)
        helperFunctions.StoredData.Set(Me, Me.pluginName & ".LastPlaying", m_LastStation)

        SyncLock m_CurrentMedia
            m_CurrentMedia = New mediaInfo()
            m_CurrentMedia.Type = eMediaType.Radio
            ' List key
            m_CurrentMedia.Location = m_LastStation
            ' Display name
            m_CurrentMedia.Name = m_Radio.CurrentFormattedChannel
            ' Main text
            m_CurrentMedia.Artist = ""
            ' Only on Now Playing
            m_CurrentMedia.Album = ""
            ' Genre
            m_CurrentMedia.Genre = ""
            ' Not supported
            m_CurrentMedia.Lyrics = ""
            ' Not supported
            m_CurrentMedia.Rating = 0
            ' Track length - not supported
            m_CurrentMedia.Length = 0
            ' Fetch or create Cover Art
        End SyncLock

        updateCover(m_CurrentMedia)

        m_artist = ""
        m_album = ""
        m_track = ""
        textLine1 = ""
        textLine2 = ""

        ' Which info has to match between MediaInfo and MediaSource????
        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.ChannelID = ""
            m_Radio_MediaSource.ChannelNameLong = m_Radio.CurrentFormattedChannel
            m_Radio_MediaSource.ChannelNameShort = m_Radio.CurrentFormattedChannel
        End SyncLock

        ' Add channel to LIVE playlist
        Select Case m_Radio_MediaSource.Name
            Case "AM"
                test = m_Radio_AM_Live.AddDistinct(m_CurrentMedia, Function(m_Radio_AM_Live) m_Radio_AM_Live.Location = m_CurrentMedia.Location)
                helperFunctions.StoredData.Set(Me, Me.pluginName & ".LastAMStation", m_LastStation)
            Case "FM"
                test = m_Radio_FM_Live.AddDistinct(m_CurrentMedia, Function(m_Radio_FM_Live) m_Radio_FM_Live.Location = m_CurrentMedia.Location)
                helperFunctions.StoredData.Set(Me, Me.pluginName & ".LastFMStation", m_LastStation)
        End Select

        If m_verbose Then
            m_Host.DebugMsg(String.Format("OMVisteonRadio - HDRadioEventTunerTuned({0})", Message), String.Format("Saving {0} to {1}.", m_CurrentMedia.Location, m_Radio_MediaSource.ChannelsPreset.DisplayName))
        End If

        m_Radio_MediaSource.ChannelNameShort = m_Radio.CurrentFormattedChannel

        push_media_info()

        ' Puts text on bottom of radio window
        For Each zone In theZones
            MediaProviderData_ReportMediaText(zone, String.Format("Radio tuned to {0}", m_Radio.CurrentFormattedChannel), "")
        Next

    End Sub
    Private Function updateCover(theMedia As mediaInfo)

        Dim chan() As String
        Dim station As String
        Dim theImage As OImage

        chan = theMedia.Location.Split(":")
        station = Left(theMedia.Name, theMedia.Name.IndexOf(" "))
        theImage = OM.Host.getPluginImage(Me, String.Format("Graphics|{0}", chan(1))).image
        If theImage Is Nothing Then
            ' Create an image
            theImage = OM.Host.getPluginImage(Me, "Graphics|RadioBase").image.Copy
            Dim bandfont = OpenMobile.Graphics.Font.Arial
            bandfont.Size = 100
            theImage.RenderText(0, 0, theImage.Width, theImage.Height, chan(0), bandfont, eTextFormat.Normal, Alignment.CenterCenter + Alignment.WordWrap, OpenMobile.Graphics.Color.DarkGray, OpenMobile.Graphics.Color.DarkGray, FitModes.FitFillSingleLine)
            Dim font = OpenMobile.Graphics.Font.Arial
            font.Size = 100
            theImage.RenderText(0, 0, theImage.Width, theImage.Height, station, font, eTextFormat.Bold, Alignment.CenterCenter + Alignment.WordWrap, OpenMobile.Graphics.Color.Black, OpenMobile.Graphics.Color.White, FitModes.FitFillSingleLine)
        End If

        SyncLock theMedia
            theMedia.coverArt = theImage
        End SyncLock

        Return True

    End Function

    Private Sub display_message(message1 As String, messag2 As String)
        ' Puts a text message at the bottom of the media scren
        For Each Zone In theZones
            MediaProviderData_ReportMediaText(Zone, message1, messag2)
        Next
    End Sub

    Public Overrides Function MediaProviderCommand_Activate(zone As OpenMobile.Zone) As String
        ' Radio is turned on at plugin initialize

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Activate()", String.Format("Requested: Activate HDRadio zone: {0}.", zone.ID))
        End If

        If Not theZones.Contains(zone) Then
            theZones.Add(zone)
            MediaProviderData_ReportMediaSource(zone, m_Radio_MediaSource)
            MediaProviderData_ReportMediaInfo(zone, m_CurrentMedia)
        End If

        If Not m_Routes.ContainsKey(zone) Then
            m_Routes.Add(zone, OM.Host.AudioDeviceHandler.ActivateRoute(m_InputDevice, zone))
        End If

        m_Radio.MuteOff()

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_ActivateMediaSource(zone As OpenMobile.Zone, mediaSourceName As String) As String
        ' Prepare/Update the MediaSource (Band)
        '  occurs before the GetMediaSource

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", "Request for band: " & mediaSourceName)
        End If

        If Not m_Radio.IsPowered Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", "Radio not ready.")
            End If
            MediaProviderData_ReportMediaText(zone, "Radio not ready.", "")
            Return "Radio not ready."
        End If

        If mediaSourceName = m_Radio_AM_MediaSource.Name Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting AM band."))
            End If
            m_Radio_MediaSource = m_Radio_AM_MediaSource
            tuneTo(helperFunctions.StoredData.Get(Me, Me.pluginName & ".LastAMStation"))
        ElseIf mediaSourceName = m_Radio_FM_MediaSource.Name Or mediaSourceName = "HD" Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting FM band."))
            End If
            m_Radio_MediaSource = m_Radio_FM_MediaSource
            tuneTo(helperFunctions.StoredData.Get(Me, Me.pluginName & ".LastFMStation"))
        Else
            ' Invalid media source selected (not likely)
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_ActivateMediaSource()", String.Format("Selecting FM band (by default)."))
            End If
            m_Radio_MediaSource = m_Radio_FM_MediaSource
            tuneTo(helperFunctions.StoredData.Get(Me, Me.pluginName & ".LastFMStation"))
        End If

        Return ""

    End Function

    Public Overrides Function MediaProviderData_GetMediaSource(zone As OpenMobile.Zone) As OpenMobile.Media.MediaSource
        ' Return the Mediasource (Band)

        If m_verbose Then
            'm_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSource()", String.Format("Requested: Current MediaSource. Responding: {0}", m_Radio_MediaSource.Name))
        End If

        If m_Radio.IsHDActive Then
            'm_CurrentMedia.Name = m_Radio.CurrentFormattedChannel.Replace("FM", "HD")
        End If

        Return m_Radio_MediaSource

    End Function

    Public Overrides Function MediaProviderCommand_Deactivate(zone As OpenMobile.Zone) As String
        ' Turns radio off (we actually leave radio powered up, just disconnect the audio route set up for this zone
        ' This saves us from having find and power on the radio if user switches back

        If theZones.Contains(zone) Then
            theZones.Remove(zone)
        End If

        If m_Radio.IsPowered Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Deactivate()", String.Format("Deselecting HDRadio on zone {0}.", zone))
            End If
            If m_Routes.ContainsKey(zone) Then
                OM.Host.AudioDeviceHandler.DeactivateRoute(m_Routes(zone))
                m_Routes.Remove(zone)
            End If
        End If

        If theZones.Count = 0 Then
            m_Radio.MuteOn()
        End If

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_Next(zone As OpenMobile.Zone, param() As Object) As String
        ' Next preset

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Next()", String.Format("Requested: Select Next Preset."))
        End If

        If Not m_Radio.IsPowered Then
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Next()", String.Format("Radio is not ready."))
            End If
            Return "Radio not ready."
        End If

        ' What is the current mediaSource and it's listSource
        '  if listSource = 1, then it's a preset and we can go to the next one
        '  if there is a next one
        Select Case m_Radio_MediaSource.Name
            Case m_Radio_FM_MediaSource.Name
                If m_Radio_FM_MediaSource.ListSource = 1 Then
                    ' Get next preset
                    m_Radio_FM_Presets.GotoNextMedia()
                Else
                    ' Get next live
                    m_Radio_FM_Live.GotoNextMedia()
                End If
            Case m_Radio_AM_MediaSource.Name
                If m_Radio_AM_MediaSource.ListSource = 1 Then
                    ' AM presets are active
                    m_Radio_AM_Presets.GotoNextMedia()
                Else
                    ' Get next live
                    m_Radio_AM_Live.GotoNextMedia()
                End If
        End Select

        ' Tune to the channel

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_Pause(zone As OpenMobile.Zone, param() As Object) As String
        ' Radio mute 
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Pause()", String.Format("Requested: Select Pause/Mute."))
        End If

        If m_Radio.IsPowered Then
            m_Radio.MuteOn()
        End If

        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Previous(zone As OpenMobile.Zone, param() As Object) As String
        ' Previous preset

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Previous()", String.Format("Requested: Select Previous Preset."))
        End If

        ' What is the current mediaSource and it's listSource
        '  if listSource = 1, then it's a preset and we can go to the next one
        '  if there is a next one

        Select Case m_Radio_MediaSource.Name
            Case m_Radio_FM_MediaSource.Name
                If m_Radio_FM_MediaSource.ListSource = 1 Then
                    ' Get next preset
                    m_Radio_FM_Presets.GotoPreviousMedia()
                Else
                    ' Get next live
                    m_Radio_FM_Live.GotoPreviousMedia()
                End If
            Case m_Radio_AM_MediaSource.Name
                If m_Radio_AM_MediaSource.ListSource = 1 Then
                    ' AM presets are active
                    m_Radio_AM_Presets.GotoPreviousMedia()
                Else
                    ' Get next live
                    m_Radio_AM_Live.GotoPreviousMedia()
                End If
        End Select

        ' Tune to the channel

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekBackward(zone As OpenMobile.Zone, param() As Object) As String

        ' Previous available channel

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_SeekBackward()", String.Format("Requested: Seek Back to previous live channel."))
        End If

        If m_Radio.IsPowered Then
            m_Radio.SeekDown(HDRadio.HDRadioSeekType.ALL)
        End If

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekForward(zone As OpenMobile.Zone, param() As Object) As String

        ' Next available channel

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_SeekForward()", String.Format("Requested: Seek forward to next live channel."))
        End If

        If m_Radio.IsPowered Then
            m_Radio.SeekUp(HDRadio.HDRadioSeekType.ALL)
        End If

        Return ""

    End Function

    Public Overrides Function MediaProviderCommand_Playback_Start(zone As OpenMobile.Zone, param() As Object) As String
        ' Un-mute radio
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Start()", String.Format("Requested: Unmute radio playback."))
        End If

        If m_Radio.IsPowered Then
            m_Radio.MuteOff()
        End If

        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Stop(zone As OpenMobile.Zone, param() As Object) As String
        ' Mute radio
        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderCommand_Playback_Stop()", String.Format("Requested: Mute radio playback."))
        End If

        If m_Radio.IsPowered Then
            m_Radio.MuteOn()
        End If

        Return ""
    End Function


    Public Overrides Function MediaProviderData_GetMediaSourceAbilities(mediaSource As String) As OpenMobile.Media.MediaProviderAbilities
        ' Return array of what we can do (per band)

        Dim abilities As New OpenMobile.Media.MediaProviderAbilities

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSourceAbilities()", String.Format("Requested: capabilities."))
        End If

        abilities.CanGotoNext = True
        abilities.CanGotoPrevious = True
        abilities.CanPause = True
        abilities.CanPlay = True
        abilities.CanRepeat = False
        abilities.CanSeek = True
        abilities.CanShuffle = False
        abilities.CanStop = True

        Return abilities

    End Function

    Public Overrides Function MediaProviderData_GetMediaSources() As List(Of OpenMobile.Media.MediaSource)
        ' Return available bands

        If m_verbose Then
            'm_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetMediaSources()", String.Format("Requested: available bands."))
        End If

        Return m_MediaSources

    End Function

    Public Overrides Function MediaProviderData_GetPlaylist(zone As OpenMobile.Zone) As OpenMobile.Media.Playlist

        ' Fetch current playlist

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Requesting playlist...."))
        End If

        Select Case m_Radio_MediaSource.Name
            Case m_Radio_AM_MediaSource.Name
                If m_Radio_AM_MediaSource.ListSource = 0 Then
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Serving up AM Live."))
                    End If
                    Return m_Radio_AM_Live
                Else
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Serving up AM Presets."))
                    End If
                    Return m_Radio_AM_Presets
                End If
            Case m_Radio_FM_MediaSource.Name
                If m_Radio_FM_MediaSource.ListSource = 0 Then
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Serving up FM Live."))
                    End If
                    Return m_Radio_FM_Live
                Else
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_GetPlaylist()", String.Format("Serving up FM Presets."))
                    End If
                    Return m_Radio_FM_Presets
                End If
            Case Else
                Return m_Radio_FM_Live
        End Select

    End Function

    Public Overrides Function MediaProviderData_SetPlaylist(zone As OpenMobile.Zone, playlist As OpenMobile.Media.Playlist) As OpenMobile.Media.Playlist
        ' Not implemented

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaProviderData_SetPlaylist()", String.Format("Not implemented."))
        End If

        Select Case playlist.Name
            Case "AM Live"
                Return m_Radio_AM_Live
            Case "AM Presets"
                Return m_Radio_AM_Presets
            Case "FM Live"
                Return m_Radio_FM_Live
            Case "FM Presets"
                Return m_Radio_FM_Presets
            Case Else
                Return m_Radio_FM_Live
        End Select

    End Function

    Public Function MediaSource_AM_OnCommand_DirectTune(zone As Zone, param() As Object)
        ' Direct tune AM band

        If Params.IsParamsValid(param, 1) Then
            If param(0).GetType() Is GetType(OpenMobile.mediaInfo) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_DirectTune()", String.Format("Direct tune to {0}", param(0).Location))
                End If
                tuneTo(param(0).Location)
                Return True
            Else
                ' Format the parameter to tune to (could test for string)
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_DirectTune()", String.Format("AM:{0}", param(0)))
                End If
                tuneTo(String.Format("AM:{0}", param(0)))
                Return True
            End If
        Else
            Return False
        End If

    End Function

    Public Function MediaSource_FM_OnCommand_DirectTune(zone As Zone, param() As Object)
        ' Direct tune FM band

        If Params.IsParamsValid(param, 1) Then
            If param(0).GetType() Is GetType(OpenMobile.mediaInfo) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_DirectTune()", String.Format("Direct tune to {0}", param(0).Location))
                End If
                tuneTo(param(0).Location)
                Return True
            Else
                ' Format the parameter to tune to (could test for string)
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_DirectTune()", String.Format("Direct tune to FM:{0}", param(0)))
                End If
                tuneTo(String.Format("FM:{0}", param(0)))
                Return True
            End If
        Else
            Return False
        End If

    End Function

    Public Function MediaSource_AM_OnCommand_SetListSource(zone As Zone, param() As Object)
        ' Select AM Live or Presets

        If Params.IsParamsValid(param, 1) Then

        End If

        If m_Radio_AM_MediaSource.ListSource = 0 Then
            m_Radio_AM_MediaSource.ListSource = 1
        Else
            m_Radio_AM_MediaSource.ListSource = 0
        End If

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_SetListSource()", String.Format("Toggling AM list source to {0}", m_Radio_AM_MediaSource.ListSource))
        End If

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return True

    End Function

    Public Function MediaSource_FM_OnCommand_SetListSource(zone As Zone, param() As Object)
        ' Select AM Live or Presets

        If m_Radio_MediaSource.ListSource = 1 Then
            m_Radio_MediaSource.ListSource = 0
        Else
            m_Radio_MediaSource.ListSource = 1
        End If

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_SetListSource()", String.Format("Toggling list source to {0}", IIf(m_Radio_MediaSource.ListSource = 0, "Live", "Presets")))
        End If

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return True

    End Function

    Public Function MediaSource_AM_OnCommand_PresetSet(zone As Zone, param() As Object)
        ' Add AM preset

        Dim test As Boolean

        If Params.IsParamsValid(param, 1) Then
            ' Supplying us with MediaInfo type?
            If param(0).GetType() Is GetType(OpenMobile.mediaInfo) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Saving preset {0} to {1}", param(0).Location, m_Radio_AM_Presets.Name))
                End If
                If m_Radio_AM_Presets.AddDistinct(param(0), Function(m_Radio_AM_Presets) m_Radio_AM_Presets.Location = param(0).Location) Then
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Preset added."))
                    End If
                Else
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Preset was not added."))
                    End If
                End If
            End If
        Else
            ' Must be coming from MediaInfo
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Saving preset {0} to {1}", m_CurrentMedia.Location, m_Radio_AM_Presets.Name))
            End If
            If m_Radio_AM_Presets.AddDistinct(m_CurrentMedia, Function(m_Radio_AM_Presets) m_Radio_AM_Presets.Location = m_CurrentMedia.Location) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Preset added."))
                End If
            Else
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetSet()", String.Format("Preset was not added."))
                End If
            End If
        End If

        test = m_Radio_AM_Presets.Save()

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return True

    End Function

    Public Function MediaSource_FM_OnCommand_PresetSet(zone As Zone, param() As Object)
        ' Add FM preset

        Dim test As Boolean

        If Params.IsParamsValid(param, 1) Then
            ' Supplying us with MediaInfo type?
            If param(0).GetType() Is GetType(OpenMobile.mediaInfo) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Saving preset {0} to {1}", param(0).Location, m_Radio_FM_Presets.Name))
                End If
                If m_Radio_FM_Presets.AddDistinct(param(0), Function(m_Radio_FM_Presets) m_Radio_FM_Presets.Location = param(0).Location) Then
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Preset addded."))
                    End If
                Else
                    If m_verbose Then
                        m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Preset was not added."))
                    End If
                End If
            End If
        Else
            ' Must be coming from MediaInfo
            If m_verbose Then
                m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Saving preset {0} to {1}", m_CurrentMedia.Location, m_Radio_FM_Presets.Name))
            End If
            If m_Radio_FM_Presets.AddDistinct(m_CurrentMedia, Function(m_Radio_FM_Presets) m_Radio_FM_Presets.Location = m_CurrentMedia.Location) Then
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Preset added."))
                End If
            Else
                If m_verbose Then
                    m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetSet()", String.Format("Preset was not added."))
                End If
            End If
        End If

        test = m_Radio_FM_Presets.Save()

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return True

    End Function

    Public Function MediaSource_AM_OnCommand_PresetRemove(zone As Zone, param() As Object)
        ' Remove AM preset

        m_Radio_AM_Presets.Remove(param(0))
        m_Radio_AM_Presets.Save()

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaSource_AM_OnCommand_PresetRemove()", String.Format("Preset {0} removed.", param(0).Name))
        End If

        'Return String.Format("Preset {0} removed.", param(0).Name)
        Return ""

    End Function

    Public Function MediaSource_FM_OnCommand_PresetRemove(zone As Zone, param() As Object)
        ' Remove FM preset

        m_Radio_FM_Presets.Remove(param(0))
        m_Radio_FM_Presets.Save()

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - MediaSource_FM_OnCommand_PresetRemove()", String.Format("Preset {0} removed.", param(0).Name))
        End If

        'Return String.Format("Preset {0} removed.", param(0).Name)
        Return ""

    End Function

    Public Function MediaSource_AM_OnCommand_PresetRename(zone As Zone, param() As Object)
        ' Rename AM preset

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return ""

    End Function

    Public Function MediaSource_FM_OnCommand_PresetRename(zone As Zone, param() As Object)
        ' Rename FM preset

        For Each zone In theZones
            MediaProviderData_RefreshPlaylist(zone)
        Next

        Return ""

    End Function

    Public Overrides Function MediaProviderData_GetRepeat(zone As OpenMobile.Zone) As Boolean

        Return "Not implemented"

    End Function

    Public Overrides Function MediaProviderData_GetShuffle(zone As OpenMobile.Zone) As Boolean

        Return "Not implemented"

    End Function

    Public Overrides Function MediaProviderData_SetRepeat(zone As OpenMobile.Zone, state As Boolean) As Boolean

        Return "Not implemented"

    End Function

    Public Overrides Function MediaProviderData_SetShuffle(zone As OpenMobile.Zone, state As Boolean) As Boolean

        Return "Not implemented"

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

    Private Sub push_media_info()
        ' We call this any time we've changed the MediaInfo stuff

        'm_CurrentMedia.Album = "[Album]"
        'm_CurrentMedia.Location =
        'm_CurrentMedia.Name = m_Radio.CurrentFormattedChannel
        'm_CurrentMedia.Genre 
        'm_CurrentMedia.Artist
        'm_CurrentMedia.Album 
        'm_CurrentMedia.Lyrics 
        'm_CurrentMedia.Rating

        ' We should attempt to update the Playlists (Live & Presets)
        '  with any new info

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - push_media_info()", String.Format("Pushing mediaInfo."))
        End If

        SyncLock theZones
            For Each Zone In theZones
                MyBase.MediaProviderData_ReportMediaInfo(Zone, m_CurrentMedia)
                MyBase.MediaProviderData_RefreshPlaylist(Zone)
            Next
        End SyncLock

    End Sub

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
                            Info.stationID = String.Format("HD:{0}:{1}", (m_Radio.CurrentFrequency * 100), i)
                            'm_CurrentMedia.Location = String.Format("FM:{0}", (m_Radio.CurrentFrequency * 100))
                            Info.stationName = String.Format("{0} HD-{1}", m_Radio.CurrentFormattedChannel, i)
                            'm_CurrentMedia.Name = Info.stationName
                            m_StationList.Add(m_Radio.CurrentFrequency, Info)
                            m_SubStationData.Add(i, New SubChannelData)
                        End If
                    Next

                    'RaiseMediaEvent(eFunction.stationListUpdated, "")

                End SyncLock
            End SyncLock
        End If
    End Sub

    Private Sub m_Radio_HDRadioEventCommError() Handles m_Radio.HDRadioEventCommError

        m_Host.DebugMsg(OpenMobile.DebugMessageType.Error, "OMVisteonRadio - m_Radio_HDRadioEventCommError()", String.Format("HD Radio communication error."))
        OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)
        m_Radio.Close()
        m_CommError = True

        myNotification.Text = String.Format("Communications error ({0})", m_ComPort)
        display_message("HD Radio", String.Format("Communications error ({0})", m_ComPort))

    End Sub

    Private Sub m_Radio_HDRadioEventSysErrorRadioNotFound() Handles m_Radio.HDRadioEventSysErrorRadioNotFound
        ' Radio Not Found

        If m_verbose Then
            m_Host.DebugMsg(OpenMobile.DebugMessageType.Info, "OMVisteonRadio", String.Format("No radio found using {0}.", m_ComPort))
        End If

        OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)

        m_Radio.Close()
        m_NotFound = True

        myNotification.Text = String.Format("No radio found using {0}", m_ComPort)
        display_message("HD Radio", String.Format("No radio found using {0}", m_ComPort))

    End Sub

    Private Sub m_Radio_HDRadioEventSysPortOpened() Handles m_Radio.HDRadioEventSysPortOpened

        m_ComPort = m_Radio.ComPortString
        helperFunctions.StoredData.Set(Me, "OMVisteonRadio.ComPort", m_ComPort)

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventSysPortOpened()", String.Format("Opened using port {0}", m_Radio.ComPortString))
        End If

        OpenMobile.helperFunctions.SerialAccess.ReleaseAccess(Me)

        myNotification.Text = String.Format("HD Radio opened using {0}", m_ComPort)
        display_message("HD Radio", String.Format("Opened using {0}", m_ComPort))

        m_Radio.PowerOn()

    End Sub

    Private Sub m_Radio_HDRadioEventHWPoweredOn() Handles m_Radio.HDRadioEventHWPoweredOn

        m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("HD Radio Powered on."))

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHWPoweredOn()", String.Format("Tuning to last playing: {0}", helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastPlaying")))
        End If

        myNotification.Text = "HD Radio ready"
        display_message("HD Radio", "HD Radio ready")

        tuneTo(helperFunctions.StoredData.Get(Me, "OMVisteonRadio.LastPlaying"))

        m_Radio.SetVolume(&H4B)

    End Sub

    Private Sub m_Radio_HDRadioEventRDSGenre(ByVal Message As String) Handles m_Radio.HDRadioEventRDSGenre

        ' Track/Station Genre
        Dim inList As Boolean = False

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventRDSGenre()", String.Format("Genre received: {0}.", Message))
        End If

        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.ProgramType = Message
        End SyncLock
        SyncLock m_CurrentMedia
            m_CurrentMedia.Genre = Message
        End SyncLock

        ' Search playlists to find current station and update genre

        If m_Radio_MediaSource.Name = m_Radio_FM_MediaSource.Name Then
            For x = 0 To m_Radio_FM_Presets.Count - 1
                If m_Radio_FM_Presets.Items(x).Location = m_CurrentMedia.Location Then
                    m_Radio_FM_Presets.Items(x).Genre = m_CurrentMedia.Genre
                    m_Radio_FM_Presets.Save()
                End If
            Next
            For x = 0 To m_Radio_FM_Live.Count - 1
                If m_Radio_FM_Live.Items(x).Location = m_CurrentMedia.Location Then
                    m_Radio_FM_Live.Items(x).Genre = m_CurrentMedia.Genre
                    m_Radio_FM_Live.Save()
                End If
            Next
        End If

        push_media_info()

        textLine2 = String.Format("{0}", Message)

        For Each Zone In theZones
            MediaProviderData_ReportMediaText(Zone, textLine1, textLine2)
        Next

    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramIdentification(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramIdentification
        ' PI for station identification ? chars?

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventRDSProgramIdentification()", String.Format("ProgramID received: {0}.", Message))
        End If

        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.ChannelNameLong = Message
            'Top right corner of media info
            m_Radio_MediaSource.ChannelID = Message
        End SyncLock

        SyncLock m_CurrentMedia
            m_CurrentMedia.Album = String.Format("{0}", Message)
        End SyncLock

        push_media_info()

        textLine1 = String.Format("{0}", Message)

        For Each Zone In theZones
            MediaProviderData_ReportMediaText(Zone, textLine1, textLine2)
        Next

    End Sub

    Private Sub m_Radio_HDRadioEventRDSProgramService(ByVal Message As String) Handles m_Radio.HDRadioEventRDSProgramService
        ' PS 8 character Call letters or ID

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventRDSProgramService()", String.Format("Program Service received: {0}.", Message))
        End If

        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.ChannelNameShort = Message
            'Top right corner of media info
            'm_Radio_MediaSource.ChannelID = Message
        End SyncLock

        SyncLock m_CurrentMedia
            m_CurrentMedia.Album = String.Format("{0}", Message)
        End SyncLock

        push_media_info()

        textLine1 = String.Format("{0}", Message)

        For Each Zone In theZones
            MediaProviderData_ReportMediaText(Zone, textLine1, textLine2)
        Next

    End Sub

    Private Sub m_Radio_HDRadioEventRDSRadioText(ByVal Message As String) Handles m_Radio.HDRadioEventRDSRadioText
        ' RT 64 character free form text

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventRDSRadioText()", String.Format("Radio Text received: {0}.", Message))
        End If

        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.ProgramText = Message
        End SyncLock

        SyncLock m_CurrentMedia
            ' Large middle line in media info box
            m_CurrentMedia.Artist = String.Format("{0}", m_Radio_MediaSource.ProgramText)
        End SyncLock

        push_media_info()

    End Sub

    Private Sub m_Radio_HDRadioEventHDActive(ByVal State As Integer) Handles m_Radio.HDRadioEventHDActive

        If State Then
            m_CurrentMedia.Name = m_Radio.CurrentFormattedChannel.Replace("FM", "HD")
        Else
            m_CurrentMedia.Name = m_Radio.CurrentFormattedChannel
        End If

        ' Update presets/live lists (only FM required)
        If m_Radio_MediaSource.Name = m_Radio_FM_MediaSource.Name Then
            For x = 0 To m_Radio_FM_Presets.Count - 1
                If m_Radio_FM_Presets.Items(x).Location = m_CurrentMedia.Location Then
                    m_Radio_FM_Presets.Items(x).Name = m_CurrentMedia.Name
                    m_Radio_FM_Presets.Save()
                End If
            Next
            For x = 0 To m_Radio_FM_Live.Count - 1
                If m_Radio_FM_Live.Items(x).Location = m_CurrentMedia.Name Then
                    m_Radio_FM_Live.Items(x).Name = m_CurrentMedia.Name
                    m_Radio_FM_Live.Save()
                End If
            Next
        End If



        push_media_info()

    End Sub

    Private Sub m_Radio_HDRadioEventHDCallSign(ByVal Message As String) Handles m_Radio.HDRadioEventHDCallSign

        ' Put station call sign in ????

        SyncLock m_Radio_MediaSource
            m_Radio_FM_MediaSource.ChannelID = String.Format("{0}", Message)
        End SyncLock

        push_media_info()

    End Sub

    Private Sub m_Radio_HDRadioEventHDArtist(ByVal Message As String) Handles m_Radio.HDRadioEventHDArtist

        Dim data() As String

        If m_Radio_MediaSource.Name = "FM" Then

            data = Message.Split("|")
            m_artist = data(1)

            SyncLock m_Radio_MediaSource
                m_Radio_FM_MediaSource.Description = String.Format("{0} - {1}", m_artist, m_track)
            End SyncLock

            push_media_info()

            textLine1 = String.Format("{0}", String.Format("{0} - {1}", m_artist, m_track))

            For Each Zone In theZones
                MediaProviderData_ReportMediaText(Zone, textLine1, textLine2)
            Next

        End If

    End Sub

    Private Sub m_Radio_HDRadioEventHDTitle(ByVal Message As String) Handles m_Radio.HDRadioEventHDTitle

        Dim data() As String

        If m_Radio_MediaSource.Name = "FM" Then

            Data = Message.Split("|")
            m_track = Data(1)

            SyncLock m_Radio_MediaSource
                m_Radio_FM_MediaSource.Description = String.Format("{0} - {1}", m_artist, m_track)
            End SyncLock

            push_media_info()

            textLine1 = String.Format("{0}", String.Format("{0} - {1}", m_artist, m_track))

            For Each Zone In theZones
                MediaProviderData_ReportMediaText(Zone, textLine1, textLine2)
            Next

        End If

    End Sub

    Private Sub m_Radio_HDRadioEventTunerSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventTunerSignalStrength

        ' Current NON-HD signal strength

        SyncLock m_Radio_MediaSource
            m_Radio_MediaSource.SignalStrength = Int(State / 300)
        End SyncLock

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventTunerSignalStrength()", String.Format("Signal Strength received: {0}.", m_Radio_MediaSource.SignalStrength))
        End If

    End Sub

    Private Sub m_Radio_HDRadioEventHDSignalStrength(ByVal State As Integer) Handles m_Radio.HDRadioEventHDSignalStrength

        ' Current HD signal strength

        If m_verbose Then
            m_Host.DebugMsg("OMVisteonRadio - HDRadioEventHDSignalStrength()", String.Format("Signal Strength received: {0}.", State))
        End If

        If m_Radio_MediaSource.Name <> "AM" Then
            SyncLock m_Radio_MediaSource
                m_Radio_MediaSource.SignalStrength = Int(State / 300)
            End SyncLock
        End If

    End Sub

    Private Class SubChannelData
        Public Station As String
        Public Artist As String
        Public Title As String
    End Class

End Class
