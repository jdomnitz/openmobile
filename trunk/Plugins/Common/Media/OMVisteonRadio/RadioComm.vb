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

Public MustInherit Class RadioComm
    Inherits MediaProviderBase

    Public Sub New()

        MyBase.New("OMVisteonRadio", OM.Host.getSkinImage("Icon-Radio"), 2.0, "Visteon/Directed Radio Hardware Interface", "jmullan", "jmullan99@gmail.com")

    End Sub

    Public Overrides Function initialize(host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

        Return True

    End Function

    Public Overrides Function MediaProviderCommand_Activate(zone As OpenMobile.Zone) As String

        Return "None"

    End Function

    Public Overrides Function MediaProviderCommand_Deactivate(zone As OpenMobile.Zone) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_Next(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_Pause(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_Previous(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overrides Function MediaProviderCommand_ActivateMediaSource(zone As OpenMobile.Zone, mediaSourceName As String) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_SeekBackward(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_SeekForward(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_Start(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overloads Function MediaProviderCommand_Playback_Stop(zone As OpenMobile.Zone, param As Object) As String

        Return "None"

    End Function

    Public Overrides Function MediaProviderData_GetMediaSource(zone As OpenMobile.Zone) As OpenMobile.Media.MediaSource

        Return Nothing

    End Function

    Public Overrides Function MediaProviderData_GetMediaSourceAbilities(mediasource As String) As OpenMobile.Media.MediaProviderAbilities

        Return Nothing

    End Function

    Public Overrides Function MediaProviderData_GetMediaSources() As Generic.List(Of OpenMobile.Media.MediaSource)

        Return Nothing

    End Function

    Public Overrides Function MediaProviderData_GetPlaylist(zone As OpenMobile.Zone) As OpenMobile.Media.Playlist

        Return Nothing

    End Function

    Public Overrides Function MediaProviderData_GetRepeat(zone As OpenMobile.Zone) As Boolean

        Return False

    End Function

    Public Overrides Function MediaProviderData_SetRepeat(zone As OpenMobile.Zone, state As Boolean) As Boolean

        Return False

    End Function

    Public Overrides Function MediaProviderData_GetShuffle(zone As OpenMobile.Zone) As Boolean

        Return False

    End Function

    Public Overrides Function MediaProviderData_SetShuffle(zone As OpenMobile.Zone, state As Boolean) As Boolean

        Return False

    End Function

    Public Overrides Function MediaProviderData_SetPlayList(zone As OpenMobile.Zone, playlist As OpenMobile.Media.Playlist) As OpenMobile.Media.Playlist

        Return Nothing

    End Function

    Public ReadOnly Property MediaProviderType As OpenMobile.Media.MediaSourceTypes
        Get

        End Get
    End Property

End Class

