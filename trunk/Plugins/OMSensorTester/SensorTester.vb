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

        m_Manager = New ScreenManager(m_Host.ScreenCount)
        m_Manager.loadPanel(Pan)

        Return eLoadStatus.LoadSuccessful

    End Function

    Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel
        If m_Manager Is Nothing Then
            Return Nothing
        End If

        Dim Pan As OMPanel = m_Manager(screen, name)
        Dim List As OMList = Pan("Sensors")
        List.Clear()

        Dim o As Object
        m_Host.getData(eGetData.GetAvailableSensors, "", o)

        If Not o Is Nothing Then
            Dim Sensors As Generic.List(Of Sensor) = o

            For Each Sen As Sensor In Sensors
                If Sen.Type = eSensorType.Output Then
                    List.Add(New OMListItem(Sen.Name & "-" & m_Host.getsensorvalue(Sen.PID).ToString))
                End If
            Next

        End If

        m_Host.setSensorValue(100101, 1)

        Return Pan
    End Function

    Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

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
