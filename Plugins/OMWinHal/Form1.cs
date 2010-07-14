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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
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
            Specific.OnHandleRequested += new Specific.getHandle(Form1_OnHandleRequested);
            try
            {
                receive = new UdpClient(8549);
            }catch(SocketException)
            {
                Environment.Exit(0);
            }
            send = new UdpClient("127.0.0.1", 8550);
            Specific.hookVolume(this.Handle);
            receive.BeginReceive(recv, null);
            this.Visible = false;
        }

        IntPtr Form1_OnHandleRequested()
        {
            return this.Handle;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
                DriveHandler.WndProc(ref m);
            else
                base.WndProc(ref m);
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
                    Specific.SetBrightness(int.Parse(arg2));
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
                    Application.SetSuspendState(PowerState.Hibernate, false, false);
                    break;
                case "46": //Shutdown
                    Specific.Shutdown(false);
                    break;
                case "47": //Restart
                    Specific.Shutdown(true);
                    break;
                case "48":
                    Application.SetSuspendState(PowerState.Suspend, false, false);
                    break;
            }
        }
        void recv(IAsyncResult res)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 8549);
            parse (ASCIIEncoding.ASCII.GetString(receive.EndReceive(res, ref remote)));
            receive.BeginReceive(recv, null);
        }

        public static void raiseStorageEvent(eMediaType MediaType, string drive)
        {
            sendIt("-3|" + MediaType + "|" + drive);
        }
    }
}
