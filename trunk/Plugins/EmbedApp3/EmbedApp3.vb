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
Imports System.Runtime.InteropServices

Public Class EmbedApp3
    Implements IHighLevel

    Private WithEvents m_Host As IPluginHost
    Private WithEvents m_Settings As Settings

    Private m_Screen As Integer = 0
    Private m_Embedded As Boolean = False
    Private m_Exe As String = ""
    Private m_Delay As Integer = 8
    Private m_WindowName As String
    Private m_Process As Process

    Private m_EmbedAppString As String = "EmbedApp3"

    Private m_DisplayName As String = m_EmbedAppString

    Private Sub m_Host_OnSystemEvent(ByVal funct As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
        If Not String.IsNullOrEmpty(m_Exe) Then
            Select Case funct
                Case Is = eFunction.TransitionToPanel
                    If Not m_Embedded Then
                        If arg2 = m_EmbedAppString Then
                            If m_Process Is Nothing Then
                                LoadNavApp()
                            End If
                            OSSpecific.embedApplication(m_Process.ProcessName, m_Screen, m_Host, New OpenMobile.Graphics.Rectangle(0, 98, 1000, 430))
                            m_Embedded = True
                        End If
                    End If

                Case Is = eFunction.TransitionFromAny
                    If m_Embedded Then
                        OSSpecific.unEmbedApplication(m_Process.ProcessName, m_Screen)
                        m_Embedded = False
                    End If

                Case Is = eFunction.closeProgram
                    If Not m_Process Is Nothing Then
                        If Not m_Process.HasExited Then
                            m_Process.Kill()
                        End If
                    End If
            End Select
        End If
    End Sub

    Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel
        m_Screen = screen
        Return New OMPanel
    End Function

    Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage

    End Function

    Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize
        m_Host = host
        LoadNavSettings()

        Return eLoadStatus.LoadSuccessful
    End Function

    Private Sub LoadNavApp()
        m_Process = Process.Start(m_Exe)
        'Give it some time to load before embedding it
        System.Threading.Thread.Sleep(m_Delay * 1000)
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings
        If m_Settings Is Nothing Then
            LoadNavSettings()

            m_Settings = New Settings(m_EmbedAppString)
            m_Settings.Add(New Setting(SettingTypes.File, m_EmbedAppString & ".Exe", "EXE", "App EXE", m_Exe))
            Dim Range As New Generic.List(Of String)
            Range.Add("1")
            Range.Add("20")
            m_Settings.Add(New Setting(SettingTypes.Range, m_EmbedAppString & ".Delay", "Delay", "App Start Up Delay (1-20 Seconds)", Nothing, Range, m_Delay))
            m_Settings.Add(New Setting(SettingTypes.Text, m_EmbedAppString & ".DisplayName", "", "Main Menu Text", m_DisplayName))

            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal St As Setting)
        Using PlugSet As New OpenMobile.Data.PluginSettings

            PlugSet.setSetting(St.Name, St.Value)

            If St.Name = m_EmbedAppString & ".DisplayName" Then
                For i As Integer = 1 To 3
                    For j As Integer = 1 To 3
                        If PlugSet.getSetting("MainMenu.MainMenu" & i & j & ".Plugin") = m_EmbedAppString Then
                            PlugSet.setSetting("MainMenu.MainMenu" & i & j & ".Display", St.Value)
                        End If
                    Next
                Next
            End If

        End Using
    End Sub

    Private Sub LoadNavSettings()
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting(m_EmbedAppString & ".Exe")) Then
                CreateSettings()
            End If
            m_Exe = Settings.getSetting(m_EmbedAppString & ".Exe")
            m_Delay = Settings.getSetting(m_EmbedAppString & ".Delay")
            m_DisplayName = Settings.getSetting(m_EmbedAppString & ".DisplayName")
        End Using
    End Sub

    Private Sub CreateSettings()
        Using Settings As New OpenMobile.Data.PluginSettings
            Settings.setSetting(m_EmbedAppString & ".Exe", "")
            Settings.setSetting(m_EmbedAppString & ".Delay", "8")
            Settings.setSetting(m_EmbedAppString & ".DisplayName", m_EmbedAppString)
        End Using
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
            Return m_EmbedAppString
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return m_EmbedAppString
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public ReadOnly Property displayName() As String Implements OpenMobile.Plugin.IHighLevel.displayName
        Get
            LoadNavSettings()
            Return m_DisplayName
        End Get
    End Property

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Try
                    If m_Embedded Then
                        OSSpecific.unEmbedApplication(m_WindowName, m_Screen)
                    End If
                    If Not m_Process Is Nothing Then
                        If Not m_Process.HasExited Then
                            m_Process.Kill()
                        End If
                    End If
                Catch ex As Exception
                End Try
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
