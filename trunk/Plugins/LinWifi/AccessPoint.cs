using System;
using NDesk.DBus;
[Interface("org.freedesktop.NetworkManager.AccessPoint")]
public interface AccessPoint
{
	/// <summary>
	/// Signal Strength [0-100%]
	/// </summary>
    byte Strength { get; }
	/// <summary>
	/// maximum bitrate in kb/s
	/// </summary>
    uint MaxBitrate { get; }
    e80211Mode Mode { get; }
    string HwAddress { get; }
    uint Frequency { get; }
    byte[] Ssid { get; }
    eSecurity RsnFlags { get; }
    eSecurity WpaFlags { get; }
    uint Flags { get; }
}
[Flags]
public enum eSecurity:uint
{
	/// <summary>
	/// Null flag.
	/// </summary>
	NM_802_11_AP_SEC_NONE = 0x0,
	/// <summary>
	/// Access point supports pairwise 40-bit WEP encryption.
	/// </summary>
	NM_802_11_AP_SEC_PAIR_WEP40 = 0x1,
	/// <summary>
	/// Access point supports pairwise 104-bit WEP encryption.
	/// </summary>
	NM_802_11_AP_SEC_PAIR_WEP104 = 0x2,
	/// <summary>
	/// Access point supports pairwise TKIP encryption.
	/// </summary>
	NM_802_11_AP_SEC_PAIR_TKIP = 0x4,
	/// <summary>
	/// Access point supports pairwise CCMP encryption.
	/// </summary>
	NM_802_11_AP_SEC_PAIR_CCMP = 0x8,
	/// <summary>
	/// Access point supports a group 40-bit WEP cipher.
	/// </summary>
	NM_802_11_AP_SEC_GROUP_WEP40 = 0x10,
	/// <summary>
	/// Access point supports a group 104-bit WEP cipher.
	/// </summary>
	NM_802_11_AP_SEC_GROUP_WEP104 = 0x20,
	/// <summary>
	/// Access point supports a group TKIP cipher.
	/// </summary>
	NM_802_11_AP_SEC_GROUP_TKIP = 0x40,
	/// <summary>
	/// Access point supports a group CCMP cipher.
	/// </summary>
	NM_802_11_AP_SEC_GROUP_CCMP = 0x80,
	/// <summary>
	/// Access point supports PSK key management.
	/// </summary>
	NM_802_11_AP_SEC_KEY_MGMT_PSK = 0x100,
	/// <summary>
	/// Access point supports 802.1x key management.
	/// </summary>
	NM_802_11_AP_SEC_KEY_MGMT_802_1X = 0x200,
}