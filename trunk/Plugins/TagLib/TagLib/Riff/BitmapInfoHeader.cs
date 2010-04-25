//
// BitmapInfoHeader.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2007 Brian Nickel
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
using System.Globalization;

namespace TagLib.Riff
{
	/// <summary>
	///    This structure provides a representation of a Microsoft
	///    BitmapInfoHeader structure.
	/// </summary>
	public struct BitmapInfoHeader : IVideoCodec
	{
#region Private Fields
		
		/// <summary>
		///    Contains the size of the header.
		/// </summary>
		uint size; 
		
		/// <summary>
		///    Contains the video width.
		/// </summary>
		uint width; 
		
		/// <summary>
		///    Contains the video height.
		/// </summary>
		uint height; 
		
		/// <summary>
		///    Contains the number of planes.
		/// </summary>
		ushort planes; 
		
		/// <summary>
		///    Contains the bit count.
		/// </summary>
		ushort bit_count; 
		
		/// <summary>
		///    Contains the compression (codec) ID.
		/// </summary>
		ByteVector compression_id; 
		
		/// <summary>
		///    Contains the size of the image.
		/// </summary>
		uint size_of_image; 
		
		/// <summary>
		///    Contains the number of X pixels per meter.
		/// </summary>
		uint x_pixels_per_meter; 
		
		/// <summary>
		///    Contains the number of Y pixels per meter.
		/// </summary>
		uint y_pixels_per_meter; 
		
		/// <summary>
		///    Contains the number of colors used.
		/// </summary>
		uint colors_used; 
		
		/// <summary>
		///    Contains the number of important colors.
		/// </summary>
		uint colors_important;
		
#endregion
		
		
		
#region Constructors
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="BitmapInfoHeader" /> by reading the raw structure
		///    from a specified position in a ByteVector
		///    object.
		/// </summary>
		/// <param name="data">
		///    A ByteVector object containing the raw
		///    data structure.
		/// </param>
		/// <param name="offset">
		///    A <see cref="int" /> value specifying the index in
		///    <paramref name="data"/> at which the structure begins.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="data" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///    <paramref name="offset" /> is less than zero.
		/// </exception>
		/// <exception cref="CorruptFileException">
		///    <paramref name="data" /> contains less than 16 bytes at
		///    <paramref name="offset" />.
		/// </exception>
		public BitmapInfoHeader (ByteVector data, int offset)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			
			if (offset + 40 > data.Count)
				throw new CorruptFileException (
					"Expected 40 bytes.");
			
			if (offset < 0)
				throw new ArgumentOutOfRangeException (
					"offset");
			
			size               = data.Mid (offset +  0, 4).ToUInt (false);
			width              = data.Mid (offset +  4, 4).ToUInt (false);
			height             = data.Mid (offset +  8, 4).ToUInt (false);
			planes             = data.Mid (offset + 12, 2).ToUShort (false);
			bit_count          = data.Mid (offset + 14, 2).ToUShort (false);
			compression_id     = data.Mid (offset + 16, 4);
			size_of_image      = data.Mid (offset + 20, 4).ToUInt (false);
			x_pixels_per_meter = data.Mid (offset + 24, 4).ToUInt (false);
			y_pixels_per_meter = data.Mid (offset + 28, 4).ToUInt (false);
			colors_used        = data.Mid (offset + 32, 4).ToUInt (false);
			colors_important   = data.Mid (offset + 36, 4).ToUInt (false);
		}
		
#endregion
		
		
		
#region Public Properties
		
		/// <summary>
		///    Gets the size of the structure in bytes.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    bytes in the structure.
		/// </value>
		public uint HeaderSize {
			get {return size;}
		}
		
		/// <summary>
		///    Gets the number of planes in the image.
		/// </summary>
		/// <value>
		///    A <see cref="ushort" /> value containing the number of
		///    planes.
		/// </value>
		public ushort Planes {
			get {return planes;}
		}
		
		/// <summary>
		///    Gets the number of bits per pixel.
		/// </summary>
		/// <value>
		///    A <see cref="ushort" /> value containing the number of
		///    bits per pixel, equivalent to the log base 2 of the
		///    maximum number of colors.
		/// </value>
		public ushort BitCount {
			get {return bit_count;}
		}
		
		/// <summary>
		///    Gets the compression ID for image.
		/// </summary>
		/// <value>
		///    A four-byte ByteVector object containing
		///    the ID of the compression system (codec) used by the
		///    image.
		/// </value>
		public ByteVector CompressionId {
			get {return compression_id;}
		}
		
		/// <summary>
		///    Gets the size of the image in bytes.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    bytes in the image.
		/// </value>
		public uint ImageSize {
			get {return size_of_image;}
		}
		
		/// <summary>
		///    Gets the horizontal resolution of the target device.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    pixels-per-meter in the hoizontal direction for the
		///    target device.
		/// </value>
		public uint XPixelsPerMeter {
			get {return x_pixels_per_meter;}
		}
		
		/// <summary>
		///    Gets the vertical resolution of the target device.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    pixels-per-meter in the vertical direction for the
		///    target device.
		/// </value>
		public uint YPixelsPerMeter {
			get {return y_pixels_per_meter;}
		}
		
		/// <summary>
		///    Gets the number of colors in the image.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    colors.
		/// </value>
		public uint ColorsUsed {
			get {return colors_used;}
		}
		
		/// <summary>
		///    Gets the number of colors important in displaying the
		///    image.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the number of
		///    important colors.
		/// </value>
		public uint ImportantColors {
			get {return colors_important;}
		}
		
#endregion
		
		
		
#region IVideoCodec
		
		/// <summary>
		///    Gets the width of the video represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing the width of the
		///    video represented by the current instance.
		/// </value>
		public int VideoWidth  {
			get {return (int)width;}
		}
		
		/// <summary>
		///    Gets the height of the video represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    A <see cref="int" /> value containing the height of the
		///    video represented by the current instance.
		/// </value>
		public int VideoHeight {
			get {return (int)height;}
		}
		
		/// <summary>
		///    Gets the types of media represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    Always MediaTypes.Video
		/// </value>
		public MediaTypes MediaTypes {
			get {return MediaTypes.Video;}
		}
		
		/// <summary>
		///    Gets the duration of the media represented by the current
		///    instance.
		/// </summary>
		/// <value>
		///    Always <see cref="TimeSpan.Zero" />.
		/// </value>
		public TimeSpan Duration {
			get {return TimeSpan.Zero;}
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
			get
            {
                return ""; //Was Bloat
			}
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
				return (int) (size ^ width ^ height ^ planes ^
					bit_count ^ compression_id.ToUInt () ^
					size_of_image ^ x_pixels_per_meter ^
					y_pixels_per_meter ^ colors_used ^
					colors_important);
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
			if (!(other is BitmapInfoHeader))
				return false;
			
			return Equals ((BitmapInfoHeader) other);
		}
		
		/// <summary>
		///    Checks whether or not the current instance is equal to
		///    another instance of <see cref="BitmapInfoHeader" />.
		/// </summary>
		/// <param name="other">
		///    A <see cref="BitmapInfoHeader" /> object to compare to
		///    the current instance.
		/// </param>
		/// <returns>
		///    A <see cref="bool" /> value indicating whether or not the
		///    current instance is equal to <paramref name="other" />.
		/// </returns>
		/// <seealso cref="M:System.IEquatable`1.Equals" />
		public bool Equals (BitmapInfoHeader other)
		{
			return size == other.size && width == other.width &&
				height == other.height && planes == other.planes &&
				bit_count == other.bit_count &&
				compression_id == other.compression_id &&
				size_of_image == other.size_of_image &&
				x_pixels_per_meter == other.x_pixels_per_meter &&
				y_pixels_per_meter == other.y_pixels_per_meter &&
				colors_used == other.colors_used &&
				colors_important == other.colors_important;
		}
		
		/// <summary>
		///    Gets whether or not two instances of <see
		///    cref="WaveFormatEx" /> are equal to eachother.
		/// </summary>
		/// <param name="first">
		///    A <see cref="BitmapInfoHeader" /> object to compare.
		/// </param>
		/// <param name="second">
		///    A <see cref="BitmapInfoHeader" /> object to compare.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if <paramref name="first" /> is
		///    equal to <paramref name="second" />. Otherwise, <see
		///    langword="false" />.
		/// </returns>
		public static bool operator == (BitmapInfoHeader first,
		                                BitmapInfoHeader second)
		{
			return first.Equals (second);
		}
		
		/// <summary>
		///    Gets whether or not two instances of <see
		///    cref="BitmapInfoHeader" /> differ.
		/// </summary>
		/// <param name="first">
		///    A <see cref="BitmapInfoHeader" /> object to compare.
		/// </param>
		/// <param name="second">
		///    A <see cref="BitmapInfoHeader" /> object to compare.
		/// </param>
		/// <returns>
		///    <see langword="true" /> if <paramref name="first" /> is
		///    unequal to <paramref name="second" />. Otherwise, <see
		///    langword="false" />.
		/// </returns>
		public static bool operator != (BitmapInfoHeader first,
		                                BitmapInfoHeader second)
		{
			return !first.Equals (second);
		}
#endregion
	}
}