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

'    There is one additional restriction when using this framework regardless of modifications to it.
'    The About Panel or its contents must be easily accessible by the end users.
'    This is to ensure all project contributors are given due credit not only in the source code.

'    2011 by John Mullan jmullan99@gmail.com
'

#End Region

#Region "About"
' 
' OMArduino provides Sensors for input pins on a Arduino with a customized Firmata compatible sketch running
'  The settings allow the user to set a label for each pin and define the sensor type.
' Includes a screen for viewing Arduino settings and toggling digital outputs.
' 
' This code requires various Open Mobile libraries as well as the FirmataVB library
' included in the VB.NET project and MUST be compiled agains .NET 3.5

#End Region

Imports System
Imports System.IO.Ports
Imports Microsoft.VisualBasic
Imports OpenMobile
Imports OpenMobile.Controls
Imports OpenMobile.Data
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.Plugin
Imports OpenMobile.Threading
Imports OpenMobile.helperFunctions

Imports Sharpduino.EasyFirmata
Imports Sharpduino.ArduinoUno

Namespace OMDSArduino

    Public Class ArduinoIO
        Inherits Sharpduino.Pin

        Dim myName As String = ""
        Dim myTitle As String = ""
        Dim myScript As String = ""
        Dim myDescr As String = ""
        Dim myImage As OMImage
        Dim myLabel As OMLabel

        '''<summary>
        '''The Program assigned name for the pin
        '''</summary>
        Public Property Name As String
            ' Program assigned Name
            Get
                Return myName
            End Get
            Friend Set(value As String)
                myName = value
            End Set
        End Property

        '''<summary>
        '''User assigned name for the pin
        '''</summary>
        Public Property Title As String
            ' Pin User Supplied Name
            Get
                Return myTitle
            End Get
            Set(value As String)
                myTitle = value
            End Set
        End Property

        '''<summary>
        '''User assigned description for the I/O pin
        '''</summary>
        Public Property Descr As String
            Get
                Return myDescr
            End Get
            Set(value As String)
                myDescr = value
            End Set
        End Property

        '''<summary>
        '''User assigned script
        '''</summary>
        Public Property Script As String
            Get
                Return myScript
            End Get
            Set(value As String)
                myScript = value
            End Set
        End Property

        '''<summary>
        '''Program generated container for on-panel display
        '''</summary>
        Public Property Label As OMLabel
            ' The button definition to use on the panel for the I/O line
            Get
                Return myLabel
            End Get
            Set(value As OMLabel)
                myLabel = value
            End Set
        End Property

        '''<summary>
        '''Program generated container for on-panel display
        '''</summary>
        Public Property Image As OMImage
            ' The button definition to use on the panel for the I/O line
            Get
                Return myImage
            End Get
            Set(value As OMImage)
                myImage = value
            End Set
        End Property

        Public Sub New()
            ' Do nothing
        End Sub

    End Class

    Public NotInheritable Class OMDSArduino
        Inherits BasePluginCode
        Implements IDataSource

        Private WithEvents theHost As IPluginHost

        Public Const INPUT As Integer = 0                   '0x00 digital pin INPUT mode
        Public Const OUTPUT As Integer = 1                  '0x01 digital pin OUTPUT mode
        Public Const ANALOG As Byte = 2                     '0x02 analog pin in analogInput mode
        Public Const PWM As Byte = 3                        '0x03 digital pin in PWM output mode
        Public Const SERVO As Byte = 4                      '0x04 digital pin in Servo output mode
        Public Const SHIFT As Byte = 5                      '0x05 // shiftIn/shiftOut mode
        Public Const I2C As Byte = 6                        '0x06 // pin included in I2C setup
        Public Const ANALOG_MAPPING_RESPONSE As Byte = 106  '0x6A // reply with mapping info
        Public Const CAPABILITY_RESPONSE As Byte = 108      '0x6C // reply with supported modes and resolution
        Public Const PIN_STATE_QUERY As Byte = 109          '0x6D // ask for a pin's current mode and value
        Public Const PIN_STATE_RESPONSE As Byte = 110       '0x6E // reply with pin's current mode and value
        Public Const STRING_DATA As Byte = 113              '0x71 // same as FIRMATA_STRING
        Public Const SHIFT_DATA As Byte = 117               '0x75 a bitstream to/from a shift register
        Public Const I2C_REPLY As Byte = 119                '0x77 begin I2C reply
        Public Const REPORT_FIRMWARE As Byte = 121          '0x79 report name and version of the firmware

        Private statusmsg As String = "Arduino Firmata"
        Private newstatus As Boolean = False
        Private firmware_version_major As Integer = 0
        Private firmware_version_minor As Integer = 0
        Private firmware_version_subver As Integer = 0
        Private firmata_version_major As Integer = 0
        Private firmata_version_minor As Integer = 0
        Private firmware_version_name As String = "Arduino"
        Private initialized As Boolean = False
        Private connected As Boolean = False
        Private settingz As Settings
        Private MaxWait As Int16 = 300
        Private SerialPort1 As System.IO.Ports.SerialPort

        Private m_ComPort As Integer = 0
        Private s_ComPort As String = ""
        Private s_Board As String = "8"
        Private m_Board As Integer = 8
        Private m_Baud As Integer = 57600
        Private vRef As Single = 5.0
        Private useI2C As Boolean = False                   ' True=Automatically set A4/A5 pin mode to I2C

        Private m_Verbose As Boolean = False

        Private shutdown As Boolean = False
        Private toggle As Boolean = False
        Private max_retries As Int16 = 10

        'Private Port As Sharpduino.SerialProviders.ComPortProvider
        Private WithEvents Arduino As Sharpduino.ArduinoUno
        Private thepin As Sharpduino.Pin
        Private Firmata As Sharpduino.EasyFirmata
        Private Port As Sharpduino.SerialProviders.ComPortProvider
        Private mypins(78) As ArduinoIO
        Private theContainer As OMContainer
        Private theImage As OMImage
        Private theLabel As OMLabel

        Public Event lostArduino()

        Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, "OMDSArduino", "")

        'Private waitHandle As New System.Threading.EventWaitHandle(False, System.Threading.EventResetMode.AutoReset)
        Delegate Sub UpdateCtrl(ByVal sender As OMControl, ByVal screen As Integer)

        Private WithEvents m_timer As New Timers.Timer(250) ' Fetch pin info this often
        Private WithEvents m_toggle_timer As New Timers.Timer(500) ' Test toggle LED this often

        Public Sub New()

            MyBase.New("OMDSArduino", OM.Host.getSkinImage("Icons|Icon-Arduino"), 0.1, "Arduino Interface", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As eLoadStatus

            Dim m_Settings As New OpenMobile.Plugin.Settings("Arduino Connector")

            theHost = host

            theHost.UIHandler.AddNotification(myNotification)

            m_Verbose = True

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.initialize()", "Initializing...")
            End If

            OpenMobile.Threading.SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub pin_changed()

            ' Just a place holder.  We need a way to know when data has changed
            ' Use a timer to compare current PIN data to saved PIN data
            ' Update the PIN array and push datasource if any items have been updated

            ' reset flag
            ' Loop through mypins() for inputs, get names, fetch current data
            ' Set flag if different
            ' Loop
            ' flag is set, push datasource

            ' theHost.DataHandler.PushDataSourceValue("OMDSArduino;OMDSArduino.Arduino.Pins", mypins)

        End Sub

        Private Sub BackgroundLoad()

            ' Create the datasource object other plugins can subscribe to
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "Arduino", "Connected", DataSource.DataTypes.binary, "Is Arduino connected"), False)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "Arduino", "Pins", DataSource.DataTypes.raw, "Arduino Pin Data"), Nothing)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "Arduino", "Count", DataSource.DataTypes.raw, "Arduino Pin Count"), 0)

            OpenMobile.helperFunctions.StoredData.SetDefaultValue(Me, "Settings.Verbose", m_Verbose)
            m_Verbose = OpenMobile.helperFunctions.StoredData.Get(Me, "Settings.Verbose")

            OpenMobile.helperFunctions.StoredData.SetDefaultValue(Me, "Settings.ComPort", s_ComPort)
            s_ComPort = OpenMobile.helperFunctions.StoredData.Get(Me, "Settings.ComPort")

            OpenMobile.helperFunctions.StoredData.SetDefaultValue(Me, "Settings.ComBaud", m_Baud)
            m_Baud = OpenMobile.helperFunctions.StoredData.Get(Me, "Settings.ComBaud")

            If String.IsNullOrEmpty(s_ComPort) Then
                myNotification.Text = "No COM port defined."
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino", "No COM port defined.")
                Exit Sub
            End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "Waiting for settings load...")
            End If

            'If (waitHandle.WaitOne(10000) = False) Then
            '' We wait here up to 10 seconds to make sure settings have been loaded
            'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "Halted!  LoadSettings did not complete")
            'Exit Sub
            'End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "...Continuing...")
            End If

            myNotification.Text = "Waiting to contact Arduino..."

            Me.connect()

        End Sub

        Private Sub load_pin_info()

            If Not Arduino Is Nothing Then

                If Arduino.IsInitialized Then

                    ' Do something here

                    Try
                        If m_Verbose Then
                            'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "Getting pin info...")
                        End If
                        Dim imageName As String = ""
                        Dim pin_info As String = ""
                        Dim pin_count As Integer = Arduino.GetPins.Count
                        Dim z As Boolean = True
                        theHost.DataHandler.PushDataSourceValue("OMDSArduino;OMDSArduino.Arduino.Count", pin_count, True)
                        For x = 0 To pin_count - 1
                            ' Build the I/O pin objects
                            mypins(x) = New ArduinoIO
                            mypins(x).CurrentValue = Arduino.GetPins(x).CurrentValue
                            mypins(x).CurrentMode = Arduino.GetPins(x).CurrentMode
                            mypins(x).Capabilities = Arduino.GetPins(x).Capabilities
                            ' Extract capabilities
                            For Each pair As KeyValuePair(Of Sharpduino.Constants.PinModes, Integer) In mypins(x).Capabilities
                                If pair.Key = Sharpduino.Constants.PinModes.Analog Then
                                    mypins(x).Name = String.Format("A{0}", x)
                                    mypins(x).Descr = String.Format("Analog #{0}", x)
                                    imageName = "gauge"
                                End If
                            Next
                            If String.IsNullOrEmpty(mypins(x).Name) Then
                                mypins(x).Name = String.Format("D{0}", x)
                                mypins(x).Descr = String.Format("Digital #{0}", x)
                                If mypins(x).CurrentValue = 0 Then
                                    imageName = "led_off"
                                Else
                                    imageName = "led_red"
                                End If
                            End If
                            mypins(x).Title = mypins(x).Name
                            mypins(x).Script = ""
                            ' Make on-screen objects to be attached to the pins
                            mypins(x).Image = New OMImage(String.Format("{0}_Image", mypins(x).Name), 0, 0, 100, 100)
                            mypins(x).Image.Image = OM.Host.getPluginImage(Me, "Images|" & imageName)
                            mypins(x).Image.Visible = True
                            mypins(x).Label = New OMLabel(String.Format("{0}_Label", mypins(x).Name), 0, 101, 100, 30, mypins(x).Name)
                            mypins(x).Label.Visible = True
                            mypins(x).Label.BackgroundColor = Color.Transparent
                            If m_Verbose Then
                                pin_info = String.Format("Pin {0}> Name:{1} Descr:{2} Label:{3}, Image:{4}", x, mypins(x).Name, mypins(x).Title, mypins(x).Label, imageName)
                                'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", pin_info)
                            End If
                        Next
                        theHost.DataHandler.PushDataSourceValue("OMDSArduino;OMDSArduino.Arduino.Pins", mypins, True)
                    Catch ex As Exception
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", String.Format("ERROR: {0}", ex.Message))
                    End Try
                End If

            End If

        End Sub

        Private Sub myNotification_clickAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        Private Sub myNotification_clearAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        ' Checking for connect/disconnect of the device
        Private Sub m_Host_OnSystemEvent(ByVal efunc As eFunction, args() As Object) Handles theHost.OnSystemEvent

            ' Currently the Arduino doesn't seem to trigger a USBDeviceAdded (even though we connect via USB serial)

            Dim x As Int16 = 0

            ' If there was a USB device connected, see if we can contact it as an ARDUINO
            If efunc.Equals(eFunction.USBDeviceAdded) Then
                ' A USB device was added
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB device added"))
                End If
                If args(0) = "PORT" Then
                    ' A serial port was added
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB Port device added"))
                    End If
                    If Arduino Is Nothing Then
                        ' We currently do not have an active ARDUINO connection
                        'OpenMobile.Threading.SafeThread.Asynchronous(AddressOf BackgroundLoad, theHost)
                    End If
                End If
            End If

            If efunc.Equals(eFunction.USBDeviceRemoved) Then
                x = x
            End If

        End Sub

        Public Sub connect()
            ' Attempt to connect to Arduino by scanning ports

            If Not Arduino Is Nothing Then
                ' no sense connecting if we already are
                Exit Sub
            End If

            Dim access_granted As Boolean = False
            Dim initialized As Boolean = False
            Dim available_ports() As String = System.IO.Ports.SerialPort.GetPortNames
            Array.Sort(available_ports)

            If available_ports.Count = 0 Then
                ' There are no com ports
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found no comm ports."))
                Exit Sub
            End If

            myNotification.Text = "Waiting to contact Arduino..."

            If Not String.IsNullOrEmpty(s_ComPort) Then
                ' Settings specified a com port, try it
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Attempting contact on {0}.", s_ComPort))
                End If
                probe_port(s_ComPort)
            End If
            If Arduino Is Nothing Then
                ' Still no arduino, so scan available ports
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Scanning available serial ports."))
                End If
                ' Loop thru each port name and see if we find a display
                For x = 0 To UBound(available_ports)
                    probe_port(available_ports(x))
                    ' We can check m_Open or m_Connected or m_moduleType
                    If Not Arduino Is Nothing Then
                        ' Looks like a port was found with an LCD
                        s_ComPort = available_ports(x)
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Connected to Arduino on {0}", s_ComPort))
                        End If
                        Exit For
                    End If
                Next
            End If

            If Arduino Is Nothing Then
                ' Did not find Arduino
                myNotification.Text = "Arduino not found."
            Else
                ' Arduino found
                connected = True
                m_timer.Enabled = True
                m_timer.Start()
                m_toggle_timer.Enabled = True
                m_toggle_timer.Start()
                theHost.DataHandler.PushDataSourceValue("OMDSArduino;OMDSArduino.Arduino.Connected", connected)
                myNotification.Text = String.Format("Connected to Arduino on {0}.", s_ComPort)
            End If

        End Sub

        Public Sub probe_port(ByVal portname As String)
            ' Attemp to contact to Arduino

            Dim access_granted As Boolean = False
            Dim initialized As Boolean = False

            If Not Arduino Is Nothing Then
                ' no sense connecting if we already are
                Exit Sub
            End If

            If String.IsNullOrEmpty(s_ComPort) Then
                ' Need a port value to continue
                Exit Sub
            End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "Waiting for serial access.")
            End If

            ' Try a few times to see if we get serial access
            For x = 1 To max_retries
                myNotification.Text = String.Format("Requesting serial lock, attempt #{0}.", x)
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMDSArduino.BackgroundLoad() - Requesting serial lock, attempt #{0}.", x))
                End If
                access_granted = helperFunctions.SerialAccess.GetAccess(Me)
                If access_granted Then
                    Exit For
                End If
            Next

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMDSArduino.BackgroundLoad() - Serial access: {0}.", IIf(access_granted, "granted", "denied")))
            End If

            If Not access_granted Then
                Exit Sub
            End If

            ' We have serial access lock
            Try
                'Port = New Sharpduino.SerialProviders.ComPortProvider(s_ComPort, m_Baud)
                'Firmata = New Sharpduino.EasyFirmata(Port)
                Arduino = New Sharpduino.ArduinoUno(s_ComPort)
                Do While Not Arduino.IsInitialized
                    System.Threading.Thread.Sleep(50)
                Loop
                ' See if we can get a version report
            Catch ex As Exception
                myNotification.Text = String.Format("Error: {0}", ex.Message)
                theHost.DebugMsg(OpenMobile.DebugMessageType.Error, "OMDSArduino.BackgroundLoad()", String.Format("Error: {0}", ex.Message))
                If Not Arduino Is Nothing Then
                    Arduino.Dispose()
                    Arduino = Nothing
                End If
            Finally
                helperFunctions.SerialAccess.ReleaseAccess(Me)
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.BackgroundLoad()", "Serial access lock released.")
                End If
            End Try

        End Sub

        Private Sub toggle_timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_toggle_timer.Elapsed

            m_toggle_timer.Enabled = False
            m_toggle_timer.Stop()

            If Not Arduino Is Nothing Then
                Try
                    If Arduino.IsInitialized Then
                        If toggle = True Then
                            toggle = False
                        Else
                            toggle = True
                        End If
                        Arduino.SetDO(Sharpduino.Constants.ArduinoUnoPins.D13, toggle)
                    End If
                Catch ex As Exception
                    ' Problem with ARDUINO 
                    lost_arduino()
                End Try
            End If

            m_toggle_timer.Enabled = True
            m_toggle_timer.Start()

        End Sub

        Private Sub timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_timer.Elapsed

            m_timer.Enabled = False
            load_pin_info()
            m_timer.Enabled = True

        End Sub

        Public Sub lost_arduino()
            ' Call this when we have an issue with ARDUINO

            myNotification.Text = "Lost connection with Arduino"

            connected = False
            theHost.DataHandler.PushDataSourceValue("OMDSArduino;OMDSArduino.Arduino.Connected", connected)

            Arduino = Nothing

            RaiseEvent lostArduino()

        End Sub
        Public Overrides Function loadSettings() As Settings

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.loadSettings()", "loading.......")
            End If

            Dim ArduinoTypes As New Generic.List(Of String)
            Dim ArduinoValues As New Generic.List(Of String)
            Dim COMOptions As New Generic.List(Of String)
            Dim COMValues As New Generic.List(Of String)
            Dim BAUDOptions As New Generic.List(Of String)
            Dim BAUDValues As New Generic.List(Of String)
            Dim pOptions As New Generic.List(Of String)
            Dim pValues As New Generic.List(Of String)
            Dim p2Options As New Generic.List(Of String)
            Dim p2Values As New Generic.List(Of String)
            Dim YNOptions As New Generic.List(Of String)
            Dim YNValues As New Generic.List(Of String)
            Dim sTypes As New Generic.List(Of String)
            Dim sValues As New Generic.List(Of String)

            Dim available_ports() As String = System.IO.Ports.SerialPort.GetPortNames

            ' To access OMs plugin settings database
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim temp As String
            Dim xFound As Boolean = False

            ' Define settings screen objects
            Me.settingz = New Settings("OMDSArduino")

            ' Find or make the current setting for com port
            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.ComPort")) Then
                ' The comport setting does not exist
                m_Settings.setSetting(Me, "Settings.ComPort", s_ComPort)
            Else
                s_ComPort = m_Settings.getSetting(Me, "Settings.ComPort")
            End If

            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.Baud")) Then
                ' The board setting does not exist
                m_Settings.setSetting(Me, "Settings.Baud", m_Baud.ToString)
            Else
                m_Baud = Val(m_Settings.getSetting(Me, "Settings.Baud"))
            End If

            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.Board")) Then
                ' The board setting does not exist
                m_Settings.setSetting(Me, "Settings.Board", s_Board)
            Else
                s_Board = m_Settings.getSetting(Me, "Settings.Board")
            End If

            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.VRef")) Then
                ' The board setting does not exist
                m_Settings.setSetting(Me, "Settings.Board", vRef.ToString)
            Else
                vRef = Val(m_Settings.getSetting(Me, "Settings.VRef"))
            End If

            ReDim Preserve available_ports(UBound(available_ports) + 1)
            available_ports(UBound(available_ports)) = s_ComPort

            ' Build a list of available ports
            COMOptions.AddRange(available_ports)
            COMOptions.Sort()
            COMValues.AddRange(available_ports)
            COMValues.Sort()

            If COMOptions.Count = 0 Then
                ' No available com ports
                '  revert to default value (empty)
                s_ComPort = ""
            Else
                ' Since there is a value, and a list of ports
                '  look for the value.  If not found revert
                '  to default value (empty)
                If Not String.IsNullOrEmpty(s_ComPort) Then
                    ' There is a value for com port
                    For x = 0 To UBound(available_ports)
                        If available_ports(x) = s_ComPort Then
                            xFound = True
                            ' Ensure it is saved in the settings
                            m_Settings.setSetting(Me, "Settings.ComPort", s_ComPort)
                            Exit For
                        End If
                    Next
                    If Not xFound Then
                        s_ComPort = ""
                    End If
                Else
                    m_Settings.setSetting(Me, "Settings.ComPort", s_ComPort)
                End If
            End If

            ' Create the comport multichoice control

            settingz.Add(New Setting(SettingTypes.MultiChoice, "Settings.ComPort", "COM", "Com Port", COMOptions, COMValues, s_ComPort))

            ' Add setting for baud rate
            BAUDOptions.AddRange({"9600", "14400", "19200", "38400", "57600", "115200"})
            BAUDValues.AddRange({"9600", "14400", "19200", "38400", "57600", "115200"})
            settingz.Add(New Setting(SettingTypes.MultiChoice, "Settings.Baud", "Baud", "Baud Rate", BAUDOptions, BAUDValues, "57600"))

            ' Add setting for reference voltage
            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.VRef")) Then
                m_Settings.setSetting(Me, "Settings.VRef", vRef)
            Else
                vRef = Val(m_Settings.getSetting(Me, "Settings.VRef"))
            End If
            settingz.Add(New Setting(SettingTypes.Text, "Settings.VRef", "VRef", "VRef", vRef.ToString))

            ' Determines if user wants to use I2C on pins A4/A5
            YNOptions.Add("No")
            YNOptions.Add("Yes")
            YNValues.Add("0")
            YNValues.Add("1")
            If String.IsNullOrEmpty(m_Settings.getSetting(Me, "Settings.UseI2C")) Then
                m_Settings.setSetting(Me, "Settings.UseI2C", IIf(useI2C, "1", "0"))
            Else
                useI2C = IIf(m_Settings.getSetting(Me, "Settings.UseI2C") = "1", True, False)
            End If
            temp = IIf(useI2C, "1", "0")
            settingz.Add(New Setting(SettingTypes.MultiChoice, "Settings.UseI2C", "I2C?", "Use I2C?", YNOptions, YNValues, temp))

            settingz.Add(New Setting(SettingTypes.MultiChoice, "Settings.Verbose", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose))

            ' Add settings to define analog sensor type
            sTypes.AddRange({"Raw", "DegreesC", "Degrees", "Percent", "Meters", "Kilometers", "Volts", "Amps", "Kph", "GS", "PSI", "RPM", "Bytes", "BPS", "Binary"})
            sValues.AddRange({"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14"})

            ' Add more settings for arduino pins.
            pOptions.Add("Input")
            pOptions.Add("Output")
            pOptions.Add("Analog")
            pOptions.Add("PWM")
            pOptions.Add("Servo")
            pOptions.Add("Shift")
            pOptions.Add("I2C")
            pValues.Add("0")
            pValues.Add("1")
            pValues.Add("2")
            pValues.Add("3")
            pValues.Add("4")
            pValues.Add("5")
            pValues.Add("6")

            'waitHandle.Set()
            AddHandler settingz.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            'Settings collections are automatically laid out by the framework
            Return Me.settingz

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)

            OpenMobile.helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)

            Select Case settings.Name
                Case "Settings.ComPort" ' If com port changed, try to connect
                    s_ComPort = settings.Value
                    ' New to reload/restart the plugin
                Case "Settings.Verbose" ' Debug logging mode
                    m_Verbose = settings.Value
            End Select

        End Sub

        Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles theHost.OnPowerChange
            Select Case type
                Case Is = ePowerEvent.SleepOrHibernatePending

                Case Is = ePowerEvent.ShutdownPending

                Case Is = ePowerEvent.SystemResumed

            End Select
        End Sub

        Private Sub thehost_OnSystemEvent(ByVal efunc As OpenMobile.eFunction, ByVal args As Object) Handles theHost.OnSystemEvent

            If efunc.Equals(eFunction.closeProgram) Or efunc.Equals(eFunction.CloseProgramPreview) Then
                ' OM Exiting?
                If Not Arduino Is Nothing Then
                    If Arduino.IsInitialized Then
                        Try
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.onSystemEvent()", String.Format("Program closing."))
                            End If
                            ' Reset the Arduino end dispose
                            Arduino.Dispose()
                        Catch ex As Exception
                            ' Problem with ARDUINO
                            lost_arduino()
                        End Try
                    End If
                End If
            End If

            ' If there was a USB device connected, see if we can contact it as an LCD
            If efunc.Equals(eFunction.USBDeviceAdded) Then
                ' A USB device was added
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB device added"))
                End If
                If args(0) = "PORT" Then
                    ' A serial port was added
                End If
            End If

            If efunc.Equals(eFunction.USBDeviceRemoved) Then
                ' Was it a serial port?
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB device removed"))
                End If
                If args(0) = "PORT" Then
                    ' A serial port was removed
                End If
            End If

        End Sub

        Private Function GetSetting(ByVal Name As String, ByVal DefaultValue As String) As String

            Using Settings As New OpenMobile.Data.PluginSettings
                If String.IsNullOrEmpty(Settings.getSetting(Me, Name)) Then
                    Settings.setSetting(Me, Name, DefaultValue)
                End If
                Return Settings.getSetting(Me, Name)
            End Using

        End Function

        Private Function make_bits(ByVal data As Byte) As String

            Dim s_result As String = ""
            For x = 0 To 7
                s_result = s_result + IIf(data And 2 ^ x, "1", "0")
            Next

            Return s_result

        End Function

        Private Function make_byte(ByVal data As Array) As Byte
            ' converts 8 bits in array to one byte

            Dim result As Byte = 0

            For x = 0 To UBound(data)
                result += data(x) * 2 ^ x
            Next

            Return result

        End Function

        Public Function StringToHex(ByVal str As String) As String

            Dim ret As String = ""

            For i As Integer = 0 To str.Length - 1
                ret &= Asc(str.Substring(i, 1)).ToString("x").ToUpper & " "
            Next

            Return ret.Trim()

        End Function

        Public Overrides ReadOnly Property authorName() As String
            Get
                Return "John Mullan"
            End Get
        End Property

        Public Overrides ReadOnly Property authorEmail() As String
            Get
                Return "jmullan99@gmail.com"
            End Get
        End Property

        Public Overrides ReadOnly Property pluginName() As String
            Get
                Return "OMDSArduino"
            End Get
        End Property

        Public ReadOnly Property displayName() As String
            Get
                Return "ArduinoDS"
            End Get
        End Property

        Public Overrides ReadOnly Property pluginIcon() As imageItem
            Get
                Return OM.Host.getPluginImage(Me, "Icon-Arduino")
            End Get
        End Property

        Public Overrides ReadOnly Property pluginVersion() As Single
            Get
                Return 0.1
            End Get
        End Property

        ' <summary>
        ' The name that will be displayed on a main menu button
        ' </summary>
        Public Overrides ReadOnly Property pluginDescription() As String
            Get
                Return "Arduino Interface"
            End Get
        End Property

        Public Overrides Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean
            Return False
        End Function

        Public Overrides Function incomingMessage(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean
            Return False
        End Function

        Public Overrides Sub Dispose()
            Try
                Arduino.Dispose()
            Catch ex As Exception
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMDSArduino.Dispose()", "No object to dispose - " + ex.Message)
                End If
            End Try

        End Sub

        Private Sub m_SysexMessageReady(ByVal theMessage As Array, ByVal theBytes As Integer)
            ' There is a SYSEX message from Firmata that is ready for us to use
            ' What do we do here then?

            Dim character As Byte = 0
            Dim lastSYSEXmessage As String = ""
            Dim lastI2Crecieved As String = ""
            Dim msgtxt As String = ""
            Dim adrtxt As String = ""
            Dim statxt As String = ""
            Dim tempx As String = ""
            Dim msgType As Byte = 0
            Dim theAddress As Byte = 0

            msgType = theMessage(0)

            Select Case msgType

                Case Is = PIN_STATE_RESPONSE ' Answer to PIN_STATE_QUERY
                    ' Message has 4 data bytes

                Case Is = ANALOG_MAPPING_RESPONSE ' reply with mapping info

                Case Is = CAPABILITY_RESPONSE ' Reply with supported modes and resolution

                Case Is = SHIFT_DATA ' bitstream from a shift register

                Case Is = REPORT_FIRMWARE  ' Incoming Firmware Version
                    ' Two+ bytes should show up: MAJOR, MINOR and NAME text
                    '  save to public variables
                    firmware_version_major = theMessage(1)
                    firmware_version_minor = theMessage(2)
                    firmware_version_subver = theMessage(3)
                    'statusmsg = "REPORT_FIRMWARE: " + Trim(Str(firmware_version_major)) + "." + Trim(Str(firmware_version_minor)) + "." + Trim(Str(firmware_version_subver))
                    newstatus = True
                Case Is = I2C_REPLY       ' There was an I2C reply
                    ' Build a text message
                    ' The Reply will have the first byte as the address being read from
                    ' The next 1 or 2 bytes depends on the register address size
                    '   1 byte if normal, 2 bytes if extended.
                    '   The software will need to know how big it was
                    ' We accomplish this by starting from sysexBytesRead - (bytes requested * 2)
                    ' We know none of this during this
                    theAddress = theMessage(1)
                    For x = 3 To theBytes - 1 Step 2
                        character = theMessage(x) + (theMessage(x + 1) << 7)
                        tempx = tempx + character.ToString + " "
                        lastI2Crecieved = lastI2Crecieved + Chr(character)
                    Next
                    statusmsg = "I2C_REPLY from " + Trim(Str(theAddress)) + ": " & lastI2Crecieved
                    newstatus = True
                Case Is = STRING_DATA   ' There is a string message
                    ' Build a text message
                    ' 2-byte character should be a NULL so skip it
                    For x = 1 To theBytes - 1 Step 2
                        character = theMessage(x)
                        character = character + (theMessage(x + 1) << 7)
                        lastSYSEXmessage = lastSYSEXmessage & Chr(character) '& " (" & make_bits(character) & ") "
                    Next
                    Select Case lastSYSEXmessage
                        Case Is = "I2C Write OK"

                        Case Is = "Sampling interval"

                        Case Else
                            For x = 1 To theBytes - 1 Step 2
                                character = theMessage(x)
                                character = character + (theMessage(x + 1) << 7)
                                If x = 1 Then
                                    adrtxt = " from " & character.ToString
                                Else
                                    msgtxt = msgtxt & make_bits(character) & " "
                                End If
                            Next

                    End Select
            End Select

        End Sub

    End Class

End Namespace
