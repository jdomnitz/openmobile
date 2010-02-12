using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using OpenMobile.Framework;
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
            int newVolume = Specific.getVolume();
            if (volume == newVolume)
                return;
            volume = newVolume;
            raiseSystemEvent(eFunction.systemVolumeChanged, newVolume.ToString(), "", "");
        }

        public static void raiseSystemEvent(eFunction eFunction, string arg, string arg2, string arg3)
        {
            string s=((int)eFunction).ToString();
            if (arg != "")
                s += "|" + arg;
            if (arg2 != "")
                s += "|" + arg3;
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
            if (message == "45")
                Environment.Exit(0);
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
                case "34":
                    Specific.setVolume(int.Parse(arg1));
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
