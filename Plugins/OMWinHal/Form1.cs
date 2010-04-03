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
        private const int MM_MIXM_CONTROL_CHANGE = 0x3D1;
        private const int WM_DEVICECHANGE = 0x0219;
        static UdpClient receive;
        static UdpClient send;
        public Form1()
        {
            InitializeComponent();
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
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
                DriveHandler.WndProc(ref m);
            else if (m.Msg == MM_MIXM_CONTROL_CHANGE)
                checkVolumeChange();
            else
                base.WndProc(ref m);
        }
        static int volume = 0;
        public static void checkVolumeChange()
        {
            int newVolume = Specific.getVolume(0);
            if (volume == newVolume)
                return;
            volume = newVolume;
            raiseSystemEvent(eFunction.systemVolumeChanged, newVolume.ToString(), "0", "");
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
                    int ret;
                    if (int.TryParse(arg1,out ret)==true){
                        if(ret>=0)
                            sendIt("3|" + arg1+"|"+ Specific.getVolume(ret));
                    }
                    break;
                case "34": //Set Volume
                    Specific.setVolume(int.Parse(arg1),0);
                    break;
                case "35": //Eject Disc
                    Specific.eject();
                    break;
                case "45": //Close Program
                    Environment.Exit(0);
                    break;
                case "46": //Shutdown
                    ProcessStartInfo info = new ProcessStartInfo("shutdown", "/s /t 0");
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(info);
                    break;
                case "47": //Restart
                    ProcessStartInfo info2 = new ProcessStartInfo("shutdown", "/r /t 0");
                    info2.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(info2);
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
