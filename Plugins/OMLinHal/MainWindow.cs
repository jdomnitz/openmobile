using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using Gtk;
using NDesk.DBus;
using OpenMobile;
using System.Threading;
using System.Collections.Generic;

public partial class MainWindow : Gtk.Window
{
	static UdpClient receive;
	static UdpClient send;
	Bus system;
	Bus session;
	SessionManager sm;
	UPower up;
	Backlight bk;
	UDisks disks; 
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		system=Bus.System;
		session=Bus.Session;
		try
        {
            receive = new UdpClient(8549);
        }
		catch(SocketException)
        {
            Environment.Exit(0);
        }
        send = new UdpClient("127.0.0.1", 8550);
        receive.BeginReceive(recv, null);
		sm=session.GetObject<SessionManager>("org.gnome.SessionManager",new ObjectPath("/org/gnome/SessionManager"));
		up=system.GetObject<UPower>("org.freedesktop.UPower",new ObjectPath("/org/freedesktop/UPower"));
		up.Resuming+=Resuming;
		up.Sleeping+=Suspending;
		sm.SessionOver+=Shutdown;
		bk=session.GetObject<Backlight>("org.gnome.PowerManager",new ObjectPath("/org/gnome/PowerManager/Backlight"));
		disks=system.GetObject<UDisks>("org.freedesktop.UDisks",new ObjectPath("/org/freedesktop/UDisks"));
		disks.DeviceAdded+=added;
		disks.DeviceChanged+=changed;
		//disks.DeviceRemoved+=remove;
	}
	Dictionary<ObjectPath,string> removables=new Dictionary<ObjectPath, string>();
	private void changed(ObjectPath path)
	{
		Properties p=system.GetObject<Properties>("org.freedesktop.UDisks",path);
		if ((bool)p.Get("org.freedesktop.UDisks.Device","DriveIsMediaEjectable"))
		{
			if ((bool)p.Get("org.freedesktop.UDisks.Device","DeviceIsMediaAvailable"))
			{
				string[] mountPath=((string[])p.Get("org.freedesktop.UDisks.Device","DeviceMountPaths"));
				if (mountPath.Length>0)
				{
					if (removables.ContainsKey(path))
						removables[path]=mountPath[0];
					else
						removables.Add(path,mountPath[0]);
					raiseStorageEvent(eMediaType.NotSet,true,mountPath[0]);
				}
			}
			else
			{
				string[] mountPath=(string[])p.Get("org.freedesktop.UDisks.Device","DeviceMountPaths");
				if (mountPath.Length>0)
				{
					if (removables.ContainsValue(mountPath[0]))
					{
						removables.Remove(path);
						raiseStorageEvent(eMediaType.DeviceRemoved,true,mountPath[0]);
					}
				}
			}
		}
	}
	private void remove(ObjectPath path)
	{
		Properties p=system.GetObject<Properties>("org.freedesktop.UDisks",path);
		string[] str=(string[])p.Get("org.freedesktop.UDisks.Device","DeviceMountPaths");
		Debug.Print(str[0]);
	}
	private void added(ObjectPath path)
	{
		Properties p=system.GetObject<Properties>("org.freedesktop.UDisks",path);
		string[] str=new string[0];
		for(int i=0;i<20;i++)
		{
			str=(string[])p.Get("org.freedesktop.UDisks.Device","DeviceMountPaths");
			if (str.Length>0)
				break;
			Thread.Sleep(100);
		}
		if (str.Length==0)
		{
			Device d=system.GetObject<Device>("org.freedesktop.UDisks",path);
			object ret=d.FilesystemMount("auto",new string[0]);
			if (ret!=null)
				raiseStorageEvent(eMediaType.NotSet,true,ret.ToString());
		}
		else
			raiseStorageEvent(eMediaType.NotSet,true,str[0]);
	}
	private void Shutdown()
	{
		raisePowerEvent(ePowerEvent.ShutdownPending);	
	}
	private void Resuming()
	{
		raisePowerEvent(ePowerEvent.SystemResumed);
	}
	private void Suspending()
	{
		raisePowerEvent(ePowerEvent.SleepOrHibernatePending);
	}
	public static void raisePowerEvent(ePowerEvent e)
        {
            sendIt("-2|"+((int)e).ToString());
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
				case "32": //Plugins Loaded
					foreach(ObjectPath path in disks.EnumerateDevices())
					{
						Properties p=system.GetObject<Properties>("org.freedesktop.UDisks",path);
						if ((bool)p.Get("org.freedesktop.UDisks.Device","DriveIsMediaEjectable"))
							if ((bool)p.Get("org.freedesktop.UDisks.Device","DeviceIsMediaAvailable"))
							{
								string[] mountPath=(string[])p.Get("org.freedesktop.UDisks.Device","DeviceMountPaths");
								if (mountPath.Length>0)
								{
									removables.Add(path,mountPath[0]);
									raiseStorageEvent(eMediaType.NotSet,false,mountPath[0]);
								}
							}
					}
					break;
                case "34": //Set Volume
                    //TODO;
                    break;
                case "35": //Eject Disc
                    if (removables.ContainsValue(arg1))
					{
						foreach(var pair in removables)
							if (pair.Value==arg1)
							{
								Properties p=system.GetObject<Properties>("org.freedesktop.UDisks",pair.Key);
								Device d=system.GetObject<Device>("org.freedesktop.UDisks",pair.Key);
								if ((bool)p.Get("org.freedesktop.UDisks","DeviceIsMounted"))
									d.FilesystemUnmount(new string[0]);
								d.DriveEject(new string[0]);
							}
					}
                    break;
                case "40": //Set Monitor Brightness
					if (arg1=="0")
						try
						{
							Process.Start("gnome-screensaver-command","-a");
						}catch(Exception)
						{
							try
							{
								Process.Start("qdbus","org.freedesktop.ScreenSaver /ScreenSaver Lock");	
							}catch(Exception){}
						}
					else
                    	bk.SetBrightness(uint.Parse(arg1));
                    break;
                case "44": //Close Program
					Application.Quit();
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

        public static void raiseStorageEvent(eMediaType MediaType,bool justInserted, string drive)
        {
            sendIt("-3|" + MediaType + "|" + justInserted + "|" + drive);
        }
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}


