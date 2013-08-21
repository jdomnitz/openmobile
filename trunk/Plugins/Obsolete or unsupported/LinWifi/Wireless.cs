using NDesk.DBus;
public delegate void SingleChange(ObjectPath path);
[Interface("org.freedesktop.NetworkManager.Device.Wireless")]
public interface Wireless
{    
    ObjectPath[] GetAccessPoints();
	    uint Bitrate { get; }
    e80211Mode Mode { get; }
    event SingleChange AccessPointRemoved;
    event SingleChange AccessPointAdded;
    //event EventHandler<Dictionary<string, object>> PropertiesChanged;
    uint WirelessCapabilities { get; }
    ObjectPath ActiveAccessPoint { get; }
    string PermHwAddress { get; }
    string HwAddress { get; }
}

public enum e80211Mode:uint
{
	/// <summary>
	/// Mode is unknown.
	/// </summary>
	NM_802_11_MODE_UNKNOWN = 0,
	/// <summary>
	/// Uncoordinated network without central infrastructure.
	/// </summary>
	NM_802_11_MODE_ADHOC = 1,
	/// <summary>
	/// Coordinated network with one or more central controllers.
	/// </summary>
	NM_802_11_MODE_INFRA = 2,
}