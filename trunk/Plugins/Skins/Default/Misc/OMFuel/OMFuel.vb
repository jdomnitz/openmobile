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
Imports OpenMobile.Plugin
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.Controls
Imports OpenMobile.Data
Imports OpenMobile.helperFunctions

Namespace OMFuel

    Public NotInheritable Class OMFuel
        Inherits HighLevelCode

        Private WithEvents theHost As IPluginHost
        Private bannerStyle As Integer = 0
        Private locationText As String = "None"
        Private gps_fixed As Boolean = False
        Private currLat As Single = 0
        Private currLng As Single = 0
        Private message As String = ""
        Private PopUpMenuStrip As ButtonStrip
        Private temp_count As Integer = 0
        Private first_run As Boolean = True
        Private updated As Boolean = False

        Dim myBanner As InfoBanner = New InfoBanner(bannerStyle, "Please wait...", 5000)

        Private GradeFilter As String = ""

        Public Sub New()

            MyBase.New("OMFuel", OM.Host.getSkinImage("Icons|Icon-Gas"), 0.1, "Display local gas prices", "Fuel Prices", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

            'theHost.DebugMsg("OMFuel - initialize()", "Initializing...")

            theHost = host

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

            PanelManager.loadPanel(omFuelpanel, True)

            ' Build the panel to manually enter a favorite
            Dim fuelFavPanel As New OMPanel("EditFavs", "Fuel Prices")

            Dim shpPriceBackground As New OMBasicShape("editFavBackground", theHost.ClientArea(0).Left + 150, theHost.ClientArea(0).Top + 100, theHost.ClientArea(0).Width - 300, 300, New ShapeData(shapes.RoundedRectangle, Color.FromArgb(235, Color.Black), Color.Transparent, 0, 5))
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

            Dim okButton As OMButton = OMButton.PreConfigLayout_BasicStyle("okButton", shpPriceBackground.Left + (shpPriceBackground.Width / 2) - 110, shpPriceBackground.Top + shpPriceBackground.Height - 50, 100, 40, OpenMobile.Graphics.GraphicCorners.All)
            okButton.Text = "Ok"
            AddHandler okButton.OnClick, AddressOf okButton_OnClick
            fuelFavPanel.addControl(okButton)

            Dim cancelButton As OMButton = OMButton.PreConfigLayout_BasicStyle("cancelButton", shpPriceBackground.Left + (shpPriceBackground.Width / 2) + 10, okButton.Top, 100, 40, OpenMobile.Graphics.GraphicCorners.All)
            cancelButton.Text = "Cancel"
            AddHandler cancelButton.OnClick, AddressOf cancelButton_OnClick
            fuelFavPanel.addControl(cancelButton)

            PanelManager.loadPanel(fuelFavPanel, False)

            ' Subscribe to datasources
            'System.Threading.Thread.Sleep(3000)
            theHost.DataHandler.SubscribeToDataSource("Fuel.Price.List", AddressOf Subscription_Updated)
            theHost.DataHandler.SubscribeToDataSource("Fuel.Messages.Text", AddressOf Subscription_Updated)
            theHost.DataHandler.SubscribeToDataSource("Location.Current", AddressOf Subscription_Updated)

            'theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Last")

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            'theHost.DebugMsg("OMFuel - Subscription_Updated()", "Datasource updated.")
            Dim mPanel As OMPanel
            Dim loc As OpenMobile.Location

            Select Case sensor.FullName

                Case "Fuel.Messages.Text"
                    ' Messages from OMDSFuelPrices
                    'theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Messages.Text", "")
                    For x = 0 To theHost.ScreenCount - 1
                        mPanel = PanelManager(x, "FuelPrices")
                        If Not mPanel Is Nothing Then
                            If mPanel.IsLoaded(x) Then
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
                    updated = True
                    For x = 0 To theHost.ScreenCount - 1
                        If PanelManager.IsPanelLoaded(x, "FuelPrices") Then
                            mPanel = PanelManager(x, "FuelPrices")
                            panel_enter(mPanel, x)
                            'theHost.UIHandler.InfoBanner_Hide(x)
                        End If
                    Next
                    updated = False

                Case "Location.Current"
                    loc = sensor.Value
                    currLat = loc.Latitude
                    currLng = loc.Longitude

            End Select

        End Sub

        Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            Return mySettings

        End Function

        Public Sub panel_leave(ByVal sender As OMPanel, ByVal screen As Integer)

            theHost.UIHandler.InfoBanner_Hide(screen)
            theHost.UIHandler.PopUpMenu.ClearButtonStrip(screen)
            PopUpMenuStrip = Nothing

        End Sub

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            'theHost.DebugMsg("OMFuel - panel_enter()", "")

            Dim client As WebClient = New WebClient

            'Dim message As String = ""
            'message = theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Messages.Text").FormatedValue

            ' The following paragraph doesn't work because the banner
            '  gets wiped out immediately upon building the panel.
            'theHost.DebugMsg("OMFuel - panel_enter()", "Adding favorites to popup.")

            ' loads up the popup with the standard items

            PopUpMenuStrip = New ButtonStrip(Me.pluginName, "FuelPrices", "PopUpMenuStrip")

            If ((currLat <> "0") And (currLng <> "0")) Then
                ' We have a current location
                PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Current", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|2-action-search"), "Current", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            End If
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Home", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Home"), "Home", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem("Favorites", theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-new-label"), "Favorites/Search" & vbCrLf & "Hold to add shown", False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))

            ' Add any favorites
            Dim x As Integer, xCount As Integer = StoredData.Get(Me, "favoriteCount")
            Dim xCity As String = "", xState As String = ""
            For x = 1 To xCount
                xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x))
                xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x))
                PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem(xCity, theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("AIcons|4-collections-label"), String.Format("*{0},{1}", xCity, xState), False, AddressOf grades_onClick, AddressOf grades_onHoldClick, Nothing))
            Next

            ' Set up the main container for prices
            Dim theContainer As OMContainer = sender(screen, "mainContainer")
            theContainer.ClearControls()

            'theHost.DebugMsg("OMFuel - panel_enter()", "Creating dictionaries.")

            Dim Grades As Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, String))) = TryCast(theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Price.List").Value, Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, String))))
            Dim Results As New Dictionary(Of Integer, Dictionary(Of String, String))
            Dim GasPrices As New Dictionary(Of String, String)

            'theHost.DebugMsg("OMFuel - panel_enter()", "Filling container.")

            Dim x_coord As Integer = 10
            Dim y_coord As Integer = 15

            Dim data As imageItem
            'Dim xx As Integer
            Dim grade As String = ""
            Dim price As String = ""
            Dim name As String = ""
            Dim address As String = ""
            Dim area As String = ""
            Dim timeStamp As String = ""
            Dim xdecimal As String = ""
            Dim ind As Integer = 0

            If Not Grades Is Nothing Then

                For Each gradeKey As String In Grades.Keys

                    If String.IsNullOrEmpty(GradeFilter) Then
                        GradeFilter = gradeKey
                    End If

                    PopUpMenuStrip.Buttons.Add(Button.CreateMenuItem(gradeKey, theHost.UIHandler.PopUpMenu.ButtonSize, 255, OM.Host.getSkinImage("Icons|Icon-Gas"), gradeKey, False, AddressOf grades_onClick, Nothing, Nothing))

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
                        timeStamp = GasPrices("TimeStamp")
                        xdecimal = GasPrices("Decimal")

                        Dim shpPriceBackground As New OMBasicShape("shpPriceBackground" & ind.ToString, x_coord, y_coord, 180, 470, New ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5))

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

                        x_coord = x_coord + 200

                    Next

                Next ' Looping through highest level dictionary

            End If

            theHost.UIHandler.PopUpMenu.SetButtonStrip(PopUpMenuStrip)
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
                    myBanner.Timeout = 10000
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Current")
                Case "Home_Button"
                    ' Ask datasource to fetch prices for saved HOME location
                    myBanner.Text = "Please wait..."
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                    ' Populate xZip and xCountry with info from favorite
                    Dim xZip As String = "", xCountry As String = "", xCity As String = "", xState As String = ""
                    xZip = StoredData.Get(Me, "FAVS.homeZip")
                    xCity = StoredData.Get(Me, "FAVS.favoriteCity")
                    xState = StoredData.Get(Me, "FAVS.favoriteState")
                    xCountry = StoredData.Get(Me, "FAVS.favoriteCountry")
                    theHost.CommandHandler.ExecuteCommand("OMDSFuelPrices.Refresh.Favorite", {xZip, xCity, xState, xCountry})
                Case "Favorites_Button"
                    ' Regular click on button does nothing
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
                    xCity = StoredData.Get(Me, String.Format("FAVS.favoriteCity{0}", x))
                    xState = StoredData.Get(Me, String.Format("FAVS.favoriteState{0}", x))
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
                    ' Save current location as HOME
                    StoredData.Set(Me, "FAVS.homeCity", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.City").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeState", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.State").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeZip", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Code").FormatedValue.ToUpper)
                    StoredData.Set(Me, "FAVS.homeCountry", theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Last.Country").FormatedValue.ToUpper)
                    myBanner.Text = "Saved as HOME"
                    theHost.UIHandler.InfoBanner_Show(screen, myBanner)
                Case "Favorites_Button"
                    ' Add displayed location to favorites
                    ' Hard coded example
                    'Dim xZip As String = "M5H2N2"
                    'Dim xCountry As String = "CA"
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
            'Dim stateBox As OMTextBox = fPanel("favState")

            Dim xZip As String, xCity As String, xState As String, xCountry As String
            xZip = zipBox.Text.ToUpper
            xCity = cityBox.Text.Substring(0, cityBox.Text.IndexOf(","))
            xState = cityBox.Text.Substring(cityBox.Text.IndexOf(",") + 1)
            xCountry = ""

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

