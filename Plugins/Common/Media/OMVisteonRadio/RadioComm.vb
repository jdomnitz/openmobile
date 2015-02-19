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
    'Private m_SubStationData As New Generic.SortedDictionary(Of Integer, SubChannelData)
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
    Private m_Radio_XM_MediaSource As MediaSource_TunedContent

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

        ' Register media sources 
        m_Radio_AM_MediaSource = New MediaSource_TunedContent(Me, "AM", "AM Radio", OM.Host.getSkinImage("Radio"))
        m_MediaSources.Add(m_Radio_FM_MediaSource)
        m_Radio_FM_MediaSource = New MediaSource_TunedContent(Me, "FM", "FM Radio", OM.Host.getSkinImage("Radio"))
        m_MediaSources.Add(m_Radio_FM_MediaSource)
        m_Radio_XM_MediaSource = New MediaSource_TunedContent(Me, "XM", "XM Sirius Radio", OM.Host.getSkinImage("siriusxm"))
        m_MediaSources.Add(m_Radio_XM_MediaSource)

        Return eLoadStatus.LoadSuccessful

    End Function

    Public Overrides Function MediaProviderCommand_Activate(zone As OpenMobile.Zone) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_ActivateMediaSource(zone As OpenMobile.Zone, mediaSourceName As String) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Deactivate(zone As OpenMobile.Zone) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Next(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Pause(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Previous(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekBackward(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_SeekForward(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Start(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderCommand_Playback_Stop(zone As OpenMobile.Zone, param() As Object) As String
        Return ""
    End Function

    Public Overrides Function MediaProviderData_GetMediaSource(zone As OpenMobile.Zone) As OpenMobile.Media.MediaSource
        Return m_Radio_FM_MediaSource
    End Function

    Public Overrides Function MediaProviderData_GetMediaSourceAbilities(mediaSource As String) As OpenMobile.Media.MediaProviderAbilities
        Return Nothing
    End Function

    Public Overrides Function MediaProviderData_GetMediaSources() As List(Of OpenMobile.Media.MediaSource)
        Return m_MediaSources
    End Function

    Public Overrides Function MediaProviderData_GetPlaylist(zone As OpenMobile.Zone) As OpenMobile.Media.Playlist

    End Function

    Public Overrides Function MediaProviderData_GetRepeat(zone As OpenMobile.Zone) As Boolean

    End Function

    Public Overrides Function MediaProviderData_GetShuffle(zone As OpenMobile.Zone) As Boolean

    End Function

    Public Overrides Function MediaProviderData_SetPlaylist(zone As OpenMobile.Zone, playlist As OpenMobile.Media.Playlist) As OpenMobile.Media.Playlist

    End Function

    Public Overrides Function MediaProviderData_SetRepeat(zone As OpenMobile.Zone, state As Boolean) As Boolean

    End Function

    Public Overrides Function MediaProviderData_SetShuffle(zone As OpenMobile.Zone, state As Boolean) As Boolean

    End Function
End Class


'Public Class RadioComm
'    Inherits MediaProviderBase

'    Public Sub New()

'        MyBase.New("OMVisteonRadio", OM.Host.getSkinImage("Icon-Radio"), 2.0, "Visteon/Directed Radio Hardware Interface", "jmullan", "jmullan99@gmail.com")

'    End Sub

'    Public Overrides Function initialize(host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

'        Return True

'    End Function

'    Public Overrides Function MediaProviderCommand_Activate(zone As OpenMobile.Zone) As String

'        Return "None"

'    End Function

'    Public Overrides Function MediaProviderCommand_Deactivate(zone As OpenMobile.Zone) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_Next(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_Pause(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_Previous(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overrides Function MediaProviderCommand_ActivateMediaSource(zone As OpenMobile.Zone, mediaSourceName As String) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_SeekBackward(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_SeekForward(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_Start(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overloads Function MediaProviderCommand_Playback_Stop(zone As OpenMobile.Zone, param As Object) As String

'        Return "None"

'    End Function

'    Public Overrides Function MediaProviderData_GetMediaSource(zone As OpenMobile.Zone) As OpenMobile.Media.MediaSource

'        Return Nothing

'    End Function

'    Public Overrides Function MediaProviderData_GetMediaSourceAbilities(mediasource As String) As OpenMobile.Media.MediaProviderAbilities

'        Return Nothing

'    End Function

'    Public Overrides Function MediaProviderData_GetMediaSources() As Generic.List(Of OpenMobile.Media.MediaSource)

'        Return Nothing

'    End Function

'    Public Overrides Function MediaProviderData_GetPlaylist(zone As OpenMobile.Zone) As OpenMobile.Media.Playlist

'        Return Nothing

'    End Function

'    Public Overrides Function MediaProviderData_GetRepeat(zone As OpenMobile.Zone) As Boolean

'        Return False

'    End Function

'    Public Overrides Function MediaProviderData_SetRepeat(zone As OpenMobile.Zone, state As Boolean) As Boolean

'        Return False

'    End Function

'    Public Overrides Function MediaProviderData_GetShuffle(zone As OpenMobile.Zone) As Boolean

'        Return False

'    End Function

'    Public Overrides Function MediaProviderData_SetShuffle(zone As OpenMobile.Zone, state As Boolean) As Boolean

'        Return False

'    End Function

'    Public Overrides Function MediaProviderData_SetPlayList(zone As OpenMobile.Zone, playlist As OpenMobile.Media.Playlist) As OpenMobile.Media.Playlist

'        Return Nothing

'    End Function

'    Public ReadOnly Property MediaProviderType As OpenMobile.Media.MediaSourceTypes
'        Get

'        End Get
'    End Property

'End Class

