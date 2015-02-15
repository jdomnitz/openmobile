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

Imports OpenMobile
Imports OpenMobile.Controls
Imports OpenMobile.Data
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports Microsoft.VisualBasic
Imports OpenMobile.Plugin
Imports System.IO.Ports
Imports OpenMobile.Threading

Namespace OMLCD

    ' Class to hold displayable "blocks" for the attached display

    Public Class display_items_struct

        Private _source As String
        Private _label As String
        Private _isnew As Boolean
        Private _value As String
        Private _onScreen As Boolean
        Private _lastChar As Integer

        Public Sub New(ByVal source As String, ByVal label As String, ByVal isnew As Boolean, ByVal value As String, ByVal onscreen As Boolean, ByVal lastchar As Integer)
            Me._source = source     ' data source
            Me._label = label       ' user assigned label
            Me._isnew = isnew       ' updated?
            Me._value = value       ' value of data source
            Me._onScreen = onscreen ' current on screen?
            Me._lastChar = lastchar ' last displayed character (for scrolling)
        End Sub

        Public Property Source As String
            Get
                Return Me._source
            End Get
            Set(ByVal value As String)
                Me._source = value
            End Set
        End Property

        Public Property Label As String
            Get
                Return Me._label
            End Get
            Set(ByVal value As String)
                Me._label = value
            End Set
        End Property

        Public Property IsNew As Boolean
            Get
                Return Me._isnew
            End Get
            Set(ByVal value As Boolean)
                Me._isnew = value
            End Set
        End Property

        Public Property Value As String
            Get
                Return Me._value
            End Get
            Set(ByVal value As String)
                Me._value = value
            End Set
        End Property

        Public Property OnScreen As Boolean
            Get
                Return Me._onScreen
            End Get
            Set(ByVal value As Boolean)
                Me._onScreen = value
            End Set
        End Property

        Public Property lastChar As Integer
            Get
                Return Me._lastChar
            End Get
            Set(ByVal value As Integer)
                If value > Len(Me._value) Then
                    Me._lastChar = Len(Me._value)
                Else
                    Me._lastChar = value
                End If
            End Set
        End Property

    End Class

    Public Structure Driver
        Public Name As String

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub

    End Structure

    ' Main class for interacting with serially connected displays

    Public Class OMLCD
        Implements IHighLevel

        Private Enum ButtonEvents As Integer
            None = 1
            Fn
            NextSong
            PreviousSong
            PausePlay
            VolumeUp
            VolumeDown
            Mute
            RestartOM
            Reboot
            Radio
            Home
            Music
            Navigation
        End Enum

        Public WithEvents LCD As Matrix_Orbital

        Public Drivers As List(Of Driver) = New List(Of Driver) _
            (New Driver() { _
             New Driver("<none>")})

        Private manager As ScreenManager

        'Array of common baud rates
        Private BaudRatesListValues() As String = New String() {"300", "1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200"}

        Private WithEvents theHost As IPluginHost

        Private m_Key_Timeout As Integer = 10               ' Time to wait for keypress for key assignment in seconds
        Private manager_busy As Boolean = False             ' Prevent multiple concurrent calls on display manager
        Private timer_busy As Boolean = False               ' Prevents subscription updates when page change is due to occur
        Private LastKeyAssign As Integer = 0                ' The keycode received while assigning buttons
        Private m_Function_Key As Boolean = False           ' Indicates a Fn keypress, waiting to handle
        Private m_Use_Fn_Ind As Boolean = True              ' Show Fn on bottom right corner while Fn key is active
        Private m_military As Boolean = False               ' Convert time to military?

        Private display_data As New List(Of display_items_struct)              ' To track displayable items

        Private m_Enable_Subs As Boolean = False            ' Enable monitoring subscriptions
        Private m_Justification As String = "Left"          ' Left, Right, Center, Full
        Private m_Verbose As Boolean = False
        Private m_Initialized As Boolean = False            ' Flags if BackgroundLoad() has finished
        Private m_Ignore As Boolean = False                 ' Ignore user label if data doesn't fit cell
        Private m_Last_Item As Integer = 0                  ' Last item displayed
        Private m_Num_Pages As Integer = 0                  ' Current number of display pages
        Private m_Curr_Page As Integer = 0                  ' Current displayed page
        Private m_Last_Page As Integer = 0                  ' Last page that was displayed
        Private m_Page_Size As Integer = 4                  ' Calculated page size (number of cells on one page)
        Private DEFAULT_CELL_SIZE_COLS As Integer = 20
        Private DEFAULT_CELL_SIZE_ROWS As Integer = 1
        Private m_Cell_Size_Cols As Integer = DEFAULT_CELL_SIZE_COLS ' User specified column width of display cell
        Private m_Cell_Size_Rows As Integer = DEFAULT_CELL_SIZE_ROWS ' User specified row length of display cell
        Private m_Cell_Size As Integer = m_Cell_Size_Cols & m_Cell_Size_Rows ' Number of characters displayable
        Private m_Number_Cell_Cols As Integer = 20          ' How many cells across the display
        Private m_Number_Cell_Rows As Integer = 1           ' How many cells down the display

        Public Shared pAssignedKey As Integer = 0           ' The key that was pressed to be assigned to a command
        Public Shared pAssignedName As String = ""          ' The user chosen label for a key assignment
        Public Shared pKey As Integer = 0
        Public Shared pKeyName As String = ""

        Private m_GPO_Selected As Integer                   ' The GPO that was picked to be assigned a datasource

        Public m_Dialog_Header As String = "Header"
        Public m_Dialog_Message As String = "Message"
        Public m_Panel_Function As String = "Data"                    ' Could be "Data", "Keys", "GPOs"
        Public m_Mode As String = "none"

        Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, "OMLCD", "")

        Private m_refresh As Integer = 5                    ' Default time between display page changes in seconds
        Private WithEvents m_Refresh_tmr As New Timers.Timer(m_refresh * 1000)

        Private m_delay As Integer = 1000                   ' Default step rate for scrollng in cell in milliseconds
        Private WithEvents m_Delay_tmr As New Timers.Timer(m_delay)

        Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize

            theHost = host
            theHost.UIHandler.AddNotification(myNotification)

            AddHandler theHost.OnPowerChange, AddressOf m_Host_OnPowerChange

            SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub myNotification_clickAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        Private Sub myNotification_clearAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        Private Sub BackgroundLoad()

            ' This loop is only for testing.  To remove GPO settings values
            'Dim q As Integer
            'For q = 0 To 255
            'helperfunctions.StoredData.Delete(Me, Me.pluginName &".GPOs." & q.ToString)
            'Next

            m_Enable_Subs = True

            ' Removes old style settings
            helperFunctions.StoredData.Delete(Me, "VerboseDebug")
            helperFunctions.StoredData.Delete(Me, "Settings.Verbose")
            helperFunctions.StoredData.Delete(Me, "OMLCD.loadPanel")

            helperFunctions.StoredData.Delete(Me, "ComPort")
            helperFunctions.StoredData.Delete(Me, "BaudRate")
            helperFunctions.StoredData.Delete(Me, "CellSize")
            helperFunctions.StoredData.Delete(Me, "CycleRate")
            helperFunctions.StoredData.Delete(Me, "KeyPadBrightness")
            helperFunctions.StoredData.Delete(Me, "LCDBrightness")
            helperFunctions.StoredData.Delete(Me, "LCDContrast")
            helperFunctions.StoredData.Delete(Me, "Cell_Size_Cols")
            helperFunctions.StoredData.Delete(Me, "Cell_Size_Rows")
            helperFunctions.StoredData.Delete(Me, "UseSysTextColor")
            helperFunctions.StoredData.Delete(Me, "UserTextColor")
            helperFunctions.StoredData.Delete(Me, "SelectedAction")

            theHost.CommandHandler.AddCommand(New Command(Me, "OMLCD", "QUIT", "QuitOM", AddressOf CommandExecutor, 0, True, "Quit OpenMobile"))
            theHost.CommandHandler.AddCommand(New Command(Me, "OMLCD", "QUIT", "ReloadOM", AddressOf CommandExecutor, 0, True, "Reload OpenMobile"))

            ' We have hard coded to only try Matrix Orbital devices for now
            '  since I have no knowledge of other devices at this time
            LCD = New Matrix_Orbital(theHost)
            Drivers.Add(LCD.Name)
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Selecting {0}.", LCD.Name))
            End If

            ' Set or get saved communication parameters
            ' Not really needed right now since we auto-detect every time
            ' If we support multiple displays in the future, then we might need to
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.ComPort", LCD.Port)
            LCD.Port = helperFunctions.StoredData.Get(Me, "Settings.ComPort")
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.BaudRate", LCD.Baud)
            LCD.Baud = helperFunctions.StoredData.GetInt(Me, "Settings.BaudRate")
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.CellSize", LCD.CellSize)
            LCD.CellSize = helperFunctions.StoredData.GetInt(Me, "Settings.CellSize")

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.CycleRate", m_refresh)
            m_refresh = helperFunctions.StoredData.GetInt(Me, "Settings.CycleRate")

            ' Settings.DelayRate = 250 in the database, m_delay has been initialized to 500
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.DelayRate", m_delay)
            m_delay = helperFunctions.StoredData.GetInt(Me, "Settings.DelayRate")

            Dim xyz As Integer

            ' These settings cannot be applied to the LCD until it's connected ( see sub FindLCD() )
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.KeyPadBrightness", LCD.KeyPadBrightness)
            xyz = helperFunctions.StoredData.GetInt(Me, "Settings.KeyPadBrightness")
            LCD.KeyPadBrightness = xyz
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.KEY.Brightness", xyz)

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.LCDBrightness", LCD.LCDBrightness)
            LCD.LCDBrightness = helperFunctions.StoredData.GetInt(Me, "Settings.LCDBrightness")
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Brightness", LCD.LCDBrightness)

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.LCDContrast", LCD.LCDContrast)
            LCD.LCDContrast = helperFunctions.StoredData.GetInt(Me, "Settings.LCDContrast")
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Contrast", LCD.LCDContrast)

            m_Cell_Size_Cols = LCD.GetModuleCols
            m_Cell_Size_Rows = 1

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.Cell_Size_Cols", m_Cell_Size_Cols)
            m_Cell_Size_Cols = helperFunctions.StoredData.GetInt(Me, "Settings.Cell_Size_Cols")
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.Cell_Size_Rows", m_Cell_Size_Rows)
            m_Cell_Size_Rows = helperFunctions.StoredData.GetInt(Me, "Settings.Cell_Size_Rows")

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.UseSysTextColor", True)
            Dim tcolor As Color = BuiltInComponents.SystemSettings.SkinTextColor
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.UserTextColor", String.Format("{0},{1},{2}", _
                                                                                            tcolor.R.ToString("X2"), _
                                                                                            tcolor.G.ToString("X2"), _
                                                                                            tcolor.B.ToString("X2")))

            ' Miscellaneous settings
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.SelectedAction", 1)

            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.VerboseDebug", m_Verbose)
            m_Verbose = helperFunctions.StoredData.GetBool(Me, "Settings.VerboseDebug")

            FindLCD()

            '----------------------------------------------------------------------------------------

            manager = New ScreenManager(Me)

            Dim panel As New OMPanel(Me.pluginName())
            AddHandler panel.Entering, AddressOf panel_enter

            Dim x As Integer = theHost.ClientArea(0).Left + 50
            Dim y As Integer = theHost.ClientArea(0).Top
            Dim w As Integer = theHost.ClientArea(0).Right - x

            Dim text1 As New OMTextBox("_Text1", x + 400, y + 200, 300, 75)
            text1.Visible = False
            text1.FontSize = 24
            text1.OSKType = OSKInputTypes.Keypad
            panel.addControl(text1)

            Dim text1lbl As New OMLabel("_Text1Lbl", x + 75, y + 200, 300, 75)
            text1lbl.Text = "Enter a label:"
            text1lbl.Visible = False
            text1lbl.FontSize = 24
            panel.addControl(text1lbl)

            Dim text1btn = OMButton.PreConfigLayout_CleanStyle("_Text1Btn", x + 420, y + 350, 150, 80)
            text1btn.Text = "OK"
            text1btn.Visible = False
            text1btn.FontSize = 24
            AddHandler text1btn.OnClick, AddressOf text1btn_OnClick
            panel.addControl(text1btn)

            Dim msg1 As New OMLabel("_Msg_Label1", x + 30, y + 10, 850, 50)
            msg1.Visible = False
            msg1.Text = Me.pluginName
            msg1.FontSize = 24
            msg1.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter
            panel.addControl(msg1)

            Dim lbl1 As New OMLabel("_Main_Label1", x + 30, y + 10, 850, 50)
            lbl1.Visible = True
            lbl1.Text = Me.pluginName
            lbl1.FontSize = 24
            lbl1.TextAlignment = OpenMobile.Graphics.Alignment.CenterCenter
            panel.addControl(lbl1)

            'Dim btn1 = helperFunctions.Controls.DefaultControls.GetButton("_Main_Button1", x + 50, y + 60, 200, 60, "", "Display")
            Dim btn1 = OMButton.PreConfigLayout_CleanStyle("_Main_Button1", x + 50, y + 60, 200, 60)
            btn1.Text = "Display"
            btn1.Visible = True
            AddHandler btn1.OnClick, AddressOf btn1_OnClick
            panel.addControl(btn1)

            Dim lbl2 As New OMLabel("_Main_Label2", 300, y + 65, 400, 50)
            lbl2.Visible = True
            lbl2.Text = "Select items to display"
            lbl2.FontSize = 24
            panel.addControl(lbl2)

            Dim btn2 = OMButton.PreConfigLayout_CleanStyle("_Main_Button2", x + 50, y + 130, 200, 60)
            btn2.Text = "Set Key"
            btn2.Visible = True
            AddHandler btn2.OnClick, AddressOf btn2_OnClick
            panel.addControl(btn2)

            Dim lbl3 As New OMLabel("_Main_Label3", 300, y + 135, 400, 50)
            lbl3.Visible = True
            lbl3.Text = "Assign action to key"
            lbl3.FontSize = 24
            panel.addControl(lbl3)

            Dim btn3 = OMButton.PreConfigLayout_CleanStyle("_Main_Button3", x + 50, y + 200, 200, 60)
            btn3.Text = "Set GPO"
            btn3.Visible = True
            AddHandler btn3.OnClick, AddressOf btn3_OnClick
            panel.addControl(btn3)

            Dim lbl4 As New OMLabel("_Main_Label4", 300, y + 205, 400, 50)
            lbl4.Visible = True
            lbl4.Text = "Assign items to GPO"
            lbl4.FontSize = 24
            panel.addControl(lbl4)

            Dim slide1 As New OMSlider("_Main_Slider1", x + 50, y + 285, 850, 20, 20, 20)
            slide1.Visible = True
            slide1.Minimum = 0
            slide1.Maximum = 255
            slide1.Value = helperFunctions.StoredData.Get(Me, "Settings.LCDBrightness")
            slide1.SliderBar = Me.theHost.getSkinImage("Slider.Bar")
            slide1.Slider = Me.theHost.getSkinImage("Slider")
            AddHandler slide1.OnSliderMoved, AddressOf slide1_moved
            panel.addControl(slide1)

            Dim lbl5 As New OMLabel("_Main_Label5", x + 50, y + 295, 850, 50)
            lbl5.Visible = True
            lbl5.Text = "LCD Brightness"
            lbl5.FontSize = 16
            panel.addControl(lbl5)

            Dim slide2 As New OMSlider("_Main_Slider2", x + 50, y + 350, 850, 20, 20, 20)
            slide2.Visible = True
            slide2.Minimum = 0
            slide2.Maximum = 255
            slide2.Value = helperFunctions.StoredData.Get(Me, "Settings.LCDContrast")
            slide2.SliderBar = Me.theHost.getSkinImage("Slider.Bar")
            slide2.Slider = Me.theHost.getSkinImage("Slider")
            AddHandler slide2.OnSliderMoved, AddressOf slide2_moved
            panel.addControl(slide2)

            Dim lbl6 As New OMLabel("_Main_Label6", x + 50, y + 360, 850, 50)
            lbl6.Visible = True
            lbl6.Text = "LCD Contrast"
            lbl6.FontSize = 16
            panel.addControl(lbl6)

            Dim slide3 As New OMSlider("_Main_Slider3", x + 50, y + 415, 850, 20, 20, 20)
            slide3.Visible = True
            slide3.Minimum = 0
            slide3.Maximum = 255
            slide3.Value = helperFunctions.StoredData.Get(Me, "Settings.KeyPadBrightness")
            slide3.SliderBar = Me.theHost.getSkinImage("Slider.Bar")
            slide3.Slider = Me.theHost.getSkinImage("Slider")
            AddHandler slide3.OnSliderMoved, AddressOf slide3_moved
            panel.addControl(slide3)

            Dim lbl7 As New OMLabel("_Main_Label7", x + 50, y + 425, 850, 50)
            lbl7.Visible = True
            lbl7.Text = "Keypad Brightness"
            lbl7.FontSize = 16
            panel.addControl(lbl7)

            Dim theList As New OMList("_Sub_List1", theHost.ClientArea(0).Left, theHost.ClientArea(0).Top + 60, theHost.ClientArea(0).Width, theHost.ClientArea(0).Height - 150)
            theList.BackgroundColor = BuiltInComponents.SystemSettings.SkinTextColor
            theList.Scrollbars = True
            theList.ListStyle = eListStyle.MultiList
            theList.Background = Color.Silver
            theList.ItemColor1 = Color.Black
            theList.HighlightColor = Color.White
            theList.SoftEdgeData.Color1 = Color.Black
            theList.Font = New Font(Font.GenericSansSerif, 26)
            theList.TextAlignment = Alignment.CenterLeft
            theList.AutoFitTextMode = OpenMobile.Graphics.FitModes.FitSingleLine
            theList.SoftEdgeData.Sides = (helperFunctions.Graphics.FadingEdge.GraphicSides.Top Or helperFunctions.Graphics.FadingEdge.GraphicSides.Bottom)
            theList.UseSoftEdges = True
            theList.Visible = False

            AddHandler theList.OnClick, AddressOf thelist_OnClick
            AddHandler theList.OnHoldClick, AddressOf thelist_HoldClick
            panel.addControl(theList)

            Dim btnAssign = OMButton.PreConfigLayout_CleanStyle("_Sub_ButtonAssign", x + 250, y + 395, 200, 60)
            btnAssign.Text = "Assign"
            btnAssign.Visible = False
            AddHandler btnAssign.OnClick, AddressOf btnAssign_OnClick
            panel.addControl(btnAssign)

            Dim btnAdd = OMButton.PreConfigLayout_CleanStyle("_Sub_ButtonAdd", x + 250, y + 395, 200, 60)
            btnAdd.Text = "Add"
            btnAdd.Visible = False
            AddHandler btnAdd.OnClick, AddressOf btnAdd_OnClick
            panel.addControl(btnAdd)

            Dim btnUp = OMButton.PreConfigLayout_CleanStyle("_Sub_ButtonUp", x + 30, y + 395, 60, 60)
            btnUp.Text = "Up"
            btnUp.Visible = False
            AddHandler btnUp.OnClick, AddressOf btnUp_OnClick
            panel.addControl(btnUp)

            Dim btnDn = OMButton.PreConfigLayout_CleanStyle("_Sub_ButtonDn", x + 100, y + 395, 60, 60)
            btnDn.Text = "Dn"
            btnDn.Visible = False
            AddHandler btnDn.OnClick, AddressOf btnDn_OnClick
            panel.addControl(btnDn)

            Dim btnDone = OMButton.PreConfigLayout_CleanStyle("_Sub_ButtonDone", x + 500, y + 395, 200, 60)
            btnDone.Text = "Done"
            btnDone.Visible = False
            AddHandler btnDone.OnClick, AddressOf btnDone_OnClick
            panel.addControl(btnDone)

            Dim btnEdit = OMButton.PreConfigLayout_CleanStyle("_ButtonEdit", x + 750, y + 395, 150, 60)
            btnEdit.Text = "Edit"
            btnEdit.Visible = False
            AddHandler btnEdit.OnClick, AddressOf btnEdit_onclick
            panel.addControl(btnEdit)

            Dim btnInvert = OMButton.PreConfigLayout_CleanStyle("_ButtonInvert", x + 10, y + 395, 200, 60)
            btnInvert.Text = "NOT"
            btnInvert.Visible = False
            btnInvert.FontSize = 24
            AddHandler btnInvert.OnClick, AddressOf btnInvert_OnClick
            panel.addControl(btnInvert)

            manager.loadPanel(panel, True)

            ' Create the datasource object other plugins can subscribe to
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "Function", "State", DataSource.DataTypes.binary, "State of Fn key"), False)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "LCD", "Contrast", DataSource.DataTypes.raw, "LCD contrast value"), LCD.LCDContrast)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "LCD", "Brightness", DataSource.DataTypes.raw, "LCD Brightness"), LCD.LCDBrightness)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, Me.pluginName, "KEY", "Brightness", DataSource.DataTypes.raw, "KEY Brightness"), LCD.KeyPadBrightness)

            If Not theHost.DataHandler.SubscribeToDataSource("Location.Current.Daytime", AddressOf m_Subscriptions_Updated) Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "OMLCD - Could not subscribe to Location.Current.Daytime")
            End If

            m_Initialized = True

        End Sub

        Public Function CommandExecutor(ByVal command As Command, ByVal param() As Object, ByRef result As Boolean) As Object

            Dim cmdResult As Boolean = False

            cmdResult = False

            Select Case command.FullName
                Case "OMLCD.QUIT.QuitOM"
                    LCD.Close()
                    theHost.execute(eFunction.closeProgram)
                    cmdResult = True
                Case "OMLCD.QUIT.ReloadOM"
                    LCD.Close()
                    theHost.execute(eFunction.restartProgram)
                    cmdResult = True
            End Select

            Return result

        End Function

        Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel

            If manager Is Nothing Then
                Return Nothing
            End If

            Return manager(screen, name)

        End Function

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            ' Don't go in if there is no device found/connected

            If Not LCD.IsOpen Then
                Dim dialog As New helperFunctions.Forms.dialog(Me.pluginName, sender.Name)
                dialog.Header = Me.pluginName()
                dialog.Text = "Nothing to do. No LCD display detected."
                dialog.Icon = helperFunctions.Forms.icons.Information
                dialog.Button = helperFunctions.Forms.buttons.OK
                If (dialog.ShowMsgBox(screen) = helperFunctions.Forms.buttons.OK) Then
                    theHost.execute(eFunction.goBack, screen)
                End If
            End If

        End Sub

        Private Sub thelist_OnClick(ByVal sender As OMControl, ByVal screen As Integer)

            ' This event only comes into play for assigning an action/dataitem
            ' This is determined by m_Panel_Function

            ' A list item was clicked. We can enable the Assign button
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            Select Case m_Panel_Function
                Case "Keys"
                    Select Case m_Mode
                        Case "none"
                            myAssignButton.Visible = False
                            myAddButton.Visible = True
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                        Case "Add"
                            myAssignButton.Visible = True
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                        Case "Assign"
                            myAssignButton.Visible = False
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                    End Select
                Case "Data"
                    Select Case m_Mode
                        Case "none"
                            myAssignButton.Visible = False
                            myAddButton.Visible = True
                            myUpButton.Visible = True
                            myDnButton.Visible = True
                            myEditButton.Visible = True
                        Case "Add"
                            myAssignButton.Visible = True
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                        Case "Assign"
                            myAssignButton.Visible = False
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                    End Select
                Case "GPOs"
                    Select Case m_Mode
                        Case "none"
                            myAssignButton.Visible = False
                            myAddButton.Visible = True
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                        Case "Add"
                            myAssignButton.Visible = True
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                        Case "Assign"
                            myAssignButton.Visible = False
                            myAddButton.Visible = False
                            myUpButton.Visible = False
                            myDnButton.Visible = False
                            myEditButton.Visible = False
                    End Select
            End Select

        End Sub

        Private Sub thelist_HoldClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Prompt to delete

            ' Depends on what we are using this screen for
            ' If 'Add' button is visible, then delete is valid
            ' If 'Assign' button is visible, then delete is NOT valid

            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            If myAddButton.Visible = False Then
                ' Delete is not valid rightnow
                Exit Sub
            End If

            ' Fetch details about selected item
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim itemIndex As Integer
            Dim item As String = myList.SelectedItem.text

            Try
                ' Sometimes this returns: Object reference not set to an instance of an object.
                itemIndex = myList.SelectedIndex
            Catch ex As Exception
                itemIndex = -1
            End Try

            ' Nothing was selected
            If itemIndex = -1 Then
                Exit Sub
            End If

            ' Dialog to confirm delete
            Dim dialog As New helperFunctions.Forms.dialog(Me.pluginName, sender.Parent.Name)
            dialog.Header = m_Dialog_Header
            dialog.Text = m_Dialog_Message & " " & item
            dialog.Icon = helperFunctions.Forms.icons.Information
            dialog.Button = helperFunctions.Forms.buttons.No + helperFunctions.Forms.buttons.Yes

            Dim m_Settings As New OpenMobile.Data.PluginSettings

            If (dialog.ShowMsgBox(screen) = helperFunctions.Forms.buttons.Yes) Then

                Dim settingData As String
                Dim settingName As String
                Dim keys() As String
                Dim indexCode As String

                ' Split the information from the selected item
                settingData = item.Replace(" -> ", ";")
                keys = settingData.Split(New Char() {";"c})

                If m_Panel_Function = "GPOs" Then
                    ' Update the setting
                    indexCode = keys(0).Replace("GPO", "")
                    settingName = Me.pluginName & ".GPOs." & indexCode
                    settingData = "GPO" & indexCode & ";none"
                    helperFunctions.StoredData.Set(Me, settingName, settingData)
                    theHost.DataHandler.UnsubscribeFromDataSource(keys(1), AddressOf m_Subscriptions_Updated)
                    LCD.SetGPOState(CByte(indexCode + 1), False)
                End If

                If m_Panel_Function = "Data" Then
                    ' Delete the setting
                    settingName = Me.pluginName & ".Data." & itemIndex.ToString
                    helperFunctions.StoredData.Delete(Me, settingName)
                    theHost.DataHandler.UnsubscribeFromDataSource(keys(1), AddressOf m_Subscriptions_Updated)
                    ' rebuild the DATA settings, giving new index
                    reindex_data()
                    loadDisplayBlocks()
                    display_manager()
                End If

                If m_Panel_Function = "Keys" Then
                    ' Delete the setting
                    settingName = Me.pluginName & ".Keys." & itemIndex.ToString
                    helperFunctions.StoredData.Delete(Me, settingName)
                    ' rebuild the KEYS settings, giving new index
                    reindex_keys()
                End If

                ' Reload items into the list
                ReLoadList(sender, screen)

            End If

        End Sub

        Private Sub text1btn_OnClick(ByVal sender As OMControl, ByVal screen As Integer)

            ' Used to add a label to a displayed data item

            Dim itemText As String = ""
            Dim rText As String = "[Data]"
            Dim itemNum As Integer = 0
            Dim msetting As String = ""
            Dim settingText As String = ""
            Dim keys() As String
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim myText As OMTextBox = sender.Parent(screen, "_Text1")
            Dim myTextLbl As OMLabel = sender.Parent(screen, "_Text1Lbl")
            Dim myOKButton As OMButton = sender.Parent(screen, "_Text1Btn")
            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myDoneButton As OMButton = sender.Parent(screen, "_Sub_ButtonDone")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")

            ' If we are editing the label, then we have a selected index
            '  if this is new, then we don't

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Label selected: {0}", myText.Text))
            End If
            If m_Mode = "edit" Then
                itemNum = myList.SelectedIndex
                itemText = myList.SelectedItem.text
                ' Split the information from the selected item
                itemText = itemText.Replace(" -> ", ";")
                keys = itemText.Split(New Char() {";"c})
                rText = keys(0)
            End If

            ' Find the last item in the list (we just added it before getting here)
            'myList.Refresh()

            If m_Mode <> "edit" Then
                ' There was no previous index
                itemNum = myList.Count - 1
            End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("List index: {0}", itemNum.ToString))
            End If
            itemText = myList(itemNum).text
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("List item: {0}", itemText))
            End If

            If Not String.IsNullOrEmpty(myText.Text) Then

                ' Get rid of the old setting (did we save it already?)
                msetting = Me.pluginName & ".Data." & itemNum.ToString
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting key: {0}", msetting))
                End If

                ' Swap the "[Data]" text with the new label (entered into text box)
                itemText = itemText.Replace(rText, myText.Text)
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("New list item: {0}", itemText))
                End If
                settingText = itemText.Replace(" -> ", ";")
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("New setting value: {0}", settingText))
                End If

                ' Update the setting, update the list
                myList(itemNum).text = itemText
                helperFunctions.StoredData.Set(Me, msetting, settingText)

            End If

            ' Show the list, the add button, the done button
            myText.Visible = False
            myTextLbl.Visible = False
            myOKButton.Visible = False
            myAddButton.Visible = True
            myDoneButton.Visible = True
            myEditButton.Visible = True
            myUpButton.Visible = True
            myDnButton.Visible = True
            myInvertButton.Visible = False
            myList.Visible = True

            m_Mode = "none"

            ' Reload list again because now we have updated the label
            myList.Refresh()

            loadDisplayBlocks()
            display_manager()

        End Sub

        Private Sub btnUp_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Move the selected item up the list
            ' Valid for DATA type only

            If m_Panel_Function <> "Data" Then
                Exit Sub
            End If

            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim pickedIndex As Integer = 0
            Dim otherIndex As Integer = 0
            Dim itemText1 As String = ""
            Dim itemText2 As String = ""
            Dim settingText1 As String = ""
            Dim settingText2 As String = ""
            Dim settingName1 As String = ""
            Dim settingName2 As String = ""

            If myList.SelectedIndex = 0 Then
                ' no room to move up
                Exit Sub
            End If

            ' Basically take the current item, and swap it with one index lower
            '  unless there is no lower index
            ' But do this with the settings items

            pickedIndex = myList.SelectedIndex
            otherIndex = pickedIndex - 1

            itemText1 = myList(pickedIndex).text
            itemText2 = myList(otherIndex).text

            myList(pickedIndex).text = itemText2
            myList(otherIndex).text = itemText1

            myList.SelectedIndex = otherIndex

            settingText1 = itemText1.Replace(" -> ", ";")
            settingText2 = itemText2.Replace(" -> ", ";")

            settingName1 = Me.pluginName & ".Data." & pickedIndex.ToString
            settingName2 = Me.pluginName & ".Data." & otherIndex.ToString

            helperFunctions.StoredData.Set(Me, settingName1, settingText2)
            helperFunctions.StoredData.Set(Me, settingName2, settingText1)

            ' then reload list
            ReLoadList(sender, screen)
            ' refresh LCD
            loadDisplayBlocks()
            display_manager()

        End Sub

        Private Sub btnDn_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' move the selected item down the list

            If m_Panel_Function <> "Data" Then
                Exit Sub
            End If

            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim pickedIndex As Integer = 0
            Dim otherIndex As Integer = 0
            Dim itemText1 As String = ""
            Dim itemText2 As String = ""
            Dim settingText1 As String = ""
            Dim settingText2 As String = ""
            Dim settingName1 As String = ""
            Dim settingName2 As String = ""

            If myList.SelectedIndex = myList.Count - 1 Then
                ' no room to move up
                Exit Sub
            End If

            ' Basically take the current item, and swap it with one index lower
            '  unless there is no lower index
            ' But do this with the settings items

            pickedIndex = myList.SelectedIndex
            otherIndex = pickedIndex + 1

            itemText1 = myList(pickedIndex).text
            itemText2 = myList(otherIndex).text

            myList(pickedIndex).text = itemText2
            myList(otherIndex).text = itemText1

            myList.SelectedIndex = otherIndex
            settingText1 = itemText1.Replace(" -> ", ";")
            settingText2 = itemText2.Replace(" -> ", ";")

            settingName1 = Me.pluginName & ".Data." & pickedIndex.ToString
            settingName2 = Me.pluginName & ".Data." & otherIndex.ToString

            helperFunctions.StoredData.Set(Me, settingName1, settingText2)
            helperFunctions.StoredData.Set(Me, settingName2, settingText1)

            ' then reload list
            ReLoadList(sender, screen)
            ' refresh LCD
            loadDisplayBlocks()
            display_manager()

        End Sub

        Private Sub btnAdd_OnClick(ByVal sender As OMControl, ByVal screen As Integer)

            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim myListItem As New OMListItem("text")
            'Dim myItemFormat As New OMListItem.subItemFormat()

            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            m_Mode = "Add"

            myUpButton.Visible = False
            myDnButton.Visible = False
            myEditButton.Visible = False

            ' Repopulate the list with the appropriate items to be assigned to the described item
            ' Change the list label

            Select Case m_Panel_Function

                Case "Data"  ' Adding new display data item
                    ' Hide the "Add" and "Assign" button, user needs to click an action item (or Done)
                    myAssignButton.Visible = False
                    myAddButton.Visible = False
                    myInvertButton.Visible = False
                    ' Fill list with available sensor items
                    myList.Clear()
                    Dim sources As Generic.List(Of OpenMobile.Data.DataSource) = BuiltInComponents.Host.DataHandler.GetDataSources("")
                    'Dim sources = BuiltInComponents.Host.DataHandler.GetDataSources("")
                    Dim names As Generic.List(Of OMListItem) = New List(Of OMListItem)
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found {0} available sources", sources.Count.ToString))
                    End If
                    myListItem = New OMListItem("-BLANK-", "   Display blank cell")
                    'myListItem = New OMListItem("-BLANK-", "   Display blank cell", New OMListItem.subItemFormat.font = New Font(networkList.Font))
                    names.Add(myListItem)
                    myListItem = New OMListItem("-STATIC-", "   Display static text")
                    names.Add(myListItem)
                    For x = 0 To sources.Count - 1
                        myListItem = New OMListItem(sources(x).FullName, "   " & sources(x).Description)
                        names.Add(myListItem)
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Item {0}: {1} - {2}", myList.Count, myListItem.text, myListItem.subItem))
                        End If
                    Next
                    If sources.Count > 0 Then
                        names.Sort(2, names.Count() - 2, Nothing)
                    End If
                    For y = 0 To names.Count - 1
                        myList.Add(names(y))
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Source {0}: {1}", y, names(y)))
                        End If
                    Next
                    myList.Refresh()

                Case "Keys"  ' Adding new key assignment
                    ' Hide the "Add" and "Assign" button, user needs to click an action item (or Done)
                    myAssignButton.Visible = False
                    myAddButton.Visible = False
                    myInvertButton.Visible = False
                    ' Fill list with Actions
                    myList.Clear()
                    Dim myScreen = "Screen" & screen.ToString & "*"
                    Dim ActionNames As New Generic.List(Of OpenMobile.Command)
                    ActionNames = theHost.CommandHandler.GetCommands("")
                    myList.Add("Fn")
                    For x = 0 To ActionNames.Count - 1
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Available command: {0}", ActionNames(x).FullName))
                        End If
                        If ActionNames(x).FullName Like "Screen*" Then
                            ' This is a screen specific command
                            If ActionNames(x).FullName Like myScreen Then
                                ' We want this one (it's our screen)
                                myList.Add(ActionNames(x).FullName)
                                'myList.Add(ActionNames(x).Description)
                                If m_Verbose Then
                                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Adding command: {0}", ActionNames(x).FullName))
                                End If
                            End If
                        Else
                            myList.Add(ActionNames(x).FullName)
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Adding command: {0}", ActionNames(x).FullName))
                            End If
                        End If
                    Next
                    myList.Sort()
                    myList.Refresh()

                Case "GPOs"  ' Adding new GPO assignment
                    If myList.SelectedIndex >= 0 Then
                        ' Hide the "Add" and "Assign" button, user needs to click an action item (or Done)
                        myAssignButton.Visible = False
                        myAddButton.Visible = False
                        myInvertButton.Visible = False
                        ' Fill list with available BOOLEAN sensor items
                        'when you present your datasources you can filter them on datasource.datatype = datatype.binary
                        ' GPO numbers are not zero based
                        m_GPO_Selected = myList.SelectedIndex + 1
                        myList.Clear()
                        Dim sources = BuiltInComponents.Host.DataHandler.GetDataSources("")
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found {0} available sources", sources.Count.ToString))
                        End If
                        For x = 0 To sources.Count - 1
                            If sources(x).DataType = DataSource.DataTypes.binary Then
                                myList.Add(sources(x).FullName)
                                If m_Verbose Then
                                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Source {0}: {1}", x, sources(x)))
                                End If
                            End If
                        Next
                        myInvertButton.Visible = False
                        myList.Refresh()
                    End If
            End Select

        End Sub
        Private Sub btnEdit_onclick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Edit data label

            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")

            If myList.SelectedIndex < 0 Then
                ' No selected item
                Exit Sub
            End If

            Dim myText As OMTextBox = sender.Parent(screen, "_Text1")
            Dim myTextLbl As OMLabel = sender.Parent(screen, "_Text1Lbl")
            Dim myOKButton As OMButton = sender.Parent(screen, "_Text1Btn")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            Dim myDoneButton As OMButton = sender.Parent(screen, "_Sub_ButtonDone")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")

            Dim key As String
            Dim keys() As String
            Dim oldLabel As String = ""
            Dim itemIndex As Integer = 0

            myList.Visible = False
            myAssignButton.Visible = False
            myAddButton.Visible = False
            myDoneButton.Visible = False
            myEditButton.Visible = False
            myUpButton.Visible = False
            myDnButton.Visible = False
            myOKButton.Visible = True
            myText.Visible = True
            myTextLbl.Visible = True

            ' Grab the current label value, if any

            ' There is a list item selected
            ' Check if there is an assignment or if it is none
            key = myList.SelectedItem.text
            itemIndex = myList.SelectedIndex

            key = key.Replace(" -> ", ";")
            keys = key.Split(New Char() {";"c})
            oldLabel = keys(0)

            If oldLabel = "[Data]" Then
                myText.Text = ""
            Else
                myText.Text = keys(0)
            End If

            m_Mode = "edit"

        End Sub
        Private Sub btnAssign_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Select data item to add

            ' Get the original list back (since we use it to count with
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim myText As OMTextBox = sender.Parent(screen, "_Text1")
            Dim myTextLbl As OMLabel = sender.Parent(screen, "_Text1Lbl")
            Dim myOKButton As OMButton = sender.Parent(screen, "_Text1Btn")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myAddButton As OMButton = sender.Parent(screen, "_Sub_ButtonAdd")
            Dim myDoneButton As OMButton = sender.Parent(screen, "_Sub_ButtonDone")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            Dim itemCount As Integer = 0
            Dim pickedAction As String = ""
            Dim pickedIndex As Integer = 0
            Dim itemText As String = ""
            Dim settingText As String = ""
            Dim settingName As String = ""
            Dim keyName As String = ""
            Dim user_label As String = ""

            If myList.SelectedIndex = -1 Then
                Exit Sub
            End If

            m_Mode = "none"

            Select Case m_Panel_Function

                Case "Data"  ' Adding new display data item

                    ' We need to get back the existing list of settings
                    pickedAction = myList.SelectedItem.ToString
                    Me.ReLoadList(sender, screen)
                    itemCount = myList.Count

                    settingName = Me.pluginName & "." & m_Panel_Function & "." & itemCount.ToString

                    ' Need to get a user label for this item, default value for now
                    user_label = "[Data]"

                    settingText = user_label & ";" & pickedAction
                    itemText = settingText.Replace(";", " -> ")

                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Key: {0}  Value: {1}.", settingName, settingText))
                    End If
                    helperFunctions.StoredData.Set(Me, settingName, settingText)

                    ' Subscribe to the datasource
                    If pickedAction <> "-BLANK-" AndAlso pickedAction <> "-STATIC-" Then
                        theHost.DataHandler.SubscribeToDataSource(pickedAction, AddressOf m_Subscriptions_Updated)
                    End If

                    ' Change the panel
                    myList.Visible = False
                    myAssignButton.Visible = False
                    myAddButton.Visible = False
                    myDoneButton.Visible = False
                    myOKButton.Visible = True
                    myEditButton.Visible = False
                    myText.Visible = True
                    myText.Text = ""
                    myTextLbl.Visible = True

                    m_Mode = "Assign"

                    ' Reload list AGAIN because we just changed it
                    Me.ReLoadList(sender, screen)

                Case "Keys"  ' Adding new key assignment

                    ' Get the value of the last list item (capture the user selected Action)
                    pickedAction = myList.SelectedItem.ToString

                    ' We need to get back the existing list of settings
                    Me.ReLoadList(sender, screen)
                    itemCount = myList.Count

                    ' Show on LCD for testing purpose
                    ' LCD.PrintText(Chr(10) & "Action: " & pickedAction)
                    ' theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting key for action: {0}.", pickedAction))

                    ' Until I find a better way, we will wait
                    '  up to 10 seconds for user to pick a key, then cancel this action if they do not

                    Dim bannerText As String = "Press a button in "
                    Dim startTime As DateTime
                    Dim elapsedTime As TimeSpan
                    Dim lastSeconds As Integer = 0
                    startTime = Now

                    Do While True
                        ' We have to set the flag to indicate we expect to receive a keypress to assign to an action
                        '  This resets the properties KeyNameAssigned and KeyAssigned, causes a seperate event to be raised, 
                        '  and resets WaitKeyAssign after the event is raised
                        LCD.WaitKeyAssign = True

                        ' Here we would like to put up a message telling user to press a key
                        If m_Function_Key = True Then
                            ' Message to press another key (with time remaining)
                        Else
                            ' Message to press a key (with time remaining)
                        End If

                        elapsedTime = Now().Subtract(startTime)
                        If elapsedTime.Seconds > m_Key_Timeout Then
                            ' User did not pick a key button to assign within time limit
                            theHost.UIHandler.InfoBanner_Hide(screen)
                            myAssignButton.Visible = False
                            myAddButton.Visible = True
                            Me.ReLoadList(sender, screen)
                            Dim dialog As New helperFunctions.Forms.dialog(Me.pluginName, sender.Parent.Name)
                            dialog.Header = "Aborted."
                            dialog.Text = "No button press detected."
                            dialog.Width = 300
                            dialog.Icon = helperFunctions.Forms.icons.Information
                            dialog.Button = helperFunctions.Forms.buttons.OK
                            If (dialog.ShowMsgBox(screen) = helperFunctions.Forms.buttons.OK) Then
                                Exit Sub
                            End If
                        End If

                        ' KeyAssigned will be != 0 when a key was pressed while WaitKeyAssign flag was TRUE

                        If pAssignedKey <> 0 Then
                            ' User pressed a button
                            ' Check if this button is already assigned as Fn
                            ' If it is, set m_Function_Key to TRUE
                            ' then we will want to wait for another key
                            If UBound(Me.scan_settings("Keys", pAssignedName & ";Fn")) >= 0 Then
                                ' Pressed a key that is assigned function Fn
                                bannerText = "Fn Press another in: "
                                'uncomment next line to reset timer after first press
                                'startTime = Now
                                m_Function_Key = True
                                pAssignedKey = 0
                                pAssignedName = ""
                            Else
                                If m_Function_Key = True Then
                                    pAssignedName = "Fn+" & pAssignedName
                                    m_Function_Key = False
                                End If
                                theHost.UIHandler.InfoBanner_Hide(screen)
                                Exit Do
                            End If
                        End If

                        If (m_Key_Timeout - elapsedTime.Seconds) <> lastSeconds Then
                            lastSeconds = m_Key_Timeout - elapsedTime.Seconds
                            theHost.UIHandler.InfoBanner_Show(screen, New InfoBanner(InfoBanner.Styles.AnimatedBanner, bannerText & lastSeconds.ToString))
                        End If

                    Loop

                    ' Close the prompt message

                    ' Translate the key name into the first part of the item text
                    ' Translate the action name into the second part of the item text

                    ' Build the data value
                    settingText = pAssignedName & ";" & pickedAction

                    ' Some commands require parameters
                    ' Add in the required number of parameters
                    If pickedAction <> "Fn" Then
                        Dim ActionItem As OpenMobile.Command
                        ActionItem = theHost.CommandHandler.GetCommand(pickedAction)
                        If ActionItem.RequiresParameters Then
                            ' We need parameter(s) for this item
                            settingText = settingText & ";" & screen.ToString
                            For z = 2 To ActionItem.RequiredParamCount
                                settingText = settingText & ";0"
                            Next
                        End If
                    End If

                    ' On-screen value
                    itemText = pAssignedName & " -> " & pickedAction

                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Assigning {0} ({1}) to {2}", pAssignedName, pAssignedKey.ToString, pickedAction))
                    End If

                    ' Show on LCD to confirm keypress Testing only
                    ' LCD.PrintText(Chr(10) & "Key: " & pAssignedName & " (" & pAssignedKey.ToString & ")")

                    ' See if this key assignment is already in the settings DB
                    '  We don't want multiple of the same combinaion, but we can have
                    '   the same key do multiple actions (should we?)

                    If UBound(Me.scan_settings("Keys", pAssignedName & ";*")) >= 0 Then
                        ' There are already assignments for this button
                        If UBound(Me.scan_settings("Keys", settingText)) < 0 Then
                            ' This exact combo does not already exist
                            settingName = Me.pluginName & "." & m_Panel_Function & "." & itemCount.ToString
                            helperFunctions.StoredData.Set(Me, settingName, settingText)
                        End If
                    Else
                        ' No matches, so just add it
                        settingName = Me.pluginName & "." & m_Panel_Function & "." & itemCount.ToString
                        helperFunctions.StoredData.Set(Me, settingName, settingText)
                    End If

                    pAssignedKey = 0
                    pAssignedName = ""

                    ' Reload list AGAIN because we just changed it
                    Me.ReLoadList(sender, screen)
                    myAssignButton.Visible = False
                    myAddButton.Visible = True

                Case "GPOs"  ' Adding new GPO assignment

                    ' The datasource name that was selected
                    pickedAction = myList.SelectedItem.ToString

                    ' Setting that will be updated
                    settingName = Me.pluginName & ".GPOs." & m_GPO_Selected
                    settingText = "GPO" & m_GPO_Selected & ";" & pickedAction
                    itemText = "GPO" & m_GPO_Selected & " -> " & pickedAction

                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Key: {0}  Value: {1}.", settingName, settingText))
                    End If
                    helperFunctions.StoredData.Set(Me, settingName, settingText)

                    ' Subscribe to the datasource
                    theHost.DataHandler.SubscribeToDataSource(pickedAction, AddressOf m_Subscriptions_Updated)

                    ' Reload list AGAIN because we just changed it
                    Me.ReLoadList(sender, screen)

                    ' Set up the panel
                    myAssignButton.Visible = False
                    myAddButton.Visible = True
                    myList.Visible = True
                    myAssignButton.Visible = False
                    myDoneButton.Visible = True
                    myInvertButton.Visible = True

            End Select

        End Sub

        Private Sub btnInvert_OnClick(ByVal sender As OMControl, ByVal screen As Integer)

            ' To invert the logic for the datasource currently selected in the list
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim key As String
            Dim keys() As String
            Dim settingName As String = ""
            Dim settingText As String = ""
            Dim sourceName As String

            If myList.SelectedIndex >= 0 Then
                ' There is a list item selected
                ' Check if there is an assignment or if it is none
                key = myList.SelectedItem.text
                key = key.Replace(" -> ", ";")
                keys = key.Split(New Char() {";"c})
                If keys(1) <> "none" Then
                    ' We can toggle the inversion of this datasource item
                    settingName = Me.pluginName & ".GPOs." & Right(keys(0), 1)
                    If Right(keys(1), 4) = ".NOT" Then
                        ' We should add the .NOT
                        sourceName = keys(1).Replace(".NOT", "")
                        myList.SelectedItem.text = keys(0) & " -> " & sourceName
                        settingText = keys(0) & ";" & sourceName
                    Else
                        ' Strip the .NOT
                        sourceName = keys(1)
                        myList.SelectedItem.text = keys(0) & " -> " & sourceName & ".NOT"
                        settingText = keys(0) & ";" & sourceName & ".NOT"
                    End If
                    helperFunctions.StoredData.Set(Me, settingName, settingText)
                    'myList.Refresh()
                    m_Subscriptions()
                    Me.ReLoadList(sender, screen)
                End If
            End If

        End Sub

        Private Sub btnDone_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Done adding items

            Dim mainGroup As New ControlLayout(sender.Parent, "_Main_")
            Dim subGroup As New ControlLayout(sender.Parent, "_Sub_")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            mainGroup.Visible = True
            subGroup.Visible = False
            myInvertButton.Visible = False
            myUpButton.Visible = False
            myDnButton.Visible = False
            myEditButton.Visible = False

            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            myList.Clear()

            Dim lbl1 As OMLabel = sender.Parent(screen, "_Main_Label1")
            lbl1.Visible = True
            lbl1.Text = Me.pluginName()

            m_Mode = "none"

        End Sub

        Private Sub btn1_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Items to be displayed

            Dim mainGroup As New ControlLayout(sender.Parent, "_Main_")
            Dim subGroup As New ControlLayout(sender.Parent, "_Sub_")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            mainGroup.Visible = False
            subGroup.Visible = True
            myInvertButton.Visible = False
            myAssignButton.Visible = False
            myUpButton.Visible = True
            myDnButton.Visible = True
            myEditButton.Visible = True

            Dim lbl1 As OMLabel = sender.Parent(screen, "_Main_Label1")
            lbl1.Visible = True
            lbl1.Text = "Data Provider Items to Display"

            m_Dialog_Header = "Data Items"
            m_Dialog_Message = "Remove:"
            m_Panel_Function = "Data"
            m_Mode = "none"

            ' Add items from settings to the list control
            Me.ReLoadList(sender, screen)

        End Sub

        Private Sub btn2_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Button assignments

            Dim mainGroup As New ControlLayout(sender.Parent, "_Main_")
            Dim subGroup As New ControlLayout(sender.Parent, "_Sub_")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")

            mainGroup.Visible = False
            subGroup.Visible = True
            myInvertButton.Visible = False
            myAssignButton.Visible = False
            myUpButton.Visible = False
            myDnButton.Visible = False
            myEditButton.Visible = False

            Dim lbl1 As OMLabel = sender.Parent(screen, "_Main_Label1")

            lbl1.Visible = True
            lbl1.Text = "Button Key Function Assignments"

            m_Dialog_Header = "Key Assignments"
            m_Dialog_Message = "De-assign:"
            m_Panel_Function = "Keys"

            Me.ReLoadList(sender, screen)

        End Sub

        Private Sub btn3_OnClick(ByVal sender As OMControl, ByVal screen As Integer)
            ' Assign functions to GPOs

            Dim mainGroup As New ControlLayout(sender.Parent, "_Main_")
            Dim subGroup As New ControlLayout(sender.Parent, "_Sub_")
            Dim myInvertButton As OMButton = sender.Parent(screen, "_ButtonInvert")
            Dim myUpButton As OMButton = sender.Parent(screen, "_Sub_ButtonUp")
            Dim myDnButton As OMButton = sender.Parent(screen, "_Sub_ButtonDn")
            Dim myAssignButton As OMButton = sender.Parent(screen, "_Sub_ButtonAssign")
            Dim myEditButton As OMButton = sender.Parent(screen, "_ButtonEdit")
            Dim key As String = ""

            mainGroup.Visible = False
            subGroup.Visible = True
            myInvertButton.Visible = True
            myAssignButton.Visible = False
            myUpButton.Visible = False
            myDnButton.Visible = False
            myEditButton.Visible = False

            Dim lbl1 As OMLabel = sender.Parent(screen, "_Main_Label1")
            lbl1.Visible = True
            lbl1.Text = "GPO Function Assignments"

            m_Dialog_Header = "GPO Assignments"
            m_Dialog_Message = "De-assign:"
            m_Panel_Function = "GPOs"
            Dim settingName As String = ""
            Dim listValue As String = ""
            Dim x As Integer

            ' Since the device always has the same number of GPOs, we will populate the settings DB to make sure
            '  there is exactly the right number, with default = none for each un-used GPO

            Dim myArray() As String = Me.scan_settings(m_Panel_Function, pAssignedName & ".*")
            helperFunctions.StoredData.Delete(Me, Me.pluginName & ".GPOs.0")
            For x = 1 To LCD.GetModuleGPOs
                settingName = Me.pluginName & ".GPOs." & x.ToString
                key = helperFunctions.StoredData.Get(Me, settingName)
                If Not String.IsNullOrEmpty(key) Then
                    ' Setting exists already
                Else
                    ' Setting does NOT already exist
                    key = "GPO" & x.ToString & ";" & "none"
                    helperFunctions.StoredData.Set(Me, settingName, key)
                End If
            Next
            For x = LCD.GetModuleGPOs + 1 To 255
                settingName = Me.pluginName & ".GPOs." & x.ToString
                helperFunctions.StoredData.Delete(Me, settingName)
            Next

            ' Add items from settings to the list control
            Me.ReLoadList(sender, screen)

        End Sub

        Private Sub ReLoadList(ByVal sender As OMControl, ByVal screen As Integer)

            ' Load items from settings to the list control
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            Dim myList As OMList = sender.Parent(screen, "_Sub_List1")
            Dim keycode As String
            Dim action As String
            Dim msetting As String
            Dim key As String
            Dim keys() As String
            Dim mStart As Integer = 0
            Dim lastIndex As Integer = 0

            lastIndex = myList.SelectedIndex

            myList.Clear()

            If m_Panel_Function = "GPOs" Then
                ' Because GPOs start at 1, not 0
                mStart = 1
            End If

            'helperfunctions.StoredData.Delete(me, "")
            ' Max number of list items (only because of possible key codes)

            For x = mStart To 255

                msetting = Me.pluginName & "." & m_Panel_Function & "." & x.ToString
                key = helperFunctions.StoredData.Get(Me, msetting)

                If Not String.IsNullOrEmpty(key) Then
                    ' add to list
                    key.Replace(" -> ", ";")
                    keys = key.Split(New Char() {";"c})
                    If keys.Length > 1 Then
                        ' Valid setting value
                        keycode = keys(0)
                        action = keys(1)
                        myList.Add(keys(0) & " -> " & keys(1))
                    Else
                        ' This setting is in an invalid format
                        helperFunctions.StoredData.Delete(Me, msetting)
                    End If
                Else
                    Exit For
                End If

            Next

            myList.Refresh()
            If lastIndex > -1 Then
                myList.SelectedIndex = lastIndex
            End If

        End Sub

        Private Function reindex_keys()

            Dim key As String = ""
            Dim y As Integer = -1
            Dim results() As String = scan_settings("Keys", "*")

            For x = 0 To 255
                helperFunctions.StoredData.Delete(Me, Me.pluginName & ".Keys." & x.ToString)
            Next

            ' re-write the entries, newly indexed
            For x = 0 To UBound(results)
                helperFunctions.StoredData.Set(Me, Me.pluginName & ".Keys." & x.ToString, results(x))
            Next

            Return True

        End Function

        Private Function reindex_data()

            Dim key As String = ""
            Dim y As Integer = -1
            Dim results() As String = scan_settings("Data", "*")

            For x = 0 To 255
                helperFunctions.StoredData.Delete(Me, Me.pluginName & ".Data." & x.ToString)
            Next

            ' re-write the entries, newly indexed
            For x = 0 To UBound(results)
                helperFunctions.StoredData.Set(Me, Me.pluginName & ".Data." & x.ToString, results(x))
            Next

            Return True

        End Function

        Private Function scan_settings(ByVal settingName As String, ByVal pattern As String) As Array

            ' find matching settings in current .m_Panel_Function.x

            Dim key As String = ""
            Dim results() = New String() {}

            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Find settings matching '{0}'.", settingName))

            For x = 0 To 255
                key = helperFunctions.StoredData.Get(Me, Me.pluginName & "." & settingName & "." & x.ToString)
                If Not String.IsNullOrEmpty(key) Then
                    key.Replace(" -> ", ";")
                    If key Like pattern Then
                        ReDim Preserve results(results.Count)
                        results(UBound(results)) = key
                    End If
                Else
                    Exit For
                End If
            Next

            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Search complete.  Found {0}", results.Count))

            Return results

        End Function

        Private Sub slide1_moved(ByVal sender As OMSlider, ByVal screen As Integer)

            Dim sldr = CType(sender, OMSlider)
            LCD.LCDBrightness = sldr.Value
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Brightness", sldr.Value)
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            m_Settings.setSetting(Me, "Settings.LCDBrightness", sldr.Value)

        End Sub

        Private Sub slide2_moved(ByVal sender As OMControl, ByVal screen As Integer)

            Dim sldr = CType(sender, OMSlider)
            LCD.LCDContrast = sldr.Value
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Contrast", sldr.Value)
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            m_Settings.setSetting(Me, "Settings.LCDContrast", sldr.Value)

        End Sub

        Private Sub slide3_moved(ByVal sender As OMSlider, ByVal screen As Integer)

            Dim sldr = CType(sender, OMSlider)
            LCD.KeyPadBrightness = sldr.Value
            theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.KEY.Brightness", sldr.Value)
            Dim m_Settings As New OpenMobile.Data.PluginSettings
            m_Settings.setSetting(Me, "Settings.KeyPadBrightness", sldr.Value)

        End Sub

        Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

            System.Threading.Thread.Sleep(100)

            ' Create a handler for settings
            Dim mySettings As New Settings(Me.pluginName & " Settings")

            ' Load the main panel for OMLCD
            Dim loadPanelBtn = New Setting(SettingTypes.Button, "Settings.loadPanel", "", "Define I/O")
            loadPanelBtn.Value = ""
            mySettings.Add(loadPanelBtn)

            mySettings.Add(New Setting(SettingTypes.MultiChoice, "Settings.VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose))

            ' Create a setting for showing Fn indicator
            Dim settingShowFnInd = New Setting(SettingTypes.MultiChoice, "Settings.ShowFnInd", String.Empty, "Show Fn indicator on LCD?", Setting.BooleanList, Setting.BooleanList)
            m_Use_Fn_Ind = helperFunctions.StoredData.GetBool(Me, "Settings.ShowFnInd") ' Read from DB
            settingShowFnInd.Value = m_Use_Fn_Ind
            mySettings.Add(settingShowFnInd)

            ' Text Entry of RGB values
            Dim settingUserTextColor = New Setting(SettingTypes.Text, "Settings.UserTextColor", "User color", "Hex R,G,B")
            settingUserTextColor.Value = helperFunctions.StoredData.Get(Me, "Settings.UserTextColor") ' Read from DB
            mySettings.Add(settingUserTextColor)

            ' Create a setting for using system text color
            Dim settingUseSysTextColor = New Setting(SettingTypes.MultiChoice, "Settings.UseSysTextColor", String.Empty, "Use System Text Color?", Setting.BooleanList, Setting.BooleanList)
            settingUseSysTextColor.Value = helperFunctions.StoredData.GetBool(Me, "Settings.UseSysTextColor") ' Read from DB
            mySettings.Add(settingUseSysTextColor)

            ' Create a setting for page cycle rate
            Dim settingCycleRate = New Setting(SettingTypes.Text, "Settings.CycleRate", "Page Cycle", "Seconds between page changes")
            settingCycleRate.Value = m_refresh.ToString
            mySettings.Add(settingCycleRate)

            ' Create a setting for page cycle rate
            Dim settingDelayRate = New Setting(SettingTypes.Text, "Settings.DelayRate", "Scroll Delay", "Milliseconds to scroll")
            settingDelayRate.Value = m_delay.ToString
            mySettings.Add(settingDelayRate)

            Dim cOptions As List(Of String) = New List(Of String)
            Select Case LCD.GetModuleCols
                Case 8
                    cOptions.Add("8")
                Case 16
                    cOptions.Add("8")
                    cOptions.Add("16")
                Case 20
                    cOptions.Add("10")
                    cOptions.Add("20")
                Case 40
                    cOptions.Add("10")
                    cOptions.Add("20")
                    cOptions.Add("40")
                Case Else
                    cOptions.Add("0")
            End Select
            Dim settingUserCellCols = New Setting(SettingTypes.MultiChoice, "Settings.Cell_Size_Cols", "Cell Columns", "Display cell width", cOptions, cOptions, helperFunctions.StoredData.GetInt(Me, "Settings.Cell_Size_Cols").ToString)
            mySettings.Add(settingUserCellCols)

            Dim cOptions2 As List(Of String) = New List(Of String)
            Select Case LCD.GetModuleRows
                Case 1
                    cOptions2.Add("1")
                Case 2
                    cOptions2.Add("1")
                    cOptions2.Add("2")
                Case 4
                    cOptions2.Add("1")
                    cOptions2.Add("2")
                    cOptions2.Add("4")
                Case Else
                    cOptions2.Add("0")
            End Select
            Dim settingUserCellRows = New Setting(SettingTypes.MultiChoice, "Settings.Cell_Size_Rows", "Cell Rows", "Display cell height", cOptions2, cOptions2, helperFunctions.StoredData.GetInt(Me, "Settings.Cell_Size_Rows").ToString)
            mySettings.Add(settingUserCellRows)

            ' Connect event to get updates when setting change
            AddHandler mySettings.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            Return mySettings

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)
            ' Setting was changed, update database

            Select Case settings.Name
                Case "Settings.ShowFnInd"
                    m_Use_Fn_Ind = CBool(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_Use_Fn_Ind)
                Case "Settings.loadPanel"
                    theHost.CommandHandler.ExecuteCommand("Screen0.Panel.Goto.Default." & Me.pluginName)
                Case "Settings.CycleRate"
                    ' Rate to cycle through display pages
                    m_refresh = CInt(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_refresh)
                    If m_refresh < 2 Then
                        ' anything less than 2 seconds might bog down the system
                        m_refresh = 2
                        helperFunctions.StoredData.Set(Me, settings.Name, m_refresh)
                    End If
                    m_Refresh_tmr.Stop()
                    m_Refresh_tmr.Interval = m_refresh * 1000
                    m_Refresh_tmr.Start()
                Case "Settings.DelayRate"
                    ' Step rate to scroll data cell
                    m_delay = CInt(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_delay)
                    If m_delay < 500 Then
                        ' anything less than 500ms might bog down the system and is too fast for our setup anyway
                        m_delay = 500
                        helperFunctions.StoredData.Set(Me, settings.Name, m_delay)
                    End If
                    m_Delay_tmr.Stop()
                    m_Delay_tmr.Interval = m_delay
                    m_Delay_tmr.Start()
                Case "Settings.Cell_Size_Cols"
                    ' User picked width of display cell
                    m_Cell_Size_Cols = CInt(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_Cell_Size_Cols)
                    reCalc()
                Case "Settings.Cell_Size_Rows"
                    ' User picked height of display cell
                    m_Cell_Size_Rows = CInt(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_Cell_Size_Rows)
                    reCalc()
                Case "Settings.UseSysTextColor"
                    ' Grab system color and save it user specified color
                    helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)
                    Dim tcolor As Color = BuiltInComponents.SystemSettings.SkinTextColor
                    If settings.Value = False Then
                        'helperfunctions.StoredData.Set(Me, Me.pluginName &".UserTextColor", String.Format("{0},{1},{2}", _
                        '                                                                                                        tcolor.R.ToString("X2"), _
                        '                                                                                                        tcolor.G.ToString("X2"), _
                        '                                                                                                       tcolor.B.ToString("X2")))
                        tcolor = helperFunctions.StoredData.GetColor(Me, "Settings.UserTextColor", Color.White)
                    End If
                    LCD.SetBackgroundColor(tcolor.R, tcolor.G, tcolor.B)
                Case "Settings.UserTextColor"
                    helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)
                    Dim tcolor As Color = helperFunctions.StoredData.GetColor(Me, "Settings.UserTextColor", Color.White)
                    LCD.SetBackgroundColor(tcolor.R, tcolor.G, tcolor.B)
                Case "Settings.VerboseDebug"
                    m_Verbose = CBool(settings.Value)
                    helperFunctions.StoredData.Set(Me, settings.Name, m_Verbose)
            End Select

        End Sub

        Private Sub FindLCD()

            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "Requesting driver to connect.")
            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Current setting PORT={0} BAUD={1}", LCD.Port, LCD.Baud))

            OpenMobile.Threading.SafeThread.Asynchronous(AddressOf connect)

        End Sub

        Private Sub connect()

            myNotification.Text = String.Format("Finding/connecting to module...")
            LCD.Connect(Me, m_Verbose)

        End Sub

        Private Sub m_Delay_tmr_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_Delay_tmr.Elapsed
            ' Handles the scrolling of data within a cell

            Dim needsRefresh As Boolean = False
            Dim slabel As String = ""
            Dim sValue As String = ""

            If timer_busy = False Then

                timer_busy = True

                ' See if it is a DATA subscription
                For x = 0 To display_data.Count - 1
                    If display_data(x).Value = "-STATIC-" Then
                        ' label is data, data is label (don't ask)
                        sValue = display_data(x).Label
                        slabel = ""
                    Else
                        If display_data(x).Label = "[Data]" Then
                            slabel = ""
                        Else
                            slabel = display_data(x).Label
                        End If
                        sValue = display_data(x).Value
                    End If
                    If m_delay <> 0 AndAlso Len(slabel & sValue) > m_Cell_Size Then
                        ' The cell data value is greater than available space
                        If display_data(x).OnScreen = True Then
                            ' This item is flagged as being on the display right now
                            display_data(x).IsNew = True
                            needsRefresh = True
                        End If
                    End If
                Next

                If needsRefresh Then
                    ' Call display manager so it can be updated immediately
                    display_manager()
                End If

                timer_busy = False

            End If

        End Sub

        Private Sub m_Refresh_tmr_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_Refresh_tmr.Elapsed
            ' Handles page changing

            If timer_busy = False Then

                ' Time to change the display page
                timer_busy = True

                ' This is used by display_manager to detect a changed page
                m_Last_Page = m_Curr_Page

                ' Increment page to be displayed
                If m_Curr_Page >= m_Num_Pages Then
                    m_Curr_Page = 1
                Else
                    m_Curr_Page += 1
                End If

                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Change to page {0}", m_Curr_Page))
                End If
                display_manager()

                timer_busy = False

            End If

        End Sub

        Private Sub NoComms() Handles LCD.CommLost
            '  Called when the LCD device disappears (unplugged?)

            m_Refresh_tmr.Stop()
            m_Refresh_tmr.Enabled = False
            m_Delay_tmr.Stop()
            m_Delay_tmr.Enabled = False

            myNotification.Text = "LCD disconnected."

        End Sub

        Private Sub NoDevice() Handles LCD.NoDevice

            ' Reset the flag
            helperFunctions.StoredData.Set(Me, "Settings.ComPort", LCD.Default_Port)
            helperFunctions.StoredData.Set(Me, "Settings.BaudRate", LCD.Default_Baud)
            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("No LCD device found."))
            myNotification.Text = "No devices were found."

        End Sub

        Private Sub display_manager()
            ' Manages/Updates the attached display
            ' forced_display indicates to force the re-display of current page
            '  with any items marked as new

            If Not LCD.IsOpen Then
                Exit Sub
            End If

            If display_data.Count = 0 Then
                Exit Sub
            End If

            ' Make sure we have a page to display
            If m_Num_Pages = 0 Then
                ' No data to display
                Exit Sub
            End If

            ' We have already been called, so give up
            If manager_busy Then
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Concurrent call"))
                End If
                Exit Sub
            End If

            ' Prevent concurrent calls
            manager_busy = True

            Dim cell_pos, theIndex, x, y As Integer
            Dim cellData As String
            Dim tempData As String = ""
            Dim keys(0) As String
            Dim iStart As Integer = 0
            Dim sLabel As String = ""
            Dim sValue As String = ""
            Dim sSource As String = ""

            m_Cell_Size = (m_Cell_Size_Cols * m_Cell_Size_Rows)

            ' Shouldn't happen, but just in case
            If m_Curr_Page > m_Num_Pages Then
                m_Curr_Page = 1
            End If

            ' Reset flag that items are on screen
            For x = 0 To display_data.Count - 1
                display_data(x).OnScreen = False
            Next

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Displaying Page {0}/{1}", m_Curr_Page, m_Num_Pages))
            End If

            ' Loop thru the number of cells that fit on a page
            For cell_pos = 1 To m_Page_Size

                ' determines which data items are to be displayed
                ' cell_pos = which cell on the page will be dealt with
                ' v = the data item index to display
                theIndex = (m_Curr_Page - 1) * m_Page_Size + cell_pos - 1

                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Page {0}, Cell {1} (index: {2}/{3})", m_Curr_Page, cell_pos, theIndex, display_data.Count.ToString))
                End If

                If theIndex > display_data.Count - 1 Then
                    ' We don't have enough data items
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Not enough data items. Index {0}, Page {1}", theIndex, m_Curr_Page))
                    End If
                    Exit For
                End If

                ' We only want to update the cell where data has changed
                ' because refreshing whole display can cause flicker.
                ' We only do this if the affected cell is currently on the display
                ' Otherwise, we're doing a whole new page so it doesn't matter.

                If display_data(theIndex).IsNew = True Or m_Last_Page <> m_Curr_Page Then

                    ' This item has been updated

                    ' Calculate physical cursor col,row position to start the cell
                    x = Int((cell_pos - 1) / m_Number_Cell_Rows) + 1
                    y = Int(cell_pos / x)
                    x = ((x - 1) * m_Cell_Size_Cols) + 1
                    y = ((y - 1) * m_Cell_Size_Rows) + 1

                    ' Grab the cell information

                    ' Determine what the label is.  If "Data" then use no label
                    '  but why don't we just use a blank label?

                    If display_data(theIndex).Label = "[Data]" Then
                        sLabel = ""
                    Else
                        sLabel = display_data(theIndex).Label
                    End If
                    sValue = display_data(theIndex).Value
                    sSource = display_data(theIndex).Source

                    If sValue Like "*example*" Then
                        sValue = sValue
                    End If

                    If m_delay <> 0 AndAlso Len(sLabel) + Len(sValue) > m_Cell_Size Then

                        ' We have a scroll speed value, and data is greater than cell size
                        ' so user allows scrolling and scrolling is necessary

                        ' Calculate the new starting position of the value that will be displayed
                        iStart = display_data(theIndex).lastChar + 1
                        If iStart > Len(sValue) - (m_Cell_Size - 1) Then
                            iStart = 1
                        End If
                        ' Save the new character position for next time
                        display_data(theIndex).lastChar = iStart

                        ' Saves the scrolled value for display (should not get truncated later as we clip it here already)
                        ' NOTE: we do not scroll the label portion
                        cellData = Left(sLabel & Mid(sValue, iStart), m_Cell_Size)

                    Else

                        ' No scrolling, just save as is (will be truncated later if necessary)
                        cellData = sLabel & sValue

                    End If

                    ' Now we format the data for the cell according to user specified alignment
                    ' Find out where the data will fall within the cell size
                    '  and determine where any line breaks (extra spaces) should be

                    ' NOTE: We could reserve a special character to force a break.  Maybe the tilde ~

                    cellData = textWrap(cellData, m_Cell_Size_Cols)
                    cellData = Left(cellData & Space(m_Cell_Size), m_Cell_Size)

                    ' Work out cell alignment

                    ' Output the info to the display
                    ' Because cells can be multi-row, we need to loop thru and reposition cursor
                    '  at a new row within the cell

                    For a = y To y + m_Cell_Size_Rows - 1
                        ' Loops thru available rows for the cell
                        LCD.SetCursorPosition(a, x)
                        'output the formatted line for the cell
                        tempData = Mid(cellData, (a - y) * m_Cell_Size_Cols + 1, m_Cell_Size_Cols)
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("LCD.SetCursorPosition({0},{1}): '{2}'", a, x, tempData))
                        End If
                        LCD.PrintText(tempData)
                    Next

                    ' Reset the d_new since it has now been displayed
                    display_data(theIndex).IsNew = False

                End If

                ' This item is on the display
                display_data(theIndex).OnScreen = True

            Next

            If m_Use_Fn_Ind = True Then
                If m_Function_Key = True Then
                    ' Function key is active
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Fn flag is set"))
                    End If
                    ' Overwrite the bottom corner with the special Fn character
                    ' Set cursor to end of display
                    LCD.SetCursorPosition(LCD.GetModuleRows, LCD.GetModuleCols)
                    ' Select the custom character memory bank we save the indicator in
                    LCD.SwitchBank(1)
                    ' Print the character
                    LCD.PrintText(Chr(1))
                End If
            End If

            ' allow other calls to resume
            manager_busy = False

            ' This is for working with inline datasource formatting
            'dsource = display_data(v).d_label & ": {" & display_data(v).d_source & "}"
            'cellData = theHost.DataHandler.GetInLineDataSource(cellData)

        End Sub

        Private Function textWrap(ByVal source As String, ByVal width As Integer) As String

            Dim text As String = ""
            Dim splitChars() As String = {" ", ".", ",", ":", ";"}
            Dim currChar As String = ""
            Dim last_splittable As Integer = 0
            Dim y As Integer = 0
            Dim z As Integer = 0

            For x = 1 To Len(source)

                currChar = Mid(source, x, 1)

                If Not String.IsNullOrEmpty(Array.Find(splitChars, Function(m) m = currChar)) Then
                    ' This is a splittable point
                    ' If the split character would be the first character of a new
                    '  line, then we really don't need to split
                    If x <= width Then
                        last_splittable = x
                    End If
                End If

                If Len(text & currChar) > width Then ' this needs refinement as it only works for the first line
                    ' This character puts us over the column width
                    '  but if it is the 1st character of a new line
                    '  we can remove it and carry on
                    If x = width + 1 Then
                        ' Why dont we just ignore this character
                        If currChar <> "" Then
                            text = text & currChar
                        End If
                        width += width
                    Else
                        ' Go back to last splittable character
                        text = Left(text, last_splittable)
                        ' Bump back the pointer
                        width += last_splittable
                        x = last_splittable + 1
                        ' Scan ahead to the next non-space character
                        For w = x To Len(source)
                            If Mid(source, w, 1) <> " " Then
                                Exit For
                            End If
                        Next
                    End If
                Else
                    text = text & currChar
                End If

            Next

            Return text

        End Function

        Private Sub m_Subscriptions()

            ' Sets datasource subscriptions based on settings DB entries
            ' Called on connected() event to get the subscriptions running

            Dim key As String = ""
            Dim keys() As String
            Dim settingData As String = ""
            Dim settingName As String = ""

            ' temporarily stop the interupt timer so we
            m_Refresh_tmr.Stop()
            m_Delay_tmr.Stop()

            theHost.DataHandler.UnsubscribeFromDataSource(Me.pluginName & ";" & Me.pluginName & ".Function.State", AddressOf m_Subscriptions_Updated)
            If theHost.DataHandler.SubscribeToDataSource(Me.pluginName & ";" & Me.pluginName & ".Function.State", AddressOf m_Subscriptions_Updated) Then
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Subscribe to: " & Me.pluginName & ";" & Me.pluginName & ".Function.State"))
                End If
            Else
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Issue subscribing to: " & Me.pluginName & ";" & Me.pluginName & ".Function.State"))
                End If
            End If

            ' Set up Data display subscriptions
            For x = 0 To display_data.Count - 1
                If Not String.IsNullOrEmpty(display_data(x).Source) Then
                    If display_data(x).Source <> "-BLANK-" Then
                        Try
                            theHost.DataHandler.UnsubscribeFromDataSource(display_data(x).Source, AddressOf m_Subscriptions_Updated)
                            theHost.DataHandler.SubscribeToDataSource(display_data(x).Source, AddressOf m_Subscriptions_Updated)
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Subscribe to: {0}", display_data(x).Source))
                            End If
                        Catch ex As Exception
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Datasource no longer available: {0}", display_data(x).Source))
                            End If
                        End Try
                    End If
                End If
            Next

            ' Set up GPO subscriptions
            For x = 0 To LCD.GetModuleGPOs - 1
                settingName = Me.pluginName & ".GPOs." & x.ToString
                settingData = helperFunctions.StoredData.Get(Me, settingName)
                ' Strip the .NOT if the user has specified inverted logic for this subscription
                settingData = settingData.Replace(".NOT", "")
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting {0} = {1}", settingName, settingData))
                End If
                If Not String.IsNullOrEmpty(settingData) Then
                    keys = settingData.Split(New Char() {";"})
                    ' Avoid trying to subscribe if un-assigned GPO
                    If keys(1) <> "none" Then
                        If m_Verbose Then
                            theHost.DataHandler.UnsubscribeFromDataSource(keys(1), AddressOf m_Subscriptions_Updated)
                        End If
                        If theHost.DataHandler.SubscribeToDataSource(keys(1), AddressOf m_Subscriptions_Updated) Then
                            If m_Verbose Then
                                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Subscribe to: {0}", keys(1)))
                            End If
                        End If
                    End If
                End If
            Next

        End Sub

        Private Sub m_Subscriptions_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            ' Going to be used to handle datasource subscription update
            '  Used to update the data items for display
            '  Or set GPOs
            Dim w As Integer
            Dim x As Integer
            Dim key As String = ""
            Dim keys() As String
            Dim settingData As String = ""
            Dim settingName As String = ""
            Dim Inverted As Boolean = False

            If Not m_Enable_Subs Then
                If m_Verbose Then
                    'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Subscription updates ignored"))
                End If
                Exit Sub
            End If

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Update from {0}: {1}", sensor.FullName, sensor.FormatedValue))
            End If

            If sensor.FullName = "Location.Current.Daytime" Then
                ' When we make an option to auto-dim, we can test the value here
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Sensor {0}: {1}", sensor.FullName, sensor.FormatedValue))
                End If
                Dim m_Settings As New OpenMobile.Data.PluginSettings
                If sensor.Value = True Then
                    ' Set the display/keypad brightness to full
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "Setting brightness levels to FULL")
                    End If
                    LCD.LCDBrightness = 230
                    theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Brightness", LCD.LCDBrightness)
                    LCD.KeyPadBrightness = 230
                    theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.KEY.Brightness", LCD.KeyPadBrightness)
                Else
                    ' Set the display/keypad brightness to option values
                    If Integer.TryParse(m_Settings.getSetting(Me, "Settings.LCDBrightness"), x) Then
                        LCD.LCDBrightness = x
                    Else
                        LCD.LCDBrightness = 200
                    End If
                    theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.LCD.Brightness", LCD.LCDBrightness)
                    If Integer.TryParse(m_Settings.getSetting(Me, "Settings.KeyPadBrightness"), x) Then
                        LCD.KeyPadBrightness = x
                    Else
                        LCD.KeyPadBrightness = 200
                    End If
                    theHost.DataHandler.PushDataSourceValue("OMLCD;OMLCD.KEY.Brightness", LCD.KeyPadBrightness)
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting brightness levels to {0}", LCD.LCDBrightness.ToString))
                    End If
                End If
            End If

            ' See if it is the Fn key subscription
            If sensor.FullNameWithProvider = Me.pluginName & ";" & Me.pluginName & ".Function.State" Then
                ' The function key has changed state
                m_Function_Key = sensor.Value
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Function Key: {0} ({1})", sensor.Value, sensor.FormatedValue))
                End If
            End If

            ' See if it is a GPO subscription
            For w = 1 To LCD.GetModuleGPOs
                settingName = Me.pluginName & ".GPOs." & w.ToString
                settingData = helperFunctions.StoredData.Get(Me, settingName)
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Setting {0} value {1}", settingName, settingData))
                End If
                If Not String.IsNullOrEmpty(settingData) Then
                    keys = settingData.Split(New Char() {";"})
                    If Right(keys(1), 4) = ".NOT" Then
                        Inverted = True
                    Else
                        Inverted = False
                    End If
                    keys(1) = keys(1).Replace(".NOT", "")
                    If keys(1) = sensor.FullName Then
                        ' There is a GPO assigned with this datasource
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Match {0} <-> {1}", keys(1), sensor.FullName))
                        End If
                        ' Check if user specified INVERTED Logic for this sensor
                        If Inverted Then
                            LCD.SetGPOState(w, Not sensor.Value)
                        Else
                            LCD.SetGPOState(w, sensor.Value)
                        End If
                    Else
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("No match {0} <-> {1}", keys(1), sensor.FullName))
                        End If
                    End If
                End If
            Next

            ' See if it is a DATA subscription
            For x = 0 To display_data.Count - 1
                If display_data(x).Source = sensor.FullName Then
                    display_data(x).Value = sensor.FormatedValue
                    If display_data(x).Source Like "*Time*" Then
                        display_data(x).Value = TimeReformat(display_data(x).Value)
                    End If
                    display_data(x).IsNew = True
                    display_data(x).lastChar = 0
                    If display_data(x).OnScreen = True Then
                        ' This item is flagged as being on the display right now
                        ' Call display manager so it can be updated immediately
                        If timer_busy = False Then
                            display_manager()
                        End If
                    End If
                End If
            Next

        End Sub

        Private Sub thehost_OnSystemEvent(ByVal efunc As eFunction, args() As Object) Handles theHost.OnSystemEvent

            If efunc.Equals(eFunction.pluginLoadingComplete) Then
                ' Subscribe so we detect when day/night happens
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "Subscribing to Location.Current.Daytime")
                End If
            End If

        End Sub

        Private Function m_Function_Status() As Boolean

            Return m_Function_Key

        End Function

        Private Sub reCalc()
            ' Recalculates cell and page sizes

            Try
                m_Number_Cell_Cols = LCD.GetModuleCols / m_Cell_Size_Cols
            Catch ex As Exception
                m_Cell_Size_Cols = DEFAULT_CELL_SIZE_COLS
                m_Number_Cell_Cols = LCD.GetModuleCols / m_Cell_Size_Cols
            End Try
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Number of cell columns: {0} of {1}.", m_Cell_Size_Cols, LCD.GetModuleCols))
            End If
            Try
                m_Number_Cell_Rows = LCD.GetModuleRows / m_Cell_Size_Rows
            Catch ex As Exception
                m_Cell_Size_Rows = DEFAULT_CELL_SIZE_ROWS
                m_Number_Cell_Rows = LCD.GetModuleRows / m_Cell_Size_Rows
            End Try
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Number of cell rows: {0} / {1}.", m_Cell_Size_Rows, LCD.GetModuleRows))
            End If
            m_Page_Size = m_Number_Cell_Cols * m_Number_Cell_Rows
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Cells per page: {0} column(s) by {1} row(s).", m_Number_Cell_Cols, m_Number_Cell_Rows))
            End If
        End Sub

        Private Sub loadDisplayBlocks()

            ' Builds/Rebuilds the display_data collection with settings stuff
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Build/Rebuild display blocks...."))
            End If

            ' Fetch display data items from settings and build the display block collection

            display_data.Clear()

            Dim keys() As String
            Dim key As String
            Dim data As String = ""
            Dim label As String = ""
            Dim source As String = ""
            Dim counter As Integer = 0

            m_Cell_Size = m_Cell_Size_Cols & m_Cell_Size_Rows

            For x = 0 To 255
                key = helperFunctions.StoredData.Get(Me, Me.pluginName & ".Data." & x.ToString)
                ' Note since this is the first build we mark all items as being new values
                If Not String.IsNullOrEmpty(key) Then
                    key.Replace(" -> ", ";")
                    keys = key.Split(New Char() {";"c})
                    ' Keys(0) = Label
                    ' Keys(1) = Data Item Name
                    If keys(1) = "-BLANK-" Then
                        ' User wants a blank line here
                        source = "-BLANK-"
                        data = Space(m_Cell_Size)
                        label = ""
                    ElseIf keys(1) = "-STATIC-" Then
                        ' User wants static text here
                        label = ""
                        source = "-STATIC-"
                        If keys(0) <> "[Data]" Then
                            ' The label is used as the static text
                            data = keys(0)
                        Else
                            ' No static text specified
                            data = Space(m_Cell_Size)
                        End If
                    Else
                        ' Standard data item display
                        If m_Verbose Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("keys(0): {0}  keys(1): {1}", keys(0), keys(1)))
                        End If
                        Try
                            data = BuiltInComponents.Host.DataHandler.GetDataSource(keys(1)).FormatedValue
                        Catch
                            data = "n/a"
                        End Try
                        label = keys(0)
                        source = keys(1)
                        If source Like "*Time*" Then
                            ' the datasource probably contains a time value
                            data = TimeReformat(data)
                        End If
                    End If
                    ' Here, if user options selected specific time format
                    '  and this is a time datasource, we can reformat here
                    display_data.Add(New display_items_struct(source, label, True, data, False, 0))
                Else
                    ' No more data to fill the current page
                    ' Fill with spaces
                    Do While ((x) Mod m_Page_Size) <> 0
                        display_data.Add(New display_items_struct(Space(m_Cell_Size_Cols & m_Cell_Size_Rows), "", True, "", False, 0))
                        x += 1
                    Loop
                    Exit For
                End If
            Next

            m_Num_Pages = Int(display_data.Count / m_Page_Size)
            If (display_data.Count / m_Page_Size) > Int(display_data.Count / m_Page_Size) Then
                m_Num_Pages += 1
            End If
            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Items {0} Page Size {1} = {2} Pages.", display_data.Count, m_Page_Size, m_Num_Pages))
            End If

        End Sub
        Private Function TimeReformat(ByVal source As String) As String
            ' Reformat time value to save space

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Time value: {0}", source))
            End If

            If source.Length > m_Cell_Size Then
                ' Remove AM/PM indicator + it's leading space
                source = source.Replace(" AM", "A")
                source = source.Replace(" PM", "P")
            End If

            If m_military Then
                ' Do nothing for now (we have to find the time value)
            End If

            Return source

        End Function

        Private Sub Connected() Handles LCD.Connected

            ' Reset the flag

            Dim m_Settings As New OpenMobile.Data.PluginSettings
            LCD.LCDBrightness = helperFunctions.StoredData.Get(Me, "Settings.LCDBrightness")
            LCD.LCDContrast = helperFunctions.StoredData.Get(Me, "Settings.LCDContrast")
            LCD.KeyPadBrightness = helperFunctions.StoredData.Get(Me, "Settings.KeyPadBrightness")

            myNotification.Text = String.Format("Connected to {0} on {1}", LCD.GetModuleType, LCD.Port)

            helperFunctions.StoredData.Set(Me, "Settings.ComPort", LCD.Port)
            helperFunctions.StoredData.Set(Me, "Settings.BaudRate", LCD.Baud)

            ' Determine what our default cell size will be so they will get saved to settings DB
            '  if this is first run and user has not set any
            If LCD.GetModuleCols < 20 Then
                m_Cell_Size_Cols = 8
            Else
                m_Cell_Size_Cols = 10
            End If
            If LCD.GetModuleRows > 2 Then
                m_Cell_Size_Rows = 2
            Else
                m_Cell_Size_Rows = 1
            End If

            ' Make sure there are the required number of GPO entries
            For x = 1 To CByte(LCD.GetModuleGPOs)
                helperFunctions.StoredData.SetDefaultValue(Me, Me.pluginName & ".GPOs." & x.ToString, "GPO" & x.ToString & ";none")
            Next

            ' Get the info required to manage the display.  Sets defaults if this is first run
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.Cell_Size_Cols", m_Cell_Size_Cols)
            m_Cell_Size_Cols = CInt(helperFunctions.StoredData.Get(Me, "Settings.Cell_Size_Cols"))
            helperFunctions.StoredData.SetDefaultValue(Me, "Settings.Cell_Size_Rows", m_Cell_Size_Rows)
            m_Cell_Size_Rows = CInt(helperFunctions.StoredData.Get(Me, "Settings.Cell_Size_Rows"))

            ' Set some variables for this session

            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found {0} on {1}", LCD.GetModuleType, LCD.Port))
            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Display type: {0}", LCD.GetModuleStyle))
            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("{0} Rows by {1} Cols", LCD.GetModuleRows, LCD.GetModuleCols))
            reCalc()

            Dim tcolor As Color = BuiltInComponents.SystemSettings.SkinTextColor
            If helperFunctions.StoredData.Get(Me, "Settings.UseSysTextColor") = False Then
                tcolor = helperFunctions.StoredData.GetColor(Me, "Settings.UserTextColor", Color.White)
            End If
            LCD.SetBackgroundColor(tcolor.R, tcolor.G, tcolor.B)

            LCD.AutoScroll = False
            LCD.LineWrap = False

            ' Custom Character(s)
            ' Define the special character for Fn indicator
            Dim CustomChar1() As Byte = {254, 193, 1, 1, 31, 16, 16, 28, 16, 23, 21, 21}
            LCD.SendCommands(CustomChar1)

            ' This is an optional section to show connection to the display
            If True Then
                If m_Verbose Then
                    theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Sending 'Connected' message"))
                End If
                Dim txt1, txt2 As String
                txt1 = "Open Mobile"
                txt2 = "Connected..."
                ' Load/Reload the startup screen definition
                LCD.RebuildStartupScreen()
                LCD.Home()
                LCD.ClearScreen()
                LCD.SwitchBank(1)
                LCD.PrintText("Connected.")
            End If

            ' Do the initial load of display blocks
            loadDisplayBlocks()

            ' Start subscriptions for each data block
            m_Subscriptions()

            ' First display
            'display_manager()

            ' Start display refresh timer
            m_Refresh_tmr.Enabled = True
            m_Refresh_tmr.Start()
            m_Delay_tmr.Enabled = True
            m_Delay_tmr.Start()

        End Sub

        Private Sub KeyPressReceived(ByVal key As Integer) Handles LCD.KeyPressReceived

            ' Reset idle timer
            ' Here, somehow, some way

            Dim assignment(1) As String    ' Holds the split values of the setting
            Dim settingParms() As Object   ' Holds any required parameters of the assigned command

            ' Fetch the string name of the pressed key
            pKeyName = [Enum].GetName(GetType(LCDKey), key)

            'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format(">>> KeyPressReceived: {0} ({1})", pKeyName, key))

            ' Fetch any/all values assigned to this key
            Dim result() As String = Me.scan_settings("Keys", pKeyName & ";*")

            If m_Verbose Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Found {0} matching key assignments.", result.Count.ToString))
            End If

            ' Search assignments to see if the key was assigned as Fn
            If UBound(result) >= 0 Then
                For x = 0 To UBound(result)
                    assignment = result(x).Split(New Char() {";"c})
                    If assignment(1) = "Fn" Then
                        ' Fn key pressed
                        If m_Function_Key = True Then
                            ' We already have Fn key active, toggle
                            m_Function_Key = False
                            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";" & Me.pluginName & ".Function.State", False)
                        Else
                            ' Set the Fn key
                            m_Function_Key = True
                            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";" & Me.pluginName & ".Function.State", True)
                        End If
                        ' Update the LCD display (in case we're showing the indicator on screen
                        display_manager()
                        ' Get out of here.  There is nothing else to do if this was a FN press
                        Exit Sub
                    End If
                Next
            End If

            ' Key was not defined as Fn
            If m_Function_Key = True Then
                ' Fn key is active, so add it to the current key name
                pKeyName = "Fn+" & pKeyName
            End If

            'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("Full key name: {0}", pKeyName))

            ' Search again as now we may have an active Fn key press
            Dim resultFn() As String = Me.scan_settings("Keys", pKeyName & ";*")

            If UBound(resultFn) >= 0 Then
                For x = 0 To UBound(resultFn)
                    assignment = resultFn(x).Split(New Char() {";"c})
                    Dim ActionItem As OpenMobile.Command
                    ActionItem = theHost.CommandHandler.GetCommand(assignment(1))
                    ' Does the command require parameters?
                    If ActionItem.RequiresParameters Then
                        If ActionItem.RequiredParamCount > 0 Then
                            ReDim settingParms(ActionItem.RequiredParamCount - 1)
                            For z = 2 To 2 + ActionItem.RequiredParamCount - 1
                                settingParms(z - 2) = assignment(z)
                            Next
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format(">>> Sending command: {0}", assignment(1)))
                            theHost.CommandHandler.ExecuteCommand(assignment(1), settingParms)
                        End If
                    Else
                        ' No parameters required
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format(">>> Sending command: {0}", assignment(1)))
                        theHost.CommandHandler.ExecuteCommand(assignment(1))
                    End If
                    ' After processing the key press, we can reset the Fn key flag
                    m_Function_Key = False
                    theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";" & Me.pluginName & ".Function.State", False)
                Next
            End If

        End Sub

        Private Sub KeyAssignReceived(ByVal key As Integer, ByVal name As String) Handles LCD.KeyAssignReceived

            ' Nothing to do here right now
            pAssignedKey = key
            pAssignedName = name

        End Sub

        Public ReadOnly Property pluginDescription() As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
            Get
                Return "Display stuff on serialy connected LCD"
            End Get
        End Property

        Public ReadOnly Property pluginName() As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
            Get
                Return "OMLCD"
            End Get
        End Property

        Public ReadOnly Property pluginVersion() As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
            Get
                Return 0.1
            End Get
        End Property

        Public ReadOnly Property displayName() As String Implements OpenMobile.Plugin.IHighLevel.displayName
            Get
                Return "OMLCD"
            End Get
        End Property

        Public ReadOnly Property authorEmail() As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
            Get
                Return "jmullan99@gmail.com"
            End Get
        End Property

        Public ReadOnly Property authorName() As String Implements OpenMobile.Plugin.IBasePlugin.authorName
            Get
                Return "John Mullan"
            End Get
        End Property

        Public ReadOnly Property pluginIcon() As imageItem Implements OpenMobile.Plugin.IBasePlugin.pluginIcon
            Get
                Return OM.Host.getPluginImage(Me, "Icon-LCD")
            End Get
        End Property

        Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage
            Return False
        End Function

        Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage
            Return False
        End Function

        Private disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                Me.disposedValue = True
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "I am being disposed.")
                If disposing Then
                    ' TODO: free other state (managed objects).
                    ' If the data source tracker is not defined, then define it
                    ' Do we need to stop subscriptions?  Probably not necessary
                    Try
                        If LCD.IsOpen Then
                            end_LCD()
                        End If
                    Catch ex As Exception
                        If (theHost IsNot Nothing) Then
                            theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "Issue closing device.")
                        End If
                    End Try
                End If
                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
        End Sub

        Private Sub end_LCD()
            ' Terminate all LCD operations

            ' Stop processing subscriptions
            m_Enable_Subs = False

            'theHost.DataHandler.UnsubscribeFromDataSource("*", AddressOf m_Subscriptions_Updated)

            m_Refresh_tmr.Enabled = False
            m_Refresh_tmr.Stop()
            m_Delay_tmr.Enabled = False
            m_Delay_tmr.Stop()

            If LCD.IsOpen Then
                theHost.DebugMsg(OpenMobile.DebugMessageType.Info, "System Event wants to close me.")
                LCD.Close()
            End If

        End Sub

        Private Sub m_Host_OnSystemEvent(ByVal type As OpenMobile.eFunction, args As Object) Handles theHost.OnSystemEvent

            Select Case type
                Case Is = eFunction.shutdown
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMLCD - m_Host_OnSystemEvent()"), "System Event: shutdown")
                    End If
                    end_LCD()
                Case Is = eFunction.closeProgram
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMLCD - m_Host_OnSystemEvent()"), "System Event: closeProgram")
                    End If
                    end_LCD()
                Case Is = eFunction.restart
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMLCD - m_Host_OnSystemEvent()"), "System Event: restart")
                    End If
                    end_LCD()
                Case Is = eFunction.restartProgram
                    If m_Verbose Then
                        theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format("OMLCD - m_Host_OnSystemEvent()"), "System Event: restartProgram")
                    End If
                    end_LCD()
            End Select

        End Sub

        Private powerEventReceived = False
        Private powerResumeReceived = False

        Private Sub m_Host_OnPowerChange(ByVal type As OpenMobile.ePowerEvent) Handles theHost.OnPowerChange
            ' It is probably safer to just trash the current device connection and
            '  start again on resume

            Select Case type
                Case Is = ePowerEvent.SystemResumed
                    powerEventReceived = False
                    If powerResumeReceived Then
                        ' Avoid multiple redundant calls
                        Exit Sub
                    End If
                    powerResumeReceived = True
                    'theHost.DebugMsg(OpenMobile.DebugMessageType.Info, String.Format(Me.pluginName & " resuming from sleep/hibernate."))
                    FindLCD()
                Case Is = ePowerEvent.SleepOrHibernatePending
                    powerResumeReceived = False
                    If powerEventReceived Then
                        ' Avoid multiple redundant calls
                        Exit Sub
                    End If
                    powerEventReceived = True
                    end_LCD()
                Case Is = ePowerEvent.ShutdownPending
                    powerEventReceived = False
                    powerResumeReceived = False
                    end_LCD()
            End Select

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

End Namespace
