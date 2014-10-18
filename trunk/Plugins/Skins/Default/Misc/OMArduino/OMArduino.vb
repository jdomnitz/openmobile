﻿#Region "GPL"
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
Imports Microsoft.VisualBasic
Imports OpenMobile
Imports OpenMobile.Controls
Imports OpenMobile.Data
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.helperFunctions
Imports OpenMobile.Plugin
Imports System.Collections.Generic

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

            ' Get any saved settings
            StoredData.SetDefaultValue(Me, "Settings.Verbose", m_Verbose)
            m_Verbose = StoredData.Get(Me, "Settings.Verbose")

            ' Build the main panel
            Dim omArduinopanel As New OMPanel("OMArduino", "OMArduino")
            AddHandler omArduinopanel.Entering, AddressOf panel_enter
            AddHandler omArduinopanel.Leaving, AddressOf panel_leave

            ' Subscribe to the various things we wish to monitor on the panel

            Dim text1lbl As New OMLabel("_text1lbl", OM.Host.ClientArea(0).Left + 50, OM.Host.ClientArea(0).Top + 410, 400, 75)
            text1lbl.Text = "Arduino connected:"
            text1lbl.TextAlignment = Alignment.CenterRight
            text1lbl.Visible = True
            text1lbl.FontSize = 24
            omArduinopanel.addControl(text1lbl)

            Dim text2lbl As New OMLabel("_text2lbl", text1lbl.Left + text1lbl.Width + 5, text1lbl.Top, 200, 75)
            text2lbl.Visible = True
            text2lbl.TextAlignment = Alignment.CenterLeft
            text2lbl.FontSize = 24
            text2lbl.DataSource = "OMDSArduino.Arduino.Connected"
            omArduinopanel.addControl(text2lbl)

            Dim text3lbl As New OMLabel("_text3lbl", text2lbl.Left + text2lbl.Width + 5, text2lbl.Top, 200, 75)
            text3lbl.Visible = True
            text3lbl.TextAlignment = Alignment.CenterLeft
            text3lbl.FontSize = 24
            text3lbl.DataSource = "OMDSArduino.Arduino.Connected"
            omArduinopanel.addControl(text3lbl)

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

            initialized = True

        End Sub

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            'om.host.DebugMsg("OMArduino - Subscription_Updated()", sensor.FullName)

            Dim mPanel As OMPanel
            'Dim mContainer As OMContainer
            Dim pincount As Integer = 0
            Dim status As OMLabel
            Dim thepins As OMLabel
            Dim mIndex As Integer = -1

            'If Not initialized Then Exit Sub

            Select Case sensor.FullName

                Case "OMDSArduino.Arduino.Connected"
                    ' Nothing here for now

                Case "OMDSArduino.Arduino.Pins"
                    ' Data must have been updated if subscription was updated.
                    If m_Verbose Then
                        'OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Pin subscription received."))
                    End If
                    mypins = sensor.Value
                    If Not mypins Is Nothing Then
                        pincount = OM.Host.DataHandler.GetDataSource("OMDSArduino.Arduino.Count").Value
                        If m_Verbose Then
                            'OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Data for {0} pins available.", pincount))
                        End If
                        For x = 0 To OM.Host.ScreenCount - 1
                            If PanelManager.IsPanelLoaded(x, "OMArduino") Then
                                mPanel = PanelManager(x, "OMArduino")
                                If Not mPanel Is Nothing Then
                                    status = mPanel.Find("_text2lbl")
                                    status.Text = String.Format("Yes")
                                    thepins = mPanel.Find("_text3lbl")
                                    thepins.text = String.Format("Pins: {0}", pincount)
                                    Dim x_start As Integer = 30     ' starting LEFT position
                                    Dim x_inc As Integer = 140      ' LEFT increment
                                    Dim x_left As Integer = 0       ' Current LEFT value
                                    Dim y_start As Integer = 20     ' starting TOP position
                                    Dim y_inc As Integer = 130      ' TOP increment
                                    Dim y_top As Integer = 0        ' Current TOP value
                                    Dim z As Integer = 0            ' Loop counter
                                    Dim a1 As String = ""
                                    For z = 0 To pincount - 1
                                        'pin.Name, pin.CurrentValue, pin.CurrentMode, pin.Capabilities
                                        'pin.Image, pin.Label, pin.Title, pin.Descr
                                        'pin = mypins(z)
                                        Dim mImage As OMImage = New OMImage(String.Format("{0}_Image", mypins(z).Name), 0, 0, 100, 100)
                                        mImage.Image = OM.Host.getPluginImage(Me, String.Format("Images|{0}", mypins(z).ImageFile))
                                        mImage.Visible = True
                                        Dim mLabel As OMLabel = New OMLabel(String.Format("{0}_Label", mypins(z).Name), 0, 101, 100, 30, mypins(z).Name)
                                        mLabel.Text = mypins(z).LabelText
                                        mLabel.Visible = True
                                        mLabel.BackgroundColor = Color.Transparent
                                        Dim mButton As OMButton = New OMButton(String.Format("{0}Button", mypins(z).Name), 0, 0, 130, 130)
                                        mButton.Opacity = 0
                                        mButton.Tag = z.ToString
                                        AddHandler mButton.OnClick, AddressOf Button_OnClick
                                        AddHandler mButton.OnHoldClick, AddressOf Button_OnHoldClick
                                        Dim myContainer As OMContainer
                                        myContainer = mPanel.Find(mypins(z).Name)
                                        If myContainer Is Nothing Then
                                            ' Create a container for this pin
                                            If m_Verbose Then
                                                OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Creating screen objects for {0} screen {1}.", mypins(z).Name, x))
                                            End If
                                            myContainer = New OMContainer(mypins(z).Name, OM.Host.ClientArea(0).Left + x_start + x_left, OM.Host.ClientArea(0).Top + y_start + y_top, 130, 130)
                                            myContainer.ScrollBar_Horizontal_Enabled = False
                                            myContainer.ScrollBar_Vertical_Enabled = False
                                            mPanel.addControl(myContainer)
                                            myContainer.addControl(mImage)
                                            myContainer.addControl(mLabel)
                                            myContainer.addControl(mButton)
                                            x_left = x_left + x_inc
                                            If x_left > (OM.Host.ClientArea(0).Right - x_inc) Then
                                                x_left = 0
                                                y_top = y_top + y_inc
                                                If y_top > (OM.Host.ClientArea(0).Bottom - y_inc) Then
                                                    ' we're out of space
                                                End If
                                            End If
                                        Else
                                            If z < 14 Then ' Remove for normal operation (un-terminated analog pins are always triggering data change)
                                                If mypins(z).Changed Then
                                                    If m_Verbose Then
                                                        OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Data changed for {0}.", mypins(z).Name))
                                                    End If
                                                    ' Refresh the stuff in the container
                                                    'theContainer = mPanel.Find(mypins(x).Name)
                                                    If m_Verbose Then
                                                        'OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Cleaning container {0}.", mypins(z).Name))
                                                    End If
                                                    myContainer.ClearControls()
                                                    'mImage.Image = OM.Host.getPluginImage(Me, String.Format("Images|{0}", mypins(z).ImageFile))
                                                    If m_Verbose Then
                                                        'OM.Host.DebugMsg(DebugMessageType.Info, "OMArduino - Subscription_Updated()", String.Format("Adding image."))
                                                    End If
                                                    myContainer.addControl(mImage)
                                                    myContainer.addControl(mLabel)
                                                    myContainer.addControl(mButton)
                                                    If Not String.IsNullOrEmpty(mypins(z).Script) Then
                                                        ' Perform any other actions here (scripting?)
                                                        run_scripts(mypins(z))
                                                    End If
                                                    mypins(z).Changed = False
                                                End If
                                            End If
                                        End If
                                    Next ' Pin Loop
                                Else
                                    ' no panel?
                                End If
                            End If
                        Next ' Screen Loop
                    Else
                        If m_Verbose Then
                            OM.Host.DebugMsg(DebugMessageType.Warning, "OMArduino - Subscription_Updated()", "No PIN data received.")
                        End If
                    End If

            End Select

        End Sub

        Private Sub my_dialog(ByVal sender As OMControl, ByVal screen As Integer, Optional ByVal title As String = "<empty>", Optional ByVal info As String = "")

            Dim myDialog As OpenMobile.helperFunctions.Forms.dialog = New OpenMobile.helperFunctions.Forms.dialog(Me.pluginName, sender.Parent.Name)

            myDialog.Header = title
            myDialog.Text = String.Format(info, sender.Name, sender.Tag)

            myDialog.Icon = OpenMobile.helperFunctions.Forms.icons.Busy
            myDialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK
            myDialog.ShowMsgBox(screen)

        End Sub

        Private Sub Button_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' I/O pin screen object clicked

            Dim x, y As Integer

            'my_dialog(sender, screen, "OMArduino message", String.Format("{0} ({1}) clicked.", sender.Name, sender.Tag))

            ' grab the required pin
            x = Val(sender.Tag)
            pin = mypins(x)

            Select Case pin.CurrentMode
                Case Sharpduino.Constants.PinModes.Input
                Case Sharpduino.Constants.PinModes.Output
                    ' Toggle the OUTPUT
                    If pin.CurrentValue = 0 Then
                        y = 1
                    Else
                        y = 0
                    End If
                    OM.Host.CommandHandler.ExecuteCommand("OMDSArduino.Pins.Execute", {x, "SETVALUE", y})
                Case Sharpduino.Constants.PinModes.Analog
                Case Sharpduino.Constants.PinModes.PWM
                Case Sharpduino.Constants.PinModes.I2C
                Case Sharpduino.Constants.PinModes.Servo
                Case Sharpduino.Constants.PinModes.Shift
            End Select

        End Sub

        Private Sub Button_OnHoldClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' I/O pin screen object long-clicked
            ' Manage user specified settings here

            Dim x As Integer

            my_dialog(sender, screen, "OMArduino message", String.Format("{0} ({1}) long clicked.", sender.Name, sender.Tag))

            ' grab the required pin
            x = Val(sender.Tag)
            pin = mypins(x)

            Select Case pin.CurrentMode
                Case Sharpduino.Constants.PinModes.Input
                    If m_Verbose Then
                        OM.Host.DebugMsg("OMArduino - CommandExecutor()", String.Format("Requesting SETMODE to OUTPUT"))
                    End If
                    OM.Host.CommandHandler.ExecuteCommand("OMDSArduino.Pins.Execute", {x, "SETMODE", Sharpduino.Constants.PinModes.Output})
                Case Sharpduino.Constants.PinModes.Output
                    If m_Verbose Then
                        OM.Host.DebugMsg("OMArduino - CommandExecutor()", String.Format("Requesting SETMODE to INPUT"))
                    End If
                    OM.Host.CommandHandler.ExecuteCommand("OMDSArduino.Pins.Execute", {x, "SETMODE", Sharpduino.Constants.PinModes.Input})
            End Select

        End Sub

        Private Sub run_scripts(pin As OMDSArduino.OMDSArduino.ArduinoIO)
            ' Runs a script attached to a pin

            Dim script As String = pin.Script

            ' Parse the script and process

        End Sub
        Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            ' Create a handler for settings changes
            AddHandler mySettings.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            mySettings.Add(New Setting(SettingTypes.MultiChoice, Me.pluginName & ";Settings.Verbose", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose))

            Return mySettings

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)

            OpenMobile.helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)

            Select Case settings.Name
                Case "OMArduino;Settings.Verbose"
                    m_Verbose = settings.Value
                    StoredData.Set(Me, "Settings.Verbose", m_Verbose)
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

