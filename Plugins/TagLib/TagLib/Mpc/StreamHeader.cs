//
// StreamHeader.cs: Provides support for reading MusePack audio properties.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   mpcproperties.cpp from TagLib
//
// Copyright (C) 2006-2007 Brian Nickel
// Copyright (C) 2004 by Allan Sandfeld Jensen (Original Implementation)
//
// This library is free software; you can redistribute it and/or modify
// it  under the terms of the GNU Lesser General Public License version
// 2.1 as published by the Free Software Foundation.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
// USA
//

using System;

namespace TagLib.MusePack {
	/// <summary>
	///    This struct implements <see cref="IAudioCodec" /> to provide
	///    support for reading MusePack audio properties.
	/// </summary>
	public class StreamHeader : IAudioCodec
	{
		#region Constants
		
		private static ushort [] sftable = {44100, 48000, 37800, 32000};
		
		#endregion
		
		
		
		#region Private Fields
		
		/// <summary>
        ///    Contains the number of bytes in the stream (only Stream Version 7 or below).
		/// </summary>
		private long stream_length;
		
		/// <summary>
		///    Contains the MusePack version.
		/// </summary>
		private int version;
		
		/// <summary>
        ///    Contains additional header information (only Stream Version 6 or below).
		/// </summary>
		private uint header_data;
		
		/// <summary>
		///    Contains the sample rate of the stream.
		/// </summary>
		private int sample_rate;
		
		/// <summary>
        ///    Contains the number of frames in the stream (only Stream Version 7).
		/// </summary>
		private uint frames;
		
        /// <summary>
		///    Contains the number of samples in the stream (only Stream Version 8).
		/// </summary>
		private ulong sample_count;

		/// <summary>
		///    Contains the number of channels in the stream (only Stream Version 8).
		/// </summary>
		private byte num_chans;
		#endregion
		
		
		
		#region Public Static Fields
		
		/// <summary>
		///    The size of a MusePack header.
		/// </summary>
		public const uint Size = 68;
		
		/// <summary>
        ///    The identifier used to recognize a WavPack file (Stream Version 7 and below).
		/// </summary>
		/// <value>
		///    "MP+"
		/// </value>
		public static readonly ReadOnlyByteVector FileIdentifier7 = "MP+";
		
        /// <summary>
		///    The identifier used to recognize a WavPack file (Stream Version 8).
		/// </summary>
		/// <value>
		///    "MPCK"
		/// </value>
		public static readonly ReadOnlyByteVector FileIdentifier8 = "MPCK";
		#endregion
		
		
		
		#region Constructors
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="StreamHeader" /> for a specified header block and
		///    stream length.
		/// </summary>
		/// <param name="data">
		///    A ByteVector object containing the stream
		///    header data.
		/// </param>
		/// <param name="streamLength">
		///    A <see cref="long" /> value containing the length of the
		///    MusePAck stream in bytes.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="data" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="CorruptFileException">
		///    <paramref name="data" /> does not begin with <see
		///    cref="FileIdentifier" /> or is less than <see cref="Size"
		///    /> bytes long.
		/// </exception>
		public StreamHeader (ByteVector data, long streamLength)
		{
            if (data == null)
				throw new ArgumentNullException ("data");

			if (data.Count < Size)
				throw new CorruptFileException(
					"Insufficient data in stream header");

			stream_length = streamLength;

			if (data.StartsWith(FileIdentifier7))
			{
				// handling of Stream Version 7 or below
				version = data[3] & 15;
				if (version >= 7)
				{
					frames = data.Mid(4, 4).ToUInt(false);
					uint flags = data.Mid(8, 4).ToUInt(false);
					sample_rate = sftable[(int)(((flags >> 17) &
						1) * 2 + ((flags >> 16) & 1))];
					header_data = 0;
				}
				else
				{
					header_data = data.Mid(0, 4).ToUInt(false);
					version = (int)((header_data >> 11) & 0x03ff);
					sample_rate = 44100;
					frames = data.Mid(4,
						version >= 5 ? 4 : 2).ToUInt(false);
				}
			}
			else if (data.StartsWith(FileIdentifier8))
			{
				// handling of Stream Version 8
				int offset = FindPacket(data, 4, "SH");
				offset += 4; // first 4 bytes are CRC
				version = data[offset];
				offset += 1; // version is 1 byte
				int len;
				sample_count = ReadSize(data, offset, out len);
				offset += len;
				ulong dummy = ReadSize(data, offset, out len);
				offset += len;
				ushort flags = data.Mid(offset, 2).ToUShort(false);
				sample_rate = sftable[(flags >> 13) & 7];
				num_chans = (byte)(((flags >> 4) & 15) + 1);
				frames = 0; // 11011,0001,1,011
				header_data = 0;
			}
			else
				throw new CorruptFileException(
					"Data does not begin with identifier.");
		}
		
