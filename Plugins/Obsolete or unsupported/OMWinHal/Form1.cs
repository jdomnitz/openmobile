/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenMobile;

namespace OMHal
{
    public partial class Form1 : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;
        static UdpClient receive;
        static UdpClient send;
        public Form1()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Specific.OnHandleRequested += new Specific.getHandle(Form1_OnHandleRequested);
            try
            {
                receive = new UdpClient(8549);
            }
            catch(SocketException)
            {
                Environment.Exit(0);
            }
            send = new UdpClient("127.0.0.1", 8550);
            
            // Send info message: initialization started 
            sendIt("-98|Init_Started");

            Specific.hookVolume(this.Handle);
            DriveHandler.hook(this.Handle);
            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                raisePowerEvent(ePowerEvent.SystemOnBattery);
            SystemEvents.PowerModeChanged+=new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            receive.BeginReceive(recv, null);
            this.Visible = false;

            // Send info message: initialization completed 
            sendIt("-98|Init_Completed|" + GetStartupInfo());
        }


        /// <summary>
        /// Gets startup info for HAL
        /// </summary>
        /// <returns></returns>
        private string GetStartupInfo()
        {
            string Info = "";
 
            // Get assembly title (Should be winHal)
            string Title = "";
            object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                System.Reflection.AssemblyTitleAttribute titleAttribute = (System.Reflection.AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != "")
                    Title = titleAttribute.Title;
            }

            // Use filename if no title is available
            if (Title == "")
                Title = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                    
            // Add title and assembly info
            Info += Title + " (" + System.Reflection.Assembly.GetExecutingAssembly().GetName() + ")";

            // Add audiounit information
            Info += "|HAL AudioUnits:";
            for (int i = 0; i < Specific.DeviceNames.Length; i++)
                Info += "|" + i.ToString() + ": " + Specific.DeviceNames[i];

            return Info;
        }

        IntPtr Form1_OnHandleRequested()
        {
            return this.Handle;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == DriveHandler.WM_MEDIA_CHANGE)
                DriveHandler.WndProc(ref m);
            else
            {
                DriveHandler.WndProc_Devices(ref m);
                base.WndProc(ref m);
            }
        }

        public static void raiseSystemEvent(eFunction eFunction, string arg, string arg2, string arg3)
        {
            string s=((int)eFunction).ToString();
            if (arg != "")
                s += "|" + arg;
            if (arg2 != "")
                s += "|" + arg2;
            if (arg2 != "")
                s += "|" + arg3;
            sendIt(s);
        }
        static void sendIt(string text)
        {
            lock(send)
                send.Send(ASCIIEncoding.ASCII.GetBytes(text), text.Length);
        }
        void parse(string message)
        {
            int ret;
            string[] parts=message.Split(new char[]{'|'});
            string arg1 = "", arg2 = "", arg3 = "";
            if (parts.Length > 3)
                arg3 = parts[3];
            if (parts.Length > 2)
                arg2 = parts[2];
            if (parts.Length > 1)
                arg1 = parts[1];
            switch (parts[0])
            {
                case "1":
                    if (int.TryParse(arg1, out ret))
                    {
                        if (ret >= 0)
                            sendIt("1|" + arg1 + "|" + Specific.GetBrightness(ret));
                    }
                    break;
                case "3": //GetData - System Volume
                    if (int.TryParse(arg1,out ret)){
                        if(ret>=0)
                            try
                            {
                                sendIt("3|" + arg1 + "|" + Specific.getVolume(ret));
                            }
                            catch (Exception)
                            {
                                //MessageBox.Show("GERROR: " + e.Message + "\nStack Trace:\n" + e.StackTrace);
                            }
                    }
                    break;
                case "32":  // Enumerate drives
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                        if ((drive.DriveType == DriveType.CDRom)||(drive.DriveType==DriveType.Removable))
                            if (drive.IsReady == true)
                            {
                                raiseStorageEvent(eMediaType.NotSet, false, drive.RootDirectory.ToString());
                            }
                    break;
                case "34": //Set Volume
                    if (int.TryParse(arg2, out ret))
                    {
                        if (ret >= 0)
                            try
                            {
                                Specific.setVolume(int.Parse(arg1), ret);
                            }
                            catch (Exception)
                            {
                                //MessageBox.Show("SERROR: " + e.Message + "\nStack Trace:\n" + e.StackTrace);
                            }
                    }
                    break;
                case "35": //Eject Disc
                    Specific.eject();
                    break;
                case "40": //Set Monitor Brightness
                    Specific.SetBrightness(int.Parse(arg1), int.Parse(arg2));
                    break;
                case "42":
                    if (int.TryParse(arg2, out ret))
                    {
                        if (ret >= 0)
                            try
                            {
                                Specific.setBalance(int.Parse(arg1), ret);
                            }
                            catch (Exception) { }
                    }
                    break;
                case "44": //Close Program
                    Environment.Exit(0);
                    break;
                case "45":
                    Specific.Hibernate();
                    break;
                case "46": //Shutdown
                    Specific.Shutdown(false);
                    break;
                case "47": //Restart
                    Specific.Shutdown(true);
                    break;
                case "48":
                    Specific.Suspend();
                    break;
                case "66":
                    Specific.setSubVolume(int.Parse(arg1), int.Parse(arg2));
                    break;
            }
        }
        void recv(IAsyncResult res)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 8549);
            parse (ASCIIEncoding.ASCII.GetString(receive.EndReceive(res, ref remote)));
            receive.BeginReceive(recv, null);
        }

        public static void raiseStorageEvent(eMediaType MediaType,bool justHappened, string drive)
        {
            sendIt("-3|" + MediaType +"|" + justHappened + "|" + drive);
        }
        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (e.Reason == SessionEndReasons.Logoff)
                try
                {
                    raisePowerEvent(ePowerEvent.LogoffPending);
                }
                catch (Exception) { }
            if (e.Reason == SessionEndReasons.SystemShutdown)
                try
                {
                    raisePowerEvent(ePowerEvent.ShutdownPending);
                }
                catch (Exception) { }
        }
        public void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                if (e.Mode == PowerModes.Resume)
                    raisePowerEvent(ePowerEvent.SystemResumed);
                else if (e.Mode == PowerModes.Suspend)
                    raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
                else
                {
                    if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                    {
                        if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Low)
                            raisePowerEvent(ePowerEvent.BatteryLow);
                        else if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.Critical)
                            raisePowerEvent(ePowerEvent.BatteryCritical);
                        else
                            raisePowerEvent(ePowerEvent.SystemOnBattery);
                    }
                    else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                        raisePowerEvent(ePowerEvent.SystemPluggedIn);
                    else
                        raisePowerEvent(ePowerEvent.Unknown);
                }
            }
            catch (Exception) { }
        }

        private void raisePowerEvent(ePowerEvent ePowerEvent)
        {
            sendIt("-2|" + ePowerEvent.ToString());
        }


        static bool ErroredOut;
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ErroredOut) //Prevent multiple bugs
                return;
            ErroredOut = true;

            // Send error message
            sendIt("-99|UnhandledException|" + spewException((Exception)e.ExceptionObject));

            Environment.Exit(0);
        }
        private static string spewException(Exception e)
        {
            string err;
            err = e.GetType().Name + "|";
            err += ("Exception Message: " + e.Message);
            err += ("|Source: " + e.Source);
            err += ("|nStack Trace: |" + e.StackTrace);
            err += ("|");
            int failsafe = 0;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                err += ("Inner Exception: " + e.Message);
                err += ("|Source: " + e.Source);
                err += ("|Stack Trace: |" + e.StackTrace);
                err += ("|");
                failsafe++;
                if (failsafe == 4)
                    break;
            }
            return err;
        }

    }
}
