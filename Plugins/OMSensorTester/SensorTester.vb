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

'    Copyright 2011-2012 Jonathan Heizer jheizer@gmail.com
#End Region
Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports OpenMobile.Controls
Imports OpenMobile.Graphics

Public Class SensorTester
    Implements IHighLevel

    Private m_Host As IPluginHost
    Private m_Manager As ScreenManager

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host

        Dim Pan As New OMPanel("")

        Dim List As New OMList(0, 100, 1000, 425)
        List.Name = "Sensors"
        List.Font = New Font(Font.GenericSansSerif, 24.0F)
        List.TextAlignment = Alignment.CenterLeft
        List.ListStyle = eListStyle.TextList
        Pan.addControl(List)
        AddHandler Pan.Entering, AddressOf Pan_Entering

        m_Manager = New ScreenManager(m_Host.ScreenCount)
        m_Manager.loadPanel(Pan)

        Return eLoadStatus.LoadSuccessful

    End Function

    Public Sub Pan_Entering(ByVal sender As OMPanel, ByVal screen As Integer)
        Dim Pan As OMPanel = sender
        Dim List As OMList = Pan("Sensors")
        List.ListStyle = eListStyle.TextList
        List.Clear()
        Dim Sensors As Generic.List(Of Sensor) = m_Host.getData(eGetData.GetAvailableSensors, "")
        If Not Sensors Is Nothing Then
            For Each Sen As Sensor In Sensors
                If Sen.Type = eSensorType.deviceSuppliesData Then
                    Dim item As New OMListItem(Sen.Name & " - {0}")
                    item.sensorName = Sen.Name
                    List.Add(item)
                End If
            Next
        End If
    End Sub

    Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel
        If m_Manager Is Nothing Then
            Return Nothing
        End If
        Return m_Manager(screen, name)
    End Function

    Public Sub DataChanged(ByVal name As String, ByVal value As Object)
        Dim List As OMList = m_Manager(0, "")("Sensors")
        List.Add(New OMListItem(name & "-" & value))
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings
        Return Nothing
    End Function

    Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "Sensor Tester"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "SensorTester"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public ReadOnly Property displayName() As String Implements OpenMobile.Plugin.IHighLevel.displayName
        Get
            Return "Sensor Tester"
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

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then

            End If

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