		#endregion
		
		#region Private Methods

		/// <summary>
		/// Returns the offset position of a packet in a Stream Version 8.
		/// </summary>
		/// <param name="data">The byte vector to scan.</param>
		/// <param name="offset">The offset position where to start the scan.</param>
		/// <param name="packetKey">The packet key to search for.</param>
		/// <returns>The offset position of the packet data (payload) in the byte vector (not including the key and size field).</returns>
		private int FindPacket(ByteVector data, int offset, ByteVector packetKey)
		{
			int len;
			while (data.Count > offset)
			{
				if (!data.Mid(offset, 2).Equals(packetKey))
				{
					ulong size = ReadSize(data, offset + 2, out len);
					offset += (int)size;
				}
				else
				{
					ulong size = ReadSize(data, offset + 2, out len);
					offset += len + packetKey.Count;
					break;
				}
			}

			return offset;
		}
		/// <summary>
		/// Reads a variable size value in big-endian notation.
		/// </summary>
		/// <param name="data">The byte vector containing the data.</param>
		/// <param name="offset">The offset position in the byte vector where to start reading the size.</param>
		/// <param name="len">Returns the number of bytes read which represent the size.</param>
		/// <returns>The size value.</returns>
		/// <remarks>
		/// In Stream Version 8 a site field is defined as followed:
		/// <code>
		/// Size is a variable-size field:
		/// bits, big-endian
		/// 0xxx xxxx                                           - value 0 to  2^7-1
		/// 1xxx xxxx  0xxx xxxx                                - value 0 to 2^14-1
		/// 1xxx xxxx  1xxx xxxx  0xxx xxxx                     - value 0 to 2^21-1
		/// 1xxx xxxx  1xxx xxxx  1xxx xxxx  0xxx xxxx          - value 0 to 2^28-1
		/// ...
		/// 
		/// </code>
		/// </remarks>
		private ulong ReadSize(ByteVector data, int offset, out int len)
		{
			ulong size = 0L;

			len = 0;
			while (true)
			{
				byte b = data[offset + len];
				size = (size << 7) + (ulong)(b & 0x7F);
				len++;
				if (b < 0x80)
					break;
			}

			return size;
		}
 		#endregion
		
		#region Public Properties
		
		/// <summary>
		///    Gets the duration of the media represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    A <see cref="TimeSpan" /> containing the duration of the
		///    media represented by the current instance.
		/// </value>
		
		public TimeSpan Duration {
			get {
 				if (sample_rate <= 0 && stream_length <= 0)
					return TimeSpan.Zero;

				if (version >= 8)
				{
					return TimeSpan.FromSeconds(
						(double)sample_count /
						(double)sample_rate);
				}
				else
				{
					return TimeSpan.FromSeconds(
						(double)(frames * 1152 - 576) /
						(double)sample_rate + 0.5);
				}
 			}
		}
		
		/// <summary>
		///    Gets the types of media represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    Always MediaTypes.Audio.
		/// </value>
		public MediaTypes MediaTypes {
			get {return MediaTypes.Audio;}
		}
		
		/// <summary>
		///    Gets a text description of the media represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="string" /> object containing a description
		///    of the media represented by the current instance.
		/// </value>
		public string Description {
			get {return string.Format (
				System.Globalization.CultureInfo.InvariantCulture,
				"MusePack Version {0} Audio", Version);}
		}
		
