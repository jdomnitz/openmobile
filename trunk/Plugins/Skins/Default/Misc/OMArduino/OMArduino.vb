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

'    There is one additional restriction when imports this framework regardless of modifications to it.
'    The About Panel or its contents must be easily accessible by the end users.
'    This is to ensure all project contributors are given due credit not only in the source code.

'    2013 by John Mullan jmullan99@gmail.com
'

#End Region

#Region "About"
' 
' OMFuel is a skin interface to display values provided by OMDSFuelPrices
'
' 
' This code requires various Open Mobile libraries
' included in the VB.NET project and MUST be compiled against .NET 3.5

#End Region

Imports System
Imports System.Net
Imports OpenMobile
Imports OpenMobile.Controls
Imports OpenMobile.Data
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.helperFunctions
Imports OpenMobile.Plugin
Imports OMDSArduino.OMDSArduino.ArduinoIO

'Imports Sharpduino.EasyFirmata

Namespace OMArduino

    Public NotInheritable Class OMArduino
        Inherits HighLevelCode

        Private m_Verbose As Boolean = False
        Private bannerStyle As Integer = 0
        Private message As String = ""
        Private PopUpMenuStrip As ButtonStrip
        Private initialized As Boolean = False
        Private loop_count As Integer = 0

        Private pin As OMDSArduino.OMDSArduino.ArduinoIO

        Private mypins(78) As OMDSArduino.OMDSArduino.ArduinoIO

        Public Sub New()

            MyBase.New("OMArduino", OM.Host.getSkinImage("Icons|Icon-Arduino"), 0.1, "OMArduino", "Arduino", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

            'host.DebugMsg("OMFuel - initialize()", "Initializing...")

            initialized = False

            OpenMobile.Threading.SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub BackgroundLoad()

            'om.host.DebugMsg("OMFuel - BackgroundLoad()", "Continuing in background...")

            ' Build the main panel
            Dim omArduinopanel As New OMPanel("OMArduino", "OMArduino")
            AddHandler omArduinopanel.Entering, AddressOf panel_enter
            AddHandler omArduinopanel.Leaving, AddressOf panel_leave

            ' Subscribe to the various things we wish to monitor on the panel

            Dim text1lbl As New OMLabel("_text1lbl", OM.Host.ClientArea(0).Left + 120, OM.Host.ClientArea(0).Top + 410, 400, 75)
            text1lbl.Text = "Arduino connected:"
            text1lbl.TextAlignment = Alignment.CenterRight
            text1lbl.Visible = True
            text1lbl.FontSize = 24
            omArduinopanel.addControl(text1lbl)

            Dim text2lbl As New OMLabel("_text2lbl", text1lbl.Left + text1lbl.Width + 5, text1lbl.Top, 125, 75)
            text2lbl.Visible = True
            text2lbl.TextAlignment = Alignment.CenterLeft
            text2lbl.FontSize = 24
            text2lbl.DataSource = "OMDSArduino.Arduino.Connected"
            omArduinopanel.addControl(text2lbl)

            PanelManager.loadPanel(omArduinopanel, True)

            System.Threading.Thread.Sleep(2000)
            If Not OM.Host.DataHandler.SubscribeToDataSource("OMDSArduino.Arduino.Connected", AddressOf Subscription_Updated) Then
                If m_Verbose Then
                    OM.Host.DebugMsg(DebugMessageType.Warning, "OMArduino - Subscription_Updated()", "Could not subscribe to OMDSArduino.Arduino.Connected")
                End If
            End If
            If Not OM.Host.DataHandler.SubscribeToDataSource("OMDSArduino.Arduino.Pins", AddressOf Subscription_Updated) Then
                If m_Verbose Then
                    OM.Host.DebugMsg(DebugMessageType.Warning, "OMArduino - Subscription_Updated()", "Could not subscribe to OMDSArduino.Arduino.Pins")
                End If
            End If
            mypins = TryCast(OM.Host.DataHandler.GetDataSource("OMDSArduino.Arduino.Pins").Value, Array)

            initialized = True

        End Sub

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            'om.host.DebugMsg("OMArduino - Subscription_Updated()", sensor.FullName)

            Dim mPanel As OMPanel
            'Dim mContainer As OMContainer
            Dim pincount As Integer = 0
            Dim status As OMLabel
            Dim mIndex As Integer = -1

            'If Not initialized Then Exit Sub

            Select Case sensor.FullName

                Case "OMDSArduino.Arduino.Connected"
                    ' Nothing here for now

                Case "OMDSArduino.Arduino.Pins"
                    ' Data must have been updated.  Only happens for INPUT pins
                    mypins = sensor.Value
                    If Not mypins Is Nothing Then
                        pincount = OM.Host.DataHandler.GetDataSource("OMDSArduino.Arduino.Count").Value
                        For x = 0 To OM.Host.ScreenCount - 1
                            If PanelManager.IsPanelLoaded(x, "OMArduino") Then
                                mPanel = PanelManager(x, "OMArduino")
                                If Not mPanel Is Nothing Then
                                    status = mPanel.Find("_text2lbl")
                                    status.Text = String.Format("Yes {0}", pincount)
                                    Dim x_start As Integer = 30     ' starting LEFT position
                                    Dim x_inc As Integer = 140      ' LEFT increment
                                    Dim x_left As Integer = 0       ' Current LEFT value
                                    Dim y_start As Integer = 20     ' starting TOP position
                                    Dim y_inc As Integer = 130      ' TOP increment
                                    Dim y_top As Integer = 0        ' Current TOP value
                                    Dim z As Integer = 0            ' Loop counter
                                    Dim a1 As String = ""
                                    For z = 0 To pincount - 1
                                        'pin.Name
                                        'pin.CurrentValue
                                        'pin.CurrentMode
                                        'pin.Capabilities
                                        'pin.Image
                                        'pin.Label
                                        'pin.Title
                                        'pin.Descr
                                        pin = mypins(z)
                                        Dim mImage As OMImage = pin.Image
                                        Dim mLabel As OMLabel = pin.Label
                                        Dim myContainer As OMContainer
                                        myContainer = mPanel.Find(pin.Name)
                                        If myContainer Is Nothing Then
                                            ' Create a container for this pin
                                            myContainer = New OMContainer(pin.Name, OM.Host.ClientArea(0).Left + x_start + x_left, OM.Host.ClientArea(0).Top + y_start + y_top, 130, 130)
                                            myContainer.ScrollBar_Horizontal_Enabled = False
                                            myContainer.ScrollBar_Vertical_Enabled = False
                                            mPanel.addControl(myContainer)
                                            myContainer.addControl(pin.Image)
                                            myContainer.addControl(pin.Label)
                                            x_left = x_left + x_inc
                                            If x_left > (OM.Host.ClientArea(0).Right - x_inc) Then
                                                x_left = 0
                                                y_top = y_top + y_inc
                                                If y_top > (OM.Host.ClientArea(0).Bottom - y_inc) Then
                                                    ' we're out of space
                                                End If
                                            End If
                                        Else
                                            ' Refresh the stuff in the container
                                            mImage = myContainer.Controls.Find(Function(a) a.Name = String.Format("{0}_Image", pin.Name))
                                            If Not mImage Is Nothing Then
                                                mImage = pin.Image
                                                If pin.Name = "D13" Then
                                                    x = x
                                                End If
                                                mIndex = myContainer.Controls.FindIndex(Function(a) a.Name = String.Format("{0}_Image", pin.Name))
                                            End If
                                            mLabel = myContainer.Controls.Find(Function(a) a.Name = String.Format("{0}_Label", pin.Name))
                                            If Not mLabel Is Nothing Then
                                                mLabel = pin.Label
                                                If pin.Name = "D13" Then
                                                    If pin.CurrentValue = 0 Then
                                                        mLabel.Text = "OFF"
                                                    Else
                                                        mLabel.Text = "ON"
                                                    End If
                                                End If
                                                mLabel.Refresh()
                                            End If
                                            myContainer.Refresh()
                                        End If
                                    Next
                                    mPanel.Refresh()
                                Else
                                    ' no panel?
                                End If
                            End If
                        Next
                    Else
                        If m_Verbose Then
                            OM.Host.DebugMsg(DebugMessageType.Warning, "OMArduino - Subscription_Updated()", "No PIN data received.")
                        End If
                    End If

            End Select

        End Sub

        Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            ' Create a handler for settings changes
            AddHandler mySettings.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            mySettings.Add(New Setting(SettingTypes.MultiChoice, Me.pluginName & ";Settings.VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose))

            Return mySettings

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)

            OpenMobile.helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)

            Select Case settings.Name
                Case "VerboseDebug"
                    m_Verbose = settings.Value
            End Select

        End Sub

        Public Sub panel_leave(ByVal sender As OMPanel, ByVal screen As Integer)

            OM.Host.UIHandler.InfoBanner_Hide(screen)
            'om.host.UIHandler.PopUpMenu.ClearButtonStrip(screen)
            PopUpMenuStrip = Nothing

        End Sub

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            'om.host.DebugMsg("OMFuel - panel_enter()", "")

            ' loads up the popup with items

            PopUpMenuStrip = New ButtonStrip(Me.pluginName, "OMArduino", "PopUpMenuStrip")

            If m_Verbose Then
                OM.Host.DebugMsg("OMArduino - panel_enter()", "....")
            End If

            'om.host.UIHandler.PopUpMenu.SetButtonStrip(PopUpMenuStrip)
            sender.PopUpMenu = PopUpMenuStrip

        End Sub

    End Class

End Namespace

