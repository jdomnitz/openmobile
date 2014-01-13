using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;

namespace OMGPS2.GEOCoding
{
    class GeoCoding
    {
        private class DownloadItem
        {
            public string Name { get; set; }
            public string NameIsoCode { get; set; }
            public string Url { get; set; }
            public DateTime Timestamp_Current { get; set; }
            public string Size { get; set; }
            public bool Active { get; set; }

            /// <summary>
            /// Gets or sets the amount of downloaded bytes for this item
            /// </summary>
            public long DownloadFile_CurrentBytes { get; set; }
            public long DownloadFile_TotalBytes { get; set; }
            public int DownloadFile_Progress { get; set; }
            public string DownloadFile_Location { get; set; }
            public DateTime DownloadFile_Timestamp { get; set; }

            public DownloadItem()
            {
            }
            public DownloadItem(string name, string nameISOCode, string url, DateTime timestamp, string size)
            {
                Name = name;
                NameIsoCode = nameISOCode;
                Url = url;
                Timestamp_Current = timestamp;
                Size = size;
            }

            public bool IsDownloadCompleted()
            {
                return DownloadFile_CurrentBytes == DownloadFile_TotalBytes;
            }

            public override string ToString()
            {
                return String.Format("{0} {1} {2}", Name, Timestamp_Current, Size);
            }
        }

        private OMGPS2 _BasePlugin = null;
        private List<DownloadItem> _DownloadItems = new List<DownloadItem>();
        private bool _CancelActions = false;
        private Notification _DownloadNotification;
        
        /// <summary>
        /// Global data from DB: GEO Names download timestamp
        /// </summary>
        const string _GlobalData_GEONamesDLTS = "GEONames.DownloadTimestamp";

        public GeoCoding(OMGPS2 basePlugin)
        {
            // Save a referance to the original plugin
            _BasePlugin = basePlugin;

            // Init DB
            DB_Init();

            // Queue panels
            _BasePlugin.PanelManager.QueuePanel("GEOCodingPanel", InitializeGEOCodingPanel);

            // Connect to system events
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);

            // Load commands
            Commands_Register();
            DataSources_Register();
        }

        private void Commands_Register()
        {
            OM.Host.CommandHandler.AddCommand(new Command(_BasePlugin.pluginName, "GPS", "Location", "ReverseGeocode", ReverseGeocode, 0, false, "Reverse Gecodes Current Lat/Long"));
        }

        private void DataSources_Register()
        {
            OM.Host.DataHandler.AddDataSource(new OpenMobile.Data.DataSource(_BasePlugin.pluginName, "GPS", "ReverseGeocode", "StatusMessage", OpenMobile.Data.DataSource.DataTypes.raw, "GPS status message of reverse geocoding"), "");
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            switch (function)
            {
                case eFunction.connectedToInternet:
                    {
                        // Load data from DB
                        _DownloadItems = DB_LoadDownloadItems();

                        // Update from the web?
                        DateTime lastDownloadTS = DB_GetGlobalData<DateTime>(_GlobalData_GEONamesDLTS);
                        if ((DateTime.Now - lastDownloadTS).TotalDays >= 7)
                            // Yes download new data
                            DownloadList_LoadFromWeb();
                    }
                    break;

                case eFunction.pluginLoadingComplete:
                    {
                        //OM.Host.CommandHandler.ExecuteCommand("GPS.Location.ReverseGeocode");
                        //Location loc = (Location)OM.Host.CommandHandler.ExecuteCommand("GPS.Location.ReverseGeocode", 42.07178f, -80.11587f);
                    }
                    break;
            }
        }

