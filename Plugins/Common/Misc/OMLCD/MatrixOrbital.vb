Imports Microsoft.VisualBasic
Imports OpenMobile
Imports OpenMobile.Framework
Imports OpenMobile.Data
Imports OpenMobile.Plugin
Imports System.IO.Ports


Namespace OMLCD

    Public Structure Device
        Public ID As Integer
        Public Model As String
        Public Type As String
        Public Cols As Integer
        Public Rows As Integer
        Public GPOs As Integer

        Public Sub New(ByVal ID As Integer, ByVal Model As String, ByVal Type As String, ByVal Cols As Integer, ByVal Rows As Integer, ByVal GPOs As Integer)
            Me.ID = ID
            Me.Model = Model
            Me.Type = Type
            Me.Cols = Cols
            Me.Rows = Rows
            Me.GPOs = GPOs
        End Sub

    End Structure

    Public Enum LCDKey As Integer

        Key0 = &HA
        Key1 = &HB
        Key2 = &HC
        Key3 = &HD
        Key4 = &HE
        Key5 = &HF
        Key6 = &H10
        Key7 = &H11
        Key8 = &H12
        Key9 = &H13
        Enter = &H6
        TopLeft = &H41      'A'
        UpArrow = &H42      'B'
        RightArrow = &H43   'C'
        LeftArrow = &H44    'D'
        Center = &H45       'E'
        BottomLeft = &H47   'G'
        DownArrow = &H48    'H'

    End Enum

    ' Device specific to Matrix Orbital brand LCD displays (serially connected)
    Public Class Matrix_Orbital
        Implements iLCDInterface

        Public WithEvents SerialPort1 As System.IO.Ports.SerialPort

        Private WithEvents theHost As OpenMobile.Plugin.IPluginHost

        Public Event KeyPressReceived As iLCDInterface.KeyPressReceivedEventHandler _
            Implements iLCDInterface.KeyPressReceived

        Public Event KeyAssignReceived As iLCDInterface.KeyAssignReceivedEventHandler _
            Implements iLCDInterface.KeyAssignReceived

        Public Event Connected As iLCDInterface.ConnectedEventHandler _
            Implements iLCDInterface.Connected

        Public Event NoDevice As iLCDInterface.NoDeviceEventHandler _
            Implements iLCDInterface.NoDevice

        Public Event CommLost As iLCDInterface.CommLostEventHandler _
            Implements iLCDInterface.CommLost

        Public Const DEFAULT_COM_PORT As String = "Auto"    'COM port name 
        Public Const DEFAULT_BAUD_RATE As Integer = 19200   'Baud rate
        Public Const DEFAULT_CELL_SIZE As Integer = 20      'Default character cell size

        Public driver_name As New Driver("Matrix Orbital")  'Our driver name

        Private m_baud_rate As Integer = DEFAULT_BAUD_RATE  'Tracks selected baud rate
        Private m_comm_port As String = DEFAULT_COM_PORT    'Tracks selected comm port
        Private m_open As Boolean = False                   'Tracks if comm port open
        Private m_moduleType As Integer = -1                'Tracks module type
        Private m_moduleVer As Integer = 0                  'Tracks module version
        Private m_CustomerData As String = ""               'Data save to and read from module
        Private m_lastRequest As Queue = New Queue          'Tracks data requests made to device
        ' 1=Model 2=Version
        Private m_Verbose As Boolean = False
        Private m_autoscroll As Boolean = False
        Private m_linewrap As Boolean = False
        Private m_receivedData As Integer = 0               'Received Data
        Private m_display As Boolean = True                 'Tracks display state (on/off)
        Private m_lcd_brightness As Integer = 126           'Tracks LCD Backlight Brightness
        Private m_lcd_contrast As Integer = 126             'Tracks LCD Contrast
        Private m_keypadbacklight As Boolean = True         'Tracks backlight state (on/off)
        Private m_keypad_brightness As Integer = 126        'Tracks keypad brightness
        Private m_bk_red As Integer                         'Tracks backlight RED value
        Private m_bk_green As Integer                       'Tracks backlight GREEN value
        Private m_bk_blue As Integer                        'Tracks backlight BLUE value
        Private mWaitKeyAssign As Boolean                   'Tracks if we are waiting on single button assignment
        Private mKeyAssigned As Integer = 0
        Private mKeyNameAssigned As String = ""
        Private m_cell_size As Integer
        Private lock_timeout As Integer = 10000              'Max milliseconds to wait for serial port lock
        Private query_response_time As Integer = 450        'Milliseconds to wait for query response from LCD

        Private who_called As OpenMobile.Plugin.IBasePlugin

        Private m_connected As Boolean = False              'Are we all done our connection process?

        Public Enum Commands As Byte

            AutoScrollOff = &H52
            AutoScrollOn = &H51
            BlinkCursorOff = &H54
            BlinkCursorOn = &H53
            ChangeStartUpScreen = &H40
            ClearScreen = &H58
            CommandStart = &HFE
            DisplayOff = 70
            DisplayOn = &H42
            GoHome = &H48
            GPOOff = &H56
            GPOOn = &H57
            GPOSetStartupState = &HC3
            InitHorzBar = &H68
            InitNarrowVertBar = &H73
            InitWideVertBar = &H76
            KeypadBacklightOff = &H9B
            LineWrapOff = &H44
            LineWrapOn = &H43
            LoadCustomChars = &HC0
            MoveCursorBack = &H4C
            MoveCursorForward = &H4D
            PlaceHorzBar = &H7C
            PlaceLargeNumber = &H23
            PlaceVertBar = &H3D
            PollKeyPress = &H26
            ReadCustomerData = &H35
            ReadModuleType = &H37
            ReadVersionNumber = &H36
            Remember = &H93
            SaveCustomChar = &HC1
            SaveCustomStartupChar = &HC2
            SaveStartupText = &H40
            SetAndSaveBrightness = &H98
            SetAndSaveContrast = &H91
            SetAndSaveKeypadBrightness = &H9D
            SetBacklightBaseColor = 130
            SetBrightness = &H99
            SetContrast = 80
            SetCursorPosition = &H47
            SetKeypadBrightness = &H9C
            UnderLineCursosOff = &H4B
            UnderLineCursosOn = &H4A
            WriteCustomerData = &H34

        End Enum

        ' List of supported devices (currently only Character types)
        '  One unused code has been used for the emulator
        Dim Devices As List(Of Device) = New List(Of Device) _
            (New Device() { _
             New Device(&H1, "LCD0821", "C", 8, 2, 3), _
             New Device(&H2, "LCD2021", "C", 20, 2, 3), _
             New Device(&H5, "LCD2041", "C", 20, 4, 3), _
             New Device(&H6, "LCD4021", "C", 40, 2, 3), _
             New Device(&H7, "LCD4041", "C", 40, 4, 3), _
             New Device(&H8, "LK202-25", "C", 20, 2, 6), _
             New Device(&H9, "LK204-25", "C", 20, 4, 6), _
             New Device(&HA, "LK404-55", "C", 40, 4, 6), _
             New Device(&HB, "VFD2021", "C", 20, 2, 1), _
             New Device(&HC, "VFD2041", "C", 20, 4, 1), _
             New Device(&HD, "VFD4021", "C", 40, 2, 1), _
             New Device(&HE, "VK202-25", "C", 20, 2, 6), _
             New Device(&HF, "VK204-25", "C", 20, 4, 6), _
             New Device(&H14, "EMULATOR", "C", 20, 4, 6), _
             New Device(&H2B, "LK204-7T-1U", "C", 20, 4, 6), _
             New Device(&H2C, "LK204-7T-1U-USB", "C", 20, 4, 6), _
             New Device(&H31, "LK404-AT", "C", 20, 4, 0), _
             New Device(&H32, "MOS-AV-162A", "C", 16, 2, 3), _
             New Device(&H33, "LK402-12", "C", 40, 2, 7), _
             New Device(&H34, "LK162-12", "C", 16, 2, 7), _
             New Device(&H35, "LK204-25PC", "C", 20, 4, 6), _
             New Device(&H36, "LK202-24-USB", "C", 20, 2, 6), _
             New Device(&H37, "VK202-24-USB", "C", 20, 2, 0), _
             New Device(&H38, "LK204-24-USB", "C", 20, 4, 0), _
             New Device(&H39, "VK204-24-USB", "C", 20, 4, 0), _
             New Device(&H3A, "PK162-12", "C", 16, 2, 7), _
             New Device(&H3B, "VK162-12", "C", 16, 2, 7), _
             New Device(&H3C, "MOS-AP-162A", "C", 16, 2, 3), _
             New Device(&H3D, "PK202-25", "C", 20, 2, 0), _
             New Device(&H3E, "MOS-AL-162A", "C", 16, 2, 3), _
             New Device(&H3F, "MOS-AL-202A", "C", 20, 2, 3), _
             New Device(&H40, "MOS-AV-202A", "C", 20, 2, 3), _
             New Device(&H41, "MOS-AP-202A", "C", 20, 2, 3), _
             New Device(&H42, "PK202-24-USB", "C", 20, 2, 0), _
             New Device(&H43, "MOS-AL-082", "C", 8, 2, 0), _
             New Device(&H44, "MOS-AL-204", "C", 20, 2, 3), _
             New Device(&H45, "MOS-AV-204", "C", 20, 4, 3), _
             New Device(&H46, "MOS-AL-402", "C", 40, 2, 3), _
             New Device(&H47, "MOS-AV-402", "C", 40, 2, 3), _
             New Device(&H48, "LK082-12", "C", 8, 2, 0), _
             New Device(&H49, "VK402-12", "C", 40, 2, 0), _
             New Device(&H4A, "VK404-55", "C", 40, 4, 0), _
             New Device(&H4B, "LK402-25", "C", 40, 2, 0), _
             New Device(&H4C, "VK402-25", "C", 40, 2, 0), _
             New Device(&H4D, "PK204-25", "C", 20, 4, 0), _
             New Device(&H4F, "MOS", "C", 20, 2, 0), _
             New Device(&H4F, "MOI", "C", 20, 2, 0), _
             New Device(&H55, "LK202-25-USB", "C", 20, 2, 0), _
             New Device(&H56, "VK202-25-USB", "C", 20, 2, 0), _
             New Device(&H57, "LK204-25-USB", "C", 20, 4, 0), _
             New Device(&H58, "VK204-25-USB", "C", 20, 4, 0), _
             New Device(&H5B, "LK162-12-TC", "C", 16, 2, 0), _
             New Device(&H73, "LK404-25", "C", 40, 4, 0), _
             New Device(&H74, "VK404-25", "C", 40, 4, 0)})

        Public Sub New(ByVal host As OpenMobile.Plugin.IPluginHost)

            theHost = host
            m_baud_rate = DEFAULT_BAUD_RATE
            m_comm_port = DEFAULT_COM_PORT

        End Sub

        Public Property AutoScroll() As Boolean Implements iLCDInterface.AutoScroll
            Get
                Return m_autoscroll
            End Get
            Set(ByVal value As Boolean)
                m_autoscroll = value
                If m_autoscroll = True Then
                    Me.SendCommand(Commands.AutoScrollOn)
                Else
                    Me.SendCommand(Commands.AutoScrollOff)
                End If
            End Set
        End Property

        Public Property LineWrap() As Boolean Implements iLCDInterface.LineWrap
            Get
                Return m_linewrap
            End Get
            Set(ByVal value As Boolean)
                m_linewrap = value
                If m_linewrap = True Then
                    Me.SendCommand(Commands.LineWrapOn)
                Else
                    Me.SendCommand(Commands.LineWrapOff)
                End If
            End Set
        End Property

        Public Property LCDBrightness() As Integer Implements iLCDInterface.LCDBrightness
            Get
                Return m_lcd_brightness
            End Get
            Set(ByVal value As Integer)
                If value < 0 Then value = 0
                If value > 255 Then value = 255
                m_lcd_brightness = value
                Me.SetBrightness(m_lcd_brightness)
            End Set
        End Property

        Public Property LCDContrast() As Integer Implements iLCDInterface.LCDContrast
            Get
                Return m_lcd_contrast
            End Get
            Set(ByVal value As Integer)
                If value < 0 Then value = 0
                If value > 255 Then value = 255
                m_lcd_contrast = value
                Me.SetContrast(m_lcd_contrast)
            End Set
        End Property

        Public Property KeyPadBrightness() As Integer Implements iLCDInterface.KeyPadBrightness
            Get
                Return m_keypad_brightness
            End Get
            Set(ByVal value As Integer)
                If value < 0 Then value = 0
                If value > 255 Then value = 255
                m_keypad_brightness = value
                Me.SetKeypadBrightness(m_keypad_brightness)
            End Set
        End Property

        Public ReadOnly Property Name() As Driver Implements iLCDInterface.Name
            Get
                Return driver_name
            End Get
        End Property

        Public Property CellSize() As Integer Implements iLCDInterface.CellSize
            Get
                Return m_cell_size
            End Get
            Set(ByVal value As Integer)
                m_cell_size = value
            End Set
        End Property

        Public ReadOnly Property Default_Baud() As Integer
            Get
                Return DEFAULT_BAUD_RATE
            End Get
        End Property

        Public ReadOnly Property Default_Port() As String
            Get
                Return DEFAULT_COM_PORT
            End Get
        End Property

        Public Property Baud() As Integer Implements iLCDInterface.Baud
            Get
                Return m_baud_rate
            End Get
            Set(ByVal value As Integer)
                m_baud_rate = value
            End Set
        End Property

        Public Property Port() As String Implements iLCDInterface.Port
            Get
                Return m_comm_port
            End Get
            Set(ByVal value As String)
                m_comm_port = value
            End Set
        End Property

        Public Property BK_Red() As Integer Implements iLCDInterface.BK_Red
            Get
                Return m_bk_red
            End Get
            Set(ByVal value As Integer)
                m_bk_red = value
            End Set
        End Property

        Public Property BK_Green() As Integer Implements iLCDInterface.BK_Green
            Get
                Return m_bk_green
            End Get
            Set(ByVal value As Integer)
                m_bk_green = value
            End Set
        End Property

        Public Property BK_Blue() As Integer Implements iLCDInterface.BK_Blue
            Get
                Return m_bk_blue
            End Get
            Set(ByVal value As Integer)
                m_bk_blue = value
            End Set
        End Property

        Public Property WaitKeyAssign() As Boolean Implements iLCDInterface.WaitKeyAssign
            Get
                Return mWaitKeyAssign
            End Get
            Set(ByVal value As Boolean)
                mWaitKeyAssign = value
            End Set
        End Property

        Public ReadOnly Property IsOpen() As Boolean Implements iLCDInterface.IsOpen
            Get
                Return m_open
            End Get
        End Property

        Public ReadOnly Property GetModuleType() As String Implements iLCDInterface.GetModuleType
            ' Reports model name
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                If String.IsNullOrEmpty(FoundDevice.Model) Then
                    m_moduleType = 0
                    Return "<none>"
                Else
                    Return FoundDevice.Model
                End If
            End Get
        End Property

        Public ReadOnly Property GetModuleStyle() As String Implements iLCDInterface.GetModuleStyle
            ' Reports module style (Graphic or Character)
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                Select Case FoundDevice.Type
                    Case Is = "G" ' Grahpical type
                        Return "Grahpical"
                    Case Is = "C" ' Character type
                        Return "Character"
                    Case Else
                        Return "<unknown>"
                End Select
            End Get
        End Property

        Public ReadOnly Property GetModuleRows() As String Implements iLCDInterface.GetModuleRows
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                Return FoundDevice.Rows
            End Get
        End Property

        Public ReadOnly Property GetModuleGPOs() As String Implements iLCDInterface.GetModuleGPOs
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                Return FoundDevice.GPOs
            End Get
        End Property

        Public ReadOnly Property GetModuleCols() As String Implements iLCDInterface.GetModuleCols
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                Return FoundDevice.Cols
            End Get
        End Property

        Public ReadOnly Property GetVersionNumber() As String Implements iLCDInterface.GetVersionNumber
            Get
                Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                Return m_moduleVer
            End Get
        End Property

        Public Property Display() As Boolean Implements iLCDInterface.Display
            Get
                Return m_display
            End Get
            Set(ByVal value As Boolean)
                Try
                    If m_open Then
                        If value = False Then
                            Me.SerialPort1.Write({254, Commands.DisplayOff}, 0, 2)
                            m_display = False
                        Else
                            Me.SerialPort1.Write({254, Commands.DisplayOn, 0}, 0, 3)
                            m_display = True
                        End If
                    End If
                Catch ex As Exception
                    disconnected()
                End Try
            End Set
        End Property

        Public Property KeyPadBacklight() As Boolean Implements iLCDInterface.KeyPadBackLight
            Get
                Return m_keypadbacklight
            End Get
            Set(ByVal value As Boolean)
                Try
                    If m_open Then
                        If value = False Then
                            Me.SerialPort1.Write({254, Commands.KeypadBacklightOff}, 0, 2)
                            m_keypadbacklight = False
                        Else
                            Me.SerialPort1.Write({254, Commands.SetKeypadBrightness, 255}, 0, 3)
                            m_keypadbacklight = True
                        End If
                    End If
                Catch ex As Exception
                    disconnected()
                End Try
            End Set
        End Property

        ' Try to find a compatible LCD
        Public Sub Connect(caller As OpenMobile.Plugin.IBasePlugin, Optional verbose As Boolean = False) Implements iLCDInterface.Connect

            ' If the current saved port name = "Auto" then we try to find a compatible device
            ' If the saved port name is a valid port, we will just connect to it
            ' If we had issue connecting, we will try to find a port
            ' If we are successful then Hurray!

            m_Verbose = verbose
            who_called = caller
            m_moduleType = -1

            ' If there is already a port opened, close it
            If Not Me.SerialPort1 Is Nothing Then
                If Me.SerialPort1.IsOpen Then
                    Me.SerialPort1.Close()
                    Me.SerialPort1.Dispose()
                End If
                Me.SerialPort1 = Nothing
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Existing connection was terminated."))
                End If
            End If

            m_open = False
            m_connected = False

            If m_comm_port = "Auto" Then
                ' Let's auto-detect
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Auto detecting......"))
                End If
                auto_detect()
            Else
                ' Try to connect to the last comm port
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Attempting to connect to last known port: {0}", m_comm_port))
                End If
                probe_port(m_comm_port, m_baud_rate)
                If m_moduleType = -1 Then
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Previous port didn't work, switching to auto......"))
                    End If
                    m_comm_port = "Auto"
                    auto_detect()
                End If
            End If

            ' If module type is = 0, we have no matrix orbital
            If m_moduleType <> -1 Then
                RaiseEvent Connected()
            Else
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found no {0} device.", driver_name))
                RaiseEvent NoDevice()
            End If

        End Sub

        Private Sub probe_port(ByVal portname As String, Optional ByVal baudrate As Int16 = DEFAULT_BAUD_RATE)
            ' Try to connect to portname

            ' Wait our turn to access serial ports
            Dim access_granted As Boolean = False

            For x = 1 To 5
                access_granted = helperFunctions.SerialAccess.GetAccess(who_called)
                If access_granted Then
                    Exit For
                End If
            Next

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Serial access: {0}.", IIf(access_granted, "granted", "denied")))
            End If

            If Not access_granted Then
                Exit Sub
            End If

            Dim startTime1 As DateTime ' Wait loop for module type
            Dim startTime2 As DateTime ' Wait loop for module version
            Dim elapsedTime As TimeSpan

            Me.SerialPort1 = New System.IO.Ports.SerialPort

            If Me.SerialPort1.IsOpen Then
                Me.SerialPort1.Close()
            End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Probing {0}.", portname))
            End If

            Me.SerialPort1.PortName = portname
            Me.SerialPort1.BaudRate = baudrate
            Me.SerialPort1.DataBits = 8
            Me.SerialPort1.Parity = IO.Ports.Parity.None
            Me.SerialPort1.StopBits = IO.Ports.StopBits.One
            Me.SerialPort1.Handshake = IO.Ports.Handshake.None
            Me.SerialPort1.WriteTimeout = 250
            Try
                Me.SerialPort1.Open()
                ' see if we can grab a model number
                If Me.SerialPort1.IsOpen() Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Opened {0}...", portname))
                    m_lastRequest.Enqueue("1")
                    ' Send command to get module type
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Querying for model code..."))
                    Me.SerialPort1.Write({254, Commands.ReadModuleType}, 0, 2)
                    ' Start Counting
                    startTime1 = Now()
                    Dim FoundDevice As Device = Devices.Find(Function(c) c.ID = m_moduleType)
                    ' Wait 500 ms to see if we get a response
                    Do While True
                        If m_moduleType <> -1 Then
                            ' We got a response to our query
                            FoundDevice = Devices.Find(Function(c) c.ID = m_moduleType)
                            If String.IsNullOrEmpty(FoundDevice.Model) Then
                                ' It was not a supported module code
                                ' Move on to next port
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Unsupported Model response: {0}", m_moduleType))
                                m_moduleType = -1
                                Exit Do
                            End If
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Model response: {0}", m_moduleType))
                            ' Ask for a version code
                            'm_lastRequest.Enqueue("2")
                            'Me.SerialPort1.Write({254, Commands.ReadVersionNumber}, 0, 2)
                            m_CustomerData = ""
                            Me.SerialPort1.Write({254, Commands.WriteCustomerData, 97, 98, 97, 98, 97, 98, 97, 98, 97, 98, 97, 98, 97, 98, 97, 98}, 0, 18)
                            m_lastRequest.Enqueue("3")
                            Me.SerialPort1.Write({254, Commands.ReadCustomerData}, 0, 2)
                            ' Start counting
                            startTime2 = Now()
                            ' Wait for a response
                            Do While True
                                If m_CustomerData = "abababababababab" Then
                                    ' We must be talking to the display
                                    If m_Verbose Then
                                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "Read/Write test: Passed.")
                                    End If
                                    Exit Do
                                End If
                                'If m_moduleVer <> 0 Then
                                'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Version response: {0}", m_moduleVer))
                                'Exit Do
                                'End If
                                ' No version code yet.  If we waited too long, get outta here
                                elapsedTime = Now().Subtract(startTime2)
                                If elapsedTime.Milliseconds > query_response_time Then
                                    ' No validation code
                                    Exit Do
                                End If
                                ' Adds a little pause between loops
                                System.Threading.Thread.Sleep(50)
                            Loop
                            Exit Do
                        End If
                        ' We did not get a module code yet
                        ' If we waited too long for this port to responsd then get outta here
                        elapsedTime = Now().Subtract(startTime1)
                        If elapsedTime.Milliseconds > query_response_time Then
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Timeout. No response from {0}", portname))
                            End If
                            Exit Do
                        End If
                        ' Adds a little pause between loops
                        System.Threading.Thread.Sleep(50)
                    Loop
                    If m_moduleType = -1 Then
                        Me.SerialPort1.Close()
                    ElseIf m_CustomerData <> "abababababababab" Then
                        Me.SerialPort1.Close()
                    Else
                        m_comm_port = portname
                        m_open = True
                        m_connected = True
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found {0} on {1}", Me.GetModuleType, m_comm_port))
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Display type: {0}", Me.GetModuleStyle))
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("{0} Rows by {1} Cols", Me.GetModuleRows, Me.GetModuleCols))
                        End If
                    End If
                Else
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("{0} did not open.", portname))
                    End If
                End If
            Catch ex As Exception
                ' if port is already in use, we should get an exception
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Port {0} not available. {1}", portname, ex.Message))
            End Try

            ' Reqlinquish our hold on serial ports
            helperFunctions.SerialAccess.ReleaseAccess(who_called)

            m_lastRequest.Clear()

            ' Finished testing this port

        End Sub

        ' auto_detect will terminate any existing connection
        Private Sub auto_detect()

            Dim x As Integer

            ' Try to find a display
            ' Get a list of available com ports

            Dim available_ports() As String = System.IO.Ports.SerialPort.GetPortNames
            Array.Sort(available_ports)

            If available_ports.Count = 0 Then
                ' There are no com ports
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found no comm ports."))
            Else
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Scanning available serial ports."))
                End If
                ' Loop thru each port name and see if we find a display
                For x = 0 To UBound(available_ports)
                    probe_port(available_ports(x))
                    ' We can check m_Open or m_Connected or m_moduleType
                    If m_moduleType <> -1 Then
                        ' Looks like a port was found with an LCD
                        m_comm_port = available_ports(x)
                        Exit For
                    End If
                Next
                End If

        End Sub

        ' Checking for connect/disconnect of the device
        Private Sub m_Host_OnSystemEvent(ByVal efunc As eFunction, args() As Object) Handles theHost.OnSystemEvent

            ' If there was a USB device connected, see if we can contact it as an LCD
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
                    If Not m_open Then
                        ' We don't currently have a port open, so try to open it
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("We are not currently connected, initiate connection."))
                        End If
                        Me.Connect(who_called, m_Verbose)
                    End If
                End If
            End If

            If efunc.Equals(eFunction.USBDeviceRemoved) Then
                ' Was it a serial port?
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB device removed"))
                End If
                If args(0) = "PORT" Then
                    ' A serial port was removed
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("USB Port device removed"))
                    End If
                    If m_open Then
                        ' We had an open port
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("We are/were connected to a serial port"))
                        End If
                        Try
                            ' See if we lost our port
                            ' Port is supposedly open
                            If Me.SerialPort1.IsOpen Then
                                ' If the test did not fail, then the port is open
                                If m_Verbose Then
                                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Our serial port object is still open."))
                                End If
                            Else
                                If m_Verbose Then
                                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Our serial port seems closed"))
                                End If
                                disconnected()
                                End If
                        Catch
                            ' The port test failed, so we likely lost the port
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Seems our serial port object has been lost."))
                            End If
                            disconnected()
                        End Try
                    End If
                End If
            End If

        End Sub

        ' Call when connection is lost
        Private Sub disconnected()

            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Connection lost."))
            m_open = False
            m_connected = False
            Try
                Me.SerialPort1.Close()
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Closed serial port object"))
                End If
                Me.SerialPort1.Dispose()
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Disposed serial port object"))
                End If
            Catch ex As Exception
                ' Likely the OS already closed the serial port object on us
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Error closing serial port object: {0}...", ex.Message))
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "...is OS closing?")
                End If
            End Try
            RaiseEvent CommLost()

        End Sub

        ' Handles SerialPort Datarecieved
        Private Sub SerialPort1_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
            'Invoke the procedure to process input
            ProcessInput()
        End Sub

        ' Procedure to process receieved serial data
        Private Sub ProcessInput()

            Try

                Dim m_request As String = ""
                If m_lastRequest.Count > 0 Then
                    m_request = m_lastRequest.Peek()
                End If

                Do While SerialPort1.BytesToRead > 0
                    Dim inputData As Integer
                    inputData = SerialPort1.ReadByte
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Request queue value: {0}", m_request))
                    End If
                    Select Case m_request
                        Case "0" ' No particular request
                        Case "1" ' Module Number request
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Received response {0} to request type {1}.", inputData, m_request))
                            End If
                            m_moduleType = inputData
                            m_lastRequest.Dequeue()
                        Case "2" ' Version request
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Received response {0} to request type {1}.", inputData, m_request))
                            End If
                            m_moduleVer = inputData
                            m_lastRequest.Dequeue()
                        Case "3" ' Version request
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Received response {0} to request type {1}.", inputData, m_request))
                            End If
                            m_CustomerData = m_CustomerData + Chr(inputData)
                            If Len(m_CustomerData) = 16 Then
                                m_lastRequest.Dequeue()
                            End If
                        Case Else
                            ' We did not ask for data
                            If inputData > &H40 And inputData < &H49 Then
                                ' Keypad button press received
                                Me.SerialPort1.DiscardInBuffer()
                                If Me.WaitKeyAssign = True Then
                                    WaitKeyAssign = False
                                    Dim key As LCDKey = inputData
                                    mKeyAssigned = key
                                    mKeyNameAssigned = key.ToString()
                                    If m_Verbose Then
                                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Input/Key/Name: {0} / {1} / {2}", inputData, mKeyAssigned, mKeyNameAssigned))
                                    End If
                                    RaiseEvent KeyAssignReceived(inputData, mKeyNameAssigned)
                                    SerialPort1.DiscardInBuffer()
                                Else
                                    RaiseEvent KeyPressReceived(inputData)
                                End If
                            End If
                    End Select
                Loop
            Catch ex As Exception
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Problem processing received data. " & ex.Message))
            End Try

        End Sub

        Public Sub Close() Implements iLCDInterface.Close

            Try
                Me.ResetGPOStates()
                Me.ShowStartupScreen()
                Me.SerialPort1.Close()
                Me.SerialPort1.Dispose()
                Me.SerialPort1 = Nothing
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Connection was terminated."))
                End If
            Catch ex As Exception
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Issue closing device. " & ex.Message))
            End Try

            m_open = False
            m_connected = False
            m_moduleType = 0
            m_comm_port = DEFAULT_COM_PORT

        End Sub

        Public Sub SendCommand(ByVal cmd As Byte)
            ' Send single byte control command to display
            Dim data() As Byte = {254, cmd}
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try
        End Sub

        Public Sub SendCommands(ByVal cmds As Array)
            ' Sends a custom command array
            Dim data() As Byte = cmds
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub ClearScreen() Implements iLCDInterface.ClearScreen
            ' Blank out the display
            Try
                If m_open Then
                    Me.SendCommand(Commands.ClearScreen)
                    Me.SendCommand(Commands.GoHome)
                End If
            Catch ex As Exception
                disconnected()
            End Try
        End Sub

        Public Sub Home() Implements iLCDInterface.Home
            ' Put cursor home
            Try
                If m_open Then
                    Me.SendCommand(Commands.GoHome)
                End If
            Catch ex As Exception
                disconnected()
            End Try
        End Sub

        Public Sub SetBackgroundColor(ByVal Red As Byte, ByVal Green As Byte, ByVal Blue As Byte) Implements iLCDInterface.SetBackgroundColor
            ' Set backlight color
            m_bk_red = Red
            m_bk_green = Green
            m_bk_blue = Blue

            Dim data() As Byte = {254, Commands.SetBacklightBaseColor, m_bk_red, m_bk_green, m_bk_blue}

            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub SetContrast(ByVal Contrast As Byte) Implements iLCDInterface.SetContrast

            Dim data() As Byte = {254, Commands.SetContrast, Contrast}
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                    m_lcd_contrast = Contrast
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub ResetGPOStates()

            For x = 0 To GetModuleGPOs - 1
                SetGPOState(x, 0)
            Next

        End Sub

        Public Sub SetGPOState(ByVal GPO As Byte, ByVal state As Boolean) Implements iLCDInterface.SetGPOState

            Dim data() As Byte = {254, Commands.GPOOff, 0}

            Try
                If m_open Then
                    If state = True Then
                        'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting GPO{0} to ON.", GPO))
                        ReDim data(2)
                        data = {254, Commands.GPOOn, GPO}
                    Else
                        'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting GPO{0} to OFF.", GPO))
                        ReDim data(2)
                        data = {254, Commands.GPOOff, GPO}
                    End If
                    Me.SerialPort1.Write(data, 0, 3)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub SetBrightness(ByVal brightness As Byte) Implements iLCDInterface.SetBrightness

            Dim data() As Byte = {254, Commands.SetBrightness, brightness}
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                    m_lcd_brightness = brightness
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub SetKeypadBrightness(ByVal Brightness As Byte) Implements iLCDInterface.SetKeypadBrightness

            Dim data() As Byte = {254, Commands.SetKeypadBrightness, Brightness}
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                    m_keypad_brightness = Brightness
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub SetCursorPosition(ByVal Row As Byte, ByVal Column As Byte) Implements iLCDInterface.SetCursorPosition
            Dim data() As Byte = {254, Commands.SetCursorPosition, Column, Row}
            Try
                If m_open Then
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub SwitchBank(ByVal Bank As Byte) Implements iLCDInterface.SwitchBank
            Dim changeBank() As Byte = {254, 192, Bank}
            Try
                If m_open Then
                    Me.SerialPort1.Write(changeBank, 0, UBound(changeBank) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub RebuildStartupScreen()
            ' Builds and saves the startup screen with custom characters in Bank 0

            Dim data() As Byte = {254, Commands.ChangeStartUpScreen, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, _
                                  32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 0, 1, 2, 3, 32, 32, Asc("O"), _
                                  Asc("P"), Asc("E"), Asc("N"), 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 4, 5, 6, 7, _
                                  32, 32, 32, 32, Asc("M"), Asc("O"), Asc("B"), Asc("I"), Asc("L"), Asc("E"), 32, 32, 32, _
                                  32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32}

            Try
                If m_open Then
                    Me.ReloadStartupCharacters()
                    Me.SerialPort1.Write(data, 0, UBound(data) + 1)
                    Me.ShowStartupScreen()
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub ReloadStartupCharacters()
            ' Rebuilds the custom characters for splash screen
            ' Saves the custom characters into Bank 0 for use in startup screen

            ' Data for OM Logo
            Dim char1() As Byte = {254, Commands.SaveCustomStartupChar, 0, 0, 1, 6, 8, 9, 18, 28, 28}
            Dim char2() As Byte = {254, Commands.SaveCustomStartupChar, 1, 15, 16, 7, 24, 0, 0, 24, 12}
            Dim char3() As Byte = {254, Commands.SaveCustomStartupChar, 2, 30, 1, 28, 3, 0, 0, 6, 12}
            Dim char4() As Byte = {254, Commands.SaveCustomStartupChar, 3, 0, 16, 12, 2, 18, 9, 7, 7}
            Dim char5() As Byte = {254, Commands.SaveCustomStartupChar, 4, 28, 28, 4, 4, 2, 1, 0, 0}
            Dim char6() As Byte = {254, Commands.SaveCustomStartupChar, 5, 0, 0, 1, 6, 0, 0, 24, 7}
            Dim char7() As Byte = {254, Commands.SaveCustomStartupChar, 6, 0, 12, 16, 0, 0, 0, 3, 28}
            Dim char8() As Byte = {254, Commands.SaveCustomStartupChar, 7, 7, 7, 4, 4, 8, 16, 0, 0}

            ' Write the characters to the display
            Try
                If m_open Then
                    Me.SerialPort1.Write(char1, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char2, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char3, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char4, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char5, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char6, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char7, 0, UBound(char1) + 1)
                    Me.SerialPort1.Write(char8, 0, UBound(char1) + 1)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub ShowStartupScreen()
            ' Show the startup splash screen
            ' Must be done manually because stored startup screen only happens
            '  on LCD power-up / restart

            ' Select Bank Command
            Dim cmd() As Byte = {254, Commands.LoadCustomChars, 0}
            ' Startup Page Data
            Dim data() As Byte = {32, 32, 32, 32, 32, 32, 32, 32, 32, 32, _
                                  32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 0, 1, 2, 3, 32, 32, Asc("O"), _
                                  Asc("P"), Asc("E"), Asc("N"), 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 4, 5, 6, 7, _
                                  32, 32, 32, 32, Asc("M"), Asc("O"), Asc("B"), Asc("I"), Asc("L"), Asc("E"), 32, 32, 32, _
                                  32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32}

            Try
                If m_open Then
                    Me.Home()
                    Me.SerialPort1.Write(cmd, 0, 3)
                    Me.SerialPort1.Write(data, 0, 80)
                End If
            Catch ex As Exception
                disconnected()
            End Try

        End Sub

        Public Sub PrintText(ByVal Text As String) Implements iLCDInterface.PrintText
            ' Send one or more characters to the display

            Dim y As Integer

            If Len(Text) = 0 Then
                Return
            End If

            Try
                If m_open Then
                    For y = 1 To Len(Text)
                        Me.SerialPort1.Write(Mid(Text, y, 1))
                    Next
                End If
            Catch ex As Exception

            End Try

        End Sub

    End Class

End Namespace

