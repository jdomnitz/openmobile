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
using OpenMobile.Input;
using System.IO;

public partial class MainWindow : Gtk.Window
{
    // Bugfixes provided by Kevin (kross@mp3car)
    static UdpClient receive;
	static UdpClient send;
	Bus system;
	Bus session;
	SessionManager sm;
    ksmserver ksm;
	UPower up;
	Backlight bk;
    PowerDevil pd;
	UDisks disks; 
	MultimediaKeys.MultimediaKeysService mediaKeys=new MultimediaKeys.MultimediaKeysService();

    int getVolume(int instance)
	{
        
        String strDevice = "hw:" + instance; // Could be "default", or hw:X where X is the soundcard number
        UIntPtr mixer;
        if(ASound.snd_mixer_open(out mixer, 0) != 0)
            return -1;
        if(ASound.snd_mixer_attach(mixer, strDevice) != 0)
        {
            ASound.snd_mixer_close(mixer);
            return -1;
        }
        if(ASound.snd_mixer_selem_register(mixer, (UIntPtr)0, (UIntPtr)0) != 0)
        {
            ASound.snd_mixer_detach(mixer, strDevice);
            ASound.snd_mixer_close(mixer);
            return -1;
        }
        if(ASound.snd_mixer_load(mixer) != 0)
        {
            ASound.snd_mixer_detach(mixer, strDevice);
            ASound.snd_mixer_close(mixer);
            return -1;
        }

        UIntPtr mixerelem = ASound.snd_mixer_first_elem(mixer);

        while (mixerelem != (UIntPtr)0)
        {
            String name = ASound.snd_mixer_selem_get_name(mixerelem);
            if (name.ToLower().Equals("master"))
                break;
            mixerelem = ASound.snd_mixer_elem_next(mixerelem);
        }

        int volumePercent = 0;
        if(mixerelem != (UIntPtr)0)
        {
            Int32 playbackSwitch;
            ASound.snd_mixer_selem_get_playback_switch(mixerelem, 0, out playbackSwitch);

            bool bMuted = (playbackSwitch == 0);
            if(bMuted)
            {
                volumePercent = -1;
            }
            else
            {
                IntPtr min, max;
                ASound.snd_mixer_selem_get_playback_volume_range(mixerelem, out min, out max);

                IntPtr volume;
                ASound.snd_mixer_selem_get_playback_volume(mixerelem, 0, out volume);

                volumePercent = (int)volume * 100 / (int)max;
            }
        }
        ASound.snd_mixer_detach(mixer, strDevice);
        ASound.snd_mixer_close(mixer);

        return volumePercent;
 	}