        private OMPanel InitializeGEOCodingPanel()
        {
            //Create the panel
            OMPanel panel = new OMPanel("GEOCodingPanel");

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 20, 900, OM.Host.ClientArea_Init.Height - 30,
                new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackground.Left = OM.Host.ClientArea_Init.Center.X - shapeBackground.Region.Center.X;
            panel.addControl(shapeBackground);

            OMLabel lblHeader = new OMLabel("lblHeader", shapeBackground.Region.Left + 10, shapeBackground.Region.Top + 5, shapeBackground.Region.Width - 20, 30);
            lblHeader.Text = "Select the countries you'd like to download data for:";
            panel.addControl(lblHeader);

            OMBasicShape shpHeaderLine = new OMBasicShape("shpHeaderLine", shapeBackground.Region.Left, lblHeader.Region.Bottom, shapeBackground.Region.Width, 1,
            new ShapeData(shapes.Rectangle)
            {
                GradientData = GradientData.CreateHorizontalGradient(
                    new GradientData.ColorPoint(0.0, 0, Color.Empty),
                    new GradientData.ColorPoint(0.5, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(1.0, 0, Color.FromArgb(128, BuiltInComponents.SystemSettings.SkinFocusColor)),
                    new GradientData.ColorPoint(0.5, 0, Color.Empty))
            });
            panel.addControl(shpHeaderLine);

            OMObjectList lstCountryList = new OMObjectList("lstCountryList", shapeBackground.Region.Left + 10, lblHeader.Region.Bottom + 5, shapeBackground.Region.Width - 20, shapeBackground.Region.Height - 60);
            panel.addControl(lstCountryList);

            OMObjectList.ListItem ItemBase = new OMObjectList.ListItem();
            OMCheckbox chkItem = new OMCheckbox("chkItem", 0, 0, lstCountryList.Region.Width, 40);
            ItemBase.Add(chkItem);

            ItemBase.Action_SetItemInfo = (OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values) =>
                {
                    DownloadItem downloadItem = values[0] as DownloadItem;

                    OMCheckbox itemBase_chkItem = item["chkItem"] as OMCheckbox;
                    itemBase_chkItem.Tag = downloadItem;
                    itemBase_chkItem.Text = String.Format("{0} [{2}]", downloadItem.Name, downloadItem.Timestamp_Current, downloadItem.Size);
                    itemBase_chkItem.Checked = downloadItem.Active;
                    itemBase_chkItem.OnClick += new userInteraction(itemBase_chkItem_OnClick);
                };
            lstCountryList.ItemBase = ItemBase;

            OMButton btnOK = OMButton.PreConfigLayout_BasicStyle("btnOK", shapeBackground.Region.Right - 120, shapeBackground.Region.Bottom - 70, 90, 60, GraphicCorners.Right, "", "Ok", 22);
            btnOK.OnClick += new userInteraction(btnOK_OnClick);
            panel.addControl(btnOK);
            OMButton btnCancel = OMButton.PreConfigLayout_BasicStyle("btnCancel", 0, 0, 90, 60, GraphicCorners.Left, "", "Cancel", 22);
            btnCancel.OnClick += new userInteraction(btnCancel_OnClick);
            panel.addControl(btnCancel, ControlDirections.Left);

            panel.Loaded += new PanelEvent(panel_Loaded);

            return panel;
        }

        void panel_Loaded(OMPanel sender, int screen)
        {
            _DownloadItems = DB_LoadDownloadItems();
            DownloadList_UpdateGUI(sender, screen);
        }

        void btnCancel_OnClick(OMControl sender, int screen)
        {   // Cancel changes
            _BasePlugin.GoBack(screen);
        }

        void btnOK_OnClick(OMControl sender, int screen)
        {
            // Save data to DB
            DB_SaveDownloadItems(_DownloadItems);

            // Spawn new thread to download and process data
            OpenMobile.Threading.SafeThread.Asynchronous(() =>
                {
                    DownloadItem_DoDownloadsAndProcessing();
                });


            _BasePlugin.GoBack(screen);
        }

        void itemBase_chkItem_OnClick(OMControl sender, int screen)
        {
            OMCheckbox chkItem = sender as OMCheckbox;
            DownloadItem item = chkItem.Tag as DownloadItem;
            if (item == null)
                return;
            item.Active = chkItem.Checked;
        }

        #region DownloadList handling

        private void DownloadList_UpdateGUI(OMPanel panel, int screen)
        {
            OMObjectList lstCountryList = panel[screen, "lstCountryList"] as OMObjectList;
            if (lstCountryList == null)
                return;

            lstCountryList.Clear();
            foreach (var item in _DownloadItems)
                lstCountryList.AddItemFromItemBase(ControlDirections.Down, item);
        }

        private void DownloadList_LoadFromWeb()
        {
            try
            {
                //string htmlGEONamesURL = @"http://download.geonames.org/export/dump/";
                //string htmlGEONames = "";
                //using (System.Net.WebClient wc = new System.Net.WebClient())
                //    htmlGEONames = wc.DownloadString(htmlGEONamesURL);

                string htmlPostalCodes = "";
                string htmlPostalCodesURL = @"http://download.geonames.org/export/zip/";
                using (System.Net.WebClient wc = new System.Net.WebClient())
                    htmlPostalCodes = wc.DownloadString(htmlPostalCodesURL);

                // Extract all location files 
                Regex regexFiles = new Regex(@"(?<=>)...zip", RegexOptions.Compiled);
                MatchCollection matches = regexFiles.Matches(htmlPostalCodes);
                string[] files = new string[matches.Count];
                for (int i = 0; i < files.Length; i++)
                    files[i] = matches[i].Value;

                // Extract all location file stamps
                Regex regexFileData = new Regex(@"(?<=>...zip</a>)(.*)", RegexOptions.Compiled);
                matches = regexFileData.Matches(htmlPostalCodes);
                string[] fileData = new string[matches.Count];
                for (int i = 0; i < fileData.Length; i++)
                    fileData[i] = matches[i].Value.Trim();

                // Extract all countries file stamps
                Regex regexAllCountriesFileData = new Regex(@"(?<=>allCountries.zip</a>)(.*)", RegexOptions.Compiled);
                matches = regexAllCountriesFileData.Matches(htmlPostalCodes);
                string[] AllCountriesFileData = new string[matches.Count];
                for (int i = 0; i < AllCountriesFileData.Length; i++)
                    AllCountriesFileData[i] = matches[i].Value.Trim();

                if (files.Length != fileData.Length)
                {
                    OM.Host.DebugMsg(DebugMessageType.Error, _BasePlugin.pluginName, "Invalid GEONameFiles<>GEONameFileStamps data match! Unable to process data");
                    return;
                }

                // Data fields: CountryName, FileURL, Timestamp, fileSize 
                // Raw data 
                //      Name:           "AD.zip"
                //      Timestamp:      "16-Nov-2013 03:29   63K"
                //                      "16-Nov-2013 03:29  198K"

                // Parse all countries data to download item
                DownloadItem downloadItem = new DownloadItem();
                downloadItem.Name = "Postal Codes (All Countries)";
                downloadItem.NameIsoCode = "All";
                downloadItem.Url = String.Format("{0}allCountries.zip", htmlPostalCodesURL);
                downloadItem.Size = AllCountriesFileData[0].Substring(AllCountriesFileData[0].LastIndexOf(" "), AllCountriesFileData[0].Length - AllCountriesFileData[0].LastIndexOf(" ")).Trim();
                string timestampString = AllCountriesFileData[0].Substring(0, AllCountriesFileData[0].Length - downloadItem.Size.Length).Trim();
                DateTime timestamp = new DateTime();
                DateTime.TryParse(timestampString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out timestamp);
                downloadItem.Timestamp_Current = timestamp;

                DownloadItem existingItem = _DownloadItems.Find(x => x.Name == downloadItem.Name);
                if (existingItem != null)
                {   // Update existing
                    existingItem.Timestamp_Current = downloadItem.Timestamp_Current;
                    existingItem.Size = downloadItem.Size;
                    existingItem.Url = downloadItem.Url;
                }
                else
                {   // Add new
                    _DownloadItems.Add(downloadItem);
                }

                // Parse downloaded text to download items                
                for (int i = 0; i < files.Length; i++)
                {
                    downloadItem = new DownloadItem();
                    
                    // Country name
                    if (files[i].Contains('.'))
                    {
                        downloadItem.NameIsoCode = files[i].Substring(0, files[i].LastIndexOf(".")).Trim();
                        downloadItem.Name = OpenMobile.Framework.Globalization.CountryISOCodeToFullName(downloadItem.NameIsoCode);
                    }

                    // File url
                    downloadItem.Url = String.Format("{0}{1}", htmlPostalCodesURL, files[i]);

                    // File size
                    downloadItem.Size = fileData[i].Substring(fileData[i].LastIndexOf(" "), fileData[i].Length - fileData[i].LastIndexOf(" ")).Trim();

                    // File timestamp
                    timestampString = fileData[i].Substring(0, fileData[i].Length - downloadItem.Size.Length).Trim();
                    DateTime.TryParse(timestampString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out timestamp);
                    downloadItem.Timestamp_Current = timestamp;

                    // Add new or update existing item?
                    existingItem = _DownloadItems.Find(x=>x.Name == downloadItem.Name);
                    if (existingItem != null)
                    {   // Update existing
                        existingItem.Timestamp_Current = downloadItem.Timestamp_Current;
                        existingItem.Size = downloadItem.Size;
                        existingItem.Url = downloadItem.Url;
                    }
                    else
                    {   // Add new
                        _DownloadItems.Add(downloadItem);
                    }
                }
                DB_SaveDownloadItems(_DownloadItems);

                // Set timestamp for last download (to prevent another download to early)
                DB_SetGlobalData(_GlobalData_GEONamesDLTS, DateTime.Now);
            }
            catch
            {
            }
        }

        private bool DB_UpdateDownloadItem(DownloadItem downloadItem)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SqliteCommand cmd = new SqliteCommand(_DBConnection))
                {
                    cmd.CommandText = "UPDATE tblDownloadItems SET NameISOCode=@NameISOCode, Url=@Url, Timestamp_Current=@Timestamp_Current, Timestamp_Download=@Timestamp_Download, Size=@Size, Active=@Active, DownloadFile_Progress=@DownloadFile_Progress, DownloadFile_Location=@DownloadFile_Location WHERE Name=@Name";
                    cmd.Parameters.AddWithValue("@Name", downloadItem.Name);
                    cmd.Parameters.AddWithValue("@NameISOCode", downloadItem.NameIsoCode);
                    cmd.Parameters.AddWithValue("@Url", downloadItem.Url);
                    cmd.Parameters.AddWithValue("@Timestamp_Current", downloadItem.Timestamp_Current);
                    cmd.Parameters.AddWithValue("@Timestamp_Download", downloadItem.DownloadFile_Timestamp);
                    cmd.Parameters.AddWithValue("@Size", downloadItem.Size);
                    cmd.Parameters.AddWithValue("@Active", downloadItem.Active);
                    cmd.Parameters.AddWithValue("@DownloadFile_Progress", downloadItem.DownloadFile_Progress);
                    cmd.Parameters.AddWithValue("@DownloadFile_Location", downloadItem.DownloadFile_Location);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool DB_SaveDownloadItems(List<DownloadItem> downloadItems)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SqliteCommand cmd = new SqliteCommand(_DBConnection))
                {
                    // Clear current data
                    cmd.CommandText = "DELETE FROM tblDownloadItems";
                    cmd.ExecuteNonQuery();

                    // Update items
                    foreach (var downloadItem in downloadItems)
                    {
                        cmd.CommandText = "INSERT OR REPLACE INTO tblDownloadItems (Name, NameISOCode, Url, Timestamp_Current, Timestamp_Download, Size, Active, DownloadFile_Progress, DownloadFile_Location) VALUES (@Name, @NameISOCode, @Url, @Timestamp_Current, @Timestamp_Download, @Size, @Active, @DownloadFile_Progress, @DownloadFile_Location)";
                        cmd.Parameters.AddWithValue("@Name", downloadItem.Name);
                        cmd.Parameters.AddWithValue("@NameISOCode", downloadItem.NameIsoCode);
                        cmd.Parameters.AddWithValue("@Url", downloadItem.Url);
                        cmd.Parameters.AddWithValue("@Timestamp_Current", downloadItem.Timestamp_Current);
                        cmd.Parameters.AddWithValue("@Timestamp_Download", downloadItem.DownloadFile_Timestamp);
                        cmd.Parameters.AddWithValue("@Size", downloadItem.Size);
                        cmd.Parameters.AddWithValue("@Active", downloadItem.Active);
                        cmd.Parameters.AddWithValue("@DownloadFile_Progress", downloadItem.DownloadFile_Progress);
                        cmd.Parameters.AddWithValue("@DownloadFile_Location", downloadItem.DownloadFile_Location);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private List<DownloadItem> DB_LoadDownloadItems()
        {
            if (!DB_ConnectAndOpen())
                return new List<DownloadItem>();

            try
            {
                List<DownloadItem> items = new List<DownloadItem>();
                using (SqliteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT Name, NameISOCode, Url, Timestamp_Current, Timestamp_Download, Size, Active, DownloadFile_Progress, DownloadFile_Location FROM tblDownloadItems";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DownloadItem item = new DownloadItem();
                            if (reader[0] != DBNull.Value)
                                item.Name = reader.GetString(0); // Name
                            if (reader[1] != DBNull.Value)
                                item.NameIsoCode = reader.GetString(1); // NameISOCode
                            if (reader[2] != DBNull.Value)
                                item.Url = reader.GetString(2); // URL
                            if (reader[3] != DBNull.Value)
                                item.Timestamp_Current = reader.GetDateTime(3); // Timestamp current
                            if (reader[4] != DBNull.Value)
                                item.DownloadFile_Timestamp = reader.GetDateTime(4); // Timestamp download
                            if (reader[5] != DBNull.Value)
                                item.Size = reader.GetString(5); // Size
                            if (reader[6] != DBNull.Value)
                                item.Active = reader.GetBoolean(6); // Active
                            if (reader[7] != DBNull.Value)
                                item.DownloadFile_Progress = reader.GetInt32(7); // DownloadFile_Progress
                            if (reader[8] != DBNull.Value)
                                item.DownloadFile_Location = reader.GetString(8); // DownloadFile_Location
                            items.Add(item);
                        }
                    }
                }
                return items;
            }
            catch
            {
                return new List<DownloadItem>();
            }
        }


