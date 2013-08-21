Namespace OMLCD

    Public Enum BarDirection As Byte

        Left = 2
        Right = 1

    End Enum

    Public Enum GPOState As Byte

        OffState = 0
        OnState = 1

    End Enum

    Public Enum VerticalBarType As Byte

        Narrow = 1
        Wide = 2

    End Enum

    ' Defines interface for serially connected displays
    Public Interface iLCDInterface

        Event KeyPressReceived(ByVal key As Integer)                      'Informs when a key press was received
        Event KeyAssignReceived(ByVal key As Integer, ByVal name As String)     'Informs when a key press for assignment was received
        Event Connected()                               'Informs when device is connected
        Event NoDevice()                                'Informs when no devices were found

        ReadOnly Property Name() As Driver              ' Name of device specific class in use (driver)
        ReadOnly Property GetModuleType() As String     ' Get model of connected device
        ReadOnly Property GetModuleStyle() As String    ' Get Style of connected device (Graphic or Character)
        ReadOnly Property GetModuleRows() As String     ' Get Number of Rows
        ReadOnly Property GetModuleCols() As String     ' Get Number of Columns
        ReadOnly Property GetModuleGPOs() As String     ' Get Number of General Purpose Outputs
        ReadOnly Property GetVersionNumber() As String  ' Get firmware version of connected device
        ReadOnly Property IsOpen() As Boolean             ' Tests if port is open

        Property Display() As Boolean                   ' Gets/Sets Display on/off
        Property CellSize() As Integer                  ' The number of characters that make up one cell
        Property KeyPadBackLight() As Boolean           ' Gets/Sets Keypad light on/off
        Property KeyPadBrightness() As Integer          ' Keypad brightness level
        Property LCDBrightness() As Integer             ' LCD Backlight Brightness
        Property LCDContrast() As Integer               ' LCD Contrast
        Property AutoScroll() As Boolean
        Property LineWrap() As Boolean
        Property WaitKeyAssign() As Boolean             ' Waiting on single key assignment
        Property Baud() As Integer                      ' Gets/Sets BAUD rate for connected device
        Property Port() As String                       ' Gets/Sets COMPORT for connected device
        Property BK_Red() As Integer
        Property BK_Green() As Integer
        Property BK_Blue() As Integer

        Sub Connect()                                   ' Connect to display device
        Sub Close()                                     ' Disconnect display device

        Sub PrintText(ByVal Text As String)                   ' Send characters to display
        Sub ClearScreen()                               ' Clear the display
        Sub Home()                                      ' Send cursor home
        Sub SwitchBank(ByVal Bank As Byte)              ' Switch Memory Bank
        Sub SetCursorPosition(ByVal Row As Byte, ByVal Column As Byte)
        Sub SetBackgroundColor(ByVal Red As Byte, ByVal Green As Byte, ByVal Blue As Byte)
        Sub SetBrightness(ByVal Brightness As Byte)
        Sub SetContrast(ByVal Contrast As Byte)
        Sub SetKeypadBrightness(ByVal Brightness As Byte)
        Sub SetGPOState(ByVal GPO As Byte, ByVal state As Boolean)

        'Function TurnScreenOff() As Boolean
        'Function TurnScreenOn() As Boolean

        'Function CreateCustomCharater(Id As Byte, Data As Array) As Boolean
        'Function MoveCursorBack() As Boolean
        'Function MoveCursorForward() As Boolean
        'Function PrintHorzBar(Column As Byte, Row As Byte, Direction As BarDirection, Length As Byte) As Boolean
        'Function PrintLargeNumber(Character As String, Column As Byte) As Boolean
        'Function PrintVerticalBar(Column As Byte, Length As Byte, Type As VerticalBarType) As Boolean

        'Function SaveStartupCustomCharater(Id As Byte, Data As Byte) As Boolean
        'Function SaveStartupMessage(Data As Array) As Boolean
        'Function SetAndSaveKeypadBrightness(Brightness As Byte) As Boolean
        'Function SetAutoLineWrap(AutoLineWrapOn As Boolean) As Boolean
        'Function SetAutoScroll(AutoScrollOn As Boolean) As Boolean
        'Function SetBackgroundBrightness(Brightness As Byte) As Boolean
        'Function SetContrast(Contrast As Byte) As Boolean
        'Function SetCursorBlinking(BlinkingOn As Byte) As Boolean
        'Function SetCursorUnderline(UnderlineOn As Boolean) As Boolean
        'Function SetGPO(OutputNumber As Integer, State As GPOState) As Boolean
        'Function SetGPOStartupState(OutputNumber As Integer, State As GPOState) As Boolean
        'Function SetRemember(Remember As Boolean) As Boolean

        'Delegate Sub KeyPressReceivedEventHandler(Key As LCDKey) as void

    End Interface

End Namespace

