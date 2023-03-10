//
// PageHeader.cs:
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Original Source:
//   oggpage.cpp from TagLib
//
// Copyright (C) 2005-2007 Brian Nickel
// Copyright (C) 2003 Scott Wheeler (Original Implementation)
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

using System.Collections.Generic;
using System;

namespace TagLib.Ogg
{
	/// <summary>
	///    This class provides a representation of an Ogg page.
	/// </summary>
	public class Page
	{
#region Private Properties
		
		/// <summary>
		///    Contains the page header.
		/// </summary>
		private PageHeader header;
		
		/// <summary>
		///    Contains the packets.
		/// </summary>
		private ByteVectorCollection packets;
		
#endregion
		
		
		
#region Constructors
		
		/// <summary>
		///    Constructs and intializes a new instance of <see
		///    cref="Page" /> with a specified header and no packets.
		/// </summary>
		/// <param name="header">
		///    A <see cref="PageHeader"/> object to use as the header of
		///    the new instance.
		/// </param>
		protected Page (PageHeader header)
		{
			this.header = header;
			packets = new ByteVectorCollection ();
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="Page" /> by reading a raw Ogg page from a specified
		///    position in a specified file.
		/// </summary>
		/// <param name="file">
		///    A <see cref="File" /> object containing the file from
		///    which the contents of the new instance are to be read.
		/// </param>
		/// <param name="position">
		///    A <see cref="long" /> value specify at what position to
		///    read.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="file" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///    <paramref name="position" /> is less than zero or greater
		///    than the size of the file.
		/// </exception>
		/// <exception cref="CorruptFileException">
		///    The Ogg identifier could not be found at the correct
		///    location.
		/// </exception>
		public Page (File file, long position)
			: this (new PageHeader (file, position))
		{
			file.Seek (position + header.Size);
			
			foreach (int packet_size in header.PacketSizes)
				packets.Add (file.ReadBlock (packet_size));
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="Page" /> with a specified header and packets.
		/// </summary>
		/// <param name="packets">
		///    A <see cref="ByteVectorCollection" /> object containing
		///    packets to use for the new instance.
		/// </param>
		/// <param name="header">
		///    A <see cref="PageHeader"/> object to use as the header of
		///    the new instance.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="packets" /> is <see langword="null" />.
		/// </exception>
		public Page (ByteVectorCollection packets, PageHeader header)
			: this (header)
		{
			if (packets == null)
				throw new ArgumentNullException ("packets");
			
			this.packets = new ByteVectorCollection (packets);
			
			List<int> packet_sizes = new List<int> ();
			
			// Build a page from the list of packets.
			foreach (ByteVector v in packets)
				packet_sizes.Add (v.Count);
			
			header.PacketSizes = packet_sizes.ToArray ();
		}
		
#endregion
		
		
		
#region Public Methods
		
		/// <summary>
		///    Renders the current instance as a raw Ogg page.
		/// </summary>
		/// <returns>
		///    A ByteVector object containing the
		///    rendered version of the current instance.
		/// </returns>
		public ByteVector Render ()
		{
			ByteVector data = header.Render ();
			
			foreach (ByteVector v in packets)
				data.Add (v);
			
			// Compute and set the checksum for the Ogg page. The
			// checksum is taken over the entire page with the 4
			// bytes reserved for the checksum zeroed and then
			// inserted in bytes 22-25 of the page header.
			
			ByteVector checksum = ByteVector.FromUInt (
				data.Checksum, false);
			
			for (int i = 0; i < 4; i++)
				data [i + 22] = checksum [i];
			
			return data;
		}
		
#endregion
		
		
		
#region Public Properties
		
		/// <summary>
		///    Gets the header of the current instance.
		/// </summary>
		/// <value>
		///    A <see cref="PageHeader" /> object that applies to the
		///    current instance.
		/// </value>
		public PageHeader Header {
			get {return header;}
		}
		
		/// <summary>
		///    Gets the packets contained in the current instance.
		/// </summary>
		/// <value>
		///    A ByteVector[] containing the packets
		///    contained in the current instance.
		/// </value>
		public ByteVector[] Packets {
			get {return packets.ToArray ();}
		}
		
		/// <summary>
		///    Gets the total size of the current instance as it
		///    appeared on disk.
		/// </summary>
		/// <value>
		///    A <see cref="uint" /> value containing the size of the
		///    page, including the header, as it appeared on disk.
		/// </value>
		public uint Size {
			get {return header.Size + header.DataSize;}
		}
		
#endregion
	}
}
