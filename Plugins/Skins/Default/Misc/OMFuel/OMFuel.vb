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

    Public Class OMFuel
        Implements IHighLevel

        Private manager As ScreenManager
        Private WithEvents theHost As IPluginHost

        Public Function initialize(ByVal host As OpenMobile.Plugin.IPluginHost) As OpenMobile.eLoadStatus Implements OpenMobile.Plugin.IBasePlugin.initialize

            theHost = host
            manager = New ScreenManager(Me)

            theHost.DataHandler.SubscribeToDataSource("OMDSFuelPrices;Fuel.Price.Count", AddressOf Subscription_Updated)

            Dim omFuelpanel As New OMPanel("FuelPrices", "Fuel Prices", Me.pluginIcon)
            Dim settings As New Settings("Fuel Price Settings")
            Dim pluginSettings As New PluginSettings()
            AddHandler omFuelpanel.Entering, AddressOf panel_enter

            Dim mainContainer = New OMContainer("mainContainer", theHost.ClientArea(0).Left, theHost.ClientArea(0).Top, theHost.ClientArea(0).Width, theHost.ClientArea(0).Height)
            mainContainer.Image = OM.Host.getSkinImage("MediaBorder")
            mainContainer.ScrollBar_ColorNormal = Color.Transparent
            mainContainer.BackgroundColor = Color.Transparent
            mainContainer.Opacity = 0
            omFuelPanel.addControl(mainContainer)

            manager.loadPanel(omFuelpanel, True)

            Return eLoadStatus.LoadSuccessful

        End Function

        Private Sub Subscription_Updated(ByVal sensor As OpenMobile.Data.DataSource)

            theHost.DebugMsg("OMFuel - Subscription_Updated()", "Datasource updated.")

            If sensor.Value < 0 Then
                ' There are no entries to process
                Exit Sub
            End If

            ' This is here for testing to prevent the plugin fail
            'Exit Sub

            If manager Is Nothing Then
                Exit Sub
            End If

            ' How do I refresh the panel
            ' How about creating/destroying subscriptions as required?

            Dim mPanel As OMPanel

            For x = 0 To theHost.ScreenCount - 1
                mPanel = manager(x, "FuelPrices")
                If Not mPanel Is Nothing Then
                    If mPanel.IsLoaded(x) Then
                        panel_enter(mPanel, x)
                        'mPanel.Refresh()
                    End If
                End If
            Next

        End Sub

        Public Function loadSettings() As OpenMobile.Plugin.Settings Implements OpenMobile.Plugin.IBasePlugin.loadSettings

            Dim mySettings As New Settings(Me.pluginName & " Settings")

            Return mySettings

        End Function

        Public Function loadPanel(ByVal name As String, ByVal screen As Integer) As OpenMobile.Controls.OMPanel Implements OpenMobile.Plugin.IHighLevel.loadPanel

            If manager Is Nothing Then
                Return Nothing
            End If

            Return manager(screen, name)

        End Function

        Public Sub panel_enter(ByVal sender As OMPanel, ByVal screen As Integer)

            'Dim theContainer As OMContainer = sender.Controls("mainContainer")
            Dim theContainer As OMContainer = sender(screen, "mainContainer")

            theContainer.ClearControls()

            Dim entryCount As Integer = theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Price.Count").Value

            If entryCount < 0 Then
                ' There are no entries to display
                Exit Sub
            End If

            Dim record() As String
            Dim dummyRecord As String = "|None|0.00|Unknown|empty|empty|empty|"
            Dim temp_string As String = ""
            Dim x_coord As Integer = 10
            Dim y_coord As Integer = 15

            For x = 0 To entryCount - 1

                temp_string = theHost.DataHandler.GetDataSource("OMDSFuelPrices;Fuel.Prices." & x.ToString).Value
                If temp_string Is Nothing Then
                    record = dummyRecord.Split("|")
                Else
                    record = temp_string.Split("|")
                End If

                Dim shpPriceBackground As New OMBasicShape("shpPriceBackground" & x.ToString, x_coord, y_coord, 180, 470, New ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 5))

                Dim entryImage = New OMImage("entryImage" & x.ToString, shpPriceBackground.Left + 5, shpPriceBackground.Top + 5, shpPriceBackground.Width - 10, shpPriceBackground.Width - 10)
                entryImage.Image = theHost.getPluginImage(Me, "Logos|" & record(3))
                entryImage.NullImage = theHost.getPluginImage(Me, "Logos|Unknown")

                Dim entryGrade = New OMLabel("entryGrade" & x.ToString, shpPriceBackground.Left + 5, shpPriceBackground.Top + 15 + entryImage.Height + 5, shpPriceBackground.Width - 10, 20)
                entryGrade.AutoFitTextMode = FitModes.Fit
                entryGrade.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                entryGrade.FontSize = 20
                entryGrade.Text = record(1)

                Dim entryPrice = New OMLabel("entryPrice" & x.ToString, shpPriceBackground.Left + 5, entryGrade.Top + entryGrade.Height + 5, shpPriceBackground.Width - 10, 35)
                entryPrice.AutoFitTextMode = FitModes.None
                entryPrice.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                entryPrice.FontSize = 26
                entryPrice.Text = FormatCurrency(Double.Parse(record(2), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture), 2)

                Dim entryName = New OMLabel("entryName" & x.ToString, shpPriceBackground.Left + 5, entryPrice.Top + entryPrice.Height + 15, shpPriceBackground.Width - 10, 35)
                entryName.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                entryName.AutoFitTextMode = FitModes.Fit
                entryName.FontSize = 20
                entryName.Text = record(3)

                Dim entryAddress = New OMLabel("entryAddress" & x.ToString, shpPriceBackground.Left + 5, entryName.Top + entryName.Height + 20, shpPriceBackground.Width - 10, 65)
                entryAddress.TextAlignment = (Alignment.WordWrap And Alignment.TopRight)
                entryAddress.AutoFitTextMode = FitModes.Fit
                entryAddress.FontSize = 20
                entryAddress.Text = record(4)

                Dim entryArea = New OMLabel("entryArea" & x.ToString, shpPriceBackground.Left + 5, entryAddress.Top + entryAddress.Height + 15, shpPriceBackground.Width - 10, 25)
                entryArea.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                entryArea.AutoFitTextMode = FitModes.Fit
                entryArea.FontSize = 20
                entryArea.Text = record(5)

                Dim entryWhen = New OMLabel("entryWhen" & x.ToString, shpPriceBackground.Left + 5, entryArea.Top + entryArea.Height + 15, shpPriceBackground.Width - 10, 30)
                entryWhen.TextAlignment = (Alignment.WordWrap And Alignment.CenterCenter)
                entryWhen.AutoFitTextMode = FitModes.Fit
                entryWhen.FontSize = 20
                entryWhen.Text = record(6)

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

        End Sub

        Public ReadOnly Property authorEmail As String Implements OpenMobile.Plugin.IBasePlugin.authorEmail
            Get
                Return "jmullan99@gmail.com"
            End Get
        End Property

        Public ReadOnly Property authorName As String Implements OpenMobile.Plugin.IBasePlugin.authorName
            Get
                Return "John Mullan"
            End Get
        End Property

        Public Function incomingMessage(ByVal message As String, ByVal source As String) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage
            Return False
        End Function

        Public Function incomingMessage1(Of T)(ByVal message As String, ByVal source As String, ByRef data As T) As Boolean Implements OpenMobile.Plugin.IBasePlugin.incomingMessage
            Return False
        End Function

        Public ReadOnly Property pluginDescription As String Implements OpenMobile.Plugin.IBasePlugin.pluginDescription
            Get
                Return "Display local fuel prices"
            End Get
        End Property

        Public ReadOnly Property pluginIcon As OpenMobile.imageItem Implements OpenMobile.Plugin.IBasePlugin.pluginIcon
            Get
                Return OM.Host.getSkinImage("Icons|Icon-Gas")
            End Get
        End Property

        Public ReadOnly Property pluginName As String Implements OpenMobile.Plugin.IBasePlugin.pluginName
            Get
                Return "OMFuel"
            End Get
        End Property

        Public ReadOnly Property pluginVersion As Single Implements OpenMobile.Plugin.IBasePlugin.pluginVersion
            Get
                Return 0.1
            End Get
        End Property

        Public ReadOnly Property displayName As String Implements OpenMobile.Plugin.IHighLevel.displayName
            Get
                Return "Fuel Prices"
            End Get
        End Property

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace

