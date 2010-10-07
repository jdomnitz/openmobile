using System;
using NDesk.DBus;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile;
using System.Threading;
using OpenMobile.Data;
namespace LinWifi
{
	public class LinWifi:INetwork
	{
		public event WirelessEvent OnWirelessEvent;

		List<connectionInfo> connections=new List<connectionInfo>();
		public connectionInfo[] getAvailableNetworks ()
		{
			detectAPs();
			return connections.ToArray();
		}

		public bool connect (OpenMobile.connectionInfo connection)
		{
			//TODO
			return false;
		}

		public bool disconnect (OpenMobile.connectionInfo connection)
		{
			if (wiDev==null)
				return false;
			wiDev.Disconnect();
			return true;
		}

		public bool refresh ()
		{
			if (wireless.Count==0)
				return false;
			detectAPs();
			return true;
		}

		public eConnectionStatus getConnectionStatus ()
		{
			if (wiDev==null)
				return eConnectionStatus.Error;
			uint state=(uint)wiProp.Get("org.freedesktop.NetworkManager.Device","State");
			switch(state)
			{
			case 0:
			case 1:
			case 9:
				return eConnectionStatus.Error;
			case 2:
			case 3:
				return eConnectionStatus.Disconnected;
			case 4:
			case 5:
			case 6:
			case 7:
				return eConnectionStatus.Connecting;
			case 8:
				return eConnectionStatus.Connected;
			}
			return eConnectionStatus.Error;
		}
		Thread t;
		public eLoadStatus initialize (IPluginHost host)
		{
			b=NDesk.DBus.Bus.System;
			if (!detectHardware())
				return eLoadStatus.LoadFailedGracefulUnloadRequested;
            using (PluginSettings settings = new PluginSettings())
                settings.setSetting("Default.Wifi", "LinWifi");
			checkNetworks();
			t=new Thread(delegate(){while(true) b.Iterate();});
			t.Start();
			return eLoadStatus.LoadSuccessful;
		}
		private void checkNetworks()
		{
			foreach(Wireless w in wireless)
			{
				if (((ObjectPath[])w.GetAccessPoints()).Length>0)
				{
					if (OnWirelessEvent!=null)
						OnWirelessEvent(eWirelessEvent.WirelessNetworksAvailable,"");
				}
			}
		}
		NDesk.DBus.Bus b;
		List<Wireless> wireless=new List<Wireless>();
		List<AccessPoint> aps=new List<AccessPoint>();
		List<Properties> apProps=new List<Properties>();
		
		private void detectAPs()
		{
			connections.Clear();
			foreach(Wireless w in wireless)
			{
				ObjectPath active=(ObjectPath)wiProp.Get("org.freedesktop.NetworkManager.Device.Wireless","ActiveAccessPoint");
				foreach(ObjectPath p in w.GetAccessPoints())
				{
					aps.Add(b.GetObject<AccessPoint>("org.freedesktop.NetworkManager",p));
					apProps.Add(b.GetObject<Properties>("org.freedesktop.NetworkManager",p));
					byte[] ssid=(byte[])apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","Ssid");
					byte signal=(byte)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","Strength");
					uint rate=(uint)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","MaxBitrate");
					uint rsnFlags=(uint)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","RsnFlags");
					uint wpaFlags=(uint)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","WpaFlags");
					uint mode=(uint)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","Mode");
					string address=(string)apProps[apProps.Count-1].Get("org.freedesktop.NetworkManager.AccessPoint","HwAddress");
					string security=guessType(rate,mode)+" ("+ getSecurity((eSecurity)rsnFlags,(eSecurity)wpaFlags)+")";
					connectionInfo info=new connectionInfo(System.Text.ASCIIEncoding.ASCII.GetString(ssid),p.ToString(),(int)rate,signal,security,ePassType.None);
					if (active.ToString()==p.ToString())
					{
						info.IsConnected=true;
						if (OnWirelessEvent!=null)
							OnWirelessEvent(eWirelessEvent.WirelessSignalStrengthChanged,signal.ToString());
					}
					connections.Add(info);
				}
			}
		}
		
		string guessType (uint rate,uint mode)
		{
			string ad="";
			if (mode==1)
				ad="Ad-Hoc ";
			if (rate<=11000)
				return ad+"802.11b";
			else if(rate<=108000)
				return ad+"802.11g";
			else
				return ad+"802.11n";
		}

