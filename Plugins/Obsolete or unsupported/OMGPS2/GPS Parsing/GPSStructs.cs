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

// Sample code found here: http://channel9.msdn.com/coding4fun/articles/Where-the-Heck-am-I-Connecting-NET-20-to-a-GPS

using System;

namespace OMGPS2
{

	public enum NMEAState 
	{
		NP_STATE_SOM =				0,		// Search for start of message
		NP_STATE_CMD,						// Get command
		NP_STATE_DATA,						// Get data
		NP_STATE_CHECKSUM_1,				// Get first checksum character
		NP_STATE_CHECKSUM_2,				// get second checksum character
	};

	public struct GPGGAData
	{
		public int Hour;
		public int Minute;
		public int Second;
		public double Latitude;			
		public Cardinal LatitudeHemisphere;
		public double Longitude;		
		public Cardinal LongitudeHemisphere;
		public GPSQuality GPSQuality;			// 0 = fix not available, 1 = GPS sps mode, 2 = Differential GPS, SPS mode, fix valid, 3 = GPS PPS mode, fix valid
		public int NumberOfSatellitesInUse;
		public double HDOP;
		public double Altitude;			// Altitude: mean-sea-level (geoid) meters
		public int Count;

	}

	public enum GPSQuality : uint
	{
		FixNotAvailable = 0,
		GPS_SPSMode = 1,
		Differential_GPS_SPSMode_FixValid = 2,
		GPS_PPSMode_FixValid = 3
	}

	public enum Cardinal : uint
	{
		North = 0,
		South = 1,
		East = 2,
		West = 3,
	}

	public struct GPGSAData
	{
		public char Mode;					// M = manual, A = automaeltic 2D/3D
		public byte mFixMode;				// 1 = fix not available, 2 = 2D, 3 = 3D
		public int[] SatsInSolution; // ID of sats in solution
		public double PDOP;					//
		public double HDOP;					//
		public double VDOP;					//
		public int Count;					//	
        public DOP PDOP_Rating;
        public DOP HDOP_Rating;
        public DOP VDOP_Rating;
        public int Precision;
    }

    public enum DOP
    {
        /// <summary>
        /// Dop is ideal (Dop 1)
        /// </summary>
        Ideal,
        /// <summary>
        /// Dop is excellent (Dop 2-3)
        /// </summary>
        Excellent,
        /// <summary>
        /// Dop is good (Dop 4-6)
        /// </summary>
        Good,
        /// <summary>
        /// Dop is moderate (Dop 7-8)
        /// </summary>
        Moderate,
        /// <summary>
        /// Dop is fair (Dop 9-20)
        /// </summary>
        Fair,
        /// <summary>
        /// Dop is poor (Dop 21-50)
        /// </summary>
        Poor
    }


	public class GPGSVData
	{
		public uint TotalNumberOfMessages;			//
		public int SatellitesInView;		//
		public Satellites Satellites = new Satellites();			//
		public int Count;					//
	}

	public struct GPRMBData
	{
		public byte DataStatus;				// A = data valid, V = navigation receiver warning
		public double CrosstrackError;		// nautical miles
		public byte DirectionToSteer;		// L/R
		public string OriginWaypoint; // Origin Waypoint ID
		public string DestWaypoint; // Destination waypoint ID
		public double DestLatitude;			// destination waypoint latitude
		public double DestLongitude;			// destination waypoint longitude
		public double RangeToDest;			// Range to destination nautical mi
		public double BearingToDest;			// Bearing to destination, degrees true
		public double DestClosingVelocity;	// Destination closing velocity, knots
		public byte ArrivalStatus;			// A = arrival circle entered, V = not entered
		public int Count;					//
	}


	public struct GPRMCData
	{
		public int Hour;					//
		public int Minute;					//
		public int Second;					//
		public char DataValid;				// A = Data valid, V = navigation rx warning
		public double Latitude;				// current latitude
        public Cardinal LatitudeHemisphere;
        public double Longitude;				// current longitude
        public Cardinal LongitudeHemisphere;
        /// <summary>
        /// Ground speed in knots (to get mph, *1.15, to get kmh, *1.852)
        /// </summary>
        public double GroundSpeed;			// speed over ground, knots
		public double Course;				// course over ground, degrees true
		public int Day;					//
		public int Month;					//
		public int Year;					//
		public double MagVar;				// magnitic variation, degrees East(+)/West(-)
		public int Count;					//	
	
	
	}


	public struct GPZDAData
	{
		public byte Hour;					//
		public byte Minute;					//
		public byte Second;					//
		public byte Day;					// 1 - 31
		public byte Month;					// 1 - 12
		public int Year;					//
		public byte LocalZoneHour;			// 0 to +/- 13
		public byte LocalZoneMinute;		// 0 - 59
		public int Count;					//		
	}
}
