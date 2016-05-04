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
' OMDSFuelPrices provides dataSource data for local gas prices
'
' OMDSFuelPrices requires an active internet connection in order to fetch prices
' 
' This code requires various Open Mobile libraries
' included in the VB.NET project and MUST be compiled agains .NET 3.5

#End Region

Imports System
Imports System.Net
Imports System.Web

Imports OpenMobile
Imports OpenMobile.Plugin
Imports OpenMobile.Framework
Imports OpenMobile.Graphics
Imports OpenMobile.Data
Imports OpenMobile.Threading
Imports OpenMobile.helperFunctions
'Imports System.Windows.Forms

Namespace OMDSFuelPrices

    Public Structure Country
        Public Name As String
        Public Sub New(ByVal name As String)
            Me.Name = name
        End Sub
    End Structure

    Public Class OMDSFuelPrices
        Inherits BasePluginCode
        Implements IDataSource

        Private status As Integer = 0
        Private status_msg As String = ""
        Private scrapeCounter As Integer = 0

        Private WithEvents theHost As IPluginHost
        'Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, Me.pluginName, "", AddressOf myNotification_clickAction, AddressOf myNotification_clearAction)
        Private myNotification As Notification = New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, Me.pluginName, "")

        Private maxWait As Integer = 5000               ' Max interval (ms) between checking GPS fixed
        Private m_Verbose As Boolean = False
        Private request_bytes As Integer = 0            ' Track the size of fetched data
        Private session_bytes As Integer = 0            ' Track the session bytes
        Private PriceList(6, 0)
        Private maxRecords As Integer = 20              ' Max price records to fetch (per Grade)
        Private maxAge As Integer = 24                  ' Max age of price data
        Private myLatitude As String
        Private myLongitude As String
        Private gps_fix As Boolean
        Private favZip As String = ""                   ' Skin request to search specific postal code
        Private favCountry As String = ""               ' Skin request to search specific country
        Private searchGrade As String = "A"             ' Fuel Grade to search for
        Private searchZip As String = ""                ' The ZIP/Postal code
        Private searchCountry As String = ""            ' The Country
        Private searchCity As String = ""               ' The City
        Private searchState As String = ""              ' The State/Prov
        Private searchParam As String = ""              ' The string to scrape with
        Private searchFormat As Integer = 1             ' Ajdusts the searched price (Canadian is in pennies, not dollars)
        Private searchDecimal As Integer = 2            ' The number of currency decimal places that the fuel price should be shown in

        Public Sub New()

            MyBase.New("OMDSFuelPrices", OM.Host.getSkinImage("Icons|Icon-Gas"), 0.1, "Fetch gas prices", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Function CommandExecutor(ByVal command As Command, ByVal param() As Object, ByRef result As Boolean) As Object

            If m_Verbose Then
                theHost.DebugMsg("OMDSFuelPrices - CommandExecutor()", String.Format("Processing command {0}.", command.FullName))
            End If

            Dim loc As Location
            Dim mFilter As String = ""

            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Messages.Text", Nothing)

            result = False

            Select Case command.NameLevel2
                Case "Refresh"
                    mFilter = command.NameLevel3
                    If mFilter = "Last" Then
                        mFilter = StoredData.Get(Me, "last.Filter")
                    End If
                    StoredData.Set(Me, "last.Filter", mFilter)
                    Select Case mFilter
                        Case "Current"
                            loc = theHost.CurrentLocation
                            searchZip = loc.Zip.Replace(" ", "")
                            searchZip = searchZip.Replace("-", "")
                            searchCity = loc.City
                            searchState = loc.State
                            searchCountry = loc.Country
                            If m_Verbose Then
                                theHost.DebugMsg("OMDSFuelPrices - CommandExecutor()", String.Format("Searching {0}, {1}, {2}.", searchCity, searchState, searchZip))
                            End If
                            result = refreshData()
                        Case "Favorite"
                            If param Is Nothing Then
                                ' Load the values we need from last search
                                searchZip = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.Code").FormatedValue
                                searchCity = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.City").FormatedValue
                                searchState = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.State").FormatedValue
                                searchCountry = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.Country").FormatedValue
                            Else
                                ' Extract the favorite to the search variables
                                searchZip = param(0)
                                searchCity = param(1)
                                searchState = param(2)
                                searchCountry = param(3)
                            End If
                            result = refreshData()
                        Case "Home"
                            If param Is Nothing Then
                                ' Load the values we need
                                searchZip = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.Code").FormatedValue
                                searchCity = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.City").FormatedValue
                                searchState = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.State").FormatedValue
                                searchCountry = theHost.DataHandler.GetDataSource(Me.pluginName & ";Fuel.Last.Country").FormatedValue
                            Else
                                ' Extract the favorite to the search variables
                                searchZip = param(0)
                                searchCity = param(1)
                                searchState = param(2)
                                searchCountry = param(3)
                            End If
                            result = refreshData()
                        Case Else
                            result = False
                    End Select
                    StoredData.Set(Me, "last.Filter", mFilter)
                    theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.Filter", mFilter)
                    If Not String.IsNullOrEmpty(status_msg) Then
                        theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Messages.Text", status_msg)
                        status_msg = ""
                    End If
                Case "Save"
                    ' We really don't save anything here. Skin manages favorites
                    ' Maybe sometime in the future
                    Select Case command.NameLevel3
                        Case Else
                            result = False
                    End Select
                Case Else
                    ' There is no previous search
                    result = False
            End Select

            Return result

        End Function

        Private Sub systemEvents(ByVal eFunction As eFunction, ByVal args As Object()) Handles theHost.OnSystemEvent

            Select Case eFunction
                Case eFunction.connectedToInternet
                    ' Fetch last filter
                Case eFunction.disconnectedFromInternet

            End Select

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

            theHost = host

            theHost.UIHandler.AddNotification(myNotification)

            If Not String.IsNullOrEmpty(StoredData.Get(Me, "refreshRate")) Then
                StoredData.Delete(Me, "refreshRate")
            End If

            ' Set up max returned records
            StoredData.SetDefaultValue(Me, "settings.max.Records", maxRecords)
            maxRecords = Val(StoredData.Get(Me, "settings.max.Records")) ' Maximum results to fetch

            ' Set data age 
            StoredData.SetDefaultValue(Me, "settings.max.Age", maxAge)
            maxAge = Val(StoredData.Get(Me, "settings.max.Age")) ' Maximum age

            ' Set up settings values
            'theHost.DebugMsg("OMDSFuelPrices - initialze()", "Creating data items...")

            StoredData.SetDefaultValue(Me, "settings.VerboseDebug", m_Verbose)
            m_Verbose = StoredData.Get(Me, "settings.VerboseDebug")

            Dim filter As String

            StoredData.SetDefaultValue(Me, "settings.last.Update", "")
            StoredData.SetDefaultValue(Me, "settings.last.Code", "")
            searchZip = StoredData.Get(Me, "settings.last.Code")

            StoredData.SetDefaultValue(Me, "settings.last.Country", "")
            searchCountry = StoredData.Get(Me, "settings.last.Country")

            StoredData.SetDefaultValue(Me, "settings.last.City", "")
            searchCity = StoredData.Get(Me, "settings.last.City")

            StoredData.SetDefaultValue(Me, "settings.last.State", "")
            searchState = StoredData.Get(Me, "settings.last.State")

            StoredData.SetDefaultValue(Me, "settings.last.Filter", "Home")
            filter = StoredData.Get(Me, "settings.last.Filter")

            ' Set up data source
            'theHost.DebugMsg("OMDSFuelPrices - initialze()", "Creating datasource...")
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Price", "List", DataSource.DataTypes.raw, "Fuel Price Scrape Results"))
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Messages", "Text", DataSource.DataTypes.text, "Latest status/error message", ""))
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Last", "Filter", DataSource.DataTypes.text, "Last search filter", filter))
            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.Filter", filter)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Last", "City", DataSource.DataTypes.text, "Last searched city", searchCity))
            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.City", searchCity)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Last", "State", DataSource.DataTypes.text, "Last searched state", searchState))
            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.State", searchState)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Last", "Code", DataSource.DataTypes.text, "Last searched zip", searchZip))
            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.Code", searchZip)
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Last", "Country", DataSource.DataTypes.text, "Last searched country", searchCountry))
            theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Last.Country", searchCountry)

            ' Set up commands
            'theHost.DebugMsg("OMDSFuelPrices - initialze()", "Creating commands...")
            theHost.CommandHandler.AddCommand(New Command(Me, "OMDSFuelPrices", "Refresh", "Current", AddressOf CommandExecutor, 0, True, "Update prices for local area"))
            theHost.CommandHandler.AddCommand(New Command(Me, "OMDSFuelPrices", "Refresh", "Favorite", AddressOf CommandExecutor, 4, True, "Update prices for specified area"))
            theHost.CommandHandler.AddCommand(New Command(Me, "OMDSFuelPrices", "Refresh", "Home", AddressOf CommandExecutor, 0, True, "Update prices for Home area"))
            theHost.CommandHandler.AddCommand(New Command(Me, "OMDSFuelPrices", "Refresh", "Last", AddressOf CommandExecutor, 0, True, "Update prices for last selected"))
            theHost.CommandHandler.AddCommand(New Command(Me, "OMDSFuelPrices", "Save", "Home", AddressOf CommandExecutor, 0, True, "Saves current location as HOME"))

            ' Subscribe to datasources
            theHost.DataHandler.SubscribeToDataSource("OMGPS;GPS.Sat.Fix", AddressOf Subscription_Updated)

            SafeThread.Asynchronous(AddressOf BackgroundLoad, host)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            Select Case sensor.FullName
                Case "GPS.Sat.Fix"
                    gps_fix = sensor.Value
                Case "Location.Current"

            End Select

        End Sub

        Private Sub myNotification_clickAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        Private Sub myNotification_clearAction(notification As Notification, screen As Integer, ByRef cancel As Boolean)
            ' Not currently used
            cancel = False
        End Sub

        Private Sub BackgroundLoad()

            Dim result As Boolean = False
            Dim city As String, state As String, country As String, code As String

            code = StoredData.Get(Me, "settings.last.Code")
            city = StoredData.Get(Me, "settings.last.City")
            state = StoredData.Get(Me, "settings.last.State")
            country = StoredData.Get(Me, "settings.last.Country")

        End Sub

        Private Sub set_message(ByVal message As String)

            status_msg = message

        End Sub

        Public Function refreshData() As Boolean
            ' This is the standard method for loading price data

            Dim result = False

            ' Status must be 0 to start a new scrape

            If status <> 0 Then
                Return False
            End If

            ' flag that the task has been queued
            status = 1

            If m_Verbose Then
                theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Attempting scrape."))
            End If

            result = getInfo()

            If m_Verbose Then
                theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Returned from scrape."))
            End If

            If result Then
                scrapeCounter += 1
                If m_Verbose Then
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Completed scrape #{0}.", scrapeCounter))
                End If
                StoredData.Set(Me, "last.Update", DateTime.Now.ToString("s"))
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Last.City", searchCity.ToUpper)
                helperFunctions.StoredData.Set(Me, "settings.last.City", searchCity.ToUpper)
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Last.Code", searchZip.ToUpper)
                helperFunctions.StoredData.Set(Me, "settings.last.Code", searchZip.ToUpper)
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Last.State", searchState.ToUpper)
                helperFunctions.StoredData.Set(Me, "settings.last.State", searchState.ToUpper)
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Last.Country", searchCountry.ToUpper)
                helperFunctions.StoredData.Set(Me, "settings.last.Country", searchCountry.ToUpper)
                theHost.execute(eFunction.dataUpdated, "OMDSFuelPrices")
            Else
                ' Tell the user we cannot scrape
                theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), "Update failed.")
                'theHost.UIHandler.RemoveAllMyNotifications(Me)
                'theHost.UIHandler.AddNotification(New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, Me.pluginName, "Update failed."))
                myNotification.Text = "Update failed"
            End If
            ' Scrape process is completed, reset flag
            status = 0

            Return result

        End Function

        Private Function getInfo() As Boolean

            ' Determine how to fetch the scrape data

            ' For US/Canada we only need a zip/postal
            ' To support other countries, we need a scrape source
            If m_Verbose Then
                theHost.DebugMsg("OMDSFuelPrices - getInfo()", "Entering...")
            End If

            myNotification.Text = "Updating..."

            Dim result As Boolean = False

            Select Case searchCountry.ToUpper
                Case "US", "UNITED STATES" ' United States
                    If String.IsNullOrEmpty(searchZip) Then
                        set_message("Insufficient data to perform search.")
                        result = False
                    Else
                        searchFormat = 1
                        searchDecimal = 2
                        searchParam = "http://www.gasbuddy.com/findsite.aspx?zip=" & searchZip
                        result = scrape_gasbuddy()
                    End If
                Case "CA", "CANADA" ' Canada
                    If String.IsNullOrEmpty(searchZip) Then
                        ' cannot search without info
                        set_message("Insufficient data to perform search.")
                        result = False
                    Else
                        searchFormat = 100
                        searchDecimal = 3
                        ' This one accepts Canadian Postal Codes as well as ZIP codes
                        searchParam = "http://www.gasbuddy.com/findsite.aspx?search=" & searchZip
                        result = scrape_gasbuddy()
                    End If
                Case ""
                    set_message("No location information.")
                    result = False
                Case Else ' Unknown country
                    set_message(String.Format("Unsupported country code {0}.", searchCountry))
                    result = False
            End Select

            'theHost.UIHandler.RemoveAllMyNotifications(Me)
            If result Then
                'theHost.DebugMsg("OMDSFuelPrices - getInfo()", String.Format("Updated {0}: {1},{2}", StoredData.Get(Me, "last.Filter"), searchCity, searchState))
                myNotification.Text = String.Format("Updated {0}: {1},{2}", StoredData.Get(Me, "last.Filter"), searchCity, searchState)
            Else
                'theHost.DebugMsg("OMDSFuelPrices - getInfo()", status_msg)
                myNotification.Text = status_msg
            End If

            Return result

        End Function

        Private Function scrape_gasbuddy() As Boolean

            Dim client As WebClient = New WebClient
            Dim x As Integer = 0
            Dim y As Integer = 0
            Dim data As String = ""
            Dim baseAddress As String = ""
            Dim URL_Grade(-1) As String ' Array of available fuel grades
            Dim URL_Paths(-1) As String ' Array of grade URLs
            Dim URL_Regular = ""        ' URL for regular fuel listings
            Dim URL_Midgrade = ""       ' URL for mid-grade fuel listings
            Dim URL_Premium = ""        ' URL for premium fuel listings
            Dim URL_Diesel = ""         ' URL for diesel fuel listings

            If m_Verbose Then
                theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), String.Format("Scraping for: {0}.", searchZip))
            End If

            Try
                If theHost.InternetAccess Then
                    data = client.DownloadString(searchParam)
                    request_bytes = data.Length
                    session_bytes = session_bytes + request_bytes
                    If m_Verbose Then
                        theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Fetched {0} bytes (Subtotal {1} bytes)", request_bytes, session_bytes))
                    End If
                Else
                    ' No Internet
                    set_message("Internet not available")
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), "Lost internet. Aborting scrape.")
                    'theHost.UIHandler.RemoveAllMyNotifications(Me)
                    'theHost.UIHandler.AddNotification(New Notification(Me, Me.pluginName(), Me.pluginIcon.image, Me.pluginIcon.image, Me.pluginName, status_msg))
                    myNotification.Text = status_msg
                    Return False
                End If

            Catch ex As Exception
                theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), String.Format("SearchParam: {0}.", searchParam))
                theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), String.Format("Exception: {0}.", ex.Message))
                set_message("Error fetching data.")
                myNotification.Text = status_msg
                Return False

            End Try

            ' Pull the base URI for subsequent requests
            x = data.IndexOf("ctl00_head_base"" href=""")
            If x < 0 Then
                ' We didn't find a base address
                set_message("No results.")
                theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Could not determine the base address.")
            End If
            y = data.IndexOf(""">", x)
            baseAddress = data.Substring(x + 23, y - (x + 23))

            ' Find the start of the price table
            x = data.IndexOf("<div id=""pp_table"">")
            If x < 0 Then
                ' Did not find what we need
                set_message("No results.")
                theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Did not find a data table")
                Return False
            End If

            ' Find the other fuel grade URLs <ul class="p_ft">
            ' Not currently saving this.  But we will when we save our datasource(s) as a list

            ' ctl00_Content_P_hlRegular
            ' Our initial scrape is always using regular so we really don't need the link
            '  but I captured it anyway
            x = data.IndexOf("ctl00_Content_P_hlRegular")
            If x < 0 Then
                ' No URL found
                set_message("Unexpected format.")
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Data not in expected format")
                End If
                myNotification.Text = "Data not in expected format."
            End If

            ' grab the URL
            y = data.IndexOf(""">", x)
            If y >= 0 Then
                URL_Regular = baseAddress & data.Substring(x + 33, y - (x + 33)).Replace("&amp;", "&")
                ReDim Preserve URL_Grade(URL_Grade.GetUpperBound(0) + 1)
                URL_Grade(URL_Grade.GetUpperBound(0)) = "Regular"
                ReDim Preserve URL_Paths(URL_Paths.GetUpperBound(0) + 1)
                URL_Paths(URL_Grade.GetUpperBound(0)) = URL_Regular
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Regular: {0}", URL_Regular))
                End If
            End If

            ' ctl00_Content_P_hlMidgrade
            x = data.IndexOf("ctl00_Content_P_hlMidgrade")
            If x < 0 Then
                ' No URL found
                set_message("Unexpected format.")
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Data not in expected format")
                End If
                myNotification.Text = "Data not in expected format."
            End If

            ' grab the URL
            y = data.IndexOf(""">", x)
            If y >= 0 Then
                URL_Midgrade = baseAddress & data.Substring(x + 34, y - (x + 34)).Replace("&amp;", "&")
                ReDim Preserve URL_Grade(URL_Grade.GetUpperBound(0) + 1)
                URL_Grade(URL_Grade.GetUpperBound(0)) = "Midgrade"
                ReDim Preserve URL_Paths(URL_Paths.GetUpperBound(0) + 1)
                URL_Paths(URL_Grade.GetUpperBound(0)) = URL_Midgrade
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Midgrade: {0}", URL_Midgrade))
                End If
            End If

            ' ctl00_Content_P_hlPremium
            x = data.IndexOf("ctl00_Content_P_hlPremium")
            If x < 0 Then
                ' No URL found
                set_message("Unexpected format.")
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Data not in expected format")
                End If
                myNotification.Text = "Data not in expected format."
            End If

            ' grab the URL
            y = data.IndexOf(""">", x)
            If y >= 0 Then
                URL_Premium = baseAddress & data.Substring(x + 33, y - (x + 33)).Replace("&amp;", "&")
                ReDim Preserve URL_Grade(URL_Grade.GetUpperBound(0) + 1)
                URL_Grade(URL_Grade.GetUpperBound(0)) = "Premium"
                ReDim Preserve URL_Paths(URL_Paths.GetUpperBound(0) + 1)
                URL_Paths(URL_Grade.GetUpperBound(0)) = URL_Premium
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Premium: {0}", URL_Premium))
                End If
            End If

            ' ctl00_Content_P_hlDiesel
            x = data.IndexOf("ctl00_Content_P_hlDiesel")
            If x < 0 Then
                ' No URL found
                set_message("Unexpected format.")
                theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Data not in expected format")
                myNotification.Text = "Data not in expected format."
            End If

            ' grab the URL
            y = data.IndexOf(""">", x)
            If y >= 0 Then
                URL_Diesel = baseAddress & data.Substring(x + 32, y - (x + 32)).Replace("&amp;", "&")
                ReDim Preserve URL_Grade(URL_Grade.GetUpperBound(0) + 1)
                URL_Grade(URL_Grade.GetUpperBound(0)) = "Diesel"
                ReDim Preserve URL_Paths(URL_Paths.GetUpperBound(0) + 1)
                URL_Paths(URL_Grade.GetUpperBound(0)) = URL_Diesel
                If m_Verbose Then
                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Diesel: {0}", URL_Diesel))
                End If
            End If

            ' If count of URL_Grade = 0 then there was no fuel grades found
            ' (although we should always find regular or we wouldn't have found anything)
            If URL_Grade.Length < 0 Then
                set_message("No prices found")
                If m_Verbose Then
                    theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Price.List", Nothing)
                End If
                Return False
            End If

            ' Start loop for the fuel grade pages

            Dim row_counter As Integer = 0
            Dim z As Integer = 0
            Dim a As Integer = 0
            Dim b As Integer = 0
            Dim c As Integer = 0
            Dim w As String = ""
            Dim price As String = ""
            Dim dprice As Double = 0
            Dim name As String = ""
            Dim imageName As String = ""
            Dim address As String = ""
            Dim area As String = ""
            Dim timeStamp As String = ""
            Dim record As String = ""
            Dim result As Boolean = False
            ReDim PriceList(6, 0)

            y = 0

            ' Create the higher level dictionary
            Dim Grades As New Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, String)))

            For v = 0 To URL_Grade.GetUpperBound(0)

                v = v
                row_counter = 0

                ' The linking level dictionary
                Dim Results As New Dictionary(Of Integer, Dictionary(Of String, String))

                ' Loop here
                Do While True

                    ' Find the start of row entry
                    w = "<tr id=""rrlow_" & row_counter.ToString
                    x = data.IndexOf(w, x)
                    If x < 0 Then
                        ' No Gas data or table not found
                        If m_Verbose Then
                            theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Data not found for {0}.", URL_Grade(v)))
                        End If
                        Exit Do
                    End If
                    If m_Verbose Then
                        theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Found record: {0}", w))
                    End If
                    ' Find the start of the price line
                    y = data.IndexOf("<div class=""sp_p", x)
                    ' Find the end of the price line
                    z = data.IndexOf("</div></div>", y)
                    ' loop between y and z finding price digits
                    price = ""
                    Do While y < z
                        y = data.IndexOf("<div class=""p", y)
                        If (y < 0) Or (y > z) Then
                            ' no more digits
                            Exit Do
                        End If
                        price = price & data.Substring(y + 13, 1)
                        y = y + 13
                    Loop
                    price = price.Replace("d", ".")
                    If m_Verbose Then
                        theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Found price: {0}", price))
                    End If

                    x = data.IndexOf("<dl class=""address"">", z)
                    a = data.IndexOf("<dt>", x)
                    b = data.IndexOf("</dt>", a)
                    w = data.Substring(a + 4, b - (a + 4))
                    a = w.LastIndexOf(";"">")
                    b = w.LastIndexOf("</a>")
                    name = w.Substring(a + 3, b - (a + 3))

                    ' Fetch GasBuddy's attached image (not used yet)
                    a = w.IndexOf("src=""", 1)
                    b = w.IndexOf("?", a + 5)
                    If a < 0 Or b < 0 Then
                        imageName = ""
                    Else
                        imageName = w.Substring(a + 5, b - (a + 5))
                    End If

                    ' Get the station address
                    x = data.IndexOf("<dd>", x)
                    z = data.IndexOf("</dd>", x)
                    address = data.Substring(x + 4, z - (x + 4))
                    address = address.Replace("&amp;", "&")

                    x = data.IndexOf("class=""p_area"">", z)
                    z = data.IndexOf("</a>", x)
                    area = data.Substring(x + 15, z - (x + 15))

                    x = data.IndexOf("title=""", z)
                    z = data.IndexOf(""">", x)
                    timeStamp = data.Substring(x + 7, z - (x + 7))

                    ' This array will ultimately be the datasource
                    '  (but probably as a list or something

                    dprice = Double.Parse(price, System.Globalization.CultureInfo.InvariantCulture) / searchFormat
                    record = "|" & URL_Grade(v) & "|" & dprice & "|" & name & "|" & address & "|" & area & "|" & timeStamp & "|" & searchDecimal & "|"

                    ' Empty the innermost dictionary
                    Dim GasPrices As New Dictionary(Of String, String)

                    'GasPrices.Add(row_counter, record)
                    GasPrices.Add("Grade", URL_Grade(v))
                    GasPrices.Add("Price", dprice.ToString)
                    GasPrices.Add("Name", name)
                    GasPrices.Add("Address", address)
                    GasPrices.Add("Area", area)
                    GasPrices.Add("State", searchState)
                    GasPrices.Add("Country", searchCountry)
                    GasPrices.Add("TimeStamp", timeStamp)
                    GasPrices.Add("Decimal", searchDecimal)

                    If m_Verbose Then
                        theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Found: {0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", _
                                                                                            GasPrices("Grade"), _
                                                                                             GasPrices("Price"), _
                                                                                             GasPrices("Name"), _
                                                                                             GasPrices("Address"), _
                                                                                             GasPrices("Area"), _
                                                                                             GasPrices("State"), _
                                                                                             GasPrices("Country"), _
                                                                                             GasPrices("TimeStamp"), _
                                                                                             GasPrices("Decimal")))
                    End If

                    Results.Add(row_counter, GasPrices)

                    ' Something from here to the top of the loop we lose data.
                    row_counter += 1

                    If row_counter > maxRecords - 1 Then
                        Exit Do
                    End If

                    x = z

                Loop ' Loop through results for grade

                If Results.Count = 0 Then
                    Exit For
                End If

                Grades.Add(URL_Grade(v), Results)

                ' Fetch the page for the next grade

                If v < URL_Grade.GetUpperBound(0) Then

                    Try
                        If m_Verbose Then
                            'theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), "Requesting page: " & URL_Paths(v + 1))
                        End If
                        data = client.DownloadString(URL_Paths(v + 1))
                    Catch
                        set_message("Lost internet?")
                        theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), "Lost internet mid scrape.")
                        Return False
                    End Try

                End If

                x = 0

            Next ' Loop through grades

            If Grades.Count > 0 Then
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Price.List", Grades)
            Else
                set_message("No prices")
                theHost.DataHandler.PushDataSourceValue("OMDSFuelPrices;Fuel.Price.List", Nothing)
            End If

            Return True

        End Function

        Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            ' Create a handler for settings changes
            AddHandler mySettings.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            Dim cOptions2 As List(Of String) = New List(Of String)
            cOptions2.Add("4")
            cOptions2.Add("8")
            cOptions2.Add("12")
            cOptions2.Add("24")
            cOptions2.Add("36")
            Dim settingMaxAge = New Setting(SettingTypes.MultiChoice, "max.Age", "Max Age", "Max age of prices (hours)", cOptions2, cOptions2)
            settingMaxAge.Value = StoredData.Get(Me, "max.Age")
            'mySettings.Add(settingMaxAge)

            Dim cOptions3 As List(Of String) = New List(Of String)
            cOptions3.Add("5")
            cOptions3.Add("10")
            cOptions3.Add("20")
            cOptions3.Add("25")
            cOptions3.Add("30")
            Dim settingMaxRecords = New Setting(SettingTypes.MultiChoice, "max.Records", "Max Records", "Maximum results to fetch", cOptions3, cOptions3)
            settingMaxRecords.Value = StoredData.Get(Me, "max.Records")
            mySettings.Add(settingMaxRecords)

            Dim cOptions4 As List(Of String) = New List(Of String)
            cOptions4.Add("CA")
            cOptions4.Add("US")
            Dim settingHomeCountry = New Setting(SettingTypes.MultiChoice, "home.Country", "Home Country", "Set Home Country Code", cOptions4, cOptions4)
            settingHomeCountry.Value = StoredData.Get(Me, "home.Country")
            mySettings.Add(settingHomeCountry)

            Dim settingHomeCode = New Setting(SettingTypes.Text, "home.Code", "Home Zip/Postal", "Set Home Zip / Postal Code", OpenMobile.helperFunctions.StoredData.Get(Me, "homeCode"))
            mySettings.Add(settingHomeCode)

            Dim verboseDebug = New Setting(SettingTypes.MultiChoice, "settings.VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose)
            mySettings.Add(verboseDebug)
            'mySettings.Add(New Setting(SettingTypes.MultiChoice, "VerboseDebug", "Verbose", "Verbose Debug Logging", Setting.BooleanList, Setting.BooleanList, m_Verbose))

            Return mySettings

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)
            ' Setting was changed, update database

            OpenMobile.helperFunctions.StoredData.Set(Me, settings.Name, settings.Value)

            ' These settings take effect immediately

            Select Case settings.Name
                Case "settings.VerboseDebug"
                    m_Verbose = CBool(settings.Value)
            End Select

        End Sub

    End Class

End Namespace