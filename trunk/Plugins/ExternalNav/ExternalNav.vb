Imports OpenMobile
Imports OpenMobile.Controls
Imports OpenMobile.Plugin
Imports OpenMobile.Framework
Imports System.Runtime.InteropServices

Public Class ExternalNav
    Implements IHighLevel

    Private WithEvents m_Host As IPluginHost
    Private WithEvents m_Settings As Settings

    Private m_Screen As Integer = 0
    Private m_Embedded As Boolean = False
    Private m_Exe As String = ""
    Private m_Delay As Integer = 8
    Private m_WindowName As String
    Private m_Process As Process

    Private Sub m_Host_OnSystemEvent(ByVal funct As OpenMobile.eFunction, ByVal arg1 As String, ByVal arg2 As String, ByVal arg3 As String) Handles m_Host.OnSystemEvent
        If Not String.IsNullOrEmpty(m_Exe) Then
            Select Case funct
                Case Is = eFunction.TransitionToPanel
                    If Not m_Embedded Then
                        If arg2 = "ExternalNav" Then
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
    End Function

    Private Sub LoadNavApp()
        m_Process = Process.Start(m_Exe)
        'Give it some time to load before moving it
        System.Threading.Thread.Sleep(m_Delay * 1000)
    End Sub

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings
        If m_Settings Is Nothing Then
            LoadNavSettings()

            m_Settings = New Settings("External GPS")
            m_Settings.Add(New Setting(SettingTypes.File, "ExternalNav.Exe", "EXE", "App EXE", m_Exe))
            Dim Range As New Generic.List(Of String)
            Range.Add("1")
            Range.Add("20")
            m_Settings.Add(New Setting(SettingTypes.Range, "ExternalNav.Delay", "Delay", "App Start Up Delay (1-20 Seconds)", Nothing, Range, m_Delay))
           
            AddHandler m_Settings.OnSettingChanged, AddressOf Changed
        End If

        Return m_Settings
    End Function

    Private Sub Changed(ByVal screen As Integer, ByVal St As Setting)
        Dim PlugSet As New OpenMobile.Data.PluginSettings
        PlugSet.setSetting(St.Name, St.Value)
    End Sub

    Private Sub LoadNavSettings()
        Using Settings As New OpenMobile.Data.PluginSettings
            If String.IsNullOrEmpty(Settings.getSetting("ExternalNav.Exe")) Then
                CreateSettings()
            End If
            m_Exe = Settings.getSetting("ExternalNav.Exe")
            m_Delay = Settings.getSetting("ExternalNav.Delay")
        End Using
    End Sub

    Private Sub CreateSettings()
        Using Settings As New OpenMobile.Data.PluginSettings
            Settings.setSetting("ExternalNav.Exe", "")
            Settings.setSetting("ExternalNav.Delay", "8")
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
            Return "Embed External Navigation"
        End Get
    End Property

    Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
        Get
            Return "ExternalNav"
        End Get
    End Property

    Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
        Get
            Return 1
        End Get
    End Property

    Public ReadOnly Property displayName() As String Implements OpenMobile.Plugin.IHighLevel.displayName
        Get
            Return "GPS"
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