		/// <summary>
		///    Gets the bitrate of the audio represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing a bitrate of the
		///    audio represented by the current instance.
		/// </value>
		public int AudioBitrate {
			get {
 				if (header_data != 0)
					return (int) ((header_data >> 23) & 0x01ff);

				if (version >= 8)
					return (sample_rate * num_chans * 16) / 1000;
				else
					return (int)(Duration > TimeSpan.Zero ?
						((stream_length * 8L) /
						Duration.TotalSeconds) / 1000 : 0);
 			}
		}
		
		/// <summary>
		///    Gets the sample rate of the audio represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing the sample rate of
		///    the audio represented by the current instance.
		/// </value>
		public int AudioSampleRate {
			get {return sample_rate;}
		}
		
		/// <summary>
		///    Gets the number of channels in the audio represented by
		///    the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing the number of
		///    channels in the audio represented by the current
		///    instance.
		/// </value>
		public int AudioChannels {
			get {
				if (version >= 8)
					return num_chans;
				else
					return 2;
            }
		}
		
		/// <summary>
		///    Gets the WavPack version of the audio represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing the WavPack version
		///    of the audio represented by the current instance.
		/// </value>
		public int Version {
			get {return version;}
		}
		
		#endregion
		
		
		
		#region IEquatable
		
		/// <summary>
		///    Generates a hash code for the current instance.
		/// </summary>
		/// <returns>
		///    A <see cref="int" /> value containing the hash code for
		///    the current instance.
		/// </returns>
		public override int GetHashCode ()
		{
			unchecked {
				return (int) (header_data ^ sample_rate ^
					frames ^ version);
			}
		}
		
		/// <summary>
		///    Checks whether or not the current instance is equal to
		///    another object.
		/// </summary>
		/// <param name="other">
		///    A <see cref="object" /> to compare to the current
		///    instance.
		/// </param>
		/// <returns>
		///    A <see cref="bool" /> value indicating whether or not the
		///    current instance is equal to <paramref name="other" />.
		/// </returns>
		/// <seealso cref="M:System.IEquatable`1.Equals" />
		public override bool Equals (object other)
		{
			if (!(other is StreamHeader))
				return false;
			
			return Equals ((StreamHeader) other);
		}
		
		/// <summary>
		///    Checks whether or not the current instance is equal to
		///    another instance of <see cref="StreamHeader" />.
		/// </summary>
		/// <param name="other">
		///    A <see cref="StreamHeader" /> object to compare to the
		///    current instance.
		/// </param>
		/// <returns>
		///    A <see cref="bool" /> value indicating whether or not the
		///    current instance is equal to <paramref name="other" />.
		/// </returns>
		/// <seealso cref="M:System.IEquatable`1.Equals" />
		public bool Equals (StreamHeader other)
		{
			return header_data == other.header_data &&
				sample_rate == other.sample_rate &&
				version == other.version &&
				frames == other.frames;
		}
		
		/// <summary>
		///    Gets whether or not two instances of <see
		///    cref="StreamHeader" /> are equal to eachother.
		/// </summary>
		/// <param name="first">
		///    A <see cref="StreamHeader" /> object to compare.
		/// </param>
		/// <param name="second">
		///    A <see cref="StreamHeader" /> object to compare.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if <paramref name="first" /> is
		///    equal to <paramref name="second" />. Otherwise, <see
		///    langword="false" />.
		/// </returns>
		public static bool operator == (StreamHeader first,
		                                StreamHeader second)
		{
			return first.Equals (second);
		}
		
		/// <summary>
		///    Gets whether or not two instances of <see
		///    cref="StreamHeader" /> differ.
		/// </summary>
		/// <param name="first">
		///    A <see cref="StreamHeader" /> object to compare.
		/// </param>
		/// <param name="second">
		///    A <see cref="StreamHeader" /> object to compare.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if <paramref name="first" /> is
		///    unequal to <paramref name="second" />. Otherwise, <see
		///    langword="false" />.
		/// </returns>
		public static bool operator != (StreamHeader first,
		                                StreamHeader second)
		{
			return !first.Equals (second);
		}
		
		#endregion
	}
}
