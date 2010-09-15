using NDesk.DBus;
public delegate void stateChange(uint arg1,uint arg2, uint arg3);
[Interface("org.freedesktop.NetworkManager.Device")]
public interface Device
{
    void Disconnect();
    event stateChange StateChanged;
    eDeviceType DeviceType { get; }
    bool FirmwareMissing { get; }
    bool Managed { get; }
    ObjectPath Ip6Config { get; }
    ObjectPath Dhcp4Config { get; }
    ObjectPath Ip4Config { get; }
    uint State { get; }
    uint Ip4Address { get; }
    uint Capabilities { get; }
    string Driver { get; }
    string IpInterface { get; }
    string Interface { get; }
    string Udi { get; }
}

public enum eDeviceType:uint
{
	/// <summary>
	/// The device type is unknown.
	/// </summary>
	NM_DEVICE_TYPE_UNKNOWN = 0,
	/// <summary>
	/// The device is wired Ethernet device. 
	/// </summary>
	NM_DEVICE_TYPE_ETHERNET = 1,
	/// <summary>
	/// The device is an 802.11 WiFi device. 
	/// </summary>
	NM_DEVICE_TYPE_WIFI = 2,
	/// <summary>
	/// The device is a GSM-based cellular WAN device.
	/// </summary>
	NM_DEVICE_TYPE_GSM = 3,
	/// <summary>
	/// The device is a CDMA/IS-95-based cellular WAN device.
	/// </summary>
	M_DEVICE_TYPE_CDMA = 4, 
}