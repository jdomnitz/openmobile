using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Dialup.RAS
{
	internal enum RasFieldSizeConstants
	{
		RAS_MaxDeviceType     =16,
		RAS_MaxPhoneNumber    =128,
		RAS_MaxIpAddress      =15,
		RAS_MaxIpxAddress     =21,
#if WINVER4
		RAS_MaxEntryName      =256,
		RAS_MaxDeviceName     =128,
		RAS_MaxCallbackNumber =RAS_MaxPhoneNumber,
#else
		RAS_MaxEntryName      =20,
		RAS_MaxDeviceName     =32,
		RAS_MaxCallbackNumber =48,
#endif

		RAS_MaxAreaCode       =10,
		RAS_MaxPadType        =32,
		RAS_MaxX25Address     =200,
		RAS_MaxFacilities     =200,
		RAS_MaxUserData       =200,
		RAS_MaxReplyMessage   =1024,
		RAS_MaxDnsSuffix      =256,
		UNLEN				  =256,
		PWLEN				  =256,
		DNLEN				  =15
	}


	[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
	public struct GUID
	{
		public uint	  Data1;
		public ushort   Data2;
		public ushort   Data3;
		[MarshalAs(UnmanagedType.ByValArray,SizeConst=8)]
		public byte[] Data4;
	}	

	[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
	internal struct RASCONN 
	{ 
		public int     dwSize; 
		public IntPtr  hrasconn; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasFieldSizeConstants.RAS_MaxEntryName+1)]
		public string    szEntryName; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasFieldSizeConstants.RAS_MaxDeviceType+1)]
		public string    szDeviceType; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasFieldSizeConstants.RAS_MaxDeviceName+1)]
		public string    szDeviceName; 
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]//MAX_PAPTH=260
		public string    szPhonebook;
		public int		  dwSubEntry;
		public GUID		  guidEntry;
		#if (WINVER501)
		 int     dwFlags;
		 public LUID      luid;
		#endif
	}
	
	[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
	internal struct LUID 
	{
		int LowPart;
		int HighPart;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct RasEntryName
    {
        public int dwSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
        public string szEntryName;
        public int dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
        public string szPhonebook;
    }

	[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
	public class RasStats 
	{
		public int dwSize=Marshal.SizeOf(typeof(RasStats));
		public int dwBytesXmited;
		public int dwBytesRcved;
		public int dwFramesXmited;
		public int dwFramesRcved;
		public int dwCrcErr;
		public int dwTimeoutErr;
		public int dwAlignmentErr;
		public int dwHardwareOverrunErr;
		public int dwFramingErr;
		public int dwBufferOverrunErr;
		public int dwCompressionRatioIn;
		public int dwCompressionRatioOut;
		public int dwBps;
		public int dwConnectDuration;
	} 

	
	public class RAS

	{

		[DllImport("Rasapi32.dll", EntryPoint="RasEnumConnectionsA",
			 SetLastError=true)]

		internal static extern int RasEnumConnections
			(
				ref RASCONN lprasconn, // buffer to receive connections data
				ref int lpcb, // size in bytes of buffer
				ref int lpcConnections // number of connections written to buffer
			);

		
		[DllImport("rasapi32.dll",CharSet=CharSet.Auto)]
		internal static extern uint RasGetConnectionStatistics(
			IntPtr hRasConn,       // handle to the connection
			[In,Out]RasStats lpStatistics  // buffer to receive statistics
			);
		[DllImport("rasapi32.dll",CharSet=CharSet.Auto)]
		public extern static uint RasHangUp(
			IntPtr hrasconn  // handle to the RAS connection to hang up
			);

		[DllImport("rasapi32.dll",CharSet=CharSet.Auto)]
		public extern static uint RasEnumEntries (
			IntPtr reserved,              // reserved, must be NULL
			IntPtr lpszPhonebook,         // pointer to full path and
			//  file name of phone-book file
			[In,Out]RasEntryName[] lprasentryname, // buffer to receive
			//  phone-book entries
			ref int lpcb,                  // size in bytes of buffer
			ref int lpcEntries             // number of entries written
			//  to buffer
			);

		[DllImport("wininet.dll",CharSet=CharSet.Auto)]
		public extern static int InternetDial(
			IntPtr hwnd,
			[In]string lpszConnectoid, 
			uint dwFlags,
			ref int lpdwConnection,
			uint dwReserved
			);

		public RAS()
		{	

		}

        
	}
}