        private void DownloadItem_DoDownloadsAndProcessing()
        {
            // Initialize Notification
            OImage imgDownload = OM.Host.getSkinImage("AIcons|9-av-download").image.Copy();

            // Remove old notifications
            OM.Host.UIHandler.RemoveNotification(_BasePlugin, "GPS_DownloadProgress");
            _DownloadNotification = null;

            // Download items
            _DownloadItems.ForEach((x)=>
                {
                    if (x.Active && (x.DownloadFile_Timestamp != x.Timestamp_Current))
                    {
                        // Show download notification 
                        if (_DownloadNotification == null)
                        {
                            _DownloadNotification = new Notification(_BasePlugin, "GPS_DownloadProgress", imgDownload, imgDownload, String.Format("{0} is downloading data...", _BasePlugin.pluginName), "Establishing connection...");
                            _DownloadNotification.Global = true;
                            OM.Host.UIHandler.AddNotification(_DownloadNotification);
                        }

                        // Do download
                        _DownloadNotification.Header = String.Format("{0} is downloading data...", _BasePlugin.pluginName);
                        DownloadItem_DoDownload(x, ref _CancelActions, ref _DownloadNotification);

                        // Process item
                        _DownloadNotification.Header = String.Format("{0} is processing data...", _BasePlugin.pluginName);
                        DownloadItem_Process(x);
                    }
                });

            // Remove notifications
            OM.Host.UIHandler.RemoveNotification(_BasePlugin, "GPS_DownloadProgress");
           
        }
        private void DownloadItem_DoDownload(DownloadItem item, ref bool cancelDownload, ref Notification notification)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(item.Url));
            FileMode fileMode;

            // Should we resume progress or start a new download file?
            if (item.DownloadFile_CurrentBytes == 0 || (item.DownloadFile_Timestamp != item.Timestamp_Current))
            {   // Start a new download
                fileMode = FileMode.Create;
                // Create a new temporary file location for saving downloads
                item.DownloadFile_Location = System.IO.Path.GetTempFileName();
            }
            else
            {   // Resume a previous download
                fileMode = FileMode.Append;
                myHttpWebRequest.AddRange((int)item.DownloadFile_CurrentBytes);
            }

            // Update notification data
            if (notification != null)
                notification.Text = String.Format("{0} (0%)", item.Name);

            // Download file
            using (FileStream fs = new FileStream(item.DownloadFile_Location, fileMode))
            {
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    Stream receiveStream = myHttpWebResponse.GetResponseStream();
                    item.DownloadFile_TotalBytes = myHttpWebResponse.ContentLength + item.DownloadFile_CurrentBytes;
                    byte[] read = new byte[2048];
                    int count = 0;
                    while ((count = receiveStream.Read(read, 0, read.Length)) > 0 && !cancelDownload)
                    {
                        fs.Write(read, 0, count);
                        item.DownloadFile_CurrentBytes += count;
                        item.DownloadFile_Progress = (int)(((float)item.DownloadFile_CurrentBytes / (float)item.DownloadFile_TotalBytes) * 100f);

                        // Update notification data
                        if (notification != null)
                            notification.Text = String.Format("{0} ({1}%)", item.Name, item.DownloadFile_Progress);
                    }
                }
            }

            // Did we complete the download?
            if (item.IsDownloadCompleted())
            {   // Yes, update timestamp
                item.DownloadFile_Timestamp = item.Timestamp_Current;
            }

            // Save data to DB
            DB_UpdateDownloadItem(item);
        }

        private void DownloadItem_Process(DownloadItem item)
        {
            // Do we have anything to process?
            if (!item.IsDownloadCompleted() || !File.Exists(item.DownloadFile_Location))
                return;

            // Unzip temporary data file
            string tmpTargetFile = OpenMobile.Path.Combine(System.IO.Path.GetTempPath(), String.Format("{0}.dta",item.NameIsoCode));            
            using (ZipInputStream zis = new ZipInputStream(File.OpenRead(item.DownloadFile_Location))) 
            {
                ZipEntry ze;
                try
                {
                    while ((ze = zis.GetNextEntry()) != null)
                    {
                        using (FileStream sw = File.Create(tmpTargetFile))
                        {
                            byte[] read = new byte[2048];
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = zis.Read(data, 0, data.Length);
                                if (size > 0)
                                    sw.Write(data, 0, data.Length);
                                else
                                    break;
                            }
                        }
                    }
                    item.DownloadFile_Location = tmpTargetFile;
                }
                catch 
                {   // Unzipping failed, do cleanup to prepare for a new download
                    item.DownloadFile_Progress = 0;
                    if (File.Exists(item.DownloadFile_Location))
                        File.Delete(item.DownloadFile_Location);
                    if (File.Exists(tmpTargetFile))
                        File.Delete(tmpTargetFile);
                    item.DownloadFile_Location = String.Empty;
                }
            }
        }

        #endregion

        #region DB methods

        private const decimal _DBVersion = 1.0m;

        /// <summary>
        /// The full filepath to the database file
        /// </summary>
        static private string _DBFile = String.Empty;

        /// <summary>
        /// The name of the database file
        /// </summary>
        static private string _DBName = String.Empty;

        /// <summary>
        /// True = The database file is valid
        /// </summary>
        static private bool _DBValid = false;

        /// <summary>
        /// The SQLite database connection object
        /// </summary>
        private SqliteConnection _DBConnection;

        private void DB_Init()
        {
            _DBName = _BasePlugin.pluginName;
            _DBFile = OpenMobile.Path.Combine(OM.Host.DataPath, _DBName);

            // Check for available DB file, if not create it
            if (!File.Exists(_DBFile))
            {   // This is a fresh start, let's create the file
                DB_Create(_DBVersion);
            }

            if (!DB_CheckVersion(_DBVersion))
            {
                _DBValid = false;
                OM.Host.UIHandler.AddNotification(new Notification(Notification.Styles.Warning, _BasePlugin, "DB_Invalid", null, String.Format("Invalid database for {0}", _BasePlugin.pluginName), String.Format("The database file was was recreated! Please check settings/data for {0}.", _BasePlugin.pluginName)));

                // Delete current DB file
                DB_Delete();

                // Recreate DB
                DB_Create(_DBVersion);
            }
        }

        /// <summary>
        /// Creates the database file and it's tables
        /// </summary>
        private void DB_Create(decimal dbVersion)
        {
            if (!DB_ConnectAndOpen())
                return;

            if (!_DBValid)
                return;

            SqliteCommand cmd = new SqliteCommand(
                "BEGIN TRANSACTION;" +
                "CREATE TABLE tblDBInfo (Owner TEXT, Description TEXT, Version NUMERIC);" +
                String.Format("INSERT INTO tblDBInfo VALUES('{0}', '{1}', '{2}');", _BasePlugin.pluginName, _BasePlugin.pluginDescription, dbVersion) +
                "CREATE TABLE tblGlobalData (Name TEXT, Value Text);" +
                "CREATE TABLE tblDownloadItems (Name TEXT, NameISOCode TEXT, Url TEXT, Timestamp_Current DateTime, Timestamp_Download DateTime, Size TEXT, Active BOOL, DownloadFile_Progress INT, DownloadFile_Location TEXT);" +
                "COMMIT;"
                , _DBConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                _DBValid = false;
            }
        }

        /// <summary>
        /// Connects to the database and opens it
        /// </summary>
        private bool DB_ConnectAndOpen()
        {
            try
            {
                if (_DBConnection == null)
                    _DBConnection = new SqliteConnection(String.Format(@"Data Source={0};Pooling=false;synchronous=0;", _DBFile));
                if (_DBConnection.State != ConnectionState.Open)
                {
                    _DBConnection.Open();
                    _DBValid = true;
                }

                if (_DBConnection.State == ConnectionState.Open)
                    _DBValid = true;
            }
            catch
            {
                _DBValid = false;
            }

            return _DBValid;
        }

        /// <summary>
        /// Checks if the required version of the database is correct
        /// </summary>
        /// <param name="minVersion"></param>
        private bool DB_CheckVersion(decimal minVersion)
        {
            if (!DB_ConnectAndOpen())
                return false;

            using (SqliteCommand command = _DBConnection.CreateCommand())
            {
                command.CommandText = "SELECT Version FROM tblDBInfo";
                decimal dec = (decimal)command.ExecuteScalar();
                if (dec >= minVersion)
                    return true;
            }
            return false;
        }

        private SqliteDataReader DB_ExecuteCommand(string SQL)
        {
            SqliteCommand cmd = new SqliteCommand(SQL, _DBConnection);
            try
            {
                return cmd.ExecuteReader();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Closes the DB connection
        /// </summary>
        private void DB_Close()
        {
            _DBConnection.Close();
        }

        /// <summary>
        /// Deletes the DB file
        /// </summary>
        private void DB_Delete()
        {
            DB_Close();
            try
            {
                File.Delete(_DBFile);
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg(String.Format("Unable to delete database file {0}", _DBFile), ex);
            }
        }

        private bool DB_SetGlobalData(string name, object value)
        {
            if (!DB_ConnectAndOpen())
                return false;

            try
            {
                using (SqliteCommand cmd = new SqliteCommand(_DBConnection))
                {
                    cmd.CommandText = "INSERT INTO tblGlobalData (Name, Value) VALUES (@Name, @Value)";
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private T DB_GetGlobalData<T>(string name)
        {
            if (!DB_ConnectAndOpen())
                return default(T);

            try
            {
                using (SqliteCommand command = _DBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT Value FROM tblGlobalData WHERE Name =@Name";
                    command.Parameters.AddWithValue("@Name", name);
                    object value = command.ExecuteScalar();
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

        #region Reverse Geocode

        private object ReverseGeocode(OpenMobile.Command command, object[] param, out bool result)
        {
            // Default return data
            result = true;

            // We update the current location if no parameters was sent along the command
            bool updateCurrentLocation = true;

            // Get source data
            float latitude = OM.Host.CurrentLocation.Latitude;
            float longitude = OM.Host.CurrentLocation.Longitude;
            
            // Do we have any location as input parameters?
            if (param != null && param.Length >= 2)
            {   // First parameter is latitude, second is longitude
                latitude = (float)Convert.ToDouble(param[0]);
                longitude = (float)Convert.ToDouble(param[1]);
                updateCurrentLocation = false;
            }

            // Do we have valid data?
            if (latitude == 0 || longitude == 0)
            {
                OM.Host.DataHandler.PushDataSourceValue(_BasePlugin.pluginName, "GPS.ReverseGeocode.StatusMessage", "Missing Latitude/Longitude data");
                return null;
            }

            // Cancel request if we have no internet connection
            if (!OM.Host.InternetAccess)
            {
                OM.Host.DataHandler.PushDataSourceValue(_BasePlugin.pluginName, "GPS.ReverseGeocode.StatusMessage", "No internet connection");
                return null;
            }

            Location location = new Location(latitude, longitude);
            string statusMessage = "";

            #region Do lookup via internet

            // Preferred service: Geocoder.ca
            if (!Web_LookupAddress_GeoCoder(latitude, longitude, out location, out statusMessage))
            {   // service failed, try other sources (geonames.org)
                if (!Web_LookupAddress_GeoNames(latitude, longitude, out location, out statusMessage))
                {   // This also failed, give up
                    OM.Host.DataHandler.PushDataSourceValue(_BasePlugin.pluginName, "GPS.ReverseGeocode.StatusMessage", statusMessage);
                    return null;
                }
            }

            #endregion

            // Change any iso country names to full names
            if (location.Country.Length == 2)
                location.Country = OpenMobile.Framework.Globalization.CountryISOCodeToFullName(location.Country);

            // Update was successfull, return data back to OM
            if (updateCurrentLocation)
                OM.Host.CurrentLocation = location;
            OM.Host.DataHandler.PushDataSourceValue(_BasePlugin.pluginName, "GPS.ReverseGeocode.StatusMessage", statusMessage);
            return location;
        }

        #region web lookup methods

        private bool Web_LookupAddress_GeoCoder(float latitude, float longitude, out Location location, out string statusMessage)
        {
            // Set initial value for out data
            location = new Location(latitude, longitude);
            statusMessage = "";

            string html = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    html = wc.DownloadString(String.Format(@"http://geocoder.ca/?latt={0}&longt={1}&geoit=xml&range=1&reverse=Reverse+GeoCode+it!", latitude.ToString().Replace(',', '.'), longitude.ToString().Replace(',', '.')));
                }
            }
            catch
            {
                statusMessage = "Problem Contacting website (GeoCoder.ca)";
                return false;
            }

            // Do we have some actual data to process
            if (html.Contains("The latitude and longitude you provided are not in the valid range"))
            {
                statusMessage = "Invalid data for this service (GeoCoder.ca)";
                return false;
            }

            // Parse returned data from the website
            try
            {
                if (html.ToLower().Contains("<error>"))
                {   // Error detected, return error message
                    statusMessage = html.Remove(0, html.IndexOf("<description>") + 13);
                    statusMessage = statusMessage.Substring(0, statusMessage.IndexOf("</"));
                    statusMessage += " (GeoCoder.ca)";
                    return false;
                }

                if (html.ToLower().Contains("<uscity>"))
                {
                    location.City = html.Remove(0, html.IndexOf("<uscity>") + 8);
                    location.City = location.City.Substring(0, location.City.IndexOf("</"));
                    location.Zip = html.Remove(0, html.IndexOf("<zip>") + 5);
                    location.Zip = location.Zip.Substring(0, location.Zip.IndexOf("</"));
                    location.State = html.Remove(0, html.IndexOf("<state>") + 7);
                    location.State = location.State.Substring(0, location.State.IndexOf("</"));
                    location.Country = "US";
                }
                else
                {
                    location.City = html.Remove(0, html.IndexOf("<city>") + 6);
                    location.City = location.City.Substring(0, location.City.IndexOf("</"));
                    location.Zip = html.Remove(0, html.IndexOf("<postal>") + 8);
                    location.Zip = location.Zip.Substring(0, location.Zip.IndexOf("</"));
                    location.State = html.Remove(0, html.IndexOf("<prov>") + 6);
                    location.State = location.State.Substring(0, location.State.IndexOf("<"));
                    location.Country = "CA"; //canada???
                }
            }
            catch
            {
                statusMessage = "Problem parsing website results (GeoCoder.ca)";
                return false;
            }
            
            return true;
        }

        private bool Web_LookupAddress_GeoNames(float latitude, float longitude, out Location location, out string statusMessage)
        {
            // Set initial value for out data
            location = new Location(latitude, longitude);
            statusMessage = "";

            string html = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    html = wc.DownloadString(String.Format(@"http://ws.geonames.org/findNearbyPostalCodesJSON?formatted=true&lat={0}&lng={1}", latitude.ToString().Replace(',', '.'), longitude.ToString().Replace(',', '.')));
                }
            }
            catch
            {
                statusMessage = "Problem Contacting website (GeoNames.org)";
                return false;
            }

            // Do we have some actual data to process
            if (html == "{\"postalCodes\": []}")
            {
                statusMessage = "Invalid data for this service (GeoNames.org)";
                return false;
            }

            // Parse returned data from the website
            try
            {
                html = html.Remove(0, html.IndexOf("postalCode") + 11);
                location.Zip = html.Remove(0, html.IndexOf("postalCode") + 11);
                location.Zip = location.Zip.Remove(0, location.Zip.IndexOf("\"") + 1);
                location.Zip = location.Zip.Substring(0, location.Zip.IndexOf("\""));
                location.Country = html.Remove(0, html.IndexOf("countryCode") + 12);
                location.Country = location.Country.Remove(0, location.Country.IndexOf("\"") + 1);
                location.Country = location.Country.Substring(0, location.Country.IndexOf("\""));
                location.City = html.Remove(0, html.IndexOf("placeName") + 10);
                location.City = location.City.Remove(0, location.City.IndexOf("\"") + 1);
                location.City = location.City.Substring(0, location.City.IndexOf("\""));
                location.State = html.Remove(0, html.IndexOf("adminName1") + 11);
                location.State = location.State.Remove(0, location.State.IndexOf("\"") + 1);
                location.State = location.State.Substring(0, location.State.IndexOf("\""));
            }
            catch
            {
                statusMessage = "Problem parsing website results (GeoNames.org)";
                return false;
            }

            return true;
        }

        #endregion

        #endregion

    }
}
