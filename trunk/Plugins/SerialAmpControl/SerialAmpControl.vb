'*********************************************************************************
'    This file is part of Open Mobile.

'    Open Mobile is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.

'    Open Mobile is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.

'    You should have received a copy of the GNU General Public License
'    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.

'    There is one additional restriction when using this framework regardless of modifications to it.
'    The About Panel or its contents must be easily accessible by the end users.
'    This is to ensure all project contributors are given due credit not only in the source code.
'*********************************************************************************
Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Plugin
Imports System.IO.Ports

Public Class SerialAmpControl
    Implements OpenMobile.Plugin.IOther

    Private WithEvents m_Host As IPluginHost
    Private m_Port As SerialPort
    Private m_ComPort As String
    Private m_Line As String = "DTR"
    Private WithEvents m_Settings As Settings

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        If Not m_ComPort = "-1" Then
            m_Host = host
            m_Port = New SerialPort(m_ComPort)
            TurnOn()
        End If
    End Function

    Private Sub TurnOn()
        If Not m_Port Is Nothing Then
            If Not m_Port.IsOpen Then
                m_Port.Open()
            End If

            If m_Line = "DTR" Then
                m_Port.DtrEnable = True
            Else
                m_Port.RtsEnable = True
            End If
        End If
    End Sub

    Private Sub TurnOff()
        If Not m_Port Is Nothing Then
            If m_Port.IsOpen Then
                If m_Line = "DTR" Then
                    m_Port.DtrEnable = False
                Else
                    m_Port.RtsEnable = False
                End If
            End If
        End If
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

        If m_Settings Is Nothing Then
            LoadRadioSettings()

            m_Settings = New Settings("Serial Amp Control")

            Dim COMOptions As New Generic.List(Of String)
            COMOptions.AddRange(System.IO.Ports.SerialPort.GetPortNames)
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "SerialAmpControl.ComPort", "COM", "Com Port", COMOptions, COMOptions, m_ComPort.ToString))

            Dim LineOptions As New Generic.List(Of String)
            LineOptions.Add("DTR")
            LineOptions.Add("RTS")
            m_Settings.Add(New Setting(SettingTypes.MultiChoice, "SerialAmpControl.Line", "Line", "Line to Trigger", LineOptions, LineOptions, m_Line))

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal St As Setting)
        Using PlugSet As New OpenMobile.Data.PluginSettings
            PlugSet.setSetting(St.Name, St.Value)
        End Using
    End Sub

    Private Sub LoadRadioSettings()
        Dim Settings As New OpenMobile.Data.PluginSettings
        If String.IsNullOrEmpty(Settings.getSetting("SerialAmpControl.ComPort")) Then
            SetDefaultSettings()
        End If
        m_ComPort = Settings.getSetting("SerialAmpControl.ComPort")
        m_Line = Settings.getSetting("SerialAmpControl.Line")
        Settings.Dispose()
    End Sub

    Private Sub SetDefaultSettings()
        Dim Settings As New OpenMobile.Data.PluginSettings
        Settings.setSetting("SerialAmpControl.ComPort", "-1")
        Settings.setSetting("SerialAmpControl.Line", "DTR")
        Settings.Dispose()
    End Sub

    Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles m_Host.OnPowerChange
        Select Case type
            Case Is = ePowerEvent.SystemResumed
                TurnOn()
            Case Is = ePowerEvent.SleepOrHibernatePending, ePowerEvent.ShutdownPending, ePowerEvent.LogoffPending
                TurnOff()
        End Select
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

    Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
        Get
            Return "Amp Control Serial Port DTR"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "Serial Amp Control"
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
                If Not m_Port Is Nothing Then
                    If m_Port.IsOpen Then
                        m_Port.DtrEnable = False
                        m_Port.Close()
                    End If
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

  
End Class
