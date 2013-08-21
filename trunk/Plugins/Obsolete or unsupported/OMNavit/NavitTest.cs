using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace OpenMobile.Plugins.OMNavit
{
    public partial class NavitTest : Form
    {
        public Func<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.PoiSearchRequest,
            OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.PoiSearchResult[]> DoPoiSearch;
        public Func<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.AddressSearchRequest,
            OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.AddressSearchResult[]> DoAddressSearchRequest;
        public Func<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.Coord> DoGetCurrentPosition;
        public Action DoClearDestination;
        public Action<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.Coord> DoSetDestination;

        private StringBuilder sb;

        public NavitTest()
        {
            InitializeComponent();
            sb = new StringBuilder();
        }

        public void DoSetMapItem(OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.MapItem mapItem)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.MapItem>(DoSetMapItem), mapItem);
                return;
            }
            tbMapPointX.Text = mapItem.Coord.X.ToString();
            tbMapPointY.Text = mapItem.Coord.Y.ToString();
        }

        private void btnAddressSearch_Click(object sender, EventArgs e)
        {
            sb.Length = 0; // Clear()
            textBox1.Text = string.Empty;
            if (DoAddressSearchRequest != null)
            {
                var results = DoAddressSearchRequest(new OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.AddressSearchRequest
                {
                    HouseNumber = tbHouseNumber.Text,
                    Street = tbStreet.Text,
                    Town = tbCity.Text,
                    Postal = tbZip.Text
                   });
                foreach (var result in results)
                {
                    var str = string.Format("{0} {1} {2}, {3}",
                        result.HouseNumber,
                        result.Street,
                        result.Town,
                        result.Country);

                    AddString(str);
                }
            }
        }

        private void btnPoiSearch_Click(object sender, EventArgs e)
        {
            sb.Length = 0; // Clear();
            textBox1.Text = string.Empty;
            
            var poiSearchRequest =
                new OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.PoiSearchRequest();
            
            try{
                
                poiSearchRequest.DistMultiplyer = int.Parse(tbDistance.Text);
                poiSearchRequest.FilterString = tbFilterString.Text;
                poiSearchRequest.IsAddressFilter = tbIsAdrFilter.Text != string.Empty;
                poiSearchRequest.PageNumber = int.Parse(tbPageNumber.Text);
                poiSearchRequest.PoisPerPage = int.Parse(tbPageCount.Text);
                poiSearchRequest.Type = (NavitOpenGLInterface.PoiType)int.Parse(tbType.Text);
                

                }catch(Exception ex){MessageBox.Show("Shitty input man: " + ex.Message); return;}
            if (DoPoiSearch != null)
            {
                var results = DoPoiSearch(poiSearchRequest);

                foreach (var result in results)
                {
                    var str = string.Format("{0} {1} {2}, {3} {4} {5}",
                        result.Direction.PadRight(6),
                        result.Distance.ToString().PadRight(10),
                        result.Name.PadRight(50),
                        result.Type.ToString(),
                        result.Coord.Y,
                        result.Coord.X);

                    AddString(str);
                }
            }
        }

        private void AddString(string pStr)
        {
            if(sb.Length > 30000)
                sb.Remove(0,500);

            sb.AppendLine(pStr);

            textBox1.Text = sb.ToString();
            textBox1.SelectionStart = textBox1.Text.Length;
        }

        private void btnGetMapPos_Click(object sender, EventArgs e)
        {
            if(DoGetCurrentPosition != null)
            {
                sb.Length = 0; //.Clear();
                var coord = DoGetCurrentPosition();

                AddString(string.Format("{0} {1}", coord.X, coord.Y));
            }
        }

        private void btnClearDestination_Click(object sender, EventArgs e)
        {
            if (DoGetCurrentPosition != null)
                DoGetCurrentPosition();
        }

        private void btnSetDestination_Click(object sender, EventArgs e)
        {
            if(DoSetDestination != null)
            {
                var coord = new OpenMobile.Plugins.OMNavit.NavitOpenGLInterface.Coord();

                try{
                    coord.X = double.Parse(tbDestinationX.Text);
                    coord.Y = double.Parse(tbDestinationY.Text);
                }
                catch(Exception ex){MessageBox.Show("Shitty input man: " + ex.Message); return;}

                DoSetDestination(coord);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tbDestinationX.Text = tbMapPointX.Text;
            tbDestinationY.Text = tbMapPointY.Text;
        }

        // SILLY GPS CODE

        private void btnStartGps_Click(object sender, EventArgs e)
        {
            StartGPS();
        }

        private void btnPauseGps_Click(object sender, EventArgs e)
        {
            pauseWrite = true;
        }

        private void btnStopGps_Click(object sender, EventArgs e)
        {
            KillSerialPortStuff();
        }

        
        private string nmea_file = @"C:\temp\Joe_to_work.txt";
        private SerialPort sendingPort;
        private Thread writeThread;
        private void StartGPS()
        {
            if (writeThread != null)
            {
                if (pauseWrite)
                    pauseWrite = false;
                return;
            }

            sendingPort = new SerialPort("COM8", 38400);
            sendingPort.WriteTimeout = 5000;
            sendingPort.Open();
            
            //recipientPort = new SerialPort("COM7", 38400);
            //recipientPort.ReadTimeout = 5000;
            //recipientPort.Open();

            //var readThread = new Thread(readPort);
            //readThread.Start();

            writeThread = new Thread(writeToPort);
            writeThread.Start();
        }
        
        private void KillSerialPortStuff()
        {
            pauseWrite = false;
            writeThread = null;
            sendingPort.Close();
            stopWrite = true;
        }

        private bool pauseWrite = false;
        private bool stopWrite = false;
        private void writeToPort()
        {
            long lastTimeStamp = 0;
            System.IO.StreamReader nmeaStream = null;
            if (!System.IO.File.Exists(nmea_file))
                return;
            try
            {
                nmeaStream = new System.IO.StreamReader(nmea_file);
            }
            catch { return; }

            while (!stopWrite)
            {
                if (pauseWrite)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (nmeaStream.EndOfStream)
                {
                    lastTimeStamp = 0;
                    nmeaStream.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    nmeaStream.DiscardBufferedData();
                    if (nmeaStream.EndOfStream)
                        throw new Exception("File is empty?");
                }

                string nmeaLine = nmeaStream.ReadLine();
                var nmeaSplit = nmeaLine.Split(new char[] { ';' });
                int sleep = 100;
                if (nmeaSplit.Length == 2)
                {
                    nmeaLine = nmeaSplit[1];
                    long parsedLong = 0;
                    if (long.TryParse(nmeaSplit[0], out parsedLong)
                        && parsedLong > 0)
                    {
                        if (lastTimeStamp > 0)
                        {
                            sleep = (int)(parsedLong - lastTimeStamp);
                            if (sleep < 0 || sleep > 10000)
                                sleep = 100;
                        }
                        lastTimeStamp = parsedLong;
                    }
                    else
                    {
                        sleep = 100;
                        lastTimeStamp = 0;
                    }
                }
                else
                {
                    lastTimeStamp = 0;
                }
                Thread.Sleep(sleep);
                if (sendingPort != null
                    && sendingPort.IsOpen)
                {
                    try
                    {
                        sendingPort.WriteLine(nmeaLine);
                        System.Diagnostics.Debug.WriteLine("OutputGPS LINE: " + nmeaLine);
                    }
                    catch (TimeoutException)
                    {
                        System.Diagnostics.Debug.WriteLine("Timeout sending NMEA data");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error " + ex.Message);
                    }
                }
            }
        }
    }
}