   void checkAllVolume()
    {
        Int32 card = -1;
        while(ASound.snd_card_next(ref card) == 0)
        {
            raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(card).ToString(), card.ToString(), "");
        }
    }

   void setVolume (int instance, int volumePercent)
    {
        String strDevice = "hw:" + instance; // Could be "default", or hw:X where X is the soundcard number

        UIntPtr mixer;
        if(ASound.snd_mixer_open(out mixer, 0) != 0)
            return;
        if(ASound.snd_mixer_attach(mixer, strDevice) != 0)
        {
            ASound.snd_mixer_close(mixer);
            return;
        }
        if(ASound.snd_mixer_selem_register(mixer, (UIntPtr)0, (UIntPtr)0) != 0)
        {
            ASound.snd_mixer_detach(mixer, strDevice);
            ASound.snd_mixer_close(mixer);
            return;
        }
        if(ASound.snd_mixer_load(mixer) != 0)
        {
            ASound.snd_mixer_detach(mixer, strDevice);
            ASound.snd_mixer_close(mixer);
            return;
        }

        UIntPtr mixerelem = ASound.snd_mixer_first_elem(mixer);

        while (mixerelem != (UIntPtr)0)
        {
            String name = ASound.snd_mixer_selem_get_name(mixerelem);
            if (name.ToLower().Equals("master"))
                break;
            mixerelem = ASound.snd_mixer_elem_next(mixerelem);
        }

        if(mixerelem != (UIntPtr)0)
        {
            if(volumePercent < 0)
            {
                ASound.snd_mixer_selem_set_playback_switch_all(mixerelem, 0);
            }
            else
            {
                ASound.snd_mixer_selem_set_playback_switch_all(mixerelem, 1);

                IntPtr min, max;
                ASound.snd_mixer_selem_get_playback_volume_range(mixerelem, out min, out max);

                IntPtr volume = (IntPtr)((int)max * volumePercent / 100);
                ASound.snd_mixer_selem_set_playback_volume_all(mixerelem, volume);
            }
        }
        ASound.snd_mixer_detach(mixer, strDevice);
        ASound.snd_mixer_close(mixer);
    }

    static string DM;
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
        DM = detectDesktopManager();
        if ((DM == "gnome")||(DM=="lxde"))
        {
            sm = session.GetObject<SessionManager>("org.gnome.SessionManager", new ObjectPath("/org/gnome/SessionManager"));
            sm.SessionOver+=Shutdown;
        }
        else
        {
            ksm = session.GetObject<ksmserver>("org.kde.ksmserver", new ObjectPath("/KSMServer")); 
        }
        up = system.GetObject<UPower>("org.freedesktop.UPower", new ObjectPath("/org/freedesktop/UPower"));
		up.Resuming+=Resuming;
		up.Sleeping+=Suspending;

        if ((DM == "gnome")||(DM=="lxde"))
        {
            bk = session.GetObject<Backlight>("org.gnome.PowerManager", new ObjectPath("/org/gnome/PowerManager/Backlight"));
        }
        else
        {
            pd = session.GetObject<PowerDevil>("org.kde.powerdevil", new ObjectPath("/modules/powerdevil/"));
        }
        disks=system.GetObject<UDisks>("org.freedesktop.UDisks",new ObjectPath("/org/freedesktop/UDisks"));
		disks.DeviceAdded+=added;
		disks.DeviceChanged+=changed;
        disks.DeviceRemoved+=remove;
		checkAllVolume();
        if (DM == "gnome")
        {
            mediaKeys.keyPressed += new MultimediaKeys.MultimediaKeysService.MediaPlayerKeyPressedHandler(handleKey);
            mediaKeys.Initialize();
        }
	}

    private static string detectDesktopManager()
    {
        if (File.Exists("/usr/bin/gdm"))
            return "gnome";
        if (File.Exists("/usr/bin/kde"))
            return "kde";
        if (File.Exists("/usr/bin/lxdm"))
            return "lxde";
        return "unknown";
    }

	private void handleKey(string application,string key)
	{
		switch(key)
		{
			case "Play":
                raiseKeypressEvent(eKeypressType.KeyDown,OpenMobile.Input.Key.PlayPause);
				raiseKeypressEvent(eKeypressType.KeyUp,OpenMobile.Input.Key.PlayPause);
                break;
            case "Next":
                raiseKeypressEvent(eKeypressType.KeyDown,OpenMobile.Input.Key.TrackNext);
				raiseKeypressEvent(eKeypressType.KeyUp,OpenMobile.Input.Key.TrackNext);
                break;
            case "Previous":
                raiseKeypressEvent(eKeypressType.KeyDown,OpenMobile.Input.Key.TrackPrevious);
				raiseKeypressEvent(eKeypressType.KeyUp,OpenMobile.Input.Key.TrackPrevious);
                break;
            case "Stop":
                raiseKeypressEvent(eKeypressType.KeyDown,OpenMobile.Input.Key.Stop);
				raiseKeypressEvent(eKeypressType.KeyUp,OpenMobile.Input.Key.Stop);
                break;
		}
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
				uint tracks=(uint)p.Get("org.freedesktop.UDisks.Device","OpticalDiscNumAudioTracks");
				if (tracks>0)
				{
					string tmpPath="cdda://"+System.IO.Path.GetFileName(path.ToString())+"/";
					raiseStorageEvent(eMediaType.AudioCD,false,tmpPath);
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
        if (str.Length>0)
            raiseStorageEvent(eMediaType.DeviceRemoved, true, str[0]);
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
				case "-1":
			            Thread.Sleep(250);
					    checkAllVolume();
					break;
                case "3": //GetData - System Volume
                  int instance;
                    if (int.TryParse(arg1,out instance) && instance >= 0)
                    {
                        try
                        {
                            sendIt("3|" + arg1 + "|" + getVolume(instance));
                        }
                        catch (Exception)
                        {
                        }
                    }
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
									continue;
								}
								uint tracks=(uint)p.Get("org.freedesktop.UDisks.Device","OpticalDiscNumAudioTracks");
								if (tracks>0)
								{
									string tmpPath="cdda://"+System.IO.Path.GetFileName(path.ToString())+"/";
									raiseStorageEvent(eMediaType.AudioCD,false,tmpPath);
								}
							}
					}
					break;
                case "34": //Set Volume
					int val;
                    if (int.TryParse(arg2,out val))
						if (int.TryParse(arg1,out val))
						{
                            setVolume(int.Parse(arg2), val);
                        }
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
                    if (arg1 == "0")
                        if (DM == "kde")
                        {
                            pd.turnOffScreen();
                        }
                        else
                        {
                            try
                            {
                                Process.Start("gnome-screensaver-command", "-a");
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    Process.Start("qdbus", "org.freedesktop.ScreenSaver /ScreenSaver Lock");
                                }
                                catch (Exception) { }
                            }
                        }
                    else
                        if (bk != null)
                            bk.SetBrightness(uint.Parse(arg1));
                        else if (pd != null)
                            pd.setBrightness(int.Parse(arg1));
                    break;
                case "44": //Close Program
					Application.Quit();
                    break;
                case "45": //Hibernate
                    up.Hibernate();
                    break;
                case "46": //Shutdown
                    if (sm!=null)
                        sm.RequestShutdown();
                    else if(ksm!=null)
                        ksm.logout(0, 2, 2);  
                    break;
                case "47": //Restart
                    if (sm != null)
                        sm.RequestReboot();
                    else if (ksm != null)
                        ksm.logout(0, 1, 2);
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
		public static void raiseKeypressEvent(eKeypressType type,OpenMobile.Input.Key args)
		{
			sendIt("-4|"+type+"|"+args);
		}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}


