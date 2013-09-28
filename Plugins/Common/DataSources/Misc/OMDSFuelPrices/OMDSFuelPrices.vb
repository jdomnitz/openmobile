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
Imports System.Windows.Forms

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
        Private scrapeCounter As Integer = 0
        Private refreshRate As Double = 30000
        Private WithEvents theHost As IPluginHost
        Private myNotification As Notification
        Private PriceList(5, 0)
        Private maxRecords As Integer = 30
        Private myLatitude As String
        Private myLongitude As String
        Private searchGrade As String = "A"             ' Fuel Grade to search for
        Private searchLocation As String = ""           ' Not required?
        Private searchZip As String = "14305"           ' The ZIP/Postal code
        Private searchParam As String = ""              ' The string to scrape with
        Private searchCity As String = "Niagara Falls"  ' The City
        Private searchState As String = "NY"            ' The State/Prov
        Private searchCountry As String = "US"          ' The Country
        Private searchCurrent As Boolean = True         ' Search current location (via GPS)?

        Private WithEvents m_Refresh_tmr As New Timers.Timer(refreshRate)

        Public Sub New()

            MyBase.New("OMDSFuelPrices", OM.Host.getSkinImage("Icons|Icon-Gas"), 0.1, "Retrieve local gas prices", "John Mullan", "jmullan99@gmail.com")

        End Sub

        Public Overrides Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus

            theHost = host

            ' Set up data source
            theHost.DebugMsg("OMDSFuelPrices - initialze()", "Creating datasource.")
            theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Price", "Count", DataSource.DataTypes.raw, "Number of available fuel prices", -1))

            m_Refresh_tmr.Enabled = False
            m_Refresh_tmr.AutoReset = False

            theHost.DebugMsg("OMDSFuelPrices - initialze()", "Initialization invoked.")

            StoredData.Set(Me, "Data.SearchCurrent", searchCurrent)
            StoredData.Set(Me, "Data.DefaultCountry", searchCountry)
            StoredData.Set(Me, "Data.DefaultState", searchState)
            StoredData.Set(Me, "Data.DefaultZip", searchZip)

            ' Do first fetch of prices (after which the timer will be started)
            ' Cannot refresh if no internet.  Perhaps set a thread to do the waiting

            SafeThread.Asynchronous(AddressOf BackgroundLoad, theHost)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub BackgroundLoad()

            Dim timeout As Integer = 30000
            Dim startTime As DateTime
            Dim elapsedTime As TimeSpan
            Dim lastSeconds As Integer = 0
            startTime = Now

            Do While True

                If theHost.InternetAccess Then
                    ' we have internet access, lets do the first fetch
                    getPrices()
                    m_Refresh_tmr.Enabled = True
                    m_Refresh_tmr.Start()
                    Exit Sub
                End If

                If elapsedTime.Seconds > timeout Then
                    ' We didn't get any internet so start the timer anyway
                    getPrices()
                    m_Refresh_tmr.Enabled = True
                    m_Refresh_tmr.Start()
                    Exit Sub
                End If

            Loop

        End Sub

        Public Function refreshData() As Boolean

            ' Reset timer stuff in case we were called via a manual refresh
            theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Stopping/Resetting timer."))
            m_Refresh_tmr.Stop()
            m_Refresh_tmr.Enabled = False
            'm_Refresh_tmr.Interval = refreshRate

            If status = 0 Then
                If theHost.InternetAccess Then
                    ' flag that the task has been queued
                    status = -1
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Initiating data scrape."))
                    OpenMobile.Threading.SafeThread.Asynchronous(AddressOf getPrices, theHost)
                    'OpenMobile.Threading.TaskManager.QueueTask(getPrices, OpenMobile.ePriority.Normal, "Update Fuel Prices")
                    Return True
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("No internet connection available."))
                    theHost.execute(eFunction.dataUpdated, "OMDSFuelPrices")
                    Return False
                End If
            Else
                theHost.DebugMsg(String.Format("OMDSFuelPrices - refreshData()"), String.Format("Scrape already in progress."))
                theHost.execute(eFunction.dataUpdated, "OMDSFuelPrices")
                Return True
            End If

        End Function

        Private Function getPrices()

            theHost.DebugMsg(String.Format("OMDSFuelPrices - getPrices()"), String.Format("Attempting scrape."))
            theHost.UIHandler.RemoveAllMyNotifications(Me)

            If getInfo() Then
                scrapeCounter += 1
                theHost.DebugMsg(String.Format("OMDSFuelPrices - getPrices()"), String.Format("Completed scrape #{0}.", scrapeCounter))
                StoredData.Set(Me, "LastUpdate", DateTime.Now.ToString("s"))
                theHost.execute(eFunction.dataUpdated, "OMDSFuelPrices")
            Else
                theHost.DebugMsg(String.Format("OMDSFuelPrices - getPrices()"), String.Format("Scrape failed."))
            End If

            ' Scrape was completed, reset flag
            status = 0

            ' Restart the timer
            theHost.DebugMsg(String.Format("OMDSFuelPrices - getPrices()"), String.Format("Restarting timer."))
            System.Threading.Thread.Sleep(2000)
            theHost.UIHandler.RemoveAllMyNotifications(Me)
            m_Refresh_tmr.Enabled = True
            m_Refresh_tmr.Start()

            Return True

        End Function

        Private Function getInfo() As Boolean

            Dim abc As OpenMobile.Location = theHost.CurrentLocation
            Dim geoLocate As Boolean = False

            ' Is location.zip set?
            If Not String.IsNullOrEmpty(abc.Zip) Then
                searchZip = abc.Zip
            Else
                geoLocate = True
            End If

            ' Is location.zip set?
            If Not String.IsNullOrEmpty(abc.Country) Then
                searchCountry = abc.Country
            Else
                geoLocate = True
            End If

            ' Is location.zip set?
            If Not String.IsNullOrEmpty(abc.State) Then
                searchState = abc.State
            Else
                geoLocate = True
            End If

            ' Is location.zip set?
            If Not String.IsNullOrEmpty(abc.City) Then
                searchCity = abc.City
            Else
                geoLocate = True
            End If

            If Not searchCurrent Then
                ' We want to search the default location
                If Not String.IsNullOrEmpty(StoredData.Get(Me, "Data.DefaultCountry")) Then
                    searchCountry = StoredData.Get(Me, "Data.DefaultCountry")
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("No default country set."))
                End If
                If Not String.IsNullOrEmpty(StoredData.Get(Me, "Data.DefaultZip")) Then
                    ' We have a default location city
                    searchZip = StoredData.Get(Me, "Data.DefaultZip")
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("No default Zip set."))
                End If
                If Not String.IsNullOrEmpty(StoredData.Get(Me, "Data.DefaultState")) Then
                    ' We have a default state
                    searchState = StoredData.Get(Me, "Data.DefaultState")
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("No default state/prov set."))
                End If
                If Not String.IsNullOrEmpty(StoredData.Get(Me, "Data.DefaultCity")) Then
                    ' We have a default location city
                    searchCity = StoredData.Get(Me, "Data.DefaultCity")
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("No default city set."))
                End If
            Else
                If geoLocate Then
                    locate_US()
                End If
            End If

            Select Case searchCountry

                Case "US" ' United States
                    If String.IsNullOrEmpty(searchZip) Then
                        theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("Insufficient data to perform search."))
                        Return False
                    End If
                    searchParam = "http://www.gasbuddy.com/findsite.asp?zip=" & searchZip
                Case "CA" ' Canada
                    If String.IsNullOrEmpty(searchState) Or String.IsNullOrEmpty(searchCity) Then
                        ' cannot search without info
                        theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("Insufficient data to perform search."))
                        Return False
                    End If
                    searchParam = "http://www.gasbuddy.com/GasPriceSearch.aspx?typ=adv&fuel=" & searchGrade & "&srch=1&state=" & searchState & "&area=" & searchCity & "&tme_limit=24"
                Case Else ' Unknown country
                    Return False

            End Select

            scrape_gasbuddy()

            'Dim ddata As Object = scrape_motortrend(zip)

            Return True

        End Function

        Private Sub locate_US()

            Dim loc As OpenMobile.Location = OM.Host.CurrentLocation

            myLatitude = loc.Latitude.ToString
            myLongitude = loc.Longitude.ToString

            ' for testing when I don't have a GPS
            'If myLatitude = "0" Or myLongitude = "0" Then
            'myLatitude = "43.0842229"
            'myLongitude = "-79.0915813"
            'End If

            ' Sometimes internet connection is not detected before initialization is complete
            'System.Threading.Thread.Sleep(5000)

            If ((myLatitude <> "0") And (myLongitude <> "0")) Then
                theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), String.Format("System says:  {0}  {1}.", loc.Latitude, loc.Longitude))
                If (theHost.InternetAccess) Then
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), "Finding ZIP using GPS...")
                    'use geonames website
                    Try
                        Dim html As String = ""
                        Dim wc As WebClient = New WebClient
                        html = wc.DownloadString("http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat=" + loc.Latitude.ToString() + "&lng=" + loc.Longitude.ToString())
                        wc.Dispose()
                        If html.Contains("postalCodes") Then
                            html = html.Remove(0, html.IndexOf("postalCode") + 11)
                            searchZip = html.Remove(0, html.IndexOf("postalCode") + 11)
                            searchZip = searchZip.Remove(0, searchZip.IndexOf("""") + 1)
                            searchZip = searchZip.Substring(0, searchZip.IndexOf(""""))
                            searchCountry = html.Remove(0, html.IndexOf("countryCode") + 12)
                            searchCountry = searchCountry.Remove(0, searchCountry.IndexOf("""") + 1)
                            searchCountry = searchCountry.Substring(0, searchCountry.IndexOf(""""))
                            searchCity = html.Remove(0, html.IndexOf("placeName") + 10)
                            searchCity = searchCity.Remove(0, searchCity.IndexOf("""") + 1)
                            searchCity = searchCity.Substring(0, searchCity.IndexOf(""""))
                            strip_city()
                            searchState = html.Remove(0, html.IndexOf("adminCode1") + 11)
                            searchState = searchState.Remove(0, searchState.IndexOf("""") + 1)
                            searchState = searchState.Substring(0, searchState.IndexOf(""""))
                            theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), String.Format("Found: {0}, {1}, {2}, {3}", searchCity, searchState, searchCountry, searchZip))
                        Else
                            theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), "Could not validate.")
                        End If

                    Catch

                    End Try
                Else
                    theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), "No Internet access.")
                End If
            Else
                theHost.DebugMsg(String.Format("OMDSFuelPrices - locate_US()"), "No GPS coordinates provided.")
            End If

        End Sub

        Private Sub strip_city()

            searchCity = searchCity.Replace(" Central", "")
            searchCity = searchCity.Replace(" Southeast", "")

            For x = Len(searchCity) To 0 Step -1

            Next

        End Sub

        Private Function scrape_gasbuddy() As Object

            Dim client As WebClient = New WebClient
            Dim data As String = ""

            Try
                'theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), String.Format("Searching location: {0}.", zip))
                data = client.DownloadString(searchParam)

            Catch ex As Exception
                theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_gasbuddy()"), String.Format("Exception: {0}.", ex.Message))
                Return False

            End Try

            ' Find the other fuel grade URLs <ul class="p_ft">
            ' ctl00_Content_P_hlMidgrade
            Dim x As Integer = data.IndexOf("ctl00_Content_P_hlMidgrade")
            If x < 0 Then
                ' No URL found
                Return False
            End If

            ' grab the URL
            Dim y As Integer = data.IndexOf(""">", x)
            Dim URL_Midgrade = data.Substring(x + 8, y - (x + 8))
            'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Regular: {0}", URL_Midgrade))

            ' ctl00_Content_P_hlPremium then
            x = data.IndexOf("ctl00_Content_P_hlPremium")
            If x < 0 Then
                ' No URL found
            End If
            ' grab the URL
            y = data.IndexOf(""">", x)
            Dim URL_Premium = data.Substring(x + 8, y - (x + 8))
            'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Premium: {0}", URL_Premium))

            ' ctl00_Content_P_hlDiesel
            x = data.IndexOf("ctl00_Content_P_hlDiesel")
            If x < 0 Then
                ' No URL found
            End If
            ' grab the URL
            y = data.IndexOf(""">", x)
            Dim URL_Diesel = data.Substring(x + 8, y - (x + 8))
            'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("URL_Diesel: {0}", URL_Diesel))

            ' Start loop for the fuel grade pages

            ' Find the start of the price table
            x = data.IndexOf("<div id=""pp_table"">")
            If x < 0 Then
                ' Did not find what we need
                'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", "Did not find a data table")
                Return False
            End If

            Dim row_counter As Integer = 0
            Dim z As Integer = 0
            Dim w As String = ""
            Dim price As String = ""
            Dim name As String = ""
            Dim address As String = ""
            Dim area As String = ""
            Dim dt As String = ""
            Dim grade As String = "Regular"
            Dim record As String = ""
            Dim result As Boolean = False
            ReDim PriceList(5, 0)

            Do While True

                ' Set grade here

                ' Loop here
                Do While True
                    ' Find the start of row entry
                    w = "<tr id=""rrlow_" & row_counter.ToString
                    x = data.IndexOf(w, x)
                    If x < 0 Then
                        ' No Gas data or table not found
                        Exit Do
                    End If
                    'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Found record: {0}", w))
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
                    'theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", String.Format("Found price: {0}", price))

                    x = data.IndexOf("<dl class=""address"">", z)
                    x = data.IndexOf("index.aspx"">", x)
                    z = data.IndexOf("</a>", x)
                    name = data.Substring(x + 12, z - (x + 12))
                    name = name.Replace("&amp;", "&")
                    name = name.Replace("'", "")

                    x = data.IndexOf("<dd>", z)
                    z = data.IndexOf("</dd>", x)
                    address = data.Substring(x + 4, z - (x + 4))
                    address = address.Replace("&amp;", "&")

                    x = data.IndexOf("class=""p_area"">", z)
                    z = data.IndexOf("</a>", x)
                    area = data.Substring(x + 15, z - (x + 15))

                    x = data.IndexOf("title=""", z)
                    z = data.IndexOf(""">", x)
                    dt = data.Substring(x + 7, z - (x + 7))

                    ReDim Preserve PriceList(5, row_counter)
                    PriceList(0, row_counter) = grade
                    PriceList(1, row_counter) = price
                    PriceList(2, row_counter) = name
                    PriceList(3, row_counter) = address
                    PriceList(4, row_counter) = area
                    PriceList(5, row_counter) = dt

                    record = "|" & grade & "|" & price & "|" & name & "|" & address & "|" & area & "|" & dt & "|"

                    theHost.DebugMsg("OMDSFuelPrices - scrape_gasbuddy()", record)

                    result = Push_Data_Source(record, row_counter)

                    row_counter += 1

                    If row_counter > maxRecords - 1 Then
                        Exit Do
                    End If

                    x = z

                Loop

                theHost.DataHandler.PushDataSourceValue(Me.pluginName & ";Fuel.Price.Count", row_counter)

                Exit Do

                ' Call for the next grade data

                ' loop around to process the next fuel grade page

            Loop

            Return True

        End Function

        Private Function Push_Data_Source(ByVal record As String, ByVal index As Integer) As Boolean

            ' This works in OMLCD so why not here?
            '  Now added pluginName to the parameter
            'Dim sourceGroup As String = Me.pluginName & ";Fuel.Prices"

            Dim sourceName As String = Me.pluginName & ";Fuel.Prices." & index.ToString
            Dim sourceDesc As String = "Fuel Price #" & index.ToString
            Dim sourceValue As String = ""

            Try
                sourceValue = theHost.DataHandler.GetDataSource(sourceName).Value
                theHost.DataHandler.PushDataSourceValue(sourceName, record)
            Catch ex As Exception
                theHost.DataHandler.AddDataSource(New DataSource(Me.pluginName, "Fuel", "Prices", index.ToString, DataSource.DataTypes.text, sourceDesc, record))
                theHost.DataHandler.PushDataSourceValue(sourceName, record)
            End Try

            'theHost.DebugMsg(String.Format("OMDSFuelPrices - Push_Data_Source()"), record)

            Return True

        End Function

        Private Function scrape_motortrend(ByVal zip As String) As Object

            Dim client As WebClient = New WebClient
            Dim data As String = ""
            Try

                theHost.DebugMsg(String.Format("OMDSFuelPrices - scrape_mototrend()"), String.Format("Searching location: {0}.", zip))
                data = client.DownloadString("http://www.motortrend.com/gas_prices/34/" + zip + "/index.html")

            Catch ex As Exception
                theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("Exception: {0}.", ex.Message))
                Return False
            End Try

            Dim x As Integer = data.IndexOf("w800 brdr1 gp_data_tbl")
            If x < 0 Then
                Return False
            End If

            data = data.Substring(x)
            x = data.IndexOf("b clr20'>") + 9
            Dim xend As Integer = data.IndexOf("</table>")
            Dim y, z As Integer

            Dim stations As GasStations = New GasStations

            If Not stations.beginWrite() Then
                Return False
            End If

            Dim station As GasStations.gasStation = New GasStations.gasStation

            Do While x >= 0

                station.postalCode = zip
                y = data.IndexOf("<", x)
                station.name = data.Substring(x, y - x).Trim()
                x = data.IndexOf("pad brdr1_b"">", x) + 13
                If (x = 12) Then
                    Return True
                End If
                y = data.IndexOf("</", x)
                station.location = data.Substring(x, y - x).Trim().Replace("<br>", "\n")
                y = data.IndexOf("'lft sub' >", x) + 11
                z = data.IndexOf("pad brdr1_b"">", x) + 13
                If ((y > z) Or (y = 10)) Then
                    x = z
                Else
                    x = y
                End If
                y = data.IndexOf("<", x)
                station.priceRegular = data.Substring(x, y - x).Trim()
                y = data.IndexOf("'lft sub' >", x) + 11
                z = data.IndexOf("pad brdr1_b"">", x) + 13
                If ((y > z) Or (y = 10)) Then
                    x = z
                Else
                    x = y
                End If
                y = data.IndexOf("<", x)
                station.pricePlus = data.Substring(x, y - x).Trim()
                y = data.IndexOf("'lft sub' >", x) + 11
                z = data.IndexOf("pad brdr1_b"">", x) + 13
                If ((y > z) Or (y = 10)) Then
                    x = z
                Else
                    x = y
                End If
                y = data.IndexOf("<", x)
                station.pricePremium = data.Substring(x, y - x).Trim()
                y = data.IndexOf("'lft sub' >", x) + 11
                z = data.IndexOf("pad brdr1_b"">", x) + 13
                If ((y > z) Or (y = 10)) Then
                    x = z
                Else
                    x = y
                End If
                y = data.IndexOf("<", x)
                station.priceDiesel = data.Substring(x, y - x)
                station.dateAdded = DateTime.Now
                If (station.priceRegular = "N/A") Then
                    station.priceRegular = ""
                End If
                If (station.pricePlus = "N/A") Then
                    station.pricePlus = ""
                End If
                If (station.pricePremium = "N/A") Then
                    station.pricePremium = ""
                End If
                If (station.priceDiesel = "N/A") Then
                    station.priceDiesel = ""
                End If

                stations.writeNext(station)

                theHost.DebugMsg(String.Format("OMDSFuelPrices - getInfo()"), String.Format("Found: {0} {1} {2} {3} {4}.", station.name, station.location, station.priceRegular, station.pricePremium, station.priceDiesel))

                x = data.IndexOf("b clr20'>", x) + 9

            Loop

            Return True

        End Function

        Public Function dataSource_Get_Count()

            Dim ret As Integer
            ret = PriceList.GetLength(1) - 1
            Return ret

        End Function

        Private Sub m_Refresh_tmr_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_Refresh_tmr.Elapsed

            ' do a data refresh
            theHost.DebugMsg("OMDSFuelPrices - m_Refresh_tmr_Elapsed()", "Timer elapsed.  Time to scrape.")
            refreshData()

        End Sub

        Public Overrides Function loadSettings() As OpenMobile.Plugin.Settings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            ' Create a handler for settings changes
            AddHandler mySettings.OnSettingChanged, AddressOf mySettings_OnSettingChanged

            Return mySettings

        End Function

        Private Sub mySettings_OnSettingChanged(ByVal screen As Integer, ByVal settings As Setting)
            ' Setting was changed, update database
        End Sub

    End Class

End Namespace