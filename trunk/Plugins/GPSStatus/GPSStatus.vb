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
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports OpenMobile.Controls
Imports OpenMobile.Graphics

Public Class GPSStatus
    Implements IHighLevel

    Private WithEvents m_Host As IPluginHost
    Private m_Manager As ScreenManager
    Private m_Output As New Generic.List(Of OMLabel)

    Private m_Gps As IRawHardware

    Private WithEvents m_Timer As New Timers.Timer(1000)

    Private PIDsLoaded As Boolean = False

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host

        Dim Pan As New OMPanel("")
        'Lat
        Dim LatTitle As New OMLabel(100, 100, 200, 50)
        LatTitle.Name = "GPSStatus_Title_Lat"
        LatTitle.TextAlignment = Alignment.CenterLeft
        LatTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        LatTitle.Text = "Latitude"
        Pan.addControl(LatTitle)

        Dim Lat As New OMLabel(300, 100, 200, 50)
        Lat.Name = "GPSStatus_Lat"
        Lat.TextAlignment = Alignment.CenterLeft
        Lat.Font = New Font(Font.GenericSansSerif, 24.0F)
        Lat.Tag = "GPS.Latitude"
        Pan.addControl(Lat)
        m_Output.Add(Lat)

        'Long
        Dim LongTitle As New OMLabel(100, 150, 200, 50)
        LongTitle.Name = "GPSStatus_Title_Long"
        LongTitle.TextAlignment = Alignment.CenterLeft
        LongTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        LongTitle.Text = "Longitude"
        Pan.addControl(LongTitle)

        Dim Lng As New OMLabel(300, 150, 200, 50)
        Lng.Name = "GPSStatus_Long"
        Lng.TextAlignment = Alignment.CenterLeft
        Lng.Font = New Font(Font.GenericSansSerif, 24.0F)
        Lng.Tag = "GPS.Longitude"
        Pan.addControl(Lng)
        m_Output.Add(Lng)

        'Speed
        Dim SpeedTitle As New OMLabel(100, 200, 200, 50)
        SpeedTitle.Name = "GPSStatus_Title_Speed"
        SpeedTitle.TextAlignment = Alignment.CenterLeft
        SpeedTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        SpeedTitle.Text = "Speed"
        Pan.addControl(SpeedTitle)

        Dim Speed As New OMLabel(300, 200, 200, 50)
        Speed.Name = "GPSStatus_Speed"
        Speed.TextAlignment = Alignment.CenterLeft
        Speed.Font = New Font(Font.GenericSansSerif, 24.0F)
        Speed.Tag = "GPS.Speed"
        Pan.addControl(Speed)
        m_Output.Add(Speed)

        'Altitude
        Dim AltTitle As New OMLabel(100, 250, 200, 50)
        AltTitle.Name = "GPSStatus_Title_Alt"
        AltTitle.TextAlignment = Alignment.CenterLeft
        AltTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        AltTitle.Text = "Altitude"
        Pan.addControl(AltTitle)

        Dim Alt As New OMLabel(300, 250, 200, 50)
        Alt.Name = "GPSStatus_Alt"
        Alt.TextAlignment = Alignment.CenterLeft
        Alt.Font = New Font(Font.GenericSansSerif, 24.0F)
        Alt.Tag = "GPS.Altitude"
        Pan.addControl(Alt)
        m_Output.Add(Alt)

        'Bearing
        Dim BearingTitle As New OMLabel(100, 300, 200, 50)
        BearingTitle.Name = "GPSStatus_Title_Bearing"
        BearingTitle.TextAlignment = Alignment.CenterLeft
        BearingTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        BearingTitle.Text = "Bearing"
        Pan.addControl(BearingTitle)

        Dim Bearing As New OMLabel(300, 300, 200, 50)
        Bearing.Name = "GPSStatus_Bearing"
        Bearing.TextAlignment = Alignment.CenterLeft
        Bearing.Font = New Font(Font.GenericSansSerif, 24.0F)
        Bearing.Tag = "GPS.Bearing"
        Pan.addControl(Bearing)
        m_Output.Add(Bearing)

        'ZipCode
        Dim ZipTitle As New OMLabel(100, 350, 200, 50)
        ZipTitle.Name = "GPSStatus_Title_ZipCode"
        ZipTitle.TextAlignment = Alignment.CenterLeft
        ZipTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        ZipTitle.Text = "ZipCode"
        Pan.addControl(ZipTitle)

        Dim Zip As New OMLabel(300, 350, 200, 50)
        Zip.Name = "GPSStatus_ZipCode"
        Zip.TextAlignment = Alignment.CenterLeft
        Zip.Font = New Font(Font.GenericSansSerif, 24.0F)
        Zip.Tag = "GPS.ZipCode"
        Pan.addControl(Zip)
        m_Output.Add(Zip)

        'City
        Dim CityTitle As New OMLabel(100, 400, 200, 50)
        CityTitle.Name = "GPSStatus_Title_City"
        CityTitle.TextAlignment = Alignment.CenterLeft
        CityTitle.Font = New Font(Font.GenericSansSerif, 24.0F)
        CityTitle.Text = "City"
        Pan.addControl(CityTitle)

        Dim City As New OMLabel(300, 400, 200, 50)
        City.Name = "GPSStatus_City"
        City.TextAlignment = Alignment.CenterLeft
        City.Font = New Font(Font.GenericSansSerif, 24.0F)
        City.Tag = "GPS.City"
        Pan.addControl(City)
        m_Output.Add(City)

        m_Manager = New ScreenManager(m_Host.ScreenCount)
        m_Manager.loadSharedPanel(Pan)

        Return eLoadStatus.LoadSuccessful

    End Function

    Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel
        If m_Manager Is Nothing Then
            Return Nothing
        End If

        Dim Pan As OMPanel = m_Manager(screen, name)

        If Not PIDsLoaded Then

            Dim o As Object
            m_Host.getData(eGetData.GetAvailableSensors, "OMSerialGPS", o)

            Dim Sensors As Generic.List(Of Sensor) = o
            Dim Sen As Sensor
            Dim C As OMLabel
            For Each Ctrl As OMLabel In m_Output
                C = Ctrl
                Sen = Sensors.Find(Function(p) p.Name = C.Tag)
                Ctrl.Tag = Sen.Name
            Next

            PIDsLoaded = True
        End If

        m_Timer.Enabled = True

        Return Pan
    End Function

    Private Sub m_Timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_Timer.Elapsed
        RefreshValues()
    End Sub

    Private Sub RefreshValues()
        For Each Ctrl As OMLabel In m_Output
            Ctrl.Text = m_Host.getSensorValue(Ctrl.Tag).ToString
        Next
    End Sub

    Private Sub m_Host_OnSystemEvent(ByVal funct As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
        If funct = eFunction.TransitionFromAny Then
            m_Timer.Enabled = False
        End If
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings
        Return Nothing
    End Function

    Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "GPS Status"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "GPSStatus"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public ReadOnly Property displayName() As String Implements OpenMobile.Plugin.IHighLevel.displayName
        Get
            Return "GPS Status"
        End Get
    End Property


    Public ReadOnly Property authorEmail() As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
        Get
            Return "jheizer@gmail.com"
        End Get
    End Property

    Public ReadOnly Property authorName() As String Implements OpenMobile.Plugin.IBasePlugin.authorName
        Get
            Return "Jon heizer"
        End Get
    End Property

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
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
