using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Gtk;
using NDesk.DBus;
using OpenMobile;

public partial class MainWindow : Gtk.Window
{
	static UdpClient receive;
	static UdpClient send;
	Bus system;
	Bus session;
	SessionManager sm;
	UPower up;
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		system=Bus.System;
		session=Bus.Session;
		try
        {
            receive = new UdpClient(8549);
        }catch(SocketException)
        {
            Environment.Exit(0);
        }
        send = new UdpClient("127.0.0.1", 8550);
        receive.BeginReceive(recv, null);
		sm=session.GetObject<SessionManager>("org.gnome.SessionManager",new ObjectPath("/org/gnome/SessionManager"));
		up=system.GetObject<UPower>("org.freedesktop.UPower",new ObjectPath("/org/freedesktop/UPower"));
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
                    //TODO
                    break;
                case "34": //Set Volume
                    //TODO;
                    break;
                case "35": //Eject Disc
                    //TODO
                    break;
                case "40": //Set Monitor Brightness
                    //TODO
                    break;
                case "44": //Close Program
                    Environment.Exit(0);
                    break;
                case "45": //Hibernate
                    up.Hibernate();
                    break;
                case "46": //Shutdown
                    sm.RequestShutdown();
                    break;
                case "47": //Restart
                    sm.RequestReboot();
                    break;
                case "48": //Standby
                    up.Suspend();
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
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}

