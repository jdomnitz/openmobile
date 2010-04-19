//
// File.cs: Provides tagging and properties support for MPEG-4 files.
//
// Author:
//   Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2006-2007 Brian Nickel
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

namespace TagLib.Mpeg4 {
	/// <summary>
	///    This class extends <see cref="TagLib.File" /> to provide tagging
	///    and properties support for MPEG-4 files.
	/// </summary>
	[SupportedMimeType("taglib/m4a", "m4a")]
	[SupportedMimeType("taglib/m4b", "m4b")]
	[SupportedMimeType("taglib/m4v", "m4v")]
	[SupportedMimeType("taglib/m4p", "m4p")]
	[SupportedMimeType("taglib/mp4", "mp4")]
	[SupportedMimeType("audio/mp4")]
	[SupportedMimeType("audio/x-m4a")]
	[SupportedMimeType("video/mp4")]
	[SupportedMimeType("video/x-m4v")]
	public class File : TagLib.File
	{
		#region Private Fields
		
		/// <summary>
		///    Contains the Apple tag.
		/// </summary>
		private AppleTag    apple_tag;
		
		/// <summary>
		///    Contains the combined tag.
		/// </summary>
		private CombinedTag tag;
		
		/// <summary>
		///    Contains the media properties.
		/// </summary>
		private Properties  properties;
		
		/// <summary>
		///    Contains the ISO user data box.
		/// </summary>
		private IsoUserDataBox udta_box;
		
		#endregion
		
		
		
		#region Constructors
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="File" /> for a specified path in the local file
		///    system and specified read style.
		/// </summary>
		/// <param name="path">
		///    A <see cref="string" /> object containing the path of the
		///    file to use in the new instance.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="path" /> is <see langword="null" />.
		/// </exception>
		public File (string path, ReadStyle propertiesStyle)
			: base (path)
		{
			Read (propertiesStyle);
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="File" /> for a specified path in the local file
		///    system with an average read style.
		/// </summary>
		/// <param name="path">
		///    A <see cref="string" /> object containing the path of the
		///    file to use in the new instance.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="path" /> is <see langword="null" />.
		/// </exception>
		public File (string path) : this (path, ReadStyle.Average)
		{
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="File" /> for a specified file abstraction and
		///    specified read style.
		/// </summary>
		/// <param name="abstraction">
		///    A IFileAbstraction object to use when
		///    reading from and writing to the file.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="abstraction" /> is <see langword="null"
		///    />.
		/// </exception>
		public File (File.IFileAbstraction abstraction,
		                ReadStyle propertiesStyle)
		: base (abstraction)
		{
			Read (propertiesStyle);
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="File" /> for a specified file abstraction with an
		///    average read style.
		/// </summary>
		/// <param name="abstraction">
		///    A IFileAbstraction object to use when
		///    reading from and writing to the file.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="abstraction" /> is <see langword="null"
		///    />.
		/// </exception>
		public File (File.IFileAbstraction abstraction)
			: this (abstraction, ReadStyle.Average)
		{
		}
		
		#endregion
		
		
		
		#region Public Properties
		
		/// <summary>
		///    Gets a abstract representation of all tags stored in the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="TagLib.Tag" /> object representing all tags
		///    stored in the current instance.
		/// </value>
		public override TagLib.Tag Tag {
			get {return tag;}
		}
		
		/// <summary>
		///    Gets the media properties of the file represented by the
		///    current instance.
		/// </summary>
		/// <value>
		///    A <see cref="TagLib.Properties" /> object containing the
		///    media properties of the file represented by the current
		///    instance.
		/// </value>
		public override TagLib.Properties Properties {
			get {return properties;}
		}
		
		#endregion
		
		
		
		#region Public Methods

		/// <summary>
		///    Gets a tag of a specified type from the current instance,
		///    optionally creating a new tag if possible.
		/// </summary>
		/// <param name="type">
		///    A <see cref="TagLib.TagTypes" /> value indicating the
		///    type of tag to read.
		/// </param>
		/// <param name="create">
		///    A <see cref="bool" /> value specifying whether or not to
		///    try and create the tag if one is not found.
		/// </param>
		/// <returns>
		///    A <see cref="Tag" /> object containing the tag that was
		///    found in or added to the current instance. If no
		///    matching tag was found and none was created, <see
		///    langword="null" /> is returned.
		/// </returns>
		/// <remarks>
		///    At the time of this writing, only <see cref="AppleTag" />
		///    is supported. All other tag types will be ignored.
		/// </remarks>
		public override TagLib.Tag GetTag (TagTypes type, bool create)
		{
			if (type == TagTypes.Apple) {
				if (apple_tag == null && create) {
					apple_tag = new AppleTag (udta_box);
					tag.SetTags (apple_tag);
				}
				
				return apple_tag;
			}
			
			return null;
		}
		
		#endregion
		
		
		
		#region Private Methods
		
		/// <summary>
		///    Reads the file with a specified read style.
		/// </summary>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		private void Read (ReadStyle propertiesStyle)
		{
			tag = new CombinedTag ();
			Mode = AccessMode.Read;
			try {
				FileParser parser = new FileParser (this);
				
				if (propertiesStyle == ReadStyle.None)
					parser.ParseTag ();
				else
					parser.ParseTagAndProperties ();
				
				InvariantStartPosition = parser.MdatStartPosition;
				InvariantEndPosition = parser.MdatEndPosition;
				
				udta_box = parser.UserDataBox;
				
				if (udta_box != null && udta_box.GetChild (BoxType.Meta)
					!= null && udta_box.GetChild (BoxType.Meta
					).GetChild (BoxType.Ilst) != null)
					TagTypesOnDisk |= TagTypes.Apple;
				
				if (udta_box == null)
					udta_box = new IsoUserDataBox ();
				
				apple_tag = new AppleTag (udta_box);
				tag.SetTags (apple_tag);
				
				// If we're not reading properties, we're done.
				if (propertiesStyle == ReadStyle.None) {
					Mode = AccessMode.Closed;
					return;
				}
				
				// Get the movie header box.
				IsoMovieHeaderBox mvhd_box = parser.MovieHeaderBox;
				if(mvhd_box == null) {
					Mode = AccessMode.Closed;
					throw new CorruptFileException (
						"mvhd box not found.");
				}
				
				IsoAudioSampleEntry  audio_sample_entry =
					parser.AudioSampleEntry;
				IsoVisualSampleEntry visual_sample_entry =
					parser.VisualSampleEntry;
				
				// Read the properties.
				properties = new Properties (mvhd_box.Duration,
					audio_sample_entry, visual_sample_entry);
			} finally {
				Mode = AccessMode.Closed;
			}
		}
		
		#endregion
	}
}