		private string getSecurity (eSecurity rsn,eSecurity wpa)
		{
			if ((rsn&eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)==eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)
				return "WPA2-PSK";	
			else if(rsn!=eSecurity.NM_802_11_AP_SEC_NONE)
				return "WPA2-Enterprise";
			
			if ((wpa&eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)==eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)
			{
				if ((wpa&eSecurity.NM_802_11_AP_SEC_PAIR_CCMP)==eSecurity.NM_802_11_AP_SEC_PAIR_CCMP)
					return "WPA-PSK";
				if ((wpa&eSecurity.NM_802_11_AP_SEC_PAIR_TKIP)==eSecurity.NM_802_11_AP_SEC_PAIR_TKIP)
					return "WPA-PSK";
			}
			else
			{
				if ((wpa&eSecurity.NM_802_11_AP_SEC_PAIR_CCMP)==eSecurity.NM_802_11_AP_SEC_PAIR_CCMP)
					return "WPA-Enterprise";
				if ((wpa&eSecurity.NM_802_11_AP_SEC_PAIR_TKIP)==eSecurity.NM_802_11_AP_SEC_PAIR_TKIP)
					return "WPA-Enterprise";
			}
				
			if ((wpa&eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)==eSecurity.NM_802_11_AP_SEC_KEY_MGMT_PSK)
				return "WEP-PSK";
			if (wpa==eSecurity.NM_802_11_AP_SEC_NONE)
				return "Open Network";
			return "WEP";
		}

		private Device wiDev;
		private Properties wiProp;
		private bool detectHardware()
		{
			ObjectPath nm=new ObjectPath("/org/freedesktop/NetworkManager");
			NetworkManager manager=b.GetObject<NetworkManager>("org.freedesktop.NetworkManager",nm);
			Properties networkProps=b.GetObject<Properties>("org.freedesktop.NetworkManager",nm);
			if ((bool)networkProps.Get("org.freedesktop.NetworkManager","WirelessEnabled")==false)
				return false;
			if ((bool)networkProps.Get("org.freedesktop.NetworkManager","WirelessHardwareEnabled")==false)
				return false;
			ObjectPath[] devPaths=manager.GetDevices();
			Device[] devices=new Device[devPaths.Length];
			Properties[] props=new Properties[devPaths.Length];
			for(int i=0;i<devices.Length;i++)
			{
				devices[i]=b.GetObject<Device>("org.freedesktop.NetworkManager",devPaths[i]);
				props[i]=b.GetObject<Properties>("org.freedesktop.NetworkManager",devPaths[i]);
			}
			for(int i=0;i<devices.Length;i++)
			{
				if ((eDeviceType)props[i].Get("org.freedesktop.NetworkManager.Device","DeviceType")==eDeviceType.NM_DEVICE_TYPE_WIFI)
				{
					wireless.Add(b.GetObject<Wireless>("org.freedesktop.NetworkManager",devPaths[i]));
					wiDev=devices[i];
					wiProp=props[i];
					wiDev.StateChanged+=stateChanged;
					wireless[wireless.Count-1].AccessPointAdded+=apChanged;
					wireless[wireless.Count-1].AccessPointRemoved+=apChanged;
				}
			}
			return true;
		}
		void apChanged(ObjectPath Path)
		{
			checkNetworks();
		}
		void stateChanged(uint newState,uint oldState,uint reason)
		{
			if ((oldState >3)&&(newState==3))
				if (OnWirelessEvent!=null)
					OnWirelessEvent(eWirelessEvent.DisconnectedFromWirelessNetwork,"");
			if ((oldState==3)&&(newState>3)&&(newState!=8))
				if (OnWirelessEvent!=null)
					OnWirelessEvent(eWirelessEvent.ConnectingToWirelessNetwork,"Unknown");
			if (newState==8)
				if (OnWirelessEvent!=null)
					OnWirelessEvent(eWirelessEvent.ConnectedToWirelessNetwork,"Unknown");
		}
		#region IBasePlugin implementation
		public Settings loadSettings ()
		{
			return null;
		}

		public bool incomingMessage (string message, string source)
		{
			return false;
		}

		public bool incomingMessage<T> (string message, string source, ref T data)
		{
			return false;
		}

		public string authorName {
			get {
				return "Justin Domnitz";
			}
		}

		public string authorEmail {
			get {
				return "";
			}
		}

		public string pluginName {
			get {
				return "LinWifi";
			}
		}

		public float pluginVersion {
			get {
				return 0.1F;
			}
		}

		public string pluginDescription {
			get {
				return "Linux Wifi Support";
			}
		}
		public void Dispose ()
		{
            //
		}
		#endregion		
	}
}

