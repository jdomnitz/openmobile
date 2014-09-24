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

'Imports Sharpduino.EasyFirmata

Namespace OMArduino

    Public NotInheritable Class OMArduino
        Inherits HighLevelCode

        Private WithEvents theHost As IPluginHost
        Private m_Verbose As Boolean = False
        Private bannerStyle As Integer = 0
        Private message As String = ""
        Private PopUpMenuStrip As ButtonStrip
        Private initialized As Boolean = False

        Private manager As ScreenManager
        Private thepin As Sharpduino.Pin

        Private mypins(78) As OMDSArduino.OMDSArduino.ArduinoIO

        Dim myBanner As InfoBanner = New InfoBanner(bannerStyle, "Please wait...", 5000)

        Public Sub New()

            MyBase.New("OMArduino", OM.Host.getSkinImage("Icons|Icon-Arduino"), 0.1, "OMArduino", "Arduino", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

            'host.DebugMsg("OMFuel - initialize()", "Initializing...")

            theHost = host

            initialized = False

            OpenMobile.Threading.SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub BackgroundLoad()

            'theHost.DebugMsg("OMFuel - BackgroundLoad()", "Continuing in background...")

            manager = New ScreenManager(Me)

            ' Build the main panel
            Dim omArduinopanel As New OMPanel("OMArduino", "OMArduino")
            AddHandler omArduinopanel.Entering, AddressOf panel_enter
            AddHandler omArduinopanel.Leaving, AddressOf panel_leave

            ' Subscribe to the various things we wish to monitor on the panel

            Dim text1lbl As New OMLabel("_Text1Lbl", theHost.ClientArea(0).Left + 100, theHost.ClientArea(0).Top + 50, 350, 75)
            text1lbl.Text = "Arduino connected:"
            text1lbl.Visible = True
            text1lbl.FontSize = 24
            omArduinopanel.addControl(text1lbl)

            Dim text2lbl As New OMLabel("_Text2Lbl", text1lbl.Left + text1lbl.Width + 10, text1lbl.Top, 100, 75)
            text2lbl.Visible = True
            text2lbl.FontSize = 24
            text2lbl.DataSource = "OMDSArduino;OMDSArduino.Arduino.Connected"
            omArduinopanel.addControl(text2lbl)

            manager.loadPanel(omArduinopanel, True)

            theHost.DataHandler.SubscribeToDataSource("OMDSArduino.Arduino.Connected", AddressOf Subscription_Updated)

            initialized = True

        End Sub

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            'theHost.DebugMsg("OMArduino - Subscription_Updated()", sensor.FullName)

            Dim mPanel As OMPanel

            Select Case sensor.FullName

                Case "OMDSArduino.Arduino.Connected"
                    ' The fuel price list has been updated
                    For x = 0 To theHost.ScreenCount - 1
                        mPanel = PanelManager(x, "OMArduino")
                        If Not mPanel Is Nothing Then
                            mPanel.Controls("text2lbl").DataSource_Refresh()
                        End If
                    Next

                Case "OMDSArduino.Arduino.Pins"
                    For x = 0 To theHost.ScreenCount - 1
                        mPanel = PanelManager(x, "OMArduino")
                        mypins = sensor.Value
                        For Each pin In mypins
                            If Not mPanel Is Nothing Then
                                ' Here we process the PIN data as necessary
                                'pin.Name
                                'pin.CurrentValue
                                'pin.CurrentMode
                                'pin.Capabilities
                                'pin.Button
                                'pin.Script
                                'pin.Title
                            End If
                        Next
                    Next

            End Select

        End Sub

        Public Overrides Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel

            If manager Is Nothing Then
                Return Nothing
            End If

            Return manager(screen, name)

        End Function

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

            theHost.UIHandler.InfoBanner_Hide(screen)
            'theHost.UIHandler.PopUpMenu.ClearButtonStrip(screen)
            PopUpMenuStrip = Nothing

        End Sub

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            'theHost.DebugMsg("OMFuel - panel_enter()", "")

            ' loads up the popup with items

            PopUpMenuStrip = New ButtonStrip(Me.pluginName, "OMArduino", "PopUpMenuStrip")

            If m_Verbose Then
                theHost.DebugMsg("OMArduino - panel_enter()", "....")
            End If

            'theHost.UIHandler.PopUpMenu.SetButtonStrip(PopUpMenuStrip)
            sender.PopUpMenu = PopUpMenuStrip

        End Sub

        Public Overrides ReadOnly Property authorEmail As String
            Get
                Return "jmullan99@gmail.com"
            End Get
        End Property

        Public Overrides ReadOnly Property authorName As String
            Get
                Return "John Mullan"
            End Get
        End Property

        Public Overrides Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean
            Return False
        End Function

    End Class

End Namespace

