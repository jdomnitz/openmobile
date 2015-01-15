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

Namespace OMFuel
    ' Fuel Prices Skin

    Public NotInheritable Class OMFuel
        Inherits HighLevelCode

        Private WithEvents theHost As IPluginHost
        Private m_Verbose As Boolean = False
        Private bannerStyle As Integer = 0
        Private locationText As String = "None"
        Private gps_fixed As Boolean = False
        Private currLat As Single = 0
        Private currLng As Single = 0
        Private message As String = ""
        Private temp_count As Integer = 0
        Private updated As Boolean = False
        Private initialized As Boolean = False
        Private GradeFilter As String = ""

        Dim myBanner As InfoBanner = New InfoBanner(bannerStyle, "Please wait...", 5000)

        Public Sub New()

            MyBase.New("OMFuel", OM.Host.getSkinImage("Icons|Icon-Gas"), 0.1, "Display local gas prices", "Fuel Prices", "John Mullan", "jmullan99@gmail.com")

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

            If String.IsNullOrEmpty(StoredData.Get(Me, "favoriteCount")) Then
                StoredData.Set(Me, "favoriteCount", 0)
            End If

            ' Build the main panel
            Dim omFuelpanel As New OMPanel("FuelPrices", "Fuel Prices")
            AddHandler omFuelpanel.Entering, AddressOf panel_enter
            AddHandler omFuelpanel.Leaving, AddressOf panel_leave

            Dim mainContainer = New OMContainer("mainContainer", theHost.ClientArea(0).Left, theHost.ClientArea(0).Top, theHost.ClientArea(0).Width, theHost.ClientArea(0).Height)
            mainContainer.Image = OM.Host.getSkinImage("MediaBorder")
            mainContainer.ScrollBar_ColorNormal = Color.Transparent
            mainContainer.BackgroundColor = Color.Transparent
            mainContainer.Opacity = 0
            omFuelpanel.addControl(mainContainer)

            Dim PopUpMenuStrip = New ButtonStrip(Me.pluginName, "FuelPrices", "PopUpMenuStrip")
            'PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Current", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Current", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            'PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Home", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Home"), "Home", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            'PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Favorites", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), "Favorites/Search" & vbCrLf & "Hold to add shown", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            AddHandler PopUpMenuStrip.OnShowing, AddressOf PopUpMenuStrip_OnShowing
            omFuelpanel.PopUpMenu = PopUpMenuStrip

            'PanelManager.loadPanel(omFuelpanel, True)

            ' Build the panel to manually enter a favorite
            Dim fuelFavPanel As New OMPanel("EditFavs", "Fuel Prices")

            Dim shpPriceBackground As New OMBasicShape("editFavBackground", theHost.ClientArea(0).Left + 150, theHost.ClientArea(0).Top + 100, theHost.ClientArea(0).Width - 300, 290, New ShapeData(shapes.RoundedRectangle, Color.FromArgb(235, Color.Black), Color.Transparent, 0, 5))
            fuelFavPanel.addControl(shpPriceBackground)

            Dim zipLabel As New OMLabel("zipLabel", shpPriceBackground.Left + 15, shpPriceBackground.Top + 10, 150, 50)
            zipLabel.Text = "Zip/Postal:"
            zipLabel.TextAlignment = Alignment.CenterRight
            fuelFavPanel.addControl(zipLabel)

            Dim favZip As New OMTextBox("favZip", zipLabel.Left + zipLabel.Width + 20, shpPriceBackground.Top + 10, 400, 50)
            favZip.OSKType = 1
            fuelFavPanel.addControl(favZip)

            Dim infoLabel As New OMLabel("infoLabel", zipLabel.Left + 20, zipLabel.Top + zipLabel.Height + 10, 650, 50)
            infoLabel.Text = "OR"
            infoLabel.TextAlignment = Alignment.CenterCenter
            fuelFavPanel.addControl(infoLabel)

            Dim cityLabel As New OMLabel("cityLabel", zipLabel.Left, infoLabel.Top + infoLabel.Height + 10, 150, 50)
            cityLabel.Text = "City, State:"
            cityLabel.TextAlignment = Alignment.CenterRight
            fuelFavPanel.addControl(cityLabel)

            Dim favCity As New OMTextBox("favCity", cityLabel.Left + cityLabel.Width + 20, cityLabel.Top, 400, 50)
            favCity.OSKType = 1
            fuelFavPanel.addControl(favCity)

            'Dim stateLabel As New OMLabel("cityLabel", cityLabel.Left, cityLabel.Top + cityLabel.Height + 10, 150, 50)
            'stateLabel.Text = "State:"
            'stateLabel.TextAlignment = Alignment.CenterRight
            'fuelFavPanel.addControl(stateLabel)

            'Dim favState As New OMTextBox("favState", stateLabel.Left + stateLabel.Width + 20, stateLabel.Top, 400, 50)
            'favState.OSKType = 1
            'fuelFavPanel.addControl(favState)

            Dim okButton As OMButton = OMButton.PreConfigLayout_CleanStyle("okButton", shpPriceBackground.Left + (shpPriceBackground.Width / 2) - 110, shpPriceBackground.Top + shpPriceBackground.Height - 50, 100, 40, OpenMobile.Graphics.GraphicCorners.All)
            okButton.Text = "Ok"
            AddHandler okButton.OnClick, AddressOf okButton_OnClick
            fuelFavPanel.addControl(okButton)

            Dim cancelButton As OMButton = OMButton.PreConfigLayout_CleanStyle("cancelButton", shpPriceBackground.Left + (shpPriceBackground.Width / 2) + 10, okButton.Top, 100, 40, OpenMobile.Graphics.GraphicCorners.All)
            cancelButton.Text = "Cancel"
            AddHandler cancelButton.OnClick, AddressOf cancelButton_OnClick
            fuelFavPanel.addControl(cancelButton)

            PanelManager.loadPanel(omFuelpanel, True)
            PanelManager.loadPanel(fuelFavPanel, False)

            ' Subscribe to datasources
            theHost.DataHandler.SubscribeToDataSource("Fuel.Price.List", AddressOf Subscription_Updated)
            theHost.DataHandler.SubscribeToDataSource("Fuel.Messages.Text", AddressOf Subscription_Updated)
            theHost.DataHandler.SubscribeToDataSource("Location.Current", AddressOf Subscription_Updated)

            'theHost.DebugMsg("OMFuel - BackgroundLoad()", "Complete. Requesting data refresh...")

            Dim sysHome As OpenMobile.Location
            sysHome = BuiltInComponents.SystemSettings.Location_Home

            StoredData.SetDefaultValue(Me, "FAVS.homeZip", sysHome.Zip)
            StoredData.SetDefaultValue(Me, "FAVS.homeCity", sysHome.City)
            StoredData.SetDefaultValue(Me, "FAVS.homeState", sysHome.State)
            StoredData.SetDefaultValue(Me, "FAVS.homeCountry", sysHome.Country)

            Dim xZip As String = "", xCountry As String = "", xCity As String = "", xState As String = "", mFilter As String = ""
            ' For testing only
            'xZip = StoredData.Delete(Me, "FAVS.homeZip")
            'xCity = StoredData.Delete(Me, "FAVS.homeCity")
            'xState = StoredData.Delete(Me, "FAVS.homeState")
            'xCountry = StoredData.Delete(Me, "FAVS.homeCountry")

            ' Fetch home favorite
            xZip = StoredData.Get(Me, "FAVS.homeZip")
            xCity = StoredData.Get(Me, "FAVS.homeCity")
            xState = StoredData.Get(Me, "FAVS.homeState")
            xCountry = StoredData.Get(Me, "FAVS.homeCountry")

            ' If other systems are not ready, we don't get the initial load of data
            '  so we'll just wait a couple of seconds. We "could" block panel_enter if
            '  background loading is not complete.
            System.Threading.Thread.Sleep(2000)

            ' Find if there was a last searched filter
            Try
                mFilter = theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Filter").FormatedValue
                ' Request prices from last searched
                theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Last")
            Catch
                ' Nothing found, request our saved HOME
                ' (assuming it was not blank)
                If Not String.IsNullOrEmpty(xZip) Then
                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Home", {xZip, xCity, xState, xCountry})
                End If
            End Try

            initialized = True

        End Sub

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            'theHost.DebugMsg("OMFuel - Subscription_Updated()", sensor.FullName)

            'If Not initialized Then
            'theHost.DebugMsg("OMFuel - Subscription_Updated()", "We're not ready yet.")
            'Exit Sub
            'End If

            Dim mPanel As OMPanel

            Select Case sensor.FullName

                Case "Fuel.Messages.Text"
                    ' Messages from OMDSFuelPrices
                    'theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Messages.Text", "")
                    For x = 0 To theHost.ScreenCount - 1
                        mPanel = PanelManager(x, "FuelPrices")
                        If Not mPanel Is Nothing Then
                            If mPanel.IsVisible(x) Then
                                If Not String.IsNullOrEmpty(sensor.FormatedValue) Then
                                    message = sensor.FormatedValue
                                    theHost.UIHandler.InfoBanner_Hide(x)
                                    myBanner.Text = message
                                    theHost.UIHandler.InfoBanner_Show(x, myBanner)
                                Else
                                    message = ""
                                End If
                            End If
                        End If
                    Next

                Case "Fuel.Price.List"
                    'updated = True
                    For x = 0 To theHost.ScreenCount - 1
                        If PanelManager.IsPanelLoaded(x, "FuelPrices") Then
                            mPanel = PanelManager(x, "FuelPrices")
                            panel_enter(mPanel, x)
                        End If
                    Next
                    'updated = False

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

            theHost.UIHandler.InfoBanner_Hide(screen)

        End Sub

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            'theHost.DebugMsg("OMFuel - panel_enter()", "")

            Dim client As WebClient = New WebClient
            Dim PopUpMenu As ButtonStrip = sender.PopUpMenu

            'Dim y As Integer
            Dim xCount As Integer = StoredData.Get(Me, "favoriteCount")
            Dim xCity As String, xState As String

            For x = PopUpMenu.Buttons.Count - 1 To 0 Step -1
                PopUpMenu.Buttons.RemoveAt(x)
            Next

            PopUpMenu.Buttons.Add(Button.CreateMenuItem("Current", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Current Location", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            PopUpMenu.Buttons.Add(Button.CreateMenuItem("Home", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Home"), "Home", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            PopUpMenu.Buttons.Add(Button.CreateMenuItem("Favorites", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), "Favorites/Search" & vbCrLf & "Hold to add shown", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))

            ' Remove previous favorites in the popup list
            'y = PopUpMenu.Buttons.Count - 1
            'For x = y To 3 Step -1
            'PopUpMenu.Buttons.RemoveAt(x)
            'Next

            ' Add the current favorites in the popup list
            For x = 1 To xCount
                xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x))
                xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x))
                PopUpMenu.Buttons.Add(Button.CreateMenuItem(xCity, theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-labels"), String.Format("*{0},{1}", xCity, xState), False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            Next

            'Dim message As String = ""
            'message = theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Messages.Text").FormatedValue

            ' Set up the main container for prices
            Dim theContainer As OMContainer = sender(screen, "mainContainer")
            theContainer.ClearControls()

            If m_Verbose Then
                theHost.DebugMsg("OMFuel - panel_enter()", "Creating dictionaries.")
            End If

            Dim Grades As Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, String))) = TryCast(theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Price.List").Value, Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, String))))
            Dim Results As New Dictionary(Of Integer, Dictionary(Of String, String))
            Dim GasPrices As New Dictionary(Of String, String)

            If m_Verbose Then
                theHost.DebugMsg("OMFuel - panel_enter()", "Filling container.")
            End If

            Dim x_coord As Integer = 10
            Dim y_coord As Integer = 1

            Dim data As imageItem
            'Dim xx As Integer
            Dim grade As String = ""
            Dim price As String = ""
            Dim name As String = ""
            Dim address As String = ""
            Dim area As String = ""
            Dim state As String = ""
            Dim country As String = ""
            Dim timeStamp As String = ""
            Dim xdecimal As String = ""
            Dim ind As Integer = 0

            If Not Grades Is Nothing Then

                For Each gradeKey As String In Grades.Keys

                    If String.IsNullOrEmpty(GradeFilter) Then
                        GradeFilter = gradeKey
                    End If

                    ' Adds found fuel grades to the popup menu list
                    PopUpMenu.Buttons.Add(Button.CreateMenuItem(gradeKey, theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Gas"), gradeKey, False, AddressOf grades_onClick, Nothing, Nothing))

                    If gradeKey <> GradeFilter Then
                        Continue For
                    End If

                    Results = Grades(GradeFilter)

                    For Each result As KeyValuePair(Of Integer, Dictionary(Of String, String)) In Results

                        ind = result.Key
                        GasPrices = Results(ind)

                        ' name could hold a URL to a logo
                        name = GasPrices("Name")
                        Try
                            data = New imageItem(OpenMobile.Net.Network.imageFromURL(name))
                        Catch
                            data = imageItem.NONE
                        End Try

                        price = GasPrices("Price")
                        address = GasPrices("Address")
                        area = GasPrices("Area")
                        state = GasPrices("State")
                        country = GasPrices("Country")
                        timeStamp = GasPrices("TimeStamp")
                        xdecimal = GasPrices("Decimal")
                        Dim mapLoc As Array = {name, address, area, state, country}

                        Dim shpPriceBackground As New OMBasicShape("shpPriceBackground" & ind.ToString, x_coord, y_coord, 180, 465, New ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5))

                        Dim invButton As OMButton = OMButton.PreConfigLayout_CleanStyle("invButton" & ind.ToString, x_coord, y_coord, 180, 465, OpenMobile.Graphics.GraphicCorners.All)
                        invButton.BackgroundColor = Color.Transparent
                        invButton.BorderColor = Color.Transparent
                        invButton.Opacity = 0
                        invButton.Tag = mapLoc
                        invButton.FillColor = Color.Transparent
                        AddHandler invButton.OnHoldClick, AddressOf invButton_onHoldClick

                        Dim entryImage = New OMImage("entryImage" & ind.ToString, shpPriceBackground.Left + 5, shpPriceBackground.Top + 5, shpPriceBackground.Width - 10, shpPriceBackground.Width - 10)
                        If data.image Is Nothing Then
                            entryImage.Image = theHost.getPluginImage(Me, "Logos|" & name)
                        Else
                            entryImage.Image = data
                        End If
                        entryImage.NullImage = theHost.getPluginImage(Me, "Logos|Unknown")

                        Dim entryName = New OMLabel("entryName" & ind.ToString, shpPriceBackground.Left + 5, shpPriceBackground.Top + 15 + entryImage.Height + 5, shpPriceBackground.Width - 10, 20)
                        entryName.AutoFitTextMode = FitModes.Fit
                        entryName.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                        entryName.FontSize = 20
                        entryName.Text = name

                        Dim entryPrice = New OMLabel("entryPrice" & ind.ToString, shpPriceBackground.Left + 5, entryName.Top + entryName.Height + 10, shpPriceBackground.Width - 10, 35)
                        entryPrice.AutoFitTextMode = FitModes.None
                        entryPrice.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                        entryPrice.FontSize = 26
                        'entryPrice.Text = FormatCurrency(Double.Parse(price, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture), xdecimal)
                        entryPrice.Text = FormatCurrency(Double.Parse(price, System.Globalization.CultureInfo.InvariantCulture), xdecimal)

                        Dim entryGrade = New OMLabel("entryGrade" & ind.ToString, shpPriceBackground.Left + 5, entryPrice.Top + entryPrice.Height + 15, shpPriceBackground.Width - 10, 30)
                        entryGrade.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                        entryGrade.AutoFitTextMode = FitModes.Fit
                        entryGrade.FontSize = 20
                        entryGrade.Text = gradeKey

                        Dim entryAddress = New OMLabel("entryAddress" & ind.ToString, shpPriceBackground.Left + 5, entryGrade.Top + entryGrade.Height + 20, shpPriceBackground.Width - 10, 70)
                        entryAddress.TextAlignment = (Alignment.WordWrap And Alignment.TopRight)
                        entryAddress.AutoFitTextMode = FitModes.Fit
                        entryAddress.FontSize = 20
                        entryAddress.Text = address

                        Dim entryArea = New OMLabel("entryArea" & ind.ToString, shpPriceBackground.Left + 5, entryAddress.Top + entryAddress.Height + 15, shpPriceBackground.Width - 10, 30)
                        entryArea.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                        entryArea.AutoFitTextMode = FitModes.Fit
                        entryArea.FontSize = 20
                        entryArea.Text = area

                        Dim entryWhen = New OMLabel("entryWhen" & ind.ToString, shpPriceBackground.Left + 5, entryArea.Top + entryArea.Height + 10, shpPriceBackground.Width - 10, 30)
                        entryWhen.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                        entryWhen.AutoFitTextMode = FitModes.Fit
                        entryWhen.FontSize = 14
                        entryWhen.Text = timeStamp

                        theContainer.addControlRelative(shpPriceBackground)
                        theContainer.addControlRelative(entryImage)
                        theContainer.addControlRelative(entryGrade)
                        theContainer.addControlRelative(entryPrice)
                        theContainer.addControlRelative(entryName)
                        theContainer.addControlRelative(entryAddress)
                        theContainer.addControlRelative(entryArea)
                        theContainer.addControlRelative(entryWhen)
                        theContainer.addControlRelative(invButton)

                        x_coord = x_coord + 200

                    Next

                Next ' Looping through highest level dictionary

            End If

            If Not String.IsNullOrEmpty(message) Then
                If sender.IsVisible(screen) Then
                    'theHost.UIHandler.InfoBanner_Hide(screen)
                    myBanner.Text = message
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    ' We received the message, clear it out
                    theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Messages", "")
                End If
            Else
                theHost.UIHandler.InfoBanner_Hide(screen)
            End If

        End Sub
        Private Sub invButton_onHoldClick(ByVal sender As OMButton, ByVal screen As Integer)

            Dim sname As String = sender.Name
            Dim mapLoc As String() = sender.Tag
            Dim x As String = ""
            Dim y As Integer = 0

            Dim myLoc As OpenMobile.Location = New OpenMobile.Location

            y = mapLoc(1).IndexOf("&")
            If y >= 0 Then
                mapLoc(1) = mapLoc(1).Substring(0, y - 1)
            Else
                y = mapLoc(1).IndexOf("near")
                If y >= 0 Then
                    mapLoc(1) = mapLoc(1).Substring(0, y - 1)
                End If
            End If
            myLoc.Address = mapLoc(1)
            myLoc.City = mapLoc(2)
            myLoc.State = mapLoc(3)
            myLoc.Country = mapLoc(4)
            'myLoc.Zip = "14305"

            myLoc.Keyword = myLoc.Address + ", " + myLoc.City + ", " + myLoc.State + ", " + myLoc.Country
            myLoc = theHost.CommandHandler.ExecuteCommand("Map.Lookup.Location", myLoc)

            ' Clear all previous markers
            theHost.CommandHandler.ExecuteCommand(screen, "Map.Markers.Clear")
            ' Clears all our markers only
            'theHost.CommandHandler.ExecuteCommand("Map.Marker.DeleteAll", Me.pluginName)

            ' Place markers on map
            ' My Current Location
            theHost.CommandHandler.ExecuteCommand(screen, "Map.Marker.Add", {OM.Host.CurrentLocation, Me.pluginName, "Me", 1, "Me"})
            theHost.CommandHandler.ExecuteCommand(screen, "Map.Marker.Add", {myLoc, Me.pluginName, "Target", 2, myLoc.Keyword})
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Show.MapPlugin")
            x = theHost.CommandHandler.ExecuteCommand(screen, "Map.Route", Nothing, myLoc)
            'System.Threading.Thread.Sleep(5000)
            OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Zoom.Level", 16)
            'System.Threading.Thread.Sleep(5000)
            'OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Marker.ZoomAll")

            ' x = theHost.CommandHandler.ExecuteCommand(screen, "Map.Goto.Location", myLoc)
            ' x = theHost.CommandHandler.ExecuteCommand(screen, "Map.Goto.Location", OM.Host.CurrentLocation)
            ' OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Marker.Add", New Location(60.72638, 10.61718), Me.pluginName, "testmarker", OM.Host.getSkinImage("OMFuel|Logos|Shell").image.Copy().Resize(30), "Shell\r\n$1.222 (Regular)")
            ' can map to button with this
            ' btn_ShowMap.Command_Click = "Screen{:S:}.Map.Show.MapPlugin"
            ' x = theHost.CommandHandler.ExecuteCommand("Map.Zoom.Out")
            'OM.Host.CommandHandler.ExecuteCommand(screen, "Map.Goto.Current")

        End Sub

        '''<summary>
        '''Preprocess the popup menu before showing
        '''</summary>
        Private Sub PopUpMenuStrip_OnShowing(sender As ButtonStrip, screen As Integer, menuContainer As OMContainer)


        End Sub

        Private Sub grades_onClick(ByVal sender As OMButton, ByVal screen As Integer)

            ' This value will be whatever the button that was clicked
            '  Example: "Regular" or "Diesel", etc
            ' If it is a Grade value (it might also be other functions for user to select)

            Dim mPanel As OMPanel = PanelManager(screen, "FuelPrices")
            Dim fPanel As OMPanel = PanelManager(screen, "EditFavs")

            theHost.UIHandler.PopUpMenu_Hide(screen, True)

            Select Case sender.Name
                Case "Current_Button"
                    ' Ask datasource to fetch prices for Current location
                    myBanner.Text = "Please wait..."
                    myBanner.Timeout = 5000
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Current")
                Case "Home_Button"
                    ' Ask datasource to fetch prices for saved HOME location
                    myBanner.Text = "Please wait..."
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    ' Populate xZip and xCountry with info from favorite
                    Dim xZip As String = "", xCountry As String = "", xCity As String = "", xState As String = ""
                    xZip = StoredData.Get(Me, "FAVS.homeZip")
                    xCity = StoredData.Get(Me, "FAVS.homeCity")
                    xState = StoredData.Get(Me, "FAVS.homeState")
                    xCountry = StoredData.Get(Me, "FAVS.homeCountry")
                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Favorite", {xZip, xCity, xState, xCountry})
                Case "Favorites_Button"
                    ' Regular click on button opens favorite search
                    ShowPanel(screen, "EditFavs")
                    Exit Sub
                Case Else
                    ' Either clicked a Favorite or a Fuel Grade
                    If Not sender.Text.Contains("*") Then
                        ' Grade filter selected
                        GradeFilter = sender.Text
                        panel_enter(mPanel, screen)
                        Exit Select
                    End If
                    ' A favorite was selected
                    myBanner.Text = "Please wait..."
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    Dim xZip As String = "", xCountry As String = "", xCity As String = "", xState As String = ""
                    Dim x As Integer, xCount As Integer = StoredData.Get(Me, "favoriteCount")
                    For x = 1 To xCount
                        xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x.ToString))
                        xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x.ToString))
                        If sender.Text.Contains(xCity) And sender.Text.Contains(xState) Then
                            ' Found the favorite
                            Exit For
                        End If
                    Next
                    ' Should never happen, but if x > xcount then we didn't find the favorite in the database
                    If x > xCount Then
                        ' Only happens when loop condition fails (but should be in data if we put it in the popup to begin with.
                        '  something else changed it?
                        Exit Sub
                    End If
                    ' Populate xZip and xCountry with info from favorite
                    xZip = StoredData.Get(Me, String.Format("FAVS.favoriteZip{0}", x))
                    xCountry = StoredData.Get(Me, String.Format("FAVS.favoriteCountry{0}", x))

                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Favorite", {xZip, xCity, xState, xCountry})

            End Select

            'locationBox.Text = theHost.DataHandler.GetDataSource("OMDSFuelPrices;lastCity").FormatedValue.ToUpper & "," & theHost.DataHandler.GetDataSource("OMDSFuelPrices;lastState").FormatedValue.ToUpper
            'theHost.UIHandler.InfoBanner_Hide(screen)

        End Sub

        Private Function grades_onLongClick(ByVal sender As OMButton, ByVal screen As Integer) As userInteraction

            theHost.UIHandler.PopUpMenu_Hide(screen, True)

            Return Nothing

        End Function

        Private Function grades_onHoldClick(ByVal sender As OMButton, ByVal screen As Integer) As userInteraction

            theHost.UIHandler.PopUpMenu_Hide(screen, True)

            Dim mPanel As OMPanel
            mPanel = PanelManager(screen, "FuelPrices")

            Dim x As Integer, xCount As Integer = StoredData.Get(Me, "favoriteCount")
            Dim xCity As String = "", xState As String = "", xZip As String, xCountry As String

            Select Case sender.Name
                Case "Home_Button"
                    ' Save last searched location as HOME
                    StoredData.Set(Me, "FAVS.homeCity", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.City").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeState", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.State").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeZip", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Code").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeCountry", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Country").FormatedValue.ToUpper)
                    myBanner.Text = "Saved as HOME"
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                Case "Favorites_Button"
                    ' Save last searched location to Favorites
                    If xCount > 0 Then
                        For x = 1 To xCount
                            xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x))
                            xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x))
                            xZip = StoredData.Get(Me, String.Format("FAVS.favoriteZip{0}", x))
                            xCountry = StoredData.Get(Me, String.Format("FAVS.favoriteCountry{0}", x))
                            If theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.City").FormatedValue.ToUpper.Contains(xCity) And _
                                theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.State").FormatedValue.ToUpper.Contains(xState) Then
                                ' Favorite already stored
                                Return Nothing
                            End If
                        Next
                    End If
                    xCount += 1
                    ' Favorite is not already stored. x should be xCount + 1
                    StoredData.Set(Me, String.Format("FAVS.favoriteCity{0}", xCount), theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.City").FormatedValue.ToUpper)
                    StoredData.Set(Me, String.Format("FAVS.favoriteState{0}", xCount), theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.State").FormatedValue.ToUpper)
                    StoredData.Set(Me, String.Format("FAVS.favoriteZip{0}", xCount), theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Code").FormatedValue.ToUpper)
                    StoredData.Set(Me, String.Format("FAVS.favoriteCountry{0}", xCount), theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Country").FormatedValue.ToUpper)
                    StoredData.Set(Me, "favoriteCount", xCount)
                    myBanner.Text = String.Format("Saved: {0},{1}", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.City").FormatedValue.ToUpper, theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.State").FormatedValue.ToUpper)
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    panel_enter(mPanel, screen)
                    ' Is the popup being updated?
                Case Else
                    ' Delete a favorite
                    If xCount > 0 Then
                        For x = 1 To xCount
                            xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x))
                            xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x))
                            xZip = StoredData.Get(Me, String.Format("FAVS.favoriteZip{0}", x))
                            xCountry = StoredData.Get(Me, String.Format("FAVS.favoriteCountry{0}", x))
                            If sender.Text.Contains(xCity) And sender.Text.Contains(xState) Then
                                ' Found the favorite
                                StoredData.Delete(Me, String.Format("FAVS.favoriteCity{0}", x))
                                StoredData.Delete(Me, String.Format("FAVS.favoriteState{0}", x))
                                StoredData.Delete(Me, String.Format("FAVS.favoriteCountry{0}", x))
                                StoredData.Delete(Me, String.Format("FAVS.favoriteZip{0}", x))
                                StoredData.Set(Me, "favoriteCount", xCount - 1)
                                myBanner.Text = String.Format("Deleted: {0},{1}", xCity, xState)
                                theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                                panel_enter(mPanel, screen)
                                For y = x + 1 To xCount
                                    xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", y))
                                    xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", y))
                                    xZip = StoredData.Get(Me, String.Format("FAVS.favoriteZip{0}", y))
                                    xCountry = StoredData.Get(Me, String.Format("FAVS.favoriteCountry{0}", y))
                                    StoredData.Set(Me, String.Format("FAVS.favoriteCity{0}", y - 1), xCity)
                                    StoredData.Set(Me, String.Format("FAVS.favoriteState{0}", y - 1), xState)
                                    StoredData.Set(Me, String.Format("FAVS.favoriteZip{0}", y - 1), xZip)
                                    StoredData.Set(Me, String.Format("FAVS.favoriteCountry{0}", y - 1), xCountry)
                                    StoredData.Delete(Me, String.Format("FAVS.favoriteCity{0}", y))
                                    StoredData.Delete(Me, String.Format("FAVS.favoriteState{0}", y))
                                    StoredData.Delete(Me, String.Format("FAVS.favoriteCountry{0}", y))
                                    StoredData.Delete(Me, String.Format("FAVS.favoriteZip{0}", y))
                                Next
                                panel_enter(mPanel, screen)
                                Exit For
                            End If
                        Next
                    End If

            End Select

            Return Nothing

        End Function
        Public Sub okButton_OnClick(ByVal sender As OMButton, ByVal screen As Integer)
            ' Okay button = search

            Dim fPanel As OMPanel = PanelManager(screen, "EditFavs")
            Dim zipBox As OMTextBox = fPanel("favZip")
            Dim cityBox As OMTextBox = fPanel("favCity")

            Dim myLoc As OpenMobile.Location = New OpenMobile.Location
            Dim xAddress As String, xZip As String = "", xCity As String, xState As String, xCountry As String

            If String.IsNullOrEmpty(zipBox.Text) Then
                myLoc.Keyword = cityBox.Text
            Else
                xZip = zipBox.Text.Replace(" ", "")
                myLoc.Keyword = xZip
            End If
            'myLoc.Keyword = "44.1627579, -77.38323"
            myLoc = theHost.CommandHandler.ExecuteCommand("Map.Lookup.Location", myLoc)

            xAddress = myLoc.Address
            'xZip = myLoc.Zip
            xCity = myLoc.City
            xState = myLoc.State
            xCountry = myLoc.Country

            zipBox.Text = ""
            cityBox.Text = ""

            theHost.execute(eFunction.HidePanel, screen, Me.pluginName & ";EditFavs")
            theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Favorite", {xZip, xCity, xState, xCountry})

        End Sub

        Public Sub cancelButton_OnClick(ByVal sender As OMButton, ByVal screen As Integer)
            ' Cancel button = don't search

            Dim fPanel As OMPanel = PanelManager(screen, "EditFavs")
            theHost.execute(eFunction.HidePanel, screen, Me.pluginName & ";EditFavs")

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

        Public Overrides ReadOnly Property pluginDescription As String
            Get
                Return "Display local fuel prices"
            End Get
        End Property

    End Class

End Namespace

